# Proposal P — Prism: One Lens, Every .NET Repo Shape — And Worth Looking Through

> Written 2026-07-17 from the unseen-repo lens audit, **two passes**:
> `eval-results/2026-07-17/lens-audit/AUDIT.md` (truth: findings A–H over 8 never-seen repos ×
> engine/CLI/MCP/desktop) and `EXPERIENCE-ADDENDUM.md` (experience + engine health: findings I–N —
> insight validity, silent-failure census, the dead I8 snapshot cache, loading/diagram/Studio UX).
> Evidence: maps, traces, 22-tool MCP transcript, 16 UI screenshots, code-read ground truth, all in
> that directory. Branch scheme: `feat/prism-p<stage>` off `develop`. Tracker: `PRISM-START.md` at P0.
> Pre-release: feature redesign/new features are in scope; waterfalls can be as detailed as needed.

## 0. What Prism is

Tapestry made detect=render hold on the repos we test. The unseen round proved two successor defect
classes:

1. **Archetype≠reality** (engine): a repo whose product is not an ASP.NET-Core app — a library with
   an aux console (Newtonsoft.Json), a client library with toy hosts (StackExchange.Redis), a
   framework whose nuget id differs from its project names (wolverine), a CLI tool (GitVersion) —
   gets a confidently wrong or empty lens. The desktop has **no library mode at all**.
2. **A product that is true but not alive** (experience + self-health): graph canvases are
   force-directed soup that clip a 13-node graph; analysis is a dead 9-word text list while the
   pipeline streams rich per-extractor progress nobody renders; the Context Studio assembles packs
   blind with no live output; entries navigation is a table dump; the `graph.orphans` insight
   accuses live code of being dead (3/5 false on podcasts) because low graph coverage is reported
   as repo fact; the engine swallows its own failures (17 bare `catch{}`, 12 in
   SemanticLitePopulator alone) and the I8 snapshot cache **has never persisted a single snapshot**
   (every cache dir 0 bytes; fire-and-forget save + swallowed exceptions) — so every question
   re-pays full analysis and nothing ever told us.

Prism's one sentence: **any .NET repo gets the lens that matches what it IS, on every surface — and
the lens is alive: visually intelligent, honest about its own coverage and health, instant on the
second question, and engaging while it works.**

Product outcomes:
1. **Shape-true archetypes** — the famous libraries render as libraries; frameworks as frameworks;
   CLI tools as CLI tools; "App with 0 entries" becomes impossible (render backstop).
2. **Entry surfaces that match 2026 .NET** — in-framework SignalR, MAUI, MapGroup prefixes, queue
   seams, Main-anchored consoles.
3. **An engine that cannot fail silently** — no bare swallows, failure counters in the waterfall,
   insights gated by their own coverage, and a snapshot cache that provably works and says so.
4. **A desktop worth demoing** — deterministic layered diagrams with kind/transport semantics (what
   an LLM would draw), a living analysis waterfall, a Studio with live pack preview, an entry
   browser instead of a table, a real library workbench.
5. **Instant second question** — snapshot cache resurrected (CLI + server + UI truthful about it),
   then the merged-compilation lever underneath.
6. **Delivery that matches the README** — engine CI on Linux/macOS or the claim narrows.

Non-goals: no LLM in core; no Graph2 rewrite; no new app pages (reshape the existing five).

## 1. Rules of engagement

Tapestry's rules carry forward (AGENTS.md T-rules, gate battery, truth ratchets, detached boundary
batteries). Additions:
- Every detection/render change re-runs the **unseen octet** via the P0 harness and diffs against
  the round-1 captures.
- **No new bare `catch` lands in Core** from P3 onward (guard in loom-guards).
- Any stage touching the UI ships a **screenshot gate** (visual-gate.mts pattern) for its pages.

## 2. Stage map

### P0 — Harness: the octet + insight validity become permanent gates (small, first)
- **P0.1** Pin the octet SHAs in `eval/README.md`; add aspirational expectation rows for intended
  verdicts (Newtonsoft=Library, SE.Redis=Library, wolverine=Framework-library, GitVersion=CliTool,
  podcasts hub+MAUI present, ScreenToGif=Desktop w/ style, bitwarden per-service styles) — the
  P-phase ratchet: each fixing stage flips its rows to `expected`.
- **P0.2** `eval/lens-audit.ps1 <repo|octet>`: clone-at-pin → timed analyze → map/json capture →
  MCP drive (`eval/mcp-qa/drive-generic.js`) → optional UI screenshot leg → one report dir.
  Encodes the round's protocol (unseen-first, cross-surface, FAIL probes: map-tokens ≪ repo size,
  Unknown+0-entries, sample rows in per-service, wall-time vs baseline).
- **P0.3** **Insight-validity harness**: for each octet repo, every emitted insight's evidence rows
  get a machine check where possible (orphan types: grep-level usage scan; auth counts: attribute
  scan) and a recorded manual verdict otherwise. Insights join the truth-ratchet discipline (I2).

