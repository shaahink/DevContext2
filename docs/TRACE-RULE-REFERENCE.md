# Trace Engine — Rule Reference

> Exhaustive reference of every rule, filter, and check in the DevContext trace engine.
> Source-verified against `src/DevContext.Core/Graph/` on 2026-06-16.

---

## 1. Entry Point Rules

Entry points are the root nodes from which a Trace starts. They are resolved from detections or user input.

### 1.1 Entry Point Kinds

| Kind | Source Detection | Resolved By | Example |
|---|---|---|---|
| `HttpEndpoint` | `EndpointDetection` | `GraphBuilder.AddHttpEntryPoints` | `POST /api/orders` |
| `MessageConsumer` | `MessageConsumerDetection` | (future: EntryPoint inventory) | `OrderCreatedConsumer` |
| `HostedService` | `BackgroundWorkerDetection` | (future: EntryPoint inventory) | `CleanupWorker` |
| `ScheduledJob` | `BackgroundWorkerDetection` (TimedJob) | (future: EntryPoint inventory) | Quartz job |
| `DomainEventHandler` | `MediatRHandlerDetection` (Notification) | (future: EntryPoint inventory) | `OrderShippedHandler` |
| `PublicApi` | `--focus` free-text match | `DiscoveryPipeline.ResolveEntryFromNode` | Any Type/Handler/Service/EntryPoint node |

### 1.2 Entry Resolution from `--focus`

When `--focus <name>` is specified but doesn't match a catalogued HTTP endpoint:

1. Strip `Type:Method` suffix — use only the type name
2. Match against nodes with kind: `Type`, `Handler`, `Service`, `EntryPoint`
3. Match by `Title` equality (case-insensitive) or `Id.Key.EndsWith("." + name)`
4. Prefer the node with the most out-edges (richest trace)

### 1.3 HTTP Entry → Handler Linking

For endpoint detections, the entry node is linked to its handler class via a `Calls` edge:

1. **Named handler** (controller action, FastEndpoints): resolve `HandlerType` to FQN via `NameResolver`, create `Calls` edge to Type node
2. **Named handler with specific method** (`HandlerMethod` known, not `<lambda>`): create a `Member` node (`TypeFqn.MethodName`), create `Calls` edge to it — anchors trace at the specific method
3. **Lambda/inline handler** (minimal API, `HandlerType` contains `=>` or is null/`λ`/`?`): fall back to the type that *contains* the endpoint registration (matched by source file)

### 1.4 Framework/Ambiguous Type Handling

Lambda detections are detected by:
- `HandlerMethod` is `<lambda>` or `<anonymous>`
- `HandlerType` is null, empty, `λ`, `?`, or contains `=>`

When no named handler can be resolved, the owning type is found by matching `SourceFile` in `model.Types`.

---

## 2. Graph Construction Rules (GraphBuilder)

The `GraphBuilder` constructs the `CodeGraph` by joining detection data into typed nodes and edges.

### 2.1 Node Kinds

| NodeKind | Created From | Id Scheme |
|---|---|---|
| `Type` | Every in-scope production `TypeDiscovery` | `NodeId.ForType(FQN)` |
| `Member` | HTTP entry handler with specific method | `NodeId.ForMember(TypeFqn, MethodName)` |
| `EntryPoint` | HTTP endpoint detection | `NodeId.ForEntry("VERB route")` |
| `Request` | MediatR handler detection + `.Send()` body scan | `NodeId.ForRequest(RequestTypeFqn)` |
| `Handler` | MediatR handler + notification handler + message consumer | `NodeId.ForHandler(HandlerFqn)` |
| `Entity` | `EfEntityDetection` | `NodeId.ForEntity(EntityTypeFqn)` |
| `Event` | MediatR notification + integration event + body scan `new XEvent()` | `NodeId.ForEvent(EventTypeFqn)` |
| `DataStore` | DbContext type from `EfEntityDetection` | `NodeId.ForType(DbContextFqn)` |
| `Service` | DI registration implementation + pipeline behaviors | `NodeId.ForService(ServiceFqn)` |

