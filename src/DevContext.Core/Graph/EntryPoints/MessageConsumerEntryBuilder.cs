using DevContext.Core.Graph2;

namespace DevContext.Core.Graph;

/// <summary>Builds message-consumer entry points from <see cref="MessageConsumerDetection"/>s
/// (MassTransit, NServiceBus).</summary>
public sealed class MessageConsumerEntryBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        SymbolTable names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var mc in model.Detections.OfType<MessageConsumerDetection>())
        {
            if (!scope.Contains(mc.SourceFile) || !noise.IsProductionEntrySource(mc.SourceFile)) continue;
            // M1.5: filter DI registration noise (AddMassTransit, UsingRabbitMq etc.)
            if (mc.MessageType == "<registration>") continue;
            // B5 (Prism D1.2d): a queue-channel consumer that is itself a hosted worker already has a
            // Background entry — the channel seam lives on its Consumes edge, not a second entry row.
            if (mc.MessageType.StartsWith("queue:", StringComparison.Ordinal)
                && model.Detections.OfType<BackgroundWorkerDetection>()
                    .Any(b => b.ImplementationType == mc.ConsumerType))
                continue;
            if (!seen.Add(mc.ConsumerType)) continue;

            var id = NodeId.ForEntry($"bus:{mc.ConsumerType}");
            g.AddNode(new GraphNode(id, mc.ConsumerType, NodeKind.EntryPoint) { FilePath = mc.SourceFile, LineNumber = mc.LineNumber });

            var typeId = NodeId.ForType(names.ResolveName(mc.ConsumerType, mc.SourceFile));
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
                Project = scope.ProjectForFile(mc.SourceFile),
            });
        }
        return entries.ToImmutable();
    }
}
