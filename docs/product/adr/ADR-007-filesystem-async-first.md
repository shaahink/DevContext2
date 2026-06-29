# ADR-007: IFileSystem async-first

**Status**: Accepted

**Context**: Synchronous `ReadAllText` blocks thread pool threads under `Parallel.ForEachAsync`, reducing throughput on large projects.

**Decision**: `IFileSystem` is async-first for all I/O operations. Sync methods exist only for existence checks and path operations.

**Consequences**: Cooperative yielding during parallel extraction. Extractors never block thread pool threads.