### P1 — Archetype & render honesty (the lens claim itself)
- **P1.1** Library robustness: transitive aux-exe references (A1); `toys`/build-tooling NoiseFilter
  rungs (A2/A3); holder csproj excluded everywhere (E2); topology applies the same filters as
  per-service (E2).
- **P1.2** Catalog self-name audit (nuget id ≠ project names: Wolverine → `Wolverine`, sweep all
  descriptors) (A4); runnable-service inference honors NoiseFilter unless SamplesAreTheProduct (A4).
- **P1.3** `Archetype.CliTool`: Exe + no web surfaces + (PackAsTool || command-parser evidence) →
  command-surface render; plain `Main()` becomes an entry (A3, B4).
- **P1.4** Render backstop — no dead maps: App w/ 0 entries + public surface ⇒ library sections;
  w/ Main ⇒ console view. Harness FAIL: map <~400 tokens on >100-file repo (A5).
- **P1.5** Hygiene: duplicate-name disambiguation (E3), TFM summarization (E5), `Update=`/MSBuild
  expression package refs (E1), stale `--profile debug` hint (E4).
- Gate: Newtonsoft/SE.Redis/GitVersion/wolverine rows flip to expected; Tapestry poles byte-identical.

### P2 — Entry surfaces 2026
- **P2.1** In-framework SignalR: `MapHub<T>`/`: Hub` evidence without package (B1 — podcasts +
  bitwarden; shamshir's fix was package-gated).
- **P2.2** MAUI: catalog descriptor + `UseMaui`/TFM probe; pages/shell as UiEntries; per-service
  rung property-probed (B2).
- **P2.3** MapGroup prefix composition into routes (B3) — fixes map, flows, Studio picker, MCP
  addressing at one locus.
- **P2.4** Queue seams: Storage-Queue/ASB/RabbitMQ senders + hosted consumers as `[approx]` channel
  edges (B5).
- **P2.5** Honest branding: hand-rolled `IRequestHandler` ⇒ "CQRS (hand-rolled)" (B6); entry target
  attribution fidelity (C6).
- Gate: podcasts hub+MAUI expected; bitwarden events >1; zero bare-`/` grouped routes on the octet.

### P3 — Graph depth & engine self-health (same locus, one stage)
- **P3.1** Razor `@code` into the call graph (C1); **P3.2** XAML code-behind/command wiring (C2).
- **P3.3** Type-node degree rollups so `neighbors`/`usages`/`impact` never dead-end on connected
  types (C3; closes conductor-DEBT SymbolTable member indexing).
- **P3.4** DI provenance ranking: focus host's registration first, `[×N hosts]` otherwise (C5).
- **P3.5** **Silent-failure amnesty ends**: every bare `catch{}` in Core becomes catch-log-count
  (a `PipelineDiagnostics` channel: extractor × failure count × sample exception); loom-guards bans
  new bare catches; SemanticLitePopulator's 12 swallows first (J1).
- **P3.6** Failure/skip/partial counters join the stage waterfall + stats (CLI, MCP `stats`, UI
  Stats page) — coverage becomes a visible number per extractor (J3).
- Gate: podcasts UiEntry traces reach services; ScreenToGif trace ≥ depth 2; impact(type) > 0;
  swallow count in Core = 0; stats show per-extractor failure columns.

### P4 — Claims truth: per-service style + insights
- **P4.1** Style evidence rungs: owns-endpoints ⇒ Web API/MVC; owns-hubs ⇒ SignalR host;
  IdentityServer/OpenIddict ⇒ Identity provider; ViewModel+WPF ⇒ Desktop MVVM (C4).
- **P4.2** Name-rung fixes (`Api` w/o dot; in-framework Razor-Pages probe) (C4).
- **P4.3** **Insight coverage-gating**: dead-code claims require edge-coverage floor + exclude
  entity-mapper-indexed and DTO-constructed types; every insight carries its coverage basis in the
  evidence line ("within the 53% of edges verified") (I1). Re-verify octet insights via P0.3.
- Gate: bitwarden ≤2/17 Unknown; ScreenToGif styled; podcasts orphans list contains zero provably
  live types.

### P5 — Living analysis & the resurrected cache
- **P5.1** **Snapshot cache resurrection**: awaited save with error surfacing (no fire-and-forget),
  load verified end-to-end CLI + server, `from cache · <sha> · <ms>` stamp honest, cache
  hit/miss/size in stats; UI Freshness card states snapshot age/HEAD/location and offers
  re-analyze (J2, L2). Target: second CLI question on bitwarden <15s without P8's compiler work.
- **P5.2** **The analysis waterfall becomes the show**: the existing observer stream renders as a
  live, detailed waterfall — per-stage bars with elapsed, current extractor, discoveries streaming
  in as they land ("125 projects · 3 gRPC services · 662 endpoints…"), skip reasons, failure
  counters (P3.6), honest big-repo expectations. As big and detailed as the data allows (K1, L7).
- **P5.3** Post-analyze, the waterfall persists as a Stats-page timeline (K2 surfacing).
- Gate: screenshot gate on the loading experience; cache round-trip truth test in the battery.

### P6 — Visual intelligence (the diagrams an LLM would draw)
- **P6.1** One canvas system: deterministic layered layout (ELK/dagre-class), fit-and-center that
  never clips, stable across pages (F2, L6 — replaces three inconsistent force soups).
- **P6.2** Semantic rendering: kind glyphs on nodes (HTTP/Bus/UI/Worker/gRPC/store), transport-
  labeled edges (HTTP/queue/gRPC/event), DDD-layer lanes where the evidence exists, stores/external
  systems visually distinct (F3, M — the engine already knows all of this; layout just ignores it).
- **P6.3** Progressive disclosure: C4-ish level-1 (services + transports) by default; expand a
  service into its entries/flows on demand; hero graph and Atlas share this.
- **P6.4** **Atlas becomes a real one-pager**: layered architecture view, per-service cards with
  style + entry mix, event/queue board, data stores, export that matches (L3).
- Gate: screenshot gates on podcasts + refit + eShop + bitwarden; a reviewer can answer "how does
  this system work" from the Atlas alone.

### P7 — Studio & navigation: the core loops
- **P7.1** **Live pack preview**: the rendered context pack (markdown, token meter, per-section
  provenance) updates live as scope/budget/intent change; Copy copies exactly what's shown (L4).
- **P7.2** Preset semantics: "I'm changing this entry" et al. get explicit names + one-line effect
  descriptions ("seeds: entry spine + tests + config for a modify task"), and their scope delta is
  visible in the preview (L4).
- **P7.3** **Entry browser replaces the table dump**: ranked, grouped (service → kind → route),
  filter-as-you-type, kind chips, auth badges — the Shift+E table remains as the power view only
  (L5). Requires P2.3 route disambiguation.
- **P7.4** Session naming unified (sln name everywhere) (F4); MCP feed origin fix (F5).
- Gate: a scripted Studio drive builds a pack for a podcasts endpoint entirely from the preview;
  entry browser screenshot gate.

### P8 — MCP & pack honesty
- **P8.1** get_context degenerate-focus honesty: low fill ⇒ say why + suggest connected focuses;
  UiEntry packs pull page members once P3.1 lands (G1).
- **P8.2** CLI trace budget enforced or relabeled — no silent 3× breaches (D3).
- Gate: octet MCP drive: zero empty navigations on connected nodes, zero silent budget breaches.

### P9 — The compiler lever (perf floor under everything)
- **P9.1** Persist/reuse the merged compilation (T7.2 lever: DntSite 81s of 95s; bitwarden 207s) —
  on-disk, keyed by source/package hash, invalidation truth-tested.
- **P9.2** With P5.1 + P9.1: cold bitwarden ~3.5min once, warm <15s, focus-question <10s (D1/D2).
- Gate: bench verdicts unchanged; warm-run targets hit on the octet's big pole.

### P10 — Cross-platform delivery
- **P10.1** Engine CI legs on ubuntu + macos (build + fast suite + CLI strict smoke) or the claim
  narrows in README/PACKAGING (H1); **P10.2** path/casing fixes it shakes out (H2).
- **P10.3** Desktop bundle decision: per-RID sidecar + Tauri mac/linux targets, or documented
  Windows-only (H2); installer version from tag (H3).
- Gate: green cross-OS CI on develop; release dry-run inventory matches the decision.

## 3. Debts folded in / still open
- conductor-DEBT: SymbolTable member indexing → P3.3; BodyFacts scoping, TfmScore, Flow hardening
  remain open.
- eval-results/ volume + analysis-exports/ — owner call, parked.
- Fast-suite load-flake — name still uncaptured; watch P-phase batteries.
- GrpcAggregator style rung — subsumed by P4.1.

## 4. Audit-process improvements (encoded in P0.2/P0.3, learned this round)
- Unseen-first, rotate the octet; cross-surface drives per repo; judge against "what would an
  honest lens say", not expectation files; FAIL probes (map-tokens ≪ repo size, etc.).
- **Verify a sample of insights against code every round** — insights are claims, not decoration.
- **Audit the engine's own health, not just its output**: swallow census, dead-feature check
  (does the cache dir contain bytes?), collected-vs-surfaced inventory.
- Experience is a first-class audit dimension: loading, diagrams, core loops (Studio), navigation —
  graded against "would this demo well / does this engage a dev", with screenshots as evidence.
- Playwright: `domcontentloaded` (MCP page never idles); explicit New-session per repo; launch node
  drives from PowerShell (MSYS mangles `cmd /d /c`).
