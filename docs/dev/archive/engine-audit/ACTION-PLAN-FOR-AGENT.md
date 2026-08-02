# Action Plan — Review DevContext Exports & Strengthen Engine

> For the agent picking up this work. Branch: `feat/desktop-v2` (commit `ff143d8`).
> All analysis artifacts live in `analysis-exports/`. Cloned repos in `analysis-repos/`.
> Companion document: `GAPS-AND-ISSUES.md` in this same directory.

---

## Step 0: Orientation

### Read these first
1. `GAPS-AND-ISSUES.md` — the full gap analysis with severity ratings
2. `docs/product/DESIGN-PHILOSOPHY.md` — the 7 principles (esp. P1, P2, P4)
3. `docs/product/PRODUCT-DIRECTION.md` — the entry-point ladder (§4) and kernel requirements (§6)
4. `docs/dev/HANDOVER.md` — phases 0–7 work summary, known deferred items
5. `docs/dev/HANDOVER-LIBRARY-SUPPORT.md` — library surface engine details
6. `docs/product/DETECTION-GUIDE.md` — the 5-rule value model and change workflow

### Build and verify
```powershell
dotnet build DevContext.slnx -c Release
dotnet test DevContext.slnx --filter "Category!=Eval&Category!=CliSmoke"
```

---

## Step 1: Audit the Exports (2-3 hours)

For each of the 4 repos, review both Map and Trace outputs. Use the checklist below.

### 1a) Serilog (library) — `analysis-exports/serilog/`

- [ ] **map.md** — Is the ENTRY API ranking correct? Are the most important types first? Check: `Log` (static facade) should be prominent, `LoggerConfiguration` (builder) should be in `build/configure` tier, `ILogger` should be in ABSTRACTIONS with many implementors.
- [ ] **map.md** — Are `*.Internal` namespaces demoted? Check `Serilog.Core.Sinks`, `Serilog.Core.Pipeline` — these are internal implementation. Should they be demoted?
- [ ] **map.md** — Does the PUBLIC SURFACE show `///` doc comments correctly? Spot-check: `ILogger` has a doc comment, `LogEvent` has one.
- [ ] **map.md** — CONSUMER PATHS: Are there real consumer paths? Serilog users `new LoggerConfiguration().WriteTo.Console().CreateLogger()` — is the `WriteTo` extension surfaced?
- [ ] **trace-log.md** — This is 4 lines. Read it. Understand why it's empty.
- [ ] **trace-loggerconfiguration.md** — Same. 4 lines. Note the empty body.

**Action:** Document 3-5 specific improvements for the library surface engine.

### 1b) Ocelot (API gateway) — `analysis-exports/ocelot/`

- [ ] **map.md** — Map shows only 3 entries from 65+ detections. Read the full map. What COULD be shown if the budget weren't cutting entries?
- [ ] **map.md** — STYLE says "ControllerBased" but also "MinimalApi=yes". Is Ocelot using both? Check `Ocelot.Samples.slnx` source.
- [ ] **map.md** — TOPOLOGY: 16 projects shown. Are they organized correctly? Ocelot (core) should be central.
- [ ] **trace-post-configuration.md** — This is the gold-standard trace. Walk it from top to bottom. Note every `[verified]` vs `[approx]` edge. Are the truncation markers honest? Check: lines 29, 34, 42 show truncation. Are the omitted branches truly beyond depth/fan-out limits?
- [ ] **trace-post-configuration.md** — Are there EMITS, TOUCHES, RESULT, NEXT sections? If not, why? (Ocelot is not event-driven, so missing EMITS/NEXT is expected.)
- [ ] **trace-delete-outputcache.md** — 7 lines. Does the RESULT section make sense? `204 No Content` for DELETE is correct.

**Action:** Identify why only 3 entries survived. Is it the token budget (C1), an entry dedup bug, or the entry-scoping in GraphBuilder?

### 1c) Files (desktop app) — `analysis-exports/files/`

- [ ] **map.md** — It's classified as LIBRARY. Walk through the PUBLIC SURFACE. Notice the WinUI controls (`Omnibar`, `SidebarView`, `Toolbar`, `StorageRing`). This is clearly a desktop app, not a library. What signals would a human use to detect this?
- [ ] **map.md** — Even as a library, what's missing? Look at `Files.App` namespace — the main app types (`App`, `MainWindow`, `BaseViewModel`) should be prominent but aren't.
- [ ] **trace-appservice.md** — This fell back to the full Library map. Why? Read `DiscoveryPipeline.RenderAsync` and trace the dispatch logic.
- [ ] **trace-mainwindow.md** — 4 lines. The call graph built 165 call edges from MainWindow, but the trace is empty. Why?

**Action:** Design what a "Desktop App" archetype entry points would look like. List the source patterns (e.g., `Window` subclass, `Application.OnLaunched`, `ICommand.Execute`) that an extractor should detect.

### 1d) aspnetcore (.NET framework) — `analysis-exports/aspnetcore/`

