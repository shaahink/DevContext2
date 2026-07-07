TRACE  POST /basket/checkout
       Services/Basket/Basket.API/Basket/CheckoutBasket/CheckoutBasketEndpoints.cs:10
       Basket.API
▸ ENTRY  POST /basket/checkout  (Services/Basket/Basket.API/Basket/CheckoutBasket/CheckoutBasketEndpoints.cs:10)
   └─ call <lambda> POST /basket/checkout  (Services/Basket/Basket.API/Basket/CheckoutBasket/CheckoutBasketEndpoints.cs:10)
      └─ send CheckoutBasketCommand  (Services/Basket/Basket.API/Basket/CheckoutBasket/CheckoutBasketEndpoints.cs:5) [approx]
             public record CheckoutBasketCommand(BasketCheckoutDto BasketCheckoutDto)
             : ICommand<CheckoutBasketResult>;
             pipeline ▸ ValidationBehavior → LoggingBehavior
         └─ handler CheckoutBasketCommandHandler  (Services/Basket/Basket.API/Basket/CheckoutBasket/CheckoutBasketHandler.cs:21)
                public class CheckoutBasketCommandHandler
                (IBasketRepository repository, IPublishEndpoint publishEndpoint)
                : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
            ├─ raises BasketCheckoutEvent  (Services/Basket/Basket.API/Basket/CheckoutBasket/CheckoutBasketHandler.cs:41) [approx]
            │      public record BasketCheckoutEvent : IntegrationEvent
            │      public string UserName { get; set; } = default!;
            │      public Guid CustomerId { get; set; } = default!;
            │  └─ consumes BasketCheckoutEventHandler  (Services/Ordering/Ordering.Application/Orders/EventHandlers/Integration/BasketCheckoutEventHandler.cs:6)
            │         public class BasketCheckoutEventHandler
            │         (ISender sender, ILogger<BasketCheckoutEventHandler> logger)
            │         : IConsumer<BasketCheckoutEvent>
            │     └─ send CreateOrderCommand  (Services/Ordering/Ordering.Application/Orders/EventHandlers/Integration/BasketCheckoutEventHandler.cs:16) [approx]
            │            public record CreateOrderCommand(OrderDto Order)
            │            : ICommand<CreateOrderResult>;
            │            pipeline ▸ ValidationBehavior → LoggingBehavior
            │        (stopped at depth 6; 1 branch omitted)
            ├─ call BasketRepository.GetBasket  (Services/Basket/Basket.API/Basket/CheckoutBasket/CheckoutBasketHandler.cs:32) [verified]
            │      public async Task<ShoppingCart> GetBasket(string userName, CancellationToken cancellationToken = default)
            │      var basket = await session.LoadAsync<ShoppingCart>(userName, cancellationToken);
            │      return basket is null ? throw new BasketNotFoundException(userName) : basket;
            ├─ call BasketCheckoutDto.Adapt  (Services/Basket/Basket.API/Basket/CheckoutBasket/CheckoutBasketHandler.cs:38) [verified]
            └─ call BasketRepository.DeleteBasket  (Services/Basket/Basket.API/Basket/CheckoutBasket/CheckoutBasketHandler.cs:43) [verified]
                   public async Task<bool> DeleteBasket(string userName, CancellationToken cancellationToken = default)
                   session.Delete<ShoppingCart>(userName);
                   await session.SaveChangesAsync(cancellationToken);
EMITS    BasketCheckoutEvent
RESULT   200 OK / 201 Created · failure → 400 Bad Request
