# Housekeeping & Stats Refresh — Handover

> Warm-start doc for whoever picks up next. Branch of record: **`chore/housekeeping-stats`**
> (off `develop` at `b633746`). Covers: GitHub refresh, eval aspirational fixes, Map/Trace
> rendering upgrades, stats page redesign, entry→target heuristics. The prior handover is
> in `docs/iterations/iteration-8/HANDOVER.md`.

## Pick your thread (read order)

- **Map/Trace rendering:** `src/DevContext.Core/Rendering/{MapRenderer,TraceRenderer,NarrativeSections}.cs`
- **Entry→target + graph construction:** `src/DevContext.Core/Graph/GraphBuilder.cs`
- **Trace engine rules:** `docs/TRACE-RULE-REFERENCE.md`
- **Stats page (Desktop):** `src/DevContext.Core/Rendering/RunReportHtmlRenderer.cs` +
  `src/DevContext.Desktop/wwwroot/app.css`
- **Eval expectations:** `eval/expectations/{eshop,verticalslice}.json`
- **North star:** `docs/IDEAL-OUTPUT-TARGET.md`
- **Gap analysis:** `docs/archive/reports/OUTPUT-QUALITY-ASSESSMENT.md`

## Stack / build / run

- .NET 10 (`global.json`). Shell: Windows PowerShell 5.1.
- Build: `dotnet build DevContext.slnx` (0w 0e).
- Test: `dotnet test tests/DevContext.Core.Tests` (**261 pass / 2 skip**) ·
  `tests/DevContext.Desktop.Tests` (**64**).
- Gate: `./eval/gates.ps1` (build → fast tests → eval → CLI matrix).
- Eval repos: `eval-repos/` (TodoApi, VerticalSlice/MinimalClean, eShop, AutoMapper, OrchardCore).
- `$env:UPDATE_GOLDENS=1; dotnet test` regenerates goldens (review first).

## What's done (this branch, 5 commits)

### GitHub Refresh
- `develop` merged to `main` (brought 30+ commits forward from June 12 base).
- 7 stale remote branches deleted, tracking pruned.

### Eval Aspirational Fixes (3 of 4 → expected)
- **eShop `arch-style`:** `ArchitectureStyleDetector` now has a topology-over-structure rule:
  when Aspire AppHost orchestration is present, Microservices outranks CleanArchitecture.
  eShop correctly reads Microservices (0.91 confidence).
- **eShop `aspire-signal`:** `Aspire.Hosting` was already in the `DependencyExtractor` package
  map — just needed the expectation file flipped to `expected`.
- **VerticalSlice `mediatr-signal`:** `Mediator` (source-gen) was already in the package map —
  flipped to `expected`.
- **VerticalSlice `no-dynamic`:** FastEndpoints routes `<dynamic>` placeholder — still aspirational.

### Map — New Entry Groups + PipelineBehaviors
- **Domain entry group:** MediatR notification handlers shown alongside HTTP routes
  (eShop: 7 handlers, VerticalSlice: 2).
- **Bus entry group:** Code ready; blocked by `MessageConsumerDetection` gap for
  RabbitMQ `IIntegrationEventHandler` pattern.
- **PipelineBehaviors:** `MapBuilder.BuildPipelineBehaviors` scans DI `ImplementationType`
  body text for `AddOpenBehavior(typeof(X))` patterns (MediatR fluent config packs calls
  into one registration). eShop shows: `LoggingBehavior → TransactionBehavior → ValidatorBehavior`.
- **Entry→target:** Route-segment heuristic with HTTP verb prefix matching (POST→Create,
  GET→Get, PUT→Update, DELETE→Delete) + plural→singular normalization. Works for CleanArch
  (GET /products → GetProductsQuery), blocked for eShop (see G1 below).

### Trace — RESULT / NEXT / TOUCHES Sections
- **RESULT:** `200 OK / 201 Created · failure → 400 Bad Request` for HTTP entries.
- **NEXT:** Lifecycle hints from emitted events (initial state → status transition →
  payment processing → fulfillment → cancellation).
- **TOUCHES:** Entity nodes collected from all out-edges of visited nodes + reverse
  Entity→DataStore scan. Noise-filtered (`<OnModelCreating>`, Migration artifacts).
- **EMITS:** Deduped via `Distinct()` in the renderer.

### Stats Page — Card Grid Redesign
- CSS Grid 2-col layout: Timing Waterfall, Extractors, Scorer+Token Funnel,
  Cache+Corpus+Parallelism+Graph.
- Gradient timing bars, hit-rate gauge (large number + bar), speedup badges.
- All Catppuccin Latte colors preserved.

### JSON Strict Mode Fix
- Self-check `budget-respected` used the catalog plan's type-based estimate (642 tokens) instead
  of the user's budget (4000). Now uses `request.MaxTokens` for JSON output.
- All 5 CLI matrix commands exit 0.

