# DevContext v1.0 — Comprehensive Technical Report

## 1. Tool Overview

DevContext is a static analysis tool for .NET codebases. It scans a project, extracts structured context (endpoints, background workers, EF Core entities, DI registrations, middleware pipelines, call graphs, anti-patterns), prunes to fit a token budget, and produces a document ready to paste into any LLM.

| Surface | Platform | Distribution | Runtime |
|---------|----------|-------------|---------|
| **CLI** | Linux, macOS, Windows | `dotnet tool install -g DevContext.Cli` | .NET 10 SDK required |
| **Desktop** | Windows 10+ (build 19041+) | `DevContext.Desktop.zip` from GitHub Releases | Self-contained, no SDK needed |

---

## 2. Features

### CLI (`devcontext analyze`)

```
devcontext analyze [PATH] [OPTIONS]
```

| Flag | Description |
|------|-------------|
| `--scenario overview\|deep-dive\|trace` | Analysis mode (default: overview) |
| `--around TypeName[:MethodName]` | Entry point for proximity pruning + call graph tracing |
| `--task "free text"` | Natural language intent auto-selects mode + profile |
| `--max-tokens N` | Token budget (default 8000) |
| `--format markdown\|json\|html` | Output format |
| `-o file.md` | Write to file |
| `--include-diagnostics` | Show pruning notes and warnings |
| `--metrics` | Show per-extractor timing |
| `--dry-run` | Plan-only mode |
| `--no-roslyn` | Skip Roslyn workspace loading |

### Desktop

| Feature | Description |
|---------|-------------|
| **ConfigPanel** | Source input (local path or GitHub URL), Intent field, Mode toggle (Overview/Trace), Entry point, 12 section checkboxes, Token budget slider, Output format, Advanced toggles |
| **OutputPanel** | Human View tab (rendered HTML with section nav), LLM View tab (raw markdown for copy-paste), Section drawer (toggle sections + token budget bar), Copy/Save/Copy LLM buttons with toast |
| **GitHub analysis** | Clone & analyze in one click — `github.com/user/repo` auto-validates, clones, analyzes |
| **Crash logging** | Global exception handlers + Serilog file sinks → `crash.log` + `devcontext.log` |
| **WebView2 bootstrapper** | Auto-installs Edge WebView2 runtime on first launch |

---

## 3. Data Flow — End to End

### 3.1 CLI Flow

```
User runs: devcontext analyze ./MyApp.sln --scenario overview --max-tokens 8000
  │
  ├─ 1. Program.cs → Spectre.Console.CLI
  │     └─ Parses args, creates AnalyzeCommand with DI services
  │
  ├─ 2. AnalyzeCommand.ExecuteAsync()
  │     ├─ ProjectRootResolver.ResolveAsync(path) → finds .sln/.csproj or walks up from folder
  │     ├─ [if GitHub URL] GitCloneService.CloneAsync() → uses SemaphoreSlim(1,1), LibGit2Sharp
  │     ├─ IntentInferrer.Infer(task) → keyword matching against 4 rule sets
  │     ├─ ScenarioRegistry.BuiltIn[scenario] → Scenario with RequiredSections, Pruning, Compression
  │     ├─ Builds ExtractionOptions + SharedAnalysisContext + DiscoveryContext
  │     └─ DiscoveryPipeline.RunAllFormatsAsync(ctx, ct) → returns RenderedContext per format
  │
  └─ 3. WriteOutput → stdout or file
```

### 3.2 Desktop Flow

