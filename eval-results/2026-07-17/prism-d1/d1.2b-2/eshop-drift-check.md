MAP  eShop     (19 projects)

STACK  net10.0, net10.0-android;net10.0-ios;net10.0-maccatalyst, net10.0-android;net10.0-ios;net10.0-maccatalyst;net10.0 · Minimal APIs · Controllers · MediatR (CQRS) · EF Core · FluentValidation · DDD aggregates

STYLE  Microservices  (confidence high)
       evidence: Aspire orchestration of 11 runnable services

       per service:
         Basket.API: gRPC Service [gRPC]
         Catalog.API: Web API [EF Core]
         ClientApp: MAUI App [.NET MAUI]
         eShop.AppHost: Aspire AppHost [Aspire]
         HybridApp: MAUI App [.NET MAUI]
         Identity.API: Web API [EF Core]
         Ordering.API: Web API [EF Core, FluentValidation]
         OrderProcessor: Worker Service [Worker]
         PaymentProcessor: Worker Service [Worker]
         WebApp: Blazor [Blazor, YARP]
         WebhookClient: Blazor [Blazor]
         Webhooks.API: Web API [EF Core]

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

CROSS-SERVICE
  bus (5)
    [bus] Catalog.API → Webhooks.API  (C:\code\DevContext2\eval-repos\eShop\src\Catalog.API\Apis\CatalogApi.cs:350 raises ProductPriceChangedIntegrationEvent)
    [bus] Catalog.API → Ordering.API  (C:\code\DevContext2\eval-repos\eShop\src\Catalog.API\IntegrationEvents\EventHandling\OrderStatusChangedToAwaitingValidationIntegrationEventHandler.cs:29 raises OrderStockConfirmedIntegrationEvent)
    [bus] Ordering.API → Basket.API  (C:\code\DevContext2\eval-repos\eShop\src\Ordering.API\Application\Commands\CreateOrderCommandHandler.cs:32 raises OrderStartedIntegrationEvent)
    [bus] Ordering.API → WebApp  (C:\code\DevContext2\eval-repos\eShop\src\Ordering.API\Application\DomainEventHandlers\OrderCancelledDomainEventHandler.cs:30 raises OrderStatusChangedToCancelledIntegrationEvent)
    [bus] PaymentProcessor → Ordering.API  (C:\code\DevContext2\eval-repos\eShop\src\PaymentProcessor\IntegrationEvents\EventHandling\OrderStatusChangedToStockConfirmedIntegrationEventHandler.cs:27 raises OrderPaymentFailedIntegrationEvent)

EVENT WIRING  (13 integration events, 8 cross-service)
  OrderPaymentFailedIntegrationEvent: PaymentProcessor → Ordering.API
  OrderPaymentSucceededIntegrationEvent: PaymentProcessor → Ordering.API
  OrderStartedIntegrationEvent: Ordering.API → Basket.API
  OrderStatusChangedToCancelledIntegrationEvent: Ordering.API → WebApp
  OrderStatusChangedToSubmittedIntegrationEvent: Ordering.API → WebApp
  OrderStockConfirmedIntegrationEvent: Catalog.API → Ordering.API
  OrderStockRejectedIntegrationEvent: Catalog.API → Ordering.API
  ProductPriceChangedIntegrationEvent: Catalog.API → Webhooks.API
  GracePeriodConfirmedIntegrationEvent: (external) · Ordering.API
  OrderStatusChangedToAwaitingValidationIntegrationEvent: (external) · WebApp, Catalog.API
  OrderStatusChangedToPaidIntegrationEvent: (external) · Catalog.API, Webhooks.API, WebApp
  OrderStatusChangedToShippedIntegrationEvent: (external) · WebApp, Webhooks.API
  OrderStatusChangedToStockConfirmedIntegrationEvent: (external) · PaymentProcessor, WebApp

