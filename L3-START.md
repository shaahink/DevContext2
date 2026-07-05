# L5+L6 — MCP server + UI/UX round ✅ DONE (2026-07-05, gaps closed same day)

**Branch:** `feat/lighthouse-l2` · **Status:** L0–L6 done, including the two gaps this audit found
(graph-layout-lib adoption, LLM context-pane restyle) and a Playwright-verified gate. L7 next.

## Verify gate (end-of-stage snapshot)
```
dotnet build C:/Code/DevContext2-ui/DevContext.slnx        # 0w 0e
dotnet test DevContext.slnx --filter "Category!=Eval"      # 429/0 (3 skipped)
pnpm check                                                  # from src/DevContext.App — lint 0/0 + test 27/27 + build
```

## L5 checkpoints (see `docs/dev/briefs/proposal-lighthouse.md` §L5)
| # | What | Commit |
|---|------|--------|
| 5.1 | DevContext.Mcp project scaffold + session manager + 13 tools | 85b15ad |
| 5.4 | ContextPackBuilder (kernel) + get_context tool | fe09351 |
| 5.5 | read_source tool | fe09351 |
| — | 2nd audit pass: `intent` (trace/explain/review) actually branches builder output (was plumbed but dead since 3a68938); snapshot-cache reuse + path→handle dedup in `McpSessionManager` (MCP never used the L1.2 cache — every `analyze` re-ran the full pipeline); new tools `CloseSession`/`ListSessions`/`Stats`; `Entrypoints(kind)` filter; `ReadSource(windowLines)`; fixed `Insights.detail` (was echoing `Title` instead of evidence) | (this commit) |

## L6 checkpoints (see `docs/dev/briefs/proposal-lighthouse.md` §L6)
| # | What | Commit |
|---|------|--------|
| 6.1 | Identity strip: human sentence + stat labels + hover tooltips | e9d1ab1 |
| 6.2 | Home insights: "What needs attention" / "Good to know" grouping | e9d1ab1 |
| 6.3 | Insight cards: impact grouping, evidence dedup + workbench links | e9d1ab1 |
| 6.4 | Deck identity: subtitles (target per row), group count badges | e9d1ab1 |
| 6.5 | Statusbar cleanup: remove node/edge plumbing | e9d1ab1 |
| 6.7 | Zen mode: F key full-screen, Escape exit, double-click header | e9d1ab1 |
| 6.8 | Focus dimming: hover dims non-neighbors; legend → popover | e9d1ab1 |
| — | Audit fixes: Ledger clickable, statusbar cleanup, intent param | 3a68938 |
| — | 2nd audit pass: memory-leak cleanup (unremoved `window` keydown listener in Stage, unsubscribed Router.events in ActivityBar/TabStrip, uncleared timers in StartHero/Inspector/WorkspaceShell); `identity-strip` ledger toggle was a plain field mutated from a template handler (not a signal) — converted, since a zoneless app has no guarantee that re-renders on a non-signal mutation; Inspector `renderedFocus` was marked before the async render resolved, so a failed render silently blocked the effect from ever retrying the same focus; Atlas → Explore project tap didn't actually filter the Workbench deck (`project` query param was dropped, `projectFilter` signal existed but nothing fed it) | (this commit) |

## Gaps found in this audit — now closed (commit: this session, post-44e25f0)

1. **L6 Session B "adopt the voted layout lib" — CLOSED.** `graph-canvas.ts` now uses `cytoscape-fcose`
   (force-directed) for System altitude instead of reusing the pre-Lighthouse flat `cytoscape-dagre`
   layout for everything; dagre stays for Flow/Node (call chains genuinely have rank direction, fcose
   doesn't fit them). Added: degree-centrality node sizing (sqrt scale, entry nodes still override),
   zoom-relative label-density hiding (threshold scales off the graph's own fit-zoom, not a fixed
   constant — a fixed constant doesn't track "is this dense right now" across graph sizes), and a
   canvas-drawn minimap (zen mode + >40 nodes only, click-to-recenter).
   *Verified live* against the cached PowerToys clone (`%LOCALAPPDATA%/DevContext/repos/microsoft-powertoys-default`,
   ~100 projects/5175 nodes on System altitude) via Playwright (`channel:'chrome'`, headless) against
   `pnpm dev:web`: fcose produces visibly distinct clusters + hub nodes (screenshots
   `04-system-altitude-clustered.png`, `05-system-zen-minimap.png`); labels correctly hidden at
   fit-zoom and reappear on zoom-in (`10-labels-at-fit-zoom.png`, `11-labels-after-zoom-in.png`).
   **Bug caught by this verification, not by code review:** cytoscape does *not* re-evaluate
   function-valued styles (the label mapper) on pan/zoom automatically — only on an explicit
   `cy.style().update()`. The first cut looked right in isolation (labels hidden/shown once, at
   whatever zoom the graph first painted at) but froze after that; zooming in never revealed labels.
   Fixed by calling `style().update()` on the `zoom` event. This is exactly the class of bug the
   proposal's "verify live, not just code review" gate exists to catch.
2. **L6 Session A "LLM context pane" restyle — CLOSED.** Added a `GetContext` gRPC RPC
   (proto + `DevContext.Server` handler) wrapping `ContextPackBuilder` with per-section *content*
   (not just token counts — `SectionAllocation` gained a `Content` field), so the desktop client can
   render a real pack instead of a raw markdown blob. `export-drawer.ts`'s Flow Review / From Trail
   presets now render collapsible per-section cards (real token meter per section, intent selector
   trace/explain/review, budget selector) instead of one `<pre>`-wrapped blob; Full/Onboarding stay on
   the old Render RPC (correct — those are whole-document renders, not context packs).
   *Verified live*: single-focus pack (screenshots `13-15`) and multi-pin From Trail pack
   (`17-export-trail-with-pins.png`, two pinned steps, 192+219=411 tok header sum confirmed correct)
   both round-trip through the real RPC against a live analysis.
3. **L6's own verify gate — now run.** Playwright (`playwright`, `channel:'chrome'`, headless) drove
   `pnpm dev:web` (server + `ng serve`, no Tauri needed since it's plain gRPC-web) through: analyze →
   System altitude → zen mode → Flow altitude with a real focus → export drawer, all four presets.
   Screenshots retained in the session scratchpad.

Gate snapshot after these fixes: `dotnet build` 0w/0e; `dotnet test --filter Category!=Eval` 429/0
(3 skipped); `pnpm check` (lint 0/0 + test 27/27 + build) clean.