### 2.2 Edge Construction Rules

#### Calls (Type → Type)
- From: `model.CallEdges` (Roslyn syntactic call graph)
- Condition: both caller and callee are real solution type nodes in the graph
- Skip: `callerId == calleeId` (self-calls)
- Resolution: from the `CallEdge.Resolution` field
- Confidence: 0.95 (Semantic) / 0.6 (Syntactic)

#### Sends (Type → Request)
- From: body scan of `.Send()` / `.SendAsync()` / `.Publish()` / `.PublishAsync()` in `SourceBody`
- Two shapes:
  - **Inline**: `.Send(new CreateOrderCommand(...))` → extract type name from `new T()`
  - **Variable**: `.Send(cmd)` → find last `new XType(...)` before the `.Send()` call
- Exception: `IsNoiseType()` check (see §3.3)

#### Handles (Request → Handler)
- From: `MediatRHandlerDetection` (Command/Query)
- Direct join: `RequestType` → `HandlerType` (both resolved to FQN via `NameResolver`)

#### Raises (Type/Handler → Event)
- From: body scan matching method-name set: `{AddDomainEvent, RaiseDomainEvent, AddEvent}` with `new TEvent()` arg
- Also: `new TIntegrationEvent(...)` constructor calls
- Mirrored to Handler node when both Type and Handler nodes exist (B5 fix)
- Method name regex: `methodName\s*\(\s*new\s+(\w+)\s*\(`

#### Consumes (Event → Handler)
- From: `MediatRHandlerDetection` (Notification) — domain events
- From: `MessageConsumerDetection` — integration events (tagged with bus kind)
- Direct join: `RequestType`/`MessageType` → `HandlerType`/`ConsumerType`

#### ReadsWrites (Type/Handler → Entity/DataStore)
- From: `EfEntityDetection` — entity → DbContext edge
- From: body reference scan — if `SourceBody` contains entity name, Type → Entity edge added
- Confidence: 0.5 (Syntactic, approximate)

#### Resolves (interface Type → impl Service)
- From: `DiRegistrationDetection` (Shape == `DirectBinding`)
- Skip: `ImplementationType` is null, `?`, starts with `sp =>`, `_ =>`, `(`, or contains `GetRequiredService`
- Fallback: single-implementor interfaces not covered by DI registrations → edge added with Resolution.Syntactic, Confidence 0.7

#### WrappedBy (Request → pipeline behavior Service)
- From: DI registrations containing `IPipelineBehavior` in `ServiceType`
- Also: `.AddOpenBehavior(typeof(LoggingBehavior<,>))` patterns
- Edge added from every `Request` node to each pipeline behavior node

### 2.3 Pipeline Behaviors — Detection

Two detection patterns:
1. **Direct**: `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))` — `ServiceType` contains `IPipelineBehavior`
2. **MediatR extension**: `.AddOpenBehavior(typeof(LoggingBehavior<,>))` — `ExtensionsUsed` contains `AddOpenBehavior` or `ServiceType == "AddOpenBehavior"`

`CleanTypeRef` strips `typeof(...)`, `nameof(...)`, and generic arity suffix (`<,>`).

---

## 3. Filter Rules (NoiseFilter + GraphBuilder)

### 3.1 Production Code Filter (NoiseFilter)

```csharp
bool IsProductionCode(TypeDiscovery type)
{
    if (IsInTestProject(type.FilePath)) return false;
    if (IsGeneratedPath(type.FilePath)) return false;
    return true;
}
```

### 3.2 Test Project Detection (ProjectClassifier)

