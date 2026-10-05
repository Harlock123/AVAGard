using Telengard.Core.Dungeon;
using Telengard.Core.Randomness;

namespace Telengard.Core.Model;

/// <summary>Lines 1400-1600: rolling attributes and building a new adventurer.</summary>
public static class CharacterCreation
{
    /// <summary>Six attributes, each 3d6, in the order STR INT WIS CON DEX CHR.</summary>
    public static int[] RollStats(IRandomSource rng)
    {
        var stats = new int[6];
        for (int i = 0; i < 6; i++)
            for (int d = 0; d < 3; d++)
                stats[i] += (int)(rng.NextDouble() * 6 + 1);
        return stats;
    }

    /// <summary>Line 16235: names use A-Z and 0-9 only, at most 30 characters; empty means DEMO.</summary>
    public static string NormalizeName(string? raw)
    {
        var chars = (raw ?? "").ToUpperInvariant().Where(c => c is >= 'A' and <= 'Z' or >= '0' and <= '9')
            .Take(Character.MaxNameLength).ToArray();
        return chars.Length == 0 ? Character.DemoName : new string(chars);
    }

    public static GameState NewGame(string name, int[] stats, IRandomSource rng)
    {
        var c = new Character
        {
            Name = NormalizeName(name),
            Stats = (int[])stats.Clone(),
            Level = 1,
            Hp = stats[Stat.Con],
            MaxHp = stats[Stat.Con],
            SpellUnits = 1,
            MaxSpellUnits = 1,
        };
        NewBoxCombination(c, rng);
        return new GameState
        {
            Character = c,
            X = DungeonMap.StartX,
            Y = DungeonMap.StartY,
            Z = DungeonMap.StartZ,
            RandomState = rng.State,
        };
    }

    /// <summary>Line 19800.</summary>
    public static void NewBoxCombination(Character c, IRandomSource rng)
    {
        for (int i = 1; i <= 4; i++)
            c.BoxCombination[i] = (int)(rng.NextDouble() * 4 + 1);
    }
}
