using System.Diagnostics;
using System.Reflection;
using System.Text;

using DevContext.Core.Analysis;
using DevContext.Core.Extractors.Generic;
using DevContext.Core.Graph;
using DevContext.Core.Observers;
using DevContext.Core.Rendering;
using DevContext.Core.Validation;

namespace DevContext.Core.Pipeline;

/// <summary>Orchestrates the discovery pipeline: extraction, signal sealing, pruning, compression, and rendering.</summary>
public sealed class DiscoveryPipeline
{
    private readonly IReadOnlyList<IDiscoveryExtractor> _extractors;
    private readonly IReadOnlyList<IPruner> _pruners;
    private readonly IReadOnlyList<ICompressionStrategy> _compressionStrategies;
    private readonly IReadOnlyDictionary<string, IContextRenderer> _renderers;
    private readonly ILogger<DiscoveryPipeline> _logger;
    private readonly IReadOnlyList<string> _validationWarnings;

    /// <summary>Project count at/above which a whole-solution run emits a "narrow your scope" hint (G1 Phase 4).</summary>
    private const int LargeSolutionProjectThreshold = 25;

    /// <summary>Creates a new discovery pipeline with the given extractors, pruners, compressors, and renderers.</summary>
    public DiscoveryPipeline(
        IReadOnlyList<IDiscoveryExtractor> extractors,
        IReadOnlyList<IPruner> pruners,
        IReadOnlyList<ICompressionStrategy> compressionStrategies,
        IReadOnlyDictionary<string, IContextRenderer> renderers,
        ILogger<DiscoveryPipeline> logger)
    {
        _extractors = extractors;
        _pruners = pruners;
        _compressionStrategies = compressionStrategies;
        _renderers = renderers;
        _logger = logger;
        _validationWarnings = ValidateExtractors();
    }

    /// <summary>Validation warnings from extractor configuration checks.</summary>
    public IReadOnlyList<string> ValidationWarnings => _validationWarnings;

    private IReadOnlyList<string> ValidateExtractors()
    {
        var warnings = new List<string>();

        foreach (var extractor in _extractors)
        {
            if ((extractor.Stage == ExecutionStage.Stage1Sequential || extractor.Stage == ExecutionStage.Stage2Parallel)
                && extractor.Capabilities.ReadsSignals.Length > 0)
            {
                var msg = $"Extractor {extractor.Name} (Stage {extractor.Stage}) reads signals {string.Join(", ", extractor.Capabilities.ReadsSignals)} but Generic extractors must not read architecture signals.";
                warnings.Add(msg);
                _logger.LogWarning("{Warning}", msg);
            }
        }

        var stageGroups = _extractors.GroupBy(e => e.Stage);
        foreach (var group in stageGroups)
        {
            var writers = group.Where(e => e.Capabilities.WritesSignals.Length > 0)
                .SelectMany(e => e.Capabilities.WritesSignals.Select(s => (Signal: s, Extractor: e.Name)))
                .ToList();
            var readers = group.Where(e => e.Capabilities.ReadsSignals.Length > 0)
                .SelectMany(e => e.Capabilities.ReadsSignals.Select(s => (Signal: s, Extractor: e.Name)))
                .ToList();

            foreach (var write in writers)
            {
                var matchingReaders = readers.Where(r => r.Signal == write.Signal).ToList();
                if (matchingReaders.Count > 0)
                {
                    var msg = $"Stage {group.Key}: {write.Extractor} writes signal '{write.Signal}' which is also read by {string.Join(", ", matchingReaders.Select(r => r.Extractor))}. This may cause races.";
                    warnings.Add(msg);
                    _logger.LogWarning("{Warning}", msg);
                }
            }
        }

        return warnings;
    }

