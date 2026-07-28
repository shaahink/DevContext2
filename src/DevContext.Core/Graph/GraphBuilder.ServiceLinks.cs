using DevContext.Core.Extractors.Specific;
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

    /// <summary>Batch B (DC3) — ServiceLinks from transport client REGISTRATIONS. The registration is
    /// where the target is named: <c>AddGrpcClient&lt;Basket.BasketClient&gt;(o =&gt; o.Address =
    /// new("http://basket-api"))</c> says both what protocol is spoken and who is spoken to, while the
    /// injection site sees only a client type (in eShop, behind a using-alias). Targets that resolve to
    /// no analyzed project become EXTERNAL service nodes rather than being dropped: the old
    /// both-ends-in-solution rule silently discarded every call leaving the repo.</summary>
    private static void AddTransportClientLinks(CodeGraphBuilder g, DiscoveryModel model,
        SolutionScope scope, NoiseFilter noise, ServiceAddressBook addresses)
    {
        foreach (var client in model.Detections.OfType<TransportClientDetection>()
            .OrderBy(d => d.SourceFile, StringComparer.Ordinal).ThenBy(d => d.LineNumber))
        {
            if (!scope.Contains(client.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(client.SourceFile)) continue;
            if (scope.ProjectForFile(client.SourceFile) is not { Length: > 0 } clientProject) continue;

            var fromId = NodeId.ForService(clientProject);
            if (!g.HasNode(fromId)) continue;   // registered inside a library, not a deployable service

            var host = ServiceAddressBook.ExtractHost(client.Address);
            NodeId toId;
            Resolution resolution;
            float confidence;

            if (host is null)
            {
                // No literal address — a generated gRPC client type still names its service.
                if (client.Transport != TransportKinds.Grpc) continue;
                if (addresses.ResolveGrpcClientType(client.ClientType) is not { } grpcProject) continue;
                if (string.Equals(grpcProject, clientProject, StringComparison.OrdinalIgnoreCase)) continue;
                toId = NodeId.ForService(grpcProject);
                if (!g.HasNode(toId)) continue;
                (resolution, confidence) = (Resolution.Join, 0.8f);
            }
            else if (addresses.ResolveHost(host) is { } targetProject)
            {
                if (string.Equals(targetProject, clientProject, StringComparison.OrdinalIgnoreCase)) continue;
                toId = NodeId.ForService(targetProject);
                if (!g.HasNode(toId)) continue;   // the address names a library — not a service seam
                (resolution, confidence) = (Resolution.Join, 0.9f);
            }
            else if (ServiceAddressBook.IsExternalHost(host))
            {
                toId = NodeId.ForService(host);
                g.AddNode(new GraphNode(toId, host, NodeKind.Service) { Tags = [RoleTags.External] });
                (resolution, confidence) = (Resolution.Syntactic, 0.7f);
            }
            else
            {
                continue;   // localhost / a config placeholder: a real registration, no nameable target
            }

            g.AddEdge(new GraphEdge(fromId, toId, EdgeKind.ServiceLink)
            {
                Provenance = $"{client.SourceFile}:{client.LineNumber}",
                Resolution = resolution,
                Confidence = confidence,
                Tags = [TransportLinkTag(client.Transport)],
            });
        }
    }

    /// <summary>Batch B — the Aspire AppHost's resource graph, which until now was detected and then
    /// thrown away. Project-to-project <c>WithReference</c> becomes a ServiceLink (A is handed B's
    /// address at startup); infrastructure resources become <see cref="NodeKind.Store"/> nodes hanging
    /// off the services that reference them, which is what <see cref="RoleTags.DataStore"/> and the
    /// Store kind were declared for. Runs AFTER the transport and bus joins so that a pair with real
    /// protocol evidence keeps its specific tag.</summary>
    /// <summary>R3 D-B — the orchestrator's own membership edge: AppHost → each project it declares as
    /// a resource, tagged <c>orchestrates</c>.
    /// <para>Until now an AppHost was a Service node that nothing pointed at, so it rendered as a
    /// floating peer of the system it launches. The <c>WithReference</c> edges it produces run
    /// project→project (A is handed B's address), never AppHost→project, so no edge existed to say
    /// the one thing the AppHost is for. The canvas draws this as containment rather than as lines.</para></summary>
    private static void AddOrchestratorEdges(CodeGraphBuilder g, DiscoveryModel model,
        SolutionScope scope, NoiseFilter noise, ServiceAddressBook addresses)
    {
        foreach (var resource in model.Detections.OfType<AspireResourceDetection>()
            .OrderBy(d => d.SourceFile, StringComparer.Ordinal).ThenBy(d => d.LineNumber))
        {
            if (resource.ProjectRef is null) continue;              // infrastructure, not a project
            if (!scope.Contains(resource.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(resource.SourceFile)) continue;
            if (scope.ProjectForFile(resource.SourceFile) is not { } hostProject) continue;
            if (!addresses.ProjectResources.TryGetValue(resource.ResourceName, out var member)) continue;
            if (string.Equals(member, hostProject, StringComparison.OrdinalIgnoreCase)) continue;

            var hostId = NodeId.ForService(hostProject);
            var memberId = NodeId.ForService(member);
            if (!g.HasNode(hostId) || !g.HasNode(memberId)) continue;
            g.AddEdge(new GraphEdge(hostId, memberId, EdgeKind.DependsOn)
            {
                Provenance = $"{resource.SourceFile}:{resource.LineNumber}",
                Resolution = Resolution.Join,
                Confidence = 0.9f,
                Tags = [OrchestratesTag],
            });
        }
    }

    /// <summary>Edge tag marking an orchestrator's membership edge (R3 D-B).</summary>
    internal const string OrchestratesTag = "orchestrates";

    private static void AddAspireTopology(CodeGraphBuilder g, DiscoveryModel model,
        SolutionScope scope, NoiseFilter noise, ServiceAddressBook addresses)
    {
        AddOrchestratorEdges(g, model, scope, noise, addresses);

        foreach (var relationship in model.Detections.OfType<AspireRelationshipDetection>()
            .OrderBy(d => d.SourceFile, StringComparer.Ordinal).ThenBy(d => d.LineNumber))
        {
            if (!scope.Contains(relationship.SourceFile)) continue;
            if (!noise.IsProductionEntrySource(relationship.SourceFile)) continue;
            if (!addresses.ProjectResources.TryGetValue(relationship.SourceResource, out var fromProject)) continue;

            var fromId = NodeId.ForService(fromProject);
            if (!g.HasNode(fromId)) continue;
            var provenance = $"{relationship.SourceFile}:{relationship.LineNumber}";

            if (addresses.ProjectResources.TryGetValue(relationship.TargetResource, out var toProject))
            {
                if (string.Equals(toProject, fromProject, StringComparison.OrdinalIgnoreCase)) continue;
                var toId = NodeId.ForService(toProject);
                if (!g.HasNode(toId)) continue;
                g.AddEdge(new GraphEdge(fromId, toId, EdgeKind.ServiceLink)
                {
                    Provenance = provenance,
                    Resolution = Resolution.Join,
                    Confidence = 0.7f,
                    Tags = [ServiceLinkTags.AspireReference],
                });
            }
            else if (addresses.StoreResources.TryGetValue(relationship.TargetResource, out var store))
            {
                var toId = NodeId.ForStore(store.Name);
                g.AddNode(new GraphNode(toId, store.Name, NodeKind.Store)
                {
                    Tags = [RoleTags.DataStore],
                    FilePath = relationship.SourceFile,
                    LineNumber = relationship.LineNumber,
                });
                g.AddEdge(new GraphEdge(fromId, toId, EdgeKind.DependsOn)
                {
                    Provenance = provenance,
                    Resolution = Resolution.Join,
                    Confidence = 0.8f,
                    Tags = [store.ResourceType.ToLowerInvariant()],
                });
            }
        }
    }

    private static string TransportLinkTag(string transport) => transport switch
    {
        TransportKinds.Grpc => ServiceLinkTags.Grpc,
        TransportKinds.Refit => ServiceLinkTags.RefitDirect,
        _ => ServiceLinkTags.HttpDirect,
    };

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
