namespace DevContext.Core.Graph;

/// <summary>
/// The three tiers an edge (or a flow/trace step) can be resolved at. Exhaustive and mutually
/// exclusive over <see cref="Resolution"/> — every edge is in exactly one.
/// </summary>
public enum EdgeTier
{
    /// <summary>A Roslyn <c>SemanticModel</c> resolved the symbol. This is what "verified" means.</summary>
    Verified,
    /// <summary>Derived by joining two existing detections. Trustworthy-ish, but nothing checked the
    /// symbol — and this is also <see cref="Resolution"/>'s DEFAULT, so an edge nobody labelled
    /// lands here. Never "verified".</summary>
    Joined,
    /// <summary>Resolved by syntax/string heuristics. Rendered "[approx]".</summary>
    Approximate,
}

/// <summary>
/// V1.1 (backlog #25) — THE one definition of what "verified" means for an edge. Every surface that
/// says the word reads it from here; nothing computes a tier for itself.
///
/// <para>Before this existed the engine shipped two answers on the same page. <c>GraphStats</c>/
/// <c>SeamStat</c> counted only <see cref="Resolution.Syntactic"/> as approximate, so the MCP
/// <c>stats</c> tool, the CLI <c>query --op stats</c> and the report's "Verified edges %" all
/// computed <c>verified = count - approx</c> and called every <see cref="Resolution.Join"/> edge
/// verified — including the ones nobody labelled at all, Join being the enum's default.
/// <c>GraphOrphansSource</c>, <c>ConfidenceLedger</c>, <c>FlowIndexBuilder</c> and the desktop
/// explorer counted <see cref="Resolution.Semantic"/> only, so the app rendered the very same edge
/// "approx" that the CLI called "verified". No number either produced was comparable with the
/// other.</para>
///
/// <para><b>Confidence is a different axis and is deliberately NOT folded in here.</b>
/// <see cref="GraphEdge.Confidence"/> is a 0..1 scalar a producer may lower for its own reasons
/// (an event-bus channel join ships at 0.6; a semantically-resolved seam target at 0.95). Mixing it
/// into the tier is what made <c>ConfidenceLedger</c>'s "approx" a fourth spelling — and it can put
/// one edge in two buckets at once, since a Semantic edge at 0.95 is verified AND under 1.0. Report
/// the tier and the confidence side by side; never let one silently redefine the other.</para>
/// </summary>
public static class EdgeConfidence
{
    /// <summary>The tier of a resolution. Total function — a future enum member would be a compile error.</summary>
    public static EdgeTier TierOf(Resolution resolution) => resolution switch
    {
        Resolution.Semantic => EdgeTier.Verified,
        Resolution.Syntactic => EdgeTier.Approximate,
        Resolution.Join => EdgeTier.Joined,
        _ => EdgeTier.Joined,
    };

    /// <inheritdoc cref="TierOf(Resolution)"/>
    public static EdgeTier TierOf(GraphEdge edge) => TierOf(edge.Resolution);

    /// <summary>True when a Roslyn symbol resolved this edge. THE definition of a verified edge.</summary>
    public static bool IsVerified(Resolution resolution) => TierOf(resolution) == EdgeTier.Verified;

    /// <inheritdoc cref="IsVerified(Resolution)"/>
    public static bool IsVerified(GraphEdge edge) => IsVerified(edge.Resolution);

    /// <summary>True when this edge was resolved by syntax/string heuristics — the "[approx]" marker.</summary>
    public static bool IsApproximate(Resolution resolution) => TierOf(resolution) == EdgeTier.Approximate;

    /// <inheritdoc cref="IsApproximate(Resolution)"/>
    public static bool IsApproximate(GraphEdge edge) => IsApproximate(edge.Resolution);

    /// <summary>True when this edge came from joining two detections (also the unlabelled default).</summary>
    public static bool IsJoined(Resolution resolution) => TierOf(resolution) == EdgeTier.Joined;

    /// <inheritdoc cref="IsJoined(Resolution)"/>
    public static bool IsJoined(GraphEdge edge) => IsJoined(edge.Resolution);

    /// <summary>The one word each tier is rendered with, so two surfaces cannot spell it differently.</summary>
    public static string Label(Resolution resolution) => TierOf(resolution) switch
    {
        EdgeTier.Verified => "verified",
        EdgeTier.Approximate => "approx",
        _ => "joined",
    };
}
