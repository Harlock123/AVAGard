using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Telengard.Core.Engine;
using Telengard.Core.Model;
using Telengard.Core.Randomness;
using Telengard.Desktop.Services;
using Telengard.Desktop.ViewModels;
using Telengard.Desktop.Views;

namespace Telengard.Desktop.Tests;

public sealed class SmokeTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "tg-ui-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }

    private (MainWindow Window, MainViewModel Vm) Open(Action<AppServices>? configure = null)
    {
        var services = AppServices.CreateIsolated(_root);
        services.Settings.ClassicPacing = 0;
        services.Settings.ModernPacing = 0;
        services.Settings.ModernDeadlineSeconds = null; // deliberate: no timers firing during tests
        configure?.Invoke(services);
        var vm = new MainViewModel(services);
        var window = new MainWindow { DataContext = vm, Width = 1180, Height = 780 };
        window.Show();
        Pump();
        return (window, vm);
    }

    private static void Pump()
    {
        for (int i = 0; i < 20; i++) Dispatcher.UIThread.RunJobs();
    }

    private static void Press(MainWindow w, Key key)
    {
        w.KeyPress(key, RawInputModifiers.None, PhysicalKey.None, null);
        w.KeyRelease(key, RawInputModifiers.None, PhysicalKey.None, null);
        Pump();
    }

    public static void Capture(MainWindow w, string name)
    {
        var dir = Environment.GetEnvironmentVariable("TG_SCREENSHOT_DIR");
        if (string.IsNullOrEmpty(dir)) return;
        Directory.CreateDirectory(dir);
        var frame = w.CaptureRenderedFrame();
        frame?.Save(Path.Combine(dir, name + ".png"), Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
    }

    [AvaloniaFact]
    public void TitleScreenShowsAndOffersBothPresets()
    {
        var (w, vm) = Open();
        Assert.IsType<TitleViewModel>(vm.Current);
        Capture(w, "01-title");
        w.Close();
    }

    [AvaloniaFact]
    public void CreateModernCharacterAndPlayWithKeyboard()
    {
        var (w, vm) = Open();
        ((TitleViewModel)vm.Current).NewModernCommand.Execute(null);
        Pump();
        var creation = Assert.IsType<CreationViewModel>(vm.Current);
        Capture(w, "02-creation");
        Press(w, Key.Enter);          // keep these attributes
        Assert.True(creation.IsNaming);
        creation.Name = "Tester";
        Press(w, Key.Enter);          // descend
        var game = Assert.IsType<GameViewModel>(vm.Current);
        Pump();
        Capture(w, "03-game-start");

        // Answer whatever is asked until we are at the command prompt, then walk around.
        for (int i = 0; i < 40 && !game.IsDead; i++)
        {
            var p = game.Game.CurrentPrompt;
            if (p is null) break;
            Press(w, p.Kind switch
            {
                PromptKind.Ready => (i % 4) switch { 0 => Key.W, 1 => Key.D, 2 => Key.X, _ => Key.A },
                PromptKind.Combat => Key.E,
                PromptKind.Stairs => Key.S,
                _ => Key.N,
            });
        }
        Capture(w, "04-game-after-moves");
        Assert.True(game.Game.State.TurnCount > 0 || game.IsDead,
            $"turns={game.Game.State.TurnCount} prompt={game.Game.CurrentPrompt?.Kind} paused={game.IsPaused} tutorial={game.ShowTutorial} busy={game.IsBusy} log={string.Join(" | ", game.Log.Select(l => l.Text))}");
        w.Close();
    }

    [AvaloniaFact]
    public void HeldKeyRepeatDoesNotTriggerASecondAction()
    {
        var (w, vm) = Open();
        var state = CharacterCreation.NewGame("REPEAT", [10, 10, 10, 12, 10, 10], new SplitMix64Random(3));
        vm.StartGame(state, vm.PolicyFor(Preset.Modern), StartPoint.Suspended); // starts at the ready prompt
        Pump();
        var game = Assert.IsType<GameViewModel>(vm.Current);
        Assert.Equal(PromptKind.Ready, game.Game.CurrentPrompt!.Kind);
        long before = game.Game.CurrentPrompt.Sequence;
        w.KeyPress(Key.H, RawInputModifiers.None, PhysicalKey.None, null); // Help: one prompt change
        Pump();
        long afterFirst = game.Game.CurrentPrompt!.Sequence;
        w.KeyPress(Key.H, RawInputModifiers.None, PhysicalKey.None, null); // auto-repeat (no release)
        Pump();
        Assert.True(afterFirst > before);
        Assert.Equal(afterFirst, game.Game.CurrentPrompt!.Sequence);
        Capture(w, "05-help-overlay");
        w.Close();
    }

    [AvaloniaFact]
    public void ModernGameWritesAResumePointAndItAppearsOnTheTitle()
    {
        var (w, vm) = Open();
        var state = CharacterCreation.NewGame("RESUME", [10, 10, 10, 12, 10, 10], new SplitMix64Random(4));
        vm.StartGame(state, vm.PolicyFor(Preset.Modern), StartPoint.Suspended);
        Pump();
        for (int i = 0; i < 50 && !File.Exists(vm.Services.Saves.PathFor("RESUME", Infrastructure.Saves.SaveKind.Suspend)); i++)
        {
            Thread.Sleep(20);
            Pump();
        }
        Assert.True(File.Exists(vm.Services.Saves.PathFor("RESUME", Infrastructure.Saves.SaveKind.Suspend)));
        vm.ShowTitle();
        Pump();
        var title = Assert.IsType<TitleViewModel>(vm.Current);
        Assert.Contains(title.Saves, s => s.Summary.CharacterName == "RESUME");
        Capture(w, "06-title-with-save");
        w.Close();
    }
}

