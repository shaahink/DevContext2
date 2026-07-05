# REPORT
**eshop-microservices**

Style: CleanArchitecture
_11 projects  ·  27 HttpEndpoint, 3 MessageConsumer, 2 DomainEventHandler, 4 GrpcService  ·  net8.0 + minimal-apis + refit + mediatr + scrutor + masstransit + efcore + razor-pages + grpc + fluentvalidation + gateway + healthchecks_

## Stats

| Metric | Value |
|--------|-------|
| Files | 146 |
| Projects | 11 |
| Nodes | 488 |
| Edges | 303 |
| Entries | 36 |
| With target | 35/36 |
| Verified edges | 60% |
| Analyzed in | 2.7s |

## Top Flows

1. **DELETE /orders/{id}** → `DeleteOrderCommand` *(HttpEndpoint)*
2. **GET /orders** → `GetOrdersQuery` *(HttpEndpoint)*
3. **GET /orders/{orderName}** → `GetOrdersByNameQuery` *(HttpEndpoint)*
4. **GET /orders/customer/{customerId}** → `GetOrdersByCustomerQuery` *(HttpEndpoint)*
5. **DELETE /basket/{userName}** → `DeleteBasketCommand` *(HttpEndpoint)*
6. **DELETE /products/{id}** → `DeleteProductCommand` *(HttpEndpoint)*
7. **GET /basket/{userName}** → `GetBasketQuery` *(HttpEndpoint)*
8. **GET /ProductDetailModel** → `ICatalogService.GetProduct` *(HttpEndpoint)*
9. **GET /products/{id}** → `GetProductByIdQuery` *(HttpEndpoint)*
10. **GET /products/category/{category}** → `GetProductByCategoryQuery` *(HttpEndpoint)*

### Trace 1: DELETE /orders/{id}

TRACE  DELETE /orders/{id}
       Services/Ordering/Ordering.API/Endpoints/DeleteOrder.cs:17

▸ ENTRY  DELETE /orders/{id}  (Services/Ordering/Ordering.API/Endpoints/DeleteOrder.cs:17)
   └─ call <lambda> DELETE /orders/{id}  (Services/Ordering/Ordering.API/Endpoints/DeleteOrder.cs:17)
      └─ send DeleteOrderCommand  (Services/Ordering/Ordering.API/Endpoints/DeleteOrder.cs:17) [approx]
             public record DeleteOrderCommand(Guid OrderId)
             : ICommand<DeleteOrderResult>;
             pipeline ▸ ValidationBehavior → LoggingBehavior
         └─ handler DeleteOrderHandler  (Services/Ordering/Ordering.Application/Orders/Commands/DeleteOrder/DeleteOrderHandler.cs:2)
                public class DeleteOrderHandler(IApplicationDbContext dbContext)
                : ICommandHandler<DeleteOrderCommand, DeleteOrderResult>
                public async Task<DeleteOrderResult> Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
            ├─ data Order [approx]
            │      public class Order : Aggregate<OrderId>
            │      private readonly List<OrderItem> _orderItems = new();
            │      public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();
            │  └─ data ApplicationDbContext  (Services/Ordering/Ordering.Infrastructure/Data/ApplicationDbContext.cs:7)
            │         public class ApplicationDbContext : DbContext, IApplicationDbContext
            │         public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            │         : base(options) { }
            ├─ call OrderId.Of  (Services/Ordering/Ordering.Application/Orders/Commands/DeleteOrder/DeleteOrderHandler.cs:11) [verified]
            │      public static OrderId Of(Guid value)
            │      ArgumentNullException.ThrowIfNull(value);
            │      if (value == Guid.Empty)
            ├─ call ApplicationDbContext.FindAsync  (Services/Ordering/Ordering.Application/Orders/Commands/DeleteOrder/DeleteOrderHandler.cs:12) [approx]
            ├─ call ApplicationDbContext.Remove  (Services/Ordering/Ordering.Application/Orders/Commands/DeleteOrder/DeleteOrderHandler.cs:20) [approx]
            └─ call ApplicationDbContext.SaveChangesAsync  (Services/Ordering/Ordering.Application/Orders/Commands/DeleteOrder/DeleteOrderHandler.cs:21) [verified]

TOUCHES  Order, OrderItem, Product, Customer
RESULT   200 OK / 204 No Content · failure → 404 Not Found

---

### Trace 2: GET /orders

TRACE  GET /orders
       Services/Ordering/Ordering.API/Endpoints/GetOrders.cs:17

