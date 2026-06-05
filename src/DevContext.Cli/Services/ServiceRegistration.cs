using DevContext.Core.Extractors.Generic;
using DevContext.Core.Extractors.Specific;
using DevContext.Core.Pruning;
using DevContext.Core.Compression;
using DevContext.Core.Rendering;

namespace DevContext.Cli.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddDevContextServices(this IServiceCollection services, string rootPath, string? configPath = null)
    {
        var config = configPath != null ? DevContextConfig.Load(configPath) : null;

        services.AddSingleton<IFileSystem>(_ => new RealFileSystem());
        services.AddSingleton<IAnalysisCache>(sp => new AnalysisCache(sp.GetRequiredService<IFileSystem>()));

        RegisterGenericExtractors(services);
        RegisterSpecificExtractors(services);
        RegisterPruners(services);
        RegisterCompressors(services);
        RegisterRenderers(services);

        services.AddSingleton<DiscoveryPipeline>(sp =>
        {
            var extractors = sp.GetServices<IDiscoveryExtractor>().ToArray();
            var pruners = sp.GetServices<IPruner>().ToArray();
            var compressors = sp.GetServices<ICompressionStrategy>().ToArray();
            var renderers = sp.GetServices<IContextRenderer>().ToDictionary(r => r.Format, r => r);
            var logger = sp.GetRequiredService<ILogger<DiscoveryPipeline>>();
            return new DiscoveryPipeline(extractors, pruners, compressors, renderers, logger);
        });

        return services;
    }

    private static void RegisterGenericExtractors(IServiceCollection services)
    {
        services.AddSingleton<IDiscoveryExtractor>(_ => new FileTreeExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new SolutionDiscoveryExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new ProjectStructureExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new DependencyExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new LayerClassifier());
        services.AddSingleton<IDiscoveryExtractor>(_ => new SyntaxStructureExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new ProgramCsFlowExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new DiRegistrationExtractor());
    }

    private static void RegisterSpecificExtractors(IServiceCollection services)
    {
        services.AddSingleton<IDiscoveryExtractor>(_ => new EndpointExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new ControllerActionExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new MediatRExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new EfCoreExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new EventBusExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new AspireExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new IndirectWiringDetector());
        services.AddSingleton<IDiscoveryExtractor>(_ => new CallGraphExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new SourceBodyExtractor());
    }

    private static void RegisterPruners(IServiceCollection services)
    {
        services.AddSingleton<IPruner>(_ => new PathProximityPruner());
        services.AddSingleton<IPruner>(_ => new CallReachabilityPruner());
        services.AddSingleton<IPruner>(_ => new PatternRelevancePruner());
        services.AddSingleton<IPruner>(_ => new TokenBudgetEnforcer());
    }

    private static void RegisterCompressors(IServiceCollection services)
    {
        services.AddSingleton<ICompressionStrategy>(_ => new TrivialMemberCompressor());
        services.AddSingleton<ICompressionStrategy>(_ => new BoilerplateCompressor());
        services.AddSingleton<ICompressionStrategy>(_ => new StructuralDeduplicator());
        services.AddSingleton<ICompressionStrategy>(_ => new NamespaceGrouper());
        services.AddSingleton<ICompressionStrategy>(_ => new LlmFriendlyFormatter());
        services.AddSingleton<ICompressionStrategy>(_ => new AggressiveTruncator());
    }

    private static void RegisterRenderers(IServiceCollection services)
    {
        services.AddSingleton<IContextRenderer>(_ => new MarkdownRenderer());
        services.AddSingleton<IContextRenderer>(_ => new JsonContextRenderer());
    }
}
