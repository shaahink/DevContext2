# D1.3 leftover (b) — per-detection `Confidence`, read or deleted

Session 23, 2026-08-14. Carried over from s22's D1.3 amendment; the audit files it as §9 item 5,
"a float nothing consumes is a future #25".

## The measurement s22 asked for, run

s22 recorded: *"I measured that no reader exists in Core (every `.Confidence` read belongs to
Insight/GraphEdge/FeatureSignal/DispatchSeamDescriptor), but the honest verdict is the compile-based
sweep: delete `Detection.Confidence`, build, classify each error as a read or a write."*

Done exactly that. `Detection.Confidence` deleted, `dotnet build DevContext.slnx`:
**29 errors** (`confidence-delete-sweep.txt`), and they split cleanly:

| Error | Meaning | Count |
|---|---|---|
| `CS0117` — type has no member `Confidence` | object-initializer **write** | **28** |
| `CS1061` — no definition for `Confidence` on an instance | **read** | **1** |

## The prediction is refuted

The one read is `src/DevContext.Core/Graph/GraphBuilder.Nodes.cs:690`:

```csharp
g.AddEdge(new GraphEdge(publisherId, channelId, EdgeKind.Raises)
{
    Provenance = $"{ef.SourceFile}:{ef.LineNumber}",
    Resolution = Resolution.Syntactic,
    Confidence = ef.Confidence,          // <- EventFlowDetection.Confidence, read
});
```

fed by `EventBusExtractor.cs:253`, whose own comment says what it is for:
`Confidence = 0.6f, // syntax-only channel join — renders [approx]`.

So the field is **not dead**. It is read for exactly one detection type, and written by 27 sites
whose numbers (hand-picked constants from 0.6f to 0.95f, across 17 extractors) reach nothing.

**Why a grep could not have found this, and the compiler could.** The read is `ef.Confidence`
assigned *into* `GraphEdge.Confidence` — textually identical to the `GraphEdge` reads s22 correctly
attributed elsewhere. Only deleting the member and reading the compiler's answer separates a write
from a read.

## The resolution: relocate, not delete

`Confidence` now lives on `EventFlowDetection` (`InMemoryEventBusExtractor.cs`), with the same
`1.0f` default it had on the base — so every `EventFlowDetection` constructed without it keeps the
value it had before, and the one that sets `0.6f` still does. **No behaviour change is possible**:
the 27 deleted writes were, by the compiler's own account, read by nothing.

- `-27` unread writes across 17 extractor files
- `+1` property on the type that is actually read, carrying a doc comment that names its single
  consumer and says not to put it back on the base

## Verification

- `dotnet build DevContext.slnx` — **0 warnings / 0 errors**, whole solution. That is the part that
  matters beyond Core: the first sweep stopped at Core's own errors, so `Server`, `Mcp`, `Cli` and
  both test projects had not been compiled against the deletion yet. They compile clean, so there is
  no reader outside Core either.
- `dotnet test DevContext.slnx --filter "Category!=Eval"` — see `confidence-regression.txt`.

## What this leaves open

D1.3's other leftover, **#2's detection half** (addressable entry names single-sourced between the
`entrypoints` render and what `get_context`/`trace` need), is untouched and still open.
