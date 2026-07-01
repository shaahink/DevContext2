namespace DevContext.Core.Graph;

/// <summary>Builds message-consumer entry points from <see cref="MessageConsumerDetection"/>s
/// (MassTransit, NServiceBus).</summary>
public sealed class MessageConsumerEntryBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        NameResolver names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var mc in model.Detections.OfType<MessageConsumerDetection>())
        {
            if (!scope.Contains(mc.SourceFile) || !noise.IsProductionEntrySource(mc.SourceFile)) continue;
            if (!seen.Add(mc.ConsumerType)) continue;

            var id = NodeId.ForEntry($"bus:{mc.ConsumerType}");
            g.AddNode(new GraphNode(id, mc.ConsumerType, NodeKind.EntryPoint) { FilePath = mc.SourceFile });

            var typeId = NodeId.ForType(names.Resolve(mc.ConsumerType));
            if (g.HasNode(typeId))
                g.AddEdge(new GraphEdge(id, typeId, EdgeKind.Calls)
                {
                    Provenance = $"{mc.SourceFile}:{mc.LineNumber}",
                    Resolution = Resolution.Join,
                });

            entries.Add(new EntryPoint(EntryPointKind.MessageConsumer, mc.ConsumerType, id)
            {
                Provenance = $"{mc.SourceFile}:{mc.LineNumber}",
                HandlerNode = typeId,
            });
        }
        return entries.ToImmutable();
    }
}
