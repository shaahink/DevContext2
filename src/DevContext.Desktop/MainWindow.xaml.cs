using System.IO;
using System.Text.Json;
using System.Windows;
using DevContext.Desktop.ViewModels;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace DevContext.Desktop;

public partial class MainWindow
{
    private readonly MainViewModel _vm = null!;
    private readonly string _logPath;

    public MainWindow()
    {
        _logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DevContext", "startup.log");
        Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
        Log("Starting DevContext Desktop...");

        try
        {
            var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
            if (string.IsNullOrEmpty(version))
            {
                Log("ERROR: WebView2 runtime not found");
                MessageBox.Show(
                    "WebView2 runtime is required.\n\nClick OK to open the download page.",
                    "WebView2 Runtime Required");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://developer.microsoft.com/microsoft-edge/webview2/",
                    UseShellExecute = true
                })?.Dispose();
                Application.Current.Shutdown();
                return;
            }
            Log($"WebView2 runtime: {version}");

            InitializeComponent();

            _vm = new MainViewModel();
            _vm.PropertyChanged += (_, e) => Dispatcher.Invoke(() => SyncState(e.PropertyName));

            WebView.CoreWebView2InitializationCompleted += OnCoreWebView2Ready;
            WebView.NavigationCompleted += (_, _) =>
            {
                Log("Navigation completed");
                Dispatcher.Invoke(() => SyncAllState());
            };

            var htmlPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "index.html");
            if (File.Exists(htmlPath))
            {
                WebView.Source = new Uri(htmlPath);
                Log($"Loading: {htmlPath}");
            }
            else
            {
                Log($"ERROR: index.html not found at {htmlPath}");
                WebView.NavigateToString($"<h1>index.html not found</h1><p>Expected at: {htmlPath}</p>");
            }
        }
        catch (Exception ex)
        {
            Log($"FATAL: {ex}");
            MessageBox.Show($"Failed to start:\n\n{ex.Message}", "Startup Error");
            Application.Current.Shutdown();
        }
    }

    private void OnCoreWebView2Ready(object? sender, EventArgs e)
    {
        Log("CoreWebView2 ready");
        WebView.CoreWebView2.WebMessageReceived += OnWebMessage;
    }

    private void SyncState(string? propertyName)
    {
        if (WebView?.CoreWebView2 == null) return;

        var json = JsonSerializer.Serialize(new UiState(_vm));
        WebView.CoreWebView2.ExecuteScriptAsync($"if(window.setDevContextState)window.setDevContextState({json})");
    }

    private void SyncAllState() => SyncState(null);

    private void OnWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var msg = JsonSerializer.Deserialize<JsMessage>(e.WebMessageAsJson);
            if (msg == null) return;

            switch (msg.Action)
            {
                case "analyze": _vm.AnalyzeCommand.Execute(null); break;
                case "setProfile": _vm.SetProfileCommand.Execute(msg.Value); break;
                case "setFormat": _vm.SetFormatCommand.Execute(msg.Value); break;
                case "setScenario": _vm.SelectedScenario = _vm.Scenarios.First(s => s.Value == msg.Value); break;
                case "setProjectPath": _vm.ProjectPath = msg.Value ?? ""; break;
                case "setAround": _vm.Around = msg.Value ?? ""; break;
                case "setMaxTokens": if (int.TryParse(msg.Value, out var t)) _vm.MaxTokens = t; break;
                case "selectRecent": _vm.SelectRecentCommand.Execute(msg.Value); break;
                case "toggleBool":
                    switch (msg.Key)
                    {
                        case "includeProvenance": _vm.IncludeProvenance = msg.BoolValue; break;
                        case "includeDiagnostics": _vm.IncludeDiagnostics = msg.BoolValue; break;
                        case "noRoslyn": _vm.NoRoslyn = msg.BoolValue; break;
                        case "dryRun": _vm.DryRun = msg.BoolValue; break;
                        case "includeAntiPatterns": _vm.IncludeAntiPatterns = msg.BoolValue; break;
                    }
                    break;
                case "toggleSection":
                    foreach (var g in _vm.SectionGroups)
                    foreach (var s in g.Children)
                        if (s.Name == msg.Value) s.IsIncluded = msg.BoolValue;
                    break;
                case "resetDefaults": _vm.ResetToScenarioDefaultsCommand.Execute(null); break;
                case "saveFile":
                    var ext2 = _vm.SelectedFormat == "json" ? "json" : "md";
                    Dispatcher.Invoke(async () =>
                    {
                        var dlg = new Microsoft.Win32.SaveFileDialog
                        {
                            Title = "Save output",
                            DefaultExt = ext2,
                            Filter = ext2 == "json" ? "JSON|*.json" : "Markdown|*.md",
                            FileName = $"devcontext-output.{ext2}"
                        };
                        if (dlg.ShowDialog() == true)
                            await File.WriteAllTextAsync(dlg.FileName, _vm.RawContent);
                    });
                    break;
                case "pickFolder":
                    Dispatcher.Invoke(() =>
                    {
                        var dlg = new Microsoft.Win32.OpenFileDialog
                        {
                            Title = "Select project folder",
                            FileName = "Folder Selection",
                            ValidateNames = false,
                            CheckFileExists = false,
                            CheckPathExists = true
                        };
                        if (dlg.ShowDialog() == true)
                            _vm.ProjectPath = Path.GetDirectoryName(dlg.FileName) ?? dlg.FileName;
                    });
                    break;
                case "pickFile":
                    Dispatcher.Invoke(() =>
                    {
                        var dlg = new Microsoft.Win32.OpenFileDialog
                        {
                            Title = "Select solution or project file",
                            Filter = "Solution/Project|*.sln;*.slnx;*.csproj"
                        };
                        if (dlg.ShowDialog() == true)
                            _vm.ProjectPath = dlg.FileName;
                    });
                    break;
            }
        }
        catch (Exception ex) { Log($"Message error: {ex.Message}"); }
    }

    private void Log(string msg)
    {
        try { File.AppendAllText(_logPath, $"{DateTime.Now:HH:mm:ss.fff} {msg}\n"); }
        catch { /* best effort */ }
    }
}

