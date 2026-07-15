using DevContext.Core.Graph.Seams;
using DevContext.Core.Graph2;
using DevContext.Core.Graph2.Seams;
using DevContext.Core.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Graph;

/// <summary>
/// Assembles the <see cref="CodeGraph"/> by JOINING existing detections + types + call edges. This is
/// the heart of the rebuild: nothing here re-detects — it connects islands the old model left separate
/// (a flat Types dict + a flat Detections bag + a separate CallGraph). Worked examples below (type
/// nodes, HTTP entries, MediatR handler joins) show the pattern; TODO-marked seams are the agent's
/// P1/P2 work. Per-seam recipes are in TRACE-ENGINE-DESIGN.md §2.2.
/// </summary>
public sealed class GraphBuilder
{
    private readonly ISymbolResolver _resolver;
    private readonly NoiseFilter _noise;

    // M1.6: event→project mappings collected from seam detectors, consumed by AddBusServiceLinks
    private Dictionary<string, HashSet<string>>? _eventPublishers;

    // P3: Entry-point builders — one per entry-point kind. Adding a new kind
    // (Blazor, gRPC, SignalR, etc.) means adding one class that implements
    // IEntryPointBuilder — no changes to GraphBuilder itself.
    private static readonly IEntryPointBuilder[] _entryBuilders =
    [
        new HttpEntryPointBuilder(),
        new WorkerEntryPointBuilder(),
        new DomainEventHandlerEntryBuilder(),
        new MessageConsumerEntryBuilder(),
        new DesktopEntryPointBuilder(),
        new GrpcEntryPointBuilder(),
        new SignalrEntryPointBuilder(),
        new FunctionsEntryPointBuilder(),
        new OrleansGrainEntryPointBuilder(),
        new GraphQlEntryPointBuilder(),
        new CliCommandEntryPointBuilder(),
    ];

    /// <summary>Creates a graph builder with a symbol resolver (syntactic now, semantic in P3) and a noise filter.</summary>
    public GraphBuilder(ISymbolResolver resolver, NoiseFilter noise)
    {
        _resolver = resolver;
        _noise = noise;
    }

    /// <summary>Builds the code graph and the entry-point inventory for one solution scope (design-doc R1).</summary>
    public (CodeGraph Graph, ImmutableArray<EntryPoint> Entries) Build(DiscoveryModel model, SolutionScope scope,
        IReadOnlyList<BodyFacts>? bodyFacts = null)
    {
        var g = new CodeGraphBuilder();
        var names = new NameResolver(model.Types.Values, f => scope.ProjectForFile(f)); // project-scoped (M1.4 / W5)
        _eventPublishers = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var archetype = ArchitectureArchetypeParser.Parse(model.Archetype);

        AddTypeNodes(g, model, scope, archetype);
        AddServiceNodes(g, model, scope);

        // ── P3: Entry-point builders (one per kind) ──────────────────────────
        // Open to extension — add a new builder for Blazor/gRPC/SignalR without
        // modifying GraphBuilder itself.
        var entries = ImmutableArray<EntryPoint>.Empty;
        foreach (var builder in _entryBuilders)
            entries = entries.AddRange(builder.Build(g, model, scope, names, _noise));

        // L4.5 — store EntryPointKind on each entry's graph node so projections
        // can derive the correct kind instead of falling back to PublicApi.
        foreach (var entry in entries)
            g.Tag(entry.Node, entry.Title, $"kind:{entry.Kind}");

        AddHandlerJoins(g, model, names, scope, _noise);            // worked example (Handles edge from MediatR detections)
        AddPipelineBehaviors(g, model, names, scope, _noise);       // B3: IPipelineBehavior → WrappedBy edges

        // ── P1 Map-facing seams ───────────────────────────────────────────
        AddEntityNodes(g, model, names, scope, _noise);             // B1: Entity nodes + aggregate tags
        AddEntityNavigationEdges(g, model, names, scope);        // A-F14: Entity→Entity relation edges
        AddEventConsumers(g, model, names, scope, _noise);          // B1: Event nodes + Consumes edges
        AddDiResolves(g, model, names, scope);              // B1: DI Resolves edges (interface → impl)

        // ── P2 Trace-facing seams ─────────────────────────────────────────
        // L2: structured seam detectors over BodyFacts (design §2.1) replace the old regex body-scan
        // sites. Edges anchor on the correct Member node by construction (BodyFacts.Member), so a
        // method-anchored trace shows only its own edges. Zero regex, zero re-parsing.
        AddSeamsFromDetectors(g, model, names, scope, bodyFacts);
        AddLambdaSeams(g, model, names, scope, bodyFacts);             // L2.4: dispatch edges for lambda entry-handlers
        AddCallEdges(g, model, names, bodyFacts);                      // C1: Calls edges from CallEdges (member→member)
        var (isSparse, hubCount) = AddHubScopeEdges(g, model, names, entries); // L3.4

        // ── M1.6-M1.8: Cross-service ServiceLink joins ────────────────────
        AddBusServiceLinks(g, model, names, scope, _noise);
        AddGrpcServiceLinks(g, model, names, scope, _noise);
        AddHttpServiceLinks(g, model, names, scope, _noise);

        var preGraph = g.Build(isSparse, hubCount);
        // Enrich (target/group-path/score) BEFORE computing flows: preGraph and the final graph share
        // identical nodes/edges (violations are metadata only), so this is safe here, and it means
        // graph.Flows carries the resolved Target — top_flows no longer reports it as null.
        var enrichedEntries = EnrichEntryScores(
            EnrichEntryGroupPaths(EnrichEntryTargets(preGraph, entries), names, scope),
            preGraph, scope);
        g.SetFlows(ComputeFlows(preGraph, enrichedEntries));
        g.SetEntries(enrichedEntries);   // T1.8 — projections read the true kind off this record, not node tags
        var violations = DetectLayerViolations(preGraph, archetype);
        var graph = g.Build(isSparse, hubCount, violations);
        return (graph, enrichedEntries);
    }

    /// <summary>L4.1 — Compute spine-first flows for all entries. Each flow is the primary dispatch
    /// path (entry → send → handler → ...) with touches/emits collected only from spine members
    /// (fixes audit E5: no EntityRelation reachability).</summary>
    private static ImmutableArray<Flow> ComputeFlows(CodeGraph graph, ImmutableArray<EntryPoint> entries,
        int maxSpineDepth = 24)
    {
        if (entries.IsDefaultOrEmpty) return [];

        var bridgeMembers = BuildBridgeIndex(graph);
        var flows = ImmutableArray.CreateBuilder<Flow>(entries.Length);

        foreach (var entry in entries)
        {
            var entryNode = graph.Node(entry.Node);
            if (entryNode is null) continue;

            var visited = new HashSet<NodeId>();
            var steps = ImmutableArray.CreateBuilder<FlowStep>();
            var touchedIds = new HashSet<NodeId>();
            var emittedIds = new HashSet<NodeId>();
            var hops = ImmutableArray.CreateBuilder<ServiceHop>();

            var entryTitle = entryNode.Kind == NodeKind.EntryPoint ? entryNode.Title : entry.Title;
            steps.Add(new FlowStep(entry.Node, null, Resolution.Join, entry.Provenance) { Title = entryTitle });
            visited.Add(entry.Node);

            // Collect touches/emits from the entry node's own out-edges
            CollectSpineTouchesAndEmits(graph, entry.Node, bridgeMembers, touchedIds, emittedIds);

            var currentId = entry.Node;
            var isTruncated = true;
            for (var d = 0; d < maxSpineDepth; d++)
            {
                var bestEdge = SelectBestSpineEdge(graph, currentId, bridgeMembers, visited);
                if (bestEdge is null) { isTruncated = false; break; }

                visited.Add(bestEdge.To);

                // Record ServiceHop when crossing a ServiceLink
                if (bestEdge.Kind == EdgeKind.ServiceLink)
                {
                    var fromNode = graph.Node(bestEdge.From);
                    var toNode = graph.Node(bestEdge.To);
                    hops.Add(new ServiceHop(
                        fromNode?.Project,
                        toNode?.Project,
                        bestEdge.Tags.IsDefaultOrEmpty ? null : string.Join(",", bestEdge.Tags),
                        bestEdge.Provenance));
                }

                var targetNode = graph.Node(bestEdge.To);
                steps.Add(new FlowStep(bestEdge.To, bestEdge.Kind, bestEdge.Resolution, bestEdge.Provenance)
                {
                    Title = targetNode?.Title,
                });

                CollectSpineTouchesAndEmits(graph, bestEdge.To, bridgeMembers, touchedIds, emittedIds);
                currentId = bestEdge.To;
            }

            var flowId = entry.Node.Key;
            if (entry.Kind == EntryPointKind.HttpEndpoint && entry.Route is { } r && entry.HttpMethod is { } m)
                flowId = $"{m} {r}";

            flows.Add(new Flow(flowId, entry, steps.ToImmutable())
            {
                Touches = [.. touchedIds],
                Emits = [.. emittedIds],
                Hops = hops.ToImmutable(),
                IsTruncated = isTruncated,
            });
        }

        return flows.ToImmutable();
    }

    private static void CollectSpineTouchesAndEmits(CodeGraph graph, NodeId nodeId,
        Dictionary<NodeId, List<NodeId>> bridgeMembers,
        HashSet<NodeId> touchedIds, HashSet<NodeId> emittedIds)
    {
        var ids = new List<NodeId> { nodeId };
        if (nodeId.Kind == NodeKind.Type && bridgeMembers.TryGetValue(nodeId, out var members))
            ids.AddRange(members);

        foreach (var id in ids)
        {
            foreach (var edge in graph.OutEdges(id))
            {
                if (edge.Kind == EdgeKind.ReadsWrites)
                    touchedIds.Add(edge.To);
                else if (edge.Kind == EdgeKind.Raises)
                    emittedIds.Add(edge.To);
            }
        }
    }

    private static GraphEdge? SelectBestSpineEdge(CodeGraph graph, NodeId nodeId,
        Dictionary<NodeId, List<NodeId>> bridgeMembers, HashSet<NodeId> visited)
    {
        GraphEdge? best = null;
        var bestPriority = int.MaxValue;
        var bestConfidence = float.MinValue;

        var ids = new List<NodeId> { nodeId };
        if (nodeId.Kind == NodeKind.Type && bridgeMembers.TryGetValue(nodeId, out var members))
            ids.AddRange(members);

        foreach (var id in ids)
        {
            foreach (var edge in graph.OutEdges(id))
            {
                if (visited.Contains(edge.To)) continue;
                if (edge.Kind is EdgeKind.WrappedBy or EdgeKind.EntityRelation or EdgeKind.DependsOn or EdgeKind.Exposes)
                    continue;

                var p = SpineEdgePriority(edge.Kind);
                if (p < bestPriority || (p == bestPriority && edge.Confidence > bestConfidence))
                {
                    bestPriority = p;
                    bestConfidence = edge.Confidence;
                    best = edge;
                }
            }
        }

        // G5: Type->Service bridge — when at a Type node with known Project, consider
        // ServiceLink edges from the containing Service node so the spine can follow
        // cross-service hops. This closes the L2.4 gap where the checkout flow spine
        // stopped at event Type nodes because they had no edge to their Service node's
        // ServiceLinks. Safe by construction: if NodeId.ForService returns a node that
        // doesn't exist, the foreach is a no-op.
        if (nodeId.Kind == NodeKind.Type)
        {
            var typeNode = graph.Node(nodeId);
            if (typeNode?.Project is { Length: > 0 })
            {
                var serviceId = NodeId.ForService(typeNode.Project);
                foreach (var edge in graph.OutEdges(serviceId, EdgeKind.ServiceLink))
                {
                    if (visited.Contains(edge.To)) continue;
                    var p = SpineEdgePriority(edge.Kind);
                    if (p < bestPriority || (p == bestPriority && edge.Confidence > bestConfidence))
                    {
                        bestPriority = p;
                        bestConfidence = edge.Confidence;
                        best = edge;
                    }
                }
            }
        }

        if (best is not null && IsFrameworkLeaf(graph.Node(best.To)))
            return null;

        return best;
    }

    private static int SpineEdgePriority(EdgeKind kind) => kind switch
    {
        EdgeKind.Sends => 0,
        EdgeKind.Handles => 1,
        EdgeKind.ServiceLink => 2,
        EdgeKind.Raises => 3,
        EdgeKind.Consumes => 4,
        EdgeKind.ReadsWrites => 5,
        EdgeKind.Resolves => 6,
        _ => 7,
    };

    private static bool IsFrameworkLeaf(GraphNode? node)
    {
        if (node is null) return true;
        var title = node.Title;
        return title.StartsWith("Microsoft.", StringComparison.Ordinal)
            || title.StartsWith("System.", StringComparison.Ordinal)
            || title == "DbContext"
            || title is "ILogger" or "IMediator" or "ISender" or "IPublisher"
            || (title.Contains("Mediator", StringComparison.Ordinal) && title != "MediatorExtension");
    }

    private static Dictionary<NodeId, List<NodeId>> BuildBridgeIndex(CodeGraph graph)
    {
        var map = new Dictionary<NodeId, List<NodeId>>();
        foreach (var node in graph.Nodes)
        {
            if (node.Id.Kind != NodeKind.Member) continue;
            var dot = node.Id.Key.LastIndexOf('.');
            if (dot <= 0) continue;
            var method = node.Id.Key[(dot + 1)..];
            var typeId = NodeId.ForType(node.Id.Key[..dot]);

            if (method is "Handle" or "HandleAsync" or "Consume" or "ConsumeAsync"
                || method.StartsWith("Execute", StringComparison.Ordinal)
                || method.StartsWith("Invoke", StringComparison.Ordinal))
            {
                if (!map.TryGetValue(typeId, out var list)) map[typeId] = list = [];
                list.Add(node.Id);
            }
        }
        return map;
    }

