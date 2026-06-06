## DevContext -- Architecture Overview on Clean.Architecture.Core

**Architecture**: MinimalApi (80% confidence)
**Signals**: minimal-apis · fluentvalidation · fast-endpoints
**Projects**: 22 -- Clean.Architecture.AspireHost, Clean.Architecture.Core, Clean.Architecture.Infrastructure, Clean.Architecture.ServiceDefaults, Clean.Architecture.UseCases, Clean.Architecture.Web, Clean.Architecture.AspireTests, Clean.Architecture.FunctionalTests, Clean.Architecture.IntegrationTests, Clean.Architecture.UnitTests, MinimalClean.Architecture.AspireHost, MinimalClean.Architecture.ServiceDefaults, MinimalClean.Architecture.Web, NimblePros.SampleToDo.AspireHost, NimblePros.SampleToDo.Core, NimblePros.SampleToDo.Infrastructure, NimblePros.SampleToDo.ServiceDefaults, NimblePros.SampleToDo.UseCases, NimblePros.SampleToDo.Web, NimblePros.SampleToDo.FunctionalTests, NimblePros.SampleToDo.IntegrationTests, NimblePros.SampleToDo.UnitTests
**Profile**: focused | **Tokens**: ~15000 (budget 15000) | **Types**: 134 in output

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

## Related types grouped by layer

- **Application**: UpdateProjectHandler, GetContributorHandler, ListContributorsHandler, IListProjectsShallowQueryService, CreateContributorHandler, DeleteProjectHandler, IListIncompleteItemsQueryService, UpdateContributorHandler, IListContributorsQueryService, CreateContributorHandler, AddToDoItemHandler, CreateProjectHandler, ListProjectsShallowHandler, MarkToDoItemCompleteHandler, DeleteContributorHandler, GetContributorQuery, ListIncompleteItemsByProjectHandler, ICacheable, IListContributorsQueryService, GetProjectWithAllItemsHandler
- **Infrastructure**: EventDispatchInterceptor, FakeListIncompleteItemsQueryService, ContributorConfiguration, AppDbContextModelSnapshot, ToDoItemConfiguration, AppDbContext, InfrastructureServiceExtensions, FakeListProjectsShallowQueryService, FakeEmailSender, FakeListContributorsQueryService, ProjectConfiguration, FakeListContributorsQueryService
- **Presentation**: OrderConfiguration, MailserverConfiguration, GetCartHandler, AddToCartRequest, GetByIdEndpoint, MiddlewareConfig, CheckoutEndpoint, VogenIntIdValueGenerator, IEmailSender, CheckoutMapper, MarkItemComplete, UpdateContributorMapper, Update, ListContributorsMapper, List, MiddlewareConfig, GetProductHandler, DatabaseOptions, GetById, ListContributorsRequest, CartItemConfiguration, CreateContributorResponse, GetContributorByIdMapper, DeleteContributorRequest, CreateContributorRequest, List, Cart, AddToCartEndpoint, GuestUserConfiguration, Price, CachingProfile, GetProductByIdMapper, GlobalExceptionHandler, AddToCartMapper, ListProjectsMapper, Create, Product, Create, CachingOptions, GuestUser, DeleteProjectRequest, CartConfiguration, AddToCartCommand, IListProductsQueryService, Delete, UpdateProjectMapper, LoggingBehavior, ListProductsMapper, AppDbContextFactory, ListEndpoint, Update, SeedData, OrderItemConfiguration, SeedData, Delete, UpdateContributorResponse, ListIncompleteItems, AddToCartHandler, CreateEndpoint, MiddlewareConfig, ResultExtensions, Order, VogenGuidIdValueGenerator, GetByIdEndpoint, OrderItem, MarkItemCompleteRequest, ProductId, GetById, CachingBehavior, CreateToDoItemRequest, ListProductsHandler, CartId, CheckoutRequest, Create, Initial, ProductConfiguration
- **Unknown**: PhoneNumber, ToDoItemCompletedEvent, ContributorName, NewItemAddedLoggingHandler, Contributor, ProjectErrorMessages, ContributorNameUpdatedEvent, ItemCompletedEmailNotificationHandler, ContributorAddedToItemLoggingHandler, LocalizationContext, Project, DeleteContributorService, NewItemAddedEvent, ContributorDeletedEvent, ContributorDeletedHandler, IToDoItemSearchService, ContributorAddedToItemEvent, Contributor, ContributorNameUpdatedEmailNotificationHandler, Extensions, DeleteContributorService, ProjectId, ToDoItemSearchService, IDeleteContributorService, ContributorId, ILocalizationContext

## Diagnostics

| Level | Source | Message |
|-------|--------|---------|
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Clean.Architecture.Infrastructure.Data.Migrations.UseDbGeneratedIds |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: MinimalClean.Architecture.Web.Infrastructure.Data.Migrations.AddGuestUsersAndOrdersSqlServer |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Clean.Architecture.Infrastructure.Data.Migrations.UpdateForNet10 |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: MinimalClean.Architecture.Web.Infrastructure.Data.Migrations.Initial |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Clean.Architecture.UnitTests.Core.Services.ToDoItemSearchService_GetAllIncompleteItems |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Clean.Architecture.Infrastructure.Data.Migrations.PhoneNumber |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Microsoft.Extensions.Hosting.Extensions |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Clean.Architecture.UnitTests.Core.Services.ToDoItemSearchService_GetNextIncompleteItem |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: global.Program |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: global.Program |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: global.Program |
| Info | CallReachabilityPruner | CallGraph not available; skipping reachability analysis. |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Microsoft.Extensions.Hosting.Extensions |

