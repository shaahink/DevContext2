# Iteration 7 — plan: finish the performance work

> Method (unchanged from iteration 6's perf pass): **profile first, fix one lever, re-benchmark, keep
> the gate green.** Bench: `dotnet run -c Debug --project benchmarks/DevContext.Benchmarks -- repos`
> (median of 3; compare to `benchmarks/results/baseline.md`; the per-phase **bind/parse** deltas are the
> stable signal — total wall swings with machine load). Gate after every change: build 0-warn ·
> Core 269/2-skip · Desktop 64 · no detection/golden regressions (ratchet only intended changes).

## Where we are (entering iteration 7)

CallGraph bind is solved (84,306 ms → ~1,800 ms on DntSite via P1/P2 + IndirectWiring fix). The wall
floor is now **Stage 2** — `SyntaxStructureExtractor` + `DiRegistrationExtractor`, ~3.5 s each on
DntSite (1336 files), ~3.8 s on OrchardCore (5146 files). These run in parallel *across* extractors but
each iterates files serially.

## P5 — Stage-2 parallelism (biggest remaining lever) · medium–high risk

**Goal:** parallelize the per-file loop in `SyntaxStructureExtractor` (and then `DiRegistrationExtractor`).
**Risk (why it was deferred):** shared mutable state under concurrency —
- **partial-type merge** (`SyntaxStructureExtractor`: `TryAdd` then `MergePartialType` mutating
  `ImmutableArray` setters on the existing `TypeDiscovery`) → races / lost merges / nondeterministic
  member order (golden churn).
- **signal registration** (`model.Architecture.Register(...)`) — confirm thread-safety or guard.
**Approach:** `Parallel.ForEachAsync` over `AllSourceFiles`; per-type-id lock (or a concurrent
accumulator + single-threaded merge pass) around partial merges; verify `ArchitectureSignals.Register`
is safe (else lock). The shared `GetOrParseSyntaxNodesAsync` node cache already dedups parsing.
**Verify:** deterministic output (run twice, diff); Core/Desktop green; bench Map on DntSite + OrchardCore.
**Expected:** Map ~3.5 s → ~1–1.5 s on DntSite; both Map and Trace benefit.

## P3 — cross-run / incremental cache · larger, highest interactive win

**Goal:** persist parsed trees keyed by **path + mtime** (and optionally a reusable `CSharpCompilation`)
so re-runs and desktop option-changes skip re-parse/re-bind. Today `AnalysisCache` is per-run.
**Approach:** a process- or disk-level cache behind `IAnalysisCache`; invalidate on mtime change.
Biggest win for the desktop (focus/profile changes currently re-analyze from scratch).
**Verify:** second run of the same repo is near-instant; correctness identical on file change.

## P4 — trim metadata references · low effort, modest

`CallGraphExtractor.ReferenceAssemblies` loads the full TPA set (~150 assemblies). Bind only needs
intra-solution resolution (receivers resolve by name from source). Trim to a minimal BCL set (or
`Basic.Reference.Assemblies`). **Verify:** semantic-edge count unchanged on eShop/DntSite traces.

## P6 — remove the unused Roslyn workspace provider · cleanup, no perf

`GetWorkspaceAsync` is never called; `RoslynWorkspaceProvider`/`MSBuildRoslynWorkspaceProvider` are
constructed and threaded into `DiscoveryContext` but unused (the call graph builds its own compilation).
Remove the dead wiring (or wire the call graph to reuse it). Clarity only.

## Follow-ups (non-perf, optional)

- **Relative paths in the Map** entry list (`MapRenderer.Where`, currently filename-only) for parity
  with traces — uses `PathDisplay.Relative`; will ratchet the Map goldens.
- **Benchmark variance:** pin iterations/affinity or report per-phase deltas in the published table.

## Suggested order

P5 (biggest, do carefully with a determinism check) → P4 (quick, while in CallGraph) → P6 (cleanup) →
P3 (separate, larger). Re-bench + ratchet after each; append a row to `benchmarks/results/PERF-<date>.md`.
