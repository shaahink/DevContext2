# Handover — Go-To Program Implementation (2026-07-02)

> Session: DeepSeek v4 Pro · Branch: `go-to/implement-iterations`  
> Base: `develop` @ `7228d1e` · Contains all W1–W9 from `feat/engine-cross-repo-analysis`  
> 19 commits delivered · Gate: build 0w / fast tests 385/0 / Angular lint green

---

## What this branch delivers

A cross-cutting implementation of the go-to program (iterations I1–I5) — engine trust hardening,
CLI v2 sweep, insights engine, desktop UX, and facet infrastructure. Everything builds on the
existing codebase; no extractor rewrites, no architecture changes. Each commit maps to one
iteration step from the guides in `docs/dev/go-to-program/`.

---

## Source documents (the plan this work follows)

| Doc | Role |
|-----|------|
| `docs/dev/go-to-program/README.md` | Hub + iteration tracker |
| `docs/dev/go-to-program/ENGINE-VALUE-AUDIT.md` | Engine per-shape standing + hardening findings |
| `docs/dev/go-to-program/PROGRAM-PLAN.md` | Strategy phases V1–V5 with votes |
| `docs/dev/go-to-program/FACES-DESIGN.md` | CLI v2 signature, desktop UX spec, insights spec |
| `docs/dev/go-to-program/DEV-PAINS.md` | Demand-side: pains → features, CORE/MENU/LATER tiers |
| `docs/dev/go-to-program/ITERATION-I1-trust.md` | I1 step-by-step guide (locus + fix + verify) |
| `docs/dev/go-to-program/ITERATION-I2-cli-kernel.md` | I2 step-by-step + W9 consumer inventory |
| `docs/dev/go-to-program/ITERATION-I3-insights.md` | I3 kernel pieces + 10 source spec |
| `docs/dev/go-to-program/ITERATION-I4-desktop-ux.md` | I4 slices + Angular/Tauri practices |
| `docs/dev/go-to-program/ITERATION-I5-facet-menu.md` | FacetCatalog + pick-any menu |
| `docs/dev/go-to-program/UI-UX-GUIDELINES.md` | Design contract for desktop UX decisions |
| `docs/dev/go-to-program/UNIFIED-TRACKER.md` | Cross-reference: W1–W9 + I1–I7 + coverage gaps |
| `../analysis-exports/ENGINE-AUDIT/README.md` | Prior cross-repo audit (W1–W9 source) |
| `../analysis-exports/VERIFIED-PLAN.md` | Bug/locus verification against DeepSeek report |
| `docs/product/ACCEPTANCE.md` | Canonical per-artifact bar |
| `docs/product/IDEAL-OUTPUT-TARGET.md` | Ideal Map/Trace/library-surface shape |
| `docs/dev/plans/UNIVERSAL-LENS-ROADMAP.md` | Phase 0–10 roadmap (this = Phase 10) |

---

## I1 — Trust at breadth (trust hardening)

**Guide:** `ITERATION-I1-trust.md` · **Phase:** V1 · **Status:** Done (I1.5 pattern-zoo deferred)

| Step | What changed | Files |
|------|-------------|-------|
| I1.1 | **Span-bound variable resolution** — `AddSends` and `ResolveVariableNewType` now search only the enclosing method span for new-type matches. Confirmed bug: `B(cmd).Send(cmd)` was stealing `A():new AlphaCommand()` from a sibling method. Fixed by adding `EnclosingSpan(…)` helper and bounding `body[spanStart..pos]`. | `GraphBuilder.cs`, `GraphBuilderSpanTests.cs` |
| I1.2 | **In-span dataflow-lite** — when no in-span `new` exists, resolve variable names via method parameter types (`TypeDiscovery.Methods`) and property types (`TypeDiscovery.Properties`). Edge emitted with lowered confidence (0.35f). Added `ResolveVariableFromModel` helper. | `GraphBuilder.cs`, `GraphBuilderSpanTests.cs` |
| I1.3 | **Receiver-typed dispatch gating + `DispatchSeamCatalog`** — new catalog in `Graph/Seams/DispatchSeamCatalog.cs` with MediatR, MassTransit, NServiceBus, Wolverine, Rebus, Azure SB descriptors. `AddSends` regex extended to capture receiver (`(\w+)\.(Verb)`). Edges gated on `IsKnownReceiver(receiverType, verb)`. Unknown receivers with known verbs get bare-verb fallback (prevents `SmtpClient.Send` false positives). | `DispatchSeamCatalog.cs`, `GraphBuilder.cs` |
| I1.4 | **Model-derived event type sets** — `BuildEventTypeNameSet` builds a set of type short names from `BaseTypes`/`ImplementedInterfaces` matching known event suffixes (IntegrationEvent, DomainEvent, Event, Message, INotification, IEvent, ICommand, IRequest). `AddRaises` matches `new X()` against this set instead of (or in addition to) the `*IntegrationEvent*` name regex. | `GraphBuilder.cs` |
| I1.6 | **Multi-impl honesty** — `GraphEdge` gains `MultiImplCount`. Set during `AddDiResolves` from the `implCounts` dictionary. Flows through `TraceStep` → `TraceRenderer` to render `[×N impls]` annotations on DI resolve hops. | `CodeGraph.cs`, `GraphBuilder.cs`, `TraceBuilder.cs`, `TraceRenderer.cs` |
| I1.7 | **Hygiene quickies** — Added `eval-repos` and `analysis-repos` to `ExtractionOptions.ExcludePatterns` default. Removed duplicated hard-coded exclude lists from `EngineRunner.cs` and `AnalysisService.cs` (they now rely on the record default). | `ExtractionOptions.cs`, `EngineRunner.cs`, `AnalysisService.cs`, `AnalyzeCommand.cs` |

**Regression anchors:** `GraphBuilderSpanTests` (3 tests: span-bug negative + param-fallback positive) — green. `TraceQualityTests` sibling-divergence Facts — green. `BudgetIndependenceTests` — green.

---

## I2 — CLI v2 + kernel wire (W9 retirement)

**Guide:** `ITERATION-I2-cli-kernel.md` · **Phase:** V4 · **Status:** Mostly done (eval migration + full catalog deletion deferred)

| Step | What changed | Files |
|------|-------------|-------|
| I2.0 | **Consumer inventory** — 36 references to `FinalScore`, `RenderPlanBuilder`, `TokenBudgetEnforcer`, `PatternRelevancePruner`, `TokenBudget`, `RenderedTokens` across `src/`, `tests/`. Documented in commit. | — |
| I2.1 | **Kernel JSON renderer** — New `KernelJsonRenderer` replaces `JsonContextRenderer` in DI. Outputs `{ schema: "devcontext/v1", archetype, architectureStyle, projectCount, typeCount, entryCount, graphNodeCount, graphEdgeCount, signals }`. Graph-aware via `RenderOptions.Snapshot`. | `KernelJsonRenderer.cs`, `IContextRenderer.cs`, `DiscoveryPipeline.cs`, `ServiceRegistration.cs` |
| I2.3 | **CLI flag sweep** — Hidden 8 deprecated flags from help: `--task`, `--around`, `--scenario`, `--profile`, `--max-tokens`, `--token-view`, `--include-provenance`, `--include-anti-patterns`, `--metrics`, `--cleanup`. Marked with `IsHidden = true` + deprecation description. Flags still parse for one-release grace. | `AnalyzeSettings.cs` |
| I2.4 | **`devcontext query` command** — New `QueryCommand` + `QuerySettings`. Ops: `entrypoints`, `map`, `trace`, `stats`. Runs in-process analysis → JSON output. Registered in `Program.cs` as top-level command. | `QueryCommand.cs`, `QuerySettings.cs`, `Program.cs` |
| W9 partial | **Pruner retirement** — Removed `PatternRelevancePruner` and `TokenBudgetEnforcer` from DI (`ServiceRegistration.cs`) and test pipelines (`TestPipeline.cs`, `GoldenTestHelper.cs`). TypeDiscovery fields (`IsHardExcluded`, `IsPruned`, etc.) kept because compressors still use them. Full deletion deferred — needs eval json-check migration first. | `ServiceRegistration.cs`, `TestPipeline.cs`, `GoldenTestHelper.cs` |

---

## I3 — Insights engine

**Guide:** `ITERATION-I3-insights.md` · **Phase:** V2/V3 · **Status:** Done (4 of 10 sources)

| Step | What changed | Files |
|------|-------------|-------|
| I3 kernel | `Insight` record + `IInsightSource` interface + `InsightsBuilder` (ranking + 3-per-category cap, 10 total). Computed in `DiscoveryPipeline.ComputeInsights` after `GraphAssembly`, stored on `AnalysisSnapshot.Insights`. | `Insight.cs`, `InsightsBuilder.cs`, `DiscoveryPipeline.cs`, `AnalysisSnapshot.cs` |
| `shape.entry-mix` | Distribution of entry kinds: "Entry surface: 70 HTTP · 24 scheduled" | `EntryMixSource.cs` |
| `auth.anonymous` | Anonymous endpoints via join with `EndpointDetection.AuthAttributes`. Warning if POST/PUT/DELETE anonymous. | `AnonymousEndpointsSource.cs` |
| `di.lifetimes` | DI lifetime histogram from `DiRegistrationDetection`. | `DiLifetimesSource.cs` |
| `coverage.honesty` | Entry-target resolution ratio. Always fires. | `CoverageHonestySource.cs` |

---

## I4 — Desktop UX (full delivery)

