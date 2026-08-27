namespace DevContext.Core.Tests;

public sealed class DiRegistrationShapeTests
{
    [Fact]
    public async Task ClassifiesDirectBinding()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Program.cs", """
            public class Program {
                public static void Main() {
                    var builder = WebApplication.CreateBuilder(args);
                    builder.Services.AddScoped<IFoo, Foo>();
                }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\Program.cs"];

        var model = new DiscoveryModel();
        var extractor = new DiRegistrationExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var regs = model.Detections.OfType<DiRegistrationDetection>().ToArray();
        Assert.NotEmpty(regs);
        var reg = regs[0];
        Assert.Equal(DevContext.Core.Models.DiRegistrationShape.DirectBinding, reg.Shape);
    }

    [Fact]
    public async Task ClassifiesSelfRegistration()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Program.cs", """
            public class Program {
                public static void Main() {
                    var builder = WebApplication.CreateBuilder(args);
                    builder.Services.AddSingleton<Foo>();
                }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\Program.cs"];

        var model = new DiscoveryModel();
        var extractor = new DiRegistrationExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var regs = model.Detections.OfType<DiRegistrationDetection>().ToArray();
        Assert.NotEmpty(regs);
        Assert.Equal(DevContext.Core.Models.DiRegistrationShape.SelfRegistration, regs[0].Shape);
    }

    [Fact]
    public async Task ClassifiesForwardingAlias()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Program.cs", """
            public class Program {
                public static void Main() {
                    var builder = WebApplication.CreateBuilder(args);
                    builder.Services.AddSingleton(sp => sp.GetRequiredService<Foo>());
                }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\Program.cs"];

        var model = new DiscoveryModel();
        var extractor = new DiRegistrationExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var regs = model.Detections.OfType<DiRegistrationDetection>().ToArray();
        Assert.NotEmpty(regs);
        Assert.Equal(DevContext.Core.Models.DiRegistrationShape.ForwardingAlias, regs[0].Shape);
    }

    [Fact]
    public async Task ClassifiesInlineFactory()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Program.cs", """
            public class Program {
                public static void Main() {
                    var builder = WebApplication.CreateBuilder(args);
                    builder.Services.AddSingleton(sp => new Foo(sp.GetRequiredService<IBar>(), 42));
                }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\Program.cs"];

        var model = new DiscoveryModel();
        var extractor = new DiRegistrationExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var regs = model.Detections.OfType<DiRegistrationDetection>().ToArray();
        Assert.NotEmpty(regs);
        Assert.Equal(DevContext.Core.Models.DiRegistrationShape.InlineFactory, regs[0].Shape);
        Assert.NotNull(regs[0].FactorySummary);
        Assert.Contains("Foo", regs[0].FactorySummary);
    }

    /// <summary>#37 (Book2Course F4's residual) — stages registered by an assembly scan. The
    /// registration site names ONLY the interface (typeof(IStage)); the implementor is a runtime
    /// Type the syntax can never spell. THE RED: before the fix this classified as DirectBinding
    /// with ImplementationType "stage" — a loop variable's name recorded as if it were a type.</summary>
    [Fact]
    public async Task ClassifiesScanRegistration()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\DependencyInjection.cs", """
            public static class PipelineModule {
                public static IServiceCollection AddStages(this IServiceCollection services) {
                    foreach (var stage in typeof(IStage).Assembly.GetTypes()
                        .Where(t => !t.IsAbstract && typeof(IStage).IsAssignableFrom(t)))
                    {
                        services.AddSingleton(typeof(IStage), stage);
                    }
                    return services;
                }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\DependencyInjection.cs"];

        var model = new DiscoveryModel();
        var extractor = new DiRegistrationExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var regs = model.Detections.OfType<DiRegistrationDetection>().ToArray();
        var scan = Assert.Single(regs, r => r.Shape == DevContext.Core.Models.DiRegistrationShape.ScanRegistration);
        Assert.Equal("IStage", scan.ServiceType);
        Assert.Equal("*", scan.ImplementationType);
    }

    /// <summary>The NAMED registration — AddSingleton(typeof(IFoo), typeof(Foo)) — spells its own
    /// implementor and is a different, deliberately deferred class. It must not ride the scan join,
    /// even inside a method that happens to mention a scan API.</summary>
    [Fact]
    public async Task ATypeofPairIsNotAScanRegistration()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Program.cs", """
            public class Program {
                public static void Main() {
                    var builder = WebApplication.CreateBuilder(args);
                    var known = typeof(IFoo).Assembly.GetTypes();
                    builder.Services.AddSingleton(typeof(IFoo), typeof(Foo));
                }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\Program.cs"];

        var model = new DiscoveryModel();
        var extractor = new DiRegistrationExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var regs = model.Detections.OfType<DiRegistrationDetection>().ToArray();
        Assert.DoesNotContain(regs, r => r.Shape == DevContext.Core.Models.DiRegistrationShape.ScanRegistration);
    }

    [Fact]
    public async Task AnnotatesImplicitPublishAsync()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Worker.cs", """
            public interface IEventBus { void PublishAsync<T>(T evt); }
            public class Worker {
                private IEventBus _bus;
                public void Process(BarEvaluated evt) {
                    _bus.PublishAsync(evt); // implicit — no type arg
                }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\Worker.cs"];

        var model = new DiscoveryModel();
        var extractor = new InMemoryEventBusExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        // Should produce a diagnostic about implicit publications
        var diags = model.Diagnostics
            .Where(d => d.Source == "InMemoryEventBusExtractor").ToList();
        Assert.NotEmpty(diags);
        Assert.Contains(diags, d => d.Message.Contains("implicit"));
    }
}
