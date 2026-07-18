using DevContext.Core.Models;

namespace DevContext.Core.Graph;

// ─────────────────────────────────────────────────────────────────────────────────────────────
// The CodeGraph — the connective-tissue model the old "detection accumulator" lacked.
// Built once at analyze-time by GraphBuilder (joining existing detections + types + call edges),
// traversed at render-time by TraceBuilder / MapBuilder. Designed to be JSON-serializable so a
// persistent content-keyed index can cache it later (TRACE-ENGINE-DESIGN.md §3). This file is the
// stable core; evolve builders/resolvers around it, not the node/edge shapes.
// ─────────────────────────────────────────────────────────────────────────────────────────────

/// <summary>The kind of a node in the <see cref="CodeGraph"/>. A node is an *entity*: a declared
/// type, one of its members, or an application entry point. A class's *role* (handler, command,
/// entity, …) is NOT a node kind — it's a <see cref="RoleTags">tag</see> on the single Type node, so
/// one C# class is exactly one node and every edge it participates in lands on that one identity.</summary>
public enum NodeKind
{
    Type, Member, EntryPoint,
    Service, Message, Store,
}

/// <summary>Role labels attached to a Type node's <see cref="GraphNode.Tags"/>. These replace the old
/// per-role node kinds (Request/Handler/Event/Entity/DataStore/Service): the relationship lives in the
/// <see cref="EdgeKind"/>, the role lives here. A node can carry several (a class can be both an
/// aggregate and an entity, or a handler that is also a service).</summary>
public static class RoleTags
{
    public const string Command = "command";
    public const string Query = "query";
    public const string Notification = "notification";
    public const string Handler = "handler";
    public const string DomainEvent = "domain-event";
    public const string IntegrationEvent = "integration-event";
    public const string Entity = "entity";
    public const string Aggregate = "aggregate";
    public const string Service = "service";
    public const string Pipeline = "pipeline";
    public const string DataStore = "datastore";
    public const string Consumer = "consumer";
    /// <summary>Marks a Service node as a runnable deployable (has an entry assembly / web SDK), as
    /// opposed to a Service node synthesized for a class library that only participates in a
    /// cross-service seam. Design §1.3: "A class library is NOT a Service."</summary>
    public const string Runnable = "runnable";
    /// <summary>Edge tag on a Resolves edge whose ONLY DI registration is in a test project — a
    /// last-resort wiring the reader should not mistake for the production binding (T2.1).</summary>
    public const string TestOnlyDi = "test-only-di";
    /// <summary>C6 (Prism D1.2f): edge tag on a Resolves edge born from a typed-HttpClient
    /// registration (<c>AddHttpClient&lt;IFeedClient, FeedClient&gt;</c>). There the interface is
    /// pure plumbing — entry targets name the implementation, never the bare interface. Domain
    /// ports (AddScoped bindings) keep their interface-as-contract display.</summary>
    public const string HttpClientBinding = "http-client-binding";
}

