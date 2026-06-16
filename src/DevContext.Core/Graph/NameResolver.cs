namespace DevContext.Core.Graph;

/// <summary>
/// Resolves a possibly-short type name (as carried by detections, e.g.
/// <c>MediatRHandlerDetection.HandlerType</c> = "CreateOrderCommandHandler") to a canonical
/// fully-qualified name matching <c>TypeDiscovery.Id</c> — the backbone of every graph join.
/// Detections stay short-name (that's all syntax gives without semantics); this index does the FQN
/// resolution once, so <see cref="NodeId"/>.Key is always canonical. This resolves design-doc Q2:
/// keep detections unchanged (no breaking change), join via a resolver.
/// </summary>
public sealed class NameResolver
{
    private readonly HashSet<string> _fqns;
    private readonly Dictionary<string, List<string>> _byShort;

    /// <summary>Indexes the discovered types by FQN and short name.</summary>
    public NameResolver(IEnumerable<TypeDiscovery> types)
    {
        _fqns = new HashSet<string>(StringComparer.Ordinal);
        _byShort = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var t in types)
        {
            _fqns.Add(t.Id);
            if (!_byShort.TryGetValue(t.Name, out var list))
                _byShort[t.Name] = list = [];
            if (!list.Contains(t.Id)) list.Add(t.Id);
        }
    }

    /// <summary>True when more than one FQN shares the given short name (a join hazard).</summary>
    public bool IsAmbiguous(string shortName)
        => _byShort.TryGetValue(shortName, out var l) && l.Count > 1;

    /// <summary>
    /// Resolves <paramref name="name"/> to a canonical FQN. Already-FQN names pass through. On a
    /// short-name collision, prefers the FQN under <paramref name="namespaceHint"/>. Returns the input
    /// unchanged when unknown (external/framework type) — callers may keep it as an opaque leaf node.
    /// </summary>
    public string Resolve(string name, string? namespaceHint = null)
    {
        if (string.IsNullOrEmpty(name)) return name;
        if (_fqns.Contains(name)) return name;
        if (!_byShort.TryGetValue(name, out var fqns) || fqns.Count == 0) return name;
        if (fqns.Count == 1) return fqns[0];

        if (!string.IsNullOrEmpty(namespaceHint))
        {
            var best = fqns.FirstOrDefault(f => f.StartsWith(namespaceHint + ".", StringComparison.Ordinal));
            if (best is not null) return best;
        }
        // TODO(agent, P2): disambiguate by implemented interface (e.g. the IRequestHandler<T> impl) or file path.
        return fqns[0];
    }
}
