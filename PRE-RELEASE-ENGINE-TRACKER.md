# DevContext pre-release - engine and agent face Phase Tracker

**Plan:** DevContext pre-release - engine and agent face | **Branch:** `feat/pre-release-engine` | **Design doc:** docs/dev/research/PRE-RELEASE-PLAN-2026-08-13.md

## Handoff (overwrite this block, ≤12 lines, no history)

A1.2 CLAIMED (4f51931). Evidence eval-results/2026-08-14/a1-adoption-gate/A1.2-EVIDENCE.md
+ ADOPTION-GATE.md beside it. THE GATE CLEARS: median per-run mcp_call_share 0.306 vs the
pre-registered 0.20 floor, so DESIGN §3.1's PROCEED branch fires - W3 runs as specified, arm B
stays primary, B-vs-G remains a test of the MCP. Pilot (22 undescribed tools) was 0.015, 17/18
below floor; this batch (14 curated described tools, prompt byte-unchanged) is 5/18 below.
Pooled 0.354 and mean 0.335 clear too, so the verdict does not turn on the estimator. $6.42,
0 censored, 0 isolation breaches; two devcontextShas in the rows are REPORT.md + the reporting
script only, so one binary under test - verified, not assumed.
THE TRAP THAT ATE s26 AND s27 IS SOLVED, use this shape: a `conductor bg` child dies with its
parent, so LAUNCH FIRST TURN then BLOCK IN-SESSION on a PowerShell poll loop (45-60s sleeps,
8-min foreground chunks) until the row count hits target. 18 runs = 22 min wall; ledger resumes
on (question,arm,rep) so a kill only costs the row in flight.
NEXT: STAGE A1 IS CLOSED (A1.1+A1.2 both DONE) and Z1.1/Z1.2 are the only open cards left in the
run. Z1.1 wants "whatever A1.2 earned" in the README: the honest sentence is the 0.015 -> 0.306
move on an unchanged prompt, and note adoption is not dominance (80 mcp vs 146 native calls).
STILL UNBUILT, owned by nobody: questions/Driewie.json + the 2nd class-B question per repo.


## Baseline numbers (from run.db)

| Metric | Value |
|---|---|
| Total checkpoints | 20 |
| Done | 16 |
| Claimed (unconfirmed) | 1 |

## Checkpoints

Status ∈ TODO · IN PROGRESS · DONE · DONE ✓ (confirmed) · BLOCKED · SKIPPED. Evidence = artifact path produced by a run this
phase (a code path is not evidence). Agent claims are marked DONE; engine confirms as DONE ✓.

### T1 — W1 agent-surface trust pack (bug 5 by measurement, curated menu, partial-truth family, wire-truth gate)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| T1.1 | Bug #5 fixed by measurement: a real MCP `tools/list` call shows a non-empty description for EVERY tool on the wire (mechanism verified against what the ModelContextProtocol SDK actually reads, not assumed); schema tax re-measured and recorded against the P1.2 baseline | DONE ✓ | 76c42dd | eval-results/2026-08-13/t1-wire-truth/T1.1-EVIDENCE.md |
| T1.2 | Menu curated: core agent surface shipped (PRODUCT-DIRECTION §7 set + navigation primitives as the candidate), the rest demoted or folded, retired names handled by the did-you-mean path reading the REAL tool list; `tools/list` shows the curated described menu | DONE ✓ | 76c42dd | eval-results/2026-08-13/t1-wire-truth/T1.2-EVIDENCE.md |
| T1.3 | Confident-partial-truth family closed on the agent path: #6 trace-by-nodeId routes through the resolver `get_context` uses; #10 invalid enum values rejected with the good error envelope; #9 fillNote names elision and the budgetTokens lever; #2 entrypoint names round-trip into `get_context`/`trace` — evidence is real MCP calls showing each new shape | DONE ✓ | cdb152c | eval-results/2026-08-13/t1-partial-truth/T1.3-EVIDENCE.md |
| T1.4 | Wire-truth gate in the battery, proven RED on the pre-fix build then green: every tool described, invalid mode/direction/format rejected, a `found:true` trace has steps or says why not, every elision named | DONE ✓ | c246d77 | eval-results/2026-08-13/t1-wire-truth-gate/T1.4-EVIDENCE.md |

