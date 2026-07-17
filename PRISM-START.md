# Prism — Phase Tracker (OPEN at D1, 2026-07-17)

**Phase plan:** `docs/dev/briefs/proposal-prism.md` (5 big deliveries + standing QA cadence — read
§1 for the rules, §2 for delivery specs, §3 for the finding→delivery traceability table).
**Audit truth:** `eval-results/2026-07-17/lens-audit/AUDIT.md` (findings A–H) +
`EXPERIENCE-ADDENDUM.md` (I–N). **Predecessor:** `docs/dev/HANDOVER-TAPESTRY.md`.
Dogfood: `C:\Users\shahi\source\repos\run-aspnetcore-microservices\src` · second pole: `C:\code\shamshir`.

Branch scheme: `feat/prism-d<n>`, merged to develop per delivery on `GATE: PASS` + the delivery's
DoD. `feat/prism-d1` is cut from `audit/library-round` (= develop `9ee401a` + the 3 audit/proposal
docs commits) — the audit evidence rides into develop with the D1 merge.

## Handoff (running)

last: 2026-07-17 **D1 session 2 CLOSING — continue at D1.2c.** Ack: INBOX 2026-07-17 standing orders
(unchanged). Session 1 died mid-D1.2 (~07:22) leaving D1.2a+D1.2b UNCOMMITTED; session 2 picked it
up, validated + finished it, then found and fixed TWO unseen D1.1c regressions the cheap gates were
blind to. **VERIFIED this session: D1.2a, D1.2b, D1.2-fix (xunit), D1.2-fix2 (dogfood + pole-guards).**
Two commits: `d138f47` (D1.2ab + xunit) and the D1.2-fix2 commit below.
  - **D1.2a/b**: in-framework SignalR (podcasts + bitwarden hubs, package-free) + MAUI signal/rung;
    "pages/shell as UiEntries" proven by NEW `eval/fixtures/MauiSurface` (podcasts' own MAUI csprojs
    are in no .sln → out of SolutionScope, a scoping fact not a gap).
  - **D1.2-fix**: xunit `Library`→`App` ratchet, bisected to D1.1c, root = condition-blind
    `ResolveOutputType` (took `<OutputType>Exe</>` from a `<When ...tests>` block for every classlib).
  - **D1.2-fix2**: dogfood style `Microservices`→`CleanArchitecture` + ROUTES/gateway edges gone,
    bisected to D1.1c, root = D1.1c's two self-name guards each killing the gateway signal. Fixed by
    exempting `SurfaceRole.Gateway` (design rationale in the D1.2-fix2 row). **AND closed the gate
    hole that hid both**: dogfood + shamshir are now eval expectation files, and `gates.ps1` Step 3
    prints skipped repos so a missing pole can't masquerade as coverage.
next: **D1.2c** MapGroup prefixes → D1.2d queue seams → D1.2e branding → D1.2f target fidelity → D1.3
→ D1.4. Everything through D1.2b + both fixes is committed and green; this is a clean resume point.
**Disk-full RESOLVED (session 3, 2026-07-17 ~16:30): ~35 GB freed; the pending boundary citation
LANDED** — `eval/gates.ps1 -Scope engine -SkipMcpQa` re-run against `e8ca7cc`: **GATE: PASS**
(`prism-d1/d1.2-fix2/gates-engine-rerun.txt`). D1.2-fix2's evidence chain is complete.

**D1.2c is scoped**: podcasts does `var shows = app.MapGroup("/shows"); shows.MapShowsApi();` in
Program.cs while `ShowsApi.cs` does `group.MapGet("/", …)` on the RouteGroupBuilder parameter —
`EndpointExtractor.ExtractGroupPrefixes` already resolves group vars, but only WITHIN one file /
extension-method body, so the prefix never crosses the call boundary and every route renders bare
`GET /`. The fix is composing the caller's prefix into the callee extension method.