- [ ] **map.md** — 934 lines. Skim the TOPOLOGY (lines 8-404). Count how many projects are test/bench/sample vs production.
- [ ] **map.md** — ENTRY POINTS (lines 405-751). Pick 20 random entries. Count how many are from test projects vs production code. The production ones should be from `src/Identity/Core/src/`, `src/Mvc/Core/src/`, etc. — not from `test/` paths.
- [ ] **map.md** — STYLE says "NLayer". Is this correct? aspnetcore is a framework, not an application. The architecture style detector should recognize framework/library patterns.
- [ ] **trace-post-login.md** — 10 lines. The trace connects to `Test.CustomDbContext`. This is wrong — the production `POST /login` should connect to the Identity `UserManager<TUser>`, not a test class.
- [ ] **trace-webapplication.md** — 4 lines. `WebApplication` is the host builder. There should be a rich trace here. What's missing?

**Action:** Audit the NoiseFilter. Walk `IsProductionEntrySource` and verify it handles `testassets/`, `stress/`, `Testing/src/` path patterns.

---

## Step 2: Engine Remediation (in priority order)

### 2a — Fix Token Budget (C1) [highest priority, largest impact]

**Current behavior:** `TokenBudgetEnforcer` cuts types before rendering based on a per-type token estimate. The actual rendered output can be 2.6x over budget (aspnetcore) or 12x under (Ocelot).

**What to investigate:**
1. `Scoring/TokenBudgetEnforcer.cs` — how are tokens estimated per type?
2. `MapRenderer.RenderAsync()` — what's the actual token consumption per section?
3. `TraceRenderer.RenderSections()` — same.

**Target fix:** Instead of cutting types pre-render, keep all types in the model and track rendered sections. Each section (TOPOLOGY, ENTRY POINTS, PACKAGES, etc.) gets a token allocation from the budget. Render sections in priority order; stop when budget is exhausted. The "what was cut" list should be emitted as a disclosure (P2, P3).

**Verification:** Run Ocelot Map again — should show 30+ entries, not 3. Run aspnetcore Map — should fit within 8000 tokens.

### 2b — Fix Noise Filter for Test Entries (C2)

**Current behavior:** Test projects in aspnetcore (`testassets/`, `stress/`, `Testing/src/`) leak HTTP entry points into the Map.

**What to investigate:**
1. `Graph/NoiseFilter.IsProductionEntrySource` — check the exact path/project checks
2. `GraphBuilder.AddHttpEntryPoints` — where is `IsProductionEntrySource` called?
3. The Path convention: aspnetcore uses `test/`, `testassets/`, `stress/`, `Testing/`. The existing filter may only check for `test` or `test/` literally.

**Target fix:** Add `testassets`, `stress`, `Testing/src`, `TestServer` to the noise filter patterns. Also check project-level (not just path-level) filtering using `.csproj` properties.

**Verification:** Re-run aspnetcore Map — entry count should drop from 518 to a few dozen (production-only entries like `POST /login`, `POST /register` from Identity).

### 2c — Add Desktop-App Entry Points (H1)

**New extractor needed.** This is the biggest feature gap — desktop apps are invisible to DevContext.

**What to detect:**
1. **Window/page lifecycle:** `Window` subclasses, `Page` subclasses, `UserControl` subclasses
2. **App startup:** `Application.OnLaunched`, `App.OnStartup`, `Program.Main` with `Application.Run()`
3. **Command handlers:** `ICommand.Execute` implementations, `RelayCommand`, `AsyncRelayCommand` (MVVM Toolkit)
4. **ViewModel methods:** `*ViewModel` classes with public methods (heuristic: public methods on `*ViewModel` types)

**What to build:**
1. `Extractors/Specific/DesktopEntryExtractor.cs` — Stage 3, gated by signal "has winui/wpf/avalonia"
2. Signal in `DependencyExtractor` or `ProgramCsFlowExtractor` for detecting desktop frameworks
3. GraphBuilder integration: new entry kind `Desktop` in the entry-point inventory
4. Entry→target resolution for command handlers (command → ViewModel method)

**Verification:** Run against Files — should show MainWindow, App, and ViewModel entries.

### 2d — Fix Library Traces (C3)

**Current behavior:** `--focus` on a library type produces an empty trace, because the trace walker expects handler/entry-member edges.

**What to investigate:**
1. `TraceBuilder.OutEdgesWithTwin` — how does the controlled bridge work?
2. Are the Call edges attached to the Type node or the Member node? (Phase 1 fix says Member-origin, but verify)
3. What happens when the entry node is a Type (not an HTTP entry point) in a library?

**Target fix:** For library archetype with focus, the trace walker should follow all out-edges from the entry Type node (calls, sends, resolves) — not require handler/entry-member bridging. The existing `OutEdgesWithTwin` bridge should expand to include all member-out-edges for a library entry.

