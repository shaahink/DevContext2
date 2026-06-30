namespace DevContext.Core.Graph;

/// <summary>Builds HTTP entry points from <see cref="EndpointDetection"/>s (minimal APIs,
/// controllers, FastEndpoints). Links entry → handler with Calls edges and resolves
/// dispatch targets via body scan.</summary>
public sealed class HttpEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        NameResolver names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        var dedup = new HashSet<(string Verb, string Route, string File, int Line)>();
        foreach (var ep in model.Detections.OfType<EndpointDetection>())
        {
            if (!scope.Contains(ep.SourceFile) || !noise.IsProductionEntrySource(ep.SourceFile)) continue;

            if (GraphBuilder.IsInfrastructureEntry(ep)) continue;

            if (!dedup.Add((ep.HttpMethod, GraphBuilder.NormalizeRoute(ep.RouteTemplate), ep.SourceFile, ep.LineNumber)))
                continue;

            var key = $"{ep.HttpMethod} {ep.RouteTemplate}";
            var id = NodeId.ForEntry(key);
            g.AddNode(new GraphNode(id, key, NodeKind.EntryPoint) { FilePath = ep.SourceFile });

            var isLambdaHandler = ep.HandlerMethod is "<lambda>" or "<anonymous>"
                || string.IsNullOrEmpty(ep.HandlerType)
                || ep.HandlerType is "λ" or "?"
                || ep.HandlerType.Contains("=>", StringComparison.Ordinal);

            var linked = false;
            NodeId? handlerNodeId = null;

            if (!isLambdaHandler)
            {
                var handlerFqn = names.Resolve(ep.HandlerType);
                var methodName = ep.HandlerMethod;
                var hasSpecificMethod = !string.IsNullOrEmpty(methodName)
                    && methodName is not "<lambda>" and not "<anonymous>"
                    && !methodName.Contains("=>", StringComparison.Ordinal);

                if (hasSpecificMethod && g.HasNode(NodeId.ForType(handlerFqn)))
                {
                    var memberNodeId = NodeId.ForMember(handlerFqn, methodName);
                    handlerNodeId = memberNodeId;
                    g.AddNode(new GraphNode(memberNodeId, ep.HandlerType + "." + methodName, NodeKind.Member)
                    {
                        FilePath = ep.SourceFile,
                        SourceBody = ep.HandlerBody,
                    });
                    g.AddEdge(new GraphEdge(id, memberNodeId, EdgeKind.Calls)
                    {
                        Provenance = $"{ep.SourceFile}:{ep.LineNumber}",
                        Resolution = Resolution.Join,
                    });
                    if (!string.IsNullOrEmpty(ep.HandlerBody))
                        GraphBuilder.AddDispatchEdgesFromBody(g, memberNodeId, ep.HandlerBody,
                            $"{ep.SourceFile}:{(ep.HandlerLine > 0 ? ep.HandlerLine : ep.LineNumber)}", names);
                    linked = true;
                }
                else
                {
                    var typeNodeId = NodeId.ForType(handlerFqn);
                    if (g.HasNode(typeNodeId))
                    {
                        handlerNodeId = typeNodeId;
                        g.AddEdge(new GraphEdge(id, typeNodeId, EdgeKind.Calls)
                        {
                            Provenance = $"{ep.SourceFile}:{ep.LineNumber}",
                            Resolution = Resolution.Join,
                        });
                        linked = true;
                    }
                }
            }

            if (!linked)
            {
                var ownerType = model.Types.Values.FirstOrDefault(t =>
                    string.Equals(t.FilePath, ep.SourceFile, StringComparison.OrdinalIgnoreCase));

                if (isLambdaHandler && !string.IsNullOrEmpty(ep.HandlerBody))
                {
                    var ownerKey = ownerType?.Id ?? Path.GetFileNameWithoutExtension(ep.SourceFile);
                    var lambdaId = NodeId.ForMember(ownerKey, $"<lambda> {key}");
                    handlerNodeId = lambdaId;
                    g.AddNode(new GraphNode(lambdaId, $"<lambda> {key}", NodeKind.Member)
                    {
                        FilePath = ep.SourceFile,
                        SourceBody = ep.HandlerBody,
                    });
                    g.AddEdge(new GraphEdge(id, lambdaId, EdgeKind.Calls)
                    {
                        Provenance = $"{ep.SourceFile}:{(ep.HandlerLine > 0 ? ep.HandlerLine : ep.LineNumber)}",
                        Resolution = Resolution.Join,
                    });
                    GraphBuilder.AddDispatchEdgesFromBody(g, lambdaId, ep.HandlerBody,
                        $"{ep.SourceFile}:{(ep.HandlerLine > 0 ? ep.HandlerLine : ep.LineNumber)}", names);
                    linked = true;
                }
                else if (ownerType is not null)
                {
                    var ownerNode = g.Nodes.FirstOrDefault(n =>
                        n.Kind == NodeKind.Type
                        && string.Equals(n.FilePath, ownerType.FilePath, StringComparison.OrdinalIgnoreCase));
                    if (ownerNode is not null)
                    {
                        handlerNodeId = ownerNode.Id;
                        g.AddEdge(new GraphEdge(id, ownerNode.Id, EdgeKind.Calls)
                        {
                            Provenance = $"{ep.SourceFile}:{ep.LineNumber}",
                            Resolution = Resolution.Join,
                        });
                    }
                }
            }

            entries.Add(new EntryPoint(EntryPointKind.HttpEndpoint, key, id)
            {
                HttpMethod = ep.HttpMethod,
                Route = ep.RouteTemplate,
                Provenance = $"{ep.SourceFile}:{ep.LineNumber}",
                HandlerNode = handlerNodeId,
            });
        }
        return entries.ToImmutable();
    }
}
