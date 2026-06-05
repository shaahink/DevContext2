using System.Xml.Linq;

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
                var tfms = ParseTargetFrameworks(doc);
                var refs = ParseProjectReferences(doc);
                var packages = ParsePackageReferences(doc);

                projects.Add(new ProjectInfo(
                    name, csprojPath, "C#",
                    tfms, refs, packages));
            }
            catch (Exception ex)
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name,
                    $"Failed to parse {csprojPath}: {ex.Message}");
            }
        }

        model.Projects = projects.ToImmutableArray();
    }

    private static ImmutableArray<string> ParseTargetFrameworks(XDocument doc)
    {
        var tfm = doc.Descendants("TargetFramework").FirstOrDefault()?.Value
               ?? doc.Descendants("TargetFrameworks").FirstOrDefault()?.Value;
        return tfm != null ? [tfm] : [];
    }

    private static ImmutableArray<string> ParseProjectReferences(XDocument doc)
        => doc.Descendants("ProjectReference")
            .Select(r => r.Attribute("Include")?.Value ?? "")
            .Where(v => !string.IsNullOrEmpty(v))
            .ToImmutableArray();

    private static ImmutableArray<PackageReferenceInfo> ParsePackageReferences(XDocument doc)
        => doc.Descendants("PackageReference")
            .Select(r => new PackageReferenceInfo(
                r.Attribute("Include")?.Value ?? r.Attribute("Update")?.Value ?? "",
                r.Attribute("Version")?.Value ?? ""))
            .Where(p => !string.IsNullOrEmpty(p.Name))
            .ToImmutableArray();
}
