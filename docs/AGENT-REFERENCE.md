# DevContext — Tool Reference for Agents

## What It Is

DevContext is a static analysis tool for .NET codebases. It scans a project, extracts structured context (endpoints, workers, entities, DI wiring, middleware), prunes to fit a token budget, and produces a document ready to paste into any LLM.

**Two surfaces**: CLI (`dotnet tool install -g DevContext.Cli`) and Windows Desktop (self-contained `.exe`).

---

## How Data Flows

```
User input (path/task) 
  → ProjectRootResolver (finds .sln, walks up, or uses folder)
  → DiscoveryPipeline (6 stages, ~20 extractors)
    Stage 1: DISCOVERY — FileTreeExtractor, SolutionDiscovery, ProjectStructureExtractor
      → Populates: AllSourceFiles, AllProjectFiles, model.Solution, model.Projects
    Stage 2: GENERIC (parallel) — DependencyExtractor, SyntaxStructureExtractor, 
             LayerClassifier, DiRegistrationExtractor, ProgramCsFlowExtractor
      → Populates: model.Types, model.Detections, Architecture signals
    [Signal Sealing] — ArchitectureStyleDetector runs, no more signal writes
    Stage 3: SPECIFIC (sequential, signal-gated) — EndpointExtractor,
             ControllerActionExtractor, EfCoreExtractor, CallGraphExtractor, et al.
      → Populates: EndpointDetection, EfEntityDetection, et al.
    Stage 4: PRUNING — PathProximityPruner → CallReachabilityPruner → 
             PatternRelevancePruner → TokenBudgetEnforcer
    Stage 5: COMPRESSION — 6 compressors (trivial members, boilerplate, dedup, etc.)
    Stage 6: RENDERING — MarkdownRenderer (CLI/LLM view) or HtmlContextRenderer (Desktop Human view)
  → Output: ~6,000 tokens of structured context
```

### Key contracts

| Interface | Location | Purpose |
|-----------|----------|---------|
| `IDiscoveryExtractor` | `src/DevContext.Core/Contracts/` | All 19 extractors implement this. `ShouldRun()` gates by signals. `ExtractAsync()` populates model. |
| `IDiscoveryObserver` | Same | Pipeline lifecycle callbacks. Desktop uses `DesktopProgressObserver`, CLI uses `SpectreDiscoveryObserver`. |
| `IContextRenderer` | Same | `MarkdownRenderer`, `JsonContextRenderer`, `HtmlContextRenderer`. `RenderAsync(model, options, ct)` → `RenderedContext`. |
| `IAnalysisCache` | `src/DevContext.Core/Analysis/` | Parse-once cache. Syntax trees, XML docs, file text keyed by path. |

### Models

| Model | Location | Key Fields |
|-------|----------|------------|
| `DiscoveryModel` | `src/DevContext.Core/Models/` | `Types: ConcurrentDictionary<string, TypeDiscovery>`, `Detections: ConcurrentBag<Detection>`, `Architecture: FeatureSignals`, `Projects: ImmutableArray<ProjectInfo>` |
| `Detection` (base) | Same | `SourceFile`, `LineNumber`, `Confidence`, `ExtractorName`. 12 derived types. |
| `ExtractionOptions` | Same | `Profile`, `MaxOutputTokens`, `AllowRoslyn`, `ExcludeExtractors`, `ExcludePatterns` |
| `RenderOptions` | `src/DevContext.Core/Contracts/` | `IncludeDiagnostics`, `RequiredSections`, `FocusPoints`, `CallGraph`, `ProjectGraph` |

---

## CLI & Desktop

### CLI (`devcontext analyze`)

```
devcontext analyze [PATH] [OPTIONS]

PATH: .sln, .csproj, directory, or github.com/user/repo
--scenario overview|deep-dive|trace  (default: overview)
--around TypeName[:MethodName]      (entry point for proximity pruning + call graph)
--task "free text intent"           (auto-selects scenario + profile)
--max-tokens 8000                   (token budget)
--format markdown|json|html
--include-diagnostics
--metrics
--dry-run
--no-roslyn
```

