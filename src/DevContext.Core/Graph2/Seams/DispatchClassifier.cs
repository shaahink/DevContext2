using DevContext.Core.Graph;
using DevContext.Core.Graph.Seams;

namespace DevContext.Core.Graph2.Seams;

/// <summary>Classifies a dispatch receiver as MediatR vs message-bus using the existing
/// <see cref="DispatchSeamCatalog"/> as data (design §2.1: "the dispatch-receiver catalog survives as
/// data for these detectors"). MediatR is the catalog's empty-signal descriptor; everything else
/// (MassTransit, NServiceBus, Wolverine, Rebus, Azure Service Bus) is a bus.</summary>
internal static class DispatchClassifier
{
    public static bool IsBusReceiver(string receiverShortName, string verb)
    {
        foreach (var d in DispatchSeamCatalog.All)
        {
            if (d.SignalKey.Length == 0) continue; // MediatR — not a bus
            if (d.ReceiverTypes.Contains(receiverShortName, StringComparer.Ordinal)
                && d.Verbs.Contains(verb, StringComparer.Ordinal))
                return true;
        }
        return false;
    }

    public static bool TryMatchBus(string receiverShortName, string verb, out float confidence)
    {
        foreach (var d in DispatchSeamCatalog.All)
        {
            if (d.SignalKey.Length == 0) continue;
            if (d.ReceiverTypes.Contains(receiverShortName, StringComparer.Ordinal)
                && d.Verbs.Contains(verb, StringComparer.Ordinal))
            {
                confidence = d.Confidence;
                return true;
            }
        }
        confidence = 0f;
        return false;
    }
}
