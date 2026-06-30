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

    [Fact]
    public void App_when_win_exe_references_internal_library_projects()
    {
        // W5: a desktop WinExe that references internal library projects (Files.App → Files.Core)
        // must NOT be classified as an auxiliary sample — it IS the product.
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Files.Core", @"C:\repo\src\Files.Core\Files.Core.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("Files.App", @"C:\repo\src\Files.App\Files.App.csproj", "C#", ["net10.0"],
                    [@"..\src\Files.Core\Files.Core.csproj"], [], OutputType: "WinExe"),
            ],
        };
        model.Types.TryAdd("Files.Core.Service", PublicType("Files.Core.Service", @"C:\repo\src\Files.Core\Service.cs"));

        Assert.Equal(Archetype.App, ArchetypeDetector.Detect(model, []));
    }

    [Fact]
    public void App_when_ui_entry_points_exist()
    {
        // W5: UiEntry kind entries make the archetype App, just like HTTP/Message entries do.
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Desktop", @"C:\repo\Desktop\Desktop.csproj", "C#", ["net10.0"], [], [], OutputType: "WinExe")],
        };
        ImmutableArray<EntryPoint> entries =
            [new EntryPoint(EntryPointKind.UiEntry, "MainWindow", NodeId.ForEntry("ui:MainWindow"))];

        Assert.Equal(Archetype.App, ArchetypeDetector.Detect(model, entries));
    }
}
