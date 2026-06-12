namespace DevContext.Core.Pruning;

public sealed class PatternRelevancePruner : IPruner
{
    public string Name => "PatternRelevancePruner";
    public int Order => 30;

    private static readonly ImmutableArray<(Type DetectionType, string FieldName, float Boost)> BoostRules =
    [
        (typeof(EndpointDetection), nameof(EndpointDetection.HandlerType), 5.0f),
        (typeof(MediatRHandlerDetection), nameof(MediatRHandlerDetection.HandlerType), 4.0f),
        (typeof(EfEntityDetection), nameof(EfEntityDetection.EntityType), 3.0f),
        (typeof(DiRegistrationDetection), nameof(DiRegistrationDetection.ImplementationType), 2.0f),
        (typeof(BackgroundWorkerDetection), nameof(BackgroundWorkerDetection.ImplementationType), 2.0f),
        (typeof(MessageConsumerDetection), nameof(MessageConsumerDetection.ConsumerType), 3.0f),
    ];

    private static readonly string[] TestPrefixes = ["When_", "Test_", "Mock_", "Fake_", "Stub_"];
    private static readonly string[] TestSuffixes = ["Tests", "Test", "Fixture", "Mock", "Stub",
        "Spec", "Specs", "Should"];
    private static readonly string[] TestNamespaceSegments = [".Tests", ".UnitTests",
        ".IntegrationTests", ".FunctionalTests", ".TestHelpers", ".Specs"];

    public ValueTask PruneAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var detectionsByTypeName = BuildDetectionLookup(model);
        var typeNamesWithAnyDetection = CollectTypeNamesWithDetections(model);
        var testProjectNames = CollectTestProjectNames(model);

        foreach (var type in model.Types.Values)
        {
            ct.ThrowIfCancellationRequested();

            // Apply relevance boosts for types referenced by detections
            if (detectionsByTypeName.TryGetValue(type.Name, out var typeDetections))
            {
                foreach (var (kind, name) in typeDetections)
                {
                    ct.ThrowIfCancellationRequested();

                    var boost = GetBoost(kind, name);
                    if (boost > 0.0f)
                    {
                        type.RelevanceScore += boost;
                        model.AddProvenance(type.Id, new InclusionReason(
                            $"Pattern '{kind.Name}' on '{name}' (+{boost:F1})", Name, boost));
                    }
                }
            }

            // Mark test/internal noise types as hard-excluded (not just low-scored)
            if (IsTestOrNoiseType(type, testProjectNames))
            {
                type.IsHardExcluded = true;
                type.ExclusionReason = "test/mock noise";
                model.PruningNotes.Add($"PatternRelevancePruner: excluded test type '{type.Id}'");
                continue;
            }
        }

        // Library-mode relevance scoring: when no web signals are present, boost public API surface
        if (!HasWebSignals(model))
        {
            ApplyLibraryModeScoring(model, testProjectNames);
            model.PruningNotes.Add("PatternRelevancePruner: library mode active — boosting public API, penalizing test types");
        }

