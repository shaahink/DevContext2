# Plan: G3 — library archetype · G5 — minimal-API per-endpoint precision

> Status: **draft.** Two render-fidelity features, independent of each other and of G1. Each can be
> taken on its own. Assessment: `docs/reports/OUTPUT-QUALITY-ASSESSMENT.md` G3, G5.
> North star: `docs/IDEAL-OUTPUT-TARGET.md` §4 (library surface), §2 (trace precision).

---

## G3 — library archetype (the AutoMapper case) · High · sizable

### Problem
A packable library with **no application entry points** (AutoMapper) renders as `STYLE NLayer` with
`0 entries` and no public surface — useless. The Map assumes an app: it leads with entry points, which
a library doesn't have. The design (§4/§5.5) wants a **capability-grouped public surface map** instead.

### Goal
Detect the **Library** archetype and, for it, render a **PUBLIC SURFACE** narrative (grouped public
API) in place of the entry-point inventory — section-aware, so the desktop drawer works.

### Design
1. **Archetype detection** — a small `ArchetypeDetector` (or extend `ArchitectureStyleDetector`):
   `Library` when (a) zero app entry points (no HTTP/bus/hosted/CLI entries in the graph) **and**
   (b) packable — any non-test project with `<IsPackable>true</IsPackable>`,
   `<GeneratePackageOnBuild>`, or `OutputType` library and no `Exe` in the scope. `App` otherwise.
   (Keep the existing *style* — NLayer/Clean/etc. — orthogonal; archetype decides which renderer.)
2. **`LibrarySurface` builder** — over the public types/methods of the main (non-test) project(s),
   grouped by capability. Start with namespace/type grouping; optionally bucket by verb-ish role
   (Configure / Execute / Extend / Register) using simple name/signature heuristics. Reuse
   `model.Types` (public accessibility) + DI-registration detections for the "Register" bucket.
3. **Surface renderer** — emit `NarrativeSection` fragments (`Overview`, `Public surface`,
   `Extension points`, `Packages`, `Footer`) via the existing `NarrativeSections` pattern so it is
   byte-stable and section-aware like Map/Trace.
4. **Wire-in** — in `DiscoveryPipeline.RenderAsync` narrative branch: archetype == Library →
   `LibrarySurfaceRenderer` instead of `MapRenderer`. `MapBuilder`/`MapModel` may gain a
   `Surface` field, or a parallel `LibraryModel` on the snapshot.

### Phases
0. Archetype detection + a test (AutoMapper fixture → Library; eShop/TodoApi → App).
1. `LibrarySurface` builder (namespace/type grouping first; capability buckets second).
2. Section-aware renderer + pipeline wire-in.
3. Goldens/eval: add an AutoMapper-shaped fixture; ratchet expectations.

### Verification
`analyze AutoMapper` → no entry-point section; a PUBLIC SURFACE grouping the real API
(`Mapper.Map`, `IMapperConfigurationExpression`, `Profile`, `AddAutoMapper` DI extension), test
projects excluded, packages intact.

### Files
new `Graph/ArchetypeDetector.cs`, `Graph/LibrarySurfaceBuilder.cs`, `Rendering/LibrarySurfaceRenderer.cs`;
`Pipeline/DiscoveryPipeline.cs` (branch), `Pipeline/AnalysisSnapshot.cs` (carry surface/archetype),
`tests/fixtures/<library-fixture>`, goldens.

---

## G5 — minimal-API per-endpoint precision · Medium · hard

### Problem
All minimal-API endpoints registered in one method (`app.MapGet(...)`, `app.MapPost(...)`, …) share a
single owner **Type** node in the graph. So a trace from `POST /todos/` shows salient body lines and
`→ target` from the *registration method* (e.g. the `MapGet("/{id}", …)` lines), not the chosen
route's lambda. The entry exists; its body/edges don't resolve to the specific endpoint.

### Goal
Per-endpoint anchoring: each `Map{Verb}(route, lambda)` becomes its **own** graph node carrying that
lambda's body and its own out-edges, so the trace from a route shows that route's handler and its real
send/call/data targets.

### Design
1. In the endpoint extractor / graph builder, create a distinct node per `Map{Verb}` invocation, keyed
   by `verb + route + lambda location` (member/lambda-level), instead of attaching everything to the
   registration Type node.
2. Attach the lambda's source span as the node's `SourceBody` (drives salient extraction) and resolve
   the lambda body's call/send/`ReadsWrites` edges onto that node (not the owner type).
3. Point `EntryPoint.Node` at the per-endpoint node; the trace walks from it.

### Risks
- Lambda receiver/closure resolution (captured variables, helper methods) — partial is acceptable.
- Inline vs delegate-reference handlers (`MapGet("/x", Handler.Method)`) — resolve to the referenced
  method node.
- FastEndpoints `<dynamic>` routes are a **separate** gap (Configure()-set routes) — out of scope here.
- Eval/golden churn for TodoApi and other minimal-API fixtures.

### Phases
0. Reproduce on TodoApi: confirm `POST /todos/` shows the wrong (MapGet) body.
1. Per-endpoint node creation + body attach in the extractor/graph builder.
2. Edge resolution from lambda body → per-endpoint node.
3. Goldens/eval ratchet.

### Verification
`analyze TodoApi --focus "POST /todos/"` → trace body lines and `→ target` come from the POST lambda,
not `MapGet`.

### Files
`src/DevContext.Core/Extractors/**/Endpoint*` (minimal-API registration handling),
`src/DevContext.Core/Graph/GraphBuilder.cs`, `Graph/EntryPoint.cs`, `tests/` + goldens.

---

## Sequencing
Independent. G3 is the higher-value (a whole archetype is "useless today"); G5 is polish for an
already-working case. Recommend G3 first. Neither needs G1, though both benefit once G1 widens scope.
