# Master Plan — Universal .NET Coverage (V2)

> Status: **DRAFT / READY TO EXECUTE** · Authored 2026-07-01 · Base: `develop` @ `4f59cef`
> Companion docs in this folder: [`HANDOVER.md`](HANDOVER.md) (what P0/P3/P1 delivered),
> [`FINDINGS-AND-PLAN.md`](FINDINGS-AND-PLAN.md) (the 27-repo analysis that motivated it).
> This plan supersedes the "Implementation Order" in FINDINGS-AND-PLAN §"Implementation Order".

---

## 0. North Star & success criteria

**Goal:** DevContext is the first tool you point at *any* .NET repo — app, library, framework, gateway,
or mix — and within seconds it tells you (a) **what kind of thing this is**, (b) **every way execution
enters it**, and (c) lets you **trace any entry inward** through real wiring (DI, MediatR, events,
reflection) with honest confidence markers.

A repo is "covered" when **all three** hold:

| Layer | Bar |
|-------|-----|
| **Archetype** (first glance) | Correct App / Library / Gateway. No false flips. |
| **Map** (at a glance) | Every entry surface the repo actually exposes is *rendered* — not just detected. Each entry shows `→ target` and `(file:line)`. |
| **Trace** (in depth) | A focus on any entry descends through DI/dispatch/event/reflection seams with `[verified]`/`[approx]` markers, and is non-empty. |

**Anti-goals (kernel hygiene):** adding the Nth app shape must not grow `GraphBuilder`, `DiscoveryPipeline`,
`ArchetypeDetector`, or any renderer by more than a single registration line. The engine stays a set of
small, self-describing plugins over a stable core. If a change requires editing 5 scattered tables, the
architecture has failed and must be corrected (see §3).

**Done = green gate:** `eval/gates.ps1` exit 0 (build 0-warn + fast tests + 39+ eval + CLI matrix).

---

## 1. Mental-model corrections

Three reframings underpin every workstream. Internalize these before touching code.

### 1.1 "Entry surface", not "HTTP + a few specials"

The universal abstraction is already named in `EntryPoint.cs`: *"A place execution can enter the system."*
HTTP, gRPC, hub method, function trigger, message consumer, hosted/scheduled worker, UI command, Orleans
grain call, Razor page handler, CLI command, and (for libraries) public API — these are **all the same
kind of thing**: an *entry surface*. The Map is the inventory of entry surfaces; the Trace starts at one.
Treat every new shape as "another entry-surface provider", never as a special case in the core.

### 1.2 Archetype is decided by **production app-entries + signal provenance**, not by signal presence

The P0 fix forced `Library` whenever a framework signal (`grpc`/`signalr`/`azure-functions`/…) was present,
*before* inspecting entries (`ArchetypeDetector.cs:36`). That conflates two different repos:

- **The framework's own source** (`grpc-dotnet`, `aspnetcore/SignalR`) — *is* the framework → Library. Its
  hub/service/function definitions live in `test/`, `samples/`, `benchmarks/`.
- **A consumer app** (an order service that *references* `Grpc.AspNetCore`) — *uses* the framework → App.
  Its services/hubs/functions live in production `src/`.

The precise discriminator is already available and unused:
- **Signal provenance** — `FeatureSignal.DetectedVia` distinguishes `"ProjectName"`/`"ProjectReference"`
  (this repo *is/contains* the framework) from `"PackageReference"`/`"ProjectSdk"` (this repo *uses* it).
- **Production-path filtering** — `NoiseFilter.IsProductionEntrySource` + `ProjectClassifier.IsSamplePath`
  already exclude test/sample/benchmark entries.

So archetype becomes: *Gateway signal → Gateway. Else, a self-sourced framework signal → Library. Else,
**production** (non-sample/test) app-entries exist → App. Else → structure fallback.* This keeps all 8
framework repos Library **and** lets consumer apps be Apps that render their entries.

### 1.3 Detected ≠ rendered

