# REPORT
**eshop-microservices**

Style: Microservices
_11 projects  ·  27 HttpEndpoint, 1 MessageConsumer, 2 DomainEventHandler, 4 GrpcService  ·  net8.0 + minimal-apis + refit + mediatr + scrutor + masstransit + efcore + razor-pages + grpc + fluentvalidation + gateway + healthchecks_

## Stats

| Metric | Value |
|--------|-------|
| Files | 146 |
| Projects | 11 |
| Nodes | 432 |
| Edges | 330 |
| ServiceLinks | 6 |
| Entries | 34 |
| With target | 33/34 |
| Verified edges | 71% |
| Analyzed in | 3.5s |

## Top Flows

1. **POST /orders** → `CreateOrderCommand` *(HttpEndpoint)*
2. **PUT /orders** → `UpdateOrderCommand` *(HttpEndpoint)*
3. **DELETE /orders/{id}** → `DeleteOrderCommand` *(HttpEndpoint)*
4. **GET /orders** → `GetOrdersQuery` *(HttpEndpoint)*
5. **GET /orders/{orderName}** → `GetOrdersByNameQuery` *(HttpEndpoint)*
6. **GET /orders/customer/{customerId}** → `GetOrdersByCustomerQuery` *(HttpEndpoint)*
7. **GET /products** → `GetProductsQuery` *(HttpEndpoint)*
8. **POST /basket** → `StoreBasketCommand` *(HttpEndpoint)*
9. **POST /basket/checkout** → `CheckoutBasketCommand` *(HttpEndpoint)*
10. **POST /products** → `CreateProductCommand` *(HttpEndpoint)*

### Trace 1: POST /orders

TRACE  POST /orders
       Services/Ordering/Ordering.API/Endpoints/CreateOrder.cs:17
       Ordering.API
