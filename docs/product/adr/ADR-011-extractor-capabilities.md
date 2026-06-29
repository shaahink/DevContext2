# ADR-011: ExtractorCapabilities as declared metadata

**Status**: Accepted

**Context**: Extractors need to declare what signals they read and write so the pipeline can validate and the dry-run planner can render without instantiating extractors.

**Decision**: `ExtractorCapabilities` is a record on `IDiscoveryExtractor`. The pipeline uses it at startup for cycle detection and in dry-run mode for planning.

**Consequences**: Every extractor must self-describe. Dry-run works without running extractors. Cycle detection can be implemented at startup.
