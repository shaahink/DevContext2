# MediatR — CLI eval (ad-hoc audit)

> `devcontext analyze` on MediatR @ pinned SHA `1fd25f5beb40aafd6859d9225a37d0c4f5062cfa`
> (`jbogard/MediatR`). Capture: `map.md` / `map.json`. Engine `v1.0.5-preview.0.142`.
> MediatR is the acid test: a *library* whose whole purpose is the entry-point (handler) pattern, with
> 10 `samples/MediatR.Examples*` projects (incl. an ASP.NET sample) full of real handlers/endpoints.

## Verdict
The **library surface is excellent**; the **entry-point inventory is wrong** — 18 phantom entries leak in
from the sample projects.

## Scorecard

| Dimension | Result | Verdict |
|---|---|---|
| Archetype | `LIBRARY MediatR (37 public types)` — correct despite 10 sample projects + an ASP.NET sample | ✅ |
| ENTRY API | `register AddMediatR` → `implement IRequestHandler / INotificationHandler / IPipelineBehavior / IRequest` — exactly the "how do I use MediatR" story | ✅ |
| ABSTRACTIONS | IRequestHandler (44), IRequest (38), IPipelineBehavior (31), INotificationHandler (22), IStream*, IRequestException* … the real seats, well-ranked | ✅ (counts include sample/test implementors — inflated but directionally right; known/accepted) |
| PUBLIC SURFACE | clean: `MediatR` (IMediator/ISender/IPublisher/Mediator/Unit…), `.Pipeline`, `.NotificationPublishers`, `.Registration`, `.Wrappers`, `Microsoft.Extensions.DependencyInjection` (AddMediatR) — with docs; samples excluded | ✅ |
| CONSUMER PATHS | wire-into-DI AddMediatR; contract → implement the handler interfaces | ✅ |
| PACKAGES | runtime only — MediatR.Contracts + Microsoft.Extensions.* (Autofac/DryIoc/Lamar/etc. sample deps excluded). `Microsoft.IdentityModel.JsonWebTokens` is a **real** `PackageReference` in `src/MediatR/MediatR.csproj` (verified) — not a leak | ✅ |
| **Entry points** | **`18 entries`** in the stats line — should be **~0** for a library | **❌ DEFECT** |

## Defect: 18 phantom entry points from sample projects

`map.json` has **93 MediatR handler detections** (`requestType`/`handlerType` ×93) — none from `src/`
(which only declares the interfaces); all from `samples/MediatR.Examples*` and `test/`. Test-project
detections are dropped (`IsProductionCode` excludes test projects), but **sample-project** notification
handlers + the `MediatR.Examples.AspNetCore` endpoints become `EntryPoint`s → 18 entries.

**Root cause.** Sample-path exclusion is applied in two of three places:
- `ArchetypeDetector` (the App/Library *decision*) — ignores sample-path entries ✅ (why archetype is right)
- `LibrarySurfaceBuilder` (the *surface*) — excludes sample-path types ✅ (why the surface is clean)
- `GraphBuilder` entry construction / `NoiseFilter.IsProductionCode` (the *entry inventory*) — does **not**
  exclude sample paths ❌

So `snapshot.Entries` (the stats count, and what the desktop entry picker / Angular entries-panel consume)
still contains the 18 sample-derived entries. The CLI **library markdown** doesn't render them (the
library surface has no app entry inventory), so the primary CLI output is unaffected — but the structured
entry inventory is wrong, which would mislead the desktop/Angular entry pickers.

**Fix (proposed).** Make sample-path a non-production signal at the single chokepoint: add
`ProjectClassifier.IsSamplePath` to `NoiseFilter.IsProductionCode` (and/or guard the entry-building
methods `AddHttpEntryPoints` / `AddDomainEventHandlerEntries` / `AddMessageConsumerEntries` /
`AddWorkerEntryPoints` in `GraphBuilder`). This drops sample types + entries from the CodeGraph
consistently with the archetype + surface. Core-path change → re-run the full Eval suite (low app risk —
apps rarely keep production code under `samples/`).

## Post-fix re-audit (applied)

`NoiseFilter.IsProductionEntrySource(filePath)` — not a test project, not generated, not a samples/snippets
path — now gates all four `GraphBuilder` entry methods (which were made instance to reach `_noise`).
Re-captured `map.md`: **entries 18 → 2.** The library surface is unchanged (archetype `Library`; ENTRY API /
ABSTRACTIONS / PUBLIC SURFACE / CONSUMER PATHS intact). The residual 2 are genuine `src/MediatR`
`INotificationHandler` types (e.g. the `NotificationHandler` wrapper) — production code, not phantom
test/sample noise.

Locked by `GraphBuilderTests.NoiseFilter_IsProductionEntrySource_excludes_test_and_sample_sources`.
Verified: fast suite **276/0/2**, full Eval **27/27** — the controller fixture (`tests/fixtures/ControllerApp`)
and eShop trace entries are preserved, because `IsInTestProject` is *project*-based (name/packages), so
non-test projects under a `tests/` path keep their entries.

## Minor
- ABSTRACTIONS implementor counts include sample/test derivers (IRequestHandler shows 44) — inflated but
  ranks the real seats correctly; consistent with the FluentValidation/Polly decision.
