# Evaluation Notes

This document captures discoveries from running DevContext against real-world .NET repositories.
It serves as a living record of what patterns the tool handles well, where gaps exist, and how the
implementation has evolved to address real codebase idioms.

## Evaluation Repositories

| Repo | Pinned SHA | Patterns | Signals Expected |
|---|---|---|---|
| davidfowl/TodoApi | latest | Minimal API, MapGroup, extension methods | minimal-apis, aspire |
| ardalis/CleanArchitecture | latest | Clean Architecture, MediatR, EF Core | mediatr, efcore, controllers |
| dotnet/eShop | latest | Aspire, microservices, MediatR, EF Core, Blazor | aspire, mediatr, efcore, blazor |
| VerticalSlice (sample) | latest | FastEndpoints, vertical slice | fast-endpoints |
| AutoMapper | latest | Library (no endpoints), heavy test noise | (minimal signals) |

## Key Discoveries

### Endpoint Detection Gaps (Resolved in Iteration 1)

**Problem**: EndpointExtractor only scanned files named `Program.cs` for direct `app.MapGet(...)` calls.
This missed:
- Extension methods containing Map* calls (TodoApi's `MapTodoApi()` pattern)
- MapGroup + chained maps on group builders
- FastEndpoints classes with `[HttpGet]` attributes
- Routes defined in `Startup.cs` or other files

**Resolution**: Rewrote EndpointExtractor with 3-phase scan:
1. Direct Map* calls in all files
2. Extension method bodies taking `IEndpointRouteBuilder`/`WebApplication`/`RouteGroupBuilder`
3. FastEndpoints classes inheriting from `Endpoint<TRequest, TResponse>` with HTTP verb attributes
9 regression tests covering all patterns.

### Test/Internal Noise (Resolved in Iteration 1)

**Problem**: Output from library repos (AutoMapper) and solutions with test projects showed
heavy test-class pollution in "Related types" sections.

**Resolution**: PatternRelevancePruner now detects test noise by:
- Name patterns: `*Tests`, `*Fixture`, `When_*`, `*Mock`, `*Stub`
- Namespace segments: `*.Tests`, `*.UnitTests`, `*.IntegrationTests`
- File paths: `\\Tests\\`
- Project names ending in `Tests`, `Test`, `Specs`
Boost rules expanded for DI registrations, background workers, and message consumers.

### Performance Observations

Generic extractors (SyntaxStructure, DiRegistration, ProgramCsFlow) all walk syntax trees
independently. Since they run in parallel (Stage 2), wall time is bounded by the slowest.
IAnalysisCache prevents re-parsing but DescendantNodes filtering is per-extractor.

### Architecture Classification

Initial heuristics were too simple (only path-based). Added:
- Project name fallback when path classification fails
- Package-based classification for FastEndpoints, Aspire, MediatR, EF Core
- ArchitectureStyleDetector now scores multiple styles and picks the best match

## Running the Evaluation Harness

```powershell
# Clone repos (first time)
.\scripts\evaluate-repos.ps1

# Run with standard token budget
dotnet run --project src\DevContext.Cli -- analyze <repo-path> --profile focused --scenario architecture --max-tokens 8000 --metrics

# Run with high token budget (surfaces irrelevant types for pruning analysis)
dotnet run --project src\DevContext.Cli -- analyze <repo-path> --profile focused --scenario architecture --max-tokens 50000 --metrics

# Dry-run with validation warnings
dotnet run --project src\DevContext.Cli -- analyze <repo-path> --dry-run
```