    /// <summary>Runs extraction, scoring, and compression — returns an immutable snapshot.</summary>
    public async Task<AnalysisSnapshot> AnalyzeAsync(DiscoveryContext context, CancellationToken ct = default)
    {
        if (context.Options.DryRun)
            return await BuildDryRunSnapshotAsync(context, ct);

        if (context.Options.Profile == ExtractionProfile.Debug && _validationWarnings.Count > 0)
            _logger.LogWarning("Strict mode: {Count} validation warning(s) found. Continuing with Debug profile.", _validationWarnings.Count);

        var model = new DiscoveryModel { Budget = new TokenBudget { MaxTokens = context.Options.MaxOutputTokens } };
        context.Observer.OnPipelineStarted(context);

        var collector = (context.Observer as CompositeDiscoveryObserver)?.GetInner()
            .OfType<RunReportCollector>().FirstOrDefault();

        await RunStageAsync(ExecutionStage.Stage1Sequential, PipelineStage.DiscoveryAndCacheWarmup, false, context, model, ct);
        await RunStageAsync(ExecutionStage.Stage2Parallel, PipelineStage.GenericExtraction, true, context, model, ct);

        ResolveFocusPoints(context, model);
        SealSignals(context, model);
        ApplyArchitectureStyle(model);

        await RunStageAsync(ExecutionStage.Stage3Specific, PipelineStage.SpecificExtraction, true, context, model, ct);

        if (context.ActiveScenario.Name is "deep-dive" && context.Options.Profile < ExtractionProfile.Debug)
        {
            model.AddDiagnostic(DiagnosticLevel.Info, "Pipeline",
                $"Scenario '{context.ActiveScenario.DisplayName}' benefits from call graph. " +
                "Re-run with '--profile debug' to enable call graph.");
        }

        // ── GraphAssembly (PLAN-10 Part A) — JOIN detections + types into the connected CodeGraph + Map.
        // Analyze-time, scoped to one solution (R1). The Trace is a render-time lens over snapshot.Graph
        // (PLAN-10 A3/Part C). Runs before scoring/compression; uses stable type ids, not mutated bodies.
        // INVARIANT (Iteration 1 / PRODUCT-DIRECTION.md §8 — "token budget out of the kernel"): the
        // CodeGraph + Map/Trace are assembled HERE, *before* RunScoringAsync (the pruners) and
        // RunCompressionAsync. They never read model.Budget, IsPruned, or RoleScore — the token budget and
        // the legacy pruners drive ONLY the legacy catalog RenderPlan (JSON/HTML). So token budgeting is
        // already structurally out of the kernel; BudgetIndependenceTests locks this (Map/Trace output is
        // invariant across --max-tokens). Do not re-couple budget to graph assembly.
        var scope = SolutionScope.FromModel(model);

        // G1 Phase 4 — perf guardrail. Whole-solution runs (no closure narrowing) over a large solution
        // scale the file walk + graph; surface a hint rather than a hard cap (measured eShop=24 projects
        // ~1.9s — no cliff — and a silent cap would skew output/evals). Pointing at a project triggers the
        // bounded ProjectReference closure instead.
        if (context.ScopedProjectDirs.IsDefaultOrEmpty && scope.Projects.Length >= LargeSolutionProjectThreshold)
        {
            model.AddDiagnostic(DiagnosticLevel.Warning, "Scope",
                $"Whole-solution analysis over {scope.Projects.Length} projects — this can be slow and noisy. "
                + "Point at a specific project (e.g. its folder or .csproj) to analyse just its reference closure.");
        }

        var graphResolver = new SyntacticSymbolResolver();
        var noiseFilter = new NoiseFilter(new ProjectClassifier(model.Projects), context.RootPath);
        var (codeGraph, entryPoints) = new GraphBuilder(graphResolver, noiseFilter).Build(model, scope);

        // W7: scan for ocelot.json files in the project root and extract gateway routes
        PopulateGatewayRoutes(model, context);

        var mapModel = MapBuilder.Build(model, codeGraph, entryPoints);
        model.Archetype = mapModel.Archetype.ToString();
        model.AddDiagnostic(DiagnosticLevel.Info, "GraphAssembly",
            $"graph: {codeGraph.NodeCount} nodes, {codeGraph.EdgeCount} edges, {entryPoints.Length} entry points"
            + (scope.SolutionName is { } sln ? $" (scope: {sln})" : ""));

        await RunScoringAsync(context, model, ct);
        await RunCompressionAsync(context, model, ct);

        context.Observer.OnPipelineCompleted(model);

        if (collector is not null)
        {
            var csharpFiles = context.Analysis.AllSourceFiles?.Count ?? 0;
            collector.SetCorpusFileCounts(0, csharpFiles);

            if (context.Cache is ICacheStatsSource ac)
                collector.SetCacheStats(ac.GetStats());
        }

        return new AnalysisSnapshot
        {
            Model = model,
            Analysis = context.Analysis,
            Scenario = context.ActiveScenario,
            Options = context.Options,
            RootPath = context.RootPath,
            Graph = codeGraph,
            Map = mapModel,
            Entries = entryPoints,
            Report = collector?.Build() ?? new RunReport
            {
                Stages = [],
                Extractors = [],
                Scorers = [],
                Compressions = [],
                Cache = new(0, 0, 0, 0),
                Corpus = new(0, 0, 0),
                Funnel = new(model.Types.Count, 0, model.Types.Count, 0, 0, 0),
                Parallelism = new(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero),
                TotalWall = TimeSpan.Zero,
            },
        };
    }

