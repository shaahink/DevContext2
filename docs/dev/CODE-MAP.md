# DevContext — Code Map

A source-verified map of the engine: where each responsibility lives, how the ANALYZE→RENDER
pipeline is wired, and where to change things. Companion to `docs/product/AGENT-REFERENCE.md` (what the
tool does) and `docs/dev/briefs/loom-graph-design.md` (why the graph model is shaped this way).

> Verified against `src/` on the `develop` branch. Paths are clickable. When a file/line here stops
> matching the code, fix the map in the same commit — it is only useful while it's true.

## 1. Solution topology

| Project | Role | Notable files |
|---------|------|---------------|
| `DevContext.Core` | The engine — analysis pipeline, graph model, renderers. ~27.5k LOC. Roslyn is a package ref here (`Microsoft.CodeAnalysis.CSharp`), not a separate project. | see §2–§8 |
| `DevContext.Cli` | `devcontext` dotnet tool; Spectre.Console commands; composition root | `Commands/`, `Services/ServiceRegistration.cs`, `Settings/AnalyzeSettings.cs` |
| `DevContext.Contracts` | proto → C# gRPC codegen (`Grpc.Tools`) | generated from the proto at build |
| `DevContext.Server` | gRPC-Web backend; session store; proto mapping | `Endpoints/DevContextGrpcService.cs`, `Sessions/`, `Mapping/ProtoMapper.cs` |
| `DevContext.Mcp` | MCP server — 22 tools over the gRPC RPCs | `DevContextTools.cs` |
| `DevContext.App` | Angular 22 (zoneless, signals) + Tauri 2 desktop | see `src/DevContext.App/AGENTS.md` |

Tests: `tests/DevContext.Core.Tests`, `tests/DevContext.Server.Tests`. Bench: `benchmarks/DevContext.Benchmarks`.
Contract: `proto/devcontext/v1/devcontext.proto` (single source of truth).

## 2. The pipeline — ANALYZE then RENDER

Orchestrator: `src/DevContext.Core/Pipeline/DiscoveryPipeline.cs`. **Analyze once → query many.**

### ANALYZE (`AnalyzeAsync`) — builds the immutable `AnalysisSnapshot`

Extractors are gated per stage by `ExecutionStage`, tier (`Deep` runs last), signals (`ShouldRun`), and
name exclusion (`RunStageAsync`). Sequence:

1. **Stage 1 — DiscoveryAndCacheWarmup** (sequential): file tree, solution/project discovery, parse-once
   cache warmup. → `model.Projects`, `AllSourceFiles`.
2. **Stage 2 — GenericExtraction** (parallel): syntax structure (all type/member decls), package→signal
   mapping, layer classify, DI registrations, Program.cs middleware/worker flow. → `model.Types`,
   `model.Detections`, architecture signals, BodyFacts.
3. **Seal point**: `ResolveFocusPoints` → `SealSignals` → `ApplyArchitectureStyle`
   (`Extractors/Generic/ArchitectureStyleDetector.cs`). After this, signals are immutable.
4. **Stage 3 — SpecificExtraction** (parallel, two waves — cheap detectors, then the `Deep` call graph):
   endpoints, controller actions, MediatR, EF Core, event bus, call graph, anti-patterns, etc.
5. **Semantic-lite (Tier B)** — `Graph2/SemanticLitePopulator.cs`: reads `assets.json`, builds real
   compilations, upgrades BodyFacts (receiver/var/creation types) and CallEdges to `Semantic` resolution.
   Runs only when `BuildFullGraph` or the Debug/Full profile is active; degrades per-project when
   `assets.json` is missing.
6. **Graph assembly** — `Graph/GraphBuilder.cs` `Build(model, scope, bodyFacts)` → `CodeGraph` + entry
   points; then `Graph/MapBuilder.cs` → `MapModel`. (see §4)
