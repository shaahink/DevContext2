# DevContext v2 — Authoritative Design Document

**Status**: Design reference for Phase 0 implementation  
**Target**: .NET 9 · C# 13 · `dotnet tool install -g DevContext.Cli`  
**Positioning**: Static analysis CLI — pure code discovery, no LLM calls, no runtime agents  
**Revision notes**: Incorporates signal registry fix, shared analysis cache, async I/O, typed detections, ShouldRun timing fix, Provenance thread-safety fix, ProjectRootResolver, ExtractorCapabilities, typed detection hierarchy

---

## 1. Solution Structure

```
DevContext.sln
├── src/
│   ├── DevContext.Core/          # Contracts, pipeline, generic extractors. Zero Roslyn/Spectre deps.
│   ├── DevContext.Roslyn/        # Roslyn workspace, semantic extractors. Loaded at runtime via abstraction.
│   └── DevContext.Cli/           # Composition root. Spectre.Console, command wiring, DI, rendering.
├── tests/
│   ├── DevContext.Core.Tests/    # Unit + golden tests with fakes.
│   ├── DevContext.Roslyn.Tests/  # In-memory Roslyn compilations (Microsoft.CodeAnalysis.Testing).
│   └── DevContext.Integration/   # End-to-end against vendored real repos.
├── benchmarks/
│   └── DevContext.Benchmarks/    # BenchmarkDotNet perf regression.
├── .github/
│   └── workflows/
│       ├── ci.yml                # build · test · format · analyzers
│       └── release.yml           # on tag: pack · NuGet push · GitHub Release
├── docs/
│   └── schemas/
│       └── devcontext-config.json
├── tests/fixtures/               # Synthetic multi-project solutions (checked in)
├── tests/goldens/                # Expected .md and .json outputs (checked in)
├── Directory.Build.props
└── global.json                   # .NET 9 SDK pin
```

**`Directory.Build.props` baseline**:
```xml
<Project>
  <PropertyGroup>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <LangVersion>13</LangVersion>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>
</Project>
```

**Key rules**:
- `DevContext.Core` references only: `Microsoft.Extensions.Logging.Abstractions`, `System.Collections.Immutable`, `Microsoft.CodeAnalysis.CSharp` (syntax-only, no workspace)
- `DevContext.Roslyn` references Core + `Microsoft.CodeAnalysis.Workspaces.MSBuild`
- `DevContext.Cli` is the only composition root — wires DI, loads all assemblies
- All async code: `ConfigureAwait(false)`, `CancellationToken` threaded through every call

---

## 2. Core Contracts

All defined in `DevContext.Core`. Nothing else ships until these compile and are reviewed.

---

### 2.1 FeatureSignal & ArchitectureSignals — the open registry

Replaces all static bool fields (`HasMinimalApis`, `HasMediatR`, etc.). Closed for modification, open for extension.

```csharp
/// <summary>
/// A detected capability signal with provenance. Immutable once created.
/// </summary>
public sealed record FeatureSignal(
    string Key,                              // "mediatr" | "efcore" | "minimal-apis" | ...
    bool Detected,
    float Confidence,                        // 0.0–1.0
    string DetectedVia,                      // "PackageReference" | "SyntaxPattern" | "FileNaming"
    ImmutableArray<string> Evidence          // concrete strings that triggered detection
)
{
    public static FeatureSignal Detected(string key, float confidence = 1.0f,
        string via = "PackageReference", params string[] evidence)
        => new(key, true, confidence, via, [..evidence]);
}

/// <summary>
/// Open signal registry. Any extractor can register signals.
/// Higher-confidence signals win on collision.
/// Sealed after Stage 2 — read-only during Stage 3 and beyond.
/// </summary>
public sealed class ArchitectureSignals
{
    private readonly ConcurrentDictionary<string, FeatureSignal> _signals = new();
    private volatile bool _sealed;

    public void Register(FeatureSignal signal)
    {
        if (_sealed) throw new InvalidOperationException(
            "Signals sealed after Stage 2. Register signals only in Generic extractors.");
        _signals.AddOrUpdate(signal.Key, signal,
            (_, existing) => signal.Confidence >= existing.Confidence ? signal : existing);
    }

    public bool Has(string key) => _signals.TryGetValue(key, out var s) && s.Detected;
    public FeatureSignal? Get(string key) => _signals.GetValueOrDefault(key);
    public IReadOnlyDictionary<string, FeatureSignal> All => _signals;
    internal void Seal() => _sealed = true;

    // Well-known keys — always use constants, never inline strings
    public static class Keys
    {
        public const string MinimalApis      = "minimal-apis";
        public const string Controllers      = "controllers";
        public const string MediatR          = "mediatr";
        public const string EfCore           = "efcore";
        public const string MassTransit      = "masstransit";
        public const string Aspire           = "aspire";
        public const string FastEndpoints    = "fast-endpoints";
        public const string Dapper           = "dapper";
        public const string Blazor           = "blazor";
        public const string WpfMvvm          = "wpf-mvvm";
        public const string SignalR          = "signalr";
        public const string Grpc             = "grpc";
        public const string Scrutor          = "scrutor";
        public const string Refit            = "refit";
        public const string FluentValidation = "fluentvalidation";
        public const string Hangfire         = "hangfire";
        // New keys added by new extractors — no core changes needed
    }
}
```

---

### 2.2 IFileSystem — async-first I/O abstraction

All file I/O goes through this. Extractors never call `System.IO` directly.

```csharp
public interface IFileSystem
{
    // Async — preferred in extractors
    ValueTask<string> ReadAllTextAsync(string path, CancellationToken ct = default);
    IAsyncEnumerable<string> EnumerateFilesAsync(
        string root, string pattern, SearchOption option, CancellationToken ct = default);

    // Sync — existence checks and path operations only
    bool FileExists(string path);
    bool DirectoryExists(string path);
    string GetRelativePath(string relativeTo, string path);
    string GetFullPath(string path);
    string? GetDirectoryName(string path);
    IEnumerable<string> EnumerateDirectories(string root, string pattern, SearchOption option);
}
```

`FakeFileSystem` (in `DevContext.Core.Tests`): full in-memory implementation. `RealFileSystem` wraps `System.IO` and `File.ReadAllTextAsync`.

---

### 2.3 IAnalysisCache — parse once, share everywhere