`GraphBuilder` turns detections into `EntryPoint`s, but `LibrarySurfaceRenderer` never reads
`map.Entries`. For any `Archetype.Library` repo the entry inventory is **silently discarded**
(`DiscoveryPipeline.cs:380-382` dispatches Library → `LibrarySurfaceRenderer`). Every new entry kind must
land **all the way to rendered output**, and an eval check must assert the rendered line — detection counts
are not evidence of coverage.

---

## 2. Current-state assessment

### 2.1 What is solid (preserve, do not regress)

- **Framework-repo archetypes** are correct & tested (gRPC/SignalR/Functions/MassTransit/Orleans/Quartz/
  xUnit → Library; YARP → Gateway).
- **Kernel-hygiene refactor** (`IEntryPointBuilder` + `GraphBuilder._entryBuilders[]`) is good and is the
  foundation §3 builds on.
- **Signal provenance is captured** (`FeatureSignal.DetectedVia`) — the lever for §1.2 exists.
- **Indirect wiring is connected and graph quality is good** — `IndirectWiringDetector` (reflection /
  service-locator / dynamic-proxy) feeds the graph; member-origin edges, DI `Resolves`, MediatR `Handles`,
  pipeline `WrappedBy`, `Raises`/`Sends`/`ReadsWrites` body seams are in place. **This is a regression
  surface, not a build surface** — guard it (WS-F), don't disturb it.

### 2.2 The five gaps (with evidence)

| # | Gap | Evidence | Effect |
|---|-----|----------|--------|
| **G1** | gRPC/SignalR/Functions signals have **no package/SDK source** — only project-name | `DependencyExtractor.cs` `PackageSignalMap` (L7-41) lacks `Grpc.AspNetCore`/`Microsoft.AspNetCore.SignalR`/`Microsoft.Azure.Functions.Worker`; signals emitted only from `ProjectNameSignalMap` (L158-173) | A real consumer app never fires `GrpcServiceExtractor`/`SignalRHubExtractor`/`AzureFunctionsExtractor` (`ShouldRun` gates on the signal) → 0 detections |
| **G2** | Archetype **conflation** — signal presence forces Library before entry inspection, ignoring provenance | `ArchetypeDetector.cs:36` calls `IsLibraryWithOptionalAppSurface` (L83-102) which checks `Has(sig)` only | Even if a consumer app got the signal, it'd be Library → entries discarded |
| **G3** | **Render drop** — Library view ignores `map.Entries` | `LibrarySurfaceRenderer.cs` (renders ENTRY API/ABSTRACTIONS/…; never reads `map.Entries`); dispatch `DiscoveryPipeline.cs:380-382` | Framework repos' own detected services/hubs/functions are invisible; HANDOVER "validation counts" are detections, not output |
| **G4** | **Eval blind spot** — no check asserts a gRPC/hub/function entry is rendered | `eval/expectations/{grpc,signalr,azurefunctions}.json` assert only archetype + signal + `"LIBRARY"` + `DiRegistrationDetection` count | G1-G3 are uncaught; MassTransit's `"Bus"` check passes coincidentally via library type names |
| **G5** | **False-positive archetype flips** — `Contains` project-name matching | `DependencyExtractor.cs:175-190` (`Contains("Functions")`, `Contains("Grpc")`, …) | A normal app with a `Company.Functions` helper project flips to Library |

> Blazor is **not** affected: its signal comes from the `Microsoft.AspNetCore.Components` package, `blazor`
> is not a library-framework signal, so a Blazor app stays App → `MapRenderer` → `@page` routes render.
> Blazor is the reference shape the other three should match end-to-end.

---

## 3. Kernel-hygiene architecture — the Entry Surface Catalog

**Problem:** a new app shape today touches **seven** places, several of them scattered tables:
1. `ArchitectureSignals.Keys.*` (signal key) — 2. `DependencyExtractor.PackageSignalMap` /
`ProjectNameSignalMap` (signal source) — 3. extractor — 4. `Detections.cs` record + `[JsonDerivedType]` —
5. `EntryPoint.cs` `EntryPointKind` — 6. `IEntryPointBuilder` + `GraphBuilder._entryBuilders[]` —
7. `MapRenderer.GroupLabel` + `ArchetypeDetector.AppEntryKinds`/`LibraryFrameworkSignals`.

