# Tapestry — Phase Tracker (resume here)

**Read order for a fresh session:** this file → `docs/dev/briefs/proposal-tapestry.md` (the plan —
read your stage's section AND §1 rules) → the evidence files your stage cites →
`docs/dev/CODE-MAP.md` (where things live) → `docs/dev/DEVELOPER-PIPELINE.md` (build/gate).
Branch scheme: `feat/tapestry-t<stage>` off `develop`. Never merge unasked.
Dogfood: `C:\Users\shahi\source\repos\run-aspnetcore-microservices\src` · second pole: `C:\code\shamshir`.

## Handoff (overwrite this block, ≤12 lines, no history)
last: 2026-07-17 **T7 COMPLETE — T7.0–T7.4 on feat/tapestry-t7** (T7.0 91533db): T7.1 bench+truth (3 new poles CompositionApp/GrpcAggregator/aspire-samples;
25/25 verdict refresh; 2 engine fixes — dot-dir sln decoy `.github/for.dependabot.only.sln` + picker scores all candidates; aspire markdown render pinned
TruthPending(T8)) → T7.3 waterfall (4 new PipelineStage rows; shamshir 99.7% accounted, SemanticLite=21.2s WAS the invisible half; fingerprints inside the
clock) → T7.2 perf (PERF-2026-07-17-0036 vs Jun-20: big-repo wall = SemanticLite, DntSite 81.1s of 94.7s; bind itself 84s→3.2s; OrchardCore edges
1708→11904 = coverage, bind flat; named lever: persist/reuse merged compilation) → T7.4 RPC budget (GetFlowIndex RPC + Core FlowIndexBuilder + session memo;
AtlasStore = ONE call; drive M: fresh load **8 RPCs** was ~150+, navs 0–1; fixed client boundary-seam name bug). Evidence: eval-results/2026-07-16/tapestry-t7/.
next: **T8 close-out** (clean-clone battery + HANDOVER-TAPESTRY + AGENTS/memory) — plus the T8-pinned finding: sample-collection RENDER honesty
(aspire-samples: samples/ noise-suppression ⇒ 0 entries ⇒ Library "0 public types" ⇒ no STYLE line). Stage battery gates-t7-stage.txt — check GATE: PASS
before merge. Standing: first `.github/workflows/eval.yml` run when github-ready merges. develop lives in C:/Code/DevContext2-ui worktree (`git -C`).
gotchas: fast suite has a load-flake (1 test fails ONCE right after other dotnet churn, green quiet, name uncaptured — watch battery logs); bench.ps1 is
ASCII now (PS 5.1 read em-dash tail 0x94 as cp1252 curly-quote INSIDE a string ⇒ parse fail when detached); dogfood PRE-EXISTING mods stand — never restore.

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
| T4.5 | VerifyContextPack staleness API (R6 engine half) + MCP verify_context | VERIFIED | a010229 | tapestry-t4/T4.5-EVIDENCE.md · snapshot FileFingerprints (sha256+lines, node-bearing files) · ContextPackVerifier per-section vs disk (modified/deleted/unknown; unknown ≠ stale) · VerifyContext RPC + T4.4 provenance proto fields (one regen, pnpm check 27P) · MCP verify_context (24th tool) · live dogfood drift cycle fresh=false→edit=true→restore=false (verify-context-dogfood.json) · ContextPackVerifierTests end-to-end on temp fixture copy |
| T4.6 | Pack assembly correctness: contracts≠signatures · empty sections omitted · archetype header (audit C2) | VERIFIED | 0d47247 | tapestry-t4/T4.6-EVIDENCE.md · BuildContracts (role-tag messages + interfaces + DTOs, entities excluded) — dogfood contracts=174tok distinct from signatures=445 · empty sections/cards dropped + recorded in omitted[] · `<!-- context card -->` out of AssembledMarkdown · ContextPackAssemblyTests (CompositionApp full-pipeline gate: contracts≠signatures, archetype filled, no `, 0 tok_`, no `<!--`) |

### T5 — Context Studio v2
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T5.1 | R1 omitted list · R4 error state · R5 save extension | VERIFIED | 89c2797 | tapestry-t5/T5.1-EVIDENCE.md · **root cause: BuildMulti DISCARDED BuildSections' omission reasons — pack omitted[] was always empty**; now propagated (focus-attributed, deduped, cap 12) + `Multi_pack_propagates_section_omission_reasons` · R4 error strip + Retry (old catch mutated cards in place — no signal fire) · R5 .txt/.md · 4 vitest specs · ui-audit-drive E/F armed, **6/6 PASS (first all-green drive)** · gates-t5.1.txt GATE: PASS (eval 58P/6S/0F) |
| T5.2 | Verification panel (R6 UI) | VERIFIED | fea8973 + fix | tapestry-t5/T5.2-EVIDENCE.md · VerificationPanel in the budget rail (fresh/stale banner · per-section ✓/⚠ rows · changed files w/ line delta · HEAD drift · Refresh + Re-analyze) · auto-verify after every re-pack, merged across focuses, advisory-on-failure · **live-drive catch: Build() never ran ResolveFocus — VerifyContext(focus=nodeId) traced null → identity-only → 0 files checked → eternal "fresh"; fixed at Build() (T3.1 addressing) + `Build_resolves_a_node_id_focus_like_a_title`** · drive I-verification-stale-cycle PASS (fresh→disk edit→stale→restore→fresh, shots 16-18) · 3 specs |
| T5.3 | R7 per-card copy · R8 JSON export · file:line provenance chips | VERIFIED | e1f3c3b | tapestry-t5/T5.3-EVIDENCE.md · proto `SectionAllocation.content=6` (sections were counts about text the client never received) + one regen · per-card copy (pack-shaped, real content) · json = third format: structured pack (cards/sections/provenance/omitted/verification/markdown) `${repo}-context-${date}.json` · chips = the card's OWN sourceLocations (tail text, full path title, click copies file:line; code-pane nav deferred to T6.8) · NUL-byte gotcha pinned |
| T5.4 | Worker/hub presets | VERIFIED | 4150e46 | tapestry-t5/T5.4-EVIDENCE.md · "I'm changing this entry" + kind-aware `presetSeedsFor`: SignalRHub → hub flow/orchestrator bodies/consumer wiring/messages/tests · worker kinds → flow/bodies/config-read/messages/tests · endpoints unchanged · 3 specs |
| T5.5 | Card content honesty: real previews (no title echo) · per-card provenance · one unit · scope-error tooltips | VERIFIED | 98d7fa1 | tapestry-t5/T5.5-EVIDENCE.md · previews = real section text (finding 40 title echo dead) · ~NL → ~N tok (one unit) · finding 50 was COLOR SEMANTICS: GrpcService kind glyph was danger-red (read as error badge) → accent-dim + kind [title] tooltips; bonus: `--vibe-accent-pink` never existed (di_wiring badge colorless) → accent-dim, tests → warn · per-card provenance rode T5.3 |
| T5.6 | Studio recompute-on-change: budget/format re-pack · plain≠markdown · save name · preset/Add UX (audit C1) | VERIFIED | cc05b46 | tapestry-t5/T5.6-EVIDENCE.md · schedulePack (350ms debounce, seq-guarded) on add/remove/reorder/retry/budget/intent, ALWAYS the whole card set (pack was latest-batch-only before) · ONE buildContext path — legacy client fallback DELETED, exports = server pack or disabled (exportReady/"Packing…"), failure clears pack · plain strips syntax keeps content · save `${repo}-context-${date}.{md\|txt}` · preset disabled @0 + "Add N to context" primary · config/tests specs now server-side (T4.3) · 10 vitest specs (37P) · drive **8/8 PASS**: G copy@4k(10749ch/"Budget: 4000")≠copy@1k(7158ch/"Budget: 1000"), H plain≠markdown · Tauri smoke alive (L6 rule) · dotnet delta zero vs gates-t5.1.txt |

### T6 — Workbench & pages revamp
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T6.0 | Full 7-page UI audit vs dogfood AND shamshir (both poles done) | VERIFIED | 7bb4c8f | eval-results/2026-07-15/feature-design-audit.md · tapestry-t6/ui-pages-audit.md + shamshir-shots/ (16/16 drive) |
| T6.1 | Home/Atlas honest on monolith+workers repos (.claude worktree exclusion, hub-radar noise filter) | VERIFIED | 2e5da08 | tapestry-t6/T6-EVIDENCE.md |
| T6.2 | Canvas revamp: entry-kind glyphs on trace roots · approx hops dashed / verified solid (trace+neighbors) | VERIFIED | (T6 batch) | graph-canvas KIND_GLYPHS + `edge[?approx]` · feature-drive/02-trace-hero.png |
| T6.3 | Insights honesty: Desktop/Library/Gateway sources archetype-gated · tier-first ranking · tier words (%. → tooltip) · writes-only validation · VM-View self-suppress · hubs ≥3-refs floor | VERIFIED | (T6 batch) | InsightHonestyTests (5) · drive tier-words + archetype-copy PASS · 04-insights-eshop.png |
| T6.4 | Settings→Server shows live serverBaseUrl() + health target (audit B12) | VERIFIED | (T6 batch) | drive live-server-url PASS · 05-settings-server.png |
| T6.5 | Keyboard reality: single-key nav wired · route-restore `/` guard · kbd drive battery | VERIFIED | 6a73546 | keyboard-observations.json · drive battery |
| T6.6 | Theme parity: shell follows light mode · vibe×mode screenshot matrix | VERIFIED | 6a73546 | theme drive battery (committed with T6.5) |
| T6.7 | Hero graphs draw edges (service-map-hero reuses canvas; MAP header chips) (audit B1) | VERIFIED | 296fb39 | drive hero-canvas + no-joined-tfms PASS · 01-home-hero-eshop.png |
| T6.8 | Names/paths: display names (296fb39) · repo-relative entry provenance · deck middle-ellipsis keeps route tails | VERIFIED | (T6 batch) | drive no-abs-paths-explore = 0 |
| T6.9 | First-run & session: deck wired-and-deep first · trace tile = deep checkout else deepest request-shaped flow (eShop checkout entries are ALL 1-hop client commands) · tiles persist · boot reattach (tryAdopt, no re-analyze) | VERIFIED | (T6 batch) | drive trace-hero-deep (≥3 hops) + reattach-no-reanalyze (Analyze 1→1) + tiles-on-revisit |
| T6.10 | MCP ergonomics: ToolCallEvent.origin (grpc-web=ui / native=agent) · feed defaults agents-only · full-handle copy + use-prefill · try-a-tool green | VERIFIED | (T6 batch) | drive T6.10 ×3 PASS · 06-mcp-page.png |
| T6.11 | One-pager fidelity: EventWiringFacet (T2.6 join server-side) · board+one-pager render it · cross-service labels · file download | VERIFIED | (T6 batch) | drive T6.11 ×4 PASS (24 services, 15 event rows) · eshop-onepager.md |

### T7 — Bench + perf honesty
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T7.0 | Harness wall-speed rider: gates.ps1 -Scope full/engine/app + engine-stamp eval cache + 2-host eval split + pnpm check as Step 5 · launcher rewrite (pid files, tree-kill, idempotent, no handle-inheritance hang) · .gitignore de-mojibake (UTF-16 tail made TestResults/.dev-logs rules dead) · overlap-the-battery rule written (skill+docs+AGENTS) | VERIFIED | 1654a49 | gates-t7.0-full-run2.txt **GATE: PASS in 13m23s** (was ~25 min serial; eval 12m29s→8m03s split; -Scope app 1m29s; stamp-cached full ≈5½ min) · run-1 catch: PS 5.1 .ExitCode reads $null w/o cached .Handle — green hosts scored FAIL (fixed, pinned in skill gotchas) |
| T7.1 | Bench + truth files extended (CompositionApp, gRPC service, aspire-samples) | VERIFIED | (T7.1 commit) | tapestry-t7/T7.1-EVIDENCE.md · 25-repo verdicts refresh (bench-t7.1.txt, truth 12P/3S/0F) · 2 engine fixes forced by new poles (dot-dir sln decoy + scaffolding-over-product pick, SolutionDiscoveryExtractorTests 8/8) · aspire markdown-render honesty pinned TruthPending(T8) |
| T7.2 | Perf baseline + edge-explosion check (devcontext-bench) | VERIFIED | (T7.2 commit) | tapestry-t7/T7.2-EVIDENCE.md · PERF-2026-07-17-0036.md vs Jun-20 baseline · big-repo wall = SemanticLite (DntSite 81.1s of 94.7s; bind itself 84s→3.2s) · OrchardCore edges 1708→11904 = coverage not fabrication, bind flat 5.1s · named lever: persist/reuse merged compilation (own checkpoint) |
| T7.3 | Stage waterfall ≥95% wall-time accounted | VERIFIED | (T7.3 commit) | tapestry-t7/T7.3-EVIDENCE.md · shamshir 99.7% (SemanticLite=21.2s was the invisible half) · dogfood 96.8% · Snapshot stage now inside the wall clock · StageWaterfallTests |
| T7.4 | Page-render RPC budget ≤15/navigation; server flow/facet memo (audit B11) | VERIFIED | (T7.4 commit) | tapestry-t7/T7.4-EVIDENCE.md · GetFlowIndex RPC + FlowIndexBuilder (Core) + session memo; AtlasStore = ONE call (worker queue + getNode fan-out deleted) · drive M: fresh load **8 RPCs** (was ~150+), navs 0–1 (bar ≤15) · 13/13 drive PASS · fixed client boundary-seam name bug (only 'send' ever scored) |

### T8 — Close-out
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T8.1 | Clean-clone full battery + HANDOVER-TAPESTRY.md + AGENTS/memory updates | IN PROGRESS | | tapestry-t8/gates-t8-tip.txt (tip battery) · gates-t8-cleanclone.txt (post-merge clean clone) · docs/dev/HANDOVER-TAPESTRY.md |
| T8.2 | Sample-collection render honesty (T7.1 finding 3): samples-only repos ARE the product | VERIFIED | e3a0690e | aspire-samples `LIBRARY (0 public types)`→`MAP`+`STYLE SampleCollection`+per-service+5 entries (68n/34e/5e was 58/31/0) · `AspireSamples_style_…` un-pended GREEN · +6 pins (NoiseFilterTests/ArchetypeDetectorTests) · MediatR-class repos byte-identical (T1.9 pins green) |
| T8.3 | Empty-fixture-dir→skip in truth gate (github-ready G5 handoff item) | VERIFIED | (T8.3 commit) | `FixtureExists` = exists AND non-empty, all 15 sites — fresh-clone gitlink dirs (empty) now SKIP not FAIL |

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
| dogfood | 2026-07-16 T4 | 439 | 339 | 34 | Microservices (App) | **T4 stage end.** Identical to T1.1 baseline — T4 is pack-layer + snapshot-metadata only (identity header, body fill, contracts/config/tests sections, provenance, fingerprints); graph structure untouched by design. Checkout pack: 35%→90.5% budget use @4k. shamshir pack 99.4% (live MCP captures in tapestry-t4/). |
| dogfood | 2026-07-16 T5 | 439 | 339 | 34 | Microservices (App) | **T5 stage end.** Identical to T4 — T5 is Studio/pack-serving layer only (omitted[] propagation, section content on the wire, Build() focus resolution); graph structure untouched by design. Fresh McpQa M4 baseline in the stage battery (gates-t5-stage.txt GATE: PASS, eval 58P/6S/0F). Note: dogfood working copy carries PRE-EXISTING local mods since 2025-12-12 (YarpApiGateway/Program.cs empty + CreateOrderHandler tweak) — every baseline row in this table includes them; do not restore. |
| aspire-samples | 2026-07-17 T8 | 68 | 34 | 5 | SampleCollection (moderate) | **T8.2 pole.** Was 58/31/0 rendering as `LIBRARY Metrics (0 public types)` with NO style line. `SamplesAreTheProduct` waives sample suppression for samples-ONLY repos: `MAP` header + `STYLE SampleCollection` + per-service rows + HTTP/UI entries w/ repo-relative provenance. Truth-pinned (`AspireSamples_style_…`). Repos with real production projects (MediatR/Ocelot/gRPC) untouched by construction. |
| dogfood | 2026-07-17 T8 | 439 | 339 | 34 | Microservices (App) | **T8 stage end.** Byte-identical to T4/T5 baseline (today's McpQa artifact `eval-results/2026-07-17/mcp-qa.md` confirms 439/339/34) — T8.2 only changes samples-only repos; dogfood has src-rooted services so the flag is false. | |

## Battery cadence (2026-07-16 — user directive; refines §1 "green before every commit")

The full battery (~25–35 min) runs at **boundaries only**: stage close-out · before pushing
dotnet-touching commits · always before merge (with loom-guards). One run validates the TIP
and retroactively covers every commit since the last run — say so in the evidence.
Between boundaries, tier by change: **app-only** → `pnpm check` + ui-audit-drive, record
"dotnet delta zero vs <last GATE: PASS artifact>"; **dotnet mid-stage** → targeted
`dotnet test tests/<affected>.Tests` + loom-guards (~90s, fine per commit), or
`eval/gates.ps1 -SkipEval` (build+fast+McpQa+CLI, ~6 min — verdict self-labels
"PASS (FAST … not a merge gate)"). R-T5 (one battery at a time) and R-T7 (ratchet)
unchanged. Launch batteries DETACHED and watch the log for `GATE:`.

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
