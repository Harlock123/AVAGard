using Telengard.Core.Content;
using Telengard.Core.Dungeon;
using Telengard.Core.Model;

namespace Telengard.Core.Engine;

public sealed partial class Game
{
    /// <summary>2000-2020: effect timers tick down, the ring of regeneration heals.</summary>
    private IEnumerable<Prompt> Tick()
    {
        State.TurnCount++;
        for (int i = 1; i <= Effect.Count; i++) Hero.Timers[i]--;
        _l = State.Z;
        Clear();
        int regen = Hero.Items[Items.RingOfRegeneration];
        if (regen != 0 && Hero.Hp != Hero.MaxHp)
        {
            Hero.Hp += regen;
            if (Hero.Hp > Hero.MaxHp) Hero.Hp = Hero.MaxHp;
        }
        _next = Label.EncounterRoll;
        yield break;
    }

    /// <summary>3000-3001: 30% chance of a monster; invisibility hides you from 80% of them.</summary>
    private IEnumerable<Prompt> EncounterRoll()
    {
        if (Rnd() > .3) { _next = Label.TreasureRoll; yield break; }
        if (Hero.TimerActive(Effect.Invisibility) && Rnd() > .2) { _next = Label.TreasureRoll; yield break; }
        _next = Label.PickMonster;
    }

    /// <summary>3005-3006: uniform type 1-20 (Fear excludes 1-4); the elven cloak may let you go unnoticed.</summary>
    private IEnumerable<Prompt> PickMonster()
    {
        do _m = Int(Rnd() * 20 + 1);
        while (Hero.TimerActive(Effect.Fear) && _m < 5);
        _next = Int(Rnd() * 20 + 1) <= Hero.Items[Items.ElvenCloak] ? Label.NotNoticed : Label.MonsterLevel;
        yield break;
    }

    /// <summary>20900-20915.</summary>
    private IEnumerable<Prompt> NotNoticed()
    {
        Say("You have not been noticed...", Tone.Info);
        yield return Ask(PromptKind.Approach, Command.Confirm, Command.Other);
        Clear();
        _next = ReturnPressed ? Label.MonsterLevel : Label.TreasureRoll;
    }

    /// <summary>3007-3008: level skewed low, up to 2·depth+2; Light sometimes turns undead away.</summary>
    private IEnumerable<Prompt> MonsterLevel()
    {
        _ml = Int(Math.Pow(Rnd(), 1.5) * (State.Z * 2 + 2) + 1);
        _undead = Monsters.IsUndead(_m);
        if (Hero.TimerActive(Effect.Light) && _undead && Rnd() > .8) { _next = Label.PickMonster; yield break; }
        _next = Label.ShowMonster;
    }

    /// <summary>3010-3080: announce the monster, then special friendly behaviour.</summary>
    private IEnumerable<Prompt> ShowMonster()
    {
        Clear();
        ShowMonsterSprite();
        Sound(SoundCue.Encounter);
        Say($"You have encountered a level {_ml} {MonsterName}.", Tone.Danger);
        if (Hero.TimerActive(Effect.TimeStop) && _m < 16 && _m != Monsters.Dwarf)
        {
            Say($"Frozen in time, the {MonsterName} cannot reach you.", Tone.Info);
            BeatClear();
            _next = Label.TreasureRoll;
            yield break;
        }

        _mh = Int(Math.Pow(Rnd(), .5) * _ml * _m + 1);
        _l = _ml;

        // 3017: 5% of all encounters trigger one of the three specials regardless of type.
        if (Rnd() > .95)
        {
            int which = Int(Rnd() * 3 + 1);
            foreach (var p in which switch { 1 => ElfHeals(), 2 => HobbitSteals(), _ => DragonGift() }) yield return p;
            yield break;
        }

        if (_m == Monsters.Elf && Rnd() <= .04 * Hero.Chr)
        {
            foreach (var p in ElfHeals()) yield return p;
            yield break;
        }

        if (_m == Monsters.Hobbit && !(Rnd() < .05 * Hero.Chr))
        {
            foreach (var p in HobbitSteals()) yield return p;
            yield break;
        }

        if (_m == Monsters.Dragon && !(Rnd() * 30 > Hero.Chr))
        {
            foreach (var p in DragonGift()) yield return p;
            yield break;
        }

        _next = Label.Initiative;
    }

    /// <summary>3030-3037.</summary>
    private IEnumerable<Prompt> ElfHeals()
    {
        Say($"The {MonsterName} takes a liking to you", Tone.Good);
        Say("and heals you to full strength.", Tone.Good);
        Hero.Hp = Hero.MaxHp;
        EndFriendly();
        yield break;
    }

