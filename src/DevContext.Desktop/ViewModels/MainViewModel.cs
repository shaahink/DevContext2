using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevContext.Core.Contracts;
using DevContext.Core.Models;
using DevContext.Core.Pipeline;
using DevContext.Core.Rendering;
using DevContext.Core.Services;
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
    private AnalysisSnapshot? _snapshot;
    private string _rawContent = "";
    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _renderCts;
    private CancellationTokenSource? _validateCts;
    private CancellationTokenSource? _maxTokensDebounceCts;
    private bool _isInitializing = true;

    // ── Section selection (forwarded from SectionSelectionModel) ───────────────
    public SectionSelectionModel SectionSelection => _sections;
    public ImmutableArray<SectionGroupViewModel> SectionGroups => _sections.SectionGroups;
    public List<SectionToggle> Sections => _sections.Sections;
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

    // ── Format selection ───────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormatMarkdown), nameof(IsFormatJson))]
    private string _selectedFormat = "markdown";

    [ObservableProperty] private ScenarioItem _selectedScenario = null!;

    public bool IsFormatMarkdown => SelectedFormat == "markdown";
    public bool IsFormatJson => SelectedFormat == "json";

    // ── Analysis state ─────────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AnalyzeButtonText))]
    private bool _isAnalyzing;

    [ObservableProperty] private bool _isProgressVisible;
    [ObservableProperty] private bool _isProgressIndeterminate;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText = "";

    // ── Output ─────────────────────────────────────────────────────────────────
    [ObservableProperty] private bool _hasOutput;
    [ObservableProperty] private string _statsText = "";
    [ObservableProperty] private bool _isHumanView = true;
    [ObservableProperty] private bool _isSectionPanelVisible = true;

    partial void OnIsHumanViewChanged(bool value) => RefreshDisplayText();

    [ObservableProperty] private string _displayText = "";

    private string _cachedLlmViewText = "";
    private string _statsHtml = "";

    public string StatsHtml => _statsHtml;

    public void RefreshDisplayText() => DisplayText = IsHumanView ? HumanViewText : LlmViewText;

    public string LlmViewText => _cachedLlmViewText;

    private void RebuildLlmViewText()
    {
        var parts = _sections.SectionGroups
            .SelectMany(g => g.Children)
            .Where(s => s.IsIncluded)
            .Select(s => s.FullText);
        _cachedLlmViewText = string.Join(Environment.NewLine, parts);
    }

    public string HumanViewText => _rawContent;

    private string? _humanViewHtml;
    public string HumanViewHtml => _humanViewHtml ?? "";

    public string AnalyzeButtonText
        => IsGitHubUrl
            ? (IsAnalyzing ? "Cloning & Analyzing..." : "Clone & Analyze")
            : HasOutput
                ? (IsAnalyzing ? "Analyzing..." : $"Analyze (~{SelectedTokenTotal:N0} tok)")
                : (IsAnalyzing ? "Analyzing..." : "Analyze");

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
        _validateCts?.Cancel();
        _validateCts?.Dispose();
        _validateCts = new CancellationTokenSource();
        var ct = _validateCts.Token;

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
    public string RawContent => _rawContent;

    // ── Collections ────────────────────────────────────────────────────────────
    public ObservableCollection<string> RecentPaths { get; } = [];

    public void SetSectionEnabled(string key, bool enabled)
    {
        _sections.SetSectionEnabled(key, enabled);
        OnRenderInputChanged();
    }

    public List<ScenarioItem> Scenarios { get; } =
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
            RefreshDisplayText();
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

    private void OnAnalysisInputChanged()
    {
        if (_isInitializing || !HasOutput || string.IsNullOrWhiteSpace(ProjectPath))
            return;

        AnalyzeCommand.Execute(null);
    }

    private void OnRenderInputChanged()
    {
        if (_isInitializing || !HasOutput || _snapshot is null || string.IsNullOrWhiteSpace(ProjectPath))
            return;

        _ = RerenderAsync();
    }

    private void DebouncedReanalyze()
    {
        if (_isInitializing || !HasOutput || string.IsNullOrWhiteSpace(ProjectPath))
            return;

        _maxTokensDebounceCts?.Cancel();
        _maxTokensDebounceCts?.Dispose();
        _maxTokensDebounceCts = new CancellationTokenSource();
        var ct = _maxTokensDebounceCts.Token;

        _ = System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    await System.Threading.Tasks.Task.Delay(500, ct).ConfigureAwait(false);
                if (!ct.IsCancellationRequested)
                {
                    var dispatcher = System.Windows.Application.Current?.Dispatcher;
                    if (dispatcher != null)
                        dispatcher.Invoke(() => OnRenderInputChanged());
                    else
                        OnRenderInputChanged();
                }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) { Log.Error(ex, "Debounced reanalysis failed"); }
            }, ct);
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
        CancelPrevious();
        CancelRender();

        _cts = new CancellationTokenSource();
        var myCts = _cts;
        var ct = myCts.Token;
        var capturedBudget = MaxTokens;

        IsAnalyzing = true;
        IsProgressVisible = true;
        IsProgressIndeterminate = true;
        ProgressValue = 0;
        ProgressText = "Starting...";
        HasOutput = false;
        StatsText = "";
        _rawContent = "";
        _humanViewHtml = null;
        _snapshot = null;

        _svc.AddRecent(ProjectPath);
        RefreshRecent();

        var workingPath = ProjectPath;

        // Clone from GitHub if this is a repo URL
        if (_gitRepoUrl is { IsValid: true } repo)
        {
            var clonePath = repo.ClonePath;

            var cloneResult = await _git.CloneAsync(repo, clonePath, repo.Ref,
                new Progress<CloneProgress>(p => ProgressText = $"{p.Phase}: {p.PercentComplete}%"), ct).ConfigureAwait(true);

            if (cloneResult is null)
            {
                ProgressText = "Error";
                _rawContent = $"Failed to clone {repo.ToDisplay()}...";
                HasOutput = true;
                IsAnalyzing = false;
                IsProgressIndeterminate = false;
                IsProgressVisible = false;
                return;
            }

            workingPath = cloneResult;
        }

        var opts = new AnalysisOptions
        {
            ProjectPath = workingPath,
            Scenario = SelectedScenario.Value,
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
        };

        var progress = new Progress<AnalysisProgress>(p =>
        {
            ProgressText = p.Text;
            if (p.Value.HasValue)
            {
                IsProgressIndeterminate = false;
                ProgressValue = p.Value.Value;
            }
        });

        try
        {
            var snapResult = await _svc.AnalyzeAsync(opts, progress, ct).ConfigureAwait(true);
            if (snapResult.Success && snapResult.Snapshot is not null)
            {
                _snapshot = snapResult.Snapshot;
                var elapsedMs = snapResult.ElapsedMs;

                // Initial render from the snapshot
                await RerenderAsync(ct).ConfigureAwait(true);

                if (ct.IsCancellationRequested) return;

                _statsHtml = _snapshot?.Report is { } r
                    ? RunReportHtmlRenderer.Render(r) : "";

#pragma warning disable MVVMTK0034
                _statsText = $"~{_sections.TotalTokens:N0} tokens · {elapsedMs / 1000.0:F1}s";
                _hasOutput = true;
                _isProgressIndeterminate = false;
                _progressValue = 100;
                _progressText = "Done";
                _sections.BudgetTokens = capturedBudget;
#pragma warning restore MVVMTK0034

                // Single notification to refresh all bindings
                OnPropertyChanged(string.Empty);

                // Clean up clone off UI thread
                if (_gitRepoUrl is { } gitRepo && CloneCleanup == "auto")
                {
                    var clonePath = gitRepo.ClonePath;
                    await System.Threading.Tasks.Task.Run(() =>
                        GitCloneService.Cleanup(clonePath)).ConfigureAwait(false);
                }
            }
            else
            {
                if (ct.IsCancellationRequested) return;
                ProgressText = "Error";
                _rawContent = snapResult.Error ?? "Analysis failed.";
                HasOutput = true;
                IsProgressIndeterminate = false;
                ProgressValue = 0;
            }
        }
        catch (OperationCanceledException)
        {
            if (ct.IsCancellationRequested) return;
            ProgressText = "Canceled";
        }
        catch (Exception ex)
        {
            if (ct.IsCancellationRequested) return;
            Log.Error(ex, "Analysis failed");
            ProgressText = "Error";
            _rawContent = ex.Message;
            HasOutput = true;
            IsProgressIndeterminate = false;
        }
        finally
        {
            IsAnalyzing = false;
            IsProgressVisible = false;
            IsProgressIndeterminate = false;
            _ = System.Threading.Tasks.Task.Run(SaveSettings);
            if (_cts == myCts)
            {
                _cts?.Dispose();
                _cts = null;
            }
        }
    }

    private void CancelPrevious()
    {
        if (_cts is { } cts)
        {
            cts.Cancel();
            cts.Dispose();
            _cts = null;
        }
    }

    private async Task RerenderAsync(CancellationToken ct = default)
    {
        if (_snapshot is null) return;

        CancelRender();
        var cts = new CancellationTokenSource();
        _renderCts = cts;
        var renderCt = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token).Token;

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
            };

            var renderResult = await _svc.RenderAsync(_snapshot, request, renderCt).ConfigureAwait(true);

            if (renderCt.IsCancellationRequested) return;

            var rawContent = renderResult.Content ?? "";

            var (_, llmText, _, _) = _sections.BuildSectionDataFromStat(renderResult.Sections);

            if (renderCt.IsCancellationRequested) return;

            _cachedLlmViewText = llmText;
            _rawContent = rawContent;
            _humanViewHtml = renderResult.HtmlContent;
            _sections.BudgetTokens = MaxTokens;
