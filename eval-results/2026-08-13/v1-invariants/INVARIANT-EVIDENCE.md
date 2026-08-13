# V1.3 — two standing invariants, enforced where a node is made

Backlog **#7's rider** and **#18**. Commit `3eb2f34` on `feat/pre-release-engine`.

    INV-A   no node carries kind Type with a MEMBER id
    INV-B   lambda / expression TEXT never becomes a node key or title

## Acceptance, declared before editing (conductor ledger, s7/V1)

(a) ONE predicate in ONE place decides both, applied at the single choke point
`CodeGraphBuilder.AddNode` — the shape V1.2 gave Member titles — so no producer and no pass ORDER
can violate it. (b) unit tests pin both and FAIL on the pre-fix build. (c) measured over the poles
with the instrument that filed #18, violations → 0, node COUNT delta stated pole by pole.
(d) eval matrix unmoved, or a moved cell explained and the work stopped. (e) build 0w/0e, Core +
Server tests green, loom-guards green.

## Instrument

`invariant-probe.ps1` — reads `query graphdump` per pole and counts violations of both invariants.
It reports **two** rules and the difference between them is the point:

| | rule | enforced? |
|---|---|---|
| **INV-B** | *expression syntax*: the text carries `= ( ) { } ; " '` or a newline/tab | **yes** — `SymbolCanon.IsExpressionText` |
| *(wider)* | *not type-name shaped*: does not parse as a C# type reference | **no** — diagnostic only |

The wider column is reported for honesty, not enforced. A queue channel node is keyed by its
display text **on purpose** (`feed-queue [AzureStorageQueue]`, B5 / Prism D1.2d), so a
"must look like a type name" rule would delete real event wiring. Both columns are Type nodes only:
EntryPoint / Service / Store titles are prose by design (`GET /todos`).

