using DevContext.Core.Graph2;

namespace DevContext.Core.Graph;

/// <summary>Builds GraphQL resolver entry points from <see cref="GraphQlFieldDetection"/>s.</summary>
public sealed class GraphQlEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        SymbolTable names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        foreach (var field in model.Detections.OfType<GraphQlFieldDetection>())
        {
            if (!scope.Contains(field.SourceFile) || !noise.IsProductionEntrySource(field.SourceFile)) continue;

            var title = $"{field.OperationType}/{field.TypeName}.{field.FieldName}";
            var id = NodeId.ForEntry($"graphql:{field.TypeName}.{field.FieldName}");
            g.AddNode(new GraphNode(id, title, NodeKind.EntryPoint) { FilePath = field.SourceFile, LineNumber = field.LineNumber });

            // Anchor on the resolver METHOD member, creating it up-front (like the HTTP builder), not the
            // owning type: the seeded call graph (T1.1) puts the resolver's Calls edges on the member, and
            // entry→target resolution drills a Member landing but only reads Sends on a Type landing — so a
            // plain-service resolver would show no target if anchored on the type.
            var typeFqn = names.ResolveName(field.TypeName, field.SourceFile);
            var typeId = NodeId.ForType(typeFqn);
            NodeId handlerNode;
            if (g.HasNode(typeId))
            {
                handlerNode = NodeId.ForMember(typeFqn, field.FieldName);
                g.AddNode(new GraphNode(handlerNode, $"{field.TypeName}.{field.FieldName}", NodeKind.Member)
                { FilePath = field.SourceFile, LineNumber = field.LineNumber });
            }
            else
            {
                handlerNode = typeId;
            }
            if (g.HasNode(handlerNode))
                g.AddEdge(new GraphEdge(id, handlerNode, EdgeKind.Calls)
                {
                    Provenance = $"{field.SourceFile}:{field.LineNumber}",
                    Resolution = Resolution.Join,
                });

            entries.Add(new EntryPoint(EntryPointKind.GraphQlField, title, id)
            {
                Provenance = $"{field.SourceFile}:{field.LineNumber}",
                HandlerNode = handlerNode,
                Project = scope.ProjectForFile(field.SourceFile),
            });
        }
        return entries.ToImmutable();
    }
}
