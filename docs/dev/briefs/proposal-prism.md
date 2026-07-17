# Proposal P — Prism: One Lens, Every .NET Repo Shape

> Written 2026-07-17 from the unseen-repo lens audit (`eval-results/2026-07-17/lens-audit/AUDIT.md`):
> 8 never-seen repos (Newtonsoft.Json, refit, StackExchange.Redis, wolverine, GitVersion,
> dotnet-podcasts, ScreenToGif, bitwarden-server) driven blind through engine, CLI, MCP, and desktop
> on develop `9ee401a` (post-Tapestry, post-github-ready). Evidence per finding: maps, traces, a
> 22-tool MCP transcript, 13 UI screenshots, and code-read ground truth in that directory.
> Branch scheme: `feat/prism-p<stage>` off `develop`. Tracker to be created at P0 as `PRISM-START.md`.

## 0. What Prism is

Tapestry made detect=render hold on the repos we test. The unseen round proved the successor defect
class: **archetype≠reality**. Give the engine a repo whose product is not an ASP.NET-Core app — a
library with an aux console (Newtonsoft.Json), a client library with toy hosts (StackExchange.Redis),
a framework whose nuget id differs from its project names (wolverine), a CLI tool (GitVersion) — and
it confidently renders the wrong lens or an empty one. The desktop is worse: it has **no library
mode at all** (an analyzed refit shows "0 entries · No entry data available" and Explore shows the
*unanalyzed* empty state), and its graph canvases clip labels and spill nodes off-viewport on
ordinary 13-node graphs.

Prism's one sentence: **any .NET repo — library, framework, tool, desktop, mobile, service, or
monolith — gets the lens that matches what it IS, on every surface, with graph depth that makes
traces and agent navigation land.**

Product outcomes:
1. **Shape-true archetypes** — the famous libraries of .NET render as libraries; frameworks as
   frameworks; CLI tools as CLI tools; and "App with 0 entries" is impossible (a render backstop
   always shows the surface that exists).
2. **A desktop that matches the CLI** — a library session gets a Library workbench (public surface,
   abstractions, generators, consumer paths), and every canvas fits its content.
3. **Entry surfaces that match 2026 .NET** — in-framework SignalR, MAUI, MapGroup prefixes, queue
   seams, Main-anchored consoles.
4. **Agent navigation without dead ends** — type-level degrees aggregate members; packs explain
   their fill; the CLI reuses analysis instead of re-paying 200s per question.
5. **Delivery that matches the README** — engine CI legs on Linux/macOS, or the claim narrows.

Non-goals: no LLM in core; no Graph2 rewrite; no new page chrome in the app (reuse Workbench/Atlas
shells); perf work beyond the named compilation lever stays out.

## 1. Rules of engagement

Tapestry's rules carry forward unchanged (AGENTS.md T-rules, gate battery per
`.claude/skills/dev-pipeline/SKILL.md`, truth ratchets, boundary-only full batteries, detached
launches). One addition: **every stage that changes detection or rendering must re-run the unseen
octet** (`eval/gates` scope stays as-is; the octet runs via the audit harness, see P0.2) and diff
against the captures in `eval-results/2026-07-17/lens-audit/` — the octet is the phase's regression
pole set, exactly as shamshir/eShop were Tapestry's.

## 2. Stage map

### P0 — Harness: make the unseen octet a permanent gate (small, first)
- **P0.1** Pin the octet: record the 8 audited SHAs in `eval/README.md` §unseen-octet; add
  `eval/expectations/` truth rows for the *intended* verdicts (Newtonsoft=Library, SE.Redis=Library,
  wolverine=Library/Framework, GitVersion=CliTool, podcasts SignalR hub present, ScreenToGif=Desktop,
  bitwarden Microservices + per-service styles ≠ all-Unknown). Marked `aspirational` until the fixing
  stage flips each to `expected` — the P-phase ratchet.
