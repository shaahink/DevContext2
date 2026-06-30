# Work Items — live plan

Grouped by capability theme (A–E). Each item: **status · root cause · locus · fix · self-VERIFY**. Loci are
verified `file:line` on `feat/engine-cross-repo-analysis` @ `ff143d8`. Reform in place; never rewrite
extractors. Verdict evidence for each is in `../VERIFIED-PLAN.md`.

Status: ✅ done · 🔄 in progress · ⬜ todo · 🔬 research.

---

## Theme A — Archetype recognition ("what is this?")

### W5 ✅ Desktop-app archetype + entry points
**Root cause (verified):** (a) `ArchetypeDetector.AppEntryKinds` (`Graph/ArchetypeDetector.cs:14-18`) is
HTTP/Bus/Hosted/Scheduled only — desktop apps have none, so they never short-circuit to `App`. (b) The
`allExeAreAuxiliary` heuristic (`ArchetypeDetector.cs:46-50`) then treats `Files.App`
(`OutputType=WinExe`, references lib projects) as a *library sample* → **Library**. Confirmed: Files.App.csproj
is `WinExe`.
**Fix:**
1. New `EntryPointKind.UiEntry`; new Stage-3 `DesktopEntryExtractor` gated on a WinUI/WPF/Avalonia signal
   (detect `App.OnLaunched`/`OnStartup`, `Window`/`Page`/`UserControl` subclasses, `[RelayCommand]` /
   `ICommand.Execute`). Add `UiEntry` to `AppEntryKinds`; add a `GroupLabel` case (`MapRenderer.cs:224-233`).
2. Narrow `allExeAreAuxiliary`: an exe is auxiliary only when itself under a sample/benchmark path — a
   non-sample `WinExe`/`Exe` is the product.
**VERIFY:** `ArchetypeDetectorTests` case: `WinExe` + WinUI signal → `App`. Eval `files.json`
`json-equals $.archetype == "App"` (land aspirational, ratchet). Observable: `analyze .../files` header
`MAP Files` with a `UI (N)` group (MainWindow, App), not `LIBRARY Files`.

### W7 ✅ API-gateway / reverse-proxy archetype
**Root cause:** Ocelot's surface is dynamic config-driven routes via middleware, not MVC/Minimal-API
endpoints — structurally unrepresentable in the HTTP-entry model. Same for YARP.
**Locus:** new dependency signal (Ocelot / `Microsoft.ReverseProxy`); new `MapBuilder`/`MapRenderer` section.
**Fix:** detect gateway packages; surface a **Routes/Clusters** section from config (`ocelot.json Routes[]`,
YARP `ReverseProxy:Routes`) + the middleware pipeline, instead of the empty endpoint list.
**VERIFY:** `ocelot.json` eval `output-contains markdown "ROUTES"` (aspirational until shipped).

---

## Theme B — Entry-point fidelity ("how to use / where to start")

### W1 ✅ Exclude test-asset / stress / template entries from the inventory
**Done (prior session).** `Graph/NoiseFilter.cs` — `IsProductionEntrySource` now also rejects
`ProjectClassifier.IsTestPath` (the `/test/` tree, catching `testassets/`) and a new `IsNonRuntimeEntrySource`
(`/testassets/`, `/Testing/`, `/stress/`, `/perf/`, `/FunctionalTests/`, `/ProjectTemplates/`). Project-level
classifier can't see *test assets* (web/console apps used by tests — no xunit, not `*Tests`); these are path
conventions. **Root-relative:** the path-convention checks match the portion **below** the analysis root
(`NoiseFilter` takes the root; `DiscoveryPipeline` passes `context.RootPath`) — so analysing a repo that
itself lives under a `…/tests/…` path (our own `tests/fixtures/ControllerApp`) doesn't exclude its surface.
**Verified:** `NoiseFilterTests.cs` (13 cases incl. root-relative) + `Server AnalyzeFlowTests` 7/7 + full Core
296 + e2e aspnetcore **518→10** (real Identity API only). Gate: see `PROGRESS-LOG.md`.
**L2 done (this session):** `NoiseFilter` checks added to `GraphBuilder` node-creation methods that were
missing them (`AddEntityNodes`, `AddEventConsumers`, `AddHandlerJoins`, `AddPipelineBehaviors`) — test
DbContexts and entities no longer leak into the graph or traces.

### W6 ✅ Prefer the product solution when several sit at the repo root
**Root cause (verified):** `Extractors/Generic/SolutionDiscoveryExtractor.cs:49-54` picks root-closest, then
breaks same-depth ties by **enumeration order** + `.First()`. Ocelot ships `Ocelot.slnx` **and**
`Ocelot.Samples.slnx` at root → it picked **Samples**, scoping the whole Map to the sample aggregator.
**Fix:** at equal depth, deprioritise `*.Samples`/`*.Tests`/`*.Benchmarks`; prefer name == repo dir, else the
solution with the most non-sample/non-test projects. Emit an Info diagnostic naming the choice + alternatives.
**VERIFY:** `SolutionDiscoveryExtractorTests`: `Ocelot.slnx`+`Ocelot.Samples.slnx` equal depth → selects
`Ocelot`. Observable: `analyze .../ocelot` header reads `MAP Ocelot`.

