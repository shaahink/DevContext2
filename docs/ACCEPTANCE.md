# Acceptance — what "good output" means, and how we measure it as we progress

> The single reference for *acceptable output per artifact*, and the bar at each roadmap phase.
> It does **not** invent a new harness — it is operationalized by what already exists:
> `eval/expectations/*.json` (ratchet), `tests/.../TraceQualityTests.cs` (trace tier), and
> `eval/gates.ps1`. Companion to `PRODUCT-DIRECTION.md` (the product) and
> `plans/UNIVERSAL-LENS-ROADMAP.md` (the phases). `IDEAL-OUTPUT-TARGET.md` is the hand-built
> reference for the *shape* of a great Trace/Map.

## How acceptance works here

- **Two tiers.** `eval/expectations/*.json` validate the *detection substrate* on a Map run
  (counts, signals, arch-style, output substrings). `TraceQualityTests` validate the *Trace*.
  Both run under `Category=Eval` in `gates.ps1` step 3.
- **Ratchet.** Each check is `expected` (blocking) or `aspirational` (known defect, non-blocking).
  **The phase gates below are expressed as ratchet moves**: a phase is done when the listed checks
  are flipped aspirational→expected (or newly added as `expected`) *and pass*.
- **Negative guards are first-class.** The current trace tier only asserts a trace *contains* the
  right wiring. As of the universal-lens work it must also assert it *excludes* the wrong wiring
  (the fabrication class of bug). Use `output-not-contains` checks + negative `TraceQualityTests`.
- **`docs/examples/*` are "current", not "golden".** They are regenerated snapshots, some still
  carrying known bugs. Do not treat them as the bar — this doc + the eval suite are the bar.

## Acceptable output, per artifact

### 1. Entry points
- Every entry has: a **kind** (HTTP / Bus / Domain-event / Worker / gRPC / SignalR / Blazor / …),
  a `file:line` **provenance**, and a stable id.
- Common shapes (controllers, minimal API, MediatR) resolve a **`→ target`** (the action method,
  command, or handler). Output states coverage honestly: `N/M → target`.
- **No infrastructure noise** as entries (`GET /` from Scalar/OpenAPI, ServiceDefaults).
- True duplicates collapsed; genuine versioned overloads (v1/v2) kept and labelled.
- *Checked by:* `detection-count` (EndpointDetection), `output-not-contains` (infra routes),
  and a new entry→target coverage check.

### 2. Topology / Map
- **Scope-stamped:** says whether it analyzed the whole solution or a project closure
  (`STYLE … (5-project closure)`); never pronounces system-level style from a single-service closure.
- Architecture style honest (primary + confidence + evidence); no whole-system style from a slice.
- **No MSBuild-variable noise** in STACK (`$(CommonTargetFrameworks)`).
- *Checked by:* `json-equals` (arch.style), `output-not-contains` (`$(`), scope-stamp substring.

### 3. Trace (the centerpiece)
- **Correct (no fabrication).** A trace anchored on a method shows **only that method's** wiring.
  Two sibling methods of the same class produce **different** traces. No edge from a sibling method
  appears. `[verified]` means "invoked from the parent member's body," nothing weaker.
- **Complete.** Follows the indirection: `call · send · handler · raises · consumes · data · di ·
  pipeline`. TOUCHES includes entities reached via `Calls` (EF access), not just `ReadsWrites`.
  Domain-event handlers and cross-cutting (pipeline) seams appear when present.
- **Honest about cuts.** Truncation is explicit (`stopped at depth N; K branches omitted`), not silent.
- **Bounded structurally** (depth · fan-out · framework-stop) — *not* by a token budget.
- *Checked by:* `TraceQualityTests` positive substrings **+ new negative substrings**
  (sibling artifacts must be absent) **+ the CreateItem≠UpdateItem divergence test**.

### 4. Stats / metadata
- Counts reconcile with detections; per-seam edge counts × resolution split; entry→target coverage.
- *Checked by:* `json-range` / `detection-count`, plus a stats reconciliation check.

### 5. Browse (once the query layer lands, Phase 5+)
- Navigable both directions: `neighbors(id, in|out)` and `find_usages(id)` return correct edges.
- *Checked by:* query-API unit tests (both edge directions).

## Per-phase ratchet (the bar "as we progress")

Each phase is done when these checks pass (flip aspirational→expected / add as expected).

