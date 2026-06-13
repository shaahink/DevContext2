# Plan 5 — Post-Refactoring Fixes (review of PLAN-1…PLAN-4 execution)

> Source: code review of `develop` (`e6e2266`…`d0128b2`) against
> `PLAN-1-ANALYZE-ONCE-RENDER-MANY.md`, `PLAN-2-UNIFIED-FOCUS-UX.md`, `PLAN-3-NERD-STATS.md`.
> The refactoring landed the intended *structure* (snapshot/plan split, scoring channels,
> RunReport plumbing, desktop rewire) but several core behaviors the plans promised are not
> actually wired through. Phases are ordered by severity; each is independently committable.
> Build + `dotnet test` after every phase; `eval/gates.ps1` after Phases 2–3.

## Defect register

### Critical — logic

- **C1. RenderPlan is never applied to output.** `RenderPlanBuilder.Build` computes
  `IncludedTypeIds`/`Excluded` (budget + `MaxSurvivingTypes`), but no renderer consults them.
  `MarkdownRenderer` reads only `Plan?.PerTypeCharCap` (`MarkdownRenderer.cs:420`); all three
  renderers filter types with `!IsHardExcluded` (`MarkdownRenderer.cs:211,949,974,1046`,
  `HtmlContextRenderer.cs:418,453`, `JsonContextRenderer.cs:34`). Pre-refactor, the budget was
  enforced by `TokenBudgetEnforcer` setting `IsPruned`, which renderers honored; that enforcer
  was unregistered (per plan) but the replacement read-side migration (PLAN-1 Phase 3,
  "every renderer read of `IsPruned` becomes membership in `Plan.IncludedTypeIds`") was never
  done. **Net effect: `--max-tokens` and scenario type caps no longer constrain which types
  render.** The P2 cut list ("what almost made it", from `Plan.Excluded`) is also never
  rendered anywhere.

- **C2. Focus has no ranking effect unless a call graph exists.** `FocusScore` is assigned
  only inside `CallReachabilityPruner.PruneAsync`'s final loop
  (`CallReachabilityPruner.cs:77`). The pruner early-returns when `CallGraph is null`
  (`:14-17`) or when there are no `TypeName` focus points (`:27-31`) — in both cases
  `FocusScore` stays 0 for every type even though `PathProximityScruner` computed
  `PathProximityScore`. Since `FinalScore = RoleWeight·RoleScore + FocusWeight·FocusScore`
  (`DiscoveryPipeline.cs:578-580`), `--focus`/`--around` does nothing for: file/folder focus,
  any run without the call-graph profile, and unresolved type focus. This silently defeats
  PLAN-2's whole "focus dial".

- **C3. Tests exercise a different pipeline than the one that ships.** Golden tests, eval
  tests, and benchmarks all go through the legacy `RunAsync`
  (`GoldenTests.cs:53,95`, `GoldenTestHelper.cs:90,172`, `EvalExpectationTests.cs:397`,
  `PipelineTests.cs:35,79`, `benchmarks/Program.cs:83`), which builds `RenderOptions` with
  **no `Plan` and no `Report`** and never wires the collector. CLI/desktop ship
  `AnalyzeAsync`+`RenderAsync`. Consequences: goldens can't catch C1; the JSON golden claims
  `schemaVersion: 1.1` but contains **no `runReport`** (it's null on the legacy path and
  omitted by `WhenWritingNull`) — the PLAN-3 "json golden schema bump" commit changed only the
  version string. `RunAllFormatsAsync` has zero callers (dead) and its own bug (seal stopwatch
  started *after* the work, `DiscoveryPipeline.cs:343-346`). The ~80-line analyze sequence is
  triplicated across `AnalyzeAsync`/`RunAsync`/`RunAllFormatsAsync` and already diverging.

