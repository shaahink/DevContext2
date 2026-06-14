# Plan 9 — Fix review findings on `feature/desktop-live-update-gating` + whole-solution test cleanup

> Companion to the review of commit `432fd4f` (PLAN-8 + feature-wiring fixes). This plan makes
> the branch genuinely merge-ready: it fixes one regression and six smaller defects, then does a
> whole-solution test pass (add the missing real-value tests, repair "test-theater" tests whose
> names don't match their assertions, delete/rewrite brittle tests, and housekeep organization).
>
> Branch: continue on `feature/desktop-live-update-gating` (base `develop`).

## Preconditions & how to build/test (read first)

- **There is no `.sln`.** The handover's `dotnet build DevContext.sln` is wrong. Build/test
  per project:
  - Core: `dotnet test tests/DevContext.Core.Tests/DevContext.Core.Tests.csproj`
  - Desktop: `dotnet test tests/DevContext.Desktop.Tests/DevContext.Desktop.Tests.csproj`
  - (Desktop is `net10.0-windows`; requires Windows.) Baseline today: **247 + 79 = 326 green.**
- After **every** task: build + run the affected test project. Do not advance on red.
- Do not change the PLAN-1 analyze→render split or the cache key shape beyond Task A2.
- Many tests construct a throwaway empty `RunReport`; Task C1 introduces a factory — use it in new
  tests rather than copy-pasting the 11-line literal.

---

# PART A — Code fixes (priority order)

## A1 (P1, BLOCKING) — Restore Anti-patterns & Event flow rendering

**Problem.** Commit `432fd4f` gated both sections:
```csharp
if (ShouldRender(SectionNames.AntiPatterns, options)) { AppendAntiPatterns(...); }
if (ShouldRender(SectionNames.EventFlow, options))    { AppendEventFlow(...); }
```
(`MarkdownRenderer.cs:110-121`). `ShouldRender` returns `RequiredSections.Contains(name)` when
`RequiredSections` is non-empty (`MarkdownRenderer.cs:548-551`). **No scenario lists
`"Anti-patterns detected"` or `"Event flow"` in `RequiredSections`** (`ScenarioRegistry.cs`), and
nothing injects `AntiPatterns` when `--include-anti-patterns` is set. Production always passes a
non-empty `RequiredSections` (CLI: `scenario.RequiredSections`; desktop: `GetActiveSections()`),
so **both sections now never render** — `--include-anti-patterns` is dead, and Event flow is gone
from every report (it's part of deep-dive's stated value).

**Fix (recommended — revert the gating; these sections self-gate on detection presence):**
- In `MarkdownRenderer.RenderAsync`, restore both calls to **unconditional** (drop the two
  `ShouldRender` wrappers added in `432fd4f`); keep the `TrackSection` calls. `AppendAntiPatterns`
  already no-ops when there are no `AntiPatternDetection`s (`MarkdownRenderer.cs:854`), and
  `AppendEventFlow` no-ops without event detections — so they only appear when relevant, which is
  the correct behavior and matches pre-`432fd4f`.

**Alternative (only if the team wants these user-selectable):** keep the gates, but (a) add
`SectionNames.EventFlow` to deep-dive's `RequiredSections`, (b) in the CLI (`AnalyzeCommand`) and
desktop (`AnalysisService`/`MainViewModel`) append `SectionNames.AntiPatterns` to the render
`Sections` when `IncludeAntiPatterns` is true, and (c) add both to `SectionSelectionModel.Sections`
so there's a real toggle. This is more surface area; do **not** do it unless selectability is a
goal — the revert is the honest minimal fix.

**Acceptance:** the new regression test in B1.1 passes; `--include-anti-patterns` on a repo with a
known anti-pattern shows the "Anti-patterns detected" section; an event-driven repo shows
"Event flow".

## A2 — Endpoint focus resolves to the handler's real file path

**Problem (R2).** `ResolveEndpointFocusPoints` rewrites the focus to
`new FocusPoint(FocusKind.Method, match.HandlerType, match.HandlerType, match.HandlerMethod)`
(`DiscoveryPipeline.cs:~205`). Arg 2 is `FilePath` — it's being set to a **type name**, so
`PathProximityPruner` path-distance can't work for endpoint-resolved focus (it falls back to
name matching only), weakening the slice.

**Fix.** Look up the handler type in `model.Types` and use its real `FilePath`:
```csharp
var handler = model.Types.Values.FirstOrDefault(t =>
    t.Name == match.HandlerType || t.Id.EndsWith("." + match.HandlerType, StringComparison.Ordinal));
resolved.Add(new FocusPoint(FocusKind.Method,
    handler?.FilePath ?? "", match.HandlerType, match.HandlerMethod));
```
**Acceptance:** B1.2 asserts the resolved focus point carries a non-empty `FilePath` matching the
handler type's file.

