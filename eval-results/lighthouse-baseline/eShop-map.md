Overview map (no focus).
Analyzing project...

MAP  eShop     (19 projects)

STACK  net10.0, net10.0-android;net10.0-ios;net10.0-maccatalyst, 
net10.0-android;net10.0-ios;net10.0-maccatalyst;net10.0 ú Minimal APIs ú 
Controllers ú MediatR (CQRS) ú EF Core ú FluentValidation ú DDD aggregates

STYLE  Microservices  (confidence high)
       evidence: Aspire orchestration with 22 service projects

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

ENTRY POINTS
   HTTP (56)
      DELETE /api/catalog/items/{id:int}   CatalogContext.SaveChangesAsync  
(src/Catalog.API/Apis/CatalogApi.cs:107)
      DELETE /api/webhooks/{id:int}   WebHooksApi  
(src/Webhooks.API/Apis/WebHooksApi.cs:66)
      GET /Account   AccountController  
(src/Identity.API/Quickstart/Account/AccountController.cs:196)
      GET /Account   AccountController  
(src/Identity.API/Quickstart/Account/AccountController.cs:146)
      GET /Account   AccountController  
(src/Identity.API/Quickstart/Account/AccountController.cs:39)
      GET /api/catalog/catalogbrands   CatalogApi  
(src/Catalog.API/Apis/CatalogApi.cs:84)
      GET /api/catalog/catalogtypes   CatalogApi  
(src/Catalog.API/Apis/CatalogApi.cs:77)
      GET /api/catalog/items   CatalogApi  
(src/Catalog.API/Apis/CatalogApi.cs:26)
      GET /api/catalog/items   CatalogApi  
(src/Catalog.API/Apis/CatalogApi.cs:21)
      GET /api/catalog/items/{id:int}   CatalogServices.Include  
(src/Catalog.API/Apis/CatalogApi.cs:36)
      GET /api/catalog/items/{id:int}/pic   CatalogContext.FindAsync  
(src/Catalog.API/Apis/CatalogApi.cs:46)
      GET /api/catalog/items/by   CatalogServices.Where  
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
      GET /api/orders/   OrdersApi  (src/Ordering.API/Apis/OrdersApi.cs:14)
      GET /api/orders/{orderId:int}   OrdersApi  
(src/Ordering.API/Apis/OrdersApi.cs:13)
      GET /api/orders/cardtypes   OrdersApi  
(src/Ordering.API/Apis/OrdersApi.cs:15)
      . and 36 more (http entries - use --focus for a drill-in)
   Bus (13)
      GracePeriodConfirmedIntegrationEventHandler   
GracePeriodConfirmedIntegrationEventHandler  
(src/Ordering.API/Application/IntegrationEvents/EventHandling/GracePeriodConfirm
edIntegrationEventHandler.cs:3)
      OrderPaymentFailedIntegrationEventHandler   
OrderPaymentFailedIntegrationEventHandler  
(src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderPaymentFailed
IntegrationEventHandler.cs:3)
      OrderPaymentSucceededIntegrationEventHandler   
OrderPaymentSucceededIntegrationEventHandler  
(src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderPaymentSuccee
dedIntegrationEventHandler.cs:3)
      OrderStartedIntegrationEventHandler   OrderStartedIntegrationEventHandler
(src/Basket.API/IntegrationEvents/EventHandling/OrderStartedIntegrationEventHand
ler.cs:6)
      OrderStatusChangedToAwaitingValidationIntegrationEventHandler   
OrderStatusChangedToAwaitingValidationIntegrationEventHandler  
(src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChan
gedToAwaitingValidationIntegrationEventHandler.cs:5)
      OrderStatusChangedToCancelledIntegrationEventHandler   
OrderStatusChangedToCancelledIntegrationEventHandler  
(src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChan
gedToCancelledIntegrationEventHandler.cs:5)
      OrderStatusChangedToPaidIntegrationEventHandler   
OrderStatusChangedToPaidIntegrationEventHandler  
(src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChan
gedToPaidIntegrationEventHandler.cs:5)
      OrderStatusChangedToShippedIntegrationEventHandler   
