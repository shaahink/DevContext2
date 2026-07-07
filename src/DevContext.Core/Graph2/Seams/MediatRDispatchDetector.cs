using DevContext.Core.Graph;
using DevContext.Core.Graph.Seams;

namespace DevContext.Core.Graph2.Seams;

/// <summary>Detects MediatR-style request dispatch (<c>sender.Send(cmd)</c>, <c>mediator.Publish(n)</c>).
/// Resolves the dispatched contract through <see cref="BodyFacts"/> (the <c>var cmd = request.Adapt&lt;T&gt;()</c>
/// + <c>sender.Send(cmd)</c> pattern — the audit's E1 flow) rather than a right-to-left text scan.
/// MassTransit/other bus receivers are excluded here — <see cref="BusPublishDetector"/> owns them.</summary>
public sealed class MediatRDispatchDetector : ISeamDetector
{
    public string Id => "MediatRDispatch";

    private static readonly HashSet<string> Receivers = new(StringComparer.Ordinal)
    {
        "IMediator", "ISender", "IPublisher", "Mediator", "Sender",
    };
    private static readonly HashSet<string> Verbs = new(StringComparer.Ordinal)
    {
        "Send", "SendAsync", "Publish", "PublishAsync",
    };
    private static readonly HashSet<string> NameHints = new(StringComparer.Ordinal)
    {
        "sender", "_sender", "mediator", "_mediator", "mediatr", "_mediatr",
    };

    public IEnumerable<SeamMatch> Detect(BodyFacts body, SeamContext ctx)
    {
        foreach (var op in body.Ops)
        {
            if (op is not InvocationOp inv || !Verbs.Contains(inv.MethodName)) continue;

            var recv = inv.ReceiverType?.Text;

            // A bus receiver with the same verb belongs to BusPublishDetector — do not double-emit.
            if (recv is not null && DispatchClassifier.IsBusReceiver(recv, inv.MethodName)) continue;

            var isMediatR = recv is not null
                ? Receivers.Contains(recv)
                : inv.ReceiverText is not null && NameHints.Contains(inv.ReceiverText);
            if (!isMediatR || inv.Args.IsDefaultOrEmpty) continue;

            var target = SeamDetectorHelpers.ResolveArgTarget(inv.Args[0], body);
            if (target is null) continue;

            yield return new SeamMatch(
                body.Member, EdgeKind.Sends, SeamDetectorHelpers.Resolve(target, ctx),
                0.7f, $"{body.File}:{inv.Line}", Id);
        }
    }
}
