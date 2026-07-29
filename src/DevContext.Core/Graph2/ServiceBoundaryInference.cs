using DevContext.Core.Contracts;
using DevContext.Core.Graph;

namespace DevContext.Core.Graph2;

public static class ServiceBoundaryInference
{
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
        // Batch D (R2 §2.D): SDK evidence now travels on ProjectInfo (parsed once from the cached
        // XDocument). This used to be three cached File.ReadAllText probes asking whether the marker
        // appeared ANYWHERE in the csproj text.
        var isExe = p.OutputType?.Contains("Exe", StringComparison.OrdinalIgnoreCase) == true;
        var hasAspNetFramework = p.PackageReferences.Any(
            pr => pr.Name.StartsWith("Microsoft.AspNetCore.App", StringComparison.OrdinalIgnoreCase));
        return isExe || hasAspNetFramework
            || p.HasSdk(SdkIds.Web) || p.HasSdk(SdkIds.Worker) || p.HasSdk(SdkIds.AspireAppHost);
    }

    public static ImmutableArray<ProjectInfo> RunnableProjects(SolutionScope scope, bool samplesAreTheProduct = false)
    {
        // T1.9 — the service topology is production only. eShop's FunctionalTests reference the ASP.NET
        // Core shared framework (WebApplicationFactory), so IsRunnableService would render 5 test projects
        // as service cards. Exclude test/benchmark/sample projects by classification, not path regex.
        // T8: in a samples-only repo the sample hosts ARE the services (tests/benchmarks still excluded).
        // D1.1b: the classifier instance adds holder + transitive build-tooling exclusion (audit A3/E2).
        var classifier = new ProjectClassifier(scope.Projects);
        return scope.Projects
            .Where(p => classifier.IsProduction(p, samplesAreTheProduct) && IsRunnableService(p))
            .ToImmutableArray();
    }

    /// <summary>
    /// R3 D-4 (G6.3) — the runnable production projects the analysed solution does NOT contain.
    /// <para>Same predicate as <see cref="RunnableProjects"/>, different population: the projects
    /// discovery found on disk minus the ones in <paramref name="scope"/>. A repo that declares
    /// several solutions is analysed one solution at a time (<see cref="SolutionScope"/>), and
    /// <c>SolutionScopeNote</c> already says which one and how many exist — but it never said that
    /// runnable APPS were among the ones you are not seeing. dotnet-podcasts keeps its two MAUI
    /// clients in sibling solutions: the engine parses their csprojs (that is where the mobile TFM
    /// triple is read from) and then had no word for them.</para>
    /// <para>These are NEVER called services. The canvas draws <see cref="RunnableProjects"/>; this
    /// list names the boundary. No project can appear in both.</para>
    /// </summary>
    public static ImmutableArray<ProjectInfo> RunnableProjectsOutsideScope(
        DiscoveryModel model, SolutionScope scope, bool samplesAreTheProduct = false)
    {
        if (scope.Projects.Length >= model.Projects.Length) return [];
        var inScope = scope.Projects
            .Select(p => p.FilePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        // The classifier reads the WHOLE project set: build-tooling exclusion is transitive, so
        // handing it only the leftovers would classify a Cake bootstrapper as production.
        var classifier = new ProjectClassifier(model.Projects);
        return model.Projects
            .Where(p => !inScope.Contains(p.FilePath))
            .Where(p => classifier.IsProduction(p, samplesAreTheProduct) && IsRunnableService(p))
            .ToImmutableArray();
    }
}
