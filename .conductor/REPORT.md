# Conductor — DevContext pre-release - engine and agent face run report

_Updated 2026-08-14 00:38 UTC · branch `feat/pre-release-engine` · HEAD `00fe9ca`_

**Status:** Idle — stage E1 used all 8 attempts without completing — inspect and `conductor resume` (or `conductor skip`) [4h 01m ago, 20:37:17Z]
**Stage:** D1 — W5 detection declared-coverage (reachability instrument, hole closures, filed set, rung-4 jobs) · attempts used 0 · working ▸ D1.3
**Checkpoints:** 13/20 done · **Sessions run:** 21 · **Cost:** $151.0239 (agent $150.8363 + gates $0.1876) · **Tokens:** 2,321,351 in / 995,587 out
**Confirmed phases:** T1, V1, E1

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| T1 | W1 agent-surface trust pack (bug 5 by measurement, curated menu, partial-truth family, wire-truth gate) | ██████████ 4/4 | confirmed ✓ |
| V1 | W2a one-vocabulary pack (25, 17, 7-rider invariant, 18) | ██████████ 3/3 | confirmed ✓ |
| E1 | W2 edge completeness batch (11, 12 via TextSpan, re-measure 8, 7) + dogfood invariant | ██████████ 4/4 | confirmed ✓ |
| D1 | W5 detection declared-coverage (reachability instrument, hole closures, filed set, rung-4 jobs) | █████░░░░░ 2/4 | **← active** |
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

<details> ✅<summary>E1 — W2 edge completeness batch (11, 12 via TextSpan, re-measure 8, 7) + dogfood invariant (4/4)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| E1.1 | #11 static type-name-receiver calls produce edges; the dogfood invariant (DevContext's own helper layer has in-edges in DevContext's own graph) added to the battery and proven red first | ✅ DONE | [`abfa564`](https://github.com/shaahink/DevContext2/commit/abfa564) |
| E1.2 | #12 fixed via the TextSpan-on-op shape (BodyOp records the invocation's own span; relocate-by-line class killed including the TryBindLocalDeclType / TryBindGenericArg / args-bind sister sites); full matrix run against cells declared BEFORE coding; engine-own approx share recorded before/after (baseline 1103/1383) | ✅ DONE | [`8f0ce0a`](https://github.com/shaahink/DevContext2/commit/8f0ce0a) |
| E1.3 | #8 re-measured (its stated mechanism is refuted by #11) and fixed or re-filed with the true mechanism; #7 explicit-interface/BCL-collision Type-node fixed | ✅ DONE | [`f686e25`](https://github.com/shaahink/DevContext2/commit/f686e25) |
| E1.4 | Batch acceptance: the probe's class-C impact question resolves the true impact set on eShop (hand-verified against the question key), matrix shows declared cells flipped and no others | ✅ DONE | [`6d0dbac`](https://github.com/shaahink/DevContext2/commit/6d0dbac) |

</details>

<details><summary>D1 — W5 detection declared-coverage (reachability instrument, hole closures, filed set, rung-4 jobs) (2/4)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| D1.1 | Catalog-reachability instrument: for every catalog descriptor a test asserts its signal is reachable and its Kind has a producing path (descriptor→signal→extractor→builder→entry); proven RED on the Orleans and TimedJob finds before any closure | ✅ DONE | - |
| D1.2 | Reachable-surface holes closed with consumer-app fixtures + expectations: Orleans packages on the descriptor; BackgroundService/IHostedService base-type detector; TimedJob producer or honest deletion of the kind; Avalonia descriptor + WinForms Exe case | ✅ DONE | - |
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
| 17 | E1 | Deliver | 1 | 08-13 21:45 | 0:26 | Advanced | E1.2 | 5 | gates green (none configured) | $11.4408 |  | 180,795/74,866 |
| 18 | E1 | Deliver | 1 | 08-13 22:11 | 0:49 | Advanced | E1.3 | 3 | gates green (none configured) | $11.5717 |  | 165,321/83,648 |
| 19 | E1 | Deliver | 1 | 08-13 23:01 | 0:33 | Advanced | E1.4 | 2 | gates green (none configured) | $13.3545 |  | 204,970/89,424 |
| 20 | D1 | Deliver | 1 | 08-13 23:55 | 0:02 | KilledByUser |  | 0 |  |  |  | 36,301/40 |
| 21 | D1 | Deliver | 1 | 08-14 00:01 | 0:37 | Advanced | D1.1 D1.2 | 6 | gates green (none configured) | $12.8113 |  | 221,543/112,888 |

