using System.Reflection;
using System.Text.Json;

using DevContext.Mcp;
using DevContext.Protos;

using Grpc.Core;

using Microsoft.Extensions.Logging.Abstractions;

namespace DevContext.Server.Tests;

/// <summary>
/// G1.3 (R4 §1 item 5) — no MCP tool may let a raw <see cref="RpcException"/> past its error
/// envelope.
///
/// The audit named five tools. A sweep of the file found EIGHT of twenty-four, because the guard is
/// hand-written per method — <c>try { handle = ResolveHandle(handle); } catch { return FromRpc(…) }</c>
/// — and it is easy to guard the handle lookup and leave the actual RPC bare. An agent on the other
/// end of a leak gets an MCP protocol error with a stack trace instead of an actionable next step.
///
/// So the fix is not eight more try blocks: it is this test, which walks EVERY tool the class
/// exposes and fails the day a new one lands unguarded.
/// </summary>
public sealed class McpErrorEnvelopeTests
{
    private static DevContextTools ToolsThatCannotReachTheServer()
        => new(new DevContextService.DevContextServiceClient(
                   McpStubCallInvoker.FailAll(StatusCode.Unavailable, "server down")),
               NullLogger<DevContextTools>.Instance);

    /// <summary>Every public method carrying the SDK's tool attribute.</summary>
    private static List<MethodInfo> ToolMethods()
        => [.. typeof(DevContextTools)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttributes().Any(a => a.GetType().Name == "McpServerToolAttribute"))
            .OrderBy(m => m.Name, StringComparer.Ordinal)];

    private static object?[] DefaultArgs(MethodInfo m)
        => [.. m.GetParameters().Select(p =>
            p.HasDefaultValue ? p.DefaultValue
            : p.ParameterType == typeof(string) ? "C:/repos/MyApp"
            : p.ParameterType == typeof(int) ? 0
            : p.ParameterType == typeof(bool) ? false
            : (object?)null)];

    [Fact]
    public async Task Every_tool_answers_an_envelope_when_the_server_fails()
    {
        var tools = ToolsThatCannotReachTheServer();
        var methods = ToolMethods();

        // A positive precondition, so this can never pass vacuously on an empty list (G1.2's lesson:
        // the entry canary first "passed" against a pack with zero sections).
        Assert.True(methods.Count >= 24, $"expected the full tool menu, found {methods.Count}");

        foreach (var m in methods)
        {
            var returned = m.Invoke(tools, DefaultArgs(m));
            var json = await (Task<string>)returned!;

            Assert.False(string.IsNullOrWhiteSpace(json), $"{m.Name} returned nothing");
            Assert.DoesNotContain("RpcException", json, StringComparison.Ordinal);
            Assert.DoesNotContain("Grpc.Core", json, StringComparison.Ordinal);

            using var doc = JsonDocument.Parse(json);
            var hasError = doc.RootElement.TryGetProperty("error", out _);
            var saysNotFound = doc.RootElement.TryGetProperty("found", out var f)
                && f.ValueKind == JsonValueKind.False;
            Assert.True(hasError || saysNotFound,
                $"{m.Name} answered a server failure without saying so: {json[..Math.Min(160, json.Length)]}");
        }
    }

    /// <summary>The envelope has to be usable: an error, a hint and an example.</summary>
    [Fact]
    public async Task The_envelope_carries_a_hint_and_an_example()
    {
        var tools = ToolsThatCannotReachTheServer();

        using var doc = JsonDocument.Parse(await tools.Entrypoints());

        Assert.Equal("server down", doc.RootElement.GetProperty("error").GetString());
        Assert.False(string.IsNullOrWhiteSpace(doc.RootElement.GetProperty("hint").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(doc.RootElement.GetProperty("example").GetString()));
    }

    /// <summary>
    /// close_session on a handle that was never open used to answer <c>{"closed":false}</c> and
    /// nothing else — indistinguishable from a successful no-op to an agent skimming for an error.
    /// </summary>
    [Fact]
    public async Task Closing_an_unknown_handle_says_it_was_not_found()
    {
        var tools = new DevContextTools(
            new DevContextService.DevContextServiceClient(new McpStubCallInvoker(rpc => rpc switch
            {
                "ListSessions" => Sessions("only-session"),
                "CloseSession" => new CloseResponse { Closed = false },
                _ => null,
            })),
            NullLogger<DevContextTools>.Instance);

        using var doc = JsonDocument.Parse(await tools.CloseSession("no-such-handle"));

        Assert.False(doc.RootElement.GetProperty("closed").GetBoolean());
        Assert.False(doc.RootElement.GetProperty("found").GetBoolean());
        Assert.Contains("nothing to close", doc.RootElement.GetProperty("note").GetString()!);
    }

    internal static ListSessionsResponse Sessions(params string[] handles)
    {
        var resp = new ListSessionsResponse();
        foreach (var h in handles)
            resp.Sessions.Add(new SessionInfo { Handle = h, Repo = $"C:/repos/{h}" });
        return resp;
    }
}
