# ADR-004: ConcurrentBag for Provenance values

**Status**: Accepted

**Context**: Prior design used `List<T>` returned from `ConcurrentDictionary.GetOrAdd` — concurrent `.Add()` on the same list is a race.

**Decision**: Use `ConcurrentBag` as the value type in `ConcurrentDictionary`. `ConcurrentBag` is append-only and concurrency-safe for this pattern.

**Consequences**: Provenance values are reliably collected from parallel extractors without locking.
