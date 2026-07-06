# AGENTS.md — DevContext monorepo

Branch `feat/meridian-m0`. Monorepo: engine + server + CLI (C#), desktop app (Angular + Rust/Tauri).

## Cold start
1. Read `MERIDIAN-START.md` — the current stage card (branch, checkpoints, key files, gate).
2. `src/DevContext.App/AGENTS.md` — app conventions, run commands, architecture layering.
3. `docs/dev/briefs/meridian-agent-playbook.md` — quality bar, anti-patterns, run/test instructions.

## Verify loop
```powershell
# From C:/Code/DevContext2-ui/src/DevContext.App
pnpm check          # lint + vitest + build
pnpm server         # start .NET server (separate terminal)
pnpm dev:web        # start Angular dev server

# From C:/Code/DevContext2-ui
dotnet build DevContext.slnx                         # 0 warnings
dotnet test  DevContext.slnx --filter "Category!=Eval"
```

## Hard rules
- `pnpm check` green before every commit. `dotnet build` green for engine changes.
- Docs move with code in the same commit.
- Append `docs/dev/go-to-program/PROGRESS-LOG.md` after every session.
- Do not write new C# extractors — reform in place.

## Work items
- **Meridian M0-M1** ✅ — harness gate + wiring truth pass (M1.1–M1.9 COMPLETE). See `MERIDIAN-START.md`.
- **Meridian M2** 🔜 — insight relevance + layer/feature facets (M2.1–M2.4). Next session.
- **Lighthouse L0–L7** ✅ — see `docs/dev/HANDOVER-LIGHTHOUSE.md`.
- **Fable** ✅ — W0-W7 done. See `docs/dev/HANDOVER-FABLE-FINAL.md`.
- **U3 Facet views** ⬜ — blocked on engine E4.

## Resume protocol (post-M1 → M2)
```
git -C C:/Code/DevContext2-ui checkout feat/meridian-m0
git -C C:/Code/DevContext2-ui pull
dotnet build C:/Code/DevContext2-ui/DevContext.slnx
Set-Location C:/Code/DevContext2-ui/src/DevContext.App; pnpm check
# Read MERIDIAN-START.md for handoff + checkpoint state
# Read docs/dev/briefs/meridian-agent-playbook.md
# Read docs/dev/briefs/proposal-meridian.md §M2 (insight relevance pass) and §M6 (Home+Atlas)
# Run baseline: dotnet run --project src/DevContext.Cli -- report <dogfood-path> -o out.md
```

## Next session — M2: insight relevance + layer/feature facets
**Recommended delivery: 4 bullet items** (M2.1–M2.4, one session, engine-only — no UI). See `proposal-meridian.md` §M2.

| # | What | Key files |
|---|------|-----------|
| M2.1 | Retire/repair discredited insight sources: folder-name "module map" → real service/feature grouping, "downstream wiring" → only actual ServiceLinks, dead-code → exclude DI/framework-resolved types, CLI leaks out of insight text | `src/DevContext.Core/Insights/`, `DiscoveryPipeline.ComputeInsights()` |
| M2.2 | New wiring-grounded insight classes: event-flow map (published w/ and w/o consumers), cross-service SPOFs, endpoints missing FluentValidation, config keys without defaults, auth surface (exclude Razor) | `src/DevContext.Core/Insights/` (add sources, implement `IInsightSource`) |
| M2.3 | Typed insight actions end-to-end (D6): engine → proto → UI — evidence chips render as links only with resolvable target. `focus:<entry>` / `node:<id>` / `filter:<kind>` | `proto/devcontext/v1/devcontext.proto`, `ProtoMapper.cs`, `src/DevContext.App/src/app/` |
| M2.4 | Layer/feature classification (D9, engine only): every Type node gets `Layer`/`Feature` facet (project role + folder conventions + base types + reference direction). Emit `LayerViolation` detections. **No UI rendering** — waits for M6/M7 lenses. | `src/DevContext.Core/Graph/GraphBuilder.cs`, `CodeGraph.cs` (node facets), `ArchitectureStyleDetector.cs` |

**Gate:** on dogfood repo, every insight card click lands on a working view; layer facet coverage ≥90% of Type nodes.
**Baseline (post-M1):** 493 nodes · 316 edges · 6 ServiceLinks · 36 entries · Style Microservices · 6 per-service styles.
