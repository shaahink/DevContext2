# DevContext — UI Simplification Action Plan

**For**: opencode deepseek v4 pro agent  
**Repo**: `C:\code\DevContext2`  
**Goal**: Replace the confusing `scenario × profile` matrix with two modes + explicit section checkboxes. No engine changes.

---

## Context

DevContext is a .NET CLI + Blazor Hybrid WPF desktop tool that generates structured context about a .NET solution for LLMs. The desktop app (`src/DevContext.Desktop`) is a WPF window hosting a `BlazorWebView` — the entire UI is in Razor components, not XAML. The engine (`src/DevContext.Core`) runs a discovery pipeline (file tree → extraction → pruning → compression → rendering).

**The problem being solved**: The UI exposes a `scenario` dropdown (overview / deep-dive / audit) and a `profile` segmented control (focused / debug / full). Users don't understand:
- What "focused", "debug", "full" mean (mechanism names, not outcome names)
- That "audit" and "overview" both show an architecture section — they look like duplicates
- That "deep-dive" only differs from "overview" when a `--around` focus point is provided
- Which profile to pair with which scenario

**What we're building instead**:
- Two modes: **Overview** (whole-codebase) and **Trace** (entry-point focused)
- Section checkboxes (what sections appear in output)
- Profile derived automatically from sections selected: "Call graph" checked → Debug, "Source code" checked → Full, neither → Focused
- `--task` natural language field added to UI (was CLI-only)
- `audit` scenario removed from UI, deprecated in CLI (maps to `overview`)
- `deep-dive` scenario renamed to display as "Trace" (engine key stays `deep-dive` for CLI compat)

---

## What NOT to touch

- `src/DevContext.Core/Pipeline/DiscoveryPipeline.cs`
- `src/DevContext.Core/Extractors/**`
- `src/DevContext.Core/Pruning/**`
- `src/DevContext.Core/Rendering/**`
- `src/DevContext.Core/Models/ExtractionOptions.cs`
- `src/DevContext.Desktop/MainWindow.xaml` and `MainWindow.xaml.cs`
- `src/DevContext.Desktop/App.razor`
- `src/DevContext.Desktop/Components/OutputPanel.razor`
- `src/DevContext.Desktop/Components/StatusBar.razor`
- `src/DevContext.Desktop/ViewModels/SectionViewModel.cs`
- Any test files

---

## File Map (files you WILL change)

```
src/DevContext.Core/Configuration/ScenarioRegistry.cs   — rename deep-dive display name, remove audit
src/DevContext.Core/Configuration/IntentInferrer.cs     — remap audit→overview in inference rules
src/DevContext.Cli/Commands/AnalyzeCommand.cs           — add trace/audit aliases, CLI stays backward compat
src/DevContext.Desktop/Services/AnalysisService.cs      — new fields on AnalysisOptions, section filtering logic
src/DevContext.Desktop/ViewModels/MainViewModel.cs      — replace profile with sections, add task field
src/DevContext.Desktop/Components/ConfigPanel.razor     — new UI layout
```

---

## Step 1 — ScenarioRegistry.cs

**File**: `src/DevContext.Core/Configuration/ScenarioRegistry.cs`

Change `deep-dive`'s `DisplayName` from `"Deep Dive"` to `"Trace"`. Remove the `audit` entry from `BuiltIn` entirely — the CLI will redirect it before lookup.

The full file after changes:

```csharp
namespace DevContext.Core.Configuration;

/// <summary>Registry of built-in scenarios defining extraction, pruning, and compression configurations.</summary>
public static class ScenarioRegistry
{
    /// <summary>Gets the dictionary of built-in scenarios keyed by name.</summary>
    public static IReadOnlyDictionary<string, Scenario> BuiltIn { get; } =
        new Dictionary<string, Scenario>
        {
            ["overview"] = new()
            {
                Name = "overview",
                DisplayName = "Overview",
                Description = "High-level architecture map, endpoints, handlers, data model, and wiring",
                Pruning = new PruningConfig { MaxPathDistance = 2, MaxSurvivingTypes = 40 },
                Compression = new CompressionConfig { AggressiveTruncation = false },
                RequiredSections = [SectionNames.ArchitectureOverview, SectionNames.Endpoints, SectionNames.MediatRHandlers, SectionNames.DataModel, SectionNames.NonObviousWiring, SectionNames.RelatedTypes]
            },
            ["deep-dive"] = new()
            {
                Name = "deep-dive",
                DisplayName = "Trace",
                Description = "Entry-point focused: call graph, handler chain, event flow",
                Pruning = new PruningConfig { MaxPathDistance = 1, MaxCallDepth = 5, MaxSurvivingTypes = 25 },
                Compression = new CompressionConfig { AggressiveTruncation = true },
                RequiredSections = [SectionNames.Endpoints, SectionNames.CallGraph, SectionNames.MediatRHandlers, SectionNames.DataModel, SectionNames.MessageConsumers, SectionNames.NonObviousWiring]
            },
        }.ToFrozenDictionary();
}
```

