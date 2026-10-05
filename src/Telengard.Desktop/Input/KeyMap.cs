using Avalonia.Input;
using Telengard.Core.Engine;

namespace Telengard.Desktop.Input;

/// <summary>
/// Translates keys into answers for the current prompt. Defaults follow the C64 V4.18 keys
/// (W/X/A/D move, S stay, F1 store, F3/F5/F7 scroll and potions, F/C/E in combat, RETURN to
/// accept) plus arrow keys and the numeric keypad. Dungeon commands can be remapped.
/// </summary>
public sealed class KeyMap
{
    public static readonly IReadOnlyDictionary<Command, Key[]> DefaultDungeonKeys = new Dictionary<Command, Key[]>
    {
        [Command.North] = [Key.W, Key.D8, Key.NumPad8, Key.Up],
        [Command.South] = [Key.X, Key.D2, Key.NumPad2, Key.Down],
        [Command.West] = [Key.A, Key.D4, Key.NumPad4, Key.Left],
        [Command.East] = [Key.D, Key.D6, Key.NumPad6, Key.Right],
        [Command.Stay] = [Key.S, Key.D5, Key.NumPad5, Key.Space],
        [Command.Help] = [Key.H, Key.F12],
        [Command.Quit] = [Key.Q],
        [Command.Replot] = [Key.P],
        [Command.Cast] = [Key.C, Key.OemPeriod],
        [Command.Save] = [Key.F1],
        [Command.UseScrollOfRescue] = [Key.F3],
        [Command.DrinkHealingPotion] = [Key.F5],
        [Command.DrinkStrengthPotion] = [Key.F7],
    };

    private readonly Dictionary<Key, Command> _dungeon = [];

    public KeyMap(IReadOnlyDictionary<string, string[]>? overrides = null)
    {
        foreach (var (command, keys) in DefaultDungeonKeys)
        {
            var effective = keys;
            if (overrides is not null && overrides.TryGetValue(command.ToString(), out var names))
            {
                var parsed = names.Select(n => Enum.TryParse<Key>(n, out var k) ? k : Key.None).Where(k => k != Key.None).ToArray();
                if (parsed.Length > 0) effective = parsed;
            }
            foreach (var k in effective) _dungeon[k] = command;
        }
    }

    public IEnumerable<Key> KeysFor(Command command) => _dungeon.Where(kv => kv.Value == command).Select(kv => kv.Key);

    public static int? Digit(Key key) => key switch
    {
        >= Key.D0 and <= Key.D9 => key - Key.D0,
        >= Key.NumPad0 and <= Key.NumPad9 => key - Key.NumPad0,
        _ => null,
    };

    private static bool IsReturn(Key key) => key is Key.Enter or Key.Return;

    /// <summary>The answer a key gives to a prompt, or null if the key means nothing here.</summary>
    public PlayerInput? Translate(Prompt prompt, Key key)
    {
        if (key is Key.None or Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.Escape or Key.Pause or Key.Tab or Key.LWin or Key.RWin) return null;
        int? digit = Digit(key);
        switch (prompt.Kind)
        {
            case PromptKind.Ready:
                return _dungeon.TryGetValue(key, out var c) ? PlayerInput.Of(c) : PlayerInput.Of(Command.Other);

            case PromptKind.Combat:
                return key switch
                {
                    Key.F or Key.F3 => PlayerInput.Of(Command.Fight),
                    Key.C or Key.F5 => PlayerInput.Of(Command.Cast),
                    Key.E or Key.F7 => PlayerInput.Of(Command.Evade),
                    _ => PlayerInput.Of(Command.Other),
                };

            case PromptKind.Stairs:
                return key switch
                {
                    Key.U or Key.D7 or Key.NumPad7 or Key.PageUp => PlayerInput.Of(Command.Up),
                    Key.D or Key.D1 or Key.NumPad1 or Key.PageDown => PlayerInput.Of(Command.Down),
                    Key.S or Key.D5 or Key.NumPad5 or Key.Space => PlayerInput.Of(Command.Stay),
                    _ => PlayerInput.Of(Command.Other),
                };

            case PromptKind.Descend:
            case PromptKind.QuitConfirm:
                return key is Key.Y or Key.D9 or Key.NumPad9 ? PlayerInput.Of(Command.Yes) : PlayerInput.Of(Command.No);

            case PromptKind.Throne:
                return key switch
                {
                    Key.P => PlayerInput.Of(Command.Pry),
                    Key.S => PlayerInput.Of(Command.Sit),
                    Key.R => PlayerInput.Of(Command.Read),
                    Key.I => PlayerInput.Of(Command.Ignore),
                    _ => PlayerInput.Of(Command.Other),
                };

            case PromptKind.PushButtons:
                return key == Key.P ? PlayerInput.Of(Command.Push) : PlayerInput.Of(Command.Ignore);

            case PromptKind.BoxButton:
                return key switch
                {
                    Key.R => PlayerInput.Of(Command.Red),
                    Key.G => PlayerInput.Of(Command.Green),
                    Key.Y => PlayerInput.Of(Command.Yellow),
                    Key.B => PlayerInput.Of(Command.Blue),
                    Key.S => PlayerInput.Of(Command.Stop),
                    _ => PlayerInput.Of(Command.Other),
                };

            case PromptKind.Inn:
                return IsReturn(key) ? PlayerInput.Of(Command.Confirm) : key == Key.F1 ? PlayerInput.Of(Command.Save) : PlayerInput.Of(Command.Other);

            case PromptKind.Help:
                return PlayerInput.Of(Command.Other);

            case PromptKind.SpellLevel:
                return PlayerInput.Digit(digit ?? 0);

            case PromptKind.SpellNumber:
                if (IsReturn(key)) return PlayerInput.Of(Command.Confirm);
                return digit is >= 1 and <= 6 ? PlayerInput.Digit(digit.Value) : PlayerInput.Of(Command.ListSpells);

            case PromptKind.SpellList:
                return digit is { } d ? PlayerInput.Digit(d) : PlayerInput.Of(Command.Other);

            case PromptKind.PassWallDirection:
                if (_dungeon.TryGetValue(key, out var dir) && dir is Command.North or Command.South or Command.West or Command.East)
                    return PlayerInput.Of(dir);
                return PlayerInput.Of(Command.Other);

            case PromptKind.Saving:
                return null;

            case PromptKind.DonationAmount:
            case PromptKind.CubeLevel:
            case PromptKind.TeleportNorth:
            case PromptKind.TeleportEast:
            case PromptKind.TeleportUp:
                return null; // handled by the number entry buffer

            default: // RETURN-to-accept prompts: any other key declines
                return IsReturn(key) ? PlayerInput.Of(Command.Confirm) : PlayerInput.Of(Command.Other);
        }
    }

    public static bool IsNumberEntry(PromptKind kind) => kind is PromptKind.DonationAmount or PromptKind.CubeLevel
        or PromptKind.TeleportNorth or PromptKind.TeleportEast or PromptKind.TeleportUp;
}
