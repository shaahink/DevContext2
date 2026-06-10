# DevContext2 — Project Status Report

**Date**: 2026-06-10 | **Tests**: 205/205 passing | **Branch**: `main` = `develop`

---

## What's Been Done

### Sprint 1 — Native Avalonia UI + Direct Engine + Tests (complete)

| Phase | Feature | Status |
|---|---|---|
| 1 | Catppuccin Latte light theme, SegmentedControl, embedded MDI icons, GridSplitter layout | ✅ |
| 2 | Direct engine (DiscoveryPipeline in-process), cancellation, DesktopProgressObserver | ✅ |
| 3 | Auto-reanalyze on option change, CancellationTokenSource management, F5 shortcut | ✅ |
| 4 | Desktop section toggles, Human/LLM view tabs, budget bar, Copy LLM button | ✅ |
| 5 | Layer classification heuristics, partial class field merging, DI shape rendering | ✅ |

### Sprint 2 — Reactive GitHub + Dual-View + Section Budgets (complete)

| Phase | Feature | Status |
|---|---|---|
| 6 | Section toggle panel (grouped), human/LLM tabs, budget bar, collapsible section panel | ✅ |
| 7 | Reactive GitHub repo analysis: URL parsing, git ls-remote, clone, 7 edge cases | ✅ |
| 8 | SectionBudget model for priority-based truncation | ✅ |
| 9 | Partial class field/constructor merging across files | ✅ |
| 10 | DI shape-based compact rendering (ForwardingAlias, InlineFactory) | ✅ |

### Post-Sprint Fixes

| Fix | Issue |
|---|---|
| DisplayText binding | Observable property notification path fixed |
| ScrollViewer section panel | Section bar capped at 200px, markdown visible again |
| Toolbar layout | Copy/Save/CopyLLM buttons aligned right (Column=2) |
| Token slider debounce | 500ms debounce prevents rapid re-analysis on drag |
| Budget captured at start | `capturedBudget` local used instead of live slider value |
| Git clone readonly crash | `DeleteDirectoryRobust` strips read-only attrs on Windows |
| GitHub ls-remote | Removed `--heads` flag (HEAD is not a branch ref) |
| Clone path for analysis | `workingPath` used instead of original URL string |
| `--profile quick` dead code | Removed from enum, CLI help, Desktop segmented control |
| EditorConfig | Added with naming conventions, formatting rules |
| British English audit | Fixed double spaces, capitalisation, em dashes, profile display |
| Async void → async Task | `ValidateGitHubUrl` now returns Task (crash-safe) |
| Fire-and-forget fix | `DebouncedReanalyze` uses UI thread dispatcher |
| try/catch in event handlers | All 6 `async void` handlers wrapped |
| IDisposable on ViewModel | CTS instances properly disposed |
| LibGit2Sharp integration | Clone with progress + git CLI fallback + auto-keep 24h |
| Anti-patterns opt-in | Disabled by default in CLI + Desktop (fixes 43-67% budget overrun) |

### OrchardCore Assessment

Ran 4 scenario/profile combos against OrchardCore (9,270 files, 101 projects). Full report at `docs/agent-sessions/ORCHARDCORE-ASSESSMENT.md`.

---

## What's Left (From Original Deep Technical Report)

### Sprint 3 — Remaining Items

| # | Feature | Effort | Priority | Blocked By |
|---|---|---|---|---|
| 11 | **HotpathSynthesizer** — linear critical path from entry to I/O, annotated with anti-patterns | Large | P3 | None |
| 12 | **SectionBudget priorities** — token enforcer respects per-section caps | Medium | P1 | Token accounting (done) |
| 13 | **Extractor control toggle** — `--exclude-extractor` CLI, Desktop toggles for event flow, etc. | Small | P0 | None |
| 14 | **Full sections + v3 config** — `--full-sections` CLI, `devcontext.json` sections block | Small | P3 | Token accounting (done) |