/// <summary>Sub-kind tags for <see cref="EdgeKind.ServiceLink"/> edges. Each tag describes the transport
/// seam: bus publish→consume, gRPC client→server, HTTP-via-gateway, or direct Refit/HttpClient.</summary>
public static class ServiceLinkTags
{
    public const string BusPublishConsume = "bus-publish→consume";
    public const string Grpc = "grpc";
    public const string HttpViaGateway = "http-via-gateway";
    public const string RefitDirect = "refit-direct";
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
    /// <summary>Entity-to-entity navigation relationship (HasOne/HasMany/BelongsTo): entity → entity.
    /// Derived from navigation properties; the arrow direction is BelongsTo (child → parent) for
    /// depth-from-aggregate-root computation.</summary>
    EntityRelation,
    /// <summary>Cross-service wiring edge: project A → project B via bus/gRPC/HTTP/YARP. The edge
    /// represents a runtime communication seam between runnable services. Sub-kind is carried as a
    /// tag on the edge via <see cref="ServiceLinkTags"/>.</summary>
    ServiceLink,
    Exposes,
    DependsOn,
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
    public static NodeId ForService(string name) => new(NodeKind.Service, name);
    public static NodeId ForMessage(string fqn) => new(NodeKind.Message, fqn);
    public static NodeId ForStore(string fqn) => new(NodeKind.Store, fqn);
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
    /// <summary>Full source body text of the type declaration (when applicable).</summary>
    public string? SourceBody { get; init; }
    /// <summary>1-based start line of this node's declaration in its source file.</summary>
    public int? LineNumber { get; init; }
    /// <summary>Free-form labels (e.g. "aggregate", "command", "scoped").</summary>
    public ImmutableArray<string> Tags { get; init; } = [];
    /// <summary>D9 Architecture layer classification (e.g. "Api", "Domain", "Infrastructure").</summary>
    public string? Layer { get; init; }
    /// <summary>D9 Feature classification derived from namespace/folder conventions.</summary>
    public string? Feature { get; init; }
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
    /// <summary>Free-form labels for sub-classification (e.g. ServiceLinkTags.BusPublishConsume).</summary>
    public ImmutableArray<string> Tags { get; init; } = [];
    /// <summary>When >1, how many DI implementations exist for this Resolves edge's service type
    /// (I1.6 multi-impl honesty). Zero otherwise.</summary>
    public int MultiImplCount { get; init; }
    /// <summary>All "file:line" registration sites when this Resolves binding is registered from more
    /// than one place (C5: N hosts each wiring the same service→impl). <see cref="Provenance"/> holds the
    /// deterministic first; the trace prefers the focus host's own site at walk time. Empty for
    /// single-site bindings and non-DI edges.</summary>
    public ImmutableArray<string> RegistrationSites { get; init; } = [];
    /// <summary>Owning project name per <see cref="RegistrationSites"/> entry (parallel array, "" when
    /// unresolvable) — how the trace matches a site to its focus host exactly instead of by path guess.</summary>
    public ImmutableArray<string> RegistrationProjects { get; init; } = [];
}

/// <summary>Immutable, queryable graph. Construct via <see cref="CodeGraphBuilder"/>.</summary>
public sealed class CodeGraph
{
    private readonly FrozenDictionary<NodeId, GraphNode> _nodes;
    private readonly FrozenDictionary<NodeId, ImmutableArray<GraphEdge>> _outEdges;
    private readonly FrozenDictionary<NodeId, ImmutableArray<GraphEdge>> _inEdges;

    /// <summary>Creates a graph from a node map and an outgoing-edge adjacency map.</summary>
    public CodeGraph(
        IReadOnlyDictionary<NodeId, GraphNode> nodes,
        IReadOnlyDictionary<NodeId, ImmutableArray<GraphEdge>> outEdges)
    {
        _nodes = nodes.ToFrozenDictionary();
        _outEdges = outEdges.ToFrozenDictionary();

        // D5.3 determinism — FrozenDictionary enumeration order is hash-layout-dependent (randomized
        // per process), so every order-exposing surface captures the CALLER's enumeration order here:
        // the builder's Dictionaries enumerate in insertion order, which is deterministic once the
        // model is sealed. Nodes/AllEdges (and the derived in-edge lists below) must never enumerate
        // the frozen dictionaries directly.
        Nodes = [.. nodes.Values];
        AllEdges = [.. outEdges.Values.SelectMany(e => e)];

        // Inverse adjacency (Phase 5 req 3): derived from out-edges so neighbors(id, in) and
        // find_usages(id) are O(degree), not a full-graph scan. Kept DERIVED — rebuilt here on
        // construct, never serialized — so the graph stays serialization-clean (Phase 9 disk index
        // remains additive).
        var inverse = new Dictionary<NodeId, List<GraphEdge>>();
        foreach (var e in AllEdges)
        {
            if (!inverse.TryGetValue(e.To, out var list)) inverse[e.To] = list = [];
            list.Add(e);
        }
        _inEdges = inverse.ToFrozenDictionary(kv => kv.Key, kv => kv.Value.ToImmutableArray());
    }

