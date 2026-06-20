# Handover: Desktop Latent Bugs & Rendering Fix

> Branch: `fix/desktop-latent-bugs-and-rendering`  
> Base: `develop` (`6a48c96`)  
> Commits: 11 forward  
> Gate: build clean, 64/64 Desktop tests, 234/236 Core tests (2 pre-existing skips)

---

## What this branch does

Fixes 28 of 30 tracked issues across 5 phases + 4 follow-on commits, spanning the desktop app, Core renderers, and the analyze/render pipeline.

**The big structural change:** unified rendering into two modes — **narrative** (Map/Trace, graph exists) and **catalog** (legacy, no graph). Both views always show the same content, just in different formats.

| Mode | Human view | LLM view | Section toggling |
|------|-----------|----------|-----------------|
| Narrative (Map) | Styled HTML from Map text | Raw Map text | No (monolithic) |
| Narrative (Trace) | Styled HTML with colored trace tree | Raw trace text | No (monolithic) |
| Catalog (no graph) | HTML fragments (toggleable) | Markdown fragments (toggleable) | Yes — both views filter |

---

## Architecture changes

### New: `NarrativeHtmlConverter` (`src/DevContext.Core/Rendering/NarrativeHtmlConverter.cs`)
Converts Map/Trace text to styled HTML for the Human view. Detects MAP/TRACE headers, section headings (TOPOLOGY, ENTRY POINTS, etc.), trace tree nodes (▸/├─/└─), code bodies, and topology trees. Produces `<h2>`, `<h3>`, `<pre>`, and `<span>` elements with CSS classes.

### New: Section fragments in `RenderedContext`
`RenderedContext` now carries:
- `Sections` (`ImmutableArray<SectionStat>`) — per-section name + token count, populated by both `HtmlContextRenderer` and `MarkdownRenderer`
- `SectionFragments` (`IReadOnlyDictionary<string, string>?`) — section key → rendered fragment, for interactive toggling

### Changed: `AnalysisService.RenderAsync` — narrative/catalog split
- When `snapshot.Graph.NodeCount > 0` (narrative mode): **skips the HTML catalog render entirely**. The catalog is a different document; showing it alongside the narrative caused the view-sync bugs. Both views get the same narrative text; the Human view gets HTML via `NarrativeHtmlConverter`.
- When no graph (catalog mode): renders both markdown and HTML. Sections + fragments populated. Section toggling works on both views.

### Changed: `OutputPanel.razor` — rendering paths
Three branches in the Human tab:
1. JSON format → message
2. Narrative mode (`VM.HasGraph`) → styled narrative HTML
3. Catalog mode → per-section HTML wrappers (fragment-based) or monolithic fallback

### Changed: `DerivedProfile` — scenario-driven, not SectionToggle-driven
Was reading from dead `SectionToggle.IsEnabled` (invisible to user). Now derives directly from the selected scenario: `"deep-dive" → "debug"`, `"overview" → "focused"`.

---

## Commit log

| Commit | Phase | Key changes |
|--------|-------|-------------|
| `e4b3d44` | Phase A | 14 latent bugs: pipeline rootPath caching (B1), MainViewModel disposal (B2), CSS vars (B3), hardcoded filter removal (B4), entry-picker debounce (B5), CancellableOperation.Link leak (B6), section ID normalization (B14/B15), AntiPatterns/EventFlow through ShouldRender (D4), dead code removal (B9/B11/M3), dispatcher exceptions (B12) |
| `b204f58` | Phase B | Determinate progress bar with staged percentages (B16/B19), cancel visibility fix (B17), clone progress as bar, overlay opacity reduction |
| `e93ef3b` | Phase C | Renderers populate `Sections` + `SectionFragments` (D3/D5/D9), `--format md` normalized (B8) |
| `c471d6c` | Phase D | Persistent Blazor `<section>` wrappers with CSS display binding (D1/D2), `BuildSectionDataFromStat` keyed merge preserves `IsIncluded` (D7), `RebuildLlmViewText` actually filters (D8/D10) |
| `bdb7074` | Phase E | GitHub URL debounce with immediate URL parsing (M2), `SectionViewModel` extended with `Key`/`Markdown`/`Html` fields |
| `1890fc4` | Follow-on | Restore HTML in Human tab for Map mode (graph always true fix) |
| `8193f89` | Follow-on | UI audit: CloneCleanup simplification, JSON Human tab message, Depth/Detail gated on entry, "Skip Roslyn" label, stale text, `DebouncedReanalyze` → `DebouncedRender` |
| `6b4f9d8` | Follow-on | LLM HTML-in-Trace fix + sections right-side overlay redesign |
| `2c9e92c` | Follow-on | Narrative/catalog unification: both views same content in all modes |
| `80d1375` | Docs | Updated `ISSUES-DESKTOP.md` with post-remediation status |
| `15921fa` | B7+B18 | `DerivedProfile` scenario-driven + `NarrativeHtmlConverter` for styled Human view |

