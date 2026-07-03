# UI/UX Gap Tracker — DevContext Desktop

> Date: 2026-07-02 · Branch: `develop` · pnpm check: green
>  
> Each gap references the spec doc, the affected file, and the expected fix.
> Ordered by priority. Items marked BLOCKED depend on engine/server work.

---

## P0 — Unshipped Features (built but not wired)

### GAP-T1: Multi-tab strip not wired
- **Severity:** Critical — entire I10 multi-tab feature is built but invisible
- **Status:** Ready to wire
- **Spec:** `docs/dev/go-to-program/ITERATION-I10-workspace-tabs.md` §3
- **File:** `src/app/shell/workspace-shell.ts`
- **What exists:** `tab-strip.ts` is complete — tab bar UI, Ctrl+T/W/1-6 shortcuts, middle-click close, auto-analyze on first activation, route restore on switch, status dots, cap enforcement. `WorkspaceStore` has full multi-tab support (create/close/switch/persist to localStorage). `SessionStore` and `TraceStore` are per-tab facades.
- **What's missing:** `TabStrip` component is never imported or placed in `workspace-shell.ts` template.
- **Expected fix:**
  1. Import `TabStrip` in `workspace-shell.ts`
  2. Add `<app-tab-strip />` between header and content area
  3. Adjust height calc: `calc(100vh - 2.75rem - 2rem - 1.75rem)` (header 2.75rem + tab strip 2rem + footer 1.75rem)

### GAP-T2: Tab shortcuts missing from help overlay
- **Severity:** Medium
- **Status:** Ready to fix
- **Spec:** `ITERATION-I10-workspace-tabs.md` §3
- **File:** `src/app/shell/workspace-shell.ts` (SHORTCUT_HELP array lines 22-33)
- **Expected fix:** Add 3 entries: `{ keys: 'Ctrl+T', desc: 'New tab' }, { keys: 'Ctrl+W', desc: 'Close active tab' }, { keys: 'Ctrl+1-6', desc: 'Jump to tab' }`

### GAP-T3: `shortLabel()` cuts paths at spaces
- **Severity:** Medium
- **Status:** Ready to fix
- **File:** `src/app/shell/tab-strip.ts` line 146-149
- **What's wrong:** `label.split(' ')[0]` returns first word — for path `C:\My Project\src`, it shows `C:\My` instead of `src`. Should use the last path segment or basename.
- **Expected fix:** Extract last segment: `label.split(/[\\/]/).pop() ?? label`

### GAP-T4: No duplicate-path guard
- **Severity:** Medium
- **Status:** Ready to fix
- **Spec:** `ITERATION-I10-workspace-tabs.md` §3: "analyzing a path already open in another tab switches to that tab"
- **File:** `src/app/state/session.store.ts` — `analyze()` method
- **Expected fix:** Before `createTab`, check all existing tabs for matching `tab.path`. If found, call `workspace.setActive(tab.id)` and return.

### GAP-T5: No Ctrl+Tab MRU cycle
- **Severity:** Medium
- **Status:** Ready to fix
- **Spec:** `ITERATION-I10-workspace-tabs.md` §3: "Ctrl+Tab/Ctrl+Shift+Tab MRU cycle"
- **File:** `src/app/shell/tab-strip.ts` — `onGlobalKey()` method
- **Expected fix:** Maintain a `mruStack: string[]` in WorkspaceStore. On tab switch, push id to front. Ctrl+Tab → activate `mruStack[1]` (next most recent). Ctrl+Shift+Tab → activate `mruStack[mruStack.length-1]`.

### GAP-N1: Root route `/` has no explicit config
- **Severity:** Low
- **Status:** Ready to fix
- **File:** `src/app/app.config.ts` — routes array
- **What's wrong:** No `path: ''` entry. The overview-page loads only as child of workspace-shell's wildcard redirect. Works but is fragile.
- **Expected fix:** Add explicit `{ path: '', redirectTo: '/overview', pathMatch: 'full' }` or `{ path: '', loadComponent: () => import('./features/pages/overview-page') }`

---

## P1 — Missing Features from UI-UX-GUIDELINES Spec

