MAP  eShop     (19 projects)
SCOPE  analyzed eShop.slnx - 1 of 3 solutions in this repo; analyze another by 
naming its solution - style/topology are local to this slice, not the whole 
system

STACK  net10.0, net10.0-android +2 more TFMs ú Minimal APIs ú Controllers ú 
MediatR (CQRS) ú EF Core ú FluentValidation ú DDD aggregates

STYLE  Microservices  (confidence high)
       evidence: Aspire orchestration of 9 runnable services

       per service:
         Basket.API: gRPC Service [gRPC]
         Catalog.API: Web API [EF Core]
         ClientApp: MAUI App [.NET MAUI]
         eShop.AppHost: Aspire AppHost [Aspire]
         HybridApp: MAUI App [.NET MAUI]
         Identity.API: Identity provider [IdentityServer]
         Ordering.API: Web API [EF Core, FluentValidation]
         OrderProcessor: Worker Service [Worker]
         PaymentProcessor: Worker Service [Worker]
         WebApp: Blazor [Blazor, YARP]
         WebhookClient: Blazor [Blazor]
         Webhooks.API: Web API [EF Core]

TOPOLOGY (depends-on)
   eShop.ServiceDefaults
   EventBusRabbitMQ ÄÄ EventBus
   IntegrationEventLogEF ÄÄ EventBus
   EventBus
   Ordering.Domain
   WebAppComponents
   Basket.API ÄÄ eShop.ServiceDefaults, EventBusRabbitMQ
   Catalog.API ÄÄ eShop.ServiceDefaults, EventBusRabbitMQ, IntegrationEventLogEF
   Identity.API ÄÄ eShop.ServiceDefaults
   Ordering.API ÄÄ eShop.ServiceDefaults, EventBusRabbitMQ, 
IntegrationEventLogEF, Ordering.Domain, Ordering.Infrastructure
   Ordering.Infrastructure ÄÄ IntegrationEventLogEF, Ordering.Domain
   OrderProcessor ÄÄ eShop.ServiceDefaults, EventBusRabbitMQ
   PaymentProcessor ÄÄ eShop.ServiceDefaults, EventBusRabbitMQ
   WebApp ÄÄ eShop.ServiceDefaults, EventBusRabbitMQ, WebAppComponents
   WebhookClient ÄÄ eShop.ServiceDefaults
   Webhooks.API ÄÄ eShop.ServiceDefaults, EventBusRabbitMQ, 
IntegrationEventLogEF
   ClientApp
   eShop.AppHost ÄÄ Basket.API, Catalog.API, Identity.API, Ordering.API, 
OrderProcessor, PaymentProcessor, WebApp, WebhookClient, Webhooks.API
   HybridApp ÄÄ WebAppComponents

CROSS-SERVICE
  apphost reference (10)
  bus (9)
  gRPC (1)
  http/direct (3)
    [apphost] Basket.API  Identity.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\eShop.AppHost\Program.cs:30)
    [apphost] Ordering.API  Identity.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\eShop.AppHost\Program.cs:40)
    [apphost] Webhooks.API  Identity.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\eShop.AppHost\Program.cs:54)
    [apphost] WebhookClient  Identity.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\eShop.AppHost\Program.cs:65)
    [apphost] WebApp  Identity.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\eShop.AppHost\Program.cs:69)
    [apphost] Identity.API  Basket.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\eShop.AppHost\Program.cs:97)
    [apphost] Identity.API  Ordering.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\eShop.AppHost\Program.cs:97)
    [apphost] Identity.API  WebApp  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\eShop.AppHost\Program.cs:97)
    [apphost] Identity.API  Webhooks.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\eShop.AppHost\Program.cs:97)
    [apphost] Identity.API  WebhookClient  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\eShop.AppHost\Program.cs:97)
    [bus] Catalog.API  Webhooks.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\Catalog.API\Apis\CatalogApi.cs:
350 raises ProductPriceChangedIntegrationEvent)
    [bus] Catalog.API  Ordering.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\Catalog.API\IntegrationEvents\E
