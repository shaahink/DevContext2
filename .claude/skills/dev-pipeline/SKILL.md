---
name: dev-pipeline
description: The DevContext developer pipeline — build, test, the gate battery, run, and branch/merge discipline. Use when starting or finishing a work session on DevContext, running the gate battery, verifying a change end-to-end, setting up a worktree, or when unsure how to build/test/run the monorepo. Points at run-devcontext (CLI), devcontext-bench (perf), and devcontext-eval-audit (output quality) for the specialized steps.
---

DevContext is a .NET 10 monorepo: one engine (`DevContext.Core`, Roslyn folded in) driving a CLI
(`DevContext.Cli`), a gRPC-Web server (`DevContext.Server`), an MCP server (`DevContext.Mcp`), and an
Angular 22 / Tauri desktop app (`DevContext.App`). Proto codegen lives in `DevContext.Contracts`.
There is **no** `DevContext.Desktop` or `DevContext.Roslyn` project — if a doc mentions them, it's stale.

Paths are repo-root-relative. Shell is **Windows PowerShell 5.1** (`powershell.exe`; no `pwsh`).
Integration branch is **`develop`**. Full reference: `docs/dev/DEVELOPER-PIPELINE.md`.

## Start-of-session ritual

1. Read the handoff: `AGENTS.md`, the newest `docs/dev/HANDOVER-*.md`, and the relevant tracker
   (the active `*-START.md` at repo root; closed-phase trackers live in `docs/dev/archive/`).
2. **Isolate if another agent is active** — give yourself a worktree + branch off `develop`:
   ```powershell
   git worktree list                                                   # who is where
   git worktree add -b feat/<slug> C:/Code/DevContext2-<slug> develop
   ```
3. Run the gate battery below **before** you start. If it's red before you touch anything, fix or
   record it — never build on red.
4. State in one line what artifact will prove your work done.

## The gate battery (green before every commit)

The battery is ONE script with a scope knob (T7.0). Pick the scope that matches the change;
only an unqualified `GATE: PASS` from `-Scope full` is citable at a stage/push/merge boundary.

```powershell
powershell -File eval/gates.ps1                # FULL (default): build → fast tests → McpQa →
                                               #   eval → CLI matrix → pnpm check. THE boundary gate.
powershell -File eval/gates.ps1 -Scope engine  # engine-only mid-stage checkpoint (skips pnpm check)
powershell -File eval/gates.ps1 -Scope app     # app-only checkpoint: build + pnpm check (~90s)
powershell -File eval/gates.ps1 -SkipEval      # engine fast form (eval skipped, labeled)
powershell -File scripts/loom-guards.ps1       # banned-pattern check + truth gate — run at every commit
```

Speed facts (measured 2026-07-16): full serial was ~25 min; the eval step now runs SPLIT across
two test hosts (~halves its 12.5 min) and is ENGINE-STAMP CACHED — a green eval writes
`eval/.eval-stamp.json` (hash over Core/CLI sources, Core tests, expectations, fixtures); while
the stamp matches, Step 3 skips with "verdict transfers" and the run is still a citable full
gate. Delete the stamp file to force a re-run. `-SerialEval` restores the one-process eval.

