# Output Quality Assessment — graph-based shift

> Date: 2026-06-19. Run against `eval-repos/` with the current build
> (`devcontext analyze <repo> [--focus <entry>] --max-tokens 12000`).
> Scores the real output against `docs/IDEAL-OUTPUT-TARGET.md` (the Map + Trace north star).

## The design shift (what we're scoring against)

The pivot moved us from **~15 disconnected catalogs** (endpoints / DI / entities / call-graph
tables, each correct but unconnected) to **two connected artifacts derived from one CodeGraph**:

- **Map** — orientation, *no code*: stack, style, topology, entry points (each `→ its target`),
  cross-cutting, packages. Produced when no focus is given.
- **Trace** — one entry point followed *down the wiring*, bridging indirection seams
  (`call · di · send · pipeline · domain-event · integration-event · data`) with `file:line`
  provenance and salient body lines. Produced when a focus is given.

So **"no call graph in default mode" is by design** — the call stack lives in the *Trace*, not the
Map. The Map intentionally omits bodies. The real question this assessment answers is whether the
two artifacts are *good enough* yet, and where they fall short of the ideal.

## What's working

- **The core Trace seam-bridging works** — the thing the pivot was for. eShop `POST /api/orders/`
  produces: `ENTRY → call OrdersApi → send CreateOrderCommand → handler CreateOrderCommandHandler
  → raises OrderStartedIntegrationEvent + call OrderingIntegrationEventService [verified] + call
  Order [verified]`, with salient body lines and `[verified]`/`[approx]` resolution tags. That is
  the design delivering.
- **Semantic resolution tags** (`[verified]` vs `[approx]`) render and look right.
- **Map skeleton** (STACK / STYLE / TOPOLOGY / ENTRY POINTS / CROSS-CUTTING / PACKAGES) renders for
  all archetypes; aggregates list is good (VerticalSlice).
- **Section-aware narrative** (this branch) — Map/Trace now expose toggleable fragments in both
  desktop views.

---

## Gaps, ranked by impact

### G1 — Multi-project scoping collapses the CQRS/DDD story  · **Critical**
Pointing at a folder/project analyzes **only that project's closure as one scope**. Effects observed:
- **eShop `Ordering.API`** → Map says `unknown (1 project)`, `STYLE MinimalApi`, *"no MediatR"* —
  even though the Trace clearly bridges MediatR `send → handler`. The style/stack signals and the
  cross-service constellation (archetype C: integration-event bus between services) are lost.
- **VerticalSlice** → Map says `(1 project)`, `MediatR with 0 handlers`.
- The Trace can't follow into `Ordering.Domain` / `Ordering.Infrastructure`, so the **`data` seam
  (SaveChanges → tables)** and **`domain-event` seam (Order ctor raises → handler)** never appear —
  the two hops the ideal §2 calls out as the whole point.