Backed by `ConcurrentDictionary<string, Lazy<Task<T>>>` — concurrent first-access safe.
`FileTreeExtractor` registers all source paths in Stage 1. Parsing happens lazily on first request.

```csharp
public interface IAnalysisCache
{
    /// <summary>File text — cached after first read.</summary>
    ValueTask<string> GetTextAsync(string filePath, CancellationToken ct = default);

    /// <summary>Roslyn SyntaxTree — parsed once per file per run. SyntaxTree is immutable.</summary>
    ValueTask<SyntaxTree> GetSyntaxTreeAsync(string filePath, CancellationToken ct = default);

    /// <summary>Parsed XDocument for .csproj, .props, .targets files.</summary>
    ValueTask<XDocument> GetXmlAsync(string filePath, CancellationToken ct = default);

    /// <summary>All known source paths — populated by FileTreeExtractor in Stage 1.</summary>
    IReadOnlyList<string> KnownFilePaths { get; }

    /// <summary>Register a path without loading. Called by FileTreeExtractor.</summary>
    void RegisterPath(string filePath);
}
```

---

### 2.4 SharedAnalysisContext — single-owner derived data

Lives inside `DiscoveryContext`. Written by specific extractors in sequence. All others read.
The call graph is never built twice.

```csharp
/// <summary>
/// Progressively populated by extractors in stage order.
/// Readers must only access fields populated by earlier stages.
/// </summary>
public sealed class SharedAnalysisContext
{
    // Stage 1 — FileTreeExtractor
    public IReadOnlyList<string> AllSourceFiles { get; set; } = [];
    public IReadOnlyList<string> AllProjectFiles { get; set; } = [];

    // Stage 1 — FocusPointParser (from --around args)
    public IReadOnlyList<FocusPoint> FocusPoints { get; set; } = [];

    // Stage 2 — ProjectStructureExtractor
    public ProjectDependencyGraph? ProjectGraph { get; set; }

    // Stage 2 — LayerClassifier
    public IReadOnlyDictionary<string, ArchitectureLayer> ProjectLayerMap { get; set; }
        = FrozenDictionary<string, ArchitectureLayer>.Empty;

    // Stage 3 — CallGraphExtractor (Deep only; null if not run)
    public CallGraph? CallGraph { get; set; }
}

public sealed record FocusPoint(
    FocusKind Kind,
    string FilePath,
    string? TypeName,     // null if Kind is File or Folder
    string? MethodName    // non-null only if Kind is Method
);

public enum FocusKind { File, Folder, Type, Method }
```

---

### 2.5 DiscoveryContext — immutable pipeline input

```csharp
/// <summary>
/// Built once before the pipeline runs. All extractors share the same instance.
/// Never mutate this — all mutable state lives in DiscoveryModel.
/// </summary>
public sealed class DiscoveryContext
{
    public required string RootPath { get; init; }
    public required ExtractionOptions Options { get; init; }
    public required Scenario ActiveScenario { get; init; }
    public required IDiscoveryObserver Observer { get; init; }
    public required IFileSystem FileSystem { get; init; }
    public required IAnalysisCache Cache { get; init; }
    public required SharedAnalysisContext Analysis { get; init; }
    public required ILogger Logger { get; init; }
    public required IRoslynWorkspaceProvider RoslynWorkspace { get; init; }
    public CancellationToken CancellationToken { get; init; }
}

public interface IRoslynWorkspaceProvider
{
    /// <summary>
    /// Load happens once. Subsequent calls return cached result.
    /// Returns null on timeout or failure (diagnostic already emitted).
    /// Cancellation properly propagated to the load operation.
    /// </summary>
    Task<IRoslynWorkspace?> GetWorkspaceAsync(CancellationToken ct);
}
```

---

### 2.6 DiscoveryModel — shared mutable enrichment target

```csharp
public sealed class DiscoveryModel
{
    // Written by SolutionDiscoveryExtractor (once)
    public SolutionInfo? Solution { get; internal set; }

    // Written by ProjectStructureExtractor (once)
    public ImmutableArray<ProjectInfo> Projects { get; internal set; } = [];

    // Written by DependencyExtractor and ArchitectureStyleDetector (Stage 2, parallel-safe)
    public ArchitectureSignals Architecture { get; } = new();

    // Written after signals sealed (orchestrator, between Stage 2 and 3)
    public ArchitectureStyle DetectedStyle { get; internal set; } = ArchitectureStyle.Unknown;
    public float StyleConfidence { get; internal set; }
    public string? StyleDetectedVia { get; internal set; }

    // Written by SyntaxStructureExtractor (TryAdd only — one owner)
    // Tags and IsPruned mutated post-creation by specific extractors / pruners
    public ConcurrentDictionary<string, TypeDiscovery> Types { get; } = new();

    // Written by specific extractors — typed, queried via OfType<T>()
    public ConcurrentBag<Detection> Detections { get; } = [];

    // Written by CallGraphExtractor (Deep only)
    public ConcurrentBag<CallEdge> CallEdges { get; } = [];

    // Written sequentially by pruners only
    public HashSet<string> PrunedTypeIds { get; } = [];
    public List<string> PruningNotes { get; } = [];

    // ConcurrentBag value — thread-safe concurrent append (fixes prior List<T> race)
    public ConcurrentDictionary<string, ConcurrentBag<InclusionReason>> Provenance { get; } = new();

    public void AddProvenance(string itemId, InclusionReason reason)
        => Provenance.GetOrAdd(itemId, _ => []).Add(reason);

    public ConcurrentBag<DiagnosticEntry> Diagnostics { get; } = [];

    public void AddDiagnostic(DiagnosticLevel level, string source, string message)
        => Diagnostics.Add(new(level, source, message, DateTimeOffset.UtcNow));

    public TokenBudget Budget { get; internal set; } = TokenBudget.Default;
}

// Mutation rules (enforced by convention + ADR-002):
// Solution, Projects     — written once by owning extractor; never reassigned
// Architecture signals   — written in Stage 2 parallel; sealed before Stage 3
// Types entries          — TryAdd only; post-creation only Tags + IsPruned mutated
// Detections, CallEdges  — ConcurrentBag.Add from any thread
// PrunedTypeIds, Notes   — written sequentially by pruners only
```

---

### 2.7 IDiscoveryExtractor — unit of analysis