7. **Insights** — `ComputeInsights` over the assembled graph (pure, `Insights/`).
8. **Compression** — `RunCompressionAsync` (legacy catalog only; see the invariant below).
9. → `Pipeline/AnalysisSnapshot.cs` (immutable: model + graph + map + options + scenario).

> **Kernel invariant ("token budget out of the kernel", `DiscoveryPipeline.cs` ~L213):** the CodeGraph
> + Map/Trace are assembled *before* scoring/compression and never read `model.Budget`, `IsPruned`, or
> `RoleScore`. The token budget + legacy pruners drive only the legacy catalog RenderPlan (JSON/HTML).
> `BudgetIndependenceTests` locks Map/Trace output invariant across `--max-tokens`. Do not re-couple.

### RENDER (`RenderAsync`) — cheap, repeatable

A `Pipeline/RenderRequest.cs` (format + sections + focus) drives `Pipeline/RenderPlanBuilder.cs` →
`RenderPlan`, consumed by the renderers (§7). The render path is a **client of the query layer** (§6) —
never a re-analysis.

## 3. The graph model — `src/DevContext.Core/Graph/CodeGraph.cs`

The connective-tissue model. Immutable, serialization-clean (primitives only, no live model refs), with
derived inverse adjacency for O(degree) `neighbors`/`find_usages`.

- **`NodeKind`**: `Type, Member, EntryPoint, Service, Message, Store`. One C# class = exactly one `Type`
  node; its *role* is a tag, not a kind.
- **`RoleTags`** (on Type nodes): `command, query, notification, handler, domain-event, integration-event,
  entity, aggregate, service, pipeline, datastore, consumer, runnable`.
- **`EdgeKind`** (caller→callee): `Calls, Sends, Handles, Raises, Consumes, ReadsWrites, Resolves,
  WrappedBy, EntityRelation, ServiceLink, Exposes, DependsOn`. `ServiceLinkTags` sub-classify ServiceLink
  (`bus-publish→consume, grpc, http-via-gateway, refit-direct`).
- **`Resolution`**: `Join` (detection join, high-confidence) · `Syntactic` (heuristic) · `Semantic`
  (Roslyn-verified). Edges upgrade Syntactic→Semantic in place (`CodeGraphBuilder.UpgradeEdge`), never down.
- **`NodeId`** = `(Kind, Key)` where Key is canonical (FQN / "VERB route" / request FQN). The backbone of
  every join — `CodeGraphBuilder.AddNode` merges same-id nodes (union of tags, first-non-null decl info).
- **`Flow`** (precomputed, spine-only, one per entry) lives on the graph (`CodeGraph.Flows`), consumed by
  projections/MCP/UI.

## 4. Graph assembly — `Graph/GraphBuilder.*.cs` (partial class, ~2.5k LOC)

`GraphBuilder` is a partial class split by pass family: `GraphBuilder.cs` (orchestration, 103 LOC) +
`.Nodes` (573) + `.Entries` (506) + `.Seams` (797) + `.ServiceLinks` (343) + `.Flows` (216).
`Build()` runs passes in a deliberate order (see the P1/P2/P3 comments in-file):

1. `AddTypeNodes` + `AddServiceNodes` — seed declarations + runnable-service nodes.
2. **P3 entry builders** (`_entryBuilders`, one per kind — open for extension, §5).
3. `AddHandlerJoins` (Handles) + `AddPipelineBehaviors` (WrappedBy).
4. **P1 map seams**: `AddEntityNodes`, `AddEntityNavigationEdges` (EntityRelation), `AddEventConsumers`
   (Consumes), `AddDiResolves` (Resolves, with multi-impl honesty).
5. **P2 trace seams**: `AddSeamsFromDetectors` (BodyFacts seam detectors, §5) + `AddLambdaSeams` +
   `AddCallEdges` (Calls) + `AddHubScopeEdges` (L3.4 sparse-graph fallback).
