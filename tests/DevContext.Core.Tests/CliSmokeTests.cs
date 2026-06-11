using System.Diagnostics;

namespace DevContext.Core.Tests;

public sealed class CliSmokeTests
{
    [Fact]
    public async Task DryRun_OnMinimalApiFixture_ShowsExpectedExtractors()
    {
        var fixturePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tests", "fixtures", "MinimalApiProject"));

        var (exitCode, output) = await RunCliAsync($"analyze --dry-run \"{fixturePath}\"");

        Assert.Equal(0, exitCode);
        Assert.Contains("DependencyExtractor", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("MediatR", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("FileTreeExtractor", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("EndpointExtractor", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ProgramCsFlowExtractor", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ScenariosCommand_ListsAllScenarios()
    {
        var (exitCode, output) = await RunCliAsync("scenarios");

        Assert.Equal(0, exitCode);
        Assert.Contains("overview", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("deep-dive", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("audit", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task VersionCommand_ReturnsVersion()
    {
        var (exitCode, output) = await RunCliAsync("version");

        Assert.Equal(0, exitCode);
        Assert.Contains("DevContext", output, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<(int ExitCode, string Output)> RunCliAsync(string arguments)
    {
        var projectDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "DevContext.Cli"));

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{projectDir}\" -- {arguments}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start process");
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return (process.ExitCode, output + error);
    }
}
