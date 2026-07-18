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

        // ── Graph-bound operations ──
        // T3.7 — entrypoints/stats/trace now run against the snapshot's GraphQuery with the same JSON
        // shape as the MCP tools (one shape, two transports), instead of falling through to the overview
        // render (which ignored --focus and returned the map payload for all three).
        if (settings.Op is "node" or "neighbors" or "usages" or "search" or "entrypoints" or "stats" or "trace")
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
                "entrypoints" => EntrypointsOp(query),
                "stats" => StatsOp(query, snapshot.Graph, snapshot.Model, snapshot.Insights),
                "trace" => TraceOp(query, settings.Focus ?? "", settings.Depth ?? 6),
                _ => null
            };

            if (output is null)
            {
                AnsiConsole.MarkupLine("[red]--focus is required for this operation.[/]");
                return 1;
            }

            // T3.7 — write machine-readable JSON to RAW stdout. AnsiConsole wraps at the console width,
            // which splits long file:line strings mid-value and corrupts the JSON for a parser.
            Console.WriteLine(JsonSerializer.Serialize(output, JsonOpts));
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

    // T3.7 — entrypoints: entry list (ranked by score) + per-kind counts. Same shape as the MCP tool.
    private static object EntrypointsOp(DevContext.Core.Graph.GraphQuery query)
    {
        var entries = query.EntryPoints();
        var byKind = entries.GroupBy(e => e.Kind.ToString())
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());
        return new
        {
            count = entries.Length,
            byKind,
            entries = entries.OrderByDescending(e => e.Score).Select(e => new
            {
                kind = e.Kind.ToString(),
                title = e.Title,
                nodeId = e.Node.Key,
                httpMethod = e.HttpMethod,
                route = e.Route,
                target = e.Target,
                project = e.Project,
                score = e.Score,
                provenance = e.Provenance,
            }).ToArray(),
        };
    }

    // T3.7 — stats: graph counts + per-kind entry counts + seam breakdown (verified/approx).
    private static object StatsOp(DevContext.Core.Graph.GraphQuery query, DevContext.Core.Graph.CodeGraph graph,
        DevContext.Core.Models.DiscoveryModel model, ImmutableArray<DevContext.Core.Insights.Insight> insights)
    {
        var (seams, entriesWithTarget, entriesWithDeepSpine, deepSpineRatio) = query.Stats();
        var entries = query.EntryPoints();
        var byKind = entries.GroupBy(e => e.Kind.ToString())
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());
        return new
        {
            nodeCount = graph.NodeCount,
            edgeCount = graph.EdgeCount,
            entryCount = entries.Length,
            entriesWithTarget,
            entriesWithDeepSpine,
            deepSpineRatio,
            entriesByKind = byKind,
            seams = seams.Select(s => new { kind = s.Seam, total = s.Count, verified = s.Count - s.Approx, approx = s.Approx }).ToArray(),
            // J1/J3 — per-component swallowed-failure counters (empty = clean run)
            extractionFailures = model.ExtractionFailures
                .Select(f => new { source = f.Source, category = f.Category, count = f.Count, sample = f.SampleException })
                .ToArray(),
            // I2 — insights ride the stats surface so the validity harness can machine-check claims
            insights = insights.Select(i => new
            {
                id = i.Id,
                category = i.Category.ToString(),
                severity = i.Severity.ToString(),
                title = i.Title,
                evidence = i.Evidence.ToArray(),
                confidence = i.Confidence,
                confidenceBasis = i.ConfidenceBasis,
            }).ToArray(),
        };
    }

    // T3.7 — trace: GraphQuery.Trace(focus, depth) as a JSON tree. Honors --focus/--depth (the render
    // fallback ignored both). Returns null on empty focus so the shared "--focus required" path fires.
    private static object? TraceOp(DevContext.Core.Graph.GraphQuery query, string focus, int depth)
    {
        if (string.IsNullOrWhiteSpace(focus)) return null;
        var trace = query.Trace(focus, depth);
        if (trace is null) return new { found = false, focus, error = $"No entry or node matched '{focus}'" };
        return new
        {
            found = true,
            focus,
            entry = new { title = trace.Entry.Title, kind = trace.Entry.Kind.ToString() },
            root = SerializeStep(trace.Root),
            touchedEntities = trace.TouchedEntities.ToArray(),
            emittedEvents = trace.EmittedEvents.ToArray(),
        };
    }

    private static object SerializeStep(DevContext.Core.Graph.TraceStep step) => new
    {
        title = step.Node.Title,
        kind = step.Node.Kind.ToString(),
        nodeId = step.Node.Id.Key,
        seam = step.Seam.ToString(),
        resolution = step.Resolution.ToString(),
        filePath = step.Node.FilePath,
        lineNumber = step.Node.LineNumber,
        // C5 — the JSON surface carries the same honesty annotations the text render shows:
        // the site that led here, multi-impl/multi-host counts, and the test-only flag.
        provenance = step.Provenance,
        multiImplCount = step.MultiImplCount > 1 ? step.MultiImplCount : (int?)null,
        diHostCount = step.DiHostCount > 1 ? step.DiHostCount : (int?)null,
        testOnly = step.TestOnly ? true : (bool?)null,
        truncated = step.Truncated,
        omitted = step.Omitted > 0 ? step.Omitted : (int?)null,
        children = step.Children.Select(SerializeStep).ToArray(),
    };

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
