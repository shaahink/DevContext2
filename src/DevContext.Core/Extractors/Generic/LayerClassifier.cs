namespace DevContext.Core.Extractors.Generic;

/// <summary>Classifies each project into an <see cref="ArchitectureLayer"/> using path heuristics and package signals.</summary>
[ExtractorOrder(15)]
public sealed class LayerClassifier : IDiscoveryExtractor
{
    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "LayerClassifier";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Generic;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage2Parallel;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [], [],
        ["context.Analysis.ProjectLayerMap"],
        "Classifies each project into an ArchitectureLayer using path heuristics and package references (not signals)");
    /// <summary>Determines whether this extractor should run.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;
    /// <summary>Classifies each project's layer based on path patterns and package references.</summary>
    public ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var map = new Dictionary<string, ArchitectureLayer>(StringComparer.OrdinalIgnoreCase);

        foreach (var project in model.Projects)
        {
            ct.ThrowIfCancellationRequested();

            var layer = ClassifyByPath(project.FilePath);

            if (layer == ArchitectureLayer.Unknown)
                layer = ClassifyBySignals(project);

            map[project.Name] = layer;
        }

        context.Analysis.ProjectLayerMap = map.ToFrozenDictionary();
        return default;
    }

    private static ArchitectureLayer ClassifyByPath(string projectPath)
    {
        var lower = projectPath.ToLowerInvariant();

        if (lower.Contains("\\domain\\") || lower.Contains("/domain/") || lower.Contains(".domain"))
            return ArchitectureLayer.Domain;
        if (lower.Contains("\\application\\") || lower.Contains("/application/") || lower.Contains(".application"))
            return ArchitectureLayer.Application;
        if (lower.Contains("\\infrastructure\\") || lower.Contains("/infrastructure/") || lower.Contains(".infrastructure"))
            return ArchitectureLayer.Infrastructure;
        if (lower.Contains("\\persistence\\") || lower.Contains("/persistence/") || lower.Contains(".persistence")
            || lower.Contains("\\data\\") || lower.Contains("/data/") || lower.Contains(".data"))
            return ArchitectureLayer.Persistence;
        if (lower.Contains("\\api\\") || lower.Contains("/api/") || lower.Contains(".api")
            || lower.Contains("\\web\\") || lower.Contains("/web/") || lower.Contains(".web"))
            return ArchitectureLayer.Api;
        if (lower.Contains("\\presentation\\") || lower.Contains("/presentation/") || lower.Contains(".presentation"))
            return ArchitectureLayer.Presentation;
        if (lower.Contains("\\tests\\") || lower.Contains("/tests/") || lower.Contains(".tests")
            || lower.Contains("\\test\\") || lower.Contains("/test/") || lower.Contains(".test"))
            return ArchitectureLayer.Testing;
        if (lower.Contains("\\shared\\") || lower.Contains("/shared/") || lower.Contains(".shared")
            || lower.Contains("\\core\\") || lower.Contains("/core/") || lower.Contains(".core"))
            return ArchitectureLayer.Shared;

        return ArchitectureLayer.Unknown;
    }

    private static ArchitectureLayer ClassifyBySignals(ProjectInfo project)
    {
        var hasMediatR = project.PackageReferences.Any(p =>
            p.Name.Equals("MediatR", StringComparison.OrdinalIgnoreCase));

        if (hasMediatR)
            return ArchitectureLayer.Application;

        var hasEfCore = project.PackageReferences.Any(p =>
            p.Name.Contains("EntityFrameworkCore", StringComparison.OrdinalIgnoreCase));

        if (hasEfCore)
            return ArchitectureLayer.Persistence;

        return ArchitectureLayer.Unknown;
    }
}
