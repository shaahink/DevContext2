# MCP Reference

DevContext ships an **MCP (Model Context Protocol) server** — `devcontext-mcp` — exposing **14
advertised tools plus 8 unlisted specialists** so any MCP-compatible agent (Claude Code, Cursor,
VS Code, Cline, …) can query an analyzed .NET codebase instead of grepping it.

Verified against `src/DevContext.Mcp/DevContextTools.cs` (the `[Description]` attributes ARE the
descriptions clients see — the XML doc file is not read by the SDK), `src/DevContext.Mcp/McpToolMenu.cs`
(which half a tool is on) and `src/DevContext.Mcp/Program.cs`.

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
  same repo at the same git HEAD and the same solution returns the existing handle.
- `analyze` says which of those two it did: `cached: true` means no analysis ran (an open session
  for this repo+HEAD+solution, or a snapshot-cache hit), `cached: false` means it analysed now and
  the result has been snapshotted. It returns the run's `summary` as well — archetype, projects,
  nodes, edges, entries, `entriesWithTarget`, `elapsedMs`, warnings — so the first call answers
  "what is this repo" without a second one. A first analysis of a large repo can take minutes.
- **How old is the answer?** The summary dates itself: `analyzedAt` (ISO-8601 UTC) is when the
  analysis behind those numbers actually finished, `gitHead` is the commit it describes, and
  `fromCache` says the numbers were rehydrated from a persisted snapshot rather than computed now.
  Read `fromCache` before `elapsedMs`: a rehydrate stops the clock at the *load*, so a snapshot from
  last week reports a couple of hundred milliseconds. `cached` and `fromCache` answer different
  questions — `cached` is "did this CALL do work" (an already-open session also makes it true),
  `fromCache` is "were these NUMBERS loaded from disk".
- `list_sessions` carries the same distinction: `ageSeconds` is how long ago the **session** opened,
  which a cache hit resets to zero; `analyzedAt`, `commitSha` and `fromCache` are about the
  **analysis**.
- A repo with several solutions analyses one of them. `map` says which in `solutionScope` and lists
  the alternatives; `analyze(path, sln:)` switches to one of them by name, file name, or
  repo-relative path.
- Every other tool takes `handle` **optionally** — omitted, it uses **the session this client last
  analyzed**, not whichever session the server touched most recently. That distinction is the whole
  point: the server bumps a session's last-access time on every call from anyone, so the desktop app
  opening one repo used to silently retarget an agent's next handle-less call to it. If this client
  has analyzed nothing and more than one session is open, the tools name the open sessions and ask
  for a handle rather than guessing.
- Symbols are addressed two ways: a precise `nodeId` (`Kind:Key`, from `resolve`/`find`) or a fuzzy
  `query`. Ambiguity is honest: a query matching several nodes returns the candidates — no tool
  ever silently picks one.
- Budgeted tools (`trace`, `get_context`) take `budgetTokens` and name what they cut
  ("N omitted") instead of truncating silently. A dial you do not name is left UNSET on the wire, so
  the server's one trace policy applies it — the MCP no longer carries its own copy of the defaults.

## Tool catalog — 14 advertised, 8 unlisted (†)

`tools/list` advertises **14** tools: the core surface for getting from "a repo" to "the right code
with its wiring". The other **8 are marked † below — unlisted, not removed.** They are built from
the same methods with the same schemas and **calling one by name works exactly as before**; they are
simply kept off the menu an agent reads at connect, because tool selection is the whole game and 22
verbs is a lot to weigh before any work has happened. An unknown-tool reply names them and says what
each one answers, so they stay discoverable.

Every tool and **every parameter** carries a description on the wire. That was not true before
2026-08-13: all 22 shipped `description: ""` while 26 XML doc summaries sat unused in the source,
because the SDK reads `[Description]` attributes and not the XML doc file.
`eval/mcp-qa/wire-truth.js` measures both facts off a real `tools/list` and fails if either regresses.

### Session

| Tool | What it does | Key parameters |
|------|--------------|----------------|
| `analyze` | Start analysis of a .NET repo; returns the session handle. Idempotent per repo+HEAD+solution. | `path`, `sln` |
| `status` † | Check whether a session handle is still valid. | `handle` |
| `list_sessions` † | List all active analysis sessions on the server. | — |
| `close_session` † | Release a session's resources (engine + any clone). Idempotent. | `handle` |

### Orientation — what is this repo?

| Tool | What it does | Key parameters |
|------|--------------|----------------|
| `overview` | One-call repo brief: identity, services, ServiceLinks, top flows, plus `startHere` — the archetype-aware starting points, each with the `nodeId` that addresses it. | `handle` |
| `map` | Architecture map, structured **and** rendered: style, archetype, topology, packages, aggregates, pipeline behaviours, per-service styles, the library surface (entry API, abstractions, namespace groups, internals, extension points, consumer paths, generators), the archetype view, the solution scope — plus the markdown. | `handle` |
| `stats` | Full analysis stats: node/edge counts, seam breakdown, every insight (category, severity, evidence, confidence, action), warnings, swallowed extraction failures. | `handle` |
| `entrypoints` | Entry points (HTTP routes, bus consumers, gRPC services). Summary by default; `kind` filters, `full:true` lists every entry. | `kind`, `top`, `full` |
| `top_flows` † | Top 20 entry points ranked by importance score. | `handle` |

