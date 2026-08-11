# DevContext agent probe - does the MCP help an agent browse code Phase Tracker

**Plan:** DevContext agent probe - does the MCP help an agent browse code | **Branch:** `feat/agent-probe` | **Design doc:** eval/agent-probe/DESIGN.md

## Handoff (overwrite this block, ≤12 lines, no history)

done: **A1.2** (judge 54/54 + paired analysis) - `results/a1.2-analysis.md`. **THE PILOT ANSWER
  IS A NULL.** Cost B vs G: median log2 **+0.120** (B costs **1.087x** G, i.e. slightly MORE),
  95% CI [-0.423, +0.346]; accelerator needs upper < -0.32. Accuracy G 18/18 = B 18/18, **zero
  discordant pairs**, McNemar p=1.0. Judge: opus-5, effort high, zero tools, 54/54 parsed, $3.48.
trap for R1.1: DESIGN 5's **Regression branch fires mechanically** (90% lower -0.131 vs -0.05 bar)
  and reporting it as a regression would be WRONG - at 18 pairs the tightest interval is +/-0.131
  even at a perfect tie, so the bar is unreachable at pilot n whatever the answers were.
  `analyse.mjs` section 6 prints that caveat itself; quote it, do not re-derive it.
dont bury: **arm M is the sharpest result** - 12/18 (66.7%) vs G 18/18 at **1.508x** cost
  (Wilcoxon p=0.0313, the n=6 minimum), total failures on `eshop-c1` (class C, impact) 0/3 and
  `eshop-f1` (class F control) 0/3. Arm B's median mcp share is **0.015** - B barely uses the MCP,
  which is why B tracks G. That and P2's manipulation-check failure are one fact seen twice.
next: **R1.1** (write `eval-results/agent-probe/RESULTS.md`), then R1.2 (20% human sample - the
  owner grades it; do not grade it yourself and do not report a kappa you did not compute).
green: `verify.mjs --tier fast` GREEN. Blindness shown not asserted: `results/a1.2-leak-scan.md`,
  54 prompts, 0 residual hits from an independently-written superset scanner.


## Baseline numbers (from run.db)

| Metric | Value |
|---|---|
| Total checkpoints | 12 |
| Done | 7 |
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
| H1.1 | `run-probe.mjs` drives all three arms as headless subprocesses, is resumable from `runs.jsonl`, caps each run at $1.50, and refuses more than 60 runs per invocation | DONE ✓ | 2d20636 | eval/agent-probe/results/h1.1-dryrun-eShop.txt |
| H1.2 | One real end-to-end run per arm is recorded in `results/runs.jsonl` with answer, toolCalls, costUsd, usage, numTurns, durationMs — and the raw result JSON plus transcript are saved under `results/raw` | DONE ✓ | 2d20636 | eval/agent-probe/results/h1.2-three-arm-smoke.md |

### P1 — Smoke - prove arm isolation and cost accounting before spending

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| P1.1 | Arm isolation proven from recorded transcripts: arm G made 0 mcp calls, arm M made 0 Read/Grep/Glob calls, cost is non-zero on every run, and analyze reported cached true for every arm | DONE ✓ | 53ca6c5 | eval/agent-probe/results/p1.1-preflight-audit.md |
| P1.2 | Tool-schema tax measured — the turn-1 input + cache-creation token delta between arm G and arm B on an identical trivial prompt, recorded as an absolute count and as a share of median run cost | DONE ✓ | 53ca6c5 | eval/agent-probe/results/p1.2-tool-schema-tax.md |

### P2 — Pilot - six eShop questions, three arms, three repetitions

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| P2.1 | 54 eShop runs recorded (6 questions x 3 arms x 3 reps), question order randomised, censored runs kept and flagged, per-arm censoring rate reported | DONE | 8807f48 | eval/agent-probe/results/p2.1-pilot.md |

### A1 — Grade and analyse

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| A1.1 | Deterministic grading pass complete: mustMention hits, mustNotMention violations, expectedVerdict match, citation resolution — scored per run into `results/graded.jsonl` | DONE | c366338 | eval/agent-probe/results/a1.1-grading.md |
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
