namespace Telengard.Core.Engine;

/// <summary>Every point where the game waits for the player.</summary>
public enum PromptKind
{
    Ready,            // "->" command prompt (8000)
    Combat,           // Fight / Cast / Evade (3100)
    TakeValuables,    // RETURN to pick up coins (4010)
    OpenChest,        // RETURN to open (4110)
    TakeItem,         // RETURN to pick up an item (4215)
    Approach,         // unnoticed thanks to the cloak: RETURN to approach (20905)
    Stairs,           // Up / Down / Stay (6515)
    Descend,          // descend into a pit? (6310)
    Worship,          // RETURN to worship at an altar (6605)
    Donate,           // RETURN to donate (6630)
    DonationAmount,   // number (6634)
    Drink,            // RETURN to drink from a fountain (6705)
    EnterCube,        // RETURN to walk into the cube (6805)
    CubeLevel,        // number 1-50 (6815)
    Throne,           // Pry / Sit / Read / Ignore (6905)
    PushButtons,      // Push / Ignore (7110)
    BoxButton,        // Red / Green / Yellow / Blue / Stop (7125)
    Inn,              // RETURN to re-enter, F1 save (6125)
    QuitConfirm,      // are you sure? (8700)
    Help,             // command list; any key (8896)
    SpellLevel,       // digit (21000)
    SpellNumber,      // digit, or list (21020)
    SpellList,        // any key; a digit picks that spell (21046)
    SlayHelpless,     // RETURN to kill a sleeping/webbed/held/charmed monster
    PassWallDirection,// N/S/W/E (25105)
    TeleportNorth,    // numbers (26105)
    TeleportEast,
    TeleportUp,
    Saving,           // the host is writing the save; answer SaveSucceeded / SaveFailed
}

public enum Command
{
    None,
    // dungeon
    North, South, West, East, Stay, Help, Quit, Replot, Cast, Save,
    UseScrollOfRescue, DrinkHealingPotion, DrinkStrengthPotion,
    // combat
    Fight, Evade,
    // generic answers
    Confirm,   // RETURN
    Yes, No,
    Up, Down,
    Pry, Sit, Read, Ignore,
    Push, Red, Green, Yellow, Blue, Stop,
    ListSpells,
    Number,    // with PlayerInput.Number
    SaveSucceeded, SaveFailed,
    /// <summary>Any other key. Many prompts treat "anything but RETURN" as declining.</summary>
    Other,
}

public readonly record struct PlayerInput(Command Command, int Number = 0, bool IsTimeout = false)
{
    public static readonly PlayerInput Timeout = new(Command.None, 0, true);
    public static PlayerInput Of(Command c) => new(c);
    public static PlayerInput Digit(int n) => new(Command.Number, n);
}

/// <summary>
/// A request for input. <see cref="Sequence"/> increases with every prompt so hosts can
/// discard stale input (late timer callbacks, repeated keys).
/// </summary>
public sealed record Prompt(PromptKind Kind, long Sequence, IReadOnlyList<Command> Options, bool HasDeadline, bool AcceptsNumber)
{
    /// <summary>Optional extra context for presentation (e.g. which stair directions exist).</summary>
    public string? Detail { get; init; }
}
