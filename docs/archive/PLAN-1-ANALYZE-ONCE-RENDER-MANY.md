# Plan 1 — Analyze Once, Render Many

> Implements P1, P2, P5(mechanics), P6 of `docs/DESIGN-PHILOSOPHY.md`.
> Goal: split the pipeline into an expensive **analyze** phase producing an immutable
> `AnalysisSnapshot`, and a cheap **render** phase driven by a `RenderPlan` lens
> (budget + sections + truncation). The desktop re-renders on any toggle without
> re-running analysis; the CLI behavior is externally unchanged.

## Ground rules for the executing agent

- Work on a branch off `develop`. Build after every phase: `dotnet build DevContext.sln`.
  Run tests after phases 2, 3, 5, 6, 7: `dotnet test` (CliSmokeTests are slow; you may filter
  them out except in the final verification).
- Do not change CLI flag names, defaults, or markdown output structure except where a phase
  explicitly says so. Golden tests (`tests/goldens/*`) guard this; if a phase legitimately
  changes output, regenerate with `UPDATE_GOLDENS=1` and *say so in the commit message*.
- Razor files use `@* *@` comments, never `<!-- -->`.
- The `__source__` sentinel in desktop section checkboxes maps to `ExtractionProfile.Full`;
  it is NOT a real `SectionNames` entry — do not add it to `SectionNames.cs`.
- Before deleting/renaming any public member, grep the whole solution for its name (including
  `.razor` files — they reference ViewModel members as `VM.X`).

## Phase 0 — Recon (read, don't edit)

Read in full before touching anything:

- `src/DevContext.Core/Pipeline/DiscoveryPipeline.cs` — current orchestration; `RunAsync`,
  `RunAllFormatsAsync`, `RunPruningAsync` (note the `MaxSurvivingTypes` block), `RunCompressionAsync`.
- `src/DevContext.Core/Pruning/` — all 4 pruners. Identify which mutations are *scoring*
  (`RelevanceScore`, `PathProximityScore`) vs *destructive* (`IsPruned = true`, `PrunedTypeIds`).
- `src/DevContext.Core/Compression/` — all 6 strategies. Identify which mutate type/member
  content in place and which only depend on `PerTypeCharCap`/budget (expect: only
  `AggressiveTruncator` is budget/scenario-dependent).
- `src/DevContext.Core/Models/DiscoveryModel.cs`, `TypeDiscovery` (wherever defined),
  `Scenario.cs`, `ExtractionOptions.cs`, `RenderOptions` (in `Contracts/IContextRenderer.cs`).
- `src/DevContext.Core/Rendering/MarkdownRenderer.cs` — grep `IsPruned` and `ShouldRender` to
  map every place rendering consults prune state or section gating. Same for
  `HtmlContextRenderer.cs`, `JsonContextRenderer.cs`.
- `src/DevContext.Cli/Commands/AnalyzeCommand.cs`, `src/DevContext.Desktop/Services/AnalysisService.cs`,
  `src/DevContext.Desktop/ViewModels/MainViewModel.cs` (especially `AnalyzeAsync`,
  `BuildSectionData`, `OnAnalysisOptionChanged`, `DebouncedReanalyze`, `RecalcTokenTotal`,
  `RebuildLlmViewText`), `src/DevContext.Desktop/ViewModels/SectionViewModel.cs`.

Write a short `RECON.md` scratch note (not committed) listing: every `IsPruned` read site,
every compressor that mutates content, every place `MaxOutputTokens` is consumed.

## Phase 1 — Pruners become scorers

**Intent:** stages 1–3 + scoring produce a model whose types are *ranked*, not destroyed.

1. In `TypeDiscovery`, add (if not present):
   ```csharp
   public double FinalScore { get; set; }              // combined ranking used by RenderPlan
   public string? ExclusionReason { get; set; }        // set only by hard-irrelevance rules
   public bool IsHardExcluded { get; set; }            // scorer says "never show" (test/mock noise)
   ```
2. `PathProximityPruner`, `CallReachabilityPruner`, `PatternRelevancePruner`: keep all score
   assignments. Where they currently set `IsPruned = true` for *relevance* reasons, instead
   leave the type in and let the score reflect it. Where the reason is categorical noise
   (test fixtures, mocks, generated bootstrap — `PatternRelevancePruner`), set
   `IsHardExcluded = true` + `ExclusionReason` instead of `IsPruned`.
