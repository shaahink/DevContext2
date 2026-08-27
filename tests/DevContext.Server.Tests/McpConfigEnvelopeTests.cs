using System.Text.Json;

using DevContext.Mcp;
using DevContext.Protos;

using Microsoft.Extensions.Logging.Abstractions;

namespace DevContext.Server.Tests;

/// <summary>
/// F3 (BUG-BACKLOG #34) — the <c>config</c> tool's envelope over the Options-pattern catalog rows.
///
/// The 2026-08-26 unseen drive measured "1 keys exist" on a 487-file repo whose config is bound the
/// modern way (<c>AddOptions&lt;T&gt;().BindConfiguration(Const)</c>). The engine-side fix merges those
/// bindings into the catalog as PatternType "OptionsBinding"; these tests pin the tool side: the row
/// flows through with its pattern type intact, and the tool's self-description tells the truth about
/// what the scan now sees — and what it still cannot.
/// </summary>
public sealed class McpConfigEnvelopeTests
{
    private const string Handle = "handle-1";

    private static ConfigResponse OneOptionsBinding()
    {
        var resp = new ConfigResponse { TotalKeys = 1 };
        resp.Bindings.Add(new ConfigBinding
        {
            Key = "Pipeline:Queue:Drain",
            FilePath = "src/Pipeline/DependencyInjection.cs",
            LineNumber = 73,
            PatternType = "OptionsBinding",
            Service = "Pipeline",
        });
        return resp;
    }

    private static DevContextTools ToolsReturning(ConfigResponse resp)
        => new(new DevContextService.DevContextServiceClient(new McpStubCallInvoker(
                   rpc => rpc == "ConfigLookup" ? resp : null)),
               NullLogger<DevContextTools>.Instance);

    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

    [Fact]
    public async Task Options_binding_rows_flow_through_with_their_pattern_type()
    {
        var root = Parse(await ToolsReturning(OneOptionsBinding()).Config(Handle, "Pipeline:Queue:Drain"));

        Assert.Equal(1, root.GetProperty("totalKeys").GetInt32());
        var sites = root.GetProperty("keys").GetProperty("Pipeline:Queue:Drain");
        Assert.Equal(1, sites.GetArrayLength());
        Assert.Equal("OptionsBinding", sites[0].GetProperty("patternType").GetString());
        Assert.Equal(73, sites[0].GetProperty("lineNumber").GetInt32());
    }

    [Fact]
    public async Task Method_self_description_names_the_options_pattern_and_its_blind_spot()
    {
        // T3.6 honesty: the envelope's "method" line is the scan's self-description. Once the scan
        // reads Options bindings it must SAY so — and keep saying what it still cannot see.
        var root = Parse(await ToolsReturning(OneOptionsBinding()).Config(Handle));

        var method = root.GetProperty("method").GetString();
        Assert.NotNull(method);
        Assert.Contains("AddOptions", method);
        Assert.Contains("not captured", method);
    }
}
