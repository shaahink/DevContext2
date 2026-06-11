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
        Assert.Equal("overview", vm.SelectedScenario.Value);
    }

    [Fact]
    public void Constructor_loads_settings()
    {
        _settings.LastFormat = "json";
        _settings.LastTokens = 4000;
        _settings.LastAround = "MyClass:Method";
        _settings.IncludeProvenance = true;
        _settings.IncludeDiagnostics = true;
        _settings.NoRoslyn = true;

        var vm = CreateVm();

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
        Assert.Equal(2, vm.Scenarios.Count);
        Assert.Contains(vm.Scenarios, s => s.Value == "overview");
        Assert.Contains(vm.Scenarios, s => s.Value == "deep-dive");
    }

    // ══ Sections ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Constructor_initializes_sections()
    {
        var vm = CreateVm();
        Assert.Equal(12, vm.Sections.Count);
        Assert.Contains(vm.Sections, s => s.Key == DevContext.Core.Constants.SectionNames.ArchitectureOverview);
        Assert.Contains(vm.Sections, s => s.Key == DevContext.Core.Constants.SectionNames.Endpoints);
        Assert.Contains(vm.Sections, s => s.Key == DevContext.Core.Constants.SectionNames.CallGraph);
        Assert.Contains(vm.Sections, s => s.Key == "__source__");
    }

    [Fact]
    public void Sections_default_to_overview_preset()
    {
        var vm = CreateVm();
        // Overview: architecture true, call graph false, source false
        Assert.True(vm.Sections.First(s => s.Key == DevContext.Core.Constants.SectionNames.ArchitectureOverview).IsEnabled);
        Assert.False(vm.Sections.First(s => s.Key == DevContext.Core.Constants.SectionNames.CallGraph).IsEnabled);
        Assert.False(vm.Sections.First(s => s.Key == "__source__").IsEnabled);
        Assert.True(vm.Sections.First(s => s.Key == DevContext.Core.Constants.SectionNames.DiRegistrations).IsEnabled);
    }

    [Fact]
    public void Sections_default_to_trace_preset_when_scenario_is_deep_dive()
    {
        var vm = CreateVm();
        vm.SelectedScenario = vm.Scenarios.First(s => s.Value == "deep-dive");

        Assert.True(vm.Sections.First(s => s.Key == DevContext.Core.Constants.SectionNames.CallGraph).IsEnabled);
        Assert.False(vm.Sections.First(s => s.Key == DevContext.Core.Constants.SectionNames.ArchitectureOverview).IsEnabled);
        Assert.True(vm.Sections.First(s => s.Key == DevContext.Core.Constants.SectionNames.Endpoints).IsEnabled);
    }

    [Fact]
    public void IsTraceMode_true_when_deep_dive_selected()
    {
        var vm = CreateVm();
        Assert.False(vm.IsTraceMode);

        vm.SelectedScenario = vm.Scenarios.First(s => s.Value == "deep-dive");
        Assert.True(vm.IsTraceMode);
    }

    [Theory]
    [InlineData("__source__", true, "full")]
    [InlineData(DevContext.Core.Constants.SectionNames.CallGraph, true, "debug")]
    [InlineData("__source__", false, "focused")]
    public void DerivedProfile_reflects_section_state(string sectionKey, bool enable, string expectedProfile)
    {
        var vm = CreateVm();
        // Reset all to false first
        foreach (var s in vm.Sections)
            s.IsEnabled = false;
        // Enable only the section under test
        vm.SetSectionEnabled(sectionKey, enable);

        Assert.Equal(expectedProfile, vm.DerivedProfile);
    }

    [Fact]
    public void DerivedProfile_full_when_source_and_call_graph_both_on()
    {
        var vm = CreateVm();
        foreach (var s in vm.Sections)
            s.IsEnabled = false;
        vm.SetSectionEnabled("__source__", true);
        vm.SetSectionEnabled(DevContext.Core.Constants.SectionNames.CallGraph, true);

        Assert.Equal("full", vm.DerivedProfile);
    }

    [Fact]
    public void SetSectionEnabled_updates_section_and_triggers_change()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";
        // Simulate output so reanalysis triggers
        typeof(MainViewModel).GetProperty(nameof(MainViewModel.HasOutput))?.SetValue(vm, true);

        var tcs = new TaskCompletionSource<AnalysisResult>();
        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(tcs.Task);

        vm.SetSectionEnabled(DevContext.Core.Constants.SectionNames.CallGraph, true);

        Assert.True(vm.Sections.First(s => s.Key == DevContext.Core.Constants.SectionNames.CallGraph).IsEnabled);

        tcs.SetResult(new AnalysisResult { Success = true, Content = "ok" });
    }

    [Fact]
    public void LoadSettings_restores_active_sections()
    {
        _settings.LastActiveSections = new List<string> { DevContext.Core.Constants.SectionNames.CallGraph, "__source__" };

        var vm = CreateVm();

        Assert.True(vm.Sections.First(s => s.Key == DevContext.Core.Constants.SectionNames.CallGraph).IsEnabled);
        Assert.True(vm.Sections.First(s => s.Key == "__source__").IsEnabled);
        // Sections not in the list should default to false
        Assert.False(vm.Sections.First(s => s.Key == DevContext.Core.Constants.SectionNames.ArchitectureOverview).IsEnabled);
    }

    // ══ Task / intent ══════════════════════════════════════════════════════════

    [Fact]
    public void Constructor_loads_last_task()
    {
        _settings.LastTask = "trace the order handler";
        var vm = CreateVm();
        Assert.Equal("trace the order handler", vm.Task);
    }

    [Fact]
    public void Task_field_initially_empty()
    {
        var vm = CreateVm();
        Assert.Equal("", vm.Task);
    }

    // ══ Computed properties ════════════════════════════════════════════════════

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
        typeof(MainViewModel).GetProperty(nameof(MainViewModel.HasOutput))?.SetValue(vm, true);

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
        typeof(MainViewModel).GetProperty(nameof(MainViewModel.HasOutput))?.SetValue(vm, true);

        // Set up mock before option change triggers re-analysis
        var tcs = new TaskCompletionSource<AnalysisResult>();
        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(tcs.Task);

        var triggered = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsAnalyzing) && vm.IsAnalyzing)
                triggered = true;
        };

        vm.SelectedFormat = "json"; // triggers re-analysis

        Assert.True(triggered);

        // Clean up
        tcs.SetResult(new AnalysisResult { Success = true, Content = "ok" });
        // Wait for the async re-analysis to complete
        await Task.Delay(100);
    }

    [Fact]
    public void Option_changes_during_initialization_do_not_trigger()
    {
        _settings.LastFormat = "json";

        var vm = CreateVm();

        // LoadSettings ran during construction with _isInitializing=true
        // so no analysis should have been triggered
        Assert.False(vm.IsAnalyzing);
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
    public async Task AnalyzeAsync_hides_progress_after_completion()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AnalysisResult { Success = true, Content = "ok" }));

        await ExecuteAnalyzeCommand(vm);

        Assert.False(vm.IsProgressVisible);
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

    [Fact]
    public async Task AnalyzeAsync_populates_DisplayText_after_success()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AnalysisResult { Success = true, Content = "## Header\ncontent", ElapsedMs = 100 }));

        await ExecuteAnalyzeCommand(vm);

        Assert.NotEmpty(vm.DisplayText);
        Assert.Contains("Header", vm.DisplayText);
    }

    [Fact]
    public async Task Toggling_section_updates_DisplayText_in_Llm_view()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AnalysisResult
            {
                Success = true,
                Content = "## Keep\nkeep content\n## Drop\ndrop content",
                ElapsedMs = 100
            }));

        await ExecuteAnalyzeCommand(vm);

        // Switch to LLM view so DisplayText reads from LlmViewText
        vm.IsHumanView = false;
        var beforeToggle = vm.DisplayText;

        // Find the "Drop" section and uncheck it
        var dropSection = vm.SectionGroups.SelectMany(g => g.Children)
            .FirstOrDefault(s => s.Name.Contains("Drop"));
        Assert.NotNull(dropSection);
        dropSection.IsIncluded = false;

        var afterToggle = vm.DisplayText;
        Assert.NotEqual(beforeToggle, afterToggle);
        Assert.DoesNotContain("drop content", afterToggle);
    }

    [Fact]
    public async Task Toggling_section_updates_token_total()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AnalysisResult
            {
                Success = true,
                Content = "## Section A\nsome long text here for tokens\n\n## Section B\nmore text here too",
                ElapsedMs = 100
            }));

        await ExecuteAnalyzeCommand(vm);

        var beforeTotal = vm.SelectedTokenTotal;
        Assert.True(beforeTotal > 0);

        // Uncheck all sections
        foreach (var group in vm.SectionGroups)
            foreach (var section in group.Children)
                section.IsIncluded = false;

        Assert.Equal(0, vm.SelectedTokenTotal);
    }

    [Fact]
    public async Task Changing_scenario_triggers_reanalysis_and_resets_defaults()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        var callCount = 0;
        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                return Task.FromResult(new AnalysisResult { Success = true, Content = "## Keep\nkeep\n## Drop\ndrop", ElapsedMs = 10 });
            });

        await ExecuteAnalyzeCommand(vm);
        Assert.Equal(1, callCount);

        vm.SelectedScenario = vm.Scenarios[1]; // Switch to Trace
        await Task.Delay(200);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task Token_budget_is_captured_at_analysis_start()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";
        vm.MaxTokens = 5000;

        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AnalysisResult { Success = true, Content = "data", ElapsedMs = 10 }));

        await ExecuteAnalyzeCommand(vm);
        Assert.Equal(5000, vm.BudgetTokens);
    }

    [Fact]
    public async Task Debounce_prevents_rapid_reanalysis_on_slider()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        var callCount = 0;
        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                return Task.FromResult(new AnalysisResult { Success = true, Content = "ok", ElapsedMs = 10 });
            });

        await ExecuteAnalyzeCommand(vm);
        Assert.Equal(1, callCount); // initial

        // Rapidly change tokens — debounce should collapse to 1 re-analysis
        vm.MaxTokens = 4000;
        vm.MaxTokens = 3000;
        vm.MaxTokens = 2000;
        vm.MaxTokens = 1000;

        await Task.Delay(1000); // wait for debounce + re-analysis
        Assert.Equal(2, callCount); // only 1 additional call (debounced)
    }

    // ══ Section data flows into AnalysisOptions ════════════════════════════════

    [Fact]
    public async Task AnalyzeAsync_passes_active_sections_to_AnalysisOptions()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        AnalysisOptions? capturedOpts = null;
        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedOpts = callInfo.Arg<AnalysisOptions>();
                return Task.FromResult(new AnalysisResult { Success = true, Content = "ok", ElapsedMs = 10 });
            });

        await ExecuteAnalyzeCommand(vm);

        Assert.NotNull(capturedOpts);
        Assert.NotEmpty(capturedOpts.ActiveSections);
        Assert.Contains(DevContext.Core.Constants.SectionNames.ArchitectureOverview, capturedOpts.ActiveSections);
        Assert.DoesNotContain(DevContext.Core.Constants.SectionNames.CallGraph, capturedOpts.ActiveSections);
    }

    [Fact]
    public async Task AnalyzeAsync_passes_derived_profile_to_AnalysisOptions()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";

        AnalysisOptions? capturedOpts = null;
        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedOpts = callInfo.Arg<AnalysisOptions>();
                return Task.FromResult(new AnalysisResult { Success = true, Content = "ok", ElapsedMs = 10 });
            });

        await ExecuteAnalyzeCommand(vm);

        Assert.NotNull(capturedOpts);
        Assert.Equal("focused", capturedOpts.Profile); // default: no call graph, no source
    }

    [Fact]
    public async Task AnalyzeAsync_passes_task_to_AnalysisOptions()
    {
        var vm = CreateVm();
        vm.ProjectPath = "C:\\Test";
        vm.Task = "debug the failing endpoint";

        AnalysisOptions? capturedOpts = null;
        _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedOpts = callInfo.Arg<AnalysisOptions>();
                return Task.FromResult(new AnalysisResult { Success = true, Content = "ok", ElapsedMs = 10 });
            });

        await ExecuteAnalyzeCommand(vm);

        Assert.NotNull(capturedOpts);
        Assert.Equal("debug the failing endpoint", capturedOpts.Task);
    }

    // ══ Section data parsing ══════════════════════════════════════════════════

