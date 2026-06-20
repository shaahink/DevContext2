# Next-session warm start — DevContext output quality

> Purpose: warm a fresh session **cheaply**. Read this first; only open the deep doc
> (`docs/reports/OUTPUT-QUALITY-ASSESSMENT.md`) when you pick up a specific gap.
> Branch: `feat/output-quality-graph` (off `develop`). Working tree clean at handover.

## Warm-start facts (don't re-derive)

- **Stack:** .NET 10 (`global.json` 10.0.300). One engine (`DevContext.Core`) → two binaries:
  CLI (`src/DevContext.Cli`, scriptable) + WPF/BlazorWebView desktop (`src/DevContext.Desktop`, Windows).
- **Shell:** Windows PowerShell 5.1 (`powershell.exe`; no `pwsh`).
- **Build/test/run:**
  ```
  dotnet build DevContext.slnx                 # analyzer warnings = errors
  dotnet test tests/DevContext.Core.Tests      # ~255 pass / 2 skip
  dotnet test tests/DevContext.Desktop.Tests   # 64 pass
  powershell -File .claude/skills/run-devcontext/smoke.ps1   # CLI smoke (Map/Trace/JSON)
  ```
- **Gotchas (bite every time):** pass **absolute** paths to the CLI (`RepoUrl.Parse` treats `a/b` as a
  GitHub repo); **rebuild `src/DevContext.Cli`** after a Core edit (it has its own Core.dll copy);
  `$env:UPDATE_GOLDENS=1; dotnet test` regenerates goldens.
- **Two output artifacts:** **Map** (no focus → architecture/topology/entries) and **Trace** (`--focus`
  → call stack down the wiring). North star: `docs/IDEAL-OUTPUT-TARGET.md`.

## Done this session (committed + pushed)

| commit | what |
|---|---|
| `a21d5e9` | **G1 part:** parse `.slnx` solutions (`SolutionFileParser`), prefer root solution. eShop/AutoMapper/VerticalSlice (`.slnx`-only) now resolve real solution/style/topology when pointed at the **root**. |
| `3d8544c` | **G7:** style detector reads MediatR from handler *types* (Stage-2 interfaces), not the package signal — eShop `Ordering.API` alone now reads CleanArchitecture, not MinimalApi. |
| `4908228` | **G8:** Map/Trace stats line is graph-shaped (`nodes · edges · entries · depth`), not `0 types kept of 0`. |
| (skill) | `.claude/skills/run-devcontext/` — verified CLI smoke driver + desktop launch helper. **`.claude/` is gitignored** → uncommitted; `git add -f` or add `!.claude/skills/` to `.gitignore` to share. |

Earlier on the branch: G2 (entry `→ target`, de-truncate), G4 (dup raises), G6 (topology), B1 (clone cache).

## Remaining gaps + the way to go

Ordered by what I'd do next. "Plan it" = write a short `docs/plans/*.md` and get the user's nod before
coding (foundational/behaviour-changing). "Surgical" = safe, do it inline at a checkpoint.

> ~~Desktop stale-UI~~ — **investigated and dismissed.** A launch screenshot showed the old controls,
> but the built `DevContext.Desktop.dll` contains the new UI (`entry-combo` present; old
> `SYMBOL FOCUS`/`MAX TOKENS` absent) and a clean rebuild changes nothing. The screenshot was a
> *different* instance (it read **v2.0.0**; this branch is **v1.0.5-preview**) — i.e. a pre-existing
> installed/running DevContext, not the freshly-built debug app. No branch bug. (Harness caveat:
> `desktop-shot.ps1` can foreground/capture a pre-existing instance; close other instances first.)

1. **G1-proper — multi-project/closure rescope · DONE** (`feat/polish-batch-and-g1-phase0`:
   P1 `06df05a` · P2 `4c87981` · P3 `1a67291` · P4 `9fb7efb`; policy = **Hybrid (C)**). Project/subfolder
   input now analyses the anchor + transitive `ProjectReference` closure; `.sln`/root input stays
   whole-solution. Verified: `analyze eShop/src/Ordering.API` → `MAP eShop (7-project closure)`,
   CleanArchitecture, cross-project topology; trace crosses into `IntegrationEventLogEF`. Plan:
   `docs/plans/PLAN-G1-multi-project-scope.md`.

2. **G3 — library archetype · High · sizable.** AutoMapper renders as `NLayer` with `0 entries` and no
   public surface. **Plan drafted → `docs/plans/PLAN-G3-library-and-G5-minimal-api.md`.**

3. **Polish batch · DONE (`ce64a0f` on `feat/polish-batch-and-g1-phase0`).**
   - **G7 residual — done:** `ArchitectureStyleDetector.HasMediatREvidence` is the single source of
     truth (package signal OR handler interfaces); the STACK line uses it, so a scoped sub-project no
     longer disagrees with STYLE.
   - **G9 — done:** PACKAGES capped at 8/group with `… (N total)` overflow.
   - **FastEndpoints `<dynamic>` — triaged, no change:** `GraphBuilder` already nulls the target and
     `OutputSelfCheck` guards the literal. Real per-endpoint resolution stays with G5.

4. **G5 — minimal-API per-endpoint precision · Medium · hard.** All minimal-API endpoints in one
   registration method share the owner Type node, so trace body/`→ target` don't match the specific
   route. **Plan drafted → `docs/plans/PLAN-G3-library-and-G5-minimal-api.md`** (second half). Defer.

## Suggested next-session sequence

**Polish batch (#3), G1 (Phase 0 + Phases 1–4), G3, and G5 are all LANDED** on
`feat/polish-batch-and-g1-phase0` (pushed; gate green — build 0-warn · Core 267 pass / 2 skip ·
Desktop 64 pass). The output-quality gap list (G1–G9 + B1) is now cleared. Remaining ideas are
follow-ups: G5's downstream call-body salience (partial by design) and FastEndpoints `<dynamic>`
`Configure()` routes.
