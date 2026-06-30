# Handover — Library Surface support (FluentValidation · Polly · CommunityToolkit.Mvvm · MediatR)

> Branch `feat/library-surface-fv-polly` (15 commits ahead of `develop`, in sync with `origin`).
> Makes DevContext a first-class tool for **.NET libraries** (not just apps): a no-app-entries codebase now
> renders a ranked **library surface** instead of an empty/app Map. Built benchmark-first across four real
> libraries of different shapes; all gates green.
>
> **Resume instruction:** read this file → run the gate (`dotnet build DevContext.slnx`,
> `dotnet test --filter "Category!=Eval&Category!=CliSmoke"`, `dotnet test --filter "Category=Eval"`) → pick
> an item from **Open follow-ups** → for a new library, run the **Library onboarding loop** below.

---

## ⚠️ Branch caveat (read first)
The base commit `03c32e6 wip(desktop-redo): tauri/angular App + gRPC Server + Contracts + proto base` is an
**unrelated** Tauri/Angular desktop-redo WIP that was parked and committed here so the library work builds on
the latest code. The library commits (`ce608ff…0871a79`) are **independent of it**. For a clean library PR,
**rebase the library commits off `develop`** (drop `03c32e6`), or cherry-pick `03c32e6` back onto
`feat/desktop-redo-tauri-angular` where it belongs.

---

## What was built

### The product
- **Archetype split** (App vs Library) decides the render path. `ArchetypeDetector` → `MapBuilder` sets
  `MapModel.Surface` (a `LibrarySurface`) for libraries; `DiscoveryPipeline.RenderAsync` dispatches to
  `LibrarySurfaceRenderer` (vs `MapRenderer`).
- **The library surface** (markdown), ranked "how do I use this":
  `ENTRY API` (annotate → register → build → derive/implement → extend) · `ABSTRACTIONS` (seats by
  implementor count) · `GENERATORS` (source generators / analyzers / code fixers) · `PUBLIC SURFACE`
  (by namespace, `*.Internal`/tooling demoted, `///` doc one-liners) · `CONSUMER PATHS` · runtime `PACKAGES`.
- **Build-free**: everything is derived from syntax + `///` doc trivia (no `SemanticModel`). Inherited-from-
  external members are not enumerated.

### The engine deltas (by iteration / work package)
- **It0** (`ce608ff`): `archetype` field in JSON (`DiscoveryModel.Archetype` ← `MapModel.Archetype`,
  emitted by `JsonContextRenderer`/`DevContextOutput`); registered eval repos; scaffolded the
  `SurfaceQualityTests` tier.
- **WP1** (`1775f34`): parse-layer capture — `MethodSignature` (+`IsExtension`/`ExtendedType`/`XmlDoc`),
  `TypeDiscovery` (+`XmlDoc`); `SyntaxStructureExtractor` detects real `this`-extension methods + extracts
  `<summary>` prose.
- **WP2–7** (`9ccac72`): the ranked surface engine in `LibrarySurfaceBuilder` + `LibrarySurfaceRenderer`
  (+`LibrarySurface` record). Real extension methods, abstract seats (implementor count), `*.Internal`
  demotion, runtime-only packages (`MapBuilder.BuildPackages` got a project-scoped overload).
- **It2 Polly** (`5273d63`): the "library with sample apps" trap. `ProjectClassifier.IsSamplePath` +
  `ArchetypeDetector`/`LibrarySurfaceBuilder` exclude `samples/`/`snippets/` so a Minimal-API demo doesn't
  flip a library to App; exe/benchmark project types excluded from the surface.
- **polish** (`0b08e0f`): drop `STYLE` from the library Overview; `IsTestPath` excludes shared `test/`
  source from the surface; `build` ENTRY-API tier for `*Builder` types with a `Build()` (Polly's
  `ResiliencePipelineBuilder`).
- **It3 WP6** (`ee31bf1`): source-generator shape. `LibrarySurfaceBuilder` detects
  `IIncrementalGenerator`/`ISourceGenerator` (generator), `DiagnosticAnalyzer`/`DiagnosticSuppressor`
  (analyzer), `CodeFixProvider` (code-fixer) → `GENERATORS` section; marker attributes → `annotate` ENTRY-API
  tier (gated on the library shipping generators, so FV/Polly are unaffected); `*.SourceGenerators`/
  `*.CodeFixers` namespaces demoted; ABSTRACTIONS deduped by name.