▸ ENTRY  POST /orders  (Services/Ordering/Ordering.API/Endpoints/CreateOrder.cs:17)
   └─ call <lambda> POST /orders  (Services/Ordering/Ordering.API/Endpoints/CreateOrder.cs:17)
      ├─ send CreateOrderCommand  (Services/Ordering/Ordering.API/Endpoints/CreateOrder.cs:5) [verified]
      │      public record CreateOrderCommand(OrderDto Order)
      │      : ICommand<CreateOrderResult>;
      │      pipeline ▸ ValidationBehavior → LoggingBehavior
      │  └─ handler CreateOrderHandler  (Services/Ordering/Ordering.Application/Orders/Commands/CreateOrder/CreateOrderHandler.cs:2)
      │         public class CreateOrderHandler(IApplicationDbContext dbContext)
      │         : ICommandHandler<CreateOrderCommand, CreateOrderResult>
      │         public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
      │     ├─ data Order  (Services/Ordering/Ordering.Application/Orders/Commands/CreateOrder/CreateOrderHandler.cs:11) [verified]
      │     │      public class Order : Aggregate<OrderId>
      │     │      private readonly List<OrderItem> _orderItems = new();
      │     │      public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();
      │     ├─ call IApplicationDbContext  (Services/Ordering/Ordering.Application/Orders/Commands/CreateOrder/CreateOrderHandler.cs:13) [approx]
      │     │      public interface IApplicationDbContext
      │     │      DbSet<Customer> Customers { get; }
      │     │      DbSet<Product> Products { get; }
      │     │  └─ di ApplicationDbContext  (Services/Ordering/Ordering.Infrastructure/DependencyInjection.cs:24)
      │     │         public class ApplicationDbContext : DbContext, IApplicationDbContext
      │     │         public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      │     │         : base(options) { }
      │     ├─ call ApplicationDbContext.SaveChangesAsync  (Services/Ordering/Ordering.Application/Orders/Commands/CreateOrder/CreateOrderHandler.cs:14) [verified]
      │     ├─ call ApplicationDbContext.Add  (Services/Ordering/Ordering.Application/Orders/Commands/CreateOrder/CreateOrderHandler.cs:13) [approx]
      │     └─ call CreateNewOrder  (Services/Ordering/Ordering.Application/Orders/Commands/CreateOrder/CreateOrderHandler.cs:11) [verified]
      │            private static Order CreateNewOrder(OrderDto orderDto)
      │            var shippingAddress = Address.Of(orderDto.ShippingAddress.FirstName, orderDto.ShippingAddress.LastName, orderDto.ShippingAddress.EmailAddress, orderDto.ShippingAddress.AddressLine, orderDto.ShippingAddress.Country, orderDto.ShippingAddress.State, orderDto.ShippingAddress.ZipCode);
      │            var billingAddress = Address.Of(orderDto.BillingAddress.FirstName, orderDto.BillingAddress.LastName, orderDto.BillingAddress.EmailAddress, orderDto.BillingAddress.AddressLine, orderDto.BillingAddress.Country, orderDto.BillingAddress.State, orderDto.BillingAddress.ZipCode);
      │        (2 more branches omitted beyond fan-out)
      │        ├─ data Order  (Services/Ordering/Ordering.Application/Orders/Commands/CreateOrder/CreateOrderHandler.cs:24) [verified]
      │        │      public class Order : Aggregate<OrderId>
      │        │      private readonly List<OrderItem> _orderItems = new();
      │        │      public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();
      │        ├─ call Order  (Services/Ordering/Ordering.Application/Orders/Commands/CreateOrder/CreateOrderHandler.cs:25) [verified]
      │        │      public class Order : Aggregate<OrderId>
      │        │      private readonly List<OrderItem> _orderItems = new();
      │        │      public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();
      │        ├─ call ProductId.Of  (Services/Ordering/Ordering.Application/Orders/Commands/CreateOrder/CreateOrderHandler.cs:35) [verified]
      │        │      public static ProductId Of(Guid value)
      │        │      ArgumentNullException.ThrowIfNull(value);
      │        │      if (value == Guid.Empty)
      │        ├─ call Add  (Services/Ordering/Ordering.Application/Orders/Commands/CreateOrder/CreateOrderHandler.cs:35) [verified]
      │        │      public void Add(ProductId productId, int quantity, decimal price)
      │        │      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
      │        │      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);
      │        ├─ call Payment.Of  (Services/Ordering/Ordering.Application/Orders/Commands/CreateOrder/CreateOrderHandler.cs:30) [verified]
      │        │      public static Payment Of(string cardName, string cardNumber, string expiration, string cvv, int paymentMethod)
      │        │      ArgumentException.ThrowIfNullOrWhiteSpace(cardName);
      │        │      ArgumentException.ThrowIfNullOrWhiteSpace(cardNumber);
      │        ├─ call OrderName.Of  (Services/Ordering/Ordering.Application/Orders/Commands/CreateOrder/CreateOrderHandler.cs:27) [verified]
      │        │      public static OrderName Of(string value)
      │        │      ArgumentException.ThrowIfNullOrWhiteSpace(value);
      │        │      //ArgumentOutOfRangeException.ThrowIfNotEqual(value.Length, DefaultLength);
      │        ├─ call CustomerId.Of  (Services/Ordering/Ordering.Application/Orders/Commands/CreateOrder/CreateOrderHandler.cs:26) [verified]
      │        │      public static CustomerId Of(Guid value)
      │        │      ArgumentNullException.ThrowIfNull(value);
      │        │      if (value == Guid.Empty)
      │        └─ call OrderId.Of  (Services/Ordering/Ordering.Application/Orders/Commands/CreateOrder/CreateOrderHandler.cs:25) [verified]
      │               public static OrderId Of(Guid value)
      │               ArgumentNullException.ThrowIfNull(value);
      │               if (value == Guid.Empty)
      └─ call CreateOrderRequest  (Services/Ordering/Ordering.API/Endpoints/CreateOrder.cs:3) [approx]
             //- Accepts a CreateOrderRequest object.
             //- Maps the request to a CreateOrderCommand.
             //- Uses MediatR to send the command to the corresponding handler.

TOUCHES  Order
RESULT   200 OK / 201 Created · failure → 400 Bad Request

---

### Trace 2: PUT /orders

TRACE  PUT /orders
       Services/Ordering/Ordering.API/Endpoints/UpdateOrder.cs:17
       Ordering.API
