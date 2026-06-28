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
    /// <summary>Number of followable branches omitted at this node (fan-out cap or depth limit) — rendered
    /// explicitly so a cut is honest, not silent (Iteration 3 Step 4).</summary>
    public int Omitted { get; init; }
    /// <summary>Pipeline behaviors that wrap the request reached by a <c>Sends</c> edge — rendered once as
    /// an annotation under the send (Iteration 3 Step 3).</summary>
    public ImmutableArray<string> Pipeline { get; init; } = [];
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
    private readonly Dictionary<NodeId, List<NodeId>> _bridgeMembersByType;

    /// <summary>Creates a trace builder over a graph.</summary>
    public TraceBuilder(CodeGraph graph)
    {
        _graph = graph;
        _bridgeMembersByType = BuildBridgeMemberIndex(graph);
    }

    /// <summary>Precomputes, once, a Type→[bridgeable-member] lookup used by <see cref="OutEdgesWithTwin"/>:
    /// <list type="bullet">
    /// <item>handler/consumer entry members (Handle/HandleAsync/Consume/ConsumeAsync/Execute*/Invoke*) — a
    /// Handles/Consumes seam lands on the handler Type, so we bridge into the method that has the wiring;</item>
    /// <item>the <b>constructor</b> of an entity/aggregate Type (method name == the type's short name) — so
    /// reaching an aggregate via a <c>data</c> edge surfaces the domain event its ctor raises (Iteration 3:
    /// the <c>OrderStartedDomainEvent → handler</c> path the trace used to drop). Scoped to entity/aggregate
    /// tags so non-domain ctors aren't expanded.</item></list></summary>
    private static Dictionary<NodeId, List<NodeId>> BuildBridgeMemberIndex(CodeGraph graph)
    {
        var map = new Dictionary<NodeId, List<NodeId>>();
        foreach (var node in graph.Nodes)
        {
            if (node.Id.Kind != NodeKind.Member) continue;
            var dot = node.Id.Key.LastIndexOf('.');
            if (dot <= 0) continue;
            var method = node.Id.Key[(dot + 1)..];
            var typeId = NodeId.ForType(node.Id.Key[..dot]);

            bool bridge;
            if (IsHandlerEntryMethod(method))
                bridge = true;
            else if (graph.Node(typeId) is { } t
                && string.Equals(method, t.Title, StringComparison.Ordinal)
                && (t.Tags.Contains(RoleTags.Aggregate) || t.Tags.Contains(RoleTags.Entity)))
                bridge = true; // entity/aggregate constructor — surfaces its domain-event raises
            else
                bridge = false;

            if (!bridge) continue;
            if (!map.TryGetValue(typeId, out var list)) map[typeId] = list = [];
            list.Add(node.Id);
        }
        return map;
    }

    private static bool IsHandlerEntryMethod(string method)
        => method is "Handle" or "HandleAsync" or "Consume" or "ConsumeAsync"
            || method.StartsWith("Execute", StringComparison.Ordinal)
            || method.StartsWith("Invoke", StringComparison.Ordinal);

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
        if (IsEventNode(step.Node))
            emitted.Add(step.Node.Title);
        foreach (var child in step.Children)
            CollectSummaries(child, touched, emitted);
    }

    /// <summary>For every visited node, collect entity/aggregate targets reached via its out-edges. EF
    /// access is usually a <c>Calls</c> edge onto an entity/repository member (not a <c>ReadsWrites</c>
    /// edge), so we collect entities reached via <b>both</b> <c>ReadsWrites</c> and <c>Calls</c> — resolving
    /// a Member callee to its owning entity Type (Iteration 3 High-5). Also collects entities that connect
    /// TO a visited DataStore node. De-duped by title with <see cref="IsNoiseEntity"/> filtering.</summary>
    private void CollectGraphEntities(HashSet<NodeId> visited, List<string> touched)
    {
        foreach (var nodeId in visited)
        {
            foreach (var edge in OutEdgesWithTwin(nodeId))
            {
                if (edge.Kind is not (EdgeKind.ReadsWrites or EdgeKind.Calls)) continue;
                var entity = ResolveEntityNode(_graph.Node(edge.To));
                if (entity is not null && !IsNoiseEntity(entity))
                    touched.Add(entity.Title);
            }
        }
        // Also collect entities that connect TO a visited DataStore node
        foreach (var (nodeId, node) in _graph.Nodes.Select(n => (n.Id, n)))
        {
            if (!IsEntityNode(node)) continue;
            if (IsNoiseEntity(node)) continue;
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

    /// <summary>Filters EF artifacts that get mistaken for entities: synthetic names (&lt;OnModelCreating&gt;),
    /// migration classes by name, and — the robust catch — anything declared under a Migrations folder
    /// (e.g. a migration class like <c>ForeignKeyChange</c> that doesn't match a name pattern).</summary>
    private static bool IsNoiseEntity(GraphNode node)
        => node.Title.StartsWith('<')
        || node.Title.Contains("Migration")
        || node.Title.Contains("Initial")
        || (node.FilePath is { } f
            && (f.Contains("\\Migrations\\", StringComparison.OrdinalIgnoreCase)
                || f.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase)));

    /// <summary>A node is an emitted event if tagged as a domain/integration event (role tags replaced
    /// the old NodeKind.Event).</summary>
    private static bool IsEventNode(GraphNode n)
        => n.Tags.Contains(RoleTags.DomainEvent) || n.Tags.Contains(RoleTags.IntegrationEvent);

    /// <summary>A node is an entity if tagged entity/aggregate (replaced the old NodeKind.Entity).</summary>
    private static bool IsEntityNode(GraphNode n)
        => n.Tags.Contains(RoleTags.Entity) || n.Tags.Contains(RoleTags.Aggregate);

    /// <summary>Resolves an edge target (a Type or Member node) to its owning entity Type, or null.
    /// For a Member callee, looks up the owning Type (via <see cref="ExtractTypeKey"/>) and checks its
    /// entity/aggregate tags — this is what makes TOUCHES surface the real entities reached via Calls
    /// (EF access) after the Iteration 3 Step 1 extension.</summary>
    private GraphNode? ResolveEntityNode(GraphNode? node)
    {
        if (node is null) return null;
        if (node.Kind is NodeKind.Type && IsEntityNode(node)) return node;
        if (node.Kind is NodeKind.Member)
        {
            var typeId = NodeId.ForType(ExtractTypeKey(node.Id.Key));
            var owner = _graph.Node(typeId);
            if (owner is not null && IsEntityNode(owner)) return owner;
        }
        return null;
    }

    private TraceStep Walk(GraphNode node, SeamKind seam, string? provenance, Resolution resolution,
        int depth, TraceOptions opts, HashSet<EdgeKind> follow, HashSet<NodeId> visited)
    {
        if (!visited.Add(node.Id) || depth >= opts.MaxDepth)
        {
            var omitted = OutEdgesWithTwin(node.Id).Where(e => follow.Contains(e.Kind))
                .Select(e => (e.To, e.Kind)).Distinct().Count();
            return new TraceStep(node, seam, depth)
            {
                Provenance = provenance,
                Resolution = resolution,
                Truncated = omitted > 0,
                Omitted = omitted,
            };
        }

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

            // Salient source comes from the FROM node's body; for a Member node, fall back to its
            // parent Type's body (the body scans attach edges + bodies at the type level).
            var fromNode = _graph.Node(edge.From) ?? node;
            var salientSource = fromNode.SourceBody ?? (
                fromNode.Id.Kind == NodeKind.Member
                && _graph.Node(NodeId.ForType(ExtractTypeKey(fromNode.Id.Key))) is { } twin
                    ? twin.SourceBody
                    : null);
            var salient = ExtractSalient(salientSource, edge.Provenance);

            // Framework-boundary stop: don't descend into generic framework internals
            // Pipeline annotation (Iteration 3 Step 3): when a Sends edge reaches a request Type,
            // collect the WrappedBy behaviors so the renderer can show the pipeline once under the send.
            var pipeline = edge.Kind == EdgeKind.Sends
                ? _graph.OutEdges(child.Id, EdgeKind.WrappedBy)
                    .Select(e => _graph.Node(e.To)?.Title)
                    .Where(t => t is not null)
                    .Cast<string>()
                    .ToImmutableArray()
                : [];

            if (IsFrameworkLeaf(child))
            {
                children.Add(new TraceStep(child, ToSeam(edge.Kind), depth + 1)
                {
                    Provenance = edge.Provenance,
                    Resolution = edge.Resolution,
                    Salient = salient,
                    Pipeline = pipeline,
                });
                continue;
            }

            children.Add(Walk(child, ToSeam(edge.Kind), edge.Provenance, edge.Resolution,
                depth + 1, opts, follow, visited) with { Salient = salient, Pipeline = pipeline });
        }

        return new TraceStep(node, seam, depth)
        {
            Provenance = provenance,
            Resolution = resolution,
            Children = children.ToImmutable(),
            Truncated = ranked.Count > taken.Count,
            Omitted = ranked.Count - taken.Count,
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

    /// <summary>Out-edges of a node, with the controlled member bridge (Phase 1). A <b>Member</b> node
    /// yields ONLY its own edges — the per-method set is now correct, so inheriting the parent Type's edges
    /// (the old behaviour) is exactly the bug that made sibling methods' traces identical. A <b>Type</b>
    /// node yields its own join edges PLUS the edges of its handler-entry members (Handle/HandleAsync/…):
    /// Handles/Consumes seams land on the handler Type, so we bridge into the one entry method that carries
    /// the handler's real wiring — not every sibling method.</summary>
    private IEnumerable<GraphEdge> OutEdgesWithTwin(NodeId id)
    {
        foreach (var e in _graph.OutEdges(id))
            yield return e;

        if (id.Kind is NodeKind.Type && _bridgeMembersByType.TryGetValue(id, out var members))
            foreach (var memberId in members)
                foreach (var e in _graph.OutEdges(memberId))
                    yield return e;
    }

    /// <summary>"TypeFqn.MethodName" → "TypeFqn"</summary>
    private static string ExtractTypeKey(string memberKey)
    {
        var dot = memberKey.LastIndexOf('.');
        return dot > 0 ? memberKey[..dot] : memberKey;
    }

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
