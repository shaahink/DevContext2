# DRAFT — Tapestry Audit Addendum (2026-07-15, awaiting approval)

> Source: `eval-results/2026-07-15/feature-design-audit.md` (live blind-drive, eShop GUI + 8-library
> CLI) cross-checked against `docs/dev/briefs/proposal-tapestry.md` and **verified against code at
> `feat/tapestry-t1` @ 3ea6c34** (includes T1.2/T1.3). Every item below names its verified locus.
> Status: DRAFT — not merged into proposal-tapestry.md until approved. Two merge modes at the end.

## 0. Coverage verdict — what Tapestry already owns

Covered by existing checkpoints (no new items; audit adds evidence/assertions only):

| Audit finding | Existing checkpoint |
|---|---|
| A12 line stamping (Razor `:1`, member bare `:`) | **T2.2** (exactly this) |
| A6 target quality (partial: bare titles fixed by T1.3) | **T2.3** |
| B13 absolute paths in packs/MCP | **T3.5** (UI half added below, T6.8) |
| C2 pack header/budget-fill/truncation markers | **T4.1 / T4.2** |
| Config+tests stub cards | **T4.3 + T5.5** (phantom `~10L` explicitly named in T5.5) |
| omitted[] hidden · silent RPC error · save extension | **T5.1** |
| Per-card copy · JSON export · provenance chips | **T5.3** |
| Insight noise/copy (desktop/library copy leaks, GET-counted validators) | **T6.3** |
| MCP multi-session truth · Settings storage truth | **T6.4** |
| Home/Atlas honesty on other archetypes | **T6.1** |
| Perf/waterfall honesty | **T7.2 / T7.3** |
| T6.0 UI audit artifact | **half-satisfied** by this audit (eShop pole); shamshir pole remains |

Not covered anywhere → new checkpoints below: entry taxonomy (A2–A5), param-passed dispatch (A1),
event-board join (A10), service-name truncation (A8), `global` display (A7), stale Studio exports
(C1), pack card duplication (C2a), CLI query wiring (A15), MCP-page handle loop (B9), hero edges
(B1), first-run + session restore (B2–B4), metric definitions (A14/B6), test-project topology
pollution (A16), library lens (A17/D), report hygiene (C5), one-pager fidelity (C3).

## 0b. Graph verdict — new graph vs old graph

**We are already on the new graph. Do not rewrite; finish the retirement as cleanup.**

Code truth (verified): `Graph2/` is the substrate — SymbolTable identity, BodyFacts, seam detectors
(MediatR/Bus/DomainEvent/EntityTouch/PlainCall), semantic-lite tiers. It **feeds** the single
product graph (`Graph/CodeGraph`) assembled by `GraphBuilder`. The old regex body-scans
(`AddSends`, `AddRaises`, …) are already deleted — GraphBuilder L2.3 runs `ISeamDetector`s instead
("zero regex, zero re-parsing"). The plan's non-goal ("no rewrite of Graph2 — extend it") is right.

What "retire the old graph" still means, concretely (new **T2.8**):
1. **Single-source entry kinds** — `EntryTableProjection.DeriveEntryKind` re-derives kind from
   `kind:` node tags and silently defaults untagged nodes to `PublicApi`
   (`GraphProjections.cs:253-261`); Grpc/Desktop builders never stamp the tag. Join
   `snapshot.Entries` (which carries the true `EntryPointKind`) instead. This is the root of the
   "gRPC 75" lie.
2. **Port legacy event joins** — GraphBuilder's project-name-based ServiceLink/event joins
   (`GraphBuilder.cs:~2099-2350`) predate Graph2's `BusPublishDetector`/
   `IntegrationEventCreationDetector`; one seam-fed join should power the event board, one-pager,
   and flow CrossService placement (see T2.6).
3. **Delete stale references** — comments still cite `AddSends` (`GraphBuilder.cs:2099`); the
   121 KB `GraphBuilder.cs` splits into assembler modules mechanically (no behavior change).

---

## New checkpoints, grouped by Tapestry section

### T1 additions — Detection strength (entry taxonomy)