```csharp
public enum ExtractorTier   { Fast, Deep }
public enum ExtractorCategory { Generic, Specific }

/// <summary>
/// Declared capabilities — used by dry-run planner and startup cycle detection.
/// </summary>
public sealed record ExtractorCapabilities(
    ImmutableArray<string> ReadsSignals,    // keys read in ShouldRun or ExtractAsync
    ImmutableArray<string> WritesSignals,   // keys this extractor registers
    ImmutableArray<string> PopulatesModel,  // descriptive: "Types" | "Detections" | "CallEdges"
    string Description                      // shown in --dry-run output
);

public interface IDiscoveryExtractor
{
    string Name { get; }
    ExtractorTier Tier { get; }
    ExtractorCategory Category { get; }
    ExtractorCapabilities Capabilities { get; }

    /// <summary>
    /// For Generic extractors: evaluated before Stage 2. Must NOT read Architecture signals.
    /// For Specific extractors: evaluated AFTER Stage 2 signals are sealed.
    /// Pure predicate — no I/O, no model mutation.
    /// </summary>
    bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel);

    /// <summary>
    /// Main work. Never throw for expected failures — call model.AddDiagnostic instead.
    /// Must be cancellation-aware via ct (not context.CancellationToken directly).
    /// </summary>
    ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct);
}
```

**Registration**: `[DiscoveryAssembly]` on assembly → DI scanning auto-registers all `IDiscoveryExtractor` implementations. `[ExtractorOrder(n)]` controls ordering within tier (default 100). CLI calls `services.AddDiscoveryAssembly(assembly)` for each.

---

### 2.8 Detection hierarchy — typed and extensible

```csharp
/// <summary>
/// Base for all specific detections. New types added by new extractors.
/// Query: model.Detections.OfType&lt;EndpointDetection&gt;()
/// </summary>
public abstract record Detection
{
    public required string ExtractorName { get; init; }
    public required string SourceFile { get; init; }
    public required int LineNumber { get; init; }
    public float Confidence { get; init; } = 1.0f;
}

public sealed record EndpointDetection(
    string HttpMethod,
    string RouteTemplate,
    string HandlerType,
    string HandlerMethod,
    ImmutableArray<string> AuthAttributes,
    ImmutableArray<string> ParameterTypes
) : Detection;

public sealed record MediatRHandlerDetection(
    string RequestType,
    string ResponseType,
    string HandlerType,
    MediatRKind Kind    // Command | Query | Notification
) : Detection;

public sealed record EfEntityDetection(
    string EntityType,
    string DbContextType,
    bool IsAggregate,
    ImmutableArray<string> KeyProperties
) : Detection;

public sealed record BackgroundWorkerDetection(
    string ServiceType,
    string ImplementationType,
    BackgroundWorkerKind Kind   // HostedService | BackgroundService | TimedJob
) : Detection;

public sealed record MiddlewareDetection(
    string MiddlewareType,
    int PipelineOrder,
    MiddlewareKind Kind     // UseX | MapX | CustomClass
) : Detection;

public sealed record IndirectWiringDetection(
    IndirectWiringKind Kind,    // ReflectionActivation | DynamicProxy | ManualServiceLocator
    string CallerType,
    string CallerMethod,
    string? TargetType          // null if not statically resolvable
) : Detection;

public sealed record MessageConsumerDetection(
    string MessageType,
    string ConsumerType,
    string BusKind              // "MassTransit" | "NServiceBus" | "Rebus"
) : Detection;

// Future — no base changes:
// BlazorComponentDetection, WpfViewModelDetection, GrpcServiceDetection, SignalRHubDetection, ...
```

---

### 2.9 IPruner

```csharp
public interface IPruner
{
    string Name { get; }
    int Order { get; }  // 10=PathProximity 20=CallReachability 30=PatternRelevance 40=Budget
    ValueTask PruneAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct);
}
```

---

### 2.10 ICompressionStrategy

```csharp
public interface ICompressionStrategy
{
    string Name { get; }
    int Order { get; }
    ValueTask<CompressionResult> CompressAsync(
        DiscoveryModel model, CompressionOptions options, CancellationToken ct);
}

public sealed record CompressionResult(
    string StrategyName, int TokensBefore, int TokensAfter, IReadOnlyList<string> Notes);
```

---

### 2.11 IContextRenderer

```csharp
public interface IContextRenderer
{
    string Format { get; }   // "markdown" | "json"
    ValueTask<RenderedContext> RenderAsync(
        DiscoveryModel model, RenderOptions options, CancellationToken ct);
}

public sealed record RenderedContext(
    string Content,
    int EstimatedTokens,
    IReadOnlyList<CompressionResult> AppliedCompressions,
    TimeSpan ElapsedTotal,
    string SchemaVersion      // "2.0" — independent of internal model version
);
```

JSON output uses a stable `DevContextOutput` schema — never direct `DiscoveryModel` serialisation.

---

### 2.12 IDiscoveryObserver

```csharp
public interface IDiscoveryObserver
{
    void OnPipelineStarted(DiscoveryContext context);
    void OnStageStarted(PipelineStage stage);
    void OnExtractorStarted(string name, ExtractorTier tier);
    void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason);
    void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals);
    void OnPrunerCompleted(string name, int itemsBefore, int itemsAfter);
    void OnCompressionApplied(CompressionResult result);
    void OnStageCompleted(PipelineStage stage, TimeSpan elapsed);
    void OnRenderCompleted(RenderedContext result);
    void OnPipelineCompleted(DiscoveryModel model);
    void OnDiagnostic(DiagnosticEntry entry);
}

// Core-provided
public sealed class NullDiscoveryObserver : IDiscoveryObserver { /* all no-ops */ }
public sealed class RecordingDiscoveryObserver : IDiscoveryObserver { /* captures all events for assertion */ }

// Cli-provided
public sealed class SpectreDiscoveryObserver : IDiscoveryObserver { /* progress bars, summary table */ }
```

---

### 2.13 Scenario

```csharp
public sealed record Scenario
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public string? Description { get; init; }

    public ImmutableArray<string> EnableExtractors { get; init; } = [];
    public ImmutableArray<string> DisableExtractors { get; init; } = [];
    public required PruningConfig Pruning { get; init; }
    public required CompressionConfig Compression { get; init; }
    public ImmutableArray<string> RequiredSections { get; init; } = [];
}
```

Built-in scenarios in `ScenarioRegistry` (DI-injected dictionary):
`architecture` · `debug-endpoint` · `add-similar-feature` · `modify-middleware` · `trace-message-flow` · `harden-di`

