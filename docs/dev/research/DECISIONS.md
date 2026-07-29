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

## D-C. The library workspace

**DECIDED 2026-07-28 (owner): C2 — consumer paths are the spine.**

> Decision brief the owner reviewed (both archetypes, on evidence captured the same day):
> <https://claude.ai/code/artifact/4b09714f-db18-4823-9d4b-931f058d2b6e>
> Current-state frames: `eval-results/2026-07-28/r3-current-state/fluentvalidation/` — Home's
> *What runs* hero drawing two boxes and one csproj arrow for something that does not run, and an
> Explore page whose Library surface was a read-only list in a pane 60% empty.

A library has no runtime, so the product's spine (entry → flow → seam) had nothing to hold, and the
Library surface — the best archetype-specific page the product has — was a **dead end**: its rows
were list items, not buttons, so the Inspector, the Trail, the pin and the export pack were all
unreachable from a library's main surface.

- **The four front doors are the spine.** `register · derive · implement · extend` (and the
  abstractions, which are seats you implement) each **open** onto the path they lead down.
- **The path is a real call path.** Clicking `derive AbstractValidator` traces it: `Include` →
  `RuleFor` → `RuleForEach` → `RuleSet`, the exact vocabulary a FluentValidation consumer writes,
  with the `approx` markers the trace tree already speaks.
- **The Trail collects a door** exactly as it collects an entry elsewhere — the loop a library was
  locked out of.

**Rationale (owner-facing, one line):** a library's real question is "how do I use this", and the
product could already answer it — the engine traces a public type into its own pipeline — but the
one surface that asks the question rendered ten rows and stopped.

### Scope correction, made before implementing (2026-07-28)

The brief said "`Consumer paths 6` is already computed and never shown", which is true and was
nearly a trap. `LibrarySurface.ConsumerPaths` is **a template sentence per entry kind**
(`"extend  →  derive AbstractValidator"`), composed from the entry's kind and title, touching no
edge — a recipe stub, not a traversal. Shipping it as "the consumer path" would have dressed a
label as evidence. It stays where it is, as a summary; what opens is the trace. Checked before
building, per the standing rule that cost S7 three stale premises.

| # | Sub-decision | Call |
|---|---|---|
| C-1 | What replaces "What runs" on Home for a library | **Open.** The Explore spine landed first; Home still asks the wrong question of a library. |
| C-2 | Atlas's five empty sections | **Open.** |
| C-3 | `0 entries` where entries do not apply | **Open** — same rule as D-3, and the two should land together. **S9 applied C-3's rule to one panel already**, see below. |

### S9 scope correction — the Confidence Ledger was suppressed on every library

