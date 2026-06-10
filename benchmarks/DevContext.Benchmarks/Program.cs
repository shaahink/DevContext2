using System.Collections.Immutable;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using DevContext.Core.Analysis;
using DevContext.Core.Configuration;
using DevContext.Core.Contracts;
using DevContext.Core.Extractors.Generic;
using DevContext.Core.IO;
using DevContext.Core.Models;
using DevContext.Core.Observers;
using DevContext.Core.Pipeline;

using Microsoft.Extensions.Logging;

var summary = BenchmarkRunner.Run<DevContextBenchmarks>();

[MemoryDiagnoser]
public class DevContextBenchmarks
{
    private FakeFileSystem _fs = null!;
    private FakeAnalysisCache _cache = null!;
    private DiscoveryPipeline _pipeline = null!;
    private DiscoveryContext _context = null!;

    [Params(10, 100, 500)]
    public int FileCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _fs = new FakeFileSystem();
        _cache = new FakeAnalysisCache(_fs);

        var analysis = new SharedAnalysisContext();
        var projectFiles = new List<string>();

        for (int i = 0; i < FileCount; i++)
        {
            _fs.AddFile($"src/Project{i}/Service{i}.cs", $"namespace App; class Service{i} {{ public void DoWork() {{ }} }}");
            _fs.AddFile($"src/Project{i}/Project{i}.csproj", $"<Project><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");
            projectFiles.Add($"src/Project{i}/Project{i}.csproj");
        }

        analysis.AllProjectFiles = projectFiles.ToImmutableArray();
        analysis.AllSourceFiles = Enumerable.Range(0, FileCount).Select(i => $"src/Project{i}/Service{i}.cs").ToImmutableArray();

        var loggerFactory = LoggerFactory.Create(b => { });

        _context = new DiscoveryContext
        {
            RootPath = "src",
            Options = new ExtractionOptions { MaxOutputTokens = 8000, AllowRoslyn = false },
            ActiveScenario = ScenarioRegistry.BuiltIn["architecture"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = _fs,
            Cache = _cache,
            Analysis = analysis,
            Logger = loggerFactory.CreateLogger("Benchmark"),
            RoslynWorkspace = new MockRoslynProvider()
        };

        var extractors = new List<IDiscoveryExtractor>
        {
            new FileTreeExtractor(),
            new SolutionDiscoveryExtractor(),
            new ProjectStructureExtractor()
        };

        _pipeline = new DiscoveryPipeline(
            extractors, [], [], new Dictionary<string, IContextRenderer>
            {
                ["markdown"] = new BenchmarkMarkdownRenderer(),
                ["json"] = new BenchmarkJsonRenderer()
            },
            loggerFactory.CreateLogger<DiscoveryPipeline>());
    }

    [Benchmark]
    public async Task FullPipeline()
    {
        await _pipeline.RunAsync(_context);
    }

    [Benchmark]
    public async Task FileTreeExtraction()
    {
        var extractor = new FileTreeExtractor();
        await extractor.ExtractAsync(_context, new DiscoveryModel(), CancellationToken.None);
    }

    [Benchmark]
    public async Task ProjectStructureExtraction()
    {
        var extractor = new ProjectStructureExtractor();
        await extractor.ExtractAsync(_context, new DiscoveryModel(), CancellationToken.None);
    }
}

public sealed class MockRoslynProvider : IRoslynWorkspaceProvider
{
    public Task<IRoslynWorkspace?> GetWorkspaceAsync(CancellationToken ct)
        => Task.FromResult<IRoslynWorkspace?>(null);
}

public sealed class BenchmarkMarkdownRenderer : IContextRenderer
{
    public string Format => "markdown";
    public ValueTask<RenderedContext> RenderAsync(DiscoveryModel model, RenderOptions options, CancellationToken ct)
    {
        var text = $"# Benchmark\nTypes: {model.Types.Count}\n";
        return new ValueTask<RenderedContext>(new RenderedContext(text, text.Length / 4, [], TimeSpan.Zero, "2.0"));
    }
}

public sealed class BenchmarkJsonRenderer : IContextRenderer
{
    public string Format => "json";
    public ValueTask<RenderedContext> RenderAsync(DiscoveryModel model, RenderOptions options, CancellationToken ct)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new { types = model.Types.Count });
        return new ValueTask<RenderedContext>(new RenderedContext(json, json.Length / 4, [], TimeSpan.Zero, "2.0"));
    }
}
