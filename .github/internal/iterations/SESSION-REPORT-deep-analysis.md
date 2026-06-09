## DevContext — Full Trace: `POST /api/backtest/start` (Shamshir TradingEngine)

**Profile**: full | **Depth**: BFS 4 | **Tokens**: ~6,200

---

## Project Dependency Graph

```
    ┌─────────────────────┐
    │   TradingEngine.Web  │  (ASP.NET Core + Controllers)
    │   - BacktestController│
    │   - BacktestOrchestrator│
    └──┬────────┬─────────┘
       │        │
       ▼        ▼
┌──────────┐ ┌──────────────────────┐
│ Domain   │ │ Infrastructure       │
│ - IEventBus│ │ - EF Core DbContexts│
│ - IBroker  │ │ - Sqlite repos      │
│ - Entity types│ │ - Skender Indicators │
└──────────┘ │ - NetMQ, Dapper      │
             └──┬───────────────────┘
                │
   ┌────────────┼────────────┐
   ▼            ▼            ▼
┌──────┐  ┌──────────┐  ┌────────────┐
│ Risk │  │Services  │  │Strategies  │
└──────┘  └──────────┘  └────────────┘
             │
             ▼
      ┌──────────────┐
      │TradingEngine │ (Worker Service — headless engine)
      │.Host         │   Serilog, live/paper brokers, strategy loop
      └──────────────┘
```

---

## Request Trace: `POST /api/backtest/start` → DB Write

### Entry: `BacktestController.Start`

```csharp
// File: src/TradingEngine.Web/Api/BacktestController.cs:38
// Constructor deps (all resolved from DI):
//   IBacktestCommandService _command   → BacktestOrchestrator (singleton)
//   BacktestOrchestrator _orchestrator → BacktestOrchestrator (singleton)
//   BacktestProgressStore _progressStore → BacktestProgressStore (singleton)
//   ILogger<BacktestController> _logger

[HttpPost("start")]
public async Task<IActionResult> Start([FromBody] StartRequest req)
{
    var cfg = new BacktestConfig
    {
        Symbol = req.Symbol.ToUpperInvariant(),
        Period = req.Period.ToLowerInvariant(),
        Start = req.Start,
        End = req.End,
        Balance = req.Balance,
        CommissionPerMillion = req.CommissionPerMillion,
        SpreadPips = req.SpreadPips,
    };

    var runId = await _command.StartAsync(cfg, HttpContext.RequestAborted);
    var state = _orchestrator.GetState(runId);
    _logger.LogInformation("Backtest started. RunId={RunId}", runId);

    return Ok(new { runId, status = state?.Status ?? "unknown" });
}
```

### ↓ calls `IBacktestCommandService.StartAsync`

Implemented by **`BacktestOrchestrator`** (concrete class, no `IBacktestOrchestrator` abstraction).

```csharp
// File: src/TradingEngine.Web/Services/BacktestOrchestrator.cs
// Constructor deps:
//   IServiceScopeFactory _scopeFactory   (framework-provided)
//   BacktestProgressStore _progressStore  (singleton)
//   ILogger<BacktestOrchestrator> _logger

public async Task<string> StartAsync(BacktestConfig cfg, CancellationToken ct)
{
    var state = Start(cfg);           // creates BacktestRunState, stores in _runs dict
    await Task.CompletedTask;         // returns immediately — actual work is fire-and-forget
    return state.RunId;
}
```

**ISSUE**: `Start(cfg)` calls `_ = RunAsync(runId, cfg)` — a fire-and-forget task. The HTTP response returns before the backtest runs. Exceptions in `RunAsync` are caught internally but there's no mechanism for the caller to await completion or propagate cancellation.

### ↓ `RunAsync` (runs on thread pool, not awaited by caller)

