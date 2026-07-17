# REPORT
**eShop**

Style: Microservices
_19 projects  ·  43 HttpEndpoint, 13 MessageConsumer, 1 HostedService, 7 DomainEventHandler, 42 UiEntry, 3 GrpcService  ·  net10.0, net10.0-android;net10.0-ios;net10.0-maccatalyst + blazor + controllers + desktop-ui + minimal-apis + identity + mediatr + efcore + fluentvalidation + grpc + aspire_

## Stats

| Metric | Value |
|--------|-------|
| Files | 527 |
| Projects | 24 |
| Nodes | 1089 |
| Edges | 837 |
| ServiceLinks | 5 |
| Entries | 109 |
| With target | 96/109 |
| Deep spine (>=2) | 96/109 (88%) |
| Verified edges | 70% |
| Analyzed in | 23.4s |

## Top Flows

1. **PUT /api/catalog/items** → `CatalogApi` *(HttpEndpoint)*
2. **PUT /api/catalog/items/{id:int}** → `CatalogIntegrationEventService.PublishThroughEventBusAsync` *(HttpEndpoint)*
3. **POST /api/orders/draft** → `CreateOrderDraftCommand` *(HttpEndpoint)*
4. **POST /api/orders/** → `IdentifiedCommand` *(HttpEndpoint)*
5. **PUT /api/orders/cancel** → `IdentifiedCommand` *(HttpEndpoint)*
6. **PUT /api/orders/ship** → `IdentifiedCommand` *(HttpEndpoint)*
7. **GET /api/catalog/items/withsemanticrelevance/{text:minlength(1)}** → `CatalogApi` *(HttpEndpoint)*
8. **GET /api/catalog/items/withsemanticrelevance** → `CatalogAI.GetEmbeddingAsync` *(HttpEndpoint)*
9. **POST /Device** → `DeviceController` *(HttpEndpoint)*
10. **Basket.UpdateBasket** → `RedisBasketRepository.UpdateBasketAsync` *(GrpcService)*

### Trace 1: PUT /api/catalog/items

TRACE  PUT /api/catalog/items
       src/Catalog.API/Apis/CatalogApi.cs:93
       Catalog.API
▸ ENTRY  PUT /api/catalog/items  (src/Catalog.API/Apis/CatalogApi.cs:93)
   └─ call CatalogApi.UpdateItemV1  (src/Catalog.API/Apis/CatalogApi.cs:93)
          public static async Task<Results<Created, BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> UpdateItemV1(
          HttpContext httpContext,
          [AsParameters] CatalogServices services,
      └─ call CatalogApi.UpdateItem  (src/Catalog.API/Apis/CatalogApi.cs:321) [verified]
             public static async Task<Results<Created, BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> UpdateItem(
             HttpContext httpContext,
             [Description("The id of the catalog item to delete")] int id,
         ├─ raises ProductPriceChangedIntegrationEvent  (src/Catalog.API/Apis/CatalogApi.cs:350) [approx]
         │      // Integration Events notes:
         │      // An Event is “something that has happened in the past”, therefore its name has to be past tense
         │      // An Integration Event is an event that can cause side effects to other microservices, Bounded-Contexts or external systems.
         │  ├─ ? Ordering.API  (src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToAwaitingValidationIntegrationEventHandler.cs:29 raises OrderStockConfirmedIntegrationEvent)
         │  │  ├─ ? Basket.API  (src/Ordering.API/Application/Commands/CreateOrderCommandHandler.cs:32 raises OrderStartedIntegrationEvent)
         │  │  └─ ? WebApp  (src/Ordering.API/Application/DomainEventHandlers/OrderCancelledDomainEventHandler.cs:30 raises OrderStatusChangedToCancelledIntegrationEvent)
         │  └─ ? Webhooks.API  (src/Catalog.API/Apis/CatalogApi.cs:350 raises ProductPriceChangedIntegrationEvent)
         ├─ call CatalogServices  (src/Catalog.API/Apis/CatalogApi.cs:330) [approx]
         │      public class CatalogServices(
         │      CatalogContext context,
         │      [FromServices] ICatalogAI catalogAI,
         │  ├─ ? Ordering.API  (src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToAwaitingValidationIntegrationEventHandler.cs:29 raises OrderStockConfirmedIntegrationEvent)
         │  │  (stopped at depth 4; 2 branches omitted)
         │  └─ ? Webhooks.API  (src/Catalog.API/Apis/CatalogApi.cs:350 raises ProductPriceChangedIntegrationEvent)
         ├─ call CatalogContext.SaveChangesAsync  (src/Catalog.API/Apis/CatalogApi.cs:360) [verified]
         ├─ call PublishThroughEventBusAsync  (src/Catalog.API/Apis/CatalogApi.cs:356) [verified]
         │      public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
         │      try
         │      logger.LogInformation("Publishing integration event: {IntegrationEventId_published} - ({@IntegrationEvent})", evt.Id, evt);
         │  ├─ call IIntegrationEventLogService  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:17) [approx]
         │  │      public interface IIntegrationEventLogService
         │  │      Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId);
         │  │      Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction);
         │  │  ├─ di IntegrationEventLogService<OrderingContext>  (src/Ordering.API/Extensions/Extensions.cs:24)
         │  │  └─ di IntegrationEventLogService<CatalogContext>  (src/Catalog.API/Extensions/Extensions.cs:27)
         │  ├─ call IntegrationEventLogService.MarkEventAsFailedAsync  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:24) [verified]
         │  │      public Task MarkEventAsFailedAsync(Guid eventId)
         │  │      return UpdateEventStatus(eventId, EventStateEnum.PublishedFailed);
         │  │  └─ call IntegrationEventLogService.UpdateEventStatus  (src/IntegrationEventLogEF/Services/IntegrationEventLogService.cs:58) [verified]
         │  │         private Task UpdateEventStatus(Guid eventId, EventStateEnum status)
         │  │         var eventLogEntry = _context.Set<IntegrationEventLogEntry>().Single(ie => ie.EventId == eventId);
         │  │         eventLogEntry.State = status;
         │  ├─ call IntegrationEventLogService.MarkEventAsPublishedAsync  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:19) [verified]
         │  │      public Task MarkEventAsPublishedAsync(Guid eventId)
         │  │      return UpdateEventStatus(eventId, EventStateEnum.Published);
         │  │  └─ call IntegrationEventLogService.UpdateEventStatus  (src/IntegrationEventLogEF/Services/IntegrationEventLogService.cs:48) [verified]
         │  │         private Task UpdateEventStatus(Guid eventId, EventStateEnum status)
         │  │         var eventLogEntry = _context.Set<IntegrationEventLogEntry>().Single(ie => ie.EventId == eventId);
         │  │         eventLogEntry.State = status;
         │  ├─ call RabbitMQEventBus.PublishAsync  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:18) [verified]
         │  │      public async Task PublishAsync(IntegrationEvent @event)
         │  │      var routingKey = @event.GetType().Name;
         │  │      if (logger.IsEnabled(LogLevel.Trace))
         │  │  ├─ call RabbitMQEventBus.SetActivityContext  (src/EventBusRabbitMQ/RabbitMQEventBus.cs:88) [verified]
         │  │  │      private static void SetActivityContext(Activity activity, string routingKey, string operation)
         │  │  │      if (activity is not null)
         │  │  │      // These tags are added demonstrating the semantic conventions of the OpenTelemetry messaging specification
         │  │  └─ call RabbitMQEventBus.SerializeMessage  (src/EventBusRabbitMQ/RabbitMQEventBus.cs:51) [verified]
         │  │         [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
         │  │         Justification = "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
         │  │         [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
         │  └─ call IntegrationEventLogService.MarkEventAsInProgressAsync  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:17) [verified]
         │         public Task MarkEventAsInProgressAsync(Guid eventId)
         │         return UpdateEventStatus(eventId, EventStateEnum.InProgress);
         │     └─ call IntegrationEventLogService.UpdateEventStatus  (src/IntegrationEventLogEF/Services/IntegrationEventLogService.cs:53) [verified]
         │            private Task UpdateEventStatus(Guid eventId, EventStateEnum status)
         │            var eventLogEntry = _context.Set<IntegrationEventLogEntry>().Single(ie => ie.EventId == eventId);
         │            eventLogEntry.State = status;
         ├─ call SaveEventAndCatalogContextChangesAsync  (src/Catalog.API/Apis/CatalogApi.cs:353) [verified]
         │      public async Task SaveEventAndCatalogContextChangesAsync(IntegrationEvent evt)
         │      logger.LogInformation("CatalogIntegrationEventService - Saving changes and integrationEvent: {IntegrationEventId}", evt.Id);
         │      //Use of an EF Core resiliency strategy when using multiple DbContexts within an explicit BeginTransaction():
         │  ├─ call IIntegrationEventLogService  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:38) [approx]
         │  │      public interface IIntegrationEventLogService
         │  │      Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId);
         │  │      Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction);
         │  │  (stopped at depth 4; 2 branches omitted)
         │  ├─ call IntegrationEventLogService.SaveEventAsync  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:38) [verified]
         │  │      public Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction)
         │  │      if (transaction == null) throw new ArgumentNullException(nameof(transaction));
         │  │      var eventLogEntry = new IntegrationEventLogEntry(@event, transaction.TransactionId);
         │  ├─ call CatalogContext.SaveChangesAsync  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:37) [verified]
         │  ├─ call ResilientTransaction.New  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:34) [verified]
         │  │      public static ResilientTransaction New(DbContext context) => new(context);
         │  └─ call ResilientTransaction.ExecuteAsync  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:34) [verified]
         │         public async Task ExecuteAsync(Func<Task> action)
         │         //Use of an EF Core resiliency strategy when using multiple DbContexts within an explicit BeginTransaction():
         │         //See: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency
         │     └─ call ResilientTransaction.action  (src/IntegrationEventLogEF/Utilities/ResilientTransaction.cs:19) [approx]
         ├─ call CatalogAI.GetEmbeddingAsync  (src/Catalog.API/Apis/CatalogApi.cs:343) [verified]
         │      public ValueTask<Vector?> GetEmbeddingAsync(CatalogItem item) =>
         │      IsEnabled ?
         │      GetEmbeddingAsync(CatalogItemToString(item)) :
         │  └─ call CatalogAI.CatalogItemToString  (src/Catalog.API/Services/CatalogAI.cs:30) [verified]
         │         private static string CatalogItemToString(CatalogItem item) => $"{item.Name} {item.Description}";
         ├─ call CatalogContext.Entry  (src/Catalog.API/Apis/CatalogApi.cs:340) [verified]
         └─ call CatalogServices.SingleOrDefaultAsync  (src/Catalog.API/Apis/CatalogApi.cs:330) [approx]
RESULT   200 OK / 204 No Content · failure → 400 Bad Request

---

### Trace 2: PUT /api/catalog/items/{id:int}

TRACE  PUT /api/catalog/items/{id:int}
       src/Catalog.API/Apis/CatalogApi.cs:98
       Catalog.API
▸ ENTRY  PUT /api/catalog/items/{id:int}  (src/Catalog.API/Apis/CatalogApi.cs:98)
   └─ call CatalogApi.UpdateItem  (src/Catalog.API/Apis/CatalogApi.cs:98)
          public static async Task<Results<Created, BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> UpdateItem(
          HttpContext httpContext,
          [Description("The id of the catalog item to delete")] int id,
      ├─ raises ProductPriceChangedIntegrationEvent  (src/Catalog.API/Apis/CatalogApi.cs:350) [approx]
      │      // Integration Events notes:
      │      // An Event is “something that has happened in the past”, therefore its name has to be past tense
      │      // An Integration Event is an event that can cause side effects to other microservices, Bounded-Contexts or external systems.
      │  ├─ ? Ordering.API  (src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToAwaitingValidationIntegrationEventHandler.cs:29 raises OrderStockConfirmedIntegrationEvent)
      │  │  ├─ ? Basket.API  (src/Ordering.API/Application/Commands/CreateOrderCommandHandler.cs:32 raises OrderStartedIntegrationEvent)
      │  │  └─ ? WebApp  (src/Ordering.API/Application/DomainEventHandlers/OrderCancelledDomainEventHandler.cs:30 raises OrderStatusChangedToCancelledIntegrationEvent)
      │  └─ ? Webhooks.API  (src/Catalog.API/Apis/CatalogApi.cs:350 raises ProductPriceChangedIntegrationEvent)
      ├─ call CatalogServices  (src/Catalog.API/Apis/CatalogApi.cs:330) [approx]
      │      public class CatalogServices(
      │      CatalogContext context,
      │      [FromServices] ICatalogAI catalogAI,
      │  ├─ ? Ordering.API  (src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToAwaitingValidationIntegrationEventHandler.cs:29 raises OrderStockConfirmedIntegrationEvent)
      │  │  (stopped at depth 3; 2 branches omitted)
      │  └─ ? Webhooks.API  (src/Catalog.API/Apis/CatalogApi.cs:350 raises ProductPriceChangedIntegrationEvent)
      ├─ call CatalogContext.SaveChangesAsync  (src/Catalog.API/Apis/CatalogApi.cs:360) [verified]
      ├─ call PublishThroughEventBusAsync  (src/Catalog.API/Apis/CatalogApi.cs:356) [verified]
      │      public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
      │      try
      │      logger.LogInformation("Publishing integration event: {IntegrationEventId_published} - ({@IntegrationEvent})", evt.Id, evt);
      │  ├─ call IIntegrationEventLogService  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:17) [approx]
      │  │      public interface IIntegrationEventLogService
      │  │      Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId);
      │  │      Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction);
      │  │  ├─ di IntegrationEventLogService<OrderingContext>  (src/Ordering.API/Extensions/Extensions.cs:24)
      │  │  └─ di IntegrationEventLogService<CatalogContext>  (src/Catalog.API/Extensions/Extensions.cs:27)
      │  ├─ call IntegrationEventLogService.MarkEventAsFailedAsync  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:24) [verified]
      │  │      public Task MarkEventAsFailedAsync(Guid eventId)
      │  │      return UpdateEventStatus(eventId, EventStateEnum.PublishedFailed);
      │  │  └─ call IntegrationEventLogService.UpdateEventStatus  (src/IntegrationEventLogEF/Services/IntegrationEventLogService.cs:58) [verified]
      │  │         private Task UpdateEventStatus(Guid eventId, EventStateEnum status)
      │  │         var eventLogEntry = _context.Set<IntegrationEventLogEntry>().Single(ie => ie.EventId == eventId);
      │  │         eventLogEntry.State = status;
      │  ├─ call IntegrationEventLogService.MarkEventAsPublishedAsync  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:19) [verified]
      │  │      public Task MarkEventAsPublishedAsync(Guid eventId)
      │  │      return UpdateEventStatus(eventId, EventStateEnum.Published);
      │  │  └─ call IntegrationEventLogService.UpdateEventStatus  (src/IntegrationEventLogEF/Services/IntegrationEventLogService.cs:48) [verified]
      │  │         private Task UpdateEventStatus(Guid eventId, EventStateEnum status)
      │  │         var eventLogEntry = _context.Set<IntegrationEventLogEntry>().Single(ie => ie.EventId == eventId);
      │  │         eventLogEntry.State = status;
      │  ├─ call RabbitMQEventBus.PublishAsync  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:18) [verified]
      │  │      public async Task PublishAsync(IntegrationEvent @event)
      │  │      var routingKey = @event.GetType().Name;
      │  │      if (logger.IsEnabled(LogLevel.Trace))
      │  │  ├─ call RabbitMQEventBus.SetActivityContext  (src/EventBusRabbitMQ/RabbitMQEventBus.cs:88) [verified]
      │  │  │      private static void SetActivityContext(Activity activity, string routingKey, string operation)
      │  │  │      if (activity is not null)
      │  │  │      // These tags are added demonstrating the semantic conventions of the OpenTelemetry messaging specification
      │  │  └─ call RabbitMQEventBus.SerializeMessage  (src/EventBusRabbitMQ/RabbitMQEventBus.cs:51) [verified]
      │  │         [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
      │  │         Justification = "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
      │  │         [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
      │  └─ call IntegrationEventLogService.MarkEventAsInProgressAsync  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:17) [verified]
      │         public Task MarkEventAsInProgressAsync(Guid eventId)
      │         return UpdateEventStatus(eventId, EventStateEnum.InProgress);
      │     └─ call IntegrationEventLogService.UpdateEventStatus  (src/IntegrationEventLogEF/Services/IntegrationEventLogService.cs:53) [verified]
      │            private Task UpdateEventStatus(Guid eventId, EventStateEnum status)
      │            var eventLogEntry = _context.Set<IntegrationEventLogEntry>().Single(ie => ie.EventId == eventId);
      │            eventLogEntry.State = status;
      ├─ call SaveEventAndCatalogContextChangesAsync  (src/Catalog.API/Apis/CatalogApi.cs:353) [verified]
      │      public async Task SaveEventAndCatalogContextChangesAsync(IntegrationEvent evt)
      │      logger.LogInformation("CatalogIntegrationEventService - Saving changes and integrationEvent: {IntegrationEventId}", evt.Id);
      │      //Use of an EF Core resiliency strategy when using multiple DbContexts within an explicit BeginTransaction():
      │  ├─ call IIntegrationEventLogService  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:38) [approx]
      │  │      public interface IIntegrationEventLogService
      │  │      Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId);
      │  │      Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction);
      │  │  (stopped at depth 3; 2 branches omitted)
      │  ├─ call IntegrationEventLogService.SaveEventAsync  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:38) [verified]
      │  │      public Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction)
      │  │      if (transaction == null) throw new ArgumentNullException(nameof(transaction));
      │  │      var eventLogEntry = new IntegrationEventLogEntry(@event, transaction.TransactionId);
      │  ├─ call CatalogContext.SaveChangesAsync  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:37) [verified]
      │  ├─ call ResilientTransaction.New  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:34) [verified]
      │  │      public static ResilientTransaction New(DbContext context) => new(context);
      │  └─ call ResilientTransaction.ExecuteAsync  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:34) [verified]
      │         public async Task ExecuteAsync(Func<Task> action)
      │         //Use of an EF Core resiliency strategy when using multiple DbContexts within an explicit BeginTransaction():
      │         //See: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency
      │     └─ call ResilientTransaction.action  (src/IntegrationEventLogEF/Utilities/ResilientTransaction.cs:19) [approx]
      ├─ call CatalogAI.GetEmbeddingAsync  (src/Catalog.API/Apis/CatalogApi.cs:343) [verified]
      │      public ValueTask<Vector?> GetEmbeddingAsync(CatalogItem item) =>
      │      IsEnabled ?
      │      GetEmbeddingAsync(CatalogItemToString(item)) :
      │  └─ call CatalogAI.CatalogItemToString  (src/Catalog.API/Services/CatalogAI.cs:30) [verified]
      │         private static string CatalogItemToString(CatalogItem item) => $"{item.Name} {item.Description}";
      ├─ call CatalogContext.Entry  (src/Catalog.API/Apis/CatalogApi.cs:340) [verified]
      └─ call CatalogServices.SingleOrDefaultAsync  (src/Catalog.API/Apis/CatalogApi.cs:330) [approx]
RESULT   200 OK / 204 No Content · failure → 400 Bad Request

---

### Trace 3: POST /api/orders/draft

TRACE  POST /api/orders/draft
       src/Ordering.API/Apis/OrdersApi.cs:16
       Ordering.API
▸ ENTRY  POST /api/orders/draft  (src/Ordering.API/Apis/OrdersApi.cs:16)
   └─ call OrdersApi.CreateOrderDraftAsync  (src/Ordering.API/Apis/OrdersApi.cs:16)
          public static async Task<OrderDraftDTO> CreateOrderDraftAsync(CreateOrderDraftCommand command, [AsParameters] OrderServices services)
          services.Logger.LogInformation(
          "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
      ├─ send CreateOrderDraftCommand  (src/Ordering.API/Apis/OrdersApi.cs:115) [verified]
      │      public record CreateOrderDraftCommand(string BuyerId, IEnumerable<BasketItem> Items) : IRequest<OrderDraftDTO>;
      │      pipeline ▸ LoggingBehavior → ValidatorBehavior → TransactionBehavior
      │  ├─ handler CreateOrderDraftCommandHandler  (src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:7)
      │  │      // Regular CommandHandler
      │  │      public class CreateOrderDraftCommandHandler
      │  │      : IRequestHandler<CreateOrderDraftCommand, OrderDraftDTO>
      │  │  (1 more branch omitted beyond fan-out)
      │  │  ├─ data Order  (src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:12) [verified]
      │  │  │      public record Order
      │  │  │      public int OrderNumber { get; init; }
      │  │  │      public DateTime Date { get; init; }
      │  │  │  ├─ ? Basket.API  (src/Ordering.API/Application/Commands/CreateOrderCommandHandler.cs:32 raises OrderStartedIntegrationEvent)
      │  │  │  └─ ? WebApp  (src/Ordering.API/Application/DomainEventHandlers/OrderCancelledDomainEventHandler.cs:30 raises OrderStatusChangedToCancelledIntegrationEvent)
      │  │  ├─ call CreateOrderDraftCommand  (src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:13) [approx]
      │  │  │      public record CreateOrderDraftCommand(string BuyerId, IEnumerable<BasketItem> Items) : IRequest<OrderDraftDTO>;
      │  │  │  (stopped at depth 4; 3 branches omitted)
      │  │  ├─ call FromOrder  (src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:19) [verified]
      │  │  │      public static OrderDraftDTO FromOrder(Order order)
      │  │  │      return new OrderDraftDTO()
      │  │  │      OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
      │  │  │  ├─ data Order  (src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:32) [approx]
      │  │  │  │      public record Order
      │  │  │  │      public int OrderNumber { get; init; }
      │  │  │  │      public DateTime Date { get; init; }
      │  │  │  │  (stopped at depth 5; 2 branches omitted)
      │  │  │  ├─ call Order  (src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:32) [approx]
      │  │  │  │      public record Order
      │  │  │  │      public int OrderNumber { get; init; }
      │  │  │  │      public DateTime Date { get; init; }
      │  │  │  │  (stopped at depth 5; 2 branches omitted)
      │  │  │  ├─ call Order.GetTotal  (src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:41) [verified]
      │  │  │  │      public decimal GetTotal() => _orderItems.Sum(o => o.Units * o.UnitPrice);
      │  │  │  └─ call Order.Select  (src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:32) [approx]
      │  │  ├─ call AddOrderItem  (src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:16) [verified]
      │  │  │      public void AddOrderItem(int productId, string productName, decimal unitPrice, decimal discount, string pictureUrl, int units = 1)
      │  │  │      var existingOrderForProduct = _orderItems.SingleOrDefault(o => o.ProductId == productId);
      │  │  │      if (existingOrderForProduct != null)
      │  │  │  ├─ call OrderItem.AddUnits  (src/Ordering.Domain/AggregatesModel/OrderAggregate/Order.cs:83) [verified]
      │  │  │  │      public void AddUnits(int units)
      │  │  │  │      if (units < 0)
      │  │  │  │      throw new OrderingDomainException("Invalid units");
      │  │  │  └─ call OrderItem.SetNewDiscount  (src/Ordering.Domain/AggregatesModel/OrderAggregate/Order.cs:80) [verified]
      │  │  │         public void SetNewDiscount(decimal discount)
      │  │  │         if (discount < 0)
      │  │  │         throw new OrderingDomainException("Discount is not valid");
      │  │  ├─ call BasketItem.ToOrderItemDTO  (src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:13) [verified]
      │  │  ├─ call CreateOrderDraftCommand.Select  (src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:13) [approx]
      │  │  ├─ call NewDraft  (src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:12) [verified]
      │  │  │      public static Order NewDraft()
      │  │  │      var order = new Order
      │  │  │      _isDraft = true
      │  │  └─ ? Basket.API  (src/Ordering.API/Application/Commands/CreateOrderCommandHandler.cs:32 raises OrderStartedIntegrationEvent)
      │  ├─ ? Basket.API  (src/Ordering.API/Application/Commands/CreateOrderCommandHandler.cs:32 raises OrderStartedIntegrationEvent)
      │  └─ ? WebApp  (src/Ordering.API/Application/DomainEventHandlers/OrderCancelledDomainEventHandler.cs:30 raises OrderStatusChangedToCancelledIntegrationEvent)
      ├─ call OrderServices  (src/Ordering.API/Apis/OrdersApi.cs:108) [approx]
      │      public class OrderServices(
      │      IMediator mediator,
      │      IOrderQueries queries,
      │  ├─ ? Basket.API  (src/Ordering.API/Application/Commands/CreateOrderCommandHandler.cs:32 raises OrderStartedIntegrationEvent)
      │  └─ ? WebApp  (src/Ordering.API/Application/DomainEventHandlers/OrderCancelledDomainEventHandler.cs:30 raises OrderStatusChangedToCancelledIntegrationEvent)
      ├─ call CreateOrderDraftCommand  (src/Ordering.API/Apis/OrdersApi.cs:110) [approx]
      │      public record CreateOrderDraftCommand(string BuyerId, IEnumerable<BasketItem> Items) : IRequest<OrderDraftDTO>;
      │  (stopped at depth 2; 3 branches omitted)
      └─ call ILogger  (src/Ordering.API/Apis/OrdersApi.cs:111) [verified]

TOUCHES  Order (root)
RESULT   200 OK / 201 Created · failure → 400 Bad Request

---

## Insights

_3 info · 4 notable · 3 warning_

### **WARNING**: Missing validation: 18/18 write endpoints have no FluentValidation validator
*(Risk)*

- DELETE /api/webhooks/{id:int} → WebhooksContext
- POST /api/webhooks/ → WebhooksContext
- POST /webhook-received → HooksRepository
- POST /logout → AuthenticationEndpoints
- POST /api/orders/ → IdentifiedCommand

### **WARNING**: 36/43 endpoints anonymous, incl. 14 POST/PUT/DELETE
*(Risk)*

- DELETE /api/webhooks/{id:int}
- POST /api/webhooks/
- GET /api/webhooks/{id:int}
- GET /api/webhooks/
- POST /webhook-received

### **WARNING**: Auth surface: 7 protected, 36 unannotated of 43 API endpoints
*(Risk)*

- 7 protected
- POST /Grants
- GET /Grants
- 36 no auth annotation

### **NOTABLE**: Most depended-upon: eShop.ServiceDefaults (9 dependents) · EventBusRabbitMQ (7 dependents) · IntegrationEventLogEF (4 dependents)
*(Topology)*

- eShop.ServiceDefaults (9 dependents)
- EventBusRabbitMQ (7 dependents)
- IntegrationEventLogEF (4 dependents)

### **NOTABLE**: Event wiring: 20 events (13 integration), 8 cross-service, 0 orphan
*(Wiring)*

- OrderPaymentFailedIntegrationEvent: PaymentProcessor → Ordering.API
- OrderPaymentSucceededIntegrationEvent: PaymentProcessor → Ordering.API
- OrderStartedIntegrationEvent: Ordering.API → Basket.API
- OrderStatusChangedToCancelledIntegrationEvent: Ordering.API → WebApp
- OrderStatusChangedToSubmittedIntegrationEvent: Ordering.API → WebApp

### **NOTABLE**: External event contracts: 1 consumed but never produced internally
*(Wiring)*

- BeforeStartEvent

### **NOTABLE**: Multi-implementation interfaces: IIntegrationEventLogService (2 impls)
*(Wiring)*

- IIntegrationEventLogService (2 impls)

### _INFO_: Middleware pipeline: 3 behaviours
*(Shape)*

- ValidatorBehavior
- LoggingBehavior
- TransactionBehavior

### _INFO_: Entry targets resolved 96/109 (88%) — trace any entry for its full path
*(Coverage)*

### _INFO_: Entry surface: 43 HTTP · 42 UI · 13 Bus · 7 DomainEventHandler
*(Shape)*

- 43 HTTP
- 42 UI
- 13 Bus
- 7 DomainEventHandler

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
  OrderStatusChangedToAwaitingValidationIntegrationEvent: (external) · Catalog.API, WebApp
  OrderStatusChangedToPaidIntegrationEvent: (external) · WebApp, Webhooks.API, Catalog.API
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
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 1277ms |
| GenericExtraction | 3943ms |
| SignalSealing | 0ms |
| SpecificExtraction | 5928ms |
| Compression | 75ms |
| **Total** | **23414ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 4372ms | 0 | 0 |
| SyntaxStructureExtractor | 3934ms | 523 | 156 |
| ProgramCsFlowExtractor | 3795ms | 0 | 156 |
| DiRegistrationExtractor | 3775ms | 0 | 136 |
| BlazorEntryExtractor | 1500ms | 0 | 199 |
| EndpointExtractor | 1158ms | 0 | 196 |
| ProjectStructure | 851ms | 0 | 0 |
| EventBusExtractor | 392ms | 0 | 157 |
| SourceBodyExtractor | 342ms | 0 | 0 |
| EfCoreExtractor | 320ms | 0 | 136 |
| MediatRExtractor | 296ms | 0 | 125 |
| ControllerActionExtractor | 269ms | 0 | 57 |
| FileTreeExtractor | 241ms | 0 | 0 |
| GrpcServiceExtractor | 207ms | 0 | 139 |
| DesktopEntryExtractor | 187ms | 0 | 55 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 591 | 205 |
| Sends | 15 | 0 |
| Handles | 18 | 0 |
| Raises | 18 | 5 |
| Consumes | 25 | 0 |
| ReadsWrites | 25 | 21 |
| Resolves | 76 | 9 |
| WrappedBy | 54 | 0 |
| EntityRelation | 10 | 10 |
| ServiceLink | 5 | 0 |

_527 files · 24 projects_
