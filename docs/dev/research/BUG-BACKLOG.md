# Bug backlog — findings filed, not fixed

> Exported 2026-08-02 from the graph-v2 autonomous run (`run bae63ba0` — 28 sessions,
> 22/22 checkpoints, full battery green at every stage close). The source of record was
> `.conductor/run.db`, **which is gitignored, so this file is the durable copy.**
>
> These are defects the run *measured* and deliberately filed instead of fixing — either
> because the fix was a product decision rather than a threshold correction, or because it
> fell outside the stage that found it. Each carries the evidence path that proves it.
> None is a regression.

> **Second source, 2026-08-13 (`run 8faf849d`, stage N0).** `docs/dev/research/STUDIO-MCP-AUDIT-2026-08-13.md`
> §3.F listed 16 truth defects across Context Studio and the MCP page. Ten were fixed in N0 and are
> recorded at the bottom of this file under **[FIXED in N0](#fixed-n0)** — they are *not* open and are
> kept only so the inventory reads whole. The remaining six were filed as **#26–#31**, each with
> its locus **re-measured on 2026-08-13** rather than copied from the audit prose. **#32** is the
> engine bug N0.1 found while fixing §3.F.3/4 and did not fix.

**Filed 2026-08-02: 24 open — 7 high, 16 medium, 1 low.** Plus #26–#32 from the second source above.

**Reconciled 2026-08-14, BOTH pre-release runs now folded in: 23 FIXED · 2 RETIRED ·
1 HELD-with-premise-refuted · 5 still open (all medium), and of those 5 only four belong to this
repo — the fifth is Conductor's.**

**Reconciled 2026-08-27, the drive-fix program: #33 · #34 · #36 FIXED, #35 FIXED-as-filed with
its residual re-filed as #37. New filings #37–#39 (1 high, 1 medium, 1 low) from the integration
battery and the post-fix re-measure.**

> **Third source, 2026-08-26 (the Book2Course unseen-repo hand drive).**
> `eval-results/2026-08-26/unseen-drive-Book2Course/DRIVE.md` drove five pre-fixed questions
> against a repo the engine had never seen and found one decisive win (impact) and five defects.
> F2 (the auth-group miss, release-blocking) was fixed the same day (`09934f2`, merged `5e42c46`)
> and carries its FIXED block in DRIVE.md itself. The remaining four are filed as **#33–#36** —
> three high, one low — at [Filed 2026-08-26](#filed-2026-08-26) at the foot of this file. The
> drive's own license line binds: **the pre-registered re-probe must not run until F1–F4 are
> filed and fixed.**
>
> **Reconciled 2026-08-27** by the drive-fix program (`fix/f5-usages-projection` ·
> `fix/f3-options-config` · `fix/f4-seam-transport` · `fix/f1-undeclared-members`, integrated and
> battery-repaired on `fix/mcp-drive-integration`, merged to develop): **#33 · #34 · #36 FIXED,
> #35 FIXED as filed with its residual re-filed as #37** — every verdict re-measured on the same
> repo with the same recorded call batches
> (`eval-results/2026-08-26/unseen-drive-Book2Course/remeasure-post-fix/REMEASURE.md`). Per-item
> reasoning: [§ Reconciliation 2026-08-27](#reconciliation-2026-08-27). Three new findings from
> the integration battery and the re-measure are filed as **#37–#39** at
> [Filed 2026-08-27](#filed-2026-08-27). The license line's letter is met — F1–F4 filed and
> fixed — but the re-measure's F4 bar still cannot pass end-to-end on this repo until #37 lands
> (`seam(BuildCoordinator → IngestStage)` now dies one hop PAST the fixed port); whether to run
> the re-probe with that known and filed stays the owner's call.

> **On the two runs.** The pre-release program ran as two parallel conductor runs against one
> backlog: **run A** (engine, `conductor.engine.plan.json`, stages T1 · V1 · E1 · D1 · R1 · A1) took
> the G-stage engine items, and **run B** (desktop, `conductor.desktop.plan.json`, stages N0–N4 · M1)
> took the Studio/MCP-page items. Each reconciled only its own half and explicitly declined to claim
> the other's closures. **This file is the merge of the two**, so every item below now carries a
> settled verdict — including `#4`, which run A recorded as "Run B's" and run B closed in N4.3.

Statuses below are the reconciliation verdict, each with the evidence that closed it. Full per-item
reasoning: [§ Reconciliation 2026-08-14](#reconciliation-2026-08-14) for run A's items, and the
**FIXED in …** sections at the foot of the file for run B's. The prose sections further down
are the ORIGINAL 2026-08-02 filings and are deliberately left unedited — a filing rewritten after the
fix stops being a record of what was measured.

| # | Sev | Stage | Status (2026-08-14) | Title |
|---|-----|-------|---------------------|-------|
| [#5](#5) | high | G4 | **FIXED** — T1.1 | Every one of the 22 MCP tools ships with an EMPTY description — 31 carefully written XML doc summaries exist in the source and none of them reach the wire |
| [#6](#6) | high | G4 | **FIXED** — T1.3, gated T1.4 | trace() handed a nodeId returns found:true with an EMPTY tree titled "Type: Type" — and its own error envelope tells the agent to pass a nodeId |
| [#7](#7) | high | G4 | **FIXED** — E1.3 (+ V1.3 rider) | A METHOD is registered as a Type node and 26 BCL System.Type references bind to it — the mis-bound node is stats' #5 "wiring hub" on Hangfire |
| [#8](#8) | high | G4 | **FIXED, residual re-filed** — E1.3 → new #5 | Calls inside a lambda argument produce NO edge — the actual storage write of Hangfire's enqueue path is invisible, and the trace of the writing type looks complete without it |
| [#9](#9) | high | G4 | **FIXED** — T1.3, gated T1.4 | get_context's fillNote says the pack "already contains everything reachable from this focus" while it is eliding the body the agent asked for — measured false at two budgets on the same focus |
| [#11](#11) | high | G4 | **FIXED** — E1.1 | Static calls with a TYPE-NAME receiver produce no call edge — DevContext's own body-fact walker has 0 in-edges in DevContext's own graph, and this REFUTES bug #8's lambda explanation |
| [#12](#12) | high | G5 | **FIXED** — E1.2 | The semantic receiver-type upgrade misses every invocation whose statement fits on one line — TryBindReceiverType relocates by LINE SPAN and then searches ANCESTORS, so the invocation is a descendant and is never found |
| [#1](#1) | medium | G1 | **FIXED** — G1.3, re-verified in source 2026-08-14 | MCP QA harness scores a false 0/12 on the first run after any Core change (accepts a session before its analysis has a graph) |
| [#2](#2) | medium | G1 | **FIXED, both halves** — T1.3 + D1.3 | `entrypoints` names an entry one way and `get_context`/`trace` cannot resolve that name (TodoApi: "GET /todos" vs "&lt;lambda&gt; GET /todos/") |
| [#3](#3) | medium | G1 | **OPEN** | The DevContext.Server the MCP spawns EXITS (-1) instead of binding when the machine is loaded — the MCP then kills itself, so an agent's first call dies with no server at all |
| [#10](#10) | medium | G4 | **FIXED** — T1.3, gated T1.4 | read_source silently accepts an INVALID mode and falls back to a 20-line window — mode:"full" returned 20 of 147 lines with found:true and no complaint |
| [#13](#13) | medium | G5 | **OPEN** — deferred register | `analyze --no-cache` does not invalidate the snapshot a later `query` reads — a changed repo returned the OLD graph with snapshotCache "HIT" |
| [#14](#14) | medium | G5 | **FIXED** — D1.3 | A GENERIC command attribute is not recognised — `[Command&lt;ConfigCommand&gt;("init", ...)]` makes GitVersion's four SUB-commands invisible, so the command surface shows 5 verbs where the tool ships 9 |
| [#15](#15) | medium | — | **OPEN — not this repo** | Conductor's gate runner keeps only the stdout tail, so a gate that dies mid-stream leaves NO diagnostic — the s16 fast-engine red arrived as three PASS lines and a banner |
| [#16](#16) | medium | G5 | **OPEN** — deferred register | AnalyzeCacheTruthTests.All_three_paths_report_themselves misses the snapshot rehydrate ~1-in-50 (third.Cached false) — cause UNKNOWN; the obvious dirty-fingerprint theory was tested and REFUTED |
| [#17](#17) | medium | G6 | **FIXED** — V1.2 | The engine ships TWO Member-title vocabularies: some Member nodes are titled "Owner.Method", others bare "Method" — same kind, same page |
| [#18](#18) | medium | G6 | **FIXED** — V1.3 | Type nodes are created from lambda/expression TEXT — a node whose id and title are a 20-line lambda body reaches the UI |
| [#19](#19) | medium | — | **FIXED** — D1.3 | Atlas/map states a FOURTH service count: STYLE evidence says "6 runnable web services" where the per-service breakdown lists 5 and the canvas draws 5 — same page, same scope, two counters |
| [#20](#20) | medium | G7 | **FIXED** — D1.3 | A library's Atlas counts an auxiliary demo executable as a SERVICE: AutoMapper reads '1 services (1 drawn)' and the per-service breakdown names TestApp |
| [#22](#22) | medium | G10 | **RETIRED — premise INVERTED** — R1.1 | graph.orphans dead-code insight is dormant: its Semantic-share floor (0.5) is unreachable on 11/11 poles (measured 2026-08-02, G10.1) |
| [#23](#23) | medium | G10 | **HELD, premise REFUTED; residual re-filed** — R1.1 → new #18 | L3.4 hub-scope broadening never fires: sparseGraph=false + hubScopeNodes=0 on 11/11 poles including its own trigger population (Dapper/Serilog/MahApps/MediatR); identity-strip's sparse line has never rendered (G10.1) |
| [#24](#24) | medium | G10 | **RETIRED — saturated** — R1.1 | Deep-spine ratio is saturated (1.000 on 5/11 poles, 0.96-0.98 on the rest): the report prints it as coverage but it separates no repo (G10.1) |
| [#25](#25) | medium | G10 | **FIXED** — V1.1 | Engine ships two definitions of a verified edge: GraphStats/SeamStat approx=Syntactic only (so Join counts as verified) while GraphOrphansSource counts Semantic only; Resolution.Join is also the enum default (G10.1) |
| [#4](#4) | low | G2 | **FIXED** — N4.3 (run B) | The desktop MCP page keeps its own tool list; it advertised `search`, a tool the MCP has never exposed |
| [#26](#fixed-n12) | high | N0 | **FIXED** — N1.2 | Pins are advertised by three surfaces and read by none: `TrailStore.pins()` reached a counter and a colour and nothing else |
| [#27](#fixed-n1) | medium | N0 | **FIXED** — N1.1 | Body toggles were cosmetic — an eye icon and an opacity; `bodyEnabled` never reached the wire while the pill claimed "All bodies hidden" |
| [#28](#fixed-n1) | medium | N0 | **FIXED** — N1.1 | The verification ledger verified a pack that was never built — full budget per focus, all sections, N RPCs per card edit |
| [#29](#fixed-n1) | medium | N0 | **FIXED** — N1.1 | Studio cards were never invalidated — the file contained no `effect()`, so a re-analyze left cards holding node ids from the previous graph |
| [#30](#fixed-n2) | medium | N0 | **FIXED** — N2.1 | The zero-entry empty state told the user to pick types from an omnibox that searches entries only — unsatisfiable in the exact state that printed it |
| [#31](#fixed-n2) | medium | N0 | **FIXED** — N2.1 | The `usage` section was built by every symbol-rooted focus and then discarded — no card type mapped to it, so Studio could never show the answer `get_context` gives the agent |
| [#32](#32) | medium | N0 | **OPEN** | `AllocateProportionalBudgets` can hand the last focus a NEGATIVE budget |

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

## MEDIUM — 17

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

> **FIXED 2026-08-14 (D1.3, commit 796843f).** The filed mechanism is confirmed verbatim; the leaf is now read off the name SYNTAX and the type argument rides out as `CliCommandDetection.ParentCommandType`, which `CliCommandEntryPointBuilder.VerbPath` turns into the two-level title `config init (ConfigInitCommand)`. Red-first log: `eval-results/2026-08-14/d1-filed/bug14-red-run.txt`.

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

> **FIXED 2026-08-14 (D1.3, commit 1122d02).** `CountRunnableWebProjects` was a third population with no production filter; it now iterates `ServiceBoundaryInference.RunnableProjects`, so the quoted count is a subset of the drawn services by construction. Red-first log: `eval-results/2026-08-14/d1-filed/bug19-bug20-red-run.txt` ("4 runnable web services" above a breakdown of 3).

<a id="20"></a>

### #20 · G7 · A library's Atlas counts an auxiliary demo executable as a SERVICE: AutoMapper reads '1 services (1 drawn)' and the per-service breakdown names TestApp

> **FIXED 2026-08-14 (D1.3, commit 1122d02).** `ArchetypeDetector.ExecutablesAreAuxiliaryToALibrary` exposes the auxiliary-exe verdict the archetype ladder already made, and `RunnableProjects` reads it. Repo-level by construction, never the per-project half. Red-first log shows the exact symptom: `Collection: ["TestApp"]`.

_Found in session #23._

<a id="22"></a>

### #22 · G10 · graph.orphans dead-code insight is dormant: its Semantic-share floor (0.5) is unreachable on 11/11 poles (measured 2026-08-02, G10.1)

_Found in session #28._

**RESOLVED 2026-08-14 (R1.1) — RETIRED, and the premise above inverted first.** E1 lifted the
Semantic share 8x (eShop 0.057 → 0.438) and the floor became reachable: the insight fired on 2 of 12
poles and every claim was false — 5/5 on eshop-microservices (four `: ICarterModule` HTTP endpoints
and a `Decorate<>`d repository), 3/5 outright on VerticalSlice with 2 unresolvable. Measured
precision 0/10. No floor value fixes it: the types are live by *registration*, which a call-edge
share cannot observe. Source deleted; `eval/lens-audit.ps1` now fails if the id ever re-emits.
Evidence: `eval-results/2026-08-14/r1-metrics/R1.1-EVIDENCE.md`.

<a id="23"></a>

### #23 · G10 · L3.4 hub-scope broadening never fires: sparseGraph=false + hubScopeNodes=0 on 11/11 poles including its own trigger population (Dapper/Serilog/MahApps/MediatR); identity-strip's sparse line has never rendered (G10.1)

_Found in session #28._

**REFUTED 2026-08-14 (R1.1) — it fires.** Hangfire reports `sparseGraph=true`, `hubScopeNodes=34`;
the 11-pole G10 set simply never contained a repo that clears both gates. The broadening is kept
unchanged with the new measurement at the call site. The residual — 3 of the 4 gate-passing poles
still exit at `k < 5` because `model.CallEdges` spans <10 types on them while the graph carries
hundreds of Calls edges from the BodyFacts path — is **conductor bug #18**.
Evidence: `eval-results/2026-08-14/r1-metrics/R1.1-EVIDENCE.md` §4.

<a id="24"></a>

### #24 · G10 · Deep-spine ratio is saturated (1.000 on 5/11 poles, 0.96-0.98 on the rest): the report prints it as coverage but it separates no repo (G10.1)

_Found in session #28._

**RESOLVED 2026-08-14 (R1.1) — RETIRED.** Re-measured on 12 poles post-E1: 1.000 on eight, 0.982
(eShop) and 0.994 (FastEndpoints) on the two others, 0 only where entries == 0 (a divide artifact).
Every before/after pole reads exactly what it read on 2026-08-02, so E1's 8x Semantic lift moved it
not at all. The bar was NOT raised — that is a question about the step distribution, which no
surface exposes. Removed from `GraphStats`/`GraphQuery`, the report's "Deep spine (>=2)" row, and the
CLI `query stats` payload. No proto field and no golden pinned it.
Evidence: `eval-results/2026-08-14/r1-metrics/R1.1-EVIDENCE.md` §5.

<a id="25"></a>

### #25 · G10 · Engine ships two definitions of a verified edge: GraphStats/SeamStat approx=Syntactic only (so Join counts as verified) while GraphOrphansSource counts Semantic only; Resolution.Join is also the enum default (G10.1)

_Found in session #28._

---

<a id="32"></a>

### #32 · N0 · `AllocateProportionalBudgets` can hand the last focus a NEGATIVE budget

```text
Found by N0.1 while fixing §3.F.3/4 (conductor bug #1, 2026-08-13); filed, not fixed.

src/DevContext.Core/Graph/ContextPackBuilder.cs:982 AllocateProportionalBudgets — every non-last
focus is given max(minPerEntry=200, its proportional share) and `remaining` is decremented by the
share that was actually handed out; the LAST focus is then given whatever `remaining` holds. When
the min-floor has been applied often enough (many focuses, small total budget), the floor payouts
overrun the ceiling and `remaining` goes negative — the last focus is allocated a negative token
budget.

BuildMulti's caller does not clamp on the way in (:546 `focusBudgets.GetValueOrDefault(focus,
minEntryBudget)`), so the negative value reaches BuildSections. N0.1 clamped the ACCOUNTING it
added (:551 `allocatedTokens += Math.Max(0, budget)`) so the reported allocation cannot go
negative, but deliberately did not change the allocation itself — that moves pack content and is
a golden-affecting engine change, not a truth fix.

REPRO SHAPE: any distribution where the non-last shares — each max(200, proportional) — sum past
the total. Two ways there: many small focuses (the 200 floor paid n−1 times), or one dominant
focus taking most of the budget with a handful of floor-paid tails behind it (e.g. 7000 + 6 × 200
against 8000).

Re-measured 2026-08-14 (Z1.1): STILL OPEN, allocation logic unchanged (`ContextPackBuilder.cs:1256`
— the last focus is still handed `remaining` with no clamp). One input did change: N2.2 reconciled
the UI's 4000 away, so the pack default is now 8000 (`ContextPackBuilder.DefaultBudgetTokens`),
which raises the many-small-focuses boundary from ~21 to ~41 focuses. The skewed-distribution
shape is unaffected by the default.
```

---

## LOW — none open

The one low item, **#4** (the desktop MCP page's hand-kept tool list), was closed by N4.3 on
2026-08-14 — see [FIXED in N4.3](#fixed-n4).

---

<a id="reconciliation-2026-08-14"></a>

## Reconciliation 2026-08-14 — what the pre-release run closed, and how it was proven

Run A of the pre-release program (`conductor.engine.plan.json`, stages T1 · V1 · E1 · D1 · R1 · A1)
was scoped straight off this file. This section is the audit trail: for each item, the checkpoint
that closed it and the artifact that proves it. Nothing here re-states a fix from a commit message —
every row names a file you can open.

Two entries did NOT survive contact and are recorded as such, because a backlog that only ever
converts OPEN → FIXED is a backlog nobody re-measured.

### Closed by measurement

| # | Closed by | Evidence |
|---|---|---|
| #5 | T1.1 — every tool description reaches the wire, mechanism verified against what the MCP SDK actually reads | `eval-results/2026-08-13/t1-wire-truth/T1.1-EVIDENCE.md` |
| #6 | T1.3 — `trace(nodeId)` routes through the same resolver `get_context` uses; a `found:true` trace has steps or says why not | `eval-results/2026-08-13/t1-partial-truth/T1.3-EVIDENCE.md` |
| #9 | T1.3 — `fillNote` names the elision AND the `budgetTokens` lever; no pack claims completeness while eliding | `eval-results/2026-08-13/t1-partial-truth/T1.3-EVIDENCE.md` |
| #10 | T1.3 — out-of-range enum values are rejected with an error envelope instead of a silent fallback | `eval-results/2026-08-13/t1-partial-truth/T1.3-EVIDENCE.md` |
| #2 | T1.3 (agent path: entry names round-trip into `get_context`/`trace`) **and** D1.3 (detection half: addressable entry names single-sourced) — both halves, in different stages | `.../T1.3-EVIDENCE.md` + `eval-results/2026-08-14/d1-filed/D1.3-EVIDENCE.md` |
| #25 | V1.1 — one verified-edge definition, stated once, read by every surface | `eval-results/2026-08-13/v1-vocabulary/EVIDENCE.md` |
| #17 | V1.2 — one member-title helper next to SymbolCanon; the owner-qualified/bare split gone on the six poles it was measured on | `eval-results/2026-08-13/v1-vocabulary/MEMBER-TITLE-EVIDENCE.md` |
| #18 | V1.3 — standing invariant, landed red-first: lambda/expression text never becomes a node title on any path | `eval-results/2026-08-13/v1-invariants/INVARIANT-EVIDENCE.md` |
| #11 | E1.1 — static type-name-receiver calls produce edges; the dogfood invariant (DevContext's own helper layer has in-edges in DevContext's own graph) is now in the battery, proven red first | `eval-results/2026-08-13/e1-edges/E1.1-EVIDENCE.md` |
| #12 | E1.2 — fixed at the shape, not the symptom: BodyOp records the invocation's own TextSpan, killing the whole relocate-by-line class including the TryBindLocalDeclType / TryBindGenericArg / args-bind sister sites | `eval-results/2026-08-13/e1-span/E1.2-EVIDENCE.md` |
| #7 | E1.3 (explicit-interface / BCL-collision Type node) + V1.3's rider invariant (no node carries kind Type with a member id) | `eval-results/2026-08-13/e1-typenode/E1.3-EVIDENCE.md` |
| #8 | E1.3 — **re-measured first.** #11 refuted this entry's stated mechanism (it was never "lambdas"), so it was fixed against the true cause and the leftover re-filed rather than quietly widened | `eval-results/2026-08-13/e1-typenode/E1.3-EVIDENCE.md` · residual = new #5 below |
| #14 | D1.3 — generic command attribute: type args stripped in the leaf comparison, the type arg carried as parent | `eval-results/2026-08-14/d1-filed/D1.3-EVIDENCE.md` |
| #19 | D1.3 — one source of truth for "what is a service"; the fourth counter is gone | `eval-results/2026-08-14/d1-filed/D1.3-EVIDENCE.md` |
| #20 | D1.3 — same single source; a library's auxiliary demo executable is no longer counted as a service | `eval-results/2026-08-14/d1-filed/D1.3-EVIDENCE.md` |
| #1 | G1.3, before this run. **Verified in source 2026-08-14, not taken on trust:** all three named causes are gone from `eval/mcp-qa/run.js` — `analyze` owns a 600s budget (L192), the readiness poll matches a session with `nodes > 0` (L209), and the `status(handle:"")` cross-repo retarget is removed | `eval/mcp-qa/run.js:176-228` |

### Re-measured, and the premise did not hold

| # | Verdict | What the re-measurement found |
|---|---|---|
| #22 | **RETIRED** (R1.1) | The entry says the insight is *dormant* because a Semantic-share floor is unreachable. Post-E1 that inverted: it FIRES on 2 of 12 poles — and **measured precision is 0/10**, hand-verified in the repos. The four eShop "orphans" are `: ICarterModule` HTTP endpoints registered by Carter's assembly scan; `CachedBasketRepository` is a `Services.Decorate<>` registration. No value of the floor separates these, because the reason they are live is not a call edge. Retired with source + tests deleted and a retirement ratchet on the id. |
| #23 | **HELD, premise REFUTED** (R1.1) | The entry says hub-scope broadening *never fires*. It does: Hangfire measures `sparseGraph=true`, `hubScopeNodes=34`. The gate is unchanged; the real residual — hubs sized off a channel post-E1 has left behind — is re-filed as new #18 below. |
| #24 | **RETIRED** (R1.1) | Still saturated, identical to the 2026-08-02 measurement: it separates no repo. Removed from GraphStats, GraphQuery, the report row and the CLI payload rather than left printing as coverage. |

Full numbers for all three: `eval-results/2026-08-14/r1-metrics/R1.1-EVIDENCE.md`. The sweep
instrument it was measured with (12 poles, private cache root, one `query stats` per pole) is
`eval-results/2026-08-14/r1-metrics/r1-metric-sweep.ps1` + `r1-aggregate.mjs` — reusable for any
later threshold work. Note six of the original G10 eleven poles (Dapper, Serilog, MahApps.Metro,
GitVersion, DntSite, wolverine) are no longer on the measuring machine; the surviving five are
eShop, dotnet-podcasts, CleanArchitecture, MediatR and self.

### Still open — 5, and only four are this repo's

Reconciled across BOTH runs (see the merge note at the top of this file). `#4` is no longer on this
list: run A left it open as "Run B's", and run B closed it in [N4.3](#fixed-n4). `#32` joins it from
run B's filings — its locus is engine code, so it belongs here rather than in the desktop's register.

| # | Sev | Why it is still open |
|---|---|---|
| #3 | medium | Never re-measured by this run; the load-dependent server exit was not reproduced or investigated. Genuinely open. |
| #13 | medium | Parked in the plan's deferred register — "fold into whichever session next touches the cache". Not attempted. |
| #16 | medium | Same register, same reason: a ~1-in-50 flake whose obvious cause was already tested and refuted. Not attempted. |
| #32 | medium | **Run B's filing, engine locus.** `AllocateProportionalBudgets` can hand the last focus a NEGATIVE budget — found by N0.1 while fixing §3.F.3/4 and deliberately not fixed there. |
| #15 | medium | **Not this repo.** It is a defect in Conductor's own gate runner (stdout-tail-only), fixable only there. |

---

<a id="fixed-n0"></a>

## FIXED in N0 — the other ten §3.F items (not open; kept so the inventory reads whole)

Stage N0 of the 2026-08-13 pre-release run ("the truth batch — no decisions") closed ten of the
sixteen. They are listed here with their fix locus so that a later reader who finds the audit
first does not re-file them.

| §3.F | Defect | Fixed in | Fix locus |
|------|--------|----------|-----------|
| 3 | Multi-entry section merge dropped `SourceLocations`/`Verified`/`Approx`, so primary-path cards lost provenance | N0.1 · `36bf916` | `ContextPackBuilder.cs:583-601` — the merge branch now concatenates locations (deduped, capped at 20) and SUMS the trust counters |
| 4 | `allocated_tokens` ≡ budget: the Studio header printed one number under two labels | N0.1 · `36bf916` | `ContextPackBuilder.cs:539-552` — allocated is now measured (the share that reached a focus which produced sections), clamped at 0 |
| 7 | Studio copy/save bypassed `clipboard.ts`; toasts fired before/regardless of outcome | N0.1 · `36bf916` | Studio copy paths go through the app clipboard helper and the toast awaits the result — pinned by `context-studio.spec.ts` (`copy-context`, `card-copy`) |
| 9 | `getMcpStatus()` called `StartMcp` — a status READ that mutated what it measured | N0.2 · `98c5067` | A real `GetMcpStatus` message (`telemetry_streaming`/`observer_count`); pinned by `mcp-page.spec.ts` "reads status WITHOUT starting anything" |
| 10 | "ships with the desktop installer" was false; all three host snippets named a command that does not resolve | N0.2 · `98c5067` | Snippets name `devcontext-mcp.exe`; the setup note no longer claims the installer ships it. Spec: "host snippets do not promise a command that will not resolve" |
| 11 | Copy-label sniffing always marked the VS Code card "Copied!" | N0.2 · `98c5067` | Per-host `copy-snippet-<host>` state. Spec: "Copy marks the card that was clicked" |
| 12 | Feed: "Total" counted rows the filter hid; timestamps were client-side | N0.2 · `98c5067` | The counter reads "Shown: N tok" and follows the filter; rows carry the WIRE `timestamp_utc_ms`. Spec: "the feed total counts the rows on screen…" |
| 13 | Sessions: `edges`/`entries` mapped but never rendered; `from_cache`/`analyzed_at` unused, so the shown age lied after a rehydrate | N0.2 · `98c5067` | Both columns render; a separate analysis-age cell with a "(cached)" marker and a title explaining the rehydrate. Spec: "sessions render the analysis age…" |
| 14 | Dead state (`mcpStateSynced`, `DevContextApi._mcpRunning`, feed `session`/`bytes`) and three silent catch-alls with no user-visible signal | N0.2 · `98c5067` | Dead fields deleted; poll/stream/status failures now surface (`mcp-status-error`, `sessions-error`, `feed-error`). Two specs pin the error paths |
| 16 | Neither page had a spec file; the page `data-testid`s were referenced by nothing | N0.2 + N0.3 | `mcp-page.spec.ts` (10 tests) and `context-studio.spec.ts` cover both pages; every `data-testid` on the two pages that the audit named is now asserted by a real spec |

**Nothing is still open from that inventory.** 3.F.2, 3.F.5 and 3.F.6 closed in N1.1 and 3.F.1
(pins) in N1.2; 3.F.8 and 3.F.15 in N2.1; the tool-list half of 3.F.12 in N4.3. See the sections
below, and the per-item status markers in `STUDIO-MCP-AUDIT-2026-08-13.md` §3.F.

<a id="fixed-n1"></a>

## FIXED in N1.1 - the three 3.F items that needed a wire or product decision

Stage N1.1 of the same run ("Studio truth pass") closed the three whose fix was blocked on a
decision rather than on effort. The decisions are recorded in `STUDIO-MCP-AUDIT-2026-08-13.md`
section 4 wire item 4 and section 8.

| # | 3.F | Defect | Decision taken | Fix locus |
|---|-----|--------|----------------|-----------|
| 28 | 5 | The verification ledger verified a pack that was never built - full budget per focus, all sections, N RPCs per card edit | **Verification MOVES INTO `GetContextPack`'s response** rather than `VerifyContextRequest` gaining cards/intent: `ContextPackVerifier.Verify()` is a pure function of the sections, and `BuildMulti` already holds the ones it kept, so a divergence becomes impossible to write instead of merely fixed. `VerifyContext` stays the single-focus RPC for MCP's `verify_context`, which reads every field of its response. | `ContextPackBuilder.cs` BuildMulti tail (merge by section key then `ContextPackVerifier`); `ContextPackResponse.verification/any_stale/analyzed_git_head/current_git_head`; `context-studio.ts` `readVerification()` replaces `verifyPack()`. Dead `checkedAt` now renders (`verification-checked-at`). Gates: `ContextPackLedgerTests`, specs "the ledger IS the pack response - no VerifyContext RPC at all" + "refresh rebuilds the pack" |
| 27 | 2 | Body toggles were cosmetic - an eye icon and an opacity; `bodyEnabled` never reached the wire while the pill claimed "All bodies hidden" | **WIRED, not deleted.** `ContextCardSpec.exclude_bodies` (negative sense, so proto3's default keeps an older caller's pack byte-for-byte); the builder drops the card's `bodies` section. The toggle renders only on card types that carry one - no inert control survives. | `ContextPackBuilder.cs` (`BodiesSection`, `BodyCapableCardTypes`, the `wanted` filter); `composition-view.ts` `canToggleBodies()`; `budget-panel.ts` pill hidden when no card can carry bodies. Gates: `ContextPackLedgerTests`, spec "bodyEnabled rides the request and a toggle re-packs" |
| 29 | 6 | Studio cards were never invalidated - the file contained no `effect()`, so a re-analyze left cards holding node ids from the previous graph | **Handle-effect invalidation** over per-tab keying: the handle *is* the identity of the graph those ids are addressed in. Shaping (budget/intent/format) is a preference and is persisted instead - it is not session state. | `context-studio.ts` constructor effect + `resetPackState()`; `prefs.store.ts` `studioBudget/studioIntent/studioFormat`. Gates: specs "a new session handle clears the cards...", "budget / intent / format are persisted...", "restores the persisted shaping on construction" |

Per-card `verified`/`approx` (audit section 3.B, not a numbered 3.F item) also landed here:
`composition-view.ts` `provenanceMix()` renders the trust mix the wire has carried since T4.4 and
no surface showed. Spec: "renders verified/approx PER CARD, and offers the body toggle only where
it acts".
<a id="fixed-n12"></a>

## FIXED in N1.2 — §3.F.1, the audit's flag finding

Stage N1.2 closed **#26** ("pins are advertised by three surfaces and read by none"), the item
owner decision 1 of `STUDIO-MCP-AUDIT-2026-08-13.md` §8 settled as **IMPLEMENT** rather than
delete-the-idiom.

| # | 3.F | Defect | Decision taken | Fix locus |
|---|-----|--------|----------------|-----------|
| 26 | 1 | `TrailStore.pins()` reached a counter and a colour and nothing else; the "From current trail" button read the RAW `steps()`, kept `kind === 'entry'` only, and no-opped in silence. Three surfaces stated the mechanism as fact. | **IMPLEMENT (owner decision 1).** Pins win over the raw trail; the trail stays the fallback so the button still works before anyone pins anything. Every step kind resolves through its `focus`, so a pinned graph NODE stops being worthless. Resolution is against the LIVE `entryGroups()`, never the pinned nodeId — the same invalidation principle N1.1 chose for cards, applied where the id crosses a re-analyze. | `context-studio.ts` `onTrailSeed()` (+ `pinCount`/`trailCount` computeds); `scope-picker.ts` seed button (`data-testid="trail-seed"`, source + count + disabled reason); `workbench-page.ts` `onPin()` (the `p` shortcut reports what it did); `inspector.ts` `pinTitle()` + trail empty-state; `trail-bar.ts` chip title; `workspace-shell.ts` `tip:pin`. Gates: 5 specs in `context-studio.spec.ts` (pins beat the trail · node steps seed · dead pins reported · silent no-op replaced · button states source/count) + 4 in the new `workbench-page.spec.ts` |

Side-finding filed while wiring the button, not fixed here: `ui/icon/icon.ts`'s registry renders an
**empty span for a name it does not carry** (`if (!node) return`), and the app binds four such names
(`box`, `edit`, `grip-vertical`, `lock`) — `bookmark` and `history` were added because the seed
button binds them. Tracked as run bug #3.

## M1.2 — the hygiene batch (2026-08-13, desktop pre-release stage M1)

The one item below is the only M1.2 item that was ever a *filed defect* — the other four
(Layer/Feature lens slots, the `createTab` MAX_TABS lie, the dock resizer, the high-contrast
theme) were checkpoint items, and their measurements are in
`eval-results/2026-08-13/M1.2-hygiene.md`. The `#` here is a **conductor run-bug** number, not a
backlog number — this table read `30` until Z1.1 corrected it, which collided with backlog
[#30](#fixed-n2), a different bug entirely.

| run bug # | Defect | Decision taken | Fix locus |
|---|--------|----------------|-----------|
| run #7 | `MapResponse.stack` (proto field 13) had **three readers and no writer**: `identity-strip.ts` (`stack()` computed), `atlas-page.ts` (chip header), MCP `overview` (`DevContextTools.cs:400`). Nothing in `ProtoMapper` ever set `resp.Stack`, and `MapModel` had no `Stack` member — the tags were computed inside `MapRenderer.AppendStack` and left only as a line of markdown. The S9 contract sweep cannot see this direction: it fails a field with no READERS. Run bug #7. | **POPULATE.** The fact existed and was already rendered on the CLI; the wire was dropping it. `MapModel.Stack` is now built once in `MapBuilder.Build` (which already holds the `DiscoveryModel` and the aggregates), the renderer JOINS that list instead of recomputing it, and `ProtoMapper` copies it. Markdown is byte-identical — the architecture goldens did not move. | `MapBuilder.cs` (`BuildStack`, + `SummarizeTfms`/`TfmRank` moved down from the renderer); `MapRenderer.cs` `AppendStack` is now four lines; `ProtoMapper.cs` `resp.Stack.AddRange(map.Stack)`. Gates: `MapStackTests` (4) + `ProtoMapperStackTests` (2) |

<a id="fixed-n2"></a>

## FIXED in N2.1 — the two §3.F items the D-G decision had to settle first (2026-08-14)

Both were filed in N0 with an explicit *"why not fixed in N0"*: each needed the pack-convergence
call (owner decision 2, `STUDIO-MCP-AUDIT-2026-08-13.md` §8), not effort. Loci re-measured
2026-08-14 (Z1.1) against the shipped code, not against the session's claim.

| # | §3.F | Defect | Decision taken | Fix locus (re-measured Z1.1) |
|---|------|--------|----------------|------------------------------|
| 30 | 8 | The zero-entry empty state told the user to pick types from an omnibox that searches entries only — unsatisfiable in the exact state that printed it | **Give the instruction somewhere to point.** The picker gained a **Types tab** over the same `MapResponse.surface` the library workbench reads (one source, second view), and both empty-state strings now name it. The fix is the scope model, as N0 predicted — not a reworded sentence. | `scope-picker.ts:66-76` — the archetype notes read "Its types are in the Types tab above" / "Scope this pack from the Types tab above"; the tab itself at `:145-177` (`picker-tab-entries` / `picker-tab-types`, counts on the tabs so an empty one says so) and `:277-309` |
| 31 | 15 | The `usage` section was built by every symbol-rooted focus and then discarded — no card type mapped to it, so the human in Studio could never see the answer `get_context` gives the agent | **Map it.** `CardTypeSections["usage"] = ["usage"]` — the inbound direction of the same convergence that made `BuildMulti` symbol-rooted. "Who calls this" is the half of a symbol-rooted pack a change-impact reader is after. | `ContextPackBuilder.cs:555-559`; the card is produced by `pack-proposal.ts:162` and `scope-picker.ts:32`, and pinned by `context-studio.spec.ts:1103-1123` + `pack-proposal.spec.ts:127` |

**Not closed, and deliberately:** #31's second clause — the `"client-only type, no server section"`
omission branch (`ContextPackBuilder.cs:638-642`). It is unreachable from the app (the app's card
vocabulary and `CardTypeSections` are the same set) but `ContextCardSpec.type` is a free string on
the wire, so any gRPC or MCP client can still reach it. It stays as a **wire-facing guard** rather
than dead code, which is why it was not deleted.

<a id="fixed-n4"></a>

## FIXED in N4.3 — the last hand-kept copy of the tool menu (2026-08-14)

| # | Defect | Decision taken | Fix locus (re-measured Z1.1) |
|---|--------|----------------|------------------------------|
| 4 | The desktop MCP page kept its **own literal array** of tool names (`availableTools`) — a third hand-maintained copy of the menu that had already drifted (it advertised `search`, which the MCP calls `find`). The app speaks gRPC, not MCP, so it had no way to check its own labels | **Serve the catalog** (the option the bug named first, chosen over build-time generation because the menu the agent gets is a *runtime* fact about the running MCP, not a compile-time one). A `ListMcpTools` RPC returns the curated, described menu the engine run's T1 froze; the page renders that. The literal array is gone — `availableTools` no longer exists anywhere in the app. | `DevContextGrpcService.cs` (`ListMcpTools`), `devcontext-api.ts`, `mcp-page.ts`; commit `6c2501e`, on top of the T1 merge `153c99f`. design + page evidence in `eval-results/2026-08-14/N4.3-catalog-served.md`, and the LIVE proof in `N4.3-deep-links.md` ("ListMcpTools answers off a LIVE tools/list — 14 advertised, 8 specialists") |

Bug #4's own note said the contract sweep cannot catch this class (a hand-written string naming a
tool, with no proto field involved). That is still true — what closed the class here is
**structural**: there is no second list left to drift. The N4.3 deep-link work chose the same
principle a level deeper, routing on the gRPC method the server recorded rather than on an MCP
tool *name*, so no name table exists there either.
---

## Filed BY the pre-release run — the durable copy

`run.db` is gitignored, so these would be lost with the run. They are conductor's own bug series and
their numbers are **independent of the `#` series above**. Read them as "new #N".

| new # | Sev | Stage | Title |
|---|---|---|---|
| 1 | medium | T1 | MCP endpoint 127.0.0.1:5179 is hardcoded and shared: a second checkout (or the desktop app) silently serves another repo's engine. *(Partly mitigated: `DEVCONTEXT_ENDPOINT` exists since T1.4 and the probes set it; the default is still shared.)* |
| 2 | medium | T1 | Eval stamp cache is structurally dead: `gates.ps1` `Get-EngineStamp` sweeps `obj/`, and MSBuild rewrites `sourcelink.json` + `AssemblyInfo.cs` on every build/commit |
| 3 | medium | V1 | `typeof(X)` registrations lose their edge instead of naming X: 25 measured DI wirings dropped by V1.3's INV-B — including MediatR's entire open-generic behavior pipeline, which can only be registered that way. Deliberately deferred out of V1 (it ADDS edges, a content change). |
| 4 | medium | E1 | A merged Calls edge keeps the FIRST resolution, not the best: 14 DevContext pairs read `approx` although one of their call sites is semantically bound (`CodeGraphBuilder.AddEdge` dedupes `(From,To,Kind)` first-wins) |
| 5 | medium | E1 | Tier A alone cannot type an UNTYPED lambda parameter, so a degraded project (no `assets.json`) loses lambda-argument call edges — **the true residual of #8** |
| 6 | medium | E1 | LINQ extension calls are attributed to the RECEIVER ROOT's type, minting member nodes that do not exist: 126/2453 Calls edges (5.1%) on the DevContext pole, 66 distinct phantoms (`IFileSystem::ToList`, `ProjectInfo::Select`, `CodeGraph::Count`) — the member-side sibling of new #7, and bigger |
| 7 | medium | E1 | `Sends` stops at the GENERIC WRAPPER type: `OrdersApi::CreateOrderAsync --Sends--> IdentifiedCommand<T,R>`, never `CreateOrderCommand`, so the class-C up-chain from a command handler cannot reach its HTTP endpoint |
| 8 | medium | E1 | A handler that implements `IRequestHandler` only through a BASE class gets NO `Handles` edge: `CreateOrderIdentifiedCommandHandler` has ZERO edges of any kind in the eShop graph — an orphan node |
| 9 | medium | E1 | The `Handles` join pairs the RESPONSE type argument as a command: `Type:bool --Handles--> CreateOrderCommand`, and `Type:bool` is minted as a node tagged "command" |
| 10 | medium | E1 | eShop's test projects are absent from the graph (4 nodes match `/Test/`, all production Webhooks.API types), so `tests_for` structurally cannot answer "what tests break" on the repo the probe measures |
| 11 | medium | E1 | `impact(direction:'both')` is an undirected flood — 207/1137 eShop nodes (18%), reaching Catalog.API/WebApp/Identity.API and the probe key's designated false positive; the directed sets are 2 (up) and 30 (down) |
| 12 | medium | E1 | No cross-service integration-event join: Ordering.API's `OrderStartedIntegrationEvent` and Basket.API's same-named copy are two unconnected nodes; only a coarse Service→Service ServiceLink exists, so the raise site never reaches the consuming handler |
| 13 | medium | E1 | `WrappedBy` is not scoped to the DI container that registers the behavior: 60 of 108 WrappedBy edges cross a project boundary |
| 14 | medium | — | The `testing` signal is unreachable from any consumer repo (catalog `Packages:[]` + `IsTestPath` suppression) |
| 15 | medium | — | The blazor descriptor declares Kind `HttpEndpoint` but the builder emits `UiEntry` for `.razor` pages (the catalog is the stale side) |
| 16 | medium | — | AWS Lambda: the canonical template shape (`FunctionHandler` + `ILambdaContext`, no attribute) produces no FunctionEntry |
| 17 | medium | — | `BackgroundWorkerKind.TimedJob` has no producer; give it one (Hangfire `RecurringJob` / Quartz `ScheduleJob` registration syntax) |
| 18 | medium | R1 | L3.4 hub-scope broadening sizes its hubs off `model.CallEdges`, a channel post-E1 has left behind: 3 of the 4 gate-passing poles exit at k<5 while carrying hundreds of graph Calls edges — **the residual of #23** |

Nothing above is a regression: every one was measured by the stage that found it and filed rather
than fixed, on the same rule as the 2026-08-02 export.

---

<a id="filed-2026-08-26"></a>

## Filed 2026-08-26 — the Book2Course unseen-repo hand drive

Source: `eval-results/2026-08-26/unseen-drive-Book2Course/DRIVE.md` (filed at commit `76e1111`) —
one operator, five questions fixed in advance, answered once with grep/read and once through
`devcontext-mcp` over stdio, against `C:/Code/BookToCourse` @ `c14da997` (a private .NET 10 Aspire
app the engine had never seen, 487 C# files). Engine under test: `develop` @ `b7b0ab0`. Driver and
full transcripts sit next to the drive record: `mcp.js`, `calls-*.json`, `analyze-out.txt`,
`q-out.txt`, `q2-out.txt` — every evidence line below is a line in one of those files, not a recall.

The drive found five defects. **F2** (auth: a `MapGroup`'d group's policy never reached its routes,
so `stats` called the most protected surface anonymous) was release-blocking and was fixed the same
day — `09934f2` on `fix/auth-group-inheritance`, merged to develop as `5e42c46`, re-measured
12/39 → 6/39 on the same repo; its record is the FIXED block inside DRIVE.md and it is deliberately
NOT re-filed here. The remaining four are filed below, numbered **#33–#36** — the `#` series'
next free numbers (highest previously #32; conductor's independent "new #N" series above is not
this series and stays untouched).

Mechanism loci below were mapped in source on 2026-08-26 by a read of the shipping code at
`b7b0ab0`, not fixture-proven — the fixture goes RED first, per house rule, before any fix lands.

| # | Sev | Stage | Drive id | Status (2026-08-27) | Title |
|---|-----|-------|----------|---------------------|-------|
| [#33](#33) | high | drive-2026-08-26 | F1 | **FIXED** — re-measured PASS | Extension and BCL methods are minted as MEMBERS of the receiver type — `startHere`'s first line is `AppDbContext.ConfigureAwait`, which nothing declares |
| [#34](#34) | high | drive-2026-08-26 | F3 | **FIXED** — re-measured PASS | `config` does not know the Options pattern — a 487-file repo reads "1 keys exist" and `config.missing-defaults` under-declares without saying so |
| [#35](#35) | high | drive-2026-08-26 | F4 | **FIXED as filed, residual re-filed** → [#37](#37) | `seam` cannot cross a transport — the port is a sink (in 8 / out 0), so found:false across a connection the graph holds fully verified |
| [#36](#36) | low | drive-2026-08-26 | F5 | **FIXED** — re-measured PASS | `usages` returns the same call site three times — one caller, one line, count 3 |

Statuses are the 2026-08-27 reconciliation — see
[§ Reconciliation 2026-08-27](#reconciliation-2026-08-27) at the foot of this file. The filings
below stay unedited, on the same rule as the 2026-08-02 originals.

<a id="33"></a>

### #33 · drive-2026-08-26 · Extension and BCL methods are minted as MEMBERS of the receiver type — `startHere`'s first line on an unseen repo is AppDbContext.ConfigureAwait / .Where / .IgnoreQueryFilters, none of which AppDbContext declares — the residual of the #7/#12 family on the extension-method path

```text
MEASURED 2026-08-26, Q1 of the hand drive (drive id F1, severity HIGH). Artifacts:
eval-results/2026-08-26/unseen-drive-Book2Course/q-out.txt:19 (the overview response, verbatim)
and DRIVE.md §F1.

WHAT THE WIRE SAYS. overview's text — the first line an agent ever reads on this repo — is:
  Start here: AppDbContext, AppDbContext.ConfigureAwait,
              AppDbContext.Where, AppDbContext.IgnoreQueryFilters
and the structured startHere behind it mints the nodes:
  Member:Book2Course.Api.Data.AppDbContext::ConfigureAwait      "Central type: 72 connections"
  Member:Book2Course.Api.Data.AppDbContext::Where               53
  Member:Book2Course.Api.Data.AppDbContext::IgnoreQueryFilters  46
  Member:Book2Course.Api.Data.AppDbContext::Select              37
  Member:Book2Course.Api.Data.AppDbContext::ToListAsync         37
AppDbContext declares NONE of these (Api/Data/AppDbContext.cs: a constructor, 13 DbSet
properties, ConfigureConventions, OnModelCreating). ConfigureAwait is Task's;
Where/Select/ToListAsync/IgnoreQueryFilters are LINQ/EF extension methods. The engine binds the
call to the RECEIVER's type, ranks by degree, and the noise sorts to the top — displacing the
real starting points on the one question class (A, orientation) the deep eval names winnable.

NOT CONFINED TO RANKING. The same phantoms appear inside traces (q-out.txt, the Q2 trace):
"→ Member: SourceUploads.ConfigureAwait [approx]", "→ Member: S3ObjectStore.ConfigureAwait
[approx]" — so trace hops route through members that do not exist.

FAMILY. #7 (a METHOD registered as a Type node, 26 BCL references bound to it) and #12 (the
receiver-type upgrade's line-span miss) are both FIXED; this is the residual of that family on
the extension-method path. It is also the unseen-repo confirmation of conductor "new #6" (LINQ
extension calls attributed to the receiver root's type: 126/2453 Calls edges, 66 distinct
phantoms, measured on the DevContext pole 2026-08-14) — same defect, now shown to corrupt the
FIRST LINE of the product on a repo nobody tuned for.

MECHANISM, MAPPED IN SOURCE 2026-08-26 (four hops, none with a declares-gate on the in-solution
value-receiver arm):
  1. BodyFactExtractor.RootIdentifier walks THROUGH invocations, so a chained call's root
     receiver becomes the guess (Graph2/BodyFactExtractor.cs:378-398).
  2. SemanticLitePopulator.MergeSemantic DROPS a semantic bind that contradicts the syntactic
     text, so the wrong guess survives the tier that knew better
     (Graph2/SemanticLitePopulator.cs:692-710 — the smoking gun).
  3. CallGraphBinder.ResolveCallee's receiver arm gates on Kind==Type + IsKnownFqn but never
     asks whether the type DECLARES the member (Graph2/CallGraphBinder.cs:200-277). The gate
     already exists on the bare-identifier arm (:261-267) and the static arm
     (SymbolTable.cs:294-308); mirror gap in PlainCallDetector.cs:86-98.
  4. GraphBuilder.AddCallEdges mints the node (Graph/GraphBuilder.Seams.cs:155-185; second
     producer AddHubScopeEdges :263-277).

INVARIANT WORTH GATING: no node may be a member of a type that does not declare it — as a
standing invariant at CodeGraphBuilder.AddNode (Graph/CodeGraph.cs:344-377) with an injected
declares-oracle. THE ORACLE MUST WALK TypeDiscovery.BaseTypes: TypeDeclaresMember is
declared-members-only today, and a naive gate drops legitimate inherited-method calls. No BCL
name lists — house policy; the declares gate replaced them (GraphBuilder.Seams.cs:749-751).

WATCH IT GO RED FIRST: the discriminating fixture is a chained extension call on a DbContext-like
receiver asserting the phantom member node is NEVER minted — template
Graph2/StaticReceiverEdgeTests.cs (negative idiom :88-108), standing-guard triple à la
BclNameCollisionEdgeTests, receiver-root case in BodyFactExtractorTests, dogfood sweep à la
DogfoodEdgeInvariantTests.

BLAST RADIUS WARNING FOR WHOEVER FIXES IT: call-edge counts move on EVERY pole — declare
flip/hold/not-worsen expectations in writing before coding (E1 discipline), and check the
startHere noise-filter tests (GraphQueryTests.cs:105-136) and approx-share numbers, which this
fix should move DOWN.
```

<a id="34"></a>

### #34 · drive-2026-08-26 · `config` does not know the Options pattern — `AddOptions&lt;T&gt;().BindConfiguration(Const)` is invisible, so a 487-file repo reads "1 keys exist" and `config.missing-defaults` under-declares without saying so

```text
MEASURED 2026-08-26, Q5 of the hand drive (drive id F3, severity HIGH). Artifacts:
eval-results/2026-08-26/unseen-drive-Book2Course/q-out.txt:37 (the config error envelope,
verbatim) and DRIVE.md §F3.

WHAT THE WIRE SAYS. config(key:"Pipeline:Queue:Drain") returns:
  "No config key exactly 'Pipeline:Queue:Drain' (1 keys exist)."
  candidates: ["OTEL_EXPORTER_OTLP_ENDPOINT"]
ONE key, in the whole repo. The repo binds config the modern way — Pipeline/DependencyInjection.cs:73:
  services.AddOptions<QueueDrainOptions>()
      .BindConfiguration(QueueDrainOptions.SectionName)   // "Pipeline:Queue:Drain"
with [Range]-validated properties (IdlePollSeconds = 2, Workers = 2, Enabled), an env-var
contract in infra/.env.example, and Storage__* / Pipeline__Media__* wiring in the AppHost. Q5 was
the question grep was PRE-REGISTERED to win ("control"); it lost by defect, not by design.

IT PROPAGATES, AND THE HEADLINE HIDES IT. Insight config.missing-defaults reports "1 consumed
keys" off the same blind spot — the catalog under-declares and does not say so anywhere a reader
looks; the admission lives only in confidenceBasis. This is the mirror of the catalog
OVER-declares class in GRAPH-DETECTION-AUDIT-2026-08-13.md.

MECHANISM, MAPPED IN SOURCE 2026-08-26 — TWO UNRELATED IMPLEMENTATIONS THAT MUST MOVE TOGETHER
OR THE NUMBERS DISAGREE:
  - Graph/ConfigScanner.cs (the syntax path behind `config`): ConfigMethods (:31-37) knows only
    GetValue / GetSection / GetConnectionString / GetRequiredSection — no AddOptions, no
    BindConfiguration, no Configure<T>. LiteralArg (:120-123) is literal-only — the exact
    defeat F2 just taught us (a const argument reads as null). And it scans only files that
    already own a graph node (:46-49), so a composition-root DependencyInjection.cs can be
    skipped wholesale.
  - Insights/ConfigDefaultsSource.cs (the regex path over SourceBody, :41-43): drives the
    "N consumed keys" number independently, admits the gap only in confidenceBasis.

FIX SHAPE (do not apply from this text — fixture first): detect Options bindings at EXTRACTION
time. DiRegistrationExtractor already captures AddOptions<QueueDrainOptions>() (generic Add*
branch :184-210); read the chained .BindConfiguration(section) / .Bind(config.GetSection(...)) /
services.Configure<T>(section) there, resolving const arguments exactly the way F2 did — copy
EndpointExtractor.ResolveGroupPrefixArgument (:484-513: literal / bare-on-enclosing-type /
qualified, NEVER guess a computed value) against FastEndpointsHelper.BuildRouteConstIndex
(:50-78 — the index already contains "QueueDrainOptions.SectionName"). Carry the keys in the
model so ConfigScanner's consumers and ConfigDefaultsSource read ONE source. Where
under-declaring remains possible, the HEADLINE says so — not just confidenceBasis. Update the
tool's self-description (DevContext.Mcp/DevContextTools.cs:1599, 1648), the proto pattern_type
comment (devcontext.proto:1199), and the docs row (docs/product/mcp-reference.md:167).

WATCH IT GO RED FIRST: extend ConfigScannerTests (the harness needs a SECOND file for the
cross-file const shape); mirror the four F2 const tests including the never-guess-a-computed-
value negative (EndpointExtractorTests.cs:249-361) for BindConfiguration(BuildSectionName());
first-ever tests for ConfigDefaultsSource (home: InsightHonestyTests.cs); a server ConfigLookup
envelope test via the McpStubCallInvoker precedents.
```

<a id="35"></a>

### #35 · drive-2026-08-26 · `seam` cannot cross a transport — producer and consumer both point INTO `IJobQueue` (in 8 / out 0, a sink), so seam reports found:false across a connection the graph itself holds fully verified

```text
MEASURED 2026-08-26, Q4 of the hand drive — two of five questions hit it (drive id F4, severity
HIGH). Artifacts: eval-results/2026-08-26/unseen-drive-Book2Course/q-out.txt:31
(seam SourceUploadEndpoints → IngestStage, found:false — TRUTHFUL, see below),
q2-out.txt:25 (seam BuildCoordinator → IngestStage, found:false — THE MISS),
q2-out.txt:27-28 (neighbors(IJobQueue, direction:in) — the proof), and DRIVE.md §F4.

WHAT THE WIRE SAYS. Both seam calls answer found:false: "…the walk exhausted everything
reachable from each end within 8 hops and neither reached the other." The neighbors call shows
exactly why the second one is wrong. The join is IN the graph, fully verified — count:8,
totalEdges:8, every edge kind Calls, resolution Semantic, each with file:line provenance:
  BuildCoordinator.AdvanceAsync   → IJobQueue   Pipeline/Workflow/BuildCoordinator.cs:34
  BuildCoordinator.CancelAsync    → IJobQueue   Pipeline/Workflow/BuildCoordinator.cs:61
  JobRunner.RunNextAsync          → IJobQueue   Pipeline/Workflow/JobRunner.cs:82
  JobRunner.RunAsync              → IJobQueue   Pipeline/Workflow/JobRunner.cs:106
  JobRunner.CompleteAsync         → IJobQueue   Pipeline/Workflow/JobRunner.cs:199
  (+ JobSettlement.CancelAsync / .FailAsync, LeaseHeartbeat.BeatAsync)
Producer and consumer BOTH point into the port; nothing points out of it. In-degree 8,
out-degree 0 — a sink. No path can route through it, and seam correctly reports no path across
a connection that plainly exists.

MECHANISM, MAPPED IN SOURCE 2026-08-26: GraphQuery.SearchSeam walks out-edges only
(Graph/GraphQuery.cs:791); both sides of a port land as IN-edges on the interface Type
(PlainCallDetector.cs:118-126). The verb evidence that could split them — enqueue vs dequeue —
is already carried on GraphEdge.TargetMember (CodeGraph.cs:191-199) and read by nothing.

WHY IT MATTERS. This is the handler-join cell of the graph-truth matrix, and it is not a corner
case: it is every queue-, bus- and outbox-driven .NET app. seam is advertised as "the only tool
that answers how does A reach B" — on this architecture it answers only within a single process
hop.

FIX SHAPE (do not apply from this text — fixture first): materialize the bridge at GRAPH BUILD,
mirroring the ONE sanctioned join — EventWiringProjection (Graph/EventWiring.cs:63-139, emits
ServiceLink with Resolution.Join, invoked off a draft graph at GraphBuilder.cs:104-108). Extend
that projection (or generalize it in place — house rule: event wiring has exactly ONE join,
never a second ad-hoc one) to classify an in-repo port whose callers split into write-verbs and
read-verbs (from TargetMember; verb tables exist at EventBusExtractor.cs:127-131 and in
DispatchSeamCatalog), emitting producer→port→consumer join edges so the seam can route through.

TRUTHFULNESS CONSTRAINT, NON-NEGOTIABLE: the drive's FIRST seam call
(SourceUploadEndpoints → IngestStage, found:false) was CORRECT — upload stages the file, ingest
happens later off the queue. The join must not fabricate a path across a boundary the graph
genuinely cannot see, and a joined hop renders as `joined`, NEVER as `verified`. If a new
SeamKind/EdgeKind is introduced, SeamVocabularyTests ratchets glyph+label, and any new proto
field must be read by a client or allow-listed (eval/contract-sweep.ps1, R-T1).

WATCH IT GO RED FIRST: a fourth factory in GraphSeamTests.cs (producer → port ← consumer shape)
asserting SeamDirection.Forward where today it is None, with a "THE RED" doc comment; wire/tool
pinning via the SeamPrimitiveTests builders. No existing fixture has this shape — add it to
PatternZoo or a small new fixture for the end-to-end.
```

<a id="36"></a>

### #36 · drive-2026-08-26 · `usages` returns the same call site three times — usages(JobRunner) reads count:3 where QueueDrainService.cs:95 is ONE call site

```text
MEASURED 2026-08-26, Q2b of the hand drive (drive id F5, severity LOW). Artifacts:
eval-results/2026-08-26/unseen-drive-Book2Course/q2-out.txt:31 (the usages response, verbatim)
and DRIVE.md §F5.

WHAT THE WIRE SAYS. usages(query:"JobRunner") → count: 3, and all three rows are:
  caller  Member:Book2Course.Pipeline.Workflow.QueueDrainService::TurnAsync
  kind    Calls
  provenance  Pipeline/Workflow/QueueDrainService.cs:95
One call site, counted three times. A small lie, but it is a COUNT — the number an agent quotes.

MECHANISM, MAPPED IN SOURCE 2026-08-26. Storage holds no duplicate edges — CodeGraph dedupes on
(From,To,Kind) (CodeGraph.cs:409-416). The Type roll-up returns edges that differ only in `To`
(the caller reaches JobRunner AND its distinct members; GraphQuery.cs:98-121), and the MCP
projection then DROPS `To` (DevContextTools.cs:1356-1362) — so rows that were distinct in the
graph render identical on the wire. The CLI has the same omission (QueryCommand.cs:295-300).

FIX SHAPE (do not apply from this text — fixture first): collapse at projection/query time —
rows agreeing on (caller, kind, provenance) merge, and the count is the merged count. Prefer the
seat that keeps TotalEdges/KindsPresent's documented "unfiltered walk" contract intact
(GraphQuery.cs:211-221), or amend that contract deliberately — not by accident. Avoid the proto
change unless the fix genuinely needs TargetMember on the wire (contract-sweep applies).

WATCH IT GO RED FIRST: a new [Fact] in GraphQueryTests.cs — row uniqueness is currently pinned
by NOTHING — plus a GraphNeighborKindTests-style member-fan-in fixture (one caller hitting a
type and two of its members), and CLI parity.
```

---

<a id="reconciliation-2026-08-27"></a>

## Reconciliation 2026-08-27 — the Book2Course drive-fix program

Four fix branches, one per filed defect, built fixture-first in their own worktrees and merged on
`fix/mcp-drive-integration` (`9e68756` F5 · `91e6d32` F3 · `b7e106d` F4 · `37f70a6` F1). The
integration battery — the first run in a while with the `eval-repos/TodoApi` + `VerticalSlice`
submodules initialized — caught two F1-introduced regressions and STOPPED ON RED per R-T7; both
were root-caused by bisection (develop `04173d6` + same submodules = 6/6 PASS; F1 head `bd46c8b`
alone = the same 3 FAIL) and repaired on the integration branch: `e085634` (a refused call
DEGRADES to the member→Type Calls edge instead of dropping — INV-C kept literal, connectivity
kept true; and the FastEndpoints entry join targets the handler the class actually DECLARES),
with the zoo fixtures made to declare what their source declares (`b24e09b`) and the ratcheted
MCP QA record moving in its own R-T7 commits (`9fea911`, `9cfefc2` — nothing loosened, every
number tied to a declared cause).

Verdicts were then re-measured with the drive's own recorded call batches against the integrated
tree (`9cfefc2`), fresh analyze, target untouched at `c14da997`. Evidence for every row:
`eval-results/2026-08-26/unseen-drive-Book2Course/remeasure-post-fix/REMEASURE.md`, raw
transcripts beside it.

| # | Verdict | What the re-measure found |
|---|---|---|
| #33 | **FIXED** (F1) | `startHere` is `ApiProblems, Course, Run, AppDbContext` + genuinely declared members; zero `ConfigureAwait`/`::Where`/`IgnoreQueryFilters` across all four transcripts including the Q2 trace (29 steps, DI hop and `[approx]` markers intact). The ledger moved the honest way: Calls approx 900 → 279 (56% → 30%), graph 1315/1739 → 1172/1078 as the minted members left, verified down only 41. The invariant stands in the battery (INV-C + dogfood sweep = zero undeclared member nodes), and the integration repair kept it literal while restoring the connectivity the first cut overshot away. |
| #34 | **FIXED** (F3) | `config(key:"Pipeline:Queue:Drain")` resolves with provenance `Pipeline/DependencyInjection.cs:73`, patternType `OptionsBinding`; the catalog reads 14 keys where it read 1; `config.missing-defaults` now states its blind spot out loud ("counts literal + Options-bound keys; computed keys are invisible here"). |
| #35 | **FIXED as filed, residual re-filed → #37** (F4) | The filed defect — the port is a SINK — is gone: `seam(BuildCoordinator → JobRunner)` is found:true, 2 paths, crossing hop `IJobQueue —Consumes/Join→ JobRunner.RunNextAsync`; stats carries `Consumes: total 1, joined 1`. The drive's recorded Q4b call still answers found:false — the walk now dies ONE HOP LATER, at `IngestStage` in-degree 0 (stages registered by a reflection assembly scan), a defect the sink had masked. Re-filed as #37 on the #8 rule: fixed against the true cause, the leftover re-filed rather than quietly widened. The truthfulness half held: `seam(SourceUploadEndpoints → IngestStage)` stayed found:false with the honest note; nothing fabricated anywhere. |
| #36 | **FIXED** (F5) | `usages(JobRunner)` reads count:2 — `QueueDrainService.TurnAsync` at `QueueDrainService.cs:95` exactly once; the second row is the visibly distinct new `Consumes` port-bridge in-edge (distinct caller, kind, provenance), not a duplicate. |

The drive's license line — the re-probe must not run until F1–F4 are filed and fixed — is met on
its letter. It is NOT met end-to-end on this repo: the pre-registered Q4 will keep reading
found:false until #37 lands, and the pilot's lesson (do not pay to measure a known-broken part)
now points at exactly that item. Owner's call.

---

<a id="filed-2026-08-27"></a>

## Filed 2026-08-27 — the integration battery and the post-fix re-measure

Source: the `fix/mcp-drive-integration` battery/repair story (commits `b24e09b` · `9fea911` ·
`e085634` · `9cfefc2` and the integrate report) and `remeasure-post-fix/REMEASURE.md`. Findings
filed, not fixed, on the standing rule. Numbers continue the `#` series.

| # | Sev | Stage | Title |
|---|-----|-------|-------|
| [#37](#37) | high | remeasure-2026-08-27 | Reflection-registered types have NO in-edges — `AddStages()`'s assembly scan is invisible, so `IngestStage` sits at in-degree 0 and the re-measured F4 bar dies one hop past the fixed port |
| [#38](#38) | low | remeasure-2026-08-27 | The config catalog sweeps stray temp copies inside the target repo — `.conductor/tmp-q14/StorageModule.cs` contributes a provenance row |
| [#39](#39) | medium | integrate-2026-08-27 | The MCP QA drive can be raced by any concurrent test run: `eval/mcp-qa/run.js` never adopted `server-identity.js`'s endpoint isolation, and `ServerTestFactory.Dispose` kills EVERY `DevContext.Server` on the machine by name |

<a id="37"></a>

### #37 · remeasure-2026-08-27 · Reflection-registered types have NO in-edges — `AddStages()`'s assembly scan is invisible, so `IngestStage` sits at in-degree 0 and the re-measured F4 bar dies one hop past the fixed port

```text
MEASURED 2026-08-27, the post-fix re-measure of the Book2Course drive (severity HIGH). Artifacts:
eval-results/2026-08-26/unseen-drive-Book2Course/remeasure-post-fix/REMEASURE.md §F4,
diag-out.txt D4 (neighbors(IngestStage, direction:in) → count: 0), q2-out.txt (the recorded Q4b
seam, still found:false with the honest note).

WHAT THE WIRE SAYS. With #35's port bridge landed and PROVEN — seam(BuildCoordinator → JobRunner)
found:true, 2 paths, through IJobQueue with the Consumes/Join hop — the recorded call still
misses:
  seam(BuildCoordinator → IngestStage) → found:false, "the walk exhausted everything reachable
  from each end within 8 hops and neither reached the other."
and the localization is one call: neighbors(IngestStage, direction:in) → count: 0. NOTHING in
the graph lands on IngestStage — no Calls, no Resolves, no seam. The walk from the now-bridged
queue exhausts honestly before any stage type.

MECHANISM, IN TARGET SOURCE (read 2026-08-27). Stages are registered by reflection —
Pipeline/DependencyInjection.cs:206-214: AddStages() scans the assembly for IStage implementors
and calls services.AddSingleton(typeof(IStage), stage) with a RUNTIME Type variable; JobRunner
dispatches to them dynamically through IStageRegistry. No spelling anywhere in source names
IngestStage as a registration or a callee, so the engine's DI arm
(Extractors/Generic/DiRegistrationExtractor.cs — generic-name and literal-argument shapes) has
nothing to bind: the implementation type NAME never appears at the registration site.

FAMILY. The registration-is-a-scan class: #22's retirement measured it from the other side (the
four eShop "orphans" were Carter ICarterModule endpoints registered by assembly scan), and
conductor new #3 (a LITERAL typeof(X) registration loses its edge) is the adjacent named case —
this is the harder no-name-at-all case. It was masked behind #35's sink until the port bridged;
it was NOT in the drive's fixed set, so it is filed, not inferred fixed (REMEASURE.md's own
words).

WHY IT MATTERS. Until it lands, the drive's Q4 bar cannot pass end-to-end on Book2Course — the
pre-registered re-probe hits it directly — and every plugin-style architecture (assembly scan +
dispatch registry) has this shape.

FIX SHAPE (do not apply from this text — fixture first, reform in place): recognize the scan
idiom at the DI arm — AddSingleton/AddScoped(typeof(I), <non-literal>) inside a method that
enumerates assembly types filtered by I — and join the INTERFACE to each in-solution implementor
with Resolution.Join, rendered joined, NEVER verified: the same truthfulness discipline #35's
port bridge just established. The seam walk then routes queue → runner → registry → stage, or
the interface-join equivalent.

WATCH IT GO RED FIRST: a fixture with the scan-registration shape (one interface, two
implementors, AddSingleton(typeof(I), t) in a scan loop, a registry dispatch) asserting the
implementor has in-edges and the seam routes — today it measures 0. The Book2Course end-to-end
re-check is the recorded calls-q2.json batch itself.
```

<a id="38"></a>

### #38 · remeasure-2026-08-27 · The config catalog sweeps stray temp copies inside the target repo — `.conductor/tmp-q14/StorageModule.cs` contributes a provenance row

```text
MEASURED 2026-08-27, the post-fix re-measure (F3's catalog check, severity LOW). Artifacts:
eval-results/2026-08-26/unseen-drive-Book2Course/remeasure-post-fix/REMEASURE.md §F3
(the observation paragraph), diag-out.txt D6 (the full 14-key catalog).

WHAT THE WIRE SAYS. The Storage key carries TWO provenance rows; the second is
  C:\Code\BookToCourse\.conductor\tmp-q14\StorageModule.cs
— a stray temp copy inside the target repo, part of no solution project. The scan swept it in
and reports it with the same file:line authority as the real StorageModule.cs row.

WHY IT MATTERS (small, but it is a truth surface): a temp/backup copy can double a key's
provenance, resurrect a deleted key, or point an agent at a file nobody compiles — stated with
provenance confidence. Harmless on this repo today (the row duplicates a real key rather than
minting one), which is why this is LOW.

MECHANISM, NOT LOCALIZED THIS PASS. ConfigScanner itself visits only node-bearing files
(Graph/ConfigScanner.cs:119-124) and the OptionsBinding rows ride the DI extractor, so the temp
copy most likely entered at DISCOVERY: the file inventory includes .cs files outside any
solution project, and .conductor/ is not among the default exclusions the way eval-repos is.
Localize before fixing — the fix seat differs (default exclusion list vs solution-scoped
inventory vs a config-scan filter).

WATCH IT GO RED FIRST: a fixture repo with a compiled source file plus a byte-identical copy
under a non-project directory (.conductor/tmp-x/), asserting the catalog carries exactly one
provenance row for the key.
```

<a id="39"></a>

### #39 · integrate-2026-08-27 · The MCP QA drive can be raced by any concurrent test run: `eval/mcp-qa/run.js` never adopted `server-identity.js`'s endpoint isolation, and `ServerTestFactory.Dispose` kills EVERY `DevContext.Server` on the machine by name

```text
READ IN SOURCE 2026-08-27 at the integration close-out (severity MEDIUM, harness infra) — the
root-cause note behind the T0.1-era "shared-state race". Two halves, one failure mode:

(a) eval/mcp-qa/run.js spawns its MCP with the AMBIENT environment: its requires are
child_process/path/readline/fs only — eval/mcp-qa/server-identity.js (built in T1.4 for exactly
this; adopted by wire-truth.js, partial-truth.js, deep-link-truth.js, classc-impact.js) is not
among them, and run.js never sets DEVCONTEXT_ENDPOINT. So the QA drive's server sits on the
shared default endpoint (conductor new #1, still open) where another checkout or the desktop
app can join it.

(b) tests/DevContext.Server.Tests/ServerTestFactory.cs:62-66 — Dispose runs
Process.GetProcessesByName("DevContext.Server") + Kill(entireProcessTree: true): machine-wide,
any owner. The factory's own host is an in-process TestServer, so in a clean run the loop finds
nothing — but the QA drive's ServerShim-spawned server IS an external DevContext.Server, and a
concurrent Server.Tests dispose (any `dotnet test --filter "Category!=Eval"`, which unlike
gates.ps1 Step 2 does not exclude McpQa — the documented verification-command trap,
gates.ps1:300-311) kills the server the QA drive is mid-conversation with.

WHY BOTH HALVES MUST MOVE TOGETHER: endpoint isolation alone does not survive a kill-by-name —
an isolated server is still named DevContext.Server. Hardening = run.js adopts probeEnv() +
health verification like the other four probes, AND the factory's kill loop gets
identity-scoped (kill only a process it can attribute — the PID it spawned, or a /health
baseDirectory under its own repo — never the bare process name).

WATCH IT GO RED FIRST: harness infra, not engine truth — the discriminating check is the
concurrent repro (QA drive + a Server.Tests dispose in parallel; the drive dies with the
first-call "no server" symptom), or at minimum run.js failing the same foreign-server identity
assertion the four converted probes already fail.
```
