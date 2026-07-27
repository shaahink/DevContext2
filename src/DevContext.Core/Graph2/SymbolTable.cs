using DevContext.Core.Models;

namespace DevContext.Core.Graph2;

public sealed record AmbiguityReport
{
    public int AmbiguousShortNameCount { get; init; }
    public ImmutableDictionary<string, int> CandidateCounts { get; init; }
        = ImmutableDictionary<string, int>.Empty;
}

/// <summary>The one symbol index of the graph world (Batch A: NameResolver collapsed into this).
/// Resolution is tiered and honest — Declared (exact canonical) → ProjectScoped → GlobalUnique →
/// Ambiguous/Unresolved — and a use-site generic arity (<see cref="SymbolRef.Arity"/> or a
/// <c>Name&lt;T, R&gt;</c> text) narrows candidates structurally instead of being erased. Ambiguity is
/// REPORTED, never silently first-matched (Law R1); the name-string API
/// (<see cref="ResolveName"/>) returns its input unchanged when it cannot resolve honestly.</summary>
public sealed class SymbolTable
{
    private readonly HashSet<string> _fqns;
    private readonly Dictionary<string, List<string>> _byShort;
    private readonly Dictionary<(string ShortName, string Project), List<string>> _byProject;
    // Member canonicals from BodyFacts live in their OWN index: a class's constructor shares the
    // class's short name, so mixing members into the type index would make every explicitly-
    // constructed type "ambiguous" (the S2 targets-blank regression). Types always win; the member
    // tier is consulted only when no type candidate exists.
    private readonly Dictionary<string, List<string>> _membersByShort;
    private readonly Dictionary<(string ShortName, string Project), List<string>> _membersByProject;
    private readonly Func<string, string?>? _fileToProject;
    private readonly Dictionary<string, string> _namespaceByFqn;
    private readonly Dictionary<string, int> _candidateCounts;
    private readonly Dictionary<string, HashSet<string>> _memberNamesByType;
    private readonly HashSet<string> _interfaceFqns;

