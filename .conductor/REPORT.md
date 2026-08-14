# Conductor — DevContext pre-release - engine and agent face run report

_Updated 2026-08-14 03:47 UTC · branch `feat/pre-release-engine` · HEAD `c5a3888`_

**Status:** Idle
**Stage:** A1 — W3 re-probe prep (DESIGN amendments, unseen repo) + the 10-dollar adoption gate · attempts used 0 · working ▸ A1.2
**Checkpoints:** 17/20 done · **Sessions run:** 25 · **Cost:** $201.5779 (agent $201.3903 + gates $0.1876) · **Tokens:** 3,078,301 in / 1,340,419 out
**Confirmed phases:** T1, V1, E1, D1, R1

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| T1 | W1 agent-surface trust pack (bug 5 by measurement, curated menu, partial-truth family, wire-truth gate) | ██████████ 4/4 | confirmed ✓ |
| V1 | W2a one-vocabulary pack (25, 17, 7-rider invariant, 18) | ██████████ 3/3 | confirmed ✓ |
| E1 | W2 edge completeness batch (11, 12 via TextSpan, re-measure 8, 7) + dogfood invariant | ██████████ 4/4 | confirmed ✓ |
| D1 | W5 detection declared-coverage (reachability instrument, hole closures, filed set, rung-4 jobs) | ██████████ 4/4 | confirmed ✓ |
| R1 | W6 metric recalibration (22, 23, 24) against the post-E1 graph | ██████████ 1/1 | confirmed ✓ |
| A1 | W3 re-probe prep (DESIGN amendments, unseen repo) + the 10-dollar adoption gate | █████░░░░░ 1/2 | **← active** |
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