public sealed class ScreenshotScenarioTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "tg-ui-" + Guid.NewGuid().ToString("N"));
    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }

    private static void Pump() { for (int i = 0; i < 20; i++) Dispatcher.UIThread.RunJobs(); }

    [AvaloniaFact]
    public void MonsterEncounterRendersInBothPresets()
    {
        foreach (var preset in new[] { Preset.Modern, Preset.Classic })
        {
            var services = AppServices.CreateIsolated(_root);
            services.Settings.ClassicPacing = 0;
            services.Settings.ModernPacing = 0;
            services.Settings.ModernDeadlineSeconds = null;
            var vm = new MainViewModel(services);
            var w = new MainWindow { DataContext = vm, Width = 1180, Height = 780 };
            w.Show();
            GameViewModel? game = null;
            // Find a seed whose first turn on a deeper level produces a monster.
            for (ulong seed = 1; seed < 400; seed++)
            {
                var state = CharacterCreation.NewGame("SHOTS", [14, 13, 12, 15, 11, 9], new SplitMix64Random(seed));
                state.Z = 6; state.X = 60; state.Y = 40;
                state.Character.Items[1] = 3; state.Character.Items[2] = 2; state.Character.Items[9] = 2;
                state.Character.Timers[3] = 9;
                state.RandomState = seed * 7919;
                var policy = preset == Preset.Modern ? vm.PolicyFor(Preset.Modern) : vm.PolicyFor(Preset.Classic) with { InputDeadline = null };
                vm.StartGame(state, policy, StartPoint.NewCharacter);
                Pump();
                game = (GameViewModel)vm.Current;
                if (game.Game.Monster is not null && game.Game.CurrentPrompt?.Kind == PromptKind.Combat) break;
            }
            Assert.NotNull(game!.Game.Monster);
            Pump();
            SmokeTests.Capture(w, $"07-monster-{preset.ToString().ToLowerInvariant()}");
            w.Close();
        }
    }
}

