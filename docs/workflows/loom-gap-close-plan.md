# Loom Gap Close — Plan Document

**Branch:** `feat/loom-l7`  |  **Baseline:** Loom L0-L8 DONE (34/34), Debt Cleanup DONE (8/8), Design Review DONE (R1+R2+R3), QA Driver DONE (s73)
**Sources:** QA-BUGFIXES.md, QA-FINAL-LOOM.md, HANDOVER-LOOM.md §7, R1/R2/R3 design reviews, .conductor/handovers/L*.md
**Anti-pattern reference:** meridian-agent-playbook.md §4 (A1-A15) — read before every phase

---

## Gap Inventory (13 gaps → 6 phases → 6 sessions)

| # | Gap | Origin | Severity | Phase |
|---|-----|--------|----------|-------|
| 1 | L2.4 checkout trace DEVIATES — bus-publish seams not walked | R1 L2.4 | P1 — engine bug | **A** |
| 2 | Tab strip height 28px (not >=30px) | QA-Driver s73 | P2 — UI | **B** |
| 3 | Code pane null on entry selection | QA-Driver s73 | P2 — UI | **B** |
| 4 | ContextPack server round-trip v0 (client assembles markdown) | HANDOVER-LOOM Trap A | P2 — polish | **D** |
| 5 | MCP page mcpRunning false on revisit | R2 L6.6 | P3 — UI | **C** |
| 6 | Inspector substring false positives ("Order" matches "OrderService") | R2 L6.3 | P3 — polish | **C** |
| 7 | bench.ps1 backtick-n encoding | QA-Driver s73 | P4 — script | **C** |
| 8 | L7.1 spine-depth metric missing | R3 L7.1 | P3 — metric | **C** |
| 9 | Perf budget doc drift (design says <=4s, reality ~6s) | R3 L8 | P4 — doc | **C** |
| 10 | LOOM-START.md baseline stale truth count (9P/2F vs 8P/3S) | R3 L8 | P0 — doc | **C** |
| 11 | Eval-1: 5 TraceQualityTests fail on eShop (non-CQRS) | HANDOVER-LOOM Eval-1 | P2 — eval | **E** |
| 12 | Eval-2: EvalExpectationTests verticalslice fails (1 test) | HANDOVER-LOOM Eval-2 | P2 — eval | **E** |
| 13 | PROGRESS-LOG.md gap (L5-L8 sessions missing) | R3 L8.1 | P4 — doc | **E** |

---

## Phase A — Engine Gap: L2.4 Checkout Trace Bus-Publish (1 session)

### Task: A1 — Fix Type->Service bridge, flip checkout truth test

**Source docs:** proposal-loom.md §L2.4 (headline artifact), R1-L0-L3.md:62-66 (enforcement attempt), QA-BUGFIXES.md:37-41

**History — why this is the one real bug:**

L2.4 is the flagship checkpoint: "POST /basket/checkout trace depth >= 5 through CLI." The test `Dogfood_checkout_flow_traces_cross_service_depth_ge_5` was written with the correct body (asserting BasketCheckoutEvent, BasketCheckoutEventHandler, CreateOrderCommand, depth >= 5) but marked `[TruthPending("L2")]` — skipped, never enforced.

**Attempt 1** (R1 design review, commit `7355577`): The R1 report initially said "just remove the attribute." The review *underestimated* the gap — it treated the attribute as an oversight, not a guardrail.

**Attempt 2** (R1 enforcement, commit `01c19af`): The R1 re-audit actually removed `[TruthPending("L2")]` and ran the truth gate. **TEST FAILED.** Error: `Assert.Contains() failure: "BasketCheckoutEvent" not found in trace`. The R1 report then correctly reclassified L2.4 as a **real implementation gap** — "Trace traversal does not walk bus-publish seams for this entry."

**Why both attempts failed:** Neither attempt traced through the actual `TraceBuilder.cs` walker code. They assumed the attribute was the blocker. It wasn't — the attribute was hiding a genuine missing bridge.

