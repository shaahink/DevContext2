using System.Text;
using DevContext.Core.Graph;
using DevContext.Core.Models;
using DevContext.Core.Rendering;
using Xunit;

namespace DevContext.Core.Tests;

/// <summary>T3.8 — a huge library's PUBLIC SURFACE (MassTransit: 4,531 types / 210 namespaces = 476 KB)
/// is a reference, not a read. The renderer caps namespaces and types-per-namespace and points at
/// --format json for the full surface, so the report stays a bounded artifact.</summary>
public sealed class LibrarySurfaceRendererTests
{
    private static LibrarySurface HugeSurface(int namespaces, int typesPerNs)
    {
        var groups = ImmutableArray.CreateBuilder<SurfaceGroup>();
        for (var n = 0; n < namespaces; n++)
        {
            var types = ImmutableArray.CreateBuilder<SurfaceType>();
            for (var t = 0; t < typesPerNs; t++)
                types.Add(new SurfaceType($"Type{n}_{t}", TypeKind.Class, []));
            groups.Add(new SurfaceGroup($"Lib.Namespace{n}", types.ToImmutable()));
        }
        return new LibrarySurface(groups.ToImmutable(), []);
    }

    [Fact]
    public void AppendSurface_caps_namespaces_and_types_and_points_at_json()
    {
        var surface = HugeSurface(namespaces: 40, typesPerNs: 30);
        var sb = new StringBuilder();
        LibrarySurfaceRenderer.AppendSurface(sb, surface);
        var text = sb.ToString();
        var lines = text.Split('\n');

        // Namespaces are capped (25) with a "more namespaces" pointer.
        var nsHeaders = lines.Count(l => l.StartsWith("   Lib.Namespace"));
        Assert.Equal(25, nsHeaders);
        Assert.Contains("more namespaces (use --format json for the full surface)", text);

        // Types-per-namespace are capped (12) with a per-namespace "more" pointer.
        var typeLines = lines.Count(l => l.TrimStart().StartsWith("Type"));
        Assert.Equal(25 * 12, typeLines);
        Assert.Contains("more (use --format json for the full surface)", text);

        // A 40x30 surface flat would be ~1200 type lines; the cap holds it to ~300.
        Assert.True(text.Length < 40_000, $"expected a bounded surface, got {text.Length} chars");
    }

    [Fact]
    public void AppendSurface_small_surface_is_shown_whole_without_pointers()
    {
        var surface = HugeSurface(namespaces: 3, typesPerNs: 4);
        var sb = new StringBuilder();
        LibrarySurfaceRenderer.AppendSurface(sb, surface);
        var text = sb.ToString();

        Assert.Equal(3, text.Split('\n').Count(l => l.StartsWith("   Lib.Namespace")));
        Assert.DoesNotContain("more namespaces", text);
        Assert.DoesNotContain("more (use --format json", text);
    }
}
