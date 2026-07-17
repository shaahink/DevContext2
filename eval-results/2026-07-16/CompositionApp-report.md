# REPORT
**CompositionApp**

Style: ControllerBased
_3 projects  ·  3 HttpEndpoint, 2 HostedService, 1 SignalRHub  ·  net10.0 + controllers + minimal-apis + signalr + aspire_

## Stats

| Metric | Value |
|--------|-------|
| Files | 13 |
| Projects | 3 |
| Nodes | 34 |
| Edges | 21 |
| Entries | 6 |
| With target | 6/6 |
| Deep spine (>=2) | 6/6 (100%) |
| Verified edges | 76% |
| Analyzed in | 1.9s |

## Top Flows

1. **PriceHub (2 methods: Subscribe, Unsubscribe)** → `PriceService.GetLastAsync` *(SignalRHub)*
2. **GET /api/addons/packs/{id}** → `AddonService.GetPackAsync` *(HttpEndpoint)*
3. **GET /api/addons/themes/{slug}** → `AddonService.GetThemeAsync` *(HttpEndpoint)*
4. **POST /api/addons/packs** → `AddonService.CreatePackAsync` *(HttpEndpoint)*
5. **BacktestWorker** → `BacktestWorker` *(HostedService)*
6. **PriceWorker** → `PriceWorker` *(HostedService)*

### Trace 1: PriceHub (2 methods: Subscribe, Unsubscribe)

TRACE  PriceHub (2 methods: Subscribe, Unsubscribe)
       src/Web/Hubs/PriceHub.cs:11
       Web
▸ ENTRY  PriceHub  (src/Web/Hubs/PriceHub.cs:11)
   ├─ call PriceHub.Subscribe  (src/Web/Hubs/PriceHub.cs:11)
   │      public async Task Subscribe(string symbol)
   │      var last = await _prices.GetLastAsync(symbol);
   │      await Clients.Caller.SendAsync("Price", symbol, last);
   │  ├─ call IPriceService  (src/Web/Hubs/PriceHub.cs:19) [approx]
   │  │      public interface IPriceService
   │  │      Task RefreshAsync(CancellationToken ct);
   │  │      Task<decimal> GetLastAsync(string symbol);
   │  │  └─ di PriceService  (src/Web/Configuration/ServiceRegistration.cs:21)
   │  │         public sealed class PriceService : IPriceService
   │  │         private decimal _last;
   │  │         public Task RefreshAsync(CancellationToken ct)
   │  └─ call PriceService.GetLastAsync  (src/Web/Hubs/PriceHub.cs:19) [verified]
   │         public Task<decimal> GetLastAsync(string symbol) => Task.FromResult(_last);
   └─ call PriceHub.Unsubscribe  (src/Web/Hubs/PriceHub.cs:11)
          public Task Unsubscribe(string symbol) =>
          Groups.RemoveFromGroupAsync(Context.ConnectionId, symbol);

---

### Trace 2: GET /api/addons/packs/{id}

TRACE  GET /api/addons/packs/{id}
       src/Web/Controllers/AddonsController.cs:20
       Web
▸ ENTRY  GET /api/addons/packs/{id}  (src/Web/Controllers/AddonsController.cs:20)
   └─ call AddonsController.GetPack  (src/Web/Controllers/AddonsController.cs:20)
          [HttpGet("packs/{id}")]
          public async Task<IActionResult> GetPack(int id)
          var pack = await _addons.GetPackAsync(id);
      ├─ call IAddonService  (src/Web/Controllers/AddonsController.cs:23) [approx]
      │      public interface IAddonService
      │      Task<Addon?> GetPackAsync(int id);
      │      Task<Addon> CreatePackAsync(string name);
      │  └─ di AddonService  (src/Web/Configuration/ServiceRegistration.cs:20)
      │         public sealed class AddonService : IAddonService
      │         public Task<Addon?> GetPackAsync(int id) => Task.FromResult<Addon?>(new Addon(id, $"pack-{id}"));
      │         public Task<Addon> CreatePackAsync(string name) => Task.FromResult(new Addon(1, name));
      └─ call AddonService.GetPackAsync  (src/Web/Controllers/AddonsController.cs:23) [verified]
             public Task<Addon?> GetPackAsync(int id) => Task.FromResult<Addon?>(new Addon(id, $"pack-{id}"));
RESULT   200 OK · failure → 404 Not Found

---

### Trace 3: GET /api/addons/themes/{slug}

TRACE  GET /api/addons/themes/{slug}
       src/Web/Controllers/AddonsController.cs:34
       Web
