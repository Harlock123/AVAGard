using Telengard.Core.Content;
using Telengard.Core.Dungeon;
using Telengard.Core.Model;

namespace Telengard.Core.Engine;

public sealed partial class Game
{
    /// <summary>6000-6020: dispatch on the room's feature; stairs up come from the room above.</summary>
    private IEnumerable<Prompt> Features()
    {
        var f = Map.Features(State.X, State.Y, State.Z);
        _ownFeature = (int)f.Own;
        _aboveFeature = f.HasStairsUp ? 4 : 0;
        if (f.IsEmpty) { _next = Label.Ready; yield break; }
        if (f.HasStairsUp) { _next = Label.Stairs; yield break; }
        _next = f.Own switch
        {
            FeatureKind.InnStairsOrElevator when State.Z == 1 => Label.Stairs,
            FeatureKind.InnStairsOrElevator => Label.Elevator,
            FeatureKind.Pit => Label.Pit,
            FeatureKind.Teleporter => Label.Teleporter,
            FeatureKind.StairsDown => Label.Stairs,
            FeatureKind.Altar => Label.Altar,
            FeatureKind.Fountain => Label.Fountain,
            FeatureKind.GrayMistyCube => Label.Cube,
            FeatureKind.Throne => Label.Throne,
            _ => Label.Box,
        };
        if (f.Own == FeatureKind.InnStairsOrElevator && State.Z == 1) _aboveFeature = 4; // 6102
    }

    /// <summary>6500-6550: timeout = stay.</summary>
    private IEnumerable<Prompt> Stairs()
    {
        bool up = _aboveFeature == 4, down = _ownFeature == 4;
        while (true)
        {
            Clear();
            Sound(SoundCue.Stairs);
            Say("You have found a stairway.", Tone.Info);
            if (up && State.Z == 1) Say("You see light above.", Tone.Info);
            var options = new List<Command>();
            if (up) options.Add(Command.Up);
            if (down) options.Add(Command.Down);
            options.Add(Command.Stay);
            yield return Ask(PromptKind.Stairs, [.. options]) with { Detail = (up ? "U" : "") + (down ? "D" : "") };
            var c = In.IsTimeout ? Command.Stay : In.Command;
            if (c == Command.Up && up)
            {
                Echo("Up");
                State.Z--;
                if (State.Z > 0) { Moved(); _next = Label.Tick; }
                else _next = Label.Inn;
                yield break;
            }
            if (c == Command.Down && down)
            {
                Echo("Down");
                State.Z++;
                Moved();
                _next = Label.Tick;
                yield break;
            }
            if (c == Command.Stay)
            {
                Echo("Stay");
                Clear();
                _next = Label.Ready;
                yield break;
            }
            Echo("???");
        }
    }

    /// <summary>
    /// 6103-6145: the inn on the surface. Gold becomes experience, one level may be gained,
    /// followers are lost, HP and spell units are restored and every effect ends.
    /// </summary>
    private IEnumerable<Prompt> Inn()
    {
        Clear();
        Sound(SoundCue.Inn);
        Say($"You have found the {InnNames.For(State.X, State.Y)}.", Tone.Good);
        Say("They cash in your gold.");
        Hero.Experience += Hero.Gold;
        Hero.Banked += Hero.Gold;
        Hero.Gold = 0;
        LevelCheck(); // only experience gains are possible here
        ClearFollowers();
        Clear();
        Hero.Hp = Hero.MaxHp;
        Say($"You have {Hero.Banked} in the safe.");
        Array.Clear(Hero.Timers);
        Hero.SpellUnits = Hero.MaxSpellUnits;
        Say("You spend the night.");
        Beat();
        Say("You feel better.", Tone.Good);
        Clear();
        _events.Add(new MovedEvent(State.X, State.Y, 0));
        _next = Label.InnPrompt;
        yield break;
    }

    /// <summary>6125-6145: no deadline (a timeout simply re-prompts in the original).</summary>
    private IEnumerable<Prompt> InnPrompt()
    {
        while (true)
        {
            yield return Ask(PromptKind.Inn, false, false, Command.Confirm, Command.Save);
            if (Pressed(Command.Save)) { _next = Label.Save; yield break; }
            if (ReturnPressed)
            {
                Echo("Re-enter the dungeon");
                BeatClear();
                State.Z = 1;
                Moved();
                _next = Label.Tick;
                yield break;
            }
            Echo("???");
            BeatClear();
        }
    }

    /// <summary>6200-6235: "you feel heavy" — carried up one level automatically.</summary>
    private IEnumerable<Prompt> Elevator()
    {
        Say("You feel heavy for a moment...", Tone.Info);
        State.Z--;
        Moved();
        _next = Label.Tick;
        yield break;
    }

