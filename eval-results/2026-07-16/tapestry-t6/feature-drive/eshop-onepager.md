# Atlas — eShop

**Archetype:** App | **Projects:** 19 | **Entries:** 109

## Services
- **Basket.API** (gRPC Service) → EventBusRabbitMQ, eShop.ServiceDefaults
  Stack: gRPC
- **Catalog.API** (Web API) → EventBusRabbitMQ, IntegrationEventLogEF, eShop.ServiceDefaults
  Stack: EF Core
- **ClientApp** (MAUI App)
  Stack: .NET MAUI
- **EventBus**
- **EventBusRabbitMQ** → EventBus
- **HybridApp** (MAUI App) → WebAppComponents
  Stack: .NET MAUI
- **Identity.API** (Web API) → eShop.ServiceDefaults
  Stack: EF Core
- **IntegrationEventLogEF** → EventBus
- **OrderProcessor** (Worker Service) → EventBusRabbitMQ, eShop.ServiceDefaults
  Stack: Worker
- **Ordering.API** (Web API) → EventBusRabbitMQ, IntegrationEventLogEF, Ordering.Domain, Ordering.Infrastructure, eShop.ServiceDefaults
  Stack: EF Core, FluentValidation
- **Ordering.Domain**
- **Ordering.Infrastructure** → IntegrationEventLogEF, Ordering.Domain
- **PaymentProcessor** (Worker Service) → EventBusRabbitMQ, eShop.ServiceDefaults
  Stack: Worker
- **WebApp** (Blazor) → EventBusRabbitMQ, WebAppComponents, eShop.ServiceDefaults
  Stack: Blazor, YARP
- **WebAppComponents**
- **WebhookClient** (Blazor) → eShop.ServiceDefaults
  Stack: Blazor
- **Webhooks.API** (Web API) → EventBusRabbitMQ, IntegrationEventLogEF, eShop.ServiceDefaults
  Stack: EF Core
- **eShop.AppHost** (Aspire AppHost) → Basket.API, Catalog.API, Identity.API, OrderProcessor, Ordering.API, PaymentProcessor, WebApp, WebhookClient, Webhooks.API
  Stack: Aspire
- **eShop.ServiceDefaults**

## Top Flows
- **ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler** (32 steps, 3 deep, 0 cross-service)
- **OrderCancelledDomainEventHandler** (28 steps, 3 deep, 0 cross-service)
- **PUT /api/catalog/items/{id:int}** (25 steps, 3 deep, 0 cross-service)
- **OrderShippedDomainEventHandler** (25 steps, 3 deep, 0 cross-service)
- **OrderStatusChangedToPaidDomainEventHandler** (23 steps, 3 deep, 0 cross-service)

## Event Wiring (15 wires, 8 cross-service)
- VerifyOrAddPaymentMethod → **BuyerAndPaymentMethodVerifiedDomainEvent** → UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler
- SetCancelledStatus → **OrderCancelledDomainEvent** → OrderCancelledDomainEventHandler
- Handle → **OrderPaymentFailedIntegrationEvent** → OrderPaymentFailedIntegrationEventHandler _(cross-service)_
- Handle → **OrderPaymentSucceededIntegrationEvent** → OrderPaymentSucceededIntegrationEventHandler _(cross-service)_
- SetShippedStatus → **OrderShippedDomainEvent** → OrderShippedDomainEventHandler
- AddOrderStartedDomainEvent → **OrderStartedDomainEvent** → ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler
- Handle → **OrderStartedIntegrationEvent** → OrderStartedIntegrationEventHandler _(cross-service)_
- SetAwaitingValidationStatus → **OrderStatusChangedToAwaitingValidationDomainEvent** → OrderStatusChangedToAwaitingValidationDomainEventHandler
- Handle → **OrderStatusChangedToCancelledIntegrationEvent** → OrderStatusChangedToCancelledIntegrationEventHandler _(cross-service)_
- SetPaidStatus → **OrderStatusChangedToPaidDomainEvent** → OrderStatusChangedToPaidDomainEventHandler
- SetStockConfirmedStatus → **OrderStatusChangedToStockConfirmedDomainEvent** → OrderStatusChangedToStockConfirmedDomainEventHandler
- Handle → **OrderStatusChangedToSubmittedIntegrationEvent** → OrderStatusChangedToSubmittedIntegrationEventHandler _(cross-service)_
- Handle → **OrderStockConfirmedIntegrationEvent** → OrderStockConfirmedIntegrationEventHandler _(cross-service)_
- Handle → **OrderStockRejectedIntegrationEvent** → OrderStockRejectedIntegrationEventHandler _(cross-service)_
- CatalogApi.UpdateItem → **ProductPriceChangedIntegrationEvent** → ProductPriceChangedIntegrationEventHandler _(cross-service)_

## Hub Radar
- Service.WebApp: 19 flows (in 1, out 0)
- Basket.API: 19 flows (in 1, out 0)
- global.CatalogServices: 8 flows (in 6, out 0)
- IntegrationEventLogService.SaveEventAsync: 7 flows (in 2, out 0)
- Services.IIntegrationEventLogService: 7 flows (in 4, out 2)
- ViewModels.CatalogViewModel: 7 flows (in 7, out 0)
- global.OrderServices: 6 flows (in 6, out 0)
- Webhooks.API: 6 flows (in 1, out 0)
- Ordering.API: 6 flows (in 2, out 2)
- OrderStatusNotificationService.NotifyOrderStatusChangedAsync: 6 flows (in 6, out 1)

## Pipeline Behaviors
- LoggingBehavior
- TransactionBehavior
- ValidatorBehavior