### Docs Archive
- `docs/archive/INDEX.md` — retracable manifest of all moved files.
- `docs/reports/` moved to `docs/archive/reports/`.
- `docs/examples/` created with curated outputs from test fixtures + eval runs.

### Sends Detection — Generic Arg Unwrap
- `AddSends` + `AddLambdaOutEdges` now handle `new IdentifiedCommand<CreateOrderCommand,bool>(...)`
  patterns: extended regex `new\s+(\w+)(?:\s*<[^>]+>)?\s*[\(;]` followed by `UnwrapGenericArg`
  to extract the first generic argument as the actual request.

### NodeId Fix
- `AddHttpEntryPoints` fallback path now finds the Type node by scanning `g.Nodes` for
  `Kind == Type` + `FilePath` match instead of reconstructing `NodeId.ForType(ownerType.Id)`.
- `ResolveEntryTarget` follows the entry's Calls edge (same traversal as `TraceBuilder`).

---

## Remaining Issues — Verification Guide

> Use the agent skill `devcontext-eval-audit` for each. Verify with real repos,
> write failing tests first, fix at source, re-capture, ratchet expectations.

### G1 — eShop entry→target (`POST /api/orders/ → CreateOrderCommand`)

**Symptom:** Map shows `POST /api/orders/ (src/...)` without `→ CreateOrderCommand`.
CleanArch fixture works (GET /products → GetProductsQuery). eShop minimal-API
endpoints do not.

**Root cause (diagnosed):** `EnrichEntryTargets` finds the correct Type node via Calls edge,
but `graph.OutEdges(node.Id, EdgeKind.Sends)` returns 0 edges — even though the SAME
code path in `TraceBuilder.Walk` sees the Sends edges. This suggests either:
- A graph freeze-timing issue (the `CodeGraphBuilder.Build()` freezes edges but
  `AddSends` runs before `Build` — confirmed correct order), OR
- Two Type nodes for the same class with different Id keys (one at `global.OrdersApi`,
  another without the `global.` prefix), and the Calls edge lands on the one without
  Sends edges.

**Additional contributing factor:** eShop `EndpointDetection` has `HandlerType = "CreateOrderAsync"`
(the method name) instead of `"OrdersApi"` (the type name). This means the named-handler
path never resolves, forcing the fallback owner-type path. Fixing `EndpointExtractor` to
report the owning type name would let the named-handler path resolve the Member node and
trace its Sends.

**Verification:**
```powershell
dotnet run --project src/DevContext.Cli -- analyze C:\Code\DevContext2\eval-repos\eShop\src\Ordering.API --max-tokens 8000
# Expected: POST /api/orders/  (src/...)  → CreateOrderCommand
# Actual:   POST /api/orders/  (src/...)                 (no →)
```

**Fix approaches (in order):**
1. **Dump the actual graph state** — add debug logging in `EnrichEntryTargets` to print:
   all nodes with title "OrdersApi", their `Id.Key`, and their out-edge count. Compare to
   `AddSends` output (which TypeDiscovery.Id it used). The mismatch will reveal whether
   two nodes exist with different keys.
2. **Normalize `TypeDiscovery.Id`** in `SyntaxStructureExtractor` — strip `global::` prefix
   (see attempt at commit `c03872b` which broke Sends detection; the fix needs to normalize
   CONSISTENTLY across both Id and NameResolver usage).
3. **Fix `EndpointExtractor`** — when `HandlerType` is detected as a method name (not a type
   name), populate `HandlerType` with the containing class name instead. This is the
   detection-level fix that bypasses the graph-level mismatch entirely.
4. **Generic fallback in `ResolveEntryTarget`** — if direct `graph.OutEdges(node.Id, EdgeKind.Sends)`
   returns 0, fall back to scanning ALL graph nodes' Sends edges and matching by route.

### G2 — FastEndpoints `<dynamic>` routes (VerticalSlice)

**Symptom:** VerticalSlice entries show `POST <dynamic>` instead of actual route patterns.
The `no-dynamic` eval check is aspirational.

**Root cause:** FastEndpoints sets routes via `.Get("/x")` inside `Configure()` —
`EndpointExtractor` (or `ControllerActionExtractor`) doesn't detect these.

**Verification:**
```powershell
dotnet run --project src/DevContext.Cli -- analyze C:\Code\DevContext2\eval-repos\VerticalSlice --max-tokens 8000
# Expected: no "<dynamic>" entries
# Actual:   most entries show <dynamic>
```

