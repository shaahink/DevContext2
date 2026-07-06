# AGENTS.md — DevContext monorepo

Branch `feat/meridian-m0`. Monorepo: engine + server + CLI (C#), desktop app (Angular + Rust/Tauri).

## Cold start
1. Read `MERIDIAN-START.md` — the current stage card (branch, checkpoints, key files, gate).
2. `src/DevContext.App/AGENTS.md` — app conventions, run commands, architecture layering.
3. `docs/dev/briefs/meridian-agent-playbook.md` — quality bar, anti-patterns, run/test instructions.

## Verify loop
```powershell
# From C:/Code/DevContext2-ui/src/DevContext.App
pnpm check          # lint + vitest + build
pnpm server         # start .NET server (separate terminal)
pnpm dev:web        # start Angular dev server

# From C:/Code/DevContext2-ui
dotnet build DevContext.slnx                         # 0 warnings
dotnet test  DevContext.slnx --filter "Category!=Eval"
```

## Hard rules
- `pnpm check` green before every commit. `dotnet build` green for engine changes.
- Docs move with code in the same commit.
- Append `docs/dev/go-to-program/PROGRESS-LOG.md` after every session.
- Do not write new C# extractors — reform in place.
- Commit before starting work, push after finishing.

## Work items
- **Meridian M0-M4** ✅ — harness gate + wiring truth pass + insight repair + MCP re-architecture + MCP feature set (9/9 tools).
- **Meridian M5** ✅ — Agent eval ratchet: 5-repo QA, token ratchets, agent transcript, CI wiring.
- **Meridian M6** ✅ — Home repo card + Atlas one-pager (service-map-hero, tiles, onboarding, flow-stepper, service-cards, export).
- **Meridian M7** 🔜 — Design-token pass + Graph↔code binding + Lenses + Chrome + Table v2.
- **Lighthouse L0–L7** ✅ — see `docs/dev/HANDOVER-LIGHTHOUSE.md`.
- **Fable** ✅ — W0-W7 done. See `docs/dev/HANDOVER-FABLE-FINAL.md`.
- **U3 Facet views** ⬜ — blocked on engine E4.

## Resume protocol (post-M6 → M7)
```powershell
git -C C:/Code/DevContext2-ui checkout feat/meridian-m0
git -C C:/Code/DevContext2-ui pull
dotnet build C:/Code/DevContext2-ui/DevContext.slnx
Set-Location C:/Code/DevContext2-ui/src/DevContext.App; pnpm check
# Read MERIDIAN-START.md for handoff + checkpoint state
# Read docs/dev/briefs/meridian-agent-playbook.md
# Read docs/dev/briefs/proposal-meridian.md §M7
```

## Next session — M7 design tokens + explore lenses (M7.0/7.1/7.2)

### Step 0: Commit before starting
```
git -C C:/Code/DevContext2-ui status   # ensure clean
```

### Step 1: Drive M6 UI for verification
Start the app and exercise the new M6 surfaces:
```powershell
pnpm server         # terminal 1
pnpm dev:web        # terminal 2 (opens http://localhost:4200)
```
- **Home page:** Analyze the dogfood repo. Verify service map hero renders (gateway left, services center), three tiles populate (entries sparkbar, wiring health %, freshness), onboarding row shows [Trace checkout] link, top flows have service-colored chips.
- **Atlas page:** Navigate to `/atlas`. Verify 6 sections render: service diagram, flow steppers, event wiring board, per-service cards, cross-cutting (behaviors/packages), hub radar. Click "Export one-pager" — verify markdown copies to clipboard.
- **Bug check:** No console errors, no missing data indicators, service cards show style+stack.

### Step 2: Read M7 docs
1. `docs/dev/briefs/proposal-meridian.md` §M7 (design tokens, graph↔code, lenses, chrome, table)
2. `docs/dev/briefs/meridian-agent-playbook.md` §UI-Explore, §UI-Chrome, §UI-Table lens
3. `src/DevContext.App/src/styles.css` — current token system (@theme inline, CSS vars, vibe palettes)
4. `src/DevContext.App/src/app/core/theme/theme.service.ts` — vibe switching, palette

### Step 3: Plan M7 delivery
| # | What | Key files |
|---|------|-----------|
| M7.0 | Design-token pass: min body 12px, icons 14–16px, per-kind color coding, contrast audit | `styles.css`, `theme.service.ts`, all component templates |
| M7.1 | Graph↔code binding: node select → code tab with full member, trace step → edge highlight + code scroll, ESC ladder | `workbench-page.ts`, `explorer/stage.ts`, `explorer/inspector.ts` |
| M7.2 | Lenses land: Service/Layer/Feature lenses, per-page default, lane sheet for layers | `workbench-page.ts`, `explorer/stage.ts`, new `explorer/lens-switcher.ts` |
| M7.3 | Trail dedupe/group/cap; deck legibility | `trail.store.ts`, `explorer/entry-deck.ts` |
| M7.4 | Chrome pass + feedback affordances (VS Code bar test) | `workspace-shell.ts`, `activity-bar.ts`, omnibox |
| M7.5 | Table lens v2 (virtualized, archetype-default columns, relationship chips, row expand) | new `features/table-lens/` |

### Step 4: Commit each checkpoint, push after
```powershell
git -C C:/Code/DevContext2-ui add <files> && git -C C:/Code/DevContext2-ui commit -m "feat(m7.X): ..."
git -C C:/Code/DevContext2-ui push
```

**M4 tools delivered (all 9):** overview, resolve, flow, impact, read_source, find, config, get_context, tests_for.
**M5 delivered:** 5-repo QA ratchet (38 calls, 6889 tok), agent transcript (2c/313tok), CI wiring.
**M6 delivered:** ServiceMapHero, HomeTiles, OnboardingRow, FlowStepper, ServiceCards, Atlas export.
**Baseline:** 493 nodes · 316 edges · 6 ServiceLinks · 34 entries · Style Microservices · 6 per-service styles · Analyzed 3.1s.
