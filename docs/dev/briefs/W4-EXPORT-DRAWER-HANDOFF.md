# W4 Export Drawer — Handoff Document

> Branch: `feat/w4-export-drawer` (off `feat/fable-redesign-skeleton`) · 2026-07-03
> Built: 1 session · Gate: `pnpm check` green + Playwright smoke verified

## What was built

The **export drawer** (proposal W4 checkpoint 7 of 9) — a Ctrl+E overlay replacing the old
page-modal export (`section-export.ts` → `/export` route) with a right-side drawer containing
4 presets, section toggles, and a "From Trail" context pack builder.

### Files changed

| File | Change | Lines |
|------|--------|-------|
| `features/export/export-drawer.ts` | **NEW** — full drawer component | 377 |
| `features/pages/workbench-page.ts` | Modified — Ctrl+E binding, Esc-ladder rung, template slot | +7 |
| `scripts/smoke-export-drawer.mts` | **NEW** — Playwright smoke script | ~195 |

Zero deletions. The old `section-export.ts` and `/export` route are untouched — they'll be
removed in W4 checkpoint 4 (route cutover). No new dependency: the repo already carries the
full `playwright` package (not `-core`) as a devDependency — use `import { chromium } from
'playwright'`, matching `scripts/audit-screenshots.mts`'s established convention.

### Component: `ExportDrawer` (`features/export/export-drawer.ts`)

```
Inputs:  open (boolean signal), dismissed (output)
Injects: SessionStore, DevContextApi, TraceStore, TrailStore, ToastService
```

**4 presets** (`.chip` buttons in header row):

| Preset | Render call | Sections |
|--------|------------|----------|
| **Full** | `api.render(handle, { format: 'markdown' })` | All (no filter) |
| **Onboarding** | `api.render(handle, { format: 'markdown', sections: ['Overview','Topology','Routes','Entry points'] })` | Identity+Architecture+Entries |
| **Flow Review** | `api.render(handle, { focus: trace.focus(), format: 'markdown' })` | Single focused render |
| **From Trail** | `api.render(handle, { focus: pin.focus, format: 'markdown' })` per pin, concatenated | `## [title]\n\n{content}` per step |

**Section toggles** (map-wide presets only): ported from `section-export.ts` — renders once
to discover sections from `RenderResponse.sections[]`, then re-renders with only-enabled
sections on each toggle. User toggle state preserved across re-renders; new sections added
default to `enabled: true`.

**States per preset:**

| State | Full/Onboarding | Flow Review | From Trail |
|-------|----------------|-------------|------------|
| Populated | `<pre>` content + token count | `<pre>` content + token count | `<pre>` concatenated content + summed tokens |
| Empty | "Choose a preset to render" | "No entry selected" + tip | "No pinned steps" + `p` key tip |
| Loading (first) | 5 Skeleton rows | 5 Skeleton rows | "Rendering step X of Y" + spinner |
| Loading (refresh) | Existing content dimmed 60% | Existing content dimmed 60% | N/A (sequential, not refreshing) |
| Error | "Render failed" + Retry button | Same | Per-step: `⚠ Render failed for "title"` |

**Layout:** Right-side drawer (480px, `absolute right-0 top-0 bottom-0`), `overlay-float` class
for the shared floating-panel shadow, `bg-elevated` panel, `bg-base/70` backdrop with
click-to-dismiss.

### Workbench page changes (`features/pages/workbench-page.ts`)

```typescript
// New signal
protected readonly exportOpen = signal(false);

// Ctrl+E handler (in onGlobalKey, between Ctrl+Shift+L and Ctrl+Z)
if (event.ctrlKey && !event.shiftKey && event.key.toLowerCase() === 'e') {
  event.preventDefault();
  this.exportOpen.set(true);
  return;
}

// Esc-ladder rung (between audit-table close and deselect-node)
if (this.exportOpen()) {
  this.exportOpen.set(false);
  return;
}

// Template
<app-export-drawer
  [open]="exportOpen()"
  (dismissed)="exportOpen.set(false)"
/>
```

## Verification receipt

