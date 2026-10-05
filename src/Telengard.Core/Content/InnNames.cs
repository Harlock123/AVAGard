namespace Telengard.Core.Content;

/// <summary>
/// Inn names are built from three word lists indexed by the inn's coordinates, using the same
/// digit formula as V4.18 (lines 6103-6109): (x·y) mod 10, (x+y) mod 10, (3x+7y) mod 10.
/// The word lists below are newly written for this project (the original lists are not copied),
/// so inn names differ from the original game's. See docs/FIDELITY.md.
/// </summary>
public static class InnNames
{
    private static readonly string[] First = ["Merry", "Crooked", "Golden", "Weary", "Silver", "Hidden", "Jolly", "Painted", "Sleeping", "Brazen"];
    private static readonly string[] Second = ["Lantern", "Goblet", "Griffin", "Anvil", "Kettle", "Raven", "Lamb", "Barrel", "Candle", "Boar"];
    private static readonly string[] Third = ["Inn", "Tavern", "Hostel", "Rest", "Inn", "Lodge", "Taproom", "Hearth", "Inn", "Hall"];

    public static string For(int x, int y) =>
        $"{First[(x * y) % 10]} {Second[(x + y) % 10]} {Third[(x * 3 + y * 7) % 10]}";
}
