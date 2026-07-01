using System.Threading.Channels;

using DevContext.Server.Mapping;
using DevContext.Server.Sessions;

using Grpc.Core;

using Proto = DevContext.Protos;

namespace DevContext.Server.Endpoints;

public sealed class DevContextGrpcService(
    IAnalysisSessionManager sessions,
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

        var work = RunAnalysisAsync(spec, channel.Writer, request.Path, ct);

        await foreach (var evt in channel.Reader.ReadAllAsync(CancellationToken.None).ConfigureAwait(false))
            await responseStream.WriteAsync(evt).ConfigureAwait(false);

        await work.ConfigureAwait(false);
    }

    private async Task RunAnalysisAsync(
        AnalyzeSpec spec, ChannelWriter<Proto.AnalyzeEvent> writer, string path, CancellationToken ct)
    {
        var progress = new ChannelProgress(writer);
        try
        {
            var session = await sessions.AnalyzeAsync(spec, progress, ct).ConfigureAwait(false);
            var (_, entriesWithTarget) = session.Query.Stats();
            var summary = ProtoMapper.ToSummary(session.Engine, session.Snapshot, entriesWithTarget);
            writer.TryWrite(new Proto.AnalyzeEvent
            {
                Result = new Proto.AnalyzeResult { Handle = session.Handle, Summary = summary },
            });
        }
        catch (OperationCanceledException)
        {
            writer.TryWrite(Error("Cancelled", "Analysis cancelled."));
        }
        catch (AnalysisException ex)
        {
            writer.TryWrite(Error(ex.Code, ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Analysis failed for {Path}", path);
            writer.TryWrite(Error("Internal", ex.Message));
        }
        finally
        {
            writer.Complete();
        }
    }

    public override Task<Proto.CloseResponse> CloseSession(Proto.SessionRequest request, ServerCallContext context)
    {
        var closed = sessions.CloseSession(request.Handle);
        return Task.FromResult(new Proto.CloseResponse { Closed = closed });
    }

    public override Task<Proto.EntryPointsResponse> ListEntryPoints(Proto.SessionRequest request, ServerCallContext context)
        => Wrap(() =>
        {
            var session = Require(request.Handle);
            var resp = new Proto.EntryPointsResponse();
            foreach (var entry in session.Query.EntryPoints())
                resp.EntryPoints.Add(ProtoMapper.ToProto(entry));
            return resp;
        });

    public override async Task<Proto.MapResponse> GetMap(Proto.SessionRequest request, ServerCallContext context)
        => await WrapAsync(async () =>
        {
            var session = Require(request.Handle);
            var markdown = await session.RenderMapMarkdownAsync(context.CancellationToken).ConfigureAwait(false);
            return ProtoMapper.ToMapResponse(session.Snapshot.Map, markdown);
        });

    public override async Task<Proto.TraceResponse> GetTrace(Proto.TraceRequest request, ServerCallContext context)
        => await WrapAsync(async () =>
        {
            var session = Require(request.Handle);
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
        => Wrap(() =>
        {
            var session = Require(request.Handle);
            var id = ResolveNode(session, request.NodeId);
            var detail = id is { } nid ? session.Query.Node(nid) : null;
            return detail is null
                ? new Proto.NodeResponse { Found = false }
                : ProtoMapper.ToNodeResponse(detail);
        });

    public override Task<Proto.NeighborsResponse> GetNeighbors(Proto.NeighborsRequest request, ServerCallContext context)
        => Wrap(() =>
        {
            var session = Require(request.Handle);
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
        => Wrap(() =>
        {
            var session = Require(request.Handle);
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

    public override Task<Proto.StatsResponse> GetStats(Proto.SessionRequest request, ServerCallContext context)
        => Wrap(() =>
        {
            var session = Require(request.Handle);
            var snapshot = session.Snapshot;
            var (seams, entriesWithTarget) = session.Query.Stats();
            return ProtoMapper.ToStatsResponse(
                snapshot.Report,
                snapshot.Graph?.NodeCount ?? 0,
                snapshot.Graph?.EdgeCount ?? 0,
                snapshot.Entries.Length,
                seams,
                entriesWithTarget,
                (long)(snapshot.Report?.TotalWall.TotalMilliseconds ?? session.Engine.ElapsedMs));
        });

    public override async Task<Proto.RenderResponse> Render(Proto.RenderRequest request, ServerCallContext context)
        => await WrapAsync(async () =>
        {
            var session = Require(request.Handle);
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

    public override Task<Proto.PingResponse> Ping(Proto.PingRequest request, ServerCallContext context)
        => Task.FromResult(new Proto.PingResponse
        {
            Version = DevContext.Core.DevContextVersion.Display,
            Ready = true,
        });

    private AnalysisSession Require(string handle)
        => sessions.Get(handle)
           ?? throw new RpcException(new Status(StatusCode.NotFound, $"Unknown session handle: {handle}"));

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

    private static Task<T> Wrap<T>(Func<T> action)
    {
        try { return Task.FromResult(action()); }
        catch (RpcException) { throw; }
        catch (Exception ex) { throw MapException(ex); }
    }

    private static async Task<T> WrapAsync<T>(Func<Task<T>> action)
    {
        try { return await action().ConfigureAwait(false); }
        catch (RpcException) { throw; }
        catch (Exception ex) { throw MapException(ex); }
    }

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
