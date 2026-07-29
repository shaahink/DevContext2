using System.Reflection;
using System.Text.Json;

using DevContext.Mcp;
using DevContext.Protos;

using Grpc.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using ModelContextProtocol.Server;

namespace DevContext.Server.Tests;

/// <summary>
/// G2.1 (R4 §1 item 11) — the tool menu is folded, and the did-you-mean handler reads the REAL
/// list instead of a second hand-maintained one.
///
/// <para>The list it replaced was CORRECT: 24 literal names beside 24 <c>[McpServerTool]</c>
/// methods, in agreement on the day it was written. That is exactly why a spot check ("does it
/// list <c>map</c>?") proves nothing here — it passed on the broken state too. What the list could
/// not do was STAY correct: folding three tools away left it advertising three tools the server no
/// longer has, and because the nearest-name search runs over that same array, calling
/// <c>insights</c> answered <c>"Did you mean 'insights'?"</c> — the handler pointing an agent at
/// the very tool that had just stopped existing.</para>
///
/// <para>So the assertion here is an EQUALITY against the SDK's own registered collection — the
/// object <c>tools/list</c> is answered from — built the same way <c>Program.cs</c> builds it.</para>
/// </summary>
public sealed class McpToolMenuTests
{
    /// <summary>The menu as the SERVER advertises it, registered exactly as Program.cs does.</summary>
    private static IReadOnlyList<string> RegisteredToolNames()
    {
        var services = new ServiceCollection();
        services.AddMcpServer().WithTools(new DevContextTools(
            new DevContextService.DevContextServiceClient(
                McpStubCallInvoker.FailAll(StatusCode.Unavailable, "not called")),
            NullLogger<DevContextTools>.Instance));

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<McpServerOptions>>().Value;
        return [.. options.ToolCollection?.PrimitiveNames ?? []];
    }

    private static List<string> ToolMethodNames()
        => [.. typeof(DevContextTools)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttributes().Any(a => a.GetType().Name == "McpServerToolAttribute"))
            .Select(m => m.Name)];

    private static JsonElement Describe(string name)
        => JsonDocument.Parse(JsonSerializer.Serialize(UnknownToolHandler.Describe(name))).RootElement;

    [Fact]
    public void The_menu_is_folded_and_nothing_advertises_a_retired_tool()
    {
        var registered = RegisteredToolNames();

        // Positive precondition first — an empty collection would make every check below vacuous,
        // and an empty ToolCollection is precisely the wiring break Program.cs logs an error for.
        Assert.Equal(ToolMethodNames().Count, registered.Count);
        Assert.Equal(22, registered.Count);   // G2.1 folded 24 -> 21; G3.1 added `seam`

        foreach (var retired in UnknownToolHandler.Folded.Select(f => f.Retired))
            Assert.DoesNotContain(retired, registered);
    }

    [Fact]
    public void Did_you_mean_advertises_exactly_the_registered_tools()
    {
        var registered = RegisteredToolNames();
        UnknownToolHandler.UseToolNames(registered);

        var advertised = Describe("no_such_tool_xyz").GetProperty("availableTools")
            .EnumerateArray().Select(e => e.GetString()!).ToArray();

        Assert.Equal([.. registered.OrderBy(n => n, StringComparer.Ordinal)],
                     [.. advertised.OrderBy(n => n, StringComparer.Ordinal)]);
    }

    /// <summary>
    /// A retired name must teach the call that replaces it. The nearest-name heuristic cannot do
    /// this job: it returns on the first SUBSTRING match, and "flow" is a substring of "top_flows",
    /// so it answered a confidently wrong tool.
    /// </summary>
    [Theory]
    [InlineData("flow", "trace")]
    [InlineData("insights", "stats")]
    [InlineData("interesting_points", "overview")]
    public void A_folded_tool_names_the_call_that_replaces_it(string retired, string replacement)
    {
        var registered = RegisteredToolNames();
        UnknownToolHandler.UseToolNames(registered);

        var reply = Describe(retired);
        var hint = reply.GetProperty("hint").GetString()!;
        var example = reply.GetProperty("example").GetString()!;

        Assert.Contains(replacement, hint, StringComparison.Ordinal);
        Assert.StartsWith(replacement + "(", example, StringComparison.Ordinal);
        // The redirect target has to be a tool that actually exists, or this is the same defect
        // one level down.
        Assert.Contains(replacement, registered);
    }

    /// <summary>The redirect table may never become a second menu: every row names a DEAD tool.</summary>
    [Fact]
    public void The_fold_table_holds_only_names_the_server_no_longer_serves()
    {
        var registered = RegisteredToolNames();

        foreach (var (retiredName, replacement, call) in UnknownToolHandler.Folded)
        {
            Assert.DoesNotContain(retiredName, registered);
            Assert.Contains(replacement, registered);
            Assert.StartsWith(replacement + "(", call, StringComparison.Ordinal);
        }
    }

    /// <summary>An unseeded handler must not invent a menu — it points at tools/list instead.</summary>
    [Fact]
    public void An_unseeded_handler_names_no_tools()
    {
        UnknownToolHandler.UseToolNames([]);
        try
        {
            var reply = Describe("map");
            Assert.Empty(reply.GetProperty("availableTools").EnumerateArray());
            Assert.Contains("tools/list", reply.GetProperty("hint").GetString()!, StringComparison.Ordinal);
        }
        finally
        {
            UnknownToolHandler.UseToolNames(RegisteredToolNames());
        }
    }
}