```
User clicks "Analyze" in desktop UI
  │
  ├─ 1. ConfigPanel.razor → VM.AnalyzeCommand.Execute(null)
  │
  ├─ 2. MainViewModel.AnalyzeAsync()
  │     ├─ CancelPrevious() — disposes previous CTS
  │     ├─ [if GitHub URL] GitCloneService.CloneAsync() → with progress callback
  │     ├─ Builds AnalysisOptions from user inputs
  │     │   └─ ActiveSections = GetActiveSections() — from SectionToggle checkboxes
  │     │   └─ Profile = DerivedProfile — computed from section state
  │     └─ _svc.AnalyzeAsync(opts, progress, ct) → AnalysisService
  │
  ├─ 3. AnalysisService.AnalyzeAsync()
  │     ├─ IntentInferrer.Infer(task) [if task set]
  │     ├─ Scenario remapping (audit→overview, trace→deep-dive)
  │     ├─ Filters Scenario.RequiredSections by ActiveSections
  │     ├─ Builds DI container per-analysis (ServiceCollection → DiscoveryPipeline)
  │     ├─ DiscoveryPipeline.RunAllFormatsAsync(ctx, ct)
  │     │   └─ Returns: { "markdown": RenderedContext, "json": RenderedContext, "html": RenderedContext }
  │     └─ Returns AnalysisResult { Content (markdown), HtmlContent, ElapsedMs }
  │
  ├─ 4. MainViewModel post-processing
  │     ├─ await Task.Run(BuildSectionData(rawContent)) — parses markdown into SectionGroupViewModels
  │     ├─ SectionGroups = newGroups.ToImmutableArray() — single assignment, no CollectionChanged
  │     ├─ _humanViewHtml = result.HtmlContent
  │     └─ OnPropertyChanged(string.Empty) — single batched UI notification
  │
  └─ 5. OutputPanel.razor renders
        ├─ Human tab: (MarkupString)VM.HumanViewHtml → dc-report HTML
        └─ LLM tab: <pre><code>VM.LlmViewText</code></pre> → raw markdown
```

---

## 4. Data Model

### 4.1 Core Models

```
DiscoveryModel
├─ Types: ConcurrentDictionary<string, TypeDiscovery>    ← populated by extractors
├─ Detections: ConcurrentBag<Detection>                    ← populated by extractors
├─ Architecture: FeatureSignals                            ← registered by extractors
├─ Projects: ImmutableArray<ProjectInfo>                   ← from ProjectStructureExtractor
├─ Solution: SolutionInfo                                  ← from SolutionExtractor
├─ DetectedStyle: ArchitectureStyle                        ← from ArchitectureStyleDetector
├─ Budget: TokenBudget                                     ← from budget enforcement
└─ Diagnostics: ConcurrentBag<DiagnosticEntry>             ← from extractors + pipeline
```

### 4.2 Detection Types (12 derived types from Detection base)

| Type | Fields | Produced By |
|------|--------|-------------|
| `EndpointDetection` | HttpMethod, RouteTemplate, HandlerType, HandlerMethod, AuthAttributes | EndpointExtractor, ControllerActionExtractor |
| `MediatRHandlerDetection` | RequestType, ResponseType, HandlerType, Kind | MediatRExtractor |
| `EfEntityDetection` | EntityType, DbContextType, IsAggregate, KeyProperties | EfCoreExtractor |
| `DiRegistrationDetection` | ServiceType, ImplementationType, Lifetime, Shape | DiRegistrationExtractor |
| `MiddlewareDetection` | MiddlewareType, PipelineOrder, Kind | ProgramCsFlowExtractor |
| `BackgroundWorkerDetection` | ServiceType, ImplementationType, Kind | ProgramCsFlowExtractor |
| `IndirectWiringDetection` | Kind, CallerType, CallerMethod, TargetType | IndirectWiringDetector |
| `AntiPatternDetection` | Pattern, Description, Severity, TargetType | AntiPatternDetector |
| `MessageConsumerDetection` | BusKind, MessageType, ConsumerType | EventBusExtractor |
| `EventFlowDetection` | EventType, Target, Kind, BusKind | InMemoryEventBusExtractor |
| `AspireResourceDetection` | ResourceType, ResourceName | AspireExtractor |
| `AspireRelationshipDetection` | SourceResource, TargetResource, RelationshipType | AspireExtractor |

### 4.3 Pipeline Configuration

```
ExtractionOptions           ← CLI/Desktop input
├─ Profile: Focused|Debug|Full
├─ MaxOutputTokens: int
├─ AllowRoslyn: bool
├─ OutputFormat: Markdown|Json|Html
├─ ExcludeExtractors: ImmutableArray<string>
└─ ExcludePatterns: ImmutableArray<string>

Scenario (from ScenarioRegistry)
├─ Name: "overview"|"deep-dive"|"trace"
├─ DisplayName: "Overview"|"Trace"
├─ Pruning: PruningConfig { MaxPathDistance, MaxCallDepth, MaxSurvivingTypes }
├─ Compression: CompressionConfig { AggressiveTruncation, PerTypeCharCap }
└─ RequiredSections: ImmutableArray<string>

RenderOptions               ← Built from Scenario + model state
├─ IncludeProvenance, IncludeDiagnostics
├─ EstimatedTokens, ScenarioDisplayName, ProfileDisplayName
├─ RequiredSections, FocusPoints, CallGraph, ProjectGraph
```

