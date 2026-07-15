# AGENTS.md — DevContext monorepo

Integration branch: **`develop`** (open PRs here). Monorepo: a C# engine + gRPC server + CLI + MCP
server, and an Angular 22 / Tauri desktop app. One engine (`DevContext.Core`) powers every surface.

> **What it does:** point it at any .NET repo and it produces a **Map** (what's here) and a **Trace**
> (how things connect) — sized for an LLM prompt, readable by a human, honest about how it got there.

## Cold start (read in this order)

1. `README.md` — product tour, features, quickstart.
2. `docs/dev/DEVELOPER-PIPELINE.md` — **the developer pipeline**: build, test, gate battery, run, bench,
   eval, screenshots, branch/release. Start here for anything hands-on.
3. `docs/product/AGENT-REFERENCE.md` — engine internals: ANALYZE→RENDER pipeline, Graph2, contracts, models.
4. `src/DevContext.App/AGENTS.md` — desktop app conventions (Angular layering, run commands, gRPC codegen).
5. `docs/dev/briefs/loom-graph-design.md` — graph-model design authority (**mandatory before touching graph code**).
6. `docs/dev/HANDOVER-LOOM.md` — most recent engine close-out: architecture, benchmarks, known gaps.
7. `proto/devcontext/v1/devcontext.proto` — the gRPC contract; single source of truth for server ⇄ app ⇄ MCP.

## Architecture

```
DevContext.Core       kernel — analysis pipeline, Graph2 identity spine, BodyFacts, projections,
                      renderers. Roslyn is folded in here (Microsoft.CodeAnalysis.CSharp package).
├── DevContext.Cli        `devcontext` dotnet tool — the primary scriptable surface
├── DevContext.Contracts  proto → C# codegen (Grpc.Tools)
├── DevContext.Server     gRPC-Web backend wrapping Core (analyze-once, query-many)
└── DevContext.Mcp        MCP server — ~24 tools mapping to the gRPC RPCs

DevContext.App        Angular 22 (zoneless, signals) + Tauri 2 desktop shell; talks to Server over gRPC-Web
```

There is **no** `DevContext.Desktop` or `DevContext.Roslyn` project — the WPF/Avalonia desktop was
retired in favour of the Angular/Tauri app, and Roslyn is a package reference inside Core. If a doc
still mentions those projects, it is stale.

## Gate battery (green before every commit)

```powershell
# From repo root — engine:
dotnet build DevContext.slnx                              # 0 warnings / 0 errors (warnings are errors)
dotnet test  DevContext.slnx --filter "Category!=Eval"    # fast unit + integration
dotnet test  DevContext.slnx --filter "Category=Truth"    # truth gates (skips are the pending ratchet)
powershell -File scripts/loom-guards.ps1                  # banned-pattern check + truth gate
powershell -File eval/gates.ps1                           # build → fast tests → eval → CLI --strict matrix

# From src/DevContext.App — desktop app:
pnpm check                                                # lint + vitest + production build
```

`dotnet build DevContext.slnx` covers `Cli`, `Contracts`, `Core`, `Mcp`, `Server` and the two test
projects (`DevContext.Core.Tests`, `DevContext.Server.Tests`). **Rebuild the CLI after a Core edit** —
`dotnet build src/DevContext.Cli` — its `bin` carries its own copy of `DevContext.Core.dll`, so an
unrebuilt CLI runs stale engine code.

## Hard rules

- Gate battery green before every commit; `dotnet build` 0w/0e for engine changes; `pnpm check` for app changes.
- Docs move with code in the **same commit** — if a doc names a file/flag/count, it must still be true.
- Do not write new C# extractors — reform in place.
- Truth gates and goldens are **ratcheted** only: loosen never; tighten with a fresh-run diff. Unit
  tests that pin internal string mechanics may be deleted when their subject dies.
- Commit before starting work; push after finishing. **Never merge unasked.**

## Branch & merge discipline

- Integration branch is `develop`; feature branches branch from and PR into `develop`.
- `main` is always deployable; tagged releases (`v*`) publish the CLI to NuGet and the desktop to GitHub Releases.
- **Worktrees:** this repo is often driven by several agents at once. Give each its own worktree +
  branch so nobody edits the same files under another agent:
  ```powershell
  git worktree add -b feat/my-thing C:/Code/DevContext2-<slug> develop
  git worktree list        # see who is where
  ```
  Never assume a fixed worktree path in a doc — resolve it with `git worktree list`.

## AI agent process management

Agents MUST NOT run foreground-blocking servers (`pnpm dev`, `pnpm dev:web`, `pnpm server`,
`concurrently`, `tauri dev`) — they block the terminal forever and hang the session. Use the
background launcher instead:

```powershell
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1            # start .NET server + Angular dev server as Jobs
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1 -Status    # check status
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1 -Kill      # kill all
```

### Screenshot capture (Playwright)

Services must already be running (`--no-spawn`):

```powershell
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1
node --experimental-strip-types src/DevContext.App/scripts/capture-readme.mts --no-spawn
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1 -Kill
```

### Anti-patterns — NEVER do these

| Anti-pattern | Why it hangs the agent |
|---|---|
| `pnpm dev` / `dev:web` / `concurrently` / `tauri dev` in foreground | Blocks the terminal forever — the command never returns |
| `spawn(..., { detached: true }).unref()` in a Node script | Does **not** reliably detach on Windows — the child re-attaches to the shell and blocks on stdio |
| `page.goto(..., { waitUntil: 'networkidle' })` | Hangs forever on pages with live connections (MCP, WebSocket, SSE). Use `domcontentloaded` |
| Per-shot timeouts > 90s, or `waitForSelector(timeout: 300_000)` | One hung shot blocks the rest and the agent sees no output. Wrap each shot in `Promise.race([fn(), timeout(90s)])` with heartbeat logging |
| Silent waits > 15s | Always log "still waiting for X (Ns elapsed)" so the agent sees progress |

## Where the work is tracked

- `conductor-DEBT.md` — open engine debt items (SymbolTable member indexing, BodyFacts scoping, TfmScore, Flow hardening, audit sweep).
- `docs/dev/HANDOVER-*.md` — per-phase close-outs (Loom, Meridian, Lighthouse, Fable, Desktop, Library-support). Read the newest for current architecture + known gaps.
- `LOOM-START.md` / `MERIDIAN-START.md` — phase trackers (historical checkpoint tables + handoff blocks).
- `docs/dev/go-to-program/PROGRESS-LOG.md` — append one line after every session.