---

### 2.14 ExtractionOptions

```csharp
public sealed record ExtractionOptions
{
    public ImmutableArray<string> EntryPaths { get; init; } = [];
    public EntryResolutionMode EntryMode { get; init; } = EntryResolutionMode.Auto;
    public ExtractionProfile Profile { get; init; } = ExtractionProfile.Focused;
    public int MaxOutputTokens { get; init; } = 8_000;
    public bool AllowRoslyn { get; init; } = true;
    public TimeSpan RoslynTimeout { get; init; } = TimeSpan.FromSeconds(30);
    public ImmutableArray<string> IncludeExtractors { get; init; } = [];
    public ImmutableArray<string> ExcludeExtractors { get; init; } = [];
    public ImmutableArray<string> ExcludePatterns { get; init; } =
        [".git", "bin", "obj", ".vs", "node_modules", ".idea"];
    public OutputFormat OutputFormat { get; init; } = OutputFormat.Markdown;
    public bool IncludeProvenance { get; init; } = false;
    public bool IncludeDiagnostics { get; init; } = false;
    public bool DryRun { get; init; } = false;
    public int MaxProjects { get; init; } = 150;   // auto-disables Deep above this
}

public enum ExtractionProfile { Quick, Focused, Debug, Full }
public enum OutputFormat { Markdown, Json }
public enum EntryResolutionMode { Auto, FileOnly, FolderOnly, TypeMethod }
```

---

## 3. Pipeline Orchestrator

`DiscoveryPipeline` — concrete class. The only component that knows stage order. Not an interface.

### Stage sequence

```
Stage 0  ProjectRootResolver               resolve root path (walk-up / walk-down / folder mode)
Stage 1  Discovery & Cache Warmup          FileTreeExtractor, SolutionDiscovery, FocusPoint resolution
Stage 2  Generic Fast Extractors           Parallel.ForEachAsync — signal registry open
         ── model.Architecture.Seal() ──   signals frozen, ArchitectureStyle determined
Stage 3  Specific Extractors               sequential — read sealed signals
Stage 4  Multi-Stage Pruning              sequential by Order (10 → 20 → 30 → 40)
Stage 5  Compression Strategies            sequential by Order
Stage 6  Render                           IContextRenderer selected by format
```

### Orchestrator shape

```csharp
public sealed class DiscoveryPipeline
{
    private readonly IReadOnlyList<IDiscoveryExtractor> _extractors;
    private readonly IReadOnlyList<IPruner> _pruners;
    private readonly IReadOnlyList<ICompressionStrategy> _compressionStrategies;
    private readonly IReadOnlyDictionary<string, IContextRenderer> _renderers;
    private readonly ILogger<DiscoveryPipeline> _logger;

    public async Task<RenderedContext> RunAsync(
        DiscoveryContext context, CancellationToken ct = default)
    {
        if (context.Options.DryRun)
            return await RunDryRunAsync(context, ct);

        var model = new DiscoveryModel();
        context.Observer.OnPipelineStarted(context);

        await RunStage1Async(context, model, ct);
        await RunStage2Async(context, model, ct);         // parallel Generic extractors

        // Seal — no more signal writes
        model.Architecture.Seal();
        model.DetectedStyle = DetermineStyle(model);
        context.Observer.OnSignalsSealed(model.Architecture.All);

        await RunStage3Async(context, model, ct);         // sequential Specific extractors
        await RunPruningAsync(context, model, ct);
        await RunCompressionAsync(context, model, ct);

        var renderer = _renderers[context.Options.OutputFormat.ToString().ToLowerInvariant()];
        var rendered = await renderer.RenderAsync(model, BuildRenderOptions(context), ct);

        context.Observer.OnRenderCompleted(rendered);
        context.Observer.OnPipelineCompleted(model);
        return rendered;
    }

    private async Task RunStage2Async(DiscoveryContext ctx, DiscoveryModel model, CancellationToken ct)
    {
        // Generic extractors — ShouldRun must NOT read Architecture.Has(...)
        var eligible = _extractors
            .Where(e => e.Tier == ExtractorTier.Fast && e.Category == ExtractorCategory.Generic)
            .Where(e => !ctx.Options.ExcludeExtractors.Contains(e.Name))
            .Where(e => e.ShouldRun(ctx, model))
            .OrderBy(GetOrder)
            .ToList();

        ctx.Observer.OnStageStarted(PipelineStage.GenericExtraction);
        var sw = Stopwatch.StartNew();

        await Parallel.ForEachAsync(eligible, ct, async (extractor, innerCt) =>
        {
            ctx.Observer.OnExtractorStarted(extractor.Name, extractor.Tier);
            var esw = Stopwatch.StartNew();
            try   { await extractor.ExtractAsync(ctx, model, innerCt).ConfigureAwait(false); }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
                  { model.AddDiagnostic(DiagnosticLevel.Warning, extractor.Name, ex.Message); }
            ctx.Observer.OnExtractorCompleted(extractor.Name, esw.Elapsed, false, null);
        });

        ctx.Observer.OnStageCompleted(PipelineStage.GenericExtraction, sw.Elapsed);
    }

    private async Task RunStage3Async(DiscoveryContext ctx, DiscoveryModel model, CancellationToken ct)
    {
        // Specific extractors — signals sealed, ShouldRun can read Architecture.Has(...)
        var eligible = _extractors
            .Where(e => e.Category == ExtractorCategory.Specific)
            .Where(e => !ctx.Options.ExcludeExtractors.Contains(e.Name))
            .Where(e => e.ShouldRun(ctx, model))
            .OrderBy(GetOrder)
            .ToList();

        ctx.Observer.OnStageStarted(PipelineStage.SpecificExtraction);
        var sw = Stopwatch.StartNew();

        foreach (var extractor in eligible)
        {
            ct.ThrowIfCancellationRequested();
            ctx.Observer.OnExtractorStarted(extractor.Name, extractor.Tier);
            var esw = Stopwatch.StartNew();
            try   { await extractor.ExtractAsync(ctx, model, ct).ConfigureAwait(false); }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
                  { model.AddDiagnostic(DiagnosticLevel.Warning, extractor.Name, ex.Message); }
            ctx.Observer.OnExtractorCompleted(extractor.Name, esw.Elapsed, false, null);
        }

        ctx.Observer.OnStageCompleted(PipelineStage.SpecificExtraction, sw.Elapsed);
    }
}
```

