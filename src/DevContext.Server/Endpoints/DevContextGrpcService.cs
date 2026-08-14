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
            var (session, cached) = await work.ConfigureAwait(false);
            var (_, entriesWithTarget, _, _) = session.Query.Stats();
            var summary = ProtoMapper.ToSummary(session.Engine, session.Snapshot, entriesWithTarget);
            await responseStream.WriteAsync(new Proto.AnalyzeEvent
            {
                Result = new Proto.AnalyzeResult { Handle = session.Handle, Summary = summary, Cached = cached },
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Client disconnected — result not needed
        }
    }

    private async Task<AnalysisOutcome> RunAnalysisWithProgressAsync(
        AnalyzeSpec spec, ChannelWriter<Proto.AnalyzeEvent> writer, string path, CancellationToken ct)
    {
        var progress = new ChannelProgress(writer);
        try
        {
            return await sessions.AnalyzeAsync(spec, progress, ct).ConfigureAwait(false);
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
            // G2.2 (R4 §1 item 12) — an ABSENT budget is now the policy's default, not "unlimited".
            // A caller that names 0 still gets the full tree; what changed is that saying nothing
            // resolves in ONE place instead of once per client. The MCP used to send its own 4000
            // literal on every call, so the server's default was unreachable and the CLI's
            // no-dials trace was a different size from the MCP's for the same focus.
            var budgetTokens = request.HasBudgetTokens
                ? request.BudgetTokens                          // explicit, including 0 = full tree
                : Core.Graph.TracePolicy.DefaultBudgetTokens;

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
            if (ResolveNode(session, request.NodeId) is not { } nid)
                return new Proto.NeighborsResponse();

            // "usages" IS the in-direction (GraphQuery.FindUsages is Neighbors(In) verbatim); reading
            // it as one avoids a second arm that could accept the kind filter differently.
            var direction = request.Direction is "in" or "usages" ? EdgeDirection.In : EdgeDirection.Out;

            // G3.2 — an unparseable kind must not fall through as "no filter", which would return
            // every edge under the caller's filter name. Parse against the enum itself, and when it
            // fails ask for the unfiltered view so the counts stay true while the rows stay empty.
            var requestedKind = request.HasKind && !string.IsNullOrWhiteSpace(request.Kind)
                ? request.Kind.Trim() : null;
            var parsed = Core.Graph.GraphQuery.TryParseEdgeKind(requestedKind, out var edgeKind);
            var unknownKind = requestedKind is not null && !parsed;

            var view = session.Query.NeighborsView(nid, direction, parsed ? edgeKind : null);
            return ProtoMapper.ToNeighborsResponse(view, requestedKind, unknownKind);
        });

    public override Task<Proto.SearchResponse> SearchNodes(Proto.SearchRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var query = request.Query.Trim();
            var limit = request.Limit > 0 ? request.Limit : 20;
            // R4 item 6 — the kind filter and the match count both belong here, above the limit.
            var (ranked, totalMatches) = session.Query.FindPage(
                query, request.HasKind ? request.Kind : null, limit);
            var graph = session.Query.Graph;

            var results = new List<(string Id, string Title, string Kind, ImmutableArray<string> Tags)>();
            foreach (var r in ranked)
            {
                var node = graph.Node(r.Id);
                results.Add((r.Id.ToString(), r.Title, r.Kind.ToString(), node?.Tags ?? ImmutableArray<string>.Empty));
            }

            return ProtoMapper.ToSearchResponse(results, totalMatches);
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

    // G3.1 (R4 item 8) — seam(from, to). Both ends resolve through the SAME ResolveNode every other
    // node-taking RPC uses; an end that does not resolve comes back named, because "OrderService did
    // not resolve" and "nothing connects these two" are answers a caller must be able to tell apart.
    public override Task<Proto.SeamResponse> GetSeam(Proto.SeamRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var maxDepth = request.HasMaxDepth && request.MaxDepth > 0
                ? request.MaxDepth : Core.Graph.GraphQuery.DefaultSeamDepth;
            var maxPaths = request.HasMaxPaths && request.MaxPaths > 0
                ? request.MaxPaths : Core.Graph.GraphQuery.DefaultSeamPaths;

            var fromId = ResolveNode(session, request.From);
            var toId = ResolveNode(session, request.To);
            if (fromId is null || toId is null)
            {
                var unresolved = fromId is null
                    ? (toId is null ? $"'{request.From}' and '{request.To}'" : $"'{request.From}'")
                    : $"'{request.To}'";
                var resp = new Proto.SeamResponse
                {
                    Found = false,
                    Direction = "none",
                    FromNodeId = fromId?.ToString() ?? "",
                    FromTitle = fromId is { } f ? Title(session, f) : "",
                    ToNodeId = toId?.ToString() ?? "",
                    ToTitle = toId is { } t ? Title(session, t) : "",
                    Note = $"{unresolved} did not resolve to a node — that is not the same as no path.",
                };
                return resp;
            }

            var result = session.Query.Seam(fromId.Value, toId.Value, maxDepth, maxPaths);
            return ProtoMapper.ToSeamResponse(result,
                fromId.Value.ToString(), Title(session, fromId.Value),
                toId.Value.ToString(), Title(session, toId.Value), maxDepth);
        });

    private static string Title(AnalysisSession session, NodeId id)
        => session.Query.Graph.Node(id)?.Title ?? id.Key;

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
            var budget = request.HasBudgetTokens ? request.BudgetTokens : ContextPackBuilder.DefaultBudgetTokens;
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
            var budget = request.BudgetTokens > 0 ? request.BudgetTokens : ContextPackBuilder.DefaultBudgetTokens;
            var intent = request.Intent is { Length: > 0 } s ? s : null;

            // N1.1 — exclude_bodies rides the spec: the Studio's eye toggle is a pack filter now,
            // not an icon. Negative sense, so an older client's unset field means "keep bodies".
            var specs = request.Cards.Select(c =>
                new ContextCardSpec(c.Type, c.Title, [.. c.EntryIds], c.ExcludeBodies)).ToList();

            var pack = builder.BuildMulti(specs, budget, intent);
            return ProtoMapper.ToContextPackResponse(pack);
        });

    // N3.2 (STUDIO-MCP §8.3, decision 3) — the repo-file hand-off. The Studio's Save used to hand
    // the pack to the browser's download path, which is why its toast could only name a FILE and
    // never a location: it did not choose one. The pack belongs in the repo it describes, and the
    // repo root is a fact this side holds — the client knows a handle, not a path.
    public override Task<Proto.SavePackFileResponse> SavePackFile(Proto.SavePackFileRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            // The ANALYZED root, not the requested path: it is the root every file:line in the
            // pack is relative to, so a pack that sits beside it addresses the same tree.
            var root = session.Snapshot.RootPath is { Length: > 0 } r ? r : session.RepoRoot;
            var written = PackFileWriter.Write(root, request.Slug, request.Content, request.Format);
            return new Proto.SavePackFileResponse
            {
                Path = written.Path,
                RelativePath = written.RelativePath,
                Gitignored = written.Gitignored,
                AgentLine = written.AgentLine,
            };
        });

    // T4.5 (audit R6) — staleness verification: rebuild the focus's sections (cheap — the graph
    // is immutable since analyze, so the file sets are stable) and compare each section's
    // analyze-time file fingerprints against disk now.
    public override Task<Proto.VerifyContextResponse> VerifyContext(Proto.VerifyContextRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var builder = new ContextPackBuilder(session.Query, session.Snapshot);
            var budget = request.HasBudgetTokens ? request.BudgetTokens : ContextPackBuilder.DefaultBudgetTokens;
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
            // M1.1 — FILE mode: the whole file, capped. It is addressable BY PATH, so it does not
            // need a node at all; a caller that has a file:line (which every provenance now is)
            // can read the file it names without first turning it back into a graph node.
            if (request.Mode == Proto.ReadSourceMode.File && request.HasFilePath)
                return ReadWholeFile(session, request.FilePath, request.MaxLines, nodeTitle: "");

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

            if (request.Mode == Proto.ReadSourceMode.File)
                return ReadWholeFile(session, node.FilePath, request.MaxLines, node.Title);

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

            return new Proto.ReadSourceResponse
            {
                Content = content,
                Language = LanguageOf(node.FilePath),
                FilePath = node.FilePath,
                StartLine = startLine,
                EndLine = endLine,
                NodeTitle = node.Title,
                // M1.1 — MEMBER/WINDOW are windows too; they just never said what they cut.
                TotalLines = lines.Length,
                Truncated = startLine > 1 || endLine < lines.Length,
            };
        });

    /// <summary>M1.1 — the default FILE-mode cap. Big enough that most real C# files come back
    /// whole, small enough that a generated 40k-line file cannot be turned into one gRPC message
    /// by accident. A caller asking for more gets <see cref="MaxFileLinesCeiling"/> at most.</summary>
    internal const int DefaultFileLines = 2000;
    internal const int MaxFileLinesCeiling = 10000;

    /// <summary>M1.1 — FILE mode. Reads a whole file, capped, and SAYS when the cap fired.
    /// Refuses anything outside the analyzed root: this RPC takes a caller-supplied path, so
    /// containment is the difference between "read the repo" and "read the disk".</summary>
    private static Proto.ReadSourceResponse ReadWholeFile(
        AnalysisSession session, string requestedPath, int maxLines, string nodeTitle)
    {
        if (ResolveInRoot(session, requestedPath) is not { } full)
            return new Proto.ReadSourceResponse
            {
                Content = "Refused: path is outside the analyzed root (" + session.Snapshot.RootPath + ").",
                Language = "text",
                FilePath = requestedPath,
            };

        if (!File.Exists(full))
            return new Proto.ReadSourceResponse
            {
                Content = "Source file not found: " + full,
                Language = "text",
                FilePath = full,
            };

        var lines = File.ReadAllLines(full);
        var cap = maxLines > 0 ? Math.Min(maxLines, MaxFileLinesCeiling) : DefaultFileLines;
        var end = Math.Min(lines.Length, cap);

        return new Proto.ReadSourceResponse
        {
            Content = string.Join("\n", lines[..end]),
            Language = LanguageOf(full),
            FilePath = full,
            StartLine = lines.Length == 0 ? 0 : 1,
            EndLine = end,
            NodeTitle = nodeTitle,
            TotalLines = lines.Length,
            Truncated = end < lines.Length,
        };
    }

    /// <summary>M1.1 — per-file edge overlay. GraphQuery could always answer "which edges happen in
    /// this file"; nothing could ASK it, because every browse RPC is node-first. This is the query
    /// the code pane needs to render wiring in the margin — and, standing alone, the answer to
    /// "what does this file actually reach" without first guessing which node to look up.</summary>
    public override Task<Proto.FileOverlayResponse> GetFileOverlay(Proto.FileOverlayRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var full = ResolveInRoot(session, request.FilePath);
            var resp = new Proto.FileOverlayResponse { FilePath = full ?? request.FilePath };
            if (full is null) return resp;

            foreach (var site in session.Query.EdgesInFile(full))
            {
                resp.Sites.Add(new Proto.FileOverlaySite
                {
                    Line = site.Line,
                    Kind = site.Kind.ToString(),
                    Resolution = site.Resolution.ToString(),
                    ToNodeId = site.To.ToString(),
                    ToTitle = site.ToTitle,
                });
                if (site.Line > 0) resp.PlacedSites++; else resp.UnplacedSites++;
            }

            return resp;
        });

    /// <summary>Resolves a caller-supplied path against the analyzed root, or null when it escapes it.
    /// Both file-addressed RPCs go through here: a path parameter is the one place a query API can
    /// turn into "read any file on the box".</summary>
    private static string? ResolveInRoot(AnalysisSession session, string requestedPath)
    {
        if (string.IsNullOrWhiteSpace(requestedPath)) return null;
        var root = session.Snapshot.RootPath;
        var full = Path.IsPathRooted(requestedPath)
            ? Path.GetFullPath(requestedPath)
            : Path.GetFullPath(Path.Combine(root, requestedPath));
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var contained = string.Equals(full, rootFull, StringComparison.OrdinalIgnoreCase)
            || full.StartsWith(rootFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        return contained ? full : null;
    }

    private static string LanguageOf(string path) => Path.GetExtension(path)?.ToLowerInvariant() switch
    {
        ".cs" => "csharp",
        ".razor" => "razor",
        ".cshtml" => "csharp",
        ".xaml" => "xml",
        ".axaml" => "xml",
        _ => "text"
    };

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
                    // R4 item 10 — AgeSeconds above is how long ago this SESSION opened. When the
                    // runner rehydrated a snapshot, that is minutes-to-days younger than the
                    // analysis it serves, and the app's freshness card read the wrong one.
                    FromCache = s.Engine.FromSnapshotCache,
                    AnalyzedAt = ProtoMapper.Iso(s.Engine.AnalyzedAtUtc),
                });
            }
            return Task.FromResult(resp);
        }
        catch (RpcException) { throw; }
        catch (Exception ex) { throw MapException(ex); }
    }

    /// <summary>N0.2 (audit §3.F.9) — the READ half. StartMcp is a mutation and the app used it
    /// as its status probe, so opening the MCP page switched telemetry on and then reported the
    /// state it had just caused. This observes; it never writes.
    ///
    /// N4.1 (audit §4 Room 2) — and it now MEASURES: the binary probe looks on disk, the
    /// last-agent-call figures come from traffic the server actually served. StartMcp/StopMcp
    /// are gone with the global mute they flipped.</summary>
    public override Task<Proto.GetMcpStatusResponse> GetMcpStatus(Proto.GetMcpStatusRequest request, ServerCallContext context)
    {
        var binary = McpBinaryLocator.Probe();
        var response = new Proto.GetMcpStatusResponse
        {
            ObserverCount = mcpObs.ObserverCount,
            McpBinaryFound = binary.Found,
            McpBinaryPath = binary.Path,
            McpBinarySource = binary.Source,
            LastAgentCallAtUtcMs = mcpObs.LastAgentCallAtUtcMs,
            LastAgentTool = mcpObs.LastAgentTool,
            AgentCallCount = mcpObs.AgentCallCount,
        };

        // N4.2 — the setup cards are composed from the SAME probe, so the snippet on screen names
        // the binary this server just found rather than a placeholder the page invented.
        foreach (var target in McpConfigWriter.Targets)
        {
            response.Hosts.Add(new Proto.McpHostConfig
            {
                Id = target.Id,
                Label = target.Label,
                RelativePath = target.RelativePath,
                Snippet = McpConfigWriter.SnippetFor(target, binary),
            });
        }

        return Task.FromResult(response);
    }

    /// <summary>N4.1 — spawn the resolved devcontext-mcp binary and run one real
    /// initialize + tools/list round trip over stdio. The only check on this page that proves a
    /// host config would work.</summary>
    public override async Task<Proto.McpHandshakeResponse> McpHandshake(
        Proto.McpHandshakeRequest request, ServerCallContext context)
    {
        var result = await McpHandshakeProbe
            .RunAsync(McpBinaryLocator.Probe(), TimeSpan.FromSeconds(30), context.CancellationToken)
            .ConfigureAwait(false);

        var response = new Proto.McpHandshakeResponse
        {
            Ok = result.Ok,
            Command = result.Command,
            ServerName = result.ServerName,
            ServerVersion = result.ServerVersion,
            ProtocolVersion = result.ProtocolVersion,
            ToolCount = result.ToolNames.Count,
            ElapsedMs = result.ElapsedMs,
            Error = result.Error,
        };
        response.ToolNames.AddRange(result.ToolNames);
        return response;
    }

    /// <summary>N4.2 (audit §4 Room 2, "setup that works") — write the host's MCP config instead
    /// of handing over a snippet with a placeholder in it. Project-scoped, merged into whatever
    /// is already there; see <see cref="McpConfigWriter"/> for why it is not the user-global
    /// file. The command written is the same probe result the status card renders.</summary>
    public override Task<Proto.WriteMcpConfigResponse> WriteMcpConfig(
        Proto.WriteMcpConfigRequest request, ServerCallContext context)
        => WrapT(request.Handle, session =>
        {
            var target = McpConfigWriter.TargetFor(request.Host)
                ?? throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    $"Unknown MCP host '{request.Host}' — known hosts: {string.Join(", ", McpConfigWriter.Targets.Select(t => t.Id))}"));

            // The ANALYZED root, exactly as SavePackFile picks it (N3.2): the config points an
            // agent at the tree the session's file:line references belong to.
            var root = session.Snapshot.RootPath is { Length: > 0 } r ? r : session.RepoRoot;
            var written = McpConfigWriter.Write(root, target, McpBinaryLocator.Probe());
            return new Proto.WriteMcpConfigResponse
            {
                Path = written.Path,
                RelativePath = written.RelativePath,
                Action = written.Action,
                Command = written.Command,
            };
        });

    public override async Task ObserveToolCalls(
        Proto.ObserveToolCallsRequest request,
        IServerStreamWriter<Proto.ToolCallEvent> responseStream,
        ServerCallContext context)
    {
        var ct = context.CancellationToken;
        var channel = Channel.CreateUnbounded<Proto.ToolCallEvent>();

        var observerId = Guid.NewGuid().ToString("N");
        using var _ = mcpObs.Subscribe(observerId, channel.Writer);

        // N4.1 — subscribing IS the start. This used to also flip the global mute on, which is
        // why the page could report "active" the moment anyone watched.
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
        // T6.10/F5 — ui vs agent, stashed in Items by Program.cs's middleware. N4.1: from the
        // client's own headers, because both the app and the MCP sidecar arrive as gRPC-web.
        var request = httpContext.HttpContext?.Request;
        var origin = httpContext.HttpContext?.Items[OriginTag.ItemKey] as string
            ?? OriginTag.FromRequest(request?.Headers.UserAgent, request?.Headers["x-user-agent"], request?.Headers.Origin);
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
