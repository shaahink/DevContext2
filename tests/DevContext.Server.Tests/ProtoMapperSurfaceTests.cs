using DevContext.Core.Graph;
using DevContext.Server.Mapping;

using TypeKind = DevContext.Core.Models.TypeKind;

namespace DevContext.Server.Tests;

/// <summary>
/// D4.4 — the full library surface flows structurally through ToMapResponse. Before this,
/// only Groups + ExtensionPoints were serialized; ENTRY API / ABSTRACTIONS / GENERATORS /
/// CONSUMER PATHS / INTERNALS / PACKAGES existed solely inside the markdown blob.
/// </summary>
public sealed class ProtoMapperSurfaceTests
{
    private static MapModel LibraryMap() => new()
    {
        Archetype = Archetype.Library,
        Style = "ControllerBased",
        Surface = new LibrarySurface(
            Groups:
            [
                new SurfaceGroup("Refit",
                [
                    new SurfaceType("ApiException", TypeKind.Class, ["Create", "GetContentAs"]) { Doc = "Represents an error." },
                    new SurfaceType("IRequestBuilder", TypeKind.Interface, []),
                ]),
            ],
            ExtensionPoints: ["HttpClientFactoryExtensions.AddRefitClient"])
        {
            EntryApi =
            [
                new SurfaceEntry("[Get]", "annotate", "Send the request with HTTP method 'GET'.", "GetAttribute.cs"),
                new SurfaceEntry("RestService", "build", null, null),
            ],
            Abstractions = [new SurfaceAbstraction("IHttpContentSerializer", TypeKind.Interface, 11)],
            Internals = [new SurfaceGroup("Refit.Internal", [new SurfaceType("Helper", TypeKind.Class, [])])],
            ConsumerPaths = ["annotate  →  [Get] on a partial class/member"],
            Generators = [new SurfaceGenerator("InterfaceStubGeneratorV2", "generator", "Produces Refit stubs.")],
            Packages = [new PackageGroup("Utilities", ["Newtonsoft.Json 13.0.4"])],
        },
    };

    [Fact]
    public void ToMapResponse_maps_the_full_library_surface()
    {
        var resp = ProtoMapper.ToMapResponse(LibraryMap(), markdown: "m");

        Assert.True(resp.IsLibrary);
        var s = resp.Surface;
        Assert.NotNull(s);

        var entry = Assert.Single(s.EntryApi, e => e.Kind == "annotate");
        Assert.Equal("[Get]", entry.Title);
        Assert.Equal("Send the request with HTTP method 'GET'.", entry.Doc);
        Assert.Equal("GetAttribute.cs", entry.Location);

        var bare = Assert.Single(s.EntryApi, e => e.Kind == "build");
        Assert.False(bare.HasDoc);
        Assert.False(bare.HasLocation);

        var abstraction = Assert.Single(s.Abstractions);
        Assert.Equal("IHttpContentSerializer", abstraction.Name);
        Assert.Equal("interface", abstraction.Kind);
        Assert.Equal(11, abstraction.ImplementorCount);

        var gen = Assert.Single(s.Generators);
        Assert.Equal(("InterfaceStubGeneratorV2", "generator"), (gen.Name, gen.Kind));

        Assert.Equal(["annotate  →  [Get] on a partial class/member"], s.ConsumerPaths);
        Assert.Equal("Refit.Internal", Assert.Single(s.Internals).Namespace);
        Assert.Equal("Utilities", Assert.Single(s.Packages).Label);

        // The pre-existing slice still flows, now with the type doc.
        var group = Assert.Single(s.Groups);
        Assert.Equal("Represents an error.", group.Types_[0].Doc);
        Assert.False(group.Types_[1].HasDoc);
    }

    [Fact]
    public void ToMapResponse_without_surface_leaves_surface_unset()
    {
        var resp = ProtoMapper.ToMapResponse(new MapModel(), markdown: "m");
        Assert.False(resp.IsLibrary);
        Assert.Null(resp.Surface);
    }
}
