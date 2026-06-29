# Audit: Graph Architecture

> v1.0.5-preview, commit `2c40662`. Based on source code review + live runs against eShop & DntSite.

## Overview

The `CodeGraph` is the connective-tissue model at the heart of DevContext. It replaces the old "detection accumulator" (flat bags of types + detections without edges) with a **typed, directed graph** that connects code entities through eight edge kinds.

Everything downstream — Map, Trace, entry resolution, stats — is derived from this one immutable graph.

---

## Node Model

### Node Identity: `NodeId(Kind, Key)`

| Kind | Key Format | Example |
|---|---|---|
| `Type` | FQN | `Type:DntSite.Web.Features.RssFeeds.Services.FeedsService` |
| `Member` | `TypeFqn.MemberName` | `Member:DntSite.Web...FeedsService.GetFeedChannel` |
| `EntryPoint` | `VERB route` or title | `EntryPoint:GET /Feed` |

**One C# class = exactly one GraphNode.** The `refactor(graph): collapse to one node per class` (commit `6376846`) merged the old per-role node types (Request, Handler, Service, Entity, etc.) into a single `Type` node tagged with `RoleTags`. An edge that implicates a class lands on that one node identity.

### Node Properties

```
GraphNode {
  Id: NodeId          // stable key
  Title: string       // display name
  Kind: NodeKind      // Type | Member | EntryPoint
  FilePath: string?   // declaring file
  Project: string?    // owning project
  SourceBody: string? // full type source text
  Tags: string[]      // role labels
}
```

### RoleTags

These replace the old per-role node kinds. A single class can carry multiple tags:

| Tag | Meaning | Example |
|---|---|---|
| `command` | MediatR IRequest | `CreateOrderCommand` |
| `query` | MediatR IRequest (read) | `GetOrdersQuery` |
| `handler` | MediatR IRequestHandler / INotificationHandler | `CreateOrderCommandHandler` |
| `entity` | EF Core entity | `Order`, `Customer` |
| `aggregate` | DDD aggregate root | `Order` (also tagged `entity`) |
| `service` | Registered in DI | `FeedsService` |
| `domain-event` | INotification domain event | `OrderStartedDomainEvent` |
| `integration-event` | Cross-service event | `OrderPaymentSucceededIntegrationEvent` |
| `pipeline` | MediatR pipeline behavior | `LoggingBehavior` |
| `datastore` | DbContext / DbSet | `OrderingContext` |
| `consumer` | Event bus/queue consumer | `OrderPaymentSucceededIntegrationEventHandler` |

---

## Edge Model

### Eight Edge Kinds

| Edge | Direction | Semantics | Trace Seam Label |
|---|---|---|---|
| `Calls` | Caller → Callee | Direct method call | `call` |
| `Sends` | Type → Request | MediatR Send/Publish dispatch | `send` |
| `Handles` | Request → Handler | IRequestHandler mapping | `handler` |
| `Raises` | Type → Event | Domain/integration event emission | `raises` |
| `Consumes` | Event → Handler | Event handler subscription | `consumes` |
| `ReadsWrites` | Type → Entity | EF Core entity access | `data` |
| `Resolves` | Interface → Impl | DI container resolution | `di` |
| `WrappedBy` | Request → Behavior | MediatR pipeline behavior | `pipeline` |

### Edge Properties

```
GraphEdge {
  From: NodeId
  To: NodeId
  Kind: EdgeKind
  Provenance: string?   // "file:line" of call/dispatch site
  Resolution: Resolution // Join | Syntactic | Semantic
  Confidence: float      // 0..1
}
```

### Resolution Confidence

| Resolution | Confidence | How Established | Trace Label |
|---|---|---|---|
| `Join` | 1.0 | Two detection records joined by type matching (e.g., endpoint detection + MediatR handler detection) | (none) |
| `Syntactic` | 1.0 | String/heuristic matching (e.g., `_mediator.Send(command)` → find type named `CreateOrderCommand`) | `[approx]` |
| `Semantic` | 1.0 | Roslyn `SemanticModel.GetSymbolInfo()` confirmed a symbol exists at this file location | `[verified]` |

