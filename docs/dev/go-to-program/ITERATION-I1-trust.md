# Iteration I1 — Trust at breadth (hardening the seams)

> **Status: NOT STARTED** · Phase: V1 · Prereq: none · One session · Gate at the end.
> Written for agent execution (DeepSeek v4 Pro). Verify every `file:line` against current code before
> editing (Step 0). Audit context: `ENGINE-VALUE-AUDIT.md` §5.

## Goal

The wiring seams are correct on arbitrary .NET code, not eShop-shaped code. Ships: the span-bounding
bug fix, receiver-typed dispatch matching over a declarative catalog, model-derived event sets, the
pattern-zoo corpus, and two hygiene quickies.

## Step 0 — Reproduce (do this first, commit nothing)

1. `pwsh -File eval/gates.ps1` — must be green before you start (else stop and report).
2. Write the failing test that proves the bug (it becomes the regression guard):

```csharp
// tests/DevContext.Core.Tests/GraphBuilderSpanTests.cs — expected to FAIL before the fix
[Fact]
public async Task Send_of_parameter_does_not_steal_sibling_methods_new()
{
    // One class, two methods: A() news up AlphaCommand; B(cmd) sends its PARAMETER.
    const string src = """
        public class Dispatcher {
            private readonly IMediator _m;
            public Task A() => _m.Send(new AlphaCommand());
            public Task B(BetaCommand cmd) => _m.Send(cmd);   // must NOT become Sends→AlphaCommand
        }
        """;
    var graph = await BuildGraphFromSource(src);              // use the existing test pipeline helper
    var bMember = graph.Nodes.Single(n => n.Id.Key.EndsWith("Dispatcher.B"));
    var sends = graph.OutEdges(bMember.Id, EdgeKind.Sends);
    Assert.DoesNotContain(sends, e => e.To.Key.EndsWith("AlphaCommand"));
}
```

## Step 1 — Span-bound variable resolution (the bug)

**Locus:** `GraphBuilder.cs` — `AddSends` (~:836-845, the `body[..pos]` search), `ResolveVariableNewType`
(~:1043) used by `AddRaises`. `methodSpans` (per-type `MethodSpan` arrays) are **already computed and
passed into both methods** — the fix is to use them to bound the search:

```csharp
// helper next to BodyMatchOrigin (which already does span lookup for edge origins):
private static (int Start, int End) EnclosingSpan(
    ImmutableArray<MethodSpan> spans, int pos)
{
    foreach (var s in spans)
        if (pos >= s.Start && pos < s.End) return (s.Start, s.End);
    return (0, pos); // outside any method (field init / ctor-less) — search from type start as before
}

// in AddSends, variable branch — replace `var before = body[..pos];` with:
var (spanStart, _) = EnclosingSpan(spans, match.Index);
var before = body[spanStart..pos];               // never cross the method boundary
```

Same change inside `ResolveVariableNewType` (thread the span start in as a parameter — do NOT re-derive
spans there; keep one lookup at the call site). **When the in-span search finds nothing: emit no edge.**
Honesty beats a cross-method guess. Step 2 adds the honest recovery for the param case.

**Pitfalls:** (a) `MethodSpan` offsets are indices into `SourceBody`, not file offsets — don't mix with
`GetLocation()`; (b) expression-bodied members have small spans — the corpus (Step 5) covers them;
(c) keep `BodyMatchOrigin` untouched — origin anchoring already works.

## Step 2 — In-span dataflow-lite (param/field fallback)

When `.Send(x)` has no in-span `new`: resolve `x` as (1) a **parameter** of the enclosing method —
parameter names+types are on the `MethodSignature` in the model; (2) a **field** of the type — the
field map `CallGraphExtractor.BuildFieldMap` builds already has this shape (don't rebuild it; if it's
not reachable from GraphBuilder, extract the small name→type dictionary to a shared helper). Emit the
edge with `Confidence = 0.5f` and `Resolution.Syntactic`. This closes the long-aspirational eShop
`/draft` (command arrives as a method parameter) — flip that expectation to `expected` when it passes.

## Step 3 — Receiver-typed dispatch + `DispatchSeamCatalog`

**Problem:** `\.(Send|SendAsync|Publish|PublishAsync)\(` fires on ANY receiver (`SmtpClient.Send` mints
a `Sends` edge) and misses other buses' verbs. **Design — one declarative catalog, mirroring
`EntrySurfaceCatalog`:**

```csharp
// src/DevContext.Core/Graph/Seams/DispatchSeamCatalog.cs
public sealed record DispatchSeamDescriptor(
    string    SignalKey,                    // gates which entries are active ("" = always, for MediatR-style)
    ImmutableArray<string> ReceiverTypes,   // interface/class SHORT names: "IMediator","ISender","IPublisher",
                                            // "IPublishEndpoint","IBus","IMessageBus","ITopicClient","ServiceBusSender"
    ImmutableArray<string> Verbs,           // "Send","SendAsync","Publish","PublishAsync","InvokeAsync","Request"
    EdgeKind  Kind,                         // Sends
    float     Confidence);
public static class DispatchSeamCatalog { public static readonly ImmutableArray<DispatchSeamDescriptor> All = [ /* MediatR, MassTransit, NServiceBus, Wolverine, Rebus, Azure SB */ ]; }
```

