# Tapestry Phase — Final Report & Handover

> **Read this first** if you're picking the project up cold. Branch chain
> `feat/tapestry-t0…t8` (stacked; the t8 tip contains the whole train) — closes the
> "Tapestry" track (`docs/dev/briefs/proposal-tapestry.md`), T0 through T8, 2026-07-15 →
> 2026-07-17. Successor to `HANDOVER-LOOM.md`. Checkpoint-level detail lives in
> `TAPESTRY-START.md` (tracker tables + drift table); this doc is the phase-level map.

## 1. What Tapestry Was

Loom left a truthful graph kernel. Tapestry made the whole product honest about it, end to
end: every AppEntry surface detected AND rendered AND gated (detect≠render was the phase's
recurring defect class), MCP v3 ergonomics, budget-filling context packs with staleness
verification, a Studio/Workbench that recomputes instead of decorating, and a perf story
with names instead of vibes. 40 checkpoints across 9 stages, all VERIFIED.

## 2. Stage Map (what landed where)

| Stage | Theme | Checkpoints | Key evidence (all under `eval-results/`) |
|-------|-------|-------------|------------------------------------------|
| T0 | Harness & hygiene — orphan-proof gates, CompositionApp fixture, truth re-baseline | 3/3 | `2026-07-15/tapestry-t0/T0-EVIDENCE.md` |
| T1 | Detection strength — catalog-driven entry seeds, Gateway/CLI/per-service archetypes, style arbitration, feature areas, entry taxonomy + kind single-sourcing, topology noise | 9/9 | `2026-07-15/tapestry-t1/T1-EVIDENCE.md` |
| T2 | Graph quality — production-first DI, member line stamping, type-focus shaping, param-dispatch seam, ONE event join, `global` never rendered, GraphBuilder split | 8/8 | `2026-07-15/tapestry-t2/T2.5-EVIDENCE.md`, `T2.6-EVIDENCE.md`, `T2.8-EVIDENCE.md` |
| T3 | MCP v3 — unified addressing, budgeted trace/entrypoints, config cache ≤500ms, CLI parity, report hygiene (476KB→34.5KB) | 8/8 | `2026-07-15/tapestry-t3/T3-MCP-EVIDENCE.md` |
| T4 | Context generation v2 — identity header, ≥85% budget fill, config/tests sections, provenance + tier mix, verify-context staleness API, contracts≠signatures | 6/6 | `2026-07-16/tapestry-t4/T4.1–T4.6-EVIDENCE.md` |
| T5 | Context Studio v2 — omitted[] end-to-end, verification panel, per-card copy/JSON export/provenance chips, kind-aware presets, card honesty, recompute-on-change | 6/6 | `2026-07-16/tapestry-t5/T5.1–T5.6-EVIDENCE.md` |
| T6 | Workbench & pages revamp — 7-page audit ×2 poles, canvas/insight/theme/keyboard honesty, first-run & session, MCP feed origin, one-pager fidelity | 12/12 | `2026-07-15/feature-design-audit.md`, `2026-07-16/tapestry-t6/T6-EVIDENCE.md` |
| T7 | Bench + perf honesty — battery 25min→13m23s, 3 new bench poles + 25-repo verdicts, perf baseline (the big-repo wall has one name), waterfall ≥95%, RPC budget ~150→8 | 5/5 | `2026-07-16/tapestry-t7/T7.1–T7.4-EVIDENCE.md`, `gates-t7.0-full-run2.txt` |
| T8 | Close-out — sample-collection render honesty, clean-clone battery, this doc | see §3 | `2026-07-17/tapestry-t8/` |

Full checkpoint tables with per-row commits: `TAPESTRY-START.md`.

## 3. T8 Close-out — This Session (2026-07-17)

| # | What | Evidence |
|---|------|----------|
| T8.2 | Sample-collection render honesty: `ProjectClassifier.SamplesAreTheProduct` (root-relative "every non-test/non-benchmark project is under a sample path") waives ONLY the sample-suppression rule across NoiseFilter entries, ArchetypeDetector ladder (5 points incl. the Orleans-named-sample framework short-circuit), RunnableProjects topology, chokepoint insight, library surface. aspire-samples: `LIBRARY (0 public types)`/no STYLE/0 entries → `MAP` + `STYLE SampleCollection (moderate)` + per-service rows + 5 entries (68n/34e/5e vs 58/31/0). MediatR-class repos byte-identical (T1.9 pins green). | commit on `feat/tapestry-t8`; `AspireSamples_style_is_sample_collection_not_microservices` un-pended → GREEN; +6 unit pins (`NoiseFilterTests`, `ArchetypeDetectorTests`) |
| T8.3 | Empty-fixture-dir → skip in `TruthExpectationTests` (github-ready G5 handoff item): fresh clones materialize gitlink paths as EMPTY dirs — bare `Directory.Exists` made un-inited fixtures FAIL the truth gate instead of skipping. `FixtureExists` = exists AND non-empty, all 15 sites. | `tests/DevContext.Core.Tests/TruthExpectationTests.cs` |
| T8.1 | Full battery on the t8 tip + clean-clone battery on the merged develop tip | `2026-07-17/tapestry-t8/gates-t8-tip.txt`, `gates-t8-cleanclone.txt` |
| T8.1 | HANDOVER-TAPESTRY.md (this doc) + AGENTS.md updates + tracker closed | this file, `AGENTS.md`, `TAPESTRY-START.md` |

## 4. Architecture Deltas (post-Loom → post-Tapestry)

What a returning agent must know beyond `HANDOVER-LOOM.md` §3:

