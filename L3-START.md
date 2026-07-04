# L3 — Kernel answers (cold-start card)

**Branch:** `feat/lighthouse-l2` · **Status:** L0–L2 done, L3 next

## Verify gate
```
dotnet build C:/Code/DevContext2-ui/DevContext.slnx        # 0w 0e
dotnet test DevContext.slnx --filter "Category!=Eval"      # 429/0 (3 skipped)
pnpm check                                                  # from src/DevContext.App — lint + test + build
cargo check                                                 # from src-tauri/
```

## L3 checkpoints (see `docs/dev/briefs/proposal-lighthouse.md` §L3)
| # | What | Key files |
|---|------|-----------|
| 3.1 | **Impact RPC** — `GetImpact` over BlastRadius, Inspector + omnibox consume it | `GraphQuery.cs:142`, proto, `DiscoveryPipeline.cs` |
| 3.2 | **Server-side Top Flows** — rank entries by reach breadth × seam richness × entity touches × cross-project depth | `TraceBuilder.cs`, new ranking method |
| 3.3 | **InterestingPoints(archetype)** — per-archetype composition with centrality fallback | new kernel method |
| 3.4 | **Graph completeness for browse** — bind call edges hub-scoped when entries sparse | `GraphBuilder.cs`, `CodeGraph.cs` |
| 3.5 | **Node line numbers** (S2) + **auth-on-entries** (S1) onto payloads | proto, `NodeDetail`, entry payload |
| 3.6 | **Module/feature grouping** — `GroupPath` on entries, gRPC method-level | `EntryPoint.cs`, grouping logic |

## Rules
- One commit per checkpoint. `pnpm check` green before each.
- Engine-only stage — no Angular changes unless proto contract requires it.
- Docs move with code in the same commit.
- Append `docs/dev/go-to-program/PROGRESS-LOG.md` after session.
