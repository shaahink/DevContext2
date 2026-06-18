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
            OnPropertyChanged(nameof(LlmViewText));
        }
    }

    private string _statsHtml = "";
    public string StatsHtml
    {
        get => _statsHtml;
        set
        {
            _statsHtml = value;
            OnPropertyChanged(nameof(StatsHtml));
        }
    }

    [ObservableProperty]
    private string _statsText = "";

    // ── Progress / status ────────────────────────────────────────────────────
    [ObservableProperty]
    private bool _hasOutput;

    [ObservableProperty]
    private bool _isAnalyzing;

    [ObservableProperty]
    private bool _isProgressVisible;

    [ObservableProperty]
    private string _progressText = "";

    /// <summary>Determinate progress percentage (0–100), or null for indeterminate.</summary>
    [ObservableProperty]
    private double? _progressValue;
}
