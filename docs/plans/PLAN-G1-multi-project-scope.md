# Plan: G1 — multi-project / closure scope

> Status: **draft for buy-in.** Foundational; do not start coding before the scope-policy
> decision below is made. Prerequisite landed: `.slnx` parsing (`a21d5e9`).
> Assessment: `docs/reports/OUTPUT-QUALITY-ASSESSMENT.md` G1. Branch: `feat/output-quality-graph`.

## Problem

Pointing at a project **subfolder** (`eShop/src/Ordering.API`) analyses only that one project's
closure of source files. Symptoms:
- Map says `(1 project)`, topology is a single node, no constellation.
- The Trace can't cross into `Ordering.Domain` / `Ordering.Infrastructure`, so the two seams the
  ideal (`IDEAL-OUTPUT-TARGET.md` §2) exists for never appear: the **data** seam (SaveChanges → EF
  tables, which live in Infrastructure) and the **domain-event** seam (aggregate ctor raises →
  handler, which crosses Domain→Application).

Root cause (confirmed in code this session):
- `AnalyzeCommand`/`AnalysisService` set `DiscoveryContext.RootPath = rootResult.RootPath` — the
  **input folder**. `FileTreeExtractor` populates `Analysis.AllSourceFiles` by walking from RootPath,
  so only the subfolder's files are discovered. Meanwhile Roslyn loads the *whole* solution from
  `rootResult.SolutionFilePath` — so the graph half-bridges, but discovery (style/topology/entries)
  sees one project. That mismatch is G1.
- `SolutionScope.FromModel` (`src/DevContext.Core/Graph/SolutionScope.cs`) is also broken: it matches
  `sln.ProjectPaths` (relative, e.g. `src/Ordering.API/Ordering.API.csproj`) against
  `p.FilePath` (absolute) → never matches → silently falls back to "all discovered projects". So even
  when a solution is known, scoping isn't precise.

## Decision needed (scope policy) — pick before coding

| Option | Behaviour when pointed at a project/subfolder | Pros | Cons |
|---|---|---|---|
| **A. Project-closure (recommended)** | Analyse the anchor project + its transitive `ProjectReference` closure (Ordering.API → Domain, Infrastructure, EventBus…) | Relevant; surfaces the data/domain-event seams; bounded (~3–5 projects) | Need closure resolution; "what's relevant" judgement |
| **B. Whole solution** | Analyse every project in the resolved `.sln`/`.slnx` | Simplest; matches Roslyn; correct constellation | eShop = 24 projects → perf + noise (unrelated Catalog/Basket); entry list explodes |
| **C. Hybrid** | Subfolder/project input → **A**; `.sln`/repo-root input → **B** | Best of both; matches user intent | Two code paths |

Recommendation: **C (hybrid)** — input intent already distinguishes "a project" from "the repo".

## Phases

**Phase 0 — fix `SolutionScope.FromModel` path matching (low-risk, do first).**
Resolve `sln.ProjectPaths` to absolute (relative to the solution dir) before comparing to
`p.FilePath`; normalise separators. Add a test (relative `.slnx` paths + absolute project paths →
correct scoped set). This alone makes topology/scope precise for the **root-pointing** case and is
safe to land independently.

**Phase 1 — resolve the scan set.** In `ProjectRootResolver`, when the input resolves to a solution
(directly or by walk-up), also return: the **anchor project** (the `.csproj` owning the input path, if
any) and the **scope set** of project directories per the chosen policy. For closure (A), parse
`ProjectReference` transitively from the anchor's `.csproj`.

**Phase 2 — thread the scope into discovery.** Today everything keys off the single
`DiscoveryContext.RootPath`. Add `ScopedProjectDirs` (the resolved scan set) to `DiscoveryContext`;
have `FileTreeExtractor` walk the union of those dirs (deduped) instead of one root, and reuse
`SolutionScope.Contains` to filter. Keep single-RootPath behaviour when no solution/scope is resolved
(folder mode unchanged).

**Phase 3 — eval + goldens.** Re-run `EvalExpectationTests`; several `eval/expectations/*.json` assume
single-project output and must be **ratcheted** to the now-correct multi-project shape. Regenerate
goldens for any fixture whose topology/entries/style change (`$env:UPDATE_GOLDENS=1`).

**Phase 4 — perf guardrails.** Measure eShop (closure vs whole-solution) wall time. Add a project-count
cap / parallel file walk / Roslyn load timeout if needed. Document the cap.

## Risks / consequences

- **Perf:** whole-solution eShop = 24 projects; closure ~4. Roslyn workspace + file walk scale with it.
- **Eval drift:** expectations assuming `(1 project)` flip — intended, but each needs review not blind
  regeneration.
- **Noise:** more entries/packages; G2 lists all entries (could get long → may need grouping, see G9).
- **VerticalSlice two-solutions:** root has `Clean.Architecture.slnx` + `MinimalClean/…slnx`; the
  resolver currently picks the shallowest. Whole-solution policy must pick deterministically and not
  union independent solutions (that's what `SolutionScope` is for — keep one scope per run).

## Verification (definition of done)

- `analyze eShop/src/Ordering.API` → Map shows Ordering.API + Domain + Infrastructure (+ closure),
  `CleanArchitecture`, real topology edges.
- A trace from `POST /api/orders/` reaches the **data** seam (SaveChanges → Order/OrderItem) and the
  **domain-event** seam (OrderStarted… → handler) across project boundaries.
- `dotnet test` green (eval ratcheted, goldens regenerated); eShop wall time within the agreed budget.

## Files
`src/DevContext.Core/Resolvers/ProjectRootResolver.cs`, `src/DevContext.Core/Graph/SolutionScope.cs`,
`src/DevContext.Core/Models/DiscoveryContext.cs`, `src/DevContext.Core/Extractors/Generic/FileTreeExtractor.cs`,
callers `src/DevContext.Cli/Commands/AnalyzeCommand.cs` + `src/DevContext.Desktop/Services/AnalysisService.cs`,
plus `eval/expectations/*.json` and `tests/goldens/*`.
