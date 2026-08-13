# Conductor — DevContext pre-release - engine and agent face run report

_Updated 2026-08-13 17:19 UTC · branch `feat/pre-release-engine` · HEAD `7061953`_

**Status:** Idle
**Stage:** T1 — W1 agent-surface trust pack (bug 5 by measurement, curated menu, partial-truth family, wire-truth gate) · attempts used 0 · working ▸ T1.4
**Checkpoints:** 3/20 done · **Sessions run:** 2 · **Cost:** $25.3623 (agent $25.3069 + gates $0.0554) · **Tokens:** 376,485 in / 163,809 out

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| T1 | W1 agent-surface trust pack (bug 5 by measurement, curated menu, partial-truth family, wire-truth gate) | ████████░░ 3/4 | **← active** |
| V1 | W2a one-vocabulary pack (25, 17, 7-rider invariant, 18) | ░░░░░░░░░░ 0/3 | todo |
| E1 | W2 edge completeness batch (11, 12 via TextSpan, re-measure 8, 7) + dogfood invariant | ░░░░░░░░░░ 0/4 | todo |
| D1 | W5 detection declared-coverage (reachability instrument, hole closures, filed set, rung-4 jobs) | ░░░░░░░░░░ 0/4 | todo |
| R1 | W6 metric recalibration (22, 23, 24) against the post-E1 graph | ░░░░░░░░░░ 0/1 | todo |
| A1 | W3 re-probe prep (DESIGN amendments, unseen repo) + the 10-dollar adoption gate | ░░░░░░░░░░ 0/2 | todo |
| Z1 | Close-out: README honesty pass, release-gate statuses, backlog reconciliation | ░░░░░░░░░░ 0/2 | todo |

