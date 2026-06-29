# PLAN-11 — Desktop UI for manual testing + Trace "flesh"

> For a fresh agent. Branch `feature/trace-engine` (or a child). Base everything on the working
> engine from PLAN-10 (gate 4/4). Read `docs/IDEAL-OUTPUT-TARGET.md` first — it is the target;
> this plan closes the gap to it.

## Where we are (read before starting)

The trace engine works end-to-end: `--focus "POST /api/orders/"` on eShop Ordering produces
`entry → send CreateOrderCommand → handler CreateOrderCommandHandler → raises … + call Order[verified]`.
The **spine** is real; semantic resolution (Roslyn, source-compilation) is wired; gate is 4/4 plus
`TraceQualityTests` guarding the three archetypes. What's missing is the **flesh** that makes the
trace a *story* (doc §2) rather than a tree of names, plus there is **no UI surface** for it yet —
the engine backbone is already in `AnalysisService`, but the Blazor UI still drives the old
catalog/section model.

**Sequencing note (to hit usable-fast):** do **Part A (UI)** first. It's mostly wiring the existing
`snapshot.Entries` + `RenderRequest.Entry/Depth/Detail` into the panel, and it gives the human a way
to *manually explore and validate* every subsequent engine change. Then Part B (flesh), then Part C
(probe), then Part D (coverage).

---

## Part A — Desktop: entry picker → Trace, Map otherwise  (do first; enables manual testing)

The backbone already exists — `IAnalysisService.AnalyzeAsync` returns an `AnalysisSnapshot` with
`.Entries` / `.Map` / `.Graph`, and `RenderAsync(snapshot, RenderRequest)` already honours
`Entry` / `Depth` / `Detail` (analyze-once-render-many, cheap re-render). The job is to surface it.

Files: `src/DevContext.Desktop/ViewModels/MainViewModel.cs`, `OutputViewModel.cs`,
`Components/ConfigPanel.razor`, `Components/OutputPanel.razor`, `Services/AnalysisService.cs`
(`AnalysisOptions` already has `Around`; add `Depth`/`Detail`).

1. **Entry picker.** After analyze, populate an autocomplete from `snapshot.Entries`
   (`Title` = "POST /api/orders", group by `Kind`). Selecting one sets `RenderRequest.Entry` and
   re-renders **without re-analyzing** (call `IAnalysisService.RenderAsync` only). Clearing it
   re-renders the Map. This *is* the "pick entry → Trace, none → Map" surface from doc §6 — no mode
   toggle. Free-text entry also works (it flows to `ResolveEntryFromNode`, so a type name traces too).
2. **Dials.** Add `Depth` (1–10 slider, default 6) and `Detail` (signature / salient / full) controls,
   bound to `RenderRequest.Depth`/`Detail`; re-render on change (debounced — `Helpers/Debouncer.cs`
   already exists).
3. **Retire the section model for graph runs.** `SectionSelectionModel`/`SectionViewModel` model the
   old 12 catalog sections. When `snapshot.Graph.NodeCount > 0`, hide them; show instead Map-facet
   toggles (topology / entries / cross-cutting / packages) — these map to the `MapRenderer` sections.
   Keep the section model only for the legacy fallback path.
4. **Render surface.** The Map/Trace output is monospace tree text. `OutputPanel.razor` currently
   shows HTML; render the Map/Trace `Content` in a `<pre>` (monospace) view. Provenance `file:line`
   strings should become clickable later (open in editor) — optional.

**Acceptance:** launch the app on `eval-repos/eShop/src/Ordering.API`, pick `POST /api/orders/` from
the picker, see the bridged trace; clear it, see the Map; move the depth slider and watch it re-render
without re-analyzing. (Verify with the `/run` or `/verify` skill — WPF + BlazorWebView, `Program.cs`.)

---

## Part B — Trace "flesh" (closes the gap to the doc §2 ideal)

Priority order is value-per-effort. **P0 first.**

### B1 (P0) — Salient body lines at `--detail salient`
Today `TraceRenderer` ignores `TraceDetail`; every node is a bare name. The §2 target shows, under a
node, the *salient lines* ("builds CreateOrderCommand :141", "raises OrderStartedDomainEvent :170").
- Add salient lines to `TraceStep` (e.g. `ImmutableArray<string> Salient`), populated in
  `TraceBuilder` from the node's `TypeDiscovery.SourceBody` around the edge's provenance line
  (the seam construction site). `SourceBody` is already populated in Debug profile (PLAN-10 fix).