- **Entry Surface Catalog** (`Core/Graph/EntrySurfaces/`) — every AppEntry surface is a
  catalog descriptor (signal key, kind, role, self-name patterns). Detection seeds, entry
  builders, archetype rules, and kind projection all read the SAME catalog — adding a
  surface is one descriptor + one builder. Kind is single-sourced from `CodeGraph.Entries`
  (no tag defaults, T1.8).
- **One event join** — `EventWiringProjection` is the ONLY bus join; board, one-pager, and
  flow all consume `graph.EventWiring` (T2.6). Legacy joins are deleted; don't re-grow them.
- **GraphBuilder is partial-split** — `GraphBuilder.{Flows,Entries,Nodes,Seams,ServiceLinks}.cs`
  (T2.8); the main file is ~118 lines of orchestration.
- **MCP v3 addressing** — every tool accepts a `query`; resolution is ranked + ambiguity-honest
  (`ResolveQueryAsync`). `trace` takes `budgetTokens` (BFS keep + named per-subtree omissions).
  Config lookups hit `AnalysisSession.ConfigBindings` (cached scan, T3.4). 24 tools.
- **Context packs** — identity header + repo-relative `file:line` everywhere; spine-first
  body expansion to ≥85% budget fill; config/tests/contracts sections built server-side;
  per-section provenance (`SectionProvenance`) and `FileFingerprints` snapshot →
  `VerifyContextPack`/`verify_context` staleness API (T4). The Studio is a THIN client over
  this: one `buildContext` path, recompute-on-change, exports are server-pack-or-disabled (T5.6).
- **Page-render RPC budget** — `GetFlowIndex` RPC + `FlowIndexBuilder` (Core) + per-session
  memo; `AtlasStore` hydrates from ONE call. Fresh page load = 8 RPCs (was ~150+), navs 0–1.
  Budget bar: ≤15/navigation (T7.4). Don't reintroduce per-node fan-outs in stores.
- **Samples-only repos** (T8.2) — `DiscoveryModel.SamplesAreTheProduct`; see §3. The flag is
  computed ONCE in the pipeline (root-relative) and stamped on the model; consumers never
  recompute it from absolute paths.
- **Stage waterfall** — every post-extraction phase is a named `PipelineStage` row; ≥95% of
  wall time accounted (shamshir 99.7%, T7.3). If you add a phase, clock it.

## 5. Perf Truth (T7.2 — carry this forward)

- **The big-repo wall is SemanticLite**, specifically building the merged compilation:
  DntSite 81.1s of 94.7s total; the bind itself is 3.2s (was 84s pre-Loom). OrchardCore edge
  growth 1708→11904 is COVERAGE (new seam detectors), not fabrication — bind stayed flat.
- **The named lever** (unclaimed, own checkpoint for a future phase): persist/reuse the
  merged compilation across sessions. Baselines: `PERF-2026-07-17-0036.md` vs the Jun-20 file.
- Bench cadence: `scripts/bench.ps1` (ASCII only — PS 5.1 mojibake killed it once, T7.1);
  25-repo verdict table in `2026-07-16/tapestry-t7/T7.1-EVIDENCE.md`.

## 6. Gate & Battery Discipline (as encoded, not aspirational)

`eval/gates.ps1` is ONE script with a scope knob (T7.0): `-Scope full` (only boundary-citable
form; ~13min — eval stamp-cached via `eval/.eval-stamp.json` + split over two test hosts),
`-Scope engine`, `-Scope app` (~90s), `-SkipEval` (self-labels not-a-merge-gate). Full battery
at boundaries ONLY (stage close, pre-push of dotnet commits, pre-merge); launch it DETACHED
and keep working (overlap rule) — only push/merge waits for `GATE:` in the log. Details:
`.claude/skills/dev-pipeline/SKILL.md` + `TAPESTRY-START.md` §Battery cadence.

## 7. Known Gaps / Next Threads

1. **Persist/reuse merged compilation** — the named perf lever (§5). Biggest single win
   available; scoped as its own future checkpoint.
2. **GrpcAggregator style** — `SampleCollection*` via the no-unifying-solution branch
   (Client+Server, no .sln). Acceptable, noted in T7.1; a per-kind style rung could name it
   better.
3. **Fast-suite load-flake** — one Core test fails ONCE when run right after other dotnet
   churn, green when quiet; name still uncaptured (watch battery logs; pinned in tracker
   gotchas since T7).
4. **eval-results/ tracked-file volume** (432 files) + `analysis-exports/` — owner decision
   pending (github-ready G4 audit); nothing blocks on it.
5. **Engine debt register** — `conductor-DEBT.md` (SymbolTable member indexing, BodyFacts
   scoping, TfmScore, Flow hardening) — unchanged by Tapestry, still real.

## 8. The Poles (regression baselines)

Byte-stable baselines to diff against (full drift table in `TAPESTRY-START.md`):

| Repo | Nodes/Edges/Entries | Style / note |
|------|--------------------|--------------|
| dogfood (eshop-microservices) | 439 / 339 / 34 | Microservices (App); carries PRE-EXISTING local mods since 2025-12-12 — never restore |
| eShop | 1089 / 837 / 109 | Microservices 0.91 — unchanged since T2.5 |
| shamshir | ~2882 / 3375 / 135 | NLayer 0.6; live repo, <1.2% source drift per session is normal |
| TodoApi | 123 / 81 / 12 | MinimalApi |
| aspire-samples | 68 / 34 / 5 | SampleCollection (T8.2 fix; was 58/31/0 as a dishonest Library) |

Dogfood path: `C:\Users\shahi\source\repos\run-aspnetcore-microservices\src` · second pole:
`C:\code\shamshir`.
