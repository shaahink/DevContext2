namespace DevContext.Core.Tests;

public static partial class GoldenTestHelper
{
    private static readonly System.Text.RegularExpressions.Regex TimingPattern =
        MyRegex();

    private static readonly System.Text.RegularExpressions.Regex GeneratedAtPattern = GeneratedAtPattern_();

    public static string NormalizeOutput(string content)
    {
        content = TimingPattern.Replace(content, "*Generated in {elapsed}ms");
        content = GeneratedAtPattern.Replace(content, "\"generatedAt\": \"{timestamp}\"");
        return content
            .Replace("\r\n", "\n")
            .Trim();
    }

    public static void AssertMatchesGolden(string actual, string goldenPath)
    {
        var normalized = NormalizeOutput(actual);
        var update = string.Equals(Environment.GetEnvironmentVariable("UPDATE_GOLDENS"), "1", StringComparison.Ordinal);

        if (update)
        {
            File.WriteAllText(goldenPath, normalized);
            return;
        }

        if (!File.Exists(goldenPath))
        {
            File.WriteAllText(goldenPath, normalized);
            Assert.Fail($"Golden file did not exist at {goldenPath}. Created from actual output.");
            return;
        }

        var expected = NormalizeOutput(File.ReadAllText(goldenPath));
        Assert.Equal(expected, normalized);
    }

    public static async Task<string> RunPipelineOnFixture(string fixtureDir, string scenario = "overview")
    {
        var fs = new FakeFileSystem();
        var fixturePath = GetFixturePath(fixtureDir);

        if (Directory.Exists(fixturePath))
        {
            foreach (var file in Directory.EnumerateFiles(fixturePath, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(fixturePath, file);
                var content = await File.ReadAllTextAsync(file).ConfigureAwait(false);
                fs.AddFile(relative, content);
            }
        }

        var cache = new FakeAnalysisCache(fs);
        var analysis = new SharedAnalysisContext();
        var observer = new RecordingDiscoveryObserver();
        var loggerFactory = LoggerFactory.Create(b => { });

        var ctx = new DiscoveryContext
        {
            RootPath = "",
            Options = new ExtractionOptions { MaxOutputTokens = 8000 },
            ActiveScenario = ScenarioRegistry.BuiltIn[scenario],
            Observer = observer,
            FileSystem = fs,
            Cache = cache,
            Analysis = analysis,
            Logger = loggerFactory.CreateLogger("Golden"),
            RoslynWorkspace = new MockRoslynProvider()
        };

        var extractors = new List<IDiscoveryExtractor>
        {
            new FileTreeExtractor(),
            new SolutionDiscoveryExtractor(),
            new ProjectStructureExtractor()
        };

        var pipeline = new DiscoveryPipeline(
            extractors, [], [], new Dictionary<string, IContextRenderer>
(StringComparer.Ordinal)
            {
                ["markdown"] = new TestMarkdownRenderer(),
                ["json"] = new TestJsonRenderer()
            },
            loggerFactory.CreateLogger<DiscoveryPipeline>());

        var result = await pipeline.RunAsync(ctx).ConfigureAwait(false);
        return result.Content;
    }

    public static async Task<RenderedContext> RunPipelineOnFixtureWithAllExtractors(
        string fixtureDir, string scenario = "overview", string format = "markdown")
    {
        var fs = new FakeFileSystem();
        var fixturePath = GetFixturePath(fixtureDir);

        if (Directory.Exists(fixturePath))
        {
            foreach (var file in Directory.EnumerateFiles(fixturePath, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(fixturePath, file);
                var content = await File.ReadAllTextAsync(file).ConfigureAwait(false);
                fs.AddFile(relative, content);
            }
        }

        var cache = new FakeAnalysisCache(fs);
        var loggerFactory = LoggerFactory.Create(b => { });

        var ctx = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath("")
            .WithOptions(new ExtractionOptions
            {
                MaxOutputTokens = 8000,
                OutputFormat = string.Equals(format, "json", StringComparison.Ordinal) ? OutputFormat.Json : OutputFormat.Markdown
            })
            .WithScenario(ScenarioRegistry.BuiltIn[scenario])
            .Build();

        var extractors = new List<IDiscoveryExtractor>
        {
            new FileTreeExtractor(),
            new SolutionDiscoveryExtractor(),
            new ProjectStructureExtractor(),
            new DependencyExtractor(),
            new SyntaxStructureExtractor(),
            new LayerClassifier(),
            new EndpointExtractor(),
            new MediatRExtractor(),
            new ControllerActionExtractor(),
            new EfCoreExtractor(),
            new EventBusExtractor(),
            new CallGraphExtractor(),
            new SourceBodyExtractor(),
            new IndirectWiringDetector(),
            new AspireExtractor(),
            new ProgramCsFlowExtractor(),
            new DiRegistrationExtractor(),
        };

        var pruners = new List<IPruner>
        {
            new PatternRelevancePruner(),
            new TokenBudgetEnforcer(),
        };

        var compressors = new List<ICompressionStrategy>
        {
            new TrivialMemberCompressor(),
            new BoilerplateCompressor(),
            new StructuralDeduplicator(),
            new NamespaceGrouper(),
            new LlmFriendlyFormatter(),
            new AggressiveTruncator(),
        };

        var pipeline = new DiscoveryPipeline(
            extractors, pruners, compressors,
            new Dictionary<string, IContextRenderer>
(StringComparer.Ordinal)
            {
                ["markdown"] = new MarkdownRenderer(),
                ["json"] = new JsonContextRenderer(),
            },
            loggerFactory.CreateLogger<DiscoveryPipeline>());

        var result = await pipeline.RunAsync(ctx).ConfigureAwait(false);
        return result;
    }

    private static string GetFixturePath(string fixtureName)
    {
        var searchPaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tests", "fixtures", fixtureName),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "tests", "fixtures", fixtureName),
            Path.Combine(Environment.CurrentDirectory, "tests", "fixtures", fixtureName),
        };

        foreach (var path in searchPaths)
        {
            var full = Path.GetFullPath(path);
            if (Directory.Exists(full))
                return full;
        }

        // Create a default fixture on the fly
        var defaultFixture = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "tests", "fixtures", fixtureName));
        Directory.CreateDirectory(defaultFixture);
        return defaultFixture;
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"\*Generated in [\d.]+ms", System.Text.RegularExpressions.RegexOptions.Compiled)]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
    [System.Text.RegularExpressions.GeneratedRegex(@"""generatedAt"":\s*""[^""]+""", System.Text.RegularExpressions.RegexOptions.Compiled)]
    private static partial System.Text.RegularExpressions.Regex GeneratedAtPattern_();
}

[CollectionDefinition("Golden tests")]
public class GoldenTestCollection : ICollectionFixture<GoldenTestFixture>;

public sealed class GoldenTestFixture
{
    public string GoldensPath { get; }

    public GoldenTestFixture()
    {
        GoldensPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tests", "goldens"));
        Directory.CreateDirectory(GoldensPath);
    }
}
