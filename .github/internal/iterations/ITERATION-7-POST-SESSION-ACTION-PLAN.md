# DevContext v2 — Iteration 7: Post-Session Action Plan

**Date**: 2026-06-08  
**State**: post-session, `feature/deep-analysis-and-anti-patterns`, 144 tests green  
**Session scope**: Renderer quick wins, call graph depth, anti-pattern detector  
**Score movement**: ~40/100 → ~60/100  

---

## Part 0 — Benchmark Assessment (Post-Robustness)

### Results table (all 7 repos, `feature/robustness-improvements`)

| Benchmark | Types | Dets | Tokens | Anti-patterns | Call graph depth | Notes |
|---|---|---|---|---|---|---|
| **Shamshir** | 234 | 176 | 8,295 | ✅ 9 real (FK:2, SL:2, NDI:1, CTX:3, UB:1) | ✅ depth 5* | Full request trace from Start → RunAsync → DB |
| **eShop** | 517 | 225 | 6,159 | ✅ 12 real (FK:2, SL:2, NDI:2, UB:2, CTX:4) | ✅ depth 1 | All test noise eliminated; new `CreateAsyncScope` found in RabbitMQEventBus |
| **VerticalSlice** | 381 | 109 | 4,320 | ✅ 12 (SL:5, CTX:6, NDI:1) | ❌ no edges | FastEndpoints — inheritance pattern not resolved |
| **TodoApi** | 43 | 56 | 2,628 | ✅ 0 (clean code) | ✅ depth 1 | Minimal APIs with static lambdas — no ctors to resolve |
| **AutoMapper** | 2,713 | 50 | 814 | ✅ 0 (all test noise filtered) | ❌ library | Architecture-only scan; no entry points needed |
| **Blazor 9.0** | 156 | 416 | 8,345 | ✅ 0 | ❌ arch-only | 8K token budget hit — pruning active |
| **AspireSamples** | 120 | 276 | 6,266 | ✅ 0 | ❌ arch-only | All production code clean |

\* Shamshir now shows: `BacktestController.Start → Orchestrator.StartAsync → Start → RunAsync → Complete/GetWriter/EnqueueLog/PushProgress/queue.Enqueue/repo.SaveAsync/sp.GetRequiredService/Guid.NewGuid`

### What improved

| Change | Before | After | Evidence |
|---|---|---|---|
| **Constructor params in field map** | `_scopeFactory` in RunAsync not resolved | Constructor params are now in field map → edges chain deeper | Shamshir depth-5 (was depth-2) |
| **fqnMap collision handling** | First type wins silently — wrong edges | Collision prefers caller's namespace | Prevents cross-namespace type confusion |
| **Anti-pattern test noise** | 50+ false positives in AutoMapper/eShop | < 5 false positives across all benchmarks | AutoMapper: 50→0; eShop: 37→12 |
| **AST-based ServiceScopeFactory** | Matched any string containing "CreateScope" | Only matches exact member name "CreateScope"/"CreateAsyncScope" | eShop found RabbitMQEventBus.cs:190 (new) |
| **CancellationToken variants** | Only `CancellationToken.None` | Also `default(Ct)` and `new Ct()` | VerticalSlice found `new Ct()` in EventDispatcherInterceptor |
| **ArrowExpressionClauseSyntax** | `int Prop => new Foo()` flagged as outside DI | Correctly allowed | VerticalSlice false positive eliminated |
| **Empty catch blocks** | Silent failures | `model.AddDiagnostic` | Debuggability improved |
| **Duplicate code** | 3 copies of EnumerateSourceFilesAsync | 1 shared in ExtractorHelpers | -16 lines, maintenance win |

### Remaining flaws (not regressions — pre-existing)

| Flaw | Root cause | Impact | Suggested fix |
|---|---|---|---|
| **FastEndpoints have no call graph** | Inheritance-based handlers with no methods overridden — `Configure()` and `ExecuteAsync()` are virtual. CallGraphExtractor only processes declared methods. | VerticalSlice (381 types) shows 0 call graph edges | Add scanning of virtual method overrides and base class method declarations via `BaseType` tracking |
| **LINQ/EF chained calls unresolved** | `db.Todos.Where(x => ...)` — the `db` local variable is not in `fieldMap`. Chained calls like this use local vars. | Most LINQ-heavy code shows garbage callee types | Fall back to `model.Types` lookup for the declared type of `db` based on its parameter type in the containing method |
| **TypeDiscovery.AdditionalFilePaths never read** | Populated in SyntaxStructureExtractor but never consumed | Dead field — wastes memory | Remove or wire into related-types rendering |
| **Partial classes: fields invisible across files** | `BuildFieldMap` only sees one syntax tree's declarations | Field resolution misses fields declared in other partial parts | Collect field maps across all files for the same `callerType` (requires pre-pass) |