**⚠️ CRITICAL:** `Semantic` resolution means "the Roslyn Symbol table confirms a symbol resolves at this file location." It does **NOT** mean "this member is actually invoked from the enclosing method's body." The `CallGraphExtractor` discovers call edges at the **class scope**, meaning every method in a class inherits the entire class's call graph. A `[verified]` edge attached to `CreateItem` may be verified at the file level but belongs to `UpdateItem`'s body. See audit-claims-vs-delivery.md §Critical Bug for the full diagnosis.

---

## Graph Assembly Pipeline

```
Stage 1+2: Generic extractors populate model.Types + model.Detections
                │
                ▼
Stage 3: Specific extractors add more detections
         CallGraphExtractor builds model.CallEdges
                │
                ▼
GraphBuilder.Build(model, scope)
    ├─ 1. AddTypeNodes: one Type node per discovered type
    ├─ 2. AddEndpointNodes: EntryPoint nodes from endpoint detections
    ├─ 3. AddService/Handler/Entity nodes: tags on existing Type nodes
    ├─ 4. JoinNetwork: build edges by joining detections
    │     ├─ Endpoint→Handler: endpoint detection + handler detection
    │     ├─ Sends: body scan for MediatR.Send<T>/Publish<T>
    │     ├─ Handles: request → handler via IRequestHandler<TRequest>
    │     ├─ Raises: body scan for AddDomainEvent / AddAndSaveEventAsync
    │     ├─ Consumes: event → handler via INotificationHandler<TEvent>
    │     ├─ ReadsWrites: DbContext type → entity types
    │     └─ WrappedBy: IPipelineBehavior<TRequest, TResponse>
    ├─ 5. CallGraphIntegration: fold CallEdges into graph
    ├─ 6. NoiseFilter: remove framework artifacts
    └─ 7. Build() → frozen CodeGraph
```

### Node Merge Semantics

When `AddNode` is called with an existing `NodeId`:
- `Tags` = union of old and new tags
- `FilePath` = first non-null (declaration info preserved)
- `SourceBody` = first non-null
- `Project` = first non-null

This is order-independent — a name-only node added by a join is later enriched when its declaration appears, and vice versa.

---

## Traversal: TraceBuilder

The `TraceBuilder` walks the `CodeGraph` forward from an entry point:

```
Entry ──→ (follows Sends/Handles/Calls/Raises/Consumes/ReadsWrites/Resolves)
    │
    ├─ BFS-like, depth-bounded (default 6, max 10)
    ├─ Fan-out capped at 12 children per node
    ├─ Edge priority ranking (Sends=0, Handles=1, Raises=2, Consumes=3,
    │   ReadsWrites=4, Resolves=5, WrappedBy=6, Calls=7)
    ├─ Framework leaf stop: Microsoft.*, System.*, DbContext, ILogger,
    │   IMediator, ISender, IPublisher
    ├─ Dedup by (target, kind) — same edge on twin nodes merged
    ├─ Member→Type bridge: a method node inherits its parent Type's
    │   out-edges
    └─ Revisit detection — a node seen before is not expanded again
```

### Summary Passes

After tree construction, two summary passes run:
- **TOUCHES**: All entity/aggregate nodes reached via ReadsWrites edges from any visited node (including edges cut by fan-out)
- **EMITS**: All domain-event/integration-event nodes in the rendered tree

---

## Live Data: Measured Graph Sizes

### eShop Ordering.API (140 files, 7 projects)

| Metric | Map | Trace (POST /api/orders/) |
|---|---|---|
| Nodes | 193 | 193 |
| Edges | 326 | 319 |
| Entries | 20 | 20 |
| Entries→target | 3/20 | 3/20 |
| Seams: Calls | 78 (13 approx) | 71 (7 approx) |
| Seams: Sends | 13 (13 approx) | 13 (13 approx) |
| Seams: Handles | 18 | 18 |
| Seams: Raises | 14 (14 approx) | 14 (14 approx) |
| Seams: Consumes | 12 | 12 |
| Seams: ReadsWrites | 119 (109 approx) | 119 (109 approx) |
| Seams: Resolves | 18 (2 approx) | 18 (2 approx) |
| Seams: WrappedBy | 54 | 54 |
| Trace depth reached | N/A | 8 (fan-out limited) |

