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

## Work items
- **Meridian M0-M4** ✅ — harness gate + wiring truth pass + insight repair + MCP re-architecture + MCP feature set (9/9 tools). See `MERIDIAN-START.md`.
- **Meridian M5** 🔜 — Agent eval ratchet: QA set → 5 repos, token ceilings, CI wiring.
- **Lighthouse L0–L7** ✅ — see `docs/dev/HANDOVER-LIGHTHOUSE.md`.
- **Fable** ✅ — W0-W7 done. See `docs/dev/HANDOVER-FABLE-FINAL.md`.
- **U3 Facet views** ⬜ — blocked on engine E4.

## Resume protocol (post-M4 → M5)
```
git -C C:/Code/DevContext2-ui checkout feat/meridian-m0
git -C C:/Code/DevContext2-ui pull
dotnet build C:/Code/DevContext2-ui/DevContext.slnx
Set-Location C:/Code/DevContext2-ui/src/DevContext.App; pnpm check
# Read MERIDIAN-START.md for handoff + checkpoint state
# Read docs/dev/briefs/meridian-agent-playbook.md
# Read docs/dev/briefs/proposal-meridian.md §M5
# Run baseline: dotnet run --project src/DevContext.Cli -- report <dogfood-path> -o out.md
```

## Next session — M5 agent eval ratchet (M5.1/5.2/5.3)
**Recommended delivery:** Extend QA set to 5 repos, add token ceilings, record agent transcript, CI wiring. See `proposal-meridian.md` §M5.

| # | What | Key files |
|---|------|-----------|
| M5.1 | QA set → 5 repos (eShop, CleanArchitecture, TodoApi, DntSite, dogfood) + per-question token ceilings | `eval/mcp-qa/run.js` |
| M5.2 | Record real agent transcript (checkout question) against MCP shim | `eval-results/<date>/` |
| M5.3 | CI wiring: `McpQa` test category + bench smoke in verify gate | `scripts/bench.ps1`, CI config |

**M4 tools delivered (all 9):** overview, resolve, flow, impact, read_source, find, config, get_context, tests_for.
**Gate:** All M4 tools functional; M0.2 QA suite passes; 0 regressions.
**Baseline:** 493 nodes · 316 edges · 6 ServiceLinks · 34 entries · Style Microservices · 6 per-service styles · Analyzed 3.1s.
