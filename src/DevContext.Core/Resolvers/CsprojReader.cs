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

    /// <summary>The project's <c>&lt;OutputType&gt;</c> (e.g. "Exe", "Library"), or null when unset.</summary>
    public static string? ParseOutputType(XDocument doc)
        => doc.Descendants("OutputType").FirstOrDefault()?.Value?.Trim() is { Length: > 0 } v ? v : null;

    /// <summary>True when the project opts into packaging (<c>&lt;IsPackable&gt;true&lt;/c&gt;</c> or
    /// <c>&lt;GeneratePackageOnBuild&gt;true&lt;/c&gt;</c>) — a strong "this is a library" signal.</summary>
    public static bool ParseIsPackable(XDocument doc)
        => IsTrue(doc, "IsPackable") || IsTrue(doc, "GeneratePackageOnBuild");

    private static bool IsTrue(XDocument doc, string element)
        => doc.Descendants(element).FirstOrDefault()?.Value?.Trim()
            .Equals("true", StringComparison.OrdinalIgnoreCase) == true;
}
