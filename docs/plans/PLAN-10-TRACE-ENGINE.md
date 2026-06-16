# Plan 10 — Trace Engine (Map + Trace over a CodeGraph)

> Rebuild the output around the two artifacts in `IDEAL-OUTPUT-TARGET.md` — a **Map** (orientation,
> no code) and a **Trace** (an entry point followed down the wiring, indirection bridged) — by adding
> a connected-graph layer (`CodeGraph`) and retiring the global relevance-scoring system. Design and
> rationale: `TRACE-ENGINE-DESIGN.md` (read §0–§8 first). The hard model is already built and
> compiling in `src/DevContext.Core/Graph/`; this plan wires it in, fills the join seams, and migrates
> rendering + scoring.
>
> Branch off `develop`: `feature/trace-engine`.

## Preconditions & how to build/test (read first)

- **There is no `.sln`.** Build/test per project:
  - Core: `dotnet test tests/DevContext.Core.Tests/DevContext.Core.Tests.csproj`
  - Desktop (`net10.0-windows`, Windows only): `dotnet test tests/DevContext.Desktop.Tests/...`
- **Analyzer warnings are errors** (CA1822 etc.). Build clean or CI fails. After every task: build +
  run the affected test project; do not advance on red.
- **The skeleton exists and compiles** (`Graph/CodeGraph.cs`, `EntryPoint.cs`, `ISymbolResolver.cs`,
  `NoiseFilter.cs`, `TraceBuilder.cs`, `MapBuilder.cs`, `GraphBuilder.cs`). It is **additive and not
  yet wired** into DI or the pipeline. Evolve these files; don't recreate them.
- **Migrate additively, switch once proven.** P1/P2 build Map/Trace *alongside* the existing
  renderer. The old pruners/scorer/sections are deleted only in **Part E**, after the new output is
  validated — never a big-bang swap.
- Eval repos at `eval-repos/` (eShop = primary CQRS/DDD target; VerticalSlice, TodoApi, AutoMapper).
  The target trace is the hand-built `POST /api/orders` in `IDEAL-OUTPUT-TARGET.md` §2.
- Run `eval/gates.ps1` at session end (per `AGENT-REFERENCE.md`).
- **Decisions resolving the open questions are baked into `TRACE-ENGINE-DESIGN.md` §9.** Key overrides
  to this plan's text: (R1) `GraphBuilder` takes an explicit **`SolutionScope`** (in-scope project
  set) — single-scope for P1, multi-solution is a later loop; build NO solution picker. (R2) Q2 is
  **done** — use `Graph/NameResolver.cs` for short→FQN in every join. (R3) the A3 branch **bypasses
  RenderPlanBuilder**; no parallel plan-builder. (R5) order P1 as **B2 → (B1,B3) → B4**. (R6) **Part D
  gates only F + G, not E** — ship A–E regardless; D delivers a probe kit run against a fresh model.
  (R7) **CLI/Core only this pass — defer ALL desktop UI to PLAN-11**: C3 is CLI-only (entry via
  `--focus`); E2's desktop `SectionSelectionModel` replacement and the desktop section/entry UI move
  to the follow-up. Validate Map/Trace through CLI golden tests, not the desktop.

---

# PART A — Wire the graph into the pipeline (plumbing)

> **STATUS (2026-06-16): A1 + A2 DONE in code — Core builds clean.** Landed: `AnalysisSnapshot.Graph`
> / `Map` / `Entries` (A1); a `GraphAssembly` step in `DiscoveryPipeline.AnalyzeAsync` (after Stage 3 +
> `ResolveEndpointFocusPoints`, before scoring) that runs `SolutionScope.FromModel(model)` →
> `new GraphBuilder(new SyntacticSymbolResolver(), new NoiseFilter(new ProjectClassifier(model.Projects)))
> .Build(model, scope)` → `MapBuilder.Build`, stores all three on the snapshot, and emits a
> `GraphAssembly` Info diagnostic (`N nodes, M edges, K entry points`) (A2). New seams: `Graph/SolutionScope.cs`
> (R1) and `Graph/NameResolver.cs` (R2), threaded through `GraphBuilder`. Test:
> `tests/DevContext.Core.Tests/GraphBuilderTests.cs` (endpoint+handler join, scope filtering, `*Spec`
> regression) — not yet run in CI; run it with the suite. **Remaining: A3 (below) — deferred to land
> with B4**, since the render branch needs a renderer. So execute **B2 → B1/B3 → B4 → A3 → C**.

