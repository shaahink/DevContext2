# Iteration 5 — Queryable kernel: inverse edges + query API (Phase 5)

> **Status:** IN PROGRESS (branch `feature/iter5-queryable-kernel`) · **Phase(s):** 5 · **Prerequisite:** Iteration 4 DONE
> (honest Map: scope-stamp, aggregates, STACK clean; gate green).
> **Fresh session? Start at [`README.md`](./README.md).** Required reading:
> `docs/PRODUCT-DIRECTION.md` §6 (the query-API sketch) + §7 (faces' data-needs),
> `docs/plans/UNIVERSAL-LENS-ROADMAP.md` (Phase 5), `docs/ACCEPTANCE.md`.
>
> **Why this builds on Iterations 1–4.** The graph is now correct (member-origin), covered (controllers),
> complete (domain-event chain), and honest (scope/aggregates). It is trustworthy — so now make it a
> **query layer**: *analyze once, query many*. Two faces (browse UI, MCP) are thin clients over one
> queryable graph; build the query layer once and the faces become thin. Today the graph stores only
> out-edges, so "who calls this" needs a full scan — Phase 5 adds inverse edges and a clean query API.

**Goal.** Turn the kernel into a query layer. (Product doc §6 req 3 + 4-v1.) Plus **fold in** the deferred
DntSite entity-FQN canonicalization so `TOUCHES` is non-empty on DntSite.

---

## What already exists (don't rebuild it)
- **`AnalysisSnapshot`** IS the "analyze-once handle": immutable, holds `Graph` (CodeGraph), `Map`
  (MapModel), `Entries` (EntryPoint[]), `Model`, `Report`, `RootPath`. Produced by
  `DiscoveryPipeline.AnalyzeAsync`. Both faces already do `AnalyzeAsync` → `RenderAsync(snapshot, request)`
  (CLI `AnalyzeCommand`, Desktop `IAnalysisService`). So analyze-once/query-many is structurally present;
  Phase 5 formalises the **query operations** over the snapshot and adds inverse edges.
- **`CodeGraph`** is already immutable + serialization-clean (`FrozenDictionary` of nodes + out-edges).
  It exposes `Node(id)`, `Nodes`, `OutEdges(id, kind?)`, `Contains(id)`. **It has no in-edges.**

## Step 0 — Baselines (measure before changing)
- Capture current CLI output for eShop Ordering.API + DntSite Map/Trace (the "no behavior change" baseline
  the golden/eval suite already encodes — note any extra goldens needed).
- **DntSite FQN diagnosis (deferred item):** dump, for the `GET /Feed/News` chain, (a) the entity Type
  node ids created by `AddEntityNodes` (via `names.Resolve(EntityType)`), and (b) the call-graph callee
  node ids for the same types (e.g. `BlogPost`) from `AddCallEdges` (via `names.Resolve(ce.CalleeType)`).
  Confirm whether they differ (short-name vs FQN, or two FQNs for one short name). That mismatch is why
  `TraceBuilder.ResolveEntityNode` can't tag the callee's owning Type → empty `TOUCHES`.

## Step 1 — Inverse edges in `CodeGraph` (req 3)
- In the `CodeGraph` constructor, derive a reverse adjacency from `_outEdges` (group by `edge.To`) into a
  `FrozenDictionary<NodeId, ImmutableArray<GraphEdge>> _inEdges`. Derived → no extra serialization; rebuilt
  on construct, so the graph stays serialization-clean (Phase 9 disk index unaffected).
- Add `public ImmutableArray<GraphEdge> InEdges(NodeId id, EdgeKind? kind = null)`.
- Unit-test both directions on a small graph: `OutEdges(A)` and `InEdges(B)` are consistent for edge A→B.

## Step 2 — The query API (req 4-v1)
- Add a `GraphQuery` (or `AnalysisQuery`) facade in `DevContext.Core` over an `AnalysisSnapshot` (or
  `CodeGraph` + `Entries`). Implement the product-doc §6 operations, reusing existing builders:
  - `EntryPoints(filter?)` → `snapshot.Entries` (group/filter by `EntryPointKind`).
  - `Trace(entry, depth, detail)` → `new TraceBuilder(graph).Build(...)` (thin wrapper).
  - `Map(facet?)` → `snapshot.Map`.
  - `Stats()` → `GraphStats.Compute(graph, entries)`.
  - `Node(id)` → a `NodeDetail` (title, kind, tags, file:line, declaration) from `graph.Node(id)`.
  - `Neighbors(id, direction)` → `graph.OutEdges(id)` / `graph.InEdges(id)`.
  - `FindUsages(id)` → `graph.InEdges(id)` (who references this node).
- Keep result types lightweight + serialization-friendly (reuse `EntryPoint`, `Trace`, `MapModel`,
  `SeamStat`; add small `NodeDetail`/`EdgeRef` records). JSON is a first-class serialization (MCP later).

## Step 3 — Re-express the CLI/render path over the query API (no behavior change)
- Route `DiscoveryPipeline.RenderAsync`'s Map/Trace/stats/entries through the `GraphQuery` operations so
  the query layer is the single path the faces use. **Output must stay byte-identical** — the golden tests
  + eval + CLI matrix are the guard.
- Optionally add a minimal CLI smoke surface for the new ops (`--neighbors <id>`, `--usages <id>`,
  `--node <id>`) — small, behind existing flags — so `neighbors`/`find_usages` have a CLI smoke path. If it
  bloats the CLI, keep them query-API-only (unit-tested) and defer the CLI surface to the browse UI.

## Step 4 — Fold in: DntSite entity-FQN canonicalization (the deferred TOUCHES gap)
- Root cause (Iter 4 diagnosis): `EfEntityDetection` entity nodes and call-graph callee nodes for the same
  type resolve to **different node ids** (FQN canonicalization mismatch). 115 entities are detected but
  `ResolveEntityNode` can't match the call-edge callee's owning Type to the tagged entity node.
- Fix at the resolution boundary so both paths agree on one canonical FQN:
  - Make `NameResolver` the single source of truth used by BOTH `AddEntityNodes`/`AddDataEdges` AND the
    call-graph node creation (`AddCallEdges` already uses `names.Resolve`; confirm `CallGraphExtractor`'s
    emitted `CalleeType` FQN round-trips through `names.Resolve` to the same key the entity node uses).
  - Where a short name maps to one declared type, both paths must produce that type's `Id`. Where the
    call graph emits an FQN that `names.Resolve` passes through unchanged but the entity node used a
    different FQN form, canonicalize (e.g. resolve via short name when the FQN isn't a known declared id).
- **Verify:** DntSite `GET /Feed/News` `TOUCHES` is non-empty (lists `BlogPost`/`DailyNewsItem`/…) and the
  inverse-edge `find_usages` on an entity returns the methods that touch it.
- **Guard against regression:** re-run Iteration 1–4 gates; the canonicalization must not merge distinct
  types or drop the eShop/controller assertions (Order/Buyer/CreateOrderCommandHandler keys unchanged).

## Harness updates (authorized)
- **Query-API unit tests** (the gate): both edge directions (`OutEdges`/`InEdges` consistency);
  `find_usages` returns correct callers on a synthetic graph; `Neighbors(in/out)` correct; `Node` detail
  shape. Plus a real-repo check: `find_usages`/`neighbors` on eShop + DntSite return non-empty, correct edges.
- **No-behavior-change guard:** existing golden tests + `eval/gates.ps1` (CLI matrix, eval JSON,
  TraceQualityTests) stay green through the query-layer re-express.
- **DntSite TOUCHES:** add a `TraceQualityTests` (or eval) assertion that DntSite `GET /Feed/News` `TOUCHES`
  is non-empty (skip-if-absent, like other DntSite checks).
- Flip `docs/ACCEPTANCE.md` Phase-5 + the Browse row's query-API checks to `expected`.

## Gate (Phase 5 done when)
- Query API unit-tested, both edge directions correct; `find_usages`/`neighbors` verified on eShop + DntSite.
- CLI output unchanged through the new layer (golden + eval + CLI matrix green).
- DntSite `GET /Feed/News` `TOUCHES` non-empty (folded FQN fix).
- `pwsh -File eval/gates.ps1` PASS incl. the new query-API tests.

## Pitfalls
- **Don't break "no behavior change":** the CLI re-express must be a pure refactor — route through the
  query API without changing rendered bytes. The goldens catch any drift.
- **In-edges are derived, not stored:** rebuild them in the `CodeGraph` ctor from out-edges; do not add a
  serialized field (keeps Phase 9 additive).
- **FQN canonicalization is high-blast-radius:** it underpins every graph join. Diagnose precisely, fix at
  one boundary (NameResolver), and re-run all prior gates. If a low-risk fix isn't clear, keep the query
  API + inverse edges as the Phase-5 deliverable and split the FQN fix into its own small PR.
- **Query API is the substrate for Phases 7 (UI) + 8 (MCP):** keep it face-agnostic and JSON-serializable;
  don't leak rendering concerns into it.

## Definition of done → next
Gate green; query API + inverse edges shipped and tested; CLI unchanged; DntSite TOUCHES fixed (or split
out with a tracked note); update Status above + the table in `README.md`; record the commit.
**Next: Iteration 6 (Phase 6 — performance & caching).**
