# MCP QA Results (M4 post-gate)

**Repo:** `C:/Users/shahi/source/repos/run-aspnetcore-microservices/src`  
**Baseline:** 432 nodes, 330 edges, 34 entries  
**Date:** 2026-07-14

## Results

| # | Pass | Calls | Tokens | Question | Detail |
|---|------|-------|--------|----------|--------|
| q1-overview | YES | 1 | 224 | What is this repo? (one-call repo brief) | overview 224 tok, archetype=true flows=true counts=true services=true |
| q2-checkout-flow | YES | 1 | 1324 | How does checkout work? | trace found: 43 steps, cross-service, 1324 tok |
| q3-discount-callers | YES | 2 | 704 | Who calls the Discount service? | 10 Discount matches, usages=true |
| q4-impact-of-handler | YES | 3 | 422 | What breaks if I change CheckoutBasketCommandHandler? | impact up=5 down=0 total=5 |
| q5-ambiguous-product | YES | 1 | 585 | What is Product? (disambiguation check) | resolve returned 10 candidates, ambiguous=true, hint=yes |
| q6-config-lookup | YES | 1 | 229 | What config keys are used? | config returned 4 keys |
| q7-tests-for | YES | 2 | 140 | What tests cover CheckoutBasketCommandHandler? | tests_for found 0 tests (best-effort), node=CheckoutBasketCommandHandler |
| gate-checkout | YES | 2 | 1548 | Checkout gate: answer in <=3 calls, <=2k tokens | 2 calls, 1548 tok, found=true, 43 steps, cross-service=true |

**Score:** 8/8  
**Checkout gate (<=3c/2ktok):** PASS  

## Transport checks
- [x] Cold start: server started and accepted initialize
- [x] Unprompted flush: analyze returned via polling workaround
- [x] Session lifecycle: create, list, close

## Tool coverage
Available tools (23): analyze, close_session, config, entrypoints, find, flow, get_context, impact, insights, interesting_points, list_sessions, map, neighbors, node, overview, read_source, resolve, stats, status, tests_for, top_flows, trace, usages

M4 tools covered: 9/9 (overview, resolve, trace, impact, read_source, find, config, get_context, tests_for)