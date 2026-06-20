---
name: run-devcontext
description: Build, run, test, and drive DevContext (a .NET CLI + WPF desktop that turns a .NET solution into LLM-ready Map/Trace context). Use when asked to run, start, build, test, smoke-test, or analyze a repo with DevContext, or to launch its desktop app.
---

DevContext is a .NET 10 tool with two binaries that share one engine (`DevContext.Core`):
a **CLI** (`src/DevContext.Cli`, the `devcontext` dotnet tool — the primary, scriptable surface)
and a **WPF/BlazorWebView desktop** (`src/DevContext.Desktop`, Windows GUI). The agent handle is
the CLI smoke driver `.claude/skills/run-devcontext/smoke.ps1`, which builds the CLI and drives the
two real artifacts (Map + Trace) plus the JSON lens against an in-repo fixture.

All paths below are relative to the repo root. Shell is **Windows PowerShell 5.1** (`powershell.exe`);
`pwsh` is not installed here.

## Prerequisites

- **.NET SDK 10** (`global.json` pins `10.0.300`, rollForward latestFeature; `10.0.301` works).
  `dotnet --version` should print `10.0.x`.
- **Desktop only:** Windows + the **WebView2 runtime** (preinstalled on Win11). The CLI is
  cross-platform; the desktop is `net10.0-windows` WPF.

## Build

Analyzer warnings are errors (MA0016/MA0051/CA1822/CS nullability), so a clean build is the gate.

```powershell
dotnet build DevContext.slnx        # builds Core, Cli, Roslyn, Desktop (net10.0 / net10.0-windows)
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
dotnet run --project src/DevContext.Cli -- analyze C:\abs\path\to\repo            # Map (overview)
dotnet run --project src/DevContext.Cli -- analyze C:\abs\path --focus OrderService # Trace from a type
dotnet run --project src/DevContext.Cli -- analyze C:\abs\path --format json --no-roslyn
```

Useful flags: `--focus "<Type|Type:Method|GET /route>"`, `--depth N`, `--detail signature|salient|full`,
`--no-roslyn` (faster, deterministic), `--stats`, `--include-diagnostics`, `-o <file>` (writes only the
rendered content; stdout also carries an explanation line + the stats summary).

## Run (human path) — desktop

```powershell
dotnet run --project src/DevContext.Desktop     # opens the WPF window; Ctrl-C / close the window to stop
```

A launch-and-screenshot helper exists (`.claude/skills/run-devcontext/desktop-shot.ps1`) — it builds,
launches, captures the app window to `desktop-launch.png`, and closes it. Reading that PNG is
token-expensive; only use it when a visual check is the point. The desktop wraps the same engine and
the same `--focus`/Map/Trace behaviour as the CLI, so prefer the CLI smoke for verification.

## Test

```powershell
dotnet test tests/DevContext.Core.Tests        # ~255 pass / 2 skip (graph, map, trace, eval, goldens)
dotnet test tests/DevContext.Desktop.Tests     # 64 pass (MVVM, sections)
$env:UPDATE_GOLDENS=1; dotnet test tests/DevContext.Core.Tests   # regenerate goldens, then unset
```

## Gotchas

- **`RepoUrl.Parse` eats relative paths** — `analyze eval-repos/Foo` is parsed as a GitHub
  `owner/repo` shorthand and tries to clone. Always pass an **absolute** local path.
- **`Select-Object -First N` on the CLI corrupts the exit code** — the truncated pipe kills `dotnet`
  early and reports `-1` even on success. Capture full output (to a var or `-o` file), then assert.
- **JSON isn't pure on stdout** — the CLI prints `Overview map (no focus).` and the stats summary to
  stdout around the content. Use `-o out.json` and parse that file, not captured stdout.
- **Stats line is stdout-only** — `… N nodes · M edges · depth D …` is printed, not written to the
  `-o` file. Check stdout for it; check the `-o` file for `MAP`/`STYLE`/`TRACE`.
- **`Get-Content` mojibakes the `·` separator** (UTF-8 read as ANSI in PS 5.1). Match ASCII markers
  (`nodes`, `edges`, `depth`), not the middot.
- **A minimal-API route focus may fall back to the Map** without Roslyn (the route node has no
  followable edges). A **type/handler** focus (`CreateOrderHandler`) traces reliably.
- **`desktop-shot.ps1` can capture a pre-existing instance** — if another DevContext desktop is already
  running (e.g. an installed release), the screenshot may show *that* window, not your freshly-built
  debug app (tell by the version label). Close other instances before launching. The branch's Razor
  compiles into `DevContext.Desktop.dll`, so the built app always reflects current source — verify
  without launching by reading UI marker strings from the DLL as UTF-16
  (`[IO.File]::ReadAllText($dll,[Text.Encoding]::Unicode).Contains('entry-combo')`).

## Troubleshooting

- **`pwsh : not recognized`** — only Windows PowerShell 5.1 is installed; invoke `powershell`, not `pwsh`.
- **CLI shows old output after a Core edit** — you ran a stale CLI; `dotnet build src/DevContext.Cli` first.
- **`Repository not found` on a local path** — it was parsed as a GitHub repo; pass an absolute path.