### 4.4 Desktop Models

```
MainViewModel
├─ [ObservableProperty] ProjectPath, Around, Task, MaxTokens
├─ [ObservableProperty] SelectedScenario, SelectedFormat
├─ [ObservableProperty] IsAnalyzing, ProgressText, HasOutput
├─ Sections: List<SectionToggle> (12 checkboxes)
├─ SectionGroups: ImmutableArray<SectionGroupViewModel>
├─ HumanViewHtml: string (HTML render from pipeline)
└─ LlmViewText: string (cached LLM view text)

SectionGroupViewModel
├─ Name: string, IsExpanded: bool
└─ Children: List<SectionViewModel>

SectionViewModel
├─ Name, Category, FullText, IsIncluded
├─ RawTokens, CompressedTokens, Weight
└─ PropertyChanged → triggers RecalcTokenTotal, RebuildLlmViewText
```

---

## 5. Pipeline Stages — Synchronous, Async, and Parallel

### Stage 1: Discovery & Cache Warmup — **Sequential async**

```
foreach extractor (ordered by [ExtractorOrder])
  await extractor.ExtractAsync(ctx, model, ct)
  
Extractors: FileTreeExtractor, SolutionDiscovery, ProjectStructureExtractor (3 total)
Concurrency: SINGLE-THREADED
```

### Stage 2: Generic Extraction — **PARALLEL (the only parallel point)**

```csharp
// DiscoveryPipeline.cs:332
var eligible = _extractors.Where(e => e.Stage == Stage2Parallel).ToList();
await Parallel.ForEachAsync(eligible, ct, async (extractor, innerCt) =>
{
    await RunSingleExtractorAsync(extractor, context, model, innerCt);
});
```

```
Extractors: DependencyExtractor, SyntaxStructureExtractor, LayerClassifier,
            DiRegistrationExtractor, ProgramCsFlowExtractor (5 total)
Concurrency: PARALLEL (Parallel.ForEachAsync)
Thread safety via:
  • ConcurrentDictionary<string, TypeDiscovery> (model.Types)
  • ConcurrentBag<Detection> (model.Detections)
  • ConcurrentDictionary<Lazy<Task<SyntaxTree>>> (AnalysisCache)
  • Lazy<Task<FileSyntaxNodes>> (SharedAnalysisContext.SyntaxNodeCache)
```

### Signal Sealing — **Synchronous**

```
model.Architecture.Seal()          ← freezes all FeatureSignals
ArchitectureStyleDetector.Detect()  ← scores 7 architecture styles
```

### Stage 3: Specific Extraction — **Sequential async**

```
foreach extractor (ordered by [ExtractorOrder])
  if (ShouldRun(signals))
    await extractor.ExtractAsync(ctx, model, ct)
    
Extractors: EndpointExtractor, ControllerActionExtractor, MediatRExtractor,
            EfCoreExtractor, CallGraphExtractor, EventBusExtractor,
            InMemoryEventBusExtractor, IndirectWiringDetector, AspireExtractor,
            AntiPatternDetector, SourceBodyExtractor (11 total)
Concurrency: SINGLE-THREADED
```

### Stage 4: Pruning — **Sequential synchronous**

```csharp
foreach (var pruner in _pruners.OrderBy(p => p.Order))
{
    await pruner.PruneAsync(model, context, ct);
    // All pruners return ValueTask.CompletedTask (synchronous)
}
```

```
Pruners: PathProximityPruner → CallReachabilityPruner → PatternRelevancePruner → TokenBudgetEnforcer
All are pure CPU-bound — no I/O, no parallelism
Mutations: Sets TypeDiscovery.IsPruned, RelevanceScore, PathProximityScore
MaxSurvivingTypes enforced from Scenario.Pruning
```

### Stage 5: Compression — **Sequential synchronous**

```csharp
foreach (var compressor in _compressors.OrderBy(c => c.Order))
{
    await compressor.CompressAsync(model, options, ct);
}
```