```csharp
private async Task RunAsync(string runId, BacktestConfig cfg)
{
    // ⚠ ISSUE: Creates a new DI scope inside a fire-and-forget task.
    // The scope is disposed at the end of 'using', but this is correct here
    // since the scope lives for the duration of RunAsync.
    using var scope = _scopeFactory.CreateScope();
    var sp = scope.ServiceProvider;

    // Resolves services via manual service location (anti-pattern)
    var config = sp.GetRequiredService<IConfiguration>();
    var runnerLogger = sp.GetRequiredService<ILogger<BacktestRunner>>();

    // ⚠ ISSUE: BacktestRunner is new'd up manually, not resolved from DI.
    // It takes IConfiguration + ILogger — these could be constructor-injected
    // if BacktestRunner were registered in DI.
    var runner = new BacktestRunner(config, runnerLogger);
    var result = await runner.RunAsync(cfg);
    // BacktestRunner shells out to a ctrader-cli process (external CLI tool)

    // ⚠ ISSUE: Trade stats are queried AFTER the runner completes.
    // If the process crashes before writing to DB, trades could be lost.
    var tradeStats = await GetTradeStatsAsync(runId);
    // ... enriches result with trade stats

    // Saves summary to DB via scoped repository
    var repo = sp.GetRequiredService<IBacktestRunRepository>();
    await repo.SaveAsync(summary, CancellationToken.None);
    // ⚠ ISSUE: CancellationToken.None — save can't be cancelled
}
```

### ↓ Calls `IBacktestRunRepository.SaveAsync`

```
IBacktestRunRepository (scoped)
  └─ SqliteBacktestRunRepository
       └─ TradingDbContext (scoped, SQLite via EF Core)
            ├─ BacktestRunEntity
            ├─ TradeResultEntity
            ├─ EquitySnapshotEntity
            ├─ EngineEventEntity
            ├─ OrderEntity
            ├─ PositionEntity
            ├─ BarEntity
            └─ BarEvaluationEntity
```

### ↓ Concurrent: SSE Streaming

While `RunAsync` runs, progress is pushed via `BacktestProgressStore`:

```csharp
// BacktestProgressStore uses System.Threading.Channels
// Writers (BacktestOrchestrator.RunAsync) push progress lines
// Readers (BacktestController.Stream) stream via SSE

// Controller side:
[HttpGet("{runId}/stream")]
public async Task Stream(string runId, CancellationToken ct)
{
    var reader = _progressStore.GetReader(runId);
    await foreach (var line in reader.ReadAllAsync(ct))
        await Response.WriteAsync($"data: {line}\n\n", ct);
}
```

---

## Issues Spotted (from code analysis)

### Critical

| # | Issue | Location | Detail |
|---|---|---|---|
| 1 | **Fire-and-forget task** | `BacktestOrchestrator.cs:77` | `_ = RunAsync(runId, cfg)` is never awaited. If the task throws after the catch block, it's an unobserved task exception. The HTTP caller gets `200 OK` before the backtest starts. |
| 2 | **Manual service location** | `BacktestOrchestrator.cs:98-101` | Uses `IServiceScopeFactory` + `sp.GetRequiredService<T>()` instead of constructor injection. Anti-pattern that hides dependencies and prevents compile-time validation. |
| 3 | **BacktestRunner new'd outside DI** | `BacktestOrchestrator.cs:103` | `new BacktestRunner(config, logger)` — not registered in DI. No lifecycle management, no testability, can't be mocked. |

### High

| # | Issue | Location | Detail |
|---|---|---|---|
| 4 | **No timeout on backtest** | `BacktestOrchestrator.cs` | `RunAsync` has no timeout. If ctrader-cli hangs, the task runs forever, leaking the scope. |
| 5 | **No `IBacktestOrchestrator` interface** | `BacktestOrchestrator.cs` | Class is injected directly as `BacktestOrchestrator` (line 15). Tight coupling — can't mock in tests. |
| 6 | **`CancellationToken.None` on DB save** | `BacktestOrchestrator.cs:128` | DB write can't be cancelled. If the app shuts down during save, data could be corrupted. |
| 7 | **No transaction boundary** | `BacktestOrchestrator.RunAsync` | EF Core SaveChanges is called without a transaction. If enrichment fails after SaveChanges, partial data persists. |

### Medium

