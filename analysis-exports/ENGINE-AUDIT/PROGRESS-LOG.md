# Progress Log (append-only)

Newest first. One entry per working session: what changed · what was verified · what's next.

---

## 2026-06-30 (d) — Phase 10.A complete: W6 + W8 + L2 landed

**W6 — Prefer product solution over *.Samples:**
- `SolutionDiscoveryExtractor.cs:49-68`: at equal depth, score candidates — deprioritise
  `*.Samples`/`*.Tests`/`*.Benchmarks` (-100), prefer name == repo dir (+1). Emits `Info` diagnostic
  with choice + alternatives. `PickPrimarySolution` helper added.
- Tests: `SolutionDiscoveryExtractorTests` +2 (Ocelot-over-Samples, MyProject-over-Tests) → 6/6.
- **Live:** `analyze .../ocelot` now reads `MAP Ocelot (2 projects)`, scoped to the real product
  solution (down from 16 projects of `Ocelot.Samples`).

**W8 — Entry→target fallback for view/no-call controller actions:**
- `GraphBuilder.cs:69-101`: `EnrichEntryTargets` now calls `ResolveOwningTypeFallback` when
  `ResolveEntryTarget` returns null. Fallback extracts the owning controller type from the entry's
  handler node (Type → title; Member → type-key lookup → title).
- Tests: `GraphBuilderTests` +1 (view action gets controller type as target) → 9/9.
- **Live:** Ocelot `2/3 → target` → `3/3 → target`. `GET /configuration → FileConfigurationController`.
- Side-effect: minimal-api golden tests resolved `POST /orders → CreateOrderCommand` (was bare) —
  correct by accident (co-located request type in same file) but semantically right. 3 golden files
  updated.

**L2 — Test-path predicate in GraphBuilder (stops test DbContext leak):**
- Added `NoiseFilter` parameter to 4 static GraphBuilder methods: `AddEntityNodes`, `AddEventConsumers`,
  `AddHandlerJoins`, `AddPipelineBehaviors`. Each now gates on `noise.IsProductionEntrySource()` after
  `scope.Contains()`, preventing test-project detections from creating graph nodes.
- Previously only `AddTypeNodes` and entry-point methods checked NoiseFilter; these 4 created nodes
  directly from detections that `SolutionScope` didn't exclude. Test DbContexts/entities no longer
  enter the graph.

**Ratchet:**
- New `eval/expectations/ocelot.json`: `output-contains "MAP Ocelot"` (W6) + `output-contains "GET /configuration →"` (W8) — both `expected`.
- Scorecard/Benchmark matrix updated: Ocelot Q2 → PASS (product-scoped, 3/3 targets); aspnetcore Q3 → PASS (L2).
- Status tables (README, WORKITEMS) flipped W6, W8, L2 to ✅.

**Full gate:** build ✅ · fast tests **374/0** ✅ · eval **11/11** (incl. new ocelot) ✅ · CLI `--strict` matrix ✅.
Do-not-regress anchors: BudgetIndependenceTests (11), TraceQualityTests — both green.

**Next: Phase 10.B — W3 (library trace dive-ins), then W3b.**

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
`eval/gates.ps1`** (junction to the main worktree, or clone per `eval-repos.json`).

**Outcome — full gate PASS** (with populated eval-repos): build ✅ · fast tests ✅ · **eval 27/27** ✅ · CLI
`--strict` matrix ✅. W1 **committed** as the Phase 10.A checkpoint:
- `b4321d8` fix(graph): exclude non-runtime sources from the entry inventory (W1, root-relative)
- `8e9e159` docs(engine-audit): resumable workspace + output contract + phased plan + AGENTS.md
`analysis-repos/` added to `.gitignore`. Working tree clean.
**Next: Phase 10.A continues — W6 (solution selection) + W8 (entry→target), then 10.B/W3.**

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
