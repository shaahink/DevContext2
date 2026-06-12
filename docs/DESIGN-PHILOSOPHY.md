# DevContext — Design Philosophy

> What this tool believes, what the codebase currently does, and the bridge between the two.
> This document is the source of truth for product and architecture decisions. The action plans
> in `docs/plans/` implement it.

## The one-sentence pitch

**DevContext is the answer to "what *is* this .NET solution?"** — point it at a folder, repo URL,
`.sln`/`.slnx`, or `.csproj`; it reads the code once, models it, and produces the most relevant
context for whatever you're doing — sized for an LLM prompt, readable by a human, and honest
about how it got there.

The two audiences are inseparable: the **LLM** gets a token-budgeted markdown briefing; the
**developer** gets a rendered report *plus the full story of how it was produced* — timings,
counts, what was kept, what was cut, and why. That second part is the personality of the tool:
it is nerdy on purpose. A tool that compresses your codebase into 8,000 tokens and *doesn't*
show its work is asking for blind trust. DevContext shows the funnel.

---

## Principles

Each principle below states: the philosophy, what exists today, the gap, and the bridge.

### P1 — Analyze once, render many

**Philosophy.** Reading and modeling code is expensive; deciding what to *show* is cheap. These
must be separate phases with an immutable artifact between them. Every "view" decision — token
budget, section selection, output format, provenance on/off — is a **lens** applied to the same
analyzed model, never a reason to re-read source files.

**Today.** Stages 1–3 (extraction) and stages 4–6 (pruning/compression/rendering) run as one
pipeline that mutates a single `DiscoveryModel`: pruners set `IsPruned`, compressors rewrite
member bodies in place, `TokenBudgetEnforcer` bakes the budget into the model. Consequence: the
desktop's `OnAnalysisOptionChanged` re-runs the *entire pipeline* when the user toggles a section
checkbox or moves the token slider (debounced, but still a full re-analysis). The UI lag saga
(v1→v4 in the technical report) is downstream of this — the UI compensates by parsing rendered
markdown back into sections, which is rendering run in reverse.

**Gap.** No immutable boundary between "what the code is" and "what we choose to show."

**Bridge.** Split the pipeline result into an **`AnalysisSnapshot`** (immutable: types with
relevance scores, detections, signals, call graph, compressed body candidates, diagnostics, run
stats) and a **`RenderPlan`** (cheap, derived: given budget + sections + focus, which items
render). Pruners become *scorers* — they rank, they don't destroy. Budget enforcement moves to
RenderPlan computation. The desktop holds the snapshot and re-renders in milliseconds on any
toggle; the CLI computes one plan and renders once. `BuildSectionData`'s markdown re-parsing
disappears — section data comes from the snapshot, not from scraping output.
→ **Plan 1**

### P2 — The budget is a dial, not a guillotine

**Philosophy.** Token budgeting has two legitimate modes and we serve both: (a) *unattended* —
the default output must already fit the budget and contain only relevant material, no user
involvement required (CLI, CI, paste-and-go); (b) *attended* — the user can turn the dial after
analysis and immediately see what enters and leaves the context, without re-analysis. Scoring
happens **during** the pipeline (so relevance is informed by focus, call reachability, and
patterns); enforcement happens **at render time** (so it is reversible and interactive). What was
cut is never invisible — it's one disclosure away ("12 types pruned: …"), so the user can promote
something the scorer got wrong.

**Today.** Scoring and enforcement are fused in Stage 4 and destructive. The desktop's section
drawer with the budget bar is the right *instinct* — but it re-runs the pipeline to honor a
toggle, and the "what was pruned" story lives only in `--include-diagnostics` notes.

**Gap.** Enforcement is baked in too early; the cut list is not a first-class, browsable thing.

**Bridge.** Plan 1's RenderPlan makes the budget a parameter of rendering. Every render reports
its funnel (P3): `discovered → relevant → fits-budget`, with the pruned remainder listed by score
so the user sees *what almost made it*. Desktop budget slider and section toggles operate on the
snapshot live. CLI gains nothing new to learn: `--max-tokens` behaves identically but the summary
explains the cut. → **Plans 1 + 3**

