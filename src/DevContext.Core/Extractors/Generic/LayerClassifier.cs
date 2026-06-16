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

        if (lower.Contains("\\domain\\", StringComparison.Ordinal) || lower.Contains("/domain/", StringComparison.Ordinal) || lower.Contains(".domain", StringComparison.Ordinal))
            return ArchitectureLayer.Domain;
        if (lower.Contains("\\application\\", StringComparison.Ordinal) || lower.Contains("/application/", StringComparison.Ordinal) || lower.Contains(".application", StringComparison.Ordinal))
            return ArchitectureLayer.Application;
        if (lower.Contains("\\infrastructure\\", StringComparison.Ordinal) || lower.Contains("/infrastructure/", StringComparison.Ordinal) || lower.Contains(".infrastructure", StringComparison.Ordinal))
            return ArchitectureLayer.Infrastructure;
        if (lower.Contains("\\persistence\\", StringComparison.Ordinal) || lower.Contains("/persistence/", StringComparison.Ordinal) || lower.Contains(".persistence", StringComparison.Ordinal)
            || lower.Contains("\\data\\", StringComparison.Ordinal) || lower.Contains("/data/", StringComparison.Ordinal) || lower.Contains(".data", StringComparison.Ordinal))
            return ArchitectureLayer.Persistence;
        if (lower.Contains("\\api\\", StringComparison.Ordinal) || lower.Contains("/api/", StringComparison.Ordinal) || lower.Contains(".api", StringComparison.Ordinal)
            || lower.Contains("\\web\\", StringComparison.Ordinal) || lower.Contains("/web/", StringComparison.Ordinal) || lower.Contains(".web", StringComparison.Ordinal))
            return ArchitectureLayer.Api;
        if (lower.Contains("\\presentation\\", StringComparison.Ordinal) || lower.Contains("/presentation/", StringComparison.Ordinal) || lower.Contains(".presentation", StringComparison.Ordinal)
            || lower.Contains("\\blazor\\", StringComparison.Ordinal) || lower.Contains("/blazor/", StringComparison.Ordinal) || lower.Contains(".blazor", StringComparison.Ordinal)
            || lower.Contains("\\wasm\\", StringComparison.Ordinal) || lower.Contains("/wasm/", StringComparison.Ordinal) || lower.Contains(".wasm", StringComparison.Ordinal))
            return ArchitectureLayer.Presentation;
        if (lower.Contains("\\tests\\", StringComparison.Ordinal) || lower.Contains("/tests/", StringComparison.Ordinal) || lower.Contains(".tests", StringComparison.Ordinal)
            || lower.Contains("\\test\\", StringComparison.Ordinal) || lower.Contains("/test/", StringComparison.Ordinal) || lower.Contains(".test", StringComparison.Ordinal))
            return ArchitectureLayer.Testing;
        if (lower.Contains("\\shared\\", StringComparison.Ordinal) || lower.Contains("/shared/", StringComparison.Ordinal) || lower.Contains(".shared", StringComparison.Ordinal)
            || lower.Contains("\\core\\", StringComparison.Ordinal) || lower.Contains("/core/", StringComparison.Ordinal) || lower.Contains(".core", StringComparison.Ordinal))
            return ArchitectureLayer.Shared;
        if (lower.Contains("\\apphost\\", StringComparison.Ordinal) || lower.Contains("/apphost/", StringComparison.Ordinal) || lower.Contains(".apphost", StringComparison.Ordinal)
            || lower.Contains("\\service-defaults\\", StringComparison.Ordinal) || lower.Contains(".servicedefaults", StringComparison.Ordinal))
            return ArchitectureLayer.Infrastructure;

        return ArchitectureLayer.Unknown;
    }

    private static ArchitectureLayer ClassifyByProjectName(string projectName)
    {
        var lower = projectName.ToLowerInvariant();

        if (lower.Contains("domain", StringComparison.Ordinal)) return ArchitectureLayer.Domain;
        if (lower.Contains("application", StringComparison.Ordinal)) return ArchitectureLayer.Application;
        if (lower.Contains("infrastructure", StringComparison.Ordinal)) return ArchitectureLayer.Infrastructure;
        if (lower.Contains("persistence", StringComparison.Ordinal) || lower.Contains("data", StringComparison.Ordinal)) return ArchitectureLayer.Persistence;
        if (lower.Contains("api", StringComparison.Ordinal) || lower.Contains("webapi", StringComparison.Ordinal)) return ArchitectureLayer.Api;
        if (lower.Contains("web", StringComparison.Ordinal) || lower.Contains("blazor", StringComparison.Ordinal) || lower.Contains("wasm", StringComparison.Ordinal)
            || lower.Contains("presentation", StringComparison.Ordinal) || lower.Contains("ui", StringComparison.Ordinal))
            return ArchitectureLayer.Presentation;
        if (lower.Contains("test", StringComparison.Ordinal) || lower.Contains("spec", StringComparison.Ordinal)) return ArchitectureLayer.Testing;
        if (lower.Contains("shared", StringComparison.Ordinal) || lower.Contains("common", StringComparison.Ordinal)) return ArchitectureLayer.Shared;

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
