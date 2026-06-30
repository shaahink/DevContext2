# Progress Log (append-only)

Newest first. One entry per working session: what changed · what was verified · what's next.

---

## 2026-06-30 (c) — Gate caught a W1 regression → root-relative fix

Ran `eval/gates.ps1`. Build passed; **fast tests failed** on
`DevContext.Server.Tests.AnalyzeFlowTests` — the gRPC e2e analyzes the fixture `tests/fixtures/ControllerApp`,
whose absolute path contains `/tests/`, so W1's `IsTestPath` wiring excluded the fixture's **own** endpoints
(no entries → trace failed). This is exactly the false-positive the original code warned about (`IsTestPath`
"never used by the app graph filter") — `/test/` matched **above** the analysis root.

**Fix (root-relative matching):** `NoiseFilter` now takes the analysis root; the path-convention checks
(`IsSamplePath`/`IsTestPath`/`IsNonRuntimeEntrySource`) apply to the portion of a path **below** that root.
`DiscoveryPipeline` passes `context.RootPath`. So a repo that itself lives under a `…/tests/…` path keeps its
surface, while test/sample/template dirs *inside* the analysed repo are still excluded. `IsInTestProject` +
`IsGeneratedPath` stay absolute (project-based / unambiguous).
- Tests: `NoiseFilterTests` +2 (root-under-test-path keeps surface; below-root test/template excluded) →
  **13/13**. `AnalyzeFlowTests` → **7/7** (regression gone). aspnetcore unaffected (its root has no `/test/`
  above it; below-root `/test/` still excluded → stays 518→10).
- General lesson logged: absolute path-substring filters are root-sensitive — always match relative to the
  analysed root.

**Gate environment gotcha (resolved):** the re-run then failed in the eval tier on TodoApi (`POST /todos/`
trace missing `TodoDbContext`). Root cause was **not** W1 — the analysis worktree's `eval-repos/` are **empty
dirs** (never cloned here), so `Directory.Exists` is true (tests don't skip) but analysis finds 0 projects.
Confirmed by running the same `TraceQualityTests.Trace_bridges_indirection` on the **main worktree** (populated
eval-repos) → **4/4 pass**. Fix: junctioned `C:\code\DevContext2-analysis\eval-repos` →
`C:\code\DevContext2\eval-repos` (populated). **Next agent: ensure eval-repos are present here before running
`eval/gates.ps1`** (junction to the main worktree, or clone per `eval-repos.json`). Gate re-run with the
junction in progress.

---

## 2026-06-30 (b) — Output contract + phased plan + resume structure

Added the two missing pieces the user asked for:
- **`OUTPUT-CONTRACT.md`** — the gradeable spec: what info each output MUST contain to answer Q1 "what is
  this" / Q2 "how to use / where to start" / Q3 "examine any part", **per archetype**. Extends the canonical
  `docs/product/ACCEPTANCE.md` + `IDEAL-OUTPUT-TARGET.md` and adds the archetypes they don't cover (Desktop,
  API-Gateway, library trace-on-demand, framework-scale). Includes a fill-in **scorecard** that drives the
  matrix; "done" = every row's Q1/Q2/Q3 + noise = PASS.
- **`PHASED-PLAN.md`** — execution order as **Phase 10.A–10.F** of the existing `UNIVERSAL-LENS-ROADMAP.md`
  (this audit IS Phase 10 "Coverage"). Each sub-phase has an exit gate expressed as acceptance ratchets:
  10.A entries/honesty (W1✅,W6,W8,L2) → 10.B dive-in (W3,W3b) → 10.C archetypes (W5,W7) → 10.D readability
  (W4); 10.E coverage + 10.F hygiene/W9 in parallel.
- README updated: index + cold-start resume protocol (README → PHASED-PLAN → OUTPUT-CONTRACT → per-item loop),
  grading step added, recommended order reconciled to the phases.

