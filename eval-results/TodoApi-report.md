# Evaluation Report: davidfowl/TodoApi

**Date**: 2026-06-05
**Profile**: focused / architecture
**Max tokens**: 8000
**Total time**: 1676ms

## Summary

| Metric | Value |
|---|---|
| Projects discovered | 7 |
| Types found | 43 |
| Types in output | 32 (25% pruned) |
| Total detections | 38 |
| Architecture style | MinimalApi (100%) |
| Signals | minimal-apis |

## Detections Breakdown

| Category | Count | Details |
|---|---|---|
| DI Registrations | 33 | Singleton: 3, Scoped: 3, Extension: 27 |
| Middleware | 5 | UseHttpLogging, UseRateLimiter, UseWebAssemblyDebugging, UseHsts, UseHttpsRedirection, UseAntiforgery |

## Signals Detected vs Expected

| Signal | Expected | Found | Match |
|---|---|---|---|
| minimal-apis | ✓ | ✓ (100%) | ✅ |
| efcore | ? | ✗ | Expected -- TodoApi uses SQLite |
| mediatr | ✗ | ✗ | ✅ |

## Performance (top extractors)

| Extractor | Time | % of total |
|---|---|---|
| SyntaxStructureExtractor | 1099ms | 65% |
| DiRegistrationExtractor | 1098ms | 65% |
| ProgramCsFlowExtractor | 246ms | 15% |
| ProjectStructure | 130ms | 8% |

## Notes

- **Endpoint detection gap**: EndpointExtractor (4ms) found no endpoints. The TodoApi defines endpoints via `TodoApi.TodoApiExtensions` extension methods with `MapGroup`. EndpointExtractor only looks for `app.MapGet`/`app.MapPost` etc. directly in Program.cs. Should be enhanced to follow extension method calls.
- **DiRegistrationExtractor**: Effectively duplicates SyntaxStructureExtractor time (1098ms vs 1099ms) because both walk all syntax trees independently. Consider sharing parsed results.
- **Middleware pipeline**: Correctly detected with sequential ordering (1-based per file).
- **Pruning**: 25% pruned, reasonable for focused profile.
- **Output**: 4113 tokens well within budget, clean JSON schema.
