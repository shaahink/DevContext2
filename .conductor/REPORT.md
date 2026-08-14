# Conductor — DevContext pre-release - engine and agent face run report

_Updated 2026-08-14 05:02 UTC · branch `feat/pre-release-engine` · HEAD `4ad7e9b`_

**Status:** Idle
**Stage:** A1 — W3 re-probe prep (DESIGN amendments, unseen repo) + the 10-dollar adoption gate · attempts used 1
**Checkpoints:** 18/20 done · **Sessions run:** 28 · **Cost:** $209.7993 (agent $209.6117 + gates $0.1876) · **Tokens:** 3,308,507 in / 1,407,656 out
**Confirmed phases:** T1, V1, E1, D1, R1

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| T1 | W1 agent-surface trust pack (bug 5 by measurement, curated menu, partial-truth family, wire-truth gate) | ██████████ 4/4 | confirmed ✓ |
| V1 | W2a one-vocabulary pack (25, 17, 7-rider invariant, 18) | ██████████ 3/3 | confirmed ✓ |
| E1 | W2 edge completeness batch (11, 12 via TextSpan, re-measure 8, 7) + dogfood invariant | ██████████ 4/4 | confirmed ✓ |
| D1 | W5 detection declared-coverage (reachability instrument, hole closures, filed set, rung-4 jobs) | ██████████ 4/4 | confirmed ✓ |
| R1 | W6 metric recalibration (22, 23, 24) against the post-E1 graph | ██████████ 1/1 | confirmed ✓ |
| A1 | W3 re-probe prep (DESIGN amendments, unseen repo) + the 10-dollar adoption gate | ██████████ 2/2 | gating… |
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

