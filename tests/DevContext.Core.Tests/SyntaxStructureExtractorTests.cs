using DevContext.Core.Extractors.Generic;
using DevContext.Core.Extractors.Specific;

namespace DevContext.Core.Tests;

public sealed class SyntaxStructureExtractorTests
{
    [Fact]
    public async Task SyntaxStructureExtractor_DiscoversTypesFromCsFiles()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Models\Product.cs", """
            namespace MyApp.Models;

            public sealed class Product
            {
                public int Id { get; set; }
                public string Name { get; set; }

                public string GetDisplayName() => Name;
            }
            """);
        fs.AddFile(@"C:\repo\src\MyApp\Services\IProductRepository.cs", """
            namespace MyApp.Services;

            public interface IProductRepository
            {
                Product? GetById(int id);
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [
            @"C:\repo\src\MyApp\Models\Product.cs",
            @"C:\repo\src\MyApp\Services\IProductRepository.cs",
        ];

        var model = new DiscoveryModel();

        var extractor = new SyntaxStructureExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.Equal(2, model.Types.Count);

        Assert.True(model.Types.ContainsKey("MyApp.Models.Product"));
        var product = model.Types["MyApp.Models.Product"];
        Assert.Equal("Product", product.Name);
        Assert.Equal(TypeKind.Class, product.Kind);
        Assert.Equal(2, product.Properties.Length);
        Assert.Single(product.Methods);

        Assert.True(model.Types.ContainsKey("MyApp.Services.IProductRepository"));
        var repo = model.Types["MyApp.Services.IProductRepository"];
        Assert.Equal("IProductRepository", repo.Name);
        Assert.Equal(TypeKind.Interface, repo.Kind);
        Assert.Single(repo.Methods);
    }

    [Fact]
    public async Task SyntaxStructureExtractor_DetectsControllerBase_SetsControllerSignal()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Controllers\ProductsController.cs", """
            namespace MyApp.Controllers;
            public sealed class ProductsController : ControllerBase
            {
                public IActionResult Get() => Ok();
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Controllers\ProductsController.cs"];

        var model = new DiscoveryModel();

        var extractor = new SyntaxStructureExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.Controllers));
    }
}
