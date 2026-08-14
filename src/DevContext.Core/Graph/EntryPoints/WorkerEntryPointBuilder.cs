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

        // D1.4 (rung 4) — the OTHER half of a job surface: the job class itself. Quartz says "this is a
        // job" with an interface (IJob) and an attribute ([DisallowConcurrentExecution]); Hangfire says
        // it with an attribute only, because a Hangfire job is a plain class. Registration alone is not
        // enough — a Quartz job scheduled from a trigger built elsewhere, or a Hangfire job enqueued
        // from a request handler, is invisible to the startup walk.
        //
        // GATED ON THE FRAMEWORK'S OWN SIGNAL, which is the whole reason this is safe: IJob is not a
        // Quartz-owned name (repos declare their own all the time) and the attribute names are just as
        // guessable, so a name-only rule mints entries for classes that are not jobs. The signal comes
        // from the catalog descriptor's Packages, so the gate IS the descriptor's declaration rather
        // than a second list that can drift from it. A_repos_own_IJob_without_a_scheduler_package_
        // mints_no_scheduled_job pins that. Runs last so a class that is BOTH registered and attributed
        // keeps its registration-sourced entry — `seen` makes the earlier loop win.
        var quartz = model.Architecture.Has(ArchitectureSignals.Keys.Quartz);
        var hangfire = model.Architecture.Has(ArchitectureSignals.Keys.Hangfire);
        if (quartz || hangfire)
        {
            foreach (var type in model.Types.Values)
            {
                if (!IsSchedulerJobType(type, quartz, hangfire)) continue;
                if (!scope.Contains(type.FilePath) || !noise.IsProductionEntrySource(type.FilePath)) continue;
                if (!seen.Add(type.Name)) continue;

                entries.Add(Emit(g, scope, names, type.Name, EntryPointKind.ScheduledJob,
                    type.FilePath, type.StartLine ?? 0));
            }
        }

        return entries.ToImmutable();
    }

    /// <summary>Quartz's job interface, and the class attributes both schedulers put on a job.
    /// Read as WRITTEN (namespace prefix stripped, the optional <c>Attribute</c> suffix stripped),
    /// the same narrowness as <see cref="IsHostedServiceImplementation"/>: an intermediate base class
    /// is not followed, because that needs the hierarchy rather than a name.</summary>
    private static bool IsSchedulerJobType(TypeDiscovery type, bool quartz, bool hangfire)
    {
        if (type.Kind != TypeKind.Class) return false;

        if (quartz)
        {
            foreach (var name in type.ImplementedInterfaces)
                if (Simplify(name) is "IJob") return true;
        }

        foreach (var raw in type.Attributes)
        {
            var name = Simplify(raw);
            if (name.EndsWith("Attribute", StringComparison.Ordinal))
                name = name[..^"Attribute".Length];

            if (quartz && name is "DisallowConcurrentExecution" or "PersistJobDataAfterExecution")
                return true;
            // DisableConcurrentExecution is Hangfire's; DisallowConcurrentExecution above is Quartz's.
            // The one-letter-apart spellings are real, and belong to different frameworks.
            if (hangfire && name is "RecurringJob" or "AutomaticRetry" or "DisableConcurrentExecution")
                return true;
        }

        return false;
    }

    private static string Simplify(string name) =>
        name.Contains('.') ? name[(name.LastIndexOf('.') + 1)..] : name;

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
            var simple = Simplify(name);
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
        // class via the ctor twin). ExecuteAsync = BackgroundService, StartAsync = IHostedService,
        // Execute = Quartz's IJob (D1.4). Execute is last: a hosted service must implement one of the
        // first two, so it can only ever be reached by a job class.
        var linkedMember = false;
        foreach (var method in (string[])["ExecuteAsync", "StartAsync", "Execute"])
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
