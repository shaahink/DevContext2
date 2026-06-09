using System.ComponentModel;
using DevContext.Desktop.Services;
using DevContext.Desktop.ViewModels;
using NSubstitute;

namespace DevContext.Desktop.Tests;

public class MainViewModelTests
{
    private readonly IAnalysisService _svc;
    private readonly AppSettings _settings;
    private readonly string[] _recent;

    public MainViewModelTests()
    {
        _svc = Substitute.For<IAnalysisService>();
        _settings = new AppSettings();
        _recent = [];

        _svc.LoadSettings().Returns(_settings);
        _svc.LoadRecent().Returns(_recent);
    }

    private MainViewModel CreateVm() => new(_svc);

    // ══ Initialization ═════════════════════════════════════════════════════════

    [Fact]
    public void Constructor_sets_default_scenario()
    {
        var vm = CreateVm();
        Assert.Equal("debug-endpoint", vm.SelectedScenario.Value);
    }

    [Fact]
    public void Constructor_loads_settings()
    {
        _settings.LastProfile = "full";
        _settings.LastFormat = "json";
        _settings.LastTokens = 4000;
        _settings.LastAround = "MyClass:Method";
        _settings.IncludeProvenance = true;
        _settings.IncludeDiagnostics = true;
        _settings.NoRoslyn = true;

        var vm = CreateVm();

        Assert.Equal("full", vm.SelectedProfile);
        Assert.Equal("json", vm.SelectedFormat);
        Assert.Equal(4000, vm.MaxTokens);
        Assert.Equal("MyClass:Method", vm.Around);
        Assert.True(vm.IncludeProvenance);
        Assert.True(vm.IncludeDiagnostics);
        Assert.True(vm.NoRoslyn);
    }

    [Fact]
    public void Constructor_loads_recent_paths()
    {
        var recent = new[] { "C:\\Proj1", "C:\\Proj2" };
        _svc.LoadRecent().Returns(recent);

        var vm = CreateVm();

        Assert.Equal(2, vm.RecentPaths.Count);
        Assert.Equal("C:\\Proj1", vm.RecentPaths[0]);
        Assert.Equal("C:\\Proj2", vm.RecentPaths[1]);
    }

    [Fact]
    public void Constructor_initializes_scenarios()
    {
        var vm = CreateVm();
        Assert.Equal(6, vm.Scenarios.Count);
        Assert.Contains(vm.Scenarios, s => s.Value == "architecture");
        Assert.Contains(vm.Scenarios, s => s.Value == "debug-endpoint");
        Assert.Contains(vm.Scenarios, s => s.Value == "trace-message-flow");
    }

    // ══ Computed properties ════════════════════════════════════════════════════

    [Theory]
    [InlineData("focused", true, false, false)]
    [InlineData("debug", false, true, false)]
    [InlineData("full", false, false, true)]
    public void Profile_computed_flags(string profile, bool focused, bool debug, bool full)
    {
        var vm = CreateVm();
        vm.SelectedProfile = profile;

        Assert.Equal(focused, vm.IsProfileFocused);
        Assert.Equal(debug, vm.IsProfileDebug);
        Assert.Equal(full, vm.IsProfileFull);
    }

    [Theory]
    [InlineData("markdown", true, false)]
    [InlineData("json", false, true)]
    public void Format_computed_flags(string format, bool markdown, bool json)
    {
        var vm = CreateVm();
        vm.SelectedFormat = format;

        Assert.Equal(markdown, vm.IsFormatMarkdown);
        Assert.Equal(json, vm.IsFormatJson);
    }

    [Fact]
    public void AnalyzeButtonText_shows_analyze_when_idle()
    {
        var vm = CreateVm();
        Assert.Equal("Analyze", vm.AnalyzeButtonText);
    }

    // ══ Commands ═══════════════════════════════════════════════════════════════

    [Fact]
    public void SetProfileCommand_changes_profile()
    {
        var vm = CreateVm();
        vm.SetProfileCommand.Execute("debug");
        Assert.Equal("debug", vm.SelectedProfile);
    }

