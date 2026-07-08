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

            // Receiver-type dispatch gating (L3.2): when a semantic bind resolved the receiver's type,
            // gate on that type's short name; fall back to the syntactic declared type, then to the
            // variable-name hint. The short name is taken from the last dotted segment so a fully-
            // qualified bound type (e.g. MediatR.ISender) still matches the catalog.
            var recv = ShortName(inv.ReceiverType?.Text);

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

    /// <summary>Short (unqualified) type name from a possibly fully-qualified type text.</summary>
    private static string? ShortName(string? typeText)
    {
        if (string.IsNullOrEmpty(typeText)) return null;
        var dot = typeText.LastIndexOf('.');
        return dot >= 0 && dot < typeText.Length - 1 ? typeText[(dot + 1)..] : typeText;
    }
}
