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

            entries.Add(Emit(g, scope, names, shortName, kind, bw.SourceFile, bw.LineNumber));
        }

        // D1.2 — a BackgroundService/IHostedService IMPLEMENTATION is a hosted service whether or not
        // this repo's Program.cs registers it with AddHostedService<T>. Registration happens out of
        // sight all the time: Scrutor assembly scanning, a library's own AddXxx() extension method, the
        // Worker SDK's generated host, or a host composed in a project outside the scan scope. Before
        // this, those workers produced no entry at all — the audit's "hosted services detect only via
        // AddHostedService<T> in scanned Program/Startup flow". The base-type walk reads BaseTypes,
        // which SyntaxStructureExtractor fills in EVERY mode (unlike SourceBody), so the entry set does
        // not depend on the profile. Registered workers keep their detection-sourced entry: that loop
        // runs first and `seen` makes it win, so nothing about the old path changes.
        foreach (var type in model.Types.Values)
        {
            if (!IsHostedServiceImplementation(type)) continue;
            if (!scope.Contains(type.FilePath) || !noise.IsProductionEntrySource(type.FilePath)) continue;
            if (!seen.Add(type.Name)) continue;

            entries.Add(Emit(g, scope, names, type.Name, EntryPointKind.HostedService,
                type.FilePath, type.StartLine ?? 0));
        }

        return entries.ToImmutable();
    }

    /// <summary>
    /// A direct implementation of the hosted-service contract. Deliberately narrow: the base list as
    /// WRITTEN, so an intermediate abstract base (<c>class MyWorker : WorkerBase</c>) is not matched —
    /// that needs the type hierarchy, not a name, and a wrong guess here mints entries for classes that
    /// are not workers.
    /// </summary>
    private static bool IsHostedServiceImplementation(TypeDiscovery type)
    {
        if (type.Kind != TypeKind.Class) return false;
        foreach (var name in type.BaseTypes.Concat(type.ImplementedInterfaces))
        {
            var simple = name.Contains('.') ? name[(name.LastIndexOf('.') + 1)..] : name;
            if (simple is "BackgroundService" or "IHostedService" or "IHostedLifecycleService")
                return true;
        }
        return false;
    }

    private static EntryPoint Emit(
        CodeGraphBuilder g, SolutionScope scope, SymbolTable names,
        string shortName, EntryPointKind kind, string sourceFile, int lineNumber)
    {
        var id = NodeId.ForEntry($"worker:{shortName}");
        g.AddNode(new GraphNode(id, shortName, NodeKind.EntryPoint) { FilePath = sourceFile, LineNumber = lineNumber });

        var typeFqn = names.ResolveName(shortName, sourceFile);
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
                Provenance = $"{sourceFile}:{lineNumber}",
                Resolution = Resolution.Join,
            });
            linkedMember = true;
            break;
        }

        if (!linkedMember && g.HasNode(typeId))
            g.AddEdge(new GraphEdge(id, typeId, EdgeKind.Calls)
            {
                Provenance = $"{sourceFile}:{lineNumber}",
                Resolution = Resolution.Join,
            });

        return new EntryPoint(kind, shortName, id)
        {
            Provenance = $"{sourceFile}:{lineNumber}",
            HandlerNode = typeId,
            Project = scope.ProjectForFile(sourceFile),
        };
    }
}
