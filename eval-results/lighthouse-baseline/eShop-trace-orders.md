Slicing from POST /api/orders/ - handler resolved after scan.
Analyzing project...

TRACE  POST /api/orders/
       src/Ordering.API/Apis/OrdersApi.cs:17

? ENTRY  POST /api/orders/  (src/Ordering.API/Apis/OrdersApi.cs:17)
   ÀÄ call OrdersApi.CreateOrderAsync  (src/Ordering.API/Apis/OrdersApi.cs:17)
      ÃÄ send CreateOrderCommand  (src/Ordering.API/Apis/OrdersApi.cs:118) 
[approx]
      ³      pipeline ? LoggingBehavior  ValidatorBehavior  
TransactionBehavior
      ³  ÀÄ handler CreateOrderCommandHandler  
(src/Ordering.API/Application/Commands/CreateOrderCommandHandler.cs:6)
      ³         {
      ³         [DataMember]
      ³         private readonly List<OrderItemDTO> _orderItems;
      ³     ÃÄ raises OrderStartedIntegrationEvent  
(src/Ordering.API/Application/Commands/CreateOrderCommandHandler.cs:29) [approx]
      ³     ³      // Add Integration event to clean the basket
      ³     ³      var orderStartedIntegrationEvent = new 
OrderStartedIntegrationEvent(message.UserId);
      ³     ³      await 
_orderingIntegrationEventService.AddAndSaveEventAsync(orderStartedIntegrationEve
nt);
      ³     ³  ÃÄ consumes OrderStartedIntegrationEventHandler  
(src/Basket.API/IntegrationEvents/EventHandling/OrderStartedIntegrationEventHand
ler.cs:6)
      ³     ³  ³      public record OrderStartedIntegrationEvent(string UserId) 
: IntegrationEvent;
      ³     ³  ³  ÀÄ data Order [approx]
      ³     ³  ³     (stopped at depth 6; 4 branches omitted)
      ³     ³  ÀÄ data Order [approx]
      ³     ³     (stopped at depth 5; 4 branches omitted)
      ³     ÃÄ data CardType [approx]
      ³     ³  ÀÄ data OrderingContext  
(src/Ordering.Infrastructure/OrderingContext.cs:10)
      ³     ÃÄ call OrderingIntegrationEventService.AddAndSaveEventAsync  
(src/Ordering.API/Application/Commands/CreateOrderCommandHandler.cs:33) 
[verified]
      ³     ³      // Add/Update the Buyer AggregateRoot
      ³     ³      // DDD patterns comment: Add child entities and value-objects
through the Order Aggregate-Root
      ³     ³      // methods and constructor so validations, invariants and 
business logic
      ³     ³  ÃÄ call IntegrationEventLogService.SaveEventAsync  
(src/Ordering.API/Application/IntegrationEvents/OrderingIntegrationEventService.
cs:40) [verified]
      ³     ³  ³      await _eventLogService.SaveEventAsync(evt, 
_orderingContext.GetCurrentTransaction());
      ³     ³  ³      }
      ³     ³  ³      }
      ³     ³  ³  ÀÄ raises IntegrationEventLogEntry  
(src/IntegrationEventLogEF/Services/IntegrationEventLogService.cs:37) [approx]
      ³     ³  ³         var eventLogEntry = new 
IntegrationEventLogEntry(@event, transaction.TransactionId);
      ³     ³  ÀÄ call OrderingContext.GetCurrentTransaction  
(src/Ordering.API/Application/IntegrationEvents/OrderingIntegrationEventService.
cs:40) [verified]
      ³     ³         await _eventLogService.SaveEventAsync(evt, 
_orderingContext.GetCurrentTransaction());
      ³     ³         }
      ³     ³         }
      ³     ÃÄ call Order.AddOrderItem  
(src/Ordering.API/Application/Commands/CreateOrderCommandHandler.cs:44) 
[verified]
      ³     ³      _logger.LogInformation("Creating Order - Order: {@Order}", 
order);
      ³     ³  ÃÄ data OrderItem [approx]
      ³     ³  ³  ÀÄ data OrderingContext  
(src/Ordering.Infrastructure/OrderingContext.cs:10)
      ³     ³  ³         public decimal UnitPrice { get; set; }
      ³     ³  ³         [JsonPropertyName("productname")]
      ³     ³  ÃÄ call OrderItem.SetNewDiscount  
(src/Ordering.Domain/AggregatesModel/OrderAggregate/Order.cs:80) [verified]
      ³     ³  ³      existingOrderForProduct.AddUnits(units);
      ³     ³  ³      }
      ³     ³  ³  ÀÄ data Order [approx]
      ³     ³  ³     (stopped at depth 6; 4 branches omitted)
      ³     ³  ÀÄ call OrderItem.AddUnits  
(src/Ordering.Domain/AggregatesModel/OrderAggregate/Order.cs:83) [verified]
      ³     ³         else
      ³     ³         {
      ³     ³         //add validated new order item
      ³     ³     ÀÄ data Order [approx]
      ³     ³        (stopped at depth 6; 4 branches omitted)
      ³     ÃÄ call OrderRepository.Add  
(src/Ordering.API/Application/Commands/CreateOrderCommandHandler.cs:49) 
[verified]
      ³     ³      return await 
_orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
      ³     ³      }
      ³     ³      }
      ³     ³  ÃÄ data Order [approx]
      ³     ³  ³  (stopped at depth 5; 4 branches omitted)
      ³     ³  ÀÄ call OrderingContext.Add  
(src/Ordering.Infrastructure/Repositories/OrderRepository.cs:17) [approx]
      ³     ³         return _context.Orders.Add(order).Entity;
      ³     ³         }
      ³     ÀÄ call OrderingContext.SaveEntitiesAsync  
(src/Ordering.API/Application/Commands/CreateOrderCommandHandler.cs:51) 
[verified]
      ³            }
      ³        ÀÄ call OrderingContext.SaveChangesAsync  
(src/Ordering.Infrastructure/OrderingContext.cs:59) [approx]
      ³               return true;
      ³               }
      ÃÄ send IdentifiedCommand  (src/Ordering.API/Apis/OrdersApi.cs:152) 
[approx]
      ÃÄ raises CreateOrderCommand  (src/Ordering.API/Apis/OrdersApi.cs:138) 
[approx]
      ³  (stopped at depth 2; 1 branch omitted)
      ÀÄ data CardType [approx]
         (stopped at depth 2; 1 branch omitted)

TOUCHES  CardType, Order (root), OrderItem (depth 1 from Order), Buyer (root), 
PaymentMethod (depth 1 from Buyer)
EMITS    CreateOrderCommand, OrderStartedIntegrationEvent, 
IntegrationEventLogEntry
RESULT   200 OK / 201 Created ú failure  400 Bad Request
NEXT     initial state

analyzed 527 files ú 1734 nodes ú 1011 edges ú 121 entries ú 108/121 target ú 
depth 6 ú ~1394 tokens ú 3.9s stage2 x2.2 stage3 x1.7
ÚÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³  Metric  ³        Value         ³
ÃÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ Solution ³      eShop.slnx      ³
³   Time   ³        4386ms        ³
³  Tokens  ³ ~1394 (budget 8000)  ³
³ Version  ³ v1.0.5-preview.0.244 ³
ÀÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ
