MAP  eShop     (19 projects)

STACK  net10.0, net10.0-android;net10.0-ios;net10.0-maccatalyst, net10.0-android;net10.0-ios;net10.0-maccatalyst;net10.0 · Minimal APIs · Controllers · MediatR (CQRS) · EF Core · FluentValidation · DDD aggregates

STYLE  Microservices  (confidence high)
       evidence: Aspire orchestration with 22 service projects

TOPOLOGY (depends-on)
   eShop.ServiceDefaults
   EventBusRabbitMQ ── EventBus
   IntegrationEventLogEF ── EventBus
   EventBus
   Ordering.Domain
   WebAppComponents
   Basket.API ── eShop.ServiceDefaults, EventBusRabbitMQ
   Catalog.API ── eShop.ServiceDefaults, EventBusRabbitMQ, IntegrationEventLogEF
   Identity.API ── eShop.ServiceDefaults
   Ordering.API ── eShop.ServiceDefaults, EventBusRabbitMQ, IntegrationEventLogEF, Ordering.Domain, Ordering.Infrastructure
   Ordering.Infrastructure ── IntegrationEventLogEF, Ordering.Domain
   OrderProcessor ── eShop.ServiceDefaults, EventBusRabbitMQ
   PaymentProcessor ── eShop.ServiceDefaults, EventBusRabbitMQ
   WebApp ── eShop.ServiceDefaults, EventBusRabbitMQ, WebAppComponents
   WebhookClient ── eShop.ServiceDefaults
   Webhooks.API ── eShop.ServiceDefaults, EventBusRabbitMQ, IntegrationEventLogEF
   ClientApp
   eShop.AppHost ── Basket.API, Catalog.API, Identity.API, Ordering.API, OrderProcessor, PaymentProcessor, WebApp, WebhookClient, Webhooks.API
   HybridApp ── WebAppComponents

