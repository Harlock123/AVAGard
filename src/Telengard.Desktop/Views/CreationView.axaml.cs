using Avalonia.Controls;
using Avalonia.Threading;
using Telengard.Desktop.ViewModels;

namespace Telengard.Desktop.Views;

public partial class CreationView : UserControl
{
    public CreationView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is CreationViewModel vm)
                vm.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(CreationViewModel.IsNaming) && vm.IsNaming)
                        Dispatcher.UIThread.Post(() => NameBox.Focus());
                };
        };
    }
}
