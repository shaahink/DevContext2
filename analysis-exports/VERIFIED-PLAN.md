# DevContext — Verified Findings & Self-Checkable Remediation Plan

> Second-pass review of the DeepSeek cross-repo analysis (`GAPS-AND-ISSUES.md` +
> `ACTION-PLAN-FOR-AGENT.md`). Every claim below was checked against the **current code**
> (`feat/desktop-v2` @ `ff143d8`) and the actual export artifacts, not the report's prose.
> Where the report's *symptom* is real but its *root cause / locus* is wrong, that is called out —
> several of the headline fixes it proposes would change nothing, and one would **break a
> deliberately test-locked invariant**.
>
> Loci use real, verified `file:line`. The report's loci were partly inferred from design docs and
> are wrong in places (e.g. it cites `Scoring/TokenBudgetEnforcer.cs`; the file is at
> `Pruning/TokenBudgetEnforcer.cs`, and that class does **not** drive the Map at all).

---

## 0. What DevContext is (grounding)

A .NET static-analysis CLI that emits LLM-ready **context** for a solution. After the 2026-06 *catalog→trace*
pivot the product is two narrative artifacts rendered from an immutable `CodeGraph`:

- **Map** — orientation: stack, topology, **entry-point inventory**, packages (no focus).
- **Trace** — one entry walked *down the wiring*, indirection bridged (MediatR send→handler, raise→handler,
  DI interface→impl, EF data access), depth/fan-out bounded.

The graph is assembled in `DiscoveryPipeline.AnalyzeAsync` → `GraphBuilder.Build` → `MapBuilder.Build`,
then `RenderAsync` lenses it to Map or Trace markdown. **A legacy "catalog" path (flat type/detection tables)
still exists and backs `--format json|html`.** This split is the source of most of the report's confusion.

---

## 1. The central correction — the token-budget story is wrong

The report's C1/H2/H4 and its entire "Token Budget Analysis" + "scorer funnel is a no-op" sections rest on
one false premise: that `TokenBudgetEnforcer` governs the Map/Trace output. **It does not.**

Verified in `Pipeline/DiscoveryPipeline.cs`:

- **Lines 117–125 & 596–599** — an explicit invariant comment: the CodeGraph + Map/Trace are assembled
  *before* `RunScoringAsync`/`RunCompressionAsync`, and "never read `model.Budget`, `IsPruned`, or
  `RoleScore` — the token budget and the legacy pruners drive ONLY the legacy catalog RenderPlan (JSON/HTML)."
- **Lines 319–376** — `RenderAsync`: when `format == markdown` and the graph is non-empty, output comes from
  `MapRenderer` / `TraceRenderer` / `LibrarySurfaceRenderer`. The `TokenBudgetEnforcer`, `RenderPlanBuilder`,
  and `model.Types`/`IsPruned` are **bypassed entirely**.
- **`tests/DevContext.Core.Tests/BudgetIndependenceTests.cs`** — *locks* this: "the narrative (markdown) Map
  and Trace must be byte-identical across different `--max-tokens`." Re-coupling the budget to the Map (the
  report's proposed fix) fails this test by design.

So:
- The "99% cut" funnel (`247→25`, `13884→20`, …) is the **legacy catalog** pruning. The markdown Map the
  report measured ignores it. The numbers are real and irrelevant to the artifact.
- aspnetcore's 21,071-token Map is **not** "the enforcer leaving headroom unused." It is the narrative
  renderers having **no truncation at all** (see W4) plus test-entry noise (W1).
- Ocelot's "3 entries" is **not** a budget cut (the budget can't touch this path). It is mostly *correct*
  (see W2/W6).

**Consequence for the agent:** do **not** "fix `TokenBudgetEnforcer`." Add a render-time cap to the
*narrative renderers* (W4), which keeps the budget out of the kernel and out of graph assembly, satisfying
`BudgetIndependenceTests` (the cap is on *rendered* sections, deterministic, not `--max-tokens`-driven).

---

## 2. Verification verdict on all 11 reported issues

