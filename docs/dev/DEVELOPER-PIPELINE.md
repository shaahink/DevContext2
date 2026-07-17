# DevContext — Developer Pipeline

The end-to-end developer pipeline: what to install, how to build/test/run, the gate battery that
must be green before every commit, and how benchmarking, eval-audit, and screenshots fit in. This is
the hands-on companion to `AGENTS.md` (cold start) and `docs/product/AGENT-REFERENCE.md` (engine internals).

Paths are relative to the repo root. Shell is **Windows PowerShell 5.1** (`powershell.exe`; `pwsh` is
not installed). A Bash tool is also available for POSIX scripts.

---

## 1. Prerequisites

| Tool | Version | For |
|------|---------|-----|
| .NET SDK | **10.0.x** (`global.json` pins `10.0.300`, rollForward latestFeature) | engine, server, CLI, MCP |
| Node | **24+** (Angular 22 needs ≥ 22.22.3 / 24) | desktop app |
| pnpm | via `corepack enable` | desktop app package manager |
| Rust + toolchain | stable | Tauri desktop shell only |
| buf | (dev only) | `pnpm gen:proto` TypeScript codegen |

Windows notes: Angular/Tauri need VS Build Tools (VC.Tools) + WebView2 (preinstalled on Win11).
`dotnet --version` should print `10.0.x`.

## 2. Monorepo layout

```
src/
  DevContext.Core/       kernel — analysis pipeline, Graph2 identity spine, BodyFacts, projections,
                         renderers. Roslyn folded in (Microsoft.CodeAnalysis.CSharp package ref).
  DevContext.Cli/        `devcontext` dotnet tool — the primary scriptable surface
  DevContext.Contracts/  proto → C# codegen (Grpc.Tools); the C# side of the gRPC contract
  DevContext.Server/     gRPC-Web backend wrapping Core (analyze-once, query-many)
  DevContext.Mcp/        MCP server — ~24 tools over the gRPC RPCs
  DevContext.App/        Angular 22 (zoneless, signals) + Tauri 2 desktop shell
tests/
  DevContext.Core.Tests/    unit + golden + eval + truth tests
  DevContext.Server.Tests/  gRPC service tests
benchmarks/DevContext.Benchmarks/   macro + micro benchmark runner
proto/devcontext/v1/devcontext.proto   THE gRPC contract (single source of truth)
```

`DevContext.slnx` contains the five C# projects + two test projects + the benchmark project. The
Angular app (`DevContext.App`) is **not** in the slnx — it builds via pnpm. There is no
`DevContext.Desktop` (retired WPF/Avalonia app) and no `DevContext.Roslyn` project.

## 3. Build

Analyzer warnings are errors (MA0016/MA0051/CA1822, CS nullability), so a clean build **is** the gate.

```powershell
dotnet build DevContext.slnx                 # engine + server + CLI + MCP + tests  (0w / 0e)
dotnet build src/DevContext.Cli              # rebuild the CLI after any Core edit (see below)
```

> **Stale-CLI trap:** `DevContext.Cli/bin` carries its own copy of `DevContext.Core.dll`. After
> editing Core, rebuild the CLI (or the whole slnx) before running it — otherwise the CLI runs stale
> engine code and you debug a ghost. The benchmark runner has the same trap.

Desktop app:

```powershell
cd src/DevContext.App
pnpm install
pnpm build                                   # production Angular build
```

## 4. Test

```powershell
# Engine (from repo root):
dotnet test DevContext.slnx --filter "Category!=Eval"     # fast unit + integration
dotnet test DevContext.slnx --filter "Category=Eval"      # eval expectation suite (real repos)
dotnet test DevContext.slnx --filter "Category=Truth"     # truth gates (skips = pending ratchet)
$env:UPDATE_GOLDENS=1; dotnet test DevContext.slnx; $env:UPDATE_GOLDENS=$null   # regenerate goldens — review the diff, never blind

# Desktop app (from src/DevContext.App):
pnpm test                                                  # Vitest, one-shot
```

Test projects: `DevContext.Core.Tests` (graph, map, trace, query, eval, goldens, truth) and
`DevContext.Server.Tests` (gRPC services). Goldens live in `tests/goldens/`; eval expectations in
`eval/expectations/*.json` and `eval-repos.json`.