| # | Issue | Location | Detail |
|---|---|---|---|
| 8 | **`_runs` dictionary never pruned** | `BacktestOrchestrator` | ConcurrentDictionary grows unbounded. Completed/cancelled runs are never removed → memory leak. |
| 9 | **ConcurrentQueue per run for logs** | `BacktestRunState` | Logs are in-memory only. If the process restarts, all historical run logs are lost. |
| 10 | **No environment separation** | `Program.cs:16,18` | Web and Host both use the same `data/trading.db` path. Dev vs prod share the same DB unless config overrides. |

---

## DI Wiring — Key Registrations with Implementations

| Lifetime | Interface | Implementation | Notes |
|---|---|---|---|
| Singleton | `IBacktestCommandService` | `BacktestOrchestrator` | Exposed as interface, but constructor also takes concrete class |
| Singleton | `BacktestOrchestrator` | `BacktestOrchestrator` | Direct registration — no interface |
| Singleton | `BacktestProgressStore` | `BacktestProgressStore` | `Channel<string>`-based SSE broadcaster |
| Singleton | `IEventBus` | `TypedEventBus` | In-memory pub/sub |
| Singleton | `IBrokerAdapter` | `NetMQBrokerAdapter` or `SimulatedBrokerAdapter` | Mode-dependent registration |
| Singleton | `IMarketDataProvider` | `LiveMarketDataProvider` or `HistoricalDataProvider` | Mode-dependent |
| Singleton | `IRiskManager` | `RiskManager` | Through `sp.GetRequiredService<RiskManager>()` forwarding |
| Singleton | `IPositionManager` | `PositionManager` | |
| Scoped | `IBacktestRunRepository` | `SqliteBacktestRunRepository` | EF Core-backed |
| Scoped | `ITradeRepository` | `SqliteTradeRepository` | EF Core-backed |
| Scoped | `IEquityRepository` | `SqliteEquityRepository` | EF Core-backed |

---

## Event Flow (in-memory pub/sub via TypedEventBus)

```
EngineWorker (HostedService)
  │  processes Bar → evaluates strategies
  │
  ├─ Publishes: BarEvaluated ─────► BarEvaluationHandler
  │                                  └─ writes to BarEvaluations table
  │
  ├─ Publishes: TradeClosed ──────► TradePersistenceHandler
  │                                  └─ writes to TradeResults table
  │
  └─ Publishes: EquityUpdated ────► EquityPersistenceHandler
                                     └─ writes to EquitySnapshots table
```

---

## Data Model (SQLite / EF Core)

```
TradingDbContext
├─ BacktestRunEntity (Id PK, Symbol, Period, Start, End, Balance, AlgoHash, StrategyParamsJson)
├─ TradeResultEntity (Id PK, RunId FK→BacktestRunEntity)
├─ EquitySnapshotEntity (Id PK)
├─ EngineEventEntity (Id PK, EventType, Payload JSON, OccurredAtUtc)
├─ OrderEntity (Id PK, Symbol, Direction, OrderType, State, ...)
├─ PositionEntity (Id PK)
├─ BarEntity (Id PK)
└─ BarEvaluationEntity (Id PK, RunId FK, StrategyId, SignalFired, Reason)

ReportingDbContext
├─ EngineEventEntity
├─ EquitySnapshotEntity
└─ TradeResultEntity   ← shared table? Potential schema conflict with TradingDbContext.
```

**ISSUE**: Both `TradingDbContext` and `ReportingDbContext` map `TradeResultEntity`, `EquitySnapshotEntity`, and `EngineEventEntity`. If they point to the same SQLite file but define different schemas (e.g., different columns or constraints), EF Core migrations or schema checks will conflict.

---

---

## Benchmark Cross-Validation (6 repos, updated tool `feature/renderer-quick-wins`)

### Results table

