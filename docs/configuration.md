# Configuration Guide

## devcontext.json

Create a `devcontext.json` at your project root for persistent settings:

```json
{
  "$schema": "https://devcontext.dev/schemas/v2/config.json",
  "defaultProfile": "focused",
  "defaultScenario": "debug-endpoint",
  "maxOutputTokens": 6000,
  "excludePatterns": [".git", "bin", "obj", "Migrations"],
  "entryPaths": ["src/Api"],
  "profiles": {
    "quick": { "profile": "focused", "maxOutputTokens": 2000, "noRoslyn": true }
  }
}
```

### Fields

| Field | Type | Default | Description |
|---|---|---|---|
| `defaultProfile` | string | `"focused"` | Profile to use when `--profile` is not specified. One of: `focused`, `debug`, `full`. |
| `defaultScenario` | string | `"architecture"` | Scenario to use when `--scenario` is not specified. |
| `maxOutputTokens` | int | `8000` | Token budget for output. Pruning and compression work to stay under this. |
| `excludePatterns` | string[] | `[".git","bin","obj"]` | Directory/file patterns to exclude from analysis. |
| `entryPaths` | string[] | `[]` | Restrict analysis to specific subdirectories. |
| `profiles` | object | `{}` | Custom named profiles that override defaults. |

## CLI Reference

```bash
devcontext [PATH] [OPTIONS]

Arguments:
  [PATH]                  Root path (.sln, .csproj, or directory)

Options:
  -s, --scenario <NAME>   architecture | debug-endpoint | add-similar-feature |
                          modify-middleware | trace-message-flow | harden-di
  -p, --profile <NAME>    focused | debug | full
  -a, --around <PATH>     Focus on a specific type/method
  -t, --task <TEXT>       Free-text intent → auto-selects scenario + profile
      --max-tokens <N>    Token budget (default 8000)
  -o, --output <FILE>     Write to file (default stdout)
      --format <FMT>      markdown (default) | json
      --include-provenance Show why each type was included
      --include-diagnostics Show pruning notes and warnings
      --no-roslyn         Disable Roslyn workspace loading
      --metrics           Show extraction performance metrics
      --dry-run           Plan extractors without running
      --verbose           Info-level logging
      --trace             Debug-level logging
```

## Scenarios

| Scenario | Best for | Key sections |
|---|---|---|
| `architecture` | New team members, codebase overview, PR context | All sections |
| `debug-endpoint` | Debugging a failing endpoint, understanding a handler | Entry points, Endpoints, Call graph, Data model, Anti-patterns, Event flow |
| `add-similar-feature` | Copying an existing pattern for a new feature | Entry points, Endpoints, Related types |
| `modify-middleware` | Adding/modifying middleware, understanding pipeline | Architecture overview, Non-obvious wiring |
| `trace-message-flow` | Tracing events through the system | MediatR Handlers, Data model, Event flow |
| `harden-di` | Security audit, DI hardening | Entry points, Non-obvious wiring, Anti-patterns |

## Profiles

| Profile | Details |
|---|---|
| `focused` | Balanced extraction with pruning (default). Good for most use cases |
| `debug` | Adds call graph extraction (BFS from entry points). Use when you need flow tracing |
| `full` | Full analysis with source body extraction. Adds type source code to output |

## Environment Variables

| Variable | Description |
|---|---|
| `DEVCONTEXT_CONFIG` | Path to devcontext.json (default: `./devcontext.json`) |
| `UPDATE_GOLDENS` | Set to `1` to auto-update golden test files during `dotnet test` |
