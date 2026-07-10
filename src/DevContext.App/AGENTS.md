# DevContext Desktop (Tauri + Angular)

A cross-platform desktop client for DevContext. The UI (Angular 22, zoneless, signals) talks to
`DevContext.Server` over **gRPC-Web**; the server wraps the `DevContext.Core` engine
(post-Loom: Graph2 identity spine, BodyFacts pipeline, projections). Tauri provides the native
shell (OS WebView — no bundled Chromium).

**Phase:** Loom L8 (close-out). Branch: `feat/loom-l7`.
See: `LOOM-START.md` (tracker), `docs/dev/HANDOVER-LOOM.md` (close-out doc).

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

### Gaps — open (engine + proto) and closed (UI-fixable)

> **Updated 2026-07-07** following the M9-ext session. All closure gaps delivered. Traps assessed as acceptable v0.

#### CLOSED (gaps 1–2 — engine + proto)

| Gap | Commit | What |
|-----|--------|------|
| G1 | `16d3166` | `read_source` RPC: proto → C# stubs → server handler → TS → Inspector uses `api.readSource()` |
| G2 | `3be265f` | Layer/feature uplumb: proto fields → ProtoMapper → TS → lens-switcher unblocked → inspector chips |
| G3 | `bf3a674` | PrismJS wired: `core/code-highlight.ts` → `inspector.ts` Code tab via `[innerHTML]="highlightedCode()"` + CSS token theme |
| G4 | `bf3a674` | Contrast audit: Graphite `#6b7280→#7a8291`, Light `#8b95a1→#5c6673`, Sepia `#5a5a5a→#858585` — all pass WCAG AA 4.5:1 on base |
| G5 | `ba6c59a` | Table lens "Shared" column: `refreshSharedTargets()` effect → counts entries sharing the same target handler |
| G6 | `bf3a674` | Dead `audit-table.ts` removed; unreachable `onAuditSelect()` deleted; comment references updated |
| G7 | `bf3a674` | Dock toggle cycles 0→2→3→0 (level 3 focus mode reachable via Ctrl+Shift+L) |

#### OPEN — Gap 1: `read_source` RPC (engine + proto)

**Symptom:** Inspector Code tab shows markdown from Render RPC, not raw source. `read_source` is not in the gRPC contract.

