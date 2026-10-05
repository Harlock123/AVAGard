using Telengard.Core.Content;
using Telengard.Core.Dungeon;
using Telengard.Core.Engine;
using Telengard.Core.Model;
using Telengard.Core.Randomness;

namespace Telengard.Core.Tests;

public class EngineFlowTests
{
    // Draws that skip the encounter (RND > .3) and treasure (RND > .2) phases.
    private const double NoEncounter = .9, NoTreasure = .9;

    [Fact]
    public void NewCharacterStartsBelowTheInnAndCanClimbToIt()
    {
        var rng = new ScriptedRandom(NoEncounter, NoTreasure);
        var state = Driver.Hero();
        state.Character.Gold = 500;
        var d = new Driver(new Game(state, RulesPolicy.Classic(), rng));

        Assert.Equal(PromptKind.Stairs, d.Prompt.Kind);
        Assert.Contains(Command.Up, d.Prompt.Options);
        Assert.DoesNotContain(Command.Down, d.Prompt.Options);

        d.Send(Command.Up);
        Assert.Equal(PromptKind.Inn, d.Prompt.Kind);
        Assert.False(d.Prompt.HasDeadline);
        Assert.Equal(0, state.Z);
        Assert.Equal(500, state.Character.Experience); // gold becomes experience 1:1
        Assert.Equal(0, state.Character.Gold);
        Assert.Equal(500, state.Character.Banked);
    }

    [Fact]
    public void StairsTimeoutMeansStay()
    {
        var d = new Driver(new Game(Driver.Hero(), RulesPolicy.Classic(), new ScriptedRandom(NoEncounter, NoTreasure)));
        d.Timeout();
        Assert.Equal(PromptKind.Ready, d.Prompt.Kind);
        Assert.Equal(1, d.Game.State.Z);
    }

    [Fact]
    public void ReadyTimeoutIsStayAndAdvancesATurn()
    {
        var rng = new ScriptedRandom(NoEncounter, NoTreasure);
        var d = new Driver(new Game(Driver.Hero(), RulesPolicy.Classic(), rng));
        d.Send(Command.Stay); // leave the stair prompt
        long turns = d.Game.State.TurnCount;
        rng.Enqueue(NoEncounter, NoTreasure);
        d.Timeout();
        Assert.Equal(turns + 1, d.Game.State.TurnCount);
    }

    [Fact]
    public void StaleAndDuplicateInputIsIgnored()
    {
        var d = new Driver(new Game(Driver.Hero(), RulesPolicy.Classic(), new ScriptedRandom(NoEncounter, NoTreasure)));
        var seq = d.Prompt.Sequence;
        d.Game.Submit(PlayerInput.Of(Command.Stay), seq);
        var after = d.Prompt.Sequence;
        Assert.Empty(d.Game.Submit(PlayerInput.Of(Command.Stay), seq)); // duplicate of the old prompt
        Assert.Equal(after, d.Prompt.Sequence);
    }

    [Fact]
    public void DemoCharacterHasNoDeadlines()
    {
        var state = Driver.Hero(name: "");
        Assert.Equal(Character.DemoName, state.Character.Name);
        var d = new Driver(new Game(state, RulesPolicy.Classic(), new ScriptedRandom(NoEncounter, NoTreasure)));
        Assert.False(d.Prompt.HasDeadline);
        Assert.Empty(d.Game.Submit(PlayerInput.Timeout, d.Prompt.Sequence));
    }

    [Fact]
    public void BlockedMoveDoesNotUseATurn()
    {
        var map = DungeonMap.Shared;
        var d = new Driver(new Game(Driver.Hero(), RulesPolicy.Classic(), new ScriptedRandom(NoEncounter, NoTreasure)));
        d.Send(Command.Stay);
        var dir = new[] { Command.North, Command.South, Command.West, Command.East }
            .FirstOrDefault(c => map.Wall(25, 13, 1, c switch
            {
                Command.North => Direction.North, Command.South => Direction.South, Command.West => Direction.West, _ => Direction.East,
            }) == WallKind.Solid);
        if (dir == Command.None) return; // the start room has no solid wall: nothing to test
        long turns = d.Game.State.TurnCount;
        d.Send(dir);
        Assert.Equal(PromptKind.Ready, d.Prompt.Kind);
        Assert.Equal(turns, d.Game.State.TurnCount);
        Assert.True(d.Said("No."));
    }

