using System.Text.Json;
using System.Diagnostics;
using System.Text;
using Photino.NET;

namespace DevContext.Desktop.Bridge;

public class DevContextBridge
{
    private readonly PhotinoWindow _window;
    private readonly string _dataDir;

    public DevContextBridge(PhotinoWindow window)
    {
        _window = window;
        _dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DevContext");
        Directory.CreateDirectory(_dataDir);
    }

    public void HandleMessage(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            var id = root.GetProperty("id").GetString() ?? "";
            var type = root.GetProperty("type").GetString() ?? "";

            switch (type)
            {
                case "pick-folder":
                    Reply(id, PickFolder());
                    break;
                case "pick-file":
                    Reply(id, PickSln());
                    break;
                case "save-file":
                    SaveFile(root, id);
                    break;
                case "get-settings":
                    Reply(id, LoadSettings());
                    break;
                case "save-settings":
                    SaveSettings(root);
                    Reply(id, true);
                    break;
                case "get-recent":
                    Reply(id, LoadRecent());
                    break;
                case "get-version":
                    Reply(id, "v2.0.0");
                    break;
                case "analyze":
                    _ = AnalyzeAsync(id, root);
                    break;
            }
        }
        catch (Exception ex)
        {
            Send("error", new { error = ex.Message });
        }
    }

    public void Send(string type, object data)
    {
        var json = JsonSerializer.Serialize(new { type, data },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _window.SendWebMessage(json);
    }

    private async Task AnalyzeAsync(string id, JsonElement root)
    {
        try
        {
            var p = root.GetProperty("data");
            var path = p.GetProperty("path").GetString() ?? "";
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                Send("error", new { id, error = "Path does not exist" });
                return;
            }

            AddRecent(path);

            // Build CLI args
            var scenario = p.GetProperty("scenario").GetString() ?? "architecture";
            var profile = p.GetProperty("profile").GetString() ?? "focused";
            var around = p.TryGetProperty("around", out var a) ? a.GetString()?.Trim() : "";
            var maxTokens = p.TryGetProperty("maxTokens", out var mt) ? mt.GetInt32() : 8000;
            var format = p.TryGetProperty("format", out var f) ? f.GetString() ?? "markdown" : "markdown";
            var provenance = p.TryGetProperty("includeProvenance", out var ip) && ip.GetBoolean();
            var diagnostics = p.TryGetProperty("includeDiagnostics", out var idg) && idg.GetBoolean();
            var noRoslyn = p.TryGetProperty("noRoslyn", out var nr) && nr.GetBoolean();
            var dryRun = p.TryGetProperty("dryRun", out var dr) && dr.GetBoolean();

            var args = new List<string> { "analyze", $"\"{path}\"", "--scenario", scenario, "--profile", profile };
            if (!string.IsNullOrWhiteSpace(around)) args.AddRange(["--around", $"\"{around}\""]);
            args.AddRange(["--max-tokens", maxTokens.ToString(), "--format", format]);
            if (provenance) args.Add("--include-provenance");
            if (diagnostics) args.Add("--include-diagnostics");
            if (noRoslyn) args.Add("--no-roslyn");
            if (dryRun) args.Add("--dry-run");

            var outFile = Path.Combine(Path.GetTempPath(), $"devcontext-{Guid.NewGuid():N}.{format}");
            args.AddRange(["-o", $"\"{outFile}\""]);

            // Find the CLI DLL
            var cliDir = Path.GetDirectoryName(typeof(DevContextBridge).Assembly.Location) ?? AppContext.BaseDirectory;
            var cliDll = Path.Combine(cliDir, "DevContext.Cli.dll");

            Send("progress", new { id, stage = "launched" });

            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{cliDll}\" {string.Join(" ", args)}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var process = new Process { StartInfo = psi };
            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) error.AppendLine(e.Data); };

            var sw = Stopwatch.StartNew();
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            sw.Stop();

            if (process.ExitCode != 0 || !File.Exists(outFile))
            {
                Send("error", new { id, error = error.Length > 0 ? error.ToString() : "Analysis failed" });
                return;
            }

            var content = await File.ReadAllTextAsync(outFile);
            try { File.Delete(outFile); } catch { }

            Send("done", new
            {
                id,
                content,
                tokens = content.Length / 4,
                timeMs = sw.ElapsedMilliseconds,
            });
        }
        catch (Exception ex)
        {
            Send("error", new { id, error = $"Analysis failed: {ex.Message}" });
        }
    }

    private string? PickFolder()
    {
        var result = _window.ShowOpenFile("Select .NET project folder", "", false);
        return result?.FirstOrDefault();
    }

    private string? PickSln()
    {
        var result = _window.ShowOpenFile("Select .sln or .csproj", "Solution/Project|*.sln;*.slnx;*.csproj", false);
        return result?.FirstOrDefault();
    }

    private void SaveFile(JsonElement root, string id)
    {
        var data = root.GetProperty("data");
        var content = data.GetProperty("content").GetString() ?? "";
        var ext = data.TryGetProperty("ext", out var e) ? e.GetString() : "md";
        var filter = ext == "json" ? "JSON (*.json)|*.json" : "Markdown (*.md)|*.md";
        var path = _window.ShowSaveFile("Save output", filter);
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, content);
            Reply(id, true);
        }
    }

    private void Reply(string id, object? data) => Send("reply", new { id, data });

    private AppSettings LoadSettings()
    {
        var path = Path.Combine(_dataDir, "settings.json");
        if (!File.Exists(path)) return new();
        return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new();
    }

    private void SaveSettings(JsonElement root)
    {
        var data = root.GetProperty("data").GetProperty("settings");
        var s = new AppSettings
        {
            LastScenario = data.TryGetProperty("scenario", out var sc) ? sc.GetString() : "architecture",
            LastProfile = data.TryGetProperty("profile", out var pr) ? pr.GetString() : "focused",
            LastFormat = data.TryGetProperty("format", out var fm) ? fm.GetString() : "markdown",
            LastTokens = data.TryGetProperty("tokens", out var tk) ? tk.GetInt32() : 8000,
            LastAround = data.TryGetProperty("around", out var ar) ? ar.GetString() : "",
            IncludeProvenance = data.TryGetProperty("provenance", out var ipp) && ipp.GetBoolean(),
            IncludeDiagnostics = data.TryGetProperty("diagnostics", out var diag) && diag.GetBoolean(),
            NoRoslyn = data.TryGetProperty("noRoslyn", out var nr) && nr.GetBoolean(),
            Theme = data.TryGetProperty("theme", out var th) ? th.GetString() : "dark",
        };
        File.WriteAllText(Path.Combine(_dataDir, "settings.json"),
            JsonSerializer.Serialize(s, new JsonSerializerOptions { WriteIndented = true }));
    }

    private string[] LoadRecent()
    {
        var p = Path.Combine(_dataDir, "recent.json");
        return File.Exists(p)
            ? JsonSerializer.Deserialize<string[]>(File.ReadAllText(p)) ?? []
            : [];
    }

    private void AddRecent(string path)
    {
        var recent = LoadRecent().Where(r => !r.Equals(path, StringComparison.OrdinalIgnoreCase)).Take(9).Prepend(path).ToArray();
        File.WriteAllText(Path.Combine(_dataDir, "recent.json"), JsonSerializer.Serialize(recent));
    }
}

public class AppSettings
{
    public string? LastScenario { get; set; } = "architecture";
    public string? LastProfile { get; set; } = "focused";
    public string? LastFormat { get; set; } = "markdown";
    public int LastTokens { get; set; } = 8000;
    public string? LastAround { get; set; } = "";
    public bool IncludeProvenance { get; set; }
    public bool IncludeDiagnostics { get; set; }
    public bool NoRoslyn { get; set; }
    public string? Theme { get; set; } = "dark";
}