- **C4. Desktop dry-run is broken.** `DiscoveryPipeline.AnalyzeAsync` has no `DryRun`
  handling (only `RunAsync` checks it, `DiscoveryPipeline.cs:219`). The desktop sets
  `options.DryRun` and calls `AnalyzeAsync` (`AnalysisService.cs:89,143`) → toggling Dry Run
  in the UI now silently runs a full analysis.

### High — stats are dishonest (violates P3, the feature's own design principle)

- **H1. Token funnel is all zeros.** `TokenFunnel.RawEstimatedTokens`/`RenderedEstimatedTokens`
  are never set (`RunReportCollector.cs:78-79`); `TypesIncluded` is `total − hardExcluded`,
  not the plan's included count. The always-on CLI summary line therefore prints
  `"… 0/8000 tokens"` and a wrong "types kept" number (`RunReportFormatter.cs:10-13`).
  PLAN-3 required the funnel to come from the `RenderPlan` and to update per render
  (Phase 1 bullet "Funnel", Phase 3 item 4). Nothing updates it: `RenderAsync` never touches
  the report, and the desktop renders `StatsHtml` once per analysis only
  (`MainViewModel.cs:457-458`).

- **H2. Parallel speedup is permanently hidden.** `CompositeDiscoveryObserver.RecordExtractorMetrics`
  calls `rrc.AccumulateCpuTime("", elapsed)` (`CompositeDiscoveryObserver.cs:54`) — the stage
  string never matches `"GenericExtraction"`/`"SpecificExtraction"`, so CPU sums stay zero and
  the `stage2 ×N` chips never render in CLI or desktop.

- **H3. Scorer funnel table is fake.** `RunScoringAsync` measures `Count(!IsPruned)`
  before/after each pruner (`DiscoveryPipeline.cs:568-570`), but post-PLAN-1 no registered
  pruner sets `IsPruned`. Every row is N→N, delta 0%. The `--stats` "Scorer Funnel" and the
  desktop equivalent display meaningless data.

- **H4. Corpus stats: total files == C# files.** `AnalyzeAsync` passes
  `AllSourceFiles.Count` twice (`DiscoveryPipeline.cs:155-157`). Summary says
  "analyzed N files" with the wrong N semantics.

- **H5. Stage waterfall is alphabetical.** `RunReportCollector.Build()` orders stages with
  `OrderBy(s => s.Stage)` (string) (`RunReportCollector.cs:111`) → Compression sorts before
  DiscoveryAndCacheWarmup. Both the CLI table and HTML waterfall show wrong chronology.

- **H6. ExtractorStat Tier/Category are placeholder strings** (`"extractor"`/`""`,
  `RunReportCollector.cs:41-42`); the real tier/category arrive via
  `RecordExtractorMetrics`, which is an empty method on the collector
  (`RunReportCollector.cs:84-87`). Additionally, `+Types`/`+Dets` per extractor are computed
  as count deltas around each extractor (`DiscoveryPipeline.cs:523-528`), which misattributes
  counts when stages run in parallel (Stage 2 and now Stage 3).

### Medium — desktop correctness

- **M1. RerenderAsync stale-write race + CTS cross-disposal.** PLAN-1 Phase 5 explicitly
  required "a cancelled render must never write its (stale) output to the bound properties —
  check `ct.IsCancellationRequested` before the property update". The implementation has no
  such check (`MainViewModel.cs:543-560`). Worse, the `finally` block disposes and nulls
  `_renderCts` *unconditionally* (`:567-571`): if render B supersedes in-flight render A,
  A's `finally` disposes **B's** CTS — B becomes uncancellable and a later `CancelRender()`
  can hit `ObjectDisposedException`. Stale render output can overwrite newer output.

- **M2. Stats tab never updates on re-render** (see H1) and `StatsText` doesn't use the
  shared formatter: `_statsText = $"~{_totalTokens:N0} tokens · {…}s"`
  (`MainViewModel.cs:461`) instead of `RunReportFormatter.Summary(report)` (PLAN-3 Phase 3.3).

