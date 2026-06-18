using System.Text;

namespace DevContext.Core.Rendering;

/// <summary>One named block of a Map/Trace narrative — a stable key plus its rendered text.
/// The text retains its own trailing blank line so concatenating fragments reproduces the
/// monolithic narrative byte-for-byte (CLI / golden / eval output is unchanged).</summary>
public readonly record struct NarrativeSection(string Key, string Text);

/// <summary>Assembles ordered narrative fragments into a <see cref="RenderedContext"/> that carries
/// both the joined <c>Content</c> and the per-section fragments the desktop toggles. This is what
/// makes Map/Trace section-aware: the same fragments drive the LLM (markdown) view and, after HTML
/// conversion, the Human view — kept in sync by the shared section keys.</summary>
public static class NarrativeSections
{
    /// <summary>Joins fragments into a rendered context with Content, per-section stats, and a
    /// key→fragment dictionary. Token estimates are character/4 approximations.</summary>
    public static RenderedContext ToRenderedContext(IReadOnlyList<NarrativeSection> sections, string schemaVersion = "2.0")
    {
        var sb = new StringBuilder();
        var stats = ImmutableArray.CreateBuilder<SectionStat>(sections.Count);
        var fragments = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var s in sections)
        {
            sb.Append(s.Text);
            stats.Add(new SectionStat(s.Key, Math.Max(1, s.Text.Length / 4)));
            fragments[s.Key] = s.Text;
        }

        var content = sb.ToString();
        return new RenderedContext(content, content.Length / 4, [], TimeSpan.Zero, schemaVersion)
        {
            Sections = stats.ToImmutable(),
            SectionFragments = fragments,
        };
    }

    /// <summary>Appends one extra fragment (e.g. the diagnostics tail) to an already-assembled
    /// narrative, keeping Content, stats, and fragments consistent. No-op for empty text.</summary>
    public static RenderedContext WithExtraSection(RenderedContext rc, string key, string text)
    {
        if (string.IsNullOrEmpty(text)) return rc;

        var fragments = rc.SectionFragments is null
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(rc.SectionFragments, StringComparer.Ordinal);
        fragments[key] = text;

        var content = rc.Content + text;
        return rc with
        {
            Content = content,
            EstimatedTokens = content.Length / 4,
            Sections = rc.Sections.Add(new SectionStat(key, Math.Max(1, text.Length / 4))),
            SectionFragments = fragments,
        };
    }
}
