# L7.4 — 22-Repo Bench Verdicts (2026-07-08 session #38)
## Gate battery

| Gate | Result |
|------|--------|
| `dotnet build DevContext.slnx` | 0w 0e |
| `dotnet test --filter "Category!=Eval"` | Core 414P/3S, Server 12P, Desktop 64P |
| `pnpm check` | lint + test(27/27) + build |

## Per-repo verdicts

| # | Repo | Status | Style | Nodes | Edges | Time | Archetype | Verdict |
|---|------|--------|-------|-------|-------|------|-----------|---------|
| 1 | DntSite | SKIP | — | — | — | — | ControllerBased | Not cloned |
| 2 | TodoApi | OK | MinimalApi | 123 | 81 | 5.4s | App | Baseline stable |
| 3 | CleanArchitecture | OK | VerticalSlices | 331 | 203 | 10.3s | App | Baseline stable |
| 4 | eShop | OK | Microservices | 1154 | 1001 | 23.3s | App | Baseline stable |
| 5 | FluentValidation | OK | Unknown | 1163 | 1057 | 10.1s | Library | Library archetype — honest Unknown style |
| 6 | Polly | OK | MinimalApi | 2101 | 2045 | 27.1s | Library | Library with sample apps |
| 7 | CommunityToolkit.Mvvm | OK | Unknown | 744 | 534 | 15.1s | Library | Library archetype |
| 8 | MediatR | OK | SampleCollection | 489 | 417 | 8s | Library | Multi-sample dir (L7.4 fix) |
| 9 | gRPC | OK | SampleCollection | 1704 | 1495 | 24.4s | Library | Multi-sample dir (L7.4 fix) |
| 10 | MassTransit | OK | Microservices | 13914 | 13151 | 75.4s | Library | Large framework, Aspire signal |
| 11 | Ocelot | OK | SampleCollection | 2048 | 1873 | 41.8s | App | Multi-sample dir (L7.4 fix) |
| 12 | AzureFunctions | OK | Unknown | 1239 | 1017 | 24.3s | Library | Worker archetype target |
| 13 | RazorPages | OK | SampleCollection | 1179 | 593 | 106.5s | App | Multi-sample dir (L7.4 fix) |
| 14 | CLI | OK | Unknown | 892 | 1172 | 13.4s | App | Baseline stable |
| 15 | Blazor | OK | SampleCollection | 50 | 29 | 15.5s | App | WASM sample — NOW SampleCollection (L7.4 fix, was Microservices) |
| 16 | Desktop | OK | Unknown | 300 | 103 | 7.2s | App | Avalonia samples |
| 17 | PowerToys | OK | Unknown | 7647 | 5817 | 56.6s | App | Desktop archetype, large WPF+WinUI repo |
| 18 | Serilog | OK | Unknown | 414 | 308 | 11.5s | Library | Library archetype |
| 19 | Spectre.Console | OK | Unknown | 1074 | 835 | 19.4s | Library | CLI library archetype |
| 20 | MassTransit-Sample | OK | Unknown | 15 | 10 | 3.9s | App | Small sample (15 nodes) |
| 21 | eshop-microservices | OK | Microservices | 436 | 338 | 6.5s | App | Dogfood — stable at 436n/338e/34e/6SL/69% |
| 22 | DevContext | OK | CleanArchitecture | 1278 | 1231 | 51.4s | App | Self-analysis |

## Key observations

- **Blazor (repo #15):** Style flipped from Microservices → SampleCollection thanks to L7.4
  multi-.sln directory detection (50 nodes, 29 edges). The `[TruthPending("L7")]` ratchet
  in `TruthExpectationTests.Blazor_archetype_is_not_microservices` has been flipped to
  `[SkippableFact]` with `Skip.IfNot` guard.
- **MediatR, gRPC, Ocelot, RazorPages:** Also now detected as SampleCollection via the
  multi-.sln mismatch check — these repos have many nested .sln files (docs/samples/tests)
  but the resolver walked down to a single small .sln.
- **Library repos (FluentValidation, Polly, CommunityToolkit, MediatR, Serilog, Spectre):**
  All produce honest graphs with non-trivial public surfaces. No false App detections.
- **Desktop archetype (PowerToys):** 7647 nodes, 5817 edges — large WPF repo, honest Unknown
  style (no microservice signals). Desktop projection surfaces from L7.2 applicable.
- **No regressions on dogfood:** eshop-microservices stays at 436n/338e/34e/6SL/69%.
- **DntSite:** Not cloned on this machine (localPath not found).

## L7.4 code changes

1. `ArchitectureStyleDetector.cs` — multi-.sln directory detection: when the resolver finds
   a single small .sln but the analyzed project set is >5x larger, score SampleCollection
   (catches blazor-samples, aspnetcore-docs, gRPC samples, etc.)
2. `ArchitectureStyleDetectorTests.cs` — 2 new tests for multi-.sln detection, 1 regression
   test for normal repos
3. `TruthExpectationTests.cs` — flipped Blazor test from `[TruthPending("L7")]` to active
   `[SkippableFact]`; added 3 new truth tests for Library (FluentValidation), Desktop
   (PowerToys), and Worker (AzureFunctions) archetypes

## Evidence

- `eval-results/2026-07-08/bench-summary.txt` — 22-repo bench summary (this run)
- `eval-results/2026-07-08/*-report.md` — individual repo reports (21 total)
- `eval-results/2026-07-08/gate-battery-l7.4-s38.txt` — gate battery results
