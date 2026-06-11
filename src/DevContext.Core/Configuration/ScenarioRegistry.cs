namespace DevContext.Core.Configuration;

/// <summary>Registry of built-in scenarios defining extraction, pruning, and compression configurations.</summary>
public static class ScenarioRegistry
{
    /// <summary>Gets the dictionary of built-in scenarios keyed by name.</summary>
    public static IReadOnlyDictionary<string, Scenario> BuiltIn { get; } =
        new Dictionary<string, Scenario>
        {
            ["overview"] = new()
            {
                Name = "overview",
                DisplayName = "Overview",
                Description = "High-level architecture map, endpoints, handlers, data model, and wiring",
                Pruning = new PruningConfig { MaxPathDistance = 2, MaxSurvivingTypes = 40 },
                Compression = new CompressionConfig { AggressiveTruncation = false },
                RequiredSections = [SectionNames.ArchitectureOverview, SectionNames.Endpoints, SectionNames.MediatRHandlers, SectionNames.DataModel, SectionNames.NonObviousWiring, SectionNames.RelatedTypes]
            },
            ["deep-dive"] = new()
            {
                Name = "deep-dive",
                DisplayName = "Deep Dive",
                Description = "Detailed endpoint view with call graph, event flow, and anti-patterns",
                Pruning = new PruningConfig { MaxPathDistance = 1, MaxCallDepth = 5, MaxSurvivingTypes = 25 },
                Compression = new CompressionConfig { AggressiveTruncation = true },
                RequiredSections = [SectionNames.Endpoints, SectionNames.CallGraph, SectionNames.MediatRHandlers, SectionNames.DataModel, SectionNames.MessageConsumers, SectionNames.NonObviousWiring]
            },
            ["audit"] = new()
            {
                Name = "audit",
                DisplayName = "Audit",
                Description = "Find indirect wiring, service locators, reflection, and middleware issues",
                Pruning = new PruningConfig { EnablePatternBoost = true, MaxSurvivingTypes = 50 },
                Compression = new CompressionConfig { AggressiveTruncation = true },
                RequiredSections = [SectionNames.ArchitectureOverview, SectionNames.NonObviousWiring, SectionNames.RelatedTypes]
            }
        }.ToFrozenDictionary();
}
