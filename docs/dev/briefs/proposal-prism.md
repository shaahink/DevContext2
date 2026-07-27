# Proposal P — Prism: One Lens, Every .NET Repo Shape — And Worth Looking Through

> Written 2026-07-17 from the unseen-repo lens audit, **two passes**:
> `eval-results/2026-07-17/lens-audit/AUDIT.md` (truth: findings A–H over 8 never-seen repos ×
> engine/CLI/MCP/desktop) and `EXPERIENCE-ADDENDUM.md` (experience + engine health: findings I–N —
> insight validity, silent-failure census, the dead I8 snapshot cache, loading/diagram/Studio UX).
> Evidence: maps, traces, 22-tool MCP transcript, 16 UI screenshots, code-read ground truth, all in
> that directory. **Organized as 5 big deliveries (one session each) with a standing QA+fix
> cadence** — not checkpoint-a-day stages. Branch scheme: `feat/prism-d<delivery>` off `develop`,
> merged per delivery after its QA passes. Tracker: `PRISM-START.md` at D1 open.
> Pre-release: feature redesign/new features in scope; waterfalls as detailed as needed.

## 0. What Prism is

Tapestry made detect=render hold on the repos we test. The unseen round proved two successor defect
classes:

1. **Archetype≠reality** (engine): a repo whose product is not an ASP.NET-Core app — a library with
   an aux console (Newtonsoft.Json), a client library with toy hosts (StackExchange.Redis), a
   framework whose nuget id differs from its project names (wolverine), a CLI tool (GitVersion) —
   gets a confidently wrong or empty lens. The desktop has **no library mode at all**.
2. **A product that is true but not alive** (experience + self-health): canvases are force-directed
   soup that clip a 13-node graph; analysis is a dead 9-word list while the pipeline streams rich
   per-extractor progress nobody renders; the Context Studio assembles packs blind; entries
   navigation is a table dump; the `graph.orphans` insight accuses live code (3/5 false on
   podcasts) because low coverage is reported as repo fact; the engine swallows its own failures
   (17 bare `catch{}`, 12 in SemanticLitePopulator) and the I8 snapshot cache **has never persisted
   a snapshot** (all cache dirs 0 bytes; fire-and-forget save + swallowed throw) — every question
   re-pays full analysis and nothing ever told us.

Prism's one sentence: **any .NET repo gets the lens that matches what it IS, on every surface — and
the lens is alive: visually intelligent, honest about its own coverage and health, instant on the
second question, and engaging while it works.**

Non-goals: no LLM in core; no Graph2 rewrite; no new app pages (reshape the existing five).

## 1. Delivery model & QA cadence (the rules)

- **Big deliveries, not micro-stages.** Each delivery D1–D5 is one focused mega-session in ONE part
  of the codebase (engine / engine / engine+server / app / server+infra), sized like a Tapestry
  stage-train day. All checkpoints inside a delivery land together on `feat/prism-d<n>`.
- **QA is a standing cadence, not a phase.**
  - *Session open (QA-back):* re-run the octet harness + affected poles against the PREVIOUS
    delivery's merge; fix regressions before new work. First 30–60 min, every session.
  - *Session close (QA-forward):* detached full gate battery + octet harness on the delivery tip;
    delivery merges to develop only on `GATE: PASS` + its own Definition-of-Done below. develop
    stays green after every delivery.
  - *QA artifacts:* every delivery ships fresh captures under `eval-results/<date>/prism-d<n>/`
    (maps/traces/screenshots/MCP transcript as relevant) — same evidence discipline as the audit.
- **Quality floor (nothing dropped):** every audit finding A–N maps to exactly one delivery
  (traceability table §3). Truth ratchets only tighten; Tapestry poles stay byte-identical unless a
  delivery's DoD says otherwise; screenshot gates guard every UI change; **no new bare `catch` in
  Core** (loom-guards ban lands in D1 with the octet harness).
- Tapestry rules carry forward otherwise (AGENTS.md T-rules, detached batteries, absolute paths,
  UPDATE_GOLDENS with review).
