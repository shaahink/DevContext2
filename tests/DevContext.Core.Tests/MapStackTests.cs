using DevContext.Core.Graph;
using DevContext.Core.Pipeline;
using DevContext.Core.Rendering;

namespace DevContext.Core.Tests;

/// <summary>
/// M1.2 — the solution-wide stack is ONE fact with ONE owner. It used to be computed inside
/// <c>MapRenderer.AppendStack</c>, which meant it existed only as a line of markdown:
/// <c>MapResponse.stack</c> (proto field 13) had three readers — the desktop identity strip, the
/// Atlas chip header, MCP <c>overview</c> — and no writer at all, so all three rendered an
/// always-empty list. These tests pin both halves: the model carries the tags, and the markdown
/// line is a JOIN of those same tags rather than a second computation that can drift.
/// </summary>
public sealed class MapStackTests
{
    private static CodeGraph EmptyGraph => new(
        new Dictionary<NodeId, GraphNode>(),
        new Dictionary<NodeId, ImmutableArray<GraphEdge>>());

    private static AnalysisSnapshot Snapshot(DiscoveryModel model) => new()
    {
        Model = model,
        RootPath = "/root",
        Analysis = new SharedAnalysisContext(),
        Scenario = ScenarioRegistry.BuiltIn["overview"],
        Options = new ExtractionOptions(),
        Report = new RunReport
        {
            Stages = [],
            Extractors = [],
            Compressions = [],
            Cache = new CacheStats(0, 0, 0, 0),
            Corpus = new CorpusStats(0, 0, 0),
            Funnel = new TokenFunnel(0, 0, 0, 0, 0, 8000),
            Parallelism = new ParallelismStats(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero),
            TotalWall = TimeSpan.Zero,
        },
    };

    private static DiscoveryModel ModelWithSignals()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Orders.Api", @"C:/repo/src/Orders.Api/Orders.Api.csproj", "C#",
                    ["net10.0"], [], [new PackageReferenceInfo("MediatR", "12.0.0")]),
            ],
        };
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 1.0f));
        // MEASURED, not assumed: the CQRS tag comes off the MediatR SIGNAL (or hand-rolled handler
        // evidence), never off a package reference — a PackageReferenceInfo alone leaves it out.
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MediatR, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.EfCore, 1.0f));
        return model;
    }

    [Fact]
    public void Map_model_carries_the_stack_tags()
    {
        var map = MapBuilder.Build(ModelWithSignals(), EmptyGraph, []);

        // The runtime leads, then the detected signals in render order. ToArray() is not cosmetic:
        // ImmutableArray<T> is IEquatable<ImmutableArray<T>> by REFERENCE, so comparing two of them
        // through Assert.Equal never matches, whatever the contents.
        Assert.Equal(["net10.0", "Minimal APIs", "MediatR (CQRS)", "EF Core"], map.Stack.ToArray());
    }

    [Fact]
    public async Task Markdown_stack_line_is_a_join_of_the_model_stack()
    {
        var model = ModelWithSignals();
        var map = MapBuilder.Build(model, EmptyGraph, []);

        var ctx = new MapRenderContext(map, Snapshot(model), "markdown", new RenderRequest { Format = "markdown", MaxTokens = 8000 });
        var result = await MapRenderer.RenderAsync(ctx, CancellationToken.None);

        // Not "contains the words" — the exact line, built from the same list the wire carries.
        Assert.Contains("STACK  " + string.Join(" · ", map.Stack), result.Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task No_signals_means_no_stack_line_and_an_empty_list()
    {
        // A model with nothing detected must not grow a "STACK" header with an empty tail — the
        // renderer's old behaviour (skip the line entirely) is the honest one and is preserved.
        var model = new DiscoveryModel { Projects = [] };
        var map = MapBuilder.Build(model, EmptyGraph, []);

        Assert.Empty(map.Stack);

        var ctx = new MapRenderContext(map, Snapshot(model), "markdown", new RenderRequest { Format = "markdown", MaxTokens = 8000 });
        var result = await MapRenderer.RenderAsync(ctx, CancellationToken.None);

        Assert.DoesNotContain("STACK", result.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void Aggregates_add_the_ddd_tag_from_the_same_list_the_map_reports()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Orders.Domain", @"C:/repo/src/Orders.Domain/Orders.Domain.csproj", "C#",
                    ["net10.0"], [], []),
            ],
        };
        model.Detections.Add(new EfEntityDetection("Order", "OrderingContext", IsAggregate: true, ["Id"])
        {
            ExtractorName = "EfCoreExtractor",
            SourceFile = @"C:/repo/src/Orders.Domain/Order.cs",
            LineNumber = 1,
        });

        var map = MapBuilder.Build(model, EmptyGraph, []);

        Assert.NotEmpty(map.Aggregates);

        // Whatever BuildAggregates found, the stack tag agrees with it — one source, not two.
        Assert.Equal(map.Aggregates.Length > 0, map.Stack.Contains("DDD aggregates"));
    }
}