**LESSON (why the eval + poles must run before a row is marked VERIFIED):** the cheap gates (build +
fast tests + loom-guards) are BLIND to archetype/style regressions. The whole-cohort eval is the only
thing that caught xunit; nothing at all caught the dogfood pole for 4 checkpoints until a manual pole
re-check. Both are now closed: run `--filter "FullyQualifiedName~EvalExpectationTests&Category=Eval"`
(~10 min, detached, 43 repos incl. the two poles) at every checkpoint that touches
archetype/signal/evidence — not just the repos you think you changed — and read the SKIPPED-repos
list gates.ps1 Step 3 now prints (a skipped pole = a hole in the verdict).
gotchas (standing, carried from Tapestry): fast-suite "load-flake" **RESOLVED at D1.1a**: it is
`McpQaGateTests.McpQaHarness_Passes_Against_Dogfood` losing its known shared-state race when run
INSIDE the parallel suite — gates.ps1 Step 2 already excludes it (`Category!=Eval&Category!=
CliSmoke&Category!=McpQa`) and runs it serially as Step 2b. Cheap-gate fast tests must use the
battery's filter + a serial `Category=McpQa` run (this session does from D1.1a on). Truth-gate
test host can still crash under heavy churn — quiet re-run cures. Orphaned DevContext.Server
after test runs locks build DLLs — sweep with `start-dev-bg.ps1 -Kill`. PS 5.1 × UTF-8 em-dashes in detached
scripts (keep battery scripts ASCII); dogfood PRE-EXISTING mods stand — never restore; never
build/test in a worktree while its battery runs; rebuild the CLI after any Core edit;
absolute CLI paths only.

## Operating model (REVISED 2026-07-17 at D1 open — supersedes proposal §1 QA cadence, owner call)

- **Orchestrated phase.** An orchestrator session spawns one visible Claude Code session per
  delivery (models per proposal §1 table), watches via tracker/git/evidence (never transcripts),
  and closes sessions when their delivery is done. **Channel discipline: the orchestrator writes
  `PRISM-INBOX.md` ONLY; delivery sessions write this tracker + code.** Delivery sessions re-read
  `PRISM-INBOX.md` at every checkpoint boundary and treat its entries as orchestrator instructions.
- **QA deferred to phase end (owner call).** NO per-delivery full battery, NO octet re-runs, NO
  QA-back between deliveries. Per-commit cheap gates REMAIN mandatory: `dotnet build` 0w/0e +
  fast tests (`--filter "Category!=Eval"`) + `scripts/loom-guards.ps1`. One massive phase-end QA
  (D5): full battery + octet harness + insight-validity + poles drift diff + clean-clone.
  Exception: a delivery whose DoD *is* an octet claim (D1) runs `eval/lens-audit.ps1 octet` once
  at its close as the DoD proof.
- **Branch train, single merge.** `feat/prism-d1 → d2 → d3 → d4 → d5` stacked (each off the
  previous tip, like Tapestry T4–T8). ONE merge to develop after phase-end QA passes, with owner
  sign-off. develop is not touched mid-phase.
- **When a session's context runs low:** finish the current checkpoint, update the handoff block
  (`D<n> SESSION <k> CLOSED — continue at D<n>.<x>`), commit, stop. The orchestrator chains a
  fresh session from tracker state. When a delivery's checkpoints are all VERIFIED: write
  `D<n> DELIVERY CLOSED` in the handoff, commit, stop.
- Evidence per delivery under `eval-results/<date>/prism-d<n>/`. Truth ratchets only tighten;
  Tapestry poles byte-identical unless a DoD says otherwise; **no new bare `catch` in Core**
  (loom-guards ban lands in D1.0).

## The octet (pinned)

Stable home since D1.0a: `eval-repos/<name>` (gitignored, alongside the expectation cohort),
copied from the audit scratchpad clones, HEADs verified against the pins below
(`prism-d1/d1.0a-octet-home.txt`). Rows + re-clone recipe pinned in `eval/README.md` §Octet.

