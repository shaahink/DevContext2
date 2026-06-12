using System.Diagnostics;
using System.Reflection;
using System.Text;

using DevContext.Core.Extractors.Generic;
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
        if (context.Options.Profile == ExtractionProfile.Debug && _validationWarnings.Count > 0)
            _logger.LogWarning("Strict mode: {Count} validation warning(s) found. Continuing with Debug profile.", _validationWarnings.Count);

        var model = new DiscoveryModel { Budget = new TokenBudget { MaxTokens = context.Options.MaxOutputTokens } };
        context.Observer.OnPipelineStarted(context);

        // Stage 1: Sequential discovery + cache warmup
        await RunStageAsync(ExecutionStage.Stage1Sequential, PipelineStage.DiscoveryAndCacheWarmup, false, context, model, ct);

        // Stage 2: Parallel Generic extractors
        await RunStageAsync(ExecutionStage.Stage2Parallel, PipelineStage.GenericExtraction, true, context, model, ct);

        // Resolve Type/Method focus points
        var unresolvedCount = context.Analysis.UnresolvedFocusPoints.Count;
        if (unresolvedCount > 0)
        {
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

        // Seal signals
        context.Observer.OnStageStarted(PipelineStage.SignalSealing);
        var sealSw = Stopwatch.StartNew();
        model.Architecture.Seal();
        context.Observer.OnSignalsSealed(model.Architecture.All);

        ApplyArchitectureStyle(model);
        context.Observer.OnStageCompleted(PipelineStage.SignalSealing, sealSw.Elapsed);

        // Stage 3: Parallel Specific extractors (signal-gated, mutually independent)
        await RunStageAsync(ExecutionStage.Stage3Sequential, PipelineStage.SpecificExtraction, true, context, model, ct);

        // Profile-scenario mismatch warning
        if (context.ActiveScenario.Name is "deep-dive"
            && context.Options.Profile < ExtractionProfile.Debug)
        {
            model.AddDiagnostic(DiagnosticLevel.Info, "Pipeline",
                $"Scenario '{context.ActiveScenario.DisplayName}' benefits from call graph. " +
                "Re-run with '--profile debug' to enable call graph.");
        }

        // Stage 4: Sequential scoring
        await RunScoringAsync(context, model, ct);

        // Stage 5: Sequential compression
        await RunCompressionAsync(context, model, ct);

        context.Observer.OnPipelineCompleted(model);

        return new AnalysisSnapshot
        {
            Model = model,
            Analysis = context.Analysis,
            Scenario = context.ActiveScenario,
            Options = context.Options,
        };
    }

    /// <summary>Renders from a snapshot according to the request lens. Cheap and repeatable.</summary>
    public async Task<RenderedContext> RenderAsync(AnalysisSnapshot snapshot, RenderRequest request, CancellationToken ct = default)
    {
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
        };

        if (!_renderers.TryGetValue(request.Format, out var renderer))
            throw new InvalidOperationException($"No renderer registered for format: {request.Format}");

        var rendered = await renderer.RenderAsync(snapshot.Model, opts, ct);
        rendered = RunSelfChecks(rendered, snapshot.Model, opts, snapshot.Model);
        return rendered;
    }

    /// <summary>Runs the full discovery pipeline and returns the rendered context.</summary>
    public async Task<RenderedContext> RunAsync(DiscoveryContext context, CancellationToken ct = default)
    {
        if (context.Options.Profile == ExtractionProfile.Debug && _validationWarnings.Count > 0)
            _logger.LogWarning("Strict mode: {Count} validation warning(s) found. Continuing with Debug profile.", _validationWarnings.Count);

        if (context.Options.DryRun)
            return await RunDryRunAsync(context, ct);

        var model = new DiscoveryModel { Budget = new TokenBudget { MaxTokens = context.Options.MaxOutputTokens } };
        context.Observer.OnPipelineStarted(context);

        // Stage 1: Sequential discovery + cache warmup (FileTree, Solution, ProjectStructure)
        await RunStageAsync(ExecutionStage.Stage1Sequential, PipelineStage.DiscoveryAndCacheWarmup, false, context, model, ct);

        // Stage 2: Parallel Generic extractors (Dependency, SyntaxStructure, LayerClassifier)
        await RunStageAsync(ExecutionStage.Stage2Parallel, PipelineStage.GenericExtraction, true, context, model, ct);

        // Resolve Type/Method focus points now that model.Types is populated
        var unresolvedCount = context.Analysis.UnresolvedFocusPoints.Count;
        if (unresolvedCount > 0)
        {
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

        // Seal signals — no more signal writes after this point
        context.Observer.OnStageStarted(PipelineStage.SignalSealing);
        var sealSw = Stopwatch.StartNew();
        model.Architecture.Seal();
        context.Observer.OnSignalsSealed(model.Architecture.All);

        // Architecture style detection: runs after signals sealed (between Stage 2 and 3 per design)
        ApplyArchitectureStyle(model);
        context.Observer.OnStageCompleted(PipelineStage.SignalSealing, sealSw.Elapsed);

        // Stage 3: Parallel Specific extractors (signal-gated, mutually independent)
        await RunStageAsync(ExecutionStage.Stage3Sequential, PipelineStage.SpecificExtraction, true, context, model, ct);

        // Profile-scenario mismatch warning
        if (context.ActiveScenario.Name is "deep-dive"
            && context.Options.Profile < ExtractionProfile.Debug)
        {
            model.AddDiagnostic(DiagnosticLevel.Info, "Pipeline",
                $"Scenario '{context.ActiveScenario.DisplayName}' benefits from call graph. " +
                "Re-run with '--profile debug' to enable call graph.");
        }

        // Stage 4: Sequential scoring
        await RunScoringAsync(context, model, ct);

        // Stage 5: Sequential compression
        await RunCompressionAsync(context, model, ct);

        // Stage 6: Render
        context.Observer.OnStageStarted(PipelineStage.Rendering);
        var renderSw = Stopwatch.StartNew();
        var format = context.Options.OutputFormat.ToString().ToLowerInvariant();
        if (!_renderers.TryGetValue(format, out var renderer))
            throw new InvalidOperationException($"No renderer registered for format: {format}");

        var renderOptions = new RenderOptions(
            context.Options.IncludeProvenance,
            context.Options.IncludeDiagnostics,
            model.Budget.MaxTokens,
            context.ActiveScenario.DisplayName,
            ProfileDisplayName: context.Options.Profile.ToString().ToLowerInvariant(),
            context.ActiveScenario.RequiredSections,
            context.Analysis.FocusPoints.ToImmutableArray(),
            context.Analysis.CallGraph,
            context.Analysis.ProjectGraph,
            TokenView: context.Options.TokenView);

        var rendered = await renderer.RenderAsync(model, renderOptions, ct);

        // Self-check invariants after rendering
        rendered = RunSelfChecks(rendered, model, renderOptions, model);

        context.Observer.OnStageCompleted(PipelineStage.Rendering, renderSw.Elapsed);
        context.Observer.OnRenderCompleted(rendered);
        context.Observer.OnPipelineCompleted(model);
        return rendered;
    }

    /// <summary>Runs the pipeline once and renders all registered formats.</summary>
    public async Task<IReadOnlyDictionary<string, RenderedContext>> RunAllFormatsAsync(DiscoveryContext context, CancellationToken ct)
    {
        var model = new DiscoveryModel();
        var budgets = new Dictionary<string, TokenBudget>();
        ct.ThrowIfCancellationRequested();

        // Stages 1-5 (identical for all formats)
        await RunStageAsync(ExecutionStage.Stage1Sequential, PipelineStage.DiscoveryAndCacheWarmup, false, context, model, ct);
        await RunStageAsync(ExecutionStage.Stage2Parallel, PipelineStage.GenericExtraction, false, context, model, ct);

        // Focus point resolution
        var unresolvedCount = context.Analysis.UnresolvedFocusPoints.Count;
        if (unresolvedCount > 0)
        {
            var resolved = FocusPointResolver.Resolve(context.Analysis.UnresolvedFocusPoints, model);
            var failedToResolve = resolved
                .Where((fp, i) => fp.Kind is FocusKind.Type or FocusKind.Method && string.IsNullOrEmpty(fp.FilePath))
                .ToList();
            foreach (var failed in failedToResolve)
            {
                var didYouMean = SuggestTypeNames(failed.TypeName, model.Types.Values);
                var suggestion = didYouMean.Count > 0 ? $" Did you mean: {string.Join(", ", didYouMean.Take(3))}?" : "";
                model.AddDiagnostic(DiagnosticLevel.Warning, "FocusPointResolver",
                    $"--around {failed.TypeName}: type not found. {suggestion}");
            }
            context.Analysis.FocusPoints = resolved;
        }

        // Signal sealing + architecture detection
        model.Architecture.Seal();
        ApplyArchitectureStyle(model);
        var sealSw = Stopwatch.StartNew();
        context.Observer.OnStageCompleted(PipelineStage.SignalSealing, sealSw.Elapsed);
        await RunStageAsync(ExecutionStage.Stage3Sequential, PipelineStage.SpecificExtraction, true, context, model, ct);
        await RunScoringAsync(context, model, ct);
        await RunCompressionAsync(context, model, ct);

        // Stage 6: Render ALL formats from the same model
        context.Observer.OnStageStarted(PipelineStage.Rendering);
        var renderSw = Stopwatch.StartNew();
        var results = new Dictionary<string, RenderedContext>();

        var renderOptions = new RenderOptions(
            context.Options.IncludeProvenance,
            context.Options.IncludeDiagnostics,
            model.Budget.MaxTokens,
            context.ActiveScenario.DisplayName,
            ProfileDisplayName: context.Options.Profile.ToString().ToLowerInvariant(),
            context.ActiveScenario.RequiredSections,
            context.Analysis.FocusPoints.ToImmutableArray(),
            context.Analysis.CallGraph,
            context.Analysis.ProjectGraph,
            TokenView: context.Options.TokenView);

        foreach (var (format, renderer) in _renderers)
        {
            var rendered = await renderer.RenderAsync(model, renderOptions, ct);
            rendered = RunSelfChecks(rendered, model, renderOptions, model);
            results[format] = rendered;
        }

        context.Observer.OnStageCompleted(PipelineStage.Rendering, renderSw.Elapsed);
        context.Observer.OnPipelineCompleted(model);
        return results;
    }

    private async Task<RenderedContext> RunDryRunAsync(DiscoveryContext context, CancellationToken ct)
    {
        await RunStageAsync(ExecutionStage.Stage1Sequential, PipelineStage.DiscoveryAndCacheWarmup, false, context, new DiscoveryModel(), ct);

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
                case ExecutionStage.Stage3Sequential:
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

        var rendered = new RenderedContext(sb.ToString(), 0, [], TimeSpan.Zero, "2.0");
        context.Observer.OnPipelineCompleted(new DiscoveryModel());
        return rendered;
    }

    /// <summary>Runs a stage of the pipeline: filters extractors by ExecutionStage, executes them sequentially or in parallel.</summary>
    private async Task RunStageAsync(ExecutionStage execStage, PipelineStage stage, bool parallel,
        DiscoveryContext ctx, DiscoveryModel model, CancellationToken ct)
    {
        ctx.Observer.OnStageStarted(stage);
        var sw = Stopwatch.StartNew();

        var eligible = _extractors
            .Where(e => e.Stage == execStage)
            .Where(e => !ctx.Options.ExcludeExtractors.Contains(e.Name))
            .Where(e => e.ShouldRun(ctx, model))
            .OrderBy(GetOrder)
            .ToList();

        if (parallel)
        {
            await Parallel.ForEachAsync(eligible, ct, async (extractor, innerCt) =>
            {
                var (typesAdded, detsAdded, elapsed) = await RunSingleExtractorAsync(ctx, model, extractor, innerCt);
                ctx.Observer.OnExtractorCompleted(extractor.Name, elapsed, false, null, typesAdded, detsAdded);
                RecordMetrics(ctx.Observer, extractor.Name, extractor.Tier, extractor.Category, elapsed, false, typesAdded, detsAdded);
            });
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
        model.StyleDetectedVia = "ArchitectureStyleDetector";
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

        // Compute FocusScore + RoleScore → FinalScore with scenario-owned weights
        var hasFocus = ctx.Analysis.FocusPoints.Count > 0;
        foreach (var type in model.Types.Values)
        {
            if (hasFocus)
                type.FinalScore = ctx.ActiveScenario.Pruning.RoleWeight * type.RoleScore
                                  + ctx.ActiveScenario.Pruning.FocusWeight * type.FocusScore;
            else
                type.FinalScore = type.RoleScore;
        }

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
}
