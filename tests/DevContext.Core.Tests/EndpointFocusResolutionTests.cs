using DevContext.Core.Configuration;
using DevContext.Core.Contracts;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Tests;

public sealed class EndpointFocusResolutionTests
{
    /// <summary>Test extractor that runs at Stage 3 and injects a known endpoint detection + handler type.</summary>
    private sealed class EndpointInjectorExtractor : IDiscoveryExtractor
    {
        public string Name => "EndpointInjector";
        public ExtractorTier Tier => ExtractorTier.Deep;
        public ExtractorCategory Category => ExtractorCategory.Specific;
        public ExecutionStage Stage => ExecutionStage.Stage3Specific;
        public ExtractorCapabilities Capabilities => new([], [], ["Detections", "Types"], "Test injector");

        private readonly string _httpMethod;
        private readonly string _route;
        private readonly string _handlerType;
        private readonly string _handlerMethod;
        private readonly string _handlerFilePath;

        public EndpointInjectorExtractor(string httpMethod, string route,
            string handlerType, string handlerMethod, string handlerFilePath)
        {
            _httpMethod = httpMethod;
            _route = route;
            _handlerType = handlerType;
            _handlerMethod = handlerMethod;
            _handlerFilePath = handlerFilePath;
        }

        public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

        public ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
        {
            model.Detections.Add(new DevContext.Core.Models.EndpointDetection(
                _httpMethod, _route, _handlerType, _handlerMethod, [], [])
            {
                ExtractorName = Name,
                SourceFile = _handlerFilePath,
                LineNumber = 1,
            });

            model.Types.TryAdd(_handlerType, new TypeDiscovery
            {
                Id = _handlerType,
                Name = _handlerType,
                Namespace = "Test",
                FilePath = _handlerFilePath,
                Kind = TypeKind.Class,
                Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
                Layer = ArchitectureLayer.Api,
            });

            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task Endpoint_focus_resolved_to_handler_via_pipeline()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Program.cs", "class Program { static void Main() {} }");
        fs.AddFile(@"src\MyApp.csproj", "<Project />");

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath("src");
        var ctx = builder.Build();

        // Set an endpoint focus point
        ctx.Analysis.FocusPoints = [
            new FocusPoint(FocusKind.Endpoint, "", null, null,
                HttpMethod: "GET", Route: "/todos"),
        ];

        // Extractor injects a matching EndpointDetection + handler TypeDiscovery
        var extractors = new List<IDiscoveryExtractor>
        {
            new EndpointInjectorExtractor("GET", "/todos", "TodosController",
                "GetTodos", @"src\Controllers\TodosController.cs"),
        };

        var pipeline = new DiscoveryPipeline(
            extractors, [], [],
            new Dictionary<string, IContextRenderer> { ["markdown"] = new TestMarkdownRenderer() },
            new NullLogger<DiscoveryPipeline>());

        var snapshot = await pipeline.AnalyzeAsync(ctx, default);

        Assert.NotNull(snapshot);
        Assert.NotEmpty(snapshot.Analysis.FocusPoints);
        var resolved = snapshot.Analysis.FocusPoints[0];
        Assert.Equal(FocusKind.Method, resolved.Kind);
        Assert.Equal("TodosController", resolved.TypeName);
        Assert.Equal("GetTodos", resolved.MethodName);
        Assert.NotEmpty(resolved.FilePath);                                   // A2 lock
        Assert.Contains(@"TodosController.cs", resolved.FilePath);
    }

    [Fact]
    public async Task Endpoint_focus_not_found_emits_diagnostic()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Program.cs", "class Program { static void Main() {} }");
        fs.AddFile(@"src\MyApp.csproj", "<Project />");

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath("src");
        var ctx = builder.Build();

        ctx.Analysis.FocusPoints = [
            new FocusPoint(FocusKind.Endpoint, "", null, null,
                HttpMethod: "GET", Route: "/nonexistent"),
        ];

        // Extractor injects no matching endpoint
        var extractors = new List<IDiscoveryExtractor>
        {
            new EndpointInjectorExtractor("GET", "/todos", "TodosController",
                "GetTodos", @"src\Controllers\TodosController.cs"),
        };

        var pipeline = new DiscoveryPipeline(
            extractors, [], [],
            new Dictionary<string, IContextRenderer> { ["markdown"] = new TestMarkdownRenderer() },
            new NullLogger<DiscoveryPipeline>());

        var snapshot = await pipeline.AnalyzeAsync(ctx, default);

        Assert.NotNull(snapshot);
        Assert.Single(snapshot.Analysis.FocusPoints);
        Assert.Equal(FocusKind.Endpoint, snapshot.Analysis.FocusPoints[0].Kind); // unchanged

        Assert.Contains(snapshot.Model.Diagnostics, d =>
            d.Level == DiagnosticLevel.Warning &&
            d.Source == "EndpointFocusResolver" &&
            d.Message.Contains("/nonexistent"));
    }

    [Fact]
    public async Task Non_endpoint_focus_unaffected_by_endpoint_resolver()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Program.cs", "class Program { static void Main() {} }");
        fs.AddFile(@"src\MyApp.csproj", "<Project />");

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath("src");
        var ctx = builder.Build();

        ctx.Analysis.FocusPoints = [
            new FocusPoint(FocusKind.Type, @"src\Services\MyService.cs", "MyService", null),
        ];

        // No stage 3 extractors — type focus should pass through unchanged
        var pipeline = new DiscoveryPipeline(
            [], [], [],
            new Dictionary<string, IContextRenderer> { ["markdown"] = new TestMarkdownRenderer() },
            new NullLogger<DiscoveryPipeline>());

        var snapshot = await pipeline.AnalyzeAsync(ctx, default);

        Assert.NotNull(snapshot);
        Assert.Single(snapshot.Analysis.FocusPoints);
        Assert.Equal(FocusKind.Type, snapshot.Analysis.FocusPoints[0].Kind);
        Assert.Equal("MyService", snapshot.Analysis.FocusPoints[0].TypeName);
    }
}
