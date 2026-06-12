# Eval Expectation File Schema

An expectation file describes what the DevContext tool should produce for a given
evaluation repository. Each file lives in `eval/expectations/<name>.json`.

## JSON Schema

```jsonc
{
  "repo": "eval-repos/eShop",        // path relative to repo root
  "checks": [
    {
      "id": "unique-check-id",       // short identifier, unique within the file
      "type": "check-type",          // see below
      // Type-specific fields:
      "path": "$.architecture.style", // JSONPath (only for json-* types)
      "format": "markdown",           // output format to check (output-* types)
      "value": "...",                 // expected value or signal name
      "min": 5,                       // minimum (json-range, detection-count)
      "max": 80,                      // maximum (json-range, detection-count, max-elapsed-ms)
      "detectionType": "EndpointDetection",  // detection type discriminator (detection-count)
      // Required fields:
      "status": "expected",           // "expected" (must pass) or "aspirational" (known defect)
      "note": "..."                   // human-readable explanation
    }
  ]
}
```

## Check Types

| Type | Purpose | Required Fields |
|------|---------|-----------------|
| `json-equals` | JSON value at path must equal value | path, value |
| `json-range` | JSON number at path must be in [min, max] | path, min, max |
| `json-contains` | JSON string at path must contain substring | path, value |
| `output-contains` | Rendered output must contain substring | format, value |
| `output-not-contains` | Rendered output must NOT contain substring | format, value |
| `signal-present` | A signal by key must be detected (true) | value (signal key) |
| `detection-count` | Count of detections of a type in [min, max] | detectionType, min, max |
| `max-elapsed-ms` | Rendering wall-clock in ms must be ≤ value | value |

## Status

- `expected` — must pass. Test failure blocks the gate.
- `aspirational` — known defect. Failure is printed but not blocking.
  Flip to `expected` in the same commit that fixes the underlying issue.
