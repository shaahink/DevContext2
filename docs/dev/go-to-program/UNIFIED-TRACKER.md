# Unified Tracker — Go-To for Every .NET Repo

> Single source of truth combining the cross-repo engine audit (W1–W9) and the go-to program (I1–I7).
> Branch: `go-to/implement-iterations` (base: `develop` @ `7228d1e`; contains all W1–W9 code).
> Gate: `build 0w · fast tests 382/0`.

> **Round-2 status (2026-07-02):** I1/I2 landed; I3+I4+I5 are **PARTIAL, not DONE** — verification found
> insights never reach any face (the "one wire contract" was skipped) and NodeLink/Graph/Settings were
> never built. The current plan is **[`ITERATION-R2-verify-and-finish.md`](ITERATION-R2-verify-and-finish.md)** —
> start there. The step-level `⬜` boxes below are the original plan grid, not live status.

## Resume protocol (cold start)

1. Read this tracker + the iteration guide for the current item.
2. `dotnet build DevContext.slnx` — must be 0 warnings.
3. `dotnet test DevContext.slnx --filter "Category!=Eval"` — must be green.
4. Execute the current item's Step 0 (reproduce) → implement → verify.
5. Commit per item. Update this tracker's Status. Append to `PROGRESS-LOG.md`.

## Status legend

✅ Done · 🔄 In progress · ⬜ Not started · 🔬 Research · ❌ Blocked

---

## Phase 10.A–10.D — Engine audit fixes (from `feat/engine-cross-repo-analysis`)

All items below are code-complete and verified present in this branch. Re-verified: build 0w, tests 382/0.

| ID | Item | Code locus | Verified | Status |
|----|------|-----------|----------|--------|
| **W1** | Exclude test/stress/template entries from inventory | `Graph/NoiseFilter.cs:IsProductionEntrySource` | `NoiseFilterTests` (13 cases) | ✅ |
| **W6** | Prefer product solution over `*.Samples` at repo root | `Extractors/Generic/SolutionDiscoveryExtractor.cs:49-68` | `SolutionDiscoveryExtractorTests` (6/6) | ✅ |
| **W8** | Entry→target fallback to owning controller type | `Graph/GraphBuilder.cs:EnrichEntryTargets` | `GraphBuilderTests` (9/9) | ✅ |
| **L2** | Share test-path predicate into GraphBuilder | `Graph/GraphBuilder.cs:AddEntityNodes/AddEventConsumers/...` | Gates green | ✅ |
| **W3** | Library type-rooted traces follow member call edges | `Graph/TraceBuilder.cs:OutEdgesWithTwin` | `TraceQualityTests` +2 InlineData | ✅ |
| **W3b** | Honest message when focus resolves but has no out-edges | `DiscoveryPipeline.cs:RenderAsync` | Unit test | ✅ |
| **W5** | Desktop-app archetype + UI entry points | `Graph/ArchetypeDetector.cs` + `DesktopEntryExtractor` | `ArchetypeDetectorTests` (7/7) | ✅ |
| **W7** | API-gateway / reverse-proxy archetype + ROUTES | `DiscoveryPipeline.PopulateGatewayRoutes` + `MapRenderer` | `ocelot.json` eval | ✅ |
| **W4** | Structural section caps + ranking for huge repos | `Rendering/MapRenderer.cs:AppendTopology/AppendEntryPoints` | `MapRendererTests` (3/3) | ✅ |

---

## I1 — Trust at breadth: hardening the seams

**Phase:** V1 · **Guide:** `ITERATION-I1-trust.md` · **Status:** ⬜

| Step | Task | Gate |
|------|------|------|
| I1.1 | Span-bound variable resolution (the bug) | `GraphBuilderSpanTests.Send_of_parameter_does_not_steal_sibling_methods_new` green |
| I1.2 | In-span dataflow-lite (param/field fallback) | eShop `/draft` aspirational → expected |
| I1.3 | Receiver-typed dispatch + `DispatchSeamCatalog` | Negative: `SmtpClient.Send(msg)` no Sends edge; catalog fixtures |
| I1.4 | Model-derived event/request type-sets | Event type-set from BaseTypes/ImplementedInterfaces; regex demoted |
| I1.5 | Pattern-zoo corpus | `PatternZooTests` green; raw-string trap fixed |
| I1.6 | Multi-impl honesty | Trace annotates `[×N impls]` when DI has >1 |
| I1.7 | Hygiene: excludes += eval-repos/analysis-repos + extractor convention test | analyze DevContext2 root completes; convention test green |