Items 1, 2, 5(role), 7 are **pure data** repeated across files. That repetition is how the engine
"explodes". **Fix: collapse the data into one declarative catalog.**

### 3.1 The descriptor (data) — `EntrySurfaceCatalog`

```csharp
// src/DevContext.Core/Graph/EntrySurfaces/EntrySurfaceDescriptor.cs
public enum SurfaceRole { AppEntry, FrameworkLibrary, Gateway }

public sealed record EntrySurfaceDescriptor(
    string            SignalKey,        // ArchitectureSignals.Keys.*
    EntryPointKind?   Kind,             // null for non-entry signals (efcore, serilog…)
    string            RenderLabel,      // "gRPC", "SignalR", "Functions", "Blazor"
    SurfaceRole       Role,             // AppEntry ⇒ counts toward App; FrameworkLibrary ⇒ self ⇒ Library
    ImmutableArray<string> Packages,    // consumer detection (package prefix ⇒ signal, via=PackageReference)
    ImmutableArray<string> SdkHints,    // e.g. "Microsoft.NET.Sdk.Functions"
    ImmutableArray<string> SelfNamePatterns); // assembly-name patterns ⇒ "this IS the framework" (via=ProjectName)
```

A single `static EntrySurfaceCatalog.All` lists every shape. **Every scattered table becomes a projection
of this catalog**, computed once:
- `DependencyExtractor` ← `Packages` / `SdkHints` / `SelfNamePatterns` (emits signals with **correct
  provenance** — package ⇒ `PackageReference`, self-name ⇒ `ProjectName`).
- `ArchetypeDetector` ← `Role` + provenance (no hand-maintained `LibraryFrameworkSignals` /
  `AppEntryKinds` lists).
- `MapRenderer.GroupLabel` ← `RenderLabel`.

**Net effect:** adding a shape = **one descriptor + one extractor + one builder + one detection record**.
The extractor and builder are the irreducible per-shape logic. Everything else is data in one file. This is
the explicit "won't explode" guarantee and the acceptance bar for every WS-E rung.

### 3.2 Migration is additive, not a rewrite

Do **not** rip out the existing tables in one commit. Introduce the catalog, make the tables *read from it*,
delete the now-empty tables once tests are green. `IEntryPointBuilder` and the extractor/pipeline
auto-discovery (`[DiscoveryAssembly]` + `ExtractorRegistry`) are kept as-is — they already satisfy §0's
anti-goal for the *build* half; the catalog fixes the *data* half.

### 3.3 The one-shape checklist (the contract every rung follows)

```
1. Add EntrySurfaceDescriptor to EntrySurfaceCatalog.All        (data: signal, packages, role, label)
2. Add EntryPointKind value (EntryPoint.cs)                      (if a new kind)
3. Add Detection record + [JsonDerivedType] (Detections.cs)
4. Write extractor : IDiscoveryExtractor (ShouldRun gates on SignalKey)  ← per-shape logic
5. Write builder : IEntryPointBuilder; register in _entryBuilders[]      ← per-shape logic
6. Add eval expectation incl. a *render-level* entry-kind check (§6)
— Zero edits to GraphBuilder.Build, DiscoveryPipeline, ArchetypeDetector, MapRenderer, ArchetypeDetector lists.
```

---

## 4. Workstreams

Each task is agent-executable: **Intent · Files · Approach · DoD**. IDs are stable for resumption.
Sequencing & dependencies in §7.

### WS-A — Fix the broken three (P0 correctness, highest ROI)

> Makes gRPC/SignalR/Functions work end-to-end on **consumer apps**, where they matter. Reuses the
> already-built extractors/builders; the bug is in wiring (G1) + archetype (G2) + render (G3).

