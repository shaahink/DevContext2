using DevContext.Core.Graph.Seams;
using DevContext.Core.Graph2;
using DevContext.Core.Graph2.Seams;
using DevContext.Core.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Graph;

public sealed partial class GraphBuilder
{
    /// <summary>L4.1 — Compute spine-first flows for all entries. Each flow is the primary dispatch
    /// path (entry → send → handler → ...) with touches/emits collected only from spine members
    /// (fixes audit E5: no EntityRelation reachability).</summary>
    private static ImmutableArray<Flow> ComputeFlows(CodeGraph graph, ImmutableArray<EntryPoint> entries,
        int maxSpineDepth = 24)
    {
        if (entries.IsDefaultOrEmpty) return [];

        var bridgeMembers = BuildBridgeIndex(graph);
        var flows = ImmutableArray.CreateBuilder<Flow>(entries.Length);

        foreach (var entry in entries)
        {
            var entryNode = graph.Node(entry.Node);
            if (entryNode is null) continue;

            var visited = new HashSet<NodeId>();
            var steps = ImmutableArray.CreateBuilder<FlowStep>();
            var touchedIds = new HashSet<NodeId>();
            var emittedIds = new HashSet<NodeId>();
            var hops = ImmutableArray.CreateBuilder<ServiceHop>();

            var entryTitle = entryNode.Kind == NodeKind.EntryPoint ? entryNode.Title : entry.Title;
            steps.Add(new FlowStep(entry.Node, null, Resolution.Join, entry.Provenance) { Title = entryTitle });
            visited.Add(entry.Node);

            // Collect touches/emits from the entry node's own out-edges
            CollectSpineTouchesAndEmits(graph, entry.Node, bridgeMembers, touchedIds, emittedIds);

            var currentId = entry.Node;
            var isTruncated = true;
            for (var d = 0; d < maxSpineDepth; d++)
            {
                var bestEdge = SelectBestSpineEdge(graph, currentId, bridgeMembers, visited);
                if (bestEdge is null) { isTruncated = false; break; }

                visited.Add(bestEdge.To);

                // Record ServiceHop when crossing a ServiceLink
                if (bestEdge.Kind == EdgeKind.ServiceLink)
                {
                    var fromNode = graph.Node(bestEdge.From);
                    var toNode = graph.Node(bestEdge.To);
                    hops.Add(new ServiceHop(
                        fromNode?.Project,
                        toNode?.Project,
                        bestEdge.Tags.IsDefaultOrEmpty ? null : string.Join(",", bestEdge.Tags),
                        bestEdge.Provenance));
                }

                var targetNode = graph.Node(bestEdge.To);
                steps.Add(new FlowStep(bestEdge.To, bestEdge.Kind, bestEdge.Resolution, bestEdge.Provenance)
                {
                    Title = targetNode?.Title,
                });

                CollectSpineTouchesAndEmits(graph, bestEdge.To, bridgeMembers, touchedIds, emittedIds);
                currentId = bestEdge.To;
            }

            var flowId = entry.Node.Key;
            if (entry.Kind == EntryPointKind.HttpEndpoint && entry.Route is { } r && entry.HttpMethod is { } m)
                flowId = $"{m} {r}";

            flows.Add(new Flow(flowId, entry, steps.ToImmutable())
            {
                Touches = [.. touchedIds],
                Emits = [.. emittedIds],
                Hops = hops.ToImmutable(),
                IsTruncated = isTruncated,
            });
        }

        return flows.ToImmutable();
    }

    private static void CollectSpineTouchesAndEmits(CodeGraph graph, NodeId nodeId,
        Dictionary<NodeId, List<NodeId>> bridgeMembers,
        HashSet<NodeId> touchedIds, HashSet<NodeId> emittedIds)
    {
        var ids = new List<NodeId> { nodeId };
        if (nodeId.Kind == NodeKind.Type && bridgeMembers.TryGetValue(nodeId, out var members))
            ids.AddRange(members);

        foreach (var id in ids)
        {
            foreach (var edge in graph.OutEdges(id))
            {
                if (edge.Kind == EdgeKind.ReadsWrites)
                    touchedIds.Add(edge.To);
                else if (edge.Kind == EdgeKind.Raises)
                    emittedIds.Add(edge.To);
            }
        }
    }

