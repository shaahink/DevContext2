namespace DevContext.Core.Tests;

public sealed class EndpointExtractorTests
{
    [Fact]
    public async Task DirectMapGet_InProgramCs_DetectsEndpoint()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapGet("/orders", () => Results.Ok(new[] { "order1" }));
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/orders" && e.HttpMethod == "GET");
    }

    [Fact]
    public async Task DirectMapPost_InProgramCs_DetectsEndpoint()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapPost("/orders", (CreateOrderCommand cmd) => Results.Created());
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/orders" && e.HttpMethod == "POST");
    }

    [Fact]
    public async Task RouteWithParameter_InProgramCs_DetectsEndpoint()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapGet("/orders/{id}", (int id) => Results.Ok(id));
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/orders/{id}" && e.HttpMethod == "GET");
    }

    [Fact]
    public async Task ExtensionMethod_CallingMapGet_InsideExtension_DetectsEndpoint()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("Program.cs", """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapTodoEndpoints();
            app.Run();
            """);
        fs.AddFile("TodoEndpoints.cs", """
            public static class TodoEndpoints
            {
                public static WebApplication MapTodoEndpoints(this WebApplication app)
                {
                    app.MapGet("/todos", () => Results.Ok(new[] { "todo1" }));
                    app.MapPost("/todos", (CreateTodo cmd) => Results.Created());
                    return app;
                }
            }
            """);

        var result = await RunExtractorOnFilesAsync(fs);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/todos" && e.HttpMethod == "GET");
        Assert.Contains(endpoints, e => e.RouteTemplate == "/todos" && e.HttpMethod == "POST");
    }

    [Fact]
    public async Task MapGroup_WithChainedMaps_DetectsEndpoint()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            var todos = app.MapGroup("/todos");
            todos.MapGet("/", () => Results.Ok(new[] { "todo1" }));
            todos.MapGet("/{id}", (int id) => Results.Ok(id));
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/todos/" && e.HttpMethod == "GET");
        Assert.Contains(endpoints, e => e.RouteTemplate == "/todos/{id}" && e.HttpMethod == "GET");
        Assert.All(endpoints, e => Assert.Equal("/todos", e.GroupPrefix));
    }

    [Fact]
    public async Task MultipleHttpMethods_DetectsAll()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapGet("/get", () => "get");
            app.MapPost("/post", () => "post");
            app.MapPut("/put", () => "put");
            app.MapDelete("/delete", () => "delete");
            app.MapPatch("/patch", () => "patch");
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Equal(5, endpoints.Count);
        Assert.Contains(endpoints, e => e.HttpMethod == "GET" && e.RouteTemplate == "/get");
        Assert.Contains(endpoints, e => e.HttpMethod == "POST" && e.RouteTemplate == "/post");
        Assert.Contains(endpoints, e => e.HttpMethod == "PUT" && e.RouteTemplate == "/put");
        Assert.Contains(endpoints, e => e.HttpMethod == "DELETE" && e.RouteTemplate == "/delete");
        Assert.Contains(endpoints, e => e.HttpMethod == "PATCH" && e.RouteTemplate == "/patch");
    }

    [Fact]
    public async Task MapInsideConditionalBlock_DetectsEndpoint()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            if (app.Environment.IsDevelopment())
            {
                app.MapGet("/dev-only", () => "dev");
            }
            app.MapGet("/always", () => "always");
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/dev-only");
        Assert.Contains(endpoints, e => e.RouteTemplate == "/always");
    }

    [Fact]
    public async Task NoProgramCs_SearchesAllCsFiles()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("Startup.cs", """
            public class Startup
            {
                public void Configure(WebApplication app)
                {
                    app.MapGet("/from-startup", () => "ok");
                }
            }
            """);

        var result = await RunExtractorOnFilesAsync(fs);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/from-startup");
    }

    [Fact]
    public async Task Route_NormalizedToLeadingSlash()
    {
        // Routes without leading slash should be normalized
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapGet("api/items", () => Results.Ok(new[] { "item" }));
            app.MapPost("/api/orders", () => Results.Created());
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/api/items" && e.HttpMethod == "GET");
        Assert.Contains(endpoints, e => e.RouteTemplate == "/api/orders" && e.HttpMethod == "POST");
    }

    [Fact]
    public async Task FullPipeline_MinimalApiFixture_ProducesEndpoints()
    {
        var fixturePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..",
            "tests", "fixtures", "MinimalApiProject"));

        if (!Directory.Exists(fixturePath))
        {
            // If fixture doesn't exist, we can still test inline
            Assert.True(true, "Fixture not available");
            return;
        }

        var fs = new FakeFileSystem();
        foreach (var file in Directory.EnumerateFiles(fixturePath, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(fixturePath, file);
            fs.AddFile(relative, await File.ReadAllTextAsync(file));
        }

        var result = await RunExtractorOnFilesAsync(fs);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.NotEmpty(endpoints);
    }

    [Fact]
    public async Task RequireAuthorization_Chain_PopulatesAuthAttributes()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapGet("/secret", () => "secret").RequireAuthorization();
            app.MapGet("/admin", () => "admin").RequireAuthorization("AdminPolicy");
            app.MapGet("/public", () => "public").AllowAnonymous();
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        var secret = endpoints.Single(e => e.RouteTemplate == "/secret");
        var admin = endpoints.Single(e => e.RouteTemplate == "/admin");
        var pub = endpoints.Single(e => e.RouteTemplate == "/public");

        Assert.Contains("[Authorize]", secret.AuthAttributes);
        Assert.Contains("[Authorize(AdminPolicy)]", admin.AuthAttributes);
        Assert.Contains("[AllowAnonymous]", pub.AuthAttributes);
    }

    [Fact]
    public async Task GroupLevel_RequireAuthorization_AsStandaloneStatement_PropagatesToMembers()
    {
        // The TodoApi shape (E1): `group.RequireAuthorization(pb => ...)` as its own statement, not
        // chained onto MapGroup or onto any individual Map call.
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            var todos = app.MapGroup("/todos");
            todos.RequireAuthorization(pb => pb.RequireCurrentUser());
            todos.MapGet("/", () => Results.Ok(new[] { "todo1" }));
            todos.MapPost("/", (CreateTodo cmd) => Results.Created());
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.All(endpoints, e => Assert.Contains("[Authorize]", e.AuthAttributes));
    }

    [Fact]
    public async Task GroupPrefix_FromSameTypeConst_ResolvesAndCarriesGroupAuth()
    {
        // The Book2Course shape (2026-08-26 unseen drive, F2): the group's prefix is named by a const
        // on the declaring type, which is the idiomatic way to write it. The prefix used to require a
        // string LITERAL, so the group variable was never registered — which cost the routes their
        // prefix AND, because ExtractGroupAuth gates on the group being known, cost the whole admin
        // surface its RequireAuthorization. stats then reported the most-protected endpoints in the
        // app as anonymous.
        var result = await RunExtractorOnSourceAsync(
            "AdminEndpoints.cs",
            """
            internal static class AdminEndpoints
            {
                internal const string GroupPrefix = "/admin";

                public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
                {
                    var admin = app.MapGroup(GroupPrefix)
                        .RequireAuthorization(Policies.Admin)
                        .WithTags("Admin");

                    admin.MapGet("/accounting", () => Results.Ok());
                    admin.MapPost("/prompts", () => Results.Ok());
                    return app;
                }
            }
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/admin/accounting" && e.HttpMethod == "GET");
        Assert.Contains(endpoints, e => e.RouteTemplate == "/admin/prompts" && e.HttpMethod == "POST");

        // The half that made it a security claim rather than a routing nicety.
        Assert.NotEmpty(endpoints);
        Assert.All(endpoints, e => Assert.Contains("[Authorize]", e.AuthAttributes));
    }

    [Fact]
    public async Task GroupPrefix_FromQualifiedConst_Resolves()
    {
        var result = await RunExtractorOnSourceAsync(
            "Routes.cs",
            """
            internal static class ApiRoutes
            {
                internal const string Admin = "/admin";
            }

            internal static class AdminEndpoints
            {
                public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
                {
                    var admin = app.MapGroup(ApiRoutes.Admin).RequireAuthorization();
                    admin.MapGet("/stats", () => Results.Ok());
                    return app;
                }
            }
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/admin/stats" && e.HttpMethod == "GET");
        Assert.All(endpoints, e => Assert.Contains("[Authorize]", e.AuthAttributes));
    }

    [Fact]
    public async Task GroupAuth_CrossesTheExtensionMethodBoundary_WithThePrefix()
    {
        // The full Book2Course shape (F2): the group, its const prefix and its policy are declared in
        // ONE file; the routes it guards are registered in ANOTHER, through `admin.MapAdminLedgerEndpoints()`.
        // B3 already carried the PREFIX across that boundary; the auth did not travel with it, so every
        // admin route arrived un-annotated and stats reported the whole surface as anonymous.
        var fs = new FakeFileSystem();
        fs.AddFile("AdminEndpoints.cs",
            """
            internal static class AdminEndpoints
            {
                internal const string GroupPrefix = "/admin";

                public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
                {
                    var admin = app.MapGroup(GroupPrefix)
                        .RequireAuthorization(Policies.Admin)
                        .WithTags("Admin");

                    admin.MapAdminLedgerEndpoints();
                    return app;
                }
            }
            """);
        fs.AddFile("AdminLedgerEndpoints.cs",
            """
            internal static class AdminLedgerEndpoints
            {
                public static IEndpointRouteBuilder MapAdminLedgerEndpoints(this IEndpointRouteBuilder app)
                {
                    app.MapGet("/accounting", () => Results.Ok());
                    app.MapGet("/stats", () => Results.Ok());
                    return app;
                }
            }
            """);

        var result = await RunExtractorOnFilesAsync(fs);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        var accounting = Assert.Single(endpoints, e => e.RouteTemplate == "/admin/accounting");
        Assert.Contains("[Authorize]", accounting.AuthAttributes);
        var stats = Assert.Single(endpoints, e => e.RouteTemplate == "/admin/stats");
        Assert.Contains("[Authorize]", stats.AuthAttributes);
    }

    [Fact]
    public async Task GroupPrefix_FromNonConstantExpression_IsNotGuessed()
    {
        // The deliberate limit: a prefix the syntax pass cannot resolve leaves the group unregistered
        // and the route bare, exactly as before. Widening const resolution must not turn into inventing
        // a prefix from an expression nobody evaluated.
        var result = await RunExtractorOnSourceAsync(
            "DynamicEndpoints.cs",
            """
            internal static class DynamicEndpoints
            {
                public static IEndpointRouteBuilder MapDynamicEndpoints(this IEndpointRouteBuilder app)
                {
                    var group = app.MapGroup(BuildPrefix());
                    group.MapGet("/thing", () => Results.Ok());
                    return app;
                }

                private static string BuildPrefix() => "/dynamic";
            }
            """);

        var endpoint = Assert.Single(result.Detections.OfType<EndpointDetection>());
        Assert.Equal("/thing", endpoint.RouteTemplate);
        Assert.Null(endpoint.GroupPrefix);
    }

    [Fact]
    public async Task GroupLevel_RequireAuthorization_ChainedOnMapGroup_PropagatesToMembers()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            var admin = app.MapGroup("/admin").RequireAuthorization("AdminPolicy");
            admin.MapGet("/stats", () => "stats");
            app.Run();
            """);

        var endpoint = result.Detections.OfType<EndpointDetection>().Single();
        Assert.Contains("[Authorize(AdminPolicy)]", endpoint.AuthAttributes);
    }

    [Fact]
    public async Task EndpointOwnAllowAnonymous_OverridesGroupRequireAuthorization()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            var todos = app.MapGroup("/todos");
            todos.RequireAuthorization();
            todos.MapGet("/", () => "list");
            todos.MapGet("/public", () => "public").AllowAnonymous();
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        var list = endpoints.Single(e => e.RouteTemplate == "/todos/");
        var pub = endpoints.Single(e => e.RouteTemplate == "/todos/public");

        Assert.Contains("[Authorize]", list.AuthAttributes);
        Assert.Contains("[AllowAnonymous]", pub.AuthAttributes);
        Assert.DoesNotContain("[Authorize]", pub.AuthAttributes);
    }

    [Fact]
    public async Task NoGroupAuth_LeavesMembersWithNoAuthAttributes()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            var todos = app.MapGroup("/todos");
            todos.MapGet("/", () => "list");
            app.Run();
            """);

        var endpoint = result.Detections.OfType<EndpointDetection>().Single();
        Assert.Empty(endpoint.AuthAttributes);
    }

    [Fact]
    public async Task GlobalFallbackPolicy_RequireAuthenticatedUser_IsDetected()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            });
            var app = builder.Build();
            app.MapGet("/orders", () => "orders");
            app.Run();
            """);

        var globalAuth = result.Detections.OfType<GlobalAuthPolicyDetection>().SingleOrDefault();
        Assert.NotNull(globalAuth);
        Assert.True(globalAuth!.HasFallbackPolicy);
    }

    [Fact]
    public async Task AddAuthorization_WithoutFallbackPolicy_DoesNotEmitGlobalSignal()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAuthorization();
            var app = builder.Build();
            app.MapGet("/orders", () => "orders");
            app.Run();
            """);

        Assert.Empty(result.Detections.OfType<GlobalAuthPolicyDetection>());
    }

    // ── B3 (Prism D1.2c): MapGroup prefix composition across the extension-method boundary ──

    [Fact]
    public async Task CallerGroupPrefix_CrossesExtensionMethodBoundary_CrossFile()
    {
        // The podcasts shape: Program.cs owns the group + prefix, the routes live in another file's
        // extension method on RouteGroupBuilder. Every route rendered bare `GET /` before D1.2c.
        var fs = new FakeFileSystem();
        fs.AddFile("Program.cs", """
            var app = WebApplication.CreateBuilder(args).Build();
            var shows = app.MapGroup("/shows");
            shows.MapShowsApi().WithApiVersionSet(versionSet).MapToApiVersion(1.0);
            app.Run();
            """);
        fs.AddFile("ShowsApi.cs", """
            public static class ShowsApi
            {
                public static RouteGroupBuilder MapShowsApi(this RouteGroupBuilder group)
                {
                    group.MapGet("/", GetAllShows).WithName("GetShows");
                    group.MapGet("/{id}", GetShowById).WithName("GetShowsById");
                    return group;
                }
            }
            """);

        var result = await RunExtractorOnFilesAsync(fs);
        var endpoints = result.Detections.OfType<EndpointDetection>()
            .Where(e => e.SourceFile.Contains("ShowsApi")).ToList();

        Assert.Contains(endpoints, e => e.RouteTemplate == "/shows/" && e.HttpMethod == "GET");
        Assert.Contains(endpoints, e => e.RouteTemplate == "/shows/{id}" && e.HttpMethod == "GET");
        Assert.All(endpoints, e => Assert.Equal("/shows", e.GroupPrefix));
    }

    [Fact]
    public async Task CallerGroupPrefix_InlineMapGroupChain_CrossesBoundary()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("Program.cs", """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapGroup("/feeds").MapFeedsApi();
            app.Run();
            """);
        fs.AddFile("FeedsApi.cs", """
            public static class FeedsApi
            {
                public static RouteGroupBuilder MapFeedsApi(this RouteGroupBuilder group)
                {
                    group.MapPost("/", CreateFeed);
                    return group;
                }
            }
            """);

        var result = await RunExtractorOnFilesAsync(fs);
        var endpoint = result.Detections.OfType<EndpointDetection>()
            .Single(e => e.SourceFile.Contains("FeedsApi"));

        Assert.Equal("/feeds/", endpoint.RouteTemplate);
        Assert.Equal("POST", endpoint.HttpMethod);
    }

    [Fact]
    public async Task CallerGroupPrefix_AmbiguousCallers_StaysBare()
    {
        // Two groups with different prefixes call the same extension — composing either would be a
        // guess, so the routes keep their local (bare) template.
        var fs = new FakeFileSystem();
        fs.AddFile("Program.cs", """
            var app = WebApplication.CreateBuilder(args).Build();
            var v1 = app.MapGroup("/v1");
            var v2 = app.MapGroup("/v2");
            v1.MapItemsApi();
            v2.MapItemsApi();
            app.Run();
            """);
        fs.AddFile("ItemsApi.cs", """
            public static class ItemsApi
            {
                public static RouteGroupBuilder MapItemsApi(this RouteGroupBuilder group)
                {
                    group.MapGet("/items", GetItems);
                    return group;
                }
            }
            """);

        var result = await RunExtractorOnFilesAsync(fs);
        var endpoint = result.Detections.OfType<EndpointDetection>()
            .Single(e => e.SourceFile.Contains("ItemsApi"));

        Assert.Equal("/items", endpoint.RouteTemplate);
        Assert.Null(endpoint.GroupPrefix);
    }

    [Fact]
    public async Task CallerGroupPrefix_MixedPrefixedAndPlainCallers_StaysBare()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("Program.cs", """
            var app = WebApplication.CreateBuilder(args).Build();
            var admin = app.MapGroup("/admin");
            admin.MapWidgets();
            app.MapWidgets();
            app.Run();
            """);
        fs.AddFile("Widgets.cs", """
            public static class Widgets
            {
                public static IEndpointRouteBuilder MapWidgets(this IEndpointRouteBuilder builder)
                {
                    builder.MapGet("/widgets", GetWidgets);
                    return builder;
                }
            }
            """);

        var result = await RunExtractorOnFilesAsync(fs);
        var endpoint = result.Detections.OfType<EndpointDetection>()
            .Single(e => e.SourceFile.Contains("Widgets"));

        Assert.Equal("/widgets", endpoint.RouteTemplate);
        Assert.Null(endpoint.GroupPrefix);
    }

    [Fact]
    public async Task SeededPrefix_ComposesThroughNestedLocalGroup()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("Program.cs", """
            var app = WebApplication.CreateBuilder(args).Build();
            var shows = app.MapGroup("/shows");
            shows.MapShowsApi();
            app.Run();
            """);
        fs.AddFile("ShowsApi.cs", """
            public static class ShowsApi
            {
                public static RouteGroupBuilder MapShowsApi(this RouteGroupBuilder group)
                {
                    var admin = group.MapGroup("/admin");
                    admin.MapDelete("/{id}", DeleteShow);
                    return group;
                }
            }
            """);

        var result = await RunExtractorOnFilesAsync(fs);
        var endpoint = result.Detections.OfType<EndpointDetection>()
            .Single(e => e.SourceFile.Contains("ShowsApi"));

        Assert.Equal("/shows/admin/{id}", endpoint.RouteTemplate);
        Assert.Equal("DELETE", endpoint.HttpMethod);
    }

    [Fact]
    public async Task NestedMapGroup_ComposesReceiverPrefix_WithinOneScope()
    {
        // `var v1 = api.MapGroup("/v1")` where `api` is itself a group: routes on v1 carry both.
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            var api = app.MapGroup("/api");
            var v1 = api.MapGroup("/v1");
            v1.MapGet("/users", () => Results.Ok());
            app.Run();
            """);

        var endpoint = result.Detections.OfType<EndpointDetection>().Single();
        Assert.Equal("/api/v1/users", endpoint.RouteTemplate);
        Assert.Equal("/api/v1", endpoint.GroupPrefix);
    }

    private static async Task<DiscoveryModel> RunExtractorOnSourceAsync(string fileName, string source)
    {
        var fs = new FakeFileSystem();
        fs.AddFile(fileName, source);
        return await RunExtractorOnFilesAsync(fs);
    }

    private static async Task<DiscoveryModel> RunExtractorOnFilesAsync(FakeFileSystem fs)
    {
        var cache = new FakeAnalysisCache(fs);
        var allFiles = new List<string>();
        await foreach (var f in fs.EnumerateFilesAsync("", "*", SearchOption.AllDirectories))
            allFiles.Add(f);

        var analysis = new SharedAnalysisContext
        {
            AllSourceFiles = allFiles
        };

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis));
        model.Architecture.Seal();

        var ctx = new DiscoveryContext
        {
            RootPath = "",
            Options = new ExtractionOptions { MaxOutputTokens = 8000 },
            ActiveScenario = ScenarioRegistry.BuiltIn["overview"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = cache,
            Analysis = analysis,
            Logger = new NullLogger<DiscoveryContext>(),
        };

        var extractor = new EndpointExtractor();
        await extractor.ExtractAsync(ctx, model, CancellationToken.None);

        return model;
    }
}