### Desktop (`DevContext.Desktop.exe`)

WPF + Blazor Hybrid app. ConfigPanel (left sidebar) for input, OutputPanel (right) for results.

**ConfigPanel**:
- Source field (local path or GitHub URL)
- Intent field (natural language)
- Mode toggle (Overview / Trace)
- Entry point field (Trace mode)
- 12 section checkboxes → profile auto-derived (Call graph→Debug, Source code→Full)
- Token budget slider
- Output format + Advanced toggles

**OutputPanel**:
- Human View tab: rendered HTML via `HtmlContextRenderer` (section nav, color-coded DI cards, collapsible call graph)
- LLM View tab: raw markdown for copy-paste
- Sections drawer: toggle individual sections, see token contributions
- Copy / Save / Copy LLM buttons with toast notifications

**Settings persisted** in `%LocalAppData%\DevContext\settings.json` + `recent.json`.

---

## Extractors (20 total)

### Stage 1 — Discovery (sequential)

| # | Extractor | Finds |
|---|-----------|-------|
| 1 | `SolutionDiscoveryExtractor` | `.sln`/`.slnx` files, project paths |
| 2 | `FileTreeExtractor` | All `.cs`, `.csproj` files (respects exclude patterns) |
| 3 | `ProjectStructureExtractor` | Target frameworks, project refs, NuGet packages |

### Stage 2 — Generic (parallel)

| # | Extractor | Finds |
|---|-----------|-------|
| 4 | `DependencyExtractor` | Package→signal mapping (34 mappings), project dependency graph |
| 5 | `SyntaxStructureExtractor` | All type declarations, methods, properties, base types, attributes |
| 6 | `LayerClassifier` | Classifies each project into ArchitectureLayer by path/name/package heuristics |
| 7 | `DiRegistrationExtractor` | `AddSingleton/Scoped/Transient`, `Add*` extensions, `AutoInjectAllServices` bulk patterns |
| 8 | `ProgramCsFlowExtractor` | Middleware pipeline (`Use*`/`Map*`), background workers (`AddHostedService`, `AddDNTScheduler`), orphan patterns |

**Between Stage 2 and 3**: `ArchitectureStyleDetector` runs (determines MinimalApi, ControllerBased, NLayer, CleanArchitecture, etc.). Signals sealed — no more writes to Architecture.

### Stage 3 — Specific (sequential, signal-gated)

| # | Extractor | Gated by | Finds |
|---|-----------|----------|-------|
| 9 | `EndpointExtractor` | `minimal-apis` / `fast-endpoints` | Minimal API `MapGet/Post/etc.`, FastEndpoints endpoints |
| 10 | `ControllerActionExtractor` | `controllers` | MVC controller actions, bare `[Route]` with verb inference, token expansion |
| 11 | `MediatRExtractor` | `mediatr` | `IRequestHandler<T>`, `INotificationHandler<T>` |
| 12 | `EfCoreExtractor` | `efcore` | `DbContext`, `DbSet<T>`, `modelBuilder.Entity<T>()`, migrations |
| 13 | `CallGraphExtractor` | (Debug/Full profile) | BFS call graph, DI map, field-type resolution |
| 14 | `EventBusExtractor` | `masstransit` / `nservicebus` | Message bus consumers |
| 15 | `InMemoryEventBusExtractor` | (always) | `IEventHandler<T>`, `Subscribe<T>`, `Publish<T>` |
| 16 | `IndirectWiringDetector` | `deep-dive` scenario | Activator.CreateInstance, service locator, Castle Proxy |
| 17 | `SourceBodyExtractor` | (Full profile) | Source text for non-pruned types |
| 18 | `AntiPatternDetector` | (opt-in) | Fire-and-forget, service locator, CancellationToken.None, async void |
| 19 | `AspireExtractor` | `aspire` | AppHost resources, Redis/Postgres/etc., relationships |

