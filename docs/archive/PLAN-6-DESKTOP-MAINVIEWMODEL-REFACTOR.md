# Plan 6 — Desktop `MainViewModel` correctness + decomposition

> Source: review of `src/DevContext.Desktop/ViewModels/MainViewModel.cs` (752 lines) on
> `develop`, traced through its three Razor consumers (`ConfigPanel`, `OutputPanel`,
> `StatusBar`), `Services/AnalysisService.cs`, and `tests/DevContext.Desktop.Tests`.
>
> `MainViewModel` is a god class: it owns form state, GitHub URL validation + cloning,
> analysis orchestration, render orchestration, section/profile mapping, section-VM
> construction + token math, dual-view text caching, stats rendering, and settings
> persistence — nine responsibilities, four hand-rolled `CancellationTokenSource` fields,
> and a `OnPropertyChanged(string.Empty)` "refresh everything" batching hack guarded by
> `#pragma warning disable MVVMTK0034`.
>
> **Hosting model that matters for every fix below:** the UI is Blazor running inside a
> `BlazorWebView` (`MainWindow.xaml`), *not* WPF XAML bindings. Components subscribe to
> `VM.PropertyChanged` and call `InvokeAsync(StateHasChanged)`. The VM is a **singleton**
> (`MainWindow.xaml.cs:43`), so component subscriptions accumulate across the app lifetime
> and any per-component leak is permanent.
>
> Phases are ordered by severity and are independently committable. **Fix the correctness
> bugs (Phase 1–2) before the decomposition (Phase 3+)** — they are small, shippable, and
> give the refactor a green baseline. Build + `dotnet test tests/DevContext.Desktop.Tests`
> after every phase.

---

## Defect register

### Critical — correctness

