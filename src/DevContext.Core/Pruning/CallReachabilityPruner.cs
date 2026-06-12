namespace DevContext.Core.Pruning;

/// <summary>Prunes types that are not call-reachable from method-level focus points in the call graph.</summary>
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

        var focusMethods = context.Analysis.FocusPoints
            .Where(fp => fp.Kind == FocusKind.Method && fp.TypeName is not null)
            .Select(fp => fp.TypeName!)
            .Distinct()
            .ToArray();

        if (focusMethods.Length == 0)
        {
            model.AddDiagnostic(DiagnosticLevel.Info, Name, "No method-level focus points; skipping reachability analysis.");
            return default;
        }

        var maxDepth = context.ActiveScenario.Pruning.MaxCallDepth;
        var reachable = BfsReachableTypes(callGraph, focusMethods, maxDepth, ct);

        foreach (var type in model.Types.Values)
        {
            ct.ThrowIfCancellationRequested();

            if (reachable.TryGetValue(type.Name, out var depth))
            {
                var boost = Math.Max(0.0f, 10.0f - depth * 2.0f);
                type.RelevanceScore += boost;
                model.AddProvenance(type.Id, new InclusionReason(
                    $"Call-reachable at depth {depth} (+{boost:F1})", Name, boost));
            }
        }

        return default;
    }

    private static FrozenDictionary<string, int> BfsReachableTypes(
        CallGraph callGraph, string[] seedTypes, int maxDepth, CancellationToken ct)
    {
        var depths = new Dictionary<string, int>();
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

            if (!callGraph.Edges.TryGetValue(current, out var edges)) continue;

            foreach (var edge in edges)
            {
                if (depths.ContainsKey(edge.CalleeType)) continue;

                depths[edge.CalleeType] = depth + 1;
                queue.Enqueue((edge.CalleeType, depth + 1));
            }
        }

        return depths.ToFrozenDictionary();
    }
}
