# DntSite — new-system audit (graph Map/Trace) vs recorded baseline

> Repo: **VahidN/DntSite** (Persian community/blog — Blazor SSR + API Controllers + SQLite + DNTScheduler).
> Cached clone analysed as-is: `C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default` (`DntSite.slnx`).
> System under test: branch `feat/polish-batch-and-g1-phase0` (G1 Hybrid scope · G2 entry targets · G3 archetype ·
> G5 per-endpoint · G6 topology · G7/G8/G9). Build 0-warn; `v1.0.5-preview.0.100`.
> Baselines compared: `eval-repos.json` (DntSite expectations) + the legacy-renderer outputs in this folder
> (`overview-debug.md`, `trace-focused.md`) which **predate** the graph Map/Trace shift.
> New artefacts (this run): `map-v2.md`, `map-v2.json`, `map-scoped-DntSite.Web-v2.md`,
> `trace-feedcontroller-v2.md`, `trace-uploadfile-v2.md`, `trace-changepassword-v2.md`.

## Verdict

The graph Map is a clear net improvement over the legacy output on **structure, entry targets, topology, and
token economy** — and the new **G1 closure run produces the *correct* architecture style where the whole-solution
run does not**. Two real defects surfaced: a **style regression at repo-root** (test project inflates the NLayer
heuristic) and a **missing-entry-class gap** (the 24 DNTScheduler jobs are detected but never become entry points).

## Methodology

Ran the new system four ways and compared each dimension to (a) the recorded `eval-repos.json` expectations,
(b) the legacy baseline files, (c) a read of the repo + the structured `map-v2.json`.

| # | Run | Command |
|---|-----|---------|
| 1 | Root Map (whole-solution) | `analyze <clone>` |
| 2 | Root JSON (counts) | `analyze <clone> --format json` |
| 3 | Scoped Map (G1 closure) | `analyze <clone>\src\DntSite.Web` |
| 4 | Traces | `--focus FeedController` · `"POST /api/UploadFile"` · `"GET /.well-known/change-password"` |

## Dimension scorecard

| Dimension | Expected / baseline | New system | Verdict |
|---|---|---|---|
| Projects in topology | 3 incl. tests (baseline) | **2** (DntSite.Web ── …BlazorSsr; test excluded) | ✅ G6 |
| Architecture style (root) | ControllerBased (80%) | **NLayer** → **ControllerBased** after fix | ✅ fixed |
| Architecture style (scoped closure) | ControllerBased | **ControllerBased** (`Controllers conf=0.9; MediatR=no; MinimalApi=yes`) | ✅ G1 win |
| HTTP endpoints | 70 | **70**, each with `→ Target` | ✅ G2 |
| Minimal-API lambda (`/.well-known/change-password`) | own handler | anchors `call <lambda> GET /.well-known/change-password` (no false target) | ✅ G5 |
| Scheduled/background jobs | **24** (DNTScheduler) | **0** → **24** entries (`Scheduled (24)`) after fix | ✅ fixed |
| Archetype | App | App (entry-point Map, not Library surface) | ✅ G3 |
| Packages | long list | grouped + capped `… (10 total)` | ✅ G9 |
| Stats line | — | `1336 files · 1452 nodes · 126 edges · 70 entries · ~1609 tokens` | ✅ G8 |
| Token cost (Map) | legacy ~8000 (budget) | **~1609** | ✅ leaner |
| Anti-patterns (ManualServiceLocator/Reflection) | present in baseline | not surfaced in Map (orientation-only artefact) | ⚠️ different artefact |

## Defect 1 — root style regression: `NLayer` instead of `ControllerBased`

- **Evidence:** `map-v2.md:8` → `STYLE NLayer (moderate) — evidence: EF Core + 3 projects; folder roles: Infrastructure, Api`. The header says `(2 projects)` but the style evidence says **3 projects**.
- **Root cause:** `ArchitectureStyleDetector` counts `model.Projects` (**includes DntSite.Tests = 3**), and the NLayer rule fires on `hasEfCore && projectCount > 2 && !CleanArchitecture`, outscoring ControllerBased. The graph topology already excludes test projects (G6/`ProjectClassifier`), so the style detector and topology disagree on project count.
- **Proof it's the cause:** the **scoped closure run** (`map-scoped-DntSite.Web-v2.md:8`) excludes the test project (count = 2) → NLayer rule doesn't fire → `STYLE ControllerBased` (correct, matches `eval-repos.json`).
- **Suggested fix:** have `ArchitectureStyleDetector` count non-test projects (reuse `ProjectClassifier`) for the NLayer `projectCount` rule, so root and closure agree. (Out of scope for this audit — recorded as a follow-up.)

## Defect 2 — 24 DNTScheduler jobs detected but not surfaced as entry points

