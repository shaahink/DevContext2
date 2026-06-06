using DevContext.Core.Extractors.Generic;
using DevContext.Core.Extractors.Specific;

namespace DevContext.Core.Tests;

public sealed class DiRegistrationAndMiddlewareTests
{
    [Fact]
    public async Task ProgramCsFlowExtractor_DetectsMiddlewareAndBackgroundWorkers()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Program.cs", """
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHostedService<OrderProcessingService>();

            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors();
            app.MapGet("/health", () => "OK");
            app.MapPost("/api/orders", (OrderRequest req) => Results.Ok());

            app.Run();
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Program.cs"];

        var model = new DiscoveryModel();

        var extractor = new ProgramCsFlowExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var middleware = model.Detections.OfType<MiddlewareDetection>().ToArray();
        Assert.Equal(5, middleware.Length);

        var useAuth = middleware.FirstOrDefault(m => m.MiddlewareType == "UseAuthentication");
        Assert.NotNull(useAuth);
        Assert.Equal(MiddlewareKind.UseX, useAuth.Kind);
        Assert.Equal(1, useAuth.PipelineOrder);

        var useCors = middleware.FirstOrDefault(m => m.MiddlewareType == "UseCors");
        Assert.NotNull(useCors);
        Assert.Equal(3, useCors.PipelineOrder);

        var mapGet = middleware.FirstOrDefault(m => m.MiddlewareType == "MapGet");
        Assert.NotNull(mapGet);
        Assert.Equal(MiddlewareKind.MapX, mapGet.Kind);

        var workers = model.Detections.OfType<BackgroundWorkerDetection>().ToArray();
        Assert.Single(workers);
        Assert.Equal("OrderProcessingService", workers[0].ImplementationType);
        Assert.Equal(BackgroundWorkerKind.HostedService, workers[0].Kind);
    }

    [Fact]
    public async Task DiRegistrationExtractor_DetectsServiceRegistrations()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Program.cs", """
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddTransient<INotificationService, EmailNotificationService>();
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            var app = builder.Build();
            app.Run();
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Program.cs"];

        var model = new DiscoveryModel();

        var extractor = new DiRegistrationExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var registrations = model.Detections.OfType<DiRegistrationDetection>().ToArray();
        Assert.Equal(4, registrations.Length);

        var singleton = registrations.FirstOrDefault(r => r.Lifetime == "Singleton");
        Assert.NotNull(singleton);
        Assert.Equal("IProductRepository", singleton.ServiceType);
        Assert.Equal("InMemoryProductRepository", singleton.ImplementationType);

        var scoped = registrations.FirstOrDefault(r => r.Lifetime == "Scoped");
        Assert.NotNull(scoped);
        Assert.Equal("IOrderService", scoped.ServiceType);

        var transient = registrations.FirstOrDefault(r => r.Lifetime == "Transient");
        Assert.NotNull(transient);

        var addMediatR = registrations.FirstOrDefault(r => r.Lifetime == "Extension");
        Assert.NotNull(addMediatR);
        Assert.Equal("AddMediatR", addMediatR.ServiceType);
    }

    [Fact]
    public async Task ProgramCsFlowExtractor_DetectsOrphanPatterns()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Program.cs", """
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors();
            builder.Services.AddAuthentication();

            var app = builder.Build();
            app.UseAuthentication();
            app.Run();
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Program.cs"];

        var model = new DiscoveryModel();

        var extractor = new ProgramCsFlowExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var diags = model.Diagnostics.ToArray();
        var corsDiag = diags.FirstOrDefault(d =>
            d.Message.Contains("AddCors") && d.Message.Contains("UseCors"));

        Assert.NotNull(corsDiag);
        Assert.Equal(DiagnosticLevel.Info, corsDiag.Level);
    }

    [Fact]
    public async Task DiRegistrationExtractor_DetectsChainedExtensions()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Program.cs", """
            var builder = WebApplication.CreateBuilder(args);
            builder.Services
                .AddCors()
                .AddMemoryCache()
                .AddSingleton<ICache, MemoryCache>()
                .AddScoped<IUserService, UserService>();
            var app = builder.Build();
            app.Run();
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Program.cs"];

        var model = new DiscoveryModel();

        var extractor = new DiRegistrationExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var registrations = model.Detections.OfType<DiRegistrationDetection>().ToArray();
        Assert.NotEmpty(registrations);

        var singleton = registrations.FirstOrDefault(r => r.Lifetime == "Singleton");
        Assert.NotNull(singleton);
        Assert.Equal("ICache", singleton.ServiceType);
    }
}
