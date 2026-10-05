using Telengard.Core.Content;
using Telengard.Core.Dungeon;
using Telengard.Core.Model;
using Telengard.Core.Randomness;

namespace Telengard.Core.Engine;

/// <summary>
/// The rules engine for one play session. It is UI-independent and deterministic given its
/// random source. Hosts call <see cref="Start"/> once, then <see cref="Submit"/> with each
/// answer to <see cref="CurrentPrompt"/>; every call returns the events produced until the
/// next prompt.
///
/// The flow mirrors the reference program's structure: each <see cref="Label"/> corresponds to a
/// block of the V4.18 listing (line numbers cited), so fidelity can be reviewed block by block.
/// It is written as a C# iterator so the game can pause at any prompt without threads.
/// </summary>
public sealed partial class Game
{
    private enum Label
    {
        Tick,              // 2000
        EncounterRoll,     // 3000
        PickMonster,       // 3005
        MonsterLevel,      // 3007
        ShowMonster,       // 3010
        Initiative,        // 3090
        CombatPrompt,      // 3100
        PlayerFight,       // 3200
        Killed,            // 3220
        KilledExperience,  // 3223
        MonsterAttack,     // 3300
        Evade,             // 3700
        EvadeSuccess,      // 3710
        AfterKill,         // 3900
        TreasureRoll,      // 4000
        TreasureFound,     // 4002
        Item,              // 4200
        AfterTreasure,     // 4900
        NotNoticed,        // 20900
        Features,          // 6000
        Inn,               // 6103
        InnPrompt,         // 6125
        Elevator,          // 6200
        Pit,               // 6300
        Teleporter,        // 6400
        Stairs,            // 6500
        Altar,             // 6600
        Fountain,          // 6700
        Cube,              // 6800
        Throne,            // 6900
        ThroneKing,        // 7030
        Box,               // 7100
        Ready,             // 8000
        Move,              // 8120-8305
        Save,              // 8600
        Death,             // 9000
        Cast,              // 21000
        SpellEnd,          // 21900
        SpellCancel,       // 21910
        MonsterDriven,     // 21920
        End,
    }

    private readonly IRandomSource _rng;
    private readonly List<GameEvent> _events = [];
    private IEnumerator<Prompt>? _flow;
    private Label _next;
    private long _sequence;
    private PlayerInput _in;

    // Transient encounter registers (original variable names in comments).
    private int _m;        // M: monster type, 0 = none
    private int _ml;       // ML: monster level
    private int _mh;       // MH: monster hit points
    private bool _undead;  // UN
    private int _l;        // L: level used for treasure (CZ, or ML after an encounter)
    private bool _melee;   // D: casting in melee
    private bool _trapped; // T
    private bool _chain;   // C=1 after a chest: items may follow each other
    private int _coinKind; // I in 4007
    private Direction _moveDirection;
    private int _ownFeature, _aboveFeature; // I and J in 6000

    public Game(GameState state, RulesPolicy policy, IRandomSource rng, DungeonMap? map = null)
    {
        State = state;
        Policy = policy;
        _rng = rng;
        Map = map ?? DungeonMap.Shared;
    }

    public GameState State { get; }

    /// <summary>Convenience overload: true = new character, false = stored character.</summary>
    public IReadOnlyList<GameEvent> Start(bool newCharacter) =>
        Start(newCharacter ? StartPoint.NewCharacter : StartPoint.StoredCharacter);
    public RulesPolicy Policy { get; }
    public DungeonMap Map { get; }
    public Character Hero => State.Character;
    public Prompt? CurrentPrompt { get; private set; }
    public GameOutcome Outcome { get; private set; } = GameOutcome.Running;

    /// <summary>The monster currently faced, for presentation (null when none).</summary>
    public (int Type, int Level)? Monster { get; private set; }

    /// <summary>True when prompts in this session have a deadline (never for DEMO).</summary>
    public bool HasDeadlines => Policy.InputDeadline is not null && !Hero.IsDemo;

    /// <summary>
    /// Begins the session. A brand-new character starts with the encounter roll (line 1630);
    /// a stored character starts with a full turn (line 1300). A Modern suspend snapshot resumes
    /// at the command prompt it was taken at.
    /// </summary>
    public IReadOnlyList<GameEvent> Start(StartPoint start)
    {
        if (_flow is not null) throw new InvalidOperationException("Already started.");
        if (State.Z <= 0) State.Z = 1; // line 1285: a character saved at an inn resumes below it
        _l = State.Z;
        _next = start switch
        {
            StartPoint.NewCharacter => Label.EncounterRoll,
            StartPoint.StoredCharacter => Label.Tick,
            _ => Label.Ready,
        };
        MarkExplored();
        _flow = Run().GetEnumerator();
        return Advance();
    }