<details> ✅<summary>D1 — W5 detection declared-coverage (reachability instrument, hole closures, filed set, rung-4 jobs) (4/4)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| D1.1 | Catalog-reachability instrument: for every catalog descriptor a test asserts its signal is reachable and its Kind has a producing path (descriptor→signal→extractor→builder→entry); proven RED on the Orleans and TimedJob finds before any closure | ✅ DONE | [`78e8998`](https://github.com/shaahink/DevContext2/commit/78e8998) |
| D1.2 | Reachable-surface holes closed with consumer-app fixtures + expectations: Orleans packages on the descriptor; BackgroundService/IHostedService base-type detector; TimedJob producer or honest deletion of the kind; Avalonia descriptor + WinForms Exe case | ✅ DONE | [`78e8998`](https://github.com/shaahink/DevContext2/commit/78e8998) |
| D1.3 | Filed set: #14 generic command verbs (strip type args in leaf comparison, carry the type arg as parent); #20/#19 one source of truth for "what is a service"; #2's detection half (addressable entry names single-sourced); Blazor UI-vs-HTTP distinction; per-detection Confidence read or deleted | ✅ DONE | [`796843f`](https://github.com/shaahink/DevContext2/commit/796843f) |
| D1.4 | Rung 4: Hangfire and Quartz consumer job entries (attribute + interface shapes), one consumer-app fixture each | ✅ DONE | [`a221296`](https://github.com/shaahink/DevContext2/commit/a221296) |

</details>

<details> ✅<summary>R1 — W6 metric recalibration (22, 23, 24) against the post-E1 graph (1/1)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| R1.1 | #22/#23/#24 recalibrated against the post-E1 graph or retired; each surviving threshold carries a comment stating the measurement and the calibration commit | ✅ DONE | [`557537c`](https://github.com/shaahink/DevContext2/commit/557537c) |

</details>

<details><summary>A1 — W3 re-probe prep (DESIGN amendments, unseen repo) + the 10-dollar adoption gate (1/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| A1.1 | DESIGN.md amended IN WRITING before any run, per RESULTS §10: unseen-repo protocol (identifier-renamed public repo prepared and committed), question mix (2× class B, E/F at half), non-inferiority bar re-specified at a reachable level, κ-on-20% rule with degenerate-marginal handling, B-instructed arm added | ✅ DONE | - |
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
| 22 | D1 | Deliver | 1 | 08-14 00:38 | 0:37 | Advanced | D1.3 | 4 | gates green (none configured) | $13.4409 |  | 205,917/72,126 |
| 23 | D1 | Deliver | 1 | 08-14 01:16 | 0:28 | Advanced | D1.4 | 3 | gates green (none configured) | $12.0738 |  | 186,290/82,351 |
| 24 | R1 | Deliver | 1 | 08-14 02:07 | 0:31 | Advanced | R1.1 | 3 | gates green (none configured) | $10.5692 |  | 167,565/71,470 |
| 25 | A1 | Deliver | 1 | 08-14 03:08 | 0:38 | Advanced | A1.1 | 5 | gates green (none configured) | $14.4701 |  | 197,178/118,885 |

## Money

_What this run has cost, from its own `costs` rows. Same numbers as `conductor money`._

| scope | sessions | tokens | cache reads | cost | checkpoints | tok/ckpt | $/ckpt |
|---|---|---|---|---|---|---|---|
| **run total** | 24 | 259.9M | 98.4% | $187.13 | 16 | 16.2M | $11.70 |
| stage T1 | 4 | 68M | 98.5% | $48.38 | 4 | 17M | $12.10 |
| stage V1 | 3 | 51.3M | 98.5% | $36.51 | 3 | 17.1M | $12.17 |
| stage E1 | 12 | 74.2M | 98.4% | $53.35 | 4 | 18.6M | $13.34 |
| stage D1 | 4 | 51.9M | 98.3% | $38.33 | 4 | 13M | $9.58 |
| stage R1 | 1 | 14.5M | 98.3% | $10.57 | 1 | 14.5M | $10.57 |
| 2026-08 | 24 | 259.9M | 98.4% | $187.13 | 16 | 16.2M | $11.70 |

_Where the money goes: agent $186.92 (100%) · gate $0.19 (0%) · advisor $0.02 (0%) · blended $0.72/M tokens._

## Timeline

_Transitions with duration, from the event log (`.conductor/events.jsonl`)._

```
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
08-14 01:38:53  • session #21 D1 → Advanced · done D1.1,D1.2 · 6 commit(s)  (37m52s)
08-14 01:38:54  • session #22 D1 Deliver started (attempt 1/6)
08-14 02:16:40  • session #22 D1 → Advanced · done D1.3 · 4 commit(s)  (37m46s)
08-14 02:16:41  • session #23 D1 Deliver started (attempt 1/6)
08-14 02:45:53  • session #23 D1 → Advanced · done D1.4 · 3 commit(s)  (29m11s)
08-14 03:07:14  ▪ gate fast-engine pass [phase]  (3m05s)
08-14 03:07:14  ▪ gate guards pass [phase]  (3m49s)
08-14 03:07:15  ▪ gate battery pass [phase]  (14m19s)
08-14 03:07:15  ✓ checkpoint D1.1 confirmed
08-14 03:07:15  ✓ checkpoint D1.2 confirmed
08-14 03:07:15  ✓ checkpoint D1.3 confirmed
08-14 03:07:15  ✓ checkpoint D1.4 confirmed
08-14 03:07:15  ▸ stage D1 confirmed  (2h12m12s)
08-14 03:07:21  ▸ stage R1 entered — W6 metric recalibration (22, 23, 24) against the post-E1 graph
08-14 03:07:21  • session #24 R1 Deliver started (attempt 1/2)
08-14 03:39:05  • session #24 R1 → Advanced · done R1.1 · 3 commit(s)  (31m43s)
08-14 04:04:17  ▪ gate fast-engine pass [phase]  (4m02s)
08-14 04:04:17  ▪ gate guards pass [phase]  (2m03s)
08-14 04:04:18  ▪ gate battery pass [phase]  (19m03s)
08-14 04:04:18  ✓ checkpoint R1.1 confirmed
08-14 04:04:18  ▸ stage R1 confirmed  (56m56s)
08-14 04:04:43  ▸ stage A1 entered — W3 re-probe prep (DESIGN amendments, unseen repo) + the 10-dollar adoption gate
08-14 04:04:44  ■ needs human — agent asked for a human in the tracker handoff (HUMAN: line) — resolve, then run `conductor resume`
08-14 04:08:05  • session #25 A1 Deliver started (attempt 1/4)
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 25 · retries 8 (32 %) · overall Alert
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
- **s22 (D1 Deliver)** — 4 commit(s):
  - [`b480065`](https://github.com/shaahink/DevContext2/commit/b480065) docs(d1): D1.3 evidence, the three backlog closures, and the handoff (D1.3)
  - [`1122d02`](https://github.com/shaahink/DevContext2/commit/1122d02) fix(detection): one verdict decides what is a service, and the counts stop disagreeing (D1.3)
  - [`0a614cc`](https://github.com/shaahink/DevContext2/commit/0a614cc) fix(detection): a Blazor @page is a UI entry in the catalog too, not an HTTP endpoint (D1.3)
  - [`796843f`](https://github.com/shaahink/DevContext2/commit/796843f) fix(detection): read the generic command attribute, and let its type argument build the second level of the tree (D1.3)
- **s23 (D1 Deliver)** — 3 commit(s):
  - [`ded5493`](https://github.com/shaahink/DevContext2/commit/ded5493) docs(d1): D1.4 evidence and the handoff for the next session (D1.4)
  - [`cbae476`](https://github.com/shaahink/DevContext2/commit/cbae476) refactor(detection): Confidence moves to the one detection that is read, 27 writes go (D1.3 leftover)
  - [`a221296`](https://github.com/shaahink/DevContext2/commit/a221296) feat(detection): a Hangfire or Quartz consumer app finally has entry points (D1.4)
- **s24 (R1 Deliver)** — 3 commit(s):
  - [`b72cb7d`](https://github.com/shaahink/DevContext2/commit/b72cb7d) chore(tracker): R1.1 handoff for the next session (R1.1)
  - [`372f77c`](https://github.com/shaahink/DevContext2/commit/372f77c) docs(metrics): the surviving threshold names its calibration commit (R1.1)
  - [`557537c`](https://github.com/shaahink/DevContext2/commit/557537c) refactor(metrics): two metrics retire on measurement, one has its premise refuted (R1.1)
- **s25 (A1 Deliver)** — 5 commit(s):
  - [`c5a3888`](https://github.com/shaahink/DevContext2/commit/c5a3888) fix(probe): store the unseen tree verbatim so its pin verifies from a checkout (A1.1)
  - [`711a23c`](https://github.com/shaahink/DevContext2/commit/711a23c) feat(probe): the unseen repo, built from a pinned source and verified by building it (A1.1)
  - [`a3f68ee`](https://github.com/shaahink/DevContext2/commit/a3f68ee) pre-register(probe): five amendments to DESIGN, written before the run they govern (A1.1)
  - [`67a6ef1`](https://github.com/shaahink/DevContext2/commit/67a6ef1) chore(conductor): s24 R1 Advanced — Idle
  - [`1d30e02`](https://github.com/shaahink/DevContext2/commit/1d30e02) chore(conductor): s24 R1 Advanced — Idle

## Last session result

> SESSION-RESULT: **A1.1 landed and is claimed DONE** (`conductor task --done A1.1`, evidence `eval-results/2026-08-14/a1-preregistration/A1.1-EVIDENCE.md`), across three pushed commits — `a3f68ee` the five DESIGN amendments, `711a23c` the unseen repo, `c5a3888` a pin-storage fix. `eval/agent-probe/DESIGN.md` now carries a §0 amendment log and five dated, in-place amendments written *before* any run: the unseen repo named/built/pinned, the "E and F at half weight" mix resolved to a per-repo assignment table, the non-inferiority endpoint replaced, κ made applicable with a degenerate-marginal rule, and arm **BI** added (verified: 21 argv elements, exactly one differs, and it is the system prompt…

## Tracker handoff

```
A1.1 CLAIMED (b0de331 + this commit). Evidence eval-results/2026-08-14/a1-preregistration/.
DESIGN.md now carries §0 an amendment log and five dated amendments: unseen repo, question mix
pinned per repo, the non-inferiority endpoint replaced, κ made applicable, arm BI added. Two
RESULTS §10.4 figures did NOT survive re-derivation - its reachability table is the acc=1.00
column called "the best each n can produce" (it is the worst; at acc=0.50 the interval is
zero-width at every n), and the pairs were pseudo-replicated 5× which makes non-inferiority
EASIER to declare. New endpoint: question-level, bootstrap over questions, ONE pooled test,
margin −0.10 (`analyse.mjs --ni-power` prints both surfaces). Unseen repo BUILT and pinned:
eval/agent-probe/unseen/Driewie/, 132 files, treeSha 9857e03c, `rename-repo.mjs --verify` PASSES,
build 0 errors/4 warnings matching the unmodified control exactly. Building it found 3 defects
reading could not - read the ledger note before touching rename-repo.mjs.
NEXT: A1.2, the $10 adoption gate, now unblocked - pre-registration is committed and DESIGN §3.1
names the branch to report either way. Run arm B alone on eShop, 18 runs, UNCHANGED prompt, via
`conductor bg start --purpose probe`. A share <0.2 is a REAL product finding, not a failure.
NOT DONE, and NOT A1.1's: questions/Driewie.json + the 2nd class-B question per repo (full-run
W3.8 inputs, must be hand-authored before any full run).
```
