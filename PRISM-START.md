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

last: 2026-07-17 **D1 session 3 — ALL remaining D1 checkpoints implemented + committed.** Ack: INBOX
2026-07-17 standing orders (unchanged; re-checked at boundaries). Session opened on the disk-full
blocker: **~35 GB freed, pending boundary citation LANDED** — `gates.ps1 -Scope engine -SkipMcpQa`
vs `e8ca7cc`: **GATE: PASS** (`prism-d1/d1.2-fix2/gates-engine-rerun.txt`), closing D1.2-fix2.
Then, per-checkpoint (each with cheap gates green + live CLI validation + unit tests; commits
pinned in the rows): **D1.2c** `2c4b47a` (caller-prefix index across the ext-method boundary;
podcasts zero bare grouped routes) → **D1.2e+D1.4b/c/d** `7330dff` (hand-rolled CQRS branding;
TFM split+summary; Include=-only package refs; ghost-hint removal) → **D1.2d** `3723b53` (queue
seams: podcasts `feed-queue [AzureStorageQueue]: Podcast.API → Podcast.Ingestion.Worker` wire;
eShop/dogfood re-driven unmoved) → **D1.3a** `42eaaed` (per-service rungs: bitwarden 17/17→1/16
Unknown; ScreenToGif `DesktopMvvm`) → **D1.2f** `27206da` (typed-client target fidelity:
`PUT /feeds/{id} → FeedClient.AddFeedAsync`; domain ports keep contract display — eShop unchanged)
→ **D1.4a** `373b1b5` (dup-name disambiguation + D1 ratchet flips). **Whole-cohort eval sweep
44/44 GREEN 15m44s (`prism-d1/cohort-sweep-1.txt`)** with the flipped expectations enforced; close
sweep + octet lens-audit are the last two gates before `D1 DELIVERY CLOSED`.
**One VERDICT REVERSED (insight-validity):** the audit's C6 "misattributed" `ShowClient.CheckLink`
is TRUE (ShowsApi.cs:37 calls it per show) — the confusion was the 5-way bare-route collision
D1.2c fixed; the podcasts expectation row now pins the composed route + true target.

**D1 SESSION 3 CLOSED — D1 is CODE-COMPLETE, all checkpoint rows VERIFIED (close sweep 44/44 GREEN
15m42s, `prism-d1/cohort-sweep-close.txt`).** Session 4: the detached octet DoD proof came back
with **2 REAL FAILs** — Newtonsoft.Json + SE.Redis `mcp-drive: map is trivial (~60 tokens)`. Root
cause was an MCP-surface fidelity gap, not a harness quirk: the MCP `map` tool dropped
`MapResponse.markdown` (field 1!) and rendered only style+topology, so a 1400-node library read as
a ~60-token dead map over MCP while the CLI rendered 2000+. Fixed as **D1.5 @ `4d0f837`** (see
table); octet relaunched on the fixed binaries came back **LENS-AUDIT: PASS 8/8**
(`prism-d1/octet-dod-proof-2.txt`) — bitwarden 205.5s (cap 522s), all archetypes intended,
ScreenToGif `DesktopMvvm`, GitVersion `CliTool`. App check green over the regenerated proto
bindings (lint + 49 vitest + build).

