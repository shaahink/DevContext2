# T3 (MCP v3) — delivery summary (2026-07-16)

Branch `feat/tapestry-t3` off develop @ 94a29db. Goal: an agent's first hour costs ~10× fewer tokens;
one addressing model; token-bounded defaults; CLI/report honesty. All 8 checkpoints landed, each with a
pin (R-T1) and gates green.

| # | Checkpoint | Commit | Pin |
|---|-----------|--------|-----|
| T3.1 | Unified symbol addressing — every symbol tool accepts fuzzy `query`; ambiguity-honest; ≤80-tok envelope | dbc217e | McpQa q8 |
| T3.2 | `entrypoints` summary default (byKind + top-15, `full:true`) — 843 tok on dogfood | dbc217e | McpQa q9 |
| T3.6 | Self-describing `method` note on tests_for/config; flow-vs-trace docs | dbc217e | McpQa q10 |
| T3.5 | Start-here noise filter (System.*/BCL/Store); repo-relative pack paths | f8e76ca | GraphQueryTests |
| T3.7 | CLI `query entrypoints\|stats\|trace` parity (kernel JSON) | afeb0f3 | gates Step 4b + cli-query-ops.txt |
| T3.4 | config latency — scan cached once per session (10.5s → ≤500ms warm) | bd66486 (+ Roslyn fix) | ConfigScannerTests + McpQa q6 |
| T3.3 | trace `budgetTokens` (~4k default, named per-subtree omissions, deep-link) | 31c64b2 | TraceBuilderTests + McpQa q11 |
| T3.8 | Report hygiene — surface cap, telemetry behind --stats, repo-derived footer | 9bdce29 | LibrarySurfaceRendererTests + goldens |

## Headline numbers
- **MCP QA 12/12 (100%)** on the dogfood repo (q1–q11 + checkout gate); run.js now exits non-zero below
  90% actionable — a real regression ratchet (`McpQaGateTests`).
- **entrypoints** summary 843 tok (was ~10k on a 128-entry repo); `full:true` → all.
- **trace budget 400** → 11 steps / 323 tok with 19 named omissions, vs full 46 steps / 1467 tok.
- **MassTransit report 476 KB → 34.5 KB** (< 40 KB gate); PUBLIC SURFACE capped 25 ns × 12 types.
- **CLI** entrypoints/stats/trace return graph JSON (were the overview render); trace honors --focus.
- Contract preserved: additive params only, nothing renamed (the 23-tool surface is intact).

## Proto change (T3.3)
`TraceRequest.budget_tokens` added; C# Contracts regenerate on build; app TS regenerated with `buf`
(the regen also synced pre-existing stale ArchetypeView/ArchetypeEntryGroup/ArchetypeEntryRow messages).
`pnpm check` (lint + vitest + ng build): exit 0.

## Delivery-drive fix — ConfigScanner regex funeral (T3.4 follow-up)
The delivery loom-guards run caught the real reason the gate was still pending: T3.4 extracted the
config-key scanner into `Core/Graph/ConfigScanner.cs` and carried its `System.Text.RegularExpressions`
import with it — a banned pattern in `Core/Graph` (L2.3 regex funeral; body-scan regexes were retired
in favour of Roslyn). Reformed the scanner to a **Roslyn syntax walk** (`CSharpSyntaxTree.ParseText` +
`DescendantNodes`, the idiom already in `GraphBuilder.Seams.cs`/`TraceBuilder.cs`) — no relocation to
dodge the guard. Result:
- **loom-guards PASS** (regex import gone; truth gate green).
- **Output parity** — dogfood config still returns **4 keys** (McpQa q6 identical; fresh 12/12 run).
  The syntax walk is strictly more precise: it can never mistake a key inside a comment or a
  non-literal argument for a real binding.
- **Faster cold** — parse+walk over all 1737 shamshir `.cs` files (a strict upper bound; the real scan
  only touches node-bearing files) = **3.9s**, vs the old regex's 10.5s. Warm is a cached in-memory
  filter (scanner-independent), so the ≤500ms warm bar holds by construction.
- `ConfigScannerTests` (2/2) unchanged — the `ConfigBindingInfo` contract is preserved.

## Delivery gate
See `gates-t3-delivery.txt` — fresh run on the fixed code: **GATE: PASS** (build / fast / MCP QA 12/12 /
eval 58P·6S·0F / CLI matrix + Step 4b) + loom-guards PASS.

## Deferred to later stages
- shamshir entrypoints≤1.5k / trace≤4k defaults (measurement; the small-repo QA harness's 45s per-call
  timeout can't drive shamshir's analyze — a pre-existing run.js limitation, not a T3 gap).
- Pack identity header + full repo-relative locations = T4.1 (T3.5 did the pack Location line).