OrderStatusChangedToShippedIntegrationEventHandler  
(src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChan
gedToShippedIntegrationEventHandler.cs:5)
      OrderStatusChangedToStockConfirmedIntegrationEventHandler   
OrderStatusChangedToStockConfirmedIntegrationEventHandler  
(src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChan
gedToStockConfirmedIntegrationEventHandler.cs:5)
      OrderStatusChangedToSubmittedIntegrationEventHandler   
OrderStatusChangedToSubmittedIntegrationEventHandler  
(src/WebApp/Services/OrderStatus/IntegrationEvents/EventHandling/OrderStatusChan
gedToSubmittedIntegrationEventHandler.cs:5)
      OrderStockConfirmedIntegrationEventHandler   
OrderStockConfirmedIntegrationEventHandler  
(src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderStockConfirme
dIntegrationEventHandler.cs:3)
      OrderStockRejectedIntegrationEventHandler   
OrderStockRejectedIntegrationEventHandler  
(src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderStockRejected
IntegrationEventHandler.cs:2)
      ProductPriceChangedIntegrationEventHandler   
ProductPriceChangedIntegrationEventHandler  
(src/Webhooks.API/IntegrationEvents/ProductPriceChangedIntegrationEventHandler.c
s:3)
   Domain (7)
      OrderCancelledDomainEventHandler   OrderCancelledDomainEventHandler  
(src/Ordering.API/Application/DomainEventHandlers/OrderCancelledDomainEventHandl
er.cs:3)
      OrderShippedDomainEventHandler   OrderShippedDomainEventHandler  
(src/Ordering.API/Application/DomainEventHandlers/OrderShippedDomainEventHandler
.cs:3)
      OrderStatusChangedToAwaitingValidationDomainEventHandler   
OrderStatusChangedToAwaitingValidationDomainEventHandler  
(src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToAwaitingVa
lidationDomainEventHandler.cs:3)
      OrderStatusChangedToPaidDomainEventHandler   
OrderStatusChangedToPaidDomainEventHandler  
(src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToPaidDomain
EventHandler.cs:3)
      OrderStatusChangedToStockConfirmedDomainEventHandler   
OrderStatusChangedToStockConfirmedDomainEventHandler  
(src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToStockConfi
rmedDomainEventHandler.cs:3)
      UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler   
UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler  
(src/Ordering.API/Application/DomainEventHandlers/UpdateOrderWhenBuyerAndPayment
MethodVerifiedDomainEventHandler.cs:3)
      ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler   
ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler  
(src/Ordering.API/Application/DomainEventHandlers/ValidateOrAddBuyerAggregateWhe
nOrderStartedDomainEventHandler.cs:3)
   UI (29)
      [RelayCommand] BasketViewModel.AddAsync   BasketViewModel  
(src/ClientApp/ViewModels/BasketViewModel.cs:53)
      [RelayCommand] BasketViewModel.CheckoutAsync   BasketViewModel  
(src/ClientApp/ViewModels/BasketViewModel.cs:104)
      [RelayCommand] BasketViewModel.DeleteAsync   BasketViewModel  
(src/ClientApp/ViewModels/BasketViewModel.cs:74)
      [RelayCommand] CatalogItemViewModel.AddCatalogItemAsync   
CatalogItemViewModel  (src/ClientApp/ViewModels/CatalogItemViewModel.cs:30)
      [RelayCommand] CatalogItemViewModel.DismissAsync   CatalogItemViewModel  
(src/ClientApp/ViewModels/CatalogItemViewModel.cs:60)
      [RelayCommand] CatalogViewModel.ApplyFilterAsync   CatalogViewModel  
(src/ClientApp/ViewModels/CatalogViewModel.cs:161)
      [RelayCommand] CatalogViewModel.ClearFilterAsync   CatalogViewModel  
(src/ClientApp/ViewModels/CatalogViewModel.cs:178)
      [RelayCommand] CatalogViewModel.Filter   CatalogViewModel  
(src/ClientApp/ViewModels/CatalogViewModel.cs:105)
      [RelayCommand] CatalogViewModel.SelectCatalogBrand   CatalogViewModel  
