MAP  eshop-microservices     (11 projects)

STACK  net8.0 ú Minimal APIs ú MediatR (CQRS) ú EF Core ú FluentValidation ú 
MassTransit

STYLE  Microservices  (confidence high)
       evidence: 6 runnable web services with gateway + message bus

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
   Ordering.Application ÄÄ BuildingBlocks, BuildingBlocks.Messaging, 
Ordering.Domain
   Ordering.Domain
   Ordering.Infrastructure ÄÄ Ordering.Application
   Basket.API ÄÄ BuildingBlocks, BuildingBlocks.Messaging
   Catalog.API ÄÄ BuildingBlocks
   Discount.Grpc
   Ordering.API ÄÄ Ordering.Application, Ordering.Infrastructure
   Shopping.Web
   YarpApiGateway

ROUTES
    /catalog-service/{**catch-all}    http://localhost:6000/{**catch-all}
    /basket-service/{**catch-all}    http://localhost:6001/{**catch-all}
    /ordering-service/{**catch-all}    http://localhost:6003/{**catch-all}
    /catalog-service/{**catch-all}    http://catalog.api:8080{**catch-all}
    /basket-service/{**catch-all}    http://basket.api:8080{**catch-all}
    /ordering-service/{**catch-all}    http://ordering.api:8080{**catch-all}

CROSS-SERVICE
  bus (1)
  gRPC (1)
  http/via gateway (1)
    [bus] Basket.API  Ordering.Application  
(C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\Services\Basket\Ba
sket.API\Basket\CheckoutBasket\CheckoutBasketHandler.cs:41 raises 
BasketCheckoutEvent)
    [gRPC] Basket.API  Discount.Grpc  
(C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\Services\Basket\Ba
sket.API\Basket\StoreBasket\StoreBasketHandler.cs:18C:\Users\shahi\source\repos
\run-aspnetcore-microservices\src\Services\Discount\Discount.Grpc\Services\Disco
untService.cs:9)
    [http] Shopping.Web  YarpApiGateway  
(C:\Users\shahi\source\repos\run-aspnetcore-microservices\src\WebApps\Shopping.W
eb\Services\IBasketService.cs:7)

EVENT WIRING  (2 integration events, 1 cross-service)
  BasketCheckoutEvent: Basket.API  Ordering.Application
  <registration>: (external) ú BuildingBlocks.Messaging

ENTRY POINTS
   HTTP (27)
      DELETE /basket/{userName}   DeleteBasketCommand  
(Services/Basket/Basket.API/Basket/DeleteBasket/DeleteBasketEndpoints.cs:10)
      DELETE /orders/{id}   DeleteOrderCommand  
(Services/Ordering/Ordering.API/Endpoints/DeleteOrder.cs:17)
      DELETE /products/{id}   DeleteProductCommand  
(Services/Catalog/Catalog.API/Products/DeleteProduct/DeleteProductEndpoint.cs:11
)
      GET /basket/{userName}   GetBasketQuery  
(Services/Basket/Basket.API/Basket/GetBasket/GetBasketEndpoints.cs:10)
      GET /Cart   IBasketService.LoadUserBasket  
(WebApps/Shopping.Web/Pages/Cart.cshtml.cs:3)
      GET /Checkout   IBasketService.LoadUserBasket  
(WebApps/Shopping.Web/Pages/Checkout.cshtml.cs:3)
      GET /Confirmation   ConfirmationModel  
(WebApps/Shopping.Web/Pages/Confirmation.cshtml.cs:3)
      GET /Contact   ContactModel  
(WebApps/Shopping.Web/Pages/Contact.cshtml.cs:6)
      GET /Error   ErrorModel  (WebApps/Shopping.Web/Pages/Error.cshtml.cs:6)
      GET /Index   ICatalogService.GetProducts  
(WebApps/Shopping.Web/Pages/Index.cshtml.cs:2)
      GET /OrderList   IOrderingService.GetOrdersByCustomer  
(WebApps/Shopping.Web/Pages/OrderList.cshtml.cs:3)
      GET /orders   GetOrdersQuery  
(Services/Ordering/Ordering.API/Endpoints/GetOrders.cs:17)
      GET /orders/{orderName}   GetOrdersByNameQuery  
