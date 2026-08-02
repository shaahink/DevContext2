# MCP QA Results (M4 post-gate)

**Repo:** `C:/Users/shahi/source/repos/run-aspnetcore-microservices/src`  
**Baseline:** 426 nodes, 331 edges, 34 entries  
**Date:** 2026-07-27

## Results

| # | Pass | Calls | Tokens | Question | Detail |
|---|------|-------|--------|----------|--------|
| q1-overview | YES | 1 | 224 | What is this repo? (one-call repo brief) | overview 224 tok, archetype=true flows=true counts=true services=true |
| q2-checkout-flow | YES | 1 | 1477 | How does checkout work? | trace found: 46 steps, cross-service, 1477 tok |
| q3-discount-callers | YES | 2 | 699 | Who calls the Discount service? | 10 Discount matches, usages=true |
| q4-impact-of-handler | YES | 3 | 1145 | What breaks if I change CheckoutBasketCommandHandler? | impact up=5 down=13 total=18 |
| q5-ambiguous-product | YES | 1 | 578 | What is Product? (disambiguation check) | resolve returned 10 candidates, ambiguous=true, hint=yes |
| q6-config-lookup | YES | 1 | 297 | What config keys are used? | config returned 4 keys |
| q7-tests-for | YES | 2 | 175 | What tests cover CheckoutBasketCommandHandler? | tests_for found 0 tests (best-effort), node=CheckoutBasketCommandHandler |
| q8-query-addressing | YES | 3 | 698 | Address impact/node/read_source by fuzzy query (not nodeId)? | query→ impact=true node=true read_source=true |
| q9-entrypoints-summary | YES | 2 | 3201 | List entry points as a bounded summary (not a token wall)? | byKind=true, showing 15/34 (848tok), full→34 |
| q10-self-describing | YES | 3 | 472 | Do best-effort tools explain their method (what 0 means)? | method note: tests_for=true, config=true |
| q11-trace-budget | YES | 2 | 1800 | Does trace respect a token budget with named omissions? | budget400: 11 steps/323tok omitted=19; full: 46 steps/1477tok |
| gate-checkout | YES | 2 | 1701 | Checkout gate: answer in <=3 calls, <=2k tokens | 2 calls, 1701 tok, found=true, 46 steps, cross-service=true |

**Score:** 12/12  
**Checkout gate (<=3c/2ktok):** PASS  

## Transport checks
- [x] Cold start: server started and accepted initialize
- [x] Unprompted flush: analyze returned via polling workaround
- [x] Session lifecycle: create, list, close

## Tool coverage
Available tools (24): analyze, close_session, config, entrypoints, find, flow, get_context, impact, insights, interesting_points, list_sessions, map, neighbors, node, overview, read_source, resolve, stats, status, tests_for, top_flows, trace, usages, verify_context

M4 tools covered: 9/9 (overview, resolve, trace, impact, read_source, find, config, get_context, tests_for)