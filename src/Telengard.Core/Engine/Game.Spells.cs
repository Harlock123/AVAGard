using Telengard.Core.Content;
using Telengard.Core.Dungeon;
using Telengard.Core.Model;

namespace Telengard.Core.Engine;

public sealed partial class Game
{
    /// <summary>
    /// 21000-21050: choose a level, then a spell. Spell units (= the spell level) are spent once a
    /// spell is chosen, even if it is then wasted.
    /// </summary>
    private IEnumerable<Prompt> Cast()
    {
        Echo("Cast");
        yield return Ask(PromptKind.SpellLevel, true, true, Command.Number) with { Detail = Spells.MaxSpellLevel(Hero.Level).ToString() };
        int level = !In.IsTimeout && In.Command == Command.Number ? In.Number : 0;
        if (level == 0) { _next = Label.SpellCancel; yield break; }
        if (!(level > 0 && level <= Hero.Level / 3 + 1 && level < 7))
        {
            Say("You don't have that level of spells!", Tone.Bad);
            Beat();
            _next = Label.SpellCancel;
            yield break;
        }
        if (level > Hero.SpellUnits)
        {
            Say("You don't have enough spell units!", Tone.Bad);
            _next = Label.SpellEnd;
            yield break;
        }

        int index;
        while (true)
        {
            yield return Ask(PromptKind.SpellNumber, true, true, Command.Number, Command.ListSpells, Command.Confirm) with { Detail = level.ToString() };
            if (In.IsTimeout || In.Command == Command.Confirm) { _next = Label.SpellCancel; yield break; }
            if (In.Command == Command.Number && In.Number is >= 1 and <= 6) { index = In.Number; break; }

            // '=' or any other key lists the level's spells; a digit pressed afterwards casts it.
            yield return Ask(PromptKind.SpellList, true, true, Command.Number, Command.Other) with { Detail = level.ToString() };
            Clear();
            if (!In.IsTimeout && In.Command == Command.Number && In.Number is >= 1 and <= 6) { index = In.Number; break; }
        }

        Hero.SpellUnits -= level;
        var spell = Spells.Get(level, index);
        Say(spell.Name, Tone.Info);
        Sound(SoundCue.Spell);
        foreach (var p in CastSpell(spell)) yield return p;
    }

    /// <summary>21900: after a spell, the monster attacks if in melee.</summary>
    private IEnumerable<Prompt> SpellEnd()
    {
        BeatClear();
        _next = _melee ? Label.MonsterAttack : Label.Ready;
        yield break;
    }

    /// <summary>21910: cancelled before spending anything; back to the same prompt.</summary>
    private IEnumerable<Prompt> SpellCancel()
    {
        Clear();
        _next = _melee ? Label.CombatPrompt : Label.Ready;
        yield break;
    }

    /// <summary>21920: the monster leaves; no experience.</summary>
    private IEnumerable<Prompt> MonsterDriven()
    {
        BeatClear();
        HideMonster();
        _next = Label.TreasureRoll;
        yield break;
    }

    /// <summary>21700: spell damage.</summary>
    private void SpellDamage(int amount)
    {
        _mh -= amount;
        Say($"It suffers {amount} hit points.");
        _next = _mh > 0 ? Label.SpellEnd : Label.Killed;
    }

    /// <summary>21800: a combat spell cast outside melee.</summary>
    private void Wasted()
    {
        Say("You just wasted a combat spell!", Tone.Bad);
        _next = Label.SpellEnd;
    }

    /// <summary>21750: a dungeon-only spell attempted in melee; the monster gets a free attack.</summary>
    private void NotInMelee()
    {
        Say("Not in melee!!", Tone.Bad);
        BeatClear();
        _next = Label.MonsterAttack;
    }

    /// <summary>21930.</summary>
    private void AlreadyDead()
    {
        Say("Undead are already dead!!");
        _next = Label.SpellEnd;
    }

    /// <summary>22402: duration spells add 5..(4+range) turns.</summary>
    private void Duration(int slot, int range)
    {
        AddTimer(slot, Int(Rnd() * range + 5));
        _next = Label.SpellEnd;
    }