    /// <summary>
    /// Answers the current prompt. Input for any other prompt (stale sequence), timeouts on a
    /// prompt without a deadline, and input after the session has ended are ignored.
    /// </summary>
    public IReadOnlyList<GameEvent> Submit(PlayerInput input, long promptSequence)
    {
        if (_flow is null || CurrentPrompt is null || promptSequence != CurrentPrompt.Sequence)
            return [];
        if (input.IsTimeout && !CurrentPrompt.HasDeadline)
            return [];
        if (CurrentPrompt.Kind == PromptKind.Saving && input.Command is not (Command.SaveSucceeded or Command.SaveFailed))
            return [];
        _in = input;
        return Advance();
    }

    private IReadOnlyList<GameEvent> Advance()
    {
        _events.Clear();
        CurrentPrompt = _flow!.MoveNext() ? _flow.Current : null;
        State.RandomState = _rng.State;
        if (CurrentPrompt is null && Outcome == GameOutcome.Running)
            Outcome = GameOutcome.Died;
        return _events.ToArray();
    }

    private IEnumerable<Prompt> Run()
    {
        while (_next != Label.End)
        {
            foreach (var p in Step(_next))
                yield return p;
        }
    }

    private IEnumerable<Prompt> Step(Label label) => label switch
    {
        Label.Tick => Tick(),
        Label.EncounterRoll => EncounterRoll(),
        Label.PickMonster => PickMonster(),
        Label.MonsterLevel => MonsterLevel(),
        Label.ShowMonster => ShowMonster(),
        Label.Initiative => Initiative(),
        Label.CombatPrompt => CombatPrompt(),
        Label.PlayerFight => PlayerFight(),
        Label.Killed => Killed(),
        Label.KilledExperience => KilledExperience(),
        Label.MonsterAttack => MonsterAttack(),
        Label.Evade => Evade(),
        Label.EvadeSuccess => EvadeSuccess(),
        Label.AfterKill => AfterKill(),
        Label.TreasureRoll => TreasureRoll(),
        Label.TreasureFound => TreasureFound(),
        Label.Item => Item(),
        Label.AfterTreasure => AfterTreasure(),
        Label.NotNoticed => NotNoticed(),
        Label.Features => Features(),
        Label.Inn => Inn(),
        Label.InnPrompt => InnPrompt(),
        Label.Elevator => Elevator(),
        Label.Pit => Pit(),
        Label.Teleporter => Teleporter(),
        Label.Stairs => Stairs(),
        Label.Altar => Altar(),
        Label.Fountain => Fountain(),
        Label.Cube => Cube(),
        Label.Throne => Throne(),
        Label.ThroneKing => ThroneKing(),
        Label.Box => Box(),
        Label.Ready => Ready(),
        Label.Move => Move(),
        Label.Save => SaveGame(),
        Label.Death => Death(),
        Label.Cast => Cast(),
        Label.SpellEnd => SpellEnd(),
        Label.SpellCancel => SpellCancel(),
        Label.MonsterDriven => MonsterDriven(),
        _ => throw new InvalidOperationException(label.ToString()),
    };

    // ---- helpers -------------------------------------------------------------------------

    private double Rnd() => _rng.NextDouble();

    /// <summary>BASIC INT(): floor.</summary>
    private static int Int(double v) => (int)Math.Floor(v);
    private static long LInt(double v) => (long)Math.Floor(v);

    /// <summary>FNR(n) = INT(RND*n+1).</summary>
    private int Fnr(int n) => Int(Rnd() * n + 1);

    private Prompt Ask(PromptKind kind, params Command[] options) => Ask(kind, true, false, options);

    private Prompt Ask(PromptKind kind, bool deadline, bool number, params Command[] options)
    {
        _in = default;
        return new Prompt(kind, ++_sequence, options, deadline && HasDeadlines, number);
    }

    private PlayerInput In => _in;
    private bool Pressed(Command c) => !_in.IsTimeout && _in.Command == c;
    private bool ReturnPressed => Pressed(Command.Confirm);

