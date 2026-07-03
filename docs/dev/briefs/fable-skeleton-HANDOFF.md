# Fable Skeleton — Handoff to the Build/Verify Agent

> Branch: `feat/fable-redesign-skeleton` · Written 2026-07-03, superseded by events since — **this
> doc is now historical narrative, not current status.** As of 2026-07-03 the branch has been
> compiled, run, and smoke-tested repeatedly across several sessions; W0 and W1 are done, W3 is
> mostly done. **For current per-stage status, always check `AGENTS.md`'s "F — Fable Workbench
> Redesign" section first** — it's updated every session and is the source of truth. The file-by-file
> risk-note table and "design intents you must not fix" list below are still accurate and worth
> reading once; the "Next steps" list below is not (AGENTS.md's is current).

## Step zero (before touching anything)

```powershell
Set-Location C:/Code/DevContext2-ui/src/DevContext.App
pnpm check > check.log; echo $LASTEXITCODE     # REAL exit code — never pipe through tail
```

This should already be green. If it isn't, something regressed since the last verified session —
bisect before writing new code.

## What this branch adds (file by file)

| File | What it is | Risk notes for you |
|------|-----------|--------------------|
| `app/core/rpc-call.ts` | `LatestGate` — switchMap semantics (epoch + abort) for store RPCs. Proposal §5.1. | Pure TS, no deps. Semantics locked by the spec file — don't "simplify" the epoch-vs-abort distinction: epoch is correctness, abort is optimization. |
| `app/core/rpc-call.spec.ts` | 9 unit tests for LatestGate. | Written jasmine/vitest-agnostic (`describe/it/expect`, no `.rejects`). If the runner needs `expectAsync`, adapt assertions, not semantics. |
| `app/core/ticker.service.ts` | StatusBar insight ticker (§6): priority queue, 6s rotation, pause-on-hover, tips shown once ever (localStorage ledger). | Inert until something calls `post()`. Wire sources in W5. |
| `app/state/trail.store.ts` | The Trail (§1/§3.8): per-tab breadcrumb + undo/redo cursor + pins. Slices live here (NOT in WorkspaceStore — additive by design) and self-GC via effect when tabs close. | `undo()/redo()/jumpTo()` RETURN the step; caller re-traces. Don't move re-tracing into the store — it has no session handle on purpose. |
| `app/state/atlas.store.ts` | Flow Atlas (§3.1): background shallow-trace indexer (4 workers, pausable, cancellable, capped 100), plus computeds: `topFlows`, `hubs`, `eventWiring`, `overallVerifiedPct`, `reachedBy(nodeId)`. | Verify `TraceNodeVm.seam` casing at runtime (I normalize lowercase; brief says seams are lowercase, SEAM_COLORS keys are capitalized). Check one real trace in devtools. |
| `app/features/explorer/entry-deck.ts` | Workbench left column: keyboard listbox (j/k, `/`, Enter, Shift+E), kind chips, unwired-entry dot. | Uses `viewChild.required` + new control flow — matches Angular 22. No virtual scroll yet: fine to ~1k rows; `@angular/cdk` ScrollingModule is the approved dep when a PublicApi library chokes it (W2 task). |
| `app/features/explorer/stage.ts` | Center canvas, 3 altitudes (§2): System = `MapResponse.topology` (list rendering for now), Flow = existing `app-trace-node` / `app-graph-canvas`, Node = neighbors list. | GraphCanvas requires a non-null tree — guarded by `@if`. System altitude graph builder + node-altitude direction toggle are W4 TODOs (marked). |
| `app/features/inspector/inspector.ts` | Right panel: Details / LLM context / Trail sections, collapsible. LLM shows `trace.markdown()` + char/4 token estimate as an honest interim until the Render RPC migration (W4). | Copy uses `navigator.clipboard` — flaky in WebView2 without focus; swap to the clipboard plugin in W6. |
| `app/shell/trail-bar.ts` | 22px breadcrumb + ⟲⟳ + pin count. Hidden until first selection. | Currently mounted inside WorkbenchPage, not the shell — promote in W4 if desired. |
| `app/features/pages/workbench-page.ts` | Wires everything: debounced (150ms) deck→trace + trail push; node click→select + push; trail restore→re-trace WITHOUT push; dock levels (Ctrl+Shift+L, persisted); auto-starts atlas once per handle; pauses atlas while a user trace is in flight. | `(window:keydown)` shortcuts are a stopgap — move to workspace-shell in W4. `onOpenAudit()` is an intentional no-op (W4 overlay). |
| `app/app.config.ts` (edit) | Adds `/explore` route. Old routes untouched — the app stays fully usable. | |
| `src/styles.css` (edit) | Graphite palette (§4.2) in `:root` + modern dark/light; new tokens `accent-dim`/`hover`/`info` (color-mix — works because `data-vibe` sits on `<html>`); motion + overlay-shadow tokens; base font 14→13px; `@layer components` vocabulary: `.panel .list-row .chip .kbd .section-h .prose-zone .overlay-float .hairline .skeleton`. | 13px base + new palette WILL shift old pages — that's the redesign, but eyeball Console/Entries for breakage. Terminal/hacker vibes inherit the derived washes automatically. |
| `app/models/seam-colors.ts` (edit) | New seam palette (§4.2 table). | Graph + trace chips change color. Check contrast on the graph canvas bg. |

## Verify (in order) — done as of 2026-07-03, re-check after further changes

1. `pnpm check` green (real exit code). ✅ lint 0/0 · test 25/25 · build 0w/0e.
2. `pnpm server` + `pnpm dev:web` → analyze a repo → navigate to `/explore` (client-side —
   a full `page.goto`/URL-bar reload loses in-memory session state, same as it would for a
   real user; `WorkspaceStore` deliberately doesn't persist session/trace/handle):
   - j/k in the deck sweeps entries; tree updates ~150ms after you stop. ✅ Verified via
     Playwright against `tests/fixtures/MinimalApiProject`: focusing the deck and scrubbing
     produced a correct trace tree + populated Inspector, no stale/blank content, zero
     console errors. LatestGate is now wired into `TraceStore` (§ below), so this is no
     longer just "no crash" — rapid scrubbing is structurally protected against stale trees.
   - Stage: System shows topology with zero traces run; Flow tree↔graph toggles;
     clicking a tree node fills Inspector Details; Node altitude lists neighbors. (Not
     re-verified this pass beyond Flow/Tree — System/Node altitudes still worth a manual look.)
   - Trail bar appears after first selection; ⟲⟳ walk it; Ctrl+Z/Y too; crumb click jumps.
     ✅ Breadcrumb populated correctly after a scrub-selection.
   - Ctrl+Shift+L hides/restores the Inspector; survives reload (localStorage). ✅ Verified —
     now backed by `PrefsStore.dockLevel` (`devcontext-prefs` key), not the old
     `devcontext-dock` key (confirmed absent from localStorage after the toggle).
   - Statusbar equivalent not wired: check atlas progress via
     `window` devtools → network: ~4 concurrent getTrace calls after analyze, pausing
     while your own trace is in flight, stopping at ≤100. (Not re-verified this pass.)
   - Close the tab mid-indexing → atlas requests stop (GC effect cancels). (Not re-verified.)
3. Old routes (`/entries`, `/trace`, `/graph`) still work, restyled by the new tokens.
   (Not re-verified this pass — the consolidation commit `0947011` touched several of these
   files for unrelated gap-fixes; `pnpm check`/build passing is the only signal so far.)

## Next steps (the waterfall, from `ui-ux-redesign-proposal-fable.md` §10)

**Stop — this list is stale. Read `AGENTS.md`'s "F — Fable Workbench Redesign" section instead;**
it has the accurate, currently-maintained per-stage status. As of the last update: W0 and W1 are
done, W3 is mostly done (only the omnibox-onto-`runLatest` and analyze-stream-cancel-audit items
remain), W2 is a partial seed, W4-W7 haven't started. The list below is kept only as a reminder of
the overall shape — do not treat its checkmarks as current.

1. ~~W3 remainder~~ / ~~Finish W0~~ / ~~W1~~ — see AGENTS.md.
2. **W4:** URL state on `/explore`, Esc-ladder, audit-table overlay, Render-RPC LLM
   section, omnibox, export drawer + From Trail (pins are already collected!),
   old-route redirects, delete superseded sections.
5. **W5:** statusbar segments + ticker wiring (`TickerService.post` from SessionStore
   analysis events, insights, AtlasStore), Home Top Flows (`atlas.topFlows()`),
   Atlas page (event board = `atlas.eventWiring()`, hubs = `atlas.hubs()`).
6. **W6:** Tauri (sidecar, dialog picker, no-flash, shortcut interception — §7).

## Design intents you must not "fix"

- **No fabricated metrics.** Inspector shows real in/out degree only. Never add LOC/CC
  placeholders (proposal §11 #1).
- **Selection is the only state that moves**; trail restores never push (would fork history).
- **Content-preserving loading**: dim + `.hairline`, never unmount→spinner→remount.
- **Panels never get shadows/radius**; only `.overlay-float` things do.
- **Tabs own Ctrl+1-6.** Dock is Ctrl+Shift+L + future drag — no digit chords (I11's
  conflict, §11 #5).
