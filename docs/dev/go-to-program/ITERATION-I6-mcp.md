# Iteration I6 — MCP server

> **Status: BLOCKED on I2** (wire contract) · Phase: V4 · One session.
> Direction confirmed by the user 2026-07-02: LLMs talk to the engine to navigate code / get insight;
> same kernel as CLI/desktop/(web later).

## Goal

`devcontext serve --mcp`: a stdio MCP server whose tools are `GraphQuery` ops — nothing more.

## Design

- **Host:** new `src/DevContext.Mcp` (or a mode inside `DevContext.Cli serve`) using the official
  `ModelContextProtocol` C# SDK. Session model = the same analyze-handle cache the Server uses
  (`analyze` tool returns a handle; other tools take it; LRU of 3 snapshots).
- **Tools (verbatim from the query surface):** `analyze(path|url)`, `list_entrypoints(kind?)`,
  `map()`, `trace(focus, depth?)`, `stats()`, `insights()`, `node(name)`, `neighbors(name, dir?)`,
  `find_usages(name)`, `search(text)`, `blast_radius(name)` when F13 exists, `facet(id)` when I5 lands.
  Responses = the I2 kernel JSON records; descriptions written for an agent ("use trace when you need
  the wiring path from one entry; use search to find a node id first").
- **Tricky bit — long analyze:** MCP tools should not block minutes. `analyze` returns immediately
  with `{handle, status}` and a `status(handle)` tool reports progress (reuse the ProgressEvents the
  Server streams); tools on a not-ready handle return a retriable status. Cache by normalized path so
  a re-`analyze` of the same repo is a hit.
- **Honesty in-band:** every response envelope carries `scope` + `coverage` (targets N/M, approx %) so
  the model can calibrate trust — this is the differentiator vs. the model grepping alone.

## Validation

- Contract tests: each tool against TodoApi snapshot (in-proc, no stdio) — schema + a golden field.
- A recorded end-to-end transcript: Claude/agent session on eShop answering "how does order placement
  reach the email?" via the tools, checked into `docs/product/examples/mcp-eshop-transcript.md`.
  This doubles as the README demo and the input for the V5.4 re-probe (probe arm D2: agent+MCP).

## Docs & gate

`docs/product/MCP-REFERENCE.md` (tool list + install snippet for Claude Desktop/Code) ·
cli-reference `serve` section · gates green · transcript committed.
