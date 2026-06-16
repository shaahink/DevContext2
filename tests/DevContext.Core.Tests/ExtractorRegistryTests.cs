using System.Reflection;

namespace DevContext.Core.Tests;

public sealed class ExtractorRegistryTests
{
    [Fact]
    public void DiscoverExtractors_FromCoreAssembly_ReturnsAllExtractors()
    {
        var coreAssembly = typeof(DiscoveryContext).Assembly;
        var extractors = ExtractorRegistry.DiscoverExtractors(coreAssembly);

        Assert.NotEmpty(extractors);
        Assert.Contains(extractors, e => string.Equals(e.Name, "FileTreeExtractor", StringComparison.Ordinal));
        Assert.Contains(extractors, e => string.Equals(e.Name, "EndpointExtractor", StringComparison.Ordinal));
        Assert.Contains(extractors, e => string.Equals(e.Name, "ProgramCsFlowExtractor", StringComparison.Ordinal));
        Assert.Contains(extractors, e => string.Equals(e.Name, "DiRegistrationExtractor", StringComparison.Ordinal));
        Assert.Contains(extractors, e => string.Equals(e.Name, "MediatRExtractor", StringComparison.Ordinal));
        Assert.Contains(extractors, e => string.Equals(e.Name, "EfCoreExtractor", StringComparison.Ordinal));
    }

    [Fact]
    public void DiscoverExtractors_FromCoreAssembly_ReturnsCorrectCount()
    {
        var coreAssembly = typeof(DiscoveryContext).Assembly;
        var extractors = ExtractorRegistry.DiscoverExtractors(coreAssembly);
        // Should have at least 15 extractors (FileTree, SolutionDiscovery, ProjectStructure,
        // Dependency, LayerClassifier, SyntaxStructure, ProgramCsFlow, DiRegistration,
        // Endpoint, ControllerAction, MediatR, EfCore, EventBus, IndirectWiring, Aspire,
        // CallGraph, SourceBody, ArchitectureStyleDetector)
        Assert.True(extractors.Count >= 15);
    }

    [Fact]
    public void DiscoverExtractors_FromAssemblyWithoutAttribute_Throws()
    {
        var testAssembly = typeof(ExtractorRegistryTests).Assembly;
        Assert.Throws<InvalidOperationException>(() =>
            ExtractorRegistry.DiscoverExtractors(testAssembly));
    }

    [Fact]
    public void DiscoverExtractors_AllExtractors_SortedByOrder()
    {
        var coreAssembly = typeof(DiscoveryContext).Assembly;
        var extractors = ExtractorRegistry.DiscoverExtractors(coreAssembly);

        for (int i = 1; i < extractors.Count; i++)
        {
            var prevOrder = extractors[i - 1].GetType().GetCustomAttribute<ExtractorOrderAttribute>(false)?.Order ?? 100;
            var currOrder = extractors[i].GetType().GetCustomAttribute<ExtractorOrderAttribute>(false)?.Order ?? 100;
            Assert.True(prevOrder <= currOrder,
                $"Extractors not sorted: {extractors[i - 1].Name} (order {prevOrder}) before {extractors[i].Name} (order {currOrder})");
        }
    }

    [Fact]
    public void DiscoverExtractors_AllExtractors_HaveStageAndCapabilities()
    {
        var coreAssembly = typeof(DiscoveryContext).Assembly;
        var extractors = ExtractorRegistry.DiscoverExtractors(coreAssembly);

        foreach (var ext in extractors)
        {
            Assert.False(string.IsNullOrEmpty(ext.Name), $"Extractor {ext.GetType().Name} has no Name");
            Assert.NotNull(ext.Capabilities);
            Assert.False(string.IsNullOrEmpty(ext.Capabilities.Description), $"Extractor {ext.Name} has no Description");
        }
    }
}
