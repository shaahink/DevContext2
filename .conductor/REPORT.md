# Conductor — DevContext agent probe - does the MCP help an agent browse code run report

_Updated 2026-08-10 23:40 UTC · branch `feat/agent-probe` · HEAD `5404eec`_

**Status:** Idle
**Stage:** H1 — Probe harness - three-arm runner · attempts used 0
**Checkpoints:** 5/12 done · **Sessions run:** 2 · **Cost:** $12.8808 (agent $12.8798 + gates $0.0010) · **Tokens:** 278,919 in / 134,268 out
**Confirmed phases:** K1, H1

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| K1 | Ground truth - pre-registered question sets | ██████████ 3/3 | confirmed ✓ |
| H1 | Probe harness - three-arm runner | ██████████ 2/2 | confirmed ✓ |
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

<details> ✅<summary>H1 — Probe harness - three-arm runner (2/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| H1.1 | `run-probe.mjs` drives all three arms as headless subprocesses, is resumable from `runs.jsonl`, caps each run at $1.50, and refuses more than 60 runs per invocation | ✅ DONE | [`2d20636`](https://github.com/shaahink/DevContext2/commit/2d20636) |
| H1.2 | One real end-to-end run per arm is recorded in `results/runs.jsonl` with answer, toolCalls, costUsd, usage, numTurns, durationMs — and the raw result JSON plus transcript are saved under `results/raw` | ✅ DONE | [`2d20636`](https://github.com/shaahink/DevContext2/commit/2d20636) |

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
| 2 | H1 | Deliver | 1 | 08-10 23:08 | 0:30 | Advanced | H1.1 H1.2 | 4 | probe-fast:OK | $7.3018 | $0.0005 | 141,236/70,278 |

## Money

_What this run has cost, from its own `costs` rows. Same numbers as `conductor money`._

| scope | sessions | tokens | cache reads | cost | checkpoints | tok/ckpt | $/ckpt |
|---|---|---|---|---|---|---|---|
| **run total** | 2 | 13.9M | 97.0% | $12.88 | 5 | 2.77M | $2.58 |
| stage K1 | 1 | 5.39M | 96.3% | $5.58 | 3 | 1.8M | $1.86 |
| stage H1 | 1 | 8.47M | 97.5% | $7.30 | 2 | 4.23M | $3.65 |
| 2026-08 | 2 | 13.9M | 97.0% | $12.88 | 5 | 2.77M | $2.58 |

_Where the money goes: agent $12.88 (100%) · gate $0.00 (0%) · blended $0.93/M tokens._

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
branch: feat/agent-probe
working tree: M AGENT-PROBE-TRACKER.md
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

## Last gate run

probe-fast:OK · probe-full:OK

## Last session result

> SESSION-RESULT: Stage H1 closed — H1.1 and H1.2 both claimed DONE with evidence (`results/h1.1-dryrun-eShop.txt`, `results/h1.1-resume-and-refusals.txt`, `results/h1.2-three-arm-smoke.md`), commits 2d20636 / e2f7372 / 5f95609 / f9982e2, working tree clean, `verify.mjs --tier fast` GREEN with zero notes. Nothing is red. The one thing that was red — arm isolation — was found by exercising the harness rather than reasoning about it, fixed, gated, and the affected runs voided and re-run in all three arms; total probe spend this session was $4.59, of which $2.82 is the voided batch. Next is P1: P1.1 is mostly a reading task because the isolation evidence is already in the recorded rows and transc…

## Tracker handoff

```
last: H1 CLOSED — H1.1 + H1.2 DONE. `run-probe.mjs` drives arms G/M/B, resumable, $1.50 cap,
  refuses >60 runs/invocation. Commits 2d20636, e2f7372, 5f95609. Gate `--tier fast` GREEN and
  `total_cost_usd` is non-zero ($0.44–$0.66/run), so DESIGN §4.1's zero-cost trap is settled.
stage: **H1 complete, attempt 1, no reds. P1 is next.**
trap: **`--allowedTools` does NOT restrict — it only auto-approves.** The first three real runs
  proved it: arm G executed Bash, arm M executed a subagent that read files with cat/ls. Those
  runs are void, kept with the reason in `results/void/`, and were re-run in ALL THREE arms. Arms
  are now exhaustive `--disallowedTools` lists; every row carries offeredOutsideArm /
  calledOutsideArm / isolationOk and `verify.mjs` fails on any row recording a breach.
next: **P1.1 is mostly reading** — the isolation evidence is already in the three recorded rows
  and `results/raw/eShop/*.stream.jsonl`. P1.2 needs two trivial-prompt runs; turn-1 tokens are
  in `usage.iterations[0]`. P2 goes through `conductor bg` and skips the 3 eshop-a1 rep1 cells.
escalation: `--bare` cannot authenticate here (no ANTHROPIC_API_KEY; bare never reads OAuth), so
  runs need `--allow-no-bare` and record isolation:"no-settings-fallback". An owner key restores
  DESIGN §6.3 verbatim; the ambient audit found no CLAUDE.md anywhere in the parent chain.
```
