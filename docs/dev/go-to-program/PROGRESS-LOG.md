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

**Next:** E1–E5 engine carry-over (remaining insight sources, pattern-zoo, W9 deletion, facets, benchmark) · I8 caching & storage · I10 workspace tabs · I9 release readiness
