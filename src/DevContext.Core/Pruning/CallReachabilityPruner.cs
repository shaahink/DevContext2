namespace DevContext.Core.Pruning;

/// <summary>Computes graph-proximity scores via BFS over the type-collapsed call graph. Scores ∈ [0,1] with decay 1/(1+depth).</summary>
public sealed class CallReachabilityPruner : IPruner
{
    /// <summary>Gets the name of this pruner.</summary>
    public string Name => "CallReachabilityPruner";
    /// <summary>Gets the execution order.</summary>
    public int Order => 20;

    public ValueTask PruneAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var callGraph = context.Analysis.CallGraph;
        if (callGraph is null)
        {
            model.AddDiagnostic(DiagnosticLevel.Info, Name, "CallGraph not available; skipping reachability analysis.");
            return default;
        }

        // Seed BFS from focus type names (both direct type focus and method focus)
        var seedTypes = context.Analysis.FocusPoints
            .Where(fp => fp.TypeName is not null)
            .Select(fp => fp.TypeName!)
            .Distinct()
            .ToHashSet();

        if (seedTypes.Count == 0)
        {
            model.AddDiagnostic(DiagnosticLevel.Info, Name, "No typed focus points; skipping reachability analysis.");
            return default;
        }

        // Build type-collapsed call graph from the existing CallGraph (edges keyed by "Type.Method")
        var typeGraph = BuildTypeCollapsedGraph(callGraph);

        var maxDepth = context.ActiveScenario.Pruning.MaxCallDepth;

        // BFS from seed types
        var depths = BfsTypeDepths(typeGraph, seedTypes, maxDepth, ct);

        // Apply graph proximity: store on each type for later combination in RunScoringAsync
        foreach (var type in model.Types.Values)
        {
            ct.ThrowIfCancellationRequested();

            double graphProximity = 0.0;

            if (depths.TryGetValue(type.Id, out var depth))
            {
                graphProximity = 1.0 / (1.0 + depth);
            }
            else if (depths.TryGetValue(type.Name, out var nameDepth))
            {
                graphProximity = 1.0 / (1.0 + nameDepth);
            }
            else
            {
                foreach (var kv in depths)
                {
                    if (type.Id.EndsWith("." + kv.Key, StringComparison.Ordinal)
                        || string.Equals(type.Name, kv.Key, StringComparison.Ordinal))
                    {
                        graphProximity = 1.0 / (1.0 + kv.Value);
                        break;
                    }
                }
            }

            type.GraphProximity = graphProximity;

            if (graphProximity > 0)
            {
                model.AddProvenance(type.Id, new InclusionReason(
                    $"Call-reachable at depth via type-collapsed graph (+{graphProximity:F2})", Name, (float)graphProximity));
            }
        }

        return default;
    }

    private static Dictionary<string, HashSet<string>> BuildTypeCollapsedGraph(CallGraph callGraph)
    {
        var graph = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        foreach (var (callerKey, edges) in callGraph.Edges)
        {
            var callerType = ExtractTypeName(callerKey);
            if (callerType is null) continue;

            foreach (var edge in edges)
            {
                var calleeType = edge.CalleeType;
                if (calleeType is null) continue;

                if (!graph.TryGetValue(callerType, out var callees))
                {
                    graph[callerType] = callees = new HashSet<string>(StringComparer.Ordinal);
                }

                callees.Add(calleeType);
            }
        }

        return graph;
    }

    private static string? ExtractTypeName(string? key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        var dotIdx = key.LastIndexOf('.');
        return dotIdx > 0 ? key[..dotIdx] : key;
    }

    private static Dictionary<string, int> BfsTypeDepths(
        Dictionary<string, HashSet<string>> typeGraph,
        HashSet<string> seedTypes,
        int maxDepth,
        CancellationToken ct)
    {
        var depths = new Dictionary<string, int>(StringComparer.Ordinal);
        var queue = new Queue<(string Type, int Depth)>();

        foreach (var seed in seedTypes)
        {
            if (!depths.ContainsKey(seed))
            {
                depths[seed] = 0;
                queue.Enqueue((seed, 0));
            }
        }

        while (queue.Count > 0)
        {
            ct.ThrowIfCancellationRequested();

            var (current, depth) = queue.Dequeue();
            if (depth >= maxDepth) continue;

            if (!typeGraph.TryGetValue(current, out var callees)) continue;

            foreach (var callee in callees)
            {
                if (depths.ContainsKey(callee)) continue;

                depths[callee] = depth + 1;
                queue.Enqueue((callee, depth + 1));
            }
        }

        return depths;
    }
}
