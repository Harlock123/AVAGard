using Telengard.Core.Dungeon;

namespace Telengard.Core.Tests;

public class DungeonMapTests
{
    private readonly DungeonMap _map = DungeonMap.Shared;

    [Fact]
    public void BordersAreSolid()
    {
        for (int i = 1; i <= 200; i += 7)
        {
            Assert.Equal(WallKind.Solid, _map.Wall(i, 1, 3, Direction.North));
            Assert.Equal(WallKind.Solid, _map.Wall(i, 200, 3, Direction.South));
            Assert.Equal(WallKind.Solid, _map.Wall(1, i, 3, Direction.West));
            Assert.Equal(WallKind.Solid, _map.Wall(200, i, 3, Direction.East));
        }
    }

    [Fact]
    public void SharedWallsAgreeFromBothSides()
    {
        Assert.Equal(_map.Wall(30, 30, 2, Direction.South), _map.Wall(30, 31, 2, Direction.North));
        Assert.Equal(_map.Wall(30, 30, 2, Direction.East), _map.Wall(31, 30, 2, Direction.West));
    }

    [Fact]
    public void StartRoomIsInnStairs()
    {
        var f = _map.Features(25, 13, 1);
        Assert.Equal(FeatureKind.InnStairsOrElevator, f.Own);
        Assert.False(f.HasStairsUp); // level 1 never derives up-stairs from level 0
    }

    [Fact]
    public void StairsUpMatchStairsDownAbove()
    {
        int checkedRooms = 0;
        for (int z = 2; z <= 50; z += 6)
            for (int y = 1; y <= 200; y += 3)
                for (int x = 1; x <= 200; x += 3)
                {
                    bool below = _map.Features(x, y, z).HasStairsUp;
                    bool above = DungeonMap.Reduce(_map.RawFeature(x, y, z - 1)) == 4;
                    Assert.Equal(above, below);
                    checkedRooms++;
                }
        Assert.True(checkedRooms > 1000);
    }

    [Fact]
    public void NoStairsDownOnTheBottomLevel()
    {
        for (int y = 1; y <= 200; y++)
            for (int x = 1; x <= 200; x++)
                Assert.NotEqual(FeatureKind.StairsDown, _map.Features(x, y, 50).Own);
    }

    [Fact]
    public void FeatureReductionSubtractsNine()
    {
        Assert.Equal(9, DungeonMap.Reduce(9));
        Assert.Equal(1, DungeonMap.Reduce(10));
        Assert.Equal(6, DungeonMap.Reduce(15));
        Assert.Equal(7, DungeonMap.Reduce(16));
    }

    [Fact]
    public void LocalViewAlwaysIncludesOwnRoomWallsAndHidesBeyondBlockedSides()
    {
        for (int y = 2; y < 200; y += 17)
            for (int x = 2; x < 200; x += 13)
            {
                var v = LocalView.Compute(_map, x, y, 4, light: false);
                foreach (var w in v.Walls) Assert.NotEqual(WallKind.Open, w.Kind);
                // Own north wall is always shown when it exists.
                if (_map.NorthBits(x, y, 4) >= 2) Assert.Contains(v.Walls, w => w is { Row: 1, Col: 1, IsNorth: true });
                // If the own north side blocks the view, nothing of the north room's far walls appears.
                if (_map.NorthBits(x, y, 4) >= 2) Assert.DoesNotContain(v.Walls, w => w is { Row: 0, Col: 1, IsNorth: true });
            }
    }

    [Fact]
    public void LevelCacheMatchesDirectComputation()
    {
        var fresh = new DungeonMap();
        for (int i = 0; i < 500; i++)
        {
            int x = i * 37 % 202, y = i * 91 % 202, z = i % 51;
            Assert.Equal(RoomWordGenerator.Compute(x, y, z), fresh.RoomWord(x, y, z));
        }
    }
}