A project is classified as test if:
- Name ends with: `Tests`, `Test`, `Specs`, `IntegrationTests`, `FunctionalTests`
- OR any package reference contains: `xunit`, `nunit`, `MSTest`, `Microsoft.NET.Test.Sdk`, `FluentAssertions`, `Moq`, `NSubstitute`, `Shouldly`

Files under a test project's directory tree are excluded. The check is structural (directory prefix), not by type-name suffix (fixes the DDD Specification-class bug).

### 3.3 Generated Code Filter

Paths containing any of these are excluded:
- `/obj/`
- `/bin/`
- `/Migrations/`
- File ends with `.g.cs` or `.Designer.cs`

### 3.4 Noise Type Filter (IsNoiseType)

Names matching these patterns are rejected from Sends/Raises body scans:
- Ends with `Exception`
- Is: `Task`, `ValueTask`, `List`, `Dictionary`, `Array`, `String`, `Object`, `Guid`, `CancellationToken`

### 3.5 Solution Scope Filter

Only types and detections whose `FilePath` is under a project directory of the resolved solution scope are included in the graph. When no `.sln` is resolved, all discovered projects form the scope.

---

## 4. Trace Traversal Rules (TraceBuilder)

### 4.1 Traversal Parameters (TraceOptions)

| Parameter | Default | Purpose |
|---|---|---|
| `MaxDepth` | 6 | Maximum hops from entry (depth 0 = entry node) |
| `MaxFanOut` | 12 | Maximum children expanded per node |
| `Follow` | All edges except `Calls` at priority 7 | Edge kinds to follow |

### 4.2 Traversal Algorithm

```
Visit(node, seam, provenance, resolution, depth):
  1. Add node.Id to visited set
  2. If depth >= MaxDepth → return leaf (Truncated if followable edges remain)
  3. Collect out-edges (with twin-node resolution) filtered by Follow kinds
  4. Rank edges by priority (EdgePriority) then by confidence
  5. Take top MaxFanOut edges
  6. For each edge:
     a. Resolve child node
     b. Extract salient source lines (--detail salient|full)
     c. If framework leaf → leaf step (no further descent)
     d. Else → recurse (depth + 1)
  7. Mark Truncated if edge count > MaxFanOut
```

### 4.3 Edge Priority Ranking

Design intent: edges that carry the core application story rank higher; framework plumbing ranks lower.

| Priority | EdgeKind | Rationale |
|---|---|---|
| 0 | `Sends` | Dispatch is the core story — the trace IS the command path |
| 1 | `Handles` | Handler is the response to dispatch |
| 2 | `Raises` | Domain events are architecturally significant |
| 3 | `Consumes` | Event consumption closes the loop |
| 4 | `ReadsWrites` | Data access — important but secondary |
| 5 | `Resolves` | DI wiring — infrastructure, not behaviour |
| 6 | `WrappedBy` | Pipeline wrappers — cross-cutting, not domain |
| 7 | `Calls` | Direct calls — lowest priority, most likely framework noise |

Within equal priority, edges with `Confidence != 1.0` rank after those with full confidence.

### 4.4 Framework Boundary Stop (IsFrameworkLeaf)

A node is a framework leaf — trace stops descending into it — when:
- `Title.StartsWith("Microsoft.")`
- `Title.StartsWith("System.")`
- `Title == "DbContext"`
- `Title` is `ILogger`, `IMediator`, `ISender`, or `IPublisher`
- `Title` contains `Mediator` (but not `MediatorExtension`)

Framework leaves still appear as trace steps (with provenance/salient) — they are just terminal.

### 4.5 Twin-Node Resolution (OutEdgesWithTwin)

Handlers, Services, and Members share source with their containing Type. After an indirection edge (Handles, Resolves, Consumes) lands on a Handler/Service/Member, the trace follows out-edges from both:
- The landed node itself
- Its **Type twin** (same class, different `NodeId`)

This prevents dead-ending: the class's own call/raise/data edges are on the Type node.