(Services/Ordering/Ordering.API/Endpoints/GetOrdersByName.cs:16)
      GET /orders/customer/{customerId}   GetOrdersByCustomerQuery  
(Services/Ordering/Ordering.API/Endpoints/GetOrdersByCustomer.cs:16)
      GET /Privacy   PrivacyModel  
(WebApps/Shopping.Web/Pages/Privacy.cshtml.cs:5)
      GET /ProductDetail   ICatalogService.GetProduct  
(WebApps/Shopping.Web/Pages/ProductDetail.cshtml.cs:3)
      GET /ProductList   ICatalogService.GetProducts  
(WebApps/Shopping.Web/Pages/ProductList.cshtml.cs:3)
      GET /products   GetProductsQuery  
(Services/Catalog/Catalog.API/Products/GetProducts/GetProductsEndpoint.cs:10)
      GET /products/{id}   GetProductByIdQuery  
(Services/Catalog/Catalog.API/Products/GetProductById/GetProductByIdEndpoint.cs:
11)
      GET /products/category/{category}   GetProductByCategoryQuery  
(Services/Catalog/Catalog.API/Products/GetProductByCategory/GetProductByCategory
Endpoint.cs:11)
      . and 7 more (http entries - trace one for a drill-in)
   Bus (1)
      BasketCheckoutEventHandler  
(Services/Ordering/Ordering.Application/Orders/EventHandlers/Integration/BasketC
heckoutEventHandler.cs:6)
   Domain (2)
      OrderCreatedEventHandler  
(Services/Ordering/Ordering.Application/Orders/EventHandlers/Domain/OrderCreated
EventHandler.cs:5)
      OrderUpdatedEventHandler  
(Services/Ordering/Ordering.Application/Orders/EventHandlers/Domain/OrderUpdated
EventHandler.cs:2)
   gRPC (4)
      DiscountProtoService.CreateDiscount   DiscountContext  
(Services/Discount/Discount.Grpc/Services/DiscountService.cs:9)
      DiscountProtoService.DeleteDiscount   DiscountContext  
(Services/Discount/Discount.Grpc/Services/DiscountService.cs:9)
      DiscountProtoService.GetDiscount   DiscountContext  
(Services/Discount/Discount.Grpc/Services/DiscountService.cs:9)
      DiscountProtoService.UpdateDiscount   DiscountContext  
(Services/Discount/Discount.Grpc/Services/DiscountService.cs:9)

CROSS-CUTTING
   MediatR pipeline (every command):  LoggingBehavior  ValidationBehavior

PACKAGES
   Web/API:  AspNetCore.HealthChecks.NpgSql 8.0.0, AspNetCore.HealthChecks.Redis
8.0.0, AspNetCore.HealthChecks.SqlServer 8.0.0, 
AspNetCore.HealthChecks.UI.Client 8.0.0, FluentValidation.AspNetCore 11.3.0, 
Grpc.AspNetCore 2.60.0, Microsoft.FeatureManagement.AspNetCore 3.2.0
   ORM/Data:  Microsoft.EntityFrameworkCore 8.0.2, 
Microsoft.EntityFrameworkCore.Design 8.0.2, Microsoft.EntityFrameworkCore.Sqlite
8.0.1, Microsoft.EntityFrameworkCore.SqlServer 8.0.2, 
Microsoft.EntityFrameworkCore.Tools 8.0.1
   Mediator/CQRS:  MediatR 12.2.0
   Messaging:  MassTransit.RabbitMQ 8.1.3
   Validation:  FluentValidation 11.9.0, 
FluentValidation.DependencyInjectionExtensions 11.9.0
   Cloud:  Microsoft.VisualStudio.Azure.Containers.Tools.Targets 1.19.5
   Utilities:  Refit.HttpClientFactory 7.0.0, Scrutor 4.2.2
   Other:  Carter 8.0.0, Mapster 7.4.0, Marten 6.4.1, 
Microsoft.Extensions.Caching.StackExchangeRedis 8.0.1, Yarp.ReverseProxy 2.1.0

 drill in:  trace a focused entry   (e.g. trace "POST /orders")

