using DevContext.Core.Graph;

namespace DevContext.Core.Graph2.Seams;

/// <summary>A seam discovered in a member body: an <see cref="Origin"/> member that reaches
/// <see cref="Target"/> via an edge of kind <see cref="Kind"/>. Detectors emit these; they NEVER write
/// to the graph. The assembler (L2.3) resolves <see cref="Target"/> via the SymbolTable and materialises
/// edges — so ambiguous targets can be skipped by Law R1 rather than silently first-matched.</summary>
public sealed record SeamMatch(
    SymbolId Origin,
    EdgeKind Kind,
    SymbolRef Target,
    float Confidence,
    string Provenance,
    string DetectorId);

/// <summary>Read-only context a detector may consult: the resolver plus classification sets derived from
/// declared types (which short names are entities/stores, integration events, domain events). Keeps
/// detectors pure — facts + context in, seams out — with no access to mutable pipeline state.</summary>
public sealed record SeamContext
{
    public SymbolTable? Symbols { get; init; }
    public ImmutableHashSet<string> KnownEntities { get; init; } = ImmutableHashSet<string>.Empty;
    public ImmutableHashSet<string> IntegrationEventTypes { get; init; } = ImmutableHashSet<string>.Empty;
    public ImmutableHashSet<string> DomainEventTypes { get; init; } = ImmutableHashSet<string>.Empty;

    public static readonly SeamContext Empty = new();
}

/// <summary>Detects seams over <see cref="BodyFacts"/>. Implementations are small (one framework family
/// each), pure, and unit-testable against real source snippets. Design §2.1.</summary>
public interface ISeamDetector
{
    /// <summary>Stable id for provenance/telemetry (matches the emitted <see cref="SeamMatch.DetectorId"/>).</summary>
    string Id { get; }

    /// <summary>Emits matches for one member body. Never mutates the graph.</summary>
    IEnumerable<SeamMatch> Detect(BodyFacts body, SeamContext ctx);
}

/// <summary>Shared helpers for target resolution across detectors.</summary>
internal static class SeamDetectorHelpers
{
    /// <summary>Resolves the <see cref="SymbolRef"/> for an invocation argument by correlating its text
    /// with a local declaration in the same body (the E1 pattern: <c>var command = request.Adapt&lt;T&gt;()</c>
    /// then <c>sender.Send(command)</c>). A semantically-upgraded local type (Tier B, Law R2) wins over the
    /// arg's syntactic type; otherwise the arg's own known type is used, then the local's syntactic type.
    /// No text scanning.</summary>
    public static SymbolRef? ResolveArgTarget(ArgFact arg, BodyFacts body)
    {
        SymbolRef? localRef = null;
        foreach (var op in body.Ops)
        {
            if (op is LocalDeclOp local && string.Equals(local.Name, arg.Text, StringComparison.Ordinal))
            {
                localRef = local.InferredFrom ?? local.DeclaredType;
                break;
            }
        }

        // A real semantic bind is authoritative — carry its tier so the assembler emits a verified edge.
        if (localRef is { Tier: ResolutionTier.Semantic }) return localRef;
        if (arg.Type is not null) return arg.Type;
        return localRef;
    }

    /// <summary>Applies the SymbolTable resolver to a raw target ref when one is available.</summary>
    public static SymbolRef Resolve(SymbolRef target, SeamContext ctx)
        => ctx.Symbols is null ? target : ctx.Symbols.Resolve(target);
}
