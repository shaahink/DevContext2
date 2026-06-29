# DevContext Deep Audit — Executive Summary

> v1.0.5-preview, commit `2c40662`. 2026-06-23.
> Companion documents: `audit-graph-architecture.md`, `audit-caching-system.md`, `audit-claims-vs-delivery.md`

---

## What DevContext Is

A .NET 10 static code analysis tool that reads a .NET solution once, builds an immutable typed `CodeGraph` (nodes + directed edges), and renders it through two lenses:

- **Map** (no `--focus`): Architecture overview — topology, stack, entry points, packages
- **Trace** (with `--focus`): Call-stack traversal from an entry point down the wiring

Output is section-aware markdown/JSON/HTML bounded by a token budget. Three surfaces: CLI (Spectre.Console), Desktop (WPF+BlazorWebView), Library (`DevContext.Core`). No external LLM dependency.

---

## Architecture at a Glance

```
Input (path + focus + options)
  │
  ▼
ProjectRootResolver → IntentResolver → 3 Pipeline Stages
  │                                      │
  │  Stage 1: Discovery (sequential)     FileTree + ProjectStructure
  │  Stage 2: Generic (parallel)         Syntax + DI + Architecture detection
  │  Stage 3: Specific (parallel, 2-wave) Endpoints, MediatR, EF, CallGraph
  │                                      │
  ▼                                      ▼
AnalysisSnapshot (immutable artifact)
  ├─ DiscoveryModel (types, detections, call edges)
  ├─ CodeGraph (nodes: 3 kinds, edges: 8 kinds)
  ├─ MapModel (topology, entries, packages)
  ├─ EntryPoint[] (catalogued entries)
  └─ RunReport (stage timings, cache stats)
  │
  ▼
RenderAsync(snapshot, request) — milliseconds
  ├─ Entry set? → TraceBuilder → TraceRenderer
  └─ No entry? → MapRenderer / LibrarySurfaceRenderer
```

---

## The CodeGraph — Connective Tissue

- **3 node kinds**: `Type`, `Member`, `EntryPoint` — one class = one node, tagged with roles
- **8 edge kinds**: `Calls`, `Sends`, `Handles`, `Raises`, `Consumes`, `ReadsWrites`, `Resolves`, `WrappedBy`
- **3 resolution levels**: `Join` (detection cross-link), `Syntactic` (heuristics, labeled `[approx]`), `Semantic` (Roslyn Symbol, labeled `[verified]`)
- **Traversal**: BFS-like, depth-bounded, fan-out capped, framework leaf stopped, edge-priority ranked

---

## Caching — Three Tiers

| Tier | Scope | Mechanism | Hit Rate (CLI) |
|---|---|---|---|
| `AnalysisCache` | Per-run, CLI | `ConcurrentDictionary` + `Lazy<Task<T>>` | 0% (destroyed after run) |
| `PersistentAnalysisCache` | Cross-run, Desktop | mtime-based invalidation | Near-100% on unchanged files |
| `SharedAnalysisContext` | Per-analysis | `Lazy<Task<FileSyntaxNodes>>` | Shared across extractors in one run |

The Desktop combines `PersistentAnalysisCache` + `AnalysisSnapshot` to achieve **"analyze once, render many"** — focus/depth/detail changes are milliseconds, not seconds.

---

## What Works Well

1. **eShop Trace quality**: Full MediatR lifecycle traced end-to-end (entry → send → handler → raises → domain handler → integration event). TOUCHES correctly lists all 5 entities. EMITS lists all 11 events. RESULT and NEXT sections are accurate.

2. **Detection breadth on DntSite**: 70 HTTP endpoints (was 13), 24 scheduled jobs, 83 DI registrations (was 1), 103 EF Core entities. The detection improvements from the CHANGELOG are real and verified.

3. **Architecture style detection**: Correctly identifies Clean Architecture (eShop, high confidence) and ControllerBased (DntSite, moderate confidence).

4. **Graph seams are informative**: The per-seam edge count table (Calls, Sends, Handles, Raises, etc.) immediately reveals the technology stack — eShop has all 8 seams; DntSite has only 3 (Calls, ReadsWrites, Resolves — correct for a non-CQRS app).

5. **`--include-map` combines both lenses**: The Map orientation stays visible while drilling into a Trace, exactly as designed.

6. **Performance on small repos**: eShop Map = 2.8s, Trace = 2.9s.

---

## What Doesn't Work Well

### Critical

1. **❌ Class-scoped call-edge attribution (root cause across all repos)**: `CallGraphExtractor` resolves edges to the correct file/class but attaches them to **every method in the class** rather than the specific method body containing the call site. This produces false edges, wrong EMITS, inflated TOUCHES, and `[verified]` overclaiming. Diagnosed via eShop Catalog.API where `CreateItem`'s trace inherited `UpdateItem`'s `ProductPriceChangedIntegrationEvent`, `DeleteItemById`'s `CatalogServices` call, and a method signature misidentified as a call. The integration-event subgraph is internally accurate — only the root attachment is wrong.

