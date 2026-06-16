# Trace Engine — Deep Analysis & Migration Design

> How we get from the current engine to the Map + Trace artifacts in `IDEAL-OUTPUT-TARGET.md`,
> **properly** — reuse what's sound, build the missing connected-graph layer, and design (not yet
> build) the persistent index that makes reliable semantic resolution affordable.
>
> Verified against current source (not the point-in-time eval `.md`s) on 2026-06-15. Files cited
> are current; the codebase changes daily, so re-confirm line refs at build time.

---

## 0. The one finding that reframes everything

The engine is a **detection accumulator, not a connected model.**

- `DiscoveryModel`: `Types` = flat `ConcurrentDictionary<FQN, TypeDiscovery>`; `Detections` =
  flat `ConcurrentBag<Detection>` (12 polymorphic kinds); `CallEdges` = a *separate* flat bag.
- `SharedAnalysisContext`: `ProjectGraph` (project adjacency) and `CallGraph` (caller-key→edges)
  are two more *separate* string-keyed structures.
- Renderers read **one detection kind at a time** and print **one table at a time**
  (`MarkdownRenderer.AppendEndpoints`, `AppendDiRegistrations`, `AppendMediatRHandlers`…).

That is *why* the output is a catalog. The trace is not blocked by missing detection — the join
data is already captured:

| To bridge this seam… | …we already have |
|---|---|
| endpoint → command → handler | `EndpointDetection` + `MediatRHandlerDetection(RequestType→HandlerType)` |
| domain event raised → handled | `MediatRHandlerDetection(Kind=Notification, RequestType=eventType→HandlerType)` |
| integration event published → consumed | `MessageConsumerDetection(MessageType→ConsumerType)` / `EventFlowDetection` |
| interface → concrete impl | `DiRegistrationDetection(ServiceType→ImplementationType, Shape)` |
| aggregates / entities / tables | `EfEntityDetection(EntityType, DbContextType, IsAggregate, KeyProperties)` |
| middleware / workers | `MiddlewareDetection`, `BackgroundWorkerDetection` |

**The work is to *connect* these into a graph, then traverse it — not to detect more.** That is the
entire thesis of this rebuild, and it's why most of it needs no semantic model.

---

## 1. What we have (current architecture, code-grounded)

**Pipeline (sound — keep).** `DiscoveryPipeline` already does analyze→`AnalysisSnapshot`→`RenderAsync`
(PLAN-1). Stages: ① discovery (seq) → ② generic extractors (parallel) → seal signals + run
`ArchitectureStyleDetector` → ③ specific extractors (parallel) → resolve focus → score (3
pruners-as-scorers) → compress → render. Budget + cap enforced once, at render, in `RenderPlanBuilder`.

**Detections (the join-data asset — keep).** All 12 extractors produce the records in the table
above. `ProjectStructureExtractor` parses TFMs + project refs + **package references with versions**
into `ProjectInfo`. `DependencyExtractor` builds the `ProjectGraph` adjacency and maps 24 packages →
signals.

**Resolution (weak — the heart of the problem).** Everything is **syntactic + string-heuristic**:
- `MediatRExtractor` parses base-type *strings* (`IRequestHandler<...>`), `HandlerType` = the class
  *short* identifier (no FQN).
- `CallGraphExtractor` resolves callees via a field-name→declared-type map + DI map +
  first-impl-wins interface map; locals/overloads/generics unresolved; bridges **no** indirection;
  the BFS-from-entries result is computed then **discarded** (diagnostic only).
- `ArchitectureStyleDetector` scores styles by **project-name substrings** (`"domain"`,
  `"application"`, `"infrastructure"`, `"module"`). eShop's `Ordering.Application` is a *folder*,
  VerticalSlice uses `Core`/`UseCases` → both misclassify as **MinimalApi**.

### Sound — reuse as-is
Stage 1–2 discovery; signals; package + project-graph parse; all detection extractors; the
snapshot/render split; `IDiscoveryObserver`/`RunReport`; renderers' plumbing; `OutputSelfCheck`;
the parse-once `AnalysisCache`; desktop `SnapshotCache` (PLAN-8).

### Weak — fix or replace
`ArchitectureStyleDetector` (name-substring brittle); `CallGraphExtractor` (syntactic, no
indirection, entry seeding discarded); NuGet list captured-but-unsurfaced; `LayerClassifier`
(domain types land in "Unknown"); detections name-based → FQN join ambiguity; `--around` =
directory proximity (models the wrong thing).

---

## 2. Target architecture — insert a Relational layer

One new layer between extraction and rendering. Extraction stays; rendering consumes graph models
instead of raw detections.

