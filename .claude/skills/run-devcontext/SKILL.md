---
name: run-devcontext
description: Build, run, test, and drive DevContext (a .NET CLI + gRPC server + Angular/Tauri desktop that turns a .NET solution into LLM-ready Map/Trace context). Use when asked to run, start, build, test, smoke-test, or analyze a repo with DevContext, or to launch its desktop app.
---

DevContext is a .NET 10 monorepo whose surfaces share one engine (`DevContext.Core`): a **CLI**
(`src/DevContext.Cli`, the `devcontext` dotnet tool — the primary, scriptable surface), a **gRPC-Web
server** (`src/DevContext.Server`), an **MCP server** (`src/DevContext.Mcp`), and an **Angular 22 /
Tauri desktop** (`src/DevContext.App`). The agent handle is the CLI smoke driver
`.claude/skills/run-devcontext/smoke.ps1`, which builds the CLI and drives the two real artifacts
(Map + Trace) plus the JSON lens against an in-repo fixture.

There is **no** `DevContext.Desktop` (retired WPF app) or `DevContext.Roslyn` project — Roslyn is a
package reference inside Core. All paths below are relative to the repo root. Shell is **Windows
PowerShell 5.1** (`powershell.exe`); `pwsh` is not installed. Full pipeline: `docs/dev/DEVELOPER-PIPELINE.md`.

## Prerequisites

- **.NET SDK 10** (`global.json` pins `10.0.300`, rollForward latestFeature; `10.0.301` works).
  `dotnet --version` should print `10.0.x`.
- **Desktop app only:** Node 24+, pnpm (`corepack enable`), and (for the native shell) Rust + WebView2
  (preinstalled on Win11). The CLI itself is cross-platform.

## Build

Analyzer warnings are errors (MA0016/MA0051/CA1822/CS nullability), so a clean build is the gate.

```powershell
dotnet build DevContext.slnx        # Core, Cli, Contracts, Server, Mcp + tests (net10.0)
```

After editing `DevContext.Core`, **rebuild the CLI project** before running the CLI — its `bin` has
its own copy of `DevContext.Core.dll`, so an unrebuilt CLI runs stale engine code:

```powershell
dotnet build src/DevContext.Cli -clp:ErrorsOnly
```