## A1 — Snapshot carries the graph  ✅ DONE

**Do.** Add to `AnalysisSnapshot` (`src/DevContext.Core/Pipeline/AnalysisSnapshot.cs`):
`CodeGraph Graph`, `MapModel Map`, `ImmutableArray<EntryPoint> Entries`. Default them to empty for the
dry-run/empty constructors so nothing else breaks.

**Acceptance.** Core builds; existing snapshot construction sites compile (set the new fields to
empty where a snapshot is built).

## A2 — `GraphAssembly` stage builds the graph  ✅ DONE

**Do.** In `DiscoveryPipeline.AnalyzeAsync` (`DiscoveryPipeline.cs`), after Stage 3 +
`ResolveEndpointFocusPoints` and **before** the scoring/compression block, add a step:
```csharp
var resolver = new SyntacticSymbolResolver(/* diMap, singleImpl from model */);
var noise    = new NoiseFilter(new ProjectClassifier(model.Projects));
var (graph, entries) = new GraphBuilder(resolver, noise).Build(model);
var map = MapBuilder.Build(model, graph, entries);
```
Store `graph`, `entries`, `map` on the snapshot (A1). Emit an observer/RunReport line
(`graph: N nodes, M edges`). Keep the old scoring/compression running for now (Part E removes it).

**Acceptance.** New `GraphAssemblyTests` (Core): run `AnalyzeAsync` on a fixture with one MediatR
handler + one endpoint; assert `snapshot.Graph.NodeCount > 0`, an `EntryPoint` exists for the
endpoint, and a `Handles` edge exists request→handler.

## A3 — Render request carries entry + depth  ⏳ DEFERRED (do with B4)

**Do.** Add to `RenderRequest` (`Pipeline/RenderRequest.cs`): `string? Entry`, `int? Depth`,
`TraceDetail Detail` (enum Signature|Salient|Full, default Salient). In `DiscoveryPipeline.RenderAsync`,
branch: if `request.Entry` resolves to a snapshot `EntryPoint`, build a `Trace`
(`new TraceBuilder(snapshot.Graph).Build(entry, new TraceOptions { MaxDepth = request.Depth ?? 6 })`)
and render it; else render the `Map`. Keep the legacy markdown path reachable behind the existing
format for now.

**Acceptance.** `RenderAsync` with `Entry = "POST /api/orders"` returns trace content rooted at that
entry; with `Entry = null` returns map content. Covered by A-tests + B (renderers).

---

# PART B — Map (Phase P1, no semantics)

> Goal: replace today's catalog with a correct Map on eShop / VerticalSlice / TodoApi. All facets are
> structural over already-parsed data — no scoring.

## B1 — Finish `GraphBuilder` Map-facing seams
Fill the worked-example pattern for the seams the Map needs (entries already done):
- `AddEventConsumers` — `MessageConsumerDetection` + notification `MediatRHandlerDetection` → `Event`
  nodes + `Consumes` edges.
- `AddDiResolves` — `DiRegistrationDetection` → `Resolves` edges (use `ISymbolResolver` for
  single-impl fallback).
- Aggregates — tag `Entity` nodes from `EfEntityDetection.IsAggregate`.

**Acceptance.** `GraphBuilderTests`: eShop-Ordering-shaped fixture yields the expected entry count,
request→handler joins, and aggregate tags.

## B2 — Evidence-based architecture style (replace the name-substring detector)
**Problem.** `ArchitectureStyleDetector` scores by project-**name** substrings ("domain",
"application") → eShop and VerticalSlice both misclassify as **MinimalApi**
(`ArchitectureStyleDetector.cs:57-74`).
**Do.** Re-implement using **evidence**: layer roles by *reference direction* (a project referenced by
many but referencing few = core/domain), folder roles (`/Domain/`, `/Application/`, `/Infrastructure/`
as folders *or* projects), presence of aggregates/MediatR/Aspire. Emit a human evidence string into
`MapModel.StyleEvidence`.
**Acceptance.** `ArchitectureStyleDetectorTests`: eShop → Microservices/CleanDDD (not MinimalApi);
VerticalSlice → CleanArchitecture/VerticalSlices; TodoApi → MinimalApi. These are the assertions that
fail today.

