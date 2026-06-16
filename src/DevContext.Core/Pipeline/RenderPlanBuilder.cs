namespace DevContext.Core.Pipeline;

/// <summary>Builds a RenderPlan from a snapshot and request. Pure, deterministic, cheap.</summary>
public static class RenderPlanBuilder
{
    private const int SafetyMargin = 500;

    public static RenderPlan Build(AnalysisSnapshot snapshot, RenderRequest request)
    {
        var model = snapshot.Model;
        var scenario = snapshot.Scenario;

        // 1. Resolve sections
        var sections = request.Sections.IsDefaultOrEmpty
            ? scenario.RequiredSections
            : request.Sections;

        // 2. Identify pinned types: explicit focus types always included (exempt from caps)
        var pinnedIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var fp in snapshot.Analysis.FocusPoints)
        {
            if (fp.TypeName is null) continue;
            foreach (var type in model.Types.Values)
            {
                if (string.Equals(type.Name, fp.TypeName, StringComparison.Ordinal)
                    || type.Id.EndsWith("." + fp.TypeName, StringComparison.Ordinal))
                {
                    pinnedIds.Add(type.Id);
                }
            }
        }

        // 3. Order candidates by FinalScore desc, then name for stable tie-break
        // (PLAN-10 E1: FinalScore is now just RoleScore from PatternRelevancePruner —
        //  the weighted multi-pruner system is retired. Map/Trace bypasses this entirely.)
        var candidates = model.Types.Values
            .OrderByDescending(t => t.FinalScore)
            .ThenBy(t => t.Name)
            .ToList();

        var included = ImmutableArray.CreateBuilder<string>();
        var excluded = ImmutableArray.CreateBuilder<PlannedExclusion>();
        var budget = Math.Max(0, request.MaxTokens - SafetyMargin);
        var usedTokens = 0;

        // Pin: explicit focus types always included, exempt from caps
        foreach (var type in candidates)
        {
            if (!pinnedIds.Contains(type.Id)) continue;
            if (type.IsHardExcluded)
            {
                excluded.Add(new PlannedExclusion(type.Id, type.Name, type.FinalScore,
                    "focus pin vetoed: " + (type.ExclusionReason ?? "test project")));
                continue;
            }

            var pinTokens = EstimateTokenCost(type);
            included.Add(type.Id);
            usedTokens += pinTokens;
        }

        // Budget pass: include remaining types in score order
        foreach (var type in candidates)
        {
            if (pinnedIds.Contains(type.Id)) continue; // already included

            // Floor: detection-bearing types are never hard-excluded (may still lose to budget)
            if (type.IsHardExcluded)
            {
                excluded.Add(new PlannedExclusion(type.Id, type.Name, type.FinalScore,
                    type.ExclusionReason ?? "hard-excluded"));
                continue;
            }

            var typeTokens = EstimateTokenCost(type);

            var underBudget = budget > 0 && usedTokens + typeTokens <= budget;

            if (underBudget)
            {
                included.Add(type.Id);
                usedTokens += typeTokens;
            }
            else
            {
                excluded.Add(new PlannedExclusion(type.Id, type.Name, type.FinalScore, "budget"));
            }
        }

        // 4. PerTypeCharCap — respect AggressiveTruncation flag
        var perTypeCap = scenario.Compression.AggressiveTruncation
            ? scenario.Compression.PerTypeCharCap
            : int.MaxValue;

        var estimatedTokens = usedTokens + SafetyMargin;

        return new RenderPlan
        {
            IncludedTypeIds = included.ToImmutable(),
            Excluded = excluded.ToImmutable(),
            Sections = sections,
            PerTypeCharCap = perTypeCap,
            EstimatedTokens = Math.Max(1, estimatedTokens),
            MaxTokens = request.MaxTokens,
        };
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

        return Math.Max(1, charCount / 4);
    }
}
