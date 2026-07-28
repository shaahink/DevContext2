using DevContext.Core.Graph2;

namespace DevContext.Core.Graph;

/// <summary>Builds HTTP entry points from <see cref="EndpointDetection"/>s (minimal APIs,
/// controllers, FastEndpoints). Links entry → handler with Calls edges and resolves
/// dispatch targets via body scan.</summary>
public sealed class HttpEntryPointBuilder : IEntryPointBuilder
{
    /// <summary>C1: the Blazor component lifecycle methods a page entry links to, in priority order —
    /// the first one present becomes the entry's primary HandlerNode (target resolution starts there).</summary>
    private static readonly string[] ComponentLifecycleMethods =
    [
        "OnInitializedAsync", "OnInitialized",
        "OnParametersSetAsync", "OnParametersSet",
        "OnAfterRenderAsync", "OnAfterRender",
    ];

    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        SymbolTable names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        var dedup = new HashSet<(string Verb, string Route, string File, int Line)>();
        var usedKeys = new HashSet<string>(StringComparer.Ordinal);

        // Batch D (R2 §2.D) — the owner-type lookup below used to be
        // `model.OrderedTypes.FirstOrDefault(t => t.FilePath == ep.SourceFile)` INSIDE this loop:
        // O(endpoints x types), and it fires for every endpoint whose handler didn't link. On a repo with
        // thousands of endpoints and thousands of types that is the quadratic the audit named (DC10).
        // First-wins keeps the FirstOrDefault semantics exactly (OrderedTypes is deterministic).
        var typeByFile = new Dictionary<string, TypeDiscovery>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in model.OrderedTypes)
            if (t.FilePath is { Length: > 0 } fp && !typeByFile.ContainsKey(fp))
                typeByFile[fp] = t;

        // Same fix, one level over: the fallback owner-NODE lookup scanned every node in the graph per
        // endpoint. Built lazily — a repo whose endpoints all link never pays for it. Safe to snapshot:
        // this builder adds only EntryPoint/Member nodes, and Type nodes are all seeded by AddTypeNodes
        // before any entry builder runs.
        Dictionary<string, GraphNode>? typeNodeByFile = null;
        GraphNode? TypeNodeForFile(string? file)
        {
            if (string.IsNullOrEmpty(file)) return null;
            if (typeNodeByFile is null)
            {
                typeNodeByFile = new Dictionary<string, GraphNode>(StringComparer.OrdinalIgnoreCase);
                foreach (var n in g.Nodes)
                    if (n.Kind == NodeKind.Type && n.FilePath is { Length: > 0 } nf && !typeNodeByFile.ContainsKey(nf))
                        typeNodeByFile[nf] = n;
            }
            return typeNodeByFile.GetValueOrDefault(file);
        }

        foreach (var ep in model.Detections.OfType<EndpointDetection>())
        {
            if (!scope.Contains(ep.SourceFile) || !noise.IsProductionEntrySource(ep.SourceFile)) continue;

            if (GraphBuilder.IsInfrastructureEntry(ep)) continue;

            if (!dedup.Add((ep.HttpMethod, GraphBuilder.NormalizeRoute(ep.RouteTemplate), ep.SourceFile, ep.LineNumber)))
                continue;

            // T1.7 — two REAL endpoints can share verb+route (API-version pairs like eShop's
            // `GET /items` v1/v2 → GetAllItemsV1 / GetAllItems, or controller overloads like `POST /Device`).
            // They must NOT merge (that loses an endpoint AND cross-wires the Calls edge onto one node) and
            // must NOT collide on the entry node id / title (the NG0955 dup-key the deck fired on). Keep the
            // route-shaped title in the common case; on collision, disambiguate by the distinguishing action
            // (handler method), then handler type, then file:line — the last is always unique here.
            var baseKey = $"{ep.HttpMethod} {ep.RouteTemplate}";
            var key = baseKey;
            if (!usedKeys.Add(key))
            {
                key = DisambiguateKey(baseKey, ep, usedKeys);
                usedKeys.Add(key);
            }
            var id = NodeId.ForEntry(key);
            g.AddNode(new GraphNode(id, key, NodeKind.EntryPoint) { FilePath = ep.SourceFile, LineNumber = ep.LineNumber });

            var isLambdaHandler = ep.HandlerMethod is "<lambda>" or "<anonymous>"
                || string.IsNullOrEmpty(ep.HandlerType)
                || ep.HandlerType is "λ" or "?"
                || ep.HandlerType.Contains("=>", StringComparison.Ordinal);

            var linked = false;
            NodeId? handlerNodeId = null;

            if (!isLambdaHandler)
            {
                var handlerFqn = names.ResolveName(ep.HandlerType, ep.SourceFile);
                var methodName = ep.HandlerMethod;
                var hasSpecificMethod = !string.IsNullOrEmpty(methodName)
                    && methodName is not "<lambda>" and not "<anonymous>" and not "<component>"
                    && !methodName.Contains("=>", StringComparison.Ordinal);

                // C1 (Prism D2): a Blazor page entry links to the component's LIFECYCLE members —
                // navigating to the route IS the framework invoking OnInitialized{Async} etc. The
                // @code virtual trees (RazorCodeVirtualizer) made the component a real Type whose
                // lifecycle methods carry member→member call edges, so linking at member level
                // lights up target resolution, reach scoring, and the trace spine exactly like a
                // controller action. A markup-only page (no @code) has no type/members and falls
                // through to the type-node/owner fallbacks below.
                if (methodName == "<component>" && g.HasNode(NodeId.ForType(handlerFqn))
                    && model.Types.TryGetValue(handlerFqn, out var component))
                {
                    foreach (var lifecycle in ComponentLifecycleMethods)
                    {
                        if (!component.Methods.Any(m => m.Name == lifecycle)) continue;
                        var lifecycleId = NodeId.ForMember(handlerFqn, lifecycle);
                        g.AddNode(new GraphNode(lifecycleId, ep.HandlerType + "." + lifecycle, NodeKind.Member)
                        {
                            FilePath = ep.SourceFile,
                        });
                        g.AddEdge(new GraphEdge(id, lifecycleId, EdgeKind.Calls)
                        {
                            Provenance = $"{ep.SourceFile}:{ep.LineNumber}",
                            Resolution = Resolution.Join,
                        });
                        handlerNodeId ??= lifecycleId; // list order = priority; first hit is primary
                        linked = true;
                    }
                }

                if (!linked && hasSpecificMethod && g.HasNode(NodeId.ForType(handlerFqn)))
                {
                    var memberNodeId = NodeId.ForMember(handlerFqn, methodName);
                    handlerNodeId = memberNodeId;
                    g.AddNode(new GraphNode(memberNodeId, ep.HandlerType + "." + methodName, NodeKind.Member)
                    {
                        FilePath = ep.SourceFile,
                        SourceBody = ep.HandlerBody,
                        LineNumber = ep.HandlerLine > 0 ? ep.HandlerLine : ep.LineNumber, // T2.2: no trailing-colon members
                    });
                    g.AddEdge(new GraphEdge(id, memberNodeId, EdgeKind.Calls)
                    {
                        Provenance = $"{ep.SourceFile}:{ep.LineNumber}",
                        Resolution = Resolution.Join,
                    });
                    // L2.3: dispatch edges are now produced by seam detectors over BodyFacts — no regex needed here.
                    linked = true;
                }
                else if (!linked)
                {
                    var typeNodeId = NodeId.ForType(handlerFqn);
                    if (g.HasNode(typeNodeId))
                    {
                        handlerNodeId = typeNodeId;
                        g.AddEdge(new GraphEdge(id, typeNodeId, EdgeKind.Calls)
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
                var ownerType = typeByFile.GetValueOrDefault(ep.SourceFile);

                if (isLambdaHandler && !string.IsNullOrEmpty(ep.HandlerBody))
                {
                    var ownerKey = ownerType?.Id ?? Path.GetFileNameWithoutExtension(ep.SourceFile);
                    var lambdaId = NodeId.ForMember(ownerKey, $"<lambda> {key}");
                    handlerNodeId = lambdaId;
                    g.AddNode(new GraphNode(lambdaId, $"<lambda> {key}", NodeKind.Member)
                    {
                        FilePath = ep.SourceFile,
                        SourceBody = ep.HandlerBody,
                        LineNumber = ep.HandlerLine > 0 ? ep.HandlerLine : ep.LineNumber, // T2.2
                    });
                    g.AddEdge(new GraphEdge(id, lambdaId, EdgeKind.Calls)
                    {
                        Provenance = $"{ep.SourceFile}:{(ep.HandlerLine > 0 ? ep.HandlerLine : ep.LineNumber)}",
                        Resolution = Resolution.Join,
                    });
                    // L2.3: dispatch edges are now produced by seam detectors over BodyFacts.
                    linked = true;
                }
                else if (ownerType is not null)
                {
                    var ownerNode = TypeNodeForFile(ownerType.FilePath);
                    if (ownerNode is not null)
                    {
                        handlerNodeId = ownerNode.Id;
                        g.AddEdge(new GraphEdge(id, ownerNode.Id, EdgeKind.Calls)
                        {
                            Provenance = $"{ep.SourceFile}:{ep.LineNumber}",
                            Resolution = Resolution.Join,
                        });
                    }
                }
            }

            // T1.7 — a Blazor page route (BlazorEntryExtractor stamps HandlerMethod "<component>" on a
            // .razor @page) is an interactive UI entry, NOT an HTTP REST endpoint. Emitting it as
            // HttpEndpoint polluted the security surface (the "49/56 endpoints anonymous" insight counted
            // Blazor pages); as UiEntry it drops out of the HTTP entry set and the insight recomputes onto
            // the real API. Route-shaped title stays — it's still how a reader recognizes the page.
            var isBlazorPage = ep.HandlerMethod == "<component>"
                || ep.SourceFile.EndsWith(".razor", StringComparison.OrdinalIgnoreCase);
            var entryKind = isBlazorPage ? EntryPointKind.UiEntry : EntryPointKind.HttpEndpoint;

            entries.Add(new EntryPoint(entryKind, key, id)
            {
                HttpMethod = isBlazorPage ? null : ep.HttpMethod,
                Route = ep.RouteTemplate,
                Provenance = $"{ep.SourceFile}:{ep.LineNumber}",
                HandlerNode = handlerNodeId,
                AuthAttributes = ep.AuthAttributes,
                Project = scope.ProjectForFile(ep.SourceFile),
            });
        }
        return entries.ToImmutable();
    }

    /// <summary>T1.7 — Produces a unique, human-meaningful key for an endpoint whose verb+route already
    /// appeared. Tries the distinguishing action (handler method), then the handler type simple name, then
    /// falls back to <c>file:line</c> — which is always unique because exact verb+route+file+line dups are
    /// dropped upstream. Never merges two real endpoints.</summary>
    private static string DisambiguateKey(string baseKey, EndpointDetection ep, HashSet<string> used)
    {
        foreach (var disc in DiscriminatorCandidates(ep))
        {
            var candidate = $"{baseKey} [{disc}]";
            if (!used.Contains(candidate)) return candidate;
        }
        return $"{baseKey} [{Path.GetFileNameWithoutExtension(ep.SourceFile)}:{ep.LineNumber}]";
    }

    private static IEnumerable<string> DiscriminatorCandidates(EndpointDetection ep)
    {
        var method = ep.HandlerMethod;
        if (!string.IsNullOrEmpty(method) && method is not "<lambda>" and not "<anonymous>" and not "<component>"
            && !method.Contains("=>", StringComparison.Ordinal))
            yield return method;

        var type = ep.HandlerType;
        if (!string.IsNullOrEmpty(type) && !type.Contains("=>", StringComparison.Ordinal) && type is not "?" and not "λ")
        {
            var dot = type.LastIndexOf('.');
            yield return dot >= 0 ? type[(dot + 1)..] : type;
        }
    }
}
