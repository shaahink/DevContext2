# REPORT
**eshop-microservices**

Style: CleanArchitecture
_11 projects  ·  27 HttpEndpoint, 3 MessageConsumer, 2 DomainEventHandler, 4 GrpcService  ·  net8.0 + minimal-apis + refit + mediatr + scrutor + masstransit + efcore + razor-pages + grpc + fluentvalidation + gateway + healthchecks_

## Stats

| Metric | Value |
|--------|-------|
| Files | 146 |
| Projects | 11 |
| Nodes | 474 |
| Edges | 213 |
| Entries | 36 |
| With target | 35/36 |
| Verified edges | 50% |
| Analyzed in | 3.8s |

## Top Flows

1. **GET /ProductDetailModel** → `ICatalogService.GetProduct` *(HttpEndpoint)*
2. **DELETE /basket/{userName}** → `DeleteBasketCommand` *(HttpEndpoint)*
3. **DELETE /orders/{id}** → `DeleteOrderCommand` *(HttpEndpoint)*
4. **DELETE /products/{id}** → `DeleteProductCommand` *(HttpEndpoint)*
5. **GET /basket/{userName}** → `GetBasketQuery` *(HttpEndpoint)*
6. **GET /orders** → `GetOrdersQuery` *(HttpEndpoint)*
7. **GET /orders/{orderName}** → `GetOrdersByNameQuery` *(HttpEndpoint)*
8. **GET /orders/customer/{customerId}** → `GetOrdersByCustomerQuery` *(HttpEndpoint)*
9. **GET /products/{id}** → `GetProductByIdQuery` *(HttpEndpoint)*
10. **GET /products/category/{category}** → `GetProductByCategoryQuery` *(HttpEndpoint)*

### Trace 1: GET /ProductDetailModel

TRACE  GET /ProductDetailModel
       WebApps/Shopping.Web/Pages/ProductDetail.cshtml.cs:3

▸ ENTRY  GET /ProductDetailModel  (WebApps/Shopping.Web/Pages/ProductDetail.cshtml.cs:3)
   └─ call ProductDetailModel.OnGetAsync  (WebApps/Shopping.Web/Pages/ProductDetail.cshtml.cs:3)
          public async Task<IActionResult> OnGetAsync(Guid productId)
          var response = await catalogService.GetProduct(productId);
          Product = response.Product;
      ├─ data Product [approx]
      │      public class Product
      │      public Guid Id { get; set; }
      │      public string Name { get; set; } = default!;
      │  └─ data ApplicationDbContext  (Services/Ordering/Ordering.Infrastructure/Data/ApplicationDbContext.cs:7)
      │         public class ApplicationDbContext : DbContext, IApplicationDbContext
      │         public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      │         : base(options) { }
      ├─ call ICatalogService.GetProduct  (WebApps/Shopping.Web/Pages/ProductDetail.cshtml.cs:17) [verified]
      │      [Get("/catalog-service/products/{id}")]
      │      Task<GetProductByIdResponse> GetProduct(Guid id);
      └─ call ProductDetailModel.Page  (WebApps/Shopping.Web/Pages/ProductDetail.cshtml.cs:20) [approx]

TOUCHES  Product, OrderItem, Order, Customer
RESULT   200 OK · failure → 404 Not Found

---

### Trace 2: DELETE /basket/{userName}

TRACE  DELETE /basket/{userName}
       Services/Basket/Basket.API/Basket/DeleteBasket/DeleteBasketEndpoints.cs:10

▸ ENTRY  DELETE /basket/{userName}  (Services/Basket/Basket.API/Basket/DeleteBasket/DeleteBasketEndpoints.cs:10)
   └─ call <lambda> DELETE /basket/{userName}  (Services/Basket/Basket.API/Basket/DeleteBasket/DeleteBasketEndpoints.cs:10)
      └─ send DeleteBasketCommand  (Services/Basket/Basket.API/Basket/DeleteBasket/DeleteBasketEndpoints.cs:10) [approx]
             public record DeleteBasketCommand(string UserName) : ICommand<DeleteBasketResult>;
