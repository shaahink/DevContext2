# Loom — The DevContext Graph System Design

> **Status: authoritative.** This document is the design the Loom sessions implement.
> Do not re-architect inside a session; if reality contradicts this doc, stop and record
> the contradiction in the tracker, then implement the smallest honest deviation.
> Written 2026-07-07 after the Meridian close-out audit (`eval-results/2026-07-07/SESSION-AUDIT.md`).
> Supersedes the seam-joining architecture of `GraphBuilder.cs` (which it absorbs, not rewrites).

## 0. Why (one paragraph)

Every confirmed defect in the audit — fabricated cross-project wiring, the depth-1
checkout trace, "(unknown)" service grouping, "Default" scope buckets, near-empty graphs
on non-CQRS repos — reduces to one disease: **identity established by string luck,
consumed as fact**. Loom keeps the good bones (immutable CodeGraph, roles-as-tags,
provenance on edges, IEntryPointBuilder) and replaces the three rotten organs: stringly
joins → typed references with resolution tiers; regex body scans → one structured
body-facts pass; project-as-fake-Type → Service as a first-class node.

Non-goals: no LLM in the core; no full rewrite (the migration is strangler-style,
one organ at a time); no MSBuild dependency in the default path (§6).

## 1. The model (DevContext.Core/Graph2 — new namespace, migrated into Graph/ at the end)

### 1.1 Identity

```csharp
/// One symbol, one id, forever. Never construct from a bare display string.
public readonly record struct SymbolId(SymbolKind Kind, string Canonical);
public enum SymbolKind { Service, Project, Type, Member, Endpoint, Message, Store, ConfigKey }
```