### Regressions: none

All 7 benchmarks produced results within expected ranges. No crashes, no stack traces, no empty outputs. Token counts changed as expected from anti-pattern filtering (down in eShop/AutoMapper, up in Shamshir from deeper call graph).

### Summary score: ~75/100 → ~80/100

| Capability | Pre-session (v2.0.0) | Post-feature | Post-robustness |
|---|---|---|---|
| Project dependency tree | ❌ flat list | ✅ ASCII tree | ✅ stable |
| Constructor deps | ❌ | ✅ | ✅ stable |
| Source code (entry + chain) | ❌ | ✅ 1 type | ✅ up to 5 types |
| Call graph depth | ❌ depth 1 | ✅ depth 2 | ✅ depth 5* |
| Anti-pattern detection | ❌ | ✅ with noise | ✅ clean, filtered |
| Duplicate code | N/A | 3× EnumerateFiles | ✅ 1× shared helper |
| Empty catch blocks | ❌ silent failures | ❌ still empty | ✅ diagnostics |
| Cancellation in BFS | ❌ none | ❌ none | ✅ checked |

\* Depth-5 only on Shamshir where the DI + interface-impl + constructor-param resolution chain fully succeeds.  

---

## Part 1 — What the session delivered

### Changes merged into `feature/deep-analysis-and-anti-patterns`

| Change | Files | Lines | Impact |
|---|---|---|---|
| Constructor extraction | `SyntaxStructureExtractor.cs` | +27 | `Depends on: IBacktestCommandService command, BacktestOrchestrator orchestrator...` now shows |
| Source code bodies | `MarkdownRenderer.cs` | +30 | Full type source rendered for entry points (97 lines of `BacktestController.cs`) |
| Project dependency tree | `IContextRenderer.cs`, `DiscoveryPipeline.cs`, `MarkdownRenderer.cs` | +40 | ASCII tree (`├── └── │`) replaces flat `- ProjectName` list |
| Deep call graph (recursive renderer) | `MarkdownRenderer.cs` | +40 | Recursive tree rendering with visited-set cycle protection |
| Field-to-type resolution | `CallGraphExtractor.cs` | +130 | `_command.StartAsync` → `BacktestOrchestrator.StartAsync`, `_orchestrator.GetState` → `BacktestOrchestrator.GetState` → `ConcurrentDictionary.TryGetValue` |
| Anti-pattern detector | `AntiPatternDetector.cs` (new) | +265 | Flags FireAndForget, IServiceScopeFactory, NewOutsideDI, CancellationToken.None, UnboundedCollections |
| Anti-patterns rendering | `MarkdownRenderer.cs` | +30 | New `## Anti-patterns detected` section with severity/pattern/description/source |

### Real bugs found

