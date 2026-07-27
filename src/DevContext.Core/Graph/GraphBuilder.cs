using DevContext.Core.Graph.Seams;
using DevContext.Core.Graph2;
using DevContext.Core.Graph2.Seams;
using DevContext.Core.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Graph;

/// <summary>
/// Assembles the <see cref="CodeGraph"/> by JOINING existing detections + types + call edges. This is
/// the heart of the rebuild: nothing here re-detects — it connects islands the old model left separate
/// (a flat Types dict + a flat Detections bag + a separate CallGraph). Worked examples below (type
/// nodes, HTTP entries, MediatR handler joins) show the pattern; TODO-marked seams are the agent's
/// P1/P2 work. Per-seam recipes are in TRACE-ENGINE-DESIGN.md §2.2.
/// </summary>
public sealed partial class GraphBuilder
{
    private readonly ISymbolResolver _resolver;
    private readonly NoiseFilter _noise;

    // P3: Entry-point builders — one per entry-point kind. Adding a new kind
    // (Blazor, gRPC, SignalR, etc.) means adding one class that implements
    // IEntryPointBuilder — no changes to GraphBuilder itself.
    private static readonly IEntryPointBuilder[] _entryBuilders =
    [
        new HttpEntryPointBuilder(),
        new WorkerEntryPointBuilder(),
        new DomainEventHandlerEntryBuilder(),
        new MessageConsumerEntryBuilder(),
        new DesktopEntryPointBuilder(),
        new GrpcEntryPointBuilder(),
        new SignalrEntryPointBuilder(),
        new FunctionsEntryPointBuilder(),
        new OrleansGrainEntryPointBuilder(),
        new GraphQlEntryPointBuilder(),
        new CliCommandEntryPointBuilder(),
    ];

    /// <summary>Creates a graph builder with a symbol resolver (syntactic now, semantic in P3) and a noise filter.</summary>
    public GraphBuilder(ISymbolResolver resolver, NoiseFilter noise)
    {
        _resolver = resolver;
        _noise = noise;
    }

    /// <summary>Builds the code graph and the entry-point inventory for one solution scope (design-doc R1).</summary>
    public (CodeGraph Graph, ImmutableArray<EntryPoint> Entries) Build(DiscoveryModel model, SolutionScope scope,
        IReadOnlyList<BodyFacts>? bodyFacts = null, SymbolTable? symbols = null)
    {
        var g = new CodeGraphBuilder();
        // ONE symbol table for the whole assembly (Batch A): name-string joins, seam-detector
        // resolution and the DI joins all read the same project-scoped, arity-aware index — shared
        // with the CallGraphBinder when the pipeline hands it in.
        var names = symbols ?? new SymbolTable(model.OrderedTypes, f => scope.ProjectForFile(f), bodyFacts);
        var archetype = ArchitectureArchetypeParser.Parse(model.Archetype);

        AddTypeNodes(g, model, scope, archetype);
        AddServiceNodes(g, model, scope);

        // ── P3: Entry-point builders (one per kind) ──────────────────────────
        // Open to extension — add a new builder for Blazor/gRPC/SignalR without
        // modifying GraphBuilder itself.
        var entries = ImmutableArray<EntryPoint>.Empty;
        foreach (var builder in _entryBuilders)
            entries = entries.AddRange(builder.Build(g, model, scope, names, _noise));

        // L4.5 — store EntryPointKind on each entry's graph node so projections
        // can derive the correct kind instead of falling back to PublicApi.
        foreach (var entry in entries)
            g.Tag(entry.Node, entry.Title, $"kind:{entry.Kind}");

        AddHandlerJoins(g, model, names, scope, _noise);            // worked example (Handles edge from MediatR detections)
        AddPipelineBehaviors(g, model, names, scope, _noise);       // B3: IPipelineBehavior → WrappedBy edges

        // ── P1 Map-facing seams ───────────────────────────────────────────
        AddEntityNodes(g, model, names, scope, _noise);             // B1: Entity nodes + aggregate tags
        AddEntityNavigationEdges(g, model, names, scope);        // A-F14: Entity→Entity relation edges
        AddEventConsumers(g, model, names, scope, _noise);          // B1: Event nodes + Consumes edges
        AddDiResolves(g, model, names, scope);              // B1: DI Resolves edges (interface → impl)

        // ── P2 Trace-facing seams ─────────────────────────────────────────
        // L2: structured seam detectors over BodyFacts (design §2.1) replace the old regex body-scan
        // sites. Edges anchor on the correct Member node by construction (BodyFacts.Member), so a
        // method-anchored trace shows only its own edges. Zero regex, zero re-parsing.
        AddSeamsFromDetectors(g, model, names, scope, bodyFacts);
        AddLambdaSeams(g, model, names, scope, bodyFacts);             // L2.4: dispatch edges for lambda entry-handlers
        AddCallEdges(g, model);                                        // C1: Calls edges from CallEdges (member→member)
        var (isSparse, hubCount) = AddHubScopeEdges(g, model, entries); // L3.4

        // ── M1.7-M1.8: Cross-service ServiceLink joins ────────────────────
        AddGrpcServiceLinks(g, model, names, scope, _noise);
        AddHttpServiceLinks(g, model, names, scope, _noise);

        // ── T2.6: the one event join ──────────────────────────────────────
        // Build the single publisher→event→consumer projection from the Raises/Consumes seams already in
        // the graph, store it, and emit the cross-service bus ServiceLink edges from it — superseding the
        // old project-name join (the former AddBusServiceLinks). A short intermediate freeze gives the
        // projection a queryable view; the emitted links land before preGraph so flows see them.
        var seamGraph = g.Build(isSparse, hubCount);
        var eventWiring = EventWiringProjection.Build(
            seamGraph, scope.ProjectForFile, _noise.IsProductionEntrySource);
        g.SetEventWiring(eventWiring);
        EventWiringProjection.EmitServiceLinks(g, eventWiring);

        var preGraph = g.Build(isSparse, hubCount);
        // Enrich (target/group-path/score) BEFORE computing flows: preGraph and the final graph share
        // identical nodes/edges (violations are metadata only), so this is safe here, and it means
        // graph.Flows carries the resolved Target — top_flows no longer reports it as null.
        var enrichedEntries = EnrichEntryScores(
            EnrichEntryGroupPaths(EnrichEntryTargets(preGraph, entries), names, scope),
            preGraph, scope);
        g.SetFlows(ComputeFlows(preGraph, enrichedEntries));
        g.SetEntries(enrichedEntries);   // T1.8 — projections read the true kind off this record, not node tags
        var violations = DetectLayerViolations(preGraph, archetype);
        var graph = g.Build(isSparse, hubCount, violations);
        return (graph, enrichedEntries);
    }
}
