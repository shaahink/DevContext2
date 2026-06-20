using System.Diagnostics;
using System.Text;

using DevContext.Cli.Services;
using DevContext.Core.Analysis;
using DevContext.Core.Configuration;
using DevContext.Core.Contracts;
using DevContext.Core.IO;
using DevContext.Core.Models;
using DevContext.Core.Pipeline;
using DevContext.Core.Resolvers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevContext.Benchmarks;

/// <summary>
/// Deterministic macro benchmark: runs the REAL analysis pipeline (incl. CallGraphExtractor) over the
/// standing eval repos in Map and Trace modes, warm-up + N timed iterations, and publishes a markdown
/// table (total wall · per-stage wall · top extractors · graph counts) to benchmarks/results/. Reuses
/// the CLI composition root (AddDevContextServices) so the extractor set never drifts from production.
/// </summary>
public static class RepoBenchmark
{
    private const int Warmups = 1;
    private const int Iterations = 3;

    private sealed record RepoCase(string Name, string Path, string? Focus, bool IsAbsolute);

    public static async Task RunAsync(string[] args)
    {
        var root = FindWorkspaceRoot();
        var eval = Path.Combine(root, "eval-repos");
        var dntSite = @"C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default";

        var cases = new List<RepoCase>
        {
            new("DntSite", dntSite, "FeedController", IsAbsolute: true),
            new("TodoApi", Path.Combine(eval, "TodoApi"), "POST /todos/", false),
            new("VerticalSlice", Path.Combine(eval, "VerticalSlice", "MinimalClean"), "POST /Products", false),
            new("eShop.Ordering.API", Path.Combine(eval, "eShop", "src", "Ordering.API"), "POST /api/orders/", false),
            new("AutoMapper", Path.Combine(eval, "AutoMapper"), null, false),
            new("OrchardCore", Path.Combine(eval, "OrchardCore"), null, false),
        };

        var only = args.Skip(1).Where(a => !a.StartsWith('-')).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (only.Count > 0)
            cases = cases.Where(c => only.Contains(c.Name)).ToList();

        var fs = new RealFileSystem();
        var pipeline = BuildPipeline();

        var rows = new List<RunRow>();
        foreach (var c in cases)
        {
            if (!Directory.Exists(c.Path))
            {
                Console.WriteLine($"[skip] {c.Name} — path not found: {c.Path}");
                continue;
            }

            Console.WriteLine($"[run ] {c.Name} (Map)…");
            var map = await MeasureAsync(pipeline, fs, c.Path, focus: null);
            if (map is not null) rows.Add(map with { Repo = c.Name, Mode = "Map" });

            if (c.Focus is not null)
            {
                Console.WriteLine($"[run ] {c.Name} (Trace: {c.Focus})…");
                var trace = await MeasureAsync(pipeline, fs, c.Path, c.Focus);
                if (trace is not null) rows.Add(trace with { Repo = c.Name, Mode = $"Trace ({c.Focus})" });
            }
        }

        var md = Render(rows);
        Console.WriteLine();
        Console.WriteLine(md);

        var outDir = Path.Combine(root, "benchmarks", "results");
        Directory.CreateDirectory(outDir);
        var stamp = DateTime.Now.ToString("yyyy-MM-dd-HHmm");
        var file = Path.Combine(outDir, $"PERF-{stamp}.md");
        File.WriteAllText(file, md);
        // Also (re)write baseline.md if it doesn't exist yet — first run is the baseline.
        var baseline = Path.Combine(outDir, "baseline.md");
        if (!File.Exists(baseline)) File.WriteAllText(baseline, md);
        Console.WriteLine($"\nWrote {file}");
    }

    private sealed record RunRow
    {
        public string Repo { get; init; } = "";
        public string Mode { get; init; } = "";
        public double MedianMs { get; init; }
        public double MinMs { get; init; }
        public double MaxMs { get; init; }
        public int Files { get; init; }
        public int Nodes { get; init; }
        public int Edges { get; init; }
        public int Entries { get; init; }
        public double Stage2Ms { get; init; }
        public double Stage3Ms { get; init; }
        public string TopExtractors { get; init; } = "";
        public string CallGraphPhases { get; init; } = "";
    }

