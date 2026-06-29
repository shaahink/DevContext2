# Iteration 7 — handover (state at start)

> Warm-start doc for iteration 7. Read this first; the iteration's work is in `./PLAN.md`. Completed
> plans live in `docs/archive/`; deep references are linked at the bottom. Branch of record:
> **`develop`** (everything in "What's done" is merged in). Last numbered iteration was 6.

## Pick your thread (read order)

- **Perf (the active plan):** `./PLAN.md` → `benchmarks/results/SUMMARY.md` → hot paths in
  `src/DevContext.Core/Extractors/Generic/SyntaxStructureExtractor.cs` + `…/Specific/CallGraphExtractor.cs`.
  Run the bench: `dotnet run -c Debug --project benchmarks/DevContext.Benchmarks -- repos`.
- **Output quality / detections:** `docs/reports/OUTPUT-QUALITY-ASSESSMENT.md` + `docs/IDEAL-OUTPUT-TARGET.md`;
  the pipeline is `src/DevContext.Core/Pipeline/DiscoveryPipeline.cs` (`AnalyzeAsync`/`RenderAsync`),
  the graph is `Graph/GraphBuilder.cs` (`Build` + seam methods), entries `Graph/EntryPoint.cs`.
- **Rendering (Map/Trace/Library surface):** `Rendering/{MapRenderer,TraceRenderer,LibrarySurfaceRenderer}.cs`
  + `NarrativeSections.cs`; archetype in `Graph/ArchetypeDetector.cs`.
- **A repo audit (like DntSite):** `eval-results/DntSite/AUDIT.md` is the worked example.

## Stack / build / run

- .NET 10 (`global.json`). One engine (`DevContext.Core`) → CLI (`src/DevContext.Cli`) + WPF/Blazor
  desktop (`src/DevContext.Desktop`, Windows). Shell: Windows PowerShell 5.1.
- Build: `dotnet build DevContext.slnx` (analyzer warnings = errors).
- Test: `dotnet test tests/DevContext.Core.Tests` (**269 pass / 2 skip**) · `tests/DevContext.Desktop.Tests` (**64**).
- Benchmark: `dotnet run -c Debug --project benchmarks/DevContext.Benchmarks -- repos [RepoName…]` →
  real pipeline over the eval repos (Map + Trace), median of 3; publishes `benchmarks/results/`.
- Gotchas: pass **absolute** paths to the CLI; rebuild `DevContext.Cli` after a Core edit; PowerShell
  redirects write UTF‑16 (decode or read via tool); `$env:UPDATE_GOLDENS=1; dotnet test` regenerates goldens.

## What's done (merged to develop)

**Output quality (the G/B gap list G1–G9 + B1–B3 is cleared):**
- **G1 — multi-project / Hybrid scope.** Project/subfolder input → anchor + transitive `ProjectReference`
  closure; `.sln`/repo-root → whole solution. `.slnx` parsing, `SolutionScope.FromModel` fix,
  `ScopeResolver`, `FileTreeExtractor` union walk, large-solution guardrail.
- **G2** entry `→ target`, **G4/G6** trace dedup + topology (test projects excluded), **G7** MediatR
  style/STACK from handler types, **G8** graph-shaped stats, **G9** package cap.
- **G3 — library archetype.** `ArchetypeDetector` (App/Library) + `LibrarySurfaceBuilder` +
  `LibrarySurfaceRenderer` → AutoMapper renders a PUBLIC SURFACE.
- **G5 — minimal-API per-endpoint.** Per-route lambda nodes; Map shows `route → Command` per route.
- **Audit fixes (DntSite):** non-test project count for style (ControllerBased, not NLayer);
  `AddScheduledTask<T>`/hosted services promoted to `ScheduledJob`/HostedService entry points.

**Performance (profile-first; `benchmarks/results/SUMMARY.md`):**
- Macro benchmark runner + `CallGraphExtractor` phase timers.
- **P2** parallel semantic bind · **P1** focus-scoped binding (+ seam-landing seed so Send→Handler
  chains aren't truncated) · **IndirectWiringDetector** O(n²)→O(depth)+parallel.
- DntSite Trace **53.9 s → ~5–10 s**; CallGraph **bind 84,306 ms → ~1,800 ms (−98%)**. No regressions.

**UX:** **repo-relative source paths** in traces (`src/…/File.cs:line`, not absolute machine paths).

## Branches

- `develop` — integration (all the above).
- `feat/polish-batch-and-g1-phase0` — G1/G3/G5 + audit fixes (ancestor of perf).
- `perf/profile-and-optimize` — perf work + relative paths (fast-forward of develop).

## What's left (deferred, none blocking)

1. **P5 — Stage-2 parallelism** (`SyntaxStructureExtractor`/`DiRegistrationExtractor`): now the wall
   floor; deferred — needs thread-safety on partial-type merges + signal registration. Biggest lever.
2. **P3 — cross-run/incremental cache** (parsed trees by path+mtime / reusable compilation): biggest
   interactive/desktop win.
3. **P4 — trim metadata references**; **P6 — remove the unused Roslyn workspace provider**
   (`GetWorkspaceAsync` is never called).
4. **Benchmark variance:** total-wall swings with machine load — prefer the per-phase (bind/parse)
   deltas, which are stable; consider pinning iterations/affinity.
5. **Relative paths in the Map** entry list (currently filename-only) — optional consistency follow-up.

## Key references

- North star: `docs/IDEAL-OUTPUT-TARGET.md`. Assessment: `docs/reports/OUTPUT-QUALITY-ASSESSMENT.md`.
- DntSite evaluation: `eval-results/DntSite/AUDIT.md`. Perf: `benchmarks/results/SUMMARY.md`.
- Plans for the landed work live in `docs/archive/` (`PLAN-G1-*`, `PLAN-G3-*`, etc.).
