# L4 — Insight engine v2 ✅ DONE (2026-07-05)

**Branch:** `feat/lighthouse-l2` · **Status:** L0–L4 done, L5 next

## Verify gate (end-of-stage snapshot)
```
dotnet build C:/Code/DevContext2-ui/DevContext.slnx        # 0w 0e
dotnet test DevContext.slnx --filter "Category!=Eval"      # 429/0 (3 skipped)
pnpm check                                                  # from src/DevContext.App — lint + test + build
```

## L3 delivery audit findings (fixed pre-L4)
| # | Finding | Commit |
|---|---------|--------|
| F1 | BfsEntryScore cross-project counting uses file names not project names | 2d9002e |
| F2–F6 | Angular integration gaps: Impact/InterestingPoints RPCs, GroupPath, LineNumber, AuthAttributes, Score | 6a1564d |
| F7 | InterestingForDesktop uses file name not GraphNode.Project | (in 92ea35b) |
| F8 | Dead hub-scope dedup loop removed | 5b7469b |
| F9 | BlastRadius O(1) entry lookup via Dictionary | (in 92ea35b) |

## L4 checkpoints (see `docs/dev/briefs/proposal-lighthouse.md` §L4)
| # | What | Commit |
|---|------|--------|
| 4.1 | **Insight envelope v2** — Confidence, WhyItMatters, Action/ActionTarget on Insight; enriched AnonymousEndpointsSource; Angular insight cards with action buttons | 7c7e515 |
| 4.2 | **Per-archetype composition** — 6 sources (Web, Library, Messaging, Desktop, CLI, Gateway) with gates + honest confidence | 269cb90 |
| 4.3 | **Confidence Ledger** — per-seam breakdown, auth coverage, entry target %; proto + gRPC integration | b5f37c3 |
| 4.4 | **Doc-summary hygiene** — vendored namespace exclusion (JetBrains.Annotations, System.Runtime.*, Microsoft.CodeAnalysis) | b5f37c3 |
| — | **L4 audit fixes** — GatewayArchetypeSource + Angular Confidence Ledger display | bf53ff0 |

## Next: L5 (see `docs/dev/briefs/proposal-lighthouse.md` §L5)
- MCP server + context packs (the agent surface)
- Tools v1: analyze, status, map, entrypoints, top_flows, interesting_points, trace, node, neighbors, usages, search, impact, insights
- get_context(focus, budget_tokens, intent) — ContextPackBuilder
- read_source(node_id, span?) — precise file:line reads