- **C1. A superseded analysis disposes the *current* analysis's CTS and clobbers the UI.**
  `AnalyzeAsync` re-entrancy is reachable: the Analyze button stays enabled while analyzing
  (`ConfigPanel.razor:176` disables only on empty path), and every option change calls
  `AnalyzeCommand.Execute(null)` directly (`MainViewModel.cs:323`), which runs the body
  unconditionally (ICommand.Execute does not gate on `CanExecute`). When run B supersedes
  run A, B calls `CancelPrevious()` (cancels A's `_cts`, sets `_cts = null`) then assigns
  `_cts = new` (B's). `AnalysisService.AnalyzeAsync` swallows `OperationCanceledException`
  and returns `{ Success = false, Error = "Cancelled" }` (`AnalysisService.cs:161-164`), so
  A's `await` *returns* rather than throws → A falls into the `else` branch and writes
  `ProgressText = "Error"`, `_rawContent = "Cancelled"`, `HasOutput = true`
  (`MainViewModel.cs:482-489`), then its `finally` runs `_cts?.Dispose(); _cts = null;`
  (`:510-511`) — **disposing B's CTS and nulling the field.** Consequences: (a) the output
  flashes "Cancelled"/"Error" mid-edit; (b) B now runs with a disposed token source and an
  orphaned `_cts == null`, so a *third* change's `CancelPrevious()` finds `null` and cannot
  cancel B — B becomes an uncancelable zombie. Note `RerenderAsync` already has the correct
  identity guard (`if (_renderCts == cts)`, `:587`); `AnalyzeAsync`'s `_cts` path never got
  it. Neither the `else` branch nor the `catch` branch checks `ct.IsCancellationRequested`
  before writing output (only the success branch does, `:457`).

- **C2. JSON output format is non-functional in the desktop app.** `RerenderAsync` builds a
  `RenderRequest { Format = SelectedFormat }` (`MainViewModel.cs:538`) — possibly `"json"` —
  but `AnalysisService.RenderAsync` ignores it and renders markdown + html unconditionally
  (`AnalysisService.cs:171-172`, `request with { Format = "markdown" }` / `{ Format = "html" }`).
  `RenderResult.Content` is therefore always markdown. Selecting **JSON** and Save writes
  markdown into a `.json` file (`OutputPanel.razor:29-31`). The whole JSON code path is dead.

### High

- **H1. Component `PropertyChanged` unsubscribe is a no-op → permanent handler leak.**
  `OutputPanel.razor:15-17` and `StatusBar.razor:8-10` add an anonymous lambda in
  `OnInitialized` and try to remove a *different* lambda instance in `Dispose`
  (`VM.PropertyChanged -= (_, _) => InvokeAsync(StateHasChanged)`). Delegates compare by
  identity, so nothing is removed. Against a singleton VM this means every recreated
  component leaks a subscription that keeps calling `StateHasChanged` on a disposed
  component (and pins it in memory). `ConfigPanel.razor:9,32` does it correctly with a named
  method (`OnVmChanged`) — copy that pattern.

- **H2. The displayed token-budget denominator goes stale after a slider change.**
  `BudgetTokens` (the "/ N tokens" denominator in `OutputPanel.razor:107`) is only assigned
  in `AnalyzeAsync` (`MainViewModel.cs:468`, from `capturedBudget`). The MaxTokens slider
  triggers `DebouncedReanalyze` → `OnRenderInputChanged` → `RerenderAsync`, which re-renders
  with the new `MaxTokens` and updates `_totalTokens`/`_selectedTokenTotal` (`:562-563`) but
  **never updates `_budgetTokens`**. So after dragging the slider the budget bar shows the
  new usage against the *old* budget denominator until the next full analysis.

- **H3. Every re-render does double work (markdown + html) regardless of view or format.**
  `AnalysisService.RenderAsync` always calls `pipeline.RenderAsync` twice
  (`AnalysisService.cs:171-172`). `RerenderAsync` fires on format change, provenance /
  diagnostics toggle, and the debounced MaxTokens slider — each pays for both renders even
  when the user is on the LLM tab (raw markdown only) or has JSON selected. On large repos
  this is the main interactive-lag source.

### Medium

- **M1. Two sources of truth for the active output tab; the "jump to LLM on edit" is dead.**
  `OutputPanel` tracks its own `SelectedTab` local field (`OutputPanel.razor:8`) and renders
  from it (`DisplayOutput`, `:52`). The VM *also* tracks `IsHumanView` (`:76`) and a derived
  `DisplayText`/`RefreshDisplayText` (`:101,108`) that **no component binds**. The section
  `PropertyChanged` handler does `if (IsHumanView) IsHumanView = false;`
  (`MainViewModel.cs:665`) intending to jump the user to the LLM view when they toggle a
  section — but because the panel reads its own `SelectedTab`, this flip is invisible. Net:
  confusing duplicated state and a feature that silently does nothing.

- **M2. GitHub URL validation fires a network call on every keystroke.** `ProjectPath` binds
  with `@bind:event="oninput"` (`ConfigPanel.razor:50`), so each character runs
  `OnProjectPathChanged` → `ValidateGitHubUrlAsync` (`MainViewModel.cs:145-192`), which (for
  anything parsing as a valid repo URL) shells out to `_git.ValidateAsync` (a `git ls-remote`).
  The only throttle is cancel-the-previous; there is no debounce delay, so typing a URL spawns
  and cancels a burst of git processes. Add a delay debounce like `DebouncedReanalyze`.

- **M3. Large blocks of VM state never reach the UI (dead weight + false batching cost).**
  Confirmed by grepping the `.razor` consumers: `DisplayText`, `LlmViewText` driving
  `DisplayText`, `IsHumanView` (read), `BudgetUtilisation` (`:97`), `TotalTokens` (the VM
  one), `ProgressValue` + `IsProgressIndeterminate` (`:69-70`), and `IsSectionPanelVisible`
  (`:77`) are never read by a component. The progress reporter computes a determinate
  `ProgressValue` (`:439-443`) but the UI only shows an indeterminate spinner + `ProgressText`
  (`OutputPanel.razor:146`). Either wire these or delete them; today they inflate the
  `OnPropertyChanged(string.Empty)` surface and the class.

### Low

- **L1. `OnPropertyChanged(string.Empty)` + `MVVMTK0034` suppressions.** The "set backing
  fields directly, then notify-all" hack (`:462-472`, `:556-578`) works in Blazor (empty name
  = re-render everything) but defeats `[NotifyPropertyChangedFor]`/`[NotifyCanExecuteChangedFor]`
  granularity and is fragile. After decomposition, child VMs with normal property setters
  remove the need for it.
- **L2. `DebouncedReanalyze` couples the VM to WPF and behaves differently under test.** It
  uses `Task.Run` + `System.Windows.Application.Current?.Dispatcher.Invoke`
  (`:344-360`); with no `Application.Current` (unit tests) it runs `RerenderAsync` on a
  thread-pool thread, mutating VM state and firing `PropertyChanged` off-thread. Replace with
  a UI-thread timer / captured `SynchronizationContext`. The name is also a misnomer — it
  re-*renders*, it does not re-analyze.
- **L3. `_snapshot` is shared mutable state read/written by overlapping `AnalyzeAsync` /
  `RerenderAsync` with no synchronization.** Harmless only because everything is *supposed*
  to run on the UI thread; C1's re-entrancy and L2's off-thread path break that assumption.
- **L4. `ShowToast` is `async void` and `_toastCts` is canceled but never disposed**
  (`OutputPanel.razor:36-46`). Component-local, minor.

---

## Target architecture

Decompose the god class into a thin composition root plus focused collaborators. Keep each
collaborator independently testable (no WPF/Blazor types). Suggested split:

| New type | Owns (moved out of `MainViewModel`) |
| --- | --- |
| `AnalysisRunner` | `AnalyzeAsync` + `RerenderAsync` lifecycle, the single source of cancellation, supersede/restart semantics. Exposes `Task<…> RunAsync(opts, progress, ct)` / `RerenderAsync(request, ct)` and raises typed results. **All four `CancellationTokenSource` fields collapse into one reusable `CancellableOperation` helper** (cancel-previous + identity-guarded dispose, the `:587` pattern applied everywhere). |
| `GitHubSourceModel` | `_gitRepoUrl`, `_gitRepoStatus`, `IsGitHubUrl`, `GitRepoDisplay`, `ValidateGitHubUrlAsync` (with a real debounce, fixes M2), clone orchestration + `CloneCleanup` policy. |
| `SectionSelectionModel` | the `Sections` toggle list, scenario presets (`ApplyScenarioSectionDefaults`), `DerivedProfile`, `GetActiveSections`, `CategorizeSection`, `BuildSectionDataFromStat`, `RecalcTokenTotal`, `SectionGroups`. Pure logic + token math; already has good test coverage to lean on. |
| `OutputViewModel` | the rendered output + **single** view-tab source of truth (`RawContent`/`HumanViewHtml`/`LlmViewText`/`StatsHtml`/`SelectedTab`). Fixes M1 by moving `SelectedTab` off `OutputPanel`. Delete `DisplayText`/`RefreshDisplayText`/`IsHumanView` duplication (M3). |
| `MainViewModel` (thin) | wires the children, exposes commands, owns plain form fields + settings/recent persistence. |

Cross-cutting helpers to introduce:
- `CancellableOperation` — wraps cancel-previous + start + identity-guarded disposal; replaces
  `_cts`, `_renderCts`, `_validateCts`, `_maxTokensDebounceCts` and kills C1 structurally.
- `Debouncer` — a UI-thread debounce (captured `SynchronizationContext`, no WPF dependency);
  used by both the MaxTokens slider (L2) and URL validation (M2).

---

## Phased plan

Each phase is one commit. Run `dotnet build` + `dotnet test tests/DevContext.Desktop.Tests`
after each; the existing suite already covers analyze lifecycle, sections, debounce, and
batched notifications, so it is your regression net.

### Phase 1 — Cancellation + re-entrancy correctness (fixes C1) `[bug]`
1. Give `AnalyzeAsync`'s `_cts` the same identity guard `RerenderAsync` already uses: in the
   `finally`, only dispose/null if `_cts` is still the instance this invocation created
   (capture `var myCts = _cts;` after assignment, then `if (_cts == myCts) { … }`).
2. Guard the `else` and `catch (Exception)` branches with `if (ct.IsCancellationRequested) return;`
   before they write `HasOutput`/`_rawContent`/`ProgressText`, so a canceled run never paints
   "Cancelled"/"Error" over a live one.
3. Decide and document the re-entrancy contract: either (a) set
   `[RelayCommand(AllowConcurrentExecutions = true, …)]` and keep explicit
   cancel-previous, or (b) switch to the toolkit's `IncludeCancelCommand` and let it own the
   token. Don't leave it implicit.
4. New tests: rapid supersede leaves the *final* run's output (not "Cancelled"); a third
   change still cancels the second run (no zombie).

### Phase 2 — Output correctness (fixes C2, H1, H2, H3) `[bug]`
1. **C2/H3:** make `AnalysisService.RenderAsync` honor `request.Format`. Render the raw
   content in the requested format; render HTML *only* when the human view needs it (markdown
   path), and skip the second render for JSON. The desktop only needs HTML for the
   markdown-human view, so gate it.
2. **H1:** replace the lambda `-=` in `OutputPanel.razor` and `StatusBar.razor` with a named
   handler field stored in `OnInitialized` and removed in `Dispose` (mirror `ConfigPanel`).
3. **H2:** update `_budgetTokens` in `RerenderAsync` (or whatever owns the budget after
   decomposition) so the denominator tracks the budget actually rendered with.
4. Tests: JSON format produces JSON content + `.json` save; component dispose actually
   detaches (assert handler count / no callback after dispose); budget denominator updates
   after a simulated slider re-render.

### Phase 3 — Extract `SectionSelectionModel` `[refactor]`
Move the section/profile/category/token-math members out behind a child object on the VM
(keep public property names stable so Razor bindings and the existing section tests don't
churn — e.g. `VM.SectionGroups` can forward to `VM.Sections.Groups`, or update the three
bindings in `OutputPanel.razor`). This is the lowest-risk extraction because the logic is
pure and already well tested.

### Phase 4 — Extract `OutputViewModel` + collapse the dual-view state (fixes M1, M3) `[refactor]`
1. Make the active tab a single property on the new output VM; bind `OutputPanel`'s tab
   buttons to it; delete the panel's local `SelectedTab` and the VM's
   `IsHumanView`/`DisplayText`/`RefreshDisplayText`.
2. Delete the now-unreachable members from M3 (or wire `ProgressValue` into a real
   determinate bar if you want the progress UI — pick one).
3. Remove the `OnPropertyChanged(string.Empty)` + `MVVMTK0034` hack now that fields live on
   small VMs with normal setters (L1).

### Phase 5 — Extract `GitHubSourceModel` + `AnalysisRunner`, introduce helpers (fixes M2, L2, L3) `[refactor]`
1. Introduce `CancellableOperation` and `Debouncer`; route all four CTS fields and both
   debounce sites through them.
2. Move URL validation (with debounce) into `GitHubSourceModel`; move analyze/render
   orchestration into `AnalysisRunner`. `MainViewModel` becomes the wiring layer.
3. Tests: URL validation debounces (one network call per settle, not per keystroke);
   re-render debounce works without a WPF dispatcher present.

### Phase 6 — Cleanup `[chore]`
`ShowToast`/`_toastCts` disposal (L4), dead-code sweep, XML-doc the new collaborators, and a
final `dotnet test` + a manual smoke (`/run`) of: analyze a local repo, drag the slider,
toggle sections, switch Markdown↔JSON, Save, and paste a GitHub URL.

---

## Agent workflow / gates

- **Branch:** work on `develop` (or a `plan-6/*` branch); one commit per phase, tagged with
  the phase number, same style as PLAN-5 commits (`PLAN-6 Phase N: … f1`).
- **Per-phase gate:** `dotnet build` clean (no new warnings — note the existing
  `MVVMTK0034` suppressions should *shrink*, not grow) **and**
  `dotnet test tests/DevContext.Desktop.Tests` green. The desktop project is
  `net10.0-windows`/WPF, so build on Windows.
- **Do not** start Phase 3+ until Phase 1–2 are committed and green; the bug fixes are the
  shippable win and the refactor's safety net.
- **Preserve public property names** consumed by the three `.razor` files unless the same
  commit updates the binding — grep `src/DevContext.Desktop/**/*.razor` for any member you
  rename.
- **Keep collaborators free of `System.Windows.*`** so they stay unit-testable; only the
  composition root and the `Debouncer`'s `SynchronizationContext` capture touch the UI thread.
- Each phase that changes behavior adds or updates a test in `tests/DevContext.Desktop.Tests`
  before being considered done.
