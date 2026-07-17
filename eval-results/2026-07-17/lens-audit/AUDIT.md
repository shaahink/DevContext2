# Lens Audit — unseen-repo round (2026-07-17)

**Question:** are we "the go-to lens for any .NET repo" yet — across engine, CLI, MCP, and desktop UI?

**Verdict: NO — the lens is excellent for modern ASP.NET-Core app repos and (when the archetype
fires) libraries, but it fails closed on 4 of 8 unseen repo shapes.** The Tapestry-era defect class
(detect≠render) is fixed; its successor is **archetype≠reality**: misclassify the repo shape and
every downstream surface renders a confidently wrong or empty lens. The desktop has **no library
mode at all**. Wall time is healthy except the known big-repo wall.

## Method

8 unseen repos (never in eval-repos/, clone cache, or prior audits), spanning the ".NET repo" space,
shallow-cloned at HEAD and driven blind through all four surfaces on `audit/library-round`
(= develop `9ee401a`):

| Repo | Shape | Size (cs files) | analyze wall |
|------|-------|-----------------|--------------|
| Newtonsoft.Json | classic library | 945 | 30s |
| refit | source-gen library | 812 | 25s |
| StackExchange.Redis | async client library | 561 | 30s |
| wolverine | messaging framework + samples (215 csproj) | 4053 | 58s |
| GitVersion | CLI tool (3 solution trees + Cake build) | 663 | 18s |
| dotnet-podcasts | multi-surface app (MAUI+Blazor+API+SignalR+workers) | 192 | 8s |
| ScreenToGif | WPF desktop app | 648 | 20s |
| bitwarden-server | large multi-service app (64 csproj, Aspire) | 4927 | 209s |

Captures: `map.md` + stdout per repo (this dir), traces (refit, podcasts, ScreenToGif,
bitwarden ×2), MCP 22-tool transcript (`dotnet-podcasts/mcp-transcript.txt`), UI screenshots ×13
(`ui/`), against ground-truth code reads.

## Per-repo scorecard

| Repo | Archetype | Style | Entries | Render | Net |
|------|-----------|-------|---------|--------|-----|
| refit | ✅ Library | ✅ (CLI suppresses) | ✅ 88 public types | ✅ ENTRY API/ABSTRACTIONS/GENERATORS/PUBLIC SURFACE — the product vision, working | **PASS (CLI); FAIL (UI — no library mode)** |
| dotnet-podcasts | ✅ App | ⚠ CleanArchitecture ok; "MediatR" is false (hand-rolled IRequestHandler, zero MediatR refs) | ⚠ 23 (HTTP+Blazor+worker; SignalR hub MISSED, MAUI invisible) | ⚠ bare `GET /` ×5 (MapGroup prefixes dropped) | **PARTIAL** |
| bitwarden-server | ✅ App | ✅ Microservices high (Aspire evidence) | ✅ 662 HTTP + 7 hosted; SignalR NotificationsHub MISSED | ⚠ per-service style: all 17 Unknown; EVENT WIRING: 1 event on an events-heavy repo | **PARTIAL** |
| ScreenToGif | ✅ Desktop | ⚠ Unknown (no MVVM/desktop style rung) | ✅ 35 UI entries | ⚠ DESKTOP VIEW duplicates ENTRY POINTS; `X → X` self-arrows; traces are entry-only | **PARTIAL** |
| Newtonsoft.Json | ❌ App (is THE library) | ❌ Unknown | ❌ 0 | ❌ 19-line dead map, no public surface | **FAIL** |
| StackExchange.Redis | ❌ App (is a library) | ❌ MinimalApi (from toys/) | ❌ 0 | ❌ no public surface; `.github`/`docs`/`docker` holder csproj in topology | **FAIL** |
| GitVersion | ❌ App w/ 0 entries (is a CLI tool) | ❌ Unknown | ❌ 0 (CLI detection is package-gated; Main() missed) | ❌ empty map; Cake build projects as "services"; `@(PackageReference)` as a package | **FAIL** |
| wolverine | ❌ App (is a framework) | ❌ CleanArchitecture (from samples) | ⚠ sample entries | ❌ ~80-row per-service table of samples/test hosts; `Messages` ×6 dup rows | **FAIL** |

## Findings ledger

### A. Archetype & render honesty — the failure class of this round
- **A1** `ArchetypeDetector.Detect` (`ArchetypeDetector.cs:89-95`): `allExeAreAuxiliary` requires the
  exe to reference the library **directly**. Newtonsoft.Json.TestConsole references only
  Newtonsoft.Json.Tests (transitively the lib) → flips the repo to App. Transitive/aux-exe blindness.
- **A2** StackExchange.Redis: `toys/` hosts aren't sample-pathed → entries flip archetype to App and
  style to MinimalApi. Sample-segment list lacks `toys` (and needs an "aux host" notion beyond paths).
