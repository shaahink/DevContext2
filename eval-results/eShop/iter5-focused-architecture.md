## DevContext -- Architecture Overview on ClientApp

**Architecture**: MinimalApi (100% confidence)
**Signals**: controllers · minimal-apis · mediatr · fluentvalidation
**Projects**: 24 -- Basket.API, Catalog.API, ClientApp, eShop.AppHost, eShop.ServiceDefaults, EventBus, EventBusRabbitMQ, HybridApp, Identity.API, IntegrationEventLogEF, Ordering.API, Ordering.Domain, Ordering.Infrastructure, OrderProcessor, PaymentProcessor, WebApp, WebAppComponents, WebhookClient, Webhooks.API, Basket.UnitTests, Catalog.FunctionalTests, ClientApp.UnitTests, Ordering.FunctionalTests, Ordering.UnitTests
**Profile**: focused | **Tokens**: ~20000 (budget 20000) | **Types**: 212 in output

---
## Architecture overview

- Basket.API
- Catalog.API
- ClientApp
- eShop.AppHost
- eShop.ServiceDefaults
- EventBus
- EventBusRabbitMQ
- HybridApp
- Identity.API
- IntegrationEventLogEF
- Ordering.API
- Ordering.Domain
- Ordering.Infrastructure
- OrderProcessor
- PaymentProcessor
- WebApp
- WebAppComponents
- WebhookClient
- Webhooks.API
- Basket.UnitTests
- Catalog.FunctionalTests
- ClientApp.UnitTests
- Ordering.FunctionalTests
- Ordering.UnitTests

## Endpoints

