namespace DevContext.Core.Tests;

public sealed class CompressorTests
{
    [Fact]
    public async Task TrivialMemberCompressor_RemovesAutoPropertiesAndEmptyCtors()
    {
        var model = new DiscoveryModel();
        model.Types.TryAdd("MyApp.Models.Product", new TypeDiscovery
        {
            Id = "MyApp.Models.Product",
            Name = "Product",
            Namespace = "MyApp.Models",
            FilePath = @"C:\repo\src\MyApp\Models\Product.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
            Methods = [
                new MethodSignature(".ctor", "void", [], [],
                    Microsoft.CodeAnalysis.Accessibility.Public, false, false),
                new MethodSignature("ToString", "string", [], [],
                    Microsoft.CodeAnalysis.Accessibility.Public, false, false),
                new MethodSignature("CalculateTotal", "decimal", ["decimal", "decimal"], ["price", "qty"],
                    Microsoft.CodeAnalysis.Accessibility.Public, false, false),
            ],
            Properties = [
                new PropertySignature("Id", "int",
                    Microsoft.CodeAnalysis.Accessibility.Public, false, false, true, true),
                new PropertySignature("Name", "string",
                    Microsoft.CodeAnalysis.Accessibility.Public, false, true, true, false),
                new PropertySignature("ReadOnlyProp", "string",
                    Microsoft.CodeAnalysis.Accessibility.Public, false, true, true, false),
            ],
        });

        var options = new CompressionOptions(8000, 3000);
        var compressor = new TrivialMemberCompressor();
        var result = await compressor.CompressAsync(model, options, default);

        var product = model.Types["MyApp.Models.Product"];
        Assert.DoesNotContain(product.Methods, m => m.Name is ".ctor" or "ToString" or "Equals" or "GetHashCode");
        Assert.Contains(product.Methods, m => string.Equals(m.Name, "CalculateTotal", StringComparison.Ordinal));

        Assert.DoesNotContain(product.Properties, p => string.Equals(p.Name, "Id", StringComparison.Ordinal));
        Assert.Contains(product.Properties, p => string.Equals(p.Name, "Name", StringComparison.Ordinal));
        Assert.Contains(product.Properties, p => string.Equals(p.Name, "ReadOnlyProp", StringComparison.Ordinal));

        Assert.NotEmpty(result.Notes);
    }

