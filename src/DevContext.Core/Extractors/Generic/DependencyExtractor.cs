using System.Xml.Linq;

namespace DevContext.Core.Extractors.Generic;

/// <summary>Detects NuGet package references to emit architecture signals and builds the project dependency graph.</summary>
[ExtractorOrder(10)]
public sealed class DependencyExtractor : IDiscoveryExtractor
{
    private static readonly FrozenDictionary<string, string> PackageSignalMap = new Dictionary<string, string>
    {
        ["MediatR"] = ArchitectureSignals.Keys.MediatR,
        ["Microsoft.EntityFrameworkCore"] = ArchitectureSignals.Keys.EfCore,
        ["Microsoft.AspNetCore.OpenApi"] = ArchitectureSignals.Keys.MinimalApis,
        ["MassTransit"] = ArchitectureSignals.Keys.MassTransit,
        ["Dapper"] = ArchitectureSignals.Keys.Dapper,
        ["FluentValidation"] = ArchitectureSignals.Keys.FluentValidation,
        ["Hangfire"] = ArchitectureSignals.Keys.Hangfire,
        ["Scrutor"] = ArchitectureSignals.Keys.Scrutor,
        ["Refit"] = ArchitectureSignals.Keys.Refit,
        ["Microsoft.Aspire"] = ArchitectureSignals.Keys.Aspire,
    }.ToFrozenDictionary();

    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "DependencyExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Generic;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage2Parallel;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [], [.. PackageSignalMap.Values],
        ["model.Architecture", "context.Analysis.ProjectGraph"],
        "Detects NuGet package references and builds project dependency graph");
    /// <summary>Determines whether this extractor should run.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var adjacency = new Dictionary<string, ImmutableArray<string>>();

        foreach (var projectInfo in model.Projects)
        {
            ct.ThrowIfCancellationRequested();

            foreach (var pkg in projectInfo.PackageReferences)
            {
                if (PackageSignalMap.TryGetValue(pkg.Name, out var signalKey))
                {
                    model.Architecture.Register(FeatureSignal.CreateDetected(
                        signalKey, confidence: 1.0f, via: "PackageReference", pkg.Name));
                }
            }

            var refs = projectInfo.ProjectReferences
                .Select(r => Path.GetFileNameWithoutExtension(r))
                .Where(r => !string.IsNullOrEmpty(r))
                .ToImmutableArray();

            adjacency[projectInfo.Name] = refs;

            var csprojPath = projectInfo.FilePath;
            if (!string.IsNullOrEmpty(csprojPath) && context.Cache.KnownFilePaths.Contains(csprojPath))
            {
                try
                {
                    var doc = await context.Cache.GetXmlAsync(csprojPath, ct);
                    var packageRefs = doc.Descendants("PackageReference")
                        .Select(r => r.Attribute("Include")?.Value ?? r.Attribute("Update")?.Value ?? "")
                        .Where(v => !string.IsNullOrEmpty(v));

                    foreach (var pkgName in packageRefs)
                    {
                        if (PackageSignalMap.TryGetValue(pkgName, out var extraSignalKey))
                        {
                            model.Architecture.Register(FeatureSignal.CreateDetected(
                                extraSignalKey, confidence: 1.0f, via: "PackageReference", pkgName));
                        }
                    }
                }
                catch
                {
                }
            }
        }

        context.Analysis.ProjectGraph = new ProjectDependencyGraph(adjacency);
    }
}
