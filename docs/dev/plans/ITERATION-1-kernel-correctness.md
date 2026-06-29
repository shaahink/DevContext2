# Iteration 1 — Kernel hygiene + member-origin correctness (Phases 0 & 1)

> **Status:** DONE (branch `feature/iter1-member-origin`) · **Phase(s):** 0, 1 · **Prerequisite:** none.
> **Fresh session? Start at [`README.md`](./README.md).**
>
> **Progress (2026-06-28):**
> - **Phase 0 DONE** (commit `2c12f95`). Key finding: the budget was already out of the kernel — the CodeGraph + Map/Trace
>   are built at `DiscoveryPipeline` line ~135, BEFORE the pruners run (~141), from `NoiseFilter`-filtered
>   types; they never read `model.Budget`/`IsPruned`. The pruners only drive the legacy catalog (JSON/HTML).
>   Action taken: documented the invariant in `DiscoveryPipeline` (GraphAssembly + RunScoring comments),
>   added `tests/.../BudgetIndependenceTests.cs` (Map+Trace byte-identical across `--max-tokens` 2000 vs
>   20000 — PASSES), and extracted a shared `tests/.../TestPipeline.cs`. Did **not** delete the legacy
>   pruners (JSON/HTML still size with them; retire with the catalog path later).
> - **Phase 1 DONE** (commit `4f457a3`). Root cause (CallEdge→Type→Type fold + Member→inherit-parent-Type in
>   OutEdgesWithTwin + EntryPointResolver dropping :Method) eliminated. Four changes in the kernel:
>   `AddCallEdges` → Member→Member; `AddRaises`/`AddSends`/`AddDataEdges` → per-method attribution via
>   Roslyn span locator; `OutEdgesWithTwin` → controlled bridge (Type→handler-entry-member only);
>   `EntryPointResolver` → Type:Method anchors on Member node. Divergence gate: CreateItem ≠ UpdateItem;
>   orders trace shows no sibling sends/raises; spine (send→handler→raises→outbox) intact via the
>   handler-`Handle` bridge; `gates.ps1` PASS (12 eval tests incl. 2 new negative/divergence guards).
>
> Agent-executable iteration. Do Phase 0 then Phase 1, back to back. End on the gate.
> **Required reading first:** `docs/product/PRODUCT-DIRECTION.md`, `docs/dev/plans/UNIVERSAL-LENS-ROADMAP.md`
> (Phases 0–1), `docs/product/ACCEPTANCE.md` (you will ADD checks here — you are authorized to improve the
> harness: gates, expectations, tests). The bug below is confirmed live; the design is decided.
>
> **Mental model.** The kernel is `CodeGraph` + builders. The product is Map/Trace rendered from it.
> Token budgeting and the legacy weighted scorer are *catalog-era* machinery that distort the graph;
> Phase 0 gets them out of the way. Phase 1 fixes the one bug that makes traces fabricate edges.
> Build/test per-project or via `DevContext.slnx`; analyzer warnings are errors (CA1822: builder
> methods without instance state must be `static`).

---

## Phase 0 — Kernel hygiene

**Goal.** Make structural bounds (depth · fan-out · framework-stop) the *sole* governor of trace/Map
size. Stop token budgeting and dead scoring from touching graph building / pruning. (Audit High-4/7,
Medium-12.)

### 0.1 Get token budgeting out of the kernel path
- Pruners are registered in **two** places: `src/DevContext.Cli/Services/ServiceRegistration.cs:28-29`
  and `tests/DevContext.Core.Tests/TraceQualityTests.cs` `BuildPipeline` (and likely
  `DevContext.Desktop` DI — grep `new TokenBudgetEnforcer`). 
- The narrative Map/Trace render path must **not** run `TokenBudgetEnforcer`. Decide the cleanest of:
  (a) remove it from the pruner list feeding the narrative path, or (b) make `TokenBudgetEnforcer`
  a no-op when the snapshot has a `Graph` (narrative mode). Prefer (a) — fewer hidden conditionals.
- Verify `TokenBudgetEnforcer` no longer mutates `model.Types`/pruning for Map/Trace. The legacy
  JSON/HTML *catalog* renderers may still size-cap; if so, scope the budget to **only** those
  renderers, never the graph. (Per `PRODUCT-DIRECTION.md` §8: if budgeting returns it is a pure
  render-time post-filter, never in graph assembly/pruning.)
