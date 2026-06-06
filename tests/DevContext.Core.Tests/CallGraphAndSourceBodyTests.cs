using DevContext.Core.Extractors.Generic;
using DevContext.Core.Extractors.Specific;

namespace DevContext.Core.Tests;

public sealed class CallGraphAndSourceBodyTests
{
    [Fact]
    public async Task CallGraphExtractor_DiscoversBasicInvocations()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Services\ProductService.cs", """
            namespace MyApp.Services;

            public sealed class ProductService
            {
                private readonly IProductRepository _repo;

                public ProductService(IProductRepository repo) => _repo = repo;

                public Product? GetProduct(int id)
                {
                    return _repo.GetById(id);
                }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Services\ProductService.cs"];
        ctx.Analysis.FocusPoints = [];

        var model = new DiscoveryModel();
        model.Types.TryAdd("MyApp.Services.ProductService", new TypeDiscovery
        {
            Id = "MyApp.Services.ProductService",
            Name = "ProductService",
            Namespace = "MyApp.Services",
            FilePath = @"C:\repo\src\MyApp\Services\ProductService.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            Methods = [
                new MethodSignature(".ctor", "void", ["IProductRepository"], ["repo"],
                    Microsoft.CodeAnalysis.Accessibility.Public, false, false),
                new MethodSignature("GetProduct", "Product?", ["int"], ["id"],
                    Microsoft.CodeAnalysis.Accessibility.Public, false, false),
            ],
        });

        var extractor = new CallGraphExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.NotEmpty(model.CallEdges);
    }

    [Fact]
    public async Task SourceBodyExtractor_PopulatesSourceBodyForTypes()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Models\Product.cs", """
            namespace MyApp.Models;

            public sealed class Product
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Models\Product.cs"];

        var model = new DiscoveryModel();
        var type = new TypeDiscovery
        {
            Id = "MyApp.Models.Product",
            Name = "Product",
            Namespace = "MyApp.Models",
            FilePath = @"C:\repo\src\MyApp\Models\Product.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        };
        model.Types.TryAdd("MyApp.Models.Product", type);

        var extractor = new SourceBodyExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.NotNull(type.SourceBody);
        Assert.Contains("class Product", type.SourceBody);
        Assert.Contains("Id", type.SourceBody);
    }
}
