using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

public sealed class LibrarySurfaceBuilderTests
{
    private static MethodSignature M(string name, bool isStatic = false) =>
        new(name, "void", [], [], Microsoft.CodeAnalysis.Accessibility.Public, isStatic, false);

    [Fact]
    public void Groups_public_types_and_surfaces_extension_points()
    {
        var model = new DiscoveryModel
        {
            Projects = [
                new ProjectInfo("AutoMapper", @"C:\repo\src\AutoMapper\AutoMapper.csproj",
                    "C#", ["net10.0"], [], [], OutputType: "Library", IsPackable: true),
                new ProjectInfo("AutoMapper.Tests", @"C:\repo\test\AutoMapper.Tests\AutoMapper.Tests.csproj",
                    "C#", ["net10.0"], [], []),
            ],
        };
        model.Types.TryAdd("AutoMapper.Mapper", new TypeDiscovery
        {
            Id = "AutoMapper.Mapper", Name = "Mapper", Namespace = "AutoMapper",
            FilePath = @"C:\repo\src\AutoMapper\Mapper.cs", Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public, Layer = ArchitectureLayer.Application,
            Methods = [M("Map"), M("get_ConfigurationProvider")],
        });
        model.Types.TryAdd("AutoMapper.DI.ServiceCollectionExtensions", new TypeDiscovery
        {
            Id = "AutoMapper.DI.ServiceCollectionExtensions", Name = "ServiceCollectionExtensions",
            Namespace = "AutoMapper.DI", FilePath = @"C:\repo\src\AutoMapper\DI\Ext.cs", Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public, Layer = ArchitectureLayer.Application,
            Methods = [M("AddAutoMapper", isStatic: true)],
        });
        model.Types.TryAdd("AutoMapper.Tests.MapperTests", new TypeDiscovery
        {
            Id = "AutoMapper.Tests.MapperTests", Name = "MapperTests", Namespace = "AutoMapper.Tests",
            FilePath = @"C:\repo\test\AutoMapper.Tests\MapperTests.cs", Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public, Layer = ArchitectureLayer.Application,
            Methods = [M("Maps_correctly")],
        });

        var surface = LibrarySurfaceBuilder.Build(model);

        Assert.Equal(new[] { "AutoMapper", "AutoMapper.DI" }, surface.Groups.Select(g => g.Namespace));
        var mapper = surface.Groups.Single(g => g.Namespace == "AutoMapper").Types.Single();
        Assert.Equal("Mapper", mapper.Name);
        Assert.Equal(new[] { "Map" }, mapper.Members);                 // accessor filtered out
        Assert.Contains("ServiceCollectionExtensions.AddAutoMapper", surface.ExtensionPoints);
        Assert.DoesNotContain(surface.Groups, g => g.Namespace == "AutoMapper.Tests"); // test project excluded
    }

    [Fact]
    public void Global_namespace_type_groups_under_its_folder_not_the_literal_global()
    {
        // A public type with no namespace is keyed Namespace="global" (SyntaxStructureExtractor). T2.7
        // requires the rendered group label to fall back to the top folder — never the literal "global".
        var model = new DiscoveryModel
        {
            Projects = [
                new ProjectInfo("Widgets", @"C:\repo\src\Widgets\Widgets.csproj",
                    "C#", ["net10.0"], [], [], OutputType: "Library", IsPackable: true),
            ],
        };
        model.Types.TryAdd("global.WidgetFactory", new TypeDiscovery
        {
            Id = "global.WidgetFactory", Name = "WidgetFactory", Namespace = "global",
            FilePath = @"C:\repo\src\Widgets\Factories\WidgetFactory.cs", Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public, Layer = ArchitectureLayer.Application,
            Methods = [M("Create")],
        });

        var surface = LibrarySurfaceBuilder.Build(model);

        Assert.DoesNotContain(surface.Groups, g => g.Namespace == "global"); // never the sentinel
        Assert.Contains(surface.Groups, g => g.Namespace == "Factories");     // folder fallback
    }
}
