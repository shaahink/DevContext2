using System.Xml.Linq;

namespace DevContext.Core.Resolvers;

/// <summary>
/// Extracts the constituent <c>.csproj</c> paths from a solution file — both the legacy text
/// <c>.sln</c> format and the newer XML <c>.slnx</c> format. One parser so the discovery extractor,
/// the Roslyn workspace provider, and anything else read solutions the same way (no `.slnx` blind
/// spots, where four of the eval repos — eShop, AutoMapper, OrchardCore, VerticalSlice — live).
/// Paths are returned exactly as written in the file (relative to the solution directory).
/// </summary>
public static class SolutionFileParser
{
    /// <summary>True when the path's extension is a solution we can parse (<c>.sln</c> or <c>.slnx</c>).</summary>
    public static bool IsSolutionFile(string path)
    {
        var ext = Path.GetExtension(path);
        return ext.Equals(".sln", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".slnx", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Parses project paths from a solution file's content, dispatching on the file extension.</summary>
    public static ImmutableArray<string> ParseProjectPaths(string content, string solutionPath)
        => Path.GetExtension(solutionPath).Equals(".slnx", StringComparison.OrdinalIgnoreCase)
            ? ParseSlnx(content)
            : ParseSln(content);

    /// <summary>Legacy <c>.sln</c>: <c>Project("{guid}") = "Name", "rel\Path.csproj", "{guid}"</c> lines.</summary>
    private static ImmutableArray<string> ParseSln(string content)
    {
        var projects = new List<string>();
        foreach (var line in content.Split('\n'))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("Project(", StringComparison.Ordinal)) continue;

            var parts = trimmed.Split(',');
            if (parts.Length < 2) continue;

            var path = parts[1].Trim().Trim('"');
            if (path.Length > 0 && path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                projects.Add(path);
        }
        return [.. projects];
    }

    /// <summary>XML <c>.slnx</c>: <c>&lt;Project Path="rel/Path.csproj" /&gt;</c> elements, possibly nested in <c>&lt;Folder&gt;</c>.</summary>
    private static ImmutableArray<string> ParseSlnx(string content)
    {
        var projects = new List<string>();
        try
        {
            // .slnx files are often saved with a UTF-8 BOM; XDocument.Parse chokes on a leading BOM char.
            var doc = XDocument.Parse(content.TrimStart('﻿'));
            foreach (var el in doc.Descendants("Project"))
            {
                var path = el.Attribute("Path")?.Value;
                if (!string.IsNullOrEmpty(path) && path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                    projects.Add(path);
            }
        }
        catch (System.Xml.XmlException)
        {
            // Malformed solution XML — treat as no projects rather than failing the whole run.
        }
        return [.. projects];
    }
}
