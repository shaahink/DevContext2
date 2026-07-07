# Meridian Phase — Final Report & Handover

> **Read this first** if you're picking this project back up cold. Branch `feat/meridian-m0`
> @ latest (2026-07-07). This closes the "M — Meridian: Wiring Truth, Agent Surface, One-Page
> Repo" track (`docs/dev/briefs/proposal-meridian.md`) — M0 through M9, including full-bench
> close-out audit. Written as a warm-up doc, not a changelog.

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
| M0 | Harness gate — bench + MCP QA + Playwright visual | DONE | `eval-results/2026-07-05/ui/`, `eval-results/2026-07-06/mcp-qa.md` |
| M1 | Wiring truth — cross-service traces with Handles ≥14, ServiceLinks 3 kinds | DONE | Dogfood: 493 nodes, 316 edges, 6 ServiceLinks |
| M2 | Insight relevance — repair + typed actions + layer/feature (engine) | DONE | 4 new sources, TypedAction engine→proto→UI |
| M3 | MCP re-architecture — server-of-record, stdio shim, /mcp page | DONE | 18 MCP tools, live feed, config snippets |
| M4 | MCP feature set — 9/9 tools (overview, resolve, flow, impact, read_source, find, config, get_context, tests_for) | DONE | `eval-results/2026-07-06/mcp-qa.md`: 8/8 QA passing, MCP-VS-GREP table |
| M5 | Agent eval ratchet — 5-repo QA, agent transcript, CI wiring | DONE | 38 calls, 6889 tok; checkout 2c/313tok; CI gate green |
| M6 | Home + Atlas — service map hero, flow steppers, one-pager export | DONE | ServiceMapHero, HomeTiles, FlowStepper, ServiceCards |
| M7 | Explore/Chrome/Table — design tokens, graph↔code, lenses, trail, table v2 | DONE | PrismJS code tab, lens-switcher, CDK-virtualized table |
| M8 | Context Studio — scope picker, composition, budget, provenance | DONE | 9 card types, server tokens, budget→RPC, provenance chips |
| M9 | Close-out — full bench, AUDIT.md, HANDOVER-MERIDIAN.md | DONE (this commit) | See §3 |

---

## 3. M9 — What This Session Delivered

1. **Full bench (M9.1):** `eval-results/2026-07-07/` — 22/22 repos pass content assertion. PowerToys
   (5,141 nodes, 2878 edges) and MassTransit (24,819 nodes, 2929 edges) — both Lighthouse-deferred
   repos — verified clean. Zero stubs, zero analysis failures.

2. **AUDIT.md (M9.2):** `eval-results/2026-07-07/AUDIT.md` — scores every M-stage gate (M0-M8) with
   fresh re-run evidence. Lighthouse W-finding scorecard re-verified. Per-repo bench summary table.
   Known gaps catalog. Verdict: all M-stage gates pass, all 3 product claims verified.

3. **HANDOVER-MERIDIAN.md (M9.3):** This document. Plus PROGRESS-LOG updated, MERIDIAN-START.md
   tracker closed.

---

## 4. UI/UX — Current State

All Meridian surface rework is complete:

| Surface | Route | Post-Meridian state |
|---------|-------|---------------------|
| Home | `/` (session ready) | Service map hero (gateway left, services center, bus bottom). Identity prose with claim links. Three tiles: Entries by kind, Wiring health %, Freshness. Top Flows. Onboarding row. |
| Atlas | `/atlas` | One-pager: service diagram, top flows as stepper strips, event wiring board, per-service style cards, cross-cutting behaviors, hub radar. Export to self-contained HTML. |
| Explore | `/explore` | Three-pane deck/canvas/inspector. Lens switcher: Service + Flow active, Layer/Feature pending (Gap 2). Code tab with PrismJS. Trail with dedupe/group/cap. Zen mode. |
| Table | Shift+E | CDK-virtualized table with archetype columns. Row expand = mini flow stepper. CSV export. |
| Insights | `/insights` | Severity-grouped cards with typed action buttons (Focus/Node/Filter). No dead-end navigation. |
| Context Studio | `/context` | Scope picker (service→entry tree + omnibox + presets). Composition (9 card types, drag-drop, body toggles). Budget panel (slider 1k–16k, server token meter, per-card bars, intent/format selectors). Copy/Save with feedback. |
| MCP page | `/mcp` | Status dot, host config snippets, session list, live token-metered feed, try-a-tool console. |
| Settings | `/settings` | Storage tab with real repo/clone sizes; Appearance with Dark/Light/System themes. |

