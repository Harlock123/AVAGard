using System.Collections.ObjectModel;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Telengard.Core.Content;
using Telengard.Core.Dungeon;
using Telengard.Core.Engine;
using Telengard.Core.Model;
using Telengard.Core.Randomness;
using Telengard.Core.Timing;
using Telengard.Desktop.Controls;
using Telengard.Desktop.Input;
using Telengard.Desktop.Services;
using Telengard.Infrastructure.Saves;

namespace Telengard.Desktop.ViewModels;

public sealed record LogLine(string Text, Tone Tone)
{
    /// <summary>A text marker so tone is not conveyed by colour alone.</summary>
    public string Marker => Tone switch
    {
        Tone.Good => "+",
        Tone.Bad => "-",
        Tone.Danger => "!",
        Tone.Info => "*",
        Tone.Echo => ">",
        _ => " ",
    };
}

public sealed record StatLine(string Label, string Value, string Tooltip);

public sealed partial class ActionButton(ActionChoice choice, Action<PlayerInput> submit) : ObservableObject
{
    public string Label => choice.Label;
    public string KeyHint => choice.KeyHint;
    public string Tooltip => string.IsNullOrEmpty(choice.Tooltip) ? $"Key: {choice.KeyHint}" : $"{choice.Tooltip}  (key: {choice.KeyHint})";
    [RelayCommand] private void Invoke() => submit(choice.Input);
}

/// <summary>
/// Hosts one play session. Engine events are played back in order with presentation pacing;
/// input is accepted only for the prompt currently shown, which prevents duplicate actions
/// from held keys or overlapping clicks. Game rules run synchronously and are tiny; file
/// writes run off the UI thread.
/// </summary>
public sealed partial class GameViewModel : ViewModelBase, IDisposable
{
    private readonly AppServices _services;
    private readonly MainViewModel _main;
    private readonly Game _game;
    private readonly PromptDeadline _deadline;
    private readonly KeyMap _keys;
    private readonly Queue<GameEvent> _pending = new();
    private readonly DispatcherTimer _deadlineTicker;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private CancellationTokenSource _skip = new();
    private bool _pumping;
    private bool _disposed;
    private Prompt? _shown;
    private GameState? _pendingSave;

    public GameViewModel(AppServices services, MainViewModel main, GameState state, RulesPolicy policy, StartPoint start)
    {
        _services = services;
        _main = main;
        Policy = policy;
        _game = new Game(state, policy, new SplitMix64Random(state.RandomState == 0 ? (ulong)Random.Shared.NextInt64() : state.RandomState));
        _deadline = new PromptDeadline(services.Time);
        _keys = new KeyMap(policy.Preset == Preset.Modern ? services.Settings.KeyBindings : null);
        _deadlineTicker = new DispatcherTimer(TimeSpan.FromMilliseconds(50), DispatcherPriority.Background, (_, _) => UpdateDeadlineBar());
        Enqueue(_game.Start(start));
        if (IsModern && start == StartPoint.NewCharacter && services.Settings.ShowTutorial)
        {
            ShowTutorial = true;
            IsPaused = true;
        }
        RefreshAll();
        Pump();
    }

    /// <summary>Newly written, optional first-game tips (Modern only).</summary>
    public string TutorialText =>
        "Welcome to AVAGard. There is no quest: explore, survive, and grow strong.\n\n" +
        "• Move with W A X D (or the arrow keys). S stays for a turn.\n" +
        "• Every decision has a time limit (shown by the bar). If you wait, the game acts for you.\n" +
        "• Gold only becomes experience at an inn: climb the stairway you start beside.\n" +
        "• Monsters scale with depth. Stay near level 1 until you reach character level 2.\n" +
        "• RETURN accepts treasure and offers; any other key declines. Esc pauses.\n" +
        "• F1 stores your character. Modern also keeps a resume point automatically.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowPauseMenu))]
    private bool _showTutorial;

    public bool ShowPauseMenu => IsPaused && !ShowTutorial;

    partial void OnIsPausedChanged(bool value) => OnPropertyChanged(nameof(ShowPauseMenu));

