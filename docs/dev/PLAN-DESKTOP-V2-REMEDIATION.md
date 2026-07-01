# PLAN — Desktop V2 Remediation (skeleton hardening before P4)

> **Branch:** `feat/desktop-v2`
> **Created:** 2026-06-30
> **Read with:** `docs/dev/PLAN-DESKTOP-V2.md` (master plan) and `docs/dev/REVIEW-DESKTOP-V2-P0-P3.md` (P0–P3 self-review)
> **Why this exists:** P0–P3 shipped all 8 views, but the *skeleton* underneath them is not sound. This plan fixes the foundation (unify the shell, make the app truthful, route every op through one busy/error truth, bake the core loop in) **before** P4 feature work. After this, resume the master plan at P4.
> **Resume:** `git checkout feat/desktop-v2` → confirm green (§ Gates) → start at the lowest unchecked phase below.

---

## 0. Findings this plan addresses

Verified against the live code on `feat/desktop-v2` (2026-06-30). Severity: **P0** = breaks the skeleton / lies to the user, **P1** = core loop / consistency, **P2** = polish.

| # | Severity | Finding | Evidence |
|---|---|---|---|
| F1 | P0 | Shell's contextual sidebar is **never used** — 240px dead column; every view reinvents its own chrome | `app-shell.ts:47-49` `<ng-content select="[sidebar]"/>`; `app.ts:7` mounts `<app-app-shell/>` with no projected content |
| F2 | P0 | Title/status bars likely **don't span** the grid — `col-span-3` is on a `display:contents` host (no box → span ignored) | `title-bar.ts:24`, `status-bar.ts:27` `host: { style: 'display: contents' }`; `app-shell.ts:30,55` `class="col-span-3"` on host |
| F3 | P0 | Connection dot **always shows Offline** — `ConnectionStore.start()` is never called | `connection.store.ts:23` `start()` defined; no caller anywhere |
| F4 | P0 | Repo validation is **fake** — `ping()` (server liveness) reported as "Repository found" | `source-view.ts:182-194` |
| F5 | P0 | `cleanup` / `dryRun` controls are **inert** — collected but never sent; server cleanup wiring unreachable | `source-view.ts:206-216` sends only `{path,depth,detail,noRoslyn}`; `devcontext-api.ts:18-24` `AnalyzeSpec` has no `cleanup`; proto has `cleanup` (field 6) but **no** `dryRun` |
| F6 | P0 | Cache "Clear" button **cancels the current op** instead of clearing cache | `cache-view.ts:118-123` |
| F7 | P1 | Only `analyze` flows through `ActivityService` — all other RPCs are fire-and-forget with **no busy + no error handling** | `stats-view.ts:194`, `document-view.ts:106`, `browse-view.ts:131-147`, `trace.store.ts:96` |
| F8 | P1 | Core loop dead-ends — Entries → `/trace` lands on a **placeholder** | `trace-view.ts` (placeholder); `entries-view.ts:131-136` navigates there |
| F9 | P1 | No desktop **folder/`.sln` picker**; export is **clipboard-only** (no save) | `source-view.ts` (typed input only); `document-view.ts:129-137` |
| F10 | P1 | Double title bar risk — `tauri.conf.json` omits `decorations` (defaults true) while a custom title bar renders | `tauri.conf.json:12-24`; `title-bar.ts:7` `data-tauri-drag-region` |
| F11 | P2 | `ThemeService` **never instantiated** (only dead `vibe-switcher`/`graph-canvas` inject it) → no theme switching; app locked to `:root` default | `theme.service.ts`; grep shows no live injector |
| F12 | P2 | Dead code still present: `features/overview/overview.ts`, `features/vibe-switcher/`, `ui/graph-canvas/`, Cytoscape dep | filesystem |
| F13 | P2 | `KIND_LABELS`/`KIND_ICONS` duplicated & drifting between `entries-view.ts:11-27` and `view-models.ts:62-78` | two sources of truth |
| F14 | P2 | Browse search fires an RPC **per keystroke**, no debounce, no error catch | `browse-view.ts:131-147` |
| F15 | P2 | Overview "Stats" card is thin/redundant (duplicates style badge); topology is text chips, not the planned boxes/arrows | `overview-view.ts:47-82` |
| F16 | P2 | Stage stepper lives only in Source; Stats never fills **live** during analyze (plan P3.2) | `stats-view.ts`; `source-view.ts:71-73` |
| F17 | P0 | **IA reads as 8 isolated, equal steps** — but 7 destinations are useless before analysis; rail is always fully enabled, so the analyze-first flow isn't communicated | `app-shell.ts:14-23` (8 peer nav items, no gating) |
| F18 | P1 | **Busy state doesn't propagate** — Analyze button, nav, and per-view actions stay enabled during analysis; progress feels inert | `source-view.ts:40` only disables its own button; nav/views never read busy |
| F19 | P1 | **Stats UX is weak** — bare "Load stats" with no skeleton/placeholder; no auto-load on visit | `stats-view.ts:171-178,194-199` |
| F20 | P0 | **Deletion safety** — any clone/repo deletion must be limited to clones *DevContext created*; never touch user-owned local paths | clone tracking not yet modeled; `cache-view.ts` |
| F21 | P1 | **Landing page not intuitive** — intake reads as a bare input; the "point at a repo" hero, picker, and recents should guide first use | `source-view.ts:16-90` |

