# UI + Feature Design Audit — live app drive, 2026-07-27

> Owner-directed audit: drive the real desktop app (Angular @ :4200, server @ :5179, branch
> `feat/prism-d5` @ fa9d706), screenshot every surface, judge **feature design + UX + product-owner
> POV** — not cosmetics. Repos chosen to cover the poles: eShop (microservices+messaging),
> FluentValidation (library), GitVersion (CliTool), CleanArchitecture (small CQRS app),
> aspire-samples (sample collection / Aspire). Driver scripts:
> `src/DevContext.App/scripts/ui-redesign-drive*.mts` (Playwright, headless, own browser).
> Screenshots + text dumps: session scratchpad `ui-audit/` (key frames copied to `./evidence/`).
> Companion piece: the same-day engine/consumption audit (4 subagent reports, summarized in the
> session log) — its findings are cross-referenced as [ENG-*].

## 0. The one-sentence verdict

The app shell, honesty machinery, and Studio/MCP mechanisms are genuinely good; the product fails
where the graph under it is wrong or starved — the flagship views (workspace tree/graph, topology
canvases) render *less and worse* than the CLI for the same repo, and three of five archetypes get
a stub or dead experience.

## 1. Engine-rooted defects visible in the product (the causal chain)

These block every downstream redesign; no UI work can compensate.

| # | Defect | Evidence | Root cause [ENG] |
|---|---|---|---|
| E1 | **Sync transports missing entirely.** eShop CROSS-SERVICE = bus(5) only; zero gRPC/HTTP ServiceLinks. Home + Atlas canvases show 8/12 services floating unconnected; legend has one transport kind. | CLI map (bus 5 only); atlas "36 dependency edges · 5 transport links"; home canvas | GrpcClient/Refit/HttpClient link detection requires both ends in-solution + didn't fire on eShop's patterns; TALKS-TO facet never built |
| E2 | **Send→Handle join broken for generic dispatch.** Node card: `IdentifiedCommand` In:1 **Out:0**. UI trace for POST /api/orders/ dead-ends at the Send; CLI trace (arg-bound CreateOrderCommand) reaches the handler chain. | eshop3/90-91 nodecard; eshop2/71 graph | Generic-arity erasure in node identity; name-keyed Handles join [ENG: structural NodeId] |
| E3 | **Dup-name CrossService noise.** CreateOrderRequest/OrderServices sprout "CrossService Basket.API/WebApp" chips + graph nodes (every service defines its own CreateOrderRequest). Inflates "9 cross-service" flow stats in Atlas. | eshop/11, eshop2/71, atlas top-flows | Name-keyed cross-service bridging, no project scoping [ENG: SymbolTable honesty] |
| E4 | **Primary-call target junk.** ~10 MAUI [RelayCommand] entries target `→ IAppEnvironmentService` (a DI dep); insights evidence shows "POST /api/catalog/items → GetEmbeddingAsync", "PUT … → Entry". | eshop2/77 audit table; insights page | Primary-call pick heuristic + noise lists [ENG: resolver unification] |
| E5 | **UI trace noisier + shallower than CLI.** ILogger rendered as a step (CLI filters it); graph defaults depth 1 (CLI: 6); different send-edge pick than CLI. Same focus, three surfaces, three different traces. | eshop/11 vs CLI baseline | No single trace contract; divergent defaults [ENG: one render core] |
| E6 | **Multi-solution repos silently scoped to one sln.** aspire-samples renders as "Metrics · 4 projects · 96 types" — one sample presented as the repo, no indicator, no picker. | aspiresamples/01 | Solution discovery picks first sln; P15 scoping UX never built |
| E7 | **Test projects pollute the graph on nonstandard naming.** GitVersion hubs: `EmptyRepositoryFixture (602)` = #1 wiring hub. | gitversion/30 insights | ProjectClassifier misses GitVersion's test naming |
| E8 | **Aspire resource topology extracted, discarded.** AppHost renders as a plain floating box on both eShop and the Aspire samples repo; redis/postgres/queues invisible. | eshop atlas; aspiresamples atlas | AspireResourceDetection has no render path [ENG: dead detections] |

## 2. Workspace page (Explore) — the graphs page, judged as the core product

**Feature design**
- W1. Default state is an empty center canvas ("Select an entry — j/k to scrub"); ~85% of the page
  blank on landing. The page never shows the repo until you pick a row.
- W2. Graph mode defaults to **depth 1** with an unlabeled native select; an unexplained "20%" fill
  meter. The product's central view shows one hop while the CLI shows six.
- W3. The Tree is a flat indented list: no expand/collapse, no per-branch omission counts, no
  "N branches cut" affordance; signatures truncate mid-parameter.