- **M3. A new DI container + pipeline is built on every render.**
  `AnalysisService.RenderAsync` does `new ServiceCollection() … BuildServiceProvider()` per
  call (`AnalysisService.cs:160-164`) — including reflection-based extractor discovery — and
  is invoked on every slider tick/section toggle (twice the renders: md + html). Violates the
  "<100 ms warm render" acceptance.

- **M4. Cancelled initial render still flips state to Done.** `RerenderAsync` swallows
  `OperationCanceledException`; `AnalyzeAsync` then proceeds to set `_hasOutput = true`,
  `"Done"`, progress 100 (`MainViewModel.cs:455-470`) without checking `ct` after the await.

### Low — edge cases, drift, dead code

- **L1. RenderPlanBuilder small-budget inversion.** `budget = MaxTokens − 500`; the check
  `underBudget = budget <= 0 || …` (`RenderPlanBuilder.cs:41,71`) means `--max-tokens ≤ 500`
  disables budgeting entirely instead of being maximally strict.
- **L2. Pinned + hard-excluded types vanish silently** — skipped in the pin loop
  (`RenderPlanBuilder.cs:49`) and skipped in the budget pass (`:59`), so they appear in
  neither `IncludedTypeIds` nor `Excluded`. Decide: pin wins (plan says "always included") or
  veto wins — either way the type must show up in exactly one list.
- **L3. `--metrics` now triggers both the Stats view and the legacy Metrics panel**
  (`AnalyzeCommand.cs:208-211`); PLAN-3 said keep it as a *hidden alias* and delete
  `MetricsDiscoveryObserver` if superseded.
- **L4. PLAN-1 Phase 6 wave split was not implemented.** Stage 3 runs blanket-parallel
  (`RunStageAsync(ExecutionStage.Stage3Sequential, …, parallel: true)`,
  `DiscoveryPipeline.cs:134`) with no wave A/B and no extension of `ValidateExtractors` to
  catch stage-3 cross-reads. It happens to be safe today (`Detections` is a `ConcurrentBag`;
  current readers consume Stage-1/2 output only) but nothing guards it. Also the enum member
  is still named `Stage3Sequential` while running parallel, and
  `CallGraphExtractor.cs:230` filters on `IsHardExcluded` which is never set until the later
  scoring stage (always false there).
- **L5. JSON `TypesSummary`/`prunedPercent` ignore the plan** (`JsonContextRenderer.cs:33-35`)
  — claims types are "in output" that the budget excluded (once C1 is fixed, this must follow).
- **L6. Cosmetics:** unused `resolver` locals (`AnalyzeCommand.cs:73`,
  `AnalysisService.cs:50`); `ServiceRegistration.cs:22` comment still says "4 pruners,
  6 compressors"; `ApplyArchitectureStyle` discards the detector's `via` value
  (`DiscoveryPipeline.cs:536-539`); `CacheStats` ignores XML hits/misses tracked in
  `AnalysisCache`.

## Phase 1 — One pipeline path (fixes C3, C4)

1. Move the `DryRun` check into `AnalyzeAsync` (return a snapshot whose model is empty and
   whose dry-run text is carried, or keep dry-run as a separate explicit method both
   front-ends call — pick one; desktop must get dry-run back).
2. Rewrite `RunAsync` as a thin wrapper: `AnalyzeAsync` + `RenderAsync` with a request built
   from `context.Options`. Delete `RunAllFormatsAsync` (no callers). Delete the triplicated
   focus-resolution/seal blocks.
3. Migrate benchmarks to `AnalyzeAsync`+`RenderAsync`. Tests may keep calling `RunAsync` —
   that's now the same path, which is the point.
4. Goldens: after the wrapper lands, `Plan`/`Report` flow into the legacy path. The JSON
   golden must now actually contain `runReport` → introduce a **golden scrubber** that
   normalizes nondeterministic report fields (all `TimeSpan`s → `"<t>"`, cache counts →
   `"<n>"`) before comparison, or serialize goldens with `runReport` stripped and assert its
   *shape* in a separate non-golden test. Regenerate with `UPDATE_GOLDENS=1`; explain diffs in
   the commit.