**Gate:** `eval/gates.ps1` green · eShop flagship trace unchanged except removed fabrications.

---

## I2 — CLI v2 + kernel wire format (W9 retirement)

**Phase:** V4 · **Guide:** `ITERATION-I2-cli-kernel.md` · **Status:** ⬜ · **Depends on:** I1

| Step | Task |
|------|------|
| I2.0 | Consumer inventory — grep `FinalScore`, `RenderPlanBuilder`, `TokenBudgetEnforcer`, `PatternRelevancePruner` |
| I2.1 | `KernelJsonRenderer` — serialize `MapModel` + `EntryPoint[]` + `Trace` + `Insights[]` |
| I2.2 | Migrate eval `json-*` checks to new shape, then delete catalog machinery |
| I2.3 | CLI v2 flag sweep (remove `--task`, `--around`, `scenarios`, `--metrics`, `--include-provenance`, `--include-anti-patterns`; hide `--scenario`/`--profile`) |
| I2.4 | `devcontext query <op>` command + `--attach` for gRPC session reuse |
| I2.5 | `--stats` gains value: `repo` (default) | `engine` (telemetry) |

**Gate:** gates green · `analyze --format json` works · `query trace` returns same hops · deleted types gone.

---

## I3 — Insights engine

**Phase:** V2/V3 · **Guide:** `ITERATION-I3-insights.md` · **Status:** ⬜ · **Depends on:** I2

| Step | Task |
|------|------|
| I3.1 | Kernel: `Insight.cs`, `IInsightSource`, `InsightsBuilder` — catalog-registered |
| I3.2 | 10 launch sources (`auth.anonymous`, `wiring.hubs`, `graph.orphans`, etc.) |
| I3.3 | Faces: CLI `--stats` prints insights first; Map Overview top-3; desktop cards |

**Gate:** gates green · eShop shows ≥4 non-Info insights · TodoApi stays quiet.

---

## I4 — Desktop UX: canvas becomes a browser

**Phase:** V3/V7 · **Guide:** `ITERATION-I4-desktop-ux.md` · **Status:** ⬜ · **Depends on:** I2

| Slice | Task |
|-------|------|
| I4.1 | Node Card — slide-over, neighbors grouped by seam, reveal-in-editor |
| I4.2 | Command palette (Ctrl+K) — SearchNodes, verbs, analyze input |
| I4.3 | Entries smartness — kind chips, filter, sort, row actions |
| I4.4 | Interactive trace — expand-on-node, seam-color edges, badges |
| I4.5 | Honesty ribbon — archetype, scope, N/M targets, approx% |
| I4.6 | Insights section (with I3) + Engine drawer |
| I4.7 | Export packs — Onboarding / Trace / Review presets |

**Gate:** `pnpm check` · app tests green · screenshot series in PR.

---

## I5 — Facet menu (pick-any)

**Phase:** V2/V3 · **Guide:** `ITERATION-I5-facet-menu.md` · **Status:** ⬜ · **Depends on:** I1 + I2

Prereq: **FacetCatalog** descriptor plumbing (shared by all picks).

| Pick | Facet | Priority |
|------|-------|----------|
| ★ | **F13 Blast radius** — `query usages --transitive` | First |
| ★ | **F3 Message matrix** — producers → consumers per message type | Early |
| | **F1 Auth surface** — `[anon]`/`[Authorize]` badges + digest | Menu |
| | **F2 Middleware pipeline** — deduplicated, project-grouped | Menu |
| | **F4 Data map** — entity ← entry-kind via ReadsWrites | Menu |
| | **F5 Talks-to** — typed clients, gRPC, Refit | Menu |
| | **F6 Config surface** — `Configure<T>` + section literals | Menu |
| | **F7 Interesting points** — centrality + per-archetype composition | Menu |
| | **F8 DI health** — lifetime histogram, captive-dependency | Menu |
| | **F9 Aspire topology** | Menu |
| | **F10 CLI command tree** | Menu |
| | **F11 Serverless trigger detail** | Menu |
| | **F12 Desktop VM wiring** | Last |

---

## I7 — Benchmark expansion + insights audit (closing loop)

**Phase:** V5 · **Guide:** `ITERATION-I7-benchmark-audit.md` · **Status:** ⬜ · **Depends on:** I3 (+ I5 picks)

