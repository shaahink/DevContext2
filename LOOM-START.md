# Loom — Phase Tracker (resume here)

**Read order for a fresh session:** this file → `AGENTS.md` (current phase protocol) →
`conductor-DEBT.md` (debt catalog, sized + gated) → `docs/workflows/loom-debt-workflow.md`
(the workflow for the current phase).
Branch: `develop` (after merge). Dogfood: `C:\Users\shahi\source\repos\run-aspnetcore-microservices\src`.

## Handoff (overwrite this block, ≤12 lines, no history)
last: D7 QA (L2.5) — s57 audit of s56 D7: all 4 claims verified (lambda scope fix via
        GetEnclosingParamType, BuildTypeEventSets+BuildSeamContext dedup, multi-lambda test,
        no SeamDetector regression). Gate battery green (433P Core, 11 BodyFactExtractor,
        12 SeamDetector, 8P/3S Truth). QA: PASS.
stage: D7 COMPLETE (verified s57). Next: D8 (L4.5 — Flow model hardening: depth, budget, kind, integration test).
trap: None. Advisory (13 NodeId.ForType) unchanged. Truth ratchets stable (8P/3S).

---

## ✅ Loom delivery — COMPLETE (34/34 checkpoints)

L0-L8 were delivered across 41 sessions. All gates green at phase-end. See `docs/dev/HANDOVER-LOOM.md`
for the full close-out. The table below is preserved for reference; all rows are DONE.

| # | Checkpoint | Status | Session | Evidence |
|---|-----------|--------|---------|----------|
| L0.1-L0.3 | Truth harness + cold-QA baseline + UI drive | DONE | s1-s4 | `eval-results/2026-07-07/` |
| L1.1-L1.5 | Identity spine: SymbolTable, Service nodes, guards | DONE | s5-s6 | `eval-results/2026-07-07/gate-battery-l1-s5.txt` |
| L2.1-L2.4 | BodyFacts + 5 seam detectors + regex funeral | DONE | s7-s10 | `eval-results/2026-07-07/gate-battery-l2-s8.txt` |
| L3.1-L3.3 | Semantic-lite Tier B (81% verified) | DONE | s11-s17 | `eval-results/2026-07-08/gate-battery-l3.3-s16.txt` |
| L4.1-L4.4 | Flows + projections + ContextPack | DONE | s18-s22 | `eval-results/2026-07-08/gate-battery-l4.4-s21.txt` |
| L5.1-L5.5 | MCP v2 cold-agent (≥90% actionable) | DONE | s23-s28 | `eval-results/2026-07-08/gate-battery-l5.5-s28.txt` |
| L6.1-L6.6 | Workbench repair (tabs, code, studio, table) | DONE | s29-s33 | `eval-results/2026-07-08/gate-battery-l6-session-33.txt` |
| L7.1-L7.4 | Repo-shape coverage (archetypes, 22-repo bench) | DONE | s34-s39 | `eval-results/2026-07-08/bench-verdicts-l7.4-s38.md` |
| L8.1 | Close-out (handover, AGENTS.md, rituals) | DONE | s40-s41 | `docs/dev/HANDOVER-LOOM.md` |

---

## Post-Loom checkpoints (in progress)

Status ∈ TODO · IN PROGRESS · DONE · BLOCKED. Evidence under `eval-results/<date>/`.
Three phases: Debt Cleanup (1-9) → Design Review (10-12) → QA Driver (13).

### Phase 1: Merge + Debt Cleanup

| # | Checkpoint | Status | Evidence |
|---|-----------|--------|----------|
| 1 | L0.5 — Cold-QA B9 denominator + UI boot-liveness | DONE | `eval-results/2026-07-09/debt-L0.5-gate.txt` |
| 2 | L3.5 — TodoApi eval gap triaged | DONE | `eval-results/2026-07-09/debt-L3.5-gate.txt` |
| 3 | L5.x — Audit-trap sweep (5 items) | DONE | `eval-results/2026-07-09/debt-L5.x-gate-attempt2.txt`, attempt3 re-verified s49 |
| 4 | Merge feat/loom-l7 → develop (squash per L-stage) | TODO | merge commits |
| 5 | L0.4 — Truth gate in battery + TruthPending sweep | DONE | `eval-results/2026-07-09/debt-L0.4-gate.txt` (s53) + QA `debt-L0.4-QA-gate-s54.txt` |
| 6 | L3.4 — TfmScore handles net10.0+ | DONE | `eval-results/2026-07-09/debt-L3.4-gate-attempt2.txt` |
| 7 | L2.5 — Lambda scope pollution + SeamContext dedup | DONE | `eval-results/2026-07-09/debt-L2.5-gate.txt` |
| 8 | L4.5 — Flow model hardening | TODO | `eval-results/<date>/debt-L4.5-gate.txt` |
| 9 | L1.6 — SymbolTable member indexing | TODO | `eval-results/<date>/debt-L1.6-gate.txt` |

### Phase 2: Static Design Review

| # | Checkpoint | Status | Evidence |
|---|-----------|--------|----------|
| 10 | R1 — L0+L1+L2 review (truth, spine, bodyfacts) | TODO | `docs/design-reviews/R1-L0-L3.md` |
| 11 | R2 — L4+L5+L6 review (flows, MCP, workbench) | TODO | `docs/design-reviews/R2-L4-L6.md` |
| 12 | R3 — L7+L8 + system-level contracts review | TODO | `docs/design-reviews/R3-L7-L8.md` |

### Phase 3: Final QA

| # | Checkpoint | Status | Evidence |
|---|-----------|--------|----------|
| 13 | QA Driver — full live UI + CLI + MCP + bench | TODO | `docs/qa-reports/QA-FINAL-LOOM.md`, `QA-BUGFIXES.md` |

---

## Baseline numbers

| Metric | Value |
|---|---|
| Dogfood | 436 nodes · 338 edges · 34 entries · 6 ServiceLinks · depth 6 · ~5.2s |
| Checkout trace depth | 6 (L2.4) |
| Cold-agent MCP actionability | 90% (L5.5) |
| Tab strip height | 32px (L6.1) |
| Truth tests | 9P/2F/0S (DntSite cloned — was skip, now passes. 2 failures flagged for Phase 2 review) |
| eShop (non-CQRS proxy) | 479 nodes · 375 edges · 96 entries |

## Quick commands

```powershell
dotnet build DevContext.slnx                                   # 0w 0e is the bar
dotnet test DevContext.slnx --filter "Category!=Eval"
powershell -File scripts/bench.ps1                             # presence bench (L0 adds -Truth)
node eval/mcp-qa/run.js                                        # scripted QA (kept as regression)
node eval/mcp-qa/run-cold.js                                   # L0.2+ cold-agent QA
cd src/DevContext.App; pnpm check                              # UI gate
node src/DevContext.App/scripts/ui-audit-drive.mjs             # UI drive gate (server+ng required)
dotnet run --project src/DevContext.Cli --no-build -- report <abs-repo-path> -o out.md
```
