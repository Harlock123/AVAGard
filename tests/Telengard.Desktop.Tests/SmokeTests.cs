using Avalonia.Headless.XUnit;
using Telengard.Desktop.ViewModels;
using Telengard.Desktop.Views;

namespace Telengard.Desktop.Tests;

public class SmokeTests
{
    [AvaloniaFact]
    public void MainWindowOpensHeadless()
    {
        var window = new MainWindow { DataContext = new MainViewModel() };
        window.Show();
        Assert.True(window.IsVisible);
        window.Close();
    }
}
