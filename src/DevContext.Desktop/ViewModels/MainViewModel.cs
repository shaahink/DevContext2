using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevContext.Core.Models;
using DevContext.Core.Services;
using DevContext.Desktop.Services;

namespace DevContext.Desktop.ViewModels;

public sealed class SectionToggle
{
    public string Key { get; init; } = "";      // matches SectionNames constant value
    public string Label { get; init; } = "";
    public string? Hint { get; init; }          // shown next to checkbox for expensive sections
    public bool IsEnabled { get; set; } = true;
}

public record ScenarioItem(string Value, string Label)
{
    public override string ToString() => Label;
}

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IAnalysisService _svc;
    private readonly GitCloneService _git = new();
    private string _rawContent = "";
    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _validateCts;
    private CancellationTokenSource? _maxTokensDebounceCts;
    private readonly object _ctsLock = new();
    private bool _isInitializing = true;

    // ── Form fields ────────────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AnalyzeCommand))]
    private string _projectPath = "";

    [ObservableProperty] private string _around = "";
    [ObservableProperty] private string _task = "";
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
    [ObservableProperty] private string _outputText = "";
    [ObservableProperty] private string _statsText = "";
    [ObservableProperty] private bool _isHumanView = true;
    [ObservableProperty] private bool _isSectionPanelVisible = true;

    partial void OnIsHumanViewChanged(bool value) => RefreshDisplayText();

    // ── Section-based dual-view ─────────────────────────────────────────────────
    public ObservableCollection<SectionGroupViewModel> SectionGroups { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BudgetUtilisation))]
    private int _selectedTokenTotal;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BudgetUtilisation))]
    private int _totalTokens;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BudgetUtilisation))]
    private int _budgetTokens = 8000;

    public float BudgetUtilisation => BudgetTokens > 0
        ? (float)TotalTokens / BudgetTokens
        : 0;

    [ObservableProperty] private string _displayText = "";

    public void RefreshDisplayText() => DisplayText = IsHumanView ? HumanViewText : LlmViewText;

    public string LlmViewText
    {
        get
        {
            var parts = SectionGroups
                .SelectMany(g => g.Children)
                .Where(s => s.IsIncluded)
                .Select(s => s.FullText);
            return string.Join(Environment.NewLine, parts);
        }
    }

    public string HumanViewText => _rawContent;

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
            _validateCts = new CancellationTokenSource();
            var status = await _git.ValidateAsync(url, _validateCts.Token).ConfigureAwait(true);
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

    public List<SectionToggle> Sections { get; } =
    [
        new() { Key = DevContext.Core.Constants.SectionNames.ArchitectureOverview, Label = "Architecture overview" },
        new() { Key = DevContext.Core.Constants.SectionNames.Endpoints,            Label = "Endpoints" },
        new() { Key = DevContext.Core.Constants.SectionNames.MediatRHandlers,      Label = "MediatR Handlers" },
        new() { Key = DevContext.Core.Constants.SectionNames.DataModel,            Label = "Data model" },
        new() { Key = DevContext.Core.Constants.SectionNames.NonObviousWiring,     Label = "DI / Wiring" },
        new() { Key = DevContext.Core.Constants.SectionNames.CallGraph,            Label = "Call graph", Hint = "+call graph, needs Roslyn" },
        new() { Key = DevContext.Core.Constants.SectionNames.MessageConsumers,     Label = "Message consumers" },
        new() { Key = DevContext.Core.Constants.SectionNames.RelatedTypes,         Label = "Related types" },
        new() { Key = "__source__",                                                Label = "Source code", Hint = "adds full C# bodies, +2k\u201312k tokens" },
    ];

    public bool IsTraceMode => SelectedScenario?.Value == "deep-dive";

    public string DerivedProfile
    {
        get
        {
            var sourceOn = Sections.FirstOrDefault(s => s.Key == "__source__")?.IsEnabled == true;
            var callGraphOn = Sections.FirstOrDefault(s => s.Key == DevContext.Core.Constants.SectionNames.CallGraph)?.IsEnabled == true;
            if (sourceOn) return "full";
            if (callGraphOn) return "debug";
            return "focused";
        }
    }

    public void SetSectionEnabled(string key, bool enabled)
    {
        var section = Sections.FirstOrDefault(s => s.Key == key);
        if (section is null) return;
        section.IsEnabled = enabled;
        OnAnalysisOptionChanged();
    }

    private void ApplyScenarioSectionDefaults()
    {
        if (IsTraceMode)
        {
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.ArchitectureOverview, false);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.Endpoints, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.MediatRHandlers, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.DataModel, false);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.NonObviousWiring, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.CallGraph, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.MessageConsumers, false);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.RelatedTypes, false);
            SetSectionEnabledSilent("__source__", false);
        }
        else
        {
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.ArchitectureOverview, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.Endpoints, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.MediatRHandlers, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.DataModel, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.NonObviousWiring, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.CallGraph, false);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.MessageConsumers, false);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.RelatedTypes, true);
            SetSectionEnabledSilent("__source__", false);
        }
    }

    private void SetSectionEnabledSilent(string key, bool enabled)
    {
        var section = Sections.FirstOrDefault(s => s.Key == key);
        if (section is not null) section.IsEnabled = enabled;
    }

    private ImmutableArray<string> GetActiveSections()
        => Sections
            .Where(s => s.IsEnabled && s.Key != "__source__")
            .Select(s => s.Key)
            .ToImmutableArray();

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
        _isInitializing = false;
    }

    // ── Auto-reanalyze on option change ────────────────────────────────────────
    partial void OnSelectedScenarioChanged(ScenarioItem value)
    {
        if (_isInitializing) return;
        ApplyScenarioSectionDefaults();
        OnPropertyChanged(nameof(IsTraceMode));
        ResetToScenarioDefaults();
        OnAnalysisOptionChanged();
    }
    partial void OnSelectedFormatChanged(string value) => OnAnalysisOptionChanged();
    partial void OnTaskChanged(string value) => OnAnalysisOptionChanged();

    partial void OnMaxTokensChanged(int value) => DebouncedReanalyze();
    partial void OnAroundChanged(string value) => OnAnalysisOptionChanged();
    partial void OnIncludeProvenanceChanged(bool value) => OnAnalysisOptionChanged();
    partial void OnIncludeDiagnosticsChanged(bool value) => OnAnalysisOptionChanged();
    partial void OnNoRoslynChanged(bool value) => OnAnalysisOptionChanged();
    partial void OnDryRunChanged(bool value)                    => OnAnalysisOptionChanged();
    partial void OnIncludeAntiPatternsChanged(bool value)        => OnAnalysisOptionChanged();

    private void OnAnalysisOptionChanged()
    {
        if (_isInitializing || !HasOutput || string.IsNullOrWhiteSpace(ProjectPath))
            return;

        AnalyzeCommand.Execute(null);
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
                        dispatcher.Invoke(() => OnAnalysisOptionChanged());
                    else
                        OnAnalysisOptionChanged();
                }
                }
                catch (OperationCanceledException) { }
                catch (Exception) { /* best effort */ }
            }, ct);
    }

    // ── Commands ───────────────────────────────────────────────────────────────
    [RelayCommand]
    private void SetFormat(string format) => SelectedFormat = format;

    [RelayCommand]
    private void SelectRecent(string path) => ProjectPath = path;

    private bool CanAnalyze() => !string.IsNullOrWhiteSpace(ProjectPath);

    [RelayCommand(CanExecute = nameof(CanAnalyze))]
    private async Task AnalyzeAsync()
    {
        CancelPrevious();

        _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        var capturedBudget = MaxTokens;

        IsAnalyzing = true;
        IsProgressVisible = true;
        IsProgressIndeterminate = true;
        ProgressValue = 0;
        ProgressText = "Starting...";
        HasOutput = false;
        OutputText = "";
        StatsText = "";
        _rawContent = "";

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
                OutputText = $"Failed to clone {repo.ToDisplay()}. Check the URL, branch, or network connection.";
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
            Task = Task,
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
            var result = await _svc.AnalyzeAsync(opts, progress, ct).ConfigureAwait(true);
            if (result.Success)
            {
                _rawContent = result.Content ?? "";
                OutputText = _rawContent;
                PopulateSections(_rawContent);
                RefreshDisplayText();
                var tokens = _rawContent.Length / 4;
                StatsText = $"~{tokens:N0} tokens · {result.ElapsedMs / 1000.0:F1}s";
                HasOutput = true;
                IsProgressIndeterminate = false;
                ProgressValue = 100;
                ProgressText = "Done";
                BudgetTokens = capturedBudget;
                TotalTokens = tokens;

                // Clean up clone if auto-clean
                if (_gitRepoUrl is { } gitRepo && CloneCleanup == "auto")
                    GitCloneService.Cleanup(gitRepo.ClonePath);
            }
            else
            {
                ProgressText = "Error";
                OutputText = result.Error ?? "Analysis failed.";
                HasOutput = true;
                IsProgressIndeterminate = false;
                ProgressValue = 0;
            }
        }
        catch (OperationCanceledException)
        {
            ProgressText = "Canceled";
        }
        catch (Exception ex)
        {
            ProgressText = "Error";
            OutputText = ex.Message;
            HasOutput = true;
            IsProgressIndeterminate = false;
        }
        finally
        {
            IsAnalyzing = false;
            IsProgressVisible = false;
            IsProgressIndeterminate = false;
            SaveSettings();
            _cts?.Dispose();
            _cts = null;
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
        Task = s.LastTask ?? "";

        if (s.LastActiveSections is { Count: > 0 })
        {
            foreach (var section in Sections)
                section.IsEnabled = s.LastActiveSections.Contains(section.Key);
        }
        else
        {
            ApplyScenarioSectionDefaults();
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
            LastTask = Task,
            LastActiveSections = Sections.Where(s => s.IsEnabled).Select(s => s.Key).ToList(),
        });

    private void RefreshRecent()
    {
        RecentPaths.Clear();
        foreach (var p in _svc.LoadRecent().Take(6))
            RecentPaths.Add(p);
    }

    private void PopulateSections(string output)
    {
        SectionGroups.Clear();

        var sections = new List<SectionViewModel>();

        var parts = output.Split("## ");
        foreach (var part in parts.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(part)) continue;
            var newlineIdx = part.IndexOf('\n');
            var name = newlineIdx > 0 ? part[..newlineIdx].Trim() : part.Trim();
            var fullText = "## " + part;
            var tokens = Math.Max(1, fullText.Length / 4);

            var section = new SectionViewModel
            {
                Name = name,
                FullText = fullText,
                RawTokens = tokens,
                CompressedTokens = tokens,
                Category = CategorizeSection(name),
            };

            section.PropertyChanged += (_, _) =>
            {
                RecalcTokenTotal();
                if (IsHumanView) IsHumanView = false; // Auto-switch to LLM view to show effect
                RefreshDisplayText();
            };

            sections.Add(section);
        }

        // Group by category
        var categoryOrder = new[] { "API", "Architecture", "Data", "Analysis", "Debug", "Other" };
        foreach (var cat in categoryOrder)
        {
            var children = sections.Where(s => s.Category == cat).ToList();
            if (children.Count == 0) continue;

            var group = new SectionGroupViewModel
            {
                Name = cat,
                IsExpanded = cat != "Debug",
            };
            group.Children.AddRange(children);
            group.PropertyChanged += (_, _) => RecalcTokenTotal();
            SectionGroups.Add(group);
        }

        TotalTokens = sections.Sum(s => s.CompressedTokens);
        RecalcTokenTotal();
        OnPropertyChanged(nameof(LlmViewText));
        OnPropertyChanged(nameof(HumanViewText));
    }

    private void RecalcTokenTotal()
    {
        SelectedTokenTotal = SectionGroups
            .SelectMany(g => g.Children)
            .Where(s => s.IsIncluded)
            .Sum(s => s.CompressedTokens);
    }

    private static string CategorizeSection(string name)
    {
        var lower = name.ToLowerInvariant();
        if (lower.Contains("endpoint") || lower.Contains("call graph") || lower.Contains("mediatr") || lower.Contains("handler"))
            return "API";
        if (lower.Contains("architecture") || lower.Contains("project") || lower.Contains("di regist") || lower.Contains("non-obvious") || lower.Contains("middleware") || lower.Contains("signal"))
            return "Architecture";
        if (lower.Contains("data model") || lower.Contains("entity") || lower.Contains("message consumer") || lower.Contains("event flow"))
            return "Data";
        if (lower.Contains("anti-pattern") || lower.Contains("related type") || lower.Contains("entry point"))
            return "Analysis";
        if (lower.Contains("diagnostic") || lower.Contains("pruning") || lower.Contains("source code") || lower.Contains("hotpath"))
            return "Debug";
        return "Other";
    }

    [RelayCommand]
    private void ResetToScenarioDefaults()
    {
        foreach (var group in SectionGroups)
            foreach (var section in group.Children)
            {
                // Always include unless it's Debug/Diagnostics category
                section.IsIncluded = section.Category != "Debug";
            }
        RecalcTokenTotal();
        OnPropertyChanged(nameof(LlmViewText));
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _validateCts?.Cancel();
        _validateCts?.Dispose();
        _validateCts = null;
        _maxTokensDebounceCts?.Cancel();
        _maxTokensDebounceCts?.Dispose();
        _maxTokensDebounceCts = null;
        _git.Dispose();
    }
}
