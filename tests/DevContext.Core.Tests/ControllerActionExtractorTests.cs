namespace DevContext.Core.Tests;

public sealed class ControllerActionExtractorTests
{
    private static (DiscoveryContext Ctx, DiscoveryModel Model) Setup(string source)
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\Controllers\TestController.cs", source);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"C:\repo\Controllers\TestController.cs"];

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Controllers));
        return (ctx, model);
    }

    [Fact]
    public async Task Detects_controller_with_HttpsVerb_attributes()
    {
        var (ctx, model) = Setup("""
            [ApiController]
            [Route("api/[controller]")]
            public class TestController : ControllerBase
            {
                [HttpGet]
                public IActionResult GetAll() => Ok();
                [HttpPost]
                public IActionResult Create() => Ok();
            }
            """);
        var extractor = new ControllerActionExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var endpoints = model.Detections.OfType<EndpointDetection>().ToArray();
        Assert.Equal(2, endpoints.Length);
        Assert.Contains(endpoints, e => string.Equals(e.HttpMethod, "GET", StringComparison.Ordinal) && string.Equals(e.HandlerMethod, "GetAll", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.HttpMethod, "POST", StringComparison.Ordinal) && string.Equals(e.HandlerMethod, "Create", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Infers_HTTP_verb_from_method_name_when_only_Route_present()
    {
        var (ctx, model) = Setup("""
            [ApiController]
            [Route("[controller]")]
            public class TestController : ControllerBase
            {
                [Route("get-all")]
                public IActionResult FetchAll() => Ok();
                [Route("create")]
                public IActionResult PostNew() => Ok();
                [Route("remove/{id}")]
                public IActionResult DeleteItem(int id) => NoContent();
                [Route("update/{id}")]
                public IActionResult PatchItem(int id) => NoContent();
                [Route("replace/{id}")]
                public IActionResult PutItem(int id) => NoContent();
            }
            """);
        var extractor = new ControllerActionExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var endpoints = model.Detections.OfType<EndpointDetection>().ToArray();
        Assert.Equal(5, endpoints.Length);
        Assert.Contains(endpoints, e => string.Equals(e.HandlerMethod, "FetchAll", StringComparison.Ordinal) && string.Equals(e.HttpMethod, "GET", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.HandlerMethod, "PostNew", StringComparison.Ordinal) && string.Equals(e.HttpMethod, "POST", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.HandlerMethod, "DeleteItem", StringComparison.Ordinal) && string.Equals(e.HttpMethod, "DELETE", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.HandlerMethod, "PatchItem", StringComparison.Ordinal) && string.Equals(e.HttpMethod, "PATCH", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.HandlerMethod, "PutItem", StringComparison.Ordinal) && string.Equals(e.HttpMethod, "PUT", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Expands_controller_token_in_route()
    {
        var (ctx, model) = Setup("""
            [ApiController]
            [Route("api/[controller]")]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult List() => Ok();
            }
            """);
        var extractor = new ControllerActionExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var endpoint = model.Detections.OfType<EndpointDetection>().Single();
        // Route should expand "api/[controller]" to "api/Orders"
        Assert.Equal("/api/Orders", endpoint.RouteTemplate);
    }

    [Fact]
    public async Task Convention_routing_with_action_token_detects_all_public_methods()
    {
        var (ctx, model) = Setup("""
            [ApiController]
            [Route("[controller]/[action]")]
            public class FeedController : ControllerBase
            {
                public IActionResult Index() => Ok();
                public IActionResult Posts() => Ok();
                public IActionResult Comments() => Ok();
                public IActionResult SiteFeed() => Ok();
            }
            """);
        var extractor = new ControllerActionExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var endpoints = model.Detections.OfType<EndpointDetection>().ToArray();
        Assert.Equal(4, endpoints.Length);
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/Feed/Posts", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/Feed/Comments", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/Feed/Index", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/Feed/SiteFeed", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Handles_multiple_Route_attributes_per_action()
    {
        var (ctx, model) = Setup("""
            [ApiController]
            [Route("[controller]/[action]")]
            public class FeedController : ControllerBase
            {
                [Route("/rss")]
                [Route("/rss.xml")]
                [Route("/feed/rss")]
                public IActionResult SiteFeed() => Ok();
            }
            """);
        var extractor = new ControllerActionExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var endpoints = model.Detections.OfType<EndpointDetection>().ToArray();
        Assert.Equal(3, endpoints.Length);
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/rss", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/rss.xml", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/feed/rss", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Absolute_route_overrides_controller_route()
    {
        var (ctx, model) = Setup("""
            [ApiController]
            [Route("[controller]")]
            public class SitemapController : ControllerBase
            {
                [Route("/sitemap.xml")]
                public IActionResult Get() => Ok();
            }
            """);
        var extractor = new ControllerActionExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var endpoint = model.Detections.OfType<EndpointDetection>().Single();
        Assert.Equal("/sitemap.xml", endpoint.RouteTemplate);
    }

    [Fact]
    public async Task Fully_qualified_Route_attribute_detected()
    {
        var (ctx, model) = Setup("""
            [ApiController]
            [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
            public class FtsController : ControllerBase
            {
                [Microsoft.AspNetCore.Mvc.HttpPost]
                [Route("search")]
                public IActionResult Search() => Ok();
            }
            """);
        var extractor = new ControllerActionExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var endpoint = model.Detections.OfType<EndpointDetection>().Single();
        Assert.Equal("/api/Fts/search", endpoint.RouteTemplate);
        Assert.Equal("POST", endpoint.HttpMethod);
    }

    [Fact]
    public async Task NonAction_methods_are_skipped()
    {
        var (ctx, model) = Setup("""
            [ApiController]
            [Route("[controller]/[action]")]
            public class TestController : ControllerBase
            {
                public IActionResult Visible() => Ok();
                [NonAction]
                public IActionResult Hidden() => Ok();
            }
            """);
        var extractor = new ControllerActionExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var endpoints = model.Detections.OfType<EndpointDetection>().ToArray();
        Assert.Single(endpoints);
        Assert.Equal("Visible", endpoints[0].HandlerMethod);
    }
}
