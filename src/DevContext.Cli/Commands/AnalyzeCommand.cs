using System.Diagnostics;

using DevContext.Cli.Observers;
using DevContext.Cli.Settings;
using DevContext.Core.Services;

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

        var inputPath = settings.Path ?? ".";

        // GitHub repo support: clone if URL detected
        var gitClonePath = null as string;
        var repoUrl = RepoUrl.Parse(settings.Repo ?? inputPath);
        if (repoUrl is { IsValid: true })
        {
            var git = new GitCloneService();
            if (!git.IsGitAvailable)
            {
                AnsiConsole.MarkupLine("[red]Git is not installed. Install Git to clone GitHub repositories.[/]");
                return 1;
            }

            var cleanup = settings.Cleanup ?? (settings.Keep ? "keep" : "auto");
            var status = await git.ValidateAsync(repoUrl, ct);
            if (status != RepoStatus.Valid)
            {
                var msg = status switch
                {
                    RepoStatus.NotFound => "Repository not found",
                    RepoStatus.Private => "Private repository (not supported)",
                    RepoStatus.NetworkError => "Network error — check connection",
                    RepoStatus.RateLimited => "Rate limited — try again later",
                    _ => "Unknown error"
                };
                AnsiConsole.MarkupLine($"[red]{msg}[/]");
                return 1;
            }

            gitClonePath = repoUrl.ClonePath;
            var branch = settings.Ref ?? repoUrl.Ref;
            var cloneResult = await git.CloneAsync(repoUrl, gitClonePath, branch, null, ct);
            if (cloneResult is null)
            {
                AnsiConsole.MarkupLine("[red]Clone failed[/]");
                return 1;
            }

            inputPath = gitClonePath;
        }

        var resolver = new ProjectRootResolver();
        var rootResult = await ProjectRootResolver.ResolveAsync(inputPath, _fs, ct);

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
                var spectreObserver = new SpectreDiscoveryObserver();
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

        // Clean up clone if auto-clean
        if (gitClonePath is not null)
        {
            var cleanup = settings.Cleanup ?? (settings.Keep ? "keep" : "auto");
            if (cleanup == "auto")
                GitCloneService.Cleanup(gitClonePath);
        }

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

    private static (string Scenario, ExtractionProfile Profile)? ResolveScenarioAndProfile(
        AnalyzeSettings settings, DevContextConfig? config)
    {
        string? scenarioName = null;

        if (settings.Task is not null)
        {
            var (inferredScen, inferredProf) = IntentInferrer.Infer(settings.Task);
            scenarioName = inferredScen;
            settings.Profile ??= inferredProf.ToString().ToLowerInvariant();
        }

        scenarioName ??= settings.Scenario ?? config?.DefaultScenario ?? "overview";

        if (!ScenarioRegistry.BuiltIn.TryGetValue(scenarioName, out _))
        {
            AnsiConsole.MarkupLine($"[red]Unknown scenario: {scenarioName}[/]");
            AnsiConsole.MarkupLine($"Available: {string.Join(", ", ScenarioRegistry.BuiltIn.Keys)}");
            return null;
        }

        var profileName = settings.Profile ?? config?.DefaultProfile ?? "focused";
        var profile = profileName.ToLowerInvariant() switch
        {
            "debug" => ExtractionProfile.Debug,
            "full" => ExtractionProfile.Full,
            _ => ExtractionProfile.Focused
        };

        if (profileName.ToLowerInvariant() is not ("debug" or "full" or "focused"))
            AnsiConsole.MarkupLine($"[yellow]Warning: unknown profile '{settings.Profile}'. Defaulting to 'focused'.[/]");

        return (scenarioName, profile);
    }

    private static (Scenario Scenario, ExtractionOptions Options) BuildOptions(
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
            TokenView = settings.TokenView,
            IncludeAntiPatterns = settings.IncludeAntiPatterns,
            OutputFormat = settings.Format?.ToLowerInvariant() switch
            {
                "json" => OutputFormat.Json,
                _ => OutputFormat.Markdown
            },
            ExcludePatterns = config?.ExcludePatterns?.ToImmutableArray()
                ?? [".git", "bin", "obj", ".vs", "node_modules", ".idea"],
            ExcludeExtractors = scenario.DisableExtractors,
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