| ID | Report's claim | Verdict | Corrected root cause / real locus |
|----|----------------|---------|-----------------------------------|
| **C1** | TokenBudgetEnforcer broken; cuts 99%; Map over/under budget | **Misdiagnosed** | Enforcer governs the dead legacy catalog path only. Map/Trace is budget-independent **by design** (`DiscoveryPipeline.cs:117-125,596-599`; locked by `BudgetIndependenceTests`). Real issue is W4 (no render-time cap). |
| **C2** | Noise filter leaks test entries (`testassets/`, `stress/`, `Testing/src/`) | **CONFIRMED** | `NoiseFilter.IsProductionEntrySource` (`NoiseFilter.cs:111-114`) only does project-dir + sample-path checks. `IsTestPath` (`/test/`,`/tests/`) **exists at `NoiseFilter.cs:57-63` but is deliberately *not* wired into the app filter.** `stress`/`Testing`/template-`content` have no pattern. → **W1** |
| **C3** | Library traces empty | **CONFIRMED** | `TraceBuilder.OutEdgesWithTwin` (`TraceBuilder.cs:330-339`) bridges, from a Type node, only **handler-entry members** (`Handle/Execute/Invoke`, `BuildBridgeMemberIndex` 80-106). `Calls` edges are member-origin, so a library Type entry (`Log`, `LoggerConfiguration`, `MainWindow`) exposes no followable edge. → **W3** |
| **H1** | Desktop apps misclassified as Library | **CONFIRMED (+ extra mechanism)** | Two causes: (a) no desktop entry kinds — `AppEntryKinds` is HTTP/Bus/Hosted/Scheduled only (`ArchetypeDetector.cs:14-18`); (b) the `allExeAreAuxiliary` heuristic (`ArchetypeDetector.cs:46-50`) treats `Files.App` (`OutputType=WinExe`, references lib projects) as a *library sample* → Library. → **W5** |
| **H2** | Map entry starvation from budget | **Refuted** | Budget can't touch the narrative path. Ocelot core genuinely has **3 production HTTP controller actions** (`src/Administration/*Controller.cs`); sample downstream controllers are *correctly* dropped by `IsSamplePath`. Latent real gaps surfaced instead: **W6** (solution pick) + **W7** (gateway archetype). |
| **H3** | `--focus` on a library renders the Map, not a trace | **Recast (= C3 family)** | Not a render-path override. `GraphQuery.Trace` returns null only when `EntryPointResolver.Resolve` finds no node (`GraphQuery.cs:64-68`); then `RenderAsync` falls through to Map. `--focus IAppService` resolves an interface node with no out-edges → empty/again Map. Fixed by **W3**; **W3b** makes the null-trace fallback honest. |
| **H4** | aspnetcore Map unreviewably large (934 lines / 21k tok) | **CONFIRMED, re-rooted** | Real. Cause = `MapRenderer.AppendEntryPoints` lists **ALL** entries ("no '... and N more'", `MapRenderer.cs:146-163`) and `AppendTopology` lists **all 395 projects** uncapped (`126-144`), with no render-time budget — *plus* W1 inflating the entry list. → **W1 + W4** |
| **M1** | MSBuild var token leak (`net472`) | **Non-issue (report self-reclassified LOW)** | Confirmed: `MapRenderer.AppendStack` already drops `$(` TFMs (`MapRenderer.cs:68`); `net472` is a real multi-target TFM. No action. |
| **M2** | Ocelot `GET /configuration` has no target | **CONFIRMED (low)** | `ResolveEntryTarget`→`ResolvePrimaryCall` (`GraphBuilder.cs:80-148`) returns null when the action has no `Service`-tagged or in-scope non-framework callee. → **W8** |
| **M3** | Serilog DI edges missing | **CONFIRMED but low value** | `DiRegistrationExtractor` keys on `IServiceCollection.Add*`; Serilog wiring is its own fluent builder + `UseSerilog()` host extension. Surfacing it adds little to a *library* map. Defer. |
| **M4** | Files call graph unused | **CONFIRMED = duplicate of C3/H1** | 165 `Calls` edges exist on `MainWindow` members; the Type entry doesn't bridge them. Same root as W3; archetype is W5. |
| **L1** | RESULT inference too aggressive | **Plausible, low** | Convention-based status inference; cosmetic. Defer. |
| **L2** | Framework boundary weak (test DbContext in trace) | **CONFIRMED = C2 family** | `TraceBuilder` doesn't apply test-path exclusion to traversed/`TOUCHES` nodes. Folds into **W1** (share the predicate) + a trace-side filter. |

**Scorecard:** 6 confirmed (C2, C3, H1, H4, M2, L2), 1 confirmed-but-low (M3), 2 misdiagnosed/refuted (C1, H2),
1 recast (H3), 1 non-issue (M1), 2 low/defer (L1, and M3). The report's *observations* are largely sound; its
*root-cause attribution to the token budget is the main error*, and it inferred several loci from docs.

