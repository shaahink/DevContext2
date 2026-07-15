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
        var isExe = p.OutputType?.Contains("Exe", StringComparison.OrdinalIgnoreCase) == true;
        var hasAspNetFramework = p.PackageReferences.Any(
            pr => pr.Name.StartsWith("Microsoft.AspNetCore.App", StringComparison.OrdinalIgnoreCase));
        var hasWebServer = p.FilePath is { } cp && IsWebSdkProject(cp);
        return isExe || hasAspNetFramework || hasWebServer;
    }

    private static bool IsWebSdkProject(string csprojPath)
    {
        if (_webSdkCache.TryGetValue(csprojPath, out var cached))
            return cached;

        try
        {
            var text = File.ReadAllText(csprojPath);
            var result = text.Contains("Microsoft.NET.Sdk.Web", StringComparison.OrdinalIgnoreCase);
            _webSdkCache[csprojPath] = result;
            return result;
        }
        catch
        {
            _webSdkCache[csprojPath] = false;
            return false;
        }
    }

    public static ImmutableArray<ProjectInfo> RunnableProjects(SolutionScope scope)
    {
        // T1.9 — the service topology is production only. eShop's FunctionalTests reference the ASP.NET
        // Core shared framework (WebApplicationFactory), so IsRunnableService would render 5 test projects
        // as service cards. Exclude test/benchmark/sample projects by classification, not path regex.
        return scope.Projects
            .Where(p => ProjectClassifier.IsProductionProject(p) && IsRunnableService(p))
            .ToImmutableArray();
    }
}