- **A3** GitVersion: CLI-tool repos have no archetype/render story: CliCommand detection is gated on
  Spectre/System.CommandLine packages; a plain `Main()` exe yields 0 entries → empty App map. Cake
  Frosting build projects (`build/…/{artifacts,docker,docs}.csproj`) render as services — NoiseFilter
  has no build-tooling rung.
- **A4** wolverine: descriptor has `Packages:["WolverineFx"]` but **no SelfNamePatterns** — project
  names are `Wolverine.*`, nuget id differs → self-source Library rule can't fire. Whole catalog needs
  a nuget-id≠project-name audit. Per-service table comes from runnable-service inference that never
  applies `NoiseFilter`/`IsSamplePath` → 80 sample rows.
- **A5** "App with 0 entries" renders a dead map (Newtonsoft 209 tokens, GitVersion 485). No render
  backstop: with 0 entries and a public surface present, render the library surface (or Main-anchored
  console view) instead of nothing.

### B. Entry-surface gaps (detect)
- **B1** **In-framework SignalR is invisible** — `MapHub<T>` with no package reference (SignalR is in
  the shared framework) never fires the signal; hub entries gated off. Hit BOTH podcasts
  (`ListenTogetherHub`, Program.cs:19) and bitwarden (`NotificationsHub`). The signalr-app fixture
  passes only because it references the client package.
- **B2** **MAUI has no catalog descriptor** and per-service MAUI rung probes packages only —
  `<UseMaui>true</UseMaui>` (SDK-provided, .NET 7+) is missed; podcasts' two MAUI apps read Unknown,
  0 entries (pages/shell invisible).
- **B3** **MapGroup route prefixes are dropped**: podcasts renders five different endpoints as bare
  `GET /` (FeedsApi/ShowsApi/CategoriesApi groups). Corrupts map, top flows, UI lists, Studio scope
  picker, and MCP entry addressing.
- **B4** No generic Main-method/console entry fallback (see A3).
- **B5** **Queue-based messaging isn't an event seam**: podcasts' real cross-process path
  (FeedsApi.CreateFeed → Azure Storage Queue → Ingestion.Worker) invisible — event board says "No
  events detected"; bitwarden (ASB/RabbitMQ/EventGrid-heavy) shows "1 integration event, no consumer".
- **B6** Style detector brands hand-rolled `IRequestHandler<,>` as "MediatR (CQRS)" — name-only match;
  podcasts has zero MediatR references. Should say "CQRS (hand-rolled mediator)".

### C. Graph & trace depth
- **C1** `.razor` `@code` blocks aren't parsed into the call graph → all Blazor UiEntries have
  score 0 / reach 0 / no out-edges; trace = "no out-edges resolved"; get_context pack is empty.
- **C2** XAML code-behind: WPF UI entries trace to themselves only (ScreenToGif `ExportPanel` →
  `ExportPanel`, 3 lines) — event handlers/commands not wired into the trace spine.
- **C3** **Member↔Type degree fragmentation**: Type nodes carry 0 in/out while member nodes hold the
  edges → MCP `node`/`neighbors`/`usages`/`impact` on a type name return empty (PodcastService:
  impact up = 0 while it IS the target of GET /landing). Navigation dead-ends for agents.
  (Related standing debt: conductor-DEBT SymbolTable member indexing.)
