using System.IO;
using System.Windows;
using System.Windows.Threading;
using Serilog;
using DevContext.Desktop.Services;

namespace DevContext.Desktop;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DevContext");

        LoggingConfig.Init(logDir);

        // Global AppDomain exception handler
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            Log.Fatal(ex, "Unhandled AppDomain exception");
            Log.CloseAndFlush();
        };

        // Unobserved task exceptions
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };

        Log.Information("DevContext Desktop v{Version} starting", DevContext.Cli.DevContextVersion.Display);

        try
        {
            var app = new Application();

            // WPF dispatcher exception handler (must be after Application creation)
            app.DispatcherUnhandledException += (_, e) =>
            {
                Log.Fatal(e.Exception, "Unhandled WPF dispatcher exception");
                e.Handled = true;
            };

            app.Run(new MainWindow());
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal startup exception");
        }
        finally
        {
            Log.Information("DevContext Desktop shutting down");
            Log.CloseAndFlush();
        }
    }
}
