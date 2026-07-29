using System.Text.Json;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace DevContext.Mcp;

// L5.2 — a call to a tool that does not exist must teach, not stonewall.
// The SDK routes any name absent from the tool collection here (calls like `routes`
// used to surface as an opaque "Unknown tool" error with no next step). We return
// an actionable envelope naming the closest real tools + the full list.
// Public, not internal: McpToolMenuTests asserts the advertised menu against the SDK's registered
// collection, and `InternalsVisibleTo` is not available here — this project and DevContext.Server
// both compile a top-level `Program` into the global namespace, so opening internals to the shared
// test assembly makes that name ambiguous (CS0433).
public static class UnknownToolHandler
{
    // G2.1 (R4 §1 item 11) — THE SECOND HAND-MAINTAINED TOOL LIST IS GONE.
    //
    // This used to be a literal array of 24 names sitting beside the 24 [McpServerTool] methods
    // that produced them. It was correct the day it was written and had no way to STAY correct:
    // the moment three tools were folded away it went on advertising three tools the server no
    // longer has, and an agent that trusted the list would call one and land right back here.
    // A list whose only defence is that someone remembers to edit it is a list that drifts.
    //
    // It is now seeded once at startup from the SDK's own registered tool collection — the same
    // object `tools/list` is built from (Program.cs) — so the menu and the did-you-mean answer
    // are one fact. McpToolMenuTests pins the equality.
    private static string[] _toolNames = [];

    /// <summary>Seed the advertised names from the server's registered tools. Called once at startup.</summary>
    public static void UseToolNames(IEnumerable<string> names)
        => _toolNames = [.. names.Distinct(StringComparer.Ordinal).OrderBy(n => n, StringComparer.Ordinal)];

    /// <summary>The names this handler advertises — the server's real menu.</summary>
    public static IReadOnlyList<string> ToolNames => _toolNames;

    // Tools that were FOLDED into another tool, with the call that replaces them. This is not a
    // second menu — every entry names a tool that no longer exists, and the test asserts exactly
    // that. Without it the nearest-name heuristic below answers `flow` with `top_flows`, because
    // "flow" is a SUBSTRING of "top_flows": a confident, wrong redirect. An agent carrying an older
    // prompt gets the working call instead.
    public static readonly (string Retired, string Replacement, string Call)[] Folded =
    [
        ("flow", "trace", "trace(handle, focus:\"POST /basket/checkout\", format:\"compact\")"),
        ("insights", "stats", "stats(handle)"),
        ("interesting_points", "overview", "overview(handle)"),
    ];

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

    public static ValueTask<CallToolResult> Handle(
        RequestContext<CallToolRequestParams> request, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(Describe(request.Params?.Name ?? "(none)"), JsonOpts);

        return ValueTask.FromResult(new CallToolResult
        {
            IsError = true,
            Content = [new TextContentBlock { Text = payload }],
        });
    }

    /// <summary>The reply body. Separated from the transport so it can be asserted without a server.</summary>
    public static object Describe(string name)
    {
        var lower = name.ToLowerInvariant();

        foreach (var fold in Folded)
        {
            if (fold.Retired != lower) continue;
            return new
            {
                error = $"Unknown tool '{name}'.",
                hint = $"'{fold.Retired}' was folded into '{fold.Replacement}' — same answer, one fewer tool. Use {fold.Call}",
                example = fold.Call,
                availableTools = _toolNames,
            };
        }

        var closest = Closest(lower);
        return new
        {
            error = $"Unknown tool '{name}'.",
            hint = closest is not null ? $"Did you mean '{closest}'? See availableTools."
                : _toolNames.Length > 0 ? "Use one of availableTools."
                : "Call tools/list for the current menu.",
            example = $"{closest ?? "overview"}(handle)",
            availableTools = _toolNames,
        };
    }

    // Cheap nearest-name: shared-prefix / substring, else Levenshtein <= 3.
    private static string? Closest(string lower)
    {
        string? best = null;
        var bestDist = int.MaxValue;
        foreach (var t in _toolNames)
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
