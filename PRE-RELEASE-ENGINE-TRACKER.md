# DevContext pre-release - engine and agent face Phase Tracker

**Plan:** DevContext pre-release - engine and agent face | **Branch:** `feat/pre-release-engine` | **Design doc:** docs/dev/research/PRE-RELEASE-PLAN-2026-08-13.md

## Handoff (overwrite this block, ≤12 lines, no history)

V1.2 IS DONE (`fd47300`, evidence eval-results/2026-08-13/v1-vocabulary/MEMBER-TITLE-EVIDENCE.md).
A Member title is now a FUNCTION OF ITS KEY: `SymbolCanon.MemberTitle` (owner short name + "." +
member), applied in ONE place -- `CodeGraphBuilder.AddNode` -- so no producer and no pass ORDER can
pick a vocabulary. All 12 producer sites pass it; **loom-guards rule 10** fails a Member title
spelled anywhere else. Measured with the probe that filed #17, 7 poles: Member 1092 mismatches -> 0,
member COUNT identical pole by pole, and Type 1893/58 + EntryPoint 84/87 + Service + Store UNMOVED.
Eval matrix 74 passed / 0 failed -- no cell moved. Truth gate 0 failures.

NEXT: V1.3 -- the two standing invariants, RED FIRST (#7 rider: no node has kind Type with a member
id; #18: lambda/expression TEXT never becomes a node title). The probe measures #18 for free: those
58 Type mismatches ARE its population -- re-run `eval-results/2026-08-13/v1-vocabulary/
member-title-probe.ps1` (poles are `name=ABSOLUTE_PATH`; the 4 not in eval-repos are cloned at
C:/Code/eval-poles). #18's producers: GraphBuilder.Seams.cs titles from detection ServiceType/
ImplementationType; its "sp =>"/"(" filter is on the DirectBinding path only.
STILL OPEN, #17's other half (NOT in V1.2's acceptance): EntryPoint titles 84/87 -- some producers
keep the key's scheme prefix (`grpc:`/`domain:`/`worker:`), some drop it.
TRAPS PAID FOR: two `dotnet test` hosts at once prints "Test host process crashed" NEXT TO two
"Passed!" lines -- a false red (R-T5), re-run serially; `$LASTEXITCODE` inside `bg start -- powershell
-Command` is eaten by the outer shell (use a .ps1 with -File); inside `namespace DevContext.Core.Tests`
`Graph2.SymbolCanon` resolves to the TEST namespace -- fully qualify it.

## Baseline numbers (from run.db)

| Metric | Value |
|---|---|
| Total checkpoints | 20 |
| Done | 4 |
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
| V1.1 | ONE verified-edge definition (#25): GraphStats/SeamStat and GraphOrphansSource agree, the definition stated in one place, every surface reads it | DONE | abfa564 | eval-results/2026-08-13/v1-vocabulary/EVIDENCE.md |
| V1.2 | ONE member-title helper (#17) next to SymbolCanon used by every producer; the owner-qualified vs bare split gone on the six poles it was measured on | TODO | - | - |
| V1.3 | Two standing invariants land red-first: no node carries kind Type with a member id (#7 rider); lambda/expression text never becomes a node title on any path (#18) | TODO | - | - |

### E1 — W2 edge completeness batch (11, 12 via TextSpan, re-measure 8, 7) + dogfood invariant

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| E1.1 | #11 static type-name-receiver calls produce edges; the dogfood invariant (DevContext's own helper layer has in-edges in DevContext's own graph) added to the battery and proven red first | TODO | - | - |
| E1.2 | #12 fixed via the TextSpan-on-op shape (BodyOp records the invocation's own span; relocate-by-line class killed including the TryBindLocalDeclType / TryBindGenericArg / args-bind sister sites); full matrix run against cells declared BEFORE coding; engine-own approx share recorded before/after (baseline 1103/1383) | TODO | - | - |
| E1.3 | #8 re-measured (its stated mechanism is refuted by #11) and fixed or re-filed with the true mechanism; #7 explicit-interface/BCL-collision Type-node fixed | TODO | - | - |
| E1.4 | Batch acceptance: the probe's class-C impact question resolves the true impact set on eShop (hand-verified against the question key), matrix shows declared cells flipped and no others | TODO | - | - |

### D1 — W5 detection declared-coverage (reachability instrument, hole closures, filed set, rung-4 jobs)

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| D1.1 | Catalog-reachability instrument: for every catalog descriptor a test asserts its signal is reachable and its Kind has a producing path (descriptor→signal→extractor→builder→entry); proven RED on the Orleans and TimedJob finds before any closure | TODO | - | - |
| D1.2 | Reachable-surface holes closed with consumer-app fixtures + expectations: Orleans packages on the descriptor; BackgroundService/IHostedService base-type detector; TimedJob producer or honest deletion of the kind; Avalonia descriptor + WinForms Exe case | TODO | - | - |
| D1.3 | Filed set: #14 generic command verbs (strip type args in leaf comparison, carry the type arg as parent); #20/#19 one source of truth for "what is a service"; #2's detection half (addressable entry names single-sourced); Blazor UI-vs-HTTP distinction; per-detection Confidence read or deleted | TODO | - | - |
| D1.4 | Rung 4: Hangfire and Quartz consumer job entries (attribute + interface shapes), one consumer-app fixture each | TODO | - | - |

### R1 — W6 metric recalibration (22, 23, 24) against the post-E1 graph

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| R1.1 | #22/#23/#24 recalibrated against the post-E1 graph or retired; each surviving threshold carries a comment stating the measurement and the calibration commit | TODO | - | - |

### A1 — W3 re-probe prep (DESIGN amendments, unseen repo) + the 10-dollar adoption gate

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| A1.1 | DESIGN.md amended IN WRITING before any run, per RESULTS §10: unseen-repo protocol (identifier-renamed public repo prepared and committed), question mix (2× class B, E/F at half), non-inferiority bar re-specified at a reachable level, κ-on-20% rule with degenerate-marginal handling, B-instructed arm added | TODO | - | - |
| A1.2 | The pre-registered adoption gate: arm B alone, eShop, 18 runs, unchanged prompt, run as a bg child; mcp_call_share reported against the 0.2 floor with the pre-registered branch stated either way | TODO | - | - |

### Z1 — Close-out: README honesty pass, release-gate statuses, backlog reconciliation

| # | Checkpoint | Status | Commit | Evidence |
|---|-----------|--------|--------|----------|
| Z1.1 | README honesty pass: agent story stated as measured (primer + whatever A1.2 earned), results linked, claims ⊆ measurements; release-gate table statuses recorded | TODO | - | - |
| Z1.2 | BUG-BACKLOG reconciled (fixed closed with evidence, re-measured re-statused); PRE-RELEASE-PLAN §3 stage table updated; full battery green; branch pushed | TODO | - | - |

## Dependencies

```
(none — stages run sequentially by plan order)
```
