# M — Meridian: Wiring Truth, Agent Surface, One-Page Repo (Mega Plan)

> Successor to `proposal-lighthouse.md`. Grounded in
> `lighthouse-delivery-audit.md` (same date, same folder) — read that first: every
> stage here traces to a verified failure, not a hunch. Waterfall, one agent session
> per stage, M0 harness gates everything. Branch: `feat/meridian-m0` off
> `feat/lighthouse-l2` (or off `develop` after Lighthouse merges).
>
> **Executing agents: `meridian-agent-playbook.md` (same folder) is mandatory reading
> before any stage** — quality bar, anti-patterns, per-surface UI specs, run/test
> instructions, and the session handoff protocol. Tracker: `MERIDIAN-START.md` at repo
> root.
>
> **v2 (2026-07-05, explicit renumber — never renumber silently):** added M8 Context
> Studio (new user feature: merged Export + LLM pane into a graph-driven context
> builder); M2 gains 2.4 (layer/feature classification); M3.3 upgraded from "MCP
> panel" to a dedicated MCP page; M7 expanded with chrome/affordances (7.4) and Table
> lens v2 (7.5); former M8 close-out is now **M9**. Decision log gains D7–D9.
>
> **Nominated dogfood repo (all stages):** `C:\Users\shahi\source\repos\run-aspnetcore-microservices\src`
> (`eshop-microservices.sln`) — Carter + MediatR + Marten + MassTransit/RabbitMQ +
> gRPC + YARP + Refit + Razor Pages, 11 projects. It is the repo in the user's
> screenshots, it is mainstream .NET, and today DevContext fails it. Secondary:
> `dotnet/eShop` (Aspire shape), `davidfowl/TodoApi` (minimal shape),
> `ardalis/CleanArchitecture` (template shape).

## 1. Vision

One sentence: **a dev (or agent) pointed at an unfamiliar .NET repo should get the
architecture and any end-to-end flow faster, cheaper, and more truthfully than by
reading files — and today that is only true for trivial repos.**

Three product claims Meridian must make true on the dogfood repo:

1. **The wiring claim** — `POST /basket/checkout` traces endpoint → command → handler
   → publish → RabbitMQ → Ordering consumer → CreateOrder → domain events, across
   three services, each step with file:line. Today: 2 nodes.
2. **The agent claim** — an MCP agent answers "how does checkout create an order?"
   correctly in ≤3 tool calls and ≤2k response tokens, beating a grep-equipped agent
   on both correctness and cost. Today: the answer is *wrong* (empty impact, dead-end
   trace) and the server didn't even start before this audit's fix.
3. **The one-page claim** — Home tells a dev in ten seconds what the repo is (services,
   how they talk, where to start); Atlas prints as a one-page architecture doc a tech
   lead would actually hand to a new joiner. Today: stat soup and a CLI dump in a div.

## 2. Evidence

See `lighthouse-delivery-audit.md` §§2–4. Shorthand used below:
**W1** invisible handlers (derived `ICommandHandler<,>`), **W2** Sends misses
Adapt-constructed commands, **W3** trace stops at command, **W4** zero cross-service
edges (bus/gRPC/Refit/YARP), **W5** cross-project name collisions, **W6** impact
returns nothing/false. Plus: fake Razor routes, DI-extension bus entries, GATEWAY
archetype overclaim, folder-name "module map", broken Insights→Explore focus jumps,
MCP transport starvation + empty tool descriptions, Atlas = raw CLI text.

## 3. Decision log (votes recorded — do not re-litigate)

