namespace DevContext.Core.Models;

public sealed record SectionBudget(
    string SectionName,
    int Priority,
    int ReservedTokens,
    int MaxTokens
);