```
Compressors: TrivialMemberCompressor → BoilerplateCompressor → StructuralDeduplicator →
             NamespaceGrouper → LlmFriendlyFormatter → AggressiveTruncator
All are pure CPU-bound — no I/O, no parallelism
Effect: Reduce token count while preserving semantic meaning
```

### Stage 6: Rendering — **Sequential async per format**

```
foreach (format in _renderers)
  await renderer.RenderAsync(model, renderOptions, ct)
  
Renderers: MarkdownRenderer, JsonContextRenderer, HtmlContextRenderer (3 total)
Called from a single RunAllFormatsAsync — all formats from the same model
```

---

## 6. Threading Model — End-to-End Summary

```
┌──────────┬───────────────┬──────────────┬─────────────────────────────┐
│ STAGE    │ CONCURRENCY   │ ASYNC/SYNC   │ THREAD SAFETY              │
├──────────┼───────────────┼──────────────┼─────────────────────────────┤
│ 1 Discovery│ Sequential    │ Async        │ Single-threaded by design  │
│ 2 Generic  │ PARALLEL      │ Async        │ ConcurrentDictionary/      │
│            │ (ForEachAsync)│              │ ConcurrentBag + Lazy<Task> │
│ Signal Seal│ Sequential    │ Synchronous  │ Read-only after seal       │
│ 3 Specific │ Sequential    │ Async        │ Single-threaded            │
│ 4 Pruning  │ Sequential    │ Synchronous  │ Single-threaded            │
│ 5 Compress │ Sequential    │ Synchronous  │ Single-threaded            │
│ 6 Render   │ Sequential    │ Async        │ Read-only model            │
└──────────┴───────────────┴──────────────┴─────────────────────────────┘

Locking primitives used in entire codebase:
  - lock statements: ZERO
  - SemaphoreSlim: 1 (GitCloneService._cloneLock — async mutual exclusion)
  - Interlocked: ZERO
  - ReaderWriterLockSlim: ZERO
  - ManualResetEvent: ZERO

Concurrent collections:
  - ConcurrentDictionary: 4 (Types, SyntaxNodeCache, TextCache, XmlCache)
  - ConcurrentBag: 3 (Detections, CallEdges, Diagnostics)
  - ImmutableArray: Multiple (Projects, FocusPoints, RequiredSections)

Cancellation:
  - CancellationToken propagated through all 6 stages
  - ct.ThrowIfCancellationRequested() in every sequential loop
  - 3 independent CancellationTokenSources in MainViewModel
```

---

## 7. Formatting, Budgeting, and Pruning

### 7.1 Token Budget

```
TokenBudget model:
  MaxTokens: int (from --max-tokens, default 8000)
  CurrentTokens: int (estimated as content.Length / 4)
  SafetyMargin: 500

Budget is enforced by TokenBudgetEnforcer (Stage 4):
  1. Sort all types by relevance score (descending)
  2. Accumulate token costs until budget is reached
  3. Mark remaining types as IsPruned = true
  4. Scenario.MaxSurvivingTypes provides an additional cap (overview=40, trace=25)
```

### 7.2 Pruning Strategies (4 stages, in order)

| # | Pruner | Logic |
|---|--------|-------|
| 1 | **PathProximityPruner** | Computes directory-distance from focus points. Types within MaxPathDistance survive (overview=2, trace=1). Without focus points, all types get 0.5 score. |
| 2 | **CallReachabilityPruner** | BFS traversal from focus method through call graph. Boosts relevance for reachable types, prunes unreachable ones. Only active for method-level focus points. |
| 3 | **PatternRelevancePruner** | Boosts relevance for types that appear in detections (endpoints, handlers, entities). Reduces test/mock/bootstrap noise. |
| 4 | **TokenBudgetEnforcer** | Final gate: accumulates by relevance score until MaxTokens. Also enforces Scenario.MaxSurvivingTypes. |

### 7.3 Compression Strategies (6 stages, in order)

| # | Compressor | Effect |
|---|-----------|--------|
| 1 | **TrivialMemberCompressor** | Removes parameterless constructors, ToString(), GetHashCode(), Equals() |
| 2 | **BoilerplateCompressor** | Removes designer-generated code, assembly attributes |
| 3 | **StructuralDeduplicator** | Groups structurally similar types (same methods/properties) |
| 4 | **NamespaceGrouper** | Groups types by namespace for compact output |
| 5 | **LlmFriendlyFormatter** | Formats source code for LLM consumption (removes redundant whitespace) |
| 6 | **AggressiveTruncator** | Per-type character cap from Scenario.Compression.PerTypeCharCap (default 3000) |