### Dry-run mode

When `Options.DryRun = true`:
1. Run Stage 0 (root resolution) and Stage 1 file tree only
2. Evaluate `ShouldRun` for all extractors against empty model
3. Do not run extraction, pruning, compression, or rendering
4. Emit dry-run plan via observer — resolved root, scenario, extractor list with descriptions and skip reasons, estimated sections and token range

---

## 4. ProjectRootResolver

Runs before `DiscoveryContext` is built. Resolves ambiguous input paths.

```csharp
public sealed class ProjectRootResolver
{
    /// <summary>
    /// Resolution priority:
    /// 1. Input is a .sln/.slnx file → use directly
    /// 2. Input is a .csproj file → parent = root, project = entry candidate
    /// 3. Input is a directory:
    ///    a. .sln found in that directory
    ///    b. Walk UP (max 5 levels) → nearest .sln wins
    ///    c. Walk DOWN (max 3 levels) → shallowest .sln wins
    ///    d. No .sln → look for .csproj files (library/tool mode)
    ///    e. No .csproj → folder mode (parse .cs files directly)
    /// </summary>
    public ProjectRootResult Resolve(string inputPath, IFileSystem fs) { ... }
}

public sealed record ProjectRootResult(
    string RootPath,
    string? SolutionFilePath,
    ImmutableArray<string> EntryCandidates,
    ResolutionMethod Method,
    string? ResolutionNote              // shown in dry-run and observer
);

public enum ResolutionMethod
{
    ExplicitSln, ExplicitCsproj,
    DirectoryContainsSln, WalkedUp, WalkedDown,
    FolderMode
}
```

---

## 5. Generic Extractor Catalogue (Phase 0–1)

All `ExtractorTier.Fast`, `ExtractorCategory.Generic`. Run in parallel during Stage 2.

| Extractor | Populates | Notes |
|---|---|---|
| `FileTreeExtractor` | `SharedAnalysisContext.AllSourceFiles`, cache path registration | First — runs in Stage 1 before others. Respects `ExcludePatterns`. |
| `SolutionDiscoveryExtractor` | `model.Solution` | Parses `.sln` and `.slnx`. Graceful on missing. |
| `ProjectStructureExtractor` | `model.Projects` | XML via `IAnalysisCache.GetXmlAsync`. One read per file. SDK-style and legacy. |
| `DependencyExtractor` | `Architecture` signals, `SharedAnalysisContext.ProjectGraph` | Reads `PackageReference` from cached `.csproj` XML. Registers `FeatureSignal` per known package key. |
| `LayerClassifier` | `SharedAnalysisContext.ProjectLayerMap` | Path heuristics + package signals → `ArchitectureLayer` per project. Ordered after `DependencyExtractor`. |
| `ArchitectureStyleDetector` | `model.DetectedStyle`, `model.StyleConfidence` | Runs after signals sealed (orchestrator calls between Stage 2 and 3). Scores styles with confidence. |
| `SyntaxStructureExtractor` | `model.Types` | Streams `.cs` via `IAsyncEnumerable`. Parses via `IAnalysisCache.GetSyntaxTreeAsync`. `TryAdd` only. |
| `ProgramCsFlowExtractor` | `MiddlewareDetection`, `BackgroundWorkerDetection` | Dedicated to `Program.cs` startup topology. Ordered registration. Orphan detection (AddX with no UseX). |
| `DiRegistrationExtractor` | `Detection` entries | `services.AddX` patterns. Always on — cheap. |

---

## 6. Specific Extractor Catalogue

`ExtractorCategory.Specific`. Evaluated after signals sealed. Run sequentially in Stage 3.

| Extractor | Tier | Gate | Produces |
|---|---|---|---|
| `EndpointExtractor` | Fast | `minimal-apis` signal | `EndpointDetection` — route, method, handler, auth, params |
| `ControllerActionExtractor` | Fast | `controllers` signal | `EndpointDetection` — controller hierarchy, filters, routes |
| `MediatRExtractor` | Fast | `mediatr` signal | `MediatRHandlerDetection` — request→handler links |
| `EfCoreExtractor` | Fast | `efcore` signal | `EfEntityDetection` — DbContext, DbSet, migrations |
| `EventBusExtractor` | Fast | `masstransit` or `nservicebus` signal | `MessageConsumerDetection` — consumers, publishers, sagas |
| `IndirectWiringDetector` | Fast | `debug` or `harden-di` scenario | `IndirectWiringDetection` — reflection, service locator, dynamic dispatch |
| `AspireExtractor` | Fast | `aspire` signal | AppHost resources and relationships |
| `CallGraphExtractor` | Deep | `Debug` or `Full` profile | `model.CallEdges`, `SharedAnalysisContext.CallGraph` |
| `SourceBodyExtractor` | Deep | `Full` profile or explicit | `TypeDiscovery.SourceBody` for surviving types — token-budget-capped |

**Adding a new extractor**: implement `IDiscoveryExtractor`, mark assembly with `[DiscoveryAssembly]`, add unit test. No pipeline or orchestrator changes.

---

## 7. TypeDiscovery

```csharp
public sealed class TypeDiscovery
{
    // Written once by SyntaxStructureExtractor — init-only
    public required string Id { get; init; }         // fully-qualified name
    public required string Name { get; init; }
    public required string Namespace { get; init; }
    public required string FilePath { get; init; }
    public required TypeKind Kind { get; init; }
    public required Accessibility Accessibility { get; init; }
    public required ArchitectureLayer Layer { get; init; }
    public ImmutableArray<MethodSignature> Methods { get; init; } = [];
    public ImmutableArray<PropertySignature> Properties { get; init; } = [];
    public ImmutableArray<string> BaseTypes { get; init; } = [];
    public ImmutableArray<string> ImplementedInterfaces { get; init; } = [];
    public ImmutableArray<string> Attributes { get; init; } = [];

    // Written by SourceBodyExtractor (Deep only)
    public string? SourceBody { get; set; }

    // Written by specific extractors in Stage 3 — concurrent append-only
    public ConcurrentBag<string> Tags { get; } = [];

    // Written by pruners — sequential, no concurrency needed here
    public bool IsPruned { get; set; }
    public float PathProximityScore { get; set; }
    public float RelevanceScore { get; set; }
}
```

---

## 8. Pruning Stages

