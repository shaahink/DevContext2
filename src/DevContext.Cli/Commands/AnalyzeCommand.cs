using System.Diagnostics;

using DevContext.Cli.Observers;
using DevContext.Cli.Settings;
using DevContext.Core.Insights;
using DevContext.Core.Services;

namespace DevContext.Cli.Commands;

public sealed class AnalyzeCommand : AsyncCommand<AnalyzeSettings>
{
    private readonly IFileSystem _fs;
    private readonly ILoggerFactory _loggerFactory;

    public AnalyzeCommand(
        IFileSystem fs,
        ILoggerFactory loggerFactory)
    {
        _fs = fs;
        _loggerFactory = loggerFactory;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, AnalyzeSettings settings, CancellationToken ct)
    {
        ConfigureLogging(settings);
        var config = DevContextConfig.Load(DevContextConfig.DefaultPath);
        ShowConfigWarnings(config);

        var inputPath = settings.Path ?? ".";

        // GitHub repo support: clone if URL detected. An explicit --repo is always honored as a repo
        // reference. The positional path, though, is only tried as "owner/repo" shorthand when it does
        // NOT already exist on disk — otherwise any one-slash relative path (eval-repos/eShop,
        // src/DevContext.Core) gets hijacked as a GitHub shorthand and fails with a confusing
        // "Repository not found" instead of being analyzed locally (E9: local-path existence beats
        // owner/repo shorthand).
        var gitClonePath = null as string;
        var fullInputPath = _fs.GetFullPath(inputPath);
        var inputExistsLocally = _fs.FileExists(fullInputPath) || _fs.DirectoryExists(fullInputPath);
        var repoUrl = settings.Repo is { Length: > 0 }
            ? RepoUrl.Parse(settings.Repo)
            : inputExistsLocally ? null : RepoUrl.Parse(inputPath);
        if (repoUrl is { IsValid: true })
        {
            var git = new GitCloneService(new CloneRegistry());
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
                return 4;
            }

            gitClonePath = repoUrl.ClonePath;
            var branch = settings.Ref ?? repoUrl.Ref;
            var cloneResult = await git.CloneAsync(repoUrl, gitClonePath, branch, null, ct);
            if (cloneResult is null)
            {
                AnsiConsole.MarkupLine("[red]Clone failed[/]");
                return 4;
            }

            inputPath = gitClonePath;
        }

        ProjectRootResult rootResult;
        try
        {
            rootResult = await ProjectRootResolver.ResolveAsync(inputPath, _fs, ct);
        }
        catch (DirectoryNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            return 2;
        }

        // E9: an empty/invalid directory — no .sln/.slnx anywhere reachable (this dir, its ancestors,
        // or one level of subfolders) and no .csproj underneath it either — is a hard error, not a
        // silent "folder mode" analysis of nothing. Analyzing it used to fall through to the legacy
        // pre-kernel renderer with a near-empty graph and exit 0, looking like a successful run.
        if (rootResult is { Method: ResolutionMethod.FolderMode, EntryCandidates.Length: 0 })
        {
            AnsiConsole.MarkupLine($"[red]No .sln, .slnx, or .csproj found at or under '{inputPath}'.[/]");
            AnsiConsole.MarkupLine("[dim]Point --path at a solution, project, or a folder that contains one.[/]");
            return 2;
        }

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