## Run (agent path) — CLI smoke driver

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .claude/skills/run-devcontext/smoke.ps1
```

Builds the CLI, then runs three checks against `tests/fixtures/MinimalApiProject` and asserts exit
codes + content markers + the stats line. Expected tail: `ALL PASS  (artifacts in …\dc-smoke)`.
Artifacts (the rendered `map.md`, `trace.md`, `out.json`) land in `%TEMP%\dc-smoke`.

Drive any local repo (absolute path; optional trace focus — a type/handler name traces reliably):

```powershell
powershell -File .claude/skills/run-devcontext/smoke.ps1 C:\path\to\repo SomeTypeName
```

| check | asserts |
|---|---|
| Map | exit 0 · `MAP`/`STYLE` in `-o` file · `N nodes · M edges` on stdout |
| Trace | exit 0 · `TRACE` header in `-o` file · `depth D` on stdout |
| JSON | exit 0 · `--format json` output parses |

## Direct CLI invocation

```powershell
dotnet run --project src/DevContext.Cli -- analyze C:\abs\path\to\repo               # Map (overview)
dotnet run --project src/DevContext.Cli -- analyze C:\abs\path --focus OrderService  # Trace from a type
dotnet run --project src/DevContext.Cli -- analyze C:\abs\path --format json --no-roslyn
```

Useful flags: `--focus "<Type|Type:Method|GET /route>"` (repeatable), `--depth N` (1–10),
`--detail signature|salient|full` (default `salient`), `--include-map` (Map sections alongside a trace),
`--no-roslyn` (faster, deterministic), `--lite`/`--fast` (speed tiers), `--stats`, `--strict` (exit 2
on self-check failure), `--include-diagnostics`, `-o <file>` (writes only the rendered content; stdout
also carries an explanation line + the stats summary). The token-budget/scenario flags are **retired**
(hidden no-op stubs) — use `--focus` + `--detail`. Full surface: `docs/product/AGENT-REFERENCE.md`.

## Run (human path) — desktop app

The desktop is the Angular 22 / Tauri app (`src/DevContext.App`), which talks to `DevContext.Server`
over gRPC-Web. **Never** foreground `pnpm dev`/`dev:web`/`server` from an agent — use the background
launcher (see root `AGENTS.md`):

```powershell
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1            # .NET server + Angular @ :4200
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1 -Status
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1 -Kill
```

The desktop wraps the same engine and the same `--focus`/Map/Trace behaviour as the CLI, so **prefer
the CLI smoke for verification** — it's faster and needs no browser/screenshot. For a visual check use
the Playwright capture (`src/DevContext.App/scripts/capture-readme.mts --no-spawn`, services running).

## Test

```powershell
dotnet test DevContext.slnx --filter "Category!=Eval"     # fast unit + integration (Core + Server tests)
dotnet test DevContext.slnx --filter "Category=Truth"     # truth gates (skips = pending ratchet)
$env:UPDATE_GOLDENS=1; dotnet test DevContext.slnx; $env:UPDATE_GOLDENS=$null   # regenerate goldens — review the diff
```

## Gate script (self-validating)

```powershell
powershell -File eval/gates.ps1    # build → fast tests → eval → CLI strict matrix → GATE: PASS/FAIL
```

The gate runs `Category=Eval` expectation tests (real repos: TodoApi, eShop, VerticalSlice,
AutoMapper, DntSite, …) and a 5-command CLI matrix (`--strict`, `--format json --strict`,
`--format html --strict`, `--dry-run`, `--max-tokens 2000 --strict`). All must exit cleanly.
See `docs/dev/DEVELOPER-PIPELINE.md` for the full gate battery, and the `devcontext-bench` /
`devcontext-eval-audit` skills for perf and output-quality work.

## Map & Trace at a glance

- **Trace** (member-anchored, honest about cuts) carries summary sections: **RESULT** (HTTP status per
  verb), **NEXT** (lifecycle hints from emitted events), **TOUCHES** (entities reachable), **EMITS**
  (deduped events), and a once-rendered **Pipeline** line for cross-cutting behaviors.
- **Map** shows architecture style, project topology, **Entry→target** (route → handler/service),
  entry groups (Domain handlers / Bus consumers / Background workers), aggregates (genuine DDD only),
  and CROSS-CUTTING pipeline behaviors + packages.

## Gotchas

- **Relative paths clone from GitHub** — `analyze eval-repos/Foo` is parsed as `owner/repo`. Always
  pass an **absolute** local path (or `--repo <url>` for an explicit GitHub URL).
- **`Select-Object -First N` on the CLI corrupts the exit code** — the truncated pipe kills `dotnet`
  early and reports `-1` even on success. Capture full output (to a var or `-o` file), then assert.
- **JSON isn't pure on stdout** — the CLI prints an explanation line + stats summary around the
  content. Use `-o out.json` and parse that file.
- **Stats line is stdout-only** — `… N nodes · M edges · depth D …` is printed, not written to the
  `-o` file. Check stdout for it; check the `-o` file for `MAP`/`STYLE`/`TRACE`.
- **`Get-Content` mojibakes the `·` separator** (UTF-8 read as ANSI in PS 5.1). Match ASCII markers
  (`nodes`, `edges`, `depth`), not the middot.
- **A minimal-API route focus may fall back to the Map** without Roslyn (the route node has no
  followable edges). A **type/handler** focus (`CreateOrderHandler`) traces reliably.

## Troubleshooting

- **`pwsh : not recognized`** — only Windows PowerShell 5.1 is installed; invoke `powershell`, not `pwsh`.
- **CLI shows old output after a Core edit** — you ran a stale CLI; `dotnet build src/DevContext.Cli` first.
- **`Repository not found` on a local path** — it was parsed as a GitHub repo; pass an absolute path.
