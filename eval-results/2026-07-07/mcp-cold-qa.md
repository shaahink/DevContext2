# MCP Cold-Agent QA — Baseline (Loom L0.2)

**Repo:** `C:/Users/shahi/source/repos/run-aspnetcore-microservices/src`  
**Date:** 2026-07-07  
**Tools:** 22 (`analyze, close_session, config, entrypoints, find, get_context, impact, insights, interesting_points, list_sessions, map, neighbors, node, overview, read_source, resolve, stats, status, tests_for, top_flows, trace, usages`)  
**tools/list envelope:** ~1227 tok (L5 target ≤1.5k)  

A naive agent with zero prior knowledge drives the tools: it calls before
analyzing, guesses natural-language focuses, invents symbol names, renames and
omits parameters, and calls a tool that does not exist. Each failing response is
scored on (a) does it say what was wrong, and (b) does it give a copyable next step.

## Actionability

**Actionable failures: 0/12 (0%)**  
False-successes (silent wrong answers): 5  
Opaque errors (no next step): 5  
Gate (L5.5 ≥90%): BELOW — baseline, gate arms in L5.5  

## Per-probe

| Probe | Phase | Intent | Verdict | Signaled | NextStep | Response (truncated) |
|-------|-------|--------|---------|----------|----------|----------------------|
| A1-overview-no-handle | A | overview before analyze | opaque error (no next step) | true | false | {"text":"An error occurred invoking 'overview'."} |
| A2-trace-no-handle | A | trace before analyze | opaque error (no next step) | true | false | {"text":"An error occurred invoking 'trace'."} |
| A3-resolve-no-handle | A | resolve before analyze | opaque error (no next step) | true | false | {"text":"An error occurred invoking 'resolve'."} |
| B1-nonexistent-tool | B | call a tool that does not exist | signaled, no next step | true | false | mcpError: Unknown tool: 'flow' |
| B2-trace-nl-focus | B | natural-language focus | false-success (silent wrong answer) | false | false | {"found":false,"focus":"how does checkout work"} |
| B3-impact-madeup | B | impact of a symbol that does not exist | false-success (silent wrong answer) | false | false | {"direction":"up","totalAffected":0,"resultsByService":{}} |
| B4-usages-shortname | B | usages by short name (not a nodeId) | false-success (silent wrong answer) | false | false | {"nodeId":"IBasketRepository","count":0,"usages":[]} |
| B5-config-exact-key | B | config filtered by a plausible key | false-success (silent wrong answer) | false | false | {"key":"ConnectionStrings","totalKeys":0,"keys":{}} |
| B6-getcontext-nl-focus | B | get_context with NL focus | false-success (silent wrong answer) | false | false | {"found":false,"focus":"basket checkout"} |
| B7-resolve-missing-required | B | omit the required query param | opaque error (no next step) | true | false | {"text":"An error occurred invoking 'resolve'."} |
| B8-trace-wrong-param-name | B | guess param name 'route' not 'focus' | opaque error (no next step) | true | false | {"text":"An error occurred invoking 'trace'."} |
| B9-find-noise-query | B | find a common word, expect ranked not noise | unactionable | false | false | {"query":"Order","cursor":0,"limit":20,"count":20,"total":40,"hasMore":true,"results":[{"n |

## Owning stages for the red items
- Missing-session default / handle ergonomics → **L5.1**
- Error envelope `{error, hint, example}`, unknown-tool → tool list, schema on bad params → **L5.2**
- Unified ranked resolution (`resolve/find/usages/impact` short-name + did-you-mean) → **L5.3**
- Real `flow` tool + fuzzy focus suggestions → **L5.4**
- This harness becomes the enforced gate → **L5.5**
