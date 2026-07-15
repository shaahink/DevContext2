# DevContext — Feature-Design & Correctness Audit (live blind-drive)

**Date:** 2026-07-15
**Branch:** `audit/ui-feature-design` (worktree `C:/Code/DevContext2-audit`, base `feat/tapestry-t1` @ c23f346 — includes the wrap-up + MCP fixes)
**Method:** Product-as-a-user audit, **no engine/UI source read** (only routes + DOM selectors for automation). Isolated stack: server `:5279`, Angular `:4300`, driven headless via Playwright (6 scripted rounds, `src/DevContext.App/scripts/audit-drive*.mts`). Real repo: `eval-repos/eShop` through every page; CLI `report`/`query` over 8 benchmark libraries (AutoMapper, FluentValidation, Polly, Serilog, MediatR, Dapper, Hangfire, MassTransit).
**Purpose:** the "what we show wrong / what we could show better" list that seeds the follow-up **code audit**. Part F maps each finding to the code area to open.
**Artifacts:** screenshots + per-page innerText + element inventories + captured exports under the session scratchpad (`…/scratchpad/audit/eshop{,2..6}`, `…/scratchpad/audit/cli`). Key exports are quoted inline so this doc stands alone.

Angular feature checklist covered: home (start-hero, identity-strip, run-console, tiles, onboarding), workbench (entry-deck, stage, lens-switcher ×5 lenses, trace-node, table-lens, inspector ×5 sections, node-peek, trail), atlas (+architecture-panel, one-pager export), insights, context-studio (scope-picker, composition-view, budget-panel, presets, intents, formats, Copy/Save), mcp page (host configs, sessions, live feed, try-a-tool), settings (all 5 groups), omnibox, styleguide. Not exercised: Tauri shell specifics, multi-tab beyond one repo, `From current trail` preset.

---

## 0. Verdict

The skeleton is right: one engine, honest-by-design surfaces (approx markers, per-card token meters, kind chips, auth badges), a genuinely good Table lens with CSV, a real MCP story, and a 29 s cold analysis of eShop. What breaks trust is that **almost every number a user reads on the first screen is wrong or unexplained** (181 "endpoints", 75 "gRPC", 19 "services", "36% verified", "atlas 84/100"), the **flagship flow renders as a two-node stub**, and the **token-export pipeline exports stale content** while its own meter says "over budget". The product currently *demos* worse than its engine actually is — most damage is in classification, counting, naming, and wiring the last mile, not in missing features.

**Top 10 (ranked by trust damage × fix leverage):**
1. Trace dies at hop 1 for MediatR param-passed commands (A1) — the "orders draft" hero flow is 2 nodes.
2. Entry taxonomy is polluted: gRPC helpers, MAUI ViewModel/Animation methods, Blazor pages as HttpEndpoints, literal duplicates (A2–A5) — 53 % of eShop's deck is ClientApp noise and every downstream count/insight inherits it.
3. Copy/Save export stale packs — budget & format changes are ignored after compose; plain ≡ markdown (C1).
4. Service names truncate to "API" ×5 in per-service breakdown and the one-pager export (A8).
5. The messaging story is absent/contradictory: 1-row event board, "0 cross-service" in one export, fabricated-looking `[CrossService]` hops in another (A10).
6. Home/Atlas "how services connect" draws **no edges** — a single-column card stack (B1), while the Service lens proves the graph renderer works.
7. Security/insight cards computed over the polluted taxonomy, ranked with "12% conf" first, GETs counted as unvalidated writes (A11).
8. CLI `query entrypoints|stats|map` all return the same map payload — two of three ops are mis-wired (A15).
9. Onboarding tiles point at the weakest output (unwired `GET /checkout`), then vanish on revisit; no session restore (B2–B4).
10. Unexplained metrics everywhere: "0%", "36% verified", "atlas 84/100", SHARED column (B6).

---

## Part A — Correctness: what we show that is wrong

