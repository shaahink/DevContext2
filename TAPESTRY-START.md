# Tapestry — Phase Tracker (resume here)

**Read order for a fresh session:** this file → `docs/dev/briefs/proposal-tapestry.md` (the plan —
read your stage's section AND §1 rules) → the evidence files your stage cites →
`docs/dev/CODE-MAP.md` (where things live) → `docs/dev/DEVELOPER-PIPELINE.md` (build/gate).
Branch scheme: `feat/tapestry-t<stage>` off `develop`. Never merge unasked.
Dogfood: `C:\Users\shahi\source\repos\run-aspnetcore-microservices\src` · second pole: `C:\code\shamshir`.

## Handoff (overwrite this block, ≤12 lines, no history)
last: 2026-07-15 Tapestry T1.1 delivered on feat/tapestry-t1 — catalog-driven entry seeds. `EntrySeedFiles` was a
hardcoded four (endpoints/MediatR/workers/hubs); gRPC/Functions/Orleans/GraphQL/consumer/CLI entries got NO Map-mode
call-graph seed → targetless, depth-1. Fix: marker `IEntrySurfaceDetection` on every AppEntry detection → EntrySeedFiles
unions their SourceFile (catalog-driven; new surface feeds seeds with no extractor edit; Desktop deferred to T1.7). Plus
Functions/GraphQL entry builders now create+anchor the handler MEMBER (like HTTP/gRPC) so ResolveEntryTarget surfaces
the service (was Type-anchored → only read Sends). New service-app fixture eval/fixtures/ServiceSurfaces (gRPC+Functions+
GraphQL, 3/3 target: SayHello→GreetingService.BuildGreeting etc.) + EntrySurfaceSeedTests + servicesurfaces.json. Fixture
lives under eval/fixtures NOT tests/ so package signals aren't suppressed. Evidence: tapestry-t1/T1-EVIDENCE.md.
Two T1.5 findings recorded (not fixed): OpenApi package = false MinimalApi signal (flips controller apps); DependencyExtractor
suppresses signals for repos rooted under /tests//samples/ (unlike NoiseFilter). eShop/shamshir NOT re-driven (surface-neutral).
stage: T0 + T1.1 + T1.2/T1.3 VERIFIED. wrapup+t0+t1 all await review/merge to develop. Remaining T1: T1.4/T1.5/T1.6 (shamshir+bench), T1.7/T1.8/T1.9 (eShop+UI drive).
next: T1.4 runnable/per-service inference, or T1.5 style arbitration (has 2 ready findings above), or T1.7+T1.8 (entry taxonomy, eShop pole).
gate: build 0w/0e · Core 453P/3S · Server 14P · Truth 0 fail · loom-guards clean · full eval 57P/6S/0 fail (servicesurfaces + 3 seed cases green) · gates.ps1 GATE: PASS (tapestry-t1/gates-t1-final.txt).

---

## Checkpoint table

Status ∈ TODO · IN PROGRESS · DONE · BLOCKED · VERIFIED. Evidence under `eval-results/<date>/`.
A checkpoint without a fresh artifact is not DONE (write BLOCKED with what's missing).

### T0 — Harness & hygiene
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T0.1 | Orphan-proof gates + launcher (Server.Tests teardown, gates.ps1 step 0 + serial McpQa step, start-dev-bg CIM kill/240s/logs) | VERIFIED | f36b66d | tapestry-t0/T0-EVIDENCE.md · gates-run1/2.txt |
| T0.2 | CompositionApp fixture + eval expectations (pins the 2026-07-15 fixes) | VERIFIED | abadb2e | compositionapp.json green · T0-EVIDENCE.md |
| T0.3 | Truth re-baseline: dogfood + shamshir drift table below filled from fresh runs | VERIFIED | (T0.3 commit) | drift table below · bench.ps1 -Truth per-kind |

### T1 — Detection strength
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T1.1 | Catalog-driven EntrySeedFiles (all AppEntry surfaces feed call-graph seeds) | VERIFIED | 5f492d5 | tapestry-t1/T1-EVIDENCE.md · ServiceSurfaces fixture (gRPC/Functions/GraphQL 3/3 target) · servicesurfaces.json + EntrySurfaceSeedTests |
| T1.2 | Gateway archetype rung — yarp eval flips green | VERIFIED | 0f410a9 | tapestry-t1/T1-EVIDENCE.md · yarp Gateway + dogfood App (Truth) |
| T1.3 | dntsite FeedsService entry-target gap — target-* evals flip green | VERIFIED | 3ea6c34 | tapestry-t1/T1-EVIDENCE.md · dntsite eval green + ConventionalController fixture |
| T1.4 | Runnable/per-service inference (Exe + AppHost refs; CLI archetype) | TODO | | |
| T1.5 | Style-ladder arbitration (controllers-heavy ≠ MinimalApi) | TODO | | |
| T1.6 | Feature areas from route prefixes (no more "Api (122 entries)") | TODO | | |
| T1.7 | Entry taxonomy hygiene: gRPC RPC-only · MAUI noise out · Blazor≠HTTP · dup disambiguation (audit A2–A5) | TODO | | |
| T1.8 | Kind single-sourcing: EntryTableProjection joins Entries, no tag default (audit "gRPC 75") | TODO | | |
| T1.9 | Topology noise: tests/samples out of services/depended/dead-code (audit A16/D) | TODO | | |

### T2 — Graph quality
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T2.1 | Production-first DI Resolves (+ test-only tag) | TODO | | |
| T2.2 | Member LineNumber stamping (packs show file:line everywhere) | TODO | | |
| T2.3 | Target quality: Type.Method titles · direct-EF label · mutating-verb guard | TODO | | |
| T2.4 | Type-focus trace shaping (member groups, named omissions) | TODO | | |
| T2.5 | Param-passed dispatch seam: BodyFacts params + resolver fallback + receiver normalization (audit A1) | TODO | | |
| T2.6 | One event join: board/one-pager/flow from Graph2 seams; legacy joins deleted (audit A10) | TODO | | |
| T2.7 | `global` display fallback namespace→project→folder (audit A7) | TODO | | |
| T2.8 | Old-graph retirement cleanup: tags · stale comments · GraphBuilder split (audit §0b) | TODO | | |

### T3 — MCP v3
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T3.1 | Unified symbol addressing (query accepted everywhere; envelopes ≤80 tok) | TODO | | |
| T3.2 | entrypoints summary default ≤1.5k tok (full:true escape) | TODO | | |
| T3.3 | trace budgetTokens (default ~4k, named omissions, deep-links) | TODO | | |
| T3.4 | config latency ≤500ms warm (was 10.5s on shamshir) | TODO | | |
| T3.5 | Repo-relative paths + Start-here noise filter | TODO | | |
| T3.6 | Self-describing heuristics (tests_for/config method note; flow-vs-trace docs) | TODO | | |
| T3.7 | CLI query parity: entrypoints/stats/trace implemented, kernel JSON envelope (audit A15) | TODO | | |
| T3.8 | Report hygiene: telemetry behind --stats · surface cap · repo-derived footer (audit C5/D) | TODO | | |

### T4 — Context generation v2
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T4.1 | Pack identity header + repo-relative file:line locations | TODO | | |
| T4.2 | Budget utilization ≥85% (spine-first body expansion, +N-lines markers) | TODO | | |
| T4.3 | config + tests sections server-side (R9) | TODO | | |
| T4.4 | Per-section provenance + tier mix (R10) | TODO | | |
| T4.5 | VerifyContextPack staleness API (R6 engine half) + MCP verify_context | TODO | | |
| T4.6 | Pack assembly correctness: contracts≠signatures · empty sections omitted · archetype header (audit C2) | TODO | | |

### T5 — Context Studio v2
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T5.1 | R1 omitted list · R4 error state · R5 save extension | TODO | | |
| T5.2 | Verification panel (R6 UI) | TODO | | |
| T5.3 | R7 per-card copy · R8 JSON export · file:line provenance chips | TODO | | |
| T5.4 | Worker/hub presets | TODO | | |
| T5.5 | Card content honesty: real previews (no title echo) · per-card provenance · one unit · scope-error tooltips | TODO | | |
| T5.6 | Studio recompute-on-change: budget/format re-pack · plain≠markdown · save name · preset/Add UX (audit C1) | TODO | | |

### T6 — Workbench & pages revamp
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T6.0 | Full 7-page UI audit vs dogfood AND shamshir (eShop pole DONE 2026-07-15; shamshir pole remains) | IN PROGRESS | c3bcb9a | eval-results/2026-07-15/feature-design-audit.md |
| T6.1 | Home/Atlas honest on monolith+workers repos | TODO | | |
| T6.2 | Canvas revamp: entry-kind glyphs, tier-styled edges, archetype lens defaults | TODO | | |
| T6.3 | Insights honesty: noise thresholds + archetype-aware copy (no "Desktop apps" on web repos) | TODO | | |
| T6.4 | MCP page multi-session truth · Settings storage truth | TODO | | |
| T6.5 | Keyboard reality: wire h/e/a/i/m/c/s nav (or drop affordance) · route-restore `/` bug · drive-gate kbd battery | TODO | | |
| T6.6 | Theme parity: shell follows light mode · 3 vibes × 3 modes screenshot matrix in the gate | TODO | | |
| T6.7 | Hero graphs draw edges (service-map-hero reuses Service-lens canvas; MAP header chips) (audit B1) | TODO | | |
| T6.8 | Names/paths/metrics: no last-segment names · repo-relative paths · metric tooltips (audit A8/A14/B5/B6) | TODO | | |
| T6.9 | First-run & session: deck sort · Trace-checkout target · persistent tiles · session reattach (audit B2–B4) | TODO | | |
| T6.10 | MCP page ergonomics: full handle + use-button · feed origin filter (audit B9) | TODO | | |
| T6.11 | One-pager fidelity: rides T2.6+T6.8 · file download · eShop golden (audit C3) | TODO | | |

### T7 — Bench + perf honesty
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T7.1 | Bench + truth files extended (CompositionApp, gRPC service, aspire-samples) | TODO | | |
| T7.2 | Perf baseline + edge-explosion check (devcontext-bench) | TODO | | |
| T7.3 | Stage waterfall ≥95% wall-time accounted | TODO | | |
| T7.4 | Page-render RPC budget ≤15/navigation; server flow/facet memo (audit B11) | TODO | | |

### T8 — Close-out
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T8.1 | Clean-clone full battery + HANDOVER-TAPESTRY.md + AGENTS/memory updates | TODO | | |

---

## Baseline drift table (R-T8 — update at every stage end)

| Repo | Date | Nodes | Edges | Entries (by kind) | Style | Notes |
|---|---|---|---|---|---|---|
| dogfood | 2026-07-15 | 432 | 330 | 34 | Microservices | mcp-qa.md M4 snapshot |
| shamshir | 2026-07-15 | 2804 | 3301 | 128 = HTTP 122 · Background 4 · SignalR 2 | MinimalApi (moderate; wrong — T1.5) | post-fix v3 map + MCP overview |
| dogfood | 2026-07-15 T0 | 432 | 330 | 34 = HTTP 27 · gRPC 4 · +3 per-svc | Microservices | **T0.3 baseline.** Identical to prior row → engine unchanged by T0 (`analyze --no-cache`). |
| shamshir | 2026-07-15 T0 | 2850 | 3349 | 135 = HTTP 128 · Background 5 · SignalR 2 | MinimalApi (moderate; wrong — T1.5) | **T0.3 baseline.** +46 nodes/+48 edges/+7 entries vs prior. Engine is byte-identical on dogfood, so the delta is shamshir's own source moving (live repo), not a regression. |
| dogfood | 2026-07-15 T1 | 432 | 330 | 34 | Microservices (App) | **T1.2+T1.3 baseline.** Unchanged from T0.3 — both change archetype SELECTION and a target DISPLAY string, not graph structure; `Dogfood_baseline_presence_ok` green. shamshir NOT re-driven this session (structurally neutral: no gateway signal; the target-title fix only affects display, not counts). |
| dogfood | 2026-07-15 T1.1 | 439 | 339 | 34 | Microservices (App) | **T1.1.** +7 nodes / +9 edges vs T0.3 (432/330), entries unchanged — the seed change binds the Discount gRPC service files, so its RPC members gain Calls edges (strictly additive: deepens, never removes). Measured by the McpQa gate (mcp-qa.md): checkout flow trace deepened **43 → 46 steps** (1324 → 1442 tok). Truth `Dogfood_baseline_presence_ok` + MCP QA green; no eval-repo count/archetype/style eval moved (57P/0F). shamshir surface-neutral (its HTTP/SignalR/worker mix was already seeded). |

## Quick commands

```powershell
# Pre-session orphan-kill is now automatic (gates.ps1 Step 0, T0.1 landed). Manual form if ever needed:
Get-Process DevContext.Server,testhost -ErrorAction SilentlyContinue | Stop-Process -Force

dotnet build DevContext.slnx                                   # 0w/0e is the bar
dotnet test DevContext.slnx --filter "Category!=Eval"
dotnet test DevContext.slnx --filter "Category=Truth"
powershell -File scripts/loom-guards.ps1
powershell -File eval/gates.ps1
cd src/DevContext.App; pnpm check                              # app gate
node eval/mcp-qa/run-cold.js --repo <path>                     # cold-agent QA
node src/DevContext.App/scripts/ui-audit-drive.mjs             # UI gate (server+ng required)
dotnet src\DevContext.Cli\bin\Debug\net10.0\DevContext.Cli.dll analyze C:\code\shamshir --stats
```
