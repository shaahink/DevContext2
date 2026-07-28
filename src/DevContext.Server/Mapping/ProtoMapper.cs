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
            Stale = engine.Stale,
            StaleMessage = engine.StaleMessage ?? string.Empty,
        };
        summary.Warnings.AddRange(engine.Warnings);
        return summary;
    }

    public static Proto.EntryPoint ToProto(EntryPoint e, string? layer = null, string? feature = null)
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
        if (e.GroupPath is { } gp) p.GroupPath = gp;
        if (!e.AuthAttributes.IsDefaultOrEmpty) p.AuthAttributes.AddRange(e.AuthAttributes);
        p.Score = e.Score;
        p.Reach = e.Reach;
        p.CrossProjects = e.CrossProjects;
        if (layer is { } l) p.Layer = l;
        if (feature is { } f) p.Feature = f;
        return p;
    }

    public static Proto.MapResponse ToMapResponse(MapModel? map, string markdown, string? solutionName = null,
        SolutionScopeNote? scope = null)
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
        if (solutionName is { Length: > 0 }) resp.SolutionName = solutionName;

        // R3 D-D: the FACTS of the choice, not the CLI's sentence about it. Only when there was a
        // choice — a one-solution repo has no scope to report and would just be noise on every page.
        if (scope is { IsPartial: true })
        {
            resp.SolutionScope = new Proto.SolutionScope
            {
                AnalyzedRelPath = scope.AnalyzedRelPath,
                AnalyzedName = scope.AnalyzedName,
                TotalOnDisk = scope.TotalOnDisk,
                WasRequested = scope.WasRequested,
                OtherPaths = { scope.OtherPaths },
            };
        }

        if (map is null) return resp;

        if (map.ScopeNote is { } note) resp.ScopeNote = note;
        if (map.StyleEvidence is { } ev) resp.StyleEvidence = ev;

        foreach (var pn in map.Topology)
        {
            var protoPn = new Proto.ProjectNode { Name = pn.Name, DependsOn = { pn.DependsOn } };
            if (pn.Layer is { } l) protoPn.Layer = l;
            if (pn.Feature is { } f) protoPn.Feature = f;
            resp.Topology.Add(protoPn);
        }

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
            // D4.4 — the full capability-grouped surface (was dropped: markdown-only).
            foreach (var e in surface.EntryApi)
            {
                var pe = new Proto.SurfaceEntry { Title = e.Title, Kind = e.Kind };
                if (e.Doc is { Length: > 0 } d) pe.Doc = d;
                if (e.Location is { Length: > 0 } loc) pe.Location = loc;
                resp.Surface.EntryApi.Add(pe);
            }
            foreach (var a in surface.Abstractions)
                resp.Surface.Abstractions.Add(new Proto.SurfaceAbstraction
                {
                    Name = a.Name,
                    Kind = a.Kind.ToString().ToLowerInvariant(),
                    ImplementorCount = a.ImplementorCount,
                });
            foreach (var g in surface.Internals)
                resp.Surface.Internals.Add(MapSurfaceGroup(g));
            resp.Surface.ConsumerPaths.AddRange(surface.ConsumerPaths);
            foreach (var gen in surface.Generators)
            {
                var pg2 = new Proto.SurfaceGenerator { Name = gen.Name, Kind = gen.Kind };
                if (gen.Doc is { Length: > 0 } d) pg2.Doc = d;
                resp.Surface.Generators.Add(pg2);
            }
            foreach (var pkg in surface.Packages)
                resp.Surface.Packages.Add(new Proto.PackageGroup { Label = pkg.Label, Packages = { pkg.Packages } });
        }

        // M1.9 / D5 — per-service styles
        foreach (var ss in map.ServiceStyles)
            resp.ServiceStyles.Add(new Proto.ServiceStyle
            {
                ProjectName = ss.ProjectName,
                Style = ss.Style,
                Stack = { ss.Stack },
            });

        // L7.2 — archetype-specific entry-point view
        if (map.ArchetypeView is { IsRelevant: true } view)
        {
            var pv = new Proto.ArchetypeView
            {
                Archetype = view.Archetype.ToString(),
                SectionLabel = view.SectionLabel,
            };
            foreach (var g in view.Groups)
            {
                var pg = new Proto.ArchetypeEntryGroup { Project = g.Project };
                if (g.Layer is { } l) pg.Layer = l;
                foreach (var e in g.Entries)
                {
                    var pe = new Proto.ArchetypeEntryRow
                    {
                        Kind = e.Kind,
                        Title = e.Title,
                        Depth = e.Depth,
                        Hops = e.Hops,
                        Score = e.Score,
                    };
                    if (e.Route is { } r) pe.Route = r;
                    if (e.Target is { } t) pe.Target = t;
                    if (e.GroupPath is { } gp) pe.GroupPath = gp;
                    pg.Entries.Add(pe);
                }
                pv.Groups.Add(pg);
            }
            resp.ArchetypeView = pv;
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

    public static Proto.NodeResponse ToNodeResponse(NodeDetail d, string? layer = null, string? feature = null)
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
        if (d.FilePath is { } fp) resp.FilePath = fp;
        if (d.LineNumber is { } ln) resp.LineNumber = ln;
        resp.Tags.AddRange(d.Tags);
        if (layer is { } ly) resp.Layer = ly;
        if (feature is { } ft) resp.Feature = ft;
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
        CodeGraph? graph,
        int nodeCount, int edgeCount, int entryCount,
        ImmutableArray<SeamStat> seams, int entriesWithTarget,
        long totalWallMs,
        ImmutableArray<Insight> insights,
        ImmutableArray<DevContext.Core.Graph.EntryPoint> entries,
        IReadOnlyList<DevContext.Core.Models.SwallowedFailure>? extractionFailures = null)
    {
        var resp = new Proto.StatsResponse { TotalWallMs = totalWallMs };

        // J1/J3 — swallowed-failure counters ride the stats surface
        foreach (var f in extractionFailures ?? [])
            resp.ExtractionFailures.Add(new Proto.SwallowedFailureStat
            {
                Source = f.Source,
                Category = f.Category,
                Count = f.Count,
                Sample = f.SampleException ?? "",
            });

        resp.Graph = new Proto.GraphStat
        {
            Nodes = nodeCount,
            Edges = edgeCount,
            Entries = entryCount,
            EntriesWithTarget = entriesWithTarget,
            SparseGraph = graph?.IsSparseGraph ?? false,
            HubScopeNodes = graph?.HubScopeNodeCount ?? 0,
        };

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
                Confidence = i.Confidence,
                Action = i.PrimaryAction?.Kind.ToString() ?? "None",
            };
            if (i.ConfidenceBasis is { } cb) pi.ConfidenceBasis = cb;
            if (i.WhyItMatters is { } wm) pi.WhyItMatters = wm;
            if (i.PrimaryAction is { } pa && !pa.IsNone) pi.ActionTarget = pa.Target;
            pi.Evidence.AddRange(i.Evidence);
            if (!i.EvidenceActions.IsDefaultOrEmpty)
            {
                foreach (var ea in i.EvidenceActions)
                {
                    pi.EvidenceActions.Add(ea is { } a && !a.IsNone
                        ? $"{a.Kind}:{a.Target}" : "");
                }
            }
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

        // L4.3 — Confidence Ledger
        if (graph is not null && !entries.IsDefaultOrEmpty)
        {
            var ledger = ConfidenceLedger.Compute(graph, entries);
            var pl = resp.ConfidenceLedger = new Proto.ConfidenceLedger
            {
                VerifiedEdgePct = ledger.VerifiedEdgePct,
                ApproxEdgePct = ledger.ApproxEdgePct,
                TotalEdges = ledger.TotalEdges,
                AuthCoveragePct = ledger.AuthCoveragePct,
                EndpointsWithAuth = ledger.EndpointsWithAuth,
                TotalEndpoints = ledger.TotalEndpoints,
                EntryTargetPct = ledger.EntryTargetPct,
                EntriesWithTarget = ledger.EntriesWithTarget,
                TotalEntries = ledger.TotalEntries,
            };
            foreach (var s in ledger.PerSeam)
                pl.PerSeam.Add(new Proto.SeamConfidence
                {
                    Seam = s.Seam,
                    Total = s.Total,
                    Verified = s.Verified,
                    Approx = s.Approx,
                });
        }

        return resp;
    }

    public static Proto.ImpactResponse ToImpactResponse(System.Collections.Immutable.ImmutableArray<ImpactResult> results, string direction)
    {
        var resp = new Proto.ImpactResponse
        {
            Direction = direction,
            TotalAffected = results.Length,
        };
        foreach (var r in results)
        {
            var pr = new Proto.ImpactResult
            {
                EntryTitle = r.Title,
                Kind = r.Kind,
                Hops = r.Hops,
                NodeId = r.NodeId.ToString(),
                NodeTitle = r.Title,
                Service = r.Service,
            };
            if (r.FilePath is { } f) pr.FilePath = f;
            if (r.LineNumber is { } l) pr.LineNumber = l;
            resp.Results.Add(pr);
        }
        return resp;
    }

    public static Proto.ContextResponse ToContextResponse(string focus, ContextPack pack)
    {
        var resp = new Proto.ContextResponse
        {
            Found = pack.Found,
            Focus = focus,
            TotalTokens = pack.TotalTokens,
        };
        foreach (var s in pack.Sections)
        {
            var section = new Proto.ContextSection
            {
                Key = s.Section, Tokens = s.Tokens, Content = s.Content,
                Verified = s.Verified, Approx = s.Approx,
            };
            section.SourceLocations.AddRange(s.SourceLocations);
            resp.Sections.Add(section);
        }
        resp.Omitted.AddRange(pack.Omitted);
        return resp;
    }

    /// <summary>T4.5 — per-section staleness verdicts + the cheap whole-repo HEAD drift signal.</summary>
    public static Proto.VerifyContextResponse ToVerifyContextResponse(
        string focus, bool found, string? analyzedHead, string? currentHead,
        System.Collections.Immutable.ImmutableArray<SectionVerification> sections)
    {
        var resp = new Proto.VerifyContextResponse
        {
            Found = found,
            Focus = focus,
            AnalyzedGitHead = analyzedHead ?? "",
            CurrentGitHead = currentHead ?? "",
            AnyStale = sections.Any(s => s.Stale),
        };
        foreach (var s in sections)
        {
            var sv = new Proto.SectionVerification
            {
                Key = s.Section, Stale = s.Stale, FilesChecked = s.FilesChecked,
            };
            foreach (var d in s.Changed)
                sv.Changed.Add(new Proto.FileDelta { File = d.File, Status = d.Status, LineDelta = d.LineDelta });
            resp.Sections.Add(sv);
        }
        return resp;
    }

    public static Proto.ContextPackResponse ToContextPackResponse(MultiContextPack pack)
    {
        var resp = new Proto.ContextPackResponse
        {
            AssembledMarkdown = pack.AssembledMarkdown,
            TotalTokens = pack.TotalTokens,
            AllocatedTokens = pack.AllocatedTokens,
        };
        foreach (var card in pack.Cards)
        {
            var item = new Proto.ContextCardItem
            {
                Type = card.Type,
                Title = card.Title,
                Tokens = card.TotalTokens,
            };
            foreach (var sa in card.Sections)
            {
                var alloc = new Proto.SectionAllocation
                {
                    Key = sa.Section, Tokens = sa.Tokens, Content = sa.Content,
                    Verified = sa.Verified, Approx = sa.Approx,
                };
                alloc.SourceLocations.AddRange(sa.SourceLocations);
                item.Sections.Add(alloc);
            }
            resp.Cards.Add(item);
        }
        resp.Omitted.AddRange(pack.Omitted);
        return resp;
    }

    public static Proto.InterestingPointsResponse ToInterestingPointsResponse(
        System.Collections.Immutable.ImmutableArray<InterestingPoint> points)
    {
        var resp = new Proto.InterestingPointsResponse();
        foreach (var p in points)
        {
            var pp = new Proto.InterestingPoint
            {
                NodeId = p.Id.ToString(),
                Title = p.Title,
                Kind = p.Kind.ToString(),
                Why = p.Why,
            };
            pp.Tags.AddRange(p.Tags);
            resp.Points.Add(pp);
        }
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

    // T7.4 — the server-side flow atlas (per-entry flow stats + top-hub degrees) in one response.
    public static Proto.FlowIndexResponse ToFlowIndexResponse(FlowIndexResult index)
    {
        var resp = new Proto.FlowIndexResponse();
        foreach (var f in index.Flows)
        {
            var row = new Proto.FlowStatRow
            {
                Focus = f.Focus,
                Title = f.Title,
                Kind = f.Kind,
                Found = f.Found,
                NodeCount = f.NodeCount,
                MaxDepth = f.MaxDepth,
                BoundaryCrossings = f.BoundaryCrossings,
                DataTouches = f.DataTouches,
                VerifiedPct = f.VerifiedPct,
                Score = f.Score,
            };
            row.TouchedEntities.AddRange(f.TouchedEntities);
            row.EmittedEvents.AddRange(f.EmittedEvents);
            row.NodeIds.AddRange(f.NodeIds);
            row.HubIds.AddRange(f.HubIds);
            resp.Flows.Add(row);
        }
        foreach (var h in index.HubDegrees)
            resp.HubDegrees.Add(new Proto.HubDegreeRow
            {
                NodeId = h.NodeId,
                InDegree = h.InDegree,
                OutDegree = h.OutDegree,
            });
        return resp;
    }

    public static Proto.GraphFacetsResponse ToGraphFacetsResponse(
        ServiceMapResult serviceMap,
        FlowListResult flowList,
        EntryTableResult entryTable,
        LayerBandResult layerBand,
        ImmutableArray<EventWire> eventWiring = default)
    {
        var resp = new Proto.GraphFacetsResponse();

        // ServiceMap facet
        resp.ServiceMap = new Proto.ServiceMapFacet();
        foreach (var svc in serviceMap.Services)
        {
            var card = new Proto.ServiceCard
            {
                Name = svc.Name,
                DisplayName = svc.DisplayName,
                Kind = svc.Kind,
            };
            if (svc.Layer is { } l) card.Layer = l;
            if (svc.Feature is { } f) card.Feature = f;
            card.Stack.AddRange(svc.Stack);
            foreach (var store in svc.Stores)
                card.Stores.Add(new Proto.ServiceStore { Name = store.Name, ResourceType = store.ResourceType });
            card.Orchestrates.AddRange(svc.Orchestrates);
            resp.ServiceMap.Services.Add(card);
        }
        foreach (var t in serviceMap.Transports)
        {
            var link = new Proto.TransportLink
            {
                FromService = t.FromService,
                ToService = t.ToService,
                Transport = t.Transport,
                Resolution = t.Resolution.ToString(),
            };
            if (t.Evidence is { } ev) link.Evidence = ev;
            resp.ServiceMap.Transports.Add(link);
        }

        // FlowList facet
        resp.FlowList = new Proto.FlowListFacet { TotalFlows = flowList.TotalFlows };
        foreach (var f in flowList.Flows)
        {
            var card = new Proto.FlowCard
            {
                Id = f.Id,
                Title = f.Title,
                Kind = f.Kind,
                Depth = f.Depth,
                Hops = f.Hops,
                Touches = f.Touches,
                Emits = f.Emits,
                Score = f.Score,
            };
            if (f.NodeId is { } nid) card.NodeId = nid;
            if (f.Route is { } rt) card.Route = rt;
            if (f.HttpMethod is { } hm) card.HttpMethod = hm;
            if (f.Target is { } tg) card.Target = tg;
            if (f.GroupPath is { } gp) card.GroupPath = gp;
            resp.FlowList.Flows.Add(card);
        }

        // EntryTable facet
        resp.EntryTable = new Proto.EntryTableFacet();
        foreach (var r in entryTable.Rows)
        {
            var row = new Proto.EntryTableRow
            {
                Kind = r.Kind,
                Title = r.Title,
                Score = r.Score,
                Reach = r.Reach,
                CrossProjects = r.CrossProjects,
            };
            if (r.Route is { } rt) row.Route = rt;
            if (r.Target is { } tgt) row.Target = tgt;
            if (r.Project is { } p) row.Project = p;
            if (r.GroupPath is { } gp) row.GroupPath = gp;
            if (r.Layer is { } ly) row.Layer = ly;
            if (r.Feature is { } ft) row.Feature = ft;
            resp.EntryTable.Rows.Add(row);
        }

        // LayerBand facet
        resp.LayerBand = new Proto.LayerBandFacet();
        resp.LayerBand.Layers.AddRange(layerBand.Layers);
        resp.LayerBand.Features.AddRange(layerBand.Features);
        foreach (var nb in layerBand.NodeBands)
        {
            var band = new Proto.NodeBand { NodeId = nb.NodeId };
            if (nb.Layer is { } l2) band.Layer = l2;
            if (nb.Feature is { } f2) band.Feature = f2;
            resp.LayerBand.NodeBands.Add(band);
        }

        // EventWiring facet (T6.11) — the ONE T2.6 join, verbatim; the Atlas board and
        // one-pager stop re-deriving publisher→event→consumer client-side.
        resp.EventWiring = new Proto.EventWiringFacet();
        if (!eventWiring.IsDefaultOrEmpty)
        {
            static Proto.EventParticipantRow ToRow(EventParticipant p)
            {
                var row = new Proto.EventParticipantRow { NodeId = p.Node.ToString(), Title = p.Title };
                if (p.Service is { } svc) row.Service = svc;
                return row;
            }
            foreach (var w in eventWiring)
            {
                var wire = new Proto.EventWireRow
                {
                    EventName = w.EventName,
                    IsIntegration = w.IsIntegration,
                    IsCrossService = w.IsCrossService,
                    IsOrphan = w.IsOrphan,
                };
                foreach (var p in w.Publishers) wire.Publishers.Add(ToRow(p));
                foreach (var c in w.Consumers) wire.Consumers.Add(ToRow(c));
                resp.EventWiring.Wires.Add(wire);
            }
            resp.EventWiring.IntegrationCount = eventWiring.Count(w => w.IsIntegration);
            resp.EventWiring.CrossServiceCount = eventWiring.Count(w => w.IsCrossService);
        }

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
        if (step.Node.Layer is { } l) node.Layer = l;
        if (step.Node.Feature is { } f) node.Feature = f;
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
            if (t.Doc is { Length: > 0 } d) st.Doc = d;
            sg.Types_.Add(st);
        }
        return sg;
    }
}