- **refactor** (`db1761d`): one `OrderBy(Id)` on `publicTypes` → byte-deterministic surface; dropped the
  over-broad `.Analyzers` namespace match; `AttributeSuffix` const.

### Cross-cutting fixes (found via eval, not part of a WP)
- **`54cda5e` call-graph seedless fallback**: the Iteration-6 entry-scoped binding produced **zero** call
  edges for a no-entry project (a library, a small tool, the `CallGraphExtractor` unit fixture) — the
  documented "full-bind fallback when the seed set is empty" was never implemented. Restored, capped at
  `SeedlessBindFileCap = 100` files. **Fixes the long-standing `CallGraphExtractor_DiscoversBasicInvocations`
  failure.**
- **`05de28d` entry-inventory production gate** (MediatR audit): a library/app's **test-project** and
  **sample** handlers/endpoints were leaking into `snapshot.Entries` (MediatR showed **18** phantom
  entries). New `NoiseFilter.IsProductionEntrySource` (not test project · not generated · not sample path)
  gates the four `GraphBuilder.Add*EntryPoints` methods (made instance to reach `_noise`). MediatR 18 → 2.
  Uses project-based `IsInTestProject` (NOT path-based `IsTestPath`) so the `tests/fixtures/*` eval fixtures
  (not test projects) keep their entries.
- **`0871a79` desktop HTML parity**: `NarrativeHtmlConverter.IsSectionHeader` now recognizes the five
  library headers, so the WPF **Human (HTML)** view styles them as `<h3>` (CLI + LLM markdown already did).

---

## Where things live (key files)

| Area | File |
|---|---|
| Library surface model | `src/DevContext.Core/Graph/LibrarySurface.cs` (`SurfaceEntry`/`SurfaceAbstraction`/`SurfaceGenerator`/`SurfaceGroup`/`SurfaceType` + the `LibrarySurface` sections) |
| Surface builder (the brain) | `src/DevContext.Core/Graph/LibrarySurfaceBuilder.cs` |
| Surface renderer (markdown) | `src/DevContext.Core/Rendering/LibrarySurfaceRenderer.cs` |
| Archetype + sample/test scoping | `src/DevContext.Core/Graph/ArchetypeDetector.cs`, `NoiseFilter.cs` (`ProjectClassifier.IsSamplePath`/`IsTestPath`, `NoiseFilter.IsProductionEntrySource`) |
| Entry inventory | `src/DevContext.Core/Graph/GraphBuilder.cs` (the 4 `Add*EntryPoints`, gated on `_noise.IsProductionEntrySource`) |
| Packages (project-scoped) | `src/DevContext.Core/Graph/MapBuilder.cs` (`BuildPackages(IEnumerable<ProjectInfo>)`) |
| Parse: docs + extensions | `src/DevContext.Core/Extractors/Generic/SyntaxStructureExtractor.cs`, `Models/MethodSignature.cs`, `Models/TypeDiscovery.cs` |
| JSON archetype | `Models/DiscoveryModel.cs`, `Models/DevContextOutput.cs`, `Rendering/JsonContextRenderer.cs`, `Pipeline/DiscoveryPipeline.cs` |
| Call-graph seedless fallback | `src/DevContext.Core/Extractors/Specific/CallGraphExtractor.cs` |
| WPF HTML section headers | `src/DevContext.Core/Rendering/NarrativeHtmlConverter.cs` |
| Tests | `tests/DevContext.Core.Tests/{SurfaceQualityTests,NarrativeHtmlConverterTests,SyntaxStructureExtractorTests}.cs`, `GraphBuilderTests.cs` (`NoiseFilter_IsProductionEntrySource_*`), `RendererTests.cs` (archetype) |
| Benchmarks/audits/gates | `eval-results/{FluentValidation,Polly,CommunityToolkit.Mvvm,MediatR}/{BENCHMARK,AUDIT,EVAL,map*}.md`, `eval/expectations/{fluentvalidation,polly,communitytoolkit}.json`, `eval-repos.json`, `eval/README.md` |
| North-star docs | `docs/product/IDEAL-OUTPUT-TARGET.md` §4, `docs/product/ACCEPTANCE.md` (ripgrep Library row) |

---

## The four libraries (shapes proven)