Found while wiring the sparse-graph caveat (S9's contract sweep), and recorded rather than quietly
restated, per the A-2 / B-3 precedent:

`ToStatsResponse` gated the whole ledger on `graph is not null && !entries.IsDefaultOrEmpty`. A
library has no entry points, so **the entire Confidence Ledger disappeared on every library** —
FluentValidation's 169 edges have a computed verified/approximate split that no reader could reach,
because no ledger means no `verified` chip and that chip is what opens the panel. Only two of the
ledger's rows depend on entries at all, and `ConfidenceLedger.Compute` has always been zero-safe.

The gate is now `graph is not null`, and the two entry-dependent rows (auth coverage, entry targets)
withhold themselves when their denominator is zero — which is exactly C-3's rule, applied to the one
panel where the suppression was hiding real numbers. C-3 itself stays open: it is about what Home
and Atlas say to a library, and that is still the owner's call.

---

## D-D. The CLI workspace

**DECIDED 2026-07-28 (owner): D2 → D1 — say the scope first, then make the verbs the centre.**

> Same brief as D-C. Current-state frames: `eval-results/2026-07-28/r3-current-state/gitversion/`
> — `1 entries · 0/1 wired · 11% verified`, and a canvas holding two unconnected boxes at billboard
> size.

**D2 — the scope.** GitVersion declares three solutions; the scorer picks `src/GitVersion.slnx`,
whose CLI is the legacy hand-rolled parser (one `Main`). That is the truth *for that solution* — the
five verbs the tool ships live in `new-cli/`. Batch C built `SolutionScopeNote` for exactly this and
its own comment says the note rides "the same Map/proto/**UI** field". **There was no UI**, and the
app had no `--sln`: a third of a repo was shown as the whole thing, silently, with no way out.

- The app now says which solution it read and offers the others as buttons.
- The proto carries the **facts** (analyzed path, count, alternatives), not the CLI's sentence —
  that sentence ends by naming a flag no GUI has, and each surface should say the truth in its own
  words.
- Absent entirely on a single-solution repo: a scope row on every repo is noise a reader learns to
  skip.

**D1 — the verbs.** A CliTool has no transports by construction, so a topology canvas can only draw
disconnected boxes. `ArchetypeView` has projected a **COMMAND SURFACE** since L7.2 and no desktop
surface read it; it is now the CliTool's landing state, with each command's handler beside it and
`no resolved handler` where the engine could not join one.

**Rationale (owner-facing, one line):** the layout was downstream of the scope — redesigning a page
that was showing the wrong third of the repo would have been decorating a lie — and once the right
solution is in view, a command-line tool's product surface is its verb list, which the engine had
been computing and no one had been rendering.

### The bug D2 uncovered

`AnalysisSessionManager` was idempotent by **repo + HEAD**, so the first solution switch was answered
in 2.3ms with the analysis already in hand: the request reached the wire carrying its `sln` and the
server declined to notice. Idempotence had quietly become a refusal. The session key now carries the
analyzed solution — the rule the snapshot cache learned in Batch C and the session layer never did.

| # | Sub-decision | Call |
|---|---|---|
| D-1 | Where the scope note lives | **Decided (delegated):** the identity strip, under the headline, with the alternatives as buttons — it is a fact about the analysis, and that is where analysis facts live. |
| D-2 | Fit clamp on a tiny graph | **Open.** `MAX_FIT_ZOOM_FILL` 2.1 is right for twelve boxes and absurd for two. |
| D-3 | `0/5 wired` on a CLI | **Open, and the brief's premise was wrong**: wiring is *not* meaningless for a CLI — a verb should reach its handler, and GitVersion's five reach none. It is a real engine gap (the entries are `ICommand<TSettings>` classes whose execute member never joined), now stated on the command surface where a reader is looking instead of only as a percentage on Home. C-3's suppression rule applies to the library, not here. |
| D-4 | Two "service" vocabularies on the Atlas page | **Open.** |

---

## D-E. Home — what a repo's front page says

**DECIDED 2026-07-28 (owner): E3 — the front page asks the archetype's question.**

> Decision brief the owner reviewed (three positions, wireframed on evidence measured the same day):
> <https://claude.ai/code/artifact/973a85a7-5ac5-43e4-8446-05762d0bbe82>
> Current-state frames: `eval-results/2026-07-28/r3-current-state/{eshop,fluentvalidation,gitversion}-s10/`.

There is no universal Home, because the honest question differs by archetype. A **service** is asked
what runs and how it connects — today's page, deduplicated. A **library** is asked how you use it:
the namespace and front-door tiles come up to where an empty canvas used to be, and no canvas is
drawn. A **CLI tool** is asked what it does: the command surface `ArchetypeView` has published since
L7.2 and S8 gave a component, with no services toggle and no two-node frame.

**Rationale (owner-facing, one line):** the page was asking every repo the same question and only one
archetype could answer it — FluentValidation's Home headed a 350px box "What runs" and drew a single
csproj arrow, GitVersion drew two unconnected boxes under a *Services* toggle for a tool that has
none — while the surfaces that DO answer a library's and a CLI's real question already existed and
sat below the fold or on another page.

**E3 carries E1's rule inside it:** every fact is stated once, at the altitude that owns it. Measured
on eShop, three of the five headline numbers were printed twice forty pixels apart, and the wiring
fact appeared three times in two notations (headline · stat row · wiring tile — and types/projects a
third time in the freshness tile).

### D-E sub-decisions

| # | Question | Decision | Rationale |
|---|---|---|---|
| E-1 | Three coverage-shaped numbers on one line, two meaning the same thing | **Keep `64/109 wired` on the strip; `% verified` moves into the Confidence Ledger** | Wired has an obvious meaning; verified is an edge-quality concept whose definition lives in the panel the chip already opens. |
| E-2 | Home and Atlas both rank "Top flows", differently; START HERE is a third rule | **One rule everywhere — request-shaped first, then depth/score** | The product should have an opinion about what matters in a repo. Two rules just document the disagreement. |
| E-3 | Does the mini-canvas stay on Home? | **Yes on a service, nowhere else** | It is the best answer the product has to "what is this repo" — and only where something connects. This IS E3. |
| D-2 | Fit clamp on a tiny graph (carried from D-D) | **Below the drawable minimum, draw no canvas at all** | A clamp still frames emptiness. GitVersion's two boxes needed a sentence, not a better zoom. |

### The regression E-2 uncovered

`START HERE` offered **`Trace [RelayCommand] CheckoutViewModel.CheckoutAsync`** on eShop — a MAUI
mobile view-model command, as the way into a twelve-service backend. `onboarding-row.ts` prefers any
flow whose title matches `/checkout/i` and reaches ≥4 nodes, and its own comment records why that
was safe: *"on eShop EVERY checkout-titled entry is a 1-hop client command (CheckoutViewModel
.CheckoutAsync = 2 nodes), so a title match alone can't deliver the ≥3-hop gate"* — with the stated
intent that *"Trace POST /api/orders/draft is the story a first visit should open on"*.

