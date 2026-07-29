namespace DevContext.Core.Graph2;

/// <summary>Structured facts about one method-like member's body, produced by a single syntax walk
/// (<see cref="BodyFactExtractor"/>). This is the "regex killer" from the Loom design (§2.1): seam
/// detectors read <see cref="BodyOp"/>s instead of scanning raw source text. Every op already knows its
/// enclosing <see cref="Member"/> and its line — so edges anchor by construction, never by char offset.</summary>
public sealed record BodyFacts(SymbolId Member, string MemberName, ImmutableArray<BodyOp> Ops)
{
    /// <summary>Declaring file of the member (for provenance).</summary>
    public string File { get; init; } = "";
    /// <summary>Owning project/service of the member.</summary>
    public string Project { get; init; } = "";
    /// <summary>1-based declaration line of the member (from the syntax walk — no re-parse). Lets the
    /// assembler stamp <c>file:line</c> on member nodes so packs never render a bare trailing colon (T2.2).</summary>
    public int DeclLine { get; init; }
}

/// <summary>One structured operation observed in a member body, at a known 1-based line.</summary>
public abstract record BodyOp(int Line);

/// <summary>A method invocation. <paramref name="ReceiverText"/> is the root identifier of the receiver
/// (e.g. <c>sender</c> in <c>sender.Send(cmd)</c>); <paramref name="ReceiverType"/> is its declared type
/// when resolvable from the member's local scope (fields, params, locals). An explicit <c>this.</c> is
/// NOT the root — <c>this.sender.Send(cmd)</c> reports <c>sender</c>, the same as the unqualified
/// spelling, because "this" resolves against no scope entry (D-3). A bare <c>this.Helper()</c> self-call
/// still reports <c>this</c>. Generic args and argument facts are captured verbatim — no regex, no
/// string-literal scanning.</summary>
public sealed record InvocationOp(
    int Line,
    string? ReceiverText,
    SymbolRef? ReceiverType,
    string MethodName,
    ImmutableArray<SymbolRef> GenericArgs,
    ImmutableArray<ArgFact> Args) : BodyOp(Line)
{
    /// <summary>The receiver's trailing member-access segment (<c>services.Mediator</c> → <c>Mediator</c>),
    /// or null when the receiver is a bare identifier (<see cref="ReceiverText"/> already names it) or absent.
    /// The root identifier alone hides a property-accessed sender, so dispatch detection consults this to
    /// recognise <c>services.Mediator.Send(cmd)</c> without resolving the container's cross-type members (T2.5).</summary>
    public string? ReceiverMember { get; init; }
}

/// <summary>An object creation: <c>new X(...)</c> or <c>new X { ... }</c>.</summary>
public sealed record CreationOp(int Line, SymbolRef Type) : BodyOp(Line);

/// <summary>A local declaration. <paramref name="InferredFrom"/> is the statically-obvious yielded type
/// of the initializer for exactly the cases the old regexes chased (<c>new X()</c>, <c>expr.Adapt&lt;X&gt;()</c>,
/// <c>expr.Map&lt;X&gt;()</c>, factory <c>Create&lt;X&gt;()</c>, awaited variants); null otherwise.</summary>
public sealed record LocalDeclOp(int Line, string Name, SymbolRef? DeclaredType, SymbolRef? InferredFrom) : BodyOp(Line);

/// <summary>A whole-word use of a simple identifier in the body (for data-touch matching).</summary>
public sealed record IdentifierUseOp(int Line, string Identifier) : BodyOp(Line);

/// <summary>One argument passed to an invocation. <paramref name="Text"/> is the root identifier or
/// expression text; <paramref name="Type"/> is filled when the argument's type is statically obvious.</summary>
public sealed record ArgFact(string Text, SymbolRef? Type = null);

/// <summary>Version tag for the content-keyed BodyFacts cache. Bump when the op shape changes so stale
/// cached facts are not reused across versions.</summary>
public static class BodyFactsVersion
{
    public const string Version = "facts-v1";
}
