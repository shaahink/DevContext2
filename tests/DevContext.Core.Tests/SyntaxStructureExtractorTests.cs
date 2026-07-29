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

    /// <summary>G8.2 — pins every rule the base-list classifier obeys, because the resolution behind
    /// it was rewritten from a full tree scan per base entry into one per-file index. Each assertion
    /// below discriminates a way the index could be built WRONG (miss enums, drop document order,
    /// mishandle a generic base, or — the one that would look like an improvement — become global).
    /// It drives the extractor's public surface only, so it survives the next refactor too.</summary>
    [Fact]
    public async Task BaseListClassification_PinsResolutionRules()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Demo.cs", """
            namespace Demo;

            public class LocalBase { }
            public interface ILocal { }
            public class Gen<T> { }
            public interface IGen<T> { }
            public enum Colour { Red }

            // Declared FIRST, so document order must make this the winner for the bare name "Dup".
            public class Outer { public class Dup { } }
            public interface Dup { }

            public class DerivesLocal : LocalBase { }
            public class ImplementsLocal : ILocal { }
            public class DerivesGeneric : Gen<int> { }
            public class ImplementsGeneric : IGen<int> { }
            public class DerivesQualified : global::System.IEquatable<DerivesQualified> { }
            public class TakesDup : Dup { }
            public class NamedAfterEnum : Colour { }
            public class DerivesUnknown : NotInThisFile { }
            public class ImplementsUnknown : INotInThisFile { }
            public class DerivesOtherFileInterface : Sneaky { }
            """);
        // A second file whose interface does NOT start with I. If resolution ever became global
        // instead of per-file, "Sneaky" would resolve to an interface and flip sides below.
        fs.AddFile(@"C:\repo\src\Other.cs", """
            namespace Demo;
            public interface Sneaky { }
            """);

        var builder = new DiscoveryContextBuilder().WithFileSystem(fs).WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\Demo.cs", @"C:\repo\src\Other.cs"];

        var model = new DiscoveryModel();
        await new SyntaxStructureExtractor().ExtractAsync(ctx, model, default);

        static void AssertBase(DiscoveryModel m, string id, string name)
        {
            var t = m.Types[id];
            Assert.Contains(name, t.BaseTypes);
            Assert.DoesNotContain(name, t.ImplementedInterfaces);
        }
        static void AssertInterface(DiscoveryModel m, string id, string name)
        {
            var t = m.Types[id];
            Assert.Contains(name, t.ImplementedInterfaces);
            Assert.DoesNotContain(name, t.BaseTypes);
        }

        // Declared in the same file: the DECLARATION decides, not the name.
        AssertBase(model, "Demo.DerivesLocal", "LocalBase");
        AssertInterface(model, "Demo.ImplementsLocal", "ILocal");

        // Generic base: the key is the name up to the first '<', and the argument list is kept.
        AssertBase(model, "Demo.DerivesGeneric", "Gen<int>");
        AssertInterface(model, "Demo.ImplementsGeneric", "IGen<int>");

        // Qualified generic resolves to nothing (no declaration is named "global::System.IEquatable"),
        // so the naming convention decides — and "global::…" does not start with I. This is the exact
        // shape that used to cost a full tree walk with no early exit.
        AssertBase(model, "Demo.DerivesQualified", "global::System.IEquatable<DerivesQualified>");

        // Two declarations share the short name "Dup"; the FIRST in document order (the nested class)
        // wins, so this is a base type and not an interface.
        AssertBase(model, "Demo.TakesDup", "Dup");

        // The index must hold every BaseTypeDeclarationSyntax, enums included — build it from type
        // declarations only and this name resolves to nothing and flips on the naming convention.
        AssertBase(model, "Demo.NamedAfterEnum", "Colour");

        // Not declared in this file: the naming convention is the fallback, both ways.
        AssertBase(model, "Demo.DerivesUnknown", "NotInThisFile");
        AssertInterface(model, "Demo.ImplementsUnknown", "INotInThisFile");

        // Declared in ANOTHER file: resolution is per-file, so the convention still decides.
        AssertBase(model, "Demo.DerivesOtherFileInterface", "Sneaky");
    }
}
