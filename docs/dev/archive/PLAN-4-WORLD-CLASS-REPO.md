# Plan 4 — World-Class Repo: Tests, Docs, README, History

> Implements P7 of `docs/DESIGN-PHILOSOPHY.md`. Run **last**, after Plans 1–3, so everything
> written here describes the final architecture. Order within this plan matters:
> make claims true (tests) → clean the house (docs) → write the front page (README) →
> reset history (maintainer-manual) → release.

## Ground rules

- Branch off the Plan 1–3 result. The history-reset phase (Phase 5) is **not** executed by the
  agent — it's a checklist the maintainer runs by hand. Everything else is agent-executable.
- "Genuine" is the constraint: no badge, claim, or doc sentence that isn't backed by code or a
  passing test. When coverage is missing, the README says so in the roadmap instead of hiding it.

## Phase 0 — Output-quality eval gate (the demo must tell the truth)

Before any presentation work: verify the analysis output is *correct* on the standing eval
repos at `eval-repos/` (TodoApi — minimal API; VerticalSlice — FastEndpoints + CleanArch;
AutoMapper — library; eShop — Aspire microservices). Iteration-4 benchmarks (2026-06-05) found
defects that iterations 5–7 may or may not have fully fixed — re-verify, don't assume:

The defects are already machine-encoded as `aspirational` checks in `eval/expectations/*.json`
(built by Plan 0): eShop arch style + Aspire signal, VerticalSlice arch style, `<dynamic>`
routes, DI lambda noise, AutoMapper structured output, duplicate endpoints/middleware.

1. Run `dotnet test --filter Category=Eval` — the remaining `ASPIRATIONAL-FAIL` lines ARE the
   triage list. (Plans 1–3 may have fixed some incidentally.)
2. Work each one following `docs/DETECTION-GUIDE.md` §2 (reproduce → failing test → fix at the
   source → verify funnel → gates). The architecture-style items follow §5 specifically
   (evidence-based scoring, primary + secondary styles, confidence shown).
3. For every fix: flip the check `aspirational` → `expected` in the same commit (the ratchet).
4. Genuinely-hard leftovers: keep `aspirational`, and add each to the README
   "Known limits / Roadmap" section in Phase 3 **honestly** instead of papering over.
5. The repo chosen for the README demo block (Phase 3) must have zero aspirational fails.

This phase gates the rest of the plan: a beautiful README around a wrong architecture label is
a failed release.

## Phase 1 — Close the advertised-feature test gaps

The four extractors with zero dedicated tests are exactly the features the README will name.
For each, follow the existing extractor-test pattern (read 2–3 files in
`tests/DevContext.Core.Tests/` covering e.g. `EndpointExtractor`, `EfCoreExtractor` to copy the
fixture style — `FakeFileSystem` + inline source strings):

1. **`EventBusExtractor`** (≥ 5 tests): detects MassTransit/RabbitMQ-style consumers (read the
   extractor to see which APIs it matches — write one test per matched API shape), produces
   `MessageConsumerDetection` with correct `BusKind`/`MessageType`/`ConsumerType`, ignores
   non-consumer classes, respects its `ShouldRun` signal gate (signal absent ⇒ no detections).
2. **`InMemoryEventBusExtractor`** (≥ 4 tests): `EventFlowDetection` for publish/subscribe
   pairs, `Kind` correctness, no false positives on unrelated `Publish` methods.
3. **`IndirectWiringDetector`** (≥ 4 tests): reflection/`Activator`/convention wiring produces
   `IndirectWiringDetection` with caller/target; plain DI does not.
4. **`AspireExtractor`** (≥ 4 tests): AppHost resource declarations → `AspireResourceDetection`;
   references → `AspireRelationshipDetection`; non-Aspire project → nothing.
5. **`AnalysisService` direct tests** (Desktop.Tests, ≥ 5): success path returns snapshot,
   unknown scenario error, cancellation returns `Error = "Cancelled"`, section filtering reaches
   the render request, settings round-trip (`SaveSettings`/`LoadSettings`).
6. **GitHub VM flow** (≥ 3, NSubstitute a clone-service interface — if `GitCloneService` is
   concrete-only, extract `IGitCloneService` first): valid URL triggers clone before analysis,
   invalid URL surfaces validation message, cleanup runs per setting.
7. **HTML + JSON golden tests**: extend the golden test harness (read
   `GoldenTestHelper`) with `tests/goldens/*.html` and ensure JSON goldens cover the
   `runReport`-bearing 1.1 schema. Keep HTML goldens structural where volatile values exist
   (timings!) — normalize: replace `\d+ms`/token counts via regex before comparison, and
   document the normalization in the helper.

Acceptance: `dotnet test` green; new tests fail when their extractor is deliberately broken
(spot-check one per extractor by temporarily inverting a condition — do not commit the break).

## Phase 2 — Docs restructure

Target tree (top level = things a visitor should see; everything else goes away):

