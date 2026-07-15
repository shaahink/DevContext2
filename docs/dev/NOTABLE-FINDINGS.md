# Notable Findings — surfaced during the docs/code-map survey

Observations gathered while mapping the engine on `develop` (2026-07-15). Not a work order — raw
material for a next iteration. Each item notes **what**, **where**, and **why it might matter**. None of
these were changed; the docs commit only refreshed documentation.

## A. Doc/config drift (accuracy)

1. **`configuration.md` documented a config that doesn't match the loader.** It showed
   `defaultScenario: "overview"` + `maxOutputTokens: 6000` and a `./devcontext.schema.json`. The real
   loader (`src/DevContext.Cli/Services/DevContextConfig.cs`) parses `defaultProfile, defaultScenario,
   maxOutputTokens, excludePatterns, entryPaths, profiles`. *(Refreshed in this commit.)*
2. **Three different config shapes coexist.** `devcontext.json` (root) has only `excludePatterns`;
   `devcontext.example.json` advertises `$schema: …/v2/config.json` with `defaultScenario:
   "debug-endpoint"` + a `profiles` map; `configuration.md` had a third. **Pick one canonical schema**
   and align all three (and publish the `v2/config.json` schema, which is referenced but not in-repo).
3. **`cli-reference.md` listed retired flags as active** (`--max-tokens, --token-view,
   --include-provenance, --include-anti-patterns, --metrics, --scenario, --profile, --around, --task,
   --cleanup`) and missed the real ones (`--include-map, --lite, --fast, --no-cache, --cache-only,
   --quiet`). *(Refreshed in this commit.)*
4. **`ADR-001-roslyn-separate-project.md` records a decision that's been reversed** — Roslyn is now a
   `Microsoft.CodeAnalysis.CSharp` package ref inside `DevContext.Core`, there is no `DevContext.Roslyn`
   project. ADRs are historical, so **add a "Superseded by …" note** rather than editing the decision.
5. **`FEATURE-FLOW-EXPLAINER.md` and `DESIGN-PHILOSOPHY.md`** still speak of "DevContext Desktop"
   framed around the retired WPF/Blazor app and old scenario/profile model. Worth a re-verify pass
   (lower priority — they're narrative, not reference).
6. **`TRACE-RULE-REFERENCE.md` header says "source-verified … on 2026-06-16"** — pre-Loom. It predates
   the Graph2 seam model (`ISeamDetector`/`SeamMatch`/`SymbolTable` tier ladder). Needs re-verification
   against `Graph2/Seams/` before it can be trusted as exhaustive.

## B. Half-retired surfaces (product decision needed)

7. **The token budget is retired at the CLI but still plumbed internally.** `--max-tokens` etc. are
   hidden no-op stubs (`AnalyzeSettings.cs`), yet `RenderRequest`/`ExtractionOptions` still carry
   `MaxOutputTokens`/`TokenView`/`IncludeProvenance`, `DevContextConfig.Validate()` still range-checks
   `maxOutputTokens` (100–100000), and the legacy catalog RenderPlan still consumes it. **Decide:**
   finish removing the token/scenario/profile model, or re-expose it. Right now a user who sets
   `maxOutputTokens` in `devcontext.json` gets it validated but (for Map/Trace) it has no effect —
   confusing. `BudgetIndependenceTests` already asserts Map/Trace are budget-invariant.
8. **`DevContextConfig.Validate()` rejects `defaultScenario` unless it's in `ScenarioRegistry.BuiltIn`,**
   but the example file uses `"debug-endpoint"` and the old docs used `"overview"` — at least one of
   those fails validation depending on what's actually registered. Confirm the registry contents and
   fix the example/init template accordingly.

## C. Code-level observations (not bugs, worth a look)

9. **`ResolutionTier.FileScoped` is defined but never produced.** `Graph2/ResolutionTier.cs` declares
   it; `SymbolTable.Resolve` returns `Declared/Semantic/ProjectScoped/GlobalUnique/Ambiguous/Unresolved`
   — never `FileScoped`. Either dead (remove) or an intended tier that was never wired.
10. **`EdgeKind.Exposes` and `EdgeKind.DependsOn` lack the XML-doc comments every other member has**
    (`Graph/CodeGraph.cs`). Suggests they were added later / are less exercised — worth confirming they
    have detectors, tests, and renderer handling.
11. **`loom-guards.ps1` still only *advises* on `NodeId.ForType(` and `fqns[0]` in `Graph/`** (the "L3
    will enforce" rules). `NodeId.ForType` static factories are still live in `CodeGraph.cs`. The
    planned L3 ratchet hasn't landed — track under `conductor-DEBT.md`.
12. **God-class watch:** `GraphBuilder.cs` (2461), `MarkdownRenderer.cs` (1162), `DiscoveryPipeline.cs`
    (958). `GraphBuilder` is well-sectioned (P1/P2/P3) but is the single biggest change-risk surface —
    every seam/pass lives in one file. `MarkdownRenderer` is the legacy catalog renderer; if the
    catalog is being retired with the token budget (item 7), a chunk of these 1162 lines may be dead.

## D. Things that are fine (checked, not a problem)

- **`DesktopEntryKind` (Window/Page/UserControl/AppStartup/RelayCommand)** is *not* dead post-WPF-removal
  — it's used by `Graph/EntryPoints/DesktopEntryPointBuilder.cs` to detect desktop entry points in
  *analyzed* repos (DevContext analyzing a WPF/Avalonia app), not DevContext's own retired shell.
- **The seam architecture is clean** — detectors are pure (`facts + context → SeamMatch`), never touch
  the graph; the assembler owns resolution and applies Law R1 (skip ambiguous). Good separation to keep.
- **"Token budget out of the kernel" is enforced structurally**, not just by convention — graph assembly
  runs before scoring/compression and a test locks it. Solid invariant.

## Suggested next-iteration order (my read)

1. Land the config decision (items 1, 2, 7, 8) — small, high-clarity, removes user-facing confusion.
2. Re-verify `TRACE-RULE-REFERENCE.md` + `DETECTION-GUIDE.md` against `Graph2/Seams/` (item 6).
3. Resolve the `FileScoped` tier + undocumented edge kinds (items 9, 10) — cheap correctness hygiene.
4. If the catalog/token model is truly retired, sweep the dead plumbing (items 7, 11, 12) behind a flag.
