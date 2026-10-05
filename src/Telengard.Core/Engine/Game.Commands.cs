using Telengard.Core.Content;
using Telengard.Core.Dungeon;
using Telengard.Core.Model;

namespace Telengard.Core.Engine;

public sealed partial class Game
{
    /// <summary>
    /// 8000-8120: the command prompt. Timeout = STAY. Invalid keys and blocked moves do not use a turn.
    /// </summary>
    private IEnumerable<Prompt> Ready()
    {
        _m = 0;
        HideMonster();
        while (true)
        {
            yield return Ask(PromptKind.Ready,
                Command.North, Command.South, Command.West, Command.East, Command.Stay, Command.Help, Command.Quit,
                Command.Replot, Command.Cast, Command.Save, Command.UseScrollOfRescue, Command.DrinkHealingPotion,
                Command.DrinkStrengthPotion);
            var c = In.IsTimeout ? Command.Stay : In.Command;

            // 8118: while drunk, movement and STAY are replaced by a random choice among them.
            if (Hero.TimerActive(Effect.Drunk) && c is Command.North or Command.South or Command.West or Command.East or Command.Stay)
            {
                Say("You're confused ->", Tone.Bad);
                c = Fnr(5) switch { 1 => Command.North, 2 => Command.South, 3 => Command.West, 4 => Command.East, _ => Command.Stay };
            }

            switch (c)
            {
                case Command.North: _moveDirection = Direction.North; _next = Label.Move; yield break;
                case Command.South: _moveDirection = Direction.South; _next = Label.Move; yield break;
                case Command.West: _moveDirection = Direction.West; _next = Label.Move; yield break;
                case Command.East: _moveDirection = Direction.East; _next = Label.Move; yield break;

                case Command.Stay:
                    // 8350: a following monster catches up, otherwise a turn passes.
                    Echo("Stay");
                    PopFollower();
                    _next = _m != 0 ? Label.ShowMonster : Label.Tick;
                    yield break;

                case Command.Help:
                    yield return Ask(PromptKind.Help, Command.Other);
                    Clear();
                    continue;

                case Command.Replot:
                    continue;

                case Command.Quit:
                    Echo("Quit");
                    Say("Are you sure?", Tone.Danger);
                    yield return Ask(PromptKind.QuitConfirm, Command.Yes, Command.No);
                    if (!Pressed(Command.Yes)) { Echo("No"); BeatClear(); continue; }
                    Echo("Yes");
                    Say("Goodbye, cruel world!!", Tone.Danger);
                    Beat();
                    _next = Label.Death;
                    yield break;

                case Command.Cast:
                    _melee = false;
                    _next = Label.Cast;
                    yield break;

                case Command.Save:
                    _next = Label.Save;
                    yield break;

                case Command.UseScrollOfRescue:
                    // 8910-8918: back to the first stairway, dropping all carried gold.
                    Echo("Read scroll of rescue");
                    if (Hero.Items[Items.ScrollOfRescue] < 1) { NoneLeft(); continue; }
                    Hero.Items[Items.ScrollOfRescue]--;
                    State.X = DungeonMap.StartX; State.Y = DungeonMap.StartY; State.Z = DungeonMap.StartZ;
                    Say("***ZAP!!***", Tone.Info);
                    Sound(SoundCue.Teleport);
                    Beat();
                    ClearFollowers();
                    Hero.Gold = 0;
                    Moved();
                    _next = Label.Tick;
                    yield break;

                case Command.DrinkHealingPotion:
                    Echo("Drink healing potion");
                    if (Hero.Items[Items.PotionOfHealing] < 1) { NoneLeft(); continue; }
                    Hero.Items[Items.PotionOfHealing]--;
                    Heal(Fnr(20));
                    Say("You feel better!", Tone.Good);
                    BeatClear();
                    continue;

                case Command.DrinkStrengthPotion:
                    Echo("Drink strength potion");
                    if (Hero.Items[Items.PotionOfStrength] < 1) { NoneLeft(); continue; }
                    AddTimer(Effect.Strength, 10 + Int(Rnd() * 20));
                    Say("Strength flows through your body!", Tone.Good);
                    Hero.Items[Items.PotionOfStrength]--;
                    BeatClear();
                    continue;

                default:
                    Say("No.", Tone.Bad);
                    Sound(SoundCue.Bump);
                    continue;
            }
        }
    }

