# R1 — Graph Truth: misdetection inventory + breadth verification protocol

> Goal of the next session on this strand: confirm the defect classes below are the *complete* set
> across every repo shape we claim to support, quantify each one's blast radius, and hand R2 a
> fix list that is exhaustive enough to batch. eShop proved the classes exist; this strand proves
> where else they bite.

## 1. Defect classes (from the 2026-07-27 audits)

### DC1 — Node identity is lossy (string-concatenated)
- Generic arity erased: `Id = ns + "." + name` [audit: `Extractors/Generic/SyntaxStructureExtractor.cs:179`].
  `Result<T>`/`Result` merge; **VERIFIED product cost**: `IdentifiedCommand` node card In:1 **Out:0**
  on eShop — the Send→Handle join fails for `IdentifiedCommand<T,R>`, the app trace for
  POST /api/orders/ dead-ends at the Send while the CLI trace (arg-bound inner command) reaches
  `CreateOrderCommandHandler` (evidence: `eval-results/2026-07-27/ui-feature-audit/evidence/eshop-nodecard-identifiedcommand.png`).
- Nested types lose the outer type (walk flattens `Outer.Inner` → `Ns.Inner`) [audit: same file :73].
- Member identity is name-only — overloads collapse [audit: `Graph/CodeGraph.cs:119` `NodeId.ForMember`].
- Call-site multiplicity destroyed at `(From,To,Kind)` dedup [audit: `CodeGraph.cs:265,313`].

### DC2 — Two resolvers with opposite honesty policies
- `CallGraphExtractor.MapSemanticReceiver`: case-insensitive, solution-wide, last-write-wins
  short-name DI map; result stamped `Resolution.Semantic`/`[verified]` [audit: `CallGraphExtractor.cs:551-601`].
- `Graph2/SymbolTable`: explicit `Ambiguous` tier, "no silent winners" [audit: `Graph2/SymbolTable.cs:88-151`].
- Compensating noise deny-lists exist only because of the first resolver
  [audit: `GraphBuilder.Seams.cs:775-780`, `GraphBuilder.Entries.cs:228-247`, `GraphBuilder.Flows.cs:183-192`].
- **VERIFIED product cost**: dup-name CrossService noise — eShop `CreateOrderRequest`/`OrderServices`
  render "CrossService → Basket.API/WebApp" chips and graph nodes (each service defines its own
  `CreateOrderRequest`); inflates Atlas flow "cross-service" counts.

### DC3 — Sync transports missing (TALKS-TO hole)
- **VERIFIED**: eShop CLI Map CROSS-SERVICE = `bus (5)` only. gRPC *server* entries detected
  (Basket.GetBasket/Update/Delete) but zero grpc/http ServiceLinks. Canvases show 8/12 services
  floating unconnected (evidence: `eshop-atlas-canvas.png`, `eshop-home.png`).
- [audit] `AddGrpcServiceLinks` requires client+server both in-solution (`GraphBuilder.ServiceLinks.cs:26`),
  Refit links additionally require a YARP/Ocelot gateway project (`:73-90`).
- HYPOTHESIS: eShop's WebApp gRPC client registration pattern (`AddGrpcClient<Basket.BasketClient>`
  via extensions / Aspire service discovery) doesn't fire `GrpcClientDetection` at all. Needs a probe:
  grep eShop for the registration, then check which detection (if any) captured it.
- `AddHttpClient<T>` typed clients: detected into `DiRegistrationDetection.ExtensionsUsed` but no
  ServiceLink is built from them [audit]. External (out-of-solution) targets render nothing.

### DC4 — Primary-call target quality
- **VERIFIED**: eShop ClientApp — ~10 `[RelayCommand]` entries all target `→ IAppEnvironmentService`
  (the DI dep, not the action); insights evidence rows "POST /api/catalog/items → GetEmbeddingAsync",
  "PUT … → Entry" (evidence: `eshop-entry-audit-table.png`).
- [audit] `ResolvePrimaryCall` keeps the FIRST service callee in call-site order; `datastore`-tag skip
  branch is dead because nothing produces the tag (`GraphBuilder.Entries.cs:141-183`).

### DC5 — Trace/flow content policy diverges by surface
- **VERIFIED**: UI renders `ILogger` as a step (CLI filters it via IsFrameworkLeaf); UI graph default
  depth 1 vs CLI 6 vs MCP budget-shaped; UI follows a different send edge than the CLI for the same
  focus. Three surfaces, three traces.
- [audit] `GetTrace` builds the tree twice with two budgets (`DevContextGrpcService.cs:139-153`);
  trace serialized 4 ways; `RESULT`/`NEXT` blocks are eShop-overfit string tables
  (`TraceRenderer.cs:66-89,180-192`).

### DC6 — Scope honesty (multi-solution repos)
- **VERIFIED**: aspire-samples analyzed as "Metrics · 4 projects · 96 types" — one sample sln
  silently presented as the repo; no indicator, no picker. Affects every multi-sln repo
  (OrchardCore? PowerToys? — check).

