# Conductor — DevContext agent probe - does the MCP help an agent browse code run report

_Updated 2026-08-11 00:00 UTC · branch `feat/agent-probe` · HEAD `d4386cd`_

**Status:** Idle
**Stage:** P1 — Smoke - prove arm isolation and cost accounting before spending · attempts used 0
**Checkpoints:** 7/12 done · **Sessions run:** 3 · **Cost:** $19.6980 (agent $19.6956 + gates $0.0024) · **Tokens:** 401,808 in / 199,223 out
**Confirmed phases:** K1, H1, P1

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| K1 | Ground truth - pre-registered question sets | ██████████ 3/3 | confirmed ✓ |
| H1 | Probe harness - three-arm runner | ██████████ 2/2 | confirmed ✓ |
| P1 | Smoke - prove arm isolation and cost accounting before spending | ██████████ 2/2 | confirmed ✓ |
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

<details> ✅<summary>P1 — Smoke - prove arm isolation and cost accounting before spending (2/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| P1.1 | Arm isolation proven from recorded transcripts: arm G made 0 mcp calls, arm M made 0 Read/Grep/Glob calls, cost is non-zero on every run, and analyze reported cached true for every arm | ✅ DONE | [`53ca6c5`](https://github.com/shaahink/DevContext2/commit/53ca6c5) |
| P1.2 | Tool-schema tax measured — the turn-1 input + cache-creation token delta between arm G and arm B on an identical trivial prompt, recorded as an absolute count and as a share of median run cost | ✅ DONE | [`53ca6c5`](https://github.com/shaahink/DevContext2/commit/53ca6c5) |

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
| 3 | P1 | Deliver | 1 | 08-10 23:40 | 0:18 | Advanced | P1.1 P1.2 | 4 | probe-fast:OK | $6.8159 | $0.0013 | 122,889/64,955 |

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

## Last gate run

probe-fast:OK · probe-full:OK

## Last session result

> SESSION-RESULT: Stage P1 closed clean — P1.1 and P1.2 both DONE with evidence (`results/p1.1-preflight-audit.md`, `results/p1.2-tool-schema-tax.md`), four commits `53ca6c5`/`aeaebfd`/`65b79b0`/`97fc7e8` pushed to `origin/feat/agent-probe`, tree clean, `verify.mjs --tier fast` GREEN. Arm isolation holds under an independent re-derivation, cost accounting is exact to the fraction of a cent, and the tool-schema tax is 2540 tokens (4.0% of a median run) with the caveat that DESIGN §4.4's statistic only measures it on a cold prefix. Nothing is red. Next is P2: `conductor bg start --purpose probe -- node eval/agent-probe/run-probe.mjs --repo eShop --reps 3 --allow-no-bare`, which skips the three r…

## Tracker handoff

```
last: **P1 CLOSED** — P1.1 + P1.2 DONE, commits 53ca6c5, aeaebfd, 65b79b0. `audit-preflight.mjs`
  re-derives DESIGN §8 assertions 1–4 from `results/raw/**` sharing no code with the harness:
  all four hold on all 3 runs. `verify.mjs --tier fast` GREEN and now runs that audit too.
stage: **P1 complete, attempt 1, no reds. P2 (54-cell pilot) is next.**
trap: **DESIGN §4.4's tax statistic is cache-dependent and read 9 tokens.** The prompt cache is
  server-side and prefix-keyed, so the schemas came back as `cache_read`, not `cache_creation`.
  Measured invariantly (turn-1 in+create+read) the tax is **2540 tokens / $0.0254 cold / 4.0% of
  a median run**; cold cross-check off the recorded runs gives 2531. Same trap bites P2: 54
  back-to-back cells share a warm prefix, so pilot costs carry an amortised tax that flatters B.
next: P2 = `conductor bg start --purpose probe -- node eval/agent-probe/run-probe.mjs --repo eShop
  --reps 3 --allow-no-bare`. It skips the 3 recorded eshop-a1 rep1 cells, so 51 runs, ~$32 at the
  observed $0.44–$0.66. Ceiling is 60/invocation, cap $1.50/run; resumable, never restart it.
escalation: `--bare` cannot authenticate here (no ANTHROPIC_API_KEY; bare never reads OAuth), so
  runs need `--allow-no-bare` and record isolation:"no-settings-fallback". Cost is exact but only
  at a **2x** cache-write rate (1h TTL), not DESIGN §4.1's 1.25x — matters only if cost ever hits 0.
```