### Pruning notes

- PatternRelevancePruner: pruned test type 'Clean.Architecture.UnitTests.UseCases.Contributors.CreateContributorHandlerHandle'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.AspireTests.Tests.AspireIntegrationTests'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.FunctionalTests.Contributors.ContributorCreate'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.UnitTests.Core.Services.ToDoItemSearchService_GetNextIncompleteItem'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.ToDoItemBuilder'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.FunctionalTests.ApiEndpoints.ContributorList'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.IntegrationTests.Data.EfRepositoryDelete'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.FunctionalTests.Contributors.ContributorUpdate'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.Core.ContributorAggregate.ContributorIdFrom'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.UnitTests.NoOpMediator'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.FunctionalTests.CustomWebApplicationFactory'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.Core.ContributorAggregate.ContributorUpdateName'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.IntegrationTests.Data.BaseEfRepoTestFixture'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.Core.ContributorAggregate.ContributorConstructor'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.Core.Services.ToDoItemSearchServiceTests'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.UseCases.Contributors.UpdateContributorHandlerHandle'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.Core.ProjectAggregate.ToDoItemConstructor'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.IntegrationTests.Data.EfRepositoryUpdate'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.Core.ProjectAggregate.Project_AddItem'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.Core.Specifications.IncompleteItemsSpecificationConstructor'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.FunctionalTests.Contributors.ContributorDelete'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.FunctionalTests.ClassFixtures.SmtpServerFixture'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.FunctionalTests.Projects.ProjectList'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.IntegrationTests.Data.EfRepositoryAdd'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.Core.ContributorAggregate.Specifications.ContributorByIdSpec'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.FunctionalTests.Projects.ProjectItemMarkComplete'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.FunctionalTests.ApiEndpoints.ContributorGetById'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.FunctionalTests.Projects.ProjectGetById'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.FunctionalTests.Projects.ProjectAddToDoItem'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.Core.ContributorAggregate.Specifications.ContributorByIdSpec'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.IntegrationTests.Data.EfRepositoryUpdate'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.UnitTests.Core.Services.DeleteContributorService_DeleteContributor'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.FunctionalTests.Contributors.ContributorList'
- PatternRelevancePruner: pruned test type 'MinimalClean.Architecture.Web.Domain.ProductAggregate.Specifications.ProductByIdSpec'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.IntegrationTests.Data.BaseEfRepoTestFixture'
- PatternRelevancePruner: pruned test type 'MinimalClean.Architecture.Web.Domain.CartAggregate.Specifications.CartByIdSpec'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.Core.ProjectAggregate.ProjectConstructor'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.Core.ProjectAggregate.Specifications.IncompleteItemsSearchSpec'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.Core.ProjectAggregate.ProjectNameFrom'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.UseCases.Contributors.GetContributorHandlerHandle'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.FunctionalTests.Projects.CreateToDoItemRequestBuilder'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.UnitTests.Core.ContributorAggregate.ContributorConstructor'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.FunctionalTests.DockerAvailabilityTests'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.Core.Handlers.ItemCompletedEmailNotificationHandlerHandle'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.FunctionalTests.Projects.ProjectCreate'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.Core.Services.DeleteContributorService_DeleteContributor'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.UnitTests.Core.ContributorAggregate.ContributorUpdateName'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.UseCases.Contributors.CreateContributorHandlerHandle'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.Core.ProjectAggregate.Specifications.ProjectsWithItemsByContributorIdSpec'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.Core.ProjectAggregate.ToDoItemMarkComplete'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.FunctionalTests.TestBase'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.Core.ProjectAggregate.Specifications.ProjectByIdWithItemsSpec'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.UnitTests.Core.Services.ToDoItemSearchService_GetAllIncompleteItems'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.UnitTests.Core.ContributorAggregate.ContributorIdFrom'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.UnitTests.Core.ContributorAggregate.ContributorNameFrom'
- PatternRelevancePruner: pruned test type 'MinimalClean.Architecture.Web.Domain.GuestUserAggregate.Specifications.GuestUserByIdSpec'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.IntegrationTests.Data.EfRepositoryDelete'
- PatternRelevancePruner: pruned test type 'MinimalClean.Architecture.Web.Domain.GuestUserAggregate.Specifications.GuestUserByEmailSpec'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.UnitTests.NoOpMediator'
- PatternRelevancePruner: pruned test type 'Clean.Architecture.IntegrationTests.Data.EfRepositoryAdd'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.FunctionalTests.CustomWebApplicationFactory'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.FunctionalTests.Contributors.ContributorGetById'
- PatternRelevancePruner: pruned test type 'NimblePros.SampleToDo.Core.ProjectAggregate.Specifications.IncompleteItemsSpec'
- TokenBudgetEnforcer: kept 301 types (17 pruned for budget 14500)

---
*Generated in 29.4ms | 381 types (134 active, 247 pruned) | Compression: TrivialMemberCompressor(−1%) · BoilerplateCompressor(−5%) · StructuralDeduplicator(−41%) | Schema v2.0*