### P3 — Show your work (nerdy is the brand)

**Philosophy.** Developers trust tools that expose their internals. Every run tells its own
story: which extractors ran (and which were skipped, and *why* — signal gating is a feature,
advertise it), how long each took, what each found, how the four scorers reshaped the ranking,
what each compressor saved, cache hit rates, parallel speedup. This is not debug output; it's
the product's personality, present in both CLI and desktop with a deliberate terminal/monospace
aesthetic. It also keeps us honest: a stats panel that renders nonsense is a regression test on
the architecture itself.

**Today.** The bones exist and are good: `IDiscoveryObserver` has rich callbacks
(`OnExtractorCompleted` with types/detections added, `OnPrunerCompleted` with before/after,
`OnCompressionApplied`, per-stage timings), `MetricsDiscoveryObserver` + `--metrics` prints a
summary panel, dry-run prints the execution plan. But the desktop observer discards everything
except stage names, the stats are an opt-in afterthought in the CLI, and nothing reports the
token funnel, cache performance, or parallelism.

**Gap.** Telemetry is collected then mostly thrown away; no unified report object; no UI surface.

**Bridge.** A first-class **`RunReport`** assembled by the pipeline and returned with every
result: stage waterfall, per-extractor table (incl. skipped + skip reason), scorer funnel,
compression savings, cache stats, file/LOC counts, token funnel, degree of parallelism. CLI:
one-line summary always, `--stats` for the full nerd view (Spectre tables; `--metrics` becomes an
alias). Desktop: a third **Stats** tab rendered from the same `RunReport` — timing waterfall,
token funnel bar, extractor grid — in the terminal aesthetic. JSON output embeds the report so
CI can track analysis performance over time. → **Plan 3**

### P4 — Two situations, one dial (we are not a query system)

**Philosophy.** There are exactly two user situations: **"I don't know this repo"** →
orientation map (no starting point exists, by definition), and **"I know where I'm standing"**
→ slice from a focus point *down the wiring* (endpoint → handler → MediatR → entities →
events) — the wiring map is the asset and this is the move it enables. The focus can be a
type, a method, or an endpoint route. The only other primary input is **Depth** (how far down
to follow). Everything else — scenario, profile, section defaults, pruning distances — is
*derived*, and the derivation is always shown ("Slicing from `GET /api/orders`, depth 5").
Deliberately absent: natural-language input. DevContext is static analysis with smart,
controllable filtering — initially automatic, then visually adjustable — not a query engine;
a free-text "ask your repo" box would be pseudo-NLU that sets expectations the tool doesn't
honor, which P7 (genuine) forbids.

**Today.** Four overlapping vocabularies: `--scenario overview|deep-dive|trace` (with `trace` a
registry-level duplicate of `deep-dive` and `audit` a deprecated alias remapped in *two* places —
`AnalyzeCommand` and `AnalysisService`), `--profile focused|debug|full` (mechanism labels users
can't map to intent; the desktop already hides profile by deriving it from checkboxes — the CLI
should learn the same trick), `--task` (keyword inference that silently picks scenario+profile),
and `--around` (the focus point). `deep-dive` without `--around` is just a worse `overview`, and
nothing tells the user that.

**Gap.** The controls describe the *mechanism* (scenario/profile/pruning) instead of the
*question* (focus/depth). Inference is silent. Scenario/profile resolution logic is duplicated
between CLI and desktop.

**Bridge.** A single **`AnalysisIntentResolver` in Core** used by both front-ends, with one
total rule: focus present → slice config; absent → overview config; `--depth` overrides the
preset's distances. **`--task` and `IntentInferrer` are removed** (deprecation message for one
release) — even the LLM-agent-as-caller case is better served by the agent passing `--focus`
itself. Focus grows instead: `--focus` accepts `Type`, `Type:Method`, or an endpoint route
(`GET /api/orders` resolves to its handler via the existing detections, then walks the wiring).
The explanation string is printed/displayed on every run. `trace`/`audit` survive only as
silent aliases; `--scenario`/`--profile` become hidden expert overrides. Desktop: a focus
picker autocompleting types, methods, *and routes* from the previous snapshot — plus the
noise side of "controllable filtering": the excluded-types list (tests, budget cuts) is a
visible, re-includable filter group in the lens, not a silent score. → **Plan 2**

