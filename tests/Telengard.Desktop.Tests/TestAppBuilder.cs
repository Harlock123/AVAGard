using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Telengard.Desktop.Tests.TestAppBuilder))]

namespace Telengard.Desktop.Tests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<Telengard.Desktop.App>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
