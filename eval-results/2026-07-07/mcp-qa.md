# MCP QA Results (M4 post-gate)

**Repo:** `C:/Users/shahi/source/repos/run-aspnetcore-microservices/src`  
**Baseline:** 493 nodes, 316 edges, 34 entries  
**Date:** 2026-07-07

## Results

| # | Pass | Calls | Tokens | Question | Detail |
|---|------|-------|--------|----------|--------|
| q1-overview | YES | 1 | 242 | What is this repo? (one-call repo brief) | overview 242 tok, archetype=true flows=true counts=true services=true |
| q2-checkout-flow | YES | 1 | 71 | How does checkout work? | trace found: 3 steps, 71 tok |
| q3-discount-callers | YES | 1 | 639 | Who calls the Discount service? | 10 Discount candidates, no service kind (expected) |
| q4-impact-of-handler | YES | 3 | 410 | What breaks if I change CheckoutBasketCommandHandler? | impact up=0 down=5 total=5 |
| q5-ambiguous-product | YES | 1 | 677 | What is Product? (disambiguation check) | resolve returned 10 candidates, ambiguous=true, hint=yes |
| q6-config-lookup | YES | 1 | 214 | What config keys are used? | config returned 4 keys |
| q7-tests-for | YES | 2 | 151 | What tests cover CheckoutBasketCommandHandler? | tests_for found 0 tests (best-effort), node=CheckoutBasketCommandHandler.Handle |
| gate-checkout | YES | 2 | 313 | Checkout gate: answer in <=3 calls, <=2k tokens | 2 calls, 313 tok, found=true, 3 steps, cross-service=false |

**Score:** 8/8  
**Checkout gate (<=3c/2ktok):** PASS  

## Transport checks
- [x] Cold start: server started and accepted initialize
- [x] Unprompted flush: analyze returned via polling workaround
- [x] Session lifecycle: create, list, close

## Tool coverage
Available tools (22): analyze, close_session, config, entrypoints, find, get_context, impact, insights, interesting_points, list_sessions, map, neighbors, node, overview, read_source, resolve, stats, status, tests_for, top_flows, trace, usages

M4 tools covered: 9/9 (overview, resolve, trace, impact, read_source, find, config, get_context, tests_for)