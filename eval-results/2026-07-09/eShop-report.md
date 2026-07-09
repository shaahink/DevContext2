# REPORT
**eShop**

Style: Microservices
_19 projects  ·  56 HttpEndpoint, 13 MessageConsumer, 7 DomainEventHandler, 29 UiEntry, 75 GrpcService  ·  net10.0, net10.0-android;net10.0-ios;net10.0-maccatalyst + blazor + controllers + desktop-ui + minimal-apis + identity + mediatr + efcore + fluentvalidation + grpc + aspire_

## Stats

| Metric | Value |
|--------|-------|
| Files | 527 |
| Projects | 24 |
| Nodes | 1132 |
| Edges | 881 |
| ServiceLinks | 5 |
| Entries | 180 |
| With target | 167/180 |
| Verified edges | 72% |
| Analyzed in | 21.6s |

## Top Flows

1. **PUT /api/catalog/items** → `CatalogApi` *(HttpEndpoint)*
2. **PUT /api/catalog/items/{id:int}** → `PublishThroughEventBusAsync` *(HttpEndpoint)*
3. **GET /api/catalog/items/withsemanticrelevance/{text:minlength(1)}** → `CatalogApi` *(HttpEndpoint)*
4. **POST /Device** → `DeviceController` *(HttpEndpoint)*
5. **POST /Device** → `DeviceController` *(HttpEndpoint)*
6. **GET /api/catalog/items/withsemanticrelevance** → `CatalogAI.GetEmbeddingAsync` *(HttpEndpoint)*
7. **GET /Account** → `AccountController` *(HttpEndpoint)*
8. **GET /Account** → `AccountController` *(HttpEndpoint)*
9. **GET /Account** → `AccountController` *(HttpEndpoint)*
10. **GET /Consent** → `ConsentController` *(HttpEndpoint)*

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
         ├─ call CatalogServices  (src/Catalog.API/Apis/CatalogApi.cs:330) [approx]
         │      public class CatalogServices(
         │      CatalogContext context,
         │      [FromServices] ICatalogAI catalogAI,
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
      ├─ call CatalogServices  (src/Catalog.API/Apis/CatalogApi.cs:330) [approx]
      │      public class CatalogServices(
      │      CatalogContext context,
      │      [FromServices] ICatalogAI catalogAI,
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

### Trace 3: GET /api/catalog/items/withsemanticrelevance/{text:minlength(1)}

TRACE  GET /api/catalog/items/withsemanticrelevance/{text:minlength(1)}
       src/Catalog.API/Apis/CatalogApi.cs:53
       Catalog.API
▸ ENTRY  GET /api/catalog/items/withsemanticrelevance/{text:minlength(1)}  (src/Catalog.API/Apis/CatalogApi.cs:53)
   └─ call CatalogApi.GetItemsBySemanticRelevanceV1  (src/Catalog.API/Apis/CatalogApi.cs:53)
          [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
          public static async Task<Results<Ok<PaginatedItems<CatalogItem>>, RedirectToRouteHttpResult>> GetItemsBySemanticRelevanceV1(
          [AsParameters] PaginationRequest paginationRequest,
      └─ call CatalogApi.GetItemsBySemanticRelevance  (src/Catalog.API/Apis/CatalogApi.cs:233) [verified]
             [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
             public static async Task<Results<Ok<PaginatedItems<CatalogItem>>, RedirectToRouteHttpResult>> GetItemsBySemanticRelevance(
             [AsParameters] PaginationRequest paginationRequest,
         ├─ call CatalogServices  (src/Catalog.API/Apis/CatalogApi.cs:251) [approx]
         │      public class CatalogServices(
         │      CatalogContext context,
         │      [FromServices] ICatalogAI catalogAI,
         ├─ call CatalogServices.Where  (src/Catalog.API/Apis/CatalogApi.cs:280) [approx]
         ├─ call CatalogServices.LogDebug  (src/Catalog.API/Apis/CatalogApi.cs:274) [approx]
         ├─ call CatalogServices.IsEnabled  (src/Catalog.API/Apis/CatalogApi.cs:264) [approx]
         ├─ call CatalogServices.LongCountAsync  (src/Catalog.API/Apis/CatalogApi.cs:259) [approx]
         ├─ call CatalogApi.GetItemsByName  (src/Catalog.API/Apis/CatalogApi.cs:255) [verified]
         │      [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
         │      public static async Task<Ok<PaginatedItems<CatalogItem>>> GetItemsByName(
         │      [AsParameters] PaginationRequest paginationRequest,
         │  └─ call CatalogApi.GetAllItems  (src/Catalog.API/Apis/CatalogApi.cs:199) [verified]
         │         [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
         │         public static async Task<Ok<PaginatedItems<CatalogItem>>> GetAllItems(
         │         [AsParameters] PaginationRequest paginationRequest,
         └─ call CatalogAI.GetEmbeddingAsync  (src/Catalog.API/Apis/CatalogApi.cs:251) [verified]
                public ValueTask<Vector?> GetEmbeddingAsync(CatalogItem item) =>
                IsEnabled ?
                GetEmbeddingAsync(CatalogItemToString(item)) :
            └─ call CatalogAI.CatalogItemToString  (src/Catalog.API/Services/CatalogAI.cs:30) [verified]
                   private static string CatalogItemToString(CatalogItem item) => $"{item.Name} {item.Description}";
RESULT   200 OK · failure → 404 Not Found

---

## Insights

_2 info · 5 notable · 3 warning_

### **WARNING**: 49/56 endpoints anonymous, incl. 14 POST/PUT/DELETE
*(Risk)*

- GET /item/{itemId:int}
- GET /
- GET /External
- GET /External
- GET /Account

### **WARNING**: Missing validation: 43/56 endpoints have no FluentValidation validator
*(Risk)*

- POST /Grants → GrantsController
- GET /Grants → GrantsController
- POST /Device → DeviceController
- POST /Device → DeviceController
- GET /Device → DeviceController

### **WARNING**: Auth surface: 7 protected, 49 unannotated of 56 API endpoints
*(Risk)*

- 7 protected
- POST /Grants
- GET /Grants
- 49 no auth annotation

### **NOTABLE**: ViewModel-View: 44 VMs + 14 Views (0 call edges)
*(Wiring)*

- 44 ViewModels
- 14 Views

### **NOTABLE**: Downstream wiring: 4 target services via bus-publish→consume
*(Wiring)*

- Webhooks.API ← bus-publish→consume
- Ordering.API ← bus-publish→consume
- Basket.API ← bus-publish→consume
- WebApp ← bus-publish→consume

### **NOTABLE**: Possible dead code: 5 public types with zero inbound references
*(Wiring)*

- AppDelegate
- GracePeriodConfirmedIntegrationEvent
- ProductPriceChangedIntegrationEvent
- EventBusBuilderExtensions
- MauiProgram

### **NOTABLE**: Internal hubs: 4 heavily-referenced internal types
*(Topology)*

- OrderingApiTrace (7 refs)
- OnChangeSubscription (2 refs)
- BasketStateChangedSubscription (1 refs)
- EShopJsonSerializerContext (1 refs)

### **NOTABLE**: Most depended-upon: eShop.ServiceDefaults (9 dependents) · EventBusRabbitMQ (7 dependents) · IntegrationEventLogEF (4 dependents)
*(Topology)*

- eShop.ServiceDefaults (9 dependents)
- EventBusRabbitMQ (7 dependents)
- IntegrationEventLogEF (4 dependents)

### _INFO_: Entry targets resolved 167/180 (92%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Module map: 8 feature areas
*(Shape)*

- eShop/ClientApp/ViewModels (79 entries)
- eShop/Catalog/API (16 entries)
- IdentityServerHost/Quickstart/UI (14 entries)
- eShop/ClientApp/Animations (10 entries)
- global (7 entries)

MAP  eShop     (19 projects)

STACK  net10.0, net10.0-android;net10.0-ios;net10.0-maccatalyst, net10.0-android;net10.0-ios;net10.0-maccatalyst;net10.0 · Minimal APIs · Controllers · MediatR (CQRS) · EF Core · FluentValidation · DDD aggregates

STYLE  Microservices  (confidence high)
       evidence: Aspire orchestration with 22 service projects

       per service:
         Basket.API: gRPC Service [gRPC]
         Catalog.API: Web API [EF Core]
         ClientApp: Unknown
         HybridApp: Unknown
         Identity.API: Web API [EF Core]
         Ordering.API: Web API [EF Core, FluentValidation]
         PaymentProcessor: Unknown
         WebApp: Gateway [YARP]
         WebhookClient: Unknown
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
    [bus] Catalog.API → Ordering.API  (Catalog.API→Ordering.API:eShop.Catalog.API.IntegrationEvents.Events.OrderStockRejectedIntegrationEvent)
    [bus] Catalog.API → Webhooks.API  (Catalog.API→Webhooks.API:eShop.Catalog.API.IntegrationEvents.Events.ProductPriceChangedIntegrationEvent)
    [bus] Ordering.API → Basket.API  (Ordering.API→Basket.API:eShop.Ordering.API.Application.IntegrationEvents.Events.OrderStartedIntegrationEvent)
    [bus] Ordering.API → WebApp  (Ordering.API→WebApp:eShop.Ordering.API.Application.IntegrationEvents.Events.OrderStatusChangedToCancelledIntegrationEvent)
    [bus] PaymentProcessor → Ordering.API  (PaymentProcessor→Ordering.API:eShop.PaymentProcessor.IntegrationEvents.Events.OrderPaymentSucceededIntegrationEvent)

ENTRY POINTS
   HTTP (56)
      DELETE /api/catalog/items/{id:int}  → CatalogServices  (src/Catalog.API/Apis/CatalogApi.cs:107)
      DELETE /api/webhooks/{id:int}  → WebhooksContext  (src/Webhooks.API/Apis/WebHooksApi.cs:66)
      GET /Account  → AccountController  (src/Identity.API/Quickstart/Account/AccountController.cs:196)
      GET /Account  → AccountController  (src/Identity.API/Quickstart/Account/AccountController.cs:146)
      GET /Account  → AccountController  (src/Identity.API/Quickstart/Account/AccountController.cs:39)
      GET /api/catalog/catalogbrands  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:84)
      GET /api/catalog/catalogtypes  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:77)
      GET /api/catalog/items  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:26)
      GET /api/catalog/items  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:21)
      GET /api/catalog/items/{id:int}  → CatalogServices  (src/Catalog.API/Apis/CatalogApi.cs:36)
      GET /api/catalog/items/{id:int}/pic  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:46)
      GET /api/catalog/items/by  → CatalogServices  (src/Catalog.API/Apis/CatalogApi.cs:31)
      GET /api/catalog/items/by/{name:minlength(1)}  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:41)
      GET /api/catalog/items/type/{typeId}/brand/{brandId?}  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:67)
      GET /api/catalog/items/type/all/brand/{brandId:int?}  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:72)
      GET /api/catalog/items/withsemanticrelevance  → CatalogAI.GetEmbeddingAsync  (src/Catalog.API/Apis/CatalogApi.cs:60)
      GET /api/catalog/items/withsemanticrelevance/{text:minlength(1)}  → CatalogApi  (src/Catalog.API/Apis/CatalogApi.cs:53)
      GET /api/orders/  → OrdersApi  (src/Ordering.API/Apis/OrdersApi.cs:14)
      GET /api/orders/{orderId:int}  → OrdersApi  (src/Ordering.API/Apis/OrdersApi.cs:13)
      GET /api/orders/cardtypes  → OrdersApi  (src/Ordering.API/Apis/OrdersApi.cs:15)
      … and 36 more (http entries — use --focus for a drill-in)
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
   Domain (7)
      OrderCancelledDomainEventHandler  → OrderCancelledDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderCancelledDomainEventHandler.cs:3)
      OrderShippedDomainEventHandler  → OrderShippedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderShippedDomainEventHandler.cs:3)
      OrderStatusChangedToAwaitingValidationDomainEventHandler  → OrderStatusChangedToAwaitingValidationDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToAwaitingValidationDomainEventHandler.cs:3)
      OrderStatusChangedToPaidDomainEventHandler  → OrderStatusChangedToPaidDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToPaidDomainEventHandler.cs:3)
      OrderStatusChangedToStockConfirmedDomainEventHandler  → OrderStatusChangedToStockConfirmedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/OrderStatusChangedToStockConfirmedDomainEventHandler.cs:3)
      UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler  → UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler.cs:3)
      ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler  → ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler  (src/Ordering.API/Application/DomainEventHandlers/ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler.cs:3)
   UI (29)
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
      … and 9 more (ui entries — use --focus for a drill-in)
   gRPC (75)
      Animation.BeginAnimation  → StoryBoard  (src/ClientApp/Animations/StoryBoard.cs:5)
      Animation.BeginAnimation  → FadeOutAnimation  (src/ClientApp/Animations/FadeToAnimation.cs:98)
      Animation.BeginAnimation  → FadeInAnimation  (src/ClientApp/Animations/FadeToAnimation.cs:40)
      Animation.BeginAnimation  → FadeToAnimation  (src/ClientApp/Animations/FadeToAnimation.cs:6)
      Animation.FadeIn  → FadeInAnimation  (src/ClientApp/Animations/FadeToAnimation.cs:40)
      Animation.FadeOut  → FadeOutAnimation  (src/ClientApp/Animations/FadeToAnimation.cs:98)
      Animation.ResetAnimation  → StoryBoard  (src/ClientApp/Animations/StoryBoard.cs:5)
      Animation.ResetAnimation  → FadeOutAnimation  (src/ClientApp/Animations/FadeToAnimation.cs:98)
      Animation.ResetAnimation  → FadeInAnimation  (src/ClientApp/Animations/FadeToAnimation.cs:40)
      Animation.ResetAnimation  → FadeToAnimation  (src/ClientApp/Animations/FadeToAnimation.cs:6)
      Basket.DeleteBasket  → BasketService  (src/Basket.API/Grpc/BasketService.cs:8)
      Basket.GetBasket  → BasketService  (src/Basket.API/Grpc/BasketService.cs:8)
      Basket.MapToCustomerBasket  → BasketService  (src/Basket.API/Grpc/BasketService.cs:8)
      Basket.MapToCustomerBasketResponse  → BasketService  (src/Basket.API/Grpc/BasketService.cs:8)
      Basket.ThrowBasketDoesNotExist  → BasketService  (src/Basket.API/Grpc/BasketService.cs:8)
      Basket.ThrowNotAuthenticated  → BasketService  (src/Basket.API/Grpc/BasketService.cs:8)
      Basket.UpdateBasket  → BasketService  (src/Basket.API/Grpc/BasketService.cs:8)
      IViewModel.ApplyQueryAttributes  → ViewModelBase  (src/ClientApp/ViewModels/Base/ViewModelBase.cs:5)
      IViewModel.InitializeAsync  → ViewModelBase  (src/ClientApp/ViewModels/Base/ViewModelBase.cs:5)
      IViewModel.IsBusyFor  → ViewModelBase  (src/ClientApp/ViewModels/Base/ViewModelBase.cs:5)
      … and 55 more (grpc entries — use --focus for a drill-in)

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

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 690ms |
| GenericExtraction | 2570ms |
| SignalSealing | 0ms |
| SpecificExtraction | 6468ms |
| Compression | 33ms |
| **Total** | **21600ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 5178ms | 0 | 0 |
| SyntaxStructureExtractor | 2567ms | 523 | 154 |
| DiRegistrationExtractor | 2471ms | 0 | 154 |
| BlazorEntryExtractor | 1286ms | 0 | 221 |
| EndpointExtractor | 1158ms | 0 | 220 |
| EventBusExtractor | 602ms | 0 | 195 |
| ProgramCsFlowExtractor | 600ms | 0 | 19 |
| SourceBodyExtractor | 559ms | 0 | 0 |
| ProjectStructure | 480ms | 0 | 0 |
| EfCoreExtractor | 467ms | 0 | 179 |
| MediatRExtractor | 451ms | 0 | 163 |
| ControllerActionExtractor | 385ms | 0 | 60 |
| BodyFactsExtractor | 373ms | 0 | 0 |
| InMemoryEventBusExtractor | 371ms | 0 | 59 |
| IndirectWiringDetector | 308ms | 0 | 30 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 639 | 201 |
| Sends | 11 | 0 |
| Handles | 18 | 0 |
| Raises | 18 | 5 |
| Consumes | 25 | 0 |
| ReadsWrites | 25 | 21 |
| Resolves | 76 | 9 |
| WrappedBy | 54 | 0 |
| EntityRelation | 10 | 10 |
| ServiceLink | 5 | 0 |

_527 files · 24 projects_
