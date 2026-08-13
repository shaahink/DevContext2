using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Graph2;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

/// <summary>Dead-code candidates: public types with zero inbound graph edges. I1 (Prism D2) made the
/// claim coverage-gated: on a graph tier that didn't walk enough wiring, "zero inbound references"
/// means "zero edges we captured", and an agent acting on the claim deletes live code (the audit's
/// podcasts round: 3/5 orphans FALSE — a DTO constructed in a LINQ projection, an interface with 5
/// implementors, an EF join entity the data-map insight itself listed). The claim now (a) requires an
/// edge-coverage floor, (b) excludes body-constructed, implemented, and entity-indexed types, and
/// (c) carries its coverage basis in the evidence line.</summary>
public sealed class GraphOrphansSource : IAnalysisAwareInsightSource
{
    public string Id => "graph.orphans";
    public InsightCategory Category => InsightCategory.Wiring;

    /// <summary>Fallback overload (no analysis context): no body-construction facts are available to
    /// clear a candidate, so no dead-code claim is made at all — the claim would be ungated.</summary>
    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
        => [];

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries,
        SharedAnalysisContext analysis)
    {
        if (graph.NodeCount < 10) yield break;

        // I1 (P7 catch, D2 close): a Library's public types are its PRODUCT — external consumers are
        // invisible to the graph by construction, so "zero inbound references" can never support a
        // dead-code claim on a library (wolverine's WolverineValidationResult was accused while being
        // the element type of a public API collection).
        if (string.Equals(model.Archetype, "Library", StringComparison.Ordinal)) yield break;

        var handlesCount = graph.AllEdges.Count(e => e.Kind == EdgeKind.Handles);
        var sendsCount = graph.AllEdges.Count(e => e.Kind == EdgeKind.Sends);
        if (handlesCount < 5 && sendsCount < 10) yield break;

        // I1 coverage floor — a dead-code claim is only as good as the walk that produced the zero.
        //
        // G10.1 RE-MEASURED 2026-08-02, 11 poles, cold analysis
        // (eval-results/2026-08-02/G10/threshold-grid.txt). THIS FLOOR IS CURRENTLY UNREACHABLE: no
        // pole clears all three clauses, so `graph.orphans` emits nothing on any of them. The clause
        // that shuts it is verifiedRatio. Semantic share of Calls edges, measured: podcasts 0.010,
        // eShop 0.057, DntSite 0.088, GitVersion 0.091, Serilog 0.099, wolverine 0.161, self 0.173,
        // CleanArchitecture 0.259, Dapper 0.410, MahApps 0.495, MediatR 0.731 — and the two above
        // 0.4 are libraries, which line 35 has already excluded, or fall under the 30-call floor.
        // Counting Join as verified too (what GraphStats/SeamStat means by the word — its "approx"
        // is Syntactic ONLY, so the engine ships two definitions of "verified") lifts eShop to 0.21
        // and still clears nothing. The other two clauses do still discriminate: wiredRatio spans
        // 0.294 (wolverine) to 0.933 (self), calls.Count 26 (MediatR) to 4023 (DntSite).
        //
        // Left AT 0.5 and Semantic-only deliberately. This is the one claim in the product that gets
        // live code deleted when it is wrong (the audit's podcasts round: 3/5 orphans FALSE), so it
        // asks for Roslyn-verified inbound edges specifically. V1.1 (#25): that reading is now the
        // ENGINE'S ONLY reading — EdgeConfidence.IsVerified — and the dashboard's looser
        // not-approximate is gone; Resolution.Join, the enum's default, is its own tier and never
        // counts as verified anywhere. Recalibrating a floor DOWNWARDS to make a destructive
        // claim start firing is not a threshold correction; whether this source earns its keep is an
        // owner call, tracked as a conductor bug, not something to settle by moving the number.
        var calls = graph.AllEdges.Where(e => e.Kind == EdgeKind.Calls).ToList();
        var verified = calls.Count(EdgeConfidence.IsVerified);
        var verifiedRatio = calls.Count > 0 ? (double)verified / calls.Count : 0;
        var wired = entries.Count(e => e.Target is not null);
        var wiredRatio = entries.Length > 0 ? (double)wired / entries.Length : 0;
        if (calls.Count < 30 || verifiedRatio < 0.5 || wiredRatio < 0.5)
            yield break; // the graph didn't walk enough wiring to accuse anything of being dead

        var entryIds = new HashSet<NodeId>(
            entries.Where(e => graph.Contains(e.Node)).Select(e => e.Node));
        var diTypes = model.Detections.OfType<DiRegistrationDetection>()
            .Select(d => d.ServiceType?.Split(',').FirstOrDefault()?.Trim())
            .Where(t => t is not null)
            .ToHashSet();

        var conventionDiTypes = FindConventionDiTypes(model);

        // I1 exclusion: types CONSTRUCTED anywhere in a walked body (`new EpisodeDto(e)` inside a
        // LINQ projection never becomes a graph edge, but the creation op is a body fact).
        var constructed = new HashSet<string>(StringComparer.Ordinal);
        foreach (var facts in analysis.AllBodyFacts)
        {
            foreach (var op in facts.Ops)
            {
                switch (op)
                {
                    case CreationOp c: constructed.Add(StripGenerics(c.Type.Text)); break;
                    case LocalDeclOp { InferredFrom: { } inf }: constructed.Add(StripGenerics(inf.Text)); break;
                }
            }
        }

        // I1 exclusion: an interface/base implemented or extended by ANY type is a live contract
        // (the audit's "IRequest dead" while 5 request classes implement it — implements-edges were
        // ignored by the in-degree test).
        // Every identifier in the type reference counts, INCLUDING generic arguments — a type that
        // is only the element of a base `List<T>` is live (the wolverine P7 catch). Over-inclusion
        // is the right error direction: an exclusion too wide keeps a live type unaccused.
        var implemented = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in model.Types.Values)
        {
            if (!type.ImplementedInterfaces.IsDefaultOrEmpty)
                foreach (var iface in type.ImplementedInterfaces)
                    AddTypeRefIdentifiers(implemented, iface);
            if (!type.BaseTypes.IsDefaultOrEmpty)
                foreach (var bt in type.BaseTypes)
                    AddTypeRefIdentifiers(implemented, bt);
        }

        var orphans = graph.Nodes
            .Where(n => n.Kind == NodeKind.Type
                && !n.Tags.Contains("framework")
                && !n.Tags.Contains("internal")
                // I1: never accuse a type the entity mapper itself indexed (EF join entities live
                // in the model/migrations; the data-map insight may list them in the same output).
                && !n.Tags.Contains(RoleTags.Entity)
                && !n.Tags.Contains(RoleTags.Aggregate)
                && graph.InEdges(n.Id).Length == 0
                && !entryIds.Contains(n.Id)
                && !diTypes.Contains(n.Id.Key)
                && !conventionDiTypes.Contains(n.Id.Key)
                && !constructed.Contains(n.Title)
                && !implemented.Contains(n.Title)
                // DI/startup extension classes are invoked via extension-method syntax the call
                // graph doesn't attribute to the class (T6.3 rider: "Extensions" classes were
                // classic dead-code false positives on eShop and MediatR).
                && !n.Title.EndsWith("Extensions", StringComparison.Ordinal))
            .Take(5)
            .Select(n => n.Title)
            .ToList();

        if (orphans.Count == 0) yield break;

        var severity = orphans.Count >= 3 ? Severity.Notable : Severity.Info;
        yield return Insight.Create(Id, Category, severity,
            $"Possible dead code: {orphans.Count} public types with zero inbound references",
            orphans,
            confidence: 0.4,
            confidenceBasis: $"coverage basis: {(int)Math.Round(verifiedRatio * 100)}% of {calls.Count} Calls edges verified, "
                + $"{wired}/{entries.Length} entries wired; body-constructed, implemented, and "
                + "entity-indexed types excluded");
    }

    private static string StripGenerics(string name)
    {
        var lt = name.IndexOf('<');
        return lt > 0 ? name[..lt] : name;
    }

    /// <summary>Adds every identifier in a type reference — outer name AND generic arguments
    /// (`List&lt;WolverineValidationResult&gt;` contributes both).</summary>
    private static void AddTypeRefIdentifiers(HashSet<string> into, string typeRef)
    {
        var start = -1;
        for (var i = 0; i <= typeRef.Length; i++)
        {
            var isIdent = i < typeRef.Length && (char.IsLetterOrDigit(typeRef[i]) || typeRef[i] == '_');
            if (isIdent) { if (start < 0) start = i; }
            else if (start >= 0)
            {
                into.Add(typeRef[start..i]);
                start = -1;
            }
        }
    }

    private static HashSet<string> FindConventionDiTypes(DiscoveryModel model)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in model.Types.Values)
        {
            var ifaceNames = type.ImplementedInterfaces;
            if (ifaceNames.IsDefaultOrEmpty) continue;
            foreach (var iface in ifaceNames)
            {
                if (IsDiConventionInterface(iface))
                {
                    result.Add(type.Id);
                    break;
                }
            }
        }

        foreach (var type in model.Types.Values)
        {
            var baseTypes = type.BaseTypes;
            if (baseTypes.IsDefaultOrEmpty) continue;
            foreach (var bt in baseTypes)
            {
                if (bt.StartsWith("AbstractValidator", StringComparison.OrdinalIgnoreCase)
                    || bt.StartsWith("DbContext", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(type.Id);
                    break;
                }
            }
        }

        return result;
    }

    private static bool IsDiConventionInterface(string iface)
    {
        return iface.StartsWith("IRequestHandler<", StringComparison.OrdinalIgnoreCase)
            || iface.StartsWith("INotificationHandler<", StringComparison.OrdinalIgnoreCase)
            || iface.StartsWith("IValidator<", StringComparison.OrdinalIgnoreCase)
            || iface.StartsWith("IConsumer<", StringComparison.OrdinalIgnoreCase)
            || iface.StartsWith("IEventHandler<", StringComparison.OrdinalIgnoreCase)
            || iface.StartsWith("ICommandHandler<", StringComparison.OrdinalIgnoreCase)
            || iface.StartsWith("IQueryHandler<", StringComparison.OrdinalIgnoreCase)
            // EF applies these by assembly scan — zero inbound references is their normal state
            // (T6.3 rider: OrderItemEntityTypeConfiguration headlined eShop's dead-code card).
            || iface.StartsWith("IEntityTypeConfiguration<", StringComparison.OrdinalIgnoreCase);
    }
}