Gate: full `dotnet test`; goldens diff reviewed line-by-line (only plan/report-related changes).

## Phase 2 — Apply the RenderPlan (fixes C1, L1, L2, L5)

1. `MarkdownRenderer`, `HtmlContextRenderer`, `JsonContextRenderer`: build
   `HashSet<string>` from `Plan.IncludedTypeIds` once per render; every current
   `!IsHardExcluded` type-listing site becomes plan membership. Fallback when `Plan is null`
   (defensive only — after Phase 1 it never is): current behavior.
2. Ranked sections ("Related Types" etc.) order by the plan's ranked order, not by name/layer
   alone (keep grouping, order within groups by rank).
3. Render the cut list from `Plan.Excluded` (top N by score, with reasons) in the
   diagnostics/pruning-notes section — this is P2's "what almost made it".
4. `RenderPlanBuilder`: clamp `budget = Math.Max(0, MaxTokens − SafetyMargin)` and treat 0 as
   "pins only"; route pinned-but-hard-excluded types into `Excluded` with reason
   `"focus pin vetoed: test project"` (veto wins, but visibly).
5. JSON `TypesSummary` counts from the plan.
6. Tests: `--max-tokens 2000` produces fewer rendered types than `8000` on the test corpus
   (this is the regression test C1 lacked); determinism test (same snapshot+request twice ⇒
   byte-identical); cut-list renders excluded types with reasons.

Gate: goldens regenerate (expected — output now honors budget); eval `expected` checks green.

## Phase 3 — Fix the focus channel (fixes C2)

1. Remove the `FocusScore` assignment from `CallReachabilityPruner`; it only computes
   per-type `graphProximity` (store on the type or in a side map on `SharedAnalysisContext`).
2. Compute `FocusScore = max(PathProximityScore, graphProximity)` for every type in
   `RunScoringAsync`, unconditionally — works with no call graph (graph term 0) and with
   path-only focus.
3. Tests (ranking invariants per PLAN-1 3b): folder focus with no call graph still reorders
   ranking; type focus with call graph unchanged from today; no-focus ⇒ `FinalScore == RoleScore`.

Gate: `eval/gates.ps1`; the PLAN-2 acceptance ("focus present → slice") manually spot-checked
with `--focus` on this repo without `--profile debug`.

## Phase 4 — Honest stats (fixes H1–H6, H3 via C1, L3)

1. **Funnel per render:** `RenderAsync` computes `TokenFunnel` from the plan
   (`TypesDiscovered = model.Types.Count`, `TypesHardExcluded` from model,
   `TypesIncluded = plan.IncludedTypeIds.Length`,
   `RenderedEstimatedTokens = rendered.EstimatedTokens`, `Budget = request.MaxTokens`) and
   exposes it on `RenderedContext.Funnel`. `AnalysisSnapshot.Report` keeps analysis-time
   numbers only; the CLI summary line and desktop Stats tab take funnel numbers from the
   latest `RenderedContext` ("this view") and timings from the report ("this run") — the
   visual separation PLAN-3 Phase 3.4 asked for.
2. **CPU sums:** delete `AccumulateCpuTime` and the `RecordExtractorMetrics` plumbing for it;
   derive `Stage2CpuSum`/`Stage3CpuSum` inside `Build()` from `_extractorRows` (sum elapsed
   grouped by `Stage`) — one bookkeeping system, per the plan's ground rule.
3. **Scorer funnel:** replace `Count(!IsPruned)` with honest numbers: before/after counts of
   `IsHardExcluded` (the only destructive thing scorers do), and drop the pretense of a
   per-scorer type funnel — or rename the table "Hard exclusions" and add the *plan* funnel
   (included/budget-cut/cap-cut) as its own table from `Plan.Excluded` reasons. Never show
   a table whose numbers are structurally constant.