| Benchmark | Types | Signals | Tokens | Anti-patterns | Constructor deps | Source code | Project tree | Call graph |
|---|---|---|---|---|---|---|---|---|
| **Shamshir** (trading engine) | 230 | controllers,dapper,minimal-apis,serilog,efcore | 5,517 | **22** (FK:3, SL:5, CTX:3, NDI:7, UB:4) | ✅ `BacktestController(IBacktestCommandService,...)` | ✅ 97 lines | ✅ tree | ✅ depth 2 |
| **eShop** (ref e-commerce) | 517 | controllers,minimal-apis,mediatr,efcore,fluentv | 7,187 | **37** (FK:2, SL:8, CTX:3, NDI:20, UB:4) | ❌ MediatR records have no ctors | ✅ | ✅ tree | ✅ depth 1 |
| **CleanArch** (FastEndpoints) | 381 | minimal-apis,efcore,fluentv,fast-endpoints | 4,373 | **0** | ❌ FastEndpoints uses inheritance | ✅ | ✅ tree | ❌ no edges |
| **TodoApi** (minimal API) | 43 | minimal-apis,efcore | 2,637 | **0** | ❌ static extension class | ✅ | ✅ tree | ✅ depth 1 |
| **AutoMapper** (lib) | 2,713 | efcore,automapper | 1,829 | **0** | ✅ (matched types present) | ✅ | ✅ tree | N/A (lib) |
| **Blazor 9.0** | 156 | minimal-apis,efcore | 8,218 | **0** | N/A (full arch) | N/A | ✅ tree | N/A |
| **AspireSamples** | 120 | swagger,controllers,dapper,minimal-apis,efcore | 6,256 | **0** | N/A (full arch) | N/A | ✅ tree | N/A |

### Patterns confirmed across benchmarks

| Feature | Works on | Fails on | Root cause |
|---|---|---|---|
| Project dep tree | ALL | — | ✅ Stable |
| Source code body | ALL with `--around` | — | ✅ Stable; full type text + truncation working |
| Constructor deps | Types with explicit ctor params (Shamshir) | Minimal APIs, FastEndpoints, MediatR records | ✅ Correct behavior — those patterns genuinely have no ctors |
| Deep call graph (depth > 1) | Shamshir (v2: `GetState→TryGetValue`, `StartAsync→Start`) | All others | ⚠️ Field-type resolution needs DI cross-ref that succeeds only when DI map + interface impl map both match. Most codebases don't have this pairing. |
| Anti-pattern detection | eShop (37 hits across 5 patterns) | CleanArch, TodoApi, AutoMapper | ✅ Correct — those repos are well-written with no discard assigns or IServiceScopeFactory use |
| IServiceScopeFactory detection | eShop: `DeviceController`, `ConsentController`, `MigrateDbContextExtensions` | — | ✅ Correctly flags manual DI resolution |
| Fire-and-forget detection | eShop: `RabbitMQEventBus.cs:229` `_=StartNew(...)`, `HooksRepository.cs:18` | — | ✅ Caught real issues |
| NewOutsideDI detection | eShop: `new OrderServices(...)`, `new CreateOrderCommandHandler(...)` in test files | — | ⚠️ Most hits are in test files (`new XxxMockService`) — useful for audit but noisy on test-heavy projects |

### New section output (anti-patterns) — real bugs found

**eShop** (ref e-commerce by Microsoft):
- `RabbitMQEventBus.cs:229`: `_ = Task.Factory.StartNew(...)` — fire-and-forget, exceptions swallowed
- `DeviceController.cs:157,165`, `ConsentController.cs:183,191`: `IServiceScopeFactory.CreateScope` — service locator anti-pattern
- `HooksRepository.cs:7-8`: `ConcurrentDictionary` + `ConcurrentQueue` without eviction — memory leak risk
- `App.xaml.cs:120`: `CancellationToken.None` — can't cancel on app shutdown

**Shamshir** (trading engine):
- `BacktestOrchestrator.cs:85`: `_ = RunAsync(cfg)` — fire-and-forget, the critical bug
- `BacktestOrchestrator.cs:52,117`: `IServiceScopeFactory.CreateScope` — service locator
- `PersistenceService.cs:14,28,42`: `IServiceScopeFactory.CreateAsyncScope` — same pattern
- `BacktestOrchestrator.cs:90`: `ConcurrentDictionary<string, BacktestRunState>` no eviction — memory leak
- `BacktestProgressStore.cs:5`: `Channel<string>` unbounded
- `Program.cs:43,44,46-49`: multiple `CancellationToken.None` — DB migration calls uncancellable

