namespace DevContext.Core.Graph;

// ─────────────────────────────────────────────────────────────────────────────────────────────
// The CodeGraph — the connective-tissue model the old "detection accumulator" lacked.
// Built once at analyze-time by GraphBuilder (joining existing detections + types + call edges),
// traversed at render-time by TraceBuilder / MapBuilder. Designed to be JSON-serializable so a
// persistent content-keyed index can cache it later (TRACE-ENGINE-DESIGN.md §3). This file is the
// stable core; evolve builders/resolvers around it, not the node/edge shapes.
// ─────────────────────────────────────────────────────────────────────────────────────────────

/// <summary>The kind of a node in the <see cref="CodeGraph"/>.</summary>
public enum NodeKind
{
    Type, Member, EntryPoint, Request, Handler, Entity, Event, DataStore, Service,
}

/// <summary>The kind of a directed edge. Each maps to a trace "seam". Direction is always caller→callee
/// so a forward walk from an entry point flows DOWN the wiring.</summary>
public enum EdgeKind
{
    /// <summary>Direct method call: member → member.</summary>
    Calls,
    /// <summary>A member dispatches a request (MediatR Send/Publish): member → request.</summary>
    Sends,
    /// <summary>A request is handled: request → handler.</summary>
    Handles,
    /// <summary>A member raises a domain/integration event: member → event.</summary>
    Raises,
    /// <summary>An event is consumed: event → handler.</summary>
    Consumes,
    /// <summary>A member reads/writes an entity or data store: member → entity/store.</summary>
    ReadsWrites,
    /// <summary>An interface/abstract resolves to a concrete impl via DI: interface → impl.</summary>
    Resolves,
    /// <summary>A request is wrapped by a pipeline behavior: request → behavior.</summary>
    WrappedBy,
}

/// <summary>How confidently an edge was established — surfaced in the report (P3: show your work).</summary>
public enum Resolution
{
    /// <summary>Derived by joining two existing detections (high confidence).</summary>
    Join,
    /// <summary>Resolved by syntax/string heuristics (approximate).</summary>
    Syntactic,
    /// <summary>Resolved via a Roslyn SemanticModel symbol (verified).</summary>
    Semantic,
}

/// <summary>
/// Stable identity for a node. <see cref="Key"/> is a canonical, serialization-stable string
/// (FQN for types/members, "VERB route" for endpoints, request type for requests). The (Kind, Key)
/// pair is unique. This id scheme is the backbone of every join — keep it boring and deterministic.
/// </summary>
public readonly record struct NodeId(NodeKind Kind, string Key)
{
    /// <summary>Stable string form, e.g. "Type:Acme.Orders.Order".</summary>
    public override string ToString() => $"{Kind}:{Key}";

    public static NodeId ForType(string fqn) => new(NodeKind.Type, fqn);
    public static NodeId ForMember(string typeFqn, string member) => new(NodeKind.Member, $"{typeFqn}.{member}");
    public static NodeId ForEntry(string key) => new(NodeKind.EntryPoint, key);
    public static NodeId ForRequest(string requestType) => new(NodeKind.Request, requestType);
    public static NodeId ForHandler(string handlerFqn) => new(NodeKind.Handler, handlerFqn);
    public static NodeId ForEntity(string entityType) => new(NodeKind.Entity, entityType);
    public static NodeId ForEvent(string eventType) => new(NodeKind.Event, eventType);
    public static NodeId ForService(string serviceType) => new(NodeKind.Service, serviceType);
}

/// <summary>A node. Serialization-stable: holds primitive data, never live model references.</summary>
public sealed record GraphNode(
    NodeId Id,
    string Title,
    NodeKind Kind)
{
    /// <summary>Declaring file, when known.</summary>
    public string? FilePath { get; init; }
    /// <summary>Owning project/service, when known.</summary>
    public string? Project { get; init; }
    /// <summary>Free-form labels (e.g. "aggregate", "command", "scoped").</summary>
    public ImmutableArray<string> Tags { get; init; } = [];
}

