# FluentValidation — Library Surface AUDIT (current CLI ↔ benchmark)

> Gap scorecard comparing today's CLI capture (`map-v2.md` / `map-v2.json`, engine
> `v1.0.5-preview.0.132`) against `BENCHMARK.md`. Drives the aspirational gates in
> `eval/expectations/fluentvalidation.json` and the WP deltas. Methodology per the
> `devcontext-eval-audit` skill.

**Verdict:** archetype routing is correct (✅ `LIBRARY` surface, not an app Map), but the surface is a
**flat alphabetical dump** — no ranking, no docs, no abstractions, name-prefix-only extension detection.
A consumer cannot tell "the 4 things you touch" from "107 public types." This is the gap the library
engine deltas close.

## Scorecard

| # | Dimension | Benchmark target | Current (`map-v2.md`) | Verdict | Root cause | WP |
|---|---|---|---|---|---|---|
| 1 | Archetype | `Library` | `LIBRARY FluentValidation (107 public types)`; JSON `"archetype":"Library"` | ✅ | `ArchetypeDetector` + It0 JSON field | — |
| 2 | ENTRY API (ranked) | 4 ranked front doors w/ `///` + `file:line` | absent — alpha list of 107 types | ❌ | no ranked `PublicApi` entry inventory for libraries | WP3 |
| 3 | Real extension methods | `DefaultValidatorExtensions` (`this IRuleBuilder<T,TProperty>`) surfaced as the rule DSL | `EXTENSION POINTS` uses name-prefix → **misses** `DefaultValidatorExtensions` (`.NotEmpty()` etc.), **includes** `ValidationTestExtension.With*` test noise | ❌ | name-prefix heuristic, not `this`-param detection | WP2 |
| 4 | `///` doc summaries | per type/member one-liner | bare member names only | ❌ | doc-comment trivia not parsed | WP1 |
| 5 | Abstractions / seats | `AbstractValidator<T>` / `IValidator<T>` / `IPropertyValidator<>` highlighted | buried alphabetically (e.g. `AbstractValidator` between `AssemblyScanner` + `AsyncValidator…Exception`) | ❌ | no seat detection / `Inherits`/`Implements` modeling | WP2 |
| 6 | Internal-by-convention | `FluentValidation.Internal` collapsed/demoted | dumped first-class (`AccessorCache`, `PropertyChain`, `RuleComponent`, …) | ⚠ | surface includes all `public`; no internal-namespace demotion | WP4 |
| 7 | TestHelper grouping | grouped as consumer-test support | mixed into surface + extension points | ⚠ | no test-helper classification | WP4 |
| 8 | Packages | runtime deps only | includes `Bogus`, `xunit`, `BenchmarkDotNet`, `Microsoft.NET.Test.Sdk` | ⚠ | package list not scoped to non-test/non-benchmark projects | WP4 |
| 9 | CONSUMER PATHS | present | absent | ❌ | not built | WP7 |
| 10 | Determinism | byte-identical re-run | (to verify) | ⬜ | — | WP8 |

## Defects → fixes

- **D1 (ENTRY API, WP3).** Add `GraphBuilder.AddLibraryEntryPoints`: rank public API into front doors —
  (a) real extension methods on framework seats (`this IServiceCollection`/`IApplicationBuilder` + verb),
  (b) abstract/interface derive-seats by implementor count, (c) high-fan-in public types. Render an
  `ENTRY API` section above `PUBLIC SURFACE`.
- **D2 (extension methods, WP2).** Detect `this`-param extension methods in `SyntaxStructureExtractor`
  (`MethodSignature.IsExtension` + `ExtendedType`); classify by extended type. Replace the
  `ExtensionVerbs` name-prefix heuristic in `LibrarySurfaceBuilder`. Fixes both the miss
  (`DefaultValidatorExtensions`) and the noise (`ValidationTestExtension`).
- **D3 (docs, WP1).** Parse leading `DocumentationCommentTrivia` → `TypeDiscovery.XmlDoc` /
  `MethodSignature.XmlDoc`; render `<summary>` one-liners.
- **D4 (seats, WP2).** Tag abstract classes/interfaces with implementors as `abstract-seat`; add
  `Inherits`/`Implements` edges; surface an `ABSTRACTIONS` section.
- **D5 (internal/test/packages, WP4).** Demote `*.Internal` namespaces; group `*.TestHelper`; scope
  `PACKAGES` to non-test/non-benchmark projects.
- **D6 (consumer paths, WP7).** Derive `CONSUMER PATHS` recipes from the ranked entries + seats.

## Post-fix re-audit (engine deltas WP1–WP7 landed)

Re-captured as `map-v3.md`. All 6 defects closed; all 12 gates in `eval/expectations/fluentvalidation.json`
are now `expected` and green (`EvalExpectationTests.EvalRepo_MatchesExpectations(fluentvalidation)` + both
`SurfaceQualityTests` pass on the real clone):

| Defect | Status | Evidence (`map-v3.md`) |
|---|---|---|
| D1 ENTRY API ranked | ✅ | `register AddValidatorsFromAssembly*` → `derive AbstractValidator` → `extend DefaultValidatorExtensions`, each with `///` summary |
| D2 real extension methods | ✅ | `DefaultValidatorExtensions` (`this IRuleBuilder`) now surfaced as `extend`; verb-prefix noise no longer the sole signal |
| D3 `///` docs | ✅ | one-liners throughout ENTRY API + PUBLIC SURFACE |
| D4 abstractions/seats | ✅ | `ABSTRACTIONS` ranks AbstractValidator (52), IPropertyValidator (12), … by implementor count |
| D5 internal/test/packages | ✅ | `.Internal` → `INTERNAL (15 types … available on request)`; PACKAGES = DI.Abstractions + System.* only (**no Bogus/xunit/BenchmarkDotNet**) |
| D6 consumer paths | ✅ | `CONSUMER PATHS`: wire-into-DI / derive / implement recipes |

Minor follow-ups (non-blocking): abstraction implementor counts include test-project derivers (inflates
AbstractValidator's 52); `*.TestHelper` not yet grouped separately; entry locations are file-only (no line).

## Notes (out of scope / pre-existing)

- **CLI JSON purity:** `--format json` stdout is wrapped by a status preamble (`Overview map (no focus).`)
  and a trailing stats banner, so a piped capture isn't valid JSON. Pre-existing, affects all formats; the
  machine gate calls `RenderAsync` directly (pure JSON) so it's unaffected. Tracked separately.
- **Call graph empty (32 edges):** the semantic `CSharpCompilation` returns no edges in this environment
  (same root cause as the pre-existing `CallGraphExtractor_DiscoversBasicInvocations` unit failure). Does
  not affect the library surface (built from public types, not call edges).
