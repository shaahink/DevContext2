using System.Text;
using System.Text.Json;

using DevContext.Protos;

using Grpc.Core;
using Grpc.Core.Interceptors;

using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace DevContext.Mcp;

/// <summary>
/// N4.3 (STUDIO-MCP audit §4, Room 2: "the feed in the agent's vocabulary") — what the AGENT
/// asked for, carried down to the server that answers it.
///
/// <para>The desktop's live feed is keyed on the gRPC method the server ran, because that is the
/// only name the server ever knew: <c>GetTrace</c>, <c>GetContextPack</c>. An agent asked for
/// <c>trace</c> and <c>get_context</c>, and one MCP verb can be several RPCs. Watching an agent
/// through a feed spelled in the other vocabulary is most of why the audit called the page an
/// observation deck with the instruments facing the wrong way.</para>
///
/// <para>The audit offered two mechanisms: map server-side, or wrap at the tool layer. Mapping
/// server-side means a table of gRPC-method → MCP-name kept next to, and drifting from, the real
/// tools — the exact defect BUG-BACKLOG #4 was. So the name travels FROM the tool layer instead:
/// this scope is set once per <c>tools/call</c> around the invocation, and every gRPC call made
/// underneath carries it in a header. A tool that makes three RPCs tags all three.</para>
/// </summary>
public static class McpCallScope
{
    /// <summary>What a feed row needs to say what an agent asked for.</summary>
    public sealed record Call(string Tool, string ArgsDigest, string PrimaryArg);

    private static readonly AsyncLocal<Call?> CurrentCall = new();

    /// <summary>The call being served on this async flow, or null outside one.</summary>
    public static Call? Current => CurrentCall.Value;

    /// <summary>
    /// The argument names, in priority order, that identify WHAT a call was about — the value the
    /// desktop deep-links on. Ordered so the most specific wins: an explicit node id beats the
    /// focus string that resolved to it.
    /// </summary>
    private static readonly string[] PrimaryArgNames =
        ["nodeId", "focus", "query", "symbol", "key", "path"];

    /// <summary>Names never worth showing: they identify the session, not the question.</summary>
    private static readonly HashSet<string> Uninteresting =
        new(StringComparer.OrdinalIgnoreCase) { "handle" };

    /// <summary>How much of the digest survives — a feed row, not a log line.</summary>
    private const int MaxDigestLength = 80;

    private static IDisposable Begin(Call call)
    {
        CurrentCall.Value = call;
        return new Scope();
    }

    /// <summary>Open a scope for one tools/call, deriving the digest from the arguments as sent.</summary>
    public static IDisposable Begin(string tool, IDictionary<string, JsonElement>? arguments)
        => Begin(new Call(tool, Digest(arguments), PrimaryArg(arguments)));

    private sealed class Scope : IDisposable
    {
        public void Dispose() => CurrentCall.Value = null;
    }

    /// <summary>
    /// A short, readable summary of the arguments an agent sent. S9's contract sweep found the
    /// original <c>args_digest</c> dead on both ends — nothing assigned it and nothing read it —
    /// and it was removed. This is the field revived with a producer: the values as sent, minus
    /// the session handle, truncated to a row's worth.
    /// </summary>
    public static string Digest(IDictionary<string, JsonElement>? arguments)
    {
        if (arguments is null || arguments.Count == 0) return string.Empty;

        var sb = new StringBuilder();
        foreach (var (name, value) in arguments)
        {
            if (Uninteresting.Contains(name)) continue;
            var text = Scalar(value);
            if (text.Length == 0) continue;

            if (sb.Length > 0) sb.Append(", ");
            sb.Append(name).Append(':').Append(text);
            if (sb.Length >= MaxDigestLength) break;
        }

        return sb.Length > MaxDigestLength ? sb.ToString(0, MaxDigestLength - 1) + "…" : sb.ToString();
    }

    /// <summary>The argument the desktop can navigate to, or empty when the tool has none.</summary>
    public static string PrimaryArg(IDictionary<string, JsonElement>? arguments)
    {
        if (arguments is null || arguments.Count == 0) return string.Empty;

        foreach (var name in PrimaryArgNames)
        {
            if (arguments.TryGetValue(name, out var value) && Scalar(value) is { Length: > 0 } text)
                return text;
        }

        return string.Empty;
    }

    /// <summary>Scalars render as themselves; anything structured is summarised by its kind.</summary>
    private static string Scalar(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => value.ToString(),
        JsonValueKind.Array => "[…]",
        JsonValueKind.Object => "{…}",
        _ => string.Empty,
    };
}

/// <summary>
/// Wraps one tool so that <see cref="McpCallScope"/> is open for the whole invocation. Applied by
/// <see cref="McpToolMenu"/> to BOTH halves of the menu, so an unlisted specialist tags its calls
/// exactly like an advertised tool does — otherwise the feed would go quiet precisely when an
/// agent reached for the unusual verb, which is when a supervisor is watching hardest.
/// </summary>
public sealed class ScopedMcpServerTool(McpServerTool inner) : McpServerTool
{
    public override Tool ProtocolTool => inner.ProtocolTool;

    /// <summary>Delegated whole: the wrapper adds a scope, it does not redescribe the tool.</summary>
    public override IReadOnlyList<object> Metadata => inner.Metadata;

    public override async ValueTask<CallToolResult> InvokeAsync(
        RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken = default)
    {
        // The scope must be opened INSIDE the async method: AsyncLocal flows to callees, and the
        // gRPC calls this tool makes are all callees of this await.
        using var _ = McpCallScope.Begin(
            request.Params?.Name ?? ProtocolTool.Name, request.Params?.Arguments);
        return await inner.InvokeAsync(request, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// Copies the open <see cref="McpCallScope"/> onto every outgoing gRPC call as headers. One place,
/// so a tool cannot forget: the server reads these in RecordToolCall and the desktop's feed is
/// finally spelled in the agent's vocabulary.
/// </summary>
public sealed class McpCallHeaderInterceptor : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        => continuation(request, Tagged(context));

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        => continuation(request, Tagged(context));

    private static ClientInterceptorContext<TRequest, TResponse> Tagged<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context)
        where TRequest : class
        where TResponse : class
    {
        var call = McpCallScope.Current;
        if (call is null) return context;

        var headers = context.Options.Headers ?? [];
        // The tool name is [a-z_] by construction, so it travels as itself; the two free-text
        // values are base64 (see the header constants for why).
        headers.Add(McpCallHeaders.Tool, call.Tool);
        if (call.ArgsDigest.Length > 0) headers.Add(McpCallHeaders.Args, Encode(call.ArgsDigest));
        if (call.PrimaryArg.Length > 0) headers.Add(McpCallHeaders.PrimaryArg, Encode(call.PrimaryArg));

        return new ClientInterceptorContext<TRequest, TResponse>(
            context.Method, context.Host, context.Options.WithHeaders(headers));
    }

    private static string Encode(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
}