Special case for `Member` nodes: the type key is extracted by truncating at the last `.` (e.g., `MyNs.OrderService.Handle` → `MyNs.OrderService`).

### 4.6 Cyclic/Revisit Handling

When a node ID is already in the `visited` set:
- Return the step immediately (no further descent)
- Set `Truncated = true` if the node still has followable out-edges beyond the visited one
- This prevents infinite loops in mutual-call pairs

### 4.7 Truncation Signals

Truncation (also called "fan-out cut") is signaled on two conditions:
1. **Depth limit**: `depth >= MaxDepth` and node has followable edges — the trace *could* continue but was cut
2. **Fan-out limit**: more eligible edges exist than `MaxFanOut` — excess edges were dropped

Truncated nodes render with `(truncated — more edges beyond depth/fan-out)`.

---

## 5. Summary Collection Rules

After traversal, a post-pass collects:

### TouchedEntities
- All `NodeKind.Entity` nodes in the trace tree
- Rendered as: `TOUCHES  Order, OrderItem, Customer`

### EmittedEvents
- All `NodeKind.Event` nodes in the trace tree
- Rendered as: `EMITS    OrderCreated, OrderShipped`

---

## 6. Resolution & Confidence

| Resolution | Meaning | Typical Confidence | Label in Output |
|---|---|---|---|
| `Join` | Derived by joining two existing detections | 1.0 | `[verified]` (or none — default) |
| `Syntactic` | Resolved by regex/string heuristics | 0.5–0.7 | `[approx]` |
| `Semantic` | Resolved via Roslyn `SemanticModel` symbol | 0.95 | `[verified]` |

Edge confidence by kind:
- `Handles`, `Consumes`, `Resolves` (DI), `WrappedBy`: Join (1.0)
- `Sends`: Syntactic (0.55)
- `Raises`: Syntactic (0.5)
- `ReadsWrites` (body scan): Syntactic (0.5)
- `Calls` (from call graph): 0.95 (Semantic) / 0.6 (Syntactic)
- `Resolves` (single-implementor fallback): Syntactic (0.7)

---

## 7. Architecture Style Detection Rules

Evidence-driven, no name-substring heuristics.

### 7.1 Evidence Sources

| Evidence | Source |
|---|---|
| Project reference counts (fan-in, fan-out per project) | `ComputeReferenceCounts` |
| Folder role conventions (Domain, Application, Infrastructure, Api, Core) | `DetectFolderRoles` — checked against project names + first 200 type file paths |
| Aggregate count | `EfEntityDetection.IsAggregate` |
| MediatR handler counts (command/query + notification) | `MediatRHandlerDetection` |
| Signal presence (MediatR, EF Core, MinimalApi, Controllers, FastEndpoints, Aspire) | `ArchitectureSignals` |
| Aspire AppHost presence | Project name ends with `.AppHost` or references `Aspire.Hosting` |
| Module naming ("module", "bounded", "context" in project name) | Project names |

### 7.2 Scoring Rules

| Style | Conditions | Score Formula |
|---|---|---|
| **Microservices** | Aspire + AppHost present + ≥3 projects total | `min(0.65 + svcCount * 0.05, 0.82)` |
| **CleanArchitecture** | MediatR present + (≥2 DDD folder roles OR ≥1 aggregate OR ≥1 notification handler) | `min(0.5 + dddLayers * 0.1 + aggregateCount * 0.05, 0.95)` |
| **VerticalSlices** | FastEndpoints present | 0.85 (with MediatR) / 0.70 (without) |
| **NLayer** | EF Core + >2 projects, no CleanArchitecture score | 0.60 |
| **MinimalApi** | Minimal APIs present, no MediatR | 0.90 (single project) / 0.65 (multi-project) |
| **ControllerBased** | Controllers present, no MediatR | 0.70 (no MinimalApi) / 0.55 (with MinimalApi, controllers stronger) |
| **ModularMonolith** | ≥2 module-named projects, no Microservices score | `0.55 + moduleCount * 0.05` |
| **Unknown** | No scores | Confidence = 0 |

