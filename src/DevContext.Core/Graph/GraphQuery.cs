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
    int InDegree,
    int? LineNumber = null);

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

    /// <summary>L4 — Precomputed flows (spine-only), one per entry. Computed at assembly time.</summary>
    public ImmutableArray<Flow> Flows => _graph.Flows;

    /// <summary>entrypoints(filter?) — the roots a trace can start from, optionally by kind.</summary>
    public ImmutableArray<EntryPoint> EntryPoints(EntryPointKind? kind = null)
        => kind is null ? _entries : [.. _entries.Where(e => e.Kind == kind)];

    /// <summary>map(facet?) — the orientation artifact (null on dry-run).</summary>
    public MapModel? Map() => _map;

    /// <summary>stats() — per-seam edge counts (with the approx share) and entry→target coverage.</summary>
    public (ImmutableArray<SeamStat> Seams, int EntriesWithTarget, int EntriesWithDeepSpine, double DeepSpineRatio) Stats()
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
            _graph.OutEdges(id).Length, _graph.InEdges(id).Length, n.LineNumber);
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

    /// <summary>L3.3 — Returns archetype-aware interesting starting points. Each archetype gets a
    /// tailored strategy; unknown/empty archetypes fall back to top-centrality. Returns up to 20
    /// points, each with a human-readable "why" explanation.</summary>
    public ImmutableArray<InterestingPoint> GetInterestingPoints(string? archetype = null)
    {
        return (archetype?.ToLowerInvariant()) switch
        {
            "web" => InterestingForWeb(),
            "library" => InterestingForLibrary(),
            "messaging" => InterestingForMessaging(),
            "desktop" => InterestingForDesktop(),
            "cli" => InterestingForCli(),
            _ => InterestingByCentrality(),
        };
    }

    private ImmutableArray<InterestingPoint> InterestingForWeb()
    {
        var results = ImmutableArray.CreateBuilder<InterestingPoint>();
        var seen = new HashSet<NodeId>();

        // Auth boundary entries
        foreach (var e in _entries.Where(e => !e.AuthAttributes.IsDefaultOrEmpty))
        {
            var node = _graph.Node(e.Node);
            if (node is null || !seen.Add(e.Node)) continue;
            results.Add(new InterestingPoint(e.Node, e.Title, node.Kind,
                $"Auth boundary: {string.Join(", ", e.AuthAttributes)}", node.Tags));
        }

        // Data hubs: most-connected entity/aggregate nodes
        foreach (var n in _graph.Nodes)
        {
            if (!seen.Add(n.Id)) continue;
            if (!n.Tags.Contains(RoleTags.Entity) && !n.Tags.Contains(RoleTags.Aggregate)) continue;
            var degree = _graph.OutEdges(n.Id).Length + _graph.InEdges(n.Id).Length;
            if (degree >= 3)
                results.Add(new InterestingPoint(n.Id, n.Title, n.Kind,
                    $"Data hub: {degree} connections", n.Tags));
        }

        // Middleware: Pipeline-tagged nodes
        foreach (var n in _graph.Nodes)
        {
            if (!seen.Add(n.Id)) continue;
            if (!n.Tags.Contains(RoleTags.Pipeline)) continue;
            results.Add(new InterestingPoint(n.Id, n.Title, n.Kind,
                "Pipeline/middleware", n.Tags));
        }

        return results.OrderByDescending(r => Score(r, _graph)).Take(20).ToImmutableArray();
    }

    private ImmutableArray<InterestingPoint> InterestingForLibrary()
    {
        var results = ImmutableArray.CreateBuilder<InterestingPoint>();
        var seen = new HashSet<NodeId>();

        // Public API: top-degree types (they're the surface)
        foreach (var n in _graph.Nodes.OrderByDescending(n =>
            _graph.OutEdges(n.Id).Length + _graph.InEdges(n.Id).Length).Take(15))
        {
            if (!seen.Add(n.Id)) continue;
            if (n.Kind != NodeKind.Type) continue;
            var deg = _graph.OutEdges(n.Id).Length + _graph.InEdges(n.Id).Length;
            results.Add(new InterestingPoint(n.Id, n.Title, n.Kind,
                $"Public API hub: {deg} connections", n.Tags));
        }

        // Implementor seats: interfaces/abstract types with most Resolves edges
        foreach (var n in _graph.Nodes)
        {
            if (!seen.Add(n.Id)) continue;
            var resolveCount = _graph.OutEdges(n.Id).Count(e => e.Kind == EdgeKind.Resolves);
            if (resolveCount >= 2)
                results.Add(new InterestingPoint(n.Id, n.Title, n.Kind,
                    $"Seat: {resolveCount} implementations", n.Tags));
        }

        return results.Take(20).ToImmutableArray();
    }

    private ImmutableArray<InterestingPoint> InterestingForMessaging()
    {
        var results = ImmutableArray.CreateBuilder<InterestingPoint>();
        var seen = new HashSet<NodeId>();

        // Message producers: entry points with Raises/Sends edges
        foreach (var e in _entries)
        {
            var sends = _graph.OutEdges(e.Node).Count(ed => ed.Kind is EdgeKind.Sends or EdgeKind.Raises);
            if (sends == 0) continue;
            var node = _graph.Node(e.Node);
            if (node is null || !seen.Add(e.Node)) continue;
            results.Add(new InterestingPoint(e.Node, e.Title, node.Kind,
                $"Producer: {sends} message edges", node.Tags));
        }

        // Consumers: Event/Notification handler entries
        foreach (var e in _entries.Where(e => e.Kind == EntryPointKind.DomainEventHandler
            || e.Kind == EntryPointKind.MessageConsumer))
        {
            var node = _graph.Node(e.Node);
            if (node is null || !seen.Add(e.Node)) continue;
            results.Add(new InterestingPoint(e.Node, e.Title, node.Kind,
                "Message consumer", node.Tags));
        }

        return results.Take(20).ToImmutableArray();
    }

    private ImmutableArray<InterestingPoint> InterestingForDesktop()
    {
        var results = ImmutableArray.CreateBuilder<InterestingPoint>();

        // Module hubs: top-central types per project
        var byProject = new Dictionary<string, List<GraphNode>>();
        foreach (var n in _graph.Nodes)
        {
            if (n.FilePath is not { } fp) continue;
            var proj = n.Project ?? Path.GetFileNameWithoutExtension(fp) ?? fp;
            if (!byProject.ContainsKey(proj)) byProject[proj] = [];
            byProject[proj].Add(n);
        }

        var seen = new HashSet<NodeId>();
        foreach (var (proj, nodes) in byProject)
        {
            var top = nodes.OrderByDescending(n =>
                _graph.OutEdges(n.Id).Length + _graph.InEdges(n.Id).Length).Take(3);
            foreach (var n in top)
            {
                if (!seen.Add(n.Id)) continue;
                var deg = _graph.OutEdges(n.Id).Length + _graph.InEdges(n.Id).Length;
                results.Add(new InterestingPoint(n.Id, n.Title, n.Kind,
                    $"Module hub ({proj}): {deg} connections", n.Tags));
            }
        }

        return results.Take(20).ToImmutableArray();
    }

    private ImmutableArray<InterestingPoint> InterestingForCli()
    {
        var results = ImmutableArray.CreateBuilder<InterestingPoint>();
        var seen = new HashSet<NodeId>();

        // Command tree root: top-level CLI command entries
        foreach (var e in _entries.Where(e => e.Kind == EntryPointKind.CliCommand))
        {
            var node = _graph.Node(e.Node);
            if (node is null || !seen.Add(e.Node)) continue;
            var deg = _graph.OutEdges(e.Node).Length + _graph.InEdges(e.Node).Length;
            results.Add(new InterestingPoint(e.Node, e.Title, node.Kind,
                $"CLI entry: {deg} connections", node.Tags));
        }

        return results.Take(20).ToImmutableArray();
    }

    private ImmutableArray<InterestingPoint> InterestingByCentrality()
    {
        var results = ImmutableArray.CreateBuilder<InterestingPoint>();
        foreach (var n in _graph.Nodes.OrderByDescending(n =>
            _graph.OutEdges(n.Id).Length + _graph.InEdges(n.Id).Length).Take(20))
        {
            var deg = _graph.OutEdges(n.Id).Length + _graph.InEdges(n.Id).Length;
            results.Add(new InterestingPoint(n.Id, n.Title, n.Kind,
                $"Central type: {deg} connections", n.Tags));
        }
        return results.ToImmutable();
    }

    private static int Score(InterestingPoint ip, CodeGraph graph)
    {
        var deg = graph.OutEdges(ip.Id).Length + graph.InEdges(ip.Id).Length;
        return deg + (ip.Why.StartsWith("Auth") ? 10 : 0);
    }

    /// <summary>M4.4 — Unified impact analysis: BFS over in-edges (up), out-edges (down), or both.
    /// Returns affected nodes with titles, file paths, service info, and hop distances.
    /// Depth-capped, cycle-safe. Max 500 results.</summary>
    public ImmutableArray<ImpactResult> Impact(NodeId from, ImpactDirection direction = ImpactDirection.Up, int maxDepth = 4)
    {
        var results = ImmutableArray.CreateBuilder<ImpactResult>();
        var visited = new HashSet<NodeId>();
        var queue = new Queue<(NodeId, int)>();

        if (direction == ImpactDirection.Up || direction == ImpactDirection.Both)
            queue.Enqueue((from, 0));
        if (direction == ImpactDirection.Down)
            queue.Enqueue((from, 0));

        var entryDict = _entries.GroupBy(e => e.Node).ToDictionary(g => g.Key, g => g.First());

        while (queue.Count > 0 && results.Count < 500)
        {
            var (current, dist) = queue.Dequeue();
            if (dist > maxDepth || !visited.Add(current)) continue;

            if (current != from)
            {
                var node = _graph.Node(current);
                var service = node?.Project ?? "";
                var filePath = node?.FilePath;
                var lineNumber = node?.LineNumber;

                var title = node?.Title ?? current.Key;
                var kind = node?.Kind.ToString() ?? "Unknown";

                if (entryDict.TryGetValue(current, out var entry))
                {
                    title = entry.Title;
                    kind = entry.Kind.ToString();
                }

                results.Add(new ImpactResult(
                    title, kind, dist, current, filePath, lineNumber, service));
            }

            if (direction == ImpactDirection.Up || direction == ImpactDirection.Both)
            {
                foreach (var edge in _graph.InEdges(current))
                    if (!visited.Contains(edge.From))
                        queue.Enqueue((edge.From, dist + 1));
            }

            if (direction == ImpactDirection.Down || direction == ImpactDirection.Both)
            {
                foreach (var edge in _graph.OutEdges(current))
                    if (!visited.Contains(edge.To))
                        queue.Enqueue((edge.To, dist + 1));
            }
        }

        return results.ToImmutable();
    }

    /// <summary>M4.4 diff-aware mode — find all graph nodes whose file path matches one of the
    /// given paths (normalized), then return their union impact closure.</summary>
    public ImmutableArray<ImpactResult> ImpactFromFiles(IEnumerable<string> filePaths, ImpactDirection direction = ImpactDirection.Down, int maxDepth = 4)
    {
        var normalized = new HashSet<string>(filePaths.Select(NormalizePath), StringComparer.OrdinalIgnoreCase);
        var affectedNodes = new HashSet<NodeId>();

        foreach (var node in _graph.Nodes)
        {
            if (node.FilePath is { } fp && normalized.Contains(NormalizePath(fp)))
                affectedNodes.Add(node.Id);
        }

        var allResults = ImmutableArray.CreateBuilder<ImpactResult>();
        foreach (var nodeId in affectedNodes)
        {
            foreach (var r in Impact(nodeId, direction, maxDepth))
                allResults.Add(r);
        }

        return allResults.DistinctBy(r => r.NodeId).Take(500).ToImmutableArray();
    }

    /// <summary>M4.9 helper — BFS over IN-edges from target; returns all caller nodes with
    /// distances. The caller (gRPC handler) filters for test methods.</summary>
    public ImmutableArray<(NodeId NodeId, string Title, string? FilePath, int? LineNumber, string? Project, int Distance)>
        FindCallers(NodeId target, int maxDepth = 6)
    {
        var results = ImmutableArray.CreateBuilder<(NodeId, string, string?, int?, string?, int)>();
        var visited = new HashSet<NodeId>();
        var queue = new Queue<(NodeId, int)>();
        queue.Enqueue((target, 0));

        while (queue.Count > 0 && results.Count < 500)
        {
            var (current, dist) = queue.Dequeue();
            if (dist > maxDepth || !visited.Add(current)) continue;

            if (current != target)
            {
                var node = _graph.Node(current);
                results.Add((current, node?.Title ?? current.Key, node?.FilePath, node?.LineNumber, node?.Project, dist));
            }

            foreach (var edge in _graph.InEdges(current))
                if (!visited.Contains(edge.From))
                    queue.Enqueue((edge.From, dist + 1));
        }

        return results.ToImmutable();
    }

    /// <summary>M4.7 helper — find all graph nodes whose file path matches one of the given paths.</summary>
    public ImmutableArray<GraphNode> NodesInFiles(IEnumerable<string> filePaths)
    {
        var normalized = new HashSet<string>(filePaths.Select(NormalizePath), StringComparer.OrdinalIgnoreCase);
        return _graph.Nodes
            .Where(n => n.FilePath is { } fp && normalized.Contains(NormalizePath(fp)))
            .ToImmutableArray();
    }

    private static string NormalizePath(string path) =>
        path.Replace('\\', '/').TrimEnd('/');

    /// <summary>I5 F13 — Blast Radius: BFS over in-edges from a node to find which entry points
    /// reach it. Returns the entry titles with hop distances. Depth-capped, cycle-safe.
    /// (Kept for backward compatibility; prefer Impact(direction: Up).)</summary>
    public ImmutableArray<BlastResult> BlastRadius(NodeId from, int maxDepth = 4)
    {
        var results = ImmutableArray.CreateBuilder<BlastResult>();
        var visited = new HashSet<NodeId>();
        var queue = new Queue<(NodeId, int)>();
        queue.Enqueue((from, 0));

        var entryDict = _entries.GroupBy(e => e.Node).ToDictionary(g => g.Key, g => g.First());

        while (queue.Count > 0 && results.Count < 500)
        {
            var (current, dist) = queue.Dequeue();
            if (dist > maxDepth || !visited.Add(current)) continue;

            if (entryDict.TryGetValue(current, out var entry) && current != from)
                results.Add(new BlastResult(
                    entry.Title,
                    entry.Kind.ToString(),
                    dist));

            foreach (var edge in _graph.InEdges(current))
                if (!visited.Contains(edge.From))
                    queue.Enqueue((edge.From, dist + 1));
        }

        return results.ToImmutable();
    }

    /// <summary>search(term) — finds nodes whose title or id key contain the term.
    /// Capped at 20 results, ranked by degree (most-connected first).</summary>
    public ImmutableArray<SearchResult> Search(string term, int cap = 20)
    {
        var results = new List<SearchResult>();
        foreach (var n in _graph.Nodes)
        {
            if (n.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                || n.Id.Key.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                var outD = _graph.OutEdges(n.Id).Length;
                var inD = _graph.InEdges(n.Id).Length;
                results.Add(new SearchResult(n.Id, n.Title, n.Kind, outD + inD));
            }
        }

        return results
            .OrderByDescending(r => r.Degree)
            .Take(cap)
            .ToImmutableArray();
    }

    /// <summary>find(query, limit) — L5.3 ranked resolution shared by resolve/find/usages/impact.
    /// Rank: exact Title == query > Title starts-with (prefix) > Title word-boundary > Title contains.
    /// Tiebreaker: Types over Members over other kinds; then by total degree (out+in edges).</summary>
    public ImmutableArray<SearchResult> Find(string query, int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
            return ImmutableArray<SearchResult>.Empty;

        var term = query.Trim();
        var results = new List<(SearchResult Result, int Rank)>();

        foreach (var n in _graph.Nodes)
        {
            var title = n.Title;
            var titleMatch = title.Contains(term, StringComparison.OrdinalIgnoreCase);
            var keyMatch = n.Id.Key.Contains(term, StringComparison.OrdinalIgnoreCase);

            if (!titleMatch && !keyMatch)
                continue;

            var rank = MatchRank(title, term);

            var outD = _graph.OutEdges(n.Id).Length;
            var inD = _graph.InEdges(n.Id).Length;
            results.Add((new SearchResult(n.Id, title, n.Kind, outD + inD), rank));
        }

        return results
            .OrderBy(r => r.Rank)
            .ThenByDescending(r => KindPriority(r.Result.Kind))
            .ThenByDescending(r => r.Result.Degree)
            .Take(limit)
            .Select(r => r.Result)
            .ToImmutableArray();
    }

    private static int MatchRank(string title, string term)
    {
        if (string.Equals(title, term, StringComparison.OrdinalIgnoreCase))
            return 0; // exact match
        if (title.StartsWith(term, StringComparison.OrdinalIgnoreCase))
            return 1; // prefix match
        if (IsWordBoundary(title, term))
            return 2; // word-boundary match (e.g. ".Order" or "OrderService")
        return 3; // general substring match
    }

    private static bool IsWordBoundary(string title, string term)
    {
        var len = title.Length;
        var tlen = term.Length;
        if (tlen == 0 || tlen > len) return false;

        for (var i = 0; i <= len - tlen; i++)
        {
            if (!MemoryExtensions.Equals(
                    title.AsSpan(i, tlen),
                    term.AsSpan(),
                    StringComparison.OrdinalIgnoreCase))
                continue;

            var before = i == 0 || !char.IsLetterOrDigit(title[i - 1]);
            var after = i + tlen == len || !char.IsLetterOrDigit(title[i + tlen]);
            if (before && after)
                return true;
        }
        return false;
    }

    /// <summary>L5.3 — Type nodes rank higher than Member nodes. Higher number = higher priority.</summary>
    private static int KindPriority(NodeKind kind) => kind switch
    {
        NodeKind.Type => 3,
        NodeKind.Service => 3,
        NodeKind.EntryPoint => 2,
        NodeKind.Member => 2,
        NodeKind.Message => 1,
        NodeKind.Store => 1,
        _ => 0,
    };
}

/// <summary>M4.4 — Impact analysis direction.</summary>
public enum ImpactDirection { Up, Down, Both }

/// <summary>M4.4 — Impact result: a node affected by or affecting a target node.</summary>
public sealed record ImpactResult(
    string Title,
    string Kind,
    int Hops,
    NodeId NodeId,
    string? FilePath,
    int? LineNumber,
    string Service);

/// <summary>Blast radius result: an entry point reachable from a target node.</summary>
public sealed record BlastResult(string EntryTitle, string Kind, int Hops);

/// <summary>Search result: a node matching a keyword query, with its degree for ranking.</summary>
public sealed record SearchResult(NodeId Id, string Title, NodeKind Kind, int Degree);

/// <summary>L3.3 — A curated starting point for understanding a repo, returned by
/// <see cref="GraphQuery.GetInterestingPoints"/>. The <c>Why</c> field explains the rationale
/// (e.g. "auth boundary", "central type", "public API seat").</summary>
public sealed record InterestingPoint(
    NodeId Id,
    string Title,
    NodeKind Kind,
    string Why,
    ImmutableArray<string> Tags);
