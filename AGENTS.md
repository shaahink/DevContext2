# AGENTS.md — DevContext go-to program worktree

You are working in the `docs/go-to-engine-audit` worktree. **Mission:** execute the go-to program — make
DevContext (a .NET static-analysis CLI) the go-to lens for **any** .NET repo. The program covers engine
trust/fidelity (I1), CLI v2 + kernel wire (I2), Insights engine (I3), desktop UX (I4), facet menu (I5),
MCP server (I6), and benchmark/coverage (I7).

## Start here (every session, before editing code)
1. Read `docs/dev/go-to-program/README.md` — the hub + iteration tracker.
2. Then `docs/dev/go-to-program/PROGRAM-PLAN.md` (phases V1–V5 with votes) and the specific iteration
   guide for the work item you're picking up.
3. Reference docs: `ENGINE-VALUE-AUDIT.md` (engine per-shape status), `FACES-DESIGN.md` (CLI v2 + UX spec),
   `DEV-PAINS.md` (demand-side), `UI-UX-GUIDELINES.md` (design contract for UI work).

## State of truth lives in the repo
Code + tests + `eval/expectations/*.json` + the `docs/dev/go-to-program/` docs. After each work item: verify,
update the iteration guide's status section, and commit. Keep these docs current — the next session resumes
from them.

## Hard rules
- **Reform in place; never rewrite extractors.** Evolve the existing engine.
- **Do-not-regress anchors:** `BudgetIndependenceTests` (Map/Trace must stay budget-independent) and the
  `TraceQualityTests` sibling-divergence Facts (the narrow handler bridge stays narrow) must remain green.
- **Docs move with code, same commit:** any flag/op change edits `docs/product/cli-reference.md`; any UI
  change edits `docs/product/desktop-ui.md`.
- **One wire contract:** anything a face shows must exist as a `GraphQuery` op / kernel JSON field first.
- Ask the user before any large, destructive, or scope-expanding change.

## Verify loop
```powershell
dotnet build DevContext.slnx                            # 0 warnings (analyzer warnings = errors)
dotnet test  DevContext.slnx --filter "Category!=Eval"  # fast tests
powershell -File eval/gates.ps1                         # full gate (build + tests + eval + CLI matrix)
```

**Before the gate:** `eval-repos/` must be populated, else the Eval tier fails on empty repo dirs
(they don't skip). Junction to `C:\code\DevContext2\eval-repos` or clone per `eval-repos.json`.
