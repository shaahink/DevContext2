## DevContext -- Architecture Overview on ClientApp

**Architecture**: CleanArchitecture (100% confidence)
**Signals**: minimal-apis · mediatr · fluentvalidation
**Projects**: 24 -- Basket.API, Catalog.API, ClientApp, eShop.AppHost, eShop.ServiceDefaults, EventBus, EventBusRabbitMQ, HybridApp, Identity.API, IntegrationEventLogEF, Ordering.API, Ordering.Domain, Ordering.Infrastructure, OrderProcessor, PaymentProcessor, WebApp, WebAppComponents, WebhookClient, Webhooks.API, Basket.UnitTests, Catalog.FunctionalTests, ClientApp.UnitTests, Ordering.FunctionalTests, Ordering.UnitTests
**Profile**: focused | **Tokens**: ~6000 (budget 6000) | **Types**: 74 in output

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

No endpoints detected.

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
| 1 | UseDefaultOpenApi | UseX |
| 1 | UseExceptionHandler | UseX |
| 1 | UseDefaultOpenApi | UseX |
| 1 | UseStaticFiles | UseX |
| 1 | UseExceptionHandler | UseX |
| 1 | UseStatusCodePages | UseX |
| 1 | MapGrpcService | MapX |
| 2 | UseHsts | UseX |
| 2 | UseCookiePolicy | UseX |
| 2 | UseHsts | UseX |
| 2 | UseDefaultOpenApi | UseX |
| 3 | UseAntiforgery | UseX |
| 3 | UseRouting | UseX |
| 3 | UseAntiforgery | UseX |
| 4 | UseStaticFiles | UseX |
| 4 | UseIdentityServer | UseX |
| 4 | UseHttpsRedirection | UseX |
| 5 | UseAuthorization | UseX |
| 5 | UseStaticFiles | UseX |

### DI registrations

| Lifetime | Service | Implementation |
|----------|---------|----------------|
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
| Extension | AddRazorComponents | ? |
| Extension | AddInteractiveServerComponents | ? |
| Extension | AddOptions | ? |
| Extension | AddApiVersioning | options =>
{
    // Include "api-supported-versions" and "api-deprecated-versions" headers in all responses
    options.ReportApiVersions = true;
} |
| Extension | AddProblemDetails | ? |
| Singleton | IBasketRepository | RedisBasketRepository |
| Singleton | WebAppComponents.Services.IProductImageUrlProvider | ProductImageUrlProvider |
| Extension | AddHttpClient | o => o.BaseAddress = new(MobileBffHost) |
| Extension | AddMauiBlazorWebView | ? |
| Extension | AddHealthChecks | ? |
| Extension | AddCheck | "self" |
| Extension | AddOpenTelemetry | ? |
| Extension | AddServiceDiscovery | ? |
| Extension | AddApiVersioning | options =>
{
    // Include "api-supported-versions" and "api-deprecated-versions" headers in all responses
    options.ReportApiVersions = true;
} |
| Extension | AddProblemDetails | ? |
| Extension | AddGrpc | ? |
| Extension | AddHttpClient | o => o.BaseAddress = new("http://webhooks-api") |
| Extension | AddApiVersion | 1.0 |
| Extension | AddAuthToken | ? |
| Singleton | HooksRepository | HooksRepository |
| Extension | AddOptions | ? |
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

## Related types grouped by layer

- **Api**: OrderStatusChangedToCancelledIntegrationEvent, CreateOrderCommand, OrderStockRejectedIntegrationEvent, SetStockRejectedOrderStatusCommandHandler, ApplicationUser, PaginatedItems, CreateOrderIdentifiedCommandHandler, OrderStatusChangedToPaidIntegrationEvent, ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler, OrderStatusChangedToPaidIntegrationEventHandler, EFLoginService, GracePeriodConfirmedIntegrationEvent, CancelOrderCommand, SetStockConfirmedOrderStatusCommandHandler, IRedirectService, RedirectService, OrderStartedIntegrationEventHandler, ILoginService, IdentifiedCommandHandler, OrderStatusChangedToStockConfirmedDomainEventHandler, UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler, CreateOrderCommandHandler, OrderStatusChangedToPaidDomainEventHandler, OrderStatusChangedToAwaitingValidationDomainEventHandler, IdentifiedCommand, CreateOrderDraftCommandHandler, OrderCancelledDomainEventHandler, SetAwaitingValidationOrderStatusCommandHandler, ShipOrderCommandHandler, CancelOrderCommandHandler, SetPaidOrderStatusCommandHandler, OrderShippedDomainEventHandler
- **Domain**: OrderAggregateTest, ClientRequestEntityTypeConfiguration, OrderDetailViewModel, OrderShippedDomainEvent, BasketViewModel, IViewModelBase, IRepository
- **Infrastructure**: BuyerRepository, RequestManager
- **Presentation**: DiagnosticsViewModel, LoggedOutViewModel, ProductPriceChangedIntegrationEventHandler, WebhookData, OrderStatusChangedToCancelledIntegrationEvent, IProductImageUrlProvider, CatalogService, OrderStatusNotificationService, MessageProcessor, Extensions, WebhookEndpoints, WebHooksApi
- **Unknown**: HttpRequestExceptionEx, WebNavigatedEventArgsConverter, SettingsService, MainActivity, BadgeView, VisualElementExtensions, IOpenUrlService, CatalogApiTests, ServiceAuthenticationException, DeleteBasketRequest, MauiAuthenticationBrowser, IAppEnvironmentService, OrderStatusChangedToStockConfirmedIntegrationEventHandler, DialogService, AutoAuthorizeMiddleware, IEventBusBuilder, GetBasketRequest, WebNavigatingEventArgsConverter, IBasketService, MapView, ContentPageBase

---
*Generated in 68.5ms | 517 types (74 active, 443 pruned) | Compressed: 1 strategies | Schema v2.0*
