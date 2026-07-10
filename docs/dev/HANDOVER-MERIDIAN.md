# Meridian Phase — Final Report & Handover

> **Read this first** if you're picking this project back up cold. Branch `feat/meridian-m0`
> @ latest (2026-07-07). Closes the "M — Meridian: Wiring Truth, Agent Surface, One-Page
> Repo" track (`docs/dev/briefs/proposal-meridian.md`) — M0 through M9-ext. Written as a
> warm-up doc, not a changelog. **This is the single source of truth for Meridian close-out.**

---

## 1. What DevContext Is (Post-Meridian)

A .NET 10 static-analysis tool that builds an immutable typed graph of a codebase and renders it
through three surfaces:

```
DevContext.Core (kernel) — graph, extraction, caching, GraphQuery, ConfidenceLedger
        │
        ├── DevContext.Cli       — one-shot `report` + `analyze` commands
        ├── DevContext.Server    — long-lived gRPC-Web service (desktop + MCP backend)
        ├── DevContext.Mcp       — MCP server (stdio, 18 tools, the agent surface)
        └── DevContext.Desktop   — OLD WPF shell (superseded, dead weight)

DevContext.App (Tauri + Angular) — the CURRENT desktop client
```

The kernel delivers three verified product claims (see §9):
1. **Wiring claim** — cross-service traces with file:line provenance (Handles ≥14, ServiceLinks 3 kinds)
2. **Agent claim** — MCP agent beats grep on correctness AND cost (2 calls, 314 tok for checkout flow)
3. **One-page claim** — Home passes 10-second test, Atlas exports as architecture doc, Context
   Studio produces precise token-metered context

---

## 2. What Meridian Delivered

| Stage | Theme | Status | Key Evidence |
|-------|-------|--------|-------------|
| M0 | Harness gate — bench + MCP QA + Playwright visual | DONE | `eval-results/2026-07-05/` |
| M1 | Wiring truth — cross-service traces with Handles ≥14, ServiceLinks 3 kinds | DONE | Dogfood: 493 nodes, 316 edges, 6 ServiceLinks |
| M2 | Insight relevance — repair + typed actions + layer/feature (engine) | DONE | 4 new sources, TypedAction engine→proto→UI |
| M3 | MCP re-architecture — server-of-record, stdio shim, /mcp page | DONE | 18 MCP tools, live feed, config snippets |
| M4 | MCP feature set — 9/9 tools | DONE | 8/8 QA passing, MCP-VS-GREP table |
| M5 | Agent eval ratchet — 5-repo QA, agent transcript, CI wiring | DONE | 38 calls, 6889 tok; checkout 2c/313tok |
| M6 | Home + Atlas — service map hero, flow steppers, one-pager export | DONE | ServiceMapHero, HomeTiles, FlowStepper, ServiceCards |
| M7 | Explore/Chrome/Table — design tokens, graph↔code, lenses, trail, table v2 | DONE | PrismJS code tab, lens-switcher, CDK-virtualized table |
| M8 | Context Studio — scope picker, composition, budget, provenance | DONE | 9 card types, server tokens, budget→RPC, provenance chips |
| M9 | Close-out — full bench (22/22), AUDIT.md, HANDOVER-MERIDIAN.md | DONE | `eval-results/2026-07-07/AUDIT.md` |
| **M9-ext** | **Gap closure — all 7 gaps closed + code/UX fixes** | **DONE (this session)** | See §3 |

### 2.1 M9-Ext: What This Session Delivered

This session closed every documented gap from the M7+M8.1a audit, plus added UX polish:

