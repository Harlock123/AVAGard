namespace Telengard.Core.Model;

public static class Stat
{
    public const int Str = 0, Int = 1, Wis = 2, Con = 3, Dex = 4, Chr = 5;
    public static readonly string[] Abbreviations = ["STR", "INT", "WIS", "CON", "DEX", "CHR"];
    public static readonly string[] Names = ["Strength", "Intelligence", "Wisdom", "Constitution", "Dexterity", "Charisma"];
}

/// <summary>
/// The adventurer. Field meanings follow V4.18 (original variable names in comments).
/// Mutable on purpose: the engine is the only writer.
/// </summary>
public sealed class Character
{
    public const string DemoName = "DEMO";
    public const int MaxNameLength = 30;

    public string Name { get; set; } = DemoName;           // NM$
    public int[] Stats { get; set; } = new int[6];          // S(0..5)
    public int Level { get; set; } = 1;                     // LV
    public long Gold { get; set; }                          // GD: carried, not yet banked
    public long Banked { get; set; }                        // TG: total cashed at inns ("in the safe")
    public long Experience { get; set; }                    // EX
    public int Hp { get; set; }                             // CH: current
    public int MaxHp { get; set; }                          // HP
    public int SpellUnits { get; set; } = 1;                // CS: current
    public int MaxSpellUnits { get; set; } = 1;             // SU
    public int[] Items { get; set; } = new int[11];         // I(1..10)
    public int[] Timers { get; set; } = new int[12];        // SF(1..11)
    public int[] BoxCombination { get; set; } = new int[5]; // B(1..4): 1 red, 2 yellow, 3 green, 4 blue

    public bool IsDemo => Name == DemoName;

    public int Str => Stats[Stat.Str];
    public int Int => Stats[Stat.Int];
    public int Wis => Stats[Stat.Wis];
    public int Con => Stats[Stat.Con];
    public int Dex => Stats[Stat.Dex];
    public int Chr => Stats[Stat.Chr];

    public bool TimerActive(int slot) => Timers[slot] > 0;

    /// <summary>Experience needed to be at <paramref name="level"/>: 1000·2^(level-1) (lines 18000-18050).</summary>
    public static long ExperienceFor(int level) => level <= 1 ? 0 : 1000L << Math.Min(level - 1, 52);

    public Character Clone()
    {
        var c = (Character)MemberwiseClone();
        c.Stats = (int[])Stats.Clone();
        c.Items = (int[])Items.Clone();
        c.Timers = (int[])Timers.Clone();
        c.BoxCombination = (int[])BoxCombination.Clone();
        return c;
    }
}
