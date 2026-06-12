# Configuration Guide

## devcontext.json

Create a `devcontext.json` at your project root for persistent settings:

```json
{
  "$schema": "./devcontext.schema.json",
  "defaultScenario": "overview",
  "maxOutputTokens": 6000,
  "excludePatterns": [".git", "bin", "obj", "Migrations"],
  "entryPaths": ["src/Api"]
}
```

---

## Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `defaultScenario` | `string` | `"overview"` | Default mode: `"overview"` or `"deep-dive"` (engine key). Alias `"trace"` maps to `"deep-dive"`. `"audit"` deprecated. |
| `maxOutputTokens` | `int` | `8000` | Token budget for pruning. Range: 500–50000 |
| `excludePatterns` | `string[]` | `[".git", "bin", "obj", ".vs", "node_modules", ".idea"]` | File/directory patterns to exclude from file tree scanning |
| `entryPaths` | `string[]` | `[]` | Directories or files to limit analysis to (e.g., `["src/Api"]`) |

---

## Mode & Profile

### Mode (Scenario)

Two modes with backward-compatible engine keys:

| Mode | Engine key | Best for |
|------|-----------|----------|
| Overview | `overview` | Broad architecture map, endpoints, entities, wiring |
| Trace | `deep-dive` | Entry-point focused with call graph, handler chains |

- `trace` is accepted as a CLI alias for `deep-dive`
- `audit` is deprecated — maps to `overview` with a warning

### Profile (Auto-Derived)

Profile is automatically derived from which sections are selected. You can still pass `--profile` explicitly for backward compatibility:

| Profile | When automatically selected |
|---------|---------------------------|
| `focused` | Default — no call graph or source code sections checked |
| `debug` | Call graph section is checked |
| `full` | Source code section is checked |

---

## CLI Flags vs Config

CLI flags override `devcontext.json`:

```bash
# Config says maxOutputTokens: 6000, but CLI overrides
devcontext . --max-tokens 12000
```

The `--task` flag also overrides the config's `defaultScenario` and `defaultProfile`:

```bash
devcontext . --task "trace the order handler"
# Auto-selects: scenario=deep-dive, profile=debug
# Overrides any defaultScenario/defaultProfile in config
```

---

## Exclude Patterns

Control which files and directories are skipped during file tree discovery:

```json
{
  "excludePatterns": [
    ".git",
    "bin",
    "obj",
    ".vs",
    "node_modules",
    ".idea",
    "Migrations",
    "wwwroot/lib",
    "Generated"
  ]
}
```

Patterns are matched against file/directory names (case-insensitive substring match).

---

## Desktop Settings

The Desktop app persists settings in `%LocalAppData%\DevContext\settings.json`:

```json
{
  "lastScenario": "overview",
  "lastProfile": "focused",
  "lastFormat": "markdown",
  "lastTokens": 8000,
  "lastAround": "",
  "lastTask": "",
  "includeProvenance": false,
  "includeDiagnostics": false,
  "noRoslyn": false,
  "lastActiveSections": ["Architecture overview", "Endpoints", "MediatR Handlers", "Data model", "DI / Wiring", "Related types"]
}
```

Recent project paths are stored in `%LocalAppData%\DevContext\recent.json`.
