# Tapestry T1 — Detection strength (evidence)

Branch `feat/tapestry-t1` (off `feat/tapestry-t0`). Delivered checkpoints: **T1.2** (gateway archetype),
**T1.3** (controller entry→target), **T1.1** (catalog-driven entry seeds). T1.2/T1.3 are the two reds the
T0 handover staged as its "one honest caveat"; T1.1 was deferred in the first T1 session for lack of a
service-app fixture — now delivered.

Not delivered yet (see "Remaining T1" below): T1.4, T1.5, T1.6, T1.7, T1.8, T1.9.

---

## T1.1 — Catalog-driven entry seeds (gRPC/Functions/GraphQL traces go deep)

**Symptom.** `CallGraphExtractor.EntrySeedFiles` — the seed set for Map-mode call-graph binding — was a
hardcoded four detection types (endpoints, MediatR handlers, background workers, SignalR hubs). gRPC
services, Azure Functions, Orleans grains, and GraphQL resolvers got **no call-graph seed in Map mode**,
so their entry methods had no Calls edges: entry→target resolved to nothing (or the owning type) and their
precomputed Flows were depth-1 — "as shallow as hubs once were" (proposal T1.1).

**Fix (three parts, R-T1: detect = render = serve = pinned).**
1. **Seed contract.** New marker `IEntrySurfaceDetection` on every AppEntry-producing detection
   (`Endpoint`, `MediatRHandler`, `BackgroundWorker`, `SignalRHub`, `GrpcService`, `FunctionEntry`,
   `Grain`, `GraphQlField`, `MessageConsumer`, `CliCommand`). `EntrySeedFiles` now unions the `SourceFile`
   of every such detection — catalog-driven, so a new AppEntry surface feeds seeds automatically with no
   `CallGraphExtractor` edit. (Desktop UI stays out until its entry taxonomy is cleaned up in T1.7.)
2. **Member anchoring (render).** The gRPC builder already created the RPC member node and anchored the
   entry's Calls edge on it; the Functions and GraphQL builders anchored on the owning **Type** node, whose
   `ResolveEntryTarget` branch only reads `Sends` — so a plain-service function/resolver showed no target
   even once seeded. Both builders now create the handler member up-front (mirroring the HTTP builder) and
   anchor the entry on it, so the seeded Calls edges surface via `ResolvePrimaryCall`.
3. **Fixture + pins.** New service-app fixture `eval/fixtures/ServiceSurfaces` (a single API tripping the
   gRPC + Functions + GraphQL package signals, each surface's handler delegating to a distinctly-named
   domain service). It lives under `eval/fixtures`, **not** `tests/fixtures`, so `DependencyExtractor`'s
   test/sample path suppression doesn't hide its package signals (the existing `tests/fixtures/*` all
   detect via code signals, not package signals — this is the first package-signal fixture).

**Verification (rendered Map, `analyze eval/fixtures/ServiceSurfaces`): 3 entries, 3/3 target.**
| Entry | Resolved target |
|---|---|
| `gRPC   Greeter.SayHello` | `GreetingService.BuildGreeting` |
| `Functions  OrderIngestFunction.Run [Function]` | `OrderIngestService.IngestOrder` |
| `GraphQL  Query/CatalogQuery.GetProduct` | `CatalogLookupService.FindProduct` |

- `EntrySurfaceSeedTests` (3 theory cases) asserts each entry's `Target` is the service method — resolves
  only if the file was seeded AND the entry anchors on the handler member.
- `servicesurfaces.json` pins the three signals, the three detection counts, and the three targets in the
  rendered markdown (the R-T1 eval pin, runs in `gates.ps1` Step 3).
- Full eval tier green: **57P / 6S / 0 fail** — no regression on the framework repos that exercise the
  builder change (`grpc`, `azurefunctions`, `orleans`, `functions-app`, `signalr-app`, eShop, dogfood).
- MCP QA green against dogfood (which has real gRPC services) — the seed change causes no MCP regression.

