using DevContext.Core.Graph2;

namespace DevContext.Core.Graph;

/// <summary>Builds Azure Functions entry points from <see cref="FunctionEntryDetection"/>s.</summary>
public sealed class FunctionsEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        SymbolTable names, NoiseFilter noise)
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
            g.AddNode(new GraphNode(id, title, NodeKind.EntryPoint) { FilePath = fn.SourceFile, LineNumber = fn.LineNumber });

            // Anchor on the trigger METHOD member, creating it up-front (like the HTTP builder), not the
            // owning type: the seeded call graph (T1.1) puts the trigger's Calls edges on the member, and
            // entry→target resolution drills a Member landing (ResolvePrimaryCall) but only reads Sends on
            // a Type landing — so a plain-service function would show no target if anchored on the type.
            var typeFqn = names.ResolveName(fn.ClassName, fn.SourceFile);
            var typeId = NodeId.ForType(typeFqn);
            NodeId handlerNode;
            if (g.HasNode(typeId))
            {
                handlerNode = NodeId.ForMember(typeFqn, fn.MethodName);
                g.AddNode(new GraphNode(handlerNode, $"{fn.ClassName}.{fn.MethodName}", NodeKind.Member)
                { FilePath = fn.SourceFile, LineNumber = fn.LineNumber });
            }
            else
            {
                handlerNode = typeId;
            }
            if (g.HasNode(handlerNode))
                g.AddEdge(new GraphEdge(id, handlerNode, EdgeKind.Calls)
                {
                    Provenance = $"{fn.SourceFile}:{fn.LineNumber}",
                    Resolution = Resolution.Join,
                });

            entries.Add(new EntryPoint(EntryPointKind.FunctionEntry, title, id)
            {
                Provenance = $"{fn.SourceFile}:{fn.LineNumber}",
                HandlerNode = handlerNode,
                Project = scope.ProjectForFile(fn.SourceFile),
            });
        }
        return entries.ToImmutable();
    }
}
