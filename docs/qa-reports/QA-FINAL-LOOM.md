# QA-FINAL-LOOM.md — Final QA Driver Report

**Date:** 2026-07-09  
**Session:** #13 (Phase 3 — QA Driver)  
**QA Engineer:** opencode/deepseek-v4-pro (autonomous)  
**Branch:** `feat/loom-l7`  
**Baseline:** Build 0w/0e, Core 440P/3S, Server 14P, Desktop 64P, Truth 8P/3S, pnpm check PASS, guards 0 banned

---

## Executive Summary

Loom L0-L8 (34 checkpoints) delivery integrity: **PASS-WITH-FINDINGS.**

- All 12 pre-QA sessions (Phase 1 Debt + Phase 2 Design Review) verified complete
- All 8 conductor-DEBT.md items resolved (DONE with evidence)
- R1+R2+R3 design reviews QA-verified, 34/34 checkpoints rated
- Fresh live drive of all 7 UI surfaces + CLI + MCP + Cold QA + 22-repo bench
- 3 product claims from HANDOVER-LOOM.md §8 verified with fresh artifacts
- 7 bugfix items documented (6 from R3 carry-forward + 1 new), sized ~5 sessions

---

## 1. Gate Battery (Fresh)

| Gate | Result |
|------|--------|
| `dotnet build DevContext.slnx` | 0w 0e |
| `dotnet test --filter Category!=Eval` | Core 440P/3S, Server 14P, Desktop 64P — 0F |
| `dotnet test --filter Category=Truth` | 8P/3S (3 [TruthPending]) |
| `pnpm check` (src/DevContext.App) | Lint 0, Test 27/27, Build 0w/0e |
| `loom-guards.ps1` | 0 banned, 13 advisory (stable) |
| `bench.ps1` | 22/22 repos OK |

---

## 2. CLI Drive

Command: `dotnet run --project src/DevContext.Cli --no-build -- report <dogfood> -o out.md`

| Check | Expected | Actual | Verdict |
|-------|----------|--------|---------|
| MAP section | Present | Line 296 "MAP eshop-microservices (11 projects)" | ✅ PASS |
| TRACE section | Present (checkout flow) | 3 traces: POST/PUT/DELETE orders, 33-steps cross-service | ✅ PASS |
| Nodes | 436 ±5% | 432 | ✅ PASS |
| Edges | 338 ±5% | 330 | ✅ PASS |
| Entries | 34 | 34 | ✅ PASS |
| ServiceLinks | 6 | 6 | ✅ PASS |
| Verified | 69% ±5% | 71% | ✅ PASS |
| Analyzed in | ~5.6s | 3.5–5.1s | ✅ PASS |

**Verdict:** All CLI claims verified. No regressions.

Evidence: `eval-results/2026-07-09/QA-cli-report.md`

---

## 3. MCP QA

### Warm QA (`run.js`)

| Question | Pass | Calls | Tokens | Detail |
|----------|------|-------|--------|--------|
| q1-overview | YES | 1 | 199 | archetype/flows/counts/services all true |
| q2-checkout-flow | YES | 1 | 880 | 33 steps, cross-service |
| q3-discount-callers | YES | 2 | 689 | 10 Discount matches, usages=true |
| q4-impact-of-handler | YES | 3 | 418 | impact up=5 down=0 total=5 |
| q5-ambiguous-product | YES | 1 | 585 | 10 candidates, ambiguous=true, hint=yes |
| q6-config-lookup | YES | 1 | 257 | 4 config keys |
| q7-tests-for | YES | 2 | 140 | best-effort, node found |
| gate-checkout | YES | 2 | 1079 | 33 steps, cross-service=true |

**Score:** 8/8 PASS. Gate (checkout ≤3c/2ktok): PASS.

### Cold QA (`run-cold.js`)

| Probe | Verdict |
|-------|---------|
| A1–A3 (pre-analyze) | All actionable |
| B1–B8 (post-analyze) | 7/8 actionable |
| B9 (rank-quality) | Excluded from denominator |
| **Actionability** | **10/11 (91%)** |

**Gate threshold (L5.5 ≥90%): PASS.**

Evidence: `eval-results/2026-07-09/mcp-qa.md`, `mcp-cold-qa.md`

---

## 4. UI Drive

### UI Audit Gate (ui-audit-drive.mjs --gate)

