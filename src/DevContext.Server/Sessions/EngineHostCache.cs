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

    /// <summary>Number of live hosts (introspection; tests pin the release behavior on it).</summary>
    public int Count => _hosts.Count;

    /// <summary>D5.3 laden-server — removes and disposes the host for a root. The session manager
    /// calls this when the LAST session on that root goes away: a host pins every parsed
    /// SyntaxTree/text/XDocument of its repo (<see cref="PersistentAnalysisCache"/>), so an
    /// unevicted host cache grew without bound across distinct analyzed roots — the
    /// "unresponsive after ~36 analyses" class, which only a server restart cured. A released
    /// root re-opens warm via the snapshot cache instead of RAM.</summary>
    public async ValueTask ReleaseAsync(string rootPath)
    {
        if (_hosts.TryRemove(rootPath, out var host))
            await host.DisposeAsync().ConfigureAwait(false);
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
