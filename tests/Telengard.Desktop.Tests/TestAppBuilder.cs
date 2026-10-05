using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Telengard.Desktop.Tests.TestAppBuilder))]

namespace Telengard.Desktop.Tests;

public class TestAppBuilder
{
    // Skia rendering (instead of headless drawing) so frames can be captured for inspection.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseSkia()
            .WithInterFont()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