last: 2026-07-17 **D2 session 1 (feat/prism-d2) — D2.0 + D2.0b both VERIFIED.** Ack: INBOX
2026-07-17 standing orders (D1-scoped; no D2 entries — general discipline carried). D2.0: the
uncommitted tier-knob code found at session open verified GREEN twice detached; run 1 exposed the
unbalanced split → weight-hinted (`$slowFirst`) rebalance + the honest finding that "<5 min" is
floor-limited (~6 min/2 hosts) while shamshir+dntsite ride — row has the numbers. D2.0b: J2 cache
RESURRECTED (row has the full mechanism): `SnapshotPersistence` serializable subset, polymorphic
detections, awaited saves ×3 sites with surfaced errors, dirty-fingerprint version keys (a working
cache keyed on bare HEAD would have served stale maps — closed before it ever shipped), litter-free
probes, honest stamp + stats hit/miss. Live: dogfood 8.1s→2.5s, sha-identical, HIT; McpQa 12/12
rode the rehydrated snapshot (server load live-proven, meta touch during the gate run). Riders
found while verifying, all closed in-commit: `--dry-run` must bypass the cache READ (a cached full
map is not a preview); `DEVCONTEXT_CACHE_ROOT` env override + ServerTestFactory temp-root redirect
(server tests were writing the USER cache, and AnalyzeFlowTests would have cache-HIT into failing
its streamed-progress assert on any unchanged-tree re-run). Working-tree leftover: `lens-run/`
(lens-audit scratch from D1 session 4) left untracked for the eval-results-clutter owner call.
**D2 SESSION 1 CLOSED — continue at C1 (`.razor` `@code` into the call graph, proposal §2-D2;
mind the T-era markup-as-C# perf trap: @code extraction only, never whole-file Razor parses).
Then C2/C3/C5 → J1/J3 → I1/I2 → D3-budget, per the D2 spec.**

last: 2026-07-17 **D2 session 2 — C1 + C2 + C3 all VERIFIED (rows above, commits e018072/76da490/c5798d4).**
Ack: INBOX unchanged (D1-scoped standing orders; general discipline carried). Owner mid-session
directive: **"rate of delivery has been low for this plan"** — cadence tightened from C2 on
(implement → unit test → ONE targeted drive → commit; sweeps at boundaries only, per §⚡ rule 3 —
C1 ran the one quick-tier gate: GATE: PASS 57+30 green incl. new blazor-surface fixture).
The graph-depth trio is done: Blazor @code virtualized (markup never parsed, #line-honest,
_Imports usings → [verified] edges), desktop entries member-linked, GraphQuery type rollups on
every query surface. Riders: snapshot schema v2→v3 (engine change must not serve pre-C1 maps —
bump discipline until J2 grows an engine-version key). Known-latent (noted in C1 row): http
ServiceLink provenance-tied pair order flaps run-to-run (dogfood, pre-existing) — determinism
rider for J1/J3. Session artifacts under `prism-d2/c1/` (incl. dntsite perf A/B 115.1→118.9s).
**D2 SESSION 2 CLOSED — continue at C5 (DI provenance ranking: focus host first, `[×N hosts]`
otherwise), then J1/J3 → I1/I2 → D3-budget per the D2 spec.**

last: 2026-07-18 **D2 session 3 — C5 + J1/J3 + I1/I2 all VERIFIED (rows below, commits
32d5f1d/…).** Ack: INBOX unchanged (D1-scoped standing orders; general discipline carried). Owner
cadence directive honored: implement → unit test → ONE targeted drive → commit per checkpoint;
**ONE batch gate for the whole session** (`gates.ps1 -Scope engine -EvalTier quick`, detached,
overlapped with I1/I2 work): **GATE: PASS** — build 0w/0e, fast tests, MCP QA 12/12, eval quick
split green (stamp not written), CLI matrix + query ops
(`prism-d2/gates-batch-c5-j1j3.txt`); I1/I2 landed after the gate's build, covered by its own 31/31
unit-test run + podcasts drive + app lint/49-vitest (proto regen diligence). Four D2 DoD lines now
ticked: bitwarden DI provenance (C5 row), **Core bare-swallow count = 0** (J1), **stats show
per-extractor failure columns** (J3), **podcasts orphans zero provably-live types** (I1 — cards=0,
floor-honest). Known-latent carried: NameResolver short-name collision picks a test-fixture FQN for
impl TYPE node identity (C5 row); http ServiceLink pair-order flap (C1 row) still open for the
determinism thread. Gate noted `analyze --format html --strict` exit 2 (self-check failures) as a
pre-existing allowed state — not chased.
**D2 SESSION 3 CLOSED — continue at D3-budget (CLI trace budget enforced or relabeled, no silent
3× breaches — the last open D2 checkpoint), then D2 close: full-cohort sweep + remaining DoD
(MCP surface drive rides the octet).**

last: 2026-07-18 **D2 session 4 — D3-budget VERIFIED @ 911f734 (row below), then D2 CLOSE ran.**
Ack: INBOX unchanged. Close battery: full-tier engine gate **GATE: PASS, stamp written** (84 eval
green both hosts, poles rode, 3 pre-existing truth-ratchet skips — `prism-d2/gates-d2-close-full.txt`)
→ octet DoD proof run 1 came back **7/8 with P7's FIRST REAL CATCH**: wolverine (Library) orphans
claim named `WolverineValidationResult`, provably live as the element type of a public `List<T>`
base — the I2 harness caught an I1 escape the day it shipped. Fixed as **I1-fix @ 4397e69**:
libraries make NO dead-code claims (external consumers invisible to the graph) + type-ref identifier
harvest includes generic arguments; snapshot v4→v5 (a v4 snapshot can serve a persisted claim the
fixed engine would never make — the stale-claim risk was live-observed on the wolverine cache HIT).
5/5 orphans unit tests; wolverine leg fresh-PASS at v5 (56.1s); **octet relaunched on the fixed
binary: LENS-AUDIT: PASS 8/8** (`prism-d2/octet-dod-proof-2.txt`, bitwarden 151.5s fresh, all
archetypes intended, P7 quiet octet-wide). Honesty note: the full-tier sweep ran at the pre-fix
commit; the fix's blast radius (one insight source + schema constant, nothing expectation-pinned)
is covered by fast suite + truth gate + the 8/8 octet — formal re-sweep rides phase-end QA.

## D3 — OPEN (feat/prism-d3, cut 2026-07-18 off 4355417)

**Scope carve (owner directive, 2026-07-18 session 1): D4 may run IN PARALLEL in another
session+worktree.** To keep the surfaces disjoint: D3 keeps ENGINE+CLI+SERVER only; D3's "UI-lite"
items (L2 Freshness card, L7 loading-waterfall UI, K2 Stats-page timeline UI) are HANDED TO THE D4
SESSION (they are `src/DevContext.App` work and D4 owns those pages). D3 plans NO proto change
(`StatsResponse.stages`/`total_wall_ms` already exist), so the proto→TS-regen collision surface is
zero. D4 mechanics: cut `feat/prism-d4` off the CLOSED D2 tip (4355417) in a worktree; rebase onto
the d3 tip when D3 closes; D4's one proto-touching item (library-surface RPC) lands AFTER that
rebase. Don't run full gate batteries in both sessions at once (build-DLL locks + orphaned-server
gotcha); `eval-repos/` is machine-local — the D4 worktree reaches it by absolute path.

### D3.1 — query ops ride the snapshot cache (J2 remainder; the "parked smell" made load-bearing)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D3.1 | `query` CLI ops load/save the J2 snapshot cache instead of re-paying full analysis per question. **Flavor-keyed poison-proofing (new, found while wiring):** the version key now carries the analysis FLAVOR when it deviates from the default full-fidelity run (`-opt-<12hex>` over AllowRoslyn/BuildFullGraph/Fast/ExcludeExtractors/ExcludePatterns) — closes the pre-existing D2.0b hole where a `--fast`/`--lite`/`--no-roslyn` save could be HIT by a full run; default flavor keys UNSUFFIXED so CLI analyze, CLI query, and the server share one slot per (repo, tree). Query options aligned to default-analyze flavor (config excludes, EntryPaths, ScopedProjectDirs — query previously diverged from analyze on all three). Honesty: stamp is CONSOLE-ONLY stderr (gates/lens scripts run PS 5.1 EAP=Stop where redirected native stderr throws — stdout stays pure JSON); machine surface = `query stats` JSON `snapshotCache` field. `--no-cache` added to query; **lens-audit P7 passes it** so insight-validity keeps recomputing independently instead of validating the snapshot it audits (the freshness thread the D2 close flagged) | VERIFIED | (this) | prism-d3/d31/ — podcasts miss·saved 8s → HIT 1s; **bitwarden second question 4.2s (DoD target <15s ✓)**, third op (usages CipherService, 43 callers) 4.0s; cross-surface BOTH directions: query saved 3e79593 → `analyze` re-drive `from cache · 3e79593 · 3491ms`; fresh-vs-rehydrated stats JSON byte-identical modulo the honest snapshotCache field; 7/7 SnapshotCacheTests (2 new flavor-key tests: default flavor unsuffixed + matches optionless overload; fast/lite/no-roslyn/excl each distinct; suffix order-insensitive); fast suite 599+15 green |

