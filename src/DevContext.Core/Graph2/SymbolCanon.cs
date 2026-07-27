using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Graph2;

/// <summary>The one canonical name algebra for type and member identity (Batch A / R2 §2.A step 1).
/// Every producer of a type id — <c>SyntaxStructureExtractor</c> (TypeDiscovery.Id),
/// <c>BodyFactExtractor</c> (BodyFacts member prefix), <c>SourceBodyExtractor</c>, and the semantic
/// side (<see cref="INamedTypeSymbol"/> canonicalisation) — goes through here, so identity is
/// structural by construction: nested-type chains are preserved and generic arity is part of the key
/// (<c>Ns.Outer.Inner`2</c>), never erased. Short display names stay bare (<c>TypeDiscovery.Name</c>,
/// node titles); the arity marker lives only in canonical ids, which every surface treats as opaque.</summary>
public static class SymbolCanon
{
    /// <summary>Canonical id for a declared type: namespace + nested-type chain + name, each generic
    /// segment carrying its <c>`N</c> arity. Global-namespace types get the <c>global</c> prefix
    /// (the TypeDiscovery.Id convention — BodyFacts member ids must not diverge from it).</summary>
    public static string ForTypeDecl(TypeDeclarationSyntax typeDecl)
    {
        var ns = typeDecl.Ancestors().OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault()?.Name.ToString() ?? "global";

        var sb = new System.Text.StringBuilder(ns);
        foreach (var outer in typeDecl.Ancestors().OfType<TypeDeclarationSyntax>().Reverse())
            sb.Append('.').Append(Segment(outer));
        sb.Append('.').Append(Segment(typeDecl));
        return sb.ToString();

        static string Segment(TypeDeclarationSyntax t)
        {
            var arity = t.TypeParameterList?.Parameters.Count ?? 0;
            return arity > 0 ? $"{t.Identifier.ValueText}`{arity}" : t.Identifier.ValueText;
        }
    }

    /// <summary>Canonical id for a bound symbol — the semantic-side mirror of
    /// <see cref="ForTypeDecl"/>. Constructed generics canonicalise to their open definition
    /// (<c>IdentifiedCommand&lt;CreateOrderCommand, bool&gt;</c> → <c>…IdentifiedCommand`2</c>) so a
    /// semantic bind lands on the same node id the declaration produced.</summary>
    public static string ForSymbol(INamedTypeSymbol type)
    {
        var def = type.OriginalDefinition;
        var segments = new List<string>();
        for (var cur = def; cur is not null; cur = cur.ContainingType)
            segments.Add(cur.Arity > 0 ? $"{cur.Name}`{cur.Arity}" : cur.Name);
        segments.Reverse();

        var ns = def.ContainingNamespace is { IsGlobalNamespace: false } n
            ? n.ToDisplayString()
            : "global";
        return ns + "." + string.Join(".", segments);
    }

    /// <summary>Splits a syntax-level type text into its base name and use-site generic arity:
    /// <c>IdentifiedCommand&lt;CreateOrderCommand, bool&gt;</c> → (<c>IdentifiedCommand</c>, 2);
    /// <c>IPipelineBehavior&lt;,&gt;</c> → (<c>IPipelineBehavior</c>, 2); <c>Foo</c> → (<c>Foo</c>, 0).
    /// Trailing <c>?</c> is ignored; malformed texts degrade to (trimmed text, 0).</summary>
    public static (string Base, int Arity) SplitGenericText(string text)
    {
        if (string.IsNullOrEmpty(text)) return (text, 0);
        var t = text.Trim();
        if (t.EndsWith('?')) t = t[..^1].TrimEnd();

        var lt = t.IndexOf('<');
        if (lt <= 0) return (t, 0);

        var depth = 0;
        var arity = 1;
        for (var i = lt; i < t.Length; i++)
        {
            switch (t[i])
            {
                case '<': depth++; break;
                case '>': depth--; break;
                case ',' when depth == 1: arity++; break;
            }
        }
        if (depth != 0) return (t[..lt].TrimEnd(), 0); // unbalanced — not a generic instantiation
        return (t[..lt].TrimEnd(), arity);
    }

