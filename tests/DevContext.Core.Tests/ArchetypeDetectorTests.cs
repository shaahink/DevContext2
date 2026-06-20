using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

public sealed class ArchetypeDetectorTests
{
    private static TypeDiscovery PublicType(string id, string file) => new()
    {
        Id = id, Name = id, Namespace = "Lib", FilePath = file,
        Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
        Layer = ArchitectureLayer.Application,
    };

    [Fact]
    public void App_when_http_entry_points_exist()
    {
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Api", @"C:\repo\Api\Api.csproj", "C#", ["net10.0"], [], [], OutputType: "Exe")],
        };
        ImmutableArray<EntryPoint> entries =
            [new EntryPoint(EntryPointKind.HttpEndpoint, "GET /x", NodeId.ForEntry("GET /x"))];

        Assert.Equal(Archetype.App, ArchetypeDetector.Detect(model, entries));
    }

    [Fact]
    public void Library_when_packable_with_public_surface_and_no_entries()
    {
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("AutoMapper", @"C:\repo\src\AutoMapper\AutoMapper.csproj",
                "C#", ["net10.0"], [], [], OutputType: "Library", IsPackable: true)],
        };
        model.Types.TryAdd("Lib.Mapper", PublicType("Lib.Mapper", @"C:\repo\src\AutoMapper\Mapper.cs"));

        Assert.Equal(Archetype.Library, ArchetypeDetector.Detect(model, []));
    }

    [Fact]
    public void App_when_executable_even_without_entries()
    {
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Tool", @"C:\repo\Tool\Tool.csproj", "C#", ["net10.0"], [], [], OutputType: "Exe")],
        };
        model.Types.TryAdd("Tool.Program", PublicType("Tool.Program", @"C:\repo\Tool\Program.cs"));

        Assert.Equal(Archetype.App, ArchetypeDetector.Detect(model, []));
    }

    [Fact]
    public void Library_when_auxiliary_exe_samples_reference_the_library()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("AutoMapper", @"C:\repo\src\AutoMapper\AutoMapper.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("Benchmark", @"C:\repo\bench\Benchmark.csproj", "C#", ["net10.0"],
                    [@"..\src\AutoMapper\AutoMapper.csproj"], [], OutputType: "Exe"),
                new ProjectInfo("TestApp", @"C:\repo\sample\TestApp.csproj", "C#", ["net10.0"],
                    [@"..\src\AutoMapper\AutoMapper.csproj"], [], OutputType: "Exe"),
            ],
        };
        model.Types.TryAdd("AutoMapper.Mapper", PublicType("AutoMapper.Mapper", @"C:\repo\src\AutoMapper\Mapper.cs"));

        Assert.Equal(Archetype.Library, ArchetypeDetector.Detect(model, []));
    }

    [Fact]
    public void App_when_standalone_exe_does_not_reference_the_library()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Core", @"C:\repo\src\Core\Core.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("Worker", @"C:\repo\src\Worker\Worker.csproj", "C#", ["net10.0"], [], [], OutputType: "Exe"),
            ],
        };
        model.Types.TryAdd("Core.Thing", PublicType("Core.Thing", @"C:\repo\src\Core\Thing.cs"));

        Assert.Equal(Archetype.App, ArchetypeDetector.Detect(model, []));
    }
}
