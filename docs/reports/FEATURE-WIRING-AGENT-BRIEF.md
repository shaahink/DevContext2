# Agent Brief — Fix Feature Wiring Defects (CLI & Desktop)

> Executable companion to `FEATURE-WIRING-AUDIT.md`. Each task is self-contained: problem,
> exact files/lines, the change, and acceptance. Do tasks in the given order; build + test after
> each. **Do not** change the PLAN-1 analyze→render split, golden output, or CLI semantics beyond
> what a task explicitly says.

## Ground rules

- Solution: `dotnet build DevContext.sln`. Core tests: `dotnet test tests/DevContext.Core.Tests`.
  Desktop tests: `dotnet test tests/DevContext.Desktop.Tests`.
- The desktop UI is **Blazor in a `BlazorWebView`** (not WPF XAML). Components subscribe to
  `VM.PropertyChanged` and call `InvokeAsync(StateHasChanged)` ignoring `e.PropertyName`. The VM
  is a DI **singleton** (parameterless ctor; `IAnalysisService` is not registered), so a field on
  the VM is session-scoped.
- Razor comments are `@* *@`. The `__source__` section key is a sentinel, **not** a `SectionNames`
  entry; keep it as-is.
- Before renaming/removing any public VM member, grep the solution **including `.razor`**.
- A claude-mem plugin hook is currently failing in this workspace and may block the `Read` tool;
  if so, use `Grep` (it still returns content) or fix/disable the hook first. This is environment
  noise, unrelated to the code.

---

## TASK 1 (P1) — Make desktop section toggles actually filter the output

**Problem (F1+F2).** The `OutputPanel` "Sections" drawer toggles `SectionViewModel.IsIncluded`,
which only updates the token tally; the rendered output comes from `GetActiveSections()`
(System A), which the drawer never touches. And `ConfigPanel.razor:112` has an empty "Sections"
block — there is no live control for System A at all. The token counter/budget bar thus lie.

**Files**
- `src/DevContext.Desktop/ViewModels/SectionSelectionModel.cs`
- `src/DevContext.Desktop/ViewModels/MainViewModel.cs`
- `src/DevContext.Desktop/Components/OutputPanel.razor`
- `tests/DevContext.Desktop.Tests/MainViewModelTests.cs`

**Pick one approach (recommend A):**

**A — Wire the drawer to the real filter (restores the feature).**
1. Give `SectionViewModel` a `Key` (the `SectionNames` constant) in addition to `Name`. Populate
   it in `BuildSectionDataFromStat` by mapping `SectionStat.Name` → key (you already map keys to
   labels in `SectionSelectionModel.Sections`; invert that, or carry the key on `SectionStat`).
2. When a `SectionViewModel.IsIncluded` flips, call back into the VM to
   `SetSectionEnabled(section.Key, included)` (System A) and re-render. The existing
   `MainViewModel.SetSectionEnabled` → `OnRenderInputChanged()` already re-renders correctly;
   the missing link is the drawer→`SetSectionEnabled` call. Route it through the existing
   `OnSectionChanged` callback (`SectionSelectionModel.cs:156`) rather than adding a new event.
3. Keep the two states in sync so the drawer checkbox reflects `Sections[key].IsEnabled` after a
   scenario reset (`ApplyScenarioSectionDefaults`).
4. Restore a minimal section list in `ConfigPanel.razor` under the `@* ── Sections ── *@` header
   (checkbox per `VM.Sections` item, `@onchange` → `VM.SetSectionEnabled(item.Key, value)`),
   **including** the `__source__` "Source code" item — or rely solely on the drawer if you prefer
   one location. Whichever you choose, there must be exactly one working control surface.

**B — If you only want the counter honest (smaller, less valuable).** Remove the per-section
checkboxes from the drawer and relabel it "Token breakdown (estimate)", and stop claiming
sections can be dropped there. Not recommended — it deletes a feature instead of fixing it.

**Acceptance**
- Unchecking a section removes that `## section` from Human view, LLM view, Copy, Copy-for-LLM,
  and Save — verified by string assertion on `VM.RawContent` before/after a toggle.
