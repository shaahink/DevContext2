using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using DevContext.Core.Extractors.Generic;

namespace DevContext.Core.Pipeline;

public sealed class DiscoveryPipeline
{
    private readonly IReadOnlyList<IDiscoveryExtractor> _extractors;
    private readonly IReadOnlyList<IPruner> _pruners;
    private readonly IReadOnlyList<ICompressionStrategy> _compressionStrategies;
    private readonly IReadOnlyDictionary<string, IContextRenderer> _renderers;
    private readonly ILogger<DiscoveryPipeline> _logger;

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
    }

    public async Task<RenderedContext> RunAsync(DiscoveryContext context, CancellationToken ct = default)
    {
        if (context.Options.DryRun)
            return await RunDryRunAsync(context, ct);

        var model = new DiscoveryModel { Budget = new TokenBudget { MaxTokens = context.Options.MaxOutputTokens } };
        context.Observer.OnPipelineStarted(context);

        await RunStage1Async(context, model, ct);
        await RunStage2Async(context, model, ct);

        model.Architecture.Seal();
        model.DetectedStyle = DetermineStyle(model);
        context.Observer.OnSignalsSealed(model.Architecture.All);

        await RunStage3Async(context, model, ct);
        await RunPruningAsync(context, model, ct);
        await RunCompressionAsync(context, model, ct);

        var format = context.Options.OutputFormat.ToString().ToLowerInvariant();
        if (!_renderers.TryGetValue(format, out var renderer))
            throw new InvalidOperationException($"No renderer registered for format: {format}");

        var renderOptions = new RenderOptions(
            context.Options.IncludeProvenance,
            context.Options.IncludeDiagnostics,
            model.Budget.MaxTokens);

        var rendered = await renderer.RenderAsync(model, renderOptions, ct);
        context.Observer.OnRenderCompleted(rendered);
        context.Observer.OnPipelineCompleted(model);
        return rendered;
    }

    private async Task<RenderedContext> RunDryRunAsync(DiscoveryContext context, CancellationToken ct)
    {
        var model = new DiscoveryModel();
        await RunStage1Async(context, model, ct);

        var plan = new List<(string Name, ExtractorTier Tier, ExtractorCategory Cat, bool WillRun)>();
        foreach (var ext in _extractors.OrderBy(GetOrder))
        {
            var willRun = !context.Options.ExcludeExtractors.Contains(ext.Name)
                          && ext.ShouldRun(context, model);
            plan.Add((ext.Name, ext.Tier, ext.Category, willRun));
        }

        var sb = new StringBuilder();
        sb.AppendLine("## Dry Run Plan");
        sb.AppendLine($"**Root**: {context.RootPath}");
        sb.AppendLine($"**Scenario**: {context.ActiveScenario.DisplayName}");
        sb.AppendLine();
        sb.AppendLine("### Extractors");
        sb.AppendLine("| Status | Name | Tier | Category |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var (name, tier, cat, willRun) in plan)
        {
            var status = willRun ? "✓" : "✗";
            sb.AppendLine($"| {status} | {name} | {tier} | {cat} |");
        }

        var rendered = new RenderedContext(sb.ToString(), 0, [], TimeSpan.Zero, "2.0");
        context.Observer.OnPipelineCompleted(new DiscoveryModel());
        return rendered;
    }

    private async Task RunStage1Async(DiscoveryContext ctx, DiscoveryModel model, CancellationToken ct)
    {
        ctx.Observer.OnStageStarted(PipelineStage.DiscoveryAndCacheWarmup);
        var sw = Stopwatch.StartNew();

        var stage1Extractors = _extractors
            .Where(e => e.Category == ExtractorCategory.Generic)
            .Where(e => !ctx.Options.ExcludeExtractors.Contains(e.Name))
            .Where(e => e.ShouldRun(ctx, model))
            .OrderBy(GetOrder)
            .Where(e => e is FileTreeExtractor or SolutionDiscoveryExtractor)
            .ToList();

        foreach (var extractor in stage1Extractors)
        {
            ct.ThrowIfCancellationRequested();
            ctx.Observer.OnExtractorStarted(extractor.Name, extractor.Tier);
            var esw = Stopwatch.StartNew();
            try
            {
                await extractor.ExtractAsync(ctx, model, ct);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, extractor.Name, ex.Message);
            }
            ctx.Observer.OnExtractorCompleted(extractor.Name, esw.Elapsed, false, null);
        }

        ctx.Observer.OnStageCompleted(PipelineStage.DiscoveryAndCacheWarmup, sw.Elapsed);
    }

    private async Task RunStage2Async(DiscoveryContext ctx, DiscoveryModel model, CancellationToken ct)
    {
        ctx.Observer.OnStageStarted(PipelineStage.GenericExtraction);
        var sw = Stopwatch.StartNew();

        var eligible = _extractors
            .Where(e => e.Tier == ExtractorTier.Fast && e.Category == ExtractorCategory.Generic)
            .Where(e => !ctx.Options.ExcludeExtractors.Contains(e.Name))
            .Where(e => e.ShouldRun(ctx, model))
            .OrderBy(GetOrder)
            .Where(e => e is not FileTreeExtractor and not SolutionDiscoveryExtractor)
            .ToList();

        await Parallel.ForEachAsync(eligible, ct, async (extractor, innerCt) =>
        {
            ctx.Observer.OnExtractorStarted(extractor.Name, extractor.Tier);
            var esw = Stopwatch.StartNew();
            try
            {
                await extractor.ExtractAsync(ctx, model, innerCt);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, extractor.Name, ex.Message);
            }
            ctx.Observer.OnExtractorCompleted(extractor.Name, esw.Elapsed, false, null);
        });

        ctx.Observer.OnStageCompleted(PipelineStage.GenericExtraction, sw.Elapsed);
    }

    private async Task RunStage3Async(DiscoveryContext ctx, DiscoveryModel model, CancellationToken ct)
    {
        ctx.Observer.OnStageStarted(PipelineStage.SpecificExtraction);
        var sw = Stopwatch.StartNew();

        var eligible = _extractors
            .Where(e => e.Category == ExtractorCategory.Specific)
            .Where(e => !ctx.Options.ExcludeExtractors.Contains(e.Name))
            .Where(e => e.ShouldRun(ctx, model))
            .OrderBy(GetOrder)
            .ToList();

        foreach (var extractor in eligible)
        {
            ct.ThrowIfCancellationRequested();
            ctx.Observer.OnExtractorStarted(extractor.Name, extractor.Tier);
            var esw = Stopwatch.StartNew();
            try
            {
                await extractor.ExtractAsync(ctx, model, ct);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, extractor.Name, ex.Message);
            }
            ctx.Observer.OnExtractorCompleted(extractor.Name, esw.Elapsed, false, null);
        }

        ctx.Observer.OnStageCompleted(PipelineStage.SpecificExtraction, sw.Elapsed);
    }

    private async Task RunPruningAsync(DiscoveryContext ctx, DiscoveryModel model, CancellationToken ct)
    {
        ctx.Observer.OnStageStarted(PipelineStage.Pruning);
        var sw = Stopwatch.StartNew();
        var before = model.Types.Count;

        foreach (var pruner in _pruners.OrderBy(p => p.Order))
        {
            ct.ThrowIfCancellationRequested();
            await pruner.PruneAsync(ctx, model, ct);
            var after = model.Types.Count(t => !t.Value.IsPruned);
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
            ctx.Observer.OnCompressionApplied(result);
        }

        ctx.Observer.OnStageCompleted(PipelineStage.Compression, sw.Elapsed);
    }

    private static ArchitectureStyle DetermineStyle(DiscoveryModel model)
    {
        var signals = model.Architecture.All;
        float maxConfidence = 0;
        var style = ArchitectureStyle.Unknown;

        if (signals.TryGetValue(ArchitectureSignals.Keys.MinimalApis, out var ma) && ma.Detected && ma.Confidence > maxConfidence)
        {
            maxConfidence = ma.Confidence;
            style = ArchitectureStyle.MinimalApi;
        }

        if (model.Projects.Length > 4 && signals.TryGetValue(ArchitectureSignals.Keys.MediatR, out var mr) && mr.Detected)
        {
            var combined = mr.Confidence * 1.2f;
            if (combined > maxConfidence)
            {
                maxConfidence = combined;
                style = ArchitectureStyle.CleanArchitecture;
            }
        }

        if (model.Projects.Length > 2 && signals.TryGetValue(ArchitectureSignals.Keys.EfCore, out var ef) && ef.Detected)
        {
            if (style == ArchitectureStyle.Unknown && ef.Confidence > 0.5f)
            {
                style = ArchitectureStyle.NLayer;
                maxConfidence = ef.Confidence;
            }
        }

        model.StyleConfidence = Math.Min(maxConfidence, 1.0f);
        return style;
    }

    internal static int GetOrder(IDiscoveryExtractor extractor)
    {
        var attr = extractor.GetType().GetCustomAttribute<ExtractorOrderAttribute>(false);
        return attr?.Order ?? 100;
    }
}