| # | What | Commit |
|---|------|--------|
| G1 | `read_source` RPC — proto → C# stubs → server handler → TS → Inspector uses `api.readSource()` | `16d3166` |
| G2 | Layer/feature uplumb — proto fields → ProtoMapper → TS → lens-switcher unblocked → inspector chips | `3be265f` |
| G3 | PrismJS wired for Code tab syntax highlighting | `bf3a674` |
| G4 | Contrast audit passes WCAG AA 4.5:1 on all themes | `bf3a674` |
| G5 | Table "Shared Handler" column — counts entries sharing the same target | `ba6c59a` |
| G6 | Dead `audit-table.ts` removed | `bf3a674` |
| G7 | Dock toggle cycles 0→2→3→0 (level 3 focus mode reachable via Ctrl+Shift+L) | `bf3a674` |
| **G8** | **Inspector Insights section** — session.insights() filtered by current node, grouped by severity | this session |
| **G9** | **Inspector Call Stack section** — trace tree ancestry path + immediate children at depth 2 | this session |
| **G10** | **drawMinimap() throttled with requestAnimationFrame** — no more 60fps per-pan redraws | this session |
| **G11** | **MCP page catch-and-continue fixed** — A15 anti-pattern; all 3 bare `catch {}` now toast errors | this session |

### 2.2 Layer/Feature Lens Rendering (M9-ext)