### 7.4 Output Format Options

| Format | Renderer | Where Used | Characteristics |
|--------|----------|-----------|----------------|
| **markdown** | `MarkdownRenderer` | CLI default, Desktop LLM View | Tables, code blocks, ASCII trees. ~1200 lines of rendering logic. |
| **json** | `JsonContextRenderer` | CLI `--format json` | Flat serialization of `DevContextOutput` schema. `SchemaVersion: "1.0"`. |
| **html** | `HtmlContextRenderer` | Desktop Human View tab | Semantic HTML — color-coded DI cards, collapsible call graph, sticky section nav, relative paths with tooltips. |

### 7.5 Section Gating

Sections are gated by `Scenario.RequiredSections` (from `ScenarioRegistry`) filtered by `ActiveSections` (from Desktop checkboxes). The `ShouldRender(sectionName, options)` method returns `true` if `RequiredSections` is empty (render all) or contains the section name.

**Desktop section checkboxes → profile auto-derivation**:
- Call graph checked → `Debug` profile
- Source code checked → `Full` profile
- Neither → `Focused` profile (default)

---

## 8. MainViewModel — Deep Dive

### 8.1 Architecture

`MainViewModel` is a CommunityToolkit.Mvvm `ObservableObject` with 14 distinct responsibilities across ~660 lines (down from 696 after recent refactoring). It serves as the single data-binding hub for the desktop UI.

```
MainViewModel
├── Input State (15+ [ObservableProperty] fields)
│   ├── ProjectPath, Around, Task, MaxTokens
│   ├── SelectedScenario, SelectedFormat
│   └── IncludeProvenance, IncludeDiagnostics, NoRoslyn, DryRun, IncludeAntiPatterns
├── Progress State
│   └── IsAnalyzing, ProgressText, ProgressValue, IsProgressIndeterminate
├── Output State
│   ├── HasOutput, StatsText, DisplayText, IsHumanView
│   ├── SectionGroups: ImmutableArray<SectionGroupViewModel>
│   ├── HumanViewHtml: string, LlmViewText: string (cached)
│   └── BudgetTokens, TotalTokens, SelectedTokenTotal, BudgetUtilisation
├── GitHub State
│   ├── _gitRepoUrl, GitRepoDisplay, IsGitHubUrl, CloneCleanup
│   └── ValidateGitHubUrlAsync() — async validation with CancellationTokenSource
├── Commands
│   ├── AnalyzeCommand (RelayCommand, CanExecute: !string.IsNullOrEmpty(ProjectPath))
│   ├── SetFormatCommand, SelectRecentCommand, ResetToScenarioDefaultsCommand
│   └── Auto-reanalysis: 10 On*Changed partial methods → OnAnalysisOptionChanged()
└── Lifecycle
    └── Dispose(), CancelPrevious(), LoadSettings(), SaveSettings(), RefreshRecent()
```

### 8.2 Key Methods

| Method | Lines | Responsibility |
|--------|-------|---------------|
| `AnalyzeAsync()` | 155 | Full analysis lifecycle: clone, build opts, call service, parse result, batch UI update |
| `BuildSectionData()` | 57 | Parses markdown output ("## Section") into SectionGroupViewModels with categorization |
| `ValidateGitHubUrlAsync()` | 42 | Async URL validation with CTS management |
| `ApplyScenarioSectionDefaults()` | 32 | Sets 24 section toggles based on Overview/Trace mode |
| `DebouncedReanalyze()` | 18 | 500ms debounce for max-tokens slider changes |

### 8.3 Known Limitations

