using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using DevContext.Core.Pipeline;

namespace DevContext.Core.Extractors.Generic;

[ExtractorOrder(40)]
public sealed class ProgramCsFlowExtractor : IDiscoveryExtractor
{
    // Base Map* methods from HttpConstants, plus middleware-specific ones
    private static readonly ImmutableArray<string> MapMethods =
        [.. HttpConstants.MapMethods, "MapGrpcService", "MapHub", "MapBlazorHub"];

    // Startup composition is routinely factored out of Program.cs into extension methods
    // (ServiceRegistration.cs, *ServiceCollectionExtensions.cs, MiddlewarePipeline.cs, …).
    // Files containing any of these tokens join the walk-set regardless of file name.
    // D1.4 — Hangfire's RecurringJob/BackgroundJob calls and Quartz's AddQuartz(...) builder are just
    // as routinely factored out of Program.cs (JobRegistration.cs, HangfireConfig.cs) as the rest of
    // startup composition, so they join the walk-set by the same token probe.
    private static readonly ImmutableArray<string> StartupCompositionTokens =
        ["AddHostedService", "AddDNTScheduler", "MapHub", "MapGrpcService", "MapBlazorHub",
         "AddQuartz", "RecurringJob", "BackgroundJob"];

    public string Name => "ProgramCsFlowExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Generic;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage2Parallel;