▸ ENTRY  GET /api/addons/themes/{slug}  (src/Web/Controllers/AddonsController.cs:34)
   └─ call AddonsController.GetTheme  (src/Web/Controllers/AddonsController.cs:34)
          [HttpGet("themes/{slug}")]
          public async Task<IActionResult> GetTheme(string slug)
          var theme = await _addons.GetThemeAsync(slug);
      ├─ call IAddonService  (src/Web/Controllers/AddonsController.cs:37) [approx]
      │      public interface IAddonService
      │      Task<Addon?> GetPackAsync(int id);
      │      Task<Addon> CreatePackAsync(string name);
      │  └─ di AddonService  (src/Web/Configuration/ServiceRegistration.cs:20)
      │         public sealed class AddonService : IAddonService
      │         public Task<Addon?> GetPackAsync(int id) => Task.FromResult<Addon?>(new Addon(id, $"pack-{id}"));
      │         public Task<Addon> CreatePackAsync(string name) => Task.FromResult(new Addon(1, name));
      └─ call AddonService.GetThemeAsync  (src/Web/Controllers/AddonsController.cs:37) [verified]
             public Task<Addon?> GetThemeAsync(string slug) => Task.FromResult<Addon?>(new Addon(0, slug));
RESULT   200 OK · failure → 404 Not Found

---

## Insights

_4 info · 1 warning_

### **WARNING**: 3/3 endpoints anonymous, incl. 1 POST/PUT/DELETE
*(Risk)*

- GET /api/addons/themes/{slug}
- POST /api/addons/packs
- GET /api/addons/packs/{id}

### _INFO_: Entry targets resolved 6/6 (100%) — trace any entry for its full path
*(Coverage)*

### _INFO_: DI: 5 Extension · 3 Singleton (8 total)
*(Wiring)*

### _INFO_: Entry surface: 3 HTTP · 2 hosted · 1 SignalRHub
*(Shape)*

- 3 HTTP
- 2 hosted
- 1 SignalRHub

### _INFO_: Most depended-upon: Core (2 dependents) · Web (1 dependents)
*(Topology)*

- Core (2 dependents)
- Web (1 dependents)

MAP  CompositionApp     (3 projects)

STACK  net10.0 · Minimal APIs · Controllers

STYLE  ControllerBased  (confidence moderate)
       evidence: Controllers detected (conf=0.9); MediatR=no, MinimalApi=yes(conf=0.9)

       per service:
         CompositionApp.AppHost: Aspire AppHost [Aspire]
         Web: Unknown

TOPOLOGY (depends-on)
   Core
   Web ── Core
   CompositionApp.AppHost ── Core, Web

ENTRY POINTS
   HTTP (3)
      GET /api/addons/packs/{id}  → AddonService.GetPackAsync  (src/Web/Controllers/AddonsController.cs:20)
      GET /api/addons/themes/{slug}  → AddonService.GetThemeAsync  (src/Web/Controllers/AddonsController.cs:34)
      POST /api/addons/packs  → AddonService.CreatePackAsync  (src/Web/Controllers/AddonsController.cs:27)
   Background (2)
      BacktestWorker  → BacktestWorker  (src/Web/Configuration/ServiceRegistration.cs:30)
      PriceWorker  → PriceWorker  (src/Web/Configuration/ServiceRegistration.cs:24)
   SignalR (1)
      PriceHub (2 methods: Subscribe, Unsubscribe)  → PriceService.GetLastAsync  (src/Web/Hubs/PriceHub.cs:11)

PACKAGES
   Web/API:  Microsoft.AspNetCore.OpenApi 10.0.0

→ drill in:  --focus "<entry>"   (e.g. --focus "PriceHub (2 methods: Subscribe, Unsubscribe)")
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 49ms |
| GenericExtraction | 154ms |
| SignalSealing | 0ms |
| SpecificExtraction | 1110ms |
| Compression | 24ms |
| **Total** | **1877ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| CallGraphExtractor | 1088ms | 0 | 0 |
| SyntaxStructureExtractor | 151ms | 12 | 12 |
| DiRegistrationExtractor | 150ms | 0 | 12 |
| ProgramCsFlowExtractor | 142ms | 0 | 4 |
| ProjectStructure | 22ms | 0 | 0 |
| EndpointExtractor | 17ms | 0 | 6 |
| DependencyExtractor | 17ms | 0 | 0 |
| LayerClassifier | 17ms | 0 | 0 |
| BodyFactsExtractor | 14ms | 0 | 0 |
| SolutionDiscovery | 13ms | 0 | 0 |
| ControllerActionExtractor | 12ms | 0 | 6 |
| FileTreeExtractor | 10ms | 0 | 0 |
| InMemoryEventBusExtractor | 6ms | 0 | 3 |
| SourceBodyExtractor | 5ms | 0 | 0 |
| AspireExtractor | 5ms | 0 | 3 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 17 | 5 |
| Resolves | 4 | 0 |

_13 files · 3 projects_