| Issue | Status | Details |
|-------|--------|---------|
| **UI lag before display** | Partially fixed | Most work moved to thread pool, but ~14 renders still queue from ImmutableArray swap + OnPropertyChanged. SectionGroups now ImmutableArray (down from ObservableCollection) — eliminates 13 CollectionChanged renders but 1 PropertyChanged + WebView2 IPC remains. |
| **No granular progress bar** | Not fixed | ProgressText shows stage transitions ("Discovering files...", "Extracting structure...", etc.) but no percentage or file count. The progress bar is indeterminate for most stages. |
| **AnalyzeAsync is 155 lines** | Not refactored | Contains clone logic, analysis options construction, service call, result parsing, property updates, and cleanup in one method. Should be split into `CloneIfNeededAsync()`, `ExecuteAnalysisAsync()`, `ApplyResultAsync()`. |
| **OnAnalysisOptionChanged re-analyzes on any change** | By design | Changing format, section checkboxes, or any toggle restarts the entire analysis pipeline. This is intentional for "live preview" but costly. |
| **DebouncedReanalyze uses fire-and-forget** | Minor | Discards the task — if dispatcher invocation fails, error is silently swallowed (though now logged). |
| **Markdown parsing fragility** | Medium | `BuildSectionData` splits on `"## "` and assumes first line is section name. If renderer format changes, parsing breaks silently. |

### 8.4 Lag History

| Iteration | What was tried | Result |
|-----------|---------------|--------|
| **v1 (original)** | All work on UI thread: markdown rendering, section parsing, property updates | Multi-second freeze |
| **v2** | `PopulateSections` moved to `Task.Run` | Still slow — ObservableCollection mutations on thread pool queued 70+ renders |
| **v3** | `BuildSectionData` split: computation on thread pool, collection mutations on UI thread in batch | 13 CollectionChanged renders from ObservableCollection.Clear/Add |
| **v4 (current)** | `ObservableCollection` → `ImmutableArray`, single `OnPropertyChanged(string.Empty)`, Markdig removed | 1 PropertyChanged render + WebView2 IPC. Still ~500ms lag from HTML content serialization. |
| **v5 (future)** | Pipeline renders HTML directly (no markdown intermediate), HtmlContent set once, no BuildSectionData parsing needed | Expect sub-100ms display |

---

## 9. Test Coverage & Confidence

### 9.1 Test Suite Overview

| Project | Tests | Framework | Confidence |
|---------|-------|-----------|-----------|
| `DevContext.Core.Tests` | 169 | xUnit + NSubstitute | **High** — covers extractors, rendering, pipeline, models |
| `DevContext.Desktop.Tests` | 63 | xUnit + NSubstitute | **Medium-High** — covers ViewModel behavior, section parsing, property batching |

### 9.2 Test Categories

#### Core Tests (169 tests)

| Category | Files | Tests | What They Cover |
|----------|-------|-------|-----------------|
| **Extractor tests** | 13 files | ~60 tests | `EndpointExtractor`, `ControllerActionExtractor` (8 tests: verb inference, token expansion, FQN, multi-route, convention routing, NonAction), `DependencyExtractor`, `SyntaxStructureExtractor`, `EfCoreExtractor`, `ProgramCsFlowExtractor`, `DiRegistrationExtractor`, `ArchitectureStyleDetector` (4 tests: ControllerBased, MinimalApi, NLayer, CleanArchitecture), `AntiPatternDetector`, `MediatRExtractor`, `CallGraphExtractor`, `SourceBodyExtractor` |
| **Golden tests** | 2 files | 10 tests | Exact output regression — compares rendered markdown/JSON against `tests/goldens/*.md` and `tests/goldens/*.json`. Use `UPDATE_GOLDENS=1` to regenerate. |
| **Pipeline tests** | 1 file | 3 tests | End-to-end pipeline with fake filesystem and mock providers |
| **Renderer tests** | 1 file | ~11 tests | `MarkdownRenderer` section structure, empty model handling, schema version; `JsonContextRenderer` schema validation |
| **Model/utility tests** | ~5 files | ~30 tests | `FocusPointParser`, `FakeFileSystem`, `DiscoveryModel`, `ArchitectureSignals`, `RepoUrl` |
| **Compressor/Pruner tests** | 2 files | ~10 tests | Trivial members, boilerplate removal, deduplication, path proximity |
| **Service tests** | ~3 files | ~20 tests | `AnalysisCache`, `ServiceRegistration`, `GoldenTestHelper` |

#### Desktop Tests (63 tests)

