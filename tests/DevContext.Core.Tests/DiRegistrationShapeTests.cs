using DevContext.Core.Extractors.Generic;
using DevContext.Core.Extractors.Specific;

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
