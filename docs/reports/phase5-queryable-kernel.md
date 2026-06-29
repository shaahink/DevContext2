# Phase 5 Capability Report — Queryable Kernel

> 2026-06-28, Iteration 5 / Phase 5 (branch `feature/iter5-queryable-kernel`, commit `1fe7576`).

## Delivered: inverse edges + query API (req 3 + 4-v1)

### Inverse edges (CodeGraph)
The `CodeGraph` ctor derives a reverse adjacency (`_inEdges`) from `_outEdges` — kept derived, not
serialized, so the graph stays serialization-clean (Phase 9 disk index is additive). `InEdges(id, kind?)`
powers `neighbors(in)` and `find_usages(id)` in O(degree), not a full scan.

### GraphQuery facade
`GraphQuery` over `(graph, entries, map)` — the query layer thin enough that the CLI, browse UI, and MCP
server are all clients of these operations:

| Operation | Returns | Built on |
|---|---|---|
| `EntryPoints(kind?)` | `ImmutableArray<EntryPoint>` | snapshot.Entries |
| `Trace(focus, depth, fanOut)` | `Trace?` | EntryPointResolver + TraceBuilder |
| `Map()` | `MapModel?` | snapshot.Map |
| `Stats()` | seams + target-coverage | GraphStats |
| `Node(id)` | `NodeDetail?` (title, tags, file, both degrees) | graph.Node + InEdges |
| `Neighbors(id, direction, kind?)` | `ImmutableArray<EdgeRef>` | OutEdges / InEdges |
| `FindUsages(id)` | `ImmutableArray<EdgeRef>` | InEdges |
| `ResolveNodeId(name)` | `NodeId?` | graph.Nodes search (Member/Type/EntryPoint) |

Supporting records: `NodeDetail`, `EdgeRef`, `EdgeDirection { Out, In }`. JSON-friendly; no rendering
concerns.

### CLI render path re-expressed
`DiscoveryPipeline.RenderAsync` now uses `GraphQuery.Stats()` and `GraphQuery.Trace()` instead of the
inline calls. Output is **byte-identical** — guarded by 8 golden tests, 18 eval tests, and the CLI
`--strict` matrix, all green.

### Unit tests (`GraphQueryTests`)
7 tests covering both edge directions, `FindUsages` (2 callers), `Neighbors(out/in)`, `Node` degrees,
`ResolveNodeId`, and `EntryPoints` filtering. All pass.

## Deferred: DntSite TOUCHES (split out)

### Diagnosis (corrected from the Phase-4 "FQN canonicalization" hypothesis)
The DntSite JSON `detections` array was parsed to extract `EfEntityDetection` entity types:

- **34 distinct entity types** returned by `EfCoreExtractor` on DntSite.
- Of those: `<OnModelCreating>` (1), `BaseEntity` (1), and **27 migration class names** (`V2024_04_19_1424`, `V2024_05_18_1347`, …).
- **Zero real business entities** detected: no `BlogPost`, `DailyNewsItem`, `DailyNewsScreenshot`, `Advertisement`, `Survey`, `News`, or any of the types the `GET /Feed/News` trace method-calls.

**Root cause:** DntSite registers entities via a custom `RegisterAllDerivedEntities` reflection pattern
(iterates `Assembly.GetTypes()`, registering types that inherit from `BaseEntity`). The
`EfCoreExtractor`'s syntax-tree walker detects the `RegisterAllDerivedEntities` method call but **cannot
see the individual types** — it only catches generic `Entity<T>()` calls and the migration classes. The
business entities themselves are never represented as `EfEntityDetection` objects → no entity Type nodes
are created for them → `ResolveEntityNode` (Iteration 3) has no entity-tagged nodes to find → empty
`TOUCHES`.

### Next step (separate follow-up)
Enhance `EfCoreExtractor` to resolve reflection-based entity registrations — either by inferring types
from the `BaseEntity` hierarchy in `model.Types`, or by walking the `GetTypes()`/`Assembly` reflection
calls. This is a detection enhancement, not a trace or query-API issue. Tracked for a future iteration
(appropriate for Iteration 6 / detection maturity).

## Gate
`pwsh -File eval/gates.ps1` **PASS** — 18 eval, fast tests + 7 GraphQueryTests, CLI matrix.

## Next
Iteration 6 — performance & caching. The query layer is ready for the faces (browse UI Phase 7, MCP Phase 8).
