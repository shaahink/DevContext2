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

    public IEnumerable<SeamMatch> Detect(BodyFacts body, SeamContext ctx)
    {
        if (ctx.Symbols is null) yield break;

        var emitted = new HashSet<string>(StringComparer.Ordinal);

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
            if (resolved.Resolved is not { Kind: SymbolKind.Type }) continue;

            // Batch C (DC4) — receiver CHAIN hop. `_appEnvironmentService.OrderService.CreateOrderAsync()`
            // used to emit Calls → IAppEnvironmentService: the aggregator that HOLDS the collaborator,
            // never the collaborator. Every eShop [RelayCommand] read as a bare DI interface because of
            // it. When the receiver's trailing segment is a property of the resolved receiver type, the
            // call lands on that property's type. Unresolvable → the receiver type stands (still true).
            // The static arm passes NO receiver member to the hop: there the trailing segment IS the
            // type name (`Utilities.RazorCodeVirtualizer`), not a property of the resolved type.
            var hopMember = inv.ReceiverType is null ? null : inv.ReceiverMember;
            var target = SeamDetectorHelpers.Resolve(receiverRef, ctx);
            if (ctx.Symbols.HopThroughProperty(resolved.Resolved.Value.Canonical, hopMember, receiverRef.Site) is { } hopped)
                target = ctx.Symbols.Resolve(new SymbolRef { Text = hopped, Site = receiverRef.Site });

            yield return new SeamMatch(
                body.Member, EdgeKind.Calls, target,
                0.5f, $"{body.File}:{inv.Line}", Id)
            {
                // Batch E: the method name was already in hand here and was dropped on the floor. It is
                // the difference between "this endpoint calls IDashboardClient" and "this endpoint calls
                // IDashboardClient.GetGrains".
                TargetMember = inv.MethodName,
            };
        }
    }

    private static string? ShortName(string? typeText)
    {
        if (string.IsNullOrEmpty(typeText)) return null;
        var dot = typeText.LastIndexOf('.');
        return dot >= 0 && dot < typeText.Length - 1 ? typeText[(dot + 1)..] : typeText;
    }
}
