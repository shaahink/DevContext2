using Microsoft.Extensions.DependencyInjection;
using DevContext.Desktop.ViewModels;

namespace DevContext.Desktop;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        var services = new ServiceCollection();
        services.AddWpfBlazorWebView();
        services.AddSingleton<MainViewModel>();
        Resources["services"] = services.BuildServiceProvider();
    }
}