    [RelayCommand]
    private void CloseTutorial(bool dontShowAgain)
    {
        ShowTutorial = false;
        if (dontShowAgain)
        {
            _services.Settings.ShowTutorial = false;
            _ = _services.SaveSettingsAsync();
        }
        if (IsPaused) TogglePause();
    }

    public RulesPolicy Policy { get; }
    public bool IsModern => Policy.Preset == Preset.Modern;
    public bool IsClassic => !IsModern;
    public string ModeLabel => IsModern
        ? Policy.InputDeadline is { } d ? $"Modern · {d.TotalSeconds:0.#} s turns" : "Modern · deliberate (no time limit)"
        : $"Classic · {RulesPolicy.ClassicDeadline.TotalSeconds:0.#} s turns";

    public ObservableCollection<LogLine> Messages { get; } = [];
    public ObservableCollection<LogLine> Log { get; } = [];
    public ObservableCollection<ActionButton> Actions { get; } = [];
    public ObservableCollection<StatLine> Stats { get; } = [];
    public ObservableCollection<StatLine> Equipment { get; } = [];
    public ObservableCollection<StatLine> Effects { get; } = [];

    [ObservableProperty] private DungeonSnapshot? _snapshot;
    [ObservableProperty] private string _promptText = "";
    [ObservableProperty] private string _numberEntry = "";
    [ObservableProperty] private bool _isNumberEntry;
    [ObservableProperty] private double _deadlineFraction;
    [ObservableProperty] private bool _deadlineVisible;
    [ObservableProperty] private bool _isPaused;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _showHelp;
    [ObservableProperty] private bool _isDead;
    [ObservableProperty] private string _deathText = "";
    [ObservableProperty] private string _heroTitle = "";
    [ObservableProperty] private string _location = "";
    [ObservableProperty] private string _followersText = "";
    [ObservableProperty] private string? _banner;

    public bool HighContrast => _services.Settings.HighContrast;
    public bool ShowAutomap => IsModern && _services.Settings.ShowAutomap;
    public Game Game => _game;

    // ---- event playback -----------------------------------------------------------------

    private void Enqueue(IEnumerable<GameEvent> events)
    {
        foreach (var e in events) _pending.Enqueue(e);
    }

    private double PacingScale => IsModern ? _services.Settings.ModernPacing : _services.Settings.ClassicPacing;

    private async void Pump()
    {
        if (_pumping || _disposed) return;
        _pumping = true;
        IsBusy = true;
        _deadline.Cancel();
        DeadlineVisible = false;
        try
        {
          while (true)
          {
            while (_pending.TryDequeue(out var e))
            {
                if (_disposed) return;
                switch (e)
                {
                    case MessageEvent m:
                        var line = new LogLine(m.Text, m.Tone);
                        Messages.Add(line);
                        while (Messages.Count > 6) Messages.RemoveAt(0);
                        Log.Add(line);
                        while (Log.Count > 500) Log.RemoveAt(0);
                        break;
                    case ClearMessagesEvent:
                        if (Messages.Count > 0) Messages.Clear();
                        break;
                    case PauseEvent p:
                        RefreshAll();
                        await Beat(p.Kind);
                        if (p.Kind == PauseKind.BeatAndClear) Messages.Clear();
                        break;
                    case SoundEvent s when _services.Settings.SoundEnabled:
                        _services.Sound.Play(s.Cue, _services.Settings.SoundVolume);
                        break;
                    case SaveRequestedEvent save:
                        _pendingSave = save.Snapshot;
                        break;
                    case DiedEvent died:
                        await OnDiedAsync(died);
                        break;
                    case SessionEndedEvent { Outcome: GameOutcome.SavedAndExited }:
                        Banner = "Character stored. Returning to the title screen...";
                        await Beat(PauseKind.Beat);
                        _main.ShowTitle();
                        return;
                }
            }
            RefreshAll();
            await PresentPromptAsync(); // may answer the prompt itself (saving), queuing more events
            if (_pending.Count == 0) break;
          }
        }
        finally
        {
            _pumping = false;
            IsBusy = false;
        }
    }

