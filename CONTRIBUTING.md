# Contributing to DevContext

## Getting Started

```bash
# Clone the repo
git clone https://github.com/shaahink/DevContext2.git
cd DevContext2

# Build
dotnet build

# Run tests
dotnet test tests/DevContext.Core.Tests
dotnet test tests/DevContext.Desktop.Tests

# Run against a project
dotnet run --project src/DevContext.Cli -- analyze ../MyProject --scenario architecture

# Run the desktop app
dotnet run --project src/DevContext.Desktop

# Check formatting
dotnet format --verify-no-changes
```

## Project Structure

```
src/
  DevContext.Core/     # Core library: contracts, extractors, pipeline, pruning, rendering
  DevContext.Roslyn/   # Roslyn workspace integration (loaded on demand)
  DevContext.Cli/      # CLI composition root: commands, DI wiring, observers
  DevContext.Desktop/  # Avalonia desktop app: direct engine, ViewModels, control themes
tests/
  DevContext.Core.Tests/     # Unit + golden tests (~144 tests)
  DevContext.Desktop.Tests/  # ViewModel + XAML headless tests (~33 tests)
  DevContext.Roslyn.Tests/   # In-memory Roslyn compilation tests
  DevContext.Integration/    # End-to-end against real repos
```

## Making Changes

1. Create a branch from `develop`: `git checkout -b feature/your-feature develop`
2. Make changes, following existing code conventions
3. Ensure `dotnet test` passes (176+ tests)
4. Update golden tests if output format changed: `$env:UPDATE_GOLDENS = "1"; dotnet test`
5. Ensure `dotnet format --verify-no-changes` passes
6. Push and open a PR against `develop`

## Code Conventions

- All public APIs must have XML documentation
- Async methods should use `ConfigureAwait(false)` and pass `CancellationToken`
- Extractors must implement `IDiscoveryExtractor` interface
- Use `model.AddDiagnostic()` for expected failures, never throw
- Signals are sealed between Stage 2 and Stage 3 — register signals only in Generic extractors

## Adding a New Extractor

1. Create a new file in `src/DevContext.Core/Extractors/Specific/` or `Generic/`
2. Implement `IDiscoveryExtractor` with `Name`, `Tier`, `Category`, `Capabilities`, `ShouldRun`, `ExtractAsync`
3. Decorates with `[ExtractorOrder(n)]` for ordering within tier
4. Add any new `Detection` record to `src/DevContext.Core/Models/Detections.cs`
5. Add rendering in `src/DevContext.Core/Rendering/MarkdownRenderer.cs`
6. Add unit tests for the extractor
7. Run the full test suite
