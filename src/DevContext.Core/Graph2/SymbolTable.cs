namespace DevContext.Core.Graph2;

public sealed class AmbiguityReport
{
    public int AmbiguousShortNameCount { get; init; }
    public ImmutableDictionary<string, int> CandidateCounts { get; init; }
        = ImmutableDictionary<string, int>.Empty;
}

public sealed class SymbolTable
{
    private readonly HashSet<string> _fqns;
    private readonly Dictionary<string, List<string>> _byShort;
    private readonly Dictionary<(string ShortName, string Project), List<string>> _byProject;
    private readonly Func<string, string?>? _fileToProject;
    private readonly Dictionary<string, string> _namespaceByFqn;
    private readonly Dictionary<string, int> _candidateCounts;

    public SymbolTable(
        IEnumerable<TypeDiscovery> types,
        Func<string, string?>? fileToProject = null)
    {
        _fileToProject = fileToProject;
        _fqns = new HashSet<string>(StringComparer.Ordinal);
        _byShort = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        _byProject = new Dictionary<(string, string), List<string>>(new ByProjectComparer());
        _namespaceByFqn = new Dictionary<string, string>(StringComparer.Ordinal);
        _candidateCounts = new Dictionary<string, int>(StringComparer.Ordinal);

        if (types is null) return;

        foreach (var t in types)
        {
            _fqns.Add(t.Id);
            if (!_byShort.TryGetValue(t.Name, out var list))
                _byShort[t.Name] = list = [];
            if (!list.Contains(t.Id)) list.Add(t.Id);

            var proj = fileToProject?.Invoke(t.FilePath) ?? "";
            var key = (t.Name, proj);
            if (!_byProject.TryGetValue(key, out var plist))
                _byProject[key] = plist = [];
            if (!plist.Contains(t.Id)) plist.Add(t.Id);

            _namespaceByFqn[t.Id] = t.Namespace;
        }

        foreach (var (shortName, fqns) in _byShort)
            if (fqns.Count > 1)
                _candidateCounts[shortName] = fqns.Count;
    }

    public AmbiguityReport AmbiguityReport => new()
    {
        AmbiguousShortNameCount = _candidateCounts.Count,
        CandidateCounts = _candidateCounts.ToImmutableDictionary(),
    };

    public bool IsKnownFqn(string fqn) => _fqns.Contains(fqn);

    public bool IsAmbiguous(string shortName) =>
        _byShort.TryGetValue(shortName, out var l) && l.Count > 1;

    public SymbolRef Resolve(SymbolRef r)
    {
        if (string.IsNullOrEmpty(r.Text))
            return r with { Tier = ResolutionTier.Unresolved };

        if (_fqns.Contains(r.Text))
            return r with { Resolved = new SymbolId(SymbolKind.Type, r.Text), Tier = ResolutionTier.Declared };

        var project = _fileToProject?.Invoke(r.Site.File) ?? r.Site.Project;
        if (!string.IsNullOrEmpty(project))
        {
            var key = (r.Text, project);
            if (_byProject.TryGetValue(key, out var sameProject) && sameProject.Count == 1)
                return r with
                {
                    Resolved = new SymbolId(SymbolKind.Type, sameProject[0]),
                    Tier = ResolutionTier.ProjectScoped,
                };
            if (sameProject is { Count: > 1 })
            {
                var ids = sameProject.Select(
                    fqn => new SymbolId(SymbolKind.Type, fqn)).ToImmutableArray();
                return r with
                {
                    Candidates = ids,
                    Tier = ResolutionTier.Ambiguous,
                };
            }
        }

        if (!_byShort.TryGetValue(r.Text, out var fqns) || fqns.Count == 0)
            return r with { Tier = ResolutionTier.Unresolved };

        if (fqns.Count == 1)
        {
            var first = fqns.First();
            return r with
            {
                Resolved = new SymbolId(SymbolKind.Type, first),
                Tier = ResolutionTier.GlobalUnique,
            };
        }

        var candidates = fqns.Select(
            fqn => new SymbolId(SymbolKind.Type, fqn)).ToImmutableArray();
        return r with
        {
            Candidates = candidates,
            Tier = ResolutionTier.Ambiguous,
        };
    }

    public string? ProjectForFile(string filePath) =>
        _fileToProject?.Invoke(filePath);

    public string GetNamespace(string fqn)
    {
        if (_namespaceByFqn.TryGetValue(fqn, out var ns))
            return ns;
        var dot = fqn.LastIndexOf('.');
        return dot > 0 ? fqn[..dot] : fqn;
    }
}

internal sealed class ByProjectComparer : IEqualityComparer<(string ShortName, string Project)>
{
    public bool Equals((string ShortName, string Project) x, (string ShortName, string Project) y)
        => string.Equals(x.ShortName, y.ShortName, StringComparison.Ordinal)
        && string.Equals(x.Project, y.Project, StringComparison.Ordinal);

    public int GetHashCode((string ShortName, string Project) obj)
    {
        var hc = new HashCode();
        hc.Add(obj.ShortName, StringComparer.Ordinal);
        hc.Add(obj.Project, StringComparer.Ordinal);
        return hc.ToHashCode();
    }
}
