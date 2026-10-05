using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Telengard.Desktop.ViewModels;

namespace Telengard.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly HashSet<Key> _down = [];

    public MainWindow()
    {
        InitializeComponent();
        // Tunnel so game keys are seen before focused buttons or text boxes act on them.
        AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
        AddHandler(KeyUpEvent, (_, e) => _down.Remove(e.Key), RoutingStrategies.Tunnel);
        Deactivated += (_, _) => _down.Clear();
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        // Ignore auto-repeat from a held key during play: one press, one action.
        bool repeat = !_down.Add(e.Key);
        if (repeat && vm.Current is GameViewModel)
        {
            e.Handled = true;
            return;
        }
        if (vm.HandleKey(e.Key)) e.Handled = true;
    }
}
