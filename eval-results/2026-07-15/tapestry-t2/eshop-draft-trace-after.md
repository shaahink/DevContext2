Slicing from POST /api/orders/draft — handler resolved after scan.
Analyzing project...

TRACE  POST /api/orders/draft
       src/Ordering.API/Apis/OrdersApi.cs:16
       Ordering.API
▸ ENTRY  POST /api/orders/draft  (src/Ordering.API/Apis/OrdersApi.cs:16)
   └─ call OrdersApi.CreateOrderDraftAsync  
(src/Ordering.API/Apis/OrdersApi.cs:16)
          public static async Task<OrderDraftDTO> 
CreateOrderDraftAsync(CreateOrderDraftCommand command, [AsParameters] 
OrderServices services)
          services.Logger.LogInformation(
          "Sending command: {CommandName} - {IdProperty}: {CommandId} 
({@Command})",
      ├─ send CreateOrderDraftCommand  (src/Ordering.API/Apis/OrdersApi.cs:115) 
[verified]
      │      public record CreateOrderDraftCommand(string BuyerId, 
IEnumerable<BasketItem> Items) : IRequest<OrderDraftDTO>;
      │      pipeline ▸ LoggingBehavior → ValidatorBehavior → 
TransactionBehavior
      │  ├─ handler CreateOrderDraftCommandHandler  
(src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:7)
      │  │      // Regular CommandHandler
      │  │      public class CreateOrderDraftCommandHandler
      │  │      : IRequestHandler<CreateOrderDraftCommand, OrderDraftDTO>
      │  │  ├─ data Order  
(src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:12) 
[verified]
      │  │  │      public record Order
      │  │  │      public int OrderNumber { get; init; }
      │  │  │      public DateTime Date { get; init; }
      │  │  │  ├─ ? Basket.API  
(Ordering.API→Basket.API:eShop.Ordering.API.Application.IntegrationEvents.Events
.OrderStartedIntegrationEvent)
      │  │  │  └─ ? WebApp  
(Ordering.API→WebApp:eShop.Ordering.API.Application.IntegrationEvents.Events.Ord
erStatusChangedToCancelledIntegrationEvent)
      │  │  ├─ call CreateOrderDraftCommand  
(src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:13) 
[approx]
      │  │  │      public record CreateOrderDraftCommand(string BuyerId, 
IEnumerable<BasketItem> Items) : IRequest<OrderDraftDTO>;
      │  │  │  (stopped at depth 4; 3 branches omitted)
      │  │  ├─ call FromOrder  
(src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:19) 
[verified]
      │  │  │      public static OrderDraftDTO FromOrder(Order order)
      │  │  │      return new OrderDraftDTO()
      │  │  │      OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
      │  │  │  ├─ data Order  
(src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:32) 
[approx]
      │  │  │  │      public record Order
      │  │  │  │      public int OrderNumber { get; init; }
      │  │  │  │      public DateTime Date { get; init; }
      │  │  │  │  (stopped at depth 5; 2 branches omitted)
      │  │  │  ├─ call Order  
(src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:32) 
[approx]
      │  │  │  │      public record Order
      │  │  │  │      public int OrderNumber { get; init; }
      │  │  │  │      public DateTime Date { get; init; }
      │  │  │  │  (stopped at depth 5; 2 branches omitted)
      │  │  │  ├─ call Order.GetTotal  
(src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:41) 
[verified]
      │  │  │  │      public decimal GetTotal() => _orderItems.Sum(o => o.Units 
* o.UnitPrice);
      │  │  │  └─ call Order.Select  
(src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:32) 
[approx]
      │  │  ├─ call AddOrderItem  
(src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:16) 
[verified]
      │  │  │      public void AddOrderItem(int productId, string productName, 
decimal unitPrice, decimal discount, string pictureUrl, int units = 1)
      │  │  │      var existingOrderForProduct = _orderItems.SingleOrDefault(o 
=> o.ProductId == productId);
      │  │  │      if (existingOrderForProduct != null)
      │  │  │  ├─ call OrderItem.AddUnits  
(src/Ordering.Domain/AggregatesModel/OrderAggregate/Order.cs:83) [verified]
      │  │  │  │      public void AddUnits(int units)
      │  │  │  │      if (units < 0)
      │  │  │  │      throw new OrderingDomainException("Invalid units");
      │  │  │  └─ call OrderItem.SetNewDiscount  
(src/Ordering.Domain/AggregatesModel/OrderAggregate/Order.cs:80) [verified]
      │  │  │         public void SetNewDiscount(decimal discount)
      │  │  │         if (discount < 0)
      │  │  │         throw new OrderingDomainException("Discount is not 
valid");
      │  │  ├─ call BasketItem.ToOrderItemDTO  
(src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:13) 
[verified]
      │  │  ├─ call CreateOrderDraftCommand.Select  
(src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:13) 
[approx]
      │  │  ├─ call NewDraft  
(src/Ordering.API/Application/Commands/CreateOrderDraftCommandHandler.cs:12) 
[verified]
      │  │  │      public static Order NewDraft()
      │  │  │      var order = new Order
      │  │  │      _isDraft = true
      │  │  ├─ ? Basket.API  
(Ordering.API→Basket.API:eShop.Ordering.API.Application.IntegrationEvents.Events
.OrderStartedIntegrationEvent)
      │  │  └─ ? WebApp  
(Ordering.API→WebApp:eShop.Ordering.API.Application.IntegrationEvents.Events.Ord
erStatusChangedToCancelledIntegrationEvent)
      │  ├─ ? Basket.API  
(Ordering.API→Basket.API:eShop.Ordering.API.Application.IntegrationEvents.Events
.OrderStartedIntegrationEvent)
      │  └─ ? WebApp  
(Ordering.API→WebApp:eShop.Ordering.API.Application.IntegrationEvents.Events.Ord
erStatusChangedToCancelledIntegrationEvent)
      ├─ call OrderServices  (src/Ordering.API/Apis/OrdersApi.cs:108) [approx]
      │      public class OrderServices(
      │      IMediator mediator,
      │      IOrderQueries queries,
      │  ├─ ? Basket.API  
(Ordering.API→Basket.API:eShop.Ordering.API.Application.IntegrationEvents.Events
.OrderStartedIntegrationEvent)
      │  └─ ? WebApp  
(Ordering.API→WebApp:eShop.Ordering.API.Application.IntegrationEvents.Events.Ord
erStatusChangedToCancelledIntegrationEvent)
      ├─ call CreateOrderDraftCommand  (src/Ordering.API/Apis/OrdersApi.cs:110) 
[approx]
      │      public record CreateOrderDraftCommand(string BuyerId, 
IEnumerable<BasketItem> Items) : IRequest<OrderDraftDTO>;
      │  (stopped at depth 2; 3 branches omitted)
      └─ call ILogger  (src/Ordering.API/Apis/OrdersApi.cs:111) [verified]

TOUCHES  Order (root)
RESULT   200 OK / 201 Created · failure → 400 Bad Request

analyzed 527 files · 1024 nodes · 732 edges · 109 entries · 96/109 →target · 
depth 5 · ~1606 tokens · 16.9s stage2 ×2.6 stage3 ×1.7
╭──────────┬──────────────────────╮
│  Metric  │        Value         │
├──────────┼──────────────────────┤
│ Solution │      eShop.slnx      │
│   Time   │       17390ms        │
│  Tokens  │ ~1606 (budget 8000)  │
│ Version  │ v1.0.5-preview.0.256 │
╰──────────┴──────────────────────╯
