# AGENTS.md — DevContext Desktop UI worktree

You are in `C:\Code\DevContext2-ui` on branch `feat/ui-iteration`.
**Mission:** Build the live console, synced lens, facet views, and release polish for the desktop app.
You work ONLY in `src/DevContext.App/` — zero C# changes needed.

## Start here (every session)
1. Read this file — the work items below are ordered.
2. `docs/dev/PLAN-DESKTOP-V3.md` — the spec for P2 Live Console and P3 Synced Lens.
3. `docs/dev/go-to-program/UI-UX-GUIDELINES.md` — design contract for all UI work.
4. `docs/dev/go-to-program/ITERATION-I5-facet-menu.md` — facet specs (for U3).
5. `src/DevContext.App/AGENTS.md` — app conventions, run commands, architecture layering.

## Work items (do NOT skip order — each builds on the prior)

### U1 — Live Console (V3 P2) ✅ DONE
Stream engine `ProgressEvent`s as a scrolling boot-log. Settle into the RunReport on completion.
- Done: `workspace.store.ts` — `LogLine` type + `consoleLog` in `TabSessionSlice`. `session.store.ts` — appends progress events to `consoleLog` signal. New `section-console.ts` (boot-log + RunReport). Wired into `narrative-canvas.ts`.
- Gate: analyzing a repo shows live streaming log; after completion = readable report with funnel.

### U2 — Synced Lens (V3 P3) ✅ DONE
Single selection → Human pane + LLM pane side-by-side. Auto-render on selection via existing `Render` RPC.
- Done: `section-lens.ts` — persistent 50/50 split. Human pane (node detail + trace tree). LLM pane (auto-rendered markdown). Debounced render on `TraceStore.focus` change. Copy button + Ctrl+C shortcut. Wired into `narrative-canvas.ts` after trace section.
- Gate: pick any entry — both panes show it. Copy is one keystroke. Zero navigation.

### U3 — Facet views (E4 facets, UI-side only — engine must deliver E4 first)
- F1 auth surface: add Auth column to Entries table (data comes from engine facet)
- F3 message matrix: producers→consumers table
- F4 data map, F5 talks-to, F8 DI health cards
- Gate: each facet renders real data from engine RPCs.

### U4 — Release UI polish (I9 UI side) ✅ DONE
- About section: version from ConnectionStore, server status dot, GitHub + issues + releases links, privacy note.
- Error telemetry hardening: audited all 34 catch sites. Fixed `palette.ts` swallow comment, added toast to `node.store.ts` failures, added toast to `node-card.ts` clipboard failures. Audit confirms no truly swallowed user-facing errors remain.
- Updates panel: "Check updates" link to GitHub Releases in About section.
- Gate: about shows real version; force an RPC error = toast appears.

### U5 — Workspace navigation + polish ✅ DONE
Navigation rail + routed views replacing single-page scroll. Entries table sorting/keyboard nav. Palette entry/node results. Keyboard shortcuts.
- Done: `shell/navigation-rail.ts` — left sidebar with icon+label navigation. `shell/workspace-shell.ts` — header + rail + router-outlet + footer, `g+key` view nav, `?` help overlay. `app.config.ts` — 8 lazy-loaded routes. `features/pages/` — overview, entries, trace, graph, insights, export page wrappers. `section-entries.ts` — sortable columns, arrow-key nav, row actions. `palette.ts` — entry search top 10, stale routes removed.
- Gate: clicking rail items navigates views; sort headers work; Ctrl+K palette searches entries; `?` shows shortcuts.

### U3 — Facet views (E4 facets, UI-side only — engine must deliver E4 first) ⬜ BLOCKED

## Verify loop
```powershell
# From C:/Code/DevContext2-ui/src/DevContext.App
pnpm check          # lint + 7/7 vitest tests + build — must be GREEN
pnpm server         # start .NET server (separate terminal)
pnpm dev:web        # start Angular dev server → http://localhost:4200
```

## Hard rules
- **No C# changes** — you don't need the engine worktree. If a face needs data the kernel can't answer, document it and move on. The engine agent fills the gap.
- **TypeScript only:** `src/DevContext.App/src/app/**`
- **pnpm check green** before every commit.
- Append `PROGRESS-LOG.md` after every session.

## Resume protocol (cold start)
```
git -C C:/Code/DevContext2-ui checkout feat/ui-iteration
git -C C:/Code/DevContext2-ui pull

# Verify baseline
Set-Location C:/Code/DevContext2-ui/src/DevContext.App
pnpm check

# Pick the first work item whose Status != DONE in this file
# Do Step 0 (reproduce) first, then execute
```