- **P0.2** Audit harness script: `eval/lens-audit.ps1 <repo|octet>` — clone-at-pin, analyze, capture
  map/json/stats/time, run the MCP drive (`eval/mcp-qa/drive-generic.js`, generalized from this
  round's `mcp-drive.js`), optional UI screenshot leg; writes `eval-results/<date>/lens-audit/`.
  This encodes the audit process improvements (see §4).
- **P0.3** Wall-time budget rows in the harness output (repo → seconds, vs recorded baseline) so
  perf regressions surface in the same artifact.

### P1 — Archetype & render honesty (the lens claim itself)
- **P1.1** Library detection robustness: aux-exe check follows **transitive** project references
  (A1); add `toys` + build-tooling path rungs to NoiseFilter (A2, A3); holder/source-less csproj
  (`docs.csproj`, `.github.csproj`) excluded everywhere topology reads projects (E2); topology
  applies the same test/sample filters as per-service (E2).
- **P1.2** Catalog self-name audit: every FrameworkLibrary/AppEntry descriptor whose nuget id ≠
  repo project names gets SelfNamePatterns (wolverine → `Wolverine`; sweep the whole catalog against
  its upstream repos) (A4). Per-service/runnable-service inference honors NoiseFilter unless
  `SamplesAreTheProduct` (A4 — wolverine's 80-row table → its real src/ projects).
- **P1.3** CliTool archetype: OutputType Exe + no web/entry surfaces + (PackAsTool || name/dist
  evidence) → `Archetype.CliTool`; render = command surface (parser-detected commands if present,
  else Main-anchored entry + first-call fan-out) (A3, B4).
- **P1.4** Render backstop — "no dead maps": archetype App with 0 entries and a public surface
  renders the library surface sections; with a Main, renders the console view. A map under ~400
  tokens on a >100-file repo is a FAIL condition in the harness (A5).
- **P1.5** Duplicate-name disambiguation in topology/per-service rows (`Messages` ×6 → path-suffixed)
  (E3); STACK line TFM summarization (E5); package extractor skips `Update=`/MSBuild-expression
  refs (E1); kill the stale `--profile debug` hint (E4).
- Gate: Newtonsoft/SE.Redis/GitVersion/wolverine expectation rows flip aspirational→expected;
  MediatR-class + aspire-samples + all Tapestry poles byte-identical.

### P2 — Entry surfaces 2026 (detect the .NET that exists)
- **P2.1** In-framework SignalR: `MapHub<T>`/`: Hub` base-type evidence fires the signal without any
  package reference (B1 — podcasts + bitwarden both regress today; shamshir passes only via package).
- **P2.2** MAUI descriptor: catalog entry + `UseMaui` property / `-android;-ios` TFM probe; pages +
  shell routes as UiEntries; per-service "MAUI App" rung fixed to property-probe (B2).
- **P2.3** MapGroup prefix resolution: endpoint extractor composes `MapGroup("/feeds")` prefixes into
  routes (B3) — fixes map, flows, Studio picker, MCP addressing in one locus.
- **P2.4** Queue seams: Azure Storage Queues / raw ASB senders+hosted consumers join event wiring as
  `[approx]` channel edges with named channel (B5 — podcasts queue path, bitwarden Events pipeline).
- **P2.5** Honest CQRS branding: name-only IRequestHandler matches render as "CQRS (hand-rolled)"
  unless the MediatR package is referenced (B6). Entry target attribution fidelity pass (C6).
- Gate: podcasts hub + MAUI rows expected; bitwarden event wiring >1; zero bare-`/` grouped routes
  on the octet.

### P3 — Graph depth where traces die today
- **P3.1** Razor `@code` blocks parsed into the call graph (Blazor UiEntry → OnInitialized →
  services); UiEntry reach/score become real (C1).
- **P3.2** XAML code-behind wiring: event handlers + ICommand bindings become out-edges of the
  window/control entry (C2 — ScreenToGif traces stop being self-loops).
- **P3.3** Type-node degree aggregation: type nodes expose member-edge rollups (or MCP resolves
  type queries to member sets) so `neighbors`/`usages`/`impact` never return empty on a type that
  has connected members (C3; closes the conductor-DEBT SymbolTable member-indexing item).
- **P3.4** DI provenance ranking: prefer the focus entry's host registration; multiple registrations
  listed as `[×N hosts]` (C5).
- Gate: podcasts UiEntry trace reaches PodcastService; ScreenToGif ExportPanel trace ≥ depth 2;
  MCP impact("PodcastService", up) > 0.

### P4 — Style truth per service
- **P4.1** Evidence rungs: owns-controllers/minimal-endpoints → "Web API"/"MVC"; owns-hubs →
  "SignalR host"; IdentityServer/OpenIddict → "Identity provider"; ViewModel-project + WPF →
  "Desktop MVVM" (C4).
- **P4.2** Name-rung fixes: `Api` without dot; `Admin` Razor-Pages probe via in-framework evidence
  not package (C4).
- Gate: bitwarden per-service table ≤2 Unknown of 17; ScreenToGif style ≠ Unknown.

### P5 — Desktop: library mode + canvas honesty (the UI half of the claim)
- **P5.1** **Library workbench**: archetype Library routes Explore to a public-surface browser
  (ENTRY API / ABSTRACTIONS / GENERATORS / PUBLIC SURFACE / CONSUMER PATHS as the rail), Atlas
  one-pager gets the library sections, home cards swap entry-metrics for surface-metrics (F1).
  Style chip suppressed for Library exactly as the CLI does (F1).
- **P5.2** Canvas fit/layout: every cytoscape canvas fits-and-centers its graph, labels never clip
  at the viewport, small graphs don't cluster in a corner (F2 — visual-gate screenshots on podcasts
  + refit + eShop).
- **P5.3** Per-kind node iconography on canvases (HTTP/Bus/UI/Worker/gRPC glyphs match the entry
  rail) (F3).
- **P5.4** Session identity: one naming rule (solution name everywhere, dir as fallback) across
  home/tab/status bar (F4); MCP live-feed origin fix — app RPCs never labeled `agent` (F5).
- Gate: refit UI session shows the full library surface; screenshot diff gate for canvas fit.

### P6 — MCP & pack honesty
- **P6.1** get_context degenerate-focus honesty: fill <50% ⇒ pack says why (entry has no graph
  reach) + suggests nearest connected focuses; UiEntry packs pull the page's own members once P3.1
  lands (G1).
- **P6.2** CLI trace budget: enforce or re-label (`Tokens ~24.7k (budget 8000)` never again —
  budgeted BFS on type focuses like MCP trace already does) (D3).
- Gate: octet MCP drive has zero empty-result navigations on connected nodes, zero silent
  budget breaches.

### P7 — The perf lever (carried, now scoped)
- **P7.1** Persist/reuse the merged compilation across runs (the named T7.2 lever: DntSite 81s of
  95s; bitwarden 207s×every-question) — on-disk cache keyed by source/package graph hash.
- **P7.2** CLI session reuse: `--focus` against an unchanged repo attaches to the cached graph
  (target: second question on bitwarden <10s, from 152s) (D1).
- Gate: bench suite unchanged verdicts; bitwarden Q2 wall <10s; cache invalidation truth-tested.

### P8 — Cross-platform delivery
- **P8.1** Engine CI legs on `ubuntu-latest` + `macos-latest` (build + fast suite + CLI strict
  smoke; eval stays Windows) — or the README/PACKAGING claim narrows explicitly (H1).
- **P8.2** Path/casing hygiene sweep for whatever P8.1 shakes out.
- **P8.3** Desktop bundles decision: either per-RID sidecar publish + Tauri mac/linux targets in
  release.yml, or a documented Windows-only-installer stance (H2). Installer version from the
  release tag (H3).
- Gate: green cross-OS CI on develop; release dry-run artifacts inventory matches the decision.

## 3. Debts folded in / still open
- conductor-DEBT: SymbolTable member indexing lands inside P3.3; BodyFacts scoping + TfmScore +
  Flow hardening remain open (unchanged by this audit).
- eval-results/ tracked-file volume (432+) + analysis-exports/ — owner call, still parked.
- Fast-suite load-flake — name still uncaptured; watch P-phase battery logs.
- GrpcAggregator per-kind style rung — subsumed by P4.1 if done there, else stays.

## 4. Audit-process improvements this round proved (encode in P0.2)
- Unseen-first: rotate a fresh octet each audit round; seen fixtures only pin regressions.
- Drive all four surfaces per repo, same session: CLI map+trace, MCP transcript with token counts,
  UI screenshots, ground-truth code read — the cross-surface diffs (CLI library lens vs UI empty
  dashboard; CLI suppressed style vs UI ControllerBased) were the highest-value findings.
- Judge exact output against "what would an honest lens say", not against expectations files —
  expectations only encode yesterday's blind spots.
- Time every analyze; treat map-token-count << repo-size as an automatic FAIL probe (caught
  Newtonsoft at 209 tokens).
- Playwright legs: `domcontentloaded` (MCP page never reaches networkidle); a second repo needs an
  explicit New-session click or it silently re-tours the old session; MSYS bash mangles `cmd /d /c`
  (use PowerShell to launch node drives).
