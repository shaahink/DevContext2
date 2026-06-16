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

        // Pruners, compressors, and renderers are registered manually rather than via
        // assembly scanning. Unlike extractors (which follow the "implement + mark + test"
        // extensibility model via IDiscoveryExtractor + [DiscoveryAssembly] + [ExtractorOrder]),
        // these components are few (4 pruners, 6 compressors, 2 renderers) and their
        // order/selection is tightly coupled to pipeline configuration.
        // If the count grows significantly, consider adding [PrunerOrder], [CompressorOrder],
        // and [RendererFormat] attributes with scanning similar to ExtractorRegistry.

        // Pruners (ordered by pipeline execution) — PLAN-10 E1: Path/Call pruners retired
        services.AddSingleton<IPruner>(_ => new PatternRelevancePruner());
        services.AddSingleton<IPruner>(_ => new TokenBudgetEnforcer());

        // Compressors (ordered by pipeline execution)
        services.AddSingleton<ICompressionStrategy>(_ => new TrivialMemberCompressor());
        services.AddSingleton<ICompressionStrategy>(_ => new BoilerplateCompressor());
        services.AddSingleton<ICompressionStrategy>(_ => new StructuralDeduplicator());
        services.AddSingleton<ICompressionStrategy>(_ => new NamespaceGrouper());
        services.AddSingleton<ICompressionStrategy>(_ => new LlmFriendlyFormatter());

        // Renderers (keyed by format)
        services.AddSingleton<IContextRenderer>(_ => new MarkdownRenderer());
        services.AddSingleton<IContextRenderer>(_ => new JsonContextRenderer());
        services.AddSingleton<IContextRenderer>(_ => new HtmlContextRenderer());

        services.AddSingleton<DiscoveryPipeline>(sp =>
        {
            var allExtractors = sp.GetServices<IDiscoveryExtractor>().ToArray();
            var prunersList = sp.GetServices<IPruner>().ToArray();
            var compressorsList = sp.GetServices<ICompressionStrategy>().ToArray();
            var renderers = sp.GetServices<IContextRenderer>().ToDictionary(r => r.Format, r => r, StringComparer.Ordinal);
            var logger = sp.GetRequiredService<ILogger<DiscoveryPipeline>>();
            return new DiscoveryPipeline(allExtractors, prunersList, compressorsList, renderers, logger);
        });

        return services;
    }
}
