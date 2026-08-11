# DevContext agent probe - does the MCP help an agent browse code Phase Tracker

**Plan:** DevContext agent probe - does the MCP help an agent browse code | **Branch:** `feat/agent-probe` | **Design doc:** eval/agent-probe/DESIGN.md

## Handoff (overwrite this block, ≤12 lines, no history)

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


## Baseline numbers (from run.db)

| Metric | Value |
|---|---|
| Total checkpoints | 12 |
| Done | 5 |
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
| P1.1 | Arm isolation proven from recorded transcripts: arm G made 0 mcp calls, arm M made 0 Read/Grep/Glob calls, cost is non-zero on every run, and analyze reported cached true for every arm | DONE | 53ca6c5 | eval/agent-probe/results/p1.1-preflight-audit.md |
| P1.2 | Tool-schema tax measured — the turn-1 input + cache-creation token delta between arm G and arm B on an identical trivial prompt, recorded as an absolute count and as a share of median run cost | DONE | 53ca6c5 | eval/agent-probe/results/p1.2-tool-schema-tax.md |

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