### 7.3 Conflict Resolution

- **MinimalApi vs ControllerBased**: when both present, compare confidence scores. If `ControllerConfidence >= MinimalApiConfidence`, remove MinimalApi score.
- **NLayer vs CleanArchitecture**: CleanArchitecture takes precedence when scored.
- Best style = `MaxBy(Score)`. If no scores, returns `Unknown`.

### 7.4 Infrastructure Project Filter

Projects excluded from service count in Microservices detection (lowered name contains):
- `.servicedefaults`, `.apphost`, `shared`, `common`, `.eventbus`

---

## 8. Salient Line Extraction

When `--detail salient` or `--detail full` is used, each trace step carries up to 3 trimmed source lines around the provenance site.

Algorithm:
1. Parse `provenance` string: extract file line number after last `:`
2. Split `SourceBody` by newlines
3. Compute body-relative index: `fileLine - bodyStartLine`
4. Take lines `[idx-1 .. idx+1]` (up to 3)
5. Skip empty/whitespace-only lines
6. Output trimmed

If `SourceBody` is null (e.g., EntryPoint nodes), the salient source comes from the **Type twin's** body.

---

## 9. Render Rules (TraceRenderer)

### 9.1 Structure

```
TRACE  <entry.Title>
       <entry.Provenance>

▸ ENTRY  <root.Node.Title>  (<file:line>)
├─ send <Request.Title>  (<file:line>) [approx]
│  ├─ handler <Handler.Title>  (<file:line>)
│  │  ├─ raises <Event.Title>  (<file:line>) [approx]
│  │  ├─ data <Entity.Title>  (<file:line>)
│  │  └─ di <Service.Title>  (<file:line>)
│  └─ pipeline <Behavior.Title>  (<file:line>)
└─ call <Type.Title>  (<file:line>)
   (truncated — more edges beyond depth/fan-out)

TOUCHES  Order, OrderItem
EMITS    OrderCreated
```

### 9.2 Resolution Labels

- `Join` or `Semantic` → `[verified]` (or no label for Join)
- `Syntactic` → `[approx]`

### 9.3 Salient Lines

When detail ≥ Salient, body lines appear indented under the step:

```
├─ send CreateOrderCommand  (src/.../OrdersController.cs:42) [approx]
│      var cmd = new CreateOrderCommand(dto);
│      await _mediator.Send(cmd);
│      return Results.Created($"/api/orders/{id}", id);
```

### 9.4 Depth 0 (Root)

The root step is the entry's owning type. If the entry node was linked via `Calls` to a Member or Type node, the trace starts at that handler, not the abstract entry point.

---

## 10. Map Render Rules (MapRenderer)

The Map is rendered when no `--focus` is specified. Sections (in order):

1. **HEADER**: solution name + project count
2. **STACK**: runtime (TFMs) + detected framework signals (MinimalAPI, Controllers, FastEndpoints, MediatR, EF Core, FluentValidation, MassTransit, NServiceBus, DDD aggregates)
3. **STYLE**: detected style + confidence (high ≥0.8, moderate ≥0.5, low <0.5) + evidence
4. **TOPOLOGY**: project dependency tree (depends-on)
5. **ENTRY POINTS**: grouped by kind (HTTP, Bus, Domain, Background, Scheduled, Public API) — capped at 10 per group
6. **CROSS-CUTTING**: pipeline behaviors, aggregates
7. **PACKAGES**: grouped by category (Web/API, ORM/Data, Mediator/CQRS, Messaging, Validation, Logging, Testing, Cloud, Utilities, Other)
8. **FOOTER**: `→ drill in:  --focus "<entry>"`

---

## 11. Package Categorization

Based on package name (case-insensitive, first match wins):