---

## 3. Gaps the report missed (engine-level, the "improve big time" asks)

- **G-A / W6 — arbitrary solution selection.** `SolutionDiscoveryExtractor` (`SolutionDiscoveryExtractor.cs:49-54`)
  picks the root-closest solution, tie-breaking same-depth roots by **enumeration order**. Ocelot ships both
  `Ocelot.slnx` and `Ocelot.Samples.slnx` at root; it picked **Samples**, so the whole Map is scoped to the
  sample aggregator and core Ocelot is just a referenced node. No heuristic prefers the *product* solution.
- **G-B / W7 — no reverse-proxy / API-gateway archetype.** Ocelot's real surface is dynamic, config-driven
  routes dispatched by middleware (`OcelotMiddleware`), not MVC/Minimal-API endpoints. The HTTP-entry model
  structurally cannot represent a gateway, so its Map looks empty no matter what. YARP has the same shape.
- **G-C — the legacy catalog + its budget/pruner machinery is dead weight on the primary path.** `--stats`
  surfaces a pruning funnel that describes a path the user never sees — which is exactly what misled the
  report. Either retire the catalog or stop presenting its funnel as if it explains the Map.
- **G-D — template scaffolding leaks as entries.** aspnetcore surfaces `src/ProjectTemplates/.../content/*.cs`
  routes (scaffolding stamped into *new* projects, not runtime code). Distinct from test/sample noise. Folds into W1.

---

## 4. Prioritized, self-verifiable work items

Each item has a **locus**, a **fix sketch**, and a **VERIFY** block that is machine-checkable (a unit test,
an eval-expectation check, or a CLI command with an observable). Repos are in
`analysis-repos/{serilog,ocelot,files,aspnetcore}`. Add the 4 to `eval-repos.json` + `eval/expectations/`
first so VERIFY blocks can run under `eval/gates.ps1`.

Run order is by leverage: **W1 → W3 → W5 → W4 → W6/W7 → W8**.

---

### W1 — Exclude test/stress/template entries from the inventory  *(was C2; also fixes most of H4)*  ★ highest leverage

**Locus:** `Graph/NoiseFilter.cs` — `IsProductionEntrySource` (111-114); reuse the existing `IsTestPath`
(57-63). Same predicate should also gate trace `TOUCHES` (L2) in `Graph/TraceBuilder.cs`
(`CollectGraphEntities` / `IsNoiseEntity` 151-188).

**Fix sketch:**
1. In `IsProductionEntrySource`, also reject `ProjectClassifier.IsTestPath(filePath)` (catches
   `/test/testassets/`, `/test/WebSites/`).
2. Add path patterns for the non-`/test/` cases: `/stress/`, `/Testing/` (segment, not substring of
   "test"), and `/ProjectTemplates/` + `/content/` (template scaffolding). Keep these in `NoiseFilter`,
   not the project classifier (they are path conventions, not test *projects*).
3. Apply the same test-path predicate to `TraceBuilder` entity collection so test DbContexts stop
   appearing in `TOUCHES` (closes L2).

**Why not the report's "check `.csproj` properties":** these are *test assets* (web/console apps used by
tests) — they don't reference xunit and don't end in `Tests`, so project-property checks miss them. Path
convention is the correct signal here, and `IsTestPath` already encodes the main one.

**VERIFY:**
- Unit: extend `tests/DevContext.Core.Tests` with a `NoiseFilterTests` asserting
  `IsProductionEntrySource("src/Http/Routing/test/testassets/RoutingWebSite/X.cs") == false`,
  `".../Kestrel/stress/Program.cs" == false`, `".../ProjectTemplates/.../content/Program.cs" == false`,
  and `"src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs" == true`.
- Eval: `eval/expectations/aspnetcore.json` →
  `{ "type":"output-not-contains", "format":"markdown", "value":"/test/testassets/", "status":"expected" }`,
  same for `/stress/` and `ProjectTemplates`.
- Observable: `analyze analysis-repos/aspnetcore` HTTP entry count drops from **518** to a few dozen
  production Identity/Mvc endpoints (`POST /manage/info`, `POST /login`, …).

---

### W3 — Library / type-rooted traces follow member call edges  *(was C3; also H3, M4)*  ★

**Locus:** `Graph/TraceBuilder.cs` — `OutEdgesWithTwin` (330-339) + `BuildBridgeMemberIndex` (80-106).

