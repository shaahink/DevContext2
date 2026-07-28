# DECISIONS — R3 feature/UI redesign

> The record of what the owner decided, per `R3-FEATURE-REDESIGN.md` §1.2. One entry per decision
> area, with the rationale in one line and the evidence it was decided against. Implementation only
> starts once a page's decision set is complete (§1.3).
>
> Evidence base for every entry below: `eval-results/2026-07-28/r3-current-state/` — a re-drive of
> the app on the POST-Batch-A..E engine. The 2026-07-27 `ui-feature-audit/` screenshots predate every
> R2 fix and must NOT be used to judge current state.

## Standing context — why S6 decides differently than R3 anticipated

R3 §3 conditioned the canvas/workspace decisions on "R2 Batches A+B (true edges, true handler
chains, transports)". Those landed in S2–S5. Re-measured on eShop 2026-07-28:

| | 07-27 audit (pre-Batch-A) | 07-28 (post-Batch-E) |
|---|---|---|
| transport links on topology canvas | 5 (bus only) — E1 | **23** (queue · http · grpc, drawn distinctly) |
| `POST /api/orders/` handler join | In:1 **Out:0**, dead end — E2 | **verified** → `IdentifiedCommandHandler` |
| graph size | — | 1152 nodes · 1089 edges · 109 entries |
| on screen when Explore opens | nothing | **still nothing** (W1) |

The engine defect chain that blocked this strand is closed. What remains on the workspace page is
render/IA, which is what R3 exists to decide.

---

## D-A. Workspace default + information architecture

**DECIDED 2026-07-28 (owner): A1+A2 hybrid — the centre pane's altitude follows focus.**

> Decision brief the owner reviewed (three architectures as wireframes, against the evidence above):
> <https://claude.ai/code/artifact/552b865b-65d5-4e5e-8267-89248944bdc7>
> Before/after frames: `eval-results/2026-07-28/r3-current-state/eshop/10-explore-default.png`
> (the empty centre) vs `.../eshop-after2/10-explore-default.png` (the topology it opens on now).

- **No focus → the topology canvas.** The page shows the repo the moment it opens.
- **Focus → the trace tree** becomes the centre; the canvas stays available as a toggle of that
  focused state (the existing `flowMode: tree | graph`).
- One centre pane, two states, one honest toggle — replacing today's three competing control axes.

**Rationale (owner-facing, one line):** landing on the canvas was the wrong call in July because the
canvas was a lie (8 of 12 services floating, bus-only legend); at 23 transport links it is now the
best single answer the product has to "what is this repo", while a deep flow still reads better as a
tree than as a graph — so take A1's default and A2's focused view rather than choosing between them.

### D-A sub-decisions

| # | Question | Decision | Rationale |
|---|---|---|---|
| A-1 | Node inspection: docked panel or modal? | **Dock; kill the modal** | The Details panel already exists, already docked, and is empty on every captured frame. A modal blocks the workspace it describes. |
| A-2 | Trace past a service boundary | **Collapse to ONE expandable row per boundary, naming the seams that cross** | See below — owner-delegated. |
| A-3 | Depth control | **Budget-elastic + labelled** | Graph still defaults to depth 1 with an unlabelled native select and a "7%" meter no legend explains; the CLI shows 6 for the same repo. |
| A-4 | Entry-list truncation | **Middle-ellipsis** | Six rows render `/api/catalog/i…` — the distinguishing segment is exactly the part being cut (W5). |
| A-5 | Trail / pin loop | **Promote to a first-class right-panel section** | It seeds the Studio pack — the product's best cross-page loop — and is discoverable today only via a rotating status-bar hint. |
| A-6 | `j`/`k` scrub · `Shift+E` audit table · omnibox · zen | **Survive untouched** | All still work, all still good (FINDINGS §7 "must not be lost"). |

### A-2 in full — the CrossService wall

**Owner delegated this one with criteria rather than picking an option:** *"make browsing enjoyable
and informative and quick."* Decided on those criteria:

The problem (new since Batch E, and a render decision, not an engine one — the edges are real):
`TracePolicy` put `ServiceLink` into the one seam order, so the tree renders every service hop.
`POST /api/orders/` produces 20+ `CrossService` rows below the handler — Ordering.API four times,
Webhooks.API three, each its own branch with its own "N omitted". The trace stops describing *this
order flow* and starts describing the mesh.

Rejected alternatives:
- *Stop at the boundary + re-root* — quick, but least informative: it hides that the flow crosses
  services at all and costs one click per service to learn what one row could say.
- *Dedupe by service* — compresses the wall without fixing its premise; the mesh still dominates
  the reading of a single flow.

**Decision:** collapse each run of sibling `CrossService` hops into one expandable row that **names
the services** and is honest about what it is hiding:

```
Handle  IdentifiedCommandHandler
        ▸ crosses 5 services · 33 hops · 21 omitted
          Basket.API · Catalog.API · WebApp · Webhooks.API · PaymentProcessor
```

- **Quick** by default: one row, so the flow stays readable top-to-bottom.
- **Informative** even closed: the service names are the answer to "where does this go", and the
  hop/omitted counts say exactly how much is behind the disclosure.
- **Enjoyable**: progressive disclosure — the reader chooses the depth — and it reuses the honesty
  idiom the product already speaks (`truncated · N omitted`).
- Expanding restores today's exact subtree, so nothing becomes unreachable.

**Scope correction, made while implementing (2026-07-28):** the first draft of this decision said the
collapsed row would also name the *transport kinds and crossing events* ("queue
OrderStartedIntegrationEvent · http 2 · grpc 1"). **It cannot, honestly.** `TraceNode`
(`proto/devcontext/v1/devcontext.proto:323`) carries `seam`, but the transport kind lives on the
ServiceLink **edge**, not on the node — the trace tree simply does not have it. Rendering it would
mean inventing it, which PRODUCT-DIRECTION §2.2 forbids. The row therefore ships with service names
+ counts, all of which are real.

> **Open engine follow-up (not done):** to get the richer label, `TraceNode` needs the crossing
> edge's transport kind (and, where known, the event name) projected onto the node — a proto field +
> a `TracePolicy` change. Worth doing; explicitly out of scope for S6 because it is engine work
> arriving after Batch E closed.

Reversible: this is a render policy, not a data change. Judge it running, not on paper.

---

## D-B … D-H — NOT YET DECIDED

Open. `D-B` (canvas semantic language) is next and now carries more weight than R3 anticipated,
because D-A makes the canvas the landing surface. Current-state evidence for it is captured at
`eval-results/2026-07-28/r3-current-state/eshop/20-atlas.png` — known open items visible there:
`eShop.AppHost`/`HybridApp`/`ClientApp` still render as floating peers (AppHost should be an
orchestrator frame per R3 §2 D-B), no kind glyphs, no store cylinders drawn, repeated `apphost` edge
labels adding noise, and Top-flows still ranked by internal DomainEventHandlers over user-facing
endpoints (audit A2, unchanged).
