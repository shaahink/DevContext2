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

        AddEventConsumers(g, model);                 // TODO(agent, P2)
        AddRaises(g, model);                         // TODO(agent, P2) — needs handler-body scan
        AddDiResolves(g, model);                     // TODO(agent, P2)
        AddDataEdges(g, model);                      // TODO(agent, P2)
        AddCallEdges(g, model);                      // TODO(agent, P2 approx → P3 semantic)

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

    // ── TODO seams — agent fills in P2; each is a JOIN over data we already capture ──────────────

    /// <summary>TODO(agent, P2): MessageConsumerDetection / EventFlowDetection → Event nodes + Consumes edges.</summary>
    private static void AddEventConsumers(CodeGraphBuilder g, DiscoveryModel model) { _ = g; _ = model; }

    /// <summary>TODO(agent, P2): scan handler/ctor bodies for AddDomainEvent(new X) / new XIntegrationEvent → Raises edges.</summary>
    private static void AddRaises(CodeGraphBuilder g, DiscoveryModel model) { _ = g; _ = model; }

    /// <summary>TODO(agent, P2): DiRegistrationDetection → Resolves(interface → impl) edges (via ISymbolResolver).</summary>
    private void AddDiResolves(CodeGraphBuilder g, DiscoveryModel model) { _ = g; _ = model; _ = _resolver; }

    /// <summary>TODO(agent, P2): EfEntityDetection + body usage → Entity/DataStore nodes + ReadsWrites edges.</summary>
    private static void AddDataEdges(CodeGraphBuilder g, DiscoveryModel model) { _ = g; _ = model; }

    /// <summary>TODO(agent, P2 approx → P3 semantic): model.CallEdges resolved via ISymbolResolver → Calls edges.</summary>
    private void AddCallEdges(CodeGraphBuilder g, DiscoveryModel model) { _ = g; _ = model; _ = _resolver; }
}