**Root cause (verified):** from a **Type** node, `OutEdgesWithTwin` yields the type's own edges plus the
edges of *only* its handler-entry members. `Calls` edges are member-origin (kernel "one node per class", but
call edges hang off `Type.Method` Member nodes). A library entry (`Log`, `LoggerConfiguration`, `MainWindow`)
has no `Handle/Execute/Invoke` member, so zero edges are exposed → 4-line trace.

**Fix sketch:** when the **root/entry** node is a Type with no handler-entry bridge members, bridge **all**
its member nodes' out-edges for the first hop (the type's public methods → their callees). Scope the
broad bridge to the entry root (depth 0) or to `EntryPointKind.PublicApi` entries, so app traces (which
rely on the narrow handler bridge to avoid sibling-method fabrication) are unaffected. Keep member-origin
divergence intact: do **not** re-collapse all member edges onto the type for non-entry nodes.

**Guard against regressions:** the existing `TraceQualityTests` "sibling methods diverge / no fabricated
edges" Facts (lines 73-154) must stay green — they prove the *narrow* bridge is still narrow for app handlers.

**VERIFY:**
- Add a `TraceQualityTests` case (mirrors the existing `[InlineData]` table, lines 17-22):
  `("analysis-repos/serilog", "Log", new[]{ "Logger" })` and assert `seamHops >= 1` (the test's existing
  empty-trace guard, line 38). A non-empty `Log` trace = fixed.
- Observable: `analyze analysis-repos/serilog --focus Log` shows `call Logger.Write …`, not a lone
  `▸ ENTRY Log`.
- Regression: `dotnet test --filter "FullyQualifiedName~TraceQualityTests"` — all sibling-divergence Facts
  still pass.