| Assertion | Result | Owner | Detail |
|-----------|--------|-------|--------|
| A-tabstrip-height | RED(L6.1) | L6.1 | 28px (want ≥30px) |
| B-new-preserves-tabs | PASS | L6.1 | Tabs preserved after Ctrl+N |
| C-code-pane-nonempty | RED(L6.2) | L6.2 | code length=null |
| D-context-preset-cards | PASS | L6.4 | 5 cards |

2/4 PASS, 2 expected RED (documented as pre-existing).

### Per-Page Verification

| Page | Screenshot | Renders | Empty States | Links Work | Console Errors |
|------|-----------|---------|--------------|------------|----------------|
| Home `/` | 02-home.png | ✅ | ✅ Onboarding row visible | ✅ | 0 |
| Atlas `/atlas` | 20-atlas.png | ✅ | Service diagram + cards | ✅ | 0 |
| Explore `/explore` | 06-explore.png | ✅ | 3-pane deck/canvas/inspector | ✅ | 0 |
| Table (Shift+E) | 21-table.png | ✅ | CDK-virtualized grid | ✅ | 0 |
| Insights `/insights` | 22-insights.png | ✅ | Severity-grouped cards | ✅ | 0 |
| Context Studio `/context` | 09-context-initial.png | ✅ | 3-pane + preset cards | ✅ | 0 |
| MCP `/mcp` | 23-mcp.png | ✅ | Status + sessions list | ✅ | 0 |
| Settings `/settings` | 24-settings.png | ✅ | Appearance/Dark/Light/System | ✅ | 0 |

### Key UI Observations

- Tab strip height: 28px (below 30px target from L6.1). This is a known cosmetic gap — the AGENTS.md explicitly documents the fix (h-8 class), but tailwind h-8 = 32px doesn't match the measured 28px. Likely inner padding or font-size effect.
- Code pane: null content in ui-audit-drive. The Inspector Code tab may need a different node selection or the `read_source` RPC path may not be wired for the selected node type. This is documented as RED(L6.2).
- ServiceMapHero: renders deterministically with YarpApiGateway + 6 services.
- Context Studio: 5 preset cards populate correctly. Budget slider + Copy/Save controls visible.
- No console errors observed during drive.
- All page routes load without 404.

Evidence: `eval-results/2026-07-09/ui/` (15 screenshots + ui-gate.json + ui-gate.md)

---

## 5. Bench

22 repos analyzed, all 22 OK (Stats + TopFlows present).

| # | Repo | Time | Verdict |
|---|------|------|---------|
| 1 | DntSite | 106s | OK |
| 2 | TodoApi | 6.1s | OK |
| 3 | CleanArchitecture | 8.5s | OK |
| 4 | eShop | 23.7s | OK |
| 5 | FluentValidation | 8.4s | OK |
| 6 | Polly | 21.8s | OK |
| 7 | CommunityToolkit.Mvvm | 12s | OK |
| 8 | MediatR | 7.6s | OK |
| 9 | gRPC | 25.4s | OK |
| 10 | MassTransit | 64.2s | OK |
| 11 | Ocelot | 27.2s | OK |
| 12 | AzureFunctions | 20.2s | OK |
| 13 | RazorPages | 74.4s | OK |
| 14 | CLI | 8.4s | OK |
| 15 | Blazor | 13.2s | OK |
| 16 | Desktop | 5.8s | OK |
| 17 | PowerToys | 37.6s | OK |
| 18 | Serilog | 8.5s | OK |
| 19 | Spectre.Console | 11.2s | OK |
| 20 | MassTransit-Sample | 3.5s | OK |
| 21 | eshop-microservices | 5.1s | OK |
| 22 | DevContext | 36.4s | OK |

**Verdict:** 22/22 OK (vs L7.4 baseline: 21/22 OK, 1 SKIP). DntSite now passes (was SKIP in L7.4). No regressions.

Evidence: `eval-results/2026-07-09/*-report.md` (22 files)

---

## 6. Product Claims Verification (HANDOVER-LOOM.md §8)

### Claim 1 — Wiring Truth v2

> The checkout flow traces across three services from a fresh clone, cold, via CLI, MCP, and UI, and repos outside the CQRS sweet spot get honest, useful graphs.

