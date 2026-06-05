namespace DevContext.Core.Pruning;

public sealed class TokenBudgetEnforcer : IPruner
{
    public string Name => "TokenBudgetEnforcer";
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
        foreach (var type in surviving)
        {
            ct.ThrowIfCancellationRequested();

            var typeTokens = EstimateTokenCost(type);
            if (usedTokens + typeTokens > budget)
            {
                type.IsPruned = true;
                model.PruningNotes.Add($"Pruned '{type.Id}' by TokenBudgetEnforcer (budget exceeded: {usedTokens}+{typeTokens}>{budget})");
                model.PrunedTypeIds.Add(type.Id);
            }
            else
            {
                usedTokens += typeTokens;
            }
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
