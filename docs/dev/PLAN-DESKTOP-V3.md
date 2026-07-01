# Plan — Desktop V3 (Terminal Instrument)

> **Branch:** `feat/desktop-v2` (continues) | **Date:** 2026-06-30
> **Supersedes:** the "complete / all green" framing of `HANDOVER-DESKTOP-V2.md`.
> **For:** the agent picking up the desktop after the first iteration was judged "a lot of basic missings."

---

## 0. Why this plan exists

The V2 iteration builds and passes `pnpm check`, but the app feels unfinished and generic. A green
gate cannot catch any of the things that are actually wrong: dead controls, fake progress, swallowed
errors, a wall of placeholder zeros, a split Human/LLM workflow that demands constant navigation, and
a visual identity cloned from GitHub. This plan fixes the foundation, gives the app a signature, and
promotes the rich engine from "hidden behind a spinner" to the centerpiece.

**Signature direction (chosen):** *full terminal / phosphor*. Monospace throughout, zero border-radius,
boot-log everything, amber-on-black, scanline texture. The engine's own telemetry becomes a first-class
**Console**. Other vibes (modern, hacker, blueprint-later) stay available via a restored switcher, but
terminal is the default character.

**Three moves this plan executes:**
1. **Make the engine visible** — a live Console streaming discover→extract→seal→score→render, settling into the report. Kills fake progress + zero-stats.
2. **Collapse Human ↔ LLM into one synced surface** — same selection, two renderings, no navigation, no manual Render.
3. **Give it a signature + fix the primitives** — terminal identity + real disabled/cursor/focus/toast underneath.

---

## 1. Audit — confirmed defects (the "inspect" deliverable)

All verified against code on `feat/desktop-v2`. `file:line` anchors included.

| # | Defect | Evidence | Bucket |
|---|---|---|---|
| D1 | `app-button` disabled is decorative-only. Custom element + `disabled:` Tailwind variants never match (`:disabled` is form-elements-only); `[attr.disabled]=""` is inert. Disabled buttons stay full-opacity and clickable. | `ui/button/button.ts:8,27` | Interaction |
| D2 | No `cursor-pointer` / `select-none` on buttons → I-beam "typing arrow", selectable labels, buttons don't feel clickable. | `ui/button/button.ts:6-8` | Interaction |
| D3 | Stage stepper percentages are hardcoded fiction (`Discovery=10…Render=90`); progress is a fixed 7-dot template, not real per-stage truth. `[class.bg-accent/15]` slash-opacity bindings may not even render. | `ui/stage-stepper/stage-stepper.ts:10-18,29-34` | Progress |
| D4 | Toast system is 100% dead: `<app-toast>` in no template; `ToastService.show()` called nowhere; component `messages` input never bound. | `shell/app-shell.ts:95`; `ui/toast/toast.ts` | Feedback |
| D5 | Errors swallowed: `runSecondary` resets to *idle* (not error) in `finally` + rethrows into `.catch(()=>{})`; clipboard/github `catch {}`. Only surface is a status-bar string auto-cleared after 5s. | `core/activity/activity.service.ts:59-68`; `features/browse/browse-view.ts:111`; `features/document/document-view.ts:81` | Feedback |
| D6 | `stats-view.loadStats()` leaves the spinner stuck forever if the RPC throws (no catch; `loading.set(false)` is after the throwing await). | `features/stats/stats-view.ts:108-117` | Feedback |
| D7 | Stats reads as placeholders: raw `?? 0` cells make a cold run look broken; **Funnel is fetched server-side but never rendered** (no funnel block in the view). | `features/stats/stats-view.ts:50-75`; `Mapping/ProtoMapper.cs:175-182` | Stats |
| D8 | Human (Overview cards) and LLM (Document markdown) are separate routes with no shared state. Document requires a manual **Render** click; getting the LLM text for the current selection means navigate-away → Render → navigate-back. | `features/overview/overview-view.ts`; `features/document/document-view.ts:68-76` | IA |
| D9 | `Render` RPC hardcodes `Format="markdown"`; the "Human (HTML)" format isn't reachable from the desktop. | `Mapping/ProtoMapper.cs:187-200` | IA |
| D10 | Default vibe is a GitHub-dark clone (`#0b0e14` base, `#4493f8` accent); no signature. Vibe-switcher was deleted, so the characterful vibes are unreachable at runtime. | `styles.css:52-68`; `HANDOVER-DESKTOP-V2.md §8` | Identity |
| D11 | Engine telemetry (streamed `ProgressEvent`, full `RunReport`) is discarded after analysis — no live log, no console, nothing to "watch the engine think." | `state/session.store.ts:56-62` (progress consumed then dropped) | Engine surface |

Severity order for fixing: **D4/D5/D1/D2** (foundation — make nothing feel broken) → **D10** (identity) → **D11/D3/D7/D6** (console + real progress + stats) → **D8/D9** (synced lens).