4. **Corpus:** pass real totals — total files from the file tree / `KnownFilePaths`, C# files
   from `AllSourceFiles`.
5. **Stage order:** record an ordinal when the stage completes (the collector is the single
   writer in `OnStageCompleted`) and sort by it in `Build()`.
6. **Tier/Category:** extend `OnExtractorCompleted` (or use the existing
   `RecordExtractorMetrics` properly) so rows carry real tier/category; delete the
   placeholder strings. Accept that `+Types`/`+Dets` are approximate under parallelism — or
   make `RunSingleExtractorAsync` count per-extractor via the model's concurrent collections
   tagged by source (cheap: extractors already pass a source name to `AddDiagnostic`; not
   worth more than that — if not fixed, label the column `~types` in both UIs).
7. **`--metrics`:** make it a true hidden alias for `--stats` (no second panel); delete
   `MetricsDiscoveryObserver` if nothing else uses it (grep tests first, per plan).
8. Collector unit tests: funnel math from synthetic callbacks, chronological stage order,
   speedup derivation from rows, skip reasons — the "≥ 8 tests" PLAN-3 Phase 1 demanded.

Gate: run `analyze . --stats` and paste output in the PR; summary line shows real token
numbers; speedup chips appear (×1–×4 plausible) or honest "sequential".

## Phase 5 — Desktop correctness (fixes M1–M4)

1. `RerenderAsync`: capture the CTS in a local (`var cts = new CancellationTokenSource(); _renderCts = cts;`);
   guard *all* property writes with `if (renderCt.IsCancellationRequested) return;`; in
   `finally`, dispose **only if `_renderCts == cts`** (then null it). Add a VM test: start
   render, supersede it, assert first render's output never lands and second remains cancellable.
2. After `await RerenderAsync(ct)` in `AnalyzeAsync`, check `ct.IsCancellationRequested`
   before setting Done/HasOutput.
3. Re-render path updates `StatsHtml` (funnel group only — split the HTML renderer into
   "this run" + "this view" fragments per Phase 4.1) and `StatsText` via
   `RunReportFormatter.Summary`.
4. `AnalysisService`: build the ServiceProvider/pipeline once (lazy field), reuse across
   `RenderAsync` calls; render md+html from the same pipeline instance. Re-assert the
   <1 s render test, log actual.
5. Desktop dry-run: route through whatever Phase 1 decided; add a smoke test that DryRun
   produces the plan text, not a full analysis.

## Phase 6 — Stage 3 guardrails (fixes L4, L6)

1. Decide explicitly: keep blanket-parallel Stage 3 (current, measured 36% win) — then rename
   the enum member (`Stage3Specific` or similar; it's not serialized — verify), document the
   decision where the plan expected waves, and **extend `ValidateExtractors`** to fail when a
   Stage-3 extractor declares reads of model areas another Stage-3 extractor writes
   (capabilities already exist; the validation is the missing half). If any real cross-read
   shows up, implement the wave split as PLAN-1 Phase 6 specified.
2. Remove the dead `IsHardExcluded` filter in `CallGraphExtractor.cs:230` (never true at that
   point) or move that logic post-scoring.
3. Sweep L6 cosmetics (unused locals, stale comments, `via` discard, XML cache stats).

## Phase 7 — Verification

- `dotnet build` 0 warnings; full `dotnet test` incl. CliSmokeTests; `eval/gates.ps1`.
- CLI: `analyze . --stats` (real numbers, chronological waterfall);
  `--max-tokens 2000` vs `8000` output size differs; `--format json | jq .runReport` works
  (and document that stdout JSON is followed by console chrome, or add `--quiet`);
  `--focus <folder>` without debug profile changes ranking; `--dry-run` works in CLI and desktop.
- Desktop: analyze, toggle sections rapidly + drag slider (no stale frame wins, no crash),
  Stats tab funnel changes on toggle while timings don't.
- Update `docs/AGENT-REFERENCE.md` if the pipeline API surface changed (RunAllFormatsAsync gone).