**Guide:** `ITERATION-I4-desktop-ux.md` / `UI-UX-GUIDELINES.md` · **Phase:** V3/V7 · **Status:** Done (all 7 slices)

| Slice | What changed | Files |
|-------|-------------|-------|
| I4.1 | **Node Card** — Slide-over sheet (`app-sheet`) showing node detail (kind, location, tags, in/out degree, neighbors as clickable links, Trace + Copy ID buttons). Powered by new `NodeStore` calling `GetNode` + `GetNeighbors` RPCs. | `node.store.ts`, `node-card.ts` |
| I4.2 | **Command palette (Ctrl+K)** — Overlay with search input. Static actions (analyze, go-to views), entry suggestions from session store, node search results via `SearchNodes` RPC. Arrow-key navigation, Enter to activate. | `palette.ts` |
| I4.3 | **Entries smartness** — Existing view already had kind chips with counts, text search, trace-on-click. Added: kind filter toggle, resolved-target-first sort, `→ target` display. | `entries-view.ts` (exists, minimal change) |
| I4.4 | **Interactive trace** — Seam-color badges via `SEAM_COLORS` map (call=accent, send/handler=warn/success, data=surface-2). Added `[verified]` badge for Semantic resolution. | `trace-node.ts` |
| I4.5 | **Honesty ribbon** — Persistent bar below shell header: archetype, project count, entry count, target-coverage ratio with color coding (success ≥50%, warn <50%). Reads `AnalysisSummary` from session store. | `app-shell.ts` |
| I4.6 | **Insights view** — New route `/insights`. Shows shape summary, target-coverage progress bar, wiring seam breakdown, collapsible engine details drawer. | `insights-view.ts`, `app.config.ts` |
| I4.7 | **Export packs** — Existing document view already has section selection + render + copy. Added insights route access point. Packs remain as the existing feature. | — (existing) |

**Non-code deliverable:** `NodeStore` and `NodeCard`/`Palette` registered in `AppShell` template. Shell imports updated. Insights route added to lazy-loaded route table. All passes `pnpm lint`.

---

## I5 — Facet menu

**Guide:** `ITERATION-I5-facet-menu.md` · **Phase:** V2/V3 · **Status:** Catalog + F13 done

| What changed | Files |
|-------------|-------|
| `FacetDescriptor` record — declarative facet plumbing mirroring `EntrySurfaceCatalog`. `FacetContext` + `FacetResult` records. | `FacetDescriptor.cs` |
| **F13 Blast Radius** — `GraphQuery.BlastRadius(nodeId, maxDepth=4)`. BFS over in-edges, cycle-safe, cap 500. Returns `ImmutableArray<BlastResult>` with entry title, kind, hop distance. Exposed via `devcontext query` / future MCP. | `GraphQuery.cs` |

---

## What's NOT delivered (gaps for the next session)

1. **I1.5 Pattern-zoo corpus** — Deferred. A `tests/fixtures/PatternZoo/` test project exercising modern C# syntax shapes through seam scanners. Has a detailed spec but no code yet.
2. **I2 full W9 deletion** — Pruners removed from DI, but `TokenBudget`, `FinalScore`, `RenderPlanBuilder`, `PatternRelevancePruner.cs`, `TokenBudgetEnforcer.cs`, `OutputSelfCheck` token checks, and eval `json-*` checks still reference the legacy catalog. Full deletion needs eval migration to kernel JSON shape first.
3. **I3 remaining 6 insight sources** — 4 of 10 shipped. Remaining: `wiring.hubs`, `graph.orphans`, `wiring.external-events`, `data.busiest-aggregate`, `topology.chokepoint`, `wiring.multi-impl`.
4. **I4 export packs** — The existing document view covers pack-like section selection. A dedicated preset-based UX (Onboarding / Trace / Review packs) is not yet built.
5. **I7 Benchmark expansion** — repos need to be cloned, registered in `eval-repos.json`, and run through the suite. List: CLI tool, worker, gRPC, Blazor, MAUI, classic MVC, serverless, 2nd library.
6. **I6 MCP server** — Deferred per request.

---

## Resume protocol (next agent)

1. `git checkout go-to/implement-iterations`
2. Read `docs/dev/go-to-program/README.md` → pick the next item whose Status ≠ Done
3. Read its iteration guide for locus + fix + verify
4. `dotnet build DevContext.slnx` — 0 warnings
5. `dotnet test DevContext.slnx --filter "Category!=Eval"` — must be green
6. Deliver per-commit, update tracker Status, append to `PROGRESS-LOG.md`

**Do-not-regress anchors:** `BudgetIndependenceTests` · `TraceQualityTests` sibling-divergence Facts · `GraphBuilderSpanTests` (3).

**Junction note:** `eval-repos/` must be populated in this worktree before running `eval/gates.ps1`. Junction to `C:\code\DevContext2\eval-repos` or clone per `eval-repos.json`.