    /// <summary>6300-6345. On level 50 a pit behaves as an elevator.</summary>
    private IEnumerable<Prompt> Pit()
    {
        if (State.Z == DungeonMap.Depth) { _next = Label.Elevator; yield break; }
        Say("You see a pit.", Tone.Info);
        bool ask;
        if (Hero.TimerActive(Effect.Levitate))
        {
            Say("You are hovering above it.");
            ask = true;
        }
        else
        {
            ask = !(Int(Rnd() * 20) > Hero.Dex + Hero.Items[Items.ElvenBoots]);
        }

        if (ask)
        {
            yield return Ask(PromptKind.Descend, Command.Yes, Command.No);
            if (!Pressed(Command.Yes))
            {
                Echo("No");
                BeatClear();
                _next = Label.Ready;
                yield break;
            }
            Echo("Yes");
        }
        else
        {
            Say("You fall in!!", Tone.Danger);
            Sound(SoundCue.Fall);
            int d = Int(Rnd() * 3 * 6 + 1); // line 20000 with L=3
            Say($"You suffer {d} points of damage.", Tone.Bad);
            if (!Hurt(d))
            {
                Say("Your life has been terminated.", Tone.Danger);
                BeatClear();
                _next = Label.Death;
                yield break;
            }
            Beat();
        }
        State.Z++;
        Moved();
        _next = Label.Tick;
    }

    /// <summary>6400-6485: deterministic jump with a 20% chance to chain.</summary>
    private IEnumerable<Prompt> Teleporter()
    {
        Say("Zzap!! You've been teleported...", Tone.Info);
        Sound(SoundCue.Teleport);
        ClearFollowers();
        Teleport();
        Moved();
        _next = Label.Tick;
        yield break;
    }

    private void Teleport()
    {
        int x = State.X, y = State.Y, z = State.Z;
        do
        {
            if (((x + y) & 1) == 0)
            {
                z--;
                if (((x + y) & 2) == 2) z += 2;
            }
            x = x + z * 8 + y * 13;
            y = y + z * 6 + x * 17;
            while (x > 200) x -= 200;
            while (y > 200) y -= 200;
            if (z == 0) z = 1;
            if (z > 50) z = 50;
        }
        while (Rnd() > .8);
        State.X = x; State.Y = y; State.Z = z;
    }

    /// <summary>6600-6680.</summary>
    private IEnumerable<Prompt> Altar()
    {
        Clear();
        Say("You have found a holy altar.", Tone.Info);
        yield return Ask(PromptKind.Worship, Command.Confirm, Command.Other);
        if (ReturnPressed)
        {
            yield return Ask(PromptKind.Donate, Command.Confirm, Command.Other);
            if (ReturnPressed)
            {
                yield return Ask(PromptKind.DonationAmount, true, true, Command.Number);
                long amount = In.IsTimeout || In.Command != Command.Number ? 0 : In.Number;
                Clear();
                if (amount > Hero.Gold)
                {
                    Say("You don't have that much!", Tone.Bad);
                }
                else if (amount >= 50 * State.Z)
                {
                    Hero.Gold -= amount;
                    double share = (double)amount / (Hero.Gold + amount);
                    if (Rnd() < share)
                    {
                        int slot = Int(Rnd() * 7 + 1);
                        AddTimer(slot, Int(Rnd() * 100 * share + 1));
                        Say("You've been heard.", Tone.Good);
                        Say($"(You feel the blessing of {Effect.Name(slot)}.)", Tone.Info);
                    }
                    else
                    {
                        Say("Thank you for your donation.");
                    }
                    BeatClear();
                    _next = Label.Ready;
                    yield break;
                }
                foreach (var p in PaganTrash()) yield return p;
                yield break;
            }
        }

        // 6610: not worshipping
        if (Rnd() > .7) { _next = Label.Ready; yield break; }
        foreach (var p in PaganTrash()) yield return p;
    }

    /// <summary>6615-6625: an undead monster attacks.</summary>
    private IEnumerable<Prompt> PaganTrash()
    {
        Say("Dirty pagan trash!", Tone.Danger);
        do _m = Int(Rnd() * 20 + 1); while (!Monsters.IsUndead(_m));
        BeatClear();
        _next = Label.MonsterLevel;
        yield break;
    }

    private static readonly string[] FountainColours = ["", "white", "green", "clear", "red", "black"];

