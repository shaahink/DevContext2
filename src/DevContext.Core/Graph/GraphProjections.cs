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
}

public sealed record TransportLink
{
    public string FromService { get; init; } = "";
    public string ToService { get; init; } = "";
    public string Transport { get; init; } = "";
    public string? Evidence { get; init; }
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

        foreach (var svc in serviceNodes)
        {
            var projName = svc.Project ?? svc.Title;
            var kind = ClassifyService(svc);
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
            });
        }

        return new ServiceMapResult { Services = services.ToImmutable(), Transports = transports.ToImmutable() };
    }

    private static string ClassifyService(GraphNode svc)
    {
        foreach (var tag in svc.Tags)
        {
            if (tag.Contains("gateway", StringComparison.OrdinalIgnoreCase)) return "Gateway";
            if (tag.Contains("grpc", StringComparison.OrdinalIgnoreCase)) return "gRPC";
        }
        if (svc.Title.Contains("Gateway", StringComparison.OrdinalIgnoreCase)
            || svc.Title.Contains("Proxy", StringComparison.OrdinalIgnoreCase)) return "Gateway";
        if (svc.Title.Contains("Grpc", StringComparison.OrdinalIgnoreCase)) return "gRPC";
        if (svc.Layer is "Api") return "Web API";
        return "Service";
    }
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
        var rows = graph.Flows.Select(f => f.Entry)
            .Concat(graph.Nodes
                .Where(n => n.Kind == NodeKind.EntryPoint)
                .Select(n => new EntryPoint(EntryPointKind.PublicApi, n.Title, n.Id) { Project = n.Project }))
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
