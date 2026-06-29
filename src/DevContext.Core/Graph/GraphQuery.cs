namespace DevContext.Core.Graph;

/// <summary>Direction for a neighbor query: <c>Out</c> = edges this node points along (callees, sends,
/// raises…), <c>In</c> = edges that point at this node (callers, who-sends-me…).</summary>
public enum EdgeDirection { Out, In }

/// <summary>A node's detail card for browse/MCP: identity, role tags, declaration site, and degree both
/// ways (so a UI can show "12 callers / 3 callees" without another query).</summary>
public sealed record NodeDetail(
    NodeId Id,
    string Title,
    NodeKind Kind,
    ImmutableArray<string> Tags,
    string? FilePath,
    int OutDegree,
    int InDegree);

/// <summary>A directed edge as a navigation result: the edge plus the resolved title of the node on the
/// other end (the one the caller is navigating TO).</summary>
public sealed record EdgeRef(
    NodeId From,
    NodeId To,
    EdgeKind Kind,
    Resolution Resolution,
    string? Provenance,
    string OtherTitle);

/// <summary>
/// The kernel's query layer — *analyze once, query many* (PRODUCT-DIRECTION.md §6). A thin, face-agnostic,
/// JSON-friendly facade over one immutable <see cref="CodeGraph"/> + its entry inventory + Map. The CLI,
/// browse UI, and MCP server are all clients of these operations (the CLI's render path is re-expressed
/// over this; the UI/MCP call them directly). No rendering concerns leak in here.
/// </summary>
public sealed class GraphQuery
{
    private readonly CodeGraph _graph;
    private readonly ImmutableArray<EntryPoint> _entries;
    private readonly MapModel? _map;

    /// <summary>Creates a query over the queryable parts of an analysis (from an AnalysisSnapshot).</summary>
    public GraphQuery(CodeGraph graph, ImmutableArray<EntryPoint> entries, MapModel? map = null)
    {
        _graph = graph;
        _entries = entries.IsDefault ? [] : entries;
        _map = map;
    }

    /// <summary>The underlying graph (for callers that still need direct access during the transition).</summary>
    public CodeGraph Graph => _graph;

    /// <summary>entrypoints(filter?) — the roots a trace can start from, optionally by kind.</summary>
    public ImmutableArray<EntryPoint> EntryPoints(EntryPointKind? kind = null)
        => kind is null ? _entries : [.. _entries.Where(e => e.Kind == kind)];

    /// <summary>map(facet?) — the orientation artifact (null on dry-run).</summary>
    public MapModel? Map() => _map;

    /// <summary>stats() — per-seam edge counts (with the approx share) and entry→target coverage.</summary>
    public (ImmutableArray<SeamStat> Seams, int EntriesWithTarget) Stats()
        => GraphStats.Compute(_graph, _entries);

    /// <summary>trace(entry, depth, ...) — resolve a focus to an entry and walk it. Null when the focus
    /// matches no entry/node. Same resolution + traversal the CLI/Desktop use.</summary>
    public Trace? Trace(string focus, int depth = 6, int maxFanOut = 12)
    {
        var entry = EntryPointResolver.Resolve(_entries, _graph, focus);
        if (entry is null) return null;
        return new TraceBuilder(_graph).Build(entry, new TraceOptions { MaxDepth = depth, MaxFanOut = maxFanOut });
    }

    /// <summary>node(id) — the detail card for a node, or null when it doesn't exist.</summary>
    public NodeDetail? Node(NodeId id)
    {
        var n = _graph.Node(id);
        if (n is null) return null;
        return new NodeDetail(n.Id, n.Title, n.Kind, n.Tags, n.FilePath,
            _graph.OutEdges(id).Length, _graph.InEdges(id).Length);
    }

    /// <summary>neighbors(id, direction) — the edges out of (callees) or into (callers) a node, as
    /// navigation results. Optionally filtered by seam kind.</summary>
    public ImmutableArray<EdgeRef> Neighbors(NodeId id, EdgeDirection direction, EdgeKind? kind = null)
    {
        var edges = direction == EdgeDirection.Out ? _graph.OutEdges(id, kind) : _graph.InEdges(id, kind);
        var b = ImmutableArray.CreateBuilder<EdgeRef>(edges.Length);
        foreach (var e in edges)
        {
            var otherId = direction == EdgeDirection.Out ? e.To : e.From;
            var otherTitle = _graph.Node(otherId)?.Title ?? otherId.Key;
            b.Add(new EdgeRef(e.From, e.To, e.Kind, e.Resolution, e.Provenance, otherTitle));
        }
        return b.ToImmutable();
    }

    /// <summary>find_usages(id) — who references this node (the inverse query): its in-edges.</summary>
    public ImmutableArray<EdgeRef> FindUsages(NodeId id) => Neighbors(id, EdgeDirection.In);

    /// <summary>Resolves a user string (short name, FQN suffix, or "Type:Method") to a node id — the
    /// convenience faces use before calling <see cref="Node"/>/<see cref="Neighbors"/>/<see cref="FindUsages"/>.
    /// Prefers an exact Member match, then a Type/EntryPoint by title or FQN suffix.</summary>
    public NodeId? ResolveNodeId(string nameOrKey)
    {
        if (string.IsNullOrWhiteSpace(nameOrKey)) return null;
        var s = nameOrKey.Trim();

        // Exact id key match in any kind.
        foreach (var kind in new[] { NodeKind.Member, NodeKind.Type, NodeKind.EntryPoint })
            if (_graph.Contains(new NodeId(kind, s)))
                return new NodeId(kind, s);

        // "Type:Method" → Member by FQN suffix.
        var colon = s.IndexOf(':');
        if (colon > 0)
        {
            var type = s[..colon];
            var method = s[(colon + 1)..].Trim();
            foreach (var n in _graph.Nodes)
                if (n.Kind == NodeKind.Member
                    && n.Id.Key.EndsWith($".{method}", StringComparison.Ordinal)
                    && (n.Id.Key.Equals($"{type}.{method}", StringComparison.OrdinalIgnoreCase)
                        || n.Id.Key.EndsWith($".{type}.{method}", StringComparison.OrdinalIgnoreCase)))
                    return n.Id;
        }

        // Short name or FQN suffix on a Type/EntryPoint, preferring the most-connected.
        GraphNode? best = null;
        foreach (var n in _graph.Nodes)
        {
            if (n.Kind is not (NodeKind.Type or NodeKind.EntryPoint)) continue;
            if (!string.Equals(n.Title, s, StringComparison.OrdinalIgnoreCase)
                && !n.Id.Key.EndsWith("." + s, StringComparison.OrdinalIgnoreCase)) continue;
            if (best is null
                || _graph.OutEdges(n.Id).Length + _graph.InEdges(n.Id).Length
                   > _graph.OutEdges(best.Id).Length + _graph.InEdges(best.Id).Length)
                best = n;
        }
        return best?.Id;
    }
}
