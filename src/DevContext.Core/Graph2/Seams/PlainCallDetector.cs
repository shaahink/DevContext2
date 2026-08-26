using DevContext.Core.Graph;

namespace DevContext.Core.Graph2.Seams;

/// <summary>Detects plain method calls that form the call-spine in non-CQRS repos (L7.1).
/// Reads <see cref="InvocationOp"/>s from <see cref="BodyFacts"/> and emits <see cref="EdgeKind.Calls"/>
/// seams for every invocation whose receiver type resolves to an in-solution type.
/// Framework types (ILogger, IMapper, etc.) and invocations already handled by specialised detectors
/// (MediatR dispatch, bus publish, domain events) are excluded — they would either create phantom edges
/// or redundant parallel edges.</summary>
public sealed class PlainCallDetector : ISeamDetector
{
    public string Id => "PlainCall";

    /// <summary>Method verbs already handled by <see cref="MediatRDispatchDetector"/> — skipped to avoid
    /// redundant parallel edges (the spine already prioritises Sends over Calls).</summary>
    private static readonly HashSet<string> DispatchVerbs = new(StringComparer.Ordinal)
    {
        "Send", "SendAsync", "Publish", "PublishAsync", "PublishNotification", "CreateStream",
    };

    /// <summary>Method verbs already handled by <see cref="DomainEventRaiseDetector"/>.</summary>
    private static readonly HashSet<string> DomainVerbs = new(StringComparer.Ordinal)
    {
        "AddDomainEvent", "RaiseDomainEvent", "Raise",
    };

    /// <summary>F1 integration repair (2026-08-27): the confidence of a DEGRADED call — the receiver
    /// type is verified in-solution but the called member is one its visible hierarchy does not
    /// declare (inherited from an out-of-solution base, or an extension method). Below the 0.5f of a
    /// declared-target call so spine selection keeps preferring vouched targets.</summary>
    private const float DegradedCallConfidence = 0.4f;