- W6. Node inspection is a **modal with a backdrop** — it blocks the workspace it describes. Its
  content is thin: kind, one tag, in/out degree, CALLED BY, Trace/Copy ID. No file:line link, no
  source preview, no edge list, no "open in graph".
- W7. The right Details panel (Code/Insights/Call Stack/Trail accordions) stays collapsed and empty
  through the whole flow — a second, competing inspection surface that never fires.
- W4. The toolbar stacks three unexplained control axes (Service/Layer/Feature color · Flow ·
  Entries/Tree/Graph · "approx only") with no grouping or labels.
- W5. Left entry list truncates so hard that six `/api/catalog/i…` rows are visually identical;
  path-derived group headers (`eShop/WebApp/Components/Pages/Item`) mix into service grouping.

**What's genuinely good (keep):** auth padlocks on entries; kind chips; j/k scrub; Shift+E raw
audit table (the full-screen grouped table is excellent); zen mode; the Trail/pin→export-pack
mechanic (buried in the status bar); omnibox with Trace/Node/Usages/Impact/Copy action tabs.

## 3. Canvases (home "What runs", Atlas hero, workspace Graph)

- C1. Services view: uniform gray boxes, only queue edges (E1), no kind glyphs at readable size, no
  store cylinders, no dashed externals, AppHost floats as a peer. Communicates near-zero for the
  flagship repo.
- C2. "All projects" lanes view: DDD lanes render but as an illegible project-reference hairball —
  tiny labels, dozens of crossing thin lines, ClientApp floating inside a huge empty Infrastructure
  lane. Mechanism landed; communication didn't.
- C3. Workspace Graph at depth 1 is dominated by E3 noise nodes + ILogger; all edges render equally
  dashed (verified vs approx indistinguishable at a glance).
- C4. Data stores exist as a text list in Atlas but are never drawn into any canvas.

## 4. Per-archetype product experience

| Archetype | Experience | Verdict |
|---|---|---|
| Microservices app (eShop) | Rich but wrong where it matters (E1/E2/E3) | Fix data, then canvases |
| Small app (CleanArchitecture) | Healthy: 7/7 wired, sensible START HERE, honest style | The baseline that works |
| Library (FluentValidation) | Workbench = good text lists (front doors, docs, tabs); **zero graph anywhere**; Atlas = page of empty-states incl. contradictory "index flows from Explore" hint; **Studio dead** ("Analyze a repo…" copy while analyzed, 0 of 0); insights "273 public types" contradicts identity strip "92 public types" | Library is half a product: orientation ok, exploration/packs absent |
| CliTool (GitVersion) | 1 entry ("Main"), 0/1 wired, wiring health 0%, style chip literally "Unknown", workspace = dead single row; command-tree insight detects 1 command on a many-command tool | Archetype detected, product never built |
| Sample collection / Aspire (aspire-samples) | Silently scoped to one sample sln (E6); SampleCollection style chip works; Aspire resources invisible (E8) | Scope honesty gap + discarded data |

## 5. Page-by-page findings (beyond the workspace)

**Home**: identity strip + FRESHNESS (HEAD, Current chip) + wiring health = genuinely good.
- H2. "37% verified" chip vs tooltip "40% edges verified, 69% approximate" — disagree and sum >100.
- H3. START HERE suggests a truncated MAUI "[RelayCommand] C…el.CheckoutAsync" over POST /api/orders.
- H4. Pre-analysis landing has no recents/sessions — retype the path every session.
- H5. Loading show is honest but minimal (2 rows in an empty viewport).

**Atlas**: the content below the hero is the page's real value — Top flows w/ metrics, the
publisher→event→consumer board (the F3 matrix exists ONLY here), data stores, per-service breakdown
with stack chips, hub radar.
- A2. Top flows ranked by internal DomainEventHandlers (inflated by E3 "cross-service" counts) above
  user-facing endpoints.
- A3. Hub radar leaks mangled names ("global.CatalogServices", "Service.WebApp … in 1 · out 0").

**Insights**: mechanism good (severity + confidence + evidence + Trace-it).
- I2. Validity: "18/18 write endpoints have no FluentValidation validator" on a repo whose Ordering
  commands ARE validated via ValidatorBehavior (endpoint-level counting reads as a false claim).
- I3. Two near-duplicate auth insights; Identity.API quickstart pages counted as product auth surface.