## Money

_What this run has cost, from its own `costs` rows. Same numbers as `conductor money`._

| scope | sessions | tokens | cache reads | cost | checkpoints | tok/ckpt | $/ckpt |
|---|---|---|---|---|---|---|---|
| **run total** | 20 | 193.5M | 98.5% | $138.24 | 11 | 17.6M | $12.57 |
| stage T1 | 4 | 68M | 98.5% | $48.38 | 4 | 17M | $12.10 |
| stage V1 | 3 | 51.3M | 98.5% | $36.51 | 3 | 17.1M | $12.17 |
| stage E1 | 12 | 74.2M | 98.4% | $53.35 | 4 | 18.6M | $13.34 |
| stage D1 | 1 | 0M | - | $0.00 | - | - | - |
| 2026-08 | 20 | 193.5M | 98.5% | $138.24 | 11 | 17.6M | $12.57 |

_Where the money goes: agent $138.02 (100%) · gate $0.19 (0%) · advisor $0.02 (0%) · blended $0.71/M tokens._

## Timeline

_Transitions with duration, from the event log (`.conductor/events.jsonl`)._

```
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
08-13 22:45:02  • session #16 E1 → Advanced · done E1.1 · 16 commit(s)  (1h00m53s)
08-13 22:45:06  • session #17 E1 Deliver started (attempt 1/8)
08-13 23:11:18  • session #17 E1 → Advanced · done E1.2 · 5 commit(s)  (26m12s)
08-13 23:11:18  • session #18 E1 Deliver started (attempt 1/8)
08-14 00:01:23  • session #18 E1 → Advanced · done E1.3 · 3 commit(s)  (50m05s)
08-14 00:01:24  • session #19 E1 Deliver started (attempt 1/8)
08-14 00:35:47  • session #19 E1 → Advanced · done E1.4 · 2 commit(s)  (34m22s)
08-14 00:54:48  ▪ gate fast-engine pass [phase]  (4m18s)
08-14 00:54:48  ▪ gate guards pass [phase]  (1m35s)
08-14 00:54:48  ▪ gate battery pass [phase]  (12m59s)
08-14 00:54:48  ✓ checkpoint E1.1 confirmed
08-14 00:54:48  ✓ checkpoint E1.2 confirmed
08-14 00:54:48  ✓ checkpoint E1.3 confirmed
08-14 00:54:48  ✓ checkpoint E1.4 confirmed
08-14 00:54:48  ▸ stage E1 confirmed  (3h20m41s)
08-14 00:55:02  ▸ stage D1 entered — W5 detection declared-coverage (reachability instrument, hole closures, filed set, rung-4 jobs)
08-14 00:55:03  • session #20 D1 Deliver started (attempt 1/6)
08-14 00:57:54  • session #20 D1 → KilledByUser  (2m51s)
08-14 01:00:57  ◆ run resumed · DevContext pre-release - engine and agent face
08-14 01:01:01  • session #21 D1 Deliver started (attempt 1/6)
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 21 · retries 8 (38 %) · overall Alert
⛔ [same-failure-loop] stage E1: 8 consecutive sessions made no progress
⚠ [context-saturation] session #16: 23,675,141 context tokens (≥ 20,000,000)
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
- **s17 (E1 Deliver)** — 5 commit(s):
  - [`0022029`](https://github.com/shaahink/DevContext2/commit/0022029) chore: drop the commit-message scratch file that git add -A swept in
  - [`66a0846`](https://github.com/shaahink/DevContext2/commit/66a0846) chore: gate artifacts left dirty by the pre-session battery
  - [`135d469`](https://github.com/shaahink/DevContext2/commit/135d469) docs(e1): E1.2 evidence + handoff (#12 closed via TextSpan-on-op)
  - [`afee44b`](https://github.com/shaahink/DevContext2/commit/afee44b) fix(graph): an out-of-solution semantic bind is not a call target, in BOTH producers (E1.2 rider)
  - [`8f0ce0a`](https://github.com/shaahink/DevContext2/commit/8f0ce0a) fix(graph): ops carry their own TextSpan so the semantic upgrade relocates exactly (E1.2, #12)
- **s18 (E1 Deliver)** — 3 commit(s):
  - [`52417c7`](https://github.com/shaahink/DevContext2/commit/52417c7) chore(e1): E1.3 handoff, plus the mcp-qa artifact a concurrent gate run left dirty
  - [`ed59465`](https://github.com/shaahink/DevContext2/commit/ed59465) docs(e1): E1.3 evidence - #7 closed at both ends on Hangfire, #8 re-filed with its true mechanism
  - [`f686e25`](https://github.com/shaahink/DevContext2/commit/f686e25) fix(graph): the invariant refusal is COUNTED, and #8 gets its regression fixture (E1.3)
- **s19 (E1 Deliver)** — 2 commit(s):
  - [`1fbadde`](https://github.com/shaahink/DevContext2/commit/1fbadde) fix(graph): a REQUEST MARKER is not a handler, and its type arg is the RESPONSE (E1.4)
  - [`6d0dbac`](https://github.com/shaahink/DevContext2/commit/6d0dbac) eval(e1): the class-C impact question, asked of the engine over a real MCP session (E1.4)
- **s21 (D1 Deliver)** — 6 commit(s):
  - [`00fe9ca`](https://github.com/shaahink/DevContext2/commit/00fe9ca) chore(eval): MCP QA run artifact from the truth gate (D1.2)
  - [`6e026d2`](https://github.com/shaahink/DevContext2/commit/6e026d2) docs(d1): D1.2 evidence, the TimedJob measurement, and the handoff (D1.2)
  - [`7aa80ee`](https://github.com/shaahink/DevContext2/commit/7aa80ee) fix(detection): a BackgroundService is a hosted service even when nothing names it (D1.2)
  - [`fa1d57f`](https://github.com/shaahink/DevContext2/commit/fa1d57f) fix(detection): a WinForms Exe and a cross-platform Avalonia head are desktop apps (D1.2)
  - [`a6360e4`](https://github.com/shaahink/DevContext2/commit/a6360e4) fix(detection): give the Orleans descriptor its packages - a consumer app can now reach a grain (D1.2)
  - [`78e8998`](https://github.com/shaahink/DevContext2/commit/78e8998) feat(detection): the catalog-reachability instrument, red-first on Orleans and TimedJob (D1.1)

## Last session result

> SESSION-RESULT:** Two checkpoints landed with proof. **D1.1** built the catalog-reachability instrument (`tests/DevContext.Core.Tests/CatalogReachabilityTests.cs`) — it generates a 42-project consumer-app repo from the catalog's *own* declarations (packages/SDK come from `EntrySurfaceCatalog`, projects are neutrally named so `SelfNamePatterns` cannot mask a hole), analyses it once, and asserts per descriptor that the signal fires and the declared Kind is produced; the whole sweep costs 6 seconds and rides the truth gate. It was RED on its first run on the two finds the checkpoint named (Orleans, TimedJob) plus three new ones — `signal:testing`, the blazor descriptor declaring `HttpEndpoint` …

## Tracker handoff

```
D1.1 + D1.2 both CLOSED (D1.2 with an AMEND: 3 of its 4 items). The instrument is
tests/DevContext.Core.Tests/CatalogReachabilityTests.cs and it is TWO harnesses: (a) a 42-project
SWEEP from the catalog's OWN declarations, neutrally named so SelfNamePatterns cannot mask a hole,
6 seconds; (b) NAMED SHAPE CASES, each in its OWN one-project repo -- isolation is load-bearing,
because signals are repo-wide and a shape in the big sweep gets its gate satisfied by a sibling.
Adding a case = one table row. eval/expectations/catalog-reachability-allow.txt is a RATCHET both
ways: closing a hole FAILS the sweep until you delete its line (that is how the Orleans closure
proved itself). 5 lines left, and they ARE the work list: signal:testing (#14), kind:ScheduledJob@-
(-> D1.4, bugs #17), kind:PublicApi@-, kind:HttpEndpoint@blazor (#15, -> D1.3),
kind:FunctionEntry@aws-lambda (#16). NEXT = D1.3 (filed set; #14 generic command verbs is also E1.4's
blocker, so E1.4 re-opens after it). D1.4 should write Hangfire/Quartz as SHAPE CASES and set
BackgroundWorkerKind.TimedJob there -- that is the honest ScheduledJob producer.
WATCH: the new <UseWPF>/<UseWindowsForms> probe is the only change that can move a real repo's
signals; eval-repos has only eShop/TodoApi/VerticalSlice locally, so the battery's eval step is the
first place a pole-level move would show.
```
