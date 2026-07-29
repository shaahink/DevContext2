# R4 §2 — the MCP dogfood report: is DevContext's MCP a proper tool?

**Checkpoint G4.3.** Covers the whole protocol: Task 1 (read/understand, G4.1), Task 2 (develop,
G4.2), Task 3 (DevContext on itself, G4.2). **Date:** 2026-07-29. **Branch:** `feat/graph-v2`.
Written after the drives, from the logged responses — every claim below is checkable against a raw
JSON file in this directory.

## 1. What was run

| task | repo | calls | tokens | wall | grades |
|---|---|---|---|---|---|
| 1 — read/understand | `eval-repos/Hangfire` (unseen, non-octet, Library, 0 entries) | 43 | 44,712 | 12.9 s | H28 / N7 / **U8** |
| 2 — develop | `eval-repos/Hangfire` | 21 | 14,424 | 9.0 s | H15 / N2 / **U4** |
| 3 — on itself | `C:\code\DevContext2` | 17 | 13,528 | 27.1 s | H12 / N4 / **U1** |
| **total** | | **81** | **72,664** | **49 s** | **H55 / N13 / U13** |

Wall time is dominated by two cold `analyze` calls (8.3 s + 26.6 s = 71% of it). The other 79 calls
total **~14 s**.

Detail: `G4.1-EVIDENCE.md` · `G4.2-EVIDENCE.md` · `CALL-GRADES.md` · `CALL-GRADES-G4.2.md` ·
`task3/DEVCONTEXT-JSON-SCOPE.md`. Logs: `call-log.jsonl`, `task2/`, `task3/`. Driver:
`eval/mcp-qa/dogfood.js`.

Both task-2/3 drives declared their target **before** the first call (`task2/CHANGE-SPEC.md`,
committed at `546fb32`), as Task 1 did with `QUESTIONS.md` at `254fd36`.

## 2. Judged against R4 §3's success bar

### Bullet 1 — *"≥8/10 answered correctly WITHOUT falling back to grep, in fewer tokens than a grep-based session"*

**Correctness: PASS, with a caveat that must travel with the number.**

- Task 1: **8/10** by the letter — **6/10** if "answered" must mean a *tool asserted the fact* rather
  than the agent inferring it from names. Q4 and Q5 fall on that stricter reading, both because the
  graph has no inheritance edge.
- Task 2: **6/6** declared facts, all TOOL-asserted, and the resulting code **compiles**
  (`task2/build.txt`, EXITCODE=0, with a deliberate red canary at
  `LogJobDurationAttribute.cs(57,23)` proving the build actually compiles the file).
- No grep in either drive. `read_source` went unused in Task 1 and became load-bearing in Tasks 2/3.

The 8/10-vs-6/6 gap is the report's most useful single observation: **the MCP is markedly better at
"what does this look like" than at "what happens next."** Task 1 asked for behaviour, which lives in
edges. Task 2 asked for declarations — namespace, signature, base class, state bag, logging
convention, a file to copy — which live in `signatures`/`bodies` and come back verbatim. A
development task is mostly the latter, which is the good news; the architecture questions this tool
is pitched at are mostly the former.

**Tokens: FAIL, twice, and it should be recorded as a fail.**

| | MCP | grep control |
|---|---|---|
| Task 1 | 44,712 tok / 43 calls | ~4,000 tok / 3 calls (post-drive verification of all ten answers) |
| Task 2 | 14,424 tok / 21 calls | ~2,362 tok / 5 calls (`task2/control/`) |

Grep wins by 6–11×. What the ratio hides can now be *named* rather than asserted: Task 2's winning
grep was `rg ": JobFilterAttribute"` — **a query that presupposes fact F3, one of the six being
sought**. Same for `rg "LogProvider\.For"` (F5). Every control was seeded by the drive it is compared
against. A cold grep session begins at `rg -i filter` over 357 files and pays for the noise.