**Fix approach:** Add a new extractor or extend `EndpointExtractor` to detect FastEndpoints
route registration patterns (`Get("/x")`, `Post("/x")`, `Put("/x")` in class methods that
inherit from `Endpoint<TRequest, TResponse>` or `EndpointWithoutRequest`.

### G3 — MessageConsumer Bus entries
**Symptom:** eShop full-solution shows no "Bus" entry group despite RabbitMQ consumers.

**Root cause:** `MessageConsumerDetection` is not produced for eShop's RabbitMQ pattern
(`IEventBus.Subscribe` with `IIntegrationEventHandler`). The `EventBusExtractor` may
not detect this pattern.

**Verification:**
```powershell
dotnet run --project src/DevContext.Cli -- analyze C:\Code\DevContext2\eval-repos\eShop --max-tokens 8000
# Current: ENTRY POINTS → HTTP (44), Domain (7) — no Bus
```

**Fix approach:** Extend `EventBusExtractor` or `InMemoryEventBusExtractor` to detect
`IIntegrationEventHandler` implementations and create `MessageConsumerDetection`.

### G4 — Trace TOUCHES missing on shallow traces
**Symptom:** Trace at depth 2–3 doesn't show TOUCHES section on simple repos (TodoApi, VerticalSlice).

**Root cause:** Entity nodes are collected from all out-edges of visited nodes. On simple
trace paths (MinimalApi → direct EF), no entity nodes are connected because the
`AddDataEdges` body scan doesn't detect the DbContext→entity link in simple lambda bodies.

**Fix approach:** Fall back to collecting entities from `EfEntityDetection` for any
DbContext type reachable in the trace, regardless of edge presence.

### G5 — Graph freeze timing investigation (diagnostic)
**Symptom:** `EnrichEntryTargets` (called inside `GraphBuilder.Build` after `g.Build()`)
sees 0 Sends edges on a Type node that the `TraceBuilder` (called later from the same
`snapshot.Graph`) can traverse Sends from.

**Investigation command:**
```powershell
# Add temporary Console.WriteLine in EnrichEntryTargets to dump:
# - entry.Title, node.Title, node.Id.Key, graph.OutEdges(node.Id).Length
# And in AddSends to dump:
# - type.Name, type.Id, type.SourceBody?.Length, edges added count
dotnet run --project src/DevContext.Cli -- analyze C:\Code\DevContext2\eval-repos\eShop\src\Ordering.API --max-tokens 4000
```

### G6 — Validation probe (IDEAL-OUTPUT-TARGET.md §7)
**Status:** Designed, not executed. Gates the decision to invest in semantic resolution (P3).

**The probe:**
1. Run the engine on eShop `POST /api/orders/` to produce the real trace.
2. Give a fresh LLM session a realistic task ("add a per-line discount to orders") with:
   (a) the DevContext trace, (b) raw source files, (c) legacy catalog output.
3. Compare answer quality: correctness, navigation speed, completeness, explanation quality.
4. If the trace wins → fund SemanticSymbolResolver (P3) + Persistent Index (P4).
   If not → syntactic precision is insufficient; semantic is prerequisite.
5. Record result in `docs/reports/` and update IDEAL-OUTPUT-TARGET.md §7.

---

## Key file inventory

| File | What changed | Role in remaining gaps |
|------|-------------|----------------------|
| `src/DevContext.Core/Graph/GraphBuilder.cs` | Entry→target + HandlerNode + Sends unwrap + PipelineBehaviors scan + fallback NodeId fix | G1, G5 |
| `src/DevContext.Core/Graph/TraceBuilder.cs` | TOUCHES graph-entities pass + CollectSummaries refactor | G4 |
| `src/DevContext.Core/Graph/MapBuilder.cs` | BuildPipelineBehaviors from DI body scan | G3 (indirectly) |
| `src/DevContext.Core/Graph/EntryPoint.cs` | Added HandlerNode field | G1 |
| `src/DevContext.Core/Rendering/TraceRenderer.cs` | RESULT + NEXT sections, EMITS dedup | — |
| `src/DevContext.Core/Rendering/MapRenderer.cs` | Entry→target via Target(ep), group labels | — |
| `src/DevContext.Core/Rendering/RunReportHtmlRenderer.cs` | Card grid layout rewrite | — |
| `src/DevContext.Core/Extractors/Generic/ArchitectureStyleDetector.cs` | Topology-over-structure Microservices boost | — |
| `src/DevContext.Core/Extractors/Generic/SyntaxStructureExtractor.cs` | Namespace prefix (global::) candidate fix area | G1 |
| `src/DevContext.Core/Pipeline/DiscoveryPipeline.cs` | JSON strict budget fix | — |
| `src/DevContext.Desktop/wwwroot/app.css` | Stats card grid styles | — |
| `src/DevContext.Desktop/ViewModels/MainViewModel.cs` | GraphSummary + render funnel to HTML renderer | — |
| `eval/expectations/eshop.json` | 2 aspirational → expected | — |
| `eval/expectations/verticalslice.json` | 1 aspirational → expected | — |
| `tests/DevContext.Desktop.Tests/MainViewModelTests.cs` | DefaultReport.TotalWall = 200ms | — |

---

## Agent skills — updated for this session

The following `.claude/skills/` have been updated to reflect current state:
- `run-devcontext/SKILL.md` — test counts, new trace features, entry groups
- `devcontext-eval-audit/SKILL.md` — new audit dimensions (Domain/Bus entries, RESULT/NEXT/TOUCHES)
- `devcontext-bench/SKILL.md` — unchanged (perf work not in scope this session)
