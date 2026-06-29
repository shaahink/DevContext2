Slicing from CatalogApi:UpdateItem, depth 5, call graph on.
Analyzing project...

TRACE  CatalogApi.UpdateItem
       src/Catalog.API/Apis/CatalogApi.cs

? ENTRY  CatalogApi.UpdateItem  (src/Catalog.API/Apis/CatalogApi.cs)
   ├─ raises ProductPriceChangedIntegrationEvent  
(src/Catalog.API/Apis/CatalogApi.cs:342) [approx]
   ├─ data CatalogItem [approx]
   │  ├─ data CatalogContext  
(src/Catalog.API/Infrastructure/CatalogContext.cs:8)
   │  │      public string Name { get; set; }
   │  │      public string? Description { get; set; }
   │  └─ data CatalogItem [approx]
   │     (stopped at depth 2; 2 branches omitted)
   ├─ call CatalogServices.SingleOrDefaultAsync  
(src/Catalog.API/Apis/CatalogApi.cs:330) [approx]
   ├─ call CatalogContext.Entry  (src/Catalog.API/Apis/CatalogApi.cs:340) 
[verified]
   ├─ call CatalogAI.GetEmbeddingAsync  (src/Catalog.API/Apis/CatalogApi.cs:343)
[verified]
   │  ├─ data CatalogItem [approx]
   │  │  (stopped at depth 2; 2 branches omitted)
   │  └─ call CatalogAI.CatalogItemToString  
(src/Catalog.API/Services/CatalogAI.cs:30) [verified]
   │         public async ValueTask<IReadOnlyList<Vector>?> 
GetEmbeddingsAsync(IEnumerable<CatalogItem> items)
   │         {
   │         if (IsEnabled)
   │     └─ data CatalogItem [approx]
   │        (stopped at depth 3; 2 branches omitted)
   ├─ call CatalogIntegrationEventService.SaveEventAndCatalogContextChangesAsync
(src/Catalog.API/Apis/CatalogApi.cs:353) [verified]
   │  ├─ call ResilientTransaction.ExecuteAsync  
(src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:34) 
[verified]
   │  │      await ResilientTransaction.New(catalogContext).ExecuteAsync(async 
() =>
   │  │      {
   │  │      // Achieving atomicity between original catalog database operation 
and the IntegrationEventLog thanks to a local transaction
   │  │  └─ call ResilientTransaction.action  
(src/IntegrationEventLogEF/Utilities/ResilientTransaction.cs:19) [approx]
   │  │         await action();
   │  │         await transaction.CommitAsync();
   │  │         });
   │  ├─ call ResilientTransaction.New  
(src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:34) 
[verified]
   │  │      await ResilientTransaction.New(catalogContext).ExecuteAsync(async 
() =>
   │  │      {
   │  │      // Achieving atomicity between original catalog database operation 
and the IntegrationEventLog thanks to a local transaction
   │  ├─ call CatalogContext.SaveChangesAsync  
(src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:37) 
[verified]
   │  │      await catalogContext.SaveChangesAsync();
   │  │      await integrationEventLogService.SaveEventAsync(evt, 
catalogContext.Database.CurrentTransaction);
   │  │      });
   │  └─ call IntegrationEventLogService.SaveEventAsync  
(src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:38) 
[verified]
   │         await integrationEventLogService.SaveEventAsync(evt, 
catalogContext.Database.CurrentTransaction);
   │         });
   │         }
   │     └─ raises IntegrationEventLogEntry  
(src/IntegrationEventLogEF/Services/IntegrationEventLogService.cs:37) [approx]
   │            var eventLogEntry = new IntegrationEventLogEntry(@event, 
transaction.TransactionId);
   ├─ call CatalogIntegrationEventService.PublishThroughEventBusAsync  
(src/Catalog.API/Apis/CatalogApi.cs:356) [verified]
   │  ├─ call IntegrationEventLogService.MarkEventAsInProgressAsync  
(src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:17) 
[verified]
   │  │      await 
integrationEventLogService.MarkEventAsInProgressAsync(evt.Id);
   │  │      await eventBus.PublishAsync(evt);
   │  │      await integrationEventLogService.MarkEventAsPublishedAsync(evt.Id);
   │  │  └─ call IntegrationEventLogService.UpdateEventStatus  
(src/IntegrationEventLogEF/Services/IntegrationEventLogService.cs:53) [verified]
   │  │         return UpdateEventStatus(eventId, EventStateEnum.InProgress);
   │  │         }
   │  ├─ call RabbitMQEventBus.PublishAsync  
(src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:18) 
[verified]
   │  │      await eventBus.PublishAsync(evt);
   │  │      await integrationEventLogService.MarkEventAsPublishedAsync(evt.Id);
   │  │      }
   │  │  ├─ call RabbitMQEventBus.SerializeMessage  
(src/EventBusRabbitMQ/RabbitMQEventBus.cs:51) [verified]
   │  │  │      // Depending on Sampling (and whether a listener is registered 
or not), the activity above may not be created.
   │  │  │      // If it is created, then propagate its context. If it is not 
created, the propagate the Current context, if any.
   │  │  └─ call RabbitMQEventBus.SetActivityContext  
(src/EventBusRabbitMQ/RabbitMQEventBus.cs:88) [verified]
   │  │         exchange: ExchangeName,
   │  │         routingKey: routingKey,
   │  │         mandatory: true,
   │  ├─ call IntegrationEventLogService.MarkEventAsPublishedAsync  
(src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:19) 
[verified]
   │  │      await integrationEventLogService.MarkEventAsPublishedAsync(evt.Id);
   │  │      }
   │  │      catch (Exception ex)
   │  │  └─ call IntegrationEventLogService.UpdateEventStatus  
(src/IntegrationEventLogEF/Services/IntegrationEventLogService.cs:48) [verified]
   │  │         return UpdateEventStatus(eventId, EventStateEnum.Published);
   │  │         }
   │  └─ call IntegrationEventLogService.MarkEventAsFailedAsync  
(src/Catalog.API/IntegrationEvents/CatalogIntegrationEventService.cs:24) 
[verified]
   │         await integrationEventLogService.MarkEventAsFailedAsync(evt.Id);
   │         }
   │         }
   │     └─ call IntegrationEventLogService.UpdateEventStatus  
(src/IntegrationEventLogEF/Services/IntegrationEventLogService.cs:58) [verified]
   │            return UpdateEventStatus(eventId, 
EventStateEnum.PublishedFailed);
   │            }
   └─ call CatalogContext.SaveChangesAsync  
(src/Catalog.API/Apis/CatalogApi.cs:360) [verified]

TOUCHES  CatalogItem, CatalogType, CatalogBrand
EMITS    ProductPriceChangedIntegrationEvent, IntegrationEventLogEntry

analyzed 66 files · 215 nodes · 125 edges · 18 entries · 7/18 target · depth 3 
· ~1516 tokens · 2.8s stage2 x2.8 stage3 x1.1
┌──────────┬──────────────────────┐
│  Metric  │        Value         │
├──────────┼──────────────────────┤
│ Solution │      eShop.slnx      │
│   Time   │        2844ms        │
│  Tokens  │ ~1516 (budget 8000)  │
│ Version  │ v1.0.5-preview.0.127 │
└──────────┴──────────────────────┘
