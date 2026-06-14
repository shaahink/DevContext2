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
                Pruning = new PruningConfig { MaxPathDistance = 2, MaxSurvivingTypes = 40, RoleWeight = 0.7, FocusWeight = 0.3 },
                Compression = new CompressionConfig { AggressiveTruncation = false },
                RequiredSections = [SectionNames.ArchitectureOverview, SectionNames.Endpoints, SectionNames.MediatRHandlers, SectionNames.DataModel, SectionNames.DiRegistrations, SectionNames.MiddlewarePipeline, SectionNames.RelatedTypes]
            },
            ["deep-dive"] = new()
            {
                Name = "deep-dive",
                DisplayName = "Slice",
                Description = "Entry-point focused: call graph, handler chain, event flow",
                Pruning = new PruningConfig { MaxPathDistance = 3, MaxCallDepth = 5, MaxSurvivingTypes = 25, RoleWeight = 0.35, FocusWeight = 0.65 },
                Compression = new CompressionConfig { AggressiveTruncation = true },
                RequiredSections = [SectionNames.Endpoints, SectionNames.CallGraph, SectionNames.MediatRHandlers, SectionNames.DataModel, SectionNames.MessageConsumers, SectionNames.DiRegistrations, SectionNames.BackgroundWorkers, SectionNames.MiddlewarePipeline]
            }
        }.ToFrozenDictionary();
}
