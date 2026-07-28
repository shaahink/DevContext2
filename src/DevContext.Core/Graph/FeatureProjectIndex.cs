namespace DevContext.Core.Graph;

/// <summary>
/// Batch D (R2 §2.D) — "which project's directory contains this file, and what namespace prefix does its
/// name imply" answered without rescanning the project list per type.
/// <para>The scan it replaces ran inside <c>DeriveFeature</c> for every type node:
/// <c>model.Projects.FirstOrDefault(p =&gt; fp.StartsWith(Path.GetDirectoryName(p.FilePath)))</c>. That is
/// O(types x projects) with a fresh <c>GetDirectoryName</c> allocation per comparison — invisible on a
/// fixture, minutes of string work on a framework-scale repo (the S3 lesson, in the shape the audit
/// called DC10).</para>
/// <para>FIRST-MATCH-WINS in <see cref="DiscoveryModel.Projects"/> order is preserved exactly: the
/// directories are pre-computed in that order and probed in that order. The per-file memo is what makes
/// it cheap in practice — a project has far more types than files.</para>
/// </summary>
public sealed class FeatureProjectIndex
{
    private readonly (string Dir, string Prefix)[] _projects;
    private readonly Dictionary<string, string?> _byFile = new(StringComparer.OrdinalIgnoreCase);

    private FeatureProjectIndex((string Dir, string Prefix)[] projects) => _projects = projects;

    /// <summary>Builds the index over a model's projects, in model order.</summary>
    public static FeatureProjectIndex Build(DiscoveryModel model)
    {
        var rows = new List<(string, string)>(model.Projects.Length);
        foreach (var p in model.Projects)
        {
            if (p.FilePath is not { } pp) continue;
            // QUIRK PRESERVED, deliberately: a project path with no directory part (a bare
            // "MyApp.csproj") yields "", and `file.StartsWith("")` is true for EVERY file — so such a
            // project owns the whole tree. That is the pre-Batch-D behaviour to the letter. It is almost
            // certainly wrong, but Batch D is hygiene: nothing moves. Recorded as a found-not-fixed
            // defect instead of being quietly repaired inside a perf change.
            rows.Add((Path.GetDirectoryName(pp) ?? "", p.Name.Replace("-", "").Replace("_", "")));
        }
        return new FeatureProjectIndex([.. rows]);
    }

    /// <summary>The owning project's name-derived namespace prefix for a file, or null when no project
    /// directory contains it.</summary>
    public string? NamespacePrefixFor(string filePath)
    {
        if (_byFile.TryGetValue(filePath, out var cached)) return cached;
        string? prefix = null;
        foreach (var (dir, p) in _projects)
        {
            if (filePath.StartsWith(dir, StringComparison.OrdinalIgnoreCase)) { prefix = p; break; }
        }
        _byFile[filePath] = prefix;
        return prefix;
    }
}