    private static void ResolveFocusPoints(DiscoveryContext context, DiscoveryModel model)
    {
        var unresolvedCount = context.Analysis.UnresolvedFocusPoints.Count;
        if (unresolvedCount == 0) return;

        var resolved = FocusPointResolver.Resolve(context.Analysis.UnresolvedFocusPoints, model);
        var failedToResolve = resolved
            .Where((fp, i) => fp.Kind is FocusKind.Type or FocusKind.Method
                && string.IsNullOrEmpty(fp.FilePath))
            .ToList();

        foreach (var failed in failedToResolve)
        {
            var didYouMean = SuggestTypeNames(failed.TypeName, model.Types.Values);
            var suggestion = didYouMean.Count > 0
                ? $" Did you mean: {string.Join(", ", didYouMean.Take(3))}?"
                : "";
            model.AddDiagnostic(DiagnosticLevel.Warning, "FocusPointResolver",
                $"--around {failed.TypeName}: type not found in {model.Types.Count} scanned types.{suggestion} "
                + "Falling back to folder-level proximity.");
        }

        context.Analysis.FocusPoints = resolved;
    }

    private static void SealSignals(DiscoveryContext context, DiscoveryModel model)
    {
        context.Observer.OnStageStarted(PipelineStage.SignalSealing);
        var sealSw = Stopwatch.StartNew();
        model.Architecture.Seal();
        context.Observer.OnSignalsSealed(model.Architecture.All);
        context.Observer.OnStageCompleted(PipelineStage.SignalSealing, sealSw.Elapsed);
    }

