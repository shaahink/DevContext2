# MCP Blind-Drive Audit — Fix Verification

**Date:** 2026-07-15
**Branch:** `feat/mcp-drive-audit`
**Follows:** `eval-results/2026-07-11/mcp-blind-drive-report.md` (the audit that found these 3 bugs)

All 3 bugs from the 2026-07-11 blind-drive audit are fixed, unit-tested, and re-verified live against
the same 2 fixture repos (CleanArchProject, ControllerApp) the original audit used.

## Root causes and fixes

| # | Symptom (2026-07-11) | Root cause | Fix |
|---|---|---|---|
| 1 | `trace("/products")` / `get_context("/products")` → "No entry or node matched" | `EntryPointResolver.Resolve` only matched an entry's exact `Title` ("GET /products"); never read the `Route` field, despite its own doc comment claiming it did. `ContextPackBuilder.ResolveFocus`/`ResolveFocusWithReach` (used by the UI's multi-card `get_context_pack`) had the identical gap. | `EntryPointResolver.cs`: added a bare-route branch — matches `HttpEndpoint` entries by normalized `Route`; single match resolves directly, an ambiguous route (several verbs on one path) prefers GET over failing. Same fix mirrored in `ContextPackBuilder.FindEntry` (was two near-duplicate `ResolveFocus`/`ResolveFocusWithReach` methods; consolidated into one). |
| 2 | `read_source` / `node` → `lineNumber: undefined` for every EntryPoint node | All 11 `IEntryPointBuilder`s constructed their `GraphNode(..., NodeKind.EntryPoint)` with `FilePath` but never `LineNumber` — even though every `Detection` has a `required int LineNumber`. | Stamped `LineNumber = <detection>.LineNumber` on the `GraphNode` in all 11 builders (Http, Grpc ×2, Desktop, Cli, Worker, Functions, MessageConsumer, DomainEventHandler, Signalr, OrleansGrain, GraphQl). |
| 3 | `top_flows` → `target: null` for every flow | `GraphBuilder.Build` called `ComputeFlows(preGraph, entries)` **before** `EnrichEntryTargets` ran — `graph.Flows[i].Entry` was a snapshot of the un-enriched entry, frozen with `Target = null`. The returned entry inventory (used by `entrypoints`) was enriched correctly, which is why only `top_flows` looked broken. | Reordered `GraphBuilder.Build`: enrichment (target/group-path/score) now runs on `preGraph` before `ComputeFlows`, so `graph.Flows` carries the same enriched entries as the returned inventory. `preGraph` and the final graph share identical nodes/edges (`DetectLayerViolations` only adds metadata), so this is safe. |

## Unit tests added

- `tests/DevContext.Core.Tests/EntryPointResolverTests.cs` (new, 6 tests) — bare-route match, ambiguous-prefers-GET, falls back to first when no GET, still matches exact title first, unknown route → null.
- `tests/DevContext.Core.Tests/GraphBuilderTests.cs` (+2 tests) — `Http_entry_point_node_stamps_line_number`, `Flow_carries_resolved_target_matching_enriched_entry`.

## Test results

- `dotnet build DevContext.slnx`: 0 warnings, 0 errors.
- Core: **448 passed**, 3 skipped, 0 failed (was 440P/3S before this session's 8 new tests).
- Server: 14 passed, 0 failed.
- Desktop (legacy WPF, removed by the `develop` merge folded into this branch): 64 passed, 0 failed, pre-merge.
- Eval-category (`Category=Eval`, 27 real-repo expectation checks): 25 passed, **2 pre-existing failures confirmed unrelated** by re-running the identical tests against the unmodified code (`git stash`): `dntsite` target-name gap (`FeedsService.GetNewsAsync`/`GetCommentsAsync` not resolved — a separate, already-tracked target-resolution gap) and `yarp` archetype-gateway (expected `Gateway`, detector returns `App` — the known "no gateway archetype" gap from `project-engine-audit`). Neither touches route resolution, line numbers, or flow targets.

## Live functional re-verification

Re-ran `eval-results/2026-07-11/mcp-blind-drive.mjs` (the exact audit script) against both fixture
repos with the fixes applied. Full capture: see below; highlights:

**CleanArchProject** — `trace("/products")` (bare route, single match):
```
--- STEP 6: top_flows ---
    HttpEndpoint: GET /products → GetProductsQuery (score=0.8, depth=4)      # was target: null

--- STEP 7: trace("/products") compact ---
  Found: true                                                                # was "No entry or node matched"
  ...
--- STEP 9: node("EntryPoint:GET /products") ---
  Kind: EntryPoint | File: src/Web/Program.cs:13                             # was :undefined
```

**ControllerApp** — `trace("/api/Products")` (bare route, ambiguous across DELETE/POST/GET):
```
--- STEP 6: top_flows ---
    HttpEndpoint: POST /api/Products → ProductService.CreateAsync (score=0.5, depth=3)
    HttpEndpoint: DELETE /api/Products → ProductService.DeleteAsync (score=0.329, depth=3)
    HttpEndpoint: GET /api/Products → ProductService.GetByIdAsync (score=0.329, depth=3)
                                                                               # all 3 were target: null

--- STEP 7: trace("/api/Products") compact ---
  Found: true                                                                 # correctly resolved to GET (ambiguous-prefers-GET)

--- STEP 9: node("EntryPoint:DELETE /api/Products") ---
  Kind: EntryPoint | File: src/Api/Controllers/ProductsController.cs:35       # was :undefined

--- STEP 10: get_context("/api/Products", budget=4000, intent=trace) ---
  Sections: identity(17tok), trace(56tok), signatures(209tok), bodies(172tok), di_wiring(15tok)
                                                                               # was "0 sections, no content"
```

Score: CleanArchProject 25 calls/3333 tok, ControllerApp 26 calls/3979 tok, 3/3 errors actionable on
both — same shape as the original audit, now with all 3 gaps closed.

## What this does NOT fix (deliberately out of scope, tracked in the master plan)

- UI Context Studio gaps (`omitted[]` not shown, no verification panel, dead `config`/`tests` card
  stubs, silent RPC-error UI, `.md`-only save extension) — R1/R4/R5/R6+ from the UI audit, all
  Angular-side or net-new features, not bugs in the engine fixed here.
- `dntsite` target-name gap and `yarp` gateway-archetype gap — pre-existing, unrelated, tracked
  separately (see master plan).
