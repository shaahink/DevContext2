using DevContext.Core.Contracts;
using DevContext.Core.Graph;
using DevContext.Core.Graph2;

namespace DevContext.Core.Tests.Graph2;

public sealed class ServiceBoundaryInferenceTests
{
    [Fact]
    public void Exe_project_is_runnable()
    {
        var p = new ProjectInfo("MyApp", @"C:\repo\MyApp\MyApp.csproj", "C#", ["net10.0"], [], [], OutputType: "Exe");
        Assert.True(ServiceBoundaryInference.IsRunnableService(p));
    }

    [Fact]
    public void WinExe_project_is_runnable()
    {
        var p = new ProjectInfo("Desktop", @"C:\repo\Desktop\Desktop.csproj", "C#", ["net10.0"], [], [], OutputType: "WinExe");
        Assert.True(ServiceBoundaryInference.IsRunnableService(p));
    }

    [Fact]
    public void AspNetCore_package_reference_makes_project_runnable()
    {
        var p = new ProjectInfo("WebApi", @"C:\repo\WebApi\WebApi.csproj", "C#", ["net10.0"], [],
            [new PackageReferenceInfo("Microsoft.AspNetCore.App", "9.0.0")]);
        Assert.True(ServiceBoundaryInference.IsRunnableService(p));
    }

    [Fact]
    public void Microsoft_AspNetCore_App_framework_reference_makes_project_runnable()
    {
        var p = new ProjectInfo("WebApi", @"C:\repo\WebApi\WebApi.csproj", "C#", ["net10.0"], [],
            [new PackageReferenceInfo("Microsoft.AspNetCore.App.Ref", "9.0.0")]);
        Assert.True(ServiceBoundaryInference.IsRunnableService(p));
    }

    [Fact]
    public void WebSdk_csproj_makes_project_runnable()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"dc-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            var csproj = Path.Combine(dir, "Web.csproj");
            File.WriteAllText(csproj, "<Project Sdk=\"Microsoft.NET.Sdk.Web\"></Project>");
            var p = new ProjectInfo("Web", csproj, "C#", ["net10.0"], [], []);
            Assert.True(ServiceBoundaryInference.IsRunnableService(p));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Class_library_is_not_runnable()
    {
        var p = new ProjectInfo("MyLib", @"C:\repo\MyLib\MyLib.csproj", "C#", ["net10.0"], [], []);
        Assert.False(ServiceBoundaryInference.IsRunnableService(p));
    }

    [Fact]
    public void Test_project_is_not_runnable_by_default()
    {
        var p = new ProjectInfo("MyApp.Tests", @"C:\repo\MyApp.Tests\MyApp.Tests.csproj", "C#", ["net10.0"], [], []);
        Assert.False(ServiceBoundaryInference.IsRunnableService(p));
    }

    [Fact]
    public void RunnableProjects_filters_correctly()
    {
        var projects = new[]
        {
            new ProjectInfo("Api", @"C:\repo\Api\Api.csproj", "C#", ["net10.0"], [], [new PackageReferenceInfo("Microsoft.AspNetCore.App", "9.0.0")]),
            new ProjectInfo("Core", @"C:\repo\Core\Core.csproj", "C#", ["net10.0"], [], []),
            new ProjectInfo("Worker", @"C:\repo\Worker\Worker.csproj", "C#", ["net10.0"], [], [], OutputType: "Exe"),
            new ProjectInfo("Tests", @"C:\repo\Tests\Tests.csproj", "C#", ["net10.0"], [], []),
        };
        var scope = new SolutionScope([.. projects]);
        var runnable = ServiceBoundaryInference.RunnableProjects(scope);
        Assert.Equal(2, runnable.Length);
        Assert.Contains(runnable, p => p.Name == "Api");
        Assert.Contains(runnable, p => p.Name == "Worker");
    }

    [Fact]
    public void IsWebSdkProject_returns_false_for_nonexistent_file()
    {
        var p = new ProjectInfo("Ghost", @"C:\nonexistent\Ghost.csproj", "C#", ["net10.0"], [], []);
        Assert.False(ServiceBoundaryInference.IsRunnableService(p));
    }
}
