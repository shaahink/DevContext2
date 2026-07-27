using DevContext.Core.Graph;
using DevContext.Core.Graph2;

namespace DevContext.Core.Tests;

public sealed class CallGraphAndSourceBodyTests
{
    [Fact]
    public async Task CallGraphBinder_DiscoversBasicInvocations()
    {
        // Batch A contract: edges come from BodyFacts resolved through the SymbolTable — an
        // interface receiver routes to its sole in-solution implementor; out-of-solution
        // receivers produce NO edge (never a guessed one).
        var fs = new FakeFileSystem();
        var file = @"C:\repo\src\MyApp\Services\ProductService.cs";
        fs.AddFile(file, """
            namespace MyApp.Services;

            public interface IProductRepository
            {
                Product? GetById(int id);
            }

            public sealed class ProductRepository : IProductRepository
            {
                public Product? GetById(int id) => null;
            }

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

        ctx.Analysis.AllSourceFiles = [file];
        ctx.Analysis.FocusPoints = [];

        var model = new DiscoveryModel();
        await new SyntaxStructureExtractor().ExtractAsync(ctx, model, default);
        await new BodyFactsExtractor().ExtractAsync(ctx, model, default);

        var symbols = new SymbolTable(model.OrderedTypes, null, ctx.Analysis.AllBodyFacts);
        CallGraphBinder.Bind(ctx, model, symbols, ctx.Analysis.AllBodyFacts,
            new NoiseFilter(new ProjectClassifier(model.Projects)), default);

        Assert.Contains(model.CallEdges, e =>
            e.CallerType == "MyApp.Services.ProductService"
            && e.CallerMethod == "GetProduct"
            && e.CalleeType == "MyApp.Services.ProductRepository"
            && e.CalleeMethod == "GetById");
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