<details><summary>T1 — W1 agent-surface trust pack (bug 5 by measurement, curated menu, partial-truth family, wire-truth gate) (3/4)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| T1.1 | Bug #5 fixed by measurement: a real MCP `tools/list` call shows a non-empty description for EVERY tool on the wire (mechanism verified against what the ModelContextProtocol SDK actually reads, not assumed); schema tax re-measured and recorded against the P1.2 baseline | ✅ DONE | [`76c42dd`](https://github.com/shaahink/DevContext2/commit/76c42dd) |
| T1.2 | Menu curated: core agent surface shipped (PRODUCT-DIRECTION §7 set + navigation primitives as the candidate), the rest demoted or folded, retired names handled by the did-you-mean path reading the REAL tool list; `tools/list` shows the curated described menu | ✅ DONE | [`76c42dd`](https://github.com/shaahink/DevContext2/commit/76c42dd) |
| T1.3 | Confident-partial-truth family closed on the agent path: #6 trace-by-nodeId routes through the resolver `get_context` uses; #10 invalid enum values rejected with the good error envelope; #9 fillNote names elision and the budgetTokens lever; #2 entrypoint names round-trip into `get_context`/`trace` — evidence is real MCP calls showing each new shape | ✅ DONE | - |
| T1.4 | Wire-truth gate in the battery, proven RED on the pre-fix build then green: every tool described, invalid mode/direction/format rejected, a `found:true` trace has steps or says why not, every elision named | ⬜ TODO | - |

</details>

<details><summary>V1 — W2a one-vocabulary pack (25, 17, 7-rider invariant, 18) (0/3)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| V1.1 | ONE verified-edge definition (#25): GraphStats/SeamStat and GraphOrphansSource agree, the definition stated in one place, every surface reads it | ⬜ TODO | - |
| V1.2 | ONE member-title helper (#17) next to SymbolCanon used by every producer; the owner-qualified vs bare split gone on the six poles it was measured on | ⬜ TODO | - |
| V1.3 | Two standing invariants land red-first: no node carries kind Type with a member id (#7 rider); lambda/expression text never becomes a node title on any path (#18) | ⬜ TODO | - |

</details>

<details><summary>E1 — W2 edge completeness batch (11, 12 via TextSpan, re-measure 8, 7) + dogfood invariant (0/4)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| E1.1 | #11 static type-name-receiver calls produce edges; the dogfood invariant (DevContext's own helper layer has in-edges in DevContext's own graph) added to the battery and proven red first | ⬜ TODO | - |
| E1.2 | #12 fixed via the TextSpan-on-op shape (BodyOp records the invocation's own span; relocate-by-line class killed including the TryBindLocalDeclType / TryBindGenericArg / args-bind sister sites); full matrix run against cells declared BEFORE coding; engine-own approx share recorded before/after (baseline 1103/1383) | ⬜ TODO | - |
| E1.3 | #8 re-measured (its stated mechanism is refuted by #11) and fixed or re-filed with the true mechanism; #7 explicit-interface/BCL-collision Type-node fixed | ⬜ TODO | - |
| E1.4 | Batch acceptance: the probe's class-C impact question resolves the true impact set on eShop (hand-verified against the question key), matrix shows declared cells flipped and no others | ⬜ TODO | - |

</details>

<details><summary>D1 — W5 detection declared-coverage (reachability instrument, hole closures, filed set, rung-4 jobs) (0/4)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| D1.1 | Catalog-reachability instrument: for every catalog descriptor a test asserts its signal is reachable and its Kind has a producing path (descriptor→signal→extractor→builder→entry); proven RED on the Orleans and TimedJob finds before any closure | ⬜ TODO | - |
| D1.2 | Reachable-surface holes closed with consumer-app fixtures + expectations: Orleans packages on the descriptor; BackgroundService/IHostedService base-type detector; TimedJob producer or honest deletion of the kind; Avalonia descriptor + WinForms Exe case | ⬜ TODO | - |
| D1.3 | Filed set: #14 generic command verbs (strip type args in leaf comparison, carry the type arg as parent); #20/#19 one source of truth for "what is a service"; #2's detection half (addressable entry names single-sourced); Blazor UI-vs-HTTP distinction; per-detection Confidence read or deleted | ⬜ TODO | - |
| D1.4 | Rung 4: Hangfire and Quartz consumer job entries (attribute + interface shapes), one consumer-app fixture each | ⬜ TODO | - |

</details>

<details><summary>R1 — W6 metric recalibration (22, 23, 24) against the post-E1 graph (0/1)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| R1.1 | #22/#23/#24 recalibrated against the post-E1 graph or retired; each surviving threshold carries a comment stating the measurement and the calibration commit | ⬜ TODO | - |

</details>

<details><summary>A1 — W3 re-probe prep (DESIGN amendments, unseen repo) + the 10-dollar adoption gate (0/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| A1.1 | DESIGN.md amended IN WRITING before any run, per RESULTS §10: unseen-repo protocol (identifier-renamed public repo prepared and committed), question mix (2× class B, E/F at half), non-inferiority bar re-specified at a reachable level, κ-on-20% rule with degenerate-marginal handling, B-instructed arm added | ⬜ TODO | - |
| A1.2 | The pre-registered adoption gate: arm B alone, eShop, 18 runs, unchanged prompt, run as a bg child; mcp_call_share reported against the 0.2 floor with the pre-registered branch stated either way | ⬜ TODO | - |

</details>

<details><summary>Z1 — Close-out: README honesty pass, release-gate statuses, backlog reconciliation (0/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| Z1.1 | README honesty pass: agent story stated as measured (primer + whatever A1.2 earned), results linked, claims ⊆ measurements; release-gate table statuses recorded | ⬜ TODO | - |
| Z1.2 | BUG-BACKLOG reconciled (fixed closed with evidence, re-measured re-statused); PRE-RELEASE-PLAN §3 stage table updated; full battery green; branch pushed | ⬜ TODO | - |

</details>

## Sessions

| # | Stage | Kind | Att | Started (UTC) | Dur | Outcome | New DONE | Commits | Gates | Cost | Overhead | Tokens |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | T1 | Deliver | 1 | 08-13 16:16 | 0:29 | Advanced | T1.1 T1.2 | 4 | fast-engine:OK · guards:OK | $12.9213 | $0.0278 | 193,772/93,007 |
| 2 | T1 | Deliver | 1 | 08-13 16:50 | 0:24 | Advanced | T1.3 | 2 | fast-engine:OK · guards:OK | $12.3856 | $0.0276 | 182,713/70,802 |

## Money

_What this run has cost, from its own `costs` rows. Same numbers as `conductor money`._

| scope | sessions | tokens | cache reads | cost | checkpoints | tok/ckpt | $/ckpt |
|---|---|---|---|---|---|---|---|
| **run total** | 1 | 17.6M | 98.4% | $12.95 | 2 | 8.8M | $6.47 |
| stage T1 | 1 | 17.6M | 98.4% | $12.95 | 2 | 8.8M | $6.47 |
| 2026-08 | 1 | 17.6M | 98.4% | $12.95 | 2 | 8.8M | $6.47 |

_Where the money goes: agent $12.92 (100%) · gate $0.03 (0%) · blended $0.74/M tokens._

## Timeline

_Transitions with duration, from the event log (`.conductor/events.jsonl`)._

```
08-13 17:16:00  ◆ run started · DevContext pre-release - engine and agent face
08-13 17:16:01  ▸ stage T1 entered — W1 agent-surface trust pack (bug 5 by measurement, curated menu, partial-truth family, wire-truth gate)
08-13 17:16:01  • session #1 T1 Deliver started (attempt 1/6)
08-13 17:50:24  ▪ gate fast-engine pass [session]  (3m16s)
08-13 17:50:24  ▪ gate guards pass [session]  (1m20s)
08-13 17:50:28  • session #1 T1 → Advanced · done T1.1,T1.2 · 4 commit(s)  (34m26s)
08-13 17:50:29  • session #2 T1 Deliver started (attempt 1/6)
08-13 18:19:07  ▪ gate fast-engine pass [session]  (3m01s)
08-13 18:19:07  ▪ gate guards pass [session]  (1m35s)
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 2 · retries 0 (0 %) · overall Ok
✓ no health concerns detected
```

## Repo

_Live git snapshot (branch, working tree, sync vs upstream)._

```
branch: feat/pre-release-engine
working tree: M eval-results/2026-08-13/mcp-qa.md
vs upstream: up to date
```

### Commits by session

- **s1 (T1 Deliver)** — 4 commit(s):
  - [`5b6fe5d`](https://github.com/shaahink/DevContext2/commit/5b6fe5d) fix(mcp): bug #10 - an out-of-range enum is rejected, not silently re-read (T1.3 partial)
  - [`41b4653`](https://github.com/shaahink/DevContext2/commit/41b4653) feat(mcp): curate the agent menu to 14 advertised + 8 unlisted specialists (T1.2)
  - [`05fcce1`](https://github.com/shaahink/DevContext2/commit/05fcce1) test(probe): tax re-measured post-T1.1 + mcp.json can no longer point at another checkout
  - [`76c42dd`](https://github.com/shaahink/DevContext2/commit/76c42dd) fix(mcp): bug #5 - tool + parameter descriptions reach the wire (T1.1)
- **s2 (T1 Deliver)** — 2 commit(s):
  - [`7061953`](https://github.com/shaahink/DevContext2/commit/7061953) docs(t1.3): evidence pack, backlog re-status for #6/#9/#2, handoff for T1.4
  - [`cdb152c`](https://github.com/shaahink/DevContext2/commit/cdb152c) fix(mcp): bugs #6 + #9 - a nodeId traces, and a cut pack says it was cut (T1.3)

## Last gate run

fast-engine:OK · guards:OK

## Last session result

> SESSION-RESULT: **T1.3 delivered and claimed** (`conductor task --done T1.3`), closing the confident-partial-truth family on the agent path. Bug #6 — `EntryPointResolver` never recognised the `Kind:Key` nodeId form the rest of the tool menu speaks, so it fell through to the `Type:Method` split and read the *kind prefix* as a type name; measured pre-fix on TodoApi, 10 of 12 nodeIds answered `found:false` while the same nodes' bare titles traced 2–5 steps, and on Hangfire the same path matched a node titled literally `Type` and returned the `"Entry: Type: Type"` phantom. A nodeId tier now resolves the id first (returning the *inventory* entry when the node is one, so an endpoint keeps its rout…

## Tracker handoff

```
T1.1 + T1.2 + T1.3 DONE (claimed). T1.4 is the last of stage T1 — take it next.

T1.3 landed #6 (nodeId tier in EntryPointResolver + a note on the vacuous
found:true/steps:0) and #9 (ContextPackBuilder.ElidedPrefix/DeclaresElision;
fillNote reads the pack's declaration instead of the fill ratio). #2 RE-MEASURED
and does NOT reproduce: 12/12 TodoApi + 40/40 eShop titles round-trip; backlog
re-statused. Read eval-results/2026-08-13/t1-partial-truth/T1.3-EVIDENCE.md, not
this block, for the shapes.

T1.4 IS MOSTLY WIRING, NOT NEW MEASUREMENT. eval/mcp-qa/partial-truth.js already
asserts every T1.4 bar except "every tool described" (that is wire-truth.js) and
exits non-zero on FAIL. Lift BOTH into eval/gates.ps1, then prove RED on a
pre-fix build: `git stash` src/DevContext.Core/Graph/EntryPointResolver.cs +
ContextPackBuilder.cs, rebuild, run, expect 3 FAILs. Probe takes a repo arg
(TodoApi ~40s, eShop ~3min) — gate on TodoApi.
TRAPS PAID FOR: PowerShell eats a bare `--` (use `conductor bg start --% ... -- cmd`).
A killed MCP child leaves DevContext.Server holding Core.dll -> next build dies
MSB3027; partial-truth.js closes stdin first, wire-truth.js still does NOT.
```
