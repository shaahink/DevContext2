using DevContext.Cli.Settings;
using DevContext.Core.Pipeline;
using DevContext.Core.Services;

using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace DevContext.Cli.Commands;

public sealed class QueryCommand : AsyncCommand<QuerySettings>
{
    private readonly IFileSystem _fs;
    private readonly ILoggerFactory _loggerFactory;
    private readonly DiscoveryPipeline _pipeline;

    private static readonly HashSet<string> ValidOps = new(StringComparer.OrdinalIgnoreCase)
    {
        "entrypoints", "map", "trace", "stats",
    };

    public QueryCommand(IFileSystem fs, ILoggerFactory loggerFactory, DiscoveryPipeline pipeline)
    {
        _fs = fs;
        _loggerFactory = loggerFactory;
        _pipeline = pipeline;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, QuerySettings settings, CancellationToken ct)
    {
        if (!ValidOps.Contains(settings.Op))
        {
            AnsiConsole.MarkupLine($"[red]Unknown op '{settings.Op}'. Valid: {string.Join(", ", ValidOps.Order())}[/]");
            return 1;
        }

        var path = settings.Path ?? ".";
        var rootResult = await ProjectRootResolver.ResolveAsync(path, _fs, ct);

        var options = new ExtractionOptions
        {
            Profile = ExtractionProfile.Focused,
            AllowRoslyn = true,
            BuildFullGraph = true,
            OutputFormat = OutputFormat.Json,
        };

        var scenario = ScenarioRegistry.BuiltIn["overview"];
        var cache = new AnalysisCache(_fs);
        var analysis = new SharedAnalysisContext();

        var ctx = new DiscoveryContext
        {
            RootPath = rootResult.EffectiveRootPath,
            Options = options,
            ActiveScenario = scenario,
            Observer = new NullDiscoveryObserver(),
            FileSystem = _fs,
            Cache = cache,
            Analysis = analysis,
            Logger = _loggerFactory.CreateLogger("DevContext"),
        };

        var snapshot = await _pipeline.AnalyzeAsync(ctx, ct);

        if (snapshot.IsDryRun)
        {
            AnsiConsole.MarkupLine("[yellow]Dry run only — no data.[/]");
            return 0;
        }

        var format = settings.Format?.ToLowerInvariant() == "md" ? "markdown" : "json";
        var request = new RenderRequest
        {
            Format = format,
            MaxTokens = 8000,
            Sections = scenario.RequiredSections,
        };

        var result = await _pipeline.RenderAsync(snapshot, request, ct);
        AnsiConsole.WriteLine(result.Content);
        return 0;
    }
}
