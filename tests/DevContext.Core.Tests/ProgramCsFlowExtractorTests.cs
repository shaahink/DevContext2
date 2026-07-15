namespace DevContext.Core.Tests;

public sealed class ProgramCsFlowExtractorTests
{
    private static (DiscoveryContext Ctx, DiscoveryModel Model) Setup(params (string Path, string Source)[] files)
    {
        var fs = new FakeFileSystem();
        foreach (var (path, source) in files)
            fs.AddFile(path, source);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [.. files.Select(f => f.Path)];

        return (ctx, new DiscoveryModel());
    }

    [Fact]
    public async Task Detects_hosted_services_registered_in_extension_method_files()
    {
        // Startup composition factored out of Program.cs (ServiceRegistration.cs et al.) is the
        // norm in real repos — seen live: shamshir's 5 hosted services were invisible because
        // only files *named* Program.cs were walked.
        var (ctx, model) = Setup(
            (@"C:\repo\Configuration\ServiceRegistration.cs", """
                public static class ServiceRegistration
                {
                    public static IServiceCollection AddWebServices(this IServiceCollection services)
                    {
                        services.AddHostedService<CacheEvictionSweeper>();
                        return services;
                    }
                }
                """));
        var extractor = new ProgramCsFlowExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var workers = model.Detections.OfType<BackgroundWorkerDetection>().ToArray();
        Assert.Single(workers);
        Assert.Equal("CacheEvictionSweeper", workers[0].ImplementationType);
    }

    [Fact]
    public async Task Factory_lambda_registration_resolves_worker_type_not_lambda_text()
    {
        var (ctx, model) = Setup(
            (@"C:\repo\Program.cs", """
                var builder = WebApplication.CreateBuilder(args);
                builder.Services.AddSingleton<EngineWorker>();
                builder.Services.AddHostedService(sp => sp.GetRequiredService<EngineWorker>());
                builder.Services.AddHostedService<DataFeedService>(sp => sp.GetRequiredService<DataFeedService>());
                """));
        var extractor = new ProgramCsFlowExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var workers = model.Detections.OfType<BackgroundWorkerDetection>().ToArray();
        Assert.Equal(2, workers.Length);
        Assert.Contains(workers, w => w.ImplementationType == "EngineWorker");
        Assert.Contains(workers, w => w.ImplementationType == "DataFeedService");
        Assert.DoesNotContain(workers, w => w.ImplementationType.Contains("=>", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Detects_MapHub_in_middleware_extension_file()
    {
        var (ctx, model) = Setup(
            (@"C:\repo\Configuration\MiddlewarePipeline.cs", """
                public static class MiddlewarePipeline
                {
                    public static WebApplication UseWebPipeline(this WebApplication app)
                    {
                        app.MapHub<RunHub>("/hubs/run");
                        return app;
                    }
                }
                """));
        var extractor = new ProgramCsFlowExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var maps = model.Detections.OfType<MiddlewareDetection>()
            .Where(m => m.Kind == MiddlewareKind.MapX).ToArray();
        Assert.Contains(maps, m => m.MiddlewareType == "MapHub");
    }
}
