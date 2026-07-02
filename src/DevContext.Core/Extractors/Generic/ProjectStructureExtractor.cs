using DevContext.Core.Resolvers;

namespace DevContext.Core.Extractors.Generic;

/// <summary>Parses .csproj files to extract project structure information including target frameworks, references, and packages.</summary>
[ExtractorOrder(0)]
public sealed class ProjectStructureExtractor : IDiscoveryExtractor
{
    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "ProjectStructure";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Generic;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage1Sequential;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [], [], ["model.Projects"],
        "Parses .csproj files to extract project structure info");
    /// <summary>Determines whether this extractor should run.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var projects = new List<ProjectInfo>();

        foreach (var csprojPath in context.Analysis.AllProjectFiles)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var doc = await context.Cache.GetXmlAsync(csprojPath, ct);
                var name = Path.GetFileNameWithoutExtension(csprojPath);
                var tfms = CsprojReader.ResolveTargetFrameworks(doc, csprojPath);
                var refs = CsprojReader.ParseProjectReferences(doc);
                var packages = CsprojReader.ParsePackageReferencesCpmAware(doc, csprojPath);

                projects.Add(new ProjectInfo(
                    name, csprojPath, "C#",
                    tfms, refs, packages,
                    CsprojReader.ResolveOutputType(doc, csprojPath),
                    CsprojReader.ResolveIsPackable(doc, csprojPath)));
            }
            catch (Exception ex)
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name,
                    $"Failed to parse {csprojPath}: {ex.Message}");
            }
        }

        model.Projects = projects.ToImmutableArray();
    }
}