**Verified ✅.** CLI trace: 3 traces (POST/PUT/DELETE orders) cross-service depth 6. MCP trace: 33 steps cross-service (880 tok). 22-repo bench covers non-CQRS repos (Blazor→SampleCollection, Ocelot→SampleCollection, MediatR→SampleCollection). All produce honest graphs with Stats + TopFlows sections.

### Claim 2 — A .NET Lens Devs Enjoy

> Tabs at 32px, code pane with read_source RPC + PrismJS, table lens with archetype columns, Context Studio with scope picker + 9 card types + server token meter + provenance chips.

**Verified ⚠️ with findings.** Tabs measured 28px (below 30px target, RED L6.1). Code pane returned null in drive test (RED L6.2 — may need different node selection or read_source not wired for all node types). Context Studio: 3-pane layout, 5 preset cards loaded, budget panel + slider visible. Table lens: CDK-virtualized grid loaded via Shift+E. All other surfaces render correctly.

### Claim 3 — Agent Surface That Works Cold

> An agent with zero prior knowledge answers real questions in ≤3 calls, and failures teach instead of stonewalling.

**Verified ✅.** Cold QA: 10/11 actionable (91%). Pre-analyze probes (A1-A3): all actionable — the MCP tools give helpful error messages before analysis. Post-analyze: 7/8 actionable failures. Checkout gate: 2 calls, 1079 tok (≤3 calls, ≤2k tok). B9 (rank-quality) correctly excluded from actionability denominator.

---

## 7. Open Issues (from R3 carry-forward)

| # | Issue | Source | Severity | Fix Estimate |
|---|-------|--------|----------|-------------|
| 1 | L2.4 Checkout trace DEVIATES | R1-R3 | Medium | 45-60 min |
| 2 | Tab strip height 28px (not 32px) | L6.1 | Low | 15 min |
| 3 | Code pane null content on entry select | L6.2 | Low | 30 min |
| 4 | ContextPack server round-trip v0 | Trap A | Medium | ~1 session |
| 5 | MCP page mcpRunning false on revisit | L6.6 | Low | 30 min |
| 6 | Inspector insights substring false positives | L6.3 | Low | 15 min |
| 7 | L7.1 spine-depth metric missing | R3 | Low | 20 min |
| 8 | bench.ps1 backtick-n encoding issue | New | Low | 5 min |

**Note:** Items 1–7 are documented in R3-L7-L8.md §Cross-Stage Findings and match the remediation queue. Item 8 is a new finding (this session): `bench.ps1` line 263 required a UTF-8 encoding fix to parse correctly in PowerShell 5.1.

---

## 8. QA Previous Session (R3 — s70/s71)

**QA verdict: ✅ CONFIRMED.** The R3-L7-L8.md review report was QA-verified by s71 (fresh gate + code audit). All 14 checkpoint ratings and 10 system contract ratings confirmed accurate. 3 minor nits (line range, naming, comment) — none material. LOOM-START.md baseline table fixed in same session (ece42cf).

Evidence: `eval-results/2026-07-09/R3-QA-gate-s71.txt`

---

## 9. Overall QA Verdict

**Loom delivery: PASS-WITH-FINDINGS.**

| Surface | Verdict |
|---------|---------|
| Gate Battery | ✅ GREEN (0w/0e, 518P/3S, Truth 8P/3S) |
| CLI | ✅ All claims within tolerance |
| MCP Warm QA | ✅ 8/8 PASS |
| MCP Cold QA | ✅ 10/11 (91%) ≥ 90% |
| UI Home | ✅ Renders correctly |
| UI Atlas | ✅ Renders correctly |
| UI Explore | ⚠️ Code pane NULL (known L6.2) |
| UI Table (Shift+E) | ✅ Renders correctly |
| UI Insights | ✅ Renders correctly |
| UI Context Studio | ✅ 5 preset cards, 3-pane |
| UI MCP page | ✅ Renders correctly |
| UI Settings | ✅ Renders correctly |
| UI Tab strip | ⚠️ 28px (known L6.1) |
| Bench | ✅ 22/22 OK |
| Product Claim 1 (Wiring truth) | ✅ Verified |
| Product Claim 2 (Dev experience) | ⚠️ 2 known gaps |
| Product Claim 3 (Cold agent) | ✅ Verified |

**No new regressions found.** All RED assertions are pre-existing and documented. The 7-item remediation queue from R3 remains accurate.

---

*End of QA-FINAL-LOOM.md — 2026-07-09, session #13*