| Phase | New/flipped acceptance checks |
|---|---|
| **0 Hygiene** | No `output-contains "over budget"`; output bounded by depth/fan-out on DntSite+OrchardCore; budget checks removed; build/tests green. |
| **1 Correct traces** | **ENFORCED** (2026-06-28): `CatalogApi:CreateItem` trace ≠ `:UpdateItem` trace (divergence test passes). `POST /api/orders` trace `output-not-contains "CancelOrderCommand"` / `"ShipOrderCommand"` / `"OrderShippedDomainEvent"` / `"OrderCancelledDomainEvent"`; genuine spine (send→handler→raises OrderStartedIntegrationEvent→outbox) survives. `[verified]` redefined as method-scoped. See `TraceQualityTests.Sibling_methods_produce_divergent_traces_no_fabricated_edges` + `Orders_trace_keeps_the_real_spine_and_drops_sibling_edges`. |
| **2 Universal entries** | **ENFORCED** (controller fixture 0/3→3/3; controller sibling-action divergence test passes; `GET /` infra entry filtered from eShop). See `controllerapp.json`, `TraceQualityTests.Controller_sibling_actions_produce_divergent_traces`. |
| **3 Complete+honest traces** | **ENFORCED** (2026-06-28): `POST /api/orders` renders `raises OrderStartedDomainEvent → consumes ValidateOrAddBuyer…Handler` (domain-event path), TOUCHES includes `Buyer` reached via `Calls` (High-5), the pipeline (`LoggingBehavior → ValidatorBehavior → TransactionBehavior`) once under the send, and an explicit `…omitted` / `stopped at depth N` truncation marker. See `TraceQualityTests.Orders_trace_is_complete_and_honest`. Re-probe recorded in `docs/reports/probe-phase3.md`. *(DntSite `GET /Feed` TOUCHES still empty — its EF entities aren't detected by `EfCoreExtractor`; that detection gap is deferred to Iteration 4, so the High-5 mechanism is asserted on eShop.)* |
| **4 Honest Map** | **ENFORCED** (2026-06-28): OrchardCore arch.style ≠ "Microservices" (`ModularMonolith`); Catalog scope-stamp present (`5-project closure of 24-project eShop`); STACK/PACKAGES `output-not-contains "$("`; Catalog CRUD types not tagged aggregate (`DDD aggregates` removed). Unit tests: partial-closure suppression + whole-solution Microservices preserved. See `catalog.json`, `ArchitectureStyleDetectorTests`, `docs/reports/phase4-honest-map.md`. |
| **5 Queryable kernel** | Query-API tests (both directions) green; CLI output unchanged through the new layer. |
| **6 Performance** | `max-elapsed-ms` ratcheted down (warm); DntSite cold materially < 41s. |
| **7 Browse UI** | UI re-query (focus/depth/detail) does no re-analysis (interaction test). |
| **8 MCP** | MCP tool contract test; re-probe as `C + MCP`. |
| **9 Index + GitHub** | Re-open near-instant (cache-hit test); GitHub-URL → Map honest-scope test. |
| **10 Coverage rungs** | Per rung: entries resolve on a representative repo; "ripgrep test" passes for that shape. |

## The "ripgrep test" (cross-cutting, run continuously)

Zero-config, seconds (warm), genuinely useful on each shape:

| Shape | Repo | Must produce |
|---|---|---|
| Controller Web API | DntSite | entries **with** targets, a correct trace, stats |
| Minimal API | TodoApi | correct entries + trace + TOUCHES |
| Blazor / front-end | `eval-repos/blazor-samples/9.0/BlazorWebAppMovies` | component entries, topology |
| CQRS / DDD / events | eShop Ordering.API | correct `POST /api/orders` trace (no sibling edges), matches IDEAL-OUTPUT-TARGET §2 shape |
| Library | AutoMapper | useful surface map (not a call-stack dump) |

## The periodic LLM probe (capability acceptance)

Beyond unit checks, re-run the before/after probe (`IDEAL-OUTPUT-TARGET.md §7`) after Phases 1, 3, and
8: give a fresh agent a real task with (a) the trace, (b) raw files, (c) repo+tools, (d) repo+tools+trace.
Acceptance target: the fixed trace moves from "primer" (cheap orientation) toward "accelerator"
(reduces a tool-using agent's cost). Record each run in `docs/reports/`.

## Adding/flipping a check

See `eval/expectations/SCHEMA.md` for check types. Put **Map/detection** assertions in the per-repo
JSON; put **Trace** assertions (positive *and* negative) in `TraceQualityTests`. Never add a UI
feature whose data the kernel can't honestly answer — acceptance is defined here first, then built.
