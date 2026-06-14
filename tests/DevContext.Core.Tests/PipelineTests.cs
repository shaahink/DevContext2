namespace DevContext.Core.Tests;

public sealed class PipelineTests
{
    [Fact]
    public async Task RunAsync_WithDryRun_ReturnsPlan()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Program.cs", "");
        fs.AddFile(@"src\MyApp.csproj", "<Project />");

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath("src")
            .WithOptions(new ExtractionOptions { DryRun = true });
        var built = builder.BuildWithRecording();
        var ctx = built.Context;
        var observer = built.Recording;

        var extractors = new List<IDiscoveryExtractor>
        {
            new FileTreeExtractor(),
            new SolutionDiscoveryExtractor(),
            new ProjectStructureExtractor()
        };

        var pipeline = new DiscoveryPipeline(
            extractors, [], [], new Dictionary<string, IContextRenderer>
            {
                ["markdown"] = new TestMarkdownRenderer(),
                ["json"] = new TestJsonRenderer()
            },
            new NullLogger<DiscoveryPipeline>());

        var result = await pipeline.RunAsync(ctx);

        Assert.Contains("Dry Run Plan", result.Content);
    }

    [Fact]
    public async Task RunAsync_WithRealExtraction_Completes()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Program.cs", "class Program { static void Main() {} }");
        fs.AddFile(@"src\MyApp.csproj", @"
<Project>
  <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""MediatR"" Version=""12.0.0"" />
  </ItemGroup>
</Project>");
        fs.AddFile(@"src\MyApp.sln", @"
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""MyApp"", ""MyApp.csproj"", ""{GUID}""
");

        var loggerFactory = LoggerFactory.Create(b => { });
        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath("src");
        var built = builder.BuildWithRecording();
        var ctx = built.Context;
        var observer = built.Recording;

        var extractors = new List<IDiscoveryExtractor>
        {
            new FileTreeExtractor(),
            new SolutionDiscoveryExtractor(),
            new ProjectStructureExtractor()
        };

        var pipeline = new DiscoveryPipeline(
            extractors, [], [], new Dictionary<string, IContextRenderer>
            {
                ["markdown"] = new TestMarkdownRenderer(),
                ["json"] = new TestJsonRenderer()
            },
            loggerFactory.CreateLogger<DiscoveryPipeline>());

        var result = await pipeline.RunAsync(ctx);

        Assert.NotNull(result.Content);
        Assert.True(result.EstimatedTokens > 0);
        Assert.Contains("PipelineStarted", observer.Events[0]);
        Assert.Contains("PipelineCompleted", observer.Events[^1]);
    }

    [Fact]
    public async Task RunPruningAsync_ReportsPerPrunerBeforeCounts()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Program.cs", "class Program { static void Main() {} }");
        fs.AddFile(@"src\MyApp.csproj", @"
<Project>
  <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
</Project>");

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath("src");
        var built = builder.BuildWithRecording();
        var ctx = built.Context;
        var observer = built.Recording;

        var model = new DiscoveryModel();
        for (var i = 0; i < 10; i++)
        {
            var id = $"Type{i}";
            model.Types.TryAdd(id, new TypeDiscovery
            {
                Id = id,
                Name = $"Type{i}",
                Namespace = "Test",
                FilePath = $"C:\\src\\Type{i}.cs",
                Kind = TypeKind.Class,
                Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
                Layer = ArchitectureLayer.Unknown,
            });
        }

        var pruners = new List<IPruner>
        {
            new TestPruner("PrunerA", _ => { }),          // prunes none
            new TestPruner("PrunerB", m =>                 // prunes 3
            {
                var count = 0;
                foreach (var t in m.Types.Values)
                    if (!t.IsHardExcluded && count++ < 3) t.IsHardExcluded = true;
            }),
            new TestPruner("PrunerC", _ => { }),          // prunes none
        };

        var pipeline = new DiscoveryPipeline(
            [], pruners, [], new Dictionary<string, IContextRenderer>
            {
                ["markdown"] = new TestMarkdownRenderer(),
            },
            new NullLogger<DiscoveryPipeline>());

        var stageField = typeof(DiscoveryPipeline)
            .GetMethod("RunScoringAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.NotNull(stageField);
        var task = (Task)stageField.Invoke(pipeline, [ctx, model, CancellationToken.None])!;
        await task;

        var prunerEvents = observer.Events
            .Where(e => e.StartsWith("PrunerCompleted:"))
            .ToList();

        Assert.Equal(3, prunerEvents.Count);
        Assert.Contains("PrunerA", prunerEvents[0]);
        Assert.Contains("PrunerB", prunerEvents[1]);
        Assert.Contains("PrunerC", prunerEvents[2]);
    }

    [Fact]
    public void GetOrder_UsesAttribute()
    {
        var early = new FileTreeExtractor();
        var normal = new ProjectStructureExtractor();

        var earlyOrder = DiscoveryPipeline.GetOrder(early);
        var normalOrder = DiscoveryPipeline.GetOrder(normal);

        Assert.True(earlyOrder < normalOrder);
    }
}
