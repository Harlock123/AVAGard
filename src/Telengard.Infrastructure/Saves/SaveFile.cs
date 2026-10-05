using System.Text.Json;
using System.Text.Json.Serialization;
using Telengard.Core.Engine;
using Telengard.Core.Model;

namespace Telengard.Infrastructure.Saves;

public enum SaveKind
{
    /// <summary>Written by the player's Store command (F1). The only kind Classic uses.</summary>
    Manual,
    /// <summary>Modern only: rewritten at each command prompt so the game can resume after exit or crash.</summary>
    Suspend,
}

/// <summary>
/// On-disk envelope. See docs/SAVE_FORMAT.md. The checksum covers the serialized state so
/// accidental corruption is detected (it is not an anti-tamper measure).
/// </summary>
public sealed class SaveFile
{
    public const string FormatId = "avagard-save";
    /// <summary>Format id written before the game was named AVAGard; still read.</summary>
    public const string LegacyFormatId = "avatelengard-save";
    public const int CurrentSchemaVersion = 1;
    public const string RulesContent = "c64-v4.18";

    // Defaults are deliberately empty so that a file missing these fields fails validation.
    public string Format { get; set; } = "";
    public int SchemaVersion { get; set; }
    public string Content { get; set; } = "";
    public string AppVersion { get; set; } = "";
    public SaveKind Kind { get; set; }
    public Preset Preset { get; set; }
    /// <summary>Modern input deadline in seconds; null = deliberate mode. Ignored for Classic.</summary>
    public double? DeadlineSeconds { get; set; }
    public bool ManualDeathRule { get; set; }
    public DateTimeOffset SavedAt { get; set; }
    public string Checksum { get; set; } = "";
    public JsonElement State { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(SaveFile))]
[JsonSerializable(typeof(GameState))]
internal sealed partial class SaveJsonContext : JsonSerializerContext;
