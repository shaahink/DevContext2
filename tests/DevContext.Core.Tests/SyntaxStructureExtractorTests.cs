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

    // ── WP1 (library support): XML doc summaries + extension-method capture ──

    private static async Task<DiscoveryModel> RunSingleFileAsync(string source)
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Demo.cs", source);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\Demo.cs"];

        var model = new DiscoveryModel();
        await new SyntaxStructureExtractor().ExtractAsync(ctx, model, default);
        return model;
    }

    [Fact]
    public async Task ExtensionMethod_IsFlaggedWithExtendedType()
    {
        var model = await RunSingleFileAsync("""
            namespace Demo;
            public static class MyExtensions {
                public static IServiceCollection AddFoo(this IServiceCollection services) => services;
                public static int Helper(int x) => x;
            }
            """);

        var type = model.Types["Demo.MyExtensions"];
        var addFoo = type.Methods.Single(m => m.Name == "AddFoo");
        var helper = type.Methods.Single(m => m.Name == "Helper");

        Assert.True(addFoo.IsExtension);
        Assert.Equal("IServiceCollection", addFoo.ExtendedType);

        // A static method without a `this` first parameter is NOT an extension method.
        Assert.False(helper.IsExtension);
        Assert.Null(helper.ExtendedType);
    }

    [Fact]
    public async Task XmlDocSummary_IsExtracted_ForTypeAndMethod()
    {
        var model = await RunSingleFileAsync("""
            namespace Demo;
            /// <summary>Base class for object validators.</summary>
            public abstract class AbstractValidator {
                /// <summary>
                /// Registers the foo service.
                /// </summary>
                public void AddFoo() { }
                public void Undocumented() { }
            }
            """);

        var type = model.Types["Demo.AbstractValidator"];
        Assert.Equal("Base class for object validators.", type.XmlDoc);

        var addFoo = type.Methods.Single(m => m.Name == "AddFoo");
        Assert.Equal("Registers the foo service.", addFoo.XmlDoc);

        // No doc comment -> null (not empty string).
        var undoc = type.Methods.Single(m => m.Name == "Undocumented");
        Assert.Null(undoc.XmlDoc);
    }
}