ventHandling\OrderStatusChangedToAwaitingValidationIntegrationEventHandler.cs:29
raises OrderStockConfirmedIntegrationEvent)
    [bus] Ordering.API  Basket.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\Ordering.API\Application\Comman
ds\CreateOrderCommandHandler.cs:32 raises OrderStartedIntegrationEvent)
    [bus] Ordering.API  Catalog.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\Ordering.API\Application\Domain
EventHandlers\OrderStatusChangedToAwaitingValidationDomainEventHandler.cs:33 
raises OrderStatusChangedToAwaitingValidationIntegrationEvent)
    [bus] Ordering.API  WebApp  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\Ordering.API\Application\Domain
EventHandlers\OrderStatusChangedToAwaitingValidationDomainEventHandler.cs:33 
raises OrderStatusChangedToAwaitingValidationIntegrationEvent)
    [bus] Ordering.API  Webhooks.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\Ordering.API\Application\Domain
EventHandlers\OrderStatusChangedToPaidDomainEventHandler.cs:32 raises 
OrderStatusChangedToPaidIntegrationEvent)
    [bus] Ordering.API  PaymentProcessor  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\Ordering.API\Application\Domain
EventHandlers\OrderStatusChangedToStockConfirmedDomainEventHandler.cs:30 raises 
OrderStatusChangedToStockConfirmedIntegrationEvent)
    [bus] OrderProcessor  Ordering.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\OrderProcessor\Services\GracePe
riodManagerService.cs:55 raises GracePeriodConfirmedIntegrationEvent)
    [bus] PaymentProcessor  Ordering.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\PaymentProcessor\IntegrationEve
nts\EventHandling\OrderStatusChangedToStockConfirmedIntegrationEventHandler.cs:2
7 raises OrderPaymentFailedIntegrationEvent)
    [gRPC] WebApp  Basket.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\WebApp\Services\BasketService.c
s:7C:\Code\DevContext2-engine\eval-repos\eShop\src\Basket.API\Grpc\BasketServic
e.cs:8)
    [http] WebApp  Catalog.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\WebApp\Extensions\Extensions.cs
:34)
    [http] WebApp  Ordering.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\WebApp\Extensions\Extensions.cs
:38)
    [http] WebhookClient  Webhooks.API  
(C:\Code\DevContext2-engine\eval-repos\eShop\src\WebhookClient\Extensions\Extens
ions.cs:19)

EVENT WIRING  (13 integration events, 13 cross-service)
  GracePeriodConfirmedIntegrationEvent: OrderProcessor  Ordering.API
  OrderPaymentFailedIntegrationEvent: PaymentProcessor  Ordering.API
  OrderPaymentSucceededIntegrationEvent: PaymentProcessor  Ordering.API
  OrderStartedIntegrationEvent: Ordering.API  Basket.API
  OrderStatusChangedToAwaitingValidationIntegrationEvent: Ordering.API  
Catalog.API, WebApp
  OrderStatusChangedToCancelledIntegrationEvent: Ordering.API  WebApp
  OrderStatusChangedToPaidIntegrationEvent: Ordering.API  Webhooks.API, 
Catalog.API, WebApp
  OrderStatusChangedToShippedIntegrationEvent: Ordering.API  Webhooks.API, 
WebApp
  OrderStatusChangedToStockConfirmedIntegrationEvent: Ordering.API  
PaymentProcessor, WebApp
  OrderStatusChangedToSubmittedIntegrationEvent: Ordering.API  WebApp
  OrderStockConfirmedIntegrationEvent: Catalog.API  Ordering.API
  OrderStockRejectedIntegrationEvent: Catalog.API  Ordering.API
  ProductPriceChangedIntegrationEvent: Catalog.API  Webhooks.API

ENTRY POINTS
   HTTP (43)
      DELETE /api/catalog/items/{id:int}   CatalogContext  
(src/Catalog.API/Apis/CatalogApi.cs:107)
      DELETE /api/webhooks/{id:int}   WebhooksContext  
(src/Webhooks.API/Apis/WebHooksApi.cs:66)
      GET /Account   AccountController  
(src/Identity.API/Quickstart/Account/AccountController.cs:39)
      GET /Account [AccessDenied]   AccountController  