- Confirm `MaxCallDepth` / `MaxFanOut` (in `Scenario.cs` / `TraceOptions`) remain the trace bound.

### 0.2 Handle PatternRelevancePruner carefully — it is NOT purely dead
- Audit says "0% *pruning*", but it still sets `RoleScore`/`FinalScore`
  (`RenderPlanBuilder.cs:34` notes "FinalScore is now just RoleScore from PatternRelevancePruner")
  and excludes test-project types + library-mode boosting. **Do not blind-delete.**
- The Trace traversal *is* the selection (no global scoring) — so the narrative path doesn't need
  FinalScore. Determine whether the **legacy JSON/HTML catalog renderers** still consume FinalScore.
  - If nothing on the surviving product surface consumes it → remove `PatternRelevancePruner` and the
    FinalScore plumbing (a real simplification).
  - If the legacy renderers still need it → leave it for a later "retire legacy catalog path" phase;
    just ensure it doesn't run on the narrative path. Confirm test-type exclusion is already covered by
    `Graph/NoiseFilter.cs` (it is, structurally) before removing PatternRelevancePruner's test-exclusion.
- Record the decision in the PR description.

### 0.3 Harness updates (you are authorized)
- Remove/relax token-budget assertions that no longer apply. Search `eval/expectations/*.json` and
  `gates.ps1` step 4 for `--max-tokens` / "budget" checks. The CLI `--max-tokens 2000 --strict` matrix
  row should still exit cleanly (flag is accepted, just doesn't drive graph pruning).
- Add to `docs/product/ACCEPTANCE.md` Phase 0 row: output bounded by depth/fan-out (no "over budget" string).

### Phase 0 gate
- `eval/gates.ps1` PASS (build, fast tests, eval, CLI matrix).
- DntSite + OrchardCore Map/Trace: output bounded by depth/fan-out, no "over budget" / budget-driven
  truncation. No behavior change other than budget no longer cutting the graph.

---

## Phase 1 — Member-origin edge correctness (the decisive fix)

**Goal.** A trace anchored on a method shows **only that method's** wiring. Two sibling methods of one
class produce different traces. Kills fabricated edges, wrong EMITS/TOUCHES, `[verified]` overclaiming —
and (proven by the probe) recovers *missed* paths. (Audit Critical-1, Medium-8.)

### Root cause (verified in code + live)
`CallEdge` carries `CallerMethod`/`CalleeMethod` (`CallGraphExtractor.cs:188`), but the graph throws
the method away:
- `GraphBuilder.AddCallEdges` (~694) folds edges to `Type→Type` (drops `CallerMethod`).
- `GraphBuilder.AddRaises` (~783), `AddSends` (~874), `AddDataEdges` (~721) regex-scan the **whole**
  `type.SourceBody` and anchor every match on the **Type** node.
- `TraceBuilder.OutEdgesWithTwin` (~245) then makes any **Member** node inherit **all** the Type's
  edges. So `Member:CatalogApi.CreateItem` inherits `UpdateItem`/`DeleteItemById` edges.

Live proof: `--focus "CatalogApi:CreateItem"` and `:UpdateItem` produce byte-identical traces headed
`▸ ENTRY CatalogApi` (the *type*; the `:Method` is ignored).

### The target model (member-origin edges)
- **Identity stays one-node-per-class** (Map/merge unaffected): `Type` node per class, role tags.
- **Edges that originate in a method body originate from a `Member` node:**
  - `Calls`: `Member:Caller.Method → Member:Callee.Method` (both already known from `CallEdge`).
  - `Raises` / `Sends` / `ReadsWrites(data)`: `Member:Owner.Method → Type:Event/Request/Entity`
    (origin = the enclosing method; target stays the type).
- **Join edges stay type-level** (they are inherently type relationships): `Handles` (request→handler),
  `Consumes` (event→handler), `Resolves` (iface→impl), `WrappedBy` (request→behavior).
- **Member→owning-Type containment** is kept (for Map + "what type is this method in").
- **Controlled member bridge** replaces "inherit all type edges": arriving at a *handler Type* via a
  `Handles`/`Consumes` edge expands only its `Handle`/`HandleAsync` member; arriving at a callee via
  `Calls` already lands on the specific `Member`.

### Implementation notes (finalized 2026-06-28 against the real code — read before coding)
- **`AddCallEdges`: caller AND callee become `Member` nodes** (`ce.CallerMethod`/`ce.CalleeMethod`), each
  created carrying its owning Type node's `FilePath` (look up `g.GetNode(NodeId.ForType(fqn))?.FilePath`).
  Keep the in-scope gate (both types must have a FilePath). Member↔Member = precise descent, so **no
  bridge is needed for `Calls`** — the spine (handler.Handle → call → OIES.AddAndSaveEventAsync → … →
  RabbitMQEventBus) descends method-to-method. (If callee were a Type, the trace could NOT descend into
  the specific called method's body and the outbox/bus spine would be lost.)
- **The bridge (`TraceBuilder.OutEdgesWithTwin`) is only for JOIN seams.** Member node → its own edges
  ONLY (delete the parent-Type inheritance — that's the bug). Type node → its own join edges PLUS, if a
  **handler-entry member** exists (`Handle`/`HandleAsync`/`Consume`/`ConsumeAsync`/`Execute*`/`Invoke*`),
  that member's edges. Needed because `Handles`/`Consumes` land on `Type:Handler`; bridge to its `Handle`.
- **Body-scan attribution:** precompute, once in `Build()`, a per-type offset→method locator by parsing
  `type.SourceBody` with Roslyn (method/ctor spans relative to SourceBody; ctor name = type short name,
  matching `CallGraphExtractor`). Pass it to `AddRaises`/`AddSends`/`AddDataEdges`; emit from
  `Member:Type.Method`; fall back to the Type node only when a match is outside any method (rare).
- **SCOPE LINE between Phase 1 and Phase 3 (important):** `new Order(...)` is an *ObjectCreation*, NOT an
  invocation, so it is not a `Calls` edge — the trace reaches an aggregate via a `data` edge to
  `Type:Order`. After member-origin, `Order`'s domain-event raises move onto its members, so the
  type-level `data Order` **stops dumping all six events** — which correctly *removes* the CreateOrder
  fabrications (OrderShipped/OrderCancelled). Reaching the *correct* `OrderStartedDomainEvent` (raised in
  the ctor) and following it to its handler is **Iteration 3**, not here. So Phase 1 makes the orders
  trace **leaner and correct** (fabrications gone); the decisive gate (`CreateItem`≠`UpdateItem`) does
  NOT need the ctor path — `CatalogApi` methods do inline `new …IntegrationEvent(...)` that `AddRaises`
  attributes to the enclosing method.

### Step order (each step builds + tests green before the next)

**1. Member nodes carry bodies.** Ensure a `Member` node exists for any method that originates an
edge, carrying `FilePath` and (where available) its own body span. The cheapest source of per-method
bodies is `CallGraphExtractor`'s per-`member` Roslyn loop — it already iterates each method's syntax.
Consider emitting, alongside each `CallEdge`, the caller method's span so GraphBuilder can build member
nodes with bodies. (If that's too big a change for step 1, build member nodes from `CallEdge` caller
identity with the type's file, body filled in step 3.)

**2. `AddCallEdges` → member origin.** Change `callerId = NodeId.ForType(...)` to
`NodeId.ForMember(callerFqn, ce.CallerMethod)`; ensure the member node exists (create with the
declaring type's file). Keep the callee as `NodeId.ForMember(calleeFqn, ce.CalleeMethod)` when the
method is known, else `Type`. Add a Member→Type containment edge if you introduce one, or rely on the
`ExtractTypeKey` convention already used by the traversal. Keep the in-scope gate (both endpoints must
be declared types with a FilePath).

**3. Body-scan seams → enclosing method.** `AddRaises`/`AddSends`/`AddDataEdges` must attribute each
regex match to the **method that contains the match offset**, not the whole type. Two options:
  - *Preferred (unification):* move these scans into `CallGraphExtractor`'s per-member loop, so each
    match is naturally owned by `member`. This kills the duplicated whole-type body scan.
  - *Local:* parse `type.SourceBody` once (Roslyn), map each method's text span, and resolve match
    offset → enclosing method; emit the edge from `Member:Type.Method`.
  Emit `Member → Type:Event/Request/Entity`. Preserve provenance (`EstimateProvenance`).

**4. Controlled member bridge in `TraceBuilder`.** Replace `OutEdgesWithTwin`'s "yield all Type
out-edges for a Member" with:
  - For a `Member` node: yield the member's own out-edges (now the correct per-method set).
  - When traversal arrives at a `Type` node via a `Handles`/`Consumes` edge (a handler/consumer), bridge
    to its `Handle`/`HandleAsync` member node and continue from there — **not** the whole type.
  - Remove the blanket Member→Type inheritance that caused the bug.
  Keep the salient-source fallback (a member's salient lines come from its own body now).

**5. Entry / focus anchoring.** `Type:Method` focus must anchor on the **Member** node.
  - `CallGraphExtractor.GetStartKeys` and `Resolvers/FocusPointParser.cs`: resolve a `Type:Method`
    focus to `Member:Type.Method`.
  - `Graph/EntryPointResolver.cs` and `GraphBuilder.AddHttpEntryPoints`: HTTP entries already create a
    Member node for a specific handler method (good) — verify they link to the **member**, and that
    `ResolveEntryTarget` follows from the member's own dispatch (it mostly does; remove the
    `ResolveViaParentType` whole-type fallback where it now drowns precision).
  - Trace header must show the method (`▸ ENTRY CatalogApi.CreateItem` / the route), not the bare type.

**6. `[verified]` semantics.** A `Calls` edge is `Semantic`/`[verified]` only when resolved within the
parent member's body. Since edges are now member-scoped, this is automatic — just confirm the renderer
label still maps `Resolution.Semantic → [verified]`.

### Harness updates (authorized — this is the acceptance backbone)
Add the **negative/divergence** guards that don't exist yet:
- `TraceQualityTests`: add a `[Fact]` asserting `CatalogApi:CreateItem` trace **differs** from
  `:UpdateItem` (and each contains its own salient line, not the other's). Add `Assert.DoesNotContain`
  for the known fabrications (CreateItem must not contain `ProductPriceChangedIntegrationEvent`).
- `eval/expectations/eshop.json`: add `output-not-contains` for `CancelOrderCommand` /
  `ShipOrderCommand` on the `POST /api/orders` focus run *if* you wire a trace-focus expectation
  variant; otherwise keep the negative trace guards in `TraceQualityTests` (it runs real focused traces).
- Flip the relevant `docs/product/ACCEPTANCE.md` Phase 1 checks to `expected`.

### Phase 1 gate (decisive)
Run these exact commands (Release CLI) and diff:
```
dotnet <cli> analyze eval-repos/eShop/src/Catalog.API --focus "CatalogApi:CreateItem" --depth 4
dotnet <cli> analyze eval-repos/eShop/src/Catalog.API --focus "CatalogApi:UpdateItem" --depth 4
dotnet <cli> analyze eval-repos/eShop/src/Ordering.API --focus "POST /api/orders/" --depth 6
```
- CreateItem ≠ UpdateItem; each header names the method; each shows only its own body's edges.
- `POST /api/orders`: no `send CancelOrderCommand`/`ShipOrderCommand` under `CreateOrderAsync`; `Order`
  ctor path does not "raise" Shipped/Cancelled. The CreateOrder spine (send→handler→raises
  OrderStartedIntegrationEvent→outbox→bus) remains intact.
- `eval/gates.ps1` PASS incl. the new negative `TraceQualityTests`. Ratchet eShop/VerticalSlice JSON
  expectations as needed.

### Pitfalls
- **CA1822**: new `GraphBuilder` helpers without instance state must be `static`.
- **NameResolver**: keep using `names.Resolve(...)` for member-owner FQNs so keys stay canonical.
- **Dedup**: edges dedup by `(From, To, Kind)` — member-origin increases distinct `From`s; verify no
  legitimate edges get dropped and no phantom member nodes (callee with unknown method) flood the graph.
- **Don't regress the good eShop spine** — the genuine send→handler→event→bus path must survive; only
  the *sibling* edges should disappear. Use the depth-6 orders trace as the canary.
- **Member→Type for Map**: the Map/stats count types, not members — confirm member nodes don't inflate
  type counts or stats (`GraphStats`). Filter members where the surface counts types.

---

## Definition of done (Iteration 1)
- Phase 0 + Phase 1 gates pass.
- The decisive live test diverges correctly.
- Harness carries the new negative/divergence guards (the fabrication class can no longer pass green).
- PR description records the PatternRelevancePruner decision and any harness changes.
- Next iteration: Phase 2 (universal entries, controllers first) — see roadmap.
