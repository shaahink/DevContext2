namespace DevContext.Core.Tests;

public sealed class AntiPatternDetectorTests
{
    [Fact]
    public async Task FireAndForget_IsDemotedInTestFiles()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\tests\MyTests\UnitTest.cs", """
            public class UnitTest
            {
                public void TestMethod()
                {
                    _ = FireAndForgetAsync();
                }
                private Task FireAndForgetAsync() => Task.CompletedTask;
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\tests\MyTests\UnitTest.cs"];

        var model = new DiscoveryModel();
        var detector = new AntiPatternDetector();
        await detector.ExtractAsync(ctx, model, default);

        var patterns = model.Detections.OfType<AntiPatternDetection>().ToArray();
        Assert.NotEmpty(patterns);
        Assert.All(patterns, p =>
        {
            Assert.Equal("low", p.Severity);
            Assert.Contains("[test file", p.Description, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task FireAndForget_IsNotDemotedInProductionFiles()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Services\Worker.cs", """
            public class Worker
            {
                public void Process()
                {
                    _ = FireAndForgetAsync();
                }
                private Task FireAndForgetAsync() => Task.CompletedTask;
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\Services\Worker.cs"];

        var model = new DiscoveryModel();
        var detector = new AntiPatternDetector();
        await detector.ExtractAsync(ctx, model, default);

        var patterns = model.Detections.OfType<AntiPatternDetection>().ToArray();
        Assert.NotEmpty(patterns);
        Assert.All(patterns, p => Assert.Equal("high", p.Severity));
    }

    [Fact]
    public async Task AsyncVoid_IsDetected()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Services\Worker.cs", """
            public class Worker
            {
                public async void BadMethod()
                {
                    await Task.Delay(100);
                }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\Services\Worker.cs"];

        var model = new DiscoveryModel();
        var detector = new AntiPatternDetector();
        await detector.ExtractAsync(ctx, model, default);

        var patterns = model.Detections.OfType<AntiPatternDetection>()
            .Where(p => string.Equals(p.Pattern, "AsyncVoid", StringComparison.Ordinal)).ToArray();
        Assert.NotEmpty(patterns);
        Assert.All(patterns, p => Assert.Equal("high", p.Severity));
    }

    [Fact]
    public async Task AsyncVoid_EventHandlersAreSkipped()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\UI\Form.cs", """
            public class Form
            {
                public async void OnButtonClick(object sender, EventArgs e)
                {
                    await SaveAsync();
                }
                private Task SaveAsync() => Task.CompletedTask;
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\UI\Form.cs"];

        var model = new DiscoveryModel();
        var detector = new AntiPatternDetector();
        await detector.ExtractAsync(ctx, model, default);

        var patterns = model.Detections.OfType<AntiPatternDetection>()
            .Where(p => string.Equals(p.Pattern, "AsyncVoid", StringComparison.Ordinal)).ToArray();
        Assert.Empty(patterns);
    }

    [Fact]
    public async Task CaptiveDependency_IsDetected()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Services\BarEvaluationHandler.cs", """
            public class BarEvaluationHandler
            {
                private readonly IBarRepository _repo;
                public BarEvaluationHandler(IBarRepository repo) { _repo = repo; }
            }
            public interface IBarRepository { }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\Services\BarEvaluationHandler.cs"];

        var model = new DiscoveryModel();

        // Pre-populate DI registrations: Singleton → Scoped dependency
        model.Detections.Add(new DiRegistrationDetection(
            "IBarRepository", "BarRepository", "Scoped", [])
        {
            ExtractorName = "DiRegistrationExtractor",
            SourceFile = @"C:\repo\src\Program.cs",
            LineNumber = 10
        });
        model.Detections.Add(new DiRegistrationDetection(
            "BarEvaluationHandler", "BarEvaluationHandler", "Singleton", [])
        {
            ExtractorName = "DiRegistrationExtractor",
            SourceFile = @"C:\repo\src\Program.cs",
            LineNumber = 11
        });

        // Pre-populate types with constructor info
        model.Types["BarEvaluationHandler"] = new TypeDiscovery
        {
            Id = "BarEvaluationHandler",
            Name = "BarEvaluationHandler",
            Namespace = "",
            FilePath = @"C:\repo\src\Services\BarEvaluationHandler.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            Methods = [new MethodSignature(
                ".ctor", "", ["IBarRepository"], ["repo"],
                Microsoft.CodeAnalysis.Accessibility.Public, false, false)]
        };

        var detector = new AntiPatternDetector();
        await detector.ExtractAsync(ctx, model, default);

        var patterns = model.Detections.OfType<AntiPatternDetection>()
            .Where(p => string.Equals(p.Pattern, "CaptiveDependency", StringComparison.Ordinal)).ToArray();
        Assert.NotEmpty(patterns);
        Assert.All(patterns, p => Assert.Equal("high", p.Severity));
    }
}
