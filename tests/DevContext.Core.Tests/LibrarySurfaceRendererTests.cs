using System.Text;
using DevContext.Core.Graph;
using DevContext.Core.Models;
using DevContext.Core.Rendering;
using Xunit;

namespace DevContext.Core.Tests;

/// <summary>T3.8 — a huge library's PUBLIC SURFACE (MassTransit: 4,531 types / 210 namespaces = 476 KB)
/// is a reference, not a read. The renderer caps namespaces and types-per-namespace and points at the
/// structured surface for the full set, so the report stays a bounded artifact.
/// <para>R4 §1 item 1 — the pointer used to name <c>--format json</c>, a flag only the CLI has, in
/// markdown that also ships over MCP and the desktop. The cap is unchanged; the words are now true
/// on every surface, and the no-CLI-flag assertion below is the gate that keeps them that way.</para></summary>
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
        Assert.Contains("more namespaces (the structured surface lists them all)", text);

        // Types-per-namespace are capped (12) with a per-namespace "more" pointer.
        var typeLines = lines.Count(l => l.TrimStart().StartsWith("Type"));
        Assert.Equal(25 * 12, typeLines);
        Assert.Contains("more (the structured surface lists them all)", text);

        // A 40x30 surface flat would be ~1200 type lines; the cap holds it to ~300.
        Assert.True(text.Length < 40_000, $"expected a bounded surface, got {text.Length} chars");
    }

    /// <summary>R4 §1 item 1 — this markdown is served verbatim over MCP (`map`) and the desktop.
    /// A pointer naming a CLI flag sends an agent to a surface it does not have.</summary>
    [Fact]
    public void AppendSurface_pointers_name_no_CLI_flag()
    {
        var sb = new StringBuilder();
        LibrarySurfaceRenderer.AppendSurface(sb, HugeSurface(namespaces: 40, typesPerNs: 30));
        var text = sb.ToString();

        var flags = System.Text.RegularExpressions.Regex.Matches(text, @"--[a-z][a-z-]+")
            .Select(m => m.Value).Distinct().ToArray();
        Assert.True(flags.Length == 0, $"library surface markdown advertises CLI flags: {string.Join(", ", flags)}");
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
        Assert.DoesNotContain("the structured surface lists them all", text);
    }
}
