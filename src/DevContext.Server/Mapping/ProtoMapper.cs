using DevContext.Core.Insights;
using DevContext.Server.Sessions;

using Proto = DevContext.Protos;

namespace DevContext.Server.Mapping;

internal static class ProtoMapper
{
    public static Proto.AnalysisSummary ToSummary(EngineResult engine, AnalysisSnapshot snapshot, int entriesWithTarget)
    {
        var summary = new Proto.AnalysisSummary
        {
            Label = engine.Label,
            Projects = engine.ProjectCount,
            Nodes = snapshot.Graph?.NodeCount ?? 0,
            Edges = snapshot.Graph?.EdgeCount ?? 0,
            Entries = snapshot.Entries.Length,
            EntriesWithTarget = entriesWithTarget,
            ElapsedMs = engine.ElapsedMs,
            Explanation = engine.Explanation,
            IsLibrary = snapshot.Map?.Archetype == Archetype.Library,
        };
        summary.Warnings.AddRange(engine.Warnings);
        return summary;
    }

    public static Proto.EntryPoint ToProto(EntryPoint e)
    {
        var p = new Proto.EntryPoint
        {
            Kind = e.Kind.ToString(),
            Title = e.Title,
            NodeId = e.Node.ToString(),
        };
        if (e.HttpMethod is { } m) p.HttpMethod = m;
        if (e.Route is { } r) p.Route = r;
        if (e.Provenance is { } pr) p.Provenance = pr;
        if (e.Project is { } proj) p.Project = proj;
        if (e.Target is { } t) p.Target = t;
        return p;
    }

    public static Proto.MapResponse ToMapResponse(MapModel? map, string markdown)
    {
        var resp = new Proto.MapResponse
        {
            Markdown = markdown,
            Style = map?.Style ?? "Unknown",
            StyleConfidence = map?.StyleConfidence ?? 0f,
            ProjectCount = map?.Topology.Length ?? 0,
            IsLibrary = map?.Archetype == Archetype.Library,
            Archetype = map?.Archetype.ToString() ?? "App",
        };

        if (map is null) return resp;

        if (map.ScopeNote is { } note) resp.ScopeNote = note;
        if (map.StyleEvidence is { } ev) resp.StyleEvidence = ev;

        foreach (var pn in map.Topology)
            resp.Topology.Add(new Proto.ProjectNode { Name = pn.Name, DependsOn = { pn.DependsOn } });

        foreach (var pg in map.Packages)
            resp.Packages.Add(new Proto.PackageGroup { Label = pg.Label, Packages = { pg.Packages } });

        resp.Aggregates.AddRange(map.Aggregates);
        resp.PipelineBehaviors.AddRange(map.PipelineBehaviors);

        if (map.Surface is { } surface)
        {
            resp.Surface = new Proto.LibrarySurface();
            foreach (var g in surface.Groups)
                resp.Surface.Groups.Add(MapSurfaceGroup(g));
            resp.Surface.ExtensionPoints.AddRange(surface.ExtensionPoints);
        }

        return resp;
    }

    public static Proto.TraceResponse ToTraceResponse(Trace trace, string markdown)
    {
        var resp = new Proto.TraceResponse
        {
            Found = true,
            Markdown = markdown,
            Root = ToProto(trace.Root),
        };
        resp.TouchedEntities.AddRange(trace.TouchedEntities);
        resp.EmittedEvents.AddRange(trace.EmittedEvents);
        return resp;
    }

    public static Proto.NodeResponse ToNodeResponse(NodeDetail d)
    {
        var resp = new Proto.NodeResponse
        {
            Found = true,
            NodeId = d.Id.ToString(),
            Title = d.Title,
            Kind = d.Kind.ToString(),
            OutDegree = d.OutDegree,
            InDegree = d.InDegree,
        };
        if (d.FilePath is { } f) resp.FilePath = f;
        resp.Tags.AddRange(d.Tags);
        return resp;
    }

    public static Proto.Edge ToProto(EdgeRef e)
    {
        var edge = new Proto.Edge
        {
            From = e.From.ToString(),
            To = e.To.ToString(),
            Kind = e.Kind.ToString(),
            Resolution = e.Resolution.ToString(),
            OtherTitle = e.OtherTitle,
        };
        if (e.Provenance is { } p) edge.Provenance = p;
        return edge;
    }

    public static Proto.SearchResponse ToSearchResponse(IReadOnlyList<(string Id, string Title, string Kind, ImmutableArray<string> Tags)> nodes)
    {
        var resp = new Proto.SearchResponse();
        foreach (var (id, title, kind, tags) in nodes)
        {
            var nr = new Proto.NodeRef { NodeId = id, Title = title, Kind = kind };
            nr.Tags.AddRange(tags);
            resp.Nodes.Add(nr);
        }
        return resp;
    }