    private async Task Beat(PauseKind kind)
    {
        double seconds = 1.5 * PacingScale;
        if (seconds <= 0.01) return;
        try { await Task.Delay(TimeSpan.FromSeconds(seconds), _services.Time, _skip.Token); }
        catch (TaskCanceledException) { }
    }

    private async Task PresentPromptAsync()
    {
        var p = _game.CurrentPrompt;
        _shown = p;
        if (p is null) return;

        if (p.Kind == PromptKind.Saving)
        {
            await WriteManualSaveAsync();
            return;
        }

        if (_skip.IsCancellationRequested) _skip = new CancellationTokenSource();
        PromptText = PromptPresenter.Text(p);
        if (IsModern && PromptPresenter.ItemComparison(p, _game.Hero) is { } compare) PromptText += "   " + compare;
        IsNumberEntry = KeyMap.IsNumberEntry(p.Kind);
        NumberEntry = "";
        ShowHelp = p.Kind == PromptKind.Help;
        Actions.Clear();
        foreach (var c in PromptPresenter.Choices(p, _game.Hero)) Actions.Add(new ActionButton(c, i => Answer(i)));

        if (p.Kind == PromptKind.Ready && Policy.SuspendSnapshots && !_game.Hero.IsDemo)
            _ = WriteSuspendAsync(_game.State.Clone());

        if (p.HasDeadline && Policy.InputDeadline is { } limit && !IsPaused)
        {
            long seq = p.Sequence;
            _deadline.Arm(seq, limit, s => Dispatcher.UIThread.Post(() => OnDeadline(s)));
            DeadlineVisible = true;
            DeadlineFraction = 1;
            _deadlineTicker.Start();
        }
    }

    private void OnDeadline(long sequence)
    {
        if (_disposed || _pumping || _shown is null || _shown.Sequence != sequence) return;
        if (IsNumberEntry && NumberEntry.Length > 0 && int.TryParse(NumberEntry, out var typed))
        {
            Submit(PlayerInput.Digit(typed)); // the original reads what was typed so far as the number
            return;
        }
        Submit(PlayerInput.Timeout);
    }

    private void UpdateDeadlineBar()
    {
        var f = _deadline.RemainingFraction;
        if (f is null) { _deadlineTicker.Stop(); return; }
        // Reduced motion: the bar moves in quarter steps instead of animating continuously.
        DeadlineFraction = _services.Settings.ReducedMotion ? Math.Ceiling(f.Value * 4) / 4 : f.Value;
    }

    // ---- input ---------------------------------------------------------------------------

    /// <summary>Keyboard entry point. Returns true if the key was used.</summary>
    public bool HandleKey(Key key)
    {
        if (IsDead) return HandleDeathKey(key);
        if (ShowTutorial)
        {
            CloseTutorial(false); // any key dismisses the tips (the key itself is not used as a command)
            return true;
        }
        if (key is Key.Escape or Key.Pause)
        {
            TogglePause();
            return true;
        }
        if (IsPaused) return false;
        if (_pumping)
        {
            _skip.Cancel(); // any key hurries the messages along
            return true;
        }
        var p = _game.CurrentPrompt;
        if (p is null || p != _shown) return false;

        if (IsNumberEntry)
        {
            if (key is Key.Enter or Key.Return)
            {
                Submit(PlayerInput.Digit(int.TryParse(NumberEntry, out var n) ? n : 0));
                return true;
            }
            if (key == Key.Back && NumberEntry.Length > 0) { NumberEntry = NumberEntry[..^1]; _deadline.Restart(); return true; }
            if ((key is Key.OemMinus or Key.Subtract) && NumberEntry.Length == 0) { NumberEntry = "-"; _deadline.Restart(); return true; }
            if (KeyMap.Digit(key) is { } d && NumberEntry.Length < 9) { NumberEntry += d; _deadline.Restart(); return true; }
            return false;
        }

        if (_keys.Translate(p, key) is { } input)
        {
            Submit(input);
            return true;
        }
        return false;
    }

    [RelayCommand]
    private void SubmitNumber() => Answer(PlayerInput.Digit(int.TryParse(NumberEntry, out var n) ? n : 0));

