# D4.0 screenshot-gate BASELINE — 2026-07-18 (branch feat/prism-d4 @ d3 tip)

Instrument: `src/DevContext.App/scripts/screenshot-gate.mts` (fresh browser context per repo —
clean localStorage, no tab/recents variance). SCREENSHOT-GATE: PASS — 16/16 captures, 0 page
errors, 0 console errors. Analyze walls: podcasts 10.0s · refit 9.7s · eShop 7.6s · bitwarden
80.2s (cold). Later checkpoints re-run the gate to a fresh `--out` dir and diff against this set.

## What the baseline shows (the "before" record, mapped to checkpoints)

**Loading (all four → D4.6/L7):** static stage list (Clone→Done), no elapsed, no active-stage
highlight, no per-stage counts, no "first analysis takes minutes / snapshotted for re-runs" copy.
bitwarden-1 proves it: 80 seconds against an unchanging list. Server already streams the stages
(StreamingProgressObserver) — the UI just doesn't render them.

**Canvas (home hero + atlas diagram → D4.1/F2/L6):** clipping and overlap everywhere.
podcasts-2: node clipped off the left edge, `Podcast.Updater.Worker`/`Podcast.API` labels
overlap. eshop-2: cluster crowded top-left with labels cut mid-word ("ntity"), ClientApp
floating far right, most of the canvas empty. podcasts-4: "13 projects · 14 edges" claimed,
~5 ListenTogether.* nodes drawn + one clipped. bitwarden-4 is the showcase: 33 projects /
76 edges as an unlabeled hairball — NO node labels at all, clipped at two edges; "how does
this system work" is unanswerable from it.

**Semantic rendering (→ D4.2/F3/M):** no transport labels on edges, no kind glyphs on nodes,
no DDD lanes, stores/externals indistinguishable — the one orange `bus-publish→consume` chip
below the canvas is the only semantics rendered.

**Atlas one-pager (→ D4.3/L3):** eShop/bitwarden have real Top flows + event wiring board;
diagram is the broken piece. refit-4 (library): "No flows indexed yet — start background
indexing" noise, empty event board, and a per-service breakdown with ONE card of 14 projects
(`Refit.NativeAotSmoke · Unknown · No stack signals detected`).

**Library workbench (→ D4.4/F1):** refit-3 Explore is a dead page — "Analyze a repo to list
its entry points", 0 entries, nothing to do. refit home/atlas show the style chip
(`ControllerBased · moderate`) that the CLI suppresses for Library archetype. Home cards show
entry-metrics (0 entries) instead of surface-metrics.

**Entry browser (→ D4.5/L5):** bitwarden-3: 676 entries as truncated raw rows (`/Organi…`,
`/organiz…` ×dozens) — no service grouping, no ranking visible, rows indistinguishable.

**Session naming (→ D4.5/F4) — NEW bug caught by the gate:** refit-2 home titles the session
**`DevContext.slnx · library · 14 projects · 842 types`** while the map hero shows Refit's own
projects and the explore/atlas pages call the same session `refit`. `DevContext.slnx` exists
only at the DevContext repo root (refit's solution is `src/Refit.slnx`) — the label smells like
server-CWD leakage into session naming. eShop also flips between `eShop.slnx` (home) and
`eShop` (atlas). Investigate at D4.5.

## Shot inventory
| repo | 1-loading | 2-home | 3-explore | 4-atlas |
|---|---|---|---|---|
| podcasts | static stage list | hero clip+overlap; cards+top flows good | 24 entries, kind chips, empty details rail | fragment diagram + clip |
| refit | static stage list | WRONG label (DevContext.slnx); style chip; 0-entry cards | dead page (library) | 4 nodes of 14; noise sections; 1 Unknown card |
| eshop | static stage list | hero clipped left, ClientApp stranded | (like podcasts, 109 entries) | hairball w/ overlaps; flows+events good |
| bitwarden | static stage list @80s | (captured) | 676-entry truncated dump | unlabeled 33-node hairball, clipped |
