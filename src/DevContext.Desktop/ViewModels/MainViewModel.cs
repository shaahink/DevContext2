using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevContext.Core.Contracts;
using DevContext.Core.Graph;
using DevContext.Core.Models;
using DevContext.Core.Pipeline;
using DevContext.Core.Rendering;
using DevContext.Core.Services;
using DevContext.Desktop.Helpers;
using DevContext.Desktop.Services;
using Serilog;

namespace DevContext.Desktop.ViewModels;

public record ScenarioItem(string Value, string Label)
{
    public override string ToString() => Label;
}

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IAnalysisService _svc;
    private readonly GitCloneService _git = new();
    private readonly SectionSelectionModel _sections = new();
    private readonly OutputViewModel _output = new();
    private readonly CancellableOperation _analyzeOp = new();
    private readonly CancellableOperation _renderOp = new();
    private readonly CancellableOperation _validateOp = new();
    private readonly Debouncer _tokenDebouncer = new(500);
    private AnalysisSnapshot? _snapshot;
    private bool _isInitializing = true;

    // ── Output (forwarded from OutputViewModel) ───────────────────────────────
    public OutputViewModel Output => _output;
    public bool HasOutput => _output.HasOutput;
    public bool IsAnalyzing => _output.IsAnalyzing;
    public bool IsProgressVisible => _output.IsProgressVisible;
    public string ProgressText => _output.ProgressText;
    public string StatsText => _output.StatsText;
    public string StatsHtml => _output.StatsHtml;
    public string HumanViewText => _output.RawContent;
    public string LlmViewText => _output.LlmViewText;
    public string HumanViewHtml => _output.HumanViewHtml;
    public string RawContent => _output.RawContent;

    // ── Section selection (forwarded from SectionSelectionModel) ───────────────
    public SectionSelectionModel SectionSelection => _sections;
    public ImmutableArray<SectionGroupViewModel> SectionGroups => _sections.SectionGroups;
    public IList<SectionToggle> Sections => _sections.Sections;
    public int SelectedTokenTotal => _sections.SelectedTokenTotal;
    public int TotalTokens => _sections.TotalTokens;
    public int BudgetTokens => _sections.BudgetTokens;
    public float BudgetUtilisation => _sections.BudgetUtilisation;
    public bool IsTraceMode => _sections.IsTraceMode;
    public string DerivedProfile => _sections.DerivedProfile;

    private ImmutableArray<string> GetActiveSections() => _sections.GetActiveSections();

    // ── Form fields ────────────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AnalyzeCommand))]
    private string _projectPath = "";

    [ObservableProperty] private string _around = "";
    [ObservableProperty] private int _maxTokens = 8000;
    [ObservableProperty] private bool _includeProvenance;
    [ObservableProperty] private bool _includeDiagnostics;
    [ObservableProperty] private bool _noRoslyn;
    [ObservableProperty] private bool _dryRun;
    [ObservableProperty] private bool _includeAntiPatterns;

    [ObservableProperty] private int _depth = 6;
    [ObservableProperty] private string _detail = "salient";

    // ── Format selection ───────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormatMarkdown), nameof(IsFormatJson))]
    private string _selectedFormat = "markdown";

    [ObservableProperty] private ScenarioItem _selectedScenario = null!;

    // ── Graph-backed state (PLAN-11 Part A) ─────────────────────────────────────
    public bool HasGraph => _snapshot?.Graph is { NodeCount: > 0 };
    public IReadOnlyList<DevContext.Core.Graph.EntryPoint> Entries
    {
        get
        {
            var list = _snapshot?.Entries ?? [];
            return list;
        }
    }
    public IEnumerable<IGrouping<string, DevContext.Core.Graph.EntryPoint>> GroupedEntries
        => Entries.GroupBy(e => e.Kind switch
        {
            DevContext.Core.Graph.EntryPointKind.HttpEndpoint => "HTTP",
            DevContext.Core.Graph.EntryPointKind.MessageConsumer => "Bus Consumers",
            DevContext.Core.Graph.EntryPointKind.DomainEventHandler => "Domain Events",
            DevContext.Core.Graph.EntryPointKind.HostedService => "Hosted Services",
            DevContext.Core.Graph.EntryPointKind.PublicApi => "Public API",
            _ => "Other"
        }, StringComparer.Ordinal);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AnalyzeButtonText))]
    private string _selectedEntry = "";

    public bool IsFormatMarkdown => string.Equals(SelectedFormat, "markdown", StringComparison.Ordinal);
    public bool IsFormatJson => string.Equals(SelectedFormat, "json", StringComparison.Ordinal);

    // ── Analysis state ─────────────────────────────────────────────────────────
    // State lives on OutputViewModel; subscribe to bubble up AnalyzeButtonText notification.
    // Subscribe in constructor.

    private void RebuildLlmViewText()
    {
        // The LLM view is the raw content; no per-section filtering needed
        _output.LlmViewText = _output.RawContent;
    }

    public string AnalyzeButtonText
        => IsGitHubUrl
            ? (_output.IsAnalyzing ? "Cloning & Analyzing..." : "Clone & Analyze")
            : HasOutput
                ? (_output.IsAnalyzing ? "Analyzing..." : $"Analyze (~{SelectedTokenTotal:N0} tok)")
                : (_output.IsAnalyzing ? "Analyzing..." : "Analyze");

    // ── GitHub repo analysis ────────────────────────────────────────────────────
    public bool IsGitAvailable => _git.IsGitAvailable;

    private RepoUrl? _gitRepoUrl;
    private RepoStatus _gitRepoStatus = RepoStatus.None;

    public bool IsGitHubUrl => _gitRepoUrl is { IsValid: true };
    public string GitRepoDisplay => _gitRepoUrl?.ToDisplay() ?? "";
    public RepoStatus GitRepoStatus => _gitRepoStatus;

    [ObservableProperty] private string _cloneCleanup = "24h"; // default: cache 24h for GitHub repos

    partial void OnProjectPathChanged(string value)
    {
        if (_isInitializing) return;
        _ = ValidateGitHubUrlAsync(value);
    }

    private async Task ValidateGitHubUrlAsync(string path)
    {
        _validateOp.Cancel();
        var ct = _validateOp.Begin();

        var url = RepoUrl.Parse(path);
        _gitRepoUrl = url;
        _gitRepoStatus = url is null ? RepoStatus.None : RepoStatus.Checking;

        OnPropertyChanged(nameof(IsGitHubUrl));
        OnPropertyChanged(nameof(GitRepoDisplay));
        OnPropertyChanged(nameof(GitRepoStatus));
        OnPropertyChanged(nameof(AnalyzeButtonText));

        if (url is not { IsValid: true }) return;
        if (!_git.IsGitAvailable)
        {
            _gitRepoStatus = RepoStatus.NoGit;
            OnPropertyChanged(nameof(GitRepoStatus));
            return;
        }

        try
        {
            var status = await _git.ValidateAsync(url, ct).ConfigureAwait(true);
            ct.ThrowIfCancellationRequested();
            _gitRepoStatus = status;
        }
        catch (OperationCanceledException) { return; }
        catch (Exception)
        {
            _gitRepoStatus = RepoStatus.NetworkError;
        }
        finally
        {
            OnPropertyChanged(nameof(GitRepoStatus));
            OnPropertyChanged(nameof(GitRepoDisplay));
            OnPropertyChanged(nameof(AnalyzeButtonText));
        }
    }

    // ── Collections ────────────────────────────────────────────────────────────
    public ObservableCollection<string> RecentPaths { get; } = [];

    public void SetSectionEnabled(string key, bool enabled)
    {
        _sections.SetSectionEnabled(key, enabled);
        OnRenderInputChanged();
    }

    public IList<ScenarioItem> Scenarios { get; } =
    [
        new("overview",   "Overview"),
        new("deep-dive",  "Trace"),
    ];

    public MainViewModel() : this(new AnalysisService()) { }

    public MainViewModel(IAnalysisService svc)
    {
        _svc = svc;
        SelectedScenario = Scenarios[0];
        LoadSettings();
        RefreshRecent();
        _sections.CompleteInitialization();
        _sections.OnSectionChanged = () =>
        {
            RebuildLlmViewText();
        };
        _output.PropertyChanged += (_, e) =>
        {
            // Forward key property changes to VM so Razor bindings update
            if (e.PropertyName is null) return;

            if (e.PropertyName is nameof(OutputViewModel.IsAnalyzing) or nameof(OutputViewModel.HasOutput))
                OnPropertyChanged(nameof(AnalyzeButtonText));

            // Bubble through: any output property change should refresh the UI
            OnPropertyChanged(e.PropertyName);
        };
        _isInitializing = false;
    }

    // ── Auto-reanalyze on option change ────────────────────────────────────────
    partial void OnSelectedScenarioChanged(ScenarioItem value)
    {
        if (_isInitializing) return;
        _sections.SelectedScenarioValue = value.Value;
        OnPropertyChanged(nameof(IsTraceMode));
        ResetToScenarioDefaults();
        OnAnalysisInputChanged();
    }
    partial void OnSelectedFormatChanged(string value) => OnRenderInputChanged();

    partial void OnMaxTokensChanged(int value) => DebouncedReanalyze();
    partial void OnAroundChanged(string value) => OnAnalysisInputChanged();
    partial void OnIncludeProvenanceChanged(bool value) => OnRenderInputChanged();
    partial void OnIncludeDiagnosticsChanged(bool value) => OnRenderInputChanged();
    partial void OnNoRoslynChanged(bool value) => OnAnalysisInputChanged();
    partial void OnDryRunChanged(bool value)                    => OnAnalysisInputChanged();
    partial void OnIncludeAntiPatternsChanged(bool value)        => OnAnalysisInputChanged();

    partial void OnDepthChanged(int value)                      => DebouncedRender();
    partial void OnDetailChanged(string value)                   => OnRenderInputChanged();
    partial void OnSelectedEntryChanged(string value)
    {
        if (_isInitializing) return;
        OnRenderInputChanged();
    }

    private void OnAnalysisInputChanged()
    {
        if (_isInitializing || !HasOutput || string.IsNullOrWhiteSpace(ProjectPath))
            return;

        AnalyzeCommand.Execute(null);
    }

    private void OnRenderInputChanged()
    {
        if (_isInitializing || _snapshot is null || string.IsNullOrWhiteSpace(ProjectPath))
            return;

        _ = RerenderAsync();
    }

    private void DebouncedReanalyze()
    {
        if (_isInitializing || !HasOutput || string.IsNullOrWhiteSpace(ProjectPath))
            return;

        _tokenDebouncer.Invoke(() => OnRenderInputChanged());
    }

    private void DebouncedRender()
    {
        if (_isInitializing || _snapshot is null || string.IsNullOrWhiteSpace(ProjectPath))
            return;

        _tokenDebouncer.Invoke(() => OnRenderInputChanged());
    }

    // ── Commands ───────────────────────────────────────────────────────────────
    [RelayCommand]
    private void SetFormat(string format) => SelectedFormat = format;

    [RelayCommand]
    private void SelectRecent(string path) => ProjectPath = path;

    private bool CanAnalyze() => !string.IsNullOrWhiteSpace(ProjectPath);

    [RelayCommand(CanExecute = nameof(CanAnalyze), AllowConcurrentExecutions = true)]
    private async Task AnalyzeAsync()
    {
        _analyzeOp.Cancel();
        _renderOp.Cancel();
        var ct = _analyzeOp.Begin();
        var capturedBudget = MaxTokens;
        var capturedDepth = Depth;
        var capturedDetail = Detail;

        ResetOutputState();

        _svc.AddRecent(ProjectPath);
        RefreshRecent();

        var (clonePath, shouldContinue) = await TryHandleGitCloneAsync(ct).ConfigureAwait(true);
        if (!shouldContinue) return;
        var workingPath = clonePath ?? ProjectPath;

        var opts = BuildAnalysisOptions(workingPath, capturedBudget, capturedDepth, capturedDetail);

        var progress = new Progress<AnalysisProgress>(p =>
        {
            _output.ProgressText = p.Text;
        });

        try
        {
            var snapResult = await _svc.AnalyzeAsync(opts, progress, ct).ConfigureAwait(true);
            await ProcessSnapshotResultAsync(snapResult, capturedBudget, ct).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            if (ct.IsCancellationRequested) return;
            _output.ProgressText = "Canceled";
        }
        catch (Exception ex)
        {
            if (ct.IsCancellationRequested) return;
            Log.Error(ex, "Analysis failed");
            _output.ProgressText = "Error";
            _output.RawContent = ex.Message;
            _output.HasOutput = true;
        }
        finally
        {
            _output.IsAnalyzing = false;
            _output.IsProgressVisible = false;
            _ = System.Threading.Tasks.Task.Run(SaveSettings);
        }
    }

    private void ResetOutputState()
    {
        _output.IsAnalyzing = true;
        _output.IsProgressVisible = true;
        _output.ProgressText = "Starting...";
        _output.HasOutput = false;
        _output.StatsText = "";
        _output.RawContent = "";
        _output.HumanViewHtml = "";
        _snapshot = null;
        SelectedEntry = "";
    }

    private async Task<(string? Path, bool ShouldContinue)> TryHandleGitCloneAsync(CancellationToken ct)
    {
        if (_gitRepoUrl is not { IsValid: true } repo)
            return (null, true);

        var cloneResult = await _git.CloneAsync(repo, repo.ClonePath, repo.Ref,
            new Progress<CloneProgress>(p => _output.ProgressText = $"{p.Phase}: {p.PercentComplete}%"), ct).ConfigureAwait(true);

        if (cloneResult is null)
        {
            _output.ProgressText = "Error";
            _output.RawContent = $"Failed to clone {repo.ToDisplay()}...";
            _output.HasOutput = true;
            _output.IsAnalyzing = false;
            _output.IsProgressVisible = false;
            return (null, false);
        }

        return (cloneResult, true);
    }

    private AnalysisOptions BuildAnalysisOptions(string workingPath, int capturedBudget, int capturedDepth, string capturedDetail)
    {
        return new AnalysisOptions
        {
            ProjectPath = workingPath,
            // Let the shared resolver auto-detect deep-dive when a focus (--around) is provided,
            // but honour the explicit scenario pick when the user has switched to Trace.
            Scenario = string.Equals(SelectedScenario.Value, "deep-dive", StringComparison.Ordinal)
                ? "deep-dive"
                : (!string.IsNullOrWhiteSpace(Around) ? null : "overview"),
            Profile = DerivedProfile,
            Around = Around,
            MaxTokens = capturedBudget,
            Format = SelectedFormat,
            IncludeProvenance = IncludeProvenance,
            IncludeDiagnostics = IncludeDiagnostics,
            NoRoslyn = NoRoslyn,
            DryRun = DryRun,
            IncludeAntiPatterns = IncludeAntiPatterns,
            ActiveSections = GetActiveSections(),
            Depth = capturedDepth,
            Detail = capturedDetail,
        };
    }

    private async Task ProcessSnapshotResultAsync(SnapshotResult snapResult, int capturedBudget, CancellationToken ct)
    {
        if (snapResult.Success && snapResult.Snapshot is not null)
        {
            _snapshot = snapResult.Snapshot;
            var elapsedMs = snapResult.ElapsedMs;

            // Initial render from the snapshot
            await RerenderAsync(ct).ConfigureAwait(true);

            if (ct.IsCancellationRequested) return;

            _output.StatsHtml = _snapshot?.Report is { } r
                ? RunReportHtmlRenderer.Render(r, BuildDetectionStats(_snapshot)) : "";

            _output.StatsText = $"~{_sections.TotalTokens:N0} tokens · {elapsedMs / 1000.0:F1}s";
            _output.HasOutput = true;
            _output.HasOutput = true;
            _output.ProgressText = "Done";
            _sections.BudgetTokens = capturedBudget;

            OnPropertyChanged(string.Empty);

            // Clean up clone off UI thread
            if (_gitRepoUrl is { } gitRepo && string.Equals(CloneCleanup, "auto", StringComparison.Ordinal))
            {
                var clonePath = gitRepo.ClonePath;
                await System.Threading.Tasks.Task.Run(() =>
                    GitCloneService.Cleanup(clonePath), CancellationToken.None).ConfigureAwait(false);
            }
        }
        else
        {
            if (ct.IsCancellationRequested) return;
            _output.ProgressText = "Error";
            _output.RawContent = snapResult.Error ?? "Analysis failed.";
            _output.HasOutput = true;
        }
    }

    private async Task RerenderAsync(CancellationToken ct = default)
    {
        if (_snapshot is null) return;

        _renderOp.Cancel();
        var renderCt = _renderOp.Link(ct);

        try
        {
            var request = new RenderRequest
            {
                Format = SelectedFormat,
                MaxTokens = MaxTokens,
                Sections = GetActiveSections(),
                IncludeProvenance = IncludeProvenance,
                IncludeDiagnostics = IncludeDiagnostics,
                TokenView = false,
                Entry = SelectedEntry,
                Depth = Depth,
                Detail = Detail switch
                {
                    "signature" => TraceDetail.Signature,
                    "salient" => TraceDetail.Salient,
                    "full" => TraceDetail.Full,
                    _ => TraceDetail.Salient,
                },
            };

            var renderResult = await _svc.RenderAsync(_snapshot, request, renderCt).ConfigureAwait(true);

            if (renderCt.IsCancellationRequested) return;

            var rawContent = renderResult.Content ?? "";

            var (_, llmText, _, _) = _sections.BuildSectionDataFromStat(renderResult.Sections);

            if (renderCt.IsCancellationRequested) return;

            _output.RawContent = rawContent;
            _output.HumanViewHtml = renderResult.HtmlContent ?? "";
            _output.LlmViewText = rawContent;
            _sections.BudgetTokens = MaxTokens;

            if (renderCt.IsCancellationRequested) return;

            _output.StatsHtml = _snapshot?.Report is { } r
                ? RunReportHtmlRenderer.Render(r, BuildDetectionStats(_snapshot)) : "";
            _output.StatsText = _snapshot?.Report is { } report
                ? RunReportFormatter.Summary(report, renderResult.RenderFunnel)
                : "";

            OnPropertyChanged(string.Empty);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Log.Error(ex, "Re-render failed");
        }
    }

    // ── Settings helpers ───────────────────────────────────────────────────────
    private void LoadSettings()
    {
        var s = _svc.LoadSettings();
        SelectedScenario = Scenarios.FirstOrDefault(sc => string.Equals(sc.Value, s.LastScenario, StringComparison.Ordinal)) ?? Scenarios[0];
        SelectedFormat = s.LastFormat ?? "markdown";
        if (s.LastTokens > 0) MaxTokens = s.LastTokens;
        Around = s.LastAround ?? "";
        IncludeProvenance = s.IncludeProvenance;
        IncludeDiagnostics = s.IncludeDiagnostics;
        NoRoslyn = s.NoRoslyn;
        Depth = s.LastDepth > 0 ? s.LastDepth : 6;
        Detail = s.LastDetail ?? "salient";

        if (s.LastActiveSections is { Count: > 0 })
        {
            _sections.LoadSectionDefaults(s.LastActiveSections);
        }
        else
        {
            _sections.ApplyScenarioSectionDefaults();
        }
    }

    private void SaveSettings() =>
        _svc.SaveSettings(new AppSettings
        {
            LastScenario = SelectedScenario.Value,
            LastProfile = DerivedProfile,
            LastFormat = SelectedFormat,
            LastTokens = MaxTokens,
            LastAround = Around,
            IncludeProvenance = IncludeProvenance,
            IncludeDiagnostics = IncludeDiagnostics,
            NoRoslyn = NoRoslyn,
            LastDepth = Depth,
            LastDetail = Detail,
            LastActiveSections = _sections.Sections.Where(s => s.IsEnabled).Select(s => s.Key).ToList(),
        });

    private void RefreshRecent()
    {
        RecentPaths.Clear();
        foreach (var p in _svc.LoadRecent().Take(6))
            RecentPaths.Add(p);
    }

    [RelayCommand]
    private void ResetToScenarioDefaults()
    {
        _sections.ResetToDefaults();
        RebuildLlmViewText();
        OnPropertyChanged(nameof(LlmViewText));
    }

    private static RunReportHtmlRenderer.DetectionStats BuildDetectionStats(AnalysisSnapshot snapshot)
    {
        var model = snapshot.Model;
        return new RunReportHtmlRenderer.DetectionStats(
            EndpointCount: model.Detections.OfType<EndpointDetection>().Count(),
            MediatRHandlerCount: model.Detections.OfType<MediatRHandlerDetection>().Count(),
            EfEntityCount: model.Detections.OfType<EfEntityDetection>().Count(
                e => !string.Equals(e.DbContextType, "Migrations", StringComparison.Ordinal)),
            EfMigrationCount: model.Detections.OfType<EfEntityDetection>().Count(
                e => string.Equals(e.DbContextType, "Migrations", StringComparison.Ordinal)),
            BackgroundWorkerCount: model.Detections.OfType<BackgroundWorkerDetection>().Count(),
            MiddlewareCount: model.Detections.OfType<MiddlewareDetection>().Count(),
            IndirectWiringCount: model.Detections.OfType<IndirectWiringDetection>().Count(),
            MessageConsumerCount: model.Detections.OfType<MessageConsumerDetection>().Count(),
            DiRegistrationCount: model.Detections.OfType<DiRegistrationDetection>().Count(),
            AntiPatternCount: model.Detections.OfType<AntiPatternDetection>().Count(),
            EventFlowCount: model.Detections.OfType<EventFlowDetection>().Count(),
            TotalDetections: model.Detections.Count);
    }

    public void Dispose()
    {
        _analyzeOp.Dispose();
        _renderOp.Dispose();
        _validateOp.Dispose();
        _tokenDebouncer.Dispose();
        _git.Dispose();
    }
}
