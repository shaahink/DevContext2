namespace DevContext.Core.Graph;

/// <summary>A resolved symbol reference: the type (and optional member) a call/expression lands on.</summary>
public readonly record struct SymbolRef(string TypeId, string? Member, Resolution How);

/// <summary>Minimal, Roslyn-free context for resolving a call receiver, so it can be built syntactically or semantically.</summary>
public sealed record SymbolContext(
    string CallerType,
    string ReceiverExpression,
    string MemberName,
    IReadOnlyDictionary<string, string> FieldTypes);

/// <summary>
/// Abstracts callee/implementation resolution so the graph layer is independent of HOW symbols are
/// resolved. P1/P2 ship <see cref="SyntacticSymbolResolver"/>; P3 adds a Roslyn SemanticModel
/// implementation (in DevContext.Roslyn) WITHOUT touching GraphBuilder. This single seam is what makes
/// the "go semantic later" decision a strategy swap rather than a rewrite. See TRACE-ENGINE-DESIGN.md §3.
/// </summary>
public interface ISymbolResolver
{
    /// <summary>Resolves the concrete implementation for an interface/abstract type (DI-aware). Null if unknown/ambiguous.</summary>
    SymbolRef? ResolveImplementation(string interfaceOrAbstractType);

    /// <summary>Resolves the type a receiver expression refers to (field/local/property/this). Null if unknown.</summary>
    SymbolRef? ResolveReceiverType(SymbolContext context);
}

/// <summary>
/// Heuristic resolver (P1/P2). Deliberately conservative — returns null rather than guess, so edges it
/// can't justify are left out instead of being wrong. The agent ports CallGraphExtractor's existing
/// field-map / DI-map heuristics into here in P2; P3 replaces the whole class with the semantic resolver.
/// </summary>
public sealed class SyntacticSymbolResolver : ISymbolResolver
{
    private readonly IReadOnlyDictionary<string, string> _diMap;       // interface -> impl (short names)
    private readonly IReadOnlyDictionary<string, string> _singleImpl;  // interface -> sole implementor

    /// <summary>Creates a resolver from a DI map (registered impls) and a single-implementor fallback map.</summary>
    public SyntacticSymbolResolver(
        IReadOnlyDictionary<string, string>? diMap = null,
        IReadOnlyDictionary<string, string>? singleImpl = null)
    {
        _diMap = diMap ?? new Dictionary<string, string>(StringComparer.Ordinal);
        _singleImpl = singleImpl ?? new Dictionary<string, string>(StringComparer.Ordinal);
    }

    /// <inheritdoc/>
    public SymbolRef? ResolveImplementation(string interfaceOrAbstractType)
    {
        if (_diMap.TryGetValue(interfaceOrAbstractType, out var impl))
            return new SymbolRef(impl, null, Resolution.Join);
        if (_singleImpl.TryGetValue(interfaceOrAbstractType, out var sole))
            return new SymbolRef(sole, null, Resolution.Syntactic);
        return null;
    }

    /// <inheritdoc/>
    public SymbolRef? ResolveReceiverType(SymbolContext context)
    {
        // TODO(agent, P2): port the field/property/ctor-param map heuristic from CallGraphExtractor.ResolveType,
        // including the DI/interface→impl follow-through. Keep it conservative: prefer null over a wrong type.
        if (context.FieldTypes.TryGetValue(context.ReceiverExpression, out var t))
            return new SymbolRef(t, null, Resolution.Syntactic);
        return null;
    }
}
