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

        var candidates = model.Types.Values
            .Where(t => !t.IsPruned)
            .OrderByDescending(t => t.RelevanceScore)
            .ToList();

        var usedTokens = 0;
        var keptCount = 0;
        var prunedCount = 0;
        var minimumSafeTypes = budget < 100 ? 0 : 5;

        foreach (var type in candidates)
        {
            ct.ThrowIfCancellationRequested();

            var typeTokens = EstimateTokenCost(type);
            if (keptCount < minimumSafeTypes || usedTokens + typeTokens <= budget)
            {
                usedTokens += typeTokens;
                keptCount++;
            }
            else
            {
                type.IsPruned = true;
                prunedCount++;
                model.PrunedTypeIds.Add(type.Id);
            }
        }

        if (prunedCount > 0)
        {
            model.PruningNotes.Add($"TokenBudgetEnforcer: kept {keptCount} types ({prunedCount} pruned for budget {budget})");
        }

        // Enforce scenario-level type count cap after token budget pass
        var maxTypes = context.ActiveScenario.Pruning.MaxSurvivingTypes;
        if (maxTypes > 0)
        {
            var survivors = model.Types.Values
                .Where(t => !t.IsPruned)
                .OrderByDescending(t => t.RelevanceScore)
                .ToList();

            if (survivors.Count > maxTypes)
            {
                foreach (var type in survivors.Skip(maxTypes))
                {
                    type.IsPruned = true;
                    model.PrunedTypeIds.Add(type.Id);
                }
                model.PruningNotes.Add($"TokenBudgetEnforcer: capped at {maxTypes} types (scenario limit)");
            }
        }

        return default;
    }

    private static int EstimateTokenCost(TypeDiscovery type)
    {
        var charCount = (type.Name?.Length ?? 0)
                        + (type.Namespace?.Length ?? 0)
                        + type.Methods.Sum(m => m.Name.Length
                            + m.ReturnType.Length
                            + m.ParameterTypes.Sum(p => p.Length)
                            + m.ParameterNames.Sum(p => p.Length))
                        + type.Properties.Sum(p => p.Name.Length + p.PropertyType.Length)
                        + type.BaseTypes.Sum(b => b.Length)
                        + type.ImplementedInterfaces.Sum(i => i.Length)
                        + type.Attributes.Sum(a => a.Length)
                        + (type.SourceBody?.Length ?? 0);

        // Use chars/4 (matching the renderer's estimation) to avoid underestimation
        return Math.Max(1, charCount / 4);
    }
}
