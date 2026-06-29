Overview map (no focus).
Analyzing project...

MAP  eShop     (5 projects)
SCOPE  5-project closure of 24-project eShop - style/topology are local to this 
slice, not the whole system

STACK  net10.0 · Minimal APIs · EF Core

STYLE  MinimalApi  (confidence moderate)
       evidence: Minimal APIs + 5 project(s); no MediatR

TOPOLOGY (depends-on)
   Catalog.API ── eShop.ServiceDefaults, EventBusRabbitMQ, IntegrationEventLogEF
   eShop.ServiceDefaults
   EventBus
   EventBusRabbitMQ ── EventBus
   IntegrationEventLogEF ── EventBus

ENTRY POINTS
   HTTP (16)
      DELETE /api/catalog/items/{id:int}   CatalogContext.SaveChangesAsync  
(src/Catalog.API/Apis/CatalogApi.cs:107)
      POST /api/catalog/items   CatalogAI.GetEmbeddingAsync  
(src/Catalog.API/Apis/CatalogApi.cs:103)
      PUT /api/catalog/items/{id:int}   CatalogContext.Entry  
(src/Catalog.API/Apis/CatalogApi.cs:98)
      PUT /api/catalog/items  (src/Catalog.API/Apis/CatalogApi.cs:93)
      GET /api/catalog/catalogbrands  (src/Catalog.API/Apis/CatalogApi.cs:84)
      GET /api/catalog/catalogtypes  (src/Catalog.API/Apis/CatalogApi.cs:77)
      GET /api/catalog/items/type/all/brand/{brandId:int?}  
(src/Catalog.API/Apis/CatalogApi.cs:72)
      GET /api/catalog/items/type/{typeId}/brand/{brandId?}  
(src/Catalog.API/Apis/CatalogApi.cs:67)
      GET /api/catalog/items/withsemanticrelevance   
CatalogAI.GetEmbeddingAsync  (src/Catalog.API/Apis/CatalogApi.cs:60)
      GET /api/catalog/items/withsemanticrelevance/{text:minlength(1)}  
(src/Catalog.API/Apis/CatalogApi.cs:53)
      GET /api/catalog/items/{id:int}/pic   CatalogContext.FindAsync  
(src/Catalog.API/Apis/CatalogApi.cs:46)
      GET /api/catalog/items/by/{name:minlength(1)}  
(src/Catalog.API/Apis/CatalogApi.cs:41)
      GET /api/catalog/items/{id:int}   CatalogServices.Include  
(src/Catalog.API/Apis/CatalogApi.cs:36)
      GET /api/catalog/items/by   CatalogServices.Where  
(src/Catalog.API/Apis/CatalogApi.cs:31)
      GET /api/catalog/items  (src/Catalog.API/Apis/CatalogApi.cs:26)
      GET /api/catalog/items  (src/Catalog.API/Apis/CatalogApi.cs:21)
   Bus (2)
      OrderStatusChangedToPaidIntegrationEventHandler  
(src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToPaidIntegra
tionEventHandler.cs:3)
      OrderStatusChangedToAwaitingValidationIntegrationEventHandler  
(src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToAwaitingVal
idationIntegrationEventHandler.cs:3)

PACKAGES
   Web/API:  Microsoft.AspNetCore.Authentication.JwtBearer, 
Microsoft.AspNetCore.OpenApi, OpenTelemetry.Instrumentation.AspNetCore, 
Scalar.AspNetCore
   ORM/Data:  Aspire.Npgsql.EntityFrameworkCore.PostgreSQL, 
Microsoft.EntityFrameworkCore.Tools, Npgsql.EntityFrameworkCore.PostgreSQL, 
Pgvector.EntityFrameworkCore
   Messaging:  Aspire.RabbitMQ.Client
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol, 
OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.GrpcNetClient, 
OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.Runtime
   Cloud:  Aspire.Azure.AI.OpenAI
   Other:  Asp.Versioning.Http, Asp.Versioning.Mvc.ApiExplorer, 
Asp.Versioning.OpenApi, CommunityToolkit.Aspire.OllamaSharp, 
Microsoft.Extensions.ApiDescription.Server, 
Microsoft.Extensions.Http.Resilience, Microsoft.Extensions.Options, 
Microsoft.Extensions.ServiceDiscovery . (10 total)

 drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus 
<TypeName>)

analyzed 66 files · 218 nodes · 134 edges · 18 entries · 7/18 target · ~858 
tokens · 2.8s stage2 x2.9 stage3 x1.1
┌──────────┬──────────────────────┐
│  Metric  │        Value         │
├──────────┼──────────────────────┤
│ Solution │      eShop.slnx      │
│   Time   │        2868ms        │
│  Tokens  │  ~858 (budget 8000)  │
│ Version  │ v1.0.5-preview.0.127 │
└──────────┴──────────────────────┘
