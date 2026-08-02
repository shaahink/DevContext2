# Changelog

All notable changes to DevContext are documented here.

## [Unreleased]

- **Release workflow: a green release could ship without its installers.** v1.0.5 published with
  only the `.nupkg` attached while both Windows installers sat in the run's artifacts, and nothing
  failed — attaching zero files was not an error. `upload-artifact` roots an artifact at the least
  common ancestor of its path list, so the desktop job's two globs kept `nsis/` and `msi/` as
  directories while the release step matched a flat `artifacts/*.exe`. Globs are now recursive, and
  `fail_on_unmatched_files` makes a missing installer fail the release. (v1.0.5's installers were
  attached by hand, so that release is complete.)
- **NuGet push would have stayed silently skipped after the key was added** — the step's guard read
  `env.NUGET_API_KEY` from its own `env:` block, which GitHub does not expose to that step's own
  `if:`. Moved to job level, and a skip now emits a `::notice::` explaining itself.

## v1.0.5 (2026-08-02)

Everything since v1.0.0, by track (close-outs live in `docs/dev/HANDOVER-*.md`). v1.0.1–v1.0.4 were
packaging iterations of the v1.0.0 engine and carry no entries of their own; this is the first
release to describe the work that followed.

640 commits since v1.0.4, of which 202 are `feat` / `fix` / `perf`.

- **Lighthouse — repo intelligence (L0–L7)** — truth-gate test battery over real-world eval repos;
  ratcheted expectations that lock detection quality per repo.
- **Loom — the graph identity spine** — new `Graph2` layer: `SymbolTable` with a monotone
  resolution-tier ladder (never first-match on ambiguity), structured `BodyFacts` replacing all
  body-scan regexes ("the regex funeral", enforced by `scripts/loom-guards.ps1`), seam detectors,
  and a semantic-lite tier that upgrades edges to Roslyn-verified resolution.
- **Meridian — wiring truth & agent surface (M0–M9)** — the MCP server (`devcontext-mcp`,
  stdio → gRPC proxy), the Overview/one-page repo brief, cross-service `ServiceLink` wiring
  (bus / gRPC / HTTP), and honesty rules for DI resolution.
- **Desktop redo** — the WPF desktop (`DevContext.Desktop`) was retired and deleted (2026-07-15),
  replaced by an Angular 22 (zoneless, signals) + Tauri 2 app (`DevContext.App`) over a gRPC-Web
  server: Home, Explore workbench (Service/Layer/Feature/Flow lenses + table lens), Atlas,
  Insights, MCP management page, Context Studio, Settings.
- **Tapestry — T0–T8, complete 2026-07-17** — T0 gate hardening + eval fixtures; T1 universal entry
  coverage (entry-surface catalog: HTTP, gRPC, SignalR, GraphQL, Azure Functions, Orleans, workers,
  consumers; gateway archetype; per-service style); T2 graph quality (production-first DI edges,
  member line stamping, type-focus trace shaping, unified event wiring); T3 MCP v3 (ambiguity-honest
  addressing, budgeted envelopes); T4 context generation v2 (pack identity header, spine-first
  budget fill, real contracts/config/tests sections, per-section provenance, and `verify_context`
  for staleness checks against analyze-time file fingerprints); T5–T8 Context Studio, the Explore
  workbench, RPC budgeting, and render honesty for sample collections.
- **GitHub readiness** — CI re-enabled mirroring the local gate battery (engine + app jobs); release
  workflow fixed (CLI → NuGet; the dead WPF job removed); README restructured around the four
  surfaces; new `docs/product/mcp-reference.md`; reference docs re-verified against source.
  Desktop installer: `DevContext.Server` is published and bundled into the Tauri app as a sidecar
  (spawned via `dotnet`, .NET 10 runtime required) and `v*` tags now attach Windows installers to
  the GitHub Release alongside the CLI package. New weekly/manual `eval.yml` workflow runs the full
  gate battery (`eval/gates.ps1 -SkipMcpQa`) against the pinned eval-repo set.
- **Prism — determinism, caching and the release pipeline (D1–D5)** — analysis output made
  reproducible run-to-run (the decisive fix: a call edge is canonicalised by its **call site**, not
  by resolution order), a snapshot cache with an explicit release path for the engine host, and a
  Windows-only release pipeline whose installer version derives from the tag rather than a checked-in
  default. DntSite Map down to 34.5 s.