RESULT   200 OK / 204 No Content · failure → 404 Not Found

---

### Trace 3: DELETE /orders/{id}

TRACE  DELETE /orders/{id}
       Services/Ordering/Ordering.API/Endpoints/DeleteOrder.cs:17

▸ ENTRY  DELETE /orders/{id}  (Services/Ordering/Ordering.API/Endpoints/DeleteOrder.cs:17)
   └─ call <lambda> DELETE /orders/{id}  (Services/Ordering/Ordering.API/Endpoints/DeleteOrder.cs:17)
      └─ send DeleteOrderCommand  (Services/Ordering/Ordering.API/Endpoints/DeleteOrder.cs:17) [approx]
             public record DeleteOrderCommand(Guid OrderId)
             : ICommand<DeleteOrderResult>;
RESULT   200 OK / 204 No Content · failure → 404 Not Found

---

## Insights

_5 info · 3 notable · 2 warning_

### **WARNING**: 27/27 endpoints anonymous, incl. 9 POST/PUT/DELETE
*(Risk)*

- GET /ProductListModel
- GET /ProductDetailModel
- GET /PrivacyModel
- GET /OrderListModel
- GET /IndexModel

### **WARNING**: Auth surface: 0 protected, 27 unannotated of 27 endpoints
*(Risk)*

- 27 no auth annotation

### **NOTABLE**: Downstream wiring: 10 target services detected
*(Wiring)*

- GetOrdersByNameQuery
- DeleteProductCommand
- <lambda> GET /products/category/{category}
- DeleteOrderCommand
- DeleteBasketCommand

### **NOTABLE**: Possible dead code: 5 public types with zero inbound references
*(Wiring)*

- CreateProductRequest
- GetOrdersResponse
- CustomerConfiguration
- PaymentModel
- GetOrdersByCustomerResponse

### **NOTABLE**: Extension seats: AddRefitClient (3 impls) · AddDbContext (2 impls) · ISaveChangesInterceptor (2 impls)
*(Wiring)*

- AddRefitClient (3 impls)
- AddDbContext (2 impls)
- ISaveChangesInterceptor (2 impls)

### _INFO_: Entry targets resolved 35/36 (97%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Module map: 8 feature areas
*(Shape)*

- Pages (10 entries)
- Endpoints (6 entries)
- Services (4 entries)
- Orders/EventHandlers/Domain (2 entries)
- Products/UpdateProduct (1 entries)

### _INFO_: Routing surface: 8 routes exposed
*(Shape)*

- GET /ProductListModel
- GET /ProductDetailModel
- GET /PrivacyModel
- GET /OrderListModel
- GET /IndexModel

### _INFO_: Public surface: 12 interfaces, 99 classes (198 total public types)
*(Shape)*

- 12 interfaces
- 99 classes

### _INFO_: Most depended-upon: BuildingBlocks (3 dependents) · BuildingBlocks.Messaging (2 dependents) · Ordering.Application (2 dependents)
*(Topology)*

- BuildingBlocks (3 dependents)
- BuildingBlocks.Messaging (2 dependents)
- Ordering.Application (2 dependents)

GATEWAY  eshop-microservices     (11 projects)

STACK  net8.0 · Minimal APIs · MediatR (CQRS) · EF Core · FluentValidation · MassTransit

STYLE  CleanArchitecture  (confidence high)
       evidence: DDD folder layers: Domain, Application, Infrastructure, Api, Core; 2 domain-event handlers; MediatR with 4 handlers

TOPOLOGY (depends-on)
   BuildingBlocks
   BuildingBlocks.Messaging
   Ordering.Application ── BuildingBlocks, BuildingBlocks.Messaging, Ordering.Domain
   Ordering.Domain
   Ordering.Infrastructure ── Ordering.Application
   Basket.API ── BuildingBlocks, BuildingBlocks.Messaging
   Catalog.API ── BuildingBlocks
   Discount.Grpc
   Ordering.API ── Ordering.Application, Ordering.Infrastructure
   Shopping.Web
   YarpApiGateway

