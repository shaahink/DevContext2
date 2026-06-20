using System.Xml.Linq;

namespace DevContext.Core.Resolvers;

/// <summary>
/// Shared <c>.csproj</c> XML reads, so resolve-time scope/closure resolution and the discovery
/// extractor parse <c>&lt;ProjectReference&gt;</c> the same way (no drift between the scan-set walk and
/// <see cref="DevContext.Core.Extractors.Generic.ProjectStructureExtractor"/>).
/// </summary>
public static class CsprojReader
{
    /// <summary>The raw <c>&lt;ProjectReference Include="..."&gt;</c> paths (relative to the csproj dir).</summary>
    public static ImmutableArray<string> ParseProjectReferences(XDocument doc)
        => doc.Descendants("ProjectReference")
            .Select(r => r.Attribute("Include")?.Value ?? "")
            .Where(v => !string.IsNullOrEmpty(v))
            .ToImmutableArray();
}