- **Model per session** (capability-matched, QA-back runs on the session's model):

  | Session | Model | Why |
  |---|---|---|
  | D1 | **Fable 5** | Highest-judgment engine work: archetype semantics ripple across 40+ fixtures; T8.2 showed how delicate this locus is. Wrong calls here are quiet and expensive. |
  | D2 | **Opus 4.8** (Fable if available) | Deep Roslyn work but well-specified; the razor/XAML call-graph design and insight-gating rules are the judgment spots — escalate those designs to Fable if they stall. |
  | D3 | **Fable 5** | Cache fix + waterfall are mechanical, but the merged-compilation persistence (invalidation correctness, Roslyn state serialization) is the phase's riskiest engineering — staff for the tail, not the average. |
  | D4 | **Sonnet 5** (Opus 4.8 for the canvas-system design pass) | High-volume Angular/component work against a detailed spec with screenshot gates — Sonnet's sweet spot. The one taste-critical piece (layered canvas system, M) benefits from an Opus first pass or review. |
  | D5 | **Sonnet 5** | CI yaml, per-RID publish, small MCP fixes, scripted QA re-runs — mechanical against explicit DoDs. |

## 2. The five deliveries

### D1 — "The repo is what it is": archetype truth + entry surfaces + style rungs (engine)
*One session. Everything that decides WHAT a repo is and WHAT enters it. Findings A1–A5, B1–B6,
C4, C6, E1–E5.*

**Harness first (it gates everything):** pin the octet SHAs in `eval/README.md`; aspirational
expectation rows for intended verdicts (Newtonsoft=Library, SE.Redis=Library,
wolverine=Framework-library, GitVersion=CliTool, podcasts hub+MAUI, ScreenToGif=Desktop-styled,
bitwarden per-service styles); `eval/lens-audit.ps1 <repo|octet>` (timed analyze → captures → MCP
drive → FAIL probes: map-tokens ≪ repo size, Unknown+0-entries, sample rows in per-service,
wall-time vs baseline); bare-`catch` ban in loom-guards.

**Archetype & render honesty:** transitive aux-exe references (A1); `toys`/build-tooling NoiseFilter
rungs (A2/A3); holder csproj excluded everywhere; topology applies per-service's filters (E2);
catalog self-name audit — every descriptor whose nuget id ≠ project names gets SelfNamePatterns,
wolverine first (A4); runnable-service inference honors NoiseFilter unless SamplesAreTheProduct
(A4); `Archetype.CliTool` (Exe + no web surfaces + PackAsTool/parser evidence → command-surface
render; plain `Main()` becomes an entry) (A3/B4); **render backstop — no dead maps** (0 entries +
public surface ⇒ library sections; + Main ⇒ console view; harness FAILs any <~400-token map on a
>100-file repo) (A5).

**Entry surfaces 2026:** in-framework SignalR via `MapHub<T>`/`: Hub` evidence, package-free (B1);
MAUI descriptor + `UseMaui`/TFM probe, pages/shell as UiEntries (B2); MapGroup prefix composition
into routes (B3 — fixes map, flows, Studio picker, MCP addressing at one locus); queue seams
(Storage-Queue/ASB/RabbitMQ senders + hosted consumers as `[approx]` channel edges) (B5); honest
branding — hand-rolled `IRequestHandler` ⇒ "CQRS (hand-rolled)" (B6); entry target attribution
fidelity (C6).

**Per-service style rungs (same detection locus):** owns-endpoints ⇒ Web API/MVC; owns-hubs ⇒
SignalR host; IdentityServer/OpenIddict ⇒ Identity provider; ViewModel+WPF ⇒ Desktop MVVM; `Api`
without dot; in-framework Razor-Pages probe (C4).

**Hygiene riders:** duplicate-name disambiguation (E3); TFM summarization (E5); skip
`Update=`/MSBuild-expression package refs (E1); remove the `--profile debug` ghost hint (E4).

**DoD:** octet expectation rows for archetype/style/entries flip aspirational→expected;
Newtonsoft/SE.Redis/GitVersion/wolverine render real lenses; podcasts hub+MAUI present; bitwarden
per-service ≤2/17 Unknown; zero bare-`/` grouped routes; MediatR-class repos + Tapestry poles
byte-identical; full battery GATE: PASS.

### D2 — "The graph doesn't lie": depth + self-health + insight validity (engine)
*One session. Everything that decides what the graph KNOWS and what we CLAIM from it. Findings
C1–C3, C5, I1–I2, J1, J3, G1-dependency, D3.*

**Depth:** `.razor` `@code` blocks into the call graph (C1); XAML code-behind/command wiring (C2);
type-node degree rollups — `neighbors`/`usages`/`impact` never dead-end on connected types (C3;
closes conductor-DEBT SymbolTable member indexing); DI provenance ranking (focus host first,
`[×N hosts]` otherwise) (C5).

**Self-health (silent-failure amnesty):** every bare `catch{}` in Core becomes catch-log-count via
a `PipelineDiagnostics` channel (extractor × failure count × sample exception),
SemanticLitePopulator's 12 first (J1); failure/skip/partial counters join the stage waterfall +
stats on all surfaces (J3).

**Insight validity:** coverage-gating — dead-code claims require an edge-coverage floor, exclude
entity-mapper-indexed and DTO-constructed types, and every insight carries its coverage basis in
its evidence line (I1); insight-validity harness — machine-checkable claims get checks, the rest
recorded manual verdicts, wired into `lens-audit.ps1` (I2). CLI trace budget enforced or relabeled —
no silent 3× breaches (D3).

**DoD:** podcasts UiEntry traces reach services; ScreenToGif traces ≥ depth 2; MCP
impact/usages/neighbors non-empty on connected types; Core bare-swallow count = 0; stats show
per-extractor failure columns; podcasts orphans list contains zero provably-live types; full
battery + octet GATE: PASS.

### D3 — "Instant and alive": cache resurrection + living waterfall + the compiler lever (engine+server+UI-lite)
*One session; the compiler lever is the flagged risky tail — it may spill into the D4 QA-back
without blocking D4. Findings J2, K1–K2, L2, L7, D1–D2, and github-ready's perf-lever thread.*

**Cache resurrection (do first — highest leverage-to-effort in the whole plan):** awaited save with
surfaced errors (kill the fire-and-forget at `AnalyzeCommand.cs:225` and the swallow at
`SnapshotCacheService.cs:205`); load verified end-to-end CLI + server; honest
`from cache · <sha> · <ms>` stamp; hit/miss/size in stats; cache round-trip truth test in the
battery; UI Freshness card states snapshot age/HEAD/location + re-analyze affordance (J2, L2).
Target before any compiler work: second CLI question on bitwarden <15s.

**Living waterfall:** the existing observer stream renders as a live, detailed waterfall —
per-stage bars with elapsed, current extractor, discoveries streaming in ("125 projects · 3 gRPC
services · 662 endpoints…"), skip reasons, D2's failure counters, honest big-repo expectations
(K1, L7 — as big as the data allows); post-analyze it persists as a Stats-page timeline (K2).

**Compiler lever (risky tail):** persist/reuse the merged compilation (T7.2: DntSite 81s of 95s;
bitwarden 207s), on-disk, keyed by source/package hash, invalidation truth-tested. With the cache:
cold bitwarden ~3.5min once, warm <15s, focus <10s (D1/D2).

**DoD:** cache round-trip green in battery; warm-run targets hit on the octet's big pole (or
compiler-lever remainder explicitly carried to D4 QA-back with cache targets still met); loading
screenshot gate; bench verdicts unchanged; full battery GATE: PASS.

### D4 — "A desktop worth demoing": visual intelligence + library workbench + Studio/nav loops (app)
*One session, pure `src/DevContext.App` (+ small server additions for the library surface RPC).
Findings F1–F6, L1, L3–L6, M, plus the UI half of D1's library archetype.*

**Visual intelligence:** ONE canvas system — deterministic layered layout (ELK/dagre-class),
fit-and-center that never clips, stable across pages (F2/L6); semantic rendering — kind glyphs,
transport-labeled edges (HTTP/queue/gRPC/event), DDD-layer lanes, stores/externals visually
distinct (F3/M — the engine already knows all of it); progressive disclosure — C4-ish level 1
(services + transports) default, expand per service (M); **Atlas becomes a real one-pager** —
layered architecture view, per-service cards (style + entry mix), event/queue board, data stores,
matching export (L3).

**Library workbench:** archetype Library routes Explore to a public-surface browser (ENTRY API /
ABSTRACTIONS / GENERATORS / PUBLIC SURFACE / CONSUMER PATHS as the rail); home cards swap
entry-metrics for surface-metrics; style chip suppressed exactly as the CLI does (F1).

**Studio & navigation:** **live pack preview** — the rendered pack (markdown, token meter,
per-section provenance) updates as scope/budget/intent change; Copy copies what's shown (L4);
preset semantics — explicit names + one-line effect, scope delta visible in preview (L4); **entry
browser replaces the table dump** — ranked, grouped service→kind→route, filter-as-you-type, kind
chips, auth badges; Shift+E table demoted to power view (L5, needs D1's B3); session naming
unified (F4); MCP feed origin fix — app RPCs never labeled `agent` (F5).

**DoD:** refit UI session shows the full library surface; scripted Studio drive builds a podcasts
pack entirely from the preview; screenshot gates on loading/home/Explore/Atlas for podcasts +
refit + eShop + bitwarden; a reviewer can answer "how does this system work" from Atlas alone;
`pnpm check` + app battery green.

### D5 — "Honest to agents, shipped everywhere": MCP polish + cross-platform + final hardening (server+infra)
*One session: the smallest delivery + the phase's final QA. Findings G1, H1–H3.*

**MCP honesty:** get_context degenerate-focus — low fill says why + suggests connected focuses;
UiEntry packs pull page members (lands free after D2's C1) (G1).

**Cross-platform delivery:** engine CI legs on ubuntu + macos (build + fast suite + CLI strict
smoke) or the README/PACKAGING claim narrows explicitly (H1); path/casing fixes it shakes out
(H2); desktop bundle decision — per-RID sidecar + Tauri mac/linux targets, or documented
Windows-only (H2); installer version from the release tag (H3).

**Final hardening (phase QA):** full octet + insight-validity + poles re-run; clean-clone battery;
cross-OS CI green on develop; HANDOVER-PRISM.md; tracker closed.

**DoD:** octet MCP drive has zero empty navigations / silent breaches; green cross-OS CI on
develop; release dry-run artifact inventory matches the bundle decision; phase handover written.

## 3. Traceability (nothing dropped)

| Audit finding | Delivery |
|---|---|
| A1–A5 archetype/render · B1–B6 surfaces · C4 style · C6 targets · E1–E5 hygiene | **D1** |
| C1–C3 depth · C5 DI provenance · I1–I2 insights · J1/J3 self-health · D3 trace budget | **D2** |
| J2 dead cache · K1/K2 unused riches · L2/L7 cache-UI/loading · D1/D2 perf · compiler lever | **D3** |
| F1–F6 UI · L1/L3–L6 experience · M diagram intelligence | **D4** |
| G1 MCP pack honesty · H1–H3 cross-platform · phase QA | **D5** |

Sequencing rationale: D1→D2 are both Core and D2's insight gating needs D1's honest graphs; D3's
waterfall shows D2's failure counters; D4's entry browser needs D1's route prefixes and its
library workbench needs D1's archetype; D5's pack fix needs D2's razor depth. QA-back at every
session open catches cross-delivery regressions within one session of their landing.

## 4. Debts folded in / still open
- conductor-DEBT: SymbolTable member indexing → D2; BodyFacts scoping, TfmScore, Flow hardening
  remain open (unchanged).
- eval-results/ volume + analysis-exports/ — owner call, parked.
- Fast-suite load-flake — name still uncaptured; watch the D-session batteries.
- GrpcAggregator style rung — subsumed by D1's style rungs.

## 5. Audit-process improvements (encoded in D1's harness, learned this round)
- Unseen-first, rotate the octet; cross-surface drives per repo; judge against "what would an
  honest lens say"; FAIL probes (map-tokens ≪ repo size, Unknown+0-entries, sample-noise rows).
- Verify a sample of insights against code every round — insights are claims, not decoration.
- Audit the engine's own health, not just its output: swallow census, dead-feature checks (does
  the cache dir contain bytes?), collected-vs-surfaced inventory.
- Experience is a first-class dimension: loading, diagrams, core loops, navigation — graded
  against "would this demo well", screenshots as evidence.
- Playwright: `domcontentloaded` (MCP page never idles); explicit New-session per repo; launch
  node drives from PowerShell (MSYS mangles `cmd /d /c`).
