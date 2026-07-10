# Loom — Phase Tracker (resume here)

**Read order for a fresh session:** this file → `AGENTS.md` (current phase protocol) →
`docs/workflows/loom-gap-close-plan.md` (plan doc with full history + fix detail per phase) →
`docs/qa-reports/QA-BUGFIXES.md` → `docs/workflows/loom-debt-workflow.md` (rituals).
Branch: `feat/loom-l7` (no merge until Phase F passes). Dogfood: `C:\Users\shahi\source\repos\run-aspnetcore-microservices\src`.

## Handoff (overwrite this block, ≤12 lines, no history)
last: s13 Phase B — B1+B2 fixes delivered. Gate battery re-verified green.
stage: Phase B DONE (B1 tab strip min-h-8, B2 code auto-load on open). Phase A VERIFIED (QA s79, 9P/2S truth).
next: Phase C (Polish Batch: MCP mcpRunning + Inspector word-boundary + bench encoding + spine metric + perf doc).
gate: Build 0w/0e, Core 440P/3S, Server 14P, Desktop 64P, Truth 9P/2S, pnpm check PASS, guards 0 banned.
evidence: eval-results/2026-07-10/phase-B-gate-battery.txt


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

## Post-Loom Gap Close — IN PROGRESS (6 phases, 14 checkpoints)

Status ∈ TODO · IN PROGRESS · DONE · BLOCKED · VERIFIED.
Evidence under `eval-results/<date>/`.
Plan doc: `docs/workflows/loom-gap-close-plan.md` — read before each session.
13 gaps from QA-BUGFIXES.md + R1/R2/R3 carry-forwards, re-investigated for root causes.
Previous 3 phases (Debt Cleanup, Design Review, QA Driver) are DONE (13/13 sessions).

### Phase A: Engine Gap — L2.4 Checkout Trace Bus-Publish

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| A1 | Fix Type→Service bridge in TraceBuilder + FlowModel, flip [TruthPending], verify 9P/2S truth | VERIFIED | a94c211 (fix), d062f39 (QA s77), 31796b4 (QA s78), [current] (QA s79) | `eval-results/2026-07-10/phase-A-s79-fresh-qa.txt`, `eval-results/2026-07-10/phase-A-checkout-trace-verified.md` |
> fix: BodyFactsExtractor missing from TestPipeline.Build() — added + auto-extract fallback hardened. Bridge code in commit 4d997d9 was always correct. Checkout trace follows cross-service hop through BasketCheckoutEvent→BasketCheckoutEventHandler→CreateOrderCommand. QA s79 fresh gate battery + CLI analyze --focus trace confirmed: Raises edge from BusPublishDetector works on real dogfood repo, Type→Service bridge connects BasketCheckoutEvent to Ordering.Application, depth 6.

### Phase B: UI Regressions (QA Driver s73 RED assertions)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| B1 | Tab strip height ≥30px — fix box model, verify ui-audit-drive assertion A green | DONE | [current] | `eval-results/2026-07-10/phase-B-gate-battery.txt` |
| B2 | Code pane non-null — debug read_source RPC, fix node coverage, verify assertion C green | DONE | [current] | `eval-results/2026-07-10/phase-B-gate-battery.txt` |

### Phase C: Polish Batch (6 small items, 1 session)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| C1 | MCP page mcpRunning queries server state on mount | TODO | | |
| C2 | Inspector insights use word-boundary matching, not substring | TODO | | |
| C3 | bench.ps1 encoding fix — replace backtick-n with Environment::NewLine | TODO | | |
| C4 | L7.1 spine-depth metric added to GraphStats + CLI report output | TODO | | |
| C5 | Perf budget doc updated (≤6s) + LOOM-START baseline truth count fixed (8P/3S) | TODO | | |

### Phase D: ContextPack Server Round-Trip (Trap A)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1 | ContextPackBuilder serializes markdown server-side; client uses server output | TODO | | |

### Phase E: Eval Gap Investigation (HANDOVER-LOOM §7.1 Eval-1/Eval-2)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| E1 | eShop TraceQuality tests: fixed or documented as known limitation | TODO | | |
| E2 | EvalExpectationTests verticalslice: fixed or documented | TODO | | |
| E3 | PROGRESS-LOG.md backfilled with L5-L8 sessions from .conductor/handovers/ | TODO | | |

### Phase F: Final QA Close-out

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| F1 | Full gate battery green: build + tests + truth (9P/2S) + pnpm + UI gate + MCP QA + bench + guards | TODO | | |
| F2 | HANDOVER-LOOM.md §7 updated with resolved gaps + LOOM-START.md final handoff | TODO | | |

---

## Baseline numbers

| Metric | Value |
|---|---|
| Dogfood | 436 nodes · 338 edges · 34 entries · 6 ServiceLinks · depth 6 · ~5.2s |
| Checkout trace depth | 6 (L2.4) |
| Cold-agent MCP actionability | 90% (L5.5) |
| Tab strip height | 32px (L6.1) |
| Truth tests | 9P/2S (2 [TruthPending] ratchets: L1 server names, L1 RazorPages; DntSite + checkout pass) |
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
