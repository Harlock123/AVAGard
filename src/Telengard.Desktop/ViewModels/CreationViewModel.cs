using System.Collections.ObjectModel;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Telengard.Core.Engine;
using Telengard.Core.Model;
using Telengard.Core.Randomness;
using Telengard.Desktop.Services;

namespace Telengard.Desktop.ViewModels;

/// <summary>
/// Lines 1400-1630. Classic: a new set of attributes appears whenever the input deadline passes
/// or any key other than RETURN is pressed ("cycled periodically", as the manual says).
/// Modern: explicit Reroll / Accept buttons, no automatic cycling.
/// </summary>
public sealed partial class CreationViewModel : ViewModelBase, IDisposable
{
    private readonly AppServices _services;
    private readonly MainViewModel _main;
    private readonly SplitMix64Random _rng = new((ulong)Random.Shared.NextInt64());
    private readonly DispatcherTimer? _cycle;

    public CreationViewModel(AppServices services, MainViewModel main, Preset preset)
    {
        _services = services;
        _main = main;
        Preset = preset;
        Roll();
        if (preset == Preset.Classic)
        {
            _cycle = new DispatcherTimer(RulesPolicy.ClassicDeadline, DispatcherPriority.Normal, (_, _) => { if (!IsNaming) Roll(); });
            _cycle.Start();
        }
    }

    public Preset Preset { get; }
    public bool IsClassic => Preset == Preset.Classic;
    public string Heading => IsClassic ? "Classic · new character" : "Modern · new character";
    public string Instructions => IsClassic
        ? "Sets of attributes appear every few seconds. Press any key for another set, RETURN to keep this one."
        : "Reroll until you like the attributes, then accept. Constitution sets your starting hit points.";

    public ObservableCollection<StatLine> Stats { get; } = [];
    private int[] _stats = new int[6];

    [ObservableProperty] private bool _isNaming;
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private int _rolls;

    public string NameHint => "Letters and digits, up to 30 (7 are shown). Leave empty for DEMO: no time limit, cannot be stored."
        + (_services.Settings.ManualDeathRule ? " Manual death rule is on: start the name with SV to keep the stored character after death." : "");

    [RelayCommand]
    private void Roll()
    {
        if (IsNaming) return;
        _stats = CharacterCreation.RollStats(_rng);
        Rolls++;
        Stats.Clear();
        for (int i = 0; i < 6; i++) Stats.Add(new StatLine(Stat.Abbreviations[i], _stats[i].ToString(), Stat.Names[i]));
    }

    [RelayCommand]
    private void Accept()
    {
        if (!IsNaming)
        {
            IsNaming = true;
            _cycle?.Stop();
            return;
        }
        var state = CharacterCreation.NewGame(Name, _stats, _rng);
        state.RandomState = _rng.State;
        _main.StartGame(state, _main.PolicyFor(Preset), StartPoint.NewCharacter);
    }

    [RelayCommand] private void Back() => _main.ShowTitle();

    public bool HandleKey(Key key)
    {
        if (key == Key.Escape) { Back(); return true; }
        if (IsNaming)
        {
            if (key is Key.Enter or Key.Return) { Accept(); return true; }
            return false; // the name box handles typing
        }
        if (key is Key.Enter or Key.Return) { Accept(); return true; }
        if (IsClassic && key is not (Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl or Key.Tab))
        {
            Roll();
            return true;
        }
        if (key == Key.R) { Roll(); return true; }
        return false;
    }

    public void Dispose() => _cycle?.Stop();
}
