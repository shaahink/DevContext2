# DevContext agent probe - does the MCP help an agent browse code Phase Tracker

**Plan:** DevContext agent probe - does the MCP help an agent browse code | **Branch:** `feat/agent-probe` | **Design doc:** eval/agent-probe/DESIGN.md

## Handoff (overwrite this block, ≤12 lines, no history)

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


## Baseline numbers (from run.db)

| Metric | Value |
|---|---|
| Total checkpoints | 12 |
| Done | 3 |
| Claimed (unconfirmed) | 2 |

## Checkpoints

Status ∈ TODO · IN PROGRESS · DONE · DONE ✓ (confirmed) · BLOCKED · SKIPPED. Evidence = artifact path produced by a run this
phase (a code path is not evidence). Agent claims are marked DONE; engine confirms as DONE ✓.

### K1 — Ground truth - pre-registered question sets

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| K1.1 | `questions/eShop.json` exists with 6 questions covering classes A B C D E F; every key symbol resolves in eval-repos/eShop at 9b4f9434; class D is a real sibling-attribution trap and class E has an empty mustMention | DONE ✓ | e210f51 | eval/agent-probe/questions/eShop.json |
| K1.2 | `questions/TodoApi.json` exists, same shape, keys verified at 307a1ead | DONE ✓ | e210f51 | eval/agent-probe/questions/TodoApi.json |
| K1.3 | `questions/FluentValidation.json` exists, same shape, keys verified at 94397908 | DONE ✓ | e210f51 | eval/agent-probe/questions/FluentValidation.json |

### H1 — Probe harness - three-arm runner

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| H1.1 | `run-probe.mjs` drives all three arms as headless subprocesses, is resumable from `runs.jsonl`, caps each run at $1.50, and refuses more than 60 runs per invocation | DONE | 2d20636 | eval/agent-probe/results/h1.1-dryrun-eShop.txt |
| H1.2 | One real end-to-end run per arm is recorded in `results/runs.jsonl` with answer, toolCalls, costUsd, usage, numTurns, durationMs — and the raw result JSON plus transcript are saved under `results/raw` | DONE | 2d20636 | eval/agent-probe/results/h1.2-three-arm-smoke.md |

### P1 — Smoke - prove arm isolation and cost accounting before spending

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| P1.1 | Arm isolation proven from recorded transcripts: arm G made 0 mcp calls, arm M made 0 Read/Grep/Glob calls, cost is non-zero on every run, and analyze reported cached true for every arm | TODO | - | - |
| P1.2 | Tool-schema tax measured — the turn-1 input + cache-creation token delta between arm G and arm B on an identical trivial prompt, recorded as an absolute count and as a share of median run cost | TODO | - | - |

### P2 — Pilot - six eShop questions, three arms, three repetitions

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| P2.1 | 54 eShop runs recorded (6 questions x 3 arms x 3 reps), question order randomised, censored runs kept and flagged, per-arm censoring rate reported | TODO | - | - |

### A1 — Grade and analyse

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| A1.1 | Deterministic grading pass complete: mustMention hits, mustNotMention violations, expectedVerdict match, citation resolution — scored per run into `results/graded.jsonl` | TODO | - | - |
| A1.2 | Judge pass complete on anonymised final answers only, plus the paired analysis: median log2 cost ratio with bootstrap CI, accuracy difference with CI, fabrication rate, mcp call share | TODO | - | - |

### R1 — Report and verdict

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| R1.1 | `eval-results/agent-probe/RESULTS.md` states the verdict against the four pre-registered outcomes, with the per-class breakdown and the honest pilot interval | TODO | - | - |
| R1.2 | Human-check sample (20%, stratified) written to a separate file for the owner; report names exactly what the full run needs to turn this pilot into a defensible number | TODO | - | - |

## Dependencies

```
(none — stages run sequentially by plan order)
```
