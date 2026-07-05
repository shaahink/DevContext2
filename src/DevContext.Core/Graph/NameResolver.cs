namespace DevContext.Core.Graph;

/// <summary>
/// Resolves a possibly-short type name (as carried by detections, e.g.
/// <c>MediatRHandlerDetection.HandlerType</c> = "CreateOrderCommandHandler") to a canonical
/// fully-qualified name matching <c>TypeDiscovery.Id</c> — the backbone of every graph join.
/// Detections stay short-name (that's all syntax gives without semantics); this index does the FQN
/// resolution once, so <see cref="NodeId"/>.Key is always canonical.
/// </summary>
public sealed class NameResolver
{
    private readonly HashSet<string> _fqns;
    private readonly Dictionary<string, List<string>> _byShort;
    private readonly Dictionary<(string ShortName, string Project), List<string>>? _byProject;
    private readonly Func<string, string?>? _fileToProject;
    private readonly Dictionary<string, string> _namespaceByFqn;

    /// <summary>Indexes the discovered types by FQN and short name. When <paramref name="fileToProject"/>
    /// is provided, also builds a project-scoped index so same-name types in different projects don't collide
    /// (W5 / M1.4).</summary>
    public NameResolver(IEnumerable<TypeDiscovery> types, Func<string, string?>? fileToProject = null)
    {
        _fileToProject = fileToProject;
        _fqns = new HashSet<string>(StringComparer.Ordinal);
        _byShort = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        _byProject = fileToProject != null
            ? new Dictionary<(string, string), List<string>>()
            : null;
        _namespaceByFqn = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var t in types)
        {
            _fqns.Add(t.Id);
            if (!_byShort.TryGetValue(t.Name, out var list))
                _byShort[t.Name] = list = [];
            if (!list.Contains(t.Id)) list.Add(t.Id);

            if (_byProject is not null && fileToProject is not null)
            {
                var project = fileToProject(t.FilePath) ?? "";
                var key = (t.Name, project);
                if (!_byProject.TryGetValue(key, out var plist))
                    _byProject[key] = plist = [];
                if (!plist.Contains(t.Id)) plist.Add(t.Id);
            }

            _namespaceByFqn[t.Id] = t.Namespace;
        }
    }

    /// <summary>True when more than one FQN shares the given short name (a join hazard).</summary>
    public bool IsAmbiguous(string shortName)
        => _byShort.TryGetValue(shortName, out var l) && l.Count > 1;

    /// <summary>
    /// Resolves <paramref name="name"/> to a canonical FQN. Already-FQN names pass through.
    /// When <paramref name="containingFile"/> is provided, prefers types in the same project
    /// (W5 / M1.4). On a short-name collision, prefers the FQN under <paramref name="namespaceHint"/>.
    /// Returns the input unchanged when unknown (external/framework type) — callers may keep it as an
    /// opaque leaf node.
    /// </summary>
    public string Resolve(string name, string? containingFile = null, string? namespaceHint = null)
    {
        if (string.IsNullOrEmpty(name)) return name;
        if (_fqns.Contains(name)) return name;

        // Project-scoped lookup (W5 / M1.4): prefer same-project types when file context is available
        if (containingFile is not null && _byProject is not null && _fileToProject is not null)
        {
            var project = _fileToProject(containingFile);
            if (!string.IsNullOrEmpty(project))
            {
                var key = (name, project);
                if (_byProject.TryGetValue(key, out var sameProject) && sameProject.Count == 1)
                    return sameProject[0];
                if (sameProject is { Count: > 1 })
                {
                    if (!string.IsNullOrEmpty(namespaceHint))
                    {
                        var best = sameProject.FirstOrDefault(
                            f => f.StartsWith(namespaceHint + ".", StringComparison.Ordinal));
                        if (best is not null) return best;
                    }
                    return sameProject[0];
                }
            }
        }

        // Global resolution fallback
        if (!_byShort.TryGetValue(name, out var fqns) || fqns.Count == 0) return name;
        if (fqns.Count == 1) return fqns[0];

        if (!string.IsNullOrEmpty(namespaceHint))
        {
            var best = fqns.FirstOrDefault(f => f.StartsWith(namespaceHint + ".", StringComparison.Ordinal));
            if (best is not null) return best;
        }
        return fqns[0];
    }

    /// <summary>
    /// Resolves <paramref name="name"/> (short or FQN) to its namespace. Returns the input unchanged
    /// when unknown (external/framework type).</summary>
    public string GetNamespace(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var fqn = Resolve(name);
        return _namespaceByFqn.TryGetValue(fqn, out var ns) ? ns : name;
    }
}