**W3b (honesty seam, small):** in `DiscoveryPipeline.RenderAsync` (338-362), when `query.Trace` returns a
trace whose root has **no children**, append a one-line note ("no out-edges resolved for `<focus>` — try a
method focus `Type:Method`, or `--profile debug` to enable the call graph") rather than silently emitting a
bare ENTRY line. When `Trace` is *null* (focus matched nothing), the current Map fallback is fine but should
print *why* (no entry/node matched `<focus>`).

---

### W5 — Desktop-app archetype + entry points  *(was H1; also M4)*

**Locus:** `Graph/ArchetypeDetector.cs` (`AppEntryKinds` 14-18; `allExeAreAuxiliary` 46-50); new extractor;
`EntryPoint.cs`/`EntryPointKind`; `GraphBuilder.Build` entry assembly (37-63); `MapRenderer.GroupLabel`
(224-233).

**Two-part fix:**
1. **New entry kind + extractor.** Add `EntryPointKind.UiEntry` (or `Desktop`) and a Stage-3
   `DesktopEntryExtractor` gated on a WinUI/WPF/Avalonia signal. Detect: `App.OnLaunched`/`OnStartup`,
   `Window`/`Page`/`UserControl` subclasses, and MVVM command handlers (`ICommand.Execute`, `[RelayCommand]`
   from CommunityToolkit.Mvvm). Wire into `ArchetypeDetector.AppEntryKinds` so any desktop entry → `App`.
2. **Tighten the auxiliary-exe heuristic** (independently wrong): a non-sample `WinExe`/`Exe` that *is*
   the repo's product should not be classed "auxiliary" merely because it references library projects.
   Once (1) lands, the desktop entries short-circuit to `App` before this code runs; still, narrow the
   heuristic to treat an exe as auxiliary only when it is itself under a sample/benchmark path.

**VERIFY:**
- Add `ArchetypeDetectorTests` case: a fixture (or `analysis-repos/files`) with `OutputType=WinExe` +
  a WinUI signal classifies as **App**, not Library.
- Eval `eval/expectations/files.json`:
  `{ "type":"json-equals", "path":"$.archetype", "value":"App", "status":"expected" }` (start `aspirational`,
  ratchet to `expected` on fix).
- Observable: `analyze analysis-repos/files` header reads `MAP Files` with a `UI (N)` entry group
  (MainWindow, App), not `LIBRARY Files`.

---

### W4 — Render-time section cap on the narrative Map  *(was C1/H4, correctly scoped)*

**Locus:** `Rendering/MapRenderer.cs` — `AppendTopology` (126-144) and `AppendEntryPoints` (146-163).

**Fix sketch (stays inside the locked invariant):** cap per-section output **deterministically** (a fixed
constant, *not* `--max-tokens` — that would break `BudgetIndependenceTests`):
- Topology: show top-level / most-depended-on projects up to N; collapse the rest to "… and M more
  projects".
- Entries: rank (production first, then by kind), show up to N per kind with an explicit
  "… and M more `<kind>` entries (use `--focus`/grep)" — replacing the current "list ALL" comment.
Make N a `const` in the renderer. The Map stays byte-identical across token budgets; it just stops dumping
1,000 lines. With W1 in front of it, aspnetcore drops well under the old 21k.

**VERIFY:**
- `BudgetIndependenceTests` still green (same output at `--max-tokens 2000` and `20000`).
- Eval `aspnetcore.json`: `{ "type":"max-elapsed-ms" ...}` already guards time; add an output-size guard,
  e.g. a new tiny test asserting the rendered Map line count < 250 for aspnetcore.
- Observable: aspnetcore Map ≤ ~200 lines with a visible "… and N more" disclosure.

---

### W6 — Prefer the product solution when several sit at the repo root  *(gap G-A)*

**Locus:** `Extractors/Generic/SolutionDiscoveryExtractor.cs:49-54`.

**Fix sketch:** at equal depth, break ties deterministically toward the *product* solution: deprioritize
names matching `*.Samples`/`*.Tests`/`*.Benchmarks`; prefer the solution whose name equals the repo
directory name; else the one with the most non-sample/non-test projects. Emit an Info diagnostic naming the
chosen solution and listing the alternatives (so the user can override). (A future `--solution` flag is the
real escape hatch; `SolutionScope` already supports single-scope-per-run.)

**VERIFY:**
- Unit `SolutionDiscoveryExtractorTests`: given `Ocelot.slnx` + `Ocelot.Samples.slnx` at equal depth,
  selects `Ocelot`.
- Observable: `analyze analysis-repos/ocelot` header reads `MAP Ocelot`, scoped to core projects.

---

### W7 — API-gateway / reverse-proxy archetype  *(gap G-B; larger, scope separately)*

**Locus:** new signal in dependency detection (Ocelot, YARP `Microsoft.ReverseProxy`); new
`MapBuilder`/`MapRenderer` section.

**Fix sketch:** detect the gateway packages; when present, surface a **Routes/Clusters** section sourced
from config (`ocelot.json` `Routes[]`, YARP `ReverseProxy:Routes`) plus the middleware pipeline, instead of
(or beside) the empty HTTP-endpoint list. This is the only way a gateway's Map is useful; the call-graph
trace model doesn't apply to config-driven dispatch.

**VERIFY:** `eval/expectations/ocelot.json` `output-contains markdown "ROUTES"` (or similar) once shipped;
start `aspirational`.

---

### W8 — Entry→target fallback for view/no-call controller actions  *(was M2; low)*

**Locus:** `Graph/GraphBuilder.cs` — `ResolveEntryTarget` (80-114) / `ResolvePrimaryCall` (122-148).

**Fix sketch:** when no MediatR send and no in-scope service callee is found, fall back to the **owning
controller type** as the target (`GET /configuration → FileConfigurationController`) rather than emitting
nothing — more useful than a blank, and honest (it's the declaring type).

**VERIFY:** Ocelot Map shows `2/3 → target` become `3/3`; add an `output-contains` check on the
`GET /configuration` line carrying a `→`.

---

## 5. Verification protocol

Bootstrap (once): register the 4 repos in `eval-repos.json`, add `eval/expectations/{serilog,ocelot,files,
aspnetcore}.json` (model on `automapper.json` for the library, `dntsite.json` for the controller app), and
add the `analysis-repos` paths so `gates.ps1` can find them.

Per work item:
```powershell
dotnet build DevContext.slnx                                   # 0 warnings (analyzer warnings = errors)
dotnet test  DevContext.slnx --filter "Category!=Eval"         # fast unit tests stay green
dotnet run --project src/DevContext.Cli -- analyze <repo> --stats   # eyeball the affected artifact
powershell -File eval/gates.ps1                                # full gate incl. eval expectations
```
Ratchet discipline (matches `eval/expectations/SCHEMA.md`): land each new check as `aspirational`, flip it to
`expected` in the **same commit** that fixes the issue. Trace-quality regressions belong in
`TraceQualityTests`, not the JSON suite (the JSON checks run a Map and would pass on an empty trace).

**Do-not-regress anchors:** `BudgetIndependenceTests` (W4 must not touch it), the
`TraceQualityTests` sibling-divergence Facts (W3 must keep the narrow handler bridge narrow).
