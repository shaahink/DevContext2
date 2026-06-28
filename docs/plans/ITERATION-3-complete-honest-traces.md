# Iteration 3 — Complete & honest traces (Phase 3)

> **Status:** NOT STARTED · **Phase(s):** 3 · **Prerequisite:** Iteration 2 DONE
> (controller entries resolve targets; sibling actions diverge; gate green).
> **Fresh session? Start at [`README.md`](./README.md).** Required reading:
> `docs/PRODUCT-DIRECTION.md`, `docs/plans/UNIVERSAL-LENS-ROADMAP.md` (Phase 3), `docs/ACCEPTANCE.md`,
> and `docs/IDEAL-OUTPUT-TARGET.md` §2 (the shape a great trace should approach).
>
> **Why this builds on Iterations 1–2.** Member-origin edges (Iter 1) mean an aggregate's ctor now
> raises only *its* event (not all six), so the right domain-event path is no longer drowned. This
> iteration makes the trace *follow* that path to its handler, count entities reached via calls, show
> cross-cutting once, and be honest about what it cut — turning the trace from a "primer" into an
> "accelerator" (the probe metric).

**Goal.** The trace surfaces the whole relevant path and is honest about cuts. (Audit High-5, Medium-11,
Low-15; closes the probe's "missed the real domain-event path" finding.)

---

## Step 0 — Reproduce (measure before changing)
- `dotnet <cli> analyze eval-repos/eShop/src/Ordering.API --focus "POST /api/orders/" --depth 6`.
  Post-Iteration-1 the `Order` ctor should raise only `OrderStartedDomainEvent`. Check whether the trace
  now follows it to `ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler`. If not, diagnose
  the break (Raises target node identity? Consumes not followed? handler bridge?).
- `dotnet <cli> analyze <controller-repo> --focus "GET /Feed" --depth 5`. Confirm `TOUCHES` is sparse
  (only `BaseEntity`) — the High-5 gap.

## Step 1 — TOUCHES via `Calls`, not only `ReadsWrites` (High-5)
EF access is usually a `Calls`/data edge on a member body, not a `ReadsWrites` edge. Extend
`TraceBuilder.CollectGraphEntities`:
- For every **visited member**, collect entity/aggregate nodes reached via its `Calls` **and** `data`
  edges, plus entities named in its body (the member-origin `AddDataEdges` from Iter 1 already emits
  `ReadsWrites` from members — ensure those members are visited and scanned here).
- Keep the existing `ReadsWrites` sweep. De-dup by title; keep `IsNoiseEntity` filtering.
- Canary: controller `GET /Feed` `TOUCHES` lists the real entities `FeedsService` touches, not just
  `BaseEntity`.

## Step 2 — Follow the `Raises → Consumes → handler` chain
The domain-event path the probe agent had to find by hand must render:
- Confirm `AddRaises` creates the event **Type** node with the **same `NodeId`** (`names.Resolve`) that
  `AddEventConsumers` uses, so the `Consumes` edge (event→handler) actually connects. If they differ
  (short vs FQN), canonicalize so the chain links.
- `EdgeKind.Consumes` is already in `TraceOptions.Follow`. Verify the walk goes: member `raises` →
  `Type:OrderStartedDomainEvent` → `Consumes` → `Type:Handler` → (Iter-1 controlled bridge) →
  `Handle` member → its body. Fix whichever hop drops.
- Canary: `POST /api/orders` shows `… raises OrderStartedDomainEvent → ValidateOrAddBuyer…Handler`.

## Step 3 — Show cross-cutting (pipeline) once, not per row (IDEAL-OUTPUT-TARGET §2)
`WrappedBy` edges exist (`AddPipelineBehaviors`) but `TraceOptions.Follow` excludes them. Don't follow
them as ordinary fan-out (that explodes every request node). Instead **render the pipeline once** under
the first `Send` that reaches a request: a `pipeline ▸ Logging → Validation → Transaction` annotation
sourced from the request's `WrappedBy` edges. This matches the ideal output and surfaces
`TransactionBehavior` (the after-commit publish trigger the probe wanted) without fan-out blow-up.

## Step 4 — Honest truncation (Medium-11)
- `TraceBuilder` already sets `Truncated`. Render it explicitly: `stopped at depth N; K branches omitted`
  with the real omitted count (`ranked.Count - taken.Count`).
- Keep the fan-out cap but confirm `EdgePriority` surfaces seam edges (Sends/Raises/Consumes/data) above
  framework `Calls`, so what survives the cap is the meaningful wiring. Consider a modest cap bump now
  that edges are member-scoped (less noise per node).

## Step 5 — Run `IndirectWiringDetector` regardless of mode (Low-15)
Find where it is gated to Trace mode and ungate it so Map runs benefit too. Verify no perf regression
(it's cheap); if it's not, defer to Phase 6 and note it.

## Step 6 — (note, not in scope) Semantic Sends/Raises
Sends/Raises stay `[approx]` (body-scan). The semantic tier (Roslyn `SemanticModel`, the old "P3")
upgrades these — schedule as a later iteration after Phase 5. Do **not** start it here.

## Harness updates (authorized)
- `TraceQualityTests`: `POST /api/orders` must contain `OrderStartedDomainEvent` **and** the buyer
  handler; controller `GET /Feed` `TOUCHES` count > 1; truncation marker present when cut; pipeline shown
  once for a CQRS entry.
- Flip `docs/ACCEPTANCE.md` Phase-3 checks to `expected`.
- **Re-probe (capability gate).** Rerun the before/after probe (`IDEAL-OUTPUT-TARGET.md §7`): conditions
  C (repo+tools) vs D (repo+tools+trace) on the **fixed** trace, plus a controller repo and a 2nd task
  (e.g. "add a per-line discount to orders"). Record in `docs/reports/probe-phase3.md`. **Target:** the
  fixed trace reduces a tool-using agent's cost (moves from "primer" toward "accelerator") — the metric
  the Iteration-0 probe set at "primer, not accelerator yet."

## Gate (Phase 3 done when)
- `POST /api/orders` renders the domain-event→handler path and the pipeline (once); controller `TOUCHES`
  is non-trivial; truncation is explicit.
- `eval/gates.ps1` PASS incl. new trace checks.
- Re-probe recorded; trace cost-vs-baseline improved over the Iteration-0 result.

## Pitfalls
- Don't re-drown the path: the `Raises` must be member-origin (Iter 1) so only the relevant event shows.
- Pipeline as **annotation**, not followed fan-out — following `WrappedBy` per request explodes the tree.
- Event node identity mismatch (short vs FQN) is the most likely reason the `Consumes` chain doesn't link
  — fix at the `names.Resolve` boundary.
- Re-run Iteration 1–2 gates; completeness changes must not reintroduce sibling fabrications.

## Definition of done → next
Gate green; re-probe shows improvement; update Status above and the table in `README.md`; record the
commit. **Next: Iteration 4 (Phase 4 — honest Map & detection)** — guide to be written from the roadmap.
