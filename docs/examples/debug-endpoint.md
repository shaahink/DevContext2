# Example: Debug a Failing Endpoint

**Scenario**: `debug-endpoint` — Focus on one endpoint to trace its request flow, dependencies, and find issues.

**Command**:
```bash
devcontext analyze ./MyApp.sln --scenario debug-endpoint --profile full --around BacktestController:Start
```

---

## What the output looks like

### 1. Entry Point with Constructor Dependencies

The tool finds the focused type and shows its constructor dependencies, resolved to concrete implementations via DI:

```markdown
### `BacktestController` (Class, Presentation)
> `TradingEngine.Web.Api.BacktestController` — BacktestController.cs

**Extends**: `ControllerBase`
**Depends on**: `IBacktestCommandService command`, `BacktestOrchestrator orchestrator`, `BacktestProgressStore progressStore`, `ILogger<BacktestController> logger`
**Resolved to**: `IBacktestCommandService` → `BacktestOrchestrator` (Program.cs:25), `BacktestProgressStore` → `BacktestProgressStore` (Program.cs:24), `ILogger<BacktestController>` → framework-provided
```

### 2. Full Source Code

For `--profile full`, the tool embeds the complete source of the entry point AND call chain types:

```csharp
// BacktestController.cs (97 lines)
[ApiController]
[Route("api/backtest")]
public sealed class BacktestController : ControllerBase
{
    private readonly IBacktestCommandService _command;
    private readonly BacktestOrchestrator _orchestrator;

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartRequest req)
    {
        var runId = await _command.StartAsync(cfg, HttpContext.RequestAborted);
        var state = _orchestrator.GetState(runId);
        ...
    }
}
```

### 3. Call Graph — Depth-5 BFS Trace

The call graph traces every method call from the endpoint through dependency injection:

```
TradingEngine.Web.Api.BacktestController.Start
├─ this.Ok (BacktestController.cs:57)
├─ TradingEngine.Web.Services.BacktestOrchestrator.GetState (BacktestController.cs:53)
│  └─ ConcurrentDictionary.TryGetValue (BacktestOrchestrator.cs:96)
├─ TradingEngine.Web.Services.BacktestOrchestrator.StartAsync (BacktestController.cs:52)
│  └─ TradingEngine.Web.Services.BacktestOrchestrator.Start (BacktestOrchestrator.cs:102)
│     ├─ TradingEngine.Web.Services.BacktestOrchestrator.RunAsync (BacktestOrchestrator.cs:90)
│     │  ├─ BacktestProgressStore.Complete → ch.TryComplete / ConcurrentDictionary.TryRemove
│     │  ├─ repo.SaveAsync  (← DB write — SQLite)
│     │  ├─ IServiceScopeFactory.CreateScope  (← flagged by anti-pattern detector)
│     │  ├─ EnqueueLog → PushProgress → queue.Enqueue
│     │  ├─ GetTradeStatsAsync → db.Trades.Where → ToListAsync
│     │  └─ scope.GetRequiredService<ITradingDbContext>
│     ├─ EnqueueLog (BacktestOrchestrator.cs:83)
│     └─ Guid.NewGuid
├─ req.Period.ToLowerInvariant (BacktestController.cs:44)
└─ req.Symbol.ToUpperInvariant (BacktestController.cs:43)
```

### 4. Anti-Patterns Automatically Flagged

The tool detects common mistakes without running the code:

| Severity | Pattern | Description | Source |
|---|---|---|---|
| 🔴 high | FireAndForget | `_ = RunAsync(cfg)` — task is never awaited. Exceptions may be lost. | BacktestOrchestrator.cs:85 |
| 🔴 high | ServiceLocator | `IServiceScopeFactory.CreateScope()` — manual service location. Prefer constructor injection. | BacktestOrchestrator.cs:117 |
| 🔴 high | ServiceLocator | `IServiceScopeFactory.CreateScope()` — manual service location. | BacktestOrchestrator.cs:52 |
| 🟡 medium | CancellationTokenNone | `CancellationToken.None` used — operation cannot be cancelled. | Program.cs:43 |
| 🟡 medium | UnboundedCollection | `ConcurrentDictionary<string, BacktestRunState>` in `BacktestOrchestrator` — no eviction/cleanup method found. | BacktestOrchestrator.cs:90 |
| 🟢 low | CancellationTokenNone | `new CancellationToken()` — default token, same as None. | BacktestProgressStore.cs:34 |

### 5. Event Flow (In-Memory Bus Wiring)

The tool traces the event bus wiring from `Program.cs` registration to handler implementation:

| Event | Direction | Target | Source |
|---|---|---|---|
| `EquityUpdated` | ← subscribed | `equityHandler` → `EquityPersistenceHandler` | Program.cs:172 |
| `TradeClosed` | ← subscribed | `tradeHandler` → `TradePersistenceHandler` | Program.cs:175 |
| `BarEvaluated` | ← subscribed | `barEvalHandler` → `BarEvaluationHandler` | Program.cs:178 |

**IEventHandler implementations**:
- `EquityUpdated` → `EquityPersistenceHandler` (writes to `EquitySnapshots` table)
- `TradeClosed` → `TradePersistenceHandler` (writes to `TradeResults` table)
- `BarEvaluated` → `BarEvaluationHandler` (writes to `BarEvaluations` table)

### 6. Projects + DI Registrations

```
└── TradingEngine.AppHost
    ├── TradingEngine.Host
    │   ├── TradingEngine.Domain
    │   ├── TradingEngine.Application
    │   ├── TradingEngine.Risk
    │   ├── TradingEngine.Services
    │   ├── TradingEngine.Infrastructure
    │   └── TradingEngine.Strategies
    └── TradingEngine.Web
        └── TradingEngine.CTraderRunner
```

---

## What an LLM can do with this

Given this output, an LLM can:

1. **Understand the issue**: `_ = RunAsync(cfg)` at BacktestOrchestrator.cs:85 is fire-and-forget. The task starts on the thread pool but nothing awaits it. If it throws, the exception is unobserved.
2. **Trace the full path**: From HTTP POST → `BacktestController.Start` → `BacktestOrchestrator.StartAsync` → `RunAsync` → shell-out to ctrader-cli → SQLite write → SSE stream back to browser.
3. **Spot the service locator anti-pattern**: `IServiceScopeFactory.CreateScope()` at lines 52 and 117. The class should inject its dependencies via constructor, not resolve them from a scope at runtime.
4. **Find the untracked state**: `ConcurrentDictionary<string, BacktestRunState>` has no eviction mechanism. Historical runs accumulate in memory.
5. **Understand the event architecture**: Three event types flow through an in-memory bus to dedicated persistence handlers.
