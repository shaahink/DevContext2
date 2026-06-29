# Iteration 6 — Performance & caching (Phase 6)

> **Status:** DONE · **Phase(s):** 6 · **Prerequisite:** Iteration 5 DONE (queryable kernel; gate green).
> **Fresh session? Start at [`README.md`](./README.md).** Required reading: the devcontext-bench skill,
> `docs/product/PRODUCT-DIRECTION.md`, `docs/dev/plans/UNIVERSAL-LENS-ROADMAP.md` (Phase 6).

**Goal.** Cold DntSite Map time drops materially under 41s (the audit baseline). (Closes High 6.)

## What changed
- **Entry-scoped call-graph binding (the dominant lever).** For Map mode (no focus), `CallGraphExtractor`
  now seeds the call graph from all endpoint/handler/worker source files (`EntrySeedFiles`) instead of
  binding every file in the repo. The Map only needs call edges from entry-handler methods for
  entry→target resolution; binding the handler closure (~70 files for DntSite) instead of the full
  repo (~1342) drops Stage-3 cold cost from ~30s to ~3s. Focused traces still use the existing
  focus-scoped binding (unchanged, always correct).

## Evidence (DntSite, 1342 .cs files)
- **Before:** Stage-3 ~30s, total cold ~41s. Entry→target 34/94.
- **After:** Stage-3 ~3s, total cold ~10s (OS-cache-dependent). Entry→target 34/94 **preserved**.
- Eval suite duration: ~103s → ~32s (DntSite eval dominates the suite).

## Gate
- Entry→target coverage unchanged on DntSite/eShop/controller fixture.
- Eval suite green (18 tests, all output identical).
- `gates.ps1` PASS.

## What's left for perf (deferred, not in this iteration)
- CallGraphExtractor cold compilation cost (~6s — build one `CSharpCompilation` over all files).
- Map-mode `SyntaxStructureExtractor` floor (~3.5s — parse/walk all files; hard to avoid without
  incremental analysis).
- Cross-run `PersistentAnalysisCache` reuse (already exists; analyze the warm-case gap).