### What the benchmarks validate

1. **No regressions**: All 6 benchmarks ran without crashes. 144/144 tests pass.
2. **Project tree is universal**: Every .NET solution gets the ASCII dependency tree.
3. **Anti-patterns are found on real codebases**: eShop and Shamshir both had genuine issues; clean codebases (TodoApi, CleanArch) correctly report 0.
4. **Constructor deps are correct**: They only show when actual constructors exist — no false positives.
5. **Call graph depth is limited by field resolution**: The DI + interface-impl map works when all 3 align (field type → interface → DI-registered impl) — this happens most often in projects with direct field-to-type resolution (no interface abstraction layer or registry patterns).

**Score**: ~60/100 for complex projects like Shamshir and eShop, ~50/100 for library/FastEndpoints projects where constructor injection isn't used.

Based on tracing every extractor → model → renderer path in the actual source code:

### Data that EXISTS in-memory but is NEVER rendered to output

| Data | Where stored | Why missing from output |
|---|---|---|
| **Project dependency graph** | `SharedAnalysisContext.ProjectGraph` - built by `DependencyExtractor.cs:128` as `Dictionary<string, ImmutableArray<string>>` (project→references) | `DiscoveryPipeline.cs:156-163` never passes `ProjectGraph` to `RenderOptions`. `MarkdownRenderer.cs:101-118` only prints `- ProjectName` (flat list). |
| **Source code bodies** | `TypeDiscovery.SourceBody` - populated by `SourceBodyExtractor` (full text of type declarations) | `MarkdownRenderer.cs` has ZERO references to `SourceBody`. Section never written. |
| **Deep call graph (BFS 5)** | `CallGraph.Edges` — `CallGraphExtractor.cs:98-113` runs proper BFS with depth tracking | Renderer at `MarkdownRenderer.cs:364-376` only prints one caller's **direct** edges (`edges.Take(10)`) — never recurses into callees |
| **DI interface→impl** | `DiRegistrationExtractor` captures `ServiceType` and `ImplementationType` for each registration | Rendered as a flat table — not cross-referenced with entry points or call graph |

### Data that CAN'T be extracted due to code-level gaps

| Data | Gap | Root cause |
|---|---|---|
| **Constructor parameters** | `SyntaxStructureExtractor.cs:146` uses `OfType<MethodDeclarationSyntax>()` — this excludes `ConstructorDeclarationSyntax`. Constructors are never captured. | Renderer at `MarkdownRenderer.cs:164-168` has the code to display "Depends on" but `type.Methods` never contains constructor entries. |
| **Field-to-concrete-type resolution** | `CallGraphExtractor.cs:172-176` resolves `_command.StartAsync` but `calleeType = "_command"` (the field name), not the concrete type `BacktestOrchestrator` | No semantic model (Roslyn workspace not wired for symbol resolution in this extractor). The `ResolveCallee` method only does syntax-level analysis. |
| **In-memory event bus flow** | `EventBusExtractor` only detects MassTransit / NServiceBus. TypedEventBus (in-memory pub/sub) is not recognized. | No signal for custom event buses. Extraction is gated on `masstransit` or `nservicebus` signals. |

### Data that would require NEW extractors

| What's needed | Description |
|---|---|
| **Anti-pattern detector** | Fire-and-forget (`_ = Task()`), `IServiceScopeFactory` usage, `CancellationToken.None`, `new` outside DI, unbounded collections |
| **Cross-DbContext conflict detector** | Detect when two EF Core contexts map the same table |
| **Event flow tracer** | Trace `IEventBus.Subscribe`/`PublishAsync` call chains independent of MassTransit |

### Scorecard: How much of the ideal report is achievable with current code