- **graph-v2 — graph truth, then the surfaces that read it** — the largest track. Batches A–E
  rebuilt identity and resolution (structural symbol ids, the call-graph binder routed through
  `SymbolTable`, one compilation per analysis, honest call edges), then a 47-pole truth matrix was
  used to hold every subsequent change to measured evidence. Notable outcomes:
  - **The MCP became a usable tool** — `map` had been dropping the library surface, packages,
    aggregates and the archetype view *after* computing them; seven of ten seam kinds rendered as
    the same mute dot; the session picker answered "whatever repo was touched last". Added `seam`
    (path-between), kind-filtered `neighbors`, and cache truth (`from_cache` / `analyzed_at` /
    `git_head`). The tool menu is now seeded from the SDK's own registered collection instead of a
    second hand-maintained list that had already drifted — **22 tools**, asserted by a test.
  - **Three fields that were read with the wrong key, or never assigned** — `Insight.Severity`
    shipped `"Warning"` while the app matched `"warning"` and MCP filtered `"WARNING"`, so the
    Insights page had never rendered its primary section on any repo; `AnalysisSummary.archetype`
    was assigned nowhere, so every analysis ever served carried `""`. A contract sweep
    (`eval/contract-sweep.ps1`) is now a gate, and four genuinely dead fields were removed.
  - **Performance: 1,275 s → 64.3 s on HotChocolate**, with output identical on 15 poles by
    SHA-256. The cause was not repo size — `SyntaxStructureExtractor` re-walked an entire file's
    syntax tree once per base-list entry, so one 11.3 MB generated client with 4,598 base lists cost
    1,216,998 ms on its own. Fixed with a per-file index, not by raising the timeout.
  - **Archetype and vocabulary honesty** — an auxiliary demo executable no longer decides a packable
    library's archetype; Atlas, the per-service breakdown and the Hub radar agree on what a service
    is; a library's empty sections now fill or state why they are withheld; raw generic arity
    (`` Logging.ILogger`1 ``) never reaches a rendered surface.
  - **Thresholds re-measured against the rebuilt graph** — five calibrated on the pre-Batch-A
    starved graph were re-run over 11 poles. Two were wrong and are corrected (a hero-flow depth
    filter that ran ahead of the rule it was under; a rate that fired at n=1, so two repos shipped a
    warning reading "1 of 1"). Three are left at their current values with the measurement stated at
    the site, because moving them would have been a product decision rather than a correction.
  - 24 measured-but-unfixed findings are recorded in `docs/dev/research/BUG-BACKLOG.md`.

## v1.0.0 (2026-06-11)

Initial public release.

- **Two-mode UI replaces 6-scenario × 3-profile matrix** — Modes: Overview (whole-codebase) and Trace (entry-point focused). Profile derived automatically from section checkboxes (Call graph → Debug, Source code → Full, neither → Focused).
- **Section checkboxes** — 9 explicit section toggles replacing opaque profile names. Users see exactly what goes into output.
- **Intent field** — `--task` natural language input in Desktop UI auto-selects mode and profile. (ConfigPanel.razor, MainViewModel)
- **`audit` scenario deprecated** — Maps to `overview` with a deprecation warning. Removed from `ScenarioRegistry`.
- **`trace` CLI alias** — Accepted as alias for `deep-dive` (engine key unchanged for backward compat).

### Extractor Improvements

- **Controller endpoints now fully detected** — `ControllerActionExtractor` handles bare `[Route(...)]` attributes without HTTP verb, infers verb from method name, expands `[controller]/[action]` tokens, supports multiple `[Route]` per action, and handles fully-qualified attribute names (`Microsoft.AspNetCore.Mvc.Route`). Real-world test: 13→70 endpoints on DntSite repo (5.4× improvement).
- **Architecture correctly classified** — New `ControllerBased` style. `ArchitectureStyleDetector` now demotes `MinimalApi` when controllers signal is stronger. Previously misclassified controller-heavy projects as MinimalApi.
- **Background workers detected** — `ProgramCsFlowExtractor` now scans scheduler config files (not just `Program.cs`) and detects `AddDNTScheduler` with individual job types. Real-world test: 0→24 background workers on DntSite.
- **DI auto-registration patterns** — `DiRegistrationExtractor` now detects `AutoInjectAllServices`, `Scan`, and similar bulk registration patterns. Also fixed case-insensitive `services.*` chain matching (previously missed lowercase `services`). Real-world test: 1→83 DI registrations.
- **EF Core entities from `OnModelCreating`** — `EfCoreExtractor` now walks `modelBuilder.Entity<T>()` calls in `OnModelCreating` to find entities even when no `DbSet<T>` properties exist. `ApplyConfigurationsFromAssembly` pattern also detected.
- **Migration classes grouped** — Migrations now shown under a single "Migrations" group instead of 30 separate tables.
- **Endpoint table filtering** — `AppendEndpoints` now respects `--around` focus points, showing only endpoints within proximity of the entry point.

### Desktop App

- **Crash logging** — Global WPF/AppDomain/Task exception handlers write to `crash.log` in `%LocalAppData%\DevContext\`. Serilog file sink with rolling daily logs + error-only crash log.
- **UI freeze eliminated** — `PopulateSections` split into pure computation (thread-pool) and collection mutation (UI thread). Batched property notifications — single `OnPropertyChanged(string.Empty)` instead of ~30 per-field events. Cached `LlmViewText` avoids recomputing 70K+ character strings on every render.
- **JS interop safety** — Copy/Save clipboard operations wrapped in try/catch.
- **File I/O off UI thread** — `SaveSettings` and `GitCloneService.Cleanup` run on thread pool.

### Tests

- **233 tests** (169 Core + 64 Desktop). Added 5 regression tests for batching, cache invalidation, section parsing, and collection notification counts.
- Golden tests regenerated to reflect new output format.

---

### Initial Technical Foundation

### Removed

- **`Quick` profile removed** — Was byte-for-byte identical to `Focused` (no extractor checked for it). Collapsed into `Focused`. Profiles are now: `focused`, `debug`, `full`. Validated by running all 6 scenarios at both profiles against fixture projects (0 diff). (ExtractionOptions, IntentInferrer, CLI, Desktop, Docs)

### Added

- **Desktop UI (Avalonia)** — Native cross-platform desktop app with resizable split layout, SegmentedControl for profile/format selection, ToggleSwitch advanced options, and StatusBar with progress. Catppuccin Latte light theme. F5 keyboard shortcut for Analyze. Auto-reanalyze when options change. (DevContext.Desktop)

- **Direct engine integration** — Desktop app runs DiscoveryPipeline in-process instead of spawning the CLI. Real CancellationToken support flows through all pipeline stages. Custom DesktopProgressObserver reports stage transitions (Discovering → Extracting → Pruning → Compressing → Rendering). No temp files, no process overhead. (AnalysisService)

- **Embedded Material Design icons** — 13 StreamGeometry icon paths embedded directly in App.axaml resources. Zero NuGet icon dependencies. Icons for scenarios (Sitemap, Magnify, PlusCircle, Cog, Message, ShieldCheck), toolbar actions (Copy, Save), browse buttons (FolderOpen, FileDocument), app branding (CodeBraces), and placeholder (TextBox).

- **SegmentedControl pattern** — Custom ListBox-based ControlTheme with UniformGrid items, blue selected state, and hover/pressed feedback. Replaces RadioButton pills. Two instances: 4-option Profile (Quick / Focused / Debug / Full) and 2-option Format (Markdown / JSON).

- **Event flow tracing** — New `InMemoryEventBusExtractor` detects `IEventBus.Subscribe<T>()`, `IEventBus.PublishAsync<T>()`, and `IEventHandler<T>` implementations. New `## Event flow` section in output shows publisher → event → handler → DB relationships for in-memory buses. (InMemoryEventBusExtractor)

- **Anti-pattern detection** — New `AntiPatternDetector` flags 5 patterns: fire-and-forget tasks (`_ = AsyncMethod()`), `IServiceScopeFactory` usage (service locator), `new` outside constructor/DI, `CancellationToken.None`, and unbounded `ConcurrentDictionary`/`ConcurrentQueue` without eviction. Output filtered to exclude test files and mock types. (AntiPatternDetector)

- **Deep call graph with field-type resolution** — CallGraphExtractor now resolves `_field.Method()` to the field's declared type, cross-references DI registrations to find concrete implementations, and chases same-class method calls (`this.DoSomething()`) to the containing type. BFS traversal depth reaches level 5+ on real projects. (CallGraphExtractor)

- **Project dependency tree** — Architecture overview now shows project references as an ASCII tree (`├── └── │`) instead of a flat list. (MarkdownRenderer)

- **Constructor dependency display** — Entry points now show `**Depends on**:` with parameter types and names, plus `**Resolved to**:` cross-referencing each dependency against DI registrations. (MarkdownRenderer, SyntaxStructureExtractor)

- **Source code bodies** — Full type source rendered in output for the entry point and up to 5 call chain types. Unlocked by `--profile full`. (MarkdownRenderer, SourceBodyExtractor)

- **LINQ chain resolution** — Chained calls like `db.Todos.Where(x => ...)` now walk to the root identifier and resolve through the field map. (CallGraphExtractor)

- **DI extension method parsing** — Generic DI extension methods like `AddHostedService<EngineWorker>()` now correctly capture the implementation type (`EngineWorker`) instead of `"?"`. (DiRegistrationExtractor)

- **CancellationToken.None variants** — `default(CancellationToken)` and `new CancellationToken()` are now detected alongside `CancellationToken.None`. (AntiPatternDetector)

### Fixed

- **Constructor extraction** — SyntaxStructureExtractor now captures `ConstructorDeclarationSyntax` (was filtered by `OfType<MethodDeclarationSyntax>`). Constructor parameters now appear in the call graph's field map. (SyntaxStructureExtractor)

- **fqnMap short name collisions** — When two types share a short name (e.g., `Models.Order` and `Dtos.Order`), the resolver now prefers the type matching the caller's namespace. Collisions are logged as diagnostics. (CallGraphExtractor)

- **ArrowExpressionClauseSyntax false positive** — `int Prop => new Foo()` in expression-bodied members is no longer incorrectly flagged as outside DI. (AntiPatternDetector)

- **Empty catch blocks** — DependencyExtractor and AntiPatternDetector empty catches now log diagnostics instead of silently swallowing errors.

- **Cancellation in BFS loops** — Call graph BFS traversal now respects `CancellationToken`. (CallGraphExtractor)

- **Duplicated code removed** — Triplicated `EnumerateSourceFilesAsync` consolidated into shared `ExtractorHelpers`. `ExtractGenericArguments` duplication resolved. (ExtractorHelpers)

- **Dead code removed** — `TypeDiscovery.AdditionalFilePaths` removed (populated but never read).

### Changed

- **Desktop default scenario** — Changed from `debug-endpoint` to `architecture` to match CLI default. Users switching between interfaces now get consistent behavior. (MainViewModel)
- **Removed empty test projects** — `DevContext.Roslyn.Tests` and `DevContext.Integration` (0 .cs files) removed from solution.
- **Deleted 0-byte placeholder** — `eval-results/DEVELOP-ASSESSMENT-ITERATION-PLAN.md`.

- **Anti-pattern severity normalized** — All fire-and-forget patterns now severity `"high"` (was `"medium"` for `ContinueWith`). `CancellationTokenNone` downgraded to `"low"` in test files.
- **ServiceLocator detection uses AST** — `DetectServiceScopeFactory` now matches on exact member name (`CreateScope`/`CreateAsyncScope`) instead of `string.Contains`.
- **AntiPatternDetection record moved** — From `AntiPatternDetector.cs` to `Models/Detections.cs` for consistency with all other detection types.
- **Test file filtering added** — `DetectServiceScopeFactory` and `DetectCancellationTokenNone` now skip or downgrade findings in test files.

### Architecture

- **Desktop test project** — New `tests/DevContext.Desktop.Tests/` with 33 tests: 31 ViewModel unit tests (NSubstitute), 1 ServiceRegistration DI test, 1 headless Avalonia XAML resource resolution test.
- **IAnalysisService interface** — Extracted for testability. ViewModel accepts `IAnalysisService` via constructor.
- **11 ADRs** covering all design decisions: Roslyn separation, pipeline stage ordering, signal sealing, typed detection hierarchy, parse-once cache, async-first IO, stable JSON schema, and more.
- **Shared** `ExtractorHelpers` for `EnumerateSourceFilesAsync` and `IsTestFile`.
- **4 projects**: `DevContext.Core`, `DevContext.Roslyn`, `DevContext.Cli`, `DevContext.Desktop`.

### Known Limitations

- In-memory event bus `PublishAsync` calls without explicit generic type arguments (<T>) are not captured (requires Roslyn semantic model).
- FastEndpoints call graph edges are not resolved (inheritance-based handlers).
- Partial class fields are invisible across partial files in the call graph.
