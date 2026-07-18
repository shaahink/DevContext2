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
    // Internal: SemanticLitePopulator unions the detectors' verb sets into its arg-bind demand gate,
    // so a verb added here is automatically part of the semantic bind demand.
    internal static readonly HashSet<string> Verbs = new(StringComparer.Ordinal)
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

            if (!IsMediatRReceiver(recv, inv) || inv.Args.IsDefaultOrEmpty) continue;

            var target = SeamDetectorHelpers.ResolveArgTarget(inv.Args[0], body);
            if (target is null) continue;

            yield return new SeamMatch(
                body.Member, EdgeKind.Sends, SeamDetectorHelpers.Resolve(target, ctx),
                0.7f, $"{body.File}:{inv.Line}", Id);
        }
    }

    /// <summary>Recognises a MediatR dispatch receiver across three shapes, in precedence order:
    /// (1) a resolved receiver type naming a MediatR contract (<c>ISender</c>/<c>IMediator</c>…) — authoritative;
    /// (2) a property-accessed receiver whose trailing segment is a MediatR name (<c>services.Mediator</c> →
    /// <c>Mediator</c>), where the resolved type is the container's, not the sender's; (3) an unresolved bare
    /// receiver whose identifier is a conventional sender name (<c>sender</c>/<c>_mediator</c>). A resolved
    /// non-MediatR bare receiver (<c>IEmailSender sender</c>) deliberately fails all three — the type is
    /// trusted over the variable name, so a stray <c>sender</c> does not fabricate a dispatch.</summary>
    private static bool IsMediatRReceiver(string? resolvedReceiverType, InvocationOp inv)
    {
        if (resolvedReceiverType is not null && Receivers.Contains(resolvedReceiverType))
            return true;
        if (inv.ReceiverMember is { } member && (Receivers.Contains(member) || NameHints.Contains(member)))
            return true;
        return resolvedReceiverType is null && inv.ReceiverText is { } root && NameHints.Contains(root);
    }

    /// <summary>Short (unqualified) type name from a possibly fully-qualified type text.</summary>
    private static string? ShortName(string? typeText)
    {
        if (string.IsNullOrEmpty(typeText)) return null;
        var dot = typeText.LastIndexOf('.');
        return dot >= 0 && dot < typeText.Length - 1 ? typeText[(dot + 1)..] : typeText;
    }
}
