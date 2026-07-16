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
| G1 | CI/CD | `ci.yml`: enable push/PR triggers, mirror the gate battery (Debug build, fast tests, loom-guards + truth, CLI `--strict` smoke), add App job (`pnpm check`). `release.yml`: drop the deleted `DevContext.Desktop` job, CLI → NuGet via MinVer, release notes. | done | `dd276a6` |
| G2 | Public docs | README overhaul (multi-surface presentation, honest desktop install story, 24 MCP tools, current agent pointers, de-brittled counts). NEW `docs/product/mcp-reference.md` (all 24 tools, setup snippets). CONTRIBUTING count fix. | done | `3910202` |
| G3 | Reference docs | Bring `docs/dev/CODE-MAP.md`, `docs/product/AGENT-REFERENCE.md`, `cli-reference.md`, `configuration.md`, `desktop-ui.md` up to T2–T4 state (verify counts, paths, flags vs source). | done | `e3b8782` |
| G4 | Repo audit | CHANGELOG catch-up, root/tracked-file clutter audit (`analysis-exports/`, `eval-results/` 432 tracked files, root phase trackers), desktop-packaging gap, screenshot freshness. Deliverable: audit section below + safe fixes applied. | done | `094faaf` |
| G5 | Fixture submodules | Reconstruct missing `.gitmodules` for `eval-repos/*` gitlinks (fresh clones had a red truth gate); `submodules: true` in CI; `Category=McpQa` excluded from CI filters (machine-local dogfood dep). | done | `2ad227d` |

## Audit findings (2026-07-16, verified against `bcae33d`)

### CI/CD
- `ci.yml` triggers were commented out (manual-only) and the job built Release without loom-guards,
  truth gates, or the App. **Fixed in G1** — CI now mirrors `AGENTS.md`'s gate battery.
- `release.yml` `desktop` job published `src/DevContext.Desktop` — deleted 2026-07-15. Any `v*` tag
  failed the whole workflow. **Fixed in G1** (job removed; see desktop-packaging gap below).
- `eval/gates.ps1` is NOT in CI — it clones real eval repos (network, ~minutes). Candidate for a
  manual/nightly workflow later; the local battery remains the merge gate for eval.
- **Fresh clones had a red truth gate (G5 fix).** `eval-repos/{TodoApi,VerticalSlice,eShop}` were
  committed as gitlinks **without `.gitmodules`** — unfetchable for anyone cloning the repo. Git
  materializes them as empty dirs, so `Skip.IfNot(Directory.Exists(...))` in `TruthExpectationTests`
  doesn't skip — the tests run against empty repos and FAIL (reproduced in this fresh worktree).
  Fixed: `.gitmodules` reconstructed (URLs from the live clones; pins already in the tree:
  TodoApi `307a1ea`, VerticalSlice = ardalis/CleanArchitecture `74624fb`, eShop `9b4f943`) and
  `submodules: true` added to the CI engine checkout. **Open recommendation for the engine strand:**
  make the fixture-absent guard treat an *empty* directory as absent so a submodule-less checkout
  skips instead of failing (test code is owned by the Tapestry strand — not touched here).
- **`Category=McpQa` excluded from CI test filters.** `McpQaGateTests` shells out to
  `eval/mcp-qa/run.js`, which targets a machine-local dogfood repo (`run.js:14`) and a live server
  port — it can never pass on GitHub Actions, and it collides with another agent's live server when
  two sessions share the machine (observed in this worktree: 508/512 pass, only McpQa red while the
  Tapestry agent was mid-session). It remains a serial step in `eval/gates.ps1` locally.

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

### Repo hygiene — fixed in G4
- `.gitignore` had 7 lines of UTF-16 text embedded in the UTF-8 file (a PS 5.1 append artifact) —
  the `_eval-dntsite/` / `devcontext-*.md` eval-artifact patterns were unreadable to git and matched
  nothing. Rewritten clean.
- CHANGELOG was frozen at v1.0.0 (2026-06-11) — added an `[Unreleased]` section summarizing
  Lighthouse, Loom, Meridian, the desktop redo, Tapestry T0–T4, and this strand.

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
