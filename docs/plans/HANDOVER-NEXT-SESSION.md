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

1. **G1-proper — multi-project/closure rescope · Critical · FOUNDATIONAL · plan it.** Pointing at a
   *subfolder* (`eShop/src/Ordering.API`) still analyses only that project's closure → `(1 project)`,
   no data/domain-event seams. The `.slnx` groundwork (#a21d5e9) is the prerequisite, not the fix.
   Touches `ProjectRootResolver`/`SolutionScope`/discovery + has perf (eShop = 24 projects) and
   eval-expectation fallout. Biggest quality lever; needs its own plan + user buy-in.

2. **G3 — library archetype · High · sizable · plan it.** AutoMapper renders as `NLayer` with `0
   entries` and no public surface. Need: archetype detection (no app entries + packable → Library), a
   `LibrarySurface` builder over public types/methods, a surface renderer (reuse the `NarrativeSections`
   fragment pattern). New feature; design §4/§5.5.

3. **Polish batch · surgical · one checkpoint.**
   - **G7 residual:** STACK line still reads the MediatR *package* signal, so a scoped sub-project shows
     "Minimal APIs" in STACK while STYLE correctly says CleanArchitecture. Light the MediatR signal from
     handler types too (where STACK is built) for consistency.
   - **G9:** PACKAGES lists are long/low-signal — cap or group.
   - **FastEndpoints `<dynamic>` routes:** `Configure()`-set routes collapse to one `GET <dynamic>` node
     (visible in VerticalSlice). G2 already suppresses misleading targets for these.

4. **G5 — minimal-API per-endpoint precision · Medium · hard · design later.** All minimal-API
   endpoints in one registration method share the owner Type node, so trace body/`→ target` don't match
   the specific route. Needs per-endpoint (member/lambda-level) graph nodes. Real work; defer.

## Suggested next-session sequence

Land the **#3 polish batch** first (safe, visible, no buy-in needed) → then write a plan for
**#1 G1-proper** and get the user's nod before touching scope (biggest lever, but foundational). **#2 G3**
and **#4 G5** are separate features, each its own plan.
