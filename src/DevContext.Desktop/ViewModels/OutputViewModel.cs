using CommunityToolkit.Mvvm.ComponentModel;

namespace DevContext.Desktop.ViewModels;

/// <summary>Holds the rendered output and the single source of truth for the active tab. Fixes M1.</summary>
public partial class OutputViewModel : ObservableObject
{
    public enum OutputTab { Human, Llm, Stats }

    [ObservableProperty]
    private OutputTab _selectedTab = OutputTab.Human;

    // ── Output content ───────────────────────────────────────────────────────
    public string RawContent { get; set; } = "";
    public string HumanViewHtml { get; set; } = ""; // non-null after null-forgiving assignment from caller

    private string _llmViewText = "";
    public string LlmViewText
    {
        get => _llmViewText;
        set
        {
            _llmViewText = value;
            OnPropertyChanged(nameof(DisplayContent));
        }
    }

    private string _statsHtml = "";
    public string StatsHtml
    {
        get => _statsHtml;
        set
        {
            _statsHtml = value;
            OnPropertyChanged(nameof(DisplayContent));
        }
    }

    [ObservableProperty]
    private string _statsText = "";

    // ── Computed display ─────────────────────────────────────────────────────
    public string DisplayContent => SelectedTab switch
    {
        OutputTab.Human => RawContent,
        OutputTab.Llm => LlmViewText,
        OutputTab.Stats => StatsHtml,
        _ => ""
    };

    public string DisplayHtml => SelectedTab == OutputTab.Human ? HumanViewHtml : "";

    // ── Progress / status ────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsActive))]
    private bool _hasOutput;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsActive))]
    private bool _isAnalyzing;

    [ObservableProperty]
    private bool _isProgressVisible;

    [ObservableProperty]
    private string _progressText = "";

    public bool IsActive => HasOutput && !IsAnalyzing;
}