Discovered the repo already has a strong contract substrate — `ACCEPTANCE.md` (per-artifact bar + phase
ratchet, phases 0–5 ENFORCED), `IDEAL-OUTPUT-TARGET.md` (hand-built ideal shapes), `UNIVERSAL-LENS-ROADMAP.md`
(phases, Phase 10 = coverage). The audit work slots cleanly into Phase 10; no competing scheme introduced.

---

## 2026-06-30 (a) — Audit verification + W1 landed + resumable workspace created

**Who/what:** Verified the DeepSeek cross-repo analysis against current code (`ff143d8`) and the export
artifacts; reframed the plan around the real north star (engine/CLI output quality across diverse .NET repos,
token explicitly demoted); implemented **W1**.

**Audit outcome** (detail in `../VERIFIED-PLAN.md`):
- DeepSeek's headline **C1 "token budget broken" is misdiagnosed** — the enforcer (`Pruning/`, not the
  report's `Scoring/`) governs only the dead legacy catalog; the Map/Trace are budget-independent **by design**
  and locked by `BudgetIndependenceTests`. **H2 "budget starved Ocelot entries" is refuted** (budget can't
  touch the narrative; Ocelot's 3 entries are mostly correct).
- Confirmed: C2 (test-entry leak), C3 (empty library traces), H1 (desktop→library), H4 (huge Map → re-rooted
  to "no structural cap", not token), M2, L2.
- New gaps found: arbitrary **solution selection** (Ocelot scoped to `Ocelot.Samples` over `Ocelot.slnx`),
  no **gateway archetype**, dead token/catalog machinery still surfaced in `--stats`.

**W1 — exclude non-runtime entry sources (DONE, verified):**
- Code: `src/DevContext.Core/Graph/NoiseFilter.cs` — `IsProductionEntrySource` now also rejects
  `ProjectClassifier.IsTestPath` (the `/test/` tree → catches `testassets/`) and a new
  `IsNonRuntimeEntrySource` (`/testassets/`, `/Testing/`, `/stress/`, `/perf/`, `/FunctionalTests/`,
  `/ProjectTemplates/`).
- Tests: new `tests/DevContext.Core.Tests/NoiseFilterTests.cs` (11 cases, anchored on real aspnetcore paths)
  → **11/11 pass**; existing `GraphBuilderTests` entry-source test still green (18/18 combined first run).
- **E2e on aspnetcore (the proof):** HTTP entries **518 → 13 → 10** (after the `FunctionalTests` tightening);
  **zero** test/stress/template/functional-test paths remain. Survivors are the real ASP.NET Core Identity
  API (`POST /login`, `/register`, `/refresh`, `/manage/info`, `/resetPassword`, …) from
  `src/Identity/Core/src/`. Token side-effect (not the goal): ~21,071 → ~4,742. Artifacts:
  scratchpad `aspnetcore-map-W1.md` (518→13) and `aspnetcore-map-W1-final.md` (→10).

**Not yet done / next:**
- Run the **full gate** (`eval/gates.ps1`) to confirm no regression on the existing eval repos (the
  `IsTestPath` wiring is the only behaviour change with cross-repo reach; risk is low — eval apps keep test
  code in test *projects* already excluded — but it must be confirmed before committing). Eval repos live in
  the **main** worktree (`C:/code/DevContext2/eval-repos`), not here.
- Add `eval/expectations/aspnetcore.json` with `output-not-contains markdown "/test/testassets/"` (+ stress,
  ProjectTemplates) and register the 4 repos in `eval-repos.json`.
- Consider sharing the test-path predicate with `TraceBuilder` to close **L2** (test DbContext in traces).
- Changes are **uncommitted** on `feat/engine-cross-repo-analysis` (commit when the user/next agent confirms
  the full gate is green). Untracked: `analysis-exports/`, `analysis-repos/`; modified:
  `NoiseFilter.cs`; added: `NoiseFilterTests.cs`.

**Then:** W3 (library/type traces) is the highest-value next item — it's what makes "examine any part" work
for libraries and (with W5) desktop apps.
