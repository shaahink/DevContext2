# REPORT
**DevContext**

Style: CleanArchitecture
_6 projects  ·  1 HttpEndpoint, 24 GrpcService, 3 CliCommand  ·  net10.0 + controllers + minimal-apis + azure-functions + serilog + graphql + grpc + signalr + cli-commands + aspire_

## Stats

| Metric | Value |
|--------|-------|
| Files | 391 |
| Projects | 25 |
| Nodes | 1788 |
| Edges | 2185 |
| ServiceLinks | 1 |
| Entries | 28 |
| With target | 28/28 |
| Deep spine (>=2) | 28/28 (100%) |
| Verified edges | 71% |
| Analyzed in | 43.3s |

## Top Flows

1. **DevContextService.GetTrace** → `AnalysisSession.RenderTraceMarkdownAsync` *(GrpcService)*
2. **DevContextService.Analyze** → `AnalysisSession` *(GrpcService)*
3. **DevContextService.GetMap** → `AnalysisSession.RenderMapMarkdownAsync` *(GrpcService)*
4. **DevContextService.Render** → `AnalysisSession.RenderAsync` *(GrpcService)*
5. **DevContextService.VerifyContext** → `ContextPackVerifier.Verify` *(GrpcService)*
6. **DevContextService.GetContextPack** → `ContextPackBuilder.BuildMulti` *(GrpcService)*
7. **DevContextService.GetContext** → `ContextPackBuilder.Build` *(GrpcService)*
8. **DevContextService.GetGraphFacets** → `ServiceMapProjection.Project` *(GrpcService)*
9. **DevContextService.GetNeighbors** → `ProtoMapper.ToProto` *(GrpcService)*
10. **DevContextService.GetNode** → `ProtoMapper.ToNodeResponse` *(GrpcService)*

### Trace 1: DevContextService.GetTrace

TRACE  DevContextService.GetTrace
       src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17
       DevContext.Server
