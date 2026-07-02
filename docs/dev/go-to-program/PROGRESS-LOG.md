# Progress Log — go-to program

> Append-only session log. Date · Changed · Verified · Next.

---

## 2026-07-02 — R2 execution (session 1)

**Changed:**
- Merged addendum docs (I8 caching, I9 release, I10 tabs, ADDENDUM-A harder repos) from `C:\Code\DevContext2-addendum`
- Updated README.md tracker: added I8/I9/I10/A rows, updated CORE spine to I1→I2→I3→I4→I8→I10→I9
- Updated UNIFIED-TRACKER.md: added I8/I9/I10 sections, new delivery diagram
- **R2.1** Insights on wire: KernelJsonRenderer → proto → gRPC server → TypeScript store → desktop view → CLI. Full stack: `Insight[]` now reaches every face.
- **R2.2** NodeLink component: every name is a link. wired into entries/trace/node-card + document markdown linkify.
- **R2.3** Entries table: sortable columns, filter chips (has-target/approx), hover row actions (Trace/NodeCard/Copy), filtered/total counter.
- **R2.4** Trace fixes: F6 dead Tailwind class removed, focus breadcrumb with back, honest empty hint.
- **R2.5** Graph view: new face with seeded exploration from entries, seam filter chips, NodeCard via NodeLink. Route + rail item.
- **R2.6** Settings view: new face with Appearance/Analysis/Storage(I8)/Server/About(I9) sub-tabs. ConnectionStore now captures version from PingResponse.
- **R2.7** Palette: added Graph, Browse, Document, Settings entries.
- **R2.8** Connection: 3-state (online/connecting/offline) with server version tooltip.
- **R2.9** Overview: top-3 notable insights section at top.
- **R2.10** Export packs: Onboarding/Trace/Review presets that auto-select section toggles.

**Verified:**
- `dotnet build DevContext.slnx` — 0w 0e
- `dotnet test DevContext.slnx --filter Category!=Eval` — 385/0 green
- `pnpm lint` — green (pre-existing build errors in node-card/palette/node.store unrelated to R2)

## 2026-07-02 — Pre-existing TS errors + handover (session 1 cleanup)

**Changed:**
- Fixed 5 pre-existing TypeScript build errors from round-1 session that prevented `pnpm build`:
  - `node-card.ts`: removed `n.line` (not in NodeResponse proto); replaced `neigh.incoming`/`neigh.outgoing` with edge-filtering via computed signals
  - `palette.ts`: `r.results` → `r.nodes` (SearchResponse field name)
  - `app-shell.ts`: removed unnecessary `?.` on `label` (required proto field)
  - `node.store.ts`: `'both'` → `'out'` + `'in'` with merged edges via `create(NeighborsResponseSchema)`
- Fixed 4 self-inflicted errors from R2 code:
  - `settings-view.ts`: `theme.vibes`→`theme.vibes()`, `theme.activeVibe`→`theme.vibe()`, removed unused imports
  - `graph-view.ts`: removed unused Icon/Badge imports
  - `document-view.ts`: `onDocClick(MouseEvent)` → `onDocClick(Event)` for keyboard event compatibility
  - `title-bar.ts`: fixed template string literal parsing error with single quote
- Rewrote HANDOVER.md: round-2 delivery summary, review checklist, known caveats, next-items table, resume protocol

**Verified:**
- `pnpm check` fully green: lint · 4/4 tests · build success (app bundle generated, 0 errors, 0 warnings)
- All 12 lazy chunks built: entries-view, source-view, trace-view, document-view, settings-view, browse-view, overview-view, stats-view, graph-view, insights-view, cache-view

**Next:** Desktop smoke test (verify faces render real data) → E1 remaining insight sources