    [Fact]
    public void FightingAndKillingAMonsterAwardsExperience()
    {
        // Encounter: RND .1 (<=.3) → type INT(.0*20+1)=1 gnoll; cloak roll; level RND .0 → 1;
        // HP: RND^.5*1*1+1 → 1; special .5 (not >.95); initiative .0 (player first).
        var rng = new ScriptedRandom(.1, .0, .99, .0, .0, .5, .0);
        var state = Driver.Hero(str: 18);
        var d = new Driver(new Game(state, RulesPolicy.Classic(), rng));
        Assert.Equal(PromptKind.Combat, d.Prompt.Kind);
        Assert.Equal((1, 1), d.Game.Monster);
        rng.Enqueue(.9, .5, .5); // to-hit 18+1+9 ≥ 10; damage
        rng.Enqueue(.9);          // AfterKill: >.5 → straight to treasure… draw trap/kind
        d.Send(Command.Fight);
        Assert.Equal(10, state.Character.Experience); // 1 × 1 × 10
        Assert.Null(d.Game.Monster);
    }

    [Fact]
    public void ClassicSaveEndsTheSessionModernContinues()
    {
        foreach (var (policy, ends) in new[] { (RulesPolicy.Classic(), true), (RulesPolicy.Modern(), false) })
        {
            var d = new Driver(new Game(Driver.Hero(), policy, new ScriptedRandom(NoEncounter, NoTreasure)));
            d.Send(Command.Stay);
            d.Send(Command.Save);
            Assert.Equal(PromptKind.Saving, d.Prompt.Kind);
            Assert.Single(d.Log.OfType<SaveRequestedEvent>());
            d.Send(Command.SaveSucceeded);
            Assert.Equal(ends, d.Game.CurrentPrompt is null);
            Assert.Equal(ends ? GameOutcome.SavedAndExited : GameOutcome.Running, d.Game.Outcome);
        }
    }

    [Fact]
    public void DemoCharacterCannotBeSaved()
    {
        var d = new Driver(new Game(Driver.Hero(name: ""), RulesPolicy.Classic(), new ScriptedRandom(NoEncounter, NoTreasure)));
        d.Send(Command.Stay);
        d.Send(Command.Save);
        Assert.Equal(PromptKind.Ready, d.Prompt.Kind);
        Assert.Empty(d.Log.OfType<SaveRequestedEvent>());
    }

    [Fact]
    public void QuittingKillsTheCharacter()
    {
        var d = new Driver(new Game(Driver.Hero(), RulesPolicy.Classic(), new ScriptedRandom(NoEncounter, NoTreasure)));
        d.Send(Command.Stay).Send(Command.Quit).Send(Command.Yes);
        Assert.Null(d.Game.CurrentPrompt);
        Assert.Equal(GameOutcome.Died, d.Game.Outcome);
        Assert.Single(d.Log.OfType<DiedEvent>());
    }

    [Fact]
    public void ScrollOfRescueReturnsToStartAndDropsGold()
    {
        var state = Driver.Hero();
        state.X = 100; state.Y = 100; state.Z = 7;
        state.Character.Items[Items.ScrollOfRescue] = 1;
        state.Character.Gold = 999;
        var rng = new ScriptedRandom(NoEncounter, NoTreasure);
        var game = new Game(state, RulesPolicy.Classic(), rng);
        var d = new Driver(game, newCharacter: true);
        while (d.Prompt.Kind != PromptKind.Ready) d.Timeout();
        rng.Enqueue(NoEncounter, NoTreasure);
        d.Send(Command.UseScrollOfRescue);
        Assert.Equal((25, 13, 1), (state.X, state.Y, state.Z));
        Assert.Equal(0, state.Character.Gold);
        Assert.Equal(0, state.Character.Items[Items.ScrollOfRescue]);
    }
}
