using System.Diagnostics;

using DevContext.Cli.Settings;
using DevContext.Core.Graph;
using DevContext.Core.Services;

namespace DevContext.Cli.Commands;

/// <summary>Produces a full report document — identity, stats, top flows, top-3 traces,
/// insights, architecture map, and run report — in one deterministic markdown (or JSON) doc.</summary>
public sealed class ReportCommand : AsyncCommand<ReportSettings>
{
    private readonly IFileSystem _fs;
    private readonly ILoggerFactory _loggerFactory;

    public ReportCommand(IFileSystem fs, ILoggerFactory loggerFactory)
    {
        _fs = fs;
        _loggerFactory = loggerFactory;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, ReportSettings settings, CancellationToken ct)
    {
        ConfigureLogging(settings);

        var inputPath = settings.Path ?? ".";

        // ── Input resolution (local path vs GitHub URL) ──
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

            var status = await git.ValidateAsync(repoUrl, ct);
            if (status != RepoStatus.Valid)
            {
                var msg = status switch
                {
                    RepoStatus.NotFound => "Repository not found.",
                    RepoStatus.Private => "Private repositories require authentication.",
                    RepoStatus.NetworkError => "Network error.",
                    RepoStatus.RateLimited => "Rate limited.",
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

        // ── Project root resolution ──
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

        if (rootResult is { Method: ResolutionMethod.FolderMode, EntryCandidates.Length: 0 })
        {
            AnsiConsole.MarkupLine($"[red]No .sln, .slnx, or .csproj found at or under '{inputPath}'.[/]");
            return 2;
        }

        // ── Build options (always Focused profile, full graph for a complete report) ──
        var options = new ExtractionOptions
        {
            EntryPaths = rootResult.EntryCandidates,
            Profile = ExtractionProfile.Focused,
            MaxOutputTokens = 32_000,
            AllowRoslyn = true,
            BuildFullGraph = true,
            OutputFormat = OutputFormat.Markdown,
            ExcludePatterns = ExtractionOptions.DefaultExcludePatterns,
        };

        var scenario = ScenarioRegistry.BuiltIn["overview"];
        var cache = new AnalysisCache(_fs);
        var analysis = new SharedAnalysisContext();
        var pipeline = BuildPipeline(cache);

        // ── Snapshot cache check ──
        var (repoKey, versionKey) = DevContext.Core.Analysis.SnapshotCacheService.ComputeKeys(rootResult.EffectiveRootPath);
        var snapCache = new DevContext.Core.Analysis.SnapshotCacheService();
        AnalysisSnapshot? snapshot = null;
        var fromCache = false;

        if (!settings.NoCache && snapCache.Exists(repoKey, versionKey))
        {
            snapshot = await snapCache.TryLoadAsync(repoKey, versionKey, ct);
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

        var collector = new RunReportCollector();
        collector.SetBudget(options.MaxOutputTokens);

        var ctx = new DiscoveryContext
        {
            RootPath = rootResult.EffectiveRootPath,
            ScopedProjectDirs = rootResult.ScopeProjectDirs,
            Options = options,
            ActiveScenario = scenario,
            Observer = new CompositeDiscoveryObserver([collector]),
            FileSystem = _fs,
            Cache = cache,
            Analysis = analysis,
            Logger = _loggerFactory.CreateLogger("DevContext"),
        };

        // ── Analyze ──
        if (!fromCache)
        {
            DevContext.Core.Analysis.SnapshotSaveResult? saveResult = null;
            await AnsiConsole.Status()
                .StartAsync("Analyzing repo...", async _ =>
                {
                    snapshot = await pipeline.AnalyzeAsync(ctx, ct);
                    if (!settings.NoCache)
                        saveResult = await snapCache.SaveAsync(repoKey, versionKey, snapshot, ct);
                });
            if (saveResult is { Success: false })
                AnsiConsole.MarkupLine($"[yellow]snapshot cache save failed: {Markup.Escape(saveResult.Error ?? "unknown")}[/]");
        }

        if (snapshot is null)
        {
            AnsiConsole.MarkupLine("[red]Analysis produced no result.[/]");
            return 1;
        }

        // ── Render ──
        var format = settings.Format?.ToLowerInvariant() == "json" ? "json" : "markdown";
        RenderedContext result;

        if (format == "json")
        {
            var req = new RenderRequest { Format = "json", MaxTokens = 32_000 };
            result = await pipeline.RenderAsync(snapshot, req, ct);
        }
        else if (snapshot.Graph is { NodeCount: > 0 })
        {
            var query = new GraphQuery(snapshot.Graph, snapshot.Entries, snapshot.Map);
            result = await ReportRenderer.RenderAsync(snapshot, query, ct);
        }
        else
        {
            var sln = Path.GetFileName(snapshot.RootPath.TrimEnd('/', '\\'));
            result = new RenderedContext(
                $"# REPORT: {sln}\n\n_No analysis data available._\n",
                0, [], TimeSpan.Zero, "devcontext/report-v1");
        }

        // ── Write output ──
        if (settings.Output is not null)
        {
            await File.WriteAllTextAsync(settings.Output, result.Content, CancellationToken.None);
            if (!settings.Quiet)
                AnsiConsole.MarkupLine($"[green]Report written to {Path.GetFullPath(settings.Output)}[/]");
        }
        else if (!settings.Quiet)
        {
            AnsiConsole.WriteLine(result.Content);
        }

        // ── Cleanup ──
        if (gitClonePath is not null)
            GitCloneService.Cleanup(gitClonePath);

        return 0;
    }

    private DiscoveryPipeline BuildPipeline(IAnalysisCache cache)
    {
        var services = new ServiceCollection();
        services.AddDevContextServices(".");
        services.AddSingleton(_loggerFactory.CreateLogger<DiscoveryPipeline>());
        var sp = services.BuildServiceProvider();
        return sp.GetRequiredService<DiscoveryPipeline>();
    }

    private static void ConfigureLogging(ReportSettings settings)
    {
        var level = settings.Quiet ? Serilog.Events.LogEventLevel.Error : Serilog.Events.LogEventLevel.Warning;
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}