**Matching (two tiers, cheapest wins):**
1. **Identifier-typed tier (no Roslyn needed):** the regex captures the receiver identifier too:
   `(\w+)\.(Verb)\(...`. Resolve the identifier against the *same* name→type sources as Step 2
   (fields, ctor params, method params — all syntactic data we hold). If the resolved type's short name
   ∈ any active descriptor's `ReceiverTypes` → emit with that descriptor's confidence.
2. **Unknown receiver:** if the identifier can't be typed (chained call, local var, static) fall back
   to the current behavior **only for verbs in an active descriptor**, with `Confidence` lowered
   (0.35f) — and only when the argument resolves to a type the model knows (kills `SmtpClient.Send(msg)`
   because `MailMessage` resolves but... it may be known too — therefore ALSO require the arg type to
   carry a request/notification marker per Step 4's type sets, or to be consumed by a known handler).
   The conjunction (verb ∈ catalog) ∧ (arg ∈ event/request type-set) is the false-positive killer.

**Do not** try full SemanticModel here — that's the deferred semantic tier; this design is its
syntactic approximation and remains the fallback tier after it.

## Step 4 — Model-derived event/request type sets

**Locus:** `AddRaises` (`GraphBuilder.cs:709-772`). Replace the `new (\w*IntegrationEvent\w*)` name
regex and supplement the `{AddDomainEvent,RaiseDomainEvent,AddEvent}` verb list:

```csharp
// Build once per Build() from data already in the model:
var eventTypes = model.Types.Values
    .Where(t => t.BaseTypes.Any(b => KnownEventBases.Contains(StripGenerics(b)))       // "IntegrationEvent","DomainEvent"
             || t.ImplementedInterfaces.Any(i => KnownEventInterfaces.Contains(StripGenerics(i)))) // "INotification","IDomainEvent","IEvent"
    .Select(t => t.Name).ToHashSet(StringComparer.Ordinal);
```

Then: every `new X(...)` in a body where `X ∈ eventTypes` ⇒ `Raises` candidate (attributed to the
enclosing member span, Step 1 rules). Keep the name-regex as a **last-resort** tier with lower
confidence for repos that define events with zero base type (rare; document it). Request/command set
analog (`IRequest<>`, `ICommand`) feeds Step 3's conjunction.

## Step 5 — Pattern-zoo corpus

`tests/fixtures/PatternZoo/` — ONE project, one file per syntax shape, each containing a *known seam*
(a `Send(new PingCommand())`) wrapped in: primary constructor, expression body, top-level statements,
record with body, local function, partial method, `required` init, collection expressions, `#if` block,
and **a raw string literal containing `.Send(new FakeCommand())`** (must NOT produce an edge — this is
a real regex trap today; if it fails, either strip string literals before the body scan (a small
lexer-level pre-pass: replace string/raw-string contents with spaces preserving offsets, so spans stay
valid) or document + guard). `PatternZooTests` = one parameterized test asserting per-file: expected
edge present, forbidden edge absent, no crash. Grow it forever; it's the standing anti-overfit harness.

## Step 6 — Hygiene quickies

1. `ExtractionOptions.cs:18` default excludes += `"eval-repos", "analysis-repos"` — and **unify** the
   three duplicated literal lists (`AnalyzeCommand.cs:123`, `Desktop/AnalysisService.cs:132`,
   `Server/EngineRunner.cs:28`) to reference `ExtractionOptions.DefaultExcludes`.
2. `tests/DevContext.Core.Tests/ExtractorConventionTests.cs`: reflection over `[DiscoveryAssembly]`
   extractors asserting every Stage-3 extractor (a) has a non-trivial `ShouldRun` (signal-gated or
   documented always-run), (b) contains no direct `CSharpSyntaxTree.ParseText` call (must use the
   parse cache) — assert via source scan of `src/DevContext.Core/Extractors/**` (simple text check is
   fine and honest), (c) skips non-`.cs` when iterating `AllSourceFiles`.
3. Multi-impl honesty (audit §5.6.2): where the trace resolves interface→impl via `Resolves`, if DI
   has >1 impl, append `[×N impls]` to the rendered hop. Data: group `DiRegistrationDetection` by
   `ServiceType`. Renderer-only.

## Docs & goldens (same commit as the behavior change)

- Eval: new negative fixtures (SmtpClient, sibling-new, raw-string); flip eShop `/draft` aspirational →
  expected; re-run `eval/gates.ps1` — **expect the F3 seam-count goldens to move** (Sends/Raises counts
  will drop where fabrications die): re-baseline them deliberately, record before/after counts in the
  commit message.
- `docs/product/TRACE-RULE-REFERENCE.md`: document the two-tier dispatch matching + catalogs.
- `docs/product/cli-reference.md`: unchanged this iteration (no flag changes).

## Gate

`eval/gates.ps1` green · Step-0 test green · PatternZoo green · seam-count deltas explained in the
commit · eShop flagship trace unchanged except removed fabrications (eyeball it, capture in commit).