| Step | Task |
|------|------|
| I7.1 | Add canonical repos per new shape (CLI, worker, gRPC, Blazor, MAUI, serverless, 2nd library) |
| I7.2 | Register in `eval-repos.json` + expectations + bench suite |
| I7.3 | Run full suite, capture outputs, judge insights quality |
| I7.4 | Ratchet eval expectations, record next-pick recs |

---

## I6 — MCP server (deferred)

**Phase:** V4 · **Guide:** `ITERATION-I6-mcp.md` · **Status:** ⬜ · **Depends on:** I2

| Step | Task |
|------|------|
| I6.1 | `DevContext.Mcp` stdio host — tools = `GraphQuery` ops |
| I6.2 | Contract tests + end-to-end transcript on eShop |

**Note:** User requested this be last or deferred to next iteration.

---

## I8 — Caching & storage (addendum)

**Phase:** V5 · **Guide:** `ITERATION-I8-caching-storage.md` · **Status:** ⬜ · **Depends on:** I2

| Step | Task |
|------|------|
| I8.1 | Snapshot store: repoKey + versionKey hashing, JSON.gz persistence |
| I8.2 | Cache hit path: analyze → load snapshot → serve instantly |
| I8.3 | Clone consolidation: fetch+checkout over re-clone |
| I8.4 | Settings → Storage face: cache list, sizes, clear, disk bar |
| I8.5 | CLI `devcontext cache list/clear/path` |

---

## I9 — Release readiness (addendum)

**Phase:** V7 · **Guide:** `ITERATION-I9-release-readiness.md` · **Status:** ⬜ · **Depends on:** I4, I8

| Step | Task |
|------|------|
| I9.1 | About panel: versions, license, third-party notices |
| I9.2 | Windowing: size/position persisted, single-instance, graceful exit |
| I9.3 | Server supervision: crash→auto-restart, port conflict handling |
| I9.4 | Errors/logs: global error boundary, log files, update check |
| I9.5 | CLI polish: exit codes, stdout/stderr discipline, --quiet, completions |

---

## I10 — Workspace tabs (addendum)

**Phase:** V7 · **Guide:** `ITERATION-I10-workspace-tabs.md` · **Status:** ⬜ · **Depends on:** I4 (+I8 for full cap)

| Step | Task |
|------|------|
| I10.1 | WorkspaceStore: multi-tab state, SessionStore/TraceStore facades |
| I10.2 | Tab strip UX: 32px strip, tab anatomy, + button, interactions |
| I10.3 | Server: MaxLiveSessions + LRU + rehydrate path |
| I10.4 | URL & persistence: active tab view in URL, restore idle tabs |

---

## 10.E — Coverage gaps (not yet benchmarked)

**Status:** ⬜ · Added in I7. The 8 missing archetypes from the cross-repo audit:

| Archetype | Candidate repo |
|-----------|---------------|
| Console/CLI | System.CommandLine sample or DevContext CLI itself |
| Worker/BackgroundService | `dotnet new worker` + MassTransit |
| gRPC service | grpc-dotnet examples |
| Blazor (WASM + Server) | `blazor-samples/9.0/BlazorWebAppMovies` |
| MAUI/Avalonia | Avalonia sample |
| Classic MVC (Razor) | Razor MVC sample |
| Serverless | Azure Functions isolated sample |
| 2nd library | Humanizer or Newtonsoft |

---

## Delivery order (CORE spine + addendum)

```
I1 (trust) ──► I2 (CLI/kernel/W9) ──┬──► I3 (insights) ──┐
                                     ├──► I4 (desktop UX)  ├──► I7 (benchmark/close)
                                     └──► I5 (facet menu)  │
                       ┌─► I8 (cache/storage) ──► I10 (tabs) ──► I9 (release)
                       │                                    │
               I6 (MCP) ←───────────────────────────────────┘ (deferred)

Per-commit delivery: each I1.1–I1.7 step is one commit; each I2 step one commit; etc.
Ratchet rule: flip eval `aspirational` → `expected` in the same commit that fixes the issue.
Do-not-regress: `BudgetIndependenceTests` + `TraceQualityTests` sibling-divergence Facts.

---

## Daily commit protocol

1. Write code → `dotnet build DevContext.slnx` → fix any warnings
2. `dotnet test DevContext.slnx --filter "Category!=Eval"` → must stay green
3. `git add` only intended files → `git commit -m "<type>(<scope>): <what>"` per item
4. Update this tracker's status column
5. Append to `PROGRESS-LOG.md` (date · changed · verified · next)
