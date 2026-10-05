using Telengard.Core.Engine;
using Telengard.Core.Model;
using Telengard.Core.Randomness;

namespace Telengard.Core.Tests;

/// <summary>Returns scripted values first, then falls back to a seeded generator.</summary>
public sealed class ScriptedRandom(params double[] script) : IRandomSource
{
    private readonly Queue<double> _script = new(script);
    private readonly SplitMix64Random _fallback = new(12345);
    public int Consumed { get; private set; }
    public ulong State { get => _fallback.State; set => _fallback.State = value; }
    public void Enqueue(params double[] values) { foreach (var v in values) _script.Enqueue(v); }
    public int Remaining => _script.Count;

    public double NextDouble()
    {
        Consumed++;
        return _script.Count > 0 ? _script.Dequeue() : _fallback.NextDouble();
    }
}

public sealed class Driver
{
    public Game Game { get; }
    public List<GameEvent> Log { get; } = [];

    public Driver(Game game, bool newCharacter = true)
    {
        Game = game;
        Log.AddRange(game.Start(newCharacter));
    }

    public Prompt Prompt => Game.CurrentPrompt ?? throw new InvalidOperationException("Session ended");

    public Driver Send(Command c, int number = 0)
    {
        Log.AddRange(Game.Submit(new PlayerInput(c, number), Prompt.Sequence));
        return this;
    }

    public Driver Timeout()
    {
        Log.AddRange(Game.Submit(PlayerInput.Timeout, Prompt.Sequence));
        return this;
    }

    public IEnumerable<string> Messages => Log.OfType<MessageEvent>().Select(m => m.Text);
    public bool Said(string fragment) => Messages.Any(m => m.Contains(fragment, StringComparison.OrdinalIgnoreCase));

    public static GameState Hero(int str = 10, int @int = 10, int wis = 10, int con = 12, int dex = 10, int chr = 10, string name = "TESTER")
        => CharacterCreation.NewGame(name, [str, @int, wis, con, dex, chr], new SplitMix64Random(1));
}