    private void Say(string text, Tone tone = Tone.Normal) => _events.Add(new MessageEvent(text, tone));
    private void Echo(string text) => _events.Add(new MessageEvent(text, Tone.Echo));
    private void Beat() => _events.Add(new PauseEvent(PauseKind.Beat));
    private void BeatClear() => _events.Add(new PauseEvent(PauseKind.BeatAndClear));
    private void Clear() => _events.Add(new ClearMessagesEvent());
    private void Sound(SoundCue cue) => _events.Add(new SoundEvent(cue));

    private string MonsterName => Monsters.Name(_m);

    private void ShowMonsterSprite()
    {
        Monster = (_m, _ml);
        _events.Add(new MonsterAppearedEvent(_m, _ml));
    }

    private void HideMonster()
    {
        if (Monster is null) return;
        Monster = null;
        _events.Add(new MonsterGoneEvent());
    }

    private void Moved()
    {
        MarkExplored();
        _events.Add(new MovedEvent(State.X, State.Y, State.Z));
    }

    private void MarkExplored()
    {
        if (!Policy.TrackExploration || State.Z < 1) return;
        State.Explored.Add(GameState.PackRoom(State.X, State.Y, State.Z));
    }

    /// <summary>Damage to the hero; returns true if still alive (CH &gt;= 1).</summary>
    private bool Hurt(int amount)
    {
        Hero.Hp -= amount;
        Sound(SoundCue.Hurt);
        return Hero.Hp >= 1;
    }

    private void Heal(int amount)
    {
        Hero.Hp = Math.Min(Hero.MaxHp, Hero.Hp + amount);
    }

    /// <summary>Adds to a duration timer, first resetting it to 0 if negative (e.g. 22402).</summary>
    private void AddTimer(int slot, int turns)
    {
        if (Hero.Timers[slot] < 0) Hero.Timers[slot] = 0;
        Hero.Timers[slot] += turns;
    }

    // ---- following-monster queue (30000-30200) --------------------------------------------

    /// <summary>Line 30000: take the nearest follower into the encounter registers.</summary>
    private void PopFollower()
    {
        var q = State.Followers;
        _m = 0;
        if (!q[0].IsEmpty)
        {
            _m = q[0].Type;
            _ml = q[0].Level;
            _mh = q[0].Hp;
            _undead = Monsters.IsUndead(_m);
        }
        Array.Copy(q, 1, q, 0, q.Length - 1);
        q[^1] = default;
    }

    /// <summary>Line 30100: every move pushes the current monster (or nothing) onto the queue.</summary>
    private void PushFollower()
    {
        var q = State.Followers;
        Array.Copy(q, 0, q, 1, q.Length - 1);
        q[0] = _m == 0 ? default : new QueuedMonster(_m, _ml, _mh);
    }

    /// <summary>Line 30200.</summary>
    private void ClearFollowers() => Array.Clear(State.Followers);

    // ---- experience levels (18000-18080) ---------------------------------------------------

    /// <summary>Applies at most one level gained or lost. Returns false if the hero died.</summary>
    private bool LevelCheck()
    {
        var h = Hero;
        if (h.Experience >= Character.ExperienceFor(h.Level + 1))
        {
            Clear();
            h.Level++;
            int gain = Int(Rnd() * h.Con + 1);
            h.Hp += gain;
            h.MaxHp += gain;
            long cap = Character.ExperienceFor(h.Level + 1);
            if (h.Experience > cap) h.Experience = cap - 1;
            h.SpellUnits += h.Level;
            h.MaxSpellUnits += h.Level;
            Say($"You went up a level! You are now level {h.Level}.", Tone.Good);
            Sound(SoundCue.LevelUp);
            BeatClear();
            Say($"You gain {gain} hit points.", Tone.Good);
            return true;
        }

        if (h.Level == 1 && h.Experience >= 0) return true;
        if (h.Experience >= Character.ExperienceFor(h.Level)) return true;

        h.SpellUnits -= h.Level;
        h.MaxSpellUnits -= h.Level;
        if (h.SpellUnits < 0) h.SpellUnits = 0;
        h.Level--;
        int loss = Int(Rnd() * h.Con + 1);
        h.Hp -= loss;
        h.MaxHp -= loss;
        Say("You go down a level!", Tone.Bad);
        Sound(SoundCue.LevelDown);
        BeatClear();
        Say($"You lose {loss} hit points.", Tone.Bad);
        if (h.Hp > 0 && h.Level > 0) return true;
        Beat();
        return false;
    }
}
