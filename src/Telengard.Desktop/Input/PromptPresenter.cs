using Telengard.Core.Content;
using Telengard.Core.Engine;
using Telengard.Core.Model;

namespace Telengard.Desktop.Input;

/// <summary>A clickable equivalent of a key answer.</summary>
public sealed record ActionChoice(string Label, string KeyHint, string Tooltip, PlayerInput Input);

/// <summary>Prompt wording and choices for presentation. The text is newly written for this project.</summary>
public static class PromptPresenter
{
    public static string Text(Prompt p) => p.Kind switch
    {
        PromptKind.Ready => "->",
        PromptKind.Combat => "Fight, Cast, or Evade?",
        PromptKind.TakeValuables => "RETURN to pick it up:",
        PromptKind.OpenChest => "RETURN to open it:",
        PromptKind.TakeItem => "RETURN to pick it up:",
        PromptKind.Approach => "RETURN to approach:",
        PromptKind.Stairs => p.Detail switch
        {
            "UD" => "Go Up, go Down, or Stay on this level?",
            "U" => "Go Up, or Stay on this level?",
            _ => "Go Down, or Stay on this level?",
        },
        PromptKind.Descend => "Do you want to descend? (Y/N)",
        PromptKind.Worship => "RETURN to worship:",
        PromptKind.Donate => "RETURN to donate money:",
        PromptKind.DonationAmount => "How much gold?",
        PromptKind.Drink => "RETURN to drink some:",
        PromptKind.EnterCube => "RETURN to walk in:",
        PromptKind.CubeLevel => "A number from 1 to 50:",
        PromptKind.Throne => "Pry some jewels, Sit down, Read the runes, or Ignore it?",
        PromptKind.PushButtons => "Push the buttons or Ignore?",
        PromptKind.BoxButton => $"Button {p.Detail}: push Red, Green, Yellow, Blue, or Stop:",
        PromptKind.Inn => "RETURN to re-enter the dungeon, F1 to store your character:",
        PromptKind.QuitConfirm => "Are you sure? (Y/N)",
        PromptKind.Help => "Press any key to continue...",
        PromptKind.SpellLevel => $"Spell level (1-{p.Detail}):",
        PromptKind.SpellNumber => "Spell (1-6, = to list):",
        PromptKind.SpellList => "Press a spell number, or any key to go back...",
        PromptKind.SlayHelpless => "RETURN to kill:",
        PromptKind.PassWallDirection => "Direction (N/S/W/E):",
        PromptKind.TeleportNorth => "+North / -South:",
        PromptKind.TeleportEast => "+East / -West:",
        PromptKind.TeleportUp => "+Up / -Down:",
        PromptKind.Saving => "Storing character...",
        _ => p.Kind.ToString(),
    };

    private static ActionChoice A(string label, string key, string tip, Command c) => new(label, key, tip, PlayerInput.Of(c));

    public static IReadOnlyList<ActionChoice> Choices(Prompt p, Character hero) => p.Kind switch
    {
        PromptKind.Ready =>
        [
            A("North", "W", "Move north (W, 8 or ↑)", Command.North),
            A("West", "A", "Move west (A, 4 or ←)", Command.West),
            A("Stay", "S", "Stay here for a turn (S, 5 or Space). A following monster catches up.", Command.Stay),
            A("East", "D", "Move east (D, 6 or →)", Command.East),
            A("South", "X", "Move south (X, 2 or ↓)", Command.South),
            A("Cast", "C", "Cast a spell", Command.Cast),
            A($"Rescue ({hero.Items[Items.ScrollOfRescue]})", "F3", "Read a Scroll of Rescue: return to the first stairway, dropping all carried gold", Command.UseScrollOfRescue),
            A($"Heal ({hero.Items[Items.PotionOfHealing]})", "F5", "Drink a Potion of Healing (1-20 HP)", Command.DrinkHealingPotion),
            A($"Strength ({hero.Items[Items.PotionOfStrength]})", "F7", "Drink a Potion of Strength (+4 to hit, +5 damage for 10-29 turns)", Command.DrinkStrengthPotion),
            A("Store", "F1", "Store (save) your character", Command.Save),
            A("Help", "H", "List the commands", Command.Help),
            A("Quit", "Q", "Quit: your character dies (\"suicide is painless\")", Command.Quit),
        ],
        PromptKind.Combat =>
        [
            A("Fight", "F", "Attack with your sword (STR, level and sword plus help)", Command.Fight),
            A("Cast", "C", "Cast a spell", Command.Cast),
            A("Evade", "E", "Try to run (DEX and elven boots help); the monster will follow you", Command.Evade),
        ],
        PromptKind.Stairs => p.Options.Select(o => o switch
        {
            Command.Up => A("Up", "U", "Climb the stairs", o),
            Command.Down => A("Down", "D", "Descend the stairs", o),
            _ => A("Stay", "S", "Stay on this level", o),
        }).ToList(),
        PromptKind.Descend or PromptKind.QuitConfirm => [A("Yes", "Y", "", Command.Yes), A("No", "N", "", Command.No)],
        PromptKind.Throne =>
        [
            A("Pry", "P", "Pry some jewels loose", Command.Pry),
            A("Sit", "S", "Sit on the throne", Command.Sit),
            A("Read", "R", "Read the runes (INT helps)", Command.Read),
            A("Ignore", "I", "Leave it alone", Command.Ignore),
        ],
        PromptKind.PushButtons => [A("Push", "P", "Try the buttons", Command.Push), A("Ignore", "I", "", Command.Ignore)],
        PromptKind.BoxButton =>
        [
            A("Red", "R", "", Command.Red), A("Green", "G", "", Command.Green),
            A("Yellow", "Y", "", Command.Yellow), A("Blue", "B", "", Command.Blue), A("Stop", "S", "", Command.Stop),
        ],
        PromptKind.Inn => [A("Re-enter", "RETURN", "Go back down into the dungeon", Command.Confirm), A("Store", "F1", "Store (save) your character", Command.Save)],
        PromptKind.Help => [A("Continue", "any key", "", Command.Other)],
        PromptKind.SpellLevel => Enumerable.Range(1, int.Parse(p.Detail ?? "1"))
            .Select(l => new ActionChoice($"Level {l}", l.ToString(), $"Costs {l} spell unit(s)", PlayerInput.Digit(l)))
            .Append(new ActionChoice("Cancel", "0", "", PlayerInput.Digit(0))).ToList(),
        PromptKind.SpellNumber or PromptKind.SpellList => Enumerable.Range(1, 6)
            .Select(i => Spells.Get(int.Parse(p.Detail ?? "1"), i))
            .Select(s => new ActionChoice($"{s.Index}. {s.Name}{(s.Combat ? " *" : "")}", s.Index.ToString(),
                (s.Combat ? "Combat spell. " : "") + s.Description, PlayerInput.Digit(s.Index)))
            .Append(A("Cancel", "RETURN", "", p.Kind == PromptKind.SpellNumber ? Command.Confirm : Command.Other)).ToList(),
        PromptKind.PassWallDirection =>
        [
            A("North", "W", "", Command.North), A("West", "A", "", Command.West),
            A("East", "D", "", Command.East), A("South", "X", "", Command.South),
        ],
        PromptKind.Saving => [],
        _ when p.AcceptsNumber => [],
        _ => [A("Yes (RETURN)", "RETURN", "", Command.Confirm), A("No", "N", "Any other key declines", Command.Other)],
    };
}
