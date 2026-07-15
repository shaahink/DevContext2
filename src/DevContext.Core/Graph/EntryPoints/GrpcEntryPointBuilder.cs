namespace DevContext.Core.Graph;

/// <summary>Builds gRPC method-level entry points from <see cref="GrpcServiceDetection"/>s.
/// L3.6: expanded from service-level to per-method entries so gRPC services get the same
/// treatment as HTTP endpoints.</summary>
public sealed class GrpcEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        NameResolver names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        foreach (var svc in model.Detections.OfType<GrpcServiceDetection>())
        {
            if (!scope.Contains(svc.SourceFile) || !noise.IsProductionEntrySource(svc.SourceFile)) continue;

            var svcTypeFqn = names.Resolve(svc.ImplementationType, svc.SourceFile);
            var svcTypeNodeId = NodeId.ForType(svcTypeFqn);
            var svcNamespace = names.GetNamespace(svc.ImplementationType);

            if (svc.Methods.Length == 0)
            {
                var id = NodeId.ForEntry($"grpc:{svc.ImplementationType}");
                g.AddNode(new GraphNode(id, $"{svc.ServiceName}.{svc.ImplementationType}", NodeKind.EntryPoint) { FilePath = svc.SourceFile, LineNumber = svc.LineNumber });

                if (g.HasNode(svcTypeNodeId))
                    g.AddEdge(new GraphEdge(id, svcTypeNodeId, EdgeKind.Calls)
                    {
                        Provenance = $"{svc.SourceFile}:{svc.LineNumber}",
                        Resolution = Resolution.Join,
                    });

                entries.Add(new EntryPoint(EntryPointKind.GrpcService, $"{svc.ServiceName}.{svc.ImplementationType}", id)
                {
                    Provenance = $"{svc.SourceFile}:{svc.LineNumber}",
                    HandlerNode = svcTypeNodeId,
                    GroupPath = svcNamespace is not null ? $"{svc.ServiceName}" : null,
                    Project = scope.ProjectForFile(svc.SourceFile),
                });
                continue;
            }

            // Per-method entries
            foreach (var method in svc.Methods)
            {
                var methodTitle = $"{svc.ServiceName}.{method}";
                var methodKey = $"grpc:{svc.ServiceName}.{method}";
                var methodId = NodeId.ForEntry(methodKey);
                g.AddNode(new GraphNode(methodId, methodTitle, NodeKind.EntryPoint) { FilePath = svc.SourceFile, LineNumber = svc.LineNumber });

                // Anchor on the RPC method member, creating it up-front (like the HTTP builder) so the
                // seeded call graph's edges (T1.1) attach to it and entry→target resolves the service the
                // RPC calls — not the owning gRPC type. Entry builders run before the call graph, so the
                // member node won't pre-exist; create it when the owning type is known.
                NodeId memberNodeId;
                if (g.HasNode(svcTypeNodeId))
                {
                    memberNodeId = NodeId.ForMember(svcTypeFqn, method);
                    g.AddNode(new GraphNode(memberNodeId, $"{svc.ImplementationType}.{method}", NodeKind.Member)
                    { FilePath = svc.SourceFile, LineNumber = svc.LineNumber });
                }
                else
                {
                    memberNodeId = svcTypeNodeId;
                }
                g.AddEdge(new GraphEdge(methodId, memberNodeId, EdgeKind.Calls)
                {
                    Provenance = $"{svc.SourceFile}:{svc.LineNumber}",
                    Resolution = Resolution.Join,
                });

                entries.Add(new EntryPoint(EntryPointKind.GrpcService, methodTitle, methodId)
                {
                    Provenance = $"{svc.SourceFile}:{svc.LineNumber}",
                    HandlerNode = memberNodeId,
                    GroupPath = svc.ServiceName,
                    Project = scope.ProjectForFile(svc.SourceFile) ?? svcNamespace,
                });
            }
        }
        return entries.ToImmutable();
    }
}
