using System.Collections.Immutable;

namespace DevContext.Core.Graph.Seams;

/// <summary>Declarative descriptor for a dispatch seam pattern: a known interface with known verbs
/// that produce edges of a given <see cref="EdgeKind"/> when the receiver matches. Adding a new bus
/// library support is data, not code — register a descriptor here.</summary>
public sealed record DispatchSeamDescriptor(
    /// <summary>Gating signal key or empty string for always-on (e.g. MediatR which is detected
    /// separately). Future: only descriptors whose signal is present activate.</summary>
    string SignalKey,
    /// <summary>Short names of receiver types (interface/class).</summary>
    ImmutableArray<string> ReceiverTypes,
    /// <summary>Verbs on the receiver that constitute a dispatch.</summary>
    ImmutableArray<string> Verbs,
    EdgeKind Kind,
    float Confidence);

/// <summary>Catalog of known dispatch interface→verb mappings. Ordered by specificity — MediatR
/// (ubiquitous) last so more specific library descriptors match first.</summary>
public static class DispatchSeamCatalog
{
    public static readonly ImmutableArray<DispatchSeamDescriptor> All =
    [
        new("wolverine", ["IMessageBus", "IMessageContext"], ["SendAsync", "InvokeAsync", "PublishAsync"], EdgeKind.Sends, 0.55f),
        new("nservicebus", ["IMessageSession", "IEndpointInstance"], ["Send", "SendLocal", "Publish"], EdgeKind.Sends, 0.55f),
        new("masstransit", ["IPublishEndpoint", "IBus", "ISendEndpoint", "ISendEndpointProvider"], ["Publish", "Send"], EdgeKind.Sends, 0.55f),
        new("rebus", ["IBus"], ["Send", "Publish", "Reply", "Defer"], EdgeKind.Sends, 0.55f),
        new("azure-servicebus", ["ServiceBusSender", "ServiceBusClient"], ["SendMessageAsync"], EdgeKind.Sends, 0.55f),
        // MediatR last — it's the most common and has its own detector (ISender/IPublisher)
        new("", ["IMediator", "ISender", "IPublisher"], ["Send", "SendAsync", "Publish", "PublishAsync"], EdgeKind.Sends, 0.55f),
    ];

    /// <summary>True when <paramref name="receiverShortName"/> matches any descriptor's 
    /// <see cref="DispatchSeamDescriptor.ReceiverTypes"/> and the <paramref name="verb"/> appears
    /// among that descriptor's <see cref="DispatchSeamDescriptor.Verbs"/>.</summary>
    public static bool IsKnownReceiver(string receiverShortName, string verb, out float confidence)
    {
        foreach (var d in All)
        {
            foreach (var rt in d.ReceiverTypes)
            {
                if (string.Equals(rt, receiverShortName, StringComparison.Ordinal))
                {
                    if (d.Verbs.Contains(verb, StringComparer.Ordinal))
                    {
                        confidence = d.Confidence;
                        return true;
                    }
                }
            }
        }
        confidence = 0;
        return false;
    }

    /// <summary>True when <paramref name="verb"/> appears among any descriptor's verbs —
    /// the bare-verb fallback for unresolvable receivers (I1.3).</summary>
    public static bool IsKnownVerb(string verb)
    {
        foreach (var d in All)
        {
            if (d.Verbs.Contains(verb, StringComparer.Ordinal))
                return true;
        }
        return false;
    }
}
