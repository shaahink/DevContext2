using System.Collections.Concurrent;
using DevContext.Core.Contracts;
using DevContext.Core.Graph;

namespace DevContext.Core.Graph2;

public static class ServiceBoundaryInference
{
    private static readonly ConcurrentDictionary<string, bool> _webSdkCache = new(StringComparer.OrdinalIgnoreCase);

    public static bool IsRunnableService(ProjectInfo p)
    {
        // Design §2.4 runnable signals: OutputType=Exe, Web SDK (Microsoft.NET.Sdk.Web), or the
        // ASP.NET Core shared-framework reference (Microsoft.AspNetCore.App[.Ref]).
        // A regular package whose name merely CONTAINS "AspNetCore" (e.g. FluentValidation.AspNetCore)
        // is NOT a runnable signal — a class library referencing it is still a library
        // (audit Claim 3 / E3: BuildingBlocks rendered as a service off an AspNetCore package ref).
        // T1.4 — an implied-Exe SDK is a runnable signal even with no explicit <OutputType>: the Worker SDK
        // (a hosted-service background host, shamshir's TradingEngine.Host) and the Aspire AppHost SDK
        // (the orchestrator, shamshir's TradingEngine.AppHost) both produce executables. Without this the
        // per-service list showed only the Web + Exe-console projects and missed the worker and the AppHost.
        var isExe = p.OutputType?.Contains("Exe", StringComparison.OrdinalIgnoreCase) == true;
        var hasAspNetFramework = p.PackageReferences.Any(
            pr => pr.Name.StartsWith("Microsoft.AspNetCore.App", StringComparison.OrdinalIgnoreCase));
        var hasWebServer = p.FilePath is { } cp && CsprojSdkContains(cp, "Microsoft.NET.Sdk.Web");
        var isWorkerHost = p.FilePath is { } wp && CsprojSdkContains(wp, "Microsoft.NET.Sdk.Worker");
        var isAspireHost = p.FilePath is { } ap && CsprojSdkContains(ap, "Aspire.AppHost.Sdk");
        return isExe || hasAspNetFramework || hasWebServer || isWorkerHost || isAspireHost;
    }

    /// <summary>True when the csproj's <c>Sdk</c> attribute contains the given marker (cached read). Used
    /// for Web / Worker / Aspire-AppHost SDK detection where the SDK implies an executable output.</summary>
    internal static bool CsprojSdkContains(string csprojPath, string marker)
    {
        var key = csprojPath + "|" + marker;
        if (_webSdkCache.TryGetValue(key, out var cached))
            return cached;

        try
        {
            var text = File.ReadAllText(csprojPath);
            var result = text.Contains(marker, StringComparison.OrdinalIgnoreCase);
            _webSdkCache[key] = result;
            return result;
        }
        catch
        {
            _webSdkCache[key] = false;
            return false;
        }
    }

    public static ImmutableArray<ProjectInfo> RunnableProjects(SolutionScope scope, bool samplesAreTheProduct = false)
    {
        // T1.9 — the service topology is production only. eShop's FunctionalTests reference the ASP.NET
        // Core shared framework (WebApplicationFactory), so IsRunnableService would render 5 test projects
        // as service cards. Exclude test/benchmark/sample projects by classification, not path regex.
        // T8: in a samples-only repo the sample hosts ARE the services (tests/benchmarks still excluded).
        return scope.Projects
            .Where(p => ProjectClassifier.IsProductionProject(p, samplesAreTheProduct) && IsRunnableService(p))
            .ToImmutableArray();
    }
}