## A3 — Remove the premature endpoint "type not found" diagnostic

**Problem (R3).** `ResolveFocusPoints` runs **before** Stage 3; its `failedToResolve` filter now
includes `or FocusKind.Endpoint` (`DiscoveryPipeline.cs:154`). Every endpoint focus has empty
`FilePath` at that point, so it emits a spurious `--around : type not found…` warning (with a null
type name) *before* `ResolveEndpointFocusPoints` runs and emits its own correct "route not found".

**Fix.** Revert `DiscoveryPipeline.cs:154` to `fp.Kind is FocusKind.Type or FocusKind.Method`
(the endpoint resolver owns the endpoint diagnostic).

**Acceptance:** B2.3 asserts no `FocusPointResolver` "type not found" diagnostic is emitted for a
resolvable endpoint focus; the existing not-found test (B1.2) still gets exactly one
`EndpointFocusResolver` warning for an unresolvable route.

## A4 — Make `24h` clone cleanup actually reuse / re-clone

**Problem (R4).** `GitCloneService.IsCloneStale` is defined and unit-tested but **never called in
the clone flow** (CLI or desktop). `--cleanup 24h` (the desktop default "Cache 24h") does no
freshness reuse and no deletion — it's identical to `keep`.

**Fix.** Add a pure, testable decision and consult it before cloning:
```csharp
public enum CloneAction { Clone, Reuse }
public static CloneAction DecideCloneAction(string clonePath, string cleanupMode) =>
    cleanupMode == "24h" && Directory.Exists(clonePath) && !IsCloneStale(clonePath)
        ? CloneAction.Reuse : CloneAction.Clone;
```
- `AnalyzeCommand` (before `CloneAsync`, ~line 63) and `MainViewModel.AnalyzeAsync` (before
  `_git.CloneAsync`, ~line 312): if `DecideCloneAction(clonePath, cleanup) == Reuse`, skip the
  clone and use the existing dir; for `24h` + stale, `Cleanup(clonePath)` first, then clone.
- `ClonePath` is already deterministic per repo+ref, so reuse is safe.

**Acceptance:** B1.3 (decision-table unit tests). If you want an integration test, gate it
`[Trait("Category","Integration")]` so CI without network skips it.

## A5 — CLI `session` cleanup must run at process exit

**Problem (R5).** `RegisterForSessionCleanup` adds to a static bag, but `CleanupSession()` is only
called in desktop `Dispose()`. The CLI never drains it, so `--cleanup session` leaks the clone.

**Fix.** In `Program.cs`, drain after the app runs:
```csharp
var exit = await app.RunAsync(args);
GitCloneService.CleanupSession();
return exit;
```
Note in code/docs: for a single-shot CLI, `session` ≈ `auto` (cleans at end of the run); the
modes differ meaningfully only in the long-lived desktop.

**Acceptance:** covered by B1.3 plus a note; process-exit itself isn't unit-tested (don't try).

## A6 — Resolve the undocumented `MaxPathDistance` change

**Problem (R6).** `ScenarioRegistry` changed deep-dive `MaxPathDistance` from `1` → `3`
(`ScenarioRegistry.cs:24`), unrelated to any task and contradicting the handover's "no behavior
change except F9." This widens what a deep-dive slice pulls in.

**Fix.** Decide intentionally:
- If intended (likely — `1` was probably too tight after the PathProximity name-fallback landed):
  keep it, **and** add B1.4 pinning deep-dive's default pruning config so the value is asserted,
  and call it out in the PR description.
- If accidental: revert to `1`.
Either way it must be a deliberate, tested value — not an unexplained drive-by.

## A7 — Stop routing `--task` into focus

**Problem (R7).** `IntentInput.Focus = settings.Task` (`AnalyzeCommand.cs:79`) still turns a
free-text `--task` string into a bogus type/endpoint focus point. Only the `Program.cs` example
was removed.

**Fix.** Set `Focus = null` (drop `settings.Task` from the intent); keep the existing
`--task is deprecated` warning (`AnalyzeCommand.cs:~103`).

**Acceptance:** B1.5.

---

# PART B — Tests to ADD (real value)

> Principle: a test earns its place only if it can **fail when the behavior breaks**. Every test
> below targets a defect this PR introduced or a feature with zero real coverage.

### B1.1 — R1 regression lock (Core/`RendererTests`) — **highest value**
`MarkdownRenderer renders AntiPatterns and EventFlow under a realistic non-empty RequiredSections`:
build a model with an `AntiPatternDetection` and an event detection, render with
`RequiredSections` set to a deep-dive-like list that **does not** contain "Anti-patterns detected"
or "Event flow" (e.g. `["Endpoints","Call graph"]`), and assert both section headers appear. This
is the test the suite was missing — it fails today and passes after A1.

