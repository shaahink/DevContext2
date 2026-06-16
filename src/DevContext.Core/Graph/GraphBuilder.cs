using System.Text.RegularExpressions;

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
        var entries = AddHttpEntryPoints(g, model, scope, names);  // worked example
        AddHandlerJoins(g, model, names, scope);            // worked example (Handles edge from MediatR detections)

        // ── P1 Map-facing seams ───────────────────────────────────────────
        AddEntityNodes(g, model, names, scope);             // B1: Entity nodes + aggregate tags
        AddEventConsumers(g, model, names, scope);          // B1: Event nodes + Consumes edges
        AddDiResolves(g, model, names, scope);              // B1: DI Resolves edges (interface → impl)

        // ── P2 Trace-facing seams ─────────────────────────────────────────
        AddRaises(g, model, names);                         // C1: Raises edges from body scan
        AddSends(g, model, names);                          // C1: Sends edges from .Send(new X())
        AddDataEdges(g, model, names);                      // C1: ReadsWrites edges from entities
        AddCallEdges(g, model, names);                      // C1: Calls edges from CallEdges

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

    /// <summary>WORKED EXAMPLE — in-scope HTTP endpoints become EntryPoint nodes + inventory entries.
    /// Links entry → handler type/class with a Calls edge so the trace can step out from the entry.</summary>
    private static ImmutableArray<EntryPoint> AddHttpEntryPoints(CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope, NameResolver names)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        foreach (var ep in model.Detections.OfType<EndpointDetection>())
        {
            if (!scope.Contains(ep.SourceFile)) continue;
            var key = $"{ep.HttpMethod} {ep.RouteTemplate}";
            var id = NodeId.ForEntry(key);
            g.AddNode(new GraphNode(id, key, NodeKind.EntryPoint) { FilePath = ep.SourceFile });

            // Link entry → its handler so the trace has a first hop down the wiring. Two shapes:
            //   1. Named handler (controller action, FastEndpoints class): HandlerType is a real type
            //      name — resolve it to a Type node.
            //   2. Lambda/inline handler (minimal API): HandlerType is the lambda's *source text* (it
            //      contains "=>") and HandlerMethod is "<lambda>" — there is no named type, so fall back
            //      to the type that *contains* the endpoint registration (matched by file).
            // NOTE: the previous code keyed the lambda branch off HandlerType == "λ", which minimal-API
            // endpoints never produce — so lambda entries got no out-edge and every trace was empty.
            var isLambdaHandler = ep.HandlerMethod is "<lambda>" or "<anonymous>"
                || string.IsNullOrEmpty(ep.HandlerType)
                || ep.HandlerType is "λ" or "?"
                || ep.HandlerType.Contains("=>", StringComparison.Ordinal);

            var linked = false;
            if (!isLambdaHandler)
            {
                var handlerFqn = names.Resolve(ep.HandlerType);
                var handlerNodeId = NodeId.ForType(handlerFqn);
                if (g.HasNode(handlerNodeId))
                {
                    g.AddEdge(new GraphEdge(id, handlerNodeId, EdgeKind.Calls)
                    {
                        Provenance = $"{ep.SourceFile}:{ep.LineNumber}",
                        Resolution = Resolution.Join,
                    });
                    linked = true;
                }
            }

            if (!linked)
            {
                var ownerType = model.Types.Values.FirstOrDefault(t =>
                    string.Equals(t.FilePath, ep.SourceFile, StringComparison.OrdinalIgnoreCase));
                if (ownerType is not null)
                {
                    var ownerNodeId = NodeId.ForType(ownerType.Id);
                    if (g.HasNode(ownerNodeId))
                    {
                        g.AddEdge(new GraphEdge(id, ownerNodeId, EdgeKind.Calls)
                        {
                            Provenance = $"{ep.SourceFile}:{ep.LineNumber}",
                            Resolution = Resolution.Join,
                        });
                    }
                }
            }

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

    // ── P2 Trace-facing seams (C1) — joins that complete the indirection-bridged trace ─────────

    /// <summary>C1: model.CallEdges → Type→Type Calls edges, but ONLY between types that are real nodes
    /// in the graph (in-scope solution types). The syntactic call graph emits a callee per invocation,
    /// many of which are local variables, fluent-chain fragments, or framework methods (e.g. "group",
    /// "pb", "AsNoTracking()"); materializing those as phantom nodes floods the trace with noise. By
    /// requiring both endpoints to already exist, the trace keeps only edges to types we actually know.
    /// Resolution flows through from the edge (semantic → [verified], syntactic → [approx]).</summary>
    private static void AddCallEdges(CodeGraphBuilder g, DiscoveryModel model, NameResolver names)
    {
        foreach (var ce in model.CallEdges)
        {
            var callerId = NodeId.ForType(names.Resolve(ce.CallerType));
            var calleeId = NodeId.ForType(names.Resolve(ce.CalleeType));

            if (callerId == calleeId) continue;                              // skip self-calls
            if (!g.HasNode(callerId) || !g.HasNode(calleeId)) continue;      // real solution types only

            g.AddEdge(new GraphEdge(callerId, calleeId, EdgeKind.Calls)
            {
                Provenance = ce.CallSiteLocation,
                Resolution = ce.Resolution,
                Confidence = ce.Resolution == Resolution.Semantic ? 0.95f : 0.6f,
            });
        }
    }

    /// <summary>C1: Link EF entities to their handler types via body references + DbContext info.
    /// For each entity detection, find handler types whose SourceBody references the entity name,
    /// and add ReadsWrites edges from the handler type to the entity.</summary>
    private static void AddDataEdges(CodeGraphBuilder g, DiscoveryModel model, NameResolver names)
    {
        var entityNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var e in model.Detections.OfType<EfEntityDetection>())
        {
            var entityFqn = names.Resolve(e.EntityType);
            var entityId = NodeId.ForEntity(entityFqn);
            var ctxFqn = names.Resolve(e.DbContextType);
            var ctxType = ctxFqn;
            if (!string.IsNullOrEmpty(ctxType) && ctxType != "?")
            {
                var ctxId = NodeId.ForType(ctxType);
                g.AddNode(new GraphNode(ctxId, ctxType, NodeKind.DataStore)
                {
                    FilePath = e.SourceFile,
                });
                g.AddEdge(new GraphEdge(entityId, ctxId, EdgeKind.ReadsWrites)
                {
                    Provenance = $"{e.SourceFile}:{e.LineNumber}",
                    Resolution = Resolution.Join,
                });
            }
            entityNames.Add(RemoveGenerics(e.EntityType));
            entityNames.Add(RemoveGenerics(entityFqn));
        }

        // Link handler/consumer types that reference entities in their bodies
        foreach (var type in model.Types.Values)
        {
            if (type.SourceBody is not { Length: > 0 } body) continue;
            foreach (var entityName in entityNames)
            {
                if (!body.Contains(entityName, StringComparison.Ordinal)) continue;
                var typeId = NodeId.ForType(type.Id);
                var entityId = NodeId.ForEntity(entityName);
                if (!g.HasNode(typeId) || !g.HasNode(entityId)) continue;

                g.AddEdge(new GraphEdge(typeId, entityId, EdgeKind.ReadsWrites)
                {
                    Resolution = Resolution.Syntactic,
                    Confidence = 0.5f,
                });
                break;
            }
        }
    }

    /// <summary>C1: Scan handler/ctor SourceBody for domain/integration event creation → Raises edges.
    /// Per R4: matches method-name set {AddDomainEvent, RaiseDomainEvent, AddEvent} with new TEvent()
    /// arg; also new TIntegrationEvent(...) constructor calls. Returns Raises edges from the type
    /// (or its handler node) to the Event node. Mark Resolution.Syntactic.</summary>
    private static void AddRaises(CodeGraphBuilder g, DiscoveryModel model, NameResolver names)
    {
        var eventMethods = new[] { "AddDomainEvent", "RaiseDomainEvent", "AddEvent" };
        foreach (var type in model.Types.Values)
        {
            if (type.SourceBody is not { Length: > 0 } body) continue;
            var typeId = NodeId.ForType(type.Id);

            foreach (var method in eventMethods)
            {
                foreach (Match match in Regex.Matches(body,
                    $@"{Regex.Escape(method)}\s*\(\s*new\s+(\w+)\s*\(", RegexOptions.Compiled))
                {
                    var eventName = match.Groups[1].Value;
                    var eventFqn = names.Resolve(eventName);
                    var eventId = NodeId.ForEvent(eventFqn);
                    if (!g.HasNode(typeId)) continue;

                    g.AddNode(new GraphNode(eventId, eventName, NodeKind.Event));
                    g.AddEdge(new GraphEdge(typeId, eventId, EdgeKind.Raises)
                    {
                        Resolution = Resolution.Syntactic,
                        Confidence = 0.5f,
                    });
                }
            }

            // new TIntegrationEvent(...) — constructor calls for integration events
            foreach (Match match in Regex.Matches(body,
                @"new\s+(\w*IntegrationEvent\w*)\s*\(", RegexOptions.Compiled))
            {
                var eventName = match.Groups[1].Value;
                var eventFqn = names.Resolve(eventName);
                var eventId = NodeId.ForEvent(eventFqn);
                if (!g.HasNode(typeId)) continue;

                g.AddNode(new GraphNode(eventId, eventName, NodeKind.Event)
                {
                    Tags = ["integration-event"],
                });
                g.AddEdge(new GraphEdge(typeId, eventId, EdgeKind.Raises)
                {
                    Resolution = Resolution.Syntactic,
                    Confidence = 0.5f,
                });
            }
        }
    }

    /// <summary>C1: Scan bodies for MediatR Send/Publish dispatch → Sends edges.
    /// Per R4: matches .Send/.SendAsync/.Publish/.PublishAsync where the receiver is a mediator
    /// field/property, with a new TRequest(...) inline arg or a local-variable arg.
    /// Creates Sends edge from the calling type to the request node.</summary>
    private static void AddSends(CodeGraphBuilder g, DiscoveryModel model, NameResolver names)
    {
        foreach (var type in model.Types.Values)
        {
            if (type.SourceBody is not { Length: > 0 } body) continue;
            var typeId = NodeId.ForType(type.Id);

            // Find all Send/Publish calls with either inline `new T()` or a variable arg.
            // Pattern: .Send(expr) where expr is either `new Type(...)` or a local name.
            foreach (Match match in Regex.Matches(body,
                @"\.(Send|SendAsync|Publish|PublishAsync)\s*\(\s*(?:new\s+(\w+)\s*\(|(\w+))",
                RegexOptions.Compiled))
            {
                string? requestName;
                if (match.Groups[2].Success)
                {
                    // Inline: .Send(new CreateOrderCommand(...))
                    requestName = match.Groups[2].Value;
                }
                else
                {
                    // Variable: .Send(cmd) — try to find `cmd` assignment via `new T()` before this call
                    var varName = match.Groups[3].Value;
                    var pos = match.Index;
                    if (pos <= 0) continue;
                    var before = body[..pos];
                    // Find `new XType ` occurring before this Send, closest to the call
                    var newMatches = Regex.Matches(before, @"new\s+(\w+)\s*[\(;]");
                    if (newMatches.Count == 0) continue;
                    // Pick the last `new XType` before the Send call as the likely request type
                    requestName = newMatches[^1].Groups[1].Value;
                }

                if (string.IsNullOrEmpty(requestName)) continue;
                var requestFqn = names.Resolve(requestName);
                var requestId = NodeId.ForRequest(requestFqn);
                if (!g.HasNode(typeId)) continue;

                g.AddNode(new GraphNode(requestId, requestName, NodeKind.Request));
                g.AddEdge(new GraphEdge(typeId, requestId, EdgeKind.Sends)
                {
                    Resolution = Resolution.Syntactic,
                    Confidence = 0.55f,
                });
            }
        }
    }

    private static string RemoveGenerics(string typeName)
    {
        var idx = typeName.IndexOf('<');
        return idx > 0 ? typeName[..idx].TrimEnd() : typeName.TrimEnd();
    }

    private static string StripGenerics(string typeName)
    {
        var idx = typeName.IndexOf('<');
        return idx > 0 ? typeName[..idx].TrimEnd() : typeName.TrimEnd();
    }
}
