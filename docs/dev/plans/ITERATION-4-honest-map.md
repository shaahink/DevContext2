# Iteration 4 — Honest Map & detection (Phase 4)

> **Status:** DONE · **Phase(s):** 4 · **Prerequisite:** Iteration 3 DONE
> (domain-event chain renders; TOUCHES includes `Buyer` via Calls; pipeline once; honest truncation; gate green).
> **Fresh session? Start at [`README.md`](./README.md).** Required reading:
> `docs/product/PRODUCT-DIRECTION.md`, `docs/dev/plans/UNIVERSAL-LENS-ROADMAP.md` (Phase 4), `docs/product/ACCEPTANCE.md`,
> and `docs/dev/audit/audit-claims-vs-delivery.md` (the OrchardCore/Catalog/DntSite live-runs).
>
> **Progress (2026-06-28):** Phase 4 DONE (commit `a336e25`). Catalog.API now stamps its partial scope
> (`5-project closure of 24-project eShop`); the partial-closure guard suppresses system-level verdicts on
> a slice while whole-eShop keeps `Microservices`. `$(…)` MSBuild-variable noise stripped from STACK +
> PACKAGES (OrchardCore clean). Aggregate detection requires a real `IAggregateRoot` (eShop: `Buyer · Order`
> only; Catalog/CleanArch CRUD: none). Controller traces drop `nameof`/`Ok`/`NotFound` self-call noise.
> OrchardCore was already `ModularMonolith` (Fix 2 gate met — it lacks an Aspire AppHost). Gate: `gates.ps1`
> PASS (18 eval tests incl. `catalog.json`; new style-detector partial-closure tests; regenerated clean-arch
> goldens). **DntSite TOUCHES diagnosed** (115 entities detected; gap is FQN canonicalization between
> EfEntityDetection nodes and call-graph callee nodes) — tracked for Iteration 5. Report: `docs/dev/reports/phase4-honest-map.md`.

**Goal.** The Map never lies about scope or style. OrchardCore ≠ "Microservices"; scope-stamped verdict with
4 honest fixes + trace-noise polish. (Closes Critical 3, Medium 10, Low 16; defers OrchardCore/STACK to
manual reports not in CI.)

---

## Step 0 — Reproduce (measure before changing)
- `dotnet <cli> analyze eval-repos/OrchardCore --stats` → confirm `STYLE Microservices (high)` (false positive
  — 206 modules, 1 web host, Aspire AppHost) and `$(CommonTargetFrameworks)` in STACK.
- `dotnet <cli> analyze eval-repos/eShop/src/Catalog.API --stats` → confirm CRUD types (`CatalogItem`,
  `Brand`, `Type`) are listed under Aggregates / CROSS-CUTTING, and the header reads whole-system with no
  scope stamp.
- **DntSite TOUCHES deferred diagnosis.** Dump the DntSite EfEntityDetections (`--format json` or `--stats`)
  and `GET /Feed/News` depth-6 trace: determine if the FeedsService chain's callees resolve to **tagged**
  entity Type nodes (FQN match), or if the chain returns DTOs / stops at depth. If it's a tagging/FQN
  mismatch → fix in Phase 4 (Fix 1b in the scope/completeness area). If the chain genuinely stops at DTOs
  → record honestly as a reachability limit.
- Confirm `ProjectInfo.OutputType` is populated for eShop/DntSite/OrchardCore so the Microservices
  discriminator works.

## Fix 1 — Scope-stamp + suppress system-style from a partial closure (Critical 3)

### 1a. Scope descriptor
- `SolutionScope`: capture the full solution's project count (`model.Solution.ProjectPaths.Length`) and the
  analyzed closure count (`Projects.Length`). When N < M → `partial (N of M)`.
- `MapModel`: add `ScopeNote` (nullable descriptor string) populated in `MapBuilder.Build` from the scope.
- `MapRenderer.AppendStyle` (or `AppendMapHeader`): render the scope stamp — e.g. `(5-project closure of
  12-project eShop.slnx)`.