        return default;
    }

    private static bool HasWebSignals(DiscoveryModel model)
    {
        return model.Architecture.Has(ArchitectureSignals.Keys.MinimalApis)
            || model.Architecture.Has(ArchitectureSignals.Keys.Controllers)
            || model.Architecture.Has(ArchitectureSignals.Keys.FastEndpoints)
            || model.Architecture.Has(ArchitectureSignals.Keys.MediatR);
    }

    private static void ApplyLibraryModeScoring(DiscoveryModel model, FrozenSet<string> testProjectNames)
    {
        // Collect types that are base types/interfaces of other surviving types
        var referencedAsBase = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in model.Types.Values)
        {
            foreach (var iface in type.ImplementedInterfaces)
                referencedAsBase.Add(iface);
            foreach (var baseType in type.BaseTypes)
                referencedAsBase.Add(baseType);
        }

        foreach (var type in model.Types.Values)
        {
            // Penalize test project types
            if (testProjectNames.Count > 0
                && testProjectNames.Any(p => type.FilePath.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                type.RelevanceScore -= 0.5f;
                continue;
            }

            // Boost public types (public API surface)
            if (type.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public)
                type.RelevanceScore += 0.3f;

            // Boost types used as base types or interfaces by other types
            if (referencedAsBase.Contains(type.Id) || referencedAsBase.Contains(type.Name))
                type.RelevanceScore += 0.4f;
        }
    }

    private static FrozenSet<string> CollectTestProjectNames(DiscoveryModel model)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var project in model.Projects)
        {
            if (project.Name.EndsWith("Tests", StringComparison.OrdinalIgnoreCase)
                || project.Name.EndsWith("Test", StringComparison.OrdinalIgnoreCase)
                || project.Name.EndsWith("Specs", StringComparison.OrdinalIgnoreCase))
            {
                names.Add(project.Name);
            }
        }
        return names.ToFrozenSet();
    }

    private static bool IsTestOrNoiseType(TypeDiscovery type, FrozenSet<string> testProjectNames)
    {
        // Type name patterns
        var name = type.Name;
        foreach (var prefix in TestPrefixes)
            if (name.StartsWith(prefix, StringComparison.Ordinal)) return true;
        foreach (var suffix in TestSuffixes)
            if (name.EndsWith(suffix, StringComparison.Ordinal)) return true;

        // Namespace patterns
        var ns = type.Namespace;
        foreach (var segment in TestNamespaceSegments)
            if (ns.Contains(segment, StringComparison.OrdinalIgnoreCase)) return true;

        // File path — check if file belongs to a test project by name
        var fileName = Path.GetFileNameWithoutExtension(type.FilePath);
        if (fileName.StartsWith("When_", StringComparison.Ordinal)
            || fileName.StartsWith("Test_", StringComparison.Ordinal)
            || fileName.StartsWith("Bug_", StringComparison.Ordinal))
            return true;

        return false;
    }

    private static FrozenDictionary<string, ImmutableArray<(Type Kind, string Name)>> BuildDetectionLookup(DiscoveryModel model)
    {
        var lookup = new Dictionary<string, ImmutableArray<(Type, string)>.Builder>();

        foreach (var detection in model.Detections)
        {
            var entry = GetTypeNameEntry(detection);
            if (entry is null) continue;

            var (typeName, kind) = entry.Value;
            if (!lookup.TryGetValue(typeName, out var builder))
            {
                builder = ImmutableArray.CreateBuilder<(Type, string)>();
                lookup[typeName] = builder;
            }

            builder.Add((detection.GetType(), typeName));
        }

        return lookup.ToFrozenDictionary(kv => kv.Key, kv => kv.Value.ToImmutable());
    }

    private static (string TypeName, Type Kind)? GetTypeNameEntry(Detection detection)
    {
        return detection switch
        {
            EndpointDetection ep => (ep.HandlerType, typeof(EndpointDetection)),
            MediatRHandlerDetection mr => (mr.HandlerType, typeof(MediatRHandlerDetection)),
            EfEntityDetection ef => (ef.EntityType, typeof(EfEntityDetection)),
            BackgroundWorkerDetection bw => (bw.ImplementationType, typeof(BackgroundWorkerDetection)),
            MiddlewareDetection mw => (mw.MiddlewareType, typeof(MiddlewareDetection)),
            IndirectWiringDetection iw when iw.TargetType is not null => (iw.TargetType, typeof(IndirectWiringDetection)),
            MessageConsumerDetection mc => (mc.ConsumerType, typeof(MessageConsumerDetection)),
            DiRegistrationDetection dr => (dr.ImplementationType, typeof(DiRegistrationDetection)),
            _ => null,
        };
    }

    private static FrozenSet<string> CollectTypeNamesWithDetections(DiscoveryModel model)
    {
        var names = new HashSet<string>();

        foreach (var detection in model.Detections)
        {
            if (GetTypeNameEntry(detection) is { } entry)
            {
                names.Add(entry.TypeName);
            }
        }

        return names.ToFrozenSet();
    }

    private static float GetBoost(Type kind, string name)
    {
        foreach (var (dt, _, boost) in BoostRules)
        {
            if (dt == kind) return boost;
        }

        return 0.0f;
    }
}
