# R2 — Engine fixes: the batch model

> Owner direction, verbatim intent: "we have been very slow — one fix at a time sometimes costs the
> whole session in back-and-forth, hope-on hope-off, test gate build. Audit deep what engine fixes
> are required and what's missing, then apply batch fixes and build/test them together." This doc is
> the batch plan. R1 feeds it the verified defect inventory; this doc owns sequencing + process.

## 1. The process change (why batching is safe here)

- One branch per batch; ALL fixes in the batch land before the first full build+test cycle.
- Inside a batch: code with `dotnet build src/DevContext.Cli -clp:ErrorsOnly` only (seconds), unit
  tests for the subsystem touched (`--filter`), NO full gate.
- Batch close: full `eval/gates.ps1` once + the R1 truth matrix once. Two runs total per batch.
- Acceptance is DECLARED UP FRONT per batch (which R1 matrix cells must flip, which must not move).
  That kills the per-fix "did it regress something" anxiety loop — the matrix answers it wholesale.
- Known session-killers, pre-empted: orphaned `DevContext.Server` locking DLLs
  (`start-dev-bg.ps1 -Kill` FIRST, always); stale CLI after Core edits (rebuild Cli project);
  PS 5.1 UTF-8 em-dashes in detached scripts (ASCII only); `Select-Object -First` corrupting CLI
  exit codes (capture to file).
- Snapshot cache: engine MVID keying means every batch invalidates all snapshots automatically —
  cold re-analyzes during verification are expected, don't chase them as regressions.

## 2. Batch plan (draft — resequence after R1 matrix lands)

### Batch A — Identity + resolution (the big surgery; fixes DC1+DC2 at the root)
1. Structural `NodeId`: generic arity + nested-type + method-arity aware keys. `SymbolId` already
   uses `Type::Method(N)` — converge on it instead of converting away
   [audit: `GraphBuilder.Seams.cs:554-566`].
2. Delete `CallGraphExtractor`'s private resolution universe: call edges produced from
   `BodyFacts`/`InvocationOp` resolved through `SymbolTable` (Ambiguous → skip, never silent-win).
   KEEP the entry-seeded closure scoping (the D3 perf win) [audit: `CallGraphExtractor.cs:219-277`].
