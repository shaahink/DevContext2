# Tapestry — Phase Tracker (resume here)

**Read order for a fresh session:** this file → `docs/dev/briefs/proposal-tapestry.md` (the plan —
read your stage's section AND §1 rules) → the evidence files your stage cites →
`docs/dev/CODE-MAP.md` (where things live) → `docs/dev/DEVELOPER-PIPELINE.md` (build/gate).
Branch scheme: `feat/tapestry-t<stage>` off `develop`. Never merge unasked.
Dogfood: `C:\Users\shahi\source\repos\run-aspnetcore-microservices\src` · second pole: `C:\code\shamshir`.

## Handoff (overwrite this block, ≤12 lines, no history)
last: 2026-07-16 **T4 OPENED — T4.1 + T4.6 VERIFIED on feat/tapestry-t4** (off develop @ 0eca05f, one commit — same-session pair per addendum).
T4.1: pack identity header (`# {repo} — {focus}` + `analyzed {utc} · HEAD {sha7}`; repo = solution name, falls back to root folder) +
repo-rel `file:line` on every located node (no trailing colon; salient-body headings carry `— file:line`). ROOT CAUSES: snapshot.Explanation
is NEVER populated by the pipeline (the audit's empty `# ` title AND `_Archetype: _` — pack no longer reads it; archetype from Map),
no AnalyzedAtUtc/GitHead on the snapshot (added; GitHeadReader extracted from SnapshotCacheService's private ComputeGitHead), and
`int?` LineNumber interpolated blind (dangling `path:`). T4.6: BuildContracts (role-tag messages + interfaces + DTOs; entities excluded)
replaces the contracts→["signatures"] alias; empty sections/cards dropped AND recorded in omitted[]; `<!-- context card -->` out of the copy.
Engine-only — the app sends the same card types; the client-side fallback builder in context-studio.ts stays stale until T5.6.
stage: **T4 2/6 (T4.1, T4.6).** Evidence eval-results/2026-07-16/tapestry-t4/ (pack-dogfood-checkout.json = live MCP get_context capture).
next: **T4.2 budget utilization** (Q7's 612/4000 under-fill → spine-first body expansion to ~85%, `… (+N lines)` markers), then T4.3 config+tests
sections server-side (R9) → T4.4 per-section provenance+tier mix → T4.5 VerifyContextPack+MCP verify_context. develop still lives in the
C:/Code/DevContext2-ui worktree — advance it there via `git -C`, never `git checkout develop` in main.
gate: gates-t4.txt = GATE: PASS (build/fast/McpQa serial/eval/CLI matrix + 4b) + loom-guards PASS (truth 0 failures).

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
| T1.4 | Runnable/per-service inference (Exe + AppHost refs; CLI archetype) | VERIFIED | 2e7747b | eShop per-service all honest (MAUI/Worker/Blazor/gRPC/Web API, 0 Unknown); shamshir 4 runnables (AppHost/Host/ResearchCli/Web) up from 2 · PerServiceStyleTests |
| T1.5 | Style-ladder arbitration (controllers-heavy ≠ MinimalApi) | VERIFIED | 0df9778 | OpenAPI≠minimal-apis + root-relative suppression; shamshir MinimalApi→NLayer · EntrySurfaceCatalogTests · eval 30P/0F (TodoApi still MinimalApi) |
| T1.6 | Feature areas from route prefixes (no more "Api (122 entries)") | VERIFIED | 785f2f8 | shamshir "Api (128)"→8 feature areas (runs/data-manager/system/…); eShop catalog/orders · GraphBuilderTests |
| T1.7 | Entry taxonomy hygiene: gRPC RPC-only · MAUI noise out · Blazor≠HTTP · dup disambiguation (audit A2–A5) | VERIFIED | 631709d | eShop gRPC 20→3 (ViewModelBase false-pos killed); Blazor→UiEntry (anon 49/56→36/43); catalog v1/v2 disambiguated · GraphBuilderTests + EntrySurfaceSeedTests + eshop.json pins |
| T1.8 | Kind single-sourcing: EntryTableProjection joins Entries, no tag default (audit "gRPC 75") | VERIFIED | ff89fda | CodeGraph.Entries → projection joins true kind; DeriveEntryKind/PublicApi default deleted · GraphProjectionTests + EntrySurfaceSeedTests |
| T1.9 | Topology noise: tests/samples out of services/depended/dead-code (audit A16/D) | VERIFIED | 0aff0e1 | ProjectClassifier.IsProductionProject; eShop 11 runnable services (no test cards); MediatR most-depended = MediatR not .Examples · TopologyNoiseTests |

### T2 — Graph quality
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T2.1 | Production-first DI Resolves (+ test-only tag) — prefer prod registration; test-only edge tagged + rendered "[test-only registration]" | VERIFIED | (T2.1 commit) | GraphBuilderTests.DiResolve_wired_only_from_a_test_project_is_tagged_test_only; NoiseFilter.IsProductionEntrySource (no regex) |
| T2.2 | Member LineNumber stamping — BodyFacts.DeclLine → seam-origin + entry-handler members; no trailing colon | VERIFIED | (T2.2 commit) | BodyFactExtractorTests.Body_facts_carry_the_member_declaration_line + SeamDetectorTests (origin node LineNumber); repro trace members all file:line |
| T2.3 | Target quality: Type.Method titles (already TargetTitle/T1.3) · direct-EF label · mutating-verb getter guard | VERIFIED | (T2.3 commit) | GraphBuilderTests.EntryTarget_mutating_verb_prefers_a_non_getter_service_call + EntryTarget_labels_direct_data_access_when_only_the_dbcontext_is_called |
| T2.4 | Type-focus trace shaping — members as branches (top-N by out-degree), named omission | VERIFIED | (T2.4 commit) | GraphBuilderTests.TypeFocus_trace_groups_by_member_not_a_flat_wall_of_callees; repro Orchestrator focus 15-op "(18 branches)"→member groups + "(3 omitted)" |
| T2.5 | Param-passed dispatch seam: receiver normalization + global-namespace member-id fix (audit A1) | VERIFIED | (T2.5 commit) | tapestry-t2/T2.5-EVIDENCE.md · eshop-draft-trace-{before,after}.md · gates-t2.5.txt · SeamDetectorTests + BodyFactExtractorTests |
| T2.6 | One event join: board/one-pager/flow from Graph2 seams; legacy joins deleted (audit A10) | VERIFIED | (T2.6 commit) | tapestry-t2/T2.6-EVIDENCE.md · EventWiringTests (6, incl. board/one-pager/flow agree) · eshop.json integration-event-rows(≥8)+cross-service-events(≥1) · eShop 13 integration events / 8 cross-service, zero drift (1089/837/109) |
| T2.7 | `global` never rendered — display fallback namespace→folder; regression-lock after T2.5's global.* ids (audit A7) | VERIFIED | (T2.7 commit) | NamespaceDisplayTests + LibrarySurfaceBuilderTests (global→folder label); engine Map/report/pack verified `global`-clean on eShop |
| T2.8 | Old-graph retirement cleanup: tags · stale comments · GraphBuilder split (audit §0b) | VERIFIED | (T2.8 commit) | tapestry-t2/T2.8-EVIDENCE.md · GraphBuilder.cs 2484→6 partial files (main 118 + Flows/Entries/Nodes/Seams/ServiceLinks); no `kind:` parsing remains (verified); stale AddSends comment removed; **dogfood drift byte-identical** 439/339/34/205/11 before=after; fast 497P/0F |

### T3 — MCP v3
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T3.1 | Unified symbol addressing (query accepted everywhere; envelopes ≤80 tok) | VERIFIED | (T3.1 commit) | tapestry-t3/T3-MCP-EVIDENCE.md · run.js q8 (impact/node/read_source via query) PASS · ResolveQueryAsync ambiguity-honest · 8 tools + query synonym |
| T3.2 | entrypoints summary default ≤1.5k tok (full:true escape) | VERIFIED | (T3.2 commit) | tapestry-t3/T3-MCP-EVIDENCE.md · q9 byKind + top-15, 843 tok summary, full:true→34 (==count) |
| T3.3 | trace budgetTokens (default ~4k, named omissions, deep-links) | VERIFIED | (T3.3 commit) | proto TraceRequest.budget_tokens; GraphQuery.Trace(budgetTokens); TraceBuilder.ShapeToBudget (BFS keep, named per-subtree omissions); MCP trace budgetTokens=4000 + deep-link hint. TraceBuilderTests (cut+named+root-kept; 0=unlimited). McpQa q11: budget400→11 steps/323tok omitted=19 vs full 46/1467. pnpm check 0 (proto regen also synced stale ArchetypeView) |
| T3.4 | config latency ≤500ms warm (was 10.5s on shamshir) | VERIFIED | (T3.4 commit) | ConfigScanner extracted (Core) + cached once per session (AnalysisSession.ConfigBindings, `??=`); ConfigLookup filters in-memory. ConfigScannerTests (keys across patterns + missing-file safe). McpQa q6 PASS via cache. shamshir warm-latency measured in delivery drive |
| T3.5 | Repo-relative paths + Start-here noise filter | VERIFIED | (T3.5 commit) | GraphQueryTests.GetInterestingPoints_excludes_framework_and_store_noise · GetInterestingPoints filters System.*/BCL/Store; ContextPackBuilder pack Location repo-relative (RootPath) · gates-t3.5.txt GATE: PASS eval 58P/6S/0F, overview intact |
| T3.6 | Self-describing heuristics (tests_for/config method note; flow-vs-trace docs) | VERIFIED | (T3.6 commit) | tapestry-t3/T3-MCP-EVIDENCE.md · q10 method note tests_for+config · flow/trace cross-doc |
| T3.7 | CLI query parity: entrypoints/stats/trace implemented, kernel JSON envelope (audit A15) | VERIFIED | (T3.7 commit) | QueryCommand entrypoints/stats/trace run against GraphQuery (MCP-shaped JSON), no longer fall to overview render; gates.ps1 Step 4b asserts each op; cli-query-ops.txt (entrypoints 2/byKind, stats 16n/9e, trace no-focus exit 1 + focus found) |
| T3.8 | Report hygiene: telemetry behind --stats · surface cap · repo-derived footer (audit C5/D) | VERIFIED | (T3.8 commit) | **MassTransit report 476KB→34.5KB (<40KB gate)**; PUBLIC SURFACE capped 25 ns × 12 types + "…N more (--format json)"; no Run Report in default output (0); map footer + library footer derive from repo's OWN top entry (goldens: POST /api/orders/→POST /orders / GET /products). LibrarySurfaceRendererTests (cap) + regenerated goldens. masstransit-report.md |

### T4 — Context generation v2
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T4.1 | Pack identity header + repo-relative file:line locations | VERIFIED | 0d47247 | tapestry-t4/T4.1-EVIDENCE.md · pack-dogfood-checkout.json (`# eshop-microservices — POST /basket/checkout` + `analyzed … · HEAD 7e43d80`; every located node repo-rel `file:line`, no trailing colon, no abs paths) · ContextPackBuilderTests (5) · root causes: snapshot.Explanation never populated (empty `# ` title) + `int?` LineNumber rendering `path:` |
| T4.2 | Budget utilization ≥85% (spine-first body expansion, +N-lines markers) | VERIFIED | 8099bd6 | tapestry-t4/T4.2-EVIDENCE.md · dogfood 88.8% (was 35%), shamshir 99.4% @ 4k budget, bodies dominant (2172/2368 tok) · BuildBodiesToFill BFS full-body expansion + `… (+N lines)` markers + omitted[] counts · depth scales with budget (4→6 @ ≥3k) · skeleton/signatures capped (shamshir sigs 2475→1000 + `+33 more members`) · Bodies_expand test |
| T4.3 | config + tests sections server-side (R9) | VERIFIED | 9f60263 | tapestry-t4/T4.3-EVIDENCE.md · BuildConfigSection (spine-file ConfigScanner subset) + BuildTestsSection (FindCallers + TestHeuristics, best-effort note, capped) · shamshir tests=467tok real rows w/ file:line; dogfood both honestly omitted · **pre-existing bug fixed: IsLikelyTestMethod matched /tests/ on ABSOLUTE paths (fixture under tests/fixtures → 794tok phantom tests card; latent in tests_for MCP) — now root-relative, one source Core/Graph/TestHeuristics** · Config/Tests unit tests + assembly gate |
| T4.4 | Per-section provenance + tier mix (R10) | VERIFIED | d15d0aa | tapestry-t4/T4.4-EVIDENCE.md · SectionProvenance per rendered item → `_provenance: N source sites · V verified · A approx_` footer + structured SectionAllocation.SourceLocations/Verified/Approx (proto exposure rides T4.5) · dogfood 90.5% fill maintained · Sections_carry_provenance test |
| T4.5 | VerifyContextPack staleness API (R6 engine half) + MCP verify_context | VERIFIED | (T4.5 commit) | tapestry-t4/T4.5-EVIDENCE.md · snapshot FileFingerprints (sha256+lines, node-bearing files) · ContextPackVerifier per-section vs disk (modified/deleted/unknown; unknown ≠ stale) · VerifyContext RPC + T4.4 provenance proto fields (one regen, pnpm check 27P) · MCP verify_context (24th tool) · live dogfood drift cycle fresh=false→edit=true→restore=false (verify-context-dogfood.json) · ContextPackVerifierTests end-to-end on temp fixture copy |
| T4.6 | Pack assembly correctness: contracts≠signatures · empty sections omitted · archetype header (audit C2) | VERIFIED | 0d47247 | tapestry-t4/T4.6-EVIDENCE.md · BuildContracts (role-tag messages + interfaces + DTOs, entities excluded) — dogfood contracts=174tok distinct from signatures=445 · empty sections/cards dropped + recorded in omitted[] · `<!-- context card -->` out of AssembledMarkdown · ContextPackAssemblyTests (CompositionApp full-pipeline gate: contracts≠signatures, archetype filled, no `, 0 tok_`, no `<!--`) |

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
| eShop | 2026-07-15 T1 | 1092 | 833 | 109 = HTTP 43 · Bus 13 · Background 1 · Domain 7 · UI 42 · gRPC 3 | Microservices (0.91) | **T1.4–T1.9 pole.** gRPC 20→3 (T1.7 killed ViewModelBase false-positives + private helpers); Blazor @page moved HTTP→UI (anon-endpoints 49/56→36/43); per-service all honest (MAUI/Worker/Blazor/Web API/gRPC, 0 Unknown); services production-only (11 runnable, 5 test projects out); module map catalog/orders (route-derived). eshop.json pins gRPC (3) + UI (. |
| shamshir | 2026-07-15 T1 | 2882 | 3375 | 135 = HTTP 128 · Background 5 · SignalR 2 | **NLayer (0.6)** | **T1.4–T1.9 pole.** Style MinimalApi→**NLayer** (T1.5 dropped the false OpenAPI minimal-apis signal); per-service 2→4 runnables (AppHost/Host-Worker/ResearchCli-CLI/Web); module map **"Api (128)"→8 feature areas** (runs 22 · data-manager 15 · system 8 · strategies 8 · research 8 · …). +32 nodes/+26 edges vs T0.3 (2850/3349) = live-repo source drift <1.2%, entries unchanged; NO count regression. |
| eShop | 2026-07-15 T2.6 | 1089 | 837 | 109 | Microservices (0.91) | **T2.6 pole.** UNCHANGED vs T2.5 (1089/837/109). The one `EventWiringProjection` replaces the legacy `AddBusServiceLinks` join and emits the identical bus ServiceLink set (5 links, 8 cross-service integration events collapsing to 5 distinct service pairs), so graph structure is byte-neutral. New: `graph.EventWiring` (20 events, 13 integration, 8 cross-service, 0 orphan) surfaced in `EVENT WIRING` map section + JSON `$.eventWiring`. `gateway.downstream-wiring` unchanged (4 targets). Setting Project on consumer/handler nodes + richer ServiceLink provenance did not move counts. |
| eShop | 2026-07-15 T2.5 | 1089 | 837 | 109 | Microservices (0.91) | **T2.5 pole.** vs T1 (1092/833): nodes **−3** (orphan duplicate member nodes merged — the global-namespace member-id fix unifies the seam origin with the entry's handler node), edges **+4** (new/connected `Sends` — property-accessed `services.Mediator.Send` on OrdersApi's draft/cancel/ship/create endpoints; "Sends only rise"), entries 109 unchanged. eShop /draft trace 2 nodes → deep (entry→send CreateOrderDraftCommand [verified]→handler→data Order). Dogfood MCP-QA checkout unchanged (no regression); eval 58P/6S/0F. |

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
