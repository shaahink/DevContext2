using System.Text.Json;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace DevContext.Mcp;

// L5.2 — a call to a tool that does not exist must teach, not stonewall.
// The SDK routes any name absent from the tool collection here (calls like `routes`
// used to surface as an opaque "Unknown tool" error with no next step). We return
// an actionable envelope naming the closest real tools + the full list.
internal static class UnknownToolHandler
{
    // The 23 tool names the server exposes (McpServerTool methods on DevContextTools).
    private static readonly string[] ToolNames =
    [
        "analyze", "overview", "resolve", "status", "close_session", "list_sessions",
        "stats", "entrypoints", "map", "top_flows", "flow", "interesting_points",
        "trace", "node", "neighbors", "usages", "find", "impact", "config",
        "tests_for", "insights", "get_context", "read_source",
    ];

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

    public static ValueTask<CallToolResult> Handle(
        RequestContext<CallToolRequestParams> request, CancellationToken ct)
    {
        var name = request.Params?.Name ?? "(none)";
        var closest = Closest(name);

        var payload = JsonSerializer.Serialize(new
        {
            error = $"Unknown tool '{name}'.",
            hint = closest is not null
                ? $"Did you mean '{closest}'? See availableTools."
                : "Use one of availableTools.",
            example = $"{closest ?? "overview"}(handle)",
            availableTools = ToolNames,
        }, JsonOpts);

        return ValueTask.FromResult(new CallToolResult
        {
            IsError = true,
            Content = [new TextContentBlock { Text = payload }],
        });
    }

    // Cheap nearest-name: shared-prefix / substring, else Levenshtein <= 3.
    private static string? Closest(string name)
    {
        var lower = name.ToLowerInvariant();
        string? best = null;
        var bestDist = int.MaxValue;
        foreach (var t in ToolNames)
        {
            if (t == lower) return t;
            if (t.Contains(lower, StringComparison.Ordinal) || lower.Contains(t, StringComparison.Ordinal))
                return t;
            var d = Levenshtein(lower, t);
            if (d < bestDist) { bestDist = d; best = t; }
        }
        return bestDist <= 3 ? best : null;
    }

    private static int Levenshtein(string a, string b)
    {
        var d = new int[a.Length + 1, b.Length + 1];
        for (var i = 0; i <= a.Length; i++) d[i, 0] = i;
        for (var j = 0; j <= b.Length; j++) d[0, j] = j;
        for (var i = 1; i <= a.Length; i++)
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        return d[a.Length, b.Length];
    }
}
