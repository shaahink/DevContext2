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

    /// <summary>Creates a graph builder with a symbol resolver (syntactic now, semantic in P3) and a noise filter.</summary>
    public GraphBuilder(ISymbolResolver resolver, NoiseFilter noise)
    {
        _resolver = resolver;
        _noise = noise;
    }

    /// <summary>Builds the code graph and the entry-point inventory for one solution scope (design-doc R1).</summary>
    public (CodeGraph Graph, ImmutableArray<EntryPoint> Entries) Build(DiscoveryModel model, SolutionScope scope)
    {
        var g = new CodeGraphBuilder();
        var names = new NameResolver(model.Types.Values); // short-name → FQN for every join (design-doc Q2)

        AddTypeNodes(g, model, scope);                      // worked example
        var entries = AddHttpEntryPoints(g, model, scope);  // worked example
        AddHandlerJoins(g, model, names, scope);            // worked example (Handles edge from MediatR detections)

        // ── P1 Map-facing seams ───────────────────────────────────────────
        AddEntityNodes(g, model, names, scope);             // B1: Entity nodes + aggregate tags
        AddEventConsumers(g, model, names, scope);          // B1: Event nodes + Consumes edges
        AddDiResolves(g, model, names, scope);              // B1: DI Resolves edges (interface → impl)

        // ── P2 Trace-facing seams ─────────────────────────────────────────
        AddRaises(g, model);                                // TODO(agent, P2)
        AddDataEdges(g, model);                             // TODO(agent, P2)
        AddCallEdges(g, model);                             // TODO(agent, P2 approx → P3 semantic)

        return (g.Build(), entries);
    }

    /// <summary>WORKED EXAMPLE — every in-scope production type becomes a TypeNode (noise filtered structurally).</summary>
    private void AddTypeNodes(CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope)
    {
        foreach (var type in model.Types.Values)
        {
            if (!_noise.IsProductionCode(type) || !scope.Contains(type.FilePath)) continue;
            g.AddNode(new GraphNode(NodeId.ForType(type.Id), type.Name, NodeKind.Type)
            {
                FilePath = type.FilePath,
            });
        }
    }

    /// <summary>WORKED EXAMPLE — in-scope HTTP endpoints become EntryPoint nodes + inventory entries.</summary>
    private static ImmutableArray<EntryPoint> AddHttpEntryPoints(CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        foreach (var ep in model.Detections.OfType<EndpointDetection>())
        {
            if (!scope.Contains(ep.SourceFile)) continue;
            var key = $"{ep.HttpMethod} {ep.RouteTemplate}";
            var id = NodeId.ForEntry(key);
            g.AddNode(new GraphNode(id, key, NodeKind.EntryPoint) { FilePath = ep.SourceFile });

            // TODO(agent, P2): link entry → its handler member (Calls edge) so the trace can step in;
            // then chase the dispatched request via AddCallEdges/AddSends.
            entries.Add(new EntryPoint(EntryPointKind.HttpEndpoint, key, id)
            {
                HttpMethod = ep.HttpMethod,
                Route = ep.RouteTemplate,
                Provenance = $"{ep.SourceFile}:{ep.LineNumber}",
            });
        }
        return entries.ToImmutable();
    }

    /// <summary>WORKED EXAMPLE — join MediatR detections into Request + Handler nodes and a Handles edge.
    /// Note the FQN resolution (<paramref name="names"/>): node keys are canonical even though detections
    /// carry short names. Every other join seam follows this exact pattern.</summary>
    private static void AddHandlerJoins(CodeGraphBuilder g, DiscoveryModel model, NameResolver names, SolutionScope scope)
    {
        foreach (var h in model.Detections.OfType<MediatRHandlerDetection>())
        {
            if (!scope.Contains(h.SourceFile)) continue;
            var requestId = NodeId.ForRequest(names.Resolve(h.RequestType));
            var handlerId = NodeId.ForHandler(names.Resolve(h.HandlerType));

            g.AddNode(new GraphNode(requestId, h.RequestType, NodeKind.Request)
            {
                Tags = [h.Kind.ToString().ToLowerInvariant()],
            });
            g.AddNode(new GraphNode(handlerId, h.HandlerType, NodeKind.Handler)
            {
                FilePath = h.SourceFile,
            });
            g.AddEdge(new GraphEdge(requestId, handlerId, EdgeKind.Handles)
            {
                Provenance = $"{h.SourceFile}:{h.LineNumber}",
                Resolution = Resolution.Join,
            });
            // TODO(agent, P2): add the Sends edge — the member that constructs + dispatches h.RequestType → requestId.
        }
    }

    // ── P1 Map-facing seams (B1) — JOIN detections into graph nodes/edges ────────────────────────

    /// <summary>B1: EfEntityDetection → Entity nodes + aggregate tags. (ReadsWrites edges are P2 C1.)</summary>
    private static void AddEntityNodes(CodeGraphBuilder g, DiscoveryModel model, NameResolver names, SolutionScope scope)
    {
        foreach (var e in model.Detections.OfType<EfEntityDetection>())
        {
            if (!scope.Contains(e.SourceFile)) continue;
            var entityId = NodeId.ForEntity(names.Resolve(e.EntityType));
            var tags = e.IsAggregate
                ? ImmutableArray.Create("aggregate")
                : ImmutableArray<string>.Empty;
            g.AddNode(new GraphNode(entityId, e.EntityType, NodeKind.Entity)
            {
                FilePath = e.SourceFile,
                Tags = tags,
            });
        }
    }

    /// <summary>B1: MediatR notification handlers + message bus consumers → Event nodes + Consumes edges.
    /// Domain events (INotificationHandler) and integration events (MessageConsumer) are unified as
    /// Event nodes; both feed into Handler nodes via Consumes edges.</summary>
    private static void AddEventConsumers(CodeGraphBuilder g, DiscoveryModel model, NameResolver names, SolutionScope scope)
    {
        // Notification handlers (domain events via MediatR)
        foreach (var h in model.Detections.OfType<MediatRHandlerDetection>())
        {
            if (h.Kind != MediatRKind.Notification) continue;
            if (!scope.Contains(h.SourceFile)) continue;
            var eventId = NodeId.ForEvent(names.Resolve(h.RequestType));
            var handlerId = NodeId.ForHandler(names.Resolve(h.HandlerType));

            g.AddNode(new GraphNode(eventId, h.RequestType, NodeKind.Event)
            {
                Tags = ["domain-event"],
            });
            g.AddNode(new GraphNode(handlerId, h.HandlerType, NodeKind.Handler)
            {
                FilePath = h.SourceFile,
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
            var eventId = NodeId.ForEvent(names.Resolve(mc.MessageType));
            var consumerType = names.Resolve(mc.ConsumerType);
            var handlerId = NodeId.ForHandler(consumerType);

            g.AddNode(new GraphNode(eventId, mc.MessageType, NodeKind.Event)
            {
                Tags = ["integration-event", mc.BusKind],
            });
            g.AddNode(new GraphNode(handlerId, mc.ConsumerType, NodeKind.Handler)
            {
                FilePath = mc.SourceFile,
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

            var svcFqn = names.Resolve(di.ServiceType);
            var implFqn = names.Resolve(di.ImplementationType);

            var svcNodeId = NodeId.ForType(svcFqn);
            var implNodeId = NodeId.ForService(implFqn);

            // Ensure both nodes exist
            if (!g.HasNode(svcNodeId))
                g.AddNode(new GraphNode(svcNodeId, di.ServiceType, NodeKind.Type));
            if (!g.HasNode(implNodeId))
                g.AddNode(new GraphNode(implNodeId, di.ImplementationType, NodeKind.Service));

            g.AddEdge(new GraphEdge(svcNodeId, implNodeId, EdgeKind.Resolves)
            {
                Provenance = $"{di.SourceFile}:{di.LineNumber}",
                Resolution = Resolution.Join,
            });
        }

        // Fallback: single-implementor interfaces not covered by DI registrations
        var diResolvedSvcIds = new HashSet<NodeId>();
        foreach (var di in model.Detections.OfType<DiRegistrationDetection>())
        {
            if (!scope.Contains(di.SourceFile)) continue;
            if (di.Shape != DiRegistrationShape.DirectBinding) continue;
            var svcFqn = names.Resolve(di.ServiceType);
            diResolvedSvcIds.Add(NodeId.ForType(svcFqn));
        }

        foreach (var (ifaceShort, implFqn) in singleImplMap)
        {
            var ifaceFqn = names.Resolve(ifaceShort);
            var svcNodeId = NodeId.ForType(ifaceFqn);
            var implNodeId = NodeId.ForService(implFqn);
            if (!g.HasNode(svcNodeId) || !g.HasNode(implNodeId)) continue;
            if (diResolvedSvcIds.Contains(svcNodeId)) continue; // already resolved via DI

            g.AddEdge(new GraphEdge(svcNodeId, implNodeId, EdgeKind.Resolves)
            {
                Resolution = Resolution.Syntactic,
                Confidence = 0.7f,
            });
        }
    }

    // ── P2 Trace-facing TODO seams — agent fills in C1 ─────────────────────────────────────────

    /// <summary>TODO(agent, P2 C1): scan handler/ctor bodies for AddDomainEvent(new X) / new XIntegrationEvent → Raises edges.</summary>
    private static void AddRaises(CodeGraphBuilder g, DiscoveryModel model) { _ = g; _ = model; }

    /// <summary>TODO(agent, P2 C1): EfEntityDetection + body usage → Entity/DataStore nodes + ReadsWrites edges.</summary>
    private static void AddDataEdges(CodeGraphBuilder g, DiscoveryModel model) { _ = g; _ = model; }

    /// <summary>TODO(agent, P2 C1 → P3 semantic): model.CallEdges resolved via ISymbolResolver → Calls edges.</summary>
    private void AddCallEdges(CodeGraphBuilder g, DiscoveryModel model) { _ = g; _ = model; _ = _resolver; }

    private static string StripGenerics(string typeName)
    {
        var idx = typeName.IndexOf('<');
        return idx > 0 ? typeName[..idx].TrimEnd() : typeName.TrimEnd();
    }
}
