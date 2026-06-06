## DevContext -- Architecture Overview on Clean.Architecture.Core

**Architecture**: MinimalApi (80% confidence)
**Signals**: minimal-apis · fluentvalidation · fast-endpoints
**Projects**: 22 -- Clean.Architecture.AspireHost, Clean.Architecture.Core, Clean.Architecture.Infrastructure, Clean.Architecture.ServiceDefaults, Clean.Architecture.UseCases, Clean.Architecture.Web, Clean.Architecture.AspireTests, Clean.Architecture.FunctionalTests, Clean.Architecture.IntegrationTests, Clean.Architecture.UnitTests, MinimalClean.Architecture.AspireHost, MinimalClean.Architecture.ServiceDefaults, MinimalClean.Architecture.Web, NimblePros.SampleToDo.AspireHost, NimblePros.SampleToDo.Core, NimblePros.SampleToDo.Infrastructure, NimblePros.SampleToDo.ServiceDefaults, NimblePros.SampleToDo.UseCases, NimblePros.SampleToDo.Web, NimblePros.SampleToDo.FunctionalTests, NimblePros.SampleToDo.IntegrationTests, NimblePros.SampleToDo.UnitTests
**Profile**: focused | **Tokens**: ~20000 (budget 20000) | **Types**: 140 in output

---
## Architecture overview

- Clean.Architecture.AspireHost
- Clean.Architecture.Core
- Clean.Architecture.Infrastructure
- Clean.Architecture.ServiceDefaults
- Clean.Architecture.UseCases
- Clean.Architecture.Web
- Clean.Architecture.AspireTests
- Clean.Architecture.FunctionalTests
- Clean.Architecture.IntegrationTests
- Clean.Architecture.UnitTests
- MinimalClean.Architecture.AspireHost
- MinimalClean.Architecture.ServiceDefaults
- MinimalClean.Architecture.Web
- NimblePros.SampleToDo.AspireHost
- NimblePros.SampleToDo.Core
- NimblePros.SampleToDo.Infrastructure
- NimblePros.SampleToDo.ServiceDefaults
- NimblePros.SampleToDo.UseCases
- NimblePros.SampleToDo.Web
- NimblePros.SampleToDo.FunctionalTests
- NimblePros.SampleToDo.IntegrationTests
- NimblePros.SampleToDo.UnitTests

## Endpoints

| Method | Route | Handler | Auth | Source |
|--------|-------|---------|------|--------|
| GET | /Products | ListEndpoint.HandleAsync | - | ListEndpoint.cs:31 |
| GET | <dynamic> | GetByIdEndpoint.HandleAsync | - | GetByIdEndpoint.cs:24 |
| POST | /Products | CreateEndpoint.HandleAsync | - | CreateEndpoint.cs:23 |
| GET | <dynamic> | GetByIdEndpoint.HandleAsync | - | GetByIdEndpoint.cs:24 |
| POST | <dynamic> | CheckoutEndpoint.HandleAsync | - | CheckoutEndpoint.cs:28 |
| PUT | <dynamic> | Update.HandleAsync | - | Update.cs:18 |
| POST | <dynamic> | MarkItemComplete.HandleAsync | - | MarkItemComplete.cs:15 |
| GET | <dynamic> | ListIncompleteItems.HandleAsync | - | ListIncompleteItems.cs:19 |
| GET | <dynamic> | List.HandleAsync | - | List.cs:14 |
| GET | <dynamic> | GetById.HandleAsync | - | GetById.cs:16 |
| DELETE | <dynamic> | Delete.HandleAsync | - | Delete.cs:18 |
| POST | <dynamic> | Create.HandleAsync | - | CreateToDoItem.cs:19 |
| POST | <dynamic> | Create.HandleAsync | - | Create.cs:14 |
| PUT | <dynamic> | Update.HandleAsync | - | Update.cs:18 |
| GET | /Contributors | List.HandleAsync | - | List.cs:13 |
| GET | <dynamic> | GetById.HandleAsync | - | GetById.cs:16 |
| DELETE | <dynamic> | Delete.HandleAsync | - | Delete.cs:18 |
| POST | <dynamic> | Create.HandleAsync | - | Create.cs:24 |
| PUT | <dynamic> | Update.HandleAsync | - | Update.cs:20 |
| GET | /Contributors | List.HandleAsync | - | List.cs:14 |
| GET | <dynamic> | GetById.HandleAsync | - | GetById.cs:18 |
| DELETE | <dynamic> | Delete.HandleAsync | - | Delete.cs:19 |
| POST | <dynamic> | Create.HandleAsync | - | Create.cs:25 |

## Non-obvious wiring

### Middleware pipeline

| Order | Type | Kind |
|-------|------|------|
| 1 | UseRequestLocalization | UseX |
| 1 | UseAppMiddlewareAndSeedDatabase | UseX |
| 1 | UseAppMiddlewareAndSeedDatabase | UseX |
| 2 | UseAppMiddleware | UseX |

### DI registrations

| Lifetime | Service | Implementation |
|----------|---------|----------------|
| Singleton | sp =>
    {
      var factory = sp.GetRequiredService<IStringLocalizerFactory>();
      // Creates a localizer for 'ProjectAggregate' – loads from i18n/{culture}/Project.json
      return factory.Create(typeof(ProjectErrorMessages));
    } | sp =>
    {
      var factory = sp.GetRequiredService<IStringLocalizerFactory>();
      // Creates a localizer for 'ProjectAggregate' – loads from i18n/{culture}/Project.json
      return factory.Create(typeof(ProjectErrorMessages));
    } |
