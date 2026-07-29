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

        // Wait for the server to be ready.
        //
        // G1.3: this was 30s, and 30s is not enough on a busy machine. Measured, from this
        // process's own log during a full `dotnet test DevContext.slnx` run (671 tests in
        // parallel):
        //   05:08:13 Starting DevContext server: …\DevContext.Server.exe
        //   05:08:44 DevContext server did not become ready within 30s
        //   05:08:46 Cannot reach DevContext server … (FATAL, Program.cs returns 1)
        // The MCP then EXITS, so the client's `initialize` handshake gets no answer at all. Nothing
        // was wrong with the product — the shim simply ran out of patience while an ASP.NET Core
        // host cold-started under load — but the MCP QA gate goes red and reads as a catastrophic
        // regression. It cost three sessions before the log above was read.
        //
        // A real user meets the same wall: the first MCP call after a reboot, on a machine that is
        // also compiling something, kills the MCP outright. Waiting longer costs nothing when the
        // server is quick (the loop exits as soon as /health answers) and everything when it is not.
        var startupBudget = TimeSpan.FromSeconds(120);
        var deadline = DateTime.UtcNow.Add(startupBudget);
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

        Serilog.Log.Warning("DevContext server did not become ready within {Budget}s (exited={Exited})",
            startupBudget.TotalSeconds, process.HasExited ? process.ExitCode.ToString() : "no");
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
                // Search all build configurations in dev layout (recursively)
                var serverProject = Path.Combine(dir, "src", "DevContext.Server");
                if (Directory.Exists(serverProject))
                {
                    foreach (var exe in Directory.GetFiles(serverProject, "DevContext.Server.exe", SearchOption.AllDirectories))
                    {
                        if (!exe.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar)
                            && !exe.Contains(Path.DirectorySeparatorChar + "ref" + Path.DirectorySeparatorChar))
                            return exe;
                    }
                }
                break;
            }
            dir = Path.GetDirectoryName(dir);
        }

        return null;
    }
}
