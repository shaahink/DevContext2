namespace DevContext.Core.Utilities;

/// <summary>Shared token estimation used by compression strategies to compute before/after token counts.</summary>
public static class TokenEstimator
{
    /// <summary>Estimates the total number of tokens in the current model state.</summary>
    /// <param name="model">The discovery model to estimate tokens for.</param>
    /// <param name="includeRelations">Include base types and implemented interfaces in the count.</param>
    /// <param name="includeSourceBody">Include type source body text in the count.</param>
    /// <param name="excludeHardExcluded">When true, skip types marked as hard-excluded (designer files, DI extensions, duplicates).</param>
    public static int Estimate(DiscoveryModel model,
        bool includeRelations = true,
        bool includeSourceBody = false,
        bool excludeHardExcluded = true)
    {
        var chars = 0;
        foreach (var type in model.Types.Values)
        {
            if (type.IsPruned) continue;
            if (excludeHardExcluded && type.IsHardExcluded) continue;

            chars += type.Name?.Length ?? 0;
            chars += type.Namespace?.Length ?? 0;
            chars += type.Methods.Sum(m => m.Name.Length + m.ReturnType.Length);
            chars += type.Properties.Sum(p => p.Name.Length + p.PropertyType.Length);

            if (includeRelations)
            {
                chars += type.BaseTypes.Sum(b => b.Length);
                chars += type.ImplementedInterfaces.Sum(i => i.Length);
            }

            if (includeSourceBody && type.SourceBody is { } body)
                chars += body.Length;
        }

        return Math.Max(1, chars / 4);
    }
}