---

## 1. Phase overview

```
R0 ── Shell unification + IA flow + landing (skeleton)         [P0]
R1 ── Make it truthful (trust pass; clone-scoped deletion)     [P0]
R2 ── One busy/error truth + busy disables controls            [P1]
R3 ── Bake the core loop in (minimal Trace)                    [P1]
R4 ── Desktop affordances (picker, save, window chrome)        [P1]
R5 ── Consistency + cleanup (dead code, theme, stats, dedupe)  [P2]
── then resume master plan at P4 (Trace polish, vibes, a11y) ──
```

Each phase ends green against § Gates and is independently committable.

---

## 2. R0 — Shell unification

**Goal:** One consistent view scaffold. Kill the dead sidebar column, fix the grid spans, and give every route the same skeleton: contextual sidebar (optional) + content header + single scroll body. This is the "clean skeleton with first features baked in."

### R0.0 — Verify & fix the grid frame (F2)
1. Run the app (§ Smoke). Confirm whether title/status bars span all 3 columns.
2. If they don't: either move `class="col-span-3"` onto the inner `<div>` of `title-bar.ts`/`status-bar.ts` (and drop `display:contents`), **or** keep `display:contents` and add `style="grid-column: 1 / -1"` to the inner div. Prefer dropping `display:contents` and letting the host be the grid item:
   - Set `host: { class: 'col-span-3' }` (host is the grid item) and remove the `display:contents` style; ensure host stretches (`block`/`h-full`).

### R0.1 — A unified `ViewFrame` (F1)
Create `src/app/shell/view-frame.ts` — the one scaffold every route uses:
```
<app-view-frame>
  <ng-container sidebar> … contextual nav/filters … </ng-container>   // optional
  <ng-container header>  … title + actions …        </ng-container>   // optional
  … main content (single overflow-auto region) …
</app-view-frame>
```
- Internally renders: optional sidebar (projected), a consistent header row, and one `min-h-0 overflow-auto` body.
- **Decide the sidebar mechanism.** The shell-level `<aside>` + content-projection across a `<router-outlet>` is awkward in Angular. Recommended: **drop the shell `<aside>`** and let `ViewFrame` own the contextual sidebar *inside* each routed view. The shell grid becomes `48px 1fr` (activity rail + content); `ViewFrame` draws its own sidebar column when a view provides one, and collapses it when not. This removes the dead column and keeps layout logic in one place.
- Migrate all 8 views to `ViewFrame`:
  - **Overview / Stats / Cache:** header = title + badges/refresh; sidebar = section jump-list (Overview/Stats) or none (Cache).
  - **Entries:** sidebar = kind filter list + search; body = entry board.
  - **Browse:** sidebar = search + kind/tag facets; body = node table; detail as a right panel or bottom sheet (keep one pattern).
  - **Document:** sidebar = section rail (already w-56) → move into ViewFrame sidebar; header = token total + budget + render/copy/save.
  - **Source:** keep centered launcher when no session (special case — `ViewFrame` with no sidebar/header).

### R0.2 — Empty-state unification (F1)
- Today each view hand-rolls "Analyze a repo to …". Add a single `<app-empty-state icon label/>` (or a `ViewFrame` input `[requiresSession]`) so the no-session message is consistent and DRY.
- Optional: a route guard that redirects `/overview…/cache` → `/` when `!session.ready()` (master plan asked for this). If kept as empty states, make them consistent and include a "Go to Source" action.