[Fact]
public async Task PopulateSections_parses_markdown_into_groups()
{
    var vm = CreateVm();
    vm.ProjectPath = "C:\\Test";

    var content = "## Architecture overview\narch content\n\n"
                + "## Endpoints\nep content\n\n"
                + "## Data model (EF Core)\ndata content\n\n"
                + "## Call graph\ncall content\n\n"
                + "## Non-obvious wiring\nwire content\n\n"
                + "## Anti-patterns\nanti content";

    _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(new AnalysisResult { Success = true, Content = content, ElapsedMs = 10 }));

    await ExecuteAnalyzeCommand(vm);

    Assert.NotEmpty(vm.SectionGroups);
    Assert.True(vm.TotalTokens > 0);
    Assert.True(vm.SelectedTokenTotal > 0);
    Assert.NotEmpty(vm.LlmViewText);
    Assert.NotEqual("", vm.LlmViewText);
}

[Fact]
public async Task PopulateSections_categorizes_correctly()
{
    var vm = CreateVm();
    vm.ProjectPath = "C:\\Test";

    var content = "## Endpoints\nep content\n\n"
                + "## Architecture overview\narch content";

    _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(new AnalysisResult { Success = true, Content = content, ElapsedMs = 10 }));

    await ExecuteAnalyzeCommand(vm);

    var apiGroup = vm.SectionGroups.FirstOrDefault(g => g.Name == "API");
    var archGroup = vm.SectionGroups.FirstOrDefault(g => g.Name == "Architecture");
    Assert.NotNull(apiGroup);
    Assert.NotNull(archGroup);
    Assert.Single(apiGroup.Children);
    Assert.Single(archGroup.Children);
    Assert.Equal("Endpoints", apiGroup.Children[0].Name);
    Assert.Equal("Architecture overview", archGroup.Children[0].Name);
}