    /// <summary>6700-6786: darker water (higher C) is riskier.</summary>
    private IEnumerable<Prompt> Fountain()
    {
        int c = Int(Rnd() * 5 + 1);
        Clear();
        Say($"You have found a fountain with running {FountainColours[c]} water.", Tone.Info);
        yield return Ask(PromptKind.Drink, Command.Confirm, Command.Other) with { Detail = FountainColours[c] };
        Clear();
        _next = Label.Ready;
        if (!ReturnPressed) yield break;
        Sound(SoundCue.Drink);

        if (!(Rnd() > .6))
        {
            if (!(Rnd() > 1 - c * .15))
            {
                Say("You feel better.", Tone.Good);
                Beat();
                int heal = Int(Rnd() * 3 * State.Z + 1);
                Say($"You heal {heal} points.", Tone.Good);
                Heal(heal);
                BeatClear();
                yield break;
            }
            if (!(Rnd() > .15 * c))
            {
                Say("It's poison!!!", Tone.Danger);
                int harm = Int(Rnd() * 3 * State.Z + 1);
                Say($"You lose {harm} hit points.", Tone.Bad);
                if (!Hurt(harm)) { _next = Label.Death; yield break; }
                BeatClear();
                yield break;
            }
            if (!ExperienceSurge()) { _next = Label.Death; yield break; }
            BeatClear();
            yield break;
        }

        if (Rnd() > .4)
        {
            Say("You feel refreshed!");
            BeatClear();
            yield break;
        }
        if (Rnd() > .5)
        {
            Say("You feel refreshed!");
            BeatClear();
            Say("Actually, you're drunk!!", Tone.Bad);
            AddTimer(Effect.Drunk, Int(Rnd() * 16 + 1));
            Clear();
            yield break;
        }
        if (Rnd() > .5)
        {
            Say("You have been dispossessed!!", Tone.Danger);
            for (int i = 1; i <= Items.Count; i++) Hero.Items[i] = 0;
            Beat();
            BeatClear();
            yield break;
        }
        Hero.SpellUnits += Int(Rnd() * 4 * State.Z + 1);
        Say("Magical power surges through your body!", Tone.Good);
        Say($"You now have {Hero.SpellUnits} spell units.");
        BeatClear();
    }

    /// <summary>20100-20120: gain or lose up to 500·depth experience.</summary>
    private bool ExperienceSurge()
    {
        int amount = Int(Rnd() * 500 * State.Z + 1);
        if (Rnd() > .5)
        {
            Say($"You just lost {amount} experience points.", Tone.Bad);
            amount = -amount;
        }
        else
        {
            Say($"You just gained {amount} experience points.", Tone.Good);
        }
        Hero.Experience += amount;
        return LevelCheck();
    }

    /// <summary>6800-6840: walk in, then 20% random level, else choose 1-50 (timeout = random).</summary>
    private IEnumerable<Prompt> Cube()
    {
        Clear();
        Say("You see a large gray misty cube.", Tone.Info);
        yield return Ask(PromptKind.EnterCube, Command.Confirm, Command.Other);
        if (!ReturnPressed) { _next = Label.Ready; yield break; }
        while (true)
        {
            if (!(Rnd() > .2))
            {
                State.Z = Int(Rnd() * 50 + 1);
                break;
            }
            yield return Ask(PromptKind.CubeLevel, true, true, Command.Number);
            if (In.IsTimeout)
            {
                State.Z = Int(Rnd() * 50 + 1);
                break;
            }
            int c = In.Command == Command.Number ? In.Number : 0;
            if (c is >= 1 and <= 50)
            {
                State.Z = c;
                break;
            }
            Say("No, you fool!!", Tone.Bad);
            BeatClear();
        }
        Say("You float in space...", Tone.Info);
        Sound(SoundCue.Teleport);
        Beat();
        Moved();
        _next = Label.Tick;
    }

