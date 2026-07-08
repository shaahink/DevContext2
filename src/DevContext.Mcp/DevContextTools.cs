using System.Collections.Concurrent;
using System.Text;
using DevContext.Protos;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevContext.Mcp;

[McpServerToolType]
public sealed class DevContextTools
{
    private readonly DevContextService.DevContextServiceClient _client;
    private readonly ILogger<DevContextTools> _logger;
    private readonly ConcurrentDictionary<string, string> _repoRoots = new(StringComparer.Ordinal); // G4 — handle→repoRoot

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public DevContextTools(DevContextService.DevContextServiceClient client, ILogger<DevContextTools> logger)
    {
        _client = client;
        _logger = logger;
    }

    private string ResolveHandle(string? handle)
    {
        if (!string.IsNullOrEmpty(handle)) return handle;

        var list = _client.ListSessions(new ListSessionsRequest());
        if (list.Sessions.Count == 0)
            throw new RpcException(new Status(StatusCode.FailedPrecondition,
                "No active session. Run analyze first. Example: analyze(\"C:/repos/MyApp\")"));

        return list.Sessions[0].Handle;
    }

    // L5.2 — error envelope: every tool failure returns {error, hint, example}.
    // Budget: error+hint+example <= 80 tokens. Kept terse by construction (short strings,
    // WhenWritingNull drops absent fields). Candidates (did-you-mean) attach for unknown symbols.
    private static string Envelope(string error, string hint, string example, object? candidates = null)
        => JsonSerializer.Serialize(new { error, hint, example, candidates }, JsonOpts);

    // Map an RpcException from the server into an actionable envelope for the given tool.
    private static string FromRpc(RpcException ex, string tool, string example)
    {
        var msg = ex.Status.Detail;
        var hint = ex.StatusCode switch
        {
            StatusCode.FailedPrecondition => "Run analyze(path) first, then retry.",
            StatusCode.NotFound => "Handle expired or unknown — analyze again or check list_sessions().",
            StatusCode.InvalidArgument => "Check the argument values against the tool schema.",
            _ => "Retry; if it persists the session may need re-analysis.",
        };
        if (string.IsNullOrWhiteSpace(msg)) msg = $"{tool} failed ({ex.StatusCode}).";
        return Envelope(msg, hint, example);
    }

    // Did-you-mean candidates for an unresolved symbol: top ranked title matches.
    private async Task<object[]> SuggestAsync(string handle, string query, int take = 5)
    {
        try
        {
            var nodes = await _client.SearchNodesAsync(new SearchRequest
            {
                Handle = handle,
                Query = FirstWord(query),
                Limit = take,
            });
            return nodes.Nodes.Take(take)
                .Select(n => (object)new { nodeId = n.NodeId, title = TrimTitle(n.Title), kind = n.Kind })
                .ToArray();
        }
        catch { return Array.Empty<object>(); }
    }

    // Search by the most-selective token so an exact-substring miss still yields candidates
    // (e.g. "how does checkout work" → "checkout").
    private static string FirstWord(string s)
    {
        var parts = s.Split(new[] { ' ', '\t', '/', '.', ':' }, StringSplitOptions.RemoveEmptyEntries);
        // longest token is usually the most distinctive symbol-ish word
        var best = "";
        foreach (var p in parts)
            if (p.Length > best.Length && !IsStopWord(p)) best = p;
        return best.Length > 0 ? best : s.Trim();
    }

    private static bool IsStopWord(string w) => w.ToLowerInvariant() is
        "how" or "does" or "the" or "what" or "is" or "are" or "work" or "works"
        or "a" or "an" or "of" or "for" or "get" or "post" or "put" or "delete" or "to" or "in";

    /// <summary>Start analysis of a .NET repo. Returns a handle for subsequent calls. Idempotent: same repo+HEAD returns existing handle. Example: analyze("C:/repos/MyApp")</summary>
    [McpServerTool]
    public async Task<string> Analyze(string path)
    {
        var req = new AnalyzeRequest { Path = path };
        var call = _client.Analyze(req);
        string? handle = null;

        await foreach (var evt in call.ResponseStream.ReadAllAsync())
        {
            if (evt.EventCase == AnalyzeEvent.EventOneofCase.Result)
            {
                handle = evt.Result.Handle;
            }
        }

        if (handle is null)
            throw new RpcException(new Status(StatusCode.Internal, "Analysis did not return a handle."));

        try
        {
            var resolved = Path.GetFullPath(path);
            if (Directory.Exists(resolved))
                _repoRoots[handle] = resolved.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            else if (File.Exists(resolved))
                _repoRoots[handle] = Path.GetDirectoryName(resolved) ?? resolved;
            else
                _repoRoots[handle] = resolved;
        }
        catch { _repoRoots[handle] = path; }

        return JsonSerializer.Serialize(new
        {
            handle,
            status = "ready",
            hint = "Analysis complete. Other tools will use this session by default. Omit 'handle' to use the most recent session.",
        }, JsonOpts);
    }

