using System.Diagnostics;
using DevContext.Cli.Observers;
using DevContext.Cli.Services;
using DevContext.Cli.Settings;

namespace DevContext.Cli.Commands;

public sealed class AnalyzeCommand : AsyncCommand<AnalyzeSettings>
{
    private readonly IFileSystem _fs;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IRoslynWorkspaceProvider _roslynProvider;

    public AnalyzeCommand(
        IFileSystem fs,
        ILoggerFactory loggerFactory,
        IRoslynWorkspaceProvider roslynProvider)
    {
        _fs = fs;
        _loggerFactory = loggerFactory;
        _roslynProvider = roslynProvider;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, AnalyzeSettings settings, CancellationToken ct)
    {
        ConfigureLogging(settings);
        var config = DevContextConfig.Load(DevContextConfig.DefaultPath);
        ShowConfigWarnings(config);

        var resolver = new ProjectRootResolver();
        var rootResult = await resolver.ResolveAsync(settings.Path ?? ".", _fs, ct);

        var resolved = ResolveScenarioAndProfile(settings, config);
        if (resolved is null) return 1;

        var (scenario, options) = BuildOptions(settings, config, rootResult, resolved.Value);

        var cache = new AnalysisCache(_fs);
        var analysis = BuildSharedAnalysis(settings);
        var pipeline = BuildPipeline(cache);
        var roslyn = BuildRoslynProvider(settings, rootResult);

        var metricsObserver = settings.Metrics ? new MetricsDiscoveryObserver() : null;

        RenderedContext result = null!;
        var sw = Stopwatch.StartNew();

        AnsiConsole.Status()
            .Start(settings.DryRun ? "Planning analysis..." : "Analyzing project...", statusCtx =>
            {
                var spectreObserver = new SpectreDiscoveryObserver(settings.DryRun ? null : statusCtx);
                var observer = metricsObserver is not null
                    ? new CompositeDiscoveryObserver(spectreObserver, metricsObserver)
                    : (IDiscoveryObserver)spectreObserver;

                var ctx = new DiscoveryContext
                {
                    RootPath = rootResult.RootPath,
                    Options = options,
                    ActiveScenario = scenario,
                    Observer = observer,
                    FileSystem = _fs,
                    Cache = cache,
                    Analysis = analysis,
                    Logger = _loggerFactory.CreateLogger("DevContext"),
                    RoslynWorkspace = roslyn
                };

                result = pipeline.RunAsync(ctx, ct).GetAwaiter().GetResult();
            });

        await WriteOutput(settings, result);
        ShowMetrics(metricsObserver);
        ShowSummary(sw, rootResult, options, result);

        return 0;
    }

