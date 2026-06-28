# Iteration 2 — Universal entries, controllers first (Phase 2)

> **Status:** NOT STARTED · **Phase(s):** 2 · **Prerequisite:** Iteration 1 DONE
> (member-origin edges; `CatalogApi:CreateItem` trace ≠ `:UpdateItem`; gate green).
> **Fresh session? Start at [`README.md`](./README.md).** Required reading:
> `docs/PRODUCT-DIRECTION.md`, `docs/plans/UNIVERSAL-LENS-ROADMAP.md` (Phase 2), `docs/ACCEPTANCE.md`.
>
> **Why this builds on Iteration 1.** Controllers are multi-action classes — only *after* member-origin
> edges does a controller action have its *own* call edges. This iteration uses that to resolve each
> action's dispatch target and trace, without shipping the fabrication bug to the most common .NET shape.

**Goal.** Every common entry resolves a **`→ target`**. Controllers go from **0/94** (audit Claim 6) to
mostly resolved. Drop infra-noise entries; dedup. (Audit Critical-2, Low-17, Low-18.)

---

## Step 0 — Reproduce (always start here; line numbers may have drifted)
- Run a Map on a controller repo and confirm `0/N → target`:
  `dotnet <cli> analyze <DntSite-or-controller-repo> --stats` → look at the `N/M →target` line.
- Inspect how controller endpoints are represented: are they `EndpointDetection` (from
  `EndpointExtractor`) or a separate detection from `ControllerActionExtractor`? Grep both extractors.
- Confirm the **root cause**: `GraphBuilder.ResolveEntryTarget` (~line 70) follows the entry's `Calls`
  edge to the handler node, then only inspects that node's **`Sends`** edges. Controllers (no MediatR)
  have no `Sends` → `null` target. That single assumption is the 0/94.

## Step 1 — Generalize "entry target" beyond MediatR dispatch
In `GraphBuilder.ResolveEntryTarget` (and its `Member`/`Type` branches), after the existing `Sends`
check, add a fallback priority:
1. **`Sends` target** (CQRS command/query) — existing behavior, keep first.
2. **Primary service call** — from the action **member's** `Calls` edges (now correct, post-Iter-1),
   pick the most significant callee: prefer a callee `Type` tagged `service` (DI-resolved), else the
   first in-scope, non-framework `Member`/`Type` callee. Return its title (and method when a Member).
3. **`null`** otherwise (honest — don't guess).
Keep `MatchRouteToSend` for the multi-dispatch case. Remove/΃retire `ResolveViaParentType`'s whole-type
fallback where member-origin now makes it drown precision (it was a class-scoped crutch).

## Step 2 — Ensure controller actions are member-anchored entries
- Verify `AddHttpEntryPoints` handles controller `EndpointDetection` (HandlerType = controller class,
  HandlerMethod = action) through the `!isLambdaHandler` + `hasSpecificMethod` path → it creates
  `NodeId.ForMember(handlerFqn, methodName)` with `ep.HandlerBody` and a `Calls` edge entry→member. Good.
- If controllers instead arrive via `ControllerActionExtractor` as a different detection shape, make
  them produce the same **member-anchored** entry (member node + body + entry→member `Calls` edge), so
  Step 1's call-edge target resolution works uniformly. Unify on one entry-creation path.

## Step 3 — Filter infrastructure pseudo-entries (Low-17)
- Drop entries that are framework/infra, not application surface: `GET /` registered by Scalar/OpenAPI,
  `ServiceDefaults`, health/metrics endpoints. Implement as a predicate in `AddHttpEntryPoints` (or a
  post-filter on the entry list): skip when the source file is ServiceDefaults / an OpenAPI/Scalar
  registration, or the route is the infra root. **Guard:** don't drop a *real* app root route — match on
  source/registration, not just `"/"`.

## Step 4 — Dedup entries (Low-18)
- Collapse exact duplicates (same verb + route + file + line). **Keep** genuine versioned overloads
  (different file/line, or an explicit `Asp.Versioning` version) as distinct entries, labelled with
  their version. `GET /api/catalog/items` at `:21` and `:26` are likely v1/v2 — keep both, labelled.

## Step 5 — Verify the controller *trace* (not just the target)
- A controller trace must descend from the action member into the services it calls (member-origin
  edges), and **two sibling actions must diverge**. Canary: `GET /Feed` on DntSite should descend into
  `FeedsService` → … and differ from another `FeedController` action.

## Harness updates (authorized)
- **Controller repo in CI.** None of the always-present `eval-repos/` is a plain-controller app (TodoApi
  = minimal, VerticalSlice = FastEndpoints, eShop = CQRS). Either (a) add DntSite to `eval-repos/` (pin a
  commit, like the others) or (b) add a small controller fixture under `tests/fixtures/`. Required so the
  controller acceptance actually runs — note it in `docs/ACCEPTANCE.md`.
- Add `eval/expectations/<controller-repo>.json`: entry→target coverage `json-range` min > 0 (ratchet
  from 0), `output-not-contains` the infra `GET /`, endpoint `detection-count`.
- `TraceQualityTests`: add a controller archetype `[InlineData]` row (entry → expected service substring)
  + a sibling-action **divergence** `[Fact]`.
- Flip `docs/ACCEPTANCE.md` Phase-2 checks to `expected`.

## Gate (Phase 2 done when)
- Controller repo: **majority of entries show `→ target`** (coverage ratchets up from 0).
- No infra `GET /` entry; versioned overloads kept distinct.
- Two sibling controller actions produce **different** traces.
- `eval/gates.ps1` PASS incl. the new controller checks.

## Pitfalls
- Step-1 "primary call" can latch onto a logger/framework call — exclude framework leaves
  (`IsFrameworkLeaf`) and prefer DI-`service` callees.
- Don't treat versioned overloads as duplicates (Step 4 vs Step 3 tension).
- Attribute routing / base-controller actions / `[ApiController]` conventions vary — test on a real
  controller repo, not a toy.
- Keep CQRS entry→target (eShop 3/20+) working — Step 1 must try `Sends` *first*.

## Definition of done → next
Gate green; harness carries controller coverage + divergence guards; update Status above and the table in
`README.md`; record the commit. **Next: [Iteration 3](./ITERATION-3-complete-honest-traces.md).**