2. **Entry→target resolution fails for controllers**: 0/94 entries in DntSite show dispatch targets. The resolver only bridges via MediatR detection joins. Controller-based projects get no `→ handler` links.

3. **The classifier has no scope-awareness**: OrchardCore (monolithic modular CMS) → "Microservices, high" (wrong — latched onto project count + Aspire signal). Catalog.API (one slice of eShop) → "MinimalApi" (locally correct but globally misleading). Neither run tells the consumer whether it analyzed the whole system or one slice.

### High

4. **Token budget is both a guillotine AND unused**: Map mode prunes 97% of types but leaves ~2,600 tokens of 8,000 unused. Trace mode (Catalog.API: 2,537 tokens vs 12,000 budget) leaves ~79% of budget on the table **while truncating the trace** with `(truncated — more edges beyond depth/fan-out)`. Fan-out/depth caps fire before the token budget. The budget isn't doing its job of buying depth.

5. **TOUCHES detection misses entities accessed via method calls**: DntSite trace found only `BaseEntity`. EF Core queries are `Calls` edges, not `ReadsWrites` edges, so `CollectGraphEntities` misses them.

6. **Performance on cold runs**: DntSite Map = 41.3s first run (30s in CallGraphExtractor alone). Second run = 10.4s (OS cache).

7. **Trace can exceed budget**: DntSite trace was 9,388 tokens vs 8,000 budget.

### Medium

8. **`[verified]` is still overclaiming**: Confirms "a symbol resolves at this location" but not "this member is invoked from the parent member." As long as edges are class-scoped, `verified` is misleading.

9. **Sends/Raises edges are always `[approx]`**: Body-string scanning only. Roslyn semantic tier gated behind `Debug` profile.

10. **DDD "aggregate" false positives**: Catalog.API (anemic CRUD) → `Aggregates: CatalogBrand · CatalogItem · CatalogType`. Folder/namespace heuristic conflates "EF-mapped class" with "domain aggregate."

11. **Fan-out truncation is aggressive**: The 12-child cap + large service classes = massive information loss.

12. **PatternRelevancePruner is dead code**: 0% pruning in all runs.

### Low

13. **No persistent graph cache**: Designed but deferred.
14. **MainViewModel is 627-line god class**.
15. **IndirectWiringDetector only runs on Trace mode**.
16. **STACK contains MSBuild variable noise** in OrchardCore.
17. **`GET /` appears as entry point** from Scalar/OpenAPI infra.
18. **GET /api/catalog/items appears twice** — possibly real v1/v2, possibly double-count.

---

## Measured Performance

| Run | Wall Time | Stage 2 | Stage 3 | Nodes | Edges | Tokens | Notes |
|---|---|---|---|---|---|---|---|
| eShop Catalog.API Map + Trace (d5) | 1.9s | — | — | 109 | 85 | ~2,537 | 66 files, 5 projects, 19 entries, 2/19→target |
| eShop Ordering.API Map | 2.8s | 209ms | 2,233ms | 193 | 326 | 1,117 | 140 files, 7 projects |
| eShop Ordering.API Trace (d8) | 2.9s | 220ms | 2,120ms | 193 | 319 | 6,402 | |
| OrchardCore Map + Trace (d5) | — | — | — | 6,041 | 1,997 | — | 5,239 files, 206 projects, 295 entries, 1,006 semantic |
| DntSite Map (cold) | 41.5s | 8,218ms | 32,342ms | 1,475 | 1,247 | 2,641 | 1,342 files, call graph: 30,490ms |
| DntSite Trace (d5) | 10.7s | 3,448ms | 6,477ms | 1,475 | 728 | 9,388 | Over budget |
| DntSite Trace+d2+map | 10.7s | 4,574ms | 5,550ms | 1,475 | 297 | 3,094 | |

---

## Recommendations

### Immediate (high impact, low risk)
1. **Fix call-edge attribution to be method-scoped, not class-scoped** — the single highest-value fix; resolves false edges, wrong EMITS, inflated TOUCHES, and `[verified]` overclaiming simultaneously across all repos
2. Fix entry→target resolution for controllers by mapping route templates to controller methods

### Short-term
3. Make fan-out/budget interact: leftover token budget should expand depth and pull source bodies, not get discarded
4. Add `Calls`-edge entity traversal in `CollectGraphEntities` for TOUCHES
5. Add scope-awareness to classifier — stamp style with "(N-project scope)" or refuse to classify whole-system style from a closure
6. Filter build variable noise from STACK output
7. Add "what was pruned" disclosure to output (P2 in DESIGN-PHILOSOPHY.md)
8. Remove or repurpose `PatternRelevancePruner`

### Medium-term
9. Build persistent graph cache (designed, deferred)
10. Cross-run `FileSyntaxNodes` cache to reduce Stage 2 floor
11. Split `MainViewModel` into focused components

### Decisive Test
12. Run `--focus "CatalogApi:UpdateItem"` on Catalog.API. If UpdateItem's trace looks identical to CreateItem's, confirms class-scoped attribution. If they diverge correctly, the bug is narrower.
