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

    // ── D1.1d — CliTool archetype (audit A3/B4, the GitVersion shape) ────────────────

    [Fact]
    public void CliTool_when_console_exe_has_tool_packaging_and_no_web_surface()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("GitVersion.App", @"C:\repo\src\GitVersion.App\GitVersion.App.csproj",
                    "C#", ["net10.0"], [@"..\GitVersion.Core\GitVersion.Core.csproj"], [],
                    OutputType: "Exe", IsToolPackaged: true),
                new ProjectInfo("GitVersion.Core", @"C:\repo\src\GitVersion.Core\GitVersion.Core.csproj",
                    "C#", ["net10.0"], [], []),
            ],
        };
        model.Types.TryAdd("GitVersion.Core.Calculator",
            PublicType("GitVersion.Core.Calculator", @"C:\repo\src\GitVersion.Core\Calculator.cs"));

        Assert.Equal(Archetype.CliTool, ArchetypeDetector.Detect(model, []));
    }

    [Fact]
    public void CliTool_when_console_exe_references_a_cli_parser()
    {
        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Tool", @"C:\repo\Tool\Tool.csproj", "C#", ["net10.0"], [],
                [new PackageReferenceInfo("Spectre.Console.Cli", "0.49.1")], OutputType: "Exe")],
        };

        Assert.Equal(Archetype.CliTool, ArchetypeDetector.Detect(model, []));
    }

    [Fact]
    public void App_not_CliTool_when_web_surface_exists_beside_a_tool_exe()
    {
        // bitwarden shape: a migrator utility packaged as a tool inside a web system stays App.
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Api", @"C:\repo\src\Api\Api.csproj", "C#", ["net10.0"], [], [], OutputType: "Exe"),
                new ProjectInfo("MsSqlMigratorUtility", @"C:\repo\util\MsSqlMigratorUtility\MsSqlMigratorUtility.csproj",
                    "C#", ["net10.0"], [], [], OutputType: "Exe", IsToolPackaged: true),
            ],
        };
        model.Architecture.Register(FeatureSignal.CreateDetected(
            ArchitectureSignals.Keys.Controllers, 1.0f, via: "PackageReference", "Microsoft.AspNetCore.Mvc"));
        ImmutableArray<EntryPoint> entries =
            [new EntryPoint(EntryPointKind.HttpEndpoint, "GET /x", NodeId.ForEntry("GET /x"))];

        Assert.Equal(Archetype.App, ArchetypeDetector.Detect(model, entries));
    }

    [Fact]
    public void Library_not_CliTool_when_aux_console_has_no_tool_evidence()
    {
        // Newtonsoft shape: an aux TestConsole without PackAsTool/parser evidence must not
        // drag the library into CliTool.
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Newtonsoft.Json", @"C:\repo\Src\Newtonsoft.Json\Newtonsoft.Json.csproj",
                    "C#", ["net10.0"], [], [], IsPackable: true),
                new ProjectInfo("Newtonsoft.Json.TestConsole", @"C:\repo\Src\Newtonsoft.Json.TestConsole\Newtonsoft.Json.TestConsole.csproj",
                    "C#", ["net10.0"], [@"..\Newtonsoft.Json\Newtonsoft.Json.csproj"], [], OutputType: "Exe"),
            ],
        };
        model.Types.TryAdd("Newtonsoft.Json.JsonConvert",
            PublicType("Newtonsoft.Json.JsonConvert", @"C:\repo\Src\Newtonsoft.Json\JsonConvert.cs"));

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

    // ── T8 — samples-only repos: the samples ARE the product ────────────────────────────────
    // aspire-samples shape: every non-test project under samples/, an Orleans-named SAMPLE
    // self-sourcing the framework signal, and every entry's provenance under samples/. Without
    // the SamplesAreTheProduct waiver this landed Library ("0 public types", no STYLE line).

    [Fact]
    public void Samples_only_repo_is_an_app_not_a_framework_library()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("OrleansVoting.AppHost", @"C:\repo\samples\OrleansVoting\OrleansVoting.AppHost\OrleansVoting.AppHost.csproj",
                    "C#", ["net10.0"], [], [], OutputType: "Exe"),
                new ProjectInfo("MetricsApp", @"C:\repo\samples\Metrics\MetricsApp\MetricsApp.csproj",
                    "C#", ["net10.0"], [], [], OutputType: "Exe"),
            ],
            SamplesAreTheProduct = true,
        };
        // A sample project named *Orleans* self-sources the framework signal — the repo is NOT Orleans.
        model.Architecture.Register(FeatureSignal.CreateDetected(
            ArchitectureSignals.Keys.Orleans, 0.7f, via: "ProjectName", "OrleansVoting.AppHost"));
        ImmutableArray<EntryPoint> entries =
        [
            new EntryPoint(EntryPointKind.HttpEndpoint, "GET /weather", NodeId.ForEntry("GET /weather"))
                { Provenance = @"samples/Metrics/MetricsApp/ClientApi.cs:12" },
        ];

        Assert.Equal(Archetype.App, ArchetypeDetector.Detect(model, entries));
    }

    [Fact]
    public void Library_with_sample_entries_stays_a_library_when_samples_are_not_the_product()
    {
        // The pre-T8 rule is unchanged for real libraries: sample entries don't flip the archetype.
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("MediatR", @"C:\repo\src\MediatR\MediatR.csproj",
                    "C#", ["net10.0"], [], [], IsPackable: true),
                new ProjectInfo("MediatR.Examples", @"C:\repo\samples\MediatR.Examples\MediatR.Examples.csproj",
                    "C#", ["net10.0"], [@"..\..\src\MediatR\MediatR.csproj"], [], OutputType: "Exe"),
            ],
        };
        model.Types.TryAdd("MediatR.Mediator", PublicType("MediatR.Mediator", @"C:\repo\src\MediatR\Mediator.cs"));
        ImmutableArray<EntryPoint> entries =
        [
            new EntryPoint(EntryPointKind.HttpEndpoint, "GET /demo", NodeId.ForEntry("GET /demo"))
                { Provenance = @"C:\repo\samples\MediatR.Examples\Demo.cs:8" },
        ];

        Assert.Equal(Archetype.Library, ArchetypeDetector.Detect(model, entries));
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
    public void Library_when_auxiliary_exe_references_the_library_transitively()
    {
        // A1 (Prism D1.1a) — the Newtonsoft.Json shape: TestConsole (Exe) references only
        // Newtonsoft.Json.Tests, which references the library. The auxiliary walk must follow
        // the in-solution reference chain (through the test project) instead of flipping to App.
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Newtonsoft.Json", @"C:\repo\Src\Newtonsoft.Json\Newtonsoft.Json.csproj",
                    "C#", ["net10.0"], [], [], IsPackable: true),
                new ProjectInfo("Newtonsoft.Json.Tests", @"C:\repo\Src\Newtonsoft.Json.Tests\Newtonsoft.Json.Tests.csproj",
                    "C#", ["net10.0"], [@"..\Newtonsoft.Json\Newtonsoft.Json.csproj"], []),
                new ProjectInfo("Newtonsoft.Json.TestConsole", @"C:\repo\Src\Newtonsoft.Json.TestConsole\Newtonsoft.Json.TestConsole.csproj",
                    "C#", ["net10.0"], [@"..\Newtonsoft.Json.Tests\Newtonsoft.Json.Tests.csproj"], [], OutputType: "Exe"),
            ],
        };
        model.Types.TryAdd("Newtonsoft.Json.JsonConvert",
            PublicType("Newtonsoft.Json.JsonConvert", @"C:\repo\Src\Newtonsoft.Json\JsonConvert.cs"));

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