**Discovered but deferred (recorded as T1.5 fodder, NOT fixed here).** An exploratory version of this
change applied `DependencyExtractor`'s test/sample path conventions repo-relative (so package signals fire
for a fixture under `tests/`). That surfaced a real **style-detector** bug: `Microsoft.AspNetCore.OpenApi`
maps to the `MinimalApis` signal at confidence 1.0, which out-ranks the code-derived `Controllers` signal
and flips a controller app (`CompositionApp`) to `MinimalApi` — exactly the T1.5 arbitration gap
("controllers-heavy app must not read MinimalApi"). Rather than pull T1.5 (which needs the 22-repo bench)
into T1.1, the fixture was relocated to `eval/fixtures` and the `DependencyExtractor` change reverted.
Two findings for T1.5: (a) OpenApi is used by controller AND minimal-API apps — a false MinimalApi signal;
(b) `DependencyExtractor` suppresses package signals for a repo whose root lives under a `/tests/` or
`/samples/` segment, unlike `NoiseFilter` which is already repo-relative.

---

## T1.2 — Gateway archetype rung (`yarp` eval: App → Gateway)

**Symptom.** `yarp.json` `archetype-gateway` expected `Gateway`, detector returned `App`
(`Expected 'Gateway', got 'App'`), a pre-existing red.

