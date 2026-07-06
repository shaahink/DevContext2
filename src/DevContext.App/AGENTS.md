# DevContext Desktop (Tauri + Angular)

A cross-platform desktop client for DevContext. The UI (Angular 22, zoneless, signals) talks to
`DevContext.Server` over **gRPC-Web**; the server wraps the unchanged `DevContext.Core` engine.
Tauri provides the native shell (OS WebView — no bundled Chromium).

## Prerequisites

- **Node 24+** (`nvm use 24` — Angular 22 requires ≥ 22.22.3 / 24)
- **pnpm** (`corepack enable`)
- **.NET 10 SDK** (for the server)
- **Rust** + platform toolchain (for the Tauri shell): Windows needs VS Build Tools (VC.Tools) + WebView2 (preinstalled on Win11)

## Install

```bash
pnpm install
```

## Run

```bash
pnpm dev        # desktop: starts the .NET server + `tauri dev` (native window)
pnpm dev:web    # browser:  starts the .NET server + `ng serve` -> http://localhost:4200
pnpm server     # just the .NET server (http://127.0.0.1:5179)
```

The UI polls `Ping`/`/health`; the connection dot in the source bar shows server readiness.

## Test & checks

```bash
pnpm test       # Vitest unit/component tests (one-shot)
pnpm lint       # angular-eslint (flat config)
pnpm build      # production build
pnpm check      # lint + test + build (the local gate)
```

Live gRPC-Web smoke (drives the real server with the same client the app uses):

```bash
pnpm server     # in one terminal
node --experimental-strip-types scripts/grpcweb-smoke.mts   # in another
```

## Contract / codegen

The gRPC contract lives at `proto/devcontext/v1/devcontext.proto` (repo root) — the single source of
truth. It generates **C#** (server, via `Grpc.Tools` in `DevContext.Contracts`) and **TypeScript**
(this app, via buf):

```bash
pnpm gen:proto  # regenerate src/app/core/grpc/gen/** after editing the .proto
```

## Architecture (clear layering)

```
src/app/
  core/        transport + generated gRPC client (DEVCONTEXT_CLIENT), config
  data-access/ DevContextApi — typed wrapper over the gRPC client
  state/       signal stores: SessionStore, TraceStore, ConnectionStore
  models/      view models + proto -> view mappers
  ui/          dumb presentational components: Icon, GraphCanvas (Cytoscape)
  features/    smart components: source-bar, entries-panel, map-panel, trace-panel, node-detail, workspace
```

- **Analyze once, query many.** `Analyze` returns a session handle; Map/Trace/Node/Neighbors are
  cheap render-time queries over the same immutable snapshot — never a re-analysis.
- **Zoneless + signals** throughout; no `zone.js`.
- **Styling**: Tailwind CSS v4 design tokens (`src/styles.css`), dark-first. Icons via `lucide`.

## Server lifecycle

In development the server runs separately (`pnpm dev` orchestrates it). For packaged builds the Tauri
shell (`src-tauri/src/lib.rs`) spawns and kills the server when `DEVCONTEXT_SERVER_DLL` is set
(bundled self-contained sidecar — see P5 in the plan).

## M6 verification — drive the new UI surfaces

After `pnpm dev:web`, analyze the dogfood repo and verify:

### Home page (`/`)
- [ ] ServiceMapHero renders deterministically — gateway (YarpApiGateway) on left column, core services in center, bus/broker rail underneath
- [ ] Three tiles populate: Entries sparkbar (colored per-service bars), Wiring health (%, color-coded), Freshness ("Analyzed in Xs", stale/current chip)
- [ ] Onboarding row shows [Trace checkout] link (auto-detected from entries), [Open atlas], [Point your agent here]
- [ ] Top flows have service-colored chips per entry
- [ ] "Needs attention" section links to insights or explore with correct params

### Atlas page (`/atlas`)
- [ ] 6 sections render in order: service diagram → top flows (flow steppers) → event wiring board → per-service cards → cross-cutting → hub radar
- [ ] Flow steppers show horizontal strips with node count, depth, cross-service count
- [ ] Event wiring board table shows publisher/event/consumer rows
- [ ] Service cards show style badge + stack tags per service
- [ ] Cross-cutting shows pipeline behaviors + packages
- [ ] Hub radar rows click → navigate to Explore with node view
- [ ] Export one-pager button copies markdown to clipboard (shows "Copied!" for 2s)

