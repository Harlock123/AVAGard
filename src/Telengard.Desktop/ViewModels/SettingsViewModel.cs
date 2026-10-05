using System.Collections.ObjectModel;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Telengard.Core.Engine;
using Telengard.Desktop.Input;
using Telengard.Desktop.Services;
using Telengard.Infrastructure.Settings;

namespace Telengard.Desktop.ViewModels;

public sealed partial class BindingRow(Command command, string keys, Action<BindingRow> rebind) : ObservableObject
{
    public Command Command => command;
    [ObservableProperty] private string _keys = keys;
    [ObservableProperty] private bool _isListening;
    [RelayCommand] private void Rebind() => rebind(this);
}

public sealed partial class SettingsViewModel : ViewModelBase
{
    private readonly AppServices _services;
    private readonly MainViewModel _main;
    private readonly GameSettings _s;
    private BindingRow? _listening;

    public SettingsViewModel(AppServices services, MainViewModel main)
    {
        _services = services;
        _main = main;
        _s = services.Settings.Clone();
        _modernDeliberate = _s.ModernDeadlineSeconds is null;
        _modernSeconds = _s.ModernDeadlineSeconds ?? 10;
        _classicPacing = _s.ClassicPacing;
        _modernPacing = _s.ModernPacing;
        _uiScale = _s.UiScale;
        _highContrast = _s.HighContrast;
        _reducedMotion = _s.ReducedMotion;
        _showAutomap = _s.ShowAutomap;
        _manualDeathRule = _s.ManualDeathRule;
        _soundEnabled = _s.SoundEnabled;
        _musicEnabled = _s.MusicEnabled;
        _ambienceEnabled = _s.AmbienceEnabled;
        _masterVolume = _s.MasterVolume;
        _effectsVolume = _s.EffectsVolume;
        _musicVolume = _s.MusicVolume;
        _ambienceVolume = _s.AmbienceVolume;
        EffectsPacks = services.SoundPacks.Where(p => p.HasSounds).Select(p => new PackChoice(p.Id, $"{p.Name} — {p.License}")).ToList();
        MusicPacks = services.SoundPacks.Where(p => p.HasMusic).Select(p => new PackChoice(p.Id, $"{p.Name} — {p.License}")).ToList();
        _effectsPack = EffectsPacks.FirstOrDefault(p => p.Id == _s.EffectsPack) ?? EffectsPacks.FirstOrDefault();
        _musicPack = MusicPacks.FirstOrDefault(p => p.Id == _s.MusicPack) ?? MusicPacks.FirstOrDefault();
        BufferChoices = Telengard.Audio.AudioBuffer.Choices.Select(c => new PackChoice(c.Id, c.Label)).ToList();
        _audioBuffer = BufferChoices.FirstOrDefault(c => c.Id == _s.AudioBuffer) ?? BufferChoices[0];
        AudioStatus = services.AudioProblem ?? "Sound is working.";
        LoadBindings();
    }

    public sealed record PackChoice(string Id, string Label)
    {
        public override string ToString() => Label;
    }

    public IReadOnlyList<PackChoice> EffectsPacks { get; }
    public IReadOnlyList<PackChoice> MusicPacks { get; }
    public IReadOnlyList<PackChoice> BufferChoices { get; }
    public string AudioStatus { get; }

    [ObservableProperty] private bool _soundEnabled;
    [ObservableProperty] private bool _musicEnabled;
    [ObservableProperty] private bool _ambienceEnabled;
    [ObservableProperty] private double _masterVolume;
    [ObservableProperty] private double _effectsVolume;
    [ObservableProperty] private double _musicVolume;
    [ObservableProperty] private double _ambienceVolume;
    [ObservableProperty] private PackChoice? _effectsPack;
    [ObservableProperty] private PackChoice? _musicPack;
    [ObservableProperty] private PackChoice? _audioBuffer;

    // Volume changes are heard immediately; Cancel restores the saved values.
    partial void OnMasterVolumeChanged(double value) => Preview();
    partial void OnEffectsVolumeChanged(double value) => Preview();
    partial void OnMusicVolumeChanged(double value) => Preview();
    partial void OnAmbienceVolumeChanged(double value) => Preview();

    private void Preview() =>
        _services.Audio.SetVolumes((float)MasterVolume, (float)EffectsVolume, (float)MusicVolume);