**Context Studio**: the best-designed feature in the product — per-card meters, provenance lines,
LIVE PREVIEW = exact copy, VERIFICATION panel (per-file fingerprint freshness), OMITTED list, budget
slider with named cuts.
- S2. It packages the defective flow (E2/E3/E5) — mechanism ≫ substance.
- S3. Scope picker is entries-only (inherits the engine's entry-only pack model) — can't build a pack
  around a type/member; dead for libraries.
- S4. State bugs: "0 of 109 selected" while 9 cards live; meter total (~1955) ≠ preview total (1804).

**MCP page**: strongest page — status, host configs (Claude Code/Cursor/VS Code), sessions table,
live feed w/ agents-only filter + token totals, tool sandbox. Sandbox Run needs a handle picked
(no default to current session).

**Settings**: fine (Appearance vibes / Analysis / Storage / Server / About).

**Nav/chrome**: Explore's nav icon is swallowed by a "99+" badge (Insights shows "10") — counts
replace identity; status-bar hint rotation is the only discoverability for the best mechanics
(p-to-pin, Shift+E, Escape-backs-out).

## 6. Ideas / solutions (feature-design level, ranked inside each lane)

**Lane A — make the graph true (engine, prerequisite for everything)**
1. TALKS-TO: outbound sync links from GrpcClient/Refit/AddHttpClient detections without requiring
   both ends in-solution; external targets as dashed nodes. (Kills E1; feeds every canvas.)
2. Structural node identity (generic arity, nested types, method arity) + project-scoped joins →
   fixes E2 (Send→Handle for IdentifiedCommand<T,R>) and E3 (dup-name links). [ENG surgery #1/#2]
3. One trace contract: same seam picks, same noise policy (ILogger out), same defaults (budget-elastic
   depth, not depth-1/depth-6 lottery) served to CLI/MCP/app from one core. [ENG render kernel]
4. Primary-call pick: prefer semantic action targets over DI-injected service fields (E4);
   suppress framework leaves in UiEntry targets.
5. Scope honesty: multi-sln repos → explicit solution picker + "analyzed X of Y solutions" banner (E6).
6. Aspire resource graph → SERVICES canvas (AppHost as orchestrator grouping, stores as cylinders,
   external resources dashed) (E8). CLI tools: render the command tree as the entry surface (GitVersion).

**Lane B — workspace redesign (the page users live in)**
1. Default state = the repo, not emptiness: land on topology (services + transports) with the entry
   list as overlay/filter; selection narrows instead of populating.
2. One inspection surface: dock the node card into the right panel (kill the modal), with file:line,
   source peek (read_source exists), edge list by kind, and "re-root here".
3. Tree v2: expandable branches, per-branch "(N omitted — expand)" counts, verified/approx visually
   distinct on edges, depth tied to budget with an honest "depth-limited, N% budget unused" line.
4. Controls: label the three axes (View: Entries/Tree/Graph · Color: Service/Layer/Feature ·
   Filter: approx), explain the fill meter, promote depth to a labeled control.
5. Surface the Trail/pin loop as a first-class right-panel section (it already seeds the Studio —
   the product's best cross-page loop is nearly invisible).

**Lane C — canvases that communicate (after Lane A data lands)**
1. Services canvas: transport-labeled edges (http/grpc/queue with distinct styles), kind glyphs,
   store cylinders, dashed externals, AppHost/orchestrator as a grouping frame, domain lanes.
2. All-projects view: edge bundling or on-hover-only ref edges; lanes sized to content; readable labels.
3. Draw the data stores (Atlas has them as text; the canvas should show service→store edges from
   ReadsWrites/EF evidence).

**Lane D — archetype completeness (product parity for non-app shapes)**
1. Library: give the workbench a canvas (namespace map / abstraction-inheritance wheel); type-rooted
   Studio packs (fixes S3 + the get_context library hole in one contract); reconcile the two public-type
   counters; suppress the flows-oriented Atlas sections for libraries instead of empty-stating them.
2. CliTool: command tree as entries (verb/option table), wire Main → command handlers so wiring
   health isn't 0%; kill the "Unknown" style chip (suppress like libraries do).
3. Sample collection: per-sample cards (T8's SamplesAreTheProduct) + solution picker (E6).

**Lane E — trust & polish (cheap, high-credibility)**
1. Reconcile every number pair: verified chip vs tooltip (H2), public-type counts (library),
   studio meter vs preview totals (S4), "0 selected" vs live cards.
2. Insight validity pass: kill duplicate auth insights; validation insight must count command-level
   validators; exclude identity-server quickstart from product auth surface. (P7 harness exists —
   point it at these.)
3. START HERE ranking: user-facing endpoints first (never truncated RelayCommands).
4. Recents on the landing hero (ListSessions exists); default MCP sandbox handle to current session.
5. Nav rail: icons + labels, badges as badges.

## 7. What must not be lost in the redesign

Identity strip · FRESHNESS card (HEAD + Current) · wiring-health card · Shift+E audit table ·
omnibox action tabs · Studio card/meter/verification/omitted machinery · MCP page wholesale ·
library front-door taxonomy (register/derive/implement/extend) · Atlas event board + per-service
breakdown · honest empty-state *tone* (the copy just needs to stop contradicting the state).
