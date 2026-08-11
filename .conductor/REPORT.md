# Conductor — DevContext agent probe - does the MCP help an agent browse code run report

_Updated 2026-08-11 01:29 UTC · branch `feat/agent-probe` · HEAD `1e540bc`_

**Status:** Idle
**Stage:** P2 — Pilot - six eShop questions, three arms, three repetitions · attempts used 1
**Checkpoints:** 8/12 done · **Sessions run:** 4 · **Cost:** $26.3429 (agent $26.3394 + gates $0.0035) · **Tokens:** 533,042 in / 259,410 out
**Confirmed phases:** K1, H1, P1

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| K1 | Ground truth - pre-registered question sets | ██████████ 3/3 | confirmed ✓ |
| H1 | Probe harness - three-arm runner | ██████████ 2/2 | confirmed ✓ |
| P1 | Smoke - prove arm isolation and cost accounting before spending | ██████████ 2/2 | confirmed ✓ |
| P2 | Pilot - six eShop questions, three arms, three repetitions | ██████████ 1/1 | gating… |
| A1 | Grade and analyse | ░░░░░░░░░░ 0/2 | todo |
| R1 | Report and verdict | ░░░░░░░░░░ 0/2 | todo |

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
| P2.1 | 54 eShop runs recorded (6 questions x 3 arms x 3 reps), question order randomised, censored runs kept and flagged, per-arm censoring rate reported | ✅ DONE | - |

</details>

