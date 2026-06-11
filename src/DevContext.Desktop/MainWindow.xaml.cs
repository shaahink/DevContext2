using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using DevContext.Desktop.ViewModels;

namespace DevContext.Desktop;

public partial class MainWindow
{
    public MainWindow()
    {
        try
        {
            // Check WebView2 runtime availability
            var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
            if (string.IsNullOrEmpty(version))
            {
                MessageBox.Show(
                    "WebView2 runtime is required to run this application.\n\n" +
                    "Click OK to open the Microsoft download page.",
                    "WebView2 Runtime Required",
                    MessageBoxButton.OK, MessageBoxImage.Information);
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
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to start DevContext:\n\n{ex.Message}\n\n{ex.StackTrace}",
                "Startup Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
    }
}