### R0.3 — Navigation reflects the analyze-first flow (F17)
The activity bar today is 8 always-enabled peers, but only **Source** (and **Cache**, which is session-independent) work before analysis. Make the IA read as a flow, not 8 isolated tools:
1. **Group the rail.** Top group: **Source** (the entry) + **Cache** (recents/clones, works anytime). A divider. Then the **lens group** over the analyzed repo: Overview · Entries · Browse · Trace · Document · Stats.
2. **Gate the lens group** on `session.ready()`: dim + `aria-disabled` + tooltip "Analyze a repo first" until ready; clicking does nothing (or routes to `/`). Removes the "why is this clickable / empty" confusion.
3. **Auto-advance on success.** When analysis completes, navigate to `/overview` so the flow's next step is obvious (the rail's lens group lights up at the same moment).
4. Keep the active-route highlight; make the gated/enabled distinction visually unmistakable.

### R0.4 — Landing page that guides first use (F21)
Reshape `source-view.ts` so the empty/no-session state is a real landing, not a bare input:
- **Hero:** product name + one-line value ("Point at any .NET repo to orient, browse, and trace") with a large, focused input as the primary action.
- **Two obvious ways in:** a "Browse…" picker button (R4.0) *beside* the input, and **recents as cards** (path + label + last-used) directly below — recents are the fastest path for return users, so make them prominent, not a small chip row.
- **Advanced options** stays collapsed but discoverable.
- Keep the centered composition only until a session exists; once `ready()`, Source is just another `ViewFrame` route (or auto-advance leaves it behind).

**Exit gate:** No empty 240px column; bars span correctly; all 8 routes share the `ViewFrame` skeleton; the rail visibly gates the lens group until a repo is analyzed and auto-advances to Overview on success; landing page leads with hero + picker + recents; `pnpm check` green; shell invariant holds (no body scroll at 900×600 min size).

---

## 3. R1 — Make it truthful

**Goal:** Every visible control/state reflects reality. Remove or wire each lie.

### R1.0 — Connection status (F3)
- Call `ConnectionStore.start()` once at boot. Cleanest: inject `ConnectionStore` in `AppShell` (or `App`) constructor and call `start()`, or add an `provideAppInitializer` in `app.config.ts`.
- Verify the dot flips green within one poll when the server is up.

### R1.1 — Repo validation: real or removed (F4)
- **Option A (recommended, honest + cheap):** classify input locally without claiming repo existence: detect `local path` vs `github url` vs `invalid`, and show a neutral hint ("Local path", "GitHub repo — will clone"). Do **not** say "Repository found" unless actually verified.
- **Option B (truthful check):** add a server `ValidateSource` RPC (or reuse clone dry-run) that does `git ls-remote` / path-exists, and surface real states (valid / not-found / private / rate-limited). Heavier; defer unless wanted.
- Pick A now; leave a `// TODO ValidateSource` seam for B.

