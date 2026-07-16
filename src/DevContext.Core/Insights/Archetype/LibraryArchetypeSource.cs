using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

/// <summary>L4.2 — Library archetype composition: public surface, internal hubs, seat implementors.</summary>
public sealed class LibraryArchetypeSource : IInsightSource
{
    public string Id => "archetype.library";
    public InsightCategory Category => InsightCategory.Shape;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var publicTypes = model.Types.Values
            .Where(t => t.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public)
            .ToList();
        if (publicTypes.Count == 0) yield break;

        // ── Public surface size ──
        var interfaces = publicTypes.Count(t => t.Kind == TypeKind.Interface);
        var classes = publicTypes.Count(t => t.Kind == TypeKind.Class);
        yield return Insight.Create("lib.public-surface", InsightCategory.Shape, Severity.Info,
            $"Public surface: {interfaces} interfaces, {classes} classes ({publicTypes.Count} total public types)",
            [$"{interfaces} interfaces", $"{classes} classes"],
            confidence: 0.9,
            confidenceBasis: "Public accessibility is compiler-level — highly reliable",
            whyItMatters: "The public surface is the library's contract — its size and shape sets the learning curve.");

        // ── Internal hubs (most-depended-upon internal types) ──
        var internalTypes = model.Types.Values
            .Where(t => t.Accessibility != Microsoft.CodeAnalysis.Accessibility.Public
                && t.Kind == TypeKind.Class)
            .ToList();

        if (internalTypes.Count > 0)
        {
            // Count how many types reference each internal type
            var internalRefs = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var ce in model.CallEdges)
            {
                if (!string.IsNullOrEmpty(ce.CalleeType))
                {
                    var name = TypeShortName(ce.CalleeType);
                    if (internalTypes.Any(t => t.Name == name))
                        internalRefs[name] = internalRefs.GetValueOrDefault(name) + 1;
                }
            }

            // ≥3-refs floor (T6.3 rider): "heavily-referenced" with "(1 refs)" is a
            // contradiction the shamshir audit rendered verbatim.
            var hubs = internalRefs.Where(kv => kv.Value >= 3)
                .OrderByDescending(kv => kv.Value).Take(5)
                .Select(kv => $"{kv.Key} ({kv.Value} refs)")
                .ToList();
            if (hubs.Count > 0)
            {
                yield return Insight.Create("lib.internal-hubs", InsightCategory.Topology, Severity.Notable,
                    $"Internal hubs: {hubs.Count} heavily-referenced internal types",
                    hubs,
                    confidence: 0.5,
                    confidenceBasis: "Reference counting is body-scan approximate — hubs are likely correct, counts approximate",
                    whyItMatters: "These are the library's 'heart' — the types every other type depends on. Start reading here.",
                    action: TypedAction.Focus(hubs.FirstOrDefault()));
            }
        }

        // ── Seat implementors ──
        var diRegs = model.Detections.OfType<DiRegistrationDetection>().ToList();
        var multiImpl = diRegs
            .Where(d => !string.IsNullOrWhiteSpace(d.ServiceType) && d.ServiceType != "?")
            // Registration-METHOD names leak in as service types from some DI shapes —
            // "AddDbContext (10 impls)" headlined shamshir's extension seats (T6.3). An
            // extension seat is an abstraction: I-prefixed or generic, never an Add*/Use* verb.
            .Where(d =>
            {
                var name = TypeShortName(d.ServiceType);
                return !(name.StartsWith("Add", StringComparison.Ordinal) || name.StartsWith("Use", StringComparison.Ordinal))
                    || name.Contains('<');
            })
            .GroupBy(d => d.ServiceType, StringComparer.Ordinal)
            .Select(g => (Type: g.Key, Count: g.Select(d => d.ImplementationType).Distinct(StringComparer.Ordinal).Count()))
            .Where(g => g.Count >= 2)
            .OrderByDescending(g => g.Count)
            .Take(3)
            .Select(g => $"{TypeShortName(g.Type)} ({g.Count} impls)")
            .ToList();

        if (multiImpl.Count > 0)
        {
            yield return Insight.Create("lib.seat-implementors", InsightCategory.Wiring, Severity.Notable,
                $"Extension seats: {string.Join(" · ", multiImpl)}",
                multiImpl,
                confidence: 0.7,
                confidenceBasis: "DI registration detection is established — counts are reliable for collected registrations",
                whyItMatters: "Interfaces with multiple implementations are extension points — they define the library's plug-in surface.",
                action: TypedAction.Node(multiImpl.FirstOrDefault()));
        }
    }

    private static string TypeShortName(string fqn)
    {
        var lastDot = fqn.LastIndexOf('.');
        return lastDot >= 0 ? fqn[(lastDot + 1)..] : fqn;
    }
}