```
Stage 1–2 discovery + Stage 3 detections           (REUSE unchanged)
        │
        ▼  [NEW pipeline stage: GraphAssembly — the "worker"]
   GraphBuilder:  join Types + Detections + CallEdges  →  CodeGraph (typed nodes + typed edges)
   EntryPointIndex: unify endpoints + consumers + workers + (library public API) → EntryPoint[]
        │
        ├─► MapBuilder(CodeGraph)            → MapModel     (analyze-time; part of the snapshot)
        └─► TraceBuilder(CodeGraph, entry, depth) → TraceModel  (RENDER-time lens — cheap, re-runnable)
        │
        ▼
   Renderers consume MapModel / TraceModel    (EVOLVE: Markdown/Html/Json render the graph,
                                               old per-detection tables become Map supporting detail)
```

Key placement decision, consistent with PLAN-1: **the `CodeGraph` is analyze-time** (goes in the
snapshot); **a `Trace` is a render-time lens** built from the graph for a chosen entry + depth. So
changing entry/depth/detail re-renders in milliseconds without re-analysis — the same property the
budget slider already has.

### 2.1 The CodeGraph — the model that's missing

```
CodeGraph
  Nodes (typed, stable Id = FQN-based):
    TypeNode        ← TypeDiscovery
    MemberNode      ← method/ctor (Type.Member)
    EntryPointNode  ← endpoint | message-consumer | hosted-service | (library) public API
    RequestNode     ← MediatR command/query/notification (incl. domain events)
    HandlerNode     ← IRequestHandler / INotificationHandler impl
    EntityNode      ← EF entity (Aggregate flag)
    EventNode       ← domain / integration event
    DataStoreNode   ← DbContext / table
    ServiceNode     ← DI service↔impl
  Edges (typed, directional, each with: Provenance file:line · Resolution{Syntactic|Semantic|Join} · Confidence):
    Calls        Member → Member
    Sends        Member → RequestNode → (Handles) → HandlerNode
    Handles      HandlerNode → RequestNode
    Raises       Member → EventNode
    Consumes     EventNode → HandlerNode
    ReadsWrites  Member → EntityNode / DataStoreNode
    Resolves     interface TypeNode → impl TypeNode   (di)
    WrappedBy    RequestNode → behavior chain          (pipeline)
```

