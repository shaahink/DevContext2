# Universal Lens — execution plans & live progress

This folder turns `docs/PRODUCT-DIRECTION.md` (the product) into ordered, **fresh-session-resumable**
iterations. Each iteration is one agent session: detailed steps, harness updates, and a gate.

## Start here (fresh session)

1. Read `docs/PRODUCT-DIRECTION.md` (what we're building) and `docs/ACCEPTANCE.md` (what "good" means).
2. Run the gate to see current reality: `pwsh -File eval/gates.ps1` (build → fast tests → eval → CLI).
3. Find the **first iteration below whose Status ≠ DONE** — that's where you pick up.
4. Open its guide, do **Step 0 (Reproduce)** first (line numbers in guides may have drifted; verify
   against current code), then execute the steps.
5. When its **Gate** passes: flip the relevant `docs/ACCEPTANCE.md` checks to `expected`, update the
   **Status** in that guide's header **and the table below**, and record the commit hash.
6. Stop at the iteration boundary. The next session repeats from step 1.

## Status

| # | Iteration (guide) | Phases | Status | Gate that proves it done |
|---|---|---|---|---|
| 1 | [Kernel hygiene + member-origin correctness](./ITERATION-1-kernel-correctness.md) | 0, 1 | **DONE** (commit `4f457a3`; branch `feature/iter1-member-origin`) | `CatalogApi:CreateItem` trace ≠ `:UpdateItem`; `POST /api/orders` has no sibling sends/raises; gates green |
| 2 | [Universal entries, controllers first](./ITERATION-2-universal-entries.md) | 2 | **DONE** (commit `2b3dd12`; branch `feature/iter2-universal-entries`) | Controller fixture 0/3→3/3 `→ target`; sibling actions diverge; infra `GET /` filtered; gates green |
| 3 | [Complete & honest traces](./ITERATION-3-complete-honest-traces.md) | 3 | **DONE** (commit `b9934f5`; branch `feature/iter3-complete-honest-traces`) | Domain-event→handler path + pipeline rendered; Buyer in TOUCHES; truncation explicit; gates green |
| 4 | [Honest Map & detection](./ITERATION-4-honest-map.md) | 4 | **DONE** (commit `a336e25`; branch `feature/iter4-honest-map`) | OrchardCore = ModularMonolith; Catalog scope-stamped; STACK/PACKAGES no `$(`; CRUD not aggregate; controller-trace noise filtered; gates green |
| 5 | Queryable kernel (inverse edges + query API) — *to write* | 5 | **NOT STARTED** (prereq #4 DONE) | Query-API both-direction tests; CLI unchanged through it; **fold in** the DntSite entity-FQN canonicalization (TOUCHES) follow-up |
| 6 | Performance & caching — *to write* | 6 | **BLOCKED** | DntSite warm fast; cold ≪ 41s |
| 7 | Browse UI redo — *to write* | 7 | **BLOCKED** | Re-query does no re-analysis |
| 8 | MCP server — *to write* | 8 | **BLOCKED** | Tool-contract test; re-probe `C + MCP` |
| 9 | Persistent index + GitHub-URL — *to write* | 9 | **BLOCKED** | Re-open near-instant; GitHub URL → honest Map |
| 10 | Coverage-ladder rungs — *to write* | 10 | **BLOCKED** | Per rung: entries resolve; "ripgrep test" passes |

Status values: **NOT STARTED** (ready, prereqs met) · **BLOCKED** (waiting on an earlier iteration) ·
**IN PROGRESS** (+ who/when) · **DONE** (+ commit).

## Conventions

- **One iteration = one session = one gate.** Don't start an iteration whose prerequisite isn't DONE.
- **Reproduce before you change.** Guides cite file:line as of authoring; verify against current code.
- **Harness is part of the work.** Every iteration ratchets `eval/expectations/*.json` and/or
  `TraceQualityTests`. You are authorized to correct gates/expectations (the eval suite is the bar).
- **Acceptance first.** `docs/ACCEPTANCE.md` defines the bar per artifact and per phase; never ship a
  feature whose data the kernel can't honestly answer.

## Map of the docs

- `docs/PRODUCT-DIRECTION.md` — the product (5-artifact set, coverage ladder, one-kernel/three-faces).
- `docs/plans/UNIVERSAL-LENS-ROADMAP.md` — all phases + the audit-issue→phase map.
- `docs/ACCEPTANCE.md` — what "good output" is + the per-phase ratchet.
- `docs/IDEAL-OUTPUT-TARGET.md` — the hand-built shape a great Map/Trace approaches.
- `docs/audit/*` — the source diagnosis these plans address.
