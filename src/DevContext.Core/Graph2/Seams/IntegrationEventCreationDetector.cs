using DevContext.Core.Graph;

namespace DevContext.Core.Graph2.Seams;

/// <summary>Detects the creation of an integration-event contract (<c>new BasketCheckoutEvent{…}</c> or
/// <c>dto.Adapt&lt;BasketCheckoutEvent&gt;()</c>). This anchors the event's provenance at the member that
/// builds it even when the subsequent publish receiver can't be resolved — a robustness net beside
/// <see cref="BusPublishDetector"/>. Emits <see cref="EdgeKind.Raises"/>; the assembler dedups.</summary>
public sealed class IntegrationEventCreationDetector : ISeamDetector
{
    public string Id => "IntegrationEventCreation";

    public IEnumerable<SeamMatch> Detect(BodyFacts body, SeamContext ctx)
    {
        foreach (var op in body.Ops)
        {
            switch (op)
            {
                case CreationOp c when IsIntegrationEvent(c.Type.Text, ctx):
                    yield return Emit(body, c.Type, ctx, c.Line);
                    break;
                case LocalDeclOp l when l.InferredFrom is { } from && IsIntegrationEvent(from.Text, ctx):
                    yield return Emit(body, from, ctx, l.Line);
                    break;
            }
        }
    }

    private SeamMatch Emit(BodyFacts body, SymbolRef target, SeamContext ctx, int line)
        => new(body.Member, EdgeKind.Raises, SeamDetectorHelpers.Resolve(target, ctx),
            0.6f, $"{body.File}:{line}", Id);

    private static bool IsIntegrationEvent(string name, SeamContext ctx)
        => ctx.IntegrationEventTypes.Contains(name)
        || name.EndsWith("IntegrationEvent", StringComparison.Ordinal);
}
