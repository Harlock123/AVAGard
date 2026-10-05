using System.Text.Json;
using System.Text.Json.Serialization;
using Telengard.Core.Engine;
using Telengard.Infrastructure.Storage;

namespace Telengard.Infrastructure.Settings;

/// <summary>Per-user preferences. Rule-affecting options are stored with each save instead.</summary>
public sealed class GameSettings
{
    public Preset DefaultPreset { get; set; } = Preset.Modern;
    /// <summary>Modern input deadline in seconds; null = deliberate mode (no deadline).</summary>
    public double? ModernDeadlineSeconds { get; set; } = 10;
    public bool ManualDeathRule { get; set; }
    /// <summary>Multiplier for message pauses (1 = the original's ~1.5 s beats).</summary>
    public double ClassicPacing { get; set; } = 1.0;
    public double ModernPacing { get; set; } = 0.35;
    public double UiScale { get; set; } = 1.0;
    public bool HighContrast { get; set; }
    public bool ReducedMotion { get; set; }
    public bool SoundEnabled { get; set; } = true;
    public double SoundVolume { get; set; } = 0.6;
    public bool ShowAutomap { get; set; } = true;
    public bool ShowTutorial { get; set; } = true;
    /// <summary>Optional remapping: command name → key names (Avalonia Key enum names).</summary>
    public Dictionary<string, string[]> KeyBindings { get; set; } = [];

    public GameSettings Clone()
    {
        var c = (GameSettings)MemberwiseClone();
        c.KeyBindings = KeyBindings.ToDictionary(kv => kv.Key, kv => (string[])kv.Value.Clone());
        return c;
    }
}

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(GameSettings))]
internal sealed partial class SettingsJsonContext : JsonSerializerContext;

public sealed class SettingsStore(string path)
{
    public string Path { get; } = path;

    public static SettingsStore ForCurrentUser() => new(UserDataPaths.ForCurrentUser().SettingsFile);

    /// <summary>Missing or unreadable settings fall back to defaults (and report why).</summary>
    public GameSettings Load(out string? problem)
    {
        problem = null;
        try
        {
            if (!File.Exists(Path)) return new GameSettings();
            return JsonSerializer.Deserialize(File.ReadAllText(Path), SettingsJsonContext.Default.GameSettings) ?? new GameSettings();
        }
        catch (Exception e) when (e is JsonException or IOException or UnauthorizedAccessException)
        {
            problem = "Settings could not be read and were reset to defaults: " + e.Message;
            return new GameSettings();
        }
    }

    public Task SaveAsync(GameSettings settings, CancellationToken ct = default) =>
        AtomicFile.WriteAllTextAsync(Path, JsonSerializer.Serialize(settings, SettingsJsonContext.Default.GameSettings), ct);
}
