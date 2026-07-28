namespace DevContext.Core.Graph;

/// <summary>A projection over a <see cref="CodeGraph"/> that produces a typed output. One truth
/// (the graph), three renderers (CLI, MCP, UI) — design §3.</summary>
public interface IGraphProjection<out TOut>
{
    TOut Project(CodeGraph graph, ProjectionOptions options);
}

/// <summary>Shared options for all projections.</summary>
public sealed record ProjectionOptions
{
    public static readonly ProjectionOptions Default = new();
    public int MaxFlows { get; init; } = 10;
    public bool IncludeLibraries { get; init; }
    /// <summary>L7.2 — the detected archetype, used by <see cref="ArchetypeProjection"/> to shape output.</summary>
    public Archetype? Archetype { get; init; }
}

// ────────────────────────────────────────────────────────
// ServiceMap
// ────────────────────────────────────────────────────────

public sealed record ServiceMapResult
{
    public ImmutableArray<ServiceCard> Services { get; init; } = [];
    public ImmutableArray<TransportLink> Transports { get; init; } = [];
}

public sealed record ServiceCard
{
    public string Name { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string Kind { get; init; } = "";
    public string? Layer { get; init; }
    public string? Feature { get; init; }
    public ImmutableArray<string> Stack { get; init; } = [];
    /// <summary>R3 D-B — the infrastructure resources this service declares a dependency on. Batch B
    /// has emitted these as <see cref="NodeKind.Store"/> nodes since the Aspire topology landed, but
    /// nothing carried them out of the graph, so no renderer could draw a repo's stores.</summary>
    public ImmutableArray<ServiceStore> Stores { get; init; } = [];
    /// <summary>R3 D-B — the projects this service ORCHESTRATES, when it is an AppHost. Non-empty on
    /// at most one card per solution; the canvas draws it as a containment frame rather than as
    /// edges, which is what an orchestrator actually expresses.</summary>
    public ImmutableArray<string> Orchestrates { get; init; } = [];
}

/// <summary>An infrastructure resource a service depends on (a Redis, a Postgres, a queue broker).
/// <paramref name="ResourceType"/> is the orchestrator's own word for it, lowercased — classify at
/// the render edge, never invent a taxonomy here.</summary>
public sealed record ServiceStore(string Name, string ResourceType);

public sealed record TransportLink
{
    public string FromService { get; init; } = "";
    public string ToService { get; init; } = "";
    public string Transport { get; init; } = "";
    public string? Evidence { get; init; }
    /// <summary>R3 D-B — how the underlying ServiceLink edge was established. The trace tree has drawn
    /// non-semantic hops dashed since T6.2; without this the topology canvas had no way to make the
    /// same distinction, so a deployment-derived guess and a Roslyn-verified call looked equally
    /// certain on the surface Explore now opens on.</summary>
    public Resolution Resolution { get; init; } = Resolution.Join;
}

public sealed class ServiceMapProjection : IGraphProjection<ServiceMapResult>
{
    public ServiceMapResult Project(CodeGraph graph, ProjectionOptions options)
    {
        var services = ImmutableArray.CreateBuilder<ServiceCard>();
        var transports = ImmutableArray.CreateBuilder<TransportLink>();

        var serviceNodes = graph.Nodes
            .Where(n => n.Kind == NodeKind.Service)
            // Design §1.3: a Service card is a RUNNABLE deployable. Service nodes synthesized for a
            // class library that only participates in a cross-service seam (e.g. a bus consumer
            // project) are excluded unless the caller explicitly asks for libraries.
            .Where(n => options.IncludeLibraries || n.Tags.Contains(RoleTags.Runnable))
            .ToList();

        // Collect stack evidence per service
        var serviceStacks = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var n in graph.Nodes)
        {
            if (n.Project is not { } proj) continue;
            if (!serviceStacks.TryGetValue(proj, out var stack))
                serviceStacks[proj] = stack = [];
            if (n.Layer is { } layer) stack.Add(layer);
            foreach (var tag in n.Tags)
                if (tag is RoleTags.Aggregate or RoleTags.Entity or RoleTags.Handler or RoleTags.DataStore)
                    stack.Add(tag);
        }