**Batches A–E made that comment false.** The resolver got honest, the client command now clears the
depth gate, and the special case beats the request-shaped preference it was written to protect.
A heuristic tuned against a starved graph inverted when the graph stopped being starved — worth
remembering wherever else this program left a threshold calibrated on pre-Batch-A data.

---

## D-F … D-H — NOT YET DECIDED

Open. Evidence for all three was captured in S10 and is summarised at the end of the D-E brief
above; the areas themselves were deliberately not briefed (owner: implement Home first).

- **D-F (Insights).** eShop ships **three overlapping auth findings** — "36/43 endpoints anonymous",
  "Auth surface: 7 protected, 36 unannotated of 43", and a missing-validation warning over the same
  endpoints. One fact, three cards, three severities. Separately "Entry targets resolved 64/109" is
  an insight *and* the coverage bar directly beneath it. Dedup belongs at the source (engine), so
  the CLI and MCP get it too.
- **D-G (Studio).** Opens on two stacked empty panes. After S10's truncation fix its picker rows are
  still not unique: eShop has five `OrderStatusChangedTo*EventHandler`s agreeing on their last 18
  characters, and three `GET /Account` actions the engine *does* disambiguate (`[Logout]`,
  `[AccessDenied]`) in data the row drops. No truncation setting fixes either — it is a design call.
- **D-H (chrome).** Largely stale: the rail has not replaced icons with badges since M7.4. The one
  live find is on Atlas — **three vocabularies for "service" on one page**: the canvas excludes
  ClientApp/HybridApp, the per-service breakdown lists them as services, and Hub radar mixes services
  with types, one rendering as `` Logging.ILogger`1 `` (raw metadata arity reaching the UI). That is
  D-4. **Its "service" half is DECIDED and LANDED (G6.1, 2026-07-29); the arity half is G6.2.**

### D-4 — one vocabulary for "service" on Atlas (DECIDED + LANDED, G6.1)

**The definition, adopted:** *a service is a project the engine judged **runnable and production** —
`ServiceBoundaryInference.RunnableProjects`, the same list `GraphBuilder.AddServiceNodes` turns into
`NodeKind.Service` nodes. Every Atlas surface that says "service" means exactly that set.* It already
existed and the canvas already obeyed it; the other two surfaces re-derived their own.

**One correction to the find above, measured on the live page before any edit**
(`eval-results/2026-07-29/G6/before-atlas-eShop.txt`): the canvas and the breakdown did **not**
disagree on membership. Both held the same 12 projects — the canvas drew 9 boxes, framed the AppHost
and **trayed** ClientApp/HybridApp as "in no relationship", while the breakdown listed 12 identical
peers. One set, three stories, no surface reconciling its count with the others.

**But the divergence was real, just latent.** Two independent membership predicates existed:
`DetectPerServiceStyles` carried its own skip list, including an infrastructure filter keyed on the
project *name* containing `shared` / `common` / `.eventbus`, and a test filter keyed on the *file
path* rather than the project. They agreed on eShop by luck. Pinned in a unit test that goes red on
the old code: five runnable production projects, of which the breakdown returned two.

What landed:

1. **Engine — one predicate.** `DetectPerServiceStyles` iterates `RunnableProjects` and answers only
   *what style is each of those*. Its second skip list is gone; the AppHost keeps its orchestrator
   **style**, not a membership exemption. Real-repo invariant sweep in
   `eval-results/2026-07-29/G6/`.
2. **App — the picture and the list reconcile.** The Architecture caption leads with the service count
   and accounts for every member of it ("12 services (9 drawn · 1 orchestrator · 2 in no
   relationship)"); the breakdown states the same count and each card names its canvas state. The
   role rule (`classifyServiceRoles`) is shared by both — it used to live inside the canvas.
3. **Hub radar — rows say what they are.** Its titles were carved out of node ids by splitting on
   `[./:]` and keeping the last two segments, so `Service:WebApp` rendered as "Service.WebApp" (the
   node **kind** read as a namespace) while `Service:Webhooks.API` rendered as "Webhooks.API",
   indistinguishable from the type rows beside it — **seven of eShop's ten hubs were Service nodes**.
   The server's `FlowIndexBuilder` now carries each row's title, kind, project and flow count, and the
   app renders the graph's own facts. The client's duplicate top-10 ranking is gone with it.
