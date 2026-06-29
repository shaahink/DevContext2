# CommunityToolkit.Mvvm — Library Surface AUDIT (current CLI ↔ benchmark)

> Gap scorecard for the MVVM Toolkit. The generic library engine (It1/It2) already nailed
> archetype=Library + ENTRY API / ABSTRACTIONS / PUBLIC SURFACE, but the **source-generator identity was
> invisible**: generators/analyzers/fixers were buried in PUBLIC SURFACE as plain classes, and the marker
> attributes (`[ObservableProperty]`, `[RelayCommand]`) — the actual consumer API — were buried
> alphabetically. WP6 closed these. Methodology per the `devcontext-eval-audit` skill.

**Verdict:** after WP6 (source-gen detection + `annotate` tier + tooling demotion + abstraction dedup),
CommunityToolkit.Mvvm renders a faithful source-gen surface; all gates green.

## Scorecard

| # | Dimension | Target | Pre-fix (`map-v2.md`) | Verdict | Fix |
|---|---|---|---|---|---|
| 1 | Archetype | `Library` | `LIBRARY dotnet (136 public types)` | ✅ | generic (It2) |
| 2 | ENTRY API marker attrs | `[ObservableProperty]`/`[RelayCommand]` lead | attributes buried in PUBLIC SURFACE; ENTRY API led with extension helpers | ❌→✅ | `annotate` tier (gated on shipping generators) |
| 3 | GENERATORS | source generators + analyzers + fixers surfaced | buried as plain classes (`ObservablePropertyGenerator` with member `Initialize`) | ❌→✅ | detect `IIncrementalGenerator`/`DiagnosticAnalyzer`/`DiagnosticSuppressor`/`CodeFixProvider` → `GENERATORS` section |
| 4 | ABSTRACTIONS | deduped | `ObservableRecipient (class) — 5 implementors` listed **twice** | ⚠→✅ | dedup by name |
| 5 | PUBLIC SURFACE | tooling demoted | `*.SourceGenerators` (incl. generator `.Models` records) + `*.CodeFixers` cluttered the surface | ⚠→✅ | demote `*.SourceGenerators`/`*.CodeFixers`/`*.Analyzers` namespaces; extract tooling types to GENERATORS |
| 6 | CONSUMER PATHS | annotate recipes | present (extend/contract only) | ✅→✅ | `annotate → [X] on a partial class/member` recipe |

## Defects → fixes (engine deltas, WP6)

- **D1 (source-gen detection).** `GeneratorKind(t)` — build-free, by interface/base/attribute:
  `IIncrementalGenerator`/`ISourceGenerator` → `generator`; `DiagnosticAnalyzer`/`DiagnosticSuppressor` →
  `analyzer`; `CodeFixProvider` → `code-fixer`. Rendered in a `GENERATORS` section.
- **D2 (marker attributes).** `IsMarkerAttribute` (public `*Attribute` class) → `annotate` ENTRY-API tier,
  **gated on the library actually shipping generators** (so non-source-gen libs are unaffected — verified
  FluentValidation/Polly produce 0 `annotate`/`GENERATORS`).
- **D3 (tooling demotion).** `IsToolingNamespace` (`*.SourceGenerators`/`*.CodeFixers`/`*.Analyzers`) folds
  those namespaces into the demoted/internal bucket; tooling types move to GENERATORS.
- **D4 (abstraction dedup).** `BuildAbstractions` dedups by type name.

## Post-fix re-audit

Re-captured as `map-v3.md`. All 6 defects closed; `eval/expectations/communitytoolkit.json` all `expected`
and green (`EvalRepo_MatchesExpectations(communitytoolkit)` + `SurfaceQualityTests` on the real clone).
ENTRY API leads with 8 `annotate` markers; `GENERATORS` lists 7 source generators + ~15 analyzers/suppressors
+ 3 code fixers; `ObservableRecipient` appears once in ABSTRACTIONS.

## Minor notes (non-blocking)

- The repo is the `CommunityToolkit/dotnet` **monorepo**, so the surface legitimately also shows
  `CommunityToolkit.Common` / `.Diagnostics` / `.HighPerformance` (not just MVVM). Header reads
  `LIBRARY dotnet` (the solution name).
- A couple of `global`-namespace artifacts (`INotifyPropertyChanged`, `ObservableRecipient` from
  no-namespace generator-template files) remain in PUBLIC SURFACE — 2 types, cosmetic.
