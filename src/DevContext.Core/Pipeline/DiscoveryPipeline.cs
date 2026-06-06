using System.Diagnostics;
using System.Reflection;
using System.Text;
using DevContext.Core.Extractors.Generic;
using DevContext.Core.Observers;
using DevContext.Core.Resolvers;

namespace DevContext.Core.Pipeline;

/// <summary>Orchestrates the discovery pipeline: extraction, signal sealing, pruning, compression, and rendering.</summary>
public sealed class DiscoveryPipeline
{
    private readonly IReadOnlyList<IDiscoveryExtractor> _extractors;
    private readonly IReadOnlyList<IPruner> _pruners;
    private readonly IReadOnlyList<ICompressionStrategy> _compressionStrategies;
    private readonly IReadOnlyDictionary<string, IContextRenderer> _renderers;
    private readonly ArchitectureStyleDetector _styleDetector;
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
        _styleDetector = new ArchitectureStyleDetector();
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
        await RunStage1Async(context, model, ct);

        // Stage 2: Parallel Generic extractors (Dependency, SyntaxStructure, LayerClassifier)
        // Note: model.Projects is fully populated by Stage 1 before parallel Stage 2 begins
        await RunStage2Async(context, model, ct);

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

        // Stage 3: Sequential Specific extractors (signal-gated)
        await RunStage3Async(context, model, ct);

