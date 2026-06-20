# Plan 0 — Self-Validation: the Tool Checks Its Own Output

> Run **first**, before Plan 1. Mostly additive (new tests, scripts, one Core validator), so
> it is low-risk — and every later session inherits an automated safety net beyond golden
> tests. Implements the corollary of P3: a tool that shows its work should also *check* its
> work. Three layers: (1) output invariants checked on every run, (2) an eval-expectation
> suite against real repos, (3) a single gate script that both the human and the agent run.

## Ground rules

- Branch off `develop`. Additive only: do not refactor pipeline/extractors here — that's
  Plan 1. If an invariant check requires plumbing (e.g. a hook at the end of rendering), add
  the smallest possible seam.
- Aspirational vs expected (see Layer 2): encoding a *known defect* as a failing check would
  block every session — known defects are recorded as `aspirational` and reported, not failed.
  Plan 4 Phase 0 flips them to `expected` as they're fixed.

## Layer 1 — Output invariants (`OutputSelfCheck` in Core)

New `src/DevContext.Core/Validation/OutputSelfCheck.cs`: a pure static class run after
rendering. Input: the model, render options/plan, and each `RenderedContext`. Output:
`ImmutableArray<SelfCheckResult>` where `SelfCheckResult(string CheckId, bool Passed, string Detail)`.

Checks (implement each as a small private method; read the relevant model/renderer code first
to get field names right — do not guess):

| Id | Invariant |
|---|---|
| `budget-respected` | `EstimatedTokens <= MaxTokens` (+ documented safety margin) |
| `no-empty-sections` | No rendered `## Section` header with zero content beneath it |
| `sections-known` | Every rendered section name exists in `SectionNames` constants |
| `detections-sourced` | Every `Detection` has non-empty `SourceFile` and `LineNumber > 0` |
| `no-duplicate-detections` | No two detections with identical stable key (type+file+line+kind) |
| `no-dynamic-routes` | Markdown/HTML contain no `<dynamic>` route placeholders |
| `json-schema-valid` | JSON output deserializes back into `DevContextOutput` round-trip |
| `html-well-formed` | HTML parses (use `XDocument`-tolerant check or a tag-balance scan; no new package if avoidable) |
| `funnel-consistent` | types discovered = included + excluded (once Plan 1/3 lands; implement the check now, mark inapplicable until the fields exist) |
| `deterministic` | (test-only, expensive) same input rendered twice ⇒ byte-identical |

Wiring:
- Pipeline runs the cheap checks at the end of every analysis; results go into
  `model.Diagnostics` as `Info` (pass) / `Warning` (fail) entries with source `SelfCheck`.
- CLI: new `--strict` flag → any failed self-check = exit code 2 with a red table of failures.
  CI and the gate script use `--strict`.
- Once Plan 3 exists, the RunReport/Stats tab displays `self-check: N/N passed` (note this as
  a TODO comment for Plan 3, don't build the report now).

Tests: one unit test per check with a deliberately broken synthetic input (≥ 10 tests).

## Layer 2 — Eval-expectation suite (real repos, machine-checked)

Structure:

```
eval/
├── expectations/
│   ├── todoapi.json
│   ├── verticalslice.json
│   ├── automapper.json
│   └── eshop.json
└── gates.ps1
```

Expectation file schema (also save as `eval/expectations/SCHEMA.md`):

```json
{
  "repo": "eval-repos/eShop",
  "checks": [
    { "id": "arch-style",       "type": "json-equals",   "path": "$.architecture.style",
      "value": "Microservices", "status": "aspirational",
      "note": "iteration-4: misdetected as MinimalApi" },
    { "id": "aspire-signal",    "type": "signal-present","value": "Aspire", "status": "aspirational" },
    { "id": "endpoint-count",   "type": "json-range",    "path": "$.endpoints.length", "min": 20, "max": 80, "status": "expected" },
    { "id": "no-dynamic",       "type": "output-not-contains", "format": "markdown", "value": "<dynamic>", "status": "expected" },
    { "id": "analysis-time",    "type": "max-elapsed-ms","value": 20000, "status": "expected" }
  ]
}
```

Check types to implement: `json-equals`, `json-range`, `json-contains`,
`output-contains` / `output-not-contains` (per format), `signal-present`,
`detection-count` (by detection type, min/max), `max-elapsed-ms`. JSON paths evaluate against
the `--format json` output — **read `DevContextOutput.cs` first** and write paths that match
the real schema, not the examples above.

Runner: new test class `EvalExpectationTests` in `DevContext.Core.Tests` (or a separate
`DevContext.Eval.Tests` project if Core.Tests' conventions make trait-filtering awkward):
- `[Trait("Category","Eval")]` — excluded from the default CI filter like `CliSmokeTests`;
  the gate script opts in.
- Runs the pipeline **in-process** per repo (markdown + json), evaluates every check.
- `expected` failure ⇒ test failure. `aspirational` ⇒ written to test output as
  `ASPIRATIONAL-FAIL <repo>/<id>: <note>` and counted in a summary assertion message — visible,
  never blocking.
- If an `eval-repos/<name>` folder is missing, skip that repo with a clear message (eval repos
  are not committed; document the clone commands in `eval/README.md` — get the four URLs from
  `docs/` or git history, e.g. dotnet/eShop; record exact pinned commit SHAs so counts are stable).

Seeding the expectation files (the agent does this honestly):
1. Run the current tool against each eval repo; inspect outputs.
2. Encode what is *currently true and correct* as `expected` (counts as ranges, not exact).
3. Encode the iteration-4 defect list as `aspirational`: eShop arch style + Aspire signal,
   VerticalSlice arch style, no-`<dynamic>`-routes, no duplicate endpoints, AutoMapper
   structured output (`json-range` on types under non-Unknown layers), DI rows without
   multi-line lambdas (`output-not-contains` a `=>` + newline pattern inside the DI table —
   inspect actual output to design a robust probe).
4. Anything currently correct that a later plan must not break (TodoApi's clean output) is
   `expected` — this is the refactor safety net.

## Layer 3 — The gate script (humans and agents run the same thing)

`eval/gates.ps1` (Windows PowerShell 5.1-compatible — no `&&`, no ternary):

1. `dotnet build DevContext.sln` (fail fast).
2. `dotnet test` with the default fast filter (exclude Eval + CliSmoke + network E2E).
3. `dotnet test --filter Category=Eval` — print the aspirational-fail lines prominently.
4. CLI matrix with `--strict`: `analyze . --strict`, `--format json --strict`,
   `--format html --strict`, `--dry-run`, `--max-tokens 2000 --strict`.
5. Print a final `GATE: PASS` / `GATE: FAIL (step N)` line and exit non-zero on failure.

Conventions:
- **Every agent session ends by running `eval/gates.ps1` and pasting its tail into the
  summary.** A session that ends on `GATE: FAIL` is not done — fix or report the blocker.
- The human gate (EXECUTION-ROADMAP) becomes: run the same script + the 2-minute desktop
  smoke for desktop-touching plans. Desktop logic-level validation stays in VM tests
  (re-render-without-reanalyze, call counts) — UI pixels remain the only human-checked part.

## Verification of this plan itself

- All new tests green; `gates.ps1` exits 0 on the current codebase; deliberately break one
  invariant (e.g. budget) locally and confirm `--strict` exits 2 and the gate fails (revert).
- Eval suite reports the known iteration-4 defects as aspirational (proves the encoding).
- Add a line to `docs/AGENT-REFERENCE.md`: "before finishing any session, run `eval/gates.ps1`".