    [Fact]
    public async Task BoilerplateCompressor_PrunesDesignerFiles()
    {
        var model = new DiscoveryModel();
        model.Types.TryAdd("MyApp.Designer.DesignerType", new TypeDiscovery
        {
            Id = "MyApp.Designer.DesignerType",
            Name = "DesignerType",
            Namespace = "MyApp.Designer",
            FilePath = @"C:\repo\src\MyApp\Some.Designer.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Unknown,
        });
        model.Types.TryAdd("MyApp.Services.RealService", new TypeDiscovery
        {
            Id = "MyApp.Services.RealService",
            Name = "RealService",
            Namespace = "MyApp.Services",
            FilePath = @"C:\repo\src\MyApp\Services\RealService.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });

        var options = new CompressionOptions(8000, 3000);
        var compressor = new BoilerplateCompressor();
        var result = await compressor.CompressAsync(model, options, default);

        Assert.True(model.Types["MyApp.Designer.DesignerType"].IsHardExcluded);
        Assert.False(model.Types["MyApp.Services.RealService"].IsHardExcluded);
        Assert.NotEmpty(result.Notes);
    }

    [Fact]
    public async Task BoilerplateCompressor_PrunesDiExtensionTypes()
    {
        var model = new DiscoveryModel();
        model.Types.TryAdd("MyApp.Startup.ServiceExtensions", new TypeDiscovery
        {
            Id = "MyApp.Startup.ServiceExtensions",
            Name = "ServiceExtensions",
            Namespace = "MyApp.Startup",
            FilePath = @"C:\repo\src\MyApp\Startup\ServiceExtensions.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Unknown,
            Methods = [
                new MethodSignature("AddCorsServices", "IServiceCollection", ["IServiceCollection"], ["services"],
                    Microsoft.CodeAnalysis.Accessibility.Public, true, false),
                new MethodSignature("AddAuthServices", "IServiceCollection", ["IServiceCollection"], ["services"],
                    Microsoft.CodeAnalysis.Accessibility.Public, true, false),
            ],
        });

        var options = new CompressionOptions(8000, 3000);
        var compressor = new BoilerplateCompressor();
        var result = await compressor.CompressAsync(model, options, default);

        Assert.True(model.Types["MyApp.Startup.ServiceExtensions"].IsHardExcluded);
    }

    [Fact]
    public async Task StructuralDeduplicator_GroupsNearIdenticalTypes()
    {
        var model = new DiscoveryModel();

        var baseMethods = new[]
        {
            new MethodSignature("GetId", "int", [], [],
                Microsoft.CodeAnalysis.Accessibility.Public, false, false),
            new MethodSignature("SetName", "void", ["string"], ["name"],
                Microsoft.CodeAnalysis.Accessibility.Public, false, false),
        };

        model.Types.TryAdd("MyApp.Models.Product", new TypeDiscovery
        {
            Id = "MyApp.Models.Product",
            Name = "Product",
            Namespace = "MyApp.Models",
            FilePath = @"C:\repo\src\MyApp\Models\Product.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
            Methods = [.. baseMethods],
            Properties = [
                new PropertySignature("Id", "int",
                    Microsoft.CodeAnalysis.Accessibility.Public, false, false, true, true),
            ],
        });

        model.Types.TryAdd("MyApp.Models.Category", new TypeDiscovery
        {
            Id = "MyApp.Models.Category",
            Name = "Category",
            Namespace = "MyApp.Models",
            FilePath = @"C:\repo\src\MyApp\Models\Category.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
            Methods = [.. baseMethods],
            Properties = [
                new PropertySignature("Id", "int",
                    Microsoft.CodeAnalysis.Accessibility.Public, false, false, true, true),
            ],
        });

        model.Types.TryAdd("MyApp.Services.RealService", new TypeDiscovery
        {
            Id = "MyApp.Services.RealService",
            Name = "RealService",
            Namespace = "MyApp.Services",
            FilePath = @"C:\repo\src\MyApp\Services\RealService.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            Methods = [
                new MethodSignature("DoWork", "void", [], [],
                    Microsoft.CodeAnalysis.Accessibility.Public, false, false),
            ],
            Properties = [],
        });

        var options = new CompressionOptions(8000, 3000);
        var compressor = new StructuralDeduplicator();
        var result = await compressor.CompressAsync(model, options, default);

        var pruned = model.Types.Values.Count(t => t.IsHardExcluded);
        Assert.Equal(1, pruned);

        var category = model.Types["MyApp.Models.Category"];
        Assert.Contains(category.Tags, t => t.StartsWith("similar-types:", StringComparison.Ordinal));

        Assert.False(model.Types["MyApp.Services.RealService"].IsHardExcluded);
    }

    [Fact]
    public async Task NamespaceGrouper_GroupsTypesByNamespace()
    {
        var model = new DiscoveryModel();
        model.Types.TryAdd("MyApp.Models.Product", new TypeDiscovery
        {
            Id = "MyApp.Models.Product",
            Name = "Product",
            Namespace = "MyApp.Models",
            FilePath = "Product.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Types.TryAdd("MyApp.Models.Order", new TypeDiscovery
        {
            Id = "MyApp.Models.Order",
            Name = "Order",
            Namespace = "MyApp.Models",
            FilePath = "Order.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Types.TryAdd("MyApp.Services.OrderService", new TypeDiscovery
        {
            Id = "MyApp.Services.OrderService",
            Name = "OrderService",
            Namespace = "MyApp.Services",
            FilePath = "OrderService.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });

        var options = new CompressionOptions(8000, 3000);
        var compressor = new NamespaceGrouper();
        var result = await compressor.CompressAsync(model, options, default);

        var product = model.Types["MyApp.Models.Product"];
        var order = model.Types["MyApp.Models.Order"];
        var svc = model.Types["MyApp.Services.OrderService"];

        Assert.Contains(product.Tags, t => string.Equals(t, "ns-group:MyApp.Models", StringComparison.Ordinal));
        Assert.Contains(order.Tags, t => string.Equals(t, "ns-group:MyApp.Models", StringComparison.Ordinal));
        Assert.Contains(svc.Tags, t => string.Equals(t, "ns-group:MyApp.Services", StringComparison.Ordinal));

        Assert.Contains(result.Notes, n => n.Contains("MyApp.Models", StringComparison.Ordinal));
        Assert.Contains(result.Notes, n => n.Contains("MyApp.Services", StringComparison.Ordinal));
    }

    [Fact]
    public async Task LlmFriendlyFormatter_NormalizesSourceBody()
    {
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
            SourceBody = """
                /// <summary>Handles product-related operations</summary>
                public class ProductService
                {
                \tpublic int Id { get; set; }
                }
                """,
        });

        var options = new CompressionOptions(8000, 3000);
        var compressor = new LlmFriendlyFormatter();
        var result = await compressor.CompressAsync(model, options, default);

        var type = model.Types["MyApp.Services.ProductService"];
        Assert.NotNull(type.SourceBody);
        Assert.Contains("~", type.SourceBody, StringComparison.Ordinal);
        Assert.Contains("<summary>Handles product-related operations</summary>", type.SourceBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AggressiveTruncator_HardTruncatesLongBodies()
    {
        var model = new DiscoveryModel();
        model.Types.TryAdd("MyApp.Services.LargeService", new TypeDiscovery
        {
            Id = "MyApp.Services.LargeService",
            Name = "LargeService",
            Namespace = "MyApp.Services",
            FilePath = @"C:\repo\src\MyApp\Services\LargeService.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            SourceBody = string.Join('\n', Enumerable.Range(0, 100).Select(i => $"line {i}")),
        });

        var options = new CompressionOptions(8000, 30);
        var compressor = new AggressiveTruncator();
        var result = await compressor.CompressAsync(model, options, default);

        var type = model.Types["MyApp.Services.LargeService"];
        Assert.NotNull(type.SourceBody);
        Assert.True(type.SourceBody.Length <= 80);

        Assert.NotEmpty(result.Notes);
    }
}
