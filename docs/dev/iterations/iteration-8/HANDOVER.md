# Iteration 8 — handover (state at end)

> Warm-start doc for whoever picks up next. The iteration's plan is in `./PLAN.md`. Branch of record:
> **`develop`** (everything in "What's done" is merged in). Iteration-7 handover/plan are alongside in
> `docs/iterations/iteration-7/`.

## Pick your thread (read order)

- **Perf:** `benchmarks/results/SUMMARY.md` → latest `benchmarks/results/PERF-*.md` → hot paths in
  `src/DevContext.Core/Extractors/Generic/SyntaxStructureExtractor.cs` + `…/DiRegistrationExtractor.cs`
  (Stage-2 floor, now parallel) and `…/Specific/CallGraphExtractor.cs` (Trace bind).
  Bench: `dotnet run -c Debug --no-build --project benchmarks/DevContext.Benchmarks -- repos [Name…]`.
- **Output quality / detections:** `docs/reports/OUTPUT-QUALITY-ASSESSMENT.md` + `docs/IDEAL-OUTPUT-TARGET.md`;
  pipeline `src/DevContext.Core/Pipeline/DiscoveryPipeline.cs`, graph `Graph/GraphBuilder.cs`.
- **Rendering:** `Rendering/{MapRenderer,TraceRenderer,LibrarySurfaceRenderer}.cs` + `NarrativeSections.cs`.
- **Caching / desktop interactivity:** `Analysis/{AnalysisCache,PersistentAnalysisCache}.cs`,
  `Desktop/Services/AnalysisService.cs`.

## Stack / build / run

- .NET 10 (`global.json`). One engine (`DevContext.Core`) → CLI (`src/DevContext.Cli`) + WPF/Blazor
  desktop (`src/DevContext.Desktop`, Windows). Shell: Windows PowerShell 5.1.
- Build: `dotnet build DevContext.slnx` (analyzer warnings = errors). **Rebuild `src/DevContext.Cli`
  after a Core edit** (its bin carries its own `DevContext.Core.dll`).
- Test: `dotnet test tests/DevContext.Core.Tests` (**271 pass / 2 skip**) ·
  `tests/DevContext.Desktop.Tests` (**64**).
- Gotchas: absolute paths to the CLI (relative ⇒ GitHub clone); PowerShell redirects are UTF-16 and
  mojibake `·` — match ASCII markers; `$env:UPDATE_GOLDENS=1; dotnet test` regenerates goldens (review first).

## What's done this iteration (merged to develop)

**P6 — removed the dead Roslyn workspace provider.** `GetWorkspaceAsync` had zero call sites. Deleted
`IRoslynWorkspaceProvider`/`IRoslynWorkspace`/`DocumentInfo`, the three impls, `Null/MockRoslynProvider`,
`DiscoveryContext.RoslynWorkspace`, and all wiring; **dropped the `DevContext.Roslyn` project** from the
slnx + ProjectReferences. `ProjectInfo`/`PackageReferenceInfo` kept (now `Contracts/ProjectInfo.cs`). No
output/perf change.

**P5 — parallelized the Stage-2 parse/walk** (`SyntaxStructureExtractor` + `DiRegistrationExtractor`).
Two-phase, output-preserving: parse + build per-file results in parallel, then commit to the shared model
single-threaded **in source-file order** (byte-identical to the old serial loops). Dropped the
`MergePartialType` reflection hack (`TypeDiscovery.BaseTypes`/`ImplementedInterfaces` now settable).

| Repo | Mode | Before | After | Stage2 before→after |
|---|---|--:|--:|--:|
| DntSite | Map | 7.7 s | **3.5 s** | 6030 → **2013 ms** |
| DntSite | Trace | 10.1 s | **4.1 s** | 6997 → 2052 ms (bind 1816→1245) |
| OrchardCore | Map | 5.7 s | **3.0 s** | 3802 → **1559 ms** |
| AutoMapper | Map | 2.0 s | **0.9 s** | 1872 → **806 ms** |

Graph counts unchanged everywhere (DntSite 1476/150/94; eShop Map 227/60/8; OrchardCore 7143/1708/281);
Map output verified byte-identical across two runs; zero golden churn.

**Determinism fix (discovered gap).** The focus-scoped Trace was *already* nondeterministic pre-P5
(eShop `POST /api/orders/` edges wandered 159/91): `CallGraphExtractor` built short-name resolution maps
by iterating `model.Types` (a `ConcurrentDictionary`, nondeterministic enumeration, first-wins) and `diMap`
from `model.Detections` (a `ConcurrentBag`, last-wins). Now iterates `model.Types` ordered by FQN and DI
detections ordered by `(SourceFile, LineNumber)`. Result: fresh-process CLI eShop trace is **byte-identical
across runs** and lands on the **complete 159-edge chain** (reaches `IntegrationEventLogEF`) every time.

**Map relative paths.** The Map entry list now shows repo-relative `src/…/File.cs:line` (trace parity)
instead of filename-only, via `PathDisplay.RelativeProvenance`. Map goldens ratcheted after review (only
the entry `(…)` provenance changed).

**P3 — cross-run cache (Desktop).** New `PersistentAnalysisCache`: per-entry mtime, re-reads/re-parses only
when a file changes on disk. `AnalysisService` reuses one cache + filesystem across analyses, so
focus/option changes that re-run the same project skip re-reading/re-parsing unchanged files. Added
`IFileSystem.GetLastWriteTimeUtc`. CLI keeps the per-run `AnalysisCache`. Unit-tested (reuse + invalidation).

## Decisions / what's left

- **P4 (trim metadata refs) — intentionally skipped.** `CallGraphExtractor.ReferenceAssemblies` is a
  `static Lazy` (loaded once per process) and the macro bench reports the **median of post-warmup
  iterations**, so the one-time ref-load is already excluded from every reported number → ~0 steady-state
  benefit, with real output risk (trimming can drop semantic resolution → flip `[verified]`/`[approx]`).
  Revisit only if a fresh-process first-run latency target appears.
- **Benchmark harness wart (follow-up).** The macro runner reuses **one `DiscoveryPipeline` across repos**
  in a single process, leaking state across them; its eShop-trace edge cell is therefore noise
  (159 alone vs 93 when DntSite runs first). Production (fresh-process CLI) is deterministic. A clean fix:
  build the pipeline per repo (or per iteration) in `RepoBenchmark`. Map/per-phase deltas are unaffected.
- **DiRegistration detection ordering** is now committed in source order, but downstream consumers that
  enumerate `model.Detections` (a `ConcurrentBag`) should sort before building order-sensitive maps — the
  determinism fix did this for `diMap`; audit other `OfType<…>()` map-builds if new ones appear.
- **Stage-2 floor** is lower but still the wall on big repos (DntSite Map Stage2 ~2 s). Next lever would be
  caching `FileSyntaxNodes` cross-run too (today only text+SyntaxTree persist on the desktop).

## Key references

- North star: `docs/IDEAL-OUTPUT-TARGET.md`. Perf: `benchmarks/results/SUMMARY.md` + `PERF-2026-06-21-0048.md`.
- Skills: `run-devcontext` (build/run/smoke), `devcontext-bench` (profile loop), `devcontext-eval-audit`.
