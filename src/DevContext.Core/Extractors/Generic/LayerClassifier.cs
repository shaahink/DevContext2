namespace DevContext.Core.Extractors.Generic;

[ExtractorOrder(15)]
public sealed class LayerClassifier : IDiscoveryExtractor
{
    public string Name => "LayerClassifier";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Generic;
    public ExecutionStage Stage => ExecutionStage.Stage2Parallel;

    public ExtractorCapabilities Capabilities => new(
        [], [],
        ["context.Analysis.ProjectLayerMap"],
        "Classifies each project into an ArchitectureLayer using path heuristics and package references");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var map = new Dictionary<string, ArchitectureLayer>(StringComparer.OrdinalIgnoreCase);

        foreach (var project in model.Projects)
        {
            ct.ThrowIfCancellationRequested();

            var layer = ClassifyByPath(project.FilePath);

            if (layer == ArchitectureLayer.Unknown)
                layer = ClassifyByProjectName(project.Name);

            if (layer == ArchitectureLayer.Unknown)
                layer = ClassifyByPackages(project);

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
        if (lower.Contains("\\presentation\\") || lower.Contains("/presentation/") || lower.Contains(".presentation")
            || lower.Contains("\\blazor\\") || lower.Contains("/blazor/") || lower.Contains(".blazor")
            || lower.Contains("\\wasm\\") || lower.Contains("/wasm/") || lower.Contains(".wasm"))
            return ArchitectureLayer.Presentation;
        if (lower.Contains("\\tests\\") || lower.Contains("/tests/") || lower.Contains(".tests")
            || lower.Contains("\\test\\") || lower.Contains("/test/") || lower.Contains(".test"))
            return ArchitectureLayer.Testing;
        if (lower.Contains("\\shared\\") || lower.Contains("/shared/") || lower.Contains(".shared")
            || lower.Contains("\\core\\") || lower.Contains("/core/") || lower.Contains(".core"))
            return ArchitectureLayer.Shared;
        if (lower.Contains("\\apphost\\") || lower.Contains("/apphost/") || lower.Contains(".apphost")
            || lower.Contains("\\service-defaults\\") || lower.Contains(".servicedefaults"))
            return ArchitectureLayer.Infrastructure;

        return ArchitectureLayer.Unknown;
    }

    private static ArchitectureLayer ClassifyByProjectName(string projectName)
    {
        var lower = projectName.ToLowerInvariant();

        if (lower.Contains("domain")) return ArchitectureLayer.Domain;
        if (lower.Contains("application")) return ArchitectureLayer.Application;
        if (lower.Contains("infrastructure")) return ArchitectureLayer.Infrastructure;
        if (lower.Contains("persistence") || lower.Contains("data")) return ArchitectureLayer.Persistence;
        if (lower.Contains("api") || lower.Contains("webapi")) return ArchitectureLayer.Api;
        if (lower.Contains("web") || lower.Contains("blazor") || lower.Contains("wasm")
            || lower.Contains("presentation") || lower.Contains("ui"))
            return ArchitectureLayer.Presentation;
        if (lower.Contains("test") || lower.Contains("spec")) return ArchitectureLayer.Testing;
        if (lower.Contains("shared") || lower.Contains("common")) return ArchitectureLayer.Shared;

        return ArchitectureLayer.Unknown;
    }

    private static ArchitectureLayer ClassifyByPackages(ProjectInfo project)
    {
        var hasFastEndpoints = project.PackageReferences.Any(p =>
            p.Name.Contains("FastEndpoints", StringComparison.OrdinalIgnoreCase));

        if (hasFastEndpoints)
            return ArchitectureLayer.Api;

        var hasMediatR = project.PackageReferences.Any(p =>
            p.Name.Equals("MediatR", StringComparison.OrdinalIgnoreCase));

        if (hasMediatR)
            return ArchitectureLayer.Application;

        var hasEfCore = project.PackageReferences.Any(p =>
            p.Name.Contains("EntityFrameworkCore", StringComparison.OrdinalIgnoreCase));

        if (hasEfCore)
            return ArchitectureLayer.Persistence;

        var hasAspire = project.PackageReferences.Any(p =>
            p.Name.Contains("Aspire", StringComparison.OrdinalIgnoreCase));

        if (hasAspire)
            return ArchitectureLayer.Infrastructure;

        return ArchitectureLayer.Unknown;
    }
}
