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

/// <summary>G3.2 — one edge kind and how many edges carry it, in one direction from one node.</summary>
public sealed record EdgeKindCount(EdgeKind Kind, int Count);

/// <summary>A kind-filtered neighbour answer that still knows what it filtered out:
/// <paramref name="Edges"/> is the filtered list, while <paramref name="TotalEdges"/> and
/// <paramref name="KindsPresent"/> describe the UNFILTERED edges in the same direction.</summary>
public sealed record NeighborView(
    ImmutableArray<EdgeRef> Edges,
    int TotalEdges,
    ImmutableArray<EdgeKindCount> KindsPresent);

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
    private readonly Lazy<Dictionary<NodeId, List<NodeId>>> _membersByType;

    /// <summary>Creates a query over the queryable parts of an analysis (from an AnalysisSnapshot).</summary>
    public GraphQuery(CodeGraph graph, ImmutableArray<EntryPoint> entries, MapModel? map = null)
    {
        _graph = graph;
        _entries = entries.IsDefault ? [] : entries;
        _map = map;
        _membersByType = new Lazy<Dictionary<NodeId, List<NodeId>>>(BuildMemberIndex);
    }

    /// <summary>C3 (Prism D2): Type → member NodeIds — the rollup index. Member↔Type degree lives
    /// fragmented (member→member Calls hang off Member nodes while the Type node reads 0 in/0 out),
    /// so every degree-shaped query on a connected type dead-ended (audit: PodcastService impact
    /// up = 0 while it IS the target of GET /landing).</summary>
    private Dictionary<NodeId, List<NodeId>> BuildMemberIndex()
    {
        var map = new Dictionary<NodeId, List<NodeId>>();
        foreach (var n in _graph.Nodes)
        {
            if (n.Id.Kind != NodeKind.Member) continue;
            if (!n.Id.Key.Contains("::", StringComparison.Ordinal)) continue;
            var typeId = NodeId.ForType(Graph2.SymbolCanon.OwnerTypeOf(n.Id.Key));
            if (!map.TryGetValue(typeId, out var list)) map[typeId] = list = [];
            list.Add(n.Id);
        }
        return map;
    }

    /// <summary>"TypeFqn::Member" → "TypeFqn"; a Type/Entry key passes through.</summary>
    private static string OwnerTypeKey(NodeId id)
        => id.Kind != NodeKind.Member ? id.Key : Graph2.SymbolCanon.OwnerTypeOf(id.Key);

    /// <summary>C3 — a node's edges in one direction, ROLLED UP for Type nodes: the type's own edges
    /// plus its members' CROSS-TYPE edges (intra-type member→member wiring stays internal — a type's
    /// neighbors are its collaborators, not its private helpers). De-duplicated by (From, To, Kind).
    /// Non-Type nodes return their direct edges unchanged.</summary>
    private ImmutableArray<GraphEdge> RolledEdges(NodeId id, EdgeDirection direction, EdgeKind? kind = null)
    {
        var direct = direction == EdgeDirection.Out ? _graph.OutEdges(id, kind) : _graph.InEdges(id, kind);
        if (id.Kind != NodeKind.Type || !_membersByType.Value.TryGetValue(id, out var members))
            return direct;

        var b = ImmutableArray.CreateBuilder<GraphEdge>();
        var seen = new HashSet<(NodeId, NodeId, EdgeKind)>();
        foreach (var e in direct)
        {
            if (seen.Add((e.From, e.To, e.Kind))) b.Add(e);
        }
        foreach (var m in members)
        {
            var edges = direction == EdgeDirection.Out ? _graph.OutEdges(m, kind) : _graph.InEdges(m, kind);
            foreach (var e in edges)
            {
                var other = direction == EdgeDirection.Out ? e.To : e.From;
                if (string.Equals(OwnerTypeKey(other), id.Key, StringComparison.Ordinal)) continue;
                if (seen.Add((e.From, e.To, e.Kind))) b.Add(e);
            }
        }
        return b.ToImmutable();
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
    /// matches no entry/node. Same resolution + traversal the CLI/Desktop use.
    /// <para>Batch E: this is THE build. Depth/fan-out defaults, the seam order, the framework stop and
    /// the budget rule all come from <see cref="TracePolicy"/>, so a focus traced through the CLI, the
    /// gRPC surface or MCP produces the same steps.</para></summary>
    public Trace? Trace(string focus, int depth = TracePolicy.DefaultDepth,
        int maxFanOut = TracePolicy.DefaultFanOut, int budgetTokens = 0, bool explicitDepth = true)
    {
        var entry = ResolveEntry(focus);
        return entry is null ? null : Trace(entry, depth, maxFanOut, budgetTokens, explicitDepth);
    }

    /// <summary>The entry a focus resolves to, or null. Exposed (G1.2) so a caller that needs the ROOT
    /// as well as the walk — the context pack, which asks whether the root is a declared entry or a
    /// symbol — resolves through the SAME inventory this query traces with, instead of a second one.</summary>
    public EntryPoint? ResolveEntry(string focus) => EntryPointResolver.Resolve(_entries, _graph, focus);

    /// <summary>Batch E — the build, for callers that already resolved the entry (the render path, which
    /// would otherwise resolve and walk a second time). <paramref name="explicitDepth"/> false lets the
    /// budget deepen the walk; an explicit dial is honoured exactly.</summary>
    public Trace Trace(EntryPoint entry, int depth = TracePolicy.DefaultDepth,
        int maxFanOut = TracePolicy.DefaultFanOut, int budgetTokens = 0, bool explicitDepth = true)
    {
        var builder = new TraceBuilder(_graph);
        var trace = builder.Build(entry, new TraceOptions { MaxDepth = depth, MaxFanOut = maxFanOut });

        // Budget-elastic depth (R2 §2.E item 1): a fixed default truncates a small entry that had room
        // to spare. Only when the caller left the depth to us, the walk actually hit the limit, and the
        // result uses little of the budget do we walk again deeper — at most once.
        if (!explicitDepth && budgetTokens > 0)
        {
            var deeper = TracePolicy.ElasticDepth(depth, TraceBuilder.EstimateTraceTokens(trace),
                budgetTokens, HitDepthLimit(trace.Root, depth));
            if (deeper > depth)
                trace = builder.Build(entry, new TraceOptions { MaxDepth = deeper, MaxFanOut = maxFanOut });
        }

        // T3.3 — the token budget shapes the tree post-build (query layer, not graph assembly — the
        // kernel invariant is preserved). 0 = unlimited, so the default trace is unchanged.
        return budgetTokens > 0 ? TraceBuilder.ShapeToBudget(trace, budgetTokens) : trace;
    }

    /// <summary>True when some branch stopped because it ran out of DEPTH (not fan-out) — the signal
    /// that walking deeper would actually show more.</summary>
    private static bool HitDepthLimit(TraceStep step, int maxDepth)
    {
        if (step.Truncated && step.Depth >= maxDepth) return true;
        foreach (var child in step.Children)
            if (HitDepthLimit(child, maxDepth)) return true;
        return false;
    }

    /// <summary>node(id) — the detail card for a node, or null when it doesn't exist.</summary>
    public NodeDetail? Node(NodeId id)
    {
        var n = _graph.Node(id);
        if (n is null) return null;
        // C3: Type-node degrees include the members' cross-type edges — a connected type never reads 0/0.
        return new NodeDetail(n.Id, n.Title, n.Kind, n.Tags, n.FilePath,
            RolledEdges(id, EdgeDirection.Out).Length, RolledEdges(id, EdgeDirection.In).Length, n.LineNumber);
    }

    /// <summary>neighbors(id, direction) — the edges out of (callees) or into (callers) a node, as
    /// navigation results. Optionally filtered by seam kind. C3: on a Type node the members' cross-type
    /// edges roll up, and each EdgeRef keeps the true member endpoints — the answer shows WHICH member
    /// carries the collaboration.</summary>
    public ImmutableArray<EdgeRef> Neighbors(NodeId id, EdgeDirection direction, EdgeKind? kind = null)
        => NeighborsView(id, direction, kind).Edges;

    /// <summary>
    /// G3.2 (R4 item 9) — the same answer, plus the two facts a filter destroys.
    /// <para><see cref="NeighborView.Edges"/> is filtered; <see cref="NeighborView.TotalEdges"/> and
    /// <see cref="NeighborView.KindsPresent"/> describe the UNFILTERED edges in the same direction.
    /// Without them "nothing writes this table" and "this node has no edges at all" arrive identical,
    /// and a caller whose kind guess missed has nothing to retry with.</para>
    /// <para>One walk, then the filter, so the list and the numbers that describe what it left out
    /// can never be about different sets. Filtering after the roll-up equals filtering inside it: the
    /// dedup key in <see cref="RolledEdges"/> includes the kind, and its intra-type skip does not
    /// depend on the kind.</para>
    /// </summary>
    public NeighborView NeighborsView(NodeId id, EdgeDirection direction, EdgeKind? kind = null)
    {
        var all = RolledEdges(id, direction);

        var counts = new Dictionary<EdgeKind, int>();
        var b = ImmutableArray.CreateBuilder<EdgeRef>();
        foreach (var e in all)
        {
            counts[e.Kind] = counts.GetValueOrDefault(e.Kind) + 1;
            if (kind is { } k && e.Kind != k) continue;
            var otherId = direction == EdgeDirection.Out ? e.To : e.From;
            var otherTitle = _graph.Node(otherId)?.Title ?? otherId.Key;
            b.Add(new EdgeRef(e.From, e.To, e.Kind, e.Resolution, e.Provenance, otherTitle));
        }

        // Busiest kind first, ties broken by the enum's own order: a stable list whose head is the
        // kind a caller who guessed wrong most likely wanted.
        var kinds = counts.OrderByDescending(p => p.Value).ThenBy(p => (int)p.Key)
            .Select(p => new EdgeKindCount(p.Key, p.Value))
            .ToImmutableArray();

        return new NeighborView(b.ToImmutable(), all.Length, kinds);
    }

    /// <summary>G3.2 — parse a caller's kind name against <see cref="EdgeKind"/> ITSELF, so no surface
    /// keeps a second copy of the list (the drift class G2.1 took out of the tool menu). Case-insensitive.
    /// Rejects the underlying number, which <see cref="Enum.TryParse{TEnum}(string, bool, out TEnum)"/>
    /// accepts and no caller means.</summary>
    public static bool TryParseEdgeKind(string? name, out EdgeKind kind)
    {
        kind = default;
        if (string.IsNullOrWhiteSpace(name)) return false;
        foreach (var n in Enum.GetNames<EdgeKind>())
        {
            if (!string.Equals(n, name, StringComparison.OrdinalIgnoreCase)) continue;
            kind = Enum.Parse<EdgeKind>(n);
            return true;
        }
        return false;
    }

    /// <summary>G3.2 — every edge kind a caller may ask for, in enum order. The one list.</summary>
    public static ImmutableArray<string> EdgeKindNames { get; } = [.. Enum.GetNames<EdgeKind>()];

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

        // "Type:Method" (and the full "Type::Method" key form) → Member by FQN suffix.
        var sep = s.IndexOf("::", StringComparison.Ordinal);
        var colon = sep >= 0 ? sep : s.IndexOf(':');
        if (colon > 0)
        {
            var type = s[..colon];
            var method = s[(colon + (sep >= 0 ? 2 : 1))..].Trim();
            foreach (var n in _graph.Nodes)
                if (n.Kind == NodeKind.Member
                    && n.Id.Key.EndsWith($"::{method}", StringComparison.Ordinal)
                    && Graph2.SymbolCanon.TypeIdMatches(
                        Graph2.SymbolCanon.OwnerTypeOf(n.Id.Key), type, StringComparison.OrdinalIgnoreCase))
                    return n.Id;
        }

        // Short name or FQN suffix on a Type/EntryPoint, preferring the most-connected.
        GraphNode? best = null;
        foreach (var n in _graph.Nodes)
        {
            if (n.Kind is not (NodeKind.Type or NodeKind.EntryPoint)) continue;
            var keyMatches = n.Kind == NodeKind.Type
                ? Graph2.SymbolCanon.TypeIdMatches(n.Id.Key, s, StringComparison.OrdinalIgnoreCase)
                : n.Id.Key.EndsWith("." + s, StringComparison.OrdinalIgnoreCase);
            if (!string.Equals(n.Title, s, StringComparison.OrdinalIgnoreCase) && !keyMatches) continue;
            // C3: rolled degree — the resolver must prefer the type that is actually connected
            // (through its members), not whichever bare Type node happens to carry a stray edge.
            if (best is null
                || RolledEdges(n.Id, EdgeDirection.Out).Length + RolledEdges(n.Id, EdgeDirection.In).Length
                   > RolledEdges(best.Id, EdgeDirection.Out).Length + RolledEdges(best.Id, EdgeDirection.In).Length)
                best = n;
        }
        return best?.Id;
    }

    /// <summary>L3.3 — Returns archetype-aware interesting starting points. Each archetype gets a
    /// tailored strategy; unknown/empty archetypes fall back to top-centrality. Returns up to 20
    /// points, each with a human-readable "why" explanation.</summary>
    public ImmutableArray<InterestingPoint> GetInterestingPoints(string? archetype = null)
    {
        var raw = (archetype?.ToLowerInvariant()) switch
        {
            "web" => InterestingForWeb(),
            "library" => InterestingForLibrary(),
            "messaging" => InterestingForMessaging(),
            "desktop" => InterestingForDesktop(),
            "cli" => InterestingForCli(),
            _ => InterestingByCentrality(),
        };
        // T3.5 — "Start here" must point at repo code an agent would actually open first, never
        // framework types (List, System.*) or infra stores (a DbContext). Same spirit as the target
        // noise rules (TraceBuilder.IsFrameworkLeaf); applied to every strategy's output as a catch-all.
        return raw.Where(p => !IsStartHereNoise(p.Id)).ToImmutableArray();
    }

    private static readonly HashSet<string> BclNoiseTitles = new(StringComparer.Ordinal)
    {
        "List", "Dictionary", "HashSet", "IEnumerable", "IList", "ICollection", "IDictionary",
        "IReadOnlyList", "IReadOnlyCollection", "Task", "ValueTask", "String", "Object", "Array",
        "Guid", "DateTime", "TimeSpan", "DbContext", "ILogger", "IMediator", "ISender", "IPublisher",
    };

    /// <summary>T3.5 — a node that should never be offered as a starting point: an infra store
    /// (DbContext), a BCL/framework type (System.*, Microsoft.*, List/Dictionary/…), or a type not
    /// declared in the analyzed repo (no source file). Services/entries are exempt (they legitimately
    /// have no single declaring file).</summary>
    private bool IsStartHereNoise(NodeId id)
    {
        var n = _graph.Node(id);
        if (n is null) return true;
        if (n.Kind == NodeKind.Store || n.Tags.Contains(RoleTags.DataStore)) return true;
        var t = n.Title;
        if (t.StartsWith("System.", StringComparison.Ordinal) || t.StartsWith("Microsoft.", StringComparison.Ordinal))
            return true;
        if (BclNoiseTitles.Contains(t)) return true;
        if (n.FilePath is null && n.Kind is not (NodeKind.Service or NodeKind.EntryPoint)) return true;
        return false;
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
        foreach (var n in _graph.Nodes
            .Where(n => !IsStartHereNoise(n.Id)) // T3.5 — filter before Take so 20 clean points survive
            .OrderByDescending(n => _graph.OutEdges(n.Id).Length + _graph.InEdges(n.Id).Length).Take(20))
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

            // C3: rolled edges — impact seeded on (or passing through) a Type node expands through
            // its members' cross-type edges instead of dead-ending on the bare type.
            if (direction == ImpactDirection.Up || direction == ImpactDirection.Both)
            {
                foreach (var edge in RolledEdges(current, EdgeDirection.In))
                    if (!visited.Contains(edge.From))
                        queue.Enqueue((edge.From, dist + 1));
            }

            if (direction == ImpactDirection.Down || direction == ImpactDirection.Both)
            {
                foreach (var edge in RolledEdges(current, EdgeDirection.Out))
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

    /// <summary>G3.1 (R4 item 8) — default hop budget for a seam search.</summary>
    public const int DefaultSeamDepth = 8;

    /// <summary>G3.1 — how many shortest paths a seam returns by default.</summary>
    public const int DefaultSeamPaths = 3;

    /// <summary>
    /// seam(from, to) — THE PATH BETWEEN TWO SYMBOLS. Every other query here is single-source
    /// (<see cref="Impact"/> = what reaches X, <see cref="BlastRadius"/> = which entries reach X,
    /// <see cref="FindCallers"/> = who calls X); none of them answers the two-ended question, which
    /// is the one an agent asks out loud — "does the checkout endpoint reach the payment service,
    /// and through what?".
    ///
    /// <para>Returns the SHORTEST paths only, which is a definition and not a hedge: shortest is the
    /// one answer that is well-defined, bounded, and stable. <c>TotalPaths</c> is the exact number of
    /// shortest paths (counted over the search DAG, not by enumerating them), so a caller shown 3 of
    /// 12 knows it was shown 3 of 12.</para>
    ///
    /// <para>When nothing runs from → to, the REVERSE direction is searched before answering: "B
    /// reaches A in 3 hops" and "these two are unconnected" are different facts, and an agent that
    /// gets the second for the first will conclude the wrong thing. <c>StoppedAtDepthLimit</c> keeps
    /// the third case separate again — "no path within <paramref name="maxDepth"/> hops" is not "no
    /// path".</para>
    ///
    /// <para>C3 roll-up applies at both ends: a Type departs through its members' edges
    /// (<see cref="RolledEdges"/>) and arriving at any member of a target Type is arriving at the
    /// Type. Without it a Type→Type seam dead-ends on two bare nodes that carry no edges of their
    /// own. Each hop names the TRUE edge endpoints, so where a member carries the collaboration the
    /// answer says which member.</para>
    /// </summary>
    public SeamResult Seam(NodeId from, NodeId to, int maxDepth = DefaultSeamDepth, int maxPaths = DefaultSeamPaths)
    {
        maxDepth = Math.Max(1, maxDepth);
        maxPaths = Math.Max(1, maxPaths);

        var forward = SearchSeam(from, to, maxDepth, maxPaths);
        if (forward.Paths.Length > 0)
            return new SeamResult(SeamDirection.Forward, forward.Paths, forward.Hops, forward.TotalPaths, forward.StoppedAtDepthLimit);

        var reverse = SearchSeam(to, from, maxDepth, maxPaths);
        if (reverse.Paths.Length > 0)
            return new SeamResult(SeamDirection.Reverse, reverse.Paths, reverse.Hops, reverse.TotalPaths, reverse.StoppedAtDepthLimit);

        return new SeamResult(SeamDirection.None, [], 0, 0,
            forward.StoppedAtDepthLimit || reverse.StoppedAtDepthLimit);
    }

    /// <summary>One direction of <see cref="Seam"/>: BFS out-edges from source, collecting every
    /// shortest-path predecessor, then read the paths back off that DAG.</summary>
    private (ImmutableArray<SeamPath> Paths, int Hops, int TotalPaths, bool StoppedAtDepthLimit)
        SearchSeam(NodeId source, NodeId target, int maxDepth, int maxPaths)
    {
        // Arriving at any member of a target Type IS arriving at the type (the C3 roll-up, read from
        // the arrival side). Deterministic order: the target itself, then its members in graph order.
        var targets = new List<NodeId> { target };
        if (target.Kind == NodeKind.Type && _membersByType.Value.TryGetValue(target, out var targetMembers))
            targets.AddRange(targetMembers);
        var targetSet = targets.ToHashSet();

        if (targetSet.Contains(source))
            return ([new SeamPath([])], 0, 1, false);

        // Predecessors on a shortest path. Via is the node the BFS EXPANDED; Edge.From can differ
        // from it when a Type rolled its member's edge up, and the hop must show the member.
        var dist = new Dictionary<NodeId, int> { [source] = 0 };
        var preds = new Dictionary<NodeId, List<(NodeId Via, GraphEdge Edge)>>();
        var frontier = new List<NodeId> { source };
        var arrivalDepth = -1;
        var stoppedAtDepthLimit = false;

        for (var depth = 0; depth < maxDepth && frontier.Count > 0 && arrivalDepth < 0; depth++)
        {
            var next = new List<NodeId>();
            foreach (var node in frontier)
            {
                foreach (var edge in RolledEdges(node, EdgeDirection.Out))
                {
                    var to = edge.To;
                    if (dist.TryGetValue(to, out var known))
                    {
                        // Another equally-short way in — a real alternative path, not a duplicate.
                        if (known == depth + 1) preds[to].Add((node, edge));
                        continue;
                    }
                    dist[to] = depth + 1;
                    preds[to] = [(node, edge)];
                    next.Add(to);
                    if (targetSet.Contains(to)) arrivalDepth = depth + 1;
                }
            }
            frontier = next;
            // The budget ran out with somewhere still to go: "no path in 8 hops", not "no path".
            if (arrivalDepth < 0 && depth + 1 == maxDepth && next.Count > 0) stoppedAtDepthLimit = true;
        }

        if (arrivalDepth < 0) return ([], 0, 0, stoppedAtDepthLimit);

        var arrivals = targets.Where(t => dist.TryGetValue(t, out var d) && d == arrivalDepth).ToList();

        // Exact count over the DAG — enumerating to count would be exponential on a hub.
        var counts = new Dictionary<NodeId, long>();
        long CountTo(NodeId node)
        {
            if (node == source) return 1;
            if (counts.TryGetValue(node, out var memo)) return memo;
            counts[node] = 0; // cycles cannot occur on a BFS layer DAG; this also guards re-entry
            long total = 0;
            if (preds.TryGetValue(node, out var ps))
                foreach (var (via, _) in ps)
                    total = Math.Min(int.MaxValue, total + CountTo(via));
            counts[node] = total;
            return total;
        }

        var totalPaths = (int)Math.Min(int.MaxValue, arrivals.Sum(CountTo));

        var paths = ImmutableArray.CreateBuilder<SeamPath>();
        var acc = new List<GraphEdge>();
        void Walk(NodeId node)
        {
            if (paths.Count >= maxPaths) return;
            if (node == source)
            {
                var hops = ImmutableArray.CreateBuilder<SeamHop>(acc.Count);
                for (var i = acc.Count - 1; i >= 0; i--) hops.Add(ToHop(acc[i]));
                paths.Add(new SeamPath(hops.ToImmutable()));
                return;
            }
            if (!preds.TryGetValue(node, out var ps)) return;
            foreach (var (via, edge) in ps)
            {
                if (paths.Count >= maxPaths) return;
                acc.Add(edge);
                Walk(via);
                acc.RemoveAt(acc.Count - 1);
            }
        }
        foreach (var arrival in arrivals) Walk(arrival);

        return (paths.ToImmutable(), arrivalDepth, totalPaths, stoppedAtDepthLimit);
    }

    private SeamHop ToHop(GraphEdge e)
    {
        var fromNode = _graph.Node(e.From);
        var toNode = _graph.Node(e.To);
        return new SeamHop(e.From, e.To,
            fromNode?.Title ?? e.From.Key, toNode?.Title ?? e.To.Key,
            e.Kind, e.Resolution, fromNode?.FilePath, fromNode?.LineNumber);
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
        => FindPage(query, null, limit).Results;

    /// <summary>R4 item 6 — <see cref="Find"/> with the kind filter applied BEFORE the limit, plus
    /// the count of everything that matched.
    ///
    /// Both halves exist because the caller was doing them after the fact: the MCP asked for a page,
    /// filtered THAT by kind, and then reported the survivors as the total. A filter downstream of a
    /// truncation can only ever describe the window, so a kind-filtered find(kind:"Type") answered
    /// "how many Types are in the first N matches", which is a fact about N.
    ///
    /// <paramref name="kind"/> matches <see cref="NodeKind"/> by name, case-insensitively; an
    /// unrecognised kind matches nothing (0 results and a total of 0 — a true answer, not an error).
    /// <paramref name="limit"/> caps the returned page only; TotalMatches is uncapped.</summary>
    public (ImmutableArray<SearchResult> Results, int TotalMatches) FindPage(string query, string? kind, int limit)
    {
        if (string.IsNullOrWhiteSpace(query))
            return (ImmutableArray<SearchResult>.Empty, 0);

        var term = query.Trim();
        var results = new List<(SearchResult Result, int Rank)>();

        foreach (var n in _graph.Nodes)
        {
            var title = n.Title;
            var titleMatch = title.Contains(term, StringComparison.OrdinalIgnoreCase);
            var keyMatch = n.Id.Key.Contains(term, StringComparison.OrdinalIgnoreCase);

            if (!titleMatch && !keyMatch)
                continue;

            if (kind is { Length: > 0 } && !string.Equals(n.Kind.ToString(), kind, StringComparison.OrdinalIgnoreCase))
                continue;

            var rank = MatchRank(title, term);

            var outD = _graph.OutEdges(n.Id).Length;
            var inD = _graph.InEdges(n.Id).Length;
            results.Add((new SearchResult(n.Id, title, n.Kind, outD + inD), rank));
        }

        var page = results
            .OrderBy(r => r.Rank)
            .ThenByDescending(r => KindPriority(r.Result.Kind))
            .ThenByDescending(r => r.Result.Degree)
            .Take(limit)
            .Select(r => r.Result)
            .ToImmutableArray();

        return (page, results.Count);
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

/// <summary>G3.1 — which way round a seam actually connects. <c>Reverse</c> is the answer to a
/// question the caller did not ask, returned because "B reaches A" is a fact, and letting it read
/// as <c>None</c> would be a false negative the caller cannot see.</summary>
public enum SeamDirection { None, Forward, Reverse }

/// <summary>G3.1 — one hop of a seam path. Endpoints are the TRUE edge endpoints: where a type's
/// member carries the collaboration, the hop names the member.</summary>
public sealed record SeamHop(
    NodeId From,
    NodeId To,
    string FromTitle,
    string ToTitle,
    EdgeKind Kind,
    Resolution Resolution,
    string? FilePath,
    int? LineNumber);

/// <summary>G3.1 — one route from the source end to the target end. Empty hops = the two ends
/// resolved to the same symbol.</summary>
public sealed record SeamPath(ImmutableArray<SeamHop> Hops);

/// <summary>G3.1 — the seam between two symbols. <paramref name="TotalPaths"/> is the exact number
/// of shortest paths, which is usually more than <paramref name="Paths"/> holds.</summary>
public sealed record SeamResult(
    SeamDirection Direction,
    ImmutableArray<SeamPath> Paths,
    int Hops,
    int TotalPaths,
    bool StoppedAtDepthLimit);

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