- `Canonical` for Type = metadata-style FQN (`Ns.Outer+Nested`, arity `` `1 `` kept).
  For Member = `TypeFqn::MethodName(paramCount)` — parameter count, not full signature,
  is enough to split overload families without semantic info.
- **The only factory is `SymbolTable`** (§2.2). `NodeId.ForType(anyString)` dies.
  A grep gate (`scripts/loom-guards.ps1`) fails the build if `SymbolId(` is constructed
  outside SymbolTable and test code.

### 1.2 References — the anti-string-luck type

```csharp
/// A *mention* of a symbol in source, before/after resolution. This is the type that
/// crosses the extractor→graph boundary. Raw strings never do.
public sealed record SymbolRef
{
    public required string Text { get; init; }          // as written: "CheckoutBasketCommand"
    public required RefSite Site { get; init; }         // file, span, project — resolution context
    public SymbolId? Resolved { get; init; }            // null until Resolve pass
    public ResolutionTier Tier { get; init; }           // how Resolved was established
    public ImmutableArray<SymbolId> Candidates { get; init; } // ≥2 ⇒ ambiguous
}

public enum ResolutionTier
{
    Declared,       // the site IS the declaration
    Semantic,       // Roslyn SemanticModel bind (tier B/C populator)
    FileScoped,     // unique via usings/namespace of the containing file
    ProjectScoped,  // unique short-name within the containing project
    GlobalUnique,   // unique short-name across the solution
    Ambiguous,      // >1 candidate — recorded, NEVER auto-picked
    Unresolved,     // external/unknown — kept as leaf, marked
}
```

**Law R1 — no silent winners.** When candidates > 1 and no tier disambiguates, the ref
stays `Ambiguous` with candidates attached. Traversals, traces, flows, and default
projections **skip Ambiguous edges**; the Stats page and `insights` report their count.
This single law deletes the RazorPages fabrication class of bugs.

**Law R2 — tier is monotone.** A pass may upgrade a ref's tier (Syntactic→Semantic),
never downgrade or overwrite a `Declared`/`Semantic` resolution with a name guess.

### 1.3 Nodes and edges

Keep `GraphNode`/`GraphEdge`/`CodeGraph` shape (immutable, frozen adjacency, tags,
provenance, confidence) with these changes:

- `NodeKind` gains **`Service`** and **`Message`** and **`Store`**:
  - `Service` = a *runnable* deployable (has an entry assembly / Program.cs / exe). A
    class library is NOT a Service. `ServiceLink` edges connect Service nodes only.
  - `Message` = command/query/event/notification contract (currently a tagged Type —
    stays a Type node if declared in-scope, but gets `Message` kind when it is *only*
    known as a contract, e.g. from another service's bus publish).
  - `Store` = DbContext/queue/topic/blob container — the things data touches.
- **`Project` and `Service` stamped at node creation** for every declared node; the
  assembler rejects (`Debug.Assert` + counter) declared nodes without them. This kills
  "(unknown)" impact groups and "Default" scope buckets at the root.
- `GraphEdge.Resolution` becomes the `ResolutionTier` enum above (Join maps to Declared,
  Syntactic→ProjectScoped/GlobalUnique per how it was actually resolved).
- New `EdgeKind.Exposes` (Service → Endpoint) and `EdgeKind.DependsOn` (Project →
  Project) so topology stops living in a parallel `MapModel` structure.

### 1.4 Flow — first-class

```csharp
public sealed record Flow(
    string Id,                       // stable: entry key
    SymbolId Entry,
    ImmutableArray<FlowStep> Steps,  // ordered spine: what a reader walks
    ImmutableArray<SymbolId> Touches,   // stores/entities on the SPINE (not nav-reachable!)
    ImmutableArray<SymbolId> Emits,     // events raised on the spine
    ImmutableArray<ServiceHop> Hops);   // cross-service transitions with transport + evidence

public sealed record FlowStep(SymbolId Node, EdgeKind Via, ResolutionTier Tier, string? Provenance);
```

Flows are computed once at assembly (top-N by score + on-demand for the rest), stored on
the graph, and consumed identically by: UI stepper strips, Atlas, MCP `flow`, `top_flows`,
Context Studio seeds. **Fix baked in:** `Touches` = stores/entities the spine's members
actually read/write (ReadsWrites edges from spine members only) — never EntityRelation
reachability (audit E5).

## 2. The pipeline

```
per file ─┐
          ├─ Parse (one Roslyn parse per file, shared)            [exists: keep]
          ├─ StructureFacts: types, members, baselists, usings    [exists: absorb]
          ├─ BodyFacts:      the NEW single body pass (§2.1)      [replaces 18 regex sites]
          └─ ArtifactFacts:  .csproj / proto / appsettings / yarp [regex allowed HERE only, behind TextProbe]
                    │
        SymbolTable build (§2.2)  →  Resolve pass (all SymbolRefs, tiered)
                    │
        Populators (§6): TierA syntax ─ TierB semantic-lite ─ TierC msbuild (opt-in)
                    │
        CodeGraphAssembler (§2.3): entry builders + seam detectors + joins
                    │
        Inference passes (§2.4): layers, features, services, style, violations
                    │
        Flows (§1.4)  →  frozen CodeGraph  →  Projections (§3)
```

### 2.1 BodyFacts — the regex killer

One walk over each method body's syntax tree (we already have the trees) yields:

```csharp
public sealed record BodyFacts(SymbolId Member, ImmutableArray<BodyOp> Ops);

public abstract record BodyOp(int Line);
public sealed record InvocationOp(int Line, string? ReceiverText, SymbolRef? ReceiverType,
    string MethodName, ImmutableArray<SymbolRef> GenericArgs,
    ImmutableArray<ArgFact> Args) : BodyOp(Line);
public sealed record CreationOp(int Line, SymbolRef Type) : BodyOp(Line);
public sealed record LocalDeclOp(int Line, string Name, SymbolRef? DeclaredType,
    /* var x = expr → what expr yields when statically obvious: */ SymbolRef? InferredFrom) : BodyOp(Line);
public sealed record IdentifierUseOp(int Line, string Identifier) : BodyOp(Line); // for data-touch matching
```

Rules:
- Built **during the existing parse**, cached with the file (content-keyed, same cache
  as today's extractors). Never re-parse a type body (today `BuildMethodSpans` re-parses
  every type — delete).
- String literals never enter Ops (syntax nodes make `StripStringLiterals` obsolete).
- Line numbers come from the syntax tree (`GetLineSpan`) — `EstimateProvenance` dies.
- `LocalDeclOp.InferredFrom` covers exactly the statically-obvious cases the current
  regexes chase: `new X(...)`, `expr.Adapt<X>()`, `expr.Map<X>()`, factory `Create<X>()`,
  awaited variants. Anything else stays null — TierB semantic resolves it or it stays
  unresolved. No right-to-left regex scans.

**Seam detectors** become small classes over BodyFacts:

```csharp
public interface ISeamDetector
{
    // Emits matches; NEVER writes to the graph. Pure: facts in, seams out.
    IEnumerable<SeamMatch> Detect(BodyFacts body, SeamContext ctx);
}
public sealed record SeamMatch(SymbolId Origin, EdgeKind Kind, SymbolRef Target,
    float Confidence, string Provenance, string DetectorId);
```

Initial detector set (each ≤ ~100 lines, one file, one test fixture):
`MediatRDispatchDetector` (Send/Publish incl. variable + Adapt patterns via LocalDeclOp),
`DomainEventRaiseDetector`, `IntegrationEventCreationDetector`, `EntityTouchDetector`
(IdentifierUseOp whole-word — the current `ContainsWholeWord` logic, now trivially exact),
`BusPublishDetector`, `HttpCallDetector` (HttpClient/Refit), `GrpcCallDetector`.
The dispatch-receiver catalog (`DispatchSeamCatalog`) survives as data for these detectors.

### 2.2 SymbolTable

Replaces `NameResolver`. Built once per solution scope from StructureFacts:

- indexes: FQN → SymbolId; (short name, project) → ids; short name → ids; file → project → service.
- `Resolve(SymbolRef) → SymbolRef` applies the tier ladder (§1.2) and *records* the tier.
- Exposes `AmbiguityReport` (count per short name) consumed by Stats + insights.

### 2.3 CodeGraphAssembler

The orchestrator that replaces GraphBuilder's god-method — target ≤ 300 lines, zero
regex, zero string parsing, **no static state** (audit E8):

```csharp
public sealed class CodeGraphAssembler(SymbolTable symbols, NoiseFilter noise,
    IReadOnlyList<IEntryPointBuilder> entryBuilders,
    IReadOnlyList<ISeamDetector> seamDetectors,
    IReadOnlyList<IInferencePass> inferencePasses)
{
    public AssemblyResult Assemble(FactModel facts, SolutionScope scope);
}
```

Order: declared nodes → entries (existing builders, ported to SymbolRef) → seam matches
→ cross-service joins (bus/grpc/http — now Service→Service on real Service nodes, taking
publishers from seam matches, not a static field) → inference → flows → freeze.
Violations computed on the builder's live adjacency (no double `Build()`).

### 2.4 Inference passes

`IInferencePass { void Run(GraphMutator g, FactModel facts); }` — each small, evidenced:
`LayerInference` (namespace/project-ref direction; keeps current label logic),
`FeatureInference`, `ServiceBoundaryInference` (runnable detection: OutputType exe,
Program.cs, Sdk.Web — this feeds Service nodes), `ArchStyleInference` (evidence-listed;
must down-weight signals when >50 projects share no solution — audit E4),
`LayerViolationInference`, `HotspotInference`.

## 3. Projections — one truth, three renderers

```csharp
public interface IGraphProjection<TOut> { TOut Project(CodeGraph graph, ProjectionOptions o); }
```

- `ServiceMapProjection` → Service nodes only (real names! runnables only!), transports
  on links, gateway/bus lanes. Consumed by: Home hero, Atlas diagram, MCP `overview`.
- `LayerBandProjection` → per-NODE layer bands + violation edges (unblocks the full M7.2
  lens; needs the per-node RPC listed in Meridian §10 P1).
- `FlowListProjection`, `EntryTableProjection` (the table lens columns incl.
  shares-handler-with), `ContextPackProjection` (Context Studio + `get_context` — server
  round-trip, closing Meridian Trap A).
- MCP text renderers render *projections*, never walk the graph ad hoc.

## 4. Graph consumption API (kill the foreach-soup)

`GraphQuery` grows the only three primitives consumers actually need:

```csharp
graph.Find("Order")            // ranked: exact > prefix > word-boundary; Types>Members; by degree — fixes MCP resolve
graph.Walk(entry).Follow(Sends, Handles, Consumes).SkipAmbiguous().ToFlow()
graph.Services / graph.Flows / graph.Types.InService(svcId)   // indexed accessors
```

Ranked `Find` is shared by MCP `resolve`/`find`/`usages`/`impact` (one resolution path,
consistent short-name behavior, "did you mean" on miss — audit §4).

## 5. Honesty surfaces (design-level requirements)

- Every projection carries `Coverage { AmbiguousEdges, UnresolvedRefs, SkippedFiles }`.
- Unknown symbol ⇒ **error object with suggestions**, never empty-zero results (MCP).
- Sparse graph (non-CQRS repo) ⇒ the report SAYS "call-spine only, N seams" instead of
  pretending; archetype-specific projections (§7 L7) fill the gap honestly.

## 6. Population tiers — the MSBuild decision

**Verified facts** (2026-07 web check + docs): `MSBuildWorkspace` needs a matching .NET
SDK installed and located via `MSBuildLocator`; self-contained bundling is explicitly
problematic (SDK tasks pin to their own runtime); design-time builds want a restore;
Buildalyzer runs MSBuild out-of-process and is the workable cross-platform wrapper but
still requires the SDK + restore on the user's machine.

**Decision: MSBuildWorkspace/Buildalyzer is NOT the default. Tier B is the win:**

- **Tier A — SyntaxPopulator (always on).** Today's pipeline over BodyFacts. Zero
  prerequisites, works on a bare clone.
- **Tier B — SemanticLitePopulator (auto-on when possible).** Build a
  `CSharpCompilation` per project **ourselves**: our own parsed trees + project-ref
  graph + framework ref-assemblies (bundle `Basic.Reference.Assemblies` or resolve from
  installed packs) + **NuGet dlls resolved by reading `obj/project.assets.json`** →
  global-packages paths. No MSBuild, no build, no SDK requirement of our own. On a
  repo the developer actually works in (assets present — the common case), this gives
  true `SemanticModel` binding for exactly the seam-critical questions: receiver types,
  `var` decls, `Adapt<T>` returns, interface closure, overload targets. Where assets are
  missing (fresh clone), Tier B degrades per-project to A and Coverage says so.
  Optional assist: offer "run `dotnet restore` to improve precision" as a UI action.
- **Tier C — MSBuild/Buildalyzer populator (explicit opt-in flag).** For exact defines,
  multi-targeting, source generators. Detects SDK; never bundled; failure degrades to B.

Tier routing is per-project, recorded in Stats (`A: 3, B: 8, C: 0 projects`).

## 7. Migration map (what dies, what moves)

| Today | Loom |
|---|---|
| `NameResolver` | `SymbolTable` (+ AmbiguityReport) |
| `GraphBuilder.AddSends/AddRaises/AddDataEdges` + 18 regex sites | `ISeamDetector` set over BodyFacts |
| `BuildAllMethodSpans` re-parse | spans from the original parse (free) |
| `StripStringLiterals`, `EstimateProvenance`, `ResolveVariable*`, `UnwrapGenericArg` | obsolete (syntax gives all of it) |
| `_eventPublishers` static | assembler-local seam-match aggregation |
| `NodeId.ForType(projectName)` ServiceLinks | Service nodes + Service→Service links |
| `MapBuilder` topology/layer aggregation | `ServiceMapProjection` + `DependsOn` edges |
| `TraceBuilder` ad-hoc walks | `graph.Walk(...).ToFlow()` + Flow store |
| `EnrichEntryTargets` route↔send name matching | Flow spine step 1 (typed), heuristics kept but tier-labeled |
| bench "content assertion" | truth expectations per repo (`eval/expectations/*.truth.json`, §L0) |

`IEntryPointBuilder` and the 11 entry builders **survive** (signature port to SymbolRef).
`CodeGraph` consumers (TraceBuilder/MapBuilder/GraphQuery/MCP/proto) keep compiling
through a compatibility pass until L4 removes the shims.

## 8. Performance budget

Dogfood ≤ 4s (today 3.9s); DntSite ≤ 20s; MassTransit ≤ 50s — Tier A. Tier B adds ≤ 2×
on first run, amortized by the existing content-keyed cache (BodyFacts and compilations
cache per file/project). The bench records tier routing so regressions are attributable.

## 9. What agents must NOT do (learned the hard way)

- Don't add a regex to fix a body-scan gap — extend BodyOps or a detector.
- Don't resolve a name by picking a list's first element — return Ambiguous.
- Don't make a project a Type node. Don't stamp `Layer="Infrastructure"` as a default.
- Don't assert a feature works because its QA script passes — drive it cold (the
  scripts in `eval/` tell you the answer's shape; a user doesn't have them).
- Don't keep evidence in prose — every claim in a handover names a fresh artifact path.
