## DevContext -- Architecture Overview on Clean.Architecture.Core

**Architecture**: MinimalApi (80% confidence)
**Signals**: minimal-apis · fluentvalidation
**Projects**: 22 -- Clean.Architecture.AspireHost, Clean.Architecture.Core, Clean.Architecture.Infrastructure, Clean.Architecture.ServiceDefaults, Clean.Architecture.UseCases, Clean.Architecture.Web, Clean.Architecture.AspireTests, Clean.Architecture.FunctionalTests, Clean.Architecture.IntegrationTests, Clean.Architecture.UnitTests, MinimalClean.Architecture.AspireHost, MinimalClean.Architecture.ServiceDefaults, MinimalClean.Architecture.Web, NimblePros.SampleToDo.AspireHost, NimblePros.SampleToDo.Core, NimblePros.SampleToDo.Infrastructure, NimblePros.SampleToDo.ServiceDefaults, NimblePros.SampleToDo.UseCases, NimblePros.SampleToDo.Web, NimblePros.SampleToDo.FunctionalTests, NimblePros.SampleToDo.IntegrationTests, NimblePros.SampleToDo.UnitTests
**Profile**: focused | **Tokens**: ~8000 (budget 8000) | **Types**: 97 in output

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
| 1 | UseAppMiddlewareAndSeedDatabase | UseX |
| 1 | UseRequestLocalization | UseX |
| 1 | UseAppMiddlewareAndSeedDatabase | UseX |
| 2 | UseAppMiddleware | UseX |

### DI registrations

| Lifetime | Service | Implementation |
|----------|---------|----------------|
| Extension | AddFastEndpoints | ? |
| Extension | AddServiceConfigs | startupLogger |
| Extension | AddOptionConfigs | builder.Configuration |
| Extension | AddHealthChecks | ? |
| Extension | AddCheck | "self" |
| Extension | AddOpenTelemetry | ? |
| Extension | AddOpenTelemetry | ? |
| Extension | AddServiceDiscovery | ? |
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

- **Api**: ContributorList
- **Application**: CreateContributorHandlerHandle, UpdateProjectHandler, GetContributorHandler, ListContributorsHandler, IListProjectsShallowQueryService, CreateContributorHandler, DeleteProjectHandler, AddToDoItemCommand, UpdateContributorHandlerHandle, IListIncompleteItemsQueryService
- **Infrastructure**: UseDbGeneratedIds, EventDispatchInterceptor, EfRepositoryDelete, FakeListIncompleteItemsQueryService, ContributorConfiguration, AppDbContextModelSnapshot, BaseEfRepoTestFixture, MimeKitEmailSender, ListContributorsQueryService, ListProjectsShallowQueryService, EfRepositoryUpdate, ToDoItemConfiguration, AppDbContext, InfrastructureServiceExtensions, EfRepositoryAdd
- **Presentation**: OrderConfiguration, MailserverConfiguration, GetCartHandler, AddToCartRequest, GetByIdEndpoint, MiddlewareConfig, CheckoutEndpoint, VogenIntIdValueGenerator, IEmailSender, CheckoutMapper, MarkItemComplete, UpdateContributorMapper, Update, CartItemId, ListContributorsMapper, List, MiddlewareConfig, GetProductHandler, Quantity, DatabaseOptions, GetById, ListContributorsRequest, CartItemConfiguration, CreateContributorResponse, GetContributorByIdMapper, ListIncompleteItemsRequest, DeleteContributorRequest, CreateContributorRequest, List, Cart, AddToCartEndpoint, GuestUserConfiguration, Price, CachingProfile, GetProductByIdMapper, GlobalExceptionHandler
- **Unknown**: PhoneNumber, ToDoItemCompletedEvent, ContributorName, ContributorCreate, ToDoItemSearchService_GetNextIncompleteItem, ToDoItemBuilder, ContributorUpdate, NewItemAddedLoggingHandler, Contributor, ContributorIdFrom, NoOpMediator, ProjectErrorMessages, CustomWebApplicationFactory, ContributorUpdateName, ContributorNameUpdatedEvent, ContributorNameUpdatedEventLoggingHandler, ItemCompletedEmailNotificationHandler, ContributorAddedToItemLoggingHandler, ToDoItemId, LocalizationContext, ContributorConstructor, ToDoItemSearchServiceTests, Project, DeleteContributorService, NewItemAddedEvent, ToDoItemConstructor, ContributorDeletedEvent, ContributorDeletedHandler, IToDoItemSearchService, Project_AddItem, IncompleteItemsSpecificationConstructor, ContributorDelete, SmtpServerFixture, ContributorAddedToItemEvent, ProjectList

---
*Generated in 21.0ms | 381 types (97 active, 284 pruned) | Compressed: 1 strategies | Schema v2.0*