    private async Task<AnalysisSnapshot> BuildDryRunSnapshotAsync(DiscoveryContext context, CancellationToken ct)
    {
        var dryRunModel = new DiscoveryModel();
        await RunStageAsync(ExecutionStage.Stage1Sequential, PipelineStage.DiscoveryAndCacheWarmup, false, context, dryRunModel, ct);

        var stage1 = new List<(string Name, string Description, bool WillRun)>();
        var stage2 = new List<(string Name, string Description, bool WillRun)>();
        var stage3 = new List<(string Name, string Description, ImmutableArray<string> RequiredSignals)>();

        foreach (var ext in _extractors.OrderBy(GetOrder))
        {
            var willRun = !context.Options.ExcludeExtractors.Contains(ext.Name)
                          && ext.ShouldRun(context, new DiscoveryModel());

            switch (ext.Stage)
            {
                case ExecutionStage.Stage1Sequential:
                    stage1.Add((ext.Name, ext.Capabilities.Description, willRun));
                    break;
                case ExecutionStage.Stage2Parallel:
                    stage2.Add((ext.Name, ext.Capabilities.Description, willRun));
                    break;
                case ExecutionStage.Stage3Specific:
                    stage3.Add((ext.Name, ext.Capabilities.Description, ext.Capabilities.ReadsSignals));
                    break;
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("## Dry Run Plan");
        sb.AppendLine($"**Root**: {context.RootPath}");
        sb.AppendLine($"**Scenario**: {context.ActiveScenario.DisplayName}");
        sb.AppendLine($"**Profile**: {context.Options.Profile}");
        sb.AppendLine($"**Max tokens**: {context.Options.MaxOutputTokens}");
        sb.AppendLine();

        sb.AppendLine("### Stage 1 (sequential)");
        sb.AppendLine("| Status | Name | Description |");
        sb.AppendLine("|---|---|---|");
        foreach (var (name, desc, willRun) in stage1)
            sb.AppendLine($"| {(willRun ? "✓" : "✗")} | {name} | {desc} |");

        sb.AppendLine();
        sb.AppendLine("### Stage 2 (parallel)");
        sb.AppendLine("| Status | Name | Description |");
        sb.AppendLine("|---|---|---|");
        foreach (var (name, desc, willRun) in stage2)
            sb.AppendLine($"| {(willRun ? "✓" : "✗")} | {name} | {desc} |");

        sb.AppendLine();
        sb.AppendLine("### Stage 3 (conditional, after signal detection)");
        sb.AppendLine("| Status | Name | Requires | Description |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var (name, desc, signals) in stage3)
        {
            var requires = signals.Length > 0
                ? string.Join(" OR ", signals)
                : "(always runs)";
            sb.AppendLine($"| ? | {name} | {requires} | {desc} |");
        }
        sb.AppendLine();
        sb.AppendLine("*Stage 3 extractors run conditionally based on Stage 2 signal results. "
            + "Use --scenario or configure signals in devcontext.json to control which run.*");

        if (_validationWarnings.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("### Validation Warnings");
            foreach (var w in _validationWarnings)
                sb.AppendLine($"- ⚠ {w}");
        }

        context.Observer.OnPipelineCompleted(dryRunModel);
        return new AnalysisSnapshot
        {
            Model = dryRunModel,
            Analysis = context.Analysis,
            Scenario = context.ActiveScenario,
            Options = context.Options,
            Report = new RunReport
            {
                Stages = [],
                Extractors = [],
                Scorers = [],
                Compressions = [],
                Cache = new(0, 0, 0, 0),
                Corpus = new(0, 0, 0),
                Funnel = new(0, 0, 0, 0, 0, 0),
                Parallelism = new(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero),
                TotalWall = TimeSpan.Zero,
            },
            IsDryRun = true,
            DryRunContent = sb.ToString(),
            RootPath = context.RootPath,
        };
    }

    /// <summary>Renders from a snapshot according to the request lens. Cheap and repeatable.</summary>
    public async Task<RenderedContext> RenderAsync(AnalysisSnapshot snapshot, RenderRequest request, CancellationToken ct = default)
    {
        // ── PLAN-10 A3: Map/Trace branch — when the Graph is available with content (always after a
        // full analyze). The Map/Trace renderers produce the human-facing markdown narrative; JSON/HTML
        // consumers (programmatic callers, the eval harness) get the structured model from the legacy
        // renderers below, so --format json stays valid, parseable structured data.
        var format = string.IsNullOrEmpty(request.Format) ? "markdown" : request.Format;
        if (format == "md") format = "markdown"; // normalize alias
        var wantsNarrative = format is "markdown";
        if (wantsNarrative && snapshot.Graph is { NodeCount: > 0 } graph)
        {
            // Phase 5: the render path is a CLIENT of the query layer (analyze once, query many) — the
            // CLI/Desktop go through the same GraphQuery the browse UI + MCP will. Output is unchanged.
            var query = new Graph.GraphQuery(graph, snapshot.Entries, snapshot.Map);

            // Graph-shaped stats (per-seam coverage + entry-target count) — the same numbers for the
            // Map and any Trace, so the stats page reflects the whole assembled graph, not the lens.
            var (seams, withTarget) = query.Stats();

            if (!string.IsNullOrEmpty(request.Entry))
            {
                var trace = query.Trace(request.Entry, request.Depth ?? 6, 12);
                if (trace is not null)
                {
                    var traceCtx = NarrativeSections.ToRenderedContext(
                        TraceRenderer.RenderSections(trace, request.Detail, snapshot.RootPath));

                    // Keep the architecture/Map sections visible alongside the trace when requested (the
                    // desktop), so drilling a call stack from any node doesn't hide the orientation view.
                    if (request.IncludeMapWithTrace && snapshot.Map is { } mapForTrace)
                    {
                        var mapForTraceCtx = new MapRenderContext(mapForTrace, snapshot, format, request);
                        var mapNarrative = mapForTrace.Archetype == Archetype.Library
                            ? await LibrarySurfaceRenderer.RenderAsync(mapForTraceCtx, ct)
                            : await MapRenderer.RenderAsync(mapForTraceCtx, ct);
                        traceCtx = NarrativeSections.Combine(mapNarrative, traceCtx);
                    }

                    // W3b: empty-but-resolved trace — append an honest hint that no wiring was found,
                    // so the user isn't left staring at a bare ENTRY line wondering what went wrong.
                    if (trace.Root.Children.Length == 0)
                    {
                        traceCtx = NarrativeSections.WithExtraSection(traceCtx, "TraceHint",
                            $"NOTE: no out-edges resolved for '{request.Entry}' — try `Type:Method`, or `--profile debug` to enable the call graph\n\n");
                    }

                    return NarrativeSections.WithExtraSection(
                        traceCtx, "Diagnostics", GraphDiagnosticsTail(snapshot, request))
                        with { GraphSummary = new GraphSummary(graph.NodeCount, graph.EdgeCount, snapshot.Entries.Length, MaxTraceDepth(trace.Root))
                            { Seams = seams, EntriesWithTarget = withTarget } };
                }
            }

            // No entry chosen — render the Map (app) or the public-surface view (library, G3).
            if (snapshot.Map is { } mapModel)
            {
                var mapCtx = new MapRenderContext(mapModel, snapshot, format, request);
                var narrative = mapModel.Archetype == Archetype.Library
                    ? await LibrarySurfaceRenderer.RenderAsync(mapCtx, ct)
                    : await MapRenderer.RenderAsync(mapCtx, ct);

                // W3b: when a focus was requested but no entry/node matched, explain why the Map
                // is shown instead so the user isn't confused by a silent fallback.
                var focusWasRequested = !string.IsNullOrEmpty(request.Entry);
                if (focusWasRequested)
                {
                    narrative = NarrativeSections.WithExtraSection(narrative, "NoMatch",
                        $"NOTE: no entry/node matched '{request.Entry}' — try the fully-qualified name, `Type:Method`, or `--focus \"<exact map entry>\"`\n\n");
                }

                return NarrativeSections.WithExtraSection(
                    narrative, "Diagnostics", GraphDiagnosticsTail(snapshot, request))
                    with { GraphSummary = new GraphSummary(graph.NodeCount, graph.EdgeCount, snapshot.Entries.Length, null)
                        { Seams = seams, EntriesWithTarget = withTarget } };
            }
        }

        var plan = RenderPlanBuilder.Build(snapshot, request);

        // For JSON output, use the user's budget as the cap (the catalog plan's type-based
        // token estimate doesn't reflect the JSON structure). Markdown keeps the plan estimate
        // which is the actual scored-and-capped token budget for the catalog renderer.
        var budgetForChecks = format == "json" ? request.MaxTokens : plan.EstimatedTokens;

        var opts = new RenderOptions(
            request.IncludeProvenance,
            request.IncludeDiagnostics,
            budgetForChecks,
            snapshot.Scenario.DisplayName,
            ProfileDisplayName: snapshot.Options.Profile.ToString().ToLowerInvariant(),
            plan.Sections,
            snapshot.Analysis.FocusPoints.ToImmutableArray(),
            snapshot.Analysis.CallGraph,
            snapshot.Analysis.ProjectGraph,
            TokenView: request.TokenView)
        {
            Plan = plan,
            Report = snapshot.Report,
        };

        if (!_renderers.TryGetValue(format, out var renderer))
            throw new InvalidOperationException($"No renderer registered for format: {format}");

        var rendered = await renderer.RenderAsync(snapshot.Model, opts, ct);
        rendered = RunSelfChecks(rendered, snapshot.Model, opts, snapshot.Model);

        rendered = rendered with
        {
            RenderFunnel = new TokenFunnel(
                snapshot.Model.Types.Count,
                snapshot.Model.Types.Values.Count(t => t.IsHardExcluded),
                plan.IncludedTypeIds.Length,
                plan.EstimatedTokens,
                rendered.EstimatedTokens,
                request.MaxTokens),
        };

        return rendered;
    }

    /// <summary>Deepest hop reached in a trace tree (for the narrative stats line).</summary>
    private static int MaxTraceDepth(Graph.TraceStep step)
    {
        var max = step.Depth;
        foreach (var child in step.Children)
            max = Math.Max(max, MaxTraceDepth(child));
        return max;
    }

    /// <summary>Resolves a free-text focus (a type or handler name) to a graph node so a Trace can start
    /// from it, not just from a catalogued HTTP entry. Lets <c>--focus OrdersController</c> /
    /// <c>--focus CreateOrderCommandHandler</c> produce a trace instead of silently falling back to the Map.
    /// Prefers behaviour-bearing nodes (Type/Handler/Service) and matches by short name.</summary>
    /// <summary>Appends graph-assembly and call-graph diagnostics under a Map/Trace when --include-diagnostics
    /// is set, so users (and we) can see node/edge counts and the call-graph resolver without the legacy path.</summary>
    private static string GraphDiagnosticsTail(AnalysisSnapshot snapshot, RenderRequest request)
    {
        if (!request.IncludeDiagnostics) return string.Empty;
        var lines = snapshot.Model.Diagnostics
            .Where(d => d.Source is "GraphAssembly" or "CallGraphExtractor" or "GraphBuilder")
            .Select(d => $"  {d.Source}: {d.Message}")
            .ToList();
        if (lines.Count == 0) return string.Empty;
        return "\n\nDIAGNOSTICS\n" + string.Join("\n", lines) + "\n";
    }

    /// <summary>Convenience: runs the full pipeline (analyze + render) and returns the rendered context.</summary>
    public async Task<RenderedContext> RunAsync(DiscoveryContext context, CancellationToken ct = default)
    {
        var snapshot = await AnalyzeAsync(context, ct);
        if (snapshot.IsDryRun)
            return new RenderedContext(snapshot.DryRunContent!, 0, [], TimeSpan.Zero, "2.0");

        var request = new RenderRequest
        {
            Format = context.Options.OutputFormat.ToString().ToLowerInvariant(),
            MaxTokens = context.Options.MaxOutputTokens,
            Sections = context.ActiveScenario.RequiredSections,
            IncludeProvenance = context.Options.IncludeProvenance,
            IncludeDiagnostics = context.Options.IncludeDiagnostics,
            TokenView = context.Options.TokenView,
        };
        return await RenderAsync(snapshot, request, ct);
    }

    /// <summary>Runs a stage of the pipeline: filters extractors by ExecutionStage, executes them sequentially or in parallel.</summary>
    private async Task RunStageAsync(ExecutionStage execStage, PipelineStage stage, bool parallel,
        DiscoveryContext ctx, DiscoveryModel model, CancellationToken ct)
    {
        ctx.Observer.OnStageStarted(stage);
        var sw = Stopwatch.StartNew();

        var stageExtractors = _extractors
            .Where(e => e.Stage == execStage)
            .OrderBy(GetOrder)
            .ToList();

        var excludedByName = stageExtractors
            .Where(e => ctx.Options.ExcludeExtractors.Contains(e.Name))
            .ToList();

        var excludedBySignal = stageExtractors
            .Where(e => !ctx.Options.ExcludeExtractors.Contains(e.Name))
            .Where(e => !e.ShouldRun(ctx, model))
            .ToList();

        var eligible = stageExtractors
            .Where(e => !ctx.Options.ExcludeExtractors.Contains(e.Name))
            .Where(e => e.ShouldRun(ctx, model))
            .ToList();

        // Report skipped extractors
        foreach (var ext in excludedByName)
            ctx.Observer.OnExtractorCompleted(ext.Name, TimeSpan.Zero, true, "excluded by scenario");

        foreach (var ext in excludedBySignal)
        {
            var signals = ext.Capabilities.ReadsSignals;
            var reason = signals.Length > 0
                ? $"signal gate: needs {string.Join(" or ", signals)}"
                : "gated by ShouldRun";
            ctx.Observer.OnExtractorCompleted(ext.Name, TimeSpan.Zero, true, reason);
        }

        if (parallel)
        {
            // Two waves: cheap detectors first, then Deep extractors (the call graph). The call graph's
            // focus-scoping needs the OTHER Stage-3 detections (endpoints, handlers, jobs) to resolve a
            // route/endpoint focus to its seed file; if it raced them it saw none and fell back to a full
            // bind (P1 lost). Nothing in this stage consumes the call graph (it's read post-stage by the
            // graph builder), so deferring the Deep wave is safe and strictly more correct.
            async Task RunWaveAsync(IReadOnlyList<IDiscoveryExtractor> wave)
            {
                await Parallel.ForEachAsync(wave, ct, async (extractor, innerCt) =>
                {
                    var (typesAdded, detsAdded, elapsed) = await RunSingleExtractorAsync(ctx, model, extractor, innerCt);
                    ctx.Observer.OnExtractorCompleted(extractor.Name, elapsed, false, null, typesAdded, detsAdded);
                    RecordMetrics(ctx.Observer, extractor.Name, extractor.Tier, extractor.Category, elapsed, false, typesAdded, detsAdded);
                });
            }

            var deep = eligible.Where(e => e.Tier == ExtractorTier.Deep).ToList();
            var detectors = eligible.Where(e => e.Tier != ExtractorTier.Deep).ToList();
            if (deep.Count > 0 && detectors.Count > 0)
            {
                await RunWaveAsync(detectors);
                await RunWaveAsync(deep);
            }
            else
            {
                await RunWaveAsync(eligible);
            }
        }
        else
        {
            foreach (var extractor in eligible)
            {
                ct.ThrowIfCancellationRequested();
                var (typesAdded, detsAdded, elapsed) = await RunSingleExtractorAsync(ctx, model, extractor, ct);
                ctx.Observer.OnExtractorCompleted(extractor.Name, elapsed, false, null, typesAdded, detsAdded);
                RecordMetrics(ctx.Observer, extractor.Name, extractor.Tier, extractor.Category, elapsed, false, typesAdded, detsAdded);
            }
        }

        ctx.Observer.OnStageCompleted(stage, sw.Elapsed);
    }

    private static async Task<(int TypesAdded, int DetsAdded, TimeSpan Elapsed)> RunSingleExtractorAsync(
        DiscoveryContext ctx, DiscoveryModel model, IDiscoveryExtractor extractor, CancellationToken ct)
    {
        ctx.Observer.OnExtractorStarted(extractor.Name, extractor.Tier);
        var esw = Stopwatch.StartNew();
        var typesBefore = model.Types.Count;
        var detsBefore = model.Detections.Count;
        try { await extractor.ExtractAsync(ctx, model, ct); }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { model.AddDiagnostic(DiagnosticLevel.Warning, extractor.Name, ex.Message); }
        return (model.Types.Count - typesBefore, model.Detections.Count - detsBefore, esw.Elapsed);
    }

    /// <summary>
    /// Architecture style detection runs after signals are sealed (between Stage 2 and 3 per design).
    /// </summary>
    private static void ApplyArchitectureStyle(DiscoveryModel model)
    {
        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        model.DetectedStyle = style;
        model.StyleConfidence = confidence;
        model.StyleDetectedVia = via ?? "ArchitectureStyleDetector";
    }

    /// <summary>Runs output self-checks, records results as diagnostics, and returns the rendered context with failure info attached.</summary>
    private static RenderedContext RunSelfChecks(RenderedContext rendered, DiscoveryModel model, RenderOptions renderOptions, DiscoveryModel diagnosticsTarget)
    {
        var results = OutputSelfCheck.Check(rendered, model, renderOptions);
        var failures = ImmutableArray.CreateBuilder<string>();
        foreach (var result in results)
        {
            if (result.Passed)
                diagnosticsTarget.AddDiagnostic(DiagnosticLevel.Info, "SelfCheck", $"{result.CheckId}: {result.Detail}");
            else
            {
                diagnosticsTarget.AddDiagnostic(DiagnosticLevel.Warning, "SelfCheck", $"{result.CheckId}: {result.Detail}");
                failures.Add($"{result.CheckId}: {result.Detail}");
            }
        }
        return rendered with { SelfCheckFailures = failures.ToImmutable() };
    }

    private async Task RunScoringAsync(DiscoveryContext ctx, DiscoveryModel model, CancellationToken ct)
    {
        // PLAN-10 E1: scoring stage shell kept for observer compatibility; the
        // weighted FinalScore + PathProximityPruner + CallReachabilityPruner are retired.
        // Noise filtering moved to NoiseFilter at graph-build time. Trace reachability
        // is now the TraceBuilder traversal over CodeGraph, not a global flat scorer.
        // NOTE (Iteration 1): these pruners (TokenBudgetEnforcer, PatternRelevancePruner) run AFTER the
        // graph is frozen and mutate only model.Types (IsPruned/RoleScore) for the legacy catalog
        // RenderPlan → JSON/HTML. The Map/Trace graph is independent of them. They are slated to retire
        // together with the catalog render path; until then they stay so JSON/HTML keep sizing.
        ctx.Observer.OnStageStarted(PipelineStage.Scoring);
        var sw = Stopwatch.StartNew();

        foreach (var pruner in _pruners.OrderBy(p => p.Order))
        {
            ct.ThrowIfCancellationRequested();
            var before = model.Types.Values.Count(t => !t.IsPruned);
            await pruner.PruneAsync(ctx, model, ct);
            var after = model.Types.Values.Count(t => !t.IsPruned);
            ctx.Observer.OnPrunerCompleted(pruner.Name, before, after);
        }

        // Simple role-score fallback for legacy render path: detection-bearing types rank higher.
        foreach (var type in model.Types.Values)
            type.FinalScore = type.RoleScore;

        ctx.Observer.OnStageCompleted(PipelineStage.Scoring, sw.Elapsed);
    }

    private async Task RunCompressionAsync(DiscoveryContext ctx, DiscoveryModel model, CancellationToken ct)
    {
        ctx.Observer.OnStageStarted(PipelineStage.Compression);
        var sw = Stopwatch.StartNew();
        var options = new CompressionOptions(model.Budget.MaxTokens, ctx.ActiveScenario.Compression.PerTypeCharCap);

        foreach (var strategy in _compressionStrategies.OrderBy(s => s.Order))
        {
            ct.ThrowIfCancellationRequested();
            var result = await strategy.CompressAsync(model, options, ct);
            model.AppliedCompressions.Add(result);
            ctx.Observer.OnCompressionApplied(result);
        }

        ctx.Observer.OnStageCompleted(PipelineStage.Compression, sw.Elapsed);
    }

    internal static int GetOrder(IDiscoveryExtractor extractor)
    {
        var attr = extractor.GetType().GetCustomAttribute<ExtractorOrderAttribute>(false);
        return attr?.Order ?? 100;
    }

    private static void RecordMetrics(IDiscoveryObserver observer, string name,
        ExtractorTier tier, ExtractorCategory category, TimeSpan elapsed,
        bool skipped, int typesAdded, int detectionsAdded)
    {
        observer.RecordExtractorMetrics(name, tier, category, elapsed, skipped, typesAdded, detectionsAdded);
    }

    private static List<string> SuggestTypeNames(string? input, IEnumerable<TypeDiscovery> types, int maxDistance = 3)
    {
        if (string.IsNullOrEmpty(input)) return [];

        return types
            .Select(t => t.Name)
            .Distinct()
            .Select(name => (Name: name, Distance: StringHelpers.LevenshteinDistance(input, name)))
            .Where(x => x.Distance <= maxDistance && x.Distance > 0)
            .OrderBy(x => x.Distance)
            .ThenBy(x => x.Name)
            .Select(x => x.Name)
            .Take(3)
            .ToList();
    }

    private static void PopulateGatewayRoutes(DiscoveryModel model, DiscoveryContext context)
    {
        if (!model.Architecture.Has(ArchitectureSignals.Keys.Gateway)) return;

        var root = context.RootPath;
        if (!Directory.Exists(root)) return;

        foreach (var file in Directory.EnumerateFiles(root, "ocelot*.json", SearchOption.AllDirectories))
        {
            if (file.Contains("\\.git\\", StringComparison.OrdinalIgnoreCase)
                || file.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase)
                || file.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase))
                continue;
            if (ProjectClassifier.IsSamplePath(file) || ProjectClassifier.IsTestPath(file))
                continue;

            try
            {
                var json = File.ReadAllText(file);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var routes = doc.RootElement.TryGetProperty("Routes", out var r) ? r : default;
                if (routes.ValueKind != System.Text.Json.JsonValueKind.Array) continue;

                foreach (var route in routes.EnumerateArray())
                {
                    var upstream = route.TryGetProperty("UpstreamPathTemplate", out var ut) ? ut.GetString() ?? "" : "";
                    var methods = route.TryGetProperty("UpstreamHttpMethod", out var um) ? FormatMethods(um) : "";
                    var downstream = route.TryGetProperty("DownstreamPathTemplate", out var dt) ? dt.GetString() ?? "" : "";
                    var hosts = route.TryGetProperty("DownstreamHostAndPorts", out var dh) ? FormatHosts(dh) : "";

                    model.GatewayRoutes.Add(new GatewayRoute(upstream, methods, downstream, hosts));
                }
            }
            catch { /* non-json or malformed — skip */ }
        }

        static string FormatMethods(System.Text.Json.JsonElement el)
        {
            if (el.ValueKind == System.Text.Json.JsonValueKind.Array)
                return string.Join(", ", el.EnumerateArray().Select(x => x.GetString() ?? ""));
            return el.GetString() ?? "";
        }

        static string FormatHosts(System.Text.Json.JsonElement el)
        {
            if (el.ValueKind != System.Text.Json.JsonValueKind.Array) return "";
            var hosts = el.EnumerateArray()
                .Select(h => h.TryGetProperty("Host", out var host) ? host.GetString() ?? "" : "")
                .Where(h => h.Length > 0);
            return string.Join(", ", hosts);
        }
    }
}
