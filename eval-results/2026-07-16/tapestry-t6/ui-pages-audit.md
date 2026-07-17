# T6.0 — UI pages audit, shamshir pole (monolith + workers + SignalR)

**Date:** 2026-07-16 · **Branch:** `feat/tapestry-t6` (off tapestry-t5 @ c8ce456)
**Method:** live blind-drive of the real dev stack (server :5179, ng :4200) with
`scripts/audit-drive-shamshir.mts` — analyze `C:\code\shamshir` (TradingEngine: 14-project
Aspire monolith, 130 HTTP + 5 workers + 2 SignalR hubs), sweep all 7 pages, capture fullpage
PNG + innerText + element inventory + per-page RPC counts + keyboard/theme probes. 16/16 steps PASS.
**Counterpart:** the eShop/microservices pole ran 2026-07-15 (`eval-results/2026-07-15/feature-design-audit.md`).
**Artifacts:** `shamshir-shots/*.png` (8 key screens), `atlas-onepager.md` (captured export),
`rpc-by-page.json`, `keyboard-observations.json`; full set in the session scratchpad `audit/shamshir/`.

## Verdict

The monolith pole holds up **better than eShop did** — wiring health 100%, honest per-service
styles (Aspire AppHost / Worker / CLI / Web App), real feature areas (walk-forward, ctrader,
trades, system), deep top flows (21–26 steps). What breaks trust on THIS pole: **every project
is counted twice** (a `.claude/worktrees/` git-worktree copy is walked by discovery → "28
services" on a 14-project repo, NG0955 duplicate-key warnings ×11, one-pager lists every
project twice), the **route-restore bug hijacks `/`** (finding 49 reproduced: navigating home
renders Settings), microservice-flavored copy on a monolith ("28 services", "137 endpoints",
"How services connect" over an edge-less card stack), hub radar is 70% DbContext noise, and the
insights page fires the exact suppression/copy bugs the addendum riders predicted.

## S1 — NEW findings (not in the eShop pole)