Live gRPC-Web smoke (drives the real server with the same client the app uses):

```powershell
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1          # server up in background
node --experimental-strip-types src/DevContext.App/scripts/grpcweb-smoke.mts
```

## 5. Gate battery (green before every commit)

Run all of these; a red gate means the session is **not** done — fix or record it.

```powershell
# Repo root — engine:
dotnet build DevContext.slnx                              # 0w / 0e
dotnet test  DevContext.slnx --filter "Category!=Eval"    # fast tests
dotnet test  DevContext.slnx --filter "Category=Truth"    # truth gates
powershell -File scripts/loom-guards.ps1                  # banned patterns + truth gate
powershell -File eval/gates.ps1                           # build → fast tests → eval → CLI --strict matrix

# src/DevContext.App — desktop app:
pnpm check                                                # lint + test + build
```

- **`scripts/loom-guards.ps1`** — bans the pre-Loom paths: no `System.Text.RegularExpressions` in
  `Core/Graph/`, no `new SymbolId(` outside `Graph2/` or tests, no `fqns[0]` in `Graph2/`; then runs
  the `Category=Truth` gate (actual failures ban; skips are the pending ratchet). Exit 0 = clean.
- **`eval/gates.ps1`** — the self-validation gate: build → fast tests (`Category!=Eval&Category!=CliSmoke`)
  → eval expectations → a CLI `--strict` matrix (`--strict`, `--format json/html --strict`, `--dry-run`,
  `--max-tokens 2000 --strict`) → `pnpm check`. Prints `GATE: PASS` / `GATE: FAIL (step N)`.
  **Scopes (T7.0):** `-Scope full` (default; the only boundary-citable form) · `-Scope engine`
  (skips the app check) · `-Scope app` (build + app check, ~90s) · `-SkipEval` (mid-stage fast
  form). Non-full verdicts self-label "not a merge gate". The eval step runs split across two
  test hosts (`-SerialEval` to disable) and is engine-stamp cached: a green run writes
  `eval/.eval-stamp.json`; while the hash of Core/CLI sources + Core tests + expectations +
  fixtures is unchanged, Step 3 skips and the previous verdict transfers. At a boundary, launch
  the full battery DETACHED (`Start-Process` + redirect to a log, poll for `GATE:`) and keep
  working on the next checkpoint — only push/merge waits for green.
- **`pnpm check`** = `pnpm lint && pnpm test && pnpm build`.

## 6. Run the app

**Never run these in the foreground from an agent** — they block the terminal forever and hang the
session. Use the background launcher.

```powershell
# Background (agent-safe): starts .NET server + Angular dev server as PowerShell Jobs
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1 -Status
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1 -Kill

# Foreground (human only):
cd src/DevContext.App
pnpm server        # just the .NET server @ http://127.0.0.1:5179
pnpm dev:web       # server + Angular @ http://localhost:4200  (browser)
pnpm dev           # server + `tauri dev` (native window)
```

The UI polls `Ping`/`/health`; the connection dot in the source bar shows server readiness.

**CLI** (the primary agent surface — see `run-devcontext` skill):

```powershell
dotnet run --project src/DevContext.Cli -- analyze C:\abs\path\to\repo                 # Map (overview)
dotnet run --project src/DevContext.Cli -- analyze C:\abs\path --focus OrderService    # Trace from a type
dotnet run --project src/DevContext.Cli -- analyze C:\abs\path --format json --strict  # JSON + self-check
```

Always pass an **absolute** local path — a relative path is parsed as a GitHub `owner/repo` and
triggers a clone. Full CLI surface is in `docs/product/AGENT-REFERENCE.md`.

## 7. Contract / codegen

`proto/devcontext/v1/devcontext.proto` is the single source of truth. It generates **C#** (server &
MCP, via `Grpc.Tools` in `DevContext.Contracts` — regenerated by `dotnet build`) and **TypeScript**
(the app, via buf):

```powershell
cd src/DevContext.App
pnpm gen:proto     # regenerate src/app/core/grpc/gen/** after editing the .proto
```

