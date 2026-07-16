# CLI Reference

Verified against `src/DevContext.Cli/Settings/AnalyzeSettings.cs`.

## `devcontext analyze [PATH] [OPTIONS]`

Analyze a .NET solution and emit a **Map** (no focus) or a **Trace** (with `--focus`).

**Argument**:

| Argument | Description |
|----------|-------------|
| `[PATH]` | Root path. Accepts `.sln`, `.csproj`, a folder, or `Type:Method` notation. **Prefer an absolute path** — a relative path that doesn't exist on disk is tried as a GitHub `owner/repo` shorthand and cloned (an existing local path always wins). Use `--repo` for an explicit URL. |

## The model: focus drives everything

| You run | You get |
|---------|---------|
| `devcontext analyze <path>` | **Map** — architecture style, stack, project topology, entry points, packages |
| `devcontext analyze <path> --focus <entry>` | **Trace** — the wiring path from that entry, down the seams |

There is no scenario/profile to choose — presence of `--focus` selects Map vs Trace. (The old
`--scenario`/`--profile`/`--task`/`--around` model is retired; see *Retired flags* below.)

## Focus & trace

| Flag | Description |
|------|-------------|
| `-f, --focus <FOCUS>` | Entry point to trace from. **Repeatable.** Formats: `TypeName` · `TypeName:MethodName` · `VERB /route` (e.g. `POST /api/orders`). |
| `--depth <N>` | Graph depth from the focus point (1–10). |
| `--detail <LEVEL>` | Trace body detail: `signature` · `salient` (default) · `full`. |
| `--include-map` | When tracing, also render the Map/architecture sections alongside the trace. |

```
devcontext analyze C:\src\Shop                                   # Map
devcontext analyze C:\src\Shop --focus "POST /api/orders"        # Trace from an endpoint
devcontext analyze C:\src\Shop --focus OrdersController --depth 3 --detail full
```

## Output

| Flag | Description |
|------|-------------|
| `-o, --output <FILE>` | Write the rendered content to a file. stdout still carries an explanation line + the stats summary. |
| `--format <FMT>` | `markdown` (default) · `json`. |
| `--include-diagnostics` | Append diagnostics (graph + call-graph) to the output. |

> **JSON isn't pure on stdout** — the CLI prints an explanation + stats line around the content. Write
> with `-o out.json` and parse the file.

## Speed & caching

| Flag | Description |
|------|-------------|
| `--no-roslyn` | Disable the Roslyn deep tier — faster, deterministic; some deep/dispatch edges drop. |
| `--lite` | Skip the full graph build (source bodies + call graph); the Map still renders but loses dispatch targets/deep traces, and a focus re-analyzes. |
| `--fast` | Skip heavy extractors (call graph, anti-patterns, unconditional scanners) for max speed. |
| `--no-cache` | Always perform a fresh analysis (result is still cached for future runs). |
| `--cache-only` | Fail if a cached snapshot is not available (CI reproducibility). |

## Diagnostics

| Flag | Description |
|------|-------------|
| `--stats` | Print the full RunReport (stage waterfall, extractor table, scorer/token funnel, cache/corpus/graph). |
| `--dry-run` | Plan only — no extraction. |
| `--strict` | Exit code 2 if any output self-check invariant fails. |
| `--verbose` / `--trace` | Info-level / Debug-level (incl. Roslyn) Serilog logging. |
| `--quiet` | Suppress output on success; errors still go to stderr. |

## GitHub repositories

| Flag | Description |
|------|-------------|
| `--repo <URL>` | GitHub repo URL to clone and analyze. |
| `--ref <REF>` | Branch or tag to check out (default: repo default). |
| `--keep` | Keep the cloned repo after analysis (default: cleaned up). |

## Retired flags (accepted as hidden no-op stubs, then removed)

`--around`, `--scenario`, `--profile`, `--task` → use `--focus`. `--max-tokens`, `--token-view`,
`--include-provenance`, `--include-anti-patterns`, `--metrics`, `--cleanup` → the token-budget /
scenario / catalog model is retired. These still parse (for a grace period) but do nothing — don't use
them in scripts.

## Other commands

| Command | Description |
|---------|-------------|
| `devcontext query` | Graph queries over a session (`-f/--focus`, `--path`, `--direction`, `--attach`, `--format`, `--depth`). |
| `devcontext report` | Render a report from a prior analysis. |
| `devcontext init` | Create a `devcontext.json` in the current directory. |
| `devcontext scenarios` | List registered scenarios. |
| `devcontext version` | Show version and commit hash. |

See `docs/product/configuration.md` for `devcontext.json`, and `docs/product/AGENT-REFERENCE.md` for the engine.
