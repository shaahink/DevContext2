# CLI Reference

## `devcontext analyze [PATH] [OPTIONS]`

Analyze a .NET project and output structured context for LLMs.

**Arguments**:

| Argument | Description |
|----------|-------------|
| `[PATH]` | Root path. Accepts `.sln`, `.csproj`, directory, or `github.com/user/repo` URL. Required positional argument. |

---

## Scenarios (Modes)

Two primary modes. The `deep-dive` engine key remains for backward compatibility.

| Mode | Alias | CLI flag | Best for |
|------|-------|----------|----------|
| **Overview** | — | `--scenario overview` | Whole-codebase architecture map, endpoints, entities, wiring. Broad picture. |
| **Trace** | `deep-dive` | `--scenario trace` or `--scenario deep-dive` | Entry-point focused: call graph, handler chain, event flow. Requires `--around` for best results. |

**Deprecated**:

| Old name | Status | Maps to |
|----------|--------|---------|
| `audit` | Deprecated — prints warning | `overview` |

---

## Profile (Auto-Derived)

Profile is now **automatically derived** from which sections are selected. You can still pass it explicitly for backward compatibility.

| Section checked | Profile |
|-----------------|---------|
| ☑ Call graph | **Debug** (enables call graph extraction) |
| ☑ Source code | **Full** (enables source body extraction) |
| Neither | **Focused** (default — balanced with pruning) |

CLI override (if needed):
```
--profile focused | debug | full
```

---

## Entry Point (`--around`)

Focus analysis on a specific file, type, or method. Repeatable.

| Format | Example | Description |
|--------|---------|-------------|
| `TypeName` | `FeedController` | Resolves the type, scopes proximity pruning to its directory |
| `TypeName:MethodName` | `FeedController:SiteFeed` | Resolves the type + method, enables call graph tracing |
| File path | `./src/Controllers/FeedController.cs` | Focuses on that file's directory neighborhood |
| Folder path | `./src/Controllers/` | Focuses on types within that folder |

```
# Focus on a single controller
devcontext analyze . --around FeedController

# Trace a specific endpoint method
devcontext analyze . --scenario trace --around FeedController:SiteFeed

# Multiple entry points
devcontext analyze . --around FeedController --around UploadFileController
```

**How filtering works**: `--around` uses **directory proximity** pruning — types within `MaxPathDistance` directory hops of the focus point's file are kept. The endpoints table is also filtered to nearby source files. This means `--around FeedController` includes nearby controllers in the same directory, not JUST FeedController.

---

## Natural Language Intent (`--task`)

Pass a free-text description of what you want and DevContext auto-selects mode + profile.

| Keywords in task | Auto-selects |
|-----------------|-------------|
| `debug`, `error`, `exception`, `failing`, `500`, `trace`, `call graph` | Trace mode, Debug profile |
| `architecture`, `overview`, `structure`, `layers`, `map`, `add`, `implement`, `crud` | Overview mode, Focused profile |
| `di`, `injection`, `middleware`, `pipeline`, `wiring`, `activator` | Overview mode, Debug profile |
| `event`, `message`, `publish`, `consume`, `queue`, `bus` | Trace mode, Focused profile |

```
# Let the tool figure out the best mode
devcontext analyze . --task "trace the failing order handler"
# → Auto-selects: Trace mode + Debug profile

devcontext analyze . --task "architecture overview"
# → Auto-selects: Overview mode + Focused profile
```

---

## Output

| Flag | Description |
|------|-------------|
| `-o, --output <FILE>` | Write output to file (default: stdout) |
| `--format <FMT>` | Output format: `markdown` (default) or `json` |
| `--max-tokens <N>` | Token budget (default: 8000). Range: 500–50000 |

---

## Diagnostics & Debugging

| Flag | Description |
|------|-------------|
| `--include-diagnostics` | Show pruning notes, warnings, and pipeline events in output |
| `--include-provenance` | Show why each type was included in the output |
| `--metrics` | Show per-extractor timing breakdown after analysis |
| `--dry-run` | Plan only — lists which extractors would run without executing |
| `--verbose` | Info-level Serilog logging to console |
| `--trace` | Debug-level Serilog logging (includes Roslyn events) |

---

## Advanced

| Flag | Description |
|------|-------------|
| `--no-roslyn` | Skip Roslyn workspace loading (faster, no call graph / syntax analysis) |
| `--include-anti-patterns` | Include anti-pattern detection in output |
| `--token-view` | Show per-section token accounting table in output |

---

## Other Commands

| Command | Description |
|---------|-------------|
| `devcontext init` | Create `devcontext.json` in current directory |
| `devcontext scenarios` | List available scenarios with descriptions and required sections |
| `devcontext version` | Show version and commit hash |