| Category | Tests | What They Cover |
|----------|-------|-----------------|
| **Initialization** | 8 | Constructor default values, settings loading, recent paths, scenario/section initialization |
| **Section model** | 6 | `PopulateSections_parses_markdown_into_groups`, categorization, `LlmViewText` cache + invalidation |
| **Property batching** | 2 | Single `OnPropertyChanged(string.Empty)` after analysis, no per-field events |
| **Analysis lifecycle** | 10 | Success/error paths, exception handling, cancellation, progress state |
| **Commands** | 5 | `SetFormatCommand`, `SelectRecentCommand`, `AnalyzeCommand` CanExecute |
| **Auto-reanalysis** | 6 | Option changes trigger re-analysis, debounce on max-tokens |
| **Computed properties** | 6 | `IsTraceMode`, `DerivedProfile`, format flags, button text |
| **Settings** | 5 | `LoadSettings` restores active sections, `SaveSettings` includes sections and task |

### 9.3 Test Gaps

| Gap | Risk | Impact |
|-----|------|--------|
| **7 extractors have zero dedicated tests** | High | `EventBusExtractor`, `InMemoryEventBusExtractor`, `IndirectWiringDetector`, `AspireExtractor` — only covered by golden tests (no assertions on detection content) |
| **No GitHub clone UI tests** | High | `MainViewModel` GitHub flow (clone, validate, cleanup) has zero ViewModel test coverage |
| **Golden tests are fragile** | Medium | Any renderer format change requires `UPDATE_GOLDENS=1` regeneration of 7 files |
| **No Desktop integration tests** | Medium | `AnalysisService.AnalyzeAsync` is only tested indirectly through `MainViewModelTests` with `NSubstitute` |
| **CliSmokeTests require build** | Low | 3 slow tests that run the CLI as a subprocess — excluded from CI filter |

### 9.4 Confidence Summary

| Aspect | Confidence | Why |
|--------|-----------|-----|
| **Pipeline correctness** | High | 169 tests covering all stages, extractors, rendering |
| **Controller detection** | High | 8 dedicated tests for `ControllerActionExtractor` — verb inference, FQN, multi-route |
| **Architecture classification** | Medium-High | 4 tests for `ArchitectureStyleDetector` — `ControllerBased`, `MinimalApi`, `NLayer`, `CleanArchitecture` |
| **Desktop UI behavior** | Medium | 63 tests for ViewModel — but no tests for ConfigPanel.razor or OutputPanel.razor rendering |
| **Event/message bus** | Low | `EventBusExtractor` and `InMemoryEventBusExtractor` have zero dedicated tests |
| **Aspire integration** | Low | `AspireExtractor` has zero dedicated tests |
| **GitHub clone flow** | Low | Only 3 integration tests in `GitHubAnalysisE2ETests` — all depend on real network |
| **Output format parity** | Medium | Golden tests cover markdown output. HTML and JSON formats not covered by golden tests. |

---

## 10. Code Map — Key Namespaces and Files

