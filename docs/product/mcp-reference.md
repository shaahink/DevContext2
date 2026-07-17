# MCP Reference

DevContext ships an **MCP (Model Context Protocol) server** — `devcontext-mcp` — exposing **24 tools**
so any MCP-compatible agent (Claude Code, Cursor, VS Code, Cline, …) can query an analyzed .NET
codebase instead of grepping it.

Verified against `src/DevContext.Mcp/DevContextTools.cs` (tool XML summaries are the tool
descriptions clients see) and `src/DevContext.Mcp/Program.cs`.

## How it works

The MCP server is a thin **stdio → gRPC proxy**: it connects to `DevContext.Server` (gRPC-Web,
`127.0.0.1:5179`) and maps each MCP tool onto a server RPC. If the server isn't running, the MCP
process **spawns it automatically**, looking in order (`ServerShim.FindServerExe`):

1. `DEVCONTEXT_SERVER` environment variable (path to `DevContext.Server.exe`)
2. Next to the MCP exe (`DevContext.Server.exe`, `server/`, or a sibling `DevContext.Server/` dir)
3. `%LOCALAPPDATA%/DevContext/server/DevContext.Server.exe`
4. Walking up from the MCP exe to a repo root (`DevContext.slnx`) and using the built server there

Logs: `%LOCALAPPDATA%/DevContext/logs/mcp-*.log` (rolling, 7 days). The desktop app's **MCP page**
shows live status, sessions, a log feed, and a try-a-tool sandbox.

## Setup

Build once from the repo root — this produces both the MCP exe and the server it spawns:

```powershell
dotnet build DevContext.slnx
```

Then register the built exe with your client (it is not yet a global `dotnet tool` — use the full
path or put the directory on `PATH`):

**Claude Code / Cursor** (`.mcp.json` / MCP settings):

```json
{
  "mcpServers": {
    "devcontext": {
      "command": "C:/path/to/DevContext2/src/DevContext.Mcp/bin/Debug/net10.0/devcontext-mcp.exe"
    }
  }
}
```

**VS Code** (`.vscode/mcp.json`):

```json
{
  "servers": {
    "devcontext": {
      "command": "C:/path/to/DevContext2/src/DevContext.Mcp/bin/Debug/net10.0/devcontext-mcp.exe"
    }
  }
}
```

## Session model

- `analyze(path)` starts (or reuses) an analysis and returns a **handle**. It is idempotent: the
  same repo at the same git HEAD returns the existing handle.
- Every other tool takes `handle` **optionally** — omitted, it uses the most recent session.
- Symbols are addressed two ways: a precise `nodeId` (`Kind:Key`, from `resolve`/`find`) or a fuzzy
  `query`. Ambiguity is honest: a query matching several nodes returns the candidates — no tool
  ever silently picks one.
- Budgeted tools (`trace`, `get_context`) take `budgetTokens` and name what they cut
  ("N omitted") instead of truncating silently.

## Tool catalog (24)

### Session

| Tool | What it does | Key parameters |
|------|--------------|----------------|
| `analyze` | Start analysis of a .NET repo; returns the session handle. Idempotent per repo+HEAD. | `path` |
| `status` | Check whether a session handle is still valid. | `handle` |
| `list_sessions` | List all active analysis sessions on the server. | — |
| `close_session` | Release a session's resources (engine + any clone). Idempotent. | `handle` |

### Orientation — what is this repo?

| Tool | What it does | Key parameters |
|------|--------------|----------------|
| `overview` | One-call repo brief: identity, services, ServiceLinks, top flows, where to start (~600 tokens). | `handle` |
| `map` | Architecture map: style, archetype, topology, project dependencies. | `handle` |
| `stats` | Full analysis stats: node/edge counts, seam breakdown, insights, warnings. | `handle` |
| `entrypoints` | Entry points (HTTP routes, bus consumers, gRPC services). Summary by default; `kind` filters, `full:true` lists every entry. | `kind`, `top`, `full` |
| `top_flows` | Top 20 entry points ranked by importance score. | `handle` |
| `interesting_points` | Archetype-aware starting points for exploring the codebase. | `handle` |
| `insights` | All insights (warnings, notable items, info) for the analyzed repo. | `handle` |

### Navigation — find and inspect symbols

| Tool | What it does | Key parameters |
|------|--------------|----------------|
| `resolve` | Resolve a symbol/route/file to candidates with kind, service, path. Never silently picks. | `query`, `limit` |
| `find` | Free-text search across graph nodes, paginated. | `query`, `kind`, `limit`, `cursor` |
| `node` | Detail card for a node: title, kind, file path, degrees. | `nodeId` or `query` |
| `neighbors` | Outgoing/incoming edges of a node. | `nodeId`/`query`, `direction: out\|in\|usages` |
| `usages` | All usages (in-edges) of a node across the codebase. | `nodeId` or `query` |
| `read_source` | Read source for a node: `window` (N lines around) or `member` (full declaration body). | `nodeId`/`query`, `mode`, `windowLines` |

### Flow & impact — how things connect

| Tool | What it does | Key parameters |
|------|--------------|----------------|
| `flow` | Compact flow summary for an entry (≤150 tokens typical): what it touches/emits. Deep-link to `trace` for detail. | `focus`/`query`, `depth` |
| `trace` | Full call spine from one entry. Budgeted: cut subtrees are named ("N omitted"); `budgetTokens: 0` = full tree. | `focus`/`query`, `depth`, `format: default\|compact`, `budgetTokens` |
| `impact` | Transitive impact: upward (what reaches this) or downward (what this affects), grouped by service. Diff-aware `files` mode for "I changed X". | `nodeId`/`query`/`files`, `direction: up\|down`, `maxDepth` |
| `tests_for` | Best-effort: test methods whose call closure reaches a node (0 = none reached, not "untested"). | `nodeId`/`query`, `maxDepth` |
| `config` | Config-key usage sites (`IConfiguration`, `GetValue`, `GetSection`), optional key filter. | `key` |

### Context packs — LLM-ready output

| Tool | What it does | Key parameters |
|------|--------------|----------------|
| `get_context` | Budget-priced context pack for a focus: identity header, flows, signatures, bodies, DI wiring, config, contracts, tests — with per-section provenance. | `focus`/`query`, `budgetTokens` (default 8000), `intent: trace\|explain\|review` |
| `verify_context` | Has the source drifted since `analyze`? Per-section stale flags, changed files with line deltas, repo HEAD then/now (hash + line-count delta, no diff). | `focus`/`query`, `budgetTokens` |

## A typical agent session

```
analyze("C:/repos/eShop")            → handle
overview()                           → identity, services, where to start
entrypoints(kind: "HttpEndpoint")    → routes
trace(query: "POST /basket/checkout")→ call spine with provenance
get_context(query: "POST /basket/checkout", budgetTokens: 6000)
                                     → paste-ready context pack
verify_context(...)                  → still fresh? (after you've edited files)
```

## Related docs

- `docs/product/cli-reference.md` — the same engine via the `devcontext` CLI
- `docs/product/TRACE-ENGINE-DESIGN.md` — how traces are built (edges, priorities, caps)
- `proto/devcontext/v1/devcontext.proto` — the gRPC contract the MCP tools proxy to