    /// <summary>6900-7035.</summary>
    private IEnumerable<Prompt> Throne()
    {
        while (true)
        {
            Clear();
            Say("You see a jewel-encrusted throne.", Tone.Info);
            Sound(SoundCue.Throne);
            yield return Ask(PromptKind.Throne, Command.Pry, Command.Sit, Command.Read, Command.Ignore);
            if (In.IsTimeout || In.Command == Command.Ignore)
            {
                Echo("Ignore");
                BeatClear();
                _next = Label.Ready;
                yield break;
            }

            switch (In.Command)
            {
                case Command.Pry:
                    Echo("Pry");
                    BeatClear();
                    if (Rnd() > .7) { _next = Label.ThroneKing; yield break; }
                    if (Rnd() > .4) { NothingHappens(); yield break; }
                    long jewels = LInt(Rnd() * 1000 * State.Z + 1);
                    Say("They pop into your greedy hands!!", Tone.Good);
                    Say($"They are worth {jewels} gold.", Tone.Good);
                    Hero.Gold += jewels;
                    BeatClear();
                    _next = Label.Ready;
                    yield break;

                case Command.Sit:
                    Echo("Sit");
                    BeatClear();
                    if (Rnd() > .7) { _next = Label.ThroneKing; yield break; }
                    if (Rnd() > .6) { NothingHappens(); yield break; }
                    if (Rnd() > .4) { _next = Label.Teleporter; yield break; }
                    Say("A loud gong sounds!", Tone.Info);
                    Sound(SoundCue.Gong);
                    BeatClear();
                    if (Rnd() < .5)
                    {
                        if (Hero.Level > State.Z) { NothingHappens(); yield break; }
                        Hero.Experience = Character.ExperienceFor(Hero.Level + 1);
                    }
                    else
                    {
                        Hero.Experience = LInt(Hero.Experience / 2.0);
                        if (Hero.Level == 1) Hero.Experience = -1;
                    }
                    _next = LevelCheck() ? Label.Ready : Label.Death;
                    yield break;

                case Command.Read:
                    Echo("Read");
                    BeatClear();
                    if (Rnd() > .7) { _next = Label.ThroneKing; yield break; }
                    if (Rnd() < Hero.Int * .05)
                    {
                        Say("A mysterious magic grips you...", Tone.Info);
                        ChangeRandomStat();
                    }
                    else
                    {
                        Say("You don't understand them...");
                    }
                    BeatClear();
                    _next = Label.Ready;
                    yield break;

                default:
                    Echo("???");
                    BeatClear();
                    break;
            }
        }
    }

    private void NothingHappens()
    {
        Say("Nothing happens...");
        BeatClear();
        _next = Label.Ready;
    }

    /// <summary>7030-7035: the throne's owner returns (level 5 to 5·depth+4).</summary>
    private IEnumerable<Prompt> ThroneKing()
    {
        _m = Int(Rnd() * 20 + 1);
        Say($"The {Monsters.Name(_m)} king returns!!", Tone.Danger);
        BeatClear();
        _undead = Monsters.IsUndead(_m);
        _ml = Int(Rnd() * State.Z * 5) + 5;
        _next = Label.ShowMonster;
        yield break;
    }

    /// <summary>20200-20240: a random attribute moves by 1 within 3..18.</summary>
    private void ChangeRandomStat()
    {
        while (true)
        {
            int i = Int(Rnd() * 6);
            if (Rnd() > .5)
            {
                if (Hero.Stats[i] == 3) continue;
                Hero.Stats[i]--;
                Say($"Your {Stat.Abbreviations[i]} goes down by 1.", Tone.Bad);
            }
            else
            {
                if (Hero.Stats[i] == 18) continue;
                Hero.Stats[i]++;
                Say($"Your {Stat.Abbreviations[i]} goes up by 1.", Tone.Good);
            }
            Beat();
            return;
        }
    }

    private static readonly Command[] BoxButtons = [Command.Red, Command.Yellow, Command.Green, Command.Blue];

    /// <summary>7100-7185: a per-character four-button combination.</summary>
    private IEnumerable<Prompt> Box()
    {
        Clear();
        Say("You see a small box with four coloured lights.", Tone.Info);
        yield return Ask(PromptKind.PushButtons, Command.Push, Command.Ignore);
        _next = Label.Ready;
        if (!Pressed(Command.Push))
        {
            Echo("Ignore");
            BeatClear();
            yield break;
        }
        Echo("Push");
        BeatClear();
        int position = 1;
        while (true)
        {
            yield return Ask(PromptKind.BoxButton, Command.Red, Command.Green, Command.Yellow, Command.Blue, Command.Stop)
                with { Detail = position.ToString() };
            if (In.IsTimeout || In.Command == Command.Stop)
            {
                Echo("Stop");
                BeatClear();
                yield break;
            }
            int colour = Array.IndexOf(BoxButtons, In.Command) + 1;
            if (colour == 0)
            {
                Echo("???");
                BeatClear();
                continue;
            }
            Echo(In.Command.ToString());
            if (Hero.BoxCombination[position] != colour)
            {
                int shock = Int(Rnd() * 2 * State.Z + 1);
                Say("An electric bolt shoots through you!!", Tone.Danger);
                Sound(SoundCue.Shock);
                BeatClear();
                Say($"You suffer {shock} points of damage.", Tone.Bad);
                if (!Hurt(shock)) { _next = Label.Death; yield break; }
                BeatClear();
                yield break;
            }
            position++;
            if (position < 5)
            {
                BeatClear();
                continue;
            }
            Say("It opens!!!", Tone.Good);
            BeatClear();
            CharacterCreation.NewBoxCombination(Hero, _rng);
            long jewels = LInt(Rnd() * 20000 * Math.Pow(State.Z, 2) + 1);
            Say($"Inside you find jewels worth {jewels} in gold!!", Tone.Good);
            Sound(SoundCue.Treasure);
            Hero.Gold += jewels;
            Clear();
            yield break;
        }
    }
}
