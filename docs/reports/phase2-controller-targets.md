# Phase 2 Capability Report — Universal Entries, Controllers First

> 2026-06-28, Iteration 2 / Phase 2 (commit `4f457a3`+ on branch `feature/iter2-universal-entries`).
> Hybrid approach: a controller fixture for the repeatable CI gate, plus a queued DntSite manual run
> for the "ripgrep test" capability evidence.

## Gate evidence (automated, runs every `gates.ps1`)

**Fixture:** `tests/fixtures/ControllerApp` — 3 controller actions (GET/POST/DELETE `/api/Products`)
calling DI-resolved `ProductService`/`AuditService`, no MediatR.

| Check | Result |
|---|---|
| **detection-count EndpointDetection ≥ 1** | PASS (controller detected) |
| **entry→target GET → ProductService.GetByIdAsync** | PASS |
| **entry→target POST → ProductService.CreateAsync** | PASS |
| **entry→target DELETE → ProductService.DeleteAsync** | PASS |
| **sibling divergence (GET ≠ DELETE)** | PASS (`TraceQualityTests.Controller_sibling_actions_produce_divergent_traces`) |
| **infra `GET /` filtered (eShop)** | PASS (`OpenApi.Extensions.cs` absent from markdown) |
| **CQRS entry→target non-regression (eShop)** | PASS (Sends tried first; eShop 4/20 targets) |

All 15 eval tests pass; `gates.ps1` green (build, fast tests, eval, CLI `--strict` matrix).

## What changed (vs Iteration 1)

1. **`ResolveEntryTarget` generalized beyond MediatR Sends** — after the existing `Sends` check (kept
   first for CQRS), `ResolvePrimaryCall` resolves a controller action to the dominant in-scope service
   callee of the action member (prefer DI-`service`-tagged, else first in-scope non-self callee).
   Controllers go from 0 → resolved. `ResolveViaParentType`'s whole-type crutch is retired.

2. **Infrastructure pseudo-entry filter** — `GET /` from `ServiceDefaults/OpenApi.Extensions.cs` (Scalar
   root) is skipped in `AddHttpEntryPoints`. Matches on source + route, not just `"/"`, so real root
   routes aren't falsely dropped.

3. **Entry dedup** — exact duplicates (verb × route × file × line) collapsed; different lines
   (versioned overloads) kept distinct.

## Queued: DntSite manual capability run

The fixture proves the mechanism; a real controller repo validates the "ripgrep test" claim.
Run this on a DntSite clone (pin a commit) and record results here:

```bash
dotnet <cli> analyze <dntsite-root> --stats
# Expected: entry→target coverage >> 0 (was 0/94 before Phase 2).
# Expected: no infra `GET /` entry; sibling controller actions diverge.
# Expected: CQRS / other entry kinds not regressed.
```

### DntSite run results (fill after run)

| Metric | Before (Iteration 1) | After (Iteration 2) |
|---|---|---|
| Entry→target coverage | 0/94 | TBD |
| Infra `GET /` | present | TBD |
| Sibling controller trace divergence | identical (bug) | TBD |

## Next

Iteration 3 — Complete & honest traces (domain-event chain, pipeline render, TOUCHES non-trivial,
re-probe). See `docs/plans/ITERATION-3-complete-honest-traces.md`.
