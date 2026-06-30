namespace DevContext.Core.Graph;

/// <summary>Builds domain-event handler entry points from <see cref="MediatRHandlerDetection"/>
/// notifications.</summary>
public sealed class DomainEventHandlerEntryBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        NameResolver names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var h in model.Detections.OfType<MediatRHandlerDetection>())
        {
            if (h.Kind != MediatRKind.Notification) continue;
            if (!scope.Contains(h.SourceFile) || !noise.IsProductionEntrySource(h.SourceFile)) continue;
            if (!seen.Add(h.HandlerType)) continue;

            var id = NodeId.ForEntry($"domain:{h.HandlerType}");
            g.AddNode(new GraphNode(id, h.HandlerType, NodeKind.EntryPoint) { FilePath = h.SourceFile });
            entries.Add(new EntryPoint(EntryPointKind.DomainEventHandler, h.HandlerType, id)
            {
                Provenance = $"{h.SourceFile}:{h.LineNumber}",
                Target = h.RequestType,
            });
        }
        return entries.ToImmutable();
    }
}
