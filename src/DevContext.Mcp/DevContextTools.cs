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

    // G1.3 (R4 §1 item 4) — the handle a handle-less call belongs to.
    // The server orders ListSessions by LastAccess and bumps LastAccess on EVERY access, including
    // the desktop app's. Taking Sessions[0] therefore meant "whatever repo anyone touched most
    // recently": the app opening repo A silently retargeted this agent's next call about repo B,
    // and the answer came back about the wrong codebase with no sign anything had moved.
    // DevContextTools is a singleton for the life of the MCP process (Program.cs), so this field
    // is exactly "the session THIS client analyzed".
    private string? _lastAnalyzedHandle;

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

        // Ours first — but only if the server still has it (sessions expire and can be closed).
        var mine = _lastAnalyzedHandle;
        if (mine is not null && list.Sessions.Any(s => s.Handle == mine)) return mine;

        // Nothing of ours. One session is unambiguous; two or more is a guess, and guessing which
        // repo the agent meant is the whole defect — so name the choices instead of picking one.
        if (list.Sessions.Count == 1) return list.Sessions[0].Handle;

        var open = string.Join(", ", list.Sessions.Take(4).Select(s => $"{s.Handle}={s.Repo}"));
        throw new RpcException(new Status(StatusCode.FailedPrecondition,
            $"This client has analyzed nothing and {list.Sessions.Count} sessions are open ({open}). "
            + "Pass handle: explicitly, or run analyze(path) first."));
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

    // D5.1 (G1) — better-connected focus candidates for a low-fill pack: the repo's top flows
    // (score/depth = connectedness), excluding the focus that under-filled. Empty on repos with
    // no ranked flows (libraries) — the fill note then stands alone.
    private async Task<object[]> SuggestConnectedAsync(string handle, string exclude, int take = 4)
    {
        try
        {
            var resp = await _client.GetGraphFacetsAsync(new GraphFacetsRequest { Handle = handle, MaxFlows = 12 });
            var flows = resp.FlowList?.Flows;
            if (flows is null) return [];
            return flows
                .Where(f => f.Depth >= 2)
                .Select(f => new
                {
                    Focus = f.HasHttpMethod && f.HasRoute ? $"{f.HttpMethod} {f.Route}" : f.Title,
                    f.Kind,
                    f.Score,
                    f.Depth,
                })
                .Where(x => !string.Equals(x.Focus, exclude, StringComparison.OrdinalIgnoreCase))
                .Take(take)
                .Select(x => (object)new { focus = x.Focus, kind = x.Kind, score = x.Score, depth = x.Depth })
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

    // T3.1 — unified symbol addressing. Every symbol-taking tool accepts a fuzzy `query` alongside
    // its precise `nodeId`/`focus`. A `query` is resolved to ONE precise nodeId through the same
    // ranked search resolve/find use, honoring ambiguity — it NEVER silently picks (Law L5.3).
    // Returns (nodeId, null) on a clean resolve, or (null, <=80-tok envelope) for the caller to
    // return on a miss/ambiguous match. An explicit "Kind:Key" precise id passes straight through.
    private async Task<(string? NodeId, string? Envelope)> ResolveQueryAsync(string handle, string query, string example)
    {
        if (query.Contains(':', StringComparison.Ordinal))
            return (query, null);

        var search = await _client.SearchNodesAsync(new SearchRequest { Handle = handle, Query = query, Limit = 10 });
        if (search.Nodes.Count == 0)
        {
            var suggestions = await SuggestAsync(handle, query);
            return (null, Envelope($"No symbol matched '{query}'.",
                suggestions.Length > 0 ? "Did you mean one of these?" : "Try resolve(query) or a broader term.",
                example, suggestions.Length > 0 ? suggestions : null));
        }

        // graph.Find ranks exact-title matches first; only an exact, unique title resolves silently.
        var exact = search.Nodes
            .Where(n => string.Equals(TrimTitle(n.Title), query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (exact.Count == 1)
            return (exact[0].NodeId, null);

        if (exact.Count == 0)
        {
            var candidates = search.Nodes.Take(5)
                .Select(n => (object)new { nodeId = n.NodeId, title = TrimTitle(n.Title), kind = n.Kind })
                .ToArray();
            return (null, Envelope($"'{query}' has no exact match — ambiguous.",
                "Pass a precise nodeId from resolve(query).", example, candidates));
        }

        // Same short name in multiple projects (identity-ambiguity) — never pick one for them.
        var multi = exact.Take(5)
            .Select(n => (object)new { nodeId = n.NodeId, title = TrimTitle(n.Title), kind = n.Kind })
            .ToArray();
        return (null, Envelope($"'{query}' matches {exact.Count} nodes — ambiguous.",
            "Pass the exact nodeId (from resolve(query)).", example, multi));
    }

    /// <summary>Start analysis of a .NET repo. Returns a handle for subsequent calls. Idempotent: same repo+HEAD+solution returns the existing handle. In a multi-solution repo, sln picks which solution to analyze (name, file name, or repo-relative path). Example: analyze("C:/repos/MyApp"), analyze("C:/repos/GitVersion", sln:"GitVersion.slnx")</summary>
    [McpServerTool]
    public async Task<string> Analyze(string path, string? sln = null)
    {
        var req = new AnalyzeRequest { Path = path };
        if (sln is { Length: > 0 }) req.Sln = sln;
        string? handle = null;
        AnalysisSummary? summary = null;
        var cached = false;
        AnalyzeError? failure = null;

        try
        {
            var call = _client.Analyze(req);
            // R4 item 7 — read the stream instead of draining it. The old loop kept the handle and
            // dropped everything else: the summary the server had already computed, and the error
            // event, so a failed analysis surfaced as the generic "did not return a handle" below
            // and the server's own reason ("Git is not installed", "Repository not found") never
            // reached the agent.
            await foreach (var evt in call.ResponseStream.ReadAllAsync())
            {
                switch (evt.EventCase)
                {
                    case AnalyzeEvent.EventOneofCase.Result:
                        handle = evt.Result.Handle;
                        summary = evt.Result.Summary;
                        cached = evt.Result.Cached;
                        break;
                    case AnalyzeEvent.EventOneofCase.Error:
                        failure = evt.Error;
                        break;
                }
            }
        }
        catch (RpcException ex) { return FromRpc(ex, "analyze", "analyze(\"C:/repos/MyApp\")"); }

        if (failure is not null)
            return Envelope(failure.Message,
                $"Analysis failed ({failure.Code}) — fix the cause and re-run analyze.",
                "analyze(\"C:/repos/MyApp\")");

        if (handle is null)
            return Envelope("Analysis did not return a handle.",
                "The server finished the stream without a result — check the path is a .NET repo, then retry.",
                "analyze(\"C:/repos/MyApp\")");

        // G1.3 — this is the session a later handle-less call belongs to.
        _lastAnalyzedHandle = handle;

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
            // R4 item 7 — `analyze` could sit for eight minutes or return in two milliseconds and say
            // exactly the same thing. `cached` is the server's answer to "did this call analyze
            // anything" (AnalyzeResult.cached: an open session for this repo+HEAD+sln, or a
            // snapshot-cache hit); the note is the CLI's honesty stamp in a sentence.
            cached,
            note = cached
                ? "Served from cache — no analysis ran, and the numbers below describe the run that produced this session."
                : "Analyzed now. A first analysis of a large repo can take minutes; the result is snapshotted, so the same repo+HEAD+solution re-opens in seconds.",
            summary = summary is null ? null : new
            {
                label = summary.Label,
                archetype = summary.Archetype,
                isLibrary = summary.IsLibrary,
                projects = summary.Projects,
                nodes = summary.Nodes,
                edges = summary.Edges,
                entries = summary.Entries,
                entriesWithTarget = summary.EntriesWithTarget,
                elapsedMs = summary.ElapsedMs,
                explanation = summary.Explanation,
                warnings = summary.Warnings.Count > 0 ? summary.Warnings.ToArray() : null,
                stale = summary.Stale ? summary.StaleMessage : null,
            },
            hint = "Analysis complete. Omit 'handle' on later calls and they use THIS session, whatever else the server is serving.",
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

        var services = facets.ServiceMap?.Services ?? new();
        // DisplayName carries the FULL runnable name (Basket.API) — no client-side truncation,
        // and libraries are already excluded (Service nodes only).
        var svcLabels = services.Select(s => s.Kind is { Length: > 0 } && s.Kind != "Service"
            ? $"{s.DisplayName} ({s.Kind})"
            : s.DisplayName).ToArray();
        if (map.IsLibrary)
        {
            // D1.5b — a library's runnable "services" are its test/sample consoles; headlining them
            // ("Library: Newtonsoft.Json.TestConsole") misstates the product identity.
            sb.Append("Library");
            if (map.HasSolutionName) sb.Append(": ").Append(map.SolutionName);
            sb.AppendLine();
            if (svcLabels.Length > 0)
                sb.Append("  hosts: ").AppendJoin(", ", svcLabels).AppendLine();
        }
        else
        {
            sb.Append(map.Archetype);
            if (svcLabels.Length > 0)
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
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "status", "analyze(path) then status()"); }
        try
        {
            await _client.GetStatsAsync(new SessionRequest { Handle = handle });
            return JsonSerializer.Serialize(new { handle, status = "ready" }, JsonOpts);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return JsonSerializer.Serialize(new { found = false, handle, error = "Unknown handle." }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "status", "status(handle)"); }
    }

    /// <summary>Release a session's resources (engine + any clone). Idempotent. Example: close_session("abc123")</summary>
    [McpServerTool]
    public async Task<string> CloseSession(string? handle = null)
    {
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "close_session", "close_session(handle)"); }
        try
        {
            var resp = await _client.CloseSessionAsync(new SessionRequest { Handle = handle });
            if (resp.Closed && handle == _lastAnalyzedHandle) _lastAnalyzedHandle = null;
            // G1.3 — closed:false used to be the whole answer, so closing a handle that was never
            // open read as "done" to an agent skimming for an error. Idempotent, but say which.
            return JsonSerializer.Serialize(new
            {
                closed = resp.Closed,
                found = resp.Closed ? (bool?)null : false,
                handle,
                note = resp.Closed ? null : "No open session with that handle — nothing to close.",
            }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "close_session", "close_session(handle)"); }
    }

    /// <summary>List all active analysis sessions on the server. Example: list_sessions()</summary>
    [McpServerTool]
    public async Task<string> ListSessions()
    {
        ListSessionsResponse resp;
        try { resp = await _client.ListSessionsAsync(new ListSessionsRequest()); }
        catch (RpcException ex) { return FromRpc(ex, "list_sessions", "list_sessions()"); }
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
            // S10: this compared against "WARNING" while the wire carried "Warning", so `warnings`
            // was an empty array on every repo ever analysed — an agent reading it concluded there
            // was nothing to worry about while `insights` above it listed the warnings in full.
            // The wire spelling is now lowercase and pinned server-side.
            warnings = resp.Insights
                .Where(i => i.Severity == "warning")
                .Select(i => i.Title)
                .ToArray(),
            // J1/J3 — per-component swallowed-failure counters (empty = clean run)
            extractionFailures = resp.ExtractionFailures.Select(f => new
            {
                source = f.Source,
                category = f.Category,
                count = f.Count,
                sample = f.Sample,
            }).ToArray(),
        }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "stats", "stats(handle)"); }
    }

    /// <summary>List entry points (HTTP routes, bus consumers, gRPC services). Summary by default: per-kind counts + top-N by score (≤~1.5k tok). Pass kind to filter, top to widen, full:true for every entry. Example: entrypoints("abc123", kind:"HttpEndpoint"), entrypoints("abc123", full:true)</summary>
    [McpServerTool]
    public async Task<string> Entrypoints(string? handle = null, string? kind = null, bool full = false, int top = 15)
    {
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "entrypoints", "analyze(path) then entrypoints()"); }
        EntryPointsResponse resp;
        try { resp = await _client.ListEntryPointsAsync(new SessionRequest { Handle = handle }); }
        catch (RpcException ex) { return FromRpc(ex, "entrypoints", "entrypoints(handle)"); }

        // Per-kind counts over the WHOLE set (honest even when the body is truncated).
        var byKind = resp.EntryPoints
            .GroupBy(e => e.Kind)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());

        var filtered = resp.EntryPoints.AsEnumerable();
        if (kind is not null)
            filtered = filtered.Where(e => string.Equals(e.Kind, kind, StringComparison.OrdinalIgnoreCase));
        var pool = filtered.OrderByDescending(e => e.Score).ToArray();

        // T3.2 — default is a bounded summary; full:true returns every entry with the rich fields.
        var shown = full ? pool : pool.Take(Math.Max(1, top)).ToArray();
        var omitted = pool.Length - shown.Length;

        object Rich(EntryPoint e) => new
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
        };
        object Lean(EntryPoint e) => new
        {
            kind = e.Kind,
            title = e.Title,
            nodeId = e.NodeId,
            httpMethod = e.HasHttpMethod ? e.HttpMethod : null,
            route = e.HasRoute ? e.Route : null,
            target = e.HasTarget ? e.Target : null,
            project = e.HasProject ? e.Project : null,
            score = e.Score,
        };

        string? hint = null;
        if (!full && omitted > 0)
            hint = kind is null
                ? $"Summary: top {shown.Length} of {pool.Length} by score. entrypoints(kind:\"HttpEndpoint\") to filter, entrypoints(full:true) for all."
                : $"Top {shown.Length} of {pool.Length} '{kind}' by score. entrypoints(kind:\"{kind}\", full:true) for all.";

        return JsonSerializer.Serialize(new
        {
            count = resp.EntryPoints.Count,
            kind,
            byKind,
            showing = shown.Length,
            omitted = omitted > 0 ? omitted : (int?)null,
            entries = shown.Select(e => full ? Rich(e) : Lean(e)).ToArray(),
            hint,
        }, JsonOpts);
    }

    /// <summary>The full architecture map: structured surface (topology, packages, aggregates, service styles, library surface, archetype view, solution scope) + rendered markdown. Example: map("abc123")</summary>
    [McpServerTool]
    public async Task<string> Map(string? handle = null)
    {
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "map", "analyze(path) then map()"); }
        try
        {
            var resp = await _client.GetMapAsync(new SessionRequest { Handle = handle });
            // D1.5a — the rendered map IS the product; meta alone made a 1400-node library read as
            // a ~60-token dead map over MCP while the CLI rendered 2000+ (octet DoD, audit A5/G).
            // R4 §1 item 1 — the STRUCTURED surface rides beside it: MapResponse carried packages,
            // aggregates, service styles, the library surface and the archetype view, and this tool
            // dropped every one of them, so the map's own markdown ("… and N more") pointed at a
            // fuller answer no MCP caller could reach. Empty collections serialize as null
            // (WhenWritingNull), so an App-archetype map is no wordier than it was.
            var surface = resp.Surface;
            return JsonSerializer.Serialize(new
            {
                meta = $"style={resp.Style} archetype={resp.Archetype} projects={resp.ProjectCount}",
                solutionName = resp.HasSolutionName ? resp.SolutionName : null,
                archetype = resp.Archetype,
                isLibrary = resp.IsLibrary,
                style = resp.Style,
                styleConfidence = resp.StyleConfidence,
                styleEvidence = resp.HasStyleEvidence ? resp.StyleEvidence : null,
                projectCount = resp.ProjectCount,
                // R3 D-D scope facts: which solution this analysis covered, and what else is on disk.
                // The facts, not the CLI's sentence — an agent switches solutions with analyze(sln:).
                solutionScope = resp.SolutionScope is { } scope ? new
                {
                    analyzedRelPath = scope.AnalyzedRelPath,
                    analyzedName = scope.AnalyzedName,
                    totalOnDisk = scope.TotalOnDisk,
                    otherPaths = scope.OtherPaths.ToArray(),
                    wasRequested = scope.WasRequested,
                } : null,
                topology = resp.Topology.Select(t => new
                {
                    name = t.Name,
                    dependsOn = t.DependsOn.ToArray(),
                    layer = t.HasLayer ? t.Layer : null,
                    feature = t.HasFeature ? t.Feature : null,
                }).ToArray(),
                serviceStyles = resp.ServiceStyles.Count > 0
                    ? resp.ServiceStyles.Select(s => new { projectName = s.ProjectName, style = s.Style, stack = s.Stack.ToArray() }).ToArray()
                    : null,
                packages = resp.Packages.Count > 0
                    ? resp.Packages.Select(p => new { label = p.Label, packages = p.Packages.ToArray() }).ToArray()
                    : null,
                aggregates = resp.Aggregates.Count > 0 ? resp.Aggregates.ToArray() : null,
                pipelineBehaviors = resp.PipelineBehaviors.Count > 0 ? resp.PipelineBehaviors.ToArray() : null,
                stack = resp.Stack.Count > 0 ? resp.Stack.ToArray() : null,
                surface = surface is not null && (surface.Groups.Count > 0 || surface.EntryApi.Count > 0
                    || surface.Abstractions.Count > 0 || surface.ExtensionPoints.Count > 0) ? new
                {
                    entryApi = surface.EntryApi.Count > 0
                        ? surface.EntryApi.Select(e => new
                        {
                            title = e.Title,
                            kind = e.Kind,
                            doc = e.HasDoc ? e.Doc : null,
                            location = e.HasLocation ? e.Location : null,
                        }).ToArray()
                        : null,
                    abstractions = surface.Abstractions.Count > 0
                        ? surface.Abstractions.Select(a => new { name = a.Name, kind = a.Kind, implementorCount = a.ImplementorCount }).ToArray()
                        : null,
                    groups = surface.Groups.Count > 0 ? SurfaceGroups(surface.Groups) : null,
                    // The markdown demotes *.Internal namespaces out of the surface and shows only a
                    // count; the structured field is where "available on request" is actually served.
                    internals = surface.Internals.Count > 0 ? SurfaceGroups(surface.Internals) : null,
                    extensionPoints = surface.ExtensionPoints.Count > 0 ? surface.ExtensionPoints.ToArray() : null,
                    consumerPaths = surface.ConsumerPaths.Count > 0 ? surface.ConsumerPaths.ToArray() : null,
                    generators = surface.Generators.Count > 0
                        ? surface.Generators.Select(g => new { name = g.Name, kind = g.Kind, doc = g.HasDoc ? g.Doc : null }).ToArray()
                        : null,
                    packages = surface.Packages.Count > 0
                        ? surface.Packages.Select(p => new { label = p.Label, packages = p.Packages.ToArray() }).ToArray()
                        : null,
                } : null,
                archetypeView = resp.ArchetypeView is { } av ? new
                {
                    archetype = av.Archetype,
                    sectionLabel = av.SectionLabel,
                    groups = av.Groups.Select(g => new
                    {
                        project = g.Project,
                        layer = g.HasLayer ? g.Layer : null,
                        entries = g.Entries.Select(e => new
                        {
                            kind = e.Kind,
                            title = e.Title,
                            route = e.HasRoute ? e.Route : null,
                            target = e.HasTarget ? e.Target : null,
                            depth = e.Depth,
                            hops = e.Hops,
                        }).ToArray(),
                    }).ToArray(),
                } : null,
                markdown = resp.Markdown,
            }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "map", "map(handle)"); }
    }

    /// <summary>Shared shape for the library surface's public and internal namespace groups.</summary>
    private static object[] SurfaceGroups(IEnumerable<SurfaceGroup> groups) =>
        groups.Select(object (g) => new
        {
            ns = g.Namespace,
            types = g.Types_.Select(t => new
            {
                name = t.Name,
                kind = t.Kind,
                members = t.Members.Count > 0 ? t.Members.ToArray() : null,
                doc = t.HasDoc ? t.Doc : null,
            }).ToArray(),
        }).ToArray();

    /// <summary>Top 20 entry points ranked by importance score. Example: top_flows("abc123")</summary>
    [McpServerTool]
    public async Task<string> TopFlows(string? handle = null)
    {
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "top_flows", "analyze(path) then top_flows()"); }
        // L4.3: ranking + shaping done once by FlowListProjection (server-side), not client-side here.
        GraphFacetsResponse resp;
        try { resp = await _client.GetGraphFacetsAsync(new GraphFacetsRequest { Handle = handle, MaxFlows = 20 }); }
        catch (RpcException ex) { return FromRpc(ex, "top_flows", "top_flows(handle)"); }
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

    /// <summary>Compact flow summary for an entry (≤150 tok typical): what it touches/emits, not the full call spine. Deep-link to trace() for full detail. Address with 'focus' OR 'query'. Example: flow("abc123", "POST /basket/checkout")</summary>
    [McpServerTool]
    public async Task<string> Flow(string? handle = null, string? focus = null, int depth = 8, string? query = null)
    {
        focus ??= query; // T3.1 — accept `query` as a synonym for `focus`
        if (string.IsNullOrWhiteSpace(focus))
            return Envelope("Missing required parameter 'focus' (or 'query').",
                "Pass the entry route or symbol to summarize as 'focus' or 'query'.",
                "flow(handle, focus:\"POST /basket/checkout\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "flow", "analyze(path) then flow(focus:\"POST /basket/checkout\")"); }
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
                var suggestions = await SuggestAsync(handle, focus);
                return Envelope($"No entry or node matched '{focus}'.",
                    suggestions.Length > 0 ? "Did you mean one of these?" : "Try top_flows() to list entries.",
                    "flow(handle, focus:\"POST /basket/checkout\")",
                    suggestions.Length > 0 ? suggestions : null);
            }

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
            var approxTokens = (compact.Length + 3) / 4;
            var stepCount = CountTraceSteps(resp.Root);

            // L5.4 — flow renders compact; if it's large, deep-link to trace for full detail.
            string? hint = null;
            if (approxTokens > 150)
                hint = $"Full detail ({stepCount} steps): trace(handle, focus:\"{focus}\", depth:{depth}, format:\"compact\")";

            return JsonSerializer.Serialize(new
            {
                found = true,
                focus,
                steps = stepCount,
                touches = resp.TouchedEntities.Count,
                emits = resp.EmittedEvents.Count,
                approxTokens,
                legend = SeamLegend(compact),
                text = compact,
                hint,
            }, JsonOpts);
        }
        catch (RpcException ex)
        {
            return FromRpc(ex, "flow", "flow(handle, focus:\"POST /basket/checkout\")");
        }
    }

    private static int CountTraceSteps(TraceNode? node)
    {
        if (node is null) return 0;
        var count = 0;
        foreach (var child in node.Children)
            count += 1 + CountTraceSteps(child);
        return count;
    }

    // T3.3 — total nodes the token budget cut across the tree (sum of per-subtree Omitted counts).
    private static int OmittedNodes(TraceNode? node)
    {
        if (node is null) return 0;
        var sum = node.Omitted;
        foreach (var child in node.Children)
            sum += OmittedNodes(child);
        return sum;
    }

    // T3.3 — when the budget cut nodes, teach the agent how to get the rest without re-tracing.
    private static string? BudgetHint(TraceNode? node, int budgetTokens)
    {
        var omitted = OmittedNodes(node);
        if (omitted == 0) return null;
        return $"{omitted} node(s) omitted to fit ~{budgetTokens} tok. Raise budgetTokens (or 0 for full), or read_source(nodeId)/flow(focus) into a truncated subtree.";
    }

    /// <summary>Archetype-aware starting points for exploring the codebase. Example: interesting_points("abc123")</summary>
    [McpServerTool]
    public async Task<string> InterestingPoints(string? handle = null)
    {
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "interesting_points", "analyze(path) then interesting_points()"); }
        MapResponse map;
        InterestingPointsResponse pts;
        try
        {
            map = await _client.GetMapAsync(new SessionRequest { Handle = handle });
            pts = await _client.GetInterestingPointsAsync(new InterestingPointsRequest
            {
                Handle = handle,
                Archetype = map.Archetype,
            });
        }
        catch (RpcException ex) { return FromRpc(ex, "interesting_points", "interesting_points(handle)"); }
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

    /// <summary>Trace execution flow. Address the entry/symbol with 'focus' OR 'query' (both accepted). trace = call spine from ONE entry (deep); use flow() for a compact summary. format: default|compact. budgetTokens (default 4000) caps the tree — cut subtrees are named ("N omitted"); set 0 for the full tree. Example: trace("abc123", "POST /api/orders", 6, "compact")</summary>
    [McpServerTool]
    // depth 6 mirrors TracePolicy.DefaultDepth. It is a literal because this project is a gRPC CLIENT
    // and deliberately does not reference DevContext.Core — the server, which does, is where the
    // contract lives; sending the same number keeps MCP on it.
    public async Task<string> Trace(string? handle = null, string? focus = null, int depth = 6, string format = "default", string? query = null, int budgetTokens = 4000)
    {
        focus ??= query; // T3.1 — accept `query` as a synonym for `focus`
        if (string.IsNullOrWhiteSpace(focus))
            return Envelope("Missing required parameter 'focus' (or 'query').",
                "Pass an entry route or symbol as 'focus' or 'query'.",
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
                BudgetTokens = budgetTokens, // T3.3 — server shapes the tree to this budget
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
                return JsonSerializer.Serialize(new { found = true, format = "compact", tokens, budgetTokens, omitted = OmittedNodes(resp.Root), hint = BudgetHint(resp.Root, budgetTokens), legend = SeamLegend(compact), text = compact }, JsonOpts);
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
                budgetTokens,
                omitted = OmittedNodes(resp.Root),
                hint = BudgetHint(resp.Root, budgetTokens),
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

    // G1.3 (R4 §1 item 3) — the glyph for a seam, keyed on the value the wire actually carries.
    //
    // These arms are Core's SeamKind enum as the server sends it (ProtoMapper: Seam =
    // step.Seam.ToString()). They used to read "Sends"/"Raises"/"Consumes"/"Handles" — four plurals
    // that enum has never produced — and Data/Resolve/Pipeline had no arm at all, so SEVEN of the
    // ten seam kinds rendered the mute fallback. A compact trace said what the next step was and
    // never why it followed: a DI resolution, a bus send and a database write all read as "·".
    // McpSeamGlyphTests walks the real enum, so a new seam kind cannot land mute again.
    //
    // Label words are TraceRenderer.SeamLabel's, so the CLI tree and this text say the same word
    // for the same edge.
    private static readonly (string Seam, string Glyph, string Label)[] SeamGlyphs =
    [
        ("Entry", "▼", "entry"),
        ("Call", "→", "call"),
        ("Send", "⇒", "send"),
        ("Handle", "◉", "handler"),
        ("Raise", "⬆", "raises"),
        ("Consume", "↓", "consumes"),
        ("Data", "≡", "data"),
        ("Resolve", "◇", "di"),
        ("Pipeline", "∥", "pipeline"),
        ("CrossService", "⇛", "cross-service"),
    ];

    /// <summary>Glyph for a wire seam name. An unmapped seam names itself rather than going mute.</summary>
    public static string SeamGlyph(string? seam)
    {
        if (string.IsNullOrEmpty(seam)) return "·";
        foreach (var (name, glyph, _) in SeamGlyphs)
            if (string.Equals(name, seam, StringComparison.Ordinal)) return glyph;
        return $"[{seam}]";
    }

    /// <summary>Key for the glyphs a rendered trace actually used — ten mystery symbols are not
    /// readable by the agent on the other end, and only the used ones are worth the tokens.</summary>
    public static string? SeamLegend(string rendered)
    {
        var used = SeamGlyphs
            .Where(g => rendered.Contains(g.Glyph, StringComparison.Ordinal))
            .Select(g => $"{g.Glyph} {g.Label}")
            .ToArray();
        return used.Length == 0 ? null : string.Join(" · ", used);
    }

    // M4.3 — compact flow: indented text with seam glyphs and provenance
    private void BuildCompactFlow(StringBuilder sb, TraceNode node, int indent, string handle)
    {
        var prefix = new string(' ', indent * 2);
        var seamGlyph = SeamGlyph(node.Seam);
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

    /// <summary>Detail card for a graph node: title, kind, file path, degrees. Address with 'nodeId' (precise) or 'query' (fuzzy). Example: node("abc123", query:"OrderService")</summary>
    [McpServerTool]
    public async Task<string> Node(string? handle = null, string? nodeId = null, string? query = null)
    {
        if (string.IsNullOrWhiteSpace(nodeId) && string.IsNullOrWhiteSpace(query))
            return Envelope("Missing 'nodeId' or 'query'.",
                "Pass a precise nodeId, or a fuzzy query to resolve.", "node(handle, query:\"OrderService\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "node", "analyze(path) then node(query:\"OrderService\")"); }
        if (string.IsNullOrWhiteSpace(nodeId)) // T3.1 — resolve fuzzy query to a precise nodeId
        {
            var (resolved, env) = await ResolveQueryAsync(handle, query!, "node(handle, query:\"OrderService\")");
            if (env is not null) return env;
            nodeId = resolved!; // non-null when env is null (ResolveQueryAsync contract)
        }
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

    /// <summary>Outgoing/incoming edges of a node. Address with 'nodeId' (precise) or 'query' (fuzzy). direction: out|in|usages. Example: neighbors("abc123", query:"OrderService", direction:"out")</summary>
    [McpServerTool]
    public async Task<string> Neighbors(string? handle = null, string? nodeId = null, string direction = "out", string? query = null)
    {
        if (string.IsNullOrWhiteSpace(nodeId) && string.IsNullOrWhiteSpace(query))
            return Envelope("Missing 'nodeId' or 'query'.",
                "Pass a precise nodeId (or a fuzzy query) and direction (out|in|usages).",
                "neighbors(handle, query:\"OrderService\", direction:\"out\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "neighbors", "analyze(path) then neighbors(query:\"OrderService\")"); }
        if (string.IsNullOrWhiteSpace(nodeId)) // T3.1 — resolve fuzzy query to a precise nodeId
        {
            var (resolved, env) = await ResolveQueryAsync(handle, query!, "neighbors(handle, query:\"OrderService\", direction:\"out\")");
            if (env is not null) return env;
            nodeId = resolved!; // non-null when env is null (ResolveQueryAsync contract)
        }
        try
        {
            var resp = await _client.GetNeighborsAsync(new NeighborsRequest
            {
                Handle = handle,
                NodeId = nodeId,
                Direction = direction,
            });
            if (resp.Edges.Count == 0 && !await NodeExistsAsync(handle, nodeId))
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

    /// <summary>Find all usages (in-edges) of a node across the codebase. Address with 'nodeId'/'query' — a short name or fuzzy query resolves (ambiguity-honest). Example: usages("abc123", query:"IOrderRepository")</summary>
    [McpServerTool]
    public async Task<string> Usages(string? handle = null, string? nodeId = null, string? query = null)
    {
        nodeId ??= query; // T3.1 — `query` synonym; usages already resolves short names below
        if (string.IsNullOrWhiteSpace(nodeId))
            return Envelope("Missing 'nodeId' or 'query'.",
                "Pass a nodeId, short name, or fuzzy query to find usages of.",
                "usages(handle, query:\"IOrderRepository\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "usages", "analyze(path) then usages(query:\"IOrderRepository\")"); }
        try
        {
            var resolved = nodeId;

            // L5.3 — if nodeId is a short name (no colon), resolve it via ranked search first.
            if (!nodeId.Contains(':', StringComparison.Ordinal))
            {
                var search = await _client.SearchNodesAsync(new SearchRequest
                {
                    Handle = handle,
                    Query = nodeId,
                    Limit = 10,
                });
                if (search.Nodes.Count == 0)
                {
                    var suggestions = await SuggestAsync(handle, nodeId);
                    return Envelope($"Symbol '{nodeId}' not found.",
                        suggestions.Length > 0 ? "Did you mean one of these?" : "Use resolve(query) to get the exact nodeId.",
                        "usages(handle, nodeId:\"IBasketRepository\")",
                        suggestions.Length > 0 ? suggestions : null);
                }

                // L5.5 — short names must resolve to an unambiguous node before querying usages.
                // graph.Find ranks exact-title matches first; take only those. A substring-only
                // top match, OR several equally-exact matches across projects, must NOT be silently
                // picked — that yields a wrong-node count the agent cannot detect (design L5.3:
                // "never silently picks").
                var exact = search.Nodes
                    .Where(n => string.Equals(TrimTitle(n.Title), nodeId, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (exact.Count == 0)
                {
                    var top = search.Nodes[0];
                    var topTitle = TrimTitle(top.Title);
                    var candidates = await SuggestAsync(handle, nodeId, 5);
                    return Envelope($"Short name '{nodeId}' is ambiguous — the top match '{topTitle}' is not an exact match.",
                        "Use the full nodeId from resolve(query).",
                        $"usages(handle, nodeId:\"{top.NodeId}\")",
                        candidates.Length > 0 ? candidates : new[] { new { nodeId = top.NodeId, title = topTitle, kind = top.Kind } });
                }

                if (exact.Count > 1)
                {
                    // Same short name in multiple projects (identity-ambiguity) — never pick one for them.
                    var candidates = exact.Take(5)
                        .Select(n => (object)new { nodeId = n.NodeId, title = TrimTitle(n.Title), kind = n.Kind })
                        .ToArray();
                    return Envelope($"Short name '{nodeId}' matches {exact.Count} nodes — ambiguous.",
                        "Pass the exact nodeId of the one you mean (from resolve(query)).",
                        $"usages(handle, nodeId:\"{exact[0].NodeId}\")",
                        candidates);
                }

                resolved = exact[0].NodeId;
            }

            var resp = await _client.GetNeighborsAsync(new NeighborsRequest
            {
                Handle = handle,
                NodeId = resolved,
                Direction = "usages",
            });

            // L5.2 — zero usages: distinguish "unknown symbol" from "genuinely unused".
            if (resp.Edges.Count == 0 && !await NodeExistsAsync(handle, resolved))
            {
                var suggestions = await SuggestAsync(handle, resolved);
                return Envelope($"Symbol '{resolved}' not found.",
                    suggestions.Length > 0 ? "Did you mean one of these? (usages needs an exact nodeId)" : "Use resolve(query) to get the exact nodeId.",
                    "usages(handle, nodeId:\"IBasketRepository\")",
                    suggestions.Length > 0 ? suggestions : null);
            }

            // L5.5 — when a short name resolves to 0 usages, the agent cannot distinguish
            // "genuinely unused" from "wrong node resolved". Signal the ambiguity.
            var wasResolved = resolved != nodeId;
            if (resp.Edges.Count == 0 && wasResolved)
            {
                var suggestions = await SuggestAsync(handle, nodeId, 5);
                return Envelope($"Short name '{nodeId}' resolved to {resolved} but has 0 usages — verify this is the intended node.",
                    "Use resolve(query) to see all matching nodes, then pass the exact nodeId.",
                    $"usages(handle, nodeId:\"{resolved}\")",
                    suggestions.Length > 0 ? suggestions : null);
            }

            return JsonSerializer.Serialize(new
            {
                nodeId = resolved,
                count = resp.Edges.Count,
                resolvedFrom = resolved != nodeId ? nodeId : null,
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
            // R4 item 6 — the kind filter and the match count are the SERVER's job now. This used to
            // over-fetch (limit+cursor+20), filter that page by kind, and report the survivors as
            // `total`: on eShop, find("Order", limit:100) answered total=120, which is the fetch
            // window to the node, and find("Order", kind:"Type", limit:5) answered 22 — "Types among
            // the first 25 matches". Both moved when the page size moved, which is the tell.
            var req = new SearchRequest { Handle = handle, Query = query, Limit = limit + cursor };
            if (kind is { Length: > 0 }) req.Kind = kind;
            var resp = await _client.SearchNodesAsync(req);

            var total = resp.TotalMatches;
            var page = resp.Nodes.Skip(cursor).Take(limit).ToArray();

            if (total == 0)
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
                total,
                hasMore = cursor + page.Length < total,
                // A cursor past the end is not an error and not "no matches" — say which it is.
                hint = page.Length == 0 ? $"cursor {cursor} is past the last of {total} matches — lower it." : null,
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

    /// <summary>Transitive impact analysis: upward (what reaches this) or downward (what does this affect). Grouped by service. Address the symbol with 'nodeId' or 'query'; or diff-aware 'files' mode for "I changed X". Example: impact("abc123", query:"OrderService", direction:"down"), impact("abc123", files:["path/to/file.cs"])</summary>
    [McpServerTool]
    public async Task<string> Impact(string? handle = null, string? nodeId = null, int maxDepth = 4, string direction = "up", string[]? files = null, string? query = null)
    {
        if (string.IsNullOrWhiteSpace(nodeId) && string.IsNullOrWhiteSpace(query) && (files is null || files.Length == 0))
            return Envelope("Provide 'nodeId'/'query' or 'files'.",
                "Give a symbol (nodeId or fuzzy query), or the changed files for diff-aware impact.",
                "impact(handle, query:\"OrderService\", direction:\"down\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "impact", "analyze(path) then impact(query:\"OrderService\")"); }
        if (string.IsNullOrWhiteSpace(nodeId) && !string.IsNullOrWhiteSpace(query) && (files is null || files.Length == 0))
        {   // T3.1 — resolve fuzzy query to a precise nodeId
            var (resolved, env) = await ResolveQueryAsync(handle, query, "impact(handle, query:\"OrderService\", direction:\"down\")");
            if (env is not null) return env;
            nodeId = resolved!; // non-null when env is null (ResolveQueryAsync contract)
        }
        try { return await ImpactCore(handle, nodeId, maxDepth, direction, files); }
        catch (RpcException ex) { return FromRpc(ex, "impact", "impact(handle, query:\"OrderService\", direction:\"down\")"); }
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
            && !await NodeExistsAsync(handle, nodeId))
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
    private async Task<bool> NodeExistsAsync(string handle, string nodeId)
    {
        try
        {
            var n = await _client.GetNodeAsync(new NodeRequest { Handle = handle, NodeId = nodeId });
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
                // T3.6 — self-describing: name the scan and its blind spot.
                method = "Scanned source for IConfiguration indexer / GetValue / GetSection / GetConnectionString with literal keys. Dynamically-built keys are not captured.",
                keys = bindings,
            }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "config", "config(handle, key:\"ConnectionStrings:Database\")"); }
    }

    /// <summary>Best-effort: find test methods whose call closure reaches a node. Address with 'nodeId' or 'query'. Method: walks in-edges up to maxDepth and name/path/project-classifies callers as tests; 0 means none reached (not "untested"). Example: tests_for("abc123", query:"OrderService")</summary>
    [McpServerTool]
    public async Task<string> TestsFor(string? handle = null, string? nodeId = null, int maxDepth = 6, string? query = null)
    {
        if (string.IsNullOrWhiteSpace(nodeId) && string.IsNullOrWhiteSpace(query))
            return Envelope("Missing 'nodeId' or 'query'.",
                "Pass a nodeId (or fuzzy query) to find covering tests for.",
                "tests_for(handle, query:\"OrderService\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "tests_for", "analyze(path) then tests_for(query:\"OrderService\")"); }
        if (string.IsNullOrWhiteSpace(nodeId)) // T3.1 — resolve fuzzy query to a precise nodeId
        {
            var (resolved, env) = await ResolveQueryAsync(handle, query!, "tests_for(handle, query:\"OrderService\")");
            if (env is not null) return env;
            nodeId = resolved!; // non-null when env is null (ResolveQueryAsync contract)
        }
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
                // T3.6 — self-describing: say what was scanned and what 0 means.
                method = $"Walked in-edges ≤{maxDepth} hops; callers classified as tests by name/path/project. 0 = none reached in-graph, not proof of no coverage.",
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
        StatsResponse resp;
        try { resp = await _client.GetStatsAsync(new SessionRequest { Handle = handle }); }
        catch (RpcException ex) { return FromRpc(ex, "insights", "insights(handle)"); }
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

    /// <summary>Budget-priced context pack for a focus. Address with 'focus' OR 'query'. budgetTokens default 8000. intent: trace|explain|review. Example: get_context("abc123", "POST /api/orders", 4000, "trace")</summary>
    [McpServerTool]
    public async Task<string> GetContext(string? handle = null, string? focus = null, int budgetTokens = 8000, string intent = "trace", string? query = null)
    {
        focus ??= query; // T3.1 — accept `query` as a synonym for `focus`
        if (string.IsNullOrWhiteSpace(focus))
            return Envelope("Missing required parameter 'focus' (or 'query').",
                "Pass the entry/symbol to build context for as 'focus' or 'query'.",
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

            // D5.1 (G1) — the T4.2 ≥85%-fill promise must never fail SILENTLY. A low-fill pack
            // says WHY: budget-cut (raise budgetTokens) vs content-exhausted (the focus's
            // connected subgraph is small — everything reachable is already in the pack), and a
            // content-exhausted under-fill suggests better-connected focuses so the agent has a
            // next move instead of a near-empty pack.
            string? fillNote = null;
            object[]? suggestedFocuses = null;
            var fillPct = budgetTokens > 0 ? (int)(resp.TotalTokens * 100L / budgetTokens) : 100;
            if (fillPct < 85)
            {
                var budgetCut = resp.Omitted.Any(o =>
                    o.Contains("budget", StringComparison.OrdinalIgnoreCase) ||
                    o.Contains("trimmed", StringComparison.OrdinalIgnoreCase));
                if (budgetCut)
                {
                    fillNote = $"fill {fillPct}%: sections were cut to fit the budget — raise budgetTokens for the rest (see omitted).";
                }
                else
                {
                    fillNote = $"fill {fillPct}%: the pack already contains everything reachable from this focus — its connected subgraph is small (not an error; a smaller budget fits it).";
                    suggestedFocuses = await SuggestConnectedAsync(handle, focus);
                    if (suggestedFocuses.Length > 0)
                        fillNote += " Better-connected focuses in suggestedFocuses.";
                }
            }

            return JsonSerializer.Serialize(new
            {
                focus,
                budgetTokens,
                totalTokens = resp.TotalTokens,
                fillNote,
                suggestedFocuses = suggestedFocuses is { Length: > 0 } ? suggestedFocuses : null,
                sections = resp.Sections.Select(s => new { key = s.Key, tokens = s.Tokens }).ToArray(),
                omitted = resp.Omitted.ToArray(),
                content = string.Join("\n", resp.Sections.Select(s => s.Content)),
            }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "get_context", "get_context(handle, focus:\"POST /api/orders\")"); }
    }

    /// <summary>Has the source drifted since analyze? Compares analyze-time file hashes vs disk per pack section for a focus. Returns per-section stale flags, changed files with line deltas, and repo HEAD then/now. Method: hash + line-count delta only (no diff). Example: verify_context("abc123", "POST /api/orders")</summary>
    [McpServerTool]
    public async Task<string> VerifyContext(string? handle = null, string? focus = null, int budgetTokens = 8000, string? query = null)
    {
        focus ??= query; // same synonym contract as get_context (T3.1)
        if (string.IsNullOrWhiteSpace(focus))
            return Envelope("Missing required parameter 'focus' (or 'query').",
                "Pass the entry/symbol whose context should be verified.",
                "verify_context(handle, focus:\"POST /api/orders\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "verify_context", "analyze(path) then verify_context(focus:\"POST /api/orders\")"); }
        try
        {
            var resp = await _client.VerifyContextAsync(new VerifyContextRequest
            {
                Handle = handle,
                Focus = focus,
                BudgetTokens = budgetTokens,
            });
            if (!resp.Found)
            {
                var suggestions = await SuggestAsync(handle, focus);
                return Envelope($"No context to verify for '{focus}'.",
                    suggestions.Length > 0 ? "Did you mean one of these?" : "Try top_flows() to find an entry.",
                    "verify_context(handle, focus:\"POST /basket/checkout\")",
                    suggestions.Length > 0 ? suggestions : null);
            }

            return JsonSerializer.Serialize(new
            {
                focus,
                anyStale = resp.AnyStale,
                headMoved = resp.AnalyzedGitHead.Length > 0 && resp.AnalyzedGitHead != resp.CurrentGitHead,
                analyzedGitHead = resp.AnalyzedGitHead,
                currentGitHead = resp.CurrentGitHead,
                sections = resp.Sections.Select(s => new
                {
                    key = s.Key,
                    stale = s.Stale,
                    filesChecked = s.FilesChecked,
                    changed = s.Changed.Select(c => new { file = c.File, status = c.Status, lineDelta = c.LineDelta }).ToArray(),
                }).ToArray(),
                method = "analyze-time sha256 + line-count vs disk now; 'unknown' = no analyze-time fingerprint for the file; re-run analyze to refresh",
            }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "verify_context", "verify_context(handle, focus:\"POST /api/orders\")"); }
    }

    /// <summary>Read source code for a node. Address with 'nodeId' or 'query'. mode: window (default, windowLines lines around) | member (full declaration body). Example: read_source("abc123", query:"OrderService", mode:"member")</summary>
    [McpServerTool]
    public async Task<string> ReadSource(string? handle = null, string? nodeId = null, int windowLines = 20, string mode = "window", string? query = null)
    {
        if (string.IsNullOrWhiteSpace(nodeId) && string.IsNullOrWhiteSpace(query))
            return Envelope("Missing 'nodeId' or 'query'.",
                "Pass a nodeId (or fuzzy query). mode: window|member.",
                "read_source(handle, query:\"OrderService\", mode:\"member\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "read_source", "analyze(path) then read_source(query:\"OrderService\")"); }
        if (string.IsNullOrWhiteSpace(nodeId)) // T3.1 — resolve fuzzy query to a precise nodeId
        {
            var (resolved, env) = await ResolveQueryAsync(handle, query!, "read_source(handle, query:\"OrderService\", mode:\"member\")");
            if (env is not null) return env;
            nodeId = resolved!; // non-null when env is null (ResolveQueryAsync contract)
        }

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
