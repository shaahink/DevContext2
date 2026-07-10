# Lighthouse Phase — Final Report & Handover

> **Read this first** if you're picking this project back up cold. Branch `feat/lighthouse-l2`
> @ latest (2026-07-05). This closes the "L — Lighthouse: Repo Intelligence Iteration" track
> (`docs/dev/briefs/proposal-lighthouse.md`) — L0 through L7, including a post-delivery static
> audit and fixes. Written as a warm-up doc, not a changelog.

---

## 1. What DevContext Is (Post-Lighthouse)

A .NET 10 static-analysis tool that builds an immutable typed graph of a codebase and renders it
through three surfaces:

```
DevContext.Core (kernel) — graph, extraction, caching, GraphQuery, ConfidenceLedger
        │
        ├── DevContext.Cli       — one-shot `report` + `analyze` commands
        ├── DevContext.Server    — long-lived gRPC-Web service (desktop + MCP backend)
        ├── DevContext.Mcp       — MCP server (stdio, 13 tools, the agent surface)
        └── DevContext.Desktop   — OLD WPF shell (superseded, dead weight)

DevContext.App (Tauri + Angular) — the CURRENT desktop client
```

The kernel has been hardened through L0-L6: trust-breakers fixed (E1-E9), performance architecture
redone (clone registry, snapshot-first open, throttled progress), and three consumption surfaces
(desktop, CLI report, MCP) all share the same `GraphQuery` kernel API.

---

## 2. What Lighthouse Delivered

| Stage | Theme | Status | Key Commit |
|-------|-------|--------|------------|
| L0 | Truth pass — fix 9 trust-breakers (E1-E9) | DONE (9/9, 8 fixed + 1 improved) | `829417e` … `2ddb83f` |
| L1 | Open fast, reopen instantly (GitHub/persistence/progress) | DONE (4/4) | `f692e5a` … `1ed766b` |
| L2 | CLI `report` + bench loop | DONE (4/4) | `799a1f9` … `dcc7e6c` |
| L3 | Kernel answers: Impact RPC, Top Flows, InterestingPoints, graph completeness | DONE (6/6) | `60ae224` … `4d2d027` |
| L4 | Insight engine v2 + archetype lenses | DONE (4/4) | `7c7e515` … `bf53ff0` |
| L5 | MCP server + context packs (the agent surface) | DONE | `85b15ad`, `fe09351` |
| L6 | UI/UX round: answers-first Home, readable graphs | DONE | `e9d1ab1`, `26b9ce3` |
| — | L5+L6 audit passes (2 rounds of fixes) | DONE | `3a68938`, `44e25f0` |
| L7 | Benchmark audit + close-out gate | DONE (this commit) | See §2.1 |

### 2.1 L7 — What this session delivered

1. **Benchmark re-run** across 10 repos covering all archetypes. Results in
   `eval-results/2026-07-05/`. The two largest repos (PowerToys, MassTransit) deferred to a
   separate session.

2. **AUDIT.md** (`eval-results/2026-07-05/AUDIT.md`) scoring every §2 evidence finding (E1-E9,
   P1-P6, value gaps) as FIXED/IMPROVED/OPEN. Verdict: **18/21 FIXED, 2 IMPROVED, 1 DEFERRED.**

3. **Snapshot cache versioning** added to `SnapshotCacheService` — stale pre-Lighthouse snapshots
   are now rejected with a schema version mismatch instead of silently producing 0-node graphs.

4. **Bench script SHA-clone fix** in `scripts/bench.ps1` — pinned-SHA refs now use
   `git clone --no-checkout` + `git checkout SHA` instead of `--depth 1 --branch SHA` (which fails).

