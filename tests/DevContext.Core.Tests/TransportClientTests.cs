using DevContext.Core.Extractors.Generic;
using DevContext.Core.Extractors.Specific;
using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>Batch B (DC3) — the sync-transport seam: a client REGISTRATION names its target, and the
/// AppHost's resource table is what turns a service-discovery host into an analyzed project. Before
/// this, eShop's gRPC/HTTP clients produced zero links and its canvases showed floating boxes.</summary>
public sealed class TransportClientTests
{
    [Fact]
    public async Task GrpcClientRegistration_carries_its_type_argument_and_address()
    {
        // The exact eShop shape. The old generic Add* branch read args[0] — the configuration lambda —
        // and never looked at the type argument, so this registration became
        // DiRegistrationDetection("AddGrpcClient", "o => o.Address = ...").
        var detections = await RunDiExtractorAsync(
            """
            builder.Services.AddGrpcClient<Basket.BasketClient>(o => o.Address = new("http://basket-api"))
                .AddAuthToken();
            """);

        var client = Assert.Single(detections.OfType<TransportClientDetection>());
        Assert.Equal(TransportKinds.Grpc, client.Transport);
        Assert.Equal("Basket.BasketClient", client.ClientType);
        Assert.Equal("http://basket-api", client.Address);
    }

    [Fact]
    public async Task TypedHttpClient_reads_the_service_discovery_scheme()
    {
        var detections = await RunDiExtractorAsync(
            """
            builder.Services.AddHttpClient<CatalogService>(o => o.BaseAddress = new("https+http://catalog-api"))
                .AddApiVersion(2.0)
                .AddAuthToken();
            """);

        var client = Assert.Single(detections.OfType<TransportClientDetection>());
        Assert.Equal(TransportKinds.Http, client.Transport);
        Assert.Equal("CatalogService", client.ClientType);
        // "2.0" is a literal in the same chain: only scheme-bearing strings are addresses.
        Assert.Equal("https+http://catalog-api", client.Address);
    }

    [Fact]
    public async Task RefitClient_address_configured_on_a_chained_call_is_still_found()
    {
        var detections = await RunDiExtractorAsync(
            """
            services.AddRefitClient<ICatalogApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.contoso.com/catalog"));
            """);

        var client = Assert.Single(detections.OfType<TransportClientDetection>());
        Assert.Equal(TransportKinds.Refit, client.Transport);
        Assert.Equal("ICatalogApi", client.ClientType);
        Assert.Equal("https://api.contoso.com/catalog", client.Address);
    }

    [Fact]
    public async Task TypedHttpClient_with_interface_and_impl_stays_a_DI_binding_too()
    {
        // C6 (Prism D1.2f) must survive: AddHttpClient<IFeedClient, FeedClient> is a real
        // interface->impl binding AND a transport registration. The client is the implementation.
        var detections = await RunDiExtractorAsync(
            """
            builder.Services.AddHttpClient<IFeedClient, FeedClient>(c => c.BaseAddress = new("http://feeds"));
            """);

        var client = Assert.Single(detections.OfType<TransportClientDetection>());
        Assert.Equal("FeedClient", client.ClientType);
        Assert.Contains(detections.OfType<DiRegistrationDetection>(),
            d => d.ServiceType == "IFeedClient" && d.ImplementationType == "FeedClient");
    }

    [Fact]
    public async Task OrdinaryRegistrations_produce_no_transport_client()
    {
        var detections = await RunDiExtractorAsync(
            """
            builder.Services.AddScoped<IBasketService, BasketService>();
            builder.Services.AddSingleton<BasketState>();
            builder.Services.AddHostedService<Worker>();
            """);

        Assert.Empty(detections.OfType<TransportClientDetection>());
    }

    [Fact]
    public async Task AppHost_reference_points_from_the_chained_resource_to_the_argument()
    {
        // The direction is the whole point: `basketApi.WithEnvironment("Identity__Url", identityEndpoint)`
        // means basket-api depends on identity-api. The old reading took args[0] as the SOURCE and
        // args[1] as the target, so every relationship pointed at "?" and the vocabulary stayed dead.
        // `identityEndpoint` is a local standing for identityApi — resolving that alias is what makes
        // eShop's identity wiring visible at all.
        var resources = await RunAspireExtractorAsync(
            """
            var builder = DistributedApplication.CreateBuilder(args);
            var identityApi = builder.AddProject<Projects.Identity_API>("identity-api");
            var identityEndpoint = identityApi.GetEndpoint("http");
            var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
                .WithEnvironment("Identity__Url", identityEndpoint);
            builder.Build().Run();
            """);

        var basket = Assert.Single(resources.OfType<AspireResourceDetection>(),
            r => r.ResourceName == "basket-api");
        Assert.Equal("Basket_API", basket.ProjectRef);
        Assert.Equal("basketApi", basket.VariableName);

        var link = Assert.Single(resources.OfType<AspireRelationshipDetection>());
        Assert.Equal("basket-api", link.SourceResource);
        // Two alias hops: identityEndpoint -> identityApi -> the resource name it was declared with.
        Assert.Equal("identity-api", link.TargetResource);
    }

