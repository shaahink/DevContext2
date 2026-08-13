# Conductor — DevContext pre-release - engine and agent face run report

_Updated 2026-08-13 21:45 UTC · branch `feat/pre-release-engine` · HEAD `795ad1b`_

**Status:** Idle — stage E1 used all 8 attempts without completing — inspect and `conductor resume` (or `conductor skip`) [1h 07m ago, 20:37:17Z]
**Stage:** E1 — W2 edge completeness batch (11, 12 via TextSpan, re-measure 8, 7) + dogfood invariant · attempts used 0 · working ▸ E1.2
**Checkpoints:** 8/20 done · **Sessions run:** 16 · **Cost:** $101.8455 (agent $101.6579 + gates $0.1876) · **Tokens:** 1,512,421 in / 634,721 out
**Confirmed phases:** T1, V1

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| T1 | W1 agent-surface trust pack (bug 5 by measurement, curated menu, partial-truth family, wire-truth gate) | ██████████ 4/4 | confirmed ✓ |
| V1 | W2a one-vocabulary pack (25, 17, 7-rider invariant, 18) | ██████████ 3/3 | confirmed ✓ |
| E1 | W2 edge completeness batch (11, 12 via TextSpan, re-measure 8, 7) + dogfood invariant | ██░░░░░░░░ 1/4 | **← active** |
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