Sequential. Each adds to `model.PruningNotes`. Respects already-pruned types.

| Order | Pruner | Logic |
|---|---|---|
| 10 | `PathProximityPruner` | Directory distance from `FocusPoints`. Prunes beyond `PruningConfig.MaxPathDistance`. |
| 20 | `CallReachabilityPruner` | BFS from focused methods via `SharedAnalysisContext.CallGraph`. Only runs if CallGraph populated. |
| 30 | `PatternRelevancePruner` | Types with `EndpointDetection`, `MediatRHandlerDetection` etc. get relevance boost — survive if tagged. |
| 40 | `TokenBudgetEnforcer` | Sort surviving types by `(PathScore + RelevanceScore)` desc. Prune from bottom until within token budget. |

---

## 9. Compression Strategies

After pruning. Sequential by Order. Each returns `CompressionResult`.

| Order | Strategy | Action |
|---|---|---|
| 10 | `TrivialMemberCompressor` | Removes auto-properties, empty ctors, `ToString`/`Equals`/`GetHashCode` overrides |
| 20 | `BoilerplateCompressor` | Removes designer-generated signals, obvious DI extension patterns |
| 30 | `StructuralDeduplicator` | Groups near-identical type shapes, emits "N similar types (showing 1)" |
| 40 | `NamespaceGrouper` | Groups surviving types by namespace, adds per-namespace count summary |
| 50 | `LlmFriendlyFormatter` | Normalises whitespace, strips XML docs (keeps `<summary>`), adds `<!-- ~N tokens -->` per section |
| 60 | `AggressiveTruncator` | Hard-truncates long method bodies with `// ... [N lines]`. Per-type char cap (configurable). |

---

## 10. CLI Surface

### Commands

```
devcontext [PATH] [OPTIONS]        main analysis command
devcontext init                    create devcontext.json in cwd
devcontext scenarios               list scenarios with descriptions
devcontext dry-run [PATH]          first-class dry run (alias: devcontext [PATH] --dry-run)
devcontext version                 version + commit hash
```

### Main command options

```
Arguments:
  [PATH]                 Root path. Default: cwd. Accepts .sln, .csproj, folder, or Type:Method notation.

Options:
  -a, --around <PATH>    Entry point. Repeat for multiple.
                         Accepts: folder | file | TypeName | TypeName:MethodName
  -s, --scenario <N>     architecture | debug-endpoint | add-similar-feature |
                          modify-middleware | trace-message-flow | harden-di
  -p, --profile <N>      quick | focused | debug | full
  -t, --task <TEXT>      Free-text intent → inferred scenario + profile
      --max-tokens <N>   Token cap (default 8000)
  -o, --output <FILE>    Write to file (default stdout)
      --copy             Clipboard (pbcopy / xclip / clip.exe, graceful fallback)
      --format <F>       markdown (default) | json
      --include-provenance
      --include-diagnostics
      --no-roslyn        Disable deep tier entirely
      --verbose          Info-level logging
      --trace            Debug-level logging (includes Roslyn events)
      --dry-run          Plan only — no extraction
```

### --around parsing (FocusPointParser)

```
src/Orders/                       → FocusKind.Folder
src/Orders/OrdersController.cs    → FocusKind.File
OrdersController                  → FocusKind.Type  (searched in model.Types)
OrdersController:CreateOrder      → FocusKind.Method
```

Produces `IReadOnlyList<FocusPoint>` stored in `SharedAnalysisContext.FocusPoints`.
Invalid `--around` → validation error in dry-run before any extraction runs.

### IntentInferrer — fixed implementation

```csharp
internal static class IntentInferrer
{
    // Scored iteration — NOT a dictionary with array keys (bug in prior design)
    private static readonly IReadOnlyList<(string[] Keywords, string Scenario)> Rules =
    [
        (["debug", "why", "failing", "error", "exception", "500"], "debug-endpoint"),
        (["add", "implement", "similar", "like", "crud", "new endpoint"],  "add-similar-feature"),
        (["middleware", "pipeline", "cross-cutting", "filter", "interceptor"], "modify-middleware"),
        (["event", "message", "publish", "consume", "queue", "bus"], "trace-message-flow"),
        (["architecture", "overview", "structure", "layers", "map"], "architecture"),
        (["di", "injection", "reflect", "activator", "register"], "harden-di"),
    ];

    public static (string Scenario, ExtractionProfile Profile) Infer(string task)
    {
        var lower = task.ToLowerInvariant();
        var scored = Rules
            .Select(r => (r.Scenario, Score: r.Keywords.Count(k => lower.Contains(k))))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        var scenario = scored.Scenario ?? "architecture";
        var profile = scenario is "debug-endpoint" or "harden-di"
            ? ExtractionProfile.Debug
            : ExtractionProfile.Focused;

        return (scenario, profile);
    }
}
```

### devcontext.json

```json
{
  "$schema": "https://devcontext.dev/schemas/v2/config.json",
  "defaultProfile": "focused",
  "defaultScenario": "debug-endpoint",
  "maxOutputTokens": 6000,
  "excludePatterns": [".git", "bin", "obj", "Migrations"],
  "entryPaths": ["src/Api"],
  "profiles": {
    "quick": { "profile": "quick", "maxOutputTokens": 2000, "noRoslyn": true }
  }
}
```

Schema-validated at startup. Invalid JSON → structured parse error, never a stack trace.

---

## 11. Observability

### Structured logging — Serilog in CLI

```csharp
// CLI composition root
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(resolvedLevel)
    .WriteTo.Console(
        theme: AnsiConsoleTheme.Code,
        outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

services.AddLogging(b => b.AddSerilog(dispose: true));
```

Default: `Warning`. `--verbose`: `Information`. `--trace`: `Debug`.

### OpenTelemetry hooks (stubbed — no external dep to compile)

```csharp
internal static class Telemetry
{
    public static readonly ActivitySource Source = new("DevContext", AssemblyVersion);
    public static readonly Meter Meter = new("DevContext", AssemblyVersion);

    public static readonly Counter<long> ExtractionsRun =
        Meter.CreateCounter<long>("devcontext.extractions.total");
    public static readonly Histogram<double> ExtractionDurationMs =
        Meter.CreateHistogram<double>("devcontext.extraction.duration_ms");
    public static readonly Histogram<long> OutputTokens =
        Meter.CreateHistogram<long>("devcontext.output.tokens");
}
```

