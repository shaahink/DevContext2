using DevContext.Core.Graph;
using DevContext.Server.Mapping;

namespace DevContext.Server.Tests;

/// <summary>
/// M1.2 — <c>MapResponse.stack</c> (proto field 13) shipped empty for its whole life: the desktop
/// identity strip, the Atlas chip header and MCP <c>overview</c> all read it, and ProtoMapper never
/// wrote it, because the tags were computed inside the markdown renderer. The contract sweep could
/// not see this — it looks for fields with no READERS, and this one had three. That is the direction
/// these tests cover: the field has a writer, and it is the map's own list.
/// </summary>
public sealed class ProtoMapperStackTests
{
    [Fact]
    public void ToMapResponse_carries_the_map_stack_onto_the_wire()
    {
        var map = new MapModel
        {
            Archetype = Archetype.App,
            Style = "ControllerBased",
            Stack = ["net10.0", "Minimal APIs", "EF Core"],
        };

        var resp = ProtoMapper.ToMapResponse(map, markdown: "");

        Assert.Equal(["net10.0", "Minimal APIs", "EF Core"], resp.Stack);
    }

    [Fact]
    public void ToMapResponse_leaves_stack_empty_when_the_map_detected_nothing()
    {
        var resp = ProtoMapper.ToMapResponse(new MapModel { Archetype = Archetype.App }, markdown: "");

        Assert.Empty(resp.Stack);
    }
}
