# Iteration 8 — plan: finish the performance work + cleanup

> Method (unchanged): **profile first, fix one lever, re-benchmark, keep the gate green.** Bench:
> `dotnet run -c Debug --no-build --project benchmarks/DevContext.Benchmarks -- repos [Name…]`
> (median of 3 after 1 warm-up; compare to `benchmarks/results/baseline.md`; the per-phase
> **bind/parse** and **Stage2/Stage3** deltas are the stable signal — total wall swings with machine
> load). Gate after every commit: build 0-warn · Core **269/2-skip** · Desktop **64** · no
> detection/golden regressions (ratchet only intended changes, **with review**, never blind
> `$env:UPDATE_GOLDENS=1`).

## Where we are (entering iteration 8)

CallGraph **bind** is solved (DntSite 84,306 ms → ~1,816 ms via P1/P2 + IndirectWiring fix). The floor
is now **Stage 2 parse/walk**, and it dominates *both* Map and Trace:

| Repo | Map total (Stage2) | Trace total (Stage2 / bind) |
|---|--:|--:|
| DntSite (1336 files) | 7.7 s (**6030 ms**) | 10.1 s (6997 ms / 1816 ms) |
| OrchardCore (5146 files) | 5.7 s (**3802 ms**) | — |
| AutoMapper (500 files) | 2.0 s (**1872 ms**) | — |

`SyntaxStructureExtractor` + `DiRegistrationExtractor` each iterate **all files serially**. Stage 2
*already* runs extractors **in parallel across each other** (`DiscoveryPipeline.RunStageAsync(…, parallel:true)`),
so every shared model collection is already concurrent: `model.Types` = `ConcurrentDictionary`,
`model.Detections`/`model.CallEdges` = `ConcurrentBag`, `model.Architecture` = `ConcurrentDictionary` +
`AddOrUpdate`. **Therefore P5's "signal registration thread-safety" question is already answered — it
is safe.** The genuine P5 risk is **determinism** (golden churn), not crashes:

1. `SyntaxStructureExtractor.MergePartialType` is a non-atomic read-modify-write on a shared
   `TypeDiscovery` (mutates `Methods`/`Properties`, reflection-sets `BaseTypes`/`ImplementedInterfaces`)
   → races + nondeterministic member order.
2. Member/detection **order** becomes file-scheduling-dependent. `DiRegistrationExtractor` order even
   feeds `CallGraphExtractor`'s `diMap` (last-write-wins by key), so order can change output.

**P5 design (output-preserving):** a uniform **two-phase pattern** — parse + build per-file results
in parallel; then **commit shared-state mutations single-threaded in deterministic file order**. This
is byte-identical to today's serial output (zero intended golden churn) while the expensive parse fans
out across cores.

## Phase order (cheap/clean → biggest-perf → biggest-architecture)

### Phase 1 — P6: remove the dead Roslyn workspace provider · cleanup, no perf/output change
- **Goal:** `GetWorkspaceAsync` has **zero call sites** (only definitions). Delete the abstraction.
- **Approach:** remove `IRoslynWorkspaceProvider`/`IRoslynWorkspace` contracts; `RoslynWorkspaceProvider`,
  `MSBuildRoslynWorkspaceProvider`, `RoslynWorkspace`, `NullRoslynProvider`, `MockRoslynProvider`;
  `DiscoveryContext.RoslynWorkspace`; and wiring in `AnalyzeCommand.BuildRoslynProvider`, CLI
  `Program.cs`, Desktop `AnalysisService`, bench `MockRoslynProvider`, tests
  (`DiscoveryContextBuilder`, `TestHelpers`, `GoldenTestHelper`, `EvalExpectationTests`).
  `DevContext.Roslyn` becomes empty → **drop the project from `DevContext.slnx` + all ProjectReferences.**
- **Files:** `src/DevContext.Roslyn/**`, `src/DevContext.Core/Contracts/IRoslynWorkspaceProvider.cs`,
  `Models/DiscoveryContext.cs`, `src/DevContext.Cli/**`, `src/DevContext.Desktop/Services/AnalysisService.cs`,
  `benchmarks/**`, `tests/**`, `DevContext.slnx`, affected `*.csproj`.
- **Risk:** low/mechanical, wide surface. **Verify:** build 0-warn · Core 269/2 · Desktop 64. **Δ:** none.

