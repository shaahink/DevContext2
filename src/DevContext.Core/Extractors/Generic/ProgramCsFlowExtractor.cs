using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Generic;

[ExtractorOrder(40)]
public sealed class ProgramCsFlowExtractor : IDiscoveryExtractor
{
    // Base Map* methods from HttpConstants, plus middleware-specific ones
    private static readonly ImmutableArray<string> MapMethods =
        [.. HttpConstants.MapMethods, "MapGrpcService", "MapHub", "MapBlazorHub"];

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
        var programFiles = context.Analysis.AllSourceFiles
            .Where(f =>
            {
                var name = Path.GetFileName(f);
                return name.Equals("Program.cs", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("SchedulersConfig.cs", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("Scheduler", StringComparison.OrdinalIgnoreCase);
            })
            .ToList();

        foreach (var filePath in programFiles)
        {
            ct.ThrowIfCancellationRequested();

            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct).ConfigureAwait(false);
            }
            catch
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {filePath}");
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);

            var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();

            var (addRegistrations, useRegistrations, mapRegistrations) =
                CollectProgramRegistrations(invocations, model, filePath);

            DetectOrphanPatterns(addRegistrations, useRegistrations, model, filePath, Name);
            DetectBackgroundWorkers(invocations, model, filePath, Name);
        }
    }

    private (List<(string Name, int Line)> AddRegs, List<(string Name, int Line)> UseRegs, List<(string Name, string Method, int Line)> MapRegs)
        CollectProgramRegistrations(IReadOnlyList<InvocationExpressionSyntax> invocations, DiscoveryModel model, string filePath)
    {
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
            var isServicesTarget = string.Equals(target, "Services", StringComparison.Ordinal) || target.EndsWith(".Services", StringComparison.Ordinal);
            var isAppTarget = string.Equals(target, "app", StringComparison.Ordinal) || target.EndsWith(".app", StringComparison.Ordinal);

            if (isServicesTarget && methodName.StartsWith("Add", StringComparison.Ordinal))
            {
                addRegistrations.Add((methodName, lineNumber));
            }
            else if (isAppTarget && methodName.StartsWith("Use", StringComparison.Ordinal))
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
            else if (isAppTarget && MapMethods.Contains(methodName, StringComparer.Ordinal))
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

        return (addRegistrations, useRegistrations, mapRegistrations);
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
        IReadOnlyList<InvocationExpressionSyntax> invocations,
        DiscoveryModel model,
        string filePath,
        string extractorName)
    {
        foreach (var invocation in invocations)
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                continue;

            var methodName = memberAccess.Name.Identifier.ValueText;
            if (!string.Equals(methodName, "AddHostedService", StringComparison.Ordinal) && !string.Equals(methodName, "AddDNTScheduler", StringComparison.Ordinal))
                continue;

            var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            var implementationType = "?";

            if (string.Equals(methodName, "AddDNTScheduler", StringComparison.Ordinal))
            {
                // Extract job types from options.AddJob<T>() calls in the lambda
                if (invocation.ArgumentList.Arguments.Count > 0)
                {
                    var arg = invocation.ArgumentList.Arguments[0].Expression;
                    ExtractSchedulerJobs(arg, model, filePath, extractorName);
                }
                continue; // DNTScheduler is detected via individual jobs
            }

            // AddHostedService<T> detection
            if (invocation.ArgumentList.Arguments.Count > 0)
            {
                var arg = invocation.ArgumentList.Arguments[0].Expression;
                implementationType = arg?.ToString() ?? "?";
            }
            else if (invocation.Expression is MemberAccessExpressionSyntax ma
                     && ma.Name is GenericNameSyntax genericName
                     && genericName.TypeArgumentList.Arguments.Count > 0)
            {
                implementationType = genericName.TypeArgumentList.Arguments[0].ToString();
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

    private static void ExtractSchedulerJobs(
        ExpressionSyntax lambda,
        DiscoveryModel model,
        string filePath,
        string extractorName)
    {
        foreach (var inv in lambda.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (inv.Expression is not MemberAccessExpressionSyntax ma)
                continue;

            if (!string.Equals(ma.Name.Identifier.ValueText, "AddJob"
, StringComparison.Ordinal) && !string.Equals(ma.Name.Identifier.ValueText, "AddScheduledTask", StringComparison.Ordinal))
                continue;

            // Extract generic type argument from AddJob<T>()
            if (ma.Name is GenericNameSyntax gns
                && gns.TypeArgumentList.Arguments.Count > 0)
            {
                var jobType = gns.TypeArgumentList.Arguments[0].ToString();
                var line = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                model.Detections.Add(new BackgroundWorkerDetection(
                    ServiceType: "DNTScheduler",
                    ImplementationType: jobType,
                    Kind: BackgroundWorkerKind.HostedService)
                {
                    ExtractorName = extractorName,
                    SourceFile = filePath,
                    LineNumber = line,
                });
            }
        }
    }

    private static string DetermineWorkerServiceType(string implementationType)
    {
        if (implementationType.Contains("BackgroundService", StringComparison.Ordinal))
            return "BackgroundService";
        return "IHostedService";
    }
}