▸ ENTRY  DevContextService.GetTrace  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
   └─ call DevContextGrpcService.GetTrace  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
          public override Task<Proto.TraceResponse> GetTrace(Proto.TraceRequest request, ServerCallContext context)
          => WrapAsyncT(request.Handle, async session =>
          var depth = request.HasDepth ? request.Depth : 6;
      ├─ call ProtoMapper.ToTraceResponse  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:152) [verified]
      │      public static Proto.TraceResponse ToTraceResponse(Trace trace, string markdown)
      │      var resp = new Proto.TraceResponse
      │      Found = true,
      │  └─ call ToProto  (src/DevContext.Server/Mapping/ProtoMapper.cs:142) [verified]
      │         public static Proto.EntryPoint ToProto(EntryPoint e, string? layer = null, string? feature = null)
      │         var p = new Proto.EntryPoint
      │         Kind = e.Kind.ToString(),
      │     ├─ call EntryPoint  (src/DevContext.Server/Mapping/ProtoMapper.cs:34) [approx]
      │     │      /// <summary>
      │     │      /// A place execution can enter the system. <see cref="Node"/> is where a <see cref="Trace"/> begins.
      │     │      /// Built by GraphBuilder from EndpointDetection / MessageConsumerDetection / hosted services / (for
      │     ├─ call EdgeRef  (src/DevContext.Server/Mapping/ProtoMapper.cs:172) [approx]
      │     │      /// <summary>A directed edge as a navigation result: the edge plus the resolved title of the node on the
      │     │      /// other end (the one the caller is navigating TO).</summary>
      │     │      public sealed record EdgeRef(
      │     ├─ call TraceStep  (src/DevContext.Server/Mapping/ProtoMapper.cs:577) [approx]
      │     │      /// <summary>One node in an entry-rooted trace tree.</summary>
      │     │      public sealed record TraceStep(
      │     │      GraphNode Node,
      │     ├─ call TraceStep.ToString  (src/DevContext.Server/Mapping/ProtoMapper.cs:582) [approx]
      │     ├─ call NodeId.ToString  (src/DevContext.Server/Mapping/ProtoMapper.cs:577) [verified]
      │     │      public override string ToString() => $"{Kind}:{Key}";
      │     └─ call EntryPoint.ToString  (src/DevContext.Server/Mapping/ProtoMapper.cs:175) [approx]
      ├─ call RenderTraceMarkdownAsync  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:150) [verified]
      │      public async Task<string> RenderTraceMarkdownAsync(string focus, int depth, TraceDetail detail, CancellationToken ct)
      │      var rendered = await Engine.Pipeline
      │      .RenderAsync(Snapshot, BuildRequest(focus, depth, detail), ct)
      │  ├─ call EngineResult  (src/DevContext.Server/Sessions/AnalysisSession.cs:39) [approx]
      │  │      /// <summary>The product of one analysis: the immutable snapshot plus the live pipeline that produced
      │  │      /// it (kept so cheap re-renders use the same path-bound resolvers), plus display metadata. The DI
      │  │      /// container that owns the pipeline lives in <see cref="EngineHostCache"/> — the session must NOT
      │  ├─ call DiscoveryPipeline  (src/DevContext.Server/Sessions/AnalysisSession.cs:40) [verified]
      │  │      /// <summary>Orchestrates the discovery pipeline: extraction, signal sealing, pruning, compression, and rendering.</summary>
      │  │      public sealed class DiscoveryPipeline
      │  │      private readonly IReadOnlyList<IDiscoveryExtractor> _extractors;
      │  ├─ call AnalysisSession.BuildRequest  (src/DevContext.Server/Sessions/AnalysisSession.cs:40) [verified]
      │  │      private RenderRequest BuildRequest(string? entry, int? depth, TraceDetail detail) => new()
      │  │      Format = "markdown",
      │  │      MaxTokens = Snapshot.Options.MaxOutputTokens,
      │  └─ call RenderAsync  (src/DevContext.Server/Sessions/AnalysisSession.cs:39) [verified]
      │         public async Task<RenderedContext> RenderAsync(AnalysisSnapshot snapshot, RenderRequest request, CancellationToken ct = default)
      │         // ── PLAN-10 A3: Map/Trace branch — when the Graph is available with content (always after a
      │         // full analyze). The Map/Trace renderers produce the human-facing markdown narrative; JSON/HTML
      │     (9 more branches omitted beyond fan-out)
      │     ├─ call NarrativeSections  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:519) [verified]
      │     │      /// <summary>Assembles ordered narrative fragments into a <see cref="RenderedContext"/> that carries
      │     │      /// both the joined <c>Content</c> and the per-section fragments the desktop toggles. This is what
      │     │      /// makes Map/Trace section-aware: the same fragments drive the LLM (markdown) view and, after HTML
      │     ├─ call AnalysisSnapshot  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:561) [approx]
      │     │      /// <summary>Immutable result of the analyze phase. The model must not be mutated after this is created.</summary>
      │     │      public sealed record AnalysisSnapshot
      │     │      public required DiscoveryModel Model { get; init; }
      │     ├─ call AnalysisSnapshot.Count  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:583) [approx]
      │     ├─ call RunSelfChecks  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:577) [verified]
      │     │      private static RenderedContext RunSelfChecks(RenderedContext rendered, DiscoveryModel model, RenderOptions renderOptions, DiscoveryModel diagnosticsTarget)
      │     │      var results = OutputSelfCheck.Check(rendered, model, renderOptions);
      │     │      var failures = ImmutableArray.CreateBuilder<string>();
      │     │  ├─ call DiscoveryModel  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:754) [approx]
      │     │  │      /// <summary>Represents the result of running the discovery pipeline, containing all extracted data.</summary>
      │     │  │      public sealed class DiscoveryModel
      │     │  │      /// <summary>Information about the solution file, if found.</summary>
      │     │  ├─ call DiscoveryModel.AddDiagnostic  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:757) [verified]
      │     │  │      public void AddDiagnostic(DiagnosticLevel level, string source, string message)
      │     │  │      => Diagnostics.Add(new(level, source, message, DateTimeOffset.UtcNow));
      │     │  └─ call OutputSelfCheck.Check  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:749) [verified]
      │     │         public static ImmutableArray<SelfCheckResult> Check(
      │     │         RenderedContext rendered,
      │     │         DiscoveryModel model,
      │     │     (stopped at depth 5; 10 branches omitted)
      │     ├─ call AnalysisSnapshot.ToImmutableArray  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:563) [approx]
      │     ├─ call AnalysisSnapshot.ToString  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:561) [approx]
      │     ├─ call RenderPlanBuilder.Build  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:549) [verified]
      │     │      public static RenderPlan Build(AnalysisSnapshot snapshot, RenderRequest request)
      │     │      => new()
      │     │      IncludedTypeIds = [],
      │     └─ call GraphDiagnosticsTail  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:543) [verified]
      │            private static string GraphDiagnosticsTail(AnalysisSnapshot snapshot, RenderRequest request)
      │            if (!request.IncludeDiagnostics) return string.Empty;
      │            var lines = snapshot.Model.Diagnostics
      │        ├─ call AnalysisSnapshot  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:611) [approx]
      │        │      /// <summary>Immutable result of the analyze phase. The model must not be mutated after this is created.</summary>
      │        │      public sealed record AnalysisSnapshot
      │        │      public required DiscoveryModel Model { get; init; }
      │        └─ call AnalysisSnapshot.Where  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:611) [approx]
      ├─ call AnalysisSession.Trace  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:146) [verified]
      ├─ call DevContextGrpcService.ParseDetail  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:143) [verified]
      │      private static TraceDetail ParseDetail(string? detail) => detail?.ToLowerInvariant() switch
      │      "signature" => TraceDetail.Signature,
      │      "full" => TraceDetail.Full,
      └─ call DevContextGrpcService.WrapAsyncT  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:140) [verified]
             private async Task<T> WrapAsyncT<T>(string handle, Func<AnalysisSession, Task<T>> action, [System.Runtime.CompilerServices.CallerMemberName] string tool = "")
             var sw = Stopwatch.StartNew();
             try
         ├─ call DevContextGrpcService.MapException  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:609) [verified]
         │      private static RpcException MapException(Exception ex) => ex switch
         │      AnalysisException ae => new RpcException(new Status(AnalysisCodeToGrpc(ae.Code), ae.Message)),
         │      InvalidOperationException => new RpcException(new Status(StatusCode.InvalidArgument, ex.Message)),
         │  └─ call DevContextGrpcService.AnalysisCodeToGrpc  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:648) [verified]
         │         private static StatusCode AnalysisCodeToGrpc(string code) => code switch
         │         "GitNotInstalled" => StatusCode.FailedPrecondition,
         │         "NotFound" => StatusCode.NotFound,
         ├─ call DevContextGrpcService.CompleteCall  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:605) [verified]
         │      private void CompleteCall<T>(AnalysisSession session, T result, string tool, Stopwatch sw)
         │      var bytes = result switch
         │      IMessage msg => msg.CalculateSize(),
         │  └─ call RecordToolCall  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:621) [verified]
         │         private void RecordToolCall(string tool, string handle, string repo, int bytes, long elapsedMs)
         │         // T6.10 — the desktop app calls over gRPC-web (content-type "application/grpc-web*"),
         │         // the MCP sidecar over native gRPC ("application/grpc"): that one header separates the
         │     ├─ call McpObservabilityService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:554) [verified]
         │     │      public sealed class McpObservabilityService
         │     │      private readonly ConcurrentDictionary<string, ChannelWriter<ToolCallEvent>> _observers = new();
         │     │      private volatile bool _running;
         │     └─ call Notify  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:554) [verified]
         │            public void Notify(ToolCallEvent evt)
         │            if (!_running) return;
         │            var stale = new List<string>();
         │        (stopped at depth 5; 1 branch omitted)
         ├─ call DevContextGrpcService.action  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:603) [approx]
         └─ call Require  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:602) [verified]
                private AnalysisSession Require(string handle)
                var session = sessions.Get(handle)
                ?? throw new RpcException(new Status(StatusCode.NotFound, $"Unknown session handle: {handle}"));
            ├─ call IAnalysisSessionManager  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:576) [approx]
            │      public interface IAnalysisSessionManager
            │      Task<AnalysisSession> AnalyzeAsync(AnalyzeSpec spec, IProgress<AnalysisProgress>? progress, CancellationToken ct);
            │      AnalysisSession? Get(string handle);
            │  └─ di AnalysisSessionManager  (src/DevContext.Server/Program.cs:16)
            │         public sealed class AnalysisSessionManager : IAnalysisSessionManager, IAsyncDisposable
            │         private readonly ConcurrentDictionary<string, SessionEntry> _sessions = new(StringComparer.Ordinal);
            │         private readonly ConcurrentDictionary<string, string> _repoToHandle = new(StringComparer.Ordinal);
            └─ call AnalysisSessionManager.Get  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:576) [verified]
                   public AnalysisSession? Get(string handle)
                   if (!_sessions.TryGetValue(handle, out var entry)) return null;
                   entry.LastAccess = DateTime.UtcNow;

