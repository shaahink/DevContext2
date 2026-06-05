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
    ];

    public ValueTask PruneAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var detectionsByTypeName = BuildDetectionLookup(model);
        var typeNamesWithAnyDetection = CollectTypeNamesWithDetections(model);

        foreach (var type in model.Types.Values)
        {
            ct.ThrowIfCancellationRequested();

            if (typeNamesWithAnyDetection.Contains(type.Name))
            {
                type.IsPruned = false;
            }

            if (!detectionsByTypeName.TryGetValue(type.Name, out var typeDetections)) continue;

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

        return default;
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