ENTRY POINTS
   HTTP (27)
      DELETE /basket/{userName}  → DeleteBasketCommand  (Services/Basket/Basket.API/Basket/DeleteBasket/DeleteBasketEndpoints.cs:10)
      DELETE /orders/{id}  → DeleteOrderCommand  (Services/Ordering/Ordering.API/Endpoints/DeleteOrder.cs:17)
      DELETE /products/{id}  → DeleteProductCommand  (Services/Catalog/Catalog.API/Products/DeleteProduct/DeleteProductEndpoint.cs:11)
      GET /basket/{userName}  → GetBasketQuery  (Services/Basket/Basket.API/Basket/GetBasket/GetBasketEndpoints.cs:10)
      GET /CartModel  → IBasketService.LoadUserBasket  (WebApps/Shopping.Web/Pages/Cart.cshtml.cs:3)
      GET /CheckoutModel  → IBasketService.LoadUserBasket  (WebApps/Shopping.Web/Pages/Checkout.cshtml.cs:3)
      GET /ConfirmationModel  → ConfirmationModel  (WebApps/Shopping.Web/Pages/Confirmation.cshtml.cs:3)
      GET /ContactModel  → ContactModel  (WebApps/Shopping.Web/Pages/Contact.cshtml.cs:6)
      GET /ErrorModel  → ErrorModel  (WebApps/Shopping.Web/Pages/Error.cshtml.cs:6)
      GET /IndexModel  → ICatalogService.GetProducts  (WebApps/Shopping.Web/Pages/Index.cshtml.cs:2)
      GET /OrderListModel  → IOrderingService.GetOrdersByCustomer  (WebApps/Shopping.Web/Pages/OrderList.cshtml.cs:3)
      GET /orders  → GetOrdersQuery  (Services/Ordering/Ordering.API/Endpoints/GetOrders.cs:17)
      GET /orders/{orderName}  → GetOrdersByNameQuery  (Services/Ordering/Ordering.API/Endpoints/GetOrdersByName.cs:16)
      GET /orders/customer/{customerId}  → GetOrdersByCustomerQuery  (Services/Ordering/Ordering.API/Endpoints/GetOrdersByCustomer.cs:16)
      GET /PrivacyModel  → PrivacyModel  (WebApps/Shopping.Web/Pages/Privacy.cshtml.cs:5)
      GET /ProductDetailModel  → ICatalogService.GetProduct  (WebApps/Shopping.Web/Pages/ProductDetail.cshtml.cs:3)
      GET /ProductListModel  → ICatalogService.GetProducts  (WebApps/Shopping.Web/Pages/ProductList.cshtml.cs:3)
      GET /products  → GetProductsRequest  (Services/Catalog/Catalog.API/Products/GetProducts/GetProductsEndpoint.cs:10)
      GET /products/{id}  → GetProductByIdQuery  (Services/Catalog/Catalog.API/Products/GetProductById/GetProductByIdEndpoint.cs:11)
      GET /products/category/{category}  → GetProductByCategoryQuery  (Services/Catalog/Catalog.API/Products/GetProductByCategory/GetProductByCategoryEndpoint.cs:11)
      … and 7 more (http entries — use --focus for a drill-in)
   Bus (3)
      AddMassTransit  → AddMassTransit  (BuildingBlocks/BuildingBlocks.Messaging/MassTransit/Extentions.cs:12)
      BasketCheckoutEventHandler  → BasketCheckoutEventHandler  (Services/Ordering/Ordering.Application/Orders/EventHandlers/Integration/BasketCheckoutEventHandler.cs:6)
      UsingRabbitMq  → UsingRabbitMq  (BuildingBlocks/BuildingBlocks.Messaging/MassTransit/Extentions.cs:19)
   Domain (2)
      OrderCreatedEventHandler  → OrderCreatedEventHandler  (Services/Ordering/Ordering.Application/Orders/EventHandlers/Domain/OrderCreatedEventHandler.cs:5)
      OrderUpdatedEventHandler  → OrderUpdatedEventHandler  (Services/Ordering/Ordering.Application/Orders/EventHandlers/Domain/OrderUpdatedEventHandler.cs:2)
   gRPC (4)
      DiscountProtoService.CreateDiscount  → DiscountService  (Services/Discount/Discount.Grpc/Services/DiscountService.cs:9)
      DiscountProtoService.DeleteDiscount  → DiscountService  (Services/Discount/Discount.Grpc/Services/DiscountService.cs:9)
      DiscountProtoService.GetDiscount  → DiscountService  (Services/Discount/Discount.Grpc/Services/DiscountService.cs:9)
      DiscountProtoService.UpdateDiscount  → DiscountService  (Services/Discount/Discount.Grpc/Services/DiscountService.cs:9)