---

### Trace 2: DevContextService.Analyze

TRACE  DevContextService.Analyze
       src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17
       DevContext.Server
▸ ENTRY  DevContextService.Analyze  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
   └─ call DevContextGrpcService.Analyze  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
          public override async Task Analyze(
          Proto.AnalyzeRequest request,
          IServerStreamWriter<Proto.AnalyzeEvent> responseStream,
      ├─ call AnalysisSession  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:63) [approx]
      │      public sealed class AnalysisSession(string handle, EngineResult engine) : IAsyncDisposable
      │      private GraphQuery? _query;
      │      public string Handle { get; } = handle;
      ├─ call ProtoMapper.ToSummary  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:71) [verified]
      │      public static Proto.AnalysisSummary ToSummary(EngineResult engine, AnalysisSnapshot snapshot, int entriesWithTarget)
      │      var summary = new Proto.AnalysisSummary
      │      Label = engine.Label,
      ├─ call AnalysisSession.Stats  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:70) [approx]
      └─ call RunAnalysisWithProgressAsync  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:41) [verified]
             private async Task<AnalysisSession> RunAnalysisWithProgressAsync(
             AnalyzeSpec spec, ChannelWriter<Proto.AnalyzeEvent> writer, string path, CancellationToken ct)
             var progress = new ChannelProgress(writer);
         ├─ call IAnalysisSessionManager  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:89) [approx]
         │      public interface IAnalysisSessionManager
         │      Task<AnalysisSession> AnalyzeAsync(AnalyzeSpec spec, IProgress<AnalysisProgress>? progress, CancellationToken ct);
         │      AnalysisSession? Get(string handle);
         │  └─ di AnalysisSessionManager  (src/DevContext.Server/Program.cs:16)
         │         public sealed class AnalysisSessionManager : IAnalysisSessionManager, IAsyncDisposable
         │         private readonly ConcurrentDictionary<string, SessionEntry> _sessions = new(StringComparer.Ordinal);
         │         private readonly ConcurrentDictionary<string, string> _repoToHandle = new(StringComparer.Ordinal);
         ├─ call DevContextGrpcService.Error  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:105) [verified]
         │      private static Proto.AnalyzeEvent Error(string code, string message)
         │      => new() { Error = new Proto.AnalyzeError { Code = code, Message = message } };
         └─ call AnalyzeAsync  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:89) [verified]
                public async Task<AnalysisSession> AnalyzeAsync(AnalyzeSpec spec, IProgress<AnalysisProgress>? progress, CancellationToken ct)
                await EvictIfNeededAsync().ConfigureAwait(false);
                var repoPath = ResolveRepoPath(spec.Path);
            ├─ call IEngineRunner  (src/DevContext.Server/Sessions/AnalysisSessionManager.cs:37) [approx]
            │      /// <summary>The one component that knows how to drive <c>DevContext.Core</c>: resolve the root,
            │      /// resolve intent, build options, stand up the engine's per-repo DI container, and analyze. Keeping
            │      /// this knowledge in a single place is what stops engine wiring from leaking into the transport layer
            │  └─ di EngineRunner  (src/DevContext.Server/Program.cs:15)
            │         public sealed class EngineRunner(ILoggerFactory loggerFactory, EngineHostCache hostCache, CloneRegistry cloneRegistry) : IEngineRunner
            │         private readonly RealFileSystem _fs = new();
            │         private readonly SnapshotCacheService _snapCache = new();
            ├─ call AnalysisSessionManager.RepoKey  (src/DevContext.Server/Sessions/AnalysisSessionManager.cs:47) [verified]
            │      private static string RepoKey(string repoPath, string commitSha)
            │      => $"{repoPath}@{commitSha}";
            ├─ call AnalyzeAsync  (src/DevContext.Server/Sessions/AnalysisSessionManager.cs:37) [verified]
            │      public async Task<EngineResult> AnalyzeAsync(AnalyzeSpec spec, IProgress<AnalysisProgress>? progress, CancellationToken ct)
            │      var sw = Stopwatch.StartNew();
            │      var repoUrl = RepoUrl.Parse(spec.Path);
            │  (8 more branches omitted beyond fan-out)
            │  ├─ call RunReportCollector  (src/DevContext.Server/Sessions/EngineRunner.cs:93) [verified]
            │  │      /// <summary>Collects structured run statistics from the observer callback stream. Thread-safe for parallel stages.</summary>
            │  │      public sealed class RunReportCollector : IDiscoveryObserver
            │  │      private readonly Stopwatch _totalSw = Stopwatch.StartNew();
            │  ├─ call SnapshotCacheService.SaveAsync  (src/DevContext.Server/Sessions/EngineRunner.cs:112) [verified]
            │  │      public async Task<bool> SaveAsync(string repoKey, string versionKey, object data, CancellationToken ct)
            │  │      var path = GetSnapshotPath(repoKey, versionKey);
            │  │      try
            │  │  (stopped at depth 5; 3 branches omitted)
            │  ├─ call AnalyzeAsync  (src/DevContext.Server/Sessions/EngineRunner.cs:110) [verified]
            │  │      public async Task<AnalysisSnapshot> AnalyzeAsync(DiscoveryContext context, CancellationToken ct = default)
            │  │      if (context.Options.DryRun)
            │  │      return await BuildDryRunSnapshotAsync(context, ct);
            │  │  (stopped at depth 5; 30 branches omitted)
            │  ├─ call RunReportCollector.SetBudget  (src/DevContext.Server/Sessions/EngineRunner.cs:93) [verified]
            │  │      public void SetBudget(int budget) => _budget = budget;
            │  ├─ call GetOrCreate  (src/DevContext.Server/Sessions/EngineRunner.cs:90) [verified]
            │  │      public EngineHost GetOrCreate(string rootPath)
            │  │      return _hosts.GetOrAdd(rootPath, _ =>
            │  │      var cache = new PersistentAnalysisCache(new RealFileSystem());
            │  │  (stopped at depth 5; 1 branch omitted)
            │  ├─ call SnapshotCacheService.TryLoadAsync  (src/DevContext.Server/Sessions/EngineRunner.cs:69) [verified]
            │  │      public async Task<T?> TryLoadAsync<T>(string repoKey, string versionKey, CancellationToken ct) where T : class
            │  │      var path = GetSnapshotPath(repoKey, versionKey);
            │  │      if (!File.Exists(path)) return null;
            │  │  (stopped at depth 5; 2 branches omitted)
            │  ├─ call SnapshotCacheService.Exists  (src/DevContext.Server/Sessions/EngineRunner.cs:67) [verified]
            │  │      public bool Exists(string repoKey, string versionKey)
            │  │      => File.Exists(GetSnapshotPath(repoKey, versionKey));
            │  │  (stopped at depth 5; 1 branch omitted)
            │  └─ call SnapshotCacheService.ComputeKeys  (src/DevContext.Server/Sessions/EngineRunner.cs:66) [verified]
            │         public static (string RepoKey, string VersionKey) ComputeKeys(string rootPath)
            │         var normalized = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            │         var repoKey = HashString(normalized);
            │     (stopped at depth 5; 3 branches omitted)
            ├─ call AnalysisSessionManager.TryGetByRepo  (src/DevContext.Server/Sessions/AnalysisSessionManager.cs:30) [verified]
            │      public AnalysisSession? TryGetByRepo(string repoPath, string commitSha)
            │      var repoKey = RepoKey(repoPath, commitSha);
            │      if (!_repoToHandle.TryGetValue(repoKey, out var handle)) return null;
            │  ├─ call AnalysisSessionManager.Get  (src/DevContext.Server/Sessions/AnalysisSessionManager.cs:66) [verified]
            │  │      public AnalysisSession? Get(string handle)
            │  │      if (!_sessions.TryGetValue(handle, out var entry)) return null;
            │  │      entry.LastAccess = DateTime.UtcNow;
            │  └─ call AnalysisSessionManager.RepoKey  (src/DevContext.Server/Sessions/AnalysisSessionManager.cs:64) [verified]
            │         private static string RepoKey(string repoPath, string commitSha)
            │         => $"{repoPath}@{commitSha}";
            ├─ call AnalysisSessionManager.ResolveCommitSha  (src/DevContext.Server/Sessions/AnalysisSessionManager.cs:25) [verified]
            │      private static string ResolveCommitSha(string repoPath)
            │      try
            │      var dir = repoPath;
            ├─ call AnalysisSessionManager.ResolveRepoPath  (src/DevContext.Server/Sessions/AnalysisSessionManager.cs:24) [verified]
            │      private static string ResolveRepoPath(string path)
            │      try
            │      var full = Path.GetFullPath(path);
            └─ call EvictIfNeededAsync  (src/DevContext.Server/Sessions/AnalysisSessionManager.cs:22) [verified]
                   private async Task EvictIfNeededAsync()
                   var capacity = _options.SessionCapacity;
                   if (_sessions.Count < capacity) return;
               ├─ call List  (src/DevContext.Server/Sessions/AnalysisSessionManager.cs:111) [approx]
               ├─ call AnalysisSession.DisposeAsync  (src/DevContext.Server/Sessions/AnalysisSessionManager.cs:131) [verified]
               │      public async ValueTask DisposeAsync()
               │      if (Engine.Cleanup == "keep") return;
               │      if (Engine.GitClonePath is { } clone)
               │  (stopped at depth 5; 1 branch omitted)
               └─ call AnalysisSessionManager.RepoKey  (src/DevContext.Server/Sessions/AnalysisSessionManager.cs:129) [verified]
                      private static string RepoKey(string repoPath, string commitSha)
                      => $"{repoPath}@{commitSha}";

---

### Trace 3: DevContextService.GetMap

TRACE  DevContextService.GetMap
       src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17
       DevContext.Server
▸ ENTRY  DevContextService.GetMap  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
   └─ call DevContextGrpcService.GetMap  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
          public override Task<Proto.MapResponse> GetMap(Proto.SessionRequest request, ServerCallContext context)
          => WrapAsyncT(request.Handle, async session =>
          var markdown = await session.RenderMapMarkdownAsync(context.CancellationToken).ConfigureAwait(false);
      ├─ call ProtoMapper.ToMapResponse  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:136) [verified]
      │      public static Proto.MapResponse ToMapResponse(MapModel? map, string markdown)
      │      var resp = new Proto.MapResponse
      │      Markdown = markdown,
      │  └─ call ProtoMapper.MapSurfaceGroup  (src/DevContext.Server/Mapping/ProtoMapper.cs:88) [verified]
      │         private static Proto.SurfaceGroup MapSurfaceGroup(SurfaceGroup g)
      │         var sg = new Proto.SurfaceGroup { Namespace = g.Namespace };
      │         foreach (var t in g.Types)
      ├─ call RenderMapMarkdownAsync  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:135) [verified]
      │      public async Task<string> RenderMapMarkdownAsync(CancellationToken ct)
      │      var rendered = await Engine.Pipeline
      │      .RenderAsync(Snapshot, BuildRequest(entry: null, depth: null, TraceDetail.Salient), ct)
      │  ├─ call EngineResult  (src/DevContext.Server/Sessions/AnalysisSession.cs:31) [approx]
      │  │      /// <summary>The product of one analysis: the immutable snapshot plus the live pipeline that produced
      │  │      /// it (kept so cheap re-renders use the same path-bound resolvers), plus display metadata. The DI
      │  │      /// container that owns the pipeline lives in <see cref="EngineHostCache"/> — the session must NOT
      │  ├─ call DiscoveryPipeline  (src/DevContext.Server/Sessions/AnalysisSession.cs:32) [verified]
      │  │      /// <summary>Orchestrates the discovery pipeline: extraction, signal sealing, pruning, compression, and rendering.</summary>
      │  │      public sealed class DiscoveryPipeline
      │  │      private readonly IReadOnlyList<IDiscoveryExtractor> _extractors;
      │  ├─ call AnalysisSession.BuildRequest  (src/DevContext.Server/Sessions/AnalysisSession.cs:32) [verified]
      │  │      private RenderRequest BuildRequest(string? entry, int? depth, TraceDetail detail) => new()
      │  │      Format = "markdown",
      │  │      MaxTokens = Snapshot.Options.MaxOutputTokens,
      │  └─ call RenderAsync  (src/DevContext.Server/Sessions/AnalysisSession.cs:31) [verified]
      │         public async Task<RenderedContext> RenderAsync(AnalysisSnapshot snapshot, RenderRequest request, CancellationToken ct = default)
      │         // ── PLAN-10 A3: Map/Trace branch — when the Graph is available with content (always after a
      │         // full analyze). The Map/Trace renderers produce the human-facing markdown narrative; JSON/HTML
      │     (9 more branches omitted beyond fan-out)
      │     ├─ call NarrativeSections  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:519) [verified]
      │     │      /// <summary>Assembles ordered narrative fragments into a <see cref="RenderedContext"/> that carries
      │     │      /// both the joined <c>Content</c> and the per-section fragments the desktop toggles. This is what
      │     │      /// makes Map/Trace section-aware: the same fragments drive the LLM (markdown) view and, after HTML
      │     ├─ call AnalysisSnapshot  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:561) [approx]
      │     │      /// <summary>Immutable result of the analyze phase. The model must not be mutated after this is created.</summary>
      │     │      public sealed record AnalysisSnapshot
      │     │      public required DiscoveryModel Model { get; init; }
      │     ├─ call AnalysisSnapshot.Count  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:583) [approx]
      │     ├─ call RunSelfChecks  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:577) [verified]
      │     │      private static RenderedContext RunSelfChecks(RenderedContext rendered, DiscoveryModel model, RenderOptions renderOptions, DiscoveryModel diagnosticsTarget)
      │     │      var results = OutputSelfCheck.Check(rendered, model, renderOptions);
      │     │      var failures = ImmutableArray.CreateBuilder<string>();
      │     │  ├─ call DiscoveryModel  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:754) [approx]
      │     │  │      /// <summary>Represents the result of running the discovery pipeline, containing all extracted data.</summary>
      │     │  │      public sealed class DiscoveryModel
      │     │  │      /// <summary>Information about the solution file, if found.</summary>
      │     │  ├─ call DiscoveryModel.AddDiagnostic  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:757) [verified]
      │     │  │      public void AddDiagnostic(DiagnosticLevel level, string source, string message)
      │     │  │      => Diagnostics.Add(new(level, source, message, DateTimeOffset.UtcNow));
      │     │  └─ call OutputSelfCheck.Check  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:749) [verified]
      │     │         public static ImmutableArray<SelfCheckResult> Check(
      │     │         RenderedContext rendered,
      │     │         DiscoveryModel model,
      │     │     (stopped at depth 5; 10 branches omitted)
      │     ├─ call AnalysisSnapshot.ToImmutableArray  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:563) [approx]
      │     ├─ call AnalysisSnapshot.ToString  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:561) [approx]
      │     ├─ call RenderPlanBuilder.Build  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:549) [verified]
      │     │      public static RenderPlan Build(AnalysisSnapshot snapshot, RenderRequest request)
      │     │      => new()
      │     │      IncludedTypeIds = [],
      │     └─ call GraphDiagnosticsTail  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:543) [verified]
      │            private static string GraphDiagnosticsTail(AnalysisSnapshot snapshot, RenderRequest request)
      │            if (!request.IncludeDiagnostics) return string.Empty;
      │            var lines = snapshot.Model.Diagnostics
      │        ├─ call AnalysisSnapshot  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:611) [approx]
      │        │      /// <summary>Immutable result of the analyze phase. The model must not be mutated after this is created.</summary>
      │        │      public sealed record AnalysisSnapshot
      │        │      public required DiscoveryModel Model { get; init; }
      │        └─ call AnalysisSnapshot.Where  (src/DevContext.Core/Pipeline/DiscoveryPipeline.cs:611) [approx]
      └─ call DevContextGrpcService.WrapAsyncT  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:133) [verified]
             private async Task<T> WrapAsyncT<T>(string handle, Func<AnalysisSession, Task<T>> action, [System.Runtime.CompilerServices.CallerMemberName] string tool = "")
             var sw = Stopwatch.StartNew();
             try
         ├─ call DevContextGrpcService.MapException  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:609) [verified]
         │      private static RpcException MapException(Exception ex) => ex switch
         │      AnalysisException ae => new RpcException(new Status(AnalysisCodeToGrpc(ae.Code), ae.Message)),
         │      InvalidOperationException => new RpcException(new Status(StatusCode.InvalidArgument, ex.Message)),
         │  └─ call DevContextGrpcService.AnalysisCodeToGrpc  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:648) [verified]
         │         private static StatusCode AnalysisCodeToGrpc(string code) => code switch
         │         "GitNotInstalled" => StatusCode.FailedPrecondition,
         │         "NotFound" => StatusCode.NotFound,
         ├─ call DevContextGrpcService.CompleteCall  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:605) [verified]
         │      private void CompleteCall<T>(AnalysisSession session, T result, string tool, Stopwatch sw)
         │      var bytes = result switch
         │      IMessage msg => msg.CalculateSize(),
         │  └─ call RecordToolCall  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:621) [verified]
         │         private void RecordToolCall(string tool, string handle, string repo, int bytes, long elapsedMs)
         │         // T6.10 — the desktop app calls over gRPC-web (content-type "application/grpc-web*"),
         │         // the MCP sidecar over native gRPC ("application/grpc"): that one header separates the
         │     ├─ call McpObservabilityService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:554) [verified]
         │     │      public sealed class McpObservabilityService
         │     │      private readonly ConcurrentDictionary<string, ChannelWriter<ToolCallEvent>> _observers = new();
         │     │      private volatile bool _running;
         │     └─ call Notify  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:554) [verified]
         │            public void Notify(ToolCallEvent evt)
         │            if (!_running) return;
         │            var stale = new List<string>();
         │        (stopped at depth 5; 1 branch omitted)
         ├─ call DevContextGrpcService.action  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:603) [approx]
         └─ call Require  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:602) [verified]
                private AnalysisSession Require(string handle)
                var session = sessions.Get(handle)
                ?? throw new RpcException(new Status(StatusCode.NotFound, $"Unknown session handle: {handle}"));
            ├─ call IAnalysisSessionManager  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:576) [approx]
            │      public interface IAnalysisSessionManager
            │      Task<AnalysisSession> AnalyzeAsync(AnalyzeSpec spec, IProgress<AnalysisProgress>? progress, CancellationToken ct);
            │      AnalysisSession? Get(string handle);
            │  └─ di AnalysisSessionManager  (src/DevContext.Server/Program.cs:16)
            │         public sealed class AnalysisSessionManager : IAnalysisSessionManager, IAsyncDisposable
            │         private readonly ConcurrentDictionary<string, SessionEntry> _sessions = new(StringComparer.Ordinal);
            │         private readonly ConcurrentDictionary<string, string> _repoToHandle = new(StringComparer.Ordinal);
            └─ call AnalysisSessionManager.Get  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:576) [verified]
                   public AnalysisSession? Get(string handle)
                   if (!_sessions.TryGetValue(handle, out var entry)) return null;
                   entry.LastAccess = DateTime.UtcNow;

---

## Insights

_5 info · 2 notable_

### **NOTABLE**: Config without defaults: 1 consumed keys have no appsettings default
*(Risk)*

- x

### **NOTABLE**: 1/1 endpoints anonymous
*(Risk)*

- GET /health

### _INFO_: Entry targets resolved 28/28 (100%) — trace any entry for its full path
*(Coverage)*

### _INFO_: DI: 28 Singleton · 22 Extension · 5 Scoped (55 total)
*(Wiring)*

### _INFO_: Entry surface: 24 GrpcService · 1 HTTP
*(Shape)*

- 24 GrpcService
- 1 HTTP

### _INFO_: Most depended-upon: Core (6 dependents) · DevContext.Core (2 dependents) · DevContext.Core (2 dependents)
*(Topology)*

- Core (6 dependents)
- DevContext.Core (2 dependents)
- DevContext.Core (2 dependents)

### _INFO_: Wiring hubs: DiscoveryModel (335) · List (133) · FakeFileSystem (105) · CodeGraphBuilder (62) · DiscoveryContext (45)
*(Wiring)*

- DiscoveryModel (335)
- List (133)
- FakeFileSystem (105)
- CodeGraphBuilder (62)
- DiscoveryContext (45)

MAP  DevContext     (6 projects)

STACK  net10.0 · Minimal APIs · Controllers · MediatR (CQRS)

STYLE  CleanArchitecture  (confidence high)
       evidence: DDD folder layers: Domain, Application, Infrastructure, Api, Core; MediatR with 2 handlers

       per service:
         DevContext.Benchmarks: Unknown
         DevContext.Cli: CLI [CLI]
         DevContext.Mcp: Unknown
         DevContext.Server: gRPC Service [gRPC]
         Api: Unknown
         Web: CQRS [MediatR]
         CompositionApp.AppHost: Aspire AppHost [Aspire]
         Web: Unknown
         Api: Unknown
         Web: Unknown
         App: CQRS [MediatR, FluentValidation]
         Api: CQRS [MediatR]

TOPOLOGY (depends-on)
   DevContext.Core
   DevContext.Cli ── DevContext.Core
   DevContext.Contracts
   DevContext.Benchmarks ── DevContext.Cli, DevContext.Core
   DevContext.Mcp ── DevContext.Contracts
   DevContext.Server ── DevContext.Cli, DevContext.Contracts, DevContext.Core

CROSS-SERVICE
  gRPC (1)
    [gRPC] DevContext.Mcp → DevContext.Server  (C:\code\DevContext2\src\DevContext.Mcp\DevContextTools.cs:26→C:\code\DevContext2\src\DevContext.Server\Endpoints\DevContextGrpcService.cs:17)

ENTRY POINTS
   HTTP (1)
      GET /health  → Program  (src/DevContext.Server/Program.cs:44)
   gRPC (24)
      DevContextService.Analyze  → AnalysisSession  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.CloseSession  → AnalysisSessionManager.CloseSessionAsync  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.ConfigLookup  → AnalysisSession.ConfigBindings  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.FindTestsFor  → TestHeuristics.IsLikelyTestMethod  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetContext  → ContextPackBuilder.Build  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetContextPack  → ContextPackBuilder.BuildMulti  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetGraphFacets  → ServiceMapProjection.Project  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetImpact  → ProtoMapper.ToImpactResponse  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetInterestingPoints  → ProtoMapper.ToInterestingPointsResponse  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetMap  → AnalysisSession.RenderMapMarkdownAsync  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetNeighbors  → ProtoMapper.ToProto  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetNode  → ProtoMapper.ToNodeResponse  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetStats  → ProtoMapper.ToStatsResponse  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.GetTrace  → AnalysisSession.RenderTraceMarkdownAsync  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.ListEntryPoints  → ProtoMapper.ToProto  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.ListSessions  → AnalysisSessionManager.ListSessions  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.ObserveToolCalls  → McpObservabilityService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.Ping  → DevContextGrpcService  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.ReadSource  → AnalysisSession.Node  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      DevContextService.Render  → AnalysisSession.RenderAsync  (src/DevContext.Server/Endpoints/DevContextGrpcService.cs:17)
      … and 4 more (grpc entries — use --focus for a drill-in)
   CLI (3)
      AnalyzeCommand —settings AnalyzeSettings  → AnalyzeCommand  (src/DevContext.Cli/Commands/AnalyzeCommand.cs:10)
      QueryCommand —settings QuerySettings  → QueryCommand  (src/DevContext.Cli/Commands/QueryCommand.cs:14)
      ReportCommand —settings ReportSettings  → ReportCommand  (src/DevContext.Cli/Commands/ReportCommand.cs:11)

