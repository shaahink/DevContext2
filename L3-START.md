# L3 — Kernel answers ✅ DONE (2026-07-05)

**Branch:** `feat/lighthouse-l2` · **Status:** L0–L3 done, L4 next

## Verify gate (end-of-stage snapshot)
```
dotnet build C:/Code/DevContext2-ui/DevContext.slnx        # 0w 0e
dotnet test DevContext.slnx --filter "Category!=Eval"      # 429/0 (3 skipped)
pnpm check                                                  # from src/DevContext.App — lint + test + build
```

## L3 checkpoints (see `docs/dev/briefs/proposal-lighthouse.md` §L3)
| # | What | Commit |
|---|------|--------|
| 3.6 | **Module/feature grouping** — `GroupPath` on `EntryPoint` + proto, namespace-derived, gRPC method-level entries | 4d2d027 |
| 3.5 | **Node line numbers + auth** — `LineNumber` on `NodeDetail`/`NodeResponse`, `AuthAttributes` on `EntryPoint` | ed1ca35 |
| 3.1 | **Impact RPC** — `GetImpact` wraps `GraphQuery.BlastRadius` | 60ae224 |
| 3.2 | **Top Flows ranking** — BFS-based scoring (reach × seams × entities × cross-project) | b82d203 |
| 3.4 | **Graph completeness** — hub-scoping for sparse graphs (entries < 5 or ratio < 0.1) | cbb759b |
| 3.3 | **InterestingPoints(archetype)** — 5 strategies + centrality fallback | ba736e1 |

## Next: L4 (see `docs/dev/briefs/proposal-lighthouse.md` §L4)
- Insight engine v2 envelope (claim · evidence · confidence · action)
- Per-archetype composition insight + facet pairs
- Confidence Ledger honesty for "50% confidence" decomposition
- Doc-summary hygiene (vendored namespace exclusion)
