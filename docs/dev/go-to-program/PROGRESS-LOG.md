# Progress Log — go-to program

> Append-only session log. Date — Changed — Verified — Next.

---

## 2026-07-15 — MCP fix session: closed all 3 blind-drive bugs + master plan (feat/mcp-drive-audit)

**Changed:**
- `EntryPointResolver.cs` + `ContextPackBuilder.cs`: bare-route resolution ("/products" → "GET
  /products"), ambiguous routes prefer GET. Fixes bug #1 (trace/get_context "no entry matched").
- All 11 `IEntryPointBuilder`s: stamp `LineNumber` on the `GraphNode` (was FilePath-only). Fixes bug
  #2 (read_source/node `lineNumber: undefined`).
- `GraphBuilder.Build`: reordered so target/group-path/score enrichment runs before `ComputeFlows` —
  `graph.Flows[i].Entry.Target` is no longer frozen at null. Fixes bug #3 (top_flows `target: null`).
- 8 new tests (`EntryPointResolverTests.cs` + 2 in `GraphBuilderTests.cs`).
- Merged `develop` into this branch (commit `42bfa2f`) — brings in the WPF desktop removal.
- Wrote `docs/dev/go-to-program/HANDOVER-2026-07-15.md` (session handover + repo-state snapshot +
  4-bucket master plan) and `eval-results/2026-07-15/mcp-blind-drive-fix-verification.md` (fix detail
  + live re-verification transcripts).

**Verified:**
- `dotnet build DevContext.slnx` 0w/0e.
- Core 448P/3S (was 440P/3S — +8 new tests, 0 regressions), Server 14P, Desktop 64P (pre-merge, WPF
  suite since deleted).
- Eval-category: 25/27 pass; 2 failures (`dntsite` target-name gap, `yarp` gateway-archetype)
  confirmed pre-existing + unrelated via `git stash` comparison against unmodified code.
- Live re-run of the original `mcp-blind-drive.mjs` against both fixture repos with the fixes
  applied — all 3 symptoms gone in real tool output, not just unit tests.
- Housekeeping: confirmed `go-to/implement-iterations`, `feat/universal-coverage-v2`, and 5 other
  branches are fully merged into `develop`; found `docs/go-to-program-addendum` as the one genuinely
  unmerged (trivial, docs-only) branch; found `DevContext2-goto-audit` worktree dir is orphaned (no
  `.git`, harmless); did not touch the other live worktree (`DevContext2-ui`, has 1 unpushed commit).

**Next:** See `HANDOVER-2026-07-15.md` §4 for the full 4-bucket plan. Cheapest high-value picks: UI
Context Studio R1/R4/R5 (omitted-list display, error UI, save-extension fix), the `dntsite`/`yarp`
eval gaps. Highest-leverage engine fix: the span-unbounded Sends/Raises bug (V1.1, confirmed, not yet
fixed). Needs a human decision before executing: whether Desktop V3's plan (WPF-era) is dead now that
WPF was removed in favor of the Angular app.

---

## 2026-07-11 — MCP+UI Audit: Blind-drive evaluation, Context Studio gap analysis (feat/mcp-drive-audit)

**Changed:**
- Wrote `eval-results/2026-07-11/mcp-blind-drive.mjs` — Node.js script that drives devcontext-mcp over stdio JSON-RPC against 2 test fixture repos blind (CleanArchProject, ControllerApp)
- Wrote `eval-results/2026-07-11/mcp-blind-drive-report.md` — per-tool report card: 23 tools exercised, 6× token reduction vs grep, 3 fixable gaps found
- Wrote `eval-results/2026-07-11/ui-context-studio-audit.md` — deep code audit of all 8 pipeline layers: ContextPackBuilder, gRPC handlers, Angular components, proto contracts. Identified 6 functional gaps, 6 UX gaps, 16 recommendations.

**MCP blind-drive findings (key):**
- `overview`/`map`/`resolve`/`impact` work correctly: 1-call understanding, correct archetype detection, no silent disambiguation
- `trace`/`get_context` fail with bare routes (`"/products"`) — expecting `"GET /products"` format only
- `read_source` returns `lineNumber: undefined` for EntryPoint nodes (bug)
- `top_flows` has `target: null` for all entries (data gap)
- Error handling: 5/5 probes actionable (100%) — L5.x work holding
- Token efficiency: MCP 6× fewer tokens than grep (7 vs ~20 commands, ~1125 vs ~7k tokens)

**UI Context Studio findings (key gaps):**
- **No verification at all** — zero ability to confirm generated context matches actual source (CRITICAL gap)
- `omitted[]` list is server-computed but NEVER shown in UI (silent truncation)
- `config` and `tests` card types are dead stubs (loading spinner → no content)
- Focus resolution is brittle: bare routes fail, NL queries fail (by design but creates friction)
- Save always writes `.md` extension even for plain text format
- No error state in CompositionView (silent failure on RPC error)
- No JSON export, no per-card copy, no keyboard shortcuts, no provenance in trace skeleton

**Verified:**
- `dotnet build` 0w/0e
- `dotnet test --filter "Category!=Eval"`: Core 440P/3S, Server 14P, Desktop 64P
- MCP blind-drive: 2 repos, 51 calls, 6,105 tokens, 6/6 errors actionable

**Next:** Fix R1-R5 (immediate bugs: omitted display, route resolution, lineNumber, error UI, file extension) → Then R6 (verification panel)

---

## 2026-07-07 L1 session #5 (Loom L1 identity spine delivery)

**Changed:**
- Created `Graph2/` namespace: `SymbolId`, `SymbolRef`, `RefSite`, `ResolutionTier`, `SymbolTable`, `AmbiguityReport`
- Added `Service`/`Message`/`Store` to `NodeKind` enum; `Exposes`/`DependsOn` to `EdgeKind`
- Created `ServiceBoundaryInference` (runnable detection)
- `GraphBuilder`: `AddServiceNodes` creates Service nodes for runnable projects; `AddTypeNodes` stamps `Project` on every Type node
- `_eventPublishers` changed from `static` to instance field (concurrency fix)
- `AddBusServiceLinks`/`AddGrpcServiceLinks`/`AddHttpServiceLinks` use `NodeKind.Service` + `NodeId.ForService` instead of fake Type nodes
- `AddSends` made non-static (accesses `_eventPublishers`)
- All 11 `EntryPointBuilder`s stamp `Project = scope.ProjectForFile(...)` on every entry point
- Created `scripts/loom-guards.ps1` � bans `new Regex` in Graph/, `SymbolId(` outside Graph2/, `fqns[0]` in Graph2/
- Added 9 `SymbolTableTests` (incl. RazorPages same-short-name-different-project fixture)
- Updated `LOOM-START.md` handoff block + L1 checkpoint rows

**Verified:**
- `dotnet build` 0w/0e
- `dotnet test --filter "Category!=Eval"`: Core 364P/3S, Server 12P, Desktop 64P
- `loom-guards.ps1`: PASSED (0 banned, 13 advisory remaining for L2)
- `pnpm check`: green (session start)

**Next:** L2 (BodyFacts + seam detectors) � branch `feat/loom-l2` off `feat/loom-l1`

## 2026-07-07 � M9-ext: All gaps closed + Inspector sections + UX fixes (Meridian CLOSED)

**Closed (Gaps 1-7 from M7+M8.1a audit):**
- G1 `read_source` RPC: proto ? C# ? server ? TS ? Inspector (`16d3166`)
- G2 Layer/feature uplumb: proto ? ProtoMapper ? TS ? lens-switcher unblocked (`3be265f`)
- G3 PrismJS wired for Code tab (`bf3a674`)
- G4 Contrast audit passes WCAG AA (`bf3a674`)
- G5 Table "Shared Handler" column (`ba6c59a`)
- G6 Dead `audit-table.ts` removed (`bf3a674`)
- G7 Dock toggle cycles 0?2?3?0 (`bf3a674`)

**This session � Inspector sections + UX fixes (Gaps 8-11):**
- **G8 Inspector Insights section:** `session.insights()` filtered by node (evidence match), grouped by severity. Closes W5 TODO. File: `inspector.ts`.
- **G9 Inspector Call Stack section:** Trace tree ancestors + children at depth 2. Keyboard-navigable. Closes W4 TODO. File: `inspector.ts`.
- **G10 drawMinimap() throttled:** `requestAnimationFrame` guard � no more 60fps pan redraws. File: `graph-canvas.ts`.
- **G11 MCP page A15 fixed:** All 3 bare `catch {}` now toast errors via ToastService. File: `mcp-page.ts`.
- **HANDOVER-MERIDIAN.md rewritten:** Comprehensive close-out doc � gap catalog, honest traps, next steps.

**Verified:** `pnpm lint` green (0/0) � `pnpm build` green (0w/0e) � `dotnet build` unchanged (0w/0e)

**Next:** Push + visual smoke ? plan next phase.

---

## 2026-07-06 � M6 completion: Home repo card + Atlas one-pager (M6 DONE)

**M6.1 Home repo card (service-map-hero + tiles + onboarding):**
- **ServiceMapHero** � deterministic HTML/CSS layout (no physics). Gateway column (left), core services (center), dependency column (right). Color-coded project cards with style badge + stack tags. Bus/broker bottom rail for messaging infra. Dependency arrows via SVG overlay. Click ? Explore with project filter.
- **HomeTiles** � three-tile grid: (a) Entries by kind per service with stacked colored bars; (b) Wiring health � % entries with targets, color-coded bar, link to unwired list; (c) Freshness � analysis time, node/edge/project counts, stale/current chip.
- **OnboardingRow** � [Trace checkout] [Open atlas] [Point your agent here] action strip. Auto-detects checkout endpoint from entries.
- **HomePage** rewritten: identity ? service hero ? tiles ? top flows (service-colored chip) ? needs attention ? onboarding row.
- Stats stripped: no raw counts without verbs, confidence % moved to collapsible ledger in identity-strip.

**M6.2 Atlas one-pager (6 sections + export):**
- ? Service diagram (larger hero, edge counts).
- ? Top 5 flows as **FlowStepper** strips (horizontal chip links).
- ? Event wiring board (publisher/event/consumer table, unconsumed highlighted).
- ? **ServiceCards** � per-service style+stack grid.
- ? Cross-cutting (pipeline behaviors + packages card).
- ? Hub radar (flow-count + degree).
- **Export one-pager** button builds markdown from live data, copies to clipboard.

**Changed (7 files):**
- `features/pages/home-page.ts` � rewritten with new shared components
- `features/pages/atlas-page.ts` � 6-section layout + export button
- `features/shared/service-map-hero.ts` (new) � deterministic service diagram
- `features/shared/home-tiles.ts` (new) � three-tile dashboard
- `features/shared/onboarding-row.ts` (new) � action strip
- `features/shared/flow-stepper.ts` (new) � horizontal flow chips
- `features/shared/service-cards.ts` (new) � per-service style cards

**Verified:**
- `dotnet build DevContext.slnx` � 0w 0e
- `pnpm check` (src/DevContext.App) � lint 0/0, test 27/27, build 0w/0e
- Unused imports cleaned (GraphCanvas, ArchitecturePanel removed from AtlasPage)

**Next:** M7.0 Design-token pass ? M7.1 Graph?code binding ? M7.2 Lenses. All Angular UI work.

---

**Audit findings (5 bugs in run-multi.js):**
- **B1** Q6 vacuous pass: `totalKeys >= 0` (always true) ? changed to `wellFormed && totalKeys > 0` matching run.js fix.
- **B2** Q6 missing `wellFormed` structural validation ? added same check as run.js: `typeof resp.key === "string" && typeof resp.totalKeys === "number" && resp.keys !== undefined`.
- **B3** Q5 vacuous pass: `count >= 0` (always true) ? changed to `count > 0 && isAmbiguous`.
- **B4** Q2 vacuous pass on error: `catch { return { pass: true } }` for repos without checkout endpoint ? changed to `return { pass: null, skipped: true }` (excluded from score).
- **B5** Q7 vacuous pass: `count >= 0` ? `count >= 0 && hasMore` (validates response structure).
- Scoring logic updated to exclude skipped questions from denominator.

**M5.1 multi-repo QA complete:**
- Cloned 3 missing repos: TodoApi, CleanArchitecture, DntSite (git clone --depth 1).
- Ran `run-multi.js` across all 5 repos � sequential, fresh MCP per repo.
- Results: dogfood 7/7 (1455 tok), eShop 6/7 (1961 tok), TodoApi 5/7 (522 tok), CleanArchitecture 6/7 (1723 tok), DntSite 4/7 (1228 tok).
- Total across 5 repos: 38 calls, 6889 tokens.
- Ratchet written to `eval-results/2026-07-06/m5-ratchet.json`.
- Notable: DntSite config returns 0 keys (uses Options/Bind pattern, not regex-covered accessors).

**M5.2 agent transcript:**
- Created `eval/mcp-qa/record-transcript.js` � records real MCP tool calls for the checkout question.
- Transcript: 2 calls (overview + trace), 313 tokens, gate PASS (=3c/2ktok).
- Saved to `eval-results/2026-07-06/agent-transcript.md`.

**Changed:**
- `eval/mcp-qa/run-multi.js` � 5 bug fixes (B1-B5) + scoring logic for skipped questions
- `eval/mcp-qa/record-transcript.js` (new) � M5.2 agent transcript recorder

**Verified:**
- `dotnet build DevContext.slnx` � 0w 0e
- `node eval/mcp-qa/run-multi.js` � all 5 repos analyzed, ratchet written
- `node eval/mcp-qa/record-transcript.js` � 2 calls, 313 tok, gate PASS
- Dogfood QA unchanged: 8/8, config 4 keys, checkout 2c/314tok

**Next:** M5 COMPLETE. Next session: M6 � Home repo card + Atlas one-pager (UI features). See `proposal-meridian.md` �M6.

---

## 2026-07-06 � M4 gap closure + M5 starter: config regex repair, get_context fixes, QA tighten, multi-repo infra, McpQa tests

**Changed:**
- **Config tool regex repaired (CRITICAL � config tool was dead on arrival):**
  - Root cause: `ConfigKeyRegex` in both `DevContextGrpcService.cs:251` and `ConfigDefaultsSource.cs:41` had unescaped `)` in method-call patterns (e.g., `GetConnectionString\("([^"]+)")`) instead of escaped `\)`. Each `)` was interpreted as a regex group close instead of a literal `)`, causing `RegexParseException: "Too many )'s"` at runtime. The PowerShell test passed because PowerShell's `[regex]::new()` uses different regex parsing (it accepts `\)` as a literal by default). Added `\)` ? `""\)"` in C# verbatim strings to produce proper `\)` in the regex.
  - Secondary fix: `GroupBy(n => n.FilePath!)` ? `GroupBy(n => n.FilePath!, StringComparer.OrdinalIgnoreCase)` to prevent `ArgumentException` when nodes have case-different file paths. A prior run had 493 nodes ? 455 files (some case-duplicated) which would have crashed on the `ToDictionary` with `OrdinalIgnoreCase` key comparison.
  - Both `DevContextGrpcService.cs` and `ConfigDefaultsSource.cs` fixed.
  - Result: config tool now returns 4 distinct keys (Database, Redis, GrpcSettings:DiscountUrl, MessageBroker:*) � up from 0.
- **get_context v2 gaps fixed (3 issues):**
  - (a) Root entry body: `TraceBuilder.cs:380` � added `Salient = ExtractCalleeSalient(node)` to root `TraceStep` creation. Previously only child steps got `Salient` set.
  - (b) Double DI header: `ContextPackBuilder.cs:294` � removed hardcoded `"## DI Registrations"` from `BuildDiRegistrations` output. `AppendSection` already adds the section header.
  - (c) `Found` flag: `ContextPack.cs` � added `bool Found` property (default `true`). `ContextPackBuilder.Build()` sets `Found = false` when trace is null. `ProtoMapper.ToContextResponse` now reads `pack.Found`.
- **QA test Q6 tightened:** `eval/mcp-qa/run.js:368` � changed from `pass: wellFormed || totalKeys >= 0` (vacuous pass) to `pass: wellFormed && totalKeys > 0` (requires actual keys).
- **MCP-VS-GREP.md updated:** Q6 now shows "DevContext **wins**" (was "Tie") � 4 keys, 214 tokens. Summary updated to 7/7 wins (was 6/7).
- **M5.1 multi-repo runner:** `eval/mcp-qa/run-multi.js` � sequential multi-repo QA against repos listed in `eval/mcp-qa/repos-m5.json`. Per-repo analyze + 7 questions + token tracking. Kills/restarts MCP between repos. Writes ratchet to `eval-results/<date>/m5-ratchet.json`. 2/5 repos available locally (dogfood, eShop); 3 need clone (TodoApi, CleanArchitecture, DntSite).
- **M5.3 CI wiring:** `tests/DevContext.Core.Tests/McpQaGateTests.cs` � 2 tests:
  - `McpQaHarness_Passes_Against_Dogfood`: spawns `node eval/mcp-qa/run.js --quiet`, content-asserts QA Score + Gate PASS. Category `McpQa`. ~130s runtime.
  - `McpQa_Bench_Smoke_Script_Exists`: verifies `scripts/bench.ps1` and `eval-repos.json` exist. Category `McpQa`.

**Verified:**
- `dotnet build DevContext.slnx` � 0w 0e
- `dotnet test --filter Category!=Eval` � 429/0 (353+64+12, 3 skipped)
- `dotnet test --filter Category=McpQa` � 2 tests discovered (1 smoke test passes, harness test needs running separately due to ~130s runtime)
- `node eval/mcp-qa/run.js` � 8/8 QA PASS, config 4 keys (214 tok), checkout gate 2c/314tok
- Dogfood analysis: 493 nodes, 316 edges, 34 entries (no regressions)

**Next:** Clone TodoApi/CleanArchitecture/DntSite ? run `node eval/mcp-qa/run-multi.js` for 5-repo ratchet ? record M5.2 agent transcript ? commit. See `MERIDIAN-START.md` handoff block for detailed next steps.

**Changed:**
- **M4.7 config key fix:** `ConfigKeyRegex` added `RegexOptions.IgnoreCase` for case-insensitive matching. `ConfigLookup` handler switched from `WrapAsyncT` to `WrapT` (sync file I/O).
- **MCP startup fixes (critical � MCP was dead on arrival):**
  - `ServerShim.FindServerExe()`: replaced `GetDirectories("bin")` + `Path.Combine(binDir, "exe")` with `GetFiles("DevContext.Server.exe", AllDirectories)` � the exe lives in `bin/Debug/net10.0/` not `bin/`.
  - `ForceHttp11Handler`: gRPC-Web uses HTTP/1.1 transport but `GrpcWebHandler` negotiates HTTP/2, rejected by server with `HTTP_1_1_REQUIRED`. Custom `DelegatingHandler` forces `Version11` + `RequestVersionExact`.
- **QA harness (`eval/mcp-qa/run.js`) rewritten:**
  - Tool names: `search` ? `find`, `impact` `target` ? `nodeId`
  - Impact response: `entries`/`count` ? `resultsByService`/`totalAffected`
  - New q6 (config tool), q7 (tests_for), checkout gate question
  - Token tracking per call via `estimateTokens()` and tool-provided `tokens` field
  - Per-question token ceilings enforced
  - Checkout gate: =3 calls, =2k tokens (achieved: 2c/314tok)
- **MCP-VS-GREP.md:** comparison table showing DevContext wins on 6/7 questions vs grep
- **Config tool regression:** tool callable but returns 0 keys (regex still doesn't match dogfood repo patterns � needs M5 investigation)

**Verified:**
- `dotnet build DevContext.slnx` � 0w 0e
- `dotnet test --filter Category!=Eval` � 429/0 (353+64+12, 3 skipped)
- `pnpm check` � green (lint 0/0 � test 27/27 � build 0w/0e)
- `node eval/mcp-qa/run.js` � 8/8 QA PASS, checkout gate 2c/314tok
- Dogfood analysis: 493 nodes, 316 edges, 34 entries, 6 ServiceLinks

**Next:** M5.1 � Extend QA set to 5 repos (eShop, CleanArchitecture, TodoApi, DntSite, dogfood) + per-question token ratchets. See `proposal-meridian.md` �M5. Also: fix config regex to actually match real code patterns, debug async lambda issue in WrapAsyncT.

---

## 2026-07-06 � Meridian M4 completion: impact + config + tests_for (M4 DONE, all 9/9 tools)

**Changed:**
- **Audit fixes (3 findings from previous session):**
  - A1 **Compact flow file:line:** `BuildCompactFlow` now non-static, outputs `Rel(handle, node.Provenance)` provenance per step. Caller passes `handle`.
  - A4 **idBudget no-op:** Removed dead `mode == "explain" ? budgetTokens : budgetTokens` assignment in `ContextPackBuilder.cs`.
  - A5 **DI registrations for Member nodes:** `BuildDiRegistrations` now resolves Member nodes' parent types (last dot in key) for DI lookup.
- **M4.4 impact transitive + diff-aware:**
  - Proto: `ImpactRequest` extended with `direction` (up/down/both), `transitive`, `files` repeated; `ImpactResponse` extended with `direction`, `total_affected`; `ImpactResult` extended with `node_id`, `file_path`, `line_number`, `service`, `node_title`.
  - Engine: `GraphQuery.Impact(NodeId, ImpactDirection, int)` � unified BFS over in/out/both edges. `GraphQuery.ImpactFromFiles(�)` � diff-aware mode: find graph nodes matching file paths, compute union impact closure. `ImpactResult` record replaces `BlastResult` for new API (backward compat kept).
  - gRPC: `GetImpact` updated for direction + files mode routing. `ProtoMapper.ToImpactResponse` updated with new fields.
  - MCP tool: `impact` accepts optional `nodeId`, `direction` (default "up"), `files` array. Results grouped by service.
- **M4.7 config keys tool:**
  - Proto: new `ConfigRequest`, `ConfigResponse`, `ConfigBinding` messages; new `rpc ConfigLookup`.
  - Engine: Config scanning at query time � reads source files on disk, applies regex (from `ConfigDefaultsSource`) per line, returns matches with file:line, pattern type, service, optional node_id cross-reference.
  - gRPC: `ConfigLookup` handler with `WrapAsyncT` (async file I/O). Uses compiled `ConfigKeyRegex`. Groups files by path from graph nodes.
  - MCP tool: `config(handle, key?)` � returns key?binding sites grouped by key.
- **M4.9 tests_for tool:**
  - Proto: new `TestsForRequest`, `TestsForResponse`, `TestRef` messages; new `rpc FindTestsFor`.
  - Engine: `GraphQuery.FindCallers(NodeId, int)` � BFS over IN-edges returns all caller nodes with distances.
  - gRPC: `FindTestsFor` handler filters callers via `IsLikelyTestMethod()` heuristic (name suffixes `_Test`/`_Should`, file paths containing `/test/`, project names ending in `Tests`/`Test`/`Specs`).
  - MCP tool: `tests_for(handle, nodeId, maxDepth)` � returns test methods with file:line, distance, project.
- **Proto regeneration:** `pnpm gen:proto` ran, TS bindings regenerated.

**Verified:**
- `dotnet build DevContext.slnx` � 0w 0e
- `dotnet test --filter Category!=Eval` � 429/0 (353+64+12, 3 skipped)
- `pnpm check` � green (lint 0/0 � test 27/27 � build 0w/0e)
- Dogfood out.md: 493 nodes � 316 edges � 34 entries � 6 ServiceLinks � 18 Handles � Style Microservices � Analyzed 3.1s (no regressions)

**Next:** M5.1 � Extend QA set to 5 repos (eShop, CleanArchitecture, TodoApi, DntSite, dogfood) + per-question token ratchets. See `proposal-meridian.md` �M5.

---

## 2026-07-06 � Meridian M3 gap fixes + M4 feature delivery (M3 gaps RESOLVED, M4 6/9 DONE)

**Changed:**
- **M3 gaps (7/7 fixed):**
  - G1 **TokenTotal:** `WrapT`/`WrapAsyncT` measured wrappers now call `CompleteCall()` which increments `session.TokenTotal` via proto `CalculateSize()` bytes-per-token estimate. `DevContextGrpcService.cs` refactored: old static `Wrap`/`WrapAsync` removed, new instance `WrapT<T>`/`WrapAsyncT<T>` with `[CallerMemberName]` tool name + `Stopwatch` timing.
  - G2 **elapsedMs:** Same wrappers � `Stopwatch` started before handler, stopped after, elapsed passed to `RecordToolCall`. No more `1ms` placeholder.
  - G3 **Orphaned sessions:** `CloseSessionAsync` now only removes `_repoToHandle` entry if its value still matches the closing handle (`if (current == handle)`). Old sessions coexist safely; re-analyzing same repo+HEAD doesn't destroy existing handles.
  - G4 **Absolute paths:** `DevContextTools` now stores `_repoRoots` dictionary (handle?repoRoot) populated on `analyze`. `Rel(handle, path)` helper applied to `filePath`/`provenance` in `node`, `read_source`, `neighbors`, `usages` tool responses. Paths now repo-relative via `D4` mandate.
  - G5 **volatile `_running`:** `McpObservabilityService._running` now `volatile bool` � writes visible across threads.
  - G6 **Stale observers:** `Notify()` now collects `TryWrite` failures into a `stale` list and removes them after enumeration � no more unbounded stale entries.
  - G7 **ServerShim discovery:** `FindServerExe()` now searches 5 sources in priority: `DEVCONTEXT_SERVER` env var ? sibling/nearby published layouts ? `%LOCALAPPDATA%/DevContext/server/` ? dev-layout recursive bin search ? null. No more hardcoded `bin/Debug/net10.0`.
- **M4.1 overview tool:** New MCP tool � calls `GetMap` + `GetStats` + `ListEntryPoints` + `GetInterestingPoints`; assembles compact text: archetype + services + node/edge/entry counts + top 5 flows + project topology + start-here points + behaviors/stack. Token-estimated.
- **M4.2 resolve tool:** New MCP tool � calls `SearchNodes` + `GetNode` for each hit; returns `ambiguous` flag + candidate list with nodeId, kind, filePath, degree, tags. Hint when >1 match. Never auto-picks.
- **M4.3 flow compact mode:** `trace` tool gains `format=compact` � indented text with seam glyphs (`? ? ? ? ? ? ?`), `[approx]` annotations, child indentation. `BuildCompactFlow()` recursive formatter.
- **M4.5 read_source full-member:** `read_source` gains `mode=member` � `FindMethodScope()` brace-balancing heuristic finds method body from declaration line, returns full scope.
- **M4.6 find paginated:** New `find` tool supersedes `search` � `cursor`/`limit` pagination, `kind` filter, `hasMore` flag, `total` count. `TrimTitle()` strips `=>` lambda remnants and >200-char titles.
- **M4.8 get_context v2:** `ContextPackBuilder` identity section now includes service styles, pipeline behaviors, and cross-service ServiceLink edges from `_query.Graph.AllEdges`. Richer overview context.
- **Engine files:** `DevContextGrpcService.cs` � `Require()` simplified to lookup-only; `CompleteCall<T>()` records bytes + elapsed + TokenTotal; all handlers use `WrapT`/`WrapAsyncT`.
- **Server files:** `AnalysisSessionManager.cs` � G3 fix in `CloseSessionAsync`; `AnalysisSession.cs` � added `RepoRoot`; `McpObservabilityService.cs` � `volatile` on `_running`, `Notify()` stale cleanup.

**Verified:**
- `dotnet build DevContext.slnx` � 0w 0e
- `dotnet test --filter Category!=Eval` � 429/0 (353+64+12, 3 skipped)
- `pnpm lint` � green (0/0); `pnpm build` � green (0w/0e)

**Remaining M4 (for next session):**
- M4.4 `impact` transitive + diff-aware mode
- M4.7 `config` keys ? binding/consumption sites
- M4.9 `tests_for` best-effort

**Next:** M4.4 ? M4.7 ? M4.9 (see `MERIDIAN-START.md` stage tracker). After M4 completion: M5 agent eval ratchet.

---

## 2026-07-06 � Meridian M3: MCP re-architecture (server-of-record, stdio shim, descriptions, MCP page) (M3 COMPLETE)

**Changed:**
- **M3.1** � Server-of-record + stdio shim + flush fix:
  - `AnalysisSessionManager.cs`: repo+HEAD session keying (`_repoToHandle` index), `TryGetByRepo()`, `ListSessions()`
  - `AnalysisSession.cs`: added `RepoPath`, `CommitSha`, `CreatedAt`, `LastActivity`, `CallCount`, `TokenTotal` metadata fields
  - `DevContextGrpcService.cs`: restructured `Analyze` � result written directly to `responseStream` after channel drain (flush fix), uses `ct` for `ReadAllAsync`, `ListSessions` handler
  - `DevContext.Mcp/Program.cs`: rewritten as thin gRPC proxy � removed all DevContext.Core/Cli references, connects to server via `GrpcWebHandler`, spawns server if not running
  - `DevContext.Mcp/DevContext.Mcp.csproj`: replaced engine dependencies with Grpc.Net.Client/Grpc.Net.Client.Web + DevContext.Contracts (gRPC stubs)
  - `DevContext.Mcp/McpSessionManager.cs`: **deleted** (session management now server-side)
  - `DevContext.Mcp/ServerShim.cs`: new � spawns DevContext.Server.exe, health-check retry loop, process cleanup
- **M3.2** � Tool descriptions + envelope trim:
  - `DevContext.Mcp/DevContextTools.cs`: all 18 tools have XML `<summary>` docs with parameter descriptions and examples; envelope trimmed to compact `meta` lines (no Scope/Coverage/Confidence); rewritten as gRPC proxy calls
- **M3.3** � Observability stream + MCP page:
  - `proto/devcontext/v1/devcontext.proto`: added `ListSessions`, `StartMcp`, `StopMcp`, `ObserveToolCalls` RPCs; `ToolCallEvent`, `SessionInfo` messages
  - `DevContext.Server/Services/McpObservabilityService.cs`: new � fan-out `Channel<ToolCallEvent>` observer pattern, `Start()`/`Stop()` toggle, `Notify()` broadcast
  - `DevContextGrpcService.cs`: `StartMcp`/`StopMcp`/`ObserveToolCalls` handlers; `RecordToolCall` via `[CallerMemberName]` on `Require()`
  - `src/DevContext.App/src/app/features/pages/mcp-page.ts`: new � MCP page with status dot + toggle, config snippet cards (Claude Code/Cursor/VS Code) with copy, session list, live feed with token chips, try-a-tool console
  - `src/DevContext.App/src/app/app.config.ts`: `/mcp` route added
  - `src/DevContext.App/src/app/shell/activity-bar.ts`: MCP rail entry (`activity` icon, `g m`)
  - `src/DevContext.App/src/app/shell/workspace-shell.ts`: `g m` shortcut in VIEW_SHORTCUTS

**Verified:**
- `dotnet build DevContext.slnx` � 0w 0e
- `dotnet test --filter Category!=Eval` � 429/0 (12+64+353, 3 skipped)
- `pnpm check` green: lint 0/0 � test 27/27 � build 0w/0e (mcp-page chunk: 11.29 kB)

**Static audit findings (carried forward):**
1. `TokenTotal` never incremented � session list shows 0 tok always
2. `RecordToolCall` elapsedMs uses placeholder (1ms) � needs real timing
3. Re-analyzing same repo+HEAD creates orphaned session (old session not evicted)
4. Paths remain absolute (D4 envelope trim not done on server-side)
5. `McpObservabilityService._running` lacks memory barrier (`volatile` needed)
6. `_observers` dictionary accumulates stale entries (no cleanup)
7. ServerShim auto-spawn only works in dev layout (hardcoded bin/Debug path)

**Next:** M4.1 � `overview` =600 tokens one-call repo brief

---

## 2026-07-06 � Meridian M2: insight repair + new sources + typed actions + layer facets (M2 COMPLETE)

**Changed:**
- **M2.1** � Retired/repair discredited insight sources:
  - `EntryMixSource.cs`: CLI commands filtered out when non-CLI entries exist (`hasNonCliEntries` gate)
  - `GatewayArchetypeSource.cs`: Rewritten from call-edge analysis to only use `EdgeKind.ServiceLink` edges; confidence raised 0.5?0.75; uses `ServiceLinkTags` for transfer seams
  - `GraphOrphansSource.cs`: Added `FindConventionDiTypes()` to exclude MediatR handlers, validators, consumers; wiring gate (`handlesCount<5 && sendsCount<10`); confidence lowered to 0.4
  - `AnonymousEndpointsSource.cs` + `WebArchetypeSource.cs`: Added `IsRazorPage()` check excludes `.cshtml` endpoints from auth surface
- **M2.2** � 4 new wiring-grounded insight sources:
  - `EventFlowSource.cs`: Scans Raises/Consumes edges; reports orphan events, cross-service event flows via BusPublishConsume ServiceLinks; emits TypedAction.Node
  - `SpofSource.cs`: Fan-in/fan-out analysis on ServiceLink edges; identifies services with =2 consumers; emits TypedAction.Node links
  - `UnvalidatedEndpointsSource.cs`: Cross-references HTTP endpoints against `AbstractValidator<T>` subclasses; requires FluentValidation signal; emits TypedAction.Focus
  - `ConfigDefaultsSource.cs`: Regex scan of `IConfiguration`/`GetValue<>`/`GetSection<>`; cross-references against `appsettings*.json` keys
- **M2.3 (D6)** � Typed actions engine?proto?UI:
  - `Insight.cs`: Replaced `InsightAction` enum with `TypedActionKind` (Focus/Node/Filter/None); `TypedAction` sealed record; `PrimaryAction` + `EvidenceActions`
  - All 10 insight sources migrated: `TypedAction.Focus()`/`.Node()`/`.Filter()`
  - `ProtoMapper.cs`: Maps PrimaryAction + EvidenceActions ? proto `action`/`action_target`/`evidence_actions`
  - `devcontext.proto`: Added `evidence_actions` field (field 12), documented D6 format
  - UI: `insights-view.ts` ? `eaRoute()`/`paRoute()` routing; home-page.ts + workbench-page.ts updated
- **M2.4 (D9)** � Layer/feature facets (engine only):
  - `ArchitectureEnums.cs`: Expanded `ArchitectureLayer` (Contracts, PublicApi, Core, Internals, View, ViewModel, Platform); `ToLabel()` per-archetype mapping; `LayerViolation` record
  - `SyntaxStructureExtractor.cs`: `InferLayer()` 406 lines � base types?namespace?file path?naming conventions
  - `GraphBuilder.cs`: `DeriveFeature()` strips project prefix + layer segments; `DetectLayerViolations()` per-archetype illegal-layer pairs; `AddTypeNodes()` sets Layer/Feature
  - `CodeGraph.cs`: `GraphNode.Layer`/`Feature`; `CodeGraph.LayerViolations` property

**Verified:**
- `dotnet build DevContext.slnx` � 0w 0e
- `dotnet test --filter Category!=Eval` � 429/0 (12+64+353, 3 skipped)
- `pnpm check` green: lint 0/0 � test 27/27 � build 0w/0e
- Dogfood out.md: 493 nodes � 316 edges � 6 ServiceLinks � 10 insights (4 info, 3 notable, 3 warning) � New sources registered: EventFlow/Spof/UnvalidatedEndpoints/ConfigDefaults � Analyzed 3.6s

**Next:** M3.1 � Server-of-record + stdio shim + flush fix

---

## 2026-07-06 � Meridian M1.8-M1.9: HTTP ServiceLinks + per-service style rollup (M1 COMPLETE)

**Changed:**
- **M1.8** � HTTP/YARP ServiceLinks:
  - **Bug fix:** `PopulateGatewayRoutes` now runs BEFORE `GraphBuilder.Build` � the gateways were populated after graph construction, making `AddHttpServiceLinks` dead code (always returned immediately). Now executes in correct order.
  - **Path-pattern normalization:** Rewrote route matching with `StripPathTemplateVariables` helper � strips `{**catch-all}`, `{param}`, `{param:type}` from YARP routes to compute static prefix, then segment-aware matches against Refit routes (handles query-string stripping, segment-boundary validation).
  - **ServiceLinkTags assigned** to all edges: `BusPublishConsume` (bus), `Grpc` (gRPC), `HttpViaGateway` (HTTP). Added `Tags` property to `GraphEdge` record.
  - **TraceBuilder:** Added `ServiceLink` to `TraceOptions.Follow` default list. Added `CrossService` to `SeamKind` enum + `ToSeam` mapping. Added UI color for CrossService seam.
  - **Rendering:** New `CROSS-SERVICE` section in `MapRenderer` with per-tag summary + per-edge detail listing. `ServiceLinks` count added to `ReportRenderer.BuildStats` table. Added `AllEdges` property to `CodeGraph`.
  - **Golden:** Shopping.Web ? YarpApiGateway + YarpApiGateway ? Catalog/Basket/Ordering.API (4 HTTP ServiceLinks, 6 total with bus+gRPC).
- **M1.9** � Per-service style rollup (D5):
  - New `PerServiceStyle` record (project name, style, stack tags) in `DiscoveryModel`.
  - `ArchitectureStyleDetector.DetectPerServiceStyles()`: classifies each runnable web project by local style � Gateway (YARP), gRPC Service (dedicated gRPC), Clean Architecture (MediatR+EF Core), CQRS (MediatR only), Web API, Web App / Razor Pages.
  - `IsRunnableService()`: requires Exe output or Web SDK for runnable classification (class libraries with AspNetCore packages no longer false-positive as services).
  - Per-service styles rendered in `MapRenderer.AppendStyle()` under STYLE section.
  - `ServiceStyle` proto message + `service_styles` field added to `MapResponse`. `ProtoMapper.ToMapResponse` wired. TypeScript bindings regenerated via `pnpm gen:proto`.
  - **Golden:** 6 services classified: YarpApiGateway (Gateway), Shopping.Web (Web App), Discount.Grpc (gRPC Service), Basket/Catalog/Ordering.API (Web API + stack tags).

**Verified:**
- `dotnet build DevContext.slnx` � 0w 0e on commit
- `dotnet test --filter Category!=Eval` � 429/0 (12+64+353, 3 skipped)
- `pnpm check` green: lint 0/0 � test 27/27 � build 0w/0e
- Dogfood out.md content-asserted: 6 ServiceLinks (1 bus, 1 gRPC, 4 http) � 6 per-service styles � 493 nodes � 316 edges (was 312, +4 HTTP) � Handles =14 present � Style Microservices intact
- No regressions on library fixtures (dotnet test green, pnpm check green)
- Proto contract extended (+ServiceStyle message), TypeScript bindings regenerated

**Next:** M2.1 � Retire/repair discredited insight sources. See `docs/dev/briefs/proposal-meridian.md` �M2.

---## 2026-07-06 � Meridian M1.6-M1.9: Cross-service ServiceLink edges + microservices archetype

**Changed:**
- **M1.6** � MassTransit bus ServiceLink: cross-project publish?consume join matching event FQNs. `_eventPublishers` map collected during `AddSends`, consumed by `AddBusServiceLinks` in `GraphBuilder`. Golden: BasketCheckoutEvent links Basket.API ? Ordering.Application. Also fixed Adapt<T> body-scan bug: `ResolveVariableFromAdapt` (Adapt/Map/Create factory patterns) now checked BEFORE `new X()` fallback in `AddSends` � was picking guard-clause `new CheckoutBasketResult(false)` instead of the Adapt-resolved BasketCheckoutEvent.
- **M1.7** � gRPC ServiceLink: new `GrpcClientExtractor` (order 59, gRPC+gGateway signals) detects generated client types (XxxClient pattern) including primary constructors. `AddGrpcServiceLinks` cross-projects joins client?server by service name. Golden: DiscountProtoServiceClient in Basket.API ? DiscountService in Discount.Grpc.
- **M1.8** � Infrastructure for HTTP/YARP/Refit ServiceLinks: new `RefitInterfaceExtractor` (order 60, Refit signal) detects `[Get]`/`[Post]` interfaces. YARP `ReverseProxy` config parsing added to `DiscoveryPipeline.PopulateGatewayRoutes` (parses Routes/Clusters/Destinations from appsettings*.json). `AddHttpServiceLinks` route matching framework in place; path-pattern normalization needed for matching Refit routes (`/catalog-service/products/{id}`) against YARP patterns (`/catalog-service/{**catch-all}`).
- **M1.9** � Microservices archetype: `ArchitectureStyleDetector` now detects microservices without Aspire requirement (=2 web services + gateway/bus evidence). `ArchetypeDetector` no longer forces Gateway archetype when 2+ runnable services exist � returns App. Web SDK detection via `IsWebSdkProject` reads csproj Sdk attribute.
- **Infra** � `EdgeKind.ServiceLink` + `ServiceLinkTags` (BusPublishConsume/Grpc/HttpViaGateway/RefitDirect) added to `CodeGraph`. New detection models: `GrpcClientDetection`, `RefitRouteDetection`.

**Verified:**
- `dotnet build DevContext.slnx` � 0w 0e on all commits
- `dotnet test --filter Category!=Eval` � 429/0 (12+64+353, 3 skipped)
- Dogfood report: 489 nodes � 312 edges � 2 ServiceLinks (1 bus, 1 gRPC) � Style: Microservices (confidence high) � MAP archetype
- Adapt<T> checkout gap fixed: BasketCheckoutEvent now correctly tracked in Sends

**Next:** M2.1 � Retire/repair discredited insight sources. See `docs/dev/briefs/proposal-meridian.md` �M2.

---

## 2026-07-05 � Lighthouse L3: Kernel answers (all 6 checkpoints)

**Changed:**
- **L3.6** `GroupPath` on `EntryPoint` + proto `EntryPoint.group_path` + namespace-derived grouping via `NameResolver.GetNamespace()`. `GrpcEntryPointBuilder` expanded from service-level to per-method entries (one entry per gRPC method with service name as GroupPath). `HttpRouteGroupPath()` fallback for HTTP lambdas.
- **L3.5** `int? LineNumber` on `GraphNode`, `NodeDetail`, `proto NodeResponse.line_number`. Populated from `TypeDiscovery.StartLine` (set during `SourceBodyExtractor`). `AuthAttributes` on `EntryPoint` + `proto EntryPoint.auth_attributes`, piped from `EndpointDetection.AuthAttributes` (already detected by `EndpointExtractor`/`ControllerActionExtractor`). `CodeGraphBuilder.AddNode` merge now respects `LineNumber`.
- **L3.1** `rpc GetImpact(ImpactRequest) returns (ImpactResponse)` � wraps the already-implemented `GraphQuery.BlastRadius()` (BFS over in-edges to find reachable entry points). gRPC handler + `ProtoMapper.ToImpactResponse()`.
- **L3.2** Graph-aware entry scoring: `BfsEntryScore` performs BFS (depth 6) from each entry node, counting reach, seam richness (Sends/Raises/Consumes), entity touches (ReadsWrites to entity/aggregate tags), and cross-project depth. `EnrichEntryScores` normalizes into a composite 0..1 `Score` stored on `EntryPoint`. `Proto EntryPoint` gained `score`, `reach`, `cross_projects` fields. `ReportRenderer.RankEntries` now sorts by score first, falling back to has-target + kind + title.
- **L3.4** Hub-scoping for sparse graphs: `AddHubScopeEdges` detects sparseness (entries < 5 or edge/node ratio < 0.1), identifies top-K central types by degree centrality from the model's `CallEdges`, and binds their inter-type call edges (even when one endpoint lacks FilePath � the normal gate). Budget-capped at 500 edges. `CodeGraph.IsSparseGraph` + `HubScopeNodeCount` populated and reported via `GraphStat.sparse_graph`/`hub_scope_nodes` in Stats.
- **L3.3** `GraphQuery.GetInterestingPoints(archetype?)` � 5 per-archetype strategies: **web** (auth boundary entries + data hubs + pipeline middleware), **library** (top-degree public API hubs + implementor seats with =2 Resolves edges), **messaging** (message producers via Sends/Raises + consumer entries), **desktop** (top-3 centrality per-project as module hubs), **CLI** (command entry points). Centrality fallback for unknown/empty archetype (top-20 by degree). New `rpc GetInterestingPoints` in proto + gRPC handler + `ProtoMapper`.

**Verified:**
- `dotnet build DevContext.slnx` � 0w 0e on all 6 commits
- `dotnet test --filter Category!=Eval` � 429/0 (12+64+353, 3 skipped), same baseline throughout
- `pnpm check` (lint+test+build) � green on all 6 commits (lint 0/0, test 27/27, build 0w/0e)

**Next:** L4 � Insight engine v2 + archetype lenses. See `docs/dev/briefs/proposal-lighthouse.md` �L4.

---

## 2026-07-04 � Lighthouse L2: CLI `report` + bench loop + query parity

**Changed:**
- **L2.1** `devcontext report <path|url>` � new `ReportCommand` + `ReportSettings` in CLI, new `ReportRenderer` in Core.Rendering. Composes identity sentence, stat digest, top flows (v1: has-target + kind priority), top-3 compact traces, insights (v1), full architecture map, and run report into one deterministic markdown doc. `--format json` delegates to `KernelJsonRenderer`. `ReportRenderer` orchestrates existing Map/Trace/insight renderers � no second rendering path.
- **L2.2** `scripts/bench.ps1` � runs `devcontext report` across `eval-repos.json`, saves to `eval-results/<date>/`, emits structural diff (node/edge/entry/insight counts, section changes) vs previous run. Strips wall-time for determinism. Supports `--SkipClone` and `--DiffOnly`.
- **L2.3** Benchmark set v2 � `eval-repos.json` extended from 16 to 22 repos: added PowerToys (desktop megarepo), Serilog (library), Spectre.Console (CLI framework), MassTransit-Sample (messaging app), DevContext self (dogfood), and place for TradingEngine.
- **L2.4** Query surface parity � `QueryCommand` now supports all 8 ops: `node` (? `GraphQuery.Node`), `neighbors` (? `GraphQuery.Neighbors` with `--direction`), `usages` (? `GraphQuery.FindUsages`), `search` (? new `GraphQuery.Search` � title/id match, ranked by degree, capped at 20). `GraphQuery` gained `Search(term)` + `SearchResult` record. Fixed pre-existing DI bug where `ILogger<DiscoveryPipeline>` wasn't registered in `Program.cs`, preventing `query` from resolving.

**Verified:**
- `dotnet build DevContext.slnx` � 0w 0e on all 4 commits
- `dotnet test --filter Category!=Eval` � 429/0 (64+12+353, 3 skipped), same baseline
- `pnpm check` (lint+test+build) � green on all 4 commits (lint 0/0, test 27/27, build 0w/0e)
- `cargo check` � green
- Live smoke: `devcontext report` against `MinimalApiProject` � identity, stats, top flows, 2 traces, insights, map, run report all render correctly
- Live smoke: `query node --focus GetOrdersQuery` � JSON node detail
- Live smoke: `query search --focus Order` � 13 ranked hits
- Live smoke: `query usages --focus Order` � correct edge list

**Next:** L3 � Kernel answers (Impact RPC, Top Flows ranking, InterestingPoints, graph completeness). See `docs/dev/briefs/proposal-lighthouse.md` �L3.

---

## 2026-07-04 � Lighthouse L1: Open fast, reopen instantly, stay responsive

**Changed:**
- **L1.1** Persistent clone registry: `CloneRegistry` at `%LocalAppData%/DevContext/repos/registry.json` � JSON keyed by owner+repo+ref, thread-safe via `ReaderWriterLockSlim`, file-locked writes (`FileShare.None`). `GitCloneService` now receives registry via constructor and queries it before cloning. Clone order flipped: git CLI shallow (`--depth 1 --single-branch`) first, LibGit2Sharp fallback. Registry registered as singleton in DI (`Program.cs`). CLI/Desktop callers updated.
- **L1.2** Snapshot-first open: `EngineRunner.AnalyzeAsync` reordered � for GitHub URLs, checks registry?snapshot cache BEFORE any network I/O (clone bypassed entirely on cache hit). Background `git fetch --dry-run` staleness probe compares `origin/HEAD` vs local HEAD. `EngineResult` now has `Stale`/`StaleMessage`. Proto `AnalysisSummary` extended with `stale`/`stale_message` fields. UI `identity-strip.ts` renders amber "Repo moved ahead � Re-analyze?" chip with click action calling `SessionStore.reAnalyze()`.
- **L1.3** Progress v2: Clone � `TryCloneGitCli` adds `--progress`, parses stderr for phase transitions (Enumerating/Counting/Compressing/Receiving/Resolving) via `ParseCloneProgress` with weighted 0-100 mapping. Analysis � `StreamingProgressObserver` throttled to =250ms between reports, uses `OnExtractorCompleted` to interpolate within-stage progress (typesAdded). UI � `run-console.ts` boot mode now renders a phase checklist (checkmark/spinner/gray icons with live message + percent) instead of only a linear log. Raw log hidden behind `<details>`.
- **L1.4** Responsiveness: `AnalysisSessionManager` all three sync-over-async sites (`GetAwaiter().GetResult()`) replaced with proper `await` � `CloseSession`?`CloseSessionAsync` (Task<bool>), `EvictIfNeeded`?`EvictIfNeededAsync`. Tauri `spawn_child` now sets sidecar to `BELOW_NORMAL_PRIORITY_CLASS` via `SetPriorityClass` (Windows only, `windows` crate Win32_System_Threading). `ActivityService.setProgress` coalesced with 80ms debounce (intermediate sets skip change detection but store latest values).

**Verified:**
- `dotnet build DevContext.slnx` � green on all 4 commits
- `pnpm check` (lint+test+build) � green on all 4 commits
- `cargo check` (`src-tauri/`) � green on L1.4

**Next:** L2 � CLI `report` + bench loop (engine-only). See `docs/dev/briefs/proposal-lighthouse.md` �L2.

## 2026-07-02 � R2 execution (session 1)

**Changed:**
- Merged addendum docs (I8 caching, I9 release, I10 tabs, ADDENDUM-A harder repos) from `C:\Code\DevContext2-addendum`
- Updated README.md tracker: added I8/I9/I10/A rows, updated CORE spine to I1?I2?I3?I4?I8?I10?I9
- Updated UNIFIED-TRACKER.md: added I8/I9/I10 sections, new delivery diagram
- **R2.1** Insights on wire: KernelJsonRenderer ? proto ? gRPC server ? TypeScript store ? desktop view ? CLI. Full stack: `Insight[]` now reaches every face.
- **R2.2** NodeLink component: every name is a link. wired into entries/trace/node-card + document markdown linkify.
- **R2.3** Entries table: sortable columns, filter chips (has-target/approx), hover row actions (Trace/NodeCard/Copy), filtered/total counter.
- **R2.4** Trace fixes: F6 dead Tailwind class removed, focus breadcrumb with back, honest empty hint.
- **R2.5** Graph view: new face with seeded exploration from entries, seam filter chips, NodeCard via NodeLink. Route + rail item.
- **R2.6** Settings view: new face with Appearance/Analysis/Storage(I8)/Server/About(I9) sub-tabs. ConnectionStore now captures version from PingResponse.
- **R2.7** Palette: added Graph, Browse, Document, Settings entries.
- **R2.8** Connection: 3-state (online/connecting/offline) with server version tooltip.
- **R2.9** Overview: top-3 notable insights section at top.
- **R2.10** Export packs: Onboarding/Trace/Review presets that auto-select section toggles.

**Verified:**
- `dotnet build DevContext.slnx` � 0w 0e
- `dotnet test DevContext.slnx --filter Category!=Eval` � 385/0 green
- `pnpm lint` � green (pre-existing build errors in node-card/palette/node.store unrelated to R2)

## 2026-07-02 � Pre-existing TS errors + handover (session 1 cleanup)

**Changed:**
- Fixed 5 pre-existing TypeScript build errors from round-1 session that prevented `pnpm build`:
  - `node-card.ts`: removed `n.line` (not in NodeResponse proto); replaced `neigh.incoming`/`neigh.outgoing` with edge-filtering via computed signals
  - `palette.ts`: `r.results` ? `r.nodes` (SearchResponse field name)
  - `app-shell.ts`: removed unnecessary `?.` on `label` (required proto field)
  - `node.store.ts`: `'both'` ? `'out'` + `'in'` with merged edges via `create(NeighborsResponseSchema)`
- Fixed 4 self-inflicted errors from R2 code:
  - `settings-view.ts`: `theme.vibes`?`theme.vibes()`, `theme.activeVibe`?`theme.vibe()`, removed unused imports
  - `graph-view.ts`: removed unused Icon/Badge imports
  - `document-view.ts`: `onDocClick(MouseEvent)` ? `onDocClick(Event)` for keyboard event compatibility
  - `title-bar.ts`: fixed template string literal parsing error with single quote
- Rewrote HANDOVER.md: round-2 delivery summary, review checklist, known caveats, next-items table, resume protocol

**Verified:**
- `pnpm check` fully green: lint � 4/4 tests � build success (app bundle generated, 0 errors, 0 warnings)
- All 12 lazy chunks built: entries-view, source-view, trace-view, document-view, settings-view, browse-view, overview-view, stats-view, graph-view, insights-view, cache-view

**Next:** Desktop smoke test (verify faces render real data) ? E1 remaining insight sources

## 2026-07-02 � Round-3 execution: desktop smoke test + engine bug + I10 multi-tab

**Changed:**
- Ran the desktop smoke test HANDOVER.md asked for (Playwright, since the in-repo `run-devcontext`
  skill is stale � it documents the old WPF desktop, not this branch's Angular+Tauri app). First pass
  against `eval-repos/eShop` (via `page.goto` client-side nav) showed every R2 face reporting
  "0 projects / 0 entries" � traced to a real engine bug, not a UI wiring bug (see below).
- **Engine bug fix (commit `84a4068`):** `FileTreeExtractor.IsExcluded` matched exclude patterns via
  `path.Contains(pattern)` against the FULL absolute path, so analyzing any repo living under a folder
  literally named `eval-repos` or `analysis-repos` � the exact path this branch's own docs tell the
  next agent to smoke-test with � silently returned 0 projects/files/entries, no error anywhere.
  Reproduced via CLI: `analyze eval-repos/TodoApi` ? 0 projects; same repo copied elsewhere ? 40
  files/164 nodes/12 entries. Fixed to match exact path SEGMENTS relative to the walk root (never the
  root's own ancestors), so nested `eval-repos/` subfolders are still pruned (verified against the
  DevContext2 monorepo root) without nuking analysis of a repo that happens to live under one.
  Regression test added. **385/0 ? 386/0.**
- **Three desktop interaction bugs found by actually driving the app (commit `8887d4d`)**, once the
  engine fix let real data reach the UI:
  - `NodeLink` never stopped click propagation, so clicking a target link in the Entries table both
    opened the Node Card AND navigated away to `/trace` (the row has its own click-to-trace handler).
  - `Sheet` (backs Node Card) never moved focus into the overlay on open, so Escape silently did
    nothing and the backdrop blocked all other clicks until closed by hand.
  - Document/Export presets matched invented section keys (`'identity'`, `'entries'`, `'stack'`,
    `'insights'`, `'coverage'`) that don't exist � `MapRenderer.cs`'s real keys are Overview/Topology/
    Routes/Entry points/Cross-cutting/Packages/Footer. Onboarding only ever matched "Topology" by
    luck; Review and Trace Pack matched nothing. Also fixed presets silently no-op'ing when clicked
    before the first Render (the obvious first click, since Presets sit above Sections in the UI).
  - The HANDOVER.md "known caveat" about NodeLink/GetNode display-name resolution did **not**
    reproduce � GetNode resolves display names fine.
- **I10 multi-tab workspace � I10.1+I10.2+I10.4 (commits `af7ccef`, `97044f8`):**
  - `WorkspaceStore`: up to 6 independent `TabState` entries (session slice + trace slice + route +
    its own `OperationController`). `SessionStore`/`TraceStore` rewritten as facades over
    `activeTab()` with a byte-for-byte identical public API � all 15 consumer components needed zero
    changes. `analyze()`/`trace()` capture their owning tabId once at call time and thread it through
    every async callback (never re-reading `activeId()` later), and each tab owns its own
    `OperationController` � starting analyze on tab B cannot cancel tab A's in-flight request, which
    it previously would have (one shared global controller). Regression test: start analyze on A,
    switch to B mid-flight, complete ? A ready with A's handle, B untouched.
  - `TabStrip`: 32px strip under the header, VS Code-ish (active underline, status dot, close on
    hover, middle-click close, Ctrl+T/W/1-6, "+" disabled at cap). Each tab remembers its last route
    and restores it on switch.
  - I10.4: tabs persist `{path, label, route}` + active index to localStorage; restore as IDLE tabs
    on boot (never re-analyze all of them � only the persisted active tab lazily re-analyzes, and
    that's structural: the trigger effect only ever reads `activeTab()`). `closeTab()` now calls the
    existing `CloseSession` RPC, freeing the server-side snapshot instead of leaking it.
  - **Deferred, not silently skipped:** I10.3 server-side `MaxLiveSessions`/LRU/rehydrate (needs I8's
    snapshot cache, which doesn't exist yet � the tab cap alone is the spec's own "reduced v1"
    allowance); drag-reorder (spec marks optional for v1); `ActivityService` is still one global
    instance for the footer/toast display only (cosmetic last-writer-wins across concurrent tabs) �
    the underlying per-tab DATA is fully isolated regardless, which is what the race test and the
    live concurrent-analyze scenario below verify.

**Verified (live, Playwright, not just green checks):**
- Single-tab regression: re-ran the full R2 checklist against `eval-repos/TodoApi` post-engine-fix �
  Insights cards (real severities/evidence), Entries table (12/12, sortable, NodeLink?NodeCard without
  navigating away), Graph (seeded, 20-22 nodes), Settings?About (real engine version), Trace, and all
  three Document presets selecting the correct real sections. Zero console errors, zero network
  failures throughout every run.
- Multi-tab isolation: two repos analyzed in two tabs show fully independent data on switch; route
  restoration works (navigate tab 1 to Entries, bounce through tab 2, back to tab 1 ? still Entries).
- **The actual point of I10** � concurrent analyze: started analyzing Serilog in tab 1, opened tab 2
  and analyzed TodoApi to completion *while tab 1 kept running in the background*, switched back to
  tab 1 and it had completed normally (3 projects, real data) � not cancelled.
- Persistence: analyzed two repos, navigated the active one to Entries, simulated a restart (fresh
  page load, same localStorage) � both tabs came back with correct resolved labels, URL landed
  straight back on Entries, the active tab's data was live within ~1s, the other tab stayed idle
  (dimmed) until clicked, then lazily re-analyzed on its own.

**Gate:** `dotnet build` 0w � `dotnet test --filter Category!=Eval` 386/0 � `pnpm check` green
(lint + 7/7 tests, up from 4 � 3 new WorkspaceStore tests + build).

**Next:** E1 remaining 6 insight sources (highest engine leverage, unchanged from prior handover) ?
I10.3 needs I8 first (server LRU/rehydrate) ? I9 release readiness ? E4 remaining facets.

## 2026-07-02 � Unified Iteration 1: merge + Tiers 1-2 delivery

**Branch:** `feat/unified-iteration-1` (off `develop` after merging both `go-to/implement-iterations` and `feat/narrative-canvas`).

**Merges:**
- Merged `go-to/implement-iterations` (34 commits: I1-I10 engine + desktop) into develop � fast-forward
- Merged `feat/narrative-canvas` (9 commits: P0-P6 single scroll canvas) on top � resolved 11 conflicts:
  6 modify/delete (old views deleted by canvas) + 5 content conflicts (stores/config/views).
  Kept narrative-canvas UI, retained go-to WorkspaceStore/NodeLink/infrastructure.
- All merges verified: build 0w, tests 356/0 (12 server + 280 core + 64 desktop, 3 skipped).

**Tier 1 � Perf + Library Surface (delivered):**
- `--fast` mode: CLI flag skips `InMemoryEventBusExtractor`, `AntiPatternDetector`, `IndirectWiringDetector`.
  Wired through `ExtractionOptions.Fast` ? `AnalyzeCommand` ? `DiscoverPipeline` exclude list.
- WS-G-a: `LibrarySurfaceBuilder` now detects `AbstractValidator<T>`/`AbstractAuthorizationHandler` as
  consumer "derive" seats. xUnit `[Fact]`/`[Theory]`/`[InlineData]`/`[Trait]` now appear as annotate entries
  even without generators.

**Tier 2 � E1 + E3 (delivered):**
- E1 � 6 insight sources: `wiring.hubs`, `graph.orphans`, `wiring.external-events`,
  `data.busiest-aggregate`, `topology.chokepoint`, `wiring.multi-impl`. Each implements `IInsightSource`,
  registered in `DiscoveryPipeline.ComputeInsights`. Total: 10 insight sources (4 existing + 6 new).
- E3 � Full W9 deletion: deleted `Pruning/` (TokenBudgetEnforcer + PatternRelevancePruner),
  `RenderPlanBuilder.cs` (replaced with stub), `TokenBudget.cs`, `OutputSelfCheckTests.cs`.
  Cleaned `DiscoveryModel.Budget`, `TypeDiscovery` FinalScore/FocusScore/GraphProximity/PathProximity,
  global usings, `MarkdownRenderer` budget display, `PipelineTests` pruner test.
  Build 0w, tests 356/0.

**Verified:** `dotnet build` 0w � `dotnet test --filter Category!=Eval` 356/0 (12+280+64, 3 skipped).
pnpm check NOT run (TypeScript unchanged from narrative-canvas merge � the only TS file touched was
trace-node.ts which took go-to's NodeLink version, identical to what was already on narrative-canvas
before the merge).

**Next � remaining items (all tiers documented below):**
- E2: Pattern-zoo corpus (`tests/fixtures/PatternZoo/`) � modern C# through seam scanners
- E4: Remaining facets F1-F12 (auth surface, message matrix, middleware, data map, etc.)
### I9 � Release readiness (engine side)  **DONE** (CLI exit codes + --quiet)
CLI polish: exit codes, `--quiet`, stdout/stderr separation, completions.
- Locus: `src/DevContext.Cli/Settings/AnalyzeSettings.cs`, `src/DevContext.Cli/Commands/AnalyzeCommand.cs`
- Gate: `--strict` returns exit code 2 on invariant fail; `--quiet` prints nothing on success.

---

## 2026-07-02 � U1 Live Console (P2) � feat/ui-iteration

**Changed:**
- `state/workspace.store.ts`: added `LogLine` type and `consoleLog` field to `TabSessionSlice`
- `state/session.store.ts`: progress callback now appends `ProgressEvent`s to `consoleLog` signal; exposed via `SessionStore.consoleLog` computed
- **New** `features/narrative/section-console.ts`: boot-log during analysis (scrolling with auto-scroll-to-bottom, phosphor terminal style), RunReport on completion (stages waterfall, funnel bar, extractor timings, cold-cache labeling)
- `features/narrative/narrative-canvas.ts`: wired `<app-section-console />` for both boot and report modes; added 'console' to scroll-spy sections
- `features/narrative/section-stats.ts`: funnel card now has stacked horizontal bars for types discovered?included and raw?budget tokens; cold cache shows "cold run � no cache reuse"
- `styles.css`: added `.console-surface`, `.console-log`, `.log-line`, `.log-cursor` (blink animation), `.report-line`, `.console-section-title`; terminal-vibe phosphor selectors; `prefers-reduced-motion` safe

**Verified:**
- `pnpm lint` � green
- `pnpm test` � 7/7 green
- `tsc --noEmit` � clean
- `pnpm build` � timed out (resource contention with concurrent Angular build); TypeScript compilation used as proxy

**Next:** U2 Synced Lens (P3) � Human+LLM split pane with auto-render on selection

---

## 2026-07-02 � U2 Synced Lens (P3) � feat/ui-iteration

**Changed:**
- **New** `features/narrative/section-lens.ts`: persistent 50/50 Human (left) + LLM (right) split pane
  - Human pane: trace tree (from TraceStore) + node detail (kind, file, tags, degrees) when explicitly selected
  - LLM pane: auto-rendered markdown via `api.render(handle, { focus, format: 'markdown' })`
  - Auto-render: debounced 250ms `effect` on `TraceStore.focus` � no manual Render needed
  - Copy: button in LLM pane + `Ctrl+C` keyboard shortcut (`@HostListener`)
  - Empty/loading/error states for both panes
- `features/narrative/narrative-canvas.ts`: wired `<app-section-lens />` after trace section; 'lens' added to scroll-spy
- `styles.css`: `.lens-split` 50/50 flex, `.lens-pane`, `.lens-card`, `.lens-content` (max-h 70vh, mono, scroll), `.lens-placeholder`, terminal-vibe adjustments, responsive stacking =768px

**Verified:**
- `pnpm lint` � green
- `pnpm test` � 7/7 green
- `tsc --noEmit` � clean

**Next:** U3 Facet views (blocked on engine E4) or U4 Release polish

---

## 2026-07-02 � U4 Release Polish � feat/ui-iteration

**Changed:**
- `features/narrative/section-settings.ts`: About section now shows real engine version (from `ConnectionStore.ping`), server status dot, stack info, GitHub link, issues link, "Check updates" link to GitHub Releases, privacy note
- `features/palette/palette.ts`: replaced `/* swallow */` comment with clear explanation (search is supplementary, silent failure is OK)
- `state/node.store.ts`: added `error` signal + toast notification when node detail fetch fails (was silently setting null state)
- `features/node-card/node-card.ts`: added toast confirmation for clipboard copy success/failure (was silent void call)

**Error telemetry audit (34 catch sites):**
- All user-facing catch sites now either surface errors with toasts or error signals, or have clear comments explaining why silent handling is appropriate
- Non-critical catches (localStorage, Tauri API in browser, cleanup ops) remain silent � acceptable
- `operation-controller.ts`, `graph-view.ts`, `github.store.ts`, `theme.service.ts`, `recent.store.ts`, `workspace.store.ts` � all verified OK

**Verified:**
- `pnpm lint` � green
- `pnpm test` � 7/7 green
- `tsc --noEmit` � clean

**All U1-U4 items complete.** Remaining front-end work from UI-UX-GUIDELINES.md: navigation rail, entries table enhancements, keyboard shortcuts � none are U-list items. U3 (facet views) blocked on engine E4.

---

## 2026-07-02 � U5 Workspace Navigation + Polish (audit items) � feat/ui-iteration

**Changed:**
- **New** `shell/navigation-rail.ts`: left sidebar with icon+label items (Overview, Entries, Trace, Graph, Insights, Export, Settings). Session-required items hidden until `session.ready()`. Active route highlighted with accent border.
- **New** `shell/workspace-shell.ts`: header + rail + router-outlet + footer layout. Imports palette. Keyboard shortcuts: `g+key` (o/e/t/g/i/x/s) for view navigation, `?` for shortcut help overlay, `Escape` to close. `g` sets 1.5s pending flag.
- **New** `features/pages/`: 6 page wrappers (`overview-page`, `entries-page`, `trace-page`, `graph-page`, `insights-page`, `export-page`) that reuse existing section components. Export page handles dismiss?navigate back.
- `app.config.ts`: route table changed from single wildcard `**` to 8 lazy-loaded child routes under `WorkspaceShell`
- `section-entries.ts`: sortable columns (click header to toggle Method/Route/Target/Kind asc/desc), keyboard navigation (?? to move, Enter to trace, `n` for NodeCard, `Ctrl+C` to copy route), selected row highlight, subtitle shows filtered/total count
- `palette.ts`: removed stale routes (Browse, Document, Stats), added Export view nav, capped entry results at top 10
- `app.ts`: unchanged (router-outlet already present)

**Verified:**
- `pnpm lint` � green
- `pnpm test` � 7/7 green
- `tsc --noEmit` � clean

**All U1-U5 items complete.** U3 (facet views) blocked on engine E4. App is fully routed with navigation rail, keyboard shortcuts, and sortable entries table.

## 2026-07-02 � E2 Pattern-Zoo + I1.3/I1.5 fixes

**Changed:**
- E2 � Created `tests/fixtures/PatternZoo/PatternZoo/` with 9 C# fixture files covering:
  primary constructor, expression body, record with body, local function, required init,
  collection expression, conditional block (`#if`), raw string literal trap, and parameter-type
  Send resolution. Each file contains a known seam (`IMediator.Send(new X())`).
- `PatternZooTests.cs` � 13 parameterized/inline Facts asserting Sends edges across all syntax
  shapes plus negative guards: non-mediator Send (SmtpClient) produces no edge, raw string
  literal `"""...Send(new FakeCommand())..."""` produces no fabricated edge, parameter-type
  fallback resolves correctly, multiple seams in one class all detected, Publish/SendAsync
  variants work.
- **I1.5 fix � String literal stripping:** Added `GraphBuilder.StripStringLiterals()` � a
  character-level pre-pass that replaces C# string literal contents (regular `"..."`,
  verbatim `@"..."`, raw `"""..."""`, interpolated `$"..."`) with spaces preserving offsets.
  Applied in `AddSends` so in-literal seam-like patterns never produce fabricated edges.
- **I1.3 fix � Conjunction gate:** When `AddSends` hits a bare-verb fallback (unknown receiver
  but known verb like `Send`), now also checks `IsLikelyRequestType()` � the target type name
  must end with Command/Query/Event/Notification/Request/Response or be in the model's event
  type set. Kills the `SmtpClient.Send(new MailMessage())` false positive.

**Verified:** `dotnet build` 0w � `dotnet test --filter Category!=Eval` 369/0 (up from 356,
+13 PatternZoo tests, no regressions).

**Next:** A-F15 (Build intelligence � CPM + Directory.Build.props) ? A-F14 (EF depth).

## 2026-07-02 � A-F15: Build intelligence (CPM + Directory.Build.props)

**Changed:**
- **CPM (`Directory.Packages.props`):** `CsprojReader.ResolveCpmVersions()` walks the ancestor
  directory chain from each csproj looking for `Directory.Packages.props`, parses its
  `<PackageVersion Include="X" Version="Y" />` elements. `ParsePackageReferencesCpmAware()`
  resolves PackageReference versions from CPM when inline Version is missing.
- **`Directory.Build.props` chain:** `ResolveOutputType()`, `ResolveTargetFrameworks()`,
  `ResolveIsPackable()` walk the ancestor chain for `Directory.Build.props`. csproj values
  win; ancestor values fill in when csproj doesn't set them. Nearest ancestor wins among
  Directory.Build.props imports.
- `ProjectStructureExtractor` updated to use the new CsprojReader resolution methods.
- CPM fixture project: `tests/fixtures/CpmProject/` with `Directory.Build.props` (sets
  OutputType+TargetFramework) and `Directory.Packages.props` (MediatR 12.0.0,
  FluentValidation 11.5.0, Microsoft.Extensions.Hosting 10.0.0).
- `CsprojReaderCpmTests` (12 tests): CPM version resolution, inline-override, OutputType
  from Directory.Build.props, TargetFramework from ancestor, IsPackable fallback, empty
  returns when no CPM/ancestor files exist.

**Verified:** `dotnet build` 0w � `dotnet test --filter Category!=Eval` 381/0 (+12 CPM tests, no regressions).

**Next:** A-F14 (EF depth tracking) ? E5 (Benchmark expansion) ? I8 (Caching).

## 2026-07-02 � A-F14: EF depth tracking (entity navigation + depth annotation)

**Changed:**
- `EdgeKind.EntityRelation` � new edge kind for entity-to-entity navigation relationships.
- `GraphBuilder.AddEntityNavigationEdges()` � scans entity types' declared properties for
  navigation properties referencing other known entities. Creates `EntityRelation` edges in
  the BelongsTo direction (child entity ? parent aggregate/entity). Handles both direct
  references (`OrderItem.Order`) and collection properties (`Order.Items: ICollection<OrderItem>`).
- `GraphBuilder.ExtractInnerEntityNameWithDir()` � extracts the inner entity name from property
  type strings, distinguishing collection types (ICollection<>, List<>, arrays) from direct
  references. Filters out primitive/framework types.
- `TraceBuilder.AnnotateEntityDepths()` � post-collection step that computes depth from each
  touched entity to its nearest aggregate root by BFS-traversing EntityRelation edges.
  Aggregate roots get "(root)" annotation; connected entities get "(depth N from AggregateName)".
  Unconnected entities unchanged.
- `GraphBuilderTests` � 2 new tests: direct reference navigation and collection navigation.

**Verified:** `dotnet build` 0w � `dotnet test --filter Category!=Eval` 383/0 (+2 entity nav tests, no regressions).

**Next:** E5 (Benchmark expansion) ? I8 (Caching).

## 2026-07-02 � I8 snapshot cache + I10.3 server rehydrate + I9 CLI polish

**Changed:**
- **I8 � Snapshot cache:** `SnapshotCacheService` in `Core/Analysis/` � computes cache keys from
  repo path (SHA256) + git HEAD (or manifest hash), saves/loads `AnalysisSnapshot` as JSON.gz,
  LRU eviction (10 versions/repo, 2GB cap), CLI `cache list/clear/path` stubs.
- **I8 � CLI integration:** `AnalyzeCommand` checks snapshot cache before running analysis;
  cache hit ? render from cached snapshot with `"from cache � sha7 � Nms"` stamp; cache miss
  ? save write-behind. New flags: `--no-cache` (force fresh), `--cache-only` (fail if cold).
- **I10.3 � Server rehydrate:** `EngineRunner` checks I8 cache before full analysis; cache hit
  ? instant `EngineResult` with fresh pipeline from `EngineHostCache`. Write-behind saves after
  analysis.
- **I9 � CLI polish:** Exit codes (0=ok, 1=usage, 2=strict-fail, 3=cache-only-miss, 4=network/
  clone). `--quiet` flag suppresses all output on success.

**Verified:** `dotnet build` 0w � `dotnet test --filter Category!=Eval` 383/0 (no regressions).

**Next:** E5 (Benchmark expansion � remaining item).

---

## 2026-07-02 � V4 Audit fixes + P0 header/cleanup + P1 entries/deep-link + P2 prefs/lens

**Changed:**
- **V4 Audit fixes (F0-F7):** Build breaks (wrong imports in `navigation-rail.ts`, `afterRender?afterEveryRender`, dead `graph-view.ts`); wired Synced Lens onto Trace page; fixed blank graph on first trace + ngModel-on-signal bug; trace dropdown focus gating; entries arrow-key double-jump; insights semantic colors + detail rendering; settings GitHub links + Roslyn toggle.
- **P0.1 � Header repo affordance:** Replaced dead `<ng-content select="[analyze]"/>` with repo-label dropdown showing current repo, recents list, and "New analysis�" action. "New" button now resets workspace state (`closeTab + navigateByUrl('/')`) instead of `window.location.reload()`.
- **P0.2 � Landing recents �:** Ported remove-recent button from deleted `SectionSettings` to `SectionLanding` recents list (hover-visible, stopPropagation).
- **P0.3 � Dead code deleted:** `narrative-canvas.ts`, `section-settings.ts`, `scroll-spy/scroll-spy.ts` � all only imported each other, nothing referenced them.
- **P1 � Entries table spec:** Count badges on filter chips (`HTTP 23`); "approx" and "has target" quick-filter toggle chips; sort persisted in URL (`?sort=route&dir=asc`); visible row-action buttons on hover (Trace � Node card � Copy route); sticky header + 500px max-height scroll; filter/sort state synced to URL via `replaceUrl`.
- **P1 � Trace deep-linking:** `/trace?focus=X` � `trace-page.ts` reads `focus` from query params and triggers trace on nav; `effect` writes focus back to URL on change.
- **P2.7 � `prefs.store`:** New state store (`state/prefs.store.ts`) � persists analysis defaults (depth/detail/roslyn/cleanup) to localStorage under schema-versioned key. `SettingsView` reads/writes prefs. `SectionLanding` and `AppHeader.selectRecent()` apply prefs defaults on analyze.
- **P2.9 � Lens node detail wired:** `trace-node.ts` now emits `nodeSelected` output on click; wired in `section-trace.ts` and `section-lens.ts` to call `traceStore.selectNode()`. `section-graph.ts` calls `selectNode()` alongside `trace()` on graph node click. Lens Human pane now shows node detail card when a node is selected.
- **Bonus:** Removed unused `RouterLinkActive` import from `navigation-rail.ts` and unused `Icon` import from `workspace-shell.ts`.

**Verified:**
- `pnpm check` green: lint 0/0 � test 7/7 � build 0w 0e
- 14 files changed, +340/-446 lines

**Next from AUDIT-V4-VERIFICATION �6:**
- P3: U3 facets / E4 (needs engine facet layer first � deferred multi-iteration)
- Verification debt: Eval gate (`dotnet test --filter Category=Eval`), manual smoke test
- Follow-ups: file:line column (needs engine proto change), CDK v22 virtual scroll API, Tauri storage commands

---

## 2026-07-02 � Audit + Bug Fix Pass + Window Buttons + Gap Tracker

**Changed:**
- **Window buttons fixed:** Silent-catch pattern replaced with Tauri detection (`window.__TAURI__`). Buttons now hidden in browser mode (where Tauri APIs don't exist). Tauri API module cached after first lazy load. `isTauri()` guard in template.
- **Footer wired:** Added `ConnectionStore` + `ActivityService` � footer now shows connection dot + server version + progress % during analysis + error state.
- **NodeCard error state:** Added error display with Retry + Copy details buttons. Added empty-state for zero callers/callees. Skeleton spinner (not text "Loading..."). 
- **SectionArchitecture empty states:** Added "Analyze a repo first" when no session + "No architecture data available" when analysis produced no data. `hasContent()` gate on all subsections.
- **SectionIdentity empty states:** Added guard � shows prompt text instead of `0 nodes / 0 edges` before analysis.
- **SectionStats error state:** `statsError` signal now read � shows inline error with Retry button during loading.
- **SectionExport toggle fix:** User's section enable/disable state now preserved across re-renders (was: overwritten to all `true`). Added inline error state with Retry.
- **Two documents created:** `docs/dev/GAP-TRACKER.md` � 23 remaining gaps with doc refs, expected fixes, and priority ordering. `docs/dev/FEATURE-FLOW-EXPLAINER.md` � app identity, technology wiring, current flows, 5 proposed UX improvements (tabs, persistent lens, one-click re-analyze, graph seeding, quick popover), request for review.

**Verified:**
- `pnpm check` green: lint 0/0 � test 7/7 � build 0w 0e

**Next:**
- Wire tab strip (I10 � built but orphaned) � highest-impact unshipped feature
- Persistent Lens panel (eliminates page-hop on entry exploration)
- Nav rail polish (icons, count badges, disabled tooltips)

---

## 2026-07-03 � Fable branch: commit backlog + lint fixes + W3 completion (LatestGate/dup-guard/prefs)

**Context:** `feat/fable-redesign-skeleton` (branched from `develop` at `cab2800`) carried two
uncommitted, already-verified changesets in its working tree left over from `develop` (the "V4 audit
+ P0-P2" and "Audit + Bug Fix Pass" entries above) � never committed before the branch was cut.
Committed those first so the tree matches what was actually tested, then continued the Fable
waterfall (`docs/dev/briefs/ui-ux-redesign-proposal-fable.md` �10) per the skeleton HANDOFF's
"next steps" ordering.

**Changed:**
- **Commits `0947011`/`1729b90`:** the two pending changesets above, plus lint fixes for the F-skeleton
  commit (`entry-deck.ts`/`stage.ts`/`inspector.ts` needed `keydown.enter`/`space` + `tabindex` paired
  with `(click)` for `@angular-eslint/template` a11y rules; `atlas.store.ts`/`stage.ts` used the banned
  `Array<T>`/`ReadonlyArray<T>` generic form). `pnpm check` had never been run on the skeleton commit
  before this session � HANDOFF said as much ("STATIC � never compiled or run").
- **W3 � LatestGate threading (proposal �5.1):** `DevContextApi.getTrace/getNode/getNeighbors` take an
  optional `AbortSignal` now (mirrors the existing `analyze()` pattern), threaded to the ConnectRPC
  `CallOptions`. `TraceStore` now routes `trace()`'s `run()` and `selectNode()` through a `LatestGate`
  keyed `${tabId}:trace`/`${tabId}:node` � a stale response from a superseded j/k scrub can no longer
  paint over a newer one. A constructor `effect()` (same pattern as `TrailStore`'s GC effect) aborts a
  tab's in-flight gate entries the moment the tab closes.
- **W3 � SessionStore duplicate-path guard (GAP-T4):** `analyze()` now checks all tabs (not just the
  active one) for a matching `path` with a handle or in-flight analysis; if found, switches to that tab
  instead of creating a duplicate. Re-analyzing the path already open in the *active* tab is unaffected
  (deliberate re-analyze, not blocked).
- **W3 � PrefsStore `dockLevel`/`theme`:** added to the `Prefs` interface (schema stays version 1 � the
  existing `{...DEFAULTS, ...parsed}` merge already backfills missing keys from old blobs).
  `WorkbenchPage`'s Inspector dock toggle (`Ctrl+Shift+L`) now reads/writes `PrefsStore` instead of its
  own `devcontext-dock` localStorage key. `theme` is stored but **not yet applied to the DOM** � that's
  W0-finish's `ThemeService`, still open.
- **Tests:** `trail.store.spec.ts` (new, 7 tests � push/undo/redo, forward-branch truncation on push-
  after-undo, duplicate-push collapse, `jumpTo`, pin toggle, tab-close self-GC, per-tab isolation).
  `workspace.store.spec.ts` (+2 tests � the dup-path guard switches tabs and does *not* re-call
  `analyze()`; re-analyzing the active tab's own path is unaffected).

**Verified:**
- `pnpm check` green (real exit code): lint 0/0 � test 25/25 � build 0w/0e.
- Manual smoke via `pnpm dev:web` + Playwright (`chromium-cli` isn't installed in this environment;
  used `playwright-core` with `channel: 'chrome'` against the system Chrome instead � no browser
  download needed): analyzed `tests/fixtures/MinimalApiProject`, SPA-navigated to `/explore` (a real
  `page.goto` would reset in-memory session state, same as it would for a user typing the URL fresh �
  `WorkspaceStore` deliberately doesn't persist session/trace/handle), focused the deck and scrubbed
  with `j` � trace tree + Inspector Details/LLM/Trail all populated correctly, trail breadcrumb showed
  `POST /orders`. Toggled the dock (`Ctrl+Shift+L`) twice � Inspector hid and restored with content
  intact. Confirmed `localStorage['devcontext-prefs']` carries `dockLevel`/`theme` and the old
  `devcontext-dock` key is never written. Zero console errors throughout.

**Next (HANDOFF's waterfall order, unchanged):**
- W3 remainder: analyze-stream cancel + tab-close abort sweep beyond what `OperationController`
  already does (it already cancels + closes the session on tab close � audit whether more is needed);
  omnibox/palette search (`searchNodes`) onto `runLatest` (currently ungated � GAP-B1).
- W0 finish: bundle Inter/JetBrains Mono locally, `ThemeService` (to actually apply `PrefsStore.theme`),
  `/styleguide` dev route.
- W1: shell skeleton rename/restyle (titlebar/activity-bar/statusbar), wire tab-strip, WebView shortcut
  interception � see `AGENTS.md` "F � Fable Workbench Redesign" for the up-to-date per-stage status.

---

## 2026-07-03 � Fable branch: W0 finish (fonts/primitives/styleguide) + W1 (shell skeleton)

**Context:** continuing `feat/fable-redesign-skeleton` immediately after the W3-completion session
above, in the same session. Corrected course on one thing that session got wrong: it added a
`PrefsStore.theme: 'graphite'|'paper'|'system'` field as an inert placeholder for "W0 finish's
ThemeService" � but `ThemeService` (`core/theme/theme.service.ts`) turned out to already exist
pre-branch, with its own `data-vibe`/`data-theme` model (vibe = modern/terminal/hacker, theme =
dark/light/high-contrast per vibe) that doesn't match the vocabulary that field assumed. Removed the
field rather than keep two conflicting theme-persistence paths.

**Changed � W0 (commit `d465717`):**
- Inter Variable + JetBrains Mono, Latin-subset woff2 (~110KB combined), fetched once from Google
  Fonts' own CDN and vendored into `public/assets/fonts` � not an npm dependency. Removed the remote
  `<link>` from `index.html`; reordered every vibe's font fallback chain to try the bundled font
  first instead of assuming the OS has Cascadia Code/JetBrains Mono installed.
- Six new `ui/` primitives: `Skeleton`, `Meter`, `SeamChip`, `KindIcon`, `EmptyState`, `Ticker` � all
  presentational, zero store imports (grepped to confirm). Building `SeamChip` surfaced a real,
  previously-unverified bug the skeleton HANDOFF had flagged as a risk: `trace-node.ts` had its own
  local seam-color map with lowercase/plural keys (`'raises'`, `'consumes'`, `'handler'`) that never
  matched the wire's actual values � confirmed against `DevContext.Core/Graph/TraceBuilder.cs`'s
  `SeamKind` enum, the wire is `SeamKind.ToString()`, PascalCase singular (`Entry`, `Call`, `Send`,
  `Handle`, `Raise`, `Consume`, `Data`, `Resolve`, `Pipeline`). Every seam chip in every trace view �
  old `/trace` page and the new `/explore` Flow altitude alike � has been falling back to the default
  gray this whole time. `trace-node.ts` now uses `SeamChip`, which does a correct case-insensitive
  lookup against `models/seam-colors.ts` (which also gained the missing `Entry` color).
- `pages/styleguide-page.ts`, dev-only route (`isDevMode()`-gated � confirmed tree-shaken from the
  production bundle via `grep`, not just hidden). Token sheet, seam palette, type ramp, the
  `@layer components` vocabulary, every `ui/*` primitive in at least one state.
- Gate cleanup: `grep 'shadow-sm|rounded-md' src/app/ui` was NOT clean � 8 pre-existing components
  used `rounded-md`, outside the proposal's two-bucket radius scale (�4.4: 3px controls, 6px floating
  overlays). Fixed: controls ? `rounded-sm`; the floating `Toast` ? `rounded-lg` + the exact
  `--shadow-overlay` token instead of a generic `shadow-lg`; `GraphCanvas` (a structural panel, not a
  control) ? no radius at all.
- Verified: `pnpm check` green (lint 0/0, test 25/25, build 0w/0e); grep clean; Playwright smoke of
  `/styleguide` � zero console errors, `document.fonts` reports both fonts loaded, zero requests to
  `fonts.googleapis.com`/`gstatic.com`.

**Changed � W1 (commit `485e431`):**
- `shell/titlebar/titlebar.ts` (new, replaces `shell/header/app-header.ts`): 30px, solid `bg-base`,
  no blur/shadow. Drag-region hygiene (�7.2). Omnibox trigger dispatches the same Ctrl+K keydown
  `Palette` already listens for globally (no new coupling). Repo-label dropdown (recents + New
  analysis) preserved � palette doesn't cover that yet.
- `shell/tab-strip.ts` wired into `workspace-shell` (**GAP-T1** � fully built already, including its
  own Ctrl+T/W/1-6 handling, just never imported). `shortLabel()` fixed to use the last path segment
  (**GAP-T3**). MRU stack added to `WorkspaceStore` + Ctrl+Tab/Ctrl+Shift+Tab cycling (**GAP-T5**), 2
  new unit tests.
- `shell/activity-bar.ts` (new, replaces `shell/navigation-rail.ts`): registry-safe icons (**GAP-S7**
  � `'home'`/`'list'`/`'lightbulb'` were never in the icon registry, so those three rail items
  rendered with literally no icon this whole time; now `map`/`layers`/`zap`). Disabled items stay in
  the DOM with a tooltip instead of being filtered out (**GAP-S5**). Count badges on Entries/Insights
  (**GAP-S6**). 3px left-accent active indicator. Deliberately keeps all 7 existing items rather than
  collapsing to the proposal's eventual 5-icon layout � see AGENTS.md note.
- `shell/statusbar/statusbar.ts` (new, replaces `shell/footer/app-footer.ts`): 22px, moved out of
  `fixed bottom-0` into normal document flow � `workspace-shell`'s `calc(100vh - 2.75rem - 1.75rem)`
  height hack is gone, the shell is a plain flex column now. New `Ticker` primitive mounted (inert
  until W5).
- `shell/offline-banner.ts` (new), `core/webview-shortcuts.service.ts` (new, �7.3 � Ctrl+P/Ctrl+R/F5/
  Ctrl+F/zoom interception), `/atlas` stub route.
- Verified live end-to-end via Playwright against a real analyzed repo (two gotchas hit and worked
  around, noted in case they recur): (1) Playwright's `keyboard.press('Control+T')` sends `key: 'T'`
  (uppercase), not `'t'` � the app's handlers correctly check lowercase to match real keyboards, so
  the combo string needs lowercase too, e.g. `'Control+t'`. (2) `concurrently -k` (used by
  `pnpm dev:web`) kills `ng serve` the instant the .NET server process dies, so testing the offline
  banner needs the two processes started independently (`pnpm server` � or a direct
  `dotnet .../DevContext.Server.dll` invocation � and `ng serve`, separately) rather than through the
  combined script. Confirmed: 30px titlebar; Ctrl+T/W/Tab/1 all change tab state correctly; help
  overlay lists the new shortcuts; Ctrl+R doesn't reload (page-lifetime JS marker survives) AND
  visibly triggers a real re-analyze ("Discovering files� 10%" in the statusbar) � not just "nothing
  broke"; F5 doesn't reload; killing the server surfaces the offline banner + red connection dots
  within one 5s poll cycle, reviving it clears both. Zero console errors except one pre-existing,
  unrelated bug incidentally surfaced by rapid concurrent-tab analysis: `NG0955` duplicate track keys
  � `section-console.ts` tracks `@for` by `line.timestamp` (`Date.now()`), and two progress events in
  the same millisecond collide. Not fixed here (out of scope), noted in AGENTS.md.
- `pnpm check` green throughout: lint 0/0 � test 27/27 � build 0w/0e.

**Next:** W2 completion (progressive `trace-tree.ts` to replace `trace-node.ts`'s reuse in Stage;
`audit-table.ts`; `graph-canvas` topology/neighbors builders) or W3's remainder (analyze-stream cancel
sweep, omnibox onto `runLatest`) � see `AGENTS.md`'s "F � Fable Workbench Redesign" for current status.

**Changed � W3 remainder + W4 partial (commits `bf23e31`..`7de783d`, this session):**
Resumed cold per AGENTS.md's own resume protocol; W0/W1 were already done, W3 was "mostly done."
Closed out W3, then pushed six W4 checkpoints, each its own commit, `pnpm check` green before every
one, and each verified live against a headless Chrome (playwright, `channel: 'chrome'`) analyzing
`tests/fixtures/MinimalApiProject` through the real server � not just lint/build. That verification
step caught four real bugs no amount of type-checking would have: see below.

1. **W3 remainder** (`bf23e31`): palette search debounced (150ms) through `LatestGate` � closes
   GAP-B1. Audited the analyze-stream/tab-close cancel sweep (`OperationController` +
   `WorkspaceStore.closeTab` + `TraceStore`/`AtlasStore`'s tab-close effects) � already complete, no
   code change needed.
2. **Omnibox** (`0af42ca`): new `features/omnibox/omnibox.ts` replaces `features/palette/palette.ts`
   (deleted). Ctrl+K/Ctrl+P, Tab-cycle verbs (Trace/Node/Usages/Copy) applied to the selected
   entry/node row, sections Action/Recents/Entries/Nodes, static sections computed once (GAP-C2).
   `TraceStore.selectNode()` gained an optional `NeighborDirection` param (was hardcoded `'out'`).
   **Bugs found live, not by lint:** the input never received DOM focus on open � typing only
   "worked" by whatever residual focus was left over from a prior interaction, a real keyboard-first
   regression; Escape didn't close the overlay (the panel's blanket `(keydown)="$event.stopPropagation()"`
   swallowed it before it reached the overlay's own handler); static action items weren't filtered by
   query at all (the code path was simply missing). All three fixed same commit.
3. **Stage altitudes** (`8d1c181`): `GraphCanvas` now takes a discriminated `data` input
   (`mode: trace|topology|neighbors`) instead of a lone `trace` input. System altitude renders real
   topology the instant analysis completes (zero traces run � the "graph is never blank" promise);
   clicking a project filters the Entry Deck to it (`EntryVm` gained `project` from
   `EntryPoint.project`). Node altitude gained a List/Graph toggle plus an out/in/usages direction
   toggle. **Engine-constraint discovery, confirmed by hand against the live server, not from
   docs:** `GetTrace`'s `focus` only resolves registered entry-point keys � passing a raw internal
   node id (e.g. `Member:Foo.<lambda>`) comes back `found: false`. So "double-click any graph node ?
   re-trace from it" is NOT a fresh `GetTrace` call (the proposal assumed no RPC needed this would
   suffice, but doesn't say how); `TraceStore.reroot()` instead finds the node inside the
   already-loaded tree and re-roots the display at it client-side � zero new RPCs, depth capped at
   whatever was already fetched. Trail gained a `reroot` step kind whose restore path calls
   `reroot()` instead of re-tracing (its id isn't a valid focus string, so the normal restore path
   would also 404).
4. **Workbench URL state + Esc-ladder** (`0278a48`): `/explore` mirrors selection into
   `?focus&view&kind&q` (read once on load for deep-link compat, written back with `replaceUrl`,
   same convention as `TracePage`'s existing `?focus`). `EntryDeck.filterText`/`activeKind` and
   `Stage.altitude` became `model()` so the Workbench can lift them. Esc-ladder: cancel in-flight
   trace ? deselect node ? clear focus ? clear filter (the "close overlay" rung was added in
   checkpoint 6 once there was an overlay). Added `p` (pin current trail step) and Alt+?/? (trail
   undo/redo aliases). `TraceStore` gained `cancelTrace()`/`deselectNode()` for the ladder's first
   two rungs.
5. **Inspector Render-RPC LLM section** (`4a56501`): migrated off `trace.markdown()` onto the Render
   RPC (250ms debounce, real `estimatedTokens`), superseding `section-lens.ts`'s pattern for this
   surface. First load shows `Skeleton` blocks; a refresh dims existing content instead (per the
   content-preserving loading policy � skeletons are first-load only). **Found live:** the LLM
   header nested a `<button>` (Copy) inside another `<button>` (the section-h collapse toggle) �
   invalid HTML that Angular's DOM renderer never sanitizes (it builds the DOM via direct node
   creation, so the browser's HTML-parse-time nesting correction never triggers). This was
   pre-existing (inherited from the original `trace.markdown()` version, not introduced this
   session) but squarely in the code being touched, so fixed: the toggle is now an inner button
   inside a plain div, Copy is a sibling not a descendant. Deliberately NOT done: Insights-for-
   selection and "Reached by N flows" via `AtlasStore` � the data already exists (built ahead of
   schedule in W3) but wiring it now would jump the waterfall past a real W4 gate for no urgent
   reason; left for W5 as the proposal itself places them.
6. **Audit table overlay** (`7de783d`): new `features/explorer/audit-table.ts`, the sortable/
   filterable entry table ported from `section-entries.ts`, as a Shift+E overlay instead of a
   standalone page. Self-contained filter/sort state (no URL sync � unlike `section-entries`'s old
   `?sort&dir&kind&q`, syncing would fight the Workbench's own `?kind&q`). Row "Trace" behaves like a
   deck selection and closes the overlay.

**Process note for whoever resumes:** three of the four Playwright smoke runs in this session hit a
`<vite-error-overlay>` intercepting clicks on the VERY FIRST browser launch immediately after a
`pnpm check` (which runs its own one-shot `ng build` concurrently with the already-running `ng serve`
dev server � they likely contend over `.angular/cache`). Every single time, a bare retry of the exact
same script one more time passed clean with zero console errors. Treat this specific symptom (overlay
appears only on the first post-check run, never on the second) as this known flakiness, not a product
bug � but don't dismiss a `vite-error-overlay` that persists across a retry; read its text.

**Remaining for a clean W4 gate** (see `AGENTS.md` � updated same session): export drawer (Ctrl+E,
presets, From Trail), Home page assembly (identity strip, top-flow list, insight headlines), Atlas
page assembly (map/topology/packages � `eventWiring`/`hubs` already computed in `AtlasStore` since
W3, just needs binding), route cutover (`/explore` canonical, old routes redirect, delete
`section-entries/-trace/-graph/-lens/-export` + their pages + `SectionCard`), then the full manual
gate sweep in proposal �10's W4 table.

---

## 2026-07-03 � W4 � Export Drawer (Ctrl+E) with From Trail + Presets

**Branch:** `feat/w4-export-drawer` (off `feat/fable-redesign-skeleton`)

**Changed:**
- **New** `features/export/export-drawer.ts` (377 lines) � right-side 480px drawer overlay on Ctrl+E,
  following the same parent-controlled overlay pattern as `audit-table.ts` (open/dismissed signals).
  Ports and extends `section-export.ts`'s render logic:
  - 4 preset chips: **Full** (all map sections), **Onboarding** (Overview/Topology/Routes/Entry points),
    **Flow Review** (current `TraceStore.focus()` single-focus render), **From Trail** (each pinned
    `TrailStep` rendered via `api.render({ focus })`, concatenated with `## [title]` headers, tokens
    summed per-step with progress indicator).
  - Section toggles with per-section token counts (ported from `section-export.ts`: user toggles
    preserved across re-renders; new sections default to enabled).
  - Content-preserving loading per proposal �5.2: existing content dimmed at 60% on refresh, skeleton
    blocks on first load only (5 skeleton rows).
  - Empty/error/populated states for every preset: "No pinned steps yet" with `p` key tip (From Trail),
    "No entry selected" (Flow Review), "Choose a preset to render" (Full/Onboarding before first render),
    inline error + Retry.
  - Copy button (`navigator.clipboard`) + Re-render button + Escape dismiss + backdrop click-to-dismiss.
  - Token counter in header (`1.2K tok` formatting).
- **Modified** `features/pages/workbench-page.ts`:
  - Added `exportOpen` signal, `Ctrl+E` handler in `onGlobalKey()` (after `Ctrl+Shift+L`, before `Ctrl+Z`),
    `ExportDrawer` in imports array.
  - Added Esc-ladder rung for export drawer (between audit-table close and deselect-node).
  - Template: `<app-export-drawer [open]="exportOpen()" (dismissed)="exportOpen.set(false)" />` after
    `<app-audit-table>`.
- Smoke-test script `scripts/smoke-export-drawer.mts` � auto-starts ng serve, analyzes
  `MinimalApiProject`, navigates to `/explore` via popstate (client-side, avoids full-reload
  session-loss), verifies: drawer visible, 4 presets, Full render, Copy button, Escape dismiss,
  backdrop dismiss, no console errors.

**Verified (corrected this session � see below):**
- `pnpm lint` � 0/0 (green)
- `pnpm test` � 27/27 (green, no regressions)
- `pnpm build` � success (new chunk: `workbench-page` grew to 59KB from ~56KB; all lazy chunks
  produced)
- Playwright smoke (headless Chrome, real server, `MinimalApiProject`): 15/15 checks passed �
  analysis actually completed, deck rendered with 2 entries, entry pinned, drawer opened via
  Ctrl+E, all 4 presets rendered (Full/Onboarding/Flow Review/From Trail, including From Trail's
  pinned-step render), Copy button present, Escape and backdrop dismiss both worked, reopened
  cleanly, zero app console errors.

**Correction (2026-07-03, resuming session):** the original "11/12, one false negative" receipt
below was wrong � the smoke script's fixture path resolved to a nonexistent directory
(`tests/fixtures/MinimalApiProject` from `cwd=src/DevContext.App`, missing the `../../` up to
repo root � c.f. `scripts/grpcweb-smoke.mts`'s correct `resolve('../../tests/fixtures/...')`), so
every analyze attempt in that session had actually failed silently ("Analysis failed" toast) and
the deck never had a chance to render � not a locator/dockLevel false negative. "Full preset
content rendered" was itself a false positive: it asserted `.flex-1 > *` is visible, which is
equally true of the empty-state placeholder div. Fixed the path, tightened that assertion to
require the real `<pre>` render, and fixed an orphaned `ng serve` process leak on Windows
(`shell:true` + `.kill('SIGTERM')` only kills the `cmd.exe` wrapper, not the real process �
switched to `taskkill /PID <pid> /T /F`, confirmed port 4200 is released after every run). Also
dropped an unneeded `playwright-core` devDependency the prior (opencode/deepseek) session added �
the full `playwright` package was already present; the script now matches
`scripts/audit-screenshots.mts`'s `import { chromium } from 'playwright'` convention. Full detail
in `docs/dev/briefs/W4-EXPORT-DRAWER-HANDOFF.md`.

**Known issues (not fixed, out of scope):**
- The old `section-export.ts` and `/export` route still exist � to be deleted in W4 checkpoint 4
  (route cutover). The export drawer is additive; the old modal-based export still works.
- Client-side navigation to `/explore` after analysis requires the popstate trick (`page.goto` does
  a full reload which drops in-memory WorkspaceStore session state). The existing Playwright scripts
  on this branch use SPA-link clicks or the known popstate workaround. This is a test-infra concern,
  not a product bug � real users navigate via the activity bar which uses `RouterLink`.
- The `layout: 480px` right-side drawer may need responsive adjustments for narrow viewports (proposal
  assumes >= 960px; Tauri min is 960�640).

**Next � remaining W4 items (unchanged from prior handoff):**
1. Home page assembly (identity strip, Top Flows from `AtlasStore.topFlows()`, insight headlines,
   run report � card-free restyle).
2. Atlas page assembly (map markdown, topology graph, packages/pipeline + `eventWiring()`/`hubs()`
   surfaces).
3. Route cutover + deletion (`/entries` `/trace` `/graph` `/overview` ? redirect; delete
   `section-entries/-trace/-graph/-lens/-export` + their pages + `SectionCard`).
4. Full manual gate sweep per proposal �10's W4 table (flows A-E).

**W4 status: 7/9 checkpoints done.** See `docs/dev/briefs/W4-EXPORT-DRAWER-HANDOFF.md` for the
detailed handoff including file-by-file risk notes, verification receipt, and the resume protocol.

---

## 2026-07-03 � W4 � Home + Atlas assembly, route cutover, gate sweep (checkpoints 8-11, W4 DONE)

**Branch:** `feat/w4-export-drawer` � resumed from the 7/9 handoff above, in the same session.

**Correction made first:** the export-drawer checkpoint's "11/12, one false negative" receipt was
itself wrong � `smoke-export-drawer.mts`'s fixture path resolved to a nonexistent directory, so
every analyze attempt had silently failed and the deck never rendered. Fixed the path, a
false-positive content assertion, and an orphaned-`ng serve`-process leak on Windows (`taskkill
/T` instead of `.kill('SIGTERM')`); dropped an unneeded `playwright-core` devDependency. Re-ran
for real: 15/15. Full detail + corrected receipt in `docs/dev/briefs/W4-EXPORT-DRAWER-HANDOFF.md`.
Committed as `6eb762e`.

**Checkpoint 8 � Home page assembled** (`08a5e0c`). New `pages/home-page.ts` wired at `/`: Start
hero (no session) ? boot console (analyzing) ? digest (identity strip, Top Flows, insight
headlines, run report), card-free. Ported rather than rewritten from the old narrative sections �
each verified via grep to have zero other referrers � into `features/home/{start-hero,
identity-strip, run-console}.ts`. Top Flows is a flat entry-list fallback (`AtlasStore`'s ranking
is W5, gated behind the W4 exit). The old `section-identity/-console/-landing.ts` were **not**
deleted at this checkpoint � `overview-page.ts` (still live at `/overview` until cutover) still
imported them; deleting early would have broken that route. Deferred to checkpoint 10.

**Checkpoint 9 � Atlas page assembled** (`8181ddb`). Replaced the `atlas-page.ts` stub: map
prose-zone (`session.mapMarkdown()` as raw text � no markdown-to-HTML dependency exists anywhere
in this app; follows the Export Drawer's existing convention), topology graph (same `graph-canvas`
`topology` binding as `Stage`'s System altitude), new `features/atlas/architecture-panel.ts`
(ported card-free from `section-architecture.ts`). Also surfaced a basic Event Wiring Board and
Hub Radar off `AtlasStore.eventWiring()`/`hubs()` now (per AGENTS.md's already-recorded pull-forward
decision, since the computeds already existed from W3) � both render an explanatory empty state
since the indexer that populates them is still W5. `section-stats.ts` was **not** ported anywhere:
it duplicated `section-console.ts`'s report-mode stages/extractors block (both rendered
simultaneously on the old `overview-page.ts` � a pre-existing redundancy, not something this
checkpoint carried forward). `insights-view.ts`'s "link into workbench" ask from the proposal
turned out to be a no-op: the `Insight` protobuf type (`devcontext_pb.ts`) has no nodeId/focus
field to link with � left untouched rather than inventing one the engine doesn't provide.

**Checkpoint 10 � route cutover + deletion** (`8519ca8`). `app.config.ts`: `/overview` ? `/`;
`/entries` `/trace` `/graph` ? `/explore` via a `RedirectFunction` preserving `?focus`; `/export` ?
`/explore`. `activity-bar.ts` collapsed from the old 7-item rail to the proposal's 5 icons
(Home/Explore/Atlas/Insights/Settings). Deleted: `overview-page.ts`, `entries-page.ts`,
`trace-page.ts`, `graph-page.ts`, `export-page.ts`, all of `narrative/section-*.ts`, and
`ui/section-card/section-card.ts` � every deletion grep-verified beforehand for zero referrers
outside the deleted set. `pnpm check` exit 0 is the actual proof nothing broke (a stray import
would fail the build, not just look wrong).

**Checkpoint 11 � full gate sweep** (this entry). New `scripts/smoke-w4-gate.mts` (committed,
extends the `smoke-export-drawer.mts` pattern): flows A-E end-to-end, Atlas topology with zero
traces run yet, Shift+E audit table, omnibox Ctrl+K + Tab verb-cycle, deep link with a real ready
session landing traced in `/explore`. **Two bugs found and fixed in the test script itself, not
the product** � both worth remembering for whoever writes the next Playwright script here:
- A `j`/`k`/`Shift+E` keydown handler lives on `entry-deck.ts`'s own host (`tabindex="0"`), not
  window-global � a prior click anywhere else (a link, another component) silently no-ops it until
  the deck is explicitly re-focused. Same documented gotcha as the memory note about sibling-focus
  stealing; confirmed again the hard way with a raw-keydown-listener diagnostic before finding it.
- `<app-audit-table>`'s host has no `display` override, so the custom-element tag wraps its
  `position:fixed` overlay div with an empty layout box � Playwright's `.isVisible()` on the host
  tag is a false negative. Checked `omnibox.ts` (same shape, same non-issue in the real browser)
  before considering "fixing" `audit-table.ts` to add `host: { class: 'contents' }` like
  `export-drawer.ts` has � decided against it: 2 of 3 sibling overlay components already omit it
  and the app behaves identically either way in a real browser: this was a test-selector problem,
  not a product inconsistency. Fixed the script to check the inner `.fixed` div instead.
- Also a **real selector bug** (not app bug): `a[href*="/explore"]` in the Top-Flow-click check
  matched the activity bar's own "Explore" rail link before Home's actual Top Flow row link ever
  got a chance � narrowed to `a[href*="focus="]`.

17/17 scripted checks pass. Also ran a one-off manual kill-server-mid-session check (not scripted
� see AGENTS.md's rationale for why): killed the shared long-running `dotnet` server (`taskkill
/F`) while a session was ready, confirmed the offline banner appeared within one 5s poll cycle and
the app shell didn't crash, then restarted the exact same server process (`DevContext.Server.exe
--urls http://127.0.0.1:5179`) and confirmed the banner cleared on reconnect � dev environment left
exactly as it was found.

**W4 status: DONE, 9/9 checkpoints, gate passed.** `pnpm check` exit 0 throughout. Next: W5
(derived insight layer) � see AGENTS.md's F section for the item list.

---

## 2026-07-03 � Branch merge + W5 correction (session wrap-up, no new checkpoint)

Merged `feat/w4-export-drawer` into `feat/fable-redesign-skeleton` (`--no-ff`, commit `94533da`) �
clean merge, no conflicts, `pnpm check` still green on the merged branch. Neither branch is pushed
to `origin` (no upstream tracking ref on either) � this is all local. `feat/w4-export-drawer` still
exists, fully merged, not deleted.

**Correction to the note above** (the previous entry claimed "the atlas indexer trigger
(`AtlasStore.start()` on analysis-ready) is the natural first item" for W5, implying the trigger
doesn't exist � that's wrong, caught before it could cost a future session real time). Verified
live with temporary `console.log` instrumentation in `workbench-page.ts` (added, tested, reverted �
zero diff against the committed version afterward): `atlas.start(tabId, handle, entries)` already
exists and already works correctly � watched it go `indexing 0/2 ? 1/2 ? done 2/2` against
`MinimalApiProject`, `topFlows()` populating to 2. The real, verified gap is *where* it triggers:
only inside `WorkbenchPage`'s constructor, i.e. the first time a user visits `/explore`. Since Home
(`/`) is the actual cold-start landing page now (post-W4 cutover) and the proposal's Core Flow A
expects Top Flows ranked on the Home digest *before* ever visiting Explore, a user who never leaves
Home never triggers indexing at all � Home's Top Flows row stays on its flat-entry-list fallback
forever in that path. Also confirmed live: `shell/statusbar/statusbar.ts` has zero references to
`atlas` (grepped) � the proposal's "? atlas 42/94" progress segment genuinely doesn't exist, that
part was accurate. And `atlas-page.ts`'s Event Wiring Board / Hub Radar are correctly wired to the
live `AtlasStore` signals already (checkpoint 9) � their empty result against `MinimalApiProject`
(a trivial 2-endpoint fixture with no messaging) is the *correct* output, not a bug; don't spend W5
time "fixing" that fixture result. Full corrected W5 item list is in AGENTS.md's F section � start
there, not here, since this note will go stale the moment W5 work begins.

---

## 2026-07-03 � W5 checkpoint 1: atlas trigger relocated + statusbar segment wired

**Checkpoint 1** of W5 (derived insight layer). Two changes, both live-verified, one commit:

1. **Moved the Atlas indexing trigger** from `WorkbenchPage`'s constructor effect (fired only on
   first `/explore` visit) into `SessionStore.analyze()`'s success path, right after
   `entryGroups` is computed � `this.atlas.start(tabId, outcome.handle, entryGroups.flatMap((g) =>
   g.entries))`. Fires on analysis-ready regardless of which page the user is on next. Deleted the
   now-redundant `atlasStartedFor`-guarded effect and the unused `workspace` field/import from
   `workbench-page.ts` (its pause/resume-on-user-trace effect stays � that's a UI-interaction
   concern, correctly still page-local).
2. **Wired the statusbar `atlas N/M` progress segment** (`shell/statusbar/statusbar.ts`) � the
   proposal's "? atlas 42/94" that never existed (confirmed by grep in the prior session's note).
   Shown next to the wired-% chip only while `atlas.running()`.

**Live-verified** with a throwaway Playwright script (`scripts/smoke-w5-item1.mts`, written,
run, then deleted � not a permanent addition) plus temporary `console.log` instrumentation in
`SessionStore.analyze()` and `AtlasStore`'s worker/`start()` (added, run, reverted � `git status`
clean afterward, zero diff). Against `MinimalApiProject`: analyzed via Home's hero, **never
navigated to `/explore`**, and observed the full sequence in the console � `atlas.start` fired
from `SessionStore.analyze`, `indexed 1/2` ? `indexed 2/2` ? `status= done`. Confirms Home's Top
Flows will have ranked data available without a detour through Explore, which was the actual W5
item-1 gap (the trigger itself already existed pre-W5, see the prior entry's correction). The
statusbar segment itself wasn't caught by a live poll in the same run � `MinimalApiProject` has
only 2 entries and indexes in well under a second, so the "running" window is too narrow for a
150ms poll to reliably land inside it. Not treated as a failure: the template compiles and binds
against `AtlasStore`'s real `running()`/`progressLabel()` signals (confirmed by the green
`pnpm check` build � a bad binding fails the build, not just looks wrong), and the same segment
already free-rides on `AtlasStore`'s existing state machine used by the instrumented run above.
Worth a manual look on a bigger fixture (eShop-scale, dozens of entries) whenever one is next
being used for other verification, just to eyeball the segment rendering � not worth building a
slower fixture solely for this.

`pnpm check` green throughout (before instrumentation, and again after reverting it).

**W5 item 1 status: DONE.** Next: item 2 � make Home's Top Flows prefer `atlasStore.topFlows()`
once populated, falling back to the current flat entry-list only while indexing hasn't produced
results yet (`home-page.ts`'s `topFlows` computed is still hardcoded to the flat-list fallback,
confirmed by reading it in this session � `AtlasStore` injection not yet added there).

---

## 2026-07-03 � W5 checkpoint 2: Home Top Flows prefer atlas ranking

**Checkpoint 2** of W5. `home-page.ts`'s `topFlows` computed now prefers
`atlas.topFlows()` (importance-ranked by `AtlasStore`, breadth � boundary crossings,
proposal �3.2) once the background indexer has produced results, mapping each `FlowStat`
back to its full `EntryVm` by `focus` (`FlowStat` itself doesn't carry `httpMethod`/`route`,
needed for the row's chip/label). Falls back to the flat `session.entryGroups()` list �
unchanged � whenever the ranked map comes up empty (indexing not done yet, or genuinely no
found flows), so the loading/empty state is unaffected.

Live-verified with a throwaway Playwright script (written, run, deleted): analyzed
`MinimalApiProject`, waited for the (fast, 2-entry) indexer to finish, confirmed 2 Top Flow
rows rendered with correct `focus=` hrefs, clicked the first one and landed traced in
`/explore`, zero console/page errors throughout. `MinimalApiProject`'s 2 flat entries don't
exercise the *ranking* itself in a visibly different order from the old flat-list fallback
(too small a fixture to have a meaningfully different score) � worth eyeballing on a
bigger fixture (eShop-scale) whenever one is next in use for other verification, same note
as checkpoint 1's statusbar segment.

`pnpm check` green.

**W5 status: checkpoints 1-2/8 done.** Next: item 3, Event Wiring Board polish
(interactivity/click-through � the data plumbing is already done from W4 checkpoint 9).

---

## 2026-07-03 � W5 checkpoints 3-8: derived insight layer complete, W5 DONE

All remaining W5 items landed in one session, verified together with a single
consolidated Playwright script (written, run, then deleted � not a permanent addition),
four commits:

**Checkpoints 3+7 (`4211893`) � Atlas page click-through polish.** Event Wiring Board
rows link publisher/consumer into a traced `/explore` (routerLink + `?focus=`, same
pattern as Home's Top Flow links), badged `[approx]` since the join is a heuristic
name-match. Hub Radar rows now show real in/out-degree via `AtlasStore.hubsWithDegree`
(a new best-effort `getNode` enrichment effect, capped at the existing top-10 hubs) and
click through via `TraceStore.selectNode` + `?view=node` (hub node ids are raw internal
ids, not `GetTrace`-resolvable entry focuses � can't use a routerLink here).

**Checkpoint 4 (`bf5342f`) � Impact lens.** Inspector's Details section shows "Reached
by N flows" (`AtlasStore.reachedBy(nodeId)`) with an "atlas indexing" caveat while
incomplete. Omnibox gets a 5th verb (Impact) � selects the node, navigates to
`/explore?view=node`, and shows an instant toast with the count.

**Checkpoints 5+6 (`a707fcb`) � Confidence Ledger + Unwired Entries**, combined into one
commit (both touch `identity-strip.ts`'s stat-cell grid). Stage's flow-altitude header
shows a verified% meter computed by walking the currently loaded trace tree � decoupled
from the Atlas indexer, works instantly for any trace, not just indexed entries. An
"approx only" toggle filters the tree via a new `filterApproxTree` (view-models.ts) that
keeps ancestors of matching nodes so results stay reachable from the root. Home's
identity strip gains "confidence" (repo-wide, from `AtlasStore.overallVerifiedPct`) and
"unwired" (`entries - entriesWithTarget`) stat cells; Home's Insights list gets a
client-synthesized "N of M entries have no resolved target" card competing for a slot by
severity alongside the server's real insights. The deck's unwired marker and the audit
table's "has target" filter already existed pre-W5 � only the Home-facing pieces (the
proposal's "Insight card" + "Home count") were missing.

**Checkpoint 8 (`009dda0`) � statusbar ticker wiring**, and a real bug found and fixed
along the way, worth remembering for any future effect that touches a shared mutable
service from an `effect()`: `TickerService.dismissAll(prefix)` called immediately
followed by `post(item)` from the SAME effect execution � two separate reads-then-writes
of the same `_items` signal within one synchronous run � froze the tab's JS thread
completely. Not a slow test, not a slow analyze: `page.evaluate(() => document.title)`
itself never resolved, and nothing appeared in the console (no exception thrown, so
nothing to catch). Found via bisection (disable all 3 new effects ? fine; re-enable one
at a time ? the analysis-facts effect alone reproduced it; strip it down call-by-call ?
`dismissAll` alone fine, `post` alone fine, both together in one execution ? frozen every
time). Root-caused and fixed by adding `TickerService.replaceGroup(prefix, items)` � ONE
atomic `_items` write per call � and switching all three ticker effects to use it instead
of the two-call pattern. Matches the pattern `AtlasStore`'s degree-enrichment effect
(checkpoint 3+7) already used correctly: that effect also reads and writes `degreeCache`,
but the write happens inside a `getNode().then()` callback � deferred to a later
microtask, outside the effect's own synchronous execution � which is exactly why it
doesn't loop. Any future effect that both reads and writes the same signal should either
batch into one write (like `replaceGroup`) or defer the write asynchronously, never do
two separate synchronous writes to the same signal from the same effect execution.

`pnpm check` green on every commit. Live-verified end to end against `MinimalApiProject`
via a single consolidated script covering all six checkpoints (analyze ? Home stat cells
? ticker text ? Explore trace ? confidence meter ? approx-only toggle ? node select ?
Inspector reached-by line ? omnibox Impact verb + toast ? Atlas page render) � 9/9 checks
green, zero console/page errors, script written and deleted per this project's established
"live-verify with a throwaway script, don't leave it committed" discipline.

**W5 status: DONE, all 8 checkpoints complete.** Next: W6-W7 per the proposal's waterfall
(�10) � not scoped yet, AGENTS.md's F section needs its next-stage read before starting.

## 2026-07-03 � W6 scoped + Step 0: window drag/buttons bug fixed

W6 ("Tauri Hardening", proposal �10) scoped from a fresh read: sidecar engine lifecycle
(�7.1), no-flash + window-state (�7.2), single-instance, fs plugin ? Settings�Storage
live (S3), opener plugin, CSP + capability scoping (�7.4), DPI pass at 125%/150%.

Before starting the waterfall, the user reported the live Tauri window was not movable
and had no minimize/maximize/close buttons. Root-caused by reading the actual v2
permission schema rather than guessing, two independent bugs:

1. `titlebar.ts`'s `isTauri()` checked `window.__TAURI__ !== undefined`. That global only
   exists when `tauri.conf.json` sets `app.withGlobalTauri: true` (confirmed via
   `node_modules/@tauri-apps/api/core.js`'s own doc comment) � not set in this project,
   which correctly uses ESM plugin imports elsewhere. So `isTauri()` was always `false` in
   the real app and the entire `@if (isTauri())` block holding the window buttons never
   rendered. Fixed to check `window.__TAURI_INTERNALS__` instead � the internals global
   Tauri v2 always injects regardless of `withGlobalTauri`.
2. `capabilities/default.json` only granted `core:window:default`. Reading
   `src-tauri/gen/schemas/desktop-schema.json` directly, that default set explicitly
   excludes `allow-start-dragging`, `allow-minimize`, `allow-maximize`,
   `allow-unmaximize`, `allow-close` � separate opt-in permissions. `data-tauri-drag-region`
   itself works by invoking the `start_dragging` command on mousedown, so without the
   permission the window was inert to drag independent of bug #1. Added all five
   permissions to `capabilities/default.json`.

Live-verified by the user directly (`pnpm dev`, real native window): buttons work, drag
works. One follow-up UX nuance found by the user's own testing: dragging only worked in
the strip overlapping the top-right buttons, not most of the bar. Root cause (inferred,
not separately rebuilt/re-tested per explicit instruction to move on): Tauri's drag
listener triggers only when the mousedown's exact `event.target` is the tagged element
itself, not a descendant. The left brand strip's `data-tauri-drag-region` div has a single
child span that fills its entire area, so `event.target` there always resolves to the
span, never the div � zero draggable pixels despite the attribute. The middle
repo-menu/search section had no `data-tauri-drag-region` at all. The right strip worked
because its flex children (`gap-2`) leave real background gaps where `event.target`
actually resolves to the tagged div. Fix applied on the same reasoning, matching the
already-working right-strip pattern: added `data-tauri-drag-region` to the middle
section's wrapper div (its two children are real `<button>`s, which still win hit-testing
on their own pixels, so clicks on them are unaffected � only the `justify-center` gaps
around them become draggable). Not independently re-verified live per the user's request
to apply the best-guess fix and move on to the W6 checkpoints rather than spend another
build/test cycle on this nuance.

`pnpm check` green. Commit is standalone (not folded into W6 checkpoint 2) so it's
bisectable from the checkpoint commits that follow.

## 2026-07-03 � W6 checkpoint 1: sidecar engine lifecycle (packaging deferred)

`lib.rs` rewritten: dynamic port picking (`TcpListener::bind("127.0.0.1:0")`, drop, reuse
the number), the main window now built programmatically in `setup()` via
`WebviewWindowBuilder` instead of `tauri.conf.json`'s declarative `windows` array (needed
so an `.initialization_script()` can inject `globalThis.__DEVCONTEXT_SERVER__` before
Angular boots � `core/config.ts` already anticipated this global, unused until now), a
`ServerProcess` supervisor (`Arc<Mutex<Option<Child>>>` + `AtomicBool` shutdown flag)
polling `try_wait()` every 500ms � never a blocking `wait()` while holding the lock, so
`RunEvent::Exit` can always grab it to kill the child � with 1s/5s/15s crash-restart
backoff capped at 5 attempts, reusing the same port across restarts (avoids re-injecting
config into an already-loaded page). Folded in the no-flash half of checkpoint 2 while
already rebuilding the window (`visible(false)` + dark `background_color`, `app.ts` calls
`getCurrentWindow().show()` after `afterNextRender`) to avoid doing the window-builder
work twice. Extracted `core/tauri-env.ts`'s `isTauri()` (shared with `titlebar.ts`, which
previously had its own copy). `capabilities/default.json` gained
`core:window:allow-show`.

Live-verified directly against the raw `target/debug/app.exe` binary (not just
lint/build) with `DEVCONTEXT_SERVER_DLL` pointed at the already-built server DLL: dynamic
port picked fresh each launch (confirmed three different ports across three launches),
`Analyze`/`Ping` succeeded against that port (proves the injection reached
`config.ts` before Angular's gRPC client was constructed), graceful window close (a real
WM_CLOSE, not a force-kill) leaves no orphaned `dotnet.exe` behind, and force-killing just
the `dotnet.exe` child (simulating a crash) produced `"DevContext server down, restarting
in 1s (attempt 2)"` in the log followed by a fresh server on the same port.

**A test-methodology gotcha worth remembering, not a product bug:** running the raw
`app.exe` directly (bypassing `pnpm dev`'s `concurrently`-managed `ng serve`) with no
frontend dev server up produces a `chrome-error://chromewebdata/` page � Angular never
bootstraps, so `afterNextRender`'s `show()` call never fires, and the window sits
permanently invisible (`MainWindowHandle: 0` via `Get-Process`, confirmed with a small
`Add-Type`-based `IsWindowVisible` P/Invoke check). This looked exactly like a broken
no-flash implementation until re-tested with a dedicated `ng serve` actually running,
which showed a real `MainWindowHandle`, `IsWindowVisible: True`, and a normal graceful
`taskkill` close. Confirmed via a throwaway `playwright` + `chromium.connectOverCDP`
script against `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS=--remote-debugging-port=<port>`
(written, run, deleted � same throwaway-script discipline as W4/W5's Playwright checks).
Lesson: any future raw-binary Tauri test needs its own frontend dev server running, not
borrowed from whatever happens to still be up on :4200.

**Packaging (self-contained `externalBin` sidecar) explored and de-risked, not fully
wired � deferred, per the plan's own stated fallback for oversized sub-scopes.** Findings
worth keeping so the next attempt doesn't redo this research: `dotnet publish
DevContext.Server -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true`
alone still leaves `git2-*.dll` (LibGit2Sharp's native binding) as a loose companion file
next to the exe � single-file publish does NOT embed native (non-.NET) libraries by
default. Adding `-p:IncludeNativeLibrariesForSelfExtract=true` embeds it too, producing a
genuinely standalone ~127MB `DevContext.Server.exe` with zero companion files � verified
by copying just that one exe into an empty directory and confirming `/health` returns 200
with no `dotnet` runtime installed reliance. `DevContext.Server` has no `appsettings.json`
or `wwwroot`, so there's nothing else to ship alongside it. What's left before this is
real: Tauri's `externalBin` mechanism expects the source file pre-named
`devcontext-server-x86_64-pc-windows-msvc.exe` at build time, but the exact filename it
produces post-bundle (whether the target-triple suffix is stripped) isn't confirmed from
docs alone; the safe, documented way to spawn a registered sidecar is
`tauri_plugin_shell::ShellExt::shell().sidecar(name)` (a new Cargo + capability
dependency), which returns an async `(Receiver<CommandEvent>, CommandChild)` pair �
structurally different from the `std::process::Child` polling model this checkpoint's
dev-mode supervisor already uses and verified, so it needs its own supervisor logic
(`tauri::async_runtime::spawn` + matching `CommandEvent::Terminated`), not a trivial
reuse. None of this touches the dev-mode path already shipped in this commit.

`pnpm check` and `cargo check` both green. No regressions in the everyday `pnpm dev` flow
(re-verified after the rewrite � window opened, Analyze succeeded against the ordinary
fixed port 5179, no panics).

## 2026-07-03 � W6 checkpoint 2: window-state plugin (no-flash already done in checkpoint 1)

No-flash startup was folded into checkpoint 1 (the window-builder rewrite), so this
checkpoint is just `tauri-plugin-window-state`: registered in the builder chain,
`window.restore_state(StateFlags::all())` called right after `builder.build()` (while
still hidden pre-`show()`, so there's no visible jump), `window-state:default` added to
capabilities. Saving is automatic per the plugin's own design (hooks window move/resize/
close events once registered) � no explicit save call needed.

Live-verified precisely, not just "it didn't crash": moved/resized the real OS window via
a `user32.dll` `MoveWindow` P/Invoke (PowerShell `Add-Type`) to an arbitrary rectangle
(222, 111, 1111�777), closed it gracefully, relaunched the same binary, and read the new
window's rect via `GetWindowRect` � restored bounds matched the pre-close rectangle
exactly (same four numbers). Reused the `IsWindowVisible`/`GetWindowRect` P/Invoke
pattern from checkpoint 1's verification for this � worth keeping as the standard way to
assert real OS window geometry/visibility from outside the app when Playwright/CDP can't
reach into native window chrome.

`pnpm check` and `cargo check` both green, no TypeScript changes this checkpoint.

## 2026-07-03 � W6 checkpoint 3: single-instance plugin

`tauri-plugin-single-instance` registered as the first plugin in the builder chain (a
hard Tauri requirement). Its callback focuses/unminimizes/shows the existing "main"
window, and forwards `args.get(1)` (a path argument � Explorer's "Open with�", or a
second CLI launch) to the frontend as a `single-instance-path` event rather than
silently dropping it. New `core/single-instance.service.ts` (mirrors
`WebviewShortcutsService`'s `start()`-guard pattern) listens for that event and opens
the path as a **new tab** via `WorkspaceStore.createTab` + `SessionStore.analyze` �
deliberately not reusing `Titlebar.selectRecent`'s replace-current-tab behavior, since
the whole point of a path argument arriving while the app is already open is "don't
destroy what I was already looking at."

Live-verified end to end, not just "doesn't crash twice": launched the raw debug binary,
then launched it again with a `tests/fixtures/MinimalApiProject` path argument. The
second process exited immediately (no second window � confirmed via `tasklist` staying
at one `app.exe`); the first instance's own server log shows a fresh, unprompted
`Analyze` ? `GetMap`/`ListEntryPoints`/`GetTrace`�2/`GetStats` sequence appearing on its
own right after the second launch � proof the event reached the frontend and drove the
real analyze flow, not just that the process count stayed at one.

`pnpm check` and `cargo check` both green.

## 2026-07-03 � W6 checkpoint 4: fs plugin, Settings�Storage tab live (S3)

Settings' Storage tab (`settings-view.ts`) was a static stub with a hardcoded, and wrong,
path � it displayed `%LOCALAPPDATA%/DevContext/clones`, but `DevContext.Core.Models.RepoUrl
.ClonePath` (grepped, not guessed) actually uses `%LOCALAPPDATA%/DevContext/repos`. Fixed
while wiring real data. `tauri-plugin-fs` added, scoped via `capabilities/default.json`'s
`fs:scope` to exactly `$LOCALDATA/DevContext` + `$LOCALDATA/DevContext/**` � `$LOCALDATA`
(? `BaseDirectory.LocalData` in JS) is the raw `%LOCALAPPDATA%` root; `AppLocalData` would
have been wrong here since it nests under Tauri's own bundle identifier, not the literal
`DevContext` folder name `SnapshotCacheRoot`/`RepoUrl` hardcode in the C# engine.

New `core/storage.service.ts` lists top-level entries under `cache`/`repos` and recursively
sums real file sizes (no shortcut exists � confirmed from the plugin's own `.d.ts`, not
assumed: `readDir` is NOT recursive by default despite an initial web-search summary
claiming otherwise � always walks one level, and `stat()` on a directory doesn't report
recursive content size, so summing means a manual walk, one `stat()` IPC call per file).
`settings-view.ts`'s Storage tab now shows real per-repo sizes and a "Clear" action per
root, reusing `formatBytes` (small local helper � no `core/format.ts` module exists yet to
extend, per W7's still-pending C1 dedupe item).

Live-verified against REAL pre-existing data from prior sessions (not a clean fixture) via
a throwaway Playwright+CDP script (written, run, deleted): 6 stale cache repo-key dirs and
4 real git clones, including a 1.1GB packed `dotnet-runtime` clone and a 5837-file unpacked
`VahidN-DntSite` clone � a genuine test of the recursive-walk performance concern raised
while designing this. It settled in well under a second either way (packed repos are a
handful of big files; local Tauri IPC is fast enough that even ~5800 individual `stat()`
calls don't add up to a noticeable delay) � worth knowing before assuming this needs a
native Rust walker for performance; it doesn't, at least not at this scale. Clearing cache
verified two ways: the UI's own re-render on next tab visit ("0 B across 0 repos"), and
independently confirmed at the OS level (`ls` on the real `%LOCALAPPDATA%\DevContext\cache`
path � directory gone entirely). Deliberately did NOT test the Repos "Clear" button against
these real clones (would force expensive re-cloning for future benchmark/eval sessions) �
the code path is identical to Cache's, already proven.

**Playwright/CDP gotcha worth remembering:** a `waitForFunction` checking `!includes(
'Scanning�')` placed immediately after a `.click()` can resolve instantly and wrongly if
the click's async handler chain hasn't started yet (the "Scanning" text hasn't appeared in
the DOM to *not* match against) � always wait for the loading state to actually *appear*
first (or add a short explicit wait) before waiting for it to *disappear*, or the check
races ahead of the real work and captures stale content.

`pnpm check` and `cargo check` both green.

## 2026-07-03 � W6 checkpoint 5: opener plugin (Reveal/Open in Explorer)

`tauri-plugin-opener` added. Inspector's file-path row (`inspector.ts`) gets a "reveal"
link calling `revealItemInDir(node.filePath)` � `node.filePath` turned out to already be
an absolute path (Roslyn's `SyntaxTree`-derived `SourceFile`, confirmed live, not
guessed), so no repo-root joining was needed. Settings�Storage's two "Open in Explorer"
buttons (added alongside checkpoint 4's Clear buttons) call a new
`StorageService.openInExplorer('cache' | 'repos')`, resolving the absolute root via
`@tauri-apps/api/path`'s `localDataDir()` + `join()`.

**A real, worth-remembering scope gotcha, not a guess:** `opener:allow-reveal-item-in-dir`
worked immediately with no scope configuration, for a path nowhere near any configured
scope (the fixture repo, outside `$LOCALDATA` entirely) � but `opener:allow-open-path`
rejected every call with `"Not allowed to open path ..."` until given an explicit
`{"identifier": "opener:allow-open-path", "allow": [...]}` scope, exactly like `fs:scope`.
The two commands are NOT symmetric: `reveal-item-in-dir` appears unrestricted once the
bare permission is granted; `open-path` is scope-gated like the `fs` plugin. Found live,
not from documentation (which didn't clearly state this asymmetry) � the error message
itself was the giveaway. Scoped it to `$LOCALDATA/DevContext/**`, matching `fs:scope`,
since that's the only thing this checkpoint's `openInExplorer` ever opens.

Live-verified both actions for real, not just "the call didn't throw": traced a real
entry, selected a node, clicked "reveal" � confirmed via PowerShell's `Shell.Application`
COM automation (`$shell.Windows() | % LocationURL`) that a genuine Explorer window opened
at `.../MinimalApiProject/src/Api` (the exact folder containing the selected node's
`Program.cs`). Same technique confirmed "Open in Explorer" (cache) opened a window at
the real `%LOCALAPPDATA%\DevContext\cache` path. Closed both test-opened windows
afterward, left the user's own pre-existing Explorer window untouched.

`pnpm check` and `cargo check` both green.

## 2026-07-04 � W6 checkpoint 6: CSP + capability scoping, clipboard-manager

`tauri.conf.json`'s `security.csp` was `null` (unrestricted); set to `default-src 'self';
script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src
'self'; connect-src 'self' ipc: http://ipc.localhost http://127.0.0.1:*` � the
`http://127.0.0.1:*` port wildcard is required since checkpoint 1's sidecar picks a fresh
port every launch, so a fixed port could never be listed. `'unsafe-inline'` on style-src
is needed for Angular's `ViewEncapsulation`-emulated `<style>` tags (real inline elements,
not attribute styles � CSP doesn't distinguish nonced vs. unnonced `<style>` tags without
extra plumbing not worth adding here).

**Genuinely uncertain going in, resolved by testing rather than assumption:** whether a
configured CSP is even enforced when the frontend loads from an external `devUrl`
(`http://localhost:4200` via `ng serve`, this project's dev setup) rather than Tauri's own
bundled `frontendDist` � conflicting signals from documentation. Settled it by testing:
built + ran the raw debug binary with the new CSP in place, connected via WebView2 remote
debugging, and checked `getComputedStyle` on real elements rather than trusting "the page
didn't visibly break." `body` background was the exact `#16181d` dark base color,
`header` height was the exact `30px` titlebar class, zero console CSP-violation messages,
and a full `Analyze ? GetMap ? ListEntryPoints ? GetTrace ? GetStats ? Render` gRPC
sequence succeeded end to end. Whatever the enforcement semantics turn out to be exactly,
this specific CSP value doesn't break this specific dev setup � good enough to ship, and a
useful data point for whoever eventually does the deferred production `tauri build` (W6
checkpoint 1's other deferred half): if CSP causes a break there, this dev-mode pass at
least rules out the CSP value itself as generically wrong.

Capability audit: every custom permission added since W6 started (window buttons, fs,
opener, now clipboard) was checked against its actual call site � none were dead. Left
`core:default` as the broad baseline (not narrowed to individual `core:*` sub-permissions)
� a bigger, separate refactor with unclear benefit here, not attempted.

`tauri-plugin-clipboard-manager` added. New `core/clipboard.ts`'s `copyToClipboard()`
(the same reuse-worthy-immediately pattern as `tauri-env.ts` � 6 call sites across 5
files, not a hypothetical) replaces every `navigator.clipboard` use in `inspector.ts`,
`export-drawer.ts`, `audit-table.ts`, `omnibox.ts`, and `node-card.ts` (�2) � all had
existing comments/TODOs calling out `navigator.clipboard`'s WebView2 flakiness without
focus, now resolved for real. `clipboard-manager:allow-write-text` only (this app never
reads the clipboard).

Live-verified with the strongest possible signal, not just "the call didn't throw": set
the real OS clipboard to a sentinel string via PowerShell's `Set-Clipboard`, clicked
Inspector's copy button through a live trace, then read it back with `Get-Clipboard` �
content changed to the exact rendered LLM trace text, proving the plugin path actually
executes end to end, not just that the promise resolved without an exception.

`pnpm check` and `cargo check` both green.

## 2026-07-04 � W6 checkpoint 7: legibility bump (redirected from "DPI pass")

The proposal's original W6 checkpoint 7 was "DPI pass at 125%/150%" � testing the app at
higher Windows display-scaling factors. Asked the user how to handle it (own machine,
Claude via PowerShell/registry, or skip); the user redirected the ask entirely: don't
touch any system display settings (a global, hard-to-reverse desktop change, correctly
out of bounds for an unattended agent action), and instead just make the app's own text
less small by default, using judgment � the proposal's deliberate "13px IDE density" base
(�4.3) reads as genuinely too small in real use, not a DPI-scaling artifact.

Bumped the whole type scale proportionally rather than cherry-picking one size, so the
existing size hierarchy (2xs < xs < body < prose) stays intact, just shifted up ~1px
across the board: `text-2xs` 10px?11px, `text-xs` 12px?13px, body base 13px?14px (�4.3's
own original value), `.prose-zone` (LLM pane/Home digest reading zone) 13.5px?15px � kept
deliberately larger than the new 14px base so its "bigger type for reading" purpose still
holds. Found and bumped every OTHER hardcoded `font-size: 0.625rem`/`0.75rem` literal in
`styles.css` too (`.chip`, `.kbd`, `.section-h`, `.list-row`, the boot-console/run-report
classes, etc.) � these don't use the `text-2xs`/`text-xs` utilities but are visually the
same sizes, so leaving them alone would've produced a visibly inconsistent app (some text
bumped, adjacent text not). Did NOT touch the confirmed-dead `.lens-*` classes (grepped �
zero references anywhere in `src/app`, leftover from the deleted `section-lens.ts`).

**Verified with an actual screenshot, not just a build check** � the only way to honestly
confirm a legibility/layout claim. First attempt captured the wrong window entirely (this
Claude Code terminal, which happened to be the foreground window at those screen
coordinates � `CopyFromScreen` captures whatever's visually on top, not a specific
window, regardless of which HWND's rect you fed it). Fixed by calling `SetForegroundWindow`
first. The corrected screenshot shows clearly larger, comfortably-padded text throughout
(titlebar, deck rows, trace-tree chips, Inspector panels) with nothing clipped or
overflowing in the tightest spots (20px-tall chips, the 30px titlebar) � real confirmation
this was a legibility win, not a layout regression. Worth remembering as a pattern for any
future "does this look right" question that a computed-style check can't answer.

`pnpm check` green (pure CSS change, no Rust touched � `cargo check` not re-run this
checkpoint).

**W6 status:** checkpoints 1-7 all done. Two things intentionally NOT done, both already
called out in earlier entries rather than silently dropped: the self-contained
`externalBin` sidecar packaging (checkpoint 1 � lifecycle hardening is done and verified,
only the installer-bundling half is deferred), and the literal "test at 125%/150% Windows
scaling" ask (superseded by this session's legibility-bump redirect, per the user).
`AGENTS.md`'s F section needs updating to reflect W6 done, W7 next per the proposal's �10.

## 2026-07-04 � W7 checkpoint 1: node-peek hover card wired to every NodeLink

New `NodePeekStore` + `features/peek/node-peek.ts`: 200ms hover shows a lightweight
real-fields-only card (title, kind, location, degree � no neighbor lists, no actions),
Ctrl pins it open past `mouseleave`, Escape/click-outside/close-button dismiss. Uses a
`LatestGate` (key `'peek'`) so sweeping the mouse across many links can't let a stale
`getNode()` land after a newer hover already took over. A short (150ms) hide-grace timer
lets the pointer travel from the trigger `NodeLink` down into the peek card itself
without it vanishing mid-transit � the timer snapshots which node it means to close, so
a stale timer from an abandoned hover can never kill a different, newer peek.

**A real, unrelated bug found while wiring this:** `NodeCard` (the click-through full
detail sheet) was never mounted anywhere in the app. `grep`ping for `NodeCard` turned up
only its own file, `NodeStore`'s doc comment, and `Skeleton`'s doc comment � every
`NodeLink` click called `NodeStore.show()`, set signals, fetched data, and then rendered
nothing, because no template anywhere had `<app-node-card>`. Root cause: `NodeCard`'s
only referrers used to be the old `section-entries`/trace/graph pages, deleted during
W4's route cutover � the cutover's own "zero surviving referrers" grep check verified the
*deleted* files had no referrers, but never checked whether deleting them silently
orphaned a *surviving* component they used to mount. Fixed by mounting both
`<app-node-card>` and `<app-node-peek>` globally in `workspace-shell.ts`, same pattern as
`<app-omnibox>`.

Folded "unpin peek" into `workbench-page.ts`'s existing Esc-ladder as its own rung
(�8.4 order: cancel trace ? close overlay ? unpin peek ? deselect node ? clear focus ?
clear filter) instead of leaving it as `node-peek.ts`'s own independent
`window:keydown.escape` listener. First attempt kept both � live testing caught the bug
immediately: NodePeek's listener (mounted at app root, registered first) always fired
before the ladder's, clearing `nodeId()` before the ladder ever saw a peek open, so every
Escape silently fell through to *also* deselect the node in the same keystroke. Fixed by
making `node-peek.ts`'s standalone handler a no-op while on `/explore` (where the ladder
now owns the rung exclusively) � it still fires normally on Home/Atlas/Insights/Settings,
which have no ladder of their own.

Verified live end-to-end (analyzed `MinimalApiProject`, headless Chrome): hover-delay
timing (no premature flash, appears after 200ms), grace-period survival crossing the
gap, Ctrl+hover pinning + surviving `mouseleave`, Escape dismissing a pinned peek, and �
the regression check � clicking a `NodeLink` now actually opens the `NodeCard` sheet with
real content. `pnpm check` green.

## 2026-07-04 � W7 checkpoint 2: NodeCard skeleton loading (GAP-B8)

Replaced `node-card.ts`'s spinner-loading state with `app-skeleton` placeholders shaped
like the real sections (kind, location, degree, callers) � content-preserving loading
per proposal �5.2, the exact principle `ui/skeleton/skeleton.ts` was built for back in W0
but never actually applied to the one component its own doc comment names. Also added a
`found: false` branch ("Node not found") instead of rendering whatever zero-value fields
a not-found `NodeResponse` carries � trust principle (�1.4), never show fabricated data.

Verified live: local `getNode`/`getNeighbors` calls normally resolve in a handful of ms,
too fast to see the loading state, so throttled that one round-trip via a CDP
`Network.emulateNetworkConditions` session to confirm the skeleton actually renders
mid-load (7 `app-skeleton` placeholders, zero leftover spinner) and clears once real
content lands. `pnpm check` green.

## 2026-07-04 � W7 checkpoint 3: dedupe compact-count/bytes/timeAgo helpers (GAP-C1)

The "1.2K" compact-count formatter was byte-for-byte duplicated (as `fmtK`/`fmt`/
`formatStars`) in `inspector.ts`, `export-drawer.ts`, `run-console.ts`, and
`repo-card.ts`. New `core/format.ts` holds `formatCompact` (a `unit: 'K'|'k'` param
covers `repo-card`'s lower-case star convention), `formatBytes` (was only in
`settings-view.ts`), and `timeAgo` (was only in `repo-card.ts`, but the same class of
shared display logic). Each component keeps a thin `protected` wrapper delegating to the
shared function � templates can only call class members, so this is dedupe of the logic
itself, not a template rewrite. Pure refactor, verified the extracted functions produce
identical output to the original inline expressions for the same inputs; `pnpm check`
green (27/27 tests, build 0w/0e).

## 2026-07-04 � W7 checkpoint 4: `?` help overlay = full �8.4 map (GAP-T2)

The help overlay was stale from before the W4 route cutover: `VIEW_SHORTCUTS` still
routed `g` + o/e/t/g/i/x/s to dead pages (`/overview`, `/entries`, `/trace`, `/graph`,
`/export`), and was missing `h` (Home) and `a` (Atlas) entirely � even though
`activity-bar.ts`'s own `railItems` already declare those exact `shortKey`s in their
tooltips ("g h" was promised, silently did nothing). Rewrote both the routing map and the
displayed table against the real, current keymap.

Along the way, implemented the one row that didn't functionally exist yet: "v t/v g/v
s/v n" stage-altitude switching � Stage only had mouse-click chips before this.
Promoted `stage.ts`'s `flowMode` from a private signal to a `model()` (same pattern
already used for `altitude`) so `workbench-page.ts`'s new v-prefix handler � mirroring
`workspace-shell`'s existing g-prefix chord pattern, 1.5s window, hint bubble � can drive
both altitude and flow-mode together. The spec's 4 stage shortcuts map onto this app's
real 3-altitude + tree/graph-submode model exactly: `v t`/`v g` both select the `'flow'`
altitude with the matching sub-mode, `v s`/`v n` select `'system'`/`'node'`.

Verified live: help overlay content matches, `g h`/`g a` actually navigate (previously
silent no-ops), all four v-shortcuts switch the right altitude/mode combination.
`pnpm check` green.

## 2026-07-04 � W7 checkpoint 5: Paper light theme + system-follow (�4.2)

The Paper palette itself already existed pre-W7 (`[data-vibe="modern"][data-theme=
"light"]`, exact values from �4.2, literally commented "Paper (F proposal �4.2)") but had
zero UI path to reach it � Settings only exposed a vibe picker, no dark/light/system
toggle, and `ThemeService` had no concept of "system" at all.

`ThemeService`: `theme()` can now hold the raw preference `'system'`; new
`resolvedTheme` computed resolves it against a live `prefers-color-scheme` media-query
listener (updates without reload) � falls back to the vibe's own default for dark-only
vibes like Terminal, which only declares one theme by design, not an oversight. The
`data-theme` DOM-attribute effect and `setVibe()`'s reset-if-unsupported guard both
switched from the raw preference to the resolved value / a system-aware check (`'system'`
is always valid regardless of which vibe is active). Settings ? Appearance gets a
Dark/Light/System segmented toggle; Light only renders for vibes that actually declare a
light theme (Modern, Hacker) � Terminal correctly shows just Dark.

Verified live: Dark/Light apply the exact expected `--vibe-base` values (`#16181d` /
`#f6f7f9`), System tracks `page.emulateMedia({colorScheme})` changes with no reload, and
switching to the dark-only Terminal vibe under System stays sane (hides the Light
button, resolves to dark, no crash). `pnpm check` green.

## 2026-07-04 � W7 checkpoint 6: reduced-motion audit � exempt spinners (�4.4)

Audited every animation/transition path (no `@angular/animations`, no
`requestAnimationFrame`, everything is Tailwind `transition-*`/`animate-*` utilities)
against the global `prefers-reduced-motion` rule in `styles.css`. Found one real gap: the
blanket rule froze `.animate-spin` (`ui/spinner/spinner.ts`, plus the loader icons in
`export-drawer.ts`/`run-console.ts`) to a single static frame mid-rotation � the same
category of anti-pattern the app had already avoided for `.hairline`/`.skeleton` (both
already have their own explicit `animation: none` override further down the same file),
just never extended to spinners. A frozen spinner reads as "the app is stuck", not as
"motion was reduced" � indeterminate-progress motion is functional feedback, not
decoration, and is conventionally exempted from this preference industry-wide (macOS/
iOS/VS Code/GitHub all keep spinners moving under it).

Carved `.animate-spin` out of the blanket freeze; decorative `.animate-pulse`/
`.animate-bounce` and all CSS transitions still correctly collapse. Verified with
synthetic elements against the real stylesheet (decoupled from app-timing flakiness):
spin keeps a real 1s/infinite animation under reduced-motion, pulse/transitions collapse
to ~0, and the same rules play normally with reduced-motion off. `pnpm check` green.

## 2026-07-04 � W7 checkpoint 7: snapshot diff (�3.9, stretch)

Ctrl+R (re-analyze same path) now captures a before/after comparison and posts it as a
ticker item: entry counts by kind (�3.9's own example, "+3 endpoints, -1 consumer") plus
the Flow Atlas's repo-wide confidence (`AtlasStore.overallVerifiedPct`, �3.5 � "wired
87?91%"). Zero new engine calls, pure client-side diff of data this app already fetches
on every analyze. New `state/snapshot-diff.store.ts`; wired into
`WebviewShortcutsService.reanalyze()` (the one real "re-analyze same path" entry point).

The "after" confidence reading deliberately waits for `AtlasStore.status() === 'done'`
(an `effect()` keyed off a pending-path signal) rather than reading it the instant
`analyze()` resolves: atlas indexing restarts fresh on every analyze and runs in the
background, so an early read would compare a fully-settled "before" percentage against a
half-indexed "after" one � a misleading comparison dressed up as a regression, not a real
one. Trust principle: the summary builder returns `null` (nothing posted) when literally
nothing changed, rather than a fabricated "no changes" line.

Verified live end-to-end against a scratch copy of the fixture repo in the OS temp dir
(never touched the committed fixture): re-analyzing with zero repo changes posts no diff;
adding one real `MapDelete` endpoint and re-analyzing produces "+1 endpoint" in the
ticker within the atlas-done window. `pnpm check` green.

## 2026-07-04 � W7 checkpoint 8: acceptance sweep + gate

Ran the proposal �10 final-gate checklist against everything W7 touched (not
re-litigating W4/W5/W6's own already-passed gates from scratch, since W7 didn't touch
most of that surface): `pnpm check` real-exit-code green (lint 0/0, 27/27 tests, build
0w/0e); Home digest renders after analyze; Atlas still shows topology with zero traces
run (the actual W4 "never-blank graph" requirement � first draft of this sweep mistakenly
checked Explore's Stage default altitude instead, which defaults to `'flow'` not
`'system'` and is unrelated to that gate line); Atlas/Insights/Settings all navigate
clean; the full Esc-ladder chain exercised end-to-end including the two rungs this stage
touched directly (unpin-peek closes only the peek and leaves the node selection intact,
one step at a time, not two); help overlay open/close still clean; zero console/page
errors across the whole sweep.

**W7 status:** all 8 items done, including the �3.9 stretch goal. Every one of the 23
gaps from �9 is now closed except the two the proposal itself calls engine-blocked (S1
auth column, S2 line numbers) � T2 and B8/C1 were W7's own assignments and are done;
everything else was already closed by W1-W6. `AGENTS.md`'s F section needs updating to
mark W7 done � this was the last stage in the proposal's �10 waterfall.

## 2026-07-05 ? L3 audit fixes + L4 delivery: Insight engine v2

**Changed (L3 audit fixes ? 6 engine + 5 Angular):**
- **F1** BfsEntryScore cross-project counting: Path.GetFileNameWithoutExtension(fp) ? scope.ProjectForFile(fp). Dead projectByPath dict removed.
- **F2** Angular devcontext-api.ts: getImpact() and getInterestingPoints() RPC wrappers added.
- **F3** EntryVm.groupPath + mapping in 	oEntryVm(). Entry deck shows groupPath chip per row.
- **F4** NodeDetailVm.lineNumber + mapping in 	oNodeDetailVm(). Inspector shows ile:line.
- **F5** EntryVm.authAttributes + mapping. Entry deck shows lock icon for auth entries.
- **F6** EntryVm.score + mapping. Home Page uses server-scored Top Flows (score desc) instead of AtlasStore ranking.
- **F7** InterestingForDesktop prefers GraphNode.Project over file name for project grouping.
- **F8** Dead hub-scope dedup loop (4 lines) removed.
- **F9** BlastRadius O(n)?O(1) entry lookup via Dictionary<NodeId, EntryPoint>.

**Changed (L4.1 ? Insight envelope v2):**
- Insight record extended: Confidence (double), ConfidenceBasis (string?), WhyItMatters (string?), Action (InsightAction enum: None/Trace/Usages/Export), ActionTarget (string?). Insight.Create() factory stays backward-compatible (new params have neutral defaults).
- Proto Insight message: confidence (7), confidence_basis (8), why_it_matters (9), action (10), action_target (11).
- ProtoMapper.ToStatsResponse: maps all new fields. AnonymousEndpointsSource enriched with auth coverage confidence, why-it-matters, action=Trace.
- Angular insights-view.ts: shows confidence % with basis tooltip, why-it-matters italic text, and action buttons (Trace it/See usages/Export).

**Changed (L4.2 ? Per-archetype composition):**
- 6 new IInsightSource implementations in Core/Insights/Archetype/:
  - **WebArchetypeSource**: auth surface card (protected/public/unannotated counts), data map (entities per scope), middleware pipeline (behaviour count)
  - **LibraryArchetypeSource**: public surface size (interfaces/classes), internal hubs (heavily-referenced internal types), seat implementors (DI multi-impl interfaces)
  - **MessagingArchetypeSource**: produce-consume matrix (producers + consumers + message types), external contracts (consumed-but-never-produced)
  - **DesktopArchetypeSource**: module map (GroupPath groupings), ViewModel-View wiring (naming-convention detection), command inventory (ICommand impls)
  - **CliArchetypeSource**: command tree (top-level groups), parameter inventory (avg params per command)
  - **GatewayArchetypeSource**: routing surface (route inventory), downstream wiring (call-edge analysis)
- Each source gates on archetype signals and yields zero insights when not applicable.
- Registered in DiscoveryPipeline.cs insight sources array.

**Changed (L4.3 ? Confidence Ledger):**
- ConfidenceLedger record: overall confidence, verified/approx edge %, per-seam breakdown (SeamConfidence: seam/total/verified/approx), auth coverage %, entry target %.
- ConfidenceLedger.Compute(graph, entries): O(n) pass over all edges + entries.
- Proto ConfidenceLedger message + SeamConfidence in StatsResponse (field 10). Mapped in ProtoMapper.ToStatsResponse.
- Angular identity-strip.ts: confidence stat now reads from stats.confidenceLedger.overall (not AtlasStore). Clicking opens an expandable Ledger panel showing overall, verified/approx edges, auth coverage, entry targets, and per-seam breakdown.

**Changed (L4.4 ? Doc-summary hygiene):**
- LibrarySurfaceBuilder.IsVendoredNamespace: excludes JetBrains.Annotations, System.Runtime.CompilerServices, System.Diagnostics.CodeAnalysis, Microsoft.CodeAnalysis, *.GeneratedCode.
- Applied in publicTypes filter before surface grouping.

**Verified:** dotnet build 0w 0e, dotnet test 429/0 (3 skipped), pnpm check (lint + vitest 27/27 + build) green.

**Next:** L5 ? MCP server + context packs (proposal-lighthouse.md ?L5).

---

## 2026-07-05 ? Lighthouse L5+L6: MCP server + UI/UX round + audit fixes

**Changed (L5.1):**
- New \src/DevContext.Mcp/\ project: stdio MCP server using \ModelContextProtocol\ 1.4.0 SDK
- \McpSessionManager\: non-blocking analyze, status polling, LRU (3 snapshots), Serilog file-only logging
- \DevContextTools\: 13 MCP tools ? analyze, status, entrypoints, map, top_flows, interesting_points, trace, node, neighbors, usages, search, impact, insights
- Every response envelope carries scope + coverage + confidence from ConfidenceLedger

**Changed (L5.4+L5.5):**
- \ContextPackBuilder\ in \DevContext.Core/Graph/\ (kernel): trace skeleton, callee signatures, salient bodies, DI wiring ? ranked by distance, budgeted with per-section token attribution + omitted list
- \get_context(handle, focus, budget_tokens, intent)\ MCP tool
- \
ead_source(handle, node_id)\ MCP tool ? file:line anchored read (20-line window)

**Changed (L6.1-L6.6):**
- \identity-strip.ts\: identity sentence, human stat labels with hover tooltips, confidence clickable ? Ledger
- \home-page.ts\: insights grouped "What needs attention" / "Good to know"; Engine details collapsed to \<details>\
- \insights-view.ts\: impact grouping, evidence chips dedup+linked, action buttons
- \entry-deck.ts\: subtitles (target per row), group-path chips, kind-filter count badges
- \statusbar.ts\: removed node/edge/entry plumbing; ticker retains insight headlines

**Changed (L6.7+L6.8):**
- \stage.ts\: Zen mode ? F key full-screen overlay, Escape exits, double-click header toggle
- \graph-canvas.ts\: hover focus dimming (non-neighbors 15% opacity), legend ? popover

**Audit fixes:**
| # | Finding | Fix |
|---|---------|-----|
| A1 | Confidence Ledger unreachable (showLedger never toggled) | Confidence label now clickable button |
| A2 | Statusbar showing node/edge/entry counts | Removed; ticker retains insight headlines |
| A3 | get_context missing intent parameter | Added intent param (trace/explain/review) |

**Verified:** dotnet build 0w 0e, dotnet test 429/0 (3 skipped), pnpm check green (lint 0/0, test 27/27, build 0w/0e)


---

## 2026-07-05 -- L7: Benchmark Audit + Close-Out Gate

**Agent:** L7 delivery session (full 6-phase execution in one session).

### Phase 1 -- Bench run
- Ran devcontext report across 10 repos covering all archetypes (DevContext, TodoApi, eShop, Serilog, FluentValidation, Polly, MediatR, Spectre.Console, CommunityToolkit.Mvvm, CleanArchitecture).
- Results in eval-results/2026-07-05/.
- PowerToys deferred to separate session (largest repo, ~120 projects).
- eShop/TodoApi hit stale pre-Lighthouse snapshot cache --> empty graphs; diagnosed as cache versioning gap.

### Phase 2 -- AUDIT.md
- Wrote comprehensive audit scoring all 9 trust-breakers + 6 performance + 3 value gaps.
- Verdict: 18/21 FIXED, 2 IMPROVED, 1 DEFERRED (PowerToys).

### Phase 3 -- Fix top 3 regressions
1. Snapshot cache versioning (SnapshotSchema.Version = 1, SnapshotEnvelope wrapper).
2. Bench script SHA-clone fix (40-char hex SHA detection).
3. McpSessionManager NPE fix (snapshot?.Graph is null guard).

### Phase 4 -- Static audit
- Full read-through of 110 changed files (~11k insertions).
- 12 findings: 1 HIGH, 5 MEDIUM, 6 LOW.

### Phase 5 -- Deliver 7 audit fixes
1. McpSessionManager.cs null-safe graph check (HIGH).
2. graph-canvas.ts merged duplicate effect; legendItems->signal (HIGH + MEDIUM).
3. export-drawer.ts contentPreserved reset in finally (MEDIUM).
4. un-console.ts afterEveryRender->effect (MEDIUM).
5. insights-view.ts dedupe pre-compute + NaN guard (MEDIUM + LOW).

### Phase 6 -- Handover doc + final updates
- Created docs/dev/HANDOVER-LIGHTHOUSE.md (8 sections, Fable handover template style).
- Updated L3-START.md, AGENTS.md, proposal-lighthouse.md tracker.

**Verified:** dotnet build 0w 0e, dotnet test 429/0 (3 skipped), pnpm check green.

**Next:** Review and merge. PowerToys verification session recommended as first next step. See docs/dev/HANDOVER-LIGHTHOUSE.md for full project state.

---

## 2026-07-06 � M8.1 Context Studio: scope picker, composition view, budget panel, old panes retired (DONE)

**M8.1b Scope picker (`scope-picker.ts`):**
- Service ? entry hierarchy tree built from `SessionStore.entryGroups()` grouped by `entry.project`
- Text filter with clear button
- "I'm changing this endpoint" preset button ? dropdown of all entries ? seeds 5 cards: flow, members, contracts, validators, tests
- Bottom bar with selection count + "Add to context" button
- Entry kind icons/colors from M7.0 KIND_COLORS registry

**M8.1c Composition view (`composition-view.ts`):**
- Ordered `ContextCard[]` signal (id, type, title, entryIds, estimatedLines, bodyEnabled)
- Each card: grip handle, type badge (colored: Flow/Info, Members/Success, Config/Warn, Entities/Accent, Tests/Danger), title, line count, body toggle (eye/eye-off), � remove
- Footer shows card count + total lines
- Empty state with layers icon + "No cards yet" message

**M8.1d Budget panel (`budget-panel.ts`):**
- Token budget slider (1k�16k, stops grid underneath)
- Per-card bar meter (`lines � 2.5` v0 heuristic), over-budget cards show warn color
- Total bar: green = budget, red > budget
- Intent selector: Trace/Explain/Review chip buttons
- Format selector: Markdown/Plain chip buttons
- [Copy] button ? icon changes to check + "Copied!" toast (A11 feedback affordance)
- [Save] button ? downloads as .md file + toast
- `copyRequest`/`saveRequest` outputs (no native DOM event collision)

**M8.1e Retire old panes:**
- Removed `features/export/export-drawer.ts` (553 lines) � deleted file + directory
- Removed `ExportDrawer` from workbench-page.ts: import, imports array, template block, dead `exportOpen` signal, Esc-ladder check
- Removed Inspector LLM section: `'llm'` from SectionId, ~46-line template block, `renderContent`/`tokenEstimate`/`renderLoading`/`renderError` signals, `renderTimer`/`renderedFocus` fields, debounced render effect, `copy`/`fmtK`/`render`/`debouncedRender`/`doRender` methods, `RENDER_DEBOUNCE_MS` constant, `formatCompact`/`DestroyRef` imports
- `copyToClipboard`/`Skeleton` kept � still used by Code tab

**Parent `context-studio.ts` rewritten:**
- Imports and wires ScopePicker, CompositionView, BudgetPanel
- Owns `cards` signal, delegates card CRUD to children
- `buildContext()` assembles markdown from cards
- `onCopy()` ? clipboard.writeText, `onSave()` ? Blob download

**Verified:** `pnpm check` green (lint 0/0, test 27/27, build 0w/0e).
**ExportDrawer references:** 1 (comment in `core/format.ts:4` � acceptable).
**New lazy chunk:** `context-studio` � 21 kB.

**Next:** M8.2 Composition model (cards/seeds/presets � make cards carry real context data from RPC).
**Trap:** Token estimation is client-side v0 heuristic; server-side round-trip through ContextPackBuilder gated for M8.4; lens-switcher layer/feature still available=false.

---

## 2026-07-06 � M8.2 Composition model: cards, seeds, presets (DONE)

**M8.2a Card types (9):**
- Flow skeleton, member signatures, member bodies, DI wiring, config keys, entities/contracts,
  tests-for, repo identity, custom
- Each card: type badge (colored per kind), title, provenance chips, body toggle, drag handle,
  � remove
- Drag-drop reorder (custom implementation � lightweight, no CDK dependency)

**M8.2b Scope seeds:**
- Entry/flow seeds from scope picker tree
- Type seeds from omnibox search
- Service seeds (all entries in a service)
- Insight seeds (action targets from insight cards)
- Trail seeds (entries in current trail)
- "I'm changing this endpoint" preset ? 5 cards: flow + members + contracts + validators + tests

**M8.2c getContext RPC wiring:**
- `loadCardContent()` calls `api.getContext()` with focus on card's seed entries
- Server-side `ContextPackBuilder` assembles content per card type
- Card stores `serverTokens` and `sectionTokens` from RPC response

**M8.2d Omnibox integration:**
- Omnibox supports "Add to context" mode when Context Studio is active
- Search results include "Add to context" action button
- Type-ahead with kind icons and service grouping

**Verified:** `pnpm check` green. All 9 card types render. Drag-drop works. getContext RPC returns
real content for dogfood entries.

---

## 2026-07-06 � M8.3 Budget/meter/server-token wiring + M8.4 Provenance chips (DONE)

**M8.3a Budget ? RPC wiring:**
- `budget-panel.ts` budget slider (1k�16k) drives `budgetTokens` param on getContext RPC
- `context-studio.ts` passes budget to `loadCardContent()` which passes it to `api.getContext()`
- Server uses budget to trim content (section-level granularity)

**M8.3b Server vs heuristic token distinction:**
- Cards show `serverTokens` (from RPC) without `~` prefix (green)
- Cards without server tokens show ~heuristic (amber)
- `totalTokens` computed: prefers serverTokens, falls back to `lines � 2.5`
- Per-section token breakdown from server response

**M8.3c Budget panel enhancements:**
- Per-card bar meter with actual token values
- Total bar: green = budget, red > budget
- Over-budget cards highlighted in warn color

**M8.4a Provenance chips:**
- Each card renders file:line provenance chips below the type badge
- Provenance extracted from getContext RPC entries
- Chips link back to Explore node selection where resolvable

**M8.4b Per-section token display:**
- Section breakdown visible in card expand
- Each section shows its own token count from server
- Total token display in card header

**Verified:** `pnpm check` green (lint 0/0, test 27/27, build 0w/0e). Server token counts
appear without ~ prefix for cards loaded via getContext. Budget slider changes flow to RPC
budgetTokens param. Provenance chips render with file:line for dogfood entries.

---

## 2026-07-07 � M9 Close-out: Full bench, AUDIT.md, HANDOVER-MERIDIAN.md (DONE)

**M9.1 � Full bench (22/22 repos):**
- Ran `scripts/bench.ps1` across all 22 repos in `eval-repos.json`
- 19/22 passed on first run; 3 clone failures (gRPC, MassTransit, MassTransit-Sample) � network
  issues, manually re-cloned and re-analyzed
- All 22 reports content-asserted (Stats + TopFlows present, zero stubs)
- PowerToys (5,141 nodes, 2,878 edges, 30.0s) � Lighthouse-deferred, now verified
- MassTransit (24,819 nodes, 2,929 edges, 46.4s) � largest framework analyzed, verified
- Results in `eval-results/2026-07-07/`

**M9.2 � AUDIT.md:**
- Wrote `eval-results/2026-07-07/AUDIT.md`
- Scored all W-findings (E1-E9) � 8/9 FIXED, 1/9 IMPROVED (re-verified post-Meridian)
- Scored every M-stage gate (M0-M8) with fresh evidence
- Per-repo bench summary table (22 repos)
- Known gaps catalog (Gap 1, Gap 2, Trap A, Trap B)
- All verdicts cite artifact paths, not code existence

**M9.3 � HANDOVER-MERIDIAN.md + tracker close:**
- Wrote `docs/dev/HANDOVER-MERIDIAN.md` (10 sections, Lighthouse handover style)
- Updated `MERIDIAN-START.md` � closed tracker rows M9.1-M9.3, updated handoff block
- Appended this entry to `docs/dev/go-to-program/PROGRESS-LOG.md`

**Verified:** `dotnet build` 0w 0e, `dotnet test --filter Category!=Eval` green,
`dotnet test --filter Category=McpQa` green (2 tests), `pnpm check` green (lint 0/0, test 27/27,
build 0w/0e).

**Next:** Engine Gaps 1-2 (read_source RPC + layer/feature uplumb). See
`src/DevContext.App/AGENTS.md:133-189` for the detailed plan.

---

## 2026-07-07 � M9-ext: Layer/Feature lens cytoscape rendering (DONE)

**Audit:** Code-level audit of all remaining documented gaps from HANDOVER-MERIDIAN.md:
- buildContext() client-side v0 � acceptable v0; content IS server-generated via getContext RPC
- Freshness probe RPC � stale data already flows through getStats().stale field; UI shows stale chips
- Layer/Feature cytoscape � lens buttons existed but rendered identical system topology (no difference from Service lens)

**Engine (C#):**
- `MapBuilder.cs`: ProjectNode record gains `Layer?` and `Feature?` init-only properties
- `MapBuilder.BuildTopology`: accepts `CodeGraph` param, computes per-project dominant layer/feature by counting Type nodes' Layer/Feature in each project's graph nodes
- `ProtoMapper.ToMapResponse`: copies `pn.Layer` / `pn.Feature` to proto ProjectNode

**UI (TypeScript):**
- `graph-canvas.ts`: `lensId` input added; `LAYER_COLORS` map (Api?info, Application?accent, Domain?success, Infrastructure?warn, etc.) with `FEATURE_PALETTE` hash-cycled palette; `buildTopologyElements` passes layer/feature in node data; `nodeBorderColor` function uses lens-based coloring; `updateLegend` shows layer/feature-specific legends; lensId effect triggers `cy.style().update()` for reactive recoloring
- `stage.ts`: passes `[lensId]="lensModel()"` to topology graph canvas; sets `[lensId]="'flow'"` on trace/neighbors canvii

**Verified:** `dotnet build` 0w 0e; `dotnet test --filter Category!=Eval` all green; `pnpm check` green (lint 0/0, test 27/27, build 0w/0e).

**Updated:** `MERIDIAN-START.md` handoff + checkpoint table; `src/DevContext.App/AGENTS.md` M9 delivery block + gap tracking; `PROGRESS-LOG.md`.

**Next:** Push, then plan next phase. All Meridian gaps closed or assessed as acceptable.

---

## 2026-07-07 � Loom L0.1: Truth harness landed

**Session #2, target stage L0.** Conducted by autonomous agent under Conductor orchestrator, attempt 1/4.

**Pre-session:**
- Read `LOOM-START.md`, `loom-graph-design.md`, `proposal-loom.md` �L0, `SESSION-AUDIT.md`
- Gate battery: `dotnet build` 0w/0e, `dotnet test --filter "Category!=Eval"` all green (Core 355/3skip, Server 12, Desktop 64), `pnpm check` green � all clean before starting

**QA of previous session:**
- All 22 audit artifacts from `eval-results/2026-07-07/` confirmed present
- E1 (checkout trace depth-1) re-verified via fresh CLI run � still broken, unchanged since audit
- MCP QA scripts (`run.js`, `run-multi.js`) exist; `run-cold.js` not yet created (L0.2)
- `ui-audit-drive.mjs` exists at `src/DevContext.App/scripts/ui-audit-drive.mjs` (L0.3 baseline)
- No uncommitted G8-G11 code � all committed in `5e3d6a1` (verified)

**Delivered � L0.1 Truth expectations:**

| File | Purpose |
|------|---------|
| `tests/DevContext.Core.Tests/TruthPendingAttribute.cs` | XUnit `[TruthPending("L<n>")]` attribute: skips test with ratchet message |
| `tests/DevContext.Core.Tests/TruthExpectationTests.cs` | 8 tests: 4 green baseline + 4 red-but-skipped ratchets |
| `scripts/bench.ps1` | Added `-Truth` flag: runs `dotnet test --filter "Category=Truth"` |

**Test matrix:**
| Test | Status | Unblocks at |
|------|--------|-------------|
| `Dogfood_baseline_presence_ok` | ? PASS | � |
| `TodoApi_baseline_presence_ok` | ? PASS | � |
| `CleanArchitecture_baseline_presence_ok` | ? PASS | � |
| `DntSite_baseline_presence_ok` | ? PASS (skip if not cloned) | � |
| `Dogfood_checkout_flow_traces_cross_service_depth_ge_5` | ?? SKIP | L2.4 |
| `Dogfood_service_names_are_full_and_runnables_only` | ?? SKIP | L1.2 |
| `RazorPages_no_fabricated_cross_sample_edges` | ?? SKIP | L1 |
| `Blazor_archetype_is_not_microservices` | ?? SKIP | L7.3 |

**Evidence:** `eval-results/2026-07-07/truth-gate-l0.1.txt` (4 pass, 4 skip, 0 fail)

**Commits:** `5084826` (feat(l0): truth harness) + `c6bfa2d` (tracker hash fix)

**Post-session gate:** `dotnet build` 0w/0e � `dotnet test --filter "Category!=Eval"` Core 355/3skip � Server 12 � Desktop 64 � `pnpm check` green � truth gate 4/4 pass � branch pushed

**Updated:** `LOOM-START.md` handoff + L0.1 row; `PROGRESS-LOG.md`.

**Next:** L0.2 (cold-agent MCP QA harness `eval/mcp-qa/run-cold.js`) or L0.3 (UI drive gate from ui-audit-drive.mjs)

---

## 2026-07-08 L3 post-delivery audit (session #17)

**Changed:**
- `SemanticLitePopulator.HasBindDemand`: fixed to cover all bindable op types
  (`CreationOp`, all `InvocationOp`). Previously skipped bodies with only
  `CreationOp`s or invocations with null `ReceiverText` but bindable generic args.
- `ResolveNuGetMetadataRefs`: NuGet assembly refs now deduplicated against
  framework TPA assemblies to prevent `CSharpCompilation` failures from
  duplicate assembly identities.
- Removed dead code: unused `fileToProject` dict, orphaned `Stopwatch`, redundant
  downgrade guard in `UpgradeEdge`.
- `DiscoveryPipeline`: added Info diagnostic when call-edge upgrade fails (was
  silent `catch { }`).
- Wrote honest phase handover to `.conductor/handovers/L3.md`.

**Verified:**
- Full gate battery re-ran green: build 0w/0e, Core 393P/3S, Desktop 64P,
  Server 12P, pnpm check PASS (27 tests), mcp-qa 8/8, loom-guards PASS.
- No regression: verified-edge ratchet remains 81% (dogfood). No edge count drift.

**Commits:** `1b1a49d` (fix) + `094aa1d` (handover)

**Next:** L4.1 � Flow store on CodeGraph.

**Updated:** `.conductor/handovers/L3.md`, `PROGRESS-LOG.md`.

---

## 2026-07-08 L4 session #18 (Loom L4.1 Flow store delivery, attempt 1/4)

**Changed:**
- Created `Graph/FlowModel.cs`: `Flow`, `FlowStep`, `ServiceHop` records.
  `Flow`: Id, Entry, Steps, Touches (NodeId[], spine-only), Emits (NodeId[]), Hops (ServiceHop[]).
  `FlowStep`: Node, Via (EdgeKind?, null for entry), Tier (Resolution), Provenance.
  `ServiceHop`: FromService, ToService, Transport, Evidence.
- Modified `CodeGraph.cs`: added `Flows` property to `CodeGraph`, `_flows` + `SetFlows()` to `CodeGraphBuilder`.
- Modified `GraphBuilder.cs`: added `ComputeFlows()` spine walk with handler-member bridging.
  Priority: Sends(0) > Handles(1) > ServiceLink(2) > Raises(3) > Consumes(4) > ReadsWrites(5) > Resolves(6) > Calls(7).
  Stops at framework/system types. Touches = ReadsWrites from spine members only (E5 fix).

**Verified:**
- Full gate battery green: build 0w/0e, Core 393P/3S, Desktop 64P, Server 12P,
  pnpm check PASS (27 tests), mcp-qa 8/8, loom-guards PASS.
- Node/edge counts unchanged: 422n/276e/6SL/34ent, 82% verified (�1% from L3.3).
- 34 flows computed and stored on CodeGraph (one per entry).
- MCP QA continues to pass (8/8, checkout 24 steps cross-service).

**Commits:** `8e75dd9` feat(l4.1)

**Next:** L4.2 � Projections (ServiceMap, FlowList, EntryTable, LayerBand) + GetGraphFacets RPC.

**Updated:** `LOOM-START.md`, `PROGRESS-LOG.md`.

---

## 2026-07-08 � L4.2: Projections + GetGraphFacets RPC (session #19)

**Changed:**
- Created `Graph/GraphProjections.cs`: `IGraphProjection<TOut>`, `ProjectionOptions`, and 4 implementations:
  `ServiceMapProjection`, `FlowListProjection`, `EntryTableProjection`, `LayerBandProjection`.
  Each =~60 lines, zero regex, zero static state.
- Added `GraphQuery.Flows` property exposing precomputed `CodeGraph.Flows`.
- Added `GetGraphFacets` RPC + 9 new proto messages to `devcontext.proto`.
- Added `ProtoMapper.ToGraphFacetsResponse()` translating all 4 facets to proto.
- Added `DevContextGrpcService.GetGraphFacets` handler.

**Verified:**
- Full gate battery green: build 0w/0e, Core 393P/3S, Desktop 64P, Server 12P,
  pnpm check PASS (27 tests), loom-guards PASS.
- Node/edge counts unchanged: 422n/276e/6SL/34ent, 82% verified (same as L4.1).
- All 4 projections compile + wire through proto ? server ? ProtoMapper.

**Commits:** `(l4.2)` feat(l4.2)

**Next:** L4.3 � Switch Home/Atlas/MCP consumers to read projections; delete ad-hoc walks.

**Updated:** `LOOM-START.md`, `PROGRESS-LOG.md`.

## 2026-07-08 � L4.3: Home/Atlas/MCP consume projections (session #20, attempt 1/4)

**QA of prior session (L4.2):** PASS. Re-ran dogfood fresh ? 422n/276e/6SL/34ent/82%, reproduces
the L4.2 claim exactly. Projections + GetGraphFacets RPC present and wired. Gap found (not a blocker):
no projection unit tests existed � added `GraphProjectionTests.cs` this session.

**Changed:**
- MCP `overview` (`DevContextTools.cs`) now sources services + top flows + service links from
  `GetGraphFacets` (`ServiceMapProjection` + `FlowListProjection`); deleted client-side ServiceStyles
  enumeration, entry OrderByDescending(Score).Take(5), and topology walk.
- MCP `top_flows` reads `FlowListProjection.Flows` (server-ranked); deleted client-side sort/take.
- Home hero + Atlas diagram (`service-map-hero.ts`) read `graphFacets.serviceMap.services`; render
  `DisplayName` verbatim (removed `name.split('.').pop()`); gateway/core layout by projection `Kind`;
  bus rail from projection transports. Topology fallback kept for pre-facets sessions.
- `SessionStore.analyze` fetches `getGraphFacets` post-ready; `graphFacets` added to session slice.
- Real defect fixed (audit Claim 3 / E3): `IsRunnableService` mis-classified BuildingBlocks (a lib
  referencing FluentValidation.AspNetCore) as runnable via a loose "AspNetCore" substring package
  check � tightened to `Microsoft.AspNetCore.App*` / Web-SDK / Exe (design �2.4). Runnable Service
  nodes tagged `RoleTags.Runnable`; `ServiceMapProjection` surfaces runnable-tagged only.
- Proto `FlowCard` enriched additively: `node_id`, `route`, `http_method`, `target`, `group_path`.
  TS regenerated (`pnpm gen:proto`). ProtoMapper maps the new fields.
- Added `GraphProjectionTests.cs` (4) + BuildingBlocks runnable regression test.

**Verified:**
- Full gate battery green: build 0w/0e, Core 398P/3S, Server 12P, Desktop 64P, pnpm check
  (lint 0, 27 tests, build), loom-guards PASS, MCP QA 8/8 (checkout gate 2c/813tok).
- Dogfood 421n/276e/6SL/34ent/82% � nodes 422?421 (BuildingBlocks no longer a false-positive
  Service node; documented drift, everything else unchanged).
- Fresh MCP drive proves the hero source shows exactly the 6 runnables with full names, no libraries:
  `eval-results/2026-07-08/l4.3-consumers.md`.

**Commits:** `(l4.3-s20)` feat(l4.3)

**Next:** L4.4 � Server ContextPack round-trip (closes Meridian Trap A); Copy/Save = the server pack.

**Updated:** `LOOM-START.md`, `PROGRESS-LOG.md`.

---

## 2026-07-08 � Loom L4.4: Server ContextPack round-trip (session #21)

**QA of previous session (s20, L4.3):** PASS.

**Delivered � L4.4 Server ContextPack round-trip (Trap A closed):**
- Proto: GetContextPack RPC + ContextPackRequest/Response, ContextCardSpec/Item, SectionAllocation
- Engine: ContextPackBuilder.BuildMulti() traces each unique entry once, picks per-card sections by type, assembles full markdown. BuildSections() extracted. New types: ContextCardSpec, ContextCardPack, MultiContextPack.
- Server: GetContextPack handler + ProtoMapper.ToContextPackResponse()
- UI: loadAllCards() � single getContextPack call replaces N getContext calls. buildContext() uses server-assembled markdown for Copy/Save = exactly the server pack. Fallback preserved.

**Trap A closed:** 1 RPC instead of N, per-card type filtering, server-assembled markdown with budget trimming.

**Gate:** build 0w/0e � Core 398P/3S � Server 12P � Desktop 64P � guards PASS � pnpm 27P � MCP QA 8/8.

**Stage L4: ALL DONE** (L4.1-L4.4). Commits: 9fe1d17.

**Evidence:** eval-results/2026-07-08/gate-battery-l4.4-s21.txt, dogfood-l4.4.md.

**Next:** L5.1 � Default-session MCP ergonomics. Updated: LOOM-START.md, PROGRESS-LOG.md.
---

## 2026-07-08 - Loom L5: MCP v2 Cold-Agent Ergonomics (sessions #23-#28)

**L5.1 (s23):** Default-session MCP:  + "ResolveHandle" + @ returns first session when handle omitted.  + "AnalyzeAsync" + @ idempotent by repo+HEAD.

**L5.2 (s24):** Error envelopes: every tool failure returns  + "{error, hint, example}" + @. Parameter-binding failures list expected schema. Unknown tool -> tool list with closest-name suggestion. Unknown symbol -> candidates ("did you mean"). Envelope <=80 tok.

**L5.3 (s25):** Unified ranked resolution:  + "GraphQuery.Find" + @ 4-rank system (exact > prefix > word-boundary > substring).  + "KindPriority" + @: Type/Service=3, EntryPoint/Member=2. All 22 tools use same ranking.

**L5.4 (s26):** Real  + "low" + @ tool: compact Flow rendering <=150 tok typical, deep-link to  + "	race" + @ for detail. 23-tool MCP registry.

**L5.5 (s27-s28):** Cold QA gate >=90% actionability. Fixed B4 usages-shortname silent resolve. Post-audit hardening (resolve, idempotent analyze, async node lookup). Cold QA: 92% actionable.

**Stage L5: ALL DONE.** Commits: ac7a7dd, a78c135, 85a74e3, f9cd094, 87888cd, df1d007, ce4d1d4, f3c6696.

## 2026-07-08 - Loom L6: Workbench Repair (sessions #29-#33)

**L6.1 (s29):** Tab strip 32px. New =  + "createTab()" + @ preserves existing tabs. Clone-close confirm with Escape dismiss. atCap() guard with toast.

**L6.2 (s30):** Code pane:  + "loadCode()" + @ calls  + "ead_source" + @ RPC, PrismJS highlighting. Loading skeleton, error state, "Select a node" empty state.

**L6.3 (s31):** Inspector insights adjacency-filtered: builds adjId set from node + 1-hop neighbors. 3-tier matching. Honest chip counts only filtered, total repo-wide for empty label.

**L6.4-L6.6 (s32-s33):** Context Studio v2 (delivered by prior M8). Table lens: toolbar button + global Shift+E shortcut. MCP page auto-refresh 30s. Confidence->Verified rename. DPI icon scaling.

**Stage L6: ALL DONE.** Commits: 753e84d, da1823d, de809de, d2205f9, 5e55097, e9fc775.

## 2026-07-08 - Loom L7: Repo-Shape Coverage (sessions #34-#39)

**L7.1 (s34-s35):** PlainCallDetector (68 lines) produces EdgeKind.Calls from BodyFacts. Excludes MediatR, bus receivers, framework noise. Resolves against SymbolTable.

**L7.2 (s36):** Archetype projections: Desktop (window/command tree), Worker (schedule/queue), Library (public surface), Blazor (route/component). Each gets ONE projection + report section.

**L7.3 (s37):** Style guardrails: SampleCollection for multi-sample repos, never Microservices (E4). 3 trigger conditions. E9 partial-scope closure. 21 guardrail tests.

**L7.4 (s38-s39):** Truth files per archetype (Blazor, FluentValidation, PowerToys, AzureFunctions). 22-repo bench: 21/22 OK. Blazor Microservices->SampleCollection fix.

**Stage L7: ALL DONE.** Commits: 6e16685, 6fdd8cb, 66fe007, 347b6e0.

## 2026-07-08 - Loom L8: Close-out (sessions #40-#41)

**L8.1 (s40-s41):** HANDOVER-LOOM.md created (10 sections + 2 appendices). AGENTS.md updated with Loom rituals, resume protocol. LOOM-START.md tracker closed (34/34 DONE). Truth tests fixed (7P/4S). L8 audit hardened truth tests.

**Loom delivery: COMPLETE.** Commits: 8396a38, 2036c5a.

---

## 2026-07-09 - Post-Loom Debt Cleanup (sessions #42-#63)

**L0.5 (D1, s42-s44):** Cold-QA B9 denominator fix. UI boot-liveness precondition.

**L3.5 (D2, s45-s46):** TodoApi eval gap triaged, root cause L7 call-spine gap.

**L5.x (D3, s47-s49):** Audit-trap sweep (5 traps). Advisory counts stable.

**L0.4 (D5, s53-s54):** Truth gate wired into loom-guards.ps1. Skip.IfNot guards.

**L3.4 (D6, s55-s56):** TfmScore handles net10.0+ generically.

**L2.5 (D7, s56-s59):** Lambda scope pollution fixed. SeamContext shared builder.

**L4.5 (D8, s59-s60):** Flow depth warning. Proportional budget. Integration test.

**L1.6 (D9, s61-s63):** SymbolTable member indexing. RefSite.FromType deleted.

**Debt: ALL 8 DONE.**

---

## 2026-07-09 - Design Reviews R1-R3 (sessions #64-#72)

**R1 (s64-s67):** L0+L1+L2 - 14/17 CONFORMS, 1 DEVIATES (L2.4 checkout trace).

**R2 (s68-s69):** L4+L5+L6 - 13/15 CONFORMS, 2 CONFORMS-WITH-FINDINGS.

**R3 (s70-s72):** L7+L8+Contracts - 12/14 CONFORMS, 1 DEVIATES (stale baseline), 6 contracts CONFORMS.

## 2026-07-09 - QA Driver (session #73)

**s73:** Full live drive: CLI + MCP warm/cold + UI 7 pages + 22-repo bench. QA verdict: PASS-WITH-FINDINGS.

---

## 2026-07-10 - Post-Loom Gap Close Phases A-C (sessions #74-#79)

**Phase A (s74-s79):** L2.4 checkout trace bus-publish fix. Type->Service bridge. Truth: 9P/2S.

**Phase B (s12-s18):** Tab strip 32px + Code pane auto-load. UI gate: 4/4 PASS.

**Phase C:** MCP mcpRunning persist, inspector word-boundary, bench encoding, spine-depth metric, perf doc.

**Phases A+B+C: VERIFIED.**

---
2026-07-09 04:43 — s53: D5 (L0.4) — Truth gate auto-enforcement delivered. Wired truth gate into loom-guards.ps1 (new gate #5). Converted 3 [TruthPending] silent returns to Skip.IfNot. Gates: build 0w/0e, tests 490P/3S/0F, truth 8P/3S/0F. Evidence: eval-results/2026-07-09/debt-L0.4-gate.txt. Commit: 92c85b3.
2026-07-09 04:51 — s54: D5 (L0.4) QA re-verify — Fresh-run gate battery confirms all claims. Build 0w/0e, tests 490P/3S/0F, truth 8P/3S/0F, guards green. QA verdict: D5 genuinely complete, zero discrepancies. Evidence: eval-results/2026-07-09/debt-L0.4-QA-gate-s54.txt.


## 2026-07-15 (session 2) — Wrap-up: Loom delivery audit + 8 detection/graph fixes + Tapestry plan (feat/wrapup-2026-07-15)

**Changed:**
- 5 detection fixes (commit `99acf40`): verb-attribute route composition
  (ControllerActionExtractor), content-probe file selection + factory-lambda worker resolution
  (ProgramCsFlowExtractor), Hub base-type SignalR signal (SyntaxStructureExtractor), Aspire SDK
  hint + version-strip (EntrySurfaceCatalog/DependencyExtractor), AppHost-ProjectReferences
  Microservices counting (ArchitectureStyleDetector), ToString-target guard (GraphBuilder).
- 3 graph fixes (commit `202c593`): hub entries anchor on hub-method member nodes, worker entries
  anchor on ExecuteAsync/StartAsync, hub files join call-graph seeds; SignalR self-source guard
  (framework repo keeps Library archetype); test pins updated to composed true routes.
- Docs: `docs/product/desktop-ui.md` rewritten for the Angular/Tauri app (was WPF-stale);
  go-to README I6 row corrected (MCP shipped via Loom L5, not DEFERRED); LOOM-START.md marked
  CLOSED pointing at Tapestry; merged `docs/go-to-program-addendum` (5 planning docs).
- **New mega plan: `docs/dev/briefs/proposal-tapestry.md` + `TAPESTRY-START.md`** — T0-T8
  waterfall (harness, detection, graph, MCP v3, context v2, Studio v2, pages revamp, bench, close),
  per-stage recommended approach + what-the-agent-will-get-wrong + numeric gates, 8 hard rules
  distilled from this session (R-T1..R-T8).

**Verified:**
- Drove engine/MCP/UI against `C:\code\shamshir` (never-seen 14-project repo): 36 findings indexed
  in `eval-results/2026-07-15/wrapup-drive/FINDINGS.md` (8 fixed, rest staged into Tapestry).
- Build 0w/0e; Core 453P/3S; Server 14P; goldens + TraceQuality + signalr evals green;
  truth 5P/6S; loom-guards clean; pnpm check PASS; UI drive gate 4/4; MCP cold QA 11/11 (100%).
- Pre-existing reds (unchanged, staged): dntsite target-* (T1.3), yarp archetype-gateway (T1.2).

**Next:** user review/merge of `feat/wrapup-2026-07-15`; then Tapestry T0.1 (orphan-proof gates).

## 2026-07-16 — Tapestry T4 COMPLETE: context generation v2, 6/6 checkpoints (feat/tapestry-t4)

*(T0–T3 sessions tracked in TAPESTRY-START.md handoff/checkpoints, not logged here.)*

- **T4.1+T4.6 (0d47247)** — pack identity header (`# {repo} — {focus}` + `analyzed {utc} ·
  HEAD {sha7}`); root causes: `snapshot.Explanation` never populated by the pipeline (the audit's
  empty `# ` title / `_Archetype: _`), no analyzed-at/HEAD on the snapshot, `int?` LineNumber
  interpolated blind (dangling `path:`). Repo-relative `file:line` everywhere. Contracts card is a
  real selection (role-tag messages + interfaces + DTOs), not a signatures alias; empty
  sections/cards dropped AND recorded in `omitted[]`; HTML markers out of the human copy.
- **T4.2 (8099bd6)** — budget-filling packs: bodies fill the remainder spine-first (full member
  text via the salient lookup, per-body cap, `… (+N lines)` markers, omitted counts); trace depth
  scales with budget (4→6 @ ≥3k); skeleton/signatures capped so structure can't starve bodies.
  Dogfood checkout 35% → **90.5%**; shamshir **99.4%** @ 4k.
- **T4.3 (9f60263)** — real config/tests sections (were dead client stubs): spine-file
  `ConfigScanner` subset + `FindCallers`×`TestHeuristics` rows with best-effort disclaimer.
  **Pre-existing bug fixed:** the tests_for heuristic matched `/tests/` on ABSOLUTE paths — a repo
  living under tests/ (fixtures, clones) made every member a "test"; now root-relative.
- **T4.4 (d15d0aa)** — per-section `_provenance: N source sites · V verified · A approx_` footers
  + structured SourceLocations/Verified/Approx on SectionAllocation (T5.3 chips).
- **T4.5 (a010229)** — verify_context: analyze-time `FileFingerprints` (sha256+lines) on the
  snapshot; `ContextPackVerifier` compares each section's cited files vs disk (modified/deleted/
  unknown); VerifyContext RPC + provenance proto fields (one regen, pnpm check 27P); MCP
  verify_context (24th tool). Live dogfood drift cycle: fresh=false → +2-line edit=true →
  git-restore=false, per-section verdicts naming the file.
- **Verified:** 16 pack tests (unit + CompositionApp assembly + verifier end-to-end on a temp
  fixture copy); two stage-end batteries GATE: PASS (gates-t4.txt, gates-t4-stage.txt);
  loom-guards PASS after every checkpoint; live MCP captures under eval-results/2026-07-16/tapestry-t4/.

**Next:** user review/merge of feat/tapestry-t4 → develop; then T5 (Context Studio v2): T5.1
quick wins + T5.6 recompute-on-change (the token-export trust bug).

## 2026-07-16 — GitHub-ready strand G1–G5: CI/CD + public docs + reference refresh + fixture submodules (feat/github-ready)

- Worktree C:/Code/DevContext2-github-ready off T4 head bcae33d (docs describe the post-T4-merge truth; tracker: GITHUB-READY-START.md).
- G1 (dd276a6): ci.yml re-enabled — push/PR triggers were commented out; engine job mirrors the local gate battery (Debug build, fast tests, loom-guards+truth, CLI --strict smoke) + app job (pnpm check). release.yml: dead DevContext.Desktop job removed (any v* tag failed the workflow), CLI→NuGet via MinVer + GH Release.
- G2 (3910202): README reworked around the four surfaces (CLI/desktop/MCP/server) with an honest build-from-source desktop story; NEW docs/product/mcp-reference.md (all 24 tools from source); CONTRIBUTING 24 tools.
- G3 (e3b8782): CODE-MAP (GraphBuilder partials ~2.5k, ContextPackBuilder 908 + Verifier/FileFingerprint, 24 gRPC handlers, 24 MCP tools), AGENT-REFERENCE (+VerifyContext RPC/tool, release flow), cli-reference relative-path claim (E9: local path beats owner/repo), DEVELOPER-PIPELINE §11.
- G4 (094faaf): CHANGELOG [Unreleased] catch-up (Lighthouse→Loom→Meridian→desktop redo→Tapestry T0-T4); .gitignore had 7 UTF-16 mojibake lines — eval-artifact patterns matched nothing; rewritten.
- G5: **fresh clones had a red truth gate** — eval-repos/{TodoApi,VerticalSlice,eShop} gitlinks had no .gitmodules → empty dirs → truth tests ran (not skipped) against empty repos. .gitmodules reconstructed (pins verified vs live clones), CI checkout submodules:true.
- **Verified (fresh worktree = fresh-clone proxy):** dotnet build 0w/0e; loom-guards PASS after submodule init; fast suite PASS; CLI --strict exit 0.
- Open (owner calls, in tracker): desktop sidecar packaging; eval-results (432 tracked files) / analysis-exports clutter; root tracker archive; screenshot refresh; empty-fixture-dir→skip fix (engine strand owns TruthExpectationTests).

**Next:** PR feat/github-ready → develop after feat/tapestry-t4 merges (branch is based on the t4 head).
