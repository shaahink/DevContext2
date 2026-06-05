using System.Diagnostics;
using DevContext.Cli.Observers;
using DevContext.Cli.Services;

var argsList = args.ToList();

if (argsList.Count == 0 || argsList[0] is "help" or "--help" or "-h")
{
    PrintHelp();
    return 0;
}

var command = argsList[0].ToLowerInvariant();
argsList.RemoveAt(0);

try
{
    return command switch
    {
        "analyze" or "run" => await RunAnalyze(argsList),
        "init" => RunInit(argsList),
        "scenarios" => RunScenarios(argsList),
        "version" or "--version" => RunVersion(argsList),
        _ => await RunAnalyze(new List<string> { command }.Concat(argsList).ToList())
    };
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    return 1;
}

static void PrintHelp()
{
    AnsiConsole.MarkupLine("[bold]DevContext v2.0.0[/] - Static analysis CLI for .NET projects");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("Usage:");
    AnsiConsole.MarkupLine("  devcontext [PATH] [options]    Main analysis command");
    AnsiConsole.MarkupLine("  devcontext init                Create devcontext.json in cwd");
    AnsiConsole.MarkupLine("  devcontext scenarios           List available scenarios");
    AnsiConsole.MarkupLine("  devcontext version             Show version");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("Options:");
    AnsiConsole.MarkupLine("  -a, --around <PATH>       Entry point (repeatable)");
    AnsiConsole.MarkupLine("  -s, --scenario <NAME>     Analysis scenario");
    AnsiConsole.MarkupLine("  -p, --profile <NAME>      quick | focused | debug | full");
    AnsiConsole.MarkupLine("  -t, --task <TEXT>         Free-text intent");
    AnsiConsole.MarkupLine("      --max-tokens <N>      Token cap (default 8000)");
    AnsiConsole.MarkupLine("  -o, --output <FILE>       Write to file");
    AnsiConsole.MarkupLine("      --format <FMT>        markdown | json");
    AnsiConsole.MarkupLine("      --include-provenance  Include provenance info");
    AnsiConsole.MarkupLine("      --include-diagnostics Include diagnostics");
    AnsiConsole.MarkupLine("      --no-roslyn           Disable deep tier");
    AnsiConsole.MarkupLine("      --verbose             Info-level logging");
    AnsiConsole.MarkupLine("      --trace               Debug-level logging");
    AnsiConsole.MarkupLine("      --dry-run             Plan only");
    AnsiConsole.MarkupLine("      --metrics             Emit structured per-extractor timing report after run");
}

static (string? path, Dictionary<string, string> opts) ParseArgs(List<string> args)
{
    string? path = null;
    var opts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    for (int i = 0; i < args.Count; i++)
    {
        var arg = args[i];
        if (arg.StartsWith("--"))
        {
            var key = arg[2..];
            var eqIdx = key.IndexOf('=');
            if (eqIdx >= 0)
            {
                opts[key[..eqIdx]] = key[(eqIdx + 1)..];
            }
            else if (i + 1 < args.Count && !args[i + 1].StartsWith("-"))
            {
                opts[key] = args[++i];
            }
            else
            {
                opts[key] = "true";
            }
        }
        else if (arg.StartsWith("-") && arg.Length > 1)
        {
            var shortName = arg[1].ToString();
            if (i + 1 < args.Count && !args[i + 1].StartsWith("-"))
            {
                opts[shortName] = args[++i];
            }
            else
            {
                opts[shortName] = "true";
            }
        }
        else if (path == null)
        {
            path = arg;
        }
    }

    return (path, opts);
}