ENTRY POINTS
   HTTP (43)
      DELETE /api/catalog/items/{id:int}  → CatalogServices  (src/Catalog.API/Apis/CatalogApi.cs:107)
      DELETE /api/webhooks/{id:int}  → WebhooksContext  (src/Webhooks.API/Apis/WebHooksApi.cs:66)
      GET /Account  → AccountController  (src/Identity.API/Quickstart/Account/AccountController.cs:196)
      GET /Account [Login]  → AccountController  (src/Identity.API/Quickstart/Account/AccountController.cs:39)
      GET /Account [Logout]  → AccountController  (src/Identity.API/Quickstart/Account/AccountController.cs:146)
      GET /api/catalog/catalogbrands  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:84)
      GET /api/catalog/catalogtypes  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:77)
      GET /api/catalog/items  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:26)
      GET /api/catalog/items [GetAllItemsV1]  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:21)
      GET /api/catalog/items/{id:int}  → CatalogServices  (src/Catalog.API/Apis/CatalogApi.cs:36)
      GET /api/catalog/items/{id:int}/pic  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:46)
      GET /api/catalog/items/by  → CatalogServices  (src/Catalog.API/Apis/CatalogApi.cs:31)
      GET /api/catalog/items/by/{name:minlength(1)}  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:41)
      GET /api/catalog/items/type/{typeId}/brand/{brandId?}  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:67)
      GET /api/catalog/items/type/all/brand/{brandId:int?}  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:72)
      GET /api/catalog/items/withsemanticrelevance  → CatalogAI.GetEmbeddingAsync  (src/Catalog.API/Apis/CatalogApi.cs:60)
      GET /api/catalog/items/withsemanticrelevance/{text:minlength(1)}  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:53)
      GET /api/orders/  → OrderServices  (src/Ordering.API/Apis/OrdersApi.cs:14)
      GET /api/orders/{orderId:int}  → OrderServices  (src/Ordering.API/Apis/OrdersApi.cs:13)
      GET /api/orders/cardtypes  → IOrderQueries  (src/Ordering.API/Apis/OrdersApi.cs:15)
      … and 23 more (http entries — use --focus for a drill-in)
   Bus (13)
      GracePeriodConfirmedIntegrationEventHandler  → GracePeriodConfirmedIntegrationEventHandler  (src/Ordering.API/Application/IntegrationEvents/EventHandling/GracePeriodConfirmedIntegrationEventHandler.cs:3)
      OrderPaymentFailedIntegrationEventHandler  → OrderPaymentFailedIntegrationEventHandler  (src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderPaymentFailedIntegrationEventHandler.cs:3)
      OrderPaymentSucceededIntegrationEventHandler  → OrderPaymentSucceededIntegrationEventHandler  (src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderPaymentSucceededIntegrationEventHandler.cs:3)
      OrderStartedIntegrationEventHandler  → OrderStartedIntegrationEventHandler  (src/Basket.API/IntegrationEvents/EventHandling/OrderStartedIntegrationEventHandler.cs:6)
      OrderStatusChangedToAwaitingValidationIntegrationEventHandler  → OrderStatusChangedToAwaitingValidationIntegrationEventHandler  (src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChangedToAwaitingValidationIntegrationEventHandler.cs:5)
      OrderStatusChangedToCancelledIntegrationEventHandler  → OrderStatusChangedToCancelledIntegrationEventHandler  (src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChangedToCancelledIntegrationEventHandler.cs:5)
      OrderStatusChangedToPaidIntegrationEventHandler  → OrderStatusChangedToPaidIntegrationEventHandler  (src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChangedToPaidIntegrationEventHandler.cs:5)
      OrderStatusChangedToShippedIntegrationEventHandler  → OrderStatusChangedToShippedIntegrationEventHandler  (src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChangedToShippedIntegrationEventHandler.cs:5)
      OrderStatusChangedToStockConfirmedIntegrationEventHandler  → OrderStatusChangedToStockConfirmedIntegrationEventHandler  (src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChangedToStockConfirmedIntegrationEventHandler.cs:5)
      OrderStatusChangedToSubmittedIntegrationEventHandler  → OrderStatusChangedToSubmittedIntegrationEventHandler  (src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChangedToSubmittedIntegrationEventHandler.cs:5)
      OrderStockConfirmedIntegrationEventHandler  → OrderStockConfirmedIntegrationEventHandler  (src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderStockConfirmedIntegrationEventHandler.cs:3)
      OrderStockRejectedIntegrationEventHandler  → OrderStockRejectedIntegrationEventHandler  (src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderStockRejectedIntegrationEventHandler.cs:2)
      ProductPriceChangedIntegrationEventHandler  → ProductPriceChangedIntegrationEventHandler  (src/Webhooks.API/IntegrationEvents/ProductPriceChangedIntegrationEventHandler.cs:3)
   Background (1)
      GracePeriodManagerService  → GracePeriodManagerService  (src/OrderProcessor/Extensions/Extensions.cs:18)
   Domain (7)
      OrderCancelledDomainEventHandler  → OrderCancelledDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderCancelledDomainEventHandler.cs:3)
      OrderShippedDomainEventHandler  → OrderShippedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderShippedDomainEventHandler.cs:3)
      OrderStatusChangedToAwaitingValidationDomainEventHandler  → OrderStatusChangedToAwaitingValidationDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToAwaitingValidationDomainEventHandler.cs:3)
      OrderStatusChangedToPaidDomainEventHandler  → OrderStatusChangedToPaidDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToPaidDomainEventHandler.cs:3)
      OrderStatusChangedToStockConfirmedDomainEventHandler  → OrderStatusChangedToStockConfirmedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToStockConfirmedDomainEventHandler.cs:3)
      UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler  → UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler.cs:3)
      ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler  → ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler.cs:3)
   UI (42)
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
      … and 22 more (ui entries — use --focus for a drill-in)
   gRPC (3)
      Basket.DeleteBasket  → RedisBasketRepository.DeleteBasketAsync  (src/Basket.API/Grpc/BasketService.cs:8)
      Basket.GetBasket  → RedisBasketRepository.GetBasketAsync  (src/Basket.API/Grpc/BasketService.cs:8)
      Basket.UpdateBasket  → RedisBasketRepository.UpdateBasketAsync  (src/Basket.API/Grpc/BasketService.cs:8)

