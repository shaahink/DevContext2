using DevContext.Core.Graph;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace DevContext.Mcp;

[McpServerToolType]
public sealed class DevContextTools
{
    private readonly McpSessionManager _sessions;
    private readonly ILogger<DevContextTools> _logger;

    public DevContextTools(McpSessionManager sessions, ILogger<DevContextTools> logger)
    {
        _sessions = sessions;
        _logger = logger;
    }

    private McpAnalysisSession Require(string handle)
    {
        var session = _sessions.GetSession(handle);
        if (session is null)
            throw new InvalidOperationException(
                $"No ready session for handle '{handle}'. Run analyze first, or check status().");
        return session;
    }

    [McpServerTool]
    public string Analyze(string path)
    {
        var handle = _sessions.StartAnalysis(path);
        return McpSessionManager.Serialize(new
        {
            handle,
            status = "analyzing",
            hint = "Use status(handle) to check progress.",
        });
    }

    [McpServerTool]
    public string Status(string handle)
    {
        var status = _sessions.GetStatus(handle);
        return McpSessionManager.Serialize(status);
    }

    [McpServerTool]
    public string CloseSession(string handle)
    {
        var removed = _sessions.CloseSession(handle);
        return McpSessionManager.Serialize(new { closed = removed, handle });
    }

    [McpServerTool]
    public string ListSessions()
    {
        var sessions = _sessions.ListSessions();
        return McpSessionManager.Serialize(new { count = sessions.Length, sessions });
    }

    [McpServerTool]
    public string Stats(string handle)
    {
        var s = Require(handle);
        var (seams, entriesWithTarget) = s.Query.Stats();
        var envelope = s.Envelope();
        var summary = _sessions.GetStatus(handle);
        return McpSessionManager.Serialize(new
        {
            envelope.Scope, envelope.Coverage, envelope.Confidence,
            projectCount = s.Snapshot.Map?.Topology.Length ?? 0,
            nodeCount = s.Snapshot.Graph?.NodeCount ?? 0,
            edgeCount = s.Snapshot.Graph?.EdgeCount ?? 0,
            entryCount = s.Snapshot.Entries.Length,
            entriesWithTarget,
            targetCoverage = s.Snapshot.Entries.Length > 0
                ? (double)entriesWithTarget / s.Snapshot.Entries.Length
                : 0,
            seams = seams.Select(seam => new
            {
                kind = seam.Seam,
                total = seam.Count,
                verified = seam.Count - seam.Approx,
                approx = seam.Approx,
            }).ToArray(),
            archetype = s.Snapshot.Map?.Archetype.ToString() ?? "unknown",
            style = s.Snapshot.Map?.Style ?? "Unknown",
            elapsedMs = summary.ElapsedMs,
            warnings = summary.Summary?.Warnings?.ToArray() ?? [],
        });
    }

    [McpServerTool]
    public string Entrypoints(string handle, string? kind = null)
    {
        var s = Require(handle);
        EntryPointKind? kindFilter = kind is not null && Enum.TryParse<EntryPointKind>(kind, ignoreCase: true, out var parsed)
            ? parsed
            : null;
        var entries = s.Query.EntryPoints(kindFilter);
        var envelope = s.Envelope();
        return McpSessionManager.Serialize(new
        {
            envelope.Scope, envelope.Coverage, envelope.Confidence,
            count = entries.Length,
            kind = kindFilter?.ToString(),
            entries = entries.Select(e => new
            {
                kind = e.Kind.ToString(),
                title = e.Title,
                nodeId = e.Node.Key,
                httpMethod = e.HttpMethod,
                route = e.Route,
                project = e.Project,
                target = e.Target,
                groupPath = e.GroupPath,
                authAttributes = e.AuthAttributes.IsDefaultOrEmpty
                    ? Array.Empty<string>()
                    : e.AuthAttributes.ToArray(),
                score = e.Score,
                reach = e.Reach,
                crossProjects = e.CrossProjects,
            }).ToArray(),
        });
    }

    [McpServerTool]
    public string Map(string handle)
    {
        var s = Require(handle);
        var map = s.Query.Map();
        var envelope = s.Envelope();
        var archetype = map?.Archetype.ToString().ToLowerInvariant() ?? "unknown";
        return McpSessionManager.Serialize(new
        {
            envelope.Scope, envelope.Coverage, envelope.Confidence,
            archetype,
            style = map?.Style ?? "Unknown",
            styleConfidence = map?.StyleConfidence ?? 0,
            projectCount = map?.Topology.Length ?? 0,
            topology = map?.Topology.Select(t => new { name = t.Name, dependsOn = t.DependsOn.ToArray() }).ToArray(),
        });
    }

