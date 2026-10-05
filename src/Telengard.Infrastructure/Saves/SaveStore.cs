using System.Security.Cryptography;
using System.Text.Json;
using Telengard.Core.Dungeon;
using Telengard.Core.Engine;
using Telengard.Core.Model;
using Telengard.Infrastructure.Storage;

namespace Telengard.Infrastructure.Saves;

public sealed record SaveSummary(
    string CharacterName, SaveKind Kind, Preset Preset, DateTimeOffset SavedAt, int Level, int Depth, string Path, string? Problem)
{
    public bool IsUsable => Problem is null;
}

public sealed record LoadedSave(SaveFile File, GameState State);

public sealed class SaveLoadException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>
/// One manual save per character name (like the original's NAME.PLR) plus, in Modern, one
/// suspend snapshot per character. All writes are atomic.
/// </summary>
public sealed class SaveStore(string directory)
{
    private const string ManualExtension = ".tgsave";
    private const string SuspendExtension = ".tgsuspend";

    public string Directory { get; } = directory;

    public static SaveStore ForCurrentUser() => new(UserDataPaths.ForCurrentUser().SaveDirectory);

    public string PathFor(string name, SaveKind kind) =>
        Path.Combine(Directory, CharacterCreation.NormalizeName(name) + (kind == SaveKind.Manual ? ManualExtension : SuspendExtension));

    public async Task SaveAsync(GameState state, SaveKind kind, RulesPolicy policy, CancellationToken ct = default)
    {
        if (state.Character.IsDemo) throw new InvalidOperationException("The demonstration character cannot be saved.");
        var file = Wrap(state, kind, policy);
        string json = JsonSerializer.Serialize(file, SaveJsonContext.Default.SaveFile);
        await AtomicFile.WriteAllTextAsync(PathFor(state.Character.Name, kind), json, ct);
    }

    public static SaveFile Wrap(GameState state, SaveKind kind, RulesPolicy policy)
    {
        var element = JsonSerializer.SerializeToElement(state, SaveJsonContext.Default.GameState);
        return new SaveFile
        {
            Format = SaveFile.FormatId,
            SchemaVersion = SaveFile.CurrentSchemaVersion,
            Content = SaveFile.RulesContent,
            Kind = kind,
            Preset = policy.Preset,
            DeadlineSeconds = policy.InputDeadline?.TotalSeconds,
            ManualDeathRule = policy.ManualDeathRule,
            SavedAt = DateTimeOffset.UtcNow,
            AppVersion = typeof(SaveStore).Assembly.GetName().Version?.ToString() ?? "",
            State = element,
            Checksum = Checksum(element),
        };
    }

    public async Task<LoadedSave> LoadAsync(string path, CancellationToken ct = default)
    {
        string json;
        try { json = await File.ReadAllTextAsync(path, ct); }
        catch (FileNotFoundException e) { throw new SaveLoadException("The save file no longer exists.", e); }
        catch (IOException e) { throw new SaveLoadException("The save file could not be read: " + e.Message, e); }
        catch (UnauthorizedAccessException e) { throw new SaveLoadException("Permission denied reading the save file.", e); }
        return Parse(json);
    }

    public static LoadedSave Parse(string json)
    {
        SaveFile? file;
        try { file = JsonSerializer.Deserialize(json, SaveJsonContext.Default.SaveFile); }
        catch (JsonException e) { throw new SaveLoadException("The save file is damaged (not valid save data).", e); }
        if (file is null || file.Format != SaveFile.FormatId)
            throw new SaveLoadException("This is not a Telengard save file.");
        if (file.SchemaVersion > SaveFile.CurrentSchemaVersion)
            throw new SaveLoadException($"This save was made by a newer version (format {file.SchemaVersion}). Please update the game.");
        if (file.SchemaVersion < 1)
            throw new SaveLoadException($"Unsupported save format version {file.SchemaVersion}.");
        if (file.Content != SaveFile.RulesContent)
            throw new SaveLoadException($"This save uses an unknown rules set ({file.Content}).");
        if (file.State.ValueKind != JsonValueKind.Object || Checksum(file.State) != file.Checksum)
            throw new SaveLoadException("The save file is damaged (checksum mismatch).");

        GameState? state;
        try { state = file.State.Deserialize(SaveJsonContext.Default.GameState); }
        catch (JsonException e) { throw new SaveLoadException("The save file is damaged (bad character data).", e); }
        if (state is null) throw new SaveLoadException("The save file is empty.");
        Validate(state);
        return new LoadedSave(file, state);
    }

    private static void Validate(GameState s)
    {
        var c = s.Character;
        string? problem =
            c.Stats.Length != 6 ? "attributes" :
            c.Items.Length != 11 ? "items" :
            c.Timers.Length != 12 ? "effects" :
            c.BoxCombination.Length != 5 ? "box combination" :
            s.Followers.Length != GameState.QueueLength ? "followers" :
            s.X is < 1 or > DungeonMap.Size || s.Y is < 1 or > DungeonMap.Size || s.Z is < 0 or > DungeonMap.Depth ? "position" :
            c.Level < 1 ? "level" :
            c.Hp < 1 || c.MaxHp < 1 ? "hit points" :
            string.IsNullOrEmpty(c.Name) || c.IsDemo ? "name" :
            null;
        if (problem is not null) throw new SaveLoadException($"The save file is damaged (invalid {problem}).");
    }

    /// <summary>Lists saves, newest first. Unreadable files are listed with a problem description.</summary>
    public IReadOnlyList<SaveSummary> List()
    {
        if (!System.IO.Directory.Exists(Directory)) return [];
        var result = new List<SaveSummary>();
        foreach (var path in System.IO.Directory.EnumerateFiles(Directory))
        {
            var ext = Path.GetExtension(path);
            if (ext != ManualExtension && ext != SuspendExtension) continue;
            var kind = ext == ManualExtension ? SaveKind.Manual : SaveKind.Suspend;
            var name = Path.GetFileNameWithoutExtension(path);
            try
            {
                var loaded = Parse(File.ReadAllText(path));
                result.Add(new SaveSummary(loaded.State.Character.Name, kind, loaded.File.Preset, loaded.File.SavedAt,
                    loaded.State.Character.Level, loaded.State.Z, path, null));
            }
            catch (Exception e) when (e is SaveLoadException or IOException or UnauthorizedAccessException)
            {
                result.Add(new SaveSummary(name, kind, Preset.Classic, File.GetLastWriteTimeUtc(path), 0, 0, path, e.Message));
            }
        }
        return result.OrderByDescending(s => s.SavedAt).ToList();
    }

    public void Delete(string name, SaveKind kind)
    {
        var path = PathFor(name, kind);
        if (File.Exists(path)) File.Delete(path);
    }

    /// <summary>
    /// Applies the death rule. V4.18 leaves the stored character untouched (default). The manual's
    /// rule deletes it unless the name begins with "SV". A suspend snapshot is always removed so a
    /// dead character cannot be resumed from it.
    /// </summary>
    public void OnDeath(string name, RulesPolicy policy)
    {
        Delete(name, SaveKind.Suspend);
        var normalized = CharacterCreation.NormalizeName(name);
        if (policy.ManualDeathRule && !normalized.StartsWith("SV", StringComparison.Ordinal))
            Delete(name, SaveKind.Manual);
    }

    /// <summary>Hash of the state re-serialized compactly, so file indentation does not matter.</summary>
    private static string Checksum(JsonElement element)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer)) element.WriteTo(writer);
        return Convert.ToHexStringLower(SHA256.HashData(buffer.ToArray()));
    }
}
