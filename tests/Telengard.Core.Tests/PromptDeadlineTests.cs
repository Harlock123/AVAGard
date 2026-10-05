using Telengard.Core.Engine;
using Telengard.Core.Timing;

namespace Telengard.Core.Tests;

public class PromptDeadlineTests
{
    [Fact]
    public void FiresOnceAfterTheDeadlineWithTheArmedSequence()
    {
        var clock = new ManualTimeProvider();
        using var deadline = new PromptDeadline(clock);
        var fired = new List<long>();
        deadline.Arm(7, TimeSpan.FromSeconds(3.7), fired.Add);
        clock.Advance(TimeSpan.FromSeconds(3.6));
        Assert.Empty(fired);
        Assert.InRange(deadline.RemainingFraction!.Value, 0.01, 0.05);
        clock.Advance(TimeSpan.FromSeconds(0.2));
        Assert.Equal([7L], fired);
        clock.Advance(TimeSpan.FromSeconds(10));
        Assert.Single(fired);
    }

    [Fact]
    public void RearmingCancelsThePreviousPrompt()
    {
        var clock = new ManualTimeProvider();
        using var deadline = new PromptDeadline(clock);
        var fired = new List<long>();
        deadline.Arm(1, TimeSpan.FromSeconds(2), fired.Add);
        clock.Advance(TimeSpan.FromSeconds(1));
        deadline.Arm(2, TimeSpan.FromSeconds(2), fired.Add);
        clock.Advance(TimeSpan.FromSeconds(1.5));
        Assert.Empty(fired);
        clock.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal([2L], fired);
    }

    [Fact]
    public void PauseFreezesTheRemainingTime()
    {
        var clock = new ManualTimeProvider();
        using var deadline = new PromptDeadline(clock);
        var fired = new List<long>();
        deadline.Arm(3, TimeSpan.FromSeconds(4), fired.Add);
        clock.Advance(TimeSpan.FromSeconds(3));
        deadline.Pause();
        clock.Advance(TimeSpan.FromMinutes(5));
        Assert.Empty(fired);
        deadline.Resume();
        clock.Advance(TimeSpan.FromSeconds(0.9));
        Assert.Empty(fired);
        clock.Advance(TimeSpan.FromSeconds(0.2));
        Assert.Equal([3L], fired);
    }

    [Fact]
    public void TimeoutThroughTheEngineChoosesThePromptDefault()
    {
        // End-to-end: the deadline expiring at the ready prompt makes the hero STAY (a turn passes).
        var clock = new ManualTimeProvider();
        var rng = new ScriptedRandom(.9, .9);
        var game = new Game(Driver.Hero(), RulesPolicy.Classic(), rng);
        game.Start(true);
        game.Submit(PlayerInput.Of(Command.Stay), game.CurrentPrompt!.Sequence); // leave stairs
        Assert.Equal(PromptKind.Ready, game.CurrentPrompt!.Kind);
        long turns = game.State.TurnCount;

        using var deadline = new PromptDeadline(clock);
        rng.Enqueue(.9, .9);
        deadline.Arm(game.CurrentPrompt.Sequence, RulesPolicy.ClassicDeadline, seq => game.Submit(PlayerInput.Timeout, seq));
        clock.Advance(TimeSpan.FromSeconds(4));
        Assert.Equal(turns + 1, game.State.TurnCount);
    }
}
