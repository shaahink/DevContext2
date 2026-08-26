using System.Collections.Concurrent;
using System.ComponentModel;
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

    // T1.1 (BUG-BACKLOG #5) — the wire carrier for every tool/parameter description is
    // [Description]; ModelContextProtocol 1.4.0 does NOT read the XML doc file (measured, see
    // DevContext.Mcp.csproj). The XML `///` summaries that used to sit on these methods were
    // deleted rather than kept alongside: two spellings of the same sentence, one of which never
    // ships, is exactly how this defect survived a year. One carrier, and a gate reads it off the
    // wire (eval/mcp-qa/wire-truth.js).
    //
    // `handle` is on 21 of 22 tools; a const keeps that one sentence single-sourced.
    private const string HandleDoc = "Session handle from analyze(). Omit to use this client's last analyzed session.";
    private const string NodeIdDoc = "Exact node id, as returned by resolve/find/neighbors.";

    /// <summary>T1.3 (BUG-BACKLOG #9) — the marker Core's ContextPackBuilder puts on every omitted[]
    /// line that reports a CUT (as opposed to an empty section). MCP references Contracts only, not
    /// Core, so this is a MIRROR of <c>ContextPackBuilder.ElidedPrefix</c> — and a mirrored constant
    /// is exactly the drift class this program has been bitten by, so
    /// <c>McpElisionContractTests</c> (Server.Tests sees both assemblies) pins the two together.</summary>
    public const string ElidedPrefix = "elided ";

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

    // T1.3 (BUG-BACKLOG #10) — an out-of-range enum is REJECTED, not quietly re-interpreted.
    //
    // Every one of these dials used to be a bare string forwarded to a `==` or a `switch` with a
    // `_ =>` default: read_source(mode:"full") returned a 20-line window with found:true, and
    // impact(direction:"sideways") computed Up and then ECHOED "sideways" back in the response. The
    // agent has no way to notice — there is no field in either reply that disagrees with what it
    // asked for. That is the confident-partial-truth family, and the fix is to answer the question
    // it actually asked or say why not.
    private static string? RejectBadEnum(string param, string value, string[] allowed, string example)
        => allowed.Contains(value, StringComparer.Ordinal) ? null
            : Envelope($"Invalid {param}: '{value}'.",
                $"{param} must be one of: {string.Join(" | ", allowed)}.", example);

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

    [McpServerTool]
    [Description("Start analysis of a .NET repo. Returns a handle for subsequent calls. Idempotent: same repo+HEAD+solution returns the existing handle. Example: analyze(\"C:/repos/MyApp\"), analyze(\"C:/repos/GitVersion\", sln:\"GitVersion.slnx\")")]
    public async Task<string> Analyze(
        [Description("Absolute path to the repo root, or a GitHub URL/owner-name to clone.")] string path,
        [Description("Which solution to analyze in a multi-solution repo: name, file name, or repo-relative path.")] string? sln = null)
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
            note = CacheNote(cached, summary),
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
                // R4 item 10 — what the numbers above are ABOUT. Without these an eight-minute run
                // and a 200ms rehydrate of a three-day-old snapshot were the same object on the wire.
                fromCache = summary.FromCache,
                analyzedAt = summary.AnalyzedAt.Length > 0 ? summary.AnalyzedAt : null,
                gitHead = summary.GitHead.Length > 0 ? summary.GitHead : null,
                explanation = summary.Explanation,
                warnings = summary.Warnings.Count > 0 ? summary.Warnings.ToArray() : null,
                stale = summary.Stale ? summary.StaleMessage : null,
            },
            hint = "Analysis complete. Omit 'handle' on later calls and they use THIS session, whatever else the server is serving.",
        }, JsonOpts);
    }

    /// <summary>R4 item 10 — the sentence that makes <c>analyze</c>'s numbers datable. "Served from
    /// cache" alone left the agent to assume the analysis was recent; it can be days old, taken at a
    /// commit that is no longer checked out anywhere it can see. When the summary carries the
    /// snapshot's own instant and head, say both — and say what elapsedMs is then measuring, because
    /// on a rehydrate it times the LOAD and reads like an implausibly fast analysis.</summary>
    private static string CacheNote(bool cached, AnalysisSummary? summary)
    {
        if (!cached)
            return "Analyzed now. A first analysis of a large repo can take minutes; the result is snapshotted, so the same repo+HEAD+solution re-opens in seconds.";

        var when = summary is { AnalyzedAt.Length: > 0 } ? $" that finished {summary.AnalyzedAt}" : "";
        var head = summary is { GitHead.Length: >= 7 } ? $" at HEAD {summary.GitHead[..7]}" : "";
        var elapsed = summary is { FromCache: true }
            ? " elapsedMs below is the snapshot load, not that analysis."
            : "";
        return $"Served from cache — no analysis ran. The numbers below describe an analysis{when}{head}.{elapsed}";
    }

    [McpServerTool]
    [Description("One-call repo brief: identity, services, ServiceLinks, top flows, where to start. ~600 tokens max. Start here after analyze. Example: overview(\"abc123\")")]
    public async Task<string> Overview([Description(HandleDoc)] string? handle = null)
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

        // G2.1 (R4 §1 item 11) — `interesting_points` folded in here. The prose line above is a
        // reading aid; these rows are the ADDRESSABLE form — a nodeId is what trace/node/read_source
        // take, and dropping it was what made the prose a dead end. Every field the retired tool
        // returned is here, all of them, not the first four.
        var startHere = pts.Points.Select(p => new
        {
            nodeId = p.NodeId,
            title = p.Title,
            kind = p.Kind,
            why = p.Why,
            tags = p.Tags.ToArray(),
        }).ToArray();

        if (map.PipelineBehaviors.Count > 0)
            sb.Append("Behaviors: ").AppendJoin(", ", map.PipelineBehaviors).AppendLine();

        if (map.Stack.Count > 0)
            sb.Append("Stack: ").AppendJoin(", ", map.Stack.Take(10)).AppendLine();

        var text = sb.ToString();
        var tokens = (text.Length + 3) / 4;

        return JsonSerializer.Serialize(new { handle, archetype = map.Archetype, tokens, text, startHere }, JsonOpts);
    }

    [McpServerTool]
    [Description("Resolve a symbol/route/file to candidates with kind, service, path and the nodeId every other tool takes. Never silently picks - returns all matches. Example: resolve(\"abc123\", \"Product\")")]
    public async Task<string> Resolve(
        [Description(HandleDoc)] string? handle = null,
        [Description("Symbol, route or file to resolve - a short name is fine.")] string? query = null,
        [Description("Max candidates to return (default 10).")] int limit = 10)
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

    [McpServerTool]
    [SpecialistTool("session lifecycle - analyze() is idempotent and hands back the handle, so most agents never need this")]
    [Description("Check whether a session handle is still valid. Example: status(\"abc123\")")]
    public async Task<string> Status([Description(HandleDoc)] string? handle = null)
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

    [McpServerTool]
    [SpecialistTool("session lifecycle - sessions expire on their own; this is server housekeeping, not agent work")]
    [Description("Release a session's resources (engine + any clone). Idempotent. Example: close_session(\"abc123\")")]
    public async Task<string> CloseSession([Description(HandleDoc)] string? handle = null)
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

    [McpServerTool]
    [SpecialistTool("session lifecycle - when two sessions make a handle-less call ambiguous the error already names them")]
    [Description("List all active analysis sessions on the server, with the repo each one covers. Example: list_sessions()")]
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
                // R4 item 10 — ageSeconds is how long ago the SESSION opened, which a snapshot-cache
                // hit resets to zero. analyzedAt/commitSha describe the ANALYSIS: what commit these
                // numbers are about and when they were actually computed.
                ageSeconds = s.AgeSeconds,
                analyzedAt = s.AnalyzedAt.Length > 0 ? s.AnalyzedAt : null,
                fromCache = s.FromCache,
                commitSha = s.CommitSha.Length > 0 ? s.CommitSha : null,
                calls = s.Calls,
                nodes = s.Nodes,
                edges = s.Edges,
                entries = s.Entries,
            }).ToArray(),
        }, JsonOpts);
    }

    [McpServerTool]
    [Description("Full analysis stats: node/edge counts, seam breakdown, insights, warnings. Example: stats(\"abc123\")")]
    public async Task<string> Stats([Description(HandleDoc)] string? handle = null)
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
                // V1.1 (#25): read the carried tier split. This was `seam.Count - seam.Approx`, which
                // reported every unlabelled (Join-default) edge to the agent as Roslyn-verified.
                verified = seam.Verified,
                joined = seam.Joined,
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
                // G2.1 — the one field insights() carried that stats() did not, so folding the two
                // menu entries into one costs an agent nothing.
                confidence = i.Confidence,
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

    [McpServerTool]
    [Description("List entry points (HTTP routes, bus consumers, gRPC services, jobs). Summary by default: per-kind counts + top-N by score (~1.5k tok). Names round-trip into trace/get_context. Example: entrypoints(\"abc123\", kind:\"HttpEndpoint\"), entrypoints(\"abc123\", full:true)")]
    public async Task<string> Entrypoints(
        [Description(HandleDoc)] string? handle = null,
        [Description("Filter to one entry kind, e.g. HttpEndpoint, MessageConsumer, GrpcService. Omit for all kinds.")] string? kind = null,
        [Description("true = every entry instead of the scored top-N summary.")] bool full = false,
        [Description("How many entries the summary lists per kind (default 15).")] int top = 15)
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

    [McpServerTool]
    [Description("The full architecture map: structured surface (topology, packages, aggregates, service styles, library surface, archetype view, solution scope) plus rendered markdown. Bigger than overview - use when you need the whole picture. Example: map(\"abc123\")")]
    public async Task<string> Map([Description(HandleDoc)] string? handle = null)
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
                // R3 D-4: runnable apps OUTSIDE the analysed solution. Separate key on purpose — an
                // agent that folded these into serviceStyles would be answering "what services are
                // there?" with projects this analysis never opened.
                outsideScopeApps = resp.OutsideScopeApps.Count > 0
                    ? resp.OutsideScopeApps.Select(s => new { projectName = s.ProjectName, style = s.Style, stack = s.Stack.ToArray() }).ToArray()
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

    [McpServerTool]
    [SpecialistTool("entrypoints() already returns the top-N by the same score, alongside the per-kind counts")]
    [Description("Top 20 entry points ranked by importance score - the shortlist of flows worth tracing. Example: top_flows(\"abc123\")")]
    public async Task<string> TopFlows([Description(HandleDoc)] string? handle = null)
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

    // G2.1 (R4 §1 item 11) — `flow` is FOLDED into trace(format:"compact"). It was not a second
    // capability: both tools called the SAME GetTrace RPC and rendered the response through the same
    // BuildCompactFlow, so the text they returned was built by one code path. What differed was the
    // dials they sent — flow walked depth 8 with NO budget, trace walks TracePolicy's depth with one —
    // which is item 12's divergence living inside a single client. The counters flow put in its
    // envelope (steps/touches/emits) now ride the compact trace, so nothing an agent could read is
    // lost. UnknownToolHandler answers the retired name with the call that replaces it.

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
    // G2.2: `budgetTokens` is null when the caller named none, and the hint then says so instead of
    // quoting a number. This client no longer knows the figure — that is the point of item 12: the
    // budget rule lives in TracePolicy on the server, and restating it here is what let the two
    // drift. Naming a number it did not send would be an invention, which is the class of defect
    // this strand exists to remove.
    private static string? BudgetHint(TraceNode? node, int? budgetTokens)
    {
        var omitted = OmittedNodes(node);
        if (omitted == 0) return null;
        var fit = budgetTokens is { } b ? $"~{b} tok" : "the server's default trace budget";
        return $"{omitted} node(s) omitted to fit {fit}. Pass budgetTokens (0 for the full tree), or read_source(nodeId) into a truncated subtree.";
    }

    // G2.1 (R4 §1 item 11) — `interesting_points` is FOLDED into overview(). Overview already made
    // the same GetInterestingPoints call; it just spent the answer on four bare titles in prose and
    // threw the nodeId away, so the one thing that made a starting point ADDRESSABLE was the thing
    // it dropped. The points now ride overview's envelope in full.

    [McpServerTool]
    [Description("Trace execution flow: the call spine from ONE entry, hop by hop. Address it with 'focus' OR 'query' - an entry route, a symbol name, or a nodeId from resolve/find. format:\"compact\" is the small flow summary (what it touches/emits, ~150 tok); format:\"default\" is the full tree. Omit depth/budgetTokens to get the server's trace policy; cut subtrees are named (\"N omitted\"); budgetTokens:0 gives the full tree. Example: trace(\"abc123\", \"POST /api/orders\", 6, \"compact\")")]
    // G2.2 (R4 §1 item 12) — depth and budgetTokens are NULLABLE, and that is the whole fix.
    //
    // They used to be `int depth = 6, int budgetTokens = 4000`: literals mirroring TracePolicy,
    // written here because this project is a gRPC CLIENT that deliberately does not reference
    // DevContext.Core. But a C# default is not the same as an unset field — the tool assigned both
    // on EVERY call, so a proto3 optional's presence bit was always set, the server's own defaults
    // were unreachable, and TracePolicy.ElasticDepth (which fires only when the caller left the
    // depth to the server) could never run at all. Mirroring a constant does not keep two surfaces
    // together; not restating it does. The server, which owns TracePolicy, decides.
    public async Task<string> Trace(
        [Description(HandleDoc)] string? handle = null,
        [Description("What to trace: an entry route, a symbol name, or a nodeId.")] string? focus = null,
        [Description("Max hops. Omit for the server's trace policy (elastic depth).")] int? depth = null,
        [Description("\"default\" (full tree) or \"compact\" (~150-token flow summary). Any other value is rejected.")] string format = "default",
        [Description("Synonym for 'focus'.")] string? query = null,
        [Description("Token budget for the rendered tree. Omit for the server's policy; 0 = no budget, full tree.")] int? budgetTokens = null)
    {
        focus ??= query; // T3.1 — accept `query` as a synonym for `focus`
        if (string.IsNullOrWhiteSpace(focus))
            return Envelope("Missing required parameter 'focus' (or 'query').",
                "Pass an entry route or symbol as 'focus' or 'query'.",
                "trace(handle, focus:\"POST /basket/checkout\")");
        if (RejectBadEnum("format", format, ["default", "compact"],
            "trace(handle, focus:\"POST /basket/checkout\", format:\"compact\")") is { } badFormat) return badFormat;
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "trace", "analyze(path) then trace(focus:\"POST /basket/checkout\")"); }
        try
        {
            var req = new TraceRequest { Handle = handle, Focus = focus };
            // Only SEND a dial the caller named. Assigning a proto3 optional sets its presence bit
            // even when the value equals the default, which is what made the server's policy
            // unreachable from here.
            if (depth is { } d) req.Depth = d;
            if (budgetTokens is { } b) req.BudgetTokens = b;

            var resp = await _client.GetTraceAsync(req);
            if (!resp.Found)
            {
                // L5.2 — brittle focus resolution now teaches: suggest real entries/nodes.
                var suggestions = await SuggestAsync(handle, focus);
                return Envelope($"No entry or node matched '{focus}'.",
                    suggestions.Length > 0 ? "Did you mean one of these? Use an exact route or nodeId." : "Try top_flows() to list traceable entries.",
                    "trace(handle, focus:\"POST /basket/checkout\")",
                    suggestions.Length > 0 ? suggestions : null);
            }

            // T1.3 (BUG-BACKLOG #6) — `found:true` with zero steps is the VACUOUS shape: it passes any
            // "did I get a trace" assertion and tells the agent nothing. A trace that resolved to a
            // node with no outgoing wiring must SAY that, and name the call that would show the other
            // direction — otherwise "this type calls nothing" and "trace could not walk this" are the
            // same reply. Kept next to the response so both formats carry it.
            var stepCount = CountTraceSteps(resp.Root);
            var emptyNote = stepCount == 0 && resp.Root is { } r0
                ? $"0 steps: nothing outbound was resolved from '{r0.Title}' — it calls, sends or handles "
                  + $"nothing the graph could bind. Its callers are the other direction: "
                  + $"neighbors(nodeId:\"{r0.NodeId}\", direction:\"in\"), or usages(nodeId:\"{r0.NodeId}\")."
                : null;

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
                // G2.1 — the three counters `flow` carried in its envelope. They are the summary an
                // agent reads before deciding whether to spend the full tree, and they cost nothing:
                // the response they count is already in hand.
                return JsonSerializer.Serialize(new
                {
                    found = true,
                    format = "compact",
                    focus,
                    steps = stepCount,
                    note = emptyNote,
                    touches = resp.TouchedEntities.Count,
                    emits = resp.EmittedEvents.Count,
                    tokens,
                    budgetTokens,
                    budgetSource = budgetTokens is null ? "server trace policy" : "caller",
                    omitted = OmittedNodes(resp.Root),
                    hint = BudgetHint(resp.Root, budgetTokens),
                    legend = SeamLegend(compact),
                    text = compact,
                }, JsonOpts);
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
                steps = stepCount,
                note = emptyNote,
                budgetTokens,
                budgetSource = budgetTokens is null ? "server trace policy" : "caller",
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

    [McpServerTool]
    [SpecialistTool("resolve() already returns title, kind, service and path; neighbors() carries the degrees")]
    [Description("Detail card for a graph node: title, kind, file path, in/out degrees. Address with 'nodeId' (precise) or 'query' (fuzzy). Example: node(\"abc123\", query:\"OrderService\")")]
    public async Task<string> Node(
        [Description(HandleDoc)] string? handle = null,
        [Description(NodeIdDoc)] string? nodeId = null,
        [Description("Fuzzy name to resolve instead of a nodeId.")] string? query = null)
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

    [McpServerTool]
    [Description("Outgoing/incoming edges of a node - one hop, with the seam kind and file:line on each. Optional 'kind' filters to one seam, which is how you ask a pointed question: who WRITES this table (direction:\"in\", kind:\"ReadsWrites\"), who SENDS this command (direction:\"in\", kind:\"Sends\"), who CONSUMES this event (kind:\"Consumes\"). The reply always names every kind this node does have, so a miss tells you what to ask instead. Example: neighbors(\"abc123\", query:\"Order\", direction:\"in\", kind:\"ReadsWrites\")")]
    public async Task<string> Neighbors(
        [Description(HandleDoc)] string? handle = null,
        [Description(NodeIdDoc)] string? nodeId = null,
        [Description("\"out\" (default), \"in\", or \"usages\". Any other value is rejected.")] string direction = "out",
        [Description("Fuzzy name to resolve instead of a nodeId.")] string? query = null,
        [Description("Filter to one seam kind, e.g. Calls, ReadsWrites, Sends, Consumes. Omit for all kinds.")] string? kind = null)
    {
        if (string.IsNullOrWhiteSpace(nodeId) && string.IsNullOrWhiteSpace(query))
            return Envelope("Missing 'nodeId' or 'query'.",
                "Pass a precise nodeId (or a fuzzy query) and direction (out|in|usages).",
                "neighbors(handle, query:\"OrderService\", direction:\"out\")");
        // Server-side this is `Direction is "in" or "usages" ? In : Out`, so anything else was
        // silently answered as `out` — and the reply echoed the word the caller sent.
        if (RejectBadEnum("direction", direction, ["out", "in", "usages"],
            "neighbors(handle, query:\"OrderService\", direction:\"in\")") is { } badDir) return badDir;
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
            var req = new NeighborsRequest { Handle = handle, NodeId = nodeId, Direction = direction };
            if (!string.IsNullOrWhiteSpace(kind)) req.Kind = kind;
            var resp = await _client.GetNeighborsAsync(req);

            var kindsPresent = resp.KindsPresent
                .Select(k => (object)new { kind = k.Kind, count = k.Count })
                .ToArray();

            // G3.2 — a filter that matched nothing is NOT a missing node, and the server already
            // told us which it is: an unfiltered count above zero proves the node is there, so the
            // not-found probe is only spent when there is genuinely nothing to describe.
            if (resp.Edges.Count == 0 && resp.TotalEdges == 0 && !await NodeExistsAsync(handle, nodeId))
            {
                var suggestions = await SuggestAsync(handle, nodeId);
                return Envelope($"Node '{nodeId}' not found.",
                    suggestions.Length > 0 ? "Did you mean one of these?" : "Use resolve(query) to find the nodeId.",
                    "neighbors(handle, nodeId:\"OrderService\", direction:\"out\")",
                    suggestions.Length > 0 ? suggestions : null);
            }

            // Note before rows, deliberately. Nothing-matched and unknown-kind both arrive as an
            // empty list, and the reader stops at `count` — so the sentence that says which one it
            // is, and what to ask instead, is placed where it is read first. (It is dropped entirely
            // when there is nothing to say: WhenWritingNull, and a clean answer carries no note.)
            return JsonSerializer.Serialize(new
            {
                nodeId,
                direction,
                kind,
                note = resp.HasNote ? resp.Note : null,
                count = resp.Edges.Count,
                // The unfiltered edge count in this direction, and every kind that makes it up: what
                // `count` alone cannot say once a filter is on.
                totalEdges = resp.TotalEdges,
                kindsPresent,
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

    [McpServerTool]
    [Description("Find all usages (in-edges) of a node across the codebase, with file:line provenance. A short name or fuzzy query resolves, and ambiguity is reported rather than guessed. Example: usages(\"abc123\", query:\"IOrderRepository\")")]
    public async Task<string> Usages(
        [Description(HandleDoc)] string? handle = null,
        [Description(NodeIdDoc)] string? nodeId = null,
        [Description("Fuzzy name to resolve instead of a nodeId.")] string? query = null)
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

    [McpServerTool]
    [Description("Free-text search across graph nodes, paginated. Returns nodeIds the navigation tools take. Example: find(\"abc123\", \"Order\", kind:\"Type\", limit:10, cursor:0)")]
    public async Task<string> Find(
        [Description(HandleDoc)] string? handle = null,
        [Description("Search term - substring match over node titles.")] string? query = null,
        [Description("Filter to one node kind, e.g. Type, Member, Entry. Omit for all kinds.")] string? kind = null,
        [Description("Page size (default 20).")] int limit = 20,
        [Description("Offset into the result list, for paging (default 0).")] int cursor = 0)
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

    [McpServerTool]
    [Description("Transitive impact analysis: upward (what reaches this) or downward (what this affects), grouped by service. Address the symbol with 'nodeId' or 'query', or pass 'files' for diff-aware \"I changed X, what breaks\". Example: impact(\"abc123\", query:\"OrderService\", direction:\"down\"), impact(\"abc123\", files:[\"path/to/file.cs\"])")]
    public async Task<string> Impact(
        [Description(HandleDoc)] string? handle = null,
        [Description(NodeIdDoc)] string? nodeId = null,
        [Description("How many hops to walk (default 4).")] int maxDepth = 4,
        [Description("\"up\" (default, what reaches this), \"down\" (what this affects), or \"both\". Any other value is rejected.")] string direction = "up",
        [Description("Changed file paths for diff-aware impact, instead of a single symbol.")] string[]? files = null,
        [Description("Fuzzy name to resolve instead of a nodeId.")] string? query = null)
    {
        if (string.IsNullOrWhiteSpace(nodeId) && string.IsNullOrWhiteSpace(query) && (files is null || files.Length == 0))
            return Envelope("Provide 'nodeId'/'query' or 'files'.",
                "Give a symbol (nodeId or fuzzy query), or the changed files for diff-aware impact.",
                "impact(handle, query:\"OrderService\", direction:\"down\")");
        // MEASURED against DevContextGrpcService.GetImpact, not read off this file's own docs: the
        // server switch accepts up | down | both and defaults everything else to Up.
        if (RejectBadEnum("direction", direction, ["up", "down", "both"],
            "impact(handle, query:\"OrderService\", direction:\"down\")") is { } badImpactDir) return badImpactDir;
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

    [McpServerTool]
    [Description("The wiring path BETWEEN two symbols - how does 'from' reach 'to', hop by hop, with the seam kind on every hop. Answers \"does the checkout endpoint actually reach the payment service, and through what\". Returns the shortest paths; says so when the connection runs the other way round, and when the depth budget rather than the graph ended the search. Example: seam(handle, from:\"OrdersController\", to:\"OrderingContext\")")]
    public async Task<string> Seam(
        [Description(HandleDoc)] string? handle = null,
        [Description("The starting symbol - a nodeId or a resolvable name.")] string? from = null,
        [Description("The destination symbol - a nodeId or a resolvable name.")] string? to = null,
        [Description("Max hops to search before giving up (default 8).")] int maxDepth = 8,
        [Description("How many distinct paths to return (default 3).")] int maxPaths = 3)
    {
        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            return Envelope("Missing 'from' or 'to'.",
                "seam takes BOTH ends — a node id or a resolvable name for each.",
                "seam(handle, from:\"OrdersController\", to:\"OrderingContext\")");
        try { handle = ResolveHandle(handle); }
        catch (RpcException ex) { return FromRpc(ex, "seam", "analyze(path) then seam(from:\"A\", to:\"B\")"); }

        try
        {
            var req = new SeamRequest { Handle = handle, From = from, To = to };
            if (maxDepth > 0) req.MaxDepth = maxDepth;
            if (maxPaths > 0) req.MaxPaths = maxPaths;
            var resp = await _client.GetSeamAsync(req);

            // An end that resolved to nothing is a DIFFERENT answer from two ends that do not
            // connect, and the second is the one an agent would wrongly believe. The server names
            // which end failed by leaving its id empty; we turn that into candidates for that end.
            var unresolved = resp.FromNodeId.Length == 0 ? from : resp.ToNodeId.Length == 0 ? to : null;
            if (unresolved is not null)
            {
                var suggestions = await SuggestAsync(handle, unresolved);
                return Envelope($"'{unresolved}' did not resolve to a node — not the same as no path.",
                    suggestions.Length > 0 ? "Did you mean one of these?" : "Use resolve(query) to get an exact nodeId.",
                    "seam(handle, from:\"OrdersController\", to:\"OrderingContext\")",
                    suggestions.Length > 0 ? suggestions : null);
            }

            return JsonSerializer.Serialize(new
            {
                found = resp.Found,
                direction = resp.Direction,
                from = new { nodeId = resp.FromNodeId, title = TrimTitle(resp.FromTitle) },
                to = new { nodeId = resp.ToNodeId, title = TrimTitle(resp.ToTitle) },
                hops = resp.Hops,
                totalPaths = resp.TotalPaths,
                paths = resp.Paths.Select(p => p.Hops.Select(h => new
                {
                    from = TrimTitle(h.FromTitle),
                    to = TrimTitle(h.ToTitle),
                    kind = h.Kind,
                    resolution = h.Resolution,
                    fromNodeId = h.FromNodeId,
                    toNodeId = h.ToNodeId,
                    site = h.HasFilePath
                        ? Rel(handle, h.FilePath) + (h.HasLineNumber ? $":{h.LineNumber}" : "")
                        : null,
                }).ToArray()).ToArray(),
                note = resp.HasNote ? resp.Note : null,
            }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "seam", "seam(handle, from:\"OrdersController\", to:\"OrderingContext\")"); }
    }

    [McpServerTool]
    [SpecialistTool("ask it when the question is specifically about configuration keys")]
    [Description("Find config key usage sites (IConfiguration indexer, GetValue, GetSection, and Options-pattern bindings: AddOptions<T>().BindConfiguration / Configure<T>) with file:line. Example: config(\"abc123\", \"GrpcSettings:DiscountUrl\")")]
    public async Task<string> Config(
        [Description(HandleDoc)] string? handle = null,
        [Description("Config key to filter on, e.g. \"GrpcSettings:DiscountUrl\". Omit for every key found.")] string? key = null)
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
                method = "Scanned source for IConfiguration indexer / GetValue / GetSection / GetConnectionString with literal keys, plus Options-pattern bindings (AddOptions<T>().BindConfiguration/.Bind, Configure<T>) with literal or const section names. Dynamically-built keys are not captured.",
                keys = bindings,
            }, JsonOpts);
        }
        catch (RpcException ex) { return FromRpc(ex, "config", "config(handle, key:\"ConnectionStrings:Database\")"); }
    }

    [McpServerTool]
    [SpecialistTool("ask it when the question is specifically about test coverage of a symbol")]
    [Description("Best-effort: find test methods whose call closure reaches a node. Method: walks in-edges up to maxDepth and classifies callers as tests by name/path/project. 0 results means none reached in the graph, NOT \"untested\". Example: tests_for(\"abc123\", query:\"OrderService\")")]
    public async Task<string> TestsFor(
        [Description(HandleDoc)] string? handle = null,
        [Description(NodeIdDoc)] string? nodeId = null,
        [Description("How many in-edge hops to walk (default 6).")] int maxDepth = 6,
        [Description("Fuzzy name to resolve instead of a nodeId.")] string? query = null)
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

    // G2.1 (R4 §1 item 11) — `insights` is FOLDED into stats(). Both read the SAME StatsResponse
    // from the same RPC, and stats already serialised every insight field but one; `confidence` now
    // rides with them, so the fold drops nothing.

    [McpServerTool]
    [Description("Budget-priced context pack for a focus: the identity, trace, signatures and usage sections an agent needs to work on one thing, cut to fit budgetTokens. Anything elided is named in the reply, and raising budgetTokens brings it back. Example: get_context(\"abc123\", \"POST /api/orders\", 4000, \"trace\")")]
    public async Task<string> GetContext(
        [Description(HandleDoc)] string? handle = null,
        [Description("What to build the pack around: an entry route, a symbol name, or a nodeId.")] string? focus = null,
        [Description("Token budget for the pack (default 8000). Raise it to un-elide sections the reply says were cut.")] int budgetTokens = 8000,
        [Description("\"trace\" (default), \"explain\", or \"review\" - shifts which sections get the budget. Any other value is rejected.")] string intent = "trace",
        [Description("Synonym for 'focus'.")] string? query = null)
    {
        focus ??= query; // T3.1 — accept `query` as a synonym for `focus`
        if (string.IsNullOrWhiteSpace(focus))
            return Envelope("Missing required parameter 'focus' (or 'query').",
                "Pass the entry/symbol to build context for as 'focus' or 'query'.",
                "get_context(handle, focus:\"POST /api/orders\")");
        if (RejectBadEnum("intent", intent, ["trace", "explain", "review"],
            "get_context(handle, focus:\"POST /api/orders\", intent:\"explain\")") is { } badIntent) return badIntent;
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
            // T1.3 (BUG-BACKLOG #9) — two different claims used to be welded to one number. "fill %"
            // is totalTokens/budgetTokens and legitimately FALLS as the budget rises; "the pack is
            // complete" is a property of what got cut. Reading completeness off the ratio produced
            // the silent-wrong-answer shape: MEASURED on TodoApi, get_context("Extensions", 1500)
            // rendered "… (+64 lines)" in its own content and said "the pack already contains
            // everything reachable from this focus" — the one reply that should say "ask for more"
            // was the one that said "there is no more" (raising to 20000 doubled the content).
            // The completeness half is now ANSWERED BY THE PACK: the builder declares every cut it
            // made on an `elided ` line, so this reads a fact instead of inferring one from a ratio.
            string? fillNote = null;
            object[]? suggestedFocuses = null;
            var fillPct = budgetTokens > 0 ? (int)(resp.TotalTokens * 100L / budgetTokens) : 100;
            var elisions = resp.Omitted.Where(o => o.StartsWith(ElidedPrefix, StringComparison.Ordinal)).ToArray();
            var legacyCut = resp.Omitted.Any(o => o.Contains("trimmed", StringComparison.OrdinalIgnoreCase));
            if (elisions.Length > 0 || legacyCut)
            {
                // Named at ANY fill: an elision at 90% fill is still content the agent did not get.
                fillNote = $"fill {fillPct}%: INCOMPLETE — {elisions.Length + (legacyCut ? 1 : 0)} cut(s) were made to fit "
                    + $"budgetTokens={budgetTokens}; each is named in omitted[]. Raise budgetTokens for the rest.";
            }
            else if (fillPct < 85)
            {
                fillNote = $"fill {fillPct}%: the pack already contains everything reachable from this focus — its connected subgraph is small (not an error; a smaller budget fits it).";
                suggestedFocuses = await SuggestConnectedAsync(handle, focus);
                if (suggestedFocuses.Length > 0)
                    fillNote += " Better-connected focuses in suggestedFocuses.";
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

    [McpServerTool]
    [SpecialistTool("ask it when a pack has been open long enough that the files behind it may have changed")]
    [Description("Has the source drifted since analyze? Compares analyze-time file hashes against disk, per pack section for a focus. Returns per-section stale flags, changed files with line deltas, and repo HEAD then/now. Method: hash + line-count delta only, no diff. Example: verify_context(\"abc123\", \"POST /api/orders\")")]
    public async Task<string> VerifyContext(
        [Description(HandleDoc)] string? handle = null,
        [Description("The same focus you passed to get_context.")] string? focus = null,
        [Description("Same budget as the get_context call being verified (default 8000), so the same sections are compared.")] int budgetTokens = 8000,
        [Description("Synonym for 'focus'.")] string? query = null)
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

    [McpServerTool]
    [Description("Read the source code behind a node, with its real file:line. Example: read_source(\"abc123\", query:\"OrderService\", mode:\"member\")")]
    public async Task<string> ReadSource(
        [Description(HandleDoc)] string? handle = null,
        [Description(NodeIdDoc)] string? nodeId = null,
        [Description("Lines of context around the node in window mode (default 20).")] int windowLines = 20,
        [Description("\"window\" (default, windowLines around the node) or \"member\" (the whole declaration body). Any other value is rejected.")] string mode = "window",
        [Description("Fuzzy name to resolve instead of a nodeId.")] string? query = null)
    {
        if (string.IsNullOrWhiteSpace(nodeId) && string.IsNullOrWhiteSpace(query))
            return Envelope("Missing 'nodeId' or 'query'.",
                "Pass a nodeId (or fuzzy query). mode: window|member.",
                "read_source(handle, query:\"OrderService\", mode:\"member\")");
        if (RejectBadEnum("mode", mode, ["window", "member"],
            "read_source(handle, query:\"OrderService\", mode:\"member\")") is { } badMode) return badMode;
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