- The budget bar / `Analyze (~N tok)` number matches the actual content (recompute from the new
  render's `SectionStat`s).
- New unit test in `MainViewModelTests`: toggling a section calls `RenderAsync` and changes
  `RawContent`; `AnalyzeAsync` is **not** called.

---

## TASK 2 (P2) — Make profile part of analysis identity (cache + staleness + re-analyze)

**Problem (F8).** `DerivedProfile` (source→`full`, callgraph→`debug`, else `focused`) is an
**analyze-time** input (gates `CallGraphExtractor`/`SourceBodyExtractor`) but is absent from
`AnalysisKey`, and the section toggles that change it are wired as render-tier. After Task 1 this
becomes live: enabling Source code / Call graph must re-analyze, not re-render.

**Files**
- `src/DevContext.Desktop/ViewModels/SnapshotCache.cs` (`AnalysisKey`)
- `src/DevContext.Desktop/ViewModels/MainViewModel.cs` (`BuildAnalysisKey`, section handlers)

**Change**
1. Add `string Profile` (or `bool CallGraph, bool Source`) to `AnalysisKey` and include
   `DerivedProfile` in `BuildAnalysisKey()` (`MainViewModel.cs:102-103`).
2. Classify the **profile-affecting** section toggles (`CallGraph`, `__source__`) as
   **analysis-tier**: when they change, call `MarkAnalysisInputsChanged()` (cache-probe-or-stale)
   instead of `OnRenderInputChanged()`. Non-profile section toggles stay render-tier.
   - Concretely: in `MainViewModel.SetSectionEnabled`, branch on whether `key` affects profile
     (`key == SectionNames.CallGraph || key == "__source__"`) → `MarkAnalysisInputsChanged()`,
     else → `OnRenderInputChanged()`.

**Acceptance**
- With output shown, enabling "Call graph" (or "Source code") on a never-analyzed profile sets
  `IsStale == true` and does **not** auto-run the pipeline; clicking Re-analyze runs exactly once
  and the call graph / source now appears.
- Switching back to a previously-analyzed profile serves from cache instantly (no pipeline run).
- Unit tests mirroring the PLAN-8 Phase 6 matrix, extended for the profile dimension.

---

## TASK 3 (P1/P2) — Honor multiple focus points (or stop advertising it)

**Problem (F3).** `--focus`/`--around` are `string[]` ("Repeatable") but only `focusInput[0]` is
used (`AnalyzeCommand.cs:76-77`); `IntentInput.Focus` is a single string.

**Files**
- `src/DevContext.Core/Configuration/AnalysisIntentResolver.cs` (`IntentInput`, `Resolve`)
- `src/DevContext.Cli/Commands/AnalyzeCommand.cs`
- `src/DevContext.Desktop/Services/AnalysisService.cs` (builds `IntentInput`)
- tests: `FocusPointParserTests` / `AnalysisIntentResolverTests`

**Change (recommended: support multiple)**
1. Change `IntentInput.Focus` (single) to `Focuses` (`IReadOnlyList<string>`), or add a second
   field, keeping a single-value convenience for desktop.
2. In `Resolve`, loop over all focus strings, parse each (endpoint vs type vs type:method), and
   append every produced `FocusPoint`. Scenario derivation = deep-dive if **any** focus present.
3. CLI passes the full `settings.Focus ?? settings.Around` array; desktop passes its single
   `Around` as a one-element list.

**Alternative (smaller):** drop "Repeatable" from the `[Description]` and warn if `>1` supplied.

**Acceptance**
- `-f A -f B` yields two `FocusPoints`; both seed `PathProximityPruner`/`CallReachabilityPruner`
  and both are pin-eligible in `RenderPlanBuilder`.
- Existing single-focus goldens unchanged.

---

## TASK 4 (P2) — Resolve endpoint focus to its handler (or stop claiming it)

**Problem (F4).** `FocusKind.Endpoint` is parsed and the explanation says "handler resolved after
scan", but nothing resolves it: `FocusPointResolver` ignores `Endpoint`, and `ResolveFocusPoints`
runs before Stage 3 where `EndpointDetection`s are produced.

**Files**
- `src/DevContext.Core/Pipeline/DiscoveryPipeline.cs` (`AnalyzeAsync`, `ResolveFocusPoints`)
- `src/DevContext.Core/Resolvers/FocusPointResolver.cs`
- tests: `PipelineTests` / a new focus-resolution test using an eval repo with a known route

**Change (recommended: implement)**
1. After Stage 3 completes (`DiscoveryPipeline.cs:103`), add an endpoint-resolution pass: for each
   `FocusKind.Endpoint` focus point, find the matching `EndpointDetection` by `(HttpMethod, Route)`
   (normalize route; method optional) and rewrite the focus point to a resolved
   `FocusKind.Method`/`Type` carrying the handler's `TypeName`/`FilePath`. Then the existing
   path/graph pruners (which run in scoring, after Stage 3) will pick it up. Note current scoring
   happens in `RunScoringAsync` after Stage 3, so the resolved `TypeName` will be in place.
2. Emit a "route not found" diagnostic (mirror the type-not-found path at
   `DiscoveryPipeline.cs:158-167`) when no endpoint matches, and include `Endpoint` in that
   failed-resolution check (`:154`).

**Alternative (smaller):** if endpoint slicing is out of scope now, change the explanation string
(`AnalysisIntentResolver.cs:108`) to stop promising resolution, and document the limitation.

**Acceptance**
- `-f "GET /todos"` on TodoApi pins/【scores】the route's handler type (assert it survives into
  output and ranks near the top); a bogus route emits a diagnostic and falls back cleanly.

