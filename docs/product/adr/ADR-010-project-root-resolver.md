# ADR-010: ProjectRootResolver as pre-pipeline step

**Status**: Accepted

**Context**: Root resolution is complex (walk-up/down, .sln vs .csproj vs folder mode). Embedding it in the pipeline would couple resolution logic with extraction.

**Decision**: `ProjectRootResolver` runs before `DiscoveryContext` is built. It is independently testable and the dry-run can show how the root was found.

**Consequences**: Clean pipeline. Resolution result is immutable input. Dry-run shows resolution method and notes.