static async Task<int> RunAnalyze(List<string> args)
{
    var (path, opts) = ParseArgs(args);
    var rootPath = path ?? Environment.CurrentDirectory;

    if (opts.ContainsKey("verbose"))
        Log.Logger = CreateLogger(Serilog.Events.LogEventLevel.Information);
    else if (opts.ContainsKey("trace"))
        Log.Logger = CreateLogger(Serilog.Events.LogEventLevel.Debug);
    else
        Log.Logger = CreateLogger(Serilog.Events.LogEventLevel.Warning);

    var fs = new RealFileSystem();

    var resolver = new ProjectRootResolver();
    var rootResult = await resolver.ResolveAsync(rootPath, fs);

    var config = DevContextConfig.Load(DevContextConfig.DefaultPath);

    if (config is not null)
    {
        var errors = config.Validate();
        foreach (var error in errors)
            AnsiConsole.MarkupLine($"[yellow]Config warning: {error}[/]");
    }

    var around = new List<string>();
    if (opts.TryGetValue("a", out var a)) around.Add(a);
    if (opts.TryGetValue("around", out var aroundVal)) around.Add(aroundVal);

    var scenarioName = opts.GetValueOrDefault("s") ?? opts.GetValueOrDefault("scenario") ?? config?.DefaultScenario ?? "architecture";
    var dryRun = opts.ContainsKey("dry-run");
    var format = opts.GetValueOrDefault("format") ?? "markdown";

    string? resolvedScenario = null;
    if (opts.TryGetValue("t", out var taskText) || opts.TryGetValue("task", out taskText))
    {
        var (inferred, inferredProfile) = IntentInferrer.Infer(taskText);
        resolvedScenario = inferred;
        if (!opts.ContainsKey("p") && !opts.ContainsKey("profile"))
            opts["profile"] = inferredProfile.ToString().ToLowerInvariant();
    }

    var activeScenarioName = resolvedScenario ?? scenarioName;
    if (!ScenarioRegistry.BuiltIn.TryGetValue(activeScenarioName, out var scenario))
    {
        AnsiConsole.MarkupLine($"[red]Unknown scenario: {activeScenarioName}[/]");
        AnsiConsole.MarkupLine("Available: " + string.Join(", ", ScenarioRegistry.BuiltIn.Keys));
        return 1;
    }

    var profileName = opts.GetValueOrDefault("p") ?? opts.GetValueOrDefault("profile") ?? config?.DefaultProfile ?? "focused";
    var profile = profileName.ToLowerInvariant() switch
    {
        "quick" => ExtractionProfile.Quick,
        "debug" => ExtractionProfile.Debug,
        "full" => ExtractionProfile.Full,
        _ => ExtractionProfile.Focused
    };

    var maxTokens = int.TryParse(opts.GetValueOrDefault("max-tokens"), out var mt) ? mt : config?.MaxOutputTokens ?? 8000;
    var noRoslyn = opts.ContainsKey("no-roslyn") || config?.Profiles?.GetValueOrDefault(profileName)?.NoRoslyn == true;
    var includeProvenance = opts.ContainsKey("include-provenance");
    var includeDiagnostics = opts.ContainsKey("include-diagnostics");
    var emitMetrics = opts.ContainsKey("metrics");

    var options = new ExtractionOptions
    {
        EntryPaths = rootResult.EntryCandidates,
        Profile = profile,
        MaxOutputTokens = maxTokens,
        AllowRoslyn = !noRoslyn,
        DryRun = dryRun,
        IncludeProvenance = includeProvenance,
        IncludeDiagnostics = includeDiagnostics,
        OutputFormat = format.ToLowerInvariant() switch
        {
            "json" => OutputFormat.Json,
            _ => OutputFormat.Markdown
        },
        ExcludePatterns = config?.ExcludePatterns?.ToImmutableArray()
            ?? [".git", "bin", "obj", ".vs", "node_modules", ".idea"]
    };

    var cache = new AnalysisCache(fs);

    var focusPoints = around
        .Select(a => FocusPointParser.Parse(a, fs))
        .Where(fp => fp != null)
        .Select(fp => fp!)
        .ToImmutableArray();

    var sharedAnalysis = new SharedAnalysisContext
    {
        FocusPoints = focusPoints
    };

    var loggerFactory = LoggerFactory.Create(b =>
        b.AddSerilog(dispose: true));

    IRoslynWorkspaceProvider roslynProvider;
    if (!noRoslyn && rootResult.SolutionFilePath != null)
    {
        roslynProvider = new DevContext.Roslyn.Services.RoslynWorkspaceProvider(
            rootResult.SolutionFilePath,
            fs,
            loggerFactory.CreateLogger<DevContext.Roslyn.Services.RoslynWorkspaceProvider>());
    }
    else
    {
        roslynProvider = new NullRoslynProvider();
    }

    var services = new ServiceCollection();
    services.AddDevContextServices(rootPath);
    services.AddSingleton(loggerFactory.CreateLogger<DiscoveryPipeline>());
    var serviceProvider = services.BuildServiceProvider();
    var pipeline = serviceProvider.GetRequiredService<DiscoveryPipeline>();

    var sw = Stopwatch.StartNew();
    RenderedContext result = null!;

    var metricsObserver = emitMetrics ? new MetricsDiscoveryObserver() : null;

    AnsiConsole.Status()
        .Start(dryRun ? "DevContext Dry Run..." : "DevContext Analysis...", statusCtx =>
        {
            var spectreObserver = new SpectreDiscoveryObserver(dryRun ? null : statusCtx);
            var compositeObserver = emitMetrics && metricsObserver != null
                ? new CompositeDiscoveryObserver(spectreObserver, metricsObserver)
                : (IDiscoveryObserver)spectreObserver;

            var ctx = new DiscoveryContext
            {
                RootPath = rootResult.RootPath,
                Options = options,
                ActiveScenario = scenario,
                Observer = compositeObserver,
                FileSystem = fs,
                Cache = cache,
                Analysis = sharedAnalysis,
                Logger = loggerFactory.CreateLogger("DevContext"),
                RoslynWorkspace = roslynProvider
            };

            result = pipeline.RunAsync(ctx).GetAwaiter().GetResult();
        });

    if (dryRun)
    {
        AnsiConsole.MarkupLine("[yellow]Dry Run Plan[/]");
        AnsiConsole.WriteLine(result.Content);
    }
    else
    {
        if (opts.TryGetValue("o", out var outputFile) || opts.TryGetValue("output", out outputFile))
        {
            await File.WriteAllTextAsync(outputFile, result.Content);
            AnsiConsole.MarkupLine($"[green]Output written to {Path.GetFullPath(outputFile)}[/]");
        }
        else
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine(result.Content);
        }
    }

    if (emitMetrics && metricsObserver != null)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine(metricsObserver.GetMetricsSummary());
    }

    var totalMs = sw.ElapsedMilliseconds;
    var box = new Panel(
        Align.Center(
            new Markup(
                $"[bold]DevContext v2.0.0[/] · {Path.GetFileName(rootResult.SolutionFilePath ?? rootPath)} · {totalMs}ms total\n" +
                $"Types: ~{result.EstimatedTokens} tokens (budget {options.MaxOutputTokens})")));
    box.Border = BoxBorder.Rounded;
    AnsiConsole.Write(box);

    return 0;
}