### Bug check
- [ ] No browser console errors
- [ ] No missing-data indicators on empty states
- [ ] All router links work (click service card → Explore, click flow stepper → Explore, Hub radar → Explore)
- [ ] Identity strip still works (stale chip, re-analyze, confidence ledger collapsible)

## M7 delivered — 6 checkpoints (session 2026-07-06)

| # | What | Commit | Files |
|---|------|--------|-------|
| M7.0 | Design-token pass: min body 12px, icons 14–16px, KIND_COLORS registry | `efa02ff` | `styles.css`, `view-models.ts`, 12 component files |
| M7.1 | Graph↔code binding: Code tab in Inspector, `highlightedNodeId` in GraphCanvas, prismjs installed | `c7affae` | `inspector.ts`, `graph-canvas.ts`, `stage.ts` |
| M7.2 | Lenses: `lens-switcher.ts`, Service/Layer/Feature/Flow, `?lens=` URL, per-page defaults | `691d319` | `lens-switcher.ts`, `stage.ts`, `workbench-page.ts` |
| M7.3 | Trail flow grouping: `TrailFlowGroup` expand/collapse, kind-colored deck dots, full-route tooltips | `c3e7078` | `trail.store.ts`, `inspector.ts`, `entry-deck.ts` |
| M7.4 | Chrome pass: titlebar 40px, rail hover labels, statusbar clickable segments | `1d2f26d` | `titlebar.ts`, `activity-bar.ts`, `statusbar.ts` |
| M7.5 | Table lens v2: CDK-virtualized, archetype columns, column picker, CSV export | `e0ca191` | `table-lens.ts`, `table-lens-columns.ts`, `workbench-page.ts` |

**Gate:** `pnpm check` green (lint 0/0, test 27/27, build 0w/0e) for every commit.

### Known gaps from M7 delivery

1. **Code tab shows metadata only** — `read_source` RPC is not in the gRPC contract. The Code tab displays file path + line number with Copy/Reveal/Load actions. The `loadCode()` method calls the Render RPC with `sections: ['members']` but the response is markdown, not raw source. **Fix:** add `read_source` to the proto + engine + codegen, then wire to Inspector.
2. **Layer/Feature lenses are structural slots** — lens-switcher.ts marks layer/feature as `available=false`. Engine M2.4 computed Layer/Feature facets but they are not exposed in the gRPC proto (`ProjectNode` has only `name` and `dependsOn`). **Fix:** add layer/feature fields to the proto, engine, and codegen.
3. **PrismJS installed but not wired** — `prismjs` is in `package.json` but no component imports or calls `highlight()`. A `code-highlight.ts` utility was planned but not created. **Fix:** create `src/app/core/code-highlight.ts`, import in Inspector Code tab.
4. **No contrast audit performed** — M7.0 bumped font sizes and icon sizes but did not compute WCAG AA ratios for the `--vibe-ink-subtle`/`--vibe-base` pair or other subtle-text-on-surface combinations. **Fix:** spot-check key color pairs (ink-subtle on base, accent on surface-2, chip text on surface) and bump contrast where needed.
5. **Table lens: relationship chips, row expand, touch-risk columns are TODOs** — the web/microservices column set has only 7 basic columns. The spec's relationship chips ("shares OrderRepository with 3"), mini flow stepper row expand, and touch-risk column are not implemented. **Fix:** add these columns (requires graph query data), row expand (reuse flow-stepper pattern), relationship chip popover.
6. **Table lens replaces audit-table but `audit-table.ts` still exists** — the file is no longer imported but remains in the tree. `onAuditSelect()` method still exists in workbench-page but is unreachable (Shift+E opens table lens only). **Fix:** remove dead code after verifying table lens is the single table surface.
7. **Dock width DOCK_WIDTHS constant has `as const` type:** the widths array is `[0, 30, 40, 100] as const` — level 3 (100%) should be focus mode. Verify dock level 3 ("focus") correctly hides the deck and shows inspector at 100% width. **Fix:** test dock toggle Ctrl+Shift+L in browser.

---

## M7 static audit — run this before any M8 work

### Anti-pattern checklist (from `docs/dev/briefs/meridian-agent-playbook.md` §4)

