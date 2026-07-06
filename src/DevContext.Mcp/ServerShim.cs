using System.Diagnostics;
using System.Net.Http.Json;

namespace DevContext.Mcp;

internal static class ServerShim
{
    public static Process? EnsureServerRunning(string endpoint)
    {
        // Check if server is already running
        using var pingClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        try
        {
            var response = pingClient.GetAsync($"{endpoint}/health").Result;
            if (response.IsSuccessStatusCode)
            {
                Serilog.Log.Information("DevContext server already running at {Endpoint}", endpoint);
                return null;
            }
        }
        catch
        {
            // Not running — will spawn
        }

        // Find the server executable
        var serverPath = FindServerExe();
        if (serverPath is null)
        {
            Serilog.Log.Warning("Could not find DevContext.Server.exe — start the server manually at {Endpoint}", endpoint);
            return null;
        }

        Serilog.Log.Information("Starting DevContext server: {Path}", serverPath);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = serverPath,
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            },
        };

        process.Start();

        // Wait for server to be ready (retry logic)
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var check = new HttpClient { Timeout = TimeSpan.FromSeconds(1) };
                var response = check.GetAsync($"{endpoint}/health").Result;
                if (response.IsSuccessStatusCode)
                {
                    Serilog.Log.Information("DevContext server ready at {Endpoint}", endpoint);
                    return process;
                }
            }
            catch
            {
                // Not ready yet
            }

            Thread.Sleep(500);
        }

        Serilog.Log.Warning("DevContext server did not become ready within 30s");
        try { process.Kill(entireProcessTree: true); process.Dispose(); } catch { /* already exited */ }
        return null;
    }

    private static string? FindServerExe()
    {
        // G7 — multi-source discovery (priority order)

        // 1. Environment variable override (DEVCONTEXT_SERVER)
        var envPath = Environment.GetEnvironmentVariable("DEVCONTEXT_SERVER");
        if (!string.IsNullOrEmpty(envPath) && File.Exists(envPath))
            return envPath;

        var mcpDir = AppContext.BaseDirectory;

        // 2. Sibling or nearby published layout (same dir as MCP exe, or parent/child)
        var siblingPaths = new[]
        {
            Path.Combine(mcpDir, "DevContext.Server.exe"),
            Path.Combine(mcpDir, "server", "DevContext.Server.exe"),
            Path.Combine(Path.GetDirectoryName(mcpDir) ?? mcpDir, "DevContext.Server", "DevContext.Server.exe"),
        };
        foreach (var p in siblingPaths)
            if (File.Exists(p)) return p;

        // 3. User-local install: %LOCALAPPDATA%/DevContext/server/
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var userInstall = Path.Combine(localAppData, "DevContext", "server", "DevContext.Server.exe");
        if (File.Exists(userInstall)) return userInstall;

        // 4. Walk up to find the solution root and search build outputs (dev layout)
        var dir = mcpDir;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "DevContext.slnx")))
            {
                // Search all build configurations in dev layout
                var serverProject = Path.Combine(dir, "src", "DevContext.Server");
                if (Directory.Exists(serverProject))
                {
                    foreach (var binDir in Directory.GetDirectories(serverProject, "bin", SearchOption.AllDirectories))
                    {
                        var exe = Path.Combine(binDir, "DevContext.Server.exe");
                        if (File.Exists(exe)) return exe;
                    }
                }
                break;
            }
            dir = Path.GetDirectoryName(dir);
        }

        return null;
    }
}
