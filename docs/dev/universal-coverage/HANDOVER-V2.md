# Handover — feat/universal-coverage-v2

> Branch: `feat/universal-coverage-v2` (off `develop` @ `4f59cef`)
> Authored: 2026-07-01 · 8 commits · Gate: GREEN on all

---

## 1. What was delivered

### WS-A/B — Consumer-app signals + provenance archetype (1 commit)
- **A1**: `Grpc.AspNetCore`, `Microsoft.AspNetCore.SignalR`, `Microsoft.Azure.Functions.Worker` added to `PackageSignalMap`; `Microsoft.NET.Sdk.Functions` SDK detection added.
- **B1**: `ArchetypeDetector.IsSelfSourcedFrameworkSignal()` replaces blind `Has(sig)` — gates on `DetectedVia ∈ {ProjectName, ProjectReference}` so consumer apps stay App.
- **A3**: `LibrarySurfaceRenderer` optionally renders `map.Entries` block.
- **A4**: `MapRenderer.GroupLabel` returns `"gRPC"`, `"SignalR"`, `"Functions"`, `"Blazor"`.
- **B2**: `TryMatchSignalFromProjectName` uses `StartsWith` only — no `Contains`. `ProjectNameSignalMap` runs FIRST; its keys tracked in `selfSourcedKeys` to suppress `PackageReference` signals for the same key. Sample/test paths filtered from all signal registration paths.
- **A5**: Validated on eShop — `Basket.BasketService` renders as gRPC entry with `→ BasketService`.

### WS-C — Eval coverage (1 commit)
- **C1**: New `entry-kind-present` check type — validates detection type in JSON `$.detections` AND rendered group header in markdown.
- **C2**: SignalR and Functions consumer-app eval fixtures + expectations.
- **C3**: Archetype guard fixture — `Company.Functions` project name stays App (B2 proof).
- Eval harness augmented with all new extractors (SignalR, gRPC, Functions, Blazor, Desktop, RazorPages).
- Removed broad `"Functions"` pattern from `ProjectNameSignalMap` (`"Azure.Functions"` alone suffices).

### WS-D — EntrySurfaceCatalog (1 commit)
- **D1**: `EntrySurfaceDescriptor` record (SignalKey, Kind, RenderLabel, Role, Packages, SdkHints, SelfNamePatterns) + `EntrySurfaceCatalog.All` with 37 descriptors.
- **D2**: `DependencyExtractor` derives `PackageSignalMap`, `ProjectNameSignalMap`, `SdkSignalMap` from catalog projections. SDK detection uses catalog lookup.
- **D3**: `ArchetypeDetector.AppEntryKinds` and `LibraryFrameworkSignals` derived from catalog. `LibraryFrameworkSignals` includes any signal with `SelfNamePatterns` (covers both `FrameworkLibrary` role and app-entry surfaces that become framework indicators when self-sourced).
- **D4**: `MapRenderer.GroupLabel` reads `RenderLabel` from catalog via `KindLabels` dictionary.

### WS-E — Coverage ladder (2 commits)
- **E1**: Razor Pages — `razor-pages` signal (emitted when `PageModel` base types found), `RazorPagesExtractor` scans `.cshtml` for `@page` route, `.cs` for `OnGet`/`OnPost` handlers. `.cshtml` files added to `FileTreeExtractor`. Reuses `EndpointDetection` + `HttpEndpoint` + `HttpEntryPointBuilder`.
- **E2**: Orleans grains — `GrainMethod` kind, `GrainDetection` record, `OrleansGrainExtractor` (scans `Grain`/`Grain<T>` base + `IGrainWith*` interfaces), `OrleansGrainEntryPointBuilder`.
- **E3**: Desktop breadth — `Form` (WinForms), `ContentPage`/`Shell` (MAUI) added to existing `DesktopEntryExtractor`. Reuses `UiEntry` + `DesktopEntryPointBuilder`.
- **E4**: Messaging breadth — `NServiceBusExtractor` (`IHandleMessages<T>`) and `WolverineExtractor` (`*Handler` convention). Reuse `MessageConsumerDetection` + `MessageConsumerEntryBuilder`. New signals: `wolverine`, `azure-servicebus`.
- **E5**: AWS Lambda — `AwsLambdaExtractor` (`[LambdaFunction]`, `ILambdaFunction`). Reuses `FunctionEntryDetection` + `FunctionsEntryPointBuilder`. New signal: `aws-lambda`.
- **E6**: GraphQL resolvers — `GraphQlField` kind, `GraphQlFieldDetection`, `GraphQlResolverExtractor`, `GraphQlEntryPointBuilder`. `graphql` signal promoted to AppEntry with packages.
- **E7**: CLI commands — `CliCommand` kind, `CliCommandDetection`, `CliCommandExtractor`, `CliCommandEntryPointBuilder`. New signal: `cli-commands`.

