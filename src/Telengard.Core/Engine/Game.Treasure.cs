using Telengard.Core.Content;
using Telengard.Core.Model;

namespace Telengard.Core.Engine;

public sealed partial class Game
{
    /// <summary>4000: 20% chance of treasure after the encounter phase.</summary>
    private IEnumerable<Prompt> TreasureRoll()
    {
        HideMonster();
        _next = Rnd() > .2 ? Label.Features : Label.TreasureFound;
        yield break;
    }

    /// <summary>4002-4155: 15% trapped; 30% valuables, else 50/50 an item or a chest.</summary>
    private IEnumerable<Prompt> TreasureFound()
    {
        _trapped = Rnd() > .85;
        _chain = false;
        if (Rnd() > .7)
        {
            if (Rnd() > .5) { _next = Label.Item; yield break; }
            foreach (var p in Chest()) yield return p;
            yield break;
        }

        _coinKind = Int(Rnd() * 5);
        Say($"You see some {Valuables.Name(_coinKind)}.", Tone.Info);
        Sound(SoundCue.Treasure);
        DetectTrapWarning();
        yield return Ask(PromptKind.TakeValuables, Command.Confirm, Command.Other);
        if (!ReturnPressed)
        {
            Echo("Leave it");
            BeatClear();
            _next = Label.Features;
            yield break;
        }
        Echo("Take it");
        if (_trapped)
        {
            int q = Int(Rnd() * 3 * _l + 1);
            Clear();
            Say("It's trapped!", Tone.Danger);
            Sound(SoundCue.Trap);
            Say($"You suffer {q} points of damage.", Tone.Bad);
            if (!Hurt(q)) { Beat(); _next = Label.Death; yield break; }
        }
        long worth = LInt(Rnd() * _coinKind * _l * 200 + 1);
        Say($"It's worth {worth} gold.", Tone.Good);
        Hero.Gold += worth;
        if (_coinKind == 0 && Rnd() > .8)
        {
            Clear();
            _next = Label.Item;
            yield break;
        }
        _next = Label.AfterTreasure;
    }

    private void DetectTrapWarning()
    {
        if (_trapped && Hero.TimerActive(Effect.DetectTraps) && Rnd() > .1)
            Say("You detect a trap!", Tone.Danger);
    }

    /// <summary>4102-4155.</summary>
    private IEnumerable<Prompt> Chest()
    {
        Say("You have found a treasure chest!!", Tone.Info);
        Sound(SoundCue.Treasure);
        DetectTrapWarning();
        yield return Ask(PromptKind.OpenChest, Command.Confirm, Command.Other);
        if (!ReturnPressed)
        {
            Echo("Ignore it");
            _next = Label.Features;
            yield break;
        }
        Echo("Open it");
        if (_trapped)
        {
            BeatClear();
            int boom = Int(Rnd() * 10 * State.Z + 1);
            Say("The chest explodes!!!", Tone.Danger);
            Sound(SoundCue.Trap);
            Say($"You suffer {boom} points of damage.", Tone.Bad);
            if (!Hurt(boom)) { _next = Label.Death; yield break; }
        }
        BeatClear();
        long gold = LInt(Rnd() * 1000 * Math.Pow(_l, 2) + 1);
        if (Rnd() > .9)
        {
            Say("Inside there are only cobwebs...");
            _next = Label.AfterTreasure;
            yield break;
        }
        Say($"Inside are {gold} gold pieces!", Tone.Good);
        Hero.Gold += gold;
        if (Rnd() > .5) { _next = Label.AfterTreasure; yield break; }
        _chain = true;
        BeatClear();
        _next = Label.Item;
    }

    /// <summary>
    /// 4200-4240: items 1-7 are replaced by the found plus (even if lower); 8-10 are counted.
    /// A trapped sword is hostile.
    /// </summary>
    private IEnumerable<Prompt> Item()
    {
        while (true)
        {
            int slot = Int(Rnd() * 10 + 1);
            int plus = Int(Math.Pow(Rnd(), .5) * (_l + 1) + 1);
            Say(Items.HasPlus(slot) ? $"You see a {Items.Name(slot)} +{plus}." : $"You see a {Items.Name(slot)}.", Tone.Info);
            Sound(SoundCue.Treasure);
            yield return Ask(PromptKind.TakeItem, Command.Confirm, Command.Other) with { Detail = $"{slot}:{plus}" };
            if (ReturnPressed)
            {
                if (slot == Items.Sword && _trapped)
                {
                    Say("It's a hostile sword!!", Tone.Danger);
                    int cut = Int(Rnd() * slot * 5 + 1);
                    Say($"You suffer {cut} points of damage.", Tone.Bad);
                    if (!Hurt(cut)) { _next = Label.Death; yield break; }
                    _next = Label.AfterTreasure;
                    yield break;
                }
                Say("It's yours!", Tone.Good);
                Hero.Items[slot]++;
                if (slot < 8) Hero.Items[slot] = plus;
            }
            else
            {
                Echo("Leave it");
            }

            if (_chain && Rnd() > .5)
            {
                BeatClear();
                continue;
            }
            _next = Label.AfterTreasure;
            yield break;
        }
    }

    /// <summary>4900-4910: half the time, a follower catches up.</summary>
    private IEnumerable<Prompt> AfterTreasure()
    {
        BeatClear();
        if (Rnd() > .5)
        {
            PopFollower();
            if (_m != 0) { _next = Label.ShowMonster; yield break; }
        }
        _next = Label.Features;
    }
}