**Overlap rule (don't idle behind the battery):** at a stage boundary, commit → launch the full
battery DETACHED → keep working on the next checkpoint (new branch/worktree) while it runs. Only
push/merge waits for `GATE:` in the log. Launch pattern:

```powershell
Start-Process powershell -ArgumentList @('-NoProfile','-File','eval\gates.ps1') `
  -WorkingDirectory (Get-Location) -WindowStyle Hidden `
  -RedirectStandardOutput 'eval-results\<date>\gates-<tag>.txt' -RedirectStandardError 'eval-results\<date>\gates-<tag>.err.txt'
# then poll the file for "GATE:" — do NOT run the battery as a foreground agent command (10-min cap kills it)
```

Also standing: truth gates `dotnet test DevContext.slnx --filter "Category=Truth"` (skips = pending
ratchet). CI: `.github/workflows/eval.yml` (github-ready strand) runs the eval suite in the cloud —
once that strand is merged, prefer CI for the pre-merge full battery and keep local scoped runs.

## Build & run

```powershell
dotnet build DevContext.slnx                 # everything C#
dotnet build src/DevContext.Cli              # REBUILD the CLI after any Core edit (stale-dll trap)

# Run the app (agent-safe background launcher — NEVER foreground pnpm dev/dev:web/server/tauri):
# T7.0 rewrite: direct processes + pid files (.dev-pids/), taskkill /T tree-kill, works from ANY
# shell, idempotent (re-invoking while up is a 2s no-op), returns promptly (~25s cold).
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1            # start (idempotent)
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1 -Status    # pid + live health per side
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1 -Kill      # tree-kill both + orphan sweep

# Drive the CLI (absolute paths only — a relative path is parsed as a GitHub repo and cloned):
dotnet run --project src/DevContext.Cli -- analyze C:\abs\repo                # Map
dotnet run --project src/DevContext.Cli -- analyze C:\abs\repo --focus Type   # Trace
```

## Verify a change end-to-end

Don't trust the build — exercise the surface you changed:
- **Engine / CLI change** → drive the CLI over a real repo (the `run-devcontext` skill + `smoke.ps1`),
  confirm Map/Trace still reach the expected seams; for output-quality changes use `devcontext-eval-audit`.
- **Perf change** → re-bench the same repos and compare per-phase numbers (`devcontext-bench`);
  trace edge count + content must be unchanged unless intended.
- **App change** → background launcher + `grpcweb-smoke.mts` or a Playwright screenshot pass.
- **Proto change** → rebuild `DevContext.Contracts` **and** `pnpm gen:proto`, then wire server + app in lockstep.

## End-of-session ritual

1. Re-run the gate battery + any truth gates your change touches.
2. Produce the evidence artifact (fresh run under `eval-results/<date>/`).
3. Update the handoff: newest `HANDOVER-*.md` block and/or tracker; append `docs/dev/go-to-program/PROGRESS-LOG.md`.
4. Commit per checkpoint (docs move with code in the same commit). Push. **Never merge unasked.**

## Discipline invariants

- Warnings are errors; a clean `dotnet build` is the gate. `pnpm check` green for app changes.
- Don't write new C# extractors — reform in place.
- Truth gates and goldens ratchet only (loosen never; tighten with a fresh-run diff). Regenerate
  goldens with `$env:UPDATE_GOLDENS=1` and **review the diff** — never blind.
- Docs that name a file/flag/count must still be true after your change.

## Specialized steps → skills

| Need | Skill |
|------|-------|
| Build/run/smoke the CLI, launch the app | `run-devcontext` |
| Benchmark & optimize the analysis pipeline | `devcontext-bench` |
| Audit Map/Trace output quality vs ground truth | `devcontext-eval-audit` |

## Gotchas

- **Rebuild after a Core edit** — CLI and benchmark runner carry their own `DevContext.Core.dll`.
- **Absolute CLI paths** — relative paths clone from GitHub.
- **PowerShell mojibakes `·`** — match ASCII markers (`nodes`/`edges`/`depth`); capture with `Out-File -Encoding utf8`.
- **JSON isn't pure on stdout** — use `-o out.json` and parse the file; don't `Select-Object -First N` a CLI pipe (corrupts the exit code).
- **Never foreground** `pnpm dev`/`dev:web`/`server`/`tauri dev`/`concurrently` — they hang the session.
- **PS 5.1 `Start-Process -PassThru`**: cache `$null = $proc.Handle` immediately or `.ExitCode`
  reads `$null` after exit (and `$null -ne 0` is `$true` — a green child scored as failure).
  Also: PS-level `-RedirectStandardOutput` on a daemon child leaks the caller's stdout handle
  into it — an agent shell piping the script never sees EOF. Let `cmd /c ... > log 2> err` own
  the redirect instead (this is how start-dev-bg.ps1 works now).
- **`.cmd` shims (pnpm) can't be exec'd** by CreateProcess (any redirected Start-Process) —
  launch via `cmd /d /c pnpm …`.
