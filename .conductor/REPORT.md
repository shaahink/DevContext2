# Conductor — DevContext pre-release - engine and agent face run report

_Updated 2026-08-13 17:59 UTC · branch `feat/pre-release-engine` · HEAD `86c15b6`_

**Status:** Idle
**Stage:** T1 — W1 agent-surface trust pack (bug 5 by measurement, curated menu, partial-truth family, wire-truth gate) · attempts used 1
**Checkpoints:** 4/20 done · **Sessions run:** 3 · **Cost:** $34.4391 (agent $34.3573 + gates $0.0817) · **Tokens:** 525,734 in / 235,487 out

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| T1 | W1 agent-surface trust pack (bug 5 by measurement, curated menu, partial-truth family, wire-truth gate) | ██████████ 4/4 | gating… |
| V1 | W2a one-vocabulary pack (25, 17, 7-rider invariant, 18) | ░░░░░░░░░░ 0/3 | todo |
| E1 | W2 edge completeness batch (11, 12 via TextSpan, re-measure 8, 7) + dogfood invariant | ░░░░░░░░░░ 0/4 | todo |
| D1 | W5 detection declared-coverage (reachability instrument, hole closures, filed set, rung-4 jobs) | ░░░░░░░░░░ 0/4 | todo |
| R1 | W6 metric recalibration (22, 23, 24) against the post-E1 graph | ░░░░░░░░░░ 0/1 | todo |
| A1 | W3 re-probe prep (DESIGN amendments, unseen repo) + the 10-dollar adoption gate | ░░░░░░░░░░ 0/2 | todo |
| Z1 | Close-out: README honesty pass, release-gate statuses, backlog reconciliation | ░░░░░░░░░░ 0/2 | todo |