▸ ENTRY  PUT /orders  (Services/Ordering/Ordering.API/Endpoints/UpdateOrder.cs:17)
   └─ call <lambda> PUT /orders  (Services/Ordering/Ordering.API/Endpoints/UpdateOrder.cs:17)
      ├─ send UpdateOrderCommand  (Services/Ordering/Ordering.API/Endpoints/UpdateOrder.cs:5) [verified]
      │      public record UpdateOrderCommand(OrderDto Order)
      │      : ICommand<UpdateOrderResult>;
      │      pipeline ▸ ValidationBehavior → LoggingBehavior
      │  └─ handler UpdateOrderHandler  (Services/Ordering/Ordering.Application/Orders/Commands/UpdateOrder/UpdateOrderHandler.cs:2)
      │         public class UpdateOrderHandler(IApplicationDbContext dbContext)
      │         : ICommandHandler<UpdateOrderCommand, UpdateOrderResult>
      │         public async Task<UpdateOrderResult> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
      │     ├─ data Order  (Services/Ordering/Ordering.Application/Orders/Commands/UpdateOrder/UpdateOrderHandler.cs:11) [approx]
      │     │      public class Order : Aggregate<OrderId>
      │     │      private readonly List<OrderItem> _orderItems = new();
      │     │      public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();
      │     ├─ call IApplicationDbContext  (Services/Ordering/Ordering.Application/Orders/Commands/UpdateOrder/UpdateOrderHandler.cs:12) [approx]
      │     │      public interface IApplicationDbContext
      │     │      DbSet<Customer> Customers { get; }
      │     │      DbSet<Product> Products { get; }
      │     │  └─ di ApplicationDbContext  (Services/Ordering/Ordering.Infrastructure/DependencyInjection.cs:24)
      │     │         public class ApplicationDbContext : DbContext, IApplicationDbContext
      │     │         public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      │     │         : base(options) { }
      │     ├─ call ApplicationDbContext.SaveChangesAsync  (Services/Ordering/Ordering.Application/Orders/Commands/UpdateOrder/UpdateOrderHandler.cs:23) [verified]
      │     ├─ call ApplicationDbContext.Update  (Services/Ordering/Ordering.Application/Orders/Commands/UpdateOrder/UpdateOrderHandler.cs:22) [approx]
      │     ├─ call UpdateOrderWithNewValues  (Services/Ordering/Ordering.Application/Orders/Commands/UpdateOrder/UpdateOrderHandler.cs:20) [verified]
      │     │      public void UpdateOrderWithNewValues(Order order, OrderDto orderDto)
      │     │      var updatedShippingAddress = Address.Of(orderDto.ShippingAddress.FirstName, orderDto.ShippingAddress.LastName, orderDto.ShippingAddress.EmailAddress, orderDto.ShippingAddress.AddressLine, orderDto.ShippingAddress.Country, orderDto.ShippingAddress.State, orderDto.ShippingAddress.ZipCode);
      │     │      var updatedBillingAddress = Address.Of(orderDto.BillingAddress.FirstName, orderDto.BillingAddress.LastName, orderDto.BillingAddress.EmailAddress, orderDto.BillingAddress.AddressLine, orderDto.BillingAddress.Country, orderDto.BillingAddress.State, orderDto.BillingAddress.ZipCode);
      │     │  ├─ data Order  (Services/Ordering/Ordering.Application/Orders/Commands/UpdateOrder/UpdateOrderHandler.cs:34) [verified]
      │     │  │      public class Order : Aggregate<OrderId>
      │     │  │      private readonly List<OrderItem> _orderItems = new();
      │     │  │      public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();
      │     │  ├─ call Order  (Services/Ordering/Ordering.Application/Orders/Commands/UpdateOrder/UpdateOrderHandler.cs:34) [verified]
      │     │  │      public class Order : Aggregate<OrderId>
      │     │  │      private readonly List<OrderItem> _orderItems = new();
      │     │  │      public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();
      │     │  ├─ call OrderName.Of  (Services/Ordering/Ordering.Application/Orders/Commands/UpdateOrder/UpdateOrderHandler.cs:35) [verified]
      │     │  │      public static OrderName Of(string value)
      │     │  │      ArgumentException.ThrowIfNullOrWhiteSpace(value);
      │     │  │      //ArgumentOutOfRangeException.ThrowIfNotEqual(value.Length, DefaultLength);
      │     │  ├─ call Update  (Services/Ordering/Ordering.Application/Orders/Commands/UpdateOrder/UpdateOrderHandler.cs:34) [verified]
      │     │  │      public void Update(OrderName orderName, Address shippingAddress, Address billingAddress, Payment payment, OrderStatus status)
      │     │  │      OrderName = orderName;
      │     │  │      ShippingAddress = shippingAddress;
      │     │  │  (stopped at depth 5; 2 branches omitted)
      │     │  ├─ call Payment.Of  (Services/Ordering/Ordering.Application/Orders/Commands/UpdateOrder/UpdateOrderHandler.cs:32) [verified]
      │     │  │      public static Payment Of(string cardName, string cardNumber, string expiration, string cvv, int paymentMethod)
      │     │  │      ArgumentException.ThrowIfNullOrWhiteSpace(cardName);
      │     │  │      ArgumentException.ThrowIfNullOrWhiteSpace(cardNumber);
      │     │  └─ call Address.Of  (Services/Ordering/Ordering.Application/Orders/Commands/UpdateOrder/UpdateOrderHandler.cs:31) [verified]
      │     │         public static Address Of(string firstName, string lastName, string emailAddress, string addressLine, string country, string state, string zipCode)
      │     │         ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);
      │     │         ArgumentException.ThrowIfNullOrWhiteSpace(addressLine);
      │     ├─ call ApplicationDbContext.FindAsync  (Services/Ordering/Ordering.Application/Orders/Commands/UpdateOrder/UpdateOrderHandler.cs:12) [approx]
      │     └─ call OrderId.Of  (Services/Ordering/Ordering.Application/Orders/Commands/UpdateOrder/UpdateOrderHandler.cs:11) [verified]
      │            public static OrderId Of(Guid value)
      │            ArgumentNullException.ThrowIfNull(value);
      │            if (value == Guid.Empty)
      └─ call UpdateOrderRequest  (Services/Ordering/Ordering.API/Endpoints/UpdateOrder.cs:3) [approx]
             //- Accepts a UpdateOrderRequest.
             //- Maps the request to an UpdateOrderCommand.
             //- Sends the command for processing.

