using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using DevContext.Desktop.ViewModels;
using Serilog;

namespace DevContext.Desktop;

public partial class MainWindow
{
    public MainWindow()
    {
        Log.Information("Initializing MainWindow...");

        try
        {
            var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
            if (string.IsNullOrEmpty(version))
            {
                Log.Warning("WebView2 runtime not found");
                MessageBox.Show("WebView2 runtime is required.\n\nClick OK to open the download page.",
                    "WebView2 Runtime Required", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://developer.microsoft.com/microsoft-edge/webview2/",
                    UseShellExecute = true
                })?.Dispose();
                Application.Current.Shutdown();
                return;
            }
            Log.Information("WebView2 runtime version: {Version}", version);

            InitializeComponent();

            var services = new ServiceCollection();
            services.AddWpfBlazorWebView();
            services.AddSingleton<MainViewModel>();
            Resources["services"] = services.BuildServiceProvider();

            Log.Information("MainWindow initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to initialize MainWindow");
            MessageBox.Show($"Failed to start:\n\n{ex.Message}", "Startup Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
    }
}