- **Evidence:** `map-v2.md` ENTRY POINTS shows only `HTTP (70)` — no Background/Scheduled section. The legacy baseline (`overview-debug.md:138-163`) listed all **24** jobs. `map-v2.json` contains the scheduler config (26 `AddScheduledTask`/`AddDNTScheduler` references; all job types present: `DotNetVersionCheckJob`, `BackupDatabaseJob`, `AIDailyNewsJob`, …).
- **Root cause:** the jobs are detected (DI/scheduler config) but `GraphBuilder` does not create `ScheduledJob`/`HostedService` `EntryPoint`s from `AddDNTScheduler` → `AddScheduledTask<TJob>(…)`. The graph-based Map's inventory therefore only reflects HTTP entries.
- **Impact:** a major part of this app's runtime surface (24 cron-like jobs) is invisible in the Map and untraceable. This is the single biggest fidelity loss vs the legacy output.
- **Suggested fix:** promote `AddScheduledTask<TJob>` detections to `EntryPointKind.ScheduledJob` entry points (one per job type), anchored on the job class — so they appear in the Map and are traceable. (Follow-up.)

## What the traces show (positive)

- **`trace-feedcontroller-v2.md`** — rich, mostly-`[verified]` call graph: `FeedController → CachedAppSettingsProvider`, `FeedsService → {ProjectsService, ProjectReleasesService, ProjectIssuesService, QuestionsService, …}` with real source lines and salient body snippets. Strong cross-service tracing.
- **`trace-changepassword-v2.md`** — the minimal-API lambda anchors on its **own** node (`call <lambda> GET /.well-known/change-password`, ChangePasswordEndpoint.cs:9), depth 1 — the G5 per-endpoint precision working on a real repo (not collapsing onto a registration type).
- **`trace-uploadfile-v2.md`** — controller endpoint trace (largest at 14 KB), exercises the controller-action path.

## Perf observations

- Root Map: **10.8 s** (1336 files, whole solution) — acceptable; under the G1 large-solution hint threshold (2 scoped projects).
- Traces (debug profile + call graph): **47–56 s** each. Notably slow on a 1336-file repo; the call-graph stage dominates. Worth a follow-up perf look if traces are run interactively.
- Map token cost **~1609** (budget 8000) — the 70-entry inventory is compact; no G9 verbosity problem here.

## Net assessment vs recorded baseline

- **Improved:** topology (test-excluded), per-entry targets (G2), per-endpoint lambda precision (G5), package grouping (G9), graph-shaped stats (G8), far lower token cost, and a correct style under G1 closure scoping.
- **Regressed/lost vs legacy:** architecture style at repo-root (test-project count inflation), and the **24 scheduled jobs** no longer appear as entries.
- **Two concrete, low-risk follow-ups** (not applied here): (1) non-test project count in `ArchitectureStyleDetector`; (2) `ScheduledJob` entry promotion for `AddScheduledTask<T>`.

---

## Post-fix re-audit (both defects resolved)

Both follow-ups were implemented and verified on the same cached clone (build 0-warn; Core/Desktop green;
unit tests added: `ArchitectureStyleDetectorTests.Controllers_with_test_project_not_misread_as_NLayer`,
`GraphBuilderTests.Background_workers_become_entry_points`).

- **Defect 1 — FIXED.** `ArchitectureStyleDetector` now counts **non-test** projects (`ProjectClassifier`).
  Root Map: `STYLE NLayer` → **`STYLE ControllerBased (moderate)`** — matches `eval-repos.json` and the
  scoped run; root and topology now agree on project count.
- **Defect 2 — FIXED.** `GraphBuilder.AddWorkerEntryPoints` promotes `BackgroundWorkerDetection`
  (incl. DNTScheduler `AddScheduledTask<T>`) to entry points. Root Map inventory: `HTTP (70)` **+
  `Scheduled (24)`** = **94 entries** (was 70), ~1947 tokens, 6.0 s. The 24 jobs are now traceable —
  e.g. `trace-worker-backupdatabasejob.md`: `BackupDatabaseJob → WebSiteBackupService →
  {EfDbLogger, AppFoldersService, TelegramUploadBackupService → CachedAppSettingsProvider}` (mostly
  `[verified]`), which did not exist before.

### Call-graph detail options (the desktop "Detail" control = `--detail`)

Same entry (`FeedController`, depth 2), three options:

| Option (`--detail`) | What it renders | FeedController size / tokens |
|---|---|---|
| **Signature** | edges only — `call X (file:line) [verified/approx]`, no body | 3.7 KB / **~728** |
| **Salient** | edges **+ key body lines** per hop (the default) | 5.1 KB / ~1051 |
| **Full** | edges + fuller method slice per hop | 5.1 KB / ~1051 |

Findings: all three produce the **same call-graph shape** (13 service calls under `FeedsService`); they
differ only in how much **body** each hop shows. **Signature** is the leanest (≈30% fewer tokens, ideal
for breadth). **Salient ≈ Full** here because DntSite's traced hops are short methods — the distinction
widens only on long method bodies. Artefacts: `trace-feedcontroller-{signature,salient,full}.md`.

### Net post-fix

DntSite now reads correctly end-to-end on the new system: **ControllerBased**, **2-project** topology
(test excluded), **70 HTTP + 24 Scheduled** entries each with targets/anchors, per-endpoint minimal-API
lambda (G5), and traceable scheduled jobs — across all three call-graph detail levels. Remaining
observation: trace wall-time (call-graph stage) is **35–56 s** on this 1336-file repo — a perf follow-up,
not a correctness issue.
