# Universal Lens — Phased Execution Roadmap

> Ordered, agent-executable plan to turn DevContext into the universal go-to lens for **any** .NET
> repo (`docs/product/PRODUCT-DIRECTION.md` is the north star). Each phase is one iteration: a clear goal,
> the audit issues/requirements it closes, key changes, key files, and a **gate** verified against the
> `eval-repos/`. Phases are ordered by dependency — finish a phase's gate before starting the next.
> Run `eval/gates.ps1` (build + tests + eval) at every phase boundary.
>
> **Detailed per-phase iteration guides + live resumable progress: [`README.md`](./README.md).**
> This roadmap is the map; the iteration guides are turn-by-turn directions; the README is the live
> status a fresh session reads first.
>
> Decisions baked in (2026-06-28): token budgeting is **out of the kernel** (deferred, §8 of the
> product doc); correctness leads coverage; human browse UI is the first face, MCP designed-in.
> Blazor acceptance repo = `eval-repos/blazor-samples/9.0/BlazorWebAppMovies`.

## Throughline

Every phase serves one promise: **point it at any .NET repo and get a correct, honest, useful
result in seconds.** "Any repo" = the entry-point coverage ladder (controllers first). "Correct +
honest" = no fabricated edges, scope-aware Map. "Useful in seconds" = queryable kernel + caching.
Two faces (browse UI, MCP) are thin clients over one queryable graph — built after the graph is
trustworthy.

## Audit issue → phase map (nothing dropped)

| Audit issue | Phase |
|---|---|
| Critical 1 — class-scoped call-edge attribution | **1** |
| Critical 2 — entry→target fails for controllers (0/94) | **2** |
| Critical 3 — classifier has no scope-awareness | **4** |
| High 4 — token budget guillotine & unused | **0** (removed from kernel) |
| High 5 — TOUCHES misses entities via method calls | **3** |
| High 6 — cold-run performance (41s) | **6** |
| High 7 — trace exceeds budget | **0** (moot once budget out; structural bound) |
| Medium 8 — `[verified]` overclaiming | **1** (fixed by member-origin) |
| Medium 9 — Sends/Raises always `[approx]` (no semantic) | **3** (note) / future semantic tier |
| Medium 10 — DDD aggregate false positives | **4** |
| Medium 11 — fan-out truncation too aggressive | **3** |
| Medium 12 — PatternRelevancePruner dead code | **0** |
| Low 13 — no persistent graph cache | **9** |
| Low 14 — MainViewModel god class | **7** |
| Low 15 — IndirectWiringDetector only on Trace mode | **3** |
| Low 16 — STACK MSBuild-variable noise | **4** |
| Low 17 — `GET /` from Scalar/OpenAPI infra as entry | **2** |
| Low 18 — `GET /api/catalog/items` appears twice | **2** |

---

## Phase 0 — Kernel hygiene (clear the distortions)

**Goal.** Make structural bounds (depth · fan-out · framework-stop) the *sole* governor of trace
size, and remove dead machinery, so later phases build on an honest kernel.

**Changes.**
- Remove token-budget logic from graph building / pruning paths. The budget must not influence
  edge selection, node pruning, or graph assembly. If any budget code is kept, it is dormant and
  render-time only (re-introduced later per product doc §8). Locate `TokenBudgetEnforcer`,
  `ScenarioBudget`, any `MaxSurvivingTypes` enforcement.
- Delete `PatternRelevancePruner` (0% effect in all runs — dead).
- Confirm trace size is bounded by `TraceOptions` (depth/fan-out) + framework-leaf stop only.

**Key files.** `TokenBudgetEnforcer`, pruning pipeline, `PatternRelevancePruner`, `TraceBuilder`,
scenario/pruning config. (Agent: grep for `TokenBudget`, `MaxSurviving`, `PatternRelevance`.)

**Gate.** Build green, tests pass (update/remove budget tests). On DntSite + OrchardCore, Map/Trace
output stays bounded by depth/fan-out (no unbounded blow-up) and no longer reports "over budget" or
budget-driven truncation. No other behavior change.

---

## Phase 1 — Correct traces: member-origin edges (the trust foundation)

**Goal.** A trace anchored on a method shows **only that method's** wiring. Kills fabricated edges,
wrong EMITS/TOUCHES, and `[verified]` overclaiming. (Closes Critical 1, Medium 8.)

**Root cause (confirmed live).** Call/raise/send/data edges originate from the **Type** node
(one-node-per-class collapse), so `TraceBuilder.OutEdgesWithTwin` makes any Member inherit every
sibling method's edges. The extractor already captures `CallerMethod`/`CalleeMethod`
(`CallGraphExtractor.cs:188`) — the graph throws it away.

**Changes.**
- `GraphBuilder.AddCallEdges`: emit `Member:Caller.Method → Member:Callee.Method` (create Member
  nodes carrying the declaring file); keep a Member→owning-Type containment for Map/identity.
- `GraphBuilder.AddRaises` / `AddSends` / `AddDataEdges`: attribute each body-scan match to its
  **enclosing method**, not the whole `type.SourceBody`. Preferred: move these scans into
  `CallGraphExtractor`'s per-member Roslyn loop (unifies the duplicated body-scan seam).