### B1.2 — Endpoint focus resolution through the real pipeline (Core/`EndpointFocusResolutionTests`, rewrite)
Replace the reflection-based tests (see B3.1) with behavioral ones driven through
`DiscoveryPipeline.AnalyzeAsync` using a **fake Stage-3 extractor** that adds a known
`EndpointDetection` (+ a matching `TypeDiscovery` for the handler so A2's file-path lookup
resolves). Assert: resolved focus is `FocusKind.Method` with the right `TypeName`/`MethodName`
**and a non-empty `FilePath`** (locks A2); an unmatched route leaves the focus unchanged and emits
exactly one `EndpointFocusResolver` warning.

### B1.3 — Clone-action decision table (Core/`GitCloneServiceTests`)
After A4: `DecideCloneAction` returns `Reuse` for `24h` + fresh dir; `Clone` for `24h` + stale,
for missing dir, and for `auto`/`session`/`keep`. Pure, no network. This replaces the current
isolated `IsCloneStale` tests (B2.4) with a test of the behavior users actually get.

### B1.4 — Pin deep-dive default pruning (Core/`ScenarioRegistryTests` or `AnalysisIntentResolverTests`)
Assert deep-dive's default `Pruning` (`MaxPathDistance`, `MaxCallDepth`, `MaxSurvivingTypes`,
`RoleWeight`, `FocusWeight`) with **no** `--depth`. Locks A6 and makes any future scoring tweak a
conscious, reviewed change. (Existing depth tests only cover the `--depth` override path.)

### B1.5 — `--task` no longer creates a focus point (Core/`CliSmokeTests` or resolver test)
After A7: `IntentInput` built the way `AnalyzeCommand` builds it from `--task "fix login"`
(Focus=null) yields `FocusPoints.Length == 0` and overview scenario. Keep the deprecation-warning
assertion if a command-level harness exists.

### B1.6 — Section toggle propagates to the render request (Desktop/`MainViewModelTests`)
The desktop tests mock `IAnalysisService`, so they can't verify real filtering — but they **can**
verify the wire. Capture the `RenderRequest` passed to `_svc.RenderAsync` and assert that after
`vm.SetSectionEnabled(Endpoints, false)` the next `RenderRequest.Sections` **excludes** Endpoints
(and re-includes on toggle-on). This is the meaningful Task-1 desktop test; pair it with the Core
renderer filter test (existing `MarkdownRenderer_RequiredSections_FiltersOutput`, B2.1) which
proves the renderer honors `Sections`.

### B1.7 — CallGraph/`__source__` toggles are analysis-tier (Desktop/`MainViewModelTests`)
After existing wiring: with output shown, `vm.SetSectionEnabled(CallGraph, true)` on an uncached
profile sets `IsStale == true` and does **not** call `_svc.AnalyzeAsync`; a plain section toggle
(e.g. DI registrations) calls `RenderAsync` only and leaves `IsStale == false`. Locks Task 2's
tier split, which currently has only indirect coverage.

---

# PART C — Tests to UPDATE (repair "test theater")

> These exist and pass, but their **bodies don't assert what their names promise** — they give
> false confidence. Fix the assertion, not just the name.

### C-update.1 — `RendererTests.AntiPatterns_GroupedByFile_WhenMultipleFiles` (`:390`)
Uses `RenderOptions(false,false,8000)` (empty `RequiredSections` ⇒ `ShouldRender` always true), so
it can't catch the A1 regression. After A1, change it to pass a realistic non-empty
`RequiredSections` so it exercises the production path, or fold it into B1.1.

### C-update.2 — `MainViewModelTests.Toggling_section_updates_DisplayText_in_Llm_view` (`:528`)
Body never toggles a section and never checks `LlmViewText`/`DisplayText` — it only asserts a
toggle exists and `IsEnabled`. Rewrite to actually toggle and assert via B1.6's captured
`RenderRequest`, or delete in favor of B1.6.

### C-update.3 — `MainViewModelTests.Toggling_section_updates_token_total` (`:555`)
Captures `beforeTotal` but never compares an `afterTotal`; only asserts `IsEnabled` flags. Make it
assert `SelectedTokenTotal` actually decreases when a section is disabled (the token-math behavior
it claims to cover).

### C-update.4 — `MainViewModelTests.LlmViewText_excludes_disabled_sections` (`:783`)
Name claims LLM-view exclusion but the body only checks toggle flags + drawer presence; with a
mocked service `LlmViewText` is constant. Either assert the wire (B1.6 style) or delete — the
"excludes disabled sections" guarantee now lives in the renderer (B2.1), not the VM.

### C-update.5 — `SnapshotCacheTests.Render_params_are_excluded_from_key_equality` (`:126`)
Render params aren't in `AnalysisKey` at all, so the test just compares two identical keys. Rename
to `Identical_keys_are_equal`, and add a sibling asserting keys differing only in `Profile`
(the Task-2 field) are **not** equal — that's the property with real risk.