**T1.7 Entry taxonomy hygiene** *(new — the audit's biggest trust item after A1)*
- gRPC: only proto RPC methods are entries. Locus: the detector filling
  `GrpcServiceDetection.Methods` (builder `GrpcEntryPointBuilder` is faithful to its input) —
  gate on `public override` of the generated service base. Today `ThrowNotAuthenticated`,
  `MapToCustomerBasket*` are gRPC entries (4 of 7 Basket rows are helpers).
- MAUI: `DesktopEntryExtractor` emits plain ViewModel methods (`OnPropertyChanged`,
  `UpdateLatitude`), animations (`StoryBoard`, `FadeOutAnimation` — note the `_ => Window`
  fallback at `:150`), and double-emits `[RelayCommand]` members. Keep: RelayCommands, pages/views,
  app startup. Drop: plain methods, animation classes; dedupe against RelayCommand.
- Blazor pages: route-shaped UI entries carry a UI kind, not `kind:HttpEndpoint` (locus:
  HTTP/razor entry path). This alone fixes the "49/56 anonymous endpoints" security insight.
- Duplicate disambiguation: identical title collisions (API-version pairs `GET /api/catalog/items`
  ×2, controller overloads `POST /Device` ×2) get a version/action/file:line disambiguator in the
  title or subtitle — data-level, so NG0955 dup-key warnings disappear.
- **Expected:** eShop 181 → ≈95–105 honest entries; ClientApp share drops from 53% to
  RelayCommands+views; per-kind chips truthful; security insights recompute on the real API
  surface. **Gate:** eshop eval pins per-kind counts written from reading eShop source (R-T7);
  NG0955 absent in ui-audit-drive console log.

**T1.8 Kind single-sourcing** *(new; = Graph-retirement item 1)*
`EntryTableProjection` joins `snapshot.Entries`; delete the tag-parse + `PublicApi` default; App
chips map 1:1 to `EntryPointKind`. **Expected:** chip counts always sum to total and match rows
("gRPC 75" class of bug becomes impossible). **Gate:** facet counts == Σ table rows on eShop +
dogfood + shamshir.

**T1.9 Topology noise: tests/samples out** *(new)*
Service diagram, "19 services", most-depended-upon, dead-code exclude test/sample/benchmark
projects (reuse `NoiseFilter.IsProductionEntrySource` semantics at project level); dedupe
project-vs-package rows (`Hangfire.Core` ×2); drop MSBuild wildcard `*` deps. **Expected:** eShop
19→14 services; MediatR most-depended = MediatR, not MediatR.Examples. **Gate:** eval pins + bench
presence run.

*(A9 rider: add to **T1.4/T1.5** gates — eShop WebApp must read Blazor storefront, not
"Gateway [YARP]"; MAUI/worker/CLI per-service archetypes stop rendering "Unknown".)*

### T2 additions — Graph quality

**T2.5 Param-passed dispatch seam** *(new — audit A1, the flagship-flow killer)*
Verified mechanism: `SeamDetectorHelpers.ResolveArgTarget` only correlates the arg with a
`LocalDeclOp`; a method-parameter command (`CreateOrderDraftAsync(CreateOrderDraftCommand command…)
→ Send(command)`) resolves to null → no seam → trace ends at hop 1. Also `BodyFacts` carries no
parameters (`record BodyFacts(Member, MemberName, Ops)`), and `MediatRDispatchDetector` name-hints
miss member-access receivers (`services.Mediator`).
Fix: (a) `BodyFactExtractor` records parameter names/types on `BodyFacts`; (b) `ResolveArgTarget`
falls back to the parameter's declared type (syntactic tier); (c) receiver normalization takes the
last member-access segment before the name-hint check. Extends Graph2 — no rewrite.
**Expected:** eShop `/draft` and `/` (create order) trace Entry → handler → domain (≥3 hops);
every MediatR-style repo gains real traces; context-pack flow cards stop shipping stubs.
**Gate:** new fixture method (param-passed command) in CompositionApp + truth: `/draft` trace ≥3
hops; 22-repo bench edge counts move only upward on Sends.

**T2.6 One event join to rule them all** *(new — audit A10; = Graph-retirement item 2)*
Sends×Consumes (bus + integration events) joined once from Graph2 seams; powers Atlas event board,
one-pager Event Wiring, flow CrossService markers. Kill the legacy project-name joins
(`GraphBuilder.cs:~2099-2350`) and the contradiction set {board=1 row, one-pager=0 cross-service,
flow card=4 misplaced hops}. **Expected:** eShop board shows the RabbitMQ backbone
(OrderStatusChanged* publisher→event→consumer rows); cross-service counts agree on all three
surfaces. **Gate:** eshop eval pins ≥N integration-event rows; the three surfaces render from one
projection (assert equality in a unit test).

**T2.7 `global` never reaches the user** *(new — audit A7)*
Display fallback namespace→project→folder (verified origin: `SyntaxStructureExtractor.cs:134`
`?? "global"`; eShop's `OrdersApi` genuinely has no namespace). Applies to group chips, module map,
hub radar, export symbol IDs. **Expected:** "Ordering.API" instead of "global" everywhere.

**T2.8 Old-graph retirement cleanup** *(new — see §0b; mechanical, low risk)*
Items 1–3 from the graph verdict. **Gate:** no `kind:` tag parsing left; no `AddSends` mentions;
GraphBuilder split compiles with zero behavior diff (drift table unchanged).

*(A13 rider: add to **T2.2** gate — Inspector Code/`read_source` MEMBER mode returns the target
member's span, not a file window that drifts into the next handler.)*

### T3 additions — agent/CLI surface

**T3.7 CLI query ops actually work** *(new — audit A15; verified: `QueryCommand.cs` routes only
`node|neighbors|usages|search`; `entrypoints|map|trace|stats` all fall through to the overview
render, `trace` ignoring `--focus`)*
Implement: `entrypoints` → Entries list (kind, method, route, target, file:line, per-kind counts);
`stats` → GraphStats + per-kind counts (same payload the bench prints since T0.3); `trace` →
`GraphQuery.Trace` honoring `--focus/--depth`; share the JSON envelope with MCP (kernel JSON).
**Expected:** CLI/MCP parity; future audits can count kinds scriptably. **Gate:** `--strict` matrix
extended with one assertion per op.

**T3.8 Report as a product artifact** *(new — audit C5/D)*
Run-report telemetry only under `--stats`; PUBLIC SURFACE capped (top-N + "…N more, use
`--format json`"); footer drill-in example derived from the repo's own entries; style line
consistent with the LIBRARY lens (see T-Lib). **Expected:** MassTransit report 476 KB → <40 KB and
readable; no eShop routes in MediatR's footer.

### T4/T5 additions — packs & Studio

**T4.6 Pack assembly correctness** *(new — audit C2a; complements T4.1/T4.2)*
`ContextPackBuilder`: contracts card gets its own section (today it duplicates signatures — the UI
even labels it "signatures: 597 tok"); empty sections omitted (with an `omitted[]` entry);
archetype header filled; strip `<!-- context card -->` comments from the copy surface (keep in a
machine block if needed). **Gate:** pack golden on CompositionApp asserts distinct
signatures/contracts content + no empty sections.

**T5.6 Studio recompute-on-change** *(new — audit C1; verified: `context-studio.ts` calls
`getContextPack` only when cards are added; `budgetTokens` changes never re-fetch; Copy serves the
frozen `serverPackMarkdown`; captured 4k/1k/plain exports byte-identical)*
Budget/intent/format changes re-pack (debounced) or flip a visible "stale — Re-pack" state that
disables Copy/Save; verify plain-vs-markdown actually differ (byte assert in ui-audit-drive);
Save name `${repo}-context-${date}.{md|txt}`; preset disabled at 0 selection with hint; the
select→"Add to context" step gets a count-badged primary affordance. **Gate:** ui-audit-drive:
copy@4k ≠ copy@1k; plain ≠ markdown; preset disabled-state screenshot.

### T6 additions — pages

**T6.7 Hero graphs draw edges** *(new — audit B1)* Reuse the Service-lens cytoscape in
`service-map-hero` (home + atlas); tests collapsed into a lane (needs T1.9). **Gate:** screenshot
diff shows ≥1 edge on eShop hero; Atlas MAP header decomposed into chips (TFMs deduped/humanized).

**T6.8 Names, paths, metric meaning** *(new — audit A8/A14/B5/B6/B13-UI)*
Kill `split('.').pop()` display names (verified: `atlas-page.ts:224-225`, `service-cards.ts:47`,
`home-page.ts:131`) — strip only the common solution prefix; repo-relative paths in Details rail /
Call Stack / Table RESOLUTION (UI half of T3.5); deck middle-truncation keeping the distinguishing
tail; every metric chip ("N% verified", "atlas N/100", SHARED, flow "0%") gets a tooltip definition
sourced from ONE computation; reconcile CLI-vs-server node counts or label the graphs differently.
**Gate:** one-pager renders full service names; grep gate: no `split('.').pop()` on service names.

**T6.9 First-run & session** *(new — audit B2/B3/B4)*
Deck default sort = wired+deep first; "Trace checkout" resolves to the deepest matching flow (never
an unwired UI route); START-HERE tiles persistent (or an explicit empty-state home section);
replace "pnpm dev:web → MCP ready" with a link to the MCP page; session reattach on boot (client
adopts the server's latest session for the repo — server half exists, `ListSessions` powers the
MCP page already). **Gate:** ui-audit-drive: fresh context reattaches without re-analyze; tiles
present on revisit.

**T6.10 MCP page ergonomics** *(new — audit B9; verified `mcp-page.ts:142` truncates the handle)*
Full-handle copy + "use this session" prefill for try-a-tool; tag live-feed rows by origin (UI vs
MCP agent) with a default filter — today 163 UI calls / ~99 k tok drown agent traffic; validate the
host-config `command` resolves. **Gate:** try-a-tool succeeds via the button with zero typing.

**T6.11 One-pager fidelity** *(rider on T2.6 + T6.8)* plus a download option next to clipboard.

*(T6.3 rider: confidence percentages become tier words or disappear; ranked ordering puts
high-tier first — the audit saw "12% conf" Warnings on top. T6.4 rider: Settings→Server shows the
LIVE `serverBaseUrl()`, not the 5179 constant — verified wrong when injected to :5279.)*

### T7 additions

**T7.4 Page-render RPC budget** *(new — audit B11)* Home/Atlas render fired ~150 `GetTrace` calls;
server-side flow/facet caching (or session memo) + ui-audit-drive asserts ≤N RPCs per page.

### T-Lib — Library lens (decision needed: small rider now, big investment later)

Minimal now (riders): style-vs-LIBRARY arbitration (when the LIBRARY view renders, the style line
says Library/SampleCollection — never NLayer/MinimalApi/Microservices from sample noise) →
**T1.5**; ViewModel-View + hub insights self-suppress at 0-edges/0-side → **T6.3**; sample/test
exclusion → **T1.9**; surface cap → **T3.8**. MediatR gets the LIBRARY section (SampleCollection
AND Library can coexist: samples shown as consumers).
Full "library HEART" program (go-to V3) stays deferred unless prioritized.

---

## Sizing & order (if merged)

| Block | Items | Sessions | Depends on |
|---|---|---|---|
| T1.7–T1.9 (+A9 riders) | taxonomy, kind single-source, topology noise | 2 | T0 (done) |
| T2.5–T2.8 (+A13 rider) | dispatch seam, event join, global, retirement | 2–3 | T0 |
| T3.7–T3.8 | CLI query ops, report hygiene | 1 | — |
| T4.6 + T5.6 | pack correctness, Studio recompute | 1–2 | T4.1/T4.2 order |
| T6.7–T6.11 (+T6.3/T6.4 riders) | heroes, names/metrics, first-run, MCP page | 2–3 | T1.7–T1.9 for data |
| T7.4 | RPC budget | 0.5 | — |

Recommended insertion order relative to the existing waterfall: T1.7/T1.8 land **with or right
after T1.1** (same detection territory); T2.5 lands **first in T2** (it is the value unlock);
T5.6 folds into the T5.1 session; T6.7–T6.11 join the existing T6 batch (T6.0's shamshir half
still runs first).

## Merge modes (pick one)

**Option A — weave (recommended):** append these checkpoints into `proposal-tapestry.md` under
their T-sections + add rows to `TAPESTRY-START.md` tables; riders become extra gate lines on
existing checkpoints. Keeps one waterfall, one tracker.

**Option B — append:** keep this file as a standalone addendum with its own tracker section
("TA-checkpoints") executed between T1 and T6. Cleaner diff, but two lists to keep green.

Either way: the audit doc + this addendum live on `audit/ui-feature-design`; merging the branch
brings both; the proposal edit happens on the integration branch after approval.
