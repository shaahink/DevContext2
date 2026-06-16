# PLAN-11 Handover — Review Notes for Fresh Agent

> Commit range: `ef42a51` → `4875b19` (3 commits on `feature/trace-engine`)
> Gate: **PASS** (4/4 — build · fast tests · eval tests · CLI matrix)
> All 299 unit tests pass, all 7 eval tests pass, full solution builds clean

## What this plan delivered

PLAN-11 added a **Desktop UI surface** for the trace engine (Part A), **"flesh"** that turns the trace from a tree of names into a story with salient body lines (Part B), a **validation probe** comparing the engine trace against the ideal target (Part C), and **coverage gap fixes** for Aspire signal detection, Mediator library support, and architecture style classification (Part D).

---

## Files changed (by part)

### Part A — Desktop UI (commit `8a2ac4b`)
| File | Change |
|------|--------|
| `src/DevContext.Desktop/Services/AnalysisService.cs` | `AnalysisOptions` + `AppSettings` got `Depth`/`Detail` fields; `IntentInput` passes depth through |
| `src/DevContext.Desktop/ViewModels/MainViewModel.cs` | `SelectedEntry`, `Depth`, `Detail` properties; `HasGraph`, `Entries`, `GroupedEntries` computed; `DebouncedRender()`; `RenderRequest` passes Entry/Depth/Detail |
| `src/DevContext.Desktop/Components/ConfigPanel.razor` | Entry picker `<datalist>`, depth slider, detail segmented control (visible only when `HasGraph`) |
| `src/DevContext.Desktop/Components/OutputPanel.razor` | Monospace `<pre>` for Map/Trace output when `HasGraph`; sections button hidden when `HasGraph` |
| `src/DevContext.Desktop/DevContext.Desktop.csproj` | Added direct `ProjectReference` to `DevContext.Core` (needed for `DevContext.Core.Graph` types) |

### Part B — Trace Flesh (commit `abe1395`)
| File | Change |
|------|--------|
| `src/DevContext.Core/Graph/CodeGraph.cs` | `GraphNode.SourceBody` property; `CodeGraphBuilder.Nodes` enumerator |
| `src/DevContext.Core/Graph/GraphBuilder.cs` | `AddTypeNodes` copies `SourceBody` to Type nodes; `AddHandlerJoins` copies to Handler nodes; `AddPipelineBehaviors` (new) detects `IPipelineBehavior`/`AddOpenBehavior` registrations → `WrappedBy` edges; `AddHttpEntryPoints` creates `Member` nodes for named handlers (B4); `AddRaises` mirrors edges to Handler nodes (B5); `AddSends`/`AddRaises` now include `Provenance` via `EstimateProvenance()` |
| `src/DevContext.Core/Graph/TraceBuilder.cs` | `TraceStep.Salient` field; `ExtractSalient()` helper extracts 3 lines around provenance from SourceBody; `CollectSummaries()` fills `TouchedEntities`/`EmittedEvents`; `OutEdgesWithTwin` bridges `Member` → Type |
| `src/DevContext.Core/Rendering/TraceRenderer.cs` | `RenderStep` honours `TraceDetail`: Signature=names only, Salient=names+body lines, Full=names+full method (future); renders `TOUCHES`/`EMITS` footer |

### Parts C+D — Probe + Coverage (commit `4875b19`)
| File | Change |
|------|--------|
| `docs/reports/probe-results.md` | New. Engine trace quality evaluation against `IDEAL-OUTPUT-TARGET.md` §2 hand-built target. Scored 5.55/10. Recommends funding Parts F+G. |
| `src/DevContext.Core/Extractors/Generic/DependencyExtractor.cs` | `"Mediator"` key added to `PackageSignalMap` (martinothamar library); Aspire key changed from `"Microsoft.Aspire"` → `"Aspire.Hosting"`; `TryMatchSignal()` prefix-matching helper |
| `src/DevContext.Core/Extractors/Generic/ArchitectureStyleDetector.cs` | Microservices requires explicit `hasAppHost` check; score capped at 0.82 (below VerticalSlices 0.85) |

---

## Architecture decisions to be aware of

1. **`GraphNode.SourceBody`** stores the full type declaration text on graph nodes. This enables `TraceBuilder.ExtractSalient()` to pull 3 lines around an edge's provenance. Source bodies are populated in `GraphBuilder.AddTypeNodes` (from `TypeDiscovery.SourceBody`) and copied to Handler/Service/Member nodes too.

2. **`OutEdgesWithTwin`** in `TraceBuilder` is the critical indirection bridge. It now handles `NodeKind.Member` nodes by extracting the type FQN from the member key (`"TypeFqn.MethodName"` → `"TypeFqn"` via `ExtractTypeKey()`). This is what lets per-method-anchored entries (B4) continue tracing into the type's call/raise/send edges.

