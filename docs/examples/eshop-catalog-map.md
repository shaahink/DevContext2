MAP  eShop     (5 projects)

STACK  net10.0 · Minimal APIs · EF Core · DDD aggregates

STYLE  MinimalApi  (confidence moderate)
       evidence: Minimal APIs + 5 project(s); no MediatR

TOPOLOGY (depends-on)
   Catalog.API ── eShop.ServiceDefaults, EventBusRabbitMQ, IntegrationEventLogEF
   eShop.ServiceDefaults
   EventBus
   EventBusRabbitMQ ── EventBus
   IntegrationEventLogEF ── EventBus

ENTRY POINTS
   HTTP (17)
      GET /  (src/eShop.ServiceDefaults/OpenApi.Extensions.cs:41)
      DELETE /api/catalog/items/{id:int}  (src/Catalog.API/Apis/CatalogApi.cs:107)
      POST /api/catalog/items  (src/Catalog.API/Apis/CatalogApi.cs:103)
      PUT /api/catalog/items/{id:int}  (src/Catalog.API/Apis/CatalogApi.cs:98)
      PUT /api/catalog/items  (src/Catalog.API/Apis/CatalogApi.cs:93)
      GET /api/catalog/catalogbrands  (src/Catalog.API/Apis/CatalogApi.cs:84)
      GET /api/catalog/catalogtypes  (src/Catalog.API/Apis/CatalogApi.cs:77)
      GET /api/catalog/items/type/all/brand/{brandId:int?}  (src/Catalog.API/Apis/CatalogApi.cs:72)
      GET /api/catalog/items/type/{typeId}/brand/{brandId?}  (src/Catalog.API/Apis/CatalogApi.cs:67)
      GET /api/catalog/items/withsemanticrelevance  (src/Catalog.API/Apis/CatalogApi.cs:60)
      GET /api/catalog/items/withsemanticrelevance/{text:minlength(1)}  (src/Catalog.API/Apis/CatalogApi.cs:53)
      GET /api/catalog/items/{id:int}/pic  (src/Catalog.API/Apis/CatalogApi.cs:46)
      GET /api/catalog/items/by/{name:minlength(1)}  (src/Catalog.API/Apis/CatalogApi.cs:41)
      GET /api/catalog/items/{id:int}  (src/Catalog.API/Apis/CatalogApi.cs:36)
      GET /api/catalog/items/by  (src/Catalog.API/Apis/CatalogApi.cs:31)
      GET /api/catalog/items  (src/Catalog.API/Apis/CatalogApi.cs:26)
      GET /api/catalog/items  (src/Catalog.API/Apis/CatalogApi.cs:21)
   Bus (2)
      OrderStatusChangedToPaidIntegrationEventHandler  (src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToPaidIntegrationEventHandler.cs:3)
      OrderStatusChangedToAwaitingValidationIntegrationEventHandler  (src/Catalog.API/IntegrationEvents/EventHandling/OrderStatusChangedToAwaitingValidationIntegrationEventHandler.cs:3)

CROSS-CUTTING
   Aggregates:   CatalogBrand · CatalogItem · CatalogType

PACKAGES
   Web/API:  Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.AspNetCore.OpenApi, OpenTelemetry.Instrumentation.AspNetCore, Scalar.AspNetCore
   ORM/Data:  Aspire.Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Tools, Npgsql.EntityFrameworkCore.PostgreSQL, Pgvector.EntityFrameworkCore
   Messaging:  Aspire.RabbitMQ.Client
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol, OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.GrpcNetClient, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.Runtime
   Cloud:  Aspire.Azure.AI.OpenAI
   Other:  Asp.Versioning.Http, Asp.Versioning.Mvc.ApiExplorer, Asp.Versioning.OpenApi, CommunityToolkit.Aspire.OllamaSharp, Microsoft.Extensions.ApiDescription.Server, Microsoft.Extensions.Http.Resilience, Microsoft.Extensions.Options, Microsoft.Extensions.ServiceDiscovery … (10 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