ENTRY POINTS
   HTTP (43)
      DELETE /api/catalog/items/{id:int}  → CatalogContext.SaveChangesAsync  (src/Catalog.API/Apis/CatalogApi.cs:107)
      DELETE /api/webhooks/{id:int}  → WebHooksApi  (src/Webhooks.API/Apis/WebHooksApi.cs:66)
      GET /Account  → AccountController  (src/Identity.API/Quickstart/Account/AccountController.cs:196)
      GET /Account  → AccountController  (src/Identity.API/Quickstart/Account/AccountController.cs:146)
      GET /Account  → AccountController  (src/Identity.API/Quickstart/Account/AccountController.cs:39)
      GET /api/catalog/catalogbrands  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:84)
      GET /api/catalog/catalogtypes  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:77)
      GET /api/catalog/items  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:26)
      GET /api/catalog/items  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:21)
      GET /api/catalog/items/{id:int}  → CatalogServices.Include  (src/Catalog.API/Apis/CatalogApi.cs:36)
      GET /api/catalog/items/{id:int}/pic  → CatalogContext.FindAsync  (src/Catalog.API/Apis/CatalogApi.cs:46)
      GET /api/catalog/items/by  → CatalogServices.Where  (src/Catalog.API/Apis/CatalogApi.cs:31)
      GET /api/catalog/items/by/{name:minlength(1)}  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:41)
      GET /api/catalog/items/type/{typeId}/brand/{brandId?}  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:67)
      GET /api/catalog/items/type/all/brand/{brandId:int?}  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:72)
      GET /api/catalog/items/withsemanticrelevance  → CatalogAI.GetEmbeddingAsync  (src/Catalog.API/Apis/CatalogApi.cs:60)
      GET /api/catalog/items/withsemanticrelevance/{text:minlength(1)}  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:53)
      GET /api/orders/  → OrdersApi  (src/Ordering.API/Apis/OrdersApi.cs:14)
      GET /api/orders/{orderId:int}  → OrdersApi  (src/Ordering.API/Apis/OrdersApi.cs:13)
      GET /api/orders/cardtypes  → OrdersApi  (src/Ordering.API/Apis/OrdersApi.cs:15)
      … and 23 more (http entries — use --focus for a drill-in)
   Bus (13)
      GracePeriodConfirmedIntegrationEventHandler  (src/Ordering.API/Application/IntegrationEvents/EventHandling/GracePeriodConfirmedIntegrationEventHandler.cs:3)
      OrderPaymentFailedIntegrationEventHandler  (src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderPaymentFailedIntegrationEventHandler.cs:3)
      OrderPaymentSucceededIntegrationEventHandler  (src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderPaymentSucceededIntegrationEventHandler.cs:3)
      OrderStartedIntegrationEventHandler  (src/Basket.API/IntegrationEvents/EventHandling/OrderStartedIntegrationEventHandler.cs:6)
      OrderStatusChangedToAwaitingValidationIntegrationEventHandler  (src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChangedToAwaitingValidationIntegrationEventHandler.cs:5)
      OrderStatusChangedToCancelledIntegrationEventHandler  (src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChangedToCancelledIntegrationEventHandler.cs:5)
      OrderStatusChangedToPaidIntegrationEventHandler  (src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChangedToPaidIntegrationEventHandler.cs:5)
      OrderStatusChangedToShippedIntegrationEventHandler  (src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChangedToShippedIntegrationEventHandler.cs:5)
      OrderStatusChangedToStockConfirmedIntegrationEventHandler  (src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChangedToStockConfirmedIntegrationEventHandler.cs:5)
      OrderStatusChangedToSubmittedIntegrationEventHandler  (src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChangedToSubmittedIntegrationEventHandler.cs:5)
      OrderStockConfirmedIntegrationEventHandler  (src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderStockConfirmedIntegrationEventHandler.cs:3)
      OrderStockRejectedIntegrationEventHandler  (src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderStockRejectedIntegrationEventHandler.cs:2)
      ProductPriceChangedIntegrationEventHandler  (src/Webhooks.API/IntegrationEvents/ProductPriceChangedIntegrationEventHandler.cs:3)
   Domain (7)
      OrderCancelledDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderCancelledDomainEventHandler.cs:3)
      OrderShippedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderShippedDomainEventHandler.cs:3)
      OrderStatusChangedToAwaitingValidationDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToAwaitingValidationDomainEventHandler.cs:3)
      OrderStatusChangedToPaidDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToPaidDomainEventHandler.cs:3)
      OrderStatusChangedToStockConfirmedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToStockConfirmedDomainEventHandler.cs:3)
      UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler.cs:3)
      ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler.cs:3)
   UI (24)
      [RelayCommand] BasketViewModel.AddAsync  → BasketViewModel  (src/ClientApp/ViewModels/BasketViewModel.cs:53)
      [RelayCommand] BasketViewModel.CheckoutAsync  → BasketViewModel  (src/ClientApp/ViewModels/BasketViewModel.cs:104)
      [RelayCommand] BasketViewModel.DeleteAsync  → BasketViewModel  (src/ClientApp/ViewModels/BasketViewModel.cs:74)
      [RelayCommand] CatalogItemViewModel.AddCatalogItemAsync  → CatalogItemViewModel  (src/ClientApp/ViewModels/CatalogItemViewModel.cs:30)
      [RelayCommand] CatalogItemViewModel.DismissAsync  → CatalogItemViewModel  (src/ClientApp/ViewModels/CatalogItemViewModel.cs:60)
      [RelayCommand] CatalogViewModel.ApplyFilterAsync  → CatalogViewModel  (src/ClientApp/ViewModels/CatalogViewModel.cs:161)
      [RelayCommand] CatalogViewModel.ClearFilterAsync  → CatalogViewModel  (src/ClientApp/ViewModels/CatalogViewModel.cs:178)
      [RelayCommand] CatalogViewModel.Filter  → CatalogViewModel  (src/ClientApp/ViewModels/CatalogViewModel.cs:105)
      [RelayCommand] CatalogViewModel.SelectCatalogBrand  → CatalogViewModel  (src/ClientApp/ViewModels/CatalogViewModel.cs:111)
      [RelayCommand] CatalogViewModel.SelectCatalogType  → CatalogViewModel  (src/ClientApp/ViewModels/CatalogViewModel.cs:136)
      [RelayCommand] CatalogViewModel.ViewBasket  → CatalogViewModel  (src/ClientApp/ViewModels/CatalogViewModel.cs:192)
      [RelayCommand] CatalogViewModel.ViewCatalogItemAsync  → CatalogViewModel  (src/ClientApp/ViewModels/CatalogViewModel.cs:90)
      [RelayCommand] CheckoutViewModel.CheckoutAsync  → CheckoutViewModel  (src/ClientApp/ViewModels/CheckoutViewModel.cs:104)
      [RelayCommand] LoginViewModel.MockSignInAsync  → LoginViewModel  (src/ClientApp/ViewModels/LoginViewModel.cs:57)
      [RelayCommand] LoginViewModel.PerformLogoutAsync  → LoginViewModel  (src/ClientApp/ViewModels/LoginViewModel.cs:104)
      [RelayCommand] LoginViewModel.RegisterAsync  → LoginViewModel  (src/ClientApp/ViewModels/LoginViewModel.cs:98)
      [RelayCommand] LoginViewModel.SettingsAsync  → LoginViewModel  (src/ClientApp/ViewModels/LoginViewModel.cs:115)
      [RelayCommand] LoginViewModel.SignInAsync  → LoginViewModel  (src/ClientApp/ViewModels/LoginViewModel.cs:83)
      [RelayCommand] LoginViewModel.Validate  → LoginViewModel  (src/ClientApp/ViewModels/LoginViewModel.cs:121)
      [RelayCommand] MainViewModel.SettingsAsync  → MainViewModel  (src/ClientApp/ViewModels/MainViewModel.cs:13)
      … and 4 more (ui entries — use --focus for a drill-in)

