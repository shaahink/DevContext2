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

last: 2026-07-17 **D1 session 2 running.** Ack: INBOX 2026-07-17 standing orders (unchanged).
Session 1 died mid-D1.2 (~07:22) leaving D1.2a+D1.2b work UNCOMMITTED and both rows TODO; session 2
picked the tree up, validated it, finished it, and committed. **D1.0 + D1.1 + D1.2a/D1.2b VERIFIED.**
Session 1's D1.2a/b code was sound and its gates were green — but it never ran the eval, which is
where the two gaps were: (1) D1.2b's "pages/shell as UiEntries" half was unproven and unprovable on
podcasts (its MAUI csprojs are in no .sln → out of SolutionScope) — now proven by the new
`eval/fixtures/MauiSurface`; (2) a **xunit ratchet was red** (`Library`→`App`), bisected to D1.1c
and root-caused to false OutputType evidence — fixed (row D1.2-fix).
next: **D1.2-fix2 FIRST** (dogfood pole regression from D1.1c — the primary pole renders the WRONG
style today; row below has the full bisect + root cause + a proposed fix shape; it needs a design
call because D1.1c's runnable guard and the gateway-self-source invariant genuinely conflict) →
then D1.2c MapGroup prefixes → D1.2d queue seams → D1.2e branding → D1.2f target fidelity → D1.3 → D1.4.
**D1.2c is scoped**: podcasts does `var shows = app.MapGroup("/shows"); shows.MapShowsApi();` in
Program.cs while `ShowsApi.cs` does `group.MapGet("/", …)` on the RouteGroupBuilder parameter —
`EndpointExtractor.ExtractGroupPrefixes` already resolves group vars, but only WITHIN one file /
extension-method body, so the prefix never crosses the call boundary and every route renders bare
`GET /`. The fix is composing the caller's prefix into the callee extension method.
**LESSON (why the eval must run before a row is marked VERIFIED):** the cheap gates (build + fast
tests + loom-guards) are blind to archetype regressions — the whole-cohort eval is the only thing
that caught xunit, and D1.1c shipped 4 checkpoints ago with it red. Run
`--filter "FullyQualifiedName~EvalExpectationTests&Category=Eval"` (~10 min, detached) at every
checkpoint that touches archetype/signal/evidence, not just the repos you think you changed.
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
| D1.1c | A4 catalog self-name audit — SelfNamePatterns wherever nuget id ≠ project names, wolverine first; runnable-service inference honors NoiseFilter unless SamplesAreTheProduct | VERIFIED | (D1.1c commit) | prism-d1/d1.1c/ — wolverine flips App→Library (6272-token LIBRARY, 1322 public types, 0 sample rows, lens PASS); 18 catalog descriptors gain SelfNamePatterns; matcher hardened: name-boundary (kills WolverineDemo/SerilogHelpers/OrleansVoting false matches) + non-runnable self-source guard; per-service honors sample filter w/ T8 waiver; 4 eval rows flipped + green; 8 new unit tests. **REGRESSED xunit (found + fixed at D1.2b, see below) — bisected to this commit: the runnable guard rests on `ProjectInfo.OutputType`, which was itself false evidence.** |
| D1.1d | A3/B4 `Archetype.CliTool`: Exe + no web surfaces + PackAsTool/parser evidence → command-surface render; plain `Main()` becomes an entry | VERIFIED | (D1.1d commit) | prism-d1/d1.1d/ — GitVersion flips App→CliTool: CLI TOOL header + COMMAND SURFACE + `CLI (1)` Main entry (Program.cs:3 provenance), lens PASS 19.9s; IsToolPackaged (PackAsTool/ToolCommandName incl. conditional) + parser-package evidence; Main fallback in CliCommandExtractor (reformed in place); 4 unit tests (bitwarden-utility + Newtonsoft-aux negatives); 2 eval rows flipped (cli-entries type corrected), green |
| D1.1e | A5 render backstop — no dead maps: 0 entries + public surface ⇒ library sections; + Main ⇒ console view; harness FAILs any <~400-token map on a >100-file repo | VERIFIED | (D1.1e commit) | ConsoleBackstop fixture (eval/fixtures) + console-backstop.json 5/5 green in-process — App+0-entries renders NOTE + ENTRY API/ABSTRACTIONS/PUBLIC SURFACE (MapBuilder backstop Surface, renderer sections reused); no-surface branch renders CONSOLE VIEW of production exes; harness probe live since D1.0c; Newtonsoft regression check byte-stable (2355 tokens) |

### D1.2 — Entry surfaces 2026 (B1–B6, C6)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.2-fix | **xunit regression (unplanned, found by the D1.2b eval sweep)** — `Library`→`App` + console-view render | VERIFIED | d138f47 | The whole-cohort eval caught it (41/42 green, xunit red on the `archetype-library` RATCHET — a row green since long before Prism). Bisected across the 5 D1.1 commits: clean at D1.1a/b, red from **D1.1c** on. Chain: xunit's `src/Directory.Build.props` sets `<OutputType>Exe</OutputType>` inside `<Choose><When Condition="…EndsWith('.tests')">`, but `ResolveOutputType`'s ANCESTOR walk took any `<OutputType>` descendant and ignored the condition → every xunit CLASSLIB read as an exe → D1.1c's new runnable-guard skipped them all → the self-sourced `testing` signal vanished → no early Library return → `nonExe.Count == 0` → `App` ("pure executable(s)"), and D1.1e's backstop then rendered a CONSOLE VIEW instead of LIBRARY. **Fixed at the root (the false evidence), not the guard** — a conditioned ANCESTOR value applies to a subset of projects and we cannot evaluate MSBuild conditions, so it is not evidence for this project; the csproj's OWN conditioned value is still honoured (eShop's ClientApp relies on it). xunit → `Library`/`signals=testing`/981 tokens, byte-identical to the D1.0d baseline. 2 regression tests (conditioned-ancestor + xunit self-name rows) |
| D1.2a | B1 in-framework SignalR: `MapHub<T>`/`: Hub` evidence, package-free (podcasts ListenTogetherHub + bitwarden NotificationsHub) | VERIFIED | d138f47 | prism-d1/d1.2a/ — podcasts renders `SignalR (1) ListenTogetherHub (8 methods: OpenRoom, JoinRoom, LeaveRoom)` with NO SignalR package; matcher takes the base AS WRITTEN (bare `Hub`/`Hub<T>` or SignalR-qualified `Microsoft.AspNetCore.SignalR.Hub` — the form BOTH podcasts and bitwarden use, and exactly what the audit missed), plus `MapHub<T>` file evidence; qualifier must end in `SignalR` so a stray `Foo.Hub` is not a hub. `hub-entry` flipped aspirational→expected for podcasts AND bitwarden, both green in-process |
| D1.2b | B2 MAUI catalog descriptor + `UseMaui`/TFM probe; pages/shell as UiEntries | VERIFIED | d138f47 | prism-d1/d1.2b-2/ + d1.2b-fixture/ — new `maui` signal from `<UseMaui>` (SDK-provided, .NET 7+ — no package exists to match) or the mobile TFM triple; podcasts' 2 MAUI apps flip Unknown→`MAUI App [.NET MAUI]`, `maui-present` flipped expected. **Pages/shell proven by fixture, not by podcasts**: `eval/fixtures/MauiSurface` (8/8 green) renders `UI (4)` = AppShell (Shell) + DiscoverPage/PlayerPage (ContentPage) + a [RelayCommand]; there `maui` alone carries the App verdict and opens the DesktopEntryExtractor gate (no desktop-ui fires). Podcasts' own MAUI csprojs are in NO solution (all 3 .sln exclude them), so SolutionScope keeps their sources out of scan — a scoping fact by design (design-doc R1), not a detection gap. eShop pole unmoved: 43 HTTP + 13 Bus + 1 Background + 7 Domain + 42 UI + 3 gRPC = 109 entries |
| D1.2c | B3 MapGroup prefix composition into routes (fixes map, flows, Studio picker, MCP addressing at one locus) | TODO | | |
| D1.2d | B5 queue seams: Storage-Queue/ASB/RabbitMQ senders + hosted consumers as `[approx]` channel edges | TODO | | |
| D1.2e | B6 honest branding — hand-rolled `IRequestHandler` ⇒ "CQRS (hand-rolled)" | TODO | | |
| D1.2f | C6 entry target attribution fidelity (`GET / → ShowClient.CheckLink`; interface-as-target) | TODO | | |

| D1.2-fix2 | **dogfood pole REGRESSION — `Microservices`→`CleanArchitecture`, ROUTES + gateway edges gone. NOT FIXED — do this FIRST** | **BLOCKED** (needs a design call) | — | Bisected the same way as D1.2-fix: clean at D1.0d/D1.1a/D1.1b (`439/339/34`, ~1799 tok), red from **D1.1c** on (`439/335/34`, ~1536 tok — the 4 lost edges ARE the gateway ones). Not mine: the committed tip 6ffe77b reproduces it exactly. Map diff at D1.1c: STYLE `Microservices (7 runnable web services with gateway + message bus)` → `CleanArchitecture (DDD folder layers…)`; the whole **ROUTES** block (6 YARP routes) gone; **CROSS-SERVICE `http/via gateway (4)`** gone. Cause: dogfood's gateway is project `YarpApiGateway` (`Sdk="Microsoft.NET.Sdk.Web"`), and the Gateway descriptor's only live evidence is `SelfNamePatterns: ["Yarp","ReverseProxy"]` — its `Packages` list holds the OLD id `Microsoft.ReverseProxy`, while dogfood references today's `Yarp.ReverseProxy`, so the NAME was the sole source. D1.1c's two guards each kill it independently: (1) name-boundary — `YarpApiGateway`[4] is `A`, not `.`; (2) runnable-guard — a Web-SDK project is runnable. **This directly contradicts a documented design invariant**: `ArchetypeDetector.cs:40-43` states "a microservices app naming a project `YarpApiGateway` self-sources the gateway signal exactly as YARP's own repo does; only the peer count separates them." **Why not fixed here:** the runnable guard is load-bearing for wolverine (D1.1c's DoD) yet a gateway host is runnable BY NATURE — the two rules are in real conflict, so this needs a design call, not a patch smuggled into a MAUI checkpoint. Likely shape: scope the runnable guard (and possibly the boundary rule) to `SurfaceRole.FrameworkLibrary` descriptors only — frameworks are classlibs; gateways/hosts are not — and refresh the Gateway descriptor's stale package id. Neither dogfood nor shamshir is in the eval cohort, which is why 42/42 green did NOT catch this; the drift table is the only guard and it is checked by hand. |

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

### D1.3 — Per-service style rungs (C4)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.3a | owns-endpoints ⇒ Web API/MVC; owns-hubs ⇒ SignalR host; IdentityServer/OpenIddict ⇒ Identity provider; ViewModel+WPF ⇒ Desktop MVVM; `Api` without dot; in-framework Razor-Pages probe | TODO | | |

### D1.4 — Hygiene riders (E1, E3–E5)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.4a | E3 duplicate-name disambiguation (`Messages` ×6, `AppHost` ×2) | TODO | | |
| D1.4b | E5 TFM summarization (no raw TFM matrices in STACK) | TODO | | |
| D1.4c | E1 skip `Update=`/MSBuild-expression package refs | TODO | | |
| D1.4d | E4 remove the `--profile debug` ghost hint | TODO | | |

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
| dogfood (eshop-microservices) | 439 / 339 / 34 | Microservices (App) | PRE-EXISTING local mods — never restore. **DRIFTED at D1.1c → 439/335/34, style now CleanArchitecture (WRONG) — see D1.2-fix2. Do NOT re-pin this row to 335: 339/Microservices is the truth to restore.** |
| eShop | 1089 / 837 / 109 | Microservices 0.91 | unchanged since T2.5 |
| shamshir | ~2882 / 3375 / 135 | NLayer 0.6 | live repo; <1.2% drift/session normal. Measured 2026-07-17 at D1.2b: 2955/3507/137 (+2.5%/+3.9%) — above the usual band, but shamshir is live and this was NOT bisected; check it alongside D1.2-fix2 rather than assuming live-repo churn |
| TodoApi | 123 / 81 / 12 | MinimalApi | |
| aspire-samples | 68 / 34 / 5 | SampleCollection | T8.2 fix |

Octet "before" snapshot (the thing D1 must flip): Newtonsoft ❌ App/0-entries/19-line map ·
SE.Redis ❌ App/MinimalApi-from-toys · GitVersion ❌ App/0-entries/empty map · wolverine ❌
App/CleanArchitecture-from-samples/80 sample rows · podcasts ⚠ hub+MAUI missing, `GET /` ×5 ·
bitwarden ⚠ 17/17 Unknown, hub missing · ScreenToGif ⚠ style Unknown · refit ✅ (CLI).
Full scorecard: AUDIT.md §Per-repo scorecard.