(src/ClientApp/ViewModels/CatalogViewModel.cs:111)
      [RelayCommand] CatalogViewModel.SelectCatalogType   CatalogViewModel  
(src/ClientApp/ViewModels/CatalogViewModel.cs:136)
      [RelayCommand] CatalogViewModel.ViewBasket   CatalogViewModel  
(src/ClientApp/ViewModels/CatalogViewModel.cs:192)
      [RelayCommand] CatalogViewModel.ViewCatalogItemAsync   CatalogViewModel  
(src/ClientApp/ViewModels/CatalogViewModel.cs:90)
      [RelayCommand] CheckoutViewModel.CheckoutAsync   CheckoutViewModel  
(src/ClientApp/ViewModels/CheckoutViewModel.cs:104)
      [RelayCommand] LoginViewModel.MockSignInAsync   LoginViewModel  
(src/ClientApp/ViewModels/LoginViewModel.cs:57)
      [RelayCommand] LoginViewModel.PerformLogoutAsync   LoginViewModel  
(src/ClientApp/ViewModels/LoginViewModel.cs:104)
      [RelayCommand] LoginViewModel.RegisterAsync   LoginViewModel  
(src/ClientApp/ViewModels/LoginViewModel.cs:98)
      [RelayCommand] LoginViewModel.SettingsAsync   LoginViewModel  
(src/ClientApp/ViewModels/LoginViewModel.cs:115)
      [RelayCommand] LoginViewModel.SignInAsync   LoginViewModel  
(src/ClientApp/ViewModels/LoginViewModel.cs:83)
      [RelayCommand] LoginViewModel.Validate   LoginViewModel  
(src/ClientApp/ViewModels/LoginViewModel.cs:121)
      [RelayCommand] MainViewModel.SettingsAsync   MainViewModel  
(src/ClientApp/ViewModels/MainViewModel.cs:13)
      . and 9 more (ui entries - use --focus for a drill-in)
   gRPC (16)
      Animation.FadeInAnimation (3 methods: BeginAnimation, ResetAnimation, 
FadeIn)   FadeInAnimation  (src/ClientApp/Animations/FadeToAnimation.cs:40)
      Animation.FadeOutAnimation (3 methods: BeginAnimation, ResetAnimation, 
FadeOut)   FadeOutAnimation  (src/ClientApp/Animations/FadeToAnimation.cs:98)
      Animation.FadeToAnimation (2 methods: BeginAnimation, ResetAnimation)   
FadeToAnimation  (src/ClientApp/Animations/FadeToAnimation.cs:6)
      Animation.StoryBoard (2 methods: BeginAnimation, ResetAnimation)   
StoryBoard  (src/ClientApp/Animations/StoryBoard.cs:5)
      Basket.BasketService (7 methods: GetBasket, UpdateBasket, DeleteBasket)  
BasketService  (src/Basket.API/Grpc/BasketService.cs:8)
      IViewModel.ViewModelBase (3 methods: ApplyQueryAttributes, 
InitializeAsync, IsBusyFor)   ViewModelBase  
(src/ClientApp/ViewModels/Base/ViewModelBase.cs:5)
      ViewModel.BasketViewModel (7 methods: InitializeAsync, AddAsync, 
AddBasketItemAsync)   BasketViewModel  
(src/ClientApp/ViewModels/BasketViewModel.cs:9)
      ViewModel.CatalogItemViewModel (3 methods: ApplyQueryAttributes, 
AddCatalogItemAsync, DismissAsync)   CatalogItemViewModel  
(src/ClientApp/ViewModels/CatalogItemViewModel.cs:11)
      ViewModel.CatalogViewModel (8 methods: InitializeAsync, 
ViewCatalogItemAsync, Filter)   CatalogViewModel  
(src/ClientApp/ViewModels/CatalogViewModel.cs:11)
      ViewModel.CheckoutViewModel (4 methods: InitializeAsync, CheckoutAsync, 
CreateOrderItems)   CheckoutViewModel  
(src/ClientApp/ViewModels/CheckoutViewModel.cs:13)
      ViewModel.LoginViewModel (10 methods: ApplyQueryAttributes, 
InitializeAsync, MockSignInAsync)   LoginViewModel  
(src/ClientApp/ViewModels/LoginViewModel.cs:11)
      ViewModel.MainViewModel (1 methods: SettingsAsync)   MainViewModel  
