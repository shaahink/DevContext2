using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Generic;

[ExtractorOrder(40)]
public sealed class ProgramCsFlowExtractor : IDiscoveryExtractor
{
    private static readonly ImmutableArray<string> MapMethods =
        ["MapGet", "MapPost", "MapPut", "MapDelete", "MapPatch", "MapGrpcService", "MapHub", "MapBlazorHub"];

    public string Name => "ProgramCsFlowExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Generic;

    public ExtractorCapabilities Capabilities => new(
        [], ["middleware-detections", "background-worker-detections"],
        ["model.Detections"],
        "Walks Program.cs files for middleware registration order and background worker detection");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var programFiles = context.Analysis.AllSourceFiles
            .Where(f => Path.GetFileName(f).Equals("Program.cs", StringComparison.OrdinalIgnoreCase))
            .ToList();

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
                    mapRegistrations.Add((methodName, methodName, lineNumber));

                    model.Detections.Add(new MiddlewareDetection(
                        MiddlewareType: methodName,
                        PipelineOrder: 100 + mapRegistrations.Count,
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
            if (methodName != "AddHostedService") continue;

            var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            var implementationType = "?";

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

    private static string DetermineWorkerServiceType(string implementationType)
    {
        if (implementationType.Contains("BackgroundService"))
            return "BackgroundService";
        if (implementationType.Contains("IHostedService"))
            return "IHostedService";
        return "IHostedService";
    }
}
