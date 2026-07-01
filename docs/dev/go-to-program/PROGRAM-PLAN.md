# Program Plan — Go-To for Every .NET Repo

> Authored 2026-07-02 · Base: `develop` @ `7228d1e` · Derived from [`ENGINE-VALUE-AUDIT.md`](ENGINE-VALUE-AUDIT.md).
> Style: same fresh-session-resumable convention as `docs/dev/plans/` — one phase ≈ one session, one gate.
> Options are presented with a **VOTE** recorded (per the working agreement: document the vote, carry on).

---

## 0. One big iteration vs phased?

**Options:** (a) one mega-iteration branch delivering hardening + facets + MCP together;
(b) phased program, each phase independently shippable behind `eval/gates.ps1`.

**VOTE: (b) phased.** The eval ratchet is the project's core honesty mechanism and it works
per-session; a mega-branch would sit unratcheted for weeks and re-create the "claims vs delivery"
failure mode the audits keep catching. The phases below are sequenced so each one makes the next
cheaper, and V2/V3 can run in parallel worktrees if two agents are available (they touch disjoint
files: builders/renderers vs query layer).

Phase order and why: **trust → value → interestingness → kernel/faces → scale.** Hardening first
because every facet and the MCP surface amplify whatever the wiring says — shipping F1–F12 on seams
that fabricate cross-method edges (audit §5.1) would repeat the `[verified]`-lie era at higher volume.

---

## Phase V1 — Trust at breadth (hardening the seams)

*Goal: the wiring is correct on arbitrary .NET code, not on eShop-shaped code.*

| # | Task | Locus | Gate evidence |
|---|---|---|---|
| V1.1 | **Span-bound variable resolution** (audit §5.1, bug) — `Send(cmd)`/`AddDomainEvent(evt)` variable lookup constrained to the enclosing method span; no in-span match ⇒ no edge (+ optional param/field-type fallback, V1.2) | `GraphBuilder.cs:836-845`, `:1043-1045` (`AddSends`, `ResolveVariableNewType`) | new negative test: sibling-method `new X()` no longer attributed; eShop goldens unchanged |
| V1.2 | **In-span dataflow-lite** — resolve dispatch args via parameter type / field type when no local `new` (closes eShop `/draft`) | same + method signature data already in model | `/draft` trace shows the param-typed command (flip the long-standing aspirational) |
| V1.3 | **Receiver-typed dispatch gating + `DispatchSeamCatalog`** — body-scan verb matches gated on receiver ∈ known dispatch interfaces; (package → interfaces → verbs → EdgeKind) as declarative catalog data; bare-verb fallback demoted to lower confidence | new `Graph/Seams/DispatchSeamCatalog.cs`; `AddSends`/`AddDispatchEdgesFromBody`; reuse `CallGraphExtractor` receiver resolution | negative test: `SmtpClient.Send(msg)` mints no `Sends` edge; MassTransit/NServiceBus/Wolverine verbs from catalog get positive fixtures |
| V1.4 | **Model-derived event-type sets** (audit §5.3) — replace `*IntegrationEvent*` name regex + fixed method list with the set of types deriving known event bases/interfaces | `AddRaises`; `TypeDiscovery.BaseTypes/ImplementedInterfaces` | positive fixture: event named `OrderPlacedMessage : IntegrationEvent` detected; negative: DTO named `FooIntegrationEventLog` not |
| V1.5 | **Pattern-zoo corpus** (audit §5.5) — `tests/fixtures/PatternZoo/` with modern-C# shapes through every seam; incl. the raw-string-literal regex trap | new fixture + `PatternZooTests` | corpus green; raw-string trap fixed or documented-and-guarded |
| V1.6 | **Multi-impl honesty** — trace annotates `[registered ×N]` when DI has multiple impls | `TraceBuilder`/`TraceRenderer`; `DiRegistrationDetection` | fixture with 2 impls renders the annotation |
| V1.7 | **Hygiene quickies** — default excludes += `eval-repos`,`analysis-repos` (`ExtractionOptions.cs:18` + the 3 duplicated literals in Cli/Desktop/Server — unify to one constant while there); extractor-convention test (signal-gated, parse-cache, `.cs`-only) | `ExtractionOptions`, `ExtractorConventionTests` (new) | analyze on DevContext2 root completes; convention test green over all 23 extractors |