    private static void ConfigureLogging(AnalyzeSettings settings)
    {
        var level = settings.Trace ? Serilog.Events.LogEventLevel.Debug
                  : settings.Verbose ? Serilog.Events.LogEventLevel.Information
                  : Serilog.Events.LogEventLevel.Warning;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private static void ShowConfigWarnings(DevContextConfig? config)
    {
        if (config is null) return;
        foreach (var error in config.Validate())
            AnsiConsole.MarkupLine($"[yellow]Config: {error}[/]");
    }

    private (string Scenario, ExtractionProfile Profile)? ResolveScenarioAndProfile(
        AnalyzeSettings settings, DevContextConfig? config)
    {
        string? scenarioName = null;

        if (settings.Task is not null)
        {
            var (inferredScen, inferredProf) = IntentInferrer.Infer(settings.Task);
            scenarioName = inferredScen;
            settings.Profile ??= inferredProf.ToString().ToLowerInvariant();
        }

        scenarioName ??= settings.Scenario ?? config?.DefaultScenario ?? "architecture";

        if (!ScenarioRegistry.BuiltIn.TryGetValue(scenarioName, out _))
        {
            AnsiConsole.MarkupLine($"[red]Unknown scenario: {scenarioName}[/]");
            AnsiConsole.MarkupLine($"Available: {string.Join(", ", ScenarioRegistry.BuiltIn.Keys)}");
            return null;
        }

        var profileName = settings.Profile ?? config?.DefaultProfile ?? "focused";
        var profile = profileName.ToLowerInvariant() switch
        {
            "quick" => ExtractionProfile.Quick,
            "debug" => ExtractionProfile.Debug,
            "full" => ExtractionProfile.Full,
            _ => ExtractionProfile.Focused
        };

        return (scenarioName, profile);
    }

    private (Scenario Scenario, ExtractionOptions Options) BuildOptions(
        AnalyzeSettings settings, DevContextConfig? config,
        ProjectRootResult rootResult, (string name, ExtractionProfile profile) resolved)
    {
        var scenario = ScenarioRegistry.BuiltIn[resolved.name];

        var options = new ExtractionOptions
        {
            EntryPaths = rootResult.EntryCandidates,
            Profile = resolved.profile,
            MaxOutputTokens = settings.MaxTokens ?? config?.MaxOutputTokens ?? 8000,
            AllowRoslyn = !settings.NoRoslyn,
            DryRun = settings.DryRun,
            IncludeProvenance = settings.IncludeProvenance,
            IncludeDiagnostics = settings.IncludeDiagnostics,
            OutputFormat = settings.Format?.ToLowerInvariant() switch
            {
                "json" => OutputFormat.Json,
                _ => OutputFormat.Markdown
            },
            ExcludePatterns = config?.ExcludePatterns?.ToImmutableArray()
                ?? [".git", "bin", "obj", ".vs", "node_modules", ".idea"]
        };

        return (scenario, options);
    }

    private static SharedAnalysisContext BuildSharedAnalysis(AnalyzeSettings settings)
    {
        var fs = new RealFileSystem();
        var focusPoints = (settings.Around ?? [])
            .Select(a => FocusPointParser.Parse(a, fs))
            .Where(fp => fp is not null)
            .Select(fp => fp!)
            .ToImmutableArray();

        return new SharedAnalysisContext
        {
            UnresolvedFocusPoints = focusPoints,
            FocusPoints = focusPoints
        };
    }

    private DiscoveryPipeline BuildPipeline(IAnalysisCache cache)
    {
        var services = new ServiceCollection();
        services.AddDevContextServices(".");
        services.AddSingleton(_loggerFactory.CreateLogger<DiscoveryPipeline>());
        var sp = services.BuildServiceProvider();
        return sp.GetRequiredService<DiscoveryPipeline>();
    }

    private IRoslynWorkspaceProvider BuildRoslynProvider(AnalyzeSettings settings, ProjectRootResult root)
    {
        if (settings.NoRoslyn || root.SolutionFilePath is null)
            return new NullRoslynProvider();

        return new DevContext.Roslyn.Services.RoslynWorkspaceProvider(
            root.SolutionFilePath, _fs,
            _loggerFactory.CreateLogger<DevContext.Roslyn.Services.RoslynWorkspaceProvider>());
    }

    private static async Task WriteOutput(AnalyzeSettings settings, RenderedContext result)
    {
        if (settings.Output is not null)
        {
            await File.WriteAllTextAsync(settings.Output, result.Content);
            AnsiConsole.MarkupLine($"[green]Output written to {Path.GetFullPath(settings.Output)}[/]");
            return;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine(result.Content);
    }

    private static void ShowMetrics(MetricsDiscoveryObserver? metrics)
    {
        if (metrics is null) return;
        AnsiConsole.WriteLine();
        var content = new Markup(metrics.GetMetricsSummary().EscapeMarkup());
        var panel = new Panel(content)
            .Header("Metrics")
            .Border(BoxBorder.Rounded);
        AnsiConsole.Write(panel);
    }

    private static void ShowSummary(Stopwatch sw, ProjectRootResult root, ExtractionOptions options, RenderedContext result)
    {
        var label = Path.GetFileName(root.SolutionFilePath ?? root.RootPath);

        var summary = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("Metric").Centered())
            .AddColumn(new TableColumn("Value").Centered())
            .AddRow("[bold]Solution[/]", label.EscapeMarkup())
            .AddRow("[bold]Time[/]", $"{sw.ElapsedMilliseconds}ms")
            .AddRow("[bold]Tokens[/]", $"~{result.EstimatedTokens} (budget {options.MaxOutputTokens})")
            .AddRow("[bold]Version[/]", "v2.0.0");

        AnsiConsole.Write(summary);
    }
}
