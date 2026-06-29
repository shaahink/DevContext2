# Handover — Universal Lens (Phases 0–7)

> Written 2026-06-28. Covers all work on `develop` at merge commit `c762c98` (PRs #1–#5) plus the
> in-progress branch `feature/iter6-7-final`. Created for a fresh session or new contributor to
> pick up and continue.
>
> **Resume instruction:** Read `docs/plans/README.md` → find the first iteration whose status ≠ DONE →
> open that iteration's guide in `docs/plans/` → do Step 0 (Reproduce) first, then execute steps. Commit
> at checkpoints; one PR per iteration → `develop`. Run `pwsh -File eval/gates.ps1` at every phase boundary.

## What was built (Phases 0–7)

### Phase 0 — Kernel hygiene
- **Decision (committed `2c12f95`):** token budgeting was already out of the kernel (the `CodeGraph` +
  Map/Trace are built BEFORE the pruners run). Documented the invariant in `DiscoveryPipeline`; added
  `BudgetIndependenceTests`. Did NOT delete the legacy pruners (they size JSON/HTML catalog).
- **PatternRelevancePruner:** kept (not deleted). Still sets `RoleScore`/`FinalScore` consumed by the
  legacy catalog; does NOT run on the narrative Map/Trace path. Retire it with the legacy catalog later.

### Phase 1 — Member-origin edge correctness (the decisive fix)
**Landed via PR #1 (`c84f088`).** `2c12f95` (docs) · `4f457a3` (implementation) · `df256f6` (status docs).

- **Root cause:** `CallEdge` carries `CallerMethod`/`CalleeMethod` but `GraphBuilder.AddCallEdges` folded
  them to `Type→Type`. `AddRaises`/`AddSends`/`AddDataEdges` scanned the whole `type.SourceBody` and
  anchored on the Type node. `TraceBuilder.OutEdgesWithTwin` made any Member inherit ALL its parent Type's
  edges. `EntryPointResolver` dropped `:Method` → anchored on the Type → `CreateItem ≡ UpdateItem`.
- **Fix:** `AddCallEdges` → Member→Member (`ce.CallerMethod`/`ce.CalleeMethod`). Body-scan seams attribute
  each match to its enclosing **method** via a once-per-build Roslyn span locator (`BuildAllMethodSpans`).
  `OutEdgesWithTwin` → **controlled bridge** (Member→own edges only; Type→own + handler-entry-member edges).
  `EntryPointResolver.ResolveFromNode` → `Type:Method` anchors on the Member node.
- **Gate:** `CatalogApi:CreateItem` ≠ `:UpdateItem`; `POST /api/orders` shows no sibling sends/raises;
  the CreateOrder spine stays intact (`gates.ps1` green).

### Phase 2 — Universal entries, controllers first
**Landed via PR #2 (`c0c3fcd`).** `c60b764` (re-probe) · `2b3dd12` (implementation) · `a9b2ed1` (status) · `c939b09` (DntSite eval).

- **Root cause:** controllers use `EndpointDetection` (member-anchored by `AddHttpEntryPoints`) but
  `ResolveEntryTarget` only inspects the handler's **Sends** edges — controllers (no MediatR) have none →
  `0/94 → target`.
- **Fix:** `ResolveEntryTarget` falls back to `ResolvePrimaryCall` — the dominant in-scope service callee
  of the action **member** (precise post member-origin). Prefer a DI-`service`-tagged callee.
  `ResolveViaParentType` retired. Plus infra pseudo-entry filter (Scalar/OpenAPI `GET /` dropped from
  `ServiceDefaults`) and entry dedup (verb×route×file×line).
- **Evidence:** DntSite 0/94 → **34/94** entry→target. Controller action traces diverge.
  `tests/fixtures/ControllerApp` added.

### Phase 3 — Complete & honest traces
**Landed via PR #3 (`259c039`).** `b9934f5` (implementation) · `0cdca7a` (status docs).

- **Domain-event chain:** `data Order → call AddOrderStartedDomainEvent → raises OrderStartedDomainEvent
  → consumes ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler`.
  Fixes: (a) **ctor bridge** — the controlled bridge expands entity/aggregate Type nodes to their
  constructor members (surfacing the ctor's domain-event raises); (b) **variable-arg raise detection** —
  eShop uses `var e = new OrderStartedDomainEvent(...); this.AddDomainEvent(e);` — `AddRaises` now
  resolves the variable to its `new EventType()` like `AddSends`.
- **TOUCHES via Calls:** `CollectGraphEntities` collects entity targets reached via `Calls` edges (EF
  access), resolving Member callees to their owning entity Type. eShop TOUCHES gained `Buyer`.
- **Pipeline annotation:** `WrappedBy` edges rendered once under the first `send`: `pipeline ▸
  LoggingBehavior → ValidatorBehavior → TransactionBehavior`.
- **Honest truncation:** explicit `(stopped at depth N; K branches omitted)` / `(K more branches omitted
  beyond fan-out)` markers. `TraceStep.Omitted` added; `HasFollowable` removed.
- **IndirectWiringDetector ungated** to also run in Map (`overview`) mode (was deep-dive-only).

### Phase 4 — Honest Map & detection
**Landed via PR #4 (`13adda7`).** `a336e25` (implementation) · `f754979` (status docs).

- **Scope stamp (Critical 3):** `SCOPE 5-project closure of 24-project eShop` rendered on a partial
  closure. `ArchitectureStyleDetector` suppresses system-level verdicts (Microservices/ModularMonolith)
  on a slice (< 75% of the `.sln`). Whole-solution eShop stays `Microservices`.
- **OrchardCore ≠ Microservices** was already satisfied on develop (`ModularMonolith` — it lacks an
  Aspire AppHost); verified. eShop Microservices preserved.
- **STACK `$(…)` filter (Low 16):** MSBuild-variable tokens (`$(CommonTargetFrameworks)`) stripped from
  TFMs (`MapRenderer`) and package versions (`MapBuilder.BuildPackages`). OrchardCore STACK is clean.
- **Aggregate tightening (Medium 10):** `IsAggregate` now requires implementing `IAggregateRoot`
  (precomputed from `model.Types`) — the `HasOwnDbSet` heuristic is dropped. eShop aggregates:
  `Buyer · Order` only (was 5). Catalog/CleanArch CRUD: none.
- **Trace-noise polish:** `nameof` pseudo-calls + ASP.NET `ControllerBase` result-helpers (`Ok`,
  `NotFound`, …) filtered in `AddCallEdges` as self-call noise.

### Phase 5 — Queryable kernel (inverse edges + query API)
**Landed via PR #5 (`c762c98`).** `1fe7576` (implementation) · `9513d36` (docs) · `d6338c5` (status docs).

- **Inverse edges (req 3):** `CodeGraph` ctor derives `_inEdges` from `_outEdges` (kept derived —
  serialization-clean). `InEdges(id, kind?)` powers `neighbors(in)` / `find_usages` in O(degree).
- **`GraphQuery` facade (req 4-v1):** `entrypoints · trace · map · stats · node · neighbors ·
  find_usages · ResolveNodeId`. Face-agnostic + JSON-friendly. `NodeDetail`, `EdgeRef`,
  `EdgeDirection` records.
- **Render path re-expressed:** `DiscoveryPipeline.RenderAsync` uses `GraphQuery.Stats()` +
  `GraphQuery.Trace()` — byte-identical (golden + eval + CLI matrix green).
- **7 `GraphQueryTests`** (both edge directions, `find_usages`, `neighbors`, `node`, `resolve`,
  `entrypoints`).
- **DntSite TOUCHES deferred (split out):** diagnosed — entities are registered via
  `RegisterAllDerivedEntities` (reflection), which `EfCoreExtractor`'s syntax-tree walker cannot resolve.
  NOT an FQN issue. Split to a separate follow-up (see "Known deferred" below).

### Phase 6 — Performance (in-progress on `feature/iter6-7-final`)
- **Entry-scoped binding:** For Map mode (no focus), the call graph is seeded from all
  endpoint/handler/worker source files (~70 for DntSite) instead of binding all ~1342 files.
  Drops Stage-3 cold cost from ~30s to ~3s. Entry→target 34/94 preserved. Eval duration: ~103s → ~32s.

### Phase 7 — Browse UI wire-up (in-progress on `feature/iter6-7-final`)
- `IAnalysisService.GetQuery(snapshot)` exposes `GraphQuery` to the desktop so the browse UI can use
  `Node`, `Neighbors`, `FindUsages`, and `ResolveNodeId`. The full browse interactive redo (clicking
  nodes in trace/map to see detail/neighbors) is **not yet wired** into the HTML-rendering layer — that
  is the Phase 7 follow-up (requires Blazor component changes in `OutputPanel.razor`).

## Architecture overview (key files + their roles)

| File | What it does |
|---|---|
| `src/DevContext.Core/Graph/GraphBuilder.cs` | The heart — joins detections into `CodeGraph`. All phase-1/2/4 changes live here: `AddCallEdges` (member→member), `AddRaises` (variable-arg + method-origin), `AddSends`/`AddDataEdges` (method-origin), `AddEntityNodes` (subtype expansion), `ResolveEntryTarget` (primary-service-call fallback), `AddHttpEntryPoints` (infra filter + dedup), `ResolvePrimaryCall`. |
| `src/DevContext.Core/Graph/TraceBuilder.cs` | Entry-rooted trace walker. `OutEdgesWithTwin` (controlled bridge — ctor + handler members), `CollectGraphEntities` (TOUCHES via Calls), `Omitted` count, `Pipeline` annotation, `BuildBridgeMemberIndex`. |
| `src/DevContext.Core/Graph/CodeGraph.cs` | Immutable, queryable graph. `_outEdges` + `_inEdges` (derived, serialization-clean). `Node(id)`, `OutEdges`, `InEdges`, `Contains`. |
| `src/DevContext.Core/Graph/GraphQuery.cs` | The Phase-5 query facade. `entrypoints · trace · map · stats · node · neighbors · find_usages · ResolveNodeId`. |
| `src/DevContext.Core/Graph/EntryPointResolver.cs` | Resolves a focus string to an `EntryPoint`. Phase 1: `Type:Method` → Member node. Phase 2: route/type fallback. |
| `src/DevContext.Core/Extractors/Specific/CallGraphExtractor.cs` | Roslyn call-graph builder. Phase 6: entry-scoped binding (Map mode). Focus-scoped binding (Trace mode). `EntrySeedFiles`. |
| `src/DevContext.Core/Extractors/Specific/EfCoreExtractor.cs` | Entity detection. Phase 4: `IsAggregate` tightened (`IAggregateRoot` signal, not `HasOwnDbSet`). |
| `src/DevContext.Core/Extractors/Generic/ArchitectureStyleDetector.cs` | Style detection. Phase 4: partial-closure suppression (Microservices/ModularMonolith on a slice < 75%). |
| `src/DevContext.Core/Graph/MapBuilder.cs` | Assembles `MapModel`. Phase 4: `ScopeNote` computation. Phase 4: `BuildPackages` filters MSBuild-var versions. |
| `src/DevContext.Core/Rendering/MapRenderer.cs` | Map markdown rendering. Phase 4: `SCOPE` line + STACK `$(` filter. Phase 3: pipeline annotation + truncation marker. |
| `src/DevContext.Core/Rendering/TraceRenderer.cs` | Trace markdown rendering. Phase 3: pipeline annotation + honest truncation. |
| `src/DevContext.Core/Pipeline/DiscoveryPipeline.cs` | The pipeline (analyze → render). Phase 5: re-expressed render path over `GraphQuery`. Phase 0: budget-invariant comments. |
| `src/DevContext.Core/Pipeline/AnalysisSnapshot.cs` | Immutable analysis handle. Holds `Graph`, `Map`, `Entries`. The "analyze once" result. |
| `src/DevContext.Core/Graph/SolutionScope.cs` | Scope metadata (which projects were analyzed). Phase 4: used by scope stamp. |
| `src/DevContext.Core/Resolvers/NameResolver.cs` | Short-name → FQN resolution. Underpins every graph join. |

## What passes every gate (`gates.ps1`)

| Step | What |
|---|---|
| Step 1 | Build (Release, 0 warnings/errors — CA1822 + analyzer enforced) |
| Step 2 | Fast unit tests (~266 core + 64 desktop, 2 skipped) |
| Step 3 | Eval expectation tests (18 currently: todoapi, verticalslice, eshop, automapper, controllerapp, dntsite, catalog — all markdown/JSON checks) |
| Step 4 | CLI `--strict` matrix (5 commands, incl. `--max-tokens 2000 --strict`) |

## Git workflow (one PR per iteration)

All iterations land on `develop` via PRs:
- PR #1 — iter1 (Phases 0–1): `c84f088`
- PR #2 — iter2 (Phase 2): `c0c3fcd`
- PR #3 — iter3 (Phase 3): `259c039`
- PR #4 — iter4 (Phase 4): `13adda7`
- PR #5 — iter5 (Phase 5): `c762c98`
- In-progress branch: `feature/iter6-7-final` (Phases 6–7 + deferred fix + this handover)

Pattern: `git checkout -b feature/iterN-<name>` off `develop` → implement → commit at checkpoints →
`pwsh -File eval/gates.ps1` → `git push -u origin feature/...` → `gh pr create --base develop` →
`gh pr merge --merge` → `git checkout develop; git merge --ff-only origin/develop`.

## Known deferred / still-open

| Item | Status | Where |
|---|---|---|
| **DntSite TOUCHES** | Entity subtype expansion applied (GraphBuilder.AddEntityNodes now tags BaseEntity children). Still empty for `GET /Feed/News` — the local-variable receiver resolution in CallGraphExtractor (`item.MapToNews...`) drops the `BlogPost` Calls edge because syntactic resolution returns lower-case "blogPost" (unknown). The subtype expansion IS correct; TOUCHES will populate when the call graph resolves entity-member receivers. | `GraphBuilder.cs:596` |
| **LLM re-probes (Phase 1 & 3)** | Scored A/B probe-kits scaffolded — inherently human-in-the-loop. | `docs/reports/phase1-member-origin-reprobe.md` + `docs/reports/probe-phase3.md` |
| **EstimateProvenance accuracy** | Line numbers are computed relative to `type.SourceBody`, not the file. Salient lines can show wrong source context (e.g., a Get action shows Create's body). Fix needs source-start-line info on `TypeDiscovery`. Pre-existing since before iter1. | `GraphBuilder.cs:1055` |
| **Browse UI interactive redo (full)** | The `GraphQuery` facade is available to the desktop (`IAnalysisService.GetQuery`). The HTML rendering in `OutputPanel.razor` has no interactive click handlers — nodes can't be clicked for detail/neighbors. Splitting the `MainViewModel` god class, rebuilding the surface as a thin view over `GraphQuery`, and adding interactive navigation is the **Phase 7 proper** scope (planned, not yet done). | `DevContext.Desktop/*` |
| **MCP server (Phase 8)** | Not started. The `GraphQuery` facade is MCP-ready (JSON-friendly operations). | — |
| **Persistent index (Phase 9)** | `CodeGraph` is already serialization-clean; in-edges are derived. Additive work. | — |
| **Coverage ladder (Phase 10)** | Entry-point shapes beyond controllers/minimal-API/MediatR (gRPC, SignalR, Blazor, Functions). | — |

## How to run / build / test

```powershell
# Build (Release)
dotnet build DevContext.slnx -c Release

# Run the CLI on a repo
dotnet src\DevContext.Cli\bin\Release\net10.0\DevContext.Cli.dll analyze <path> [--focus <entry>] [--depth N]

# Run the gate (build + fast tests + eval + CLI matrix)
pwsh -File eval\gates.ps1

# Run only eval tests
dotnet test DevContext.slnx --filter "Category=Eval"

# Regenerate golden tests (when output legitimately changes)
$env:UPDATE_GOLDENS="1"
dotnet test DevContext.slnx --filter "FullyQualifiedName~Golden"
$env:UPDATE_GOLDENS=""

# Run the benchmark
dotnet run -c Debug --no-build --project benchmarks/DevContext.Benchmarks -- repos
```

## Key design conventions

- **Builder methods without instance state must be `static`** — the project treats analyzer warnings as errors (CA1822 enforced).
- **`CodeGraph` is immutable and serialization-clean** — `FrozenDictionary` of nodes + out-edges; in-edges derived in ctor.
- **One node per class** (collapsed to `Type` with role `Tags`) — replaces the old per-role node kinds (Handler/Request/…).
- **Member-origin edges:** call/raise/send/data edges originate from the `Member` node that contains them. The `OutEdgesWithTwin` bridge expands Type→handler-member + entity/aggregate Type→ctor-member.
- **Unified FQN resolution:** `NameResolver.Resolve(shortName)` is the single source of canonical FQNs used by all joins.
- **Phase-5 query API:** faces (CLI, desktop, MCP) are clients of `GraphQuery` over the immutable `CodeGraph` — no rendering concerns leak in.
- **Harness delta:** every iteration ratchets `eval/expectations/` (JSON checks) + `TraceQualityTests` (trace guards) + `ACCEPTANCE.md` (the bar). Golden tests guard byte-identical output.
