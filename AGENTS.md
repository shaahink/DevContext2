# AGENTS.md — DevContext engine audit worktree

You are working in the `feat/engine-cross-repo-analysis` worktree. **Mission:** make DevContext (a .NET
static-analysis CLI) the go-to lens for **any** .NET repo — at a glance answer *"what is this / how do I use
it / how does any part work"* via correct **archetype**, real **entry points**, and faithful **trace**
dive-ins. Output quality (the Map/Trace the CLI and UI emit) is the product; detection counts are a substrate.

## Start here (every session, before editing code)
1. Read `analysis-exports/ENGINE-AUDIT/README.md` — the hub + resume protocol.
2. Then `analysis-exports/ENGINE-AUDIT/PHASED-PLAN.md` (what to do next, in order) and
   `OUTPUT-CONTRACT.md` (the bar each output must meet). Follow the README per-item loop exactly.
3. Verdicts/evidence for prior findings: `analysis-exports/VERIFIED-PLAN.md`. Canonical output contract:
   `docs/product/ACCEPTANCE.md` + `docs/product/IDEAL-OUTPUT-TARGET.md`. This audit = Phase 10 of
   `docs/dev/plans/UNIVERSAL-LENS-ROADMAP.md`.

## State of truth lives in the repo
Code + tests + `eval/expectations/*.json` + the `ENGINE-AUDIT/` docs. After each work item: verify, grade the
re-captured output against `OUTPUT-CONTRACT.md`, update the scorecard + `BENCHMARK-MATRIX.md` +
`PROGRESS-LOG.md`, and commit. Keep these docs current — the next session resumes from them.

## Hard rules
- **Reform in place; never rewrite extractors.** Evolve the existing engine.
- **Do-not-regress anchors:** `BudgetIndependenceTests` (Map/Trace must stay budget-independent) and the
  `TraceQualityTests` sibling-divergence Facts (the narrow handler bridge stays narrow) must remain green.
- **Token budgeting is NOT the goal** — it governs a dead legacy catalog path and is a removal candidate
  (W9). Do not "fix" the token budget to change the Map/Trace; that path is budget-independent by design.
- **`analyze` needs an ABSOLUTE repo path** — a relative `owner/repo`-shaped path is treated as a GitHub
  clone target. Cloned test repos are in `analysis-repos/`; eval repos in `eval-repos/`.
- Ask the user before any large, destructive, or scope-expanding change.

## Verify loop
```powershell
dotnet build DevContext.slnx                            # 0 warnings (analyzer warnings = errors)
dotnet test  DevContext.slnx --filter "Category!=Eval"  # fast tests
powershell -File eval/gates.ps1                         # full gate (build + tests + eval + CLI matrix)
```
Flip an eval check `aspirational` → `expected` in the **same commit** that fixes its issue.

**Before the gate:** `eval-repos/` here must be populated, else the Eval tier fails on empty repo dirs
(they don't skip). It's junctioned to `C:\code\DevContext2\eval-repos`; if that's gone, re-create with
`New-Item -ItemType Junction -Path eval-repos -Target C:\code\DevContext2\eval-repos` or clone per
`eval-repos.json`. Test repos for the cross-repo audit are separate, in `analysis-repos/`.
