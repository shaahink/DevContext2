using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using DevContext.Desktop.ViewModels;
using Serilog;

namespace DevContext.Desktop;

public partial class MainWindow
{
    private static readonly string BootstrapperUrl =
        "https://go.microsoft.com/fwlink/p/?LinkId=2124703";

    private static readonly string UserDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DevContext", "WebView2");

    private ServiceProvider? _services;

    public MainWindow()
    {
        Log.Information("Initializing MainWindow...");

        try
        {
            var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
            if (string.IsNullOrEmpty(version))
            {
                Log.Warning("WebView2 runtime not found — downloading Evergreen bootstrapper...");
                InstallWebView2Sync();
                Log.Information("WebView2 runtime installed successfully");
            }
            else
            {
                Log.Information("WebView2 runtime version: {Version}", version);
            }

            InitializeComponent();

            var services = new ServiceCollection();
            services.AddWpfBlazorWebView();
            services.AddSingleton<MainViewModel>();
            _services = services.BuildServiceProvider();
            Resources["services"] = _services;

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

    protected override void OnClosed(EventArgs e)
    {
        // Dispose the singleton MainViewModel (owns CancellableOperations, Debouncer, GitCloneService)
        // and the DI container before the window is fully torn down.
        try
        {
            if (_services?.GetService(typeof(MainViewModel)) is MainViewModel vm)
                vm.Dispose();
            _services?.Dispose();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error during MainWindow disposal");
        }
        base.OnClosed(e);
    }

    private static void InstallWebView2Sync()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DevContext");
        Directory.CreateDirectory(tempDir);
        var installer = Path.Combine(tempDir, "MicrosoftEdgeWebview2Setup.exe");

        // Download the Evergreen bootstrapper (~2 MB)
        using var client = new HttpClient();
        using var response = client.GetAsync(BootstrapperUrl).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        using var fs = new FileStream(installer, FileMode.Create, FileAccess.Write);
        response.Content.CopyToAsync(fs).GetAwaiter().GetResult();
        fs.Close();

        // Run the bootstrapper silently — it installs WebView2 Evergreen Runtime
        var psi = new ProcessStartInfo
        {
            FileName = installer,
            Arguments = "/silent /install",
            UseShellExecute = true,
            CreateNoWindow = true,
        };

        var process = Process.Start(psi)!;
        process.WaitForExit();

        // Clean up
        try { File.Delete(installer); } catch { /* best effort */ }

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"WebView2 installation failed with exit code {process.ExitCode}");
    }
}
