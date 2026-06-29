# Detection Guide — How to Read, Change, and Improve Extractors

> The detections ARE the product. This guide is binding for any agent (or human) touching
> `src/DevContext.Core/Extractors/`, pruning/scoring, or detection rendering. It encodes the
> value model, the change workflow, and the anti-patterns that have historically caused
> regressions. Companion docs: `DESIGN-PHILOSOPHY.md` (why), `plans/PLAN-0-SELF-VALIDATION.md`
> (the harness every change must pass).

## 1. The value model — what makes a detection worth emitting

A detection earns its tokens only if all five hold:

1. **Truthful** — a false positive is *worse* than a miss. The output is pasted into an LLM
   prompt; a wrong "this app uses RabbitMQ" poisons every downstream answer. When confidence
   is genuinely low, either drop the detection or emit it with `Confidence = Low` *and* a
   reason the renderer shows. Never present a guess as a fact.
2. **Sourced** — `SourceFile` + `LineNumber` always set. An unsourced claim can't be verified
   by the user or the LLM. (Enforced by the `detections-sourced` self-check.)
3. **Deduplicated** — one fact, one row. Multi-targeting, partial classes, and source
   generators all produce duplicate syntax; dedupe on a stable key (handler type + route +
   verb, not file path alone) before adding to the model.
4. **Ranked** — every detection should contribute to relevance scoring (`PatternRelevancePruner`
   boosts detected types). If you add a detection type and don't wire it into scoring, focused
   output won't surface it — half the value lost.
5. **Renderable in all three formats** — markdown (LLM), HTML (human), JSON (machine). A
   detection that only renders in markdown is a bug; goldens + self-checks catch missing
   sections, not missing *content quality* — review all three outputs manually once per change.

**Per-section value rubric** (what the consumer actually needs — optimize for this, not for
completeness):

| Section | Valuable | Noise to suppress |
|---|---|---|
| Endpoints | verb, route, handler type+method, auth | duplicate rows per target framework, `<dynamic>` placeholders (resolve or omit the route, never print the placeholder) |
| DI registrations | service → impl, lifetime, shape | multi-line lambda bodies (collapse to `factory(...)`), framework-internal registrations, >~40 unranked rows |
| Data model | entities, keys, DbContext ownership, aggregate roots | migration/designer artifacts |
| Call graph | the path from focus point, depth-limited | unrooted full-graph dumps |
| Architecture overview | style **with evidence and confidence**, project graph | repeating the raw project list a second time |
| Events/messaging | bus kind, message type, producer→consumer pairing | every `Publish`-named method on non-bus types |
| Anti-patterns | actionable, severity-ranked, with location | style nitpicks; anything an analyzer/linter already covers better |

## 2. The change workflow (always in this order)

1. **Reproduce on a real repo.** Run against an `eval-repos/` project (or add a public repo
   that exhibits the case — pin its commit SHA in `eval/README.md`). Save the before-output.
   Never start from an imagined example.
2. **Write the failing test first** — minimal inline-source fixture using the existing
   `FakeFileSystem` pattern (copy the style of `ControllerActionExtractorTests`). One test per
   syntax shape, plus at least one **negative** test (code that looks similar but must NOT
   match — this is the false-positive guard, and it is not optional).
3. **Locate the right home.** Which extractor owns this fact? Check the stage contract:
   - Stage 1/2 (Generic): must not read architecture signals (the pipeline's
     `ValidateExtractors` warns); runs on everything; must be fast.
   - Stage 3 (Specific): gated by `ShouldRun(signals)` — if you broaden a detector, check
     whether its gate still admits the new case (a perfect matcher behind a wrong gate finds
     nothing; the Plan-3 stats "skipped: signal gate" row makes this visible).
   - If no extractor owns it, see §4 (new extractor checklist) before bolting it onto a
     neighbor.
4. **Prefer syntax, fall back to semantics.** Default path is syntax-tree matching via
   `AnalysisCache` (fast, works with `--no-roslyn`). Use the Roslyn semantic model only when
   syntax is genuinely ambiguous (e.g. resolving an interface implemented in another project),
   and always behind the `AllowRoslyn`/provider check — **every detection must degrade
   gracefully to its syntax-only quality level when Roslyn is off**, never to a crash or a
   silently empty section (emit a diagnostic: `"X detection reduced: running without Roslyn"`).
