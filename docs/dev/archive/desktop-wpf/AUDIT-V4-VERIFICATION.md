# Audit & Verification — Desktop V4 (U1–U5) + Engine E2–I9

> **Date:** 2026-07-02 · **Branch:** `develop` @ `f8ec7a0` · **Author:** verification pass
> **Scope:** independent verification of `HANDOVER-DESKTOP-V4.md`, static analysis of the UI + engine,
> conflicting-branch feature check, and a driven fix of the urgent/high/front-end defects.
> **Supersedes the "all ✅ done" framing of the V4 handover** where noted below.

---

## 0. TL;DR

The handover said "U1–U5 ✅ / E2–I9 ✅" with "pnpm check green". **The UI did not actually build on
`develop`** — a wrong-depth import in `navigation-rail.ts` broke `ng build`. The prior "green" was a
measurement artifact: every gate was run as `<cmd> 2>&1 | tail -N`, so the recorded exit code was
`tail`'s (0), not the tool's. On top of that, **three "delivered" UI features were broken or invisible
in the shipped (routed) app**, and the live Settings route was stale.

All of that is now **fixed** (build green, features wired). The engine work
(E2/E5/I8/I10.3/A-F14/A-F15/I9) verifies clean against the test suite. U3 facets remain correctly
deferred — but the remaining work is much larger than "just proto changes" (see §4).

> **Process lesson:** never verify through a pipe. `cmd | tail` reports the pipe's exit status, not
> the command's. Use `cmd > log 2>&1; echo $?` or `set -o pipefail`.

---

## 1. Baseline verification — GREEN

Run on `develop` @ `f8ec7a0`, all from a cold cache:

| Gate | Command | As-received (`develop` @ f8ec7a0) | After this pass |
|---|---|---|---|
| UI lint | `pnpm lint` (`ng lint`) | ✅ pass | ✅ pass |
| UI unit tests | `pnpm test` (vitest) | ✅ 7/7 (3 files) | ✅ 7/7 |
| UI typecheck | `tsc --noEmit -p tsconfig.app.json` | ❌ **3 errors** | ✅ clean |
| UI build | `pnpm build` (`ng build`) | ❌ **FAILED** | ✅ complete |
| Engine build | `dotnet build DevContext.slnx` | ✅ 0 warn / 0 err | ✅ |
| Engine tests | `dotnet test --filter Category!=Eval` | ✅ 383 pass / 3 skip (Core 307+3, Desktop 64, Server 12) | ✅ |
| Engine eval | `dotnet test --filter Category=Eval` | ⚠️ **not run** — needs populated `eval-repos/` + network | ⚠️ still owed |

**The UI did not build as received.** `ng lint` passes without full module resolution, so it did not
catch it; the unit tests don't import the broken files; and the handover's "tsc clean / build OK" were
recorded through `<cmd> | tail`, capturing `tail`'s exit code. Three files never compiled — see F0.

---

## 2. Defects found & FIXED this pass (front-end, high value, low risk)