---

## Step 2 — IntentInferrer.cs

**File**: `src/DevContext.Core/Configuration/IntentInferrer.cs`

Remove the `audit` scenario from inference rules and remap the DI/middleware rule to `overview`. The `audit` scenario no longer exists in the registry, so any rule that produces it must be updated.

Full file after changes:

```csharp
namespace DevContext.Core.Configuration;

/// <summary>Infers the most likely scenario and extraction profile from a user's task description using keyword matching.</summary>
public static class IntentInferrer
{
    private static readonly (string[] Keywords, string Scenario, ExtractionProfile Profile)[] Rules =
    [
        (["debug", "why", "failing", "error", "exception", "500", "trace", "call graph"], "deep-dive", ExtractionProfile.Debug),
        (["add", "implement", "similar", "like", "crud", "new endpoint", "architecture", "overview", "structure", "layers", "map"], "overview", ExtractionProfile.Focused),
        (["di", "injection", "reflect", "activator", "register", "middleware", "pipeline", "audit", "wiring"], "overview", ExtractionProfile.Debug),
        (["event", "message", "publish", "consume", "queue", "bus"], "deep-dive", ExtractionProfile.Focused),
    ];

    public static (string Scenario, ExtractionProfile Profile) Infer(string task)
    {
        var lower = task.ToLowerInvariant();
        var best = Rules
            .Select(r => (r.Scenario, r.Profile, Score: r.Keywords.Count(k => lower.Contains(k))))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        return best.Score > 0 ? (best.Scenario, best.Profile) : ("overview", ExtractionProfile.Focused);
    }
}
```

---

## Step 3 — AnalyzeCommand.cs (CLI backward compat)

**File**: `src/DevContext.Cli/Commands/AnalyzeCommand.cs`

In the `ResolveScenarioAndProfile` method, add alias remapping before the registry lookup. Find this block:

```csharp
scenarioName ??= settings.Scenario ?? config?.DefaultScenario ?? "overview";

if (!ScenarioRegistry.BuiltIn.TryGetValue(scenarioName, out _))
{
    AnsiConsole.MarkupLine($"[red]Unknown scenario: {scenarioName}[/]");
    AnsiConsole.MarkupLine($"Available: {string.Join(", ", ScenarioRegistry.BuiltIn.Keys)}");
    return null;
}
```

Replace with:

```csharp
scenarioName ??= settings.Scenario ?? config?.DefaultScenario ?? "overview";

// Backward-compat aliases
if (scenarioName == "trace") scenarioName = "deep-dive";
if (scenarioName == "audit")
{
    AnsiConsole.MarkupLine("[yellow]Warning: 'audit' scenario is deprecated. Use 'overview' instead.[/]");
    scenarioName = "overview";
}

if (!ScenarioRegistry.BuiltIn.TryGetValue(scenarioName, out _))
{
    AnsiConsole.MarkupLine($"[red]Unknown scenario: {scenarioName}[/]");
    AnsiConsole.MarkupLine($"Available: {string.Join(", ", ScenarioRegistry.BuiltIn.Keys)}");
    return null;
}
```

No other changes to AnalyzeCommand.cs.

---

## Step 4 — AnalysisService.cs

**File**: `src/DevContext.Desktop/Services/AnalysisService.cs`

### 4a. Update `AnalysisOptions` record

Replace the existing `AnalysisOptions` record at the bottom of the file with:

```csharp
public record AnalysisOptions
{
    public string ProjectPath { get; init; } = "";
    public string Scenario { get; init; } = "overview";
    public string Profile { get; init; } = "focused";       // derived by VM; kept for plumbing
    public string Around { get; init; } = "";
    public int MaxTokens { get; init; } = 8000;
    public string Format { get; init; } = "markdown";
    public bool IncludeProvenance { get; init; }
    public bool IncludeDiagnostics { get; init; }
    public bool NoRoslyn { get; init; }
    public bool DryRun { get; init; }
    public bool IncludeAntiPatterns { get; init; }
    public string Task { get; init; } = "";                 // natural language intent
    public ImmutableArray<string> ActiveSections { get; init; } = [];  // section names to include; empty = use scenario defaults
}
```

### 4b. Update `AppSettings`

Replace the existing `AppSettings` class with:

```csharp
public class AppSettings
{
    public string? LastScenario { get; set; } = "overview";
    public string? LastProfile { get; set; } = "focused";
    public string? LastFormat { get; set; } = "markdown";
    public int LastTokens { get; set; } = 8000;
    public string? LastAround { get; set; } = "";
    public bool IncludeProvenance { get; set; }
    public bool IncludeDiagnostics { get; set; }
    public bool NoRoslyn { get; set; }
    public string? LastTask { get; set; } = "";
    public List<string>? LastActiveSections { get; set; }
}
```

### 4c. Update `AnalyzeAsync`

Find the block that resolves scenario and profile (lines ~50–75 of `AnalyzeAsync`):

```csharp
if (!ScenarioRegistry.BuiltIn.TryGetValue(opts.Scenario, out var scenario))
    return new AnalysisResult { Success = false, Error = $"Unknown scenario: {opts.Scenario}" };

var profile = opts.Profile.ToLowerInvariant() switch
{
    "debug" => ExtractionProfile.Debug,
    "full" => ExtractionProfile.Full,
    _ => ExtractionProfile.Focused,
};
```

Replace with:

```csharp
// --task overrides scenario/profile via intent inference
var scenarioKey = opts.Scenario;
var profileStr = opts.Profile;
if (!string.IsNullOrWhiteSpace(opts.Task))
{
    var (inferredScenario, inferredProfile) = IntentInferrer.Infer(opts.Task);
    scenarioKey = inferredScenario;
    profileStr = inferredProfile.ToString().ToLowerInvariant();
}

// audit is a deprecated alias
if (scenarioKey == "audit") scenarioKey = "overview";
if (scenarioKey == "trace") scenarioKey = "deep-dive";

if (!ScenarioRegistry.BuiltIn.TryGetValue(scenarioKey, out var scenarioBase))
    return new AnalysisResult { Success = false, Error = $"Unknown scenario: {scenarioKey}" };

var profile = profileStr.ToLowerInvariant() switch
{
    "debug" => ExtractionProfile.Debug,
    "full" => ExtractionProfile.Full,
    _ => ExtractionProfile.Focused,
};

// Build effective scenario: filter RequiredSections to what user selected.
// If ActiveSections is empty, use the scenario defaults unchanged.
var scenario = opts.ActiveSections.Length > 0
    ? scenarioBase with
    {
        RequiredSections = scenarioBase.RequiredSections
            .Where(s => opts.ActiveSections.Contains(s))
            .Concat(opts.ActiveSections.Where(s => !scenarioBase.RequiredSections.Contains(s)))
            .ToImmutableArray()
    }
    : scenarioBase;
```

The rest of `AnalyzeAsync` is unchanged — it already uses `scenario` and `profile` variables.

---

## Step 5 — MainViewModel.cs

**File**: `src/DevContext.Desktop/ViewModels/MainViewModel.cs`

This is the most substantial change. Read the full file first, then apply all changes below.

### 5a. Add a `SectionToggle` class above `ScenarioItem`

Add this before the `ScenarioItem` record (at the top of the file, after `using` statements):

```csharp
public sealed class SectionToggle
{
    public string Key { get; init; } = "";      // matches SectionNames constant value
    public string Label { get; init; } = "";
    public string? Hint { get; init; }          // shown next to checkbox for expensive sections
    public bool IsEnabled { get; set; } = true;
}
```

### 5b. Remove profile-related members

Remove these from the `ObservableProperty` declarations and computed properties:

