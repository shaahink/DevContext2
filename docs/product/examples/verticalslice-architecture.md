Overview map (no focus).
Analyzing project...

MAP  MinimalClean.Architecture     (3 projects)

STACK  Minimal APIs · FastEndpoints · MediatR (CQRS) · EF Core · DDD aggregates

STYLE  VerticalSlices  (confidence high)
       evidence: FastEndpoints detected; MediatR with 0 handlers

TOPOLOGY (depends-on)
   MinimalClean.Architecture.AspireHost ── 
MinimalClean.Architecture.ServiceDefaults, MinimalClean.Architecture.Web
   MinimalClean.Architecture.ServiceDefaults
   MinimalClean.Architecture.Web ── MinimalClean.Architecture.ServiceDefaults

ENTRY POINTS
   HTTP (5)
      GET /Products   ListProductsQuery  
(src/MinimalClean.Architecture.Web/ProductFeatures/List/ListEndpoint.cs:31)
      GET /Products/{ProductId}  
(src/MinimalClean.Architecture.Web/ProductFeatures/GetById/GetByIdEndpoint.cs:24
)
      POST /Products  
(src/MinimalClean.Architecture.Web/ProductFeatures/Create/CreateEndpoint.cs:23)
      GET /cart/{CartId}  
(src/MinimalClean.Architecture.Web/CartFeatures/GetById/GetByIdEndpoint.cs:24)
      POST /cart/{CartId}/checkout  
(src/MinimalClean.Architecture.Web/CartFeatures/Checkout/CheckoutEndpoint.cs:28)

CROSS-CUTTING
   Aggregates:   Cart · GuestUser · Order · Product

PACKAGES
   Web/API:  Ardalis.Result.AspNetCore, FastEndpoints, FastEndpoints.Swagger, 
OpenTelemetry.Instrumentation.AspNetCore, Scalar.AspNetCore, Serilog.AspNetCore
   ORM/Data:  Ardalis.Specification.EntityFrameworkCore, 
Aspire.Hosting.SqlServer, Microsoft.EntityFrameworkCore.Design, 
Microsoft.EntityFrameworkCore.Relational, 
Microsoft.EntityFrameworkCore.SqlServer
   Logging:  OpenTelemetry.Exporter.OpenTelemetryProtocol, 
OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.Http, 
OpenTelemetry.Instrumentation.Runtime, Serilog.Sinks.OpenTelemetry
   Cloud:  Azure.Identity
   Other:  Ardalis.GuardClauses, Ardalis.ListStartupServices, Ardalis.Result, 
Ardalis.SharedKernel, Ardalis.SmartEnum, Ardalis.Specification, 
Aspire.Hosting.AppHost, MailKit . (16 total)

 drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus 
<TypeName>)

analyzed 76 files · 263 nodes · 156 edges · 5 entries · 1/5 target · ~512 
tokens · 2.3s stage2 x2.7 stage3 x1.1
┌──────────┬────────────────────────────────┐
│  Metric  │             Value              │
├──────────┼────────────────────────────────┤
│ Solution │ MinimalClean.Architecture.slnx │
│   Time   │             2364ms             │
│  Tokens  │       ~512 (budget 8000)       │
│ Version  │      v1.0.5-preview.0.127      │
└──────────┴────────────────────────────────┘