- **A1 — Package/SDK signal sources.** *Intent:* a consumer app firing the right signal.
  *Files:* `DependencyExtractor.cs` (or the catalog in §3 if WS-D lands first).
  *Approach:* map `Grpc.AspNetCore*` → `grpc`; `Microsoft.AspNetCore.SignalR*` → `signalr`;
  `Microsoft.Azure.Functions.Worker*` + SDK `Microsoft.NET.Sdk.Functions` → `azure-functions`. Emit with
  `via:"PackageReference"`/`"ProjectSdk"`. *DoD:* unit test — a csproj referencing `Grpc.AspNetCore` yields
  signal `grpc` with `DetectedVia=="PackageReference"`.

- **A2 — Provenance-gated archetype** (depends A1, WS-B/B1). *Intent:* consumer app with these signals
  stays App; framework repo stays Library. Covered by **B1**.

- **A3 — Render entry inventory for non-App archetypes (safety net).** *Intent:* even if a framework repo
  has production entries we *want* shown, they render. *Files:* `LibrarySurfaceRenderer.cs`.
  *Approach:* after ABSTRACTIONS, add an optional `ENTRY POINTS` block reusing `MapRenderer`'s grouping for
  any `map.Entries` that survive production filtering (usually empty for clean framework repos — correct).
  *DoD:* a Library repo *with* production entries renders them; a clean framework repo (entries all
  sample/test) renders none. (Mostly mooted by B1 for consumer apps, but closes G3 defensively.)

- **A4 — GroupLabel for new kinds.** *Files:* `MapRenderer.cs:284-294` (→ catalog `RenderLabel` post WS-D).
  *Approach:* `GrpcService→"gRPC"`, `SignalRHub→"SignalR"`, `FunctionEntry→"Functions"`, `BlazorPage→
  "Blazor"`. *DoD:* Map renders `gRPC (n)` not `GrpcService (n)`.

- **A5 — Validate on a real App.** *Intent:* prove the path. *Approach:* run CLI on **eShop** (`Basket.API`
  is a gRPC service) → expect a `gRPC` group in the Map with `→ target`. Capture before/after in
  `analysis/`. *DoD:* rendered Map shows the Basket gRPC service entry.

### WS-B — Archetype model correctness

- **B1 — Provenance + production-entry decision.** *Intent:* §1.2 design.
  *Files:* `ArchetypeDetector.cs`. *Approach:* rewrite `Detect`:
  1. Gateway signal → Gateway.
  2. A `FrameworkLibrary`-role signal whose `DetectedVia ∈ {ProjectName, ProjectReference}` → Library.
  3. Production app-entries (`AppEntryKinds`, filtered by `IsProductionEntrySource`/`!IsSamplePath`) → App.
  4. Existing structure fallback.
  Remove the blunt `Has(sig)` force; keep the `LibraryFrameworkSignals` set as *role* data (or catalog).
  *DoD:* all 8 framework repos stay Library/Gateway; a synthetic consumer-app fixture with a `grpc` package
  signal + a production service → App.

- **B2 — Tighten self-name matching (fixes G5).** *Files:* `DependencyExtractor.cs:175-190` (or catalog
  `SelfNamePatterns`). *Approach:* match assembly-name **prefix/segment** (`Grpc.`, `Grpc.AspNetCore`,
  `Microsoft.AspNetCore.SignalR`, `Yarp.ReverseProxy`) not `Contains`; require the pattern to cover a
  **plurality** of non-test projects before it implies "this IS the framework". *DoD:* a guard test — an App
  with one `Company.Functions` helper project stays App.

### WS-C — Eval coverage (close G4, the blind spot)

- **C1 — Render-level entry-kind check.** *Intent:* assert entries reach output. *Files:* eval harness
  (`eval/` check types), `eval/expectations/*.json`. *Approach:* add check type `entry-kind-present`
  (asserts `$.entries[*].kind` contains X **and** the rendered markdown contains the `RenderLabel` group
  header). *DoD:* new check type runs in the harness.