    [Fact]
    public async Task InjectedGrpcClient_behind_a_using_alias_is_still_a_client()
    {
        // The other half of the DC3 answer, at the injection site. eShop's BasketService takes its
        // client as `GrpcBasketClient` — a file-local alias for Basket.BasketClient. The X.XClient
        // shape test requires a dotted name, so the unqualified alias was rejected and eShop's gRPC
        // client was invisible from both ends at once.
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\WebApp\Services\BasketService.cs",
            """
            using GrpcBasketClient = eShop.Basket.API.Grpc.Basket.BasketClient;

            namespace eShop.WebApp.Services;

            public class BasketService(GrpcBasketClient basketClient)
            {
                public async Task DeleteBasketAsync() => await basketClient.DeleteBasketAsync(new());
            }
            """);

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Grpc));
        model.Architecture.Seal();

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\WebApp\Services\BasketService.cs"];

        await new GrpcClientExtractor().ExtractAsync(ctx, model, default);

        var client = Assert.Single(model.Detections.OfType<GrpcClientDetection>());
        Assert.Equal("Basket", client.ServiceName);
        Assert.Contains("DeleteBasketAsync", client.Methods);
    }

    [Fact]
    public void AddressBook_resolves_a_host_through_the_AppHost_resource_table()
    {
        var model = ModelWith(
            ["Basket.API", "WebApp"],
            new AspireResourceDetection("Project", "basket-api", null, "basketApi", "Basket_API")
            {
                ExtractorName = "AspireExtractor", SourceFile = @"C:\r\AppHost\AppHost.cs", LineNumber = 3,
            });

        var book = ServiceAddressBook.Build(model, new SolutionScope(model.Projects));

        Assert.Equal("Basket.API", book.ResolveHost("basket-api"));
        Assert.Equal("Basket.API", book.ProjectResources["basketApi"]);
    }

    [Fact]
    public void AddressBook_falls_back_to_a_normalized_project_name_without_an_AppHost()
    {
        // TodoApi's Todo.Web/Server points a typed client at http://todoapi with no Aspire mapping
        // needed: "todoapi" and "Todo.Api" are the same name once separators are dropped.
        var model = ModelWith(["Todo.Api", "Todo.Web.Server"]);
        var book = ServiceAddressBook.Build(model, new SolutionScope(model.Projects));

        Assert.Equal("Todo.Api", book.ResolveHost("todoapi"));
        Assert.Equal("Todo.Web.Server", book.ResolveHost("todo-web-server"));
        Assert.Null(book.ResolveHost("api.stripe.com"));
    }

    [Fact]
    public void AddressBook_refuses_to_pick_a_winner_between_same_named_projects()
    {
        // Batch A's rule holds here: a name-shaped guess never gets to choose silently. GitVersion
        // carries two GitVersion.Core projects; a host matching both must resolve to nothing.
        var model = ModelWith(["Catalog.Api", "catalog-api"]);
        var book = ServiceAddressBook.Build(model, new SolutionScope(model.Projects));

        Assert.Null(book.ResolveHost("catalogapi"));
    }

    [Theory]
    [InlineData("http://basket-api", "basket-api")]                 // plain service discovery
    [InlineData("https+http://catalog-api", "catalog-api")]         // .NET service-discovery scheme
    [InlineData("http://basket-api:8080", "basket-api")]            // port stripped
    [InlineData("https://api.contoso.com/v1/catalog", "api.contoso.com")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Host_extraction_handles_the_address_forms_registrations_use(string? address, string? expected)
        => Assert.Equal(expected, ServiceAddressBook.ExtractHost(address));

    [Theory]
    [InlineData("api.contoso.com", true)]      // a real service in someone else's repo
    [InlineData("payments-gateway", true)]
    [InlineData("localhost", false)]           // the app talking to itself
    [InlineData("127.0.0.1", false)]
    [InlineData("{ServiceUrl}", false)]        // an unsubstituted configuration placeholder
    [InlineData("8080", false)]                // no letters: not a name
    public void External_hosts_are_only_the_ones_that_name_something(string host, bool expected)
        => Assert.Equal(expected, ServiceAddressBook.IsExternalHost(host));

    private static DiscoveryModel ModelWith(string[] projectNames, params Detection[] detections)
    {
        var model = new DiscoveryModel
        {
            Projects = [.. projectNames.Select(n =>
                new ProjectInfo(n, $@"C:\r\{n}\{n}.csproj", "C#", ["net10.0"], [], []))],
        };
        foreach (var detection in detections) model.Detections.Add(detection);
        return model;
    }

    private static async Task<List<Detection>> RunDiExtractorAsync(string body)
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\App\Extensions.cs",
            $$"""
            public static class Extensions
            {
                public static void AddApplicationServices(this IHostApplicationBuilder builder)
                {
                    {{body}}
                }
            }
            """);

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\App\Extensions.cs"];

        var model = new DiscoveryModel();
        await new DiRegistrationExtractor().ExtractAsync(ctx, model, default);
        return [.. model.Detections];
    }

    private static async Task<List<Detection>> RunAspireExtractorAsync(string source)
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\AppHost\AppHost.cs", source);

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Aspire));
        model.Architecture.Seal();

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\AppHost\AppHost.cs"];

        await new AspireExtractor().ExtractAsync(ctx, model, default);
        return [.. model.Detections];
    }
}
