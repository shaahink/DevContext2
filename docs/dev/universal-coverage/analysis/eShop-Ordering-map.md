MAP  eShop     (7 projects)
SCOPE  7-project closure of 24-project eShop — style/topology are local to this slice, not the whole system

STACK  net10.0 · Minimal APIs · MediatR (CQRS) · EF Core · FluentValidation · DDD aggregates

STYLE  CleanArchitecture  (confidence high)
       evidence: DDD folder layers: Domain, Application, Infrastructure, Api, Core; 7 domain-event handlers; MediatR with 16 handlers

TOPOLOGY (depends-on)
   EventBus
   IntegrationEventLogEF ── EventBus
   Ordering.Domain
   eShop.ServiceDefaults
   EventBusRabbitMQ ── EventBus
   Ordering.Infrastructure ── IntegrationEventLogEF, Ordering.Domain
   Ordering.API ── eShop.ServiceDefaults, EventBusRabbitMQ, IntegrationEventLogEF, Ordering.Domain, Ordering.Infrastructure

ENTRY POINTS
   HTTP (7)
      GET /api/orders/  → OrdersApi  (src/Ordering.API/Apis/OrdersApi.cs:14)
      GET /api/orders/{orderId:int}  → OrdersApi  (src/Ordering.API/Apis/OrdersApi.cs:13)
      GET /api/orders/cardtypes  → OrdersApi  (src/Ordering.API/Apis/OrdersApi.cs:15)
      POST /api/orders/  → CreateOrderCommand  (src/Ordering.API/Apis/OrdersApi.cs:17)
      POST /api/orders/draft  → ShipOrderCommand  (src/Ordering.API/Apis/OrdersApi.cs:16)
      PUT /api/orders/cancel  → CancelOrderCommand  (src/Ordering.API/Apis/OrdersApi.cs:11)
      PUT /api/orders/ship  → ShipOrderCommand  (src/Ordering.API/Apis/OrdersApi.cs:12)
   Bus (5)
      GracePeriodConfirmedIntegrationEventHandler  (src/Ordering.API/Application/IntegrationEvents/EventHandling/GracePeriodConfirmedIntegrationEventHandler.cs:3)
      OrderPaymentFailedIntegrationEventHandler  (src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderPaymentFailedIntegrationEventHandler.cs:3)
      OrderPaymentSucceededIntegrationEventHandler  (src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderPaymentSucceededIntegrationEventHandler.cs:3)
      OrderStockConfirmedIntegrationEventHandler  (src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderStockConfirmedIntegrationEventHandler.cs:3)
      OrderStockRejectedIntegrationEventHandler  (src/Ordering.API/Application/IntegrationEvents/EventHandling/OrderStockRejectedIntegrationEventHandler.cs:2)
   Domain (7)
      OrderCancelledDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderCancelledDomainEventHandler.cs:3)
      OrderShippedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderShippedDomainEventHandler.cs:3)
      OrderStatusChangedToAwaitingValidationDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToAwaitingValidationDomainEventHandler.cs:3)
      OrderStatusChangedToPaidDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToPaidDomainEventHandler.cs:3)
      OrderStatusChangedToStockConfirmedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToStockConfirmedDomainEventHandler.cs:3)
      UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler.cs:3)
      ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler.cs:3)

CROSS-CUTTING
   MediatR pipeline (every command):  LoggingBehavior → TransactionBehavior → ValidatorBehavior
   Aggregates:   Buyer · Order

PACKAGES
   Web/API:  Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.AspNetCore.OpenApi, OpenTelemetry.Instrumentation.AspNetCore, Scalar.AspNetCore
   ORM/Data:  Aspire.Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Tools, Npgsql.EntityFrameworkCore.PostgreSQL
   Mediator/CQRS:  MediatR
   Messaging:  Aspire.RabbitMQ.Client
   Validation:  FluentValidation, FluentValidation.DependencyInjectionExtensions
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol, OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.GrpcNetClient, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.Runtime
   Other:  Asp.Versioning.Http, Asp.Versioning.Mvc.ApiExplorer, Asp.Versioning.OpenApi, Microsoft.Extensions.Http.Resilience, Microsoft.Extensions.Options, Microsoft.Extensions.ServiceDiscovery, Microsoft.OpenApi, System.Reflection.TypeExtensions

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
