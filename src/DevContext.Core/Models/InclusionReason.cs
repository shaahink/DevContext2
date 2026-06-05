namespace DevContext.Core.Models;

public sealed record InclusionReason(
    string Reason,
    string Source,
    float Weight
);