After editing the proto: rebuild `DevContext.Contracts` (C# stubs) **and** run `pnpm gen:proto` (TS),
then wire server handler + app data-access. Keep the two generated sides in lockstep.

## 8. Benchmark (profile-first)

Perf work is measure → find the hot path from data → fix one lever → re-bench → keep the gate green.
See the **`devcontext-bench`** skill for the worked loop.

```powershell
dotnet build DevContext.slnx                                                          # rebuild first (Core → runner)
dotnet run -c Debug --no-build --project benchmarks/DevContext.Benchmarks -- repos    # macro: real AnalyzeAsync over eval repos
dotnet run -c Debug --no-build --project benchmarks/DevContext.Benchmarks -- repos DntSite TodoApi
```

Results land in `benchmarks/results/PERF-<date>.md`; compare against `benchmarks/results/baseline.md`.
Per-phase (parse/compile/bind/bfs) and per-extractor numbers are the stable signal; total wall swings
with machine load. `scripts/bench.ps1` / `scripts/bench-simple.ps1` are convenience wrappers.

## 9. Eval-audit (output quality)

Run DevContext over a real repo, capture Map + focused Traces, compare to ground truth, write a
findings report. See the **`devcontext-eval-audit`** skill.

- Inputs: `eval-repos/` (local clones), `eval-repos.json` (per-repo expectations), goldens.
- Capture with `Out-File -Encoding utf8` after `[Console]::OutputEncoding = [UTF8Encoding]::new()`
  (avoids `·`/box-char mojibake). Don't pipe the CLI through `Select-Object -First N` — it truncates
  the pipe and corrupts the exit code.
- Report to `eval-results/<Repo>/AUDIT.md`; preserve prior baselines, add `-v2` captures alongside.

## 10. Screenshots

Services must already be running (`--no-spawn`):

```powershell
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1
node --experimental-strip-types src/DevContext.App/scripts/capture-readme.mts --no-spawn
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1 -Kill
```

Playwright rules (see `AGENTS.md`): navigate with `domcontentloaded` (never `networkidle` — live MCP/
SSE connections never idle); wrap each shot in `Promise.race([fn(), timeout(90s)])`; log a heartbeat
during long waits. Related drivers: `visual-gate.mts`, `audit-screenshots.mts`, `ui-audit-drive.mjs`.

## 11. Branch & release

- **Branch:** feature branches off `develop`; PR into `develop`. `main` is always deployable.
  Multiple agents → multiple worktrees (`git worktree add -b <branch> C:/Code/DevContext2-<slug> develop`).
- **Release:** MinVer with a `v` tag prefix drives `.github/workflows/release.yml` — Windows builds,
  tests (fast suite), and `dotnet pack`s the CLI → NuGet (when `NUGET_API_KEY` is set) + GitHub
  Release with the `.nupkg`. The desktop installer bundles `DevContext.Server` as a Tauri
  sidecar (github-ready G8; verified live install). Remaining nicety: the installer version is
  `tauri.conf.json`'s `0.1.0`, not the release tag (tracked in `GITHUB-READY-START.md`).
  ```powershell
  git tag -a v1.2.3 -m "Release notes"; git push origin v1.2.3
  ```

## 12. Skill ↔ pipeline map

| Step | Skill | What it drives |
|------|-------|----------------|
| Build / run / smoke the CLI | `run-devcontext` | CLI smoke driver, direct invocation, desktop launch |
| Benchmark & optimize | `devcontext-bench` | macro/micro benchmark harness, profile-first fix loop |
| Audit output quality | `devcontext-eval-audit` | Map/Trace fidelity vs ground truth, findings reports |
| Full pipeline overview | `dev-pipeline` | this document — gate battery, verify loop, branch discipline |

## Common gotchas

- **Absolute paths to the CLI** — a relative path is parsed as a GitHub `owner/repo` and cloned.
- **Rebuild after a Core edit** — CLI and benchmark runner carry their own `DevContext.Core.dll`.
- **PowerShell mojibakes `·`** (UTF-8 read as ANSI in PS 5.1) — match ASCII markers (`nodes`, `edges`,
  `depth`), not the middot; capture with `Out-File -Encoding utf8` after setting `OutputEncoding`.
- **JSON isn't pure on stdout** — the CLI prints an explanation + stats line around the content. Use
  `-o out.json` and parse the file, not captured stdout.
- **Don't `Select-Object -First N` a CLI pipe** — the truncated pipe kills `dotnet` early and reports
  a bogus non-zero exit code.