| Repo | Pinned SHA | Origin | Intended verdict (aspirational until D1) |
|------|-----------|--------|------------------------------------------|
| Newtonsoft.Json | `4f73e74372445108d2c1bda37b36e6f5e43402e0` | JamesNK/Newtonsoft.Json | Library (aux console ≠ App) |
| refit | `71634f2c5d0845c311b1cf4f4bb512437fe86fb5` | reactiveui/refit | Library (already PASS on CLI) |
| StackExchange.Redis | `0b03ed1d12a6a783873a44cd1f6fad3acf54395f` | StackExchange/StackExchange.Redis | Library (toys/ = aux hosts) |
| wolverine | `7019b7d1b4520f84f90adbc6d407998c85e5e750` | JasperFx/wolverine | Framework-library (SelfNamePatterns) |
| GitVersion | `6476e5c478ec1b56a45914b3af4f6edcfd20deb0` | GitTools/GitVersion | CliTool (new archetype) |
| dotnet-podcasts | `5ee8be2990b81eb681bbd100875c263aaa5ab68a` | microsoft/dotnet-podcasts | App: hub entry + MAUI present, grouped routes |
| ScreenToGif | `27a49c3be69486f2db964290f4f2274e790fb687` | NickeManarin/ScreenToGif | Desktop, MVVM style rung |
| bitwarden-server | `3e79593151787eb94853cb29420530d32f9b543c` | bitwarden/server | App: per-service styles ≤2/17 Unknown, hub entry |

## Delivery table

| Delivery | Theme | Findings | Status |
|----------|-------|----------|--------|
| **D1** | Archetype truth + entry surfaces + style rungs (engine) | A1–A5, B1–B6, C4, C6, E1–E5 | **IN PROGRESS** (opened 2026-07-17) |
| D2 | Graph depth + self-health + insight validity (engine) | C1–C3, C5, I1–I2, J1, J3, G1-dep, D3 | TODO |
| D3 | Cache resurrection + living waterfall + compiler lever | J2, K1–K2, L2, L7, D1–D2, perf lever | TODO |
| D4 | Visual intelligence + library workbench + Studio/nav (app) | F1–F6, L1, L3–L6, M | TODO |
| D5 | MCP polish + cross-platform + final hardening | G1, H1–H3, phase QA | TODO |

## D1 checkpoint table

Status ∈ TODO · IN PROGRESS · DONE · BLOCKED · VERIFIED. Evidence under
`eval-results/2026-07-17/prism-d1/`. A checkpoint without a fresh artifact is not DONE.

### D1.0 — Harness first (it gates everything)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.0a | Octet SHAs pinned in `eval/README.md` + stable clone home | VERIFIED | c19e42e | prism-d1/d1.0a-octet-home.txt — 8/8 SHAs match pins |
| D1.0b | Aspirational expectation rows for intended verdicts (table above) | VERIFIED | d80be12 | prism-d1/d1.0b-octet-expectations-validation.txt — 8 new expectation files, in-process eval 8/8 green (expected rows pass today incl. bitwarden <480s; aspirational rows = the D1 contract) |
| D1.0c | `eval/lens-audit.ps1 <repo\|octet>`: timed analyze → captures → MCP drive → FAIL probes (map-tokens ≪ repo size; Unknown+0-entries; sample rows in per-service; wall-time vs baseline) | VERIFIED | 6db193a | prism-d1/lens-run-smoke/ — podcasts PASS (wall 7.9s, MCP drive PASS incl. 23-entry inventory); GitVersion FAIL(2) = P2 archetype + P3 unknown-zero fire as designed (17.6s/484 tokens ≈ audit 18s/485) |
| D1.0d | Bare-`catch` ban in `scripts/loom-guards.ps1` (Core; existing swallows grandfathered until D2) | VERIFIED | 6e66dc5 | census 30 swallows (16 empty + 14 comment-only) / 11 files grandfathered as per-file MAX; negative test fired (+1 scratch bare catch → BANNED, exit 1); clean run PASS |

