using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using DevContext.Desktop.ViewModels;

namespace DevContext.Desktop;

public partial class MainWindow
{
    private readonly string _logPath;

    public MainWindow()
    {
        _logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DevContext", "blazor.log");
        Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
        Log("Starting DevContext Desktop (Blazor Hybrid)...");

        try
        {
            var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
            Log($"WebView2 runtime: {version ?? "NOT FOUND"}");
            if (string.IsNullOrEmpty(version))
            {
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

            InitializeComponent();

            var services = new ServiceCollection();
            services.AddWpfBlazorWebView();
            services.AddSingleton<MainViewModel>();
            Resources["services"] = services.BuildServiceProvider();

            Log("Initialized successfully");
        }
        catch (Exception ex)
        {
            Log($"FATAL: {ex}");
            MessageBox.Show($"Failed to start:\n\n{ex.Message}", "Startup Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
    }

    private void Log(string msg)
    {
        try { File.AppendAllText(_logPath, $"{DateTime.Now:HH:mm:ss.fff} {msg}\n"); }
        catch { /* best effort */ }
    }
}