### GAP-S1: Auth column on Entries table
- **Severity:** Medium — BLOCKED on U3/E4 engine facets
- **Status:** Deferred
- **Spec:** `docs/dev/go-to-program/UI-UX-GUIDELINES.md` §4
- **File:** `src/app/features/narrative/section-entries.ts`
- **What's missing:** Column shows Auth `[anon]` badge or policy name when F1 data is present. Hidden when no auth data.
- **Blocked on:** Engine facet layer (F1 auth surface). `EntryPoint` proto has no auth fields. `FacetDescriptor.cs` is an orphaned stub.

### GAP-S2: file:line column on Entries table
- **Severity:** Medium — BLOCKED on engine proto change
- **Status:** Deferred
- **Spec:** `UI-UX-GUIDELINES.md` §3, §4
- **File:** `src/app/features/narrative/section-entries.ts`
- **What's missing:** "subtle, reveal-on-click" column showing file path + line number.
- **Blocked on:** `EntryPoint` proto has no `filePath` or `line` field. Could lazy-fetch via `getNode` RPC as workaround.

### GAP-S3: Storage tab — no file operations
- **Severity:** Medium — needs Tauri plugin
- **Status:** Deferred
- **Spec:** `UI-UX-GUIDELINES.md` §7
- **File:** `src/app/features/settings/settings-view.ts`
- **What's missing:** "Open folder" button for cache/clone dirs, per-repo cache list with sizes, clear button, total disk usage bar.
- **Expected fix:** Wire `@tauri-apps/plugin-fs` or custom Rust commands for file-system operations.

### GAP-S4: No export pack presets
- **Severity:** Low
- **Status:** Ready
- **Spec:** `UI-UX-GUIDELINES.md` §9: "per-section visibility toggles (Export packs cover it)"
- **File:** `src/app/features/narrative/section-export.ts`
- **What's missing:** Named packs (Onboarding / Trace / Review) that auto-select section presets.
- **Expected fix:** Add 3 preset buttons that call `sectionData.update()` to set specific checkboxes.

### GAP-S5: Nav rail — disabled items show no tooltip
- **Severity:** Low
- **Status:** Ready
- **Spec:** `UI-UX-GUIDELINES.md` §1: "Disabled (with tooltip 'analyze first') until session.ready()"
- **File:** `src/app/shell/navigation-rail.ts`
- **What happens:** Items with `requiresSession: true` are filtered out entirely when no session. Spec says they should be visible but disabled with tooltip.
- **Expected fix:** Change `visibleItems` filter to always show items, add `[class.opacity-40]` + `[class.pointer-events-none]` when disabled, and `title="Analyze a repo first"`.

### GAP-S6: Nav rail — no count badges
- **Severity:** Low
- **Status:** Ready
- **Spec:** `UI-UX-GUIDELINES.md` §1: "counts badge on Entries/Insights when ready"
- **File:** `src/app/shell/navigation-rail.ts`
- **Expected fix:** Show small badge number overlaid on Entries and Insights rail icons. Read from `session.entryCount()` and `session.insightCount()`.

### GAP-S7: Nav rail — missing icon registry entries
- **Severity:** Low
- **Status:** Ready
- **File 1:** `src/app/shell/navigation-rail.ts` lines 17-22 — uses `home`, `list`, `lightbulb`
- **File 2:** `src/app/ui/icon/icon.ts` — REGISTRY has no entries for these names
- **Expected fix:** Either add the icons to the registry (`Home`, `List`, `Lightbulb` from lucide) or change rail items to use existing icons (e.g., `map` for overview, `layers` for entries, `zap` for insights).

---

## P2 — Bugs & Robustness

### GAP-B1: Palette — no debounce on node search
- **Severity:** Medium
- **Spec:** `UI-UX-GUIDELINES.md` §6: "Nodes (SearchNodes, debounced 150ms, top 8)"
- **File:** `src/app/features/palette/palette.ts`
- **What's wrong:** Every keystroke ≥2 chars fires a gRPC call immediately.
- **Expected fix:** Add `debounceTime(150)` via RxJS or a `setTimeout` guard on the node search RPC call.

### GAP-B2: Palette — no Tab verb cycling
- **Severity:** Low
- **Spec:** `UI-UX-GUIDELINES.md` §6: "Tab cycles verbs shown as chips (Trace · Node · Usages · Copy)"
- **File:** `src/app/features/palette/palette.ts` — `onKey()` method
- **Expected fix:** Add Tab key handler that cycles through verb options. Show verb chips below the selected result.