### Phase 2 — P5: parallelize Stage 2 (biggest lever) · medium risk, guarded
- **Goal:** fan the per-file parse/walk across cores in both Stage-2 extractors.
- **Approach:** `Parallel.ForEachAsync` over source files.
  - **Phase A (parallel):** parse via the shared `SyntaxNodeCache` (already thread-safe `Lazy`), build
    per-file `List<TypeDiscovery>` / `List<DiRegistrationDetection>` (pure, no shared writes).
  - **Phase B (serial, file order):** `Types.TryAdd` + partial merge, controller-signal registration,
    `Detections.Add`. Deterministic input order = byte-identical output.
  - Drop the `MergePartialType` reflection hack: make `TypeDiscovery.BaseTypes` /
    `ImplementedInterfaces` settable.
- **Files:** `Extractors/Generic/SyntaxStructureExtractor.cs`, `…/DiRegistrationExtractor.cs`,
  `Models/TypeDiscovery.cs`.
- **Risk:** determinism. **Verify (extra):** run `analyze` **twice and diff** (identical); goldens +
  Core/Desktop green; re-bench DntSite + OrchardCore + AutoMapper; eyeball deep seams.
- **Expected Δ:** DntSite Map **7.7 → ~3–4 s** (Stage2 6030 → ~1200–1800 ms), Trace **10.1 → ~5–6 s**;
  OrchardCore Map **5.7 → ~3 s**; AutoMapper **2.0 → ~1 s**.

### Phase 3 — Map relative paths · small, reviewed golden ratchet
- **Goal:** parity with traces — the Map entry list shows `src/…/File.cs:line`, not filename-only.
- **Approach:** thread `RootPath` into `MapRenderContext` (TraceRenderer already relativizes via
  `PathDisplay`); rewrite `MapRenderer.Where` to use `PathDisplay.RelativeProvenance`.
- **Files:** `Rendering/MapRenderer.cs` (+ caller wiring), `tests/goldens/*` (reviewed).
- **Risk:** low. **Verify:** inspect each Map golden diff (only path formatting changed), then ratchet.
  **Δ:** none (cosmetic).

### Phase 4 — P4: trim metadata references · honest scope, likely marginal
- **Finding:** `CallGraphExtractor.ReferenceAssemblies` is a **static `Lazy`** → loaded once per
  process, amortized to ~0 across repeated runs and in the desktop. Benchmark upside ≈ 0; output risk
  is real (fewer refs can drop semantic resolution → flip `[verified]`/`[approx]`).
- **Approach:** measure first-bind cost; **only** trim to a minimal BCL set **if** material **and**
  semantic-edge counts are provably unchanged on eShop (159) + DntSite. Else document the finding and
  skip. **Verify:** semantic-edge parity is the gate.

### Phase 5 — P3: cross-run cache (Desktop) · largest, architectural
- **Goal:** skip re-parse when files are unchanged so desktop focus/option changes are near-instant
  (today each run does `new AnalysisCache(fs)` → re-parses from scratch).
- **Scope (chosen):** **Desktop process-level cache + mtime invalidation only** (no disk persistence).
- **Approach:** add `GetLastWriteTimeUtc(path)` to `IFileSystem` (+ `RealFileSystem`, `FakeFileSystem`);
  make an mtime-validating cache that invalidates a cached entry when its file's mtime changes; reuse a
  single cache instance across Desktop analyses.
- **Files:** `Contracts/IFileSystem.cs`, `IO/RealFileSystem.cs`, test `FakeFileSystem`,
  `Analysis/AnalysisCache.cs` (or a new `PersistentAnalysisCache`), `Desktop/Services/AnalysisService.cs`.
- **Risk:** highest for correctness (stale → wrong output). **Verify:** edit a file → entry busts →
  identical output; unchanged re-run near-instant; Core/Desktop green.

## Per-commit gate & correctness protocol (non-negotiable)

1. `dotnet build DevContext.slnx` (warnings = errors) → **rebuild `src/DevContext.Cli`** after a Core edit.
2. `dotnet test tests/DevContext.Core.Tests` (269/2-skip) + `tests/DevContext.Desktop.Tests` (64).
3. Re-bench the affected repos; append `benchmarks/results/PERF-<date>.md`; commit with before/after.
4. **Correctness:** TraceQualityTests is shallow — eyeball deep hops: eShop `POST /api/orders/` →
   `IntegrationEventLogEF` (23 hops / 159 edges) and DntSite `FeedController` (15 hops) unchanged.
   P5/P4 additionally diff a twice-run capture / compare semantic-edge counts.
5. **Ratchet only with review.** Gotchas: absolute CLI paths; UTF-8 capture / match ASCII markers
   (not `·`); rebuild CLI after Core edit; `--no-build` keeps bench iterations comparable.

## Branch / merge

`feat/iteration-8` off `develop`. This plan is committed first; one commit per phase; push. At the end:
merge to `develop` (fast-forward if possible), push, refresh `docs/iterations/iteration-8/HANDOVER.md`.