PACKAGES
   Web/API:  Grpc.AspNetCore 2.66.0, Grpc.AspNetCore.Web 2.*, HotChocolate.AspNetCore 13.9.0, Microsoft.AspNetCore.Mvc.Testing 10.0.*, Microsoft.AspNetCore.OpenApi 10.0.0
   ORM/Data:  Dapper 2.1.35, Microsoft.EntityFrameworkCore 10.0.0-preview.3.25171.5
   Mediator/CQRS:  MediatR 12.0.0
   Validation:  FluentValidation 11.5.0
   Logging:  Serilog 4.3.1, Serilog.Extensions.Logging 10.0.0, Serilog.Sinks.Console 6.1.1, Serilog.Sinks.File 7.0.0
   Testing:  coverlet.collector 10.0.1, xunit 2.9.3, xunit.runner.visualstudio 3.1.5, Xunit.SkippableFact 1.5.23
   Cloud:  Microsoft.Azure.Functions.Worker 1.23.0
   Other:  BenchmarkDotNet 0.15.8, Google.Protobuf 3.*, Grpc.Core.Api 2.*, Grpc.Net.Client 2.*, Grpc.Net.Client.Web 2.*, Grpc.Tools 2.*, LibGit2Sharp 0.30.*, Microsoft.CodeAnalysis.CSharp 5.3.0 … (17 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "DevContextService.GetTrace")
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 20957ms |
| GenericExtraction | 975ms |
| SignalSealing | 0ms |
| SpecificExtraction | 5681ms |
| Compression | 140ms |
| **Total** | **43335ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| FileTreeExtractor | 14362ms | 0 | 0 |
| SolutionDiscovery | 6518ms | 0 | 0 |
| CallGraphExtractor | 4173ms | 0 | 0 |
| EndpointExtractor | 1502ms | 0 | 37 |
| ProgramCsFlowExtractor | 971ms | 576 | 67 |
| SyntaxStructureExtractor | 945ms | 576 | 55 |
| DiRegistrationExtractor | 940ms | 1 | 55 |
| CliCommandExtractor | 634ms | 0 | 10 |
| MediatRExtractor | 614ms | 0 | 27 |
| SignalRHubExtractor | 609ms | 0 | 28 |
| InMemoryEventBusExtractor | 571ms | 0 | 24 |
| GrpcServiceExtractor | 563ms | 0 | 25 |
| ControllerActionExtractor | 549ms | 0 | 23 |
| GrpcClientExtractor | 476ms | 0 | 21 |
| AzureFunctionsExtractor | 443ms | 0 | 20 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 2164 | 635 |
| Sends | 7 | 0 |
| Resolves | 13 | 1 |
| ServiceLink | 1 | 0 |

_391 files · 25 projects_