    private void Answer(PlayerInput input)
    {
        if (_pumping || IsPaused || IsDead) return;
        Submit(input);
    }

    private void Submit(PlayerInput input)
    {
        var p = _game.CurrentPrompt;
        if (p is null || p != _shown) return;
        _deadline.Cancel();
        _deadlineTicker.Stop();
        DeadlineVisible = false;
        _shown = null; // blocks a second answer to the same prompt
        Actions.Clear();
        Enqueue(_game.Submit(input, p.Sequence));
        Pump();
    }

    [RelayCommand]
    private void TogglePause()
    {
        if (IsDead) return;
        IsPaused = !IsPaused;
        if (IsPaused) _deadline.Pause();
        else _deadline.Resume();
    }

    [RelayCommand]
    private void ToggleHelp() => ShowHelp = !ShowHelp;

    // ---- saving ----------------------------------------------------------------------------

    private async Task WriteManualSaveAsync()
    {
        var snapshot = _pendingSave ?? _game.State.Clone();
        _pendingSave = null;
        bool ok;
        try
        {
            await _writeLock.WaitAsync();
            try { await Task.Run(() => _services.Saves.SaveAsync(snapshot, SaveKind.Manual, Policy)); }
            finally { _writeLock.Release(); }
            ok = true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            Banner = "Could not store the character: " + e.Message;
            ok = false;
        }
        var p = _game.CurrentPrompt!;
        _shown = p;
        Submit(PlayerInput.Of(ok ? Command.SaveSucceeded : Command.SaveFailed));
    }

    private async Task WriteSuspendAsync(GameState snapshot)
    {
        if (!await _writeLock.WaitAsync(0)) return; // a write is already in progress; the next prompt will write again
        try { await Task.Run(() => _services.Saves.SaveAsync(snapshot, SaveKind.Suspend, Policy)); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Banner = "Warning: the resume snapshot could not be written (" + e.Message + ").";
        }
        finally { _writeLock.Release(); }
    }

    // ---- death -------------------------------------------------------------------------------

    private async Task OnDiedAsync(DiedEvent died)
    {
        RefreshAll();
        if (!_game.Hero.IsDemo)
        {
            await _writeLock.WaitAsync();
            try { await Task.Run(() => _services.Saves.OnDeath(_game.Hero.Name, Policy)); }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
            finally { _writeLock.Release(); }
        }
        var stored = !_game.Hero.IsDemo && File.Exists(_services.Saves.PathFor(_game.Hero.Name, SaveKind.Manual));
        DeathText = (died.Mighty ? "Another mighty adventurer bites the dust." : "Another not-so-mighty adventurer bites the dust.")
            + $"\n\n{_game.Hero.Name}, level {_game.Hero.Level}, {_game.Hero.Experience} experience."
            + (stored ? "\nThe character you last stored can still be read in again." : "")
            + "\n\nDo you want to try again? (Y/N)";
        PromptText = "";
        Actions.Clear();
        DeadlineVisible = false;
        IsDead = true;
    }

    private bool HandleDeathKey(Key key)
    {
        if (key is Key.Y or Key.Enter or Key.Return or Key.D9) { TryAgain(); return true; }
        if (key is Key.N or Key.D3) { GiveUp(); return true; }
        return false;
    }

    [RelayCommand] private void TryAgain() => _main.ShowTitle();
    [RelayCommand] private void GiveUp() => _main.RequestExit();
    [RelayCommand] private void BackToTitle() => _main.ShowTitle();

    // ---- presentation state ------------------------------------------------------------

