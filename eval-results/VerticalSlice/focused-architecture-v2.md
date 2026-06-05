## DevContext -- Architecture Overview on Clean.Architecture.Core

**Architecture**: MinimalApi (80% confidence)
**Signals**: minimal-apis · fluentvalidation
**Projects**: 22 -- Clean.Architecture.AspireHost, Clean.Architecture.Core, Clean.Architecture.Infrastructure, Clean.Architecture.ServiceDefaults, Clean.Architecture.UseCases, Clean.Architecture.Web, Clean.Architecture.AspireTests, Clean.Architecture.FunctionalTests, Clean.Architecture.IntegrationTests, Clean.Architecture.UnitTests, MinimalClean.Architecture.AspireHost, MinimalClean.Architecture.ServiceDefaults, MinimalClean.Architecture.Web, NimblePros.SampleToDo.AspireHost, NimblePros.SampleToDo.Core, NimblePros.SampleToDo.Infrastructure, NimblePros.SampleToDo.ServiceDefaults, NimblePros.SampleToDo.UseCases, NimblePros.SampleToDo.Web, NimblePros.SampleToDo.FunctionalTests, NimblePros.SampleToDo.IntegrationTests, NimblePros.SampleToDo.UnitTests
**Profile**: focused | **Tokens**: ~20000 (budget 20000) | **Types**: 183 in output

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

No endpoints detected.

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
| Extension | AddHealthChecks | ? |
| Extension | AddCheck | "self" |
| Extension | AddOpenTelemetry | ? |
| Extension | AddOpenTelemetry | ? |
| Extension | AddServiceDiscovery | ? |
| Extension | AddFastEndpoints | ? |
| Extension | AddServiceConfigs | startupLogger |
| Extension | AddOptionConfigs | builder.Configuration |
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
| Extension | AddHealthChecks | ? |
| Extension | AddCheck | "self" |
| Extension | AddOpenTelemetry | ? |
| Extension | AddOpenTelemetry | ? |
| Extension | AddServiceDiscovery | ? |

## Related types grouped by layer

- **Api**: ContributorList, ContributorGetById
- **Application**: CreateContributorHandlerHandle, UpdateProjectHandler, GetContributorHandler, ListContributorsHandler, IListProjectsShallowQueryService, CreateContributorHandler, DeleteProjectHandler, UpdateContributorHandlerHandle, IListIncompleteItemsQueryService, UpdateContributorHandler, IListContributorsQueryService, CreateContributorHandler, AddToDoItemHandler, CreateProjectHandler, ListProjectsShallowHandler, MarkToDoItemCompleteHandler, DeleteContributorHandler, GetContributorQuery, ListIncompleteItemsByProjectHandler, GetContributorHandlerHandle, ICacheable, IListContributorsQueryService, GetProjectWithAllItemsHandler
- **Infrastructure**: EventDispatchInterceptor, EfRepositoryDelete, FakeListIncompleteItemsQueryService, ContributorConfiguration, AppDbContextModelSnapshot, BaseEfRepoTestFixture, EfRepositoryUpdate, ToDoItemConfiguration, AppDbContext, InfrastructureServiceExtensions, EfRepositoryAdd, FakeListProjectsShallowQueryService, FakeEmailSender, FakeListContributorsQueryService, BaseEfRepoTestFixture, ProjectConfiguration, FakeListContributorsQueryService, AppDbContext, EfRepositoryAdd
- **Presentation**: OrderConfiguration, MailserverConfiguration, GetCartHandler, AddToCartRequest, GetByIdEndpoint, MiddlewareConfig, CheckoutEndpoint, VogenIntIdValueGenerator, IEmailSender, CheckoutMapper, MarkItemComplete, UpdateContributorMapper, Update, ListContributorsMapper, List, MiddlewareConfig, GetProductHandler, DatabaseOptions, GetById, ListContributorsRequest, CartItemConfiguration, CreateContributorResponse, GetContributorByIdMapper, DeleteContributorRequest, CreateContributorRequest, List, Cart, AddToCartEndpoint, GuestUserConfiguration, Price, CachingProfile, GetProductByIdMapper, GlobalExceptionHandler, AddToCartMapper, ListProjectsMapper, Create, Product, Create, CachingOptions, GuestUser, DeleteProjectRequest, CartConfiguration, AddToCartCommand, IListProductsQueryService, Delete, UpdateProjectMapper, LoggingBehavior, ListProductsMapper, AppDbContextFactory, ListEndpoint, Update, SeedData, OrderItemConfiguration, SeedData, Delete, UpdateContributorResponse, ListIncompleteItems, AddToCartHandler, CreateEndpoint, MiddlewareConfig, ResultExtensions, Order, VogenGuidIdValueGenerator, GetByIdEndpoint, OrderItem, MarkItemCompleteRequest, ProductId, GetById, CachingBehavior, CreateToDoItemRequest, ListProductsHandler, CartId, CheckoutRequest, Create, ProductConfiguration, ListProductsQueryService, CheckoutHandler, AppDbContext, AddGuestUsersAndOrdersSqlServer
- **Unknown**: PhoneNumber, ToDoItemCompletedEvent, ContributorName, ContributorCreate, ToDoItemSearchService_GetNextIncompleteItem, ToDoItemBuilder, ContributorUpdate, NewItemAddedLoggingHandler, Contributor, ContributorIdFrom, NoOpMediator, ProjectErrorMessages, CustomWebApplicationFactory, ContributorUpdateName, ContributorNameUpdatedEvent, ItemCompletedEmailNotificationHandler, ContributorAddedToItemLoggingHandler, LocalizationContext, ContributorConstructor, ToDoItemSearchServiceTests, Project, DeleteContributorService, NewItemAddedEvent, ToDoItemConstructor, ContributorDeletedEvent, ContributorDeletedHandler, IToDoItemSearchService, Project_AddItem, IncompleteItemsSpecificationConstructor, ContributorDelete, SmtpServerFixture, ContributorAddedToItemEvent, ProjectItemMarkComplete, Contributor, ProjectGetById, ProjectAddToDoItem, DeleteContributorService_DeleteContributor, ContributorNameUpdatedEmailNotificationHandler, Extensions, ProjectConstructor, ProjectNameFrom, CreateToDoItemRequestBuilder, DockerAvailabilityTests, ItemCompletedEmailNotificationHandlerHandle, DeleteContributorService, ProjectCreate, ContributorUpdateName, ProjectId, ToDoItemMarkComplete, ToDoItemSearchService, TestBase, ToDoItemSearchService_GetAllIncompleteItems, IDeleteContributorService, ContributorNameFrom, ContributorId, ToDoItem, CustomWebApplicationFactory, ILocalizationContext, ContributorGetById, IDeleteContributorService

---
*Generated in 24.3ms | 381 types (183 active, 198 pruned) | Schema v2.0*
