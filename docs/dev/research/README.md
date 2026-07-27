# Research Program — post-Prism redesign (started 2026-07-27)

> Owner direction: DevContext is a greenfield, unpublished open-source repo. Deep surgery is
> allowed. Be pragmatic about turning it into a proper tool. The four strands below can run in
> parallel (separate sessions/worktrees); each doc is self-contained for a cold start.

## Execution plan

**[PLAN.md](PLAN.md) is the entry point for a fresh session** — sequencing (R1+R2 interleaved →
R3 on the cleaner engine, R4 parallel), session map S1–S6, batch discipline, token-economy rules,
and the STATUS board tracking where the program stands. Read its §2 STATUS first, then only what
your session needs.

## Strands

| Doc | Strand | Session goal |
|---|---|---|
| [R1-GRAPH-TRUTH.md](R1-GRAPH-TRUTH.md) | Graph misdetection audit — **breadth first** | Verify/expand the defect inventory across ALL supported .NET repo shapes (not just eShop), so the fix list is complete before batching |
| [R2-ENGINE-BATCH-FIXES.md](R2-ENGINE-BATCH-FIXES.md) | Engine fixes as **batches**, not one-at-a-time | Deep-audit what's required + missing → apply batch fixes → build/test ONCE per batch. Kills the "one fix costs a session" pattern |
| [R3-FEATURE-REDESIGN.md](R3-FEATURE-REDESIGN.md) | Feature/UI redesign decisions | Walk mock-up alternatives (text/HTML), decide the key features, then implement to the decided spec |
| [R4-MCP-PROPER-TOOL.md](R4-MCP-PROPER-TOOL.md) | MCP → a tool agents actually want | Land the MCP fixes, then a REAL dogfood session (use it to develop/read something), grade usefulness honestly |

## Source material (2026-07-27 audits — where the evidence lives)

- **Live app drive findings**: `eval-results/2026-07-27/ui-feature-audit/FINDINGS.md` (+ `evidence/` PNGs).
  Five poles driven: eShop, FluentValidation, GitVersion, CleanArchitecture, aspire-samples.
  Drivers: `src/DevContext.App/scripts/ui-redesign-drive*.mts` (Playwright, headless — reusable).
- **Engine/consumption audits** (same day, 4 deep-dives): condensed into R1/R2/R4. File:line refs
  in those docs marked [audit] came from the deep-dives — **re-verify against source before fixing**
  (they were accurate at fa9d706 but are agent-reported, not human-confirmed).
- Standing docs: `docs/dev/HANDOVER-PRISM.md` (§4 architecture deltas, §5 known-latents),
  `eval-results/2026-07-17/lens-audit/AUDIT.md` + `EXPERIENCE-ADDENDUM.md`,
  `docs/dev/NOTABLE-FINDINGS.md`, `docs/dev/archive/conductor-DEBT.md`.
  Do NOT trust: `GAP-TRACKER.md` (dead), `FEATURE-FLOW-EXPLAINER.md` (stale),
  go-to-program tracker status columns (over-claim W9 + Addendum-A).

## Evidence-status legend used in these docs

- **VERIFIED** — reproduced this session with a citable artifact (screenshot, CLI output, node card).
- **[audit]** — reported by the 2026-07-27 code deep-dives with file:line; spot-check before acting.
- **HYPOTHESIS** — suspected root cause; needs a code read or probe to confirm.

## Standing constraints

- Prism train (`feat/prism-d1…d5`) is still **unmerged**; the single owner-signed merge to `develop`
  is pending. Branch new work off the d5 tip or wait for the merge — owner's call. NEVER merge unasked.
- Keep the determinism seals intact (SealableBag/OrderedTypes/insertion-order/call-site edge canon —
  `HANDOVER-PRISM.md` §4). Any resolver/identity surgery must keep `DeterministicOrderTests` green.
- Gate cadence: `eval/gates.ps1 -Scope engine|app` during work, full battery only at batch close
  (see `feedback-gate-battery-cadence` discipline; launch full battery detached and overlap).