TOUCHES  Order
RESULT   200 OK / 204 No Content · failure → 400 Bad Request

---

### Trace 3: DELETE /orders/{id}

TRACE  DELETE /orders/{id}
       Services/Ordering/Ordering.API/Endpoints/DeleteOrder.cs:17
       Ordering.API
▸ ENTRY  DELETE /orders/{id}  (Services/Ordering/Ordering.API/Endpoints/DeleteOrder.cs:17)
   └─ call <lambda> DELETE /orders/{id}  (Services/Ordering/Ordering.API/Endpoints/DeleteOrder.cs:17)
      └─ send DeleteOrderCommand  (Services/Ordering/Ordering.API/Endpoints/DeleteOrder.cs:3) [verified]
             public record DeleteOrderCommand(Guid OrderId)
             : ICommand<DeleteOrderResult>;
             pipeline ▸ ValidationBehavior → LoggingBehavior
         └─ handler DeleteOrderHandler  (Services/Ordering/Ordering.Application/Orders/Commands/DeleteOrder/DeleteOrderHandler.cs:2)
                public class DeleteOrderHandler(IApplicationDbContext dbContext)
                : ICommandHandler<DeleteOrderCommand, DeleteOrderResult>
                public async Task<DeleteOrderResult> Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
            ├─ call IApplicationDbContext  (Services/Ordering/Ordering.Application/Orders/Commands/DeleteOrder/DeleteOrderHandler.cs:12) [approx]
            │      public interface IApplicationDbContext
            │      DbSet<Customer> Customers { get; }
            │      DbSet<Product> Products { get; }
            │  └─ di ApplicationDbContext  (Services/Ordering/Ordering.Infrastructure/DependencyInjection.cs:24)
            │         public class ApplicationDbContext : DbContext, IApplicationDbContext
            │         public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            │         : base(options) { }
            ├─ call ApplicationDbContext.SaveChangesAsync  (Services/Ordering/Ordering.Application/Orders/Commands/DeleteOrder/DeleteOrderHandler.cs:21) [verified]
            ├─ call ApplicationDbContext.Remove  (Services/Ordering/Ordering.Application/Orders/Commands/DeleteOrder/DeleteOrderHandler.cs:20) [approx]
            ├─ call ApplicationDbContext.FindAsync  (Services/Ordering/Ordering.Application/Orders/Commands/DeleteOrder/DeleteOrderHandler.cs:12) [approx]
            └─ call OrderId.Of  (Services/Ordering/Ordering.Application/Orders/Commands/DeleteOrder/DeleteOrderHandler.cs:11) [verified]
                   public static OrderId Of(Guid value)
                   ArgumentNullException.ThrowIfNull(value);
                   if (value == Guid.Empty)
