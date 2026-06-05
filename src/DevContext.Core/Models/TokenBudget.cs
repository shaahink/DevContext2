namespace DevContext.Core.Models;

/// <summary>Defines token budget constraints for the output context.</summary>
public sealed record TokenBudget
{
    /// <summary>Maximum number of tokens allowed in the output.</summary>
    public int MaxTokens { get; init; } = 8000;
    /// <summary>Safety margin subtracted from max tokens for overhead.</summary>
    public int SafetyMargin { get; init; } = 500;

    /// <summary>Default token budget (8000 max, 500 margin).</summary>
    public static TokenBudget Default => new();
}
