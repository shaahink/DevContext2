using System.Text.Json;
using System.Text.Json.Serialization;

using DevContext.Cli.Settings;
using DevContext.Core.Pipeline;
using DevContext.Core.Services;

using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace DevContext.Cli.Commands;

public sealed class QueryCommand : AsyncCommand<QuerySettings>
{
    private readonly IFileSystem _fs;
    private readonly ILoggerFactory _loggerFactory;
    private readonly DiscoveryPipeline _pipeline;

    private static readonly HashSet<string> ValidOps = new(StringComparer.OrdinalIgnoreCase)
    {
        "entrypoints", "map", "trace", "stats", "node", "neighbors", "usages", "search",
    };

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public QueryCommand(IFileSystem fs, ILoggerFactory loggerFactory, DiscoveryPipeline pipeline)
    {
        _fs = fs;
        _loggerFactory = loggerFactory;
        _pipeline = pipeline;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, QuerySettings settings, CancellationToken ct)
    {
        if (!ValidOps.Contains(settings.Op))
        {
            AnsiConsole.MarkupLine($"[red]Unknown op '{settings.Op}'. Valid: {string.Join(", ", ValidOps.Order())}[/]");
            return 1;
        }

        var path = settings.Path ?? ".";
        var rootResult = await ProjectRootResolver.ResolveAsync(path, _fs, ct);

        var options = new ExtractionOptions
        {
            Profile = ExtractionProfile.Focused,
            AllowRoslyn = true,
            BuildFullGraph = true,
            OutputFormat = OutputFormat.Json,
        };

        var scenario = ScenarioRegistry.BuiltIn["overview"];
        var cache = new AnalysisCache(_fs);
        var analysis = new SharedAnalysisContext();

        var ctx = new DiscoveryContext
        {
            RootPath = rootResult.EffectiveRootPath,
            Options = options,
            ActiveScenario = scenario,
            Observer = new NullDiscoveryObserver(),
            FileSystem = _fs,
            Cache = cache,
            Analysis = analysis,
            Logger = _loggerFactory.CreateLogger("DevContext"),
        };

        var snapshot = await _pipeline.AnalyzeAsync(ctx, ct);

        if (snapshot.IsDryRun)
        {
            AnsiConsole.MarkupLine("[yellow]Dry run only — no data.[/]");
            return 0;
        }

        // ── Graph-bound operations (node, neighbors, usages, search) ──
        if (settings.Op is "node" or "neighbors" or "usages" or "search")
        {
            if (snapshot.Graph is not { NodeCount: > 0 })
            {
                AnsiConsole.MarkupLine("[red]No graph data available.[/]");
                return 1;
            }

            var query = new DevContext.Core.Graph.GraphQuery(
                snapshot.Graph, snapshot.Entries, snapshot.Map);

            object? output = settings.Op switch
            {
                "search" => Search(query, settings.Focus ?? ""),
                "node" => NodeOp(query, settings.Focus ?? ""),
                "neighbors" => NeighborsOp(query, settings.Focus ?? "", settings.Direction ?? "out"),
                "usages" => UsagesOp(query, settings.Focus ?? ""),
                _ => null
            };

            if (output is null)
            {
                AnsiConsole.MarkupLine("[red]--focus is required for this operation.[/]");
                return 1;
            }

            AnsiConsole.WriteLine(JsonSerializer.Serialize(output, JsonOpts));
            return 0;
        }

        var format = settings.Format?.ToLowerInvariant() == "md" ? "markdown" : "json";
        var request = new RenderRequest
        {
            Format = format,
            MaxTokens = 8000,
            Sections = scenario.RequiredSections,
        };

        var result = await _pipeline.RenderAsync(snapshot, request, ct);
        AnsiConsole.WriteLine(result.Content);
        return 0;
    }

    private static object? NodeOp(DevContext.Core.Graph.GraphQuery query, string focus)
    {
        if (string.IsNullOrWhiteSpace(focus)) return null;

        var id = query.ResolveNodeId(focus);
        if (id is null) return new { error = $"No node matched '{focus}'" };

        var node = query.Node(id.Value);
        if (node is null) return new { error = $"Node '{id}' not found in graph" };

        return new
        {
            node = new
            {
                id = node.Id.Key,
                kind = node.Kind.ToString(),
                title = node.Title,
                filePath = node.FilePath,
                outDegree = node.OutDegree,
                inDegree = node.InDegree,
                tags = node.Tags.ToArray(),
            },
        };
    }

    private static object? NeighborsOp(DevContext.Core.Graph.GraphQuery query, string focus, string direction)
    {
        if (string.IsNullOrWhiteSpace(focus)) return null;

        var id = query.ResolveNodeId(focus);
        if (id is null) return new { error = $"No node matched '{focus}'" };

        var dir = string.Equals(direction, "in", StringComparison.OrdinalIgnoreCase)
            ? DevContext.Core.Graph.EdgeDirection.In
            : DevContext.Core.Graph.EdgeDirection.Out;

        var edges = query.Neighbors(id.Value, dir);
        return new
        {
            node = id.Value.Key,
            direction = dir.ToString().ToLowerInvariant(),
            count = edges.Length,
            edges = edges.Select(e => new
            {
                from = e.From.Key,
                to = e.To.Key,
                kind = e.Kind.ToString(),
                resolution = e.Resolution.ToString(),
                otherTitle = e.OtherTitle,
                provenance = e.Provenance,
            }).ToArray(),
        };
    }

    private static object? UsagesOp(DevContext.Core.Graph.GraphQuery query, string focus)
    {
        if (string.IsNullOrWhiteSpace(focus)) return null;

        var id = query.ResolveNodeId(focus);
        if (id is null) return new { error = $"No node matched '{focus}'" };

        var edges = query.FindUsages(id.Value);
        return new
        {
            node = id.Value.Key,
            count = edges.Length,
            usages = edges.Select(e => new
            {
                caller = e.From.Key,
                kind = e.Kind.ToString(),
                otherTitle = e.OtherTitle,
            }).ToArray(),
        };
    }

    private static object Search(DevContext.Core.Graph.GraphQuery query, string term)
    {
        if (string.IsNullOrWhiteSpace(term)) return new { results = Array.Empty<object>() };

        var hits = query.Search(term);
        return new
        {
            term,
            count = hits.Length,
            results = hits.Select(h => new
            {
                id = h.Id.Key,
                kind = h.Kind.ToString(),
                title = h.Title,
                degree = h.Degree,
            }).ToArray(),
        };
    }
}
