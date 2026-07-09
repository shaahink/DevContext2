# MCP Cold-Agent QA — Baseline (Loom L0.2)

**Repo:** `C:/Users/shahi/source/repos/run-aspnetcore-microservices/src`  
**Date:** 2026-07-09  
**Tools:** 23 (`analyze, close_session, config, entrypoints, find, flow, get_context, impact, insights, interesting_points, list_sessions, map, neighbors, node, overview, read_source, resolve, stats, status, tests_for, top_flows, trace, usages`)  
**tools/list envelope:** ~1337 tok (L5 target ≤1.5k)  

A naive agent with zero prior knowledge drives the tools: it calls before
analyzing, guesses natural-language focuses, invents symbol names, renames and
omits parameters, and calls a tool that does not exist. Each failing response is
scored on (a) does it say what was wrong, and (b) does it give a copyable next step.

## Actionability

**Failure-actionability probes:** 11 (rank-quality probes excluded)  
**Actionable failures: 9/11 (82%)**  
False-successes (silent wrong answers): 0  
Opaque errors (no next step): 0  
Gate (L5.5 ≥90%): BELOW — baseline, gate arms in L5.5  
Rank-quality probes (excluded from failure denominator): B9-find-noise-query  
rank-quality (B9 find "Order"): top="Order" aggregate#1=true results=20  

## Per-probe

| Probe | Phase | Intent | Type | Verdict | Signaled | NextStep | Response (truncated) |
|-------|-------|--------|------|---------|----------|----------|----------------------|
| A1-overview-no-handle | A | overview before analyze | failure | unactionable | false | false | {"handle":"828cf781f2e44bab9a7f922f38761927","tokens":199,"text":"App: Discount.Grpc (gRPC |
| A2-trace-no-handle | A | trace before analyze | failure | actionable | true | true | {"error":"No entry or node matched 'checkout'.","hint":"Did you mean one of these? Use an  |
| A3-resolve-no-handle | A | resolve before analyze | failure | actionable | true | true | {"query":"Order","count":10,"ambiguous":true,"candidates":[{"nodeId":"Type:Ordering.Domain |
| B1-nonexistent-tool | B | call a tool that does not exist | failure | actionable | true | true | {"error":"Unknown tool 'dependencies'.","hint":"Use one of availableTools.","example":"ove |
| B2-trace-nl-focus | B | natural-language focus | failure | actionable | true | true | {"error":"No entry or node matched 'how does checkout work'.","hint":"Did you mean one of  |
| B3-impact-madeup | B | impact of a symbol that does not exist | failure | actionable | true | true | {"error":"Symbol 'TotallyMadeUpType' not found — not the same as zero impact.","hint":"Use |
| B4-usages-shortname | B | usages by short name (not a nodeId) | failure | unactionable | false | false | {"nodeId":"Type:Basket.API.Data.IBasketRepository","count":7,"resolvedFrom":"IBasketReposi |
| B5-config-exact-key | B | config filtered by a plausible key | failure | actionable | true | true | {"error":"No config key exactly 'ConnectionStrings' (4 keys exist).","hint":"Keys are exac |
| B6-getcontext-nl-focus | B | get_context with NL focus | failure | actionable | true | true | {"error":"No context could be built for 'basket checkout'.","hint":"Did you mean one of th |
| B7-resolve-missing-required | B | omit the required query param | failure | actionable | true | true | {"error":"Missing required parameter 'query'.","hint":"Pass the symbol/route/file to resol |
| B8-trace-wrong-param-name | B | guess param name 'route' not 'focus' | failure | actionable | true | true | {"error":"Missing required parameter 'focus'.","hint":"Pass an entry route or symbol. The  |
| B9-find-noise-query | B | find a common word, expect ranked not noise | rank-quality | unactionable | false | false | {"query":"Order","cursor":0,"limit":20,"count":20,"total":40,"hasMore":true,"results":[{"n |

## Owning stages for the red items
- Missing-session default / handle ergonomics → **L5.1**
- Error envelope `{error, hint, example}`, unknown-tool → tool list, schema on bad params → **L5.2**
- Unified ranked resolution (`resolve/find/usages/impact` short-name + did-you-mean) → **L5.3**
- Real `flow` tool + fuzzy focus suggestions → **L5.4**
- This harness becomes the enforced gate → **L5.5**