### GAP-B3: Palette — no Recents section
- **Severity:** Low
- **Spec:** `UI-UX-GUIDELINES.md` §6: "Sources merged & sectioned: Actions · Entries · Nodes · Recents"
- **File:** `src/app/features/palette/palette.ts` — `buildItems()` method
- **Expected fix:** Inject `RecentStore`, add a "Recents" section with recent repo entries.

### GAP-B4: Palette — no kind icons on node results
- **Severity:** Low
- **Spec:** `UI-UX-GUIDELINES.md` §6: "Nodes ... kind icon"
- **File:** `src/app/features/palette/palette.ts`
- **Expected fix:** Show `KIND_ICONS[node.kind]` icon next to each node result.

### GAP-B5: Palette — wrong "no results" text
- **Severity:** Low
- **Spec:** `UI-UX-GUIDELINES.md` §6: "No results → 'search the graph for '<q>''"
- **File:** `src/app/features/palette/palette.ts`
- **Expected fix:** Change empty-state text to include the query: `search the graph for '${query}'`.

### GAP-B6: Graph page — wrong empty state for no session
- **Severity:** Low
- **File:** `src/app/features/pages/graph-page.ts` + `src/app/features/narrative/section-graph.ts`
- **What's wrong:** SectionGraph shows "Select an entry and trace it..." even when no repo is analyzed. Should show "Analyze a repo first."
- **Expected fix:** Add `@if (!session.ready()) { ... }` guard in graph-page or section-graph.

### GAP-B7: Export page — no loading/error states
- **Severity:** Low
- **Spec:** `UI-UX-GUIDELINES.md` §3: "Empty/loading/error triad is MANDATORY per view"
- **File:** `src/app/features/pages/export-page.ts`
- **Expected fix:** Show spinner when session is busy, error message when failed, redirect to landing when idle.

### GAP-B8: NodeCard — no skeleton on loading
- **Severity:** Low
- **Spec:** `UI-UX-GUIDELINES.md` §3: "skeleton (not spinner) for loading"
- **File:** `src/app/features/node-card/node-card.ts` — shows text "Loading..." instead
- **Expected fix:** Replace with a skeleton pulse block.

---

## P3 — Code Quality

### GAP-C1: Duplicated helper functions
- **Severity:** Low
- **Files:** `src/app/features/narrative/section-stats.ts` (lines 153-182) and `src/app/features/narrative/section-console.ts` (lines 148-176)
- **Duplicated:** `ms()`, `num()`, `pct()`, `fmtK()`, `cacheHitRate()`, `funnelTypesPct()`, `funnelTokensPct()`, `maxStageMs()` — ~40 lines identical.
- **Expected fix:** Extract to shared file `src/app/models/pipeline-helpers.ts`.

### GAP-C2: Palette rebuilds entire list on every keystroke
- **Severity:** Low
- **File:** `src/app/features/palette/palette.ts` — `buildItems()` called from `filtered()` computed
- **What's wrong:** Static items (actions, view shortcuts) are rebuilt alongside search-dependent items.
- **Expected fix:** Separate into static section (computed once) and search section (computed per keystroke).

---

## Summary

| Priority | Count | Ready Now | Blocked |
|----------|-------|-----------|---------|
| P0 (Unshipped) | 6 | 6 | 0 |
| P1 (Missing from Spec) | 7 | 4 | 3 |
| P2 (Bugs) | 8 | 8 | 0 |
| P3 (Code Quality) | 2 | 2 | 0 |
| **Total** | **23** | **20** | **3** |

**Next actions (in order):**
1. Wire tab strip (T1-T5) — 30 min of work, unlocks multi-repo workflows
2. Nav rail polish (S5-S7) — 15 min
3. Palette fixes (B1-B5) — 20 min
4. Graph page empty state (B6) — 5 min
5. Export page states (B7) — 10 min
6. NodeCard skeleton (B8) — 10 min
7. Export packs (S4) — 20 min
8. Duplicate helpers (C1) — 10 min
9. Palette perf (C2) — 15 min
