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
- **Meridian M0-M3** ✅ — harness gate + wiring truth pass + insight repair + MCP re-architecture (COMPLETE). See `MERIDIAN-START.md`.
- **Meridian M4** 🔜 — MCP feature set: 6/9 tools delivered (overview, resolve, flow, read_source, find, get_context v2). Remaining: impact, config, tests_for.
- **Lighthouse L0–L7** ✅ — see `docs/dev/HANDOVER-LIGHTHOUSE.md`.
- **Fable** ✅ — W0-W7 done. See `docs/dev/HANDOVER-FABLE-FINAL.md`.
- **U3 Facet views** ⬜ — blocked on engine E4.

## Resume protocol (post-M3+M4 partial → M4 completion)
```
git -C C:/Code/DevContext2-ui checkout feat/meridian-m0
git -C C:/Code/DevContext2-ui pull
dotnet build C:/Code/DevContext2-ui/DevContext.slnx
Set-Location C:/Code/DevContext2-ui/src/DevContext.App; pnpm check
# Read MERIDIAN-START.md for handoff + checkpoint state
# Read docs/dev/briefs/meridian-agent-playbook.md
# Read docs/dev/briefs/proposal-meridian.md §M4 (remaining: M4.4/4.7/4.9)
# Run baseline: dotnet run --project src/DevContext.Cli -- report <dogfood-path> -o out.md
```

## Next session — M4 completion (remaining: M4.4/4.7/4.9) + optional M5 prep
**Recommended delivery:** 3 bullet items (M4.4 impact, M4.7 config, M4.9 tests_for) in one session. See `proposal-meridian.md` §M4.

| # | What | Key files |
|---|------|-----------|
| M4.4 | `impact` transitive + diff-aware: BFS over out-edges for downward impact, `files=[...]` mode for "changed these files" analysis | `DevContextTools.cs`, `GraphQuery.cs`, `DevContextGrpcService.cs` |
| M4.7 | `config` keys → binding/consumption sites: scan IConfiguration/GetValue/GetSection usages for specific config keys | `DevContextTools.cs`, `GraphQuery.cs` |
| M4.9 | `tests_for` best-effort: find test methods whose call closure reaches a target node | `DevContextTools.cs`, `GraphQuery.cs` |

**M4 tools delivered this session (M4.1/4.2/4.3/4.5/4.6/4.8):** See `DevContextTools.cs` lines 72–171 for `overview`, `resolve`, enhanced `trace`/`flow`, `read_source`/`find`, and `ContextPackBuilder.cs` for `get_context` v2 identity.
**Gate:** all M4 tools functional against dogfood repo; M0.2 QA suite passes current tools.
**Baseline (post-M3):** 493 nodes · 316 edges · 6 ServiceLinks · 36 entries · Style Microservices · 6 per-service styles.