3. **Delete** `TokenBudgetEnforcer` from the pruner pipeline registration (keep the file for
   now; Phase 3 reuses its accumulation logic inside `RenderPlanBuilder`). Remove the
   `MaxSurvivingTypes` enforcement block from `DiscoveryPipeline.RunPruningAsync` — it moves to
   `RenderPlanBuilder` too.
4. At the end of the (renamed) scoring stage, compute
   `FinalScore = PathProximityScore + RelevanceScore` once, in one place. (This preserves
   today's ranking bug-for-bug on purpose — Phase 3b replaces the math; don't improve it here.)
5. Keep `IsPruned` as a *computed compatibility property* only if renderer call sites are too
   numerous to migrate in this phase; otherwise migrate read sites in Phase 3 and delete it.
   Decide based on your Phase 0 grep count and record the decision in the commit message.
6. Rename the stage in `PipelineStage` enum from `Pruning` to `Scoring` **only if** the enum is
   not serialized anywhere (grep for usages in JSON output and tests first; if it is, keep the
   name and just update observer display strings).

Tests: existing pruner tests assert `IsPruned` — update them to assert scores /
`IsHardExcluded` instead. Run `dotnet test --filter FullyQualifiedName~Prun`.

## Phase 2 — Compression becomes canonicalization + render-time truncation

1. Strategies 1–5 (`TrivialMemberCompressor`, `BoilerplateCompressor`, `StructuralDeduplicator`,
   `NamespaceGrouper`, `LlmFriendlyFormatter`) are budget-independent canonicalization: they
   stay in the analyze phase, run once over **all non-hard-excluded types** (not just
   budget-survivors — survivors are no longer known at this point).
2. `AggressiveTruncator` leaves the pipeline. Its per-type cap becomes a `RenderPlan` field
   (Phase 3) applied by renderers (or by a small shared helper) at render time. Preserve its
   exact truncation semantics (cap source, keep signatures — read the implementation) so golden
   output is unchanged for the default path.
3. Measure: analyze-phase cost of compressing all types vs. previous survivor-only set on this
   repo itself (`devcontext analyze .`). If it regresses wall time > 25%, gate strategy 5
   (formatting) to lazy/per-type-on-first-render and note it in the RunReport later.

## Phase 3 — `AnalysisSnapshot`, `RenderPlan`, `RenderPlanBuilder`

New files in `src/DevContext.Core/Pipeline/`:

```csharp
/// Immutable result of the analyze phase. The model must not be mutated after this is created.
public sealed record AnalysisSnapshot
{
    public required DiscoveryModel Model { get; init; }
    public required SharedAnalysisContext Analysis { get; init; }
    public required Scenario Scenario { get; init; }
    public required ExtractionOptions Options { get; init; }
    // RunReport added by Plan 3; reserve the property now as `object? Report` is NOT ok —
    // just leave it out until Plan 3.
}

/// What the user wants to see right now. Cheap to construct, cheap to apply.
public sealed record RenderRequest
{
    public required string Format { get; init; }                  // "markdown" | "json" | "html"
    public required int MaxTokens { get; init; }
    public ImmutableArray<string> Sections { get; init; } = [];   // empty = scenario defaults
    public bool IncludeProvenance { get; init; }
    public bool IncludeDiagnostics { get; init; }
    public bool TokenView { get; init; }
}

/// The lens: which types render, in what order, truncated how, and what was cut.
public sealed record RenderPlan
{
    public required ImmutableArray<string> IncludedTypeIds { get; init; }   // ranked
    public required ImmutableArray<PlannedExclusion> Excluded { get; init; }
    public required ImmutableArray<string> Sections { get; init; }
    public required int PerTypeCharCap { get; init; }
    public required int EstimatedTokens { get; init; }
    public required int MaxTokens { get; init; }
}

public sealed record PlannedExclusion(string TypeId, string TypeName, double Score, string Reason);
```

`RenderPlanBuilder` (static, pure, deterministic):

