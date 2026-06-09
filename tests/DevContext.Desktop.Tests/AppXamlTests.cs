using Avalonia;
using Avalonia.Headless;

namespace DevContext.Desktop.Tests;

public class AppXamlTests
{
    static AppXamlTests()
    {
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .SetupWithoutStarting();
    }

    [Fact]
    public void App_xaml_resources_resolve_without_error()
    {
        var app = new App();
        var ex = Record.Exception(() => app.Initialize());
        Assert.Null(ex);
    }
}
