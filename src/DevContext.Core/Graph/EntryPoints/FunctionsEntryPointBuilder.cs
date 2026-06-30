namespace DevContext.Core.Graph;

/// <summary>Builds Azure Functions entry points from <see cref="FunctionEntryDetection"/>s.</summary>
public sealed class FunctionsEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        NameResolver names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var fn in model.Detections.OfType<FunctionEntryDetection>())
        {
            if (!scope.Contains(fn.SourceFile) || !noise.IsProductionEntrySource(fn.SourceFile)) continue;
            var key = $"{fn.ClassName}.{fn.MethodName}";
            if (!seen.Add(key)) continue;

            var triggers = string.Join(", ", fn.Triggers);
            var title = $"{key} [{triggers}]";
            var id = NodeId.ForEntry($"func:{key}");
            g.AddNode(new GraphNode(id, title, NodeKind.EntryPoint) { FilePath = fn.SourceFile });

            var typeId = NodeId.ForType(names.Resolve(fn.ClassName));
            if (g.HasNode(typeId))
                g.AddEdge(new GraphEdge(id, typeId, EdgeKind.Calls)
                {
                    Provenance = $"{fn.SourceFile}:{fn.LineNumber}",
                    Resolution = Resolution.Join,
                });

            entries.Add(new EntryPoint(EntryPointKind.FunctionEntry, title, id)
            {
                Provenance = $"{fn.SourceFile}:{fn.LineNumber}",
                HandlerNode = typeId,
            });
        }
        return entries.ToImmutable();
    }
}
