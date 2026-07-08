# Loom — Conductor-Discovered Debt & Followups

**Generated:** 2026-07-08 by Conductor Baton cross-project audit.
**Read order for a debt-resolution session:** this file → `LOOM-START.md` handoff → stage plan in `docs/dev/briefs/proposal-loom.md` → handover in `.conductor/handovers/L<N>.md` → relevant source.

This file records every deferred bug, weak spot, and unresolved followup found by the 5 Conductor
audit sessions (L0–L4). Each entry is grouped by owning stage, sized for one session, and gated.

---

## L0.4 — Truth gate auto-enforcement + [TruthPending] guard

**Session size:** small (~30 min)
**Files touched:**
- `scripts/loom-guards.ps1` (add truth gate invocation)
- `tests/DevContext.Core.Tests/TruthExpectationTests.cs` (guard fix)
- `eval/mcp-qa/run-cold.js` (B9 denominator fix, optional)

**Background (from L0 audit, `.conductor/handovers/L0.md` §3):**
The truth tests (`Category=Truth`) are excluded from the conductor's automated battery (`dotnet test --filter "Category!=Eval"`). The conductor cannot detect a truth regression, and more importantly, when L1/L2/L7 flip a `[TruthPending]` test green, the conductor will NOT verify it. The harness exists and is real, but its enforcement is manual. **This was flagged as the single largest weakness of L0** and deferred to L1.4 — but through L5, the truth gate remains unenforced.

Additionally, the 4 `[TruthPending]` bodies still use silent-return fixture guards (`if(!Directory.Exists(p)) return;`). When L1/L2/L7 flip the `[TruthPending]` attribute to claim a fix, those silent-return guards will re-introduce the green-wash bug (test passes by returning early, asserting nothing). Every `[TruthPending]` body must be switched to `Skip.IfNot(...)` BEFORE the attribute is removed.

**Gate:**
- `scripts/loom-guards.ps1` invocation includes truth gate (or `bench.ps1 -Truth` piping to guards)
- At minimum, the dogfood truth runs and asserts (not skipped)
- Any `[TruthPending]` body with a removed attribute uses `Skip.IfNot`, not `if(!exists) return;`
- Build 0w/0e, existing test counts hold

**Checkpoint:** `L0.4 — truth gate wired into automated battery; pending-test guards audited`

---

## L0.5 — Cold-QA B9 denominator fix + UI drive boot-liveness

**Session size:** small (~20 min)
**Files touched:**
- `eval/mcp-qa/run-cold.js` (classify.js scoring model)
- `eval/mcp-qa/classify.js`
- `eval/ui-audit-drive.mjs` (boot-liveness precondition)

**Background (from L0 audit, `.conductor/handovers/L0.md` §3):**
B9 ("resolve Order → aggregate #1") is a **success probe scored inside a failure denominator.** `find "Order"` legitimately returns results, yet it counts toward the 0/12 "actionable failures" ratio and can never be "actionable." At L5.5 the ≥90% gate math is awkward with B9 in the denominator. Separate rank-quality probes from failure-actionability probes before arming the ≥90% gate.

The UI drive gate with `--gate` passes a dead environment (server not up): `page.goto` throws, every assertion fails, all are `expectedRedUntil`, and the run is still GREEN. Add a boot-liveness precondition before arming `--gate`, or a dead environment will silently pass.

**Gate:**
- Cold-QA scoring: B9 and similar rank probes excluded from actionability denominator
- UI gate: `--gate` mode fails distinctly (not green) if browser never reaches the app shell
- Existing mcp-qa 8/8 and cold-QA baseline preserved

**Checkpoint:** `L0.5 — cold-QA denominator clean; UI drive gate liveness precheck`

---

## L1.6 — SymbolTable member/endpoint indexing + dead code removal

**Session size:** medium (~45 min)
**Files touched:**
- `src/DevContext.Core/Graph2/SymbolTable.cs` (add Member/Endpoint indexing)
- `src/DevContext.Core/Graph2/SymbolId.cs` (verify 8-variant enum populated)
- `src/DevContext.Core/Graph2/RefSite.cs` (delete `FromType` dead code)
- `tests/DevContext.Core.Tests/Graph2/SymbolTableTests.cs` (new indexing tests)

