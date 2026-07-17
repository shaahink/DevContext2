# Contributing to DevContext

## Getting Started

```bash
git clone https://github.com/shaahink/DevContext2.git
cd DevContext2

# Build the engine + server + CLI + MCP (+ tests). Warnings are errors.
dotnet build DevContext.slnx

# Run the tests
dotnet test DevContext.slnx --filter "Category!=Eval"

# Run the CLI against a project (absolute path; a relative path is treated as a GitHub repo)
dotnet run --project src/DevContext.Cli -- analyze C:\abs\path\to\repo
dotnet run --project src/DevContext.Cli -- analyze C:\abs\path --focus OrderService   # trace

# Desktop app (Angular + Tauri)
cd src/DevContext.App
pnpm install
pnpm check          # lint + test + build
```

Full pipeline (build, test, gate battery, run, bench, eval, screenshots): `docs/dev/DEVELOPER-PIPELINE.md`.

## Project Structure

```
src/
  DevContext.Core/       # kernel: analysis pipeline, Graph2 identity spine, BodyFacts, renderers (Roslyn folded in)
  DevContext.Cli/        # `devcontext` dotnet tool — primary scriptable surface
  DevContext.Contracts/  # proto → C# gRPC codegen
  DevContext.Server/     # gRPC-Web backend
  DevContext.Mcp/        # MCP server (24 tools — see docs/product/mcp-reference.md)
  DevContext.App/        # Angular 22 (zoneless, signals) + Tauri 2 desktop
tests/
  DevContext.Core.Tests/    # unit + golden + eval + truth tests
  DevContext.Server.Tests/  # gRPC service tests
proto/devcontext/v1/devcontext.proto   # the gRPC contract — single source of truth
```

There is no `DevContext.Desktop` (retired WPF/Avalonia app) and no `DevContext.Roslyn` project.

## Making Changes

1. Branch from `develop`: `git checkout -b feature/your-feature develop` (or use a worktree — see
   `AGENTS.md`). Multiple agents/contributors → give each its own worktree so nobody edits the same files.
2. Make changes, following existing conventions.
3. Run the gate battery green (see below).
4. If output format changed, regenerate goldens and **review the diff**: `$env:UPDATE_GOLDENS=1; dotnet test DevContext.slnx`.
5. If you edited the proto, rebuild `DevContext.Contracts` **and** run `pnpm gen:proto` in `src/DevContext.App`.
6. Push and open a PR against `develop`.

## Gate battery (green before every commit)

```powershell
dotnet build DevContext.slnx                              # 0 warnings / 0 errors
dotnet test  DevContext.slnx --filter "Category!=Eval"    # fast tests
dotnet test  DevContext.slnx --filter "Category=Truth"    # truth gates
powershell -File scripts/loom-guards.ps1                  # banned-pattern check + truth gate
powershell -File eval/gates.ps1                           # build → fast tests → eval → CLI --strict matrix
cd src/DevContext.App; pnpm check                          # app: lint + test + build
```

## Code Conventions

- All public APIs must have XML documentation.
- Async methods use `ConfigureAwait(false)` and pass `CancellationToken`.
- Use `model.AddDiagnostic()` for expected failures — never throw.
- **Do not add new C# extractors** — reform detection in place.
- Truth gates and goldens ratchet only: loosen never; tighten with a fresh-run diff.
- Graph code: read `docs/dev/briefs/loom-graph-design.md` first; `scripts/loom-guards.ps1` bans the
  pre-Loom paths (no `Regex` in `Core/Graph/`, no `new SymbolId(` outside `Graph2/`, no `fqns[0]` in `Graph2/`).
- Docs move with code in the same commit — a doc that names a file/flag/count must still be true.
