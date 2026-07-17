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
| G6 | Eval workflow | `eval.yml` (weekly + manual): submodule pins + clones the 5 non-submodule pinned repos from `eval/README.md`, runs `eval/gates.ps1 -SkipMcpQa` (new opt-in switch; step 2b needs the machine-local dogfood repo). 120-min timeout. | done | `09b1e98` |
| G7 | Tracker archive | Closed-phase root trackers (`L3-/LOOM-/MERIDIAN-START.md`, `conductor-*.md`, `plan.json`) → `docs/dev/archive/` (INDEX updated); live pointers fixed (AGENTS.md, dev-pipeline skill). | done | `179c414` |
| G8 | Desktop sidecar | `pnpm publish:server` → `src-tauri/resources/server`, bundled via `bundle.resources`; Rust falls back env var → bundled DLL, `CREATE_NO_WINDOW` on spawn; desktop job restored in `release.yml` (installers attach to releases); README flips to install-from-Releases. Verified: local build → both installers; release exe spawned bundled server, `/health` 200, graceful close kills child. | done | `22768be` |
| G9 | Screenshots | All 12 re-captured at T4 state vs live app (dogfood eShop). 3 retaken past the script: Context Studio/Export needed real cards (script never clicked "Add to context"), start shot needed a fresh browser context. | done | `8018ddd` |

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

### Desktop packaging gap — CLOSED in G8
`pnpm publish:server` publishes a framework-dependent `DevContext.Server` into
`src-tauri/resources/server` (runs in `beforeBuildCommand`); `bundle.resources` ships it; the Rust
shell resolves env override → bundled DLL (`lib.rs`, `BaseDirectory::Resource`, exists-guarded so
dev keeps the separate-server flow). Users still need the .NET 10 runtime (`dotnet` spawn) — README
says so. Remaining nicety: the installer version is `tauri.conf.json`'s `0.1.0`, not the release tag.

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
- Screenshots — refreshed to T4 state in G9. Capture-script debt for next time: it selects scope
  rows but never clicks "Add to context" (empty Context Studio), and the final home shot inherits
  the last route (needs a fresh browser context). `start-dev-bg.ps1`'s Start-Job nesting also didn't
  launch `ng serve` from the agent harness — detached `cmd /c` launches worked.
- CHANGELOG — caught up in G4 with a `[Unreleased]` section summarizing post-v1.0.0 phases.

## Remaining (updated 2026-07-17 — strand MERGED)

1. ~~PR `feat/github-ready` → `develop`~~ — **DONE**: merged directly (user-authorized) as
   `0b285f2`, immediately after the Tapestry T4–T8 train (`e1ab299`). Conflict resolutions:
   gates.ps1 kept the T7.0 rewrite with `-SkipMcpQa` grafted (semantic union); `.gitignore` the
   t8 superset; AGENTS.md/PROGRESS-LOG.md unions; DEVELOPER-PIPELINE's pre-G8 "no desktop
   artifact yet" claim un-staled.
2. ~~First `eval.yml` run~~ — **DONE 2026-07-17**: green in **6m03s** (run 29547946883) after
   three first-run fixes it existed to surface: clone loop fetched from a nonexistent `origin`
   remote (fetch by URL now); the dogfood truth test was the only plain-`[Fact]` truth test so
   `Skip.IfNot` FAILED on a dogfood-less machine (`[SkippableFact]` now); the runner had no
   pnpm for gates step 5 (app toolchain setup added). CI (`ci.yml`) green on the same tip —
   first green CI on develop (pnpm action now reads the APP's package.json).
3. ~~Engine strand: empty-fixture-dir→skip~~ — **DONE** by Tapestry T8.3 (`FixtureExists` in
   `TruthExpectationTests`, all 15 sites). Still open: owner decisions on `eval-results/`
   (432 tracked files) + `analysis-exports/`.
4. **Niceties, not blockers** — installer version from the release tag (today `0.1.0` from
   tauri.conf.json); capture-script fixes noted in the screenshots hygiene entry above.

## Session log

- 2026-07-16 — worktree created off `bcae33d`; baseline `dotnet build DevContext.slnx` green (exit 0);
  full audit of CI, release, README, docs tree done; delivery in progress chunk by chunk (see table).
- 2026-07-16 (wrap-up) — G6–G9 delivered: eval.yml, tracker archive, desktop sidecar (verified: local
  build produced NSIS+MSI installers; release exe spawned the bundled server, `/health` 200, graceful
  close cleaned up the child), 12 screenshots refreshed against the live app. CHANGELOG + README
  updated. Strand delivery complete; remaining items are external (see §Remaining).
