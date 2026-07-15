namespace DevContext.Core.Graph;

/// <summary>Builds HTTP entry points from <see cref="EndpointDetection"/>s (minimal APIs,
/// controllers, FastEndpoints). Links entry → handler with Calls edges and resolves
/// dispatch targets via body scan.</summary>
public sealed class HttpEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        NameResolver names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        var dedup = new HashSet<(string Verb, string Route, string File, int Line)>();
        var usedKeys = new HashSet<string>(StringComparer.Ordinal);
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
                var handlerFqn = names.Resolve(ep.HandlerType, ep.SourceFile);
                var methodName = ep.HandlerMethod;
                var hasSpecificMethod = !string.IsNullOrEmpty(methodName)
                    && methodName is not "<lambda>" and not "<anonymous>"
                    && !methodName.Contains("=>", StringComparison.Ordinal);

                if (hasSpecificMethod && g.HasNode(NodeId.ForType(handlerFqn)))
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
                else
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
                var ownerType = model.Types.Values.FirstOrDefault(t =>
                    string.Equals(t.FilePath, ep.SourceFile, StringComparison.OrdinalIgnoreCase));

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
                    var ownerNode = g.Nodes.FirstOrDefault(n =>
                        n.Kind == NodeKind.Type
                        && string.Equals(n.FilePath, ownerType.FilePath, StringComparison.OrdinalIgnoreCase));
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