### D1.1 — Archetype & render honesty (A1–A5, E2)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.1a | A1 transitive aux-exe references (Newtonsoft TestConsole → Tests → lib) | VERIFIED | (D1.1a commit) | prism-d1/d1.1a/ — Newtonsoft.Json flips App→Library, dead 209-token map → 2355-token LIBRARY render (ENTRY API/ABSTRACTIONS/PUBLIC SURFACE), lens-audit PASS, wall 30.6s ≈ baseline; unit test (transitive chain through test project); 5 eval rows flipped expected, in-process green |
| D1.1b | A2/A3 `toys`/build-tooling NoiseFilter rungs; holder csproj (`.github`/`docs`/`docker`) excluded everywhere; topology applies per-service filters (E2) | VERIFIED | (D1.1b commit) | prism-d1/d1.1b-2/ — SE.Redis flips App→Library (1955-token LIBRARY render, lens PASS, holders+toys+MinimalApi all gone); GitVersion loses all 8 Cake build rows (transitive closure artifacts/publish/release→common); 6 eval rows flipped expected + both green in-process; 32 unit tests incl. holder/Traversal/closure cases |
| D1.1c | A4 catalog self-name audit — SelfNamePatterns wherever nuget id ≠ project names, wolverine first; runnable-service inference honors NoiseFilter unless SamplesAreTheProduct | VERIFIED | (D1.1c commit) | prism-d1/d1.1c/ — wolverine flips App→Library (6272-token LIBRARY, 1322 public types, 0 sample rows, lens PASS); 18 catalog descriptors gain SelfNamePatterns; matcher hardened: name-boundary (kills WolverineDemo/SerilogHelpers/OrleansVoting false matches) + non-runnable self-source guard; per-service honors sample filter w/ T8 waiver; 4 eval rows flipped + green; 8 new unit tests. **SHIPPED TWO UNSEEN REGRESSIONS, both bisected to this commit, both fixed later: (1) xunit App←Library — the runnable guard rested on `ProjectInfo.OutputType`, itself false evidence (D1.2-fix); (2) dogfood style Microservices→CleanArchitecture, ROUTES + gateway edges gone — the two new guards each killed the gateway self-source (D1.2-fix2). Both survived 4 green checkpoints because no pole/xunit-style regression was in the eval cohort — the lesson that drove the pole-guard work.** |
| D1.1d | A3/B4 `Archetype.CliTool`: Exe + no web surfaces + PackAsTool/parser evidence → command-surface render; plain `Main()` becomes an entry | VERIFIED | (D1.1d commit) | prism-d1/d1.1d/ — GitVersion flips App→CliTool: CLI TOOL header + COMMAND SURFACE + `CLI (1)` Main entry (Program.cs:3 provenance), lens PASS 19.9s; IsToolPackaged (PackAsTool/ToolCommandName incl. conditional) + parser-package evidence; Main fallback in CliCommandExtractor (reformed in place); 4 unit tests (bitwarden-utility + Newtonsoft-aux negatives); 2 eval rows flipped (cli-entries type corrected), green |
| D1.1e | A5 render backstop — no dead maps: 0 entries + public surface ⇒ library sections; + Main ⇒ console view; harness FAILs any <~400-token map on a >100-file repo | VERIFIED | (D1.1e commit) | ConsoleBackstop fixture (eval/fixtures) + console-backstop.json 5/5 green in-process — App+0-entries renders NOTE + ENTRY API/ABSTRACTIONS/PUBLIC SURFACE (MapBuilder backstop Surface, renderer sections reused); no-surface branch renders CONSOLE VIEW of production exes; harness probe live since D1.0c; Newtonsoft regression check byte-stable (2355 tokens) |

