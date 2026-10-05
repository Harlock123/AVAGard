using Telengard.Core.Dungeon;

namespace Telengard.Desktop.Controls;

/// <summary>Immutable data the dungeon view renders. A new instance triggers a redraw.</summary>
public sealed record DungeonSnapshot(
    int X, int Y, int Z,
    LocalView? View,
    RoomFeatures Features,
    (int Type, int Level)? Monster,
    IReadOnlyList<int> FollowerDistances,
    bool Invisible,
    string? InnName,
    IReadOnlySet<int>? Explored)
{
    public bool AtInn => InnName is not null;
}
