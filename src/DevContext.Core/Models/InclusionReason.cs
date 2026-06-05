namespace DevContext.Core.Models;

/// <summary>Records why a particular type or item was included in the final output.</summary>
public sealed record InclusionReason(
    string Reason,
    string Source,
    float Weight
);