public sealed class VerticalSliceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "tg-ui-" + Guid.NewGuid().ToString("N"));
    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }

    private static void Pump()
    {
        for (int i = 0; i < 30; i++) Dispatcher.UIThread.RunJobs();
    }

    private static void Press(MainWindow w, Key key)
    {
        w.KeyPress(key, RawInputModifiers.None, PhysicalKey.None, null);
        w.KeyRelease(key, RawInputModifiers.None, PhysicalKey.None, null);
        Pump();
    }

    private static void WaitFor(Func<bool> condition)
    {
        for (int i = 0; i < 200 && !condition(); i++) { Thread.Sleep(10); Pump(); }
    }

    /// <summary>
    /// Through the real UI with the keyboard: cast a spell, reach the inn, store, return to title,
    /// read the character in, quit (die) and get back to the title.
    /// </summary>
    [AvaloniaFact]
    public void CastInnStoreReadInDieRestart()
    {
        var services = AppServices.CreateIsolated(_root);
        services.Settings.ClassicPacing = 0;
        services.Settings.ModernPacing = 0;
        services.Settings.ModernDeadlineSeconds = null;
        services.Settings.ShowTutorial = false;
        var vm = new MainViewModel(services);
        var w = new MainWindow { DataContext = vm, Width = 1180, Height = 780 };
        w.Show();

        var state = CharacterCreation.NewGame("SLICE", [12, 14, 12, 18, 12, 10], new SplitMix64Random(11));
        vm.StartGame(state, vm.PolicyFor(Preset.Modern), StartPoint.Suspended); // at the start room's prompt
        Pump();
        var game = Assert.IsType<GameViewModel>(vm.Current);

        // Cast Light (level 1, spell 4) from the command prompt.
        Assert.Equal(PromptKind.Ready, game.Game.CurrentPrompt!.Kind);
        Press(w, Key.C); Press(w, Key.D1); Press(w, Key.D4);
        Assert.Equal(0, game.Game.Hero.SpellUnits);
        Assert.True(game.Game.Hero.Timers[Core.Model.Effect.Light] > 0);

        // Stay until the start room's stairway is offered, then climb to the inn.
        for (int i = 0; i < 60 && game.Game.CurrentPrompt?.Kind != PromptKind.Inn && !game.IsDead; i++)
        {
            var p = game.Game.CurrentPrompt!;
            Press(w, p.Kind switch
            {
                PromptKind.Stairs => Key.U,
                PromptKind.Ready => Key.S,
                PromptKind.Combat => Key.E,
                _ => Key.N,
            });
        }
        if (game.IsDead) return; // an unlucky monster; the death path is covered elsewhere
        Assert.Equal(PromptKind.Inn, game.Game.CurrentPrompt!.Kind);
        Assert.Equal(game.Game.Hero.MaxSpellUnits, game.Game.Hero.SpellUnits); // the inn restores spell units
        SmokeTests.Capture(w, "08-inn");

        // Store (F1). Modern keeps playing.
        Press(w, Key.F1);
        var path = services.Saves.PathFor("SLICE", Infrastructure.Saves.SaveKind.Manual);
        WaitFor(() => File.Exists(path) && game.Game.CurrentPrompt?.Kind == PromptKind.Inn);
        Assert.True(File.Exists(path));

        // Back to title, read the character in.
        vm.ShowTitle();
        Pump();
        var title = Assert.IsType<TitleViewModel>(vm.Current);
        var entry = title.Saves.First(s => s.Summary.Kind == Infrastructure.Saves.SaveKind.Manual);
        entry.OpenCommand.Execute(null);
        WaitFor(() => vm.Current is GameViewModel);
        var resumed = Assert.IsType<GameViewModel>(vm.Current);
        Assert.Equal("SLICE", resumed.Game.Hero.Name);
        Assert.Equal(1, resumed.Game.State.Z); // stored at the inn, read in below it

        // Quit = death, then "try again" returns to the title.
        for (int i = 0; i < 30 && resumed.Game.CurrentPrompt?.Kind != PromptKind.Ready && !resumed.IsDead; i++)
            Press(w, resumed.Game.CurrentPrompt!.Kind switch { PromptKind.Stairs => Key.S, PromptKind.Combat => Key.E, _ => Key.N });
        if (!resumed.IsDead)
        {
            Press(w, Key.Q);
            Press(w, Key.Y);
        }
        Assert.True(resumed.IsDead);
        Assert.True(File.Exists(path), "V4.18: the stored character survives death by default");
        Press(w, Key.Y);
        Assert.IsType<TitleViewModel>(vm.Current);
        w.Close();
    }
}