### D1.2 — Entry surfaces 2026 (B1–B6, C6)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.2-fix | **xunit regression (unplanned, found by the D1.2b eval sweep)** — `Library`→`App` + console-view render | VERIFIED | d138f47 | The whole-cohort eval caught it (41/42 green, xunit red on the `archetype-library` RATCHET — a row green since long before Prism). Bisected across the 5 D1.1 commits: clean at D1.1a/b, red from **D1.1c** on. Chain: xunit's `src/Directory.Build.props` sets `<OutputType>Exe</OutputType>` inside `<Choose><When Condition="…EndsWith('.tests')">`, but `ResolveOutputType`'s ANCESTOR walk took any `<OutputType>` descendant and ignored the condition → every xunit CLASSLIB read as an exe → D1.1c's new runnable-guard skipped them all → the self-sourced `testing` signal vanished → no early Library return → `nonExe.Count == 0` → `App` ("pure executable(s)"), and D1.1e's backstop then rendered a CONSOLE VIEW instead of LIBRARY. **Fixed at the root (the false evidence), not the guard** — a conditioned ANCESTOR value applies to a subset of projects and we cannot evaluate MSBuild conditions, so it is not evidence for this project; the csproj's OWN conditioned value is still honoured (eShop's ClientApp relies on it). xunit → `Library`/`signals=testing`/981 tokens, byte-identical to the D1.0d baseline. 2 regression tests (conditioned-ancestor + xunit self-name rows) |
| D1.2a | B1 in-framework SignalR: `MapHub<T>`/`: Hub` evidence, package-free (podcasts ListenTogetherHub + bitwarden NotificationsHub) | VERIFIED | d138f47 | prism-d1/d1.2a/ — podcasts renders `SignalR (1) ListenTogetherHub (8 methods: OpenRoom, JoinRoom, LeaveRoom)` with NO SignalR package; matcher takes the base AS WRITTEN (bare `Hub`/`Hub<T>` or SignalR-qualified `Microsoft.AspNetCore.SignalR.Hub` — the form BOTH podcasts and bitwarden use, and exactly what the audit missed), plus `MapHub<T>` file evidence; qualifier must end in `SignalR` so a stray `Foo.Hub` is not a hub. `hub-entry` flipped aspirational→expected for podcasts AND bitwarden, both green in-process |
| D1.2b | B2 MAUI catalog descriptor + `UseMaui`/TFM probe; pages/shell as UiEntries | VERIFIED | d138f47 | prism-d1/d1.2b-2/ + d1.2b-fixture/ — new `maui` signal from `<UseMaui>` (SDK-provided, .NET 7+ — no package exists to match) or the mobile TFM triple; podcasts' 2 MAUI apps flip Unknown→`MAUI App [.NET MAUI]`, `maui-present` flipped expected. **Pages/shell proven by fixture, not by podcasts**: `eval/fixtures/MauiSurface` (8/8 green) renders `UI (4)` = AppShell (Shell) + DiscoverPage/PlayerPage (ContentPage) + a [RelayCommand]; there `maui` alone carries the App verdict and opens the DesktopEntryExtractor gate (no desktop-ui fires). Podcasts' own MAUI csprojs are in NO solution (all 3 .sln exclude them), so SolutionScope keeps their sources out of scan — a scoping fact by design (design-doc R1), not a detection gap. eShop pole unmoved: 43 HTTP + 13 Bus + 1 Background + 7 Domain + 42 UI + 3 gRPC = 109 entries |
| D1.2c | B3 MapGroup prefix composition into routes (fixes map, flows, Studio picker, MCP addressing at one locus) | DONE | (D1.2c commit) | prism-d1/d1.2c/podcasts-map.txt — ALL grouped routes composed (`GET /shows/`, `GET /categories/`, `POST /feeds/`, `PUT /feeds/{id}`…); the one remaining `GET /` is ListenTogether.Hub's REAL root route. Mechanism: repo-wide caller-prefix index (`shows.MapShowsApi()` AND inline `app.MapGroup("/x").MapXApi()` chains) seeds the extension method's receiver param; single-distinct-prefix + never-called-bare rule keeps ambiguous methods honest; receiver-aware nesting (`var v1 = api.MapGroup("/v1")` → `/api/v1`); ext-method scan now runs BEFORE the whole-file scan so the composed route wins the file:line dedup. 6 unit tests (cross-file, inline chain, ambiguous→bare, mixed→bare, nested seed, receiver nesting). VERIFIED pending cohort sweep |
| D1.2d | B5 queue seams: Storage-Queue/ASB/RabbitMQ senders + hosted consumers as `[approx]` channel edges | DONE | (D1.2d commit) | prism-d1/d1.2d/ — podcasts event board: `EVENT WIRING (1 integration, 1 cross-service)`: `feed-queue [AzureStorageQueue]: Podcast.API → Podcast.Ingestion.Worker` + CROSS-SERVICE `bus (1)` (was "No events detected"). Mechanism: EventBusExtractor queue-seam phase (package-gated) — send/receive verb sites per transport joined by channel (site literal, else the repo's single `new QueueClient(…, "name")` literal, else unresolved); publishers → EventFlowDetection `Publish`, hosted consumers → MessageConsumerDetection; GraphBuilder adds the Raises half onto the shared channel node (Resolution.Syntactic = [approx]); a type doing BOTH directions on one transport is the bus IMPLEMENTATION (eShop EventBusRabbitMQ) and is dropped whole; a hosted-worker consumer keeps its Background entry (no dup Bus row). **Poles re-driven live: eShop 1089/837/109 + 13 events, dogfood 439/339/34 Microservices + gateway(4) — both unmoved.** 3 unit tests (podcasts shape, eShop guard, bitwarden split-classes join). VERIFIED pending cohort sweep |
| D1.2e | B6 honest branding — hand-rolled `IRequestHandler` ⇒ "CQRS (hand-rolled)" | DONE | (D1.2e commit) | podcasts STACK now `CQRS (hand-rolled mediator)` (was `MediatR (CQRS)` with zero MediatR refs — d1.2c/podcasts-map.txt). Discriminator: repo DECLARES `IRequestHandler` itself ⇒ HandRolled; package signal or impls-without-local-declaration (G7 scoped-sub-project) ⇒ Package. Style evidence strings follow (`hand-rolled mediator with N handlers`); scoring unchanged. 4 unit tests. VERIFIED pending cohort sweep |
| D1.2f | C6 entry target attribution fidelity (`GET / → ShowClient.CheckLink`; interface-as-target) | DONE | (D1.2f commit) | prism-d1/d1.2f/ — podcasts `PUT /feeds/{id}` → **`FeedClient.AddFeedAsync`** (was bare `IFeedClient`). Mechanism: `AddHttpClient<TInterface,TImpl>` is now a real DirectBinding (was invisible to DI) → the call-graph DI map resolves interface member calls to the impl; plus a `http-client-binding` tag on the Resolves edge + Type-kind target substitution in ResolvePrimaryCall as backstop. **Principled scope: typed-CLIENT interfaces only — domain ports keep interface-as-contract display, so eShop's `GET /api/orders/cardtypes → IOrderQueries` is UNCHANGED (byte-diffed).** The audit's other exemplar `GET / → ShowClient.CheckLink` was code-read verified TRUE attribution (ShowsApi.cs:37 calls showClient.CheckLink per show) — its confusion was the 5-way bare-route collision D1.2c fixed; now renders per-route. VERIFIED pending cohort sweep |

| D1.2-fix2 | **dogfood pole regression (`Microservices`→`CleanArchitecture`, ROUTES + gateway edges gone) + poles made first-class gate coverage** | VERIFIED | 78aebc7 | prism-d1/d1.2-fix2/ — bisected to **D1.1c** (clean `439/339/34`~1799 tok at D1.1b; red `439/335/34`~1536 tok from D1.1c; the 4 lost edges ARE the gateway ones). Cause: dogfood's gateway `YarpApiGateway` (`Sdk="Microsoft.NET.Sdk.Web"`) self-sourced `gateway` purely by NAME (Gateway descriptor's `Packages` holds the STALE id `Microsoft.ReverseProxy`; dogfood uses `Yarp.ReverseProxy`), and D1.1c's two guards each killed it — name-boundary (`YarpApiGateway`[4]=`A`≠`.`) and runnable-guard (Web-SDK is runnable). **Design call made:** the guards are right for framework libraries + load-bearing for wolverine (role=AppEntry, so "scope to FrameworkLibrary" was WRONG); the outlier is Gateway — 1 descriptor, already disambiguated STRUCTURALLY by peer-count in ArchetypeDetector (`cs:40-43`: "self-source is NOT the discriminator … only the peer count separates them"), gateway branch runs BEFORE the framework branch so restoring the signal can't flip to Library. **Fix:** exempt `SurfaceRole.Gateway` from BOTH guards (runnable-skip + keep prefix matching, since concatenation IS the gateway naming convention); threaded the descriptor Role through `ProjectNameSignalMap`. dogfood byte-identical to baseline again (439/339/34, Microservices, ROUTES + `http/via gateway (4)` back). **Deeper fix (the real gate hole): poles are now eval expectations** — `dogfood-microservices.json` + `shamshir-pole.json` (machine-local paths → SKIP on CI, now PRINTED by gates.ps1 Step 3; pin SEMANTICS only, never live-repo counts). 4 new gateway unit tests. Full eval 43 repos: only dogfood/cross-service-gateway fails and it's now aspirational (CLI-vs-in-process http-service-link divergence, named for D2 — style/gateway/routes all pass). |

