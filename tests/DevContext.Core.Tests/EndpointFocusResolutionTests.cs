using DevContext.Core.Configuration;
using DevContext.Core.Pipeline;
using DevContext.Core.Services;

namespace DevContext.Core.Tests;

public sealed class EndpointFocusResolutionTests
{
    private static DiscoveryPipeline CreatePipeline(List<IPruner>? pruners = null)
    {
        return new DiscoveryPipeline(
            [], pruners ?? [], [],
            new Dictionary<string, IContextRenderer>
            {
                ["markdown"] = new TestMarkdownRenderer(),
            },
            new NullLogger<DiscoveryPipeline>());
    }

    [Fact]
    public async Task Endpoint_focus_resolved_to_handler_after_stage3()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Program.cs", "class Program { static void Main() {} }");
        fs.AddFile(@"src\MyApp.csproj", "<Project />");

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath("src");
        var built = builder.BuildWithRecording();
        var ctx = built.Context;

        // Add a resolved endpoint detection to the model (simulating Stage 3 output)
        var model = new DiscoveryModel();
        model.Detections.Add(new DevContext.Core.Models.EndpointDetection(
            "GET", "/todos", "TodosController", "GetTodos",
            [], [])
        {
            ExtractorName = "TestExtractor",
            SourceFile = "src/Controllers/TodosController.cs",
            LineNumber = 1,
        });

        // Set focus point to an unresolved endpoint
        ctx.Analysis.FocusPoints = [
            new FocusPoint(FocusKind.Endpoint, "", null, null,
                HttpMethod: "GET", Route: "/todos"),
        ];

        var pipeline = CreatePipeline();

        // Invoke the AnalyzeAsync method which includes ResolveEndpointFocusPoints after Stage 3
        // But since AnalyzeAsync is async with stages, we need to invoke the resolution directly
        // via reflection or by running the pipeline. We'll use the public API path.

        // Instead: we construct the pipeline and call AnalyzeAsync with a minimal context
        // The pipeline's stage execution is complex; we test the resolver via a simpler path.
        // Let's verify the resolver works by invoking ResolveEndpointFocusPoints via reflection.

        var resolveMethod = typeof(DiscoveryPipeline)
            .GetMethod("ResolveEndpointFocusPoints",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.NotNull(resolveMethod);
        resolveMethod.Invoke(null, [ctx, model]);

        Assert.Single(ctx.Analysis.FocusPoints);
        var resolved = ctx.Analysis.FocusPoints[0];
        Assert.Equal(FocusKind.Method, resolved.Kind);
        Assert.Equal("TodosController", resolved.TypeName);
        Assert.Equal("GetTodos", resolved.MethodName);
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
        var built = builder.BuildWithRecording();
        var ctx = built.Context;

        var model = new DiscoveryModel();
        // No endpoint detection registered

        ctx.Analysis.FocusPoints = [
            new FocusPoint(FocusKind.Endpoint, "", null, null,
                HttpMethod: "GET", Route: "/nonexistent"),
        ];

        var resolveMethod = typeof(DiscoveryPipeline)
            .GetMethod("ResolveEndpointFocusPoints",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.NotNull(resolveMethod);
        resolveMethod.Invoke(null, [ctx, model]);

        Assert.Single(ctx.Analysis.FocusPoints);
        Assert.Equal(FocusKind.Endpoint, ctx.Analysis.FocusPoints[0].Kind); // unchanged

        Assert.Contains(model.Diagnostics, d =>
            d is DiagnosticEntry diag &&
            diag.Level == DiagnosticLevel.Warning &&
            diag.Source == "EndpointFocusResolver" &&
            diag.Message.Contains("/nonexistent"));
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
        var built = builder.BuildWithRecording();
        var ctx = built.Context;

        var model = new DiscoveryModel();

        ctx.Analysis.FocusPoints = [
            new FocusPoint(FocusKind.Type, "SomeFile.cs", "MyService", null),
        ];

        var resolveMethod = typeof(DiscoveryPipeline)
            .GetMethod("ResolveEndpointFocusPoints",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.NotNull(resolveMethod);
        resolveMethod.Invoke(null, [ctx, model]);

        Assert.Single(ctx.Analysis.FocusPoints);
        Assert.Equal(FocusKind.Type, ctx.Analysis.FocusPoints[0].Kind);
        Assert.Equal("MyService", ctx.Analysis.FocusPoints[0].TypeName);
    }
}
