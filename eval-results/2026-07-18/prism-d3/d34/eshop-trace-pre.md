TRACE  POST /api/orders/
       src/Ordering.API/Apis/OrdersApi.cs:17
       Ordering.API
▸ ENTRY  POST /api/orders/  (src/Ordering.API/Apis/OrdersApi.cs:17)
   └─ call OrdersApi.CreateOrderAsync  (src/Ordering.API/Apis/OrdersApi.cs:17)
          public static async Task<Results<Ok, BadRequest<string>>> CreateOrderAsync(
          [FromHeader(Name = "x-requestid")] Guid requestId,
          CreateOrderRequest request,
      ├─ send IdentifiedCommand  (src/Ordering.API/Apis/OrdersApi.cs:155) [verified]
      ├─ call OrderServices  (src/Ordering.API/Apis/OrdersApi.cs:126) [approx]
      │      public class OrderServices(
      │      IMediator mediator,
      │      IOrderQueries queries,
      ├─ call CreateOrderRequest  (src/Ordering.API/Apis/OrdersApi.cs:128) [approx]
      │      public record CreateOrderRequest(
      │      string UserId,
      │      string UserName,
      └─ call ILogger  (src/Ordering.API/Apis/OrdersApi.cs:129) [verified]
RESULT   200 OK / 201 Created · failure → 400 Bad Request
