using Telengard.Core.Content;
using Telengard.Core.Dungeon;
using Telengard.Core.Engine;
using Telengard.Core.Model;

namespace Telengard.Core.Tests;

public class SpellTests
{
    private static Driver ReadyHero(int level, ScriptedRandom rng, int x = 100, int y = 100, int z = 5)
    {
        var state = Driver.Hero();
        var h = state.Character;
        h.Level = level;
        h.MaxSpellUnits = h.SpellUnits = 100;
        h.MaxHp = h.Hp = 100;
        state.X = x; state.Y = y; state.Z = z;
        // Find a quiet room so the first prompt is Ready.
        while (!DungeonMap.Shared.Features(state.X, state.Y, state.Z).IsEmpty) state.X++;
        rng.Enqueue(.9, .9);
        var d = new Driver(new Game(state, RulesPolicy.Classic(), rng));
        Assert.Equal(PromptKind.Ready, d.Prompt.Kind);
        return d;
    }

    [Fact]
    public void PassWallMovesThroughWalls()
    {
        var rng = new ScriptedRandom();
        var d = ReadyHero(9, rng);
        int x = d.Game.State.X;
        d.Send(Command.Cast).Send(Command.Number, 4);
        Assert.Equal(PromptKind.SpellNumber, d.Prompt.Kind);
        d.Send(Command.Number, 1);
        Assert.Equal(PromptKind.PassWallDirection, d.Prompt.Kind);
        rng.Enqueue(.9, .9);
        d.Send(Command.East);
        Assert.Equal(x + 1, d.Game.State.X);
        Assert.Equal(100 - 4, d.Game.State.Character.SpellUnits);
    }

    [Fact]
    public void SpellLevelRequiresCharacterLevel()
    {
        var rng = new ScriptedRandom();
        var d = ReadyHero(8, rng);
        d.Send(Command.Cast).Send(Command.Number, 4);
        Assert.True(d.Said("don't have that level"));
        Assert.Equal(PromptKind.Ready, d.Prompt.Kind);
        Assert.Equal(100, d.Game.State.Character.SpellUnits);
    }

    [Theory]
    [InlineData(1, 1)] [InlineData(2, 1)] [InlineData(3, 2)] [InlineData(5, 2)] [InlineData(6, 3)]
    [InlineData(9, 4)] [InlineData(12, 5)] [InlineData(15, 6)] [InlineData(40, 6)]
    public void MaxSpellLevelFollowsCharacterLevel(int characterLevel, int expected) =>
        Assert.Equal(expected, Spells.MaxSpellLevel(characterLevel));

    [Fact]
    public void CombatSpellOutsideMeleeIsWastedButCostsUnits()
    {
        var rng = new ScriptedRandom();
        var d = ReadyHero(1, rng);
        d.Send(Command.Cast).Send(Command.Number, 1).Send(Command.Number, 1);
        Assert.True(d.Said("wasted"));
        Assert.Equal(99, d.Game.State.Character.SpellUnits);
    }

    [Fact]
    public void DurationSpellAddsTurns()
    {
        var rng = new ScriptedRandom();
        var d = ReadyHero(3, rng);
        rng.Enqueue(.0); // INT(0*21+5) = 5
        d.Send(Command.Cast).Send(Command.Number, 2).Send(Command.Number, 2); // Levitate
        Assert.Equal(5, d.Game.State.Character.Timers[Effect.Levitate]);
    }

    [Fact]
    public void WordOfRecallStripsItemsAndGold()
    {
        var rng = new ScriptedRandom();
        var d = ReadyHero(15, rng);
        var h = d.Game.State.Character;
        h.Items[Items.Sword] = 5;
        h.Gold = 1234;
        rng.Enqueue(.9, .9);
        d.Send(Command.Cast).Send(Command.Number, 6).Send(Command.Number, 4);
        Assert.Equal((25, 13, 1), (d.Game.State.X, d.Game.State.Y, d.Game.State.Z));
        Assert.Equal(0, h.Items[Items.Sword]);
        Assert.Equal(0, h.Gold);
    }
}
