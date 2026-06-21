# DevContext performance — summary (perf/profile-and-optimize)

Profile-first work: measure the real pipeline over the standing eval repos, find the hot path
empirically, fix one lever at a time, re-benchmark, keep tests green. Harness: `benchmarks` `repos`
mode (median of 3 after 1 warm-up, reusing the production composition root). Raw runs in this folder.

## Iteration 8 (P5 + determinism) — latest

Stage-2 parse/walk parallelized (output-preserving two-phase: parallel parse → deterministic serial
commit). Latest full run: `PERF-2026-06-21-0048.md`.

| Repo | Mode | iter-7 after | iter-8 after | Stage2 |
|---|---|--:|--:|--:|
| DntSite | Map | 7.7 s | **3.5 s** | 6030 → **2013 ms** |
| DntSite | Trace | 10.1 s | **4.1 s** | bind 1816 → 1245 ms |
| OrchardCore | Map | 5.7 s | **3.0 s** | 3802 → **1559 ms** |
| AutoMapper | Map | 2.0 s | **0.9 s** | 1872 → **806 ms** |

Also fixed a **pre-existing** focus-scoped-trace nondeterminism (`CallGraphExtractor` iterated
`model.Types`/`model.Detections`, both concurrent collections with nondeterministic enumeration, to build
first/last-wins resolution maps) — eShop `POST /api/orders/` edges wandered 159/91; now a fresh-process
CLI trace is byte-identical and always lands on the complete 159-edge chain. P4 (trim metadata refs) was
measured and **skipped** (static `Lazy`, already excluded from the post-warmup bench median). Caveat: the
macro bench reuses one pipeline across repos, so its eShop-trace edge cell is harness noise — production
is deterministic.

---

## Iteration 7 baseline (below)

## Headline (DntSite, 1336 files — the representative repo)

| Stage | Baseline (`f31a2a2`) | After (P2+P1+IndirectWiring+seam-fix) | Δ |
|---|--:|--:|---|
| **CallGraph bind** (Trace) | **84,306 ms** | **~1,800 ms** | **−98%** (machine-independent) |
| Trace total wall | 53.9 s | ~5–10 s* | ≈ −80–90% |
| Map total wall | 7.8 s | ~5–8 s* | parse-bound floor |

*Total wall is now dominated by the Stage-2 parse/structure floor (`SyntaxStructureExtractor` +
`DiRegistrationExtractor`), which is machine-variable; the **bind** delta is the stable, isolated win.

## What changed (each committed + benchmarked, no detection/golden regressions)

1. **Phase 0 — `f31a2a2`**: real-repo macro benchmark runner + `CallGraphExtractor` parse/compile/bind/bfs
   phase timers. Empirically pinned the hot path: **bind** = ~all of CallGraph (parse/compile/bfs ≈ 0).
2. **P2 — `7fb9a5b`**: parallelize the CallGraph semantic-bind loop (`Parallel.ForEach`, concurrent
   `GetSemanticModel`). Trace 53.9 s → 22.7 s.
3. **P1 — `a63f4cc`**: focus-scoped binding — bind only the focus-reachable file frontier (full-bind
   fallback when unresolved). Trace 22.7 s → 10.3 s; bind 18.7 s → 2.8 s.
4. **IndirectWiringDetector — `428a2f1`**: O(invocations×methods) per-file scan → O(depth) ancestor
   walk + parallel; dropped a ~6.5 s secondary cost out of the top extractors.
5. **P1 correctness fix — `64a80f0`**: focus-scoping expanded only by CALL edges, silently truncating
   traces that cross a **Send→Handler seam** (eShop `POST /api/orders/` lost the deep `IntegrationEventLogEF`
   chain: 5 hops). Now seeds the closure with seam-landing files (MediatR handlers, scheduled jobs).
   Verified eShop **5 → 23 hops** (graph edges 159 restored); DntSite unchanged (15 hops).

## Correctness gate (every step)

`dotnet test` Core **269 pass / 2 skip** · Desktop **64 pass** · build 0-warn. Trace content verified
identical where graphs were unchanged (DntSite 15 hops; eShop edges 159).

## What's left (deferred, by design)

- **P5 — Stage-2 parallelism** (`SyntaxStructureExtractor` / `DiRegistrationExtractor`): now the wall
  floor (~3.5–6 ms… s on large repos). Deferred as higher-risk: parallelizing touches **partial-type
  merges** (`ImmutableArray` setters) and **signal registration** — needs lock/thread-safety review to
  avoid nondeterminism. The biggest remaining lever.
- **P3 — cross-run / incremental cache**: persist parsed trees (path+mtime) and/or a reusable
  compilation so re-runs and desktop option-changes skip re-parse/re-bind. Biggest interactive win.
- **P4 — trim metadata references** to a minimal set (modest).
- **P6 — remove the unused Roslyn workspace provider** (`GetWorkspaceAsync` is never called) — clarity.
- **Benchmark variance**: total-wall numbers swing with machine load; consider pinning iterations/
  affinity or reporting per-phase (bind/parse) deltas, which are stable.
