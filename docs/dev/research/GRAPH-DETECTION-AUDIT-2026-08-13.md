# Graph & Detection — deep assessment audit

_2026-08-13 · companion to `DEEP-EVAL-2026-08-13.md` (which covers the MCP/agent surface and the probe). This doc audits the two subsystems underneath: the **graph** (identity, edges, trace/map/query) and **detection** (signals, entry surfaces, archetype). Written as a self-contained section for the owner's next master plan. Sources: `R1-GRAPH-TRUTH.md`, `R2-ENGINE-BATCH-FIXES.md`, `BUG-BACKLOG.md`, `DECISIONS.md`, `PLAN.md` §2–§3, `HANDOVER-{LOOM,TAPESTRY,PRISM}.md`, `universal-coverage/HANDOVER-V2.md`, `eval-results/2026-07-28/graph-truth/MATRIX.md`, `PRODUCT-DIRECTION.md`, plus a full code sweep made today. Code claims marked **[audit 2026-08-13]** are from today's sweep — accurate as read, but re-verify file:line before fixing (house rule). Three of the biggest were re-verified by hand today and are marked VERIFIED._

---

## Verdict in three sentences

The graph and detection layers are the strongest they have ever been — structural identity, honest resolution tiers, a 47-pole truth matrix, catalog-driven entry surfaces, provenance-gated archetypes — and they are also **exactly where the product's remaining credibility debt lives**: the probe's impact-question failure (class C, 0/3) is the direct price of four filed edge-completeness bugs (#7/#8/#11/#12), and 80% of the engine's call edges on its own repo are still `approx`. Detection's breadth story is genuinely good on the app poles it was built against, but today's sweep found a second ring of silent coverage holes (Orleans consumers unreachable, scheduled-job kind unproduced, hosted services detected only through one registration spelling, Avalonia undetectable) that the matrix never measured because no expectation was ever declared for them. The strategic read: **the graph needs depth (edge completeness + one honesty vocabulary) before any re-probe, and detection needs a declared-coverage audit (catalog vs. reality) before the "universal" claim widens further** — both are cheap relative to what has already been built, because the instruments (matrix, contract sweep, fixture protocol) already exist.

---

# Part I — The graph

## 1. What exists (architecture as-built)

Two layers plus consumers. Full paths so the next session can cold-start.

**Layer 1 — `src/DevContext.Core/Graph2/` — the identity/facts substrate ("graph-v2").** Not a second graph; the resolution spine that feeds the one CodeGraph.

- `SymbolCanon.cs` — the one name algebra. Type ids keep namespace + nested chain + generic arity (`Ns.Outer.Inner`2`); member keys are `Type::Member` via `MemberKey`. One sanctioned lossy step: declared method arity `(N)` is dropped when a BodyFacts SymbolId becomes a graph member key (overloads collapse — deliberate, documented).
- `SymbolTable.cs` — tiered resolution `Declared → ProjectScoped → GlobalUnique → Ambiguous/Unresolved`; ambiguity is reported, never first-matched (**Law R1: no silent winners**). Types and members live in separate indexes (mixing them once made every constructed class "ambiguous"). `HopThroughProperty` for receiver chains.
- `BodyFacts.cs` / `BodyFactExtractor.cs` — one syntax walk per file → typed ops (`InvocationOp`, `CreationOp`, `LocalDeclOp`, `IdentifierUseOp`). Lambda bodies fold into the enclosing member; `await`/`Task<>` unwrapped; `this.` normalization (D-3); Razor `#line`-mapped provenance.
- `SemanticLitePopulator.cs` — Tier B: ONE compilation over all trees + NuGet refs from `project.assets.json`; upgrades receiver/decl/generic-arg types to `Semantic` (**Law R2: upgrade only, never re-point**). Arg-binding demand-scoped to detector-consumed verbs; bind parallel by tree.
- `CallGraphBinder.cs` — BodyFacts → `CallEdge`s. Ambiguous → skip; receiver-chain hop; interface dispatch → DI impl, else sole implementor, else **the interface itself** (conflict ≠ drop); bare-identifier self-calls gated on "the type declares the method" (retired the deny-list). Entry-seeded closure (`MaxClosureRounds=16`) + seedless fallback capped at **100 files**.
- `Seams/` — pure detectors (MediatR dispatch, bus publish, event creation/raise, entity touch, plain call) + `DispatchClassifier` off `DispatchSeamCatalog`.

**Layer 2 — `src/DevContext.Core/Graph/` — model, assembler, queries.**

- `CodeGraph.cs` — `NodeKind {Type, Member, EntryPoint, Service, Message, Store}` + `RoleTags`; `EdgeKind {Calls, Sends, Handles, Raises, Consumes, ReadsWrites, Resolves, WrappedBy, EntityRelation, ServiceLink, Exposes, DependsOn}` with `ServiceLinkTags` (bus/grpc/http-direct/http-via-gateway/refit/aspire); `Resolution {Join | Syntactic | Semantic}`. Edges carry Provenance (file:line), Confidence, `MultiImplCount`, `RegistrationSites`, `TargetMember`. **Edge dedup key is `(From, To, Kind)` — first write wins** (exploited deliberately for ServiceLink tag precedence; also the reason call-site multiplicity is lost — DC1 residue). Frozen dictionaries for lookup, builder-insertion order for enumeration, lazy inverse adjacency.
- `GraphBuilder.cs` (+ `.Nodes/.Entries/.Seams/.ServiceLinks/.Flows` partials) — the assembler; `Build` is the whole pipeline in ~60 lines.
- `GraphQuery.cs` — node/neighbors/usages/find/impact/seam/trace; the C3 type roll-up (a Type's neighbors include its members' cross-type edges).
- `TraceBuilder.cs` + `TracePolicy.cs` — the **single** seam-priority/fan-out/depth/budget contract (Batch E; depth 6, fan-out 12, elastic to 12, budget 4000 with render reserve). Sends > Handles > ServiceLink > Raises > … > Calls.
- `MapBuilder.cs` / `MapRenderer.cs` — Map renders **only ServiceLink** edges; Calls never reach the Map surface (the old "Map edge explosion" was analysis cost, and T7.2's verdict on OrchardCore 1,708→11,904 edges was *coverage, not fabrication*).
- Persistence: graph recomputed per run, persisted whole (`SnapshotPersistence.cs`, schema v7, engine-version stamped); `EngineHostCache` caches the DI host per repo root (the "unresponsive after ~36 analyses" fix); no incremental graph.

**Determinism is a construction property** (Prism D5.3): extractor outputs sealed post-extraction, **call-edge canonical order = CALL SITE (file, then numeric line)** because source order is semantic for the primary-call pick; `DeterministicOrderTests` pins it; fresh A/B byte-identical on dogfood + bitwarden. Standing prohibition on `ConcurrentBag`/frozen-order enumeration in anchor picks. This survives all later surgery — treat as non-negotiable.

## 2. What the program proved (done, with numbers)

The arc, compressed — each step has its close evidence in `PLAN.md` §2:

| When | What | Measured result |
|---|---|---|
| Loom→Tapestry | BodyFacts + SymbolTable replace body-scan regexes; ONE event join (`EventWiringProjection`) | truthful kernel baseline |
| Prism D2–D5 | Blazor `@code` virtualization; determinism 3-leg kill; **merged-compilation cost FALSIFIED** (55ms, not 81s — the real lever was serial demand-set bind) | DntSite Map 125.5→34.5s |
| Batch A (S2) | Identity surgery: `SymbolCanon` structural ids; **`CallGraphExtractor` deleted**; Ambiguous→skip; deny-lists retired (data-access noise list kept — probed, KEEP) | DntSite 34.5→24.9s, OrchardCore 30.8→14.1s; eShop `IdentifiedCommand` join fixed |
| Batch B (S3) | Transports: `AddGrpcClient<T>`/typed `AddHttpClient<T>`/Refit-by-address; Aspire topology (direction was read **backwards**); CLI verbs | eShop cross-service **5 (bus only) → 23 links**; bus pinned at exactly 9; single-project false-positive guards clean |
| Batch C (S4) | Entry-target quality: receiver-chain hop shared by both producers; DI conflict → interface; `SolutionCatalog` + `--sln` (DC6) | GitVersion new-cli 5 verbs when scoped |
| Batch D (S5) | Perf riders only — acceptance was "nothing moves" | **0 moved cells across 44 pole comparisons** |
| Batch E (S5) | One `TracePolicy`; trace built once; CrossService render collapse (33 rows → 1) | seam-order divergence between map/trace killed |
| G3 | `seam(from,to)` + kind-filtered neighbors + context packs | 3/9 seam tests fail if run on direct edges — proof the wiring now hangs off members |
| G8 | Scale wall: `SyntaxStructureExtractor.ResolveTypeDeclaration` walked the whole tree per base-list entry | HotChocolate **1,275s → 64.3s**; class = "large AND base-list dense"; 15 poles byte-identical |
| G10 | Threshold recalibration vs the pre-Batch-A starved graph | 2 of 5 thresholds had silently inverted |

The instrument that made this safe: **`eval/graph-truth.ps1` — the 47-pole matrix** with 7 checks (transport counts vs hand-written expectation, handler-join reachability, hub sanity, entry-target sanity, style, sln scope, dup-name) — plus `eval/contract-sweep.ps1` (dead-field gate) and the perf bench with its pinned baselines. Batches were judged by which cells flip and which must not move.

## 3. What is broken, missing, or dishonest — ranked

### 3.1 The edge-completeness family (the release blocker; = DEEP-EVAL W2)

These four are why the probe's arm M went 12/18 with impact 0/3. All filed in `BUG-BACKLOG.md` with "watch it go red first" fixtures.

- **#11 (high)** — static calls with a TYPE-NAME receiver produce **no edge**. 3-for-3 on the engine's own repo: `BodyFactExtractor`, `RazorCodeVirtualizer`, `ExtractorHelpers` all have 0 in-edges. **80% of the engine's own Calls edges are `approx` (1103/1383).** "Who calls this helper" is answered "nobody" with a confident shape. Measure the receiver-resolution arm for a receiver that is a TYPE, not a chain rooted at a local/parameter/field.
- **#12 (high)** — the Semantic receiver upgrade misses **every invocation whose statement fits on one line**: `TryBindReceiverType` relocates by LINE SPAN then searches ANCESTORS, so the invocation is a descendant and is never found (15/16 GitVersion sites). Everything keyed on receiver type degrades (dispatch detection, DI iface→impl, `HopThroughProperty`, the Resolution tag). Sister sites: `TryBindLocalDeclType`, `TryBindGenericArg`, the `Args[0]` bind. **Blast-radius: moves call-edge counts on every pole — batch-with-a-matrix-run, never surgical.**
- **#8 (high)** — calls inside lambda arguments drop edges (Hangfire's storage write invisible; the trace looks complete). **Its stated mechanism is REFUTED by #11** (lambda calls demonstrably bind on the engine's own repo) — re-measure before fixing.
- **#7 (high)** — an explicit interface method emitted as a **Type node** (`Type:…::Type(1)`, empty filePath) absorbing 26 BCL `System.Type` references = 4.2% of Hangfire's graph; ranks 5th in `stats` wiring hubs. Fires for any explicit interface impl colliding with a BCL type name (Type, Task, Action, Path…). Free invariant: *no node may carry kind Type and a `::name(arity)` member id*.

### 3.2 Structural absences (limitations, not bugs — but they decide questions)

- **No inheritance/implements edge kind.** `SeamKind` has none; G4.1's dogfood lost 2 of 10 questions to exactly this, and a library's call graph "fragments at every interface" (19 `Resolves` for 51 interfaces on Hangfire). Biggest single structural gap for the Library archetype, `tests_for`, and impact.
- **Property accessors, expression-bodied properties, indexers, operators, event accessors produce NO BodyFacts ops** — only method + constructor declarations are walked (`BodyFactExtractor.DescribeMember`). Calls made inside a getter are invisible. **[audit 2026-08-13]** — not filed anywhere before this audit; needs a discriminating fixture before sizing.
- **Overloads collapse** (member keys carry no arity — documented Batch A deviation) and **call-site multiplicity collapses** at the `(From,To,Kind)` dedup (DC1 residue): only the first provenance survives between any node pair.
- **`Channel<T>` in-proc seam — planned in R2 §2.B, deliberately never built** (producer/consumer tracking through BodyFacts is trace-seam work).
- **Seedless fallback cap**: a repo with no detected entries and >100 body files gets **zero call edges, silently** (`CallGraphBinder.SeedlessBindFileCap`). Interacts badly with any detection miss (see Part II) — a coverage hole upstream mutes the whole graph downstream.

### 3.3 One-vocabulary / honesty debts (cheap, high trust-per-dollar)

- **#25** — TWO definitions of a verified edge: `GraphStats`/`SeamStat` count approx = Syntactic only (so `Join` — the enum default — counts as verified) while `GraphOrphansSource` counts Semantic only. No number either produces is comparable until this is one definition.
- **#17** — two Member-title vocabularies (343 owner-qualified vs 627 bare across 6 poles). Wants one member-title helper next to `SymbolCanon`, used by every producer.
- **#18** — Type nodes minted from lambda/expression TEXT (a 20-line lambda body as a node title reaches the UI); the `GraphBuilder.Seams` filter is incomplete and not applied on every path.
- **#22/#23/#24** — three graph metrics that have **never fired or never discriminated**: `graph.orphans` (Semantic-share floor 0.5 unreachable on 11/11 poles — and it's the one insight that gets live code deleted when wrong), L3.4 hub-scope broadening (`sparseGraph=false`+`hubScopeNodes=0` everywhere including its own trigger population), deep-spine ratio (saturated ≈1.0, separates nothing).
  **[CLOSED 2026-08-14, R1.1 — and this bullet's own premise did not survive the re-measurement.]**
  Post-E1, `graph.orphans` *inverted*: the floor became reachable and it fired 0/10 true, so it is
  **retired**. The hub-scope broadening **does** fire (Hangfire: `sparseGraph=true`,
  `hubScopeNodes=34`) — the 11-pole set never held a repo that clears its gates — so it is **held**
  with the measurement at the call site. Deep-spine is still saturated on 12 poles and is
  **retired**. `eval-results/2026-08-14/r1-metrics/R1.1-EVIDENCE.md`.
- **#13/#16** — snapshot freshness: `analyze --no-cache` doesn't invalidate what a later `query` reads (stale graph served with `snapshotCache: HIT` — a way to produce false evidence with correct commands); plus the ~1-in-50 rehydrate flake (dirty-fingerprint theory tested and REFUTED; next suspect is the swallowed `SaveAsync` failure).

### 3.4 Standing latents (named, carried)

eShop deep-trace render stops at the `IntegrationEventLogEF` send seam (render-depth, pre-D3); wolverine `Envelope` = accepted limitation (pinned at 1); the gRPC pole's examples sit outside the analysed solution (open matrix cell); RazorPages cross-sample fabrication + ControllerApp sibling-action precision (`[TruthPending]` since Loom); the +6/+15 CLI↔MCP node/edge divergence has **no confirmed cause** (the devcontext.json theory was tested: mechanism real, moves nothing); `TraceNode` carries no transport kind so the collapsed CrossService row counts seams but cannot name them (A-2 follow-up); framework-boundary stop is a literal name list in `TracePolicy.IsFrameworkLeaf`.

### 3.5 Performance state (healthy; know the shapes)

The wall is **SemanticLite's bind**, not graph join (bind flat at ~5s while OrchardCore edges grew 7×). Known classes: "large AND base-list dense" (G8 — fixed by per-file declaration index; the budget was NOT raised); per-file whole-tree walks invisible on fixtures and catastrophic at framework scale (Batch B's alias-lookup regression — the matrix's big poles catch what unit tests cannot); MVID-keyed snapshots mean every Core edit cold-starts everything (this is also bug #1's false-red mechanism in the MCP QA harness). The once-named "persist/reuse merged compilation" lever **must carry its falsification caveat** (D3 measured 55ms) — the version of it that could still pay is persisting the *SemanticLite bind results/BodyFacts semantic upgrades*, not the compilation object; re-measure before believing either.

## 4. What we can do (the option space, priced)

1. **Close the edge family** (#11 → #12 → re-measure #8 → #7). Engine work, batch discipline, full matrix rerun (#12 moves every pole by design). Acceptance already written in DEEP-EVAL W2: the probe's class-C question resolves the true impact set on eShop; the engine's own `approx` share drops measurably from 80%. The **dogfood invariant** ("DevContext's own helper layer must have in-edges in DevContext's own graph") is cheap and would have been red for months.
2. **Fix-shape upgrade while in there**: record the invocation's own `TextSpan` on `BodyOp` (BUG #12's named fix shape) — kills the whole relocate-by-line bug class (#12 + sister sites) instead of patching one arm.
3. **One honesty vocabulary**: single verified-edge definition (#25), one member-title helper (#17), the Type-kind-with-member-id invariant (#7 rider), lambda-text node filter unified (#18). Each is small; together they make every number on every surface mean one thing.
4. **Add the missing edge classes** (bigger, optional per-class): inheritance/implements seam kind (top value for libraries + impact); property-accessor walking **[audit 2026-08-13]**; call-site multiplicity (per-edge site list rather than first-wins) if impact/usages should count, not just name.
5. **Recalibrate or retire the dead metrics** (#22/#23/#24) — after #11/#12 land, because those fixes change the Semantic share that two of the three thresholds read. Sequencing matters: G10's lesson is that thresholds calibrated on a starved graph silently invert.
6. **Snapshot freshness truth** (#13/#16): decide what `--no-cache` means, surface `SaveAsync` failure, and let `query` say what it read.
7. **Trace/graph niceties**: transport kind on `TraceNode`; incremental analysis (unpriced, large); `Channel<T>` seam (deferred, still priced as trace-seam work).

## 5. What we should do (recommendation)

**Order: 3 → 1(+2) → 5 → 6; 4's inheritance kind rides with 1 if the re-probe needs libraries, else after.** Rationale: the honesty vocabulary (3) is days of work, zero blast radius, and every later measurement reads through it — do it first so the edge-family acceptance numbers are trustworthy. The edge family (1, with 2's TextSpan upgrade) is the actual release blocker per DEEP-EVAL gate 4 and must run as a batch with the matrix. Metrics recalibration (5) only makes sense after the graph it measures stops moving. Everything in 3.4 stays filed unless a probe question hits it.

---

# Part II — Detection

## 6. What exists (architecture as-built)

Four layers; the **Entry Surface Catalog** is the declarative spine (`src/DevContext.Core/Graph/EntrySurfaces/EntrySurfaceCatalog.cs`, 38 descriptors). Contract in its doc comment: *adding a shape = one descriptor + one extractor + one builder + one detection record* — zero edits to GraphBuilder/DiscoveryPipeline/ArchetypeDetector/MapRenderer.

1. **Signals** (Stage 2, sealed after): `ArchitectureSignals` keeps the highest-confidence signal per key. Producers: `DependencyExtractor` (packages at 1.0, SDK/ProjectReference at 0.9, project-name boundary match at 0.7 — all catalog projections) + `SyntaxStructureExtractor` code fallbacks (controllers/razor-pages/signalr at 0.9).
2. **Detections** (Stage 3): 26 extractors, all gated on a Stage-2 signal (`ShouldRun`), emitting 24 typed `Detection` records (ADR-005); `IEntrySurfaceDetection` marks entry surfaces — and its consumer is the **call-graph seed**, so a detection miss also silences the binder.
3. **Entry points**: 13 `EntryPointKind`s, 11 builders (`Graph/EntryPoints/`), post-assembly target/score enrichment (`GraphBuilder.Entries.cs`: reach/seam/entity/cross-project weighted score from a depth-6 BFS).
4. **Classification & render**: `ArchetypeDetector` (App/Library/Gateway/Desktop/Worker/Blazor/CliTool) with the provenance ladder; `ArchitectureStyleDetector` (7 styles); `ProjectClassifier` (+`SamplesAreTheProduct`); Map sections + `LibrarySurfaceRenderer` for libraries; archetype-scoped insight sources.

**Provenance gating is real and layered**: package (1.0) beats syntax (0.9) beats project-name (0.7); self-source suppression (only non-runnable projects self-source, boundary-matched names, repo-relative sample-path suppression); `IsSelfSourcedFrameworkSignal` (DetectedVia ∈ {ProjectName, ProjectReference} ⇒ the repo IS the framework ⇒ Library); G9.1's symmetric-declaration-evidence rule (`PackAsTool` loses to `IsPackable`). Per-detection `Confidence` floats exist (gRPC 0.85, Functions 0.95…) but **nothing downstream reads them** [audit 2026-08-13].

**The detect≠render gap is closed on the main path** — Map labels/groups derive from the catalog, MCP `entrypoints` filters by kind, every entry is a trace root, `entry-kind-present` in the eval expectations asserts detection AND render together. The two S9/S10 instruments (dead-field contract sweep; the case-sensitivity find `"Warning"/"warning"/"WARNING"`) exist because the gap kept reopening in new clothes — treat it as a *class*, permanently gated, not an era that ended.

## 7. What the program proved

- **Breadth on the app poles**: entry kinds 11→13, extractors 16→23, catalog 0→38 (universal-coverage phase); gRPC/SignalR/Functions/GraphQL/CLI/UI all detect AND render with negative tests (ViewModelBase is not a gRPC base; EventHub is not a hub; the Command-Palette SDK is not System.CommandLine).
- **Archetype honesty at the poles**: framework repos read Library via self-source; consumer apps stay App; samples-only repos read honestly (`SamplesAreTheProduct`); auxiliary executables stopped deciding archetypes (G9.1) — 18-fact `ArchetypeDetectorTests` pins the ladder.
- **Matrix verdicts** (2026-07-28 grid): style and hub-sanity green almost everywhere; the historical FAIL columns were sln-scope (10 repos — Batch C's `SolutionCatalog` addressed the mechanism) and the entry-surface breadth rows below.
- **Honest scope machinery**: `outside_scope_apps` names (but does not draw) runnable projects outside the analysed sln; zero-entry repos render `PUBLIC SURFACE` instead of a dead map; the Confidence Ledger is counts-and-ratios only (the blended number was deleted for lying twice on one chip).

## 8. Gaps — ranked, with the NEW findings from today's sweep

### 8.1 Declared-but-unreachable surfaces **[audit 2026-08-13 — the new ring]**

These are worse than missing features: the catalog *claims* the shape, so nothing looks missing.

- **Orleans consumer apps are unreachable — VERIFIED today.** The descriptor ships `Packages: []` (`EntrySurfaceCatalog.cs:220`) and `PackageSignalMap` is built only from `Packages`, so an app referencing `Microsoft.Orleans.Server/.Client/.Sdk` never fires the signal, `OrleansGrainExtractor` never runs, and `GrainMethod` entries never exist. The only live path is Orleans' own repo — which self-sources to Library. No consumer-app Orleans expectation exists anywhere in `eval/expectations/`.
- **`ScheduledJob` is near-dead — VERIFIED today.** `WorkerEntryPointBuilder` emits it only for `BackgroundWorkerKind.TimedJob` or DNTScheduler — and **no producer ever sets `TimedJob`** (grep: the enum declaration and the builder's check are the only two references in Core).
- **Hosted services detect only via `AddHostedService<T>` in scanned Program/Startup flow.** A `BackgroundService` subclass registered by Scrutor scanning, a library's own `AddXxx()` extension, or the Worker SDK produces no entry; there is no base-type detector (the syntax extractor only role-tags).
- **Avalonia is documented in `DesktopEntryExtractor` and undetectable**: no Avalonia package/SDK in the catalog, and an Avalonia app fires neither `desktop-ui` (needs WinExe/WindowsDesktop/WinAppSDK) nor `maui`. WinForms is half-wired (an `Exe`-declared WinForms app gets nothing).
- **`PublicApi` kind has no builder — VERIFIED today** (synthesized only by `EntryPointResolver`/`TraceBuilder` for focus/trace bridging, which is legitimate); `ArchetypeProjection.CollectPublicApiEntries` is dead code because Library routes to `LibrarySurfaceRenderer`. **`Archetype.Gateway` has no archetype view at all** (falls to Empty — a Gateway repo gets the generic map + Routes). The `wpf-mvvm` signal key has no descriptor and no register site.

### 8.2 Frameworks with no detector at all **[audit 2026-08-13]**

Hangfire and Quartz job surfaces (zero hits for `IJob`/`RecurringJob`/`BackgroundJob` in Core — both repos sit in the eval set as Library poles, so the *consumer* story was never measured); Kafka (`Confluent.Kafka` known only to the style detector's package list), Rebus, CAP, Coravel, Akka.NET, Silverback, Marten, Elsa; **code-first gRPC** (`protobuf-net.Grpc` `[Service]`/`[Operation]`) — the extractor hard-requires the protoc `X.XBase` nesting. Per `PRODUCT-DIRECTION.md` §4, rung 4 of the entry ladder ("hosted services / background workers / scheduled jobs — detected; surface as entries") is **at best half true** today.

### 8.3 Filed and open (backlog + decisions)

- **#14** — generic `[Command<T>("verb")]` invisible: GitVersion shows 5 verbs where 9 ship, and the `cli.command-tree` insight asserts the wrong tree at 0.8 confidence. The type argument is the parent link a real 2-level tree needs.
- **#20 (+#19)** — two judgements about the same projects disagree: `ArchetypeDetector` excludes auxiliary/sample executables, `ServiceBoundaryInference.RunnableProjects` does not (AutoMapper Atlas: "1 services", names `TestApp`; a fourth service count on one page).
- **#2** — entry names don't round-trip (`entrypoints` renders "GET /todos", `get_context`/`trace` need "`<lambda>` GET /todos/"). Detection naming meets the agent surface; single-source the addressable name.
- **Blazor renders as `HTTP`** — component pages are indistinguishable from REST routes in the entries table unless the whole archetype resolves to Blazor.
- **Legacy Markdown/HTML context renderers render only the 10 oldest detection types** — the 11 newer ones (gRPC, SignalR, Functions, Grain, GraphQL, CLI, Desktop, transport clients, Refit, auth policy) reach the user only via the Map's ENTRY POINTS [audit 2026-08-13]. Either give them parity or fold those renderers into the render-kernel decision (R3) and delete.
- **D-F insight dedup** (three overlapping auth findings on eShop) — engine-side so CLI/MCP benefit; still undecided (S11).
- Scope residue: `outside_scope_apps` named-not-drawn; the STACK line reads every discovered project (dotnet-podcasts prints `net7.0-android` from outside the sln) — "honest by accident, recorded rather than fixed."

## 9. What we can do, and what we should do

**Can do**, in rough cost order:

1. **A declared-coverage audit as an instrument, not a doc** (cheap, first): for every catalog descriptor, assert its signal is *reachable* (a consumer-app fixture fires it) and its Kind (if any) has a producing path — descriptor→signal→extractor→builder→entry. Today's Orleans/TimedJob/Avalonia finds are all instances of one class: **the catalog over-declares**. One test sweep pins the whole class, the same way `entry-kind-present` pinned detect≠render and the contract sweep pinned dead fields. This is the detection twin of the graph's dogfood invariant.
2. **Close the reachable-surface holes it finds**: Orleans packages on the descriptor; a `BackgroundService`/`IHostedService` base-type detector (which also revives `HostedService` breadth); a TimedJob producer or an honest deletion of the kind; Avalonia descriptor + WinForms `Exe` case. Each follows the catalog's own add-a-shape contract, each ships with a consumer-app fixture + expectation (the missing-expectation gap is *why* these survived the 47-pole matrix).
3. **Rung 4 properly**: Hangfire/Quartz job entries (attribute + interface shapes; both frameworks are already eval poles as libraries, so the fixture cost is one consumer app each).
4. **Fix the filed set**: #14 (strip generic type args in the leaf comparison; carry the type arg as the parent verb), #20/#19 (make `ServiceBoundaryInference` read the archetype's exclusions — one source of truth for "what is a service"), #2 (addressable entry names), Blazor `UiEntry`-vs-`HttpEndpoint` distinction or a kind tag.
5. **Read or delete per-detection `Confidence`** — a float nothing consumes is a future #25.
6. **Bigger, optional**: Kafka/Rebus/CAP consumers; code-first gRPC; the D-F dedup decision; renderer parity or deletion.

**Should do: 1 → 2 → 4 → 3; 5 rides along; 6 waits for demand.** The audit instrument comes first because it converts today's hand-found holes into a permanently red/green property, and because widening the catalog *without* it just grows the over-declared surface. Then close what it finds, then the filed bugs (#14/#20 are the kind of one-session fixes the backlog already scoped), then rung 4. Detection work is largely independent of the graph's W2 batch and can run in parallel sessions.

---

# Part III — Cross-cutting

## 10. Instruments and lessons that must survive into the next plan

- **The matrix answers "did it regress" wholesale** — every graph batch is judged by flip/hold/not-worsen cells declared *before* coding. Keep the declared-acceptance discipline; it caught the callee-name-order regression, the POST-target flip, and Batch B's perf regression.
- **"A driver check is vacuous until you have watched it go red."** Three occurrences in the program's own count. Every fix above has its discriminating fixture named in the backlog — build that first.
- **The signature defect class is a confident surface over absent substance** (DEEP-EVAL §3): in graph/detection clothing it is: edges that don't exist behind a complete-looking trace (#8/#11), a catalog that declares what can't fire (Orleans), a kind no producer makes (`TimedJob`/`ScheduledJob`), metrics that never discriminate (#22–#24), two definitions of one word (#25, #20). The cure is always the same shape: an invariant that would have been red the day it was written.
- **Thresholds are only as good as their calibration data** (G10): after #11/#12 move the Semantic share, recalibrate #22/#23 against the *new* graph, with `git blame` against the calibration commit as the discriminator.
- **Agent-premise-wrong / engine-right happened five times** — re-verify before fixing, including this audit's own [audit] marks.
- **Determinism seals + `SamplesAreTheProduct` + provenance gating are load-bearing** — any of the work above that touches resolver order, project classification, or signal registration must keep `DeterministicOrderTests`, `ArchetypeDetectorTests`, and the false-positive negative tests green.

## 11. How this folds into the big plan

The pre-release plan in `DEEP-EVAL-2026-08-13.md` §4 already carries the graph's critical path as **W2** (edge family, batch discipline, matrix rerun) gated behind **W1** (agent trust pack). This audit adds, without disturbing that ordering:

- **W2a (prepend to W2, days):** the one-vocabulary pack — #25 verified-edge definition, #17 member-title helper, #7's kind/id invariant, #18's node-text filter — so W2's acceptance numbers are measured in a single honest currency.
- **W2b (append to W2):** the TextSpan-on-op fix shape (kills the #12 class, not the instance) and the dogfood in-edge invariant as a standing gate.
- **W5 (new, parallel to W2, independent):** detection declared-coverage — the catalog-reachability instrument, then Orleans/hosted-service/TimedJob/Avalonia closures, then #14/#20/#2, then rung-4 job surfaces. None of it touches the binder, so it can run while W2's matrix batch is in flight.
- **W6 (after W2 lands):** metric recalibration (#22/#23/#24) against the post-W2 graph.
- **Explicitly deferred, restated:** inheritance seam kind (build when a probe question or the Library story demands it — likely at the unseen-repo re-probe), incremental analysis, Channel seam, Kafka-family detectors, renderer parity (blocked on the render-kernel decision), snapshot-freshness #13/#16 (fold into whichever session next touches the cache).

The release gates in DEEP-EVAL §4 stand unchanged; this audit's contribution to them is that **gate 4 (impact non-inferiority on an unseen repo) is not reachable without W2, and W2's own numbers are not trustworthy without W2a** — which is why the vocabulary pack goes first.