---

## 2. Phase plan

Each phase ends in something the user can *feel*. Gate after each: `pnpm check` green + manual run.

### P0 — Foundation: nothing feels broken (no new features)

- **Rewrite `app-button` as a real control.** Render a native `<button>` internally (or host on a `<button>` selector), so `:disabled` works; add `cursor-pointer`, `select-none`, `disabled:cursor-not-allowed`, real `disabled` attribute. Audit every `(click)` consumer for actual disabled wiring.
- **Mount the toast.** Put `<app-toast [messages]="toast.messages()" />` in the shell template; expose `ToastService` on the shell; verify stacking + auto-dismiss.
- **Route every catch to feedback.** `runSecondary` should set `error` state (not silently idle) and surface via toast; replace `.catch(()=>{})` in browse/document/github with a toast call; fix `stats-view` so a throw clears `loading` and shows an error (try/finally). Establish the rule: *no empty catch in a view — it either retries or it toasts.*
- **Kill the fake stepper** (its replacement arrives in P2's Console). Until then, the status bar shows the real streamed `stage`/`message`, no fabricated percent dots.

**Gate:** disabled buttons are visibly disabled and unclickable; a forced RPC error raises a toast; cursor is a pointer over every button.

### P1 — Signature identity (terminal default + switcher)

- **Make `terminal` the default vibe**; harden it: amber-on-black phosphor, zero radius, mono everywhere, a subtle scanline/grid texture layer (CSS `repeating-linear-gradient` overlay, `prefers-reduced-motion`-safe, no animation cost), focus = phosphor glow not blue outline.
- **Restore a vibe switcher** in the title bar (the old `vibe-switcher` was deleted) — small affordance cycling terminal / modern / hacker, persisted (the `ThemeService` localStorage path already exists).
- **Typographic system pass:** one mono stack, a tabular-numbers utility for all stat/percent columns, consistent density tokens. Replace ad-hoc `text-[9px]/[10px]/[11px]` with a small scale.
- **Boot affordance:** the Source/landing screen adopts the boot-log framing (`DEVCONTEXT v2 // …`) so the identity is set from first paint.

**Gate:** app opens in amber terminal skin with character; switcher flips vibes live and persists across reload.

### P2 — Live Console + real Stats (make the engine visible)

- **Console surface** (new route `/console`, and an embedded strip on Source during analyze): stream the engine's `ProgressEvent`s as a scrolling boot-log (`> discover…OK 142 files`, `> extract…OK 37 extractors`, per-stage timing as it completes). The store currently consumes progress then drops it (`session.store.ts:56-62`) — instead append to a `signal<LogLine[]>` the Console renders.
- **Settle into the report.** When analysis completes, the same surface shows the `RunReport`: stages waterfall, extractor timings + skips, seams, funnel. **Merge Stats into this** (the user explicitly suggested console+stats merged).
- **Render the funnel** (D7): types discovered→included, raw→rendered tokens vs budget — as a real bar, the one honest progress visualization in the app.
- **De-zero cold cache** (D7): show hit-rate and label a cold run ("cold run — no cache reuse") instead of four `0` cells that read as broken.
- **Real progress** replaces D3 entirely: the Console *is* the progress; the status bar mirrors the latest line.

**Gate:** analyzing a repo shows a live streaming log; after completion the same page is a readable report with a populated funnel and no naked-zero wall.

### P3 — Synced Human ↔ LLM lens (kill the navigation tax)

- **Unify Overview + Document into one lens** driven by a single selection. Layout: structured **Human** pane + **LLM** pane (split, or a toggle that preserves selection). Selecting a node/section in Human immediately shows its exact rendered markdown in LLM — no manual Render, no route change, always in sync.
- **Auto-render on selection** using the existing `Render` RPC (section/focus scoped); debounce; cache per selection.
- **One-keystroke copy-for-LLM** from anywhere in the lens (current `copyContent` exists but is buried in Document).
- **(Optional) wire the HTML format** (D9) if a richer Human rendering is wanted — server `Render` would stop hardcoding `Format="markdown"`.

**Gate:** pick any entry/section; the LLM text for exactly that appears beside it instantly; copy is one key; zero navigation to move between human and llm.

---

## 3. Out of scope / deferred (carried from V2 §8)

- Tauri folder/`.sln` native picker + save-to-file (`@tauri-apps/plugin-dialog`/`-fs`).
- Custom title-bar window controls (decorations are off).
- SVG path graph in Trace.
- axe-core a11y pass (do a manual keyboard sweep in P0/P1 instead).
- Blueprint vibe (offered but not chosen; can be added to the switcher later).

---

## 4. Gates (run after every phase)

```powershell
dotnet build DevContext.slnx -clp:ErrorsOnly      # 0/0
dotnet test tests/DevContext.Server.Tests          # green
pushd src\DevContext.App; pnpm check; popd         # lint + tests + build
# then RUN it — pnpm server + pnpm dev:web — and look at it
```

The lesson from V2: **a green gate is necessary, not sufficient. Every phase must be eyeballed in the running app before it's called done.**

---

## 5. Session log & resume state (for the next agent)

> Update this section every working session. It is the single source of truth for "where are we."

### Status board

| Phase | State | Notes |
|---|---|---|
| **P0 Foundation** | ✅ done | Live-eyeballed, optional guard added. See changes below. |
| **P1 Identity** | ✅ done | Terminal default, scanline, switcher, type pass. See changes below. |
| P2 Console + Stats | ⬜ not started | Next up. Start points below. |
| P3 Synced lens | ⬜ not started | |

### P0 — what changed (2026-06-30)

- **D1/D2 — `ui/button/button.ts`:** removed the dead `disabled:` Tailwind variants (never matched on a custom element); now binds `pointer-events-none` + `opacity-40` off `disabled()`, sets `tabindex=-1` + `aria-disabled` when disabled, and added `cursor-pointer select-none` to the base. Disabled buttons are now genuinely un-clickable and dimmed; cursor is a pointer.
- **D4 — toast mounted:** `shell/app-shell.ts` imports `Toast`, exposes `ToastService` as `toast`, renders `<app-toast [messages]="toast.messages()" />`. Also fixed `ui/toast/toast.ts` — it had **three duplicate `[class.text-accent-ink]` bindings** that would have broken the build the moment it was first compiled (it was never imported before); moved that class to the static list.
- **D5/D6 — feedback funnel:** `core/activity/activity.service.ts` now injects `ToastService`; `setError()` raises a toast; `runSecondary()` **catches → surfaces → swallows** (returns `T | undefined`) so callers' loading flags always clear (fixes the stuck stats spinner) and no view needs an empty catch. Removed the now-dead `.catch(()=>{})` in `features/browse/browse-view.ts`. `features/document/document-view.ts` `copyContent()` now toasts success/failure instead of swallowing.
- **D3 — fake stepper killed:** deleted `ui/stage-stepper/` (hardcoded fake percents). `features/source/source-view.ts` now shows an honest indicator: spinner + real streamed `activity.label()`/`stage()` + a real progress bar driven by the engine-reported `activity.percent()` (only shown when `> 0`).
- **P0.3 guard:** `features/stats/stats-view.ts` Refresh button now has `[disabled]="loading()"`. `features/document/document-view.ts` Render + Copy buttons now have `[disabled]="loading()"` with a `loading` signal wrapping both `renderDoc()` and `copyContent()`.

**Gate run:** `pnpm -C src/DevContext.App check` → lint clean, 4/4 tests, build OK.

---

### P1 — what changed (2026-06-30)

- **Default vibe → terminal:** `core/theme/theme.service.ts:82` — `loadVibe()` fallback changed from `VIBES[0].id` (modern) to `'terminal'`. First launch now shows amber-on-black phosphor with monospace, zero radius.
- **Scanline texture:** `styles.css:245-259` — added a `body::after` pseudo-element with `repeating-linear-gradient` scanline overlay, gated by `[data-vibe="terminal"]`, 4% opacity, `pointer-events: none`. Also added phosphor glow on `:focus-visible` (amber `box-shadow`, not blue outline) at `:261-265`.
- **Vibe switcher:** `shell/title-bar.ts` — injected `ThemeService`; added a `<button>` in the title bar showing current vibe name (e.g. `TERMINAL`), cycles `terminal → modern → hacker` on click. Persists via existing `ThemeService.setVibe()` localStorage path.
- **Type/density pass:** replaced all `text-[10px]` → `text-2xs` and `text-[11px]` → `text-3xs` across 11 files. Added `@utility text-2xs` (0.625rem), `text-3xs` (0.6875rem), and `tabular-nums` in `styles.css:27-37`. Applied `tabular-nums` to all stat columns in `features/stats/stats-view.ts`.

**Gate run:** `pnpm -C src/DevContext.App check` → lint clean, 4/4 tests, build OK (413 KB initial / 106 KB transfer).

---

### P2 start points (next session)

1. **Live Console route** (`/console` + embedded strip on Source): stream `ProgressEvent`s into a `signal<LogLine[]>` the Console renders as a scrolling boot-log. The store currently consumes progress then drops it (`state/session.store.ts:56-62`).
2. **Settle into report:** when analysis completes, the same surface shows the `RunReport`: stages waterfall, extractor timings, seams, funnel.
3. **Render the funnel** (D7 fix): types discovered→included, raw→rendered tokens vs budget as a real bar.
4. **De-zero cold cache** (D7 fix): show hit-rate and label cold runs instead of four `0` cells.
5. **Real progress replaces D3 entirely:** the Console *is* the progress; status bar mirrors the latest line.

### Task tracker

In-session tasks are tracked via the harness Task list. A fresh agent should re-create P2 tasks from the start points above.