**Shamshir** (trading engine) — 22 detections:
- `BacktestOrchestrator.cs:85`: fire-and-forget `_ = RunAsync(cfg)` (critical — exceptions lost, no await)
- `BacktestOrchestrator.cs:52,117`: `IServiceScopeFactory.CreateScope` (service locator)
- `PersistenceService.cs:14,28,42`: `IServiceScopeFactory.CreateAsyncScope` (same pattern ×3)
- `BacktestOrchestrator.cs:90`: `ConcurrentDictionary` no eviction (memory leak)
- `Program.cs:43,44,46-49`: `CancellationToken.None` on DB migrations (can't cancel on shutdown)

**eShop** (Microsoft reference app) — 37 detections:
- `RabbitMQEventBus.cs:229`: fire-and-forget `_ = Task.Factory.StartNew(...)` (exceptions swallowed in event bus)
- `DeviceController.cs:157,165`, `ConsentController.cs:183,191`: `IServiceScopeFactory.CreateScope` (service locator in production code)
- `HooksRepository.cs:7-8`: `ConcurrentDictionary` + `ConcurrentQueue` without eviction (leak risk)
- `App.xaml.cs:120`: `CancellationToken.None` (can't cancel on app shutdown)

---

## Part 2 — Remaining gaps (code-level root causes)

### Gap A: Same-class method calls don't resolve (call graph chain breaks)

**Root cause**: `CallGraphExtractor.ResolveCallee` handles `IdentifierNameSyntax` by calling `ResolveType("this", ...)`. But `ResolveType` tries to look up `"this"` in the field map, fails, and returns `"this"`. The callee key becomes `this.RunAsync`, which never matches any caller key (which is `TradingEngine.Web.Services.BacktestOrchestrator.RunAsync`).

**Fix location**: `CallGraphExtractor.cs` line ~226 (`ResolveType`), case for `"this"`:
```csharp
if (fieldName == "this" || fieldName == "base")
    return callerType;  // callerType is passed through from the caller
```

**Effort**: ~3 lines. **Impact**: Unlocks `Start` → `RunAsync` → `scope.CreateScope` → deeper BFS chain.

**Before fix**: Call graph shows depth-1 or depth-1.5 (half-resolved).  
**After fix**: Call graph shows full depth-5 BFS traversal: `Start` → `StartAsync` → `Start` → `RunAsync` → `GetTradeStatsAsync`/`ConfigLoader`/`scope.GetRequiredService`/`repo.SaveAsync`.

### Gap B: Source bodies only for entry point, not whole call chain

**Root cause**: `AppendSourceBodies` only renders types from `options.FocusPoints` — the `--around` argument. It should collect all types visited during BFS call graph traversal.

**Fix location**: `MarkdownRenderer.cs` in `AppendSourceBodies` — intersect BFS-visited types from `options.CallGraph` with `model.Types` instead of using focus points only.

**Effort**: ~25 lines. **Impact**: An LLM sees `BacktestController.cs`, `BacktestOrchestrator.cs`, `PersistenceService.cs`, etc. — not just the entry point.

### Gap C: Anti-pattern detector noise (test files, well-known types)

**Root cause**: `AntiPatternDetector.DetectNewOutsideDI` flags `new OrderMockService(...)` in test files, `new ApiVersionHandler(...)` in test setup, `new Exception()` patterns. Test constructors for mocks are standard practice, not anti-patterns. The `IsLikelyService` heuristic over-matches.

**Fix location**: `AntiPatternDetector.cs` — add check for `filePath` containing `\test` or `\Tests\` before flagging, and skip types ending in `MockService`, `Mock`, `Test`.

**Effort**: ~15 lines. **Impact**: Reduces eShop detections from 37 to ~10 (actual production code issues).

### Gap D: `DetectNewOutsideDI` is too narrow for production

**Root cause**: The `IsLikelyService` heuristic checks for keywords like "service", "handler", "manager", "orchestrator" etc. This misses `new BasketService()` in BasketServiceTests (not a problem) but also misses `new ExternalProvider()` in AccountController.cs (caught only because of naming patterns).

**Fix location**: `AntiPatternDetector.cs` — broaden to flag `new` of any non-framework type in method bodies (not constructors, not field initializers), not just "service-like" names.

**Effort**: ~10 lines. **Impact**: Catches more production issues.

### Gap E: Event bus tracing for in-memory buses

**Root cause**: `EventBusExtractor` is gated on `masstransit` or `nservicebus` signals. Shamshir uses a custom `TypedEventBus` with `IEventBus.Subscribe<T>(handler)` / `PublishAsync<T>(evt)`. No extractor detects this pattern.

**Fix location**: New extractor or enhancement to `EventBusExtractor` — detect `IEventBus.Subscribe` and `IEventBus.PublishAsync` calls by walking `Program.cs` and event handler files.

**Effort**: ~150 lines new extractor. **Impact**: Could trace `EngineWorker` → `BarEvaluated` → `BarEvaluationHandler` flow.

### Gap F: DI map bypasses call graph when services are registered via extension methods

**Root cause**: `CallGraphExtractor` builds `diMap` from `DiRegistrationDetection`. But many DI registrations use extension methods like `services.AddDbContext(...)`, `services.AddHostedService<EngineWorker>()`, `services.AddScoped<IBacktestRunRepository, SqliteBacktestRunRepository>()`. The `DiRegistrationDetection` for extension methods often has an empty `ImplementationType` or a placeholder, so they're filtered out.

**Fix location**: `DiRegistrationExtractor` — improve extraction of implementation types from generic extension methods (e.g., `AddHostedService<T>` → implementation type = `T`). Or fall back to type-level analysis in `CallGraphExtractor`.

**Effort**: ~30 lines. **Impact**: More interface→impl pairs resolved in call graph chaining.

---

## Part 3 — Prioritized action plan

### P0: Fix same-class method resolution (next session, ~5 min)

| Step | File | Change |
|---|---|---|
| Pass `callerType` to `ResolveType` | `CallGraphExtractor.cs` | Add parameter to `ResolveType` call |
| Handle "this" and "base" | `CallGraphExtractor.cs` line ~226 | `if (fieldName is "this" or "base") return callerType;` |
| Build + test | — | `dotnet test` |

### P1: Show source bodies for whole call chain (~15 min)

| Step | File | Change |
|---|---|---|
| Add BFS-visited types to focus set | `MarkdownRenderer.cs` `AppendSourceBodies` | Intersect `model.Types` with BFS-visited keys from `CallGraph.Edges` |
| Collect matched type names | `MarkdownRenderer.cs` | Walk all edges, extract callee types, look up in model |

### P2: Filter anti-pattern noise (~10 min)

| Step | File | Change |
|---|---|---|
| Skip test files | `AntiPatternDetector.cs` | Add `filePath.Contains("\test") || filePath.Contains("\Tests\")` checks |
| Skip well-known mock types | `AntiPatternDetector.cs` | Extend type skip list |

### P3: Push + open PR (~5 min)

| Step | Command |
|---|---|
| Push to origin | `git push origin feature/deep-analysis-and-anti-patterns` |
| Create PR | `gh pr create --base develop` |

### P4: Re-run benchmark matrix (~10 min)

| Benchmark | Command |
|---|---|
| Run all 6 with new build | `foreach (repo) { dotnet run ... scenario debug-endpoint --profile full --around <entry> }` |
| Check depth increase | Verify `BacktestOrchestrator.Start` → `RunAsync` → deeper chain visible |

### P5: Broadening DI resolution for call graph chain (~20 min)

| Step | File | Change |
|---|---|---|
| Extract impl from generic extensions | `DiRegistrationExtractor.cs` or `CallGraphExtractor.cs` | Parse `AddHostedService<T>` → impl = `T` |
| Fall back to type hierarchy | `CallGraphExtractor.cs` | When DI map fails, use `interfaceImplMap` from model.Types (already exists but underused) |

### P6: In-memory event bus tracing (~2 hrs, separate session)

| Step | File | Change |
|---|---|---|
| New `InMemoryEventBusExtractor` | New file | Detect `IEventBus.Subscribe<T>` + `PublishAsync<T>` patterns |
| Add to renderer | `MarkdownRenderer.cs` | New section: `## Event flow` |

---

## Part 4 — Score trajectory

| Milestone | Score | Key wins |
|---|---|---|
| Pre-session (v2.0.0) | ~40/100 | Endpoint routes, flat project list, DI table, no source |
| Post-session (current) | ~60/100 | Project tree, constructor deps, source code, anti-patterns, partial call graph depth |
| After P0+P1 (same-class + chain bodies) | ~75/100 | Full BFS call graph with source code at every node |
| After P5 (DI extension resolution) | ~80/100 | More call edges resolved from AddDbContext/AddHostedService patterns |
| After P6 (in-memory event bus) | ~85/100 | Event flow diagrams for custom IEventBus implementations |
| After semantic analysis (long-term) | ~95/100 | FK relationship detection, field-type resolution via Roslyn semantic model |

---

## Part 5 — Files to modify in P0-P3

All files are on `feature/deep-analysis-and-anti-patterns` branch.

```
src/DevContext.Core/Extractors/Specific/CallGraphExtractor.cs   (P0: +3 lines)
src/DevContext.Core/Rendering/MarkdownRenderer.cs               (P1: +25 lines)
src/DevContext.Core/Extractors/Specific/AntiPatternDetector.cs   (P2: +15 lines)
```

Total estimated code: **~43 lines**, 3 files.
