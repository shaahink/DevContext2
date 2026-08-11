# DevContext agent probe - does the MCP help an agent browse code Phase Tracker

**Plan:** DevContext agent probe - does the MCP help an agent browse code | **Branch:** `feat/agent-probe` | **Design doc:** eval/agent-probe/DESIGN.md

## Handoff (overwrite this block, ≤ 12 lines, no history)

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


## Baseline numbers (from run.db)

| Metric | Value |
|---|---|
| Total checkpoints | 12 |
| Done | 7 |
| Claimed (unconfirmed) | 1 |

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