    private static async Task<RunRow?> MeasureAsync(DiscoveryPipeline pipeline, IFileSystem fs, string repoPath, string? focus)
    {
        try
        {
            var rootResult = await ProjectRootResolver.ResolveAsync(repoPath, fs);
            var intent = AnalysisIntentResolver.Resolve(new IntentInput { Focus = focus });

            var samples = new List<(double Ms, RunReport Report, AnalysisSnapshot Snap)>();

            for (var i = 0; i < Warmups + Iterations; i++)
            {
                var collector = new RunReportCollector();
                var ctx = new DiscoveryContext
                {
                    RootPath = rootResult.EffectiveRootPath,
                    ScopedProjectDirs = rootResult.ScopeProjectDirs,
                    Options = new ExtractionOptions
                    {
                        MaxOutputTokens = 8000,
                        OutputFormat = OutputFormat.Markdown,
                        AllowRoslyn = true,
                        Profile = intent.Profile,
                        ExcludePatterns = [".git", "bin", "obj", ".vs", "node_modules", ".idea"],
                    },
                    ActiveScenario = intent.Scenario,
                    Observer = collector,
                    FileSystem = fs,
                    Cache = new AnalysisCache(fs),
                    Analysis = new SharedAnalysisContext
                    {
                        FocusPoints = intent.FocusPoints,
                        UnresolvedFocusPoints = intent.FocusPoints,
                    },
                    Logger = LoggerFactory.Create(_ => { }).CreateLogger("bench"),
                };

                var snap = await pipeline.AnalyzeAsync(ctx);
                var report = collector.Build();
                if (i >= Warmups) samples.Add((report.TotalWall.TotalMilliseconds, report, snap));
            }

            // Pick the median sample so the breakdown (extractors/stages/phases) matches the reported time.
            samples.Sort((a, b) => a.Ms.CompareTo(b.Ms));
            var med = samples[samples.Count / 2];
            var lastReport = med.Report;
            var lastSnap = med.Snap;

            var top = lastReport!.Extractors
                .Where(e => !e.Skipped && e.Elapsed > TimeSpan.Zero)
                .OrderByDescending(e => e.Elapsed)
                .Take(3)
                .Select(e => $"{e.Name} {e.Elapsed.TotalMilliseconds:F0}ms");

            var cgPhases = lastSnap!.Model.Diagnostics
                .Where(d => d.Source == "CallGraphExtractor" && d.Message.Contains("phases:"))
                .Select(d => d.Message[(d.Message.IndexOf("phases:", StringComparison.Ordinal) + 8)..])
                .FirstOrDefault() ?? "";

            return new RunRow
            {
                MedianMs = med.Ms,
                MinMs = samples[0].Ms,
                MaxMs = samples[^1].Ms,
                Files = lastSnap.Analysis.AllSourceFiles?.Count ?? 0,
                Nodes = lastSnap.Graph?.NodeCount ?? 0,
                Edges = lastSnap.Graph?.EdgeCount ?? 0,
                Entries = lastSnap.Entries.Length,
                Stage2Ms = lastReport.Parallelism.Stage2Wall.TotalMilliseconds,
                Stage3Ms = lastReport.Parallelism.Stage3Wall.TotalMilliseconds,
                TopExtractors = string.Join(", ", top),
                CallGraphPhases = cgPhases,
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[fail] {repoPath} ({focus}): {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    private static string Render(List<RunRow> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# DevContext performance — {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine();
        sb.AppendLine($"Real pipeline (incl. call graph) over the standing eval repos. Median of {Iterations} timed iterations after {Warmups} warm-up. Wall time is the full `AnalyzeAsync`.");
        sb.AppendLine();
        sb.AppendLine("| Repo | Mode | Median | Min–Max | Files | Nodes | Edges | Entries | Stage2 | Stage3 | Top extractors (ms) |");
        sb.AppendLine("|---|---|--:|--:|--:|--:|--:|--:|--:|--:|---|");
        foreach (var r in rows)
        {
            sb.AppendLine($"| {r.Repo} | {r.Mode} | {r.MedianMs / 1000:F1}s | {r.MinMs / 1000:F1}–{r.MaxMs / 1000:F1}s | "
                + $"{r.Files} | {r.Nodes} | {r.Edges} | {r.Entries} | {r.Stage2Ms:F0}ms | {r.Stage3Ms:F0}ms | {r.TopExtractors} |");
        }
        sb.AppendLine();
        sb.AppendLine("## Call-graph phase breakdown (Trace runs)");
        sb.AppendLine();
        foreach (var r in rows.Where(r => !string.IsNullOrEmpty(r.CallGraphPhases)))
            sb.AppendLine($"- **{r.Repo}** {r.Mode}: {r.CallGraphPhases}");
        return sb.ToString();
    }

    private static DiscoveryPipeline BuildPipeline()
    {
        var services = new ServiceCollection();
        services.AddDevContextServices(".");
        services.AddSingleton(LoggerFactory.Create(_ => { }).CreateLogger<DiscoveryPipeline>());
        return services.BuildServiceProvider().GetRequiredService<DiscoveryPipeline>();
    }

    private static string FindWorkspaceRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "DevContext.slnx")))
        {
            var parent = Path.GetDirectoryName(dir.TrimEnd(Path.DirectorySeparatorChar));
            if (parent == dir) break;
            dir = parent;
        }
        return dir ?? Environment.CurrentDirectory;
    }
}