    [McpServerTool]
    public string TopFlows(string handle)
    {
        var s = Require(handle);
        var entries = s.Query.EntryPoints()
            .OrderByDescending(e => e.Score)
            .ThenBy(e => e.Target is not null ? 0 : 1)
            .Take(20)
            .ToArray();
        var envelope = s.Envelope();
        return McpSessionManager.Serialize(new
        {
            envelope.Scope, envelope.Coverage, envelope.Confidence,
            count = entries.Length,
            topFlows = entries.Select(e => new
            {
                kind = e.Kind.ToString(),
                title = e.Title,
                nodeId = e.Node.Key,
                route = e.Route,
                httpMethod = e.HttpMethod,
                target = e.Target,
                groupPath = e.GroupPath,
                score = e.Score,
            }).ToArray(),
        });
    }

    [McpServerTool]
    public string InterestingPoints(string handle)
    {
        var s = Require(handle);
        var archetypeStr = s.Query.Map()?.Archetype.ToString().ToLowerInvariant();
        var points = s.Query.GetInterestingPoints(archetypeStr);
        var envelope = s.Envelope();
        return McpSessionManager.Serialize(new
        {
            envelope.Scope, envelope.Confidence,
            archetype = archetypeStr ?? "unknown",
            count = points.Length,
            points = points.Select(p => new
            {
                nodeId = p.Id.Key,
                title = p.Title,
                kind = p.Kind.ToString(),
                why = p.Why,
                tags = p.Tags.ToArray(),
            }).ToArray(),
        });
    }

    [McpServerTool]
    public string Trace(string handle, string focus, int depth = 6)
    {
        var s = Require(handle);
        var trace = s.Query.Trace(focus, depth);
        if (trace is null)
            return McpSessionManager.Serialize(new { found = false, focus });

        var envelope = s.Envelope();
        return McpSessionManager.Serialize(new
        {
            found = true,
            envelope.Scope, envelope.Confidence,
            entry = new
            {
                kind = trace.Entry.Kind.ToString(),
                title = trace.Entry.Title,
                nodeId = trace.Entry.Node.Key,
            },
            root = SerializeTraceStep(trace.Root),
            touchedEntities = trace.TouchedEntities.ToArray(),
            emittedEvents = trace.EmittedEvents.ToArray(),
        });
    }

    private static object SerializeTraceStep(TraceStep step) => new
    {
        nodeId = step.Node.Id.Key,
        title = step.Node.Title,
        kind = step.Node.Kind.ToString(),
        seam = step.Seam.ToString(),
        depth = step.Depth,
        resolution = step.Resolution.ToString(),
        provenance = step.Provenance,
        truncated = step.Truncated,
        omitted = step.Omitted,
        salient = step.Salient.ToArray(),
        pipeline = step.Pipeline.ToArray(),
        tags = step.Node.Tags.ToArray(),
        multiImplCount = step.MultiImplCount,
        children = step.Children.Select(SerializeTraceStep).ToArray(),
    };

    [McpServerTool]
    public string Node(string handle, string nodeId)
    {
        var s = Require(handle);
        var id = s.Query.ResolveNodeId(nodeId);
        if (id is null)
            return McpSessionManager.Serialize(new { found = false, query = nodeId });

        var node = s.Query.Node(id.Value);
        if (node is null)
            return McpSessionManager.Serialize(new { found = false, nodeId = id.Value.Key });

        var envelope = s.Envelope();
        return McpSessionManager.Serialize(new
        {
            found = true,
            envelope.Scope, envelope.Confidence,
            nodeId = node.Id.Key,
            title = node.Title,
            kind = node.Kind.ToString(),
            tags = node.Tags.ToArray(),
            filePath = node.FilePath,
            lineNumber = node.LineNumber,
            outDegree = node.OutDegree,
            inDegree = node.InDegree,
        });
    }

    [McpServerTool]
    public string Neighbors(string handle, string nodeId, string direction = "out")
    {
        var s = Require(handle);
        var id = s.Query.ResolveNodeId(nodeId);
        if (id is null)
            return McpSessionManager.Serialize(new { found = false, query = nodeId });

        var dir = string.Equals(direction, "in", StringComparison.OrdinalIgnoreCase)
            ? EdgeDirection.In
            : EdgeDirection.Out;

        var edges = s.Query.Neighbors(id.Value, dir);
        var envelope = s.Envelope();
        return McpSessionManager.Serialize(new
        {
            envelope.Scope, envelope.Confidence,
            nodeId = id.Value.Key,
            direction = dir.ToString().ToLowerInvariant(),
            count = edges.Length,
            edges = edges.Select(e => new
            {
                from = e.From.Key,
                to = e.To.Key,
                kind = e.Kind.ToString(),
                resolution = e.Resolution.ToString(),
                otherTitle = e.OtherTitle,
                provenance = e.Provenance,
            }).ToArray(),
        });
    }

