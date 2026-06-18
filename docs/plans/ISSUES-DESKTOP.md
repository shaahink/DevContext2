# Desktop App — Consolidated Issue Register

> Compiled from a full audit of `src/DevContext.Desktop` + `src/DevContext.Core/Rendering`
> on `develop`. Cross-references `docs/plans/PLAN-6` and `docs/plans/PLAN-12-HTML-SECTION-TOGGLING.md`.
> Status reflects the state on branch `develop` before remediation began.

Severity legend: **C** critical · **H** high · **M** medium · **L** low · **B** latent bug

---

## PLAN-12 — HTML section toggling (all open before remediation)

| ID | Sev | Status | Issue | Evidence |
|----|-----|--------|-------|----------|
| D1 | H | open | Section drawer toggles don't filter HTML or text content | `OutputPanel.razor:145` → `section.IsIncluded=…` → `OnSectionChanged` → `RebuildLlmViewText` no-op (`MainViewModel.cs:116-120`) |
| D2 | H | open | Nav links break after MarkupString replacement (DOM destroyed) | `OutputPanel.razor:163` `@((MarkupString)VM.HumanViewHtml)` |
| D3 | H | open | `RenderedContext.Sections` always empty — never populated by any renderer | `HtmlContextRenderer.cs:75`, `MarkdownRenderer.cs:139`, `JsonContextRenderer.cs:27` |
| D4 | M | open | AntiPatterns & EventFlow bypass `ShouldRender` section filtering | `HtmlContextRenderer.cs:53-54` (direct calls, not via `RenderSection`) |
| D5 | L | open | `SectionTokenRecord` list collected but never used | `HtmlContextRenderer.cs:17` (only Header tracked, `:27`); never returned |
| D6 | — | fixed | Duplicate `_output.HasOutput = true` assignment | each branch sets it once (`MainViewModel.cs:384,403,417`) |
| D7 | M | open | Section VM state lost on re-render — new VMs each time | `SectionSelectionModel.cs:161-181` recreates `SectionViewModel`s per call |
| D8 | M | open | `RebuildLlmViewText()` is a no-op — just reassigns same value | `MainViewModel.cs:116-120` |
| D9 | L | open | HTML sections render empty wrappers when query returns 0 items | inner render methods emit tags unconditionally |
| D10 | M | open | Map/Trace narrative path ignores section filtering entirely | `DiscoveryPipeline.cs:302-331` never consults `RequiredSections` |

## PLAN-6 — MainViewModel refactor (mostly fixed)

| ID | Sev | Status | Issue | Evidence |
|----|-----|--------|-------|----------|
| C1 | C | fixed | Superseded analysis disposes current CTS and clobbers UI | `CancellableOperation` used (`MainViewModel.cs:301-303`); cancellation guards (`:400,408,413`); `AllowConcurrentExecutions=true` (`:298`) |
| C2 | C | fixed | JSON output format non-functional | `AnalysisService.RenderAsync` honors format (`:171-177`); `RenderRequest.Format=SelectedFormat` (`MainViewModel.cs:438`) |
| H1 | H | fixed | PropertyChanged unsubscribe no-op → handler leak | all three components use named `OnVmChanged` |
| H2 | H | fixed | Budget denominator stale after slider change | `_sections.BudgetTokens=MaxTokens` in `RerenderAsync` (`MainViewModel.cs:468`) |
| H3 | H | partial | Every re-render does double work (md+html) regardless of view/format | HTML skipped for JSON (`AnalysisService.cs:175-177`); markdown still double-renders regardless of active tab |
| M1 | M | fixed | Two sources of truth for active output tab; "jump to LLM on edit" dead | `OutputViewModel.SelectedTab` single source (`OutputViewModel.cs:8-11`); `OutputPanel.razor:69-71` |
| M2 | H | open | GitHub URL validation fires a network call on every keystroke | `OnProjectPathChanged` (`MainViewModel.cs:141-145`) → `ValidateGitHubUrlAsync` with no debounce; `@bind:event="oninput"` (`ConfigPanel.razor:56`) |
| M3 | M | partial | Large blocks of VM state never reach the UI | `DisplayContent`/`DisplayHtml` (`OutputViewModel.cs:43-51`), `BudgetUtilisation`/`TotalTokens`/`Sections`/`SetSectionEnabled` (`MainViewModel.cs:51-55,191-195`) never read |
| L1 | L | partial | `OnPropertyChanged(string.Empty)` + `MVVMTK0034` hack | still used (`MainViewModel.cs:388,478`); MVVMTK0034 pragmas gone |
| L2 | L | fixed | `DebouncedReanalyze` couples VM to WPF | `Debouncer` uses `SynchronizationContext` (`Debouncer.cs:13,29`) |
| L3 | L | open (mitigated) | `_snapshot` shared mutable state, no synchronization | relies on UI-thread marshalling; broken by C1 re-entrancy (now fixed) and L2 (now fixed) |
| L4 | L | mostly fixed | `ShowToast` async void + `_toastCts` never disposed | `_toastCts` disposed (`OutputPanel.razor:20,54-55`); still `async void` (`:52`) |

## Latent bugs found beyond the two plans