**Root cause (deep investigation of TraceBuilder.cs + GraphBuilder.cs + BusPublishDetector.cs):**

The pipeline works correctly through step 4 of the checkout flow:

1. `BusPublishDetector` detects `publishEndpoint.Publish(evt)` in `CheckoutBasketCommandHandler` → emits `Raises` seam to `BasketCheckoutEvent`
2. `GraphBuilder.cs:1722-1734` tracks event publishers: `_eventPublishers["BasketCheckoutEvent"] = {"Basket.API"}`
3. `GraphBuilder.cs:2090-2102` creates `ServiceLink(bus-publish)` edge: **Basket.API Service** → **Ordering.Application Service**
4. `TraceBuilder.cs:55-58` **includes** `EdgeKind.ServiceLink` in the Follow set — the trace WOULD follow ServiceLinks

**Where it breaks (step 5):**
- The `Raises` edge goes: `CheckoutBasketCommandHandler` (Type/Member) → `BasketCheckoutEvent` (Type, tagged IntegrationEvent)
- The `ServiceLink` edge is between two **Service** nodes (Basket.API → Ordering.Application)
- When the trace reaches `BasketCheckoutEvent` (Type node), `OutEdgesWithTwin()` (TraceBuilder.cs:412-438) yields:
  - The Type node's own out-edges (none that lead to a Service)
  - Bridge member edges (BasketCheckoutEvent has NO handler-entry members — it's an event, not a handler)
- **Result:** The trace **stops at BasketCheckoutEvent** — can never reach the Service node to follow the ServiceLink to Ordering.Application
- **Same problem exists in Flow spine:** `SelectBestSpineEdge()` (GraphBuilder.cs:204-237) has the identical issue

**Fix approach:**

In `OutEdgesWithTwin()` (TraceBuilder.cs:412):
```
// ADD after existing bridge member logic:
// Type->Service bridge: when at a Type node with known Project, yield ServiceLink edges
// from the containing Service node so trace can follow cross-service hops (L2.4 gap close)
if (id.Kind == NodeKind.Type)
{
    var node = _graph.Node(id);
    if (node?.Project is { Length: > 0 })
    {
        var serviceId = NodeId.ForService(node.Project);
        foreach (var e in _graph.OutEdges(serviceId, EdgeKind.ServiceLink))
            yield return e;
    }
}
```

In `SelectBestSpineEdge()` (GraphBuilder.cs:204): same bridge — after iterating the Type node's own edges, also check ServiceLink edges from the containing Service node.

In `TruthExpectationTests.cs:36`: remove `[TruthPending("L2")]`, verify the test body's assertions are still correct, add `[SkippableFact]` if needed.

**Verification steps:**

1. **Verify bug exists:** Remove `[TruthPending("L2")]` temporarily, run `dotnet test --filter "Category=Truth"` — confirm "BasketCheckoutEvent not found in trace" failure (repro the R1 finding)
2. **Apply fix:** Add Type->Service bridge in TraceBuilder.cs + GraphBuilder.cs
3. **Verify fix:** Run `dotnet test --filter "Category=Truth"` → `Dogfood_checkout_flow_traces_cross_service_depth_ge_5` passes
4. Full truth gate: `dotnet test DevContext.slnx --filter "Category=Truth"` → **9P/2S** (was 8P/3S — one skip activated)
5. Run dogfood CLI trace: `dotnet run --project src/DevContext.Cli -- report <dogfood-path> -o out.md` → TRACE section includes checkout flow with BasketCheckoutEvent + cross-service hop

**Files touched:**
- `src/DevContext.Core/Graph/TraceBuilder.cs` — `OutEdgesWithTwin()` (+~10 lines)
- `src/DevContext.Core/Graph/GraphBuilder.cs` — `SelectBestSpineEdge()` (+~10 lines)
- `tests/DevContext.Core.Tests/TruthExpectationTests.cs` — remove `[TruthPending("L2")]`

**Gate:** `dotnet test --filter "Category=Truth"` → 9P/2S (checkout test now passing)

**Evidence:** `eval-results/current-date/phase-A-truth.txt`

**Anti-patterns to avoid:**
- **A13 (fixture-shaped fixture):** Test on dogfood repo, not a hand-built fixture. The existing test already asserts against real checkout flow steps.
- **A15 (catch-and-continue):** The bridge code must not silently swallow failures. If `NodeId.ForService()` returns a node that doesn't exist, the `foreach` over out-edges is a no-op — safe by construction.

---

## Phase B — UI Regressions (1 session)

### Task B1 — Tab Strip Height 28px -> >=30px

**Source docs:** QA-FINAL-LOOM.md:118, QA-BUGFIXES.md:44-48, ui-gate.json:11 (stripH=28), ui/notes.md:3

**History:**
- L6.1 delivered tab strip with `h-8` tailwind class (32px) — claimed as DONE
- QA Driver s73 measured actual height at **28px** — below 30px target
- `h-8 = 32px` in Tailwind, so the effective height is being compressed
- Likely cause: inner padding on child `.tab` elements, or font-size/line-height reducing visible height
- R3 re-confirmed this is a real gap (CONFORMS-WITH-FINDINGS rating for L6.1)
- 0 fix attempts — found by QA, never touched

**Verify bug exists:** Run `node scripts/ui-audit-drive.mjs --gate` → assertion A-tabstrip-height shows RED with stripH=28px

**Fix approach:**
1. Inspect `tab-strip.ts` template for the actual HTML structure — find the container div that gets `h-8`
2. Check for inner padding on child elements, line-height on text, or flex alignment that compresses the box
3. Ensure outer container has `min-height: 32px` (or `30px` minimum — 32px is the target) regardless of child content
4. May need to add explicit `min-height: 2rem` (32px) or `box-sizing: border-box` to the tab strip element
5. If the issue is font-size (11.375px per ui/notes.md), ensure the line-height doesn't compress: add `leading-none` or explicit line-height

**Verification:** Run `node scripts/ui-audit-drive.mjs --gate` → assertion A flips GREEN (stripH >= 30px)

**Files touched:**
- `src/DevContext.App/src/app/components/tab-strip/tab-strip.ts` (template)
- `src/DevContext.App/src/styles.css` (if global styles affected)

### Task B2 — Code Pane Null on Entry Selection

**Source docs:** QA-FINAL-LOOM.md:119, QA-BUGFIXES.md:50-54, ui-gate.json:28 (code length=null), ui/notes.md:6

**History:**
- L6.2 delivered code pane with `read_source` RPC + PrismJS — claimed as DONE
- QA Driver s73 assertion C fails: code pane content length is **null**
- The audit script selects an entry node, expects the Inspector Code tab to show source code
- L6 handover says code pane has loading skeleton, error state, and "No source file path" empty state — so null means one of these paths is being hit
- The `read_source` RPC may not support the specific node kind the audit script selects
- 0 fix attempts — found by QA

**Verify bug exists:** Run `node scripts/ui-audit-drive.mjs --gate` → assertion C shows RED with code length=null. Also manually test: open dogfood in UI, select POST /basket/checkout entry in Explore, check Code tab.

**Fix approach:**
1. Identify which node the audit-drive script selects (read `ui-audit-drive.mjs` — it likely selects the first entry from `listEntries`)
2. Check `read_source` RPC handler in `DevContextGrpcService.cs` — verify it returns source for that node kind
3. If the node is an EntryPoint kind without file source: the RPC should return the entry's file:line from `EntryPoint.Provenance`
4. If the RPC returns correctly but the Inspector doesn't show it: fix `inspector.ts` `loadCode()` to handle the response shape
5. The fix should be on the **server side** (ensure RPC returns source spans for all node kinds that have them) AND possibly **client side** (handle edge cases gracefully with a useful empty state)

**Verification:** 
- Manual: select a known node (e.g., Order class or POST /basket/checkout) → Code tab shows C# source with highlighting
- Gate: `node scripts/ui-audit-drive.mjs --gate` → assertion C flips GREEN (code length > 0)

**Files touched:**
- `src/DevContext.Server/Endpoints/DevContextGrpcService.cs` (read_source handler)
- `src/DevContext.App/src/app/components/inspector/inspector.ts` (loadCode path)
- `src/DevContext.App/scripts/ui-audit-drive.mjs` (possibly — update node selection if needed)

---

## Phase C — Polish Batch (1 session)

### Task C1 — MCP Page mcpRunning False on Revisit

**Source docs:** R2-L4-L6.md:64 (Finding L6.6), QA-BUGFIXES.md:65-69, L6 handover:50-52

**History:**
- `mcpRunning` signal initialized to `false` in `mcp-page.ts`
- On component mount, actual MCP server state is never queried
- If server was already running MCP (prior session, manual start), page shows "Stopped" incorrectly
- L6 handover documents this as "thin/shortcut/assumed" — honest, just not fixed
- 0 fix attempts

**Verify bug exists:** Start DevContext server manually (it will auto-start MCP if configured), open MCP page in browser, verify it shows "Stopped" while MCP is actually running.

**Fix approach:**
1. Add a `getMcpStatus()` method to the MCP client service (or the existing RPC layer)
2. Call it in `ngOnInit()` before initializing the `mcpRunning` signal
3. Option A (cleanest): Add new RPC to proto, implement in server, call from UI
4. Option B (quicker): Ping the existing `listSessions`/`getSession` endpoint and base on whether it responds
5. Set `mcpRunning` signal based on actual server state

**Verification:** Start server -> visit MCP page -> shows "Running" -> navigate to Home -> return to MCP page -> still shows "Running"

**Files touched:**
- `src/DevContext.App/src/app/pages/mcp-page/mcp-page.ts` (init)
- `src/DevContext.App/src/app/state/mcp.store.ts` (status check)
- Possibly: proto file + `DevContextGrpcService.cs` for new RPC

### Task C2 — Inspector Insights Substring False Positives

**Source docs:** R2-L4-L6.md:55 (L6.3 matching), QA-BUGFIXES.md:72-76, L6 handover:49

**History:**
- `inspector.ts:354-357` uses `.includes(title)` for insight matching — substring fallback
- "Order" matches "OrderService", "CustomerOrder", "OrderItem" — false positives on short node names
- L6 handover correctly documents this as "best-effort, not exact" with honest empty state
- R2 finding L6.3: ~15 min fix — very small scope
- 0 fix attempts

**Verify bug exists:** Select "Order" node in Explore -> Inspector Insights tab -> check if "OrderService" appears in Order's insight list. It shouldn't (they're different nodes).

**Fix approach:**
1. Change the loose `evidence.some(e => e.toLowerCase().includes(title))` fallback to word-boundary matching
2. Use the same `IsWordBoundary()` logic from `GraphQuery.cs:546-566` — keeps consistency with engine
3. Or: match against resolved `NodeId.Key` equality first, then word-boundary, then substring as last resort with a lower weight
4. The chip should still show honest counts — only filtered insights contribute to the "filtered N / total M" display

**Verification:** Select "Order" node -> insights show only Order-specific items, no "OrderService" false matches

**Files touched:**
- `src/DevContext.App/src/app/components/inspector/inspector.ts` (~line 354)

### Task C3 — bench.ps1 Encoding Fix

**Source docs:** QA-BUGFIXES.md:86-90

**History:**
- `scripts/bench.ps1:263` has a backtick-n escaped newline character
- PowerShell 5.1 parses this incorrectly (UTF-8 encoding issue)
- QA session s73 temporarily fixed it by rewriting with UTF-8 no-BOM
- Needs a permanent fix that works on any encoding
- 0 permanent fix attempts

**Fix approach:**
- Replace the backtick-n escaped newline with `[Environment]::NewLine` in the `Write-Host` call
- This works regardless of file encoding

**Verification:** `powershell -File scripts/bench.ps1` exits clean

**Files touched:**
- `scripts/bench.ps1` (line 263)

### Task C4 — L7.1 Spine-Depth Metric

**Source docs:** R3-L7-L8.md:25-26 (Finding L7.1), QA-BUGFIXES.md:79-83, proposal-loom.md §L7.1

**History:**
- L7.1 design spec: "entries with >=2-deep spine >=70% on non-CQRS repos"
- `PlainCallDetector` produces the edges — plumbing works fine
- No aggregation of spine depth per entry exists
- `FlowModel.IsTruncated` tracks depth exhaustion but no counter
- R3 rated L7.1 as CONFORMS-WITH-FINDINGS — metric missing
- 0 fix attempts

**Verify bug exists:** Run CLI on dogfood, check Stats section. No "entries with deep spine" line exists.

**Fix approach:**
1. In `GraphStats.Compute()`: iterate `graph.Flows`, count entries where `flow.Steps.Length >= 2` (call-spine depth >= 2)
2. Compute percentage: `deepCount / totalEntries`
3. Add to `GraphStats` result type: `EntriesWithDeepSpine` (int) and `DeepSpineRatio` (double 0-1)
4. Surface in CLI report (renderer) and UI Stats page
5. Verify claim: on RazorPages/CleanArchitecture repos, >=70% of entries have depth >= 2

**Verification:** Run on dogfood -> Stats line shows "N entries with deep call-spine (X%)". Run on non-CQRS test repos to verify 70% threshold.

**Files touched:**
- `src/DevContext.Core/Graph/GraphStats.cs` (add counter)
- `src/DevContext.Core/Rendering/*ReportRenderer*.cs` (expose in CLI output)
- Possibly: UI Stats component

### Task C5 — Perf Budget Doc + LOOM-START Baseline Fix

**Source docs:** R3-L7-L8.md:66 (perf), QA-BUGFIXES.md:93-96 (budget), LOOM-START.md:85 (stale truth count)

**History:**
- Design doc §8 says "Dogfood <= 4s" — this was the aspirational Meridian baseline from 2026-07-07
- Loom added Flows, ArchetypeDetector, 6 seam detectors, SemanticLite, PlainCallDetector
- Post-Loom reality: ~5.6s on dogfood
- LOOM-START.md line 85 baseline table says "9P/2F/0S" — actual truth gate is "8P/3S" (0 failures, 3 skips)
- Both docs — 5-10 min fix each

**Fix approach:**
1. `loom-graph-design.md` §8: Update "Dogfood <= 4s" to "Dogfood <= 6s (Tier A only <= 4s)" — reflect post-Loom feature surface
2. `LOOM-START.md:85`: Change "9P/2F/0S" to "8P/3S (3 [TruthPending] ratchets: L1 server names, L2 checkout flow, L1 RazorPages; DntSite passes)"

**Verification:** Files read back consistent. Truth test count matches actual gate output.

**Files touched:**
- `docs/dev/briefs/loom-graph-design.md`
- `LOOM-START.md`

---

## Phase D — ContextPack Server Round-Trip (1 session)

### Task D1 — Server Assembled Markdown

**Source docs:** HANDOVER-LOOM.md:184 (Trap A), R2-L4-L6.md:25 (CONFORMS-WITH-FINDINGS), QA-BUGFIXES.md:57-62, proposal-loom.md §L4.4:218-219

**History:**
- L4.4 design target: "Copy/Save = exactly the server pack"
- `ContextPackBuilder.BuildMulti()` was delivered in L4.4 (commit 9fe1d17) — server returns structured cards
- PROGRESS-LOG.md s21 claims "1 RPC instead of N, per-card type filtering, server-assembled markdown with budget trimming" — but the **final markdown assembly** still happens in the client
- The server returns `ContextCardPack` objects; the client (`composition-view.ts`) concatenates them into markdown
- HANDOVER-LOOM.md §7.1 Trap A acknowledges this as v0 limitation
- The gap is the LAST mile: the server should produce the final formatted markdown string, not just structured cards
- 0 fix attempts — documented as "known v0 deferral"

**Verify bug exists:** Inspect `composition-view.ts` — confirm it does client-side markdown concatenation. Check `ContextPackBuilder.cs` — confirm it returns structured objects, not markdown string.

**Fix approach:**
1. Add a `SerializeToMarkdown()` method (or similar) to `ContextPackBuilder` that takes a `MultiContextPack` and returns the final markdown string
2. Update the `getContext` RPC handler to return the server-assembled markdown string (add a field to the proto response, or replace the structured response)
3. Update `composition-view.ts` to use the server markdown instead of client concatenation
4. Maintain backward compatibility — existing UI should work with both old (client-assembled) and new (server-assembled) paths
5. Token meter must use server-side token counts (it may already do this via `allocatedTokens` on the response)

**Verification:**
- Copy from Context Studio -> paste into editor -> verify markdown matches expected format
- Verify token meter shows server-computed totals
- Client-side string concatenation code in `composition-view.ts` is removed or unused

**Files touched:**
- `src/DevContext.Core/Graph/ContextPackBuilder.cs` (add markdown serialization)
- `src/DevContext.Server/Endpoints/DevContextGrpcService.cs` (return assembled markdown)
- `src/DevContext.App/src/app/pages/context-studio/composition-view.ts` (use server output)
- Proto file (possibly add markdown string to response)

---

## Phase E — Eval Gap Investigation + Docs (1 session)

### Task E1 — eShop TraceQuality Tests Investigation

**Source docs:** HANDOVER-LOOM.md:182 (Eval-1), L8 handover:36

**History:**
- 5 TraceQualityTests fail on `eval-repos/eShop` (a non-CQRS eShop implementation)
- eShop uses Carter HTTP framework + MediatR proxy pattern, NOT a real CQRS stack
- Failing tests: missing 'send' in POST /api/orders/ trace, missing CreateOrderCommand, missing ProductPriceChangedIntegrationEvent, missing OrderStartedDomainEvent, Sends seams = 0
- These are call-spine coverage gaps on non-CQRS patterns
- 0 investigation attempts — never run to see the actual failures
- Handled via `--filter "Category!=Eval"` in gate battery — excluded from daily gates

**Verify bug exists:** Run `dotnet test --filter "Category=Eval"` — confirm 5 TraceQualityTests fail on eShop. Inspect the actual trace output for eShop orders endpoint.

**Action (investigation, not fix):**
1. Run eShop analysis: `dotnet run --project src/DevContext.Cli -- report <eshop-repo-path> -o eshop-out.md`
2. Inspect the trace output for POST /api/orders/ — what DOES the trace contain?
3. Compare trace shape against expected assertions:
   - If the trace genuinely lacks cross-service hops because of engine limits: document as known limitation
   - If test expectations are wrong for the Carter proxy pattern: update expectations to match reality
4. Apply fix based on investigation:
   - If trivial engine fix: fix it (small scope only — don't derail into L7 refactoring)
   - If structural (non-CQRS call-spine gap): mark with `[Skip.IfNot]` + reason, document in HANDOVER-LOOM.md
5. Outcome: either 5 more passing Eval tests, or 5 honestly skipped with documentation

**Verification:** `dotnet test --filter "Category=Eval"` — eShop tests either pass or skip with documented reason. No silent failures.

**Files touched:**
- `tests/DevContext.Core.Tests/TraceQualityTests.cs` (skip/fix assertions)
- `docs/dev/HANDOVER-LOOM.md` (update §7.1 with investigation outcome)

### Task E2 — EvalExpectationTests Verticalslice

**Source docs:** HANDOVER-LOOM.md:183 (Eval-2), L8 handover:37

**History:**
- 1 test (verticalslice) fails with 5 assertion failures:
  - arch-style expected "VerticalSlices", got "Unknown"
  - mediatr-signal not found
  - endpoint-count = 0
  - detection-endpoints = 0
  - detection-ef = 0
- The VerticalSlice eval repo may have changed, or expectations were written against a different repo state
- 0 investigation attempts

**Verify bug exists:** Run `dotnet test --filter "Category=Eval"` — confirm verticalslice test fails. Inspect the actual analysis output.

**Action (investigation, not fix):**
1. Clone/verify VerticalSlice repo exists and is at expected HEAD
2. Run analysis on it: `dotnet run --project src/DevContext.Cli -- report <verticalslice-path> -o vs-out.md`
3. Compare output against existing expectations in `TruthExpectationTests.cs`
4. If repo changed: regenerate expectations from fresh run, update test assertions
5. If expectations were always wrong: fix to match actual analysis output
6. If the test is fundamentally invalid (wrong repo, wrong expectations): mark as `Skip.IfNot` with documented reason
7. Outcome: test either passes or honestly skips

**Verification:** `dotnet test --filter "Category=Eval"` — verticalslice test passes or skips with documented reason.

**Files touched:**
- `tests/DevContext.Core.Tests/TruthExpectationTests.cs` (verticalslice portion)
- `docs/dev/HANDOVER-LOOM.md` (update §7.1 with investigation outcome)

### Task E3 — PROGRESS-LOG.md Backfill

**Source docs:** R3-L7-L8.md:41

**History:**
- `docs/dev/go-to-program/PROGRESS-LOG.md` entries stop at L4.4 (s21), then jump directly to post-Loom debt sessions (s53+)
- L5, L6, L7, L8 core delivery sessions are ONLY documented in `.conductor/handovers/L*.md`
- L7 sessions (s34-s39) have NO conductor handover — information exists in LOOM-START.md checkpoint table + gate battery evidence files
- R3 finding flagged this as "low risk" data gap

**Fix approach:**
1. Extract session summaries from `.conductor/handovers/L5.md`, `L6.md`, `L8.md`
2. For L7 sessions: derive from LOOM-START.md checkpoint table (L7.1-L7.4) + gate battery files under `eval-results/2026-07-08/`
3. Write entries into PROGRESS-LOG.md with consistent format: date, session #, checkpoint, what was delivered, evidence path, next step
4. Place them between the existing L4.4 entry (s21) and the debt entries (s53+)

**Verification:** PROGRESS-LOG.md has continuous entries from L0 through L8 close-out. No jumps or missing stages.

**Files touched:**
- `docs/dev/go-to-program/PROGRESS-LOG.md`

---

## Phase F — Final QA Close-out (1 session)

### Task F1/F2 — Full Gate Battery + Docs Update

**Source docs:** loom-debt-workflow.md §3, QA-FINAL-LOOM.md (existing baseline)

**Action:**
1. Run FULL gate battery:
   - `dotnet build DevContext.slnx -clp:ErrorsOnly` -> 0w/0e
   - `dotnet test DevContext.slnx --filter "Category!=Eval"` -> all pass
   - `dotnet test DevContext.slnx --filter "Category=Truth"` -> **9P/2S** (was 8P/3S)
   - `pnpm check` (src/DevContext.App) -> lint 0, tests 27/27, build 0w/0e
   - `node scripts/ui-audit-drive.mjs --gate` -> **assertions A + C flip GREEN** (was RED)
   - `node eval/mcp-qa/run.js` -> 8/8
   - `node eval/mcp-qa/run-cold.js --gate` -> >=90%
   - `powershell -File scripts/bench.ps1` -> 22/22 OK
   - `powershell -File scripts/loom-guards.ps1` -> 0 banned

2. Update `HANDOVER-LOOM.md` §7:
   - Move resolved gaps from "Known Gaps" to a new "§7.3 Post-Loom Gap Close (2026-07-09)" section
   - Mark Trap A (ContextPack) as RESOLVED if Phase D completed
   - Mark Eval-1 (eShop) as RESOLVED or KNOWN-LIMITATION per Phase E outcome
   - Mark Eval-2 (verticalslice) as RESOLVED or KNOWN-LIMITATION per Phase E outcome
   - Update truth test count in §9

3. Update `LOOM-START.md`:
   - Overwrite handoff block with final state: all phases DONE, gates green, evidence paths
   - Mark all checkpoint rows as DONE or VERIFIED
   - Update baseline numbers if any changed

4. Produce evidence:
   - `eval-results/current-date/qa-close/gate-battery.txt` (full gate output)
   - `eval-results/current-date/qa-close/truth-gate.txt`
   - `eval-results/current-date/qa-close/ui-gate.json`
   - `eval-results/current-date/qa-close/mcp-qa.txt`, `mcp-cold-qa.txt`
   - `eval-results/current-date/qa-close/bench.txt`

5. Commit: `chore(loom): gap-close QA — Phase F final verification, all 6 phases done`

**Gate:** ALL gates green. HANDOVER-LOOM.md updated. LOOM-START.md handoff updated. Evidence committed.

---

## Anti-Pattern Checklist (per meridian-agent-playbook.md §4)

The agent MUST check these before marking any checkpoint DONE:

| AP | Description | How to avoid |
|----|-------------|-------------|
| **A1** | Dead-parameter fix | Spine metric (C4): must appear in CLI report output and be content-asserted |
| **A3** | Stub artifact | Every evidence file must be a FRESH RUN with real output. No placeholder files. |
| **A4** | Gate skipped, claimed run | Run the gate BEFORE marking DONE. Gate output is the evidence. |
| **A6** | Ship-without-launch | Every fix must be verified manually at least once. L2.4: run checkout trace. B2: open Code tab. |
| **A10** | Dead-end navigation | Verify MCP page status check actually queries the server. Verify Inspector linking works. |
| **A13** | Fixture-shaped fixture | L2.4 fix MUST be verified on dogfood repo. The test already uses dogfood — don't add a fixture-only test. |
| **A14** | TODO-as-delivery | No `// TODO` comments in changed files. If something is incomplete, the checkpoint stays IN PROGRESS. |
| **A15** | Catch-and-continue | TraceBuilder bridge code: no silent `catch{}`. If node.Project is null, the foreach is a no-op — safe by design. |

---

## Session Ritual (per loom-debt-workflow.md §2)

### Pre-session (every session):
1. Read `LOOM-START.md` handoff block
2. Read the relevant phase section in THIS document
3. Run selective gate:
   - Engine change -> `dotnet build` + `dotnet test --filter Category!=Eval`
   - UI change -> `pnpm check`
   - Config/docs -> skip
4. State in tracker what artifact proves this session done

### Post-session (every session):
1. Re-run selective gate
2. Produce evidence artifact under `eval-results/<date>/phase-<letter>-<desc>.txt`
3. Overwrite `LOOM-START.md` handoff block (<=12 lines)
4. Update checkpoint status in LOOM-START.md table
5. Commit (`fix(loom): <phase-letter> — <one-line what>`). Push.

### Gate policy:
- **perPhase**: fast-tier gates (build, tests, truth, pnpm) run after each session. Full-tier gates (guards, bench, mcp-qa, cold-qa, ui-gate) run only when a phase completes (all its checkpoints DONE).

---

*End of plan — 6 phases, 6 sessions, 14 checkpoints. Read this document before every session. The anti-pattern checklist is non-negotiable.*