**Root cause (two bugs in `ArchetypeDetector`'s gateway block).**
1. `IsRunnableService` counted any project *referencing* an `AspNetCore` package as a "runnable
   service". YARP's own `Yarp.ReverseProxy` is an `<OutputType>Library` built ON ASP.NET Core — so it
   was miscounted as a second peer service.
2. The peer-service count excluded neither samples/tests nor `testassets/` — the YARP source repo
   ships ~30 sample/test/testasset web+exe hosts, so the count was always ≥2 → App.

**Fix.** Count only GENUINE peer services: real hosts (`Exe` or `Microsoft.NET.Sdk.Web`, via the new
`IsRunnableHost`) that pass `NoiseFilter.IsProductionEntrySource` (which already excludes
samples/tests/testassets/benchmarks/templates) and don't themselves reference the reverse-proxy
package. `peerServiceCount ≥ 2` ⇒ App (gateway is a role in microservices); else Gateway.

**Why not "self-source ⇒ Gateway".** An earlier attempt short-circuited on the gateway signal being
self-sourced (`DetectedVia ProjectName`). That is WRONG and was caught by the dogfood Truth gate: the
dogfood repo (`run-aspnetcore-microservices`) contains a project literally named `YarpApiGateway`,
which self-sources the signal exactly as YARP's own repo does — the rule flipped the whole
microservices app to Gateway (`Dogfood_baseline_presence_ok` failed: "MAP" not found). The peer count
is the correct discriminator; self-source is not.

**Verification.**
- `yarp` eval → Gateway (was App). ✓
- `Dogfood_baseline_presence_ok` (dogfood microservices, HAS `YarpApiGateway`) → still App, "MAP"
  present. ✓ (no regression — the earlier self-source attempt broke exactly this.)
- `eShop` eval → `style=Microservices` unchanged (eShop has no gateway signal: its `Yarp.ReverseProxy`
  package doesn't match the catalog's `Ocelot`/`Microsoft.ReverseProxy` keys). ✓
- Render (R-T1): `MapRenderer` already emits `GATEWAY`; MCP carries `map.Archetype`; `yarp.json`
  pins it. Detect + render + serve + eval all covered.

## T1.3 — Controller entry→target (`dntsite` eval: no target → resolved)

**Symptom.** `dntsite.json` `target-feednews`/`target-feedcomments` expected
`FeedsService.GetNewsAsync` / `FeedsService.GetCommentsAsync` in the Map markdown; both were absent.
DntSite eval runtime ≈ 6m45s — too slow to iterate on directly.

**Reproduced fast** with a new minimal fixture `tests/fixtures/ConventionalController` mirroring
DntSite's `FeedController` shape: primary-constructor injection, conventional `[controller]/[action]`
routing (no HTTP verb attributes), expression-bodied actions wrapping the service call in
`new FeedResult<T>(await feedsService.GetNewsAsync(await ShowBriefDescriptionAsync()))`, and a
primary-ctor `FeedsService` that is auto-registered (NO explicit `AddScoped` — so interface→impl
resolves from the type graph, not a DI registration, exactly like DntSite).

**Root cause.** The target rendered as a BARE method name (`GetNewsAsync`) instead of
`FeedsService.GetNewsAsync`. A Graph2 semantic body-scan seam (`PlainCallDetector`) creates a member
node whose title is just the method name, though its NodeId still encodes the owning type.
`ResolvePrimaryCall` returned `callee.Title` verbatim → an ownerless verb. (ControllerApp passed only
because its DI-registered `ProductService` produced an already-qualified title.)

**Fix.** `ResolvePrimaryCall` now returns `TargetTitle(...)`, which reconstructs `Type.Method` from the
resolved owning-type node when the callee is a member whose title is bare (no `.`). Already-qualified
titles and Type callees are unchanged (ControllerApp unaffected).

**Verification.**
- New fixture: `GET /Feed/News → FeedsService.GetNewsAsync`, `GET /Feed/Comments →
  FeedsService.GetCommentsAsync`. ✓ (`conventionalcontroller.json` pins both — the fast R-T1 pin.)
- `dntsite` eval → `target-feednews` + `target-feedcomments` PASS (in the full 9m20s eval run). ✓
- `ControllerApp` eval → `ProductService.GetByIdAsync`/`CreateAsync`/`DeleteAsync` still resolve. ✓

---

## Gate battery

**T1.2 / T1.3 session:**

| Gate | Result |
|---|---|
| `dotnet build DevContext.slnx` | 0 warnings / 0 errors |
| Fast unit (`Category!=Eval&!CliSmoke&!McpQa`) | Core 448P/3S · Server 14P |
| Truth (`Category=Truth`) | 5P/6S (dogfood presence green) |
| `scripts/loom-guards.ps1` | no banned patterns; truth 0 failures |
| Full eval (`Category=Eval`) | 52P/6S, 0 fail after the dogfood fix (yarp Gateway · dntsite targets · all archetype/style evals) |
| `eval/gates.ps1` | see gates-t1.txt (GATE: PASS — first clean exit, T0 caveat resolved) |

**T1.1 session:**

| Gate | Result |
|---|---|
| `dotnet build DevContext.slnx` | 0 warnings / 0 errors |
| Fast unit (`Category!=Eval`) | Core 453P/3S · Server 14P |
| Truth (`Category=Truth`) | 0 failures (skips are the pending ratchet) |
| `scripts/loom-guards.ps1` | no banned patterns |
| Full eval (`Category=Eval`) | **57P/6S, 0 fail** (adds servicesurfaces + 3 EntrySurfaceSeed cases; grpc/functions/orleans/eShop/dogfood all green) |
| MCP QA (`gates.ps1` Step 2b) | passed against dogfood (real gRPC) — no MCP regression from the seed change |
| `eval/gates.ps1` | see gates-t1-final.txt (GATE: PASS) |

## Baseline drift (R-T8)

T1.2/T1.3 change archetype SELECTION and a target DISPLAY string, not graph structure — dogfood
node/edge/entry presence counts are unchanged (Truth `Dogfood_baseline_presence_ok` green, same
432/330/34 baseline as T0.3). No structural drift.

**T1.1:** the catalog-driven seed adds gRPC/Functions/GraphQL/consumer/CLI files to the Map-mode bind set,
so a repo with those surfaces gains Calls edges (deeper flows, more resolved targets) — strictly additive.
Measured on dogfood by the McpQa gate (`mcp-qa.md`): **432 → 439 nodes (+7), 330 → 339 edges (+9)**, 34 entries
unchanged — the Discount gRPC service's RPC members now carry Calls edges, deepening the checkout flow trace
**43 → 46 steps**. Truth presence + MCP QA green; no eval-repo archetype/style/count evals moved (57P/0F).
Shamshir NOT re-driven (no new surface kinds vs its SignalR/worker/HTTP mix; the seed change can only deepen).

## Remaining T1 (handed off, not attempted)

- **T1.4** runnable/per-service inference · **T1.5** style-ladder arbitration (has two ready findings from
  T1.1's exploration — see the OpenApi/test-path notes above) · **T1.6** feature-area derivation — need the
  shamshir pole + 22-repo bench regression sweep.
- **T1.7** entry taxonomy hygiene · **T1.8** kind single-sourcing · **T1.9** topology noise — audit-weave
  checkpoints, need the eShop pole + a UI drive (NG0955 / facet-count assertions).
