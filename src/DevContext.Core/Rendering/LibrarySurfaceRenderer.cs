using System.Text;

using DevContext.Core.Graph;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Rendering;

/// <summary>
/// Renders a <see cref="Archetype.Library"/> as a capability-grouped PUBLIC SURFACE (design §4) in place
/// of the app entry-point inventory. Section-aware via <see cref="NarrativeSections"/> — same fragment
/// pattern as <see cref="MapRenderer"/> so the CLI markdown and the desktop drawer stay in sync.
/// </summary>
public static class LibrarySurfaceRenderer
{
    private const int MaxPackagesPerGroup = 8;

    public static ValueTask<RenderedContext> RenderAsync(MapRenderContext ctx, CancellationToken ct)
    {
        var model = ctx.Snapshot.Model;
        var surface = ctx.Map.Surface;
        var sections = new List<NarrativeSection>();

        Add(sections, "Overview", sb =>
        {
            var name = model.Solution?.Name ?? "library";
            var typeCount = surface?.Groups.Sum(g => g.Types.Length) ?? 0;
            sb.AppendLine($"LIBRARY  {name}     ({typeCount} public type{(typeCount != 1 ? "s" : "")})");
            sb.AppendLine();
            if (!string.IsNullOrEmpty(ctx.Map.StyleEvidence))
            {
                sb.AppendLine($"STYLE  {ctx.Map.Style}");
                sb.AppendLine();
            }
        });
        Add(sections, "Public surface", sb => AppendSurface(sb, surface));
        Add(sections, "Extension points", sb => AppendExtensionPoints(sb, surface));
        Add(sections, "Packages", sb => AppendPackages(sb, ctx.Map));
        Add(sections, "Footer", sb =>
            sb.AppendLine("→ drill in:  --focus \"<TypeName>\"   (e.g. --focus Mapper)"));

        return new ValueTask<RenderedContext>(NarrativeSections.ToRenderedContext(sections));
    }

    private static void Add(List<NarrativeSection> sections, string key, Action<StringBuilder> build)
    {
        var sb = new StringBuilder();
        build(sb);
        if (sb.Length > 0)
            sections.Add(new NarrativeSection(key, sb.ToString()));
    }

    private static void AppendSurface(StringBuilder sb, LibrarySurface? surface)
    {
        if (surface is null || surface.Groups.IsDefaultOrEmpty) return;
        sb.AppendLine("PUBLIC SURFACE");
        foreach (var group in surface.Groups)
        {
            sb.AppendLine($"   {group.Namespace}");
            foreach (var type in group.Types)
            {
                var kind = type.Kind.ToString().ToLowerInvariant();
                var members = type.Members.IsDefaultOrEmpty ? "" : ":  " + string.Join(", ", type.Members);
                sb.AppendLine($"      {type.Name} ({kind}){members}");
            }
        }
        sb.AppendLine();
    }

    private static void AppendExtensionPoints(StringBuilder sb, LibrarySurface? surface)
    {
        if (surface is null || surface.ExtensionPoints.IsDefaultOrEmpty) return;
        sb.AppendLine("EXTENSION POINTS");
        foreach (var ep in surface.ExtensionPoints)
            sb.AppendLine($"   {ep}");
        sb.AppendLine();
    }

    private static void AppendPackages(StringBuilder sb, MapModel map)
    {
        if (map.Packages.IsDefaultOrEmpty) return;
        sb.AppendLine("PACKAGES");
        foreach (var group in map.Packages)
        {
            var shown = group.Packages.Take(MaxPackagesPerGroup).ToList();
            var line = string.Join(", ", shown);
            if (group.Packages.Length > MaxPackagesPerGroup)
                line += $" … ({group.Packages.Length} total)";
            sb.AppendLine($"   {group.Label}:  {line}");
        }
        sb.AppendLine();
    }
}