CROSS-CUTTING
   MediatR pipeline (every command):  LoggingBehavior → TransactionBehavior → ValidatorBehavior
   Aggregates:   Buyer · Order

PACKAGES
   Web/API:  Duende.IdentityServer.AspNetIdentity 7.3.2, Grpc.AspNetCore, Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.AspNetCore.Authentication.OpenIdConnect, Microsoft.AspNetCore.Components.QuickGrid, Microsoft.AspNetCore.Components.Web, Microsoft.AspNetCore.Components.WebView.Maui 9.0.30, Microsoft.AspNetCore.Identity.EntityFrameworkCore … (14 total)
   ORM/Data:  Aspire.Npgsql, Aspire.Npgsql.EntityFrameworkCore.PostgreSQL, Duende.IdentityServer.EntityFramework 7.3.2, Microsoft.EntityFrameworkCore.Tools, Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1, Pgvector.EntityFrameworkCore 0.3.0
   Mediator/CQRS:  MediatR 13.0.0
   Messaging:  Aspire.Hosting.RabbitMQ, Aspire.RabbitMQ.Client
   Validation:  FluentValidation 12.0.0, FluentValidation.DependencyInjectionExtensions 12.0.0
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol 1.15.0, OpenTelemetry.Extensions.Hosting 1.15.0, OpenTelemetry.Instrumentation.GrpcNetClient 1.15.0-beta.1, OpenTelemetry.Instrumentation.Http 1.15.0, OpenTelemetry.Instrumentation.Runtime 1.15.0
   Testing:  NSubstitute 5.3.0, NSubstitute.Analyzers.CSharp 1.0.17, xunit.v3.mtp-v2 3.2.1
   Cloud:  Aspire.Azure.AI.OpenAI, Aspire.Hosting.Azure.CognitiveServices
   Other:  Asp.Versioning.Http, Asp.Versioning.Http.Client, Asp.Versioning.Mvc.ApiExplorer, Asp.Versioning.OpenApi, Aspire.Hosting.PostgreSQL, Aspire.Hosting.Redis, Aspire.Hosting.Yarp, Aspire.StackExchange.Redis … (35 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "PUT /api/catalog/items")
