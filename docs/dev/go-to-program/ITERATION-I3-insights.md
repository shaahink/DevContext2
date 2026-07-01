# Iteration I3 — Insights engine (stats that aren't boring)

> **Status: BLOCKED on I2** (wants the wire contract; can start after I1 if I2 slips — only the JSON
> exposure moves) · Phase: V2/V3 · One session. Design contract: `FACES-DESIGN.md` §3.

## Goal

`--stats` and the desktop stats section stop showing engine telemetry first and start showing ranked,
linkable, repo-specific findings. Ten launch sources, catalog-registered.

## Step 1 — The kernel pieces

- `src/DevContext.Core/Insights/Insight.cs` + `IInsightSource` + `InsightsBuilder` exactly per
  FACES-DESIGN §3.2. Builder runs in `DiscoveryPipeline` right after `GraphAssembly`, result on
  `AnalysisSnapshot.Insights`. Sources discovered like extractors (reflection over the assembly +
  ordered by Id) — no hand-registration list.
- `GraphQuery.Insights()` returns them; kernel JSON includes them (I2's `insights: []` fills in).

**Tricky bit — ranking without a scoring system creeping back:** do NOT invent weights. Order =
`Severity desc → Category round-robin → source Id`. Cap 3 per category, 10 total. Deterministic,
no floats. (We deleted one weighted scorer already; don't grow another.)

**Tricky bit — linkable evidence:** `Evidence` strings must be resolvable: use the node's short name
exactly as `GraphQuery.ResolveNodeId` accepts (Type short name or `Type:Method`), or `file:line`.
Add a unit test that every emitted evidence string on eShop resolves via `ResolveNodeId` or parses as
`path:line` — this is what makes desktop evidence clickable for free in I4.

## Step 2 — The ten launch sources (FACES-DESIGN §3.3 table)

Implementation notes for the non-obvious ones:

- `auth.anonymous`: an endpoint is anonymous when `AuthAttributes` is empty AND no controller-level
  authorize attribute exists (the extractor already merges controller-level attrs into the detection —
  VERIFY; if not, join `TypeDiscovery.Attributes` of the handler type). `[AllowAnonymous]` present =
  explicitly anonymous → stronger wording. Severity: Warning only when POST/PUT/DELETE anonymous
  exists; else Notable.
- `wiring.hubs`: degree over **production Type nodes only** (NoiseFilter), exclude entries themselves
  and DbContexts (their fan-in is structural). Outlier = top node whose in-degree ≥ 2× the next.
  If no outlier, stay silent.
- `graph.orphans`: candidates = public production types, in-degree 0, not entry, not DI-registered
  (`DiRegistrationDetection` by impl type), not a detected handler/consumer, **and not named as a
  target by any `IndirectWiringDetection`** (reflection honesty). Wording "likely unused". Cap 7.
- `wiring.external-events`: consumed message types (`Handles`/`Consumes` edge targets +
  `MessageConsumerDetection.MessageType`) minus produced (`Sends`/`Raises` targets). Cross-check the
  type actually exists in-model (else it's an external contract — that IS the insight).
- `coverage.honesty`: reuse `GraphStats.Compute` — entries-with-target ratio + approx share. Always
  fires (it's the trust line), Severity Info.

Each source: one file, one pure function, one focused unit test with a hand-built model. **A source
that would restate the Map stays silent** (no "this app has 24 projects").

## Step 3 — Faces

- CLI: `--stats` (= `--stats repo`) prints `INSIGHTS` block (title + first evidence line + jump-off),
  then the coverage line; `--stats engine` prints the old RunReport telemetry.
- Map Overview gains the top-3 insight titles as single lines (FACES-DESIGN §3.4) — gated: only
  Severity ≥ Notable, so quiet repos stay quiet.
- Desktop: `section-stats.ts` renders `session.stats().insights` as cards (Category chip, Title,
  Evidence list, jump-off as a button that pre-fills focus/palette); pipeline telemetry moves into a
  collapsed "Engine" disclosure at the bottom. Server: `GetStats` response gains the insights message
  (proto change + regen `devcontext_pb.ts`).

## Docs & goldens

- Eval: per-source expectations on the repos where they must fire — eShop (`wiring.external-events`,
  `data.busiest-aggregate`), DntSite (`auth.anonymous`, `shape.entry-mix`, `wiring.hubs`), and one
  **negative**: TodoApi must NOT fire `graph.orphans`/`topology.chokepoint` (tiny repo silence).
- `docs/product/cli-reference.md`: `--stats` doc. `docs/product/desktop-ui.md`: Insights section.
- New `docs/product/INSIGHTS-REFERENCE.md`: one line per source Id (the contract users/agents read).

## Gate

Gates green · insight evidence resolvability test green · eShop + DntSite runs show ≥4 distinct
non-Info insights each (paste output in commit) · TodoApi stays quiet · desktop screenshot of the
Insights section attached to the PR.
