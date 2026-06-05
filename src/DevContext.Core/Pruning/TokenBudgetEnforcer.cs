namespace DevContext.Core.Pruning;

/// <summary>Enforces the token budget by pruning lowest-scoring types when the estimated token count exceeds the budget.</summary>
public sealed class TokenBudgetEnforcer : IPruner
{
    /// <summary>Gets the name of this pruner.</summary>
    public string Name => "TokenBudgetEnforcer";
    /// <summary>Gets the execution order.</summary>
    public int Order => 40;

    public ValueTask PruneAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var budget = model.Budget.MaxTokens - model.Budget.SafetyMargin;
        if (budget <= 0) return default;

        var surviving = model.Types.Values
            .Where(t => !t.IsPruned)
            .OrderByDescending(t => t.PathProximityScore + t.RelevanceScore)
            .ToList();

        var usedTokens = 0;
        var prunedCount = 0;
        foreach (var type in surviving)
        {
            ct.ThrowIfCancellationRequested();

            var typeTokens = EstimateTokenCost(type);
            if (usedTokens + typeTokens > budget)
            {
                type.IsPruned = true;
                prunedCount++;
                model.PrunedTypeIds.Add(type.Id);
            }
            else
            {
                usedTokens += typeTokens;
            }
        }

        if (prunedCount > 0)
        {
            model.PruningNotes.Add($"TokenBudgetEnforcer: pruned {prunedCount} types (budget {budget} exceeded by {surviving.Sum(t => EstimateTokenCost(t)) - budget} tokens)");
        }

        return default;
    }

    private static int EstimateTokenCost(TypeDiscovery type)
    {
        var charCount = (type.Name?.Length ?? 0)
                        + (type.Namespace?.Length ?? 0)
                        + type.Methods.Sum(m => m.Name.Length + m.ReturnType.Length + m.ParameterTypes.Sum(p => p.Length))
                        + type.Properties.Sum(p => p.Name.Length + p.PropertyType.Length)
                        + type.BaseTypes.Sum(b => b.Length)
                        + type.ImplementedInterfaces.Sum(i => i.Length)
                        + type.Attributes.Sum(a => a.Length)
                        + (type.SourceBody?.Length ?? 0);

        return Math.Max(1, charCount / 4);
    }
}