**Known latent, found while root-causing D1.2-fix — NOT fixed (deliberately out of D1.2b's blast
radius; candidates for D2's self-health strand):**
- `CsprojReader.ResolveIsPackable` / `ResolveTargetFrameworksFromAncestors` walk ancestor
  `Directory.Build.props` with the SAME condition-blindness that caused the xunit flip. Only
  `ResolveOutputType` is hardened. `IsPackable` feeds the Library verdict and TFMs now feed the MAUI
  probe, so a repo with a conditioned `<TargetFrameworks>` or `<IsPackable>` in shared props can be
  mislabelled the same way. Fixing all three at once was rejected as too wide a change to ride in on
  a MAUI checkpoint — it needs its own commit + cohort sweep.
- `ServiceBoundaryInference.CsprojSdkContains` says "the csproj's `Sdk` attribute" but actually
  full-text-searches the whole csproj (`File.ReadAllText(...).Contains(marker)`), so any csproj that
  merely MENTIONS `Microsoft.NET.Sdk.Web`/`.Worker`/`Aspire.AppHost.Sdk` anywhere (a comment, an
  `<Import>`, a property value) reads as a runnable host. Not xunit's cause, but the same evidence-
  honesty class D1 exists to kill.
- Gateway descriptor's `Packages` id is STALE: `Microsoft.ReverseProxy` should be `Yarp.ReverseProxy`
  (the modern package). Harmless now that D1.2-fix2 made the NAME path work for dogfood, but it is why
  the name was the sole gateway source — a package-referencing YARP gateway with a non-matching name
  would still be missed. One-line descriptor fix; do it with the CsprojReader sweep above.

### D1.3 — Per-service style rungs (C4)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.3a | owns-endpoints ⇒ Web API/MVC; owns-hubs ⇒ SignalR host; IdentityServer/OpenIddict ⇒ Identity provider; ViewModel+WPF ⇒ Desktop MVVM; `Api` without dot; in-framework Razor-Pages probe | DONE | (D1.3a commit) | prism-d1/d1.3a/ — bitwarden 17/17 Unknown → **1/16** (`Api: Web API`, `Identity/Sso: Identity provider [IdentityServer]` via 1-hop transitive pkg, `Notifications: SignalR host` (name-hinted hub-dominance — it owns aux controllers too), Admin/Billing/Events/Icons/Scim: MVC via endpoint OWNERSHIP, `*Utility`: CLI, bare `AppHost`: Aspire AppHost, MicroBenchmarks filtered as benchmark; only Setup left Unknown). ScreenToGif: **STYLE `DesktopMvvm` (new enum member, fallback-only rung: UseWPF/WinForms probe + ≥3 ViewModels)** + per-service `Desktop (MVVM) [WPF/WinForms]`. **Sanctioned pole delta (documented, not suppressed): eShop `Identity.API` now `Identity provider [IdentityServer]` (was `Web API [EF Core]`) — strictly truer; counts/style/targets unchanged (1089/837/109 byte-diffed otherwise).** VERIFIED pending cohort sweep |

### D1.4 — Hygiene riders (E1, E3–E5)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.4a | E3 duplicate-name disambiguation (`Messages` ×6, `AppHost` ×2) | DONE | (D1.4a commit) | prism-d1/d1.4a/ — `MapBuilder.DisambiguateNames`: a duplicated short name gets its nearest non-echo ancestor dir as qualifier, widened until distinct (`GitVersion.Configuration (src)` renders live; wolverine's `Messages` ×6 already dead via D1.1's sample filtering — the mechanism + unit test cover the general case). Unique names stay bare so no other repo's topology moves. VERIFIED pending close sweep |
| D1.4b | E5 TFM summarization (no raw TFM matrices in STACK) | DONE | (D1.4bcd commit) | `ParseTargetFrameworks` SPLITS `;`-joined values into real TFMs (root cause: the whole matrix travelled as ONE token); STACK renders ≤3 verbatim (poles byte-safe) else 2 most-modern + `+N more TFMs` ranked family-then-version (podcasts: `net7.0, net7.0-android +2 more TFMs`). Multi-TFM extractor test re-pinned to split behavior. VERIFIED pending cohort sweep |
| D1.4c | E1 skip `Update=`/MSBuild-expression package refs | DONE | (D1.4bcd commit) | `ParsePackageReferencesCpmAware` takes `Include=` ONLY (an `Update=`-only element is an MSBuild metadata patch, not a dependency — GitVersion's `@(PackageReference)` "package") + MSBuild-expression names (`@(`/`$(`) filtered; DependencyExtractor keeps Update as SIGNAL evidence but drops expressions. VERIFIED pending cohort sweep |
| D1.4d | E4 remove the `--profile debug` ghost hint | DONE | (D1.4bcd commit) | Both hint sites rewritten (DiscoveryPipeline deep-dive diagnostic + empty-trace NOTE) — the flag is hidden on analyze and absent on query, so naming it was unactionable. No test/golden pinned the old text. VERIFIED pending cohort sweep |

### D1 Definition of Done (from proposal §2)
- Octet expectation rows for archetype/style/entries flip aspirational→expected.
- Newtonsoft / SE.Redis / GitVersion / wolverine render real lenses.
- podcasts hub + MAUI present; bitwarden per-service ≤2/17 Unknown; zero bare-`/` grouped routes.
- MediatR-class repos + Tapestry poles byte-identical.
- Per-commit cheap gates green throughout; `eval/lens-audit.ps1 octet` run at close as the DoD
  proof. (Full battery deferred to phase-end QA — operating model above.)

## Baseline drift table (poles — must stay byte-identical through D1)

| Repo | Nodes/Edges/Entries | Style | Note |
|------|--------------------|-------|------|
| dogfood (eshop-microservices) | 439 / 339 / 34 | Microservices (App) | PRE-EXISTING local mods — never restore. **RESTORED at D1.2-fix2** (was drifted 439/335/CleanArchitecture from D1.1c). Now guarded by `eval/expectations/dogfood-microservices.json`, not just this hand-checked row |
| eShop | 1089 / 837 / 109 | Microservices 0.91 | counts/style/targets unchanged since T2.5. **Two SANCTIONED text deltas at D1.3a/D1.2f (documented, code-read verified):** (1) per-service `Identity.API: Identity provider [IdentityServer]` (was `Web API [EF Core]`) — it IS eShop's Duende host, the D1.3a rung is strictly truer; (2) event-wire participant ORDER shuffled (same sets — new AddHttpClient DirectBinding detections shifted edge insertion order). `GET /api/orders/cardtypes → IOrderQueries` byte-verified UNCHANGED (domain ports keep contract display) |
| shamshir | ~2882 / 3375 / 135 | NLayer 0.6 | live repo; <1.2% drift/session normal. Now guarded by `eval/expectations/shamshir-pole.json` (SEMANTICS only — archetype App + style NLayer + Aspire/Worker rungs — never counts, since it's live). The D1.2b count reading 2955/3507/137 was live-repo churn: the style + rungs the pole pins are all green at D1.2-fix2 |
| TodoApi | 123 / 81 / 12 | MinimalApi | |
| aspire-samples | 68 / 34 / 5 | SampleCollection | T8.2 fix |

Octet "before" snapshot (the thing D1 must flip): Newtonsoft ❌ App/0-entries/19-line map ·
SE.Redis ❌ App/MinimalApi-from-toys · GitVersion ❌ App/0-entries/empty map · wolverine ❌
App/CleanArchitecture-from-samples/80 sample rows · podcasts ⚠ hub+MAUI missing, `GET /` ×5 ·
bitwarden ⚠ 17/17 Unknown, hub missing · ScreenToGif ⚠ style Unknown · refit ✅ (CLI).
Full scorecard: AUDIT.md §Per-repo scorecard.