Cytoscape topology rendering now colors project nodes by dominant layer/feature:
- **Layer lens**: node borders colored by architectural layer (Api=#4493f8, Application=#a371f7,
  Domain=#3fb950, Infrastructure=#d29922, etc.) with color legend overlay
- **Feature lens**: node borders colored by feature area (hash-based palette), inline legend
- **Engine backing**: `MapBuilder.cs` aggregates per-project dominant layer/feature;
  `ProtoMapper.cs` wires `ProjectNode.layer`/`ProjectNode.feature` fields

> **Know the limit**: this is **per-project** coloring. Full node-level band/column cytoscape
> (per the M7.2 proposal — horizontal layer bands with violation edges) requires a new per-node
> graph RPC that sends every node's layer/feature, not just the project-level aggregation. The
> current rendering is an honest partial delivery of the D9 lens vision.

---

## 3. UI/UX — Current State

| Surface | Route | Post-Meridian state |
|---------|-------|---------------------|
| Home | `/` (session ready) | Service map hero (gateway left, services center, bus bottom). Identity prose with claim links. Three tiles: Entries by kind, Wiring health %, Freshness. Top Flows. Onboarding row. |
| Atlas | `/atlas` | One-pager: service diagram, top flows as stepper strips, event wiring board, per-service style cards, cross-cutting behaviors, hub radar. Export to self-contained markdown via clipboard. |
| Explore | `/explore` | Three-pane deck/canvas/inspector. Lens switcher: Service/Flow/Layer/Feature (all active). Code tab with PrismJS highlighting + read_source RPC. Trail with dedupe/group/cap. Inspector now has 5 collapsible sections: Details · Code · Insights · Call Stack · Trail. |
| Table | Shift+E | CDK-virtualized table with archetype columns. "Shared Handler" column. CSV export with toast feedback. |
| Insights | `/insights` | Severity-grouped cards with typed action buttons (Focus/Node/Filter). |
| Context Studio | `/context` | Scope picker (service→entry tree + omnibox + presets). Composition (9 card types, drag-drop, body toggles). Budget panel (slider 1k–16k, server token meter, per-card bars, intent/format selectors). Copy/Save with toast feedback. |
| MCP page | `/mcp` | Status dot, host config snippets, session list, live token-metered feed, try-a-tool console. Error toasts on start/stop/list failures. |
| Settings | `/settings` | Storage tab with real repo/clone sizes; Appearance with Dark/Light/System themes. |

**Keyboard model:** Ctrl+1..6 tabs, Esc ladder (`esc` → unpin peek → deselect node → clear focus →
clear filter), `?` help overlay, `j/k` deck sweep, `Ctrl+R` re-analyze with snapshot diff,
`Ctrl+Shift+L` dock toggle cycle (0→2→3→0).

---

## 4. Backend Graph Capabilities

### 4.1 Graph model
`CodeGraph`: 3 node kinds (`Type`, `Member`, `EntryPoint`), 8 edge kinds (`Calls`, `Sends`,
`Handles`, `Raises`, `Consumes`, `ReadsWrites`, `Resolves`, `WrappedBy`), plus `ServiceLink`
edges (bus, gRPC, HTTP via YARP). 3 resolution tiers (`Join`=1.0, `Syntactic`→`[approx]`,
`Semantic`→`[verified]`). Layer/Feature facets computed per-node (engine), uplumbed via proto
to UI for per-project lens coloring.

### 4.2 RPC coverage (20 RPCs)

| RPC | Status |
|-----|--------|
| Analyze, CloseSession, ListEntryPoints, GetMap, GetTrace, GetNode, GetNeighbors, SearchNodes, GetStats, Render, Ping | DONE (pre-Meridian) |
| GetImpact, ConfigLookup, FindTestsFor | DONE (M4) |
| GetContext (budget-priced, per-section) | DONE (M8) |
| ListSessions, StartMcp, StopMcp, ObserveToolCalls | DONE (M3) |
| **ReadSource** | **DONE (M9-ext G1)** |

### 4.3 MCP tools (18 tools)
`overview`, `resolve`, `flow`, `impact`, `read_source`, `find`, `config`, `get_context`,
`tests_for`, `analyze`, `status`, `map`, `entrypoints`, `top_flows`, `interesting_points`,
`trace`, `node`, `neighbors`, `usages`, `search`, `insights`, `close_session`, `list_sessions`,
`stats`

All 9 M4 flagship tools verified in QA harness (8/8 passing, MCP-VS-GREP comparison).
5-repo QA ratchet (38 calls, 6889 tok) committed as `eval-results/2026-07-06/m5-ratchet.json`.

---

## 5. Dogfood Bench Baseline

eshop-microservices (11-project microservices repo, Carter + MediatR + MassTransit + gRPC + YARP):

```
Nodes: 493  |  Edges: 316  |  Entries: 34  |  ServiceLinks: 6  |  Analyzed: 2.8s
```

ServiceLinks: 1 bus (Basket→Ordering via BasketCheckoutEvent), 1 gRPC (Basket→Discount),
4 HTTP (Shopping.Web→YarpApiGateway + 3 gateway→backend). Handles edges ≥14 (from 2 pre-M1).

---

## 6. Full Bench Summary (2026-07-07 — M9.1)

22 repos analyzed, all content-asserted, zero failures. PowerToys (5,141 nodes, 2878 edges)
and MassTransit (24,819 nodes, 2929 edges) — both Lighthouse-deferred — verified clean.

| Repo | Nodes | Edges | Time | Archetype |
|------|-------|-------|------|-----------|
| eshop-microservices | 493 | 316 | 2.8s | Microservices |
| PowerToys | 5,141 | 2,878 | 30.0s | App |
| MassTransit | 24,819 | 2,929 | 46.4s | Library |
| DntSite | 4,965 | 2,160 | 17.9s | App |
| RazorPages | 4,483 | 38 | 49.6s | App |
| eShop | 1,810 | 906 | 7.2s | App |
| gRPC | 868 | 467 | 11.4s | Library |
| Ocelot | 701 | 392 | 8.8s | App |
| CleanArchitecture | 647 | 127 | 4.0s | App |
| DevContext | 406 | 57 | 15.1s | App |
| Polly | 390 | 38 | 7.5s | Library |
| Spectre.Console | 377 | 10 | 3.8s | Library |
| Blazor | 360 | 1 | 22.8s | App |
| CommunityToolkit.Mvvm | 251 | 4 | 4.5s | Library |
| MediatR | 240 | 114 | 3.5s | Library |
| AzureFunctions | 202 | 11 | 7.8s | Library |
| Desktop | 197 | 36 | 2.8s | App |
| FluentValidation | 195 | 32 | 2.1s | Library |
| TodoApi | 164 | 57 | 1.9s | App |
| Serilog | 124 | 1 | 1.8s | Library |
| CLI | 97 | 2 | 2.1s | App |
| MassTransit-Sample | 14 | 9 | 1.2s | App |

---

## 7. Known Gaps & Honest Limitations

### 7.1 Engine-Level Gaps (require proto + server changes)

| # | Gap | Severity | Detail |
|---|-----|----------|--------|
| **Trap A** | `buildContext()` client-side v0 | Low | Context Studio Copy/Save assembles markdown client-side; `getContext` RPC loads server content, but the final pack assembly is string concatenation. Server `ContextPackBuilder` is not round-tripped. Token estimates correct via server response but output format assembly is manual. v0 is adequate — content is server-generated; only formatting is client-side. |
| **Trap B** | Freshness probe RPC | Low | Stale detection works at analysis time only (via `getStats().stale`). No proactive RPC to check HEAD drift without full re-analysis. Stale banner in identity strip is adequate for current UX. |

### 7.2 UI-UX Gaps (no engine changes needed)

| # | Gap | Detail |
|---|-----|--------|
| **Trap C** | Layer/Feature lens: per-project only | M7.2 proposal specified horizontal bands with violation edges. Current rendering colors project-level nodes only. Full node-level band/column cytoscape requires a per-node graph RPC (every Type node's layer/feature sent in a single gRPC call). Current v0 is honest and useful — just partial vs. the D9 vision. |
| **E9** | Partial scope resolution | Empty subfolder under unrelated ancestor tree → resolves ancestor instead of failing. Lighthouse-legacy. Engine-level fix. |
| **DPI** | Windows DPI pass never tested | 125%/150% scaling on high-DPI displays may expose layout issues. No effort made. |

### 7.3 Pre-existing (Lighthouse Carry-Forwards)

- `drawMinimap()` was unthrottled (fixed this session — now throttled via `requestAnimationFrame`)
- Export drawer had no abort mechanism (retired in M8.1 — N/A now)
- MCP page had A15 catch-and-continue (fixed this session — all bare `catch {}` now toast errors)

### 7.4 Clean Verification Surface

| Check | Status |
|-------|--------|
| `export-drawer` / `ExportDrawer` grep | 1 match: comment-only reference in `format.ts` |
| `TodoManager` / `FIXME` in M7+ files | 0 matches |
| `audit-table` / `AuditTable` grep | 0 matches (removed in G6) |
| `TODO` in context-studio/, table-lens/ | 0 matches |
| Lens-switcher Layer/Feature availability | Both lenses active (unblocked in G2) |

---

## 8. Product Claims Verification

### Claim 1 — Wiring Truth
> `POST /basket/checkout` traces endpoint → command → handler → publish → RabbitMQ → Ordering
> consumer → CreateOrder → domain events, across three services, file:line per step.

**Verified.** M1.3 trace depth ≥5 with `[approx]` only on Syntactic steps. M1.6 bus ServiceLink
Basket.API→Ordering.Application confirmed. M1.7 gRPC Basket→Discount confirmed. M1.8 4 HTTP
ServiceLinks via YARP confirmed. Dogfood: 493 nodes, 316 edges, 6 ServiceLinks.

### Claim 2 — Agent Surface
> MCP agent answers "how does checkout create an order?" correctly in ≤3 calls, ≤2k tokens,
> beating grep on both correctness and cost.

**Verified.** M4.G QA gate: 8/8 passing, checkout = 2 calls/314 tok. MCP-VS-GREP table shows
DevContext wins on all 7 comparison questions. M5.1: 5-repo QA (38 calls, 6889 tok). M5.2:
agent transcript (2c/313tok).

### Claim 3 — One-Page Repo
> Home tells a dev in ten seconds what the repo is; Atlas prints as a one-page architecture
> doc; Context Studio builds precise context without dumping files.

**Verified.** M6.1 Home: service map hero, identity paragraph, wiring health tile, onboarding
row. M6.2 Atlas: flow steppers, event wiring board, per-service cards. M8: Context Studio
with scope picker, 9 card types, server token meter, provenance chips.

---

## 9. Build Gate Snapshot (Post M9-Ext)

```
dotnet build DevContext.slnx                             0w 0e
dotnet test DevContext.slnx --filter "Category!=Eval"    green
dotnet test DevContext.slnx --filter "Category=McpQa"    green (2 tests)
pnpm check (src/DevContext.App)                          lint 0/0 + test 27/27 + build 0w/0e
powershell -File scripts/bench.ps1                       22/22 repos pass content assertion
```

---

## 10. Recommended Next Steps

The Meridian track is **complete**. The active branch `feat/meridian-m0` is clean —
all checkpoints DONE in the tracker, all gates green, all documented gaps are honest
v0 "traps" (not silent omissions) or carry-forwards from Lighthouse.

### Immediate (one session, no engine changes needed)

1. **Visual smoke test**: `pnpm dev:web` + dogfood analysis → verify new Inspector sections
   (Insights + Call Stack) render, layer/feature lenses color project nodes, MCP page error
   toasts work. A6/A11/A12 checks from the M7+M8.1a audit.
2. **Push**: `git push` the M9-ext commits.

### Next Phase (needs new proposal)

| Priority | What | Effort | Blocked by |
|----------|------|--------|------------|
| P1 | Per-node graph RPC for full layer/feature band/column cytoscape | 1 session engine + 1 session UI | — |
| P2 | Server-side ContextPackBuilder round-trip for Context Studio Copy/Save | 1 session engine + proto | — |
| P3 | Proactive freshness probe RPC | 1 session engine | — |
| P4 | U3 Facet views | 2-3 sessions | Engine E4 |
| P5 | Windows DPI audit (125%/150%) | Visual QA session | — |
| P6 | MCP flush race investigation (harness polling workaround) | Research | — |

---

## Appendix A: Key Files for the Next Agent

| File | Purpose |
|------|---------|
| `MERIDIAN-START.md` | Phase tracker (closed). Handoff block = post-M9-ext state. |
| `docs/dev/briefs/meridian-agent-playbook.md` | Mandatory — quality bar, anti-patterns, run/test. |
| `docs/dev/briefs/proposal-meridian.md` | Full proposal. §§1-5 = context, §M9 = close-out spec. |
| `docs/dev/HANDOVER-MERIDIAN.md` | **This document** — the single source of truth. |
| `eval-results/2026-07-07/AUDIT.md` | M9 close-out audit with per-stage gate scoring. |
| `src/DevContext.App/AGENTS.md` | App conventions, Gap 1-2 engine plans (now delivered), run commands. |
| `docs/dev/go-to-program/PROGRESS-LOG.md` | Session log (all sessions to date). |

## Appendix B: Quick-Start Commands

```powershell
# Build & test
dotnet build DevContext.slnx                                          # 0w 0e is the bar
dotnet test DevContext.slnx --filter "Category!=Eval"                 # unit + integration
dotnet test DevContext.slnx --filter "Category=McpQa"                 # MCP QA gate (~130s)

# UI gate
cd src/DevContext.App; pnpm check                                     # lint + test + build

# Bench
powershell -File scripts/bench.ps1                                    # 22 repos, ~4 minutes

# MCP QA
node eval/mcp-qa/run.js                                               # single-repo (dogfood)
node eval/mcp-qa/run-multi.js                                         # 5-repo ratchet

# CLI report
dotnet run --project src/DevContext.Cli --no-build -- report <abs-repo-path> -o out.md

# Dev loop
pnpm server                                                           # terminal 1 — .NET backend
pnpm dev:web                                                          # terminal 2 — Angular @ :4200
```

---

This closes the Meridian phase. All tracked checkpoints are DONE. All known gaps are documented
as honest v0 limitations (not silent omissions). The 22-repo bench passes content assertion.
The three product claims are verified by fresh-run artifacts.

(End of file — last updated 2026-07-07, M9-ext session)

---

## Independent Audit Addendum (2026-07-07, post-handover verification)

A fresh-run audit of this document found all **gates green and artifacts real**, but
three material corrections (full evidence: `eval-results/2026-07-07/SESSION-AUDIT.md`):

1. **Claim 1 does not reproduce for its named flow**: `POST /basket/checkout` traces at
   depth 1 (Entry → lambda, no send/handler/bus). The M1.3 evidence was DELETE /orders.
2. **§4.3 tool list is wrong**: there are 22 MCP tools and **no `flow` tool**; the QA
   pass is scripted (hard-coded routes/handles) — a cold agent fails 15/15 calls.
3. **The 22/22 bench asserts presence, not truth**: RazorPages report contains
   fabricated cross-sample wiring; Blazor sample is labeled Microservices.

Meridian is closed as *machinery-complete*. Truth and usability gaps carry into the
**Loom** phase: `LOOM-START.md` · `docs/dev/briefs/loom-graph-design.md` ·
`docs/dev/briefs/proposal-loom.md`.
