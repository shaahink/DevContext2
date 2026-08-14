# A1.2 - the adoption gate: does arm B reach for the tools?

DESIGN.md section 3.1, amended 2026-08-14 (A1.1) **before this run**: arm B alone, one repo
(eShop), 18 runs, prompt and system text unchanged, against the tool surface as revised by the
trust pack. The floor is `mcp_call_share` **>= 0.2**, and it is the same floor in both branches.

## The number

| statistic | gate (n=18) | pilot (n=18) | floor | clears? |
|---|---|---|---|---|
| **median per-run share** (the decision statistic) | **0.306** | 0.015 | 0.2 | **YES** |
| pooled MCP calls / executed calls | 0.354 | 0.091 | - | reported, not decisive |
| mean per-run share | 0.335 | 0.061 | - | reported, not decisive |
| runs below the floor | 5/18 | 17/18 | - | - |
| runs that called the MCP at all | 14/18 | 9/18 | - | - |
| MCP calls / all executed tool calls | 80/226 | 22/243 | - | - |

## Per run

| # | question | rep | executed calls | mcp calls | share | cost | censored |
|---|---|---|---|---|---|---|---|
| 1 | eshop-c1 | 3 | 32 | 10 | 0.31 | $0.90 | no |
| 2 | eshop-b1 | 2 | 21 | 9 | 0.43 | $0.63 | no |
| 3 | eshop-c1 | 2 | 27 | 9 | 0.33 | $0.80 | no |
| 4 | eshop-d1 | 3 | 7 | 5 | 0.71 | $0.12 | no |
| 5 | eshop-e1 | 3 | 5 | 1 | 0.20 | $0.09 | no |
| 6 | eshop-d1 | 2 | 6 | 5 | 0.83 | $0.11 | no |
| 7 | eshop-f1 | 2 | 2 | 0 | 0.00 | $0.05 | no |
| 8 | eshop-a1 | 1 | 22 | 6 | 0.27 | $0.54 | no |
| 9 | eshop-a1 | 2 | 15 | 0 | 0.00 | $0.53 | no |
| 10 | eshop-d1 | 1 | 6 | 5 | 0.83 | $0.10 | no |
| 11 | eshop-b1 | 1 | 13 | 8 | 0.62 | $0.42 | no |
| 12 | eshop-e1 | 2 | 8 | 2 | 0.25 | $0.12 | no |
| 13 | eshop-b1 | 3 | 18 | 8 | 0.44 | $0.49 | no |
| 14 | eshop-f1 | 1 | 2 | 0 | 0.00 | $0.05 | no |
| 15 | eshop-a1 | 3 | 10 | 3 | 0.30 | $0.45 | no |
| 16 | eshop-e1 | 1 | 6 | 1 | 0.17 | $0.10 | no |
| 17 | eshop-f1 | 3 | 2 | 0 | 0.00 | $0.04 | no |
| 18 | eshop-c1 | 1 | 24 | 8 | 0.33 | $0.89 | no |

## Pre-flight (DESIGN section 8)

Assertions **2 and 3 do not apply to a B-only batch**: 2 bounds arm G's transcript and 3 bounds
arm M's, and neither arm ran. Assertion 1 (`analyze` warm, `cached: true`) is a property of the
batch's warm pass, not of a row - it is quoted from the batch log in the evidence file.

- repo pin: 9b4f9434 (assertion 5)
- DevContext build under test: e15e5769, 98b0dc96 (assertion 5)
- `total_cost_usd` non-zero: 18/18 rows (assertion 4)
- arm isolation breaches: 0 (the harness stops the batch on the first one)
- isolation mode: no-settings-fallback (pilot: no-settings-fallback)
- MCP tools offered to the agent: 14 (pilot: 22)
- censored runs: 0/18
- batch cost: $6.42 (pilot arm B: $6.50)

## The branch this fires

**>= 0.2 - PROCEED.** The manipulation took: with a curated, described surface an agent
does reach for the tools unprompted. The full study runs as specified and arm B stays the
primary treatment arm. The B-vs-G contrast is a test of the MCP.

## Provenance

- rows: `eval/agent-probe/results/a1.2-adoption-gate/runs.jsonl` (18), transcripts under `raw/`.
- pilot comparison: `eval/agent-probe/results/runs.jsonl` arm B (18), the 22-tool undescribed surface.
- regenerate: `node eval/agent-probe/adoption-gate.mjs --dir a1.2-adoption-gate`.
- estimator: median of per-run (mcp calls / executed tool calls), `analyse.mjs` L30/L401 -
  the same statistic that produced the pilot's published 0.015.
