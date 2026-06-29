MAP  CleanArch     (4 projects)

STACK  net10.0 · Minimal APIs · MediatR (CQRS) · EF Core

STYLE  CleanArchitecture  (confidence high)
       evidence: DDD folder layers: Domain, Application, Infrastructure; MediatR with 1 handlers

TOPOLOGY (depends-on)
   Application ── Domain
   Domain
   Infrastructure ── Domain
   Web ── Application, Infrastructure

ENTRY POINTS
   HTTP (1)
      GET /products  → GetProductsQuery  (src/Web/Program.cs:13)

PACKAGES
   ORM/Data:  Microsoft.EntityFrameworkCore 10.0.0-preview.3.25171.5
   Mediator/CQRS:  MediatR 12.0.0

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)