**Correction (this session, resuming from opencode/deepseek):** the prior verification receipt
below (11/12, "Workbench not loaded" false negative) was **wrong** — it was never actually
verifying anything. `scripts/smoke-export-drawer.mts` resolved its fixture path as
`resolve('tests/fixtures/MinimalApiProject')`, which from `cwd = src/DevContext.App` points at
a directory that doesn't exist (the real fixture is two levels up, at repo-root
`tests/fixtures/MinimalApiProject` — see `scripts/grpcweb-smoke.mts`'s
`resolve('../../tests/fixtures/ControllerApp')` for the correct pattern). Every "Analyze" click
the script ever made failed silently (toast: "Analysis failed"), so `session.ready()` was never
true and the deck genuinely never rendered — not a locator/dockLevel false negative. The
"Full preset content rendered" check was also a false positive: it asserted `.flex-1 > *` is
visible, which is equally true of the *empty-state* placeholder div, so it passed regardless of
whether real content rendered. Also fixed: the script's `ng serve` child (`shell:true` +
`ng.kill('SIGTERM')`) left an orphaned `ng serve` process on every run on Windows (SIGTERM only
reaches the `cmd.exe` wrapper) — now cleaned up via `taskkill /PID <pid> /T /F`, confirmed with
back-to-back runs that port 4200 is released each time. Also removed an unnecessary
`playwright-core` devDependency the prior session added — the full `playwright` package was
already present; switched the script back to `import { chromium } from 'playwright'}`.
Re-verified for real below.

### Static checks (real exit codes, `pnpm check > check.log; echo $?`)
```
pnpm lint    → 0/0 (all files pass)
pnpm test    → 27/27 (5 suites, 0 failures)
pnpm build   → success (15 lazy chunks, no warnings)
EXIT_CODE=0
```

### Live smoke (Playwright, headless Chrome, real repo-root `tests/fixtures/MinimalApiProject`, analysis actually completing)

| Check | Result |
|-------|--------|
| App loads | PASS |
| Analyze clicked | PASS |
| Analysis completed | PASS |
| Workbench loaded (deck visible) | PASS |
| Deck has entries (2) | PASS |
| Entry pinned (`p`) | PASS |
| Ctrl+E opens drawer | PASS |
| 4 preset chips visible | PASS |
| Full preset renders real content (`<pre>`, not placeholder) | PASS |
| From Trail renders pinned step | PASS |
| Copy button present | PASS |
| Escape dismisses drawer | PASS |
| Reopen (Ctrl+E again) | PASS |
| Backdrop click dismisses | PASS |
| No app console errors | PASS |

15/15 checks passed, exit code 0. Reran twice back-to-back to confirm no flake and no leaked
`ng serve` process between runs.

## Design fidelity check

Against the proposal doc (`ui-ux-redesign-proposal-fable.md`):

| Proposal requirement | Status |
|---------------------|--------|
| Ctrl+E shortcut (§8.4) | Done |
| Section toggles + presets (§3.8) | Done |
| Onboarding / Flow-Review / Full presets (§3.8) | Done |
| From Trail — each pinned focus via Render RPC, concatenated (§3.8) | Done |
| Token estimate (§3.8) | Done |
| Content-preserving loading (§5.2) | Done |
| Overlay-float visual vocabulary (§4.1) | Done |
| Right-side drawer (matches "drawer" nomenclature) | Done |
| Escape ladder (§8.4) | Done |
| Clipboard plugin for copy (§7.4) | Deferred — `navigator.clipboard` used for now (plugin arrives W6) |

## Files NOT to touch (for the next agent)

1. `features/narrative/section-export.ts` — the old export modal. **Still needed** by the `/export`
   route until W4 checkpoint 4 (route cutover). Do not delete yet.
2. `features/pages/export-page.ts` — wraps `SectionExport`. Same: delete in W4 cp4.
3. `shell/activity-bar.ts` — still shows old 7-item layout. Collapse to proposal's 5-icon layout
   is part of W4 checkpoint 4, not this checkpoint.
4. `app.config.ts` — `/export` route still points to old `ExportPage`. Redirect in W4 cp4.

## Resume protocol

```powershell
# Use the export-drawer branch
git -C C:/Code/DevContext2-ui checkout feat/w4-export-drawer

# Baseline check
Set-Location C:/Code/DevContext2-ui/src/DevContext.App
pnpm check > check.log; echo $LASTEXITCODE

# Start server + dev:web (separate terminals)
# 1: pnpm server
# 2: pnpm dev:web

# Verify manually:
# - Open http://localhost:4200
# - Analyze tests/fixtures/MinimalApiProject
# - Navigate to /explore (click activity bar, then type /explore in URL)
# - Select an entry, press p to pin
# - Ctrl+E → drawer opens
# - Click each preset, verify content renders
# - From Trail: verify pinned step appears
# - Escape dismisses, Ctrl+E reopens

# Or run the automated smoke:
node --experimental-strip-types scripts/smoke-export-drawer.mts
```

**Next W4 items** (remaining 2 of 9 checkpoints):
1. Home page assembly
2. Atlas page assembly
3. Route cutover + deletion
4. Full manual gate sweep

See `AGENTS.md`'s "F — Fable Workbench Redesign" section for the complete waterfall status.
