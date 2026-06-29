# ADR-008: Stable DevContextOutput JSON schema

**Status**: Accepted

**Context**: Internal `DiscoveryModel` is subject to change. Downstream tools and agents should not break when the internal model evolves.

**Decision**: Internal `DiscoveryModel` is never serialised directly. Renderers build a `RenderedContext` with its own versioned schema. Internal model can evolve without breaking downstream tooling or agents that pin to a schema version.

**Consequences**: An extra mapping step in renderers. Schema version field enables compatibility negotiation.
