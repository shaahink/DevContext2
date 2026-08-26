# F1 (#33) — blast-radius declaration (written BEFORE coding, E1 discipline)

Invariant being established: **no node may be a member of a type that does not declare it**
(where "declares" = declared on the type or anywhere in its **in-solution-visible** inheritance
chain — `TypeDiscovery.BaseTypes`/`ImplementedInterfaces` walked; an out-of-solution base ends
visibility, it never vouches).

## Must DROP (flip downward — these counts fall on every pole)

- Member nodes minted for extension/BCL methods on an in-solution receiver:
  `AppDbContext::ConfigureAwait`, `::Where`, `::IgnoreQueryFilters`, `::ToListAsync`,
  `SourceUploads::ConfigureAwait`, `S3ObjectStore::ConfigureAwait` and kin — and every
  Calls edge into or out of them (both producers: `AddCallEdges`, `AddHubScopeEdges`).
- CallGraphBinder receiver-arm edges whose method the resolved callee type does not
  visibly declare (post property-hop, post DI routing) — includes calls to methods
  inherited from an OUT-of-solution base (`_db.SaveChangesAsync()` → no
  `AppDbContext::SaveChangesAsync` member node; the invariant is literal).
- PlainCallDetector member→type Calls seams for the same undeclared shapes (edge-only mirror).
- Chained-call receiver guesses: `x.Y().Z()` no longer roots Z's receiver at x
  (a call result's type is not the root identifier's type). Syntactic-only chains
  lose their (wrong) edge; Tier-B repos re-bind the receiver semantically to Y's
  RETURN type and keep only in-solution, declaring targets.
- Semantic-contradicted receiver refs: when the Tier-B bind disagrees with the
  syntactic scope guess, the ref is now UNRESOLVED (was: wrong guess survived).
  Edges that only existed through the wrong guess drop.

## Must HOLD (unmodified suites stay green)

- `StaticReceiverEdgeTests` — static receiver arm already carries the declares gate; untouched.
- `OpSpanRelocationTests` — receiver/arg/creation binds where syntax and semantics AGREE.
- `LambdaArgumentEdgeTests` — lambda-param receivers; declared interface methods pass the gate.
- `BclNameCollisionEdgeTests` — the standing guard stays green AND gains INV-C siblings.
- DI-routed edges (`diImpl`/`soleImpl`): implementations declare what they implement.
- Property-hop edges (Batch C): the hop lands on the property's declared type and the
  called method is declared there.
- Inherited **in-solution** methods: `Derived.M()` where `Base` (in-solution) declares M —
  the oracle walks BaseTypes, so these edges/nodes SURVIVE (the naive-gate trap).
- Constructor member ids: ctors are recorded in `TypeDiscovery.Methods` (name = type name).
- Entry-builder member nodes for declared handler methods (HTTP actions, lifecycle
  members, gRPC overrides, SignalR hub methods, worker ExecuteAsync overrides).
- Lambda/synthetic member ids (`Type::<lambda> GET /x`) — INV-C skips non-identifier
  member names; it judges only real method names on known types.

## Must NOT WORSEN

- `GraphQueryTests` startHere noise filter (l.105-136): gains a case; existing pass.
- `GraphInvariants` diagnostic: still absent on healthy graphs (BclNameCollision triple);
  INV-C refusals are COUNTED when a producer regresses, never silent.
- Dogfood truth sweep: DevContext's own graph must contain ZERO members of types that do
  not visibly declare them, with zero INV-C refusals (no producer even tries).

## Expected eval/golden deltas (NOT edited here — left for the integrate commit, R-T7)

- Call-edge totals drop on every pole (the approx share should fall: undeclared-member
  edges were the bulk of the 900-approx class measured on Book2Course).
- `startHere` orderings change wherever an undeclared member ranked by degree.
- Any goldens/truth expectations pinning exact node/edge counts or startHere rows will
  shift; exact deltas to be measured by the integrate agent's full battery.

## Measured (dogfood, DevContext's own src — develop 04173d6 vs this branch)

- graph: 1693 → **1391 nodes** (−302), 2540 → **1832 edges** (−708); entry points **34 → 34** (held).
- CallGraphBinder: 1872 → **1424 edges** (−448); semantic 575 → 564 (11 semantic edges whose
  callee's visible hierarchy does not declare the method — correctly refused).
- GraphInvariants refusals: INV-B **5 → 5, identical keys** (pre-existing DI-lambda expression
  texts — measured on a throwaway develop worktree, NOT introduced here); INV-A 0 → 0; INV-C **0**
  (no producer even tries post-fix).
- Dogfood truth sweep: **zero** member nodes of types that do not visibly declare them.
- Arg-side note (same RootIdentifier change): an argument that is a call result is no longer
  text-rooted at its leading identifier either — `Send(_factory.Create())` no longer types the
  argument as Factory. Wrong-typed dispatch contracts drop with it.
- Entry-target rider: inline minimal-API lambdas whose only resolvable calls were
  undeclared-member seams now label "inline (N calls)" (the E6 label — its Title-based check never
  matched V1.2's "Owner.<lambda> …" titles and was dead) instead of naming the registration type.

## Addendum — integration repair (2026-08-27, fix/mcp-drive-integration)

The integrate battery (first run with the TodoApi/VerticalSlice submodules initialized) proved two
of the declared drops overshot:

- **"PlainCallDetector member→type Calls seams for the same undeclared shapes" is RETRACTED.**
  Dropping the seam alongside the member node severed TRUE connectivity: TodoApi's `POST /todos/`
  lambda reaches its store only through `db.Todos.Add(..)` / `db.SaveChangesAsync()` — DbContext
  members, all out-of-solution — and the RATCHETED truth pin (`TruthExpectationTests`,
  "TodoDbContext" in the trace) caught the loss. The repair keeps the invariant literal (no member
  node, INV-C untouched, binder still refuses) and DEGRADES the refused call to the member→TYPE
  Calls edge PlainCallDetector already emits — called name on the EDGE (`TargetMember`, the shape
  F4's port bridge reads), confidence 0.4, yielded after declared matches so a degraded call never
  steals the first-wins edge slot. `UndeclaredMemberEdgeTests` grew the degrade fixtures; the
  develop-era `Entry_target_for_lambda_with_only_noise_calls` pin ("CatalogContext") is restored.
- **Entry-builder member nodes are only "declared handler methods" if the DETECTION stamps a
  declared one.** FastEndpointsHelper stamped `HandlerMethod="HandleAsync"` by convention;
  MinimalClean's `CreateEndpoint` declares `ExecuteAsync`, the entry join minted
  `CreateEndpoint::HandleAsync`, INV-C (correctly) refused it, and `POST /Products` dead-ended with
  zero out-edges. The helper now resolves the handler from what the class DECLARES
  (ExecuteAsync/HandleAsync priority list, the ComponentLifecycleMethods pattern); the attribute
  path's class-name stamp (`Type::Type`) got the same resolver.