- **C2 — Consumer-app fixtures.** *Intent:* test the App path, not just framework Library path. *Approach:*
  add minimal fixtures (or reuse eShop gRPC): a tiny SignalR app (1 hub), a tiny Functions worker (1
  `[Function]`), confirm eShop gRPC. Add `entry-kind-present` expectations for each. *DoD:* expectations
  exist and pass post WS-A/B.

- **C3 — Archetype guard expectations.** *Approach:* expectation that the §B2 false-positive fixture stays
  App. *DoD:* fails before B2, passes after.

### WS-D — Entry Surface Catalog refactor (kernel hygiene, §3)

- **D1 — Introduce `EntrySurfaceCatalog` + descriptor.** *Files:* new `Graph/EntrySurfaces/`. *Approach:*
  encode the existing ~10 entry shapes + framework-library signals as descriptors. *DoD:* catalog compiles;
  unit test enumerates expected shapes.

- **D2 — Make `DependencyExtractor` read the catalog.** *DoD:* signal tables delegate to catalog;
  existing signal tests green.

- **D3 — Make `ArchetypeDetector` read `Role` from catalog.** *DoD:* delete `LibraryFrameworkSignals` /
  `AppEntryKinds` literals; archetype tests green.

- **D4 — Make `MapRenderer.GroupLabel` read `RenderLabel`.** *DoD:* label test green; A4 folded in.

> WS-D may land **before or after** WS-A/B. Recommended: do A/B first (fix correctness fast), then D
> (refactor under green tests). A/B written against current tables port trivially to the catalog.

### WS-E — New coverage ladder (highest-value *additions*)

Prioritized by **(population × correctness-debt) ÷ cost**. Each rung = one §3.3 checklist pass.
Correctness debt (we currently emit *wrong/empty* output) outranks raw breadth.

| Rung | Shape | Why this rank | Entry kind | Detect on |
|------|-------|---------------|-----------|-----------|
| **E1** | **Razor Pages** | Huge LOB-app population; today **invisible** (no entry) | `HttpEndpoint` (reuse) | `PageModel` subclass + `OnGet/OnPost*` handlers; `.cshtml` `@page` |
| **E2** | **Orleans grains** | We already classify Orleans repos as Library but show **no grain surface** — the grains *are* the entry surface | new `GrainMethod` | `Grain`/`IGrainWithXKey` impls + public grain-interface methods |
| **E3** | **Desktop breadth: WinForms · MAUI · Avalonia** | Completes the desktop dimension W5 started (WPF); shares `UiEntry` machinery → low marginal cost | `UiEntry` (reuse) | `Form`/`ContentPage`/`Window`+`UserControl` bases; MAUI `[RelayCommand]` already covered |
| **E4** | **Messaging breadth: NServiceBus · Azure Service Bus · Wolverine** | `nservicebus` signal already reserved-but-unmapped; messaging is a core backend shape | `MessageConsumer` (reuse) | `IHandleMessages<T>`, `ServiceBusProcessor` handlers, Wolverine handler conventions |
| **E5** | **AWS Lambda** | Serverless breadth beyond Azure | new `FunctionEntry` (reuse) or `LambdaHandler` | `ILambdaFunction`/`[LambdaFunction]`/`FunctionHandler` + `Amazon.Lambda.*` packages |
| **E6** | **GraphQL resolvers (HotChocolate)** | Pair with HotChocolate scoping (deferred); query/mutation/subscription types are entries | new `GraphQlField` | `[QueryType]`/`[MutationType]`/`ObjectType<T>`; `graphql` signal exists |
| **E7** | **CLI commands (Spectre.Console.Cli · System.CommandLine)** | Tooling repos; `Command<T>.Execute`, command builders | new `CliCommand` | `Command<TSettings>` subclass / `RootCommand` handlers |

> Rungs are independent; pick by demand. **E1 and E2 are the recommended first additions** — E1 for
> population, E2 because it finishes a shape we already half-support. Do **not** start a rung until WS-A/B/C
> are green (the catalog + render + eval scaffolding they establish is what keeps each rung cheap).