```
docs/
├── getting-started.md        (new: install CLI + desktop, first analysis, 5 min)
├── cli-reference.md          (kept, updated to Focus+Depth vocabulary from Plan 2)
├── desktop-ui.md             (kept, updated: tabs incl. Stats, question row)
├── configuration.md          (kept: devcontext.json)
├── architecture.md           (new: condensed from AGENT-REFERENCE.md + the analyze/render
│                              split; the "how it works" page with the 6-stage diagram)
├── DESIGN-PHILOSOPHY.md      (kept — linked from README)
├── output-schema.md          (from docs/schemas/, document JSON schema 1.1)
├── adr/                      (kept — real ADRs are a credibility asset; add ADR for D2
│                              snapshot/lens and D5 WebView2 decision, backfilled honestly
│                              and dated)
└── plans/                    (kept until executed, then delete in Phase 5 prep)
```

Delete (git history reset in Phase 5 makes this permanent; until then `git rm`):
`agent-sessions/` (all 11 files), `COMBINED-BENCHMARK-REPORT.md`, `COMPREHENSIVE-TECHNICAL-REPORT.md`
(superseded by architecture.md — port any still-true tables first, e.g. threading model,
extractor inventory), `STATUS-REPORT.md`, `report-cleanup.md`, `report-final.md`,
`report-v4-overview.md`, `gap-analysis-dntsite.md`, `design.md` (merge surviving content into
architecture.md), `examples/` (replace with one curated example under getting-started).
Grep for inbound links to every deleted file (README, AGENT-REFERENCE, code comments) and fix.

`AGENT-REFERENCE.md` stays at repo root or docs/ as the contributor/agent guide — update it to
post-Plan-1/2/3 reality (data flow section, new types, test layout). CLAUDE.md if present:
same treatment.

## Phase 3 — README (the front page)

Structure (write it; placeholder GIF noted for the maintainer):

1. **Hero:** one sentence (use the P0 pitch from DESIGN-PHILOSOPHY.md), badges that are true:
   build (actions workflow exists), NuGet version, license. No coverage badge unless coverage
   is actually wired.
2. **The 30-second demo:** `dotnet tool install -g DevContext.Cli` →
   `devcontext analyze https://github.com/dotnet/eShop --focus "POST /api/orders"` → trimmed
   real output block (run it, paste it, trim honestly with `[...]`). `[demo.gif placeholder —
   maintainer records: desktop analyze + section toggle + stats tab]`.
3. **What it extracts:** the detection table (12 types) condensed to one scannable list.
4. **How it decides what to show:** 4 sentences on score-then-budget + the cut list +
   `--stats` — link DESIGN-PHILOSOPHY.md. This is the differentiator paragraph; spend effort.
5. **CLI vs Desktop** table (platforms, install, what each is for).
6. **Quickstart for both surfaces**, link getting-started.md.
7. **Honest roadmap:** TS/other-language analysis status (if .NET-only today, say ".NET today;
   the pipeline is language-agnostic by design"), known limits (private repos, Roslyn
   requirements).
8. **Contributing + license.**

Tone: confident, concrete, zero marketing adjectives without a number or link behind them.

## Phase 4 — CI hygiene

1. PR workflow: build + `dotnet test` (excluding network-dependent `GitHubAnalysisE2ETests`
   and `CliSmokeTests` via the existing filter convention) on push/PR to `main`/`develop`.
   Verify one exists; create/repair if not.
2. Release workflow (`release.yml` exists): add the desktop publish **size check** — fail if
   `DevContext.Desktop.zip` > 110 MB (P6 budget) — and a smoke run of the published CLI
   (`devcontext analyze <small fixture> --dry-run`).
3. `dotnet format --verify-no-changes` (or the repo's analyzer config) in PR workflow; fix the
   fallout once.

## Phase 5 — History reset (maintainer-manual checklist; agent prepares only)

Agent prepares: ensure working tree is the final state, all plans executed, `docs/plans/`
deleted, README done, tags noted (`git tag -l`), and writes the current `develop` SHA into the
PR description as the pre-reset anchor.

Maintainer then runs (not the agent):

```bash
git checkout develop
git checkout --orphan fresh
git add -A && git commit -m "DevContext v2.0 — .NET solution context for humans and LLMs"
git branch -M fresh main                  # replaces main
git push --force origin main
git tag -d $(git tag -l) && git push origin --delete <old tags>   # retag from new root
git tag -a v2.0.0 -m "v2.0.0" && git push origin v2.0.0
# keep a private backup ref first: git branch backup/pre-reset <old-sha>; push to a private remote
```

Decisions the maintainer makes at that moment (documented here so they're deliberate):
single squashed root vs. 5–10 curated milestone commits (recommendation: single root + tag;
fabricated milestone history violates the "genuine" rule); whether `develop` continues or
trunk-based on `main` (recommendation: trunk + release tags for a solo project).

## Phase 6 — Final verification

- Fresh clone → `dotnet build` → `dotnet test` green on a machine without prior state.
- `dotnet tool install` from the produced nupkg locally; run against a real OSS solution.
- Desktop zip: unzip on a clean Windows VM/profile, runs without SDK, cold start ≤ 2 s,
  analyze + toggle + stats works.
- Read the README top to bottom against the rule: every claim has a test, a doc, or a number.
