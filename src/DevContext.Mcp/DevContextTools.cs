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

    /// <summary>Start analysis of a .NET repo. Returns a handle for subsequent calls. Example: analyze("C:/repos/MyApp")</summary>
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

        // G4 — store repo root for path relativization
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
            hint = "Analysis complete. Use other tools with this handle.",
        }, JsonOpts);
    }

    /// <summary>One-call repo brief: identity, services, ServiceLinks, top flows, where to start. ~600 tokens max. Example: overview("abc123")</summary>
    [McpServerTool]
    public async Task<string> Overview(string handle)
    {
        var map = await _client.GetMapAsync(new SessionRequest { Handle = handle });
        var stats = await _client.GetStatsAsync(new SessionRequest { Handle = handle });
        var entries = await _client.ListEntryPointsAsync(new SessionRequest { Handle = handle });

        var sb = new StringBuilder();

        sb.Append(map.Archetype);
        if (map.ServiceStyles.Count > 0)
        {
            var services = map.ServiceStyles.Select(s => $"{s.ProjectName} ({s.Style})");
            sb.Append(": ").AppendJoin(", ", services);
            sb.AppendLine();
        }
        sb.Append("  ").Append(stats.Graph?.Nodes ?? 0).Append(" nodes · ");
        sb.Append(stats.Graph?.Edges ?? 0).Append(" edges · ");
        sb.Append(stats.Graph?.Entries ?? 0).Append(" entries · ");
        sb.Append(map.ProjectCount).AppendLine(" projects");

        var top = entries.EntryPoints
            .OrderByDescending(e => e.Score)
            .ThenBy(e => e.HasTarget ? 0 : 1)
            .Take(5).ToArray();
        if (top.Length > 0)
        {
            sb.AppendLine("Top flows:");
            foreach (var e in top)
            {
                var label = e.HasRoute ? $"{e.HttpMethod} {e.Route}" : e.Title;
                sb.Append("  ").Append(e.Kind).Append(": ").Append(label);
                if (e.HasTarget) sb.Append(" → ").Append(e.Target);
                sb.AppendLine();
            }
        }

        if (map.Topology.Count > 0)
        {
            sb.AppendLine("Projects:");
            foreach (var t in map.Topology.Take(8))
            {
                sb.Append("  ").Append(t.Name);
                if (t.DependsOn.Count > 0)
                    sb.Append(" → ").AppendJoin(", ", t.DependsOn.Take(3));
                sb.AppendLine();
            }
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
    public async Task<string> Resolve(string handle, string query, int limit = 10)
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
    public async Task<string> Status(string handle)
    {
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
    public async Task<string> CloseSession(string handle)
    {
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
    public async Task<string> Stats(string handle)
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

    /// <summary>List entry points (HTTP routes, bus consumers, gRPC services). Filter by kind. Example: entrypoints("abc123", "HttpEndpoint")</summary>
    [McpServerTool]
    public async Task<string> Entrypoints(string handle, string? kind = null)
    {
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
    public async Task<string> Map(string handle)
    {
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
    public async Task<string> TopFlows(string handle)
    {
        var resp = await _client.ListEntryPointsAsync(new SessionRequest { Handle = handle });
        var entries = resp.EntryPoints
            .OrderByDescending(e => e.Score)
            .ThenBy(e => e.HasTarget ? 0 : 1)
            .Take(20)
            .ToArray();
        return JsonSerializer.Serialize(new
        {
            count = entries.Length,
            topFlows = entries.Select(e => new
            {
                kind = e.Kind,
                title = e.Title,
                nodeId = e.NodeId,
                route = e.HasRoute ? e.Route : null,
                httpMethod = e.HasHttpMethod ? e.HttpMethod : null,
                target = e.HasTarget ? e.Target : null,
                groupPath = e.HasGroupPath ? e.GroupPath : null,
                score = e.Score,
            }).ToArray(),
        }, JsonOpts);
    }

    /// <summary>Archetype-aware starting points for exploring the codebase. Example: interesting_points("abc123")</summary>
    [McpServerTool]
    public async Task<string> InterestingPoints(string handle)
    {
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
    public async Task<string> Trace(string handle, string focus, int depth = 6, string format = "default")
    {
        try
        {
            var resp = await _client.GetTraceAsync(new TraceRequest
            {
                Handle = handle,
                Focus = focus,
                Depth = depth,
            });
            if (!resp.Found)
                return JsonSerializer.Serialize(new { found = false, focus }, JsonOpts);

            if (format == "compact")
            {
                // M4.3 — compact flow: indented text with file:line per step, seam glyphs
                var sb = new StringBuilder();
                var entryInfo = resp.Root is not null
                    ? $"{resp.Root.Kind}: {resp.Root.Title}" : focus;
                sb.Append("Entry: ").AppendLine(entryInfo);
                if (resp.Root is not null)
                    BuildCompactFlow(sb, resp.Root, 0);
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
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return JsonSerializer.Serialize(new { found = false, focus, error = ex.Message }, JsonOpts);
        }
    }

    // M4.3 — compact flow: indented text with seam glyphs and provenance
    private static void BuildCompactFlow(StringBuilder sb, TraceNode node, int indent)
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
        sb.AppendLine();

        foreach (var child in node.Children)
            BuildCompactFlow(sb, child, indent + 1);
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
    public async Task<string> Node(string handle, string nodeId)
    {
        var resp = await _client.GetNodeAsync(new NodeRequest { Handle = handle, NodeId = nodeId });
        if (!resp.Found)
            return JsonSerializer.Serialize(new { found = false, query = nodeId }, JsonOpts);

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

    /// <summary>Outgoing/incoming edges of a node. direction: out|in|usages. Example: neighbors("abc123", "OrderService", "out")</summary>
    [McpServerTool]
    public async Task<string> Neighbors(string handle, string nodeId, string direction = "out")
    {
        var resp = await _client.GetNeighborsAsync(new NeighborsRequest
        {
            Handle = handle,
            NodeId = nodeId,
            Direction = direction,
        });
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

    /// <summary>Find all usages (in-edges) of a node across the codebase. Example: usages("abc123", "IOrderRepository")</summary>
    [McpServerTool]
    public async Task<string> Usages(string handle, string nodeId)
    {
        var resp = await _client.GetNeighborsAsync(new NeighborsRequest
        {
            Handle = handle,
            NodeId = nodeId,
            Direction = "usages",
        });
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

    /// <summary>Free-text search across graph nodes, paginated. limit default 20, cursor for offset. Example: find("abc123", "Order", kind:"Type", limit:10, cursor:0)</summary>
    [McpServerTool]
    public async Task<string> Find(string handle, string query, string? kind = null, int limit = 20, int cursor = 0)
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

    /// <summary>Blast-radius analysis: which entries reach this node? maxDepth 1-10, default 4. Example: impact("abc123", "OrderService", 4)</summary>
    [McpServerTool]
    public async Task<string> Impact(string handle, string nodeId, int maxDepth = 4)
    {
        var resp = await _client.GetImpactAsync(new ImpactRequest
        {
            Handle = handle,
            NodeId = nodeId,
            MaxDepth = maxDepth,
        });
        return JsonSerializer.Serialize(new
        {
            nodeId,
            maxDepth,
            count = resp.Results.Count,
            results = resp.Results.Select(r => new
            {
                entryTitle = r.EntryTitle,
                kind = r.Kind,
                hops = r.Hops,
            }).ToArray(),
        }, JsonOpts);
    }

    /// <summary>List all insights (warnings, notable items, info) for the analyzed repo. Example: insights("abc123")</summary>
    [McpServerTool]
    public async Task<string> Insights(string handle)
    {
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
    public async Task<string> GetContext(string handle, string focus, int budgetTokens = 8000, string intent = "trace")
    {
        var resp = await _client.GetContextAsync(new ContextRequest
        {
            Handle = handle,
            Focus = focus,
            BudgetTokens = budgetTokens,
            Intent = intent,
        });
        if (!resp.Found)
            return JsonSerializer.Serialize(new { found = false, focus }, JsonOpts);

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

    /// <summary>Read source code for a node. mode: window (default, windowLines lines around) | member (full declaration body). Example: read_source("abc123", "OrderService", 30, "member")</summary>
    [McpServerTool]
    public async Task<string> ReadSource(string handle, string nodeId, int windowLines = 20, string mode = "window")
    {
        var resp = await _client.GetNodeAsync(new NodeRequest { Handle = handle, NodeId = nodeId });
        if (!resp.Found || !resp.HasFilePath)
            return JsonSerializer.Serialize(new { found = false, query = nodeId }, JsonOpts);

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