- `TraceBuilder.OutEdgesWithTwin`: replace "inherit ALL type edges" with a **controlled member
  bridge** — arriving at a handler Type via `Handles` expands only its `Handle`/`HandleAsync`
  member; arriving via a Calls edge lands on the specific callee Member.
- Entry/focus resolution: anchor `Type:Method` focus on the **Member** node (today it resolves to
  the Type — `▸ ENTRY CatalogApi`). Touch `GetStartKeys`, `FocusPointParser`, `EntryPointResolver`.
- Redefine `[verified]` = "invoked from the parent member's body."

**Key files.** `Graph/GraphBuilder.cs`, `Graph/TraceBuilder.cs`, `Graph/CodeGraph.cs`,
`Extractors/Specific/CallGraphExtractor.cs`, `Graph/EntryPointResolver.cs`,
`Resolvers/FocusPointParser.cs`, graph tests.

**Gate (decisive test).**
- `--focus "CatalogApi:CreateItem"` ≠ `--focus "CatalogApi:UpdateItem"` on Catalog.API (they diverge
  correctly; entry header names the method).
- `POST /api/orders` trace shows **no** `send CancelOrderCommand`/`ShipOrderCommand` under
  `CreateOrderAsync`, and `Order` no longer "raises" Shipped/Cancelled from the ctor path.
- Graph tests updated; `eval/gates.ps1` passes (ratchet eShop/VerticalSlice expectations).

---

## Phase 2 — Universal entries, controllers first (the common case)

**Goal.** Every common entry shape resolves to its dispatch target. Controllers go from **0/94** to
mostly resolved. (Closes Critical 2, Low 17, Low 18.)

**Changes.**
- Controller route→action resolution: map `[Route]`/`[HttpGet]`/attribute-routed controller actions
  to their action **method** (a Member node), independent of MediatR joins. Builds on Phase 1 so the
  action's trace is correctly scoped.
- Entry noise filter: drop infrastructure pseudo-entries (`GET /` from Scalar/OpenAPI, ServiceDefaults).
- Entry dedup: collapse true duplicates; keep genuine versioned overloads (v1/v2) distinct and labelled.

**Key files.** `Extractors/Specific/EndpointExtractor` (controllers), `Graph/GraphBuilder.cs`
(`AddHttpEntryPoints`, `ResolveEntryTarget`), `Graph/EntryPointResolver.cs`, entry noise filter.

**Gate.** DntSite: majority of 94 entries show a `→ target`; a controller-action trace is correct and
scoped to that action (verify two sibling actions diverge). No `GET /` infra entry. `eval/gates.ps1`
ratchets DntSite entry→target from 0.

---

## Phase 3 — Complete & honest traces

**Goal.** The trace surfaces the *whole* relevant path and is honest about what it cut. (Closes High 5,
Medium 11, Low 15; addresses the probe's "missed the real domain-event path" finding.)

**Changes.**
- TOUCHES via `Calls`-edge entity traversal (EF access is a call, not a `ReadsWrites` edge) so entity
  collection isn't limited to direct data edges.
- Follow `Consumes` + pipeline (`WrappedBy`) seams from the right nodes so domain-event handlers and
  cross-cutting behaviors (e.g. `TransactionBehavior`) appear — the path the probe agent had to find
  by hand.
- Fan-out ranking + explicit "what was cut" (`stopped at depth N; K branches omitted`) instead of
  silent aggressive truncation; raise/relax the 12-child cap with structural ranking.
- Run `IndirectWiringDetector` regardless of mode (Low 15).
- *Note (Medium 9):* Sends/Raises remain `[approx]` (body-scan). A later **semantic resolution tier**
  (Roslyn `SemanticModel`, the old P3) upgrades these — schedule after Phase 5; not required here.

**Key files.** `Graph/TraceBuilder.cs` (`CollectGraphEntities`, `Walk`, ranking), `Graph/GraphBuilder.cs`
(seam following), `IndirectWiringDetector`.

**Gate.** DntSite `GET /Feed` TOUCHES lists more than `BaseEntity`. `POST /api/orders` surfaces the
`OrderStartedDomainEvent → ValidateOrAddBuyer…` path. **Re-run the probe** (D vs C) on the fixed
trace + a controller repo + a 2nd task; target: trace reduces a tool-using agent's cost (move from
"primer" toward "accelerator"). Record in `docs/dev/reports/`.

---

## Phase 4 — Honest Map & detection (trust on any repo)

**Goal.** The Map never lies about scope or style. (Closes Critical 3, Medium 10, Low 16.)

**Changes.**
- Scope-awareness: stamp the verdict with what was analyzed — `STYLE … (5-project closure)` — and
  refuse whole-system style claims from a single-service closure. OrchardCore monolith must not read
  "Microservices, high."
- STACK noise filter: strip MSBuild variables (`$(CommonTargetFrameworks)` etc.).
- Tighten DDD aggregate detection so anemic CRUD (Catalog `CatalogItem/Brand/Type`) isn't tagged
  `aggregate` on a folder/namespace heuristic.