    public static Proto.StatsResponse ToStatsResponse(
        RunReport? report,
        int nodeCount, int edgeCount, int entryCount,
        ImmutableArray<SeamStat> seams, int entriesWithTarget,
        long totalWallMs,
        ImmutableArray<Insight> insights)
    {
        var resp = new Proto.StatsResponse { TotalWallMs = totalWallMs };

        resp.Graph = new Proto.GraphStat { Nodes = nodeCount, Edges = edgeCount, Entries = entryCount, EntriesWithTarget = entriesWithTarget };

        foreach (var s in seams)
            resp.Seams.Add(new Proto.SeamStat { Seam = s.Seam, Count = s.Count, Approx = s.Approx });

        foreach (var i in insights)
        {
            var pi = new Proto.Insight
            {
                Id = i.Id,
                Category = i.Category.ToString(),
                Severity = i.Severity.ToString(),
                Title = i.Title,
                Detail = "",
            };
            pi.Evidence.AddRange(i.Evidence);
            resp.Insights.Add(pi);
        }

        if (report is null) return resp;

        foreach (var s in report.Stages)
            resp.Stages.Add(new Proto.StageStat { Stage = s.Stage, ElapsedMs = (long)s.Elapsed.TotalMilliseconds });

        foreach (var e in report.Extractors)
            resp.Extractors.Add(new Proto.ExtractorStat
            {
                Name = e.Name,
                Tier = e.Tier,
                ElapsedMs = (long)e.Elapsed.TotalMilliseconds,
                TypesAdded = e.TypesAdded,
                DetectionsAdded = e.DetectionsAdded,
                Skipped = e.Skipped,
                SkipReason = e.SkipReason ?? string.Empty,
            });

        resp.Corpus = new Proto.CorpusStat { TotalFiles = report.Corpus.TotalFiles, CsharpFiles = report.Corpus.CSharpFiles, Projects = report.Corpus.Projects };

        resp.Cache = new Proto.CacheStat
        {
            TextHits = report.Cache.TextHits,
            TextMisses = report.Cache.TextMisses,
            SyntaxTreeHits = report.Cache.SyntaxTreeHits,
            SyntaxTreeMisses = report.Cache.SyntaxTreeMisses,
        };

        resp.Funnel = new Proto.FunnelStat
        {
            TypesDiscovered = report.Funnel.TypesDiscovered,
            TypesIncluded = report.Funnel.TypesIncluded,
            RawTokens = report.Funnel.RawEstimatedTokens,
            RenderedTokens = report.Funnel.RenderedEstimatedTokens,
            Budget = report.Funnel.Budget,
        };

        return resp;
    }

    public static Proto.RenderResponse ToRenderResponse(RenderedContext ctx)
    {
        var resp = new Proto.RenderResponse
        {
            Content = ctx.Content,
            Format = "markdown",
            EstimatedTokens = ctx.EstimatedTokens,
        };

        foreach (var s in ctx.Sections)
            resp.Sections.Add(new Proto.SectionInfo { Key = s.Name, Tokens = s.Tokens });

        return resp;
    }

    private static Proto.TraceNode ToProto(TraceStep step)
    {
        var node = new Proto.TraceNode
        {
            NodeId = step.Node.Id.ToString(),
            Title = step.Node.Title,
            Kind = step.Node.Kind.ToString(),
            Seam = step.Seam.ToString(),
            Depth = step.Depth,
            Resolution = step.Resolution.ToString(),
            Truncated = step.Truncated,
            Omitted = step.Omitted,
        };
        if (step.Provenance is { } prov) node.Provenance = prov;
        if (!step.Salient.IsDefaultOrEmpty) node.Salient = string.Join('\n', step.Salient);
        node.Tags.AddRange(step.Node.Tags);
        if (!step.Pipeline.IsDefaultOrEmpty) node.Pipeline.AddRange(step.Pipeline);
        foreach (var child in step.Children)
            node.Children.Add(ToProto(child));
        return node;
    }

    private static Proto.SurfaceGroup MapSurfaceGroup(SurfaceGroup g)
    {
        var sg = new Proto.SurfaceGroup { Namespace = g.Namespace };
        foreach (var t in g.Types)
        {
            var st = new Proto.SurfaceType { Name = t.Name, Kind = t.Kind.ToString() };
            st.Members.AddRange(t.Members);
            sg.Types_.Add(st);
        }
        return sg;
    }
}
