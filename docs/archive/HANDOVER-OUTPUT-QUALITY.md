# Handover: Output-quality work (graph-based shift)

> Branch: `feat/output-quality-graph` (off `fix/desktop-latent-bugs-and-rendering`)
> Assessment that drives this work: `docs/reports/OUTPUT-QUALITY-ASSESSMENT.md` (read it first — it
> has the ranked gaps G1–G9 + bugs B1–B3, each with concrete evidence from the eval repos).
> Gate at handover: **build 0-warn · Core 245+ pass / 2 skip · Desktop 64 pass.** Working tree clean.

## What this branch is doing

Improving the **Map + Trace** output against `docs/IDEAL-OUTPUT-TARGET.md`, and fixing the bugs the
user flagged, while keeping **desktop and CLI consistent** (shared Core renderers + one `EntryPoint`
model feed both). Done in small, committed checkpoints.

## Done (committed)

| Commit | Gap | What |
|--------|-----|------|
| `ecb2ed5` | **G2** | `EntryPoint.Target` resolved from the graph in `GraphBuilder.Build` (feeds Map *and* desktop picker). `MapRenderer` lists ALL entries (no "…N more") as `route → Target (file:line)`, short filenames. Target is truthful — null when ambiguous (minimal-API owner with many sends) or `<dynamic>` route. |
| `6845d29` | entry UX | ConfigPanel `<datalist>` → custom searchable combobox (browse/filter/clear), shows `route → Target`, commits `VM.Focus` only on pick/Enter (no re-analyze per keystroke). |
| `dcb5439` | **B1** | GitHub clone no longer deleted after each run (that defeated `GitCloneService`'s 24h cache → re-clone on every option change). Now reused for the session, cleaned on Dispose when cleanup=="auto". Label → "Auto-clean on exit". |
| `f7f0fd7` | **G4+G6** | Trace dedups followable edges by `(target, kind)` (twin-node double-counted Raises) + `Distinct` summaries. Topology reduces `..\X.csproj` refs to names (also un-breaks the name-based scope filter) and drops test/benchmark projects via `ProjectClassifier`. |
| `a21d5e9` | **G1 (part)** | Parse `.slnx` solutions (new `SolutionFileParser`, used by `SolutionDiscoveryExtractor` + `RoslynWorkspaceProvider`; resolver globs `*.slnx`). Four eval repos are `.slnx`-only and previously resolved to an EMPTY solution (name `""`, 0 projects). Extractor now prefers the **root** solution over nested ones. Verified: eShop root → `eShop`/24 projects, `CleanArchitecture`+MediatR lit, constellation topology (was empty/`MinimalApi`/no-MediatR). **Root-pointing** now correct; **subfolder** pointing still analyses only that closure (rescope = remaining G1). |
| `3d8544c` | **G7** | `ArchitectureStyleDetector` keyed MediatR styles off the package *signal* (missed when a sub-project is scoped) and counted handlers from `MediatRHandlerDetection` — but it runs between Stage 2/3 while that detector is Stage 3, so the count was always 0. Now counts handlers from the types' implemented interfaces (available in Stage 2) and lights `hasMediatR` from them. Verified: eShop `Ordering.API` alone → `CleanArchitecture` (7 domain-event handlers; MediatR with 16) instead of `MinimalApi`/no-MediatR. |
| `4908228` | **G8** | Map/Trace stats line was `0 types kept of 0` (type funnel is meaningless for graph artifacts). `RenderedContext.GraphSummary` (nodes/edges/entries/depth) now drives a graph-shaped summary in CLI + desktop: `247 nodes · 7 edges · 0 entries · ~236 tokens` (Map) / `… depth 2 …` (Trace). |

(Note `ad8bdb2` and earlier are the prior consistency branch — already on `fix/desktop-latent-bugs-and-rendering`, merged into this branch's history.)

## Remaining (in recommended order)

### ~~G1 — multi-project / multi-solution scope~~ · **DONE (Hybrid policy)**
> **DONE** on `feat/polish-batch-and-g1-phase0`. `.slnx` groundwork `a21d5e9`; `SolutionScope.FromModel`
> fix `8c93c57`; **Hybrid closure** P1 `06df05a` · P2 `4c87981` · P3 `1a67291` · P4 `9fb7efb`. Project/
> subfolder input → anchor + transitive `ProjectReference` closure; `.sln`/root → whole-solution. Verified:
> `analyze eShop/src/Ordering.API` → `MAP eShop (7-project closure)`, CleanArchitecture, cross-project
> topology, trace crosses into `IntegrationEventLogEF`. See `docs/plans/PLAN-G1-multi-project-scope.md`.
> (Historical investigation notes below retained for context.)

Pointing at a project subfolder analyzes only that project's closure. Symptoms:
- eShop `Ordering.API` → Map `unknown (1 project)`, `STYLE MinimalApi`, *"no MediatR"* — yet its own
  Trace bridges MediatR send→handler. Data/domain-event seams never appear (Domain/Infra not scanned).
- VerticalSlice → `(1 project)` because the repo has **two** solutions (`Clean.Architecture.slnx` at
  root + a `MinimalClean/` tree); the resolver picks one, endpoints come from the other.

**Investigation notes (important):**
- The style bug is partly *separate from* rescoping. `ArchitectureStyleDetector` (`src/DevContext.Core/
  Extractors/Generic/ArchitectureStyleDetector.cs`) keys CleanArchitecture off `hasMediatR` = the
  **MediatR architecture *signal*** (`signals[Keys.MediatR].Detected`), NOT the handler count. eShop's
  `MediatRHandlerDetection`s ARE found (trace works) but the *signal* isn't lit, so it falls through to
  `MinimalApi`. **Cheap surgical win:** light the MediatR signal when notification/command handlers are
  detected (or have the style detector also consider `mediatRHandlerCount`/`notificationHandlerCount`,
  which it already computes but only uses inside the `if (hasMediatR)` branch). Verify against
  `eval/expectations/*.json` + `EvalExpectationTests` — some expectations may assume the current
  (wrong) style and need ratcheting.
- True rescoping (analyze the whole `.sln`/closure, or project + referenced projects) touches
  `ProjectRootResolver`, `SolutionScope` (`src/DevContext.Core/Graph/SolutionScope.cs`), discovery
  extractors, and has **perf** (eShop = 24 projects) and **eval-expectation** consequences. The design
  doc itself flags this as a foundational fork. Get the user's nod on the tradeoffs and give it its own
  plan; don't bundle it into a polish pass.

### ~~G3 — library archetype (AutoMapper)~~ · **DONE (`d667478`)**
ArchetypeDetector (App/Library) + LibrarySurfaceBuilder + LibrarySurfaceRenderer; AutoMapper → PUBLIC
SURFACE over 230 public types. See `docs/plans/PLAN-G3-library-and-G5-minimal-api.md`.

### ~~G8 — narrative stats line~~ · **DONE (`4908228`)** — graph-shaped summary in CLI + desktop.

### ~~G5 — minimal-API per-endpoint precision~~ · **DONE (`0c75d73`)**
Per-endpoint lambda nodes carry each route's body; Map shows `route → Command` per route. Residual
(partial by design): downstream call-body salience. See `docs/plans/PLAN-G3-library-and-G5-minimal-api.md`.

### ~~G7 — signal consistency~~ · **DONE (`3d8544c` + residual `ce64a0f`)** — MediatR style reads
handler types, not just the package signal. Residual fixed: `ArchitectureStyleDetector.HasMediatREvidence`
is now the single source of truth and the STACK line uses it, so a scoped sub-project's STACK no longer
disagrees with STYLE.
### ~~G9 — PACKAGES verbosity~~ · **DONE (`ce64a0f`)** — capped at 8/group with `… (N total)` overflow.
### ~~FastEndpoints `<dynamic>` routes~~ · **triaged, no change** — `GraphBuilder` nulls the target and
`OutputSelfCheck` guards the literal; G2 already suppresses misleading targets. Real per-endpoint
resolution stays with G5.

## How to verify / reproduce

```pwsh
# Build + the relevant suites
dotnet build DevContext.slnx
dotnet test tests/DevContext.Core.Tests        # graph/map/trace + eval (eval-repos/ present)
dotnet test tests/DevContext.Desktop.Tests

# Re-run the assessment matrix (writes UTF-16 .txt to %TEMP%\dc-assess)
$cli = "src/DevContext.Cli/bin/Debug/net10.0/DevContext.Cli.dll"
dotnet $cli analyze "C:\code\DevContext2\eval-repos\eShop\src\Ordering.API"                       # Map
dotnet $cli analyze "C:\code\DevContext2\eval-repos\eShop\src\Ordering.API" --focus "POST /api/orders/"  # Trace
```

**Gotchas:**
- CLI `RepoUrl.Parse` treats `eval-repos/TodoApi` as a GitHub `owner/repo` shorthand — use an
  **absolute path** for local eval runs.
- PowerShell `*>`/redirect writes **UTF-16**; decode with `iconv -f UTF-16 -t UTF-8` or read via the
  Read tool. Piping a native exe to `Select-Object -First N` truncates the pipe and reports a non-zero
  exit even though analysis succeeded.
- After editing Core, **rebuild the CLI project** (its `bin` has its own copy of `DevContext.Core.dll`)
  before running the CLI dll, or you'll test stale output.

## Key files touched on this branch
- Core: `Graph/EntryPoint.cs`, `Graph/GraphBuilder.cs` (target resolution), `Graph/TraceBuilder.cs`
  (edge dedup), `Graph/MapBuilder.cs` (topology), `Rendering/MapRenderer.cs` (entry rendering).
- Desktop: `Components/ConfigPanel.razor` (combobox), `ViewModels/MainViewModel.cs` (session clones),
  `wwwroot/app.css`.
- Tests: `NarrativeRendererTests.cs`, `TraceBuilderTests.cs`.
- Docs: `docs/reports/OUTPUT-QUALITY-ASSESSMENT.md`, `docs/cli-reference.md`, `docs/desktop-ui.md`.

## Consistency rule (keep holding it)
Every output change goes through the **Core** renderers / one `EntryPoint` model so the **CLI Map** and
the **desktop picker/views** stay identical. Desktop-only changes (combobox) must mirror what the CLI
shows (it shows the same `→ Target`). When in doubt, add the data to Core, not the surface.
