using System.Xml.Linq;

namespace DevContext.Core.Resolvers;

/// <summary>
/// Shared <c>.csproj</c> XML reads, so resolve-time scope/closure resolution and the discovery
/// extractor parse <c>&lt;ProjectReference&gt;</c> the same way (no drift between the scan-set walk and
/// <see cref="DevContext.Core.Extractors.Generic.ProjectStructureExtractor"/>).
/// From A-F15: also resolves properties from the <c>Directory.Build.props</c> ancestor chain and
/// <c>Directory.Packages.props</c> for Central Package Management (CPM) version resolution.
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

    /// <summary>The root <c>&lt;Project Sdk="..."&gt;</c> attribute (e.g. "Microsoft.NET.Sdk.Web",
    /// "Microsoft.Build.NoTargets/3.3.0"), or null for old-style/attribute-less projects. A NoTargets or
    /// Traversal SDK marks a HOLDER project — a csproj that builds no code (Prism D1.1b / audit E2).</summary>
    public static string? ParseSdk(XDocument doc)
        => doc.Root?.Attribute("Sdk")?.Value?.Trim() is { Length: > 0 } v ? v : null;

    /// <summary>D1.1d — true when the project declares dotnet-tool packaging anywhere in the csproj
    /// (<c>&lt;PackAsTool&gt;</c> or <c>&lt;ToolCommandName&gt;</c>, including inside conditional
    /// PropertyGroups — GitVersion.App sets both under a CI-only condition). Element PRESENCE is the
    /// evidence: a repo that ships a dotnet tool is a CLI tool regardless of the local build config.</summary>
    public static bool ParseIsToolPackaged(XDocument doc)
        => doc.Descendants("PackAsTool").Any() || doc.Descendants("ToolCommandName").Any();

    /// <summary>True when the project opts into packaging (<c>&lt;IsPackable&gt;true&lt;/c&gt;</c> or
    /// <c>&lt;GeneratePackageOnBuild&gt;true&lt;/c&gt;</c>) — a strong "this is a library" signal.</summary>
    public static bool ParseIsPackable(XDocument doc)
        => IsTrue(doc, "IsPackable") || IsTrue(doc, "GeneratePackageOnBuild");

    private static bool IsTrue(XDocument doc, string element)
        => doc.Descendants(element).FirstOrDefault()?.Value?.Trim()
            .Equals("true", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>Resolves OutputType by walking the <c>Directory.Build.props</c> ancestor chain from
    /// the csproj's directory. The csproj's own value (in <paramref name="doc"/>) takes precedence;
    /// ancestor values fill in when the csproj doesn't set it. Nearest ancestor wins among imports.
    /// <para>A CONDITIONED ancestor value is not evidence for this project (Prism D1.2b): a shared
    /// props file sets properties for a SUBSET of its projects, and we cannot evaluate MSBuild
    /// conditions. xunit's src/Directory.Build.props sets OutputType=Exe inside a
    /// <c>&lt;When Condition="...EndsWith('.tests')"&gt;</c>; taking it unconditionally made every
    /// xunit CLASSLIB read as an exe, which erased the self-sourced framework signal (the exe is
    /// "runnable") and flipped the repo Library -> App with a console-view render. The csproj's OWN
    /// conditioned value is still honoured — it at least applies to that project.</para></summary>
    public static string? ResolveOutputType(XDocument doc, string csprojPath)
    {
        var direct = ParseOutputType(doc);
        if (direct is not null) return direct;
        foreach (var ancestor in WalkAncestorProps(csprojPath, "Directory.Build.props"))
        {
            var v = ParseUnconditionedOutputType(ancestor);
            if (v is not null) return v;
        }
        return null;
    }

    /// <summary>The first <c>&lt;OutputType&gt;</c> that no enclosing element makes conditional.</summary>
    private static string? ParseUnconditionedOutputType(XDocument doc)
        => doc.Descendants("OutputType")
            .Where(e => !IsConditioned(e))
            .Select(e => e.Value.Trim())
            .FirstOrDefault(v => v.Length > 0);

    /// <summary>True when the element or any ancestor carries a non-empty <c>Condition</c> attribute
    /// (<c>&lt;When&gt;</c>, a conditioned <c>&lt;PropertyGroup&gt;</c>, or the property itself), or it
    /// sits in an <c>&lt;Otherwise&gt;</c> branch — all of which make the value apply to only some projects.</summary>
    private static bool IsConditioned(XElement e)
    {
        for (var n = e; n is not null; n = n.Parent)
        {
            if (n.Attribute("Condition")?.Value.Trim() is { Length: > 0 }) return true;
            if (n.Name.LocalName is "Otherwise") return true;
        }
        return false;
    }

    /// <summary>Resolves <c>IsPackable</c> from the ancestor chain. The csproj's own value wins;
    /// ancestor <c>Directory.Build.props</c> files fill in. Returns false when unset everywhere.</summary>
    public static bool ResolveIsPackable(XDocument doc, string csprojPath)
    {
        if (ParseIsPackable(doc)) return true;
        foreach (var ancestor in WalkAncestorProps(csprojPath, "Directory.Build.props"))
            if (ParseIsPackable(ancestor)) return true;
        return false;
    }

    /// <summary>Parses the target framework(s) from the csproj, falling back to
    /// <c>Directory.Build.props</c> ancestors when unset. Returns the TFM string ("net10.0") or
    /// multi-targeting string ("net10.0;netstandard2.0"). Returns empty when unset everywhere.</summary>
    public static ImmutableArray<string> ResolveTargetFrameworks(XDocument doc, string csprojPath)
        => ParseTargetFrameworks(doc) is { Length: > 0 } tfms ? tfms
            : ResolveTargetFrameworksFromAncestors(csprojPath);

    /// <summary>Parses target frameworks directly from a document (without ancestor fallback).
    /// E5 (Prism D1.4b): a multi-targeting <c>&lt;TargetFrameworks&gt;</c> value is SPLIT on ';' so
    /// consumers see real TFMs — Newtonsoft's "net46;net40;net35;net20" used to travel as one
    /// unreadable token all the way to the STACK line.</summary>
    public static ImmutableArray<string> ParseTargetFrameworks(XDocument doc)
    {
        var tfm = doc.Descendants("TargetFramework").FirstOrDefault()?.Value
               ?? doc.Descendants("TargetFrameworks").FirstOrDefault()?.Value;
        return tfm is { Length: > 0 }
            ? [.. tfm.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)]
            : [];
    }

    private static ImmutableArray<string> ResolveTargetFrameworksFromAncestors(string csprojPath)
    {
        foreach (var ancestor in WalkAncestorProps(csprojPath, "Directory.Build.props"))
        {
            var tfm = ParseTargetFrameworks(ancestor);
            if (tfm is { Length: > 0 }) return tfm;
        }
        return [];
    }

    /// <summary>Builds a dictionary mapping NuGet package names to their CPM versions by walking
    /// the ancestor directory chain from the csproj looking for <c>Directory.Packages.props</c>.
    /// Returns empty when no CPM file is found. Keys are the package Include name, values are the
    /// Version attribute from <c>&lt;PackageVersion&gt;</c> elements.</summary>
    public static IReadOnlyDictionary<string, string> ResolveCpmVersions(string csprojPath)
    {
        var dir = Path.GetDirectoryName(csprojPath);
        if (dir is null) return new Dictionary<string, string>();

        foreach (var current in WalkUpDirectories(dir))
        {
            var cpmPath = Path.Combine(current, "Directory.Packages.props");
            if (!File.Exists(cpmPath)) continue;
            try
            {
                var doc = XDocument.Load(cpmPath);
                return doc.Descendants("PackageVersion")
                    .Select(pv => (
                        Name: pv.Attribute("Include")?.Value ?? "",
                        Version: pv.Attribute("Version")?.Value ?? ""))
                    .Where(x => !string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(x.Version))
                    .DistinctBy(x => x.Name)
                    .ToDictionary(x => x.Name, x => x.Version, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        return new Dictionary<string, string>();
    }

    /// <summary>Parses <c>&lt;PackageReference&gt;</c> elements from the csproj document, filling in
    /// missing Version attributes from the CPM <c>Directory.Packages.props</c> ancestor chain.
    /// Package references with <c>VersionOverride</c> keep their override value.</summary>
    public static ImmutableArray<PackageReferenceInfo> ParsePackageReferencesCpmAware(
        XDocument doc, string csprojPath)
    {
        var cpmVersions = ResolveCpmVersions(csprojPath);

        // E1 (Prism D1.4c): only `Include=` declares a dependency. An `Update=`-only element is an
        // MSBuild metadata patch on an item declared elsewhere — GitVersion's
        // `<PackageReference Update="@(PackageReference)">` ingested a "package" literally named
        // `@(PackageReference)`. MSBuild expressions are never real package ids either way.
        return doc.Descendants("PackageReference")
            .Select(r =>
            {
                var name = r.Attribute("Include")?.Value ?? "";
                var version = r.Attribute("Version")?.Value
                           ?? r.Attribute("VersionOverride")?.Value
                           ?? "";
                if (string.IsNullOrEmpty(version) && cpmVersions.TryGetValue(name, out var cpmVer))
                    version = cpmVer;
                return new PackageReferenceInfo(name, version);
            })
            .Where(p => !string.IsNullOrEmpty(p.Name)
                && !p.Name.Contains("@(", StringComparison.Ordinal)
                && !p.Name.Contains("$(", StringComparison.Ordinal))
            .ToImmutableArray();
    }

    private static IEnumerable<XDocument> WalkAncestorProps(string csprojPath, string fileName)
    {
        var dir = Path.GetDirectoryName(csprojPath);
        if (dir is null) yield break;
        foreach (var current in WalkUpDirectories(dir))
        {
            var path = Path.Combine(current, fileName);
            if (!File.Exists(path)) continue;
            XDocument? doc = null;
            try { doc = XDocument.Load(path); } catch { continue; }
            if (doc is not null) yield return doc;
        }
    }

    private static IEnumerable<string> WalkUpDirectories(string startDir)
    {
        var current = startDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        while (current is { Length: > 0 })
        {
            yield return current;
            var parent = Path.GetDirectoryName(current);
            if (parent is null || parent == current) break;
            current = parent;
        }
    }
}