5. **Static audit of all L0-L6 code** (C# + Angular) — 19 findings documented. 7 fixes delivered:
   - `McpSessionManager`: null-safe graph check (was `snapshot!.Graph!`)
   - `graph-canvas.ts`: merged duplicate `effect()` calls, converted `legendItems` to signal
   - `export-drawer.ts`: `contentPreserved` now resets in `finally` (was only on success)
   - `run-console.ts`: replaced `afterEveryRender` with `effect()` (per-render vs per-change)
   - `insights-view.ts`: evidence deduplication pre-computed in signal, NaN guard on entriesWithTarget

**Remaining findings** (documented, not fixed):
- `drawMinimap()` not throttled (pan/zoom at 60fps) — MEDIUM
- `export-drawer.ts` has no abort mechanism for concurrent renders — MEDIUM
- `run-console.ts` dead `'compress'` case + fragile substring matching — LOW
- `identity-strip.ts` `reanalyze()` discards promise (errors handled internally by store) — LOW
- Several code-style findings (non-signal fields alongside signals, swallowed error details) — LOW

---

## 3. UI/UX — Current State

All routes from the Fable redesign are preserved and enhanced:

| Surface | Route | Post-Lighthouse changes |
|---------|-------|------------------------|
| Home | `/` (session ready) | Identity *sentence* replaces classifier chips. Stat strip human-readable ("85 endpoints · 12 services"). Top Flows with targets + "why". Insights v2 with impact grouping + action buttons. Engine telemetry collapsed. Dead W9 funnel UI deleted. |
| Workbench | `/explore` | Deck identity subtitles (action/target). Group-by-module dedupe. gRPC per-method entries. System altitude uses fcose force-directed layout. Flow/Node use dagre layered. Zoom-relative label density. Zen mode minimap (>40 nodes). Focus dimming. |
| Atlas | `/atlas` | Confidence Ledger with per-seam breakdown. Topology + package view. |
| Insights | `/insights` | Severity-grouped cards with action buttons (Trace/Usages/Export). No CLI copy leaks. Evidence deduped. |
| Export drawer | Ctrl+E | Collapsible per-section cards with real token meter. Intent selector (trace/explain/review). Budget selector. Backed by `GetContext` gRPC RPC wrapping `ContextPackBuilder`. |
| MCP | stdio | 13 tools: analyze, status, map, entrypoints, top_flows, interesting_points, trace, node, neighbors, usages, search, impact, insights, get_context, read_source, close_session, list_sessions, stats. |

**Keyboard model unchanged** from Fable: tabs on `Ctrl+1-6`, Esc ladder, `?` help overlay, `j/k` deck sweep, `Ctrl+Z` trail undo.

---

## 4. Backend Graph Capabilities

### 4.1 Graph model
`CodeGraph`: 3 node kinds (`Type`, `Member`, `EntryPoint`), 8 edge kinds (`Calls`, `Sends`,
`Handles`, `Raises`, `Consumes`, `ReadsWrites`, `Resolves`, `WrappedBy`), 3 resolution tiers
(`Join`=1.0, `Syntactic`→`[approx]`, `Semantic`→`[verified]`).

### 4.2 GraphQuery API surface (post-L3)
```
EntryPoints(kind?)     Map()             Stats()
Trace(focus, depth, fanOut)              Node(id)
Neighbors(id, direction, kind?)          FindUsages(id)
ResolveNodeId(nameOrKey)                 BlastRadius(from, maxDepth)
InterestingPoints(archetype)             TopFlows(count)
ContextPackBuilder(focus, budget, intent)
```

### 4.3 RPC coverage
| Kernel capability | RPC | Status |
|---|---|---|
| All `GraphQuery` ops above | gRPC-Web `DevContextService` | All wired |
| `BlastRadius` → `GetImpact` | gRPC-Web | Wired (L3.1) |
| `ContextPackBuilder` → `GetContext` | gRPC-Web | Wired (L6.4-b) |
| **MCP tools** | MCP stdio protocol | Wired (L5) |

### 4.4 Key fix: the L0 truth pass
All 9 trust-breakers from the evidence audit (E1-E9) are addressed:
- E1 (false auth claims) → `MapGroup`/`RequireAuthorization` propagation + `AddAuthorization` fallback detection
- E2 (`?` names, non-interfaces as "multi-impl") → Fixed grouping + render assertion
- E3 (wrong code in trace snippets) → Callee-span snapshots, fixed file-line↔body-offset
- E4 (sibling-member data edge contamination) → Span-bounded `AddReadsWrites`
- E5 (commands reported as events) → `AddRaises` gated on model-derived event type set
- E6 (DbContext targets on entries) → Preference-order target selection
- E7 (WPF ICommand misclassified as CLI) → CLI-framework base-type gating
- E8 (style/stack overclaims) → Behavioral module evidence, usage-gated stack chips
- E9 (input honesty) → Local-path beats `owner/repo` shorthand; empty dir → exit 2

Each fix has corresponding test fixtures in `tests/DevContext.Core.Tests/`.

---

## 5. Caching & Persistence Architecture

Five tiers, all operational:

| Tier | Scope | Post-Lighthouse status |
|------|-------|----------------------|
| `AnalysisCache` | Per-run | CLI one-shot (correct) |
| `PersistentAnalysisCache` | Process-lifetime, mtime-invalidated | Server via `EngineHostCache` |
| `SharedAnalysisContext.SyntaxCache` | Per-analysis | Extractor sharing |
| `SnapshotCacheService` | Cross-run, disk-persisted, SHA256+git-HEAD keyed, LRU | **Now versioned** — L7 added `SnapshotSchema.Version = 1`; stale pre-Lighthouse snapshots rejected on load |
| `CloneRegistry` | Cross-run, `%LOCALAPPDATA%/DevContext/repos/registry.json` | Persistent, file-locked, retry 3x on IO contention |

**Freshness protocol:** Snapshot-first open (registry→snapshot key before network), background
staleness probe, "Repo moved ahead — Re-analyze?" chip.

---

## 6. Known Issues & Gaps

### 6.1 Engine Side

- [ ] **E9 partial scope**: Empty subfolder under an unrelated ancestor solution still resolves the
       ancestor at exit 0. Needs a `ScopeResolver`-level decision.
- [ ] **5 stale TraceQualityTests golden failures** against `eval-repos/eShop` (pre-existing from
       Fable handover, not Lighthouse-related).
- [ ] **`Sends`/`Raises` edges remain `[approx]`** — body-scan only, no semantic (Roslyn) tier.
- [ ] **Snapshot schema versioning (new in L7)**: The `SnapshotEnvelope` wrapper is backward-
       compatible for the save path but existing snapshots without the envelope will fail to load.
       This is intentional — forces re-analysis for stale caches — but a migration path for
       existing caches could be added.

### 6.2 UI/UX Side

- [ ] **`drawMinimap()` not throttled** — pan/zoom at 60fps triggers O(n) bounding box calculation
- [ ] **Export drawer has no abort mechanism** for concurrent renders
- [ ] **`run-console.ts` fragile phase-key substring matching** + dead `'compress'` case
- [ ] **Sidecar `externalBin` install packaging** not wired (lifecycle works, packaging step remains)
- [ ] Windows DPI pass (125%/150%) never explicitly tested

### 6.3 Bench/Verification Side

- [ ] **PowerToys megarepo not verified** — deferred to separate session. The L3.6 `GroupPath`,
       L6 graph layout, and L4 archetype insights need verification against this shape.
- [ ] **DntSite clone lost** — needs re-clone from GitHub
- [ ] **10 repos skipped** from full bench suite: MassTransit (framework), gRPC-dotnet, Ocelot,
       AzureFunctions, RazorPages, CLI (System.CommandLine), Blazor, Desktop (Avalonia),
       MassTransit-Sample (repo moved?), DntSite

---

## 7. Deliberately Deferred / Out of Scope

These were considered, voted on, and recorded as LATER — they are not open gaps, they are
conscious deferrals:

| Item | Rationale |
|------|-----------|
| MSBuildWorkspace "high-fidelity" tier (semantic Sends/Raises) | Body-scan + catalogs stay the default; MSBuild tier is an optional future upgrade |
| F1/F2/F5/F6/F8/F9/F11 facets | Selectively absorbed into L4 archetype composition; remaining stay backlog |
| Web face (browser-only deployment) | Desktop via Tauri + gRPC-Web is the current surface |
| CLI public polish | `report` command is the harness; polish catches up later |
| `--profile debug` option removed from user-facing UI | Hidden CLI escape kept; docs updated |
| "Skip Roslyn" user-facing toggle | Removed from UI; kept as internal `--no-roslyn` debug escape |

---

## 8. Recommended Next Steps

Pick based on priority; these are independent:

1. **Review & merge this phase.** `dotnet build` 0w 0e, `dotnet test --filter Category!=Eval` 429/0,
   `pnpm check` (lint + test 27/27 + build) green. All L0-L7 checkpoints tracked with commit refs.

2. **PowerToys verification session.** Clone PowerToys, run `devcontext report`, verify:
   - L3.6 GroupPath surfacing ~30 utilities + CmdPal module groups (not 237 flat rows)
   - L6 System altitude fcose layout producing distinct clusters
   - L4 desktop archetype insights (module map, VM↔View wiring)

3. **MCP agent transcript.** Record an agent answering a cross-service wiring question (e.g.,
   "how does order placement reach the basket in eShop?") using the 13 MCP tools, compare
   token count vs a grep-baseline attempt. This is the quantifiable pitch for MCP.

4. **Next quality iteration.** Pick from the §6 known issues list. Top candidates:
   - Semantic `Sends`/`Raises` tier (largest quality gap)
   - Coverage ladder: gRPC, SignalR, Blazor, Azure Functions/Lambda trigger entries
   - Export drawer abort mechanism (responsiveness under rapid preset switching)

5. **Cleanup session.** Run the remaining 10 benchmark repos, ratchet eval expectations, fix the
   5 stale TraceQualityTests goldens, close the E9 partial-scope gap.

---

## Appendix A: L7 Static Audit Findings (Full List)

| # | File | Finding | Severity | Fixed? |
|---|------|---------|----------|--------|
| 1 | `graph-canvas.ts:242-253` | Double `effect()` causes `rebuild()` twice per data change | HIGH | YES |
| 2 | `graph-canvas.ts:220` | `legendItems` plain array mutated in-place — not signal-reactive | MEDIUM | YES |
| 3 | `graph-canvas.ts:422-429` | `drawMinimap()` called on every pan/zoom without throttle | MEDIUM | NO |
| 4 | `graph-canvas.ts:274-287` | No try/catch around `cytoscape()` constructor | LOW | NO |
| 5 | `export-drawer.ts:448,479` | `contentPreserved` not reset on render failure | MEDIUM | YES |
| 6 | `export-drawer.ts:405-522` | No abort mechanism for concurrent async renders | MEDIUM | NO |
| 7 | `run-console.ts:267-273` | `afterEveryRender` fires on every app render — uses `effect()` now | MEDIUM | YES |
| 8 | `run-console.ts:284-294` | Dead `'compress'` case; fragile substring matching | LOW | NO |
| 9 | `insights-view.ts:191-193` | `dedupe()` allocates new array on every template evaluation | MEDIUM | YES |
| 10 | `insights-view.ts:184-188` | Unguarded `entriesWithTarget` could produce NaN | LOW | YES |
| 11 | `McpSessionManager.cs:179` | `snapshot!.Graph!` null forgiveness could NPE | HIGH | YES |
| 12 | `McpSessionManager.cs:234-244` | `EvictLru` LINQ over `ConcurrentDictionary` not atomic (benign) | LOW | NO |

## Appendix B: Gate Snapshot

```
dotnet build DevContext.slnx                             0w 0e
dotnet test DevContext.slnx --filter "Category!=Eval"    429/0 (3 skipped)
pnpm check (src/DevContext.App)                          lint 0/0 + test 27/27 + build clean
```

---

This closes the Lighthouse phase. See `docs/dev/briefs/proposal-lighthouse.md` for the full plan
and tracker. See `eval-results/2026-07-05/AUDIT.md` for the benchmark close-out audit.