    /// <summary>Declared generic arity encoded in a canonical id's last segment (<c>Ns.Foo`2</c> → 2;
    /// no marker → 0).</summary>
    public static int ArityOf(string canonical)
    {
        var tick = canonical.LastIndexOf('`');
        if (tick < 0 || tick == canonical.Length - 1) return 0;
        return int.TryParse(canonical.AsSpan(tick + 1), out var n) ? n : 0;
    }

    // ── Member keys ─────────────────────────────────────────────────────────────────────────────
    // Graph member identity is "{TypeCanonical}::{MemberName}" — the same structural separator as
    // BodyFacts SymbolIds, so ToMemberNodeId no longer converts identity away. The declared-arity
    // "(N)" suffix stays OUT of node keys: entry detections and focus strings only ever know a bare
    // member name, and a key half the producers cannot construct would split one method across two
    // disconnected nodes. "::" also makes lambda member keys parse correctly when the route segment
    // contains dots ("Type::<lambda> GET /api/v1.0/x" — the old last-dot split truncated these).

    private const string MemberSep = "::";

    /// <summary>Builds a canonical member key: <c>Ns.Type`1::Handle</c>.</summary>
    public static string MemberKey(string typeCanonical, string memberName)
        => typeCanonical + MemberSep + memberName;

    /// <summary>The owning-type part of a member key (<c>Ns.Type::M</c> → <c>Ns.Type</c>).
    /// A key without the member separator is returned whole (it IS a type key).</summary>
    public static string OwnerTypeOf(string memberKey)
    {
        var sep = memberKey.IndexOf(MemberSep, StringComparison.Ordinal);
        return sep > 0 ? memberKey[..sep] : memberKey;
    }

    /// <summary>The member-name part of a member key (<c>Ns.Type::M</c> → <c>M</c>); the whole key
    /// when no separator is present.</summary>
    public static string MemberNameOf(string memberKey)
    {
        var sep = memberKey.IndexOf(MemberSep, StringComparison.Ordinal);
        return sep >= 0 ? memberKey[(sep + MemberSep.Length)..] : memberKey;
    }

    /// <summary>Converts a BodyFacts member <see cref="SymbolId"/> canonical
    /// (<c>Type::Method(N)</c>) to the graph member key (<c>Type::Method</c>) — the one sanctioned
    /// lossy step (declared arity), applied in exactly one place.</summary>
    public static string MemberKeyFromSymbolId(string memberCanonical)
    {
        var sep = memberCanonical.IndexOf(MemberSep, StringComparison.Ordinal);
        if (sep < 0) return memberCanonical;
        var paren = memberCanonical.IndexOf('(', sep);
        return paren > 0 && memberCanonical.EndsWith(')') ? memberCanonical[..paren] : memberCanonical;
    }

    /// <summary>Bare display short name of a canonical id's last segment — nested chain and arity
    /// marker stripped (<c>Ns.Outer.Inner`2</c> → <c>Inner</c>). For member keys, pass the owner
    /// type through <see cref="OwnerTypeOf"/> first.</summary>
    public static string ShortNameOf(string canonical)
    {
        var end = canonical.LastIndexOf('`');
        var span = end > 0 ? canonical.AsSpan(0, end) : canonical.AsSpan();
        var dot = span.LastIndexOf('.');
        return (dot >= 0 ? span[(dot + 1)..] : span).ToString();
    }

    /// <summary>True when a canonical TYPE id matches a user/detection-supplied name: the bare short
    /// name (arity-blind), or a dotted suffix (<c>Ns.Sub.Type</c> matches <c>Sub.Type</c>). Use this
    /// instead of <c>EndsWith("." + name)</c>, which arity markers break for generic types.</summary>
    public static bool TypeIdMatches(string canonicalId, string? name,
        StringComparison comparison = StringComparison.Ordinal)
    {
        if (string.IsNullOrEmpty(name)) return false;
        if (string.Equals(canonicalId, name, comparison)) return true;

        var tick = canonicalId.LastIndexOf('`');
        var noArity = tick > 0 ? canonicalId[..tick] : canonicalId;
        if (string.Equals(noArity, name, comparison)) return true;
        return noArity.EndsWith("." + name, comparison);
    }
}