**Gate:** full `eval/gates.ps1` + the new negative fixtures. **Non-goal:** full semantic Sends/Raises
(stays backlog #11; V1.3/V1.4 are its stepping stones and remain the fallback tier).

---

## Phase V2 — Per-type value packs (facets over existing data)

*Goal: a dev on each repo type sees the answer to their top questions in the first screen. Audit §4.*

Prereq decision — **facet plumbing**. Options: (a) hard-code each facet into `MapRenderer`;
(b) a **FacetCatalog** (descriptor: gating archetype/signal → section → cap → `GraphQuery` op), renderers
enumerate it. **VOTE: (b)** — same reasoning as WS-D/EntrySurfaceCatalog; 12 facets hard-coded would
undo the kernel-hygiene win. Descriptor first, then facets land as data + one query function each.

Wave 1 (render-only or pure joins — data verified present):
- **F1 auth surface** (`AuthAttributes` → entry-line annotation + Overview digest)
- **F2 middleware pipeline** (`MiddlewareDetection.PipelineOrder` → Cross-cutting line)
- **F3 message flow matrix** (Sends/Raises × Handles/Consumes join → messaging repos' Map; `?` marks
  consumed-never-produced for honest partial view)
- **F7 hotspots/centrality** (top-N by degree, NoiseFilter'd → Stats + the §3 universal fallback)
- **F8 DI health histogram** (lifetime/shape counts → Stats)

Wave 2 (small extractor deltas):
- **F4 data map** (fix the DntSite EfCore detection gap first — known Iteration-5 leftover — then
  aggregate TOUCHES to Map)
- **F5 talks-to** (typed/named HttpClient + gRPC clients → Topology `TALKS TO` group)
- **F6 config surface** (`Configure<T>` + section literals)
- **F10 CLI command tree** (`[CommandOption]`/`[CommandArgument]` from settings classes)
- **F11 serverless trigger detail** (binding kind + literal on entry line)
- **F9 Aspire topology render** · **F12 desktop VM wiring** (order last; F12 is the largest delta)

Every facet ships with a render-level eval check (the `entry-kind-present` lesson: detection counts are
not coverage). **Gate:** gates green + per-facet checks + *no facet renders on repos where its gate
signal is absent* (anti-soup assertion, one negative expectation per facet).

---

## Phase V3 — Interesting points per archetype (the drill-down brain)

*Goal: §3's three-tier table (entry / common / interesting) becomes a kernel concept every face can query.*

- V3.1 `GraphQuery.InterestingPoints(archetype)` — per-archetype composition: web → auth boundaries +
  data hubs; messaging → matrix hubs; library → internal hubs (centrality within library projects) +
  seat implementors; CLI → command tree roots; fallback → centrality. Mostly composition of V2 facets.
- V3.2 **Library contributor view** — a `HEART` line-group in the library Map: top internal hubs +
  the pipeline spine (highest-betweenness path from ENTRY API into internals; approximate with
  degree-weighted BFS, don't over-engineer).
- V3.3 **Human surface compression** — the PUBLIC SURFACE wall-of-types (Serilog: 109 types) gets a
  structural cap with `… and N more` + namespaces ranked by (public-type count × centrality); full list
  stays in JSON/LLM output. (CLI `--full-surface` escape hatch.)
- V3.4 Desktop/CLI wiring: the desktop's entry list + node detail consume `InterestingPoints` so the
  "what should I look at" panel is engine-provided, not UI heuristics.

**Gate:** for each archetype in the §2 matrix, `InterestingPoints` returns a non-empty, noise-filtered
list on its exemplar repo; snapshot expectations on 3 (DntSite, Serilog, eShop).

---

## Phase V4 — Kernel faces: W9 retirement, JSON = kernel, MCP server

*Goal: one kernel, one wire contract, three thin faces. Audit §6.1 + §7.*

W9 decision — **options:** (a) keep JSON/HTML catalog but hide its funnel from `--stats`;
(b) **retire**: delete `TokenBudgetEnforcer`/`PatternRelevancePruner`/`RenderPlanBuilder` catalog path;
`--format json` becomes the serialization of `MapModel` + `GraphQuery` results (+`CodeGraph` on
`--include-graph`); HTML re-renders from the same JSON; (c) leave as-is documented.
**VOTE: (b).** The query API now exists, eval JSON checks are ours to migrate (explicitly authorized),
and MCP needs exactly this contract — building MCP JSON *next to* a lying legacy JSON would be the
worst of both. Phase-0-style check first: enumerate remaining `FinalScore`/catalog consumers
(`RenderPlanBuilder.cs`, legacy renderers, desktop) before deletion, migrate eval `json-*` checks to the
new paths, then delete. HTML: regenerate from kernel JSON or drop to a minimal shell — **sub-vote:
regenerate minimal** (it's the desktop's print/export seam, cheap to keep honest).

- V4.1 W9 retirement per above (biggest single hygiene payoff; ~removes the last "two products" seam).
- V4.2 `GraphQuery.Search` (name/route/tag/kind filter) — needed by MCP + desktop find; trivial scan now,
  index later (V5).
- V4.3 **MCP server** (`DevContext.Mcp`, stdio): tools = `analyze`, `list_entrypoints`, `map`, `trace`,
  `stats`, `node`, `neighbors`, `find_usages`, `search`, `interesting_points`, `facet`. Thin: every tool
  is a `GraphQuery` call + the analyze-handle cache (`analyze once, query many`). Contract tests +
  a recorded end-to-end transcript on eShop as the demo artifact.
- V4.4 CLI `devcontext query <op>` twin (same handles) — makes CI scripting real (e.g. fail-if-anonymous-
  endpoints-grew uses F1 via query).

**Gate:** gates green with legacy machinery gone; MCP contract tests; the eShop MCP transcript checked in.

---

## Phase V5 — Scale, reach, and the re-probe

*Goal: "just run it" on anything, including huge repos and a GitHub URL; measure the value claim.*

- V5.1 **Persistent content-keyed index** (the long-deferred P4): cache compilation + `CodeGraph` by
  (path, content hash); warm re-open ≈ instant; amortizes the fixed BCL-bind wall (audit §6 perf note).
- V5.2 **GitHub-URL path** (`analyze https://github.com/...`): shallow clone → analyze → cached. The
  discovery surface from PRODUCT-DIRECTION §5.
- V5.3 **Huge-repo scoping**: `--solution` selector UX (multi-solution repos), per-area sub-maps for
  megarepos (aspnetcore), HotChocolate as the standing perf fixture.
- V5.4 **Re-run the value probe** (audit §8c): agent task with/without trace + with MCP, on eShop +
  DntSite, against the V1-hardened engine. This is the honest checkpoint for the "accelerator" claim —
  and the input to whatever marketing/README claims ship publicly.

**Gate:** DntSite warm re-open < 2s; GitHub-URL e2e on a mid-size repo; probe report committed.

---

## Cross-cutting rules (all phases)

1. Never render what the kernel can't honestly answer; every facet/feature ships with a negative check.
2. Adding a shape/facet/seam = catalog data + one focused unit of logic — if a change fans out across
   >2 core files, stop and fix the catalog instead (WS-D precedent).
3. `eval/gates.ps1` green at every phase boundary; expectations ratchet, never loosen silently.
4. Perf: any new extractor/facet must not regress the bench baselines (eShop, DntSite, OrchardCore).

## Votes recap (for quick review)

| Fork | VOTE | One-line why |
|---|---|---|
| One big iteration vs phased | **Phased (V1→V5)** | the eval ratchet works per-session; mega-branches rot |
| Hardening vs features first | **Hardening (V1)** | facets amplify whatever the wiring says; §5.1 is live |
| Facet plumbing | **FacetCatalog descriptor** | 12 hard-coded facets would undo WS-D's hygiene win |
| Dispatch matching | **Receiver-typed + DispatchSeamCatalog** | kills both false-positive and false-negative overfit; data-driven growth |
| Event detection | **Model-derived type sets over name regex** | reuses extracted BaseTypes; the general anti-overfit move |
| W9 legacy machinery | **Retire; JSON = kernel serialization** | one wire contract for JSON/MCP/web; deletes the "two products" seam |
| HTML output | **Regenerate minimal from kernel JSON** | keeps the export seam without a second renderer stack |
| MCP timing | **After W9 (V4), before web** | MCP rides the same contract W9 produces; web rides Server later |
| Semantic Sends/Raises | **Defer; V1.3/V1.4 are the stepping stones** | catalog + type-sets deliver 80% at 20% cost; semantic stays backlog #11 |
| Surface wall-of-types | **Structural cap for humans, full in LLM/JSON** | same budget-free philosophy as W4 |
