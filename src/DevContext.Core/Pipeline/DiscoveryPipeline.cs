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
                var matchingReaders = readers.Where(r => string.Equals(r.Signal, write.Signal, StringComparison.Ordinal)).ToList();
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
            return await BuildDryRunSnapshotAsync(context, ct).ConfigureAwait(false);

        if (context.Options.Profile == ExtractionProfile.Debug && _validationWarnings.Count > 0)
            _logger.LogWarning("Strict mode: {Count} validation warning(s) found. Continuing with Debug profile.", _validationWarnings.Count);

        var model = new DiscoveryModel { Budget = new TokenBudget { MaxTokens = context.Options.MaxOutputTokens } };
        context.Observer.OnPipelineStarted(context);

        var collector = (context.Observer as CompositeDiscoveryObserver)?.GetInner()
            .OfType<RunReportCollector>().FirstOrDefault();

        await RunStageAsync(ExecutionStage.Stage1Sequential, PipelineStage.DiscoveryAndCacheWarmup, false, context, model, ct).ConfigureAwait(false);
        await RunStageAsync(ExecutionStage.Stage2Parallel, PipelineStage.GenericExtraction, true, context, model, ct).ConfigureAwait(false);

        ResolveFocusPoints(context, model);
        SealSignals(context, model);
        ApplyArchitectureStyle(model);

        await RunStageAsync(ExecutionStage.Stage3Specific, PipelineStage.SpecificExtraction, true, context, model, ct).ConfigureAwait(false);

        if (context.ActiveScenario.Name is "deep-dive" && context.Options.Profile < ExtractionProfile.Debug)
        {
            model.AddDiagnostic(DiagnosticLevel.Info, "Pipeline",
                $"Scenario '{context.ActiveScenario.DisplayName}' benefits from call graph. " +
                "Re-run with '--profile debug' to enable call graph.");
        }

        // ── GraphAssembly (PLAN-10 Part A) — JOIN detections + types into the connected CodeGraph + Map.
        // Analyze-time, scoped to one solution (R1). The Trace is a render-time lens over snapshot.Graph
        // (PLAN-10 A3/Part C). Runs before scoring/compression; uses stable type ids, not mutated bodies.
        var scope = SolutionScope.FromModel(model);
        var graphResolver = new SyntacticSymbolResolver();
        var noiseFilter = new NoiseFilter(new ProjectClassifier(model.Projects));
        var (codeGraph, entryPoints) = new GraphBuilder(graphResolver, noiseFilter).Build(model, scope);
        var mapModel = MapBuilder.Build(model, codeGraph, entryPoints);
        model.AddDiagnostic(DiagnosticLevel.Info, "GraphAssembly",
            $"graph: {codeGraph.NodeCount} nodes, {codeGraph.EdgeCount} edges, {entryPoints.Length} entry points"
            + (scope.SolutionName is { } sln ? $" (scope: {sln})" : ""));

        await RunScoringAsync(context, model, ct).ConfigureAwait(false);
        await RunCompressionAsync(context, model, ct).ConfigureAwait(false);

        context.Observer.OnPipelineCompleted(model);

        if (collector is not null)
        {
            var csharpFiles = context.Analysis.AllSourceFiles?.Count ?? 0;
            collector.SetCorpusFileCounts(0, csharpFiles);

            if (context.Cache is AnalysisCache ac)
                collector.SetCacheStats(ac.GetStats());
        }

        return new AnalysisSnapshot
        {
            Model = model,
            Analysis = context.Analysis,
            Scenario = context.ActiveScenario,
            Options = context.Options,
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
        await RunStageAsync(ExecutionStage.Stage1Sequential, PipelineStage.DiscoveryAndCacheWarmup, false, context, dryRunModel, ct).ConfigureAwait(false);

        var planContent = BuildDryRunPlanMarkdown(context);

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
            DryRunContent = planContent,
        };
    }

    private string BuildDryRunPlanMarkdown(DiscoveryContext context)
    {
        var (stage1, stage2, stage3) = BuildStageLists(context);

        var sb = new StringBuilder();
        sb.AppendLine("## Dry Run Plan");
        sb.AppendLine($"**Root**: {context.RootPath}");
        sb.AppendLine($"**Scenario**: {context.ActiveScenario.DisplayName}");
        sb.AppendLine($"**Profile**: {context.Options.Profile}");
        sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"**Max tokens**: {context.Options.MaxOutputTokens}");
        sb.AppendLine();

        AppendStageTable(sb, "Stage 1 (sequential)", stage1, (item, _) => $"| {(item.WillRun ? "✓" : "✗")} | {item.Name} | {item.Description} |");
        sb.AppendLine();
        AppendStageTable(sb, "Stage 2 (parallel)", stage2, (item, _) => $"| {(item.WillRun ? "✓" : "✗")} | {item.Name} | {item.Description} |");
        sb.AppendLine();
        sb.AppendLine("### Stage 3 (conditional, after signal detection)");
        sb.AppendLine("| Status | Name | Requires | Description |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var item in stage3)
        {
            var requires = item.RequiredSignals.Length > 0
                ? string.Join(" OR ", item.RequiredSignals)
                : "(always runs)";
            sb.AppendLine($"| ? | {item.Name} | {requires} | {item.Description} |");
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

        return sb.ToString();
    }

    private (List<StageItem> Stage1, List<StageItem> Stage2, List<Stage3Item> Stage3) BuildStageLists(DiscoveryContext context)
    {
        var stage1 = new List<StageItem>();
        var stage2 = new List<StageItem>();
        var stage3 = new List<Stage3Item>();

        foreach (var ext in _extractors.OrderBy(GetOrder))
        {
            var willRun = !context.Options.ExcludeExtractors.Contains(ext.Name, StringComparer.Ordinal)
                          && ext.ShouldRun(context, new DiscoveryModel());

            switch (ext.Stage)
            {
                case ExecutionStage.Stage1Sequential:
                    stage1.Add(new(ext.Name, ext.Capabilities.Description, willRun));
                    break;
                case ExecutionStage.Stage2Parallel:
                    stage2.Add(new(ext.Name, ext.Capabilities.Description, willRun));
                    break;
                case ExecutionStage.Stage3Specific:
                    stage3.Add(new(ext.Name, ext.Capabilities.Description, ext.Capabilities.ReadsSignals));
                    break;
            }
        }

        return (stage1, stage2, stage3);
    }

    private static void AppendStageTable<T>(StringBuilder sb, string title, List<T> items, Func<T, int, string> rowSelector)
    {
        sb.AppendLine($"### {title}");
        sb.AppendLine("| Status | Name | Description |");
        sb.AppendLine("|---|---|---|");
        for (var i = 0; i < items.Count; i++)
            sb.AppendLine(rowSelector(items[i], i));
    }

    private readonly record struct StageItem(string Name, string Description, bool WillRun);
    private readonly record struct Stage3Item(string Name, string Description, ImmutableArray<string> RequiredSignals);

    /// <summary>Renders from a snapshot according to the request lens. Cheap and repeatable.</summary>
    public async Task<RenderedContext> RenderAsync(AnalysisSnapshot snapshot, RenderRequest request, CancellationToken ct = default)
    {
        // ── PLAN-10 A3: Map/Trace branch — when the Graph is available with content (always after a
        // full analyze). The Map/Trace renderers produce the human-facing markdown narrative; JSON/HTML
        // consumers (programmatic callers, the eval harness) get the structured model from the legacy
        // renderers below, so --format json stays valid, parseable structured data.
        var format = string.IsNullOrEmpty(request.Format) ? "markdown" : request.Format;
        var wantsNarrative = format is "markdown" or "md";
        if (wantsNarrative && snapshot.Graph is { NodeCount: > 0 } graph)
        {
            if (!string.IsNullOrEmpty(request.Entry))
            {
                var entry = snapshot.Entries.FirstOrDefault(e =>
                    string.Equals(e.Title, request.Entry, StringComparison.OrdinalIgnoreCase))
                    ?? ResolveEntryFromNode(graph, request.Entry);
                if (entry is not null)
                {
                    var trace = new TraceBuilder(graph).Build(entry, new Graph.TraceOptions
                    {
                        MaxDepth = request.Depth ?? 6,
                        MaxFanOut = 12,
                    });
                    var traceContent = TraceRenderer.Render(trace, request.Detail)
                        + GraphDiagnosticsTail(snapshot, request);
                    return new RenderedContext(traceContent, traceContent.Length / 4, [], TimeSpan.Zero, "2.0");
                }
            }

            // No entry chosen — render the Map.
            if (snapshot.Map is { } mapModel)
            {
                var mapCtx = new MapRenderContext(mapModel, snapshot, format, request);
                var map = await MapRenderer.RenderAsync(mapCtx, ct).ConfigureAwait(false);
                var tail = GraphDiagnosticsTail(snapshot, request);
                return tail.Length == 0 ? map : map with { Content = map.Content + tail };
            }
        }

        var plan = RenderPlanBuilder.Build(snapshot, request);

        var opts = new RenderOptions(
            request.IncludeProvenance,
            request.IncludeDiagnostics,
            plan.EstimatedTokens,
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

        var rendered = await renderer.RenderAsync(snapshot.Model, opts, ct).ConfigureAwait(false);
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

    /// <summary>Resolves a free-text focus (a type or handler name) to a graph node so a Trace can start
    /// from it, not just from a catalogued HTTP entry. Lets <c>--focus OrdersController</c> /
    /// <c>--focus CreateOrderCommandHandler</c> produce a trace instead of silently falling back to the Map.
    /// Prefers behaviour-bearing nodes (Type/Handler/Service) and matches by short name.</summary>
    private static Graph.EntryPoint? ResolveEntryFromNode(Graph.CodeGraph graph, string focus)
    {
        var name = focus.Trim();
        // Type:Method narrows to the type; the trace walks the type's out-edges either way.
        var colon = name.IndexOf(':');
        if (colon > 0) name = name[..colon];

        Graph.GraphNode? best = null;
        foreach (var node in graph.Nodes)
        {
            if (node.Kind is not (Graph.NodeKind.Type or Graph.NodeKind.Handler
                or Graph.NodeKind.Service or Graph.NodeKind.EntryPoint)) continue;
            if (!string.Equals(node.Title, name, StringComparison.OrdinalIgnoreCase)
                && !node.Id.Key.EndsWith("." + name, StringComparison.OrdinalIgnoreCase)) continue;
            // Prefer a node that actually has somewhere to go.
            if (best is null || graph.OutEdges(node.Id).Length > graph.OutEdges(best.Id).Length)
                best = node;
        }

        return best is null
            ? null
            : new Graph.EntryPoint(Graph.EntryPointKind.PublicApi, best.Title, best.Id)
            {
                Provenance = best.FilePath,
            };
    }

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
        return "\n\nDIAGNOSTICS\n" + string.Join('\n', lines) + "\n";
    }

    /// <summary>Convenience: runs the full pipeline (analyze + render) and returns the rendered context.</summary>
    public async Task<RenderedContext> RunAsync(DiscoveryContext context, CancellationToken ct = default)
    {
        var snapshot = await AnalyzeAsync(context, ct).ConfigureAwait(false);
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
        return await RenderAsync(snapshot, request, ct).ConfigureAwait(false);
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

        // Single pass partitions extractors into three buckets, calling ShouldRun at most once
        var excludedByName = new List<IDiscoveryExtractor>();
        var excludedBySignal = new List<IDiscoveryExtractor>();
        var eligible = new List<IDiscoveryExtractor>();

        foreach (var ext in stageExtractors)
        {
            if (ctx.Options.ExcludeExtractors.Contains(ext.Name, StringComparer.Ordinal))
            {
                excludedByName.Add(ext);
            }
            else if (!ext.ShouldRun(ctx, model))
            {
                excludedBySignal.Add(ext);
            }
            else
            {
                eligible.Add(ext);
            }
        }

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
            await Parallel.ForEachAsync(eligible, ct, async (extractor, innerCt) =>
            {
                var (typesAdded, detsAdded, elapsed) = await RunSingleExtractorAsync(ctx, model, extractor, innerCt).ConfigureAwait(false);
                ctx.Observer.OnExtractorCompleted(extractor.Name, elapsed, false, null, typesAdded, detsAdded);
                RecordMetrics(ctx.Observer, extractor.Name, extractor.Tier, extractor.Category, elapsed, false, typesAdded, detsAdded);
            }).ConfigureAwait(false);
        }
        else
        {
            foreach (var extractor in eligible)
            {
                ct.ThrowIfCancellationRequested();
                var (typesAdded, detsAdded, elapsed) = await RunSingleExtractorAsync(ctx, model, extractor, ct).ConfigureAwait(false);
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
        try { await extractor.ExtractAsync(ctx, model, ct).ConfigureAwait(false); }
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
        ctx.Observer.OnStageStarted(PipelineStage.Scoring);
        var sw = Stopwatch.StartNew();

        foreach (var pruner in _pruners.OrderBy(p => p.Order))
        {
            ct.ThrowIfCancellationRequested();
            var before = model.Types.Values.Count(t => !t.IsPruned);
            await pruner.PruneAsync(ctx, model, ct).ConfigureAwait(false);
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
            var result = await strategy.CompressAsync(model, options, ct).ConfigureAwait(false);
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
            .Distinct(StringComparer.Ordinal)
            .Select(name => (Name: name, Distance: StringHelpers.LevenshteinDistance(input, name)))
            .Where(x => x.Distance <= maxDistance && x.Distance > 0)
            .OrderBy(x => x.Distance)
            .ThenBy(x => x.Name, StringComparer.Ordinal)
            .Select(x => x.Name)
            .Take(3)
            .ToList();
    }
}
