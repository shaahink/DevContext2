# Example: DI Hardening Audit

**Scenario**: `harden-di` — Identify indirect wiring patterns that make DI hard to reason about.

**Command**:
```bash
devcontext analyze ./MyApp.sln --scenario harden-di --profile focused
```

---

## What the output looks like

### Entry Point Constructor Dependencies

The tool focuses on the type and shows exactly what it depends on:

```markdown
### `BacktestOrchestrator` (Class, Presentation)
> `MyApp.Services.BacktestOrchestrator`

**Depends on**: `IServiceScopeFactory scopeFactory`, `BacktestProgressStore progressStore`, `ILogger<BacktestOrchestrator> logger`
**Resolved to**: `IServiceScopeFactory` → framework-provided, `BacktestProgressStore` → `BacktestProgressStore` (Program.cs:24)
```

### Indirect Wiring Detected

The tool finds all places where services resolve dependencies outside constructor injection:

| Kind | Caller | Target |
|---|---|---|
| ManualServiceLocator | BacktestOrchestrator.RunAsync | unknown |
| ManualServiceLocator | BacktestOrchestrator.RunAsync | unknown |
| ManualServiceLocator | BacktestOrchestrator.RunAsync | unknown |
| ReflectionActivation | StrategyRegistry.CreateStrategies | type! |
| ManualServiceLocator | StrategyRegistry.RegisterFactories | unknown |
| ManualServiceLocator | Program.Main | unknown |

### Anti-Patterns Cross-Reference

| Severity | Pattern | Description | Source |
|---|---|---|---|
| 🔴 high | ServiceLocator | IServiceScopeFactory.CreateScope() — manual service location | BacktestOrchestrator.cs:117 |
| 🔴 high | ServiceLocator | IServiceScopeFactory.CreateAsyncScope() — manual service location | BacktestQueryService.cs:30 |
| 🔴 high | ServiceLocator | IServiceScopeFactory.CreateAsyncScope() — manual service location | PersistenceService.cs:42 |
| 🟡 medium | UnboundedCollection | `ConcurrentDictionary<string, BacktestRunState>` — no eviction | BacktestOrchestrator.cs:90 |

### DI Registrations Table

| Lifetime | Service | Implementation | Source |
|---|---|---|---|
| Singleton | IBacktestCommandService | sp => sp.GetRequiredService<BacktestOrchestrator>() | Program.cs:25 |
| Singleton | BacktestOrchestrator | BacktestOrchestrator | Program.cs:24 |
| Scoped | IBacktestRunRepository | SqliteBacktestRunRepository | Program.cs:21 |
| Scoped | ITradeRepository | SqliteTradeRepository | Program.cs:122 |
| Extension | AddHostedService | EngineWorker | Program.cs:165 |
| Extension | AddDbContext | opt => opt.UseSqlite(...) | Program.cs:121 |

### Background Workers

- DailyResetService (HostedService)
- EngineWorker (HostedService)
- DataFeedService (HostedService)

---

## What this tells you

1. **BacktestOrchestrator uses IServiceScopeFactory** — At lines 52 and 117, this class resolves dependencies at runtime instead of accepting them via constructor. This makes the class harder to unit test and hides its true dependencies.
2. **Lambda registrations bypass static analysis** — `sp => sp.GetRequiredService<BacktestOrchestrator>()` at line 25 is a lambda registration. The DI container resolves it at runtime, but static analysis can't verify the dependency graph.
3. **Unbounded collections** — The `ConcurrentDictionary` in `BacktestOrchestrator` has no eviction. In a long-running process, completed backtest runs accumulate in memory.
4. **StrategyRegistry uses ReflectionActivation** — Strategies are created via reflection, bypassing DI entirely. New strategy types added to the project won't be automatically wired.
