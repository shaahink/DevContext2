---
name: devcontext-bench
description: Profile and speed up the DevContext analysis pipeline. Use when asked to benchmark, profile, find a hot path, or optimize DevContext performance (analyze/Map/Trace speed, call graph, parsing). Drives the real-repo macro benchmark, reads per-extractor/phase timings, and follows the profile-first fix loop.
---

DevContext perf work is **profile-first**: measure the real pipeline over real repos, find the hot
path from data (not guesses), fix one lever, re-benchmark, keep the gate green. Paths are relative to
the repo root. Shell is **Windows PowerShell 5.1** (`powershell.exe`).

## The harness

`benchmarks/DevContext.Benchmarks` has two modes:
- **Macro runner** (the one you want) — `… -- repos [Name…]` runs the *real* `AnalyzeAsync` (incl.
  `CallGraphExtractor`) over the standing eval repos in **Map** and **Trace** modes, warm-up + median
  of 3, reusing the CLI composition root (`AddDevContextServices`) so the extractor set never drifts.
- **Synthetic BenchmarkDotNet micro-bench** (default, no args) — fake files, `AllowRoslyn=false`; only
  the cheap Stage-1 extractors. Useful for isolated micro-deltas, **not** the call-graph hot path.

```powershell
dotnet build DevContext.slnx                                              # rebuild first (Core→runner)
dotnet run -c Debug --no-build --project benchmarks/DevContext.Benchmarks -- repos          # all repos
dotnet run -c Debug --no-build --project benchmarks/DevContext.Benchmarks -- repos DntSite TodoApi
```

Publishes `benchmarks/results/PERF-<date>.md` (and seeds `baseline.md` on first run). The standing
suite: DntSite (representative, 1336 files) · TodoApi (small) · VerticalSlice · eShop.Ordering.API
(closure) · AutoMapper (library, Map only) · OrchardCore (large, 5146 files). Compare against
`benchmarks/results/baseline.md`; the narrative is in `benchmarks/results/SUMMARY.md`.

## Reading the result

Each row: median wall · min–max · files · nodes/edges/entries · **Stage2** / **Stage3** wall · **top
extractors by elapsed** (already the hot-path list). Plus a **call-graph phase breakdown**
(parse · compile · bind · bfs) — `CallGraphExtractor` emits these as a diagnostic. The per-phase and
per-extractor numbers are the **stable** signal; total wall swings with machine load.

What the baseline taught us (don't re-derive): the trace hot path is **`CallGraphExtractor` semantic
bind**; parse/compile are lazy (~0); the Map floor is **`SyntaxStructureExtractor` + `DiRegistrationExtractor`**
(parse/walk all files). Landed fixes: parallel bind, focus-scoped binding (+ seam-landing seed),
`IndirectWiringDetector` O(n²)→O(depth). Remaining levers in `docs/iterations/iteration-7/PLAN.md`.

## The fix loop (one lever at a time)

1. Bench → identify the top cost from the table/phases.
2. Apply ONE change (parallelize a serial loop · scope work to the focus · fix an O(n²) · trim refs).
3. `dotnet build DevContext.slnx` → **rebuild CLI after a Core edit** · `dotnet test tests/DevContext.Core.Tests`
   (269/2-skip) + `tests/DevContext.Desktop.Tests` (64) — **no detection/golden regressions**.
4. Re-bench the same repos; record the delta vs baseline; commit with the before/after numbers.

## Correctness guardrails (perf must not change output)

- A trace's **edge count** and **content** must be unchanged unless intended. Verify a focused trace
  still reaches its deep/cross-project seams — e.g. eShop `POST /api/orders/` must still reach
  `IntegrationEventLogEF` (a Send→Handler seam, *not* a call edge). `TraceQualityTests` only checks a
  few shallow substrings, so **eyeball the deep hops** too (capture the CLI trace and grep).
- Roslyn `GetSemanticModel` is safe to call concurrently on one `Compilation`; `model.Detections` /
  `model.Types` are concurrent collections. But **partial-type merges** and **signal registration**
  are not obviously thread-safe — guard them before parallelizing `SyntaxStructureExtractor`.

## Gotchas

- **Rebuild after a Core edit** — the runner and CLI carry their own `DevContext.Core.dll`.
- **Total-wall variance** — background load swings it; rely on per-phase (bind/parse) deltas or run
  the suite when the machine is quiet; `--no-build` keeps iterations comparable.
- **PowerShell mojibakes `·`** (UTF-8 as ANSI) — match ASCII markers, not the middot.
- **Absolute paths** to the CLI; relative paths are parsed as GitHub `owner/repo`.