### W8 ✅ Entry→target fallback for view / no-call controller actions
**Root cause (verified):** `Graph/GraphBuilder.cs` `ResolveEntryTarget`(80-114)/`ResolvePrimaryCall`(122-148)
return null when an action has no MediatR send and no in-scope, non-framework, non-self callee (e.g. returns a
view or a property). Ocelot `GET /configuration` → no target (Map shows `2/3 → target`).
**Fix:** fall back to the **owning controller type** as the target rather than nothing — honest (it's the
declaring type) and more useful than a blank drill-in hint.
**VERIFY:** Ocelot Map `2/3` → `3/3`; `output-contains` a `→` on the `GET /configuration` line.

---

## Theme C — Trace / focus dive-in ("examine any part")

### W3 ✅ Library / type-rooted traces follow member call edges  ★ next
**Root cause (verified):** `Calls` edges are **member-origin** (hang off `Type.Method` Member nodes). From a
**Type** entry, `TraceBuilder.OutEdgesWithTwin` (`Graph/TraceBuilder.cs:330-339`) bridges only
**handler-entry members** (`Handle/Execute/Invoke`, via `BuildBridgeMemberIndex` 80-106). A library/UI type
(`Log`, `LoggerConfiguration`, `MainWindow`) has none → zero followable edges → 4-line trace.
**Fix:** when the **entry root** is a Type with no handler-entry bridge members, bridge **all** its member
nodes' out-edges for the first hop. **Scope to the entry root (depth 0) / `EntryPointKind.PublicApi`** so app
handler traces keep the narrow bridge (which is what prevents sibling-method fabrication). Do NOT re-collapse
member edges onto the type for non-entry nodes.
**VERIFY:** add a `TraceQualityTests` `[InlineData]`: `("analysis-repos/serilog", "Log", new[]{"Logger"})`,
assert the existing `seamHops >= 1` guard (line 38) and the substring. Regression: the sibling-divergence
Facts (`TraceQualityTests` 73-154) stay green. Observable: `analyze .../serilog -f Log` shows
`call Logger.Write …`.

### W3b ✅ Honest message when a focus resolves but has no out-edges
**Root cause:** `DiscoveryPipeline.RenderAsync` (338-362): a trace whose root has no children renders a bare
`▸ ENTRY X`; a null trace silently falls back to the Map (the Files `--focus IAppService` "Map fallback"
confusion — H3). Both are opaque.
**Fix:** for an empty-but-resolved trace, append a one-line hint ("no out-edges resolved for `<focus>` — try
`Type:Method`, or `--profile debug` to enable the call graph"). For a null match, print why ("no entry/node
matched `<focus>`") before the Map. Pairs with W3.
**VERIFY:** unit on a no-edge focus → output contains the hint, not a bare ENTRY line.

---

## Theme D — Map "at a glance" readability

### W4 ✅ Structural section caps + ranking for huge repos (NOT token-driven)
**Root cause (verified):** the narrative Map has **no truncation** — `MapRenderer.AppendEntryPoints`
(`Rendering/MapRenderer.cs:146-163`) lists ALL entries ("no '... and N more'") and `AppendTopology` (126-144)
lists ALL projects (aspnetcore: 395). The token budget is intentionally out of this path
(`BudgetIndependenceTests`), so the cap must be **structural/deterministic**, not `--max-tokens`-driven.
**Fix:** cap per section with a `const` N, ranked (entries: production-first then by kind; topology:
most-depended-on / top-level), with an explicit "… and M more (use `--focus`/grep)" disclosure. With **W1** in
front, aspnetcore already drops sharply; W4 bounds the worst case.
**VERIFY:** `BudgetIndependenceTests` stays green (same output at `--max-tokens 2000` vs `20000`). Add a test:
aspnetcore Map line count < 250. Observable: Map ≤ ~200 lines with visible "… and N more".

---

## Theme E — Engine hygiene (strategic)

### W9 🔬 Quarantine / retire the legacy catalog + token machinery
**Why:** `Pruning/TokenBudgetEnforcer.cs` + `Pruning/PatternRelevancePruner.cs` + the `RenderPlanBuilder`
catalog feed only `--format json|html`. The Map/Trace (the product) ignore them by design
(`DiscoveryPipeline.cs:117-125,596-599`). Yet `--stats` surfaces their pruning funnel as if it explained the
Map — which is exactly what derailed the DeepSeek analysis (its entire "99% cut" / "scorer no-op" thesis).
The user has flagged token as non-core and a removal candidate.
**Options (decide with the user):** (a) keep JSON/HTML but stop printing the catalog funnel in `--stats` for
narrative runs; (b) reduce JSON to a serialization of the `CodeGraph`/`MapModel` (kill the catalog +
pruners + `TokenBudget` entirely); (c) leave as-is but document loudly. (b) is the clean end state and
deletes a large confusing surface; gate it behind confirming no eval/JSON consumer needs the old shape.
**VERIFY:** if (b): `EvalExpectationTests` JSON checks migrate to the graph-shaped JSON; full gate green.

---

## Coverage research (🔬, parallelisable)

Add the missing archetypes from `BENCHMARK-MATRIX.md` Tier 3 (CLI, worker, gRPC, Blazor, MAUI/Avalonia,
classic MVC, serverless, a second library). For each: clone a small canonical repo → register in
`eval-repos.json` + `eval/expectations/<name>.json` → capture Map+Trace into `../<name>/` → add a matrix row
with the gap. This is how we earn "go-to for **any** .NET repo."