| Extension | AddJsonLocalization | options =>
    {
      options.ResourcesPath = "i18n";  // Path to the JSON files (e.g., i18n/en/Project.json)
      options.CacheDuration = TimeSpan.FromHours(1);  // Optional: Cache for performance
      options.FileEncoding = Encoding.UTF8; //Optional: Specify file encoding
    } |
| Extension | AddLocalization | options => options.ResourcesPath = "i18n" |
| Extension | AddMetronome | ? |
| Extension | AddMemoryCache | ? |
| Extension | AddDbContext | options =>
                options.UseSqlite(connectionString) |
| Extension | AddValidatorsFromAssemblyContaining | ? |
| Extension | AddFastEndpoints | ? |
| Extension | AddServiceConfigs | appLogger |
| Extension | AddOptionConfigs | builder.Configuration |
| Extension | AddHealthChecks | ? |
| Extension | AddCheck | "self" |
| Extension | AddOpenTelemetry | ? |
| Extension | AddOpenTelemetry | ? |
| Extension | AddServiceDiscovery | ? |
| Extension | AddFastEndpoints | ? |
| Extension | AddServiceConfigs | startupLogger |
| Extension | AddOptionConfigs | builder.Configuration |
| Extension | AddFastEndpoints | ? |
| Extension | AddServiceConfigs | startupLogger |
| Extension | AddOptionConfigs | builder.Configuration |
| Extension | AddHealthChecks | ? |
| Extension | AddCheck | "self" |
| Extension | AddOpenTelemetry | ? |
| Extension | AddOpenTelemetry | ? |
| Extension | AddServiceDiscovery | ? |
| Extension | AddHealthChecks | ? |
| Extension | AddCheck | "self" |
| Extension | AddOpenTelemetry | ? |
| Extension | AddOpenTelemetry | ? |
| Extension | AddServiceDiscovery | ? |

## Related types grouped by layer

- **Application**: UpdateProjectHandler, GetContributorHandler, ListContributorsHandler, IListProjectsShallowQueryService, CreateContributorHandler, DeleteProjectHandler, IListIncompleteItemsQueryService, UpdateContributorHandler, IListContributorsQueryService, CreateContributorHandler, AddToDoItemHandler, CreateProjectHandler, ListProjectsShallowHandler, MarkToDoItemCompleteHandler, DeleteContributorHandler, GetContributorQuery, ListIncompleteItemsByProjectHandler, ICacheable, IListContributorsQueryService, GetProjectWithAllItemsHandler
- **Infrastructure**: EventDispatchInterceptor, FakeListIncompleteItemsQueryService, ContributorConfiguration, AppDbContextModelSnapshot, ToDoItemConfiguration, AppDbContext, InfrastructureServiceExtensions, FakeListProjectsShallowQueryService, FakeEmailSender, FakeListContributorsQueryService, ProjectConfiguration, FakeListContributorsQueryService, AppDbContext
- **Presentation**: OrderConfiguration, MailserverConfiguration, GetCartHandler, AddToCartRequest, GetByIdEndpoint, MiddlewareConfig, CheckoutEndpoint, VogenIntIdValueGenerator, IEmailSender, CheckoutMapper, MarkItemComplete, UpdateContributorMapper, Update, ListContributorsMapper, List, MiddlewareConfig, GetProductHandler, DatabaseOptions, GetById, ListContributorsRequest, CartItemConfiguration, CreateContributorResponse, GetContributorByIdMapper, DeleteContributorRequest, CreateContributorRequest, List, Cart, AddToCartEndpoint, GuestUserConfiguration, Price, CachingProfile, GetProductByIdMapper, GlobalExceptionHandler, AddToCartMapper, ListProjectsMapper, Create, Product, Create, CachingOptions, GuestUser, DeleteProjectRequest, CartConfiguration, AddToCartCommand, IListProductsQueryService, Delete, UpdateProjectMapper, LoggingBehavior, ListProductsMapper, AppDbContextFactory, ListEndpoint, Update, SeedData, OrderItemConfiguration, SeedData, Delete, UpdateContributorResponse, ListIncompleteItems, AddToCartHandler, CreateEndpoint, MiddlewareConfig, ResultExtensions, Order, VogenGuidIdValueGenerator, GetByIdEndpoint, OrderItem, MarkItemCompleteRequest, ProductId, GetById, CachingBehavior, CreateToDoItemRequest, ListProductsHandler, CartId, CheckoutRequest, Create, ProductConfiguration, ListProductsQueryService, CheckoutHandler, AppDbContext, AddGuestUsersAndOrdersSqlServer
- **Unknown**: PhoneNumber, ToDoItemCompletedEvent, ContributorName, NewItemAddedLoggingHandler, Contributor, ProjectErrorMessages, ContributorNameUpdatedEvent, ItemCompletedEmailNotificationHandler, ContributorAddedToItemLoggingHandler, LocalizationContext, Project, DeleteContributorService, NewItemAddedEvent, ContributorDeletedEvent, ContributorDeletedHandler, IToDoItemSearchService, ContributorAddedToItemEvent, Contributor, ContributorNameUpdatedEmailNotificationHandler, Extensions, DeleteContributorService, ProjectId, ToDoItemSearchService, IDeleteContributorService, ContributorId, ToDoItem, ILocalizationContext, IDeleteContributorService

---
*Generated in 18.4ms | 381 types (140 active, 241 pruned) | Compression: TrivialMemberCompressor(−1%) · BoilerplateCompressor(−5%) · StructuralDeduplicator(−42%) | Schema v2.0*
