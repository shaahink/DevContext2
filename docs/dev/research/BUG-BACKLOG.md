# Bug backlog — findings filed, not fixed

> Exported 2026-08-02 from the graph-v2 autonomous run (`run bae63ba0` — 28 sessions,
> 22/22 checkpoints, full battery green at every stage close). The source of record was
> `.conductor/run.db`, **which is gitignored, so this file is the durable copy.**
>
> These are defects the run *measured* and deliberately filed instead of fixing — either
> because the fix was a product decision rather than a threshold correction, or because it
> fell outside the stage that found it. Each carries the evidence path that proves it.
> None is a regression.

**24 open — 7 high, 16 medium, 1 low.**

| # | Sev | Stage | Title |
|---|-----|-------|-------|
| [#5](#5) | high | G4 | Every one of the 22 MCP tools ships with an EMPTY description — 31 carefully written XML doc summaries exist in the source and none of them reach the wire |
| [#6](#6) | high | G4 | trace() handed a nodeId returns found:true with an EMPTY tree titled "Type: Type" — and its own error envelope tells the agent to pass a nodeId |
| [#7](#7) | high | G4 | A METHOD is registered as a Type node and 26 BCL System.Type references bind to it — the mis-bound node is stats' #5 "wiring hub" on Hangfire |
| [#8](#8) | high | G4 | Calls inside a lambda argument produce NO edge — the actual storage write of Hangfire's enqueue path is invisible, and the trace of the writing type looks complete without it |
| [#9](#9) | high | G4 | get_context's fillNote says the pack "already contains everything reachable from this focus" while it is eliding the body the agent asked for — measured false at two budgets on the same focus |
| [#11](#11) | high | G4 | Static calls with a TYPE-NAME receiver produce no call edge — DevContext's own body-fact walker has 0 in-edges in DevContext's own graph, and this REFUTES bug #8's lambda explanation |
| [#12](#12) | high | G5 | The semantic receiver-type upgrade misses every invocation whose statement fits on one line — TryBindReceiverType relocates by LINE SPAN and then searches ANCESTORS, so the invocation is a descendant and is never found |
| [#1](#1) | medium | G1 | MCP QA harness scores a false 0/12 on the first run after any Core change (accepts a session before its analysis has a graph) |
| [#2](#2) | medium | G1 | `entrypoints` names an entry one way and `get_context`/`trace` cannot resolve that name (TodoApi: "GET /todos" vs "&lt;lambda&gt; GET /todos/") |
| [#3](#3) | medium | G1 | The DevContext.Server the MCP spawns EXITS (-1) instead of binding when the machine is loaded — the MCP then kills itself, so an agent's first call dies with no server at all |
| [#10](#10) | medium | G4 | read_source silently accepts an INVALID mode and falls back to a 20-line window — mode:"full" returned 20 of 147 lines with found:true and no complaint |
| [#13](#13) | medium | G5 | `analyze --no-cache` does not invalidate the snapshot a later `query` reads — a changed repo returned the OLD graph with snapshotCache "HIT" |
| [#14](#14) | medium | G5 | A GENERIC command attribute is not recognised — `[Command&lt;ConfigCommand&gt;("init", ...)]` makes GitVersion's four SUB-commands invisible, so the command surface shows 5 verbs where the tool ships 9 |
| [#15](#15) | medium | — | Conductor's gate runner keeps only the stdout tail, so a gate that dies mid-stream leaves NO diagnostic — the s16 fast-engine red arrived as three PASS lines and a banner |
| [#16](#16) | medium | G5 | AnalyzeCacheTruthTests.All_three_paths_report_themselves misses the snapshot rehydrate ~1-in-50 (third.Cached false) — cause UNKNOWN; the obvious dirty-fingerprint theory was tested and REFUTED |
| [#17](#17) | medium | G6 | The engine ships TWO Member-title vocabularies: some Member nodes are titled "Owner.Method", others bare "Method" — same kind, same page |
| [#18](#18) | medium | G6 | Type nodes are created from lambda/expression TEXT — a node whose id and title are a 20-line lambda body reaches the UI |
| [#19](#19) | medium | — | Atlas/map states a FOURTH service count: STYLE evidence says "6 runnable web services" where the per-service breakdown lists 5 and the canvas draws 5 — same page, same scope, two counters |
| [#20](#20) | medium | G7 | A library's Atlas counts an auxiliary demo executable as a SERVICE: AutoMapper reads '1 services (1 drawn)' and the per-service breakdown names TestApp |
| [#22](#22) | medium | G10 | graph.orphans dead-code insight is dormant: its Semantic-share floor (0.5) is unreachable on 11/11 poles (measured 2026-08-02, G10.1) |
| [#23](#23) | medium | G10 | L3.4 hub-scope broadening never fires: sparseGraph=false + hubScopeNodes=0 on 11/11 poles including its own trigger population (Dapper/Serilog/MahApps/MediatR); identity-strip's sparse line has never rendered (G10.1) |
| [#24](#24) | medium | G10 | Deep-spine ratio is saturated (1.000 on 5/11 poles, 0.96-0.98 on the rest): the report prints it as coverage but it separates no repo (G10.1) |
| [#25](#25) | medium | G10 | Engine ships two definitions of a verified edge: GraphStats/SeamStat approx=Syntactic only (so Join counts as verified) while GraphOrphansSource counts Semantic only; Resolution.Join is also the enum default (G10.1) |
| [#4](#4) | low | G2 | The desktop MCP page keeps its own tool list; it advertised `search`, a tool the MCP has never exposed |

---

## HIGH — 7

<a id="5"></a>

### #5 · G4 · Every one of the 22 MCP tools ships with an EMPTY description — 31 carefully written XML doc summaries exist in the source and none of them reach the wire

```text
MEASURED 2026-07-29, first call of the G4.1 dogfood drive. Artifact: eval-results/2026-07-29/mcp-dogfood/raw/001-tools_list.json (the real tools/list response, 13,870 chars / ~3.5k tokens).

WHAT THE WIRE SAYS. Every tool object comes back as:
  { "name": "top_flows", "description": "", "inputSchema": { ... }, "execution": {...} }
All 22 tools. `description` is the empty string on every one, and no parameter carries a description either — the inputSchema is bare types and nulls. So ~3.5k tokens of an agent's context is spent on a menu of 22 names with zero semantic content, at connect, before any work happens.

WHY THIS IS THE STRAND'S SIGNATURE DEFECT, NOT A COSMETIC ONE. The descriptions are NOT missing. src/DevContext.Mcp/DevContextTools.cs carries 31 `///` XML doc summaries, written FOR the agent, with worked examples in them:
  /// <summary>Start analysis of a .NET repo. Returns a handle for subsequent calls. Idempotent:
  /// same repo+HEAD+solution returns the existing handle. ... Example: analyze("C:/repos/MyApp"),
  /// analyze("C:/repos/GitVersion", sln:"GitVersion.slnx")</summary>
  [McpServerTool]
Somebody did the work. It has never shipped. This is the same family as S10's Insight.Severity (the field IS read, with the wrong key) and G1.4's AnalysisSummary.archetype (assigned nowhere): the fact exists on one side and the surface shows nothing.

ROOT CAUSE, CANDIDATE — NOT YET MEASURED. src/DevContext.Mcp/DevContext.Mcp.csproj does NOT set <GenerateDocumentationFile>, so no .xml doc file is emitted next to devcontext-mcp.dll and there is nothing for the SDK to read. There are no [Description] attributes either (grep: zero occurrences of "Description" in DevContextTools.cs).

DO NOT FIX BY READING THIS PARAGRAPH. Whether ModelContextProtocol 1.4.0 / AIFunctionFactory picks descriptions up from an XML doc file at all, or only from System.ComponentModel [Description] attributes, is exactly the kind of thing this program has been wrong about three times. MEASURE IT: add <GenerateDocumentationFile>true</GenerateDocumentationFile>, `dotnet build src/DevContext.Mcp`, re-run tools/list, and look at the field. If it is still empty, the fix is [Description] attributes generated from the existing summaries — same text, different carrier.

REGRESSION NET IT NEEDS. Nothing catches this today: eval/contract-sweep.ps1 asks whether a PROTO field has a reader, and no proto field is involved; drive-r4.js's `menu` case checks tool NAMES against tools/list and never looks at the description. The obvious guard is one assertion in that case — every tool in tools/list has a non-empty description — which would have gone red on this state from the day it was written.

WHY IT MATTERS FOR R4 §3 ("what does it lack, how does it become a proper tool"): the first thing every agent that ever connects to this MCP sees is 22 unexplained verbs. Tool selection is the whole game for an agent, and this is the input to it.
```

<a id="6"></a>

### #6 · G4 · trace() handed a nodeId returns found:true with an EMPTY tree titled "Type: Type" — and its own error envelope tells the agent to pass a nodeId

> **FIXED 2026-08-13 (T1.3, commit cdb152c).** `EntryPointResolver` gained a nodeId tier. Re-measured
> on a real MCP session: `eval-results/2026-08-13/t1-partial-truth/{pre-fix,post-fix}/trace-by-nodeid.json`
> — 10 of 12 nodeIds RED before (found:false while the bare title traced), 12/12 agreeing after. The
> "Type: Type" phantom needs a graph carrying a node titled `Type`, which TodoApi has not, so that half
> is pinned by `EntryPointResolverTests.Resolve_of_a_nodeId_never_lands_on_a_node_titled_like_its_kind`
> (proven red with only the resolver stashed to HEAD). The vacuous `found:true, steps:0` shape now
> carries a `note` naming the `neighbors(direction:"in")` call, which is what the bar below asked for.

```text
MEASURED 2026-07-29 in the G4.1 dogfood drive, real MCP calls on eval-repos/Hangfire. Raw dumps: eval-results/2026-07-29/mcp-dogfood/raw/{007,008,009,010,011,012}-*.json. This is a SILENT WRONG ANSWER, the worst class — nothing in the reply says anything went wrong.

THE FOUR CALLS, same subject, same session:

1. neighbors(nodeId:"Type:Hangfire.BackgroundJobClient", direction:"out")  ->  count 4, four Calls edges with file:line provenance (Create -> JobStorage @BackgroundJobClient.cs:153, Create -> IBackgroundJobFactory @:156, ChangeState -> JobStorage @:175, ChangeState -> IBackgroundJobStateChanger @:177). The wiring is THERE, and the C3 roll-up finds it from the Type node.

2. trace(focus:"Type:Hangfire.BackgroundJobClient", format:"compact")  ->  {"found":true, "steps":0, "tokens":9, "budgetSource":"server trace policy", "legend":"[v] entry", "text":"Entry: Type: Type\r\n[v] Type: Type\r\n"}
   found TRUE. Zero steps. And the rendered title is the string "Type" — the node's own kind, twice. A phantom node.

3. trace(query:"Type:Hangfire.BackgroundJobClient", format:"compact")  ->  byte-identical phantom. Both parameters do it, so this is the focus RESOLUTION, not one argument's plumbing.

4. trace(query:"BackgroundJobClient", format:"compact")  ->  found:true, steps 6, the correct tree: Type -> Member::ChangeState -> {JobStorage, IBackgroundJobStateChanger}, Type -> Member::Create -> {JobStorage, IBackgroundJobFactory}, each hop with provenance. So the BARE NAME works and the nodeId does not.

NOT a bad nodeId: get_context(focus:"Type:Hangfire.BackgroundJobClient", budgetTokens:3000) resolves the very same string correctly — 4 sections (identity/trace/signatures/usage), 942 tokens. Every other tool in the menu takes nodeId (neighbors, usages, impact, node, read_source, tests_for, seam). trace is the outlier.

WHY AN AGENT HITS THIS ON THE FIRST TRY, NOT THE HUNDREDTH: the whole workflow hands you nodeIds. resolve/find/neighbors all return `nodeId` fields, so "read the id off resolve, hand it to trace" is the obvious move. Worse, trace's OWN error envelope for an unmatched focus says:
    "hint": "Did you mean one of these? Use an exact route or nodeId."
    "candidates": [ {"nodeId":"Type:Hangfire.BackgroundJobClient", ...}, ... ]
So the tool instructs the agent to pass a nodeId, hands over the exact nodeIds to use, and then answers a nodeId with a confident empty trace. That is R4 §3's third bullet ("every dead-end reply names a next step that WORKS") failing in the one direction that cannot be noticed.

ALSO MEASURED, same family: trace(focus:"Member:Hangfire.BackgroundJobClient::Create") -> "No entry or node matched", with candidates that are all Type nodes. So a member nodeId does not resolve at all, and the candidate list it offers back routes the agent straight into the phantom above.

ROOT-CAUSE HYPOTHESIS, NOT MEASURED — DO NOT FIX BY READING IT. The rendered "Type: Type" looks like a "Kind: Title" render where Title got the substring before the first ':'. Find where trace's focus resolution builds a synthetic entry when nothing matched, measure what it puts in the title, and make the nodeId path go through the same resolver get_context uses (that one demonstrably works on this exact string). Watch the check go RED before believing the fix: a trace that returns found:true with 0 steps passes any "did I get a trace" assertion.

WHAT THE BAR SHOULD BE. `found:true` with `steps:0` is the vacuous shape. Either the focus resolved to a node with genuinely no out-edges (say so: found:true, steps:0, and a note naming the neighbors call that would show its in-edges) or it did not resolve (found:false + candidates). Silently doing neither is what makes this a bug rather than a limitation.
```

<a id="7"></a>

### #7 · G4 · A METHOD is registered as a Type node and 26 BCL System.Type references bind to it — the mis-bound node is stats' #5 "wiring hub" on Hangfire

> **FIXED — re-measured closed 2026-08-13 (E1.3, commit f686e25 + the V1.3/E1.2 fixes it verifies).**
> Measured at both ends on the bug's OWN repo, not argued: at `0fd1cbe` (pre-V1.3) Hangfire ships
> `Type:Hangfire.StackTraceHtmlFragments::Type(1)` with **inDegree 26**, verbatim as filed — plus a
> SECOND instance of the same class never reported before, `Type:ConsoleSample.Services::Random(1)`
> (inDegree 1). At HEAD both are gone: zero nodes carry kind Type with a `::` id, and the only edge
> left touching the fragment type is the legitimate `IStackTraceFormatter\`1 → StackTraceHtmlFragments`
> Resolves. Nothing collapsed with them — the same repo went 928→994 nodes and 599→877 edges.
> Three fixes did it: V1.3's producer-level refusal (a member answer is not a type answer), V1.3's
> INV-A refusal at `AddNode`, and E1.2's `afee44b` out-of-solution gate.
> **What E1.3 added:** the refusal is no longer SILENT (it dropped the node *and every edge that
> wanted it*, invisibly). `CodeGraphBuilder.RefusedNodes` counts distinct refused keys and
> `GraphBuilder` reports the tally as a `GraphInvariants` diagnostic, with a positive-control test so
> a measured zero cannot be a dead instrument. **Swept at HEAD: 0 refusals on all 7 poles** — no
> producer even attempts the shape (`eval-results/2026-08-13/e1-typenode/refusal-sweep.txt`).
> Standing guard: `BclNameCollisionEdgeTests` (labelled in its own doc comment as a guard, NOT a
> reproduction — it passes at 0fd1cbe too). Evidence: `eval-results/2026-08-13/e1-typenode/`.

```text
MEASURED 2026-07-29 in the G4.1 dogfood drive on eval-repos/Hangfire, then verified against the repo source. Raw dumps: eval-results/2026-07-29/mcp-dogfood/raw/{016-stats,019-resolve,020-node,023-usages}.json.

WHAT THE GRAPH HOLDS:
  node(query:"Type") -> {"nodeId":"Type:Hangfire.StackTraceHtmlFragments::Type(1)","title":"Type","kind":"Type","filePath":"","outDegree":0,"inDegree":26}

WHAT IT ACTUALLY IS (verified in source): src/Hangfire.Core/App_Packages/StackTraceFormatter/StackTraceFormatter.cs:47
  string IStackTraceFormatter<string>.Type(string markup) => BeforeType + markup + AfterType;
An EXPLICIT INTERFACE METHOD IMPLEMENTATION with one parameter — hence the `::Type(1)` member signature sitting inside an id whose prefix says `Type:`, and hence `kind: "Type"`. A member is wearing a type node's identity. It also carries an EMPTY filePath while every honest node in the same response carries one, so "no file" is available as a cheap detector.

THE 26 IN-EDGES ARE ALL MIS-BINDINGS. usages(nodeId) names them: JobDisplayNameAttribute::InitResourceManager, SqlServer.ExceptionTypeHelper::IsCatchableExceptionType, Common.Job::ToString, and 23 more. Verified in source: none of them touch the dashboard's stack-trace formatter. What they touch is the BCL:
  JobDisplayNameAttribute.cs:33  ConcurrentDictionary<Type, ResourceManager>   / :74  InitResourceManager(Type type)
  ExceptionTypeHelper.cs:23      private static readonly Type OutOfMemoryType = typeof(OutOfMemoryException);
So references to System.Type are resolving onto a same-named local member. 26 of Hangfire's 615 edges — 4.2% of the entire call graph — are phantom.

HOW IT REACHES A USER. Three surfaces, no error anywhere:
  1. stats -> insight `wiring.hubs` reads "Wiring hubs: SqlServerStorage · ILog · IStorageConnection · JobStorage · Type". A formatter fragment ranks fifth in the repo by connectivity.
  2. overview/find rank on degree, so it competes for "start here" attention.
  3. It is the node every `trace(focus:"Type:...")` lands on — see the separate bug on trace's prefix-token resolution. That bug is INVISIBLE on repos without a symbol called "Type"; this node is what made it observable.

WHY IT MATTERS BEYOND THIS REPO: the same shape fires for any explicit interface implementation whose method name collides with a BCL type name that the repo also references — Type, Task, Action, Path, Environment, Console, Convert. The collision is with the NAME, so the more idiomatic the code, the likelier the hit.

WHAT TO MEASURE FIRST (do not fix from this text): (a) why an explicit interface implementation is emitted with a Type: id and kind Type instead of a Member node — that is the identity-spine half and it is upstream of everything else; (b) whether the binder's fallback matches an unresolved BCL name against local members by short name, and whether it should refuse when the resolved candidate's kind disagrees with the syntactic context. A cheap invariant worth adding either way: no node may have kind Type and a `::name(arity)` member signature in its id — that shape is self-contradictory and is trivially assertable over any graph.
```

<a id="8"></a>

### #8 · G4 · Calls inside a lambda argument produce NO edge — the actual storage write of Hangfire's enqueue path is invisible, and the trace of the writing type looks complete without it

> **FIXED by E1.2, and the FILED MECHANISM BELOW IS REFUTED — re-measured 2026-08-13 (E1.3, f686e25).**
> "Whether the extractor walks lambda/anonymous-function bodies at all" is answered: it always did.
> `BodyFactExtractor.WalkMember` walks `body.DescendantNodes()`, so a lambda-body call whose receiver
> is a FIELD, or a TYPED lambda parameter (`GetEnclosingParamType`), has always produced an edge on the
> syntax tier alone — both pinned green in `LambdaArgumentEdgeTests`.
> **The true mechanism is one token narrower: a receiver rooted at an UNTYPED lambda parameter is in no
> syntactic scope, so only Tier B (SemanticLite) can type it.** Hangfire's own site is worse than any
> delegate-signature lookup could fix — `RetryOnException<TContext>(ref int, Action<int,TContext>,
> TContext)` infers the parameter's type from a LATER argument.
> **RED FIRST, and it dates the fix:** at `795b71b` (post-#11, pre-#12) the two semantic fixtures FAIL
> and the three syntactic ones pass; at HEAD all five pass. E1.2's TextSpan-on-op is what closed it —
> not #11, and not a lambda walk (`eval-results/2026-08-13/e1-typenode/bug8-redfirst-at-795b71b.txt`).
> **Both halves measured closed on Hangfire at HEAD:** `CoreBackgroundJobFactory::CreateBackgroundJob`
> `TwoSteps → IStorageConnection [Calls/Semantic]` now exists (that member is lines 76–142 and every
> storage call in it is inside a lambda; the type went from 3 out-edges to 7), and `InvocationData` —
> filed with ONE in-caller where source had at least four — now has 7, including all three the bug
> named by file:line. **Residual, named not hidden:** on a project that degrades to Tier A (missing
> assets.json) the untyped-lambda-parameter receiver still yields no edge; pinned as
> `Tier_A_alone_cannot_type_an_untyped_lambda_parameter` and re-filed at MEDIUM below.

```text
MEASURED 2026-07-29 in the G4.1 dogfood drive (eval-repos/Hangfire), then verified in source. Raw dumps: eval-results/2026-07-29/mcp-dogfood/raw/{027-trace,043-neighbors}.json.

THE QUESTION IT BROKE: "when application code calls BackgroundJob.Enqueue, where does the job get written to storage?" — the first question on the list, and the flavour R4 §2 names first ("where does X get persisted").

GROUND TRUTH (src/Hangfire.Core/Client/CoreBackgroundJobFactory.cs:89):
    static (_, ctx) => ctx.Context.Connection.CreateExpiredJob(
i.e. the write happens inside a static lambda passed to the type's own RetryOnException helper.

WHAT THE GRAPH HAS — neighbors(query:"CoreBackgroundJobFactory", direction:"out"), the C3 roll-up over the whole type: count 3, totalEdges 3.
    Create           -> JobStorage      @CoreBackgroundJobFactory.cs:61
    Create           -> CreateContext   @CoreBackgroundJobFactory.cs:66
    RetryOnException -> ILog            @CoreBackgroundJobFactory.cs:172
No edge to IStorageConnection. No edge for line 89. The type whose one job is to persist a background job has no edge to storage anywhere in the graph.

WHY IT IS WORSE THAN A MISSING EDGE: trace(query:"CoreBackgroundJobFactory") returns found:true with a tidy 5-step tree and no truncation marker, no `omitted` count, no caveat. Nothing distinguishes "this member calls nothing else" from "this member's real work is in a lambda the extractor did not walk". An agent reads the tree and concludes the factory does not touch storage — which is the exact opposite of the truth.

SCOPE — this is not a Hangfire quirk. Any call made inside a lambda argument disappears: retry/resilience helpers (Polly, the shape above), services.AddX(o => ...), Parallel.ForEach bodies, LINQ selector bodies, EF SaveChanges wrappers, minimal-API handler bodies passed as lambdas. Modern C# puts a large fraction of real work there. NOTE the related shape already visible elsewhere in this same drive: neighbors(query:"InvocationData", direction:"in") reports ONE caller (SqlServer.SqlServerMonitoringApi::DeserializeJob) when the source has at least four in Core alone (RecurringJobExtensions.cs:115, RecurringJobManager.cs:118, StorageConnectionExtensions.cs:115) — measure whether that undercount has the same cause before assuming it does.

WHAT TO MEASURE FIRST: whether the extractor walks lambda/anonymous-function bodies at all, and if it does, whether the resulting call is attributed to the enclosing member or dropped for having no member to hang on. Write the check against a fixture with a call ONLY inside a lambda argument and WATCH IT GO RED first — a fixture whose lambda body duplicates a call made elsewhere in the same method passes on the broken state.

MINIMUM HONEST FALLBACK IF THE WALK IS OUT OF SCOPE: a member whose body contains lambda call sites the graph did not attribute should say so, so a trace that stops there is visibly incomplete instead of visibly finished.
```

<a id="9"></a>

### #9 · G4 · get_context's fillNote says the pack "already contains everything reachable from this focus" while it is eliding the body the agent asked for — measured false at two budgets on the same focus

> **FIXED 2026-08-13 (T1.3, commit cdb152c).** The "what to measure first" question below is answered:
> the composer had NO knowledge of an elision — `BuildBodiesToFill` counted only bodies dropped whole,
> so a body truncated to `… (+N lines)` was recorded nowhere. Reproduced on TodoApi, not Hangfire:
> `get_context("Extensions", 1500)` rendered `… (+64 lines)` and claimed completeness; at 20000 the
> elision was gone and the content doubled. The pack now declares every cut on an `elided ` line naming
> `budgetTokens` (`ContextPackBuilder.ElidedPrefix` / `DeclaresElision` — one definition), and `fillNote`
> reads that declaration instead of inferring completeness from the fill ratio. Evidence:
> `eval-results/2026-08-13/t1-partial-truth/{pre-fix,post-fix}/elision-honesty.json`.

```text
MEASURED 2026-07-29 in the G4.2 dogfood drive (Task 2, real development work on eval-repos/Hangfire). Raw dumps: eval-results/2026-07-29/mcp-dogfood/task2/raw/{012,015}-get_context.json. Same focus, same session, same handle, two budgets.

THE TWO CALLS

  get_context(focus:"PerformContext", budgetTokens:3000)
    -> totalTokens 1272, sections bodies=707
    -> fillNote: "fill 42%: the pack already contains everything reachable from this focus - its
                  connected subgraph is small (not an error; a smaller budget fits it)."
    -> the body I needed rendered as:
         ### PerformContext - src/Hangfire.Core/Server/PerformContext.cs:29
         ```csharp
         /// <summary>
         /// Provides information about the context in which the job
         /// is performed.
         ... (+75 lines)
         ```

  get_context(focus:"PerformContext", budgetTokens:20000)
    -> totalTokens 3645, sections bodies=3080  (bodies grew 4.4x)
    -> the (+75 lines) elision is GONE; the class body is rendered in full, and it is where the
       answer lives (PerformContext's ctor threads `context.Items` through, which is the bag a
       server filter has to stash state in between OnPerforming and OnPerformed).
    -> fillNote: "fill 18%: the pack already contains everything reachable from this focus ..."
       - THE SAME SENTENCE, at a budget where it is finally true.

WHY THIS IS THE SILENT-WRONG-ANSWER CLASS, NOT A COSMETIC WORDING BUG
The note is the only signal an agent has for "should I ask for more". It asserts completeness in the
one situation where the pack is incomplete, and it asserts it in the reassuring register ("not an
error"). I read it at budget 3000, believed it, and went looking for the Items bag through a
different tool. Had the focus been one where no other route existed, the drive would have concluded
that PerformContext carries no state bag - which is false, and nothing in the response would have
contradicted it.

THE UNDERLYING CONFUSION, NAMED: two different claims are welded to one number.
  - "fill %" = totalTokens / budgetTokens, i.e. HOW MUCH OF YOUR BUDGET IS USED. It legitimately
    goes DOWN as the budget goes up: 42% -> 18%.
  - the sentence = THE PACK IS COMPLETE. That is a property of the graph walk, not of the budget.
Content went UP (1272 -> 3645 tokens) while fill% went DOWN, which is exactly the shape you get when
a ratio is narrated as if it were a completeness verdict. A low fill CAN mean "small subgraph"; it
can equally mean "big subgraph, small budget, and I truncated the bodies". The response does not
distinguish them.

WHAT TO MEASURE FIRST (do not fix by reading this): find where the bodies section elides to
"... (+N lines)" and find where fillNote is composed, and establish whether the composer has any
knowledge of whether an elision occurred. My expectation is that it does not - it looks at the
ratio only - but that is a guess and this program has been wrong about exactly this kind of guess
three times. WATCH THE CHECK GO RED FIRST: a test asserting "fillNote does not claim completeness"
passes trivially on a focus with no bodies at all. The discriminating case is a focus WITH an
elided body at a low budget - PerformContext on Hangfire at budgetTokens 3000 is a real one.

THE HONEST BAR: if any section elided anything, the note must say so and name the lever
(budgetTokens), because that lever demonstrably works - 20000 rendered it. Right now the one
response that should say "ask for more" is the one that says "there is no more".

Related but NOT the same as bug #5/#6: those are empty/wrong content. This one is correct content
with a false claim about its own completeness.
```

<a id="11"></a>

### #11 · G4 · Static calls with a TYPE-NAME receiver produce no call edge — DevContext's own body-fact walker has 0 in-edges in DevContext's own graph, and this REFUTES bug #8's lambda explanation

```text
MEASURED 2026-07-29 in the G4.2 dogfood drive, Task 3 (DevContext analysed by its own MCP). Raw dumps: eval-results/2026-07-29/mcp-dogfood/task3/raw/{006,009,010,011,013,014,015,016}-*.json. Handle analysed C:/code/DevContext2 @546fb32: 1260 nodes, 1398 edges, 30 entries.

THE GROUND TRUTH, src/DevContext.Core/Extractors/Specific/BodyFactsExtractor.cs, ExtractAsync (line 38-90). Six call sites, read from source AFTER the drive:

  :42  BuildFileToProject(context)                                    self-call (suppressed by design, Batch C)
  :51  context.Analysis.GetOrBuildBodyFactsAsync(...)   INSIDE a lambda        -> EDGE EXISTS (SharedAnalysisContext)
  :54  context.Cache.GetSyntaxTreeAsync(...)            INSIDE A NESTED lambda -> EDGE EXISTS (IAnalysisCache)
  :56  BodyFactExtractor.Extract(root, filePath, project)  INSIDE nested lambda -> NO EDGE
  :62  context.Logger.LogWarning(...)                   INSIDE a lambda catch  -> EDGE EXISTS (DiscoveryContext)
  :74  Utilities.RazorCodeVirtualizer.EnumerateVirtualTreesAsync(context, ct)  NOT IN ANY LAMBDA -> NO EDGE
  :80  BodyFactExtractor.Extract(...)                                          -> NO EDGE

neighbors(Type:...BodyFactsExtractor, out) returns exactly 3 edges: the three at :51, :54, :62.

THIS REFUTES THE LAMBDA HYPOTHESIS, IN BOTH DIRECTIONS AT ONCE.
 - Calls inside lambda arguments DO bind here — :51 and :62 are inside a Parallel.ForEachAsync lambda,
   and :54 is inside a lambda nested inside that lambda. Bug #8 says a call inside a lambda argument
   produces no edge. On this file it demonstrably does.
 - And :74, which produces NO edge, is not inside any lambda at all. It is an ordinary statement in
   the method body.
So whatever bug #8 saw on Hangfire's CoreBackgroundJobFactory.cs:89, "lambda" is not the discriminator
here. Bug #8 should be re-measured against this counter-example before anyone fixes it as stated.

WHAT THE THREE MISSING EDGES HAVE IN COMMON: all three are STATIC calls whose receiver is a TYPE NAME
(BodyFactExtractor.Extract, Utilities.RazorCodeVirtualizer.EnumerateVirtualTreesAsync). All three
edges that DID bind are INSTANCE calls through a receiver chain rooted at a parameter (context.X.Y()).

THE HYPOTHESIS TESTED ON TWO MORE TYPES, INDEPENDENTLY - 3 for 3:
  neighbors(Type:DevContext.Core.Graph2.BodyFactExtractor, in)        -> count 0, totalEdges 0
  neighbors(Type:DevContext.Core.Utilities.RazorCodeVirtualizer, in)  -> count 0, totalEdges 0
  neighbors(Type:DevContext.Core.Utilities.ExtractorHelpers, in)      -> count 0, totalEdges 0
ExtractorHelpers is a static helper class used across the extractor family; it is called by nobody
according to this graph. All three ARE real nodes (find() returns them with members: BodyFactExtractor
carries Extract / WalkMember / SplitName), and BodyFactExtractor even has 3 OUT-edges resolved
Semantic — so these are live, bound, well-formed nodes that nothing points at. Not a missing-node
problem: I tested that hypothesis first and it was wrong.

WHY IT MATTERS MORE THAN ONE FILE: 1103 of this repo's 1383 Calls edges are `approx` (stats: verified
280 / approx 1103, i.e. 80% approximate) and the whole static-utility layer of the engine is invisible
to its own graph. Any question of the form "who calls this helper" is answered "nobody" with the same
confident shape as a true zero — the count-0 reply carries no caveat.

SELF-REFERENTIAL POINT WORTH KEEPING: the maintenance question this drive was trying to answer WAS
bug #8's own "what to measure first" — does call-edge extraction walk lambda bodies, and where is that
code. The MCP could not lead me to `BodyFactExtractor` (the walker) because nothing in the graph points
at it. I found it by read_source-ing the extractor and reading line 56. The tool's blind spot was
exactly the subject of the question.

WHAT TO MEASURE FIRST (do NOT fix from this text): in CallGraphBinder / SymbolTable, find how a
receiver is resolved for an InvocationOp and establish whether a receiver that is a TYPE (static call)
is handled at all, or only receiver chains rooted at a local/parameter/field. Batch C added
`SymbolTable.HopThroughProperty` for receiver chains — check whether the static case has an
equivalent arm or falls off the end. WATCH IT GO RED FIRST: a fixture whose static call is
accompanied by an instance call to the same target passes on the broken state. The discriminating
fixture has a type-name-receiver static call as the ONLY reference to its target, and asserts one
in-edge on that target.
```

<a id="12"></a>

### #12 · G5 · The semantic receiver-type upgrade misses every invocation whose statement fits on one line — TryBindReceiverType relocates by LINE SPAN and then searches ANCESTORS, so the invocation is a descendant and is never found

```text
MEASURED 2026-07-29 during G5.1, two independent ways. Artifacts: eval-results/2026-07-29/G5.1/raw/probe-semantic.txt (a temporary env-gated probe inside SemanticLitePopulator, since reverted; CLI rebuilt from reverted source) and eval-results/2026-07-29/G5.1/raw/fixture-graph.json (a controlled 4-case fixture, sources archived at eval-results/2026-07-29/G5.1/fixture/).

THE CODE (src/DevContext.Core/Graph2/SemanticLitePopulator.cs:776-794, TryBindReceiverType):
    var span = tree.GetText().Lines[Math.Max(0, inv.Line - 1)].Span;
    var node = root.FindNode(span);
    var invocation = node?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
    if (invocation is null) return null;
The op only records a LINE, so the node is relocated by the whole line's span. For a statement that fits on one line, the innermost node containing that span is the STATEMENT (ExpressionStatement / LocalDeclarationStatement / ReturnStatement). The invocation is then a DESCENDANT of that node, and AncestorsAndSelf never reaches it. The upgrade silently returns null.

MEASURED ON ALL 16 INVOCATION SITES IN GITVERSION'S FIVE COMMAND FILES: 15 report invocationAncestorFound=False with foundNodeKind = a statement kind; exactly one (CalculateCommand.cs:23, whose argument list wraps onto line 24) reports foundNodeKind=InvocationExpression and binds. Same file, same member, same receiver expression `this.logger`, two lines apart: L23 upgrades to Microsoft.Extensions.Logging.ILogger at Semantic tier, L25 gets nothing.

THE CONTROLLED FIXTURE (one interface, one field, one DI-resolved impl, four spellings of the same call):
  A  `widget.Do();`                  one line, no this.        -> 2 edges [Syntactic]   (syntactic path, unaffected)
  B  `this.widget.Do();`             one line                  -> 0 edges
  C  `var v = this.widget.Do(\n);`   spans 2 lines, stmt starts at `var`  -> 0 edges
  D  `this.widget.Do(\n);`           spans 2 lines, stmt STARTS at the call -> 2 edges [Semantic]
So multi-line is not sufficient either: the rescue fires only when the invocation itself is the innermost node containing the line span, i.e. roughly when the statement begins at the invocation AND continues past that line. That is an accident of formatting, not a rule.

WHY IT MATTERS BEYOND GITVERSION. This is the only path that can give an invocation a Semantic receiver type. Everything downstream that keys on receiver type — dispatch detection (ISender/IMediator), the DI interface->impl route, HopThroughProperty, the Resolution tag on the edge — is degraded for the overwhelmingly common case of a one-line call statement. GitVersion's run reports "upgraded ... 1093 receiver" across 657 trees, so it is not zero; what it is, is arbitrary. Note the counting is also misleading: ReceiversResolved counts successful upgrades, so the metric cannot show the miss.

SISTER SITES TO CHECK, SAME MECHANISM: TryBindLocalDeclType, TryBindGenericArg and the Args[0] bind in the same file all relocate from an op Line. TryBindGenericArg and the arg bind go through the same FindNode+Ancestors shape and are likely to have the same hole; measure each rather than assuming.

FIX SHAPE (do not apply from this text - measure first): the op knows its line and its method name; find the invocation by DESCENDING from the statement node (root.FindNode(span).DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()) and disambiguate by method name + line, rather than ascending. Better still, have BodyFactExtractor record the invocation's own TextSpan on the op so relocation is exact and this whole class of bug disappears.

WATCH IT GO RED FIRST: a test whose call site spans two lines PASSES on the broken state (case D). The discriminating fixture is a ONE-LINE call statement asserting a Semantic-tier receiver type.

BLAST RADIUS WARNING FOR WHOEVER FIXES IT: this will move call-edge counts on every pole in the matrix. It is a batch-with-a-matrix-run change, not a surgical one. G5.2 deliberately does NOT depend on it (see the separate bug on `this.field` binding, which is surgical: 0 sites in CleanArchitecture / Hangfire / Polly / Serilog).
```

---

## MEDIUM — 16

<a id="1"></a>

### #1 · G1 · MCP QA harness scores a false 0/12 on the first run after any Core change (accepts a session before its analysis has a graph)

```text
MEASURED 2026-07-29 during G1.1, both directions.

SYMPTOM: `DevContext.Core.Tests.McpQaGateTests.McpQaHarness_Passes_Against_Dogfood` fails with "Assert.Contains() Failure: Sub-string not found / Not found: PASS". The generated eval-results/<date>/mcp-qa.md reads "Baseline: ? nodes, ? edges, ? entries" and Score 0/12 — every question fails with empty-analysis shapes ("resolve returned 0 candidates", "config returned 0 keys", "byKind=undefined"). It looks exactly like a catastrophic engine regression. It is not one.

PROOF IT IS THE HARNESS, NOT THE ENGINE: same binaries, no code change between the two runs.
  run 1 (cold snapshot, right after the Core edit)  -> 0/12,  "? nodes, ? edges, ? entries"
  run 2 (warm snapshot, minutes later)              -> 12/12, "426 nodes, 328 edges, 34 entries"
Run 2's baseline is identical to 2026-07-28's 12/12. Artifacts: eval-results/2026-07-29/g1.1-unit-tests.txt (the red), eval-results/2026-07-29/mcp-qa-rerun-warm.txt + mcp-qa.md (the green).

ROOT CAUSE: eval/mcp-qa/run.js `analyzeRepo` (~line 166). It fires `analyze` and then polls `list_sessions` every 500ms, taking the FIRST session whose status is "ready" or "done" — a session record that exists before its graph is populated. Meanwhile the `analyze` call itself has only the file's 45s generic timeout (line ~67); on a cold analyse it throws, and the catch at ~line 198 substitutes `{ handle, status: "ready" }`. So the harness proceeds against a live-but-empty session and marks the transport check "[x] Unprompted flush: analyze returned via polling workaround" while the analysis is still running.

WHY IT FIRES NOW AND NOT ON 07-28: snapshots are MVID-keyed, so EVERY Core edit invalidates all of them (PLAN.md §4 says cold re-analyses after a batch are expected). The dogfood repo therefore analyses cold on the first battery run after any Core change — which is exactly when the battery runs.

CONSEQUENCE: this gate produces a false red on the first post-Core-change battery of every session, and a session that trusts it will hunt a regression that does not exist. Conversely a real 0/12 regression is indistinguishable from this.

FIX (not applied — outside G1.1's scope, and this is a gate, so it wants its own commit): make the poll accept a session only once its analysis has actually produced a graph — e.g. require stats/nodeCount > 0 (or a genuine terminal status) rather than the presence of a "ready" row — and give the cold `analyze` call its own generous timeout instead of the shared 45s one. Do NOT fix it by pre-warming the snapshot; that hides the race rather than removing it.
```

<a id="2"></a>

### #2 · G1 · `entrypoints` names an entry one way and `get_context`/`trace` cannot resolve that name (TodoApi: "GET /todos" vs "&lt;lambda&gt; GET /todos/")

> **RE-MEASURED 2026-08-13 (T1.3) — DOES NOT REPRODUCE.** At HEAD the entry Title IS the nodeId key
> (`"POST /todos/"` / `EntryPoint:POST /todos/`); the minimal-API lambda titles are gone. 12/12 titles
> on TodoApi and 40/40 on eShop round-trip into BOTH `get_context` and `trace` —
> `eval-results/2026-08-13/t1-partial-truth/*/entry-roundtrip.json`. Closed by re-measurement, not by a
> new fix; the round-trip is now a standing check in `eval/mcp-qa/partial-truth.js` so it cannot
> silently regress. The detection half (#2 in D1.3, addressable entry names single-sourced) is separate
> and still open.

```text
MEASURED 2026-07-29 during G1.2, real MCP calls on eval-repos/TodoApi.

REPRO:
1. analyze(path: <repo>/eval-repos/TodoApi)
2. entrypoints(handle, limit: 20) — lists an entry whose displayed title is `GET /todos`.
3. get_context(handle, focus: "GET /todos") — returns the envelope "No context could be built for 'GET /todos'.", whose own candidate list then offers `Member:TodoApi.TodoApi::<lambda> GET /todos/`, `Member:TodoApi.TodoApi::<lambda> POST /todos/`, ...

So the tool that LISTS entries and the tool that CONSUMES an entry name disagree about how one entry is named: the inventory's Title is the minimal-API lambda form `<lambda> GET /todos/` (note the trailing slash), while `entrypoints` renders a cleaned `GET /todos`. An agent doing the obvious thing — read a name off `entrypoints`, hand it to `get_context` — gets nothing.

This is the same defect CLASS as R4 §1 item 2 (two tools, one name, different answers), but a different tool pair, so it is filed rather than folded into G1.2. Note EntryPointResolver's bare-route tier already normalizes routes for HttpEndpoint entries (GraphBuilder.NormalizeRoute handles the trailing slash) — it did not fire here, which suggests these minimal-API lambda entries are not classified HttpEndpoint, or carry no Route. MEASURE which before fixing.

Artifact: eval-results/2026-07-29/mcp-r4-after/getctx-entry-canary-todoapi.json (the envelope + its candidate list).

Suggested fix locus: the entry's Title/Route should be single-sourced so the displayed name is an addressable name — or EntryPointResolver should match the displayed form. Do not fix by loosening the resolver alone without checking what `entrypoints` actually renders.
```

<a id="3"></a>

### #3 · G1 · The DevContext.Server the MCP spawns EXITS (-1) instead of binding when the machine is loaded — the MCP then kills itself, so an agent's first call dies with no server at all

```text
MEASURED 2026-07-29 during G1.3, from the MCP's own log at %LOCALAPPDATA%/DevContext/logs/mcp-20260729_001.log.

REPRO (reliable on this machine): run the MCP QA drive while the machine is busy — e.g. `dotnet test DevContext.slnx --no-build --filter "Category!=Eval"`, which pulls Category=McpQa into a 674-test parallel run. (Note: that is NOT how eval/gates.ps1 verifies — see the ledger trap. The battery runs the drive alone, which is why the battery is green. But the load condition is real and a user can meet it.)

WHAT THE LOG SAYS, with the diagnostic field added in G1.3:
  05:13:11 [INF] Starting DevContext server: ...\DevContext.Server.exe
  05:15:12 [WRN] DevContext server did not become ready within 120s (exited=-1)
  05:15:14 [FTL] Cannot reach DevContext server at http://127.0.0.1:5179 (Unavailable, connection refused)
`exited=-1` is the point: the child is not SLOW, it is DEAD. Raising ServerShim's readiness budget from 30s to 120s (done in G1.3, and worth having on its own) does not help this case — it just waits longer for a process that has already gone. Program.cs then returns 1, so the MCP process exits and the client's `initialize` handshake is never answered.

CONSEQUENCE for a real user, not just the gate: the first MCP call on a machine that is also compiling gets the MCP killed outright, with an error that names the symptom ("Is the server running?") and not the cause.

WHAT IS NOT KNOWN and needs measuring before fixing: WHY the child exits -1. ServerShim starts it with UseShellExecute=true and captures no stdout/stderr, so the server's own startup failure is invisible. Candidates worth testing in order: (a) port 5179 contention with a dying predecessor, (b) environment variables the test host injects into the child (coverlet/testhost), (c) a genuine startup crash under memory pressure.

SUGGESTED FIX SHAPE: capture the child's stdout/stderr (or point it at the server's own log) so the exit reason is recorded, and have ServerShim retry a spawn that exits early rather than waiting out a budget for a dead process. Do NOT fix by raising the budget further — that is what G1.3 already tried, and the log shows why it is the wrong lever.
```

<a id="10"></a>

### #10 · G4 · read_source silently accepts an INVALID mode and falls back to a 20-line window — mode:"full" returned 20 of 147 lines with found:true and no complaint

```text
MEASURED 2026-07-29 in the G4.2 dogfood drive (Task 2, real development work on eval-repos/Hangfire). Raw dumps: eval-results/2026-07-29/mcp-dogfood/task2/raw/{016,019}-read_source.json — same node, same session, two mode values.

THE PAIR

  read_source(query:"PerformContext", mode:"full")
    -> {"found":true, "startLine":24, "endLine":43, "totalLines":147, "content":"..."}
    -> 20 lines of a 147-line file. No error, no warning, no note. `found:true`.

  read_source(query:"PerformContext", mode:"member")
    -> {"found":true, "startLine":30, "endLine":146, "totalLines":147, ...}
    -> the whole class. Correct.

THE CODE (src/DevContext.Mcp/DevContextTools.cs:1756):
    if (mode == "member" && resp.HasLineNumber) { ...full declaration through balancing brace... }
    else { var before = Math.Max(0, windowLines / 3); ... }   // <- ANY other string lands here
So `mode` has exactly two legal values and the else-branch swallows every other string as the
default window. "full", "all", "file", a typo, an empty string — all silently become a 20-line
window centred on the declaration line.

WHY THIS IS A WRONG ANSWER AND NOT A UX NIT. The response carries `totalLines:147` and
`startLine/endLine` bounding 20 of them, so the truth IS on the wire — but nothing flags that the
caller asked for something that does not exist. An agent that guesses "full" (a reasonable guess:
the tool's own summary line says "mode: window (default, windowLines lines around) | member (full
declaration body)", and the word "full" appears in that sentence describing the OTHER mode) gets a
truncated read presented as a successful one. In this drive it mattered directly: the 20-line window
stopped before `public IDictionary<string, object> Items { get; }`, which was the whole fact I was
after (F4 of the declared change spec). I only got it because a different call happened to include
it.

RELATED, SAME FAMILY, ALREADY FILED SEPARATELY: bug #9 (get_context's fillNote asserting
completeness while eliding a body). Both are the shape "the response is confident and incomplete,
and nothing on the wire says which". Bug #6 (trace answering a nodeId with an empty tree) is the
third. This class — not emptiness, not error, but confident partial truth — is now the most common
defect this strand has found.

WHAT TO MEASURE FIRST: whether any other tool parameter has the same shape (a string compared
against a literal with an unguarded else). `direction` on neighbors/impact ("in"/"out"/"up"/"down"),
`format` on trace ("compact"/...), `intent` on get_context, `mode` here. Grep for `== "` in
DevContextTools.cs and check each comparison for an else-branch that means "default" rather than
"reject". A cheap uniform fix is to validate the enum-ish parameters and return the existing error
envelope shape (which is good — it names candidates and gives an example) instead of defaulting.

WATCH IT GO RED FIRST: a test that calls read_source with a valid mode passes on the broken state.
The discriminating case is an INVALID mode value, asserting that the response is an envelope naming
the legal values rather than a successful window.
```

<a id="13"></a>

### #13 · G5 · `analyze --no-cache` does not invalidate the snapshot a later `query` reads — a changed repo returned the OLD graph with snapshotCache "HIT"

```text
MEASURED 2026-07-29 during G5.1, by accident, while iterating a fixture repo.

REPRO:
1. analyze <abs fixture path> --no-cache   -> prints "1 files - 13 nodes - 9 edges"
2. edit the fixture: add one class with one call site
3. analyze <same path> --no-cache          -> prints "1 files - 15 nodes - 13 edges"   (correct, sees the edit)
4. query graphdump --path <same path>      -> returns 13 nodes / 9 edges                (the PRE-EDIT graph)
5. query stats --path <same path>          -> "snapshotCache": "HIT - manifes", nodeCount 13, edgeCount 9

So the analyze that ran with --no-cache computed the right answer and the subsequent query served the stale one. Copying the fixture to a NEW directory and analysing there produced 15/13 from both verbs, which is how I noticed.

WHY IT IS WORSE THAN A CACHE NIT: the two verbs disagree with each other in the same session on the same path, and the disagreement is silent - graphdump carries no cache marker at all, and stats' marker says HIT, which reads like "we checked and it was current". Anyone measuring a change with the documented analyze-then-query pair (which is exactly what eval/graph-truth.ps1 does, graph-truth.ps1:195) can be shown pre-change numbers as post-change evidence. This program's own rule is "evidence or it did not happen"; this is a way to produce false evidence with correct commands.

WHAT TO MEASURE FIRST (do not fix by reading this): whether --no-cache means "do not READ the snapshot" only, or also "do not WRITE it" - if it skips the write, the stale snapshot survives and every later query hits it, which matches what I saw. Then check what the snapshot key actually covers ("manifes..." in the stats string suggests a manifest-derived key) and whether a source-file content change is part of it. Note the fixture edit changed a .cs file but not the .csproj.

WATCH IT GO RED FIRST: a test that analyzes, queries, and compares passes on the broken state if it never edits the repo between the two. The discriminating case is analyze -> EDIT A SOURCE FILE -> analyze -> query, asserting the query sees the edit.

WORKAROUND USED IN G5.1: analyse a fresh copy under a new directory name.
```

<a id="14"></a>

### #14 · G5 · A GENERIC command attribute is not recognised — `[Command&lt;ConfigCommand&gt;("init", ...)]` makes GitVersion's four SUB-commands invisible, so the command surface shows 5 verbs where the tool ships 9

```text
MEASURED 2026-07-29 during G5.1 on eval-repos/GitVersion (analyze --sln new-cli/GitVersion.slnx). Artifact: eval-results/2026-07-29/G5.1/raw/entries.json (5 entries) vs the repo source (9 [Command...] declarations).

CliCommandExtractor.FindCommandAttributeVerb (src/DevContext.Core/Extractors/Specific/CliCommandExtractor.cs:140-160) does:
    var name = attribute.Name.ToString();
    var leaf = name[(name.LastIndexOf('.') + 1)..];
    if (leaf is not ("Command" or "CommandAttribute")) continue;
For a generic attribute the syntax text is `Command<ConfigCommand>`, so leaf == "Command<ConfigCommand>" and the arm is skipped.

GitVersion declares its sub-commands exactly that way — the type argument names the PARENT verb:
  new-cli/GitVersion.Configuration/Init/ConfigInitCommand.cs:8      [Command<ConfigCommand>("init", ...)]
  new-cli/GitVersion.Configuration/Show/ConfigShowCommand.cs        [Command<ConfigCommand>("show", ...)]
  new-cli/GitVersion.Output/AssemblyInfo/OutputAssemblyInfoCommand.cs  [Command<OutputCommand>("assemblyinfo", ...)]
  new-cli/GitVersion.Output/Project/OutputProjectCommand.cs         [Command<OutputCommand>("project", ...)]
(and OutputWixCommand). GitVersion.Common.Command carries BOTH CommandAttribute and CommandAttribute`1, and both type nodes are in our graph — so the graph knows the generic attribute exists and the extractor still does not read it.

CONSEQUENCE, and it is the honesty class this strand keeps finding: the CLI's COMMAND SURFACE prints five flat verbs with no indication that anything was dropped, and the `cli.command-tree` insight asserts "Command tree: 5 CLI commands, 5 top-level groups" with confidence 0.8. The tree is the thing that is wrong: these four ARE the second level of it, and the type argument is the parent link that would build it.

FIX SHAPE: strip the type-argument list before the leaf comparison (use `attribute.Name` as GenericNameSyntax -> Identifier.ValueText, not ToString()), and carry the type argument as the parent verb so the command tree gets a real second level. Two structural facts, no name list: an attribute whose base identifier is Command/CommandAttribute AND a first argument that is a non-empty string literal — the existing rule, applied to the generic spelling too.

WATCH IT GO RED FIRST: a fixture with a non-generic [Command("x")] passes on the broken state. The discriminating fixture declares its verb with [Command&lt;Parent&gt;("x")] only.
```

<a id="15"></a>

### #15 · — · Conductor's gate runner keeps only the stdout tail, so a gate that dies mid-stream leaves NO diagnostic — the s16 fast-engine red arrived as three PASS lines and a banner

<a id="16"></a>

### #16 · G5 · AnalyzeCacheTruthTests.All_three_paths_report_themselves misses the snapshot rehydrate ~1-in-50 (third.Cached false) — cause UNKNOWN; the obvious dirty-fingerprint theory was tested and REFUTED

```text
OBSERVED (G5 s18, 2026-07-29): 1 failure in ~55 runs of DevContext.Server.Tests.AnalyzeCacheTruthTests.
Artifact: eval-results/2026-07-29/G5-s18/solo-12.txt.
  Assert.True() Failure at AnalyzeCacheTruthTests.cs line 78 (pre-fix numbering) = `Assert.True(third.Cached)`.
The third call re-analyses instead of rehydrating the snapshot the first call persisted. This is a DIFFERENT
defect from the DirectoryNotFoundException in A_rehydrate_reports_the_originals_instant_not_its_own, which was
root-caused and fixed in this session (process-wide DEVCONTEXT_CACHE_ROOT contention; 25/25 green after).
It survived the fix: it reproduced in the SOLO run where no concurrent collection existed.

THEORY TESTED AND REFUTED — do not spend a session re-deriving it:
  Theory: SnapshotCacheService.ComputeKeys (SnapshotCacheService.cs:115) appends
  GitHeadReader.ReadDirtyFingerprint (GitHeadReader.cs:34), which runs `git status --porcelain -uall` and hashes
  each listed file's mtime+length. `-uall` includes UNTRACKED files and git status is repo-wide, so
  tests/fixtures/ControllerApp keys on the whole DevContext2 working tree. Any repo write between call 1 and
  call 3 would flip a legitimate hit into a legitimate miss.
  HALF of that is MEASURED TRUE. eval-results/2026-07-29/G5-s18/probe-versionkey.ps1 analysed the fixture
  through the CLI into a private cache root, created ONE untracked file elsewhere in the repo, analysed again,
  and got a SECOND snapshot under a different key:
      d53218b4...-dirty-FC670061FC38EC7B-opt-372C9FDCC0AB.snap.json.gz
      d53218b4...-dirty-F8AA6627EFE09FE5-opt-372C9FDCC0AB.snap.json.gz
  The other half is FALSE. Two provocation runs (provoke-flakeB.ps1, provoke-flakeB2.ps1) drove the tests while
  a churn process rewrote an untracked repo file every 200-250ms — with the churn PROVEN to have run
  (churned=True on every iteration, mtime advancing) — and got 0 failures in 14 iterations. So repo churn does
  not make this test red, and the cause is still open.

NEXT SUSPECT (untested): SnapshotCacheService.SaveAsync swallows every failure into
SnapshotSaveResult.Fail(...) and EngineRunner only LOGS it (EngineRunner.cs:129-131), with the test's logger
factory configured to no sinks. A transient save failure (AV scan, file lock) would therefore be invisible and
would produce exactly this symptom. Cheapest next step: assert in the test that the first analyse actually
persisted a file before relying on it, or surface the save error on the outcome.

NOT REPRODUCIBLE ON DEMAND YET — treat as an open flake, not a fixed one.
```

<a id="17"></a>

### #17 · G6 · The engine ships TWO Member-title vocabularies: some Member nodes are titled "Owner.Method", others bare "Method" — same kind, same page

```text
MEASURED 2026-07-29 (G6.2 s21), artifact eval-results/2026-07-29/G6/label-mirror-fidelity.txt,
produced by eval-results/2026-07-29/G6/label-mirror-fidelity.ps1 (re-runnable, 6 poles).

The probe compared each graphdump node's TITLE against a single derivation rule applied to its ID.
It was written to validate a client-side mirror; what it actually measured is that the ENGINE's
GraphNode.Title is not one algebra for Member nodes:

  eShop        Member:eShop.Catalog.API.CatalogApi::GetAllItemsV1  -> title "CatalogApi.GetAllItemsV1"
  AutoMapper   Member:AutoMapper.Mapper::MapCore                   -> title "MapCore"
  MediatR      Member:MediatR.Mediator::Send                       -> title "Send"
  CleanArch    Member:...ContributorAggregate.Contributor::UpdatePhoneNumber -> title "UpdatePhoneNumber"

Totals across eShop/FluentValidation/AutoMapper/MediatR/CleanArchitecture/dotnet-podcasts:
  Member  343 owner-qualified / 627 bare      <-- two vocabularies
  Type   1405 short-name       /  50 other
  EntryPoint 83 / 58 (the key's scheme prefix "domain:"/"worker:"/"signalr:" is dropped by SOME
             producers and kept by others)
  Service/Store 100% consistent.

WHY IT MATTERS: a bare "Send" or "Validate" in a neighbours list, call stack or hub row does not say
whose. eShop's own nodes say "CatalogApi.GetAllItemsV1" three rows away. This is D-4's defect class
(one page, two vocabularies) in a field D-4 did not look at. The per-kind producer split is visible
in the artifact: eShop's owner-qualified members come from the EntryPoint builders
(GraphBuilder.EntryPoints/*, e.g. "${ep.HandlerType}.{methodName}"), the bare ones from
GraphBuilder.Seams.cs / call-graph member creation.

NOT FIXED IN G6.2 (whose bar is arity specifically, and which landed on its own evidence). Fix wants
one member-title helper next to SymbolCanon, used by every producer of a Member node.
```

<a id="18"></a>

### #18 · G6 · Type nodes are created from lambda/expression TEXT — a node whose id and title are a 20-line lambda body reaches the UI

```text
MEASURED 2026-07-29 (G6.2 s21), artifact eval-results/2026-07-29/G6/label-mirror-fidelity.txt.

Real Type nodes in the graph, id AND title verbatim:

  dotnet-podcasts  Type:options => options.AddFixedWindowLimiter("feeds", options =>\n{\n    options.PermitLimit = 5;\n ... })     (8 lines)
  CleanArchitecture Type:options =>\n    {\n      // Lifetime: Singleton is fastest per docs; ... }   (20 lines, comments and all)
  MediatR          Type:new System.IO.StringWriter()   /  Type:new MediatR.Tests.PipelineTests.Logger()
  FluentValidation Type:type.Assembly  /  Type:typeof(T).Assembly
  AutoMapper       Type:opt => opt.AddMaps(typeof(Source))

These are NodeKind.Type nodes whose Title is the raw source text of a DI-registration lambda or an
expression, so they render as multi-line "type names" in any list, canvas label or neighbours row
that shows a title. ~50 across 6 poles (see the Type MISMATCH column).

Cause is upstream of the graph: a DI/registration detection recorded the ARGUMENT EXPRESSION as a
type name and GraphBuilder trusted it (GraphBuilder.Seams.cs:83/87 title = first.ServiceType /
first.ImplementationType, straight from the detection). GraphBuilder.Seams.cs already filters some of
these shapes for the DirectBinding path ("sp =>", "_ =>", "(", "GetRequiredService") -- the filter is
incomplete and is not applied on every path that creates a Type node from detection text.

NOT FIXED IN G6.2 (bar is arity). Related to the two-member-vocabularies bug filed alongside it.
```

<a id="19"></a>

### #19 · — · Atlas/map states a FOURTH service count: STYLE evidence says "6 runnable web services" where the per-service breakdown lists 5 and the canvas draws 5 — same page, same scope, two counters

<a id="20"></a>

### #20 · G7 · A library's Atlas counts an auxiliary demo executable as a SERVICE: AutoMapper reads '1 services (1 drawn)' and the per-service breakdown names TestApp

_Found in session #23._

<a id="22"></a>

### #22 · G10 · graph.orphans dead-code insight is dormant: its Semantic-share floor (0.5) is unreachable on 11/11 poles (measured 2026-08-02, G10.1)

_Found in session #28._

<a id="23"></a>

### #23 · G10 · L3.4 hub-scope broadening never fires: sparseGraph=false + hubScopeNodes=0 on 11/11 poles including its own trigger population (Dapper/Serilog/MahApps/MediatR); identity-strip's sparse line has never rendered (G10.1)

_Found in session #28._

<a id="24"></a>

### #24 · G10 · Deep-spine ratio is saturated (1.000 on 5/11 poles, 0.96-0.98 on the rest): the report prints it as coverage but it separates no repo (G10.1)

_Found in session #28._

<a id="25"></a>

### #25 · G10 · Engine ships two definitions of a verified edge: GraphStats/SeamStat approx=Syntactic only (so Join counts as verified) while GraphOrphansSource counts Semantic only; Resolution.Join is also the enum default (G10.1)

_Found in session #28._

---

## LOW — 1

<a id="4"></a>

### #4 · G2 · The desktop MCP page keeps its own tool list; it advertised `search`, a tool the MCP has never exposed

```text
src/DevContext.App/src/app/features/pages/mcp-page.ts — the "Try a tool" sandbox labels gRPC RPC probes with MCP tool NAMES from its own literal array (`availableTools`). That makes it a third hand-maintained copy of the menu, and it was already drifted before G2.1 touched anything:

  - `search` is not an MCP tool and never has been. The MCP calls it `find`. The row mapped to the
    `searchNodes` RPC, so it worked — under a name no agent could use.
  - `insights` mapped to `getStats` in the same switch, i.e. the app had independently worked out
    that insights and stats are one call. G2.1 folded that tool away.

G2.1 corrected both labels (search -> find, insights dropped) as a same-commit truth fix, but the
structural problem is untouched: the app speaks gRPC, not MCP, so it has no way to check its labels
against the real menu the way UnknownToolHandler now does. Options: serve the tool catalog over
gRPC (a `ListTools` RPC or a field on Ping), or generate the list at build time from the
[McpServerTool] methods. Until then this list drifts silently — the failure is a label, so nothing
errors.

Note: eval/contract-sweep.ps1 cannot catch this class either. The sweep asks whether a proto field
has a reader; this is a hand-written string that names a tool, with no proto field involved. Same
family as S10's Insight.Severity find (the field IS read, with the wrong key).
```