| Section of ideal report | Auto-detectable with current code? | Effort to close gap |
|---|---|---|
| Project dependency graph (ASCII diagram) | 90% — data exists, just needs rendering (pass `ProjectGraph` to `RenderOptions`, render as tree) | ~20 lines in renderer + 1 line in pipeline |
| Constructor deps on entry points | 20% — fix `OfType<MethodDeclarationSyntax>` to also include constructors; data exists | ~5 lines in SyntaxStructureExtractor |
| Source code bodies in output | 100% — `SourceBody` is populated; renderer just needs a section to print it | ~20 lines in renderer |
| Deep call graph (BFS tree) | 60% — `CallGraph.Edges` has full adjacency; renderer must recursively walk it | ~40 lines in renderer |
| Field-type resolution | 30% — needs Roslyn semantic model (SymbolFinder) to resolve field types | Requires deeper Roslyn integration |
| Anti-pattern flagging | 0% — needs new extractor(s) | ~200+ lines new code |
| Event flow for custom buses | 0% — needs new extractor | ~150+ lines new code |

**Bottom line**: ~50% of the ideal report's data is already being captured by the tool but discarded at render time. Fixing the renderer, adding constructor extraction, and surfacing the project dependency graph would double the tool's usefulness with minimal new extraction logic.


## Summary for LLM

## Quick-Wins Implementation Results (branch: `feature/renderer-quick-wins`)

Four changes delivered +1 section and enriched 2 existing sections:

### Before vs After: `debug-endpoint --full --around BacktestController:Start`