    public ExtractorCapabilities Capabilities => new(
        [], ["middleware-detections", "background-worker-detections"],
        ["model.Detections"],
        "Walks Program.cs files for middleware registration order and background worker detection");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var programFiles = new List<string>();
        foreach (var f in context.Analysis.AllSourceFiles)
        {
            var name = Path.GetFileName(f);
            if (name.Equals("Program.cs", StringComparison.OrdinalIgnoreCase)
                || name.Equals("SchedulersConfig.cs", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Scheduler", StringComparison.OrdinalIgnoreCase))
            {
                programFiles.Add(f);
                continue;
            }

            // Cheap text probe over the cached source keeps the walk-set small while
            // catching startup composition factored into extension-method files.
            try
            {
                var text = await context.Cache.GetTextAsync(f, ct).ConfigureAwait(false);
                if (StartupCompositionTokens.Any(t => text.Contains(t, StringComparison.Ordinal)))
                    programFiles.Add(f);
            }
            catch (Exception ex)
            {
                // unreadable file — skip, the named-file paths above were not affected
                PipelineDiagnostics.Swallowed("ProgramCsFlowExtractor", "file-read", ex);
            }
        }

        foreach (var filePath in programFiles)
        {
            ct.ThrowIfCancellationRequested();

            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct);
            }
            catch
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {filePath}");
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
            var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            var addRegistrations = new List<(string Name, int Line)>();
            var useRegistrations = new List<(string Name, int Line)>();
            var mapRegistrations = new List<(string Name, string Method, int Line)>();

            foreach (var invocation in invocations)
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                    continue;

                var methodName = memberAccess.Name.Identifier.ValueText;
                var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                var target = memberAccess.Expression.ToString();
                var isServicesTarget = target == "Services" || target.EndsWith(".Services");
                var isAppTarget = target == "app" || target.EndsWith(".app");

                if (isServicesTarget && methodName.StartsWith("Add"))
                {
                    addRegistrations.Add((methodName, lineNumber));
                }
                else if (isAppTarget && methodName.StartsWith("Use"))
                {
                    useRegistrations.Add((methodName, lineNumber));

                    model.Detections.Add(new MiddlewareDetection(
                        MiddlewareType: methodName,
                        PipelineOrder: useRegistrations.Count,
                        Kind: MiddlewareKind.UseX)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                    });
                }
                else if (isAppTarget && MapMethods.Contains(methodName))
                {
                    var order = mapRegistrations.Count + 1;
                    mapRegistrations.Add((methodName, methodName, lineNumber));

                    model.Detections.Add(new MiddlewareDetection(
                        MiddlewareType: methodName,
                        PipelineOrder: order,
                        Kind: MiddlewareKind.MapX)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                    });
                }
            }

            DetectOrphanPatterns(addRegistrations, useRegistrations, model, filePath, Name);

            DetectBackgroundWorkers(root, model, filePath, Name);
        }
    }

    private static void DetectOrphanPatterns(
        List<(string Name, int Line)> addRegs,
        List<(string Name, int Line)> useRegs,
        DiscoveryModel model,
        string filePath,
        string extractorName)
    {
        var addSet = addRegs.Select(r => r.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var useSet = useRegs.Select(r => r.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var add in addRegs)
        {
            var useEquivalent = add.Name switch
            {
                "AddCors" => "UseCors",
                "AddAuthentication" => "UseAuthentication",
                "AddAuthorization" => "UseAuthorization",
                "AddResponseCompression" => "UseResponseCompression",
                "AddStaticFiles" => "UseStaticFiles",
                "AddSession" => "UseSession",
                "AddExceptionHandler" => "UseExceptionHandler",
                "AddRouting" => "UseRouting",
                "AddEndpoints" => "UseEndpoints",
                _ => null,
            };

            if (useEquivalent != null && !useSet.Contains(useEquivalent))
            {
                model.AddDiagnostic(DiagnosticLevel.Info, extractorName,
                    $"Orphan pattern: '{add.Name}' at line {add.Line} in {Path.GetFileName(filePath)} "
                    + $"has no corresponding '{useEquivalent}' call");
            }
        }
    }

    private static void DetectBackgroundWorkers(
        SyntaxNode root,
        DiscoveryModel model,
        string filePath,
        string extractorName)
    {
        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                continue;

            var methodName = memberAccess.Name.Identifier.ValueText;

            // D1.4 (rung 4) — Hangfire and Quartz put the fact that a class is a JOB in the
            // REGISTRATION, not in the class: a Hangfire job is a plain class that nothing but
            // `RecurringJob.AddOrUpdate<T>` names, and a Quartz schedule lives inside the
            // `AddQuartz(q => …)` builder. Registration syntax in a startup file is visible in EVERY
            // extraction profile — which is exactly why D1.2 ruled out a body-driven "is it timer
            // driven" verdict (SourceBody/BodyFacts only exist under Full/Debug) and named this the
            // honest producer of BackgroundWorkerKind.TimedJob. Before this, TimedJob had no producer
            // at all: the enum declaration and WorkerEntryPointBuilder's check were its only two
            // references in Core.
            if (methodName == "AddQuartz")
            {
                if (invocation.ArgumentList.Arguments.Count > 0)
                    ExtractSchedulerJobs(
                        invocation.ArgumentList.Arguments[0].Expression, model, filePath, extractorName,
                        serviceType: "Quartz", kind: BackgroundWorkerKind.TimedJob,
                        methods: ["AddJob", "ScheduleJob"]);
                continue;
            }

            if (TryReadHangfireJob(memberAccess, methodName, out var hangfireJobType))
            {
                model.Detections.Add(new BackgroundWorkerDetection(
                    ServiceType: "Hangfire",
                    ImplementationType: hangfireJobType,
                    Kind: BackgroundWorkerKind.TimedJob)
                {
                    ExtractorName = extractorName,
                    SourceFile = filePath,
                    LineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                });
                continue;
            }

            if (methodName != "AddHostedService" && methodName != "AddDNTScheduler")
                continue;

            var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            var implementationType = "?";

            if (methodName == "AddDNTScheduler")
            {
                // Extract job types from options.AddJob<T>() calls in the lambda
                if (invocation.ArgumentList.Arguments.Count > 0)
                {
                    var arg = invocation.ArgumentList.Arguments[0].Expression;
                    ExtractSchedulerJobs(arg, model, filePath, extractorName,
                        serviceType: "DNTScheduler", kind: BackgroundWorkerKind.HostedService,
                        methods: ["AddJob", "AddScheduledTask"]);
                }
                continue; // DNTScheduler is detected via individual jobs
            }

            // AddHostedService<T> detection — the generic type argument is authoritative; a factory
            // argument (`AddHostedService(sp => sp.GetRequiredService<EngineWorker>())`) yields the
            // resolved type, never the raw lambda text.
            if (invocation.Expression is MemberAccessExpressionSyntax ma
                && ma.Name is GenericNameSyntax genericName
                && genericName.TypeArgumentList.Arguments.Count > 0)
            {
                implementationType = genericName.TypeArgumentList.Arguments[0].ToString();
            }
            else if (invocation.ArgumentList.Arguments.Count > 0)
            {
                var arg = invocation.ArgumentList.Arguments[0].Expression;
                if (arg is LambdaExpressionSyntax lambda)
                {
                    var resolved = lambda.DescendantNodes().OfType<GenericNameSyntax>()
                        .FirstOrDefault(gn => gn.Identifier.ValueText is "GetRequiredService" or "GetService"
                            && gn.TypeArgumentList.Arguments.Count == 1);
                    implementationType = resolved?.TypeArgumentList.Arguments[0].ToString()
                        ?? arg.ToString();
                }
                else
                {
                    implementationType = arg?.ToString() ?? "?";
                }
            }
            var serviceType = DetermineWorkerServiceType(implementationType);

            model.Detections.Add(new BackgroundWorkerDetection(
                ServiceType: serviceType,
                ImplementationType: implementationType,
                Kind: BackgroundWorkerKind.HostedService)
            {
                ExtractorName = extractorName,
                SourceFile = filePath,
                LineNumber = lineNumber,
            });
        }
    }

    /// <summary>
    /// Reads the job types out of a scheduler builder lambda — DNTScheduler's
    /// <c>AddJob&lt;T&gt;</c>/<c>AddScheduledTask&lt;T&gt;</c> and (D1.4) Quartz's
    /// <c>AddJob&lt;T&gt;</c>/<c>ScheduleJob&lt;T&gt;</c>. The generic type argument is the only
    /// authoritative name here; a non-generic <c>AddJob(typeof(T))</c> is deliberately not read,
    /// because guessing at an argument expression is how a registration list turns into noise.
    /// </summary>
    private static void ExtractSchedulerJobs(
        ExpressionSyntax lambda,
        DiscoveryModel model,
        string filePath,
        string extractorName,
        string serviceType,
        BackgroundWorkerKind kind,
        ImmutableArray<string> methods)
    {
        foreach (var inv in lambda.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (inv.Expression is not MemberAccessExpressionSyntax ma)
                continue;

            if (!methods.Contains(ma.Name.Identifier.ValueText))
                continue;

            // Extract generic type argument from AddJob<T>()
            if (ma.Name is GenericNameSyntax gns
                && gns.TypeArgumentList.Arguments.Count > 0)
            {
                var jobType = gns.TypeArgumentList.Arguments[0].ToString();
                var line = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                model.Detections.Add(new BackgroundWorkerDetection(
                    ServiceType: serviceType,
                    ImplementationType: jobType,
                    Kind: kind)
                {
                    ExtractorName = extractorName,
                    SourceFile = filePath,
                    LineNumber = line,
                });
            }
        }
    }

    /// <summary>
    /// D1.4 — a Hangfire job registration: <c>RecurringJob.AddOrUpdate&lt;T&gt;</c>,
    /// <c>BackgroundJob.Enqueue&lt;T&gt;</c>/<c>Schedule&lt;T&gt;</c>, and the DI-injected
    /// <c>IRecurringJobManager</c>/<c>IBackgroundJobClient</c> forms of the same three calls.
    /// <para>
    /// Two guards keep this off ordinary code, and both are needed: <c>Enqueue</c> and <c>Schedule</c>
    /// are common method names, so the RECEIVER must look like Hangfire's (the static entry points, or
    /// a field/parameter whose name ends in <c>JobClient</c>/<c>JobManager</c>) — a
    /// <c>Queue&lt;Job&gt; _jobs</c> does not match; and the call must carry a generic TYPE ARGUMENT,
    /// which is the only place the job class is named. <c>BackgroundJob.Enqueue(() => …)</c> against a
    /// captured instance names no type and is skipped rather than guessed at.
    /// </para>
    /// </summary>
    private static bool TryReadHangfireJob(
        MemberAccessExpressionSyntax memberAccess, string methodName, out string jobType)
    {
        jobType = "";
        if (methodName is not ("AddOrUpdate" or "Enqueue" or "Schedule"))
            return false;

        if (memberAccess.Name is not GenericNameSyntax generic
            || generic.TypeArgumentList.Arguments.Count == 0)
            return false;

        var target = memberAccess.Expression.ToString();
        var receiver = target.Contains('.') ? target[(target.LastIndexOf('.') + 1)..] : target;
        var isHangfireReceiver =
            receiver.Equals("RecurringJob", StringComparison.Ordinal)
            || receiver.Equals("BackgroundJob", StringComparison.Ordinal)
            || receiver.EndsWith("JobClient", StringComparison.OrdinalIgnoreCase)
            || receiver.EndsWith("JobManager", StringComparison.OrdinalIgnoreCase);
        if (!isHangfireReceiver)
            return false;

        jobType = generic.TypeArgumentList.Arguments[0].ToString();
        return true;
    }

    private static string DetermineWorkerServiceType(string implementationType)
    {
        if (implementationType.Contains("BackgroundService"))
            return "BackgroundService";
        if (implementationType.Contains("IHostedService"))
            return "IHostedService";
        return "IHostedService";
    }
}