    [McpServerTool]
    public string Usages(string handle, string nodeId)
    {
        var s = Require(handle);
        var id = s.Query.ResolveNodeId(nodeId);
        if (id is null)
            return McpSessionManager.Serialize(new { found = false, query = nodeId });

        var edges = s.Query.FindUsages(id.Value);
        var envelope = s.Envelope();
        return McpSessionManager.Serialize(new
        {
            envelope.Scope, envelope.Confidence,
            nodeId = id.Value.Key,
            count = edges.Length,
            usages = edges.Select(e => new
            {
                caller = e.From.Key,
                kind = e.Kind.ToString(),
                otherTitle = e.OtherTitle,
                provenance = e.Provenance,
            }).ToArray(),
        });
    }

    [McpServerTool]
    public string Search(string handle, string query)
    {
        var s = Require(handle);
        var hits = s.Query.Search(query);
        var envelope = s.Envelope();
        return McpSessionManager.Serialize(new
        {
            envelope.Scope, envelope.Confidence,
            query,
            count = hits.Length,
            results = hits.Select(h => new
            {
                nodeId = h.Id.Key,
                title = h.Title,
                kind = h.Kind.ToString(),
                degree = h.Degree,
            }).ToArray(),
        });
    }

    [McpServerTool]
    public string Impact(string handle, string nodeId, int maxDepth = 4)
    {
        var s = Require(handle);
        var id = s.Query.ResolveNodeId(nodeId);
        if (id is null)
            return McpSessionManager.Serialize(new { found = false, query = nodeId });

        var results = s.Query.BlastRadius(id.Value, maxDepth);
        var envelope = s.Envelope();
        return McpSessionManager.Serialize(new
        {
            envelope.Scope, envelope.Confidence,
            nodeId = id.Value.Key,
            maxDepth,
            count = results.Length,
            results = results.Select(r => new
            {
                entryTitle = r.EntryTitle,
                kind = r.Kind,
                hops = r.Hops,
            }).ToArray(),
        });
    }

    [McpServerTool]
    public string Insights(string handle)
    {
        var s = Require(handle);
        var insights = s.Snapshot.Insights;
        var envelope = s.Envelope();
        return McpSessionManager.Serialize(new
        {
            envelope.Scope, envelope.Coverage, envelope.Confidence,
            count = insights.Length,
            insights = insights.Select(i => new
            {
                id = i.Id,
                category = i.Category.ToString(),
                severity = i.Severity.ToString(),
                title = i.Title,
                detail = string.Join("; ", i.Evidence.Take(3)),
                evidence = i.Evidence.ToArray(),
                confidence = i.Confidence,
                confidenceBasis = i.ConfidenceBasis,
                whyItMatters = i.WhyItMatters,
                action = i.Action.ToString(),
                actionTarget = i.ActionTarget,
                jumpOff = i.JumpOff,
            }).ToArray(),
        });
    }

    [McpServerTool]
    public string GetContext(string handle, string focus, int budgetTokens = 8000, string intent = "trace")
    {
        var s = Require(handle);
        var builder = new ContextPackBuilder(s.Query, s.Snapshot);
        var pack = builder.Build(focus, budgetTokens, intent);
        var envelope = s.Envelope();
        return McpSessionManager.Serialize(new
        {
            envelope.Scope, envelope.Confidence,
            focus,
            budgetTokens,
            totalTokens = pack.TotalTokens,
            sections = pack.Sections.Select(sec => new { section = sec.Section, tokens = sec.Tokens }).ToArray(),
            omitted = pack.Omitted.ToArray(),
            content = pack.Content,
        });
    }

    [McpServerTool]
    public string ReadSource(string handle, string nodeId, int windowLines = 20)
    {
        var s = Require(handle);
        var id = s.Query.ResolveNodeId(nodeId);
        if (id is null)
            return McpSessionManager.Serialize(new { found = false, query = nodeId });

        var node = s.Query.Node(id.Value);
        if (node is null || node.FilePath is null)
            return McpSessionManager.Serialize(new { found = false, nodeId = id.Value.Key, error = "Node has no file path." });

        try
        {
            if (!File.Exists(node.FilePath))
                return McpSessionManager.Serialize(new
                {
                    found = true,
                    nodeId = node.Id.Key,
                    filePath = node.FilePath,
                    error = "File not found on disk.",
                });

            var lines = File.ReadAllLines(node.FilePath);
            var lineNum = node.LineNumber ?? 1;
            var before = Math.Max(0, windowLines / 3);
            var after = windowLines - before;
            var start = Math.Max(0, lineNum - before);
            var end = Math.Min(lines.Length, lineNum + after);
            var selectedLines = lines.Skip(start).Take(end - start).ToArray();

            return McpSessionManager.Serialize(new
            {
                found = true,
                nodeId = node.Id.Key,
                title = node.Title,
                filePath = node.FilePath,
                lineNumber = lineNum,
                startLine = start + 1,
                endLine = end,
                totalLines = lines.Length,
                content = string.Join("\n", selectedLines),
            });
        }
        catch (Exception ex)
        {
            return McpSessionManager.Serialize(new
            {
                found = true,
                nodeId = node.Id.Key,
                filePath = node.FilePath,
                error = ex.Message,
            });
        }
    }
}