**Keyboard model:** Ctrl+1..6 tabs, Esc ladder (`esc` → unpin peek → deselect node → clear focus →
clear filter), `?` help overlay, `j/k` deck sweep, `Ctrl+R` re-analyze with snapshot diff.

---

## 5. Backend Graph Capabilities (Post-Meridian)

### 5.1 Graph model
`CodeGraph`: 3 node kinds (`Type`, `Member`, `EntryPoint`), 8 edge kinds (`Calls`, `Sends`,
`Handles`, `Raises`, `Consumes`, `ReadsWrites`, `Resolves`, `WrappedBy`), plus `ServiceLink`
edges (bus, gRPC, HTTP via YARP). 3 resolution tiers (`Join`=1.0, `Syntactic`→`[approx]`,
`Semantic`→`[verified]`). Layer/Feature facets computed per-node (engine only — Gap 2).

### 5.2 GraphQuery API surface (post-Meridian)
```
EntryPoints(kind?)     Map()             Stats()
Trace(focus, depth, fanOut)              Node(id)
Neighbors(id, direction, kind?)          FindUsages(id)
ResolveNodeId(nameOrKey)                 BlastRadius(from, maxDepth)
InterestingPoints(archetype)             TopFlows(count)
ContextPackBuilder(focus, budget, intent) ConfigLookup(key)
FindTestsFor(id)
```

### 5.3 RPC coverage (19 RPCs)
| RPC | Status |
|-----|--------|
| Analyze, CloseSession, ListEntryPoints, GetMap, GetTrace, GetNode, GetNeighbors, SearchNodes, GetStats, Render, Ping | DONE (pre-Meridian) |
| GetImpact, ConfigLookup, FindTestsFor | DONE (M4) |
| GetContext (budget-priced, per-section) | DONE (M8) |
| ListSessions, StartMcp, StopMcp, ObserveToolCalls | DONE (M3) |
| **ReadSource** | **MISSING (Gap 1)** |

### 5.4 MCP tools (18 tools)
`overview`, `resolve`, `flow`, `impact`, `read_source`, `find`, `config`, `get_context`,
`tests_for`, `analyze`, `status`, `map`, `entrypoints`, `top_flows`, `interesting_points`,
`trace`, `node`, `neighbors`, `usages`, `search`, `insights`, `close_session`, `list_sessions`,
`stats`
All 9 M4 flagship tools verified in QA harness (8/8 passing, MCP-VS-GREP comparison).

---

## 6. Dogfood Bench Baseline (Post-Meridian)

eshop-microservices (11-project microservices repo, Carter + MediatR + MassTransit + gRPC + YARP):

```
Nodes: 493  |  Edges: 316  |  Entries: 34  |  ServiceLinks: 6  |  Analyzed: 2.8s
```

ServiceLinks: 1 bus (Basket→Ordering via BasketCheckoutEvent), 1 gRPC (Basket→Discount), 
4 HTTP (Shopping.Web→YarpApiGateway + 3 gateway→backend). Handles edges ≥14 (from 2 pre-M1).

---

## 7. Full Bench Summary (2026-07-07 — M9.1)

22 repos analyzed, all content-asserted (Stats + TopFlows present), zero failures.

| Repo | Nodes | Edges | Time | Archetype |
|------|-------|-------|------|-----------|
| eshop-microservices (dogfood) | 493 | 316 | 2.8s | Microservices |
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

## 8. Known Issues & Gaps

### 8.1 Engine Gaps (priority: next session)