RESULT   200 OK / 204 No Content · failure → 404 Not Found

---

## Insights

_4 info · 3 notable · 3 warning_

### **WARNING**: 27/27 endpoints anonymous, incl. 9 POST/PUT/DELETE
*(Risk)*

- PUT /products
- GET /products
- GET /products/{id}
- GET /products/category/{category}
- DELETE /products/{id}

### **WARNING**: Missing validation: 17/27 endpoints have no FluentValidation validator
*(Risk)*

- GET /products → GetProductsQuery
- GET /products/{id} → GetProductByIdQuery
- GET /products/category/{category} → GetProductByCategoryQuery
- GET /basket/{userName} → GetBasketQuery
- GET /orders/{orderName} → GetOrdersByNameQuery

### **WARNING**: Auth surface: 0 protected, 27 unannotated of 27 API endpoints
*(Risk)*

- 27 no auth annotation

### **NOTABLE**: Downstream wiring: 6 target services via http-via-gateway, bus-publish→consume, grpc
*(Wiring)*

- YarpApiGateway ← http-via-gateway
- Ordering.Application ← bus-publish→consume
- Discount.Grpc ← grpc
- Ordering.API ← http-via-gateway
- Catalog.API ← http-via-gateway

### **NOTABLE**: Possible dead code: 5 public types with zero inbound references
*(Wiring)*

- GetBasketResult
- CreateProductEndpoint
- PaginationRequest
- BadRequestException
- GetProductByIdResult

### **NOTABLE**: Extension seats: AddRefitClient (3 impls) · AddDbContext (2 impls) · ISaveChangesInterceptor (2 impls)
*(Wiring)*

- AddRefitClient (3 impls)
- AddDbContext (2 impls)
- ISaveChangesInterceptor (2 impls)

### _INFO_: Entry targets resolved 33/34 (97%) — use --focus for deeper traces
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

MAP  eshop-microservices     (11 projects)

STACK  net8.0 · Minimal APIs · MediatR (CQRS) · EF Core · FluentValidation · MassTransit

STYLE  Microservices  (confidence high)
       evidence: 7 runnable web services with gateway + message bus

       per service:
         YarpApiGateway: Gateway [YARP]
         Shopping.Web: Web App [Refit]
         Basket.API: Web API
         Catalog.API: Web API
         Discount.Grpc: gRPC Service [gRPC]
         Ordering.API: Web API [EF Core]

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

ROUTES
    /catalog-service/{**catch-all}  →  http://catalog.api:8080{**catch-all}
    /basket-service/{**catch-all}  →  http://basket.api:8080{**catch-all}
    /ordering-service/{**catch-all}  →  http://ordering.api:8080{**catch-all}
    /catalog-service/{**catch-all}  →  http://localhost:6000/{**catch-all}
    /basket-service/{**catch-all}  →  http://localhost:6001/{**catch-all}
    /ordering-service/{**catch-all}  →  http://localhost:6003/{**catch-all}

CROSS-SERVICE
  bus (1)
  gRPC (1)
  http/via gateway (4)
    [bus] Basket.API → Ordering.Application  (Basket.API→Ordering.Application:BuildingBlocks.Messaging.Events.BasketCheckoutEvent)
    [gRPC] Basket.API → Discount.Grpc  (C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\Services\Basket\Basket.API\Basket\StoreBasket\StoreBasketHandler.cs:18→C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\Services\Discount\Discount.Grpc\Services\DiscountService.cs:9)
    [http] YarpApiGateway → Basket.API  (C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\WebApps\Shopping.Web\Services\IBasketService.cs:16)
    [http] YarpApiGateway → Catalog.API  (C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\WebApps\Shopping.Web\Services\ICatalogService.cs:11)
    [http] Shopping.Web → YarpApiGateway  (C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\WebApps\Shopping.Web\Services\IOrderingService.cs:11)
    [http] YarpApiGateway → Ordering.API  (C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\WebApps\Shopping.Web\Services\IOrderingService.cs:11)

