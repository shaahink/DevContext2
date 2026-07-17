# Experience & Engine-Health Addendum — lens audit round 2 (2026-07-17)

The first pass (`AUDIT.md`) graded **honesty** hard and **experience** soft. This addendum answers
the harder question — is the product smart, engaging, alive — plus two verification legs the first
pass skipped: are the insights *true*, and is the engine itself healthy after ~9 iterations.
Findings continue the AUDIT.md ledger (I–N).

## I. Insight validity — NOT all insights are true

Verified podcasts' `graph.orphans` insight ("Possible dead code: 5 public types with zero inbound
references: IRequest, PlayerService, EventHandlers, FeedCategory, EpisodeDto") against the code:

| Claim | Ground truth | Verdict |
|---|---|---|
| `EpisodeDto` dead | Constructed + returned by `EpisodesApi.GetEpisodeById` (`Ok<EpisodeDto>`, `.Select(e => new EpisodeDto(e))`) | **FALSE** |
| `IRequest` dead | Implemented by 5 request classes — the same hand-rolled CQRS the style evidence celebrates | **FALSE** (implements-edges ignored) |
| `FeedCategory` dead | EF join entity (migrations, model) — AND listed as an entity by the *data-map insight in the same list* | **FALSE + self-contradictory** |
| `PlayerService`, `EventHandlers` | plausibly thin, unverified | — |

- **I1** The orphans insight converts **low graph coverage into a code-quality accusation**. On a
  repo where the same stats admit 92/124 Calls edges are `[approx]` and 47% of entries are wired,
  "zero inbound references" means "zero edges we captured". An agent acting on this deletes live
  code. Insights must be **coverage-gated**: no dead-code claim from a graph tier that didn't walk
  LINQ projections/DTO constructions, and never for types the entity mapper itself indexed.
- **I2** General rule the round proved: every insight is a *claim about the repo* and needs the
  same truth discipline as entries — a validity harness (sample N insights per octet repo, verify
  against code, expectation rows) does not exist today. Auth-surface counts spot-checked fine;
  orphans failed 3/5.

## J. Engine self-health — silent failure is the house style

- **J1** **121 catch sites in Core; 17 bare `catch { }` swallows.** The worst offender is the
  quality-critical path: `Graph2/SemanticLitePopulator.cs` has **12 bare swallows** (lines 76, 133,
  286, 492, 521, 550, 623, 711, 777…). A failed semantic bind silently drops edges — no counter, no
  diagnostic, no stats row. The sparse-graph findings (approx-heavy edges, 47% wired, type nodes
  with zero degree, the false orphans insight) are plausibly *our* silent failures compounding, not
  repo weirdness. `DiscoveryPipeline.cs` (169, 981) and `ConfigDefaultsSource` swallow too.
- **J2** **The I8 snapshot cache has NEVER worked.** `%LOCALAPPDATA%\DevContext\cache` holds ~40
  repo-key directories, **every one 0 bytes** — `GetSnapshotPath` creates the dir, then the
  write-behind save (`AnalyzeCommand.cs:225`: `_ = snapCache.SaveAsync(...)`) dies — fire-and-forget
  task killed by CLI process exit and/or a serialization throw eaten by `SnapshotCacheService.cs:205
  catch { }`. Result: `Exists()` never true, every run re-analyzes (bitwarden: 207s + 152s per
  trace), and the feature has self-reported nothing since I8. **This is the answer to "why is there
  no cache avoiding the second analysis" — there is one; it shipped dead and nothing could tell us.**
- **J3** Nothing in stats/waterfall counts extraction *failures* — the waterfall accounts wall time
  (T7.3) but not error/skip/partial rates per extractor. Health is invisible by construction.

## K. Collected-but-unused riches

- **K1** The pipeline streams `OnStageStarted` / `OnExtractorStarted` / `OnExtractorCompleted`
  (with elapsed, skipped+reason, typesAdded, detectionsAdded) live — the UI renders 9 gray words
  and a single percent (`ui/loading-08s.png`). Per-extractor timings, discovery counts, skip
  reasons, the whole T7.3 waterfall: collected, never shown at analyze time — precisely when the
  user is captive and curious. Pre-release there is no size constraint: this should be the show.
- **K2** `--include-diagnostics`, `RunReportCollector`, self-check results, seam verified/approx
  ratios, per-service entry counts — all exist in the engine; the UI surfaces none of them during
  or after loading (Stats page shows aggregates only).

