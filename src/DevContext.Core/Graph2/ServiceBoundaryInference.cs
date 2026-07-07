using DevContext.Core.Contracts;
using DevContext.Core.Graph;

namespace DevContext.Core.Graph2;

public static class ServiceBoundaryInference
{
    public static bool IsRunnableService(ProjectInfo p)
    {
        var isExe = p.OutputType?.Contains("Exe", StringComparison.OrdinalIgnoreCase) == true;
        var isWebSdk = p.PackageReferences.Any(
            pr => pr.Name.Contains("AspNetCore", StringComparison.OrdinalIgnoreCase));
        var hasWebServer = p.FilePath is { } cp && IsWebSdkProject(cp);
        return isExe || isWebSdk || hasWebServer;
    }

    private static bool IsWebSdkProject(string csprojPath)
    {
        try
        {
            var text = File.ReadAllText(csprojPath);
            return text.Contains("Microsoft.NET.Sdk.Web", StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    public static ImmutableArray<ProjectInfo> RunnableProjects(SolutionScope scope)
    {
        return scope.Projects
            .Where(p => IsRunnableService(p))
            .ToImmutableArray();
    }
}
