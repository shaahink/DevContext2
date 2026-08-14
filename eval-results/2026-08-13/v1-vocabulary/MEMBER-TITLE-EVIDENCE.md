# V1.2 — ONE Member-title vocabulary (backlog #17)

**Claim:** every Member node's title is now derived from its own key by one helper, so the engine
speaks one vocabulary about a member wherever it appears. Measured, not asserted.

## What was wrong

Node titles merge **first-write-wins** (`CodeGraphBuilder.AddNode`), and a dozen producers spelled a
Member title for themselves:

| producer family | what it wrote | example |
|---|---|---|
| the 7 entry-point builders (`Graph/EntryPoints/*`) | `HandlerType + "." + method` | `CatalogApi.GetAllItemsV1` |
| `GraphBuilder.Seams.AddCallEdges` / hub broadening | `TypeNode.Title + "." + method` | `Mediator.Send` |
| `GraphBuilder.Seams.EnsureMemberId` (seam origins) | `BodyFacts.MemberName` — **bare** | `Send` |

So which vocabulary a node ended up with was decided by **pass order**, not by anything about the
node: a bare `Send` or `Validate` in a neighbours list, call stack or hub row does not say *whose*,
while eShop's own nodes said `CatalogApi.GetAllItemsV1` three rows away.

## The fix — one derivation, applied once

- `SymbolCanon.MemberTitle(memberKey)` (`src/DevContext.Core/Graph2/SymbolCanon.cs`) — the whole
  rule: owner **short** name (nested chain and generic arity stripped) + `.` + member name. It sits
  in the name algebra beside `MemberKey` / `OwnerTypeOf` / `MemberNameOf`, which is where the key it
  reads was built.
- Applied in exactly one place — `CodeGraphBuilder.AddNode` (`Graph/CodeGraph.cs`) — for
  `NodeId.Kind == Member`. A title is therefore a **function of the key**: no producer, and no
  ordering of producers, can change it.
- All 12 producer sites now pass `SymbolCanon.MemberTitle(id.Key)` so the source says what the graph
  does; `EnsureMemberId` no longer takes a `memberName` parameter at all.
- **loom-guards rule 10** fails any `new GraphNode(... NodeKind.Member)` whose title is spelled
  outside `SymbolCanon.MemberTitle` — the same enforcement shape V1.1 gave the edge tier.
- `NumberReconciliationTests.One_member_title_vocabulary_whoever_writes_the_node_first` writes the
  same node the two ways the two producer families used to, **in both orders**, and pins one title.

## Measurement — the instrument that filed the bug, re-run

`eval-results/2026-08-13/v1-vocabulary/member-title-probe.ps1` is the 2026-07-29 probe
(`eval-results/2026-07-29/G6/label-mirror-fidelity.ps1`) with the same rule character for character,
poles passed as `name=ABSOLUTE_PATH`. It derives a title from each graphdump node's **id** and
compares it to the engine's own `GraphNode.Title`, per kind. **Member MISMATCH = the size of the
second vocabulary.**

Seven poles: the six #17 was measured on, plus DevContext itself (dogfood). BEFORE was run with the
CLI built at `f5361ac` (pre-change); AFTER with the same CLI rebuilt after the edit.

| kind | BEFORE match / MISMATCH | AFTER match / MISMATCH |
|---|---|---|
| **Member** | 624 / **1092** | 1716 / **0** |
| Type | 1893 / 58 | 1893 / 58 |
| EntryPoint | 84 / 87 | 84 / 87 |
| Service | 23 / 0 | 23 / 0 |
| Store | 6 / 0 | 6 / 0 |

Per pole, Member (match / MISMATCH → match / MISMATCH):

| pole | before | after |
|---|---|---|
| eShop | 258 / 183 | 441 / 0 |
| FluentValidation | 0 / 115 | 115 / 0 |
| AutoMapper | 0 / 208 | 208 / 0 |
| MediatR | 0 / 21 | 21 / 0 |
| CleanArchitecture | 10 / 27 | 37 / 0 |
| dotnet-podcasts | 75 / 75 | 150 / 0 |
| DevContext (dogfood) | 281 / 463 | 744 / 0 |

**Blast radius: zero.** Member node COUNT is identical pole by pole (441, 115, 208, 21, 37, 150, 744
before and after) — nothing was added, merged away or lost; only the titles moved. Every other
kind's columns are unchanged to the node.

**The instrument reproduces the filed number.** Across the six original poles the owner-qualified
count BEFORE is 258+0+0+0+10+75 = **343** — exactly the 343 in backlog #17 — against 629 bare (the
bug says 627; the graph has moved by two members since 2026-07-29).

Sample of what changed (from `member-title-before.txt`, engine vs the one rule):

```
Member:AutoMapper.Features.FeatureExtensions::SetFeature
   engine='SetFeature'                      -> now 'FeatureExtensions.SetFeature'
Member:Clean.Architecture.Core.ContributorAggregate.Contributor::UpdatePhoneNumber
   engine='UpdatePhoneNumber'               -> now 'Contributor.UpdatePhoneNumber'
Member:eShop.Catalog.API.CatalogApi::<lambda> GET /api/catalog/catalogtypes
   engine='<lambda> GET /api/catalog/...'   -> now 'CatalogApi.<lambda> GET /api/catalog/...'
```

The last one is the shape worth naming: a minimal-API lambda handler now says which endpoint class
owns it. The **EntryPoint** node's own title is untouched (that surface is unchanged).

## What this does NOT close

- **The EntryPoint half of #17** (84 match / 87 mismatch, unmoved): some producers keep the key's
  scheme prefix (`grpc:`, `domain:`, `worker:`) in the title and some drop it. Not in V1.2's
  acceptance; still open, and the numbers above are its baseline.
- **#18** (58 Type mismatches): Type nodes minted from lambda/expression TEXT. Untouched by design —
  it is V1.3's neighbour, and this probe measures it for free.
- The desktop's `nodeIdLabel` (`core/format.ts`) is a *last-resort id renderer*, not a title
  producer: it prints the humanized key when no title is on hand (D-4's rule 8). It is not a third
  member vocabulary, but it does print a fuller string than the engine's title for the same node.

## Gates (fast loop, this session — the full battery is Conductor's to run)

| gate | result |
|---|---|
| `dotnet build src/DevContext.Cli` | 0 warnings / 0 errors |
| `dotnet test tests/DevContext.Core.Tests --filter Category!=Eval` | 739 passed, 2 skipped, **exit 0** |
| `dotnet test tests/DevContext.Server.Tests --filter Category!=Eval` | 108 passed, **exit 0** |
| `scripts/loom-guards.ps1` | PASSED — incl. new **rule 10** and the truth gate (0 failures) |
| `dotnet test tests/DevContext.Core.Tests --filter Category=Eval` (the matrix) | 74 passed, 9 skipped (repos absent locally), **0 failed — no cell moved** |

Logs: `.conductor/bg-logs/unit-tests4-*.log`, `loom-guards-*.log`, `eval-matrix-*.log`.

## Files

- `member-title-probe.ps1` — the instrument (re-runnable)
- `member-title-before.txt` / `member-title-after.txt` — the two runs
- unit test: `tests/DevContext.Core.Tests/NumberReconciliationTests.cs`
- guard: `scripts/loom-guards.ps1` rule 10
