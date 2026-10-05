namespace Telengard.Core.Model;

/// <summary>A monster following the player after an evade (M%/L%/H% queue, lines 30000-30200).</summary>
public readonly record struct QueuedMonster(int Type, int Level, int Hp)
{
    public bool IsEmpty => Type == 0;
}

/// <summary>
/// Everything needed to resume a game at a command prompt. Saves are only taken at the
/// ready prompt or at an inn, where no encounter is in progress (as in the original).
/// </summary>
public sealed class GameState
{
    public const int QueueLength = 20;

    public Character Character { get; set; } = new();
    public int X { get; set; }
    public int Y { get; set; }
    /// <summary>Dungeon level 1..50, or 0 while staying at an inn on the surface.</summary>
    public int Z { get; set; }
    public QueuedMonster[] Followers { get; set; } = new QueuedMonster[QueueLength];
    public ulong RandomState { get; set; }
    /// <summary>Modern-only: rooms seen, for the optional automap. Packed as z*65536 + y*256 + x.</summary>
    public HashSet<int> Explored { get; set; } = [];
    public long TurnCount { get; set; }

    public bool AtInn => Z == 0;

    public static int PackRoom(int x, int y, int z) => (z << 16) | (y << 8) | x;

    public GameState Clone()
    {
        var s = (GameState)MemberwiseClone();
        s.Character = Character.Clone();
        s.Followers = (QueuedMonster[])Followers.Clone();
        s.Explored = [.. Explored];
        return s;
    }
}