    /// <summary>All nodes, in builder insertion order (deterministic — never frozen-dictionary order).</summary>
    public ImmutableArray<GraphNode> Nodes { get; }
    /// <summary>Total node count.</summary>
    public int NodeCount => _nodes.Count;
    /// <summary>Total edge count.</summary>
    public int EdgeCount => AllEdges.Length;
    /// <summary>All edges in the graph, in builder insertion order (deterministic).</summary>
    public ImmutableArray<GraphEdge> AllEdges { get; }
    /// <summary>L3.4 — True when the graph required hub-scoping because normal call-edge binding produced
    /// too few edges (entries &lt; 5 or edge/node ratio &lt; 0.1). Reported honestly in Stats.</summary>
    public bool IsSparseGraph { get; init; }
    /// <summary>L3.4 — Number of hub-scoped nodes additional edges were bound for.</summary>
    public int HubScopeNodeCount { get; init; }
    /// <summary>D9 — Layer violations detected during graph assembly.</summary>
    public ImmutableArray<LayerViolation> LayerViolations { get; init; } = [];
    /// <summary>L4 — Precomputed flows (spine-only), one per entry. Computed at assembly time, consumed
    /// by projections, MCP tools, and UI surfaces (design §1.4, §3).</summary>
    public ImmutableArray<Flow> Flows { get; init; } = [];
    /// <summary>T1.8 — The authoritative entry inventory (the same records GraphBuilder returns), carried
    /// on the graph so projections read the true <see cref="EntryPointKind"/> off the builder-stamped
    /// record instead of re-deriving it from node tags (the "gRPC 75" facet lie). One entry = one record;
    /// a bare EntryPoint node with no matching record is an assembler error, not a PublicApi default.</summary>
    public ImmutableArray<EntryPoint> Entries { get; init; } = [];
    /// <summary>T2.6 — the single publisher→event→consumer projection, built once from the Raises/Consumes
    /// seams. The event board, one-pager, and flow cross-service markers all read this so they cannot tell
    /// three different stories about the same bus.</summary>
    public ImmutableArray<EventWire> EventWiring { get; init; } = [];

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

    /// <summary>Incoming edges to a node (the inverse adjacency), optionally filtered by kind. Powers
    /// <c>neighbors(id, in)</c> and <c>find_usages(id)</c> without a full-graph scan (Phase 5 req 3).</summary>
    public ImmutableArray<GraphEdge> InEdges(NodeId id, EdgeKind? kind = null)
    {
        if (!_inEdges.TryGetValue(id, out var edges)) return [];
        return kind is null ? edges : [.. edges.Where(e => e.Kind == kind)];
    }
}

/// <summary>Mutable builder for <see cref="CodeGraph"/>. Deduplicates nodes (first write wins) and edges.</summary>
public sealed class CodeGraphBuilder
{
    private readonly Dictionary<NodeId, GraphNode> _nodes = [];
    private readonly Dictionary<NodeId, List<GraphEdge>> _out = [];
    private readonly HashSet<(NodeId, NodeId, EdgeKind)> _edgeKeys = [];
    private readonly List<Flow> _flows = [];
    private ImmutableArray<EntryPoint> _entries = [];
    private ImmutableArray<EventWire> _eventWiring = [];

    /// <summary>All nodes added so far.</summary>
    public IEnumerable<GraphNode> Nodes => _nodes.Values;

    /// <summary>Adds a node, or MERGES into the existing one with the same id. Because a class collapses
    /// to one Type node touched by many passes (AddTypeNodes seeds the declaration; each join adds a role
    /// tag), merge = union of <see cref="GraphNode.Tags"/> + first-non-null declaration info
    /// (FilePath/SourceBody/Project). Order-independent: a name-only node added by a join is later enriched
    /// when its declaration appears, and vice-versa. Returns the resulting node.</summary>
    public GraphNode AddNode(GraphNode node)
    {
        if (_nodes.TryGetValue(node.Id, out var existing))
        {
            var mergedTags = existing.Tags.IsDefaultOrEmpty
                ? node.Tags
                : node.Tags.IsDefaultOrEmpty
                    ? existing.Tags
                    : [.. existing.Tags.Union(node.Tags, StringComparer.Ordinal)];
            var merged = existing with
            {
                Tags = mergedTags,
                FilePath = existing.FilePath ?? node.FilePath,
                SourceBody = existing.SourceBody ?? node.SourceBody,
                Project = existing.Project ?? node.Project,
                LineNumber = existing.LineNumber ?? node.LineNumber,
                Layer = existing.Layer ?? node.Layer,
                Feature = existing.Feature ?? node.Feature,
            };
            _nodes[node.Id] = merged;
            return merged;
        }
        _nodes[node.Id] = node;
        return node;
    }