Read the anti-pattern catalog. For each, check if M7 commits introduced it:

| # | Anti-pattern | Look for | M7 check |
|---|-------------|----------|----------|
| A1 | Dead-parameter fix | New signal/input never consumed | Check `highlightedNodeId` is actually wired in all 3 GraphCanvas instances; check `codeContent`/`codeLoading`/`codeError` signals are read in template |
| A2 | Silent checkpoint renumbering | Tracker rows not matching commits | Verify MERIDIAN-START.md checkpoint table has correct commit hashes and all 6 M7 rows are present |
| A3 | Stub artifact | Generated output is empty | Run `pnpm check` — it produces real build output |
| A4 | Gate skipped, claimed run | "Green" claimed but command not pasted | Every commit message includes `pnpm check green` — verify by running it fresh |
| A5 | Verify where it's cheap | Tested against fixture not dogfood | The dogfood repo was not run against M7 changes (UI-only changes, no engine test needed) — but verify the UI loads against dogfood |
| A6 | Ship-without-launch | Code compiles, never executed | Run `pnpm dev:web` and click through M7 surfaces: lens switcher, Code tab, table lens, trail grouping |
| A7 | Literal-string type matching | String-matching type names | Check `lens-switcher.ts` — lenses are defined by explicit `LensId` type union, not string matching |
| A8 | Cross-scope name grabbing | Unqualified matching across scopes | N/A (no name resolution in UI code) |
| A9 | Framework noise as signal | DI/infra shown as domain facts | N/A (UI-only session) |
| A10 | Dead-end navigation | Links that don't resolve | Check `statusbar.ts` `goMCP()` navigates to `/mcp` — verify route exists |
| A11 | Silent success UI | Button acts with zero visible feedback | Audit every `(click)` in new code: Code tab copy shows "copied" state, table CSV export shows toast, lens switch has active state |
| A12 | The identical-canvas shortcut | Same System canvas everywhere | Lens switcher exists (M7.2), but check that Atlas page has its own lens default |
| A13 | Fixture-shaped fixture | Test matches code, not reality | N/A (no new tests written in M7) |
| A14 | TODO-as-delivery | `// TODO(agent)` in diff of delivered checkpoint | Grep for `TODO` in M7 files — should not exist in delivered features |
| A15 | Catch-and-continue | Silent exception swallowing | Check `try { localStorage.setItem(...) } catch { /* ignore */ }` in `table-lens.ts` — intentional localStorage fallthrough is acceptable per pre-existing pattern in `theme.service.ts` |

### Self-audit procedure

```powershell
# 1. Run the gate fresh
cd src/DevContext.App; pnpm check

# 2. TODO sweep — any left in M7 files?
rg "TODO|FIXME" src/app/features/table-lens/ src/app/features/explorer/lens-switcher.ts src/app/features/inspector/inspector.ts src/app/ui/graph-canvas/graph-canvas.ts src/app/state/trail.store.ts

# 3. Dead import check — is audit-table.ts still imported anywhere?
rg "AuditTable|audit-table" src/app/ --include="*.ts"

# 4. Verify all new signals are template-referenced (A1 check):
#    codeContent, codeLoading, codeError, codePathCopied in inspector template
#    highlightedNodeId in stage template
#    groupedBreadcrumb in inspector trail section

# 5. Visual smoke test — start the app, analyze dogfood, click through:
pnpm server     # terminal 1
pnpm dev:web    # terminal 2
# - Lens switcher: click Service/Layer/Feature/Flow — verify content changes
# - Code tab: select a trace node — verify Code tab opens, path shown
# - Table lens: Shift+E — verify virtualized table, column picker, CSV export
# - Trail: trace multiple entries — verify flow groups collapse
# - Chrome: verify titlebar ≈40px, rail hover labels appear
```

### Gap-closure order

After completing the static audit above, address gaps in this order:

1. **Remove dead `audit-table.ts` import/references** (gap 6) — simplest, unblock agent
2. **Wire prismjs** (gap 3) — create `core/code-highlight.ts`, import to Inspector Code tab
3. **Contrast audit** (gap 4) — check at least 3 key color pairs, adjust CSS if needed
4. **Verify dock level 3 focus mode** (gap 7) — Ctrl+Shift+L toggles correctly
5. **Relationship chips** (gap 5 partial) — add `shares-logic-with` column using existing flow data if available
6. **Layer/Feature uplumb** (gap 2) — requires engine + proto changes
7. **read_source RPC** (gap 1) — requires engine + proto changes