3. **Salient line provenance** requires edges to carry `Provenance`. The body-scan methods (`AddSends`, `AddRaises`) now use `EstimateProvenance()` which counts newlines from a character offset. Edges created via `AddHttpEntryPoints`, `AddHandlerJoins`, and `AddCallEdges` already have provenance from their source data.

4. **Deskop UI needs `--profile debug`** to populate `SourceBody` on types. The `SourceBodyExtractor` runs in `Stage3Specific` and gates on `ExtractionProfile.Debug` or `Full`. Without source bodies, salient lines won't appear in the trace.

5. **The FQN `DevContext.Core.Graph.*` namespace** is not accessible via transitive reference from the Desktop project — a direct `ProjectReference` to `DevContext.Core` had to be added to `DevContext.Desktop.csproj`.

---

## How to verify

### Quick smoke test
```bash
# Full build
dotnet build DevContext.slnx

# All tests (fast + eval)
dotnet test --filter "Category!=CliSmoke"

# Gate script
.\eval\gates.ps1
```

### Engine trace demo
```bash
dotnet run --project src\DevContext.Cli -- analyze "eval-repos\eShop\src\Ordering.API" --focus "POST /api/orders/" --depth 8 --detail salient --profile debug
```
Should show a trace with ☆ ENTRY → `call OrdersApi` → `send CreateOrderCommand` (with salient body line) → `handler CreateOrderCommandHandler` → `raises OrderStartedIntegrationEvent` (with 3 salient lines) → `EMITS OrderStartedIntegrationEvent`.

### Desktop UI smoke
Launch the Desktop app, point it at `eval-repos/eShop/src/Ordering.API`, click Analyze. After analysis completes:
- The entry picker datalist should appear below Focus (with entries like `POST /api/orders/`)
- Pick `POST /api/orders/` — the Human tab should show the monospace trace
- Clear the entry — the Human tab should show the Map
- Move the depth slider — the trace should re-render without re-analyzing

---

## Known gaps (what PLAN-11 did NOT address)

| Gap | Priority | Notes |
|-----|----------|-------|
| **Idempotency wrapper** (`IdentifiedCommand<CreateOrderCommand,bool>`) | P1 | Double-send pattern not detected. The Sends edge from IdentifiedCommandHandler to CreateOrderCommand requires following the inner dispatch. |
| **Domain event chain** (`OrderStartedDomainEvent` → `ValidateOrAddBuyer...`) | P1 | B5 mirrors Raises to Handler but domain events raised via aggregate ctor (`AddDomainEvent`) aren't detected by the regex — only integration events are. |
| **Pipeline behavior detection in eShop** | P1 | eShop registers pipelines via MediatR config API (`cfg.AddOpenBehavior(...)`), not `services.AddTransient(typeof(IPipelineBehavior<,>), ...)`. Current detection only handles the latter. |
| **Save-time dispatch boundary** (`SaveEntitiesAsync` intermediary) | P2 | The UoW dispatch pattern (domain events dispatched at SaveChanges) is not modelled. |
| **Cross-service integration events** | P2 | Multi-project scope (archetype C) not yet implemented — `GraphBuilder.Build` runs per-project. |
| **AutoMapper library archetype (D)** | P2 | Falls through to generic Map. Would need surface-map-by-capability rendering. |
| **Semantic resolution** (`[approx]` → `[verified]`) | P1 | Plan's Part F — would upgrade all syntactic edges to Roslyn `SemanticModel` resolved edges. |
| **Persistent index** | P1 | Plan's Part G — would cache the graph keyed by content hash for instant re-analysis. |
| **`<dynamic>` FastEndpoints routes** | Aspirational | Routes set via `Get(variableName)` render as `<dynamic>`. Requires resolving non-literal route arguments. |
| **Entry method-level for minimal API lambdas** | Aspirational | Lambda handlers still fall back to owner type. B4 only helps named handlers. |

---

## Probe result: trace scored 5.55/10 vs ideal target

The engine trace captured ~40% of the hand-built ideal trace from `IDEAL-OUTPUT-TARGET.md` §2. Key findings:
- **B1 Salient lines** are the single highest-impact feature — they turn names into a story
- **Navigation** scores 7/10 — file:line provenance on every hop
- **Completeness** scores 4/10 — lacks domain events, idempotency, pipeline, save boundary
- **Explanation** scores 5/10 — seam labels (`send`/`handler`/`raises`/`call`) clarify control transfer

The probe recommends **funding Parts F (SemanticSymbolResolver) and G (Persistent Index)** — the engine architecture is sound; the gaps are in detection patterns, not in the engine design.