---

## TASK 5 (P2) — Implement or collapse clone-cleanup modes

**Problem (F6).** Only `auto` is handled; `session`/`24h`/`keep` are identical no-ops, yet the
desktop defaults to `24h` ("Cache 24h").

**Files**
- `src/DevContext.Core/Services/GitCloneService.cs` (verify `ClonePath` determinism + any age logic)
- `src/DevContext.Cli/Commands/AnalyzeCommand.cs:208-213`
- `src/DevContext.Desktop/ViewModels/MainViewModel.cs:377-382`

**Decide**
- **Implement:** `24h` = reuse existing clone if `< 24h` old else re-clone (needs deterministic
  `ClonePath` + mtime check); `session` = delete on app/process exit (`Dispose` for desktop;
  process-exit hook for CLI); `keep` = never delete; `auto` = delete now (current behavior).
- **Or collapse:** reduce the option set to `auto | keep`, change the desktop default, and update
  `AnalyzeSettings`/`ConfigPanel` copy.

**Acceptance** documented behavior for each accepted value, plus a test for the chosen `24h`
reuse path if implemented.

---

## TASK 6 (P3) — Remove dead pruner + dead score fields

**Problem (F5).** `TokenBudgetEnforcer` is unregistered (dead); `RelevanceScore` is never assigned;
`IsPruned` is never set true. They duplicate `RenderPlanBuilder`'s budget/cap with a broken,
pre-`FinalScore` formula and are a re-introduction hazard.

**Files**
- `src/DevContext.Core/Pruning/TokenBudgetEnforcer.cs` (delete)
- `src/DevContext.Core/Models/TypeDiscovery.cs` (remove `RelevanceScore`, `IsPruned`)
- all six `src/DevContext.Core/Compression/*.cs` (drop the `type.IsPruned ||` guards, keep
  `IsHardExcluded`)
- `tests/DevContext.Core.Tests/PrunerTests.cs` / `RankingTests.cs` (remove enforcer-specific tests)

**Change.** Delete the file and the two fields; simplify the compression guards. Confirm via grep
that nothing else references `TokenBudgetEnforcer`, `RelevanceScore`, or `IsPruned`. Budget + cap
remain enforced solely by `RenderPlanBuilder`.

**Acceptance.** Build + full test suite green; goldens unchanged (the enforcer never ran, so output
must be byte-identical).

---

## TASK 7 (P3) — Polish

- **F9:** wrap `AppendAntiPatterns` / `AppendEventFlow` in `ShouldRender(...)` guards (add
  `AntiPatterns`/`EventFlow` to the relevant scenarios' `RequiredSections` if they should show by
  default), so the section filter is consistent. Keep CLI golden output stable — verify.
- **F10:** stop routing `--task` into `Focus`; keep only the deprecation warning
  (`AnalyzeCommand.cs:81,103-104`).
- **F11:** debounce `OnProjectPathChanged` validation or switch `ConfigPanel.razor:50` to
  `onchange`.
- **F12:** delete the unused `rootPath` parameter of `AddDevContextServices`
  (`ServiceRegistration.cs:5`) **or** key `AnalysisService._cachedPipeline` by root — pick one and
  remove the ambiguity.
- Doc: `--format` accepts `html` but the help says `markdown | json`; align help with reality.

**Acceptance.** Build + tests green; CLI goldens unchanged unless a task explicitly changes output.

---

## Verification checklist (run at the end)

1. `dotnet build DevContext.sln` clean.
2. `dotnet test` (Core + Desktop) green.
3. Desktop manual (per PLAN-8 G-desktop, extended):
   - Toggle a section → output **content** changes, token bar matches (Task 1).
   - Enable Call graph / Source code → stale banner → Re-analyze → data appears (Task 2).
   - `-f` with two values (CLI) → both focuses reflected (Task 3).
   - `-f "GET /route"` → handler sliced or honest "not implemented" message (Task 4).
4. CLI goldens unchanged except where a task intentionally alters output (Task 7/F9).