### OrchardCore-Assessment Discovered Issues

| # | Issue | Severity | Fix |
|---|---|---|---|
| A | Controllers signal not detected on OrchardCore (missing `ControllerBase` detection) | 🔴 Critical | Debug `SyntaxStructureExtractor` signal fallback |
| B | Anti-patterns eat 43-67% of budget | ✅ **Fixed** | Opt-in toggle (commit `a5ac1ed`) |
| C | LLM view is a no-op (no Debug sections present in most runs) | 🟡 Medium | Expand exclusion logic beyond Debug category |
| D | 20-32 types classified as `Unknown` on non-standard namespaces | 🟡 Medium | Add project-name heuristic to `InferLayer` |
| E | Profile has no effect on anti-patterns/DI output | 🟢 Low | Already improved by opt-in toggle |
| F | "No endpoints detected" on controller-based OrchardCore | 🔴 Critical | Same as issue A (controllers signal) |
| G | Token budget not respected (all OrchardCore runs over budget) | 🔴 High | Phase 12 section-aware pruning |

### Tech Debt / Nice-To-Haves

| Item | Status |
|---|---|
| `--full-sections` CLI flag | Not done |
| `devcontext.json` v3 schema with `sections` block | Not done |
| `MockRoslynProvider` deduplication (×3 copies) | Not done |
| Empty test projects cleanup (`Roslyn.Tests`, `Integration`) | Done (Sprint 2) |
| Duplicate benchmark renderers | Not done |
| Accurate token counting via SharpToken | Not done |

---

## Known Issues

| # | Issue | Workaround |
|---|---|---|
| 1 | Golden file tests fail when output format changes | Run `$env:UPDATE_GOLDENS = "1"; dotnet test` |
| 2 | `File.WriteAllTextAsync` blocks on UI during settings save | Negligible — <1KB files |
| 3 | `AddRecent` has theoretical lost-update race | Not practically exploitable (single-threaded UI access) |
| 4 | CT1861/CA1869 analyzer warnings suppressed as suggestion | Will fix in separate chore PR |
| 5 | LibGit2Sharp native binaries not bundled for Linux/macOS | Falls back to git CLI |
| 6 | GitHub clone `SemaphoreSlim` not disposed when service goes out of scope | Now fixed (IDisposable added) |

---

## Git Branch Structure

```
main (prod) ← develop (integration)
  ├── improvement/sprint-1     (Sprint 1 — merged)
  ├── improvement/sprint-2     (Sprint 2 — merged)
  ├── fix/anti-patterns-opt-in (merged)
  ├── fix/github-analysis-e2e  (merged)
  ├── fix/git-clone-readonly-develop (merged)
  ├── fix/section-panel-redesign     (merged)
  ├── fix/british-english-audit      (merged)
  ├── chore/add-editorconfig         (merged)
  ├── feature/libgit2sharp-async-cleanup (merged)
  ├── analysis/simplify-cli-engine   (report branch — pushed)
  └── assessment/orchardcore         (report branch — pushed)
```

**Active branches**: `main`, `develop` (both at `a5ac1ed`)

---

## Next Steps (Recommended)

| Priority | Action | Branch |
|---|---|---|
| **P0** | Phase 13: Extractor toggles for event flow, DI, etc. | `feature/extractor-control-toggle` |
| **P0** | Fix controllers signal detection (OrchardCore issue A/F) | `fix/controllers-signal-detection` |
| **P1** | Phase 12: Section budget pruning | `feature/section-budget-pruning` |
| **P1** | Expand LLM view exclusion logic (OrchardCore issue C) | `fix/llm-view-exclusion` |
| **P2** | Project-name heuristic for layer (OrchardCore issue D) | `fix/layer-project-heuristic` |
| **P3** | Phase 11: HotpathSynthesizer | `feature/hotpath-synthesizer` |
| **P3** | Phase 14: Full sections + v3 config | `feature/full-sections-config` |
