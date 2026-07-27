using DevContext.Core.Graph;

namespace DevContext.Core.Graph2.Seams;

/// <summary>Detects message-bus publishes/sends (<c>publishEndpoint.Publish(evt)</c>, <c>bus.Send(msg)</c>).
/// Emits a <see cref="EdgeKind.Raises"/> seam for the published contract — the cross-service hop
/// (publisher → bus → consumer) is materialised by the assembler joining this against the declared
/// <c>IConsumer&lt;T&gt;</c>. The published type resolves through the body's LocalDecl inference
/// (<c>var evt = dto.Adapt&lt;BasketCheckoutEvent&gt;()</c> then <c>publishEndpoint.Publish(evt)</c>).</summary>
public sealed class BusPublishDetector : ISeamDetector
{
    public string Id => "BusPublish";

    private static readonly HashSet<string> NameHints = new(StringComparer.Ordinal)
    {
        "publishEndpoint", "_publishEndpoint", "bus", "_bus", "endpoint",
        "sendEndpoint", "_sendEndpoint", "messageBus", "_messageBus", "session",
    };
    // Internal: SemanticLitePopulator unions the detectors' verb sets into its arg-bind demand gate,
    // so a verb added here is automatically part of the semantic bind demand.
    internal static readonly HashSet<string> Verbs = new(StringComparer.Ordinal)
    {
        "Publish", "PublishAsync", "Send", "SendAsync", "SendMessageAsync",
    };

    public IEnumerable<SeamMatch> Detect(BodyFacts body, SeamContext ctx)
    {
        foreach (var op in body.Ops)
        {
            if (op is not InvocationOp inv || !Verbs.Contains(inv.MethodName)) continue;

            var recv = inv.ReceiverType?.Text;
            float confidence;
            if (recv is not null)
            {
                if (!DispatchClassifier.TryMatchBus(recv, inv.MethodName, out confidence)) continue;
            }
            else
            {
                if (inv.ReceiverText is null || !NameHints.Contains(inv.ReceiverText)) continue;
                confidence = 0.5f; // name-only heuristic, lower confidence
            }

            if (inv.Args.IsDefaultOrEmpty) continue;
            var target = SeamDetectorHelpers.ResolveArgTarget(inv.Args[0], body);
            if (target is null) continue;

            yield return new SeamMatch(
                body.Member, EdgeKind.Raises, SeamDetectorHelpers.Resolve(target, ctx),
                confidence, $"{body.File}:{inv.Line}", Id);
        }
    }
}
