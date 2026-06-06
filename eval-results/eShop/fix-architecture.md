## DevContext -- Architecture Overview on ClientApp

**Architecture**: MinimalApi (100% confidence)
**Signals**: controllers · minimal-apis · mediatr · fluentvalidation
**Projects**: 24 -- Basket.API, Catalog.API, ClientApp, eShop.AppHost, eShop.ServiceDefaults, EventBus, EventBusRabbitMQ, HybridApp, Identity.API, IntegrationEventLogEF, Ordering.API, Ordering.Domain, Ordering.Infrastructure, OrderProcessor, PaymentProcessor, WebApp, WebAppComponents, WebhookClient, Webhooks.API, Basket.UnitTests, Catalog.FunctionalTests, ClientApp.UnitTests, Ordering.FunctionalTests, Ordering.UnitTests
**Profile**: focused | **Tokens**: ~20000 (budget 20000) | **Types**: 29 in output

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
| POST | /Grants | - | GrantsController.Revoke | - | GrantsController.cs:41 |
| GET | /Grants | - | GrantsController.Index | - | GrantsController.cs:32 |
| POST | /Device | - | DeviceController.Callback | - | DeviceController.cs:51 |
| POST | /Device | - | DeviceController.UserCodeCapture | - | DeviceController.cs:41 |
| GET | /Device | - | DeviceController.Index | - | DeviceController.cs:27 |
| POST | /Consent | - | ConsentController.Index | - | ConsentController.cs:47 |
| GET | /Consent | - | ConsentController.Index | - | ConsentController.cs:32 |
| GET | /External | - | ExternalController.Callback | - | ExternalController.cs:63 |
| GET | /External | - | ExternalController.Challenge | - | ExternalController.cs:33 |
| GET | /Account | - | AccountController.AccessDenied | - | AccountController.cs:196 |
| POST | /Account | - | AccountController.Logout | - | AccountController.cs:165 |
| GET | /Account | - | AccountController.Logout | - | AccountController.cs:146 |
| POST | /Account | - | AccountController.Login | - | AccountController.cs:59 |
| GET | /Account | - | AccountController.Login | - | AccountController.cs:39 |
| DELETE | /api/webhooks/{id:int} | /api/webhooks | λ WebHooksApi.cs:66 | - | WebHooksApi.cs:66 |
| POST | /api/webhooks/ | /api/webhooks | λ WebHooksApi.cs:35 | - | WebHooksApi.cs:35 |
| GET | /api/webhooks/{id:int} | /api/webhooks | λ WebHooksApi.cs:20 | - | WebHooksApi.cs:20 |
| GET | /api/webhooks/ | /api/webhooks | λ WebHooksApi.cs:13 | - | WebHooksApi.cs:13 |
| POST | /webhook-received | - | λ WebhookEndpoints.cs:31 | - | WebhookEndpoints.cs:31 |
| POST | /logout | - | λ AuthenticationEndpoints.cs:12 | - | AuthenticationEndpoints.cs:12 |
| POST | /api/orders/ | api/orders | CreateOrderAsync.CreateOrderAsync | - | OrdersApi.cs:17 |
| POST | /api/orders/draft | api/orders | CreateOrderDraftAsync.CreateOrderDraftAsync | - | OrdersApi.cs:16 |
| GET | /api/orders/cardtypes | api/orders | GetCardTypesAsync.GetCardTypesAsync | - | OrdersApi.cs:15 |
| GET | /api/orders/ | api/orders | GetOrdersByUserAsync.GetOrdersByUserAsync | - | OrdersApi.cs:14 |
| GET | /api/orders/{orderId:int} | api/orders | GetOrderAsync.GetOrderAsync | - | OrdersApi.cs:13 |
| PUT | /api/orders/ship | api/orders | ShipOrderAsync.ShipOrderAsync | - | OrdersApi.cs:12 |
| PUT | /api/orders/cancel | api/orders | CancelOrderAsync.CancelOrderAsync | - | OrdersApi.cs:11 |
| DELETE | /api/catalog/items/{id:int} | api/catalog | DeleteItemById.DeleteItemById | - | CatalogApi.cs:107 |
| POST | /api/catalog/items | api/catalog | CreateItem.CreateItem | - | CatalogApi.cs:103 |
| PUT | /api/catalog/items/{id:int} | api/catalog | UpdateItem.UpdateItem | - | CatalogApi.cs:98 |
| PUT | /api/catalog/items | api/catalog | UpdateItemV1.UpdateItemV1 | - | CatalogApi.cs:93 |
| GET | /api/catalog/catalogbrands | api/catalog | λ CatalogApi.cs:84 | - | CatalogApi.cs:84 |
| GET | /api/catalog/catalogtypes | api/catalog | λ CatalogApi.cs:77 | - | CatalogApi.cs:77 |
| GET | /api/catalog/items/type/all/brand/{brandId:int?} | api/catalog | GetItemsByBrandId.GetItemsByBrandId | - | CatalogApi.cs:72 |
| GET | /api/catalog/items/type/{typeId}/brand/{brandId?} | api/catalog | GetItemsByBrandAndTypeId.GetItemsByBrandAndTypeId | - | CatalogApi.cs:67 |
| GET | /api/catalog/items/withsemanticrelevance | api/catalog | GetItemsBySemanticRelevance.GetItemsBySemanticRelevance | - | CatalogApi.cs:60 |
| GET | /api/catalog/items/withsemanticrelevance/{text:minlength(1)} | api/catalog | GetItemsBySemanticRelevanceV1.GetItemsBySemanticRelevanceV1 | - | CatalogApi.cs:53 |
| GET | /api/catalog/items/{id:int}/pic | api/catalog | GetItemPictureById.GetItemPictureById | - | CatalogApi.cs:46 |
| GET | /api/catalog/items/by/{name:minlength(1)} | api/catalog | GetItemsByName.GetItemsByName | - | CatalogApi.cs:41 |
| GET | /api/catalog/items/{id:int} | api/catalog | GetItemById.GetItemById | - | CatalogApi.cs:36 |
| GET | /api/catalog/items/by | api/catalog | GetItemsByIds.GetItemsByIds | - | CatalogApi.cs:31 |
| GET | /api/catalog/items | api/catalog | GetAllItems.GetAllItems | - | CatalogApi.cs:26 |
| GET | /api/catalog/items | api/catalog | GetAllItemsV1.GetAllItemsV1 | - | CatalogApi.cs:21 |
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
| 1 | UseStaticFiles | UseX |
| 1 | UseDefaultOpenApi | UseX |
| 1 | UseStatusCodePages | UseX |
| 1 | UseDefaultOpenApi | UseX |
| 1 | UseExceptionHandler | UseX |
| 1 | UseExceptionHandler | UseX |
| 1 | MapGrpcService | MapX |
| 2 | UseCookiePolicy | UseX |
| 2 | UseDefaultOpenApi | UseX |
| 2 | UseHsts | UseX |
| 2 | UseHsts | UseX |
| 3 | UseRouting | UseX |
| 3 | UseAntiforgery | UseX |
| 3 | UseAntiforgery | UseX |
| 4 | UseIdentityServer | UseX |
| 4 | UseStaticFiles | UseX |
| 4 | UseHttpsRedirection | UseX |
| 5 | UseAuthorization | UseX |
| 5 | UseStaticFiles | UseX |