Two deductions the MCP is owed: 29% of Task 2's bill was the *discriminating probe for bug #9*, not
orientation; and `analyze` is once per repo per HEAD, after which G1.4's `cached` flag reopens a
session in milliseconds.

**The defensible claim, unchanged across all three tasks: the MCP's value is not token economy. It
is that it produces correct symbol names, signatures and `file:line` provenance from zero prior
knowledge, in under a second of tool time per call — and grep cannot, because grep needs the name
first.** The right posture for an agent is MCP to find the names, then read the source.

### Bullet 2 — *"No tool returns content instructing the agent to use another surface"*

**PASS.** G1.1 fixed the CLI-flags-in-MCP class at the Core renderers all three surfaces share, and
across 81 responses no CLI-only flag was advertised. Nothing regressed.

### Bullet 3 — *"Every dead-end reply names a next step that WORKS over MCP"*

**FAIL — and now measured twice, in two repos, on two different tool pairs.**

- Task 1, call 38: an envelope recommended `usages(nodeId:…)`; call 41 ran exactly that and got
  `{count:0, usages:[]}` — **strictly less than the reply that suggested it**.
- Task 2, calls 6 → 8: `usages(query:"IServerFilter")` returned an envelope recommending
  `usages(handle, nodeId:"Type:Hangfire.Server.IServerFilter")`; that call returned `{count:0}`.

Partial credit where it is due: `get_context` now emits `suggestedFocuses` on low-fill packs (seen in
Task 3), which is exactly the right shape — a named, working alternative. It is the one bright spot
in this bullet and the model the others should copy.

## 3. What it lacks — ranked by what it cost an agent doing real work

1. **No inheritance edge kind.** `SeamKind` is `Entry, Call, Send, Handle, Raise, Consume, Data,
   Resolve, Pipeline, CrossService`. So *"who implements this interface"* and *"what does this derive
   from"* — the two questions a library forces on you — cannot be asked. `IServerFilter`, implemented
   by every filter in Hangfire, has **inDegree 0 and outDegree 0**. This cost Task 1 two questions and
   was the largest single obstacle in Task 2. It is the biggest gap for the `Library` archetype, which
   S10's E3 decision made first-class.
   **Workaround found in Task 2, usable today with no engine change:** ask about the type the
   interface's method takes as a *parameter*. `get_context(focus:"PerformingContext")`'s `usage`
   section named three implementors with `file:line` when the interface itself named none.

2. **Static calls with a type-name receiver produce no call edge** (bug #11, high). Three static types
   in DevContext's own repo — `BodyFactExtractor` (the body walker), `RazorCodeVirtualizer`,
   `ExtractorHelpers` — have **0 in-edges** despite known call sites. All are live, well-formed nodes
   with out-edges. 80% of this repo's own Calls edges are `approx` (verified 280 / approx 1103).
   **This also refutes bug #8's stated cause**: calls inside lambda arguments *do* bind (two of them,
   one nested two deep, in the same method), and the call that binds nothing is in no lambda at all.
   Re-measure #8 against this counter-example before fixing it as written.

3. **"Confident partial truth" is now the strand's signature defect class** — bugs #6, #9, #10, and
   arguably #7 and #11. Not emptiness, not an error: a reply shaped exactly like a complete answer.
   - #6 `trace` + a nodeId → `found:true`, 0 steps, `"Type: Type"`.
   - #9 `get_context`'s `fillNote` asserts *"the pack already contains everything reachable"* while
     eliding the body you asked for — proven on one focus at two budgets, same sentence both times.
   - #10 `read_source` silently accepts an invalid `mode` and returns 20 of 147 lines, `found:true`.
   The general rule this implies is one line long and would cover all three: **anything the response
   elided, truncated, or defaulted must appear on the wire.** `eval/contract-sweep.ps1` cannot catch
   this family — it asks whether a proto field has a *reader*, and here every field is read.