    public IEnumerable<SeamMatch> Detect(BodyFacts body, SeamContext ctx)
    {
        if (ctx.Symbols is null) yield break;

        var emitted = new HashSet<string>(StringComparer.Ordinal);
        var degraded = new List<SeamMatch>();

        // MEASURED CONSEQUENCE OF THE STATIC ARM BELOW, carried openly (conductor bug, E1.4/R1 to
        // decide): CodeGraphBuilder.AddEdge dedupes (From, To, Kind) FIRST-WINS, so when a member
        // both instance-calls and static-calls the same type (`Insight.For(..)` beside an instance
        // call on Insight) whichever match is emitted first decides the pair's Resolution. Emitting
        // in op order downgraded 14 of DevContext's own 1380 Calls pairs from Semantic to Syntactic
        // (0 pairs were lost). The real fix is that a merged edge should carry the BEST resolution of
        // its call sites, not the first — that is the audit's call-site-multiplicity item, not a
        // deferral trick here.
        foreach (var op in body.Ops)
        {
            if (op is not InvocationOp inv) continue;

            // Skip invocations whose verbs are already captured by MediatR/Bus/DomainEvent detectors.
            if (DispatchVerbs.Contains(inv.MethodName)) continue;
            if (DomainVerbs.Contains(inv.MethodName)) continue;

            // E1.1 (#11) — the receiver of a STATIC call names a TYPE, so it resolves from no local
            // scope and ReceiverType is null. Before this arm the detector skipped it outright, which
            // is why DevContext's own ExtractorHelpers / BodyFactExtractor had ZERO in-edges in
            // DevContext's own graph while every instance call around them bound fine. Same rule the
            // call binder uses (one rule, two producers) — unambiguous, in-solution, declares-the-method.
            var receiverRef = inv.ReceiverType;
            if (receiverRef is null)
            {
                if (ctx.Symbols.ResolveStaticReceiverType(inv, body.File) is not { } staticFqn) continue;
                receiverRef = new SymbolRef
                {
                    Text = staticFqn,
                    Site = new RefSite { File = body.File, Line = inv.Line, Project = body.Project },
                };
            }

            var receiverText = receiverRef.Text;
            if (string.IsNullOrEmpty(receiverText)) continue;
            var sn = ShortName(receiverText);
            if (sn is not null && DispatchClassifier.IsBusReceiver(sn, inv.MethodName)) continue;

            // Dedupe by (receiver-type, line) so multi-arg overloads emit exactly one Calls edge.
            var key = $"{receiverText}@{inv.Line}";
            if (!emitted.Add(key)) continue;

            // Only emit when the receiver type resolves to an in-solution TYPE (SymbolTable has it).
            // Framework types (ILogger, IMapper, HttpClient, DbContext wrappers, etc.) return
            // Resolved==null and are silently excluded — no phantom type nodes, no noise.
            //
            // V1.3 (backlog #7 rider): the sentence above says TYPE and the test said "not null",
            // and the gap is a measured defect. SymbolTable's member tier fires when NO type
            // candidate exists, so `Convert.ToInt32(...)` inside a converter class resolved onto
            // that class's own `Convert` METHOD and the seam target became a member id wearing
            // kind Type (eShop WebNavigatingEventArgsConverter::Convert(4); Hangfire's ::Type(1) is
            // the same shape, and it collected 26 phantom in-edges). A member answer is not a type
            // answer — drop the invocation, exactly as an unresolved framework receiver is dropped.
            var resolved = ctx.Symbols.Resolve(receiverRef);
            if (resolved.Resolved is not { Kind: SymbolKind.Type } resolvedType) continue;

            // E1.2 (#12) rider, MEASURED: once the semantic upgrade relocates exactly, it names the
            // receiver of a FRAMEWORK static too (Console, Activator, Array, System.Type). Those used to
            // arrive with a null ReceiverType and only E1.1's gated static arm could speak for them
            // (in-solution + declares-the-method); now they arrive as Semantic refs, and Law R2
            // (SymbolTable.Resolve) deliberately KEEPS an out-of-solution bound id rather than discarding
            // it — so the Kind==Type check above passes and a BCL node is minted. Measured on MediatR:
            // 34 new System.* type nodes, 174 → 255 nodes, before this gate. The rule the paragraph above
            // already states ("only when the receiver type resolves to an in-solution TYPE") was being
            // enforced by accident, by nothing ever naming a framework type here. State it.
            if (!ctx.Symbols.IsKnownFqn(resolvedType.Canonical)) continue;

            // Batch C (DC4) — receiver CHAIN hop. `_appEnvironmentService.OrderService.CreateOrderAsync()`
            // used to emit Calls → IAppEnvironmentService: the aggregator that HOLDS the collaborator,
            // never the collaborator. Every eShop [RelayCommand] read as a bare DI interface because of
            // it. When the receiver's trailing segment is a property of the resolved receiver type, the
            // call lands on that property's type. Unresolvable → the receiver type stands (still true).
            // The static arm passes NO receiver member to the hop: there the trailing segment IS the
            // type name (`Utilities.RazorCodeVirtualizer`), not a property of the resolved type.
            // E1.2: `ReceiverType is null` was the marker for "static call" and it no longer is — the
            // semantic upgrade fills it for a type-name receiver too. The honest marker is the trailing
            // segment itself: for `Utilities.RazorCodeVirtualizer.Walk(x)` it IS the receiver type's own
            // name, not a property of it, so hopping through it would be nonsense.
            var hopMember = inv.ReceiverType is null ? null : inv.ReceiverMember;
            if (hopMember is not null && string.Equals(hopMember, ShortName(receiverRef.Text), StringComparison.Ordinal))
                hopMember = null;
            var target = SeamDetectorHelpers.Resolve(receiverRef, ctx);
            var targetType = resolvedType.Canonical;
            if (ctx.Symbols.HopThroughProperty(resolvedType.Canonical, hopMember, receiverRef.Site) is { } hopped)
            {
                target = ctx.Symbols.Resolve(new SymbolRef { Text = hopped, Site = receiverRef.Site });
                targetType = hopped;
            }

            // F1 (#33) — same declares oracle as CallGraphBinder.ResolveCallee's receiver arm, judged
            // AFTER the chain hop, on the type the call actually lands on; tri-state — only a positive
            // "no" demotes. A method the target type does not VISIBLY declare (declared members +
            // in-solution base walk) is somebody else's member — an extension method or a member
            // inherited from an OUT-of-solution base. The binder REFUSES those outright (a member node
            // the hierarchy cannot vouch for is never minted — INV-C). This seam is the sanctioned
            // DEGRADE of the same call (F1 integration repair, 2026-08-27): the receiver type is
            // in-solution by the gates above, the call really lands on one of its instances, and the
            // emitted shape is member→TYPE with the called name riding on the EDGE (Batch E) — no
            // member node, no phantom degree, nothing for INV-C to judge. Refusing the seam too
            // severed TRUE connectivity: TodoApi's `POST /todos/` lambda lost its only path to the
            // store (`db.SaveChangesAsync()` / `db.Todos.Add(..)` — both DbContext members, all
            // out-of-solution), and the ratcheted "TodoDbContext" truth pin caught it. Degraded
            // matches are HELD BACK until every declared-target call has been yielded, so a degraded
            // call never steals the (From, To, Kind) first-wins edge slot — nor its F4 port-bridge
            // verb evidence — from a declared call on the same receiver later in the body.
            var declares = ctx.Symbols.DeclaresMemberInHierarchy(targetType, inv.MethodName);

            var match = new SeamMatch(
                body.Member, EdgeKind.Calls, target,
                declares == false ? DegradedCallConfidence : 0.5f, $"{body.File}:{inv.Line}", Id)
            {
                // Batch E: the method name was already in hand here and was dropped on the floor. It is
                // the difference between "this endpoint calls IDashboardClient" and "this endpoint calls
                // IDashboardClient.GetGrains".
                TargetMember = inv.MethodName,
            };

            if (declares == false) { degraded.Add(match); continue; }
            yield return match;
        }

        // Degraded fallbacks last — a vouched call always claims the edge slot first.
        foreach (var match in degraded) yield return match;
    }

    private static string? ShortName(string? typeText)
    {
        if (string.IsNullOrEmpty(typeText)) return null;
        var dot = typeText.LastIndexOf('.');
        return dot >= 0 && dot < typeText.Length - 1 ? typeText[(dot + 1)..] : typeText;
    }
}