5. **Match by structure, not by string.** Match invocation shapes, attribute names, base
   types — not file paths or substrings of source text. If you find yourself widening a regex
   until the case passes, stop: that's how false positives are built. Path-based heuristics
   are allowed only in `LayerClassifier`-style classification, with `Confidence = Low`.
6. **Verify the funnel, not just the test.** After the unit test passes: run
   `analyze <eval-repo> --stats --include-diagnostics` and confirm (a) the detection appears,
   (b) it survives scoring into a default-budget render or you can articulate why it
   shouldn't, (c) extractor timing didn't regress noticeably, (d) all three formats render it.
7. **Run the harness**: `eval/gates.ps1`. Update goldens only deliberately
   (`UPDATE_GOLDENS=1`) with the diff explained in the commit message. If your change fixes an
   `aspirational` eval check, **flip it to `expected`** in the same commit — that's the
   ratchet that makes quality monotonic.

## 3. Confidence semantics (be consistent)

- `High` — syntactic certainty: the attribute/call/base-type is literally there.
- `Medium` — framework convention: name/shape implies it (e.g. `*Controller` suffix,
  convention routing).
- `Low` — heuristic inference: directory names, string contents, statistical signals. Low
  detections must carry a human-readable reason and should render visually de-emphasized.

Renderers may filter by confidence per scenario; never inflate confidence to make a detection
survive filtering — fix the scoring instead.

## 4. New extractor checklist

A new extractor ships only with ALL of:
- [ ] Stage + `[ExtractorOrder]` chosen with rationale; `Capabilities` (description,
      reads/writes signals) declared honestly — the pipeline validation and Plan-1 wave
      assignment depend on them.
- [ ] `ShouldRun` gate based on a Stage-2 signal (a Stage-3 extractor that always runs needs a
      written justification).
- [ ] A `Detection` subtype in `Models/Detections.cs` with sourced fields; a `SectionNames`
      constant; rendering in **all three** renderers; scoring hook (does
      `PatternRelevancePruner` know about it?).
- [ ] Tests: ≥ 3 positive shapes, ≥ 1 negative, ≥ 1 `ShouldRun`-gate test (signal absent ⇒
      zero detections); golden regeneration if defaults changed.
- [ ] Appears correctly in `--dry-run` plan output and in the stats extractor table.
- [ ] An eval expectation (`detection-count` range) on at least one eval repo that exercises it.

## 5. Architecture-style detection (the known weak spot)

`ArchitectureStyleDetector` scores 7 styles; iteration-4 found misclassifications (eShop ⇒
"MinimalApi" instead of microservices-with-Aspire; VerticalSlice ⇒ "MinimalApi"). Approach for
improving it — **evidence-based, not rule-stacking**:

1. Make the detector return *evidence*, not just a label: each style's score should accumulate
   named evidence items (`"Aspire AppHost project"`, `"7 services referenced by AppHost"`,
   `"MediatR handlers in feature folders"`). Persist evidence on the model.
2. Composition over exclusion: real codebases are hybrids. Output should allow a primary style
   + secondary traits (`Microservices (Aspire) · services internally MinimalApi`) rather than
   forcing one winner. The current single-enum result is the root cause of the eShop bug —
   endpoint-style evidence outvotes topology evidence. Topology signals (multiple hosts,
   AppHost orchestration, service references) must rank above intra-service endpoint style.
3. Render label + confidence + top-3 evidence lines in every format. An honest
   `"NLayer (medium confidence — via folder convention)"` beats a confident wrong answer
   (P3 applies to detections too).
4. Test with the 4 eval repos as fixtures-by-expectation (eval suite), plus unit fixtures per
   style; add a hybrid fixture asserting primary+secondary.

## 6. Anti-patterns (rejected in review, no exceptions)

- Catching `Exception` and continuing **silently** — the pipeline already converts extractor
  failures to diagnostics; inner swallowing hides real misses.
- Adding a config flag/option to avoid deciding a default. Decide; flags are for genuine
  user-context differences, not for shipping both halves of an argument.
- Fixing output problems in the **renderer** when the model is wrong (e.g. filtering duplicate
  detections at render time — dedupe at the source; all three formats benefit).
- Editing golden files by hand. Always regenerate.
- "Improving" a detection by deleting its negative tests.
- Tuning scoring/pruning constants against a single repo — any constant change must be
  re-checked against all four eval repos (the suite makes this one command).