<details> ✅<summary>V1 — W2a one-vocabulary pack (25, 17, 7-rider invariant, 18) (3/3)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| V1.1 | ONE verified-edge definition (#25): GraphStats/SeamStat and GraphOrphansSource agree, the definition stated in one place, every surface reads it | ✅ DONE | [`abfa564`](https://github.com/shaahink/DevContext2/commit/abfa564) |
| V1.2 | ONE member-title helper (#17) next to SymbolCanon used by every producer; the owner-qualified vs bare split gone on the six poles it was measured on | ✅ DONE | [`fd47300`](https://github.com/shaahink/DevContext2/commit/fd47300) |
| V1.3 | Two standing invariants land red-first: no node carries kind Type with a member id (#7 rider); lambda/expression text never becomes a node title on any path (#18) | ✅ DONE | [`3eb2f34`](https://github.com/shaahink/DevContext2/commit/3eb2f34) |

</details>

<details><summary>E1 — W2 edge completeness batch (11, 12 via TextSpan, re-measure 8, 7) + dogfood invariant (1/4)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| E1.1 | #11 static type-name-receiver calls produce edges; the dogfood invariant (DevContext's own helper layer has in-edges in DevContext's own graph) added to the battery and proven red first | ✅ DONE | - |
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
| 4 | T1 | Fix | 2 | 08-13 17:59 | 0:31 | Progress |  | 5 | fast-engine:OK · guards:OK | $13.9168 | $0.0271 | 164,370/78,266 |
| 5 | V1 | Deliver | 1 | 08-13 18:46 | 0:28 | Advanced | V1.1 | 4 | fast-engine:OK-retry · guards:OK | $12.8408 | $0.0606 | 186,202/82,566 |
| 6 | V1 | Deliver | 1 | 08-13 19:25 | 0:27 | Advanced | V1.2 | 2 | fast-engine:OK · guards:OK | $8.7991 | $0.0181 | 153,341/57,563 |
| 7 | V1 | Deliver | 1 | 08-13 19:56 | 0:26 | Advanced | V1.3 | 3 | gates green (none configured) | $14.7864 |  | 223,675/82,121 |
| 8 | E1 | Deliver | 1 | 08-13 20:34 | 0:00 | AgentError |  | 0 | gates green (none configured) | $0.3330 |  | 25,299/891 |
| 9 | E1 | Fix | 2 | 08-13 20:35 | 0:00 | AgentError |  | 0 | gates green (none configured) | $0.0000 |  |  |
| 10 | E1 | Deliver | 3 | 08-13 20:35 | 0:00 | AgentError |  | 0 | gates green (none configured) | $0.0000 |  |  |
| 11 | E1 | Deliver | 4 | 08-13 20:35 | 0:00 | AgentError |  | 0 | gates green (none configured) | $0.0000 |  |  |
| 12 | E1 | Deliver | 5 | 08-13 20:36 | 0:00 | AgentError |  | 0 | gates green (none configured) | $0.0000 |  |  |
| 13 | E1 | Deliver | 6 | 08-13 20:36 | 0:00 | AgentError |  | 0 | gates green (none configured) | $0.0000 |  |  |
| 14 | E1 | Deliver | 7 | 08-13 20:36 | 0:00 | AgentError |  | 0 | gates green (none configured) | $0.0000 |  |  |
| 15 | E1 | Deliver | 8 | 08-13 20:36 | 0:00 | AgentError |  | 0 | gates green (none configured) | $0.0000 |  |  |
| 16 | E1 | Deliver | 1 | 08-13 20:44 | 1:00 | Advanced | E1.1 | 16 | gates green (none configured) | $16.6244 |  | 233,800/97,827 |

## Money

_What this run has cost, from its own `costs` rows. Same numbers as `conductor money`._

| scope | sessions | tokens | cache reads | cost | checkpoints | tok/ckpt | $/ckpt |
|---|---|---|---|---|---|---|---|
| **run total** | 15 | 119.4M | 98.5% | $85.24 | 7 | 17.1M | $12.18 |
| stage T1 | 4 | 68M | 98.5% | $48.38 | 4 | 17M | $12.10 |
| stage V1 | 3 | 51.3M | 98.5% | $36.51 | 3 | 17.1M | $12.17 |
| stage E1 | 8 | 0.13M | 80.1% | $0.36 | - | - | - |
| 2026-08 | 15 | 119.4M | 98.5% | $85.24 | 7 | 17.1M | $12.18 |

_Where the money goes: agent $85.03 (100%) · gate $0.19 (0%) · advisor $0.02 (0%) · blended $0.71/M tokens._

## Timeline

_Transitions with duration, from the event log (`.conductor/events.jsonl`)._

```
08-13 20:25:46  • session #6 V1 Deliver started (attempt 1/4)
08-13 20:56:18  ▪ gate fast-engine pass [session]  (1m52s)
08-13 20:56:18  ▪ gate guards pass [session]  (1m08s)
08-13 20:56:24  • session #6 V1 → Advanced · done V1.2 · 2 commit(s)  (30m38s)
08-13 20:56:25  ◆ plan reloaded — v1 · 7 stages · 3 gates
08-13 20:56:29  • session #7 V1 Deliver started (attempt 1/4)
08-13 21:22:43  • session #7 V1 → Advanced · done V1.3 · 3 commit(s)  (26m13s)
08-13 21:34:00  ▪ gate fast-engine pass [phase]  (2m18s)
08-13 21:34:00  ▪ gate guards pass [phase]  (1m21s)
08-13 21:34:00  ▪ gate battery pass [phase]  (7m35s)
08-13 21:34:01  ✓ checkpoint V1.1 confirmed
08-13 21:34:01  ✓ checkpoint V1.2 confirmed
08-13 21:34:01  ✓ checkpoint V1.3 confirmed
08-13 21:34:01  ▸ stage V1 confirmed  (1h47m22s)
08-13 21:34:06  ▸ stage E1 entered — W2 edge completeness batch (11, 12 via TextSpan, re-measure 8, 7) + dogfood invariant
08-13 21:34:06  • session #8 E1 Deliver started (attempt 1/8)
08-13 21:35:03  • session #8 E1 → AgentError  (57.0s)
08-13 21:35:04  • session #9 E1 Fix started (attempt 2/8)
08-13 21:35:18  ■ needs human — advisor: human intervention required
08-13 21:35:22  • session #9 E1 → AgentError  (18.5s)
08-13 21:35:23  • session #10 E1 Deliver started (attempt 3/8)
08-13 21:35:37  ■ needs human — advisor: human intervention required
08-13 21:35:42  • session #10 E1 → AgentError  (19.2s)
08-13 21:35:42  • session #11 E1 Deliver started (attempt 4/8)
08-13 21:35:56  ■ needs human — advisor: human intervention required
08-13 21:36:01  • session #11 E1 → AgentError  (19.1s)
08-13 21:36:01  • session #12 E1 Deliver started (attempt 5/8)
08-13 21:36:16  ■ needs human — advisor: human intervention required
08-13 21:36:20  • session #12 E1 → AgentError  (18.8s)
08-13 21:36:20  • session #13 E1 Deliver started (attempt 6/8)
08-13 21:36:35  ■ needs human — advisor: human intervention required
08-13 21:36:40  • session #13 E1 → AgentError  (19.4s)
08-13 21:36:40  • session #14 E1 Deliver started (attempt 7/8)
08-13 21:36:52  ■ needs human — advisor: human intervention required
08-13 21:36:55  • session #14 E1 → AgentError  (15.4s)
08-13 21:36:55  • session #15 E1 Deliver started (attempt 8/8)
08-13 21:37:08  ■ needs human — advisor: human intervention required
08-13 21:37:12  • session #15 E1 → AgentError  (16.1s)
08-13 21:37:17  ■ needs human — stage E1 used all 8 attempts without completing — inspect and `conductor resume` (or `conductor skip`)
08-13 21:44:09  • session #16 E1 Deliver started (attempt 1/8)
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 16 · retries 8 (50 %) · overall Alert
⛔ [same-failure-loop] stage E1: 8 consecutive sessions made no progress
⚠ [context-saturation] session #4: 20,624,874 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #7: 20,985,542 context tokens (≥ 20,000,000)
```

## Repo

_Live git snapshot (branch, working tree, sync vs upstream)._

```
branch: feat/pre-release-engine
working tree: clean
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
- **s4 (T1 Fix)** — 5 commit(s):
  - [`8e17904`](https://github.com/shaahink/DevContext2/commit/8e17904) evidence(t1): second full battery - steps 0-4b green again after the node_modules install
  - [`44184cd`](https://github.com/shaahink/DevContext2/commit/44184cd) docs(t1): handoff - two faults behind the phase-gate RED, the second hidden by the first
  - [`5360be2`](https://github.com/shaahink/DevContext2/commit/5360be2) fix(gates): step 5 names a missing node_modules instead of forty lines of resolver noise
  - [`62622d5`](https://github.com/shaahink/DevContext2/commit/62622d5) docs(t1): handoff + evidence for the phase-gate RED (battery, not T1's bars)
  - [`fa7d4b6`](https://github.com/shaahink/DevContext2/commit/fa7d4b6) fix(gates): the battery's Step 3 stamp no longer depends on which shell launched it
- **s5 (V1 Deliver)** — 4 commit(s):
  - [`e5d825b`](https://github.com/shaahink/DevContext2/commit/e5d825b) docs(v1): handoff for V1.2 + session-4 evidence leftovers
  - [`131265c`](https://github.com/shaahink/DevContext2/commit/131265c) fix(graph): one definition of a verified edge, and every surface reads it (V1.1, #25)
  - [`651e524`](https://github.com/shaahink/DevContext2/commit/651e524) chore(conductor): s4 T1 Progress — Idle
  - [`abfa564`](https://github.com/shaahink/DevContext2/commit/abfa564) chore(conductor): s4 T1 Progress — Idle
- **s6 (V1 Deliver)** — 2 commit(s):
  - [`e1f40e3`](https://github.com/shaahink/DevContext2/commit/e1f40e3) docs(v1): V1.2 evidence gates + handoff for V1.3
  - [`fd47300`](https://github.com/shaahink/DevContext2/commit/fd47300) fix(graph): one Member-title vocabulary, derived from the key (V1.2, #17)
- **s7 (V1 Deliver)** — 3 commit(s):
  - [`44e5319`](https://github.com/shaahink/DevContext2/commit/44e5319) docs(v1): V1.3 gate results — Core 766/0, Server 108/0; matrix + guards left to the battery
  - [`e648644`](https://github.com/shaahink/DevContext2/commit/e648644) docs(v1): V1.3 evidence + handoff for E1
  - [`3eb2f34`](https://github.com/shaahink/DevContext2/commit/3eb2f34) fix(graph): two standing invariants, enforced where a node is made (V1.3, #7 rider + #18)
- **s16 (E1 Deliver)** — 16 commit(s):
  - [`795ad1b`](https://github.com/shaahink/DevContext2/commit/795ad1b) Merge remote-tracking branch 'origin/feat/pre-release-engine' into feat/pre-release-engine
  - [`82f0e0f`](https://github.com/shaahink/DevContext2/commit/82f0e0f) fix(graph): static type-name-receiver calls produce edges, in BOTH producers (E1.1, #11)
  - [`3b740d1`](https://github.com/shaahink/DevContext2/commit/3b740d1) chore(conductor): s7 V1 Advanced — Idle
  - [`af29f58`](https://github.com/shaahink/DevContext2/commit/af29f58) chore(conductor): s7 V1 Advanced — Idle
  - [`44e5319`](https://github.com/shaahink/DevContext2/commit/44e5319) docs(v1): V1.3 gate results — Core 766/0, Server 108/0; matrix + guards left to the battery
  - [`e648644`](https://github.com/shaahink/DevContext2/commit/e648644) docs(v1): V1.3 evidence + handoff for E1
  - [`3eb2f34`](https://github.com/shaahink/DevContext2/commit/3eb2f34) fix(graph): two standing invariants, enforced where a node is made (V1.3, #7 rider + #18)
  - [`0fd1cbe`](https://github.com/shaahink/DevContext2/commit/0fd1cbe) chore(conductor): s6 V1 Advanced — Idle
  - [`8502785`](https://github.com/shaahink/DevContext2/commit/8502785) chore(conductor): s6 V1 Advanced — Idle
  - [`e1f40e3`](https://github.com/shaahink/DevContext2/commit/e1f40e3) docs(v1): V1.2 evidence gates + handoff for V1.3
  - [`fd47300`](https://github.com/shaahink/DevContext2/commit/fd47300) fix(graph): one Member-title vocabulary, derived from the key (V1.2, #17)
  - [`f5361ac`](https://github.com/shaahink/DevContext2/commit/f5361ac) chore(conductor): s5 V1 Advanced — Idle

## Last session result

> SESSION-RESULT: **E1.1 is delivered, claimed (`conductor task --done E1.1`), committed (`82f0e0f`, merged as `795ad1b`) and pushed.** Bug #11 turned out to need fixing in **two** producers, not one: `CallGraphBinder` makes member→member Calls edges only for files inside the entry-seeded closure, while `PlainCallDetector` makes member→TYPE Calls edges for every in-scope body — fixing only the binder left all three helper TYPE nodes still at zero in-edges, which I caught because the dogfood invariant stayed red while the unit fixtures went green. Both now consume one shared rule, `SymbolTable.ResolveStaticReceiverType` (unambiguous `ResolveName` + `IsKnownFqn` as the Law-R1 gate + `TypeDeclare…

## Tracker handoff

```
E1.1 IS DONE (evidence eval-results/2026-08-13/e1-edges/E1.1-EVIDENCE.md). #11 needed fixing in TWO
producers, not one: CallGraphBinder makes member->member Calls edges ONLY inside the entry-seeded
closure; PlainCallDetector makes member->TYPE Calls edges for every in-scope body. One shared rule now
serves both -- SymbolTable.ResolveStaticReceiverType (unambiguous ResolveName + IsKnownFqn +
TypeDeclaresMember). Dogfood invariant (Truth category, 28s) went 0/0/0 -> 3/3/7 in-edges. 7-pole
before/after matrix in the same folder; TodoApi moved by 0 (negative control), DevContext 1380->2404.
RED, PRE-EXISTING, NOT MINE: McpQaGateTests.McpQaHarness_Passes_Against_Dogfood (backlog #1 / conductor
bug #1, shared port 5179 with the live desktop run) -- it also failed before my edit landed.
NEXT: E1.2 (#12 TextSpan-on-BodyOp). Two things it inherits: (a) a NEW conductor bug -- a merged Calls
edge keeps the FIRST resolution, not the best, so 14 of DevContext's 1380 pairs now read approx though
one call site is semantically bound; fix it with E1.2's matrix, not surgically. (b) Bumping the BodyOp
shape means bumping BodyFactsVersion (facts-v1) or stale cached facts are reused.
```
