MAP  eshop-microservices     (11 projects)

STACK  net8.0 · Minimal APIs · MediatR (CQRS) · EF Core · FluentValidation · MassTransit

STYLE  Microservices  (confidence high)
       evidence: 7 runnable web services with gateway + message bus

       per service:
         YarpApiGateway: Gateway [YARP]
         Shopping.Web: Razor Pages [Refit, Razor]
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
    [bus] Basket.API → Ordering.Application  (C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\Services\Basket\Basket.API\Basket\CheckoutBasket\CheckoutBasketHandler.cs:41 raises BasketCheckoutEvent)
    [gRPC] Basket.API → Discount.Grpc  (C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\Services\Basket\Basket.API\Basket\StoreBasket\StoreBasketHandler.cs:18→C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\Services\Discount\Discount.Grpc\Services\DiscountService.cs:9)
    [http] YarpApiGateway → Basket.API  (C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\WebApps\Shopping.Web\Services\IBasketService.cs:16)
    [http] YarpApiGateway → Catalog.API  (C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\WebApps\Shopping.Web\Services\ICatalogService.cs:11)
    [http] Shopping.Web → YarpApiGateway  (C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\WebApps\Shopping.Web\Services\IOrderingService.cs:11)
    [http] YarpApiGateway → Ordering.API  (C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\WebApps\Shopping.Web\Services\IOrderingService.cs:11)

EVENT WIRING  (2 integration events, 1 cross-service)
  BasketCheckoutEvent: Basket.API → Ordering.Application
  <registration>: (external) · BuildingBlocks.Messaging

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
      DiscountProtoService.CreateDiscount  → Coupon  (Services/Discount/Discount.Grpc/Services/DiscountService.cs:9)
      DiscountProtoService.DeleteDiscount  → DiscountService  (Services/Discount/Discount.Grpc/Services/DiscountService.cs:9)
      DiscountProtoService.GetDiscount  → DiscountService  (Services/Discount/Discount.Grpc/Services/DiscountService.cs:9)
      DiscountProtoService.UpdateDiscount  → Coupon  (Services/Discount/Discount.Grpc/Services/DiscountService.cs:9)

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

→ drill in:  --focus "<entry>"   (e.g. --focus "PUT /orders")