```csharp
// REMOVE this entire block:
[ObservableProperty]
[NotifyPropertyChangedFor(
    nameof(IsProfileFocused),
    nameof(IsProfileDebug), nameof(IsProfileFull))]
private string _selectedProfile = "focused";

// REMOVE:
public bool IsProfileFocused => SelectedProfile == "focused";
public bool IsProfileDebug => SelectedProfile == "debug";
public bool IsProfileFull => SelectedProfile == "full";
```

Also remove:

```csharp
// REMOVE:
partial void OnSelectedProfileChanged(string value) => OnAnalysisOptionChanged();
```

And remove the `SetProfileCommand`:

```csharp
// REMOVE:
[RelayCommand]
private void SetProfile(string profile) => SelectedProfile = profile;
```

### 5c. Add new fields after `_around`

Add these observable properties after `[ObservableProperty] private string _around = "";`:

```csharp
[ObservableProperty] private string _task = "";

partial void OnTaskChanged(string value) => OnAnalysisOptionChanged();
```

### 5d. Add `Sections` collection and helpers

Add after the `RecentPaths` collection property:

```csharp
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
    new() { Key = "__source__",                                                Label = "Source code", Hint = "adds full C# bodies, +2k–12k tokens" },
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
        // Trace: call graph on, source on by default, architecture off
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
        // Overview: broad picture, no call graph or source by default
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
```

### 5e. Update `Scenarios` list

Change the existing `Scenarios` list to:

```csharp
public List<ScenarioItem> Scenarios { get; } =
[
    new("overview",  "Overview"),
    new("deep-dive", "Trace"),
];
```

### 5f. Update `OnSelectedScenarioChanged`

The existing handler calls `ResetToScenarioDefaults()` and `OnAnalysisOptionChanged()`. Extend it to also apply section defaults:

```csharp
partial void OnSelectedScenarioChanged(ScenarioItem value)
{
    if (_isInitializing) return;
    ApplyScenarioSectionDefaults();
    OnPropertyChanged(nameof(IsTraceMode));
    ResetToScenarioDefaults();
    OnAnalysisOptionChanged();
}
```

### 5g. Update `AnalyzeAsync` options construction

Find the `var opts = new AnalysisOptions { ... }` block and update it to:

```csharp
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
```

### 5h. Update `LoadSettings`

Replace the existing `LoadSettings` body with:

```csharp
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
```

### 5i. Update `SaveSettings`

Replace the existing `SaveSettings` body with:

```csharp
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
```

---

## Step 6 — ConfigPanel.razor

**File**: `src/DevContext.Desktop/Components/ConfigPanel.razor`

Replace the entire file content with the following. The `@code { }` block at the top keeps the same component logic but removes the now-unused `_selectedScenario` tracking (mode selection is now handled directly through `VM.SelectedScenario`).

