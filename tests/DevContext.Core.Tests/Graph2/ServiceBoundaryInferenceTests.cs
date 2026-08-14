using DevContext.Core.Constants;
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
    public void WebSdk_project_is_runnable()
    {
        // Batch D (R2 §2.D): the SDK is EVIDENCE ON THE PROJECT, parsed once at load — this test used to
        // write a real csproj to a temp directory because the rule re-read the file off disk. It now
        // needs no filesystem at all, which is the point: the rule is pure and testable in memory.
        var p = new ProjectInfo("Web", @"C:\repo\Web\Web.csproj", "C#", ["net10.0"], [], [],
            Sdks: [SdkIds.Web]);
        Assert.True(ServiceBoundaryInference.IsRunnableService(p));
    }

    [Fact]
    public void AspireAppHost_sdk_declared_as_child_element_is_runnable()
    {
        // The AppHost SDK is normally added as <Sdk Name="Aspire.AppHost.Sdk" /> ALONGSIDE the root
        // Microsoft.NET.Sdk — the root-attribute-only read could never see it (Batch D).
        var p = new ProjectInfo("AppHost", @"C:\repo\AppHost\AppHost.csproj", "C#", ["net10.0"], [], [],
            Sdks: ["Microsoft.NET.Sdk", SdkIds.AspireAppHost]);
        Assert.True(ServiceBoundaryInference.IsRunnableService(p));
    }

    [Fact]
    public void Base_sdk_alone_does_not_answer_the_web_sdk_question()
    {
        // HasSdk is EQUALITY, not substring: the old text probe would say yes to any csproj that merely
        // mentioned the string anywhere in the file.
        var p = new ProjectInfo("Lib", @"C:\repo\Lib\Lib.csproj", "C#", ["net10.0"], [], [],
            Sdks: ["Microsoft.NET.Sdk"]);
        Assert.False(p.HasSdk(SdkIds.Web));
        Assert.False(ServiceBoundaryInference.IsRunnableService(p));
    }

    [Fact]
    public void Class_library_is_not_runnable()
    {
        var p = new ProjectInfo("MyLib", @"C:\repo\MyLib\MyLib.csproj", "C#", ["net10.0"], [], []);
        Assert.False(ServiceBoundaryInference.IsRunnableService(p));
    }

    [Fact]
    public void Library_with_aspnetcore_flavored_package_is_not_runnable()
    {
        // L4.3 regression: a class library that references FluentValidation.AspNetCore must NOT be
        // classified as a runnable service (audit Claim 3 / E3 — dogfood BuildingBlocks).
        var p = new ProjectInfo("BuildingBlocks", @"C:\repo\BuildingBlocks\BuildingBlocks.csproj", "C#", ["net8.0"], [],
            [new PackageReferenceInfo("FluentValidation.AspNetCore", "11.3.0"),
             new PackageReferenceInfo("MediatR", "12.2.0")]);
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
        var runnable = ServiceBoundaryInference.RunnableProjects(
            scope, new Models.DiscoveryModel { Projects = [.. projects] });
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
