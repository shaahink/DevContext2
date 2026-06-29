using System.Text;

using DevContext.Core.Graph;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Rendering;

/// <summary>
/// Renders a <see cref="Archetype.Library"/> as a ranked, capability-grouped surface (design §4) in place
/// of the app entry-point inventory: <c>ENTRY API</c> (how you use it) → <c>ABSTRACTIONS</c> (what you
/// implement/derive) → <c>PUBLIC SURFACE</c> (by namespace, internals demoted) → <c>CONSUMER PATHS</c> →
/// runtime <c>PACKAGES</c>. Section-aware via <see cref="NarrativeSections"/> — same fragment pattern as
/// <see cref="MapRenderer"/> so the CLI markdown and the desktop drawer stay in sync.
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
        Add(sections, "Entry API", sb => AppendEntryApi(sb, surface));
        Add(sections, "Abstractions", sb => AppendAbstractions(sb, surface));
        Add(sections, "Public surface", sb => AppendSurface(sb, surface));
        Add(sections, "Consumer paths", sb => AppendConsumerPaths(sb, surface));
        Add(sections, "Packages", sb => AppendPackages(sb, surface));
        Add(sections, "Footer", sb =>
            sb.AppendLine($"→ drill in:  --focus \"<TypeName>\"   (e.g. --focus {ExampleFocus(surface)})"));

        return new ValueTask<RenderedContext>(NarrativeSections.ToRenderedContext(sections));
    }

    private static void Add(List<NarrativeSection> sections, string key, Action<StringBuilder> build)
    {
        var sb = new StringBuilder();
        build(sb);
        if (sb.Length > 0)
            sections.Add(new NarrativeSection(key, sb.ToString()));
    }

    private static void AppendEntryApi(StringBuilder sb, LibrarySurface? surface)
    {
        if (surface is null || surface.EntryApi.IsDefaultOrEmpty) return;
        sb.AppendLine("ENTRY API");
        foreach (var e in surface.EntryApi)
        {
            var loc = string.IsNullOrEmpty(e.Location) ? "" : $"   ({e.Location})";
            sb.AppendLine($"   {e.Kind,-9} {e.Title}{loc}");
            if (!string.IsNullOrEmpty(e.Doc))
                sb.AppendLine($"      {e.Doc}");
        }
        sb.AppendLine();
    }

    private static void AppendAbstractions(StringBuilder sb, LibrarySurface? surface)
    {
        if (surface is null || surface.Abstractions.IsDefaultOrEmpty) return;
        sb.AppendLine("ABSTRACTIONS");
        foreach (var a in surface.Abstractions)
        {
            var kind = a.Kind.ToString().ToLowerInvariant();
            var impl = a.ImplementorCount == 1 ? "1 implementor" : $"{a.ImplementorCount} implementors";
            sb.AppendLine($"   {a.Name} ({kind})  — {impl}");
        }
        sb.AppendLine();
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
                if (!string.IsNullOrEmpty(type.Doc))
                    sb.AppendLine($"         {type.Doc}");
            }
        }
        if (!surface.Internals.IsDefaultOrEmpty)
        {
            var n = surface.Internals.Sum(g => g.Types.Length);
            sb.AppendLine($"   INTERNAL  ({n} type{(n != 1 ? "s" : "")} in *.Internal — available on request)");
        }
        sb.AppendLine();
    }

    private static void AppendConsumerPaths(StringBuilder sb, LibrarySurface? surface)
    {
        if (surface is null || surface.ConsumerPaths.IsDefaultOrEmpty) return;
        sb.AppendLine("CONSUMER PATHS");
        foreach (var p in surface.ConsumerPaths)
            sb.AppendLine($"   {p}");
        sb.AppendLine();
    }

    private static void AppendPackages(StringBuilder sb, LibrarySurface? surface)
    {
        if (surface is null || surface.Packages.IsDefaultOrEmpty) return;
        sb.AppendLine("PACKAGES");
        foreach (var group in surface.Packages)
        {
            var shown = group.Packages.Take(MaxPackagesPerGroup).ToList();
            var line = string.Join(", ", shown);
            if (group.Packages.Length > MaxPackagesPerGroup)
                line += $" … ({group.Packages.Length} total)";
            sb.AppendLine($"   {group.Label}:  {line}");
        }
        sb.AppendLine();
    }

    private static string ExampleFocus(LibrarySurface? surface)
        => surface?.EntryApi.FirstOrDefault()?.Title.Split('.')[0]
            ?? surface?.Groups.FirstOrDefault()?.Types.FirstOrDefault()?.Name
            ?? "TypeName";
}