    /// <summary>
    /// Prompt to finish off a sleeping/webbed/held/charmed monster. Any other key flees (3710).
    /// </summary>
    private IEnumerable<Prompt> SlayHelpless(Func<bool> succeeds, string failure = "It broke free!!")
    {
        yield return Ask(PromptKind.SlayHelpless, Command.Confirm, Command.Other);
        if (!ReturnPressed)
        {
            Clear();
            _next = Label.EvadeSuccess;
            yield break;
        }
        if (succeeds()) { _next = Label.Killed; yield break; }
        Say(failure, Tone.Bad);
        Beat();
        _next = Label.MonsterAttack;
    }

    private IEnumerable<Prompt> CastSpell(SpellInfo spell)
    {
        // Combat spells cast outside melee are wasted; where the original prints flavour text
        // first (lightning, finger of death, wall of fire, prismatic wall) it is printed first here too.
        switch (spell.Number)
        {
            case 1: // Magic Missile (22100)
                BeatClear();
                if (!_melee) { Wasted(); break; }
                SpellDamage(Int(Rnd() * 8 + 5));
                break;

            case 2: // Sleep (22200)
                BeatClear();
                if (!_melee) { Wasted(); break; }
                if (_undead) { Say("Undead don't sleep!"); _next = Label.MonsterAttack; break; }
                if (Int(Rnd() * 20 + 1) > Hero.Int) { Say($"The {MonsterName} isn't sleepy!"); _next = Label.MonsterAttack; break; }
                Say($"The {MonsterName} is sleeping.", Tone.Good);
                foreach (var p in SlayHelpless(() => Rnd() > .2, "It woke up!!")) yield return p;
                break;

            case 3: // Cure Light Wounds (22300)
                Heal(Int(Rnd() * 8 + 1));
                Say("You feel better.", Tone.Good);
                _next = Label.SpellEnd;
                break;

            case 4: Duration(Effect.Light, 11); break;

            case 5: // Turn Undead (22500)
                BeatClear();
                if (!_melee) { Wasted(); break; }
                if (!_undead)
                {
                    Say($"The {MonsterName} is insulted at being called undead.");
                    _next = Label.SpellEnd;
                    break;
                }
                if (Rnd() < .05 * Hero.Wis + .05 * Hero.Level - .05 * _ml)
                {
                    _ml = Int(_ml / 2.0 + .5);
                    Say("It runs in fear!!", Tone.Good);
                    _next = Label.KilledExperience;
                    break;
                }
                Say($"The {MonsterName} listens with deaf ears.");
                _next = Label.SpellEnd;
                break;

            case 6: Duration(Effect.ProtectionFromEvil, 11); break;

            case 7: // Web (23100)
                BeatClear();
                if (!_melee) { Wasted(); break; }
                if (Int(Math.Pow(Rnd(), 2) * 20 + _ml) > Hero.Int) { Say($"The {MonsterName} dodges aside!"); _next = Label.MonsterAttack; break; }
                Say($"The {MonsterName} is webbed!", Tone.Good);
                foreach (var p in SlayHelpless(() => Rnd() - _ml / 20.0 > .2)) yield return p;
                break;

            case 8: Duration(Effect.Levitate, 21); break;

            case 9: // Cause Light Wounds (23300)
                BeatClear();
                if (!_melee) { Wasted(); break; }
                SpellDamage(Int(Rnd() * 12 + 3));
                break;

            case 10: Duration(Effect.DetectTraps, 21); break;

            case 11: // Charm (23500)
                Clear();
                if (!_melee) { Wasted(); break; }
                if (_undead) { Say("The undead ignore your wiles!"); _next = Label.MonsterAttack; break; }
                if (Int(Math.Pow(Rnd(), 2) * 20 + 1) > Hero.Chr) { Say($"The {MonsterName} resists you!"); _next = Label.MonsterAttack; break; }
                Say($"The {MonsterName} is charmed.", Tone.Good);
                foreach (var p in SlayHelpless(() => Rnd() - _ml / 20.0 > .2)) yield return p;
                break;

            case 12: Duration(Effect.Strength, 21); break;

            case 13: // Lightning Bolt (24100)
                Clear();
                Say("Zzzzaaaapp!!!", Tone.Info);
                if (!_melee) { Wasted(); break; }
                Beat();
                SpellDamage(Int(Rnd() * 6 * Hero.Level + 15));
                break;

            case 14: // Cure Serious Wounds (24200)
                Heal(Int(Rnd() * 24 + 1));
                Say("You feel better.", Tone.Good);
                _next = Label.SpellEnd;
                break;

            case 15: Duration(Effect.Light, 31); break;
            case 16: Duration(Effect.Invisibility, 21); break;

            case 17: // Hold Monster (24500)
                BeatClear();
                if (!_melee) { Wasted(); break; }
                if (Int(Rnd() * 20 + _ml) > Hero.Int) { Say($"The {MonsterName} ignores you!"); _next = Label.MonsterAttack; break; }
                Say($"The {MonsterName} is held!", Tone.Good);
                foreach (var p in SlayHelpless(() => Rnd() > .2 + _ml * .03)) yield return p;
                break;

            case 18: // Phantasmal Forces (24600)
                BeatClear();
                if (!_melee) { Wasted(); break; }
                if (Int(Rnd() * 22 + _ml) < Hero.Int) { Say("It believes!... Arrgh..."); _next = Label.Killed; break; }
                Say($"The {MonsterName} doesn't believe!");
                _next = Label.MonsterAttack;
                break;

            case 19: // Pass Wall (25100)
                BeatClear();
                if (_melee) { NotInMelee(); break; }
                while (true)
                {
                    yield return Ask(PromptKind.PassWallDirection, Command.North, Command.South, Command.West, Command.East);
                    if (In.IsTimeout) { _next = Label.SpellEnd; break; }
                    Direction? d = In.Command switch
                    {
                        Command.North => Direction.North, Command.South => Direction.South,
                        Command.West => Direction.West, Command.East => Direction.East, _ => null,
                    };
                    if (d is null) { Echo("???"); Clear(); continue; }
                    bool room = d switch
                    {
                        Direction.North => State.Y > 1, Direction.South => State.Y < DungeonMap.Size,
                        Direction.West => State.X > 1, _ => State.X < DungeonMap.Size,
                    };
                    if (!room) { Say("Only stone there..."); _next = Label.SpellEnd; break; }
                    ClearFollowers();
                    Say("Poof!", Tone.Info);
                    StepTo(d.Value);
                    break;
                }
                break;

            case 20: // Fireball (25200)
                Clear();
                Say("Whooooshh!!!", Tone.Info);
                if (!_melee) { Wasted(); break; }
                Say($"The {MonsterName} is burning!");
                SpellDamage(Int(Rnd() * 12 * Hero.Level + 15));
                break;

            case 21: // Cause Serious Wounds (25300)
                BeatClear();
                if (!_melee) { Wasted(); break; }
                SpellDamage(Int(Rnd() * 32 + 10));
                break;

            case 22: // Flesh to Stone (25400)
                BeatClear();
                if (!_melee) { Wasted(); break; }
                if (Rnd() > .6) { Say($"The {MonsterName} isn't affected."); _next = Label.MonsterAttack; break; }
                Say("One stone statue...", Tone.Good);
                Beat();
                _next = Label.Killed;
                break;

            case 23: Duration(Effect.Fear, 30); break;

            case 24: // Finger of Death (25600)
                BeatClear();
                Say("Die!!!!!!", Tone.Info);
                if (!_melee) { Wasted(); break; }
                if (_undead) { AlreadyDead(); break; }
                Beat();
                if (Rnd() > .3 + _ml * .04 - Hero.Level * .03) { _next = Label.Killed; break; }
                Say($"The {MonsterName} laughs!");
                BeatClear();
                _next = Label.MonsterAttack;
                break;

            case 25: // Teleport (26100)
                BeatClear();
                if (_melee) { NotInMelee(); break; }
                while (true)
                {
                    yield return Ask(PromptKind.TeleportNorth, true, true, Command.Number);
                    int ny = NumberOrZero();
                    yield return Ask(PromptKind.TeleportEast, true, true, Command.Number);
                    int nx = NumberOrZero();
                    yield return Ask(PromptKind.TeleportUp, true, true, Command.Number);
                    int nz = NumberOrZero();
                    double distance = Math.Sqrt(Math.Pow(nx, 2) + Math.Pow(ny, 2) + Math.Pow(nz * 5, 2)) - .1;
                    if (distance > Hero.Level * 5)
                    {
                        Say("Too far... try again.", Tone.Bad);
                        BeatClear();
                        continue;
                    }
                    int tx = State.X + nx, ty = State.Y - ny, tz = State.Z - nz;
                    if (!(tx > 0 && tx < 201 && ty > 0 && ty < 201 && tz < 51))
                    {
                        Say("Only stone there... the spell fails.", Tone.Bad);
                        _next = Label.SpellEnd;
                        break;
                    }
                    if (tz < 1)
                    {
                        Say("Only thin air... the spell fails.", Tone.Bad);
                        _next = Label.SpellEnd;
                        break;
                    }
                    State.X = tx; State.Y = ty; State.Z = tz;
                    Say("Poof!", Tone.Info);
                    Sound(SoundCue.Teleport);
                    ClearFollowers();
                    Moved();
                    _next = Label.Tick;
                    break;
                }
                break;

            case 26: Duration(Effect.AstralWalk, 16); break;

            case 27: // Power Word Kill (26300)
                BeatClear();
                if (!_melee) { Wasted(); break; }
                Say("Qwerty!!!!", Tone.Info);
                Beat();
                if (!_undead && Rnd() < .8) { _next = Label.Killed; break; }
                if (_undead) { AlreadyDead(); break; }
                Say($"The {MonsterName} doesn't hear...");
                _next = Label.SpellEnd;
                break;

            case 28: // Ice Storm (26400)
                BeatClear();
                if (!_melee) { Wasted(); break; }
                Say("Brrrr!!!!", Tone.Info);
                SpellDamage(60);
                break;

            case 29: // Wall of Fire (26500)
                BeatClear();
                Say("Wwhhoooosshhh!!!", Tone.Info);
                Say("A wall of fire appears about you.");
                Beat();
                BeatClear();
                if (!_melee) { Wasted(); break; }
                if (Rnd() > .4) { Say($"The {MonsterName} is gone."); _next = Label.MonsterDriven; break; }
                Say($"The {MonsterName} walks through!!!");
                SpellDamage(Int(Rnd() * 12 + 8));
                break;

            case 30: // Plague (26600)
                BeatClear();
                if (!_melee) { Wasted(); break; }
                if (_undead) { AlreadyDead(); break; }
                Say($"Black death for the {MonsterName}!");
                BeatClear();
                if (Rnd() > Hero.Wis * .05) { Say("It is immune!"); _next = Label.SpellEnd; break; }
                if (Rnd() * 2 < Hero.Con * .03 + 1) { _next = Label.Killed; break; }
                Say("The spell backfires!!", Tone.Danger);
                BeatClear();
                _next = Label.Death;
                break;

            case 31: // Time Stop (27100)
                BeatClear();
                Say("Time is frozen; monsters cannot attack you.", Tone.Info);
                AddTimer(Effect.TimeStop, Fnr(20) + 4);
                _next = _melee ? Label.TreasureRoll : Label.SpellEnd;
                break;

            case 32: Duration(Effect.RaiseDead, 40); break;

            case 33: // Holy Symbol (27300)
                BeatClear();
                if (!_melee) { Wasted(); break; }
                Beat();
                if (Rnd() < .9) { _next = Label.Killed; break; }
                Say($"The {MonsterName} doesn't see...");
                _next = Label.SpellEnd;
                break;

            case 34: // Word of Recall (27400)
                BeatClear();
                if (_melee) { NotInMelee(); break; }
                Array.Clear(Hero.Items);
                Hero.Gold = 0;
                ClearFollowers();
                State.X = DungeonMap.StartX; State.Y = DungeonMap.StartY; State.Z = DungeonMap.StartZ;
                Say("***ZAP!!***", Tone.Info);
                Sound(SoundCue.Teleport);
                Beat();
                Moved();
                _next = Label.Tick;
                break;

            case 35: // Restoration (27500)
                BeatClear();
                Say("You feel better!", Tone.Good);
                Hero.Hp = Hero.MaxHp;
                _next = Label.SpellEnd;
                break;

            case 36: // Prismatic Wall (27600)
                BeatClear();
                Say("A shifting multicoloured wall appears.", Tone.Info);
                if (!_melee) { Wasted(); break; }
                Say($"The {MonsterName} is gone.");
                _next = Label.MonsterDriven;
                break;
        }
    }

    private int NumberOrZero() => !In.IsTimeout && In.Command == Command.Number ? In.Number : 0;
}
