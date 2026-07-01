namespace DevContext.Core.Graph;

/// <summary>Builds Orleans grain entry points from <see cref="GrainDetection"/>s.</summary>
public sealed class OrleansGrainEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        NameResolver names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var grain in model.Detections.OfType<GrainDetection>())
        {
            if (!scope.Contains(grain.SourceFile) || !noise.IsProductionEntrySource(grain.SourceFile)) continue;
            if (!seen.Add(grain.GrainType)) continue;

            var methodStr = grain.Methods.Length > 0
                ? $" ({grain.Methods.Length} methods: {string.Join(", ", grain.Methods.Take(3))})" : "";
            var title = $"{grain.GrainType} : {grain.InterfaceType}{methodStr}";
            var id = NodeId.ForEntry($"grain:{grain.GrainType}");
            g.AddNode(new GraphNode(id, title, NodeKind.EntryPoint) { FilePath = grain.SourceFile });

            var typeId = NodeId.ForType(names.Resolve(grain.GrainType));
            if (g.HasNode(typeId))
                g.AddEdge(new GraphEdge(id, typeId, EdgeKind.Calls)
                {
                    Provenance = $"{grain.SourceFile}:{grain.LineNumber}",
                    Resolution = Resolution.Join,
                });

            entries.Add(new EntryPoint(EntryPointKind.GrainMethod, title, id)
            {
                Provenance = $"{grain.SourceFile}:{grain.LineNumber}",
                HandlerNode = typeId,
            });
        }
        return entries.ToImmutable();
    }
}
