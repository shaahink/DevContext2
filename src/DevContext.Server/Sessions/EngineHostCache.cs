using System.Collections.Concurrent;

namespace DevContext.Server.Sessions;

public sealed class EngineHostCache : IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, EngineHost> _hosts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILoggerFactory _loggerFactory;

    public EngineHostCache(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public EngineHost GetOrCreate(string rootPath)
    {
        return _hosts.GetOrAdd(rootPath, _ =>
        {
            var cache = new PersistentAnalysisCache(new RealFileSystem());
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDevContextServices(rootPath);
            var sp = services.BuildServiceProvider();
            var pipeline = sp.GetRequiredService<DiscoveryPipeline>();
            return new EngineHost(sp, pipeline, cache);
        });
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var host in _hosts.Values)
            await host.DisposeAsync().ConfigureAwait(false);
        _hosts.Clear();
    }
}

public sealed class EngineHost : IAsyncDisposable
{
    public ServiceProvider ServiceProvider { get; }
    public DiscoveryPipeline Pipeline { get; }
    public PersistentAnalysisCache Cache { get; }

    public EngineHost(ServiceProvider serviceProvider, DiscoveryPipeline pipeline, PersistentAnalysisCache cache)
    {
        ServiceProvider = serviceProvider;
        Pipeline = pipeline;
        Cache = cache;
    }

    public async ValueTask DisposeAsync()
    {
        await ServiceProvider.DisposeAsync().ConfigureAwait(false);
    }
}
