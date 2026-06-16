namespace DevContext.Core.Graph;

/// <summary>How control transferred into a trace step — the human-facing label for an <see cref="EdgeKind"/>.</summary>
public enum SeamKind { Entry, Call, Send, Handle, Raise, Consume, Data, Resolve, Pipeline }

/// <summary>One node in an entry-rooted trace tree.</summary>
public sealed record TraceStep(
    GraphNode Node,
    SeamKind Seam,
    int Depth)
{
    /// <summary>"file:line" of the site that led here.</summary>
    public string? Provenance { get; init; }
    /// <summary>How the edge into this step was resolved.</summary>
    public Resolution Resolution { get; init; } = Resolution.Join;
    /// <summary>Child steps, deeper down the wiring.</summary>
    public ImmutableArray<TraceStep> Children { get; init; } = [];
    /// <summary>True when traversal stopped here (depth/fan-out/revisit) with more beyond.</summary>
    public bool Truncated { get; init; }
}

/// <summary>An entry-rooted trace: the call stack down the wiring, with indirection bridged.</summary>
public sealed record Trace(EntryPoint Entry, TraceStep Root)
{
    /// <summary>Entities the trace writes/reads (filled by a later summary pass).</summary>
    public ImmutableArray<string> TouchedEntities { get; init; } = [];
    /// <summary>Events the trace emits (filled by a later summary pass).</summary>
    public ImmutableArray<string> EmittedEvents { get; init; } = [];
}

/// <summary>The render-time dials for a trace.</summary>
public sealed record TraceOptions
{
    /// <summary>Maximum hops from the entry.</summary>
    public int MaxDepth { get; init; } = 6;
    /// <summary>Maximum children expanded per node.</summary>
    public int MaxFanOut { get; init; } = 12;
    /// <summary>Edge kinds to follow. Default follows every "down the wiring" seam.</summary>
    public ImmutableArray<EdgeKind> Follow { get; init; } =
    [
        EdgeKind.Sends, EdgeKind.Handles, EdgeKind.Calls, EdgeKind.Raises,
        EdgeKind.Consumes, EdgeKind.ReadsWrites, EdgeKind.Resolves,
    ];
}

/// <summary>
/// Builds a <see cref="Trace"/> by walking the <see cref="CodeGraph"/> forward from an entry point.
/// This traversal IS the selection mechanism that replaces global relevance scoring: what a trace
/// contains is what's reachable, depth/fan-out bounded — no FinalScore, no MaxSurvivingTypes.
/// The spine is functional; evolve (a) structural fan-out ranking and (b) the framework-boundary stop.
/// </summary>
public sealed class TraceBuilder
{
    private readonly CodeGraph _graph;

    /// <summary>Creates a trace builder over a graph.</summary>
    public TraceBuilder(CodeGraph graph) => _graph = graph;

    /// <summary>Builds the entry-rooted trace tree.</summary>
    public Trace Build(EntryPoint entry, TraceOptions? options = null)
    {
        var opts = options ?? new TraceOptions();
        var follow = opts.Follow.ToHashSet();
        var visited = new HashSet<NodeId>();
        var rootNode = _graph.Node(entry.Node)
            ?? new GraphNode(entry.Node, entry.Title, NodeKind.EntryPoint);
        var root = Walk(rootNode, SeamKind.Entry, entry.Provenance, Resolution.Join, 0, opts, follow, visited);
        return new Trace(entry, root);
    }

    private TraceStep Walk(GraphNode node, SeamKind seam, string? provenance, Resolution resolution,
        int depth, TraceOptions opts, HashSet<EdgeKind> follow, HashSet<NodeId> visited)
    {
        // Revisit guard + depth cap: render the node, but don't expand it again.
        if (!visited.Add(node.Id) || depth >= opts.MaxDepth)
            return new TraceStep(node, seam, depth)
            {
                Provenance = provenance,
                Resolution = resolution,
                Truncated = HasFollowable(node.Id, follow),
            };

        var edges = _graph.OutEdges(node.Id).Where(e => follow.Contains(e.Kind)).ToList();

        // TODO(agent, P2): rank fan-out structurally before Take — prefer edges that lead toward a
        // sink (Data write / Raise / another EntryPoint) over framework-leaf Calls.
        var taken = edges.Take(opts.MaxFanOut).ToList();

        var children = ImmutableArray.CreateBuilder<TraceStep>(taken.Count);
        foreach (var edge in taken)
        {
            var child = _graph.Node(edge.To);
            if (child is null) continue;
            children.Add(Walk(child, ToSeam(edge.Kind), edge.Provenance, edge.Resolution,
                depth + 1, opts, follow, visited));
        }

        return new TraceStep(node, seam, depth)
        {
            Provenance = provenance,
            Resolution = resolution,
            Children = children.ToImmutable(),
            Truncated = edges.Count > taken.Count,
        };
    }

    private bool HasFollowable(NodeId id, HashSet<EdgeKind> follow)
        => _graph.OutEdges(id).Any(e => follow.Contains(e.Kind));

    private static SeamKind ToSeam(EdgeKind kind) => kind switch
    {
        EdgeKind.Sends => SeamKind.Send,
        EdgeKind.Handles => SeamKind.Handle,
        EdgeKind.Raises => SeamKind.Raise,
        EdgeKind.Consumes => SeamKind.Consume,
        EdgeKind.ReadsWrites => SeamKind.Data,
        EdgeKind.Resolves => SeamKind.Resolve,
        EdgeKind.WrappedBy => SeamKind.Pipeline,
        _ => SeamKind.Call,
    };
}