### D3.2 — K1 living waterfall (CLI half; the app loading screen is D4's per the carve)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D3.2 | Interactive `analyze` renders the observer stream as a LIVE waterfall (Spectre Progress): one ticking row per pipeline stage (elapsed column), the extractors currently running (2 names + `+N`), per-stage `+Nt +Nd`/skip counts, stage extras (`N signals`, `~A→~B tok`), and a cumulative discoveries TICKER; skip/fail detail stays in --stats. Structure: `WaterfallModel` (Core/Observers — pure, thread-safe, console-free aggregation = the testable UI contract) + `WaterfallDiscoveryObserver` (CLI Spectre binder); diagnostics buffered during the live region and replayed after (writing through a live display corrupts it). L7's CLI half: an uncached run says up front "first analysis can take minutes on a large repo; the result is snapshotted for instant re-runs". Non-interactive runs keep the OLD line stream byte-stable (harnesses unaffected); `DEVCONTEXT_WATERFALL=on\|off` overrides the TTY probe (on = capturable fallback rendering for drives) | VERIFIED | (this) | prism-d3/k1/ — forced-fallback drive on podcasts shows the contract live: ticker `230 types · 582 detections · 15 extractors · 13 skipped`, active row `Extract (specific) · +0t +273d · 13 skipped · running EndpointExtractor, EventBusExtractor +1`, stage rows Semantic upgrade/Assemble graph; 4 WaterfallModelTests (running-list cap+overflow, completion bakes counts + clears running, cumulative ticker, extras, no-active-stage no-ops); fast suite 603+15 green. **Honesty note: the true interactive TTY appearance can't be machine-verified from this harness — the fallback render + model tests pin the content; owner eyeball invited on next interactive analyze** |

### D3.3 — K2 timeline persistence + stats surface (server StatsResponse.stages already existed)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D3.3 | The analyze-time waterfall PERSISTS and rides the stats surfaces. Two gaps closed: (1) QueryCommand ran a `NullDiscoveryObserver`, so the pipeline's `collector?.Build()` returned the EMPTY report — query-originated snapshots persisted no timeline; query now passes a real `RunReportCollector` (composite), so every snapshot carries the stage waterfall regardless of which surface produced it. (2) `query stats` JSON gains `totalWallMs` + `stages[{stage,ms}]` — the timeline of the run that PRODUCED the snapshot (a cache HIT serves the original run's timings verbatim; empty only on pre-D3.3 snapshots, which honestly recorded nothing). Server side needed NO change: `StatsResponse.stages`/`total_wall_ms` (proto fields 1/8) already flow from `snapshot.Report`, which the J2 cache persists — the app Stats-page timeline (D4's per the carve) has live data on both fresh and rehydrated sessions. gates.ps1 Step 4b stats probe now REQUIRES a non-empty stages timeline (fresh fixture ⇒ never empty) | VERIFIED | (this) | prism-d3/k2/ — dogfood fresh query: 9 stages recorded by query's own collector (Discovery 123ms → SemanticLite 2008ms → … wall 4239ms), saved; re-run `HIT · 7e43d80` serves the IDENTICAL persisted timeline (stages+wall compared equal); fast suite 603+15 green, build 0w/0e |

# ✅ D2 DELIVERY CLOSED (2026-07-18, session 4)

### D2 Definition of Done (proposal §2-D2) — ALL TICKED at close
- ✅ podcasts UiEntry traces reach services (C1: `GET /discover → PodcastService.GetShows`, razor-line provenance)
- ✅ ScreenToGif traces ≥ depth 2 (C2: ExportPanel depth 3+)
- ✅ MCP impact/usages/neighbors non-empty on connected types (C3 GraphQuery locus; octet P6 MCP drives 8/8)
- ✅ Core bare-swallow count = 0 (J1: loom-guards allowance table EMPTY, PASS)
- ✅ stats show per-extractor failure columns (J3: analyze Fails column + query stats + gRPC/MCP proto field)
- ✅ podcasts orphans list contains zero provably-live types (I1: cards=0 floor-honest; P7 enforces octet-wide since I1-fix)
- ✅ Close battery: full-tier gate GATE: PASS (stamp written) + octet LENS-AUDIT: PASS 8/8 on the
  final binary (full phase battery = phase-end QA per operating model)

**Continue at D3 (delivery): cache resurrection remainder + living waterfall + compiler lever —
J2 landed early (D2.0b), so D3 opens on K1/K2 (analyze-time waterfall show), L2/L7, D1/D2 perf
lever (persist/reuse merged compilation), per proposal §2-D3. Note for D3: `query` CLI ops still
bypass the snapshot cache (parked smell, natural D3 item — P7's recompute honesty currently
DEPENDS on it, so wiring query ops to the cache must keep insights fresh or bump-disciplined).**

# ✅ D1 DELIVERY CLOSED (2026-07-17, session 4)