- `TraceRenderer.Render` honours `detail`: `Signature` = names only (today), `Salient` = names + 1–3
  salient lines per node, `Full` = the method slice. This is the single highest-leverage change —
  it turns the "tree of names" into the "story".

### B2 (P0) — Summary footer (RESULT / TOUCHES / EMITS / NEXT)
`Trace.TouchedEntities` / `EmittedEvents` already exist but are never filled. Add a summary pass in
`TraceBuilder.Build` that walks the assembled tree and collects Entity nodes (TOUCHES) and Event nodes
(EMITS); `TraceRenderer` prints the footer (it already has the code path, just unfed).

### B3 (P1) — `pipeline` seam (MediatR `IPipelineBehavior`)
Every `send` is wrapped by registered `IPipelineBehavior<,>` (Logging → Validation → Transaction).
Detect behavior registrations (a small extractor or extend `DiRegistrationExtractor`), add a
`WrappedBy` edge `Request → behavior`, and render it **once** under the first `send` (doc §2 shows it
inline, not per row). `EdgeKind.WrappedBy` + `SeamKind.Pipeline` already exist.

### B4 (P1) — Per-method anchoring (precision)
Minimal-API/method-group entries currently anchor on the owner *type* (e.g. `OrdersApi`), so the trace
shows the file's combined calls. Anchor on the specific handler **method**: create Member nodes for
endpoint handlers and attribute each endpoint's lambda/method body's calls to it. Touches
`GraphBuilder.AddHttpEntryPoints` (link entry → Member node) and `CallGraphExtractor` (already emits
per-method edges; stop the Type→Type collapse in `AddCallEdges` for handler members).

### B5 (P2) — domain-event save-time dispatch
Model the UoW convention (events dispatched at `SaveChanges`, then EF write) so a `raises
OrderStartedDomainEvent` bridges to its handler via the save boundary, not just the raise site.

---

## Part C — Run the §7 validation probe (gates further investment)

`docs/IDEAL-OUTPUT-TARGET.md` §7 + `docs/reports/probe-kit.md`. Now finally runnable (traces aren't
empty). Give a fresh LLM "add a per-line discount to orders" with (a) our trace, (b) raw files,
(c) the legacy catalog; score correctness / navigation / completeness / explanation. Record the result
in `docs/reports/`. If the trace wins → fund Part D + the persistent index (PLAN-10 Part G). If not →
the probe tells you *which* flesh is missing before you build more. Do this after B1+B2 (the trace
should be in "story" form for a fair test).

---

## Part D — Coverage & detection gaps (P2, surfaced by the refreshed eval)

- **Cross-service integration-event** (publish → consumer across projects) + multi-project scope
  (archetype C). `SolutionScope` seam exists; pointing at a sub-project sees one project. Run the
  engine per scope and join published event types to consumers across the closure.
- **Library archetype D** (AutoMapper surface map by capability, doc §4) — currently falls through to
  the generic Map.
- **FastEndpoints `<dynamic>` routes** — routes set in `Configure()` via `Get("/x")` aren't captured
  (`verticalslice/no-dynamic` aspirational). Extend `FastEndpointsHelper`.
- **Aspire signal** — package is `Aspire.Hosting.*`, the map keys `Microsoft.Aspire`
  (`DependencyExtractor.PackageSignalMap`); fixing it lets eShop read `Microsoft­services`
  (`eshop/arch-style` + `aspire-signal` aspirational).
- **`Mediator` library** (martinothamar) — VerticalSlice uses it, not `MediatR`; teach
  `DependencyExtractor` the package (handlers already detected via the shared `IRequestHandler`).

When any aspirational eval check starts passing, **ratchet it to `expected` in the same commit**
(`eval/expectations/*.json`), and add a `TraceQualityTests` row for any new archetype/seam.

---

## Gate & done criteria
- `eval/gates.ps1` → PASS (build · fast tests · eval incl. `TraceQualityTests` · CLI matrix).
- App launches and a human can pick an entry → see a *story-form* trace (B1/B2) and move dials.
- Probe result recorded in `docs/reports/`.
- Every newly-fixed aspirational check ratcheted; no stale `iteration-4` notes left describing
  behaviour that has changed.
