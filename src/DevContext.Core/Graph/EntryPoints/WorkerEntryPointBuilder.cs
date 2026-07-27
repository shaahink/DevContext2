using DevContext.Core.Graph2;

namespace DevContext.Core.Graph;

/// <summary>Builds background worker entry points (HostedService, ScheduledJob) from
/// <see cref="BackgroundWorkerDetection"/>s.</summary>
public sealed class WorkerEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        SymbolTable names, NoiseFilter noise)
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
            g.AddNode(new GraphNode(id, shortName, NodeKind.EntryPoint) { FilePath = bw.SourceFile, LineNumber = bw.LineNumber });

            var typeFqn = names.ResolveName(shortName, bw.SourceFile);
            var typeId = NodeId.ForType(typeFqn);

            // Anchor on the worker's execute member when the call graph bound it — the member's
            // own edges are the real spine (a Type anchor either dead-ends or walks the whole
            // class via the ctor twin). ExecuteAsync = BackgroundService, StartAsync = IHostedService.
            var linkedMember = false;
            foreach (var method in (string[])["ExecuteAsync", "StartAsync"])
            {
                var memberId = NodeId.ForMember(typeFqn, method);
                if (!g.HasNode(memberId)) continue;
                g.AddEdge(new GraphEdge(id, memberId, EdgeKind.Calls)
                {
                    Provenance = $"{bw.SourceFile}:{bw.LineNumber}",
                    Resolution = Resolution.Join,
                });
                linkedMember = true;
                break;
            }

            if (!linkedMember && g.HasNode(typeId))
                g.AddEdge(new GraphEdge(id, typeId, EdgeKind.Calls)
                {
                    Provenance = $"{bw.SourceFile}:{bw.LineNumber}",
                    Resolution = Resolution.Join,
                });

            entries.Add(new EntryPoint(kind, shortName, id)
            {
                Provenance = $"{bw.SourceFile}:{bw.LineNumber}",
                HandlerNode = typeId,
                Project = scope.ProjectForFile(bw.SourceFile),
            });
        }
        return entries.ToImmutable();
    }
}
