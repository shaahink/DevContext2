# Tapestry Audit Addendum — approved 2026-07-15 (merge mode A: woven into the proposal)

> Source: `eval-results/2026-07-15/feature-design-audit.md` (live blind-drive: eShop through every
> GUI page on an isolated stack, token exports captured to disk, CLI `report`/`query` over 8
> benchmark libraries), cross-checked against `docs/dev/briefs/proposal-tapestry.md` and verified
> against code at `feat/tapestry-t1` (3ea6c34 + T1.2/T1.3). This file is the **detailed spec**;
> the proposal carries the woven checkpoint summaries and `docs/dev/archive/trackers/TAPESTRY-START.md` carries the tracker
> rows. Finding IDs (A1…, B1…, C1…, D…) refer to the audit doc.
>
> Rules of engagement are unchanged — R-T1..R-T8 apply to every checkpoint here, especially
> R-T1 (detect ≠ render ≠ serve: every fix lands with its render, MCP visibility, and an eval pin)
> and R-T7 (truth files written from reading the target repo's source, never from output).

## 0. Coverage verdict — what existing Tapestry checkpoints already own

No new items for these; the audit adds evidence and extra gate assertions only:

| Audit finding | Existing checkpoint | Added gate assertion (woven) |
|---|---|---|
| A12 line stamping (Razor `:1`, members bare `:` in packs) | **T2.2** | pack golden: zero `file:` without a number |
| A6 target-title quality (bare titles fixed by T1.3 already) | **T2.3** | deck target chip = handler, labeled when deep-callee |
| A13 Inspector Code shows wrong region (drifts into next handler) | **T2.2** | `read_source` MEMBER returns the member's span |
| B13 absolute paths in packs/MCP | **T3.5** | UI half in T6.8 |
| C2 pack header / budget under-fill / unmarked truncation | **T4.1 / T4.2** | header carries repo+timestamp+HEAD+focus |
| Config/tests stub cards (+ phantom `~10L` estimates) | **T4.3 + T5.5** | — |
| omitted[] hidden · silent RPC failure · save extension | **T5.1** | — |
| Per-card copy · JSON export · provenance chips | **T5.3** | — |
| Insight copy leaks ("desktop app", "library's heart", `--focus` CLI-ism) | **T6.3** | riders below |
| MCP multi-session truth · Settings storage truth | **T6.4** | rider: live server URL |
| Home/Atlas honesty on other archetypes | **T6.1** | — |
| Perf/waterfall honesty | **T7.2 / T7.3** | — |
| T6.0 seven-page UI audit | **half-satisfied** by this audit (eShop pole + microservices class); the shamshir pole still runs |

## 0b. Graph verdict — new graph vs old graph (decision recorded)

**We are already on the new graph. No rewrite. Retirement = cleanup checkpoint T2.8.**

Code truth (verified at 3ea6c34): `Graph2/` is the substrate — `SymbolTable` identity, `BodyFacts`,
seam detectors (`MediatRDispatchDetector`, `BusPublishDetector`, `DomainEventRaiseDetector`,
`EntityTouchDetector`, `PlainCallDetector`, `IntegrationEventCreationDetector`), semantic-lite
tiers, `ServiceBoundaryInference`. It **feeds** the single product graph (`Graph/CodeGraph`)
assembled by `GraphBuilder`. The old regex body-scans (`AddSends`/`AddRaises`/…) are already
deleted — GraphBuilder L2.3 runs `ISeamDetector`s over pre-extracted BodyFacts ("zero regex, zero
re-parsing"). The proposal's non-goal ("no rewrite of Graph2 — extend it, don't reopen it") stands.

What still deserves retirement (all in **T2.8**):
1. `EntryTableProjection.DeriveEntryKind` (`GraphProjections.cs:253-261`) re-derives entry kind
   from `kind:` node tags and silently defaults untagged nodes to `PublicApi`; the Grpc and
   Desktop builders never stamp the tag → root cause of the "gRPC 75" facet lie (A2).
2. GraphBuilder's legacy project-name event joins (`GraphBuilder.cs:~2099-2350`) predate Graph2's
   bus/integration-event seams; T2.6 replaces them with one seam-fed join.
3. Stale comments still cite `AddSends` (`GraphBuilder.cs:2099`); the 121 KB `GraphBuilder.cs`
   splits into assembler modules mechanically (zero behavior diff — drift table proves it).

---

## T1 additions — Detection strength (entry taxonomy)

### T1.7 Entry taxonomy hygiene (A2, A3, A4, A5)

**Evidence:** eShop deck: 4 of 7 "gRPC" rows are BasketService private helpers
(`ThrowNotAuthenticated`, `ThrowBasketDoesNotExist`, `MapToCustomerBasket*`); 96 of 181 entries are
ClientApp MAUI noise (`ViewModel.OnPropertyChanged`, `ViewModel.UpdateLatitude`,
`Animation.FadeOut`, `StoryBoard.BeginAnimation`), with `[RelayCommand]` members ALSO emitted as
plain-method twins and `ViewModel.InitializeAsync` ×5 indistinguishable; Blazor pages (`GET /`,
`/checkout`, `/cart` → `*.razor:1`) carry `kind:HttpEndpoint`; literal duplicates
(`GET /api/catalog/items` ×2 — API-version pair; `POST /Device` ×2 — controller overloads) fire
NG0955 dup-key warnings in four components.

**Verified loci:** detector filling `GrpcServiceDetection.Methods` (the builder
`Graph/EntryPoints/GrpcEntryPointBuilder.cs` is faithful to its input);
`Extractors/Specific/DesktopEntryExtractor.cs` (plain-method emission + `_ => DesktopEntryKind.Window`
fallback at `:150`); the HTTP/razor entry path that stamps Blazor page routes as HTTP.

**Approach:**
1. gRPC: entries only for `public override` methods of the generated service base (proto RPCs).
   Helpers disappear. eShop truth: Basket = 3 RPCs (`GetBasket`, `UpdateBasket`, `DeleteBasket`).
2. MAUI/desktop: keep `[RelayCommand]` members, page/view/window types, app startup; stop emitting
   plain public methods and animation classes; dedupe RelayCommand vs plain-method twin (one entry,
   the RelayCommand form). Review the `_ => Window` fallback — unknown bases are not entries.
3. Blazor page routes: `UiEntry` (route-shaped title is fine), never `HttpEndpoint`. This alone
   recomputes the "49/56 endpoints anonymous" security insight onto the real API surface.
4. Collision disambiguation at the source: when two entries share method+route+kind, suffix the
   title with the discriminator (API version segment, action name, or `file:line`) — data-level,
   so every surface (deck, scope-picker, top-flows, trail) stops rendering twins and NG0955 goes
   silent.

**Gate:** eshop eval pins per-kind counts written from reading eShop source (R-T7 — count the
RPCs/pages/commands in the repo, not in our output); ui-audit-drive console log asserts zero
NG0955; 22-repo bench presence run no regressions.
**Trap:** deduping the API-version pair into one entry — they are two real endpoints; disambiguate,
don't merge. And don't "fix" the security insight by filtering it — fix the kind, the insight
follows.

### T1.8 Kind single-sourcing (A2-counts; retirement item 1)

**Approach:** `EntryTableProjection` joins `snapshot.Entries` by entry node id (builders already
carry the true `EntryPointKind`); delete `DeriveEntryKind` tag-parsing and the `PublicApi` default;
App chips map 1:1 to `EntryPointKind` (no label buckets of convenience).
**Gate:** facet counts == Σ table rows == deck chip counts on eShop + dogfood + shamshir (assert in
a Server test + ui-audit-drive).
**Trap:** leaving the tag path as a "fallback" — that silent default is the bug; if an entry node
has no matching Entry record, that's an assembler error to surface, not a bucket to invent.

### T1.9 Topology noise: tests/samples/benchmarks out (A16, D)

**Evidence:** eShop "19 services" includes 5 test projects as service cards; MediatR
"Most depended-upon: MediatR.Examples (9)" outranks MediatR; Hangfire lists `Hangfire.Core` twice
(5 and 3 — project vs package not deduped); Dapper shows a literal `*` dependency.
**Approach:** classify projects (production / test / sample / benchmark) once — reuse
`NoiseFilter.IsProductionEntrySource` semantics at project level + name/reference heuristics
(xunit/benchmarkdotnet refs); service diagram, services count, most-depended-upon, dead-code, and
hub radar consume only production projects (tests collapse into a count chip or a collapsed lane);
dedupe project-vs-package dependency rows; drop MSBuild wildcard versions from display.
**Gate:** eShop services = 14; MediatR most-depended = MediatR; no duplicate name+different-count
rows anywhere in a report.
**Trap:** excluding by path regex (`tests/`) instead of project classification (R-T2's cousin);
and hiding test projects entirely — collapse, don't erase (they're real code).

### Riders on existing T1 checkpoints

- **T1.4/T1.5 gate additions:** eShop `WebApp` must read as the Blazor storefront (Blazor signal
  must out-rank the YARP proxy package for per-service style), not "Gateway [YARP]";
  MAUI (`ClientApp`/`HybridApp`), workers (`PaymentProcessor`/`OrderProcessor`), Blazor client
  (`WebhookClient`) stop rendering "Unknown — no stack signals detected".
- **T1.5 rider (D):** when the LIBRARY lens renders, the headline style must be
  Library/SampleCollection — never an app-shape label harvested from samples (AutoMapper "NLayer",
  Polly "MinimalApi", MassTransit "Microservices"). MediatR gets the LIBRARY section too
  (SampleCollection and Library coexist: samples render as consumers of the library surface).

---

## T2 additions — Graph quality

### T2.5 Param-passed dispatch seam (A1 — the flagship-flow killer)

**Evidence:** `POST /api/orders/draft` renders `[Entry] → [Call] OrdersApi.CreateOrderDraftAsync`
and stops — in the Flow lens, the graph lens, and every context pack. The engine's deepest flows
(32-step domain-event traces) prove the machinery works once the seam resolves.

**Verified mechanism:** `SeamDetectorHelpers.ResolveArgTarget` (Graph2/Seams/ISeamDetector.cs)
correlates the `Send(x)` argument only with a `LocalDeclOp` in the same body; a **method-parameter**
command (`CreateOrderDraftAsync(CreateOrderDraftCommand command, …) → Send(command)`) matches no
local, `arg.Type` is null without a semantic bind → returns null → **no seam emitted**. Two
supporting gaps: `BodyFacts` carries no parameters (`record BodyFacts(Member, MemberName, Ops)`),
and `MediatRDispatchDetector` name-hints (`mediator`, `sender`, …) miss member-access receivers
(`services.Mediator`).

**Approach (extends Graph2, no rewrite):**
1. `BodyFactExtractor`: record parameter names + declared types on `BodyFacts`
   (`ImmutableArray<ParamFact>`; syntactic tier).
2. `ResolveArgTarget`: after the local scan, fall back to a parameter whose name matches the arg
   text → its declared type (tier: syntactic → the assembler emits an approx edge, honest).
3. `MediatRDispatchDetector`: normalize `ReceiverText` to its last member-access segment before the
   name-hint check (`services.Mediator` → `Mediator` → hint match); semantic receiver bind stays
   authoritative when present.
4. Fixture: add a param-passed-command endpoint to `tests/fixtures/CompositionApp` (mirrors the
   OrdersApi shape per R-T3: `[AsParameters]` services record + `Send(command)`).

**Gate:** CompositionApp truth: the param-passed endpoint traces ≥3 hops (entry → handler →
domain callee); eShop `/draft` and `POST /api/orders/` reach `CreateOrderDraftCommandHandler`;
22-repo bench: Sends edge counts move only upward; drift table updated (expect eShop edges +N,
explained).
**Trap:** resolving the parameter by TYPE anywhere in the body instead of by NAME (over-matching);
and stamping the fallback as verified — it is syntactic/approx unless semantic-lite bound it.

### T2.6 One event join to rule them all (A10; retirement item 2)

**Evidence:** eShop is RabbitMQ integration events end-to-end, yet: Atlas event board = 1 row (a
domain event, approx); one-pager says "0 cross-service" for all five top flows; the pack's DELETE
flow shows `[CrossService] Webhooks.API / Ordering.API / Basket.API / WebApp` dangling under an
`[approx]` param-record node. Three surfaces, three stories.
**Approach:** one projection joins publisher→event→consumer from Graph2 seams
(`BusPublishDetector` + `IntegrationEventCreationDetector` + consumer entries) — powers (a) the
Atlas event board, (b) the one-pager Event Wiring section, (c) flow CrossService markers (attached
at the PUBLISHING member, not an unrelated approx node). Delete the legacy project-name joins in
`GraphBuilder.cs:~2099-2350`.
**Gate:** eshop eval pins ≥8 integration-event rows (write the list from eShop source: OrderStatusChanged*
family, OrderStarted, ProductPriceChanged, GracePeriodConfirmed…); a unit test asserts board,
one-pager, and flow markers render from the same projection output; cross-service counts agree.
**Trap:** joining on event NAME text — join on the model-derived event type sets (the anti-overfit
rule from the go-to audit); and keeping the legacy join "for safety" — that's the contradiction
machine.

### T2.7 `global` never reaches the user (A7)

**Evidence:** "global" as service/group label on Ordering.API rows, hub radar
(`global.CatalogServices`), module map ("global (7 entries)"), export IDs
(`Member:global.OrdersApi…`). Verified origin: `SyntaxStructureExtractor.cs:134` (`?? "global"`) —
and eShop's `OrdersApi` genuinely has no namespace declaration, so this is a **display** problem.
**Approach:** display fallback chain namespace → project name → top folder, applied at the
grouping/render layer (NameResolver consumers), not by faking the extracted namespace. Symbol IDs
keep `global.` internally if needed but never render it.
**Gate:** grep gate over rendered Map/report/pack outputs on the eval fleet: zero standalone
"global" group labels.

### T2.8 Old-graph retirement cleanup (§0b; mechanical)

Items 1–3 from §0b. **Gate:** no `kind:` tag parsing; no `AddSends` mentions; GraphBuilder split
compiles with the drift table byte-identical on dogfood (`analyze --no-cache`).
**Trap:** sneaking behavior changes into the split — the split commit must show an unchanged drift
row, or it's not the split commit.

---

## T3 additions — agent/CLI surface

### T3.7 CLI query ops actually work (A15)

**Evidence + verified locus:** `Cli/Commands/QueryCommand.cs` routes only
`node|neighbors|usages|search`; `entrypoints|map|trace|stats` all fall through to one overview
render (`scenario "overview"` → `RenderAsync`) — `query entrypoints` and `query stats` return the
map payload (captured byte-similar JSON), `query trace` ignores `--focus/--depth`.
**Approach:** implement the three missing ops against the snapshot the command already has:
`entrypoints` → Entries list (kind, method, route, target, `file:line`, per-kind counts — same
shape as the MCP tool); `stats` → GraphStats + per-kind counts (the payload bench prints since
T0.3); `trace` → `GraphQuery.Trace(focus, depth)` with the markdown/json envelope; `map` stays the
render. Share JSON envelopes with MCP (kernel JSON — one shape, two transports).
**Gate:** `eval/gates.ps1` CLI matrix extended with one `--strict` assertion per op (entrypoints
returns >0 entries with kinds on TodoApi; trace honors a focus; stats has per-kind counts).
**Trap:** inventing a new JSON shape — reuse the MCP/kernel serialization or the surfaces drift
again (A14).

### T3.8 Report as a product artifact (C5, D)

**Evidence:** MassTransit report = 476 KB (PUBLIC SURFACE enumerates 4,531 types); run-report
telemetry (extractor timing tables) embedded in a user-facing doc; MediatR footer suggests
`--focus "POST /api/orders/"` (an eShop route) on every repo.
**Approach:** telemetry renders only under `--stats`; PUBLIC SURFACE capped (top-N per namespace +
"… N more — `--format json` for the full surface"); footer drill-in example derived from the
repo's own top entry; reconcile the two "public types" numbers (Insights counts all projects,
LIBRARY counts the library surface — label both or drop one; A14).
**Gate:** MassTransit report < 40 KB; no `Run Report` section without `--stats`; footer example
resolves against the repo's own entries on all 8 library repos.

---

## T4/T5 additions — packs & Studio

### T4.6 Pack assembly correctness (C2a; complements T4.1/T4.2)

**Evidence (captured export):** "Contracts and interfaces" card is a verbatim duplicate of
"Member signatures" (597 tok twice; the UI even labels the contracts card "signatures: 597 tok");
`_Archetype: _` empty; "Entities — 0 tok" empty section still emitted; `<!-- context card -->`
HTML comments shipped in the copy surface.
**Approach:** `ContextPackBuilder`: contracts section selects interfaces/DTOs/message contracts
from the traced spine (distinct from signatures); empty sections are omitted AND recorded in
`omitted[]`; archetype header filled from the snapshot; HTML comments dropped from the human copy
(machine markers only in the JSON export, T5.3).
**Gate:** pack golden on CompositionApp asserts signatures ≠ contracts content, no empty sections,
non-empty archetype.

### T5.6 Studio recompute-on-change (C1 — the token-export trust bug)

**Evidence + verified locus:** captured live — compose at 4000 → Copy; slider to 1000 (meter goes
red "~2189 / 1000 tok — Over budget"); Copy again; switch plain; Copy; Save: **all four artifacts
byte-identical**, header still "Budget: 4000". `context-studio.ts`: `loadAllCards` (the only
`getContextPack` caller) runs only when cards are added; `budgetTokens` is a signal nothing
observes for re-fetch; `onCopy` serves the frozen `serverPackMarkdown`.
**Approach:** budget/intent changes re-pack (debounced ~500ms) — or, if re-pack is too chatty,
flip a visible "stale — Re-pack" state that disables Copy/Save until refreshed; verify
plain-vs-markdown actually differ (the plain strip exists in `buildContext` — assert bytes);
Save filename `${repoName}-context-${yyyyMMdd}.{md|txt}`; preset "I'm changing this endpoint"
disabled at 0 selection with a hint tooltip; the select→"Add to context" two-step gets a
count-badged primary button (the audit lost three drive rounds to it; users will too).
**Gate:** ui-audit-drive: copy@4k ≠ copy@1k (bytes), plain ≠ markdown (bytes), header budget
matches the slider, preset disabled-state + stale-state screenshots committed.
**Trap:** re-packing on every slider tick (debounce, or explicit apply); and fixing Copy while
Save still reads the stale string — one code path for both (`buildContext`).

---

## T6 additions — pages

### T6.7 Hero graphs draw edges (B1)

**Evidence:** home "How services connect" and Atlas "Service diagram — 19 projects · 36 dependency
edges" both render a single-column card stack with **zero edges**; the Workbench Service lens
renders the same data as a real cytoscape graph — the renderer works, the hero component doesn't
use it. Atlas MAP header is a raw text wall (CLI blob incl. duplicated MAUI TFM strings).
**Approach:** `service-map-hero` reuses the Service-lens cytoscape (smaller, non-interactive
config); tests collapse into a lane (T1.9 data); Atlas MAP header decomposes into structured rows/
chips with TFMs deduped and humanized ("net10.0 + MAUI targets").
**Gate:** screenshot pair in the drive gate: eShop hero shows ≥1 edge; no raw `;`-joined TFM string
anywhere in the DOM.

### T6.8 Names, paths, metric meaning (A8, A14, B5, B6, B13-UI)

**Evidence + verified loci:** `atlas-page.ts:224-225` (one-pager!), `service-cards.ts:47`,
`home-page.ts:131` all do `name.split('.').pop()` → Basket/Catalog/Identity/Ordering/Webhooks .API
all render "API", and the one-pager exports "AppHost → API, API, API…". Deck truncates 15 Catalog
rows into identical "GET /api/c…" strings. Metrics with no definition anywhere: "36% verified",
"atlas 84/100" (reads 76 on another page), flow "0%", Table SHARED column. Absolute machine paths
in Details rail, Call Stack, Table RESOLUTION.
**Approach:** display names strip only the common solution prefix (else full name — never last
segment); deck middle-ellipsis keeps the distinguishing tail + collision disambiguator (T1.7 data);
one computation source per metric with a tooltip definition (verified% and the atlas score defined
in one place, or dropped if not explainable in a sentence); repo-relative paths across Details/
CallStack/Table with copy-absolute affordance; reconcile CLI-vs-server node counts (1137/886 vs
1156/904 — likely lite-vs-full graph; if intentional, label them as different graphs).
**Gate:** one-pager renders full service names; grep gate: no `split('.').pop()` on service/project
names in App; every visible metric chip has a `title`/tooltip; drive dumps show zero `C:\` paths.

### T6.9 First-run & session (B2, B3, B4)

**Evidence:** deck default order starts at unwired Blazor `GET /` (one-node trace, "0%");
"Trace checkout" START-HERE tile lands on `GET /checkout` (one-node dead end); tiles exist only on
the freshly-analyzed home (verified count 1 → 0 on revisit); "Point your agent here:
`pnpm dev:web` → MCP ready" is DevContext-developer leakage; every new browser context starts
over (no session reattach — server sessions survive and are listable).
**Approach:** deck default sort = wired-and-deep first (unwired UI routes sink); "Trace checkout"
resolves via flow search to the deepest matching flow (`CheckoutViewModel.CheckoutAsync` /
`POST /api/orders/`), never an unwired route; START-HERE persists (post-analysis home and revisit
render the same digest); agent tile links to the MCP page (which already has correct host configs);
client adopts the server's latest session for the repo on boot (the MCP page's sessions RPC is the
server half — client reattach + a "resume last repo" affordance).
**Gate:** ui-audit-drive: fresh context reattaches without re-analyze; tiles present on revisit;
"Trace checkout" lands on a ≥3-hop flow.

### T6.10 MCP page ergonomics (B9)

**Evidence + verified locus:** `mcp-page.ts:142` renders `s.handle.slice(0, 8)`; TRY-A-TOOL
requires typing a handle and rejects the truncated one the page itself shows
(`Error: [not_found] Unknown session handle: a08195c3`); the live feed logged 163 calls / ~99 k tok
of the UI's own gRPC traffic on one page load — agent traffic would drown.
**Approach:** sessions table gets full-handle copy + a "use" button that prefills TRY-A-TOOL (zero
typing); feed rows tagged by origin (UI vs MCP/stdio) with default filter = agents; verify the
host-config `command` (`devcontext-mcp`) resolves for the packaged install and show a hint when it
doesn't.
**Gate:** drive: try-a-tool succeeds via the button with zero typing; feed filtered by default
(UI rows hidden) — screenshot.

### T6.11 One-pager fidelity (C3 — rider on T2.6 + T6.8)

Clipboard export stays; add a file download; content inherits T6.8 names + T2.6 event wiring +
honest cross-service counts. **Gate:** exported one-pager golden on eShop: full service names,
≥8 event rows, cross-service counts consistent with the flow cards.

### Riders on existing T6 checkpoints

- **T6.3:** confidence percentages become tier words (or disappear); ranking puts high-tier first
  (the audit saw "12% conf" Warnings ranked top); "missing validation" counts writes only;
  dead-code suppresses convention-instantiated shapes (EF `IEntityTypeConfiguration`, DI extension
  classes); "internal hubs" needs a ≥3-refs floor; ViewModel-View self-suppresses when either side
  is 0 or edges are 0 (fires on Polly/Hangfire/FluentValidation today).
- **T6.4:** Settings→Server shows the LIVE `serverBaseUrl()` + health target, not the 5179
  constant (verified wrong when injected to :5279).

---

## T7 addition

### T7.4 Page-render RPC budget (B11)

**Evidence:** rendering home+atlas fired ~150 `GetTrace` + dozens of `GetNode` in ~2 s (visible in
the MCP feed; 8 aborted `GetTrace` on navigation).
**Approach:** server-side session memo for flows/facets (or a `top_flows` RPC the pages share);
ui-audit-drive asserts an RPC budget per page (≤15 per navigation as a starting bar).
**Gate:** drive RPC counter green; MCP feed on a fresh page load shows <20 UI-origin calls.

---

## Sizing & insertion order

| Block | Sessions | Insertion |
|---|---|---|
| T1.7–T1.9 (+T1.4/T1.5 riders) | 2 | with/right after T1.1 (same territory) |
| T2.5–T2.8 (+T2.2/T2.3 riders) | 2–3 | T2.5 FIRST in T2 (value unlock), then T2.2… existing order |
| T3.7–T3.8 | 1 | anytime; T3.7 unblocks scriptable audits |
| T4.6 + T5.6 | 1–2 | T4.6 with T4.1; T5.6 in the T5.1 session |
| T6.7–T6.11 (+T6.3/T6.4 riders) | 2–3 | in the T6 batch; T6.0 shamshir half still first |
| T7.4 | 0.5 | with T7.2 |

Total ≈ 9–12 sessions on top of the existing waterfall.