    /// <summary>Adds the given role tags to an existing node (creating a minimal Type node if absent).
    /// Convenience over reconstructing a <see cref="GraphNode"/> just to tag it.</summary>
    public void Tag(NodeId id, string title, params string[] tags)
        => AddNode(new GraphNode(id, title, id.Kind) { Tags = [.. tags] });

    /// <summary>Adds an edge if both endpoints exist and the (from, to, kind) triple is new.</summary>
    public bool AddEdge(GraphEdge edge)
    {
        if (!_nodes.ContainsKey(edge.From) || !_nodes.ContainsKey(edge.To)) return false;
        if (!_edgeKeys.Add((edge.From, edge.To, edge.Kind))) return false;
        if (!_out.TryGetValue(edge.From, out var list)) _out[edge.From] = list = [];
        list.Add(edge);
        return true;
    }

    /// <summary>L3.3 — Upgrades the <see cref="GraphEdge.Resolution"/> of an existing edge in-place when
    /// a semantic populator (Tier B) has verified the target. No-op if the edge doesn't exist or the
    /// new resolution is not an upgrade (Syntactic can go to Semantic; Semantic cannot go to Syntactic).</summary>
    public bool UpgradeEdge(NodeId from, NodeId to, EdgeKind kind, Resolution newResolution)
    {
        if (newResolution == Resolution.Syntactic) return false;
        if (!_out.TryGetValue(from, out var list)) return false;
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].To == to && list[i].Kind == kind)
            {
                if (list[i].Resolution == newResolution) return false;
                list[i] = list[i] with { Resolution = newResolution };
                return true;
            }
        }
        return false;
    }

    /// <summary>True if a node with the given id has been added.</summary>
    public bool HasNode(NodeId id) => _nodes.ContainsKey(id);

    /// <summary>Total nodes added so far.</summary>
    public int NodeCount => _nodes.Count;

    /// <summary>Total edges added so far.</summary>
    public int EdgeCount => _edgeKeys.Count;

    /// <summary>The node with the given id, or null. Lets passes inspect what's already there
    /// (e.g. restrict Calls edges to declared in-scope types, which carry a FilePath).</summary>
    public GraphNode? GetNode(NodeId id) => _nodes.TryGetValue(id, out var n) ? n : null;

    /// <summary>L4 — Sets the computed flows on the builder. Replaces any previously set flows.</summary>
    public void SetFlows(IEnumerable<Flow> flows) { _flows.Clear(); _flows.AddRange(flows); }

    /// <summary>T1.8 — Sets the authoritative entry inventory on the builder so the frozen graph carries
    /// the true <see cref="EntryPointKind"/> per entry. Replaces any previously set entries.</summary>
    public void SetEntries(ImmutableArray<EntryPoint> entries) { _entries = entries.IsDefault ? [] : entries; }

    /// <summary>T2.6 — Sets the event-wiring projection so the frozen graph carries the single
    /// publisher→event→consumer join every event surface renders from. Replaces any previously set wiring.</summary>
    public void SetEventWiring(ImmutableArray<EventWire> wiring) { _eventWiring = wiring.IsDefault ? [] : wiring; }

    /// <summary>Freezes the accumulated nodes/edges into an immutable <see cref="CodeGraph"/>.</summary>
    public CodeGraph Build(bool isSparse = false, int hubScopeNodeCount = 0, ImmutableArray<LayerViolation> layerViolations = default)
    {
        var outFrozen = _out.ToDictionary(kv => kv.Key, kv => kv.Value.ToImmutableArray());
        return new CodeGraph(_nodes, outFrozen) { IsSparseGraph = isSparse, HubScopeNodeCount = hubScopeNodeCount, LayerViolations = layerViolations, Flows = [.. _flows], Entries = _entries, EventWiring = _eventWiring };
    }
}