    /// <summary>After the graph is assembled, resolve each entry's dispatch target (the command it
    /// sends or the handler it invokes) so the Map and the desktop picker can show "route → Target".
    /// Uses the entry's <see cref="EntryPoint.HandlerNode"/> (set during graph construction) to find
    /// the connected Type/Member node and its Sends edges.</summary>
    private static ImmutableArray<EntryPoint> EnrichEntryTargets(CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        if (entries.IsDefaultOrEmpty) return entries;
        var b = ImmutableArray.CreateBuilder<EntryPoint>(entries.Length);
        foreach (var e in entries)
        {
            var target = ResolveEntryTarget(graph, e)
                ?? ResolveOwningTypeFallback(graph, e);
            b.Add(e with { Target = target });
        }
        return b.ToImmutable();
    }

    /// <summary>When <see cref="ResolveEntryTarget"/> finds no dispatch target (e.g. a view-returning
    /// controller action with no service call and no MediatR send), fall back to the owning controller
    /// type — honest (it's the declaring type) and more useful than a blank drill-in hint (W8). E6: a
    /// minimal-API lambda with real work inside but no single named collaborator (every call was a
    /// data-access noise verb, or several tied on out-degree) says so — "inline (N calls)" — rather than
    /// naming the whole registration type, which the reader would mistake for a real handler.</summary>
    private static string? ResolveOwningTypeFallback(CodeGraph graph, EntryPoint entry)
    {
        if (entry.HandlerNode is not { } hn) return null;
        var handler = graph.Node(hn);
        if (handler is null) return null;

        if (handler.Kind == NodeKind.Type)
            return handler.Title;

        if (handler.Kind == NodeKind.Member)
        {
            if (handler.Title.StartsWith("<lambda>", StringComparison.Ordinal))
            {
                var callCount = graph.OutEdges(handler.Id, EdgeKind.Calls).Length;
                if (callCount > 0) return $"inline ({callCount} call{(callCount == 1 ? "" : "s")})";
            }

            var typeKey = ExtractTypeKey(handler.Id.Key);
            return graph.Node(NodeId.ForType(typeKey))?.Title;
        }

        return null;
    }

    /// <summary>Resolves an entry's primary target by following the entry's Calls edge to the
    /// target node, then checking that node's Sends edges — same traversal the TraceBuilder uses.</summary>
    private static string? ResolveEntryTarget(CodeGraph graph, EntryPoint entry)
    {
        if (entry.Node.Key.Contains("<dynamic>", StringComparison.Ordinal)) return null;

        foreach (var call in graph.OutEdges(entry.Node, EdgeKind.Calls))
        {
            var node = graph.Node(call.To);
            if (node is null) continue;

            switch (node.Kind)
            {
                case NodeKind.Member:
                    // 1. CQRS dispatch (MediatR Send/Publish) — try FIRST so eShop entry→target is unchanged.
                    var msends = graph.OutEdges(node.Id, EdgeKind.Sends)
                        .Select(s => s.To).Distinct().ToList();
                    if (msends.Count == 1) return graph.Node(msends[0])?.Title;
                    if (msends.Count > 1 && entry.Title is { } mroute)
                        return MatchRouteToSend(mroute, msends, graph);
                    // 2. Primary service call — a handler that dispatches no request (a plain controller
                    //    action) resolves to the dominant in-scope service it calls. The action member's
                    //    own Calls edges are precise post member-origin (Iteration 1), so this takes
                    //    controllers from 0 → target without guessing via the whole class.
                    return ResolvePrimaryCall(graph, node);
                case NodeKind.Type:
                    var sends = graph.OutEdges(node.Id, EdgeKind.Sends)
                        .Select(s => s.To).Distinct().ToList();
                    if (sends.Count == 1)
                        return graph.Node(sends[0])?.Title;
                    if (sends.Count > 1 && entry.Title is { } route)
                        return MatchRouteToSend(route, sends, graph);
                    return null;
            }
        }
        return null;
    }

    /// <summary>Resolves an entry whose handler dispatches no MediatR request (e.g. a plain controller
    /// action or a minimal-API lambda) to the primary service it calls: the dominant in-scope callee of
    /// the action <b>member</b>. Prefers a DI-resolved <c>service</c>-tagged callee, else the in-scope,
    /// non-self, non-framework callee with the most outgoing calls of its own (E6: a real collaborator
    /// keeps working, a data-access leaf doesn't). Returns its title (member form, e.g.
    /// "ProductService.GetByIdAsync"), or null when the action calls nothing meaningful — honest, never
    /// guessed via the whole class (member-origin made the action's own Calls edges precise, so the old
    /// <c>ResolveViaParentType</c> whole-type crutch is retired).</summary>
    private static string? ResolvePrimaryCall(CodeGraph graph, GraphNode member)
    {
        var ownerTypeKey = ExtractTypeKey(member.Id.Key);
        GraphNode? bestFallback = null;
        GraphNode? bestFallbackType = null;
        var bestOutDegree = -1;
        foreach (var call in graph.OutEdges(member.Id, EdgeKind.Calls))
        {
            var callee = graph.Node(call.To);
            if (callee is null) continue;

            var calleeTypeKey = callee.Kind == NodeKind.Member ? ExtractTypeKey(callee.Id.Key) : callee.Id.Key;
            // Skip self-calls (a controller action calling ControllerBase helpers like Ok()/NotFound(),
            // which the syntactic resolver attributes to `this`).
            if (string.Equals(calleeTypeKey, ownerTypeKey, StringComparison.Ordinal)) continue;

            // In-scope only: the callee's owning Type must be a declared type we own (non-null FilePath),
            // which excludes framework leaves.
            var calleeType = graph.Node(NodeId.ForType(calleeTypeKey));
            if (calleeType?.FilePath is null) continue;

            // E6: a raw data-access call is an implementation detail, not the endpoint's meaning — skip a
            // callee on a DataStore-tagged type (a DbContext) and any call whose OWN method name is a bare
            // EF/LINQ verb (Where/FindAsync/SaveChangesAsync/...), even when the syntactic resolver
            // attributed it to a wrapper type (e.g. an `[AsParameters]` services struct) rather than the
            // DbContext itself.
            var calleeMemberName = callee.Kind == NodeKind.Member ? ExtractMemberName(callee.Id.Key) : null;
            if (calleeType.Tags.Contains(RoleTags.DataStore)
                || IsDataAccessNoiseMethod(calleeMemberName)
                || IsObjectNoiseMethod(calleeMemberName))
                continue;

            // Prefer a DI-resolved service (the action's real collaborator) outright; else remember the
            // meaningful callee with the highest out-degree of its own — a real handler keeps working,
            // a leaf call doesn't.
            if (calleeType.Tags.Contains(RoleTags.Service))
                return TargetTitle(callee, calleeType, calleeMemberName);

            var outDegree = graph.OutEdges(callee.Id, EdgeKind.Calls).Length;
            if (outDegree > bestOutDegree)
            {
                bestOutDegree = outDegree;
                bestFallback = callee;
                bestFallbackType = calleeType;
            }
        }
        return bestFallback is null
            ? null
            : TargetTitle(bestFallback, bestFallbackType!,
                bestFallback.Kind == NodeKind.Member ? ExtractMemberName(bestFallback.Id.Key) : null);
    }

    /// <summary>An entry target is always rendered <c>Type.Method</c> for a member callee. The semantic
    /// body-scan seams sometimes create a member node with a BARE method-name title (e.g. DntSite's
    /// auto-registered <c>FeedsService</c>, whose target read as an ownerless "GetNewsAsync"); its NodeId
    /// still encodes the owning type, so reconstruct the qualified name from the resolved type node so
    /// "FeedsService.GetNewsAsync" survives (T1.3). A callee whose title is already qualified — or a Type
    /// callee — keeps its own title.</summary>
    private static string TargetTitle(GraphNode callee, GraphNode calleeType, string? memberName)
        => callee.Kind == NodeKind.Member
            && memberName is { Length: > 0 }
            && !callee.Title.Contains('.', StringComparison.Ordinal)
            ? $"{calleeType.Title}.{memberName}"
            : callee.Title;

    /// <summary>"TypeFqn.MethodName" → "MethodName" (the inverse of <see cref="ExtractTypeKey"/>).</summary>
    private static string ExtractMemberName(string memberKey)
    {
        var dot = memberKey.LastIndexOf('.');
        return dot >= 0 ? memberKey[(dot + 1)..] : memberKey;
    }

    /// <summary>E6: bare EF Core / LINQ verbs — never a meaningful entry target on their own, whichever
    /// type the syntactic resolver happened to attribute the call to.</summary>
    private static readonly HashSet<string> _dataAccessNoiseMethods = new(StringComparer.Ordinal)
    {
        "Where", "Select", "SelectMany", "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending",
        "Include", "ThenInclude", "Skip", "Take", "GroupBy", "Distinct",
        "Any", "AnyAsync", "All", "Count", "CountAsync", "Sum", "SumAsync", "Average",
        "First", "FirstAsync", "FirstOrDefault", "FirstOrDefaultAsync",
        "Single", "SingleAsync", "SingleOrDefault", "SingleOrDefaultAsync",
        "ToList", "ToListAsync", "ToArray", "ToArrayAsync", "ToDictionary", "ToDictionaryAsync",
        "Find", "FindAsync", "Add", "AddAsync", "AddRange", "AddRangeAsync",
        "Remove", "RemoveRange", "Update", "UpdateRange", "SaveChanges", "SaveChangesAsync",
        "Attach", "AsNoTracking", "AsQueryable", "AsEnumerable",
    };

    private static bool IsDataAccessNoiseMethod(string? methodName)
        => methodName is not null && _dataAccessNoiseMethods.Contains(methodName);

    /// <summary>System.Object/lifetime plumbing — calling <c>service.ToString()</c> must never make
    /// that service the entry's target (seen live: "GET /api/ctrader/listen → CTraderListenService.ToString").</summary>
    private static bool IsObjectNoiseMethod(string? methodName)
        => methodName is "ToString" or "GetHashCode" or "Equals" or "GetType" or "Dispose" or "DisposeAsync";

    /// <summary>"TypeFqn.MethodName" → "TypeFqn" (strips the trailing member segment from a Member key).</summary>
    private static string ExtractTypeKey(string memberKey)
    {
        var dot = memberKey.LastIndexOf('.');
        return dot > 0 ? memberKey[..dot] : memberKey;
    }

    /// <summary>When a registration type dispatches many commands (minimal APIs), match an entry's
    /// route to the most likely request by extracting the last significant route segment and finding
    /// the Send target whose request name contains it.</summary>
    private static string? MatchRouteToSend(string route, List<NodeId> sendTargets, CodeGraph graph)
    {
        // Extract the last significant segment: "POST /api/orders/" → "orders"
        var segment = route.TrimEnd('/');
        var lastSlash = segment.LastIndexOf('/');
        if (lastSlash >= 0)
            segment = segment[(lastSlash + 1)..];
        // Strip {params}: "orders/{orderId:int}" → "orders"
        var brace = segment.IndexOf('{');
        if (brace > 0) segment = segment[..brace];
        if (segment.Length < 2) return null;

        // Also try singular form (routes are often plural, type names singular)
        var singular = segment.EndsWith("s", StringComparison.OrdinalIgnoreCase)
            ? segment[..^1] : null;
        // HTTP-verb prefix hints: POST→Create, GET→Get/List, PUT→Update, DELETE→Delete
        var verb = route.AsSpan().TrimStart();
        var space = verb.IndexOf(' ');
        var httpVerb = space > 0 ? verb[..space].ToString() : "";

        string? best = null;
        foreach (var targetId in sendTargets)
        {
            var name = graph.Node(targetId)?.Title;
            if (name is null) continue;
            if (!name.Contains(segment, StringComparison.OrdinalIgnoreCase)
                && (singular is null || !name.Contains(singular, StringComparison.OrdinalIgnoreCase)))
                continue;

            // Prefer targets whose verb-derived prefix matches
            if (MatchesVerbPrefix(name, httpVerb))
                return name;
            best ??= name;
        }
        return best;
    }

