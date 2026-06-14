using DevContext.Desktop.Services;
using DevContext.Desktop.ViewModels;

namespace DevContext.Desktop.Tests;

/// <summary>Integration tests using the real AnalysisService and MainViewModel (no NSubstitute mocks)
/// against the MinimalApiProject fixture on disk. Full VM→Service→Pipeline→Renderer chain.</summary>
public sealed class DesktopIntegrationTests
{
    private static string ResolveFixturePath()
    {
        var searchPaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tests", "fixtures", "MinimalApiProject"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "tests", "fixtures", "MinimalApiProject"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "tests", "fixtures", "MinimalApiProject"),
        };

        foreach (var p in searchPaths)
        {
            var full = Path.GetFullPath(p);
            if (Directory.Exists(full))
                return full;
        }

        throw new DirectoryNotFoundException("Cannot find MinimalApiProject fixture. Ensure tests/fixtures/MinimalApiProject exists.");
    }

    // DI-1
    [Fact]
    public async Task Full_VM_analyze_cycle_produces_output()
    {
        var vm = new MainViewModel();
        vm.ProjectPath = ResolveFixturePath();

        await ExecuteAnalyzeCommand(vm);

        Assert.True(vm.HasOutput);
        Assert.NotEmpty(vm.RawContent);
        Assert.True(vm.RawContent.Length > 200);
        Assert.Contains("tokens", vm.StatsText);
        Assert.False(vm.IsAnalyzing);
        Assert.False(vm.IsProgressVisible);
    }

    // DI-2
    [Fact]
    public async Task Focus_change_sets_stale_then_re_analyze_updates_output()
    {
        var vm = new MainViewModel();
        vm.ProjectPath = ResolveFixturePath();

        // Ensure a known clean state — reset focus to something distinct
        vm.Around = "InitialFocus_Test_" + Guid.NewGuid();

        await ExecuteAnalyzeCommand(vm);
        var initialContent = vm.RawContent;
        Assert.False(vm.IsStale);

        vm.Around = "CreateOrderHandler";
        Assert.True(vm.IsStale);

        await ExecuteAnalyzeCommand(vm);
        Assert.False(vm.IsStale);
        Assert.NotEmpty(vm.RawContent);
        Assert.NotEqual(initialContent, vm.RawContent);
    }

    // DI-3
    [Fact]
    public async Task Section_toggle_filters_rendered_output()
    {
        var vm = new MainViewModel();
        vm.ProjectPath = ResolveFixturePath();

        await ExecuteAnalyzeCommand(vm);

        // Check Endpoints section toggle exists and is enabled
        var epToggle = vm.Sections.FirstOrDefault(s => s.Key == DevContext.Core.Constants.SectionNames.Endpoints);
        Assert.NotNull(epToggle);
        Assert.True(epToggle.IsEnabled);

        // Verify Endpoints appear in output
        Assert.Contains("## Endpoints", vm.RawContent);

        // Toggle Endpoints off
        vm.SetSectionEnabled(DevContext.Core.Constants.SectionNames.Endpoints, false);
        Assert.False(vm.Sections.First(s => s.Key == DevContext.Core.Constants.SectionNames.Endpoints).IsEnabled);

        // Wait for re-render (fire-and-forget via SetSectionEnabled → OnRenderInputChanged → RerenderAsync)
        var maxWait = 20; // 2 seconds
        var waited = 0;
        while (vm.RawContent.Contains("## Endpoints") && waited < maxWait)
        {
            await Task.Delay(100);
            waited++;
        }

        Assert.DoesNotContain("## Endpoints", vm.RawContent);
    }

    private static async Task ExecuteAnalyzeCommand(MainViewModel vm)
    {
        vm.AnalyzeCommand.NotifyCanExecuteChanged();
        if (vm.AnalyzeCommand.CanExecute(null))
            await vm.AnalyzeCommand.ExecuteAsync(null);
    }
}