### R1.2 — Wire `cleanup`, drop fake `dryRun` (F5)
- Add `cleanup?: 'auto' | 'keep'` to `AnalyzeSpec` (`devcontext-api.ts`) and pass `cleanup` into `client.analyze({... cleanup})`.
- Pass `this.cleanup()` from `source-view.ts:analyze()`.
- Confirm end-to-end: client → proto field 6 → `DevContextGrpcService` → `AnalysisSession.DisposeAsync` honors `keep`.
- **`dryRun`:** the proto has no such field. Either remove the checkbox, or add `dry_run` to the proto + server (additive) and wire it. Recommend **remove** for now (the skeleton shouldn't show inert controls); add back in P5 if real.

### R1.3 — Cache page honesty + clone-scoped deletion (F6, F20)
- Fix "Clear": it must not call `session.cancel()`. Until a real clear-cache RPC exists, either (a) remove the button, or (b) wire it to a new `ClearCache` RPC / `CloseSession`. Recommend remove + leave `// TODO ClearCache` until P5.
- The "Clone management" card claims auto/keep behavior — make it reflect the *actual* `cleanup` choice now that R1.2 wires it (e.g., read from session/last spec).
- **Deletion safety (hard rule):** the app may only ever delete clones **DevContext created** — never a user's local path or pre-existing repo. To enforce:
  - Track clones server-side with an explicit `IsManagedClone`/origin flag (the `EngineResult.GitClonePath` is only set for repos we cloned — make that the single source of truth).
  - Any "delete clone" UI must operate on that managed-clone list only; local-path sessions expose **no** delete affordance.
  - "Remove from recents" stays list-only (never touches disk) — keep that distinction explicit in the UI copy.

**Exit gate:** No control shows a state it can't back up; connection dot works; `cleanup=keep` verifiably leaves the clone on disk (manual check); deletion is impossible for non-managed (local) paths; `pnpm check` + server tests green.

---

## 4. R2 — One busy/error truth

**Goal:** The status bar is genuinely the single source of "busy," and **no RPC failure is silent.**

### R2.0 — Route secondary ops through `ActivityService` (F7)
- Add a lightweight helper on `ActivityService` (or a `withActivity(label, fn)` wrapper) so `getStats`, `render`, `searchNodes`, `getTrace`, `getNode`, `getNeighbors` set busy/label while running and clear on completion.
- Keep analyze as the cancellable long op; secondary ops can be non-cancellable but must still show busy + clear.

### R2.1 — Global error surfacing (F7)
- Wrap every RPC call site (or centralize in `DevContextApi`) so failures call `activity.setError(...)` and/or raise a toast (`ui/toast` exists). No more bare `await api.x()` with no catch.
- Add a tiny `ToastService` mount in the shell if not already mounted; show RPC errors there.

### R2.2 — Busy disables controls + livelier progress (F18)
- **Busy must propagate.** Bind `session.busy()` / `activity.state()==='busy'` to disable: the Analyze button, the picker, recents, advanced options, and the lens-group nav (already gated by R0.3, but also block re-entrancy while busy). No interactive control should stay clickable while an op runs.
- **Make progress feel alive:** the stage stepper should advance on real `ProgressEvent` stage transitions (not just percent thresholds), show the current stage label + percent prominently, and offer a visible **Cancel** while busy. Status bar already binds `ActivityService` — ensure stage/percent update smoothly and the busy dot animates.
- Re-clicking Analyze while busy must cancel-then-restart (already half-wired via `OperationController`) — surface that as intentional, not as a second concurrent run.

**Exit gate:** During analyze, no stale-enabled buttons; progress shows live stage + percent + a working Cancel; loading Stats/Document/Browse shows busy in the status bar; killing the server mid-action shows an error (toast + status), not a silent no-op; `pnpm check` green.

---

## 5. R3 — Bake the core loop in (minimal Trace)

**Goal:** The skeleton's primary drill-down must work. Implement a *right-sized* Trace now (the master plan's P4.0 narrative tree), without the fancy graph.

### R3.0 — Trace view (F8)
Replace `features/trace/trace-view.ts` placeholder with a real view backed by the existing `TraceStore` (already complete — `trace.store.ts`):
- **Focus:** read focus from `TraceStore.focus()` (set by Entries/Browse click). Add a focus picker (searchable combobox over entries + `SearchNodes`) for direct use.
- **Narrative tree:** recursive component rendering `TraceNodeVm` — seam · title · file:line · salient · approx/resolved badge · truncation/omitted note. (`toTraceVm` already maps children.)
- **Controls:** depth + detail selectors wired to `TraceStore.setDepth/setDetail` (instant re-trace, render-many).
- **Defer** the SVG path graph to master-plan P4 (note it).
- Wire `entries-view` and `browse-view` "trace" actions to land here meaningfully.

**Exit gate:** Click an entry → Trace renders a narrative tree; changing depth/detail re-renders without re-analysis; not-found and empty states handled; `pnpm check` green.

---

## 6. R4 — Desktop affordances

**Goal:** Make it feel native.

### R4.0 — Folder / `.sln` picker (F9)
- Add `@tauri-apps/plugin-dialog`; add a "Browse…" button in Source that opens a folder or `.sln`/`.csproj` picker and fills the path input. Gate on Tauri runtime (no-op/hidden in plain web dev).

### R4.1 — Save-to-file export (F9)
- In Document, add "Save" using `@tauri-apps/plugin-fs` / dialog save — write the rendered content (markdown/JSON) to disk. Keep copy-to-clipboard too.
- While here: make the budget slider *do* something (trim/sort sections to fit budget, or at least warn when over) and let section toggles re-render or recompute live (today toggling needs a manual Render).

### R4.2 — Window chrome decision (F10)
- Decide custom vs native title bar:
  - **Recommended (fits the "vibe" product):** `tauri.conf.json` → `"decorations": false`, and add min/maximize/close controls to `title-bar.ts` via `@tauri-apps/api/window`.
  - **Simpler:** set `"decorations": true` and delete the custom title bar (move connection dot into the status bar).