## ⚡ Pipeline-speed adjustment (owner directive at session-3 close: "testing/pipeline is slow —
adjust it properly for the long plan"). Measured this session and ENCODED for D2:

- **Facts:** whole-cohort in-process eval = **~16 min SERIAL** via bare
  `dotnet test --filter Category=Eval` (this session paid it TWICE = 32 min wall); the octet's
  heavyweights (bitwarden ~4-5 min, ScreenToGif, wolverine, Newtonsoft, shamshir) dominate; every
  run RE-PAYS full analysis because the I8 snapshot cache has never worked (J2, planned D3);
  engine-scope battery ≈ 19 min.
- **Immediate rules (no code):** (1) NEVER sweep via bare single-host `dotnet test` — use gates.ps1
  Step 3's TWO-HOST SPLIT (~halves it) or run the two filter halves yourself; (2) exploit the eval
  STAMP (`eval/.eval-stamp.json`): a green sweep transfers while Core/CLI/expectations/fixtures are
  unchanged — don't re-run for docs-only commits; (3) keep per-checkpoint targeted CLI drives (fast)
  and reserve cohort sweeps for checkpoints touching archetype/signal/evidence, as the LESSON says.
- **D2.0 (new first checkpoint of D2 — harness, ~1h):** add a cohort TIER knob to the eval harness:
  QUICK tier (small/mid repos + BOTH poles, target <5 min split) for mid-delivery checkpoint sweeps;
  HEAVY tier (bitwarden, ScreenToGif, wolverine, Newtonsoft.Json, SE.Redis) joins only at
  delivery-close sweeps. Wire gates.ps1 Step 3 to accept the knob. The close sweep stays full-cohort.