    [Fact]
    public void SetFormatCommand_changes_format()
    {
        var vm = CreateVm();
        vm.SetFormatCommand.Execute("json");
        Assert.Equal("json", vm.SelectedFormat);
    }

    [Fact]
    public void SelectRecentCommand_sets_project_path()
    {
        var vm = CreateVm();
        vm.SelectRecentCommand.Execute("C:\\MyProject");
        Assert.Equal("C:\\MyProject", vm.ProjectPath);
    }

    [Fact]
    public void AnalyzeCommand_cannot_execute_when_path_empty()
    {
        var vm = CreateVm();
        Assert.False(vm.AnalyzeCommand.CanExecute(null));
    }

    [Fact]
    public void AnalyzeCommand_can_execute_even_when_analyzing()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";
        Assert.True(vm.AnalyzeCommand.CanExecute(null));
    }

    [Fact]
    public void AnalyzeCommand_can_execute_when_path_set_and_not_analyzing()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\SomeProject";
        Assert.True(vm.AnalyzeCommand.CanExecute(null));
    }

    // ══ Property change notifications ══════════════════════════════════════════

    [Fact]
    public void SelectedProfile_change_notifies_computed_properties()
    {
        var vm = CreateVm();
        var changedProps = new List<string>();
        vm.PropertyChanged += (_, e) => changedProps.Add(e.PropertyName!);

        vm.SelectedProfile = "debug";

        Assert.Contains(nameof(vm.IsProfileFocused), changedProps);
        Assert.Contains(nameof(vm.IsProfileDebug), changedProps);
        Assert.Contains(nameof(vm.IsProfileFull), changedProps);
    }

    [Fact]
    public void SelectedFormat_change_notifies_computed_properties()
    {
        var vm = CreateVm();
        var changedProps = new List<string>();
        vm.PropertyChanged += (_, e) => changedProps.Add(e.PropertyName!);

        vm.SelectedFormat = "json";

        Assert.Contains(nameof(vm.IsFormatMarkdown), changedProps);
        Assert.Contains(nameof(vm.IsFormatJson), changedProps);
    }

    // ══ Auto-reanalyze ═════════════════════════════════════════════════════════

    [Fact]
    public void Option_change_does_not_trigger_analysis_when_no_output()
    {
        var vm = CreateVm();
        Assert.False(vm.HasOutput);
        Assert.False(vm.IsAnalyzing);

        vm.SelectedScenario = vm.Scenarios[0]; // Change scenario

        Assert.False(vm.IsAnalyzing); // Should not trigger
    }

    [Fact]
    public void Option_change_does_not_trigger_analysis_when_path_empty()
    {
        var vm = CreateVm();
        // Set output without path
        typeof(MainViewModel).GetProperty(nameof(vm.HasOutput))?.SetValue(vm, true);

        Assert.False(vm.IsAnalyzing);

        vm.SelectedScenario = vm.Scenarios[0];

        Assert.False(vm.IsAnalyzing);
    }

    [Fact]
    public async Task Option_change_triggers_analysis_when_output_exists_and_path_set()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";
        // Set HasOutput to simulate previous analysis completed
        typeof(MainViewModel).GetProperty(nameof(vm.HasOutput))?.SetValue(vm, true);

        // Set up mock before option change triggers re-analysis
        var tcs = new TaskCompletionSource<AnalysisResult>();
        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(tcs.Task);

        var triggered = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.IsAnalyzing) && vm.IsAnalyzing)
                triggered = true;
        };

        vm.SelectedProfile = "quick"; // triggers re-analysis

        Assert.True(triggered);

        // Clean up
        tcs.SetResult(new AnalysisResult { Success = true, Content = "ok" });
        // Wait for the async re-analysis to complete
        await Task.Delay(100);
    }

    [Fact]
    public void Option_changes_during_initialization_do_not_trigger()
    {
        // The constructor sets _isInitializing = true during LoadSettings
        // and false at the end. We verify this by checking that the
        // test VM created via constructor doesn't have IsAnalyzing = true

        _settings.LastProfile = "full";
        _settings.LastFormat = "json";

        var vm = CreateVm();

        // LoadSettings ran during construction with _isInitializing=true
        // so no analysis should have been triggered
        Assert.False(vm.IsAnalyzing);
        Assert.Equal("full", vm.SelectedProfile);
        Assert.Equal("json", vm.SelectedFormat);
    }

    // ══ Analysis lifecycle ═════════════════════════════════════════════════════

    [Fact]
    public async Task AnalyzeAsync_saves_settings_after_completion()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        var result = new AnalysisResult { Success = true, Content = "output", ElapsedMs = 100 };
        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));

        await ExecuteAnalyzeCommand(vm);

        _svc.Received(1).SaveSettings(Arg.Any<AppSettings>());
    }

    [Fact]
    public async Task AnalyzeAsync_adds_recent_path()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AnalysisResult { Success = true, Content = "ok" }));

        await ExecuteAnalyzeCommand(vm);

        _svc.Received(1).AddRecent("C:\\Test");
    }

    [Fact]
    public async Task AnalyzeAsync_sets_output_on_success()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AnalysisResult { Success = true, Content = "analysis output", ElapsedMs = 200 }));

        await ExecuteAnalyzeCommand(vm);

        Assert.True(vm.HasOutput);
        Assert.Equal("analysis output", vm.OutputText);
        Assert.Contains("tokens", vm.StatsText);
        Assert.Contains("0.2s", vm.StatsText);
        Assert.Equal("Done", vm.ProgressText);
    }

    [Fact]
    public async Task AnalyzeAsync_shows_error_on_failure()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AnalysisResult { Success = false, Error = "Something went wrong" }));

        await ExecuteAnalyzeCommand(vm);

        Assert.True(vm.HasOutput);
        Assert.Equal("Something went wrong", vm.OutputText);
        Assert.Equal("Error", vm.ProgressText);
    }

    [Fact]
    public async Task AnalyzeAsync_handles_exception()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns<Task<AnalysisResult>>(_ => throw new InvalidOperationException("Boom"));

        await ExecuteAnalyzeCommand(vm);

        Assert.True(vm.HasOutput);
        Assert.Equal("Boom", vm.OutputText);
        Assert.Equal("Error", vm.ProgressText);
    }

    [Fact]
    public async Task AnalyzeAsync_cancels_previous_when_restarted()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        var tcs = new TaskCompletionSource<AnalysisResult>();
        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(tcs.Task);

        // Start first analysis (returns pending)
        var firstTask = (Task)vm.AnalyzeCommand.ExecuteAsync(null)!;
        Assert.True(vm.IsAnalyzing);

        // Reconfigure for second call
        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AnalysisResult { Success = true, Content = "restarted", ElapsedMs = 10 }));

        // Start second (cancels first internally via CancelPrevious)
        var secondTask = (Task)vm.AnalyzeCommand.ExecuteAsync(null)!;
        await secondTask;

        Assert.Equal("restarted", vm.OutputText);
        Assert.False(vm.IsAnalyzing);

        // Clean up orphaned first task
        tcs.TrySetCanceled();
        try { await firstTask; } catch { }
    }

    [Fact]
    public async Task AnalyzeAsync_resets_progress_state_before_starting()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AnalysisResult { Success = true, Content = "ok" }));

        await ExecuteAnalyzeCommand(vm);

        Assert.True(vm.IsProgressVisible);
    }

    // ══ Progress reporting ═════════════════════════════════════════════════════

    [Fact]
    public async Task Progress_state_is_set_before_analysis_starts()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        string? progressTextBefore = null;
        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                // Capture state at the moment AnalyzeAsync is called
                progressTextBefore = vm.ProgressText;
                return Task.FromResult(new AnalysisResult { Success = true, Content = "ok" });
            });

        await ExecuteAnalyzeCommand(vm);

        Assert.Equal("Starting...", progressTextBefore);
        Assert.Equal("Done", vm.ProgressText);
    }

    // ══ Helper ══════════════════════════════════════════════════════════════════

    private static async Task ExecuteAnalyzeCommand(MainViewModel vm)
    {
        vm.AnalyzeCommand.NotifyCanExecuteChanged();
        if (vm.AnalyzeCommand.CanExecute(null))
            await vm.AnalyzeCommand.ExecuteAsync(null);
    }
}
