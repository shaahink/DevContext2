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

### D-A sub-decision status, re-checked against the code in S7

Three of the four "decided, still owed" items were not what the S6 note said they were. Checked
before implementing, because building to a stale premise is how a fix lands on a bug that moved:

| # | What S6 recorded as owed | What the code actually said (2026-07-28) |
|---|---|---|
| A-3 | "still depth-1 + an unexplained 7% meter" | **Premise stale.** `stage.ts` defaults `graphDepth` to **3** and the select already reads "depth 1…depth 4". The label half of the decision is done; **budget-elastic is still open** and is the only part left. |
| A-4 | middle-ellipsis truncation | **Half-built, never fired.** `middleEllipsis` has existed since T6.8 but its threshold was 48 characters while the deck column shows about 34 — so CSS `truncate` always cut first, and the audit's six identical `/api/catalog/i…` rows were the result. **Fixed in S7** by putting the budget under the column width. |
| A-5 | "promote the Trail to a first-class right-panel section" | **Already satisfied.** The inspector has carried a `trail` section since `f81a31f`; the S6 evidence frame shows it expanded, with its own hint copy. Nothing to do. |
| A-1 | "dock the node card; kill the modal" | **Real, and bigger than it reads.** Still open — see below. |

**A-1 is deferred to S8 with a dependency the decision did not see.** The Inspector's Details section
already renders everything the modal shows *except its neighbour lists*: the docked panel has
Details, Code, Insights, Call Stack and Trail, and Call Stack is the path from the ENTRY to the
selected node, not the node's Called-by/Calls. Deleting `NodeCard` today would therefore delete the
only surface those lists have. Killing the modal is one line in `node-link.ts`
(`nodeStore.show` → `trace.selectNode`); doing it honestly means giving the Inspector a Neighbours
section first, fed by the `trace.neighbors()` the store already loads on selection.

---

## D-B. Canvas semantic language

**DECIDED 2026-07-28 (owner): B2 is the language, B1's frame is a conditional enrichment, and B3's
lane grammar is where the canvas is a picture rather than a workspace.**

> Decision brief the owner reviewed (three positions as wireframes on post-Batch-E eShop data):
> <https://claude.ai/code/artifact/25a53935-e9a2-49da-9bd9-486381d7db25>
> Current-state frame: `eval-results/2026-07-28/r3-current-state/eshop-after2/10-explore-default.png`
> — 12 service boxes · 23 transport links · ~9 repeated `apphost` edge labels · 3 floating peers ·
> 0 kind glyphs · 0 stores drawn.

- **Transports are the edge layer.** Only a real transport (http · grpc · queue · event) draws a
  line. A deployment reference is not a call and stops competing with calls for the edge layer.
- **A declared orchestrator draws a frame, not edges.** Where an AppHost exists, containment says
  once what nine identical `apphost` labels were saying. Where none is declared, the frame simply
  does not appear — the grammar below it is unchanged, which is why B2 and not B1 is the base.
- **Lane arrangement is for the picture surfaces.** Home's *What runs* and Atlas, where the canvas is
  read rather than operated.

**Rationale (owner-facing, one line):** B2's grammar is the only one of the three that survives
contact with the rest of the 47-pole matrix — every repo has calls, almost none has an AppHost — and
it extends the verified/inferred honesty idiom the trace tree already speaks instead of adding a
second one; B1 is strictly better than nine identical edges wherever an orchestrator is actually
declared; B3 reads best where the canvas is a picture and worst where a service owns two roles.

### D-B sub-decisions

| # | Question | Decision | Rationale |
|---|---|---|---|
| B-1 | Legend | **Always-visible strip, listing only the classes present on this canvas** | Collapsed into a corner button today, so the colour language is undiscoverable on the surface Explore now opens on. |
| B-2 | Parallel edges | **Collapse per pair per transport, count on the label (`http ×3`)** | 23 links currently draw 23 lines and 23 labels; the count is the information, the repetition is not. |
| B-3 | Viewport fit | **Let the landing pane fit at a higher zoom clamp than an embedded hero** | eShop's topology uses about a quarter of the pane it now owns. |
| B-4 | Node tap | **Keep expand-in-place; the same tap fills D-A's docked panel** | Two behaviours that don't conflict — one reveals structure, the other describes the node. |
| B-5 | Nodes in no flow | **Named tray, not floating boxes** | `ClientApp`/`HybridApp`/`eShop.AppHost` float today. A tray that says how many and why is honest; whitespace is not. |

### B3's placement — owner-delegated

The owner picked B2+B1 for the working canvas and kept B3 on the grounds that it is *"very good
visual … we can use them some places in the app"*, without naming the places. Decided on that:

**B3 is not a new mode or a new toggle.** Home's *What runs* and Atlas already arrange in lanes (DDD
layers, live since D4.2) — what they lack is B3's vocabulary. They inherit it rather than gaining a
sixth artifact (PRODUCT-DIRECTION §3) or a fourth control axis on a page D-A just simplified.

> **Scope correction, made while implementing (2026-07-28) — the second of this decision.** The
> paragraph above originally promised the lane views would also get "transport-coloured edges in
> place of grey csproj dependency lines". **They cannot, honestly.** Lanes live at the ALL-PROJECTS
> level, whose edges are csproj references; transports are a SERVICE-level fact and there is no
> service-to-project mapping that makes a project reference into a queue. Colouring them would be
> inventing traffic. What the lane views can honestly inherit is the kind glyph (for the projects
> that are services) and a store lane. Deferred to S8 with that narrower scope, because the frame
> and the edge layer were what the landing surface needed first.

> **Honesty guard.** A lane claims a service has one role, and some genuinely have two (`WebApp` is a
> client that is also called). The lane is assigned by the service's dominant owned entry surface and
> the box keeps its kind glyph, so the glyph can contradict the lane rather than the lane being the
> only claim on screen.

### What this costs, and what is engine work

Kind glyphs are **dead by construction** and must be revived before any of this reads: service nodes
are created with no layer (`GraphBuilder.Nodes.cs:45`) and `ClassifyService` only returns "Web API"
when the layer is `Api` (`GraphProjections.cs:126`), so every service on every repo classifies as
"Service" and every glyph renders empty. The replacement derives the kind from the entry surfaces a
service actually owns — evidence the graph already carries and already attributes per project.

Two more facet gaps: `ServiceCard` has no field for the `Store` nodes Batch B emits, so the canvas
cannot draw stores; and `TransportLink` carries no resolution tier, so topology edges cannot make the
verified/inferred distinction the trace tree makes. Both are one field each.

**Correction made while implementing (2026-07-28).** The brief said the drawing "occupies the lower
third of a pane whose top half is empty". It does not — `fitAndCenter` centres it correctly. What is
actually wrong is the fit CLAMP: `MAX_FIT_ZOOM` is 1.25, a ceiling that exists so a three-node graph
does not balloon inside an embedded hero, and on a full pane it holds eShop's 12 boxes to about a
quarter of the space available. B-3 is therefore a clamp that knows which surface it is on, not a
centring fix. Recorded rather than quietly restated, per the A-2 precedent.

Reversible: everything except the three facet fields is render policy. Judge it running, not on paper.

---

## D-C … D-H — NOT YET DECIDED

Open. Current-state evidence for `D-C` (library) and `D-D` (CliTool) was **not** gathered in S6 —
capture FluentValidation and GitVersion before either is briefed. Known open item carried for `D-E`:
Top-flows is still ranked by internal `DomainEventHandler`s over user-facing endpoints (audit A2,
unchanged) — it belongs to Home, not to the canvas language.
