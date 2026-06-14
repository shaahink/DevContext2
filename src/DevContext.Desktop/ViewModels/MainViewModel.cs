using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevContext.Core.Contracts;
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
    private readonly SnapshotCache _cache = new(capacity: 8);
    private AnalysisSnapshot? _snapshot;
    private AnalysisKey? _displayedKey;
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

    private AnalysisKey BuildAnalysisKey() =>
        new(ProjectPath, SelectedScenario.Value, Around, DerivedProfile, NoRoslyn, DryRun, IncludeAntiPatterns);

    public bool IsStale =>
        HasOutput && _displayedKey is not null && _displayedKey != BuildAnalysisKey();

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
        if (key == DevContext.Core.Constants.SectionNames.CallGraph || key == "__source__")
            MarkAnalysisInputsChanged();
        else
            OnRenderInputChanged();
    }

    private void SetSectionEnabledSilent(string key, bool enabled)
    {
        _sections.SetSectionEnabled(key, enabled);
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
        };
        _sections.OnSectionToggled = (key, included) =>
        {
            SetSectionEnabledSilent(key, included);
            if (!_sections.IsBuildingSections)
                OnRenderInputChanged();
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
        MarkAnalysisInputsChanged();
    }
    partial void OnSelectedFormatChanged(string value) => OnRenderInputChanged();

    partial void OnMaxTokensChanged(int value) => DebouncedReanalyze();
    partial void OnAroundChanged(string value) => MarkAnalysisInputsChanged();
    partial void OnIncludeProvenanceChanged(bool value) => OnRenderInputChanged();
    partial void OnIncludeDiagnosticsChanged(bool value) => OnRenderInputChanged();
    partial void OnNoRoslynChanged(bool value) => MarkAnalysisInputsChanged();
    partial void OnDryRunChanged(bool value)                    => MarkAnalysisInputsChanged();
    partial void OnIncludeAntiPatternsChanged(bool value)        => MarkAnalysisInputsChanged();

    private void MarkAnalysisInputsChanged()
    {
        if (_isInitializing || !HasOutput || string.IsNullOrWhiteSpace(ProjectPath))
            return;

        var key = BuildAnalysisKey();
        if (_cache.TryGet(key, out var snapshot))
        {
            _snapshot = snapshot;
            _displayedKey = key;
            _ = RerenderAsync();
        }

        OnPropertyChanged(nameof(IsStale));
        OnPropertyChanged(nameof(AnalyzeButtonText));
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

        _output.IsAnalyzing = true;
        _output.IsProgressVisible = true;
        _output.ProgressText = "Starting...";
        _output.HasOutput = false;
        _output.StatsText = "";
        _output.RawContent = "";
        _output.HumanViewHtml = "";
        _snapshot = null;

        var key = BuildAnalysisKey();

        // Cache-hit fast path: skip clone + pipeline, serve from cache
        if (_cache.TryGet(key, out var cached))
        {
            _snapshot = cached;
            _displayedKey = key;
            await RerenderAsync(ct).ConfigureAwait(true);
            _output.HasOutput = true;
            _output.IsAnalyzing = false;
            _output.IsProgressVisible = false;
            OnPropertyChanged(nameof(IsStale));
            return;
        }

        _svc.AddRecent(ProjectPath);
        RefreshRecent();

        var workingPath = ProjectPath;

        // Clone from GitHub if this is a repo URL
        if (_gitRepoUrl is { IsValid: true } repo)
        {
            var clonePath = repo.ClonePath;

            var cloneResult = await _git.CloneAsync(repo, clonePath, repo.Ref,
                new Progress<CloneProgress>(p => _output.ProgressText = $"{p.Phase}: {p.PercentComplete}%"), ct).ConfigureAwait(true);

            if (cloneResult is null)
            {
                _output.ProgressText = "Error";
                _output.RawContent = $"Failed to clone {repo.ToDisplay()}...";
                _output.HasOutput = true;
                _output.IsAnalyzing = false;
                _output.IsProgressVisible = false;
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
            _output.ProgressText = p.Text;
        });

        try
        {
            var snapResult = await _svc.AnalyzeAsync(opts, progress, ct).ConfigureAwait(true);
            if (snapResult.Success && snapResult.Snapshot is not null)
            {
                _snapshot = snapResult.Snapshot;
                _cache.Set(key, _snapshot);
                _displayedKey = key;
                var elapsedMs = snapResult.ElapsedMs;

                // Initial render from the snapshot
                await RerenderAsync(ct).ConfigureAwait(true);

                if (ct.IsCancellationRequested) return;

                _output.StatsHtml = _snapshot?.Report is { } r
                    ? RunReportHtmlRenderer.Render(r) : "";

                _output.StatsText = $"~{_sections.TotalTokens:N0} tokens · {elapsedMs / 1000.0:F1}s";
                _output.HasOutput = true;
                _output.ProgressText = snapResult.Explanation.Length > 0
                    ? snapResult.Explanation : "Done";
                _sections.BudgetTokens = capturedBudget;
                OnPropertyChanged(nameof(IsStale));

                OnPropertyChanged(string.Empty);

                // Clean up clone according to cleanup mode
                if (_gitRepoUrl is { } gitRepo)
                {
                    var clonePath = gitRepo.ClonePath;
                    switch (CloneCleanup)
                    {
                        case "auto":
                            await System.Threading.Tasks.Task.Run(() =>
                                GitCloneService.Cleanup(clonePath)).ConfigureAwait(false);
                            break;
                        case "session":
                            GitCloneService.RegisterForSessionCleanup(clonePath);
                            break;
                        // "24h" and "keep" — no immediate cleanup; 24h freshness checked on next run
                    }
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
                ? RunReportHtmlRenderer.Render(r) : "";
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
        _cache.Clear();
        GitCloneService.CleanupSession();
        _analyzeOp.Dispose();
        _renderOp.Dispose();
        _validateOp.Dispose();
        _tokenDebouncer.Dispose();
        _git.Dispose();
    }
}