### DntSite (1,342 files, 2 projects)

| Metric | Map | Trace (GET /Feed, d5) | Trace (FeedController, d2+map) |
|---|---|---|---|
| Nodes | 1,475 | 1,475 | 1,475 |
| Edges | 1,247 | 728 | 297 |
| Entries | 94 | 94 | 94 |
| Entries→target | 0/94 | 0/94 | 0/94 |
| Seams: Calls | 1,072 (189 approx) | 553 (46 approx) | 122 (3 approx) |
| Seams: ReadsWrites | 47 (13 approx) | 47 (13 approx) | 47 (13 approx) |
| Seams: Resolves | 128 (105 approx) | 128 (105 approx) | 128 (105 approx) |
| No Sends/Handles/Raises | (No MediatR) | (No MediatR) | (No MediatR) |
| Trace depth reached | N/A | 5 (deep, many truncated) | 2 (signature only) |

---

## Edge Resolution Quality

### eShop (project-level MediatR project)
- **Sends edges**: 13/13 are `[approx]` — body scan finds `services.Mediator.Send()` but resolves the command type syntactically
- **Handles edges**: 18/18 are `Join` — MediatR handler detection joins correctly
- **Raises edges**: 14/14 are `[approx]` — `AddAndSaveEventAsync()` body scan, no semantic verification
- **Consumes edges**: 12/12 are `Join` — domain event handler detection joins
- **Resolves edges**: 2/18 are `[approx]` — DI registration matching mostly fails for this project
- **Calls edges**: 7/71 are `[approx]` — most calls verified by Roslyn semantic model

### DntSite (controller-based, no MediatR)
- **All edges are Calls, ReadsWrites, or Resolves** — no Sends/Handles/Raises seams exist, correct for a non-CQRS project
- **Calls**: 1,072 edges, 189 `[approx]` — 82.4% verified, 17.6% approximate
- **Resolves**: 128 edges, 105 `[approx]` — only 18% verified; most DI edges are syntactically resolved (this is because DntSite uses controller-based injection, not interface→impl constructor injection that's easy to match)
- **ReadsWrites**: 47 edges, 13 `[approx]` — 72.3% from EF Core detection joins

---

## Key Architecture Observations

### What Works Well
1. **The collapse to one node per class** eliminated the twin-node duplication problem. The `Member→Type` bridge in `TraceBuilder` correctly inherits parent Type edges.
2. **Edge deduplication by (From, To, Kind)** prevents graph inflation.
3. **The node merge strategy** (union tags, first-non-null for declaration info) handles the multi-pass accumulation correctly.
4. **NoiseFilter** removes synthetic EF Core names and Migration folder artifacts.
5. **Framework leaf detection** (Microsoft.*, System.*) prevents infinite descent into BCL types.
6. **The edge priority ranking** surface Sends/Handles/Raises above Calls — dispatch is the core story.

### Gaps & Concerns
1. **Entry→target resolution is weak**: eShop gets 3/20, DntSite gets 0/94. Most entries show no dispatch target. The EntryPointResolver cannot bridge from a route to its handler for controller-based projects (only works when MediatR/extractor joins supply the link).
2. **Sends/Raises edges are always [approx]**: They rely on body-string scanning (`_mediator.Send`, `AddDomainEvent`, `AddAndSaveEventAsync`), not Roslyn semantic resolution. The Roslyn semantic tier is gated behind `ExtractionProfile.Debug` but the body scan runs regardless.
3. **No inverse edge indexing**: The graph only stores out-edges. Finding "what sends to this node" requires a full scan.
4. **No persistent graph cache**: Every run rebuilds the graph from scratch. A content-hash-keyed graph cache would make solution re-opening instant. Designed but deferred.
5. **Flood-fan-out on big services**: DntSite's `FeedsService` has the entire app wired through it (calls 30+ other services), causing massive fan-out that hits the 12-child cap and truncates most branches. The ranking helps but structural deduplication doesn't exist.
