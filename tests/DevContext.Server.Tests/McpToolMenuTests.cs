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
    private static DevContextTools Tools() => new(
        new DevContextService.DevContextServiceClient(
            McpStubCallInvoker.FailAll(StatusCode.Unavailable, "not called")),
        NullLogger<DevContextTools>.Instance);

    /// <summary>The menu as the SERVER advertises it, registered exactly as Program.cs does — which
    /// since T1.2 means the CORE half of <see cref="McpToolMenu"/>, not every attributed method.</summary>
    private static IReadOnlyList<string> RegisteredToolNames()
    {
        var services = new ServiceCollection();
        var (core, _) = McpToolMenu.Build(Tools());
        foreach (var t in core) services.AddSingleton(t);
        services.AddMcpServer();

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
        Assert.Equal(McpToolMenu.CoreMethods().Count, registered.Count);
        // G2.1 folded 24 -> 21; G3.1 added `seam`; T1.2 curated 22 -> 14 advertised + 8 unlisted
        // specialists. This number is the PRODUCT decision, so it is pinned: a tool joining the
        // menu an agent reads at connect should cost a deliberate edit here.
        Assert.Equal(14, registered.Count);
        Assert.Equal(8, McpToolMenu.SpecialistMethods().Count);
        Assert.Equal(22, ToolMethodNames().Count);   // nothing was deleted, only demoted

        foreach (var retired in UnknownToolHandler.Folded.Select(f => f.Retired))
            Assert.DoesNotContain(retired, registered);
    }

    /// <summary>
    /// T1.2 — curation must not be a capability cut. Every specialist is still BUILT, from the same
    /// method with the same schema, and carries a line saying what it answers; and no name is on
    /// both halves. Without this, "demoted" quietly becomes "deleted" one refactor later.
    /// </summary>
    [Fact]
    public void Every_unlisted_specialist_is_built_reasoned_and_off_the_menu()
    {
        var tools = Tools();
        var (core, specialists) = McpToolMenu.Build(tools);
        var reasons = McpToolMenu.SpecialistReasons(tools);
        var advertised = core.Select(t => t.ProtocolTool.Name).ToArray();

        Assert.NotEmpty(specialists);
        Assert.Equal(specialists.Count, reasons.Count);

        foreach (var s in specialists)
        {
            var name = s.ProtocolTool.Name;
            Assert.DoesNotContain(name, advertised);            // unlisted
            Assert.True(reasons.TryGetValue(name, out var why));// and explained
            Assert.False(string.IsNullOrWhiteSpace(why));
            // Same carrier as the advertised half: a specialist an agent reaches for by name gets
            // the same described schema it would have got from tools/list.
            Assert.False(string.IsNullOrWhiteSpace(s.ProtocolTool.Description));
        }
    }

    /// <summary>Every advertised tool describes itself. BUG-BACKLOG #5 shipped 22 empty strings for
    /// a year; the wire gate catches it end-to-end, this catches it in the unit suite.</summary>
    [Fact]
    public void Every_advertised_tool_has_a_description()
    {
        var (core, _) = McpToolMenu.Build(Tools());
        var undescribed = core.Where(t => string.IsNullOrWhiteSpace(t.ProtocolTool.Description))
            .Select(t => t.ProtocolTool.Name).ToArray();
        Assert.Empty(undescribed);
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