**Background (from L1 audit, `.conductor/handovers/L1.md` §4-5):**
The `SymbolKind` enum has 8 variants (`Service, Project, Type, Member, Endpoint, Message, Store, ConfigKey`) but only `Type` is populated. L2/L3 added member-level body-facts extraction but never indexed member/endpoint symbols into the table — so the resolution tier chain always returns `SymbolKind.Type`. This is a structural cap on resolution quality: every body-fact target is a Type node, not the actual method that contains it.

Additionally, `RefSite.FromType` is dead code: never called, hardcodes `Project = ""`, would produce wrong project-scoped resolution if ever used. **Remove** (don't fix — it has no callers). Deferred since L1.

**Gate:**
- SymbolTable indexes at minimum Member symbols from BodyFacts pipeline output
- `SymbolTableTests` cover member resolution with same-project and cross-project ambiguity cases
- `RefSite.FromType` deleted (verified: no callers via grep)
- Build 0w/0e, Core test count ≥ current + member indexing tests

**Checkpoint:** `L1.6 — SymbolTable indexes members; RefSite.FromType removed`

---

## L2.5 — Lambda scope pollution + SeamContext dedup

**Session size:** medium (~40 min)
**Files touched:**
- `src/DevContext.Core/Graph2/BodyFactExtractor.cs` (per-lambda scoping in WalkMember)
- `src/DevContext.Core/Graph/GraphBuilder.cs` (extract shared SeamContext builder)
- `tests/DevContext.Core.Tests/Graph2/BodyFactExtractorTests.cs` (multi-lambda test)
- `tests/DevContext.Core.Tests/Graph2/SeamDetectorTests.cs` (regression for shared context)

**Background (from L2 audit, `.conductor/handovers/L2.md` §4):**
`BodyFactExtractor.WalkMember` adds ALL lambda parameters (including nested lambdas) to a single member-level scope dictionary. In methods with multiple lambdas, a parameter `x` in one sub-expression could incorrectly resolve a receiver in a different sub-expression. Typical patterns (single-lambda handlers) are unaffected, but this is a latent correctness bug that blocks complex dispatch patterns.

`SeamContext` is built from scratch twice: `AddSeamsFromDetectors` scans all detections+types+graph nodes (~70 lines), then `AddLambdaSeams` does a similar scan (~16 lines). Both run serially. Not a performance problem today but a drift risk.

**Gate:**
- Test: multi-lambda method with same-name param in different lambdas produces correct receiver type for each
- Extracted `BuildSeamContext()` shared helper called from both `AddSeamsFromDetectors` and `AddLambdaSeams`
- Existing seam detector tests pass unchanged
- Build 0w/0e, Core test count ≥ current

**Checkpoint:** `L2.5 — per-lambda scoping; shared SeamContext builder`

---

## L3.4 — TfmScore + HasBindDemand profile + DntSite measurement

**Session size:** medium (~35 min)
**Files touched:**
- `src/DevContext.Core/Graph2/SemanticLitePopulator.cs` (TfmScore pattern-based, profile HasBindDemand)
- `tests/DevContext.Core.Tests/Graph2/SemanticLitePopulatorTests.cs` (TfmScore tests)
- `eval-results/` (run DntSite measurement if repo present)

**Background (from L3 audit, `.conductor/handovers/L3.md` §4):**
`TfmScore` only handles up to net9.0 explicitly — net10.0+ scores lowest (1), potentially choosing a less-compatible TFM. Should enumerate `net\d+` patterns generically. **This is especially relevant after the B0 net10 migration:** the codebase is now on net10.0 but the scoring function is blind to it.

`HasBindDemand` is now maximally permissive (after the audit fix) — every body with any InvocationOp passes the gate, potentially causing unnecessary binding attempts. Profile against dogfood to confirm cost is within budget (~3.9s), but add a benchmark trace.

DntSite controller sub-measurement (34/94 baseline on net8.0) was never recorded because the DntSite repo is absent on this machine. If the repo becomes available, run and record the baseline for the truth ratchet.

**Gate:**
- `TfmScore` handles `net\d+(\.\d+)?` pattern; net10.0 scores correctly
- Dogfood SemanticLite pass ≤ 4.0s (regression check)
- If DntSite present: run and record. If absent: document honestly (no silent skip-as-green).
- Build 0w/0e, tests pass

**Checkpoint:** `L3.4 — TfmScore handles net10+; HasBindDemand profiled; DntSite measured or documented absent`

---

## L3.5 — Eval test TodoApi + Call-spine gap (L7 prep)

**Session size:** small (~25 min)
**Files touched:**
- `tests/DevContext.Core.Tests/TruthExpectationTests.cs` (investigation + documentation)
- `eval-results/2026-07-08/` (record investigation outcome)

**Background (from L3/L4 audits, both handovers §5):**
`TodoApi_baseline_presence_ok` has been failing as a Category=Eval test since at least L3. The failure is: `TodoDbContext` is absent from the POST /todos trace — an L7 call-spine gap. The test was not fixed in L3 or L4 (out of scope). This session should:
1. Reproduce the failure and confirm root cause
2. Document exactly what L7 needs to fix (which symbol should appear, what trace step is missing)
3. Mark the test with `[TruthPending("L7")]` so it's an honest skip (not a green-wash)
4. Record the investigation in `eval-results/`

If root cause turns out to be external (TodoApi repo missing/different), mark as `Skip.IfNot` instead.

**Gate:**
- `TodoApi_baseline_presence_ok` is either: genuinely passing (fixed), skippable-with-reason, or catalogued as L7-gap
- Truth gate exits with 0 (all remaining failures honest-skips, not throws)
- Build 0w/0e

**Checkpoint:** `L3.5 — TodoApi eval gap triaged and honestly tracked`

---

## L4.5 — Flow model hardening (depth, budget, kind)

**Session size:** medium (~40 min)
**Files touched:**
- `src/DevContext.Core/Graph/FlowModel.cs` (depth warning, make configurable)
- `src/DevContext.Core/Graph/Projections/EntryTableProjection.cs` (kind propagation)
- `src/DevContext.Core/Graph/Projections/ContextPackBuilder.cs` (proportional budget)
- `src/DevContext.Server/GraphQueryService.cs` (GetContextPack integration test)
- `tests/DevContext.Core.Tests/Graph/FlowModelTests.cs`
- `tests/DevContext.Server.Tests/` (new integration test)

**Background (from L4 audit, `.conductor/handovers/L4.md` §4-5):**
1. Flow spine `maxSpineDepth=24` is hard-coded. Could silently truncate deep event choreography chains. Add a warning diagnostic when depth exhausted; consider making configurable.
2. `BuildMulti` splits budget evenly across entries — large entries starved, small overfunded. Consider proportional allocation.
3. `EntryTableProjection` stray entries all get `PublicApi` kind since flow nodes don't store `EntryPointKind`. Consider storing kind on entry-point GraphNodes during assembly.
4. `GetContextPack` RPC has **no dedicated integration test** — unproven end-to-end.

**Gate:**
- Flow spine warns (log event, not exception) when max depth reached
- Budget allocation at least proportional (entry complexity proxy: reach count)
- EntryTable kind: derive from node tags or explicit EntryPointKind stored on flow node
- Integration test: BuildMulti with multi-entry specs, verify section content + budget attribution
- Build 0w/0e, existing flow tests pass

**Checkpoint:** `L4.5 — Flow depth warning; proportional budget; entry kind provenance; integration test`

---

## L5.x — Unresolved from L0-L4 audit traps (sweep)

**Session size:** small (~20 min)
**Files touched:** varied

These are small audits that were flagged in handover "risks" sections but not tracked as formal checkpoints. One session to sweep them:

| Trap | Source | Action |
|---|---|---|
| `ServiceBoundaryInference` reads from disk per-call, silent `catch{}` | L1 §4 | Add caching or accept + document; verify no perf regression on dogfood |
| `ServiceBoundaryInference` never called more than once per solution today | L1 §5 risk #4 | If unchanged since L1, close as "still single-call, no action" |
| 13 advisory `NodeId.ForType(` in `Graph/` tracked by guards | L1 §5 risk #3 | Verify count hasn't drifted up; if L3 cleared them, update loom-guards threshold |
| `AmbiguityReport` is a class not record | L1 §4 | Convert to record or document why not |
| BuildInfo.g.cs re-dirties on every build | L4 audit trap | Already tracked, just verify `.gitignore` or conductor setup handles it |

**Gate:**
- Each trap is either: fixed, documented as still-low-risk, or escalated to a formal checkpoint
- Guards pass (0 banned, advisory count stable or decreasing)
- Build 0w/0e

**Checkpoint:** `L5.x — audit-trap sweep: all L0-L4 deferred traps triaged`
