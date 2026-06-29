# ADR-009: Compression after pruning

**Status**: Accepted

**Context**: Compressing before pruning wastes work on data that will be discarded.

**Decision**: Compression runs after pruning in the pipeline. Strategies operate on relevant data only, and token estimates are accurate at the point they matter.

**Consequences**: Pruning removes ~80-96% of types before compression runs. Compression strategies process only the surviving set.
