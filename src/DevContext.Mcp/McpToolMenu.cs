using System.Reflection;

using ModelContextProtocol.Server;

namespace DevContext.Mcp;

/// <summary>
/// T1.2 (DEEP-EVAL W1.2) — the CURATED agent menu, and the one place that decides what is on it.
///
/// <para>The complaint the audit measured was not that 22 tools is too many verbs to implement; it
/// is that 22 verbs is what an agent has to read and choose between before it has done any work,
/// every session, and tool selection is the whole game. So the menu <c>tools/list</c> advertises is
/// now the core surface an agent needs to get from "a repo" to "the right code with its wiring" —
/// PRODUCT-DIRECTION §7's set plus the navigation primitives — and the rest are SPECIALISTS.</para>
///
/// <para><b>Demoted is not deleted.</b> A specialist is unlisted, not retired: it is still built
/// from the same method, with the same schema, and <see cref="UnknownToolHandler"/> dispatches a
/// call by name straight to it. Nothing an agent could do before it can no longer do — the cost of
/// asking for one is that you have to know its name, which is the correct price for a tool nine
/// sessions in ten do not want. Retiring them instead would have deleted real capability
/// (<c>config</c>, <c>tests_for</c> and <c>verify_context</c> answer questions no other tool
/// answers) and broken every harness that drives the session lifecycle.</para>
///
/// <para>The split is carried by <see cref="SpecialistToolAttribute"/> on the method, so it lives
/// beside the tool rather than in a list somewhere that can drift from it — the same lesson G2.1
/// learned when the did-you-mean array went on advertising three tools the server no longer had.
/// </para>
/// </summary>
public static class McpToolMenu
{
    private static IEnumerable<MethodInfo> ToolMethods()
        => typeof(DevContextTools)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttribute<McpServerToolAttribute>() is not null);

    /// <summary>The methods behind the advertised menu.</summary>
    public static IReadOnlyList<MethodInfo> CoreMethods()
        => [.. ToolMethods().Where(m => m.GetCustomAttribute<SpecialistToolAttribute>() is null)];

    /// <summary>The methods behind the unlisted specialists.</summary>
    public static IReadOnlyList<MethodInfo> SpecialistMethods()
        => [.. ToolMethods().Where(m => m.GetCustomAttribute<SpecialistToolAttribute>() is not null)];

    /// <summary>Build both halves against a live tools instance. Program.cs and the tests use this
    /// same call, so what the test asserts is what the server serves.</summary>
    public static (IReadOnlyList<McpServerTool> Core, IReadOnlyList<McpServerTool> Specialists) Build(
        DevContextTools tools)
        => ([.. CoreMethods().Select(m => McpServerTool.Create(m, tools))],
            [.. SpecialistMethods().Select(m => McpServerTool.Create(m, tools))]);

    /// <summary>Specialist tool name → the one line saying what it answers. Read off the same
    /// attribute the split is carried by, so the envelope cannot describe a different set than the
    /// server dispatches.</summary>
    public static IReadOnlyDictionary<string, string> SpecialistReasons(DevContextTools tools)
        => SpecialistMethods()
            .Select(m => (Name: McpServerTool.Create(m, tools).ProtocolTool.Name,
                          Why: m.GetCustomAttribute<SpecialistToolAttribute>()!.Why))
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .ToDictionary(t => t.Name, t => t.Why, StringComparer.Ordinal);
}

/// <summary>
/// Marks a tool as a SPECIALIST: built and callable, but kept off <c>tools/list</c> so the menu an
/// agent reads at connect stays the core surface. <see cref="Why"/> is the one line the unknown-tool
/// envelope shows when an agent calls it, so it must say what the tool is for, not that it is
/// demoted.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class SpecialistToolAttribute(string why) : Attribute
{
    /// <summary>What this specialist answers — shown to an agent that calls it by name.</summary>
    public string Why { get; } = why;
}