    public SymbolTable(
        IEnumerable<TypeDiscovery> types,
        Func<string, string?>? fileToProject = null,
        IEnumerable<BodyFacts>? bodyFacts = null)
    {
        _fileToProject = fileToProject;
        _fqns = new HashSet<string>(StringComparer.Ordinal);
        _byShort = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        _byProject = new Dictionary<(string, string), List<string>>(new ByProjectComparer());
        _membersByShort = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        _membersByProject = new Dictionary<(string, string), List<string>>(new ByProjectComparer());
        _namespaceByFqn = new Dictionary<string, string>(StringComparer.Ordinal);
        _candidateCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        _memberNamesByType = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        _interfaceFqns = new HashSet<string>(StringComparer.Ordinal);

        if (types is not null)
        {
            // D5.3 determinism — the types source is typically ConcurrentDictionary.Values
            // (per-process-randomized order), so sort by FQN to keep short-name candidate lists
            // stable run-to-run.
            foreach (var t in types.OrderBy(t => t.Id, StringComparer.Ordinal))
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

                if (t.Kind == Models.TypeKind.Interface) _interfaceFqns.Add(t.Id);

                if (!t.Methods.IsDefaultOrEmpty)
                {
                    if (!_memberNamesByType.TryGetValue(t.Id, out var members))
                        _memberNamesByType[t.Id] = members = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var m in t.Methods) members.Add(m.Name);
                }
            }
        }

        if (bodyFacts is not null)
        {
            foreach (var body in bodyFacts)
            {
                var canonical = body.Member.Canonical;
                _fqns.Add(canonical);

                if (!_membersByShort.TryGetValue(body.MemberName, out var list))
                    _membersByShort[body.MemberName] = list = [];
                if (!list.Contains(canonical)) list.Add(canonical);

                var project = body.Project ?? fileToProject?.Invoke(body.File ?? "") ?? "";
                var key = (body.MemberName, project);
                if (!_membersByProject.TryGetValue(key, out var plist))
                    _membersByProject[key] = plist = [];
                if (!plist.Contains(canonical)) plist.Add(canonical);
            }
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
        ShortCandidates(shortName) is { Count: > 1 };

    /// <summary>Type candidates first; the member tier only when NO type carries the short name.</summary>
    private List<string>? ShortCandidates(string name)
        => _byShort.TryGetValue(name, out var t) && t.Count > 0 ? t
         : _membersByShort.TryGetValue(name, out var m) && m.Count > 0 ? m
         : null;

    private List<string>? ProjectCandidates(string name, string project)
        => _byProject.TryGetValue((name, project), out var t) && t.Count > 0 ? t
         : _membersByProject.TryGetValue((name, project), out var m) && m.Count > 0 ? m
         : null;

    /// <summary>True when the canonical id names a declared interface (used for honest DI routing).</summary>
    public bool IsInterface(string fqn) => _interfaceFqns.Contains(fqn);

    /// <summary>True when the type declares a member with this name (declared, not inherited).
    /// The self-call gate: a bare-identifier invocation joins the caller type only when the type
    /// actually declares the method — inherited framework helpers (<c>Ok()</c>) and pseudo-calls
    /// (<c>nameof</c>) fail this structurally, which is what retired IsSelfCallNoise.</summary>
    public bool TypeDeclaresMember(string typeFqn, string memberName)
        => _memberNamesByType.TryGetValue(typeFqn, out var members) && members.Contains(memberName);

    /// <summary>Narrows short-name candidates by use-site generic arity: an explicit arity keeps only
    /// structurally-matching declarations; a bare mention (arity 0) prefers non-generic declarations
    /// but falls back to all candidates (a stripped mention of a generic type).</summary>
    private static List<string> Narrow(List<string> candidates, int arity)
    {
        if (candidates.Count <= 1)
            return arity > 0 && candidates.Count == 1 && SymbolCanon.ArityOf(candidates[0]) != arity
                ? []
                : candidates;

        List<string>? matches = null;
        foreach (var f in candidates)
            if (SymbolCanon.ArityOf(f) == arity)
                (matches ??= []).Add(f);

        if (arity > 0) return matches ?? [];
        return matches is { Count: > 0 } ? matches : candidates;
    }

    public SymbolRef Resolve(SymbolRef r)
    {
        if (string.IsNullOrEmpty(r.Text))
            return r with { Tier = ResolutionTier.Unresolved };

        // Law R2 — tier is monotone: a ref that already carries a real semantic bind is never downgraded
        // or re-ambiguated. Map it onto the in-scope node id when the short name is uniquely known (so the
        // graph node identity matches the syntactic path — no drift), otherwise keep the bound id. Either
        // way the Semantic tier survives, which the assembler turns into a verified edge.
        if (r.Tier == ResolutionTier.Semantic && r.Resolved is { } bound)
        {
            if (_fqns.Contains(bound.Canonical))
                return r;
            if (ShortCandidates(r.Text) is { } inScopeAll
                && Narrow(inScopeAll, r.Arity) is { Count: 1 } inScope)
                return r with { Resolved = new SymbolId(KindFromCanonical(inScope[0]), inScope[0]) };
            return r;
        }

        if (_fqns.Contains(r.Text))
            return r with { Resolved = new SymbolId(KindFromCanonical(r.Text), r.Text), Tier = ResolutionTier.Declared };

        var project = _fileToProject?.Invoke(r.Site.File) ?? r.Site.Project;
        if (!string.IsNullOrEmpty(project))
        {
            if (ProjectCandidates(r.Text, project) is { } sameProjectAll)
            {
                var sameProject = Narrow(sameProjectAll, r.Arity);
                if (sameProject.Count == 1)
                    return r with
                    {
                        Resolved = new SymbolId(KindFromCanonical(sameProject[0]), sameProject[0]),
                        Tier = ResolutionTier.ProjectScoped,
                    };
                if (sameProject.Count > 1)
                {
                    var ids = sameProject.Select(
                        fqn => new SymbolId(KindFromCanonical(fqn), fqn)).ToImmutableArray();
                    return r with
                    {
                        Candidates = ids,
                        Tier = ResolutionTier.Ambiguous,
                    };
                }
            }
        }

        if (ShortCandidates(r.Text) is not { } globalAll)
            return r with { Tier = ResolutionTier.Unresolved };

        var narrowed = Narrow(globalAll, r.Arity);
        if (narrowed.Count == 0)
            return r with { Tier = ResolutionTier.Unresolved };

        if (narrowed.Count == 1)
        {
            var sole = narrowed[0];
            return r with
            {
                Resolved = new SymbolId(KindFromCanonical(sole), sole),
                Tier = ResolutionTier.GlobalUnique,
            };
        }

        var candidates = narrowed.Select(
            fqn => new SymbolId(KindFromCanonical(fqn), fqn)).ToImmutableArray();
        return r with
        {
            Candidates = candidates,
            Tier = ResolutionTier.Ambiguous,
        };
    }

    /// <summary>Name-string resolution (the collapsed NameResolver API): resolves a possibly-short,
    /// possibly-generic type text (<c>IdentifiedCommand&lt;T, R&gt;</c>) to its canonical id. Already-
    /// canonical ids pass through; project scope (via <paramref name="containingFile"/>) narrows first.
    /// Returns the INPUT UNCHANGED when unknown or ambiguous — never a silent first-match (the DC2
    /// last-write-wins defect this batch removes). Callers may keep the returned text as an opaque
    /// leaf node key exactly as before.</summary>
    public string ResolveName(string name, string? containingFile = null)
    {
        if (string.IsNullOrEmpty(name)) return name;
        if (_fqns.Contains(name)) return name;

        var (baseName, arity) = SymbolCanon.SplitGenericText(name);
        if (arity > 0 && _fqns.Contains($"{baseName}`{arity}"))
            return $"{baseName}`{arity}"; // qualified generic text → canonical (Ns.Foo<T> → Ns.Foo`1)

        if (!_byShort.TryGetValue(baseName, out var globalAll) || globalAll.Count == 0)
            return name;

        if (containingFile is not null && _fileToProject is not null)
        {
            var project = _fileToProject(containingFile);
            if (!string.IsNullOrEmpty(project)
                && _byProject.TryGetValue((baseName, project), out var sameProjectAll)
                && Narrow(sameProjectAll, arity) is { Count: 1 } sameProject)
                return sameProject[0];
        }

        var narrowed = Narrow(globalAll, arity);
        return narrowed.Count == 1 ? narrowed[0] : name;
    }

    public string? ProjectForFile(string filePath) =>
        _fileToProject?.Invoke(filePath);

    /// <summary>Resolves <paramref name="name"/> (short or canonical) to its namespace. Unknown
    /// DOTTED names fall back to the dot-slice (an external FQN still yields a namespace-ish prefix
    /// for grouping); an unknown short name comes back unchanged.</summary>
    public string GetNamespace(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var fqn = ResolveName(name);
        if (_namespaceByFqn.TryGetValue(fqn, out var ns))
            return ns;
        var dot = fqn.LastIndexOf('.');
        return dot > 0 ? fqn[..dot] : name;
    }

    private static SymbolKind KindFromCanonical(string canonical)
        => canonical.Contains("::") ? SymbolKind.Member : SymbolKind.Type;
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
