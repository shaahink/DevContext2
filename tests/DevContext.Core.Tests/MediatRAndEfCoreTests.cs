namespace DevContext.Core.Tests;

public sealed class MediatRAndEfCoreTests
{
    [Fact]
    public async Task MediatRExtractor_DetectsIRequestHandler()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Handlers\GetProductHandler.cs", """
            namespace MyApp.Handlers;

            public sealed class GetProductHandler : IRequestHandler<GetProductQuery, Product>
            {
                public Task<Product> Handle(GetProductQuery request, CancellationToken ct)
                    => Task.FromResult(new Product());
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo")
            .WithSignal(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MediatR));
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Handlers\GetProductHandler.cs"];

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MediatR));

        var extractor = new MediatRExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var handlers = model.Detections.OfType<MediatRHandlerDetection>().ToArray();
        Assert.Single(handlers);
        Assert.Equal("GetProductQuery", handlers[0].RequestType);
        Assert.Equal("Product", handlers[0].ResponseType);
        Assert.Equal("GetProductHandler", handlers[0].HandlerType);
        Assert.Equal(MediatRKind.Command, handlers[0].Kind);
    }

    [Fact]
    public async Task EfCoreExtractor_DetectsDbContextAndDbSet()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Data\AppDbContext.cs", """
            namespace MyApp.Data;

            public sealed class AppDbContext : DbContext
            {
                public DbSet<Product> Products { get; set; }
                public DbSet<Order> Orders { get; set; }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo")
            .WithSignal(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.EfCore));
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Data\AppDbContext.cs"];

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.EfCore));

        var extractor = new EfCoreExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var entities = model.Detections.OfType<EfEntityDetection>().ToArray();
        Assert.Equal(2, entities.Length);

        Assert.Contains(entities, e => e.EntityType == "Product" && e.DbContextType == "AppDbContext");
        Assert.Contains(entities, e => e.EntityType == "Order" && e.DbContextType == "AppDbContext");
    }
}
