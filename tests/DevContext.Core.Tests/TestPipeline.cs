namespace DevContext.Core.Tests;

/// <summary>Shared composition of the full discovery pipeline for eval-style tests (extractors, pruners,
/// compressors, renderers) — one definition so trace/budget/quality tests agree on the wiring.</summary>
internal static class TestPipeline
{
    public static DiscoveryPipeline Build(ILoggerFactory loggerFactory)
    {
        var extractors = new List<IDiscoveryExtractor>
        {
            new FileTreeExtractor(),
            new SolutionDiscoveryExtractor(),
            new ProjectStructureExtractor(),
            new DependencyExtractor(),
            new SyntaxStructureExtractor(),
            new LayerClassifier(),
            new EndpointExtractor(),
            new MediatRExtractor(),
            new ControllerActionExtractor(),
            new EfCoreExtractor(),
            new EventBusExtractor(),
            new CallGraphExtractor(),
            new SourceBodyExtractor(),
            new IndirectWiringDetector(),
            new AspireExtractor(),
            new ProgramCsFlowExtractor(),
            new DiRegistrationExtractor(),
            new DesktopEntryExtractor(),
            new BlazorEntryExtractor(),
            new GrpcServiceExtractor(),
            new SignalRHubExtractor(),
            new AzureFunctionsExtractor(),
            new RazorPagesExtractor(),
            new OrleansGrainExtractor(),
            new NServiceBusExtractor(),
            new WolverineExtractor(),
            new AwsLambdaExtractor(),
            new GraphQlResolverExtractor(),
            new CliCommandExtractor(),
        };

        var pruners = new List<IPruner>();

        var compressors = new List<ICompressionStrategy>
        {
            new TrivialMemberCompressor(),
            new BoilerplateCompressor(),
            new StructuralDeduplicator(),
            new NamespaceGrouper(),
            new LlmFriendlyFormatter(),
            new AggressiveTruncator(),
        };

        var renderers = new Dictionary<string, IContextRenderer>
        {
            ["markdown"] = new MarkdownRenderer(),
            ["json"] = new JsonContextRenderer(),
        };

        return new DiscoveryPipeline(
            extractors, pruners, compressors, renderers,
            loggerFactory.CreateLogger<DiscoveryPipeline>());
    }
}
