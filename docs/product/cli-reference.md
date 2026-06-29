# CLI Reference

## `devcontext analyze [PATH] [OPTIONS]`

Analyze a .NET project and output structured context for LLMs.

**Arguments**:

| Argument | Description |
|----------|-------------|
| `[PATH]` | Root path. Accepts `.sln`, `.csproj`, a directory, or a `github.com/user/repo` URL. Defaults to `.` (current directory). |

---

## The model: Focus drives everything

There are exactly two situations, selected by whether you give a `--focus`:

| You run | You get | Derived scenario | Derived profile |
|---------|---------|------------------|-----------------|
| `devcontext analyze .` | **Map** — architecture style, stack, project topology, entry points, packages | `overview` | `focused` |
| `devcontext analyze . --focus <entry>` | **Trace** — the call stack from that entry, down the wiring | `deep-dive` | `debug` (call graph on) |

You normally never set scenario or profile by hand — `--focus` derives both. `--scenario` / `--profile` exist only as advanced overrides.

---

## Focus (`-f`, `--focus`)

The entry point to trace from. Repeatable (first focus drives the trace).

| Format | Example | Resolves to |
|--------|---------|-------------|
| `TypeName` | `OrdersController` | A Type/Handler/Service graph node |
| `TypeName:MethodName` | `OrdersController:Create` | The type (trace walks its out-edges) |
| `VERB /route` | `POST /api/orders` | The matching HTTP endpoint |

```
# Whole-codebase Map
devcontext analyze .

# Trace from an endpoint
devcontext analyze . --focus "POST /api/orders"

# Trace from a type, 3 hops deep, full method bodies
devcontext analyze . --focus OrdersController --depth 3 --detail full
```

| Flag | Description |
|------|-------------|
| `-f, --focus <FOCUS>` | Entry point to trace from (see formats above). Repeatable. |
| `--depth <N>` | Trace depth from the focus (1–10, default 6). |
| `--detail <LEVEL>` | Trace body detail: `signature` (names only), `salient` (key body lines, default), `full` (method slice). |

**Deprecated aliases** (still accepted): `-a, --around` is an alias for `--focus`; `-t, --task` (free-text intent) is deprecated — use `--focus`.

---

## Output

| Flag | Description |
|------|-------------|
| `-o, --output <FILE>` | Write output to file (default: stdout). |
| `--format <FMT>` | `markdown` (default) → Map/Trace narrative; `json` / `html` → structured legacy renderers. |
| `--max-tokens <N>` | Token budget (default 8000; `devcontext.json` validates 100–100000). |
| `--token-view` | Per-section token accounting in the output. |
| `--include-provenance` | (Catalog output) show why each type was included. |
| `--include-anti-patterns` | (Catalog output) include anti-pattern detection. |
| `--include-diagnostics` | Show diagnostics — under a Map/Trace this appends graph + call-graph diagnostics. |

---

## Diagnostics & debugging

| Flag | Description |
|------|-------------|
| `--stats` (alias `--metrics`) | Print the full RunReport (stage waterfall, extractor table, scorer funnel, cache/corpus). |
| `--dry-run` | Plan only — lists which extractors would run, without executing. |
| `--strict` | Exit code 2 if any output self-check invariant fails. |
| `--verbose` | Info-level Serilog logging. |
| `--trace` | Debug-level Serilog logging (includes Roslyn events). |

---

## Advanced

| Flag | Description |
|------|-------------|
| `--no-roslyn` | Skip the Roslyn deep tier (faster; weaker semantic call resolution). |
| `-s, --scenario <NAME>` | Override the derived scenario: `overview` \| `deep-dive` (`trace` is an alias). |
| `-p, --profile <NAME>` | Override the derived profile: `focused` \| `debug` \| `full`. |

---

## GitHub repositories

A `github.com/user/repo` URL as `[PATH]` (or via `--repo`) is cloned, analyzed, then cleaned up.

| Flag | Description |
|------|-------------|
| `--repo <URL>` | GitHub repo URL to clone and analyze. |
| `--ref <REF>` | Branch or tag to check out (default: repo default). |
| `--cleanup <MODE>` | `auto` (default — delete after analysis) \| `keep` (retain the clone). |
| `--keep` | Shorthand for `--cleanup keep`. |

---

## Other commands

| Command | Description |
|---------|-------------|
| `devcontext init` | Create `devcontext.json` in the current directory. |
| `devcontext scenarios` | List scenarios (`overview`, `deep-dive`) with their required sections. |
| `devcontext version` | Show version and commit hash. |
