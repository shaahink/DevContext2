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

        foreach (var op in body.Ops)
        {
            if (op is not InvocationOp inv || inv.ReceiverType is null) continue;
            var receiverText = inv.ReceiverType.Text;
            if (string.IsNullOrEmpty(receiverText)) continue;

            // Skip invocations whose verbs are already captured by MediatR/Bus/DomainEvent detectors.
            if (DispatchVerbs.Contains(inv.MethodName)) continue;
            if (DomainVerbs.Contains(inv.MethodName)) continue;
            var sn = ShortName(receiverText);
            if (sn is not null && DispatchClassifier.IsBusReceiver(sn, inv.MethodName)) continue;

            // Dedupe by (receiver-type, line) so multi-arg overloads emit exactly one Calls edge.
            var key = $"{receiverText}@{inv.Line}";
            if (!emitted.Add(key)) continue;

            // Only emit when the receiver type resolves to an in-solution type (SymbolTable has it).
            // Framework types (ILogger, IMapper, HttpClient, DbContext wrappers, etc.) return
            // Resolved==null and are silently excluded — no phantom type nodes, no noise.
            var resolved = ctx.Symbols.Resolve(inv.ReceiverType);
            if (resolved.Resolved is null) continue;

            yield return new SeamMatch(
                body.Member, EdgeKind.Calls, SeamDetectorHelpers.Resolve(inv.ReceiverType, ctx),
                0.5f, $"{body.File}:{inv.Line}", Id);
        }
    }

    private static string? ShortName(string? typeText)
    {
        if (string.IsNullOrEmpty(typeText)) return null;
        var dot = typeText.LastIndexOf('.');
        return dot >= 0 && dot < typeText.Length - 1 ? typeText[(dot + 1)..] : typeText;
    }
}
