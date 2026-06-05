# ADR-006: IAnalysisCache for parse-once

**Status**: Accepted

**Context**: Multiple parallel extractors may request the same file. Re-reading and re-parsing wastes I/O and CPU.

**Decision**: `ConcurrentDictionary<string, Lazy<Task<T>>>` ensures each file is read and parsed exactly once regardless of how many parallel extractors request it. `FileTreeExtractor` pre-registers all paths so the dictionary exists before concurrent access begins.

**Consequences**: Guaranteed single read and parse per file per run. Memory overhead of cached content is bounded by file count.