Every edge records **how it was resolved** so the report can be honest ("call edge: approx /
syntactic" vs "verified / semantic") — this is P3 (show your work) applied to the graph itself.

### 2.2 Seam recipes — what builds each edge, and whether it needs semantics

| Edge | Built from (existing data) | Needs SemanticModel? |
|---|---|---|
| `Handles` / `Consumes` | `MediatRHandlerDetection` / `MessageConsumerDetection` (type→handler) | No — pure join |
| `Resolves` (di) | `DiRegistrationDetection`; fallback single-implementor | No — join |
| entity/aggregate/table | `EfEntityDetection` | No — join |
| `WrappedBy` (pipeline) | small new detector for `IPipelineBehavior` / `AddXxxBehavior` regs | No — small add |
| `Sends` (which request a member dispatches) | caller body: `new XCommand(...)` + `Send/Publish(x)` | Better with semantics; syntactic OK for the common `new` + `Send` shape |
| `Raises` (member → event) | caller body: `AddDomainEvent(new XEvent())` / `new XIntegrationEvent()` | Better with semantics; syntactic OK |
| `Calls` | `CallEdges` (today syntactic, lossy) | **Yes for reliability** (overloads/locals/generics/dispatch) |

**~5 of 7 seam kinds are joins over data we already have** → the Map and the *indirection-bridged
skeleton* of the Trace are reachable with **no** semantic model. Only fine-grained `Calls` (and
precise body→request linkage) need semantics for trust.

---

## 3. Reliability upgrade (semantic) + the index (design now, build later)

**One interface, two implementations.** Put resolution behind:

```csharp
interface ISymbolResolver {
    SymbolRef? ResolveCallee(Invocation ctx);     // who does this call land on
    TypeRef?   ResolveImplementation(TypeRef iface, DiContext di);  // interface → concrete
    TypeRef?   ResolveExpressionType(Expression e);  // locals, returns, generics
}
```

- `SyntacticResolver` — today's field-map/DI-map heuristics (ship first).
- `SemanticResolver` — Roslyn `SemanticModel` over the **already-loaded** MSBuild workspace
  (`RoslynWorkspaceProvider`); real symbols, overloads, dispatch, generics.

`GraphBuilder` depends on `ISymbolResolver`, never the impl. So we ship `CodeGraph` on syntactic
resolution and **swap to semantic without touching graph/trace/render**. This is the foundational
fork made cheap: it's a strategy swap, not a rewrite.

**The cost, and the index that removes it (your caching insight).** `SemanticModel` needs a
compiled `Compilation` per project — slow on big solutions. Mitigation = the deferred **persistent
index**: serialize the resolved `CodeGraph` (records, stable ids) keyed by
`(git HEAD + dirty-file digest + tool schema version)`. First run pays the semantic cost; re-runs
and every re-render load from disk → cost becomes **on/off (first run)**, not per-run. The desktop
in-memory `SnapshotCache` becomes the hot tier of the same index.

**Design now, build later:** make `CodeGraph` and its nodes/edges **serializable records with
stable ids from day one**, so the index is a drop-in later, not a refactor. Nothing in P1/P2 should
assume in-memory-only identity.

---

## 4. New engine vs evolve — recommendation

**Evolve, with a new core layer. Do not rewrite extraction** (it's the asset and the test corpus
depends on it). Concretely:

- **New folder `DevContext.Core/Graph/`**: `CodeGraph` + nodes/edges, `GraphBuilder`,
  `EntryPointIndex`, `TraceBuilder`, `MapBuilder`, `ISymbolResolver` (+ `SyntacticResolver`).
  Roslyn `SemanticResolver` lives in `DevContext.Roslyn` (keeps the heavy dep isolated, per ADR-001).
- **New pipeline stage `GraphAssembly`** between Stage 3 and Scoring — the "new worker." Builds the
  `CodeGraph` and `MapModel`; both go into `AnalysisSnapshot`.
- **`RenderRequest` gains `Entry` + `Depth` + `Detail`** → `TraceBuilder` produces a `TraceModel` at
  render time from the snapshot's `CodeGraph`. Map vs Trace = derived from whether `Entry` is set.
- **Scoring**: replace directory-proximity with **graph distance from the entry** on `CodeGraph`
  (`FocusScore` = closeness in the trace). Retire `PathProximityPruner`; `--around` becomes the
  entry picker.
- **Renderers**: add `TraceRenderer` + `MapRenderer`; the existing per-detection `Append*` methods
  become the Map's supporting-detail facets, not top-level sections. JSON serializes `CodeGraph` →
  first-class structured output.
- **Style/layer**: replace name-substring scoring with evidence (reference direction, folder roles,
  aggregate/MediatR presence). Surface the NuGet list (data already in `ProjectInfo`).

What gets **deleted/demoted**: `--task`/intent NLU; `PathProximityPruner`; the 12-section toggle
model (→ Map facets + Trace detail dial); `MarkdownRenderer` as god-class (split per artifact).

---

## 5. Phasing — each phase ships something real

| Phase | Delivers | Needs semantic? | Gate |
|---|---|---|---|
| **P1 — Map** | `GraphBuilder` skeleton + `EntryPointIndex` + `MapBuilder` + evidence-based style + surfaced NuGet list + dep tree scoped to one solution | No | Map is correct on eShop/VerticalSlice/TodoApi |
| **P2 — Trace skeleton** | `TraceBuilder` over `CodeGraph`, all join-seams bridged, depth/detail dials, entry-as-render-lens; `Calls` marked "approx" | No (syntactic) | Hand-built `POST /api/orders` trace reproduced |
| **validation** | LLM-value probe (§7 of target doc): P1+P2 trace vs raw files vs old output | — | trace wins → fund P3 |
| **P3 — Semantic** | `SemanticResolver` behind `ISymbolResolver`; trustworthy `Calls`/impl/route/body→request | Yes | call edges verified on eShop |
| **P4 — Index** | serialize content-keyed `CodeGraph`; semantic cost → one-time | — | warm-run ≪ cold-run |
| **P2.5/later — Library** | public-API surface map for AutoMapper-shaped repos | No | — |

P1 alone replaces today's catalog with a real Map and **proves the join model** before any heavy
investment. P2 delivers the headline feature on syntactic resolution. Semantics (P3) and the index
(P4) are funded only after the §7 probe says the artifact is worth it.

---

## 6. Open questions to settle before P1

1. **Solution scoping.** Repos like VerticalSlice contain *multiple* solutions (22 projects = 3
   sample apps). Map/Trace must scope to one solution/entry-host, not union everything. Pick the
   rule (nearest `.sln`, or user-selected) before building the Map.
2. **Id scheme.** Detections use short type names; `Types` uses FQN. The graph needs **one** id
   scheme + a name→FQN resolver (handle collisions by namespace/assembly). This is the backbone of
   every join — design it first.
3. **Library entry definition.** Deferred, but decide the shape (public API by capability) so the
   `EntryPoint` abstraction is forward-compatible.
4. **How much body to read for `Sends`/`Raises` in P2** without semantics — the common
   `new XCommand(...)` + `mediator.Send(x)` shape is syntactically reliable; enumerate the shapes we
   accept and mark the rest "approx".

---

## 7. Scoring & pruning — what survives

Decoded, the current system is **one global relevance-ranker over the flat universe of every
discovered type**, feeding exactly one decision: which types `RenderPlanBuilder` prints (the
"Related types by layer" dump) within budget. `PathProximityPruner`→`PathProximityScore`,
`CallReachabilityPruner`→`GraphProximity`, `PatternRelevancePruner`→`RoleScore` (+ test
hard-exclusion + name penalties + a separate library-mode boost); then
`FinalScore = w·RoleScore + w·FocusScore`; then sort-and-fill-to-budget.

**Verdict: it's an artifact of the catalog problem, and the Map+Trace model dissolves most of it.**

- The **Map** has no type-dump → no global type ranker needed (entries, projects, packages,
  aggregates are bounded; show or group them all).
- The **Trace** selects nodes by **reachability from the entry** over the CodeGraph, bounded by
  depth/fan-out → the selection *is the graph structure*, not a score. `FocusScore` is promoted from
  "a term in a weighted sum" to "the traversal itself".

Three concerns are conflated in the pruners and do **not** share a fate:

| Concern (currently fused) | Verdict | Becomes |
|---|---|---|
| Noise filtering (tests/generated/migrations) | Keep — but it's a *filter*, not a score | `NoiseFilter` (binary, structural, at graph-build) |
| Reachability (PathProximity + CallReachability) | PathProximity dies; CallReachability is the right idea | the **Trace traversal** over `CodeGraph` |
| Role importance (the weight table) | Mostly dies as a constant | **graph position** (an entry matters because it's an entry; a handler because the trace reaches it) |

**The replacement is filter → structure → local ranking:**
1. **Filter** (binary, deterministic, no weights): exclude tests/generated/migrations from being
   nodes. The only survivor of `PatternRelevancePruner`.
2. **Structural selection** (no scores): Map = facet builders; Trace = reachability. The CodeGraph
   *is* the relevance model.
3. **Local ranking** — only where a bounded list overflows budget, and only on explainable
   structural signals (entry centrality/fan-out; trace fan-out "leads-to-sink vs framework-leaf";
   depth-first trimming with the cut shown). No global `FinalScore`, no scenario weight tables, no
   incompatible [0,1]-vs-boost scales.

**Delete:** `PathProximityPruner`, the weighted `FinalScore`, the `RoleWeights` table as a *ranker*,
scenario pruning weights, the global `MaxSurvivingTypes` cap.
**Repurpose:** `CallReachabilityPruner`'s BFS → the Trace traversal core (over `CodeGraph`).
**Keep, cleaned up:** noise exclusion → `NoiseFilter`, moved to graph-build, structural not
name-suffix.

**The defect that proves it.** The name-suffix rule (`"Spec"`, `"Specs"`, `"Should"` → "noise") is
misfiring on production code *right now*: the VerticalSlice run "pruned as test type"
`Clean.Architecture.Core.ContributorAggregate.Specifications.ContributorByIdSpec` and
`NimblePros.SampleToDo.Core.ProjectAggregate.Specifications.IncompleteItemsSpec` — DDD
Specification-pattern classes, core domain code. `NoiseFilter` keys off project kind + folder role
instead, and never excludes by type-name suffix.

---

## 8. Skeleton already in the repo (`src/DevContext.Core/Graph/`)

Additive, compiles clean, **not yet wired into DI/pipeline** — the spine for the agent to evolve.

| File | What it is | State |
|---|---|---|
| `CodeGraph.cs` | `NodeKind`/`EdgeKind`/`Resolution`, `NodeId`, `GraphNode`, `GraphEdge`, `CodeGraph`, `CodeGraphBuilder` | **Done — the stable model.** Serialization-ready for the P4 index. |
| `EntryPoint.cs` | `EntryPointKind`, `EntryPoint` | Done — DTO |
| `ISymbolResolver.cs` | `ISymbolResolver`, `SymbolRef`, `SymbolContext`, `SyntacticSymbolResolver` | Interface done; syntactic impl is a conservative stub (P2 ports heuristics, P3 adds semantic impl) |
| `NoiseFilter.cs` | `ProjectClassifier`, `NoiseFilter` | **Done — real, structural, fixes the Spec bug** |
| `TraceBuilder.cs` | `SeamKind`, `TraceStep`, `Trace`, `TraceOptions`, `TraceBuilder` | **Traversal spine functional** — evolve fan-out ranking + framework-boundary stop |
| `MapBuilder.cs` | `MapModel`, `ProjectNode`, `PackageGroup`, `MapBuilder` | Model done; builder is a stub with per-facet TODOs |
| `GraphBuilder.cs` | `GraphBuilder` | Worked examples (type nodes, HTTP entries, MediatR `Handles`) + stubbed join seams |

Execution plan: `docs/plans/PLAN-10-TRACE-ENGINE.md`.

---

## 9. Resolved decisions (2026-06-16) — the §6 open questions, closed

**R1 — Scope / multi-solution (Q1).** The unit of analysis is **one system** = the resolved
`.sln`/`.slnx` (its `model.Solution.ProjectPaths`), or the project-reference closure from the entry
when there's no `.sln`. **Build single-scope for P1 — no solution picker.** But make scope an
**explicit `SolutionScope` (the in-scope project set) parameter to `GraphBuilder`**, never implicit
"all `model.Projects`". A microservice constellation (eShop) is *one* scope spanning many projects —
correct and desired. A repo with several independent solutions (the VerticalSlice eval folder)
becomes **"run the engine once per scope" later — a loop, zero model change**. When >1 `.sln` exists,
analyze the resolved one and emit a diagnostic ("N solutions found; analyzing X; pass `--solution`");
`--solution` is the future multi-select seam. *This is the "design it in now, implement later" answer.*

**R2 — Id scheme (Q2) — DONE IN CODE.** `Graph/NameResolver.cs` maps short→FQN with collision
disambiguation; `NodeId.Key` is always canonical FQN. **Detections stay short-name** (no breaking
change — option (b) rejected). `GraphBuilder.AddHandlerJoins` is the worked pattern; every join calls
`names.Resolve(...)`. Optional later: collapse `Handler`/`Service` node kinds into `Type` + role tags.

**R3 — RenderPlanBuilder migration (Q3).** The A3 `RenderAsync` branch **bypasses RenderPlanBuilder
entirely** for Map/Trace. **No `MapPlanBuilder`/`TracePlanBuilder`** — a "plan over a flat list" is
exactly what we're deleting. Trace budget = `TraceOptions` depth/fan-out + post-trim (cut shown); Map
budget = facet caps. RenderPlanBuilder + the scorer keep running for the **legacy path only**,
untouched until Part E.

**R4 — Sends/Raises shapes for P2 (Q4).** Match by **method-name set + a `new T(...)` argument**,
reusing receiver/local resolution:
- *Sends/Publish*: receiver `.Send`/`.SendAsync`/`.Publish`/`.PublishAsync` where the receiver
  resolves to IMediator/ISender/IPublisher; arg = `new TRequest(...)` inline or a local assigned
  `new TRequest(...)` in the same method. This covers `_mediator.`, `this.mediator.`, `await mediator.`
  — all the same member-access shape, so don't special-case names.
- *Raises*: method-name set `{AddDomainEvent, RaiseDomainEvent, AddEvent}` with a `new TEvent(...)`
  arg; plus `new TIntegrationEvent(...)` handed to an integration-event service.
Anything outside these (local not from `new`, dynamic dispatch, factory-built events) is **marked
`Resolution.Syntactic` (approx), not dropped**. Part F cleans the long tail. The list is a small
constant the agent extends.

**R5 — P1 order (Q5).** **B2 first** (style detector — actively wrong today and unit-testable without
the renderer), **B4 last** (renderer consumes the others). B1/B3 either order. → B2 → (B1, B3) → B4.

**R6 — Validation probe (Q6).** Part D **gates only Parts F + G** (the expensive semantic + index
work), **not Part E**. The useful core (A–E) ships regardless — Map+Trace beat the catalog even on
syntactic resolution. Deliverable: a probe kit in `docs/reports/` (the three inputs — trace / raw
files / legacy — + the task + a scoring rubric), run against a **fresh, context-free model session**
(unbiased), then decide F/G.

**R7 — Desktop (Q7).** **CLI/Core first; desktop deferred to PLAN-11.** PLAN-10 builds A–G against
Core + CLI and validates Map/Trace via CLI golden tests. The desktop work — C3's entry picker and
E2's `SectionSelectionModel` replacement — moves to a follow-up; the engine is the substance and the
desktop is a thin view over it. CLI still gets the entry picker via `--focus`/`--around`.
