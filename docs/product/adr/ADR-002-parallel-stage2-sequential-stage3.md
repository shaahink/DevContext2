# ADR-002: Parallel Stage 2, sequential Stage 3

**Status**: Accepted

**Context**: Generic extractors in Stage 2 read independent inputs (filesystem, XML) and write to thread-safe collections. Specific extractors in Stage 3 read the sealed signal registry and may cross-reference model data.

**Decision**: Stage 2 runs via `Parallel.ForEachAsync` for throughput. Stage 3 runs sequentially to avoid races without locking overhead.

**Consequences**: Stage 2 extractors must be provably non-conflicting. Stage 3 extractors can safely read any model state without synchronization.