4. **All 22 tools ship `description: ""`** (bug #5). 31 written `///` summaries, with worked examples,
   never reach the wire; the first thing every agent sees is 22 unexplained verbs for ~3,500 tokens.
   The remedy is **measured**, not guessed: `GenerateDocumentationFile` changes nothing (byte-identical
   response), `[System.ComponentModel.Description]` works. Tool selection is the whole game for an
   agent, and this is its only input.

5. **Dead ends still do not name a working next step** (§3 bullet 3 above). Two measured instances.

6. **`map` is all-or-nothing** — 17,105 tokens on Hangfire, one parameter (`handle`), 38% of Task 1's
   entire bill. There is no way to ask for less of it. Compare `get_context`, which takes
   `budgetTokens` and (now) suggests better focuses.

7. **A method registered as a Type node** (bug #7): `Type:Hangfire.StackTraceHtmlFragments::Type(1)`,
   an explicit interface method with an empty `filePath`, absorbing 26 BCL `System.Type` references —
   **4.2% of the repo's edges** — and ranking 5th in `stats`' wiring hubs.

8. **Two tools, one entry, different names** (bug #2): `entrypoints` renders `GET /todos` while
   `get_context`/`trace` only resolve `<lambda> GET /todos/`.

9. **The CLI and the server disagree about the same repo** — 1254/1383 vs 1260/1398 nodes/edges on
   DevContext2. Small (0.5% / 1.1%) but real, and R4 §2 blamed the wrong cause: measured, the server
   ignoring `devcontext.json` moves **no nodes and no edges** (Batch C's solution scoping already
   excludes the fixture projects). Rider: `DevContextConfig.DefaultPath` reads the config from the
   **working directory**, not the analysed repo root.

## 4. What works, and must not be lost

The G1–G3 fixes are visible all over these drives and carried them:

- `analyze`'s honest envelope (G1.4): `cached`, `analyzedAt`, `gitHead`, archetype — the frame for
  everything after it, and the instrument that made A5's measurement possible at all.
- **`file:line` provenance and a `resolution` on every edge** in every `neighbors`/`usages` reply.
  This is what turned post-drive verification into three shell calls instead of a hunt, and it is the
  single most valuable property the tool has.
- `get_context` on a symbol root (G1.2) — a real pack on a library with zero entries.
- The compact-trace legend and glyphs (G1.3).
- Kind-filtered `neighbors` (G3.2) and server-side `find(kind:)` totals (G1.4).
- Error envelopes are, in *shape*, good: they say what resolved, offer candidates, give an example.
  Every failure above is about where they point, not how they are built.

## 5. The verdict asked for

*Is it a proper tool?* **Not yet — but the gap is now specific, and it is not the gap R4 assumed.**

R4 §3 framed the question as economy: fewer tokens than grep. Measured across three tasks, that bar
is **not met and probably should not be the bar**, because the comparison is unwinnable by
construction — grep is cheap once you know the name, and knowing the name is the thing the MCP sells.
The honest replacement bar is the one Task 2 accidentally demonstrated: **an agent oriented only by
this tool wrote code that compiled, in a repo it had never read, in 21 calls and 0.7 s of tool time
after the analyze.** That is a proper tool's behaviour.

What stops it being one today is not breadth — it has 22 tools and the primitives it was missing
(`seam`, kind-filtered `neighbors`, cache truth) landed in G3. It is **two things**:

1. **The graph is missing whole classes of edge** — inheritance (never modelled) and static calls
   (modelled, silently not bound). An architecture tool whose call graph cannot see a static utility
   layer, and whose type graph cannot see an interface's implementors, is answering a lot of questions
   with a confident *nobody*.
2. **The replies do not distinguish "no" from "I did not look."** `count: 0` reads identically whether
   the answer is genuinely zero or the edge kind does not exist. Every one of this strand's eight
   silent-wrong-answer findings reduces to that sentence.

Fix those two and the tool is proper; the descriptions (#5) are the cheapest large win on top, and
they are a day's work with a measured remedy already in hand.