(src/Identity.API/Quickstart/Account/AccountController.cs:196)
      GET /Account [Logout]   AccountController  
(src/Identity.API/Quickstart/Account/AccountController.cs:146)
      GET /api/catalog/catalogbrands   CatalogContext  
(src/Catalog.API/Apis/CatalogApi.cs:84)
      GET /api/catalog/catalogtypes   CatalogContext  
(src/Catalog.API/Apis/CatalogApi.cs:77)
      GET /api/catalog/items   CatalogApi  
(src/Catalog.API/Apis/CatalogApi.cs:21)
      GET /api/catalog/items [GetAllItems]   CatalogApi  
(src/Catalog.API/Apis/CatalogApi.cs:26)
      GET /api/catalog/items/{id:int}   CatalogServices  
(src/Catalog.API/Apis/CatalogApi.cs:36)
      GET /api/catalog/items/{id:int}/pic   CatalogContext  
(src/Catalog.API/Apis/CatalogApi.cs:46)
      GET /api/catalog/items/by   CatalogServices  
(src/Catalog.API/Apis/CatalogApi.cs:31)
      GET /api/catalog/items/by/{name:minlength(1)}   CatalogApi  
(src/Catalog.API/Apis/CatalogApi.cs:41)
      GET /api/catalog/items/type/{typeId}/brand/{brandId?}   CatalogApi  
(src/Catalog.API/Apis/CatalogApi.cs:67)
      GET /api/catalog/items/type/all/brand/{brandId:int?}   CatalogApi  
(src/Catalog.API/Apis/CatalogApi.cs:72)
      GET /api/catalog/items/withsemanticrelevance   
CatalogAI.GetEmbeddingAsync  (src/Catalog.API/Apis/CatalogApi.cs:60)
      GET /api/catalog/items/withsemanticrelevance/{text:minlength(1)}   
CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:53)
      GET /api/orders/   OrderQueries.GetOrdersFromUserAsync  
(src/Ordering.API/Apis/OrdersApi.cs:14)
      GET /api/orders/{orderId:int}   OrderQueries.GetOrderAsync  
(src/Ordering.API/Apis/OrdersApi.cs:13)
      GET /api/orders/cardtypes   OrderQueries.GetCardTypesAsync  
(src/Ordering.API/Apis/OrdersApi.cs:15)
      . and 23 more (http entries - trace one for a drill-in)
   Bus (13)
      GracePeriodConfirmedIntegrationEventHandler  
(src/Ordering.API/Application/IntegrationEvents/EventHandling/GracePeriodConfirm
edIntegrationEventHandler.cs:3)
      OrderPaymentFailedIntegrationEventHandler  
(src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderPaymentFailed
IntegrationEventHandler.cs:3)
      OrderPaymentSucceededIntegrationEventHandler  
(src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderPaymentSuccee
dedIntegrationEventHandler.cs:3)
      OrderStartedIntegrationEventHandler  
(src/Basket.API/IntegrationEvents/EventHandling/OrderStartedIntegrationEventHand
ler.cs:6)
      OrderStatusChangedToAwaitingValidationIntegrationEventHandler  
(src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToAwaitingVal
idationIntegrationEventHandler.cs:3)
      OrderStatusChangedToCancelledIntegrationEventHandler  
(src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChan
gedToCancelledIntegrationEventHandler.cs:5)
      OrderStatusChangedToPaidIntegrationEventHandler  
(src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToPaidIntegra
tionEventHandler.cs:3)
      OrderStatusChangedToShippedIntegrationEventHandler  
(src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChan
gedToShippedIntegrationEventHandler.cs:5)
      OrderStatusChangedToStockConfirmedIntegrationEventHandler  
(src/PaymentProcessor/IntegrationEvents/EventHandling/OrderStatusChangedToStockC
onfirmedIntegrationEventHandler.cs:3)
      OrderStatusChangedToSubmittedIntegrationEventHandler  
(src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChan
gedToSubmittedIntegrationEventHandler.cs:5)
      OrderStockConfirmedIntegrationEventHandler  
(src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderStockConfirme
dIntegrationEventHandler.cs:3)
      OrderStockRejectedIntegrationEventHandler  