### P5 — Never read twice, cancel anywhere, parallel where it pays

**Philosophy.** Source files are read once into shared caches; every stage observes
cancellation; parallelism is applied exactly where the dependency structure allows it and
nowhere else — measured, not assumed (the Stats tab proves it).

**Today.** Genuinely strong: `AnalysisCache` with `Lazy<Task<…>>` dedup, zero locks, concurrent
collections, CT propagation through all stages, capability/signal validation that warns about
same-stage read/write races. Two blemishes: Stage 3's eleven extractors run sequentially even
though several are mutually independent (they're gated on *sealed* signals and write to
concurrent collections — the race-validation machinery already exists to prove independence),
and `AnalyzeCommand` blocks a thread with `.GetAwaiter().GetResult()` inside the non-async
`AnsiConsole.Status().Start(...)`.

**Gap.** One missed parallel opportunity; one sync-over-async; no numbers proving any of it.

**Bridge.** Declare read/write capabilities for Stage 3 extractors and run the independent
subset via the same `Parallel.ForEachAsync` path Stage 2 uses (CallGraphExtractor and
SourceBodyExtractor stay ordered last if they consume others' output). Switch the CLI to
`Status().StartAsync`. Surface wall-clock vs. sum-of-extractor time in the RunReport so the
speedup is a visible number, not a claim. → **Plans 1 (mechanics) + 3 (proof)**

### P6 — The desktop stays light, smooth, and faff-free

**Philosophy.** Download a zip, run the exe, analyze. No SDK, no installer ceremony, no
multi-hundred-MB download, no UI freeze, ever. The desktop is a *thin view* over the same Core
engine the CLI uses — it must never grow its own analysis logic.

**Today (and the decision).** After the Photino → Avalonia → WPF+BlazorWebView journey, the
answer is: **keep WPF + BlazorWebView (WebView2)**. Rationale: the human-facing output *is*
HTML (`HtmlContextRenderer` is a core renderer, not a UI hack); WebView2 is evergreen-installed
on Win10/11 so it ships outside the zip; Razor keeps the UI in the .NET+TS domain; and the
remaining lag is a data-flow problem (P1), not a framework problem — switching frameworks again
would carry the same flaw along. Revisit only if cross-platform desktop becomes a goal (then
Avalonia, knowingly).

**Gap.** Residual ~500 ms display lag from re-rendering and section re-parsing; `MainViewModel`
is a ~660-line god class doing clone/orchestrate/parse/present; `AnalysisService` duplicates
Core resolution logic (P4).

**Bridge.** Plan 1 removes the re-parse and the re-analysis-on-toggle (snapshot + lens), which
is also the lag fix ("v5" in the lag history table). `MainViewModel` splits along the seams that
already exist in its region structure: input state, analysis lifecycle, output presentation,
GitHub flow. Publish size budget: self-contained zip ≤ 110 MB, cold start ≤ 2 s, toggle-to-render
≤ 100 ms — enforced as numbers in the release checklist, because P3 applies to ourselves.
→ **Plans 1 + 2**

### P7 — A genuine, world-class repository

**Philosophy.** The repo *is* the product page and the CV line. Everything public is true:
claims match tests, docs match code, history reads like a project (not like 40 agent sessions).
"Pragmatic perfectionist" means: polish what users touch (README, quickstart, output quality,
release artifacts), be honest about what's not covered, and don't gold-plate internals nobody
sees.

**Today.** 232 passing tests but four extractors have zero dedicated coverage (EventBus,
InMemoryEventBus, IndirectWiring, Aspire — exactly the features a README would brag about);
golden tests cover markdown only (HTML/JSON drift silently); `docs/` mixes real reference docs
with eleven agent-session reports and four overlapping status/report files; README is not yet
the hero page; git history records the construction scaffolding.

**Gap.** Coverage holes under advertised features; docs archaeology visible to visitors; history
not presentable.

**Bridge.** In order: close the four extractor test gaps + add HTML/JSON goldens + direct
`AnalysisService` tests (claims become true) → restructure `docs/` (user docs at top level,
`agent-sessions/` and stale reports deleted or moved to an untracked archive) → rewrite README
(what/why/quickstart/demo GIF/philosophy link/honest roadmap) → **then** reset history (squash to
a curated initial commit + meaningful milestones, retag) → release. History reset is the *last*
step and is performed by the maintainer, not an agent. → **Plan 4**

---

## Decisions log (the short version)

| # | Decision | Why |
|---|----------|-----|
| D1 | Budget scoring **during**, enforcement **at render**; cut list always inspectable | Serves unattended *and* attended use without re-analysis (P2) |
| D2 | Immutable `AnalysisSnapshot` + cheap `RenderPlan` lens between analyze and render | Kills re-analysis-on-toggle, the UI lag, and the markdown re-parsing in one move (P1) |
| D3 | `RunReport` is part of every pipeline result; CLI `--stats`, desktop Stats tab | Nerdy transparency is the brand, and it regression-tests the architecture (P3) |
| D4 | User-facing model is **Focus + Depth** (focus = type, method, or endpoint route); scenario/profile become derived + hidden expert flags; one shared resolver in Core; **`--task`/NL intent inference removed** — DevContext is static analysis with controllable filtering, not a query system | Ends the vocabulary soup and CLI/desktop duplication; no pseudo-NLU (P4, P7) |
| D5 | Desktop stays **WPF + BlazorWebView (WebView2)** | HTML is already the human output format; evergreen runtime keeps the zip small; remaining lag is data-flow, fixed by D2 (P6) |
| D6 | Parallelize the independent subset of Stage 3 via declared capabilities; fix CLI sync-over-async | Measured win, surfaced in RunReport (P5) |
| D7 | Repo order: tests → docs → README → history reset → release; history reset is maintainer-manual | Make claims true before making them loud (P7) |
| D8 | The tool validates its own output: invariant self-checks on every run (`--strict` in CI), a machine-checked eval-expectation suite over real repos with an `aspirational`→`expected` ratchet, one gate script run by humans and agents alike | P3's corollary — show your work, *check* your work; quality becomes monotonic and gates need no eyeballs (see `plans/PLAN-0-SELF-VALIDATION.md`, `DETECTION-GUIDE.md`) |

## Deliberately deferred (post-release, by decision — not by oversight)

- **Persistent snapshot cache** — serialize `AnalysisSnapshot` to disk keyed by content hash
  (git HEAD + dirty-file digest) so repeat runs and solution re-opens skip analysis entirely.
  The natural completion of P1/P5; deferred because it's real scope (invalidation, schema
  versioning) and the in-session reuse from Plan 1 captures most of the value. No agent should
  partially implement this during Plans 1–4.
- **LLM-value benchmark** — an honest harness measuring "does an LLM answer codebase questions
  better with DevContext output than with a raw file dump?". The claim stays out of the README
  until this exists.
- **Beyond .NET** — TS/other languages stay roadmap-only this release; the pipeline is
  language-agnostic by design and the README may say exactly that, nothing more.

## Execution order

0. **Plan 0 — Self-validation harness** (`plans/PLAN-0-SELF-VALIDATION.md`) — additive safety
   net (output self-checks, eval-expectation suite, gate script) built *before* the refactor
   so every later session is machine-gated. Detection work follows `DETECTION-GUIDE.md`.
1. **Plan 1 — Analyze once, render many** (`plans/PLAN-1-ANALYZE-ONCE-RENDER-MANY.md`) — the architectural keystone; Plans 2–3 build on its types.
2. **Plan 2 — Unified Focus + Depth UX** (`plans/PLAN-2-UNIFIED-FOCUS-UX.md`) and **Plan 3 — Nerd stats** (`plans/PLAN-3-NERD-STATS.md`) — independent of each other, either order.
3. **Plan 4 — World-class repo** (`plans/PLAN-4-WORLD-CLASS-REPO.md`) — last, so docs/tests describe the post-1/2/3 reality.