### Dedicated detectors

| Detector | When | What |
|----------|------|------|
| `ArchitectureStyleDetector` | Between Stage 2 and 3 | Scores 7 styles (MinimalApi, NLayer, CleanArchitecture, ControllerBased, etc.) from sealed signals |
| `FocusPointResolver` | After Stage 2 | Resolves `--around Type:Method` to specific types in `model.Types` |

---

## Rendering

### MarkdownRenderer (`Format: "markdown"`)

Produces the default output. Used by CLI and Desktop LLM View. ~1,069 lines, god class with ~35 methods. Renders 15+ sections with tables, code blocks, ASCII trees.

### HtmlContextRenderer (`Format: "html"`)

New in v1.0 — semantic HTML for Desktop Human View. ~500 lines. Produces:
- Sticky section navigation bar
- Responsive endpoint tables with relative paths and tooltips
- Collapsible call graph tree (`<details><summary>`)
- Color-coded DI registration cards (Singleton=blue, Scoped=green, Bulk=orange)
- Numbered middleware pipeline
- Background workers as chip list
- Anti-patterns grouped by file (collapsible)
- All using existing CSS variables from `app.css`

### JsonContextRenderer (`Format: "json"`)

Flat JSON output. `DevContextOutput` record with `SchemaVersion = "1.0"`. Detections serialized as `IReadOnlyList<object>`. Used programmatically — less feature-complete than Markdown.

---

## Testing

| Project | Tests | Framework | Key Patterns |
|---------|-------|-----------|-------------|
| `tests/DevContext.Core.Tests` | 169 | xUnit + NSubstitute | `FakeFileSystem` for AST input, `DiscoveryContextBuilder` for pipeline setup, `GoldenTestHelper` for output regression |
| `tests/DevContext.Desktop.Tests` | 64 | xUnit + NSubstitute | `IAnalysisService` mocked, ViewModel property-batching verified, section parsing tested |

**Golden tests**: 7 `.md` / `.json` fixtures in `tests/goldens/`. Use `UPDATE_GOLDENS=1` to regenerate. Check exact output format.

**Key extractor tests**: `ControllerActionExtractorTests` (8 tests: verb inference, token expansion, FQN, multi-route), `ArchitectureStyleDetectorTests` (4 tests: ControllerBased, MinimalApi, NLayer, CleanArchitecture).

---

## Report Generation Flow

```
MainViewModel.AnalyzeAsync
  → creates AnalysisOptions { Scenario, ActiveSections, Around, Task, ... }
  → AnalysisService.AnalyzeAsync
      → IntentInferrer.Infer(Task) → overrides scenario/profile (if --task used)
      → Builds ExtractionOptions + SharedAnalysisContext
      → DiscoveryPipeline.RunAsync
          → All 6 stages (discovery → rendering)
          → Returns markdown (CLI/LLM) + HTML (Human View)
      → Returns AnalysisResult { Content, HtmlContent, ElapsedMs }
  → MainViewModel processes result:
      → Task.Run(BuildSectionData) — parses output into SectionGroups
      → Batched UI update (single OnPropertyChanged)
      → HtmlContent shown in Human View tab
```

---

## Branch & Release Strategy

```
main   ← always deployable, tagged releases
develop ← integration, feature branches merge here
```

**Versioning**: MinVer with `v` tag prefix. `git tag -a v1.0.5` triggers release.yml:
- Linux: build CLI, run Core tests, `dotnet pack` → `.nupkg` → NuGet.org
- Windows: `dotnet publish` Desktop → `.zip` artifact → GitHub Release

**Release command**:
```bash
git tag -a v1.0.5 -m "Release notes"
git push origin v1.0.5
```
