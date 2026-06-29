# ADR-005: Typed Detection hierarchy

**Status**: Accepted

**Context**: Pruners and renderers need type-safe access to detections without switch statements or type checking.

**Decision**: `Detection` is an abstract record. All specific detections (`EndpointDetection`, `MediatRHandlerDetection`, etc.) extend it. Query via `model.Detections.OfType<T>()`.

**Consequences**: Zero-cast access. New detection types extend the hierarchy without touching base or existing types. Pruners and renderers remain open for extension.
