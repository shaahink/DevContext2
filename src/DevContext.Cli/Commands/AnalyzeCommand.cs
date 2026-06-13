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
                    RepoStatus.NotFound => "Repository not found. Check the URL or ensure the repo is public.",
                    RepoStatus.Private => "Private repositories require authentication. Clone the repo locally and run DevContext on the local path.",
                    RepoStatus.NetworkError => "Network error — check your connection or try again later.",
                    RepoStatus.RateLimited => "GitHub API rate limit exceeded. Wait a few minutes or use a local path instead of a URL.",
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

        // Build IntentInput from settings
        var focusInput = settings.Focus ?? settings.Around;
        var focusText = focusInput is { Length: > 0 } ? focusInput[0] : null;
        var intentInput = new IntentInput
        {
            Focus = focusText ?? settings.Task,
            Depth = settings.Depth,
            ExplicitScenario = settings.Scenario ?? config?.DefaultScenario,
            ExplicitProfile = settings.Profile ?? config?.DefaultProfile,
        };

        ResolvedIntent resolvedIntent;
        try
        {
            resolvedIntent = AnalysisIntentResolver.Resolve(intentInput);
        }
        catch (ArgumentException ex)
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            return 1;
        }

        // Print explanation and warnings
        AnsiConsole.MarkupLine($"[dim]{resolvedIntent.Explanation}[/]");
        foreach (var warning in resolvedIntent.Warnings)
            AnsiConsole.MarkupLine($"[yellow]{warning}[/]");

        // --task deprecation
        if (!string.IsNullOrWhiteSpace(settings.Task))
            AnsiConsole.MarkupLine("[yellow]--task is deprecated. Use --focus instead.[/]");

        var options = new ExtractionOptions
        {
            EntryPaths = rootResult.EntryCandidates,
            Profile = resolvedIntent.Profile,
            MaxOutputTokens = settings.MaxTokens ?? config?.MaxOutputTokens ?? 8000,
            AllowRoslyn = !settings.NoRoslyn,
            DryRun = settings.DryRun,
            IncludeProvenance = settings.IncludeProvenance,
            IncludeDiagnostics = settings.IncludeDiagnostics,
            TokenView = settings.TokenView,
            IncludeAntiPatterns = settings.IncludeAntiPatterns,
            Strict = settings.Strict,
            OutputFormat = settings.Format?.ToLowerInvariant() switch
            {
                "json" => OutputFormat.Json,
                "html" => OutputFormat.Html,
                _ => OutputFormat.Markdown
            },
            ExcludePatterns = config?.ExcludePatterns?.ToImmutableArray()
                ?? [".git", "bin", "obj", ".vs", "node_modules", ".idea"],
            ExcludeExtractors = resolvedIntent.Scenario.DisableExtractors,
        };

        var scenario = resolvedIntent.Scenario;

        var cache = new AnalysisCache(_fs);
        var analysis = new SharedAnalysisContext
        {
            UnresolvedFocusPoints = resolvedIntent.FocusPoints,
            FocusPoints = resolvedIntent.FocusPoints,
        };
        var pipeline = BuildPipeline(cache);
        var roslyn = BuildRoslynProvider(settings, rootResult);

        RenderedContext result = null!;
        AnalysisSnapshot? snapshot = null;
        var sw = Stopwatch.StartNew();

        var collector = new RunReportCollector();
        collector.SetBudget(options.MaxOutputTokens);

        var spectreObserver = new SpectreDiscoveryObserver();
        var inner = new List<IDiscoveryObserver> { spectreObserver };
        inner.Add(collector);
        var observer = new CompositeDiscoveryObserver([.. inner]);

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

        await AnsiConsole.Status()
            .StartAsync("Analyzing project...", async statusCtx =>
            {
                var capturedSnapshot = await pipeline.AnalyzeAsync(ctx, ct);
                snapshot = capturedSnapshot;

                if (capturedSnapshot.IsDryRun)
                {
                    result = new RenderedContext(capturedSnapshot.DryRunContent!, 0, [], TimeSpan.Zero, "2.0");
                }
                else
                {
                    var request = new RenderRequest
                    {
                        Format = options.OutputFormat.ToString().ToLowerInvariant(),
                        MaxTokens = options.MaxOutputTokens,
                        Sections = scenario.RequiredSections,
                        IncludeProvenance = options.IncludeProvenance,
                        IncludeDiagnostics = options.IncludeDiagnostics,
                        TokenView = options.TokenView,
                    };

                    result = await pipeline.RenderAsync(capturedSnapshot, request, ct);
                }
            });

        await WriteOutput(settings, result);
        if (settings.Strict && HandleStrictMode(result))
            return 2;

        if (snapshot?.Report is { } report)
        {
            var summary = RunReportFormatter.Summary(report, result.RenderFunnel);
            if (!settings.DryRun)
                AnsiConsole.MarkupLine($"[dim]{summary}[/]");
        }

        if (settings.Stats || settings.Metrics)
            ShowStats(snapshot?.Report);

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

    private static void ShowStats(RunReport? report)
    {
        if (report is null) return;

        AnsiConsole.WriteLine();

        // Stage waterfall
        var waterfall = new Table()
            .Border(TableBorder.Rounded)
            .Title("Stage Timing")
            .AddColumn("Stage")
            .AddColumn(new TableColumn("Time").RightAligned())
            .AddColumn(new TableColumn("Bar").LeftAligned());

        var totalMs = report.TotalWall.TotalMilliseconds;
        foreach (var stage in report.Stages)
        {
            var barLen = totalMs > 0 ? (int)(stage.Elapsed.TotalMilliseconds / totalMs * 40) : 0;
            waterfall.AddRow(stage.Stage, $"{stage.Elapsed.TotalMilliseconds:F0}ms",
                new string('\u2588', Math.Min(barLen, 40)));
        }
        waterfall.AddRow("[bold]Total[/]", $"[bold]{totalMs:F0}ms[/]", "");
        AnsiConsole.Write(waterfall);

        // Extractor table
        if (report.Extractors.Length > 0)
        {
            AnsiConsole.WriteLine();
            var extractorTable = new Table()
                .Border(TableBorder.Rounded)
                .Title("Extractors")
                .AddColumn("Name")
                .AddColumn(new TableColumn("Time").RightAligned())
                .AddColumn(new TableColumn("+Types").RightAligned())
                .AddColumn(new TableColumn("+Dets").RightAligned())
                .AddColumn("Status");

            foreach (var ex in report.Extractors.Take(25))
            {
                var status = ex.Skipped
                    ? $"[dim]skipped: {ex.SkipReason ?? "?"}[/]"
                    : "[green]ran[/]";
                var name = ex.Skipped ? $"[dim]{ex.Name}[/]" : ex.Name;
                extractorTable.AddRow(name, $"{ex.Elapsed.TotalMilliseconds:F0}ms",
                    ex.TypesAdded.ToString(), ex.DetectionsAdded.ToString(), status);
            }
            AnsiConsole.Write(extractorTable);
        }

        // Scorer funnel
        if (report.Scorers.Length > 0)
        {
            AnsiConsole.WriteLine();
            var scorerTable = new Table()
                .Border(TableBorder.Rounded)
                .Title("Scorer Funnel")
                .AddColumn("Scorer")
                .AddColumn(new TableColumn("Before").RightAligned())
                .AddColumn(new TableColumn("After").RightAligned())
                .AddColumn(new TableColumn("Delta").RightAligned());

            foreach (var sc in report.Scorers)
            {
                var delta = sc.TypesBefore > 0
                    ? (sc.TypesBefore - sc.TypesAfter) * 100 / sc.TypesBefore
                    : 0;
                scorerTable.AddRow(sc.Name, sc.TypesBefore.ToString(),
                    sc.TypesAfter.ToString(), $"{delta}%");
            }
            AnsiConsole.Write(scorerTable);
        }

        // Cache + corpus chips
        var cachePct = report.Cache.TextHits > 0
            ? (double)(report.Cache.TextHits + report.Cache.SyntaxTreeHits) /
              (report.Cache.TextHits + report.Cache.TextMisses +
               report.Cache.SyntaxTreeHits + report.Cache.SyntaxTreeMisses) * 100
            : 0;

        var chips = $"cache {cachePct:F0}% hit · {report.Corpus.CSharpFiles} files · {report.Corpus.Projects} projects";
        AnsiConsole.MarkupLine($"[dim]{chips}[/]");
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
            .AddRow("[bold]Version[/]", $"v{DevContextVersion.Display}");

        AnsiConsole.Write(summary);
    }

    private static bool HandleStrictMode(RenderedContext result)
    {
        var failures = result.SelfCheckFailures;
        if (failures.Length == 0)
            return false;

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[red]Self-Check Failures[/]")
            .AddColumn("Check")
            .AddColumn("Detail");

        foreach (var failure in failures)
        {
            var parts = failure.Split(": ", 2);
            var check = parts.Length > 0 ? parts[0] : failure;
            var detail = parts.Length > 1 ? parts[1] : "";
            table.AddRow($"[red]{check.EscapeMarkup()}[/]", detail.EscapeMarkup());
        }
        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[red]Strict mode: {failures.Length} self-check failure(s) detected. Exit code 2.[/]");
        return true;
    }
}