### SpectreDiscoveryObserver — expected terminal output

```
● Resolving root              ✓ 8ms    walked up 2 levels → MyApp.sln
● Stage 1: Discovery          ✓ 45ms
  ∟ FileTreeExtractor         ✓ 12ms   412 files registered
  ∟ SolutionDiscovery         ✓ 5ms    MyApp.sln (6 projects)
● Stage 2: Generic            ✓ 320ms  (parallel)
  ∟ ProjectStructure          ✓ 38ms   6 projects
  ∟ DependencyExtractor       ✓ 44ms   signals: mediatr efcore minimal-apis
  ∟ SyntaxStructure           ✓ 218ms  847 types
● Signals sealed              mediatr · efcore · minimal-apis
● Stage 3: Specific           ✓ 180ms
  ∟ EndpointExtractor         ✓ 62ms   12 endpoints
  ∟ MediatRExtractor          ✓ 48ms   8 handlers
  ∟ EfCoreExtractor           ✓ 70ms   4 entities
● Stage 4: Pruning            ✓ 28ms   847 → 34 types (96% pruned)
● Stage 5: Compression        ✓ 12ms   ~8,400 → ~3,200 tokens (−62%)
● Rendering                   ✓ 8ms    markdown

╔══════════════════════════════════════════════════════════╗
║  DevContext v2.1.0 · MyApp.sln · 2.3s total             ║
║  Signals: mediatr · efcore · minimal-apis                ║
║  Types: 847 found → 34 in output (96% pruned)           ║
║  Tokens: ~3,200 (budget 8,000) · Compressed 62%         ║
╚══════════════════════════════════════════════════════════╝
```

Non-interactive / piped output: ANSI suppressed, progress bars omitted, plain text fallback.

---

## 12. Output — Markdown Section Order

```markdown
## DevContext — [Scenario] on [Entry / Root]
**Architecture**: Clean Architecture (0.87 confidence — layer folders + MediatR boundary)
**Signals**: mediatr · efcore · minimal-apis
**Projects**: 6 (Domain · Application · Infrastructure · Web · Tests×2)
**Entry**: src/Api/Orders (12 endpoints · 8 MediatR handlers)
**Profile**: focused | **Tokens**: ~3,200 (budget 8,000) | **Compressed**: −62%

---
## Architecture overview
[ASCII layer diagram or description]

## Entry: src/Api/Orders
### Endpoints
[route table — method · route · handler · auth]

### Call graph (depth 3)
[BFS from focused method — only if Debug or Full profile]
POST /orders → CreateOrderEndpoint.Handle → IMediator.Send → CreateOrderHandler.Handle
  ├─ IOrderRepository.AddAsync → OrdersDbContext
  └─ IEventBus.Publish(OrderStartedIntegrationEvent)

## Domain signals
[EF entities · MediatR handlers in scope · message flows]

## Non-obvious wiring
[IndirectWiringDetection — reflection · background workers · service locator calls]

## Related types
[surviving types grouped by layer]

## Diagnostics
[only if --include-diagnostics]

---
*DevContext v2.1.0 (abc1234) · 2.3s · TrivialMember(−18%) Boilerplate(−12%)*
```

---

## 13. Memory & Performance

| Concern | Approach |
|---|---|
| File reads | `IAnalysisCache` — text + SyntaxTree + XDocument each parsed once |
| Large solutions | `SyntaxStructureExtractor` streams files via `IAsyncEnumerable` |
| Roslyn workspace | `IRoslynWorkspaceProvider` — one load, shared, proper CT propagation |
| Auto-disable Deep | If `Projects.Length > Options.MaxProjects` (default 150) skip Deep + warn |
| String building | `ObjectPool<StringBuilder>` in signature extraction |
| Call graph reads | `FrozenDictionary` adjacency list after construction |
| Token estimation | `chars / 4` approximation until render; exact count only in renderer |
| Memory ceiling | `model.Types` is main allocator — pruning removes refs, allows GC |

---

## 14. Testing Architecture

### Unit tests — `DevContext.Core.Tests`

Every extractor tested in isolation:

```csharp
[Fact]
public async Task EndpointExtractor_DetectsMinimalApiRoute()
{
    var fs = new FakeFileSystem();
    fs.AddFile("Program.cs", """
        app.MapGet("/orders/{id}", async (int id, IOrderService svc) =>
            await svc.GetByIdAsync(id));
        """);

    var ctx = DiscoveryContextBuilder.WithFileSystem(fs)
        .WithSignal(FeatureSignal.Detected(ArchitectureSignals.Keys.MinimalApis))
        .WithSealedSignals()     // signals sealed, extractor ShouldRun will pass
        .Build();

    var model = new DiscoveryModel();
    await new EndpointExtractor().ExtractAsync(ctx, model, CancellationToken.None);

    var detection = Assert.Single(model.Detections.OfType<EndpointDetection>());
    Assert.Equal("GET", detection.HttpMethod);
    Assert.Equal("/orders/{id}", detection.RouteTemplate);
}
```

### Golden tests

- Input: synthetic fixture in `tests/fixtures/`
- Expected: `.md` + `.json` in `tests/goldens/`
- Failure: test fails with a diff
- Update: `UPDATE_GOLDENS=1 dotnet test`
- Normalisation: timestamps, absolute paths, version numbers scrubbed

### Integration tests — `DevContext.Integration`

Vendored repos as git submodules under `tests/benchmarks/repos/`.

| Repo | Assertions |
|---|---|
| `davidfowl/TodoApi` | Minimal API endpoints detected; execution < 3s; < 8000 tokens |
| `ardalis/CleanArchitecture` | Style = CleanArchitecture ≥ 0.80 confidence; MediatR handlers found |
| `dotnet/eShop` | Aspire detected; integration events found; multiple services resolved |
| DevContext itself | Solution found; projects parsed; `--dry-run` exits 0 |

### CLI smoke tests

```csharp
[Fact]
public async Task DryRun_OnTodoApi_Succeeds()
{
    var result = await CliTestHarness.RunAsync(
        ["tests/benchmarks/repos/TodoApi", "--dry-run"],
        timeoutMs: 5_000);

    Assert.Equal(0, result.ExitCode);
    Assert.Contains("minimal-apis", result.Output);
    Assert.Contains("Extractors that would run", result.Output);
}
```

---

## 15. Versioning & Release