## B3 — Surface the NuGet package list + topology
**Problem.** Packages are parsed into `ProjectInfo.PackageReferences` (with versions) but never shown;
topology renders as a flat list on multi-solution repos.
**Do.** `MapBuilder`: build `Packages` (dedup by name, keep max version, group) and `Topology` (from
`ProjectReferences`, **scoped to one solution** — see open Q1). 
**Acceptance.** `MapBuilderTests`: package list non-empty with versions; topology is a tree (roots =
projects nothing references), scoped to a single host.

## B4 — `MapRenderer`
**Do.** New `Rendering/MapRenderer.cs` producing the Map layout in `IDEAL-OUTPUT-TARGET.md` §3
(markdown). Wire HTML/JSON: JSON serializes `MapModel`; HTML mirrors markdown. Register so
`RenderAsync` (A3) uses it when no entry is chosen.
**Acceptance.** Golden test of the Map on TodoApi (small, stable). Manual: eShop Map shows correct
style + entry inventory + packages + topology.

---

# PART C — Trace (Phase P2, joins + syntactic calls)

## C1 — Finish `GraphBuilder` trace seams
- `AddRaises` — scan handler/ctor bodies for `AddDomainEvent(new X)` / `new XIntegrationEvent(...)`
  → `Raises` edges (accept the enumerated syntactic shapes; mark others approx — open Q4).
- `AddDataEdges` — `EfEntityDetection` + `DbSet`/repository usage in bodies → `ReadsWrites` + `Entity`
  nodes.
- `AddCallEdges` — `model.CallEdges` via `ISymbolResolver` → `Calls` (Resolution.Syntactic; **mark
  approx**). Port the `CallGraphExtractor.ResolveType` heuristic into `SyntacticSymbolResolver`.
- Entry→handler `Sends`: from the endpoint handler body, `new XCommand(...)` + `mediator.Send(x)` →
  `Member`→`Request` `Sends` edge (so the trace flows entry → send → handler).

**Acceptance.** `GraphBuilderTraceTests`: on an eShop-Ordering fixture the graph contains the path
entry → Sends → request → Handles → handler → Raises → event → Consumes → handler, and a
ReadsWrites to the Order entity.

## C2 — `TraceRenderer` + the dials
**Do.** New `Rendering/TraceRenderer.cs` rendering a `Trace` in the `IDEAL-OUTPUT-TARGET.md` §2 layout
(seam labels, provenance, `Resolution` shown as "verified/approx", `TraceDetail` controls body
inclusion). Implement the fan-out structural ranking TODO in `TraceBuilder.Walk` (leads-to-sink >
framework-leaf) and a framework-boundary stop.
**Acceptance.** Rendering the `POST /api/orders` trace **reproduces the hand-built target**
(`IDEAL-OUTPUT-TARGET.md` §2) modulo `Calls` marked approx. Golden test on a TodoApi entry.

## C3 — Entry picker replaces `--around` proximity
**Do.** Focus = choosing an entry (or a type/method). CLI: `--focus`/`--around` resolves against the
entry inventory + types; desktop: autocomplete from `snapshot.Entries`. `--depth`/`--detail` map to
`TraceOptions`. Map vs Trace derived from whether an entry is set (no scenario/mode).
**Acceptance.** `-f "POST /api/orders"` on eShop yields the trace; bare run yields the Map.

---

# PART D — Validation gate (before P3)

Run the `IDEAL-OUTPUT-TARGET.md` §7 probe: give an LLM a real task ("add a per-line discount to
orders") with **(a)** the P1+P2 trace, **(b)** raw files, **(c)** legacy output. **Only proceed to
Part F (semantics) if the trace wins.** Record the result in `docs/reports/`.

---

# PART E — Retire the scoring/pruning system + test cleanup

> Do this only after B+C render correctly. See `TRACE-ENGINE-DESIGN.md` §7. This is where "remove a
> lot of tests" happens — the deletions are large and intended.

## E1 — Delete the global scorer
- Delete `Pruning/PathProximityPruner.cs` (directory-hop proximity — wrong model).
- Remove the `FinalScore = w·RoleScore + w·FocusScore` block in `DiscoveryPipeline.RunScoringAsync`
  and the `RoleWeights` table in `PatternRelevancePruner`.
- `RenderPlanBuilder`: stop sorting a flat type list by `FinalScore`. The Trace's reachability and the
  Map's facets are the selection; budget trims **depth × fan-out** (Trace) / facet caps (Map) with the
  cut shown. Drop `MaxSurvivingTypes` and the scenario `RoleWeight`/`FocusWeight`/`MaxPathDistance`
  pruning config.
- Repurpose `CallReachabilityPruner`'s BFS as the `TraceBuilder` traversal (already implemented in the
  skeleton) and delete the pruner.