**Key files.** `Graph/ArchetypeDetector.cs`, `ArchitectureStyleDetector`, `Graph/SolutionScope.cs`,
`Graph/MapBuilder.cs` (STACK), entity/aggregate detection.

**Gate.** OrchardCore not "Microservices"; Catalog.API stamped as partial-scope; no `$(Var)` in STACK;
Catalog CRUD types not labelled aggregates.

---

## Phase 5 — Queryable kernel (the substrate both faces sit on)

**Goal.** Turn the kernel into a query layer: **analyze once, query many.** (Product doc §6 req 3+4 v1.)

**Changes.**
- Inverse edges in `CodeGraph` (store in-edges) → `neighbors(id, direction)` and `find_usages(id)`.
- A clean query API over one cached `AnalysisHandle`: `entrypoints · trace · map · stats · node ·
  neighbors · find_usages` (product doc §6 sketch). Re-express the CLI over it (no behavior change) as
  the smoke test.
- Keep `CodeGraph` serialization-clean (it already is) so Phase 9's disk index is additive.

**Key files.** `Graph/CodeGraph.cs` (in-edges), new query-service over `AnalysisSnapshot`,
`DiscoveryPipeline.RenderAsync`, CLI command surface.

**Gate.** Query API unit-tested (both edge directions correct); CLI output unchanged through the new
layer; `find_usages`/`neighbors` verified on eShop + DntSite.

---

## Phase 6 — Performance & caching ("just run it")

**Goal.** Warm runs in seconds; cold runs tolerable. (Closes High 6.)

**Changes.**
- Attack `CallGraphExtractor` cold cost (30s on DntSite): reuse the focus-scoped binding, cross-run
  `FileSyntaxNodes` cache, avoid redundant compilation.
- Lean on `PersistentAnalysisCache` for cross-run reuse.

**Key files.** `Extractors/Specific/CallGraphExtractor.cs`, `SharedAnalysisContext`,
`PersistentAnalysisCache`, caching layer.

**Gate.** DntSite warm < a few seconds; cold materially under 41s; no correctness regression
(re-run Phase 1–4 gates). Use the `devcontext-bench` skill.

---

## Phase 7 — Browse UI redo (human face, first)

**Goal.** A thin interactive client over the query API: navigate entries → topology → trace → node
detail → stats; analyze once, re-query on interaction. (Closes Low 14.)

**Changes.**
- Rebuild the desktop surface as a thin view over Phase 5's query API.
- Split the 627-line `MainViewModel` god class into focused components.

**Key files.** `DevContext.Desktop/*` (`MainViewModel`, `AnalysisService`, `OutputPanel`, config panel).

**Gate.** From a fresh analyze, user navigates all five artifacts with sub-second re-queries on
focus/depth/detail changes; no re-analysis on navigation.

---

## Phase 8 — MCP server (agent face)

**Goal.** Expose the query API as MCP tools so an agent calls DevContext for what it can't compute.

**Changes.**
- MCP tools: `list_entrypoints`, `trace`, `map`, `stats`, `get_node`, `find_usages`, over a stable
  JSON serialization of the graph.

**Key files.** new `DevContext.Mcp` (or equivalent) over the Phase 5 query service.

**Gate.** An agent session resolves a real task by calling the tools (re-run the probe as `C + MCP`);
JSON contract stable + documented.

---

## Phase 9 — Persistent index + GitHub-URL path (scale + discovery)

**Goal.** Instant re-opens; "paste any GitHub URL, get a map." (Closes Low 13; product doc §6 req 4 v2.)

**Changes.**
- Content-keyed persistent graph index (the deferred P4) — additive on the serialization-clean graph.
- Fetch/clone path for GitHub URLs feeding the same pipeline.

**Key files.** new disk-index layer, `CodeGraph` (de)serialization, repo-fetch path,
`ProjectRootResolver`.

**Gate.** Unchanged repo re-opens near-instant; a public GitHub .NET repo URL → Map in reasonable
time, with honest scope.

---

## Phase 10 — Coverage ladder rungs (interleave once kernel is trustworthy)

**Goal.** Widen "any .NET repo" beyond controllers/minimal-API/MediatR.

**Changes (each its own slice, with negative tests + honest covered/not-covered signal).**
- Hosted services / background workers / scheduled jobs (already detected — surface fully).
- gRPC services, SignalR hubs.
- Blazor components (lifecycle + event handlers) — acceptance repo `blazor-samples/9.0/BlazorWebAppMovies`.
- Azure Functions / Lambda triggers; console `Main`.

**Gate.** Each rung: entries resolve on a representative repo; the "ripgrep test" (product doc §9)
passes for that shape.

---

## Acceptance — the "ripgrep test" (run continuously)

Zero-config, seconds (warm), genuinely useful on each shape:
controller (DntSite) · minimal API (TodoApi) · Blazor (`blazor-samples/9.0/BlazorWebAppMovies`) ·
CQRS/DDD (eShop Ordering.API) · library (AutoMapper). Plus the re-probe gate from Phase 3/8.