    private void NoneLeft()
    {
        Say("You don't have one!!", Tone.Bad);
        BeatClear();
    }

    /// <summary>
    /// 8150-8305: solid walls (3) block unless Astral Walk is active; the map edge always blocks.
    /// A blocked move returns to the prompt (line 8115 → 8000, which also forgets an evaded monster).
    /// </summary>
    private IEnumerable<Prompt> Move()
    {
        var d = _moveDirection;
        bool astral = Hero.TimerActive(Effect.AstralWalk);
        bool edge = d switch
        {
            Direction.North => State.Y == 1,
            Direction.South => State.Y == DungeonMap.Size,
            Direction.West => State.X == 1,
            _ => State.X == DungeonMap.Size,
        };
        if ((SideBits(d) > 2 && !astral) || edge)
        {
            Say("No.", Tone.Bad);
            Sound(SoundCue.Bump);
            _next = Label.Ready;
            yield break;
        }
        StepTo(d);
    }

    /// <summary>Moves one room, pushes the current monster (if any) onto the follower queue.</summary>
    private void StepTo(Direction d)
    {
        Echo(d.ToString());
        switch (d)
        {
            case Direction.North: State.Y--; break;
            case Direction.South: State.Y++; break;
            case Direction.West: State.X--; break;
            default: State.X++; break;
        }
        PushFollower();
        Sound(SoundCue.Step);
        Moved();
        _next = Label.Tick;
    }

    /// <summary>
    /// 8600-8675: store the character. Classic returns to the title afterwards, as V4.18 does.
    /// The demonstration character cannot be stored.
    /// </summary>
    private IEnumerable<Prompt> SaveGame()
    {
        bool atInn = State.Z == 0;
        Echo($"Store {Hero.Name}");
        if (Hero.IsDemo)
        {
            Say("You cannot store the demonstration character!", Tone.Bad);
            BeatClear();
            _next = atInn ? Label.InnPrompt : Label.Ready;
            yield break;
        }

        _events.Add(new SaveRequestedEvent(State.Clone()));
        yield return Ask(PromptKind.Saving, false, false, Command.SaveSucceeded, Command.SaveFailed);
        if (In.Command == Command.SaveSucceeded)
        {
            Say($"{Hero.Name} stored.", Tone.Good);
            if (Policy.SaveEndsSession)
            {
                Outcome = GameOutcome.SavedAndExited;
                _events.Add(new SessionEndedEvent(Outcome));
                _next = Label.End;
                yield break;
            }
        }
        else
        {
            Say("The character could not be stored.", Tone.Danger);
        }
        BeatClear();
        _next = atInn ? Label.InnPrompt : Label.Ready;
    }

    /// <summary>9000-9020, with Raise Dead (27205-27220).</summary>
    private IEnumerable<Prompt> Death()
    {
        HideMonster();
        Clear();
        Say("You died!!", Tone.Danger);
        Sound(SoundCue.Death);
        BeatClear();
        if (Hero.TimerActive(Effect.RaiseDead))
        {
            Say(Spells.Get(6, 2).Name + "...", Tone.Info);
            BeatClear();
            Hero.Stats[Stat.Con]--;
            Hero.Timers[Effect.RaiseDead] = 0;
            if (!(Rnd() > Hero.Con * .06))
            {
                Say("It works!!", Tone.Good);
                Hero.Hp = Hero.MaxHp;
                _melee = false;
                BeatClear();
                _next = Label.Ready;
                yield break;
            }
            Say("It doesn't work!", Tone.Danger);
            _next = Label.Death;
            yield break;
        }

        bool mighty = Hero.Level >= 4;
        Say(mighty ? "Another mighty adventurer bites the dust." : "Another not-so-mighty adventurer bites the dust.", Tone.Danger);
        Outcome = GameOutcome.Died;
        _events.Add(new DiedEvent(mighty));
        _events.Add(new SessionEndedEvent(Outcome));
        _next = Label.End;
    }
}
