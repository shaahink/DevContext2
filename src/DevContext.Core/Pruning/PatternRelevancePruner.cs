namespace DevContext.Core.Pruning;

/// <summary>Computes RoleScore ∈ [0,1] based on the type's architectural roles (endpoint, entity, DI, etc).
/// Applies veto/penalty rules: test-project types are hard-excluded, name-pattern matches get RoleScore forced to 0,
/// detection-bearing types are never hard-excluded.</summary>
public sealed class PatternRelevancePruner : IPruner
{
    public string Name => "PatternRelevancePruner";
    public int Order => 30;

    /// <summary>How load-bearing is this role for understanding the app — used in RoleScore max.</summary>
    private static readonly Dictionary<Type, double> RoleWeights = new()
    {
        [typeof(EndpointDetection)] = 1.0,
        [typeof(MediatRHandlerDetection)] = 0.8,
        [typeof(MessageConsumerDetection)] = 0.7,
        [typeof(EfEntityDetection)] = 0.6,
        [typeof(BackgroundWorkerDetection)] = 0.5,
        [typeof(MiddlewareDetection)] = 0.5,
        [typeof(IndirectWiringDetection)] = 0.5,
        [typeof(DiRegistrationDetection)] = 0.35,
    };

    private static readonly string[] TestPrefixes = ["When_", "Test_", "Mock_", "Fake_", "Stub_"];
    private static readonly string[] TestSuffixes = ["Tests", "Test", "Fixture", "Mock", "Stub",
        "Spec", "Specs", "Should"];
    private static readonly string[] TestNamespaceSegments = [".Tests", ".UnitTests",
        ".IntegrationTests", ".FunctionalTests", ".TestHelpers", ".Specs"];

    public ValueTask PruneAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var detectionsByTypeName = BuildDetectionLookup(model);
        var testProjectNames = CollectTestProjectNames(model);

        foreach (var type in model.Types.Values)
        {
            ct.ThrowIfCancellationRequested();

            // Compute RoleScore from detections
            if (detectionsByTypeName.TryGetValue(type.Name, out var typeDetections))
            {
                var distinctRoles = typeDetections
                    .Select(d => d.Kind)
                    .Distinct()
                    .ToList();

                var maxWeight = distinctRoles.Max(r => RoleWeights.TryGetValue(r, out var w) ? w : 0);
                var additionalBonus = 0.1 * (distinctRoles.Count - 1);
                type.RoleScore = Math.Min(1.0, maxWeight + additionalBonus);

                foreach (var (kind, _) in typeDetections)
                {
                    if (RoleWeights.TryGetValue(kind, out var w) && w > 0)
                    {
                        model.AddProvenance(type.Id, new InclusionReason(
                            $"Role '{kind.Name}' (+{w:F2})", Name, (float)w));
                    }
                }
            }

            // Floor: detection-bearing types are never hard-excluded
            var hasDetection = detectionsByTypeName.ContainsKey(type.Name);

            // Veto: types in test projects are hard-excluded
            if (testProjectNames.Count > 0
                && testProjectNames.Any(p => type.FilePath.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                if (!hasDetection)
                {
                    type.IsHardExcluded = true;
                    type.ExclusionReason = "test project";
                    model.PruningNotes.Add($"PatternRelevancePruner: excluded test-project type '{type.Id}'");
                    continue;
                }
            }

            // Penalty: name-pattern matches get RoleScore forced to 0 (not hard-excluded)
            if (IsTestOrNoiseName(type) && !hasDetection)
            {
                type.RoleScore = 0;
                model.PruningNotes.Add($"PatternRelevancePruner: penalized name-pattern type '{type.Id}' (RoleScore=0)");
            }
        }

        // Library-mode scoring: when no web signals, boost public API surface
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
                type.RoleScore = Math.Max(0, type.RoleScore - 0.5);
                continue;
            }

            // Additive library scores, capped at 1.0
            var added = 0.0;
            if (type.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public)
                added += 0.4;
            if (referencedAsBase.Contains(type.Id) || referencedAsBase.Contains(type.Name))
                added += 0.6;

            if (added > 0)
                type.RoleScore = Math.Min(1.0, type.RoleScore + added);
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

    private static bool IsTestOrNoiseName(TypeDiscovery type)
    {
        var name = type.Name;
        foreach (var prefix in TestPrefixes)
            if (name.StartsWith(prefix, StringComparison.Ordinal)) return true;
        foreach (var suffix in TestSuffixes)
            if (name.EndsWith(suffix, StringComparison.Ordinal)) return true;

        var ns = type.Namespace;
        foreach (var segment in TestNamespaceSegments)
            if (ns.Contains(segment, StringComparison.OrdinalIgnoreCase)) return true;

        var fileName = Path.GetFileNameWithoutExtension(type.FilePath);
        if (fileName.StartsWith("When_", StringComparison.Ordinal)
            || fileName.StartsWith("Test_", StringComparison.Ordinal)
            || fileName.StartsWith("Bug_", StringComparison.Ordinal))
            return true;

        return false;
    }

    private static FrozenDictionary<string, ImmutableArray<(Type Kind, string Name)>> BuildDetectionLookup(DiscoveryModel model)
    {
        var lookup = new Dictionary<string, ImmutableArray<(Type, string)>.Builder>(StringComparer.Ordinal);

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

            builder.Add((kind, typeName));
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
}
