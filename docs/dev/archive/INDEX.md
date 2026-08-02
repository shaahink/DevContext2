# Docs Archive — Index

> Archived 2026-06-23 from `chore/housekeeping-stats`. Reorganized 2026-06-28 into `docs/dev/archive/`
> as part of the product/dev doc separation. All files are retracable via git history. Nothing was
> deleted — just reorganized for clarity.
>
> **Note:** paths within these archived files may reference the OLD `docs/` layout (e.g.
> `docs/reports/` rather than `docs/dev/reports/`). These files are preserved as historical records;
> cross-references in active files have been updated to the current layout.

## What was moved

### Previously cleaned up (iteration-7/8 merges)

| Original location | Moved to | Reason |
|---|---|---|
| `docs/agent-sessions/` (10 files) | Deleted | Obsolete session reports superseded by reference docs |
| `docs/examples/` (4 files) | Deleted | Outdated examples; replaced by `eval-results/` and test fixtures |
| `docs/design.md` | Deleted | Replaced by `DESIGN-PHILOSOPHY.md` |
| `docs/STATUS-REPORT.md`, `docs/COMBINED-BENCHMARK-REPORT.md`, etc. | Deleted | Stale reports |
| 17 plan/handover files | `docs/archive/` | Archived for traceability |
| `docs/reports/` (3 files) | `docs/archive/reports/` | Older assessment reports |

## Current archive contents

### `archive/` — Historical plans and handovers (17 files)

These document the construction of the tool and are preserved for historical
reference. They are not user-facing documentation.

| File | What it was |
|------|-------------|
| `PLAN-0-SELF-VALIDATION.md` | Self-validation harness design |
| `PLAN-1-ANALYZE-ONCE-RENDER-MANY.md` | Snapshot/lens architecture |
| `PLAN-2-UNIFIED-FOCUS-UX.md` | Focus + Depth UX design |
| `PLAN-3-NERD-STATS.md` | Stats page original design |
| `PLAN-4-WORLD-CLASS-REPO.md` | Repo quality plan |
| `PLAN-5-REFACTORING-FIXES.md` | Post-iteration fixes |
| `PLAN-6-DESKTOP-MAINVIEWMODEL-REFACTOR.md` | ViewModel refactoring |
| `PLAN-10-TRACE-ENGINE.md` | Trace engine design |
| `PLAN-11-DESKTOP-AND-TRACE-FLESH.md` | Desktop trace integration |
| `PLAN-11-HANDOVER.md` | Handover for that plan |
| `PLAN-12-HTML-SECTION-TOGGLING.md` | Section toggle implementation |
| `PLAN-G1-multi-project-scope.md` | Multi-project scope design |
| `PLAN-G3-library-and-G5-minimal-api.md` | Library + minimal API |
| `HANDOVER-DESKTOP-FIXES.md` | Desktop bug fix handover |
| `HANDOVER-OUTPUT-QUALITY.md` | Output quality handover |
| `handover-trace-engine.md` | Trace engine handover |
| `ISSUES-DESKTOP.md` | Known desktop issues |

### `archive/reports/` — Older assessment reports (3 files)

- `OUTPUT-QUALITY-ASSESSMENT.md` — Pre-v2 output quality audit
- `probe-kit.md` — Roslyn semantic probe kit
- `probe-results.md` — Probe results

### Closed phase trackers (archived 2026-07-16 from repo root)

Moved from the repo root once their phases closed; only the ACTIVE `*-START.md` tracker stays at
root. Paths inside these files are as they were when the phases ran.

| File | What it was |
|------|-------------|
| `L3-START.md` | Loom stage-3 session tracker |
| `LOOM-START.md` | Loom phase tracker (checkpoint tables + handoff blocks) |
| `MERIDIAN-START.md` | Meridian phase tracker |
| `conductor-CLEANUP.md` | Conductor cleanup plan (Loom era) |
| `conductor-DEBT.md` | Conductor-discovered debt items (all resolved, see `docs/qa-reports/QA-FINAL-LOOM.md`) |
| `plan.json` | Conductor machine-readable plan for the Loom gap-close phases |

### `archive/trackers/` — closed phase trackers (archived 2026-08-02 from the repo root)

The repo root now holds **no `*-START.md`**, which is the signal that no phase is mid-flight. The
live entry point is `docs/dev/research/PLAN.md` §2 STATUS. References to these files across the
active docs were repointed to their new paths in the same commit.

| File | What it was | Closed |
|------|-------------|--------|
| `GRAPH-V2-START.md` | graph-v2 autonomous remainder — G1–G10, driven by conductor | 2026-08-02, 22/22 confirmed over 28 sessions |
| `PRISM-START.md` | Prism phase tracker (D1–D5) | 2026-07-19, merged to develop 07-27 |
| `PRISM-INBOX.md` | Prism orchestrator → delivery-session channel | with Prism |
| `TAPESTRY-START.md` | Tapestry phase tracker (T0–T8) | 2026-07-17 |
| `GITHUB-READY-START.md` | GitHub-readiness strand tracker | merged 2026-07-17 |

### `archive/desktop-wpf/` — the retired WPF desktop (archived 2026-08-02 from `docs/dev/`)

`DevContext.Desktop` (WPF) was deleted 2026-07-15 in favour of the Angular 22 + Tauri 2 app. These
eight files planned, reviewed and handed over an application that no longer exists; they referenced
only each other. Kept because they carry the reasoning behind the replacement.

| File | What it was |
|------|-------------|
| `PLAN-DESKTOP-V2.md` · `PLAN-DESKTOP-V2-REMEDIATION.md` · `PLAN-DESKTOP-V3.md` | Build, remediation and terminal-phosphor plans |
| `REVIEW-DESKTOP-V2-P0-P3.md` · `AUDIT-V4-VERIFICATION.md` | Phase review and V4 audit verification |
| `HANDOVER-DESKTOP-V2.md` · `HANDOVER-DESKTOP-V4.md` · `HANDOVER-DESKTOP-REDO.md` | Per-stage handovers, including the redo that chose Angular |
