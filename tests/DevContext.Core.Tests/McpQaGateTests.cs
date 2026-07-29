using System.Diagnostics;
using Xunit;

namespace DevContext.Core.Tests;

/// <summary>M5.3 — CI gate that invokes the MCP QA harness against the dogfood repo.
/// Marked with <c>McpQa</c> category so CI can select/exclude it independently.</summary>
[Trait("Category", "McpQa")]
public sealed class McpQaGateTests
{
    private const int TimeoutMs = 300_000; // 5 minutes

    [Fact]
    public async Task McpQaHarness_Passes_Against_Dogfood()
    {
        var repoRoot = FindRepoRoot();
        var runScript = Path.Combine(repoRoot, "eval", "mcp-qa", "run.js");

        var startInfo = new ProcessStartInfo
        {
            FileName = "node",
            Arguments = $"\"{runScript}\" --quiet",
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var proc = Process.Start(startInfo)!;
        using var cts = new CancellationTokenSource(TimeoutMs);

        var outputTask = proc.StandardOutput.ReadToEndAsync(cts.Token);
        var errorTask = proc.StandardError.ReadToEndAsync(cts.Token);

        try
        {
            await proc.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            try { proc.Kill(entireProcessTree: true); } catch { }
            Assert.Fail($"MCP QA harness timed out after {TimeoutMs / 1000}s");
            return;
        }

        var output = await outputTask;
        var errors = await errorTask;

        // G1.3 — the assertions carry the harness's own diagnosis. They used to be bare
        // Assert.Contains, so a red gate read "Sub-string not found: QA Score:" and nothing else,
        // while the harness had printed exactly why it stopped ("FATAL: Timeout: <method>") on
        // STDERR — which the old code only revealed after these asserts, i.e. never. Three
        // sessions have now investigated this gate; none of them could see what it saw.
        var diagnosis = $"exit {proc.ExitCode}\n--- stderr ---\n{errors}\n--- stdout ---\n{output}";

        Assert.True(output.Contains("QA Score:", StringComparison.Ordinal),
            $"The harness never reached scoring.\n{diagnosis}");
        Assert.True(output.Contains("Gate (checkout <=3c/2ktok)", StringComparison.Ordinal),
            $"The harness scored but did not report the checkout gate.\n{diagnosis}");
        Assert.True(output.Contains("PASS", StringComparison.Ordinal),
            $"The harness reported no PASS.\n{diagnosis}");

        if (proc.ExitCode != 0)
        {
            Assert.Fail($"MCP QA harness failed ({diagnosis})");
        }
    }

    [Fact]
    public void McpQa_Bench_Smoke_Script_Exists()
    {
        var repoRoot = FindRepoRoot();
        var benchScript = Path.Combine(repoRoot, "scripts", "bench.ps1");
        Assert.True(File.Exists(benchScript), $"Bench script not found: {benchScript}");

        var benchJson = Path.Combine(repoRoot, "eval-repos.json");
        Assert.True(File.Exists(benchJson), $"Bench config not found: {benchJson}");
    }

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "DevContext.slnx")))
                return dir;
            dir = Path.GetDirectoryName(dir);
        }
        throw new InvalidOperationException("Could not find repo root");
    }
}
