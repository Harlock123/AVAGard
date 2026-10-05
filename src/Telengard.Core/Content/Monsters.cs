namespace Telengard.Core.Content;

/// <summary>The twenty monster types, numbered as in the original (1 = weakest rank).</summary>
public static class Monsters
{
    public const int Count = 20;

    private static readonly string[] Names =
    [
        "", "Gnoll", "Kobold", "Skeleton", "Hobbit", "Zombie", "Orc", "Fighter", "Mummy", "Elf", "Ghoul",
        "Dwarf", "Troll", "Wraith", "Ogre", "Minotaur", "Giant", "Specter", "Vampire", "Demon", "Dragon",
    ];

    public const int Hobbit = 4, Elf = 9, Ghoul = 10, Dwarf = 11, Wraith = 13, Specter = 17, Vampire = 18, Demon = 19, Dragon = 20;

    public static string Name(int type) => Names[type];

    /// <summary>Line 20700: skeleton, zombie, mummy, ghoul, wraith, specter, vampire.</summary>
    public static bool IsUndead(int type) => type is 3 or 5 or 8 or 10 or 13 or 17 or 18;
}