### WS-F — Graph quality & indirect wiring (regression + new-kind enrichment)

- **F1 — New entries participate in target resolution.** *Intent:* a gRPC/hub/function/grain entry resolves
  `→ target` like HTTP does. *Files:* `GraphBuilder.EnrichEntryTargets`/`ResolveEntryTarget`, the new
  builders. *Approach:* ensure each new builder sets `HandlerNode` to the impl Type and that the impl's
  primary service call / dispatch is resolved (reuse `ResolvePrimaryCall`). *DoD:* eShop Basket gRPC entry
  shows `→ <repository/service>`, not blank.

- **F2 — Traces from new entries are non-empty.** *Approach:* a focus on a hub/service/function descends via
  `Calls`/`Sends`/`Resolves`. *DoD:* `--focus <HubType>` yields ≥1 hop with a confidence marker.

- **F3 — Indirect-wiring regression guard.** *Intent:* protect the "already connected" wiring the user
  values. *Approach:* a golden test asserting reflection/service-locator/MediatR/DI/event seam counts on a
  reference repo don't drop. *DoD:* test fails if a seam regresses.

### WS-G — Library surface quality (P2, from FINDINGS §"P2")

- **G-a — Base-type & attribute seats.** *Intent:* FluentValidation shows `AbstractValidator<T>` seat;
  xUnit shows `[Fact]`/`[Theory]`/`ITestFramework`. *Files:* `LibrarySurfaceBuilder.cs`
  (`BuildAbstractions`/`BuildEntryApi`). *DoD:* FluentValidation surface lists the validator seat; xUnit
  lists the attribute entry API. *Status:* genuinely lower value than WS-A–F; schedule after.

---

## 5. Target output specs (the output we want from the new support)

The bar for "rendered correctly". An **App** that uses these frameworks renders them in `ENTRY POINTS`:

```text
ENTRY POINTS
   gRPC (1)
      Basket.BasketService (4 methods: GetBasket, UpdateBasket, …)  → RedisBasketRepository
            (src/Basket.API/Grpc/BasketService.cs:23)
   SignalR (2)
      NotificationHub (3 methods: Subscribe, Send, …)  → INotificationService
            (src/Web/Hubs/NotificationHub.cs:15)
   Functions (1)
      OrderProcessor.Run [QueueTrigger]  → OrderService.Process
            (src/Functions/OrderProcessor.cs:22)
   HTTP (12)
      …
```

Rules:
- Group header is the `RenderLabel` + count; capped at `MaxEntriesPerKind` with `… and N more` disclosure.
- Each line: title `(method digest)` · `→ target` (F1) · `(repo-relative file:line)`.
- A **framework's own repo** (Library) renders **no** entry block for these — its hubs/services live in
  `test/`/`samples/` and are filtered; the value there is the `PUBLIC SURFACE`/`ABSTRACTIONS` (e.g. `Hub`,
  `IHubContext<T>`). This *absence* is correct, and C2's framework expectations assert it.

---

## 6. Eval & gate strategy

- **Add a render-level check** (`entry-kind-present`, WS-C1): asserts both `$.entries[*].kind` **and** the
  rendered group header. This is the check that would have caught G1-G3.
- **Two fixtures per shape**: the **framework repo** (Library, asserts archetype + *absence* of app entry
  block) and a **consumer app** (App, asserts the entry renders). Reuse eShop for gRPC.
- **Archetype guard** (WS-C3): a fixture App with a framework-named helper project stays App.
- **Keep all checks `"expected"` (blocking)** per HANDOVER convention; no aspirational tier.
- **Perf budget:** the syntax-tree extractors scan `AllSourceFiles`; they share the parse cache and gate on
  signal, but verify huge-repo runs stay within `max-elapsed-ms`. Use the `devcontext-bench` skill if a rung
  regresses Map time.
- **Gate:** `eval/gates.ps1` exit 0. Per HANDOVER §7: build 0-warn, fast tests (304 core / 64 desktop / 12
  server), eval (≥39), CLI matrix.

