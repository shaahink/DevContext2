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
                // Critical exceptions (StackOverflow, OutOfMemory, ThreadAbort) should not be
                // swallowed — mark non-critical exceptions as handled so the app can recover.
                var isCritical = e.Exception is StackOverflowException or OutOfMemoryException or ThreadAbortException;
                if (isCritical)
                {
                    Log.Fatal(e.Exception, "Critical unhandled dispatcher exception — terminating");
                    Log.CloseAndFlush();
                    // Let the default crash behavior take over.
                    e.Handled = false;
                }
                else
                {
                    Log.Error(e.Exception, "Unhandled WPF dispatcher exception (suppressed to keep app alive)");
                    e.Handled = true;
                }
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
