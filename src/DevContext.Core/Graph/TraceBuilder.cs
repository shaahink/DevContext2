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
    /// <summary>Salient source lines around the provenance site (1-3 lines, for --detail salient).</summary>
    public ImmutableArray<string> Salient { get; init; } = [];
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
        var touched = new List<string>();
        var emitted = new List<string>();
        CollectSummaries(root, touched, emitted);
        // Also collect entities from ALL out-edges of visited nodes (not just the followed
        // ones) — ReadWrites edges at priority 4 may be cut by fan-out, but the entities
        // they connect to should still appear in TOUCHES.
        CollectGraphEntities(visited, touched);
        return new Trace(entry, root)
        {
            TouchedEntities = [.. touched.Distinct()],
            EmittedEvents = [.. emitted.Distinct()],
        };
    }

    private static void CollectSummaries(TraceStep step, List<string> touched, List<string> emitted)
    {
        // Entity nodes are collected by CollectGraphEntities (below) which scans
        // all edges including cut ones — not limited to the rendered tree.
        if (step.Node.Kind == NodeKind.Event)
            emitted.Add(step.Node.Title);
        foreach (var child in step.Children)
            CollectSummaries(child, touched, emitted);
    }

    /// <summary>For every visited node, check all out-edges (including twin-node edges) for Entity
    /// targets that were cut by fan-out. Also collects entities from incoming edges (Entity→DataStore)
    /// when the DataStore node was visited. This ensures TOUCHES lists all entities reachable from the
    /// trace, not just the ones in the rendered tree.</summary>
    private void CollectGraphEntities(HashSet<NodeId> visited, List<string> touched)
    {
        foreach (var nodeId in visited)
        {
            foreach (var edge in OutEdgesWithTwin(nodeId))
            {
                if (edge.Kind == EdgeKind.ReadsWrites)
                {
                    var targetNode = _graph.Node(edge.To);
                    if (targetNode is not null && targetNode.Kind == NodeKind.Entity
                        && !IsNoiseEntityName(targetNode.Title))
                        touched.Add(targetNode.Title);
                }
            }
        }
        // Also collect entities that connect TO a visited DataStore node
        foreach (var (nodeId, node) in _graph.Nodes.Select(n => (n.Id, n)))
        {
            if (node.Kind != NodeKind.Entity) continue;
            if (IsNoiseEntityName(node.Title)) continue;
            foreach (var edge in _graph.OutEdges(nodeId, EdgeKind.ReadsWrites))
            {
                if (visited.Contains(edge.To))
                {
                    touched.Add(node.Title);
                    break;
                }
            }
        }
    }

    private static bool IsNoiseEntityName(string name)
        => name.StartsWith('<')           // <OnModelCreating> etc.
        || name.Contains("Migration")     // Migration artifacts
        || name.Contains("Initial");      // Initial migration

    private TraceStep Walk(GraphNode node, SeamKind seam, string? provenance, Resolution resolution,
        int depth, TraceOptions opts, HashSet<EdgeKind> follow, HashSet<NodeId> visited)
    {
        if (!visited.Add(node.Id) || depth >= opts.MaxDepth)
            return new TraceStep(node, seam, depth)
            {
                Provenance = provenance,
                Resolution = resolution,
                Truncated = HasFollowable(node.Id, follow),
            };

        // Dedup by (target, kind): a Handler/Service node and its Type twin can carry the SAME edge
        // (e.g. a Raises edge mirrored onto both), which would otherwise render the child twice.
        var edges = OutEdgesWithTwin(node.Id)
            .Where(e => follow.Contains(e.Kind))
            .GroupBy(e => (e.To, e.Kind))
            .Select(grp => grp.OrderByDescending(e => e.Confidence).First())
            .ToList();

        // Structural ranking: prefer edges that lead to sinks (Data/Raise/Consumes) over framework
        // leaf Calls. Sends/Handles/Resolves are medium priority. Calls are lowest.
        var ranked = edges
            .OrderBy(e => EdgePriority(e.Kind))
            .ThenBy(e => e.Confidence != 1.0f ? 1 : 0)   // uncertain edges last
            .ToList();

        var taken = ranked.Take(opts.MaxFanOut).ToList();

        var children = ImmutableArray.CreateBuilder<TraceStep>(taken.Count);
        foreach (var edge in taken)
        {
            var child = _graph.Node(edge.To);
            if (child is null) continue;

            // Use source body from the FROM node, preferring Type twin body over Handler/Service body
            var fromNode = _graph.Node(edge.From) ?? node;
            var salientSource = fromNode.SourceBody ?? (
                fromNode.Kind != node.Kind && _graph.Node(NodeId.ForType(ExtractTypeKey(fromNode.Id.Key))) is { } twin
                    ? twin.SourceBody
                    : null);
            var salient = ExtractSalient(salientSource, edge.Provenance);

            // Framework-boundary stop: don't descend into generic framework internals
            if (IsFrameworkLeaf(child))
            {
                children.Add(new TraceStep(child, ToSeam(edge.Kind), depth + 1)
                {
                    Provenance = edge.Provenance,
                    Resolution = edge.Resolution,
                    Salient = salient,
                });
                continue;
            }

            children.Add(Walk(child, ToSeam(edge.Kind), edge.Provenance, edge.Resolution,
                depth + 1, opts, follow, visited) with { Salient = salient });
        }

        return new TraceStep(node, seam, depth)
        {
            Provenance = provenance,
            Resolution = resolution,
            Children = children.ToImmutable(),
            Truncated = ranked.Count > taken.Count,
        };
    }

    private static int EdgePriority(EdgeKind kind) => kind switch
    {
        EdgeKind.Sends => 0,     // highest: dispatch is the core story
        EdgeKind.Handles => 1,   // handler is the response
        EdgeKind.Raises => 2,    // events are important
        EdgeKind.Consumes => 3,  // event consumption
        EdgeKind.ReadsWrites => 4, // data access
        EdgeKind.Resolves => 5,  // DI wiring
        EdgeKind.WrappedBy => 6, // pipeline wrappers
        _ => 7,                  // Calls — lowest priority, most likely to be framework noise
    };

    private static bool IsFrameworkLeaf(GraphNode node)
    {
        var title = node.Title;
        return title.StartsWith("Microsoft.", StringComparison.Ordinal)
            || title.StartsWith("System.", StringComparison.Ordinal)
            || title == "DbContext"
            || title is "ILogger" or "IMediator" or "ISender" or "IPublisher"
            || title.Contains("Mediator", StringComparison.Ordinal) && title != "MediatorExtension";
    }

    /// <summary>Out-edges of a node, plus those of its Type twin. A Handler/Service/Member node and the class's
    /// Type node are the same class with different ids: the Handles/Resolves/Consumes/Calls edge lands on the
    /// Handler/Service/Member node, but the class's own call/raise/data edges were attached to the Type node.
    /// Without this bridge the trace dead-ends the moment it crosses an indirection seam into a handler.</summary>
    private IEnumerable<GraphEdge> OutEdgesWithTwin(NodeId id)
    {
        foreach (var e in _graph.OutEdges(id))
            yield return e;

        if (id.Kind is NodeKind.Handler or NodeKind.Service or NodeKind.Member)
        {
            var typeKey = id.Kind == NodeKind.Member
                ? ExtractTypeKey(id.Key)
                : id.Key;
            var twin = NodeId.ForType(typeKey);
            if (_graph.Contains(twin))
                foreach (var e in _graph.OutEdges(twin))
                    yield return e;
        }
    }

    /// <summary>"TypeFqn.MethodName" → "TypeFqn"</summary>
    private static string ExtractTypeKey(string memberKey)
    {
        var dot = memberKey.LastIndexOf('.');
        return dot > 0 ? memberKey[..dot] : memberKey;
    }

    private bool HasFollowable(NodeId id, HashSet<EdgeKind> follow)
        => OutEdgesWithTwin(id).Any(e => follow.Contains(e.Kind));

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

    /// <summary>Extracts up to 3 salient source lines around a provenance line from SourceBody.</summary>
    private static ImmutableArray<string> ExtractSalient(string? sourceBody, string? provenance)
    {
        if (string.IsNullOrEmpty(sourceBody) || string.IsNullOrEmpty(provenance))
            return [];

        var colon = provenance.LastIndexOf(':');
        if (colon < 0 || !int.TryParse(provenance[(colon + 1)..], out var lineNumber))
            return [];

        var lines = sourceBody.Replace("\r\n", "\n").Split('\n');
        var idx = lineNumber - 1; // provenance is 1-based
        if (idx < 0 || idx >= lines.Length)
            return [];

        var salientLines = ImmutableArray.CreateBuilder<string>();
        var context = 1; // 1 line before and after
        for (var i = Math.Max(0, idx - context); i <= Math.Min(lines.Length - 1, idx + context); i++)
        {
            var line = lines[i].Trim();
            if (line.Length > 0)
                salientLines.Add(line);
        }
        return salientLines.ToImmutable();
    }
}
