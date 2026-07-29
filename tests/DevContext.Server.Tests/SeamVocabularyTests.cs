using System.Reflection;

using DevContext.Core.Graph;
using DevContext.Mcp;

namespace DevContext.Server.Tests;

/// <summary>
/// G1.3 (R4 §1 item 3) — every seam kind must be sayable on every surface that renders one.
///
/// The MCP's compact trace matched "Sends" / "Raises" / "Consumes" / "Handles". Core's
/// <see cref="SeamKind"/> has never produced a plural — the server sends
/// <c>step.Seam.ToString()</c>, i.e. "Send", "Handle", "Raise", "Consume" — and Data / Resolve /
/// Pipeline had no arm at all. So SEVEN of the ten seam kinds rendered the same mute "·" and a
/// compact trace could say what the next step was but never why it followed: a DI resolution, a bus
/// send and a database write all looked identical. Core's own tree had the smaller version of the
/// same hole: CrossService fell through to "?".
///
/// Both switches are hand-written against a Core enum neither can see (the MCP is a gRPC client and
/// deliberately does not reference Core), so the only thing that can keep them honest is a test
/// that walks the real enum.
/// </summary>
public sealed class SeamVocabularyTests
{
    /// <summary>The exact wire value ProtoMapper sends for a trace step's seam.</summary>
    private static string OnTheWire(SeamKind kind) => kind.ToString();

    [Fact]
    public void Every_seam_kind_has_an_mcp_glyph()
    {
        var mute = Enum.GetValues<SeamKind>()
            .Where(k => DevContextTools.SeamGlyph(OnTheWire(k)) is "·" or "")
            .ToArray();

        Assert.Empty(mute);
    }

    /// <summary>Distinct glyphs, or the reader cannot tell a send from a database write.</summary>
    [Fact]
    public void The_glyphs_are_distinct()
    {
        var glyphs = Enum.GetValues<SeamKind>().Select(k => DevContextTools.SeamGlyph(OnTheWire(k))).ToArray();

        Assert.Equal(glyphs.Length, glyphs.Distinct(StringComparer.Ordinal).Count());
    }

    /// <summary>
    /// A glyph the wire spells differently must not silently fall back. This is the assertion that
    /// would have caught the plurals the day they shipped.
    /// </summary>
    [Fact]
    public void An_unmapped_seam_names_itself_instead_of_going_mute()
    {
        Assert.Equal("[Sends]", DevContextTools.SeamGlyph("Sends"));
        Assert.Equal("·", DevContextTools.SeamGlyph(""));
    }

    /// <summary>The legend names only the glyphs a given render actually used.</summary>
    [Fact]
    public void The_legend_covers_what_was_rendered()
    {
        var rendered = $"{DevContextTools.SeamGlyph("Entry")} x\n  {DevContextTools.SeamGlyph("Send")} y\n";

        var legend = DevContextTools.SeamLegend(rendered);

        Assert.NotNull(legend);
        Assert.Contains("entry", legend, StringComparison.Ordinal);
        Assert.Contains("send", legend, StringComparison.Ordinal);
        Assert.DoesNotContain("pipeline", legend, StringComparison.Ordinal);
        Assert.Null(DevContextTools.SeamLegend("no glyphs here"));
    }

    /// <summary>
    /// Core's own tree: the CLI printed a bare "?" for every cross-service hop.
    ///
    /// <see cref="SeamKind.Entry"/> is excluded on purpose, and it was measured before excluding it:
    /// its label is EMPTY by design because the root step renders "▸ ENTRY" from its own branch and
    /// never asks for a label. Entry is the one seam that cannot reach this switch.
    /// </summary>
    [Fact]
    public void Every_seam_kind_a_step_can_carry_has_a_core_label()
    {
        var label = typeof(DevContext.Core.Rendering.TraceRenderer).GetMethod(
            "SeamLabel", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(label); // renamed or gone — this test is then measuring nothing

        var unlabelled = Enum.GetValues<SeamKind>()
            .Where(k => k != SeamKind.Entry)
            .Where(k => (string)label!.Invoke(null, [k])! is "?" or "")
            .ToArray();

        Assert.Empty(unlabelled);
    }
}