#pragma warning disable MVVMTK0034
            _displayText = IsHumanView ? rawContent : llmText;
#pragma warning restore MVVMTK0034

            if (renderCt.IsCancellationRequested) return;

            // Update stats from render result
            _statsHtml = _snapshot?.Report is { } r
                ? RunReportHtmlRenderer.Render(r) : "";
#pragma warning disable MVVMTK0034
            _statsText = _snapshot?.Report is { } report
                ? RunReportFormatter.Summary(report, renderResult.RenderFunnel)
                : "";
#pragma warning restore MVVMTK0034

            OnPropertyChanged(string.Empty);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Log.Error(ex, "Re-render failed");
        }
        finally
        {
            if (_renderCts == cts)
            {
                _renderCts?.Dispose();
                _renderCts = null;
            }
        }
    }

    private void CancelRender()
    {
        _renderCts?.Cancel();
        _renderCts?.Dispose();
        _renderCts = null;
    }

    // ── Settings helpers ───────────────────────────────────────────────────────
    private void LoadSettings()
    {
        var s = _svc.LoadSettings();
        SelectedScenario = Scenarios.FirstOrDefault(sc => sc.Value == s.LastScenario) ?? Scenarios[0];
        SelectedFormat = s.LastFormat ?? "markdown";
        if (s.LastTokens > 0) MaxTokens = s.LastTokens;
        Around = s.LastAround ?? "";
        IncludeProvenance = s.IncludeProvenance;
        IncludeDiagnostics = s.IncludeDiagnostics;
        NoRoslyn = s.NoRoslyn;

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

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _renderCts?.Cancel();
        _renderCts?.Dispose();
        _renderCts = null;
        _validateCts?.Cancel();
        _validateCts?.Dispose();
        _validateCts = null;
        _maxTokensDebounceCts?.Cancel();
        _maxTokensDebounceCts?.Dispose();
        _maxTokensDebounceCts = null;
        _git.Dispose();
    }
}