| Category | Matches |
|---|---|
| Web/API | `aspnet`, `microsoft.asp`, `swashbuckle`, `fastendpoints`, `minimalapi` |
| ORM/Data | `entityframework`, `ef.`, `efcore`, `dapper`, `sqlite`, `sqlserver`, `npgsql`, `mysql`, `cosmos` |
| Mediator/CQRS | `mediatr` |
| Messaging | `masstransit`, `nservicebus`, `rabbitmq`, `azure.messaging`, `amqp` |
| Validation | `fluentvalidation` |
| Logging | `serilog`, `nlog`, `log4net`, `opentelemetry`, `applicationinsights` |
| Testing | `xunit`, `nunit`, `mstest`, `moq`, `nsubstitute`, `bogus`, `fluentassertions`, `shouldly`, `testcontainers`, `coverlet` |
| Cloud | `azure.`, `amazon.`, `aws.` |
| Utilities | `polly`, `automapper`, `scrutor`, `humanizer`, `newtonsoft`, `refit`, `restsharp`, `swagger` |
| Other | Everything else |

Version deduplication: keep highest version by major.minor comparison.

---

## 12. Pipeline Stage Order

1. **Stage 1 (sequential)**: Discovery + cache warmup — project structure, solution parsing
2. **Stage 2 (parallel)**: Generic extractors — signals, file tree, DI registrations, dependencies
3. **Signal sealing**: `Architecture. Seal()` — freeze signals for Stage 3 gating
4. **Style detection**: `ArchitectureStyleDetector.Detect()`
5. **Stage 3 (parallel)**: Specific extractors — endpoints, MediatR, EF Core, call graph, event bus, anti-patterns, Aspire, indirect wiring, source bodies
6. **Focus resolution**: `FocusPointResolver.Resolve()`
7. **Graph assembly** (after Stage 3, before scoring): `GraphBuilder` builds `CodeGraph` + `MapBuilder` builds `MapModel`
8. **Scoring** (shell — pruners run but scores are legacy fallback): `FinalScore = RoleScore`
9. **Compression**: boilerplate removal, namespace grouping, structural dedup, LLM formatting, aggressive truncation
10. **Render**: Map or Trace from snapshot, via `RenderPlanBuilder` (legacy) or direct graph traversal (Map/Trace path)

---

## 13. Error/Fallback Handling

| Scenario | Behaviour |
|---|---|
| `--focus` type not found | Levenshtein distance ≤3 suggestion; falls back to folder proximity |
| Lambda handler can't resolve to type | Fall back to file-matched owning type |
| Short-name collision in `NameResolver` | Prefer FQN matching namespace hint; else pick first |
| Body scan provenance estimate | Count newlines from char offset, rebase to file line via `bodyStartLine` |
| SourceBody extraction failure | Handle gracefully: `catch (Exception ex) { model.AddDiagnostic(Warning, extractorName, ex.Message) }` |
| DI Registration with unrecognized impl | Skip edge if impl is `?`, null, lambda, or `GetRequiredService` |
| Single-implementor becomes ambiguous | Remove from map (multiple impls → no safe edge) |
| No out-edges from entry | Trace is a single root node with no children |

---

## 14. Provenance Estimation

`EstimateProvenance(sourceBody, charOffset, filePath, bodyStartLine)`:
1. Count `\n` characters from start to `charOffset`
2. `fileLine = bodyLineCount + bodyStartLine`
3. Returns `"filePath:fileLine"`

Used by body-scan edge builders (Sends, Raises) where only a character offset in the source body is available.

---

## 15. Map Render — Entry Point Display

Grouped by `EntryPointKind`:
- `HttpEndpoint` → "HTTP"
- `MessageConsumer` → "Bus"
- `DomainEventHandler` → "Domain"
- `HostedService` → "Background"
- `ScheduledJob` → "Scheduled"
- `PublicApi` → "Public API"

If ≤10 entries in a group: list all with provenance. If >10: show first 10 + "... and N more".