| # | Finding | Evidence | Owner |
|---|---------|----------|-------|
| S1.1 | **Discovery walks nested git worktrees**: `C:\code\shamshir\.claude\worktrees\refactor-godclasses-finish\` is a full repo copy; `ExtractionOptions.ExcludePatterns` has no `.claude` → topology/serviceStyles/packages path-keyed rows duplicate ×2 ("28 projects", per-service cards ×2, one-pager ×2, "22 dependents" on 13 possible); id-keyed nodes/entries merge silently. NG0955 warnings are this bug surfacing in the UI. | `atlas-onepager.md` (every project twice); `05-atlas.png`; console-log NG0955 ×11 | **T6.1** (engine) |
| S1.2 | **Route-restore hijacks `/`** (eShop finding 49, now reproduced): tab-strip boot nav sends `/` to the tab's remembered route — after visiting Settings, every fresh load of `/` renders Settings. The keyboard probe's "pressing `e` landed on /context" was this bug, not a nav handler. | `10-home-revisit.png` (Settings at `/`), `keyboard-observations.json` | **T6.5** |
| S1.3 | **Every full page load re-analyzes and re-indexes**: boot auto-analyze (no session reattach) + atlas flow indexer fire `Analyze` + ~100 `GetTrace` + ~40-50 `GetNode` per reload; MCP live feed logged 145 UI-origin calls / **183,193 tok** in one drive. | `rpc-by-page.json`, `07-mcp.png` | **T6.9** (reattach) + T6.10 (feed filter) + T7.4 (budget) |
| S1.4 | **Hub radar is data-plumbing noise**: 7 of 10 rows are `TradingDbContext` + its EF members (SaveChangesAsync/AsNoTracking/Where/FindAsync/Add/FirstOrDefaultAsync); also a title bug `DownloadJobService.List(0)` and "in 269 · out 0" degrees that don't correlate with flow membership. | `05-atlas.png`, one-pager Hub Radar | **T6.1** |
| S1.5 | **Atlas MAP header duplicates its per-service list** inside the raw text wall (the ×2 rows again) and mixes three project counts on one screen: header "28 projects", evidence line "32 projects", diagram "28 projects · 74 dependency edges" — while the hero renders only 4 runnable cards, zero edges. | `05-atlas.png` | T6.1/T6.7 |
| S1.6 | **Monolith wording**: identity strip "137 endpoints · 28 services" (130 HTTP + 5 workers + 2 hubs are not all "endpoints"; a monolith has projects, not services), tile "137 entries across 28 services", hero heading "How services connect" over a single-column stack. | `03-home-analyzed.png` | **T6.1** |
| S1.7 | Top-flow service chips are unreadable: `svcColor()` sets a light background under default chip ink; the project chip renders as a colored blob. | `03-home-analyzed.png` (Top flows rows) | T6.8 |
| S1.8 | Event wiring board empty-state copy lies on a monolith: "No event wiring data — index flows from the Explore page" — indexing HAS run (100 traces); the repo genuinely has no cross-service events. Should say so. | `05-atlas.txt` | T6.1 |
| S1.9 | Entry deck TARGET chip is `TradingDbContext` for 8 of the first 16 rows (the deep data touch, not the handler) — A6 lives on this pole too. | `11-explore-first-entry.png` | T6.8 (label) |
| S1.10 | Insights fires every predicted rider: "ViewModel-View: 0 VMs + 6 Views (0 call edges)" on a trading engine; "Internal hubs: heavily-referenced" including "(1 refs)"; "50% conf"/"70% conf" raw percentages; copy leaks "desktop app's connective tissue", "library's 'heart'", "use `--focus` for deeper traces"; two cards on the same anonymous-endpoints fact; "Extension seats" headlined by framework methods (AddDbContext, AddHostedService). | `06-insights.png` | **T6.3** |

## S2 — Per-page notes (data shown / quality / gaps)

### Home (`03-home-analyzed.png`)
- **Shown:** identity strip (137 endpoints · 28 services · 2.8K types · 113.5s), archetype chip
  App + NLayer · 60%, "How services connect" hero (4 runnable cards: ResearchCli, Web, Host,
  AppHost — correct set, from the T1.4 serviceMap projection), 3 tiles (entries-by-kind
  HTTP 130 / Hosted 5 / SignalR 2 — honest; wiring 100%; freshness Current), 7 top flows (all
  real POST/GET routes), needs-attention insights, START-HERE row.
- **Good:** kind mix is honest, wiring health is real, per-kind tile reads correctly on a
  worker+hub repo, "Trace checkout" tile correctly absent (no checkout entry).
- **Gaps:** S1.1 counts, S1.6 wording, hero draws zero edges (B1), "35% verified" unexplained
  (B6), "NLayer · 60%" raw confidence, `pnpm dev:web → MCP ready` developer leakage (B3),
  service chip contrast (S1.7).

### Explore (`11-explore-first-entry.png`, `12-lens-*.png`)
- **Shown:** deck of 137 entries with kind chips (HTTP 130 / Hosted services 5 / SignalR hubs 2),
  feature-area group paths (walk-forward, ctrader, trades, system — T1.6 works), auth locks,
  method colors; first entry traces deep; 5 lenses render.
- **Good:** deck filter + kind chips + feature areas are the strongest first-contact surface on
  this pole; no MAUI-style noise; hosted services and hubs are first-class rows.
- **Gaps:** TARGET chip = TradingDbContext on half the rows (S1.9); deck order is registration
  order, not wired-and-deep (B2 — milder here since everything is wired); absolute paths in the
  inspector rail (B13, unchanged).

### Atlas (`05-atlas.png`, `atlas-onepager.md`)
- **Shown:** MAP text wall, service diagram (4 cards, "28 projects · 74 dependency edges", zero
  edges), top flows (26/23/23/22/21 steps — real), empty event board, per-service breakdown
  (every card ×2), packages, hub radar.
- **Gaps:** S1.1/S1.4/S1.5/S1.8; one-pager: services ×2, last-segment names (`Host`, `Web`,
  `CTrader` from `split('.').pop()` — A8), hub radar noise verbatim, flows all "0 cross-service"
  (honest here, but the metric renders even when the repo can't cross services).

### Insights (`06-insights.png`)
- S1.10 in full. Ranking is fine on this pole (Warnings first), "Most depended-upon:
  TradingEngine.Domain (22 dependents)" — count inflated by S1.1 (13 max possible dependents).
- "External event contracts: 3 consumed but never produced internally (BarIngested, TradeClosed,
  EquityUpdated)" — plausible-looking but unverified; worth an engine cross-check when T6.3 lands.

### MCP (`07-mcp.png`)
- **Shown:** endpoint active, 3 host configs with copy, sessions table (1 row: C:\code\shamshir,
  handle `16712bea`…, 593 calls), live feed 145 events / 183k tok.
- **Gaps:** handle truncated to 8 chars with no copy/use affordance (B9 — TRY-A-TOOL rejects the
  shown handle); feed is 100% UI-origin traffic (GetTrace ~3228t each) — agent calls would drown
  (B9); `devcontext-mcp` PATH resolution still unverified on a packaged install.

### Context Studio (`14-context-*.png`)
- **Shown:** scope picker groups by project (TradingEngine.Web 136 + Host), presets, T5's
  composition/budget/verification stack.
- **Good:** T5 surfaces intact on the second pole. Preset with 0 selection is disabled (T5.6 fix
  holds).
- **Gap (kind-aware presets, T5.4 rider):** with a WORKER entry selected the preset seeds the
  worker card set — verified in T5; nothing new broken on this pole.

### Settings (`09-settings.png`)
- Appearance renders; **Server group still shows the configured constant** (B12) — not
  re-verified live this round (default tab only); the T6.4 fix will drive it.

### Shell/theme/keyboard probes
- **Light mode (`16-home-light.png`):** the shell (titlebar, tab strip, activity rail,
  statusbar) DOES follow light mode in the Modern vibe — findings 38–39 look fixed for Modern;
  Terminal/Hacker × light remain unverified → the T6.6 matrix stays necessary.
  Note the shot itself shows Settings rendering at `/` (S1.2) and statusbar "atlas 49/100" (B6).
- **Keyboard:** `g`+key nav works; `?` help opens; Ctrl+K omnibox opens. Single-key h/e/a…
  do nothing (the rail's `shortKey` affordance is dead — finding 37); the earlier "e →
  /context" observation was S1.2, not a nav handler.
- **Console:** NG0955 ×11 (S1.1 data), cytoscape wheel-sensitivity ×3, `allowSignalWrites`
  deprecation ×1 (`graph-canvas.ts`).

## S3 — What this changes about the T6 batch

1. **T6.1 gains an engine half**: exclude `.claude` (nested agent worktrees) from discovery —
   the "28 services" lie, NG0955, doubled cards, and inflated dependent counts all collapse to
   one root cause. Shamshir baselines will shift (projects 28→14; node/entry counts barely — ids
   already merged); record in the drift table with the cause named.
2. **T6.5's route-restore fix is not optional** — it corrupts every fresh load after a Settings
   visit, and it invalidated two probes in this very audit.
3. **T6.9 reattach has a measured cost basis**: ~100 GetTrace + ~50 GetNode + full Analyze per
   reload today.
4. Hub radar noise suppression joins T6.1 (data honesty on the Atlas page).
5. eShop-pole findings that REMAIN true on this pole and keep their owners: B1 hero edges
   (T6.7), A8 `split('.').pop()` (T6.8), B6 unexplained metrics (T6.8), B9 MCP handle/feed
   (T6.10), B12 server constant (T6.4), B13 absolute paths (T6.8), insights riders (T6.3).
