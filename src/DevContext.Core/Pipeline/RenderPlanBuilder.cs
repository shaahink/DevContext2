using System.Collections.Immutable;

namespace DevContext.Core.Pipeline;

/// <summary>Minimal render plan builder — the legacy catalog/token machinery is retired (W9).</summary>
public static class RenderPlanBuilder
{
    public static RenderPlan Build(AnalysisSnapshot snapshot, RenderRequest request)
        => new()
        {
            IncludedTypeIds = [],
            Excluded = [],
            Sections = [],
            PerTypeCharCap = 0,
            EstimatedTokens = request.MaxTokens,
            MaxTokens = request.MaxTokens,
        };
}
