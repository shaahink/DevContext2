using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>
/// W1 (assessment): the entry-point inventory must exclude non-runtime sources that a framework-scale repo
/// (aspnetcore) is full of — test assets, stress/perf harnesses, test-server infrastructure, and project
/// templates — none of which are application entry points. Paths below are the real ones sampled from the
/// aspnetcore Map export that leaked 518 HTTP "entries". The project classifier can't catch them (they are
/// not test <i>projects</i>), so <see cref="NoiseFilter.IsProductionEntrySource"/> adds the path rules.
/// </summary>
public sealed class NoiseFilterTests
{
    // No registered test projects: these assertions exercise the PATH rules, independent of project shape.
    private static NoiseFilter Filter() => new(new ProjectClassifier(ImmutableArray<ProjectInfo>.Empty));

    [Theory]
    // Test assets under a /test/ tree (caught by IsTestPath).
    [InlineData(@"C:\repo\src\Http\Routing\test\testassets\RoutingWebSite\UseRouterStartup.cs")]
    [InlineData(@"C:\repo\src\Mvc\test\WebSites\SimpleWebSiteWithWebApplicationBuilder\Program.cs")]
    [InlineData(@"C:\repo\src\Components\test\testassets\Components.TestServer\RemoteAuthenticationStartup.cs")]
    // Non-/test/ path conventions (caught by IsNonRuntimeEntrySource).
    [InlineData(@"C:\repo\src\Servers\Kestrel\stress\Program.cs")]
    [InlineData(@"C:\repo\src\Components\Testing\src\Infrastructure\ServerFixture.cs")]
    [InlineData(@"C:\repo\src\SignalR\clients\ts\FunctionalTests\Startup.cs")]
    [InlineData(@"C:\repo\src\ProjectTemplates\Web.ProjectTemplates\content\WebApi-CSharp\Program.cs")]
    public void Excludes_non_runtime_entry_sources(string path)
        => Assert.False(Filter().IsProductionEntrySource(path), $"expected NON-production entry source: {path}");

    [Theory]
    // Real production entry sources that MUST survive (aspnetcore Identity API + a normal app layout).
    [InlineData(@"C:\repo\src\Identity\Core\src\IdentityApiEndpointRouteBuilderExtensions.cs")]
    [InlineData(@"C:\repo\src\Mvc\Mvc.Core\src\Routing\X.cs")]
    [InlineData(@"C:\repo\src\Orders.Api\Endpoints\OrderEndpoints.cs")]
    public void Keeps_production_entry_sources(string path)
        => Assert.True(Filter().IsProductionEntrySource(path), $"expected production entry source: {path}");

    [Fact]
    public void Still_excludes_generated_and_sample_sources()
    {
        var f = Filter();
        Assert.False(f.IsProductionEntrySource(@"C:\repo\src\App\obj\Debug\App.g.cs"));
        Assert.False(f.IsProductionEntrySource(@"C:\repo\samples\Demo\Program.cs"));
        Assert.False(f.IsProductionEntrySource(@"C:\repo\src\App\Migrations\0001_Init.cs"));
    }

    // Path-convention checks are relative to the analysis root: a repo that itself lives under a
    // `…/tests/…` path (our own fixtures) keeps its surface; test/sample dirs BELOW the root are excluded.
    private static NoiseFilter FilterRooted(string root) => new(new ProjectClassifier(ImmutableArray<ProjectInfo>.Empty), root);

    [Fact]
    public void Root_under_a_test_path_does_not_exclude_the_repos_own_surface()
    {
        // Analysing tests/fixtures/ControllerApp: the /tests/ is ABOVE the root → its endpoints survive.
        var f = FilterRooted(@"C:\dev\DevContext\tests\fixtures\ControllerApp");
        Assert.True(f.IsProductionEntrySource(@"C:\dev\DevContext\tests\fixtures\ControllerApp\Controllers\ProductsController.cs"));
    }

    [Fact]
    public void Test_and_template_dirs_below_the_root_are_still_excluded()
    {
        var f = FilterRooted(@"C:\src\aspnetcore");
        Assert.False(f.IsProductionEntrySource(@"C:\src\aspnetcore\src\Mvc\test\WebSites\SimpleWebSite\Program.cs"));
        Assert.False(f.IsProductionEntrySource(@"C:\src\aspnetcore\src\Servers\Kestrel\stress\Program.cs"));
        Assert.True(f.IsProductionEntrySource(@"C:\src\aspnetcore\src\Identity\Core\src\IdentityApiEndpointRouteBuilderExtensions.cs"));
    }
}