CROSS-CUTTING
   MediatR pipeline (every command):  LoggingBehavior → ValidationBehavior

PACKAGES
   Web/API:  AspNetCore.HealthChecks.NpgSql 8.0.0, AspNetCore.HealthChecks.Redis 8.0.0, AspNetCore.HealthChecks.SqlServer 8.0.0, AspNetCore.HealthChecks.UI.Client 8.0.0, FluentValidation.AspNetCore 11.3.0, Grpc.AspNetCore 2.60.0, Microsoft.FeatureManagement.AspNetCore 3.2.0
   ORM/Data:  Microsoft.EntityFrameworkCore 8.0.2, Microsoft.EntityFrameworkCore.Design 8.0.2, Microsoft.EntityFrameworkCore.Sqlite 8.0.1, Microsoft.EntityFrameworkCore.SqlServer 8.0.2, Microsoft.EntityFrameworkCore.Tools 8.0.1
   Mediator/CQRS:  MediatR 12.2.0
   Messaging:  MassTransit.RabbitMQ 8.1.3
   Validation:  FluentValidation 11.9.0, FluentValidation.DependencyInjectionExtensions 11.9.0
   Cloud:  Microsoft.VisualStudio.Azure.Containers.Tools.Targets 1.19.5
   Utilities:  Refit.HttpClientFactory 7.0.0, Scrutor 4.2.2
   Other:  Carter 8.0.0, Mapster 7.4.0, Marten 6.4.1, Microsoft.Extensions.Caching.StackExchangeRedis 8.0.1, Yarp.ReverseProxy 2.1.0

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 415ms |
| GenericExtraction | 968ms |
| SignalSealing | 0ms |
| SpecificExtraction | 1851ms |
| Compression | 29ms |
| **Total** | **3751ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 1531ms | 0 | 0 |
| SyntaxStructureExtractor | 963ms | 205 | 53 |
| DiRegistrationExtractor | 961ms | 0 | 53 |
| RazorPagesExtractor | 313ms | 0 | 54 |
| ProgramCsFlowExtractor | 305ms | 0 | 14 |
| ProjectStructure | 206ms | 0 | 0 |
| EndpointExtractor | 188ms | 0 | 34 |
| FileTreeExtractor | 125ms | 0 | 0 |
| SolutionDiscovery | 80ms | 0 | 0 |
| EventBusExtractor | 77ms | 0 | 18 |
| IndirectWiringDetector | 58ms | 0 | 17 |
| SourceBodyExtractor | 57ms | 0 | 0 |
| EfCoreExtractor | 47ms | 0 | 17 |
| InMemoryEventBusExtractor | 39ms | 0 | 12 |
| MediatRExtractor | 35ms | 0 | 12 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 79 | 15 |
| Sends | 19 | 19 |
| Handles | 2 | 0 |
| Raises | 2 | 2 |
| Consumes | 5 | 0 |
| ReadsWrites | 75 | 68 |
| Resolves | 26 | 1 |
| WrappedBy | 4 | 0 |
| EntityRelation | 1 | 1 |

_146 files · 0 projects_
