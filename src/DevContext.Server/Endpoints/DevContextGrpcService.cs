using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Channels;

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
            request.HasCleanup ? request.Cleanup : null);

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
            var (_, entriesWithTarget) = session.Query.Stats();
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
                resp.EntryPoints.Add(ProtoMapper.ToProto(entry));
            return resp;
        });

    public override Task<Proto.MapResponse> GetMap(Proto.SessionRequest request, ServerCallContext context)
        => WrapAsyncT(request.Handle, async session =>
        {
            var markdown = await session.RenderMapMarkdownAsync(context.CancellationToken).ConfigureAwait(false);
            return ProtoMapper.ToMapResponse(session.Snapshot.Map, markdown);
        });

    public override Task<Proto.TraceResponse> GetTrace(Proto.TraceRequest request, ServerCallContext context)
        => WrapAsyncT(request.Handle, async session =>
        {
            var depth = request.HasDepth ? request.Depth : 6;
            var detail = ParseDetail(request.HasDetail ? request.Detail : null);

            var trace = session.Query.Trace(request.Focus, depth);
            if (trace is null)
                return new Proto.TraceResponse { Found = false };

            var markdown = await session.RenderTraceMarkdownAsync(request.Focus, depth, detail, context.CancellationToken)
                .ConfigureAwait(false);
            return ProtoMapper.ToTraceResponse(trace, markdown);
        });

    public override Task<Proto.NodeResponse> GetNode(Proto.NodeRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var id = ResolveNode(session, request.NodeId);
            var detail = id is { } nid ? session.Query.Node(nid) : null;
            return detail is null
                ? new Proto.NodeResponse { Found = false }
                : ProtoMapper.ToNodeResponse(detail);
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
            var graph = session.Query.Graph;

            var results = new List<(string Id, string Title, string Kind, ImmutableArray<string> Tags)>();

            foreach (var node in graph.Nodes)
            {
                if (results.Count >= limit) break;
                if (string.IsNullOrEmpty(query)
                    || node.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || node.Id.ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add((node.Id.ToString(), node.Title, node.Kind.ToString(), node.Tags));
                }
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

    // M4.7 — config key lookup: scan source files for IConfiguration/GetValue/GetSection usage
    private static readonly Regex ConfigKeyRegex = new(
        @"(?:\bIConfiguration\b|\bConfiguration\b|(?<!\w)(?:_config|_configuration|_cfg|_conf|_c)\b|(?<!\w)(?:cfg|conf)\b(?=\s*\.\s*\[))\s*(?:\[""([^""]+)""\]|\.GetValue<[^>]+>\(""([^""]+)"")|\.GetSection\(""([^""]+)"")|\.GetConnectionString\(""([^""]+)"")|\.GetRequiredSection\(""([^""]+)"")",
        RegexOptions.Compiled);

    public override Task<Proto.ConfigResponse> ConfigLookup(Proto.ConfigRequest request, ServerCallContext context)
        => WrapAsyncT(request.Handle, async session =>
        {
            var resp = new Proto.ConfigResponse();
            var keyFilter = request.HasKey ? request.Key : null;
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var filesByPath = session.Query.Graph.Nodes
                .Where(n => n.FilePath is not null)
                .GroupBy(n => n.FilePath!)
                .ToDictionary(g => g.Key, g => g.AsEnumerable(), StringComparer.OrdinalIgnoreCase);

            foreach (var (filePath, nodes) in filesByPath)
            {
                if (!File.Exists(filePath)) continue;
                string[] lines;
                try { lines = await File.ReadAllLinesAsync(filePath).ConfigureAwait(false); }
                catch { continue; }

                var service = nodes.FirstOrDefault()?.Project ?? "";
                var nodesInFile = nodes.ToDictionary(n => n.Id.Key, StringComparer.OrdinalIgnoreCase);

                for (var i = 0; i < lines.Length; i++)
                {
                    var matches = ConfigKeyRegex.Matches(lines[i]);
                    foreach (Match m in matches)
                    {
                        string? key = null;
                        string patternType = "Indexer";
                        for (var g = 1; g <= 5; g++)
                        {
                            if (m.Groups[g].Success)
                            {
                                key = m.Groups[g].Value;
                                patternType = g switch { 1 => "Indexer", 2 => "GetValue", 3 => "GetSection", 4 => "GetConnectionString", 5 => "GetRequiredSection", _ => "Indexer" };
                                break;
                            }
                        }
                        if (key is null) continue;
                        if (keyFilter is not null && !string.Equals(key, keyFilter, StringComparison.OrdinalIgnoreCase)) continue;

                        seenKeys.Add(key);

                        string? nodeId = null;
                        foreach (var (nodeKey, node) in nodesInFile)
                        {
                            if (node.LineNumber is { } ln && ln == i + 1)
                            {
                                nodeId = node.Id.ToString();
                                break;
                            }
                        }

                        resp.Bindings.Add(new Proto.ConfigBinding
                        {
                            Key = key,
                            FilePath = filePath,
                            LineNumber = i + 1,
                            NodeId = nodeId ?? "",
                            PatternType = patternType,
                            Service = service,
                        });
                    }
                }
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
                if (!IsLikelyTestMethod(title, filePath, project)) continue;

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
            var (seams, entriesWithTarget) = session.Query.Stats();
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
                snapshot.Entries);
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
        => Task.FromResult(new Proto.StartMcpResponse { Running = true });

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
        mcpObs.Notify(new Proto.ToolCallEvent
        {
            SessionHandle = handle,
            Tool = tool,
            SessionRepo = repo,
            Bytes = bytes,
            EstTokens = bytes / 4, // rough estimate: ~4 chars per token
            ElapsedMs = elapsedMs,
            TimestampUtcMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
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

    // M4.9 — heuristic test method detection by name, path, and project
    private static bool IsLikelyTestMethod(string title, string? filePath, string? project)
    {
        if (string.IsNullOrEmpty(title)) return false;

        var lower = title.ToLowerInvariant();

        if (lower.EndsWith("_test") || lower.EndsWith("_should") || lower.EndsWith("_when")
            || title.StartsWith("Test") || title.StartsWith("Should")
            || title.Contains("_Tests_") || title.Contains(".Tests."))
            return true;

        if (filePath is not null)
        {
            var fp = filePath.Replace('\\', '/').ToLowerInvariant();
            if (fp.Contains("/test/") || fp.Contains("/tests/")) return true;
        }

        if (project is not null)
        {
            var p = project.ToLowerInvariant();
            if (p.EndsWith("tests") || p.EndsWith("test") || p.EndsWith("specs")
                || p.Contains(".tests.") || p.Contains(".test."))
                return true;
        }

        return false;
    }

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
