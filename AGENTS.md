# AGENTS.md — DevContext monorepo

Branch `feat/lighthouse-l2`. Monorepo: engine + server + CLI (C#), desktop app (Angular + Rust/Tauri).

## Cold start
1. Read `L3-START.md` — the current stage card (branch, checkpoints, key files, gate).
2. `src/DevContext.App/AGENTS.md` — app conventions, run commands, architecture layering.

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
- **Lighthouse L0–L7** ✅ — truth pass, open fast, report + bench + query, kernel answers, insight engine v2, MCP server + context packs, UI/UX round, benchmark close-out audit. See `docs/dev/HANDOVER-LIGHTHOUSE.md`.
- **Fable** ✅ — W0-W7 done. See `docs/dev/HANDOVER-FABLE-FINAL.md`.
- **U3 Facet views** ⬜ — blocked on engine E4.

## Resume protocol
```
git -C C:/Code/DevContext2-ui checkout feat/lighthouse-l2
git -C C:/Code/DevContext2-ui pull
dotnet build C:/Code/DevContext2-ui/DevContext.slnx
Set-Location C:/Code/DevContext2-ui/src/DevContext.App; pnpm check
# Read docs/dev/HANDOVER-LIGHTHOUSE.md for full project state + recommended next steps
```