    private static bool MatchesVerbPrefix(string name, string httpVerb) => httpVerb switch
    {
        "POST" => name.StartsWith("Create", StringComparison.OrdinalIgnoreCase)
               || name.StartsWith("Add", StringComparison.OrdinalIgnoreCase),
        "GET" => name.StartsWith("Get", StringComparison.OrdinalIgnoreCase)
              || name.StartsWith("List", StringComparison.OrdinalIgnoreCase)
              || name.StartsWith("Find", StringComparison.OrdinalIgnoreCase),
        "PUT" => name.StartsWith("Update", StringComparison.OrdinalIgnoreCase),
        "DELETE" => name.StartsWith("Delete", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Remove", StringComparison.OrdinalIgnoreCase),
        "PATCH" => name.StartsWith("Update", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Patch", StringComparison.OrdinalIgnoreCase),
        _ => false,
    };

    /// <summary>L3.6 — Derives a project-relative GroupPath for each entry. Uses the handler type's
    /// namespace (resolved via NameResolver) and strips the project's typical root-namespace prefix
    /// to produce a grouping key like "Controllers/Orders" or "Services/Ordering".</summary>
    private static ImmutableArray<EntryPoint> EnrichEntryGroupPaths(
        ImmutableArray<EntryPoint> entries, NameResolver names, SolutionScope scope)
    {
        if (entries.IsDefaultOrEmpty) return entries;
        var b = ImmutableArray.CreateBuilder<EntryPoint>(entries.Length);
        foreach (var e in entries)
        {
            var gp = DeriveGroupPath(e, names, scope);
            b.Add(e with { GroupPath = gp });
        }
        return b.ToImmutable();
    }

    private static string? DeriveGroupPath(EntryPoint entry, NameResolver names, SolutionScope scope)
    {
        // 1. Resolve the handler type's FQN (via HandlerNode or by parsing Provenance)
        string? ns = null;
        string? project = null;

        // Extract project name from provenance (file:line string)
        if (entry.Provenance is { } provenance)
        {
            var colon = provenance.LastIndexOf(':');
            var filePath = colon > 0 ? provenance[..colon] : provenance;
            project = scope.ProjectForFile(filePath);
        }

        // T1.6 — HTTP feature areas come from the ROUTE first, not the handler namespace. Grouping every
        // endpoint under its shared "…Api" namespace collapsed 128 shamshir endpoints into one useless
        // "Api (128 entries)" module row; the route's first meaningful segment is the real feature
        // (/api/addons/* → addons, /api/orders/* → orders). Namespace/folder still groups non-HTTP entries.
        if (entry.Kind == EntryPointKind.HttpEndpoint && entry.Route is { } route
            && HttpRouteGroupPath(route) is { } routeGroup)
            return routeGroup;

        if (entry.HandlerNode is { } hn)
        {
            var fqn = ExtractTypeKey(hn.Key);
            ns = names.GetNamespace(fqn);
        }

        if (ns is null) return project;

        // Derive GroupPath from namespace, stripping project-root prefix
        return NamespaceGroupPath(ns, project);
    }

    /// <summary>Derives a GroupPath from the last 1-2 meaningful namespace segments, stripping
    /// common project/root prefixes (e.g. "MyApp.Api.Controllers.Orders" → "Controllers/Orders"
    /// when project is "MyApp.Api").</summary>
    private static string? NamespaceGroupPath(string ns, string? project)
    {
        var parts = ns.Split('.');
        if (parts.Length <= 1) return ns;

        // Find where the namespace diverges from the project (typically namespaces mirror projects)
        var start = 0;
        if (project is not null)
        {
            var projParts = project.Split('.');
            for (var i = 0; i < Math.Min(parts.Length, projParts.Length); i++)
            {
                if (string.Equals(parts[i], projParts[i], StringComparison.OrdinalIgnoreCase))
                    start = i + 1;
                else break;
            }
        }

        // Take the remaining meaningful segments, skip "Controllers"/"Endpoints" as redundant
        var remaining = parts[start..];
        if (remaining.Length == 0) return project;
        if (remaining.Length == 1) return remaining[0];

        // Skip the ubiquitous first segment if it's a well-known structural layer marker
        if (remaining.Length >= 2
            && (remaining[0] == "Controllers" || remaining[0] == "Endpoints"
                || remaining[0] == "Handlers" || remaining[0] == "Services"
                || remaining[0] == "Consumers" || remaining[0] == "Hubs"))
            return string.Join("/", remaining[1..]);

        return string.Join("/", remaining);
    }

    /// <summary>T1.6 — Derives the FEATURE-AREA GroupPath from an HTTP route: the first meaningful path
    /// segment, skipping the "api" prefix, version segments (v1, v2.0), and route parameters
    /// (e.g. "GET /api/orders/{id}" → "orders", "POST /api/v2/addons" → "addons"). Returns null for a
    /// route with no meaningful segment (e.g. "/") so the caller can fall back to namespace/project.</summary>
    private static string? HttpRouteGroupPath(string route)
    {
        var space = route.IndexOf(' ');
        var path = space > 0 ? route[(space + 1)..] : route;
        foreach (var seg in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (seg.StartsWith('{')) continue;                                   // route parameter
            if (seg.Equals("api", StringComparison.OrdinalIgnoreCase)) continue; // ubiquitous api prefix
            if (IsRouteVersionSegment(seg)) continue;                            // v1, v2, v2.0
            return seg.ToLowerInvariant();                                       // first meaningful segment = feature
        }
        return null;
    }

    /// <summary>True for an API-version route segment like "v1" or "v2.0" (letter v + digit).</summary>
    private static bool IsRouteVersionSegment(string s)
        => s.Length >= 2 && s[0] is 'v' or 'V' && char.IsDigit(s[1]);

    /// <summary>L3.2 — Computes graph-aware scores for each entry: BFS from the entry's node outward
    /// through Calls/Sends edges to count reach, seam richness, entity touches, and cross-project depth.
    /// Produces a composite 0..1 score for ranking.</summary>
    private static ImmutableArray<EntryPoint> EnrichEntryScores(
        ImmutableArray<EntryPoint> entries, CodeGraph graph, SolutionScope scope)
    {
        if (entries.IsDefaultOrEmpty) return entries;

        var maxReach = 0d;
        var maxSeam = 0d;
        var maxEntity = 0d;
        var (reach, seam, ent, xProjects) = ScoreEntries(entries, graph, scope);

        if (reach.Length > 0) { maxReach = reach.Max(); maxSeam = Math.Max(maxSeam, seam.Max()); maxEntity = Math.Max(maxEntity, ent.Max()); }

        var b = ImmutableArray.CreateBuilder<EntryPoint>(entries.Length);
        for (var i = 0; i < entries.Length; i++)
        {
            var normReach = maxReach > 0 ? reach[i] / maxReach : 0;
            var normSeam = maxSeam > 0 ? seam[i] / maxSeam : 0;
            var normEntity = maxEntity > 0 ? ent[i] / maxEntity : 0;
            var normProj = reach.Length > 0 ? xProjects[i] / Math.Max(xProjects.Max(), 1) : 0;

            var score = normReach * 0.4 + normSeam * 0.3 + normEntity * 0.2 + normProj * 0.1;
            b.Add(entries[i] with
            {
                Score = Math.Round(score, 3),
                Reach = reach[i],
                SeamRichness = seam[i],
                EntityTouches = ent[i],
                CrossProjects = xProjects[i],
            });
        }
        return b.ToImmutable();
    }

    private static (int[] Reach, int[] Seam, int[] Entity, int[] XProj) ScoreEntries(
        ImmutableArray<EntryPoint> entries, CodeGraph graph, SolutionScope scope)
    {
        var n = entries.Length;
        var reach = new int[n];
        var seam = new int[n];
        var entity = new int[n];
        var xProj = new int[n];

        for (var i = 0; i < n; i++)
        {
            var (r, s, e, x) = BfsEntryScore(graph, entries[i], scope);
            reach[i] = r;
            seam[i] = s;
            entity[i] = e;
            xProj[i] = x;
        }
        return (reach, seam, entity, xProj);
    }

    private static (int Reach, int Seam, int Entity, int XProj) BfsEntryScore(CodeGraph graph, EntryPoint entry, SolutionScope scope)
    {
        var visited = new HashSet<NodeId>();
        var queue = new Queue<(NodeId, int)>();
        queue.Enqueue((entry.Node, 0));
        visited.Add(entry.Node);

        var reach = 0;
        var seam = 0;
        var entity = 0;
        var projects = new HashSet<string>();

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();
            if (depth > 6) continue;
            if (current != entry.Node) reach++;

            foreach (var edge in graph.OutEdges(current))
            {
                // Track seam richness
                if (edge.Kind is EdgeKind.Sends or EdgeKind.Raises or EdgeKind.Consumes)
                    seam++;
                if (edge.Kind == EdgeKind.ReadsWrites)
                {
                    var target = graph.Node(edge.To);
                    if (target is not null && (target.Tags.Contains(RoleTags.Entity)
                        || target.Tags.Contains(RoleTags.Aggregate)))
                        entity++;
                }
                // Track cross-project: resolve the owning project from the target node's file path.
                var targetNode = graph.Node(edge.To);
                if (targetNode?.FilePath is { } fp)
                {
                    var proj = scope.ProjectForFile(fp) ?? targetNode.Project ?? Path.GetFileNameWithoutExtension(fp);
                    if (proj is not null) projects.Add(proj);
                }

                if (visited.Add(edge.To) && depth < 6)
                    queue.Enqueue((edge.To, depth + 1));
            }
        }

        return (reach, seam, entity, projects.Count);
    }

    /// <summary>WORKED EXAMPLE — every in-scope production type becomes a TypeNode (noise filtered structurally).</summary>
    private void AddTypeNodes(CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope, ArchitectureArchetype archetype)
    {
        foreach (var type in model.Types.Values)
        {
            if (!_noise.IsProductionCode(type) || !scope.Contains(type.FilePath)) continue;
            var feature = DeriveFeature(type, model);
            var project = scope.ProjectForFile(type.FilePath);
            g.AddNode(new GraphNode(NodeId.ForType(type.Id), type.Name, NodeKind.Type)
            {
                FilePath = type.FilePath,
                SourceBody = type.SourceBody,
                LineNumber = type.StartLine,
                Layer = type.Layer != ArchitectureLayer.Unknown ? type.Layer.ToLabel(archetype) : null,
                Feature = feature,
                Project = project,
            });
        }
    }

    private static void AddServiceNodes(CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope)
    {
        var runnable = ServiceBoundaryInference.RunnableProjects(scope);
        foreach (var proj in runnable)
        {
            g.AddNode(new GraphNode(NodeId.ForService(proj.Name), proj.Name, NodeKind.Service)
            {
                Project = proj.Name,
                Tags = [RoleTags.Runnable],
            });
        }
    }

    /// <summary>D9 — derives the feature label from namespace, stripping project and known layer prefixes.
    /// Returns the first meaningful segment after removing project-root namespace segments and layer-ish segments.</summary>
    private static string? DeriveFeature(TypeDiscovery type, DiscoveryModel model)
    {
        var ns = type.Namespace;
        if (string.IsNullOrWhiteSpace(ns)) return null;

        if (type.FilePath is not { } fp) return CarveFeature(ns);

        var matchedProject = model.Projects.FirstOrDefault(p =>
            p.FilePath is { } pp && fp.StartsWith(Path.GetDirectoryName(pp) ?? "", StringComparison.OrdinalIgnoreCase));
        if (matchedProject is not null)
        {
            var prefix = matchedProject.Name.Replace("-", "").Replace("_", "");
            if (ns.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                ns = ns[prefix.Length..].TrimStart('.');
        }
        if (ns.StartsWith("Services.", StringComparison.OrdinalIgnoreCase))
            ns = ns["Services.".Length..];

        return CarveFeature(ns);
    }

    private static readonly HashSet<string> LayerSegments = new(StringComparer.OrdinalIgnoreCase)
    {
        "Api", "Controllers", "Endpoints", "Presentation", "UI",
        "Application", "UseCases", "Services", "Handlers", "Behaviors", "Validators",
        "Domain", "Entities", "Aggregates", "ValueObjects", "Events",
        "Infrastructure", "Persistence", "Data", "Repositories", "External",
        "Contracts", "Dto", "Messages", "Requests", "Responses",
        "Extensions", "Filters", "Middleware", "Mapping", "Configuration",
        "Pages", "Components", "Views", "ViewModels", "Platform", "Core", "Internals",
    };

    private static string? CarveFeature(string ns)
    {
        var segments = ns.Split('.');
        var meaningful = segments
            .Where(s => !string.IsNullOrWhiteSpace(s) && !LayerSegments.Contains(s))
            .ToArray();
        return meaningful.Length > 0 ? meaningful[0] : null;
    }


    /// <summary>Creates Handles edges from MediatRHandlerDetection detections AND from
    /// TypeDiscovery objects that transitively implement known handler interfaces (M1.1 closure).
    /// Transitive detection catches classes that inherit from a handler base class (not common
    /// but required for the "match handlers transitively" golden).</summary>
    private static void AddHandlerJoins(CodeGraphBuilder g, DiscoveryModel model, NameResolver names, SolutionScope scope, NoiseFilter noise)
    {
        foreach (var h in model.Detections.OfType<MediatRHandlerDetection>())
        {
            if (!scope.Contains(h.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(h.SourceFile)) continue;
            EmitHandlerJoin(g, model, names, h.RequestType, h.HandlerType, h.Kind, h.SourceFile, h.LineNumber);
        }

        // M1.1 transitive: scan model types for classes whose BaseTypes transitively
        // implement handler interfaces but weren't picked up by the syntax-level extractor.
        var handlerByShortName = new Dictionary<string, List<TypeDiscovery>>(StringComparer.Ordinal);
        foreach (var t in model.Types.Values)
        {
            var sn = StripGenerics(t.Name);
            if (!handlerByShortName.TryGetValue(sn, out var list))
                handlerByShortName[sn] = list = [];
            list.Add(t);
        }

        var knownHandlerTypes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var h in model.Detections.OfType<MediatRHandlerDetection>())
            knownHandlerTypes.Add(names.Resolve(h.HandlerType, h.SourceFile));

        foreach (var type in model.Types.Values)
        {
            if (type.Kind != Models.TypeKind.Class) continue;
            if (!scope.Contains(type.FilePath)) continue;
            if (!noise.IsProductionCode(type)) continue;
            if (knownHandlerTypes.Contains(type.Id)) continue;

            // Check if any BaseType transitively reaches a known handler type
            var reached = FindHandlerBaseType(type, handlerByShortName, knownHandlerTypes, []);
            if (reached is null) continue;

            // Find the most-specific handler interface in the chain
            var handlerIfaces = reached.ImplementedInterfaces
                .Where(i => IsHandlerInterface(i, handlerByShortName, []))
                .ToArray();
            if (handlerIfaces.Length == 0) continue;

            var handlerInterface = handlerIfaces[0];
            var args = ExtractGenericArgs(handlerInterface);
            if (args.Length < 1) continue;

            var requestType = args[0];
            var responseType = args.Length >= 2 ? args[1] : "Unit";
            var kind = handlerInterface.Contains("Notification", StringComparison.Ordinal)
                ? MediatRKind.Notification
                : MediatRKind.Command;

            EmitHandlerJoin(g, model, names, requestType, type.Name, kind, type.FilePath, type.StartLine ?? 1);
        }
    }

    private static void EmitHandlerJoin(CodeGraphBuilder g, DiscoveryModel model, NameResolver names,
        string requestType, string handlerShortName, MediatRKind kind, string sourceFile, int lineNumber)
    {
        var requestId = NodeId.ForType(names.Resolve(requestType, sourceFile));
        var handlerId = NodeId.ForType(names.Resolve(handlerShortName, sourceFile));

        g.AddNode(new GraphNode(requestId, requestType, NodeKind.Type)
        {
            Tags = [kind.ToString().ToLowerInvariant()],
            Layer = "Application",
        });
        g.AddNode(new GraphNode(handlerId, handlerShortName, NodeKind.Type)
        {
            FilePath = sourceFile,
            Tags = [RoleTags.Handler],
            Layer = "Application",
            SourceBody = model.Types.Values
                .FirstOrDefault(t => t.Id == names.Resolve(handlerShortName, sourceFile))
                ?.SourceBody,
        });
        g.AddEdge(new GraphEdge(requestId, handlerId, EdgeKind.Handles)
        {
            Provenance = $"{sourceFile}:{lineNumber}",
            Resolution = Resolution.Join,
        });
    }

    private static TypeDiscovery? FindHandlerBaseType(TypeDiscovery type,
        Dictionary<string, List<TypeDiscovery>> byShortName,
        HashSet<string> knownHandlers,
        HashSet<string> visited)
    {
        if (knownHandlers.Contains(type.Id)) return type;
        foreach (var bt in type.BaseTypes)
        {
            var stripped = StripGenerics(bt);
            if (!visited.Add(stripped)) continue;
            if (byShortName.TryGetValue(stripped, out var bases))
            {
                foreach (var baseType in bases)
                {
                    var result = FindHandlerBaseType(baseType, byShortName, knownHandlers, visited);
                    if (result is not null) return result;
                }
            }
        }
        return null;
    }

    private static bool IsHandlerInterface(string ifaceName,
        Dictionary<string, List<TypeDiscovery>> byShortName,
        HashSet<string> visited)
    {
        var stripped = StripGenerics(ifaceName);
        if (stripped is "IRequestHandler" or "INotificationHandler" or "IStreamRequestHandler")
            return true;
        if (!visited.Add(stripped)) return false;
        if (byShortName.TryGetValue(stripped, out var matches))
        {
            foreach (var match in matches)
            {
                if (match.Kind != Models.TypeKind.Interface) continue;
                foreach (var parent in match.ImplementedInterfaces)
                {
                    if (IsHandlerInterface(parent, byShortName, visited))
                        return true;
                }
            }
        }
        return false;
    }

    private static string[] ExtractGenericArgs(string typeName)
    {
        var open = typeName.IndexOf('<');
        if (open < 0) return [];
        var close = typeName.LastIndexOf('>');
        if (close <= open) return [];
        var inner = typeName.Substring(open + 1, close - open - 1);
        return SplitGenericCsv(inner);
    }

    private static string[] SplitGenericCsv(string args)
    {
        var depth = 0;
        var parts = new List<string>();
        var current = new System.Text.StringBuilder();
        foreach (var ch in args)
        {
            switch (ch)
            {
                case '<': depth++; current.Append(ch); break;
                case '>': depth--; current.Append(ch); break;
                case ',' when depth == 0: parts.Add(current.ToString().Trim()); current.Clear(); break;
                default: current.Append(ch); break;
            }
        }
        if (current.Length > 0) parts.Add(current.ToString().Trim());
        return parts.ToArray();
    }

    /// <summary>B3: Detects IPipelineBehavior registrations from DI detections and creates
    /// WrappedBy edges from every Request node to each pipeline behavior. The trace renders
    /// pipeline behaviors as a "pipeline" seam under the first send that reaches a Request.</summary>
    private static void AddPipelineBehaviors(CodeGraphBuilder g, DiscoveryModel model, NameResolver names, SolutionScope scope, NoiseFilter noise)
    {
        var behaviors = new HashSet<(string BehaviorType, string? SourceFile, int? LineNumber)>();

        foreach (var di in model.Detections.OfType<DiRegistrationDetection>())
        {
            if (!scope.Contains(di.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(di.SourceFile)) continue;

            // Direct registration: services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            if (di.ServiceType.Contains("IPipelineBehavior", StringComparison.Ordinal))
            {
                var impl = CleanTypeRef(di.ImplementationType);
                if (!string.IsNullOrEmpty(impl) && impl != "?")
                    behaviors.Add((impl, di.SourceFile, di.LineNumber));
            }
            // MediatR extension: services.AddMediatR(cfg => { cfg.AddOpenBehavior(typeof(LoggingBehavior<,>)); })
            if (di.ExtensionsUsed.Contains("AddOpenBehavior") || di.ServiceType == "AddOpenBehavior")
            {
                var impl = CleanTypeRef(di.ImplementationType);
                if (!string.IsNullOrEmpty(impl) && impl != "?")
                    behaviors.Add((impl, di.SourceFile, di.LineNumber));
            }
            // Fluent config packed in lambda body: scan for AddOpenBehavior(typeof(X)) patterns
            if (di.ImplementationType is { Length: > 0 } body
                && body.Contains("AddOpenBehavior", StringComparison.Ordinal))
            {
                // Scan for AddOpenBehavior(typeof(X)) patterns — manual string scan (L2.3: no Regex here)
                var pos = 0;
                while ((pos = body.IndexOf("AddOpenBehavior", pos, StringComparison.Ordinal)) >= 0)
                {
                    pos += "AddOpenBehavior".Length;
                    var rest = body[pos..];
                    var bp = 0;
                    while (bp < rest.Length && char.IsWhiteSpace(rest[bp])) bp++;
                    if (bp < rest.Length && rest[bp] == '(') bp++;
                    while (bp < rest.Length && char.IsWhiteSpace(rest[bp])) bp++;
                    if (bp + "typeof".Length <= rest.Length
                        && rest.AsSpan(bp, "typeof".Length).SequenceEqual("typeof"))
                    {
                        bp += "typeof".Length;
                        while (bp < rest.Length && char.IsWhiteSpace(rest[bp])) bp++;
                        if (bp < rest.Length && rest[bp] == '(') bp++;
                        while (bp < rest.Length && char.IsWhiteSpace(rest[bp])) bp++;
                        var start = bp;
                        while (bp < rest.Length && (char.IsLetterOrDigit(rest[bp]) || rest[bp] == '_')) bp++;
                        if (bp > start)
                        {
                            var name = rest[start..bp];
                            if (name.Length > 0 && name != "?")
                                behaviors.Add((name, di.SourceFile, di.LineNumber));
                        }
                    }
                }
            }
        }

        foreach (var (behaviorType, file, line) in behaviors)
        {
            var behaviorFqn = names.Resolve(behaviorType, file);
            var behaviorNodeId = NodeId.ForType(behaviorFqn);
            g.AddNode(new GraphNode(behaviorNodeId, behaviorType, NodeKind.Type)
            {
                FilePath = file,
                Tags = [RoleTags.Service, RoleTags.Pipeline],
                Layer = "Infrastructure",
                SourceBody = model.Types.Values
                    .FirstOrDefault(t => t.Id == behaviorFqn)?.SourceBody,
            });

            // WrappedBy edge from every request node (a Type tagged command/query/notification) to
            // this pipeline behavior.
            foreach (var node in g.Nodes.Where(IsRequestNode))
            {
                g.AddEdge(new GraphEdge(node.Id, behaviorNodeId, EdgeKind.WrappedBy)
                {
                    Provenance = file is not null && line is not null ? $"{file}:{line}" : null,
                    Resolution = Resolution.Join,
                });
            }
        }
    }

    /// <summary>Strips typeof(…) / nameof(…) / generics to get a raw type name.</summary>
    private static string CleanTypeRef(string expr)
    {
        var s = expr.AsSpan().Trim();
        // typeof(X) → X
        if (s.StartsWith("typeof(", StringComparison.Ordinal) && s[^1] == ')')
            s = s.Slice(7, s.Length - 8);
        // nameof(X) → X
        else if (s.StartsWith("nameof(", StringComparison.Ordinal) && s[^1] == ')')
            s = s.Slice(7, s.Length - 8);
        // Strip generic arity suffix: LoggingBehavior<,> → LoggingBehavior
        var generic = s.IndexOf('<');
        if (generic > 0) s = s.Slice(0, generic);
        return s.ToString().Trim();
    }

    // ── P1 Map-facing seams (B1) — JOIN detections into graph nodes/edges ────────────────────────

    /// <summary>B1: EfEntityDetection → Entity nodes + aggregate tags PLUS subtypes of detected entity
    /// bases so entities registered via reflection (e.g. DntSite's RegisterAllDerivedEntities) are also
    /// tagged — Iteration 6 deferred / DntSite TOUCHES gap.</summary>
    private static void AddEntityNodes(CodeGraphBuilder g, DiscoveryModel model, NameResolver names, SolutionScope scope, NoiseFilter noise)
    {
        var knownEntityFqns = new HashSet<string>(StringComparer.Ordinal);
        foreach (var e in model.Detections.OfType<EfEntityDetection>())
        {
            if (!scope.Contains(e.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(e.SourceFile)) continue;
            var entityId = NodeId.ForType(names.Resolve(e.EntityType, e.SourceFile));
            var tags = e.IsAggregate
                ? ImmutableArray.Create(RoleTags.Entity, RoleTags.Aggregate)
                : ImmutableArray.Create(RoleTags.Entity);
            g.AddNode(new GraphNode(entityId, e.EntityType, NodeKind.Type)
            {
                FilePath = e.SourceFile,
                Tags = tags,
                Layer = "Domain",
            });
            knownEntityFqns.Add(names.Resolve(e.EntityType, e.SourceFile));
        }

        // Iteration 6 deferred: when a base entity is detected but its subtypes aren't (because they were
        // registered via reflection — DntSite's RegisterAllDerivedEntities from BaseEntity), create
        // entity-tagged nodes for every in-scope production type whose base resolves to a known entity.
        foreach (var type in model.Types.Values)
        {
            if (!scope.Contains(type.FilePath) || type.IsHardExcluded) continue;
            if (type.BaseTypes.IsDefaultOrEmpty) continue;
            foreach (var bt in type.BaseTypes)
            {
                if (knownEntityFqns.Contains(names.Resolve(bt, type.FilePath)))
                {
                    g.AddNode(new GraphNode(NodeId.ForType(type.Id), type.Name, NodeKind.Type)
                    {
                        FilePath = type.FilePath,
                        Tags = [RoleTags.Entity],
                        Layer = "Domain",
                    });
                    break;
                }
            }
        }
    }

    /// <summary>A-F14: Creates EntityRelation edges between entity type nodes by inspecting each entity's
    /// declared navigation properties. Creates edges in the BelongsTo direction (child entity → parent
    /// aggregate/entity) for depth-from-aggregate-root traversal. For reference properties (OrderItem.Order),
    /// the child entity owns the property → edge goes child→parent. For collection properties
    /// (Order.ICollection&lt;OrderItem&gt;), the parent owns the property → edge is reversed to child→parent.
    /// Honesty note: declared-shape only; fluent-API <c>HasMany</c> mappings are not parsed in v1.</summary>
    private static void AddEntityNavigationEdges(CodeGraphBuilder g, DiscoveryModel model,
        NameResolver names, SolutionScope scope)
    {
        // Build a set of known entity short names from detections + already entity-tagged graph nodes
        var entityShortNames = model.Detections.OfType<EfEntityDetection>()
            .Select(d => d.EntityType)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var node in g.Nodes)
        {
            if (node.Kind != NodeKind.Type || !node.Tags.Contains(RoleTags.Entity))
                continue;
            entityShortNames.Add(node.Title);
        }

        foreach (var type in model.Types.Values)
        {
            if (!scope.Contains(type.FilePath)) continue;
            if (!entityShortNames.Contains(type.Name)) continue;
            if (type.Properties.IsDefaultOrEmpty) continue;

            var entityId = NodeId.ForType(type.Id);

            foreach (var prop in type.Properties)
            {
                var (targetName, isCollection) = ExtractInnerEntityNameWithDir(prop.PropertyType);
                if (targetName is null || targetName == type.Name) continue;
                if (!entityShortNames.Contains(targetName)) continue;

                var targetFqn = names.Resolve(targetName, type.FilePath);
                var targetId = NodeId.ForType(targetFqn);

                // BelongsTo direction: edge from child → parent.
                // For collection properties (e.g. Order has ICollection<OrderItem>), the owning type
                // is the parent; edge direction is reversed so OrderItem → Order.
                // For reference properties (e.g. OrderItem has Order Order), the owning type IS the
                // child, so edge direction is already child→parent.
                if (isCollection)
                    g.AddEdge(new GraphEdge(targetId, entityId, EdgeKind.EntityRelation)
                    {
                        Resolution = Resolution.Syntactic,
                        Confidence = 0.6f,
                    });
                else
                    g.AddEdge(new GraphEdge(entityId, targetId, EdgeKind.EntityRelation)
                    {
                        Resolution = Resolution.Syntactic,
                        Confidence = 0.6f,
                    });
            }
        }
    }

    /// <summary>Extracts the inner entity name and collection-direction flag from a property type string.
    /// Returns (name, isCollection) where isCollection is true for <c>ICollection&lt;T&gt;</c>,
    /// <c>List&lt;T&gt;</c>, <c>IEnumerable&lt;T&gt;</c>, <c>T[]</c> patterns.
    /// Returns null for non-entity property types like <c>string</c>, <c>int</c>, <c>DateTime</c>.</summary>
    private static (string? Name, bool IsCollection) ExtractInnerEntityNameWithDir(string propertyType)
    {
        if (string.IsNullOrEmpty(propertyType)) return (null, false);
        var type = propertyType.AsSpan().Trim();

        // Array: OrderItem[] → collection
        if (type.EndsWith("[]"))
        {
            var inner = type[..^2].Trim();
            return inner.IsEmpty ? (null, false) : (inner.ToString(), true);
        }

        // Generic collection: ICollection<OrderItem>, List<Product>, IEnumerable<Entity>, etc.
        var open = type.IndexOf('<');
        var close = type.LastIndexOf('>');
        if (open >= 0 && close > open)
        {
            var inner = type[(open + 1)..close].Trim();
            if (inner.EndsWith("?"))
                inner = inner[..^1];
            return inner.IsEmpty ? (null, false) : (inner.ToString(), true);
        }

        // Nullable reference: Order?
        if (type.EndsWith("?"))
            type = type[..^1];

        // Skip primitives and framework types
        if (type is "string" or "int" or "long" or "short" or "byte" or "float" or "double"
            or "bool" or "char" or "decimal" or "DateTime" or "Guid" or "TimeSpan" or "DateTimeOffset"
            or "Uri" or "object" or "String")
            return (null, false);

        return (type.ToString(), false);
    }

    /// <summary>Extracts the inner entity name from a property type string like
    /// <c>ICollection&lt;OrderItem&gt;</c> → "OrderItem",
    /// <c>List&lt;Product&gt;</c> → "Product",
    /// <c>Order</c> → "Order".
    /// Returns null for non-entity property types like <c>string</c>, <c>int</c>, <c>DateTime</c>.</summary>
    private static string? ExtractInnerEntityName(string propertyType)
    {
        var (name, _) = ExtractInnerEntityNameWithDir(propertyType);
        return name;
    }

    /// <summary>B1: MediatR notification handlers + message bus consumers → Event nodes + Consumes edges.
    /// Domain events (INotificationHandler) and integration events (MessageConsumer) are unified as
    /// Event nodes; both feed into Handler nodes via Consumes edges.</summary>
    private static void AddEventConsumers(CodeGraphBuilder g, DiscoveryModel model, NameResolver names, SolutionScope scope, NoiseFilter noise)
    {
        // Notification handlers (domain events via MediatR)
        foreach (var h in model.Detections.OfType<MediatRHandlerDetection>())
        {
            if (h.Kind != MediatRKind.Notification) continue;
            if (!scope.Contains(h.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(h.SourceFile)) continue;
            var eventId = NodeId.ForType(names.Resolve(h.RequestType, h.SourceFile));
            var handlerId = NodeId.ForType(names.Resolve(h.HandlerType, h.SourceFile));

            g.AddNode(new GraphNode(eventId, h.RequestType, NodeKind.Type)
            {
                Tags = [RoleTags.DomainEvent],
                Layer = "Domain",
            });
            g.AddNode(new GraphNode(handlerId, h.HandlerType, NodeKind.Type)
            {
                FilePath = h.SourceFile,
                Tags = [RoleTags.Handler],
                Layer = "Application",
            });
            g.AddEdge(new GraphEdge(eventId, handlerId, EdgeKind.Consumes)
            {
                Provenance = $"{h.SourceFile}:{h.LineNumber}",
                Resolution = Resolution.Join,
            });
        }

        // Message bus consumers (integration events via RabbitMQ/MassTransit/etc.)
        foreach (var mc in model.Detections.OfType<MessageConsumerDetection>())
        {
            if (!scope.Contains(mc.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(mc.SourceFile)) continue;
            var eventId = NodeId.ForType(names.Resolve(mc.MessageType, mc.SourceFile));
            var consumerType = names.Resolve(mc.ConsumerType, mc.SourceFile);
            var handlerId = NodeId.ForType(consumerType);

            g.AddNode(new GraphNode(eventId, mc.MessageType, NodeKind.Type)
            {
                Tags = [RoleTags.IntegrationEvent, mc.BusKind],
                Layer = "Contracts",
            });
            g.AddNode(new GraphNode(handlerId, mc.ConsumerType, NodeKind.Type)
            {
                FilePath = mc.SourceFile,
                Tags = [RoleTags.Consumer],
                Layer = "Infrastructure",
            });
            g.AddEdge(new GraphEdge(eventId, handlerId, EdgeKind.Consumes)
            {
                Provenance = $"{mc.SourceFile}:{mc.LineNumber}",
                Resolution = Resolution.Join,
            });
        }
    }

    /// <summary>B1: DiRegistrationDetection → Resolves (interface → impl) edges.
    /// Only DirectBinding registrations (explicit interface-to-implementation). Uses ISymbolResolver
    /// for single-implementor fallback. Creates Resolves edges from interface TypeNode to impl TypeNode.</summary>
    private void AddDiResolves(CodeGraphBuilder g, DiscoveryModel model, NameResolver names, SolutionScope scope)
    {
        // Pre-compute single-implementor map for fallback when no DI registration
        var singleImplMap = new Dictionary<string, string>(StringComparer.Ordinal);
        var implCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var type in model.Types.Values)
        {
            if (!scope.Contains(type.FilePath) || !_noise.IsProductionCode(type)) continue;
            foreach (var iface in type.ImplementedInterfaces)
            {
                var ifaceShort = StripGenerics(iface);
                if (!implCounts.TryGetValue(ifaceShort, out var count))
                {
                    count = 0;
                }
                implCounts[ifaceShort] = count + 1;
                if (count == 0)
                    singleImplMap[ifaceShort] = type.Id;
                else
                    singleImplMap.Remove(ifaceShort); // multiple impls → ambiguous
            }
        }

        foreach (var di in model.Detections.OfType<DiRegistrationDetection>())
        {
            if (!scope.Contains(di.SourceFile)) continue;
            if (di.Shape != DiRegistrationShape.DirectBinding) continue;
            if (string.IsNullOrEmpty(di.ImplementationType)
                || di.ImplementationType == "?"
                || di.ImplementationType.StartsWith("sp =>")
                || di.ImplementationType.StartsWith("_ =>")
                || di.ImplementationType.StartsWith("(")
                || di.ImplementationType.Contains("GetRequiredService")) continue;

            var svcFqn = names.Resolve(di.ServiceType, di.SourceFile);
            var implFqn = names.Resolve(di.ImplementationType, di.SourceFile);

            var svcNodeId = NodeId.ForType(svcFqn);
            var implNodeId = NodeId.ForType(implFqn);

            // Ensure both nodes exist
            if (!g.HasNode(svcNodeId))
                g.AddNode(new GraphNode(svcNodeId, di.ServiceType, NodeKind.Type)
                {
                    Layer = "Infrastructure", // DI extension methods (AddMediatR, AddDbContext, etc.)
                });
            g.AddNode(new GraphNode(implNodeId, di.ImplementationType, NodeKind.Type)
            {
                Tags = [RoleTags.Service],
                Layer = "Infrastructure", // DI-registered implementations
            });

            // I1.6 — tag Resolves edges with multi-impl count for render annotation
            var svcShort = StripGenerics(di.ServiceType);
            var multiCount = implCounts.TryGetValue(svcShort, out var c) && c > 1 ? c : 0;
            g.AddEdge(new GraphEdge(svcNodeId, implNodeId, EdgeKind.Resolves)
            {
                Provenance = $"{di.SourceFile}:{di.LineNumber}",
                Resolution = Resolution.Join,
                MultiImplCount = multiCount,
            });
        }

        // Fallback: single-implementor interfaces not covered by DI registrations
        var diResolvedSvcIds = new HashSet<NodeId>();
        foreach (var di in model.Detections.OfType<DiRegistrationDetection>())
        {
            if (!scope.Contains(di.SourceFile)) continue;
            if (di.Shape != DiRegistrationShape.DirectBinding) continue;
            var svcFqn = names.Resolve(di.ServiceType, di.SourceFile);
            diResolvedSvcIds.Add(NodeId.ForType(svcFqn));
        }

        foreach (var (ifaceShort, implFqn) in singleImplMap)
        {
            var ifaceFqn = names.Resolve(ifaceShort);
            var svcNodeId = NodeId.ForType(ifaceFqn);
            var implNodeId = NodeId.ForType(implFqn);
            if (!g.HasNode(svcNodeId) || !g.HasNode(implNodeId)) continue;
            if (diResolvedSvcIds.Contains(svcNodeId)) continue; // already resolved via DI

            var fallbackMultiCount = implCounts.TryGetValue(ifaceShort, out var fc) && fc > 1 ? fc : 0;
            g.AddEdge(new GraphEdge(svcNodeId, implNodeId, EdgeKind.Resolves)
            {
                Resolution = Resolution.Syntactic,
                Confidence = 0.7f,
                MultiImplCount = fallbackMultiCount,
            });
        }
    }

    // ── P2 Trace-facing seams (C1) — joins that complete the indirection-bridged trace ─────────

    /// <summary>C1: model.CallEdges → <b>member→member</b> Calls edges, but ONLY between types that are
    /// real nodes in the graph (in-scope solution types). The syntactic call graph emits a callee per
    /// invocation, many of which are local variables, fluent-chain fragments, or framework methods (e.g.
    /// "group", "pb", "AsNoTracking()"); materializing those as phantom nodes floods the trace with noise.
    /// By requiring both endpoints to already exist as declared Type nodes (non-null FilePath), the trace
    /// keeps only edges to types we actually know. Origin is the caller <b>method</b> and target the callee
    /// <b>method</b> (both carried on <see cref="CallEdge"/>), so a focused trace descends method-to-method
    /// — the spine — instead of inheriting every sibling method's edges. Member nodes carry their owning
    /// Type's FilePath (salient lines fall back to the Type body in <see cref="TraceBuilder"/>).
    /// Resolution flows through from the edge (semantic → [verified], syntactic → [approx]).</summary>
    private static void AddCallEdges(CodeGraphBuilder g, DiscoveryModel model, NameResolver names,
        IReadOnlyList<BodyFacts>? bodyFacts = null)
    {
        // L3.3 — build semantic index from upgraded BodyFacts for Call edge verification.
        var semanticLocs = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        if (bodyFacts is { Count: > 0 })
        {
            foreach (var body in bodyFacts)
            {
                foreach (var op in body.Ops)
                {
                    var prov = $"{body.File}:{op.Line}";
                    switch (op)
                    {
                        case CreationOp c when c.Type is { Tier: ResolutionTier.Semantic } s:
                            AddToIndex(semanticLocs, prov, s.Text);
                            break;
                        case LocalDeclOp l when l.InferredFrom is { Tier: ResolutionTier.Semantic } s:
                            AddToIndex(semanticLocs, prov, s.Text);
                            break;
                        case InvocationOp i when i.ReceiverType is { Tier: ResolutionTier.Semantic } s:
                            AddToIndex(semanticLocs, prov, s.Text);
                            break;
                        case InvocationOp i:
                            foreach (var ga in i.GenericArgs)
                                if (ga is { Tier: ResolutionTier.Semantic } s)
                                    AddToIndex(semanticLocs, prov, s.Text);
                            break;
                    }
                }
            }
        }

        foreach (var ce in model.CallEdges)
        {
            var callerFqn = names.Resolve(ce.CallerType, ce.CallSiteLocation);
            var calleeFqn = names.Resolve(ce.CalleeType, ce.CallSiteLocation);

            // Filter self-calls to known noise targets — syntactic-resolver mis-attributions (nameof,
            // controller result-helpers) that member-origin precision surfaced (Iteration 4 noise polish).
            if (callerFqn == calleeFqn && IsSelfCallNoise(ce.CalleeMethod)) continue;

            // Declared in-scope types only. After the Type+tags collapse, requests/events/handlers that
            // live in referenced projects also exist as Type nodes (name-only, added by joins) — gating
            // on a non-null FilePath (set only by AddTypeNodes) keeps Calls restricted to types we
            // actually declared, exactly as before the collapse, so no phantom call edges appear.
            var callerType = g.GetNode(NodeId.ForType(callerFqn));
            var calleeType = g.GetNode(NodeId.ForType(calleeFqn));
            if (callerType?.FilePath is null || calleeType?.FilePath is null) continue;

            var callerId = NodeId.ForMember(callerFqn, ce.CallerMethod);
            var calleeId = NodeId.ForMember(calleeFqn, ce.CalleeMethod);
            if (callerId == calleeId) continue;                              // skip direct self-recursion

            // L3.3 — check if this call site was semantically verified via Tier B body facts.
            var resolution = ce.Resolution;
            if (resolution == Resolution.Syntactic
                && ce.CallSiteLocation is { } loc
                && semanticLocs.TryGetValue(loc, out var semTargets)
                && IsAnyShortMatch(ce.CalleeType, semTargets))
            {
                resolution = Resolution.Semantic;
            }

            // Member nodes for both endpoints, carrying the owning Type's file (body filled — when at all —
            // by the body-scan seams / HTTP entry; salient otherwise falls back to the parent Type body).
            g.AddNode(new GraphNode(callerId, $"{callerType.Title}.{ce.CallerMethod}", NodeKind.Member)
            {
                FilePath = callerType.FilePath,
            });
            g.AddNode(new GraphNode(calleeId, $"{calleeType.Title}.{ce.CalleeMethod}", NodeKind.Member)
            {
                FilePath = calleeType.FilePath,
            });

            g.AddEdge(new GraphEdge(callerId, calleeId, EdgeKind.Calls)
            {
                Provenance = ce.CallSiteLocation,
                Resolution = resolution,
                Confidence = resolution == Resolution.Semantic ? 0.95f : 0.6f,
            });
        }
    }

    /// <summary>L3.4 — Broadens call-edge binding for sparse graphs (library/tool archetypes where
    /// normal CallEdges produce very few edges because one or both endpoints lack a FilePath).
    /// Detects sparseness (entries &lt; 5 or edge/node ratio &lt; 0.1), identifies top-K central
    /// type nodes by degree, and binds their inter-type call edges from the model's CallEdges.
    /// Budget-capped at 500 additional edges; honest scope reported in Stats.</summary>
    private static (bool IsSparse, int HubCount) AddHubScopeEdges(CodeGraphBuilder g, DiscoveryModel model, NameResolver names,
        ImmutableArray<EntryPoint> entries)
    {
        var nodeCount = g.NodeCount;
        var edgeCount = g.EdgeCount;
        var ratio = nodeCount > 0 ? (double)edgeCount / nodeCount : 0;

        if (entries.Length >= 5 && ratio >= 0.1) return (false, 0);

        // Compute degree centrality for all types with a FilePath (in-scope, production code)
        var typeDegrees = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var ce in model.CallEdges)
        {
            var cfqn = names.Resolve(ce.CallerType);
            var dfqn = names.Resolve(ce.CalleeType);
            if (cfqn != ce.CallerType) typeDegrees[cfqn] = typeDegrees.GetValueOrDefault(cfqn) + 1;
            if (dfqn != ce.CalleeType) typeDegrees[dfqn] = typeDegrees.GetValueOrDefault(dfqn) + 1;
        }

        // Build a set of type nodes already present with FilePath (production code)
        var existingTypes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var node in g.Nodes)
            if (node.Kind == NodeKind.Type && node.FilePath is not null)
                existingTypes.Add(node.Id.Key);

        // Top-K hubs
        var k = Math.Min(50, Math.Min(nodeCount / 4, typeDegrees.Count / 2));
        if (k < 5) return (false, 0);

        var hubs = typeDegrees
            .Where(kv => existingTypes.Contains(kv.Key))
            .OrderByDescending(kv => kv.Value)
            .Take(k)
            .Select(kv => kv.Key)
            .ToHashSet(StringComparer.Ordinal);

        var added = 0;
        foreach (var ce in model.CallEdges)
        {
            if (added >= 500) break;

            var cfqn = names.Resolve(ce.CallerType);
            var dfqn = names.Resolve(ce.CalleeType);
            if (cfqn == dfqn) continue;

            // At least one endpoint must be a hub
            if (!hubs.Contains(cfqn) && !hubs.Contains(dfqn)) continue;

            var callerId = NodeId.ForMember(cfqn, ce.CallerMethod);
            var calleeId = NodeId.ForMember(dfqn, ce.CalleeMethod);
            if (callerId == calleeId) continue;

            var callerNode = g.GetNode(NodeId.ForType(cfqn));
            var calleeNode = g.GetNode(NodeId.ForType(dfqn));

            g.AddNode(new GraphNode(callerId, $"{callerNode?.Title ?? cfqn}.{ce.CallerMethod}", NodeKind.Member)
            {
                FilePath = callerNode?.FilePath,
            });
            g.AddNode(new GraphNode(calleeId, $"{calleeNode?.Title ?? dfqn}.{ce.CalleeMethod}", NodeKind.Member)
            {
                FilePath = calleeNode?.FilePath,
            });

            if (g.AddEdge(new GraphEdge(callerId, calleeId, EdgeKind.Calls)
            {
                Provenance = ce.CallSiteLocation,
                Resolution = ce.Resolution,
                Confidence = (ce.Resolution == Resolution.Semantic ? 0.95f : 0.6f) * 0.8f,
            }))
                added++;
        }
        return (true, hubs.Count);
    }

    // ── L2.3: Seam detectors over BodyFacts (replaces the old regex body-scan methods) ────────────

    /// <summary>L2.3 — Runs structured seam detectors (<see cref="ISeamDetector"/>) over the pre-extracted
    /// <see cref="BodyFacts"/>, replacing the old regex body-scan methods (<c>AddSends</c>, <c>AddRaises</c>,
    /// <c>AddDataEdges</c>). Detectors are pure (facts in, seams out); here we resolve targets via the
    /// <see cref="SymbolTable"/> and materialise graph nodes/edges. Ambiguous targets are skipped per
    /// Law R1 (no silent winners); unresolved (external) types use the short name as-is. Edge provenance
    /// comes from the body-fact line number, anchored on the correct Member node by construction — never a
    /// char-offset estimate. Event→project mappings are tracked for the downstream cross-service bus
    /// ServiceLink join (replaces the old regex-based <c>_eventPublishers</c> collection).</summary>
    private void AddSeamsFromDetectors(CodeGraphBuilder g, DiscoveryModel model, NameResolver names,
        SolutionScope scope, IReadOnlyList<BodyFacts>? allBodyFacts)
    {
        // Auto-extract BodyFacts from model TypeDiscovery SourceBodies when the pipeline hasn't
        // pre-extracted them (backward compatibility for tests that build directly from model).
        if (allBodyFacts is null || allBodyFacts.Count == 0)
        {
            var facts = new List<BodyFacts>();
            foreach (var type in model.Types.Values)
            {
                if (type.SourceBody is not { Length: > 0 } sb) continue;
                try
                {
                    var hasTypeDecl = sb.Contains("class ", StringComparison.Ordinal)
                        || sb.Contains("struct ", StringComparison.Ordinal)
                        || sb.Contains("record ", StringComparison.Ordinal);
                    var fullSource = sb;
                    if (!hasTypeDecl)
                    {
                        var nsDecl = !string.IsNullOrEmpty(type.Namespace) && !sb.Contains("namespace ", StringComparison.Ordinal)
                            ? $"namespace {type.Namespace} {{ "
                            : (sb.Contains("namespace ", StringComparison.Ordinal) ? "" : $"namespace {type.Name} {{ ");
                        var closings = nsDecl.Length > 0 ? " }}" : " }";
                        fullSource = $"{nsDecl}public class {type.Name} {{ {sb}{closings}";
                    }
                    else if (!string.IsNullOrEmpty(type.Namespace) && !sb.Contains("namespace ", StringComparison.Ordinal))
                    {
                        fullSource = $"namespace {type.Namespace} {{ {sb} }}";
                    }
                    var parseOpts = CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG");
                    var tree = CSharpSyntaxTree.ParseText(fullSource, parseOpts, path: type.FilePath);
                    var project = scope.ProjectForFile(type.FilePath) ?? "";
                    facts.AddRange(BodyFactExtractor.Extract(tree, type.FilePath, project));
                }
                catch { /* parse failure → skip */ }
            }
            allBodyFacts = facts;
        }
        if (allBodyFacts.Count == 0) return;

        // Build SeamContext from model detections + type base/interface data
        var (integrationTypes, domainTypes) = BuildTypeEventSets(model);
        var knownEntities = new HashSet<string>(StringComparer.Ordinal);

        foreach (var e in model.Detections.OfType<EfEntityDetection>())
        {
            knownEntities.Add(e.EntityType);
            knownEntities.Add(names.Resolve(e.EntityType, e.SourceFile));
        }
        foreach (var mc in model.Detections.OfType<MessageConsumerDetection>())
            integrationTypes.Add(mc.MessageType);
        foreach (var h in model.Detections.OfType<MediatRHandlerDetection>())
        {
            if (h.Kind == MediatRKind.Notification)
                domainTypes.Add(h.RequestType);
        }

        // Entity and event names that are also FQNs
        foreach (var e in model.Detections.OfType<EfEntityDetection>())
        {
            var entityFqn = names.Resolve(e.EntityType, e.SourceFile);
            if (!string.IsNullOrEmpty(entityFqn) && entityFqn != "?" && entityFqn != e.EntityType)
                knownEntities.Add(entityFqn);
        }
        foreach (var mc in model.Detections.OfType<MessageConsumerDetection>())
        {
            var msgFqn = names.Resolve(mc.MessageType, mc.SourceFile);
            if (!string.IsNullOrEmpty(msgFqn) && msgFqn != "?" && msgFqn != mc.MessageType)
                integrationTypes.Add(msgFqn);
        }
        foreach (var node in g.Nodes)
        {
            if (node.Kind == NodeKind.Type && node.Tags.Contains(RoleTags.Entity))
                knownEntities.Add(node.Title);
        }

        var ctx = BuildSeamContext(model, scope, integrationTypes, domainTypes, knownEntities, allBodyFacts);

        var detectors = new ISeamDetector[]
        {
            new MediatRDispatchDetector(),
            new BusPublishDetector(),
            new IntegrationEventCreationDetector(),
            new DomainEventRaiseDetector(),
            new EntityTouchDetector(),
            new PlainCallDetector(),
        };

        // L3.3 — build a quick index of (provenance → semantic-short-names) from upgraded BodyFacts.
        // Both provenance-level (file:line) and body-level keys are stored.
        var semanticLocs = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var body in allBodyFacts)
        {
            foreach (var op in body.Ops)
            {
                var prov = $"{body.File}:{op.Line}";
                switch (op)
                {
                    case CreationOp c when c.Type is { Tier: ResolutionTier.Semantic } s:
                        AddToIndex(semanticLocs, prov, s.Text);
                        break;
                    case LocalDeclOp l when l.InferredFrom is { Tier: ResolutionTier.Semantic } s:
                        AddToIndex(semanticLocs, prov, s.Text);
                        break;
                    case InvocationOp i when i.ReceiverType is { Tier: ResolutionTier.Semantic } s:
                        AddToIndex(semanticLocs, prov, s.Text);
                        break;
                    case InvocationOp i:
                        foreach (var ga in i.GenericArgs)
                            if (ga is { Tier: ResolutionTier.Semantic } s)
                                AddToIndex(semanticLocs, prov, s.Text);
                        break;
                }
            }
        }

        foreach (var body in allBodyFacts)
        {
            foreach (var detector in detectors)
            {
                try
                {
                    foreach (var match in detector.Detect(body, ctx))
                    {
                        var originId = ToMemberNodeId(match.Origin);
                        EnsureMemberId(g, originId, body.MemberName, body.File, body.Project, body.DeclLine);

                        var resolved = ctx.Symbols!.Resolve(match.Target);
                        if (resolved.Tier == ResolutionTier.Ambiguous)
                            continue; // Law R1: no silent winners

                        NodeId targetId;
                        string targetDisplayName = match.Target.Text;

                        if (resolved.Resolved is { } symId)
                            targetId = NodeId.ForType(symId.Canonical);
                        else
                            targetId = NodeId.ForType(match.Target.Text);

                        if (!g.HasNode(targetId))
                        {
                            var tags = match.Kind switch
                            {
                                EdgeKind.ReadsWrites => ImmutableArray.Create(RoleTags.Entity),
                                EdgeKind.Raises => match.DetectorId switch
                                {
                                    "IntegrationEventCreation" or "BusPublish" => ImmutableArray.Create(RoleTags.IntegrationEvent),
                                    "DomainEventRaise" => ImmutableArray.Create(RoleTags.DomainEvent),
                                    _ => ImmutableArray.Create(RoleTags.DomainEvent),
                                },
                                EdgeKind.Sends => ImmutableArray.Create(RoleTags.Command),
                                _ => ImmutableArray<string>.Empty,
                            };
                            g.AddNode(new GraphNode(targetId, targetDisplayName, NodeKind.Type)
                            {
                                Tags = tags,
                                Layer = match.Kind switch
                                {
                                    EdgeKind.ReadsWrites => "Domain",
                                    EdgeKind.Raises => "Domain",
                                    EdgeKind.Sends => "Application",
                                    _ => null,
                                },
                            });
                        }

                        var isSemantic = resolved.Tier == ResolutionTier.Semantic
                            || match.Target.Tier == ResolutionTier.Semantic
                            || (match.Provenance is { } p && semanticLocs.TryGetValue(p, out var semTargets)
                                && semTargets.Contains(match.Target.Text))
                            || IsTargetSemanticInBody(body, match.Target.Text);

                        g.AddEdge(new GraphEdge(originId, targetId, match.Kind)
                        {
                            Provenance = match.Provenance,
                            Resolution = isSemantic ? Resolution.Semantic : Resolution.Syntactic,
                            Confidence = match.Confidence,
                        });

                        // Track event→publisher for cross-service bus ServiceLink join
                        if (match.Kind == EdgeKind.Raises
                            && (match.DetectorId is "BusPublish" or "IntegrationEventCreation"))
                        {
                            var pubProject = scope.ProjectForFile(body.File) ?? body.Project;
                            if (!string.IsNullOrEmpty(pubProject) && _eventPublishers is not null)
                            {
                                var trackingKey = resolved.Resolved?.Canonical ?? match.Target.Text;
                                if (!_eventPublishers.TryGetValue(trackingKey, out var pubSet))
                                    _eventPublishers[trackingKey] = pubSet = [];
                                pubSet.Add(pubProject);
                            }
                        }
                    }
                }
                catch { /* detector failure → skip its matches, continue with others */ }
            }
        }
    }

    private static void AddToIndex(Dictionary<string, HashSet<string>> map, string prov, string text)
    {
        if (!map.TryGetValue(prov, out var set)) map[prov] = set = [];
        set.Add(text);
    }

    private static bool IsTargetSemanticInBody(BodyFacts body, string targetShort)
    {
        foreach (var op in body.Ops)
        {
            if (op is LocalDeclOp l && l.InferredFrom is { Tier: ResolutionTier.Semantic } s
                && string.Equals(s.Text, targetShort, StringComparison.Ordinal))
                return true;
            if (op is CreationOp c && c.Type is { Tier: ResolutionTier.Semantic } cs
                && string.Equals(cs.Text, targetShort, StringComparison.Ordinal))
                return true;
            if (op is InvocationOp i && i.ReceiverType is { Tier: ResolutionTier.Semantic } rs
                && string.Equals(rs.Text, targetShort, StringComparison.Ordinal))
                return true;
            if (op is InvocationOp ig)
                foreach (var ga in ig.GenericArgs)
                    if (ga is { Tier: ResolutionTier.Semantic } gs
                        && string.Equals(gs.Text, targetShort, StringComparison.Ordinal))
                        return true;
        }
        return false;
    }

    private static bool IsAnyShortMatch(string fqn, HashSet<string> shortNames)
    {
        // The short name matches if it equals the last segment of the FQN or the full FQN.
        foreach (var sn in shortNames)
            if (string.Equals(fqn, sn, StringComparison.Ordinal)
                || fqn.EndsWith("." + sn, StringComparison.Ordinal))
                return true;
        return false;
    }

    /// <summary>Converts a BodyFacts <see cref="SymbolId"/> (format <c>TypeFqn::MethodName(N)</c>) to the
    /// <see cref="NodeId"/> format used by the graph (<c>TypeFqn.MethodName</c>).</summary>
    private static NodeId ToMemberNodeId(SymbolId memberId)
    {
        var canonical = memberId.Canonical;
        var sep = canonical.IndexOf("::", StringComparison.Ordinal);
        if (sep < 0) return NodeId.ForMember(canonical, canonical);
        var typeFqn = canonical[..sep];
        var after = canonical[(sep + 2)..];
        var paren = after.IndexOf('(');
        var methodName = paren > 0 ? after[..paren] : after;
        return NodeId.ForMember(typeFqn, methodName);
    }

    /// <summary>L2.4 — Runs seam detectors on lambda entry-handler member nodes that carry a SourceBody
    /// (populated by <see cref="HttpEntryPointBuilder"/>). Lambdas live inside the enclosing method's
    /// BodyFacts, so the main pass attributes edges to the enclosing method. This post-pass extracts
    /// per-lambda facts and attributes edges to the lambda member node so entry→lambda→dispatch traces
    /// work correctly for the checkout flow.</summary>
    private static void AddLambdaSeams(CodeGraphBuilder g, DiscoveryModel model, NameResolver names, SolutionScope scope,
        IReadOnlyList<BodyFacts>? upgradedFacts)
    {
        var (integrationTypes, domainTypes) = BuildTypeEventSets(model);
        var ctx = BuildSeamContext(model, scope, integrationTypes, domainTypes, ImmutableHashSet<string>.Empty, upgradedFacts);

        // L3.2/L3.3 — semantic overlay: the lambda body is re-parsed in isolation (a synthetic tree not in
        // the Tier-B compilation), so its ops carry only syntactic types. Re-attach the semantic tier that the
        // whole-file pass already established, matched by (file, expression text, short type) — no line
        // dependency, so the synthetic tree's shifted lines don't matter. Covers dispatch via a `var` local
        // (LocalDecl.InferredFrom), inline argument (`sender.Send(new XCommand(..))` — ArgFact.Type), and
        // generic type arguments (`Adapt<T>`). Law R2: upgrade only.
        var semanticRefs = new Dictionary<(string File, string Text, string Type), Graph2.SymbolRef>();
        if (upgradedFacts is not null)
        {
            foreach (var body in upgradedFacts)
            {
                foreach (var op in body.Ops)
                {
                    switch (op)
                    {
                        case LocalDeclOp { InferredFrom: { Tier: ResolutionTier.Semantic } sem } local:
                            semanticRefs[(body.File, local.Name, sem.Text)] = sem;
                            break;
                        case InvocationOp inv:
                            foreach (var arg in inv.Args)
                                if (arg.Type is { Tier: ResolutionTier.Semantic } at)
                                    semanticRefs[(body.File, arg.Text, at.Text)] = at;
                            foreach (var ga in inv.GenericArgs)
                                if (ga is { Tier: ResolutionTier.Semantic } gat)
                                    semanticRefs[(body.File, gat.Text, gat.Text)] = gat;
                            break;
                    }
                }
            }
        }

        var detectors = new ISeamDetector[]
        {
            new MediatRDispatchDetector(),
            new BusPublishDetector(),
            new IntegrationEventCreationDetector(),
            new DomainEventRaiseDetector(),
            new EntityTouchDetector(),
            new PlainCallDetector(),
        };

        foreach (var node in g.Nodes.Where(n => n.Kind == NodeKind.Member
            && n.SourceBody is { Length: > 0 }
            && (n.Id.Key.Contains("<lambda>", StringComparison.Ordinal)
                || n.Id.Key.Contains("<anonymous>", StringComparison.Ordinal))).ToList())
        {
            try
            {
                // Wrap the lambda body in a synthetic method + class so the extractor finds it
                var body = node.SourceBody!;
                var filePath = node.FilePath ?? "";
                var project = node.Project ?? scope.ProjectForFile(filePath) ?? "";
                var wrapped = $"namespace _ {{ public class _ {{ public void _() {{ ({body})(); }} }} }}";
                var tree = CSharpSyntaxTree.ParseText(wrapped, path: filePath);
                var facts = OverlaySemanticLocals(
                    BodyFactExtractor.Extract(tree, filePath, project), filePath, semanticRefs);

                foreach (var bodyFacts in facts)
                {
                    foreach (var detector in detectors)
                    {
                        try
                        {
                            foreach (var match in detector.Detect(bodyFacts, ctx))
                            {
                                var resolved = ctx.Symbols!.Resolve(match.Target);
                                if (resolved.Tier == ResolutionTier.Ambiguous) continue;

                                NodeId targetId;
                                if (resolved.Resolved is { } symId)
                                    targetId = NodeId.ForType(symId.Canonical);
                                else
                                    targetId = NodeId.ForType(match.Target.Text);

                                EnsureMemberId(g, node.Id, node.Title, node.FilePath, node.Project);

                                if (!g.HasNode(targetId))
                                {
                                    g.AddNode(new GraphNode(targetId, match.Target.Text, NodeKind.Type)
                                    {
                                        Tags = match.Kind switch
                                        {
                                            EdgeKind.Sends => ImmutableArray.Create(RoleTags.Command),
                                            EdgeKind.Raises => ImmutableArray.Create(RoleTags.DomainEvent),
                                            _ => ImmutableArray<string>.Empty,
                                        },
                                        Layer = match.Kind switch
                                        {
                                            EdgeKind.Sends => "Application",
                                            EdgeKind.Raises => "Domain",
                                            _ => null,
                                        },
                                    });
                                }

                                g.AddEdge(new GraphEdge(node.Id, targetId, match.Kind)
                                {
                                    Provenance = match.Provenance,
                                    Resolution = resolved.Tier == ResolutionTier.Semantic
                                        ? Resolution.Semantic
                                        : Resolution.Syntactic,
                                    Confidence = match.Confidence,
                                });
                            }
                        }
                        catch { /* detector failure → skip its matches for this lambda */ }
                    }
                }
            }
            catch { /* parse failure → skip */ }
        }
    }

    /// <summary>Re-attaches semantic types (Tier B) onto facts re-parsed from a lambda body, matched by
    /// (file, expression text, short type). Upgrade-only (Law R2): a syntactic ref whose text+type matches a
    /// whole-file semantic bind is lifted to Semantic; everything else is untouched. Covers dispatch via a
    /// <c>var</c> local (<see cref="LocalDeclOp.InferredFrom"/>), inline argument (<see cref="ArgFact.Type"/>),
    /// and generic type arguments (<see cref="InvocationOp.GenericArgs"/>).</summary>
    private static ImmutableArray<BodyFacts> OverlaySemanticLocals(
        ImmutableArray<BodyFacts> facts, string filePath,
        Dictionary<(string File, string Text, string Type), Graph2.SymbolRef> semanticRefs)
    {
        if (semanticRefs.Count == 0) return facts;

        var result = ImmutableArray.CreateBuilder<BodyFacts>(facts.Length);
        foreach (var body in facts)
        {
            var ops = body.Ops;
            var changed = false;
            for (var i = 0; i < ops.Length; i++)
            {
                switch (ops[i])
                {
                    case LocalDeclOp { InferredFrom: { Tier: not ResolutionTier.Semantic } inf } local
                        when semanticRefs.TryGetValue((filePath, local.Name, inf.Text), out var sem):
                        ops = ops.SetItem(i, local with { InferredFrom = sem });
                        changed = true;
                        break;

                    case InvocationOp inv:
                    {
                        var newInv = inv;
                        var invChanged = false;

                        if (!inv.Args.IsDefaultOrEmpty)
                        {
                            var args = inv.Args;
                            var argsChanged = false;
                            for (var ai = 0; ai < args.Length; ai++)
                            {
                                if (args[ai].Type is { Tier: not ResolutionTier.Semantic } at
                                    && semanticRefs.TryGetValue((filePath, args[ai].Text, at.Text), out var semArg))
                                { args = args.SetItem(ai, args[ai] with { Type = semArg }); argsChanged = true; }
                            }
                            if (argsChanged) { newInv = newInv with { Args = args }; invChanged = true; }
                        }

                        if (inv.GenericArgs.Length > 0)
                        {
                            var gargs = inv.GenericArgs;
                            var gargsChanged = false;
                            for (var gi = 0; gi < gargs.Length; gi++)
                            {
                                if (gargs[gi].Tier != ResolutionTier.Semantic
                                    && semanticRefs.TryGetValue((filePath, gargs[gi].Text, gargs[gi].Text), out var semG))
                                { gargs = gargs.SetItem(gi, semG); gargsChanged = true; }
                            }
                            if (gargsChanged) { newInv = newInv with { GenericArgs = gargs }; invChanged = true; }
                        }

                        if (invChanged) { ops = ops.SetItem(i, newInv); changed = true; }
                        break;
                    }
                }
            }
            result.Add(changed ? body with { Ops = ops } : body);
        }
        return result.ToImmutable();
    }

    /// <summary>Ensures a Member node exists in the graph for the given id (first-write wins).</summary>
    private static void EnsureMemberId(CodeGraphBuilder g, NodeId id, string? memberName, string? file, string? project, int? line = null)
    {
        if (g.HasNode(id)) return;
        g.AddNode(new GraphNode(id, memberName ?? id.Key, NodeKind.Member)
        {
            FilePath = file,
            Project = project,
            LineNumber = line is > 0 ? line : null,
        });
    }


    /// <summary>True for a self-call target that is syntactic-resolver noise, not real wiring: the
    /// <c>nameof</c> pseudo-call, and the common ASP.NET <c>ControllerBase</c> result helpers (inherited,
    /// not declared on the controller) that resolve to <c>this</c> (Iteration 4 noise polish).</summary>
    private static bool IsSelfCallNoise(string method)
        => method is "nameof"
            or "Ok" or "NotFound" or "BadRequest" or "NoContent" or "Created" or "CreatedAtAction"
            or "CreatedAtRoute" or "Accepted" or "Unauthorized" or "Forbid" or "StatusCode"
            or "Content" or "Json" or "Redirect" or "RedirectToAction" or "File" or "ValidationProblem"
            or "Problem" or "Conflict" or "UnprocessableEntity";

    /// <summary>True for a node that represents a MediatR request (a Type tagged command/query/
    /// notification) � the targets a pipeline behavior wraps. Replaces the old NodeKind.Request check.</summary>
    private static bool IsRequestNode(GraphNode n)
        => n.Kind == NodeKind.Type
            && (n.Tags.Contains(RoleTags.Command)
                || n.Tags.Contains(RoleTags.Query)
                || n.Tags.Contains(RoleTags.Notification));

    /// <summary>Strips generic type arguments: <c>List&lt;int&gt;</c>?"List". Used in handler-joins, DI resolution, and type-name heuristics.</summary>
    private static string StripGenerics(string typeName)
    {
        var idx = typeName.IndexOf('<');
        return idx > 0 ? typeName[..idx].TrimEnd() : typeName.TrimEnd();
    }

    /// <summary>True for an EndpointDetection that is a framework/infrastructure pseudo-entry � OpenAPI/Scalar
    /// root routes registered in ServiceDefaults or extension files � not genuine application surface. The
    /// guard matches on both source and route, not just <c>"/"</c>, so a real root route isn't falsely dropped.</summary>
    internal static bool IsInfrastructureEntry(EndpointDetection ep)
    {
        if (ep.RouteTemplate is "/" or "" or "/index.html" or "/openapi" or "/scalar")
        {
            var f = ep.SourceFile.AsSpan();
            if (f.Contains("ServiceDefaults", StringComparison.OrdinalIgnoreCase)
                || f.Contains("OpenApi", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>Normalizes a route template for dedup comparison.</summary>
    internal static string NormalizeRoute(string route) => route.TrimStart('/').TrimEnd('/');


    // ── M1.6-M1.8: Cross-service ServiceLink joins (W4) ──────────────────────────

    /// <summary>M1.6 — Cross-project MassTransit bus ServiceLinks. Uses event→publisher
    /// mappings collected during AddSends, matched against MessageConsumerDetection consumers.</summary>
    private void AddBusServiceLinks(CodeGraphBuilder g, DiscoveryModel model,
        NameResolver names, SolutionScope scope, NoiseFilter noise)
    {

        if (_eventPublishers is null || _eventPublishers.Count == 0) return;

        // Collect consumer projects from MessageConsumerDetection detections
        var consumesByEvent = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var mc in model.Detections.OfType<MessageConsumerDetection>())
        {
            if (!scope.Contains(mc.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(mc.SourceFile)) continue;

            var eventFqn = names.Resolve(mc.MessageType, mc.SourceFile);
            var consumerProject = scope.ProjectForFile(mc.SourceFile) ?? "";
            if (string.IsNullOrEmpty(consumerProject)) continue;

            if (!consumesByEvent.TryGetValue(eventFqn, out var conSet))
                consumesByEvent[eventFqn] = conSet = [];
            conSet.Add(consumerProject);
        }

        // Cross-project join: match publishers to consumers
        foreach (var (eventFqn, publisherProjects) in _eventPublishers)
        {
            if (!consumesByEvent.TryGetValue(eventFqn, out var consumerProjects))
            {
                // Try short-name match as fallback
                var shortName = eventFqn.Contains('.') ? eventFqn[(eventFqn.LastIndexOf('.') + 1)..] : eventFqn;
                var matches = consumesByEvent.Where(kv => kv.Key.EndsWith("." + shortName, StringComparison.OrdinalIgnoreCase)
                    || kv.Key.Equals(shortName, StringComparison.OrdinalIgnoreCase)).ToList();
                if (matches.Count == 0) continue;
                consumerProjects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var (_, cps) in matches)
                    foreach (var cp in cps)
                        consumerProjects.Add(cp);
                if (consumerProjects.Count == 0) continue;
            }

            foreach (var pubProject in publisherProjects)
            {
                foreach (var conProject in consumerProjects)
                {
                    if (string.Equals(pubProject, conProject, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var fromId = NodeId.ForService(pubProject);
                    var toId = NodeId.ForService(conProject);
                    g.AddNode(new GraphNode(fromId, pubProject, NodeKind.Service));
                    g.AddNode(new GraphNode(toId, conProject, NodeKind.Service));

                    g.AddEdge(new GraphEdge(fromId, toId, EdgeKind.ServiceLink)
                    {
                        Provenance = $"{pubProject}→{conProject}:{eventFqn}",
                        Resolution = Resolution.Join,
                        Confidence = 0.8f,
                        Tags = [ServiceLinkTags.BusPublishConsume],
                    });

                }
            }
        }
    }

    /// <summary>M1.7 — Cross-project gRPC ServiceLinks. Matches <see cref="GrpcClientDetection"/>
    /// (client type usage in project A) to <see cref="GrpcServiceDetection"/> (service implementation
    /// in project B) by matching the service name.</summary>
    private static void AddGrpcServiceLinks(CodeGraphBuilder g, DiscoveryModel model,
        NameResolver names, SolutionScope scope, NoiseFilter noise)
    {

        var clients = model.Detections.OfType<GrpcClientDetection>().ToList();
        var servers = model.Detections.OfType<GrpcServiceDetection>().ToList();
        if (clients.Count == 0 || servers.Count == 0) return;

        foreach (var client in clients)
        {
            if (!scope.Contains(client.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(client.SourceFile)) continue;
            var clientProject = scope.ProjectForFile(client.SourceFile) ?? "";

            foreach (var server in servers)
            {
                if (!scope.Contains(server.SourceFile)) continue;
                if (!noise.IsProductionEntrySource(server.SourceFile)) continue;
                var serverProject = scope.ProjectForFile(server.SourceFile) ?? "";

                if (string.Equals(clientProject, serverProject, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Match by service name (e.g. "DiscountProtoService")
                if (!string.Equals(client.ServiceName, server.ServiceName, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Create a project-level ServiceLink edge: client project → server project
                var fromId = NodeId.ForService(clientProject);
                var toId = NodeId.ForService(serverProject);
                g.AddNode(new GraphNode(fromId, clientProject, NodeKind.Service));
                g.AddNode(new GraphNode(toId, serverProject, NodeKind.Service));

                g.AddEdge(new GraphEdge(fromId, toId, EdgeKind.ServiceLink)
                {
                    Provenance = $"{client.SourceFile}:{client.LineNumber}→{server.SourceFile}:{server.LineNumber}",
                    Resolution = Resolution.Join,
                    Confidence = 0.85f,
                    Tags = [ServiceLinkTags.Grpc],
                });

            }
        }
    }

    /// <summary>M1.8 — Cross-project HTTP/YARP/Refit ServiceLinks. Matches Refit interface routes
    /// through YARP gateway config to downstream service HTTP entry points. Uses segment-aware
    /// path-pattern normalization: strips YARP template variables ({**catch-all}, {param}) to
    /// static-prefix-match against Refit route segments.</summary>
    private static void AddHttpServiceLinks(CodeGraphBuilder g, DiscoveryModel model,
        NameResolver names, SolutionScope scope, NoiseFilter noise)
    {

        if (model.GatewayRoutes.Count == 0) return;

        // Find the gateway project (has YARP/Ocelot packages)
        string? gatewayProject = null;
        foreach (var proj in model.Projects)
        {
            if (proj.PackageReferences.Any(pr =>
                pr.Name.Contains("Yarp", StringComparison.OrdinalIgnoreCase)
                || pr.Name.Contains("Ocelot", StringComparison.OrdinalIgnoreCase)))
            {
                gatewayProject = proj.Name;
                break;
            }
        }
        if (string.IsNullOrEmpty(gatewayProject)) return;

        var refitRoutes = model.Detections.OfType<RefitRouteDetection>().ToList();
        if (refitRoutes.Count == 0) return;

        // ── Build per-gateway-route static prefix for matching ─────────────────
        // YARP routes like "/catalog-service/{**catch-all}" have a static prefix
        // before the first template parameter. Strip template vars to get the stable
        // prefix for segment-aware matching against Refit routes.
        var gwPrefixes = new List<(GatewayRoute Route, string StaticPrefix, string RouteName)>();
        foreach (var gw in model.GatewayRoutes)
        {
            var staticPrefix = StripPathTemplateVariables(gw.UpstreamTemplate);
            if (staticPrefix.Length > 1) // at least "/x"
            {
                // Extract route-name label from last segment of static prefix
                var segments = staticPrefix.TrimEnd('/').Split('/');
                var label = segments.Length > 0 ? segments.Last() : "";
                gwPrefixes.Add((gw, staticPrefix, label));
            }
        }

        if (gwPrefixes.Count == 0) return;

        // Collect Refit interfaces and their owning projects
        var refitByProject = new Dictionary<string, List<RefitRouteDetection>>(StringComparer.OrdinalIgnoreCase);
        foreach (var rr in refitRoutes)
        {
            if (!scope.Contains(rr.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(rr.SourceFile)) continue;
            var proj = scope.ProjectForFile(rr.SourceFile) ?? "";
            if (proj.Length == 0) continue;
            if (!refitByProject.TryGetValue(proj, out var list))
                refitByProject[proj] = list = [];
            list.Add(rr);
        }

        // Index HTTP entries by project
        var httpEntriesByProject = new Dictionary<string, List<(string Route, string HandlerType, string File, int Line)>>(StringComparer.OrdinalIgnoreCase);
        foreach (var ep in model.Detections.OfType<EndpointDetection>())
        {
            if (!scope.Contains(ep.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(ep.SourceFile)) continue;
            var proj = scope.ProjectForFile(ep.SourceFile) ?? "";
            if (proj.Length == 0) continue;
            if (!httpEntriesByProject.TryGetValue(proj, out var list))
                httpEntriesByProject[proj] = list = [];
            list.Add((ep.RouteTemplate, ep.HandlerType, ep.SourceFile, 0));
        }

        // ── Match Refit routes → YARP gateway routes → downstream services ──
        foreach (var (clientProject, routes) in refitByProject)
        {
            foreach (var rr in routes)
            {
                // Strip query string and trailing slash for prefix matching
                var rawPath = rr.RouteTemplate;
                var qIdx = rawPath.IndexOf('?');
                var refitPath = (qIdx >= 0 ? rawPath[..qIdx] : rawPath).TrimEnd('/');
                if (refitPath.Length == 0) continue;

                foreach (var (gw, gwStaticPrefix, gwLabel) in gwPrefixes)
                {
                    // Segment-aware prefix match: Refit route must start with the entire
                    // static portion of the YARP path (minus template vars). Handle both
                    // "/catalog-service/{**catch-all}" and "/api/{version}/products/{**catch-all}".
                    if (!refitPath.StartsWith(gwStaticPrefix, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Validate segment boundary: after consuming the static prefix, the
                    // remainder must be empty (exact match), start with '/' (next segment),
                    // or be a continuation of the last segment (the prefix ended at a segment
                    // boundary with trailing '/', so any non-empty remainder is valid).
                    var afterPrefix = refitPath.AsSpan(gwStaticPrefix.Length);
                    if (afterPrefix.Length == 0) { /* exact match — valid */ }
                    else if (afterPrefix[0] == '/') { /* next path segment — valid */ }
                    else if (gwStaticPrefix.EndsWith('/'))
                    { /* prefix ends at segment boundary, next char starts the segment — valid */ }
                    else
                    {
                        // Prefix didn't end at a segment boundary (e.g. "/api" didn't match
                        // "/apiv2/endpoint" because "/apiv2" starts with "/api" but "v2" isn't
                        // a segment separator)
                        continue;
                    }

                    // ── Edge 1: client project → gateway project ──
                    var fromId = NodeId.ForService(clientProject);
                    var gwId = NodeId.ForService(gatewayProject!);
                    g.AddNode(new GraphNode(fromId, clientProject, NodeKind.Service));
                    g.AddNode(new GraphNode(gwId, gatewayProject!, NodeKind.Service));
                    g.AddEdge(new GraphEdge(fromId, gwId, EdgeKind.ServiceLink)
                    {
                        Provenance = $"{rr.SourceFile}:{rr.LineNumber}",
                        Resolution = Resolution.Join,
                        Confidence = 0.75f,
                        Tags = [ServiceLinkTags.HttpViaGateway],
                    });

                    // ── Edge 2: gateway project → downstream backend ──
                    var destAddr = gw.DownstreamHosts.ToLowerInvariant();
                    if (destAddr.Length > 0)
                    {
                        foreach (var (backendProject, httpEntries) in httpEntriesByProject)
                        {
                            if (string.Equals(backendProject, clientProject, StringComparison.OrdinalIgnoreCase))
                                continue;
                            if (string.Equals(backendProject, gatewayProject, StringComparison.OrdinalIgnoreCase))
                                continue;

                            var backendLower = backendProject.ToLowerInvariant();
                            if (destAddr.Contains(backendLower, StringComparison.Ordinal)
                                || destAddr.Contains(backendLower.Replace(".api", ""), StringComparison.Ordinal))
                            {
                                var beId = NodeId.ForService(backendProject);
                                g.AddNode(new GraphNode(beId, backendProject, NodeKind.Service));
                                g.AddEdge(new GraphEdge(gwId, beId, EdgeKind.ServiceLink)
                                {
                                    Provenance = $"{rr.SourceFile}:{rr.LineNumber}",
                                    Resolution = Resolution.Syntactic,
                                    Confidence = 0.65f,
                                    Tags = [ServiceLinkTags.HttpViaGateway],
                                });
                                break; // one backend match per gateway route
                            }
                        }
                    }

                    break; // one YARP route match per Refit route
                }
            }
        }
    }

    /// <summary>Strips template variables ({param}, {**catch-all}, {param:type}) from a
    /// URL path pattern, returning the static prefix up to (but not including) the first
    /// template variable. For YARP/Refit route matching (M1.8).</summary>
    private static string StripPathTemplateVariables(string path)
    {
        if (string.IsNullOrEmpty(path)) return "/";
        // Find the first template variable opening brace and slice before it
        var braceIdx = path.IndexOf('{');
        if (braceIdx < 0) return path.TrimEnd('/');
        if (braceIdx == 0) return "/";
        return path[..braceIdx].TrimEnd('/');
    }

    /// <summary>Resolves a graph node's owning project from its FilePath via the solution scope,
    /// falling back to the node's stored Project field.</summary>
    private static string? ResolveNodeProject(GraphNode node, SolutionScope scope)
    {
        if (node.FilePath is { } fp)
        {
            var proj = scope.ProjectForFile(fp);
            if (proj is not null) return proj;
        }
        return node.Project;
    }

    /// <summary>D9 — Detects layer violations by scanning graph edges for disallowed cross-layer
    /// references using archetype-dependent dependency rules.</summary>
    private static ImmutableArray<LayerViolation> DetectLayerViolations(CodeGraph graph, ArchitectureArchetype archetype)
    {
        var layers = new Dictionary<NodeId, string>();
        foreach (var n in graph.Nodes)
        {
            if (n.Layer is { } layer)
                layers[n.Id] = layer;
        }

        var illegal = archetype switch
        {
            ArchitectureArchetype.Library => new HashSet<(string, string)>(new[]
            {
                ("Internals", "PublicApi"),
            }),
            ArchitectureArchetype.Desktop => new HashSet<(string, string)>(new[]
            {
                ("Domain", "View"),
                ("Domain", "Platform"),
                ("Platform", "View"),
            }),
            // Web, Microservices, Gateway, and unknown default to clean-architecture rules
            _ => new HashSet<(string, string)>(new[]
            {
                ("Domain", "Infrastructure"),
                ("Domain", "Persistence"),
                ("Domain", "Presentation"),
                ("Domain", "Api"),
                ("Application", "Presentation"),
                ("Application", "Api"),
            }),
        };

        var result = ImmutableArray.CreateBuilder<LayerViolation>();
        var edgesSeen = new HashSet<(NodeId, NodeId, string)>();

        foreach (var n in graph.Nodes)
        {
            if (!layers.TryGetValue(n.Id, out var fromLayer)) continue;
            var outEdges = graph.OutEdges(n.Id);
            foreach (var e in outEdges)
            {
                if (!layers.TryGetValue(e.To, out var toLayer)) continue;
                if (fromLayer == toLayer) continue;
                if (!illegal.Contains((fromLayer, toLayer))) continue;
                var key = (e.From, e.To, fromLayer + "->" + toLayer);
                if (!edgesSeen.Add(key)) continue;
                result.Add(new LayerViolation(
                    e.From.ToString(), e.To.ToString(),
                    fromLayer, toLayer, e.Kind.ToString(), e.Provenance));
            }
        }

        return result.ToImmutable();
    }

    private static (HashSet<string> IntegrationTypes, HashSet<string> DomainTypes) BuildTypeEventSets(DiscoveryModel model)
    {
        var integrationTypes = new HashSet<string>(StringComparer.Ordinal);
        var domainTypes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var t in model.Types.Values)
        {
            foreach (var bt in t.BaseTypes)
            {
                var stripped = StripGenerics(bt);
                if (stripped.Contains("IntegrationEvent", StringComparison.OrdinalIgnoreCase)
                    || stripped is "INotification" or "IDomainEvent" or "IEvent")
                {
                    integrationTypes.Add(t.Name);
                    break;
                }
                if (stripped.Contains("DomainEvent", StringComparison.Ordinal))
                    domainTypes.Add(t.Name);
            }
        }
        foreach (var t in model.Types.Values)
        {
            if (t.Name.Contains("DomainEvent", StringComparison.Ordinal))
                domainTypes.Add(t.Name);
        }
        return (integrationTypes, domainTypes);
    }

    private static SeamContext BuildSeamContext(DiscoveryModel model, SolutionScope scope,
        IEnumerable<string> integrationEventTypes, IEnumerable<string> domainEventTypes,
        IEnumerable<string> knownEntities, IReadOnlyList<BodyFacts>? bodyFacts = null)
    {
        return new SeamContext
        {
            Symbols = new SymbolTable(model.Types.Values, scope.ProjectForFile, bodyFacts),
            KnownEntities = knownEntities.ToImmutableHashSet(StringComparer.Ordinal),
            IntegrationEventTypes = integrationEventTypes.ToImmutableHashSet(StringComparer.Ordinal),
            DomainEventTypes = domainEventTypes.ToImmutableHashSet(StringComparer.Ordinal),
        };
    }
}
