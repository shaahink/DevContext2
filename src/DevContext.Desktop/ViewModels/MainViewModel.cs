using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevContext.Core.Models;
using DevContext.Core.Services;
using DevContext.Desktop.Services;

namespace DevContext.Desktop.ViewModels;

public record ScenarioItem(string Value, string Label)
{
    public override string ToString() => Label;
}

public partial class MainViewModel : ObservableObject
{
    private readonly IAnalysisService _svc;
    private readonly GitCloneService _git = new();
    private string _rawContent = "";
    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _validateCts;
    private bool _isInitializing = true;

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

    // ── Profile / format selection ─────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsProfileFocused),
        nameof(IsProfileDebug), nameof(IsProfileFull))]
    private string _selectedProfile = "focused";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormatMarkdown), nameof(IsFormatJson))]
    private string _selectedFormat = "markdown";

    [ObservableProperty] private ScenarioItem _selectedScenario = null!;

    public bool IsProfileFocused => SelectedProfile == "focused";
    public bool IsProfileDebug   => SelectedProfile == "debug";
    public bool IsProfileFull    => SelectedProfile == "full";
    public bool IsFormatMarkdown => SelectedFormat == "markdown";
    public bool IsFormatJson     => SelectedFormat == "json";

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

    // ── Section-based dual-view ─────────────────────────────────────────────────
    public ObservableCollection<SectionViewModel> Sections { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BudgetUtilisation))]
    private int _totalTokens;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BudgetUtilisation))]
    private int _budgetTokens = 8000;

    public float BudgetUtilisation => BudgetTokens > 0
        ? (float)TotalTokens / BudgetTokens
        : 0;

    public string DisplayText => IsHumanView ? HumanViewText : LlmViewText;

    public string LlmViewText
    {
        get
        {
            var parts = Sections.Where(s => s.IsIncluded).Select(s => s.FullText);
            return string.Join(Environment.NewLine, parts);
        }
    }

    public string HumanViewText => _rawContent;

    public string AnalyzeButtonText
        => IsGitHubUrl && GitRepoStatus == RepoStatus.Valid
            ? (IsAnalyzing ? "Cloning & Analyzing..." : "Clone & Analyze")
            : (IsAnalyzing ? "Analyzing..." : "Analyze");

    // ── GitHub repo analysis ────────────────────────────────────────────────────
    public bool IsGitAvailable => _git.IsGitAvailable;

    private RepoUrl? _gitRepoUrl;
    private RepoStatus _gitRepoStatus = RepoStatus.None;

    public bool IsGitHubUrl => _gitRepoUrl is { IsValid: true };
    public string GitRepoDisplay => _gitRepoUrl?.ToDisplay() ?? "";
    public RepoStatus GitRepoStatus => _gitRepoStatus;

    [ObservableProperty] private string _cloneCleanup = "auto";

    partial void OnProjectPathChanged(string value)
    {
        if (_isInitializing) return;
        ValidateGitHubUrl(value);
    }

    private async void ValidateGitHubUrl(string path)
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

        _validateCts = new CancellationTokenSource();
        var status = await _git.ValidateAsync(url, _validateCts.Token);
        _gitRepoStatus = status;

        OnPropertyChanged(nameof(GitRepoStatus));
        OnPropertyChanged(nameof(GitRepoDisplay));
        OnPropertyChanged(nameof(AnalyzeButtonText));
    }
    public string RawContent => _rawContent;

    // ── Collections ────────────────────────────────────────────────────────────
    public ObservableCollection<string> RecentPaths { get; } = [];

    public List<ScenarioItem> Scenarios { get; } =
    [
        new("architecture",          "Architecture"),
        new("debug-endpoint",        "Debug Endpoint"),
        new("add-similar-feature",   "Add Similar Feature"),
        new("modify-middleware",     "Modify Middleware"),
        new("trace-message-flow",    "Trace Message Flow"),
        new("harden-di",             "Harden DI"),
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
    partial void OnSelectedScenarioChanged(ScenarioItem value)  => OnAnalysisOptionChanged();
    partial void OnSelectedProfileChanged(string value)         => OnAnalysisOptionChanged();
    partial void OnSelectedFormatChanged(string value)          => OnAnalysisOptionChanged();
    partial void OnMaxTokensChanged(int value)                  => OnAnalysisOptionChanged();
    partial void OnAroundChanged(string value)                  => OnAnalysisOptionChanged();
    partial void OnIncludeProvenanceChanged(bool value)         => OnAnalysisOptionChanged();
    partial void OnIncludeDiagnosticsChanged(bool value)        => OnAnalysisOptionChanged();
    partial void OnNoRoslynChanged(bool value)                  => OnAnalysisOptionChanged();
    partial void OnDryRunChanged(bool value)                    => OnAnalysisOptionChanged();

    private void OnAnalysisOptionChanged()
    {
        if (_isInitializing || !HasOutput || string.IsNullOrWhiteSpace(ProjectPath))
            return;

        AnalyzeCommand.Execute(null);
    }

    // ── Commands ───────────────────────────────────────────────────────────────
    [RelayCommand]
    private void SetProfile(string profile) => SelectedProfile = profile;

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
            ProgressText = "Cloning from GitHub...";
            var clonePath = repo.ClonePath;

            var cloneResult = await _git.CloneAsync(repo, clonePath, repo.Ref,
                new Progress<string>(msg => ProgressText = msg), ct);

            if (cloneResult is null)
            {
                ProgressText = "Error";
                OutputText = $"Failed to clone {repo.ToDisplay()}. Check the URL, branch, or network connection.";
                HasOutput = true;
                IsAnalyzing = false;
                IsProgressIndeterminate = false;
                return;
            }

            workingPath = cloneResult;
        }

        var opts = new AnalysisOptions
        {
            ProjectPath = ProjectPath,
            Scenario = SelectedScenario.Value,
            Profile = SelectedProfile,
            Around = Around,
            MaxTokens = MaxTokens,
            Format = SelectedFormat,
            IncludeProvenance = IncludeProvenance,
            IncludeDiagnostics = IncludeDiagnostics,
            NoRoslyn = NoRoslyn,
            DryRun = DryRun,
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
            var result = await _svc.AnalyzeAsync(opts, progress, ct);
            if (result.Success)
            {
                _rawContent = result.Content ?? "";
                OutputText = _rawContent;
                PopulateSections(_rawContent);
                var tokens = _rawContent.Length / 4;
                StatsText = $"~{tokens:N0} tokens  ·  {result.ElapsedMs / 1000.0:F1}s";
                HasOutput = true;
                IsProgressIndeterminate = false;
                ProgressValue = 100;
                ProgressText = "Done";
                BudgetTokens = MaxTokens;
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
            ProgressText = "Cancelled";
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
        SelectedScenario = Scenarios.FirstOrDefault(sc => sc.Value == s.LastScenario) ?? Scenarios[1];
        SelectedProfile = s.LastProfile ?? "focused";
        SelectedFormat = s.LastFormat ?? "markdown";
        if (s.LastTokens > 0) MaxTokens = s.LastTokens;
        Around = s.LastAround ?? "";
        IncludeProvenance = s.IncludeProvenance;
        IncludeDiagnostics = s.IncludeDiagnostics;
        NoRoslyn = s.NoRoslyn;
    }

    private void SaveSettings() =>
        _svc.SaveSettings(new AppSettings
        {
            LastScenario = SelectedScenario.Value,
            LastProfile = SelectedProfile,
            LastFormat = SelectedFormat,
            LastTokens = MaxTokens,
            LastAround = Around,
            IncludeProvenance = IncludeProvenance,
            IncludeDiagnostics = IncludeDiagnostics,
            NoRoslyn = NoRoslyn,
        });

    private void RefreshRecent()
    {
        RecentPaths.Clear();
        foreach (var p in _svc.LoadRecent().Take(6))
            RecentPaths.Add(p);
    }

    private void PopulateSections(string output)
    {
        Sections.Clear();

        // Split by ## section headers
        var parts = output.Split("## ");
        foreach (var part in parts.Skip(1)) // Skip content before first ##
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
            };

            section.PropertyChanged += (_, _) => OnPropertyChanged(nameof(LlmViewText));
            Sections.Add(section);
        }

        OnPropertyChanged(nameof(LlmViewText));
        OnPropertyChanged(nameof(HumanViewText));
    }
}
