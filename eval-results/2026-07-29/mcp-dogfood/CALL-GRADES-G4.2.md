# G4.2 call grades — Tasks 2 and 3 (38 calls)

Grade per R4 §2: **HELPED** (an answer came from it) / **NEUTRAL** (redundant with a cheaper call) /
**HURT** (wrong, misleading, or wasted budget). Every grade is checkable against
`task2/raw/NNN-*.json` and `task3/raw/NNN-*.json`.

## Task 2 — Hangfire (21 calls · 14,424 tok · 8,955 ms)

| # | tool | args | tok | grade | why |
|---|---|---|---|---|---|
| 1 | analyze | Hangfire | 170 | HELPED | handle + G1.4's honest envelope (`cached:false`, archetype, gitHead) |
| 2 | find | `IServerFilter` | 48 | HELPED | **F1** namespace `Hangfire.Server` |
| 3 | find | `Filter` kind:Type | 777 | HELPED | the whole filter family; surfaced `JobFilterAttribute` (**F3** candidate) |
| 4 | node | IServerFilter | 64 | HELPED | **F1** file:line — `find` returns no path, so this call is mandatory |
| 5 | get_context | IServerFilter | 578 | HELPED | **F2** the interface body verbatim, both signatures |
| 6 | usages | query:IServerFilter | 105 | **HURT** | envelope recommends `usages(nodeId:…)` — the call that returns the same nothing |
| 7 | node | JobFilterAttribute | 68 | HELPED | **F3** file:line |
| 8 | usages | nodeId:IServerFilter | 18 | **HURT** | took the tool's own advice → `{count:0}`. §3 bullet 3, measured |
| 9 | find | `ServerFilter` kind:Type | 51 | NEUTRAL | only the interface — proves name search cannot find implementors, but yields no fact |
| 10 | get_context | PerformingContext | 724 | HELPED | **F6** — the `usage` section named three implementors with file:line |
| 11 | get_context | PerformedContext | 923 | HELPED | corroborates **F4**/**F6** on the after-hook |
| 12 | get_context | PerformContext @3000 | 1,634 | **HURT** | elided the body (`… (+75 lines)`) *and* asserted completeness → **bug #9** |
| 13 | find | `Log` kind:Type | 601 | HELPED | **F5** — `Hangfire.Logging.{ILog,LogProvider,LogExtensions}` |
| 14 | get_context | LogProvider | 986 | HELPED | **F5** LibLog location + `GetLogger` |
| 15 | get_context | PerformContext @20000 | 4,188 | HELPED | the body at last — and the probe that proved #9 (29% of the task's bill) |
| 16 | read_source | PerformContext mode:"full" | 301 | **HURT** | invalid mode silently defaulted → 20 of 147 lines, stopping short of `Items` → **bug #10** |
| 17 | read_source | DisableConcurrentExecutionAttribute | 838 | HELPED | **F3 + F4 + F6 in one call** — best call of the drive |
| 18 | read_source | AutomaticRetryAttribute | 651 | HELPED | **F5** `LogProvider.For<T>()`, the exact convention |
| 19 | read_source | PerformContext mode:"member" | 1,319 | HELPED | **F4** `public IDictionary<string,object> Items { get; }`; also the legal-mode control for #10 |
| 20 | find | `InfoFormat` | 51 | HELPED | **F5** tail — the logging verb is a real member |
| 21 | read_source | LogExtensions | 329 | NEUTRAL | window landed on `LogLevel`/`Is*Enabled`, not `InfoFormat`; call 20 had settled it |

**HELPED 15 · NEUTRAL 2 · HURT 4.**

## Task 3 — DevContext on itself (17 calls · 13,528 tok · 27,100 ms)

| # | tool | args | tok | grade | why |
|---|---|---|---|---|---|
| 1 | analyze | DevContext2 | 171 | HELPED | **A5**'s MCP-side numbers: 1260 nodes / 1398 edges / 30 entries |
| 2 | find | CallGraphBinder | 271 | HELPED | the binder and its members |
| 3 | get_context | CallGraphBinder @8000 | 8,002 | **HURT** | **59% of the task's entire token bill** and it advanced no fact — the answer was three calls away in `read_source`. Wasted budget is HURT by §2's own rubric |
| 4 | find | BodyFacts | 140 | HELPED | named `BodyFactsExtractor` as the producer |
| 5 | find | BodyFactsExtractor kind:Member | 64 | HELPED | **one** member indexed — the first sign the class is thin in the graph |
| 6 | neighbors | BodyFactsExtractor out | 283 | HELPED | the 3 edges. The anomaly that cracked the question open |
| 7 | find | `Invocation` | 49 | NEUTRAL | one result, no advance |
| 8 | get_context | InvocationOp | 938 | NEUTRAL | a DTO; did not advance |
| 9 | read_source | BodyFactsExtractor mode:"member" | 1,392 | HELPED | **decisive** — the six real call sites |
| 10 | find | BodyFactExtractor | 141 | HELPED | killed my missing-node hypothesis: the callee IS a node |
| 11 | find | RazorCodeVirtualizer | 193 | HELPED | same, for the non-lambda call site |
| 12 | find | `Extract` kind:Type | 974 | NEUTRAL | denominator context, no fact |
| 13 | neighbors | BodyFactExtractor **in** | 33 | HELPED | **0 in-edges** — the finding |
| 14 | neighbors | RazorCodeVirtualizer **in** | 34 | HELPED | 0 — and this call site is in no lambda, which refutes bug #8's cause |
| 15 | neighbors | BodyFactExtractor out | 261 | HELPED | 3 `Semantic` out-edges — a live node, not a stub |
| 16 | neighbors | ExtractorHelpers **in** | 33 | HELPED | 0 — third independent static type, 3-for-3 |
| 17 | stats | — | 549 | HELPED | Calls verified 280 / approx 1103 — the scale of it |

**HELPED 12 · NEUTRAL 4 · HURT 1.**

## Combined

| | calls | tokens | HELPED | NEUTRAL | HURT |
|---|---|---|---|---|---|
| G4.1 Task 1 | 43 | 44,712 | 28 | 7 | 8 |
| G4.2 Task 2 | 21 | 14,424 | 15 | 2 | 4 |
| G4.2 Task 3 | 17 | 13,528 | 12 | 4 | 1 |
| **total** | **81** | **72,664** | **55** | **13** | **13** |

Four of the five G4.2 HURTs are the *confident partial truth* shape (#6/#8 dead-end advice, #12 bug
#9, #16 bug #10). The fifth is pure budget: one 8,002-token pack that answered nothing.