▸ ENTRY  GET /orders  (Services/Ordering/Ordering.API/Endpoints/GetOrders.cs:17)
   └─ call <lambda> GET /orders  (Services/Ordering/Ordering.API/Endpoints/GetOrders.cs:17)
      └─ send GetOrdersQuery  (Services/Ordering/Ordering.API/Endpoints/GetOrders.cs:17) [approx]
             public record GetOrdersQuery(PaginationRequest PaginationRequest)
             : IQuery<GetOrdersResult>;
             pipeline ▸ ValidationBehavior → LoggingBehavior
         └─ handler GetOrdersHandler  (Services/Ordering/Ordering.Application/Orders/Queries/GetOrders/GetOrdersHandler.cs:4)
                public class GetOrdersHandler(IApplicationDbContext dbContext)
                : IQueryHandler<GetOrdersQuery, GetOrdersResult>
                public async Task<GetOrdersResult> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
            ├─ call ApplicationDbContext.LongCountAsync  (Services/Ordering/Ordering.Application/Orders/Queries/GetOrders/GetOrdersHandler.cs:15) [approx]
            └─ call ApplicationDbContext.Include  (Services/Ordering/Ordering.Application/Orders/Queries/GetOrders/GetOrdersHandler.cs:17) [approx]
RESULT   200 OK · failure → 404 Not Found

---

### Trace 3: GET /orders/{orderName}

TRACE  GET /orders/{orderName}
       Services/Ordering/Ordering.API/Endpoints/GetOrdersByName.cs:16

▸ ENTRY  GET /orders/{orderName}  (Services/Ordering/Ordering.API/Endpoints/GetOrdersByName.cs:16)
   └─ call <lambda> GET /orders/{orderName}  (Services/Ordering/Ordering.API/Endpoints/GetOrdersByName.cs:16)
      └─ send GetOrdersByNameQuery  (Services/Ordering/Ordering.API/Endpoints/GetOrdersByName.cs:16) [approx]
             public record GetOrdersByNameQuery(string Name)
             : IQuery<GetOrdersByNameResult>;
             pipeline ▸ ValidationBehavior → LoggingBehavior
         └─ handler GetOrdersByNameHandler  (Services/Ordering/Ordering.Application/Orders/Queries/GetOrdersByName/GetOrdersByNameHandler.cs:2)
                public class GetOrdersByNameHandler(IApplicationDbContext dbContext)
                : IQueryHandler<GetOrdersByNameQuery, GetOrdersByNameResult>
                public async Task<GetOrdersByNameResult> Handle(GetOrdersByNameQuery query, CancellationToken cancellationToken)
            └─ call ApplicationDbContext.Include  (Services/Ordering/Ordering.Application/Orders/Queries/GetOrdersByName/GetOrdersByNameHandler.cs:10) [approx]
RESULT   200 OK · failure → 404 Not Found

---

## Insights

_5 info · 3 notable · 2 warning_

### **WARNING**: 27/27 endpoints anonymous, incl. 9 POST/PUT/DELETE
*(Risk)*

- PUT /products
- GET /products
- GET /products/{id}
- GET /products/category/{category}
- DELETE /products/{id}

### **WARNING**: Auth surface: 0 protected, 27 unannotated of 27 endpoints
*(Risk)*

- 27 no auth annotation

### **NOTABLE**: Downstream wiring: 10 target services detected
*(Wiring)*

- <lambda> DELETE /basket/{userName}
- Address.Of
- Order.Create
- OrderId.Of
- CustomerId.Of

### **NOTABLE**: Possible dead code: 5 public types with zero inbound references
*(Wiring)*

- Product
- BadRequestException
- CreateProductCommand
- PaymentModel
- Extensions

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

- PUT /products
- GET /products
- GET /products/{id}
- GET /products/category/{category}
- DELETE /products/{id}

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
| DiscoveryAndCacheWarmup | 223ms |
| GenericExtraction | 263ms |
| SignalSealing | 0ms |
| SpecificExtraction | 1693ms |
| Compression | 26ms |
| **Total** | **2744ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 1500ms | 0 | 0 |
| SyntaxStructureExtractor | 259ms | 205 | 53 |
| DiRegistrationExtractor | 254ms | 0 | 53 |
| EndpointExtractor | 190ms | 0 | 70 |
| ProgramCsFlowExtractor | 186ms | 0 | 14 |
| FileTreeExtractor | 101ms | 0 | 0 |
| RazorPagesExtractor | 86ms | 0 | 54 |
| SolutionDiscovery | 74ms | 0 | 0 |
| EventBusExtractor | 67ms | 0 | 43 |
| SourceBodyExtractor | 57ms | 0 | 0 |
| EfCoreExtractor | 53ms | 0 | 33 |
| MediatRExtractor | 44ms | 0 | 31 |
| ProjectStructure | 41ms | 0 | 0 |
| IndirectWiringDetector | 39ms | 0 | 23 |
| GrpcServiceExtractor | 38ms | 0 | 23 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 121 | 30 |
| Sends | 19 | 19 |
| Handles | 18 | 0 |
| Raises | 2 | 2 |
| Consumes | 5 | 0 |
| ReadsWrites | 75 | 68 |
| Resolves | 26 | 1 |
| WrappedBy | 36 | 0 |
| EntityRelation | 1 | 1 |

_146 files · 0 projects_
