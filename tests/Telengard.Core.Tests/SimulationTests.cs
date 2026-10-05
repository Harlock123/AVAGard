using Telengard.Core.Dungeon;
using Telengard.Core.Engine;
using Telengard.Core.Model;
using Telengard.Core.Randomness;

namespace Telengard.Core.Tests;

/// <summary>
/// Automated play with random (but seeded) inputs. These find crashes, stuck states and
/// broken invariants; they make no claim about historical balance.
/// </summary>
public class SimulationTests
{
    public static Prompt? Play(Game game, Random bot, int maxInputs, Action<Game>? check = null)
    {
        for (int i = 0; i < maxInputs && game.CurrentPrompt is { } p; i++)
        {
            PlayerInput input;
            if (p.Kind == PromptKind.Saving)
                input = PlayerInput.Of(bot.Next(4) == 0 ? Command.SaveFailed : Command.SaveSucceeded);
            else if (p.HasDeadline && bot.Next(6) == 0)
                input = PlayerInput.Timeout;
            else if (p.AcceptsNumber && bot.Next(3) > 0)
                input = PlayerInput.Digit(bot.Next(2) == 0 ? bot.Next(1, 7) : bot.Next(-3, 60));
            else if (bot.Next(12) == 0)
                input = PlayerInput.Of(Command.Other);
            else if (p.Kind == PromptKind.Ready && bot.Next(5) == 0)
                input = PlayerInput.Of(Command.Cast);
            else
                input = PlayerInput.Of(p.Options[bot.Next(p.Options.Count)]);

            // Avoid quitting too eagerly so sessions get deep.
            if (input.Command is Command.Quit or Command.Save && bot.Next(10) > 0) input = PlayerInput.Of(Command.Stay);
            if (input.Command == Command.Stay && p.Kind != PromptKind.Ready && !p.Options.Contains(Command.Stay))
                input = PlayerInput.Of(p.Options[0]);

            game.Submit(input, p.Sequence);
            check?.Invoke(game);
        }
        return game.CurrentPrompt;
    }

    private static void Invariants(Game g)
    {
        var s = g.State;
        var h = s.Character;
        if (g.CurrentPrompt is null) return;
        Assert.True(s.Z == 0 || DungeonMap.InBounds(s.X, s.Y, s.Z), $"out of bounds {s.X},{s.Y},{s.Z}");
        Assert.InRange(s.X, 1, 200);
        Assert.InRange(s.Y, 1, 200);
        Assert.InRange(s.Z, 0, 50);
        Assert.True(h.Hp <= h.MaxHp, "HP above maximum");
        Assert.True(h.Hp >= 1 || g.CurrentPrompt.Kind == PromptKind.Saving, "alive with no HP");
        Assert.All(h.Stats, v => Assert.InRange(v, 2, 18)); // Raise Dead can take CON below 3
        for (int i = 1; i <= 10; i++) Assert.True(h.Items[i] >= 0);
        Assert.True(h.Level >= 1);
        if (g.CurrentPrompt.Kind == PromptKind.Ready) Assert.Null(g.Monster);
        if (g.CurrentPrompt.Kind == PromptKind.Inn) Assert.Equal(0, s.Z);
    }

    [Theory]
    [InlineData(Preset.Classic)]
    [InlineData(Preset.Modern)]
    public void RandomPlayNeverCrashesOrBreaksInvariants(Preset preset)
    {
        int deaths = 0, turns = 0;
        var reached = new HashSet<PromptKind>();
        for (int seed = 1; seed <= 2000; seed++)
        {
            var rng = new SplitMix64Random((ulong)seed);
            var state = CharacterCreation.NewGame("SIM" + seed, CharacterCreation.RollStats(rng), rng);
            // Give some sessions a seasoned hero so deeper systems (spells, deep levels) are exercised.
            if (seed % 3 == 0)
            {
                var h = state.Character;
                h.Level = 1 + seed % 18;
                h.MaxHp = h.Hp = 60 + seed % 200;
                h.MaxSpellUnits = h.SpellUnits = 200;
                state.Z = 1 + seed % 50;
                state.X = 1 + seed * 7 % 200;
                state.Y = 1 + seed * 13 % 200;
            }
            var policy = preset == Preset.Classic ? RulesPolicy.Classic() : RulesPolicy.Modern(TimeSpan.FromSeconds(10));
            var game = new Game(state, policy, rng);
            game.Start(true);
            Play(game, new Random(seed), 3000, g =>
            {
                Invariants(g);
                if (g.CurrentPrompt is { } p) reached.Add(p.Kind);
            });
            turns += (int)game.State.TurnCount;
            if (game.Outcome == GameOutcome.Died) deaths++;
        }
        Assert.True(turns > 10_000, $"only {turns} turns simulated");
        Assert.True(deaths > 0, "nobody ever died, which suggests combat is not reachable");
        var missing = Enum.GetValues<PromptKind>().Except(reached).ToList();
        Assert.True(missing.Count == 0, "never reached: " + string.Join(", ", missing));
    }

    [Fact]
    public void SameSeedAndInputsGiveIdenticalGames()
    {
        string Run()
        {
            var rng = new SplitMix64Random(99);
            var state = CharacterCreation.NewGame("SAME", CharacterCreation.RollStats(rng), rng);
            var game = new Game(state, RulesPolicy.Classic(), rng);
            var log = new List<string>();
            log.AddRange(game.Start(true).OfType<MessageEvent>().Select(m => m.Text));
            var bot = new Random(5);
            for (int i = 0; i < 500 && game.CurrentPrompt is { } p; i++)
            {
                var input = p.Kind == PromptKind.Saving ? PlayerInput.Of(Command.SaveSucceeded) : PlayerInput.Of(p.Options[bot.Next(p.Options.Count)]);
                if (input.Command is Command.Quit or Command.Save) input = PlayerInput.Of(p.Options[0]);
                log.AddRange(game.Submit(input, p.Sequence).OfType<MessageEvent>().Select(m => m.Text));
            }
            return string.Join("\n", log) + $"|{state.X},{state.Y},{state.Z}|{state.Character.Experience}";
        }
        Assert.Equal(Run(), Run());
    }
}
