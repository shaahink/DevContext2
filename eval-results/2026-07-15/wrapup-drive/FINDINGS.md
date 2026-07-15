# Wrap-up session findings — 2026-07-15 (drive evidence index)

One line per finding; F=fixed this session (commits `99acf40`, `202c593`), T#=Tapestry stage owner.
Artifacts in this directory unless noted.

## Engine / detection (driving `C:\code\shamshir`, 14-project Aspire/worker/SignalR repo)

| # | Finding | Status | Evidence |
|---|---------|--------|----------|
| 1 | Controller verb-attribute route templates (`[HttpGet("packs/{id}")]`) never read → duplicate truncated routes sharing one wrong target | **F** | shamshir-map.md vs -v3.md |
| 2 | Workers/MapHub/middleware in extension-method files invisible (Program.cs-only walk) | **F** | map v1 (no Background section) vs v3 (Background 4) |
| 3 | Built-in SignalR undetected (package-gated signal; shared-framework apps have no package) | **F** | map v1 vs v3 (SignalR 2) |
| 4 | Aspire SDK-style AppHost undetected (`Aspire.AppHost.Sdk/13.3.5` is an SDK, not a package) | **F** | shamshir-stats.txt "skipped: needs aspire" |
| 5 | Microservices-by-project-count ("19 service projects" for a 2-runnable orchestrated monolith) | **F** (counts AppHost ProjectReferences) | map v2 vs v3 |
| 6 | Factory-lambda worker registrations render raw lambda text | **F** | map v2 line "GetRequiredService<EngineWorker>()" vs v3 |
| 7 | `service.ToString()` becomes entry target | **F** | map v2 "CTraderListenService.ToString" vs v3 |
| 8 | Hub trace depth-1; worker trace 13.6k tok (Type-anchored entries) | **F** (member anchors) | shamshir-trace-runhub-v2 vs v3, -engineworker-v2 |
| 9 | Style "MinimalApi (moderate)" for controllers-heavy layered app | T1.5 | map v3 |
| 10 | Per-service list: 2 of ~5 runnables; ResearchCli "Unknown"; AppHost listed as service | T1.4 | map v3, ui-shamshir/01-home.png |
| 11 | Feature map collapses to "Api (122 entries)" | T1.6 | shamshir-stats.txt insight |
| 12 | DI Resolves provenance cites a TEST file registration (SqliteTradeRepository ← InProcessEngineSmokeTests.cs:89) | T2.1 | shamshir-trace-orchestrator.md |
| 13 | ~26s of 51s wall time unattributed in stage waterfall | T7.3 | shamshir-stats.txt |
| 14 | XML-doc `<see cref>` stripped → mangled library summaries ("Adds the and related services to the and…") | T6.3/renderer | grpc-map.md ENTRY API |
| 15 | Insight noise: "ViewModel-View: 0 VMs + 6 Views" on a non-MVVM web app | T6.3 | shamshir-stats.txt |

## MCP (cold QA + 12-question qualitative drive, both vs shamshir)

| # | Finding | Status | Evidence |
|---|---------|--------|----------|
| 16 | Cold-agent actionability 11/11 (100%), gate PASS on unseen repo | verified good | mcp-cold-qa.md |
| 17 | Parameter names inconsistent: `focus` vs `nodeId` vs `query` — 3 naive calls wasted | T3.1 | transcript Q8/Q11/Q12 |
| 18 | `entrypoints` = ~10k tok (128 entries dumped) | T3.2 | transcript Q2 |
| 19 | `trace(EngineWorker)` = ~13.6k tok (bounded now by anchor fix, still uncapped by tokens) | T3.3 | transcript Q4 |
| 20 | `config` = 10.5s (every other query ≤320ms) | T3.4 | transcript Q10 |
| 21 | Absolute machine paths in outputs; overview "Start here: List, TradingDbContext" noise | T3.5 | transcript Q1/Q7 |
| 22 | top_flows targets are bare member names ("RunAsync"); groupPath all "Api" | T2.3/T1.6 | transcript Q6 |

## Context generation (get_context Q7 + Studio audit cross-check)

| # | Finding | Status | Evidence |
|---|---------|--------|----------|
| 23 | Member locations render `file:` with NO line number (only the entry has one) | T2.2/T4.1 | transcript Q7 |
| 24 | Budget under-fill: 612 of 4000 tokens used while bodies are 3-line salient cuts | T4.2 | transcript Q7 |
| 25 | Pack header is an empty `# ` (no repo name/timestamp/HEAD) | T4.1 | transcript Q7 |
| 26 | config/tests cards still dead stubs in Studio (audit R9 unchanged) | T4.3/T5 | 2026-07-11 audit §3 GAP-3 |