<details><summary>A1 — Grade and analyse (0/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| A1.1 | Deterministic grading pass complete: mustMention hits, mustNotMention violations, expectedVerdict match, citation resolution — scored per run into `results/graded.jsonl` | ⬜ TODO | - |
| A1.2 | Judge pass complete on anonymised final answers only, plus the paired analysis: median log2 cost ratio with bootstrap CI, accuracy difference with CI, fabrication rate, mcp call share | ⬜ TODO | - |

</details>

<details><summary>R1 — Report and verdict (0/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| R1.1 | `eval-results/agent-probe/RESULTS.md` states the verdict against the four pre-registered outcomes, with the per-class breakdown and the honest pilot interval | ⬜ TODO | - |
| R1.2 | Human-check sample (20%, stratified) written to a separate file for the owner; report names exactly what the full run needs to turn this pilot into a defensible number | ⬜ TODO | - |

</details>

## Sessions

| # | Stage | Kind | Att | Started (UTC) | Dur | Outcome | New DONE | Commits | Gates | Cost | Overhead | Tokens |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | K1 | Deliver | 1 | 08-10 22:28 | 0:16 | Advanced | K1.1 K1.2 K1.3 | 4 | probe-fast:OK | $5.5780 | $0.0005 | 137,683/63,990 |
| 2 | H1 | Deliver | 1 | 08-10 23:08 | 0:30 | Advanced | H1.1 H1.2 | 4 | probe-fast:OK | $7.3018 | $0.0005 | 141,236/70,278 |
| 3 | P1 | Deliver | 1 | 08-10 23:40 | 0:18 | Advanced | P1.1 P1.2 | 4 | probe-fast:OK | $6.8159 | $0.0013 | 122,889/64,955 |
| 4 | P2 | Deliver | 1 | 08-11 00:00 | 1:29 | GatesRed | P2.1 | 6 | probe-fast:FAIL-retry | $6.6438 | $0.0011 | 131,234/60,187 |

## Money

_What this run has cost, from its own `costs` rows. Same numbers as `conductor money`._

| scope | sessions | tokens | cache reads | cost | checkpoints | tok/ckpt | $/ckpt |
|---|---|---|---|---|---|---|---|
| **run total** | 3 | 22M | 97.3% | $19.70 | 7 | 3.14M | $2.81 |
| stage K1 | 1 | 5.39M | 96.3% | $5.58 | 3 | 1.8M | $1.86 |
| stage H1 | 1 | 8.47M | 97.5% | $7.30 | 2 | 4.23M | $3.65 |
| stage P1 | 1 | 8.1M | 97.7% | $6.82 | 2 | 4.05M | $3.41 |
| 2026-08 | 3 | 22M | 97.3% | $19.70 | 7 | 3.14M | $2.81 |

_Where the money goes: agent $19.70 (100%) · gate $0.00 (0%) · blended $0.90/M tokens._

## Timeline

_Transitions with duration, from the event log (`.conductor/events.jsonl`)._

```
08-10 23:28:38  ◆ run started · DevContext agent probe - does the MCP help an agent browse code
08-10 23:28:39  ▸ stage K1 entered — Ground truth - pre-registered question sets
08-10 23:28:39  • session #1 K1 Deliver started (attempt 1/6)
08-10 23:44:46  ▪ gate probe-fast pass [session]  (5.0s)
08-10 23:44:47  • session #1 K1 → Advanced · done K1.1,K1.2,K1.3 · 4 commit(s)  (16m08s)
08-11 00:08:37  ◆ run resumed · DevContext agent probe - does the MCP help an agent browse code
08-11 00:08:50  ▪ gate probe-fast pass [phase]  (5.2s)
08-11 00:08:50  ▪ gate probe-full pass [phase]  (4.8s)
08-11 00:08:50  ✓ checkpoint K1.1 confirmed
08-11 00:08:50  ✓ checkpoint K1.2 confirmed
08-11 00:08:50  ✓ checkpoint K1.3 confirmed
08-11 00:08:50  ▸ stage K1 confirmed  (40m11s)
08-11 00:08:54  ▸ stage H1 entered — Probe harness - three-arm runner
08-11 00:08:54  • session #2 H1 Deliver started (attempt 1/4)
08-11 00:39:56  ▪ gate probe-fast pass [session]  (5.4s)
08-11 00:39:59  • session #2 H1 → Advanced · done H1.1,H1.2 · 4 commit(s)  (31m04s)
08-11 00:40:17  ▪ gate probe-fast pass [phase]  (7.1s)
08-11 00:40:17  ▪ gate probe-full pass [phase]  (10.6s)
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
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 4 · retries 0 (0 %) · overall Ok
✓ no health concerns detected
```

## Repo

_Live git snapshot (branch, working tree, sync vs upstream)._

```
branch: feat/agent-probe
working tree: clean
vs upstream: up to date
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

## Last gate run

probe-fast:FAIL-retry

<details><summary>probe-fast — exit 1</summary>

```
[conductor] retried once (SC4.1): the first attempt exited 1 after 6s. Below is the SECOND run.
```
</details>

## Last session result

> SESSION-RESULT: Stage P2 delivered — P2.1 claimed DONE via `conductor task --done P2.1 --evidence eval/agent-probe/results/p2.1-pilot.md`, all 54 pilot cells recorded, commits 8807f48 / be87562 / 2021561 / 21de408 / 647c745 / 1e540bc pushed to `origin/feat/agent-probe`, working tree clean. The pilot's headline is a pre-registered check failing: arm B's `mcp_call_share` median is 0.01 against a 0.20 floor, with classes D/E/F using the MCP zero times across 3/3 reps despite it being connected and offered — a real negative result about the treatment, not a harness fault. Arm M costs more than arm G on all six questions. One red is open and was NOT made green: `audit-preflight`'s `A1-analyze-cac…

## Tracker handoff

```
last: **P2.1 DONE, 54/54 cells** — commits 8807f48, be87562, 2021561, 647c745. Isolation 54/54,
  cost 54/54, 1 censored run kept+flagged (eshop-c1/M/rep1, `error_max_budget_usd`, $1.5134).
  Spend $23.67 recorded + $0.99 quarantined = **$24.66**. Read `results/p2.1-pilot.md`.
result: **DESIGN §3.1's arm-B manipulation check FAILED** — median mcp share **0.01**, 17/18 below
  the 0.20 floor. Classes D/E/F used the MCP **zero** times, 3/3 reps each, with 22 tools offered
  and `devcontext:connected` on every run: agent CHOICE, not availability. Arm B does not measure
  what DESIGN wrote it to measure. Medians G $0.2702 / M $0.5073 / B $0.2509 — **M costs more than
  G on all six questions individually**. Between-rep CV 0.116, so n=5 can resolve a 20% effect.
red: **`verify.mjs --tier fast` is RED and I did not make it green.** `A1-analyze-cached` fails on
  exactly those 9 zero-mcp arm-B runs. A2/A3/X isolation pass 54/54 and A4 pass 54/54, so nothing
  is void; batch warmth is proven separately by the pre-batch warm gate (bg log lines 3-5).
escalation: the A1 predicate assumed an MCP-capable arm always calls `analyze`. Two options, both
  costed, with a recommendation, in **`results/p2.1-gate-red-A1.md`** — owner/next session's call.
next: A1.1 grading. Do NOT restart the pilot. Trap: `subtype` is not a success signal from this
  CLI — read `is_error` and `terminal_reason` too (see `results/infra-failures.jsonl`).
```
