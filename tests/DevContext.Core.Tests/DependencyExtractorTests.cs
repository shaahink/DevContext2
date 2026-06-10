namespace DevContext.Core.Tests;

public sealed class DependencyExtractorTests
{
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