### 1b. Suppress system-level style on a partial closure
- In `ArchitectureStyleDetector.Detect`: when the scope is a partial closure (single service / N<M of a
  larger solution), **cap confidence** and **suppress system-level verdicts** (Microservices, ModularMonolith)
  — a single-service slice may state its *local* style (CleanArchitecture/ControllerBased/MinimalApi)
  but cannot pronounce the system architecture. If `partial && (verdict is Microservices or
  ModularMonolith)`, downgrade to the local style with capped confidence and add `scope: partial` to
  evidence.

## Fix 2 — OrchardCore ≠ "Microservices" (Critical 3)

Root cause: the Microservices heuristic requires only `hasAspire && hasAppHost && projectCount >= 3`. A
modular monolith of 206 library projects + a dev AppHost trips it. Builds on Fix 1 (scope-awareness).

- Use `OutputType` to discriminate **service hosts** from **libraries**. A microservice architecture has
  many independent web/exe hosts; a modular monolith has 1 host + many libs. `ProjectInfo.OutputType` is
  already available (`"Exe"` for web/console apps, null/`"Library"` for libraries).
- Count `serviceHosts = model.Projects.Count(p => p.OutputType is "Exe" or null/WebSdk).` Require
  `serviceHosts >= 3` (not just `projectCount >= 3`) for the Microservices score.
- OrchardCore: ≈1 web host → falls to `ModularMonolith` (it has module-named projects) or its structural
  style.
- Fallback if `OutputType` isn't populated: count distinct projects with ≥1 endpoint detection (endpoint-
  per-project, a reliable `Stage3Specific` signal) — same logic, different discriminator.

## Fix 3 — STACK MSBuild-variable filter (Low 16)

- Filter tokens containing `$(` in target-framework strings. Source-side: in the csproj reader where
  `TargetFrameworks` are parsed, strip unevaluated property tokens. Render-side guard in
  `MapRenderer.AppendStack`: `.Where(f => !f.Contains("$("))` as backstop.
- Unit-test with a synthetic `ProjectInfo` having `TargetFrameworks = ["net10.0", "$(CommonTargetFrameworks)"]`;
  assert the `$(` token is absent from STACK output.

## Fix 4 — Tighten DDD aggregate detection (Medium 10)

Root cause: `EfCoreExtractor` sets `IsAggregate = HasOwnDbSet(...) || IsAggregateRootPattern(...)`. The
`HasOwnDbSet` heuristic tags **nearly every DbSet entity** as aggregate — Catalog's anemic CRUD, eShop's
child `OrderItem`/`CardType` alongside genuine aggregates. `IsAggregate` feeds 4 consumers: graph node
tags (`GraphBuilder.AddEntityNodes`), the Map's Aggregates list (`MapBuilder.BuildAggregates`), style
scoring (`ArchitectureStyleDetector`), and legacy renderers. All consume from the **detection**, not the
graph tag, so the fix must be at the source.

- **Drop `HasOwnDbSet` as the primary aggregate signal.** Replace with real DDD evidence:
  - The entity's type in `model.Types` implements/inherits `IAggregateRoot` or `AggregateRoot` (check
    `ImplementedInterfaces` + `BaseTypes` via `model.Types.TryGetValue(entityFqn, ...)`).
  - Keep the name pattern (`IsAggregateRootPattern`).
- **Catalog CRUD → not aggregate** (no `IAggregateRoot`, no aggregate-root name); **eShop `Order` → aggregate**
  (`Entity, IAggregateRoot` base); **eShop `OrderItem` → not aggregate** (child entity, no IAggregateRoot).
- After EfCoreExtractor, the `ArchitectureStyleDetector`'s aggregate count (for the CleanArchitecture
  score) will reflect genuine aggregates — CleanArchitecture score drops slightly but stays correct.

## Fix 5 (polish) — Trace noise: `nameof` + ControllerBase result-helpers

Member-origin precision (Iter 1) surfaced syntactic pseudo-calls as trace children on controller/handler
traces. Two low-risk fixes:
- **`nameof(variable)`** — the syntactic resolver resolves `nameof` as an identifier → `call X.nameof`.
  Filter: skip call edges where `CalleeMethod == "nameof"` (or `== nameof`). In `CallGraphExtractor`
  right after capture, or in `AddCallEdges` as a noise gate. Prefer `CallGraphExtractor` (cleaner model).