    [ObservableProperty] private bool _modernDeliberate;
    [ObservableProperty] private double _modernSeconds;
    [ObservableProperty] private double _classicPacing;
    [ObservableProperty] private double _modernPacing;
    [ObservableProperty] private double _uiScale;
    [ObservableProperty] private bool _highContrast;
    [ObservableProperty] private bool _reducedMotion;
    [ObservableProperty] private bool _showAutomap;
    [ObservableProperty] private bool _manualDeathRule;
    [ObservableProperty] private string? _status;

    public ObservableCollection<BindingRow> Bindings { get; } = [];

    private void LoadBindings()
    {
        Bindings.Clear();
        var map = new KeyMap(_s.KeyBindings);
        foreach (var command in KeyMap.DefaultDungeonKeys.Keys)
            Bindings.Add(new BindingRow(command, string.Join(", ", map.KeysFor(command)), StartListening));
    }

    private void StartListening(BindingRow row)
    {
        if (_listening is not null) _listening.IsListening = false;
        _listening = row;
        row.IsListening = true;
        Status = $"Press a key for {row.Command} (Escape to cancel).";
    }

    public bool HandleKey(Key key)
    {
        if (_listening is null)
        {
            if (key == Key.Escape) { Cancel(); return true; }
            return false;
        }
        var row = _listening;
        _listening = null;
        row.IsListening = false;
        if (key == Key.Escape) { Status = null; return true; }
        // Remove the key from any other command so one key has one meaning.
        foreach (var (cmd, names) in _s.KeyBindings.ToList())
            _s.KeyBindings[cmd] = names.Where(n => n != key.ToString()).ToArray();
        _s.KeyBindings[row.Command.ToString()] = [key.ToString()];
        foreach (var other in KeyMap.DefaultDungeonKeys.Keys.Where(c => c != row.Command && !_s.KeyBindings.ContainsKey(c.ToString())))
        {
            var defaults = KeyMap.DefaultDungeonKeys[other].Select(k => k.ToString()).ToArray();
            if (defaults.Contains(key.ToString())) _s.KeyBindings[other.ToString()] = defaults.Where(k => k != key.ToString()).ToArray();
        }
        LoadBindings();
        Status = $"{row.Command} is now on {key} (Modern mode; Classic always uses the original keys).";
        return true;
    }

    [RelayCommand]
    private void ResetBindings()
    {
        _s.KeyBindings.Clear();
        LoadBindings();
        Status = "Key bindings reset to defaults.";
    }

    [RelayCommand]
    private async Task Save()
    {
        _s.ModernDeadlineSeconds = ModernDeliberate ? null : Math.Clamp(Math.Round(ModernSeconds, 1), 2, 15);
        _s.ClassicPacing = Math.Clamp(ClassicPacing, 0, 2);
        _s.ModernPacing = Math.Clamp(ModernPacing, 0, 2);
        _s.UiScale = Math.Clamp(UiScale, 0.75, 2.5);
        _s.HighContrast = HighContrast;
        _s.ReducedMotion = ReducedMotion;
        _s.ShowAutomap = ShowAutomap;
        _s.ManualDeathRule = ManualDeathRule;
        _s.SoundEnabled = SoundEnabled;
        _s.MusicEnabled = MusicEnabled;
        _s.AmbienceEnabled = AmbienceEnabled;
        _s.MasterVolume = Math.Clamp(MasterVolume, 0, 1);
        _s.EffectsVolume = Math.Clamp(EffectsVolume, 0, 1);
        _s.MusicVolume = Math.Clamp(MusicVolume, 0, 1);
        _s.AmbienceVolume = Math.Clamp(AmbienceVolume, 0, 1);
        if (EffectsPack is not null) _s.EffectsPack = EffectsPack.Id;
        if (MusicPack is not null) _s.MusicPack = MusicPack.Id;
        if (AudioBuffer is not null) _s.AudioBuffer = AudioBuffer.Id;
        _services.Settings = _s;
        _services.ApplyAudioSettings();
        _main.UiScale = _s.UiScale;
        await _services.SaveSettingsAsync();
        _main.ShowTitle();
    }

    [RelayCommand]
    private void Cancel()
    {
        _services.ApplyAudioSettings(); // undo any volume preview
        _main.ShowTitle();
    }
}
