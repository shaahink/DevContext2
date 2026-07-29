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
   Its companion `docs/dev/CODE-MAP.md` is the source-verified module map + "where do I change X?" index.
4. `src/DevContext.App/AGENTS.md` — desktop app conventions (Angular layering, run commands, gRPC codegen).
5. `docs/dev/briefs/loom-graph-design.md` — graph-model design authority (**mandatory before touching graph code**).
6. `docs/dev/HANDOVER-TAPESTRY.md` — most recent close-out: post-Tapestry architecture deltas, perf
   truth, known gaps. Its predecessor `HANDOVER-LOOM.md` still holds the Graph2 architecture detail.
7. `proto/devcontext/v1/devcontext.proto` — the gRPC contract; single source of truth for server ⇄ app ⇄ MCP.

## Architecture

```
DevContext.Core       kernel — analysis pipeline, Graph2 identity spine, BodyFacts, projections,
                      renderers. Roslyn is folded in here (Microsoft.CodeAnalysis.CSharp package).
├── DevContext.Cli        `devcontext` dotnet tool — the primary scriptable surface
├── DevContext.Contracts  proto → C# codegen (Grpc.Tools)
├── DevContext.Server     gRPC-Web backend wrapping Core (analyze-once, query-many)
└── DevContext.Mcp        MCP server — 21 tools mapping to the gRPC RPCs (docs/product/mcp-reference.md)

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
powershell -File eval/gates.ps1                           # FULL battery: build → tests → eval → CLI → pnpm check
powershell -File eval/gates.ps1 -Scope app                # app-only checkpoint (~90s); -Scope engine skips app
powershell -File eval/contract-sweep.ps1                  # dead proto fields (Step 1a of the battery; ~2s standalone)
#   full = the only boundary-citable form; eval step is stamp-cached (eval/.eval-stamp.json) +
#   split over two test hosts. At a boundary launch full DETACHED and keep working — see
#   .claude/skills/dev-pipeline/SKILL.md

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

### Tapestry invariants (T-rules — full text in `docs/dev/briefs/proposal-tapestry.md` §1)

- **Detection lands with render + serve + eval in the same checkpoint** (R-T1). The phase's
  recurring defect class was detect≠render — a signal the JSON knows but the map hides. If you
  teach the engine a new fact, prove a surface shows it and a gate pins it.
  **`eval/contract-sweep.ps1` is the mechanical half of this rule** (battery Step 1a, every scope): a response
  field no client reads fails the gate unless `eval/expectations/contract-sweep-allow.txt` says why
  that is correct. Seven fields shipped dead before it existed — adding a proto field without a
  reader is now a gate failure, not a discovery three sessions later.
- **One battery at a time** (R-T5); full battery at boundaries only, launched DETACHED —
  don't run `dotnet build/test` in a worktree while its battery runs (locked-DLL collisions).
- **Truth files change only in dedicated commits citing target-repo sources** (R-T7).
- **Drift table row at every stage end** (R-T8) — `TAPESTRY-START.md` §Baseline drift table.
- New AppEntry surfaces go through the **Entry Surface Catalog** (one descriptor + one builder),
  never ad-hoc detection. Kind is single-sourced from `CodeGraph.Entries`.
- Event wiring has **one join** (`EventWiringProjection`); pack sections and Studio content are
  built **server-side** (the app is a thin client — one `buildContext` path, no client fallbacks).
- Page loads respect the **RPC budget** (≤15/navigation; `GetFlowIndex` + session memo — no
  per-node fan-outs in stores).

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
powershell -File src/DevContext.App/scripts/start-dev-bg.ps1            # start server+web (idempotent; pid files in .dev-pids/)
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

- **`TAPESTRY-START.md` — the most recent phase tracker** (plan: `docs/dev/briefs/proposal-tapestry.md`;
  close-out: `docs/dev/HANDOVER-TAPESTRY.md`). T0–T8 complete 2026-07-17 — read the handover first,
  the tracker for checkpoint-level detail.
- `docs/dev/archive/conductor-DEBT.md` — open engine debt items (SymbolTable member indexing, BodyFacts
  scoping, TfmScore, Flow hardening, audit sweep). Archived location, still-live register.
- `docs/dev/HANDOVER-*.md` — per-phase close-outs (Loom, Meridian, Lighthouse, Fable, Desktop, Library-support). Read the newest for current architecture + known gaps.
- `docs/dev/archive/` — CLOSED phase trackers (`LOOM-START.md`, `MERIDIAN-START.md`, `L3-START.md`, `conductor-*.md`; historical checkpoint tables + handoff blocks). See its `INDEX.md`.
- `docs/dev/go-to-program/PROGRESS-LOG.md` — append one line after every session.