    private static GraphEdge? SelectBestSpineEdge(CodeGraph graph, NodeId nodeId,
        Dictionary<NodeId, List<NodeId>> bridgeMembers, HashSet<NodeId> visited)
    {
        GraphEdge? best = null;
        var bestPriority = int.MaxValue;
        var bestConfidence = float.MinValue;

        var ids = new List<NodeId> { nodeId };
        if (nodeId.Kind == NodeKind.Type && bridgeMembers.TryGetValue(nodeId, out var members))
            ids.AddRange(members);

        foreach (var id in ids)
        {
            foreach (var edge in graph.OutEdges(id))
            {
                if (visited.Contains(edge.To)) continue;
                if (edge.Kind is EdgeKind.WrappedBy or EdgeKind.EntityRelation or EdgeKind.DependsOn or EdgeKind.Exposes)
                    continue;

                var p = SpineEdgePriority(edge.Kind);
                if (p < bestPriority || (p == bestPriority && edge.Confidence > bestConfidence))
                {
                    bestPriority = p;
                    bestConfidence = edge.Confidence;
                    best = edge;
                }
            }
        }

        // G5: Type->Service bridge — when at a Type node with known Project, consider
        // ServiceLink edges from the containing Service node so the spine can follow
        // cross-service hops. This closes the L2.4 gap where the checkout flow spine
        // stopped at event Type nodes because they had no edge to their Service node's
        // ServiceLinks. Safe by construction: if NodeId.ForService returns a node that
        // doesn't exist, the foreach is a no-op.
        if (nodeId.Kind == NodeKind.Type)
        {
            var typeNode = graph.Node(nodeId);
            if (typeNode?.Project is { Length: > 0 })
            {
                var serviceId = NodeId.ForService(typeNode.Project);
                foreach (var edge in graph.OutEdges(serviceId, EdgeKind.ServiceLink))
                {
                    if (visited.Contains(edge.To)) continue;
                    var p = SpineEdgePriority(edge.Kind);
                    if (p < bestPriority || (p == bestPriority && edge.Confidence > bestConfidence))
                    {
                        bestPriority = p;
                        bestConfidence = edge.Confidence;
                        best = edge;
                    }
                }
            }
        }

        if (best is not null && IsFrameworkLeaf(graph.Node(best.To)))
            return null;

        return best;
    }

    private static int SpineEdgePriority(EdgeKind kind) => kind switch
    {
        EdgeKind.Sends => 0,
        EdgeKind.Handles => 1,
        EdgeKind.ServiceLink => 2,
        EdgeKind.Raises => 3,
        EdgeKind.Consumes => 4,
        EdgeKind.ReadsWrites => 5,
        EdgeKind.Resolves => 6,
        _ => 7,
    };

    private static bool IsFrameworkLeaf(GraphNode? node)
    {
        if (node is null) return true;
        var title = node.Title;
        // Batch A: the "*Mediator*" Contains-overfit is gone — honest call-edge resolution no longer
        // produces edges onto framework mediator types (out-of-solution receivers are skipped at the
        // binder), so only the literal framework names below remain meaningful.
        return title.StartsWith("Microsoft.", StringComparison.Ordinal)
            || title.StartsWith("System.", StringComparison.Ordinal)
            || title == "DbContext"
            || title is "ILogger" or "IMediator" or "ISender" or "IPublisher";
    }

    private static Dictionary<NodeId, List<NodeId>> BuildBridgeIndex(CodeGraph graph)
    {
        var map = new Dictionary<NodeId, List<NodeId>>();
        foreach (var node in graph.Nodes)
        {
            if (node.Id.Kind != NodeKind.Member) continue;
            if (!node.Id.Key.Contains("::", StringComparison.Ordinal)) continue;
            var method = Graph2.SymbolCanon.MemberNameOf(node.Id.Key);
            var typeId = NodeId.ForType(Graph2.SymbolCanon.OwnerTypeOf(node.Id.Key));

            if (method is "Handle" or "HandleAsync" or "Consume" or "ConsumeAsync"
                || method.StartsWith("Execute", StringComparison.Ordinal)
                || method.StartsWith("Invoke", StringComparison.Ordinal))
            {
                if (!map.TryGetValue(typeId, out var list)) map[typeId] = list = [];
                list.Add(node.Id);
            }
        }
        return map;
    }

}