6. **Cross-service + the one join**: `AddGrpcServiceLinks`, `AddHttpServiceLinks` (ServiceLink); then
   `EventWiringProjection` off a draft — `Build`+`EmitServiceLinks` (bus ServiceLinks, superseding the
   old `AddBusServiceLinks`) and `BuildTransportPorts`+`EmitPortBridges` (F4: joined port→consumer
   Consumes bridges over an in-repo queue/bus port, so seam/trace can route through the transport).
7. `ComputeFlows` (spine-first per entry) → `DetectLayerViolations` → freeze; then `EnrichEntryTargets`
   / `EnrichEntryGroupPaths` / `EnrichEntryScores`.

## 5. Graph2 — the identity spine & seam layer (`src/DevContext.Core/Graph2/`)

The post-Loom "regex funeral": structured facts + a resolver replace body-scan regexes.

- **`SymbolTable.cs`** — the resolver. `Resolve(SymbolRef)` walks a **monotone tier ladder**
  (`ResolutionTier`): `Semantic` (kept, Law R2 — never downgraded) → `Declared` (exact FQN) →
  `ProjectScoped` (unique short-name in project) → `GlobalUnique` (unique short-name globally) →
  `Ambiguous` (multiple candidates — carried as `Candidates`, **never** `fqns[0]`, Law R1) → `Unresolved`.
- **`BodyFacts.cs` / `BodyFactExtractor.cs`** — one syntax walk per member yields structured `BodyOp`s
  (`InvocationOp`, `CreationOp`, `LocalDeclOp`, `IdentifierUseOp`) that already know their member + line,
  so edges anchor by construction. Cache-versioned (`BodyFactsVersion`).
- **`Seams/`** — `ISeamDetector` implementations (6): `MediatRDispatchDetector`, `BusPublishDetector`,
  `DomainEventRaiseDetector`, `IntegrationEventCreationDetector`, `EntityTouchDetector`, `PlainCallDetector`,
  plus `DispatchClassifier`. Detectors emit `SeamMatch` and **never write the graph** — the assembler
  resolves the target via `SymbolTable` and skips ambiguous ones (Law R1).
- **`SemanticLitePopulator.cs`** (798 LOC) — Tier B: builds compilations from `assets.json`, upgrades
  BodyFacts + CallEdges to `Semantic`.
- **Entry-point builders** — `Graph/EntryPoints/*` (11): `Http`, `Grpc`, `Signalr`, `GraphQl`, `Functions`,
  `Worker`, `OrleansGrain`, `Desktop`, `CliCommand`, `DomainEventHandler`, `MessageConsumer`. Add a new
  entry surface here without touching `GraphBuilder`.

## 6. Query layer — `Graph/GraphQuery.cs` (640 LOC)

The read API over a frozen graph (what the server/MCP/renderers call):
`EntryPoints`, `Map`, `Stats`, `Trace(focus, depth, maxFanOut)`, `Node`, `Neighbors` / `NeighborsView`,
`FindUsages`, `Seam(from, to)`, `ResolveNodeId`, `GetInterestingPoints`, `Impact` / `ImpactFromFiles` /
`BlastRadius`, `Search`, `Find`.
`NeighborsView` (G3.2) is the one implementation behind the kind filter: it walks the rolled edges
ONCE, then filters, so `TotalEdges`/`KindsPresent` always describe the unfiltered set in that
direction and cannot drift from the rows. `Neighbors(id, dir, kind?)` is its `Edges`. The filter sits
ABOVE the C3 roll-up on purpose — a Type node carries almost no edges of its own, so filtering
`_graph.OutEdges(type, kind)` reports a type that writes a table on every request as writing nothing.
The trace itself is built by `Graph/TraceBuilder.cs` (656 LOC); context packs by
`Graph/ContextPackBuilder.cs` (908 LOC — T4: identity header, spine-first budget fill, real
contracts/config/tests sections, per-section provenance); pack staleness by
`Graph/ContextPackVerifier.cs` against analyze-time `Analysis/FileFingerprint.cs` hashes (T4.5);
the library/public-surface view by `Graph/LibrarySurfaceBuilder.cs`.

