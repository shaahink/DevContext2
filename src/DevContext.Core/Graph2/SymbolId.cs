namespace DevContext.Core.Graph2;

public readonly record struct SymbolId(SymbolKind Kind, string Canonical)
{
    public override string ToString() => $"{Kind}:{Canonical}";
}