1. Resolve sections: `request.Sections` if non-empty else `snapshot.Scenario.RequiredSections`.
2. Order candidate types by `FinalScore` desc, then name (stable tie-break — determinism is an
   acceptance criterion). Skip `IsHardExcluded` (they go straight to `Excluded` with their reason).
3. Accumulate estimated token cost per type (reuse `TokenBudgetEnforcer`'s `length/4` estimate
   and its safety margin of 500) until `MaxTokens`; also enforce
   `Scenario.Pruning.MaxSurvivingTypes`. Everything past the line → `Excluded` with reason
   `"budget"` or `"scenario cap"` and its score.
4. `PerTypeCharCap` from `Scenario.Compression.PerTypeCharCap` (respect
   `AggressiveTruncation == false` ⇒ `int.MaxValue`).

Pipeline API — add to `DiscoveryPipeline`, keep old methods as wrappers:

```csharp
public async Task<AnalysisSnapshot> AnalyzeAsync(DiscoveryContext ctx, CancellationToken ct);
public async Task<RenderedContext> RenderAsync(AnalysisSnapshot snapshot, RenderRequest request, CancellationToken ct);
// RunAsync / RunAllFormatsAsync become: AnalyzeAsync + RenderAsync per format. Keep their
// signatures so AnalyzeCommand/AnalysisService compile during the transition; slim them in
// Phases 4–5; then mark [Obsolete] or delete if nothing external uses them.
```

Renderer migration: `RenderOptions` gains `RenderPlan Plan`. Every renderer read of
`type.IsPruned` becomes membership in `Plan.IncludedTypeIds` (build a `HashSet` once per
render); ordering of "Related Types" style sections uses the plan's ranked order; per-type
truncation applies `Plan.PerTypeCharCap`. The "pruning notes" diagnostics section now renders
from `Plan.Excluded` (top N by score with reasons) — this is the P2 "what almost made it" list.

Acceptance for this phase:
- Same input + same request twice ⇒ byte-identical output (determinism test).
- `RenderAsync` on a warm snapshot completes in `< 100 ms` for this repo's own analysis
  (add a test that asserts `< 1 s` to be CI-safe, and log the actual number).
- Golden tests pass, or are regenerated with an explanation of every diff.

## Phase 3b — Scoring Model v2 (behavior-changing; own session, own gate)

> Phases 1–3 are structural and behavior-preserving. This phase deliberately changes *which
> types rank where* — run it only after Phase 3's gates pass, so structural and behavioral
> diffs are never conflated. Golden + eval changes here are expected and must be reviewed,
> not rubber-stamped. Read `docs/DETECTION-GUIDE.md` §1 first.

The current math has audited defects: incompatible scales (proximity ∈ [0,1] vs reachability
boosts up to 10.0 vs pattern boosts 2–5 — summed raw), additive per-detection boosts (3 DI
rows outrank 1 endpoint), pruners overruling each other (`PatternRelevancePruner` un-prunes
what `PathProximityPruner` pruned), a `PathProximityScore == 0.0f` float sentinel,
type-level focus receiving zero graph signal, `MaxSurvivingTypes` enforced in two places,
and name-suffix test detection hard-excluding production types.

Replace with three **normalized channels**, all ∈ [0, 1], each emitting `InclusionReason`
provenance (keep that machinery — it feeds the cut list and stats):

1. **FocusScore** = `max(pathProximity, graphProximity)` where `pathProximity` is the
   existing directory-distance formula and `graphProximity` comes from BFS over the
   **type-collapsed call graph** (edges already carry `CalleeType`; build
   `Dictionary<string, HashSet<string>>` caller-type → callee-types once). Seed from focus
   **types as well as methods** — this fixes the `--around TypeName` blind spot and the
   clean-architecture bias (cross-project relevance flows through calls, not folders).
   Decay: `1.0 / (1.0 + depth)`, capped at `MaxCallDepth`.
