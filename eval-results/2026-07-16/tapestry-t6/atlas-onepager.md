# Atlas — TradingEngine.slnx

**Archetype:** App | **Projects:** 28 | **Entries:** 137

## Services
- **CTrader**
- **CTrader**
- **AppHost** (Aspire AppHost) → Host, Web
  Stack: Aspire
- **AppHost** (Aspire AppHost) → Host, Web
  Stack: Aspire
- **Application** → Domain, Engine
- **Application** → Domain, Engine
- **CTraderRunner** → Domain, Infrastructure
- **CTraderRunner** → Domain, Infrastructure
- **Domain**
- **Domain**
- **Engine** → Domain
- **Engine** → Domain
- **Experiments** → Application, Domain, Infrastructure
- **Experiments** → Application, Domain, Infrastructure
- **Host** (Worker Service) → Application, Domain, Experiments, Infrastructure, Risk, Services, Strategies
  Stack: Worker
- **Host** (Worker Service) → Application, Domain, Experiments, Infrastructure, Risk, Services, Strategies
  Stack: Worker
- **Infrastructure** → Application, Domain, Risk, Services
- **Infrastructure** → Application, Domain, Risk, Services
- **ResearchCli** (CLI) → Domain
  Stack: CLI
- **ResearchCli** (CLI) → Domain
  Stack: CLI
- **Risk** → Application, Domain, Services
- **Risk** → Application, Domain, Services
- **Services** → Application, Domain, Engine
- **Services** → Application, Domain, Engine
- **Strategies** → Domain, Services
- **Strategies** → Domain, Services
- **Web** (Web App) → CTraderRunner, Domain, Host, Infrastructure, Risk, Services, Strategies
  Stack: EF Core
- **Web** (Web App) → CTraderRunner, Domain, Host, Infrastructure, Risk, Services, Strategies
  Stack: EF Core

## Top Flows
- **POST /api/runs/{runId}/force-fail** (26 steps, 3 deep, 0 cross-service)
- **GET /api/runs/{runId}/export/json** (23 steps, 3 deep, 0 cross-service)
- **POST /api/exit-lab/evaluate** (23 steps, 3 deep, 0 cross-service)
- **GET /api/entry-quality** (22 steps, 3 deep, 0 cross-service)
- **GET /api/trades/{id:guid}/chart** (21 steps, 3 deep, 0 cross-service)

## Hub Radar
- Persistence.TradingDbContext: 70 flows (in 158, out 0)
- TradingDbContext.SaveChangesAsync: 26 flows (in 44, out 0)
- DownloadJobService.List(0): 20 flows (in 269, out 0)
- TradingDbContext.AsNoTracking: 19 flows (in 25, out 0)
- TradingDbContext.Where: 18 flows (in 25, out 0)
- TradingDbContext.FindAsync: 17 flows (in 20, out 0)
- TradingDbContext.Add: 13 flows (in 16, out 0)
- TradingDbContext.FirstOrDefaultAsync: 12 flows (in 12, out 0)
- Services.IRunQueryService: 12 flows (in 11, out 1)
- Services.RunQueryService: 12 flows (in 1, out 0)