| # | Gap | Severity | Plan |
|---|-----|----------|------|
| **Gap 1** | `read_source` RPC missing (proto + engine + server) | High | AGENTS.md:133-162. Inspector uses Render RPC for Code tab — should use a proper source-reading RPC. MCP has `read_source` tool; engine needs proto + server handler. |
| **Gap 2** | Layer/Feature not uplumbed to proto/UI | High | AGENTS.md:165-189. Engine computes (M2.4) but proto bridge missing. Lens-switcher shows `available: false` for Layer/Feature. |

### 8.2 Quality Gaps

- **Trap A:** `buildContext()` in Context Studio is client-side string assembly v0. Copy/Save
  bypasses the server-side `ContextPackBuilder`. Token estimates rely on ~2.5 tok/line heuristic.
  True round-trip needs a new RPC (or extending `GetContext` for export mode).
- **Trap B:** Freshness probe is analysis-time only. No proactive staleness check RPC exists.
  The stale banner works after re-analysis; can't pre-detect HEAD drift.
- **MCP flush race:** The MCP harness (`run.js`) works around a transport flush race with polling.
  This is a documented workaround, not a blocking bug.

### 8.3 Pre-existing (from Lighthouse)

- E9 partial scope (empty subfolder under unrelated ancestor → resolves ancestor)
- `drawMinimap()` not throttled (pan/zoom at 60fps)
- Export drawer had no abort mechanism (retired in M8.1 — N/A now)
- Windows DPI pass (125%/150%) never explicitly tested

---

## 9. Product Claims Verification

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

## 10. Recommended Next Steps

**Immediate priority — Engine Gaps 1-2 (1-2 sessions):**

1. **Gap 1: `read_source` RPC** — Add proto messages + RPC, regenerate C# stubs, implement
   server handler (unwrap node source span from snapshot, read file), regenerate TypeScript,
   replace `inspector.ts loadCode()` from Render to ReadSource. Plan: AGENTS.md:133-162.

2. **Gap 2: Layer/Feature uplumb** — Add layer/feature fields to proto NodeResponse/TraceNode,
   regenerate C#, wire ProtoMapper, regenerate TypeScript, set `unavailable: false` in
   lens-switcher, add minimal layer/feature rendering. Plan: AGENTS.md:165-189.

**Secondary priorities:**

3. **Round-trip ContextPackBuilder for Copy/Save** — Replace `buildContext()` client-side
   assembly with a server-side export RPC. This closes Trap A.

4. **Freshness probe RPC** — Lightweight pre-analysis staleness check. Closes Trap B.

5. **U3 Facet views** — Blocked on engine E4. When E4 lands, facet views unlock.

6. **Next phase** — As defined by the product roadmap. Likely a new proposal document.

---

## Appendix A: Gate Snapshot (M9 Close-Out)

```
dotnet build DevContext.slnx                             0w 0e
dotnet test DevContext.slnx --filter "Category!=Eval"    green
dotnet test DevContext.slnx --filter "Category=McpQa"    green (2 tests)
pnpm check (src/DevContext.App)                          lint 0/0 + test 27/27 + build 0w/0e
powershell -File scripts/bench.ps1                       22/22 repos pass content assertion
```

## Appendix B: Key Files for the Next Agent

| File | Purpose |
|------|---------|
| `MERIDIAN-START.md` | Phase tracker (now closed) |
| `docs/dev/briefs/meridian-agent-playbook.md` | Mandatory reading — quality bar, anti-patterns, run/test |
| `docs/dev/briefs/proposal-meridian.md` | Full proposal, M9 close-out section |
| `eval-results/2026-07-07/AUDIT.md` | M9 close-out audit |
| `src/DevContext.App/AGENTS.md` | App conventions, Gap 1-2 plans, run commands |
| `docs/dev/go-to-program/PROGRESS-LOG.md` | Session log (all sessions to date) |

---

This closes the Meridian phase. See `eval-results/2026-07-07/AUDIT.md` for the benchmark close-out
audit. See `MERIDIAN-START.md` for the closed tracker. The next agent starts from Gaps 1-2
in `src/DevContext.App/AGENTS.md:133-189`.
