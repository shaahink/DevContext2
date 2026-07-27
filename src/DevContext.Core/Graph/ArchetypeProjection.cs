namespace DevContext.Core.Graph;

/// <summary>L7.2 — archetype-shaped projections. A single projection that branches on the detected
/// <see cref="Archetype"/> to produce the right entry-point view: window/command tree for Desktop,
/// schedule/queue for Worker, public surface for Library, route/component tree for Blazor.
/// The graph model does NOT change — archetypes differ in entry builders + projections only.</summary>
public sealed record ArchetypeView
{
    public Archetype Archetype { get; init; }
    public ImmutableArray<ArchetypeEntryGroup> Groups { get; init; } = [];

    public string SectionLabel => Archetype switch
    {
        Archetype.Library => "PUBLIC SURFACE",
        Archetype.Desktop => "DESKTOP VIEW",
        Archetype.Worker  => "WORKER VIEW",
        Archetype.Blazor  => "COMPONENT TREE",
        Archetype.CliTool => "COMMAND SURFACE",
        _                 => "",
    };

    public bool IsRelevant => SectionLabel.Length > 0;
}

public sealed record ArchetypeEntryGroup
{
    public string Project { get; init; } = "";
    public string? Layer { get; init; }
    public ImmutableArray<ArchetypeEntryRow> Entries { get; init; } = [];
}

public sealed record ArchetypeEntryRow
{
    public string Kind { get; init; } = "";
    public string Title { get; init; } = "";
    public string? Route { get; init; }
    public string? Target { get; init; }
    public string? GroupPath { get; init; }
    public int Depth { get; init; }
    public int Hops { get; init; }
    public double Score { get; init; }
}

public sealed class ArchetypeProjection : IGraphProjection<ArchetypeView>
{
    public ArchetypeView Project(CodeGraph graph, ProjectionOptions options)
    {
        var archetype = options.Archetype ?? Archetype.App;
        var entries = archetype switch
        {
            Archetype.Desktop => CollectDesktopEntries(graph),
            Archetype.Worker  => CollectWorkerEntries(graph),
            Archetype.Blazor  => CollectBlazorEntries(graph),
            Archetype.Library => CollectPublicApiEntries(graph),
            Archetype.CliTool => CollectCliEntries(graph),
            _                 => ImmutableArray<(EntryPointKind Kind, EntryPoint Entry)>.Empty,
        };

        if (entries.IsDefaultOrEmpty)
            return new ArchetypeView { Archetype = archetype };

        return new ArchetypeView
        {
            Archetype = archetype,
            Groups = BuildGroups(graph, entries),
        };
    }

    private static ImmutableArray<(EntryPointKind, EntryPoint)> CollectDesktopEntries(CodeGraph graph)
    {
        var result = ImmutableArray.CreateBuilder<(EntryPointKind, EntryPoint)>();
        foreach (var flow in graph.Flows)
        {
            if (flow.Entry.Kind == EntryPointKind.UiEntry)
                result.Add((flow.Entry.Kind, flow.Entry));
        }
        foreach (var node in graph.Nodes)
        {
            if (node.Kind != NodeKind.EntryPoint) continue;
            // Desktop entries often have UiEntry-kind nodes tagged as windows/forms
            if (node.Tags.Contains("wpf") || node.Tags.Contains("winforms")
                || node.Tags.Contains("desktop") || node.Tags.Contains("ui"))
            {
                result.Add((EntryPointKind.UiEntry,
                    new EntryPoint(EntryPointKind.UiEntry, node.Title, node.Id) { Project = node.Project }));
            }
        }
        return result.ToImmutable();
    }

    private static ImmutableArray<(EntryPointKind, EntryPoint)> CollectWorkerEntries(CodeGraph graph)
    {
        var result = ImmutableArray.CreateBuilder<(EntryPointKind, EntryPoint)>();
        foreach (var flow in graph.Flows)
        {
            var k = flow.Entry.Kind;
            if (k == EntryPointKind.HostedService
                || k == EntryPointKind.ScheduledJob
                || k == EntryPointKind.FunctionEntry)
            {
                result.Add((k, flow.Entry));
            }
        }
        return result.ToImmutable();
    }

    private static ImmutableArray<(EntryPointKind, EntryPoint)> CollectBlazorEntries(CodeGraph graph)
    {
        var result = ImmutableArray.CreateBuilder<(EntryPointKind, EntryPoint)>();
        foreach (var flow in graph.Flows)
        {
            if (flow.Entry.Kind == EntryPointKind.HttpEndpoint)
                result.Add((flow.Entry.Kind, flow.Entry));
        }
        return result.ToImmutable();
    }

    private static ImmutableArray<(EntryPointKind, EntryPoint)> CollectCliEntries(CodeGraph graph)
    {
        // D1.1d — the command surface: parser-based commands and plain-Main fallback entries.
        var result = ImmutableArray.CreateBuilder<(EntryPointKind, EntryPoint)>();
        foreach (var flow in graph.Flows)
        {
            if (flow.Entry.Kind == EntryPointKind.CliCommand)
                result.Add((flow.Entry.Kind, flow.Entry));
        }
        return result.ToImmutable();
    }

    private static ImmutableArray<(EntryPointKind, EntryPoint)> CollectPublicApiEntries(CodeGraph graph)
    {
        var result = ImmutableArray.CreateBuilder<(EntryPointKind, EntryPoint)>();
        foreach (var flow in graph.Flows)
        {
            if (flow.Entry.Kind == EntryPointKind.PublicApi)
                result.Add((flow.Entry.Kind, flow.Entry));
        }
        return result.ToImmutable();
    }

    private static ImmutableArray<ArchetypeEntryGroup> BuildGroups(
        CodeGraph graph, ImmutableArray<(EntryPointKind Kind, EntryPoint Entry)> entries)
    {
        return entries
            .GroupBy(e => e.Entry.Project ?? "")
            .OrderByDescending(g => g.Count())
            .Select(g =>
            {
                var projectName = g.Key;
                var representativeNode = graph.Nodes.FirstOrDefault(n =>
                    n.Project == projectName && n.Layer is not null);
                return new ArchetypeEntryGroup
                {
                    Project = projectName.Length > 0 ? projectName : "(no project)",
                    Layer = representativeNode?.Layer,
                    Entries = g
                        .OrderByDescending(e => e.Entry.Score)
                        .Select(e => new ArchetypeEntryRow
                        {
                            Kind = e.Kind.ToString(),
                            Title = e.Entry.Title,
                            Route = e.Entry.Route,
                            Target = e.Entry.Target,
                            GroupPath = e.Entry.GroupPath,
                            Depth = 1,
                            Hops = e.Entry.CrossProjects,
                            Score = e.Entry.Score,
                        })
                        .ToImmutableArray(),
                };
            })
            .ToImmutableArray();
    }
}