### A1. Trace stops at the first hop for MediatR param-passed commands — CRITICAL
`POST /api/orders/draft` → Flow lens shows exactly: `[Entry] POST /api/orders/draft → [Call] OrdersApi.CreateOrderDraftAsync` — end. No `Send(CreateOrderDraftCommand)` → handler hop, no TOUCHES/EMITS. Same 2-node stub appears in the Context-Pack flow card. This matches the known span-scoping bug (`GraphBuilder.AddSends`/`ResolveVariableNewType` resolve over the whole type body; param-passed commands fall through). The deepest flows the engine *does* find (32-step domain-event traces) prove the machinery works when the seam resolves.
**Show instead:** the mediator dispatch as a first-class `[Send]` hop; when the seam can't be resolved, an explicit `⚠ dispatch unresolved (command param)` node instead of silent termination.

### A2. gRPC entry over-detection + facet count mismatch
Deck lists 7 `Basket.*` gRPC rows, of which only `GetBasket`/`UpdateBasket`/`DeleteBasket` are proto RPCs — `ThrowNotAuthenticated`, `ThrowBasketDoesNotExist`, `MapToCustomerBasketResponse`, `MapToCustomerBasket` are private helpers. Meanwhile the kind chip claims **gRPC 75**. Only ~7 gRPC-shaped rows exist in the DOM; the ~68 surplus aligns almost exactly with ClientApp's plain ViewModel/Animation methods. Either the kind is misassigned on those entries or the facet counter is wrong; both are user-visible lies. (Home "ENTRIES BY KIND" inherits it.)

### A3. MAUI ClientApp floods the entry surface (96 of 181 entries)
Plain lifecycle/mutator methods (`ViewModel.OnPropertyChanged`, `ViewModel.UpdateLatitude`, `IViewModel.IsBusyFor`), and even animation classes (`Animation.FadeOut`, `StoryBoard.BeginAnimation`) are entries. `[RelayCommand]` members appear **twice** (as `[RelayCommand] CheckoutViewModel.CheckoutAsync` and again as `ViewModel.CheckoutAsync…`), and `ViewModel.InitializeAsync` appears 5× with no disambiguation. Entry = "where execution starts"; these are neither commands nor routes.

### A4. Blazor pages are `kind:HttpEndpoint`
`GET /`, `/checkout`, `/cart` (WebApp/WebhookClient/HybridApp Razor pages, target `*.razor:1`) carry `kind:HttpEndpoint`. Consequences: HTTP=56 is inflated; the top security insight ("49/56 endpoints anonymous, incl. 14 POST/PUT/DELETE") counts storefront pages and IdentityServer quickstart UI as anonymous API endpoints. They already have a home (UI kind) — route-shaped UI entries should be `UiRoute`, not HTTP.

### A5. Literal duplicate entries
`GET /api/catalog/items` ×2 (identical route + handler chip), `POST /Device` ×2, `GET /` ×3 (legit — 3 apps — but rendered indistinguishably). Angular fires NG0955 duplicated-key warnings in 4 components (deck, top-flows, scope-picker, trail) — the duplication is in the data, and the UI has no disambiguator (controller action name, file:line) on collision.

### A6. The "target" chip has three different meanings
Rows show: entry class (`CatalogApi`), a parameter record (`CatalogServices`), or a deep transitive callee (`CatalogAI.GetEmbeddingAsync` for POST items, `PublishThroughEventBusAsync` for PUT items/{id}). A dev reads this column as "handler"; showing an arbitrary interesting callee without labeling is misattribution.

### A7. `global` leaks as a grouping/name everywhere
Ordering.API rows grouped under "global"; hub radar "global.CatalogServices"; module map "global (7 entries)"; export IDs "`Member:global.OrdersApi…`". File-scoped/missing namespace resolution falls back to a literal that reads like a bug.

### A8. Service display name = last dot segment
Per-service breakdown (Atlas + one-pager export) renders Basket/Catalog/Identity/Ordering/Webhooks .API all as "**API**"; the export's AppHost line is "`AppHost → API, API, API, OrderProcessor, API, PaymentProcessor, WebApp, WebhookClient, API`". Corrupts the flagship export outright.