| ID | Sev | Status | Issue | Evidence |
|----|-----|--------|-------|----------|
| B1 | H | open | `AnalysisService.GetPipeline` caches by first-seen rootPath — analyzing a second project reuses the first project's DI graph | `AnalysisService.cs:45-55`; `RenderAsync` calls `GetPipeline(".")` (`:169`) |
| B2 | M | open | `MainViewModel` never disposed — singleton, ServiceProvider never disposed | `MainWindow.xaml.cs:43`; `MainViewModel.Dispose()` (`:542-549`) unreachable |
| B3 | L | open | Undefined CSS variables `--text-accent`, `--border-light`, `--font-sans` | `app.css:238,244,260,407,414,451` — section-heading underlines render transparent |
| B4 | H | open | `RenderEndpoints` hardcodes filter against `ChangePasswordEndpoint.cs` — debug leftover | `HtmlContextRenderer.cs:206` |
| B5 | M | open | Entry-picker `@bind:event="oninput"` triggers `RerenderAsync` per keystroke, no debounce | `ConfigPanel.razor:124` |
| B6 | L | open | `CancellableOperation.Link` leaks the linked CTS — called per `RerenderAsync` | `CancellableOperation.cs:43-51`; `MainViewModel.cs:432` |
| B7 | M | open | Two parallel section-state systems drift independently | `SectionToggle.IsEnabled` (read by `DerivedProfile`) vs `SectionViewModel.IsIncluded` (bound by drawer) |
| B8 | L | open | `--format md` throws "No renderer registered" | `DiscoveryPipeline.cs:301` treats `md` as narrative; `:351` only has `markdown`/`json`/`html` |
| B9 | L | open | `AnalysisResult` record is dead — declared, never referenced | `AnalysisService.cs:308-315` |
| B10 | — | not-a-bug | `DiscoveryPipeline.RunAsync` — used by Core tests, not dead | `tests/DevContext.Core.Tests/*.cs` (7 call sites) |
| B11 | L | open | `App.razor:5` adds `has-output`/`is-loading` classes never styled | `App.razor:5`; grep `app.css` — zero hits |
| B12 | M | open | `DispatcherUnhandledException` swallows ALL dispatcher exceptions (`e.Handled=true`) | `Program.cs:42-46` |
| B13 | L | open | `InstallWebView2Sync` blocks UI thread on download + install | `MainWindow.xaml.cs:57-88` |
| B14 | M | open | Inconsistent section IDs — lowercase literals don't match `SectionNames` constants | `dc-antipatterns` (`HtmlContextRenderer.cs:398`), `dc-eventflow` (`:413`), `dc-diagnostics` (`:440`), `dc-entry-points` (`:191`) |
| B15 | M | open | `RenderCallGraph` emits `<section>` without `id` when CallGraph is available | `HtmlContextRenderer.cs:245` |
| B16 | M | open | Spinner-only loading; `AnalysisProgress.Value` plumbed but never populated/read | `OutputPanel.razor:175-178`; `AnalysisService.cs:256` always `null`; no determinate-bar CSS |
| B17 | L | open | On cancel, `finally` hides spinner before "Canceled" shows | `MainViewModel.cs:409,422` |
| B18 | H | open | Human tab renders plain `<pre>` text in Trace/Map mode — no HTML, identical to LLM view | `OutputPanel.razor:157-164` |
| B19 | L | open | `DesktopProgressObserver` mostly empty stubs — only coarse stage text | `AnalysisService.cs:235-270` |
| B20 | L | open | External Google Fonts dependency fails silently offline | `index.html:9-11` |

## Doc inconsistencies (out of scope for this pass — CLI/docs untouched)

| Issue | Evidence |
|-------|----------|
| `cli-reference.md` documents `--scenario trace` (doesn't exist in `ScenarioRegistry`) | `cli-reference.md:22`; `ScenarioRegistry.cs` only has `overview`/`deep-dive` |
| `cli-reference.md` documents `audit` as deprecated-maps-to-overview (no handling in code) | `cli-reference.md:28` |
| `--focus`/`--depth`/`--detail`/`--stats`/`--strict` in code+README, absent from `cli-reference.md` | `AnalyzeSettings.cs:11-109` |
| `--repo`/`--ref`/`--cleanup`/`--keep` in code, absent from both docs | `AnalyzeSettings.cs:91-105` |
| `--max-tokens` range: docs 500–50000, code 100–100000 | `cli-reference.md:103`; `DevContextConfig.cs:46-47` |
| `--profile quick` valid in code, undocumented | `DevContextConfig.cs:48` |
| `--format html` valid in code, undocumented | `AnalyzeCommand.cs:118-123` |
| `--task` documented as live feature, marked deprecated in code | `cli-reference.md:75-93`; `AnalyzeSettings.cs:35`; `AnalyzeCommand.cs:103-104` |
| README "no natural-language input" contradicts shipped `--task` | `README.md:49` vs `AnalyzeSettings.cs:35` |
| `--around` documented as primary in `cli-reference.md`, silently aliased to `--focus` in code | `cli-reference.md:49-71`; `AnalyzeCommand.cs:76` |
| README Map/Trace mode names don't appear in CLI (uses overview/deep-dive) | `README.md:46-49`; `ScenariosCommand.cs:15-19` |
| README's CodeGraph trace engine knobs (MaxFanOut, edge kinds) not exposed by CLI | `README.md:51-118` |

---

## Remediation phases

- **Phase A** — Latent bugs (B1-B15, M3, B12): quick wins, no UX change.
- **Phase B** — Loading/progress UX (B16, B17, B19, B20).
- **Phase C** — Renderers emit fragments (D3, D4, D5, D9, B8, B14, B15).
- **Phase D** — Desktop section-fragment binding (D1, D2, D7, D8, D10, B7).
- **Phase E** — Mode parity + cleanup (B18, M2, H3, L1, L3).

Each phase: `dotnet build` + `dotnet test tests/DevContext.Desktop.Tests` green.