    /// <summary>3045-3060: an item is stolen (whole item for slots 1-7, one unit for 8-10).</summary>
    private IEnumerable<Prompt> HobbitSteals()
    {
        Say($"The {MonsterName} makes a quick move...", Tone.Bad);
        Beat();
        bool hasAny = false;
        for (int i = 1; i <= Items.Count; i++) if (Hero.Items[i] > 0) hasAny = true;
        if (!hasAny)
        {
            Say("You have nothing worth stealing!");
        }
        else
        {
            int slot;
            do slot = Int(Rnd() * 10 + 1); while (Hero.Items[slot] < 1);
            if (slot < 8)
            {
                Say($"It steals your {Items.Name(slot)}!", Tone.Bad);
                Hero.Items[slot] = 0;
            }
            else
            {
                Say($"It steals a {Items.Name(slot)}!", Tone.Bad);
                Hero.Items[slot]--;
            }
        }
        EndFriendly();
        yield break;
    }

    /// <summary>3072-3080: a gift that raises an item, but only if the item is below the monster's level.</summary>
    private IEnumerable<Prompt> DragonGift()
    {
        int slot = Int(Rnd() * 7 + 1);
        if (Hero.Items[slot] >= _ml) { _next = Label.Initiative; yield break; }
        int c = Int(Rnd() * (_ml - Hero.Items[slot]) + 1);
        Hero.Items[slot] += c;
        Say($"The {MonsterName} likes you!", Tone.Good);
        BeatClear();
        Say($"It gives you a {Items.Name(slot)} +{Hero.Items[slot]}.", Tone.Good);
        Sound(SoundCue.Treasure);
        BeatClear();
        HideMonster();
        _next = Label.TreasureRoll;
    }

    private void EndFriendly()
    {
        Beat();
        HideMonster();
        _next = Label.TreasureRoll;
    }

    /// <summary>3090: the monster strikes first unless you are quick (DEX).</summary>
    private IEnumerable<Prompt> Initiative()
    {
        _next = Rnd() > .5 + Hero.Dex * .02 ? Label.MonsterAttack : Label.CombatPrompt;
        yield break;
    }

    /// <summary>3100-3115. Timeout = WAIT: the monster attacks.</summary>
    private IEnumerable<Prompt> CombatPrompt()
    {
        yield return Ask(PromptKind.Combat, Command.Fight, Command.Cast, Command.Evade);
        if (In.IsTimeout)
        {
            Echo("Wait");
            _next = Label.MonsterAttack;
            yield break;
        }
        switch (In.Command)
        {
            case Command.Fight: _next = Label.PlayerFight; break;
            case Command.Cast: _melee = true; _next = Label.Cast; break;
            case Command.Evade: _next = Label.Evade; break;
            default:
                Say($"The {MonsterName} is not amused.");
                BeatClear();
                _next = Label.CombatPrompt;
                break;
        }
    }

    /// <summary>3200-3210.</summary>
    private IEnumerable<Prompt> PlayerFight()
    {
        Echo("Fight");
        double toHit = Int(Rnd() * 20) + Hero.Level + Hero.Items[Items.Sword] + Hero.Str / 2.0;
        if (Hero.TimerActive(Effect.Strength)) toHit += 4;
        if (toHit < 10)
        {
            Say("You missed...");
            Sound(SoundCue.Miss);
            _next = Label.MonsterAttack;
            yield break;
        }
        int damage = Int(Rnd() * 8 + Rnd() * Hero.Level * 2 + Hero.Items[Items.Sword] + 1);
        if (Hero.TimerActive(Effect.Strength)) damage += 5;
        Say($"You do {damage} points of damage.");
        Sound(SoundCue.Hit);
        _mh -= damage;
        _next = _mh > 0 ? Label.MonsterAttack : Label.Killed;
    }

    /// <summary>3220.</summary>
    private IEnumerable<Prompt> Killed()
    {
        Say("It died...", Tone.Good);
        Sound(SoundCue.Kill);
        _next = Label.KilledExperience;
        yield break;
    }

    /// <summary>3223-3230: experience = monster level × type × 10.</summary>
    private IEnumerable<Prompt> KilledExperience()
    {
        long e = (long)_ml * _m * 10;
        Hero.Experience += e;
        Beat();
        HideMonster();
        Clear();
        Say($"You gain {e} experience points.", Tone.Good);
        if (!LevelCheck()) { _next = Label.Death; yield break; }
        BeatClear();
        _next = Label.AfterKill;
    }

    /// <summary>3900: half the time a kill leads straight to treasure.</summary>
    private IEnumerable<Prompt> AfterKill()
    {
        _next = Rnd() > .5 ? Label.TreasureFound : Label.TreasureRoll;
        yield break;
    }