## UI (drive gate + 9-screenshot shamshir tour, `ui-shamshir/`)

| # | Finding | Status | Evidence |
|---|---------|--------|----------|
| 27 | UI drive gate 4/4 PASS (tab strip 32px, New preserves tabs, code pane 874 chars, preset 5 cards) — Loom L6 claims verified live | verified good | eval-results/2026-07-15/ui/ |
| 28 | Home hero: 3 stacked boxes (AppHost/Web/ResearchCli), no Host/CTraderRunner, "14 services" = project count | T1.4→T6.1 | 01-home.png |
| 29 | Entries-by-kind tile + entry-deck kind chips correctly show HTTP 122 · Hosted 4 · SignalR 2 (session fixes visible end-to-end) | verified good | 01-home.png, 02-explore.png |
| 30 | Entry deck: aggressive route truncation (4 indistinguishable `/api/walk-forward…` rows) + redundant "Api" chip on every row | T6.2/T1.6 | 02-explore.png |
| 31 | Atlas: MAP text renders as flattened wall; service diagram = same 3 boxes; event wiring empty | T6.1 | 03-atlas.png |
| 32 | /mcp page polls continuously (never network-idle) | T6.4 | tour timeout log |

## Deep UI QA — eShop dogfood, user-style drive (`ui-deep-qa/`, 33 shots, script `src/DevContext.App/scripts/deep-qa-eshop.mjs`)

| # | Finding | Status | Evidence |
|---|---------|--------|----------|
| 37 | **Single-key nav shortcuts (h/e/a/i/m/c/s) do not navigate** — activity bar declares them (`shortKey`), only `?` overlay + `Ctrl+K` + `Shift+E` work | T6.5 | notes.md kbd-* FAIL rows |
| 38 | Light mode themes the routed page but the SHELL stays dark (titlebar, tab strip, activity rail) — half-themed UI | T6.6 | 31-explore-light.png, 30-home-light.png |
| 39 | THEME/VIBE has 3 vibes (Modern/Terminal/Hacker) × 3 modes — Terminal/Hacker never QA'd; no gate covers them | T6.6 | 30-home-light.png (settings) |
| 40 | Studio card previews echo the card TITLE ("Flow: /ProductList") instead of content; preview adds no information | T5.5 | 24-studio-budget-min.png |
| 41 | Studio provenance chips: all 5 preset cards cite the same `ProductList.cshtml.cs:3` (entry echo, not per-card source) | T5.3 | 24-studio-budget-min.png |
| 42 | Studio meter mixes units: server cards "110 tok" vs dead-stub tests/validator cards "~10L" (lines) — R9 stubs visible in UI | T4.3/T5.1 | 24-studio-budget-min.png |
| 43 | Omitted list absent even at 634/1000 tok near-cap (audit R1 confirmed live) | T5.1 | 24-studio-budget-min.png, notes.md |
| 44 | Insights copy bugs: "Missing validation … every WRITE endpoint" lists GET endpoints as evidence; "Module map: … DESKTOP apps are organised" on a web microservices repo | T6.3 | 21-insights-page.png |
| 45 | "Possible dead code" chips include framework-shaped types (NotFoundException, OrderItem) — noise | T6.3 | 21-insights-page.png |
| 46 | Omnibox works well: Ctrl+K → "checkout" → Enter deep-links to `/explore?focus=GET /Checkout` | verified good | 13-15 shots |
| 47 | Atlas "Export one-pager" works (Copied! feedback) | verified good | 20-atlas-export-clicked.png |
| 48 | Inspector five sections all render (Details/Code 874ch/Insights/Call Stack/Trail) | verified good | 07-11 shots |
| 49 | Navigating to `/` after visiting Settings renders Settings (route restore?) — Home unreachable by URL in that state | T6.5 verify | 30-home-light.png |
| 50 | Scope-picker groups by service with counts (Shopping.Web 10 …) ✓; `DiscountProtoServic…` row carries a red error icon with no visible explanation | T5.5/T6.2 | 24-studio-budget-min.png |

## Tooling / process (cost this session real time)

| # | Finding | Status | Evidence |
|---|---------|--------|----------|
| 33 | Server.Tests + eval CLI matrix leak `DevContext.Server` processes → next build fails on locked DLLs (hit 4×) | T0.1 | session log |
| 34 | start-dev-bg.ps1: Angular-timeout path leaks the just-started server; job output to NUL hides diagnosis | T0.1 | session log |
| 35 | audit-screenshots.mts spawns `pnpm` without shell → ENOENT on Windows | T0.1 | tour log |
| 36 | App worktree had stale node_modules → ng build module-not-found while vitest green | R-T6 rule | session log |
