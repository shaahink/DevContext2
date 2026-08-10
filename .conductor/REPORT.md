# Conductor — DevContext agent probe - does the MCP help an agent browse code run report

_Updated 2026-08-10 23:08 UTC · branch `feat/agent-probe` · HEAD `919f6fb`_

**Status:** Idle
**Stage:** K1 — Ground truth - pre-registered question sets · attempts used 0
**Checkpoints:** 3/12 done · **Sessions run:** 1 · **Cost:** $5.5785 (agent $5.5780 + gates $0.0005) · **Tokens:** 137,683 in / 63,990 out
**Confirmed phases:** K1

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| K1 | Ground truth - pre-registered question sets | ██████████ 3/3 | confirmed ✓ |
| H1 | Probe harness - three-arm runner | ░░░░░░░░░░ 0/2 | todo |
| P1 | Smoke - prove arm isolation and cost accounting before spending | ░░░░░░░░░░ 0/2 | todo |
| P2 | Pilot - six eShop questions, three arms, three repetitions | ░░░░░░░░░░ 0/1 | todo |
| A1 | Grade and analyse | ░░░░░░░░░░ 0/2 | todo |
| R1 | Report and verdict | ░░░░░░░░░░ 0/2 | todo |

<details> ✅<summary>K1 — Ground truth - pre-registered question sets (3/3)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| K1.1 | `questions/eShop.json` exists with 6 questions covering classes A B C D E F; every key symbol resolves in eval-repos/eShop at 9b4f9434; class D is a real sibling-attribution trap and class E has an empty mustMention | ✅ DONE | [`e210f51`](https://github.com/shaahink/DevContext2/commit/e210f51) |
| K1.2 | `questions/TodoApi.json` exists, same shape, keys verified at 307a1ead | ✅ DONE | [`e210f51`](https://github.com/shaahink/DevContext2/commit/e210f51) |
| K1.3 | `questions/FluentValidation.json` exists, same shape, keys verified at 94397908 | ✅ DONE | [`e210f51`](https://github.com/shaahink/DevContext2/commit/e210f51) |

</details>

<details><summary>H1 — Probe harness - three-arm runner (0/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| H1.1 | `run-probe.mjs` drives all three arms as headless subprocesses, is resumable from `runs.jsonl`, caps each run at $1.50, and refuses more than 60 runs per invocation | ⬜ TODO | - |
| H1.2 | One real end-to-end run per arm is recorded in `results/runs.jsonl` with answer, toolCalls, costUsd, usage, numTurns, durationMs — and the raw result JSON plus transcript are saved under `results/raw` | ⬜ TODO | - |

</details>

<details><summary>P1 — Smoke - prove arm isolation and cost accounting before spending (0/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| P1.1 | Arm isolation proven from recorded transcripts: arm G made 0 mcp calls, arm M made 0 Read/Grep/Glob calls, cost is non-zero on every run, and analyze reported cached true for every arm | ⬜ TODO | - |
| P1.2 | Tool-schema tax measured — the turn-1 input + cache-creation token delta between arm G and arm B on an identical trivial prompt, recorded as an absolute count and as a share of median run cost | ⬜ TODO | - |

</details>

<details><summary>P2 — Pilot - six eShop questions, three arms, three repetitions (0/1)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| P2.1 | 54 eShop runs recorded (6 questions x 3 arms x 3 reps), question order randomised, censored runs kept and flagged, per-arm censoring rate reported | ⬜ TODO | - |

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

## Money

_What this run has cost, from its own `costs` rows. Same numbers as `conductor money`._

| scope | sessions | tokens | cache reads | cost | checkpoints | tok/ckpt | $/ckpt |
|---|---|---|---|---|---|---|---|
| **run total** | 1 | 5.39M | 96.3% | $5.58 | 3 | 1.8M | $1.86 |
| stage K1 | 1 | 5.39M | 96.3% | $5.58 | 3 | 1.8M | $1.86 |
| 2026-08 | 1 | 5.39M | 96.3% | $5.58 | 3 | 1.8M | $1.86 |

_Where the money goes: agent $5.58 (100%) · gate $0.00 (0%) · blended $1.03/M tokens._

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
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 1 · retries 0 (0 %) · overall Ok
✓ no health concerns detected
```

## Repo

_Live git snapshot (branch, working tree, sync vs upstream)._

```
branch: feat/agent-probe
working tree: M AGENT-PROBE-TRACKER.md, M eval/agent-probe/probe.plan.json
```

### Commits by session

- **s1 (K1 Deliver)** — 4 commit(s):
  - [`5e41567`](https://github.com/shaahink/DevContext2/commit/5e41567) docs(probe): hand off K1 -> H1
  - [`f7d40f4`](https://github.com/shaahink/DevContext2/commit/f7d40f4) eval(probe): K1.3 - pre-registered FluentValidation question set + mustNotMention scoring rule
  - [`e6205fb`](https://github.com/shaahink/DevContext2/commit/e6205fb) eval(probe): K1.2 - pre-registered TodoApi question set, keys read from source
  - [`e210f51`](https://github.com/shaahink/DevContext2/commit/e210f51) eval(probe): K1.1 - pre-registered eShop question set, six classes, keys read from source

## Last gate run

probe-fast:OK · probe-full:OK

## Last session result

> SESSION-RESULT: Stage K1 landed complete — `questions/eShop.json`, `questions/TodoApi.json` and `questions/FluentValidation.json`, 18 pre-registered questions covering classes A–F on each of the three pinned repos, every answer key confirmed against the source at its pinned SHA rather than taken from a doc; `node eval/agent-probe/verify.mjs --tier fast` is GREEN, which is the proof that each key symbol and each trap term actually resolves in the cloned tree. K1.1/K1.2/K1.3 are claimed through `conductor task --done` with evidence paths; commits e210f51, e6205fb, f7d40f4, 5e41567; working tree clean. Nothing is red and nothing is blocked. Two pre-run decisions are on the record: one question …

## Tracker handoff

```
last: K1 CLOSED — K1.1/K1.2/K1.3 all DONE. 18 pre-registered questions (one per class A–F)
  in `questions/{eShop,TodoApi,FluentValidation}.json`. Commits e210f51, e6205fb, f7d40f4.
stage: **K1 complete, attempt 1, no reds. H1 is next.**
gate: `node eval/agent-probe/verify.mjs --tier fast` → GREEN; every key resolves in the cloned
  repo at its pinned SHA. The keys are now FROZEN — a later stage may not adjust one.
next: **H1.1** — `run-probe.mjs`: three arms as headless subprocesses, resumable from
  `results/runs.jsonl`, `--max-budget-usd 1.50` on every invocation, hard refusal above 60 runs
  per invocation. DESIGN.md §8 carries the three command lines verbatim.
trap: eShop @ 9b4f9434 is the **Aspire** eShop, not eShopOnContainers — there is no
  `UserCheckoutAcceptedIntegrationEvent`, Basket.API has exactly ONE subscription, and every
  event in the repo has a subscriber, so the class E controls are false-premise questions
  rather than orphan events. Answers echoing the old sample are recall, not reading.
note: one question per class, not DESIGN §3.3's B×2 + E/F at ½ — see the ledger and file notes.
```
