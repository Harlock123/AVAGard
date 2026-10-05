using Telengard.Core.Model;

namespace Telengard.Core.Engine;

public enum Tone { Normal, Good, Bad, Danger, Info, Echo }

public enum PauseKind
{
    /// <summary>The original waits about 1.5 s (GOSUB 19000).</summary>
    Beat,
    /// <summary>Wait, then clear the message area (GOSUB 17500).</summary>
    BeatAndClear,
}

public enum SoundCue { Step, Bump, Encounter, Hit, Miss, Hurt, Kill, Spell, Treasure, Trap, Teleport, Fall, Stairs, Inn, LevelUp, LevelDown, Death, Gong, Shock, Drink, Throne, Breath, Paralyzed, Drain, Confused }

public abstract record GameEvent;

/// <summary>A line for the message area / log.</summary>
public sealed record MessageEvent(string Text, Tone Tone = Tone.Normal) : GameEvent;
/// <summary>Presentation pacing. Never affects rules; hosts may shorten or skip.</summary>
public sealed record PauseEvent(PauseKind Kind) : GameEvent;
/// <summary>The original clears its message area here (GOSUB 17000).</summary>
public sealed record ClearMessagesEvent : GameEvent;
public sealed record SoundEvent(SoundCue Cue) : GameEvent;
public sealed record MonsterAppearedEvent(int Type, int Level) : GameEvent;
public sealed record MonsterGoneEvent : GameEvent;
/// <summary>The host must persist this snapshot, then answer the Saving prompt.</summary>
public sealed record SaveRequestedEvent(GameState Snapshot) : GameEvent;
public sealed record DiedEvent(bool Mighty) : GameEvent;
public sealed record SessionEndedEvent(GameOutcome Outcome) : GameEvent;
/// <summary>Position or level changed (teleport, stairs, cube, etc.).</summary>
public sealed record MovedEvent(int X, int Y, int Z) : GameEvent;

public enum GameOutcome { Running, Died, SavedAndExited }