---

## 7. Sequencing, dependencies, resumability

```
WS-A (A1,A3,A4,A5) ─┐
WS-B (B1,B2) ───────┼─► green correctness  ─► WS-D (catalog refactor under green tests)
WS-C (C1,C2,C3) ────┘                                   │
                                                        ▼
                              WS-E rungs (E1,E2 first), each via §3.3 checklist
                              WS-F runs alongside every entry-producing task
                              WS-G last (lower value)
```

**Critical path:** A1 → B1 → (A3/A4) → C1/C2 proves it → D refactors → E adds breadth.
**Hard dependencies:** B1 depends on A1 (needs the package-sourced signal to test the App branch). C2
depends on A+B (fixtures only pass once correctness lands). E* depend on C1 (the render check is how each
rung proves itself) and benefit from D (cheaper per rung).

**Resume protocol for an agent:**
1. Read this file + `HANDOVER.md` §5 (extension model) + §3.3 here (one-shape checklist).
2. `git log --oneline -15` to see which WS-IDs are done (commit messages reference WS-IDs).
3. Pick the next unstarted task on the critical path. Write the failing eval/unit test first.
4. Implement → `eval/gates.ps1` → commit `feat/fix(engine): <WS-ID> — <summary>` → continue.
5. Never start a WS-E rung with red WS-A/B/C.

---

## 8. Definition of Done (by task type)

| Task type | DoD |
|-----------|-----|
| **Signal source** (A1, D2) | Unit test: target package/SDK/name → expected signal key + correct `DetectedVia`. |
| **Archetype** (B1, B2) | All 8 framework repos unchanged; new App fixture classifies App; guard fixture stays App. |
| **Render** (A3, A4) | Rendered markdown shows the group header + line; golden/snapshot updated intentionally. |
| **New entry shape** (E*) | §3.3 checklist complete; `entry-kind-present` expectation green; trace from one entry non-empty (F2). |
| **Refactor** (D*) | Behaviour-preserving: full gate green with **zero** output diffs vs pre-refactor goldens. |
| **Quality** (F3, G-a) | New assertion added; gate green. |

Every task: **0 build warnings**, fast tests green, no regression in the 39 eval checks.

---

## 9. Risk register

| Risk | Mitigation |
|------|-----------|
| B1 rewrite flips a framework repo to App | Lock current 8 archetypes as expectations *before* editing `Detect`; they must stay green. |
| Self-name tightening (B2) drops a legit framework signal | Keep package/SDK sources (A1) as the primary path; self-name is only the "is-the-framework" tie-break. |
| Catalog refactor (D) changes output | D is behaviour-preserving — gate on byte-identical goldens; land A/B first so D has a stable baseline. |
| New extractors slow huge repos | Signal-gated + shared parse cache; `entry-kind` perf check via `devcontext-bench`. |
| Engine re-explodes as rungs accrue | §3.3 checklist is the PR gate — a rung that edits core files beyond one registration line is rejected. |
| Indirect-wiring quality silently regresses | F3 golden seam-count guard. |

---

## 10. One-glance task index (for tracking)

```
WS-A Fix broken three      A1 pkg-signals · A2(=B1) · A3 lib-render · A4 labels · A5 eShop-validate
WS-B Archetype model       B1 provenance+prod-entry · B2 tighten self-name
WS-C Eval coverage         C1 entry-kind check · C2 consumer fixtures · C3 archetype guard
WS-D Catalog refactor      D1 catalog · D2 dep-extractor · D3 archetype · D4 labels
WS-E Coverage ladder       E1 RazorPages · E2 Orleans grains · E3 desktop(WinForms/MAUI/Avalonia)
                           E4 messaging(NServiceBus/ASB/Wolverine) · E5 AWS Lambda · E6 GraphQL · E7 CLI
WS-F Graph quality         F1 target-resolve · F2 non-empty trace · F3 wiring regression guard
WS-G Library quality       G-a base-type/attribute seats (FluentValidation, xUnit)
```
