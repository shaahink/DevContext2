using DevContext.Core.Graph.Seams;
using DevContext.Core.Graph2;
using DevContext.Core.Graph2.Seams;
using DevContext.Core.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Graph;

public sealed partial class GraphBuilder
{
    // ── M1.7-M1.8: Cross-service ServiceLink joins (W4) ──────────────────────────
    // (The bus publish→consume join is now the T2.6 EventWiringProjection, emitted in Build.)

    /// <summary>M1.7 — Cross-project gRPC ServiceLinks. Matches <see cref="GrpcClientDetection"/>
    /// (client type usage in project A) to <see cref="GrpcServiceDetection"/> (service implementation
    /// in project B) by matching the service name.</summary>
    private static void AddGrpcServiceLinks(CodeGraphBuilder g, DiscoveryModel model,
        SymbolTable names, SolutionScope scope, NoiseFilter noise)
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
        SymbolTable names, SolutionScope scope, NoiseFilter noise)
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
        foreach (var t in model.OrderedTypes)
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
        foreach (var t in model.OrderedTypes)
        {
            if (t.Name.Contains("DomainEvent", StringComparison.Ordinal))
                domainTypes.Add(t.Name);
        }
        return (integrationTypes, domainTypes);
    }

    private static SeamContext BuildSeamContext(SymbolTable symbols,
        IEnumerable<string> integrationEventTypes, IEnumerable<string> domainEventTypes,
        IEnumerable<string> knownEntities)
    {
        return new SeamContext
        {
            Symbols = symbols, // the ONE table built in Build() — never a second index (Batch A)
            KnownEntities = knownEntities.ToImmutableHashSet(StringComparer.Ordinal),
            IntegrationEventTypes = integrationEventTypes.ToImmutableHashSet(StringComparer.Ordinal),
            DomainEventTypes = domainEventTypes.ToImmutableHashSet(StringComparer.Ordinal),
        };
    }
}