### V1 — W2a one-vocabulary pack (25, 17, 7-rider invariant, 18)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| V1.1 | ONE verified-edge definition (#25): GraphStats/SeamStat and GraphOrphansSource agree, the definition stated in one place, every surface reads it | DONE ✓ | abfa564 | eval-results/2026-08-13/v1-vocabulary/EVIDENCE.md |
| V1.2 | ONE member-title helper (#17) next to SymbolCanon used by every producer; the owner-qualified vs bare split gone on the six poles it was measured on | DONE ✓ | fd47300 | eval-results/2026-08-13/v1-vocabulary/MEMBER-TITLE-EVIDENCE.md |
| V1.3 | Two standing invariants land red-first: no node carries kind Type with a member id (#7 rider); lambda/expression text never becomes a node title on any path (#18) | DONE ✓ | 3eb2f34 | eval-results/2026-08-13/v1-invariants/INVARIANT-EVIDENCE.md |

### E1 — W2 edge completeness batch (11, 12 via TextSpan, re-measure 8, 7) + dogfood invariant

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| E1.1 | #11 static type-name-receiver calls produce edges; the dogfood invariant (DevContext's own helper layer has in-edges in DevContext's own graph) added to the battery and proven red first | DONE ✓ | abfa564 | eval-results/2026-08-13/e1-edges/E1.1-EVIDENCE.md |
| E1.2 | #12 fixed via the TextSpan-on-op shape (BodyOp records the invocation's own span; relocate-by-line class killed including the TryBindLocalDeclType / TryBindGenericArg / args-bind sister sites); full matrix run against cells declared BEFORE coding; engine-own approx share recorded before/after (baseline 1103/1383) | DONE ✓ | 8f0ce0a | eval-results/2026-08-13/e1-span/E1.2-EVIDENCE.md |
| E1.3 | #8 re-measured (its stated mechanism is refuted by #11) and fixed or re-filed with the true mechanism; #7 explicit-interface/BCL-collision Type-node fixed | DONE ✓ | f686e25 | eval-results/2026-08-13/e1-typenode/E1.3-EVIDENCE.md |
| E1.4 | Batch acceptance: the probe's class-C impact question resolves the true impact set on eShop (hand-verified against the question key), matrix shows declared cells flipped and no others | DONE ✓ | 6d0dbac | eval-results/2026-08-14/e1-batch/E1.4-EVIDENCE.md |

### D1 — W5 detection declared-coverage (reachability instrument, hole closures, filed set, rung-4 jobs)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.1 | Catalog-reachability instrument: for every catalog descriptor a test asserts its signal is reachable and its Kind has a producing path (descriptor→signal→extractor→builder→entry); proven RED on the Orleans and TimedJob finds before any closure | DONE ✓ | 78e8998 | eval-results/2026-08-14/d1-coverage/D1.1-EVIDENCE.md |
| D1.2 | Reachable-surface holes closed with consumer-app fixtures + expectations: Orleans packages on the descriptor; BackgroundService/IHostedService base-type detector; TimedJob producer or honest deletion of the kind; Avalonia descriptor + WinForms Exe case | DONE ✓ | 78e8998 | eval-results/2026-08-14/d1-coverage/D1.2-EVIDENCE.md |
| D1.3 | Filed set: #14 generic command verbs (strip type args in leaf comparison, carry the type arg as parent); #20/#19 one source of truth for "what is a service"; #2's detection half (addressable entry names single-sourced); Blazor UI-vs-HTTP distinction; per-detection Confidence read or deleted | DONE ✓ | 796843f | eval-results/2026-08-14/d1-filed/D1.3-EVIDENCE.md |
| D1.4 | Rung 4: Hangfire and Quartz consumer job entries (attribute + interface shapes), one consumer-app fixture each | DONE ✓ | a221296 | eval-results/2026-08-14/d1-rung4/D1.4-EVIDENCE.md |

### R1 — W6 metric recalibration (22, 23, 24) against the post-E1 graph

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| R1.1 | #22/#23/#24 recalibrated against the post-E1 graph or retired; each surviving threshold carries a comment stating the measurement and the calibration commit | DONE ✓ | 557537c | eval-results/2026-08-14/r1-metrics/R1.1-EVIDENCE.md |

### A1 — W3 re-probe prep (DESIGN amendments, unseen repo) + the 10-dollar adoption gate

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| A1.1 | DESIGN.md amended IN WRITING before any run, per RESULTS §10: unseen-repo protocol (identifier-renamed public repo prepared and committed), question mix (2× class B, E/F at half), non-inferiority bar re-specified at a reachable level, κ-on-20% rule with degenerate-marginal handling, B-instructed arm added | DONE | 1d30e02 | eval-results/2026-08-14/a1-preregistration/A1.1-EVIDENCE.md |
| A1.2 | The pre-registered adoption gate: arm B alone, eShop, 18 runs, unchanged prompt, run as a bg child; mcp_call_share reported against the 0.2 floor with the pre-registered branch stated either way | IN PROGRESS | - | - |

### Z1 — Close-out: README honesty pass, release-gate statuses, backlog reconciliation

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| Z1.1 | README honesty pass: agent story stated as measured (primer + whatever A1.2 earned), results linked, claims ⊆ measurements; release-gate table statuses recorded | TODO | - | - |
| Z1.2 | BUG-BACKLOG reconciled (fixed closed with evidence, re-measured re-statused); PRE-RELEASE-PLAN §3 stage table updated; full battery green; branch pushed | TODO | - | - |

## Dependencies

```
(none — stages run sequentially by plan order)
```
