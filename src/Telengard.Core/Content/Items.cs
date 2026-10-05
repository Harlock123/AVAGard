namespace Telengard.Core.Content;

/// <summary>Item slots 1-10. Slots 1-7 hold a plus value; 8-10 hold a count.</summary>
public static class Items
{
    public const int Sword = 1, Armor = 2, Shield = 3, ElvenCloak = 4, ElvenBoots = 5,
        RingOfRegeneration = 6, RingOfProtection = 7, ScrollOfRescue = 8, PotionOfHealing = 9, PotionOfStrength = 10;

    public const int Count = 10;

    private static readonly string[] Names =
    [
        "", "Sword", "Armor", "Shield", "Elven Cloak", "Elven Boots", "Ring of Regeneration",
        "Ring of Protection", "Scroll of Rescue", "Potion of Healing", "Potion of Strength",
    ];

    public static string Name(int slot) => Names[slot];
    public static bool HasPlus(int slot) => slot <= 7;
}

/// <summary>Coin-like treasure (line 1035) in increasing value.</summary>
public static class Valuables
{
    private static readonly string[] Names = ["refuse", "silver", "gold", "gems", "jewels"];
    public static string Name(int kind) => Names[kind];
}
