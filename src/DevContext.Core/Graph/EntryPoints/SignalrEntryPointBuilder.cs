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
            g.AddNode(new GraphNode(id, hub.HubType, NodeKind.EntryPoint) { FilePath = hub.SourceFile, LineNumber = hub.LineNumber });

            var typeFqn = names.Resolve(hub.HubType, hub.SourceFile);
            var typeId = NodeId.ForType(typeFqn);

            // Anchor the entry on the hub METHOD members — the members carry whatever
            // member-origin edges the call graph bound, and even edge-less they show the hub's
            // client-callable surface. A bare Type link gives a depth-1 trace (the controlled
            // bridge only expands handler/ctor members). Mirrors HttpEntryPointBuilder, which
            // creates the handler member node itself.
            var linkedMember = false;
            if (g.HasNode(typeId))
            {
                foreach (var method in hub.HubMethods)
                {
                    var memberId = NodeId.ForMember(typeFqn, method);
                    g.AddNode(new GraphNode(memberId, $"{hub.HubType}.{method}", NodeKind.Member)
                    {
                        FilePath = hub.SourceFile,
                    });
                    g.AddEdge(new GraphEdge(id, memberId, EdgeKind.Calls)
                    {
                        Provenance = $"{hub.SourceFile}:{hub.LineNumber}",
                        Resolution = Resolution.Join,
                    });
                    linkedMember = true;
                }
            }

            if (!linkedMember && g.HasNode(typeId))
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
                Project = scope.ProjectForFile(hub.SourceFile),
            });
        }
        return entries.ToImmutable();
    }
}
