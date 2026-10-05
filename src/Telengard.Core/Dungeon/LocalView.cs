namespace Telengard.Core.Dungeon;

/// <summary>A wall segment in the local 4×4 room grid: row 0..3 is y-1..y+2, column 0..3 is x-1..x+2.</summary>
public readonly record struct WallSegment(int Row, int Col, bool IsNorth, WallKind Kind);

/// <summary>
/// What the original draws around the player (lines 15000-15440): the player's room is grid cell
/// (1,1). Doors and walls both block the view beyond them; only walls and doors are drawn (open
/// sides are invisible). With Light active, features in the eight neighbouring rooms are shown
/// when a line of sight reaches them.
/// </summary>
public sealed class LocalView
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Z { get; init; }
    public required IReadOnlyList<WallSegment> Walls { get; init; }
    /// <summary>Neighbouring cells (row, col) revealed as containing a feature (Light only).</summary>
    public required IReadOnlyList<(int Row, int Col)> LitFeatures { get; init; }
    /// <summary>
    /// Cells whose interior is in line of sight (the hero's room plus neighbours reached through
    /// open sides), using the same paths the original uses to reveal features with Light.
    /// </summary>
    public required IReadOnlyList<(int Row, int Col)> VisibleRooms { get; init; }

    public static LocalView Compute(DungeonMap map, int x, int y, int z, bool light)
    {
        int N(int r, int c) => map.NorthBits(x + c - 1, y + r - 1, z);
        int W(int r, int c) => map.WestBits(x + c - 1, y + r - 1, z);
        bool Blocks(int bits) => bits > 1;

        var walls = new List<WallSegment>();
        void H(int r, int c) { int b = N(r, c); if (b >= 2) walls.Add(new(r, c, true, DungeonMap.DecodeWall(b))); }
        void V(int r, int c) { int b = W(r, c); if (b >= 2) walls.Add(new(r, c, false, DungeonMap.DecodeWall(b))); }

        if (!(Blocks(N(1, 1)) || Blocks(W(0, 1)))) H(0, 0);
        if (!(Blocks(N(1, 0)) || Blocks(W(1, 1)))) V(0, 0);
        if (!Blocks(N(1, 1))) { H(0, 1); V(0, 1); V(0, 2); }
        if (!(Blocks(N(1, 1)) || Blocks(W(0, 2)))) H(0, 2);
        if (!(Blocks(W(1, 2)) || Blocks(N(1, 2)))) V(0, 3);
        if (!Blocks(W(1, 2))) { H(1, 2); V(1, 3); H(2, 2); }
        if (!(Blocks(N(2, 1)) || Blocks(W(2, 2)))) H(3, 2);
        if (!(Blocks(W(1, 2)) || Blocks(N(2, 2)))) V(2, 3);
        if (!Blocks(N(2, 1))) { V(2, 2); H(3, 1); V(2, 1); }
        if (!(Blocks(N(2, 1)) || Blocks(W(2, 1)))) H(3, 0);
        if (!(Blocks(W(1, 1)) || Blocks(N(2, 0)))) V(2, 0);
        if (!Blocks(W(1, 1))) { H(2, 0); V(1, 0); H(1, 0); }
        V(1, 1); H(1, 1); V(1, 2); H(2, 1);

        bool Clear(int bits) => bits < 2;
        var visible = new List<(int, int)> { (1, 1) };
        if ((Clear(N(1, 0)) && Clear(W(1, 1))) || !(Blocks(N(1, 1)) || Blocks(W(0, 1)))) visible.Add((0, 0));
        if (Clear(N(1, 1))) visible.Add((0, 1));
        if ((Clear(N(1, 1)) && Clear(W(0, 2))) || !(Blocks(N(1, 2)) || Blocks(W(1, 2)))) visible.Add((0, 2));
        if (Clear(W(1, 2))) visible.Add((1, 2));
        if ((Clear(N(2, 2)) && Clear(W(1, 2))) || !(Blocks(N(2, 1)) || Blocks(W(2, 2)))) visible.Add((2, 2));
        if (Clear(N(2, 1))) visible.Add((2, 1));
        if ((Clear(N(2, 0)) && Clear(W(1, 1))) || !(Blocks(N(2, 1)) || Blocks(W(2, 1)))) visible.Add((2, 0));
        if (Clear(W(1, 1))) visible.Add((1, 0));

        // Lines 15300-15440: with Light, features in visible neighbouring rooms are shown.
        var lit = light
            ? visible.Where(v => v != (1, 1) && map.RawFeature(x + v.Item2 - 1, y + v.Item1 - 1, z) != 0).ToList()
            : [];

        return new LocalView { X = x, Y = y, Z = z, Walls = walls, LitFeatures = lit, VisibleRooms = visible };
    }
}