(src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderStockRejected
IntegrationEventHandler.cs:2)
      ProductPriceChangedIntegrationEventHandler  
(src/Webhooks.API/IntegrationEvents/ProductPriceChangedIntegrationEventHandler.c
s:3)
   Background (2)
      GracePeriodManagerService  
(src/OrderProcessor/Extensions/Extensions.cs:18)
      RabbitMQEventBus  (src/EventBusRabbitMQ/RabbitMQEventBus.cs:13)
   Domain (7)
      OrderCancelledDomainEventHandler  
(src/Ordering.API/Application/DomainEventHandlers/OrderCancelledDomainEventHandl
er.cs:3)
      OrderShippedDomainEventHandler  
(src/Ordering.API/Application/DomainEventHandlers/OrderShippedDomainEventHandler
.cs:3)
      OrderStatusChangedToAwaitingValidationDomainEventHandler  
(src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToAwaitingVa
lidationDomainEventHandler.cs:3)
      OrderStatusChangedToPaidDomainEventHandler  
(src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToPaidDomain
EventHandler.cs:3)
      OrderStatusChangedToStockConfirmedDomainEventHandler  
(src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToStockConfi
rmedDomainEventHandler.cs:3)
      UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler  
(src/Ordering.API/Application/DomainEventHandlers/UpdateOrderWhenBuyerAndPayment
MethodVerifiedDomainEventHandler.cs:3)
      ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler  
(src/Ordering.API/Application/DomainEventHandlers/ValidateOrAddBuyerAggregateWhe
nOrderStartedDomainEventHandler.cs:3)
   UI (42)
      [RelayCommand] BasketViewModel.CheckoutAsync   
MauiNavigationService.NavigateToAsync  
(src/ClientApp/ViewModels/BasketViewModel.cs:104)
      [RelayCommand] BasketViewModel.DeleteAsync   ObservableCollectionEx  
(src/ClientApp/ViewModels/BasketViewModel.cs:74)
      [RelayCommand] CatalogItemViewModel.AddCatalogItemAsync   
MauiNavigationService.PopAsync  
(src/ClientApp/ViewModels/CatalogItemViewModel.cs:30)
      [RelayCommand] CatalogItemViewModel.DismissAsync   
MauiNavigationService.PopAsync  
(src/ClientApp/ViewModels/CatalogItemViewModel.cs:60)
      [RelayCommand] CatalogViewModel.ApplyFilterAsync   
ICatalogService.FilterAsync  (src/ClientApp/ViewModels/CatalogViewModel.cs:161)
      [RelayCommand] CatalogViewModel.ClearFilterAsync   
ICatalogService.GetCatalogAsync  
(src/ClientApp/ViewModels/CatalogViewModel.cs:178)
      [RelayCommand] CatalogViewModel.ViewBasket   
MauiNavigationService.NavigateToAsync  
(src/ClientApp/ViewModels/CatalogViewModel.cs:192)
      [RelayCommand] CatalogViewModel.ViewCatalogItemAsync   
MauiNavigationService.NavigateToAsync  
(src/ClientApp/ViewModels/CatalogViewModel.cs:90)
      [RelayCommand] CheckoutViewModel.CheckoutAsync   
MauiNavigationService.NavigateToAsync  
(src/ClientApp/ViewModels/CheckoutViewModel.cs:104)
      [RelayCommand] LoginViewModel.MockSignInAsync   
MauiNavigationService.NavigateToAsync  
(src/ClientApp/ViewModels/LoginViewModel.cs:57)
      [RelayCommand] LoginViewModel.PerformLogoutAsync   
IIdentityService.SignOutAsync  (src/ClientApp/ViewModels/LoginViewModel.cs:104)
      [RelayCommand] LoginViewModel.RegisterAsync   OpenUrlService.OpenUrl  
(src/ClientApp/ViewModels/LoginViewModel.cs:98)
      [RelayCommand] LoginViewModel.SettingsAsync   
MauiNavigationService.NavigateToAsync  
(src/ClientApp/ViewModels/LoginViewModel.cs:115)
      [RelayCommand] LoginViewModel.SignInAsync   