**Engine side (C#):**
1. Add RPC to `proto/devcontext/v1/devcontext.proto`:
   ```protobuf
   rpc ReadSource(ReadSourceRequest) returns (ReadSourceResponse);

   message ReadSourceRequest {
     string session_id = 1;
     string node_id = 2;            // graph node id
     ReadSourceMode mode = 3;       // MEMBER = full member span; WINDOW = context window
   }
   enum ReadSourceMode { MEMBER = 0; WINDOW = 1; }
   message ReadSourceResponse {
     string content = 1;            // raw source text
     string language = 2;           // "csharp", "razor", etc.
     string file_path = 3;
     int32 start_line = 4;
     int32 end_line = 5;
   }
   ```
2. Regenerate C# stubs: `dotnet build src/DevContext.Contracts`
3. Implement in `DevContext.Server`: unwrap node's source span from `AnalysisSnapshot`, read file from disk, return the line range. PRECEDENT: `GraphQuery` already has `GetNodeSourceSpans()` — extend that path.
4. Regenerate TypeScript: `pnpm gen:proto` (from `src/DevContext.App`)
5. Gate: `dotnet build DevContext.slnx` 0w0e; smoke: call `read_source` on a known node, verify C# source returned.

**UI side (after engine is green):**
1. In `inspector.ts`, replace `loadCode()` RPC call from `api.render(..., sections:['members'])` to `api.readSource(handle, { nodeId, mode:'MEMBER' })`
2. Set `this.codeContent.set(res.content)` — PrismJS highlighting already wired (G3)

#### OPEN — Gap 2: Layer/Feature uplumb (engine + proto)

**Symptom:** Lens switcher marks layer/feature as `available=false`. Engine M2.4 computed Layer/Feature facets but they are not exposed in the gRPC proto.

**Engine side (C#):**
1. Add fields to `proto/devcontext/v1/devcontext.proto` — in the node messages used by Map:
   ```protobuf
   message ProjectNode {
     // ...existing fields...
     string layer = 10;             // "Api" | "Application" | "Domain" | "Infrastructure" | "Contracts"
     string feature = 11;           // namespace-derived feature area
     repeated string layer_violations = 12;  // layer dependency violations
   }
   ```
   Also add `Layer` field to `EntryNode`, `TraceNode`, `NodeDetail` messages.
2. Regenerate C# stubs: `dotnet build src/DevContext.Contracts`
3. Implement in engine mapper (likely `ProtoMapper` or `GraphQuery`): copy `TypeNode.Layer`, `TypeNode.Feature` from `AnalysisSnapshot` into the proto response. M2.4 evidence: `InferLayer` + `DeriveFeature` already compute these — they just need the proto bridge.
4. Regenerate TypeScript: `pnpm gen:proto` (from `src/DevContext.App`)
5. Gate: `dotnet build DevContext.slnx` 0w0e; `dotnet test DevContext.slnx --filter "Category!=Eval"` green.

**UI side (after engine is green):**
1. In `lens-switcher.ts`, change layer/feature from `available: false` → `available: true`
2. In `view-models.ts`, add `layer`/`feature` fields to `NodeDetailVm` / `EntryVm`
3. In `stage.ts` layer lens rendering: horizontal bands colored by layer, violation edges in warn color
4. In `stage.ts` feature lens: feature columns with nodes grouped by `Feature` field
5. Gate: `pnpm check` green; visual smoke: click Layer lens → bands visible; click Feature lens → columns visible

---

## M7+M8.1a static audit (ran 2026-07-06)

### Audit results

| # | Check | Result |
|---|-------|--------|
| | `pnpm check` | Lint 0, tests 27/27, build 0w/0e |
| A14 | TODO sweep (M7 files) | No TODOs in table-lens, lens-switcher, graph-canvas, trail.store, inspector |
| A1 | Dead signals | `highlightedNodeId` wired in 3 GraphCanvas instances; `codeContent/codeLoading/codeError/codePathCopied` consumed in inspector template; `groupedBreadcrumb` consumed in inspector trail section |
| A2 | Checkpoint renumbering | MERIDIAN-START.md has correct commit hashes for all M7 rows |
| A7 | LensId types | Typed union `'service' | 'layer' | 'feature' | 'flow'` — no string matching |
| A10 | Dead navigation | `goMCP()` navigates to `/mcp` route (verified); new `/context` route registered in app.config.ts |

### Anti-patterns NOT checked this session (visual smoke required)

| # | Check | Requires |
|---|-------|----------|
| A6 | Ship-without-launch | `pnpm dev:web` + analyze dogfood + click through M7/M8.1a surfaces |
| A11 | Silent success | Verify copy button shows "copied", CSV export shows toast, lens switch has active state |
| A12 | Identical-canvas | Verify Atlas page has its own lens default (not System canvas) |
| A5 | Verify dogfood | Load dogfood repo and verify Home/Atlas/Explore/Context surfaces render |

> **The next agent MUST run `pnpm dev:web`, analyze the dogfood repo, and verify A6/A11/A12/A5 before continuing M8.1b.**

---

## M8 completion plan — remaining checkpoints

### Current state
| # | What | Status | Commit |
|---|------|--------|--------|
| M8.1a | /context route + rail entry + Ctrl+E→/context redirect + 3-pane stub | DONE | `2a6e585` |
| M8.1b | Scope picker | DONE | |
| M8.1c | Composition view | DONE | |
| M8.1d | Budget panel | DONE | |
| M8.1e | Retire old panes (ExportDrawer + Inspector LLM section) | DONE | |
| M8.2 | Composition model (cards/seeds/presets) | DONE | |
| M8.3 | Budget/meter/intent/copy controls | TODO | |
| M8.4 | Provenance + staleness + builder round-trip | TODO | |

### M8.1b — Scope picker
- **File:** NEW `features/context-studio/scope-picker.ts`
- **What:** Tree of the current session's services → entries, searchable/filterable. Omnibox for free-form type/flow selection. "I'm changing this endpoint" preset button that:
  1. Opens a modal/dropdown of all entries
  2. On select: seeds the composition with flow skeleton + target member bodies + contracts + validators + tests
- **Data source:** `SessionStore.entryGroups()` (already in ContextStudio component). No new RPC needed.
- **Gate:** Tree renders with service→entry hierarchy; omnibox filters; preset button populates composition placeholder cards.

### M8.1c — Composition view
- **File:** NEW `features/context-studio/composition-view.ts`
- **What:** Ordered list of context cards. Each card has: title, type badge (flow/members/config/entities/tests), per-card body toggle (on/off), drag handle (reorder), × remove. Cards flow from scope picker selections.
- **State:** Signal array of `ContextCard` objects (id, type, title, bodyEnabled, entries, sourceIds).
- **No RPC needed:** all data from SessionStore + graph queries the app already makes.
- **Gate:** Add 3 cards → reorder → toggle body off → remove one → list updates correctly.

### M8.1d — Budget panel
- **File:** NEW `features/context-studio/budget-panel.ts`
- **What:** Token budget slider (1k–16k), live meter with per-card bar visualization (estimate: lines × ~2.5 tokens), intent selector (trace/explain/review), format selector (markdown/plain), [Copy] [Save] buttons with feedback affordances (A11: icon morphs to check + toast "Copied!").
- **Token estimation:** Client-side heuristic — `totalLines * 2.5` per card. Server-side meter (round-trip `ContextPackBuilder`) gates M8.4; this is the v0 approximation.
- **Gate:** Slider adjusts budget; meter updates per-card bars; Copy produces context markdown; "Copied!" toast visible.

### M8.1e — Retire old panes
- **Remove:** `features/export/export-drawer.ts` (and its import in workbench-page)
- **Remove:** Inspector LLM context section (`sectionId 'llm'` tab + render logic in inspector.ts)
- **Remove:** Dead `exportOpen` signal and template block in workbench-page.ts
- **Verify:** `rg "ExportDrawer|export-drawer" src/app/ --include="*.ts"` → zero results (except comments)
- **Gate:** Ctrl+E goes to /context; no console errors; no dead routes; `pnpm check` green.

### Gate for full M8.1 (all 5 checkpoints)
- `/context` page renders 3-pane layout
- Scope picker shows services/entries from current session
- "I'm changing this endpoint" preset seeds composition cards
- Budget slider adjusts; Copy produces token-metered markdown
- Ctrl+E opens /context; ExportDrawer and Inspector LLM section removed
- `pnpm check` green; no console errors; no dead routes

---

## Resume protocol for next agent

```powershell
git -C C:/Code/DevContext2-ui checkout feat/loom-l7
git -C C:/Code/DevContext2-ui pull
Set-Location C:/Code/DevContext2-ui/src/DevContext.App; pnpm check
# Read LOOM-START.md for handoff + checkpoint state
# Read docs/dev/briefs/loom-graph-design.md (design authority)
# Read docs/dev/HANDOVER-LOOM.md (close-out doc)
# Read this AGENTS.md from top
```

### Session cycle for next agent

1. **Gate battery (MANDATORY first step):**
   ```powershell
   cd src/DevContext.App; pnpm check
   ```
   From repo root:
   ```powershell
   dotnet build DevContext.slnx                                           # 0w 0e
   dotnet test DevContext.slnx --filter "Category!=Eval"                  # unit + integration
   powershell -File scripts/loom-guards.ps1                               # zero banned patterns
   ```

2. **Fix bugs** — address any regressions found. Verify known Loom invariants:
   - `rg "Regex" src/DevContext.Core/Graph/` — must return empty
   - `rg "NodeId.ForType" src/DevContext.Core/Graph/` — only advisory sites remain
   - `rg "TODO|FIXME" src/app/features/context-studio/ src/app/features/table-lens/` — must return empty

3. **Deliver** — one checkpoint at a time, with evidence artifact per checkpoint.

4. **Commit** — one small commit per checkpoint:
   ```powershell
   git -C C:/Code/DevContext2-ui add <files>
   git -C C:/Code/DevContext2-ui commit -m "feat(L<stage>): ..."
   ```

5. **After ALL checkpoints green, update docs for the NEXT agent:**
   - Overwrite `LOOM-START.md` Handoff block
   - Append `docs/dev/go-to-program/PROGRESS-LOG.md`
   - Commit: `docs: handoff — L<stage> delivered`

---

## L8 delivery — Loom close-out (overwrite this block each session, no history)

status: L8.1 DONE — Loom phase closed. Branch `feat/loom-l7`.
delivered: HANDOVER-LOOM.md, AGENTS.md rituals, truth test fixes (7P/4S), LOOM-START.md tracker updated.
last: gate battery green (build 0w/0e, test 414P/3S Core + 64P Desktop + 12P Server, pnpm 27/27).
next: conductor-DEBT.md resolution or next phase planning.
evidence: eval-results/2026-07-08/gate-battery-l8-s40.txt, truth-battery-l8-s40.txt, docs/dev/HANDOVER-LOOM.md.


M8.3 detail:
  - Server-side token meter: card.serverTokens stored from ContextResponse.totalTokens after getContext RPC
  - Budget→RPC wiring: budget slider value (BudgetPanel.budget model) flows to getContext budgetTokens param
  - Per-section token breakdown: card.sectionTokens = [{key, tokens}, ...] from ContextResponse sections
  - Exact vs heuristic distinction: server tokens shown without ~ prefix, heuristic estimatedLines with ~ prefix
  - BudgetPanel.budget: signal→model for two-way binding with parent via [(budget)]

M8.4 detail:
  - Provenance chips: file:line per-card (extracted from EntryVm.provenance of seeded entries)
  - Composition footer shows total tokens not lines (uses serverTokens when available)
  - Per-card token badge: green text for server-confirmed (formatTokens), muted ~ for heuristic estimate
  - Toast feedback on copy/save already wired from M8.2

M8.1–M8.2 detail (delivered prior session):
  - 9 card types: flow | signatures | bodies | di_wiring | config | entities | contracts | tests | identity
  - getContext RPC wired: cards load real content from server, fall back to placeholder on error
  - Preset "I'm changing this endpoint": flow + bodies + contracts + validators + tests
  - Omnibox: dropdown search across all entries by title/route/target, kind-colored badges
  - Drag-drop: grip handle → native HTML5 drag events, reorder via cardReorder output
  - Global body toggle: "All bodies shown/hidden" button in budget panel, model-based two-way binding
  - Trail seeds: "From current trail" button seeds flow cards from TrailStore steps
  - Format selector: markdown/plain produces real different output (strips markdown in plain mode)
  - Intent ordering: trace/explain/review reorders cards via INTENT_CARD_ORDER mapping + effect()
  - ExportDrawer retired (file deleted), Inspector LLM section removed, Ctrl+E→/context redirect
