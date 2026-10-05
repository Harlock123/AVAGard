namespace Telengard.Core.Engine;

public enum Preset { Classic, Modern }

/// <summary>
/// Everything that differs between presets. Gameplay rules are shared; only timing, saving and
/// presentation-facing behaviour live here. See docs/CLASSIC_VS_MODERN.md.
/// </summary>
public sealed record RulesPolicy
{
    public required Preset Preset { get; init; }

    /// <summary>
    /// How long a prompt waits before its default action. Null = deliberate mode (no deadline;
    /// a disclosed change to the original real-time rules). The DEMO character never has a deadline.
    /// </summary>
    public TimeSpan? InputDeadline { get; init; }

    /// <summary>Classic: saving returns to the title (V4.18 runs the program again after storing).</summary>
    public bool SaveEndsSession { get; init; }

    /// <summary>Modern: keep a suspend snapshot at each command prompt so the game resumes after exit or crash.</summary>
    public bool SuspendSnapshots { get; init; }

    /// <summary>
    /// The manual's rule (not enforced by the V4.18 code): on death the character's save is deleted
    /// unless the name begins with "SV". Off by default.
    /// </summary>
    public bool ManualDeathRule { get; init; }

    /// <summary>Record explored rooms for the optional automap.</summary>
    public bool TrackExploration { get; init; }

    /// <summary>
    /// V4.18's 400-poll timeout, as emulated (about 3.4-3.8 s). Reconstruction: no
    /// real-hardware measurement exists; the manual says "about 5 seconds".
    /// </summary>
    public static readonly TimeSpan ClassicDeadline = TimeSpan.FromSeconds(3.7);

    public static RulesPolicy Classic() => new()
    {
        Preset = Preset.Classic,
        InputDeadline = ClassicDeadline,
        SaveEndsSession = true,
        SuspendSnapshots = false,
        TrackExploration = false,
    };

    public static RulesPolicy Modern(TimeSpan? deadline = null) => new()
    {
        Preset = Preset.Modern,
        InputDeadline = deadline,
        SaveEndsSession = false,
        SuspendSnapshots = true,
        TrackExploration = true,
    };
}
