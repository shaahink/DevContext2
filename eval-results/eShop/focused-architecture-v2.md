## DevContext -- Architecture Overview on ClientApp

**Architecture**: CleanArchitecture (100% confidence)
**Signals**: minimal-apis · mediatr · fluentvalidation
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

| Method | Route | Handler | Auth |
|--------|-------|---------|------|
| DELETE | /{id:int} | async Task<Results<Accepted, NotFound<string>>> (
            WebhooksContext context,
            ClaimsPrincipal user,
            int id) =>
        {
            var userId = user.GetUserId();
            var subscription = await context.Subscriptions.SingleOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (subscription != null)
            {
                context.Remove(subscription);
                await context.SaveChangesAsync();
                return TypedResults.Accepted($"/api/webhooks/{subscription.Id}");
            }

            return TypedResults.NotFound($"Subscriptions {id} not found");
        }.<lambda> | - |
| POST | / | async Task<Results<Created, BadRequest<string>>> (
            WebhookSubscriptionRequest request,
            IGrantUrlTesterService grantUrlTester,
            WebhooksContext context,
            ClaimsPrincipal user) =>
        {
            var grantOk = await grantUrlTester.TestGrantUrl(request.Url, request.GrantUrl, request.Token ?? string.Empty);

            if (grantOk)
            {
                var subscription = new WebhookSubscription()
                {
                    Date = DateTime.UtcNow,
                    DestUrl = request.Url,
                    Token = request.Token,
                    Type = Enum.Parse<WebhookType>(request.Event, ignoreCase: true),
                    UserId = user.GetUserId()
                };

                context.Add(subscription);
                await context.SaveChangesAsync();

                return TypedResults.Created($"/api/webhooks/{subscription.Id}");
            }
            else
            {
                return TypedResults.BadRequest($"Invalid grant URL: {request.GrantUrl}");
            }
        }.<lambda> | - |
| GET | /{id:int} | async Task<Results<Ok<WebhookSubscription>, NotFound<string>>> (
            WebhooksContext context,
            ClaimsPrincipal user,
            int id) =>
        {
            var userId = user.GetUserId();
            var subscription = await context.Subscriptions
                .SingleOrDefaultAsync(s => s.Id == id && s.UserId == userId);
            if (subscription != null)
            {
                return TypedResults.Ok(subscription);
            }
            return TypedResults.NotFound($"Subscriptions {id} not found");
        }.<lambda> | - |
| GET | / | async (WebhooksContext context, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var data = await context.Subscriptions.Where(s => s.UserId == userId).ToListAsync();
            return TypedResults.Ok(data);
        }.<lambda> | - |
| POST | /webhook-received | async (WebhookData hook, HttpRequest request, ILogger<Program> logger, HooksRepository hooksRepository) =>
        {
            var token = request.Headers[webhookCheckHeader];

            logger.LogInformation("Received hook with token {Token}. My token is {MyToken}. Token validation is set to {ValidateToken}", token, tokenToValidate, validateToken);

            if (!validateToken || tokenToValidate == token)
            {
                logger.LogInformation("Received hook is going to be processed");
                var newHook = new WebHookReceived()
                {
                    Data = hook.Payload,
                    When = hook.When,
                    Token = token
                };
                await hooksRepository.AddNew(newHook);
                logger.LogInformation("Received hook was processed.");
                return Results.Ok(newHook);
            }

            logger.LogInformation("Received hook is NOT processed - Bad Request returned.");
            return Results.BadRequest();
        }.<lambda> | - |