### Navigation — find and inspect symbols

| Tool | What it does | Key parameters |
|------|--------------|----------------|
| `resolve` | Resolve a symbol/route/file to candidates with kind, service, path. Never silently picks. | `query`, `limit` |
| `find` | Free-text search across graph nodes, paginated. `kind` filters server-side, so `total` and `hasMore` count every match, not the page. | `query`, `kind`, `limit`, `cursor` |
| `node` † | Detail card for a node: title, kind, file path, degrees. | `nodeId` or `query` |
| `neighbors` | Outgoing/incoming edges of a node. `kind` narrows to one seam server-side, which is how you ask a pointed question: who **writes** this table (`direction: in, kind: ReadsWrites`), who **sends** this command (`kind: Sends`), who **consumes** this event (`kind: Consumes`). The reply always carries `totalEdges` and `kindsPresent` for the **unfiltered** set in that direction, so a filter that matched nothing names what to ask instead — and an unknown kind returns nothing plus the valid list, never the unfiltered edges wearing your filter's name. On a type, the members' edges roll up, so the answer names *which member* carries the seam. | `nodeId`/`query`, `direction: out\|in\|usages`, `kind` |
| `usages` | All usages (in-edges) of a node across the codebase. | `nodeId` or `query` |
| `read_source` | Read source for a node: `window` (N lines around) or `member` (full declaration body). | `nodeId`/`query`, `mode`, `windowLines` |

### Flow & impact — how things connect

| Tool | What it does | Key parameters |
|------|--------------|----------------|
| `trace` | Call spine from one entry. `format: compact` is the small flow summary (~150 tokens: `steps`/`touches`/`emits`, each step prefixed with a seam glyph, plus a `legend` keying the ones it used); `format: default` is the full tree. **Omit `depth`/`budgetTokens` and the server's trace policy decides** — and only then can it deepen a walk that hit the limit with budget to spare. Naming a dial gets exactly that dial; `budgetTokens: 0` = full tree. Cut subtrees are named ("N omitted"), and `budgetSource` says whether the budget was yours or the policy's. | `focus`/`query`, `depth`, `format: default\|compact`, `budgetTokens` |
| `impact` | Transitive impact: upward (what reaches this) or downward (what this affects), grouped by service. Diff-aware `files` mode for "I changed X". | `nodeId`/`query`/`files`, `direction: up\|down`, `maxDepth` |
| `seam` | The wiring path **between** two symbols, hop by hop, each hop naming its seam kind, how the edge was bound, and the file:line. `trace` walks down from one entry and `impact` returns a set with distances; this is the only tool that answers "how does A reach B". Returns the shortest paths (`totalPaths` counts every one, including those the page left out); says `direction: reverse` when the connection runs the other way round, and names the retry when the hop budget — not the graph — ended the search. | `from`, `to`, `maxDepth` (8), `maxPaths` (3) |
| `tests_for` † | Best-effort: test methods whose call closure reaches a node (0 = none reached, not "untested"). | `nodeId`/`query`, `maxDepth` |
| `config` † | Config-key usage sites (`IConfiguration`, `GetValue`, `GetSection`), optional key filter. | `key` |

### Context packs — LLM-ready output

| Tool | What it does | Key parameters |
|------|--------------|----------------|
| `get_context` | Budget-priced context pack for a focus: identity header, flows, signatures, bodies, DI wiring, config, contracts, tests — with per-section provenance. A pack filling <85% of budget says why (`fillNote`: budget-cut vs content-exhausted) and, when the focus is weakly connected, suggests better-connected focuses (`suggestedFocuses`). | `focus`/`query`, `budgetTokens` (default 8000), `intent: trace\|explain\|review` |
| `verify_context` † | Has the source drifted since `analyze`? Per-section stale flags, changed files with line deltas, repo HEAD then/now (hash + line-count delta, no diff). | `focus`/`query`, `budgetTokens` |

### Folded tools (removed — the replacement answers the same question)

Three tools were second doors onto a call the surviving tool already made, so the menu carried them
and an agent had to learn which of two names to pick. Calling a retired name returns an envelope
naming the replacement.

| Retired | Use instead | Why it was the same answer |
|---------|-------------|----------------------------|
| `flow` | `trace(focus, format: "compact")` | Both called `GetTrace` and rendered the response through the same compact builder; only the dials differed. The `steps`/`touches`/`emits` counters moved onto the compact trace. |
| `insights` | `stats` | Both read the same `GetStats` response; `stats` already returned every insight, and now carries `confidence` too. |
| `interesting_points` | `overview` | `overview` already made the call — it just spent the answer on four bare titles. The points ride its `startHere` array in full, `nodeId` included. |

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