## L. The experience critique, point by point (all confirmed)

- **L1 Home page** — information-dense and honest (wiring health, freshness, needs-attention) but
  the hero "What runs" graph is broken (clipped labels, off-viewport nodes, plain squares) and top
  flows are illegible bare `GET /` rows (B3). Verdict: right content, weak execution.
- **L2 Cache in the UI** — Freshness card says "Current · analyzed in 3.8s" but the truth is
  in-memory-session-only; close the server and everything re-analyzes (J2). The card should say
  what is cached, where, and offer the snapshot age/HEAD — once the cache actually works.
- **L3 Atlas** — a header card, ONE badly-laid-out diagram, top-flow chips, an empty event board.
  As "the one-pager you show a new teammate" it under-delivers: no layered architecture view, no
  service grouping/lanes, no data stores, no external systems. It reads as a stub, not an atlas.
- **L4 Context Studio** — scope picker + budget + intent exist, but there is **no live output
  preview**: you assemble cards blind and only see the pack after Copy/Save. "I'm changing this
  entry" preset is ambiguous (changing *what*? it reads like an edit action, and its effect on the
  pack is invisible). The four indistinguishable `GET /` rows (B3) make scope picking guesswork.
  The core loop the product promises — *see the context an agent would get, live, as you shape
  it* — does not exist in the UI.
- **L5 Entry search** — search-or-jump (Ctrl+K) exists, but the entries affordance on Home/status
  bar routes to `Shift+E … full entry audit table` — a raw table dump as the primary navigation for
  the product's core object. No grouped, ranked, filter-as-you-type entry browser.
- **L6 The graphs** — three different canvases (hero topology, Explore flow canvas, Atlas service
  diagram) with three behaviors, all force-directed node soup, none communicating structure the way
  even a text-mode LLM diagram does (layers, lanes, direction, grouping, emphasis). No hierarchy,
  no edge semantics visible, no per-kind glyphs (F3), broken fit (F2). "Hard to fathom by a human"
  is accurate: they demo poorly and inform less than the CLI's ASCII output.
- **L7 Loading** — dead text list while a 60–200s analysis runs (K1). No waterfall, no per-stage
  timing bars, no live discoveries ("125 projects · 3 gRPC services · 662 endpoints…"), no honest
  big-repo expectations ("SemanticLite bind ~80s on repos this size").

## M. Diagram intelligence — the gap named

What an LLM sketches for bitwarden unprompted: layered lanes (clients → gateways → services →
data), services grouped by domain, arrows labeled with transport (HTTP/queue/gRPC), stores drawn as
cylinders, external systems dashed — because the *reader's question* is "how does this system
work", not "what does a force simulation converge to". Our canvases have strictly more data and
communicate strictly less. The engine already knows kinds, layers (DDD folder evidence), transports
(seam kinds), and groupings (services, feature areas) — none of it drives layout. This is a
renderer decision, not an engine gap: adopt deterministic layered layouts (ELK/dagre-class),
kind-driven glyphs/lanes, transport-labeled edges, and progressive disclosure (start at C4-ish
level 1, expand on demand).

## N. Scores for the questions asked

| Question | Answer |
|---|---|
| Home page after redesign | Content right, execution weak (L1) |
| Cache avoiding 2nd analyze, shown in UI | Feature exists, **dead since I8**, UI misleads (J2/L2) |
| Atlas | Stub-grade (L3) |
| Studio live-output while editing | Missing entirely; preset ambiguous (L4) |
| Entries search = big table | Confirmed (L5) |
| Graphs hard to fathom | Confirmed; three inconsistent soups (L6, F2, F3) |
| Insights all true? | **No** — orphans 3/5 false on podcasts (I1) |
| Loading dead/boring | Confirmed; data exists, presentation dead (L7, K1) |
| Unused riches (suppressed exceptions etc.) | Confirmed both readings: engine swallows its own failures (J1) AND collects unsurfaced data (K1/K2) |
| Engine healthy after iterations? | **No** — silent-failure culture + a dead flagship feature (J1–J3) |

**Net:** the first audit's "PARTIAL/PASS" grades stand for *truth*; for *experience* the honest
grade today is: CLI ✅ · MCP ✅ · engine internals ⚠ (silent failures) · desktop ❌ (broken visuals,
missing core loops, dead loading). The plan must treat experience as a pillar, not a punch list.
