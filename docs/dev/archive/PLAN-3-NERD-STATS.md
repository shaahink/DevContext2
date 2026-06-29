# Plan 3 — Nerd Stats: RunReport Everywhere

> Implements P3 (and P5's "prove it" half) of `docs/DESIGN-PHILOSOPHY.md`. Depends on Plan 1
> (`AnalysisSnapshot` exists; scoring/render split done). Independent of Plan 2.
> Goal: every analysis produces a structured `RunReport`; the CLI shows a one-line summary
> always and full tables behind `--stats`; the desktop gets a Stats tab in a terminal
> aesthetic; JSON output embeds the report. The numbers must come from the existing observer
> stream — no second bookkeeping system.

## Ground rules

- Branch off Plan 1's result. Build/test per phase. Razor comments `@* *@`.
- The observer interface (`IDiscoveryObserver`) is the single collection point. Extend it only
  if a number cannot be derived from existing callbacks; prefer deriving.
- Stats must never slow the hot path measurably: collectors are plain fields + lists, no
  locking beyond what concurrent stage callbacks require (use `ConcurrentBag` for extractor
  rows since Stage 2/3-wave-A complete in parallel).

## Phase 0 — Recon

Read: `src/DevContext.Core/Contracts/IDiscoveryObserver.cs`, the existing
`MetricsDiscoveryObserver` (in Cli — note `GetMetricsSummary`), `CompositeDiscoveryObserver`,
`SpectreDiscoveryObserver`, `src/DevContext.Core/Analysis/AnalysisCache.cs` (find where lazy
cache entries are created — that's where hit/miss counters go), `DiscoveryPipeline`
(`RecordMetrics`, stage stopwatches), `RenderedContext`, `JsonContextRenderer` +
`DevContextOutput.cs`, `OutputPanel.razor` (tab structure), the desktop CSS/theme file
(grep for the Catppuccin color definitions under `src/DevContext.Desktop/`).

## Phase 1 — `RunReport` model + collector

New `src/DevContext.Core/Models/RunReport.cs`:

```csharp
public sealed record RunReport
{
    public required ImmutableArray<StageStat> Stages { get; init; }          // name, wall time
    public required ImmutableArray<ExtractorStat> Extractors { get; init; }
    public required ImmutableArray<ScorerStat> Scorers { get; init; }        // pruner before/after
    public required ImmutableArray<CompressionStat> Compressions { get; init; }
    public required CacheStats Cache { get; init; }
    public required CorpusStats Corpus { get; init; }                         // files, C# files, LOC, projects
    public required TokenFunnel Funnel { get; init; }
    public required ParallelismStats Parallelism { get; init; }
    public required TimeSpan TotalWall { get; init; }
}

public sealed record StageStat(string Stage, TimeSpan Elapsed);
public sealed record ExtractorStat(string Name, string Tier, string Category, string Stage,
    TimeSpan Elapsed, int TypesAdded, int DetectionsAdded, bool Skipped, string? SkipReason);
public sealed record ScorerStat(string Name, int TypesBefore, int TypesAfter);
public sealed record CompressionStat(string Name, int TokensSaved);   // derive from CompressionResult fields
public sealed record CacheStats(int TextHits, int TextMisses, int SyntaxTreeHits, int SyntaxTreeMisses);
public sealed record CorpusStats(int TotalFiles, int CSharpFiles, int Projects, long TotalBytes);
public sealed record TokenFunnel(int TypesDiscovered, int TypesHardExcluded, int TypesIncluded,
    int RawEstimatedTokens, int RenderedEstimatedTokens, int Budget);
public sealed record ParallelismStats(TimeSpan Stage2Wall, TimeSpan Stage2CpuSum,
    TimeSpan Stage3Wall, TimeSpan Stage3CpuSum);   // speedup = CpuSum / Wall
```

`RunReportCollector : IDiscoveryObserver` in Core (not Cli — both front-ends need it):
accumulates from the existing callbacks; `Build()` returns the record. Notes:

- **Skipped extractors:** `OnExtractorCompleted` has `skipped`/`skipReason` parameters but
  `DiscoveryPipeline.RunStageAsync` currently filters non-eligible extractors *before* the
  loop, passing `false` always. Change the pipeline: extractors excluded by `ExcludeExtractors`
  or `ShouldRun == false` still get an `OnExtractorCompleted(name, TimeSpan.Zero, skipped: true,
  reason)` call with reason `"excluded by scenario"` / `"signal gate: needs X"` (derive the
  signal name from `Capabilities.ReadsSignals`). Signal-gated skips are a *feature* — they must
  show in the report.
- **Cache stats:** add `Interlocked`-incremented counters to `AnalysisCache` (`Hits`/`Misses`
  per cache kind — a hit is "Lazy already created"). Expose `AnalysisCacheStats GetStats()`.
  The collector reads it at `OnPipelineCompleted`.
- **Corpus stats:** from `model.Projects` + the file tree (`FileTreeExtractor` output — find
  where file counts live in the model; if absent, count during `OnPipelineCompleted` from
  model fields, never by re-walking the disk).
- **Funnel:** discovered = `model.Types.Count`; hard-excluded/included from the `RenderPlan`
  of the *default* render (wire: `DiscoveryPipeline.RenderAsync` updates funnel fields on a
  mutable holder inside the report, or — simpler — `RunReport.Funnel` is computed per
  `RenderedContext` and `RenderedContext` gains `TokenFunnel Funnel`; pick the simpler and be
  consistent).
- **Parallelism:** stage wall from `OnStageCompleted`; cpu-sum = Σ extractor elapsed per stage.

`AnalysisSnapshot` gains `public required RunReport Report { get; init; }` (reserved in Plan 1).
Wire the collector into both front-ends via `CompositeDiscoveryObserver` (CLI keeps Spectre
progress; desktop keeps `DesktopProgressObserver`).

Tests: collector unit tests feeding synthetic callback sequences; assert funnel math, skip
reasons, speedup derivation. ≥ 8 tests.

## Phase 2 — CLI surface

1. **Always-on summary line** (replaces nothing; add after output):
   `analyzed 412 files · 38 types kept of 167 · 7,842/8,000 tokens · 1.9s (stage2 ×3.1 parallel)`
   — dim markup, single line, built from `RunReport`.
2. `--stats` flag (keep `--metrics` as a hidden alias): renders the full nerd view with
   Spectre:
   - stage waterfall table (stage, wall ms, bar made of `█` proportional to total),
   - extractor table sorted by elapsed desc (incl. skipped rows, dim, with reason),
   - scorer funnel (`PathProximity 167→167 · CallReachability 167→52 · PatternRelevance 52→48 · budget 48→38`),
   - compression savings per strategy,
   - cache hits/misses, corpus stats, parallel speedup.
   Reuse/replace `MetricsDiscoveryObserver`'s formatting; delete it if fully superseded
   (grep tests first).
3. `--format json`: `DevContextOutput` gains a `runReport` property (camelCase, serialized from
   `RunReport`). Bump `SchemaVersion` to `"1.1"` and update the schema file under
   `docs/schemas/` if one exists for output (check; the folder exists).
4. Golden tests: markdown goldens must NOT change (summary line goes to console, not into
   `-o` file output — verify `WriteOutput` path). JSON golden will change → regenerate with
   `UPDATE_GOLDENS=1`, explain in commit.

## Phase 3 — Desktop Stats tab

1. `OutputPanel.razor`: third tab **Stats** next to Human/LLM. Content = HTML rendered from
   `RunReport` by a new small `RunReportHtmlRenderer` in Core Rendering (so CLI `--format html`
   can embed the same block later; keep it a standalone fragment renderer, ~150 lines max):
   - timing waterfall: one row per stage, monospace label + proportional bar (`<div>` widths,
     no JS chart libs),
   - extractor grid: name, time, +types, +detections; skipped rows dimmed with reason tooltip,
   - token funnel: `discovered → relevant → rendered` as three stacked bars with counts,
   - cache + parallelism chips (`cache 98% hit`, `stage2 ×3.1`, `stage3 ×1.7`).
2. Aesthetic (this is the "nerdy theme" beachhead): monospace font stack
   (`Cascadia Code, Consolas, monospace`), existing Catppuccin palette, subtle borders, no
   animation. Keep all styling in the existing stylesheet location; prefix classes `dc-stats-`.
3. `MainViewModel`: expose `StatsHtml` (rendered once per analysis from
   `_snapshot.Report`), include in the single batched property update. The existing
   `StatsText` one-liner in the toolbar gets the same summary string as the CLI line (share
   the formatter: `RunReportFormatter.Summary(report)` in Core).
4. Re-render on section/budget toggles (Plan 1's `RerenderAsync`) must update the funnel
   numbers in the Stats tab (funnel is per-render; timings are per-analysis — visually
   separate the two groups so the distinction is honest: "this run" vs "this view").

Desktop tests: `StatsHtml` populated after analysis; funnel updates after render-input change
while extractor table is unchanged.

## Phase 4 — Verification

- `dotnet build` + full `dotnet test`.
- CLI: `analyze . --stats` screenshot-level review (paste output in PR); `--format json | jq .runReport`
  sanity; summary line absent from `-o file.md` content.
- Desktop: analyze this repo, Stats tab renders, toggle a section → funnel changes, timings don't.
- Honesty check (P3 applies to us): the parallel speedup numbers must be plausible (×1–×4);
  if stage3 shows ×1.0 because Plan 1 Phase 6 was reverted, the chip should simply say
  `stage3 sequential` — never fake a number.
- Update `docs/cli-reference.md` (`--stats`) and `docs/desktop-ui.md` (Stats tab).
