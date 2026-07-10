using DevContext.Core.Graph;

namespace DevContext.Core.Graph2.Seams;

/// <summary>Detects domain-event raises inside aggregates/entities: <c>AddDomainEvent(new OrderCreatedEvent(…))</c>,
/// <c>RaiseDomainEvent(evt)</c>, <c>Raise(evt)</c>. The raised type resolves from the argument's inline
/// creation (captured as <see cref="ArgFact.Type"/>) or a correlated local declaration. Emits
/// <see cref="EdgeKind.Raises"/>.</summary>
public sealed class DomainEventRaiseDetector : ISeamDetector
{
    public string Id => "DomainEventRaise";

    private static readonly HashSet<string> RaiseVerbs = new(StringComparer.Ordinal)
    {
        "AddDomainEvent", "RaiseDomainEvent", "Raise", "AddEvent", "ApplyEvent",
    };

    public IEnumerable<SeamMatch> Detect(BodyFacts body, SeamContext ctx)
    {
        foreach (var op in body.Ops)
        {
            if (op is not InvocationOp inv || !RaiseVerbs.Contains(inv.MethodName)) continue;
            if (inv.Args.IsDefaultOrEmpty) continue;

            var target = SeamDetectorHelpers.ResolveArgTarget(inv.Args[0], body);
            if (target is null) continue;

            // Guard the generic-verb "Raise" against non-event targets when the classification is known.
            if (inv.MethodName == "Raise"
                && !ctx.DomainEventTypes.IsEmpty
                && !ctx.DomainEventTypes.Contains(target.Text)
                && !target.Text.EndsWith("Event", StringComparison.Ordinal))
                continue;

            yield return new SeamMatch(
                body.Member, EdgeKind.Raises, SeamDetectorHelpers.Resolve(target, ctx),
                0.65f, $"{body.File}:{inv.Line}", Id);
        }
    }
}