(src/ClientApp/ViewModels/MainViewModel.cs:6)
      ViewModel.MapViewModel (1 methods: InitializeAsync)   MapViewModel  
(src/ClientApp/ViewModels/MapViewModel.cs:6)
      ViewModel.OrderDetailViewModel (3 methods: InitializeAsync, 
ToggleCancelOrderAsync, ApplyQueryAttributes)   OrderDetailViewModel  
(src/ClientApp/ViewModels/OrderDetailViewModel.cs:9)
      ViewModel.ProfileViewModel (4 methods: InitializeAsync, LogoutAsync, 
RefreshAsync)   ProfileViewModel  
(src/ClientApp/ViewModels/ProfileViewModel.cs:9)
      ViewModel.SettingsViewModel (14 methods: OnPropertyChanged, 
ToggleMockServices, ToggleFakeLocation)   SettingsViewModel  
(src/ClientApp/ViewModels/SettingsViewModel.cs:13)

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

 drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus 
<TypeName>)

analyzed 527 files ú 1764 nodes ú 1096 edges ú 121 entries ú 108/121 target ú 
~3681 tokens ú 5.1s stage2 x2.3 stage3 x1.5

                                    Insights                                    
ÚÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³ Sev  ³ Category ³ Title                        ³ Evidence                    ³
ÃÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ WARN ³ Risk     ³ 56/56 endpoints anonymous,   ³ GET /user/orders, GET       ³
³      ³          ³ incl. 18 POST/PUT/DELETE     ³ /user/logout, GET           ³
³      ³          ³                              ³ /item/{itemId:int}          ³
³ NOTE ³ Wiring   ³ Possible dead code: 5 public ³ CustomNavigationView,       ³
³      ³          ³ types with zero inbound      ³ IndexViewModel, IEventBus   ³
³      ³          ³ references                   ³                             ³
³ NOTE ³ Topology ³ Most depended-upon:          ³ eShop.ServiceDefaults (9    ³
³      ³          ³ eShop.ServiceDefaults (9     ³ dependents),                ³
³      ³          ³ dependents) ú                ³ EventBusRabbitMQ (7         ³
³      ³          ³ EventBusRabbitMQ (7          ³ dependents),                ³
³      ³          ³ dependents) ú                ³ IntegrationEventLogEF (4    ³
³      ³          ³ IntegrationEventLogEF (4     ³ dependents)                 ³
³      ³          ³ dependents)                  ³                             ³
³ NOTE ³ Wiring   ³ External event contracts: 1  ³ BeforeStartEvent            ³
³      ³          ³ consumed but never produced  ³                             ³
³      ³          ³ internally                   ³                             ³
³ NOTE ³ Wiring   ³ Multi-implementation         ³ ? (29 impls),               ³
³      ³          ³ interfaces: ? (29 impls) ú   ³ ReportApiVersions = true;   ³
³      ³          ³ ReportApiVersions = true;    ³ } (3 impls),                ³
³      ³          ³ } (3 impls) ú                ³ ServerAuthenticationStatePr ³
³      ³          ³ ServerAuthenticationStatePro ³ ovider (2 impls)            ³
³      ³          ³ vider (2 impls)              ³                             ³
³ INFO ³ Coverage ³ Entry targets resolved       ³                             ³
³      ³          ³ 108/121 (89%) - use --focus  ³                             ³
³      ³          ³ for deeper traces            ³                             ³
³ INFO ³ Shape    ³ Entry surface: 56 HTTP ú 29  ³ 56 HTTP, 29 UI, 16          ³
³      ³          ³ UI ú 16 GrpcService ú 13 Bus ³ GrpcService                 ³
ÀÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ
                           Stage Timing                           
ÚÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³ Stage                   ³   Time ³ Bar                         ³
ÃÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ DiscoveryAndCacheWarmup ³  265ms ³ ÛÛ                          ³
³ GenericExtraction       ³  543ms ³ ÛÛÛÛ                        ³
³ SignalSealing           ³    1ms ³                             ³
³ SpecificExtraction      ³ 3458ms ³ ÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛÛ ³
³ Compression             ³   52ms ³                             ³
³ Total                   ³ 5071ms ³                             ³
ÀÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ

                                   Extractors                                   
ÚÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³ Name                     ³   Time ³ +Types ³ +Dets ³ Status                  ³
ÃÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ CallGraphExtractor       ³ 3002ms ³      0 ³     0 ³ ran                     ³
³ SyntaxStructureExtractor ³  540ms ³    523 ³   154 ³ ran                     ³
³ DiRegistrationExtractor  ³  467ms ³      0 ³   154 ³ ran                     ³
³ EndpointExtractor        ³  453ms ³      0 ³   221 ³ ran                     ³
³ EventBusExtractor        ³  268ms ³      0 ³   208 ³ ran                     ³
³ EfCoreExtractor          ³  201ms ³      0 ³   190 ³ ran                     ³
³ ControllerActionExtracto ³  178ms ³      0 ³   153 ³ ran                     ³
³ r                        ³        ³        ³       ³                         ³
³ ProgramCsFlowExtractor   ³  175ms ³      0 ³    19 ³ ran                     ³
³ BlazorEntryExtractor     ³  174ms ³      0 ³   153 ³ ran                     ³
³ MediatRExtractor         ³  150ms ³      0 ³    98 ³ ran                     ³
³ InMemoryEventBusExtracto ³  141ms ³      0 ³    48 ³ ran                     ³
³ r                        ³        ³        ³       ³                         ³
³ IndirectWiringDetector   ³  127ms ³      0 ³    32 ³ ran                     ³
³ SourceBodyExtractor      ³  125ms ³      0 ³     0 ³ ran                     ³
³ GrpcServiceExtractor     ³  119ms ³      0 ³   160 ³ ran                     ³
³ DesktopEntryExtractor    ³  111ms ³      0 ³   105 ³ ran                     ³
³ ProjectStructure         ³  107ms ³      0 ³     0 ³ ran                     ³
³ FileTreeExtractor        ³   91ms ³      0 ³     0 ³ ran                     ³
³ SolutionDiscovery        ³   48ms ³      0 ³     0 ³ ran                     ³
³ DependencyExtractor      ³   29ms ³      0 ³     0 ³ ran                     ³
³ LayerClassifier          ³   27ms ³      0 ³     0 ³ ran                     ³
³ AspireExtractor          ³    7ms ³      0 ³    50 ³ ran                     ³
³ AntiPatternDetector      ³    0ms ³      0 ³     0 ³ skipped: gated by       ³
³                          ³        ³        ³       ³ ShouldRun               ³
³ AwsLambdaExtractor       ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs aws-lambda        ³
³ AzureFunctionsExtractor  ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs azure-functions   ³
³ CliCommandExtractor      ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs cli-commands      ³
ÀÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ

            Graph Seams            
ÚÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄ¿
³ Seam           ³ Edges ³ Approx ³
ÃÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄ´
³ Calls          ³   381 ³     50 ³
³ Sends          ³    16 ³     16 ³
³ Handles        ³    18 ³      0 ³
³ Raises         ³    49 ³     49 ³
³ Consumes       ³    20 ³      0 ³
³ ReadsWrites    ³   472 ³    459 ³
³ Resolves       ³    76 ³      9 ³
³ WrappedBy      ³    54 ³      0 ³
³ EntityRelation ³    10 ³     10 ³
ÀÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÙ
1764 nodes ú 1096 edges ú 108/121 entries  target
cache 88% hit ú 527 files ú 0 projects
ÚÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³  Metric  ³        Value         ³
ÃÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ Solution ³      eShop.slnx      ³
³   Time   ³        5533ms        ³
³  Tokens  ³ ~3681 (budget 8000)  ³
³ Version  ³ v1.0.5-preview.0.244 ³
ÀÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ
