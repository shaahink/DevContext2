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
- **Meridian M0-M7** ✅ — Harness gate + wiring truth + insight repair + MCP re-architecture + MCP feature set (9/9 tools) + Agent eval ratchet + Home/Atlas cards + Explore/lenses/chrome/table + Context Studio (scope picker, composition, budget, server tokens, provenance chips).
- **Meridian M8** ✅ — Context Studio: M8.1 (surface + scope picker + composition + budget panel + retired old panes), M8.2 (composition model, 9 card types, RPC, preset, omnibox, drag-drop, trail seeds), M8.3 (server-side token meter, budget→RPC wiring, per-section tokens), M8.4 (provenance chips, server token display).
- **Meridian M9** 🔜 — Full bench, AUDIT.md, HANDOVER-MERIDIAN.md close-out.
- **Lighthouse L0–L7** ✅ — see `docs/dev/HANDOVER-LIGHTHOUSE.md`.
- **Fable** ✅ — W0-W7 done. See `docs/dev/HANDOVER-FABLE-FINAL.md`.
- **U3 Facet views** ⬜ — blocked on engine E4.

## Resume protocol (post-M8 → M9)
```powershell
git -C C:/Code/DevContext2-ui checkout feat/meridian-m0
git -C C:/Code/DevContext2-ui pull
dotnet build C:/Code/DevContext2-ui/DevContext.slnx
Set-Location C:/Code/DevContext2-ui/src/DevContext.App; pnpm check
# Read MERIDIAN-START.md for handoff + checkpoint state
# Read docs/dev/briefs/meridian-agent-playbook.md
# Read docs/dev/briefs/proposal-meridian.md §M9
```

## Next session — M9 close-out (or engine gaps 1-2)

### Step 0: Commit before starting
```
git -C C:/Code/DevContext2-ui status   # ensure clean
```

### Step 1: Drive M8 UI for verification
Start the app and exercise the new M8 surfaces:
```powershell
pnpm server         # terminal 1
pnpm dev:web        # terminal 2 (opens http://localhost:4200)
```
- **Context Studio (/context):** Analyze the dogfood repo, navigate or Ctrl+E → /context. Verify 3-pane layout renders (scope picker left, composition center, budget right).
- **Token meter:** Add cards from scope picker, verify per-card token bars show server-computed counts. Budget slider changes should flow to RPC budgetTokens. Server tokens show in green without ~ prefix.
- **Provenance chips:** Each card displays file:line provenance chips below the type badge.
- **Copy/Save:** Copy produces token-metered markdown; "Copied!" toast visible for 2s.
- **Bug check:** No console errors, no dead routes, ExportDrawer fully removed (rg "export-drawer" → 0 results).

### Step 2: Read M9 docs
1. `docs/dev/briefs/proposal-meridian.md` §M9 (close-out: full bench, AUDIT.md, handover)
2. `docs/dev/briefs/meridian-agent-playbook.md` §8 (assertion recommendations), §2 (session end ritual)

### Step 3: Plan M9 delivery (or engine gaps)
| # | What | Key files |
|---|------|-----------|
| M9.1 | Full bench: all M0.1 repos + PowerToys + MassTransit | `eval-results/` |
| M9.2 | AUDIT.md with fresh-artifact verdicts only | `eval-results/<date>/AUDIT.md` |
| M9.3 | HANDOVER-MERIDIAN.md + memory + tracker close | `docs/dev/HANDOVER-MERIDIAN.md` |
| Gap 1 | read_source RPC (engine+proto) for Inspector Code tab | `devcontext.proto`, `DevContext.Server`, `inspector.ts` |
| Gap 2 | Layer/feature uplumb (engine+proto) for lens-switcher | `devcontext.proto`, `ProtoMapper`, `lens-switcher.ts` |

### Step 4: Commit each checkpoint, push after
```powershell
git -C C:/Code/DevContext2-ui add <files> && git -C C:/Code/DevContext2-ui commit -m "feat(m9.X): ..."
git -C C:/Code/DevContext2-ui push
```

**M4 tools delivered (all 9):** overview, resolve, flow, impact, read_source, find, config, get_context, tests_for.
**M5 delivered:** 5-repo QA ratchet (38 calls, 6889 tok), agent transcript (2c/313tok), CI wiring.
**M6 delivered:** ServiceMapHero, HomeTiles, OnboardingRow, FlowStepper, ServiceCards, Atlas export.
**Baseline:** 493 nodes · 316 edges · 6 ServiceLinks · 34 entries · Style Microservices · 6 per-service styles · Analyzed 3.1s.
