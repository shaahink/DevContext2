using System.Collections.Immutable;

using DevContext.Core.Analysis;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Graph;

/// <summary>One drifted file inside a section: modified (hash changed, with the line-count
/// delta), deleted, or unknown (no analyze-time fingerprint to compare against).</summary>
public sealed record FileDelta(string File, string Status, int LineDelta);

/// <summary>Verification verdict for one pack section: its file set, whether any of it drifted.</summary>
public sealed record SectionVerification(
    string Section, bool Stale, int FilesChecked, ImmutableArray<FileDelta> Changed);

/// <summary>T4.5 (audit R6, engine half) — compares the snapshot's analyze-time file fingerprints
/// against disk NOW, per pack section (a section's file set = its T4.4 SourceLocations). v1 by
/// design: hash + line-count delta per file, nothing more — no diff engine.</summary>
public sealed class ContextPackVerifier
{
    private readonly AnalysisSnapshot _snapshot;

    public ContextPackVerifier(AnalysisSnapshot snapshot)
    {
        _snapshot = snapshot;
    }

    public ImmutableArray<SectionVerification> Verify(ImmutableArray<SectionAllocation> sections)
    {
        var results = ImmutableArray.CreateBuilder<SectionVerification>();
        // Disk state is read once per distinct file, not once per section that cites it.
        var diskCache = new Dictionary<string, FileFingerprint?>(StringComparer.OrdinalIgnoreCase);

        foreach (var section in sections)
        {
            var files = section.SourceLocations
                .Select(StripLine)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var changed = ImmutableArray.CreateBuilder<FileDelta>();
            foreach (var relFile in files)
            {
                var absolute = ToAbsolute(relFile);
                if (!_snapshot.FileFingerprints.TryGetValue(absolute, out var analyzed))
                {
                    changed.Add(new FileDelta(relFile, "unknown", 0));
                    continue;
                }

                if (!diskCache.TryGetValue(absolute, out var now))
                    diskCache[absolute] = now = FileFingerprinter.Read(absolute);

                if (now is null)
                    changed.Add(new FileDelta(relFile, "deleted", -analyzed.LineCount));
                else if (!string.Equals(now.Value.Sha256, analyzed.Sha256, StringComparison.Ordinal))
                    changed.Add(new FileDelta(relFile, "modified", now.Value.LineCount - analyzed.LineCount));
            }

            // "unknown" is honesty, not evidence of drift — only real change flips stale.
            var stale = changed.Any(c => c.Status is "modified" or "deleted");
            results.Add(new SectionVerification(section.Section, stale, files.Count, changed.ToImmutable()));
        }

        return results.ToImmutable();
    }

    /// <summary>SourceLocations are repo-relative `file:line` — drop the trailing line suffix.</summary>
    private static string StripLine(string location)
    {
        var colon = location.LastIndexOf(':');
        return colon > 0 && colon < location.Length - 1 && location[(colon + 1)..].All(char.IsDigit)
            ? location[..colon]
            : location;
    }

    private string ToAbsolute(string repoRelative)
    {
        var root = _snapshot.RootPath;
        if (string.IsNullOrEmpty(root)) return repoRelative;
        return Path.IsPathRooted(repoRelative.Replace('/', Path.DirectorySeparatorChar))
            ? repoRelative
            : Path.GetFullPath(Path.Combine(root, repoRelative));
    }
}