### DC7 — Project classification misses
- **VERIFIED**: GitVersion #1 wiring hub = `EmptyRepositoryFixture (602)` — test fixture dominating
  the production graph (test-project naming defeats `ProjectClassifier`).
- Known-latent: refit → `ControllerBased`, StackExchange.Redis → `MinimalApi`, wolverine →
  `CleanArchitecture` style verdicts still produced (suppressed by surfaces, wrong at source) —
  2026-07-19 octet `lens-run/summary.txt`.
- **VERIFIED**: GitVersion style chip renders literally "Unknown" in the app (CliTool has no rung and
  the chip isn't suppressed like libraries').

### DC8 — Detections extracted but never rendered/joined (product-invisible knowledge)
- Aspire resources/relationships (AppHost graph: redis/postgres/service refs) — extracted, zero
  render path [audit: `AspireExtractor.cs` → only OutputSelfCheck reads it]. **VERIFIED cost**: Aspire
  flagship repo shows a generic 4-project map; eShop.AppHost floats as a plain box.
- `MiddlewareDetection`, `IndirectWiringDetection`, `AntiPatternDetection`, full
  `DiRegistrationDetection` shape, `EndpointDetection.AuthAttributes` (Map/Trace), non-aggregate
  `EfEntityDetection` — all land only in DEAD renderers [audit: render-surface report §B.1].
- CLI command tree: F10 insight exists but detects "1 command" on GitVersion (many-command tool) —
  under-detection of its command framework. **VERIFIED** via insights page.

### DC9 — Numbers that disagree (trust killers, cross-surface)
- **VERIFIED**: eShop "37% verified" chip vs tooltip "40% verified, 69% approximate";
  FluentValidation "92 public types" (identity) vs "273 total public types" (insight);
  Studio meter ~1955/4000 vs preview "1804 tok"; "0 of 109 selected" while 9 cards live.

### DC10 — Structural/graph-assembly correctness riders
[audit, engine-core report §B — re-verify then batch in R2]: graph frozen 3× per analysis;
`CallGraphExtractor` BFS is a no-op in Map mode; gateway JSON scan uses Windows-only separators
(walks `.git/bin/obj` fully on Linux); two Roslyn compilations over the same trees; dead vocabulary
(`NodeKind.Message/Store`, `EdgeKind.Exposes/DependsOn`, `RoleTags.DataStore` never produced);
`IPruner` zero impls still injected; `SymbolTable` indexes only Type kind (L1.6);
`BodyFactExtractor` multi-lambda scope pooling (L2.5); `TfmScore` caps at net9 (L3.4).

## 2. Breadth verification protocol (the actual work of the next session)

The eShop findings came from ONE repo drive. Before batching fixes, run the same truth checks across
the supported breadth. We have 47 repos in `eval-repos/`. Protocol:

1. **Build the truth matrix script** (`eval/graph-truth.ps1` or extend `lens-audit.ps1`): for each
   repo, run CLI `analyze` + `query stats` + `query map --format json` and record:
   - transport link counts by kind (bus/grpc/http) vs a hand-written expectation per repo;
   - handler-join reachability: for every `Sends` edge, does the target have an outgoing
     `Handles`/handler edge? (catches DC1/DC2 generically — the IdentifiedCommand class);
   - top-5 hubs: assert no test/fixture types (DC7);
   - entry target sanity: % of entries whose target is a DI-injected service interface vs a real
     action (DC4 proxy metric);
   - style/archetype vs expectation (DC7);
   - solution scope: sln count found vs analyzed (DC6);
   - orphan/dup-name check: count same-short-name types spanning >1 service that received
     cross-service edges (DC3-noise proxy).
2. **Pole coverage** — at minimum run the matrix over: eShop, dotnet-podcasts, aspire-samples,
   CleanArchitecture, VerticalSlice, RazorPages, Blazor(+blazor-samples), gRPC, SignalR, Functions
   (+functions-app, company-functions, AzureFunctions), MassTransit(+sample), wolverine, Hangfire,
   Quartz.NET, Orleans, YARP, Ocelot (gateways), GitVersion, CLI, Spectre.Console (CLI pole),
   FluentValidation, Polly, MediatR, Serilog, AutoMapper, Newtonsoft, refit, RestSharp,
   StackExchange.Redis, Dapper, xUnit (libraries), PowerToys, ScreenToGif, MahApps,
   CommunityToolkit.Mvvm (desktop pole), OrchardCore, bitwarden-server, DntSite, HotChocolate (big/monolith pole).
3. **Record per-repo verdicts** into `eval-results/<date>/graph-truth/MATRIX.md` — defect class ×
   repo grid with FAIL cells linked to raw output. That grid is R2's acceptance baseline: batches
   are done when their cells flip.
4. **Probes for the two open hypotheses**: (a) eShop gRPC client registration → which detection
   fires (DC3); (b) GitVersion command framework → why F10 sees 1 command (DC8).

## 3. Exit criteria for this strand

- Every DC class has: root cause confirmed in code (not just [audit]), blast-radius row in the
  matrix, and either a fix ticket in R2 or an explicit "accepted limitation" note.
- No NEW defect class discovered in the last 10 repos checked (loop-until-dry).
