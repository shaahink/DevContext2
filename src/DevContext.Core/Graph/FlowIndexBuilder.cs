namespace DevContext.Core.Graph;

/// <summary>Per-entry flow statistics — everything derivable from one shallow trace. Server-side
/// port of the app's atlas indexer (T7.4, audit B11): the client used to background-fire up to
/// 100 GetTrace RPCs per boot/reattach to compute exactly this, then ~10 GetNode calls for hub
/// degrees. One difference is deliberate: boundary crossings count the REAL boundary seams
/// (Send/Handle/Consume/Raise/CrossService) — the client compared lowercased seam names against
/// 'consumes'/'raises'/'handler', which never match the wire's 'consume'/'raise'/'handle', so it
/// undercounted every boundary except sends (and under-ranked deep cross-service flows).</summary>
public sealed record FlowStatRow(
    string Focus,
    string Title,
    string Kind,
    bool Found)
{
    public int NodeCount { get; init; }
    public int MaxDepth { get; init; }
    public int BoundaryCrossings { get; init; }
    public int DataTouches { get; init; }
    /// <summary>% of nodes with Semantic (Roslyn-verified) resolution.</summary>
    public int VerifiedPct { get; init; }
    public ImmutableArray<string> TouchedEntities { get; init; } = [];
    public ImmutableArray<string> EmittedEvents { get; init; } = [];
    /// <summary>Every node on the flow — feeds the impact lens.</summary>
    public ImmutableArray<string> NodeIds { get; init; } = [];
    /// <summary>NodeIds minus data-seam nodes — feeds the hub radar (a DbContext and its EF members
    /// sit at the end of most flows on data-heavy repos; they're shared plumbing, not hubs).</summary>
    public ImmutableArray<string> HubIds { get; init; } = [];
    /// <summary>Importance ranking: breadth × (1 + boundary crossings).</summary>
    public double Score { get; init; }
}

/// <summary>In/out degree for one of the flow index's top hubs.</summary>
public sealed record HubDegree(string NodeId, int InDegree, int OutDegree);

public sealed record FlowIndexResult(
    ImmutableArray<FlowStatRow> Flows,
    ImmutableArray<HubDegree> HubDegrees);

/// <summary>Builds the whole flow atlas in one pass over the immutable snapshot: shallow-trace
/// every entry into a <see cref="FlowStatRow"/>, then compute degrees for the top hubs (nodes
/// appearing on more than one flow). Deterministic and side-effect free — memoize per session.</summary>
public static class FlowIndexBuilder
{
    /// <summary>Parity with the client indexer's MAX_FLOWS.</summary>
    private const int MaxFlows = 100;
    /// <summary>Parity with the client indexer's INDEX_DEPTH.</summary>
    private const int IndexDepth = 3;
    /// <summary>Parity with the client hub radar's top-10.</summary>
    private const int MaxHubs = 10;

    private static readonly FrozenSet<SeamKind> BoundarySeams =
        new[] { SeamKind.Send, SeamKind.Handle, SeamKind.Consume, SeamKind.Raise, SeamKind.CrossService }
            .ToFrozenSet();

    public static FlowIndexResult Build(GraphQuery query, ImmutableArray<EntryPoint> entries)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var flows = ImmutableArray.CreateBuilder<FlowStatRow>();
        var idMap = new Dictionary<string, NodeId>(StringComparer.Ordinal);

        foreach (var entry in entries)
        {
            // The client keys flows by "VERB route" (HTTP) or the title — mirror it exactly so the
            // stat rows join the entry rows the app already renders.
            var focus = entry is { HttpMethod: not null, Route: not null }
                ? $"{entry.HttpMethod} {entry.Route}"
                : entry.Title;
            if (string.IsNullOrEmpty(focus) || !seen.Add(focus)) continue;
            if (flows.Count >= MaxFlows) break;

            var trace = query.Trace(focus, IndexDepth);
            flows.Add(trace is null
                ? new FlowStatRow(focus, entry.Title, entry.Kind.ToString(), Found: false)
                : Compute(focus, entry, trace, idMap));
        }

        return new FlowIndexResult(flows.ToImmutable(), TopHubDegrees(query, flows, idMap));
    }

    private static FlowStatRow Compute(string focus, EntryPoint entry, Trace trace,
        Dictionary<string, NodeId> idMap)
    {
        var nodeCount = 0;
        var maxDepth = 0;
        var boundaryCrossings = 0;
        var dataTouches = 0;
        var verified = 0;
        var nodeIds = ImmutableArray.CreateBuilder<string>();
        var hubIds = ImmutableArray.CreateBuilder<string>();

        var stack = new Stack<TraceStep>();
        stack.Push(trace.Root);
        while (stack.Count > 0)
        {
            var step = stack.Pop();
            var id = step.Node.Id.ToString();
            idMap[id] = step.Node.Id;
            nodeCount++;
            nodeIds.Add(id);
            if (step.Depth > maxDepth) maxDepth = step.Depth;
            if (BoundarySeams.Contains(step.Seam)) boundaryCrossings++;
            if (step.Seam == SeamKind.Data) dataTouches++;
            else hubIds.Add(id);
            if (step.Resolution == Resolution.Semantic) verified++;
            foreach (var child in step.Children) stack.Push(child);
        }

        return new FlowStatRow(focus, entry.Title, entry.Kind.ToString(), Found: true)
        {
            NodeCount = nodeCount,
            MaxDepth = maxDepth,
            BoundaryCrossings = boundaryCrossings,
            DataTouches = dataTouches,
            VerifiedPct = nodeCount > 0 ? (int)Math.Round(verified * 100.0 / nodeCount) : 0,
            TouchedEntities = trace.TouchedEntities,
            EmittedEvents = trace.EmittedEvents,
            NodeIds = nodeIds.ToImmutable(),
            HubIds = hubIds.ToImmutable(),
            Score = nodeCount * (1.0 + boundaryCrossings),
        };
    }

    /// <summary>The same top hubs the client radar computes (distinct per flow, on &gt;1 flow, top 10
    /// by flow count), with real graph degrees — so the app needs no per-hub GetNode calls.</summary>
    private static ImmutableArray<HubDegree> TopHubDegrees(
        GraphQuery query, ImmutableArray<FlowStatRow>.Builder flows,
        Dictionary<string, NodeId> idMap)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var flow in flows)
            foreach (var id in flow.HubIds.Distinct(StringComparer.Ordinal))
                counts[id] = counts.GetValueOrDefault(id) + 1;

        return counts
            .Where(kv => kv.Value > 1)
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key, StringComparer.Ordinal)
            .Take(MaxHubs)
            .Select(kv => new HubDegree(kv.Key,
                query.Graph.InEdges(idMap[kv.Key]).Length,
                query.Graph.OutEdges(idMap[kv.Key]).Length))
            .ToImmutableArray();
    }
}
