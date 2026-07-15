# Tapestry T1 — Detection strength (evidence)

Branch `feat/tapestry-t1` (off `feat/tapestry-t0`). Delivered checkpoints: **T1.2** (gateway archetype)
and **T1.3** (controller entry→target). These are the two reds the T0 handover staged as its "one
honest caveat" (gates.ps1 Step 3 exited 1 on the pre-existing `yarp` + `dntsite` evals) — fixing them
flips `gates.ps1` to a clean **GATE: PASS**, which is the T0 wrap-up folded into T1 as requested.

Not delivered this session (deferred — see "Remaining T1" below): T1.1, T1.4, T1.5, T1.6.

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

## Gate battery (this session)

| Gate | Result |
|---|---|
| `dotnet build DevContext.slnx` | 0 warnings / 0 errors |
| Fast unit (`Category!=Eval&!CliSmoke&!McpQa`) | Core 448P/3S · Server 14P |
| Truth (`Category=Truth`) | 5P/6S (dogfood presence green) |
| `scripts/loom-guards.ps1` | no banned patterns; truth 0 failures |
| Full eval (`Category=Eval`) | 52P/6S, 0 fail after the dogfood fix (yarp Gateway · dntsite targets · all archetype/style evals) |
| `eval/gates.ps1` | see gates-t1.txt (GATE: PASS — first clean exit, T0 caveat resolved) |

## Baseline drift (R-T8)

T1.2/T1.3 change archetype SELECTION and a target DISPLAY string, not graph structure — dogfood
node/edge/entry presence counts are unchanged (Truth `Dogfood_baseline_presence_ok` green, same
432/330/34 baseline as T0.3). No structural drift.

## Remaining T1 (handed off, not attempted)

- **T1.1 Catalog-driven EntrySeedFiles.** Mechanically small (every `Detection` already carries
  `SourceFile`; add gRPC/Functions/Orleans/GraphQL to `CallGraphExtractor.EntrySeedFiles`), but the
  plan's gate requires a trace ≥2 deep per surface, and the existing grpc/orleans/functions evals are
  FRAMEWORK repos (archetype Library, no app entries) — proper proof needs new *service-app* fixtures
  (like the T1.3 `ConventionalController` fixture). Deferred to avoid an R-T1 detection-without-proof.
- **T1.4** runnable/per-service inference · **T1.5** style-ladder arbitration · **T1.6** feature-area
  derivation — need the shamshir pole + 22-repo bench regression sweep. Next session.