- Pick one; eliminate the double bar.

**Exit gate:** Folder picker fills the path; export saves a file; exactly one title bar; `pnpm check` + `cargo build` (tauri) green.

---

## 7. R5 — Consistency + cleanup

**Goal:** Remove dead weight, dedupe, and tidy the patterns.

1. **Theme wiring (F11):** instantiate `ThemeService` at boot (inject in shell) so `data-vibe`/`data-theme` are set. Decide vibe-switcher placement (title bar menu or a small settings affordance) and mount it, **or** consciously defer switching to master-plan P4.1 and document that the app is dark-modern-locked until then.
2. **Delete dead code (F12):** `features/overview/overview.ts`, `features/vibe-switcher/` (if not mounted), `ui/graph-canvas/`, and the Cytoscape dependency in `package.json`. Confirm no imports remain.
3. **Dedupe kind maps (F13):** single source in `models/view-models.ts`; `entries-view` imports it. Add `KIND_ICONS` there too.
4. **Debounce + guard Browse search (F14):** 250–300ms debounce on the query effect; wrap in try/catch → error toast (covered by R2.1).
5. **Overview depth (F15):** drop the redundant "Stats" card or replace with real counts (nodes/edges/entries from summary); upgrade topology to simple SVG boxes/arrows (master-plan intent) — or explicitly defer to P4 with a note.
6. **Stats UX (F19, F16):** auto-load stats on first visit to `/stats` (no mandatory click); show a **skeleton/placeholder** while loading instead of a bare button; keep an explicit Refresh. Stretch (or master-plan P3.2): fill the Code panel **live** during analyze by subscribing to the progress stream.
7. **`[class.x/opacity]` bindings:** audit `[class.bg-accent/15]` / `[class.bg-accent/10]` in `stage-stepper.ts`, `browse-view.ts`, `entries`/trace — confirm the active/selected highlight actually applies (slash-opacity utility inside a `[class.…]` binding). If flaky, switch to a computed class string or `[ngClass]`.

**Exit gate:** No dead imports; one kind map; theme decision documented; `pnpm check` green; bundle not larger than before (Cytoscape removed should shrink it).

---

## 8. Gates (every phase, every commit)

```powershell
# .NET
dotnet build DevContext.slnx -clp:ErrorsOnly        # 0 warnings, 0 errors
dotnet test tests/DevContext.Server.Tests             # all green

# App (from src/DevContext.App)
pnpm check                                            # lint + test + build
pnpm gen:proto                                        # only after proto changes
node --experimental-strip-types scripts/grpcweb-smoke.mts  # live transport smoke

# Tauri (after R4)
cargo build --manifest-path src/DevContext.App/src-tauri/Cargo.toml
```
**Shell invariant:** body never scrolls; nothing overflows the viewport at the 900×600 min window size.

---

## 9. Smoke test

```powershell
# Terminal 1
pushd src\DevContext.App; pnpm server
# Terminal 2
pushd src\DevContext.App; pnpm dev:web   # open http://localhost:4200
```
1. Shell: one title bar, activity rail, **no empty sidebar column**, status bar; connection dot **green**.
2. Before analysis: lens group (Overview…Stats) is **visibly gated**; landing leads with hero + picker + recents.
3. Source: type `C:\code\DevContext2` (or pick a folder) → Analyze → buttons/nav **disable**, stage stepper advances on real stages, Cancel works → on success **auto-advances to Overview** and the lens group lights up.
4. Secondary loads (Stats/Document/Browse) show busy; **Stats auto-loads with a skeleton**, not a bare button.
5. Entries → click an entry → **Trace renders a narrative tree**; change depth/detail → re-renders.
6. Kill the server mid-action → error toast (not silence).
7. Advanced options → `cleanup=keep` → clone remains after exit; a **local path session offers no delete affordance** (only managed clones are deletable).

---

## 10. Resume / handoff

- Work phases **R0 → R5 in order**; each is committable at its exit gate.
- After R5, update `docs/dev/PLAN-DESKTOP-V2.md` §12 resume pointer and continue the master plan at **P4** (Trace path-graph + vibes/light modes + a11y) and **P5** (engine wiring breadth + eval).
- Keep `docs/dev/REVIEW-DESKTOP-V2-P0-P3.md` for history; this file supersedes its "resume" section.

### Session log
- 2026-06-30 — Plan created from a full code review of the P0–P3 implementation. Nothing executed yet. Start at R0.0.