Nine poles, four of them cloned to `C:/Code/eval-poles` (`clone-poles.ps1`; Hangfire added this
session because it is the repo #7 was measured on). Poles are passed as `name=ABSOLUTE_PATH`.

## BEFORE → AFTER (same probe, same poles, CLI rebuilt on both sides)

`invariants-before.txt` (CLI 20:59:56, HEAD `0fd1cbe`) → `invariants-after.txt` (CLI 21:14:05, `3eb2f34`)

| pole | nodes | Type nodes | INV-A | INV-B | wider |
|---|---|---|---|---|---|
| eShop | 1136 → **1112** | 568 → **545** | 1 → **0** | 22 → **0** | 3 → 3 |
| FluentValidation | 319 → **318** | 204 → **203** | 0 → 0 | 1 → **0** | 0 → 0 |
| AutoMapper | 468 → **467** | 259 → **258** | 0 → 0 | 1 → **0** | 0 → 0 |
| MediatR | 192 → **172** | 170 → **150** | 0 → 0 | 20 → **0** | 0 → 0 |
| CleanArchitecture | 139 → **133** | 93 → **87** | 0 → 0 | 6 → **0** | 1 → 1 |
| dotnet-podcasts | 344 → **333** | 165 → **154** | 0 → 0 | 11 → **0** | 2 → 2 |
| Hangfire | 928 → **903** | 479 → **476** | 2 → **0** | 1 → **0** | 0 → 0 |
| TodoApi | 124 → **117** | 71 → **64** | 0 → 0 | 7 → **0** | 1 → 1 |
| DevContext | 1269 → **1260** | 492 → **484** | 1 → **0** | 7 → **0** | 0 → 0 |
| **total** | 4919 → **4815** | 2501 → **2421** | **4 → 0** | **76 → 0** | **7 → 7** |

**The wider column does not move.** All seven survivors are still there — `<OnModelCreating>` ×4,
`feed-queue [AzureStorageQueue]`, and eShop's `2.0` / `1.0` API-version nodes. The rule reached
exactly what it named and nothing else.

Node-count deltas track the violations: every node that left was one of the 80, plus a handful of
Member nodes that existed only as the origin of a seam whose target was a phantom.

## What INV-A actually was

Filed on Hangfire, and the audit reads as if it were a Hangfire curiosity. It is not:

    eShop        eShop.ClientApp.Converters.WebNavigatingEventArgsConverter::Convert(4)
    Hangfire     Hangfire.StackTraceHtmlFragments::Type(1)
    Hangfire     ConsoleSample.Services::Random(1)
    DevContext   DevContext.Core.Tests.NullLogger`1::Log(5)

Four repos, one shape: a method whose name collides with a BCL type the repo also references
(`Type`, `Convert`, `Random`, `Log`). **Cause, re-verified in the source rather than read off the
audit:** `SymbolTable.Resolve` answers with a `SymbolKind`, and its MEMBER tier fires only when no
TYPE candidate exists (`SymbolTable.cs:326` — `KindFromCanonical`, "contains `::` → Member"). Three
consumers took that canonical straight to `NodeId.ForType`:

| site | before | |
|---|---|---|
| `Graph2/CallGraphBinder.cs:209` | `is not { Kind: SymbolKind.Type }` | already correct |
| `Graph/GraphBuilder.Seams.cs:429` | `is { } symId` | **Kind-blind** |
| `Graph/GraphBuilder.Seams.cs:597` | `is { } symId` | **Kind-blind** |
| `Graph2/Seams/PlainCallDetector.cs:54` | `is null` | **Kind-blind** |

`PlainCallDetector`'s own comment said *"resolves to an in-solution **type**"* while its test said
*"is not null"* — the doc comment was right and the code was wrong, which is the third time this
program has found that pair. All three now gate on `SymbolKind.Type`, and a member answer is treated
as **no** answer: the invocation is dropped exactly as an unresolved framework receiver already is.
That is what stops the 26 phantom in-edges Hangfire's formatter fragment had collected — 4.2% of
that repo's entire call graph.

## Red-first

`red-first.ps1` stashes the three **behavioural** files (`CodeGraph.cs`, `GraphBuilder.Seams.cs`,
`PlainCallDetector.cs`) and leaves `SymbolCanon.cs` — its addition is two pure predicates called from
nowhere in the stashed set, so it is inert pre-fix. Stashing all four instead makes the pre-fix run a
*compile error*, which proves the helper is new and proves nothing about the graph.

    PRE-FIX    Failed: 7, Passed: 20, Total: 27
               No_node_carries_kind_Type_with_a_member_id                  FAIL
               Lambda_or_expression_text_never_becomes_a_Type_node  x5     FAIL
               A_call_whose_receiver_resolves_to_a_MEMBER_...             FAIL
    POST-FIX   Failed: 0, Passed: 27, Total: 27

Full log: `red-first-run.log`. **Its `POSTFIX_EXIT=1` line is a false red** — the scripted
`git stash pop` raced the dotnet build holding the files and silently failed ("The stash entry is
kept in case you need it again", no conflict line), so that leg ran against un-restored code. The
pop was completed by hand and the post-fix run above was re-run after it.

## Gates

    dotnet build DevContext.slnx              0 warnings / 0 errors
    Core.Tests   Category!=Eval               PLACEHOLDER_CORE
    Server.Tests Category!=Eval               PLACEHOLDER_SERVER
    Eval matrix  Category=Eval                PLACEHOLDER_EVAL
    loom-guards.ps1                           PLACEHOLDER_GUARDS

`scripts/loom-guards.ps1` **rule 11** is new: a resolved symbol may not become a Type node id without
a `SymbolKind.Type` gate within three lines. The graph can no longer show that regression — which is
precisely why the source has to keep saying it.

## V1.2 not regressed

The member-title probe re-run on the same nine poles (`member-title-after-v13.txt`):
Member **1714 match / 0 MISMATCH** (V1.2 measured 1716/0 on seven poles; the two missing Member
nodes are seam ORIGINS whose only reason to exist was a seam whose target was a phantom, and they
leave with it). **Type mismatch 58 -> 14** on the same rule -- #18 was the bulk of it, as V1.2
predicted. EntryPoint stays 84/87: that is #17's open other half (the scheme-prefix split), untouched
here and still open.

## Residue, named rather than swept up

1. **`typeof(X)` registrations lose their edge** — 25 of the 76 refused nodes are `typeof(...)`
   texts whose argument *is* a real type name (MediatR 20, CleanArchitecture 3, eShop 1, AutoMapper
   1). Open-generic DI registration is the only way to register an open generic, so MediatR's whole
   behaviour pipeline is wired this way. Before V1.3 the engine drew those edges between two nodes
   titled `typeof(...)` — garbage on both ends; now it draws nothing. The fix is one normalisation
   (unwrap whole-text `typeof(X)` before `ResolveName`), but it *adds* resolved edges, which is a
   content change belonging to **E1**'s matrix, not to a stage whose contract is zero blast radius.
   **Filed as conductor bug #3.**
2. **`type.Assembly`** (FluentValidation) — a dotted expression with no syntax at all is
   indistinguishable from a nested type by any rule at this layer. Not claimed as caught. It needs
   the detector fixed, not a name rule.
3. **The seven wider-column nodes** are untouched and listed above. `<OnModelCreating>` and eShop's
   `2.0`/`1.0` are worth a look one day; the channel node must stay.