        // R3 D-B: entry surfaces per owning project — the evidence ClassifyService now reads.
        var surfaces = new Dictionary<string, Dictionary<EntryPointKind, int>>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in graph.Entries)
        {
            if (entry.Project is not { } proj) continue;
            if (!surfaces.TryGetValue(proj, out var counts))
                surfaces[proj] = counts = [];
            counts[entry.Kind] = counts.GetValueOrDefault(entry.Kind) + 1;
        }

        foreach (var svc in serviceNodes)
        {
            var projName = svc.Project ?? svc.Title;
            var kind = ClassifyService(svc, surfaces.GetValueOrDefault(projName));
            var displayName = projName; // full name, not tail-name

            var stack = serviceStacks.TryGetValue(projName, out var s)
                ? s.OrderBy(x => x).ToImmutableArray()
                : ImmutableArray<string>.Empty;

            services.Add(new ServiceCard
            {
                Name = svc.Id.Key,
                DisplayName = displayName,
                Kind = kind,
                Layer = svc.Layer,
                Feature = svc.Feature,
                Stack = stack,
                Stores = StoresOf(graph, svc.Id),
                Orchestrates = OrchestratedBy(graph, svc.Id),
            });
        }

        // Collect transports from ServiceLink edges
        foreach (var link in graph.AllEdges.Where(e => e.Kind == EdgeKind.ServiceLink))
        {
            var fromSvc = graph.Node(link.From)?.Project ?? link.From.Key;
            var toSvc = graph.Node(link.To)?.Project ?? link.To.Key;
            var transport = link.Tags.IsDefaultOrEmpty ? "unknown" : link.Tags[0];

            transports.Add(new TransportLink
            {
                FromService = fromSvc,
                ToService = toSvc,
                Transport = transport,
                Evidence = link.Provenance,
                Resolution = link.Resolution,
            });
        }

