using Avalonia.Controls;
using Avalonia.Threading;
using Telengard.Desktop.ViewModels;

namespace Telengard.Desktop.Views;

public partial class GameView : UserControl
{
    public GameView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is GameViewModel vm)
                vm.Log.CollectionChanged += (_, _) => Dispatcher.UIThread.Post(() => LogScroller.ScrollToEnd(), DispatcherPriority.Background);
        };
    }
}