- Semantic versioning via `MinVer` (git tags drive version)
- `--version` output: `DevContext v2.1.0 (commit abc1234, built 2025-06-05)`
- JSON output: `schemaVersion` field — breaking schema changes bump major version
- `release.yml`: on tag → `dotnet pack` → NuGet push + GitHub Release binary artifacts
- Backward compatibility: existing scenarios and profiles always honoured

---

## 16. Architecture Decision Records

**ADR-001: Roslyn in a separate project**  
Roslyn assemblies are large and slow to load. `DevContext.Roslyn` is never referenced by `Core`. The CLI wires them via abstraction. Fast-only runs never load Roslyn.

**ADR-002: Parallel Stage 2, sequential Stage 3**  
Generic extractors read independent inputs (filesystem, XML) and write to thread-safe collections — provably non-conflicting. Specific extractors read the sealed signal registry and may cross-reference model data, so sequential avoids races without locking overhead.

**ADR-003: Signals sealed between Stage 2 and 3**  
`ShouldRun` for Specific extractors must see the complete signal picture. Sealing after `Parallel.ForEachAsync` joins guarantees all Generic extractors have committed before any Specific extractor reads. The `Seal()` call also makes bugs (writing after seal) immediately visible.

**ADR-004: ConcurrentBag<InclusionReason> for Provenance values**  
Prior design used `List<T>` returned from `ConcurrentDictionary.GetOrAdd` — concurrent `.Add()` on the same list is a race. `ConcurrentBag` is append-only and concurrency-safe for this pattern.

**ADR-005: Typed Detection hierarchy**  
`model.Detections.OfType<T>()` gives type-safe, zero-cast access. Pruners and renderers need no switch statements. New detection types extend the hierarchy without touching the base or existing types.

**ADR-006: IAnalysisCache for parse-once**  
`ConcurrentDictionary<string, Lazy<Task<T>>>` ensures each file is read and parsed exactly once regardless of how many parallel extractors request it. `FileTreeExtractor` pre-registers all paths so the dictionary exists before concurrent access begins.

**ADR-007: IFileSystem async-first**  
Synchronous `ReadAllText` blocks thread pool threads under `Parallel.ForEachAsync`, reducing throughput on large projects. Async variants allow cooperative yielding.

**ADR-008: Stable DevContextOutput JSON schema**  
Internal `DiscoveryModel` is never serialised directly. `JsonContextRenderer` builds a `DevContextOutput` record with its own versioned schema. Internal model can evolve without breaking downstream tooling or agents that pin to a schema version.

**ADR-009: Compression after pruning**  
Compressing before pruning wastes work on data that will be discarded. Compressing the pruned subset means strategies operate on relevant data only, and token estimates are accurate at the point they matter.

**ADR-010: ProjectRootResolver as pre-pipeline step**  
Root resolution is complex (walk-up/down, .sln vs .csproj vs folder mode). Isolating it before `DiscoveryContext` is built keeps the pipeline clean, makes resolution independently testable, and allows dry-run to show how the root was found.

**ADR-011: ExtractorCapabilities as declared metadata**  
Extractors declare what signals they read and write. The pipeline can validate at startup that no extractor reads a signal it also writes in the same tier (cycle). The dry-run planner renders capability descriptions without instantiating extractors.

---

## 17. Phase 0 — Exit Criteria

Phase 0 ends when this checklist is fully green. No extraction logic required yet.

**Contracts**
- [ ] All contracts in Section 2 compile with zero warnings, zero suppressions
- [ ] XML doc on every public member
- [ ] ADRs 001–011 written as markdown in `docs/`

**Core infrastructure**
- [ ] `IFileSystem` + `FakeFileSystem` with full in-memory implementation
- [ ] `IAnalysisCache` + `FakeAnalysisCache` backed by `FakeFileSystem`
- [ ] `SharedAnalysisContext` + `FocusPointParser` (all 4 FocusKind variants)
- [ ] `ArchitectureSignals` with registry, `Seal()`, `Keys` constants
- [ ] `ProjectRootResolver` with all 5 resolution strategies, fully unit tested
- [ ] `ScenarioRegistry` with 6 built-in scenarios (config records only, no extraction)
- [ ] `IntentInferrer` with scored keyword matching (fixed)
- [ ] `TokenBudget` type defined with `Default` constant

**Pipeline**
- [ ] `DiscoveryPipeline` skeleton: all stage methods, log "would run N extractors", no real extraction
- [ ] Stage sequence enforced by tests (stage order, signal seal point, observer events)
- [ ] Dry-run mode returns without running extractors

**Generic extractors (Phase 0 scope — basic)**
- [ ] `FileTreeExtractor` — walks tree, respects excludes, registers cache paths
- [ ] `SolutionDiscoveryExtractor` — parses `.sln` / `.slnx`, handles missing
- [ ] `ProjectStructureExtractor` — parses `.csproj` XML, SDK-style + legacy

**Observers**
- [ ] `NullDiscoveryObserver` (all no-ops)
- [ ] `RecordingDiscoveryObserver` (captures events for assertion)
- [ ] `SpectreDiscoveryObserver` bare minimum (`AnsiConsole.MarkupLine` per event, no progress bars yet)

**CLI**
- [ ] `devcontext [PATH]` → prints "Analysis complete" + solution info + elapsed time
- [ ] `devcontext --dry-run` → resolves root, shows extractor plan, exits 0
- [ ] `devcontext version` → prints version + commit hash
- [ ] `devcontext.json` loading with schema validation

**Testing**
- [ ] `DiscoveryContextBuilder` test helper for ergonomic test setup
- [ ] Golden test infrastructure (`GoldenTestHelper`, normaliser, `UPDATE_GOLDENS` support)
- [ ] `dotnet build` green (zero warnings)
- [ ] `dotnet test` green (≥ 80 tests)
- [ ] `dotnet format --verify-no-changes` green

**Integration gate**
- [ ] `devcontext davidfowl/TodoApi --dry-run` exits 0, mentions file count, no crash

**Not in Phase 0**: `SyntaxStructureExtractor`, `DependencyExtractor`, `LayerClassifier`, endpoint extraction, MediatR, EF Core, pruning logic, compression strategies, Roslyn integration, rendering beyond plain text.

---

*This document is the canonical implementation reference for DevContext v2.*  
*Contract changes require an ADR. Phases beyond 0 plan in separate documents that reference these contracts.*
