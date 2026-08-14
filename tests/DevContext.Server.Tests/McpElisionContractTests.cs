using DevContext.Core.Graph;
using DevContext.Mcp;

namespace DevContext.Server.Tests;

/// <summary>
/// T1.3 (BUG-BACKLOG #9) — the pack declares a cut with a marker; the MCP decides whether to call a
/// pack complete by looking for that marker. DevContext.Mcp references Contracts only, never Core,
/// so the marker exists twice: <see cref="ContextPackBuilder.ElidedPrefix"/> and
/// <see cref="DevContextTools"/>'s mirror. A constant spelled in two assemblies is the
/// ONE-FIELD-TWO-SPELLINGS class this repo has been bitten by before — if they drift, get_context
/// goes back to asserting completeness over a truncated pack and nothing else notices.
/// </summary>
public sealed class McpElisionContractTests
{
    [Fact]
    public void The_mcp_reads_the_same_elision_marker_the_pack_writes()
    {
        Assert.Equal(ContextPackBuilder.ElidedPrefix, DevContextTools.ElidedPrefix);
    }

    [Fact]
    public void The_marker_is_not_a_prefix_of_the_empty_section_line()
    {
        // "entities: empty — omitted" must never read as a cut: opposite claims, similar words.
        Assert.False("entities: empty — omitted".StartsWith(ContextPackBuilder.ElidedPrefix, StringComparison.Ordinal));
    }
}