<details> ✅<summary>T1 — W1 agent-surface trust pack (bug 5 by measurement, curated menu, partial-truth family, wire-truth gate) (4/4)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| T1.1 | Bug #5 fixed by measurement: a real MCP `tools/list` call shows a non-empty description for EVERY tool on the wire (mechanism verified against what the ModelContextProtocol SDK actually reads, not assumed); schema tax re-measured and recorded against the P1.2 baseline | ✅ DONE | [`76c42dd`](https://github.com/shaahink/DevContext2/commit/76c42dd) |
| T1.2 | Menu curated: core agent surface shipped (PRODUCT-DIRECTION §7 set + navigation primitives as the candidate), the rest demoted or folded, retired names handled by the did-you-mean path reading the REAL tool list; `tools/list` shows the curated described menu | ✅ DONE | [`76c42dd`](https://github.com/shaahink/DevContext2/commit/76c42dd) |
| T1.3 | Confident-partial-truth family closed on the agent path: #6 trace-by-nodeId routes through the resolver `get_context` uses; #10 invalid enum values rejected with the good error envelope; #9 fillNote names elision and the budgetTokens lever; #2 entrypoint names round-trip into `get_context`/`trace` — evidence is real MCP calls showing each new shape | ✅ DONE | [`cdb152c`](https://github.com/shaahink/DevContext2/commit/cdb152c) |
| T1.4 | Wire-truth gate in the battery, proven RED on the pre-fix build then green: every tool described, invalid mode/direction/format rejected, a `found:true` trace has steps or says why not, every elision named | ✅ DONE | [`c246d77`](https://github.com/shaahink/DevContext2/commit/c246d77) |

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
| 3 | T1 | Deliver | 1 | 08-13 17:19 | 0:28 | Advanced | T1.4 | 4 | fast-engine:OK · guards:OK | $9.0504 | $0.0264 | 149,249/71,678 |

## Money

_What this run has cost, from its own `costs` rows. Same numbers as `conductor money`._

| scope | sessions | tokens | cache reads | cost | checkpoints | tok/ckpt | $/ckpt |
|---|---|---|---|---|---|---|---|
| **run total** | 3 | 47.2M | 98.4% | $34.44 | 4 | 11.8M | $8.61 |
| stage T1 | 3 | 47.2M | 98.4% | $34.44 | 4 | 11.8M | $8.61 |
| 2026-08 | 3 | 47.2M | 98.4% | $34.44 | 4 | 11.8M | $8.61 |

_Where the money goes: agent $34.36 (100%) · gate $0.08 (0%) · blended $0.73/M tokens._

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
08-13 18:19:11  • session #2 T1 → Advanced · done T1.3 · 2 commit(s)  (28m42s)
08-13 18:19:12  • session #3 T1 Deliver started (attempt 1/6)
08-13 18:52:04  ▪ gate fast-engine pass [session]  (2m56s)
08-13 18:52:04  ▪ gate guards pass [session]  (1m26s)
08-13 18:52:13  • session #3 T1 → Advanced · done T1.4 · 4 commit(s)  (33m00s)
08-13 18:59:42  ▪ gate fast-engine pass [phase]  (2m38s)
08-13 18:59:43  ▪ gate guards pass [phase]  (2m01s)
08-13 18:59:43  ▪ gate battery FAIL [phase]  (1m23s)
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 3 · retries 0 (0 %) · overall Ok
✓ no health concerns detected
```

## Repo

_Live git snapshot (branch, working tree, sync vs upstream)._

```
branch: feat/pre-release-engine
working tree: M PRE-RELEASE-ENGINE-TRACKER.md, M eval-results/2026-08-13/mcp-qa.md
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
- **s3 (T1 Deliver)** — 4 commit(s):
  - [`788e0d4`](https://github.com/shaahink/DevContext2/commit/788e0d4) docs(t1.4): handoff - pre-fix worktree removed, recipe kept instead
  - [`4aef713`](https://github.com/shaahink/DevContext2/commit/4aef713) docs(t1.4): handoff for stage V1 + tracker/report refresh
  - [`1ee81eb`](https://github.com/shaahink/DevContext2/commit/1ee81eb) test(t1.4): the gate STEP proven red then green, and the pre-fix run attributed
  - [`c246d77`](https://github.com/shaahink/DevContext2/commit/c246d77) feat(gates): the wire-truth gate - the battery finally speaks MCP (T1.4)

## Last gate run

fast-engine:OK · guards:OK · battery:FAIL-retry

<details><summary>battery — exit 1</summary>

```
wire-truth: C:\Code\DevContext2-engine\src\DevContext.Mcp\bin\Debug\net10.0\devcontext-mcp.exe
  wrote C:\Users\shahi\AppData\Local\Temp\devcontext-gate-wire-truth\server-identity.json
  PASS  the server answering is THIS repo's fresh build - C:\Code\DevContext2-engine\src\DevContext.Server\bin\Debug\net10.0\ (pid 49300, started 2026-08-13T17:59:22.4042278Z)
  wrote C:\Users\shahi\AppData\Local\Temp\devcontext-gate-wire-truth\tools-list.json
  wrote C:\Users\shahi\AppData\Local\Temp\devcontext-gate-wire-truth\wire-truth.json

  tools: 14 | described: 14 | params: 52/52 described
  tools/list payload: 12358 chars (~3090 tokens), 883 chars/tool

  PASS  every tool on the wire has a non-empty description - all described
  PASS  every tool parameter on the wire has a description - all described
  PASS  tools/list is non-empty - 14 tools
  wrote C:\Users\shahi\AppData\Local\Temp\devcontext-gate-wire-truth\specialist-list_sessions.json
  PASS  an unlisted specialist is still callable and answers for real - list_sessions (unlisted) -> {"count":0,"sessions":[]}
  wrote C:\Users\shahi\AppData\Local\Temp\devcontext-gate-wire-truth\unknown-tool-envelope.json
  PASS  an unknown name gets the did-you-mean envelope - {"error":"Unknown tool \u0027no_such_tool_xyz\u0027.","hint":"Use one of availableTools.",
  PASS  the envelope's availableTools equals the advertised menu - 14 vs 14
  PASS  the envelope names the unlisted specialists, each with what it answers - close_session config list_sessions node status tests_for top_flows verify_context
  PASS  no specialist is also on the advertised menu - menu and specialists are disjoint
  wrote C:\Users\shahi\AppData\Local\Temp\devcontext-gate-wire-truth\enum-dials.json
  PASS  every out-of-range enum dial is rejected, not silently re-read - 5 dials reject
  PASS  a VALID enum value still reaches the real code path (the guard is not a blanket reject) - valid values pass the guard
  wrote C:\Users\shahi\AppData\Local\Temp\devcontext-gate-wire-truth\wire-truth.json

wire-truth: GREEN
  PASS  wire truth: every tool + parameter described, menu curated, enum dials reject
  PASS  [#env] the server answering is THIS repo's fresh build - C:\Code\DevContext2-engine\src\DevContext.Server\bin\Debug\net10.0\ (pid 49456, started 2026-08-13T17:59:29.3653519Z)
  handle: 085403002549499380f789019733b48e
  PASS  [#2] every entrypoints title resolves in get_context AND trace - 12 titles round-tripped
  PASS  [#6] trace(nodeId) never renders a phantom whose title IS its kind - 12 nodeIds
  PASS  [#6] a found:true trace with 0 steps says WHY (note) instead of an empty tree - no silent empty trace
  PASS  [#6] trace(nodeId) is not weaker than trace(bare name) for the same node - id and name agree
  PASS  [#9] the probe found a real elided pack to judge (else the check is vacuous) - Extensions elides +64 lines
  PASS  [#9] no pack claims 'everything reachable' while its own text shows an elision - no false completeness claim
  PASS  [#9] every elided pack names the elision AND the budgetTokens lever - all elisions named
  PASS  [#10] an out-of-range enum is rejected, not silently re-read - Invalid mode: 'full'.
partial-truth: GREEN
  PASS  partial truth: entry names round-trip, nodeId traces, every elision named

--- Step 3: Eval expectation tests ---
```
</details>

## Last session result

> SESSION-RESULT:** T1.4 landed and is claimed (`conductor task --done T1.4 --evidence eval-results/2026-08-13/t1-wire-truth-gate/T1.4-EVIDENCE.md`), closing stage T1. `eval/gates.ps1` gained **Step 2c — Wire-truth gate**, the first step in this battery that speaks MCP: it drives a real stdio handshake against the binary step 1 just built and runs both probes — `wire-truth.js` (every tool + every parameter described, the curated 14-tool menu, the did-you-mean envelope naming the 8 unlisted specialists, all five enum dials rejecting an out-of-range value while a valid one still reaches the real code path — the enum checks folded in from the T1.3 spot probe that lived under `eval-results/` where…

## Tracker handoff

```
STAGE T1 IS CLOSED — T1.1–T1.4 all DONE (claimed). Next stage is V1.

T1.4 = eval/gates.ps1 **Step 2c**: a real MCP handshake, both probes, exit 2 on
either. +~90s to full/engine scope, 0 to app. Read
eval-results/2026-08-13/t1-wire-truth-gate/T1.4-EVIDENCE.md for the numbers.

THE ONE THING THAT WILL BITE YOU: an MCP measurement on this machine can be
served by the OTHER conductor run's engine. ServerShim reuses whatever answers
/health on the hardcoded 127.0.0.1:5179 — my post-fix probe was served by
C:\Code\DevContext2-desktop's server and called T1.3's landed fixes broken.
Fixed: DEVCONTEXT_ENDPOINT on both hosts (probes use 5279) + /health now carries
baseDirectory/pid/startedAt + eval/mcp-qa/server-identity.js FAILS any probe not
served by THIS repo's fresh build. Any new probe you write must require() it.
To re-prove a red cheaply: eval/mcp-qa/step2c-harness.ps1 -RepoRoot <root> runs
the gate step's own TEXT against any checkout. Recipe (~50s): worktree add
--detach <pre-fix sha>, `git checkout HEAD -- eval/mcp-qa src/DevContext.Server`
into it, build the slnx, point the harness at it. Mine is removed.
```