2. **RoleScore** = `max` (not sum) over the type's detection roles, + 0.1 per *additional
   distinct* role, capped at 1.0. Weights (one table, in code, with this comment —
   rationale: how load-bearing is this type for understanding the app):
   `Endpoint 1.0, MediatRHandler 0.8, MessageConsumer 0.7, EfEntity 0.6, BackgroundWorker 0.5,
   Middleware 0.5, IndirectWiring-target 0.5, DiRegistration 0.35`.
   Library mode (no web signals — keep the existing `HasWebSignals` gate): RoleScore from API
   surface instead: `referenced-as-base/interface 0.6, public 0.4`, additive, capped 1.0.
3. **FinalScore** = scenario-owned weights (add to `Scenario`, so depth changes ranking
   intent): overview `0.55·Role + 0.25·Focus + 0.20·Graph-part-of-focus` → simplify to two
   weights since graph folded into Focus: overview `0.7·Role + 0.3·Focus`; deep-dive/trace
   `0.35·Role + 0.65·Focus`. **No focus points** ⇒ Focus channel is meaningless: use
   `FinalScore = RoleScore` (do NOT keep the flat-0.5 hack).

**Rules, separated from scores**, applied in this order by `RenderPlanBuilder` (delete all
`IsPruned` flips from scorers; delete the float sentinel; delete the un-prune override):

1. **Pin**: explicit focus types always included (top of plan, exempt from caps).
2. **Veto**: types in test *projects* → `IsHardExcluded`, reason `"test project"`.
   Name-pattern-only matches (`*Mock`, `*Should`, …) get RoleScore forced to 0 (penalty)
   but are NOT vetoed — fixes false hard-exclusion on production/library types.
3. **Floor**: detection-bearing types are never hard-excluded (may still lose to budget —
   that's the plan's job, visible in the cut list).