- **C4** Per-service style ladder (`ArchitectureStyleDetector.DetectPerServiceStyles`): 
  `ownsHttpEndpoints` used only negatively — **no "owns endpoints → Web API/MVC" rung**; `.API`
  suffix check requires the dot (bitwarden's `Api` fails); no SignalR-host/IdentityServer rungs; no
  desktop MVVM rung. Result: bitwarden 17/17 Unknown, ScreenToGif Unknown.
- **C5** DI provenance picks an arbitrary registration cross-service (bitwarden Api trace resolves
  `CurrentContext` to `bitwarden_license/src/Sso/Startup.cs:46`); prefer the focus host's own
  registration, else list all.
- **C6** Entry target attribution glitches: `GET / → ShowClient.CheckLink` (ShowsApi.cs:13 is
  GetAllShows), `PUT /{id} → IFeedClient` (an interface as target).

### D. Perf & session UX
- **D1** **No persisted graph for the CLI**: every `--focus` re-analyzes (bitwarden: 207s map + 152s
  per trace). The named lever (persist/reuse merged compilation + graph cache) is a UX defect now,
  not just a bench number. MCP/server sessions already amortize (analyze 8.8s → all tools 0.0–0.2s).
- **D2** Wall times otherwise healthy: 8–58s for ≤215-csproj repos; bitwarden 209s = the SemanticLite
  wall at 4927 files.
- **D3** Trace stats admit `Tokens ~24774 (budget 8000)` on a god-class type focus — budget not
  enforced/explained on the CLI trace path.

### E. Hygiene
- **E1** `<PackageReference Update="@(PackageReference)">` ingested as a package named
  `@(PackageReference)` (GitVersion) — skip Update-only/MSBuild-expression refs.
- **E2** Holder/utility csproj (`.github.csproj`, `docs.csproj`, `docker.csproj`) render as topology
  nodes; topology also skips the test-path filter (`RedisConfigs` under tests/).
- **E3** Duplicate project names render as indistinguishable rows (`Messages` ×6, `AppHost` ×2,
  `GitVersion.Core` twice in topology) — need dedup/disambiguation.
- **E4** Trace NOTE recommends `--profile debug` — flag doesn't exist in the CLI.
- **E5** STACK line dumps raw TFM matrices (`net46;net40;net35;net20;…` ×4 projects) — unreadable.

### F. Desktop UI
- **F1** **No library mode**: refit session → home shows "0 entries / No entry data available";
  Explore shows the *unanalyzed* empty-state copy ("Analyze a repo to list its entry points") on an
  analyzed repo. The CLI's ENTRY API/ABSTRACTIONS/GENERATORS/PUBLIC SURFACE has no UI equivalent.
  Also stamps `Library · ControllerBased` — a style the CLI rightly suppresses for libraries.
- **F2** **Graph canvases don't fit/center content**: home hero + Atlas diagram clip node labels at
  the canvas edge, spill nodes off-viewport, leave 60% of the canvas empty (podcasts 13-node graph,
  refit). This is the single biggest visual-quality defect.
- **F3** Graph nodes are plain squares — no per-kind iconography on canvases (rail/entry-list icons
  are fine).
- **F4** Session naming inconsistent: home says `NetPodcast.Services.sln` / `Refit.slnx`, other pages
  + tab say the directory name.
- **F5** MCP page live feed under "agents only" shows the app's own RPCs (`GetFlowIndex`, `GetStats`)
  labeled `agent` — feed origin mislabel (suspected; verify).
- **F6** Route truncation + B3 make entry lists indistinguishable (three `/listen-toge…` rows; four
  `GET /` rows in Studio scope picker).

### G. MCP
- **G1** `get_context` on a degenerate focus returned 166/6000 tokens (2.8% fill) with no
  explanation or alternative suggestion — the ≥85% fill promise silently fails.
- **G2** C3 makes `neighbors`/`usages`/`impact` dead ends on type queries.
- **G3** Otherwise strong: honest ambiguity in `resolve`, method disclosure (`config`, `tests_for`),
  clean staleness API, all 24 tools sub-second after analyze.

### H. CI/CD & cross-platform delivery
- **H1** Engine CI runs on `windows-latest` only; app checks on `ubuntu-latest` only. The engine is
  never built/tested on Linux/macOS anywhere, while the CLI ships to NuGet as a (nominally
  cross-platform) net10.0 tool. Path-separator/casing behavior is untested off-Windows.
- **H2** Release: NSIS+MSI only. Tauri supports .dmg/.deb/AppImage; the server sidecar publish is
  framework-dependent win-x64 today — per-RID publish needed for mac/linux bundles.
- **H3** Installer version comes from tauri.conf.json `0.1.0`, not the release tag (known nicety).
- **H4** README §Platform support states the Windows-only truth honestly — the gap is delivery, not
  honesty.

## What's genuinely good (don't break it)
- Modern app-repo maps: bitwarden Microservices-high via Aspire evidence, 662 resolved routes with
  auth attributes; podcasts CleanArchitecture w/ Blazor routes + workers; eval poles unchanged.
- Trace spine quality on C# apps: interface→`di` implementation hops with `[×2 impls]` honesty
  (EF + Dapper), named fan-out cuts, salient lines, verified/approx provenance.
- MCP ergonomics (G3); insights honesty (auth surface, config-without-defaults, tier words, 47%
  targeted admission); wall time on small/mid repos; solution-pick shown in UI header.

## Priority (impact-ordered)
1. **A1–A5 archetype/render honesty** — 4/8 repos fail here; it's the lens claim itself.
2. **F1+F2 UI library mode + canvas fit** — the desktop contradicts the product on half the corpus.
3. **B1–B3** SignalR-in-framework, MAUI, MapGroup prefixes — real apps hit all three today.
4. **C3+C4** type-node degrees (MCP navigation) + per-service style rungs (Unknown epidemic).
5. **D1** persist/reuse compilation + CLI session reuse — turns the 207s×N workflow into 207s+ε.
6. The rest: C1/C2 (razor/XAML depth), B5/B6, C5/C6, E1–E5, F3–F6, G1, H1–H3.
