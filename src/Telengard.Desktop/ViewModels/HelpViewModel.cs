using CommunityToolkit.Mvvm.Input;
using Telengard.Core.Content;

namespace Telengard.Desktop.ViewModels;

public sealed partial class HelpViewModel(MainViewModel main) : ViewModelBase
{
    public IReadOnlyList<SpellInfo> SpellList => Spells.All;
    [RelayCommand] private void Back() => main.ShowTitle();
    public bool Close() { main.ShowTitle(); return true; }
}