---

# PART D — Tests to DELETE / REWRITE

### D.1 — `EndpointFocusResolutionTests` reflection bodies → delete, replaced by B1.2
The three current tests invoke the private static `ResolveEndpointFocusPoints` via
`BindingFlags.NonPublic` (the file even comments that it's a hack) and are `async Task` with no
`await`. They test an implementation detail, not the pipeline contract, and won't notice if the
method stops being called after Stage 3. Delete and replace with the behavioral B1.2.

### D.2 — Audit `PipelineTests` reflection usage
`PipelineTests` also uses reflection (grep hit). Confirm whether it pokes private members; if so,
re-express against the public `AnalyzeAsync`/snapshot surface. Don't delete coverage — convert it.

### D.3 — Retire the now-isolated `IsCloneStale` tests
Once B1.3 covers `DecideCloneAction`, the standalone `IsCloneStale_returns_*` tests are redundant
detail (the helper is exercised through the decision). Keep one boundary test (missing dir ⇒
stale) if cheap; drop the rest.

---

# PART E — Organization & housekeeping (whole solution)

Current state: **326 tests across 47 files** (Core 39 files, Desktop 5, + helpers). Generally
healthy and well-named; the issues are localized.

1. **DRY the empty `RunReport`.** The 11-line empty/placeholder `RunReport` literal is duplicated
   across **7 files** — src: `DiscoveryPipeline`, `AnalyzeCommand`, `AnalysisService`,
   `RunReportCollector`; tests: `RankingTests`, `MainViewModelTests`, `SnapshotCacheTests`. Add a
   `public static RunReport Empty(int typeCount = 0)` factory on `RunReport` and replace all 7.
   Pure cleanup, no behavior change; shrinks every new test that needs a snapshot.

2. **Split `MainViewModelTests` (55 tests, ~1090 lines).** Break into cohesive partial classes /
   files: `MainViewModelAnalyzeTests`, `MainViewModelCacheAndStaleTests`,
   `MainViewModelSectionTests`, `MainViewModelRenderTier Tests`. Easier to navigate and to place
   B1.6/B1.7. Move the shared NSubstitute `_svc` setup + `DefaultReport` into a small base/helper.

3. **CS1998 audit.** Several `public async Task` tests have no `await` (the reflection endpoint
   tests are the clearest). After D.1/D.2, sweep for warnings and make genuinely-sync tests
   `[Fact] void` (or add the real `await`). Build with warnings-as-errors locally once to surface
   them.

4. **Static mutable state in `GitCloneService`.** `_sessionClones` (static `ConcurrentBag`) makes
   session tests order-/parallel-sensitive and bleeds across the suite. Prefer instance-scoped
   session tracking owned by the desktop VM, or expose a test-only reset. At minimum, keep all
   session tests in one class (sequential within a class) and wrap temp-dir creation in
   `try/finally` so a failed assert doesn't leak temp directories.

5. **Renderer tests bypass the filter (17×).** 17 `RendererTests` use `RenderOptions(false,false,
   8000)` (no `RequiredSections`) — fine for "does X render at all" but they don't cover the
   section-filter contract. Beyond C-update.1, add one parameterized test asserting that each
   major section is suppressed when absent from `RequiredSections` and present when included
   (extends `MarkdownRenderer_RequiredSections_FiltersOutput`, `:364`). Prevents future
   F9-style regressions for *any* section.

6. **No solution file.** Add a `DevContext.sln` (or `dotnet-test.proj`) covering Core+Desktop
   (+Cli/Roslyn) so `dotnet test DevContext.sln` works and CI/handovers stop referencing a
   non-existent file. Exclude `eval-repos/**` and `benchmarks/**` from the test glob.

7. **`SnapshotCacheTests.MakeSnapshot(label)`** ignores `label` (all snapshots identical). Drop
   the unused parameter or make snapshots distinguishable; minor.

---

# Final verification gate

1. `dotnet test tests/DevContext.Core.Tests/...` and `.../DevContext.Desktop.Tests/...` — green.
2. Net test count ≥ 326 (B1 adds ~8–10; C/D update/replace, not net-remove coverage).
3. The A1 regression test (B1.1) and the profile-key test (C-update.5) **fail on `develop`/`432fd4f`
   and pass after the fix** — verify by stashing the renderer change.
4. CLI manual: `--include-anti-patterns` on a repo with a known anti-pattern shows the section;
   `-f "GET /<route>"` on TodoApi pins the handler (appears near the top of output); `--cleanup
   24h` reuses a fresh clone (log: no re-clone), re-clones when stale.
5. PR description updated: list A6's `MaxPathDistance` decision explicitly; correct the build/test
   commands (no `.sln` unless E.6 is done).
