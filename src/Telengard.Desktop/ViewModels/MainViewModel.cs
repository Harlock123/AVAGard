using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Telengard.Core.Engine;
using Telengard.Core.Model;
using Telengard.Desktop.Services;

namespace Telengard.Desktop.ViewModels;

/// <summary>Screen host: title → creation → game, plus settings.</summary>
public sealed partial class MainViewModel : ViewModelBase
{
    public MainViewModel(AppServices services)
    {
        Services = services;
        _current = new TitleViewModel(services, this);
    }

    public AppServices Services { get; }

    [ObservableProperty] private ViewModelBase _current;
    [ObservableProperty] private double _uiScale = 1;

    public event Action? ExitRequested;

    partial void OnCurrentChanged(ViewModelBase? oldValue, ViewModelBase newValue)
    {
        if (oldValue is IDisposable d) d.Dispose();
    }

    public void ShowTitle() => Current = new TitleViewModel(Services, this);
    public void ShowSettings() => Current = new SettingsViewModel(Services, this);
    public void ShowCreation(Preset preset) => Current = new CreationViewModel(Services, this, preset);
    public void ShowHelp() => Current = new HelpViewModel(this);
    public void RequestExit() => ExitRequested?.Invoke();

    public RulesPolicy PolicyFor(Preset preset, bool? manualDeathRule = null, double? deadlineSeconds = null, bool useStoredDeadline = false)
    {
        var s = Services.Settings;
        if (preset == Preset.Classic)
            return RulesPolicy.Classic() with { ManualDeathRule = manualDeathRule ?? s.ManualDeathRule };
        double? seconds = useStoredDeadline ? deadlineSeconds : s.ModernDeadlineSeconds;
        return RulesPolicy.Modern(seconds is { } v ? TimeSpan.FromSeconds(v) : null) with { ManualDeathRule = manualDeathRule ?? s.ManualDeathRule };
    }

    public void StartGame(GameState state, RulesPolicy policy, StartPoint start) =>
        Current = new GameViewModel(Services, this, state, policy, start);

    /// <summary>Routes a key to the current screen.</summary>
    public bool HandleKey(Key key) => Current switch
    {
        GameViewModel g => g.HandleKey(key),
        CreationViewModel c => c.HandleKey(key),
        HelpViewModel h when key == Key.Escape => h.Close(),
        SettingsViewModel s => s.HandleKey(key),
        _ => false,
    };
}
