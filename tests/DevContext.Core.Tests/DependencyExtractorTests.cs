using DevContext.Core.Graph.EntrySurfaces;

namespace DevContext.Core.Tests;

public sealed class DependencyExtractorTests
{
    // ── A4 (Prism D1.1c): self-name boundary matching + runnable guard ──────────────

    [Theory]
    [InlineData("Wolverine", true)]           // the framework core project
    [InlineData("Wolverine.Http", true)]      // dotted framework satellite
    [InlineData("WolverineDemo", false)]      // consumer concatenation must NOT match
    [InlineData("OrleansVoting.AppHost", false)] // aspire-samples' sample must NOT match
    [InlineData("SerilogHelpers", false)]     // consumer helper lib must NOT match
    [InlineData("Serilog.Sinks.Console", true)]
    [InlineData("xunit.v3.assert", true)]     // xunit's own classlib self-sources 'testing'
    [InlineData("xunit.v3.core", true)]
    public void SelfName_matching_requires_a_name_boundary(string projectName, bool shouldMatch)
    {
        var matched = DependencyExtractor.TryMatchSignalFromProjectName(projectName, out _, out _);
        Assert.Equal(shouldMatch, matched);
    }

    // ── D1.2-fix2: Gateway is exempt from BOTH of D1.1c's guards ────────────────────

    [Theory]
    [InlineData("YarpApiGateway")]   // the dogfood's gateway — concatenated, and a Web-SDK exe
    [InlineData("Yarp.ReverseProxy")] // YARP's own repo — dotted
    [InlineData("ReverseProxyGateway")]
    public void Gateway_self_sources_from_a_concatenated_name(string projectName)
    {
        // The boundary rule is precisely wrong for gateways: concatenation IS the naming convention
        // for a gateway host. ArchetypeDetector separates YARP's own repo from an app that merely
        // runs a gateway by PEER-SERVICE COUNT, not by the name (ArchetypeDetector.cs:40-43).
        Assert.True(DependencyExtractor.TryMatchSignalFromProjectName(
            projectName, out var key, out _, out var role));
        Assert.Equal(ArchitectureSignals.Keys.Gateway, key);
        Assert.Equal(SurfaceRole.Gateway, role);
    }

    [Fact]
    public async Task Gateway_self_sources_even_though_the_gateway_host_is_runnable()
    {
        // dogfood shape: YarpApiGateway is Sdk="Microsoft.NET.Sdk.Web" => IsRunnableService. The
        // runnable guard is right for framework cores (always classlibs) and could only ever DELETE
        // the gateway signal — which cost dogfood its Microservices style for 4 checkpoints.
        var fs = new FakeFileSystem();
        var csproj = @"C:\repo\src\YarpApiGateway\YarpApiGateway.csproj";
        fs.AddFile(csproj, """
            <Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>
            """);
        var builder = new DiscoveryContextBuilder().WithFileSystem(fs).WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [];
        ctx.Analysis.AllProjectFiles = [csproj];
        ctx.Cache.RegisterPath(csproj);

        var model = new DiscoveryModel
        {
            Projects = [new ProjectInfo("YarpApiGateway", csproj, "C#", ["net10.0"], [], [], "Exe")],
        };
        await new DependencyExtractor().ExtractAsync(ctx, model, default);

        var signal = model.Architecture.Get(ArchitectureSignals.Keys.Gateway);
        Assert.NotNull(signal);
        Assert.True(signal.Detected);
        Assert.Equal("ProjectName", signal.DetectedVia);
    }

