using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Telengard.Core.Engine;
using Telengard.Desktop.Services;
using Telengard.Infrastructure.Saves;

namespace Telengard.Desktop.ViewModels;

public sealed partial class SaveEntry(SaveSummary summary, Action<SaveSummary> open) : ObservableObject
{
    public SaveSummary Summary => summary;
    public string Title => summary.IsUsable
        ? $"{summary.CharacterName}  ·  level {summary.Level}  ·  {(summary.Depth == 0 ? "at an inn" : $"dungeon level {summary.Depth}")}"
        : $"{summary.CharacterName}  ·  unreadable";
    public string Detail => summary.IsUsable
        ? $"{summary.Preset} · {(summary.Kind == SaveKind.Suspend ? "resume point" : "stored character")} · {summary.SavedAt.LocalDateTime:g}"
        : summary.Problem!;
    public bool IsUsable => summary.IsUsable;
    [RelayCommand] private void Open() => open(summary);
}

public sealed partial class TitleViewModel : ViewModelBase
{
    private readonly AppServices _services;
    private readonly MainViewModel _main;

    public TitleViewModel(AppServices services, MainViewModel main)
    {
        _services = services;
        _main = main;
        Message = services.StartupProblem;
        Refresh();
    }

    public ObservableCollection<SaveEntry> Saves { get; } = [];
    [ObservableProperty] private string? _message;
    public bool HasSaves => Saves.Count > 0;

    private void Refresh()
    {
        Saves.Clear();
        try
        {
            foreach (var s in _services.Saves.List()) Saves.Add(new SaveEntry(s, Open));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Message = "Could not read the save folder: " + e.Message;
        }
        OnPropertyChanged(nameof(HasSaves));
    }

    [RelayCommand] private void NewClassic() => _main.ShowCreation(Preset.Classic);
    [RelayCommand] private void NewModern() => _main.ShowCreation(Preset.Modern);
    [RelayCommand] private void Settings() => _main.ShowSettings();
    [RelayCommand] private void Help() => _main.ShowHelp();
    [RelayCommand] private void Quit() => _main.RequestExit();

    private async void Open(SaveSummary summary)
    {
        if (!summary.IsUsable)
        {
            Message = $"{summary.CharacterName}: {summary.Problem}";
            return;
        }
        try
        {
            var loaded = await Task.Run(() => _services.Saves.LoadAsync(summary.Path));
            var f = loaded.File;
            var policy = _main.PolicyFor(f.Preset, f.ManualDeathRule, f.DeadlineSeconds, useStoredDeadline: true);
            _main.StartGame(loaded.State, policy, f.Kind == SaveKind.Suspend ? StartPoint.Suspended : StartPoint.StoredCharacter);
        }
        catch (SaveLoadException e)
        {
            Message = $"{summary.CharacterName}: {e.Message}";
            Refresh();
        }
    }
}