| # | Decision | Rationale |
|---|----------|-----------|
| D1 | **MCP = thin stdio adapter; DevContext.Server is the one analysis host.** MCP exe stays a separate binary (agent hosts expect to spawn a command) but holds **no pipeline** — it proxies to the Server over gRPC (localhost), auto-starting it if absent. | One graph in memory instead of three; UI and agents see the *same* session; sessions survive agent restarts; UI gains observability for free. UI never needs the CLI; CLI stays standalone for one-shot use. |
| D2 | **UI gets an MCP panel**: start/stop the MCP endpoint, live tool-call feed (tool, args, response size, est. tokens, duration), per-session token totals, and a kill switch per session. | The user's explicit ask: "see what's going on, the back-and-forth, token usage and control over it." Server-side event stream already exists in skeleton (progress events) — extend, don't invent. |
| D3 | **Graph gains a service layer.** Nodes carry `Service` (runnable project grouping); new edge kind `ServiceLink` (kinds: http-via-gateway, grpc, bus-publish→consume, refit-direct) with provenance both ends. System altitude renders services + ServiceLinks, not project refs. | W4 is not renderable without a first-class concept. Also the unit agents reason about. |
| D4 | **Trace/context output is compact text, not pretty JSON**, for agent-facing tools; JSON stays for UI RPCs. Relative repo paths everywhere; no confidence float on every envelope. | Token discipline. Measured: envelope + absolute paths ≈ 30–40% of every MCP response today. |
| D5 | Archetype vocabulary gains `microservices`; style becomes **per-service** with a solution-level rollup. | GATEWAY/CleanArchitecture solution-wide claims are wrong on the dogfood repo. |
| D6 | Insight actions become **typed** (`focus:<entry>` / `node:<id>` / `filter:<kind>` / none) — the UI never navigates on raw evidence strings. | The broken-jump class of bug becomes unrepresentable. |
| D7 | **Context Studio replaces both the Export drawer and the Explore LLM-context pane.** One surface where a user composes precise LLM-ready context from the graph — scope seeds (flow / class / service / trail / "I'm changing this endpoint"), per-section toggles incl. with/without code bodies, live token meter, copy/export. | The two panes duplicate `ContextPackBuilder` output with different UIs and neither lets the user *choose* what goes in. The job-to-be-done: update an endpoint without dumping whole files into a web LLM. |
| D8 | **MCP becomes a dedicated left-rail page**, not a settings panel: status + host config snippets, session list, live token-metered tool-call feed, budget caps, kill switch, manual "try a tool" console. | A rail page has room to be genuinely informative (the user's ask); a buried panel doesn't. |
| D9 | **The graph is modeled with three node facets — `Service`, `Layer`, `Feature` — plus flows, and every surface renders its own *lens* (projection), never the same System canvas.** Lenses: Service map, Layered view (with dependency-violation edges), Feature columns, Flow stepper. Per-archetype facet mapping: web/microservices (service = runnable project; layer = Api/Application/Domain/Infrastructure/Contracts), library (layer = Public API/Core/Internals; feature = namespace area), desktop (service = module; layer = View/ViewModel/Domain/Platform). | "System-level graph is the same every page" — because there is only one projection today. Layering/feature structure is statically derivable and is exactly the architecture picture devs want. |

## 4. The waterfall — stages M0 → M8

### M0 — Harness first: the gates that L7 skipped
*One session. Nothing later ships without these passing.*

- **0.1** App-repo bench gate: `scripts/bench.ps1` set = eshop-microservices (local
  path), eShop, TodoApi, CleanArchitecture, DntSite (re-clone). Bench fails loudly on
  a stub report (`_No analysis data available._` ⇒ exit 1). Re-generate the L7 stubs
  (eShop, TodoApi) so `eval-results/` carries no dead artifacts.
- **0.2** MCP agent-QA harness (extends this audit's driver script, checked into
  `eval/mcp-qa/`): scripted question set against the dogfood repo — checkout flow,
  discount callers, impact-of-handler, ambiguous `Product`, config lookup — each with
  expected-answer assertions + token budgets. Runs headless; produces a scored table.
  Include the transport regression checks (analyze response must flush unprompted;
  server must start cold).
- **0.3** Playwright visual gate for the four surfaces against the dogfood repo
  (screenshot per surface per theme; interaction steps for graph zoom + node select —
  the cytoscape freeze lesson).
- **Gate:** all three harnesses runnable in one command each; current failures
  recorded as the baseline table in the tracker.

### M1 — Wiring truth pass (the engine earns the product back)
*Two sessions: in-service wiring, then cross-service.*

Session A — in-service:
- **1.1** (W1) Handler joins resolve interface inheritance transitively: build an
  interface-derivation closure from the type model (`ICommandHandler<,>` →
  `IRequestHandler<,>`), match handler detections through it. Golden: dogfood repo
  yields ≥14 `Handles` edges (today 2).
- **1.2** (W2) Sends via local semantic pass: when an argument to
  `Send/Publish/PublishAsync` isn't a `new X()`, resolve the local's declared/inferred
  type (Roslyn semantic model already loaded — no MSBuildWorkspace needed). Covers
  `Adapt<T>()`, factories, variables. `Sends` seam verified-tier when semantic.
- **1.3** (W3) TraceBuilder traverses Sends→Handles→(handler body)→Raises/Publish
  chains; `touchedEntities`/`emittedEvents` populate from the full walk.
- **1.4** (W5) `NameResolver` becomes project-scoped: same-project match wins; cross-
  project only via project-reference direction; never cross unrelated projects on a
  short name. Kills the Product→ApplicationDbContext lie and the false impact edge.
- **1.5** Presentation truth: Razor route = real `@page` route (`/ProductDetail`),
  PageModel title as subtitle, `Page()`/framework helpers dropped from traces; Bus
  entries exclude DI extension methods (require `IConsumer<T>`/handler evidence);
  report footer project count fix.

Session B — cross-service (W4), each join is detection + `ServiceLink` edge + trace hop:
- **1.6** MassTransit: `Publish<T>` sites → `IConsumer<T>` implementations across
  projects (event-type FQN join). Dogfood golden: BasketCheckoutEvent links Basket.API
  → Ordering.Application.
- **1.7** gRPC: generated client usage (`XProtoService.XProtoServiceClient.Method`) →
  service implementation (proto symmetry already half-exists: per-method entries).
  Golden: Basket.API → Discount.Grpc per-method links.
- **1.8** HTTP-client seams: Refit interface routes + `HttpClient` BaseAddress config →
  downstream endpoint route matching; YARP `ReverseProxy` config (appsettings
  routes/clusters) parsed so gateway-prefixed paths resolve to the target service's
  route table. Golden: Shopping.Web `GET /catalog-service/products/{id}` →
  YarpApiGateway → Catalog.API `GET /products/{id}`.
- **1.9** Archetype/style: `microservices` detection (≥2 runnable web services +
  gateway/bus evidence); per-service style rollup (D5). Identity sentence data fixed
  at the source (services = runnable projects, not project count).
- **Gate:** M0.1 bench + checkout-flow golden (≥10 steps, 3 services); `impact
  (CheckoutBasketCommandHandler)` returns `POST /basket/checkout`; zero regressions on
  the library repos' reports.

### M2 — Relevance pass (insights a dev respects)
*One session.*

- **2.1** Retire/repair the discredited sources: folder-name "module map" (use real
  service/feature grouping), "downstream wiring" (only actual ServiceLinks as
  evidence), dead-code (only after W1–W4 edges exist; exclude DI/framework-resolved
  types), CLI copy leaks out of insight text.
- **2.2** New insight classes, all wiring-grounded and archetype-aware:
  event flow map (published events with/without consumers), cross-service
  single-points-of-failure (services all flows traverse), endpoints missing
  validation (FluentValidation absent on command path), config keys consumed
  without defaults (`GrpcSettings:DiscountUrl`), auth surface (kept — it's correct —
  but excludes Razor pages from "endpoints" counting).
- **2.3** (D6) Typed insight actions end-to-end (engine → proto → UI); evidence chips
  render as links **only** when carrying a resolvable target.
- **2.4** (D9) Layer/feature classification: every Type node gains `Layer` and
  `Feature` facets, evidence-based (project role + folder conventions + base types +
  reference direction), per-archetype mapping per D9. Emit `LayerViolation`
  detections (e.g., Domain → Infrastructure reference). Ships as node metadata +
  stats; **rendering waits for M6/M7 lenses** — no UI in this stage.
- **Gate:** on the dogfood repo, every insight card's every click lands on a working
  view; insight set reviewed against "would a senior dev nod?" per item in the PR
  description; layer facet coverage ≥90% of Type nodes on dogfood + CleanArchitecture
  with spot-check table in the PR.

### M3 — MCP re-architecture (D1/D2 plumbing)
*One session.*

- **3.1** Server-of-record: session manager keyed by repo path+HEAD in
  DevContext.Server; MCP exe becomes a stdio↔gRPC shim (spawns/attaches to Server);
  CLI untouched. Fix inherited bugs at the root: transport starvation (analyze reply
  must flush immediately — no fire-and-forget on the transport's context), the DI
  crash fix from the audit stays.
- **3.2** Tool schema hygiene: every tool gets a written description + param docs +
  1-line example (the agent's onboarding — budget ~40 tokens each); envelope trimmed
  (D4): repo-relative paths, no per-response confidence, single compact `meta` line.
- **3.3** (D2/D8) Observability stream + **dedicated MCP page** (left rail, own
  route): server broadcasts tool-call events (tool, session, args digest, bytes,
  est. tokens, ms); the page shows server status + start/stop, host config snippets
  (Claude Code / Cursor / VS Code, copy with feedback), session list (repo, age,
  token total, evict), live feed with per-call token chips, and a "try a tool"
  console (pick tool, fill args, see raw response + cost). Functional this stage;
  visual polish rides M7.0 tokens. Nominated layout in the playbook §UI-MCP.
- **Gate:** M0.2 harness green including cold-start and flush regressions; the MCP
  page shows a live feed of the harness run; toggling MCP off stops new sessions.

### M4 — MCP feature set (what an agent actually needs against a .NET repo)
*Two sessions. Derived from the dogfood transcript, not speculation.*

- **4.1** `overview(handle)` — one-call repo brief ≤600 tokens: identity, services +
  ServiceLinks summary, top 5 flows by name, conventions (CQRS/pipeline behaviors),
  where-to-start pointers. Replaces 4-call discovery ritual (stats+map+entrypoints+insights ≈ 4k tokens today).
- **4.2** `resolve(query)` — symbol/route/file → candidates with kind+service+path;
  **never silently picks** among ambiguous matches (the `Product` trap). All other
  tools accept only resolved ids or unique names.
- **4.3** `flow(entry, format=compact)` — the M1 cross-service walk as indented text
  with `file:line` per step and seam glyphs; `[approx]` only where true. This is the
  flagship tool; it must answer the checkout question alone.
- **4.4** `impact(target, direction=up|down, transitive)` — over the repaired graph,
  grouped by service, entries first. Plus **diff-aware mode**: `impact(files=[...])`
  for "I changed these files, what flows/entries/tests are affected" — the agent
  pre-commit killer feature.
- **4.5** `read_source(id|path, mode=member|window)` — full member body via graph
  spans (today: arbitrary 20-line window that decapitates methods).
- **4.6** `find(pattern, kind?, service?, limit/cursor)` — paginated, kind-filtered;
  node titles never contain code bodies (fix the lambda-title leak at the source).
- **4.7** `config(key?)` — configuration keys → binding/consumption sites; the
  `GrpcSettings:DiscountUrl` question in one call.
- **4.8** `get_context` v2 — rebuilt on `flow` + full-member bodies + DI wiring +
  entities; sections actually filled; explain-mode identity paragraph written from
  overview data. (Today: 167-token empty box.)
- **4.9** `tests_for(id)` — test methods whose call closure reaches the node (best-
  effort, labeled).
- **Gate:** M0.2 QA suite: every question answered correctly; checkout question ≤3
  calls, ≤2k tokens; grep-baseline comparison table written into
  `eval-results/<date>/MCP-VS-GREP.md`.

### M5 — Agent eval ratchet
*One session — the M0.2 harness becomes a permanent, ratcheted eval.*

- **5.1** Extend QA set to eShop (Aspire), CleanArchitecture, TodoApi, DntSite; add
  per-question token ceilings as assertions (ratchet like golden tests).
- **5.2** Record a real agent transcript (Claude Code against the shim) for the
  checkout question; commit the transcript as the demo artifact.
- **5.3** CI wiring: `dotnet test` category `McpQa` + bench smoke in the verify gate.
- **Gate:** ratchet file committed; transcript reproduces; regression on any ratchet
  fails the build.

### M6 — Home + Atlas: the one-page claim
*Two sessions (Home; Atlas). Design tokens first — see M7.0 note below if sequencing flips.*

- **6.1** Home = repo card, answers-first: (a) **service map hero** — the D3 service
  graph as a small, opinionated diagram (services as labeled cards, ServiceLink arrows
  with seam glyphs; gateway left, bus bottom), not a physics layout; (b) identity
  paragraph in prose ("A YARP gateway fronts three services; orders flow from Basket
  to Ordering over RabbitMQ…" — generated from graph facts, every claim clickable);
  (c) stat strip retired — replaced by three meaningful tiles (Entries by kind with
  sparkbar per service · Wiring health = % entries with complete flows, linked to the
  unwired list · Analysis freshness/time). "confidence 21%" dies; the ledger moves to
  a diagnostics drawer; (d) onboarding row: "Trace checkout · Open the atlas · Point
  your agent here" (MCP snippet). Numbers a dev can't act on don't render.
- **6.2** Atlas = the printable one-pager, structured top-down: ① service diagram
  (same hero, bigger, with edge counts); ② **top flows as stepper strips** — each a
  horizontal chain of steps (service-colored chips: endpoint → handler → event →
  consumer …) that a reader scans like a sequence diagram, click-through per step;
  ③ event wiring board on real ServiceLinks (approx tag gone; W5-copy empty state
  gone); ④ per-service style/stack cards (D5); ⑤ cross-cutting (behaviors, health
  checks); ⑥ hub radar. Raw CLI markdown dump removed; "Export one-pager" produces a
  self-contained HTML/PDF.
- **Gate:** M0.3 screenshots; the "hand it to a new joiner" review on the dogfood
  repo; Atlas one-pager export opens standalone.

### M7 — Explore, chrome, and the Table lens
*Three sessions: (A) tokens+chrome+affordances, (B) graph↔code + lenses, (C) Table lens v2.*

- **7.0** Design-token pass (do first, sweeps all surfaces): minimum body text 12px,
  icons 14–16px with per-kind color coding, contrast audit for chips/subtle text —
  kills the "squint" problem globally.
- **7.1** Graph↔code binding: selecting a node opens its source (full member, syntax-
  highlighted) in the inspector *simultaneously* with graph focus; selecting a trace
  step highlights the corresponding edge + scrolls code to the line; ESC returns.
  Code pane is a first-class Explore citizen, not a peek.
- **7.2** (D9) Lenses land: System altitude = Service map lens with drill-in;
  **Layered lens** (layer bands, violation edges highlighted) and **Feature lens**
  (feature columns) selectable; Flow altitude gets the M6 stepper as an alternative
  linear view. Each page owns its default lens + remembers it — the identical
  System canvas repeated on every page is retired. Label collision fixed by the
  card-based lens layouts (no free-floating overlapping labels).
- **7.3** Trail: dedupe consecutive visits, group by flow, cap visible length with
  "n earlier" expander; deck rows get service-colored kind dots and full-route
  tooltips.
- **7.4** Chrome pass (the VS Code comparison): top bar (workspace picker + omnibox —
  clearer hit targets and active states), left rail (persistent labels or hover
  labels + active indicator bar + legible badges), **tabs** (taller, readable title,
  hover close, strong active contrast — today they're hard to see), footer status
  bar (segmented: repo/stats | health/version, each segment clickable). Plus the
  **feedback affordance rule** everywhere: any click that performs an action shows
  visible confirmation (copy → icon morphs to check + "Copied" toast; export →
  progress → done state). No silent buttons survive this checkpoint.
- **7.5** Table lens v2 (Shift+E — today "a big table"): becomes the repo
  spreadsheet with graph-derived metadata columns and row relationships —
  per-archetype:
  *web/microservices*: route · service · target handler · flow depth · downstream
  services touched · events published · auth · validation · **shares-logic-with**
  (entries whose flows converge on the same handler/entity/repository, rendered as
  linked chips: "shares OrderRepository with 3 endpoints") · **touch-risk** ("changing
  this affects N flows in M services", from impact);
  *library*: public type · kind · extension seats · in-repo usage count · doc
  presence · derive/override seats;
  *desktop*: view · viewmodel · commands · events · module.
  Row expand = mini flow stepper. Group by service/feature; column sets are
  archetype defaults, user-toggleable.
- **Gate:** M0.3 interaction scripts (select node → code visible; select step → edge
  highlighted; lens switch; copy feedback visible) pass; before/after screenshots per
  checkpoint in PR; Table lens renders correct columns for dogfood repo, Serilog, and
  a desktop fixture.

### M8 — Context Studio (D7: the precise-context builder)
*One to two sessions. Requires M1 wiring + M4 get_context v2.*

- **8.1** Surface: new rail page + "Build context" entry points from Explore (from
  current trail/selection) and Home (from a top flow). Replaces the Export drawer
  (Ctrl+E) and the Explore LLM-context pane — both retire; their routes/shortcuts
  redirect here.
- **8.2** Composition model: a context is an ordered set of **cards** — flow
  skeleton, member signatures, member bodies (per-member toggle), DI wiring, config
  keys, entities/contracts, tests-for, repo identity line. Scope seeds: entry/flow,
  type, service, insight, trail, or free pick via omnibox. "I'm changing this
  endpoint" preset = flow + target member bodies + contracts + validators + tests.
- **8.3** Controls: with/without code bodies globally and per card; budget slider
  with **live token meter per card and total** (server-side estimate, same tokenizer
  as MCP); intent selector (trace/explain/review) reordering sections; format
  (markdown / plain); Copy + Save-to-file with feedback affordances (7.4 rule).
- **8.4** Fidelity: every card shows its graph provenance (file:line chips); a
  "stale" banner if the session snapshot is older than the working tree (reuses the
  freshness probe). Output must round-trip through `ContextPackBuilder` v2 — no
  UI-side string assembly.
- **Gate:** scripted Playwright flow — load dogfood repo, seed "update PUT /products",
  toggle bodies off/on, budget 4k, copy — produces deterministic context whose
  token count matches the meter ±5%; golden context file checked into eval; both old
  panes gone with no dead routes/shortcuts.

### M9 — Close-out (the honesty gate, done right this time)
*One session.*

- **9.1** Full bench: all M0.1 repos **plus** PowerToys and MassTransit (the two
  Lighthouse deferred forever); stub reports impossible (M0.1).
- **9.2** `eval-results/<date>/AUDIT.md` scores every W-finding and every M-stage gate
  with fresh evidence; any FIXED verdict must cite a re-run artifact, not code
  existence.
- **9.3** `HANDOVER-MERIDIAN.md` + memory updates + tracker close.
- **Gate:** the three M0 harnesses green on every repo in scope; no deferred-forever
  list without owner + date.

## 5. Success criteria (user-visible)

1. Checkout trace on the dogfood repo: ≥10 steps, 3 services, every step file:line.
2. `impact(CheckoutBasketCommandHandler)` names `POST /basket/checkout`; ambiguous
   `Product` returns a disambiguation list, and each candidate's impact is truthful.
3. An MCP agent answers the checkout question correctly in ≤3 calls / ≤2k tokens;
   the vs-grep table shows DevContext winning on tokens *and* correctness.
4. UI runs without the CLI installed; MCP starts/stops from the UI with a live
   token-metered feed of agent traffic.
5. Home passes the ten-second test on the dogfood repo; Atlas exports a one-pager
   that reads as an architecture doc; no insight click dead-ends; no 10px text.
6. Library/report quality of Lighthouse is not regressed (bench diff clean).
7. A dev updating one endpoint builds a precise, body-toggled, token-metered context
   in Context Studio in under a minute — without opening a single file by hand.
8. The Table lens answers "which endpoints share logic?" and "what should I worry
   about if I touch this?" as sortable columns, for web, library, and desktop shapes.
9. Every actionable button in the app visibly confirms it acted (no silent copies);
   tabs/rail/status bar pass the "can you see where you are at a glance?" check.

## 6. Tracker

**Live tracker: `MERIDIAN-START.md` at repo root** — per-checkpoint status, evidence
links, and the session handoff block. This table is the plan-side summary only;
update BOTH when a stage closes.

| Stage | Sessions | Status | Evidence |
|-------|----------|--------|----------|
| M0 harnesses | 1 | TODO | |
| M1 wiring truth (A in-service, B cross-service) | 2 | TODO | |
| M2 insight relevance + layer/feature facets | 1 | TODO | |
| M3 MCP re-architecture + MCP page | 1 | TODO | |
| M4 MCP feature set | 2 | TODO | |
| M5 agent eval ratchet | 1 | TODO | |
| M6 Home + Atlas | 2 | TODO | |
| M7 Explore + chrome + Table lens (A/B/C) | 3 | TODO | |
| M8 Context Studio | 1–2 | TODO | |
| M9 close-out | 1 | TODO | |

Resume protocol per stage: read `meridian-agent-playbook.md`, then this doc's stage
section + `lighthouse-delivery-audit.md` + the M0 baseline table; run the relevant
harness before and after; update `MERIDIAN-START.md` with commit refs and artifact
paths. A stage is DONE only when its **Gate** line has fresh artifacts — code existing
is not evidence (Lighthouse lesson, encoded).
