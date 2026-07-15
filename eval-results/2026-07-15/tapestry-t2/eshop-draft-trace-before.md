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
RESULT   200 OK / 201 Created · failure → 400 Bad Request

analyzed 527 files · 1031 nodes · 732 edges · 109 entries · 96/109 →target · 
depth 1 · ~147 tokens · 24.8s stage2 ×2.9 stage3 ×2.2
╭──────────┬──────────────────────╮
│  Metric  │        Value         │
├──────────┼──────────────────────┤
│ Solution │      eShop.slnx      │
│   Time   │       25243ms        │
│  Tokens  │  ~147 (budget 8000)  │
│ Version  │ v1.0.5-preview.0.256 │
╰──────────┴──────────────────────╯
TRACE_DONE exit=0