---

## Files changed (21 files, +920 −232)

### Core (5 files)
| File | Changes |
|------|---------|
| `src/DevContext.Core/Pipeline/AnalysisSnapshot.cs` | Added `RootPath` property |
| `src/DevContext.Core/Pipeline/DiscoveryPipeline.cs` | `md`→`markdown` normalization, `RootPath` on snapshot |
| `src/DevContext.Core/Contracts/IContextRenderer.cs` | Added `SectionFragments` to `RenderedContext` |
| `src/DevContext.Core/Rendering/HtmlContextRenderer.cs` | Section ID normalization, fragment capture, AntiPatterns/EventFlow through RenderSection, hardcoded filter removal |
| `src/DevContext.Core/Rendering/MarkdownRenderer.cs` | Fragment + `SectionStat` capture in TrackSection |
| `src/DevContext.Core/Rendering/NarrativeHtmlConverter.cs` | **New** — Map/Trace text to styled HTML |

### Desktop (13 files)
| File | Changes |
|------|---------|
| `Services/AnalysisService.cs` | RootPath-caching fix, narrative/catalog split, fragment passthrough, `AnalysisResult` removal |
| `ViewModels/MainViewModel.cs` | `CancellableOperation` usage, debouncers, narrative HTML conversion, `RebuildLlmViewText` filtering, `ProgressValue`, cancel UX, URL debounce, `OnSectionChanged` notifications |
| `ViewModels/OutputViewModel.cs` | `ProgressValue`, removed dead `DisplayContent`/`DisplayHtml`/`IsActive` |
| `ViewModels/SectionSelectionModel.cs` | `DerivedProfile` scenario-driven, fragment parameters in `BuildSectionDataFromStat`, keyed merge for `IsIncluded` |
| `ViewModels/SectionViewModel.cs` | Added `Key`/`Markdown`/`Html` fields |
| `Components/OutputPanel.razor` | Narrative/catalog branches, persistent wrappers, JSON message, sections overlay, determinate progress bar |
| `Components/ConfigPanel.razor` | Focus hint, CloneCleanup, Depth/Detail gating, labels |
| `Helpers/CancellableOperation.cs` | Linked CTS tracking |
| `MainWindow.xaml.cs` | VM + ServiceProvider disposal on close |
| `Program.cs` | Non-critical dispatcher exception handling |
| `App.razor` | Dead class removal |
| `wwwroot/app.css` | CSS vars, progress bar, narrative HTML, sections overlay |

### Tests (1 file)
| File | Changes |
|------|---------|
| `tests/DevContext.Desktop.Tests/MainViewModelTests.cs` | Updated `DerivedProfile` tests to validate scenario-based derivation |

### Docs (2 files)
| File | Changes |
|------|---------|
| `docs/plans/ISSUES-DESKTOP.md` | **New** — consolidated issue register with 30 tracked issues + status |
| `docs/plans/PLAN-12-HTML-SECTION-TOGGLING.md` | Existing plan doc (tracked by git) |

---

## Deferred (5 minor items)

| ID | Issue | Reason |
|----|-------|--------|
| B13 | WebView2 install blocks UI | Pre-existing, runs once |
| B20 | Offline Google Fonts | Falls back to `system-ui` |
| L1 | `OnPropertyChanged(string.Empty)` | Correct pattern for Blazor multi-field notify |
| L3 | `_snapshot` shared state | Mitigated by UI-thread marshalling |
| L4 | `ShowToast` async void | Acceptable fire-and-forget |

---

## How to verify

```bash
# Build
dotnet build DevContext.slnx

# Full test gate
dotnet test tests/DevContext.Desktop.Tests   # 64 passed
dotnet test tests/DevContext.Core.Tests      # 234 passed, 2 skipped

# Manual smoke (Desktop app)
# 1. Analyze a local .NET project
# 2. Human tab → HTML catalog with sections
# 3. Toggle sections → Human filters, LLM filters
# 4. Pick an entry point → Human = styled trace tree, LLM = raw trace
# 5. Clear entry → Human = styled Map, LLM = raw Map
# 6. Drag token budget slider → re-renders with new budget
# 7. Switch format to JSON → Human shows message, LLM shows JSON
# 8. Toggle between tabs → views stay in sync
```