public record JsMessage(string Action, string? Value = null, string? Key = null, bool BoolValue = false);

public sealed class UiState
{
    public string projectPath { get; set; } = "";
    public string around { get; set; } = "";
    public int maxTokens { get; set; } = 8000;
    public int budgetTokens { get; set; } = 8000;
    public string selectedProfile { get; set; } = "focused";
    public string selectedFormat { get; set; } = "markdown";
    public string scenarioValue { get; set; } = "architecture";
    public bool includeProvenance { get; set; }
    public bool includeDiagnostics { get; set; }
    public bool noRoslyn { get; set; }
    public bool dryRun { get; set; }
    public bool includeAntiPatterns { get; set; }
    public bool isGitHubUrl { get; set; }
    public string gitRepoDisplay { get; set; } = "";
    public string cloneCleanup { get; set; } = "24h";
    public bool isAnalyzing { get; set; }
    public bool hasOutput { get; set; }
    public bool isProgressVisible { get; set; }
    public string progressText { get; set; } = "";
    public string outputText { get; set; } = "";
    public string humanViewText { get; set; } = "";
    public string llmViewText { get; set; } = "";
    public string displayText { get; set; } = "";
    public string statsText { get; set; } = "";
    public string analyzeButtonText { get; set; } = "Analyze";
    public int selectedTokenTotal { get; set; }
    public int totalTokens { get; set; }
    public bool isHumanView { get; set; } = true;
    public bool isSectionPanelVisible { get; set; }
    public List<ScenarioUi> scenarios { get; set; } = [];
    public List<string> recentPaths { get; set; } = [];
    public List<SectionGroupUi> sectionGroups { get; set; } = [];
    public List<string> formatValues { get; set; } = ["markdown", "json"];
    public List<ProfileUi> profiles { get; set; } = [];

    public UiState(MainViewModel vm)
    {
        projectPath = vm.ProjectPath;
        around = vm.Around;
        maxTokens = vm.MaxTokens;
        budgetTokens = vm.BudgetTokens;
        selectedProfile = vm.SelectedProfile;
        selectedFormat = vm.SelectedFormat;
        scenarioValue = vm.SelectedScenario?.Value ?? "architecture";
        includeProvenance = vm.IncludeProvenance;
        includeDiagnostics = vm.IncludeDiagnostics;
        noRoslyn = vm.NoRoslyn;
        dryRun = vm.DryRun;
        includeAntiPatterns = vm.IncludeAntiPatterns;
        isGitHubUrl = vm.IsGitHubUrl;
        gitRepoDisplay = vm.GitRepoDisplay;
        cloneCleanup = vm.CloneCleanup;
        isAnalyzing = vm.IsAnalyzing;
        hasOutput = vm.HasOutput;
        isProgressVisible = vm.IsProgressVisible;
        progressText = vm.ProgressText;
        humanViewText = vm.HumanViewText;
        llmViewText = vm.LlmViewText;
        displayText = vm.DisplayText;
        outputText = vm.OutputText;
        statsText = vm.StatsText;
        analyzeButtonText = vm.AnalyzeButtonText;
        selectedTokenTotal = vm.SelectedTokenTotal;
        totalTokens = vm.TotalTokens;
        isHumanView = vm.IsHumanView;
        isSectionPanelVisible = vm.IsSectionPanelVisible;
        scenarios = vm.Scenarios.Select(s => new ScenarioUi(s)).ToList();
        recentPaths = vm.RecentPaths.ToList();
        sectionGroups = vm.SectionGroups.Select(g => new SectionGroupUi(g)).ToList();
        profiles = [new ProfileUi("focused", "Focused"), new ProfileUi("debug", "Debug"), new ProfileUi("full", "Full")];
    }
}

public record ScenarioUi(string value, string label) { public ScenarioUi(ScenarioItem s) : this(s.Value, s.Label) { } }
public record ProfileUi(string value, string label);
public record SectionGroupUi(string name, bool isExpanded, bool isIncluded, int totalTokens, List<SectionUi> children)
{
    public SectionGroupUi(SectionGroupViewModel g) : this(g.Name, g.IsExpanded, g.IsIncluded, g.TotalTokens,
        g.Children.Select(c => new SectionUi(c)).ToList()) { }
}
public record SectionUi(string name, bool isIncluded, int compressedTokens, string category)
{
    public SectionUi(SectionViewModel s) : this(s.Name, s.IsIncluded, s.CompressedTokens, s.Category) { }
}