### A9. Archetype gaps and overrides
WebApp = "Gateway [YARP]" (it's the Blazor storefront; YARP package beat the Blazor signal). ClientApp/HybridApp (MAUI), PaymentProcessor/OrderProcessor (workers), WebhookClient (Blazor) all "Unknown — no stack signals detected" despite obvious signals. On libraries: AutoMapper="NLayer", Polly="MinimalApi", MassTransit="Microservices" (see D).

### A10. The messaging backbone is invisible — and three surfaces contradict each other
eShop is RabbitMQ integration events end-to-end; the Event wiring board shows **one** row (a domain event, marked approx). The Atlas one-pager says "0 cross-service" for all five top flows; the Context-Pack flow card shows `[CrossService] Webhooks.API / Ordering.API / Basket.API / WebApp` dangling under `[Call] CatalogServices [approx]` in a DELETE-item trace (wrong parent node, dubious set). Publisher→event→consumer (Sends×Consumes) exists in the graph (home shows "bus-publish→consume") but is never joined into one truthful board.

### A11. Insight cards mislead
- Ranked list opens with "**12% conf**" and "13% conf" Warnings — low-confidence first, and raw confidence percentages erode trust rather than building it.
- "Missing validation: 43/56 endpoints" counts GETs while the copy says "every write endpoint needs a validator".
- "Possible dead code": `OrderItemEntityTypeConfiguration` (convention-discovered), DI `Extensions` classes, `IUnitOfWork` — classic false positives; on MediatR the list includes `MediatRServiceCollectionExtensions` while the *same report* says "AddMediatR (38 impls)".
- "Internal hubs: heavily-referenced" with 1–2 refs ("(1 refs)").
- Copy leaks the wrong archetype voice: "the desktop app's connective tissue", "the library's 'heart'" on a microservices repo; "use `--focus` for deeper traces" is a CLI flag inside the GUI.
- Two cards state the same fact (anonymous endpoints / auth surface).

### A12. Line-number stamping is shallow
Razor entries all `:1`; Call Stack shows entry and handler both at `OrdersApi.cs:16`; export member locations end with a bare `Location: …OrdersApi.cs:` (colon, no number). Agents can't jump; humans can't verify.

### A13. Inspector "Code" shows the wrong region
For `/api/orders/draft` the preview is a raw file window starting at the `MapPost` line that scrolls into **`CancelOrderAsync`'s body** — a different handler. It should show the resolved target member's span.

### A14. Numbers disagree across surfaces
Same repo, same day: GUI identity 1156 nodes / 904 edges / "1.2K types" vs CLI query 1137 / 886 / 523 types. AutoMapper report: "2778 total public types" (Insights) vs "LIBRARY (138 public types)" two sections later. Statusbar "atlas 84/100" on one page, "76/100" on another; "36% verified" (home) has no definition anywhere. Every unexplained or contradictory number is a trust withdrawal.

### A15. Query/tooling round-trips broken
- CLI `query entrypoints`, `query stats`, `query map` all return the **same map summary payload** — no entry list, no stats. Two of the documented ops are wired to the wrong handler.
- MCP page "TRY A TOOL" rejects the handle shown in its own Sessions table: `Error: [not_found] Unknown session handle: a08195c3` (display-truncated handle isn't accepted back). The error surface itself is good; the loop is broken.

### A16. Test/sample projects pollute topology
"19 services" includes `Ordering.FunctionalTests`, `Ordering.UnitTests`, `Basket.UnitTests`, `ClientApp.UnitTests`, `Catalog.FunctionalTests` as service cards. MediatR "Most depended-upon: **MediatR.Examples** (9 dependents)" outranks MediatR itself; Hangfire lists `Hangfire.Core` twice (5 and 3 dependents — package vs project dedup); Dapper lists a literal `*` dependency (MSBuild wildcard).

### A17. Library repos get the wrong lens
The LIBRARY view (ENTRY API verbs, ABSTRACTIONS with implementor counts, GENERATORS) is the right product for these repos — but headline styles say NLayer/MinimalApi/Microservices (samples' shape wins), MediatR gets *no* LIBRARY section (style=SampleCollection routes it to the app view: one bogus entry, a self-referential 1-step "Top Flow", and a CROSS-CUTTING claim that eight **test fixture** behaviors run on "every command"). Detection ≠ render selection, again.

---

## Part B — Feature design, per surface

### B1. Home & Atlas: the connection views don't connect
"How services connect" (home) and "Service diagram — 19 projects · 36 dependency edges" (Atlas) both render a **single-column stack of cards with zero edges**. The Workbench Service lens renders a real cytoscape graph of the same data, so this is the hero component, not the renderer. Also on Atlas the MAP header is a raw text wall (the CLI blob pasted as a paragraph, TFM strings and all: "net10.0-android;net10.0-ios;net10.0-maccatalyst…" twice).
**Better:** the hero *is* the graph (grouped: services / tests collapsed), MAP header decomposed into chips/rows, TFMs deduped and humanized ("net10.0 + MAUI targets").

### B2. First-contact flow showcases the weakest output
Deck default order starts at unwired Blazor `GET /` (one-node trace, "0%"); the START-HERE tile "Trace checkout" lands on `GET /checkout` — also a one-node trace ("Entry focus — click a node…", nothing to click). The engine's best demos (32-step domain-event flows, 25-step PUT catalog) are two scrolls away.
**Better:** default deck sort = wired-and-deep first; "Trace checkout" should resolve to the deepest flow matching "checkout" (`[RelayCommand] CheckoutViewModel.CheckoutAsync` or `POST /api/orders/`), never to an unwired UI route.

### B3. START-HERE tiles are one-shot; agent hint is dev-facing
Tiles ("Trace checkout", "Open atlas", "Point your agent here", "Run report") exist only on the freshly-analyzed home; revisiting `/` they're gone (verified: `Run report`/`Trace checkout` count 1 right after analysis, 0 on return). And "Point your agent here: **pnpm dev:web → MCP ready**" tells the *user of DevContext* to run DevContext's own dev script — meaningless outside this repo; the MCP page already has the right content (host-config JSON).

### B4. No session restore in the client
Every new browser context lands on the empty start screen; `/context` and `/explore` render empty shells. The server keeps sessions (MCP page lists them) but the client can't reattach. This turned three audit rounds into re-analyses; a returning user pays the same tax. (Planned I10 "idle restore" — currently the single biggest quality-of-life gap.)

### B5. Deck rows collapse into identical strings
15 Catalog rows render as "GET /api/c… eShop/Catalog/API" (route truncated at the same prefix). With dup entries (A5) and triple `GET /`, the deck cannot be scanned.
**Better:** middle-ellipsis keeping the distinguishing tail; disambiguate collisions with action/file; optional group-by-service headers (scope-picker already groups — deck should offer it).

### B6. Metrics without meaning
"0%" on every flow header (unexplained; even a 25-step flow shows 0%), "36% verified", "atlas 84/100" (changes to 76 between pages), Table's SHARED column, "Reached by 1 flow". No tooltip, no legend, no docs link anywhere.
**Better:** every metric chip gets a hover definition + a "how computed" line; drop any score we can't explain in one sentence.

### B7. Insights page ranking/copy (see A11)
Also: "Trace it →" exists on some cards, not others; "Engine details" link at the bottom is engine telemetry, not a user insight.

### B8. Context Studio interaction traps
- Preset "I'm changing this endpoint" **silently no-ops** with nothing selected (verified: click → "0 of 181 selected · No cards yet"). Disable it + hint, or make it pick the current workbench selection.
- The select→"Add to context" two-step is invisible: Copy/Save sit disabled with no pointer to why. (Cost this audit three rounds; will cost users the same.)
- Dead stub cards (Config "~10L", Tests "~15L", Entities "~20L") still render with phantom size estimates, then produce nothing ("Entities — 0 tok" section in the export).
- `omitted[]` is still never shown (verified at 1k budget: no "omit" text anywhere in the DOM).
- Budget slider after compose: meter turns red "~2189 / 1000 tok — Over budget — remove cards or increase limit" but nothing re-fetches, and Copy exports the old pack (C1). Either re-pack on change or say "re-compose to apply".

### B9. MCP page: right idea, two blockers
Host-config cards (Claude Code/Cursor/VS Code) with copy buttons are exactly right (verify `devcontext-mcp` actually resolves on PATH for a dev build). Sessions table + LIVE FEED with per-call ~token estimates is a genuinely novel observability surface. But (a) the feed logs the **UI's own gRPC traffic** — 163 calls/≈99 k tok just from rendering home+atlas — so real agent traffic will drown; tag calls by origin (UI vs MCP) and default-filter to agents. (b) TRY-A-TOOL: Run is disabled until a handle is typed, the page never prefills it from its own Sessions row, and the displayed handle is rejected (A15). One-click "use this session" fixes the whole loop.

### B10. Table lens is the best surface — finish it
METHOD/ROUTE/HANDLER/SERVICE/KIND/SHARED/RESOLUTION/AUTH + column picker + CSV export. Gaps: RESOLUTION shows absolute machine paths (B13); SHARED unexplained; AUTH renders "Authorize" or "—" (roles/policies would fit); verify CSV exports all 181 rows, not the virtualized window.

### B11. The UI recomputes the world on every page load
Rendering home/atlas fired ~150 `GetTrace` + dozens of `GetNode` calls in ~2 s (visible in the MCP feed; also 8 aborted `GetTrace` on navigation). Flows/hub-radar look client-assembled per visit. Server-side `top_flows`/facet caching (or a session-scoped memo) would cut chatter and make the MCP feed readable.

### B12. Settings
Analysis defaults (depth/detail/Roslyn/auto-cleanup) — good, verify they actually thread into new analyses. Storage honesty is good. **Server group shows the configured constant (`5179`), not the live connection** — this audit ran against `:5279` while the page claimed 5179; show `serverBaseUrl()` + actual health target. About page (privacy line, update check) is right.

### B13. Absolute paths everywhere
Details rail, Call Stack, Table RESOLUTION, and every export line carry `C:\code\DevContext2\eval-repos\…`. Repo-relative paths (with copy-absolute affordance) are shorter, portable, and token-cheaper.

### B14. Small frictions
- Left-rail "99+" badge is permanently saturated (deck count) — badges that never change are noise.
- Statusbar insight ticker is nice but rotates unexplained numbers (B6).
- `allowSignalWrites` deprecation + cytoscape wheel-sensitivity warnings in console (cosmetic, but they're the app's own).

---

## Part C — Token export audit (the user-facing artifacts)

### C1. Copy/Save export stale bytes — CRITICAL for agent workflows
Captured live: compose 3 cards at 4000 budget → Copy; slider to 1000 (meter goes red) → Copy; format=plain → Copy; Save. **All four artifacts are byte-identical** (9,239 B, header still "_Budget: 4000 tokens_"). Budget changes and the markdown/plain toggle change nothing after compose; Save filename is hardcoded `devcontext-context.md`.

### C2. The pack itself (quoting the captured export)
- `_Archetype: _` — empty metadata slot in line 3.
- "Contracts and interfaces" card is a **verbatim duplicate** of "Member signatures" (597 tok twice; the UI even labels the contracts card "signatures: 597 tok").
- Member locations: `Location: …\OrdersApi.cs:` — bare colon, no line (entry rows have lines; members don't).
- IDs leak `global.` namespaces (`Member:global.OrdersApi.CreateOrderDraftAsync`).
- Bodies fenced as ```csharp but cut mid-statement with no truncation marker (`services.Logger.LogInformation(` … end of block).
- "Entities — 0 tok" empty section still emitted; `<!-- context card: … -->` HTML comments still shipped.
- No repo name / timestamp / analysis handle / staleness line — an agent can't tell what this pack describes or how fresh it is.
- Absolute paths ×27 in a 9 KB pack.
- Flow card carries A1's stub trace and A10's misplaced `[CrossService]` hops — export propagates engine untruths verbatim.

### C3. Atlas "Export one-pager"
Clipboard-only (no download event), 2.4 KB markdown. Compact and the right concept, but corrupted: services list is "API" ×5 (A8), `AppHost → API, API, API…`, Hub Radar with `Service.WebApp` / `global.CatalogServices` and in/out counts that don't correlate (12 flows · in 1 · out 0), Event Wiring = 1 row, every flow "0 cross-service". This is the artifact a user would paste into a PR/wiki — it must be the most correct, not the least.

### C4. Missing export affordances
No JSON export of the pack (structured cards exist server-side); no per-card copy; no export of Insights; CSV lives only in Table lens. "Run report" tile produced no artifact in-run (needs a re-check — likely renders elsewhere or was a dead tile on revisit-home).

### C5. CLI `report` as an export artifact
Ends with engine telemetry (Stages/Extractors/Graph Seams tables) — meaningless to a repo's consumer; keep behind `--stats`. MassTransit report is **476 KB** because PUBLIC SURFACE enumerates 4,531 types (voted V3 "surface cap" not yet applied); MediatR's footer example suggests `--focus "POST /api/orders/"` — an eShop route pasted into every repo's report.

---

## Part D — CLI/report on benchmark libraries ("GUI-simulated" pass)

| Repo | Style shown | LIBRARY view | Report size | Worst content issue |
|---|---|---|---|---|
| AutoMapper | **NLayer** | yes (138 types) | 18 KB | 2778 vs 138 "public types" in one doc; ViewModel-View card (25 VMs from test fixtures) |
| FluentValidation | Unknown | yes (92) | 14 KB | "ViewModel-View: 1 VMs + 0 Views (0 call edges)" |
| Polly | **MinimalApi** | yes (187) | 29 KB | style label; "0 VMs + 1 Views" card |
| Serilog | Unknown | yes (109) | 16 KB | — (cleanest) |
| MediatR | SampleCollection | **no** | 5.6 KB | bogus 1-entry flow; test behaviors as "every command" pipeline; `typeof(IPipelineBehavior<,>)` as a seat name; Examples ranked most-depended |
| Dapper | Unknown | yes (38) | 9 KB | `*` as dependency name |
| Hangfire | Unknown | yes (278) | 35 KB | `Hangfire.Core` listed twice (5 and 3 dependents); "0 VMs + 16 Views" (dashboard pages) |
| MassTransit | **Microservices** | yes (4531) | **476 KB** | uncapped PUBLIC SURFACE; "190 published / 31 consumed / 181 orphan" headline (test noise) |

Patterns: (1) headline style and LIBRARY view are decided independently — pick the lens from the archetype, and when LIBRARY renders, the style line should say Library, not the samples' app shape; (2) ViewModel-View insight needs self-suppression (either side 0, or 0 edges); (3) "most depended-upon"/"dead code" must exclude tests/samples/benchmarks and dedupe project-vs-package; (4) PUBLIC SURFACE needs a cap + "N more" (full list behind `--format json`); (5) LIBRARY ENTRY-API/ABSTRACTIONS sections are genuinely excellent — this is the go-to-lens value for libraries, invest here.

---

## Part E — Cross-surface consistency matrix (same repo, same session)

| Fact | Home | Explore | Atlas | One-pager | Context pack | CLI query | CLI report |
|---|---|---|---|---|---|---|---|
| nodes/edges | 1156/904 | 1156/904 | 1156/904 | — | — | **1137/886** | n/a |
| types | "1.2K" | — | — | — | — | **523** | n/a |
| "endpoints" | 181 (all entries) | HTTP 56 | — | Entries 181 | — | — | — |
| gRPC entries | 75 | 75 chip / **7 rows** | — | — | — | — | — |
| services | 19 (incl. 5 test projects) | — | 19 projects | 19 + "API"×5 | — | 24 projectNames / 19 topology | — |
| cross-service | "bus-publish→consume" | — | 1 event row | **0 everywhere** | **4 hops in one flow** | — | — |
| verified | 36% | — | — | — | — | — | "Verified edges 88%" (MediatR) |
| atlas score | 84/100 | — | — | — | — | — | 76/100 (styleguide page) |

One kernel, one number: every surface should read the same snapshot fields, and every derived metric needs a single named definition.

---

## Part F — Ranked fixes → where the code audit should look

Immediate (trust bugs, high leverage):
1. **A1** mediator dispatch span fix — `DevContext.Core/Graph/GraphBuilder.cs` `AddSends`/`ResolveVariableNewType` (known V1.1 locus); add `/draft`-shaped fixture to Truth gates.
2. **A2/A3/A4** entry taxonomy — gRPC builder (gate on proto-base overrides), MAUI builder ([RelayCommand]+routes only; drop plain methods/animations; dedupe), Blazor route kind (UI, not HTTP); then re-check facet counts (`GetGraphFacets`/entry-kind counting).
3. **A8** service display name — wherever per-service breakdown + one-pager compose names (use project name, never last dot segment).
4. **C1/B8** Context Studio — `context-studio.ts`: re-fetch on budget/format change (or freeze slider post-compose), show `omitted[]`, wire error state, filename `${repo}-context.md`, remove stub cards or implement config/tests; `ContextPackBuilder`: contracts≠signatures duplication, member line numbers, archetype header, empty-section suppression, relative paths.
5. **A15** CLI `query` op dispatch (entrypoints/stats returning map) — `DevContext.Cli` query command wiring; MCP page handle round-trip (send full handle; prefill from Sessions).
6. **A16** exclude test/sample projects from services, depended-upon, dead-code; dedupe package/project rows.

Next (value unlocks):
7. **A10** Sends×Consumes join → real Event Wiring board + honest cross-service counts (one implementation shared by board, one-pager, flow card).
8. **B1** service-map hero: render edges (reuse Service-lens cytoscape), collapse test projects.
9. **A11/B7** insight catalog pass: suppression rules, GET exclusion, threshold sanity, confidence → tier words (or hide), per-archetype copy.
10. **B2/B3** first-run: deck default sort by wired+depth; "Trace checkout" targets deepest matching flow; persistent START-HERE; replace `pnpm dev:web` hint with MCP-page link.
11. **B4** session reattach on boot (list server sessions → adopt latest for the repo).
12. **A14/B6** metric definitions: one place computes verified%/atlas score; tooltips everywhere; reconcile CLI-vs-server node counts (likely lite-vs-full graph — then label them differently).
13. **C5/D** report: telemetry behind `--stats`, PUBLIC SURFACE cap, archetype-consistent style line, library-aware footer example.

Positives to protect (don't regress while fixing): 29 s eShop cold analysis; Table lens + CSV; kind filter chips; auth badges; approx markers; per-card token meters; MCP host configs + live token feed; LIBRARY ENTRY-API/ABSTRACTIONS; settings honesty; styleguide discipline; actionable error strings (`[not_found] …`).

---

## Appendix — audit mechanics

- Rounds 1–6 scripts: `src/DevContext.App/scripts/audit-drive.mts` … `audit-drive6.mts` (committed on this branch). Server URL injected via `globalThis.__DEVCONTEXT_SERVER__`; clipboard + download captured in-page.
- Round-trip quirks that cost time (useful as UX evidence): scope-picker needs select→**Add to context** (two-step, undiscoverable); preset no-ops silently at 0 selection; no session restore made every round re-analyze; home tiles only exist immediately post-analysis.
- Known-good baseline consulted: `eval-results/2026-07-11/ui-context-studio-audit.md` (code-driven). Confirmed still true live: omitted[] hidden (GAP 2), config/tests stubs (GAP 3), silent error path (GAP 5), `.md` filename (GAP 6). Fixed since: EntryPoint line numbers exist in UI chips (R3), server-assembled markdown used for Copy (Trap A) — though now stale (C1).
- Not verified (candidates for next session): `devcontext-mcp` on PATH from a packaged install; CSV completeness under virtualization; Analysis-defaults actually applied; Tauri-shell-only behaviors; "Run report" tile output; multi-repo tabs.
