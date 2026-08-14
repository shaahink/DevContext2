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
        "entrypoints", "map", "trace", "stats", "node", "neighbors", "usages", "search", "graphdump",
    };

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    // r1 — graphdump payloads reach ~100k edges on the monolith poles; compact JSON keeps the raw
    // dump files (and the PS 5.1 parse in eval/graph-truth.ps1) tractable.
    private static readonly JsonSerializerOptions JsonOptsCompact = new()
    {
        WriteIndented = false,
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
        var rootResult = await ProjectRootResolver.ResolveAsync(path, _fs, settings.Solution, ct);

        // D3.1 — query options mirror the DEFAULT analyze flavor exactly (config excludes, entry
        // paths, full graph), so query and analyze share one snapshot-cache slot per (repo, tree):
        // the second question after any analyze is a cache load, not a 150s re-analysis.
        var config = DevContextConfig.Load(DevContextConfig.DefaultPath);
        var options = new ExtractionOptions
        {
            EntryPaths = rootResult.EntryCandidates,
            Profile = ExtractionProfile.Focused,
            AllowRoslyn = true,
            BuildFullGraph = true,
            OutputFormat = OutputFormat.Json,
            ExcludePatterns = config?.ExcludePatterns?.ToImmutableArray()
                ?? ExtractionOptions.DefaultExcludePatterns,
            SolutionPath = settings.Solution,
        };

        var scenario = ScenarioRegistry.BuiltIn["overview"];
        var cache = new AnalysisCache(_fs);
        var analysis = new SharedAnalysisContext();

        // K2 (D3.3) — a real report collector, not the null observer: the pipeline builds the stage
        // timeline from it, so query-originated snapshots persist the same analyze waterfall that
        // analyze-originated ones do (and the stats op below can always serve it).
        var collector = new RunReportCollector();
        collector.SetBudget(options.MaxOutputTokens);

        var ctx = new DiscoveryContext
        {
            RootPath = rootResult.EffectiveRootPath,
            ScopedProjectDirs = rootResult.ScopeProjectDirs,
            RequestedSolution = settings.Solution,
            Options = options,
            ActiveScenario = scenario,
            Observer = new CompositeDiscoveryObserver([collector]),
            FileSystem = _fs,
            Cache = cache,
            Analysis = analysis,
            Logger = _loggerFactory.CreateLogger("DevContext"),
        };

        // D3.1 — query ops ride the snapshot cache (stdout stays pure JSON; the honesty stamp goes
        // to stderr). The insight-validity harness (P7) passes --no-cache so its claims-check stays
        // an independent recompute rather than validating the very snapshot it audits.
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var (repoKey, versionKey) = DevContext.Core.Analysis.SnapshotCacheService.ComputeKeys(rootResult.EffectiveRootPath, options);
        var snapCache = new DevContext.Core.Analysis.SnapshotCacheService();
        AnalysisSnapshot? snapshot = null;
        var fromCache = false;
        if (!settings.NoCache && snapCache.Exists(repoKey, versionKey))
        {
            snapshot = await snapCache.TryLoadAsync(repoKey, versionKey, ct);
            if (snapshot is not null)
            {
                fromCache = true;
                snapshot = snapshot with { Options = options, RootPath = rootResult.EffectiveRootPath };
            }
        }

        DevContext.Core.Analysis.SnapshotSaveResult? saveResult = null;
        if (snapshot is null)
        {
            snapshot = await _pipeline.AnalyzeAsync(ctx, ct);
            if (!settings.NoCache && !snapshot.IsDryRun)
                saveResult = await snapCache.SaveAsync(repoKey, versionKey, snapshot, ct);
        }

        // Honesty stamp — CONSOLE ONLY. Redirected stderr stays byte-silent: the gate/eval scripts
        // run PS 5.1 with EAP=Stop, where any redirected native stderr line becomes an ErrorRecord.
        // Machine consumers get the same truth via the stats op's snapshotCache field.
        var shortVer = versionKey[..Math.Min(7, versionKey.Length)];
        var cacheStatus = settings.NoCache
            ? "bypassed (--no-cache)"
            : fromCache
                ? $"HIT · {shortVer}"
                : saveResult switch
                {
                    { Success: true } => $"miss · saved {shortVer}",
                    { Success: false } => $"miss · save FAILED: {saveResult.Error}",
                    _ => "miss · not saved",
                };
        if (!Console.IsErrorRedirected)
            Console.Error.WriteLine($"snapshot cache: {cacheStatus} · {sw.ElapsedMilliseconds}ms");

        if (snapshot.IsDryRun)
        {
            AnsiConsole.MarkupLine("[yellow]Dry run only — no data.[/]");
            return 0;
        }

        // ── Graph-bound operations ──
        // T3.7 — entrypoints/stats/trace now run against the snapshot's GraphQuery with the same JSON
        // shape as the MCP tools (one shape, two transports), instead of falling through to the overview
        // render (which ignored --focus and returned the map payload for all three).
        if (settings.Op is "node" or "neighbors" or "usages" or "search" or "entrypoints" or "stats" or "trace" or "graphdump")
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
                "neighbors" => NeighborsOp(query, settings.Focus ?? "", settings.Direction ?? "out", settings.Kind),
                "usages" => UsagesOp(query, settings.Focus ?? ""),
                "entrypoints" => EntrypointsOp(query),
                "stats" => StatsOp(query, snapshot.Graph, snapshot.Model, snapshot.Insights, cacheStatus, snapshot.Report),
                // Batch E: the default comes from TracePolicy, not from a literal repeated per surface.
                // G2.2: the nullable Depth is passed through, not collapsed here — "the user named a
                // depth" and "the user said nothing" have to stay distinguishable all the way to the
                // engine, or the budget-elastic rule can never fire (it fires only on the second).
                "trace" => TraceOp(query, settings.Focus ?? "", settings.Depth),
                "graphdump" => GraphDumpOp(snapshot.Graph),
                _ => null
            };

            if (output is null)
            {
                AnsiConsole.MarkupLine("[red]--focus is required for this operation.[/]");
                return 1;
            }

            // T3.7 — write machine-readable JSON to RAW stdout. AnsiConsole wraps at the console width,
            // which splits long file:line strings mid-value and corrupts the JSON for a parser.
            var opts = settings.Op.Equals("graphdump", StringComparison.OrdinalIgnoreCase) ? JsonOptsCompact : JsonOpts;
            Console.WriteLine(JsonSerializer.Serialize(output, opts));
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

    private static object? NeighborsOp(DevContext.Core.Graph.GraphQuery query, string focus, string direction,
        string? kindFilter)
    {
        if (string.IsNullOrWhiteSpace(focus)) return null;

        var id = query.ResolveNodeId(focus);
        if (id is null) return new { error = $"No node matched '{focus}'" };

        var dir = string.Equals(direction, "in", StringComparison.OrdinalIgnoreCase)
            ? DevContext.Core.Graph.EdgeDirection.In
            : DevContext.Core.Graph.EdgeDirection.Out;

        // G3.2 — an unknown kind is refused, never treated as "no filter": the alternative is
        // handing back every edge under the name of a filter that never ran. The valid set comes
        // from the enum, so this message cannot drift away from what the engine accepts.
        var requested = string.IsNullOrWhiteSpace(kindFilter) ? null : kindFilter.Trim();
        DevContext.Core.Graph.EdgeKind? kind = null;
        if (requested is not null)
        {
            if (!DevContext.Core.Graph.GraphQuery.TryParseEdgeKind(requested, out var parsed))
            {
                return new
                {
                    error = $"Unknown edge kind '{requested}'",
                    validKinds = DevContext.Core.Graph.GraphQuery.EdgeKindNames.ToArray(),
                };
            }
            kind = parsed;
        }

        var view = query.NeighborsView(id.Value, dir, kind);
        return new
        {
            node = id.Value.Key,
            direction = dir.ToString().ToLowerInvariant(),
            kind = requested,
            count = view.Edges.Length,
            // The unfiltered facts, so a filtered count is readable as a share of something.
            totalEdges = view.TotalEdges,
            kindsPresent = view.KindsPresent.Select(k => new { kind = k.Kind.ToString(), count = k.Count }).ToArray(),
            edges = view.Edges.Select(e => new
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
        DevContext.Core.Models.DiscoveryModel model, ImmutableArray<DevContext.Core.Insights.Insight> insights,
        string snapshotCache, DevContext.Core.Models.RunReport? report)
    {
        var (seams, entriesWithTarget) = query.Stats();
        var entries = query.EntryPoints();
        var byKind = entries.GroupBy(e => e.Kind.ToString())
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());
        return new
        {
            // D3.1 — what the snapshot cache did for THIS invocation (CLI-local; the honesty stamp
            // is console-only so redirected stderr stays clean).
            snapshotCache,
            nodeCount = graph.NodeCount,
            edgeCount = graph.EdgeCount,
            entryCount = entries.Length,
            // S9 contract sweep — L3.4's sparse verdict was computed on every analysis and reported
            // NOWHERE: not here, not in the app, not in the CLI's own chips. It is the caveat that
            // qualifies every edge number beside it, because on a sparse graph call-edge binding was
            // BROADENED over the top central types rather than resolved the ordinary way.
            sparseGraph = graph.IsSparseGraph,
            hubScopeNodes = graph.HubScopeNodeCount,
            entriesWithTarget,
            // R1.1 (#24): entriesWithDeepSpine/deepSpineRatio left this payload with the metric —
            // saturated on 12 poles post-E1 (GraphStats.Compute carries the measurement).
            entriesByKind = byKind,
            // V1.1 (#25): read the tier split off the row. This used to be `verified = total - approx`,
            // which called every Join edge — the enum's default, so every edge nobody labelled —
            // Roslyn-verified, while the desktop app rendered those same edges "approx".
            seams = seams.Select(s => new { kind = s.Seam, total = s.Count, verified = s.Verified, joined = s.Joined, approx = s.Approx }).ToArray(),
            // K2 (D3.3) — the analyze-time waterfall rides the stats surface: stage timeline of the
            // run that PRODUCED this snapshot (persisted, so a cache HIT serves the original run's
            // timings). Empty on pre-D3.3 snapshots — honest, that run recorded no stages.
            totalWallMs = (long)(report?.TotalWall.TotalMilliseconds ?? 0),
            stages = (report?.Stages ?? []).Select(s => new
            {
                stage = s.Stage,
                ms = (long)s.Elapsed.TotalMilliseconds,
            }).ToArray(),
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
    //
    // G2.2 (R4 §1 item 12): the budget comes from TracePolicy, like the depth above it. This op ran
    // UNBUDGETED while the MCP shaped the same focus to its own 4000-token literal, so the two
    // surfaces answered the same question with different trees. One rule, read from one place.
    private static object? TraceOp(DevContext.Core.Graph.GraphQuery query, string focus, int? depth)
    {
        if (string.IsNullOrWhiteSpace(focus)) return null;
        var trace = query.Trace(focus,
            depth ?? DevContext.Core.Graph.TracePolicy.DefaultDepth,
            budgetTokens: DevContext.Core.Graph.TracePolicy.DefaultBudgetTokens,
            explicitDepth: depth is not null);
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

    // r1 — graphdump: the full node/edge inventory for the graph-truth matrix (eval/graph-truth.ps1).
    // A read-only projection of what the builder already assembled — no SourceBody (bulk), no degrees
    // (derivable from edges). Node ids use the "Kind:Key" form so edges join on one string.
    private static object GraphDumpOp(DevContext.Core.Graph.CodeGraph graph) => new
    {
        nodeCount = graph.NodeCount,
        edgeCount = graph.EdgeCount,
        nodes = graph.Nodes.Select(n => new
        {
            id = n.Id.ToString(),
            kind = n.Kind.ToString(),
            title = n.Title,
            project = n.Project,
            tags = n.Tags.IsDefaultOrEmpty ? null : n.Tags.ToArray(),
        }).ToArray(),
        edges = graph.AllEdges.Select(e => new
        {
            from = e.From.ToString(),
            to = e.To.ToString(),
            kind = e.Kind.ToString(),
            resolution = e.Resolution.ToString(),
            tags = e.Tags.IsDefaultOrEmpty ? null : e.Tags.ToArray(),
        }).ToArray(),
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