### DI registrations

| Lifetime | Service | Implementation | Source |
|----------|---------|----------------|--------|
| Singleton | IBasketRepository | RedisBasketRepository | Extensions.cs:16 |
| Extension | AddRazorComponents | (AddRazorComponents) | Program.cs:5 |
| Extension | AddInteractiveServerComponents | (AddInteractiveServerComponents) | Program.cs:5 |
| Extension | AddRazorComponents | (AddRazorComponents) | Program.cs:8 |
| Extension | AddInteractiveServerComponents | (AddInteractiveServerComponents) | Program.cs:8 |
| Extension | AddApiVersioning | options =>... | Program.cs:7 |
| Extension | AddProblemDetails | (AddProblemDetails) | Program.cs:5 |
| Transient | IRedirectService | RedirectService | Program.cs:41 |
| Transient | ILoginService<ApplicationUser> | EFLoginService | Program.cs:40 |
| Transient | IProfileService | ProfileService | Program.cs:39 |
| Extension | AddIdentityServer | options =>... | Program.cs:18 |
| Extension | AddInMemoryIdentityResources | Config.GetResources() | Program.cs:18 |
| Extension | AddInMemoryApiScopes | Config.GetApiScopes() | Program.cs:18 |
| Extension | AddInMemoryApiResources | Config.GetApis() | Program.cs:18 |
| Extension | AddInMemoryClients | Config.GetClients(builder.Configuration) | Program.cs:18 |
| Extension | AddAspNetIdentity | (AddAspNetIdentity) | Program.cs:18 |
| Extension | AddDeveloperSigningCredential | (AddDeveloperSigningCredential) | Program.cs:18 |
| Extension | AddIdentity | (AddIdentity) | Program.cs:14 |
| Extension | AddEntityFrameworkStores | (AddEntityFrameworkStores) | Program.cs:14 |
| Extension | AddDefaultTokenProviders | (AddDefaultTokenProviders) | Program.cs:14 |
| Extension | AddMigration | (AddMigration) | Program.cs:12 |
| Extension | AddControllersWithViews | (AddControllersWithViews) | Program.cs:5 |
| Singleton | WebAppComponents.Services.IProductImageUrlProvider | ProductImageUrlProvider | MauiProgram.cs:31 |
| Extension | AddHttpClient | o => o.BaseAddress = new(MobileBffHost) | MauiProgram.cs:30 |
| Extension | AddMauiBlazorWebView | (AddMauiBlazorWebView) | MauiProgram.cs:22 |
| Transient | SettingsView | SettingsView | MauiProgram.cs:181 |
| Transient | ProfileView | ProfileView | MauiProgram.cs:180 |
| Transient | MapView | MapView | MauiProgram.cs:179 |
| Transient | OrderDetailView | OrderDetailView | MauiProgram.cs:178 |
| Transient | LoginView | LoginView | MauiProgram.cs:177 |
| Transient | FiltersView | FiltersView | MauiProgram.cs:176 |
| Transient | CheckoutView | CheckoutView | MauiProgram.cs:175 |
| Transient | CatalogView | CatalogView | MauiProgram.cs:174 |
| Transient | BasketView | BasketView | MauiProgram.cs:173 |
| Singleton | CatalogItemView | CatalogItemView | MauiProgram.cs:171 |
| Transient | SettingsViewModel | SettingsViewModel | MauiProgram.cs:164 |
| Transient | OrderDetailViewModel | OrderDetailViewModel | MauiProgram.cs:163 |
| Transient | CheckoutViewModel | CheckoutViewModel | MauiProgram.cs:162 |
| Singleton | ProfileViewModel | ProfileViewModel | MauiProgram.cs:160 |
| Singleton | MapViewModel | MapViewModel | MauiProgram.cs:159 |
| Singleton | CatalogItemViewModel | CatalogItemViewModel | MauiProgram.cs:158 |
| Singleton | CatalogViewModel | CatalogViewModel | MauiProgram.cs:157 |
| Singleton | BasketViewModel | BasketViewModel | MauiProgram.cs:156 |
| Singleton | LoginViewModel | LoginViewModel | MauiProgram.cs:155 |
| Singleton | MainViewModel | MainViewModel | MauiProgram.cs:154 |
| Transient | IBrowser | MauiAuthenticationBrowser | MauiProgram.cs:116 |
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
            } | serviceProvider =>... | MauiProgram.cs:97 |