4. `MaxSurvivingTypes` enforced in exactly **one** place: `RenderPlanBuilder` (delete the
   pipeline's "ScenarioBudget" block AND the copy inside the old enforcer).

**Tests — ranking invariants, not constant-pinning** (new file, ≥ 8; these encode intent and
survive future retuning):
- overview: endpoint handler at path distance 3 outranks plain POCO at distance 0;
- overview: 1 endpoint role outranks 3 DI registrations;
- trace with type-level focus: type call-reachable at depth 1 outranks unreachable endpoint;
- trace: focus type itself always included even with budget 1000;
- no-focus: ranking equals RoleScore ordering;
- library mode: public base interface outranks internal helper;
- name-pattern type in production project: included when budget allows, RoleScore 0;
- test-project type: excluded with reason, visible in `Excluded` list.

Gate: `eval/gates.ps1` + regenerate goldens deliberately; eval `expected` checks must stay
green, and the AutoMapper (library) aspirational output should *improve* — if it gets worse,
the weights are wrong, stop and report rather than tune blindly against one repo
(DETECTION-GUIDE §6 applies to scoring constants too).

## Phase 4 — CLI rewire

In `AnalyzeCommand.ExecuteAsync`:

1. Replace the `AnsiConsole.Status().Start(... GetAwaiter().GetResult())` block with
   `await AnsiConsole.Status().StartAsync(...)` — remove the sync-over-async.
2. Flow becomes: `var snapshot = await pipeline.AnalyzeAsync(ctx, ct);` then build one
   `RenderRequest` from settings and `await pipeline.RenderAsync(snapshot, request, ct)`.
3. `--max-tokens`, `--format`, sections, `--include-diagnostics` now feed `RenderRequest`,
   not `ExtractionOptions` (leave the `ExtractionOptions` fields in place if removal breaks
   too much; route reads through the request).
4. External behavior identical: same flags, same output, same summary table. Verify with
   `dotnet run --project src/DevContext.Cli -- analyze . --scenario overview` against a saved
   pre-change output.

## Phase 5 — Desktop rewire (the payoff)

1. `AnalysisService.AnalyzeAsync` splits:
   ```csharp
   Task<SnapshotResult> AnalyzeAsync(AnalysisOptions opts, IProgress<AnalysisProgress>?, CancellationToken);
   Task<RenderResult>  RenderAsync(AnalysisSnapshot snapshot, RenderRequest request, CancellationToken);
   ```
   `SnapshotResult` carries the `AnalysisSnapshot` (+ Success/Error/ElapsedMs). `RenderResult`
   carries markdown + html strings. Render markdown and html from the same snapshot.
2. `MainViewModel`:
   - Hold `private AnalysisSnapshot? _snapshot;` Invalidate it only when *analysis inputs*
     change: project path, around/focus, task, scenario, NoRoslyn, anti-patterns toggle.
   - Section checkboxes, token slider, format, provenance/diagnostics toggles become
     *render inputs*: `OnAnalysisOptionChanged` splits into `OnAnalysisInputChanged()`
     (full re-analyze, debounced as today) and `OnRenderInputChanged()` (calls
     `RerenderAsync()` — `Task.Run` the plan+render, then one batched `OnPropertyChanged("")`,
     same pattern as today's post-analysis update).
   - `RerenderAsync` needs its own `CancellationTokenSource` (separate from the analysis CTS):
     rapid slider movement must cancel the in-flight render, and a cancelled render must never
     write its (stale) output to the bound properties — check `ct.IsCancellationRequested`
     before the property update, mirroring how `AnalyzeAsync` guards today.
   - **Delete `BuildSectionData` markdown parsing.** Section rows now come from the snapshot:
     for each section name in scope, token counts from the plan/renderer
     (`RenderedContext` should expose per-section token estimates — add
     `ImmutableArray<SectionStat> Sections` to `RenderedContext` populated by
     `MarkdownRenderer`; `SectionStat(string Name, int Tokens)`). `SectionViewModel` keeps its
     shape; `FullText` per section may be dropped if only used for token math — grep first.
   - Budget bar (`SelectedTokenTotal`, `BudgetUtilisation`) recomputes from the plan, not from
     parsed markdown.
3. Grep before removing anything: `BuildSectionData`, `RebuildLlmViewText`, `RecalcTokenTotal`,
   and every `VM.` reference in `Components/*.razor`.
4. Desktop tests: `MainViewModelTests` that stub `IAnalysisService` need the new two-method
   interface; update NSubstitute setups. Tests asserting "option change triggers re-analysis"
   split into "analysis-input change triggers re-analysis" and "render-input change triggers
   re-render only" (assert `AnalyzeAsync` call count stays 1).

Acceptance: with a loaded analysis, toggling any section checkbox or moving the token slider
must not call `AnalysisService.AnalyzeAsync` again, and the visible output updates. Manually
verify once via the app (`dotnet run --project src/DevContext.Desktop`).

## Phase 6 — Stage 3 partial parallelization

1. For each Stage 3 extractor, fill in `Capabilities.ReadsSignals` / model-area read/write
   declarations honestly (read the extractor bodies; e.g. `CallGraphExtractor` writes
   `Analysis.CallGraph`, `SourceBodyExtractor` reads types others added).
2. In `DiscoveryPipeline`, split Stage 3 into two waves: wave A = extractors with no
   dependency on another Stage 3 extractor's output → run via the existing parallel path
   (`RunStageAsync(..., parallel: true)`); wave B = the rest (expect at least
   `CallGraphExtractor`, `SourceBodyExtractor`, `IndirectWiringDetector` — verify by reading,
   don't guess) → sequential, ordered as today.
   Use the `[ExtractorOrder]` attribute or a new `RunsAfter` capability — pick whichever the
   existing validation machinery (`ValidateExtractors`) can check, and extend the validation
   to assert wave A members don't read what wave A members write.
3. Measure on this repo + one large OSS solution (e.g. clone `dotnet/eShop`): record before/after
   Stage 3 wall time in the PR description. If win < 15%, keep the code but note it; if it
   introduces flaky test failures, revert wave assignment to sequential and leave the
   capability declarations (they're documentation either way).

## Phase 7 — Final verification

- `dotnet build DevContext.sln` — 0 warnings/0 errors.
- `dotnet test` — full suite including CliSmokeTests.
- CLI manual: `analyze .` (markdown to stdout), `--format json -o out.json`, `--format html`,
  `--around DiscoveryPipeline`, `--dry-run`, `--max-tokens 2000` (verify the cut list shows in
  `--include-diagnostics`).
- Desktop manual: analyze this repo, toggle 3 sections, move slider, switch Human/LLM tabs,
  copy buttons. No freeze, no re-analysis on toggles.
- Update `docs/AGENT-REFERENCE.md` data-flow section to describe Analyze/Render phases.
