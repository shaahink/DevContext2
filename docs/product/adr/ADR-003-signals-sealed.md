# ADR-003: Signals sealed between Stage 2 and 3

**Status**: Accepted

**Context**: `ShouldRun` for Specific extractors must see a complete signal picture before deciding whether to execute.

**Decision**: `ArchitectureSignals.Seal()` is called after `Parallel.ForEachAsync` joins, guaranteeing all Generic extractors have committed before any Specific extractor reads. Writing after seal throws `InvalidOperationException`.

**Consequences**: Signal writes are provably complete by the time Specific extractors evaluate. Bugs (writing after seal) are immediately visible.
