namespace DevContext.Core.Pipeline;

/// <summary>Builds a RenderPlan from a snapshot and request. Pure, deterministic, cheap.</summary>
public static class RenderPlanBuilder
{
    private const int SafetyMargin = 500;

    public static RenderPlan Build(AnalysisSnapshot snapshot, RenderRequest request)
    {
        var model = snapshot.Model;
        var scenario = snapshot.Scenario;

        var sections = request.Sections.IsDefaultOrEmpty
            ? scenario.RequiredSections
            : request.Sections;

        var pinnedIds = BuildPinnedIds(snapshot, model);

        var candidates = model.Types.Values
            .OrderByDescending(t => t.FinalScore)
            .ThenBy(t => t.Name, StringComparer.Ordinal)
            .ToList();

        var included = ImmutableArray.CreateBuilder<string>();
        var excluded = ImmutableArray.CreateBuilder<PlannedExclusion>();
        var usedTokens = 0;

        // Pin: explicit focus types always included, exempt from caps
        IncludePinned(candidates, pinnedIds, included, excluded, ref usedTokens);

        // Budget pass: include remaining types in score order
        ApplyBudgetPass(candidates, pinnedIds, included, excluded, ref usedTokens, request.MaxTokens);

        var perTypeCap = scenario.Compression.AggressiveTruncation
            ? scenario.Compression.PerTypeCharCap
            : int.MaxValue;

        return new RenderPlan
        {
            IncludedTypeIds = included.ToImmutable(),
            Excluded = excluded.ToImmutable(),
            Sections = sections,
            PerTypeCharCap = perTypeCap,
            EstimatedTokens = Math.Max(1, usedTokens + SafetyMargin),
            MaxTokens = request.MaxTokens,
        };
    }

    private static HashSet<string> BuildPinnedIds(AnalysisSnapshot snapshot, DiscoveryModel model)
    {
        var pinnedIds = new HashSet<string>(StringComparer.Ordinal);
        if (snapshot.Analysis.FocusPoints.Count == 0)
            return pinnedIds;

        // Build name-to-ID lookup once
        var nameToIds = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var type in model.Types.Values)
        {
            if (!nameToIds.TryGetValue(type.Name, out var ids))
                nameToIds[type.Name] = ids = [];
            ids.Add(type.Id);
        }

        foreach (var fp in snapshot.Analysis.FocusPoints)
        {
            if (fp.TypeName is null) continue;
            if (nameToIds.TryGetValue(fp.TypeName, out var exactMatches))
            {
                pinnedIds.UnionWith(exactMatches);
                continue;
            }
            // Fallback: suffix match (e.g. "Service" matches "MyApp.Services.Service")
            foreach (var type in model.Types.Values)
            {
                if (type.Id.EndsWith("." + fp.TypeName, StringComparison.Ordinal))
                    pinnedIds.Add(type.Id);
            }
        }
        return pinnedIds;
    }

    private static void IncludePinned(
        List<TypeDiscovery> candidates,
        HashSet<string> pinnedIds,
        ImmutableArray<string>.Builder included,
        ImmutableArray<PlannedExclusion>.Builder excluded,
        ref int usedTokens)
    {
        foreach (var type in candidates)
        {
            if (!pinnedIds.Contains(type.Id)) continue;
            if (type.IsHardExcluded)
            {
                excluded.Add(new PlannedExclusion(type.Id, type.Name, type.FinalScore,
                    "focus pin vetoed: " + (type.ExclusionReason ?? "test project")));
                continue;
            }

            included.Add(type.Id);
            usedTokens += EstimateTokenCost(type);
        }
    }

    private static void ApplyBudgetPass(
        List<TypeDiscovery> candidates,
        HashSet<string> pinnedIds,
        ImmutableArray<string>.Builder included,
        ImmutableArray<PlannedExclusion>.Builder excluded,
        ref int usedTokens,
        int maxTokens)
    {
        var budget = Math.Max(0, maxTokens - SafetyMargin);

        foreach (var type in candidates)
        {
            if (pinnedIds.Contains(type.Id)) continue;

            if (type.IsHardExcluded)
            {
                excluded.Add(new PlannedExclusion(type.Id, type.Name, type.FinalScore,
                    type.ExclusionReason ?? "hard-excluded"));
                continue;
            }

            var typeTokens = EstimateTokenCost(type);

            if (budget > 0 && usedTokens + typeTokens <= budget)
            {
                included.Add(type.Id);
                usedTokens += typeTokens;
            }
            else
            {
                excluded.Add(new PlannedExclusion(type.Id, type.Name, type.FinalScore, "budget"));
            }
        }
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