- Move noise exclusion entirely to `NoiseFilter` (already structural); delete the name-suffix
  `IsTestOrNoiseName` logic. **This fixes the live bug** where DDD `*Spec` types are dropped as tests.
- Remove `TypeDiscovery.PathProximityScore/GraphProximity/FocusScore/RoleScore/FinalScore/RelevanceScore`
  once nothing reads them.

## E2 — Delete the obsolete sections + god-class
- Demote `MarkdownRenderer`'s per-detection `Append*` table methods to **Map supporting detail** (or
  delete the ones the Map subsumes: Related-types-by-layer, the raw DI dump, the flat endpoint table).
- Remove the 12-section toggle model (`SectionSelectionModel` System A/B) in favor of Map-facet
  toggles + Trace detail dial (desktop).

## E3 — Test cleanup (expected: large net deletion)
Delete/disable, with the code they covered:
- `PrunerTests` (PathProximity/CallReachability/PatternRelevance scoring) — gone with E1.
- `RankingTests` / any `FinalScore` ordering tests — the ranking they assert no longer exists.
- Scenario pruning-config tests (`MaxSurvivingTypes`, `RoleWeight`, `FocusWeight`, `MaxPathDistance`)
  — config deleted.
- Renderer tests for deleted sections (Related-types-by-layer, raw DI dump).
- Desktop section-drawer System-A/B tests superseded by Map-facet tests.
Add in their place: `GraphBuilderTests`, `TraceBuilderTests`, `MapBuilderTests`, `NoiseFilterTests`
(incl. a regression: a `*Spec`/`*Should` **production** type is **kept**), `ArchitectureStyleDetectorTests`
(eShop/VerticalSlice no longer MinimalApi), Map/Trace golden tests.

**Net effect:** fewer, higher-value tests asserting graph structure + rendered artifacts, replacing
the brittle weighted-score tests.

---

# PART F — Semantic resolution (Phase P3)

**Do.** Add `SemanticSymbolResolver` in **DevContext.Roslyn** implementing `ISymbolResolver` over the
already-loaded MSBuild workspace `SemanticModel` (real symbols: overloads, interface dispatch,
generics, locals). Inject it in place of `SyntacticSymbolResolver` when Roslyn is available
(`!--no-roslyn` and the workspace compiled). `GraphBuilder` is untouched (that's the point of the
seam). `Calls` edges become `Resolution.Semantic`; the trace's "approx" labels turn to "verified".
**Acceptance.** On eShop, the `Calls` edges out of `CreateOrderCommandHandler.Handle` resolve to the
real callees (repository impl, mediator) with `Resolution.Semantic`; route resolution fills the
`<dynamic>` FastEndpoints cases on VerticalSlice.

---

# PART G — Persistent index (Phase P4)

**Do.** Serialize `CodeGraph` (+ `MapModel`, entries) to disk keyed by
`git HEAD + dirty-file digest + tool schema version`. On run: if the key matches, load the graph and
skip extraction + graph assembly entirely; else build and write. The desktop `SnapshotCache` (PLAN-8)
becomes the in-memory hot tier of the same index. The `CodeGraph` records are already serialization-
ready (stable ids); add a `System.Text.Json` context + an `IGraphIndex` with a file impl.
**Acceptance.** Warm run (no source change) loads from index and is ≫ faster than cold; touching one
file invalidates only via the digest and rebuilds.

---

# Final verification gate

1. Core + Desktop test projects green; net test count reflects E3 (expected lower, higher-value).
2. `eval/gates.ps1` passes (`--strict` self-checks clean) on the eval repos.
3. **Map correctness:** eShop style ≠ MinimalApi; entry inventory, packages (with versions), and
   topology tree are present and correct.
4. **Trace correctness:** `-f "POST /api/orders"` on eShop reproduces the `IDEAL-OUTPUT-TARGET.md` §2
   target (seams bridged); `Calls` are "verified" after Part F.
5. **Regression:** a production `*Spec` type appears in output (the old name-suffix bug is gone).
6. Part D probe result recorded; semantics (F) and index (G) were funded only after it passed.