<details> ✅<summary>A1 — W3 re-probe prep (DESIGN amendments, unseen repo) + the 10-dollar adoption gate (2/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| A1.1 | DESIGN.md amended IN WRITING before any run, per RESULTS §10: unseen-repo protocol (identifier-renamed public repo prepared and committed), question mix (2× class B, E/F at half), non-inferiority bar re-specified at a reachable level, κ-on-20% rule with degenerate-marginal handling, B-instructed arm added | ✅ DONE | [`1d30e02`](https://github.com/shaahink/DevContext2/commit/1d30e02) |
| A1.2 | The pre-registered adoption gate: arm B alone, eShop, 18 runs, unchanged prompt, run as a bg child; mcp_call_share reported against the 0.2 floor with the pre-registered branch stated either way | ✅ DONE | [`4f51931`](https://github.com/shaahink/DevContext2/commit/4f51931) |

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
| 26 | A1 | Deliver | 1 | 08-14 03:47 | 0:11 | Progress |  | 2 | gates green (none configured) | $3.7595 |  | 89,159/30,359 |
| 27 | A1 | Deliver | 1 | 08-14 03:59 | 0:09 | Progress |  | 1 | gates green (none configured) | $2.2953 |  | 78,089/15,861 |
| 28 | A1 | Deliver | 1 | 08-14 04:08 | 0:31 | Advanced | A1.2 | 2 | gates green (none configured) | $2.1665 |  | 62,958/21,017 |

## Money

_What this run has cost, from its own `costs` rows. Same numbers as `conductor money`._

| scope | sessions | tokens | cache reads | cost | checkpoints | tok/ckpt | $/ckpt |
|---|---|---|---|---|---|---|---|
| **run total** | 28 | 288M | 98.4% | $209.82 | 18 | 16M | $11.66 |
| stage T1 | 4 | 68M | 98.5% | $48.38 | 4 | 17M | $12.10 |
| stage V1 | 3 | 51.3M | 98.5% | $36.51 | 3 | 17.1M | $12.17 |
| stage E1 | 12 | 74.2M | 98.4% | $53.35 | 4 | 18.6M | $13.34 |
| stage D1 | 4 | 51.9M | 98.3% | $38.33 | 4 | 13M | $9.58 |
| stage R1 | 1 | 14.5M | 98.3% | $10.57 | 1 | 14.5M | $10.57 |
| stage A1 | 4 | 28.1M | 97.8% | $22.69 | 2 | 14.1M | $11.35 |
| 2026-08 | 28 | 288M | 98.4% | $209.82 | 18 | 16M | $11.66 |

_Where the money goes: agent $209.61 (100%) · gate $0.19 (0%) · advisor $0.02 (0%) · blended $0.73/M tokens._

## Timeline

_Transitions with duration, from the event log (`.conductor/events.jsonl`)._

```
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
08-14 04:47:09  • session #25 A1 → Advanced · done A1.1 · 5 commit(s)  (39m04s)
08-14 04:47:10  • session #26 A1 Deliver started (attempt 1/4)
08-14 04:58:58  • session #26 A1 → Progress · 2 commit(s)  (11m47s)
08-14 04:59:01  • session #27 A1 Deliver started (attempt 1/4)
08-14 05:08:35  • session #27 A1 → Progress · 1 commit(s)  (9m34s)
08-14 05:08:39  • session #28 A1 Deliver started (attempt 1/4)
08-14 05:39:59  • session #28 A1 → Advanced · done A1.2 · 2 commit(s)  (31m19s)
08-14 06:02:54  ▪ gate fast-engine FAIL [phase]  (2m16s)
08-14 06:02:54  ▪ gate guards pass [phase]  (2m41s)
08-14 06:02:54  ▪ gate battery FAIL [phase]  (3m43s)
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 28 · retries 8 (29 %) · overall Alert
⛔ [same-failure-loop] stage E1: 8 consecutive sessions made no progress
⚠ [context-saturation] session #16: 23,675,141 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #4: 20,624,874 context tokens (≥ 20,000,000)
⚠ [context-saturation] session #7: 20,985,542 context tokens (≥ 20,000,000)
```

## Repo

_Live git snapshot (branch, working tree, sync vs upstream)._

```
branch: feat/pre-release-engine
working tree: M PRE-RELEASE-ENGINE-TRACKER.md, M eval-results/2026-08-14/mcp-qa.md
vs upstream: up to date
```

### Commits by session

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
- **s26 (A1 Deliver)** — 2 commit(s):
  - [`e15e576`](https://github.com/shaahink/DevContext2/commit/e15e576) eval(probe): the adoption gate reports one statistic and names the branch it fires (A1.2)
  - [`c1d1ed7`](https://github.com/shaahink/DevContext2/commit/c1d1ed7) eval(probe): a re-measurement gets its own ledger, and the revised surface is measured first (A1.2)
- **s27 (A1 Deliver)** — 1 commit(s):
  - [`76a7e2b`](https://github.com/shaahink/DevContext2/commit/76a7e2b) eval(probe): the gate names which pre-flight assertions a B-only batch can carry (A1.2)
- **s28 (A1 Deliver)** — 2 commit(s):
  - [`ec64ea9`](https://github.com/shaahink/DevContext2/commit/ec64ea9) chore(tracker): hand off the cleared adoption gate; stage A1 closed (A1.2)
  - [`4f51931`](https://github.com/shaahink/DevContext2/commit/4f51931) eval(probe): the adoption gate clears - a described surface moves unprompted use 20x (A1.2)

## Last gate run

fast-engine:FAIL-retry · guards:OK · battery:FAIL-retry

<details><summary>fast-engine — exit 2</summary>

```
[conductor] retried once (SC4.1): the first attempt exited 2 after 106s. Below is the SECOND run.
--- Step 0: Clear orphaned build-locking processes ---
  PASS  Cleared 1 orphaned process(es)

--- Step 1: Build solution ---
  PASS  Build succeeded

--- Step 1a: Contract sweep (dead proto fields) ---
  PASS  Contract sweep clean (every response field read or allow-listed with a reason)

--- Step 2: Fast unit tests ---
  PASS  Fast tests passed

--- Step 2b: MCP QA gate (serial) ---
Test run for C:\Code\DevContext2-engine\tests\DevContext.Core.Tests\bin\Debug\net10.0\DevContext.Core.Tests.dll (.NETCoreApp,Version=v10.0) A total of 1 test files matched the specified pattern. Test run for C:\Code\DevContext2-engine\tests\DevContext.Server.Tests\bin\Debug\net10.0\DevContext.Server.Tests.dll (.NETCoreApp,Version=v10.0) A total of 1 test files matched the specified pattern. No test matches the given testcase filter `Category=McpQa` in C:\Code\DevContext2-engine\tests\DevContext.Server.Tests\bin\Debug\net10.0\DevContext.Server.Tests.dll  [xUnit.net 00:00:39.06]     DevContext.Core.Tests.McpQaGateTests.McpQaHarness_Passes_Against_Dogfood [FAIL]   Failed DevContext.Core.Tests.McpQaGateTests.McpQaHarness_Passes_Against_Dogfood [38 s]   Error Message:    MCP QA harness failed (exit 1 --- stderr ---  --- stdout --- DevContext MCP QA Harness (M4 post-gate) Repo: C:/Users/shahi/source/repos/run-aspnetcore-microservices/src MCP:  C:\Code\DevContext2-engine\src\DevContext.Mcp\bin\Debug\net10.0\devcontext-mcp.exe    q1-overview: What is this repo? (one-call repo brief) ... PASS     overview 199 tok, archetype=true flows=true counts=true services=true  [1c 199tok]   q2-checkout-flow: How does checkout work? ... PASS     trace found: 46 steps, cross-service, 1476 tok  [1c 1476tok]   q3-discount-callers: Who calls the Discount service? ... PASS     10 Discount matches, usages=true  [2c 699tok]   q4-impact-of-handler: What breaks if I change CheckoutBasketCommandHandler? ... PASS     impact up=5 down=12 total=17  [3c 1146tok]   q5-ambiguous-product: What is Product? (disambiguation check) ... PASS     resolve returned 10 candidates, ambiguous=true, hint=yes  [1c 584tok]   q6-config-lookup: What config keys are used? ... PASS     config returned 4 keys  [1c 297tok]   q7-tests-for: What tests cover CheckoutBasketCommandHandler? ... PASS     tests_for found 0 tests (best-effort), node=CheckoutBasketCommandHandler  [2c 182tok]   q8-query-addressing: Address impact/node/read_source by fuzzy query (not nodeId)? ... PASS     queryΓåÆ impact=true node=true read_source=true  [3c 717tok]   q9-entrypoints-summary: List entry points as a bounded summary (not a token wall)? ... FAIL     byKind=true, showing 15/34 (848tok), fullΓåÆ0  [2c 951tok]   q10-self-describing: Do best-effort tools explain their method (what 0 means)? ... FAIL     method note: tests_for=false, config=false  [3c 172tok]   q11-trace-budget: Does trace respect a token budget with named omissions? ... FAIL     budget400: 0 steps/undefinedtok omitted=undefined; full: 0 steps/undefinedtok  [2c 84tok]  ======================================== QA Scored Table (M4 post-gate) ======================================== | Question     | Pass | Calls | Tokens | Detail | |--------------|------|-------|--------|--------| | q1-overview  | YES  | 1     | 199    | overview 199 tok, archetype=true flows=true counts=true services=true | | q2-checkout-flow | YES  | 1     | 1476   | trace found: 46 steps, cross-service, 1476 tok | | q3-discount-callers | YES  | 2     | 699    | 10 Discount matches, usages=true | | q4-impact-of-handler | YES  | 3     | 1146   | impact up=5 down=12 total=17 | | q5-ambiguous-product | YES  | 1     | 584    | resolve returned 10 candidates, ambiguous=true, hint=yes | | q6-config-lookup | YES  | 1     | 297    | config returned 4 keys | | q7-tests-for | YES  | 2     | 182    | tests_for found 0 tests (best-effort), node=CheckoutBasketCommandHandler | | q8-query-addressing | YES  | 3     | 717    | queryΓåÆ impact=true node=true read_source=true | | q9-entrypoints-summary | NO   | 2     | 951    | byKind=true, showing 15/34 (848tok), fullΓåÆ0 | | q10-self-describing | NO   | 3     | 172    | method note: tests_for=false, config=false | | q11-trace-budget | NO   | 2     | 84     | budget400: 0 steps/undefinedtok omitted=undefined; full: 0 steps/undefinedtok | | gate-checkout | NO   | 2     | 77     | 2 calls, 77 tok, found=false, 0 steps, cross-service=false |  QA Score: 8/12 passing (67%) Gate (checkout <=3c/2ktok): FAIL Gate (QA >=90% actionable): FAIL )   Stack Trace:      at DevContext.Core.Tests.McpQaGateTests.McpQaHarness_Passes_Against_Dogfood() in C:\Code\DevContext2-engine\tests\DevContext.Core.Tests\McpQaGateTests.cs:line 66 --- End of stack trace from previous location ---  Failed!  - Failed:     1, Passed:     1, Skipped:     0, Total:     2, Duration: 38 s - DevContext.Core.Tests.dll (net10.0)
  FAIL  MCP QA gate failed

GATE: FAIL (step 2b - MCP QA)
```
</details>

<details><summary>battery — exit 5</summary>

```
handle: 59193a11ee62449db9c73b3ec5355947
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
  PASS  Eval cached - engine inputs unchanged since last green (2026-08-14 05:52); verdict transfers

--- Step 4: CLI --strict matrix ---
  Running: analyze . --strict...
    exit 0 (clean)
  Running: analyze --format json --strict...
    exit 0 (clean)
  Running: analyze --format html --strict...
    exit 2 (self-check failures)
  Running: analyze --dry-run...
    exit 0 (clean)
  Running: analyze --max-tokens 2000 --strict...
    exit 0 (clean)
  PASS  CLI matrix: all commands ran successfully

--- Step 4b: CLI query ops (T3.7) ---
    entrypoints: 2 entries across 1 kind(s)
    stats: 15 nodes, 2 entries, 9 waterfall stages
    trace (no focus): required-focus guard fired (exit 1)
    trace('POST /orders'): found, root=POST /orders
  PASS  CLI query ops: entrypoints/stats/trace return graph JSON

--- Step 5: App check (pnpm check) ---
 All files pass linting.   > devcontext-app@0.0.0 test C:\Code\DevContext2-engine\src\DevContext.App > ng test --watch=false  ❯ Building... ✔ Building... Application bundle generation complete. [11.128 seconds] - 2026-08-14T05:02:13.489Z    RUN  v4.1.9 C:/Code/DevContext2-engine/src/DevContext.App   ❯ |devcontext-app| src/app/ui/graph-canvas/graph-layout.spec.ts (8 tests | 1 failed) 11889ms      × same input twice → identical geometry 6211ms System.Management.Automation.RemoteException ⎯⎯⎯⎯⎯⎯⎯ Failed Tests 1 ⎯⎯⎯⎯⎯⎯⎯ System.Management.Automation.RemoteException  FAIL  |devcontext-app| src/app/ui/graph-canvas/graph-layout.spec.ts > graph-layout (D4.1 determinism + no-clip contract) > same input twice → identical geometry Error: Test timed out in 5000ms. If this is a long-running test, pass a timeout value as the last argument or configure it globally with "testTimeout".  ❯ src/app/ui/graph-canvas/graph-layout.spec.ts:41:3      39|      40| describe('graph-layout (D4.1 determinism + no-clip contract)', () => {      41|   it('same input twice → identical geometry', async () => {        |   ^      42|     const { nodes, edges } = bigFixture();      43|     const a = await layoutGraph(nodes, edges); System.Management.Automation.RemoteException ⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯[1/1]⎯ System.Management.Automation.RemoteException   Test Files  1 failed | 19 passed (20)       Tests  1 failed | 158 passed (159)    Start at  06:02:14    Duration  31.71s (transform 21.59s, setup 19.72s, import 70.53s, tests 19.69s, environment 122.15s)   ELIFECYCLE  Test failed. See above for more details.  ELIFECYCLE  Command failed with exit code 1.
  FAIL  pnpm check failed

GATE: FAIL (step 5 - app check)
```
</details>

## Last session result

> SESSION-RESULT:** A1.2 — the pre-registered adoption gate — is DELIVERED and claimed DONE (`conductor task --done A1.2`), evidence at `eval-results/2026-08-14/a1-adoption-gate/A1.2-EVIDENCE.md` plus the generated `ADOPTION-GATE.md`, commits `4f51931` (18 rows + transcripts + evidence) and `ec64ea9` (handoff), branch pushed, tree clean. **The gate CLEARS: median per-run `mcp_call_share` = 0.306 against the pre-registered 0.20 floor**, so DESIGN §3.1's `>= 0.2` PROCEED branch fires — the full W3 study runs as specified, arm B stays the primary treatment arm, and B-vs-G remains a genuine test of the MCP. The pilot's identical measurement on the 22-tool *undescribed* surface was 0.015 with 17/18…

## Tracker handoff

```
A1.2 CLAIMED (4f51931). Evidence eval-results/2026-08-14/a1-adoption-gate/A1.2-EVIDENCE.md
+ ADOPTION-GATE.md beside it. THE GATE CLEARS: median per-run mcp_call_share 0.306 vs the
pre-registered 0.20 floor, so DESIGN §3.1's PROCEED branch fires - W3 runs as specified, arm B
stays primary, B-vs-G remains a test of the MCP. Pilot (22 undescribed tools) was 0.015, 17/18
below floor; this batch (14 curated described tools, prompt byte-unchanged) is 5/18 below.
Pooled 0.354 and mean 0.335 clear too, so the verdict does not turn on the estimator. $6.42,
0 censored, 0 isolation breaches; two devcontextShas in the rows are REPORT.md + the reporting
script only, so one binary under test - verified, not assumed.
THE TRAP THAT ATE s26 AND s27 IS SOLVED, use this shape: a `conductor bg` child dies with its
parent, so LAUNCH FIRST TURN then BLOCK IN-SESSION on a PowerShell poll loop (45-60s sleeps,
8-min foreground chunks) until the row count hits target. 18 runs = 22 min wall; ledger resumes
on (question,arm,rep) so a kill only costs the row in flight.
NEXT: STAGE A1 IS CLOSED (A1.1+A1.2 both DONE) and Z1.1/Z1.2 are the only open cards left in the
run. Z1.1 wants "whatever A1.2 earned" in the README: the honest sentence is the 0.015 -> 0.306
move on an unchanged prompt, and note adoption is not dominance (80 mcp vs 146 native calls).
STILL UNBUILT, owned by nobody: questions/Driewie.json + the 2nd class-B question per repo.
```
