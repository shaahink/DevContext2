namespace DevContext.Core.Graph;

/// <summary>Builds SignalR hub entry points from <see cref="SignalRHubDetection"/>s.</summary>
public sealed class SignalrEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        NameResolver names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var hub in model.Detections.OfType<SignalRHubDetection>())
        {
            if (!scope.Contains(hub.SourceFile) || !noise.IsProductionEntrySource(hub.SourceFile)) continue;
            if (!seen.Add(hub.HubType)) continue;

            var id = NodeId.ForEntry($"signalr:{hub.HubType}");
            g.AddNode(new GraphNode(id, hub.HubType, NodeKind.EntryPoint) { FilePath = hub.SourceFile });

            var typeId = NodeId.ForType(names.Resolve(hub.HubType));
            if (g.HasNode(typeId))
                g.AddEdge(new GraphEdge(id, typeId, EdgeKind.Calls)
                {
                    Provenance = $"{hub.SourceFile}:{hub.LineNumber}",
                    Resolution = Resolution.Join,
                });

            var methods = hub.HubMethods.Length > 0
                ? $" ({hub.HubMethods.Length} methods: {string.Join(", ", hub.HubMethods.Take(3))})" : "";
            entries.Add(new EntryPoint(EntryPointKind.SignalRHub, hub.HubType + methods, id)
            {
                Provenance = $"{hub.SourceFile}:{hub.LineNumber}",
                HandlerNode = typeId,
            });
        }
        return entries.ToImmutable();
    }
}