Root: `SolutionScope.FromModel` + `ProjectRootResolver` resolve to a single project; signals like
MediatR are read per-scope so they don't light up. This is the #1 thing degrading archetype A.
(Listed as "deferred: multi-project scope" — but it's the dominant quality gap, not a footnote.)

### G2 — Map entry points are thin: no `→ target`, truncated at 10  · **High**  *(your "every endpoint / 20 more")*
- `MapRenderer.AppendEntryPoints` lists `route (file:line)` only. The ideal shows
  `POST api/orders → CreateOrderCommand` — the entry→command/handler mapping that makes the Map a
  launch pad for tracing. The data exists (entry node's `Sends`/`Handles` edge in the graph); it
  just isn't surfaced.
- Groups with >10 entries print 10 then `... and N more` (VerticalSlice: "13 more"; a full eShop
  would hide ~20). For the *primary entry-discovery surface*, hiding entries defeats the purpose.
  Either show all (they're one line each) or make the cap a budget dial, not a hard 10.

### G3 — Library archetype not detected (AutoMapper)  · **High**
AutoMapper → `STYLE NLayer`, test/benchmark projects in topology, **no PUBLIC SURFACE section,
no entries**. Ideal §4 wants a *surface map by capability* (Configure / Execute / Extend /
Register). Archetype detection (app vs library vs constellation) from §5.5 is unimplemented, so the
library case — the one the design explicitly calls "useless today" — is still useless.

### G4 — Duplicate `raises` / `EMITS` edges  · **Medium (bug)**
eShop trace lists `raises OrderStartedIntegrationEvent` **twice** (same event, same `:29`) and
`EMITS OrderStartedIntegrationEvent, OrderStartedIntegrationEvent`. Almost certainly the twin-node
bridging in `TraceBuilder` (following out-edges from both the landed node *and* its Type twin)
double-counting an edge present on both. Needs edge-dedup per step + dedup of `EmittedEvents`.

### G5 — Minimal-API trace body imprecision  · **Medium**
TodoApi `POST /todos/` shows salient body lines from `MapGet("/{id}", …)` and `MapPost("/", …)` —
the *registration method*, not the focused endpoint's handler. All minimal-API endpoints in one
registration method share the owner Type node, so the body slice doesn't match the chosen route.
(Known deferred "per-endpoint precision"; confirmed still present and visible.)

### G6 — Topology rendering rough  · **Medium**
- eShop/AutoMapper topology prints **raw `..\X\X.csproj` paths** instead of project names.
- TodoApi topology is **flat with no `depends-on` edges** (every project shows no deps).
- **Test/benchmark projects** appear in topology (TodoApi.Tests, AutoMapper.UnitTests, Benchmark) —
  `NoiseFilter` filters them from the graph but not from the Map topology.

### G7 — Style/signal inconsistency when scoped  · **Low/Medium**
eShop: Map STACK/STYLE say "no MediatR / MinimalApi" while the Trace uses MediatR handlers. The
architecture *signal* isn't lit even though `MediatRHandlerDetection` exists — Map and Trace read
different sources of truth for "is there MediatR." (Same root as G1.) VerticalSlice uses the
`Mediator` source-generator library → "MediatR with 0 handlers".

### G8 — Narrative stats line is meaningless  · **Low**
Every Map/Trace footer says `… 0 types kept of 0 …`. The type-funnel doesn't apply to the graph
artifacts; it should report **nodes / edges / entries / trace depth**, not "types kept."

### G9 — PACKAGES verbose  · **Low**
Long comma lists (TodoApi/VerticalSlice). Fine, but low signal vs the space it takes.

---

## Bugs you flagged

### B1 — GitHub re-clones on every option change  · **confirmed bug**
`GitCloneService` has a 24h in-memory cache keyed `owner/repo-branch`, and `RepoUrl.ClonePath` is
deterministic — so the cache *should* make re-analysis instant. But `MainViewModel.AnalyzeAsync`
deletes the clone after each run when `CloneCleanup == "auto"` (the default). The cache entry then
fails its `Directory.Exists(path)` check on the next run → **re-clone every time you change a focus
/ option**. The auto-cleanup defeats the cache.

Fix direction (small): in the interactive desktop, don't auto-delete between re-analyses — keep the
clone for the session (the 24h cache already handles reuse) and clean up on app close, on repo
change, or by TTL. The one-shot CLI can keep deleting. Then "bake caching into the engine" (your
note: persistent on-disk analysis cache keyed by content) is the larger follow-up once scoping
(G1) is solid.

### B2 — "No call stack in default mode"  · **by design, but Map is too thin**
Working as intended (call stack = Trace, not Map). The real fix is G2: make the Map's ENTRY POINTS
show `→ command/handler` so the default view conveys wiring at a glance, and let users one-click a
trace from there.

### B3 — "Doesn't show every endpoint"  · = G2 (truncation at 10).

---

## Recommended order

1. **G1 multi-project scope** — biggest quality lever; unlocks data/domain-event seams + correct
   style/MediatR signals for the flagship archetype. Everything else is partly downstream of this.
2. **G2 Map entries `→ target` + de-truncate** — cheap, high-visibility, fixes your endpoint
   complaint and makes the Map a real launch pad.
3. **B1 clone caching** — small, removes the most annoying desktop friction.
4. **G4 raises/EMITS dedup** + **G6 topology names/no-test/edges** — small correctness/polish.
5. **G3 library archetype** — larger; needed before AutoMapper-class repos are useful.
6. **G5 minimal-API per-endpoint precision**, **G8 stats**, **G7 signals**, **G9 packages** — polish.

## Method note
Outputs captured to `%TEMP%/dc-assess/*.txt`. Repos: TodoApi (B), VerticalSlice (A/vertical),
eShop/Ordering.API (A), AutoMapper (D). Re-run with the one-liner at the top to refresh.