ENTRY POINTS
   HTTP (27)
      DELETE /basket/{userName}  → DeleteBasketCommand  (Services/Basket/Basket.API/Basket/DeleteBasket/DeleteBasketEndpoints.cs:10)
      DELETE /orders/{id}  → DeleteOrderCommand  (Services/Ordering/Ordering.API/Endpoints/DeleteOrder.cs:17)
      DELETE /products/{id}  → DeleteProductCommand  (Services/Catalog/Catalog.API/Products/DeleteProduct/DeleteProductEndpoint.cs:11)
      GET /basket/{userName}  → GetBasketQuery  (Services/Basket/Basket.API/Basket/GetBasket/GetBasketEndpoints.cs:10)
      GET /Cart  → IBasketService  (WebApps/Shopping.Web/Pages/Cart.cshtml.cs:3)
      GET /Checkout  → IBasketService  (WebApps/Shopping.Web/Pages/Checkout.cshtml.cs:3)
      GET /Confirmation  → ConfirmationModel  (WebApps/Shopping.Web/Pages/Confirmation.cshtml.cs:3)
      GET /Contact  → ContactModel  (WebApps/Shopping.Web/Pages/Contact.cshtml.cs:6)
      GET /Error  → ErrorModel  (WebApps/Shopping.Web/Pages/Error.cshtml.cs:6)
      GET /Index  → ICatalogService  (WebApps/Shopping.Web/Pages/Index.cshtml.cs:2)
      GET /OrderList  → IOrderingService  (WebApps/Shopping.Web/Pages/OrderList.cshtml.cs:3)
      GET /orders  → GetOrdersQuery  (Services/Ordering/Ordering.API/Endpoints/GetOrders.cs:17)
      GET /orders/{orderName}  → GetOrdersByNameQuery  (Services/Ordering/Ordering.API/Endpoints/GetOrdersByName.cs:16)
      GET /orders/customer/{customerId}  → GetOrdersByCustomerQuery  (Services/Ordering/Ordering.API/Endpoints/GetOrdersByCustomer.cs:16)
      GET /Privacy  → PrivacyModel  (WebApps/Shopping.Web/Pages/Privacy.cshtml.cs:5)
      GET /ProductDetail  → ICatalogService  (WebApps/Shopping.Web/Pages/ProductDetail.cshtml.cs:3)
      GET /ProductList  → ICatalogService  (WebApps/Shopping.Web/Pages/ProductList.cshtml.cs:3)
      GET /products  → GetProductsQuery  (Services/Catalog/Catalog.API/Products/GetProducts/GetProductsEndpoint.cs:10)
      GET /products/{id}  → GetProductByIdQuery  (Services/Catalog/Catalog.API/Products/GetProductById/GetProductByIdEndpoint.cs:11)
      GET /products/category/{category}  → GetProductByCategoryQuery  (Services/Catalog/Catalog.API/Products/GetProductByCategory/GetProductByCategoryEndpoint.cs:11)
      … and 7 more (http entries — use --focus for a drill-in)
   Bus (1)
      BasketCheckoutEventHandler  → BasketCheckoutEventHandler  (Services/Ordering/Ordering.Application/Orders/EventHandlers/Integration/BasketCheckoutEventHandler.cs:6)
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
| DiscoveryAndCacheWarmup | 127ms |
| GenericExtraction | 206ms |
| SignalSealing | 0ms |
| SpecificExtraction | 1253ms |
| Compression | 17ms |
| **Total** | **3483ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 1115ms | 0 | 0 |
| SyntaxStructureExtractor | 203ms | 205 | 53 |
| DiRegistrationExtractor | 200ms | 0 | 53 |
| ProgramCsFlowExtractor | 154ms | 0 | 14 |
| EndpointExtractor | 136ms | 0 | 81 |
| FileTreeExtractor | 67ms | 0 | 0 |
| EventBusExtractor | 56ms | 0 | 45 |
| SourceBodyExtractor | 56ms | 0 | 0 |
| RazorPagesExtractor | 54ms | 0 | 46 |
| EfCoreExtractor | 39ms | 0 | 44 |
| SolutionDiscovery | 32ms | 0 | 0 |
| IndirectWiringDetector | 32ms | 0 | 32 |
| MediatRExtractor | 31ms | 0 | 32 |
| RefitInterfaceExtractor | 30ms | 0 | 23 |
| BodyFactsExtractor | 30ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 176 | 76 |
| Sends | 33 | 0 |
| Handles | 18 | 0 |
| Raises | 4 | 0 |
| Consumes | 5 | 0 |
| ReadsWrites | 25 | 16 |
| Resolves | 26 | 1 |
| WrappedBy | 36 | 0 |
| EntityRelation | 1 | 1 |
| ServiceLink | 6 | 3 |

_146 files · 11 projects_
