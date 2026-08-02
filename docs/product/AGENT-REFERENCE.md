# DevContext — Tool Reference for Agents

Engine-internals reference for the current (post-Loom, Graph2) architecture. For the hands-on
pipeline (build/test/run/gate) see `docs/dev/DEVELOPER-PIPELINE.md`; for the graph-model design
authority see `docs/dev/briefs/loom-graph-design.md`.

## What it is

DevContext turns any .NET solution into a **structured code graph** and renders two artifacts from it:
a **Map** (what's in the repo — architecture style, topology, entry points, packages) and a **Trace**
(how things connect — endpoint → handler → event → consumer → entity → DI). Both are sized for an LLM
prompt, readable by a human, and carry provenance (`file:line`) for how each fact was derived.

**Surfaces** (all wrap one engine, `DevContext.Core`):

| Surface | Project | Notes |
|---------|---------|-------|
| CLI (`devcontext`) | `DevContext.Cli` | Primary scriptable surface; `dotnet tool install -g DevContext.Cli` |
| Desktop | `DevContext.App` | Angular 22 (zoneless, signals) + Tauri 2; talks to the server over gRPC-Web |
| gRPC-Web server | `DevContext.Server` | Analyze-once/query-many backend the desktop calls |
| MCP server | `DevContext.Mcp` | 22 tools mapping to the gRPC RPCs, for AI-agent integration |
| Contract codegen | `DevContext.Contracts` | proto → C# stubs (Grpc.Tools) |

> There is **no** `DevContext.Desktop` (retired WPF/Blazor/Avalonia app) and **no** `DevContext.Roslyn`
> project — Roslyn is a `Microsoft.CodeAnalysis.CSharp` package reference inside `DevContext.Core`.

## The core model: analyze once, query many

Analysis is **immutable**: `Analyze` builds an `AnalysisSnapshot` (the code graph + model + options)
once. Map, Trace, Node, Neighbors, Search, Context, etc. are cheap **render-time queries** over that
same snapshot — never a re-analysis. The server keeps snapshots per session; the desktop holds a
session handle and issues query RPCs as you navigate.

### gRPC contract (`proto/devcontext/v1/devcontext.proto` — single source of truth)

The proto is the authority for server ⇄ app ⇄ MCP. RPCs:

```
Session:   Analyze · ListSessions · CloseSession · Ping · GetStats
Map:       GetMap · ListEntryPoints · GetGraphFacets · GetInterestingPoints
Trace:     GetTrace · GetNeighbors · GetNode · GetImpact
Search:    SearchNodes · FindTestsFor · ConfigLookup
Source:    ReadSource · Render
Context:   GetContext · GetContextPack · VerifyContext
MCP mgmt:  StartMcp · StopMcp · ObserveToolCalls
```

Editing the proto: rebuild `DevContext.Contracts` (C# stubs) **and** run `pnpm gen:proto` (TypeScript)
so both sides stay in lockstep, then wire the server handler + app data-access.

## Engine internals

`DevContext.Core` runs a two-phase pipeline (`Pipeline/DiscoveryPipeline.cs`). For the file-level map
of every module, see `docs/dev/CODE-MAP.md`; for the graph-model design authority (identity laws,
prohibitions), `docs/dev/briefs/loom-graph-design.md`.

**ANALYZE** (`AnalyzeAsync` — builds the immutable `AnalysisSnapshot`):
1. **Stage 1** (sequential) — discovery + parse-once cache warmup.
2. **Stage 2** (parallel) — syntax structure, package→signal mapping, layer classify, DI regs,
   Program.cs flow, and per-member **BodyFacts**.
3. **Seal** — resolve focus points, seal signals, run `ArchitectureStyleDetector` (no more signal writes).
4. **Stage 3** (parallel, two waves — detectors then the `Deep` call graph) — endpoints, controllers,
   MediatR, EF Core, event bus, anti-patterns, call graph.
5. **Semantic-lite (Tier B)** — `Graph2/SemanticLitePopulator` builds real compilations from `assets.json`
   and upgrades BodyFacts + call edges to `Semantic` resolution (Debug/Full profile or full-graph builds).
6. **Graph assembly** — `Graph/GraphBuilder.Build` joins detections + types + seams into the `CodeGraph`
   and entry points; `MapBuilder` derives the `MapModel`.
7. **Insights** + **compression**, then freeze into the snapshot.

**The identity spine** (`Graph2/SymbolTable`): every unresolved name goes through `Resolve`, a **monotone
resolution-tier ladder** — `Semantic` (verified, never downgraded) → `Declared` (exact FQN) →
`ProjectScoped` → `GlobalUnique` → `Ambiguous` → `Unresolved`. Ambiguous names are carried as
`Candidates` and **never blindly first-matched** (Laws R1/R2). **Seam detectors** (`Graph2/Seams/`,
`ISeamDetector`) read BodyFacts and emit `SeamMatch` records — they never write the graph; the assembler
resolves each target through the SymbolTable and skips ambiguous ones. This is the post-Loom "regex
funeral" (zero body-scan regexes in `Graph/`, enforced by `scripts/loom-guards.ps1`).

> **Kernel invariant:** the CodeGraph + Map/Trace are assembled *before* scoring/compression and never
> read the token budget or pruner state — "token budget out of the kernel," locked by
> `BudgetIndependenceTests`. Map/Trace output is invariant across `--max-tokens`.

**RENDER** (cheap, repeatable, lens-driven): a `RenderRequest` selects format + sections + focus;
`RenderPlanBuilder` computes the plan and the renderers emit Markdown / JSON / HTML from the snapshot —
a client of the query layer (`Graph/GraphQuery`), never a re-analysis.

### Edge model (trace priority)

The trace is a structural traversal over the typed graph. Each edge carries provenance (`file:line`),
resolution (Join / Syntactic / Semantic), and confidence. Higher-priority edges win when the traversal
must choose:

| Edge | Priority | Built from |
|------|----------|-----------|
| Sends | 0 (highest) | `.Send(new XCommand())` / `.Publish(new XEvent())` |
| Handles | 1 | MediatR handler join |
| Raises | 2 | `AddDomainEvent(new XEvent())` |
| Consumes | 3 | event → handler join |
| ReadsWrites | 4 | EF entity + body reference |
| Resolves | 5 | DI registration + single implementor |
| WrappedBy | 6 | `IPipelineBehavior` |
| Calls | 7 (lowest) | Roslyn call graph |

Depth limit (default 6), fan-out cap, framework-boundary detection, revisit guard, and cycle breaking
keep traces focused.

### `DevContext.Core` layout

`Graph/` (model + `GraphBuilder` + `GraphQuery` + `TraceBuilder` + 11 entry-point builders) ·
`Graph2/` (identity spine — `SymbolTable`, `BodyFacts`, 6 seam detectors, `SemanticLitePopulator`) ·
`Pipeline/` (orchestration, `AnalysisSnapshot`, `RenderRequest`/`RenderPlan`) · `Extractors/`
(Generic + Specific — reform in place) · `Rendering/` · `Insights/` · `Analysis/` (parse-once cache) ·
`Resolvers/` · `Compression/`. Full module-by-module map with sizes and a "where do I change X?" index:
**`docs/dev/CODE-MAP.md`**.

## CLI reference (`devcontext analyze`)

```
devcontext analyze [PATH] [OPTIONS]
```

`PATH` accepts a `.sln`, `.csproj`, a folder, or `Type:Method` notation. Prefer an **absolute** local
path — a relative path that doesn't exist on disk is tried as a GitHub `owner/repo` shorthand and
triggers a clone; an existing local path always wins (use `--repo` for an explicit GitHub URL).

| Option | Meaning |
|--------|---------|
| `-f, --focus <F>` | Focus point (repeatable): `TypeName` \| `TypeName:MethodName` \| `GET /route`. Presence switches Map → Trace. |
| `--depth <1-10>` | Graph depth from the focus point |
| `--detail <level>` | Trace detail: `signature` \| `salient` \| `full` (default `salient`) |
| `--include-map` | When tracing, also render the Map/architecture sections alongside the trace |
| `--format <fmt>` | `markdown` \| `json` |
| `-o, --output <file>` | Write rendered content to a file (stdout still carries the explanation + stats line) |
| `--stats` | Show the full RunReport (timing, funnel, cache, corpus, parallelism, graph) |
| `--strict` | Exit code 2 on any self-check invariant violation (CI gate) |
| `--include-diagnostics` | Include diagnostics in output |
| `--no-roslyn` | Disable the Roslyn deep tier (faster, deterministic; some deep/dispatch edges drop) |
| `--lite` | Skip the full graph build (source bodies + call graph) for speed; Map still renders, focus re-analyzes |
| `--fast` | Skip heavy extractors (call graph, anti-patterns, unconditional scanners) for max speed |
| `--no-cache` / `--cache-only` | Force fresh analysis / require a cached snapshot (CI reproducibility) |
| `--repo <url>` / `--ref <branch>` / `--keep` | Clone a GitHub repo, check out a ref, keep the clone |
| `--dry-run` | Plan only — no extraction |
| `--verbose` / `--trace` / `--quiet` | Info logging / debug logging (incl. Roslyn) / suppress success output |

> **Retired flags** (accepted as hidden no-op stubs for a grace period, then gone): `--around`,
> `--scenario`, `--profile`, `--task`, `--max-tokens`, `--token-view`, `--include-provenance`,
> `--include-anti-patterns`, `--metrics`, `--cleanup`. The token budget and scenario/profile model
> were removed — use `--focus` and `--detail`. Don't reference these in new docs or scripts.

Other CLI commands: `init` (config scaffold), `query` (graph queries — `--focus`, `--direction`,
`--attach`), `report`, `scenarios`, `version`.

## MCP tools (`DevContext.Mcp` — server name `devcontext`)

22 tools over the gRPC RPCs (`DevContextTools.cs`; public catalog with setup snippets:
`docs/product/mcp-reference.md`):

```
Analyze · Overview · Resolve · Status · CloseSession · ListSessions · Stats · Entrypoints ·
Map · TopFlows · Trace · Node · Neighbors · Usages · Find · Impact · Seam · Config ·
TestsFor · GetContext · VerifyContext · ReadSource
```

The desktop MCP page manages the server (status, config snippets, sessions, live log feed, try-a-tool
sandbox). See `AGENTS.md` for background-process rules when running any server as an agent.

## Testing

| Project | Covers |
|---------|--------|
| `tests/DevContext.Core.Tests` | graph, map, trace, query, eval expectations, goldens, truth gates |
| `tests/DevContext.Server.Tests` | gRPC services |

Test categories drive the gate battery: `Category!=Eval` (fast), `Category=Eval` (real-repo
expectations), `Category=Truth` (truth gates — skips are the pending ratchet). Goldens in
`tests/goldens/`; regenerate with `$env:UPDATE_GOLDENS=1` and **review the diff**. Desktop app tests
run under Vitest (`pnpm test`).

## Branch & release

`develop` is the integration branch (feature branches PR here); `main` is always deployable. MinVer
with a `v` tag prefix drives the release workflow (`.github/workflows/release.yml`: build + test +
pack the CLI on Windows → NuGet + GitHub Release; no desktop artifact until server-sidecar bundling
lands — see `docs/dev/archive/trackers/GITHUB-READY-START.md`). See `docs/dev/DEVELOPER-PIPELINE.md` §11.

## See also

- `docs/dev/DEVELOPER-PIPELINE.md` — build, test, gate battery, run, bench, eval, screenshots.
- `docs/dev/briefs/loom-graph-design.md` — graph-model design authority (identity laws, prohibitions).
- `docs/dev/HANDOVER-LOOM.md` — engine close-out: architecture, benchmarks, known gaps.
- `docs/product/TRACE-ENGINE-DESIGN.md` — trace traversal design.
- `proto/devcontext/v1/devcontext.proto` — the gRPC contract.
