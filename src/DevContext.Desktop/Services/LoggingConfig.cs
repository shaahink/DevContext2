using System.IO;
using Serilog;
using Serilog.Events;

namespace DevContext.Desktop.Services;

public static class LoggingConfig
{
    public static void Init(string logDir)
    {
        Directory.CreateDirectory(logDir);

        var logConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(logDir, "devcontext.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                Path.Combine(logDir, "crash.log"),
                restrictedToMinimumLevel: LogEventLevel.Error,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message:lj}{NewLine}{Exception}{NewLine}---{NewLine}");

        Log.Logger = logConfig.CreateLogger();
    }
}