## 7. Rendering — `src/DevContext.Core/Rendering/` (12 files, ~3.5k LOC)

- `MapRenderer.cs` — the Map narrative (architecture, topology, entries, cross-cutting).
- `MarkdownRenderer.cs` (1162 LOC) — the legacy catalog markdown (large; §9 refactor candidate).
- `HtmlContextRenderer.cs` / `JsonContextRenderer` — `--format html|json`.
- `ReportRenderer.cs` — the `--stats` RunReport (waterfall, extractor table, funnel, cache/graph).

## 8. Server, MCP, CLI

- **Server** (`DevContext.Server`): `Endpoints/DevContextGrpcService.cs` — 26 gRPC handlers (`Analyze` and
  `ObserveToolCalls` stream; the rest are unary). Sessions in `Sessions/` (`AnalysisSessionManager`,
  `AnalysisSession`, `EngineRunner`, `EngineHostCache` — analyze-once, keep the snapshot, serve queries).
  `Mapping/ProtoMapper.cs` converts engine models ⇄ proto.
- **MCP** (`DevContext.Mcp/DevContextTools.cs`): 22 tools — `Analyze, Overview, Resolve, Status, ListSessions,
  CloseSession, Stats, Entrypoints, Map, TopFlows, Trace, Node, Neighbors, Usages, Find, Impact, Seam,
  Config, TestsFor, GetContext, VerifyContext, ReadSource` (server name `devcontext`; public catalog:
  `docs/product/mcp-reference.md`). `Flow`/`InterestingPoints`/`Insights` were folded away in G2.1.
- **CLI** (`DevContext.Cli`): `Commands/` = `Analyze, Init, Query, Report, Scenarios, Version`; options in
  `Settings/AnalyzeSettings.cs`; composition root `Services/ServiceRegistration.cs` (`AddDevContextServices`);
  config `Services/DevContextConfig.cs`.

## 9. Extractors — `src/DevContext.Core/Extractors/` (36 files, ~7k LOC)

`Generic/` (Stage 2, signal-writing) and `Specific/` (Stage 3, signal-gated). All implement
`IDiscoveryExtractor` (`Contracts/`). **Reform in place — do not add new C# extractors.** Detection change
workflow + value model: `docs/product/DETECTION-GUIDE.md`. Biggest: `CallGraphExtractor.cs` (699),
`ArchitectureStyleDetector.cs` (487), `SyntaxStructureExtractor.cs` (441), `EndpointExtractor.cs` (436).

## 10. "Where do I change…?" quick index

| Goal | Start here |
|------|-----------|
| A new entry surface (Blazor, etc.) | `Graph/EntryPoints/` (add an `IEntryPointBuilder`) |
| A new seam / edge from method bodies | `Graph2/Seams/` (add an `ISeamDetector`) + wire in `GraphBuilder.AddSeamsFromDetectors` |
| Detection accuracy (endpoints, handlers, …) | `Extractors/Specific/` + `docs/product/DETECTION-GUIDE.md` |
| Architecture style scoring | `Extractors/Generic/ArchitectureStyleDetector.cs` |
| Symbol resolution / ambiguity | `Graph2/SymbolTable.cs` |
| Map/Trace output text | `Rendering/MapRenderer.cs`, `Graph/TraceBuilder.cs` |
| A new query for server/MCP | `Graph/GraphQuery.cs` → `Server/Endpoints/DevContextGrpcService.cs` → proto → `Mcp/DevContextTools.cs` |
| A new gRPC field/RPC | `proto/…/devcontext.proto` → rebuild `Contracts` + `pnpm gen:proto` → server handler + app data-access |

## Hot spots (size / refactor candidates)

`MarkdownRenderer.cs` (1162) · `DiscoveryPipeline.cs` (967) · `ContextPackBuilder.cs` (908) ·
`SemanticLitePopulator.cs` (798) · `GraphBuilder.Seams.cs` (797) · `CallGraphExtractor.cs` (700).
See `docs/dev/NOTABLE-FINDINGS.md`.
