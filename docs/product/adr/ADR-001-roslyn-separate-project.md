# ADR-001: Roslyn in a separate project

**Status**: Accepted

**Context**: Roslyn assemblies are large and slow to load. Fast-only runs (Quick/Focused profiles) should never pay the cost of loading Roslyn.

**Decision**: `DevContext.Core` references only `Microsoft.CodeAnalysis.CSharp` (syntax parsing only, no workspace). `DevContext.Roslyn` references Core + workspace APIs and is loaded at runtime via the `IRoslynWorkspaceProvider` abstraction.

**Consequences**: CLI is the composition root that wires Roslyn. Fast-only runs skip the Roslyn assembly load entirely. The abstraction (`IRoslynWorkspace`/`IRoslynWorkspaceProvider`) supports testing with fakes.
