namespace DevContext.Core.Graph;

/// <summary>
/// Builds entry points of a specific kind from the discovery model into the code graph.
/// Each implementation is self-contained and independently testable. Adding a new
/// entry-point kind (Blazor, gRPC, SignalR, etc.) means creating a new class with this
/// interface — no changes to GraphBuilder itself.
/// </summary>
public interface IEntryPointBuilder
{
    /// <summary>Builds entry points of this builder's kind and adds corresponding
    /// nodes/edges to the graph builder.</summary>
    ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g,
        DiscoveryModel model,
        SolutionScope scope,
        NameResolver names,
        NoiseFilter noise);
}