        return new ServiceMapResult { Services = services.ToImmutable(), Transports = transports.ToImmutable() };
    }

    /// <summary>The infrastructure resources hanging off a service, in declaration order. The resource
    /// TYPE rides the DependsOn edge as its tag (AddAspireTopology), so a store whose type the
    /// orchestrator never named stays typeless rather than being guessed from its resource name.</summary>
    private static ImmutableArray<ServiceStore> StoresOf(CodeGraph graph, NodeId serviceId)
    {
        var stores = ImmutableArray.CreateBuilder<ServiceStore>();
        foreach (var edge in graph.OutEdges(serviceId))
        {
            if (edge.Kind != EdgeKind.DependsOn || edge.To.Kind != NodeKind.Store) continue;
            var name = graph.Node(edge.To)?.Title ?? edge.To.Key;
            stores.Add(new ServiceStore(name, edge.Tags.IsDefaultOrEmpty ? "" : edge.Tags[0]));
        }
        return stores.ToImmutable();
    }

    /// <summary>The projects an AppHost declares as resources — empty for every service that is not
    /// one. Project names, in declaration order, deduplicated.</summary>
    private static ImmutableArray<string> OrchestratedBy(CodeGraph graph, NodeId serviceId)
    {
        var members = ImmutableArray.CreateBuilder<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var edge in graph.OutEdges(serviceId, EdgeKind.DependsOn))
        {
            if (edge.To.Kind != NodeKind.Service) continue;
            if (edge.Tags.IsDefaultOrEmpty || !edge.Tags.Contains(GraphBuilder.OrchestratesTag)) continue;
            var name = graph.Node(edge.To)?.Project ?? edge.To.Key;
            if (seen.Add(name)) members.Add(name);
        }
        return members.ToImmutable();
    }

    /// <summary>What a service OFFERS, derived from the entry surfaces it actually owns.
    /// <para>R3 D-B: the previous implementation asked for the service node's <see cref="GraphNode.Layer"/>,
    /// which <c>AddServiceNodes</c> never sets — so on every repo every service classified as "Service",
    /// and every kind glyph the canvas drew was empty. Entry kinds are evidence the graph already
    /// carries and already attributes per project, which is what the classifier should have read.</para>
    /// <para>The dominant surface wins; <see cref="KindPrecedence"/> breaks ties deterministically.
    /// <see cref="EntryPointKind.DomainEventHandler"/> is excluded from the tally on purpose — reacting
    /// to a domain event is something a service does internally, not something it offers, and counting
    /// it would let a handful of internal reactions outvote the endpoints a service exists to serve.</para></summary>
    private static string ClassifyService(GraphNode svc, Dictionary<EntryPointKind, int>? surfaces)
    {
        // Tag evidence outranks the tally: a gateway's defining trait is that it forwards someone
        // else's traffic, which is a topology fact rather than an entry surface it owns.
        foreach (var tag in svc.Tags)
            if (tag.Contains("gateway", StringComparison.OrdinalIgnoreCase)) return "Gateway";
        if (svc.Title.Contains("Gateway", StringComparison.OrdinalIgnoreCase)
            || svc.Title.Contains("Proxy", StringComparison.OrdinalIgnoreCase)) return "Gateway";

        if (surfaces is null || surfaces.Count == 0) return "Service";

        var best = "";
        var bestCount = 0;
        var bestRank = int.MaxValue;
        foreach (var (entryKind, count) in surfaces)
        {
            if (KindOf(entryKind) is not { } named) continue;
            var rank = Array.IndexOf(KindPrecedence, named);
            if (count > bestCount || (count == bestCount && rank < bestRank))
                (best, bestCount, bestRank) = (named, count, rank);
        }
        return best.Length == 0 ? "Service" : best;
    }

    /// <summary>Tie-break order — most specific offer first. A service that owns equal numbers of two
    /// surfaces is named for the one that says more about why it is deployed.</summary>
    private static readonly string[] KindPrecedence =
        ["gRPC", "GraphQL", "Realtime", "Web API", "Functions", "Grains", "Worker", "CLI", "UI", "Library"];

    private static string? KindOf(EntryPointKind kind) => kind switch
    {
        EntryPointKind.HttpEndpoint => "Web API",
        EntryPointKind.GrpcService => "gRPC",
        EntryPointKind.GraphQlField => "GraphQL",
        EntryPointKind.SignalRHub => "Realtime",
        EntryPointKind.FunctionEntry => "Functions",
        EntryPointKind.GrainMethod => "Grains",
        EntryPointKind.MessageConsumer or EntryPointKind.HostedService or EntryPointKind.ScheduledJob => "Worker",
        EntryPointKind.CliCommand => "CLI",
        EntryPointKind.UiEntry => "UI",
        EntryPointKind.PublicApi => "Library",
        _ => null, // DomainEventHandler — internal reaction, see remarks
    };
}

// ────────────────────────────────────────────────────────
// FlowList
// ────────────────────────────────────────────────────────

public sealed record FlowListResult
{
    public ImmutableArray<FlowCard> Flows { get; init; } = [];
    public int TotalFlows { get; init; }
}

public sealed record FlowCard
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Kind { get; init; } = "";
    public string? NodeId { get; init; }
    public string? Route { get; init; }
    public string? HttpMethod { get; init; }
    public string? Target { get; init; }
    public string? GroupPath { get; init; }
    public int Depth { get; init; }
    public int Hops { get; init; }
    public int Touches { get; init; }
    public int Emits { get; init; }
    public double Score { get; init; }
}

