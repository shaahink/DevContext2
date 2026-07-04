using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

public sealed class MultiImplSource : IInsightSource
{
    public string Id => "wiring.multi-impl";
    public InsightCategory Category => InsightCategory.Wiring;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var diRegs = model.Detections.OfType<DiRegistrationDetection>().ToList();
        if (diRegs.Count < 10) yield break;

        // Resolve declared types once, keyed by short name, so `LooksLikeServiceAbstraction` can tell
        // an in-repo interface from an in-repo class without a full semantic model (E2).
        var typesByShortName = model.Types.Values.ToLookup(t => t.Name, StringComparer.Ordinal);

        var multiImpl = diRegs
            .Where(d => IsResolvedName(d.ServiceType) && IsResolvedName(d.ImplementationType))
            .Where(d => LooksLikeServiceAbstraction(d.ServiceType, typesByShortName))
            .GroupBy(d => d.ServiceType, StringComparer.Ordinal)
            .Select(g => (ServiceType: g.Key, ImplCount: g.Select(d => d.ImplementationType).Distinct(StringComparer.Ordinal).Count()))
            .Where(g => g.ImplCount >= 2)
            .OrderByDescending(g => g.ImplCount)
            .Take(3)
            .Select(g => $"{TypeName(g.ServiceType)} ({g.ImplCount} impls)")
            .ToList();

        if (multiImpl.Count == 0) yield break;

        yield return Insight.Create(Id, Category, Severity.Notable,
            $"Multi-implementation interfaces: {string.Join(" · ", multiImpl)}", multiImpl);
    }

    /// <summary>An unresolved registration ("?", from a DI shape the extractor couldn't read a type
    /// name out of) must never leak into a headline as a bare "?" (E2 no-placeholder invariant).</summary>
    private static bool IsResolvedName(string name) =>
        !string.IsNullOrWhiteSpace(name) && name != "?" && name != "*";

    /// <summary>
    /// "Multi-implementation interfaces" previously grouped by <c>ImplementationType ?? ServiceType</c>
    /// — i.e. by concrete implementation, not the abstraction being implemented — so a plain class
    /// registered twice (e.g. a DbContext) headlined as a "multi-impl interface" (E2). Require the
    /// service type to actually be an interface: resolved locally via <see cref="DiscoveryModel.Types"/>
    /// when it's declared in this repo, else fall back to the `IFoo` naming convention for
    /// framework/vendor abstractions (mirrors <c>SyntaxStructureExtractor.IsInterface</c>).
    /// </summary>
    private static bool LooksLikeServiceAbstraction(string serviceType, ILookup<string, TypeDiscovery> typesByShortName)
    {
        var shortName = TypeName(StripGenericArgs(serviceType));
        var declared = typesByShortName[shortName].ToList();
        if (declared.Count > 0)
            return declared.Any(t => t.Kind == TypeKind.Interface);

        return shortName.Length >= 2 && shortName[0] == 'I' && char.IsUpper(shortName[1]);
    }

    private static string StripGenericArgs(string typeName)
    {
        var lt = typeName.IndexOf('<');
        return lt >= 0 ? typeName[..lt] : typeName;
    }

    private static string TypeName(string fqn)
    {
        var lastDot = fqn.LastIndexOf('.');
        return lastDot >= 0 ? fqn[(lastDot + 1)..] : fqn;
    }
}