        // Print explanation and warnings. Escaped: a focus like "[RelayCommand] X.Y" would
        // otherwise parse as a Spectre style tag and throw.
        AnsiConsole.MarkupLine($"[dim]{Markup.Escape(resolvedIntent.Explanation)}[/]");
        foreach (var warning in resolvedIntent.Warnings)
            AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(warning)}[/]");

        // --task deprecation
        if (!string.IsNullOrWhiteSpace(settings.Task))
            AnsiConsole.MarkupLine("[yellow]--task is deprecated. Use --focus instead.[/]");

        var options = new ExtractionOptions
        {
            EntryPaths = rootResult.EntryCandidates,
            Profile = resolvedIntent.Profile,
            MaxOutputTokens = settings.MaxTokens ?? config?.MaxOutputTokens ?? 8000,
            AllowRoslyn = !settings.NoRoslyn,
            BuildFullGraph = !settings.Lite,
            Fast = settings.Fast,
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
                ?? ExtractionOptions.DefaultExcludePatterns,
            ExcludeExtractors = settings.Fast
                ? resolvedIntent.Scenario.DisableExtractors
                    .AddRange((string[])["InMemoryEventBusExtractor", "AntiPatternDetector", "IndirectWiringDetector"])
                : resolvedIntent.Scenario.DisableExtractors,
        };

        var scenario = resolvedIntent.Scenario;

        var cache = new AnalysisCache(_fs);
        var analysis = new SharedAnalysisContext
        {
            UnresolvedFocusPoints = resolvedIntent.FocusPoints,
            FocusPoints = resolvedIntent.FocusPoints,
        };
        var pipeline = BuildPipeline(cache);

        RenderedContext result = null!;
        AnalysisSnapshot? snapshot = null;
        var sw = Stopwatch.StartNew();

        var collector = new RunReportCollector();
        collector.SetBudget(options.MaxOutputTokens);

        var spectreObserver = new SpectreDiscoveryObserver();
        var inner = new List<IDiscoveryObserver> { spectreObserver };
        inner.Add(collector);
        var observer = new CompositeDiscoveryObserver([.. inner]);

        // I8 — snapshot cache check
        var (repoKey, versionKey) = DevContext.Core.Analysis.SnapshotCacheService.ComputeKeys(rootResult.EffectiveRootPath);
        var snapCache = new DevContext.Core.Analysis.SnapshotCacheService();
        var fromCache = false;
        if (!settings.NoCache && snapCache.Exists(repoKey, versionKey))
        {
            snapshot = await snapCache.TryLoadAsync<AnalysisSnapshot>(repoKey, versionKey, ct);
            if (snapshot is not null)
            {
                fromCache = true;
                snapshot = snapshot with { Options = options, RootPath = rootResult.EffectiveRootPath };
            }
        }
        if (!fromCache && settings.CacheOnly)
        {
            AnsiConsole.MarkupLine("[red]No cached snapshot available and --cache-only was specified.[/]");
            return 3;
        }

        var ctx = new DiscoveryContext
        {
            RootPath = rootResult.EffectiveRootPath,
            ScopedProjectDirs = rootResult.ScopeProjectDirs,
            Options = options,
            ActiveScenario = scenario,
            Observer = observer,
            FileSystem = _fs,
            Cache = cache,
            Analysis = analysis,
            Logger = _loggerFactory.CreateLogger("DevContext")
        };

        if (!fromCache)
        {
            await AnsiConsole.Status()
                .StartAsync("Analyzing project...", async statusCtx =>
                {
                    var capturedSnapshot = await pipeline.AnalyzeAsync(ctx, ct);
                    snapshot = capturedSnapshot;

                    // I8 — save snapshot to cache (write-behind after analysis)
                    _ = snapCache.SaveAsync(repoKey, versionKey, capturedSnapshot, ct);

                    if (capturedSnapshot.IsDryRun)
                    {
                        result = new RenderedContext(capturedSnapshot.DryRunContent!, 0, [], TimeSpan.Zero, "2.0");
                    }
                    else
                    {
                        result = await Render(capturedSnapshot, pipeline, settings, options, scenario, focusText, ct);
                    }
                });
        }
        else
        {
            // I8 — render from cached snapshot (no re-analysis needed)
            result = await Render(snapshot!, pipeline, settings, options, scenario, focusText, ct);
            var elapsed = sw.ElapsedMilliseconds;
            var fromCacheStamp = $"  from cache · {versionKey[..Math.Min(7, versionKey.Length)]} · {elapsed}ms";
            AnsiConsole.MarkupLine($"[dim]{fromCacheStamp}[/]");
        }

        await WriteOutput(settings, result);
        if (settings.Strict && HandleStrictMode(result))
            return 2;

        if (snapshot?.Report is { } report)
        {
            var summary = RunReportFormatter.Summary(report, result.RenderFunnel, result.GraphSummary, result.EstimatedTokens);
            if (!settings.DryRun && !settings.Quiet)
                AnsiConsole.MarkupLine($"[dim]{summary}[/]");
        }

        if ((settings.Stats || settings.Metrics) && !settings.Quiet)
            ShowStats(snapshot?.Report, result.GraphSummary, snapshot?.Insights ?? default);

        if (!settings.Quiet)
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

    private static async Task<RenderedContext> Render(AnalysisSnapshot snapshot, DiscoveryPipeline pipeline,
        AnalyzeSettings settings, ExtractionOptions options, Scenario scenario, string? focusText, CancellationToken ct)
    {
        if (snapshot.IsDryRun)
            return new RenderedContext(snapshot.DryRunContent!, 0, [], TimeSpan.Zero, "2.0");

        var traceDetail = settings.Detail?.ToLowerInvariant() switch
        {
            "signature" => TraceDetail.Signature,
            "salient" => TraceDetail.Salient,
            "full" => TraceDetail.Full,
            _ => TraceDetail.Salient,
        };

        var request = new RenderRequest
        {
            Format = options.OutputFormat.ToString().ToLowerInvariant(),
            MaxTokens = options.MaxOutputTokens,
            Sections = scenario.RequiredSections,
            IncludeProvenance = options.IncludeProvenance,
            IncludeDiagnostics = options.IncludeDiagnostics,
            TokenView = options.TokenView,
            Entry = focusText,
            Depth = settings.Depth,
            Detail = traceDetail,
            IncludeMapWithTrace = settings.IncludeMapWithTrace,
        };

        return await pipeline.RenderAsync(snapshot, request, ct);
    }

    private static async Task WriteOutput(AnalyzeSettings settings, RenderedContext result)
    {
        if (settings.Output is not null)
        {
            await File.WriteAllTextAsync(settings.Output, result.Content);
            if (!settings.Quiet)
                AnsiConsole.MarkupLine($"[green]Output written to {Path.GetFullPath(settings.Output)}[/]");
            return;
        }

        if (!settings.Quiet)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine(result.Content);
        }
    }

    private static void ShowStats(RunReport? report, GraphSummary? graph = null, ImmutableArray<Insight> insights = default)
    {
        if (report is null) return;

        AnsiConsole.WriteLine();

        // Insights (per I3.3 — render first)
        if (insights is { IsDefaultOrEmpty: false })
        {
            var insightTable = new Table()
                .Border(TableBorder.Rounded)
                .Title("Insights")
                .AddColumn("Sev")
                .AddColumn("Category")
                .AddColumn("Title")
                .AddColumn("Evidence");

            foreach (var i in insights)
            {
                var sev = i.Severity switch
                {
                    Severity.Warning => "[red]WARN[/]",
                    Severity.Notable => "[yellow]NOTE[/]",
                    _ => "[blue]INFO[/]",
                };
                insightTable.AddRow(sev, i.Category.ToString(), i.Title, string.Join(", ", i.Evidence.Take(3)));
            }

            AnsiConsole.Write(insightTable);
        }

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

        // Graph seam coverage — how much wiring was bridged and how confidently (the Map/Trace
        // equivalent of the legacy scorer funnel). "approx" = resolved syntactically only.
        if (graph is { Seams.Length: > 0 })
        {
            AnsiConsole.WriteLine();
            var seamTable = new Table()
                .Border(TableBorder.Rounded)
                .Title("Graph Seams")
                .AddColumn("Seam")
                .AddColumn(new TableColumn("Edges").RightAligned())
                .AddColumn(new TableColumn("Approx").RightAligned());
            foreach (var s in graph.Seams)
                seamTable.AddRow(s.Seam, s.Count.ToString(), s.Approx > 0 ? $"[yellow]{s.Approx}[/]" : "0");
            AnsiConsole.Write(seamTable);

            if (graph.Entries > 0)
                AnsiConsole.MarkupLine(
                    $"[dim]{graph.Nodes} nodes · {graph.Edges} edges · {graph.EntriesWithTarget}/{graph.Entries} entries → target[/]");
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
