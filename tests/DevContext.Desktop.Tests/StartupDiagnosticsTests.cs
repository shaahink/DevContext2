using DevContext.Desktop.Services;
using DevContext.Desktop.ViewModels;

namespace DevContext.Desktop.Tests;

public class StartupDiagnosticsTests
{
    [Fact]
    public void MainViewModel_initializes_with_all_properties()
    {
        var vm = new MainViewModel(new MockAnalysisService());
        Assert.NotNull(vm);
        Assert.NotNull(vm.Scenarios);
        Assert.NotEmpty(vm.Scenarios);
        Assert.Equal("focused", vm.SelectedProfile);
        Assert.Equal("markdown", vm.SelectedFormat);
        Assert.False(vm.IsAnalyzing);
        Assert.False(vm.HasOutput);
    }

    [Fact]
    public void UiState_serializes_all_fields()
    {
        var vm = new MainViewModel(new MockAnalysisService());
        vm.ProjectPath = "C:\\test";
        vm.MaxTokens = 5000;

        var state = new UiState(vm);

        Assert.Equal("C:\\test", state.projectPath);
        Assert.Equal(5000, state.maxTokens);
        Assert.Equal(8000, state.budgetTokens);
        Assert.NotNull(state.scenarios);
        Assert.NotEmpty(state.scenarios);
        Assert.NotNull(state.profiles);
        Assert.Equal(3, state.profiles.Count);
    }

    [Fact]
    public void UiState_handles_null_sections()
    {
        var vm = new MainViewModel(new MockAnalysisService());
        var state = new UiState(vm);

        Assert.NotNull(state.sectionGroups);
        Assert.Empty(state.sectionGroups);
        Assert.NotNull(state.recentPaths);
    }

    private sealed class MockAnalysisService : IAnalysisService
    {
        public Task<AnalysisResult> AnalyzeAsync(AnalysisOptions opts, IProgress<AnalysisProgress>? progress = null, CancellationToken ct = default)
            => Task.FromResult(new AnalysisResult { Success = true });
        public AppSettings LoadSettings() => new();
        public void SaveSettings(AppSettings s) { }
        public string[] LoadRecent() => [];
        public void AddRecent(string path) { }
    }
}