public sealed class FlowListProjection : IGraphProjection<FlowListResult>
{
    public FlowListResult Project(CodeGraph graph, ProjectionOptions options)
    {
        var flows = graph.Flows;
        if (flows.IsDefaultOrEmpty)
            return new FlowListResult { TotalFlows = 0 };

        var cards = flows
            .Select(f => new FlowCard
            {
                Id = f.Id,
                Title = f.Entry.Title,
                Kind = f.Entry.Kind.ToString(),
                NodeId = f.Entry.Node.ToString(),
                Route = f.Entry.Route,
                HttpMethod = f.Entry.HttpMethod,
                Target = f.Entry.Target,
                GroupPath = f.Entry.GroupPath,
                Depth = f.Steps.Length,
                Hops = f.Hops.Length,
                Touches = f.Touches.Length,
                Emits = f.Emits.Length,
                Score = f.Entry.Score,
            })
            .OrderByDescending(c => c.Score)
            .ThenByDescending(c => c.Depth)
            .Take(options.MaxFlows)
            .ToImmutableArray();

        return new FlowListResult { Flows = cards, TotalFlows = flows.Length };
    }
}

// ────────────────────────────────────────────────────────
// EntryTable
// ────────────────────────────────────────────────────────

public sealed record EntryTableResult
{
    public ImmutableArray<EntryTableRow> Rows { get; init; } = [];
}

public sealed record EntryTableRow
{
    public string Kind { get; init; } = "";
    public string Title { get; init; } = "";
    public string? Route { get; init; }
    public string? Target { get; init; }
    public string? Project { get; init; }
    public string? GroupPath { get; init; }
    public string? Layer { get; init; }
    public string? Feature { get; init; }
    public double Score { get; init; }
    public int Reach { get; init; }
    public int CrossProjects { get; init; }
}

public sealed class EntryTableProjection : IGraphProjection<EntryTableResult>
{
    public EntryTableResult Project(CodeGraph graph, ProjectionOptions options)
    {
        // T1.8 — Single-source the kind: rows come from the authoritative entry inventory
        // (graph.Entries), whose EntryPointKind is stamped by the builder that produced the entry.
        // We no longer re-derive kind from node `kind:` tags nor default untagged EntryPoint nodes
        // to PublicApi (the "gRPC 75" facet lie): one entry = one row = its true kind. A bare
        // EntryPoint node with no matching Entry record is an assembler bug to surface, not a bucket.
        var rows = graph.Entries
            .DistinctBy(e => e.Node)
            .Select(e =>
            {
                var node = graph.Node(e.Node);
                return new EntryTableRow
                {
                    Kind = e.Kind.ToString(),
                    Title = e.Title,
                    Route = e.Route,
                    Target = e.Target,
                    Project = e.Project ?? node?.Project,
                    GroupPath = e.GroupPath,
                    Layer = node?.Layer,
                    Feature = node?.Feature,
                    Score = e.Score,
                    Reach = e.Reach,
                    CrossProjects = e.CrossProjects,
                };
            })
            .OrderByDescending(r => r.Score)
            .ToImmutableArray();

        return new EntryTableResult { Rows = rows };
    }
}

// ────────────────────────────────────────────────────────
// LayerBand
// ────────────────────────────────────────────────────────

public sealed record LayerBandResult
{
    public ImmutableArray<NodeBand> NodeBands { get; init; } = [];
    public ImmutableArray<string> Layers { get; init; } = [];
    public ImmutableArray<string> Features { get; init; } = [];
}

public sealed record NodeBand
{
    public string NodeId { get; init; } = "";
    public string? Layer { get; init; }
    public string? Feature { get; init; }
}

public sealed class LayerBandProjection : IGraphProjection<LayerBandResult>
{
    public LayerBandResult Project(CodeGraph graph, ProjectionOptions options)
    {
        var bands = ImmutableArray.CreateBuilder<NodeBand>();
        var layers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var features = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in graph.Nodes)
        {
            if (node.Layer is { } l) layers.Add(l);
            if (node.Feature is { } f) features.Add(f);

            if (node.Layer is not null || node.Feature is not null)
            {
                bands.Add(new NodeBand
                {
                    NodeId = node.Id.ToString(),
                    Layer = node.Layer,
                    Feature = node.Feature,
                });
            }
        }

        return new LayerBandResult
        {
            NodeBands = bands.ToImmutable(),
            Layers = [.. layers.OrderBy(l => l)],
            Features = [.. features.OrderBy(f => f)],
        };
    }
}
