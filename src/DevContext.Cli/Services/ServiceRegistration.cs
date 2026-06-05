using System.Reflection;
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

        // Auto-discover extractors via [DiscoveryAssembly] scanning
        // Load the Core assembly explicitly since it carries the [DiscoveryAssembly] attribute
        var coreAssembly = typeof(DiscoveryContext).Assembly;
        var extractors = ExtractorRegistry.DiscoverExtractors(coreAssembly);
        foreach (var extractor in extractors)
            services.AddSingleton(extractor);

        RegisterPruners(services);
        RegisterCompressors(services);
        RegisterRenderers(services);

        services.AddSingleton<DiscoveryPipeline>(sp =>
        {
            var allExtractors = sp.GetServices<IDiscoveryExtractor>().ToArray();
            var prunersList = sp.GetServices<IPruner>().ToArray();
            var compressorsList = sp.GetServices<ICompressionStrategy>().ToArray();
            var renderers = sp.GetServices<IContextRenderer>().ToDictionary(r => r.Format, r => r);
            var logger = sp.GetRequiredService<ILogger<DiscoveryPipeline>>();
            return new DiscoveryPipeline(allExtractors, prunersList, compressorsList, renderers, logger);
        });

        return services;
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
