using System.Text.RegularExpressions;

using DevContext.Core.Models;

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
        var entries = AddHttpEntryPoints(g, model, scope, names)  // worked example
            .AddRange(AddWorkerEntryPoints(g, model, scope, names)); // hosted services + scheduled jobs (DntSite audit)
        AddHandlerJoins(g, model, names, scope);            // worked example (Handles edge from MediatR detections)
        AddPipelineBehaviors(g, model, names, scope);       // B3: IPipelineBehavior → WrappedBy edges

        // ── P1 Map-facing seams ───────────────────────────────────────────
        AddEntityNodes(g, model, names, scope);             // B1: Entity nodes + aggregate tags
        AddEventConsumers(g, model, names, scope);          // B1: Event nodes + Consumes edges
        AddDiResolves(g, model, names, scope);              // B1: DI Resolves edges (interface → impl)

        // ── P2 Trace-facing seams ─────────────────────────────────────────
        AddRaises(g, model, names);                         // C1: Raises edges from body scan
        AddSends(g, model, names);                          // C1: Sends edges from .Send(new X())
        AddDataEdges(g, model, names);                      // C1: ReadsWrites edges from entities
        AddCallEdges(g, model, names);                      // C1: Calls edges from CallEdges

        var graph = g.Build();
        return (graph, EnrichEntryTargets(graph, entries));
    }

    /// <summary>After the graph is assembled, resolve each entry's dispatch target (the command it
    /// sends or the handler it invokes) so the Map and the desktop picker can show "route → Target".
    /// The Sends/Handles edges don't exist yet when entries are first created, so this runs last.</summary>
    private static ImmutableArray<EntryPoint> EnrichEntryTargets(CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        if (entries.IsDefaultOrEmpty) return entries;
        var b = ImmutableArray.CreateBuilder<EntryPoint>(entries.Length);
        foreach (var e in entries)
            b.Add(e with { Target = ResolveEntryTarget(graph, e.Node) });
        return b.ToImmutable();
    }

    /// <summary>Resolves an entry's primary target by stepping one hop down its Calls edge: a named
    /// handler method/class is the target; a type that dispatches exactly one request shows that
    /// command; an ambiguous registration class (many sends — minimal APIs) yields null (the Trace
    /// disambiguates per-endpoint). Truthful by construction — no guess when the owner fans out.</summary>
    private static string? ResolveEntryTarget(CodeGraph graph, NodeId entryNode)
    {
        // Unresolved (FastEndpoints Configure()-set) routes collapse to a single "<dynamic>" node, so
        // every such entry would resolve to whichever endpoint won that bucket — misleading. Skip them.
        if (entryNode.Key.Contains("<dynamic>", StringComparison.Ordinal)) return null;

        foreach (var call in graph.OutEdges(entryNode, EdgeKind.Calls))
        {
            var node = graph.Node(call.To);
            if (node is null) continue;
            switch (node.Kind)
            {
                case NodeKind.Member:
                    // G5: a per-endpoint minimal-API lambda node resolves its target through its own
                    // Sends edge (like a Type) — "route → Command" when it dispatches exactly one,
                    // else null. Its synthetic "<lambda> …" title must never surface as the target.
                    if (node.Title.StartsWith("<lambda>", StringComparison.Ordinal))
                    {
                        var lsends = graph.OutEdges(node.Id, EdgeKind.Sends)
                            .Select(s => s.To).Distinct().ToList();
                        return lsends.Count == 1 ? graph.Node(lsends[0])?.Title : null;
                    }
                    // "CreateEndpoint.HandleAsync" → "CreateEndpoint" (the handler class).
                    var dot = node.Title.LastIndexOf('.');
                    return dot > 0 ? node.Title[..dot] : node.Title;
                case NodeKind.Handler:
                    return node.Title;
                case NodeKind.Type:
                    var sends = graph.OutEdges(node.Id, EdgeKind.Sends)
                        .Select(s => s.To).Distinct().ToList();
                    if (sends.Count == 1)
                        return graph.Node(sends[0])?.Title;
                    // 0 or >1 sends → ambiguous; leave the route to speak for itself.
                    return null;
            }
        }
        return null;
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
                SourceBody = type.SourceBody,
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
                var methodName = ep.HandlerMethod;
                var hasSpecificMethod = !string.IsNullOrEmpty(methodName)
                    && methodName is not "<lambda>" and not "<anonymous>"
                    && !methodName.Contains("=>", StringComparison.Ordinal);

                if (hasSpecificMethod && g.HasNode(NodeId.ForType(handlerFqn)))
                {
                    // B4: Anchor on the specific handler method via a Member node
                    var memberNodeId = NodeId.ForMember(handlerFqn, methodName);
                    var typeNode = g.Nodes.FirstOrDefault(n => n.Id.Key == handlerFqn);
                    g.AddNode(new GraphNode(memberNodeId, ep.HandlerType + "." + methodName, NodeKind.Member)
                    {
                        FilePath = ep.SourceFile,
                        SourceBody = typeNode?.SourceBody,
                    });
                    g.AddEdge(new GraphEdge(id, memberNodeId, EdgeKind.Calls)
                    {
                        Provenance = $"{ep.SourceFile}:{ep.LineNumber}",
                        Resolution = Resolution.Join,
                    });
                    linked = true;
                }
                else
                {
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
            }

            if (!linked)
            {
                var ownerType = model.Types.Values.FirstOrDefault(t =>
                    string.Equals(t.FilePath, ep.SourceFile, StringComparison.OrdinalIgnoreCase));

                // G5: an inline minimal-API lambda gets its OWN node carrying that route's body, keyed by
                // verb+route, so the trace from this route shows this lambda — not the shared registration
                // type that every Map{Verb} in the file would otherwise collapse onto.
                if (isLambdaHandler && !string.IsNullOrEmpty(ep.HandlerBody))
                {
                    var ownerKey = ownerType?.Id ?? Path.GetFileNameWithoutExtension(ep.SourceFile);
                    var lambdaId = NodeId.ForMember(ownerKey, $"<lambda> {key}");
                    g.AddNode(new GraphNode(lambdaId, $"<lambda> {key}", NodeKind.Member)
                    {
                        FilePath = ep.SourceFile,
                        SourceBody = ep.HandlerBody,
                    });
                    g.AddEdge(new GraphEdge(id, lambdaId, EdgeKind.Calls)
                    {
                        Provenance = $"{ep.SourceFile}:{(ep.HandlerLine > 0 ? ep.HandlerLine : ep.LineNumber)}",
                        Resolution = Resolution.Join,
                    });
                    AddLambdaOutEdges(g, lambdaId, ep, names);
                    linked = true;
                }
                else if (ownerType is not null)
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

    /// <summary>Hosted services + scheduled jobs (incl. DNTScheduler <c>AddScheduledTask&lt;T&gt;</c>)
    /// become entry points so the Map's inventory and the Trace can start from them — not just HTTP.
    /// Anchored on the worker's implementation type; deduped by short type name. (DntSite audit: 24 jobs
    /// were detected but never surfaced as entries.)</summary>
    private static ImmutableArray<EntryPoint> AddWorkerEntryPoints(CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope, NameResolver names)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var bw in model.Detections.OfType<BackgroundWorkerDetection>())
        {
            if (!scope.Contains(bw.SourceFile)) continue;
            var impl = bw.ImplementationType;
            if (string.IsNullOrEmpty(impl) || impl == "?") continue;
            var shortName = impl.Contains('.') ? impl[(impl.LastIndexOf('.') + 1)..] : impl;
            if (!seen.Add(shortName)) continue;

            var kind = bw.Kind == BackgroundWorkerKind.TimedJob
                || string.Equals(bw.ServiceType, "DNTScheduler", StringComparison.OrdinalIgnoreCase)
                ? EntryPointKind.ScheduledJob
                : EntryPointKind.HostedService;

            var id = NodeId.ForEntry($"worker:{shortName}");
            g.AddNode(new GraphNode(id, shortName, NodeKind.EntryPoint) { FilePath = bw.SourceFile });

            var typeId = NodeId.ForType(names.Resolve(shortName));
            if (g.HasNode(typeId))
                g.AddEdge(new GraphEdge(id, typeId, EdgeKind.Calls)
                {
                    Provenance = $"{bw.SourceFile}:{bw.LineNumber}",
                    Resolution = Resolution.Join,
                });

            entries.Add(new EntryPoint(kind, shortName, id) { Provenance = $"{bw.SourceFile}:{bw.LineNumber}" });
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
                SourceBody = model.Types.Values
                    .FirstOrDefault(t => t.Id == names.Resolve(h.HandlerType))
                    ?.SourceBody,
            });
            g.AddEdge(new GraphEdge(requestId, handlerId, EdgeKind.Handles)
            {
                Provenance = $"{h.SourceFile}:{h.LineNumber}",
                Resolution = Resolution.Join,
            });
            // TODO(agent, P2): add the Sends edge — the member that constructs + dispatches h.RequestType → requestId.
        }
    }

    /// <summary>B3: Detects IPipelineBehavior registrations from DI detections and creates
    /// WrappedBy edges from every Request node to each pipeline behavior. The trace renders
    /// pipeline behaviors as a "pipeline" seam under the first send that reaches a Request.</summary>
    private static void AddPipelineBehaviors(CodeGraphBuilder g, DiscoveryModel model, NameResolver names, SolutionScope scope)
    {
        var behaviors = new HashSet<(string BehaviorType, string? SourceFile, int? LineNumber)>();

        foreach (var di in model.Detections.OfType<DiRegistrationDetection>())
        {
            if (!scope.Contains(di.SourceFile)) continue;

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
        }

        foreach (var (behaviorType, file, line) in behaviors)
        {
            var behaviorFqn = names.Resolve(behaviorType);
            var behaviorNodeId = NodeId.ForService(behaviorFqn);
            g.AddNode(new GraphNode(behaviorNodeId, behaviorType, NodeKind.Service)
            {
                FilePath = file,
                Tags = ["pipeline"],
                SourceBody = model.Types.Values
                    .FirstOrDefault(t => t.Id == behaviorFqn)?.SourceBody,
            });

            // WrappedBy edge from every Request node to this pipeline behavior
            foreach (var node in g.Nodes.Where(n => n.Kind == NodeKind.Request))
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
            var handlerId = NodeId.ForHandler(type.Id);

            foreach (var method in eventMethods)
            {
                foreach (Match match in Regex.Matches(body,
                    $@"{Regex.Escape(method)}\s*\(\s*new\s+(\w+)\s*\(", RegexOptions.Compiled))
                {
                    var eventName = match.Groups[1].Value;
                    if (IsNoiseType(eventName)) continue;
                    var eventFqn = names.Resolve(eventName);
                    var eventId = NodeId.ForEvent(eventFqn);
                    if (!g.HasNode(typeId)) continue;

                    g.AddNode(new GraphNode(eventId, eventName, NodeKind.Event));
                    var prov = EstimateProvenance(body, match.Index, type.FilePath);
                    g.AddEdge(new GraphEdge(typeId, eventId, EdgeKind.Raises)
                    {
                        Provenance = prov,
                        Resolution = Resolution.Syntactic,
                        Confidence = 0.5f,
                    });
                    // B5: Mirror to Handler node so trace can cross from handler → event
                    if (g.HasNode(handlerId))
                    {
                        g.AddEdge(new GraphEdge(handlerId, eventId, EdgeKind.Raises)
                        {
                            Provenance = prov,
                            Resolution = Resolution.Syntactic,
                            Confidence = 0.5f,
                        });
                    }
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
                var prov = EstimateProvenance(body, match.Index, type.FilePath);
                g.AddEdge(new GraphEdge(typeId, eventId, EdgeKind.Raises)
                {
                    Provenance = prov,
                    Resolution = Resolution.Syntactic,
                    Confidence = 0.5f,
                });
                // B5: Mirror to Handler node
                if (g.HasNode(handlerId))
                {
                    g.AddEdge(new GraphEdge(handlerId, eventId, EdgeKind.Raises)
                    {
                        Provenance = prov,
                        Resolution = Resolution.Syntactic,
                        Confidence = 0.5f,
                    });
                }
            }
        }
    }

    /// <summary>C1: Scan bodies for MediatR Send/Publish dispatch → Sends edges.
    /// Per R4: matches .Send/.SendAsync/.Publish/.PublishAsync where the receiver is a mediator
    /// field/property, with a new TRequest(...) inline arg or a local-variable arg.
    /// Creates Sends edge from the calling type to the request node.</summary>
    /// <summary>G5: best-effort out-edges for a per-endpoint minimal-API lambda node. Scans the lambda's
    /// own body for MediatR Send/Publish dispatch (same pattern as <see cref="AddSends"/>) so the trace
    /// from this route shows ITS send target, anchored on the lambda — not the registration type.</summary>
    private static void AddLambdaOutEdges(CodeGraphBuilder g, NodeId fromId, EndpointDetection ep, NameResolver names)
    {
        if (ep.HandlerBody is not { Length: > 0 } body) return;

        foreach (Match match in Regex.Matches(body,
            @"\.(Send|SendAsync|Publish|PublishAsync)\s*\(\s*(?:new\s+(\w+)\s*\(|(\w+))",
            RegexOptions.Compiled))
        {
            string? requestName;
            if (match.Groups[2].Success)
            {
                requestName = match.Groups[2].Value;
            }
            else
            {
                var pos = match.Index;
                if (pos <= 0) continue;
                var newMatches = Regex.Matches(body[..pos], @"new\s+(\w+)\s*[\(;]");
                if (newMatches.Count == 0) continue;
                requestName = newMatches[^1].Groups[1].Value;
            }

            if (string.IsNullOrEmpty(requestName) || IsNoiseType(requestName)) continue;
            var requestId = NodeId.ForRequest(names.Resolve(requestName));
            g.AddNode(new GraphNode(requestId, requestName, NodeKind.Request));
            g.AddEdge(new GraphEdge(fromId, requestId, EdgeKind.Sends)
            {
                Provenance = $"{ep.SourceFile}:{(ep.HandlerLine > 0 ? ep.HandlerLine : ep.LineNumber)}",
                Resolution = Resolution.Syntactic,
                Confidence = 0.55f,
            });
        }
    }

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

                if (string.IsNullOrEmpty(requestName) || IsNoiseType(requestName)) continue;
                var requestFqn = names.Resolve(requestName);
                var requestId = NodeId.ForRequest(requestFqn);
                if (!g.HasNode(typeId)) continue;

                // Approximate provenance: file + line of the Send call
                var prov = EstimateProvenance(body, match.Index, type.FilePath);

                g.AddNode(new GraphNode(requestId, requestName, NodeKind.Request));
                g.AddEdge(new GraphEdge(typeId, requestId, EdgeKind.Sends)
                {
                    Provenance = prov,
                    Resolution = Resolution.Syntactic,
                    Confidence = 0.55f,
                });
            }
        }
    }

    /// <summary>True for names the body-scan Sends/Raises regexes pick up but that are never a real
    /// request/event — chiefly framework exceptions (<c>new ArgumentNullException(...)</c> caught by the
    /// variable-arg .Send() heuristic). Keeps the trace's indirection seams honest.</summary>
    private static bool IsNoiseType(string name)
        => name.EndsWith("Exception", StringComparison.Ordinal)
            || name is "Task" or "ValueTask" or "List" or "Dictionary" or "Array"
                or "String" or "Object" or "Guid" or "CancellationToken";

    /// <summary>Estimates a "file:line" provenance from a character offset in the source body.</summary>
    private static string? EstimateProvenance(string sourceBody, int charOffset, string? filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return null;
        var line = 1;
        for (var i = 0; i < Math.Min(charOffset, sourceBody.Length); i++)
        {
            if (sourceBody[i] == '\n') line++;
        }
        return $"{filePath}:{line}";
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