```
src/DevContext.Core/
├── Analysis/
│   ├── AnalysisCache.cs          — Thread-safe lazy cache (SyntaxTree, Text, XML)
│   └── SharedAnalysisContext.cs  — FocusPoints, CallGraph, ProjectGraph, SyntaxNodeCache
├── Compression/ (6 compressors, Stage 5)
├── Configuration/
│   ├── ScenarioRegistry.cs       — Built-in scenario definitions (overview, deep-dive, trace)
│   ├── IntentInferrer.cs         — Keyword-matching for --task → scenario/profile
│   └── DevContextConfig.cs       — devcontext.json reader
├── Constants/
│   └── SectionNames.cs           — 17 section name constants
├── Contracts/
│   ├── IContextRenderer.cs       — RenderAsync + RenderOptions + RenderedContext
│   ├── IDiscoveryExtractor.cs    — ShouldRun + ExtractAsync
│   ├── IDiscoveryObserver.cs     — Pipeline lifecycle callbacks
│   └── IAnalysisCache.cs         — GetTextAsync, GetSyntaxTreeAsync, GetXmlAsync
├── Extractors/
│   ├── Generic/ (5 extractors, Stage 1+2)
│   │   ├── FileTreeExtractor.cs
│   │   ├── SolutionDiscoveryExtractor.cs
│   │   ├── ProjectStructureExtractor.cs
│   │   ├── DependencyExtractor.cs
│   │   ├── SyntaxStructureExtractor.cs
│   │   ├── LayerClassifier.cs
│   │   ├── DiRegistrationExtractor.cs
│   │   ├── ProgramCsFlowExtractor.cs
│   │   └── ArchitectureStyleDetector.cs
│   └── Specific/ (11 extractors, Stage 3)
│       ├── EndpointExtractor.cs
│       ├── ControllerActionExtractor.cs
│       ├── MediatRExtractor.cs
│       ├── EfCoreExtractor.cs
│       ├── CallGraphExtractor.cs
│       ├── EventBusExtractor.cs
│       ├── InMemoryEventBusExtractor.cs
│       ├── IndirectWiringDetector.cs
│       ├── AspireExtractor.cs
│       ├── AntiPatternDetector.cs
│       └── SourceBodyExtractor.cs
├── Models/
│   ├── DiscoveryModel.cs         — Central mutable model
│   ├── DiscoveryContext.cs       — Aggregate context for pipeline run
│   ├── Scenario.cs               — PruningConfig, CompressionConfig
│   ├── ExtractionOptions.cs      — Pipeline configuration
│   ├── Detections.cs             — 12 detection types
│   ├── Detection.cs              — Base class (SourceFile, LineNumber, Confidence)
│   └── DevContextOutput.cs       — JSON output schema
├── Pipeline/
│   └── DiscoveryPipeline.cs      — 6-stage orchestrator (394 lines)
├── Pruning/ (4 pruners, Stage 4)
├── Rendering/
│   ├── MarkdownRenderer.cs       — CLI + Desktop LLM View (~1065 lines)
│   ├── HtmlContextRenderer.cs    — Desktop Human View (~480 lines)
│   └── JsonContextRenderer.cs    — CLI --format json (69 lines)
├── Resolvers/
│   ├── ProjectRootResolver.cs    — Finds .sln/.csproj root
│   └── FocusPointParser.cs       — Parses --around strings
├── Services/
│   └── GitCloneService.cs        — GitHub clone (LibGit2Sharp + git CLI fallback)
└── Utilities/
    └── StringHelpers.cs          — LevenshteinDistance, NormalizeRoute

src/DevContext.Cli/
├── Commands/
│   ├── AnalyzeCommand.cs         — CLI entry point
│   ├── VersionCommand.cs         — version display
│   └── ScenariosCommand.cs       — lists scenarios
├── Services/
│   └── ServiceRegistration.cs    — DI registration (AddDevContextServices)
├── Settings/
│   └── AnalyzeSettings.cs        — Spectre.Console.CLI argument definitions
├── Observers/
│   └── SpectreDiscoveryObserver.cs — Console progress rendering
├── Program.cs                    — Top-level CLI entry point
└── DevContextVersion.cs          — MinVer-backed version display

src/DevContext.Desktop/
├── Components/
│   ├── ConfigPanel.razor         — Left sidebar (source, intent, mode, sections, output)
│   └── OutputPanel.razor         — Right side (tabs, drawer, toolbar, toast)
├── Services/
│   ├── AnalysisService.cs        — Desktop analysis orchestration (~310 lines)
│   └── LoggingConfig.cs          — Serilog file sink setup
├── ViewModels/
│   ├── MainViewModel.cs          — God class (~660 lines, 14 responsibilities)
│   └── SectionViewModel.cs       — SectionGroupViewModel + SectionViewModel + TokenWeight
├── Program.cs                    — WPF entry point with global exception handlers
├── MainWindow.xaml.cs            — WebView2 bootstrapper + DI setup
└── MainWindow.xaml               — WPF XAML layout
```

---

## 11. Versioning and Release

```
MinVer tag prefix: v

git tag -a v1.0.4 -m "Release notes"
git push origin v1.0.4

Release workflow (release.yml):
  cli job (Linux):
    → Restore + Build src/DevContext.Cli
    → Test Core
    → dotnet pack → DevContext.Cli.{version}.nupkg
    → Push to NuGet.org (if NUGET_API_KEY set)
  
  desktop job (Windows):
    → dotnet publish src/DevContext.Desktop -r win-x64 --self-contained
    → Zip → DevContext.Desktop.zip
  
  release job:
    → Create GitHub Release with .nupkg + .zip
    → Generate release notes from commits
```

---

*Generated 2026-06-11 | Schema v1.0 | Tests: 232 passing (169 Core + 63 Desktop)*