| POST | /logout | async (HttpContext httpContext, IAntiforgery antiforgery) =>
        {
            await antiforgery.ValidateRequestAsync(httpContext);
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }.<lambda> | - |
| POST | / | CreateOrderAsync.CreateOrderAsync | - |
| POST | /draft | CreateOrderDraftAsync.CreateOrderDraftAsync | - |
| GET | /cardtypes | GetCardTypesAsync.GetCardTypesAsync | - |
| GET | / | GetOrdersByUserAsync.GetOrdersByUserAsync | - |
| GET | {orderId:int} | GetOrderAsync.GetOrderAsync | - |
| PUT | /ship | ShipOrderAsync.ShipOrderAsync | - |
| PUT | /cancel | CancelOrderAsync.CancelOrderAsync | - |
| DELETE | /items/{id:int} | DeleteItemById.DeleteItemById | - |
| POST | /items | CreateItem.CreateItem | - |
| PUT | /items/{id:int} | UpdateItem.UpdateItem | - |
| PUT | /items | UpdateItemV1.UpdateItemV1 | - |
| GET | /catalogbrands | [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
            async (CatalogContext context) => await context.CatalogBrands.OrderBy(x => x.Brand).ToListAsync().<lambda> | - |
| GET | /catalogtypes | [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
            async (CatalogContext context) => await context.CatalogTypes.OrderBy(x => x.Type).ToListAsync().<lambda> | - |
| GET | /items/type/all/brand/{brandId:int?} | GetItemsByBrandId.GetItemsByBrandId | - |
| GET | /items/type/{typeId}/brand/{brandId?} | GetItemsByBrandAndTypeId.GetItemsByBrandAndTypeId | - |
| GET | /items/withsemanticrelevance | GetItemsBySemanticRelevance.GetItemsBySemanticRelevance | - |
| GET | /items/withsemanticrelevance/{text:minlength(1)} | GetItemsBySemanticRelevanceV1.GetItemsBySemanticRelevanceV1 | - |
| GET | /items/{id:int}/pic | GetItemPictureById.GetItemPictureById | - |
| GET | /items/by/{name:minlength(1)} | GetItemsByName.GetItemsByName | - |
| GET | /items/{id:int} | GetItemById.GetItemById | - |
| GET | /items/by | GetItemsByIds.GetItemsByIds | - |
| GET | /items | GetAllItems.GetAllItems | - |
| GET | /items | GetAllItemsV1.GetAllItemsV1 | - |
| GET | / | () => Results.Redirect($"/scalar/{defaultDocument}").<lambda> | - |

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
| 1 | UseExceptionHandler | UseX |
| 1 | UseDefaultOpenApi | UseX |
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
| Transient | IWebhooksSender | WebhooksSender |
| Transient | IWebhooksRetriever | WebhooksRetriever |
| Transient | IGrantUrlTesterService | GrantUrlTesterService |
| Extension | AddMigration | ? |
| Scoped | ICatalogAI | CatalogAI |
| Extension | AddOptions | ? |
| Transient | ICatalogIntegrationEventService | CatalogIntegrationEventService |
| Transient | IIntegrationEventLogService | IntegrationEventLogService<CatalogContext> |
| Extension | AddMigration | ? |
| Extension | AddDbContext | ? |
| Singleton | IBasketRepository | RedisBasketRepository |
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
| Singleton | WebAppComponents.Services.IProductImageUrlProvider | ProductImageUrlProvider |
| Extension | AddHttpClient | o => o.BaseAddress = new(MobileBffHost) |
| Extension | AddMauiBlazorWebView | ? |
| Extension | AddHttpClient | o => o.BaseAddress = new("http://webhooks-api") |
| Extension | AddApiVersion | 1.0 |
| Extension | AddAuthToken | ? |
| Singleton | HooksRepository | HooksRepository |
| Extension | AddOptions | ? |
| Extension | AddKeyedTransient | typeof(T) |
| Extension | AddRazorComponents | ? |
| Extension | AddInteractiveServerComponents | ? |
| Extension | AddRazorComponents | ? |
| Extension | AddInteractiveServerComponents | ? |
| Extension | AddApiVersioning | options =>
{
    // Include "api-supported-versions" and "api-deprecated-versions" headers in all responses
    options.ReportApiVersions = true;
} |
| Extension | AddProblemDetails | ? |
| Extension | AddHttpContextAccessor | ? |
| Singleton | sp => (RabbitMQEventBus)sp.GetRequiredService<IEventBus>() | sp => (RabbitMQEventBus)sp.GetRequiredService<IEventBus>() |
| Singleton | IEventBus | RabbitMQEventBus |
| Singleton | RabbitMQTelemetry | RabbitMQTelemetry |
| Extension | AddOpenTelemetry | ? |
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
| Extension | AddHostedService | ? |
| Extension | AddOptions | ? |
| Extension | AddApiVersioning | options =>
{
    // Include "api-supported-versions" and "api-deprecated-versions" headers in all responses
    options.ReportApiVersions = true;
} |
| Extension | AddOptions | ? |
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

## Related types grouped by layer

- **Api**: OrderStatusChangedToCancelledIntegrationEvent, CreateOrderCommand, OrderStockRejectedIntegrationEvent, SetStockRejectedOrderStatusCommandHandler, PaginatedItems, OrderStatusChangedToPaidIntegrationEvent, ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler, OrderStatusChangedToPaidIntegrationEventHandler, EFLoginService, GracePeriodConfirmedIntegrationEvent, SetStockConfirmedOrderStatusCommandHandler, IRedirectService, RedirectService, OrderStartedIntegrationEventHandler, ILoginService, OrderingApiTrace, IdentifiedCommandHandler, IBasketRepository, OrderPaymentSucceededIntegrationEventHandler, CatalogAI, CatalogItem, Config, OrderStatusChangedToStockConfirmedDomainEventHandler, UsersSeed, UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler, ServerCallContextIdentityExtensions, ProfileService, CreateOrderCommandHandler, OrderingContextSeed, OrderStatusChangedToPaidDomainEventHandler, OrderDraftDTO, CatalogTypeEntityTypeConfiguration, OrderingIntegrationEventService, OrderStatusChangedToAwaitingValidationDomainEventHandler, ICatalogIntegrationEventService, LoggingBehavior, BasketItem, OrderStockItem, IdentifiedCommand, OrderPaymentFailedIntegrationEventHandler, CreateOrderCommandValidator, LinqSelectExtensions, CatalogIntegrationEventService, CatalogItemEntityTypeConfiguration, IIdentityService, IOrderQueries, CatalogApi, CancelOrderIdentifiedCommandHandler, CatalogContext, OrderStockRejectedIntegrationEventHandler, Initial, RedisBasketRepository, BasketService, CreateOrderDraftCommandHandler, OrderCancelledDomainEventHandler, SetAwaitingValidationOrderStatusCommandHandler, ShipOrderCommandHandler, CancelOrderCommandHandler, SetPaidOrderStatusCommandHandler, OrderShippedDomainEventHandler
- **Application**: NewOrderRequestHandlerTest, OrdersWebApiTest, IdentifiedCommandHandlerTest, SetStockRejectedOrderStatusCommandTest
- **Domain**: OrderAggregateTest, ClientRequestEntityTypeConfiguration, OrderDetailViewModel, OrderShippedDomainEvent, BasketViewModel, IViewModelBase, IRepository, CustomerBasket, PaymentMethodEntityTypeConfiguration, ViewModelBase, AuthorizeRequest, Address, OrderStatusChangedToPaidDomainEvent, IAggregateRoot, LoginViewModel, OrderBuilder, PaymentMethod, MapViewModel, CatalogItemViewModelTests, AddressBuilder, CancelOrderCommand, OrderEntityTypeConfiguration, MockViewModelTests, IBuyerRepository, OrderItem, MainViewModel, IUnitOfWork, SettingsViewModel, OrderItemEntityTypeConfiguration, BuyerAggregateTest, BuyerEntityTypeConfiguration, Buyer, CatalogViewModel, BasketItem
- **Infrastructure**: BuyerRepository, RequestManager, OrderRepository, MediatorExtension, OrderingContextModelSnapshot, IRequestManager
- **Presentation**: DiagnosticsViewModel, LoggedOutViewModel, ProductPriceChangedIntegrationEventHandler, WebhookData, IProductImageUrlProvider, CatalogService, OrderStatusNotificationService, MessageProcessor, Extensions, WebhookEndpoints, WebHooksApi, IWebhooksSender, OrderingService, BasketState, LoginViewModel, BasketQuantity, IBasketState, GrantUrlTesterService, ProcessConsentResult, RouteHandlerBuilderExtensions, OrderStatusChangedToSubmittedIntegrationEventHandler, GrantsController, WebhooksRetriever, BasketStateChangedSubscription, OrderStatusChangedToAwaitingValidationIntegrationEvent, AccountController, BasketService, ConsentController, OrderStatusChangedToCancelledIntegrationEventHandler, AccountOptions, ICatalogService, DeviceController
- **Unknown**: HttpRequestExceptionEx, WebNavigatedEventArgsConverter, SettingsService, MainActivity, BadgeView, VisualElementExtensions, IOpenUrlService, CatalogApiTests, ServiceAuthenticationException, DeleteBasketRequest, MauiAuthenticationBrowser, IAppEnvironmentService, OrderStatusChangedToStockConfirmedIntegrationEventHandler, DialogService, AutoAuthorizeMiddleware, IEventBusBuilder, GetBasketRequest, WebNavigatingEventArgsConverter, IBasketService, MapView, ContentPageBase, GenericTypeExtensions, CustomTabbedPage, INavigationService, Program, CatalogServices, ItemsToHeightConverter, ThemeEffects, AutoAuthorizeStartupFilter, UpdateBasketRequest, DeleteBasketResponse, OrderingApiFixture, CatalogMockService, ILocationService, OrdersApi, MigrateDbContextExtensions, OrderService, RabbitMQTelemetry, BasketMockService, AddForwardHeadersSubscriber, AppDelegate, ISettingsService, HasCountConverter, EventBusBuilder, IIntegrationEventHandler, Extensions, TestingExtensions, LocationService, IIntegrationEventLogService, App, MockViewModel, BasketClient, FadeToAnimation, FadeOutAnimation, BasketItem, SecuritySchemeDefinitionsTransformer, IntegrationEventLogEntry, IntegrationEventLogService, IOrderService, AppEnvironmentService, MockNavigationService, ITheme, DictionaryExtensions, App, Basket, IValidity, FixUriService, MigrationHostedService, FadeInAnimation, CatalogServiceTests, AppShell, ConfigurationExtensions, IsNotNullOrEmptyRule, EventBusSubscriptionInfo, ICommandExtensions, IDbSeeder

---
*Generated in 15.2ms | 517 types (212 active, 305 pruned) | Compressed: 1 strategies | Schema v2.0*
