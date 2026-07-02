using System.Text.RegularExpressions;

using DevContext.Core.Graph.Seams;
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
    public (CodeGraph Graph, ImmutableArray<EntryPoint> Entries) Build(DiscoveryModel model, SolutionScope scope)
    {
        var g = new CodeGraphBuilder();
        var names = new NameResolver(model.Types.Values); // short-name → FQN for every join (design-doc Q2)

        AddTypeNodes(g, model, scope);                      // worked example

        // ── P3: Entry-point builders (one per kind) ──────────────────────────
        // Open to extension — add a new builder for Blazor/gRPC/SignalR without
        // modifying GraphBuilder itself.
        var entries = ImmutableArray<EntryPoint>.Empty;
        foreach (var builder in _entryBuilders)
            entries = entries.AddRange(builder.Build(g, model, scope, names, _noise));

        AddHandlerJoins(g, model, names, scope, _noise);            // worked example (Handles edge from MediatR detections)
        AddPipelineBehaviors(g, model, names, scope, _noise);       // B3: IPipelineBehavior → WrappedBy edges

        // ── P1 Map-facing seams ───────────────────────────────────────────
        AddEntityNodes(g, model, names, scope, _noise);             // B1: Entity nodes + aggregate tags
        AddEventConsumers(g, model, names, scope, _noise);          // B1: Event nodes + Consumes edges
        AddDiResolves(g, model, names, scope);              // B1: DI Resolves edges (interface → impl)

        // ── P2 Trace-facing seams ─────────────────────────────────────────
        // Member-origin correctness (Iteration 1 / Phase 1): edges that originate in a method body must
        // originate from that method's Member node, not the whole Type — otherwise a trace anchored on
        // one method inherits every sibling method's edges. Precompute, once, a per-type offset→method
        // locator from each type's SourceBody so the body-scan seams below attribute each match to the
        // method that contains it. (See PRODUCT-DIRECTION.md §6 req.1.)
        var methodSpans = BuildAllMethodSpans(model);
        AddRaises(g, model, names, methodSpans);            // C1: Raises edges from body scan (member-origin)
        AddSends(g, model, names, methodSpans);             // C1: Sends edges from .Send(new X()) (member-origin)
        AddDataEdges(g, model, names, methodSpans);         // C1: ReadsWrites edges from entities (member-origin)
        AddCallEdges(g, model, names);                      // C1: Calls edges from CallEdges (member→member)

        var graph = g.Build();
        return (graph, EnrichEntryTargets(graph, entries));
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
    /// type — honest (it's the declaring type) and more useful than a blank drill-in hint (W8).</summary>
    private static string? ResolveOwningTypeFallback(CodeGraph graph, EntryPoint entry)
    {
        if (entry.HandlerNode is not { } hn) return null;
        var handler = graph.Node(hn);
        if (handler is null) return null;

        if (handler.Kind == NodeKind.Type)
            return handler.Title;

        if (handler.Kind == NodeKind.Member)
        {
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
    /// action) to the primary service it calls: the dominant in-scope callee of the action <b>member</b>.
    /// Prefers a DI-resolved <c>service</c>-tagged callee, else the first in-scope, non-self, non-framework
    /// callee. Returns its title (member form, e.g. "ProductService.GetByIdAsync"), or null when the action
    /// calls nothing meaningful — honest, never guessed via the whole class (member-origin made the action's
    /// own Calls edges precise, so the old <c>ResolveViaParentType</c> whole-type crutch is retired).</summary>
    private static string? ResolvePrimaryCall(CodeGraph graph, GraphNode member)
    {
        var ownerTypeKey = ExtractTypeKey(member.Id.Key);
        GraphNode? firstInScope = null;
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

            // Prefer a DI-resolved service (the action's real collaborator); else remember the first
            // in-scope callee as a fallback.
            if (calleeType.Tags.Contains(RoleTags.Service))
                return callee.Title;
            firstInScope ??= callee;
        }
        return firstInScope?.Title;
    }

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


    /// <summary>WORKED EXAMPLE — join MediatR detections into Request + Handler nodes and a Handles edge.
    /// Note the FQN resolution (<paramref name="names"/>): node keys are canonical even though detections
    /// carry short names. Every other join seam follows this exact pattern.</summary>
    private static void AddHandlerJoins(CodeGraphBuilder g, DiscoveryModel model, NameResolver names, SolutionScope scope, NoiseFilter noise)
    {
        foreach (var h in model.Detections.OfType<MediatRHandlerDetection>())
        {
            if (!scope.Contains(h.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(h.SourceFile)) continue;
            var requestId = NodeId.ForType(names.Resolve(h.RequestType));
            var handlerId = NodeId.ForType(names.Resolve(h.HandlerType));

            g.AddNode(new GraphNode(requestId, h.RequestType, NodeKind.Type)
            {
                Tags = [h.Kind.ToString().ToLowerInvariant()],
            });
            g.AddNode(new GraphNode(handlerId, h.HandlerType, NodeKind.Type)
            {
                FilePath = h.SourceFile,
                Tags = [RoleTags.Handler],
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
                foreach (Match m in Regex.Matches(body,
                    @"AddOpenBehavior\s*\(\s*typeof\s*\(\s*(\w+)",
                    RegexOptions.Compiled))
                {
                    var name = m.Groups[1].Value;
                    if (name is { Length: > 0 } && name != "?")
                        behaviors.Add((name, di.SourceFile, di.LineNumber));
                }
            }
        }

        foreach (var (behaviorType, file, line) in behaviors)
        {
            var behaviorFqn = names.Resolve(behaviorType);
            var behaviorNodeId = NodeId.ForType(behaviorFqn);
            g.AddNode(new GraphNode(behaviorNodeId, behaviorType, NodeKind.Type)
            {
                FilePath = file,
                Tags = [RoleTags.Service, RoleTags.Pipeline],
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
            var entityId = NodeId.ForType(names.Resolve(e.EntityType));
            var tags = e.IsAggregate
                ? ImmutableArray.Create(RoleTags.Entity, RoleTags.Aggregate)
                : ImmutableArray.Create(RoleTags.Entity);
            g.AddNode(new GraphNode(entityId, e.EntityType, NodeKind.Type)
            {
                FilePath = e.SourceFile,
                Tags = tags,
            });
            knownEntityFqns.Add(names.Resolve(e.EntityType));
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
                if (knownEntityFqns.Contains(names.Resolve(bt)))
                {
                    g.AddNode(new GraphNode(NodeId.ForType(type.Id), type.Name, NodeKind.Type)
                    {
                        FilePath = type.FilePath,
                        Tags = [RoleTags.Entity],
                    });
                    break;
                }
            }
        }
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
            var eventId = NodeId.ForType(names.Resolve(h.RequestType));
            var handlerId = NodeId.ForType(names.Resolve(h.HandlerType));

            g.AddNode(new GraphNode(eventId, h.RequestType, NodeKind.Type)
            {
                Tags = [RoleTags.DomainEvent],
            });
            g.AddNode(new GraphNode(handlerId, h.HandlerType, NodeKind.Type)
            {
                FilePath = h.SourceFile,
                Tags = [RoleTags.Handler],
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
            var eventId = NodeId.ForType(names.Resolve(mc.MessageType));
            var consumerType = names.Resolve(mc.ConsumerType);
            var handlerId = NodeId.ForType(consumerType);

            g.AddNode(new GraphNode(eventId, mc.MessageType, NodeKind.Type)
            {
                Tags = [RoleTags.IntegrationEvent, mc.BusKind],
            });
            g.AddNode(new GraphNode(handlerId, mc.ConsumerType, NodeKind.Type)
            {
                FilePath = mc.SourceFile,
                Tags = [RoleTags.Consumer],
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
            var implNodeId = NodeId.ForType(implFqn);

            // Ensure both nodes exist
            if (!g.HasNode(svcNodeId))
                g.AddNode(new GraphNode(svcNodeId, di.ServiceType, NodeKind.Type));
            g.AddNode(new GraphNode(implNodeId, di.ImplementationType, NodeKind.Type)
            {
                Tags = [RoleTags.Service],
            });

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
            var implNodeId = NodeId.ForType(implFqn);
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
    private static void AddCallEdges(CodeGraphBuilder g, DiscoveryModel model, NameResolver names)
    {
        foreach (var ce in model.CallEdges)
        {
            var callerFqn = names.Resolve(ce.CallerType);
            var calleeFqn = names.Resolve(ce.CalleeType);

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
                Resolution = ce.Resolution,
                Confidence = ce.Resolution == Resolution.Semantic ? 0.95f : 0.6f,
            });
        }
    }

    /// <summary>C1: Link EF entities to their data store and to the code that touches them. Entity→
    /// DataStore comes from the detection; the touch edges come from scanning bodies that name an entity
    /// — attributed to the enclosing <b>method</b> (member-origin), so a method-anchored trace shows only
    /// its own data access. The minimal-API lambda/handler-method Member pass keeps a direct MinimalApi→EF
    /// trace (TodoApi) surfacing its touched entity (G4).</summary>
    private static void AddDataEdges(CodeGraphBuilder g, DiscoveryModel model, NameResolver names,
        Dictionary<string, ImmutableArray<MethodSpan>> methodSpans)
    {
        // Map every entity alias (short + FQN) → its node id, so a body that names the entity either way
        // links to the SAME node (FQN-keyed entities used to never match a short-name body reference).
        var entityIdByName = new Dictionary<string, NodeId>(StringComparer.Ordinal);
        foreach (var e in model.Detections.OfType<EfEntityDetection>())
        {
            var entityFqn = names.Resolve(e.EntityType);
            var entityId = NodeId.ForType(entityFqn);
            var ctxFqn = names.Resolve(e.DbContextType);
            if (!string.IsNullOrEmpty(ctxFqn) && ctxFqn != "?")
            {
                var ctxId = NodeId.ForType(ctxFqn);
                g.AddNode(new GraphNode(ctxId, ctxFqn, NodeKind.Type)
                {
                    FilePath = e.SourceFile,
                    Tags = [RoleTags.DataStore],
                });
                g.AddEdge(new GraphEdge(entityId, ctxId, EdgeKind.ReadsWrites)
                {
                    Provenance = $"{e.SourceFile}:{e.LineNumber}",
                    Resolution = Resolution.Join,
                });
            }
            entityIdByName[RemoveGenerics(e.EntityType)] = entityId;
            entityIdByName[RemoveGenerics(entityFqn)] = entityId;
        }
        if (entityIdByName.Count == 0) return;

        // Type bodies (handlers/services) that reference an entity — attributed to the enclosing METHOD so
        // a member-anchored trace gets only its own data access. A type whose body can't be split into
        // methods (parse miss / no methods) falls back to a single Type-level edge (the old behaviour).
        foreach (var type in model.Types.Values)
        {
            if (type.SourceBody is not { Length: > 0 } body) continue;
            var spans = methodSpans.TryGetValue(type.Id, out var s) ? s : [];
            if (spans.IsDefaultOrEmpty)
            {
                LinkBodyToEntity(g, NodeId.ForType(type.Id), body, entityIdByName);
                continue;
            }
            foreach (var span in spans)
            {
                var memberId = EnsureMember(g, type, span.Method);
                LinkBodyToEntity(g, memberId, body[span.Start..span.End], entityIdByName);
            }
        }

        // Minimal-API lambda / captured handler-method Member nodes that reference an entity — this is
        // what makes a direct MinimalApi→EF trace (TodoApi) surface its touched entity.
        foreach (var member in g.Nodes.Where(n => n.Kind == NodeKind.Member && n.SourceBody is { Length: > 0 }).ToList())
            LinkBodyToEntity(g, member.Id, member.SourceBody, entityIdByName);
    }

    /// <summary>Adds one ReadsWrites edge from <paramref name="fromId"/> to the first entity its body
    /// names (syntactic, approximate). Shared by the type-body and member-body passes.</summary>
    private static void LinkBodyToEntity(CodeGraphBuilder g, NodeId fromId, string? body,
        Dictionary<string, NodeId> entityIdByName)
    {
        if (body is not { Length: > 0 } || !g.HasNode(fromId)) return;
        foreach (var (entityName, entityId) in entityIdByName)
        {
            if (entityName.Length < 3 || !body.Contains(entityName, StringComparison.Ordinal)) continue;
            if (fromId == entityId || !g.HasNode(entityId)) continue;
            g.AddEdge(new GraphEdge(fromId, entityId, EdgeKind.ReadsWrites)
            {
                Resolution = Resolution.Syntactic,
                Confidence = 0.5f,
            });
            break;
        }
    }

    /// <summary>C1: Scan handler/ctor SourceBody for domain/integration event creation → Raises edges.
    /// Per R4: matches method-name set {AddDomainEvent, RaiseDomainEvent, AddEvent} with new TEvent()
    /// arg; also new TIntegrationEvent(...) constructor calls. The Raises edge originates from the
    /// enclosing <b>method's</b> Member node (member-origin), falling back to the Type node only when the
    /// match is outside any method — so the type-level <c>data Order</c> no longer dumps every method's
    /// domain events. Resolution.Syntactic.</summary>
    private static void AddRaises(CodeGraphBuilder g, DiscoveryModel model, NameResolver names,
        Dictionary<string, ImmutableArray<MethodSpan>> methodSpans)
    {
        var eventMethods = new[] { "AddDomainEvent", "RaiseDomainEvent", "AddEvent" };
        foreach (var type in model.Types.Values)
        {
            if (type.SourceBody is not { Length: > 0 } body) continue;
            var typeId = NodeId.ForType(type.Id);
            if (!g.HasNode(typeId)) continue;
            var spans = methodSpans.TryGetValue(type.Id, out var s) ? s : [];

            foreach (var method in eventMethods)
            {
                // Match both inline `AddDomainEvent(new X(...))` and the variable form
                // `AddDomainEvent(evt)` where `evt = new X(...)` earlier (eShop's Order ctor raises
                // OrderStartedDomainEvent this way — group 1 = inline new type, group 2 = variable name).
                foreach (Match match in Regex.Matches(body,
                    $@"{Regex.Escape(method)}\s*\(\s*(?:new\s+(\w+)\s*\(|(\w+))", RegexOptions.Compiled))
                {
                    var eventName = match.Groups[1].Success
                        ? match.Groups[1].Value
                        : ResolveVariableNewType(body, match.Index, match.Groups[2].Value,
                            EnclosingSpan(spans, match.Index, body.Length).Start);
                    if (string.IsNullOrEmpty(eventName) || IsNoiseType(eventName)) continue;
                    var eventId = NodeId.ForType(names.Resolve(eventName));

                    g.AddNode(new GraphNode(eventId, eventName, NodeKind.Type)
                    {
                        Tags = [RoleTags.DomainEvent],
                    });
                    g.AddEdge(new GraphEdge(BodyMatchOrigin(g, type, spans, match.Index, typeId), eventId, EdgeKind.Raises)
                    {
                        Provenance = EstimateProvenance(body, match.Index, type.FilePath),
                        Resolution = Resolution.Syntactic,
                        Confidence = 0.5f,
                    });
                }
            }

            // I1.4 — build event type-set from model (BaseTypes / ImplementedInterfaces)
            // so the body-scan matches the real event types, not just *IntegrationEvent* by name.
            var eventTypeNames = BuildEventTypeNameSet(model);

            // new TEvent(...) where TEvent ∈ model-derived event type set (I1.4)
            foreach (Match match in Regex.Matches(body, @"new\s+(\w+)\s*\(", RegexOptions.Compiled))
            {
                var eventName = match.Groups[1].Value;
                if (!eventTypeNames.Contains(eventName))
                {
                    // Fallback: legacy *IntegrationEvent* name regex for repos without event base types
                    if (!eventName.Contains("IntegrationEvent", StringComparison.OrdinalIgnoreCase))
                        continue;
                }
                var eventId = NodeId.ForType(names.Resolve(eventName));

                g.AddNode(new GraphNode(eventId, eventName, NodeKind.Type)
                {
                    Tags = [RoleTags.IntegrationEvent],
                });
                g.AddEdge(new GraphEdge(BodyMatchOrigin(g, type, spans, match.Index, typeId), eventId, EdgeKind.Raises)
                {
                    Provenance = EstimateProvenance(body, match.Index, type.FilePath),
                    Resolution = Resolution.Syntactic,
                    Confidence = 0.5f,
                });
            }
        }
    }

    /// <summary>Best-effort Sends edges FROM a specific Member node (a minimal-API lambda or a captured
    /// handler-method body) by scanning that body for MediatR Send/Publish dispatch — the same pattern
    /// as <see cref="AddSends"/> but anchored on the one endpoint's body, so the trace and the Map's
    /// entry→target reflect exactly what THIS route dispatches (not the whole registration type). A
    /// method that only queries adds no Sends edge, so it correctly resolves to no command.</summary>
    internal static void AddDispatchEdgesFromBody(CodeGraphBuilder g, NodeId fromId, string body, string? provenance, NameResolver names)
    {
        foreach (Match match in Regex.Matches(body,
            @"\.(Send|SendAsync|Publish|PublishAsync)\s*\(\s*(?:new\s+(\w+)(?:\s*<[^>]+>)?\s*\(|(\w+))",
            RegexOptions.Compiled))
        {
            string? requestName;
            if (match.Groups[2].Success)
            {
                // Inline: .Send(new X(...)) — unwrap generic wrapper like IdentifiedCommand<Inner>
                requestName = UnwrapGenericArg(body, match.Groups[2].Index, match.Groups[2].Value);
            }
            else
            {
                var pos = match.Index;
                if (pos <= 0) continue;
                var newMatches = Regex.Matches(body[..pos], @"new\s+(\w+)(?:\s*<[^>]+>)?\s*[\(;]");
                if (newMatches.Count == 0) continue;
                var lastMatch = newMatches[^1];
                requestName = UnwrapGenericArg(body, lastMatch.Groups[1].Index, lastMatch.Groups[1].Value);
            }

            if (string.IsNullOrEmpty(requestName) || IsNoiseType(requestName)) continue;
            var requestId = NodeId.ForType(names.Resolve(requestName));
            g.AddNode(new GraphNode(requestId, requestName, NodeKind.Type));
            g.AddEdge(new GraphEdge(fromId, requestId, EdgeKind.Sends)
            {
                Provenance = provenance,
                Resolution = Resolution.Syntactic,
                Confidence = 0.55f,
            });
        }
    }

    private static void AddSends(CodeGraphBuilder g, DiscoveryModel model, NameResolver names,
        Dictionary<string, ImmutableArray<MethodSpan>> methodSpans)
    {
        foreach (var type in model.Types.Values)
        {
            if (type.SourceBody is not { Length: > 0 } body) continue;
            var typeId = NodeId.ForType(type.Id);
            var spans = methodSpans.TryGetValue(type.Id, out var s) ? s : [];

            // Find all Send/Publish calls with the receiver captured for dispatch-gating (I1.3).
            // Pattern: receiver.Send(expr) where expr is either `new Type(...)` or a local name.
            // Groups: 1=receiver, 2=verb, 3=inline-new-type, 4=variable-name.
            foreach (Match match in Regex.Matches(body,
                @"(\w+)\.(Send|SendAsync|Publish|PublishAsync)\s*\(\s*(?:new\s+(\w+)(?:\s*<[^>]+>)?\s*\(|(\w+))",
                RegexOptions.Compiled))
            {
                var receiverName = match.Groups[1].Value;
                var verb = match.Groups[2].Value;
                string? requestName;
                var isParamFallback = false;

                // I1.3 — gate on known dispatch receiver to prevent false positives
                var receiverType = ResolveReceiverType(type, spans, match.Index, receiverName);
                var catalogConfidence = 0f;
                var isKnown = receiverType is not null
                    && DispatchSeamCatalog.IsKnownReceiver(receiverType, verb, out catalogConfidence);
                float confidence;

                if (match.Groups[3].Success)
                {
                    // Inline: .Send(new X(...))
                    requestName = UnwrapGenericArg(body, match.Groups[3].Index, match.Groups[3].Value);
                    confidence = isKnown ? catalogConfidence : 0.35f;
                }
                else
                {
                    // Variable: .Send(cmd)
                    var varName = match.Groups[4].Value;
                    var pos = match.Index;
                    if (pos <= 0) continue;
                    var (spanStart, _) = EnclosingSpan(spans, pos, body.Length);
                    var before = body[spanStart..pos];
                    var newMatches = Regex.Matches(before, @"new\s+(\w+)(?:\s*<[^>]+>)?\s*[\(;]");
                    if (newMatches.Count > 0)
                    {
                        var lastMatch = newMatches[^1];
                        requestName = UnwrapGenericArg(body, lastMatch.Groups[1].Index, lastMatch.Groups[1].Value);
                        confidence = isKnown ? catalogConfidence : 0.35f;
                    }
                    else
                    {
                        // I1.2 — fall back to param/property types
                        requestName = ResolveVariableFromModel(type, spans, pos, varName);
                        isParamFallback = requestName is not null;
                        confidence = isKnown ? catalogConfidence : 0.35f;
                    }
                }

                // Unknown receiver — emit only for known verbs (bare-verb fallback),
                // skip completely unknown patterns (I1.3 false-positive prevention).
                if (!isKnown && !DispatchSeamCatalog.IsKnownVerb(verb))
                    continue;

                if (string.IsNullOrEmpty(requestName) || IsNoiseType(requestName)) continue;
                var requestFqn = names.Resolve(requestName);
                var requestId = NodeId.ForType(requestFqn);
                if (!g.HasNode(typeId)) continue;

                // Approximate provenance: file + line of the Send call
                var prov = EstimateProvenance(body, match.Index, type.FilePath);

                g.AddNode(new GraphNode(requestId, requestName, NodeKind.Type));
                g.AddEdge(new GraphEdge(BodyMatchOrigin(g, type, spans, match.Index, typeId), requestId, EdgeKind.Sends)
                {
                    Provenance = prov,
                    Resolution = Resolution.Syntactic,
                    Confidence = confidence,
                });
            }
        }
    }

    /// <summary>True for names the body-scan Sends/Raises regexes pick up but that are never a real
    /// request/event — chiefly framework exceptions (<c>new ArgumentNullException(...)</c> caught by the
    /// variable-arg .Send() heuristic). Keeps the trace's indirection seams honest.</summary>
    internal static bool IsNoiseType(string name)
        => name.EndsWith("Exception", StringComparison.Ordinal)
            || name is "Task" or "ValueTask" or "List" or "Dictionary" or "Array"
                or "String" or "Object" or "Guid" or "CancellationToken";

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
    /// notification) — the targets a pipeline behavior wraps. Replaces the old NodeKind.Request check.</summary>
    private static bool IsRequestNode(GraphNode n)
        => n.Kind == NodeKind.Type
            && (n.Tags.Contains(RoleTags.Command)
                || n.Tags.Contains(RoleTags.Query)
                || n.Tags.Contains(RoleTags.Notification));

    /// <summary>A method/ctor text span within a type's SourceBody (offsets relative to SourceBody, which
    /// is the type declaration's full text — so they share the Regex match index's origin). Ctor name is
    /// the type short name, matching <see cref="DevContext.Core.Extractors.Specific.CallGraphExtractor"/>'s
    /// caller-method key.</summary>
    private readonly record struct MethodSpan(int Start, int End, string Method);

    /// <summary>Precomputes, once per build, each type's offset→method locator from its SourceBody so the
    /// body-scan seams (Raises/Sends/ReadsWrites) can attribute every match to the enclosing method
    /// (member-origin) rather than the whole type.</summary>
    private static Dictionary<string, ImmutableArray<MethodSpan>> BuildAllMethodSpans(DiscoveryModel model)
    {
        var map = new Dictionary<string, ImmutableArray<MethodSpan>>(StringComparer.Ordinal);
        foreach (var type in model.Types.Values)
            if (type.SourceBody is { Length: > 0 } body)
                map[type.Id] = BuildMethodSpans(body);
        return map;
    }

    /// <summary>Parses a type's SourceBody fragment and returns the text span of each direct method/ctor.
    /// Mirrors CallGraphExtractor's per-member iteration (direct members of the type, ctors keyed by the
    /// type short name). A parse failure yields an empty list → callers fall back to Type-level origin.</summary>
    private static ImmutableArray<MethodSpan> BuildMethodSpans(string sourceBody)
    {
        try
        {
            var root = CSharpSyntaxTree.ParseText(sourceBody).GetRoot();
            var typeDecl = root.DescendantNodes().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            if (typeDecl is null) return [];

            var typeShort = typeDecl.Identifier.ValueText;
            var spans = ImmutableArray.CreateBuilder<MethodSpan>();
            foreach (var member in typeDecl.Members)
            {
                switch (member)
                {
                    case MethodDeclarationSyntax m:
                        spans.Add(new MethodSpan(m.Span.Start, m.Span.End, m.Identifier.ValueText));
                        break;
                    case ConstructorDeclarationSyntax c:
                        spans.Add(new MethodSpan(c.Span.Start, c.Span.End, typeShort));
                        break;
                }
            }
            return spans.ToImmutable();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>Resolves a SourceBody character offset to the enclosing method name, or null when the
    /// offset is outside every method (e.g. a field initializer) — callers then attribute to the Type.</summary>
    private static string? EnclosingMethod(ImmutableArray<MethodSpan> spans, int offset)
    {
        foreach (var span in spans)
            if (offset >= span.Start && offset < span.End)
                return span.Method;
        return null;
    }

    /// <summary>Resolves a SourceBody character offset to the start/end of the enclosing method span,
    /// or (0, <paramref name="wholeBodyEnd"/>) when outside every method — the variable search is then
    /// the whole body before the match, as it was before span-bounding (field/ctor-less init).</summary>
    private static (int Start, int End) EnclosingSpan(
        ImmutableArray<MethodSpan> spans, int offset, int wholeBodyEnd)
    {
        foreach (var s in spans)
            if (offset >= s.Start && offset < s.End)
                return (s.Start, s.End);
        return (0, wholeBodyEnd);
    }

    /// <summary>Ensures a Member node exists for <paramref name="type"/>.<paramref name="method"/>, carrying
    /// the owning Type's file, and returns its id. First-write-wins merge keeps any body/edges already
    /// attached by the call graph or HTTP-entry passes.</summary>
    private static NodeId EnsureMember(CodeGraphBuilder g, TypeDiscovery type, string method)
    {
        var id = NodeId.ForMember(type.Id, method);
        g.AddNode(new GraphNode(id, $"{type.Name}.{method}", NodeKind.Member) { FilePath = type.FilePath });
        return id;
    }

    /// <summary>Origin node for a body-scan match: the enclosing method's Member node (member-origin), or
    /// the Type node when the match isn't inside any method.</summary>
    private static NodeId BodyMatchOrigin(CodeGraphBuilder g, TypeDiscovery type,
        ImmutableArray<MethodSpan> spans, int offset, NodeId typeFallback)
    {
        var method = EnclosingMethod(spans, offset);
        return method is null ? typeFallback : EnsureMember(g, type, method);
    }

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

    /// <summary>When a <c>new</c> expression wraps the real request in a generic type
    /// (e.g. <c>new IdentifiedCommand&lt;CreateOrderCommand,bool&gt;(...)</c>), extract the
    /// first generic argument as the actual request type. Returns the original typeName
    /// if no generic wrapper is detected.</summary>
    internal static string UnwrapGenericArg(string body, int typeNamePos, string typeName)
    {
        var after = typeNamePos + typeName.Length;
        if (after >= body.Length || body[after] != '<') return typeName;

        // Extract the first generic argument: everything between < and the first , or >
        var start = after + 1;
        var comma = body.IndexOf(',', start);
        var close = body.IndexOf('>', start);
        if (close < 0) return typeName;
        var end = comma > 0 && comma < close ? comma : close;
        if (end <= start) return typeName;

        var inner = body[start..end].Trim();
        return inner.Length > 0 ? inner : typeName;
    }

    /// <summary>True for an EndpointDetection that is a framework/infrastructure pseudo-entry — OpenAPI/Scalar
    /// root routes registered in ServiceDefaults or extension files — not genuine application surface. The
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

    /// <summary>Resolves the event type for a variable-form raise (<c>AddDomainEvent(evt)</c>): prefers the
    /// variable's own <c>evt = new X(...)</c> assignment before the call, else the last <c>new X(...)</c>
    /// before it — span-bounded to the enclosing method (I1.1).</summary>
    private static string? ResolveVariableNewType(string body, int callPos, string varName, int spanStart)
    {
        if (callPos <= 0 || string.IsNullOrEmpty(varName)) return null;
        var before = body[spanStart..callPos];
        var assign = Regex.Matches(before, $@"\b{Regex.Escape(varName)}\s*=\s*new\s+(\w+)");
        if (assign.Count > 0) return assign[^1].Groups[1].Value;
        var news = Regex.Matches(before, @"new\s+(\w+)(?:\s*<[^>]+>)?\s*[\(;]");
        return news.Count > 0 ? news[^1].Groups[1].Value : null;
    }

    /// <summary>Resolves a variable name to its declared type using model data when the body-scan finds
    /// no in-span <c>new</c>: (1) method parameter, (2) property, (3) field from SourceBody outside method
    /// spans. Returns null when none match — callers then emit no edge (honesty > guess).</summary>
    private static string? ResolveVariableFromModel(TypeDiscovery type,
        ImmutableArray<MethodSpan> spans, int pos, string varName)
    {
        // (1) Method parameter — use EnclosingMethod to find which method, then match param name→type
        var methodName = EnclosingMethod(spans, pos);
        if (methodName is not null && !type.Methods.IsDefaultOrEmpty)
        {
            foreach (var m in type.Methods)
            {
                if (!string.Equals(m.Name, methodName, StringComparison.Ordinal)) continue;
                if (m.ParameterNames.IsDefaultOrEmpty) break;
                for (var i = 0; i < Math.Min(m.ParameterNames.Length, m.ParameterTypes.Length); i++)
                {
                    if (string.Equals(m.ParameterNames[i], varName, StringComparison.Ordinal))
                        return StripGenerics(m.ParameterTypes[i]);
                }
                break;
            }
        }

        // (2) Property — directly declared on the type
        if (!type.Properties.IsDefaultOrEmpty)
        {
            foreach (var p in type.Properties)
            {
                if (string.Equals(p.Name, varName, StringComparison.Ordinal))
                    return StripGenerics(p.PropertyType);
            }
        }

        // (3) Field — regex on class-level text outside method bodies (best-effort, no Roslyn)
        if (type.SourceBody is { Length: > 0 } body)
        {
            var classLevel = GetClassLevelBody(body, spans);
            // Match patterns like: private readonly IMediator _m; (type before the field name)
            var fieldMatch = Regex.Match(classLevel,
                $@"\b(\w+(?:<[^>]+>)?(?:\?)?)\s+_{Regex.Escape(varName)}\b",
                RegexOptions.Compiled);
            if (fieldMatch.Success)
                return StripGenerics(fieldMatch.Groups[1].Value);

            // Also try without underscore prefix: IMediator mediator;
            var bareMatch = Regex.Match(classLevel,
                $@"\b(\w+(?:<[^>]+>)?(?:\?)?)\s+{Regex.Escape(varName)}\b",
                RegexOptions.Compiled);
            if (bareMatch.Success)
                return StripGenerics(bareMatch.Groups[1].Value);
        }

        return null;
    }

    /// <summary>Resolves a receiver variable name to its short type name — for gating dispatch seams
    /// on known interfaces (I1.3). Uses the same sources as <see cref="ResolveVariableFromModel"/>:
    /// properties, method params, and field declarations.</summary>
    private static string? ResolveReceiverType(TypeDiscovery type,
        ImmutableArray<MethodSpan> spans, int pos, string receiverName)
    {
        return ResolveVariableFromModel(type, spans, pos, receiverName);
    }

    /// <summary>Builds a set of type short names that are events/commands/notifications by checking
    /// each type's BaseTypes and ImplementedInterfaces against known event markers (I1.4).</summary>
    private static HashSet<string> BuildEventTypeNameSet(DiscoveryModel model)
    {
        var knownSuffixes = new[]
        {
            "IntegrationEvent", "DomainEvent", "Event", "Message",
            "INotification", "IDomainEvent", "IEvent", "IMessage",
            "IRequest", "ICommand", "IQuery",
        };

        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in model.Types.Values)
        {
            var isEvent = false;
            foreach (var bt in type.BaseTypes)
            {
                var stripped = StripGenerics(bt);
                foreach (var suffix in knownSuffixes)
                {
                    if (stripped.EndsWith(suffix, StringComparison.Ordinal))
                    {
                        isEvent = true;
                        break;
                    }
                }
                if (isEvent) break;
            }
            if (!isEvent)
            {
                foreach (var iface in type.ImplementedInterfaces)
                {
                    var stripped = StripGenerics(iface);
                    foreach (var suffix in knownSuffixes)
                    {
                        if (stripped.EndsWith(suffix, StringComparison.Ordinal))
                        {
                            isEvent = true;
                            break;
                        }
                    }
                    if (isEvent) break;
                }
            }
            if (isEvent)
                names.Add(type.Name);
        }
        return names;
    }

    /// <summary>Returns the portion of a type's SourceBody that is outside all method spans —
    /// field/property declarations live here and nowhere else. When spans is empty, the whole body
    /// is returned (the type was unparseable).</summary>
    private static string GetClassLevelBody(string body, ImmutableArray<MethodSpan> spans)
    {
        if (spans.IsDefaultOrEmpty) return body;
        var sb = new System.Text.StringBuilder();
        var pos = 0;
        // spans are in declaration order from BuildMethodSpans — iterate directly
        for (var i = 0; i < spans.Length; i++)
        {
            var s = spans[i];
            if (s.Start > pos)
                sb.Append(body.AsSpan(pos, s.Start - pos));
            pos = s.End;
        }
        if (pos < body.Length)
            sb.Append(body.AsSpan(pos));
        return sb.ToString();
    }

}