    /// <summary>3300-3398: the monster attacks.</summary>
    private IEnumerable<Prompt> MonsterAttack()
    {
        double damageMultiplier = 1, paralysis = 0, drain = 0;
        int bonus = 0;
        switch (_m)
        {
            case Monsters.Wraith: drain = .1; break;
            case Monsters.Specter: drain = .2; break;
            case Monsters.Vampire: drain = .3; paralysis = .3; break;
            case Monsters.Ghoul: paralysis = .5; break;
            case Monsters.Demon:
                BeatClear();
                if (Rnd() > .6) { Say("It uses its sword!!!", Tone.Danger); bonus = 4; damageMultiplier = 3; }
                else { Say("It uses its whip!!", Tone.Danger); bonus = 2; damageMultiplier = 2; }
                break;
            case Monsters.Dragon:
                if (Rnd() > .3) { bonus = 5; damageMultiplier = 2; break; }
                Say("The dragon breathes fire!!!", Tone.Danger);
                int burn = Int(Rnd() * 20 * _ml + 1);
                // Line 3394 as written: a high CON makes it *less* likely that you partially dodge.
                if (!(Rnd() < .05 * Hero.Con))
                {
                    Clear();
                    Say("You partially dodge it.");
                    burn = Int(burn / 2.0 + .5);
                }
                Say($"You burn for {burn} points of damage.", Tone.Bad);
                Beat();
                foreach (var p in AfterMonsterDamage(burn, 0, 0)) yield return p;
                yield break;
        }

        // 3305-3310
        int toHit = Int(Rnd() * 20) + _ml - Hero.Items[Items.Armor] - Hero.Items[Items.Shield] + bonus;
        if (_m is > 16 and < 20 && Hero.TimerActive(Effect.ProtectionFromEvil)) toHit -= 6;
        if (toHit < 10)
        {
            Say("It missed...");
            Sound(SoundCue.Miss);
            BeatClear();
            _next = Label.CombatPrompt;
            yield break;
        }

        int damage = Int((Rnd() * 8 + Rnd() * _ml * 2 + 1) * damageMultiplier);
        Say($"It does {damage} points of damage.", Tone.Bad);
        bool paralysed = false;
        foreach (var p in AfterMonsterDamage(damage, drain, paralysis, () => paralysed = true)) yield return p;
        if (paralysed) _next = Label.MonsterAttack; // 3327 → 3300: the attack modifiers are re-rolled
    }

    /// <summary>3320-3327: apply damage, then level drain or paralysis.</summary>
    private IEnumerable<Prompt> AfterMonsterDamage(int damage, double drain, double paralysis, Action? attackAgain = null)
    {
        if (!Hurt(damage)) { _next = Label.Death; yield break; }
        Beat();
        if (!(Rnd() > drain || Hero.TimerActive(Effect.ProtectionFromEvil)))
        {
            Clear();
            Say("It drains a level!!!", Tone.Danger);
            Hero.Experience = LInt(Hero.Experience / 2.0);
            if (Hero.Level == 1) Hero.Experience = -1;
            if (!LevelCheck()) { _next = Label.Death; yield break; }
            BeatClear();
            _next = Label.CombatPrompt;
            yield break;
        }
        if (Rnd() > paralysis)
        {
            BeatClear();
            _next = Label.CombatPrompt;
            yield break;
        }
        Clear();
        Say("You're paralyzed!!!", Tone.Danger);
        BeatClear();
        attackAgain?.Invoke();
    }

    /// <summary>3700-3707: d18 must be below DEX + elven boots.</summary>
    private IEnumerable<Prompt> Evade()
    {
        int q = Int(Rnd() * 18 + 1);
        Echo("Evade");
        if (q < Hero.Dex + Hero.Items[Items.ElvenBoots])
        {
            _next = Label.EvadeSuccess;
            yield break;
        }
        Say("You're rooted to the spot!", Tone.Bad);
        BeatClear();
        _next = Label.MonsterAttack;
    }

    /// <summary>
    /// 3710-3795: flee through a random non-solid side, trying N, S, W, E cyclically from a random
    /// start. Astral Walk picks any direction. The monster then follows (see <see cref="PushFollower"/>).
    /// </summary>
    private IEnumerable<Prompt> EvadeSuccess()
    {
        if (Hero.TimerActive(Effect.AstralWalk))
        {
            _moveDirection = (Direction)Fnr(4);
        }
        else
        {
            int start = Fnr(4);
            Direction? found = null;
            for (int k = 0; k < 4 && found is null; k++)
            {
                var d = (Direction)((start - 1 + k) % 4 + 1);
                if (SideBits(d) < 3) found = d;
            }
            if (found is null)
            {
                Say("You're rooted to the spot!", Tone.Bad);
                BeatClear();
                _next = Label.MonsterAttack;
                yield break;
            }
            _moveDirection = found.Value;
        }
        HideMonster();
        Clear();
        _next = Label.Move;
    }

    private int SideBits(Direction d) => d switch
    {
        Direction.North => Map.NorthBits(State.X, State.Y, State.Z),
        Direction.South => Map.SouthBits(State.X, State.Y, State.Z),
        Direction.West => Map.WestBits(State.X, State.Y, State.Z),
        _ => Map.EastBits(State.X, State.Y, State.Z),
    };
}
