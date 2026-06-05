namespace DevContext.Core.Configuration;

public static class ScenarioRegistry
{
    public static IReadOnlyDictionary<string, Scenario> BuiltIn { get; } =
        new Dictionary<string, Scenario>
        {
            ["architecture"] = new()
            {
                Name = "architecture",
                DisplayName = "Architecture Overview",
                Description = "High-level architecture map with layers and signals",
                Pruning = new PruningConfig { MaxSurvivingTypes = 30 },
                Compression = new CompressionConfig { AggressiveTruncation = false },
                RequiredSections = ["Architecture overview", "Related types"]
            },
            ["debug-endpoint"] = new()
            {
                Name = "debug-endpoint",
                DisplayName = "Debug Endpoint",
                Description = "Detailed view of a specific endpoint with call graph",
                Pruning = new PruningConfig { MaxPathDistance = 1, MaxCallDepth = 5, MaxSurvivingTypes = 20 },
                Compression = new CompressionConfig { AggressiveTruncation = true },
                RequiredSections = ["Entry", "Endpoints", "Call graph"]
            },
            ["add-similar-feature"] = new()
            {
                Name = "add-similar-feature",
                DisplayName = "Add Similar Feature",
                Description = "Context for implementing a new feature similar to existing ones",
                Pruning = new PruningConfig { MaxPathDistance = 2, MaxSurvivingTypes = 40 },
                Compression = new CompressionConfig(),
                RequiredSections = ["Entry", "Related types"]
            },
            ["modify-middleware"] = new()
            {
                Name = "modify-middleware",
                DisplayName = "Modify Middleware",
                Description = "Focus on pipeline and middleware registration",
                Pruning = new PruningConfig { EnablePatternBoost = true, MaxSurvivingTypes = 25 },
                Compression = new CompressionConfig { RemoveTrivialMembers = true },
                RequiredSections = ["Entry", "Non-obvious wiring"]
            },
            ["trace-message-flow"] = new()
            {
                Name = "trace-message-flow",
                DisplayName = "Trace Message Flow",
                Description = "Event/message flow through the system",
                Pruning = new PruningConfig { MaxCallDepth = 5, MaxSurvivingTypes = 30 },
                Compression = new CompressionConfig(),
                RequiredSections = ["Domain signals", "Call graph"]
            },
            ["harden-di"] = new()
            {
                Name = "harden-di",
                DisplayName = "Harden DI",
                Description = "Find indirect wiring, reflection, and service locator patterns",
                Pruning = new PruningConfig { EnablePatternBoost = true, MaxSurvivingTypes = 50 },
                Compression = new CompressionConfig { AggressiveTruncation = true },
                RequiredSections = ["Non-obvious wiring"]
            }
        }.ToFrozenDictionary();
}
