# Handover — Desktop Redo (Tauri + Angular + gRPC) — Phases 0–4

> Written 2026-06-29. For a fresh agent picking up the cross-platform desktop rewrite of DevContext.
> Branch: **`feat/desktop-redo-tauri-angular`** (off `develop`). Nothing committed yet — all work is
> in the working tree.
>
> **Resume instruction:** read this whole file, run the build/validate commands in §8 to confirm green,
> then continue at the **Resume point (§6)**. Work in the parallel waves/streams in §5. Keep every
> change green against the gates in §9.

---

## 1. Mission & product vision

Rebuild the DevContext desktop as a **world-class, cross-platform, lightweight "_.NET repo lens_"** —
"the first thing you run on any .NET repo." Point it at a folder / `.sln` / GitHub URL → orient (Map),
trace any flow down the wiring (Trace), browse the graph, pull stats, export LLM-ready context.

**Design law (decided by the user):** do **not** clone the WPF app's layout. Use the WPF app, the CLI,
the engine data, and existing features as *raw material* to craft a genuinely new product. Keyboard-first,
navigable, scannable, **vibe-skinnable** (must not look like every other app). The WPF app
(`src/DevContext.Desktop`) **stays untouched** — the new app is additive.

The engine (`DevContext.Core`) and CLI (`DevContext.Cli`) are **reused unchanged**. Do not modify them.

---

## 2. Locked decisions (do not relitigate)

| Area | Decision |
|---|---|
| Shell | **Tauri v2** (OS WebView, sidecar-spawns the .NET server in packaged builds) |
| Frontend | **Angular 22** — standalone, **signals**, **zoneless** (no zone.js) |
| Styling | **Tailwind CSS v4** (CSS-first) + **in-house components on Angular CDK** (NOT Spartan) |
| Icons | **`lucide`** core (NOT `lucide-angular` — deprecated) |
| State | plain Angular **signal stores** (no NgRx) |
| Graph | **Cytoscape.js** (+ `cytoscape-dagre` for the trace tree; elk reserved for Map topology) |
| Contract | **Protobuf via buf** → C# (`Grpc.Tools` in `DevContext.Contracts`) + TS (`@bufbuild/protoc-gen-es`) |
| Comms | **ConnectRPC `@connectrpc/connect-web`** (gRPC-Web) ↔ **ASP.NET Core gRPC + `UseGrpcWeb()`**; server-streaming for progress |
| Pkg mgr | **pnpm** · Node **24+** (Angular 22 requires it) |
| Trace UX | **dual view** — interactive graph ⇄ synced narrative story (cross-highlight) |
| Vibes | token-first runtime-swappable skins; seed **Modern + Terminal/CLI + Hacker/Cyber** (+ light/high-contrast) |
| Parallel dev | **mock `DEVCONTEXT_CLIENT` + captured fixtures (`pnpm start:mock`) + `/gallery` route** |
| Sequence | **P0 → P1 graph → P2 server surface → P3 product UX → P4 workbench+vibes → P5 ship → P6 MCP** |

---

## 3. What exists today (validated)

### New .NET projects (in `DevContext.slnx`)
- **`proto/devcontext/v1/devcontext.proto`** — the contract. **Just expanded** to the full P0+P2 surface
  (Analyze stream, CloseSession, ListEntryPoints, GetMap[structured], GetTrace, GetNode, GetNeighbors,
  **SearchNodes, GetStats, Render**, Ping). `proto/buf.yaml` (lint config). `csharp_namespace = DevContext.Protos`.
- **`src/DevContext.Contracts`** — proto → C# (`GrpcServices="Both"`). Builds clean.
- **`src/DevContext.Server`** — ASP.NET Core gRPC + gRPC-Web. Clean DI:
  `Program.cs` (composition root) → `Endpoints/DevContextGrpcService` (thin transport) →
  `Sessions/` (`IEngineRunner`→`EngineRunner`, `IAnalysisSessionManager`→`AnalysisSessionManager`,
  `AnalysisSession`, `StreamingProgressObserver`, `AnalysisContracts`) → `Mapping/ProtoMapper`.
  Reuses the CLI's `AddDevContextServices`. Health at `GET /health`. Default URL `http://127.0.0.1:5179`.