**Verification:** `--focus Log` on Serilog should show a trace like:
```
▸ ENTRY Log
   ├─ call Logger.Write
   │  └─ call Logger.Emit
   │     └─ call ILogEventSink.Emit
```

### 2e — Fix Map→Trace Dispatch for Libraries (H3)

**Current behavior:** `--focus IAppService` on Files renders the full Library map, not a trace.

**What to investigate:**
1. `Pipeline/DiscoveryPipeline.RenderAsync` — where does it decide Map vs Trace?
2. `AnalysisIntentResolver` — does it derive `deep-dive` scenario when `--focus` is set on a library?
3. The current code path: Library archetype → `LibrarySurfaceRenderer.RenderAsync()` unconditionally.

**Target fix:** When `--focus` is set, always use Trace renderer regardless of archetype. Library surface is only for Map (no-focus) mode.

**Verification:** `--focus IAppService` on Files should produce a trace tree, not the library map.

---

## Step 3: Deeper Graph Improvements

### 3a — DI Registration Detection for Extension Methods (M3)

**Current behavior:** Serilog's DI registrations (via `UseSerilog()` extension on `IHostBuilder`) are not detected.

**What to investigate:**
1. `DiRegistrationExtractor` — what patterns does it look for? (`services.Add*`, `.Add*` calls)
2. Serilog's registration pattern: extension methods on `LoggerConfiguration` (fluent builder) and on `IHostBuilder` (host integration). These are not the conventional `IServiceCollection.Add*` pattern.

**Target:** Broaden DI detection to recognize `IServiceCollection` parameter usage in any method, not just `Add*` convention methods. Or add a dedicated "fluent builder registration" pattern.

### 3b — Entry→Target for View-Only Controller Actions (M2)

**Current behavior:** Ocelot's `GET /configuration` has no target because the controller action returns a view (no service call).

**What to investigate:**
1. `GraphBuilder.ResolveEntryTarget` — the `ResolvePrimaryCall` fallback
2. When a controller action has no DI-tagged service callee, what happens?

**Target:** Fall back to the controller type itself as the target when no service callee is found. `GET /configuration → FileConfigurationController` is more useful than no target at all.

---

## Step 4: Verification Protocol

After each fix, run this verification sequence:

```powershell
# 1. Rebuild
dotnet build DevContext.slnx -c Release

# 2. Fast tests (must stay green)
dotnet test DevContext.slnx --filter "Category!=Eval&Category!=CliSmoke"

# 3. Re-run the affected analysis
dotnet run --no-build --project src/DevContext.Cli -- analyze <repo-path> --stats -o <output-file>

# 4. Compare output quality
#    - Entry count improved? (C2, H1)
#    - Token budget honored? (C1)
#    - Trace non-empty? (C3, H3)
#    - Architecture correct? (H1)

# 5. Full gate
powershell -File eval/gates.ps1
```

---

## Step 5: Filing Issues

For each finding that needs team discussion or is too large for immediate fix, create an issue with:
- **Title:** Short, specific (e.g., "TokenBudgetEnforcer cuts 99% of types, leaving Map sparse")
- **Labels:** The severity tag (critical/high/medium/low) and the component tag
- **Body:** The evidence from this analysis (copy from GAPS-AND-ISSUES.md), the locus (file:line), and a suggested fix approach

---

## Appendix: Key Engine Files to Read

| File | What it controls |
|------|-----------------|
| `Graph/GraphBuilder.cs` | Entry point assembly, edge creation, entry→target resolution |
| `Graph/TraceBuilder.cs` | Trace walker, controlled bridge, truncation, TOUCHES/EMITS |
| `Graph/CodeGraph.cs` | Immutable graph, nodes/edges, inverse adjacency |
| `Graph/GraphQuery.cs` | Query facade (entrypoints, trace, map, stats, node) |
| `Graph/ArchetypeDetector.cs` | App vs Library classification |
| `Graph/NoiseFilter.cs` | Test/generated/sample exclusion |
| `Graph/MapBuilder.cs` | MapModel assembly (topology, packages, style, surface) |
| `Rendering/MapRenderer.cs` | Map markdown rendering |
| `Rendering/TraceRenderer.cs` | Trace markdown rendering |
| `Rendering/LibrarySurfaceRenderer.cs` | Library surface markdown |
| `Pipeline/DiscoveryPipeline.cs` | Analysis pipeline, render dispatch |
| `Configuration/AnalysisIntentResolver.cs` | Scenario/profile derivation from --focus |
| `Scoring/TokenBudgetEnforcer.cs` | Pre-render token budget cut |
| `Scoring/PatternRelevancePruner.cs` | Relevance scoring (currently no-op) |
| `Extractors/Specific/CallGraphExtractor.cs` | Roslyn call-graph builder |
| `Extractors/Specific/EndpointExtractor.cs` | Minimal API endpoint detection |
| `Extractors/Specific/ControllerActionExtractor.cs` | MVC controller action detection |
| `Extractors/Generic/DiRegistrationExtractor.cs` | DI registration detection |