        // Stage 4: Sequential pruning
        await RunPruningAsync(context, model, ct);

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
            model.Budget.MaxTokens);

        var rendered = await renderer.RenderAsync(model, renderOptions, ct);
        context.Observer.OnStageCompleted(PipelineStage.Rendering, renderSw.Elapsed);
        context.Observer.OnRenderCompleted(rendered);
        context.Observer.OnPipelineCompleted(model);
        return rendered;
    }

    private async Task<RenderedContext> RunDryRunAsync(DiscoveryContext context, CancellationToken ct)
    {
        await RunStage1Async(context, new DiscoveryModel(), ct);

        var plan = new List<(string Name, ExtractorTier Tier, ExtractorCategory Cat, bool WillRun, string Description)>();
        foreach (var ext in _extractors.OrderBy(GetOrder))
        {
            var willRun = !context.Options.ExcludeExtractors.Contains(ext.Name)
                          && ext.ShouldRun(context, new DiscoveryModel());
            plan.Add((ext.Name, ext.Tier, ext.Category, willRun, ext.Capabilities.Description));
        }

        var sb = new StringBuilder();
        sb.AppendLine("## Dry Run Plan");
        sb.AppendLine($"**Root**: {context.RootPath}");
        sb.AppendLine($"**Scenario**: {context.ActiveScenario.DisplayName}");
        sb.AppendLine($"**Profile**: {context.Options.Profile}");
        sb.AppendLine($"**Max tokens**: {context.Options.MaxOutputTokens}");
        sb.AppendLine();
        sb.AppendLine("### Extractors");
        sb.AppendLine("| Status | Name | Tier | Category | Description |");
        sb.AppendLine("|---|---|---|---|---|");
        foreach (var (name, tier, cat, willRun, desc) in plan)
        {
            var status = willRun ? "✓" : "✗";
            sb.AppendLine($"| {status} | {name} | {tier} | {cat} | {desc} |");
        }

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

    /// <summary>
    /// Stage 1: Sequential — all extractors with ExecutionStage.Stage1Sequential.
    /// These populate the analysis context and cache, establishing the data that Stage 2 reads.
    /// Extractors declare their stage via the Stage property; the pipeline has no hardcoded names.
    /// </summary>
    private async Task RunStage1Async(DiscoveryContext ctx, DiscoveryModel model, CancellationToken ct)
    {
        ctx.Observer.OnStageStarted(PipelineStage.DiscoveryAndCacheWarmup);
        var sw = Stopwatch.StartNew();

        var stage1Extractors = _extractors
            .Where(e => e.Stage == ExecutionStage.Stage1Sequential)
            .Where(e => !ctx.Options.ExcludeExtractors.Contains(e.Name))
            .Where(e => e.ShouldRun(ctx, model))
            .OrderBy(GetOrder)
            .ToList();

        foreach (var extractor in stage1Extractors)
        {
            ct.ThrowIfCancellationRequested();
            ctx.Observer.OnExtractorStarted(extractor.Name, extractor.Tier);
            var esw = Stopwatch.StartNew();
            var typesBefore = model.Types.Count;
            var detsBefore = model.Detections.Count;
            try { await extractor.ExtractAsync(ctx, model, ct); }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { model.AddDiagnostic(DiagnosticLevel.Warning, extractor.Name, ex.Message); }
            var typesAdded = model.Types.Count - typesBefore;
            var detsAdded = model.Detections.Count - detsBefore;
            ctx.Observer.OnExtractorCompleted(extractor.Name, esw.Elapsed, false, null, typesAdded, detsAdded);
            if (ctx.Observer is MetricsDiscoveryObserver mdo)
                mdo.RecordExtractorMetrics(extractor.Name, extractor.Tier, extractor.Category, esw.Elapsed, false, typesAdded, detsAdded);
        }

        ctx.Observer.OnStageCompleted(PipelineStage.DiscoveryAndCacheWarmup, sw.Elapsed);
    }

    /// <summary>
    /// Stage 2: Parallel — all extractors with ExecutionStage.Stage2Parallel.
    /// model.Projects and analysis context are fully populated by Stage 1 before this runs.
    /// </summary>
    private async Task RunStage2Async(DiscoveryContext ctx, DiscoveryModel model, CancellationToken ct)
    {
        ctx.Observer.OnStageStarted(PipelineStage.GenericExtraction);
        var sw = Stopwatch.StartNew();

        var eligible = _extractors
            .Where(e => e.Stage == ExecutionStage.Stage2Parallel)
            .Where(e => !ctx.Options.ExcludeExtractors.Contains(e.Name))
            .Where(e => e.ShouldRun(ctx, model))
            .OrderBy(GetOrder)
            .ToList();

        await Parallel.ForEachAsync(eligible, ct, async (extractor, innerCt) =>
        {
            ctx.Observer.OnExtractorStarted(extractor.Name, extractor.Tier);
            var esw = Stopwatch.StartNew();
            var typesBefore = model.Types.Count;
            var detsBefore = model.Detections.Count;
            try { await extractor.ExtractAsync(ctx, model, innerCt); }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { model.AddDiagnostic(DiagnosticLevel.Warning, extractor.Name, ex.Message); }
            var typesAdded = model.Types.Count - typesBefore;
            var detsAdded = model.Detections.Count - detsBefore;
            ctx.Observer.OnExtractorCompleted(extractor.Name, esw.Elapsed, false, null, typesAdded, detsAdded);
            if (ctx.Observer is MetricsDiscoveryObserver mdo)
                mdo.RecordExtractorMetrics(extractor.Name, extractor.Tier, extractor.Category, esw.Elapsed, false, typesAdded, detsAdded);
        });

        ctx.Observer.OnStageCompleted(PipelineStage.GenericExtraction, sw.Elapsed);
    }

    /// <summary>
    /// Architecture style detection runs after signals are sealed (between Stage 2 and 3 per design).
    /// </summary>
    private static void ApplyArchitectureStyle(DiscoveryModel model)
    {
        var detector = new ArchitectureStyleDetector();
        var (style, confidence, via) = detector.Detect(model);
        model.DetectedStyle = style;
        model.StyleConfidence = confidence;
        model.StyleDetectedVia = "ArchitectureStyleDetector";
    }

    /// <summary>
    /// Stage 3: Sequential — all extractors with ExecutionStage.Stage3Sequential (signal-gated Specific + Deep).
    /// </summary>
    private async Task RunStage3Async(DiscoveryContext ctx, DiscoveryModel model, CancellationToken ct)
    {
        ctx.Observer.OnStageStarted(PipelineStage.SpecificExtraction);
        var sw = Stopwatch.StartNew();

        var eligible = _extractors
            .Where(e => e.Stage == ExecutionStage.Stage3Sequential)
            .Where(e => !ctx.Options.ExcludeExtractors.Contains(e.Name))
            .Where(e => e.ShouldRun(ctx, model))
            .OrderBy(GetOrder)
            .ToList();

        foreach (var extractor in eligible)
        {
            ct.ThrowIfCancellationRequested();
            ctx.Observer.OnExtractorStarted(extractor.Name, extractor.Tier);
            var esw = Stopwatch.StartNew();
            var typesBefore = model.Types.Count;
            var detsBefore = model.Detections.Count;
            try { await extractor.ExtractAsync(ctx, model, ct); }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { model.AddDiagnostic(DiagnosticLevel.Warning, extractor.Name, ex.Message); }
            var typesAdded = model.Types.Count - typesBefore;
            var detsAdded = model.Detections.Count - detsBefore;
            ctx.Observer.OnExtractorCompleted(extractor.Name, esw.Elapsed, false, null, typesAdded, detsAdded);
            if (ctx.Observer is MetricsDiscoveryObserver mdo)
                mdo.RecordExtractorMetrics(extractor.Name, extractor.Tier, extractor.Category, esw.Elapsed, false, typesAdded, detsAdded);
        }

        ctx.Observer.OnStageCompleted(PipelineStage.SpecificExtraction, sw.Elapsed);
    }

    private async Task RunPruningAsync(DiscoveryContext ctx, DiscoveryModel model, CancellationToken ct)
    {
        ctx.Observer.OnStageStarted(PipelineStage.Pruning);
        var sw = Stopwatch.StartNew();

        foreach (var pruner in _pruners.OrderBy(p => p.Order))
        {
            ct.ThrowIfCancellationRequested();
            var before = model.Types.Values.Count(t => !t.IsPruned);
            await pruner.PruneAsync(ctx, model, ct);
            var after = model.Types.Values.Count(t => !t.IsPruned);
            ctx.Observer.OnPrunerCompleted(pruner.Name, before, after);
        }

        ctx.Observer.OnStageCompleted(PipelineStage.Pruning, sw.Elapsed);
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

    private static List<string> SuggestTypeNames(string? input, IEnumerable<TypeDiscovery> types, int maxDistance = 3)
    {
        if (string.IsNullOrEmpty(input)) return [];

        return types
            .Select(t => t.Name)
            .Distinct()
            .Select(name => (Name: name, Distance: LevenshteinDistance(input, name)))
            .Where(x => x.Distance <= maxDistance && x.Distance > 0)
            .OrderBy(x => x.Distance)
            .ThenBy(x => x.Name)
            .Select(x => x.Name)
            .Take(3)
            .ToList();
    }

    private static int LevenshteinDistance(string a, string b)
    {
        var lenA = a.Length;
        var lenB = b.Length;
        var d = new int[lenA + 1, lenB + 1];

        for (var i = 0; i <= lenA; i++) d[i, 0] = i;
        for (var j = 0; j <= lenB; j++) d[0, j] = j;

        for (var i = 1; i <= lenA; i++)
        {
            for (var j = 1; j <= lenB; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }

        return d[lenA, lenB];
    }
}