### WS-F — Graph quality + hygiene (2 commits)
- **F1**: All entry builders now set `HandlerNode` so `→ target` resolves. Fixed `WorkerEntryPointBuilder`, `DomainEventHandlerEntryBuilder`, `MessageConsumerEntryBuilder` (were missing `HandlerNode` and/or Calls edges).
- **F3**: Golden indirect-wiring regression guard — `GraphStats.Compute` on eShop asserts minima: Calls≥80, Handles≥4, Resolves≥20, Sends≥4, Raises≥2, WrappedBy≥4, entries-with-target≥30.
- **Hygiene**: Removed dead `BlazorPage` enum (Blazor uses `HttpEndpoint`), removed unused `AppEntryDescriptors` field, merged duplicate GraphQL catalog descriptor, removed 274 lines dead code from `GraphBuilder.cs`, DRY'd eval harness to share `TestPipeline.Build()`.

---

## 2. Current state

| Metric | Before | After |
|--------|--------|-------|
| Entry point kinds | 11 | 13 |
| Extractors (specific) | 16 | 23 |
| Catalog descriptors | 0 | 36 |
| Eval tests | 39 | 44 |
| Dead code removed | 0 | 274 lines |
| Lines added/removed | — | +1493 / -435 |

### Entry points detected (13 kinds)
`HttpEndpoint`, `MessageConsumer`, `HostedService`, `ScheduledJob`, `DomainEventHandler`, `PublicApi`, `UiEntry`, `GrpcService`, `SignalRHub`, `FunctionEntry`, `GrainMethod`, `GraphQlField`, `CliCommand`

### Extractors (23 specific)
`Endpoint` · `ControllerAction` · `MediatR` · `EfCore` · `EventBus` · `CallGraph` · `SourceBody` · `IndirectWiring` · `Aspire` · `ProgramCsFlow` · `DiRegistration` · `DesktopEntry` · `BlazorEntry` · `GrpcService` · `SignalRHub` · `AzureFunctions` · `RazorPages` · `OrleansGrain` · `NServiceBus` · `Wolverine` · `AwsLambda` · `GraphQlResolver` · `CliCommand`

### Gate
`eval/gates.ps1` exit 0 — build 0-warn, 303 fast + 64 desktop + 12 server + 44 eval, CLI matrix 5/5.

---

## 3. Open issue: CI timeout on DevContext2 itself

The CLI times out (>600s) when run against our own solution (24+ projects). Likely causes:

1. **New extractors scan all source files** — 23 specific extractors each iterate `AllSourceFiles`, multiplying the per-file cost. Many do full Roslyn parse (`GetSyntaxTreeAsync`).
2. **`FileTreeExtractor` now includes `.cshtml` and `.razor` files** — more source files to scan.
3. **No early termination for no-signal repos** — extractors still iterate files even when their signal is absent (the `ShouldRun` check gates the extractor entirely, but within-extract the loop still runs).

### Immediate mitigation options
- Profile with `devcontext-bench` skill to identify the hot extractors
- Add early bail-out: if signal is absent, skip the file iteration entirely (already done via `ShouldRun`, but verify)
- Reduce redundant Roslyn parsing (share parse cache across extractors — verify it works)
- Cap per-extractor file count or add `--fast` mode that skips low-priority extractors
- Run eShop (564 files in 5.8s) as performance baseline vs DevContext2

---

## 4. What remains (MASTER-PLAN §4)

| Item | Description | Priority |
|------|-------------|----------|
| **Perf profile** | Profile and fix the timeout on real repos | HIGH |
| **WS-G-a** | Library surface: FluentValidation `AbstractValidator<T>` seat, xUnit `[Fact]`/`[Theory]` | LOW |
| **Merge to develop** | Open PR, review, squash-merge | — |

### Out of scope (separate branches)
- MCP server (Phase 8)
- Persistent index (Phase 9)
- Browse UI interactive redo (Phase 7)
- HotChocolate large-repo scoping
- DntSite local-variable tracking
- Roslyn SemanticModel upgrade for body-scan seams

---

## 5. Resume instructions

```powershell
git checkout feat/universal-coverage-v2   # or develop after merge
git log --oneline -10                      # see state

# Gate
dotnet build DevContext.slnx               # must be 0 warnings
dotnet test DevContext.slnx --filter "Category!=Eval&Category!=CliSmoke"
powershell -File eval/gates.ps1

# Profile the performance issue
# Use devcontext-bench skill or run:
dotnet run --project src/DevContext.Cli -- analyze C:\Code\DevContext2 --format json --stats
```

## Architecture for adding new shapes

```
1. Add EntrySurfaceDescriptor to EntrySurfaceCatalog.All   (data: signal, packages, role, label)
2. Add EntryPointKind value (EntryPoint.cs)                 (if new kind)
3. Add Detection record + [JsonDerivedType] (Detections.cs)
4. Write extractor : IDiscoveryExtractor                    (ShouldRun gates on SignalKey)
5. Write builder : IEntryPointBuilder; register in _entryBuilders[]
6. Add eval expectation with render-level entry-kind check
→ Zero edits to GraphBuilder, DiscoveryPipeline, ArchetypeDetector, MapRenderer
```
