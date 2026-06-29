# Polly — Library Surface AUDIT (current CLI ↔ benchmark)

> Gap scorecard for Polly. Unlike FluentValidation, the **generic library engine (It1) produced nothing
> useful at first** because Polly was *misclassified as an App* — its `samples/Chaos` Minimal-API demo made
> the archetype detector think Polly is a web app. This is the "library with sample apps" trap, and the
> headline It2 finding. Methodology per the `devcontext-eval-audit` skill.

**Verdict:** after the It2 sample/benchmark-exclusion fix, Polly renders a faithful library surface and all
gates are green. Two cosmetic follow-ups remain (a stray test type; the `STYLE` line).

## Scorecard

| # | Dimension | Target | Pre-fix (`map-v2.md`) | Verdict | Fix |
|---|---|---|---|---|---|
| 1 | Archetype | `Library` | **`App` / `MAP Polly` / `STYLE MinimalApi`** — `samples/Chaos/Program.cs` (`Sdk.Web`, `app.MapGet`) → 77 EndpointDetections → app entries | ❌→✅ | `ProjectClassifier.IsSamplePath`; `ArchetypeDetector` ignores sample-path entries + projects |
| 2 | ENTRY API | ranked | n/a (App) → `register AddResiliencePipeline` / `derive ResilienceStrategy` / `extend *Syntax` | ✅ | generic engine (It1 WP3) |
| 3 | ABSTRACTIONS | seats | n/a → Policy/AsyncPolicy (24), ResilienceStrategy (22), … | ✅ | generic (It1 WP2) |
| 4 | PUBLIC SURFACE | clean | `Polly.Benchmarks` / `Polly.Core.Benchmarks` types leaked (216 types) | ⚠→✅ | exclude exe/benchmark-project types (`NonLibraryProjectDirs`); 188 types |
| 5 | PACKAGES | runtime | `Refit`, `RestSharp`, `Flurl.Http`, `xunit`, `NSubstitute` leaked (samples/tests) | ❌→✅ | sample-path + exe + test scoping in `BuildRuntimePackages` |
| 6 | CONSUMER PATHS | present | n/a → wire-into-DI / derive | ✅ | generic (It1 WP7) |

## Defects → fixes (engine deltas, It2)

- **D1 (archetype, the headline).** `ProjectClassifier.IsSamplePath(filePath)` (samples/snippets/examples/demos).
  `ArchetypeDetector` now (a) excludes sample-path **entries** from the app-entry check, (b) excludes
  sample-path **projects** from `nonTest`, (c) excludes sample-path types from `hasPublicSurface`. A library
  with a Minimal-API sample no longer flips to App.
- **D2 (benchmark/exe types in surface).** `LibrarySurfaceBuilder.NonLibraryProjectDirs` collects exe /
  benchmark / sample project directories and excludes their types from the surface — a library's surface is
  its *library* projects.
- **D3 (sample/test packages).** `BuildRuntimePackages` now also drops sample-path projects (on top of
  test/exe/benchmark), leaving runtime deps only.

## Post-fix re-audit

Re-captured as `map-v3.md` (188 public types). `archetype=Library`; ENTRY API / ABSTRACTIONS / PUBLIC SURFACE
(docs) / CONSUMER PATHS / runtime-only PACKAGES all present; benchmark types + sample packages gone. All
`eval/expectations/polly.json` checks `expected` and green.

## Minor follow-ups (non-blocking, noted for later)

- `Polly.Tests.StrongNameTests` still appears — a test type whose project isn't caught by
  `ProjectClassifier` (no `*Tests` suffix / test-package marker on that project). One type.
- `STYLE  MinimalApi` on the library header — the Chaos sample's Minimal-API **signal** still reaches the
  style detector (separate from archetype). Cosmetic; consider suppressing style for libraries.
- The v8 `ResiliencePipelineBuilder` (primary fluent builder) is in PUBLIC SURFACE but not promoted to a
  top ENTRY API "build" tier; the v7 `*Syntax` classes dominate the `extend` tier. A "key-builder" ranking
  tier would sharpen the v8 story.
