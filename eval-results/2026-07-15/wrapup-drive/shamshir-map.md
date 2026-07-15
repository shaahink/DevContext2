MAP  TradingEngine     (14 projects)

STACK  net10.0, net6.0 · Minimal APIs · Controllers · EF Core

STYLE  MinimalApi  (confidence moderate)
       evidence: Minimal APIs + 16 project(s); no MediatR

       per service:
         TradingEngine.ResearchCli: Unknown
         TradingEngine.Web: Web App [EF Core]

TOPOLOGY (depends-on)
   TradingEngine.Domain
   TradingEngine.Application ── TradingEngine.Domain, TradingEngine.Engine
   TradingEngine.Services ── TradingEngine.Application, TradingEngine.Domain, TradingEngine.Engine
   TradingEngine.Infrastructure ── TradingEngine.Application, TradingEngine.Domain, TradingEngine.Risk, TradingEngine.Services
   TradingEngine.Risk ── TradingEngine.Application, TradingEngine.Domain, TradingEngine.Services
   TradingEngine.Engine ── TradingEngine.Domain
   TradingEngine.Host ── TradingEngine.Application, TradingEngine.Domain, TradingEngine.Experiments, TradingEngine.Infrastructure, TradingEngine.Risk, TradingEngine.Services, TradingEngine.Strategies
   TradingEngine.Strategies ── TradingEngine.Domain, TradingEngine.Services
   TradingEngine.CTraderRunner ── TradingEngine.Domain, TradingEngine.Infrastructure
   TradingEngine.Experiments ── TradingEngine.Application, TradingEngine.Domain, TradingEngine.Infrastructure
   TradingEngine.Web ── TradingEngine.CTraderRunner, TradingEngine.Domain, TradingEngine.Host, TradingEngine.Infrastructure, TradingEngine.Risk, TradingEngine.Services, TradingEngine.Strategies
   TradingEngine.Adapters.CTrader
   TradingEngine.AppHost ── TradingEngine.Host, TradingEngine.Web
   TradingEngine.ResearchCli ── TradingEngine.Domain

ENTRY POINTS
   HTTP (122)
      DELETE /api/addons  → DeleteAsync  (src/TradingEngine.Web/Api/AddOnPacksController.cs:110)
      DELETE /api/prop-firm-rules  → DeleteAsync  (src/TradingEngine.Web/Api/PropFirmRulesController.cs:73)
      DELETE /api/risk-profiles  → DeleteAsync  (src/TradingEngine.Web/Api/RiskProfilesController.cs:96)
      DELETE /api/runs  → BacktestOrchestrator.Cancel  (src/TradingEngine.Web/Api/RunsController.cs:215)
      DELETE /api/strategies  → DeleteAsync  (src/TradingEngine.Web/Api/StrategiesController.cs:212)
      GET /api/addons  → AddOnAutoTuner.Tune  (src/TradingEngine.Web/Api/AddOnPacksController.cs:123)
      GET /api/addons  → AddOnAutoTuner.Tune  (src/TradingEngine.Web/Api/AddOnPacksController.cs:20)
      GET /api/addons  → AddOnAutoTuner.Tune  (src/TradingEngine.Web/Api/AddOnPacksController.cs:17)
      GET /api/backtest/analytics  → TradingDbContext  (src/TradingEngine.Web/Api/BacktestAnalyticsController.cs:195)
      GET /api/backtest/analytics  → TradingDbContext  (src/TradingEngine.Web/Api/BacktestAnalyticsController.cs:173)
      GET /api/backtest/analytics  → TradingDbContext  (src/TradingEngine.Web/Api/BacktestAnalyticsController.cs:160)
      GET /api/backtest/analytics  → TradingDbContext  (src/TradingEngine.Web/Api/BacktestAnalyticsController.cs:109)
      GET /api/backtest/analytics  → TradingDbContext  (src/TradingEngine.Web/Api/BacktestAnalyticsController.cs:87)
      GET /api/backtest/analytics  → TradingDbContext  (src/TradingEngine.Web/Api/BacktestAnalyticsController.cs:68)
      GET /api/backtest/analytics  → TradingDbContext  (src/TradingEngine.Web/Api/BacktestAnalyticsController.cs:53)
      GET /api/backtest/analytics  → TradingDbContext  (src/TradingEngine.Web/Api/BacktestAnalyticsController.cs:29)
      GET /api/bars  → BarQueryService.TakeLast  (src/TradingEngine.Web/Api/BarsController.cs:13)
      GET /api/ctrader/listen  → CTraderListenService.ToString  (src/TradingEngine.Web/Api/CtraderListenController.cs:63)
      GET /api/ctrader/sessions  → TradingDbContext  (src/TradingEngine.Web/Api/VenueSessionsController.cs:25)
      GET /api/ctrader/sessions  → TradingDbContext  (src/TradingEngine.Web/Api/VenueSessionsController.cs:11)
      … and 102 more (http entries — use --focus for a drill-in)

PACKAGES
   Web/API:  Microsoft.AspNetCore.Mvc.Testing 10.0.9, Microsoft.AspNetCore.OpenApi 10.0.9, Scalar.AspNetCore 2.16.4, Serilog.AspNetCore 10.0.0
   ORM/Data:  Dapper 2.1.79, Microsoft.EntityFrameworkCore 10.0.9, Microsoft.EntityFrameworkCore.Design 10.0.9, Microsoft.EntityFrameworkCore.InMemory 10.0.9, Microsoft.EntityFrameworkCore.Sqlite 10.0.9
   Logging:  Serilog 4.3.1, Serilog.Extensions.Hosting 10.0.0, Serilog.Settings.Configuration 10.*, Serilog.Sinks.Console 6.1.1, Serilog.Sinks.File 7.0.0
   Testing:  coverlet.collector 6.0.4, FluentAssertions 8.10.0, NSubstitute 5.3.0, xunit 2.9.3, xunit.runner.visualstudio 3.1.4, Xunit.SkippableFact 1.5.23
   Utilities:  Newtonsoft.Json 13.*, Scrutor 6.0.1
   Other:  CliWrap 3.*, cTrader.Automate *, Microsoft.Extensions.Configuration 10.*, Microsoft.Extensions.Configuration.EnvironmentVariables 10.*, Microsoft.Extensions.Configuration.Json 10.*, Microsoft.Extensions.DependencyInjection.Abstractions 10.0.8, Microsoft.Extensions.Hosting 10.0.8, Microsoft.Extensions.Hosting.Abstractions 10.0.8 … (13 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
