# GITHUB-READY — public-repo readiness tracker

Branch: `feat/github-ready` (worktree `C:/Code/DevContext2-github-ready`, based on `bcae33d` = T4 head,
so every count/claim below is true for the state that merges after Tapestry T4).
Mission: make the repo presentable and functional as a public GitHub project — working CI/CD,
accurate multi-surface docs (CLI / Desktop app / MCP / Server), a clean README, and an honest gap list.

**Done means:** CI runs green on push/PR, a `v*` tag produces a release without touching deleted
projects, every public doc claim is source-verified, and remaining gaps are tracked here — not hidden.

## Checkpoints

| # | Chunk | Scope | Status | Commit |
|---|-------|-------|--------|--------|
| G1 | CI/CD | `ci.yml`: enable push/PR triggers, mirror the gate battery (Debug build, fast tests, loom-guards + truth, CLI `--strict` smoke), add App job (`pnpm check`). `release.yml`: drop the deleted `DevContext.Desktop` job, CLI → NuGet via MinVer, release notes. | done | — |
| G2 | Public docs | README overhaul (multi-surface presentation, honest desktop install story, 24 MCP tools, current agent pointers, de-brittled counts). NEW `docs/product/mcp-reference.md` (all 24 tools, setup snippets). CONTRIBUTING count fix. | pending | — |
| G3 | Reference docs | Bring `docs/dev/CODE-MAP.md`, `docs/product/AGENT-REFERENCE.md`, `cli-reference.md`, `configuration.md`, `desktop-ui.md` up to T2–T4 state (verify counts, paths, flags vs source). | pending | — |
| G4 | Repo audit | CHANGELOG catch-up, root/tracked-file clutter audit (`analysis-exports/`, `eval-results/` 432 tracked files, root phase trackers), desktop-packaging gap, screenshot freshness. Deliverable: audit section below + safe fixes applied. | pending | — |

## Audit findings (2026-07-16, verified against `bcae33d`)

### CI/CD
- `ci.yml` triggers were commented out (manual-only) and the job built Release without loom-guards,
  truth gates, or the App. **Fixed in G1** — CI now mirrors `AGENTS.md`'s gate battery.
- `release.yml` `desktop` job published `src/DevContext.Desktop` — deleted 2026-07-15. Any `v*` tag
  failed the whole workflow. **Fixed in G1** (job removed; see desktop-packaging gap below).
- `eval/gates.ps1` is NOT in CI — it clones real eval repos (network, ~minutes). Candidate for a
  manual/nightly workflow later; the local battery remains the merge gate for eval.

### Desktop packaging gap (open — blocks "download the desktop app")
A packaged Tauri build only works when `DEVCONTEXT_SERVER_DLL` points at a published
`DevContext.Server` (the Rust shell spawns `dotnet <dll>`, `src-tauri/src/lib.rs:64`). Nothing bundles
the server into the Tauri installer yet, and the spawn requires a .NET runtime on the user's machine.
Until a sidecar/publish story lands, the desktop app is **build-from-source** — README says so now.
Work needed: publish Server → Tauri resource + set env in Rust, or ship self-contained sidecar exe.

### Stale claims fixed in G2/G3
- README: "download `DevContext.Desktop.exe`" (WPF, deleted), "23 tools" (24 since T4.5
  `verify_context`), agent pointers to Loom-era docs, hardcoded test counts (518/27) that rot.
- CONTRIBUTING: "~23 tools" → 24.
- MCP had **no public reference doc** — `docs/product/mcp-reference.md` now documents all 24 tools.

### Repo hygiene (open items, deliberate non-actions)
- `eval-results/` — 432 tracked files of internal eval evidence. It's the project's evidence-artifact
  convention, but it's noise for a public repo. Options: keep (honest history), or move to a branch /
  GH releases. **Not touched** — owner call.
- `analysis-exports/` — 21 tracked files of internal audit workspace output; belongs under `docs/dev/`
  or deletion. **Not touched** — referenced by the engine-audit strand (memory: W9 still open).
- Root trackers (`LOOM-START.md`, `MERIDIAN-START.md`, `L3-START.md`, `plan.json`,
  `conductor-*.md`) — closed-phase files at root. Moving them breaks active agents' pointers
  (TAPESTRY-START.md is ACTIVE — never move). Recommend archiving closed ones to
  `docs/dev/archive/` in a quiet window, updating `AGENTS.md` pointers in the same commit.
- Screenshots (`docs/screenshots/*.png`, 12) — captured pre-T4; Context Studio and MCP page have
  since changed. Re-capture via `capture-readme.mts` next time services are up (needs live app).
- CHANGELOG — caught up in G4 with a `[Unreleased]` section summarizing post-v1.0.0 phases.

## Session log

- 2026-07-16 — worktree created off `bcae33d`; baseline `dotnet build DevContext.slnx` green (exit 0);
  full audit of CI, release, README, docs tree done; delivery in progress chunk by chunk (see table).