| Metric | Before | After | Delta |
|---|---|---|---|
| Output lines | 230 | 327 | +97 (+42%) |
| Sections | 6 | 7 | +1 (Source code) |
| Tokens | 3,111 | 3,922 | +811 (+26%) |
| Constructor deps shown | ❌ No | ✅ `IBacktestCommandService command, BacktestOrchestrator orchestrator, BacktestProgressStore progressStore, ILogger<BacktestController> logger` |
| Source code | ❌ None | ✅ Full `BacktestController.cs` (97 lines of C#) |
| Call graph depth | 1 level (flat) | Rendered as recursive tree (still depth 1 until field-type resolution is fixed) |

### Before vs After: `architecture --debug`

| Metric | Before | After |
|---|---|---|
| Section | `- ProjectName` (flat list) | ASCII tree with `├── └── │   ` showing parent→child dependencies |
| Example | `- TradingEngine.Web` | `└── TradingEngine.AppHost`<br>`    ├── TradingEngine.Host`<br>`    │   ├── TradingEngine.Domain`<br>`    │   ├── TradingEngine.Risk`<br>`    │   └── TradingEngine.Infrastructure`<br>`    └── TradingEngine.Web` |

---

## V2 Output vs Ideal Report: Honest Assessment

The v2 output (above, `debug-endpoint-full-v2.md`) is compared head-to-head against the ideal report's sections:

### Scorecard per Section

| # | Ideal Report Section | V2 Status | Score | What's missing |
|---|---|---|---|---|
| 1 | **Project dependency graph** | ✅ Rendered as ASCII tree in architecture scenario | 8/10 | Tree shows parent→child references correctly. Missing: edge labels (what kind of dependency?), layer annotations (Domain/App/Infra) on tree nodes. |
| 2 | **Constructor deps on entry points** | ✅ `Depends on: IBacktestCommandService command, BacktestOrchestrator orchestrator, BacktestProgressStore progressStore, ILogger<BacktestController> logger` | 9/10 | Shows all 4 params with types. Missing: resolved concrete implementation for interface types (which class implements `IBacktestCommandService`?) |
| 3 | **Full source code** | ✅ 97 lines of BacktestController.cs shown | 7/10 | Shows ONLY the entry-point type. Missing: `BacktestOrchestrator.cs`, `BacktestRunner.cs` — the types downstream in the call chain. Partial truncation at line 116 (`// ... [1 lines]`). |
| 4 | **Request trace with code at each step** | ⚠️ Call graph shows depth 1 only — `_command.StartAsync` but nothing deeper | 3/10 | `_command` resolves to field name, not `BacktestOrchestrator`. Can't chain into `RunAsync`. Also: no code blocks at each graph node. |
| 5 | **Deep call graph (BFS tree)** | ⚠️ Rendered as recursive tree structure, but only depth 1 because callee keys are field names | 4/10 | Structure is correct: `├─ └─` tree with visited-set protection. Data: field-type resolution needed in `CallGraphExtractor.ResolveCallee`. |
| 6 | **Issues/anti-patterns table** | ❌ Not implemented | 0/10 | Fire-and-forget `_ = RunAsync()`, `IServiceScopeFactory`, `CancellationToken.None`, unbounded dictionary — none detected. |
| 7 | **DI wiring with interface→impl** | ⚠️ Flat table of DI registrations exists. Interface→impl pairs visible but not cross-referenced with entry points. | 5/10 | Table shows `IBacktestCommandService → BacktestOrchestrator` but doesn't link it to `BacktestController`'s constructor param. |
| 8 | **Event flow diagram** | ❌ In-memory `TypedEventBus` events not detected | 0/10 | `EventBusExtractor` only fires for MassTransit/NServiceBus signals. No detection for custom in-memory buses like `IEventBus.Subscribe<T>()`. |
| 9 | **Data model relationships** | ⚠️ Entities listed per DbContext but no FK relationships shown | 4/10 | Shows `TradeResultEntity` has PK `Id` but doesn't show `RunId` FK to `BacktestRunEntity`. Cross-DbContext table overlap not detected. |
| 10 | **Unbounded state detection** | ❌ Not implemented | 0/10 | `ConcurrentDictionary<string, BacktestRunState> _runs` never pruned — would need a detector for unbounded collections. |

### Aggregate: 40 / 100

**What's solid**: The tool now produces a genuine structural understanding — an LLM can see the HTTP surface, the entry point's full code, its constructor dependencies, the project architecture tree, the EF Core entity map, and the DI wiring table.

**What blocks a meaningful security/architecture review**: 
- Call graph stops at depth 1 — can't trace `Start` → `StartAsync` → `RunAsync` → shell-out → DB write
- No anti-pattern detection — 3 critical issues in this codebase (fire-and-forget, service locator, no timeout) are invisible
- Source bodies are only for the entry point — an LLM can't see what `BacktestOrchestrator.RunAsync` actually does
- Event bus wiring not traced — `IEventBus.Subscribe` calls in `Program.cs:168-173` are invisible

### What one more session could deliver

| Change | Effort | Would unlock |
|---|---|---|
| Field-to-type resolution in `CallGraphExtractor` | ~30 lines | Depth-5 call graph tracing `Start` → `StartAsync` → `RunAsync` → `BacktestRunner` → shell |
| Source body for ALL types in call chain | ~20 lines in renderer (collect source bodies from BFS-visited types) | Full code at every step of the request trace |
| Fire-and-forget detector | ~40 lines new extractor | Flags `_ = RunAsync(...)` pattern |
| Unbounded collection detector | ~30 lines new extractor | Flags `ConcurrentDictionary` without eviction |
| IServiceScopeFactory detector | ~20 lines | Flags manual service location patterns |

**Estimated next session**: +5 extractors/features, ~150 lines, score 40 → ~70. The remaining 30 points (in-memory event bus tracing, FK relationships, dual-DbContext conflict) require deeper semantic analysis.

This is a **dual-entry-point .NET 10 trading engine**:

| Entry point | Purpose | Startup |
|---|---|---|
| `TradingEngine.Web` | Backtest dashboard UI + API | `WebApplication` (ASP.NET Core) |
| `TradingEngine.Host` | Headless strategy execution engine | `Host.CreateApplicationBuilder` (Worker Service) |

**Key flow**: `POST /api/backtest/start` → `BacktestController` → `BacktestOrchestrator.StartAsync` (returns immediately, fires `RunAsync` on thread pool) → `BacktestRunner` shells out to `ctrader-cli` → reads trade results from SQLite → saves summary via `IBacktestRunRepository`. Progress is streamed to the browser via SSE through `BacktestProgressStore` (Channels).

**Top risks**: Fire-and-forget task (unobserved exceptions), manual service location throughout `RunAsync`, no timeout on external process execution, unbounded in-memory state (`_runs` dictionary, log queues), dual-DbContext mapping overlap.