    /// <summary>One-call repo brief: identity, services, ServiceLinks, top flows, where to start. ~600 tokens max. Example: overview("abc123")</summary>
    [McpServerTool]
    public async Task<string> Overview(string? handle = null)
    {
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "overview", "analyze(\"C:/repos/MyApp\") then overview()"); }
        try { return await OverviewCore(handle); }
        catch (RpcException ex) { return FromRpc(ex, "overview", "overview(handle)"); }
    }

    private async Task<string> OverviewCore(string handle)
    {
        // L4.3: services + top flows come from the graph projections (one truth), not ad-hoc walks.
        var facets = await _client.GetGraphFacetsAsync(new GraphFacetsRequest { Handle = handle, MaxFlows = 5 });
        var map = await _client.GetMapAsync(new SessionRequest { Handle = handle });
        var stats = await _client.GetStatsAsync(new SessionRequest { Handle = handle });

        var sb = new StringBuilder();

        sb.Append(map.Archetype);
        var services = facets.ServiceMap?.Services ?? new();
        if (services.Count > 0)
        {
            // DisplayName carries the FULL runnable name (Basket.API) — no client-side truncation,
            // and libraries are already excluded (Service nodes only).
            var svcLabels = services.Select(s => s.Kind is { Length: > 0 } && s.Kind != "Service"
                ? $"{s.DisplayName} ({s.Kind})"
                : s.DisplayName);
            sb.Append(": ").AppendJoin(", ", svcLabels);
            sb.AppendLine();
        }
        sb.Append("  ").Append(stats.Graph?.Nodes ?? 0).Append(" nodes · ");
        sb.Append(stats.Graph?.Edges ?? 0).Append(" edges · ");
        sb.Append(stats.Graph?.Entries ?? 0).Append(" entries · ");
        sb.Append(map.ProjectCount).AppendLine(" projects");

        var top = facets.FlowList?.Flows ?? new();
        if (top.Count > 0)
        {
            sb.AppendLine("Top flows:");
            foreach (var f in top)
            {
                var label = f.HasRoute && f.HasHttpMethod ? $"{f.HttpMethod} {f.Route}" : f.Title;
                sb.Append("  ").Append(f.Kind).Append(": ").Append(label);
                if (f.HasTarget) sb.Append(" → ").Append(f.Target);
                sb.AppendLine();
            }
        }

        var transports = facets.ServiceMap?.Transports ?? new();
        if (transports.Count > 0)
        {
            sb.AppendLine("Service links:");
            foreach (var t in transports.Take(8))
                sb.Append("  ").Append(t.FromService).Append(" → ").Append(t.ToService)
                  .Append(" (").Append(t.Transport).Append(')').AppendLine();
        }

        var pts = await _client.GetInterestingPointsAsync(new InterestingPointsRequest
        {
            Handle = handle,
            Archetype = map.Archetype,
        });
        if (pts.Points.Count > 0)
        {
            sb.Append("Start here: ");
            sb.AppendJoin(", ", pts.Points.Take(4).Select(p => p.Title));
            sb.AppendLine();
        }

        if (map.PipelineBehaviors.Count > 0)
            sb.Append("Behaviors: ").AppendJoin(", ", map.PipelineBehaviors).AppendLine();

        if (map.Stack.Count > 0)
            sb.Append("Stack: ").AppendJoin(", ", map.Stack.Take(10)).AppendLine();

        var text = sb.ToString();
        var tokens = (text.Length + 3) / 4;

        return JsonSerializer.Serialize(new { handle, tokens, text }, JsonOpts);
    }

    /// <summary>Resolve a symbol/route/file to candidates with kind, service, path. Never silently picks — returns all matches. Example: resolve("abc123", "Product")</summary>
    [McpServerTool]
    public async Task<string> Resolve(string? handle = null, string? query = null, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Envelope("Missing required parameter 'query'.",
                "Pass the symbol/route/file to resolve.", "resolve(handle, query:\"Order\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "resolve", "analyze(path) then resolve(query:\"Order\")"); }
        try { return await ResolveCore(handle, query, limit); }
        catch (RpcException ex) { return FromRpc(ex, "resolve", "resolve(handle, query:\"Order\")"); }
    }

    private async Task<string> ResolveCore(string handle, string query, int limit)
    {
        var nodes = await _client.SearchNodesAsync(new SearchRequest
        {
            Handle = handle,
            Query = query,
            Limit = limit,
        });

        var result = new List<object>();
        foreach (var n in nodes.Nodes)
        {
            NodeResponse? detail = null;
            try
            {
                detail = await _client.GetNodeAsync(new NodeRequest { Handle = handle, NodeId = n.NodeId });
            }
            catch { /* node might not be resolvable */ }

            result.Add(new
            {
                nodeId = n.NodeId,
                title = n.Title,
                kind = n.Kind,
                filePath = detail is { HasFilePath: true } ? Rel(handle, detail.FilePath) : null,
                lineNumber = detail is { HasLineNumber: true } ? (int?)detail.LineNumber : null,
                outDegree = detail?.OutDegree ?? 0,
                inDegree = detail?.InDegree ?? 0,
                tags = n.Tags.ToArray(),
            });
        }

        // L5.2 — unknown symbol returns candidates ("did you mean"), never zero-shaped success.
        if (result.Count == 0)
        {
            var suggestions = await SuggestAsync(handle, query);
            return Envelope($"No match for '{query}'.",
                suggestions.Length > 0 ? "Did you mean one of these?" : "Try a broader term or find(query).",
                "resolve(handle, query:\"Order\")",
                suggestions.Length > 0 ? suggestions : null);
        }

        return JsonSerializer.Serialize(new
        {
            query,
            count = result.Count,
            ambiguous = result.Count > 1,
            candidates = result,
            hint = result.Count > 1 ? "Multiple matches — provide a specific nodeId" : null,
        }, JsonOpts);
    }

    /// <summary>Check if a session handle is still valid. Example: status("abc123")</summary>
    [McpServerTool]
    public async Task<string> Status(string? handle = null)
    {
        handle = ResolveHandle(handle);
        try
        {
            await _client.GetStatsAsync(new SessionRequest { Handle = handle });
            return JsonSerializer.Serialize(new { handle, status = "ready" }, JsonOpts);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return JsonSerializer.Serialize(new { found = false, handle, error = "Unknown handle." }, JsonOpts);
        }
    }

    /// <summary>Release a session's resources (engine + any clone). Idempotent. Example: close_session("abc123")</summary>
    [McpServerTool]
    public async Task<string> CloseSession(string? handle = null)
    {
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "close_session", "close_session(handle)"); }
        var resp = await _client.CloseSessionAsync(new SessionRequest { Handle = handle });
        return JsonSerializer.Serialize(new { closed = resp.Closed, handle }, JsonOpts);
    }

    /// <summary>List all active analysis sessions on the server. Example: list_sessions()</summary>
    [McpServerTool]
    public async Task<string> ListSessions()
    {
        var resp = await _client.ListSessionsAsync(new ListSessionsRequest());
        return JsonSerializer.Serialize(new
        {
            count = resp.Sessions.Count,
            sessions = resp.Sessions.Select(s => new
            {
                handle = s.Handle,
                repo = s.Repo,
                ageSeconds = s.AgeSeconds,
                calls = s.Calls,
                nodes = s.Nodes,
                edges = s.Edges,
                entries = s.Entries,
            }).ToArray(),
        }, JsonOpts);
    }

    /// <summary>Full analysis stats: node/edge counts, seam breakdown, insights, warnings. Example: stats("abc123")</summary>
    [McpServerTool]
    public async Task<string> Stats(string? handle = null)
    {
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "stats", "analyze(path) then stats()"); }
        try
        {
        var resp = await _client.GetStatsAsync(new SessionRequest { Handle = handle });
        return JsonSerializer.Serialize(new
        {
            meta = $"nodes={resp.Graph?.Nodes ?? 0} edges={resp.Graph?.Edges ?? 0}",
            nodeCount = resp.Graph?.Nodes ?? 0,
            edgeCount = resp.Graph?.Edges ?? 0,
            entryCount = resp.Graph?.Entries ?? 0,
            entriesWithTarget = resp.Graph?.EntriesWithTarget ?? 0,
            totalWallMs = resp.TotalWallMs,
            seams = resp.Seams.Select(seam => new
            {
                kind = seam.Seam,
                total = seam.Count,
                verified = seam.Count - seam.Approx,
                approx = seam.Approx,
            }).ToArray(),
            insights = resp.Insights.Select(i => new
            {
                id = i.Id,
                category = i.Category,
                severity = i.Severity,
                title = i.Title,
                detail = i.Detail,
                evidence = i.Evidence.ToArray(),
                action = i.Action,
                actionTarget = i.ActionTarget,
            }).ToArray(),
            warnings = resp.Insights
                .Where(i => i.Severity == "WARNING")
                .Select(i => i.Title)
                .ToArray(),
        }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "stats", "stats(handle)"); }
    }

    /// <summary>List entry points (HTTP routes, bus consumers, gRPC services). Filter by kind. Example: entrypoints("abc123", "HttpEndpoint")</summary>
    [McpServerTool]
    public async Task<string> Entrypoints(string? handle = null, string? kind = null)
    {
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "entrypoints", "analyze(path) then entrypoints()"); }
        var resp = await _client.ListEntryPointsAsync(new SessionRequest { Handle = handle });
        var entries = resp.EntryPoints.AsEnumerable();
        if (kind is not null)
            entries = entries.Where(e => string.Equals(e.Kind, kind, StringComparison.OrdinalIgnoreCase));

        return JsonSerializer.Serialize(new
        {
            count = resp.EntryPoints.Count,
            kind,
            entries = entries.Select(e => new
            {
                kind = e.Kind,
                title = e.Title,
                nodeId = e.NodeId,
                httpMethod = e.HasHttpMethod ? e.HttpMethod : null,
                route = e.HasRoute ? e.Route : null,
                project = e.HasProject ? e.Project : null,
                target = e.HasTarget ? e.Target : null,
                groupPath = e.HasGroupPath ? e.GroupPath : null,
                authAttributes = e.AuthAttributes.ToArray(),
                score = e.Score,
                reach = e.Reach,
                crossProjects = e.CrossProjects,
            }).ToArray(),
        }, JsonOpts);
    }

    /// <summary>Architecture map: style, archetype, topology, project dependencies. Example: map("abc123")</summary>
    [McpServerTool]
    public async Task<string> Map(string? handle = null)
    {
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "map", "analyze(path) then map()"); }
        var resp = await _client.GetMapAsync(new SessionRequest { Handle = handle });
        return JsonSerializer.Serialize(new
        {
            meta = $"style={resp.Style} archetype={resp.Archetype} projects={resp.ProjectCount}",
            archetype = resp.Archetype,
            style = resp.Style,
            styleConfidence = resp.StyleConfidence,
            projectCount = resp.ProjectCount,
            topology = resp.Topology.Select(t => new { name = t.Name, dependsOn = t.DependsOn.ToArray() }).ToArray(),
        }, JsonOpts);
    }

    /// <summary>Top 20 entry points ranked by importance score. Example: top_flows("abc123")</summary>
    [McpServerTool]
    public async Task<string> TopFlows(string? handle = null)
    {
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "top_flows", "analyze(path) then top_flows()"); }
        // L4.3: ranking + shaping done once by FlowListProjection (server-side), not client-side here.
        var resp = await _client.GetGraphFacetsAsync(new GraphFacetsRequest { Handle = handle, MaxFlows = 20 });
        var flows = resp.FlowList?.Flows ?? new();
        return JsonSerializer.Serialize(new
        {
            count = flows.Count,
            topFlows = flows.Select(f => new
            {
                kind = f.Kind,
                title = f.Title,
                nodeId = f.HasNodeId ? f.NodeId : null,
                route = f.HasRoute ? f.Route : null,
                httpMethod = f.HasHttpMethod ? f.HttpMethod : null,
                target = f.HasTarget ? f.Target : null,
                groupPath = f.HasGroupPath ? f.GroupPath : null,
                score = f.Score,
                depth = f.Depth,
                hops = f.Hops,
            }).ToArray(),
        }, JsonOpts);
    }

    /// <summary>Archetype-aware starting points for exploring the codebase. Example: interesting_points("abc123")</summary>
    [McpServerTool]
    public async Task<string> InterestingPoints(string? handle = null)
    {
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "interesting_points", "analyze(path) then interesting_points()"); }
        var map = await _client.GetMapAsync(new SessionRequest { Handle = handle });
        var pts = await _client.GetInterestingPointsAsync(new InterestingPointsRequest
        {
            Handle = handle,
            Archetype = map.Archetype,
        });
        return JsonSerializer.Serialize(new
        {
            archetype = map.Archetype,
            count = pts.Points.Count,
            points = pts.Points.Select(p => new
            {
                nodeId = p.NodeId,
                title = p.Title,
                kind = p.Kind,
                why = p.Why,
                tags = p.Tags.ToArray(),
            }).ToArray(),
        }, JsonOpts);
    }

    /// <summary>Trace execution flow. format: default|compact. compact gives indented text with file:line. Example: trace("abc123", "POST /api/orders", 6, "compact")</summary>
    [McpServerTool]
    public async Task<string> Trace(string? handle = null, string? focus = null, int depth = 6, string format = "default")
    {
        if (string.IsNullOrWhiteSpace(focus))
            return Envelope("Missing required parameter 'focus'.",
                "Pass an entry route or symbol. The parameter is 'focus', not 'route'.",
                "trace(handle, focus:\"POST /basket/checkout\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "trace", "analyze(path) then trace(focus:\"POST /basket/checkout\")"); }
        try
        {
            var resp = await _client.GetTraceAsync(new TraceRequest
            {
                Handle = handle,
                Focus = focus,
                Depth = depth,
            });
            if (!resp.Found)
            {
                // L5.2 — brittle focus resolution now teaches: suggest real entries/nodes.
                var suggestions = await SuggestAsync(handle, focus);
                return Envelope($"No entry or node matched '{focus}'.",
                    suggestions.Length > 0 ? "Did you mean one of these? Use an exact route or nodeId." : "Try top_flows() to list traceable entries.",
                    "trace(handle, focus:\"POST /basket/checkout\")",
                    suggestions.Length > 0 ? suggestions : null);
            }

            if (format == "compact")
            {
                // M4.3 — compact flow: indented text with file:line per step, seam glyphs
                var sb = new StringBuilder();
                var entryInfo = resp.Root is not null
                    ? $"{resp.Root.Kind}: {resp.Root.Title}" : focus;
                sb.Append("Entry: ").AppendLine(entryInfo);
                if (resp.Root is not null)
                    BuildCompactFlow(sb, resp.Root, 0, handle);
                if (resp.TouchedEntities.Count > 0)
                    sb.Append("Touches: ").AppendJoin(", ", resp.TouchedEntities).AppendLine();
                if (resp.EmittedEvents.Count > 0)
                    sb.Append("Emits: ").AppendJoin(", ", resp.EmittedEvents).AppendLine();

                var compact = sb.ToString();
                var tokens = (compact.Length + 3) / 4;
                return JsonSerializer.Serialize(new { found = true, format = "compact", tokens, text = compact }, JsonOpts);
            }

            return JsonSerializer.Serialize(new
            {
                found = true,
                entry = resp.Root is not null ? new
                {
                    nodeId = resp.Root.NodeId,
                    title = resp.Root.Title,
                    kind = resp.Root.Kind,
                } : null,
                root = resp.Root is not null ? SerializeTraceNode(resp.Root) : null,
                touchedEntities = resp.TouchedEntities.ToArray(),
                emittedEvents = resp.EmittedEvents.ToArray(),
            }, JsonOpts);
        }
        catch (RpcException ex)
        {
            return FromRpc(ex, "trace", "trace(handle, focus:\"POST /basket/checkout\")");
        }
    }

    // M4.3 — compact flow: indented text with seam glyphs and provenance
    private void BuildCompactFlow(StringBuilder sb, TraceNode node, int indent, string handle)
    {
        var prefix = new string(' ', indent * 2);
        var seamGlyph = node.Seam switch
        {
            "Entry" => "▼",
            "Call" => "→",
            "Sends" => "⇒",
            "Raises" => "⬆",
            "Consumes" => "↓",
            "Handles" => "◉",
            "CrossService" => "⇛",
            _ => "·",
        };
        sb.Append(prefix).Append(seamGlyph).Append(' ').Append(node.Kind).Append(": ").Append(node.Title);
        if (node.HasSalient && node.Salient.Length > 0)
            sb.Append(" (").Append(node.Salient).Append(')');
        if (node.Resolution == "Syntactic")
            sb.Append(" [approx]");
        if (node.Truncated)
            sb.Append($" ({node.Omitted} omitted)");
        if (node.HasProvenance && !string.IsNullOrEmpty(node.Provenance))
            sb.Append("  ").Append(Rel(handle, node.Provenance));
        sb.AppendLine();

        foreach (var child in node.Children)
            BuildCompactFlow(sb, child, indent + 1, handle);
    }

    private static object? SerializeTraceNode(TraceNode node) => new
    {
        nodeId = node.NodeId,
        title = node.Title,
        kind = node.Kind,
        seam = node.Seam,
        depth = node.Depth,
        resolution = node.Resolution,
        truncated = node.Truncated,
        omitted = node.Omitted,
        salient = node.HasSalient ? node.Salient : null,
        tags = node.Tags.ToArray(),
        pipeline = node.Pipeline.ToArray(),
        children = node.Children.Select(SerializeTraceNode).ToArray(),
    };

    /// <summary>Detail card for a graph node: title, kind, file path, degrees. Example: node("abc123", "OrderService")</summary>
    [McpServerTool]
    public async Task<string> Node(string? handle = null, string? nodeId = null)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return Envelope("Missing required parameter 'nodeId'.",
                "Pass a nodeId (or short name).", "node(handle, nodeId:\"OrderService\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "node", "analyze(path) then node(nodeId:\"OrderService\")"); }
        try
        {
            var resp = await _client.GetNodeAsync(new NodeRequest { Handle = handle, NodeId = nodeId });
            if (!resp.Found)
            {
                var suggestions = await SuggestAsync(handle, nodeId);
                return Envelope($"Node '{nodeId}' not found.",
                    suggestions.Length > 0 ? "Did you mean one of these?" : "Use resolve(query) to find the nodeId.",
                    "node(handle, nodeId:\"OrderService\")",
                    suggestions.Length > 0 ? suggestions : null);
            }

            return JsonSerializer.Serialize(new
            {
                found = true,
                nodeId = resp.NodeId,
                title = resp.Title,
                kind = resp.Kind,
                tags = resp.Tags.ToArray(),
                filePath = Rel(handle, resp.HasFilePath ? resp.FilePath : null),
                lineNumber = resp.HasLineNumber ? (int?)resp.LineNumber : null,
                outDegree = resp.OutDegree,
                inDegree = resp.InDegree,
            }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "node", "node(handle, nodeId:\"OrderService\")"); }
    }

    /// <summary>Outgoing/incoming edges of a node. direction: out|in|usages. Example: neighbors("abc123", "OrderService", "out")</summary>
    [McpServerTool]
    public async Task<string> Neighbors(string? handle = null, string? nodeId = null, string direction = "out")
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return Envelope("Missing required parameter 'nodeId'.",
                "Pass a nodeId and direction (out|in|usages).",
                "neighbors(handle, nodeId:\"OrderService\", direction:\"out\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "neighbors", "analyze(path) then neighbors(nodeId:\"OrderService\")"); }
        try
        {
            var resp = await _client.GetNeighborsAsync(new NeighborsRequest
            {
                Handle = handle,
                NodeId = nodeId,
                Direction = direction,
            });
            if (resp.Edges.Count == 0 && !NodeExists(handle, nodeId))
            {
                var suggestions = await SuggestAsync(handle, nodeId);
                return Envelope($"Node '{nodeId}' not found.",
                    suggestions.Length > 0 ? "Did you mean one of these?" : "Use resolve(query) to find the nodeId.",
                    "neighbors(handle, nodeId:\"OrderService\", direction:\"out\")",
                    suggestions.Length > 0 ? suggestions : null);
            }
            return JsonSerializer.Serialize(new
            {
                nodeId,
                direction,
                count = resp.Edges.Count,
                edges = resp.Edges.Select(e => new
                {
                    from = e.From,
                    to = e.To,
                    kind = e.Kind,
                    resolution = e.Resolution,
                    otherTitle = e.OtherTitle,
                    provenance = Rel(handle, e.HasProvenance ? e.Provenance : null),
                }).ToArray(),
            }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "neighbors", "neighbors(handle, nodeId:\"OrderService\", direction:\"out\")"); }
    }

    /// <summary>Find all usages (in-edges) of a node across the codebase. Example: usages("abc123", "IOrderRepository")</summary>
    [McpServerTool]
    public async Task<string> Usages(string? handle = null, string? nodeId = null)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return Envelope("Missing required parameter 'nodeId'.",
                "Pass a nodeId (or short name) to find usages of.",
                "usages(handle, nodeId:\"IOrderRepository\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "usages", "analyze(path) then usages(nodeId:\"IOrderRepository\")"); }
        try
        {
            var resp = await _client.GetNeighborsAsync(new NeighborsRequest
            {
                Handle = handle,
                NodeId = nodeId,
                Direction = "usages",
            });

            // L5.2 — zero usages: distinguish "unknown symbol" from "genuinely unused".
            if (resp.Edges.Count == 0 && !NodeExists(handle, nodeId))
            {
                var suggestions = await SuggestAsync(handle, nodeId);
                return Envelope($"Symbol '{nodeId}' not found.",
                    suggestions.Length > 0 ? "Did you mean one of these? (usages needs an exact nodeId)" : "Use resolve(query) to get the exact nodeId.",
                    "usages(handle, nodeId:\"IBasketRepository\")",
                    suggestions.Length > 0 ? suggestions : null);
            }

            return JsonSerializer.Serialize(new
            {
                nodeId,
                count = resp.Edges.Count,
                usages = resp.Edges.Select(e => new
                {
                    caller = e.From,
                    kind = e.Kind,
                    otherTitle = e.OtherTitle,
                    provenance = Rel(handle, e.HasProvenance ? e.Provenance : null),
                }).ToArray(),
            }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "usages", "usages(handle, nodeId:\"IOrderRepository\")"); }
    }

    /// <summary>Free-text search across graph nodes, paginated. limit default 20, cursor for offset. Example: find("abc123", "Order", kind:"Type", limit:10, cursor:0)</summary>
    [McpServerTool]
    public async Task<string> Find(string? handle = null, string? query = null, string? kind = null, int limit = 20, int cursor = 0)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Envelope("Missing required parameter 'query'.",
                "Pass a search term.", "find(handle, query:\"Order\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "find", "analyze(path) then find(query:\"Order\")"); }
        try
        {
            var resp = await _client.SearchNodesAsync(new SearchRequest
            {
                Handle = handle,
                Query = query,
                Limit = limit + cursor + 20, // fetch extra for offset
            });

            var results = resp.Nodes.AsEnumerable();
            if (kind is not null)
                results = results.Where(n => string.Equals(n.Kind, kind, StringComparison.OrdinalIgnoreCase));

            var all = results.ToArray();
            var page = all.Skip(cursor).Take(limit).ToArray();

            if (all.Length == 0)
            {
                var suggestions = await SuggestAsync(handle, query);
                return Envelope($"No nodes match '{query}'" + (kind is not null ? $" of kind '{kind}'." : "."),
                    suggestions.Length > 0 ? "Did you mean one of these?" : "Try a broader term or drop the kind filter.",
                    "find(handle, query:\"Order\")",
                    suggestions.Length > 0 ? suggestions : null);
            }

            return JsonSerializer.Serialize(new
            {
                query,
                kind,
                cursor,
                limit,
                count = page.Length,
                total = all.Length,
                hasMore = cursor + limit < all.Length,
                results = page.Select(n => new
                {
                    nodeId = n.NodeId,
                    title = TrimTitle(n.Title), // M4.6 — strip lambda/code body from titles
                    kind = n.Kind,
                    tags = n.Tags.ToArray(),
                }).ToArray(),
            }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "find", "find(handle, query:\"Order\")"); }
    }

    // M4.6 — strip code body remnants from node titles (lambda-title leak fix)
    private static string TrimTitle(string title)
    {
        var idx = title.IndexOf("=>", StringComparison.Ordinal);
        if (idx > 0)
        {
            // Only trim if the => is not part of a valid C# operator (like >=)
            if (idx + 2 >= title.Length || title[idx + 2] != '=') // not >=
            {
                title = title[..idx].TrimEnd();
            }
        }
        // Also trim excessively long titles (>200 chars likely contains code)
        if (title.Length > 200)
            title = title[..197] + "...";
        return title;
    }

    /// <summary>Transitive impact analysis: upward (what reaches this) or downward (what does this affect). Grouped by service. Diff-aware files mode for "I changed X". Example: impact("abc123", "OrderService", 4, "down"), impact("abc123", files:["path/to/file.cs"])</summary>
    [McpServerTool]
    public async Task<string> Impact(string? handle = null, string? nodeId = null, int maxDepth = 4, string direction = "up", string[]? files = null)
    {
        if (string.IsNullOrWhiteSpace(nodeId) && (files is null || files.Length == 0))
            return Envelope("Provide 'nodeId' or 'files'.",
                "Give a symbol nodeId, or the changed files for diff-aware impact.",
                "impact(handle, nodeId:\"OrderService\", direction:\"down\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "impact", "analyze(path) then impact(nodeId:\"OrderService\")"); }
        try { return await ImpactCore(handle, nodeId, maxDepth, direction, files); }
        catch (RpcException ex) { return FromRpc(ex, "impact", "impact(handle, nodeId:\"OrderService\", direction:\"down\")"); }
    }

    private async Task<string> ImpactCore(string handle, string? nodeId, int maxDepth, string direction, string[]? files)
    {
        var req = new ImpactRequest
        {
            Handle = handle,
            MaxDepth = maxDepth,
            Direction = direction,
        };

        if (nodeId is not null)
            req.NodeId = nodeId;
        if (files is not null)
            req.Files.AddRange(files);

        var resp = await _client.GetImpactAsync(req);

        // L5.2 — unknown symbol must not masquerade as "no impact" (totalAffected:0).
        // Verify the symbol exists; if it doesn't, return candidates instead of a false zero.
        if (resp.TotalAffected == 0 && nodeId is not null && (files is null || files.Length == 0)
            && !NodeExists(handle, nodeId))
        {
            var suggestions = await SuggestAsync(handle, nodeId);
            return Envelope($"Symbol '{nodeId}' not found — not the same as zero impact.",
                suggestions.Length > 0 ? "Did you mean one of these?" : "Use resolve(query) to find the exact nodeId.",
                "impact(handle, nodeId:\"CheckoutBasketCommandHandler\", direction:\"up\")",
                suggestions.Length > 0 ? suggestions : null);
        }

        var results = resp.Results
            .GroupBy(r => string.IsNullOrEmpty(r.Service) ? "(unknown)" : r.Service)
            .ToDictionary(g => g.Key, g => g.Select(r => new
            {
                title = r.EntryTitle,
                kind = r.Kind,
                hops = r.Hops,
                nodeId = r.NodeId,
                filePath = r.HasFilePath ? Rel(handle, r.FilePath) : null,
                lineNumber = r.HasLineNumber ? (int?)r.LineNumber : null,
            }).ToArray());

        return JsonSerializer.Serialize(new
        {
            direction = resp.Direction,
            totalAffected = resp.TotalAffected,
            resultsByService = results,
        }, JsonOpts);
    }

    // Does an exact node id / resolvable name exist? Used to separate "unknown" from "zero impact".
    private bool NodeExists(string handle, string nodeId)
    {
        try
        {
            var n = _client.GetNode(new NodeRequest { Handle = handle, NodeId = nodeId });
            return n.Found;
        }
        catch { return false; }
    }

    /// <summary>Find config key usage sites (IConfiguration, GetValue, GetSection). Optional key filter. Example: config("abc123", "GrpcSettings:DiscountUrl")</summary>
    [McpServerTool]
    public async Task<string> Config(string? handle = null, string? key = null)
    {
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "config", "analyze(path) then config()"); }
        try
        {
            var req = new ConfigRequest { Handle = handle };
            if (key is not null) req.Key = key;

            var resp = await _client.ConfigLookupAsync(req);

            // L5.2 — a filtered key that matches nothing must not read as "repo has no config".
            // Re-query unfiltered so we can offer the real keys as a next step.
            if (resp.TotalKeys == 0 && key is not null)
            {
                var all = await _client.ConfigLookupAsync(new ConfigRequest { Handle = handle });
                if (all.TotalKeys > 0)
                {
                    var available = all.Bindings.Select(b => b.Key).Distinct(StringComparer.OrdinalIgnoreCase)
                        .Where(k => k.Contains(key, StringComparison.OrdinalIgnoreCase))
                        .Take(8).ToArray();
                    if (available.Length == 0)
                        available = all.Bindings.Select(b => b.Key).Distinct(StringComparer.OrdinalIgnoreCase).Take(8).ToArray();
                    return Envelope($"No config key exactly '{key}' ({all.TotalKeys} keys exist).",
                        "Keys are exact (e.g. 'ConnectionStrings:Db'). Try one of these or omit 'key'.",
                        "config(handle, key:\"" + available[0] + "\")",
                        available);
                }
            }

            var bindings = resp.Bindings
                .GroupBy(b => b.Key)
                .ToDictionary(g => g.Key, g => g.Select(b => new
                {
                    filePath = Rel(handle, b.FilePath),
                    lineNumber = b.LineNumber,
                    patternType = b.PatternType,
                    service = string.IsNullOrEmpty(b.Service) ? null : b.Service,
                    nodeId = string.IsNullOrEmpty(b.NodeId) ? null : b.NodeId,
                }).ToArray());

            return JsonSerializer.Serialize(new
            {
                key = key ?? "(all)",
                totalKeys = resp.TotalKeys,
                keys = bindings,
            }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "config", "config(handle, key:\"ConnectionStrings:Database\")"); }
    }

    /// <summary>Best-effort: find test methods whose call closure reaches a node. Example: tests_for("abc123", "OrderService")</summary>
    [McpServerTool]
    public async Task<string> TestsFor(string? handle = null, string? nodeId = null, int maxDepth = 6)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return Envelope("Missing required parameter 'nodeId'.",
                "Pass a nodeId to find covering tests for.",
                "tests_for(handle, nodeId:\"OrderService\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "tests_for", "analyze(path) then tests_for(nodeId:\"OrderService\")"); }
        try
        {
            var resp = await _client.FindTestsForAsync(new TestsForRequest
            {
                Handle = handle,
                NodeId = nodeId,
                MaxDepth = maxDepth,
            });

            return JsonSerializer.Serialize(new
            {
                nodeId,
                nodeTitle = resp.NodeTitle,
                isBestEffort = resp.IsBestEffort,
                count = resp.Tests.Count,
                tests = resp.Tests.Select(t => new
                {
                    title = t.Title,
                    nodeId = t.NodeId,
                    filePath = Rel(handle, t.FilePath),
                    lineNumber = t.LineNumber,
                    distance = t.Distance,
                    isDirect = t.IsDirect,
                    project = string.IsNullOrEmpty(t.Project) ? null : t.Project,
                }).ToArray(),
            }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "tests_for", "tests_for(handle, nodeId:\"OrderService\")"); }
    }

    /// <summary>List all insights (warnings, notable items, info) for the analyzed repo. Example: insights("abc123")</summary>
    [McpServerTool]
    public async Task<string> Insights(string? handle = null)
    {
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "insights", "analyze(path) then insights()"); }
        var resp = await _client.GetStatsAsync(new SessionRequest { Handle = handle });
        return JsonSerializer.Serialize(new
        {
            count = resp.Insights.Count,
            insights = resp.Insights.Select(i => new
            {
                id = i.Id,
                category = i.Category,
                severity = i.Severity,
                title = i.Title,
                detail = i.Detail,
                evidence = i.Evidence.ToArray(),
                confidence = i.Confidence,
                action = i.Action,
                actionTarget = i.ActionTarget,
            }).ToArray(),
        }, JsonOpts);
    }

    /// <summary>Budget-priced context pack for a focus. budgetTokens default 8000. intent: trace|explain|review. Example: get_context("abc123", "POST /api/orders", 4000, "trace")</summary>
    [McpServerTool]
    public async Task<string> GetContext(string? handle = null, string? focus = null, int budgetTokens = 8000, string intent = "trace")
    {
        if (string.IsNullOrWhiteSpace(focus))
            return Envelope("Missing required parameter 'focus'.",
                "Pass the entry/symbol to build context for.",
                "get_context(handle, focus:\"POST /api/orders\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "get_context", "analyze(path) then get_context(focus:\"POST /api/orders\")"); }
        try
        {
            var resp = await _client.GetContextAsync(new ContextRequest
            {
                Handle = handle,
                Focus = focus,
                BudgetTokens = budgetTokens,
                Intent = intent,
            });
            // L5.2 — focus that resolves to (almost) nothing teaches instead of returning a thin pack.
            // A miss yields only the generic "identity" section (no trace/signatures/bodies).
            var substantive = resp.Sections.Any(s => !string.Equals(s.Key, "identity", StringComparison.OrdinalIgnoreCase));
            if (!resp.Found || !substantive)
            {
                var suggestions = await SuggestAsync(handle, focus);
                return Envelope($"No context could be built for '{focus}'.",
                    suggestions.Length > 0 ? "Did you mean one of these? Use an exact route or nodeId." : "Try top_flows() to find an entry.",
                    "get_context(handle, focus:\"POST /basket/checkout\")",
                    suggestions.Length > 0 ? suggestions : null);
            }

            return JsonSerializer.Serialize(new
            {
                focus,
                budgetTokens,
                totalTokens = resp.TotalTokens,
                sections = resp.Sections.Select(s => new { key = s.Key, tokens = s.Tokens }).ToArray(),
                omitted = resp.Omitted.ToArray(),
                content = string.Join("\n", resp.Sections.Select(s => s.Content)),
            }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "get_context", "get_context(handle, focus:\"POST /api/orders\")"); }
    }

    /// <summary>Read source code for a node. mode: window (default, windowLines lines around) | member (full declaration body). Example: read_source("abc123", "OrderService", 30, "member")</summary>
    [McpServerTool]
    public async Task<string> ReadSource(string? handle = null, string? nodeId = null, int windowLines = 20, string mode = "window")
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return Envelope("Missing required parameter 'nodeId'.",
                "Pass a nodeId. mode: window|member.",
                "read_source(handle, nodeId:\"OrderService\", mode:\"member\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "read_source", "analyze(path) then read_source(nodeId:\"OrderService\")"); }

        NodeResponse resp;
        try
        {
            resp = await _client.GetNodeAsync(new NodeRequest { Handle = handle, NodeId = nodeId });
        }
        catch (RpcException ex) { return FromRpc(ex, "read_source", "read_source(handle, nodeId:\"OrderService\")"); }

        if (!resp.Found || !resp.HasFilePath)
        {
            var suggestions = await SuggestAsync(handle, nodeId);
            return Envelope($"No source location for '{nodeId}'.",
                suggestions.Length > 0 ? "Did you mean one of these?" : "Use resolve(query) to find a node with a file location.",
                "read_source(handle, nodeId:\"OrderService\")",
                suggestions.Length > 0 ? suggestions : null);
        }

        var filePath = resp.FilePath;
        try
        {
            if (!File.Exists(filePath))
                return JsonSerializer.Serialize(new { found = true, nodeId = resp.NodeId, filePath = Rel(handle, filePath), error = "File not found on disk." }, JsonOpts);

            var lines = await File.ReadAllLinesAsync(filePath);
            var lineNum = resp.HasLineNumber ? resp.LineNumber : 1;

            int start, end;
            if (mode == "member" && resp.HasLineNumber)
            {
                // M4.5 — full member: read from declaration line through balancing brace scope
                var scopeStart = lineNum - 1; // 0-indexed
                var (bodyStart, bodyEnd) = FindMethodScope(lines, scopeStart);
                if (bodyEnd > bodyStart)
                {
                    start = bodyStart;
                    end = bodyEnd;
                }
                else
                {
                    // Fallback: extra-large window around declaration
                    start = Math.Max(0, lineNum - 5);
                    end = Math.Min(lines.Length, lineNum + 80);
                }
            }
            else
            {
                var before = Math.Max(0, windowLines / 3);
                var after = windowLines - before;
                start = Math.Max(0, lineNum - before);
                end = Math.Min(lines.Length, lineNum + after);
            }

            return JsonSerializer.Serialize(new
            {
                found = true,
                nodeId = resp.NodeId,
                title = resp.Title,
                filePath = Rel(handle, filePath),
                lineNumber = lineNum,
                startLine = start + 1,
                endLine = end,
                totalLines = lines.Length,
                content = string.Join("\n", lines.Skip(start).Take(end - start)),
            }, JsonOpts);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { found = true, nodeId = resp.NodeId, filePath = Rel(handle, filePath), error = ex.Message }, JsonOpts);
        }
    }

    // M4.5 — find method/type body scope from declaration line (simple brace balancing)
    private static (int bodyStart, int bodyEnd) FindMethodScope(string[] lines, int scopeStart)
    {
        var depth = 0;
        var started = false;
        var bodyStart = scopeStart;
        var bodyEnd = scopeStart;

        for (var i = scopeStart; i < lines.Length && i < scopeStart + 200; i++)
        {
            var line = lines[i];
            foreach (var ch in line)
            {
                if (ch == '{') { depth++; started = true; if (depth == 1) bodyStart = i; }
                if (ch == '}') { depth--; if (started && depth == 0) { bodyEnd = i + 1; return (bodyStart, bodyEnd); } }
            }
            // Stop if we hit the next member declaration without finding braces
            if (!started && i > scopeStart + 5 && depth == 0)
            {
                if (line.TrimStart().StartsWith("public ") || line.TrimStart().StartsWith("private ")
                    || line.TrimStart().StartsWith("protected ") || line.TrimStart().StartsWith("internal ")
                    || line.TrimStart().StartsWith("class ") || line.TrimStart().StartsWith("record "))
                {
                    return (scopeStart, scopeStart);
                }
            }
        }

        // If braces never balanced, return what we have
        return (Math.Max(bodyStart, scopeStart), bodyEnd > bodyStart ? bodyEnd : Math.Min(lines.Length, scopeStart + 80));
    }

    // G4 — relativize absolute paths to repo-relative (D4)
    private string Rel(string handle, string? absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath)) return absolutePath ?? "";
        if (!_repoRoots.TryGetValue(handle, out var root)) return absolutePath;
        var abs = absolutePath.Replace('/', Path.DirectorySeparatorChar);
        var rooted = root + Path.DirectorySeparatorChar;
        if (abs.StartsWith(rooted, StringComparison.OrdinalIgnoreCase))
            return abs[rooted.Length..].Replace(Path.DirectorySeparatorChar, '/');
        return abs;
    }
}
