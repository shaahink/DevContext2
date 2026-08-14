using DevContext.Core.Graph;
using DevContext.Core.Rendering;
using DevContext.Server.Mapping;

namespace DevContext.Server.Tests;

/// <summary>
/// M1.1 — ProtoMapper.ToProto(TraceStep) dropped four fields Core computes and the CLI has rendered
/// for four batches (OmittedNames/Batch E, MultiImplCount/I1.6, DiHostCount/C5, TestOnly/T2.1), and
/// carried the provenance SITE only as text. The GUI therefore showed a multi-impl resolve as if it
/// were the only binding, and a cut subtree as a bare count. These pin the mapping, not the render.
/// </summary>
public sealed class ProtoMapperTraceHonestyTests
{
    private static GraphNode Node(string title) =>
        new(NodeId.ForMember("Acme.Api", title), title, NodeKind.Member);

    private static Trace TraceOf(TraceStep root) =>
        new(new EntryPoint(EntryPointKind.HttpEndpoint, root.Node.Title, root.Node.Id), root);

    [Fact]
    public void MapsTheFourFieldsThatUsedToBeDropped()
    {
        var step = new TraceStep(Node("IOrderRepo"), SeamKind.Resolve, 0)
        {
            Truncated = true,
            Omitted = 6,
            OmittedNames = ["PricingService", "AuditService"],
            MultiImplCount = 3,
            DiHostCount = 2,
            TestOnly = true,
        };

        var node = ProtoMapper.ToTraceResponse(TraceOf(step), "md").Root;

        Assert.Equal(new[] { "PricingService", "AuditService" }, node.OmittedNames);
        Assert.Equal(3, node.MultiImplCount);
        Assert.Equal(2, node.DiHostCount);
        Assert.True(node.TestOnly);
    }

    [Fact]
    public void SplitsProvenanceIntoTheSiteAsData_KeepingTheTextForm()
    {
        var step = new TraceStep(Node("OrdersApi.Create"), SeamKind.Entry, 0)
        {
            Provenance = @"C:\repo\src\OrdersApi.cs:47",
        };

        var node = ProtoMapper.ToTraceResponse(TraceOf(step), "md").Root;

        Assert.Equal(@"C:\repo\src\OrdersApi.cs:47", node.Provenance);
        Assert.Equal(@"C:\repo\src\OrdersApi.cs", node.FilePath);
        Assert.Equal(47, node.LineNumber);
    }

    [Fact]
    public void LeavesTheSiteUnsetWhenProvenanceHasNoLine()
    {
        // EntryPointResolver emits bare file paths for PublicApi entries. Half a site is worse than
        // none: a client that saw file_path with line_number 0 would anchor at the top of the file.
        var step = new TraceStep(Node("PublicThing"), SeamKind.Entry, 0) { Provenance = @"C:\repo\src\X.cs" };

        var node = ProtoMapper.ToTraceResponse(TraceOf(step), "md").Root;

        Assert.False(node.HasFilePath);
        Assert.False(node.HasLineNumber);
        Assert.Equal(@"C:\repo\src\X.cs", node.Provenance);
    }

    [Fact]
    public void MapsChildrenRecursively()
    {
        var child = new TraceStep(Node("Child"), SeamKind.Call, 1) { Omitted = 1, OmittedNames = ["Deep"] };
        var root = new TraceStep(Node("Root"), SeamKind.Entry, 0) { Children = [child] };

        var node = ProtoMapper.ToTraceResponse(TraceOf(root), "md").Root;

        Assert.Equal(new[] { "Deep" }, node.Children[0].OmittedNames);
    }

    [Theory]
    // The rule the wire now single-sources: PathDisplay owns "which colon ends the drive letter".
    [InlineData(@"C:\repo\src\X.cs:47", @"C:\repo\src\X.cs", 47)]
    [InlineData("/home/me/src/X.cs:1", "/home/me/src/X.cs", 1)]
    [InlineData(@"C:\repo\src\X.cs", @"C:\repo\src\X.cs", null)]
    [InlineData("X.cs:notanumber", "X.cs:notanumber", null)]
    [InlineData("", "", null)]
    public void SplitProvenanceIsTheOneParse(string input, string expectedPath, int? expectedLine)
    {
        var (path, line) = PathDisplay.SplitProvenance(input);
        Assert.Equal(expectedPath, path);
        Assert.Equal(expectedLine, line);
    }
}