/// <summary>A directed, typed edge with provenance and resolution confidence.</summary>
public sealed record GraphEdge(
    NodeId From,
    NodeId To,
    EdgeKind Kind)
{
    /// <summary>"file:line" of the call/dispatch site, when known.</summary>
    public string? Provenance { get; init; }
    /// <summary>How this edge was established.</summary>
    public Resolution Resolution { get; init; } = Resolution.Join;
    /// <summary>0..1 confidence.</summary>
    public float Confidence { get; init; } = 1.0f;
}

/// <summary>Immutable, queryable graph. Construct via <see cref="CodeGraphBuilder"/>.</summary>
public sealed class CodeGraph
{
    private readonly FrozenDictionary<NodeId, GraphNode> _nodes;
    private readonly FrozenDictionary<NodeId, ImmutableArray<GraphEdge>> _outEdges;

    /// <summary>Creates a graph from a node map and an outgoing-edge adjacency map.</summary>
    public CodeGraph(
        IReadOnlyDictionary<NodeId, GraphNode> nodes,
        IReadOnlyDictionary<NodeId, ImmutableArray<GraphEdge>> outEdges)
    {
        _nodes = nodes.ToFrozenDictionary();
        _outEdges = outEdges.ToFrozenDictionary();
    }

    /// <summary>All nodes.</summary>
    public IReadOnlyCollection<GraphNode> Nodes => _nodes.Values;
    /// <summary>Total node count.</summary>
    public int NodeCount => _nodes.Count;
    /// <summary>Total edge count.</summary>
    public int EdgeCount => _outEdges.Values.Sum(e => e.Length);

    /// <summary>Returns the node with the given id, or null.</summary>
    public GraphNode? Node(NodeId id) => _nodes.TryGetValue(id, out var n) ? n : null;
    /// <summary>True if a node with the given id exists.</summary>
    public bool Contains(NodeId id) => _nodes.ContainsKey(id);

    /// <summary>Outgoing edges from a node, optionally filtered by kind.</summary>
    public ImmutableArray<GraphEdge> OutEdges(NodeId id, EdgeKind? kind = null)
    {
        if (!_outEdges.TryGetValue(id, out var edges)) return [];
        return kind is null ? edges : [.. edges.Where(e => e.Kind == kind)];
    }
}

/// <summary>Mutable builder for <see cref="CodeGraph"/>. Deduplicates nodes (first write wins) and edges.</summary>
public sealed class CodeGraphBuilder
{
    private readonly Dictionary<NodeId, GraphNode> _nodes = [];
    private readonly Dictionary<NodeId, List<GraphEdge>> _out = [];
    private readonly HashSet<(NodeId, NodeId, EdgeKind)> _edgeKeys = [];

    /// <summary>Adds a node, or returns the existing one with the same id (first write wins).</summary>
    public GraphNode AddNode(GraphNode node)
    {
        if (_nodes.TryGetValue(node.Id, out var existing)) return existing;
        _nodes[node.Id] = node;
        return node;
    }

    /// <summary>Adds an edge if both endpoints exist and the (from, to, kind) triple is new.</summary>
    public bool AddEdge(GraphEdge edge)
    {
        if (!_nodes.ContainsKey(edge.From) || !_nodes.ContainsKey(edge.To)) return false;
        if (!_edgeKeys.Add((edge.From, edge.To, edge.Kind))) return false;
        if (!_out.TryGetValue(edge.From, out var list)) _out[edge.From] = list = [];
        list.Add(edge);
        return true;
    }

    /// <summary>True if a node with the given id has been added.</summary>
    public bool HasNode(NodeId id) => _nodes.ContainsKey(id);

    /// <summary>Freezes the accumulated nodes/edges into an immutable <see cref="CodeGraph"/>.</summary>
    public CodeGraph Build()
    {
        var outFrozen = _out.ToDictionary(kv => kv.Key, kv => kv.Value.ToImmutableArray());
        return new CodeGraph(_nodes, outFrozen);
    }
}