| Library | Shape | Result | Gate |
|---|---|---|---|
| **FluentValidation** | abstract-seat + extension-DI + fluent DSL | `register AddValidatorsFromAssembly` → `derive AbstractValidator` → `extend DefaultValidatorExtensions` | `eval/expectations/fluentvalidation.json` (12) ✅ |
| **Polly** | fluent builder + options + "samples aren't the library" trap | `build ResiliencePipelineBuilder`; runtime-only packages | `polly.json` (13) ✅ |
| **CommunityToolkit.Mvvm** | source generators + analyzers + marker attributes | `annotate [ObservableProperty]/[RelayCommand]` + `GENERATORS` (ObservablePropertyGenerator…) | `communitytoolkit.json` (11) ✅ |
| **MediatR** | the handler-pattern library itself | archetype Library; entries 18→2 after the inventory fix | ad-hoc (`eval-results/MediatR/EVAL.md`) — **no gate yet** |

---

## The Library onboarding loop (extensible procedure — repeat per library)
Self-wiring: no harness changes; `EvalExpectationTests` auto-discovers `eval/expectations/*.json`,
`SurfaceQualityTests` is the durable regression tier.
1. **Register** — `eval-repos.json` + pinned SHA in `eval/README.md`; clone to `eval-repos/<Lib>` (gitignored).
2. **Capture** — `dotnet <Cli.dll> analyze eval-repos/<Lib>` (md) + `--format json` → `eval-results/<Lib>/map*`.
3. **Benchmark** — hand-author `eval-results/<Lib>/BENCHMARK.md` from a source read (build-free; only assert
   what syntax + `///` can know).
4. **Audit** — `eval-results/<Lib>/AUDIT.md` gap scorecard (capture ↔ benchmark).
5. **Gate (aspirational)** — `eval/expectations/<lib>.json` (`SCHEMA.md` check types), status `aspirational`.
6. **Engine deltas** → re-capture → flip gates `aspirational→expected` (same commit) → add a
   `SurfaceQualityTests` `[InlineData]`.

---

## Verification (all green)
- Full solution: `dotnet build DevContext.slnx` → **0 warnings / 0 errors**.
- Fast suite: `dotnet test tests/DevContext.Core.Tests --filter "Category!=Eval&Category!=CliSmoke"` →
  **282 pass / 0 fail / 2 skip** (the 2 skips are `GoldenExtractionTests.*_ProducesJson`).
- Eval suite: `dotnet test tests/DevContext.Core.Tests --filter "Category=Eval"` → **27/27** (apps + 3 library
  gates + traces). Or `pwsh -File eval/gates.ps1`.

---

## Honest caveats / known limits
- **Desktop-redo base** `03c32e6` rides along — see the branch caveat at the top.
- **Build-free trade-off**: inherited-from-external members not enumerated; `ABSTRACTIONS` implementor counts
  **include test/sample derivers** (inflated, e.g. AbstractValidator 52 / IRequestHandler 44) — deliberately
  kept because it ranks the *primary consumer seat* correctly (a library's main seat is consumer-derived, so
  in-repo-only counts under-represent it).
- **MediatR residual**: 2 `src/MediatR` `INotificationHandler` types still count as entries (real src types,
  not phantom test/sample noise).
- **Angular**: the gRPC `Proto.LibrarySurface` is **stale** — `ProtoMapper.ToMapResponse` maps only
  `Groups`/`ExtensionPoints`, not the new `EntryApi`/`Abstractions`/`Generators`/`ConsumerPaths`/`Doc`. The
  Angular **Markdown** view shows everything (via `MapResponse.markdown`); the structured **Overview** view
  does not. WPF HTML header styling is now fixed (`0871a79`).
- **Pre-existing**: nothing outstanding — the call-graph env failure is fixed (`54cda5e`).

---

## Open follow-ups
1. **MediatR gate** — add `eval/expectations/mediatr.json` (archetype=Library + `AddMediatR` + seats) and a
   `SurfaceQualityTests` `[InlineData("eval-repos/MediatR")]` for automated regression coverage.
2. **Angular structured surface** — extend `proto/devcontext/v1/devcontext.proto`
   (`LibrarySurface`: +entryApi/abstractions/generators/consumerPaths; `SurfaceType`: +doc), regenerate
   (`pnpm gen:proto` + Grpc.Tools), map them in `ProtoMapper.ToMapResponse`, render in the Angular
   Overview. Needs the desktop toolchain (Node/pnpm/buf).
3. **Optional refinements** — drop the 2 residual MediatR entries (e.g. suppress entries entirely for a
   Library archetype); exclude test/sample derivers from `ABSTRACTIONS` counts **only if** the primary seat
   still surfaces.