- **`tests/DevContext.Server.Tests`** — `AnalyzeFlowTests` (3 tests, green): Analyze(stream)→Map→Entries→Trace→Node, Ping, unknown-handle NotFound.

### New desktop app: `src/DevContext.App` (Angular 22 + Tauri)
File map (all under `src/app/` unless noted):
```
core/config.ts                    server base URL (window.__DEVCONTEXT_SERVER__ ?? http://127.0.0.1:5179)
core/grpc/client.ts               DEVCONTEXT_CLIENT root token (connect-web transport)
core/grpc/gen/devcontext/v1/...   GENERATED TS client (do not hand-edit; `pnpm gen:proto`)
data-access/devcontext-api.ts     typed wrapper over the client (analyze/getMap/listEntryPoints/getTrace/getNode/getNeighbors/ping)
state/session.store.ts            analysis lifecycle (status/progress/handle/summary/map/entries)
state/trace.store.ts              trace + node browse (focus/depth/detail/tree/selected/nodeDetail/neighbors)
state/connection.store.ts         Ping-based connection dot
models/view-models.ts (+spec)     proto -> view mappers (EntryVm, TraceNodeVm, NodeDetailVm, EdgeVm, groupEntries)
ui/icon/icon.ts (+spec)           lucide icon component
ui/graph-canvas/graph-canvas.ts   Cytoscape wrapper (CURRENTLY weak: breadthfirst + hardcoded colors)
features/source-bar | entries-panel | map-panel | node-detail | trace-panel | workspace
app.ts / app.config.ts            root (zoneless)
src/styles.css                    Tailwind v4 + @theme color tokens (NOT yet vibe-swappable)
src-tauri/                        Tauri shell (lib.rs has env-gated server sidecar spawn/kill); compiles
scripts/grpcweb-smoke.mts         live gRPC-Web smoke (Ping + streaming Analyze + Map/Entries/Trace)
buf.gen.yaml .postcssrc.json eslint.config.js AGENTS.md README.md
```

### Validated end-to-end
- Server integration tests **3/3**.
- **Live gRPC-Web smoke PASS** (the key risk): connect-web ↔ `UseGrpcWeb`, incl. **server-streaming** Analyze,
  Map, Entries, and a real trace `GET /api/Products → ProductsController.GetById → ProductService.GetByIdAsync`.