    private void RefreshAll()
    {
        var s = _game.State;
        var h = s.Character;
        var at = _game.CurrentPrompt?.Kind == PromptKind.Inn || s.Z == 0;
        var followers = s.Followers.Select((f, i) => (f, i)).Where(t => !t.f.IsEmpty).Select(t => t.i + 1).ToList();
        Snapshot = new DungeonSnapshot(
            s.X, s.Y, s.Z,
            at ? null : LocalView.Compute(_game.Map, s.X, s.Y, s.Z, h.TimerActive(Effect.Light)),
            at ? default : _game.Map.Features(s.X, s.Y, s.Z),
            _game.Monster,
            followers,
            h.TimerActive(Effect.Invisibility),
            at ? InnNames.For(s.X, s.Y) : null,
            IsModern ? s.Explored : null);

        HeroTitle = $"{h.Name}   LVL {h.Level}";
        Location = at ? "At the inn" : IsModern ? $"Level {s.Z} · ({s.X}, {s.Y}) · turn {s.TurnCount}" : "";
        FollowersText = followers.Count == 0 ? "" :
            IsModern ? $"Followed by {followers.Count} monster(s); nearest {followers[0]} step(s) behind" : new string('Q', followers.Count);

        Stats.Clear();
        for (int i = 0; i < 6; i++)
            Stats.Add(new StatLine(Stat.Abbreviations[i], h.Stats[i].ToString(), StatTooltip(i)));
        Stats.Add(new StatLine("HP", $"{h.Hp}/{h.MaxHp}", "Hit points: current / maximum. Restored fully at an inn."));
        Stats.Add(new StatLine("SU", $"{h.SpellUnits}/{h.MaxSpellUnits}", "Spell units: current / maximum. A spell costs its level in units. Restored at an inn."));
        Stats.Add(new StatLine("EX", h.Experience.ToString(), $"Experience. Next level at {Character.ExperienceFor(h.Level + 1)}."));
        Stats.Add(new StatLine("GD", h.Gold.ToString(), "Gold carried. It only becomes experience when you reach an inn."));
        if (h.Banked > 0) Stats.Add(new StatLine("Safe", h.Banked.ToString(), "Total gold deposited at inns."));

        Equipment.Clear();
        for (int i = 1; i <= Items.Count; i++)
        {
            if (h.Items[i] <= 0) continue;
            Equipment.Add(new StatLine(Items.Name(i), Items.HasPlus(i) ? $"+{h.Items[i]}" : $"×{h.Items[i]}", ItemTooltip(i)));
        }
        if (Equipment.Count == 0) Equipment.Add(new StatLine("(none)", "", "You start with nothing; treasure may contain arms, armour, rings, scrolls and potions."));

        Effects.Clear();
        for (int i = 1; i <= Effect.Count; i++)
            if (h.Timers[i] > 0)
                Effects.Add(new StatLine(Effect.Name(i), IsModern ? $"{h.Timers[i]} turns" : "", ""));
        if (Effects.Count == 0) Effects.Add(new StatLine("(none)", "", ""));
    }

    private static string StatTooltip(int i) => i switch
    {
        Stat.Str => "Strength: adds half its value to your chance to hit.",
        Stat.Int => "Intelligence: resists failure of Sleep, Web, Hold Monster, Phantasmal Forces; reading throne runes.",
        Stat.Wis => "Wisdom: Turn Undead and Plague.",
        Stat.Con => "Constitution: hit points gained per level; Plague and Raise Dead.",
        Stat.Dex => "Dexterity: striking first, evading, avoiding pits.",
        _ => "Charisma: friendly elves, thieving hobbits, generous dragons, Charm.",
    };

    private static string ItemTooltip(int slot) => slot switch
    {
        Items.Sword => "Adds its plus to hit and to damage.",
        Items.Armor => "Subtracts its plus from the monster's chance to hit.",
        Items.Shield => "Subtracts its plus from the monster's chance to hit (it does not reduce dragon fire).",
        Items.ElvenCloak => "Each plus gives a 1-in-20 chance that a monster does not notice you.",
        Items.ElvenBoots => "Adds its plus to Dexterity when evading and avoiding pits.",
        Items.RingOfRegeneration => "Restores its plus in hit points every turn.",
        Items.RingOfProtection => "The manual says it absorbs damage, but in the reference version it has no effect.",
        Items.ScrollOfRescue => "F3: return to the first stairway; you drop all carried gold.",
        Items.PotionOfHealing => "F5: restores 1-20 hit points.",
        _ => "F7: +4 to hit and +5 damage for 10-29 turns.",
    };

    public void Dispose()
    {
        _disposed = true;
        _deadline.Dispose();
        _deadlineTicker.Stop();
        _skip.Cancel();
    }
}
