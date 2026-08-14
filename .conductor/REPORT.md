# Conductor — DevContext agent probe - does the MCP help an agent browse code run report

_Updated 2026-08-11 02:30 UTC · branch `feat/agent-probe` · HEAD `157420b`_

**Status:** Completed
**Stage:** R1 — Report and verdict · attempts used 0
**Checkpoints:** 12/12 done · **Sessions run:** 7 · **Cost:** $51.7443 (agent $51.7389 + gates $0.0053) · **Tokens:** 950,772 in / 468,326 out
**Confirmed phases:** K1, H1, P1, P2, A1, R1

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| K1 | Ground truth - pre-registered question sets | ██████████ 3/3 | confirmed ✓ |
| H1 | Probe harness - three-arm runner | ██████████ 2/2 | confirmed ✓ |
| P1 | Smoke - prove arm isolation and cost accounting before spending | ██████████ 2/2 | confirmed ✓ |
| P2 | Pilot - six eShop questions, three arms, three repetitions | ██████████ 1/1 | confirmed ✓ |
| A1 | Grade and analyse | ██████████ 2/2 | confirmed ✓ |
| R1 | Report and verdict | ██████████ 2/2 | confirmed ✓ |

<details> ✅<summary>K1 — Ground truth - pre-registered question sets (3/3)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| K1.1 | `questions/eShop.json` exists with 6 questions covering classes A B C D E F; every key symbol resolves in eval-repos/eShop at 9b4f9434; class D is a real sibling-attribution trap and class E has an empty mustMention | ✅ DONE | [`e210f51`](https://github.com/shaahink/DevContext2/commit/e210f51) |
| K1.2 | `questions/TodoApi.json` exists, same shape, keys verified at 307a1ead | ✅ DONE | [`e210f51`](https://github.com/shaahink/DevContext2/commit/e210f51) |
| K1.3 | `questions/FluentValidation.json` exists, same shape, keys verified at 94397908 | ✅ DONE | [`e210f51`](https://github.com/shaahink/DevContext2/commit/e210f51) |

</details>

<details> ✅<summary>H1 — Probe harness - three-arm runner (2/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| H1.1 | `run-probe.mjs` drives all three arms as headless subprocesses, is resumable from `runs.jsonl`, caps each run at $1.50, and refuses more than 60 runs per invocation | ✅ DONE | [`2d20636`](https://github.com/shaahink/DevContext2/commit/2d20636) |
| H1.2 | One real end-to-end run per arm is recorded in `results/runs.jsonl` with answer, toolCalls, costUsd, usage, numTurns, durationMs — and the raw result JSON plus transcript are saved under `results/raw` | ✅ DONE | [`2d20636`](https://github.com/shaahink/DevContext2/commit/2d20636) |

</details>

<details> ✅<summary>P1 — Smoke - prove arm isolation and cost accounting before spending (2/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| P1.1 | Arm isolation proven from recorded transcripts: arm G made 0 mcp calls, arm M made 0 Read/Grep/Glob calls, cost is non-zero on every run, and analyze reported cached true for every arm | ✅ DONE | [`53ca6c5`](https://github.com/shaahink/DevContext2/commit/53ca6c5) |
| P1.2 | Tool-schema tax measured — the turn-1 input + cache-creation token delta between arm G and arm B on an identical trivial prompt, recorded as an absolute count and as a share of median run cost | ✅ DONE | [`53ca6c5`](https://github.com/shaahink/DevContext2/commit/53ca6c5) |

</details>

<details> ✅<summary>P2 — Pilot - six eShop questions, three arms, three repetitions (1/1)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| P2.1 | 54 eShop runs recorded (6 questions x 3 arms x 3 reps), question order randomised, censored runs kept and flagged, per-arm censoring rate reported | ✅ DONE | [`8807f48`](https://github.com/shaahink/DevContext2/commit/8807f48) |

</details>

<details> ✅<summary>A1 — Grade and analyse (2/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| A1.1 | Deterministic grading pass complete: mustMention hits, mustNotMention violations, expectedVerdict match, citation resolution — scored per run into `results/graded.jsonl` | ✅ DONE | [`c366338`](https://github.com/shaahink/DevContext2/commit/c366338) |
| A1.2 | Judge pass complete on anonymised final answers only, plus the paired analysis: median log2 cost ratio with bootstrap CI, accuracy difference with CI, fabrication rate, mcp call share | ✅ DONE | [`33aba13`](https://github.com/shaahink/DevContext2/commit/33aba13) |

</details>

<details> ✅<summary>R1 — Report and verdict (2/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| R1.1 | `eval-results/agent-probe/RESULTS.md` states the verdict against the four pre-registered outcomes, with the per-class breakdown and the honest pilot interval | ✅ DONE | [`cd4d15d`](https://github.com/shaahink/DevContext2/commit/cd4d15d) |
| R1.2 | Human-check sample (20%, stratified) written to a separate file for the owner; report names exactly what the full run needs to turn this pilot into a defensible number | ✅ DONE | [`cd4d15d`](https://github.com/shaahink/DevContext2/commit/cd4d15d) |

</details>

## Sessions

| # | Stage | Kind | Att | Started (UTC) | Dur | Outcome | New DONE | Commits | Gates | Cost | Overhead | Tokens |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | K1 | Deliver | 1 | 08-10 22:28 | 0:16 | Advanced | K1.1 K1.2 K1.3 | 4 | probe-fast:OK | $5.5780 | $0.0005 | 137,683/63,990 |
| 2 | H1 | Deliver | 1 | 08-10 23:08 | 0:30 | Advanced | H1.1 H1.2 | 4 | probe-fast:OK | $7.3018 | $0.0005 | 141,236/70,278 |
| 3 | P1 | Deliver | 1 | 08-10 23:40 | 0:18 | Advanced | P1.1 P1.2 | 4 | probe-fast:OK | $6.8159 | $0.0013 | 122,889/64,955 |
| 4 | P2 | Deliver | 1 | 08-11 00:00 | 1:29 | GatesRed | P2.1 | 6 | probe-fast:FAIL-retry | $6.6438 | $0.0011 | 131,234/60,187 |
| 5 | P2 | Fix | 2 | 08-11 01:29 | 0:17 | Advanced | A1.1 | 4 | probe-fast:OK | $7.0867 | $0.0007 | 148,752/67,848 |
| 6 | A1 | Deliver | 1 | 08-11 01:47 | 0:20 | Advanced | A1.2 | 3 | probe-fast:OK | $11.9547 | $0.0006 | 137,247/68,734 |
| 7 | R1 | Deliver | 1 | 08-11 02:08 | 0:21 | Advanced | R1.1 R1.2 | 5 | probe-fast:OK | $6.3581 | $0.0006 | 131,731/72,334 |

## Money

_What this run has cost, from its own `costs` rows. Same numbers as `conductor money`._

| scope | sessions | tokens | cache reads | cost | checkpoints | tok/ckpt | $/ckpt |
|---|---|---|---|---|---|---|---|
| **run total** | 7 | 62.4M | 97.7% | $51.74 | 12 | 5.2M | $4.31 |
| stage K1 | 1 | 5.39M | 96.3% | $5.58 | 3 | 1.8M | $1.86 |
| stage H1 | 1 | 8.47M | 97.5% | $7.30 | 2 | 4.23M | $3.65 |
| stage P1 | 1 | 8.1M | 97.7% | $6.82 | 2 | 4.05M | $3.41 |
| stage P2 | 2 | 15.8M | 97.4% | $13.73 | 2 | 7.92M | $6.87 |
| stage A1 | 1 | 17.9M | 98.9% | $11.96 | 1 | 17.9M | $11.96 |
| stage R1 | 1 | 6.66M | 96.9% | $6.36 | 2 | 3.33M | $3.18 |
| 2026-08 | 7 | 62.4M | 97.7% | $51.74 | 12 | 5.2M | $4.31 |

_Where the money goes: agent $51.74 (100%) · gate $0.01 (0%) · blended $0.83/M tokens._

## Timeline

_Transitions with duration, from the event log (`.conductor/events.jsonl`)._

```
08-11 00:40:18  ✓ checkpoint H1.1 confirmed
08-11 00:40:18  ✓ checkpoint H1.2 confirmed
08-11 00:40:18  ▸ stage H1 confirmed  (31m23s)
08-11 00:40:21  ▸ stage P1 entered — Smoke - prove arm isolation and cost accounting before spending
08-11 00:40:21  • session #3 P1 Deliver started (attempt 1/4)
08-11 00:59:33  ▪ gate probe-fast pass [session]  (13.3s)
08-11 00:59:36  • session #3 P1 → Advanced · done P1.1,P1.2 · 4 commit(s)  (19m14s)
08-11 01:00:01  ▪ gate probe-fast pass [phase]  (14.9s)
08-11 01:00:01  ▪ gate probe-full pass [phase]  (9.4s)
08-11 01:00:01  ✓ checkpoint P1.1 confirmed
08-11 01:00:01  ✓ checkpoint P1.2 confirmed
08-11 01:00:01  ▸ stage P1 confirmed  (19m40s)
08-11 01:00:04  ▸ stage P2 entered — Pilot - six eShop questions, three arms, three repetitions
08-11 01:00:04  • session #4 P2 Deliver started (attempt 1/6)
08-11 02:29:47  ▪ gate probe-fast FAIL [session]  (5.5s)
08-11 02:29:48  • session #4 P2 → GatesRed · done P2.1 · 6 commit(s)  (1h29m44s)
08-11 02:29:49  • session #5 P2 Fix started (attempt 2/6)
08-11 02:47:22  ▪ gate probe-fast pass [session]  (6.8s)
08-11 02:47:23  • session #5 P2 → Advanced · done A1.1 · 4 commit(s)  (17m34s)
08-11 02:47:37  ▪ gate probe-fast pass [phase]  (6.3s)
08-11 02:47:37  ▪ gate probe-full pass [phase]  (6.9s)
08-11 02:47:37  ✓ checkpoint A1.1 confirmed
08-11 02:47:37  ▸ stage P2 confirmed  (1h47m32s)
08-11 02:47:40  ▸ stage A1 entered — Grade and analyse
08-11 02:47:40  • session #6 A1 Deliver started (attempt 1/4)
08-11 03:08:13  ▪ gate probe-fast pass [session]  (6.1s)
08-11 03:08:15  • session #6 A1 → Advanced · done A1.2 · 3 commit(s)  (20m34s)
08-11 03:08:30  ▪ gate probe-fast pass [phase]  (8.1s)
08-11 03:08:30  ▪ gate probe-full pass [phase]  (6.6s)
08-11 03:08:31  ✓ checkpoint A1.2 confirmed
08-11 03:08:31  ▸ stage A1 confirmed  (20m50s)
08-11 03:08:33  ▸ stage R1 entered — Report and verdict
08-11 03:08:33  • session #7 R1 Deliver started (attempt 1/4)
08-11 03:30:33  ▪ gate probe-fast pass [session]  (5.6s)
08-11 03:30:35  • session #7 R1 → Advanced · done R1.1,R1.2 · 5 commit(s)  (22m01s)
08-11 03:30:50  ▪ gate probe-fast pass [phase]  (7.6s)
08-11 03:30:50  ▪ gate probe-full pass [phase]  (7.5s)
08-11 03:30:51  ✓ checkpoint R1.1 confirmed
08-11 03:30:51  ✓ checkpoint R1.2 confirmed
08-11 03:30:51  ▸ stage R1 confirmed  (22m17s)
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 7 · retries 1 (14 %) · overall Ok
✓ no health concerns detected
```

## Repo

_Live git snapshot (branch, working tree, sync vs upstream)._

```
branch: feat/agent-probe
working tree: M AGENT-PROBE-TRACKER.md
vs upstream: 1 ahead
```

### Commits by session

- **s1 (K1 Deliver)** — 4 commit(s):
  - [`5e41567`](https://github.com/shaahink/DevContext2/commit/5e41567) docs(probe): hand off K1 -> H1
  - [`f7d40f4`](https://github.com/shaahink/DevContext2/commit/f7d40f4) eval(probe): K1.3 - pre-registered FluentValidation question set + mustNotMention scoring rule
  - [`e6205fb`](https://github.com/shaahink/DevContext2/commit/e6205fb) eval(probe): K1.2 - pre-registered TodoApi question set, keys read from source
  - [`e210f51`](https://github.com/shaahink/DevContext2/commit/e210f51) eval(probe): K1.1 - pre-registered eShop question set, six classes, keys read from source
- **s2 (H1 Deliver)** — 4 commit(s):
  - [`f9982e2`](https://github.com/shaahink/DevContext2/commit/f9982e2) docs(probe): hand off H1 -> P1
  - [`5f95609`](https://github.com/shaahink/DevContext2/commit/5f95609) eval(probe): H1.2 - three real runs, one per arm, isolation proven from transcripts
  - [`e2f7372`](https://github.com/shaahink/DevContext2/commit/e2f7372) eval(probe): H1.2 - arms were not isolated; rebuild them as exhaustive deny lists
  - [`2d20636`](https://github.com/shaahink/DevContext2/commit/2d20636) eval(probe): H1.1 - three-arm probe runner, resumable, hard-capped
- **s3 (P1 Deliver)** — 4 commit(s):
  - [`97fc7e8`](https://github.com/shaahink/DevContext2/commit/97fc7e8) docs(probe): hand off P1 -> P2
  - [`65b79b0`](https://github.com/shaahink/DevContext2/commit/65b79b0) eval(probe): gate the transcripts themselves, not just the harness's summary of them
  - [`aeaebfd`](https://github.com/shaahink/DevContext2/commit/aeaebfd) eval(probe): P1.2 - tool-schema tax measured, and DESIGN 4.4's statistic is cache-dependent
  - [`53ca6c5`](https://github.com/shaahink/DevContext2/commit/53ca6c5) eval(probe): P1.1 - pre-flight assertions re-derived independently of the harness
- **s4 (P2 Deliver)** — 6 commit(s):
  - [`1e540bc`](https://github.com/shaahink/DevContext2/commit/1e540bc) docs(probe): hand off P2 -> A1
  - [`647c745`](https://github.com/shaahink/DevContext2/commit/647c745) eval(probe): P2.1 - 54/54 eShop pilot cells recorded
  - [`21de408`](https://github.com/shaahink/DevContext2/commit/21de408) eval(probe): P2 - bank recorded pilot runs (mid-batch)
  - [`2021561`](https://github.com/shaahink/DevContext2/commit/2021561) eval(probe): P2 - report quarantined spend and terminal_reason, mid-batch data
  - [`be87562`](https://github.com/shaahink/DevContext2/commit/be87562) eval(probe): P2.1 evidence generator
  - [`8807f48`](https://github.com/shaahink/DevContext2/commit/8807f48) eval(probe): P2 pre-run - censored and broken are not the same event
- **s5 (P2 Fix)** — 4 commit(s):
  - [`ee70d80`](https://github.com/shaahink/DevContext2/commit/ee70d80) docs(probe): hand off P2 gate-green + A1.1 -> A1.2
  - [`1be9129`](https://github.com/shaahink/DevContext2/commit/1be9129) eval(probe): A1.1 - deterministic grading pass over all 54 runs
  - [`de235f6`](https://github.com/shaahink/DevContext2/commit/de235f6) docs(probe): record six pre-registration deviations
  - [`c366338`](https://github.com/shaahink/DevContext2/commit/c366338) eval(probe): A1 warmth proxy - zero-analyze is n/a, not unproven
- **s6 (A1 Deliver)** — 3 commit(s):
  - [`15c0c6b`](https://github.com/shaahink/DevContext2/commit/15c0c6b) eval(probe): A1.2 - judge pass 54/54 and the paired analysis. The pilot is a NULL
  - [`df09b5e`](https://github.com/shaahink/DevContext2/commit/df09b5e) eval(probe): A1.2 paired analysis script (DESIGN 4, 5, 9)
  - [`33aba13`](https://github.com/shaahink/DevContext2/commit/33aba13) eval(probe): A1.2 judge harness - blind-grading redactor + leak scan
- **s7 (R1 Deliver)** — 5 commit(s):
  - [`f27bfe2`](https://github.com/shaahink/DevContext2/commit/f27bfe2) chore(probe): s7 handoff - stage R1 complete, R1.1 + R1.2 landed
  - [`0caa5f7`](https://github.com/shaahink/DevContext2/commit/0caa5f7) eval(probe): R1.2 - the 20% human check, drawn blind and left for the owner
  - [`8bfd7fb`](https://github.com/shaahink/DevContext2/commit/8bfd7fb) chore(probe): s7 R1.1 handoff - verdict is did-not-test-the-MCP, not null
  - [`09eb609`](https://github.com/shaahink/DevContext2/commit/09eb609) eval(probe): R1.1 - RESULTS.md, the pilot verdict
  - [`cd4d15d`](https://github.com/shaahink/DevContext2/commit/cd4d15d) eval(probe): D7 - the question set is one-per-class, not DESIGN 3.3's mix

## Last gate run

probe-fast:OK · probe-full:OK

## Last session result

> SESSION-RESULT: Stage R1 is complete and with it every checkpoint in the probe program — R1.1 (`eval-results/agent-probe/RESULTS.md`, 09eb609) and R1.2 (`eval/agent-probe/results/r1.2-human-sample/`, 0caa5f7), plus the queued D7 entry (cd4d15d) and both queued instructions discharged; gate `verify.mjs --tier fast` GREEN with 0 notes, tree clean, `feat/agent-probe` pushed. The reported verdict is that the B-vs-G contrast **did not test the MCP** — arm B's median `mcp_call_share` is 0.015 with 17/18 runs below §3.1's 0.20 floor, which the pre-registration says must be reported as such and not as a null — while arm M, the one configuration that did exercise the MCP, came out negative at 12/18 a…

## Tracker handoff

```
done: **STAGE R1 COMPLETE, and with it every checkpoint in the program.** R1.1
  `eval-results/agent-probe/RESULTS.md` (09eb609) + **D7** (cd4d15d); R1.2
  `eval/agent-probe/results/r1.2-human-sample/` (0caa5f7). Gate `--tier fast` GREEN, both
  queued instructions discharged, tree clean, branch pushed.
headline, do NOT revert it: an earlier handoff said "the pilot answer is a NULL" - wrong against
  the pre-registration. DESIGN 3.1 pre-committed that arm B mcp share < 0.2 means "the B-vs-G
  comparison is not a test of the MCP and must be reported as such, **not as a null**". Share =
  0.015, 17/18 below the floor. No branch is earned; Null and the power-artifact Regression are
  reported but **subordinate**. Arm M (12/18 vs G 18/18 at 1.508x) is the only real test, and
  it is negative. **D7** = question-set composition (DESIGN 3.3 wants B2 and E/F at half).
owner action, blocking nothing else: grade the 11 `item-NN.txt` files, then
  `node eval/agent-probe/kappa.mjs`. No agent grades them and none reports a kappa.
two pre-registered gates are DEGENERATE at pilot n - the transferable lesson: -0.05
  non-inferiority is unreachable at 18 pairs AND at 30 (one repo at full size); kappa >= 0.8
  needs perfect 11/11 agreement (one disagreement -> 0.62). Neither was moved. See RESULTS.md
  9 and 10, which say what the full run needs. `PRODUCT-DIRECTION.md` 9 is deliberately NOT
  rewritten - it must not be written off a disqualified contrast.
```
