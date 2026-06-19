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

(Note `ad8bdb2` and earlier are the prior consistency branch — already on `fix/desktop-latent-bugs-and-rendering`, merged into this branch's history.)

## Remaining (in recommended order)

### G1 — multi-project / multi-solution scope · **Critical · FOUNDATIONAL — do not one-shot**
Pointing at a project analyzes only that project's closure. Symptoms:
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

### G3 — library archetype (AutoMapper) · **High · sizable**
No archetype detection → AutoMapper renders as `NLayer` with test projects, no PUBLIC SURFACE, no
entries. Design §4 wants a capability-grouped surface map. Needs: (a) archetype detection (no app
entries + packable library → Library), (b) a `LibrarySurface` builder over public types/methods, (c) a
surface renderer (reuse the `NarrativeSections` fragment pattern from this branch so it stays
section-aware in the desktop). New work — plan it.

### G8 — narrative stats line · **Low · trivial & safe — good next quick win**
Every Map/Trace footer says `… 0 types kept of 0 …` (type-funnel is meaningless for the graph
artifacts). Report **nodes / edges / entries / trace depth** instead. Source: the summary is built
from `snapshot.Report` / `RenderFunnel` in `RunReportFormatter.Summary`; the narrative `RenderAsync`
branch in `DiscoveryPipeline` returns no `RenderFunnel`. Either populate a graph-shaped funnel there or
special-case the summary for narrative mode.

### G5 — minimal-API per-endpoint precision · **Medium · hard**
All minimal-API endpoints in one registration method share the owner Type node, so trace body lines /
`→ target` don't match the specific route (e.g. TodoApi `POST /todos/` shows `MapGet("/{id}"…)` lines).
Needs per-endpoint anchoring (member/lambda-level nodes). Known-deferred; real work.

### G7 — signal consistency · folds into G1 (the MediatR-signal fix above).
### G9 — PACKAGES verbosity · **Low · cosmetic** (cap/group long lists).
### FastEndpoints `<dynamic>` routes · separate known gap — Configure()-set routes collapse to one
`GET <dynamic>` node (visible in VerticalSlice). G2 already suppresses misleading targets for these.

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
