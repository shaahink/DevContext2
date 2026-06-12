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

        // 2. Order candidates by FinalScore desc, then name for stable tie-break
        var candidates = model.Types.Values
            .OrderByDescending(t => t.FinalScore)
            .ThenBy(t => t.Name)
            .ToList();

        var included = ImmutableArray.CreateBuilder<string>();
        var excluded = ImmutableArray.CreateBuilder<PlannedExclusion>();
        var budget = request.MaxTokens - SafetyMargin;
        var maxTypes = scenario.Pruning.MaxSurvivingTypes;
        var usedTokens = 0;

        foreach (var type in candidates)
        {
            if (type.IsHardExcluded)
            {
                excluded.Add(new PlannedExclusion(type.Id, type.Name, type.FinalScore,
                    type.ExclusionReason ?? "hard-excluded"));
                continue;
            }

            var typeTokens = EstimateTokenCost(type);

            var underBudget = budget <= 0 || usedTokens + typeTokens <= budget;
            var underCap = maxTypes <= 0 || included.Count < maxTypes;

            if (underBudget && underCap)
            {
                included.Add(type.Id);
                usedTokens += typeTokens;
            }
            else
            {
                var reason = !underCap ? "scenario cap" : "budget";
                excluded.Add(new PlannedExclusion(type.Id, type.Name, type.FinalScore, reason));
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
