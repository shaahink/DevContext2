MAP  MinimalApiProject     (3 projects)

STACK  net10.0 · Minimal APIs · MediatR (CQRS) · EF Core

STYLE  CleanArchitecture  (confidence moderate)
       evidence: DDD folder layers: Infrastructure, Api, Core; MediatR with 1 handlers

TOPOLOGY (depends-on)
   Core
   Api ── Core
   Infrastructure ── Core

ENTRY POINTS
   HTTP (2)
      GET /orders  → GetOrdersQuery  (src/Api/Program.cs:9)
      POST /orders  → CreateOrderCommand  (src/Api/Program.cs:15)

PACKAGES
   Web/API:  Microsoft.AspNetCore.OpenApi 10.0.0
   ORM/Data:  Dapper 2.1.35, Microsoft.EntityFrameworkCore 10.0.0-preview.3.25171.5
   Mediator/CQRS:  MediatR 12.0.0

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)