- App: builds, **4 unit tests** pass, eslint clean.
- **`dotnet build DevContext.slnx`: 0 warnings / 0 errors**. Tauri Rust shell `cargo build` succeeds.
- The First View runs in the browser (`pnpm dev:web`) and the user confirmed it feels light/responsive.
  Feedback: the **trace graph is illegible** (the #1 thing P1 fixes) and the colours are placeholder.

---

## 4. The current in-progress step (what I was mid-doing)

I had just **expanded the proto** (full P0+P2 surface) and **regenerated** C# + TS. **Everything still
builds green** (server 0/0, app OK). The new RPCs are NOT implemented yet (the gRPC base returns
UNIMPLEMENTED for them), `GetMap` still returns markdown-only, and none of the server hardening is done.

So the immediate next work is **Phase 0-A server hardening + Phase 2-A new RPCs** (see §6).

---

## 5. Parallel streams & waves

Sync only at the **proto** and the **fixtures**. After P0, streams run concurrently.

| Stream | Owns | Status |
|---|---|---|
| **A — Backend/contract** | server hardening (P0) → full query surface (P2) | proto expanded; impl next |
| **B — UI foundation & vibe** | vibe engine, `ui/` primitives, gallery, mock/fixtures (P0) → vibes (P4) | not started |
| **C — Trace graph** | dual-view graph (P1) | not started |
| **D — Features/UX** | product UX (P3) | not started |

**P0 waves (parallel where noted):**
1. A: proto adds (done) + server hardening · B: vibe tokens + ThemeService · F: capture fixtures from the running server.
2. B: `ui/` primitives (needs tokens) · B: mock client (needs fixtures).
3. B: refactor features onto primitives + graph de-hardcode · B: `/gallery` + router + `start:mock`.
4. Gates: prettier/stylelint/buf in `pnpm check`; `gate.ps1`; Playwright mock smoke; CI.

---

## 6. RESUME POINT — do these next, in order

### 6A. Server hardening (Stream A) — `src/DevContext.Server`
1. **`EngineHostCache`** (new singleton) — warm reuse. Owns a per-`rootPath`
   `EngineHost { ServiceProvider, DiscoveryPipeline, PersistentAnalysisCache }`
   (`PersistentAnalysisCache` is `DevContext.Core.Analysis`, ctor `(IFileSystem)`). `Get(root)` creates once
   and caches (re-analyzing a repo reuses the parsed-file cache via mtime). Dispose all on app shutdown
   (`IAsyncDisposable`). Register as singleton in `Program.cs`.
2. **Re-own `EngineRunner`/`AnalysisSession`:** `EngineRunner` gets its pipeline+cache from `EngineHostCache`
   (not a fresh `ServiceCollection` per Analyze). `AnalysisSession` holds the snapshot + the `EngineHost`
   (for `RenderAsync`) + `gitClonePath`. `AnalysisSession.DisposeAsync` cleans the clone only (the host is
   shared/cached — do NOT dispose the SP per session).
3. **Session eviction:** `AnalysisSessionManager` → LRU + idle-TTL with a cap (default 5) from `ServerOptions`;
   evict → dispose session (clone cleanup).
4. **`CloseSession` RPC** — remove + dispose the session; return `{closed}`. Idempotent.
5. **`ServerOptions`** (urls, session cap, idle timeout) bound from config in `Program.cs`.
6. **Central error mapping** — one helper exc→`RpcException` for the unary RPCs.
7. **Clone progress:** in `EngineRunner.PrepareSourceAsync`, the `null` passed to `git.CloneAsync(...)` is an
   **`IProgress<CloneProgress>?`** (`CloneProgress(Phase, PercentComplete, Message)`, `DevContext.Core.Services`).
   Wire it to emit `ProgressEvent` over the Analyze stream. Honor `AnalyzeRequest.cleanup` (auto|keep) on dispose.
8. **Tests:** eviction, CloseSession, warm reuse, error mapping.

### 6B. New RPCs (Stream A, P2) — implement in `DevContextGrpcService` + `ProtoMapper`
Use `session.Query` (`GraphQuery`) and `session.Snapshot`:
- **`SearchNodes(handle, query, limit)`** → scan `Query.Graph.Nodes` (title/FQN contains, prefer Type/EntryPoint,
  cap at limit) → `repeated NodeRef{node_id,title,kind,tags}`. (Reuse `GraphQuery.ResolveNodeId` heuristics for ranking.)
- **`GetStats(handle)`** → from `session.Snapshot.Report` (`RunReport`, `DevContext.Core.Models`) + `Query.Stats()`
  (`(Seams, EntriesWithTarget)`) + `Snapshot.Graph.NodeCount/EdgeCount` + `Snapshot.Entries.Length`. Map
  Stages/Extractors/Cache/Corpus/Funnel/Seams/Graph/TotalWall (see RunReport shape in §7).
- **`Render(handle, focus?, depth?, detail?, format, sections[], include_diagnostics)`** → build engine
  `RenderRequest` (`DevContext.Core.Pipeline`) → `Host.Pipeline.RenderAsync(snapshot, req)` → return
  `RenderResponse{content, format, estimated_tokens, sections[]}` (from `RenderedContext.Content/EstimatedTokens/Sections`).
- **Structured `GetMap`** → map `Snapshot.Map` (`MapModel`) facets (topology/packages/aggregates/pipeline/archetype/
  is_library/surface) into the new `MapResponse` (plus existing markdown). See MapModel shape in §7.
- Re-capture fixtures after; add an integration test per RPC. `buf breaking` (once a baseline is committed) must stay additive.

### 6C. App Phase 0 (Stream B)
1. **Vibe engine:** rework `src/styles.css` to `@theme inline { --color-*: var(--vibe-*) ... }` + `[data-vibe][data-theme]`
   blocks; broaden tokens beyond color (type, radii, borders, density, effects, motion). Add
   `core/theme/theme.service.ts` (signal vibe+mode, sets `data-vibe`/`data-theme` on `<html>`, persists) + a
   `vibes/` registry (`VibeDefinition`); seed `modern` (port current dark) + `modern-light` + minimal `terminal`.
2. **`ui/` primitives** (Tailwind + CDK): Button, Segmented, Badge, Panel(+title slot), Tabs, Field/TextInput,
   KbdHint, Spinner, ToastHost, **Card, SearchField, Sheet, Drawer, CommandPalette** (anticipate P3). Refactor the
   existing features onto them. **De-hardcode `GraphCanvas` colors** → read from `ThemeService.palette()`
   (computed CSS vars; recompute on vibe change).
3. **Parallel-dev rig:** `scripts/capture-fixtures.mts` (drive the live server like `grpcweb-smoke.mts`, dump JSON to
   `src/app/testing/fixtures/`); a **mock `DEVCONTEXT_CLIENT`** (`provideMockClient()`) returning fixtures (simulate
   the Analyze progress stream); an Angular **`mock` build config** → `pnpm start:mock`; add **`provideRouter`** with
   `''`→Workspace and `gallery`→Gallery; build the **`/gallery`** page (all primitives + features × vibes + switcher).

### 6D. Then P1 → P3 → P4 (see §10 for full scope)
- **P1 (graph, Stream C):** dagre L→R, role-coloured labelled nodes + kind icons, seam-labelled edges, legend,
  fit/zoom, path-highlight on select, collapse/expand; **story panel rendered from `trace.tree()`** (seam · title ·
  file:line · salient) with **graph⇄story cross-highlight**; render RESULT/TOUCHES/EMITS/NEXT summaries.
- **P3 (product UX, Stream D):** launcher/start page · structured Overview (Map) page · **command palette + universal
  search as primary nav** · node inspector/browse (neighbors both ways, find-usages) · **LLM-export sheet**
  (budget + section toggles) · **Stats drawer** · recents (localStorage) · copy/save (Tauri save dialog or download) ·
  Tauri folder/.sln picker (`@tauri-apps/plugin-dialog`) · advanced options.
- **P4 (Stream B):** Terminal + Hacker/Cyber vibe content + switcher + effects (scanlines/glow/glitch, respect
  `prefers-reduced-motion`); resizable workbench panels (CDK); shortcuts; light/high-contrast; a11y (axe).

---

## 7. Engine API reference (what the server consumes)

**Canonical analyze recipe** (already in `EngineRunner`; mirror of `DevContext.Cli` AnalyzeCommand + WPF `AnalysisService`):
`ProjectRootResolver.ResolveAsync(path, fs, ct)` → `AnalysisIntentResolver.Resolve(new IntentInput{Focus,Depth})`
→ build `ExtractionOptions{ EntryPaths=root.EntryCandidates, Profile, AllowRoslyn, BuildFullGraph=true, OutputFormat=Markdown,
ExcludePatterns, ExcludeExtractors=scenario.DisableExtractors }` → `SharedAnalysisContext{ FocusPoints, UnresolvedFocusPoints }`
→ DI via `services.AddLogging(); services.AddDevContextServices(root)` → `DiscoveryContext{ RootPath=root.EffectiveRootPath,
ScopedProjectDirs, Options, ActiveScenario, Observer, FileSystem, Cache, Analysis, Logger, CancellationToken }`
→ `pipeline.AnalyzeAsync(ctx, ct)` → `AnalysisSnapshot`.

**Namespaces:** `ProjectRootResolver`→`DevContext.Core.Resolvers`; `ProjectRootResult`,`RepoUrl`,`ExtractionOptions`,
`OutputFormat`,`SharedAnalysisContext`,`DiscoveryContext`,`Scenario`,`RunReport`→`DevContext.Core.Models`;
`RepoStatus`,`GitCloneService`,`CloneProgress`→`DevContext.Core.Services`; `IntentInput`,`AnalysisIntentResolver`→`DevContext.Core.Configuration`;
`AnalysisCache`,`PersistentAnalysisCache`→`DevContext.Core.Analysis`; `DiscoveryPipeline`,`AnalysisSnapshot`,`RenderRequest`,`TraceDetail`,`RenderedContext`→`DevContext.Core.Pipeline`;
`GraphQuery`,`NodeId`,`NodeKind`,`EntryPoint`,`EntryPointKind`,`Trace`,`TraceStep`,`SeamKind`,`MapModel`,`ProjectNode`,`PackageGroup`,`LibrarySurface`,`Archetype`,`NodeDetail`,`EdgeRef`,`EdgeDirection`→`DevContext.Core.Graph`;
`AddDevContextServices`→`DevContext.Cli.Services`; version → `DevContext.Core.DevContextVersion.Display`.

**`GraphQuery`** (`new GraphQuery(snapshot.Graph!, snapshot.Entries, snapshot.Map)`):
`EntryPoints(kind?)` · `Map()` · `Stats() -> (ImmutableArray<SeamStat> Seams, int EntriesWithTarget)` ·
`Trace(focus, depth=6, maxFanOut=12) -> Trace?` · `Node(NodeId) -> NodeDetail?` ·
`Neighbors(NodeId, EdgeDirection, EdgeKind?) -> EdgeRef[]` · `FindUsages(NodeId)` · `ResolveNodeId(string) -> NodeId?` · `Graph`.

**Key shapes:**
- `RunReport { Stages[StageStat{Stage,Elapsed,Ordinal}], Extractors[ExtractorStat{Name,Tier,Category,Stage,Elapsed,TypesAdded,DetectionsAdded,Skipped,SkipReason}], Scorers, Compressions, Cache{TextHits,TextMisses,SyntaxTreeHits,SyntaxTreeMisses}, Corpus{TotalFiles,CSharpFiles,Projects}, Funnel{TypesDiscovered,TypesHardExcluded,TypesIncluded,RawEstimatedTokens,RenderedEstimatedTokens,Budget}, Parallelism, TotalWall }`
- `MapModel { Style, StyleConfidence, StyleEvidence, Entries[], Topology[ProjectNode{Name,DependsOn[]}], Packages[PackageGroup{Label,Packages[]}], Aggregates[], PipelineBehaviors[], Archetype, Surface?(LibrarySurface{Groups[SurfaceGroup{Namespace,Types[SurfaceType{Name,Kind,Members[]}]}], ExtensionPoints[]}), ScopeNote }`
- `NodeId(NodeKind Kind, string Key)` — `ToString()="{Kind}:{Key}"`; `NodeKind{Type,Member,EntryPoint}`.
- `TraceStep(GraphNode Node, SeamKind Seam, int Depth){ Provenance?, Resolution, Children[], Truncated, Omitted, Pipeline[], Salient[] }`; `Trace(EntryPoint Entry, TraceStep Root){ TouchedEntities[], EmittedEvents[] }`.
- `EntryPoint(Kind,Title,NodeId Node){ HttpMethod?,Route?,Provenance?,Project?,Target?,HandlerNode? }`; `EntryPointKind{HttpEndpoint,MessageConsumer,HostedService,ScheduledJob,DomainEventHandler,PublicApi}`.

---

## 8. Run / build / test / validate

```powershell
# Prereqs: nvm use 24.15.0 ; corepack enable ; .NET 10 SDK ; (Tauri) VS Build Tools + WebView2 — all present here.

# --- .NET ---
dotnet build DevContext.slnx -clp:ErrorsOnly        # gate: 0 warnings (analyzers-as-errors)
dotnet test tests/DevContext.Server.Tests           # server gRPC integration tests
dotnet test DevContext.slnx                          # all (Core + Desktop + Server)

# --- App (run from src/DevContext.App) ---
pnpm install
pnpm gen:proto                                       # regenerate TS client after editing the .proto
pnpm check                                           # lint + test + build  (add prettier/stylelint/buf in P0)
pnpm dev:web                                         # server + ng serve -> http://localhost:4200 (quickest to eyeball)
pnpm dev                                             # server + tauri dev (native window)

# --- Live gRPC-Web smoke (proves the full path incl. streaming) ---
# terminal 1: pnpm server      terminal 2:
node --experimental-strip-types scripts/grpcweb-smoke.mts
```

---

## 9. Gates (every phase must stay green; ratchet per phase)

- **Contract:** `buf lint` (+ `buf breaking` once a baseline is committed — keep additive).
- **.NET:** 0-warning `dotnet build DevContext.slnx`; Core/Desktop/**Server** tests; existing eval ratchet + CLI strict matrix (`eval/gates.ps1`).
- **App:** eslint · prettier `--check` · stylelint · vitest (`ng test --watch=false`) · `ng build`.
- **E2E:** live gRPC-Web smoke + Playwright UI smoke (run against `pnpm start:mock` for determinism).
- **Budgets:** cold start ≤ 2s · trace re-render ≤ 100ms · app initial transfer ≤ ~300KB · warm re-analyze fast.
- **P0 exit:** `start:mock` runs the UI offline; `/gallery` renders primitives in ≥2 vibes with a runtime switch;
  server eviction/CloseSession/warm-reuse tests pass; unified `gate.ps1` + CI green.

---

## 10. Full phase scope (condensed) — see prior plan for detail
- **P0** Refactor & foundations: server hardening (§6A) · vibe engine + `ui/` primitives + mock/fixtures/gallery (§6C) · gates/CI.
- **P1** Legible trace dual-view (graph + story, cross-highlight, RESULT/TOUCHES/EMITS/NEXT).
- **P2** Full query surface (SearchNodes, GetStats, Render, structured GetMap, clone progress, CloseSession).
- **P3** Product UX (launcher · structured Overview · palette/search-first nav · inspector/browse · LLM-export sheet · Stats drawer · recents · copy/save · picker · advanced).
- **P4** Workbench & vibes (Terminal + Hacker vibes + switcher + effects · resizable panels · shortcuts · light/high-contrast · a11y).
- **P5 (later)** Package & ship (sidecar bundling self-contained .NET, NativeAOT probe, Win/macOS/Linux installers, perf budgets, cross-platform CI).
- **P6 (later)** MCP face + persistent index over the same gRPC/GraphQuery.

---

## 11. Gotchas learned the hard way (read before you build)
- **Node 24+ required** for Angular 22 (`nvm use 24.15.0`; nvm-windows is installed). pnpm 10 via corepack.
- **`lucide` core, NOT `lucide-angular`** (deprecated, caps at Angular 21). Don't add `@types/cytoscape` (deprecated — Cytoscape bundles its own).
- **proto3 `optional` fields THROW if assigned null** in generated C# — guard every nullable in `ProtoMapper` (`if (x is {} v) msg.Field = v;`). Requests expose `HasField` for optional presence.
- **Running the server under `concurrently`:** use the **literal `dotnet <dll>` command** (the raw host ignores stdin-EOF). `dotnet run` and `pnpm`-wrapped scripts exit 0 on stdin-EOF under `concurrently`, and `-k` then kills the UI. The `dev`/`dev:web` scripts therefore `pnpm build:server` first, then run the literal dll.
- **`RepoUrl.Parse` eats relative paths** (`owner/repo` shorthand → tries to clone) — always pass absolute local paths.
- **Server references `DevContext.Cli`** for `AddDevContextServices` — the canonical engine wiring; don't re-implement it.
- **`.NET` is warnings-as-errors + nullable + analyzers** (`Directory.Build.props`, LangVersion 13, net10.0). A clean build is the gate.
- **Tailwind v4 is CSS-first** (`src/styles.css`, `@import "tailwindcss"`, `@theme`, `.postcssrc.json`). For vibes, move to `@theme inline` + `var(--vibe-*)` + `[data-vibe]` so utilities re-bind at runtime (no rebuild). Keep components role/token-based — **never hardcode colors** (the Cytoscape graph currently does; fix in P0).
- **Tauri sidecar** is env-gated (`DEVCONTEXT_SERVER_DLL`) for packaged builds; dev uses `concurrently`. Tauri Rust builds need VS Build Tools (present).
- **gRPC-Web + CORS:** server has a permissive localhost CORS policy + `UseGrpcWeb(DefaultEnabled=true)`; the Node smoke proves the transport (CORS is browser-only, validated implicitly via the running app).

---

## 12. Files to know
- Plan/vision: this file + `docs/product/PRODUCT-DIRECTION.md`, `docs/product/IDEAL-OUTPUT-TARGET.md`, `docs/product/DESIGN-PHILOSOPHY.md` (D5/P6 say "WPF" — superseded by this redo; record an ADR when shipping).
- Engine handover (separate, still valid): `docs/dev/HANDOVER.md`.
- Skill: `.claude/skills/run-devcontext/SKILL.md` (CLI smoke driver, conventions).