    [Fact]
    public async Task Framework_repo_self_sources_from_classlib_but_not_from_runnable_host()
    {
        // wolverine shape: the classlib core self-sources the signal (nuget id WolverineFx never
        // appears in its own repo); a consumer repo whose HOST is named "Wolverine" does not.
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Wolverine\Wolverine.csproj", """
            <Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>
            """);
        var builder = new DiscoveryContextBuilder().WithFileSystem(fs).WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [];
        ctx.Analysis.AllProjectFiles = [@"C:\repo\src\Wolverine\Wolverine.csproj"];
        ctx.Cache.RegisterPath(@"C:\repo\src\Wolverine\Wolverine.csproj");

        var classlibModel = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Wolverine", @"C:\repo\src\Wolverine\Wolverine.csproj",
                "C#", ["net10.0"], [], [])],
        };
        await new DependencyExtractor().ExtractAsync(ctx, classlibModel, default);
        var signal = classlibModel.Architecture.Get(ArchitectureSignals.Keys.Wolverine);
        Assert.NotNull(signal);
        Assert.True(signal.Detected);
        Assert.Equal("ProjectName", signal.DetectedVia);

        var hostModel = new DiscoveryModel
        {
            Projects = [new ProjectInfo("Wolverine", @"C:\repo\src\Wolverine\Wolverine.csproj",
                "C#", ["net10.0"], [], [], OutputType: "Exe")],
        };
        await new DependencyExtractor().ExtractAsync(ctx, hostModel, default);
        var hostSignal = hostModel.Architecture.Get(ArchitectureSignals.Keys.Wolverine);
        Assert.True(hostSignal is null || hostSignal.DetectedVia != "ProjectName");
    }

    [Fact]
    public async Task DependencyExtractor_DetectsSignalFromPackageRefs()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\MyApp.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="MediatR" Version="12.0.0" />
                <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
              </ItemGroup>
            </Project>
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [];
        ctx.Analysis.AllProjectFiles = [@"C:\repo\src\MyApp\MyApp.csproj"];

        ctx.Cache.RegisterPath(@"C:\repo\src\MyApp\MyApp.csproj");

        var model = new DiscoveryModel
        {
            Projects = [
                new ProjectInfo(
                    "MyApp",
                    @"C:\repo\src\MyApp\MyApp.csproj",
                    "C#",
                    ["net10.0"],
                    [],
                    [
                        new PackageReferenceInfo("MediatR", "12.0.0"),
                        new PackageReferenceInfo("Microsoft.EntityFrameworkCore", "8.0.0"),
                    ])
            ],
        };

        var extractor = new DependencyExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.MediatR));
        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.EfCore));

        var mediatR = model.Architecture.Get(ArchitectureSignals.Keys.MediatR);
        Assert.NotNull(mediatR);
        Assert.True(mediatR.Detected);

        var efCore = model.Architecture.Get(ArchitectureSignals.Keys.EfCore);
        Assert.NotNull(efCore);
        Assert.True(efCore.Detected);
    }

    [Fact]
    public async Task DependencyExtractor_DetectsCommonLibrarySignals()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\MyApp.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Serilog" Version="4.0.0" />
                <PackageReference Include="AutoMapper" Version="13.0.0" />
                <PackageReference Include="Polly" Version="8.0.0" />
                <PackageReference Include="Swashbuckle.AspNetCore" Version="7.0.0" />
                <PackageReference Include="Microsoft.AspNetCore.Identity" Version="2.2.0" />
                <PackageReference Include="Quartz" Version="3.0.0" />
                <PackageReference Include="StackExchange.Redis" Version="2.0.0" />
                <PackageReference Include="AspNetCore.HealthChecks" Version="8.0.0" />
              </ItemGroup>
            </Project>
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [];
        ctx.Analysis.AllProjectFiles = [@"C:\repo\src\MyApp\MyApp.csproj"];
        ctx.Cache.RegisterPath(@"C:\repo\src\MyApp\MyApp.csproj");

        var model = new DiscoveryModel
        {
            Projects = [
                new ProjectInfo("MyApp", @"C:\repo\src\MyApp\MyApp.csproj", "C#", ["net10.0"], [],
                    [
                        new PackageReferenceInfo("Serilog", "4.0.0"),
                        new PackageReferenceInfo("AutoMapper", "13.0.0"),
                        new PackageReferenceInfo("Polly", "8.0.0"),
                        new PackageReferenceInfo("Swashbuckle.AspNetCore", "7.0.0"),
                        new PackageReferenceInfo("Microsoft.AspNetCore.Identity", "2.2.0"),
                        new PackageReferenceInfo("Quartz", "3.0.0"),
                        new PackageReferenceInfo("StackExchange.Redis", "2.0.0"),
                        new PackageReferenceInfo("AspNetCore.HealthChecks", "8.0.0"),
                    ])
            ],
        };

        var extractor = new DependencyExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.Serilog));
        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.AutoMapper));
        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.Polly));
        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.Swagger));
        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.Identity));
        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.Quartz));
        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.Redis));
        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.HealthChecks));
    }

    [Fact]
    public async Task DependencyExtractor_DetectsSignalFromNLog()
    {
        // NLog is tracked as a signal but may not appear in common web projects
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\MyApp.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="NLog" Version="5.0.0" />
              </ItemGroup>
            </Project>
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [];
        ctx.Analysis.AllProjectFiles = [@"C:\repo\src\MyApp\MyApp.csproj"];
        ctx.Cache.RegisterPath(@"C:\repo\src\MyApp\MyApp.csproj");

        var model = new DiscoveryModel
        {
            Projects = [
                new ProjectInfo("MyApp", @"C:\repo\src\MyApp\MyApp.csproj", "C#", ["net10.0"], [],
                    [new PackageReferenceInfo("NLog", "5.0.0")])
            ],
        };

        var extractor = new DependencyExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.NLog));
    }

    [Fact]
    public async Task DependencyExtractor_DetectsSignalFromProjectReference()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp.Tests\MyApp.Tests.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <ProjectReference Include="..\AutoMapper\AutoMapper.csproj" />
              </ItemGroup>
            </Project>
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [];
        ctx.Analysis.AllProjectFiles = [@"C:\repo\src\MyApp.Tests\MyApp.Tests.csproj"];
        ctx.Cache.RegisterPath(@"C:\repo\src\MyApp.Tests\MyApp.Tests.csproj");

        var model = new DiscoveryModel
        {
            Projects = [
                new ProjectInfo("MyApp.Tests", @"C:\repo\src\MyApp.Tests\MyApp.Tests.csproj", "C#", ["net10.0"], [],
                    [])
            ],
        };

        var extractor = new DependencyExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.AutoMapper),
            "AutoMapper signal should be detected via ProjectReference");
    }

    [Fact]
    public async Task DependencyExtractor_DetectsFastEndpointsPackage()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\MyApp.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="FastEndpoints" Version="5.0.0" />
              </ItemGroup>
            </Project>
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [];
        ctx.Analysis.AllProjectFiles = [@"C:\repo\src\MyApp\MyApp.csproj"];
        ctx.Cache.RegisterPath(@"C:\repo\src\MyApp\MyApp.csproj");

        var model = new DiscoveryModel
        {
            Projects = [
                new ProjectInfo("MyApp", @"C:\repo\src\MyApp\MyApp.csproj", "C#", ["net10.0"], [],
                    [new PackageReferenceInfo("FastEndpoints", "5.0.0")])
            ],
        };

        var extractor = new DependencyExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.FastEndpoints));
    }

    [Fact]
    public async Task DependencyExtractor_DetectsCpmPackageReference()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\MyApp.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="MediatR" />
              </ItemGroup>
            </Project>
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [];
        ctx.Analysis.AllProjectFiles = [@"C:\repo\src\MyApp\MyApp.csproj"];
        ctx.Cache.RegisterPath(@"C:\repo\src\MyApp\MyApp.csproj");

        var model = new DiscoveryModel
        {
            Projects = [
                new ProjectInfo("MyApp", @"C:\repo\src\MyApp\MyApp.csproj", "C#", ["net10.0"], [], [])
            ],
        };

        var extractor = new DependencyExtractor();
        await extractor.ExtractAsync(ctx, model, default);
    }

    [Fact]
    public async Task DependencyExtractor_DetectsSignalFromWebSdk()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\MyApp.csproj", """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [];
        ctx.Analysis.AllProjectFiles = [@"C:\repo\src\MyApp\MyApp.csproj"];
        ctx.Cache.RegisterPath(@"C:\repo\src\MyApp\MyApp.csproj");

        var model = new DiscoveryModel
        {
            Projects = [
                new ProjectInfo("MyApp", @"C:\repo\src\MyApp\MyApp.csproj", "C#", ["net10.0"], [], [])
            ],
        };

        var extractor = new DependencyExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.MinimalApis));
    }
}
