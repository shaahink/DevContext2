namespace DevContext.Core.Graph;

/// <summary>Builds GraphQL resolver entry points from <see cref="GraphQlFieldDetection"/>s.</summary>
public sealed class GraphQlEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        NameResolver names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        foreach (var field in model.Detections.OfType<GraphQlFieldDetection>())
        {
            if (!scope.Contains(field.SourceFile) || !noise.IsProductionEntrySource(field.SourceFile)) continue;

            var title = $"{field.OperationType}/{field.TypeName}.{field.FieldName}";
            var id = NodeId.ForEntry($"graphql:{field.TypeName}.{field.FieldName}");
            g.AddNode(new GraphNode(id, title, NodeKind.EntryPoint) { FilePath = field.SourceFile });

            var typeId = NodeId.ForType(names.Resolve(field.TypeName));
            if (g.HasNode(typeId))
                g.AddEdge(new GraphEdge(id, typeId, EdgeKind.Calls)
                {
                    Provenance = $"{field.SourceFile}:{field.LineNumber}",
                    Resolution = Resolution.Join,
                });

            entries.Add(new EntryPoint(EntryPointKind.GraphQlField, title, id)
            {
                Provenance = $"{field.SourceFile}:{field.LineNumber}",
                HandlerNode = typeId,
            });
        }
        return entries.ToImmutable();
    }
}