MauiNavigationService.NavigateToAsync  
(src/ClientApp/ViewModels/LoginViewModel.cs:83)
      [RelayCommand] MainViewModel.SettingsAsync   
MauiNavigationService.NavigateToAsync  
(src/ClientApp/ViewModels/MainViewModel.cs:13)
      [RelayCommand] OrderDetailViewModel.ToggleCancelOrderAsync   
IOrderService.CancelOrderAsync  
(src/ClientApp/ViewModels/OrderDetailViewModel.cs:43)
      [RelayCommand] ProfileViewModel.LogoutAsync   
MauiNavigationService.NavigateToAsync  
(src/ClientApp/ViewModels/ProfileViewModel.cs:35)
      [RelayCommand] ProfileViewModel.OrderDetailAsync   
MauiNavigationService.NavigateToAsync  
(src/ClientApp/ViewModels/ProfileViewModel.cs:66)
      [RelayCommand] ProfileViewModel.RefreshAsync   
IOrderService.GetOrdersAsync  (src/ClientApp/ViewModels/ProfileViewModel.cs:48)
      GET /   CatalogService.GetCatalogItems  
(src/HybridApp/Components/Pages/Catalog/Catalog.razor:1)
      . and 22 more (ui entries - trace one for a drill-in)
   gRPC (3)
      Basket.DeleteBasket   RedisBasketRepository.DeleteBasketAsync  
(src/Basket.API/Grpc/BasketService.cs:8)
      Basket.GetBasket   RedisBasketRepository.GetBasketAsync  
(src/Basket.API/Grpc/BasketService.cs:8)
      Basket.UpdateBasket   RedisBasketRepository.UpdateBasketAsync  
(src/Basket.API/Grpc/BasketService.cs:8)

CROSS-CUTTING
   MediatR pipeline (every command):  LoggingBehavior  TransactionBehavior  
ValidatorBehavior
   Aggregates:   Buyer ú Order

PACKAGES
   Web/API:  Duende.IdentityServer.AspNetIdentity 7.3.2, Grpc.AspNetCore, 
Microsoft.AspNetCore.Authentication.JwtBearer, 
Microsoft.AspNetCore.Authentication.OpenIdConnect, 
Microsoft.AspNetCore.Components.QuickGrid, Microsoft.AspNetCore.Components.Web, 
Microsoft.AspNetCore.Components.WebView.Maui 9.0.30, 
Microsoft.AspNetCore.Identity.EntityFrameworkCore . (14 total)
   ORM/Data:  Aspire.Npgsql, Aspire.Npgsql.EntityFrameworkCore.PostgreSQL, 
Duende.IdentityServer.EntityFramework 7.3.2, 
Microsoft.EntityFrameworkCore.Tools, Npgsql.EntityFrameworkCore.PostgreSQL 
10.0.1, Pgvector.EntityFrameworkCore 0.3.0
   Mediator/CQRS:  MediatR 13.0.0
   Messaging:  Aspire.Hosting.RabbitMQ, Aspire.RabbitMQ.Client
   Validation:  FluentValidation 12.0.0, 
FluentValidation.DependencyInjectionExtensions 12.0.0
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol 1.15.0, 
OpenTelemetry.Extensions.Hosting 1.15.0, 
OpenTelemetry.Instrumentation.GrpcNetClient 1.15.0-beta.1, 
OpenTelemetry.Instrumentation.Http 1.15.0, OpenTelemetry.Instrumentation.Runtime
1.15.0
   Testing:  NSubstitute 5.3.0, NSubstitute.Analyzers.CSharp 1.0.17, 
xunit.v3.mtp-v2 3.2.1
   Cloud:  Aspire.Azure.AI.OpenAI, Aspire.Hosting.Azure.CognitiveServices
   Other:  Asp.Versioning.Http, Asp.Versioning.Http.Client, 
Asp.Versioning.Mvc.ApiExplorer, Asp.Versioning.OpenApi, 
Aspire.Hosting.PostgreSQL, Aspire.Hosting.Redis, Aspire.Hosting.Yarp, 
Aspire.StackExchange.Redis . (35 total)

 drill in:  trace a focused entry   (e.g. trace "PUT /api/catalog/items")

