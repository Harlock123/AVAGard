namespace Telengard.Core.Dungeon;

public enum WallKind { Open, Door, Solid }

/// <summary>Feature codes after reduction (V4.18 lines 6000-6020).</summary>
public enum FeatureKind
{
    None = 0,
    /// <summary>Level 1: stairway up to an inn. Deeper levels: elevator up one level.</summary>
    InnStairsOrElevator = 1,
    Pit = 2,
    Teleporter = 3,
    StairsDown = 4,
    Altar = 5,
    Fountain = 6,
    GrayMistyCube = 7,
    Throne = 8,
    Box = 9,
}

public enum Direction { North = 1, South = 2, West = 3, East = 4 }

/// <summary>What a room contains once the room above has been consulted for up-stairs.</summary>
public readonly record struct RoomFeatures(FeatureKind Own, bool HasStairsUp)
{
    public bool HasStairsDown => Own == FeatureKind.StairsDown;
    public bool IsEmpty => Own == FeatureKind.None && !HasStairsUp;
}

/// <summary>
/// Read-only view of the deterministic dungeon. Thread-safe; caches each level lazily.
/// </summary>
public sealed class DungeonMap
{
    public const int Size = 200;
    public const int Depth = 50;
    public const int StartX = 25, StartY = 13, StartZ = 1;

    public static DungeonMap Shared { get; } = new();

    // Index 0..201 on both axes so borders and neighbours never need special cases.
    private readonly ushort[]?[] _levels = new ushort[]?[Depth + 1];

    public int RoomWord(int x, int y, int z)
    {
        if ((uint)x > 201 || (uint)y > 201 || (uint)z > Depth)
            return RoomWordGenerator.Compute(x, y, z);
        var level = Volatile.Read(ref _levels[z]);
        if (level is null)
        {
            level = new ushort[202 * 202];
            for (int yy = 0; yy <= 201; yy++)
                for (int xx = 0; xx <= 201; xx++)
                    level[yy * 202 + xx] = (ushort)RoomWordGenerator.Compute(xx, yy, z);
            Interlocked.CompareExchange(ref _levels[z], level, null);
            level = _levels[z]!;
        }
        return level[y * 202 + x];
    }

    public static WallKind DecodeWall(int bits) => bits switch { 2 => WallKind.Door, 3 => WallKind.Solid, _ => WallKind.Open };

    /// <summary>Raw two-bit north wall value (0..3) of room (x,y).</summary>
    public int NorthBits(int x, int y, int z) => RoomWord(x, y, z) & 3;
    public int WestBits(int x, int y, int z) => (RoomWord(x, y, z) >> 2) & 3;
    public int SouthBits(int x, int y, int z) => NorthBits(x, y + 1, z);
    public int EastBits(int x, int y, int z) => WestBits(x + 1, y, z);

    public WallKind Wall(int x, int y, int z, Direction d) => DecodeWall(d switch
    {
        Direction.North => NorthBits(x, y, z),
        Direction.South => SouthBits(x, y, z),
        Direction.West => WestBits(x, y, z),
        _ => EastBits(x, y, z),
    });

    /// <summary>The unreduced feature value stored in the room word (0 = none).</summary>
    public int RawFeature(int x, int y, int z) => RoomWord(x, y, z) >> 8;

    /// <summary>Feature values above 9 are reduced by repeated subtraction of 9 (line 6010).</summary>
    public static int Reduce(int raw)
    {
        while (raw > 9) raw -= 9;
        return raw;
    }

    /// <summary>Lines 6000-6013: own feature, plus stairs up when the room above has stairs down.</summary>
    public RoomFeatures Features(int x, int y, int z)
    {
        int own = Reduce(RawFeature(x, y, z));
        int above = z == 1 ? 0 : Reduce(RawFeature(x, y, z - 1));
        if (z == Depth && own == (int)FeatureKind.StairsDown) own = 0;
        return new RoomFeatures((FeatureKind)own, above == (int)FeatureKind.StairsDown);
    }

    public static bool InBounds(int x, int y, int z) => x is >= 1 and <= Size && y is >= 1 and <= Size && z is >= 1 and <= Depth;
}
