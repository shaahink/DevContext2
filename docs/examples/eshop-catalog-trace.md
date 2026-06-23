TRACE  POST /api/catalog/items
       src/Catalog.API/Apis/CatalogApi.cs:103

▸ ENTRY  POST /api/catalog/items  (src/Catalog.API/Apis/CatalogApi.cs:103)
   └─ call CatalogApi.CreateItem  (src/Catalog.API/Apis/CatalogApi.cs:103)
      ├─ raises ProductPriceChangedIntegrationEvent  (src/Catalog.API/Apis/CatalogApi.cs:342) [approx]
      │      //Create Integration Event to be published through the Event Bus
      │      var priceChangedEvent = new ProductPriceChangedIntegrationEvent(catalogItem.Id, productToUpdate.Price, priceEntry.OriginalValue);
      ├─ data CatalogType [approx]
      │  └─ data CatalogContext  (src/Catalog.API/Infrastructure/CatalogContext.cs:8)
      │         public int Id { get; set; }
      │     └─ data CatalogType [approx]
      │        (truncated — more edges beyond depth/fan-out)
      ├─ call CatalogServices  (src/Catalog.API/Apis/CatalogApi.cs:394) [approx]
      │      services.Context.CatalogItems.Remove(item);
      │      await services.Context.SaveChangesAsync();
      │      return TypedResults.NoContent();
      ├─ call CatalogContext  (src/Catalog.API/Apis/CatalogApi.cs:402) [verified]
      │      ".gif" => "image/gif",
      │      ".jpg" or ".jpeg" => "image/jpeg",
      │      ".bmp" => "image/bmp",
      │  (truncated — more edges beyond depth/fan-out)
      ├─ call CatalogAI  (src/Catalog.API/Apis/CatalogApi.cs:382) [verified]
      │      public static async Task<Results<NoContent, NotFound>> DeleteItemById(
      │      [AsParameters] CatalogServices services,
      │  └─ data CatalogItem [approx]
      │     ├─ data CatalogContext  (src/Catalog.API/Infrastructure/CatalogContext.cs:8)
      │     │      public string Name { get; set; }
      │     │      public string? Description { get; set; }
      │     │  (truncated — more edges beyond depth/fan-out)
      │     └─ data CatalogType [approx]
      │        (truncated — more edges beyond depth/fan-out)
      └─ call CatalogIntegrationEventService  (src/Catalog.API/Apis/CatalogApi.cs:353) [verified]
             await services.Context.SaveChangesAsync();
             }
             return TypedResults.Created($"/api/catalog/items/{id}");
         ├─ call ResilientTransaction  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:34) [verified]
         │      await ResilientTransaction.New(catalogContext).ExecuteAsync(async () =>
         │      {
         │      // Achieving atomicity between original catalog database operation and the IntegrationEventLog thanks to a local transaction
         ├─ call CatalogContext  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:37) [verified]
         │      await catalogContext.SaveChangesAsync();
         │      await integrationEventLogService.SaveEventAsync(evt, catalogContext.Database.CurrentTransaction);
         │      });
         │  (truncated — more edges beyond depth/fan-out)
         ├─ call IntegrationEventLogService  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:38) [verified]
         │      await integrationEventLogService.SaveEventAsync(evt, catalogContext.Database.CurrentTransaction);
         │      });
         │      }
         │  └─ raises IntegrationEventLogEntry  (src/IntegrationEventLogEF/Services/IntegrationEventLogService.cs:37) [approx]
         │         var eventLogEntry = new IntegrationEventLogEntry(@event, transaction.TransactionId);
         └─ call RabbitMQEventBus  (src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:18) [verified]
                await eventBus.PublishAsync(evt);
                await integrationEventLogService.MarkEventAsPublishedAsync(evt.Id);
                }
            ├─ call EventBusSubscriptionInfo  (src/EventBusRabbitMQ/RabbitMQEventBus.cs:192) [approx]
            │      // Get all the handlers using the event type as the key
            │      foreach (var handler in scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType))
            └─ call OrderStatusChangedToAwaitingValidationIntegrationEventHandler  (src/EventBusRabbitMQ/RabbitMQEventBus.cs:206) [verified]
                   }
                   [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
               ├─ raises OrderStockRejectedIntegrationEvent  (src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToAwaitingValidationIntegrationEventHandler.cs:27) [approx]
               │      var confirmedIntegrationEvent = confirmedOrderStockItems.Any(c => !c.HasStock)
               │      ? (IntegrationEvent)new OrderStockRejectedIntegrationEvent(@event.OrderId, confirmedOrderStockItems)
               │      : new OrderStockConfirmedIntegrationEvent(@event.OrderId);
               ├─ raises OrderStockConfirmedIntegrationEvent  (src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToAwaitingValidationIntegrationEventHandler.cs:28) [approx]
               │      ? (IntegrationEvent)new OrderStockRejectedIntegrationEvent(@event.OrderId, confirmedOrderStockItems)
               │      : new OrderStockConfirmedIntegrationEvent(@event.OrderId);
               ├─ data CatalogItem [approx]
               │  (truncated — more edges beyond depth/fan-out)
               ├─ call CatalogContext  (src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToAwaitingValidationIntegrationEventHandler.cs:17) [approx]
               │      var catalogItem = catalogContext.CatalogItems.Find(orderStockItem.ProductId);
               │      if (catalogItem is not null)
               │      {
               │  (truncated — more edges beyond depth/fan-out)
               └─ call CatalogIntegrationEventService  (src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToAwaitingValidationIntegrationEventHandler.cs:31) [verified]
                      await catalogIntegrationEventService.SaveEventAndCatalogContextChangesAsync(confirmedIntegrationEvent);
                      await catalogIntegrationEventService.PublishThroughEventBusAsync(confirmedIntegrationEvent);
                      }
                  (truncated — more edges beyond depth/fan-out)

TOUCHES  CatalogType, CatalogItem, CatalogBrand
EMITS    ProductPriceChangedIntegrationEvent, IntegrationEventLogEntry, OrderStockRejectedIntegrationEvent, OrderStockConfirmedIntegrationEvent
RESULT   200 OK / 201 Created · failure → 400 Bad Request
NEXT     status transition
