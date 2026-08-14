# DevContext agent probe - does the MCP help an agent browse code Phase Tracker

**Plan:** DevContext agent probe - does the MCP help an agent browse code | **Branch:** `feat/agent-probe` | **Design doc:** eval/agent-probe/DESIGN.md

## Handoff (overwrite this block, ≤12 lines, no history)

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


## Baseline numbers (from run.db)

| Metric | Value |
|---|---|
| Total checkpoints | 12 |
| Done | 8 |
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
| A1.1 | Deterministic grading pass complete: mustMention hits, mustNotMention violations, expectedVerdict match, citation resolution — scored per run into `results/graded.jsonl` | DONE ✓ | c366338 | eval/agent-probe/results/a1.1-grading.md |
| A1.2 | Judge pass complete on anonymised final answers only, plus the paired analysis: median log2 cost ratio with bootstrap CI, accuracy difference with CI, fabrication rate, mcp call share | DONE | 33aba13 | eval/agent-probe/results/a1.2-analysis.md |

### R1 — Report and verdict

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| R1.1 | `eval-results/agent-probe/RESULTS.md` states the verdict against the four pre-registered outcomes, with the per-class breakdown and the honest pilot interval | TODO | - | - |
| R1.2 | Human-check sample (20%, stratified) written to a separate file for the owner; report names exactly what the full run needs to turn this pilot into a defensible number | TODO | - | - |

## Dependencies

```
(none — stages run sequentially by plan order)
```