3. One Roslyn compilation (SemanticLite's, with NuGet refs) — delete the second
   [audit: `CallGraphExtractor.cs:142-145` vs `SemanticLitePopulator.cs:200`].
4. Delete the compensating deny-lists that the honest resolver obsoletes (IsSelfCallNoise,
   _dataAccessNoiseMethods, Mediator-Contains overfit) — VERIFY each is truly redundant first.
5. Collapse `NameResolver` into `SymbolTable` (same index, different honesty)
   [audit: `Graph/NameResolver.cs` vs `Graph2/SymbolTable.cs`].
- Acceptance: eShop IdentifiedCommand Out>0 + UI trace reaches CreateOrderCommandHandler;
  dup-name CrossService cells flip; DeterministicOrderTests green; bench within budget
  (`benchmarks/` PERF baseline 2026-07-18-1346: DntSite Map 34.5s, OrchardCore 30.8s — no >10% regression).
- RISK: this is the deepest cut. Do it FIRST while everything else is quiet, not last.

### Batch B — Transports + joins (DC3 + DC8 joins)
1. gRPC/HTTP client link detection: cover `AddGrpcClient<T>`, typed `AddHttpClient<T>`, Refit
   without gateway requirement, Aspire service-discovery URIs; allow EXTERNAL targets
   (render as dashed external node) — drop the both-ends-in-solution gate
   [audit: `GraphBuilder.ServiceLinks.cs:26,73-90`].
2. Aspire resource graph → ServiceLinks/topology (AppHost = orchestrator grouping; stores as Store
   nodes — produce `RoleTags.DataStore`/`NodeKind.Store` instead of leaving them dead vocabulary).
3. CLI command-tree: detect GitVersion-class frameworks (probe from R1); command tree becomes the
   CliTool entry surface (fixes wiring-health 0% + 1-entry stub).
4. Channel<T> in-proc seam (planned, never built — `ENGINE-VALUE-AUDIT` §5.6.3) if cheap.
- Acceptance: eShop CROSS-SERVICE shows grpc+http links; atlas canvas has connected services;
  GitVersion entries = command tree; aspire-samples topology shows resources.

### Batch C — Entry/target quality + classification (DC4 + DC6 + DC7)
1. Primary-call pick: skip DI-injected service-interface receivers when a deeper semantic action
   exists; framework-leaf suppression applied to UiEntry targets (IAppEnvironmentService class).
2. Multi-sln scope: enumerate slns, analyze the picked one EXPLICITLY, emit scope note into
   Map/proto/UI ("analyzed X of Y solutions — pick another"), CLI `--sln` flag.
3. ProjectClassifier: catch fixture/test naming beyond the standard conventions (GitVersion class);
   add the R1 hub-sanity check to the eval expectations.
4. Style verdicts: CliTool/library → suppress at the DETECTOR (emit None/NotApplicable), not per-surface.
- Acceptance: RelayCommand targets meaningful; aspire-samples shows scope note; GitVersion hubs
  contain no fixtures; refit/Redis style cells flip to n/a.

### Batch D — Assembly hygiene + perf riders (DC10, low risk, mechanical)
Build graph once (not 3×); index the per-endpoint FirstOrDefault scans; memoize BfsEntryScore/
ComputeFlows; cache NoiseFilter.IsProductionEntrySource; fix gateway-scan separators (cross-OS bug)
and move it out of GraphAssembly; delete `IPruner`/dead vocabulary/`DevContext.Roslyn` dir/second
Detection serialization scheme; SDK evidence onto ProjectInfo (archetype detection pure).
- Acceptance: bench delta (expect GraphAssembly improvement on OrchardCore ~10.6s stage); full
  battery green; grep-proofs for deleted symbols.

### Batch E — One trace contract + number reconciliation (DC5 + DC9)
1. Single trace policy module: seam pick order, framework-leaf filter, budget-elastic depth
   (kill depth-1 default; kill the double-build in GetTrace) — served identically to CLI/proto/MCP.
2. Reconcile every number pair: verified% chip vs tooltip; public-type counts; studio totals;
   selected-count. One counting function per stat, tested.
3. Retire `RESULT`/`NEXT` eShop string tables; spend tokens on omitted-node identities instead.
- Acceptance: same focus → same steps on CLI/MCP/UI (modulo format); numbers match across surfaces.

### Explicitly deferred (needs R3 decisions first)
Render kernel / FacetCatalog consolidation (the six-render-paths problem) — it reshapes output
contracts, so decide the target feature set in R3 before building the kernel. Same for legacy
renderer deletion (~5k LOC dead surface incl. MarkdownRenderer path, scenarios/init/ghost flags,
`Render` RPC): safe any time, but bundle with the kernel work to avoid churning contracts twice.

## 3. What's missing (gaps, not bugs — candidates to fold into batches)
- `seam(from,to)` path query — no two-endpoint query exists anywhere in GraphQuery (goes with
  Batch E or R4's MCP fixes).
- Kind-filtered neighbors/find/entrypoints in proto (3 fields; unlocks "who writes this table").
- get_context/pack for non-entry roots (type-rooted packs — unblocks libraries; R3/R4 shared).
- Snapshot-cache truth in proto (from_cache/analyzed_at/git_head — home Freshness card currently lies).
- Server reads devcontext.json (CLI and server currently analyze DIFFERENT file sets when config
  exists — dogfood inconsistency).

## 4. Verification instruments (build once, in Batch A session, reuse every batch)
- R1 truth matrix (`eval/graph-truth.ps1`) — the batch acceptance instrument.
- The existing gate battery unchanged (full run only at batch close).
- Bench: `benchmarks/` macro run, compare to PERF-2026-07-18-1346.