Gaps 1-5 are fixable in the UI. Gaps 6-7 require engine changes and should be deferred to an engine session.

---

## M8 plan — Context Studio first session

### M8.1 scope (from proposal-meridian.md §M8)
- New rail page + "Build context" entry points from Explore and Home
- Replaces Export drawer (Ctrl+E) and Explore LLM-context pane
- Three-pane: scope picker left, composition center, budget controls right

### Key files to create / modify

| File | Change |
|------|--------|
| **New:** `features/context-studio/context-studio.ts` | Full-page component with 3-pane layout |
| **New:** `features/context-studio/scope-picker.ts` | Left panel: tree of services/entries/types/flows + omnibox + presets |
| **New:** `features/context-studio/composition-view.ts` | Center: ordered cards (flow skeleton, member signatures, bodies, DI, config keys, entities, tests-for) |
| **New:** `features/context-studio/budget-panel.ts` | Right: budget slider, live token meter per card, intent selector, format, Copy/Save |
| **Modify:** `shell/workspace-shell.ts` | Add `/context` route, register in activity-bar rail, Ctrl+E redirect |
| **Modify:** `shell/activity-bar.ts` | Add "Context Studio" rail item |
| **Modify:** `features/pages/workbench-page.ts` | Ctrl+E redirects to `/context` instead of opening ExportDrawer |
| **Modify:** `features/export/export-drawer.ts` | Retire or redirect |
| **Modify:** `features/inspector/inspector.ts` | Remove LLM context section (replaced by Context Studio) |

### M8.1 delivery order
| # | Step | Description |
|---|------|-------------|
| 8.1a | Route + rail entry | Add `/context` route, rail item with icon, Ctrl+E redirect |
| 8.1b | Scope picker | Tree of services/entries + omnibox + "I'm changing this endpoint" preset |
| 8.1c | Composition view | Cards in order: flow skeleton, member signatures, bodies toggle, DI, config, entities, tests-for |
| 8.1d | Budget panel | Slider, live token meter per card, intent selector, Copy/Save with feedback |
| 8.1e | Retire old panes | Remove ExportDrawer, remove Inspector LLM context section, verify no dead routes |

### Gate for M8.1
- New `/context` page renders with 3 panes
- Scope picker shows services/entries from current session
- "I'm changing this endpoint" preset selects the trace focus
- Budget slider adjusts token limit
- Copy button produces context that matches token meter ±5%
- Ctrl+E opens /context (not old drawer)
- No dead routes, no console errors

---

## Resume protocol for next agent

```powershell
git -C C:/Code/DevContext2-ui checkout feat/meridian-m0
git -C C:/Code/DevContext2-ui pull
Set-Location C:/Code/DevContext2-ui/src/DevContext.App; pnpm check
# Read this AGENTS.md from top
# Read docs/dev/briefs/proposal-meridian.md §M8
# Read docs/dev/briefs/meridian-agent-playbook.md §UI-Context Studio
```

### Session cycle (repeat for every agent session)

1. **Static audit** — run the self-audit procedure above against the current delivered codebase
2. **Fix bugs** — close any anti-patterns found, address gaps 1-5 from the gap list
3. **Plan M8** — read the M8.1 scope and files list, choose the next checkpoint to deliver
4. **Build** — implement the checkpoint, `pnpm check` after every commit
5. **Commit** — one small commit per checkpoint, message prefix `feat(m8.X):`
6. **Update** — overwrite the `## M8 delivery` section below with what was done, remaining gaps, and next checkpoint
7. **Repeat** — the next agent starts from `AGENTS.md` and `MERIDIAN-START.md`, not from your chat transcript

---

## M8 delivery (overwrite this block each session, no history)

status: not yet started
last commit: `feat(m7.5)` (e0ca191)
next: M8.1a — route + rail entry for /context
gaps open: 1 (read_source RPC), 2 (layer/feature uplumb), 3 (prismjs unwired), 4 (contrast audit), 5 (table relationship chips), 6 (dead audit-table), 7 (dock level 3 verify)

