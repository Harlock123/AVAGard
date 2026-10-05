namespace Telengard.Core.Model;

/// <summary>Duration effect slots SF(1..11); each decrements once per turn and may go negative.</summary>
public static class Effect
{
    public const int Strength = 1, DetectTraps = 2, Light = 3, ProtectionFromEvil = 4, Levitate = 5,
        Invisibility = 6, Fear = 7, AstralWalk = 8, TimeStop = 9, RaiseDead = 10, Drunk = 11;

    public const int Count = 11;

    private static readonly string[] Names =
    [
        "", "Strength", "Detect Traps", "Light", "Protection", "Levitate", "Invisible", "Fear",
        "Astral Walk", "Time Stop", "Raise Dead", "Drunk",
    ];

    public static string Name(int slot) => Names[slot];
}
