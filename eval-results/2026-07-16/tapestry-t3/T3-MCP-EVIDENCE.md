# T3 MCP ergonomics — T3.1 + T3.2 + T3.6 evidence (2026-07-16)

Branch `feat/tapestry-t3`. All three are MCP-layer only (DevContextTools.cs + run.js) — no engine
change, so the eval matrix is provably unaffected (verified: fast 483P/3S + 14P, eval untouched).

## T3.1 — Unified symbol addressing
Every symbol-taking tool now accepts a fuzzy `query` alongside its precise `nodeId`/`focus`
(additive — nothing renamed, `nodeId`/`focus` stay the precise form):
- `node`, `neighbors`, `impact`, `tests_for`, `read_source` gain `query`; resolved via the new shared
  `ResolveQueryAsync` (exact-title-first through graph.Find, **ambiguity-honest — never silently picks**;
  returns a ≤80-tok envelope with candidates on a miss/ambiguous match).
- `trace`, `flow`, `get_context`, `usages` accept `query` as a synonym for `focus`/`nodeId`.
Pin: `run.js` q8-query-addressing — `impact(query:…)`, `node(query:…)`, `read_source(query:…, mode:member)`
all resolve. **PASS** (impact=true node=true read_source=true, 3c/697tok). During bring-up an intentionally
non-exact query ("BasketService") correctly returned an ambiguity envelope — the honesty is real.

## T3.2 — entrypoints summary default
`entrypoints()` default → per-kind counts (`byKind`) + top-N (default 15) by score, lean fields, with an
`omitted` count + hint. `full:true` returns every entry with the rich fields; `kind:` filters; `top:` widens.
Pin: q9-entrypoints-summary — **PASS**: byKind present, showing 15/34, summary **843 tok** (bound ≤1500),
`full:true` → 34 (== count). On a 128-entry repo the top-15 cap holds the summary bounded (shamshir check
carried into the T3 delivery drive).

## T3.6 — self-describing heuristics
- `tests_for` response carries `method`: "Walked in-edges ≤N hops; callers classified as tests by
  name/path/project. 0 = none reached in-graph, not proof of no coverage."
- `config` response carries `method`: names the regex scan and its blind spot (dynamic keys not captured).
- flow-vs-trace documented in BOTH tool descriptions (trace = deep call spine from one entry; flow =
  compact touches/emits summary, deep-link to trace).
Pin: q10-self-describing — **PASS** (tests_for.method + config.method both present).

## Harness hardening (rides with T3.1)
`run.js` is now a real regression ratchet: exits non-zero when QA < 90% actionable or the checkout gate
fails — so `McpQaGateTests` (asserts exit 0) goes red on a regression. Matches the T3 gate
"cold QA ≥90% actionable".

## Gate
build 0w/0e · fast 483P/3S + 14P (Core) / 14P (Server) · loom-guards PASSED (0 truth failures) ·
**MCP QA 11/11 (100%), checkout gate PASS, QA≥90% ratchet PASS** (mcp-qa-t3.6.txt).
Dogfood baseline unchanged: 439 nodes / 339 edges / 34 entries.
