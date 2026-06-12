namespace DevContext.Core.Tests;

public sealed class IndirectWiringDetectorTests
{
    [Fact]
    public async Task Detects_ActivatorCreateInstance()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Services\ReflectiveFactory.cs",
            """
            using System;
            public class ReflectiveFactory
            {
                public object Create(string typeName)
                {
                    var type = Type.GetType(typeName);
                    return Activator.CreateInstance(type);
                }
            }
            """);

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo")
            .WithScenario(ScenarioRegistry.BuiltIn["deep-dive"])
            .BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\Services\ReflectiveFactory.cs"];

        var model = new DiscoveryModel();
        var extractor = new IndirectWiringDetector();
        await extractor.ExtractAsync(ctx, model, default);

        var wirings = model.Detections.OfType<IndirectWiringDetection>().ToList();
        Assert.Contains(wirings, w => w.Kind == IndirectWiringKind.ReflectionActivation);
    }

    [Fact]
    public async Task Detects_ServiceLocator_Pattern()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Services\LocatorService.cs",
            """
            using Microsoft.Extensions.DependencyInjection;
            public class LocatorService
            {
                private readonly IServiceProvider serviceProvider;
                public LocatorService(IServiceProvider sp) { serviceProvider = sp; }
                public void DoWork()
                {
                    var service = serviceProvider.GetRequiredService<IMyService>();
                    service.Execute();
                }
            }
            """);

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo")
            .WithScenario(ScenarioRegistry.BuiltIn["deep-dive"])
            .BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\Services\LocatorService.cs"];

        var model = new DiscoveryModel();
        var extractor = new IndirectWiringDetector();
        await extractor.ExtractAsync(ctx, model, default);

        var wirings = model.Detections.OfType<IndirectWiringDetection>().ToList();
        Assert.Contains(wirings, w => w.Kind == IndirectWiringKind.ManualServiceLocator);
    }

    [Fact]
    public async Task Detects_ReflectionScanning()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Services\AssemblyScanner.cs",
            """
            using System;
            using System.Reflection;
            public class AssemblyScanner
            {
                public void Scan()
                {
                    var types = Assembly.GetExecutingAssembly().GetTypes();
                    foreach (var t in types)
                        if (typeof(IMarker).IsAssignableFrom(t))
                            Register(t);
                }
                void Register(Type t) { }
            }
            """);

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo")
            .WithScenario(ScenarioRegistry.BuiltIn["deep-dive"])
            .BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\Services\AssemblyScanner.cs"];

        var model = new DiscoveryModel();
        var extractor = new IndirectWiringDetector();
        await extractor.ExtractAsync(ctx, model, default);

        var wirings = model.Detections.OfType<IndirectWiringDetection>().ToList();
        Assert.Contains(wirings, w => w.Kind == IndirectWiringKind.ReflectionActivation);
    }

    [Fact]
    public async Task Ignores_Plain_DI_Constructor_Injection()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Services\CleanService.cs",
            """
            public class CleanService
            {
                private readonly IRepository _repo;
                public CleanService(IRepository repo) { _repo = repo; }
                public void DoWork() => _repo.Save();
            }
            """);

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo")
            .WithScenario(ScenarioRegistry.BuiltIn["deep-dive"])
            .BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\Services\CleanService.cs"];

        var model = new DiscoveryModel();
        var extractor = new IndirectWiringDetector();
        await extractor.ExtractAsync(ctx, model, default);

        var wirings = model.Detections.OfType<IndirectWiringDetection>().ToList();
        Assert.Empty(wirings);
    }
}
