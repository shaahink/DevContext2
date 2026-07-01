using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

using Xunit.Abstractions;

namespace DevContext.Core.Tests;

[Trait("Category", "Eval")]
public sealed class EvalExpectationTests : IDisposable
{
    private readonly ITestOutputHelper _output;

    public EvalExpectationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // Read the expectation files and run one test per repo.
    // Each test runs the full pipeline and evaluates every check.

    public static IEnumerable<object[]> ExpectationFiles()
    {
        var expectationsDir = ResolveExpectationsDir();
        if (!Directory.Exists(expectationsDir))
            yield break;

        foreach (var file in Directory.EnumerateFiles(expectationsDir, "*.json"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            // Skip raw output dumps — only process expectation files
            if (name.EndsWith("-output", StringComparison.OrdinalIgnoreCase))
                continue;
            yield return new object[] { name, file };
        }
    }

    [Theory]
    [MemberData(nameof(ExpectationFiles))]
    public async Task EvalRepo_MatchesExpectations(string repoName, string expectationFile)
    {
        var expectation = JsonSerializer.Deserialize<EvalExpectation>(File.ReadAllText(expectationFile), JsonOpts)!;

        var repoPath = RepoRoot(expectation.Repo);
        if (!Directory.Exists(repoPath))
        {
            _output.WriteLine($"SKIP {repoName}: repo not found at {repoPath}. Clone with: git clone ... {expectation.Repo}");
            return;
        }

        _output.WriteLine($"Running analysis on {repoName} ({repoPath})...");

        // Run pipeline in-process — markdown + json
        var markdownResult = await RunPipelineAsync(repoPath, OutputFormat.Markdown);
        var jsonResult = await RunPipelineAsync(repoPath, OutputFormat.Json);

        var markdownContent = markdownResult.Content;
        var jsonContent = jsonResult.Content;

        // Parse JSON output
        JsonNode? jsonRoot = null;
        try { jsonRoot = JsonNode.Parse(jsonContent); } catch { /* handled below */ }

        var failures = new List<string>();
        var aspirationalNotes = new List<string>();

        foreach (var check in expectation.Checks)
        {
            var (passed, detail) = EvaluateCheck(check, markdownContent, jsonContent, jsonRoot, jsonResult);
            if (passed)
            {
                _output.WriteLine($"  [pass] {check.Id}");
            }
            else if (check.Status == "aspirational")
            {
                var note = check.Note ?? "";
                aspirationalNotes.Add($"ASPIRATIONAL-FAIL {repoName}/{check.Id}: {note} — {detail}");
                _output.WriteLine($"  [aspir] {check.Id} — {detail}");
            }
            else
            {
                failures.Add($"{check.Id}: {detail}");
                _output.WriteLine($"  [FAIL] {check.Id} — {detail}");
            }
        }

        // Print aspirational summary
        if (aspirationalNotes.Count > 0)
        {
            _output.WriteLine("");
            _output.WriteLine($"── {aspirationalNotes.Count} aspirational failure(s) ──");
            foreach (var note in aspirationalNotes)
                _output.WriteLine($"  {note}");
        }

        Assert.Empty(failures);
    }

    // ── check evaluation ──────────────────────────────────────────────

    private static (bool Passed, string Detail) EvaluateCheck(
        EvalCheck check, string markdown, string json, JsonNode? jsonRoot, RenderedContext jsonResult)
    {
        return check.Type switch
        {
            "json-equals" => EvaluateJsonEquals(check, jsonRoot),
            "json-range" => EvaluateJsonRange(check, jsonRoot),
            "json-contains" => EvaluateJsonContains(check, jsonRoot),
            "output-contains" => EvaluateOutputContains(check, markdown, json),
            "output-not-contains" => EvaluateOutputNotContains(check, markdown, json),
            "signal-present" => EvaluateSignalPresent(check, jsonRoot),
            "detection-count" => EvaluateDetectionCount(check, jsonRoot),
            "entry-kind-present" => EvaluateEntryKindPresent(check, jsonRoot, markdown),
            "max-elapsed-ms" => EvaluateMaxElapsed(check, jsonResult),
            _ => (false, $"Unknown check type: {check.Type}")
        };
    }

    private static (bool, string) EvaluateJsonEquals(EvalCheck check, JsonNode? root)
    {
        if (root is null) return (false, "JSON not parseable");
        if (check.Path is null) return (false, "Missing path");
        var node = ResolveJsonPath(root, check.Path);
        if (node is null) return (false, $"Path '{check.Path}' not found in JSON output");
        var actual = node.ToString();
        var expected = check.Value?.ToString() ?? "";
        var match = string.Equals(actual.Trim('"'), expected.Trim('"'), StringComparison.OrdinalIgnoreCase);
        return (match, $"Expected '{expected}', got '{actual}'");
    }

    private static (bool, string) EvaluateJsonRange(EvalCheck check, JsonNode? root)
    {
        if (root is null) return (false, "JSON not parseable");
        if (check.Path is null) return (false, "Missing path");
        var node = ResolveJsonPath(root, check.Path);
        if (node is null) return (false, $"Path '{check.Path}' not found");

        // Try JSON value types
        double d;
        if (node is JsonValue val)
        {
            if (val.TryGetValue<int>(out var i))
                d = i;
            else if (val.TryGetValue<long>(out var l))
                d = l;
            else if (val.TryGetValue<double>(out var dv))
                d = dv;
            else
                return (false, $"Path '{check.Path}' is not numeric: '{val.ToJsonString()}'");
        }
        else if (double.TryParse(node.ToJsonString(), out d))
        {
            // fallback: parse JSON string
        }
        else
        {
            return (false, $"Path '{check.Path}' is not numeric: '{node.ToJsonString()}'");
        }

        var min = check.Min ?? double.MinValue;
        var max = check.Max ?? double.MaxValue;
        var ok = d >= min && d <= max;
        return (ok, $"Value {d} in range [{min}, {max}]");
    }

    private static (bool, string) EvaluateJsonContains(EvalCheck check, JsonNode? root)
    {
        if (root is null) return (false, "JSON not parseable");
        if (check.Path is null) return (false, "Missing path");
        var node = ResolveJsonPath(root, check.Path);
        if (node is null) return (false, $"Path '{check.Path}' not found");
        var text = node.ToString();
        var sub = check.Value?.ToString() ?? "";
        return (text.Contains(sub, StringComparison.OrdinalIgnoreCase),
            $"'{text}' contains '{sub}': {text.Contains(sub, StringComparison.OrdinalIgnoreCase)}");
    }

    private static (bool, string) EvaluateOutputContains(EvalCheck check, string markdown, string json)
    {
        var content = check.Format == "json" ? json : markdown;
        var sub = check.Value?.ToString() ?? "";
        var found = content.Contains(sub, StringComparison.Ordinal);
        if (found)
            return (true, $"Found '{sub}' at index {content.IndexOf(sub, StringComparison.Ordinal)}");
        return (false, $"'{sub}' not found in output");
    }

    private static (bool, string) EvaluateOutputNotContains(EvalCheck check, string markdown, string json)
    {
        var content = check.Format == "json" ? json : markdown;
        var sub = check.Value?.ToString() ?? "";
        var idx = content.IndexOf(sub, StringComparison.Ordinal);
        if (idx < 0)
            return (true, $"'{sub}' correctly absent");
        return (false, $"'{sub}' found at index {idx}");
    }

    private static (bool, string) EvaluateSignalPresent(EvalCheck check, JsonNode? root)
    {
        if (root is null) return (false, "JSON not parseable");
        var signals = root["signals"]?.AsArray();
        if (signals is null) return (false, "No signals array");
        var key = check.Value?.ToString() ?? "";
        foreach (var signal in signals)
        {
            if (signal?["key"]?.ToString() == key)
            {
                var detected = signal["detected"]?.GetValue<bool>() ?? false;
                return (detected, $"Signal '{key}' detected={detected}");
            }
        }
        return (false, $"Signal '{key}' not found in signals array");
    }

    private static (bool, string) EvaluateDetectionCount(EvalCheck check, JsonNode? root)
    {
        if (root is null) return (false, "JSON not parseable");
        var detections = root["detections"]?.AsArray();
        if (detections is null) return (false, "No detections array");
        var type = check.DetectionType ?? "";
        var count = detections.Count(d => d?["type"]?.ToString() == type);
        var min = check.Min ?? 0;
        var max = check.Max ?? int.MaxValue;
        var ok = count >= min && count <= max;
        return (ok, $"Found {count} '{type}' detections (range [{min}, {max}])");
    }

    private static (bool, string) EvaluateEntryKindPresent(EvalCheck check, JsonNode? root, string markdown)
    {
        if (root is null) return (false, "JSON not parseable");

        // Value = detection type (e.g. "GrpcServiceDetection")
        var detectionType = check.Value?.ToString() ?? "";
        // Format = expected label in markdown (e.g. "gRPC")
        var label = check.Format ?? detectionType;

        var detections = root["detections"]?.AsArray();
        if (detections is null || detections.Count == 0)
            return (false, $"No detections array in JSON");

        var count = detections.Count(d => d?["type"]?.ToString() == detectionType);
        if (count == 0)
            return (false, $"Detection type '{detectionType}' not found in $.detections");

        var header = label + " (";
        var foundLabel = markdown.Contains(header, StringComparison.Ordinal);
        if (!foundLabel)
            return (false, $"Rendered group header '{header}' not found in markdown output");

        return (true, $"Entry kind '{detectionType}' ({label}) present with {count} detections and rendered header '{header}'");
    }

    private static (bool, string) EvaluateMaxElapsed(EvalCheck check, RenderedContext jsonResult)
    {
        var elapsed = jsonResult.ElapsedTotal.TotalMilliseconds;
        var max = 30000.0;
        if (check.Value is JsonElement je && je.TryGetDouble(out var jeVal))
            max = jeVal;
        else if (check.Value is double d)
            max = d;
        else if (check.Value is int i)
            max = i;
        else if (check.Value is not null && double.TryParse(check.Value.ToString(), out var sVal))
            max = sVal;

        var ok = elapsed <= max;
        return (ok, $"Elapsed {elapsed:F0}ms <= {max}ms");
    }

    // ── JSON path resolution ──────────────────────────────────────────

    private static JsonNode? ResolveJsonPath(JsonNode root, string path)
    {
        // Simple dot-notation path: $.architecture.style, $.typesSummary.found
        var segments = path.TrimStart('$').TrimStart('.').Split('.');
        JsonNode? current = root;
        foreach (var seg in segments)
        {
            if (current is null) return null;

            // Special array pseudo-properties
            if (current is JsonArray arrNode)
            {
                if (seg is "length" or "count")
                    return JsonValue.Create(arrNode.Count);
                if (int.TryParse(seg, out var arrIdx) && arrIdx >= 0 && arrIdx < arrNode.Count)
                {
                    current = arrNode[arrIdx];
                    continue;
                }
                return null;
            }

            if (seg.EndsWith(']'))
            {
                // Array access like [0]
                var bracket = seg.IndexOf('[');
                var prop = seg[..bracket];
                var idxStr = seg[(bracket + 1)..^1];
                current = current[prop];
                if (current is JsonArray arr && int.TryParse(idxStr, out var idx))
                    current = idx < arr.Count ? arr[idx] : null;
            }
            else
            {
                // Try as property
                var child = current[seg];
                if (child is not null)
                {
                    current = child;
                }
                else
                {
                    // Try case-insensitive (camelCase JSON keys)
                    if (current is JsonObject obj)
                    {
                        var match = obj.FirstOrDefault(kvp =>
                            string.Equals(kvp.Key, seg, StringComparison.OrdinalIgnoreCase));
                        current = match.Value;
                    }
                    else
                    {
                        current = null;
                    }
                }
            }
        }
        return current;
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static async Task<RenderedContext> RunPipelineAsync(string repoPath, OutputFormat format)
    {
        var fs = new RealFileSystem();
        var cache = new AnalysisCache(fs);

        var rootResult = await ProjectRootResolver.ResolveAsync(repoPath, fs, CancellationToken.None);
        if (rootResult.RootPath is null)
            throw new InvalidOperationException($"Could not resolve project root at {repoPath}");

        var options = new ExtractionOptions
        {
            MaxOutputTokens = 8000,
            OutputFormat = format,
            AllowRoslyn = true,
        };

        var scenario = ScenarioRegistry.BuiltIn["overview"];
        var loggerFactory = LoggerFactory.Create(b => b.AddProvider(new NullLoggerProvider()));

        var analysis = new SharedAnalysisContext();
        var observer = new NullDiscoveryObserver();

        var ctx = new DiscoveryContext
        {
            RootPath = rootResult.EffectiveRootPath,
            ScopedProjectDirs = rootResult.ScopeProjectDirs,
            Options = options,
            ActiveScenario = scenario,
            Observer = observer,
            FileSystem = fs,
            Cache = cache,
            Analysis = analysis,
            Logger = loggerFactory.CreateLogger("Eval")
        };

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
            new DesktopEntryExtractor(),
            new BlazorEntryExtractor(),
            new GrpcServiceExtractor(),
            new SignalRHubExtractor(),
            new AzureFunctionsExtractor(),
            new RazorPagesExtractor(),
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

        var renderers = new Dictionary<string, IContextRenderer>
        {
            ["markdown"] = new MarkdownRenderer(),
            ["json"] = new JsonContextRenderer(),
        };

        var pipeline = new DiscoveryPipeline(
            extractors, pruners, compressors, renderers,
            loggerFactory.CreateLogger<DiscoveryPipeline>());

        return await pipeline.RunAsync(ctx);
    }

    // ── helpers ───────────────────────────────────────────────────────

    private static string? _repoRootCache;

    /// <summary>Walks up from the test assembly directory to find the solution root.</summary>
    private static string FindRepoRoot()
    {
        if (_repoRootCache is not null)
            return _repoRootCache;

        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "DevContext.slnx")))
            {
                _repoRootCache = dir;
                return dir;
            }
            var parent = Path.GetDirectoryName(dir);
            if (parent == dir) break; // reached root
            dir = parent;
        }

        // Fallback
        _repoRootCache = Environment.CurrentDirectory;
        return _repoRootCache;
    }

    private static string RepoRoot(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return "MISSING_REPO";

        var root = FindRepoRoot();
        var repoRoot = Path.GetFullPath(Path.Combine(root, relativePath));

        if (Directory.Exists(repoRoot))
            return repoRoot;

        return repoRoot;
    }

    private static string ResolveExpectationsDir()
    {
        var candidates = new[]
        {
            Path.Combine(Environment.CurrentDirectory, "eval", "expectations"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "eval", "expectations")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "..", "eval", "expectations")),
        };

        foreach (var path in candidates)
        {
            if (Directory.Exists(path))
                return path;
        }
        return candidates[0];
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    // ── types ─────────────────────────────────────────────────────────

    private sealed record EvalExpectation(string Repo, List<EvalCheck> Checks);

    private sealed record EvalCheck(
        string Id,
        string Type,
        string? Path = null,
        string? Format = null,
        object? Value = null,
        int? Min = null,
        int? Max = null,
        string? DetectionType = null,
        string Status = "expected",
        string? Note = null
    );
}

internal sealed class NullLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new NullLogger<object>();
    public void Dispose() { }
}
