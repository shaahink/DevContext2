namespace DevContext.Core.Graph;

/// <summary>Builds gRPC service entry points from <see cref="GrpcServiceDetection"/>s.</summary>
public sealed class GrpcEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        NameResolver names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var svc in model.Detections.OfType<GrpcServiceDetection>())
        {
            if (!scope.Contains(svc.SourceFile) || !noise.IsProductionEntrySource(svc.SourceFile)) continue;
            if (!seen.Add(svc.ImplementationType)) continue;

            var title = $"{svc.ServiceName}.{svc.ImplementationType}";
            var id = NodeId.ForEntry($"grpc:{svc.ImplementationType}");
            g.AddNode(new GraphNode(id, title, NodeKind.EntryPoint) { FilePath = svc.SourceFile });

            var typeId = NodeId.ForType(names.Resolve(svc.ImplementationType));
            if (g.HasNode(typeId))
                g.AddEdge(new GraphEdge(id, typeId, EdgeKind.Calls)
                {
                    Provenance = $"{svc.SourceFile}:{svc.LineNumber}",
                    Resolution = Resolution.Join,
                });

            var methods = svc.Methods.Length > 0
                ? $" ({svc.Methods.Length} methods: {string.Join(", ", svc.Methods.Take(3))})" : "";
            entries.Add(new EntryPoint(EntryPointKind.GrpcService, title + methods, id)
            {
                Provenance = $"{svc.SourceFile}:{svc.LineNumber}",
                HandlerNode = typeId,
            });
        }
        return entries.ToImmutable();
    }
}
