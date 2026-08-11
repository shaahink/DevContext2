# Conductor — DevContext agent probe - does the MCP help an agent browse code run report

_Updated 2026-08-11 01:47 UTC · branch `feat/agent-probe` · HEAD `4c9dbb5`_

**Status:** Idle
**Stage:** P2 — Pilot - six eShop questions, three arms, three repetitions · attempts used 0
**Checkpoints:** 9/12 done · **Sessions run:** 5 · **Cost:** $33.4303 (agent $33.4261 + gates $0.0042) · **Tokens:** 681,794 in / 327,258 out
**Confirmed phases:** K1, H1, P1, P2

## Stage progress

| Stage | Title | Progress | State |
|---|---|---|---|
| K1 | Ground truth - pre-registered question sets | ██████████ 3/3 | confirmed ✓ |
| H1 | Probe harness - three-arm runner | ██████████ 2/2 | confirmed ✓ |
| P1 | Smoke - prove arm isolation and cost accounting before spending | ██████████ 2/2 | confirmed ✓ |
| P2 | Pilot - six eShop questions, three arms, three repetitions | ██████████ 1/1 | confirmed ✓ |
| A1 | Grade and analyse | █████░░░░░ 1/2 | partial |
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
| P2.1 | 54 eShop runs recorded (6 questions x 3 arms x 3 reps), question order randomised, censored runs kept and flagged, per-arm censoring rate reported | ✅ DONE | [`8807f48`](https://github.com/shaahink/DevContext2/commit/8807f48) |

</details>

<details><summary>A1 — Grade and analyse (1/2)</summary>

| # | Title | Status | Commit |
|---|---|---|---|
| A1.1 | Deterministic grading pass complete: mustMention hits, mustNotMention violations, expectedVerdict match, citation resolution — scored per run into `results/graded.jsonl` | ✅ DONE | [`c366338`](https://github.com/shaahink/DevContext2/commit/c366338) |
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
| 5 | P2 | Fix | 2 | 08-11 01:29 | 0:17 | Advanced | A1.1 | 4 | probe-fast:OK | $7.0867 | $0.0007 | 148,752/67,848 |

## Money

_What this run has cost, from its own `costs` rows. Same numbers as `conductor money`._

| scope | sessions | tokens | cache reads | cost | checkpoints | tok/ckpt | $/ckpt |
|---|---|---|---|---|---|---|---|
| **run total** | 5 | 37.8M | 97.3% | $33.43 | 9 | 4.2M | $3.71 |
| stage K1 | 1 | 5.39M | 96.3% | $5.58 | 3 | 1.8M | $1.86 |
| stage H1 | 1 | 8.47M | 97.5% | $7.30 | 2 | 4.23M | $3.65 |
| stage P1 | 1 | 8.1M | 97.7% | $6.82 | 2 | 4.05M | $3.41 |
| stage P2 | 2 | 15.8M | 97.4% | $13.73 | 2 | 7.92M | $6.87 |
| 2026-08 | 5 | 37.8M | 97.3% | $33.43 | 9 | 4.2M | $3.71 |

_Where the money goes: agent $33.43 (100%) · gate $0.00 (0%) · blended $0.88/M tokens._

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
08-11 02:29:48  • session #4 P2 → GatesRed · done P2.1 · 6 commit(s)  (1h29m44s)
08-11 02:29:49  • session #5 P2 Fix started (attempt 2/6)
08-11 02:47:22  ▪ gate probe-fast pass [session]  (6.8s)
08-11 02:47:23  • session #5 P2 → Advanced · done A1.1 · 4 commit(s)  (17m34s)
08-11 02:47:37  ▪ gate probe-fast pass [phase]  (6.3s)
08-11 02:47:37  ▪ gate probe-full pass [phase]  (6.9s)
08-11 02:47:37  ✓ checkpoint A1.1 confirmed
```

## Health

_Execution-health signals, folded from the event log (`.conductor/events.jsonl`)._

```
sessions 5 · retries 1 (20 %) · overall Ok
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

## Last gate run

probe-fast:OK · probe-full:OK

## Last session result

> SESSION-RESULT:** The gate is green and two checkpoints are banked. The escalated `A1-analyze-cached` red was real but misdiagnosed as a bar: DESIGN §8 pre-registers its assertions as running *"before every **batch**"*, and that batch-level bar passed — `run-probe.mjs`'s warm gate refused to start until a **fresh** MCP process reported `cached=true`, now committed as `results/p2.1-warm-gate.txt`. `audit-preflight.mjs`'s per-run re-derivation was P1's stricter *proxy* on top, and it had a false-positive mode: a run that calls no `analyze` performed no analysis and cannot have paid a cold cost, which is the identical reasoning arm G was already granted `n/a` under. I narrowed the proxy, not th…

## Tracker handoff

```
green: **`verify.mjs --tier fast` is GREEN** (c366338). The escalated A1 red was the *proxy*, not
  the bar: DESIGN §8 pre-registers its assertions "before every **batch**", and that bar passed —
  warm gate now committed as `results/p2.1-warm-gate.txt`. Zero-analyze runs are `n/a` (the same
  reason arm G already was); the 9 runs got MORE visible — own counted section in the audit.
deviations: **`eval/agent-probe/DEVIATIONS.md`, six entries** (de235f6) — D1 no `--bare`, D2 cap
  $1.50 not $2.00, D3 tax statistic, D4 deny-list isolation, D5 the A1 proxy, D6 build SHA moved.
  D6 came from sweeping `runs.jsonl`, not row 1: 3/54 rows carry `e2f7372`. Immaterial —
  `git diff --name-only e2f73724 8807f48e -- src/ proto/` is EMPTY and the 3 cells are one
  `eshop-a1` rep1 triple spanning G/M/B. No key, threshold or arm moved; no number changed.
done: **A1.1** (1be9129) — 54/54 graded, `results/a1.1-grading.md`. Median recall 100% all arms,
  0 trap violations, D/E/F 9/9 G · 9/9 B · 8/9 M. Unresolved citations G 14 / B 15 / **M 5**.
next: **A1.2** judge pass — launch it as a `conductor bg` child. Accuracy is at CEILING in all
  three arms, so R1.1 must say a 6-question set cannot show non-inferiority with any power.
trap: `eshop-b1` 6/7 and `eshop-c1` 4/5 in *every* run looks like a grader bug and is not —
  `IOrderRepository`/`NewOrderRequestHandlerTest` are in no answer at all. Don't touch the keys.
```
