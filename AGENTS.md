# AGENTS.md — DevContext monorepo

You are in `C:\Code\DevContext2-ui` on branch `feat/lighthouse-l1`.
**Mission:** Lighthouse iteration — open fast, truth pass, harness, insight engine.
The monorepo contains the engine (C#), server (C#), CLI (C#), and desktop app (Angular + Rust/Tauri).

## Start here (every session)
1. Read this file — work items below.
2. `docs/dev/briefs/proposal-lighthouse.md` — the full L0→L7 waterfall plan.
3. `src/DevContext.App/AGENTS.md` — app conventions, run commands, architecture layering.

## Verify loop
```powershell
# From C:/Code/DevContext2-ui/src/DevContext.App
pnpm check          # lint + vitest + build — must be GREEN
pnpm server         # start .NET server (separate terminal)
pnpm dev:web        # start Angular dev server → http://localhost:4200
```

```powershell
# From C:/Code/DevContext2-ui
dotnet build DevContext.slnx                         # 0 warnings
dotnet test  DevContext.slnx --filter "Category!=Eval"
```

## Work items

### L — Lighthouse: Repo Intelligence Iteration 🔶 IN PROGRESS
**Branch:** `feat/lighthouse-l1` · **Spec:** `docs/dev/briefs/proposal-lighthouse.md`

- L0 (truth pass) — **done**. 9 trust-breakers fixed (E1-E9).
- L1 (open fast) — **done** (2026-07-04, 4 commits). Clone registry, snapshot-first open, progress v2, responsiveness. `pnpm check` + `dotnet build` + `cargo check` green on each.
- **Next: L2** — CLI `report` + bench loop (engine-only stage). See proposal §L2.

### F — Fable Workbench Redesign ✅ DONE
**Branch:** `feat/fable-redesign-skeleton` · W0-W7 complete. See `docs/dev/HANDOVER-FABLE-FINAL.md`.
Deferred: sidecar `externalBin` packaging, engine-blocked S1/S2 gaps.

### U3 — Facet views ⬜ BLOCKED (engine must deliver E4 first)

### I11 — Focus Workspace ⬜ SUPERSEDED by F, which is superseded by L

## Hard rules
- `pnpm check` green before every commit. `dotnet build` green for engine changes.
- Docs move with code in the same commit.
- Append `docs/dev/go-to-program/PROGRESS-LOG.md` after every session.
- Do not write new C# extractors — reform in place.

## Resume protocol (cold start)
```
git -C C:/Code/DevContext2-ui checkout feat/lighthouse-l1
git -C C:/Code/DevContext2-ui pull

Set-Location C:/Code/DevContext2-ui/src/DevContext.App
pnpm check
dotnet build C:/Code/DevContext2-ui/DevContext.slnx

# Pick the first work item whose Status != DONE above
# Do Step 0 (reproduce) first, then execute
```
