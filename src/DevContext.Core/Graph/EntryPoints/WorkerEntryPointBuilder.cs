namespace DevContext.Core.Graph;

/// <summary>Builds background worker entry points (HostedService, ScheduledJob) from
/// <see cref="BackgroundWorkerDetection"/>s.</summary>
public sealed class WorkerEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        NameResolver names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var bw in model.Detections.OfType<BackgroundWorkerDetection>())
        {
            if (!scope.Contains(bw.SourceFile) || !noise.IsProductionEntrySource(bw.SourceFile)) continue;
            var impl = bw.ImplementationType;
            if (string.IsNullOrEmpty(impl) || impl == "?") continue;
            var shortName = impl.Contains('.') ? impl[(impl.LastIndexOf('.') + 1)..] : impl;
            if (!seen.Add(shortName)) continue;

            var kind = bw.Kind == BackgroundWorkerKind.TimedJob
                || string.Equals(bw.ServiceType, "DNTScheduler", StringComparison.OrdinalIgnoreCase)
                ? EntryPointKind.ScheduledJob
                : EntryPointKind.HostedService;

            var id = NodeId.ForEntry($"worker:{shortName}");
            g.AddNode(new GraphNode(id, shortName, NodeKind.EntryPoint) { FilePath = bw.SourceFile });

            var typeId = NodeId.ForType(names.Resolve(shortName));
            if (g.HasNode(typeId))
                g.AddEdge(new GraphEdge(id, typeId, EdgeKind.Calls)
                {
                    Provenance = $"{bw.SourceFile}:{bw.LineNumber}",
                    Resolution = Resolution.Join,
                });

            entries.Add(new EntryPoint(kind, shortName, id) { Provenance = $"{bw.SourceFile}:{bw.LineNumber}" });
        }
        return entries.ToImmutable();
    }
}
