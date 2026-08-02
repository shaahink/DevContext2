# Phased Plan — execution order with exit gates

This cross-repo audit is the deep execution of **Phase 10 "Coverage ladder rungs"** of
`docs/dev/plans/UNIVERSAL-LENS-ROADMAP.md` (phases 0–5 are ENFORCED; 6–9 are infra). It also closes a few
residual Phase 2/4 items the new repos exposed. Sub-phases below are ordered by dependency and by
north-star leverage. **Each phase is done only when its exit gate is green and the ratchet is flipped.**

Per-phase loop (same every time):
1. Implement the listed work items (`WORKITEMS.md` has locus + fix).
2. Add/flip the acceptance checks (eval `expected`, `TraceQualityTests`), keep do-not-regress anchors green.
3. Run `eval/gates.ps1`. 4. Re-capture the affected repo's output, grade it against `OUTPUT-CONTRACT.md`,
   update the scorecard + `BENCHMARK-MATRIX.md`. 5. Append to `PROGRESS-LOG.md`. 6. Commit.

Do-not-regress anchors (every phase): `BudgetIndependenceTests` (Map/Trace budget-independent),
`TraceQualityTests` sibling-divergence Facts (narrow handler bridge stays narrow).

---

## Phase 10.A — Entry-point fidelity & honesty  *(Q2 for apps & gateways)*
**Goal:** every app/gateway repo's entry surface is production-only and honestly targeted.
**Items:** **W1 ✅ done** · **W6** (prefer product solution over `*.Samples` at equal depth) · **W8**
(entry→target fallback to owning controller type) · **L2** (share the W1 test-path predicate into
`TraceBuilder` so traces stop touching test DbContexts).
**Exit gate (ratchet):**
- `eval/expectations/aspnetcore.json`: `output-not-contains markdown "/test/testassets/"`, `/stress/`,
  `/ProjectTemplates/` → `expected` (W1). New `ocelot.json`: header `output-contains "MAP Ocelot"` not
  `"Ocelot.Samples"` (W6); `GET /configuration` line `output-contains "→"` (W8).
- `SolutionDiscoveryExtractorTests` (W6) + `GraphBuilder` entry-target test (W8) green; full gate green.
- **Scorecard:** Q2 + Noise columns PASS for aspnetcore (done), Ocelot.

## Phase 10.B — Dive-in everywhere  *(Q3 for libraries; unblocks desktop)*
**Goal:** focusing any public type/method on a library (or any type-rooted node) yields a non-empty, faithful
trace.
**Items:** **W3** (entry-root Type bridges all member call edges, scoped to the root / `PublicApi` entry) ·
**W3b** (honest message when a focus resolves but has no out-edges; explain a null-match before Map fallback).
**Exit gate (ratchet):**
- New `TraceQualityTests` `[InlineData("analysis-repos/serilog","Log", new[]{"Logger"})]` passes the existing
  `seamHops >= 1` guard; a second case for `LoggerConfiguration`. Sibling-divergence Facts **stay green**
  (proves the app handler bridge is still narrow).
- **Scorecard:** Q3 column PASS for Serilog, FluentValidation, Polly.

## Phase 10.C — Archetype recognition  *(Q1 for desktop & gateway)*
**Goal:** desktop apps and gateways are correctly identified with their real entry surface.
**Items:** **W5** (Desktop archetype: `UiEntry` kind + `DesktopEntryExtractor`; narrow the `allExeAreAuxiliary`
heuristic) · **W7** (Gateway archetype: detect Ocelot/YARP; ROUTES/CLUSTERS-from-config section).
**Exit gate (ratchet):**
- `ArchetypeDetectorTests`: WinExe + WinUI signal → App (W5). `files.json` `json-equals $.archetype "App"`
  → `expected`. `ocelot.json` `output-contains markdown "ROUTES"` → `expected` (W7).
- **Scorecard:** Q1 PASS for Files (Desktop) and Ocelot (Gateway); with 10.B, Q3 PASS for Files.

## Phase 10.D — At-a-glance readability at scale  *(Q1 topology for big repos)*
**Goal:** a 395-project framework Map is reviewable.
**Items:** **W4** (deterministic structural caps + ranking in `MapRenderer` topology & entries, with
"… and N more" disclosure — NOT token-driven).
**Exit gate (ratchet):**
- New test: aspnetcore Map line count < 250. `BudgetIndependenceTests` **stays green** (cap is a `const`, not
  `--max-tokens`). Scope stamp present on partial closures.
- **Scorecard:** Q1 PASS for aspnetcore.

## Phase 10.E — Coverage expansion  *(new archetypes — parallelisable with A–D)*
**Goal:** earn "any .NET repo" by benchmarking the shapes we have no data for.
**Items:** for each of {Worker/BackgroundService, Console/CLI, gRPC, Blazor (WASM+Server), MAUI/Avalonia,
classic MVC+Razor, Azure Functions/Lambda, a 2nd library}: clone a small canonical repo → register in
`eval-repos.json` + `eval/expectations/<name>.json` → capture Map+Trace into `../<name>/` → grade against
`OUTPUT-CONTRACT.md` → add a `BENCHMARK-MATRIX.md` row + any new work item.
**Exit gate:** per rung, the "ripgrep test" passes (entries resolve + one correct trace) and its acceptance
check is `expected`. (Blazor acceptance repo already named in the roadmap:
`eval-repos/blazor-samples/9.0/BlazorWebAppMovies`.)

## Phase 10.F — Engine hygiene  *(decision-gated — token is non-core)*
**Goal:** stop the dead token/catalog machinery from confusing output and audits.
**Items:** **W9** — decide with the user: (a) hide the catalog pruning funnel from `--stats` on narrative
runs; (b) reduce `--format json` to a serialization of `CodeGraph`/`MapModel` and **delete** the catalog +
`TokenBudgetEnforcer` + `PatternRelevancePruner` + `TokenBudget`; (c) document-only. (b) is the clean end
state.
**Exit gate:** if (b), `EvalExpectationTests` JSON checks migrate to graph-shaped JSON; full gate green; the
`--stats` funnel no longer describes a path the user can't see.

---

## At-a-glance sequencing

```
10.A entries/honesty ─┬─► 10.B dive-in ──► 10.C archetypes ──► 10.D readability
   (W1✅,W6,W8,L2)     │     (W3,W3b)         (W5,W7)             (W4)
                       └─► 10.E coverage (new archetypes)   ║ parallel
                       └─► 10.F hygiene/W9   ║ decision-gated, last
```
Recommended next pickup: finish **10.A** (run `eval/gates.ps1` to bank W1, then W6 + W8), then **10.B/W3**
(highest north-star leverage — makes "examine any part" real for libraries and unblocks desktop).
