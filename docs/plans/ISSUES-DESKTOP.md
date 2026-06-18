# Desktop App — Consolidated Issue Register

> Compiled from a full audit of `src/DevContext.Desktop` + `src/DevContext.Core/Rendering`
> on `develop`. Cross-references `docs/plans/PLAN-6` and `docs/plans/PLAN-12-HTML-SECTION-TOGGLING.md`.
> Status reflects the state on branch `develop` before remediation began.

Severity legend: **C** critical · **H** high · **M** medium · **L** low · **B** latent bug

---

## PLAN-12 — HTML section toggling

| ID | Sev | Status | Issue | Fixed by |
|----|-----|--------|-------|----------|
| D1 | H | fixed | Section drawer toggles don't filter HTML or text content | Phase D: persistent wrappers + RebuildLlmViewText |
| D2 | H | fixed | Nav links break after MarkupString replacement (DOM destroyed) | Phase D: persistent wrappers (only display toggled) |
| D3 | H | fixed | `RenderedContext.Sections` always empty | Phase C: both renderers populate Sections |
| D4 | M | fixed | AntiPatterns & EventFlow bypass `ShouldRender` | Phase A: routed through RenderSection |
| D5 | L | fixed | `SectionTokenRecord` list collected but never used | Phase C: removed from HtmlContextRenderer |
| D6 | — | fixed | Duplicate `_output.HasOutput = true` assignment | Phase D cleanup |
| D7 | M | fixed | Section VM state lost on re-render | Phase D: keyed merge preserves IsIncluded |
| D8 | M | fixed | `RebuildLlmViewText()` is a no-op | Phase D: filters by included markdown fragments |
| D9 | L | fixed | HTML sections render empty wrappers | Phase C: only non-empty fragments tracked |
| D10 | M | fixed | Map/Trace ignores section filtering | Narrative/catalog unification: drawer hidden in narrative mode |

## PLAN-6 — MainViewModel refactor

| ID | Sev | Status | Issue | Fixed by |
|----|-----|--------|-------|----------|
| C1 | C | fixed | Superseded analysis disposes current CTS | `CancellableOperation` + cancellation guards (Phase A) |
| C2 | C | fixed | JSON output format non-functional | `AnalysisService.RenderAsync` honors format (Phase A) |
| H1 | H | fixed | PropertyChanged unsubscribe leak | Named `OnVmChanged` handlers (Phase A) |
| H2 | H | fixed | Budget denominator stale after slider | `_sections.BudgetTokens = MaxTokens` in RerenderAsync |
| H3 | H | fixed | Every re-render does double work | HTML skipped for JSON (Phase A); HTML skipped entirely in narrative mode (narrative/catalog unification) |
| M1 | M | fixed | Two sources of truth for active output tab | `OutputViewModel.SelectedTab` single source |
| M2 | H | fixed | GitHub URL validation fires per keystroke | Debounced URL validation (Phase E) |
| M3 | M | fixed | Dead VM state (`DisplayContent`/`DisplayHtml`/`BudgetUtilisation`/`IsActive`) | Removed (Phase A) |
| L1 | L | deferred | `OnPropertyChanged(string.Empty)` in 2 places | Correct pattern for Blazor multi-field updates |
| L2 | L | fixed | `DebouncedReanalyze` couples VM to WPF | `Debouncer` uses `SynchronizationContext` (Phase A) |
| L3 | L | deferred | `_snapshot` shared mutable state | Mitigated by UI-thread marshalling; low risk |
| L4 | L | deferred | `ShowToast` async void | Acceptable fire-and-forget pattern |

## Latent bugs found beyond the two plans

| ID | Sev | Status | Issue | Fixed by |
|----|-----|--------|-------|----------|
| B1 | H | fixed | Pipeline caches by first-seen rootPath — cross-project contamination | `_cachedRootPath` check + rebuild on change (Phase A) |
| B2 | M | fixed | MainViewModel never disposed | `OnClosed` disposes VM + ServiceProvider (Phase A) |
| B3 | L | fixed | Undefined CSS variables `--text-accent`, `--border-light`, `--font-sans` | Defined in `:root` (Phase A) |
| B4 | H | fixed | Hardcoded `ChangePasswordEndpoint.cs` filter | Removed (Phase A) |
| B5 | M | fixed | Entry-picker per-keystroke re-render | Routed through `DebouncedRender` (Phase A) |
| B6 | L | fixed | `CancellableOperation.Link` leaks linked CTS | Tracked and disposed (Phase A) |
| B7 | M | fixed | Two parallel section-state systems | `DerivedProfile` now derives from scenario (deep-dive→debug, overview→focused) instead of dead `SectionToggle.IsEnabled`. Removes the invisible stale state driving the extraction profile. |
| B8 | L | fixed | `--format md` throws | Normalized to "markdown" (Phase C) |
| B9 | L | fixed | `AnalysisResult` record dead | Removed (Phase A) |
| B10 | — | not-a-bug | `DiscoveryPipeline.RunAsync` | Used by Core tests (7 call sites) |
| B11 | L | fixed | `has-output`/`is-loading` classes unstyled | Removed from App.razor (Phase A) |
| B12 | M | fixed | Dispatcher swallows ALL exceptions | Only critical exceptions not handled (Phase A) |
| B13 | L | deferred | `InstallWebView2Sync` blocks UI | Pre-existing. Only runs once at first launch. |
| B14 | M | fixed | Inconsistent section IDs (lowercase literals) | Normalized to `SectionNames` constants (Phase A) |
| B15 | M | fixed | `RenderCallGraph` missing `id` attribute | Added (Phase A) |
| B16 | M | fixed | Spinner-only loading | Determinate progress bar + staged percentages (Phase B) |
| B17 | L | fixed | Cancel hides spinner before "Canceled" shows | ProgressText set before return, brief delay in finally (Phase B) |
| B18 | H | fixed | Human tab plain `<pre>` in Trace/Map mode — no HTML, identical to LLM | Narrative/catalog unification: both views show same narrative content. `NarrativeHtmlConverter` converts Map/Trace text to styled HTML (headings, trace tree with colored nodes, code blocks, topology trees). Human view now has visual distinction from LLM view. |
| B19 | L | fixed | `DesktopProgressObserver` empty stubs | Populated `OnStageStarted` with percentages; `OnPipelineCompleted` reports 100% (Phase B) |
| B20 | L | deferred | External Google Fonts fails silently offline | Falls back to `system-ui`. Non-critical. |

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

- **Phase A** — Latent bugs (B1-B15, M3, B12): 14 quick-win fixes. ✅
- **Phase B** — Loading/progress UX (B16, B17, B19). ✅
- **Phase C** — Renderers emit fragments (D3, D4, D5, D9, B8, B14, B15). ✅
- **Phase D** — Desktop section-fragment binding (D1, D2, D7, D8, D10). ✅
- **Phase E** — GitHub URL debounce + cleanup (M2, H3 partial). ✅
- **Follow-on 1** — Restore HTML view in Human tab for Map mode. ✅
- **Follow-on 2** — UI audit: unwired options, mode-gated controls, stale labels. ✅
- **Follow-on 3** — Fix LLM view showing HTML in Trace mode + sections right-side overlay. ✅
- **Follow-on 4** — Narrative/catalog unification: both views show same content in all modes. ✅

**Deferred (minor / aspirational):**
- B13: Async WebView2 install (pre-existing, runs once)
- B20: Offline font fallback (non-critical)
- L1, L3, L4: Minor code quality items (acceptable patterns)
- Trace as collapsible HTML `<details>` tree — aspirational enhancement over current styled monospace rendering
- Doc reconciliation: `cli-reference.md` / `desktop-ui.md` (out of scope)