[Fact]
public async Task LlmViewText_excludes_disabled_sections()
{
    var vm = CreateVm();
    vm.ProjectPath = "C:\\Test";

    var content = "## Keep\nkeep content\n\n## Drop\ndrop content";

    _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(new AnalysisResult { Success = true, Content = content, ElapsedMs = 10 }));

    await ExecuteAnalyzeCommand(vm);

    var before = vm.LlmViewText;
    Assert.Contains("keep content", before);
    Assert.Contains("drop content", before);

    // Toggle off "Drop"
    var drop = vm.SectionGroups.SelectMany(g => g.Children).First(s => s.Name.Contains("Drop"));
    drop.IsIncluded = false;

    var after = vm.LlmViewText;
    Assert.Contains("keep content", after);
    Assert.DoesNotContain("drop content", after);
}

// ══ PropertyChanged batching ══════════════════════════════════════════════

[Fact]
public async Task AnalyzeAsync_fires_single_batched_PropertyChanged()
{
    var vm = CreateVm();
    vm.ProjectPath = "C:\\Test";

    _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(new AnalysisResult { Success = true, Content = "## Test\ncontent", ElapsedMs = 10 }));

    var events = new List<string?>();
    vm.PropertyChanged += (_, e) => events.Add(e.PropertyName);

    await ExecuteAnalyzeCommand(vm);

    // A single batched notification (empty/null PropertyName) must fire
    var batched = events.Where(e => string.IsNullOrEmpty(e)).ToList();
    Assert.Single(batched);

    // Fields set exclusively in the batch block must NOT appear as individual events
    var batchExclusive = new[] { "DisplayText", "StatsText", "HasOutput", "BudgetTokens", "OutputText", "SelectedTokenTotal" };
    foreach (var field in batchExclusive)
    {
        var perField = events.Where(e => e == field).ToList();
        Assert.Empty(perField);
    }
}

[Fact]
public async Task AnalyzeAsync_CollectionChanged_fires_on_UI_bound_groups()
{
    var vm = CreateVm();
    vm.ProjectPath = "C:\\Test";

    _svc.AnalyzeAsync(Arg.Any<AnalysisOptions>(), Arg.Any<IProgress<AnalysisProgress>>(), Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(new AnalysisResult
        {
            Success = true,
            Content = "## Section A\nA\n\n## Section B\nB\n\n## Section C\nC",
            ElapsedMs = 10
        }));

    var collectionEvents = new List<string>();
    vm.SectionGroups.CollectionChanged += (_, e) =>
        collectionEvents.Add(e.Action.ToString());

    await ExecuteAnalyzeCommand(vm);

    Assert.NotEmpty(collectionEvents);
    Assert.Equal("Reset", collectionEvents[0]);  // Clear first, then Adds
    var adds = collectionEvents.Skip(1).Where(a => a == "Add").Count();
    Assert.True(adds >= 1);
}
}
