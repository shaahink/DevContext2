using System.Diagnostics;
using System.Threading.Channels;

using DevContext.Core.Analysis;
using DevContext.Core.Graph;
using DevContext.Server.Mapping;
using DevContext.Server.Services;
using DevContext.Server.Sessions;

using Google.Protobuf;
using Grpc.Core;

using Proto = DevContext.Protos;

namespace DevContext.Server.Endpoints;

public sealed class DevContextGrpcService(
    IAnalysisSessionManager sessions,
    McpObservabilityService mcpObs,
    IHttpContextAccessor httpContext,
    ILogger<DevContextGrpcService> logger)
    : Proto.DevContextService.DevContextServiceBase
{
    public override async Task Analyze(
        Proto.AnalyzeRequest request,
        IServerStreamWriter<Proto.AnalyzeEvent> responseStream,
        ServerCallContext context)
    {
        var ct = context.CancellationToken;
        var spec = new AnalyzeSpec(
            request.Path,
            request.HasFocus ? request.Focus : null,
            request.HasDepth ? request.Depth : null,
            request.HasDetail ? request.Detail : null,
            request.NoRoslyn,
            request.HasCleanup ? request.Cleanup : null,
            request.HasSln ? request.Sln : null);

        var channel = Channel.CreateUnbounded<Proto.AnalyzeEvent>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

        var work = RunAnalysisWithProgressAsync(spec, channel.Writer, request.Path, ct);

        bool sawError = false;
        try
        {
            await foreach (var evt in channel.Reader.ReadAllAsync(ct).ConfigureAwait(false))
            {
                await responseStream.WriteAsync(evt).ConfigureAwait(false);
                if (evt.EventCase == Proto.AnalyzeEvent.EventOneofCase.Error)
                {
                    sawError = true;
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Client disconnected — let work drain naturally
        }

        if (sawError)
        {
            try { await work.ConfigureAwait(false); } catch { /* error already streamed */ }
            return;
        }

        try
        {
            var session = await work.ConfigureAwait(false);
            var (_, entriesWithTarget, _, _) = session.Query.Stats();
            var summary = ProtoMapper.ToSummary(session.Engine, session.Snapshot, entriesWithTarget);
            await responseStream.WriteAsync(new Proto.AnalyzeEvent
            {
                Result = new Proto.AnalyzeResult { Handle = session.Handle, Summary = summary },
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Client disconnected — result not needed
        }
    }

    private async Task<AnalysisSession> RunAnalysisWithProgressAsync(
        AnalyzeSpec spec, ChannelWriter<Proto.AnalyzeEvent> writer, string path, CancellationToken ct)
    {
        var progress = new ChannelProgress(writer);
        try
        {
            var session = await sessions.AnalyzeAsync(spec, progress, ct).ConfigureAwait(false);
            return session;
        }
        catch (OperationCanceledException)
        {
            writer.TryWrite(Error("Cancelled", "Analysis cancelled."));
            throw;
        }
        catch (AnalysisException ex)
        {
            writer.TryWrite(Error(ex.Code, ex.Message));
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Analysis failed for {Path}", path);
            writer.TryWrite(Error("Internal", ex.Message));
            throw;
        }
        finally
        {
            writer.Complete();
        }
    }

    public override async Task<Proto.CloseResponse> CloseSession(Proto.SessionRequest request, ServerCallContext context)
    {
        var closed = await sessions.CloseSessionAsync(request.Handle).ConfigureAwait(false);
        return new Proto.CloseResponse { Closed = closed };
    }

    public override Task<Proto.EntryPointsResponse> ListEntryPoints(Proto.SessionRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var resp = new Proto.EntryPointsResponse();
            foreach (var entry in session.Query.EntryPoints())
            {
                var graphNode = session.Query.Graph.Node(entry.Node);
                resp.EntryPoints.Add(ProtoMapper.ToProto(entry, graphNode?.Layer, graphNode?.Feature));
            }
            return resp;
        });

    public override Task<Proto.MapResponse> GetMap(Proto.SessionRequest request, ServerCallContext context)
        => WrapAsyncT(request.Handle, async session =>
        {
            var markdown = await session.RenderMapMarkdownAsync(context.CancellationToken).ConfigureAwait(false);
            return ProtoMapper.ToMapResponse(session.Snapshot.Map, markdown, session.Snapshot.Model.Solution?.Name,
                session.Snapshot.Model.ScopeNote);
        });

    public override Task<Proto.TraceResponse> GetTrace(Proto.TraceRequest request, ServerCallContext context)
        => WrapAsyncT(request.Handle, async session =>
        {
            // Batch E (R2 §2.E item 1) — ONE build, ONE shaping, one answer. This used to walk the graph
            // twice: once here for the tree, then again inside the markdown render. The two walks were
            // shaped by DIFFERENT budgets (the request's budgetTokens vs the snapshot's MaxTokens), so
            // the tree the app drew and the document it copied could disagree about what was omitted.
            var depth = request.HasDepth ? request.Depth : Core.Graph.TracePolicy.DefaultDepth;
            var detail = ParseDetail(request.HasDetail ? request.Detail : null);
            var budgetTokens = request.HasBudgetTokens ? request.BudgetTokens : 0; // T3.3 — 0 = unlimited

            var entry = Core.Graph.EntryPointResolver.Resolve(session.Snapshot.Entries, session.Query.Graph, request.Focus);
            if (entry is null)
                return new Proto.TraceResponse { Found = false };

            var trace = session.Query.Trace(entry, depth, budgetTokens: budgetTokens,
                explicitDepth: request.HasDepth);

            var markdown = await session.RenderTraceMarkdownAsync(request.Focus, depth, detail, trace, context.CancellationToken)
                .ConfigureAwait(false);
            return ProtoMapper.ToTraceResponse(trace, markdown);
        });

    public override Task<Proto.NodeResponse> GetNode(Proto.NodeRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var id = ResolveNode(session, request.NodeId);
            var detail = id is { } nid ? session.Query.Node(nid) : null;
            if (detail is null)
                return new Proto.NodeResponse { Found = false };
            var graphNode = session.Query.Graph.Node(detail.Id);
            return ProtoMapper.ToNodeResponse(detail, graphNode?.Layer, graphNode?.Feature);
        });

    public override Task<Proto.NeighborsResponse> GetNeighbors(Proto.NeighborsRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var resp = new Proto.NeighborsResponse();
            if (ResolveNode(session, request.NodeId) is { } nid)
            {
                var edges = request.Direction switch
                {
                    "in" => session.Query.Neighbors(nid, EdgeDirection.In),
                    "usages" => session.Query.FindUsages(nid),
                    _ => session.Query.Neighbors(nid, EdgeDirection.Out),
                };
                foreach (var edge in edges)
                    resp.Edges.Add(ProtoMapper.ToProto(edge));
            }
            return resp;
        });

    public override Task<Proto.SearchResponse> SearchNodes(Proto.SearchRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var query = request.Query.Trim();
            var limit = request.Limit > 0 ? request.Limit : 20;
            var ranked = session.Query.Find(query, limit);
            var graph = session.Query.Graph;

            var results = new List<(string Id, string Title, string Kind, ImmutableArray<string> Tags)>();
            foreach (var r in ranked)
            {
                var node = graph.Node(r.Id);
                results.Add((r.Id.ToString(), r.Title, r.Kind.ToString(), node?.Tags ?? ImmutableArray<string>.Empty));
            }

            return ProtoMapper.ToSearchResponse(results);
        });

    public override Task<Proto.ImpactResponse> GetImpact(Proto.ImpactRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var maxDepth = request.MaxDepth > 0 ? request.MaxDepth : 4;
            var direction = request.Direction?.ToLowerInvariant() switch
            {
                "down" => ImpactDirection.Down,
                "both" => ImpactDirection.Both,
                _ => ImpactDirection.Up,
            };

            ImmutableArray<ImpactResult> results;

            if (request.Files.Count > 0)
            {
                // M4.4 diff-aware mode: impact from changed files
                results = session.Query.ImpactFromFiles(request.Files, direction, maxDepth);
            }
            else
            {
                var nodeId = ResolveNode(session, request.NodeId);
                if (nodeId is null)
                    return new Proto.ImpactResponse { Direction = request.Direction ?? "up" };

                results = session.Query.Impact(nodeId.Value, direction, maxDepth);
            }

            return ProtoMapper.ToImpactResponse(results, request.Direction ?? "up");
        });

    public override Task<Proto.InterestingPointsResponse> GetInterestingPoints(
        Proto.InterestingPointsRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var archetype = request.HasArchetype ? request.Archetype : null;
            var points = session.Query.GetInterestingPoints(archetype);
            return ProtoMapper.ToInterestingPointsResponse(points);
        });

    public override Task<Proto.ContextResponse> GetContext(Proto.ContextRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var builder = new ContextPackBuilder(session.Query, session.Snapshot);
            var budget = request.HasBudgetTokens ? request.BudgetTokens : 8000;
            var intent = request.HasIntent ? request.Intent : null;
            var pack = builder.Build(request.Focus, budget, intent);
            return ProtoMapper.ToContextResponse(request.Focus, pack);
        });

    // L4.4 — Multi-card context pack: server assembles the complete pack once from card specs,
    // with per-card budget-driven section assembly. Closes Meridian Trap A (N GetContext calls
    // replaced by one GetContextPack call; Copy/Save = exactly the server's assembled markdown).
    public override Task<Proto.ContextPackResponse> GetContextPack(Proto.ContextPackRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var builder = new ContextPackBuilder(session.Query, session.Snapshot);
            var budget = request.BudgetTokens > 0 ? request.BudgetTokens : 8000;
            var intent = request.Intent is { Length: > 0 } s ? s : null;

            var specs = request.Cards.Select(c =>
                new ContextCardSpec(c.Type, c.Title, [.. c.EntryIds])).ToList();

            var pack = builder.BuildMulti(specs, budget, intent);
            return ProtoMapper.ToContextPackResponse(pack);
        });

    // T4.5 (audit R6) — staleness verification: rebuild the focus's sections (cheap — the graph
    // is immutable since analyze, so the file sets are stable) and compare each section's
    // analyze-time file fingerprints against disk now.
    public override Task<Proto.VerifyContextResponse> VerifyContext(Proto.VerifyContextRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var builder = new ContextPackBuilder(session.Query, session.Snapshot);
            var budget = request.HasBudgetTokens ? request.BudgetTokens : 8000;
            var pack = builder.Build(request.Focus, budget);

            var sections = new ContextPackVerifier(session.Snapshot).Verify(pack.Sections);
            var currentHead = GitHeadReader.Read(session.Snapshot.RootPath);
            return ProtoMapper.ToVerifyContextResponse(
                request.Focus, pack.Found, session.Snapshot.GitHead, currentHead, sections);
        });

    // T7.4 (audit B11) — the whole flow atlas in one call, memoized on the session: the app's
    // per-boot ~100 GetTrace + ~10 GetNode storm becomes 1 RPC (page-render budget ≤15/nav).
    public override Task<Proto.FlowIndexResponse> GetFlowIndex(Proto.FlowIndexRequest request, ServerCallContext context)
        => WrapT(request.Handle, session => ProtoMapper.ToFlowIndexResponse(session.FlowIndex()));

    public override Task<Proto.GraphFacetsResponse> GetGraphFacets(Proto.GraphFacetsRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var graph = session.Query.Graph;
            var maxFlows = request.MaxFlows > 0 ? request.MaxFlows : 10;

            // S9 contract sweep — this call used to project four facets and ship them all. The
            // EntryTable and LayerBand projections ran on every call (LayerBand walks every node in
            // the graph) and NO client read either: the app reads entries from GetEntryPoints, and
            // MCP only ever touched ServiceMap and FlowList. Both are out of the contract now.
            var serviceMap = new ServiceMapProjection().Project(graph, ProjectionOptions.Default);
            var flowList = new FlowListProjection().Project(graph, new ProjectionOptions { MaxFlows = maxFlows });

            return ProtoMapper.ToGraphFacetsResponse(serviceMap, flowList, graph.EventWiring);
        });

    // Gap 1 — read_source RPC: Returns raw source code for the Inspector Code tab.
    // MEMBER mode uses the node's SourceBody line count to determine the span;
    // WINDOW mode returns context lines around the node's declaration line.
    public override Task<Proto.ReadSourceResponse> ReadSource(Proto.ReadSourceRequest request, ServerCallContext context)
        => WrapT(request.SessionId, session =>
        {
            var nodeId = ResolveNode(session, request.NodeId);
            if (nodeId is null)
                return new Proto.ReadSourceResponse
                {
                    Content = "Node not found: " + request.NodeId,
                    Language = "text"
                };

            var node = session.Query.Graph.Node(nodeId.Value);
            if (node is null || node.FilePath is null || node.LineNumber is null)
                return new Proto.ReadSourceResponse
                {
                    Content = "Source not available for node: " + node?.Title ?? request.NodeId,
                    Language = "text"
                };

            if (!File.Exists(node.FilePath))
                return new Proto.ReadSourceResponse
                {
                    Content = "Source file not found: " + node.FilePath,
                    Language = "text",
                    FilePath = node.FilePath
                };

            var lines = File.ReadAllLines(node.FilePath);
            var anchorLine = node.LineNumber.Value;
            int startLine;
            int endLine;

            if (request.Mode == Proto.ReadSourceMode.Window)
            {
                var window = request.WindowLines > 0 ? request.WindowLines : 50;
                startLine = Math.Max(1, anchorLine - window);
                endLine = Math.Min(lines.Length, anchorLine + window);
                // Clamp start to ensure we show at most 2*window lines total
                startLine = Math.Max(1, endLine - 2 * window + 1);
            }
            else // MEMBER — full declaration body
            {
                startLine = anchorLine;
                var bodyLineCount = node.SourceBody?.Count(c => c == '\n') ?? 0;
                endLine = bodyLineCount > 0
                    ? Math.Min(lines.Length, anchorLine + bodyLineCount)
                    : Math.Min(lines.Length, anchorLine + 20); // fallback: 20-line window
            }

            var selectedLines = lines[(startLine - 1)..endLine];
            var content = string.Join("\n", selectedLines);

            var language = Path.GetExtension(node.FilePath)?.ToLowerInvariant() switch
            {
                ".cs" => "csharp",
                ".razor" => "razor",
                ".cshtml" => "csharp",
                ".xaml" => "xml",
                ".axaml" => "xml",
                _ => "text"
            };

            return new Proto.ReadSourceResponse
            {
                Content = content,
                Language = language,
                FilePath = node.FilePath,
                StartLine = startLine,
                EndLine = endLine,
                NodeTitle = node.Title
            };
        });

    // M4.7 / T3.4 — config key lookup. The file scan + regex is now done ONCE per session
    // (AnalysisSession.ConfigBindings, cached) and filtered in-memory here — previously it re-scanned
    // every node-bearing file on every call (10.5s on shamshir). ConfigScanner owns the scan.
    public override Task<Proto.ConfigResponse> ConfigLookup(Proto.ConfigRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var resp = new Proto.ConfigResponse();
            var keyFilter = request.HasKey ? request.Key : null;
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var b in session.ConfigBindings())
            {
                if (keyFilter is not null && !string.Equals(b.Key, keyFilter, StringComparison.OrdinalIgnoreCase)) continue;
                seenKeys.Add(b.Key);
                resp.Bindings.Add(new Proto.ConfigBinding
                {
                    Key = b.Key,
                    FilePath = b.FilePath,
                    LineNumber = b.LineNumber,
                    NodeId = b.NodeId,
                    PatternType = b.PatternType,
                    Service = b.Service,
                });
            }

            resp.TotalKeys = seenKeys.Count;
            return resp;
        });

    // M4.9 — tests_for: find test methods whose call closure reaches a target node
    public override Task<Proto.TestsForResponse> FindTestsFor(Proto.TestsForRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var nodeId = ResolveNode(session, request.NodeId);
            var resp = new Proto.TestsForResponse
            {
                NodeId = request.NodeId,
                NodeTitle = "",
                IsBestEffort = true,
            };

            if (nodeId is null) return resp;

            var targetNode = session.Query.Graph.Node(nodeId.Value);
            resp.NodeTitle = targetNode?.Title ?? nodeId.Value.Key;

            var maxDepth = request.MaxDepth > 0 ? request.MaxDepth : 6;
            var callers = session.Query.FindCallers(nodeId.Value, maxDepth);

            foreach (var (cid, title, filePath, lineNumber, project, distance) in callers)
            {
                if (!TestHeuristics.IsLikelyTestMethod(title, filePath, project, session.Snapshot.RootPath)) continue;

                resp.Tests.Add(new Proto.TestRef
                {
                    NodeId = cid.ToString(),
                    Title = title,
                    FilePath = filePath ?? "",
                    LineNumber = lineNumber ?? 0,
                    Distance = distance,
                    IsDirect = distance == 1,
                    Project = project ?? "",
                });
            }

            return resp;
        });

    public override Task<Proto.StatsResponse> GetStats(Proto.SessionRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var snapshot = session.Snapshot;
            var (seams, entriesWithTarget, _, _) = session.Query.Stats();
            return ProtoMapper.ToStatsResponse(
                snapshot.Report,
                snapshot.Graph,
                snapshot.Graph?.NodeCount ?? 0,
                snapshot.Graph?.EdgeCount ?? 0,
                snapshot.Entries.Length,
                seams,
                entriesWithTarget,
                (long)(snapshot.Report?.TotalWall.TotalMilliseconds ?? session.Engine.ElapsedMs),
                snapshot.Insights,
                snapshot.Entries,
                snapshot.Model.ExtractionFailures);
        });

    public override Task<Proto.RenderResponse> Render(Proto.RenderRequest request, ServerCallContext context)
        => WrapAsyncT(request.Handle, async session =>
        {
            var detail = ParseDetail(request.HasDetail ? request.Detail : null);

            var sections = request.Sections.Count > 0
                ? ImmutableArray.CreateRange(request.Sections)
                : session.Snapshot.Scenario.RequiredSections;

            var rendered = await session.RenderAsync(
                request.HasFocus ? request.Focus : null,
                request.HasDepth ? request.Depth : null,
                detail,
                string.IsNullOrEmpty(request.Format) ? "markdown" : request.Format,
                sections,
                request.IncludeDiagnostics,
                context.CancellationToken).ConfigureAwait(false);

            return ProtoMapper.ToRenderResponse(rendered);
        });

    public override Task<Proto.ListSessionsResponse> ListSessions(Proto.ListSessionsRequest request, ServerCallContext context)
    {
        try
        {
            var list = sessions.ListSessions();
            var resp = new Proto.ListSessionsResponse();
            foreach (var s in list)
            {
                resp.Sessions.Add(new Proto.SessionInfo
                {
                    Handle = s.Handle,
                    Repo = s.RepoPath,
                    CommitSha = s.CommitSha,
                    AgeSeconds = (long)(DateTime.UtcNow - s.CreatedAt).TotalSeconds,
                    Calls = s.CallCount,
                    TokenTotal = s.TokenTotal,
                    Status = "ready",
                    Nodes = s.Snapshot.Graph?.NodeCount ?? 0,
                    Edges = s.Snapshot.Graph?.EdgeCount ?? 0,
                    Entries = s.Snapshot.Entries.Length,
                });
            }
            return Task.FromResult(resp);
        }
        catch (RpcException) { throw; }
        catch (Exception ex) { throw MapException(ex); }
    }

    public override Task<Proto.StartMcpResponse> StartMcp(Proto.StartMcpRequest request, ServerCallContext context)
    {
        mcpObs.Start();
        return Task.FromResult(new Proto.StartMcpResponse { Running = mcpObs.IsRunning });
    }

    public override Task<Proto.StopMcpResponse> StopMcp(Proto.StopMcpRequest request, ServerCallContext context)
    {
        mcpObs.Stop();
        return Task.FromResult(new Proto.StopMcpResponse { Stopped = true });
    }

    public override async Task ObserveToolCalls(
        Proto.ObserveToolCallsRequest request,
        IServerStreamWriter<Proto.ToolCallEvent> responseStream,
        ServerCallContext context)
    {
        var ct = context.CancellationToken;
        var channel = Channel.CreateUnbounded<Proto.ToolCallEvent>();

        var observerId = Guid.NewGuid().ToString("N");
        using var _ = mcpObs.Subscribe(observerId, channel.Writer);

        // Start MCP if not already running
        mcpObs.Start();

        try
        {
            await foreach (var evt in channel.Reader.ReadAllAsync(ct).ConfigureAwait(false))
                await responseStream.WriteAsync(evt).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Observer disconnected
        }
    }

    // M3.3 — emit tool-call events on every session access
    private void RecordToolCall(string tool, string handle, string repo, int bytes, long elapsedMs)
    {
        // T6.10/F5 — ui vs agent from the PRE-UseGrpcWeb content-type (stashed in Items by
        // Program.cs: the middleware rewrites grpc-web requests to plain application/grpc,
        // so reading Request.ContentType here mislabeled every app RPC as "agent").
        var origin = httpContext.HttpContext?.Items[OriginTag.ItemKey] as string
            ?? OriginTag.FromContentType(httpContext.HttpContext?.Request.ContentType);
        mcpObs.Notify(new Proto.ToolCallEvent
        {
            SessionHandle = handle,
            Tool = tool,
            SessionRepo = repo,
            Bytes = bytes,
            EstTokens = bytes / 4, // rough estimate: ~4 chars per token
            ElapsedMs = elapsedMs,
            TimestampUtcMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Origin = origin,
        });
    }

    public override Task<Proto.PingResponse> Ping(Proto.PingRequest request, ServerCallContext context)
        => Task.FromResult(new Proto.PingResponse
        {
            Version = DevContext.Core.DevContextVersion.Display,
            Ready = true,
        });

    private AnalysisSession Require(string handle)
    {
        var session = sessions.Get(handle)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Unknown session handle: {handle}"));
        return session;
    }

    // M4 — measured wrappers that time + record every tool call (G1 TokenTotal, G2 elapsedMs)
    private Task<T> WrapT<T>(string handle, Func<AnalysisSession, T> action, [System.Runtime.CompilerServices.CallerMemberName] string tool = "")
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var session = Require(handle);
            var result = action(session);
            sw.Stop();
            CompleteCall(session, result, tool, sw);
            return Task.FromResult(result);
        }
        catch (RpcException) { throw; }
        catch (Exception ex) { throw MapException(ex); }
    }

    private async Task<T> WrapAsyncT<T>(string handle, Func<AnalysisSession, Task<T>> action, [System.Runtime.CompilerServices.CallerMemberName] string tool = "")
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var session = Require(handle);
            var result = await action(session).ConfigureAwait(false);
            sw.Stop();
            CompleteCall(session, result, tool, sw);
            return result;
        }
        catch (RpcException) { throw; }
        catch (Exception ex) { throw MapException(ex); }
    }

    private void CompleteCall<T>(AnalysisSession session, T result, string tool, Stopwatch sw)
    {
        var bytes = result switch
        {
            IMessage msg => msg.CalculateSize(),
            string s => System.Text.Encoding.UTF8.GetByteCount(s),
            _ => 0,
        };
        session.TokenTotal += (bytes / 4) + 1;
        RecordToolCall(tool, session.Handle, session.RepoPath, bytes, sw.ElapsedMilliseconds);
    }

    private static NodeId? ResolveNode(AnalysisSession session, string idOrName)
    {
        var colon = idOrName.IndexOf(':', StringComparison.Ordinal);
        if (colon > 0 && Enum.TryParse<NodeKind>(idOrName[..colon], out var kind))
        {
            var candidate = new NodeId(kind, idOrName[(colon + 1)..]);
            if (session.Query.Graph.Contains(candidate))
                return candidate;
        }
        return session.Query.ResolveNodeId(idOrName);
    }

    private static TraceDetail ParseDetail(string? detail) => detail?.ToLowerInvariant() switch
    {
        "signature" => TraceDetail.Signature,
        "full" => TraceDetail.Full,
        _ => TraceDetail.Salient,
    };

    private static Proto.AnalyzeEvent Error(string code, string message)
        => new() { Error = new Proto.AnalyzeError { Code = code, Message = message } };

    private static RpcException MapException(Exception ex) => ex switch
    {
        AnalysisException ae => new RpcException(new Status(AnalysisCodeToGrpc(ae.Code), ae.Message)),
        InvalidOperationException => new RpcException(new Status(StatusCode.InvalidArgument, ex.Message)),
        ArgumentException => new RpcException(new Status(StatusCode.InvalidArgument, ex.Message)),
        _ => new RpcException(new Status(StatusCode.Internal, ex.Message)),
    };

    private static StatusCode AnalysisCodeToGrpc(string code) => code switch
    {
        "GitNotInstalled" => StatusCode.FailedPrecondition,
        "NotFound" => StatusCode.NotFound,
        "Private" => StatusCode.PermissionDenied,
        "NetworkError" => StatusCode.Unavailable,
        "RateLimited" => StatusCode.ResourceExhausted,
        _ => StatusCode.Internal,
    };

    private sealed class ChannelProgress(ChannelWriter<Proto.AnalyzeEvent> writer) : IProgress<AnalysisProgress>
    {
        public void Report(AnalysisProgress value)
            => writer.TryWrite(new Proto.AnalyzeEvent
            {
                Progress = new Proto.ProgressEvent
                {
                    Stage = value.Stage,
                    Percent = value.Percent,
                    Message = value.Message,
                },
            });
    }
}