| Singleton | ITheme | Theme | MauiProgram.cs:95 |
| Singleton | ILocationService | LocationService | MauiProgram.cs:93 |
| Singleton | IFixUriService | FixUriService | MauiProgram.cs:92 |
| Singleton | sp =>
            {
                var browser = sp.GetRequiredService<IBrowser>();
                var settingsService = sp.GetRequiredService<ISettingsService>();
                var debugHttpHandler = sp.GetKeyedService<HttpMessageHandler>("DebugHttpMessageHandler");
                return new IdentityService(browser, settingsService, debugHttpHandler);
            } | sp =>... | MauiProgram.cs:84 |
| Singleton | sp =>
            {
                var debugHttpHandler = sp.GetKeyedService<HttpMessageHandler>("DebugHttpMessageHandler");
                return new RequestProvider(debugHttpHandler);
            } | sp =>... | MauiProgram.cs:78 |
| Singleton | IOpenUrlService | OpenUrlService | MauiProgram.cs:77 |
| Singleton | IDialogService | DialogService | MauiProgram.cs:76 |
| Singleton | INavigationService | MauiNavigationService | MauiProgram.cs:75 |
| Singleton | ISettingsService | SettingsService | MauiProgram.cs:74 |
| Extension | AddOptions | (AddOptions) | Program.cs:8 |
| Singleton | sp => (RabbitMQEventBus)sp.GetRequiredService<IEventBus>() | sp => (RabbitMQEventBus)sp.GetRequiredService<IEventBus>() | RabbitMqDependencyInjectionExtensions.cs:37 |
| Singleton | IEventBus | RabbitMQEventBus | RabbitMqDependencyInjectionExtensions.cs:35 |
| Singleton | RabbitMQTelemetry | RabbitMQTelemetry | RabbitMqDependencyInjectionExtensions.cs:34 |
| Extension | AddOpenTelemetry | (AddOpenTelemetry) | RabbitMqDependencyInjectionExtensions.cs:24 |
| Extension | AddHttpContextAccessor | (AddHttpContextAccessor) | HttpClientExtensions.cs:13 |
| Extension | AddHttpClient | o => o.BaseAddress = new("http://webhooks-api") | Extensions.cs:19 |
| Extension | AddApiVersion | 1.0 | Extensions.cs:19 |
| Extension | AddAuthToken | (AddAuthToken) | Extensions.cs:19 |
| Singleton | HooksRepository | HooksRepository | Extensions.cs:16 |
| Extension | AddOptions | (AddOptions) | Extensions.cs:15 |
| Extension | AddHttpClient | o => o.BaseAddress = new("https+http://ordering-api") | Extensions.cs:38 |
| Extension | AddApiVersion | 1.0 | Extensions.cs:38 |
| Extension | AddAuthToken | (AddAuthToken) | Extensions.cs:38 |
| Extension | AddHttpClient | o => o.BaseAddress = new("https+http://catalog-api") | Extensions.cs:34 |
| Extension | AddApiVersion | 2.0 | Extensions.cs:34 |
| Extension | AddAuthToken | (AddAuthToken) | Extensions.cs:34 |
| Extension | AddGrpcClient | o => o.Address = new("http://basket-api") | Extensions.cs:31 |
| Extension | AddAuthToken | (AddAuthToken) | Extensions.cs:31 |
| Singleton | IProductImageUrlProvider | ProductImageUrlProvider | Extensions.cs:27 |
| Singleton | OrderStatusNotificationService | OrderStatusNotificationService | Extensions.cs:26 |
| Singleton | BasketService | BasketService | Extensions.cs:25 |
| Scoped | LogOutService | LogOutService | Extensions.cs:24 |
| Scoped | BasketState | BasketState | Extensions.cs:23 |
| Extension | AddHttpForwarderWithServiceDiscovery | (AddHttpForwarderWithServiceDiscovery) | Extensions.cs:20 |
| Extension | AddHostedService | (AddHostedService) | Extensions.cs:18 |
| Extension | AddOptions | (AddOptions) | Extensions.cs:15 |
| Extension | AddApiVersioning | options =>... | Program.cs:6 |
| Extension | AddGrpc | (AddGrpc) | Program.cs:6 |
| Transient | IWebhooksSender | WebhooksSender | Extensions.cs:16 |
| Transient | IWebhooksRetriever | WebhooksRetriever | Extensions.cs:15 |
| Transient | IGrantUrlTesterService | GrantUrlTesterService | Extensions.cs:14 |
| Extension | AddMigration | (AddMigration) | Extensions.cs:12 |
| Extension | AddKeyedTransient | typeof(T) | EventBusBuilderExtensions.cs:27 |
| Scoped | ICatalogAI | CatalogAI | Extensions.cs:49 |
| Extension | AddOptions | (AddOptions) | Extensions.cs:35 |
| Transient | ICatalogIntegrationEventService | CatalogIntegrationEventService | Extensions.cs:29 |
| Transient | IIntegrationEventLogService | IntegrationEventLogService<CatalogContext> | Extensions.cs:27 |
| Extension | AddMigration | (AddMigration) | Extensions.cs:24 |
| Extension | AddDbContext | (AddDbContext) | Extensions.cs:11 |
| Extension | AddHealthChecks | (AddHealthChecks) | Extensions.cs:101 |
| Extension | AddCheck | "self" | Extensions.cs:101 |
| Extension | AddOpenTelemetry | (AddOpenTelemetry) | Extensions.cs:58 |
| Extension | AddServiceDiscovery | (AddServiceDiscovery) | Extensions.cs:20 |
| Extension | AddApiVersioning | options =>... | Program.cs:7 |
| Extension | AddProblemDetails | (AddProblemDetails) | Program.cs:5 |

## Related types grouped by layer

- **Api**: CreateOrderCommand, SetStockRejectedOrderStatusCommandHandler, ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler, SetStockConfirmedOrderStatusCommandHandler, IdentifiedCommandHandler, OrderStatusChangedToStockConfirmedDomainEventHandler, UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler, CreateOrderCommandHandler, OrderStatusChangedToPaidDomainEventHandler, OrderStatusChangedToAwaitingValidationDomainEventHandler, IdentifiedCommand, CreateOrderDraftCommandHandler, OrderCancelledDomainEventHandler, SetAwaitingValidationOrderStatusCommandHandler, ShipOrderCommandHandler, CancelOrderCommandHandler, SetPaidOrderStatusCommandHandler, OrderShippedDomainEventHandler
- **Domain**: OrderDetailViewModel, BasketViewModel
- **Presentation**: GrantsController, AccountController, ConsentController, DeviceController, ExternalController
- **Unknown**: SettingsService, MauiAuthenticationBrowser, DialogService, ProductImageUrlProvider

---
*Generated in 19.7ms | 517 types (29 active, 488 pruned) | Compression: TrivialMemberCompressor(−10%) · StructuralDeduplicator(−1%) | Schema v2.0*