CROSS-CUTTING
   MediatR pipeline (every command):  LoggingBehavior → TransactionBehavior → ValidatorBehavior
   Aggregates:   Buyer · Order

PACKAGES
   Web/API:  Duende.IdentityServer.AspNetIdentity, Grpc.AspNetCore, Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.AspNetCore.Authentication.OpenIdConnect, Microsoft.AspNetCore.Components.QuickGrid, Microsoft.AspNetCore.Components.Web, Microsoft.AspNetCore.Components.WebView.Maui 9.0.30, Microsoft.AspNetCore.Identity.EntityFrameworkCore … (14 total)
   ORM/Data:  Aspire.Npgsql, Aspire.Npgsql.EntityFrameworkCore.PostgreSQL, Duende.IdentityServer.EntityFramework, Microsoft.EntityFrameworkCore.Tools, Npgsql.EntityFrameworkCore.PostgreSQL, Pgvector.EntityFrameworkCore
   Mediator/CQRS:  MediatR
   Messaging:  Aspire.Hosting.RabbitMQ, Aspire.RabbitMQ.Client
   Validation:  FluentValidation, FluentValidation.DependencyInjectionExtensions
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol, OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.GrpcNetClient, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.Runtime
   Testing:  NSubstitute, NSubstitute.Analyzers.CSharp, xunit.v3.mtp-v2
   Cloud:  Aspire.Azure.AI.OpenAI, Aspire.Hosting.Azure.CognitiveServices
   Other:  Asp.Versioning.Http, Asp.Versioning.Http.Client, Asp.Versioning.Mvc.ApiExplorer, Asp.Versioning.OpenApi, Aspire.Hosting.PostgreSQL, Aspire.Hosting.Redis, Aspire.Hosting.Yarp, Aspire.StackExchange.Redis … (35 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