- **D2.0b (PLAN RESEQUENCE, recommended under the owner's adjust directive):** promote D3's **J2
  cache resurrection** (awaited save with surfaced errors, verified load CLI+server, honest
  `from cache` stamp, hit/miss in stats — NOT the compiler lever, which stays D3) into D2 as an
  early checkpoint. Rationale: proposal calls it "highest leverage-to-effort in the whole plan";
  D2's own sweeps + everything after immediately stop re-paying cold analysis. The D2 session should
  do D2.0 → D2.0b → then the planned C1/C2/C3… train.

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
| D1.2c | B3 MapGroup prefix composition into routes (fixes map, flows, Studio picker, MCP addressing at one locus) | VERIFIED | 2c4b47a | prism-d1/d1.2c/podcasts-map.txt — ALL grouped routes composed (`GET /shows/`, `GET /categories/`, `POST /feeds/`, `PUT /feeds/{id}`…); the one remaining `GET /` is ListenTogether.Hub's REAL root route. Mechanism: repo-wide caller-prefix index (`shows.MapShowsApi()` AND inline `app.MapGroup("/x").MapXApi()` chains) seeds the extension method's receiver param; single-distinct-prefix + never-called-bare rule keeps ambiguous methods honest; receiver-aware nesting (`var v1 = api.MapGroup("/v1")` → `/api/v1`); ext-method scan now runs BEFORE the whole-file scan so the composed route wins the file:line dedup. 6 unit tests (cross-file, inline chain, ambiguous→bare, mixed→bare, nested seed, receiver nesting). CLOSE-SWEEP 44/44 GREEN (cohort-sweep-close.txt) |
| D1.2d | B5 queue seams: Storage-Queue/ASB/RabbitMQ senders + hosted consumers as `[approx]` channel edges | VERIFIED | 3723b53 | prism-d1/d1.2d/ — podcasts event board: `EVENT WIRING (1 integration, 1 cross-service)`: `feed-queue [AzureStorageQueue]: Podcast.API → Podcast.Ingestion.Worker` + CROSS-SERVICE `bus (1)` (was "No events detected"). Mechanism: EventBusExtractor queue-seam phase (package-gated) — send/receive verb sites per transport joined by channel (site literal, else the repo's single `new QueueClient(…, "name")` literal, else unresolved); publishers → EventFlowDetection `Publish`, hosted consumers → MessageConsumerDetection; GraphBuilder adds the Raises half onto the shared channel node (Resolution.Syntactic = [approx]); a type doing BOTH directions on one transport is the bus IMPLEMENTATION (eShop EventBusRabbitMQ) and is dropped whole; a hosted-worker consumer keeps its Background entry (no dup Bus row). **Poles re-driven live: eShop 1089/837/109 + 13 events, dogfood 439/339/34 Microservices + gateway(4) — both unmoved.** 3 unit tests (podcasts shape, eShop guard, bitwarden split-classes join). CLOSE-SWEEP 44/44 GREEN (cohort-sweep-close.txt) |
| D1.2e | B6 honest branding — hand-rolled `IRequestHandler` ⇒ "CQRS (hand-rolled)" | VERIFIED | 7330dff | podcasts STACK now `CQRS (hand-rolled mediator)` (was `MediatR (CQRS)` with zero MediatR refs — d1.2c/podcasts-map.txt). Discriminator: repo DECLARES `IRequestHandler` itself ⇒ HandRolled; package signal or impls-without-local-declaration (G7 scoped-sub-project) ⇒ Package. Style evidence strings follow (`hand-rolled mediator with N handlers`); scoring unchanged. 4 unit tests. CLOSE-SWEEP 44/44 GREEN (cohort-sweep-close.txt) |
| D1.2f | C6 entry target attribution fidelity (`GET / → ShowClient.CheckLink`; interface-as-target) | VERIFIED | 27206da | prism-d1/d1.2f/ — podcasts `PUT /feeds/{id}` → **`FeedClient.AddFeedAsync`** (was bare `IFeedClient`). Mechanism: `AddHttpClient<TInterface,TImpl>` is now a real DirectBinding (was invisible to DI) → the call-graph DI map resolves interface member calls to the impl; plus a `http-client-binding` tag on the Resolves edge + Type-kind target substitution in ResolvePrimaryCall as backstop. **Principled scope: typed-CLIENT interfaces only — domain ports keep interface-as-contract display, so eShop's `GET /api/orders/cardtypes → IOrderQueries` is UNCHANGED (byte-diffed).** The audit's other exemplar `GET / → ShowClient.CheckLink` was code-read verified TRUE attribution (ShowsApi.cs:37 calls showClient.CheckLink per show) — its confusion was the 5-way bare-route collision D1.2c fixed; now renders per-route. CLOSE-SWEEP 44/44 GREEN (cohort-sweep-close.txt) |

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
| D1.3a | owns-endpoints ⇒ Web API/MVC; owns-hubs ⇒ SignalR host; IdentityServer/OpenIddict ⇒ Identity provider; ViewModel+WPF ⇒ Desktop MVVM; `Api` without dot; in-framework Razor-Pages probe | VERIFIED | 42eaaed | prism-d1/d1.3a/ — bitwarden 17/17 Unknown → **1/16** (`Api: Web API`, `Identity/Sso: Identity provider [IdentityServer]` via 1-hop transitive pkg, `Notifications: SignalR host` (name-hinted hub-dominance — it owns aux controllers too), Admin/Billing/Events/Icons/Scim: MVC via endpoint OWNERSHIP, `*Utility`: CLI, bare `AppHost`: Aspire AppHost, MicroBenchmarks filtered as benchmark; only Setup left Unknown). ScreenToGif: **STYLE `DesktopMvvm` (new enum member, fallback-only rung: UseWPF/WinForms probe + ≥3 ViewModels)** + per-service `Desktop (MVVM) [WPF/WinForms]`. **Sanctioned pole delta (documented, not suppressed): eShop `Identity.API` now `Identity provider [IdentityServer]` (was `Web API [EF Core]`) — strictly truer; counts/style/targets unchanged (1089/837/109 byte-diffed otherwise).** CLOSE-SWEEP 44/44 GREEN (cohort-sweep-close.txt) |

### D1.4 — Hygiene riders (E1, E3–E5)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.4a | E3 duplicate-name disambiguation (`Messages` ×6, `AppHost` ×2) | VERIFIED | 373b1b5 | prism-d1/d1.4a/ — `MapBuilder.DisambiguateNames`: a duplicated short name gets its nearest non-echo ancestor dir as qualifier, widened until distinct (`GitVersion.Configuration (src)` renders live; wolverine's `Messages` ×6 already dead via D1.1's sample filtering — the mechanism + unit test cover the general case). Unique names stay bare so no other repo's topology moves. CLOSE-SWEEP 44/44 GREEN (cohort-sweep-close.txt) |
| D1.4b | E5 TFM summarization (no raw TFM matrices in STACK) | VERIFIED | 7330dff | `ParseTargetFrameworks` SPLITS `;`-joined values into real TFMs (root cause: the whole matrix travelled as ONE token); STACK renders ≤3 verbatim (poles byte-safe) else 2 most-modern + `+N more TFMs` ranked family-then-version (podcasts: `net7.0, net7.0-android +2 more TFMs`). Multi-TFM extractor test re-pinned to split behavior. CLOSE-SWEEP 44/44 GREEN (cohort-sweep-close.txt) |
| D1.4c | E1 skip `Update=`/MSBuild-expression package refs | VERIFIED | 7330dff | `ParsePackageReferencesCpmAware` takes `Include=` ONLY (an `Update=`-only element is an MSBuild metadata patch, not a dependency — GitVersion's `@(PackageReference)` "package") + MSBuild-expression names (`@(`/`$(`) filtered; DependencyExtractor keeps Update as SIGNAL evidence but drops expressions. CLOSE-SWEEP 44/44 GREEN (cohort-sweep-close.txt) |
| D1.4d | E4 remove the `--profile debug` ghost hint | VERIFIED | 7330dff | Both hint sites rewritten (DiscoveryPipeline deep-dive diagnostic + empty-trace NOTE) — the flag is hidden on analyze and absent on query, so naming it was unactionable. No test/golden pinned the old text. CLOSE-SWEEP 44/44 GREEN (cohort-sweep-close.txt) |

### D1.5 — MCP lens parity (octet DoD FAILs, audit A5/G)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.5a | MCP `map` returns the rendered map, not a topology stub | VERIFIED | (this) | Octet DoD proof FAILed Newtonsoft + SE.Redis on `mcp-drive: map is trivial (~60 tokens)`: the tool serialized style/archetype/topology and DROPPED `MapResponse.markdown` — the server renders the full map per request and it dies at the MCP boundary. Tool now emits `markdown` + `solutionName` alongside the structured fields. Live re-drive: Newtonsoft map ~60→**~2535 tok**, SE.Redis ~77→**~2136 tok**, both repos LENS-AUDIT PASS |
| D1.5b | `overview` names the product, not its test console | VERIFIED | (this) | `Library: Newtonsoft.Json.TestConsole` / `Library: ConsoleTestBaseline` — the headline joined ServiceMap runnables (a library's are its test/sample hosts) onto the archetype. Additive proto `MapResponse.solution_name` (from `Model.Solution.Name`, same truth as the CLI's `LIBRARY` header); library headline now `Library: <product>` + demoted `hosts:` line; non-library headline unchanged. Server test pins `SolutionName == "ControllerApp"`; live: `Library: Newtonsoft.Json` / `hosts: Newtonsoft.Json.TestConsole`, `Library: StackExchange.Redis` / `hosts: ConsoleTestBaseline` |

### D1 Definition of Done (from proposal §2) — ALL TICKED at close, session 4
- ✅ Octet expectation rows for archetype/style/entries flip aspirational→expected (ratchets
  flipped at D1.4a `373b1b5`; enforced by close sweep 44/44 ×2).
- ✅ Newtonsoft / SE.Redis / GitVersion / wolverine render real lenses (octet-dod-proof-2.txt:
  2355/1955/438/6293 map tokens, all archetypes intended).
- ✅ podcasts hub + MAUI present; bitwarden per-service 1/16 Unknown (≤2/17 required); zero
  bare-`/` grouped routes (D1.2c).
- ✅ MediatR-class repos + Tapestry poles byte-identical (two sanctioned, documented eShop text
  deltas — see Baseline drift table).
- ✅ Per-commit cheap gates green throughout; `eval/lens-audit.ps1 octet` run at close as the DoD
  proof: **LENS-AUDIT: PASS (8 repos), `prism-d1/octet-dod-proof-2.txt`** — first run caught 2
  real MCP-surface FAILs, fixed as D1.5, rerun 8/8. (Full battery deferred to phase-end QA —
  operating model above.)

## D2 — OPEN (feat/prism-d2, cut 2026-07-17 off 76f4029)

Opens with the pipeline-speed adjustment (§⚡, owner directive), then the proposal's C1/C2/C3 train.

### D2.0 — Cohort tier knob (harness)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D2.0 | `gates.ps1 -EvalTier quick`: Step 3 minus the 5 heavy repos (bitwarden-server, screentogif, wolverine, newtonsoft-json, stackexchange-redis), poles always ride, LOUD exclusion print, stamp NEVER written on quick green, verdict self-labels not-a-merge-gate (both split + `-SerialEval` paths) | VERIFIED | 9b94393 | TWO green runs, stamp deleted before + absent after both: `prism-d2/gates-d20-quick.txt` (exclusion print, `stamp NOT written` label, GATE: PASS) and `gates-d20-quick2.txt` (same, on the weight-balanced split). **Split rebalance rider:** run 1 exposed the alternating-alphabetical split stacking shamshir-pole+dntsite+dotnet-podcasts in one bucket (halves 3m56 vs **8m28**) — Step 3 now seeds a `$slowFirst` heaviest-first list (LPT-style round-robin, alternation starts at B since host A also carries the non-expectation eval classes) before the alternating fill. **Timing truth:** quick-tier SERIAL total ≈ 12.4 min, so ~6 min is the 2-HOST FLOOR while shamshir (pole, must ride) + dntsite ride — the §⚡ "<5 min" aspiration is not reachable by splitting alone; run 2's halves (10m53/7m41) were inflated by this session's own interactive load on the same box, not by the rebalance. The real lever is J2 (D2.0b) + reserving quick sweeps for checkpoints that need them |

### D2.C1 — `.razor` `@code` into the call graph (audit C1, proposal §2-D2 Depth)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| C1 | Blazor UiEntries stop dead-ending: @code-only virtualization → component types + call edges + lifecycle-member entry links; targets/reach/trace light up | VERIFIED | (this) | prism-d2/c1/ — podcasts `GET /discover  → PodcastService.GetShows` (was the audit's score-0/reach-0 exemplar); trace `ENTRY → DiscoverPage.OnInitializedAsync → GetShows [verified] → data Show` with call site at the TRUE razor line (DiscoverPage.razor:72, #line-mapped) — [verified] because _Imports @using ride into the virtual tree; eShop `GET /cart` traces 4 deep into `BasketState/BasketService [verified]` (counts 1089/837→1139/870, entries 109 unchanged); dogfood BYTE-IDENTICAL (c798bb7, no razor); dntsite pins hold, perf A/B same-box: 115.1s pre / 118.9s post (~3% on the cohort's most razor-heavy repo — no T-era trap); quick-tier gate GATE: PASS (57+30 eval green incl. new blazor-surface fixture); 13 unit tests + eval/fixtures/BlazorSurface; snapshot schema v2→v3 rider (an unchanged repo must not serve the pre-C1 map — bump discipline noted until J2 grows an engine-version key). **Known-latent noted:** http ServiceLink pair sharing one provenance anchor flaps order run-to-run (dogfood Shopping.Web↔YarpApiGateway lines; pre-existing, the D1.2-fix2 divergence thread) — determinism rider candidate for the J1/J3 self-health strand |

### D2.C2 — XAML code-behind/command wiring (audit C2)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| C2 | Desktop UiEntries link at MEMBER level: RelayCommand → its exact command member; Window/UserControl → ctor + event-handler-shaped members (`(object sender, …EventArgs e)` — the signature IS the XAML wiring convention, no XAML parse); type-node link kept only as no-evidence fallback | VERIFIED | (this) | prism-d2/c1/stg-trace-exportpanel.txt — ScreenToGif `ExportPanel` was the audit's "traces to itself, 3 lines"; now `ENTRY → ctor + Panel_Loaded + TypeComboBox_SelectionChanged… → LoadPresets → GeneratePresets/PersistPresets` (depth 3+, DoD "≥ depth 2" met), wall 22.1s (stg-map.md). Desktop-affected in-proc evals green (desktop-app, communitytoolkit, compositionapp, maui-surface, eshop — 8/8); fast suite 587+15; loom-guards PASS. eShop ClientApp `[RelayCommand]` targets may deepen from owning-type to real collaborators — expected text delta class, hand-diffed at the next boundary sweep |

### D2.C3 — Type-node degree rollups (audit C3, conductor-DEBT SymbolTable member indexing)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| C3 | `GraphQuery` member rollup: on a Type node, `node`/`neighbors`/`usages`/`impact` + the resolver's most-connected tiebreak include the members' CROSS-TYPE edges (intra-type helper wiring stays internal); EdgeRefs keep true member endpoints so the answer names WHICH member collaborates. One locus — CLI query, gRPC server, and MCP tools all route through GraphQuery | VERIFIED | (this) | Live: `query usages --focus PodcastService` on podcasts 0 → **9 callers** (DiscoverPage.OnInitializedAsync/FetchShows, CategoryPage, Landing.OnGet…) — the audit's exact "impact up = 0 while IS the target of GET /landing" exemplar; unit test pins ResolveNodeId→Type, InDegree>0, FindUsages non-empty, Impact-Up contains the caller; fast suite 587+15 green, loom-guards PASS. (DoD "MCP impact/usages/neighbors non-empty on connected types" — surface-level MCP drive rides the phase-end octet, same GraphQuery locus) |

### D2.C5 — DI provenance ranking (audit C5)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| C5 | Multi-host DI bindings stop citing an arbitrary registration: ALL "file:line" sites + owning projects ride the one Resolves edge (deterministic order — also kills the first-wins bag-order flap); TraceBuilder cites the FOCUS HOST's own site when it has one, else deterministic-first + honest `[×N hosts]` (text render + JSON `diHostCount`); trace JSON surface gains provenance/multiImplCount/diHostCount/testOnly it never carried | VERIFIED | (this) | prism-d2/c5/ (2026-07-18) — bitwarden Api trace `POST /accounts/set-password`: `ICurrentContext → CurrentContext` cites **`src/Api/Startup.cs:92`** (the audit's exemplar cited `bitwarden_license/src/Sso/Startup.cs:46`; 8 hosts register the pair); `IEventService → EventService` (Api registers none) shows `diHostCount: 2` + deterministic-first site. Mechanism: AddDiResolves groups DirectBindings per (svc,impl) pair → RegistrationSites + RegistrationProjects (via scope.ProjectForFile) on the edge; RankDiProvenance matches EntryPoint.Project exactly (no path guessing). Snapshot schema v3→v4 (edges gained fields). Unit test pins both branches + deterministic first (GraphBuilderTests C5, 24/24 green). **Known-latent:** impl node FQN resolves to `Bit.Core.Test.AutoFixture.CurrentContextFixtures.CurrentContext` — NameResolver short-name collision picks a test-fixture namespace for the TYPE node identity (site citation correct); pre-existing, rider candidate for the self-health strand |

### D2.J1+J3 — silent-failure amnesty + health counters on every surface (audit J1/J3)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| J1+J3 | `PipelineDiagnostics` channel (AsyncLocal per-run scope opened by AnalyzeAsync; `Swallowed(source, category, ex)` counts + keeps first sample; no-op outside scope = zero behavior change at every site). ALL 30 grandfathered bare catches converted (SemanticLitePopulator's 9 + 5 body-carrying blind ones incl. both `semanticModel = null` sites; DiscoveryPipeline/CallGraph/GraphBuilder/Insights/Resolvers/ConfigDefaults/GitClone/SnapshotCache) — **loom-guards allowance table now EMPTY, PASS: Core bare-swallow count = 0 (DoD line ✓)**. J3 surfaces: analyze `--stats` extractor table gains Fails column + `Swallowed Failures` table (source×category×count×sample); `query stats` JSON `extractionFailures`; proto `StatsResponse.extraction_failures` (additive field 11) → gRPC + MCP stats + app TS regenerated (buf); persisted in the v4 snapshot | VERIFIED | (this) | prism-d2/j1/ — 4 unit tests (count/aggregate/sample-first/parallel-sum/scope-clear); **live truth: dogfood (439/339/34, ~1802 tok), GitVersion, shamshir poles all analyze GENUINELY CLEAN (0 swallows)** — the audit's "sparse graph = our silent failures compounding" hypothesis is now falsifiable and, on these repos, falsified; forced-failure proof: malformed .slnx renders `SolutionFileParser · slnx-parse · 1 · XmlException` end-to-end (scratchpad drive). Bitwarden note: its pre-J1 v4 snapshot deserializes empty failures (fields rode the same v4 bump in-session — no external exposure) |

### D2.I1+I2 — insight validity: coverage-gated claims + validity harness (audit I1/I2)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| I1+I2 | GraphOrphansSource is coverage-gated (I1): dead-code claims require ≥30 Calls edges, ≥50% verified, ≥50% wired entries (below the floor "zero inbound references" = "zero edges we captured" → NO claim); body-CONSTRUCTED types (CreationOp/LocalDeclOp facts — the audit's `new EpisodeDto(e)` in a LINQ projection), IMPLEMENTED/extended contracts (`IRequest` ×5 impls), and ENTITY-indexed types (`FeedCategory`) are excluded; the claim carries its coverage basis in the evidence line. Plumbing: `IAnalysisAwareInsightSource` (opt-in overload receiving SharedAnalysisContext; fallback overload makes NO ungated claim). I2: `query stats` JSON now carries insights (+confidenceBasis); lens-audit.ps1 gains **P7 insight-validity** — every `graph.orphans` evidence type is greped for liveness (constructed/implemented/DbSet) = machine-checked claims; all insights RECORDED per repo (`insights-recorded.json`) for manual verdicts on the rest | VERIFIED | (this) | prism-d2/i1/ — **podcasts (the audit's 3/5-FALSE exemplar): orphans cards = 0** — floor caught it honestly (88/193 = 45.6% verified Calls < 50%; wiring 22/24 = 92% post-C1/C3, verified ratio is the binding constraint) → DoD "podcasts orphans list contains zero provably-live types" ✓. 3 unit tests: live-types excluded + basis present; sub-floor silence; ungated fallback silent. P7 rides the phase-end octet run |

### D2.D3 — CLI trace budget honesty (audit D3)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D3-budget | The focused-analyze render path honors the token budget the stats line always CLAIMED: RenderAsync shapes the trace via the existing T3.3 `ShapeToBudget` (render boundary, post-build — kernel §8 decoupling intact; net of a ~1.2k reserve for TOUCHES/EMITS+diagnostics measured on the exemplar) and a cut is NEVER silent — `NOTE: trace shaped to the ~N-token budget — K deeper step(s) omitted` + per-subtree "(N omitted)". `BudgetIndependenceTests` trace half re-pinned to the new contract (Map half byte-invariant unchanged) | VERIFIED | (this) | prism-d2/d3/ — audit exemplar `analyze bitwarden --focus CipherService`: **~8026 tokens (budget 8000)** vs the audit's 24774 silent 3.1× breach; NOTE names 306 omitted steps; second drive **`from cache · 4776ms`** (audit paid 152s/trace — J2+AnalyzeCommand save now amortize the CLI too). Budget tests 2/2 green |

### D2.0b — J2 snapshot-cache resurrection (promoted from D3, §⚡)
| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D2.0b | J2: persisted serializable subset + awaited saves w/ surfaced errors + honest `from cache` stamp + hit/miss in stats + dirty-tree-safe version keys + litter-free probes | VERIFIED | (this) | prism-d2/j2-drive-*.{log,md} + 5 unit tests (`SnapshotCacheTests`) + cheap-gates-d20b.txt. **Root causes confirmed (all three from the scoping note):** the fire-and-forget `_ =` saves, the swallowed serialization failure (`AnalysisSnapshot` is NOT serializable: `SharedAnalysisContext` carries Roslyn syntax-node caches with parent-pointer cycles; `CodeGraph`/`CallGraph`/`ProjectDependencyGraph` are ctor-built classes; `Detection` is polymorphic), and the dir-littering `GetSnapshotPath`. **Mechanism:** `SnapshotPersistence` (Core/Analysis) defines `PersistedSnapshot` — the render-complete subset (Model flattened: bags→arrays, signals→list, per-type Tags as parallel map; Analysis subset: file lists + focus points + CallGraph/ProjectGraph/LayerMap as plain dicts; Graph as flat nodes+edges rebuilt through the `CodeGraph` ctor; Scenario/Report/Map/Entries/Insights/fingerprints as-is; NOT persisted by design: Options — every load site replaces it — and the syntax/body-facts caches). Detections round-trip via a reflection-registered `$dtype` discriminator (new detection types need no attribute; unknown discriminator = miss). Saves are AWAITED at all 3 sites (AnalyzeCommand, ReportCommand, EngineRunner), return `SnapshotSaveResult` — CLI prints a yellow warning, server logs; `--no-cache` now bypasses the WRITE too. Schema v2 (v1 never produced a file — no migration). **Dirty-tree honesty (found while fixing, not in the scoping note):** versionKey was bare `git rev-parse HEAD`, so a working cache would have served yesterday's map after any uncommitted edit; `ComputeKeys` now appends `-dirty-<16-hex>` = SHA-256 over `git status --porcelain -uall` paths + per-file mtime+length. **Proof:** analyze dogfood ×2 → `miss · saved 7e43d80` then `from cache · analyzed 21:49 · 829ms` + `Snapshot cache: HIT`, outputs sha256-IDENTICAL, wall 8.1s→2.5s; unit round-trip pins fresh-vs-rehydrated renders byte-identical in BOTH formats (markdown + json, run-local GeneratedAt normalized) on a real analyzed fixture |

## Baseline drift table (poles — must stay byte-identical through D1)

| Repo | Nodes/Edges/Entries | Style | Note |
|------|--------------------|-------|------|
| dogfood (eshop-microservices) | 439 / 339 / 34 | Microservices (App) | PRE-EXISTING local mods — never restore. **RESTORED at D1.2-fix2** (was drifted 439/335/CleanArchitecture from D1.1c). Now guarded by `eval/expectations/dogfood-microservices.json`, not just this hand-checked row |
| eShop | 1139 / 870 / 109 | Microservices 0.91 | **SANCTIONED count delta at D2-C1** (was 1089/837/109): +50 nodes/+33 edges = the WebApp/WebhookClient Blazor components' @code virtual types+members; entries 109 unchanged; rendered map text byte-identical to the D1.2b drift check except the 3 already-sanctioned D1 deltas (`GET /cart` now traces 4 deep into BasketState/BasketService [verified] — prism-d2/c1/eshop-trace-cart.txt). **Two SANCTIONED text deltas at D1.3a/D1.2f (documented, code-read verified):** (1) per-service `Identity.API: Identity provider [IdentityServer]` (was `Web API [EF Core]`) — it IS eShop's Duende host, the D1.3a rung is strictly truer; (2) event-wire participant ORDER shuffled (same sets — new AddHttpClient DirectBinding detections shifted edge insertion order). `GET /api/orders/cardtypes → IOrderQueries` byte-verified UNCHANGED (domain ports keep contract display) |
| shamshir | ~2882 / 3375 / 135 | NLayer 0.6 | live repo; <1.2% drift/session normal. Now guarded by `eval/expectations/shamshir-pole.json` (SEMANTICS only — archetype App + style NLayer + Aspire/Worker rungs — never counts, since it's live). The D1.2b count reading 2955/3507/137 was live-repo churn: the style + rungs the pole pins are all green at D1.2-fix2 |
| TodoApi | 123 / 81 / 12 | MinimalApi | |
| aspire-samples | 68 / 34 / 5 | SampleCollection | T8.2 fix |

Octet "before" snapshot (the thing D1 must flip): Newtonsoft ❌ App/0-entries/19-line map ·
SE.Redis ❌ App/MinimalApi-from-toys · GitVersion ❌ App/0-entries/empty map · wolverine ❌
App/CleanArchitecture-from-samples/80 sample rows · podcasts ⚠ hub+MAUI missing, `GET /` ×5 ·
bitwarden ⚠ 17/17 Unknown, hub missing · ScreenToGif ⚠ style Unknown · refit ✅ (CLI).
Full scorecard: AUDIT.md §Per-repo scorecard.