- **ControllerBase result-helpers** (`Ok()`/`NotFound()`/`CreatedAtAction()`/`BadRequest()`/`NoContent()`):
  resolved to `this` → self-call on the controller. Filter: if the callee method is a known ASP.NET
  `ControllerBase` helper and the callee type is the same as the caller, skip the edge. In
  `GraphBuilder.AddCallEdges` as a self-call noise filter. Keep the set minimal so non-ASP.NET `Ok`/etc.
  aren't falsely dropped.

## Harness updates (authorized)
- **Unit tests (fast, deterministic, the gate):**
  - `ArchitectureStyleDetectorTests`: OrchardCore-shape model (206 libs, 1 web host, Aspire) ⇒ NOT
    Microservices (expect ModularMonolith or CleanArchitecture). Partial-closure model (analyzed 5 of 12
    projects) ⇒ suppressed/capped system-level verdict.
  - Aggregate-detection unit tests: Catalog-shape (no IAggregateRoot) ⇒ `IsAggregate=false`; Order-shape
    (`Entity, IAggregateRoot`) ⇒ `IsAggregate=true`; child entity (`Entity` only) ⇒ `IsAggregate=false`.
  - STACK `$(` filter unit test: synthetic `ProjectInfo` with MSBuild-variable TFM ⇒ absent from STACK.
  - `nameof` + result-helper noise-filter unit tests.
- **eShop eval JSON ratchet:**
  - `eshop.json`: `output-not-contains "$("` ; Catalog scope-stamp substring (`sh-project closure`) ;
    CRUD types (`CatalogItem`) `output-not-contains` in markdown. *Note:* the Map renderer's Aggregates
    list is on the whole-eShop eval (which has real aggregates — Order/Buyer/…). For the CRUD assertion,
    a unit test on the aggregate heuristic is deterministic; the eval check can be a `signal-present` or a
    unit test.
- **Manual OrchardCore run** → `docs/dev/reports/phase4-honest-map.md` (repo is gitignored + 5239 files — kept
  out of the automated suite, like DntSite).
- Flip `docs/product/ACCEPTANCE.md` Phase-4 checks to `expected`.

## Gate (Phase 4 done when)
- OrchardCore not "Microservices" (unit test + manual run).
- Catalog.API stamped partial-scope; `$(Var)` absent from STACK (unit test + eShop eval ratchet).
- Catalog CRUD types not labelled aggregate (unit test).
- Trace noise (nameof + result-helpers) absent from controller/fixture traces (unit test + eval fixtures).
- `pwsh -File eval/gates.ps1` PASS incl. new unit tests + eval ratchet.

## Pitfalls
- `IsAggregate` feeds the `ArchitectureStyleDetector` CleanArchitecture score — tightening it will drop
  the aggregate count and may lower CleanArchitecture confidence. Keep it grounded in real signals
  (`IAggregateRoot` base, name pattern) — the score dip is honest, not a regression.
- `OutputType` may be null for some projects (pre-.NET 8 SDKs or custom builds). Fall back to endpoint-
  per-project as a stochastic discriminator; both are monotonic (won't regress a genuine Microservices
  detection).
- The full-solution `ProjectPaths` count (M) may not be available when `model.Solution` is null (single-
  project entry). The scope stamp degrades honestly: `(5-project closure of <entry>)` — no M available.
- Re-run Iteration 1–3 gates; scope + aggregate changes must not regress the existing eShop/controller
  assertions. The Catalog scope-stamp MUST show partial = true for a sub-service entry.

## Definition of done → next
Gate green; harness carries the 4 honest-Map guards + trace-noise polish; OrchardCore + DntSite honest
reports recorded; ACCEPTANCE Phase-4 ENFORCED. Update Status above and the table in `README.md`; record
the commit. **Next: Iteration 5 (Phase 5 — queryable kernel: inverse edges + query API).**
