namespace DevContext.Core.Models;

public sealed record TokenBudget
{
    public int MaxTokens { get; init; } = 8000;
    public int SafetyMargin { get; init; } = 500;

    public static TokenBudget Default => new();
}