| # | Severity | Defect | Root cause | Fix |
|---|---|---|---|---|
| F0 | **Critical** | **The UI did not build.** `ng build` / `tsc` fail — three "delivered" files never compiled. | (a) `shell/navigation-rail.ts` imported `../../state/…` / `../../ui/…` (two levels → `src/state`, nonexistent) instead of `../…` (sibling `workspace-shell.ts` had it right). (b) `narrative/section-console.ts` imported `afterRender` from `@angular/core`, which was **removed in Angular 22** (renamed `afterEveryRender`). (c) `features/graph/graph-view.ts` (orphaned) imported `../../shell/view-frame`, a file deleted with the old architecture. | (a) corrected the import depth; (b) switched to `afterEveryRender`; (c) deleted the dead+broken `graph-view.ts`. Build now green. |
| F1 | **High** | **Synced Lens (U2/P3) invisible in the shipped app.** A headline "delivered" feature never rendered on any route. | `SectionLens` was only placed in the orphaned `narrative-canvas.ts`. `overview-page.ts` *imported* it but never put `<app-section-lens/>` in its template. | Wired `<app-section-lens/>` into `trace-page.ts` (its natural home — trace focus drives it); removed the dead import from `overview-page.ts`. |
| F2 | **High** | **Graph view blank on first trace.** Canvas only appeared after the user changed the depth dropdown. | `section-graph.ts` gated the canvas with `@if (renderKey())` where `renderKey` starts at `0` → falsy → not rendered. The remount key was also unnecessary (the canvas already re-renders on `maxDepth` via its own effect). | Removed the `@if (renderKey())` wrapper and the `renderKey`/`onDepthChange` machinery. |
| F3 | **Medium** | Graph depth `[(ngModel)]="graphDepth"` bound two-way to a **signal object** (banana-in-box reassigns the signal). Latent bug. | `[(ngModel)]` on a `WritableSignal`. | Changed to `[ngModel]="graphDepth()" (ngModelChange)="graphDepth.set(+$event)"` (matches the trace section's pattern). |
| F4 | **Medium** | **Trace focus suggestions dropdown floats open permanently**, overlapping content — even with an empty query it showed 10 rows and never closed. | `section-trace.ts` had no focus/blur gating; empty query populated 10 rows. | Added a `focusOpen` signal; dropdown now shows only while the input is focused (150 ms blur delay so clicks register). |
| F5 | **Medium** | **Entries table arrow-keys jump by 2.** | Both the `<table>`'s `(keydown)` (`onTableKey`) **and** a `window:keydown` `HostListener` (`onGlobalKey` → `onTableKey`) fired for the same event when focus was in the table. | Removed the redundant `onGlobalKey` window listener (+ its now-unused `HostListener` import). The table-level handler already covers in-table nav. |
| F6 | **Medium** | **Insights view is off the design system** — hardcoded `red-500/amber-500/blue-500` instead of semantic tokens; and `insight.detail` was mapped into the view-model but **never rendered**. Stale "Go to source" link label. | `insights-view.ts` was built to a different color convention and dropped the detail line. | Switched severities to `danger/warn/accent` tokens; render `insight.detail`; relabeled the link to "Go to overview". |
| F7 | **Medium** | **Live Settings route was stale/misleading**: GitHub links pointed at the wrong org (`anomalyco/DevContext` vs the real `shaahink/DevContext2`); two `onclick="alert('…coming with I8')"` placeholder buttons (I8 has shipped); an inverted "Use Roslyn semantic tier" checkbox bound to `noRoslyn`. | The U4 "release polish" About was written into `section-settings.ts` (`SectionSettings`), which is **dead** (only used by the orphaned `narrative-canvas`). The routed `/settings` uses the older `settings-view.ts`. | Fixed all links to `shaahink/DevContext2`, added a "Check for updates" link, removed the `alert()` placeholders (honest "managed automatically" note), fixed the Roslyn toggle to `useRoslyn` (correct polarity), switched the server dot to `bg-success`/`bg-danger`. |

All fixes verified against `ng lint` (green) and `ng build`.

---

## 3. Root problem: two parallel UI architectures, one of them dead

The previous iterations left **two complete, overlapping UI architectures** in the tree:

- **Live (routed):** `shell/workspace-shell.ts` + `shell/navigation-rail.ts` + `features/pages/*` +
  `features/narrative/section-*.ts`. This is what `app.config.ts` routes to. It matches
  `UI-UX-GUIDELINES.md` (rail + routed views).
- **Dead (orphaned):** `features/narrative/narrative-canvas.ts` (the old one-scroll page) + its
  exclusive deps `shell/scroll-spy/scroll-spy.ts` and `features/narrative/section-settings.ts`
  (`SectionSettings`). Nothing routes to or imports `narrative-canvas`.

This is *why* the About polish (F7) and the Synced Lens (F1) looked "done" but weren't live — they
were wired into the dead canvas. The confusing part is that `narrative-canvas` still `import`s and
lists the live sections, so it reads like the real shell.

**Not deleted this pass** (deliberately — deleting drops the only remove-recent UI and widens the
diff): see the consolidation task in §6.

Independent orphans (present but unreferenced, future-intended): `shell/tab-strip.ts` (`TabStrip`,
the I10 multi-tab strip), `features/graph/graph-view.ts` (`GraphView`, the richer BFS graph — note
it `import`s `../../shell/view-frame` which **does not exist on develop**, so it would not compile if
wired; it is a leftover from the old architecture). `trace.store.ts#selectNode()` is defined but
never called → the Lens "node detail" card branch is currently dead (node clicks open the NodeCard
sheet via `node.store` instead).

---

## 4. U3 / E4 facets — deferred, but bigger than the handover implies

The handover frames U3 as "blocked on E4 proto changes." Reality from static analysis:

- `proto/devcontext/v1/devcontext.proto` has **no facet messages at all** (no F1 auth, F3 message
  matrix, F4 data map, F5 talks-to, F8 DI health). `MapResponse` carries topology/packages/
  aggregates/stack/surface only.
- `src/DevContext.Core/Graph/Facets/FacetDescriptor.cs` is an **orphaned stub** — it is referenced
  *only by its own file* (grep: no `FacetCatalog`, no registration, no consumer, no computation). Its
  doc-comment describes a catalog/renderer wiring that does not exist.

So U3 needs: (a) a real facet computation layer + catalog on the engine, (b) proto messages, (c)
server mapping, (d) UI. That is a multi-iteration feature, not a proto tweak. Keep it deferred, but
budget it honestly.

---

## 5. Conflicting-branch feature check (the "not missing features" ask)

`go-to/implement-iterations` and `docs/go-to-program-addendum` are a **wholesale earlier UI
architecture** — `shell/app-shell.ts` + `title-bar.ts` + `status-bar.ts` + `view-frame.ts` and
separate `browse/document/entries/overview/source/stats/trace/cache` views. A merge would *delete*
develop's current sections (the handover is right to keep them unmerged).

Feature-parity delta (old branch → develop): every capability is reproduced by the new architecture
(overview, entries, trace, graph, insights, export, console, lens, settings) **except one**:

- `features/cache/cache-view.ts` — a real Storage/cache-management view (I8). On develop, Settings →
  Storage only shows static paths (the `alert()` stubs are now removed). **Not a regression** (the old
  branch never shipped either), but if a storage manager is wanted, that file is the reference. Needs
  a Tauri file command to open folders / clear cache.

No silently-lost feature was found beyond that.

---

## 6. Continuation list (for the next agent) — prioritized

### P0 — front-end correctness / plan-goal gaps
1. **Header repo affordance (UI-UX-GUIDELINES §1).** `app-header.ts` has a dead
   `<ng-content select="[analyze]"/>` slot; `workspace-shell.ts` renders `<app-header/>` with nothing
   projected. There is **no in-app re-analyze / repo switcher** — the only path is the "New" button,
   which does a full `window.location.reload()`. Add the repo-label ▾ recents dropdown + an analyze
   entry to the header, and make "New" reset the store instead of reloading.
2. **Consolidate the dead architecture.** Delete `narrative-canvas.ts`, `section-settings.ts`
   (`SectionSettings`), and `scroll-spy.ts` — but first port the **remove-recent (×)** affordance from
   `SectionSettings` onto the Landing recents list (guidelines §7: "Recents move to Landing; the
   remove-x stays"), since that is currently the only remove UI and it lives in dead code.

### P1 — Entries view to spec (UI-UX-GUIDELINES §4)
3. Add **file:line** column (subtle, reveal-on-click) and **count badges** on the filter chips
   (`HTTP 70`). Add "has target" / "approx" quick-filter toggle chips.
4. Persist **sort in the URL** (`?sort=`); add visible hover/focus **row-action buttons** (Trace ·
   Node card · Copy route · Reveal file) — today only keyboard `Enter`/`n`/`Ctrl+C` exist.
5. **>150 rows:** `@defer` + CDK virtual scroll; sticky header.

### P1 — deep-linking / state-in-URL (guidelines §1)
6. Sync **trace focus** to `/trace?focus=X` and **entries filter/kind** to query params (URL→store on
   nav, store→URL `replaceUrl` on change). Currently `trace-page.ts` is a bare wrapper — no deep links.

### P2 — Settings substance (guidelines §7)
7. Add a **`prefs.store`** (persisted via localStorage/Tauri, schema-versioned) and actually **apply**
   Analysis defaults (depth/detail/roslyn) to new analyses. Today the Settings→Analysis controls are
   cosmetic — nothing reads them.
8. **Storage tab:** wire a Tauri file command for open-folder / per-repo cache list + sizes + clear
   (reference `cache-view.ts` on the old branch).

### P2 — Lens / graph polish
9. Either call `trace.store.selectNode()` from a node click to populate the Lens "node detail" card,
   or delete that dead branch of `section-lens.ts`.
10. Consider wiring the richer `GraphView` (BFS exploration) — but it imports a non-existent
    `shell/view-frame`; port or drop it. `section-graph.ts` (cytoscape) is the working one.

### P3 — the big deferred one
11. **U3 facets / E4** — full facet layer (engine catalog + compute + proto + server map + UI). See §4.
    Not a proto tweak; scope it as its own iteration.

### Verification debt
12. Run the **eval gate** (`Category=Eval`) against populated `eval-repos/` to confirm E5's 8 new
    archetype expectations actually pass (only the non-eval suite was verified here).
13. **Manual smoke test** the running app (the V3 plan's standing rule): `pnpm server` + `pnpm dev:web`,
    then walk every route, the g-key nav, live console, and the (now-wired) Lens on the Trace page.

---

## 7. Files touched this pass

```
src/DevContext.App/src/app/shell/navigation-rail.ts               (F0a — build break)
src/DevContext.App/src/app/features/narrative/section-console.ts  (F0b — build break)
src/DevContext.App/src/app/features/graph/graph-view.ts           (F0c — DELETED, dead+broken)
src/DevContext.App/src/app/features/narrative/section-graph.ts    (F2, F3)
src/DevContext.App/src/app/features/pages/trace-page.ts           (F1)
src/DevContext.App/src/app/features/pages/overview-page.ts        (F1)
src/DevContext.App/src/app/features/narrative/section-trace.ts    (F4)
src/DevContext.App/src/app/features/narrative/section-entries.ts  (F5)
src/DevContext.App/src/app/features/insights/insights-view.ts     (F6)
src/DevContext.App/src/app/features/settings/settings-view.ts     (F7)
docs/dev/AUDIT-V4-VERIFICATION.md                                 (this file)
```
