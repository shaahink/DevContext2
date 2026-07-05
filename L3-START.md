# L5+L6 — MCP server + UI/UX round ⚠ DONE-WITH-GAPS (2026-07-05, second audit pass)

**Branch:** `feat/lighthouse-l2` · **Status:** L0–L6 code-complete but L6 graph-readability and
LLM-pane checkpoints were never actually done (see "Known gaps" below) — do not treat L6 as a clean
gate for L7. L0–L5 solid.

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

## Known gaps (found in this audit, NOT fixed — need a scoping decision before L7)

1. **L6 Session B checkpoint "adopt the voted layout lib" was never done.** The proposal's decision
   log (§3) explicitly voted for elkjs (Flow, layered DAG) + fcose/Cytoscape compound clustering
   (System). `graph-canvas.ts` still uses the same flat `cytoscape-dagre` layout for all three
   altitudes that has been in the repo since the original Tauri/Angular scaffold commit (`03c32e6`,
   pre-dates Lighthouse entirely) — nothing new was adopted. No System-altitude clustering/compound
   nodes/expand-collapse, no node-size-by-centrality, no label-density zoom thresholds, no minimap.
   Commit `e9d1ab1` silently renumbers L6 checkpoints (its own message calls "confidence chip in
   identity strip" **6.6**) and drops this item with no tracker line at all. Zen mode (6.7) and focus
   dimming (6.8) are real and good, but they're polish on the *same* layout the plan called clutter —
   the plan's own gate ("a stranger can name the main clusters unprompted" on PowerToys-class System
   views) cannot honestly be claimed passed.
2. **L6 Session A item 4, "LLM context pane" restyle, was never done.** `export-drawer.ts` still
   renders content as a raw `<pre>` block via the old Render RPC — the exact "looks like an accident"
   flaw the plan called out — rather than a styled preview backed by the new `ContextPackBuilder`
   (L5.4). Section token counts exist (a partial "token meter"), but the pack itself isn't sourced
   from `get_context`.
3. **L6's own verify gate was never run.** The proposal mandates "Playwright against a real analyzed
   repo... plus screenshots" before a UI stage is marked DONE. There are no Playwright specs and no
   screenshots anywhere in history for L6 — only unit tests (vitest, 27 tests, none of which touch
   `graph-canvas.ts` or the export drawer). This is very likely *why* gaps 1 and 2 shipped as "DONE."

Recommend folding 1+2 into a proper `L6.9` checkpoint (with the Playwright gate this time) before
opening L7, since L7's whole job is auditing claims like these against evidence.