```razor
@inject MainViewModel VM

@implements IDisposable

@code {
    protected override void OnInitialized()
    {
        VM.PropertyChanged += OnVmChanged;
    }

    private void OnVmChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        => InvokeAsync(StateHasChanged);

    private void OnScenarioChanged(string value)
    {
        VM.SelectedScenario = VM.Scenarios.First(s => s.Value == value);
    }

    private void OnTokensInput(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var v))
            VM.MaxTokens = Math.Clamp(v, 500, 50000);
    }

    private void OnSectionChanged(string key, ChangeEventArgs e)
    {
        var enabled = (bool)(e.Value ?? false);
        VM.SetSectionEnabled(key, enabled);
    }

    public void Dispose() => VM.PropertyChanged -= OnVmChanged;
}

<aside class="config-panel">
    <header class="config-header">
        <h1 class="app-title">
            <svg class="icon" viewBox="0 0 24 24"><path d="M8 3a2 2 0 0 0-2 2v4a2 2 0 0 1-2 2H3v2h1a2 2 0 0 1 2 2v4a2 2 0 0 0 2 2h2v-2H8v-5a2 2 0 0 0-2-2 2 2 0 0 0 2-2V5h2V3m6 0a2 2 0 0 1 2 2v4a2 2 0 0 0 2 2h1v2h-1a2 2 0 0 0-2 2v4a2 2 0 0 1-2 2h-2v-2h2v-5a2 2 0 1 1 2-2 2 2 0 0 1-2-2V5h-2V3h2z"/></svg>
            DevContext <span class="version">v2.0.0</span>
        </h1>
    </header>

    <nav class="config-sections">

        {{!-- ── Source ─────────────────────────────────────────────── --}}
        <section class="config-group">
            <h2 class="group-label">Source</h2>
            <div class="source-row">
                <input class="input" placeholder="Path, .sln, .csproj, or github.com/user/repo"
                       @bind="VM.ProjectPath" @bind:event="oninput" />
            </div>

            @if (VM.IsGitHubUrl)
            {
                <div class="github-pill">
                    <span class="pill-text">@VM.GitRepoDisplay</span>
                    <select class="cleanup-select" @bind="VM.CloneCleanup">
                        <option value="24h">Cache 24h</option>
                        <option value="auto">Auto-clean</option>
                        <option value="session">Keep for session</option>
                        <option value="keep">Keep permanently</option>
                    </select>
                </div>
            }

            @if (VM.RecentPaths.Count > 0)
            {
                <div class="recent-list">
                    @foreach (var path in VM.RecentPaths)
                    {
                        <button class="chip" title="@path"
                                @onclick="() => VM.SelectRecentCommand.Execute(path)">
                            @(path.Contains('/') ? path.Split('/').Last() : System.IO.Path.GetFileName(path.TrimEnd('\\')))
                        </button>
                    }
                </div>
            }
        </section>

        {{!-- ── Intent (optional task field) ─────────────────────── --}}
        <section class="config-group">
            <h2 class="group-label">Intent <span class="hint">(optional)</span></h2>
            <input class="input" placeholder="e.g. trace the order submission handler"
                   @bind="VM.Task" @bind:event="oninput" />
            <p class="field-hint">Fills in mode and sections automatically when provided.</p>
        </section>

        {{!-- ── Mode ──────────────────────────────────────────────── --}}
        <section class="config-group">
            <h2 class="group-label">Mode</h2>
            <div class="segmented">
                @foreach (var s in VM.Scenarios)
                {
                    <button class="seg-option @(VM.SelectedScenario?.Value == s.Value ? "active" : "")"
                            @onclick="() => OnScenarioChanged(s.Value)">
                        @s.Label
                    </button>
                }
            </div>
            <p class="field-hint">
                @if (VM.IsTraceMode)
                {
                    <span>Focused on a specific entry point. Set a symbol below for best results.</span>
                }
                else
                {
                    <span>Whole-codebase architecture map.</span>
                }
            </p>
        </section>

        {{!-- ── Focus point (prominent in Trace, secondary in Overview) ── --}}
        @if (VM.IsTraceMode)
        {
            <section class="config-group">
                <h2 class="group-label">Entry point</h2>
                <input class="input" placeholder="Controller:Action or Namespace.Class:Method"
                       @bind="VM.Around" @bind:event="oninput" />
                <p class="field-hint">The type or method to trace from. Leave empty to scan all entry points.</p>
            </section>
        }

        {{!-- ── Sections ───────────────────────────────────────────── --}}
        <section class="config-group">
            <h2 class="group-label">Sections</h2>
            <div class="section-list">
                @foreach (var section in VM.Sections)
                {
                    <label class="toggle-label">
                        <input type="checkbox" checked="@section.IsEnabled"
                               @onchange="(e) => OnSectionChanged(section.Key, e)" />
                        @section.Label
                        @if (section.Hint is not null)
                        {
                            <span class="section-hint">@section.Hint</span>
                        }
                    </label>
                }
            </div>
        </section>

        {{!-- ── Token budget ─────────────────────────────────────────── --}}
        <section class="config-group">
            <h2 class="group-label">Token budget</h2>
            <div class="token-slider">
                <input type="range" min="500" max="50000" step="500"
                       @bind="VM.MaxTokens" @bind:event="oninput" />
                <input type="number" class="input token-input" min="500" max="50000"
                       value="@VM.MaxTokens" @onchange="OnTokensInput" />
            </div>
        </section>

        {{!-- ── Symbol focus (secondary position in Overview mode) ──── --}}
        @if (!VM.IsTraceMode)
        {
            <section class="config-group">
                <h2 class="group-label">Symbol focus <span class="hint">(optional)</span></h2>
                <input class="input" placeholder="Namespace.Class:Method"
                       @bind="VM.Around" @bind:event="oninput" />
            </section>
        }

        {{!-- ── Output ────────────────────────────────────────────────── --}}
        <section class="config-group">
            <h2 class="group-label">Output</h2>

            <label class="field-label">Format</label>
            <div class="segmented">
                @foreach (var f in new[] { ("markdown", "Markdown"), ("json", "JSON") })
                {
                    <button class="seg-option @(VM.SelectedFormat == f.Item1 ? "active" : "")"
                            @onclick="() => VM.SetFormatCommand.Execute(f.Item1)">
                        @f.Item2
                    </button>
                }
            </div>

            <details class="advanced">
                <summary class="advanced-summary">Advanced</summary>
                <div class="advanced-body">
                    <label class="toggle-label">
                        <input type="checkbox" checked="@VM.IncludeProvenance"
                               @onchange="@(e => VM.IncludeProvenance = (bool)(e.Value ?? false))" />
                        Include provenance
                    </label>
                    <label class="toggle-label">
                        <input type="checkbox" checked="@VM.IncludeDiagnostics"
                               @onchange="@(e => VM.IncludeDiagnostics = (bool)(e.Value ?? false))" />
                        Include diagnostics
                    </label>
                    <label class="toggle-label">
                        <input type="checkbox" checked="@VM.NoRoslyn"
                               @onchange="@(e => VM.NoRoslyn = (bool)(e.Value ?? false))" />
                        Skip Roslyn (faster, no call graph)
                    </label>
                    <label class="toggle-label">
                        <input type="checkbox" checked="@VM.DryRun"
                               @onchange="@(e => VM.DryRun = (bool)(e.Value ?? false))" />
                        Dry run (plan only)
                    </label>
                    <label class="toggle-label">
                        <input type="checkbox" checked="@VM.IncludeAntiPatterns"
                               @onchange="@(e => VM.IncludeAntiPatterns = (bool)(e.Value ?? false))" />
                        Include anti-pattern detection
                    </label>
                </div>
            </details>
        </section>

    </nav>

    <footer class="config-footer">
        <button class="btn-primary" @onclick="() => VM.AnalyzeCommand.Execute(null)"
                disabled="@(string.IsNullOrWhiteSpace(VM.ProjectPath))">
            @VM.AnalyzeButtonText
        </button>
    </footer>
</aside>
```