| Method | Route | Group | Handler | Auth | Source |
|--------|-------|-------|---------|------|--------|
| POST | / | - | GrantsController.Revoke | - | GrantsController.cs:41 |
| GET | / | - | GrantsController.Index | - | GrantsController.cs:32 |
| POST | / | - | DeviceController.Callback | - | DeviceController.cs:51 |
| POST | / | - | DeviceController.UserCodeCapture | - | DeviceController.cs:41 |
| GET | / | - | DeviceController.Index | - | DeviceController.cs:27 |
| POST | / | - | ConsentController.Index | - | ConsentController.cs:47 |
| GET | / | - | ConsentController.Index | - | ConsentController.cs:32 |
| GET | / | - | ExternalController.Callback | - | ExternalController.cs:63 |
| GET | / | - | ExternalController.Challenge | - | ExternalController.cs:33 |
| GET | / | - | AccountController.AccessDenied | - | AccountController.cs:196 |
| POST | / | - | AccountController.Logout | - | AccountController.cs:165 |
| GET | / | - | AccountController.Logout | - | AccountController.cs:146 |
| POST | / | - | AccountController.Login | - | AccountController.cs:59 |
| GET | / | - | AccountController.Login | - | AccountController.cs:39 |
| DELETE | /api/webhooks/{id:int} | /api/webhooks | λ WebHooksApi.cs:66 | - | WebHooksApi.cs:66 |
| POST | /api/webhooks/ | /api/webhooks | λ WebHooksApi.cs:35 | - | WebHooksApi.cs:35 |
| GET | /api/webhooks/{id:int} | /api/webhooks | λ WebHooksApi.cs:20 | - | WebHooksApi.cs:20 |
| GET | /api/webhooks/ | /api/webhooks | λ WebHooksApi.cs:13 | - | WebHooksApi.cs:13 |
| POST | /webhook-received | - | λ WebhookEndpoints.cs:31 | - | WebhookEndpoints.cs:31 |
| POST | /logout | - | λ AuthenticationEndpoints.cs:12 | - | AuthenticationEndpoints.cs:12 |
| POST | api/orders/ | api/orders | CreateOrderAsync.CreateOrderAsync | - | OrdersApi.cs:17 |
| POST | api/orders/draft | api/orders | CreateOrderDraftAsync.CreateOrderDraftAsync | - | OrdersApi.cs:16 |
| GET | api/orders/cardtypes | api/orders | GetCardTypesAsync.GetCardTypesAsync | - | OrdersApi.cs:15 |
| GET | api/orders/ | api/orders | GetOrdersByUserAsync.GetOrdersByUserAsync | - | OrdersApi.cs:14 |
| GET | api/orders/{orderId:int} | api/orders | GetOrderAsync.GetOrderAsync | - | OrdersApi.cs:13 |
| PUT | api/orders/ship | api/orders | ShipOrderAsync.ShipOrderAsync | - | OrdersApi.cs:12 |
| PUT | api/orders/cancel | api/orders | CancelOrderAsync.CancelOrderAsync | - | OrdersApi.cs:11 |
| DELETE | api/catalog/items/{id:int} | api/catalog | DeleteItemById.DeleteItemById | - | CatalogApi.cs:107 |
| POST | api/catalog/items | api/catalog | CreateItem.CreateItem | - | CatalogApi.cs:103 |
| PUT | api/catalog/items/{id:int} | api/catalog | UpdateItem.UpdateItem | - | CatalogApi.cs:98 |
| PUT | api/catalog/items | api/catalog | UpdateItemV1.UpdateItemV1 | - | CatalogApi.cs:93 |
| GET | api/catalog/catalogbrands | api/catalog | λ CatalogApi.cs:84 | - | CatalogApi.cs:84 |
| GET | api/catalog/catalogtypes | api/catalog | λ CatalogApi.cs:77 | - | CatalogApi.cs:77 |
| GET | api/catalog/items/type/all/brand/{brandId:int?} | api/catalog | GetItemsByBrandId.GetItemsByBrandId | - | CatalogApi.cs:72 |
| GET | api/catalog/items/type/{typeId}/brand/{brandId?} | api/catalog | GetItemsByBrandAndTypeId.GetItemsByBrandAndTypeId | - | CatalogApi.cs:67 |
| GET | api/catalog/items/withsemanticrelevance | api/catalog | GetItemsBySemanticRelevance.GetItemsBySemanticRelevance | - | CatalogApi.cs:60 |
| GET | api/catalog/items/withsemanticrelevance/{text:minlength(1)} | api/catalog | GetItemsBySemanticRelevanceV1.GetItemsBySemanticRelevanceV1 | - | CatalogApi.cs:53 |
| GET | api/catalog/items/{id:int}/pic | api/catalog | GetItemPictureById.GetItemPictureById | - | CatalogApi.cs:46 |
| GET | api/catalog/items/by/{name:minlength(1)} | api/catalog | GetItemsByName.GetItemsByName | - | CatalogApi.cs:41 |
| GET | api/catalog/items/{id:int} | api/catalog | GetItemById.GetItemById | - | CatalogApi.cs:36 |
| GET | api/catalog/items/by | api/catalog | GetItemsByIds.GetItemsByIds | - | CatalogApi.cs:31 |
| GET | api/catalog/items | api/catalog | GetAllItems.GetAllItems | - | CatalogApi.cs:26 |
| GET | api/catalog/items | api/catalog | GetAllItemsV1.GetAllItemsV1 | - | CatalogApi.cs:21 |
| GET | / | - | () => Results.Redirect($"/scalar/{def... | - | OpenApi.Extensions.cs:41 |

## MediatR Handlers

| Kind | Request | Response | Handler |
|------|---------|----------|---------|
| Notification | OrderStartedDomainEvent | Unit | ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler |
| Notification | BuyerAndPaymentMethodVerifiedDomainEvent | Unit | UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler |
| Notification | OrderStatusChangedToStockConfirmedDomainEvent | Unit | OrderStatusChangedToStockConfirmedDomainEventHandler |
| Notification | OrderStatusChangedToPaidDomainEvent | Unit | OrderStatusChangedToPaidDomainEventHandler |
| Notification | OrderStatusChangedToAwaitingValidationDomainEvent | Unit | OrderStatusChangedToAwaitingValidationDomainEventHandler |
| Notification | OrderShippedDomainEvent | Unit | OrderShippedDomainEventHandler |
| Notification | OrderCancelledDomainEvent | Unit | OrderCancelledDomainEventHandler |
| Command | ShipOrderCommand | bool | ShipOrderCommandHandler |
| Command | SetStockRejectedOrderStatusCommand | bool | SetStockRejectedOrderStatusCommandHandler |
| Command | SetStockConfirmedOrderStatusCommand | bool | SetStockConfirmedOrderStatusCommandHandler |
| Command | SetPaidOrderStatusCommand | bool | SetPaidOrderStatusCommandHandler |
| Command | SetAwaitingValidationOrderStatusCommand | bool | SetAwaitingValidationOrderStatusCommandHandler |
| Command | IdentifiedCommand<T, R> | R | IdentifiedCommandHandler |
| Command | R | Unit | IdentifiedCommand |
| Command | CreateOrderDraftCommand | OrderDraftDTO | CreateOrderDraftCommandHandler |
| Command | CreateOrderCommand | bool | CreateOrderCommandHandler |
| Command | bool | Unit | CreateOrderCommand |
| Command | CancelOrderCommand | bool | CancelOrderCommandHandler |

## Non-obvious wiring

### Middleware pipeline

| Order | Type | Kind |
|-------|------|------|
| 1 | UseExceptionHandler | UseX |
| 1 | UseDefaultOpenApi | UseX |
| 1 | UseDefaultOpenApi | UseX |
| 1 | UseExceptionHandler | UseX |
| 1 | UseStaticFiles | UseX |
| 1 | UseStatusCodePages | UseX |
| 1 | MapGrpcService | MapX |
| 2 | UseHsts | UseX |
| 2 | UseHsts | UseX |
| 2 | UseCookiePolicy | UseX |
| 2 | UseDefaultOpenApi | UseX |
| 3 | UseAntiforgery | UseX |
| 3 | UseAntiforgery | UseX |
| 3 | UseRouting | UseX |
| 4 | UseStaticFiles | UseX |
| 4 | UseHttpsRedirection | UseX |
| 4 | UseIdentityServer | UseX |
| 5 | UseStaticFiles | UseX |
| 5 | UseAuthorization | UseX |

### DI registrations

| Lifetime | Service | Implementation |
|----------|---------|----------------|
| Singleton | IBasketRepository | RedisBasketRepository |
| Extension | AddOptions | ? |
| Singleton | WebAppComponents.Services.IProductImageUrlProvider | ProductImageUrlProvider |
| Extension | AddHttpClient | o => o.BaseAddress = new(MobileBffHost) |
| Extension | AddMauiBlazorWebView | ? |
| Extension | AddHealthChecks | ? |
| Extension | AddCheck | "self" |
| Extension | AddOpenTelemetry | ? |
| Extension | AddServiceDiscovery | ? |
| Extension | AddHttpClient | o => o.BaseAddress = new("http://webhooks-api") |
| Extension | AddApiVersion | 1.0 |
| Extension | AddAuthToken | ? |
| Singleton | HooksRepository | HooksRepository |
| Extension | AddOptions | ? |
| Extension | AddHttpClient | o => o.BaseAddress = new("https+http://ordering-api") |
| Extension | AddApiVersion | 1.0 |
| Extension | AddAuthToken | ? |
| Extension | AddHttpClient | o => o.BaseAddress = new("https+http://catalog-api") |
| Extension | AddApiVersion | 2.0 |
| Extension | AddAuthToken | ? |
| Extension | AddGrpcClient | o => o.Address = new("http://basket-api") |
| Extension | AddAuthToken | ? |
| Singleton | IProductImageUrlProvider | ProductImageUrlProvider |
| Singleton | OrderStatusNotificationService | OrderStatusNotificationService |
| Singleton | BasketService | BasketService |
| Scoped | LogOutService | LogOutService |
| Scoped | BasketState | BasketState |
| Extension | AddHttpForwarderWithServiceDiscovery | ? |
| Extension | AddKeyedTransient | typeof(T) |
| Scoped | ICatalogAI | CatalogAI |
| Extension | AddOptions | ? |
| Transient | ICatalogIntegrationEventService | CatalogIntegrationEventService |
| Transient | IIntegrationEventLogService | IntegrationEventLogService<CatalogContext> |
| Extension | AddMigration | ? |
| Extension | AddDbContext | ? |
| Extension | AddRazorComponents | ? |
| Extension | AddInteractiveServerComponents | ? |
| Extension | AddApiVersioning | options =>
{
    // Include "api-supported-versions" and "api-deprecated-versions" headers in all responses
    options.ReportApiVersions = true;
} |
| Extension | AddProblemDetails | ? |
| Extension | AddRazorComponents | ? |
| Extension | AddInteractiveServerComponents | ? |
| Transient | SettingsView | SettingsView |
| Transient | ProfileView | ProfileView |
| Transient | MapView | MapView |
| Transient | OrderDetailView | OrderDetailView |
| Transient | LoginView | LoginView |
| Transient | FiltersView | FiltersView |
| Transient | CheckoutView | CheckoutView |
| Transient | CatalogView | CatalogView |
| Transient | BasketView | BasketView |
| Singleton | CatalogItemView | CatalogItemView |
| Transient | SettingsViewModel | SettingsViewModel |
| Transient | OrderDetailViewModel | OrderDetailViewModel |
| Transient | CheckoutViewModel | CheckoutViewModel |
| Singleton | ProfileViewModel | ProfileViewModel |
| Singleton | MapViewModel | MapViewModel |
| Singleton | CatalogItemViewModel | CatalogItemViewModel |
| Singleton | CatalogViewModel | CatalogViewModel |
| Singleton | BasketViewModel | BasketViewModel |
| Singleton | LoginViewModel | LoginViewModel |
| Singleton | MainViewModel | MainViewModel |
| Transient | IBrowser | MauiAuthenticationBrowser |
| Singleton | serviceProvider =>
            {
                var requestProvider = serviceProvider.GetRequiredService<IRequestProvider>();
                var fixUriService = serviceProvider.GetRequiredService<IFixUriService>();
                var settingsService = serviceProvider.GetRequiredService<ISettingsService>();
                var identityService = serviceProvider.GetRequiredService<IIdentityService>();

                var aes =
                    new AppEnvironmentService(
                        new BasketMockService(), new BasketService(identityService, settingsService, fixUriService),
                        new CatalogMockService(), new CatalogService(settingsService, requestProvider, fixUriService),
                        new OrderMockService(), new OrderService(identityService, settingsService, requestProvider),
                        new IdentityMockService(), identityService);

                aes.UpdateDependencies(settingsService.UseMocks);
                return aes;
            } | serviceProvider =>
            {
                var requestProvider = serviceProvider.GetRequiredService<IRequestProvider>();
                var fixUriService = serviceProvider.GetRequiredService<IFixUriService>();
                var settingsService = serviceProvider.GetRequiredService<ISettingsService>();
                var identityService = serviceProvider.GetRequiredService<IIdentityService>();

                var aes =
                    new AppEnvironmentService(
                        new BasketMockService(), new BasketService(identityService, settingsService, fixUriService),
                        new CatalogMockService(), new CatalogService(settingsService, requestProvider, fixUriService),
                        new OrderMockService(), new OrderService(identityService, settingsService, requestProvider),
                        new IdentityMockService(), identityService);

                aes.UpdateDependencies(settingsService.UseMocks);
                return aes;
            } |
| Singleton | ITheme | Theme |
| Singleton | ILocationService | LocationService |
| Singleton | IFixUriService | FixUriService |
| Singleton | sp =>
            {
                var browser = sp.GetRequiredService<IBrowser>();
                var settingsService = sp.GetRequiredService<ISettingsService>();
                var debugHttpHandler = sp.GetKeyedService<HttpMessageHandler>("DebugHttpMessageHandler");
                return new IdentityService(browser, settingsService, debugHttpHandler);
            } | sp =>
            {
                var browser = sp.GetRequiredService<IBrowser>();
                var settingsService = sp.GetRequiredService<ISettingsService>();
                var debugHttpHandler = sp.GetKeyedService<HttpMessageHandler>("DebugHttpMessageHandler");
                return new IdentityService(browser, settingsService, debugHttpHandler);
            } |
| Singleton | sp =>
            {
                var debugHttpHandler = sp.GetKeyedService<HttpMessageHandler>("DebugHttpMessageHandler");
                return new RequestProvider(debugHttpHandler);
            } | sp =>
            {
                var debugHttpHandler = sp.GetKeyedService<HttpMessageHandler>("DebugHttpMessageHandler");
                return new RequestProvider(debugHttpHandler);
            } |
| Singleton | IOpenUrlService | OpenUrlService |
| Singleton | IDialogService | DialogService |
| Singleton | INavigationService | MauiNavigationService |
| Singleton | ISettingsService | SettingsService |
| Extension | AddGrpc | ? |
| Transient | IWebhooksSender | WebhooksSender |
| Transient | IWebhooksRetriever | WebhooksRetriever |
| Transient | IGrantUrlTesterService | GrantUrlTesterService |
| Extension | AddMigration | ? |
| Extension | AddHostedService | ? |
| Extension | AddOptions | ? |
| Extension | AddApiVersioning | options =>
{
    // Include "api-supported-versions" and "api-deprecated-versions" headers in all responses
    options.ReportApiVersions = true;
} |
| Extension | AddApiVersioning | options =>
{
    // Include "api-supported-versions" and "api-deprecated-versions" headers in all responses
    options.ReportApiVersions = true;
} |
| Extension | AddProblemDetails | ? |
| Transient | IRedirectService | RedirectService |
| Transient | ILoginService<ApplicationUser> | EFLoginService |
| Transient | IProfileService | ProfileService |
| Extension | AddIdentityServer | options =>
{
    //options.IssuerUri = "null";
    options.Authentication.CookieLifetime = TimeSpan.FromHours(2);

    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    // TODO: Remove this line in production.
    options.KeyManagement.Enabled = false;
} |
| Extension | AddInMemoryIdentityResources | Config.GetResources() |
| Extension | AddInMemoryApiScopes | Config.GetApiScopes() |
| Extension | AddInMemoryApiResources | Config.GetApis() |
| Extension | AddInMemoryClients | Config.GetClients(builder.Configuration) |
| Extension | AddAspNetIdentity | ? |
| Extension | AddDeveloperSigningCredential | ? |
| Extension | AddIdentity | ? |
| Extension | AddEntityFrameworkStores | ? |
| Extension | AddDefaultTokenProviders | ? |
| Extension | AddMigration | ? |
| Extension | AddControllersWithViews | ? |
| Singleton | sp => (RabbitMQEventBus)sp.GetRequiredService<IEventBus>() | sp => (RabbitMQEventBus)sp.GetRequiredService<IEventBus>() |
| Singleton | IEventBus | RabbitMQEventBus |
| Singleton | RabbitMQTelemetry | RabbitMQTelemetry |
| Extension | AddOpenTelemetry | ? |
| Extension | AddHttpContextAccessor | ? |

## Related types grouped by layer

- **Api**: OrderStatusChangedToCancelledIntegrationEvent, CreateOrderCommand, OrderStockRejectedIntegrationEvent, SetStockRejectedOrderStatusCommandHandler, PaginatedItems, OrderStatusChangedToPaidIntegrationEvent, ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler, OrderStatusChangedToPaidIntegrationEventHandler, EFLoginService, GracePeriodConfirmedIntegrationEvent, SetStockConfirmedOrderStatusCommandHandler, IRedirectService, RedirectService, OrderStartedIntegrationEventHandler, ILoginService, OrderingApiTrace, IdentifiedCommandHandler, IBasketRepository, OrderPaymentSucceededIntegrationEventHandler, CatalogAI, CatalogItem, Config, OrderStatusChangedToStockConfirmedDomainEventHandler, UsersSeed, UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler, ServerCallContextIdentityExtensions, ProfileService, CreateOrderCommandHandler, OrderingContextSeed, OrderStatusChangedToPaidDomainEventHandler, OrderDraftDTO, CatalogTypeEntityTypeConfiguration, OrderingIntegrationEventService, OrderStatusChangedToAwaitingValidationDomainEventHandler, ICatalogIntegrationEventService, LoggingBehavior, BasketItem, OrderStockItem, IdentifiedCommand, OrderPaymentFailedIntegrationEventHandler, CreateOrderCommandValidator, LinqSelectExtensions, CatalogIntegrationEventService, CatalogItemEntityTypeConfiguration, IIdentityService, IOrderQueries, CatalogApi, CancelOrderIdentifiedCommandHandler, CatalogContext, OrderStockRejectedIntegrationEventHandler, Initial, RedisBasketRepository, BasketService, CreateOrderDraftCommandHandler, ApplicationDbContextModelSnapshot, OrderStockConfirmedIntegrationEventHandler, OrderCancelledDomainEventHandler, SetAwaitingValidationOrderStatusCommandHandler, ShipOrderCommandHandler, CancelOrderCommandHandler, SetPaidOrderStatusCommandHandler, OrderShippedDomainEventHandler
- **Domain**: ClientRequestEntityTypeConfiguration, OrderDetailViewModel, OrderShippedDomainEvent, BasketViewModel, IViewModelBase, IRepository, CustomerBasket, PaymentMethodEntityTypeConfiguration, ViewModelBase, AuthorizeRequest, Address, OrderStatusChangedToPaidDomainEvent, IAggregateRoot, LoginViewModel, PaymentMethod, MapViewModel, CancelOrderCommand, OrderEntityTypeConfiguration, IBuyerRepository, OrderItem, MainViewModel, IUnitOfWork, SettingsViewModel, OrderItemEntityTypeConfiguration, BuyerEntityTypeConfiguration, Buyer, CatalogViewModel, BasketItem, IdentityService, CheckoutViewModel, CatalogItemViewModel, ProfileViewModel
- **Infrastructure**: BuyerRepository, RequestManager, OrderRepository, MediatorExtension, IRequestManager
- **Presentation**: DiagnosticsViewModel, LoggedOutViewModel, ProductPriceChangedIntegrationEventHandler, WebhookData, IProductImageUrlProvider, CatalogService, OrderStatusNotificationService, MessageProcessor, Extensions, WebhookEndpoints, WebHooksApi, IWebhooksSender, OrderingService, BasketState, LoginViewModel, BasketQuantity, IBasketState, GrantUrlTesterService, ProcessConsentResult, RouteHandlerBuilderExtensions, OrderStatusChangedToSubmittedIntegrationEventHandler, GrantsController, WebhooksRetriever, BasketStateChangedSubscription, OrderStatusChangedToAwaitingValidationIntegrationEvent, AccountController, BasketService, ConsentController, OrderStatusChangedToCancelledIntegrationEventHandler, AccountOptions, ICatalogService, DeviceController, LogoutViewModel, ExternalController, LogOutService, HooksRepository, WebhooksSender
- **Unknown**: HttpRequestExceptionEx, WebNavigatedEventArgsConverter, SettingsService, MainActivity, BadgeView, VisualElementExtensions, IOpenUrlService, ServiceAuthenticationException, DeleteBasketRequest, MauiAuthenticationBrowser, IAppEnvironmentService, OrderStatusChangedToStockConfirmedIntegrationEventHandler, DialogService, IEventBusBuilder, GetBasketRequest, WebNavigatingEventArgsConverter, IBasketService, MapView, ContentPageBase, GenericTypeExtensions, CustomTabbedPage, INavigationService, Program, CatalogServices, ItemsToHeightConverter, ThemeEffects, UpdateBasketRequest, DeleteBasketResponse, CatalogMockService, ILocationService, OrdersApi, MigrateDbContextExtensions, OrderService, RabbitMQTelemetry, BasketMockService, AddForwardHeadersSubscriber, AppDelegate, ISettingsService, HasCountConverter, EventBusBuilder, IIntegrationEventHandler, Extensions, LocationService, IIntegrationEventLogService, App, BasketClient, FadeToAnimation, FadeOutAnimation, BasketItem, SecuritySchemeDefinitionsTransformer, IntegrationEventLogEntry, IntegrationEventLogService, IOrderService, AppEnvironmentService, ITheme, DictionaryExtensions, App, Basket, IValidity, FixUriService, MigrationHostedService, FadeInAnimation, AppShell, ConfigurationExtensions, IsNotNullOrEmptyRule, EventBusSubscriptionInfo, ICommandExtensions, IDbSeeder, IEventBus, Theme, MauiNavigationService, ProductImageUrlProvider, BasketService, CatalogView, OpenUrlService, RabbitMQEventBus

## Diagnostics

| Level | Source | Message |
|-------|--------|---------|
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: eShop.Catalog.API.Infrastructure.Migrations.RemoveHiLoAndIndexCatalogName |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: eShop.Catalog.API.Infrastructure.Migrations.Initial |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Webhooks.API.Migrations.Initial |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: global.Extensions |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Ordering.Infrastructure.Migrations.UseEnumForOrderStatus |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: eShop.HybridApp.Program |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: eShop.HybridApp.Program |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: eShop.HybridApp.AppDelegate |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: eShop.ClientApp.Program |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: eShop.ClientApp.AppDelegate |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: global.Extensions |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Ordering.Infrastructure.Migrations.FixOrderitemseqSchema |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Ordering.Infrastructure.Migrations.Initial |
| Info | CallReachabilityPruner | CallGraph not available; skipping reachability analysis. |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: eShop.Identity.API.Data.Migrations.InitialMigration |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: eShop.Catalog.API.Infrastructure.Migrations.Outbox |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: global.Extensions |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: global.Program |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: Ordering.Infrastructure.Migrations.Outbox |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: eShop.EventBus.Abstractions.IIntegrationEventHandler |
| Warning | SyntaxStructureExtractor | Duplicate type id skipped: eShop.ServiceDefaults.Extensions |

### Pruning notes

- PatternRelevancePruner: pruned test type 'eShop.Ordering.UnitTests.Domain.OrderAggregateTest'
- PatternRelevancePruner: pruned test type 'eShop.Catalog.FunctionalTests.CatalogApiTests'
- PatternRelevancePruner: pruned test type 'eShop.Ordering.FunctionalTests.AutoAuthorizeMiddleware'
- PatternRelevancePruner: pruned test type 'eShop.Ordering.UnitTests.Application.NewOrderRequestHandlerTest'
- PatternRelevancePruner: pruned test type 'eShop.Ordering.FunctionalTests.AutoAuthorizeStartupFilter'
- PatternRelevancePruner: pruned test type 'eShop.Ordering.UnitTests.Domain.OrderBuilder'
- PatternRelevancePruner: pruned test type 'eShop.Ordering.UnitTests.Application.OrdersWebApiTest'
- PatternRelevancePruner: pruned test type 'eShop.Ordering.FunctionalTests.OrderingApiFixture'
- PatternRelevancePruner: pruned test type 'ClientApp.UnitTests.ViewModels.CatalogItemViewModelTests'
- PatternRelevancePruner: pruned test type 'eShop.Ordering.UnitTests.Domain.AddressBuilder'
- PatternRelevancePruner: pruned test type 'ClientApp.UnitTests.Mocks.MockDialogService'
- PatternRelevancePruner: pruned test type 'ClientApp.UnitTests.TestingExtensions'
- PatternRelevancePruner: pruned test type 'eShop.Ordering.UnitTests.Application.IdentifiedCommandHandlerTest'
- PatternRelevancePruner: pruned test type 'ClientApp.UnitTests.Mocks.MockViewModel'
- PatternRelevancePruner: pruned test type 'ClientApp.UnitTests.ViewModels.MockViewModelTests'
- PatternRelevancePruner: pruned test type 'ClientApp.UnitTests.Mocks.MockNavigationService'
- PatternRelevancePruner: pruned test type 'eShop.Ordering.UnitTests.Application.SetStockRejectedOrderStatusCommandTest'
- PatternRelevancePruner: pruned test type 'eShop.Ordering.UnitTests.Domain.BuyerAggregateTest'
- PatternRelevancePruner: pruned test type 'ClientApp.UnitTests.Services.CatalogServiceTests'
- PatternRelevancePruner: pruned test type 'ClientApp.UnitTests.Services.BasketServiceTests'
- PatternRelevancePruner: pruned test type 'ClientApp.UnitTests.ViewModels.CatalogViewModelTests'
- PatternRelevancePruner: pruned test type 'ClientApp.UnitTests.ViewModels.MainViewModelTests'
- PatternRelevancePruner: pruned test type 'eShop.Basket.UnitTests.BasketServiceTests'
- PatternRelevancePruner: pruned test type 'eShop.Ordering.FunctionalTests.OrderingApiTests'
- PatternRelevancePruner: pruned test type 'eShop.Catalog.FunctionalTests.CatalogApiFixture'
- PatternRelevancePruner: pruned test type 'ClientApp.UnitTests.ViewModels.OrderViewModelTests'
- PatternRelevancePruner: pruned test type 'eShop.Basket.UnitTests.Helpers.TestServerCallContext'
- PatternRelevancePruner: pruned test type 'ClientApp.UnitTests.Mocks.MockSettingsService'
- PatternRelevancePruner: pruned test type 'ClientApp.UnitTests.Services.OrdersServiceTests'
- TokenBudgetEnforcer: kept 352 types (136 pruned for budget 19500)

---
*Generated in 38.6ms | 517 types (212 active, 305 pruned) | Compression: TrivialMemberCompressor(−12%) · BoilerplateCompressor(−1%) · StructuralDeduplicator(−22%) | Schema v2.0*
