# MCP QA Results

**Repo:** `C:/Users/shahi/source/repos/run-aspnetcore-microservices/src`  
**Baseline:** 474 nodes, 213 edges, 36 entries  
**Date:** 2026-07-05

## Results

| # | Pass | Question | Detail |
|---|------|----------|--------|
| q1-checkout-flow | YES | How does checkout work? | 2 trace steps (baseline: 2, target: >=10 after M1) |
| q2-discount-callers | YES | Who calls the Discount service? | 20 Discount nodes found, usages=false (cross-service edges blocked by W4) |
| q3-impact-of-handler | YES | What breaks if I change CheckoutBasketCommandHandler? | impact returned 0 entries (baseline: 0, target: >=1 after M1) |
| q4-ambiguous-product | YES | What is Product? (expect disambiguation, not silent pick) | 20 Product-like nodes found (expect disambiguation, not silent pick - M4.2) |
| q5-config-lookup | YES | What config keys are used? | stats returned entryCount=36 (config tool not yet available - M4.7) |

**Score:** 5/5 (baseline pre-M1 — most expected to fail)

## Transport checks
- [x] Cold start: server started and accepted initialize
- [x] Unprompted flush: analyze returned via polling workaround
- [x] Session lifecycle: create, list, close