**Note on Blazor comment syntax**: Razor does not support `{{!-- --}}`. Replace all `{{!-- ... --}}` comment lines with `@* ... *@` Razor comment syntax. They are shown with `{{!-- --}}` here only to avoid parser issues in this document.

---

## Step 7 — Build verification

After all changes, run:

```
dotnet build src/DevContext.Desktop/DevContext.Desktop.csproj
dotnet build src/DevContext.Cli/DevContext.Cli.csproj
dotnet test tests/DevContext.Core.Tests/DevContext.Core.Tests.csproj
```

Expected: zero errors. The tests do not cover UI components so they should be unaffected.

If build fails on `DerivedProfile` not found in `AnalyzeAsync`: ensure the computed property `DerivedProfile` is defined on `MainViewModel` (Step 5d) and that `AnalyzeAsync` references `DerivedProfile` not a field.

If build fails on `SectionNames` namespace: the full namespace is `DevContext.Core.Constants.SectionNames` — use the full path in `MainViewModel.cs` since it may not be in `GlobalUsings`.

If build fails on `ImmutableArray` in `AnalysisService.cs`: add `using System.Collections.Immutable;` at the top of the file (it is likely already present).

---

## What this achieves

| Before | After |
|---|---|
| 3 scenarios × 3 profiles = 9 combinations | 2 modes + 9 section checkboxes |
| Profile names: focused / debug / full (opaque) | Profile derived silently from sections |
| "audit" and "overview" look like duplicates | audit gone; one overview mode |
| deep-dive only useful with `--around` but no hint | Trace mode surfaces entry point field prominently |
| `--task` (intent inference) CLI-only | Task field in UI |
| Sections fixed per scenario | User controls exactly what appears |

Engine (pipeline, extractors, pruners, renderer) is **completely unchanged**. All existing CLI flags continue to work. `--scenario audit` prints a deprecation warning and runs as overview. `--scenario trace` is accepted as an alias for `deep-dive`.