static int RunInit(List<string> args)
{
    var path = DevContextConfig.DefaultPath;
    if (File.Exists(path))
    {
        AnsiConsole.MarkupLine("[yellow]devcontext.json already exists[/]");
        return 0;
    }

    var config = new
    {
        defaultProfile = "focused",
        defaultScenario = "debug-endpoint",
        maxOutputTokens = 6000,
        excludePatterns = new[] { ".git", "bin", "obj", "Migrations" },
        entryPaths = new[] { "src/Api" }
    };

    var json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions
    {
        WriteIndented = true
    });

    File.WriteAllText(path, json);
    AnsiConsole.MarkupLine($"[green]Created {path}[/]");
    return 0;
}

static int RunScenarios(List<string> args)
{
    var table = new Table();
    table.AddColumn("Name");
    table.AddColumn("Display Name");
    table.AddColumn("Description");
    table.AddColumn("Required Sections");

    foreach (var (name, scenario) in ScenarioRegistry.BuiltIn)
    {
        table.AddRow(
            name,
            scenario.DisplayName,
            scenario.Description ?? "",
            string.Join(", ", scenario.RequiredSections));
    }

    AnsiConsole.Write(table);
    return 0;
}

static int RunVersion(List<string> args)
{
    AnsiConsole.MarkupLine($"[bold]DevContext[/] v2.0.0");
    return 0;
}

static Serilog.ILogger CreateLogger(Serilog.Events.LogEventLevel level)
{
    return new LoggerConfiguration()
        .MinimumLevel.Is(level)
        .WriteTo.Console(
            outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();
}

public sealed class NullRoslynProvider : IRoslynWorkspaceProvider
{
    public Task<IRoslynWorkspace?> GetWorkspaceAsync(CancellationToken ct)
        => Task.FromResult<IRoslynWorkspace?>(null);
}
