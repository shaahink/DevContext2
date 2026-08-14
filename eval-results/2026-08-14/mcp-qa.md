# MCP QA Results (M4 post-gate)

**Repo:** `C:/Users/shahi/source/repos/run-aspnetcore-microservices/src`  
**Baseline:** 415 nodes, 340 edges, 34 entries  
**Date:** 2026-08-14

## Results

| # | Pass | Calls | Tokens | Question | Detail |
|---|------|-------|--------|----------|--------|
| q1-overview | YES | 1 | 221 | What is this repo? (one-call repo brief) | overview 221 tok, archetype=true flows=true counts=true services=true |
| q2-checkout-flow | YES | 1 | 1476 | How does checkout work? | trace found: 46 steps, cross-service, 1476 tok |
| q3-discount-callers | YES | 2 | 699 | Who calls the Discount service? | 10 Discount matches, usages=true |
| q4-impact-of-handler | YES | 3 | 1146 | What breaks if I change CheckoutBasketCommandHandler? | impact up=5 down=12 total=17 |
| q5-ambiguous-product | YES | 1 | 584 | What is Product? (disambiguation check) | resolve returned 10 candidates, ambiguous=true, hint=yes |
| q6-config-lookup | YES | 1 | 297 | What config keys are used? | config returned 4 keys |
| q7-tests-for | YES | 2 | 182 | What tests cover CheckoutBasketCommandHandler? | tests_for found 0 tests (best-effort), node=CheckoutBasketCommandHandler |
| q8-query-addressing | YES | 3 | 717 | Address impact/node/read_source by fuzzy query (not nodeId)? | query→ impact=true node=true read_source=true |
| q9-entrypoints-summary | YES | 2 | 3174 | List entry points as a bounded summary (not a token wall)? | byKind=true, showing 15/34 (848tok), full→34 |
| q10-self-describing | YES | 3 | 479 | Do best-effort tools explain their method (what 0 means)? | method note: tests_for=true, config=true |
| q11-trace-budget | YES | 2 | 1805 | Does trace respect a token budget with named omissions? | budget400: 11 steps/329tok omitted=19; full: 46 steps/1476tok |
| gate-checkout | YES | 2 | 1697 | Checkout gate: answer in <=3 calls, <=2k tokens | 2 calls, 1697 tok, found=true, 46 steps, cross-service=true |

**Score:** 12/12  
**Checkout gate (<=3c/2ktok):** PASS  

## Transport checks
- [x] Cold start: server started and accepted initialize
- [x] Analyze: the call returned its own handle in 8.4s
- [x] Session lifecycle: create, list, close

## Tool coverage
Advertised tools (14): analyze, entrypoints, find, get_context, impact, map, neighbors, overview, read_source, resolve, seam, stats, trace, usages

M4 tools on the advertised menu: 7/9 (overview, resolve, trace, impact, read_source, find, get_context)
M4 tools served as unlisted specialists: config, tests_for